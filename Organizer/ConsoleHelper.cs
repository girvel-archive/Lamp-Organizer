using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Organizer.TaskSystem;
using static System.Console;
using static Organizer.Program;
using static Organizer.ConsoleDecorator;

namespace Organizer {
    public static class ConsoleHelper {
        public enum ConsoleMode {
            Normal,
            Correct,
        }

        public static bool Active { get; set; }
        public static bool Saved { get; set; } = true;

        public static ConsoleMode Mode { get; set; } = ConsoleMode.Normal;

        public static int CursorPosition { get; set; } = -1;

        public static int LineParts = 5;
        public static bool[,] Line = null;

        public static Dictionary<ConsoleMode, Dictionary<ConsoleKey, Action>> KeyActions { get; set; }
        public static Dictionary<ConsoleMode, Dictionary<ConsoleKey, string>> KeyDescriptions { get; set; }

        public static Dictionary<Type, Dictionary<ConsoleKey, Action<IConsoleWritable>>> KeyCorrectingActions { get; set; }

        static ConsoleHelper() {
            KeyActions = new Dictionary<ConsoleMode, Dictionary<ConsoleKey, Action>> {
                [ConsoleMode.Normal] = new Dictionary<ConsoleKey, Action> {
                    [ConsoleKey.T] = NewTask,
                    [ConsoleKey.R] = () => NewTarget(GetString("tag: #")),
                    [ConsoleKey.E] = EndTask,
                    [ConsoleKey.C] = () => Mode = ConsoleMode.Correct,
                    [ConsoleKey.D] = () => MainSession.Description = GetString("new description:\n"),
                    [ConsoleKey.U] = BuildStatistics,
                    [ConsoleKey.S] = Save,
                    [ConsoleKey.O] = Open,
                    [ConsoleKey.H] = ShowHelp,
                    [ConsoleKey.F12] = ShowInfo,
                },
                [ConsoleMode.Correct] = new Dictionary<ConsoleKey, Action> {
                    [ConsoleKey.UpArrow] = () => CursorPosition = Math.Max(-1, CursorPosition - 1),
                    [ConsoleKey.DownArrow] = () => 
                        CursorPosition = Math.Min(
                            MainSession.Targets.Count + MainSession.Tasks.Count - 1,
                            CursorPosition + 1),
                    [ConsoleKey.Escape] = () => Mode = ConsoleMode.Normal,
                    [ConsoleKey.Enter] = ChooseCorrectingActions,
                    [ConsoleKey.S] = Save
                },
            };

            KeyDescriptions = new Dictionary<ConsoleMode, Dictionary<ConsoleKey, string>> {
                [ConsoleMode.Normal] = new Dictionary<ConsoleKey, string> {
                    [ConsoleKey.T] = "creates new Task",
                    [ConsoleKey.R] = "creates new taRget",
                    [ConsoleKey.E] = "Ends last task",
                    [ConsoleKey.C] = "Corrects elements",
                    [ConsoleKey.D] = "changes Description",
                    [ConsoleKey.U] = "bUilds statistics",
                    [ConsoleKey.S] = "Saves current session",
                    [ConsoleKey.O] = "Opens organizer file",
                    [ConsoleKey.H] = "views Help",
                    [ConsoleKey.F12] = "views application description",
                },
                [ConsoleMode.Correct] = new Dictionary<ConsoleKey, string> {
                    [ConsoleKey.UpArrow] = "Ups the cursor",
                    [ConsoleKey.DownArrow] = "Downs the cursor",
                    [ConsoleKey.Escape] = "changes the mode to normal",
                    [ConsoleKey.Enter] = "chooses current element to edit",
                    [ConsoleKey.S] = "saves current session",
                },
            };

            KeyCorrectingActions = new Dictionary<Type, Dictionary<ConsoleKey, Action<IConsoleWritable>>> {
                [typeof(Task)] = new Dictionary<ConsoleKey, Action<IConsoleWritable>> {
                    [ConsoleKey.B] = w => SetTaskBeginTime((Task)w),
                    [ConsoleKey.E] = w => SetTaskEndTime((Task)w),
                    [ConsoleKey.T] = w => SetTaskParentTarget((Task)w),
                    [ConsoleKey.F] = w => SetTaskEfficiency((Task)w),
                    [ConsoleKey.N] = w => ((Task) w).Name = GetString("name: "),
                    [ConsoleKey.D] = w => TaskDelete((Task)w),
                    [ConsoleKey.Delete] = w => KeyCorrectingActions[typeof(Task)][ConsoleKey.D](w),
                },

                [typeof(Target)] = new Dictionary<ConsoleKey, Action<IConsoleWritable>> {
                    [ConsoleKey.U] = w => SetTargetUseful((Target)w),
                    [ConsoleKey.N] = w => ((Target)w).Name = GetString("name: "),
                    [ConsoleKey.T] = w => ((Target)w).Tag = GetString("tag: #"),
                    [ConsoleKey.C] = w => SetTargetCondition((Target)w),
                    [ConsoleKey.D] = w => TargetDelete((Target)w),
                    [ConsoleKey.Delete] = w => KeyCorrectingActions[typeof(Target)][ConsoleKey.D](w),
                },
            };
        }

