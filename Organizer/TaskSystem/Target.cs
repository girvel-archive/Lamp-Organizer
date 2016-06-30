using System;

namespace Organizer.TaskSystem {
    [Serializable]
    public class Target : IConsoleWritable, IPortable {
        public string Name { get; set; }
        public string Tag { get; set; }
        public TargetCondition Condition { get; set; }

        public bool Useful { get; set; }

        public string ConsoleInfo => $"({Condition.Symbol})\t{ (Useful ? "useful" : "not") }\t#{Tag}\t{Name}" ;

        public Target(string name, string tag, bool useful, TargetCondition condition) {
            Name = name;
            Tag = tag;
            Useful = useful;
            Condition = condition;
        }

        public void Port(ProgramVersion oldVersion) {
            if (oldVersion > ProgramVersion.Basic10) return;

            Condition.Port(oldVersion);
        }
    }
}