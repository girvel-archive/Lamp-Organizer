using System;
using static System.Console;
using static Organizer.Program;

namespace Organizer {
    public class ConsoleDecorator {
        public static void ColorWrite(
            string message, 
            ConsoleColor foregroundColor) {

            ColorWrite(message, foregroundColor, BackgroundColor);
        }

        public static void ColorWrite(
            string message,
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor) {

            var oldF = ForegroundColor;
            var oldB = BackgroundColor;

            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;

            Write(message);

            ForegroundColor = oldF;
            BackgroundColor = oldB;
        }

        public static void AnyKey(string addition = "") {
            WriteLine($"\nPress any key {addition}");
            ReadKey(true);
        }

        public static void Separator() {
            WriteLine("\n----------\n\n");
        }

        public static void Error(string message) {
            WriteLine($"E: {message}");
            AnyKey("to continue");
        }

        public static bool GetAgreement(string message, ConsoleKey keyAgree = ConsoleKey.Y) {
            return GetConsoleKeyInfo($"{message} [y/n]").Key == keyAgree;
        }

        public static string GetString(string message) {
            Write(message);
            return ReadLine();
        }

        public static string GetPath(string message = "path: @") {
            return GetString(message)
                .Replace("*", DateTime.Now.ToString("dd MM yyyy"))
                .Replace("~", "saves")
                .Replace("`", CurrentFilePath)
                .Replace("?", "saves\\" + DateTime.Now.ToString("dd MM yyyy"));
        }

        public static ConsoleKeyInfo GetConsoleKeyInfo(string message) {
            WriteLine(message);
            return ReadKey(true);
        }
    }
}