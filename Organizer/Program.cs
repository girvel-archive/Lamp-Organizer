using static Organizer.ConsoleHelper;

namespace Organizer {
    public static class Program {
        public static Session MainSession;

        public static string CurrentFilePath;

        private static void Main(string[] args) {
            Open();

            ControlLoop();
        }
    }
}