        public static void Refresh() {
            Clear();
            WriteLine(MainSession.Date.ToString("dd.MM.yyyy"));
            ColorWrite(Saved ? "Saved\n\n" : "***\n\n", ConsoleColor.DarkGray);
            
            ColorWrite(MainSession.Description + "\n\n", ConsoleColor.DarkGray);

            var i = 0;
            Action<IConsoleWritable> writeDel = t => {
                if (Mode == ConsoleMode.Correct && i == CursorPosition)
                    ForegroundColor = ConsoleColor.White;

                WriteLine(t.ConsoleInfo);

                if (Mode == ConsoleMode.Correct && i == CursorPosition)
                    ResetColor();

                i++;
            };
            
            ColorWrite("## Targets:\n\n", ConsoleColor.White, ConsoleColor.DarkGray);
            ResetColor();

            MainSession.Targets.ForEach(writeDel);

            ColorWrite("\n\n## Tasks:\n\n", ConsoleColor.White, ConsoleColor.DarkGray);
            MainSession.Tasks.ForEach(writeDel);
            
            if (Line != null)
                for (var x = 0; x < Line.GetLength(0); x++) {
                    for (var y = 0; y < Line.GetLength(1); y++) {
                        ColorWrite(" ", ForegroundColor, Line[x, y] ? ConsoleColor.DarkGray : ConsoleColor.Black);
                    }
                    WriteLine();
                }

            ColorWrite("\nUse [H] to get help and [F12] to get some useful information\n", ConsoleColor.DarkGray);
        }

        public static void ControlLoop() {
            Refresh();

            Active = true;
            while (Active) {
                try {
                    KeyActions[Mode][ReadKey(true).Key]();
                }
                catch (KeyNotFoundException) { }
                Refresh();
            }
        }

        // ----- KEY ACTIONS: -----

        public static void Open() {
            do {
                if (!Saved) {
                    if (GetAgreement("You have unsaved changes. Save them?"))
                        Save();
                }
                ColorWrite("Print '?' to save file in saves/<current date>\n", ConsoleColor.DarkGray);
                CurrentFilePath = GetPath();

                FileStream file = null;
                try {
                    if (CatchFileExceptions(
                            () => file = File.Open(CurrentFilePath, FileMode.OpenOrCreate),
                            CurrentFilePath))
                        continue;
                    MainSession = Session.Open(file);
                }
                catch (SerializationException) {
                    MainSession = new Session();
                }
                finally {
                    file?.Close();
                }

            } while (MainSession == null);
            Saved = true;
        }

        private static void Save() {
            if (GetAgreement("Create new file?"))
                CurrentFilePath = GetPath();

            FileStream file = null;

            if (CatchFileExceptions(
                    () => file = File.Open(CurrentFilePath, FileMode.Create),
                    CurrentFilePath))
                return;

            MainSession.Save(file);
            file.Close();

            Saved = true;
        }

        private static void NewTask() {
            var name = GetString("name: ");

            Target target;
            bool targetLoop;
            do {
                targetLoop = false;

                var targetTag = GetString("tag: #");
                target = MainSession.Targets.Find(t => t.Tag.Contains(targetTag ?? string.Empty));

                if (target != null) continue;

                if (GetAgreement("This target does not exist. Create new one?")) {
                    target = NewTarget(targetTag);
                }
                else {
                    targetLoop = true;
                }
            } while (targetLoop);

            MainSession.Tasks.Add(new Task(name, target, TaskEfficiency.NotDefined, DateTime.Now, DateTime.Now));
            Saved = false;
        }

