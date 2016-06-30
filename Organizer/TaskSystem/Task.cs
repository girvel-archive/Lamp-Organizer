using System;

namespace Organizer.TaskSystem {
    [Serializable]
    public class Task : IConsoleWritable, IPortable {
        public string Name { get; set; }
        public Target ParentTarget { get; set; }
        public TaskEfficiency Efficiency { get; set; }

        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public string ConsoleInfo => 
            $"{BeginTime.ToString("HH:mm")}\t{EndTime.ToString("HH:mm")}\t#{ParentTarget.Tag}\t#{Efficiency.Symbol}\t{Name}";

        public Task(
            string name,
            Target parentTarget,
            TaskEfficiency efficiency,
            DateTime beginTime,
            DateTime endTime) {

            Name = name;
            ParentTarget = parentTarget;
            Efficiency = efficiency;
            BeginTime = beginTime;
            EndTime = endTime;
        }

        public void Port(ProgramVersion oldVersion) {
            
        }
    }
}
