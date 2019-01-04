using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ChessScoreboard.Desktop
{
    public static class ConsoleUtility
    {
        #region DllImports
        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        #endregion DllImports

        #region Console Size
        /// <summary>
        /// Prevent's the user from resizing the console
        /// </summary>
        public static void PreventResize()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
                DeleteMenu(sysMenu, 0xF000, 0x00000000);
        }

        /// <summary>
        /// Increase the console with by 50%
        /// </summary>
        public static void ConfigureConsoleWidths() => Console.WindowWidth += Console.WindowWidth / 2;
        #endregion Console Size

        #region Padding
        /// <summary>
        /// Pad's the left side of the console with 2 spaces
        /// </summary>
        public static void PadConsole() => PadConsole(' ');

        /// <summary>
        /// Pad's the left side of the console with 2 characters (paddingChar)
        /// </summary>
        /// <param name="paddingChar">The character to pad the left side of the console with</param>
        public static void PadConsole(char paddingChar) => PadConsole(2, paddingChar);

        /// <summary>
        /// Pad's the left side of the console with the specified (width) number of characters (paddingChar)
        /// </summary>
        /// <param name="width">Number of characters to pad with</param>
        /// <param name="paddingChar">Character to pad with</param>
        public static void PadConsole(int width, char paddingChar) => Console.Write("".PadLeft(width, paddingChar));
        #endregion Padding

        #region Writing
        /// <summary>
        /// Determines whether the WriteLineAnimated() method should be used over the default Console.WriteLine()
        /// </summary>
        public static bool UseWriteLineAnimated { get; set; } = true;

        /// <summary>
        /// Toggles the value of <see cref="UseWriteLineAnimated"/>
        /// </summary>
        public static void ToggleWriteLineAnimated() => UseWriteLineAnimated = !UseWriteLineAnimated;

        /// <summary>
        /// The amount of time the thread should sleep between writing characters in the <see cref="WriteLineAnimated(string)"/> method
        /// </summary>
        public static int AnimatedWriteLineSpeed { get; set; } = 5;

        /// <summary>
        /// Return the appropriate method to be used for writing a line of text the console
        /// </summary>
        private static Action<string> WriteLineMethod => UseWriteLineAnimated ? WriteLineAnimated : (Action<string>)Console.WriteLine;

        /// <summary>
        /// Write's an empty line. Essentially a carriage return.
        /// </summary>
        public static void WriteLine() => WriteLine("");

        /// <summary>
        /// Writes a line of text to the console
        /// </summary>
        /// <param name="message">Text to be written to the console</param>
        public static void WriteLine(string message, bool padConsole = true)
        {
            if (padConsole)
                PadConsole();

            WriteLineMethod(message);
        }

        /// <summary>
        /// Print's the heading for a user action surrounded by hyphen lines
        /// </summary>
        /// <param name="heading">The text to be written</param>
        /// <param name="clearFirst">Whether the console should be cleared before writing the heading</param>
        public static void WriteLineAsHeading(string heading, bool clearFirst = true)
        {
            if (clearFirst)
                Console.Clear();

            WriteHyphenLine();
            WriteLine(heading);
            WriteHyphenLine();
        }

        /// <summary>
        /// Writes a line of text in an animated style. Sleeps between each character
        /// </summary>
        /// <param name="message">Text to be written to the console</param>
        public static void WriteLineAnimated(string message)
        {
            foreach (char character in message)
            {
                Console.Write(character);
                Thread.Sleep(AnimatedWriteLineSpeed);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Print's a line of hypens the width of the console
        /// </summary>
        public static void WriteHyphenLine() => WriteHyphenLine(Console.BufferWidth);

        /// <summary>
        /// Print's a line of hypens the width of the passed message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteHyphenLine(string message) => WriteHyphenLine(message.Length + 2);

        /// <summary>
        /// Print's a line of hyphens the width of the passed value
        /// </summary>
        /// <param name="width">The number of character's wide the line should be</param>
        public static void WriteHyphenLine(int width) => WriteLine("".PadLeft(width, '-'), false);
        #endregion Writing

        #region Reading
        /// <summary>
        /// Read's a line of input form the user with style.
        /// </summary>
        /// <returns></returns>
        public static string ReadLine()
        {
            PadConsole(' ');
            PadConsole(1, '>');
            PadConsole(1, ' ');

            return Console.ReadLine().Trim();
        }

        public static List<string> AcceptableYesAnswers => new List<string>
        {
            "Y", "Yes", "Yea", "Sure", "Yep"
        };

        /// <summary>
        /// Read's the user's input and returns whether it was an acceptable Yes or No.
        /// </summary>
        /// <returns></returns>
        public static bool ReadYesNoAnswer()
        {
            string input = ReadLine();
            return AcceptableYesAnswers.Any(acceptableYesAnswer => acceptableYesAnswer.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reads and validates an integer from the user.
        /// </summary>
        /// <returns></returns>
        public static int ReadInt()
        {
            int number;
            string input = ReadLine();

            while (!int.TryParse(input, out number))
            {
                WriteLine();
                WriteLine($"{input} is not a valid number. Please enter a valid number.");
                input = ReadLine();
            }

            return number;
        }
        #endregion Reading
    }
}