        private static void EndTask() {
            MainSession.Tasks.Last().EndTime = DateTime.Now;
            try {
                ForegroundColor = ConsoleColor.DarkGray;
                WriteLine("Task may be ended many times");
                ResetColor();
                MainSession.Tasks.Last().Efficiency = TaskEfficiency.Parse(GetString("efficiency: #").ToCharArray()[0]);
            }
            catch (IndexOutOfRangeException) {
                Error("this efficiency does not exist");
                AnyKey("to continue");
            }
            catch (FormatException) {
                Error("this efficiency does not exist");
                AnyKey("to continue");
            }
            Saved = false;
        }

        private static void ChooseCorrectingActions() {
            IConsoleWritable subject;

            if (CursorPosition < MainSession.Targets.Count)
                subject = MainSession.Targets[CursorPosition];
            else if (CursorPosition < MainSession.Targets.Count + MainSession.Tasks.Count)
                subject = MainSession.Tasks[CursorPosition - MainSession.Targets.Count];
            else
                return;

            WriteLine($"\nsubject: {subject.ConsoleInfo}");

            try {
                var message = 
                    subject is Task
                    ? "\nedit: [Begin time / End time / Target's tag / eFficiency / Name / Delete]" 
                    : "\nedit: [Name / Tag / Condition / Useful / Delete]"; // TODO to dictionary

                KeyCorrectingActions[subject.GetType()][GetConsoleKeyInfo(message).Key](subject);
            }
            catch (KeyNotFoundException) {
                Error("wrong key");
            }
        }

        private static void ShowHelp() {
            Separator();
            foreach (var keyDescriptionsDictionary in KeyDescriptions) {
                WriteLine($"Mode: {keyDescriptionsDictionary.Key}");
                foreach (var action in keyDescriptionsDictionary.Value) {
                    WriteLine($"\t[{action.Key}]\t - {action.Value}");
                }
                WriteLine();
            }
            AnyKey("to hide keys list");
        }

        private static void ShowInfo() {
            Separator();
            WriteLine($"Lamp organizer, version {MainSession.Version}");

            var infoStream = File.OpenText(@"info.txt");
            WriteLine("\n" + infoStream.ReadToEnd());
            infoStream.Close();

            AnyKey("to hide description");
        }

        private static void BuildStatistics() { // TODO optimization
            var targetsParts = new Dictionary<Target, double>();
            
            var beginTime = DateTime.MaxValue;
            var endTime = DateTime.MinValue;
            var totalTime = 0.0;

            MainSession.Tasks.ForEach(t => {
                if (beginTime > t.BeginTime)
                    beginTime = t.BeginTime;

                if (endTime < t.EndTime)
                    endTime = t.EndTime;

                var duration = (t.EndTime - t.BeginTime).TotalSeconds * Math.Sqrt((double)t.Efficiency.Value / TaskEfficiency.MaxValue);

                totalTime += duration;
                try {
                    targetsParts[t.ParentTarget] += duration;
                }
                catch (KeyNotFoundException) {
                    targetsParts[t.ParentTarget] = duration;
                }
            });

            double allTime;
            if (GetAgreement("\nUse custom day duration? (alt: first action begin time - last action end time)"))
                allTime = Convert.ToInt32(GetString("Day duration in minutes: ")) * 60;
            else
                allTime = (endTime - beginTime).TotalSeconds;

            var targetPartsSorted = targetsParts.ToList();
            targetPartsSorted.Sort((p1, p2) => (int) (p2.Value - p1.Value));

            WriteLine();

            targetPartsSorted.ForEach(
                t => WriteLine($"{(int)t.Value / 60}m" + $"\t{(int)(t.Value / allTime * 100)}%\t{t.Key.Name}"));

            var lostTime = allTime - totalTime;
            WriteLine($"{(int)lostTime / 60}m" + $"\t{(int)(lostTime / allTime * 100)}%\tlost");

            AnyKey();
        }

        // ----- CORRECTING ACTIONS: -----

