using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Organizer.TaskSystem;

namespace Organizer {
    [Serializable]
    public class Session {
        public ProgramVersion Version = ProgramVersion.Basic101;

        public List<Task> Tasks = new List<Task>();
        public List<Target> Targets = new List<Target>();
        public DateTime Date { get; set; }

        public string Description { get; set; }

        public void Save(Stream serializationStream) {
            new BinaryFormatter().Serialize(serializationStream, this);
        }

        public void CheckVersion() {
            Tasks.ForEach(t => t.Port(Version));
            Targets.ForEach(t => t.Port(Version));

            Version = ProgramVersion.Basic101;
        }

        public static Session Open(Stream serializationStream) {
            var result = (Session) new BinaryFormatter().Deserialize(serializationStream);
            result.CheckVersion();
            return result;
        }

        public Session() {
            Date = DateTime.Now;
        }
    }
}