        private static void SetTaskEndTime(Task subject) {
            try {
                var time = GetString("time: ").Split(':');

                subject.EndTime = new DateTime(
                    subject.EndTime.Year,
                    subject.EndTime.Month,
                    subject.EndTime.Day,
                    Convert.ToInt32(time[0]),
                    Convert.ToInt32(time[1]),
                    0);
            }
            catch (IndexOutOfRangeException) {
                Error("wrong time format");
            }
            catch (FormatException) {
                Error("wrong time format");
            }
        }

        private static void SetTaskBeginTime(Task subject) {
            try {
                var time = GetString("time: ").Split(':');

                subject.BeginTime = new DateTime(
                    subject.BeginTime.Year,
                    subject.BeginTime.Month,
                    subject.BeginTime.Day,
                    Convert.ToInt32(time[0]),
                    Convert.ToInt32(time[1]),
                    0);
            }
            catch (IndexOutOfRangeException) {
                Error("wrong time format");
            }
        }

        private static void SetTaskParentTarget(Task subject) {
            var tag = GetString("tag: #");

            var result = (from target in MainSession.Targets
                          where target.Tag == tag
                          select target).First();

            if (result != null)
                subject.ParentTarget = result;
            else {
                Error("this tag does not exist");
            }
        }

        private static void SetTaskEfficiency(Task subject) {
            try {
                subject.Efficiency = TaskEfficiency.Parse(GetString("efficiency: #").First());
            }
            catch (FormatException) {
                Error("this efficiency does not exist");
            }
        }

        private static void TaskDelete(Task subject) {
            if (GetAgreement("Are you sure?"))
                MainSession.Tasks.Remove(subject);
        }

        private static void SetTargetCondition(Target subject) {
            try {
                subject.Condition =
                    TargetCondition.Parse(GetString("condition: ").First());
            }
            catch (ArgumentException) {
                Error("wrong value");
            }
        }

        private static void SetTargetUseful(Target target) {
            try {
                target.Useful = bool.Parse(GetString("useful: "));
            }
            catch (FormatException) {
                Error("wrong value");
            }
        }

        private static void TargetDelete(Target subject) {
            if (GetAgreement("Are you sure?"))
                MainSession.Targets.Remove(subject);

            var targetedTasks =
                from task in MainSession.Tasks
                where task.ParentTarget == subject
                select task;

            if (!targetedTasks.Any()) return;
            WriteLine($"There are {targetedTasks.Count()} tasks of deleted target");

            switch (GetConsoleKeyInfo("[do Nothing / Delete them / Change tag]").Key) {
                case ConsoleKey.N:
                    break;

                case ConsoleKey.D:
                    MainSession.Tasks.RemoveAll(targetedTasks.Contains);
                    break;

                case ConsoleKey.C:
                    var tag = GetString("tag: #");
                    var newTarget = (from target in MainSession.Targets
                                     where target.Tag == tag
                                     select target);

                    if (!newTarget.Any())
                        if (GetAgreement("This tag does not exist. Create new one?"))
                            NewTarget(tag);
                        else
                            goto case ConsoleKey.C;

                    foreach (var task in targetedTasks)
                        task.ParentTarget = newTarget.First();
                    break;

                default:
                    Error("wrong key");
                    break;
            }
        }

        // ----- ADDITIONAL METHODS: -----

        private static Target NewTarget(string targetTag) {
            var targetName = GetString("name: ");

            targetset:
            try {
                ColorWrite("true/false ( = истина/ложь)\n", ConsoleColor.DarkGray);
                MainSession.Targets.Add(new Target(
                    targetName, targetTag, bool.Parse(GetString("useful: ")), TargetCondition.InProgress));
            }
            catch (FormatException) {
                goto targetset;
            }
            return MainSession.Targets.Last();
        }

        private static bool CatchFileExceptions(Action action, string path) {
            action:
            try {
                action();
            }
            catch (ArgumentException) {
                Error("wrong path");
                return true;
            }
            catch (DirectoryNotFoundException) {
                var dir = "";
                var i = 0;
                var folders = path.Split('\\');

                foreach (var folder in folders.TakeWhile(folder => i < folders.Length - 1)) {
                    dir += folder + '\\';
                    i++;
                }
                
                ReadKey();

                Directory.CreateDirectory(dir);
                goto action;
            }
            catch (UnauthorizedAccessException) {
                Error("access denied");
                return true;
            }
            return false;
        }
    }
}