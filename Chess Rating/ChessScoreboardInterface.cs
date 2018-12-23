using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Chess_Rating.Models;

namespace Chess_Rating
{
    public class ChessScoreboardInterface
    {
        private bool RemainOpen = true;
        private bool UseAnimatedWriteLine = true;
        private int AnimatedWriteLineSpeed = 5;
        private readonly bool PreventResizingOfConsole = true;
        private readonly ChessScoreboardAPI ChessScoreboardAPI = new ChessScoreboardAPI();

        public void Start()
        {
            if (PreventResizingOfConsole)
                PreventResize();

            ConfigureConsoleWidths();

            LoadInitialData();

            PrintWelcomeMessage();

            do
            {
                WriteLine("What would you like to do?");

                ProcessActionInput(ReadLine());

            } while (RemainOpen);
        }

        #region Start Up Methods

        #region Prevent Resize
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private void PreventResize()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
                DeleteMenu(sysMenu, 0xF000, 0x00000000);
        }
        #endregion Prevent Resize

        private void ConfigureConsoleWidths() => Console.WindowWidth += Console.WindowWidth / 2;

        private void PrintWelcomeMessage()
        {
            PrintHyphenLine();
            WriteLine("Welcome to the RSI Augusta Chess Board Interface!");
            WriteLine();
            WriteLine("Enter the action 'Help' to view information about how to use this application");
            PrintHyphenLine();
        }

        private void LoadInitialData()
        {
            UseAnimatedWriteLine = false;

            PrintHyphenLine();
            WriteLine("Loading Chess Scoreboard spreadsheet data into memory...");
            WriteLine();

            LoadData();

            WriteLine("Loading Complete.");

            Console.Clear();

            UseAnimatedWriteLine = true;
        }

        private void LoadData()
        {
            Players = ChessScoreboardAPI.GetPlayers().ToList();
            Games = ChessScoreboardAPI.GetGamesPlayed(Players).ToList();
        }
        #endregion Start Up Methods

        #region Console Interaction Methods
        private void PadConsole() => PadConsole(' ');
        private void PadConsole(char paddingChar) => PadConsole(2, paddingChar);
        private void PadConsole(int width, char paddingChar) => Console.Write("".PadLeft(width, paddingChar));

        private void PrintHyphenLine() => Console.WriteLine("".PadLeft(Console.BufferWidth, '-'));
        private void PrintHyphenLine(string message) => WriteLine("".PadLeft(message.Length + 2, '-'));

        private void WriteLine() => WriteLine("");
        private void WriteLine(string message)
        {
            PadConsole();

            if (UseAnimatedWriteLine)
                AnimatedWriteLine(message);
            else
                Console.WriteLine(message);
        }

        private void AnimatedWriteLine(string message)
        {
            foreach (char character in message)
            {
                Console.Write(character);
                Thread.Sleep(AnimatedWriteLineSpeed);
            }

            Console.WriteLine();
        }

        private string ReadLine()
        {
            PadConsole(' ');
            PadConsole(1, '>');
            PadConsole(1, ' ');

            return Console.ReadLine().Trim();
        }


        private List<string> AcceptableYesAnswers = new List<string> { "Y", "Yes", "Yea", "Sure", "Yep" };
        private bool ReadYesNoAnswer()
        {
            string input = ReadLine();
            return AcceptableYesAnswers.Any(acceptableYesAnswer => acceptableYesAnswer.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        private void PrintActionHeading(string heading)
        {
            Console.Clear();
            PrintHyphenLine();
            WriteLine(heading);
            PrintHyphenLine();
        }
        #endregion Console Interaction Methods

        #region User Actions
        private void ProcessActionInput(string input)
        {
            foreach ((string Action, string Description, Action Method, List<string> Aliases) availableAction in AvaiableActions)
            {
                if (input.Equals(availableAction.Action, StringComparison.OrdinalIgnoreCase)
                    || availableAction.Aliases.Any(alias => input.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    availableAction.Method();
                    return;
                }
            }

            UnrecognizedCommand(input);
        }

        private void UnrecognizedCommand(string action)
        {
            PrintActionHeading($"Unrecognized Action '{action}'!");

            WriteLine("If you are unsure of what actions are available, please use the 'Help' action");

            WriteLine();
            PrintHyphenLine();
        }

        private void Help()
        {
            AnimatedWriteLineSpeed = 1;

            PrintActionHeading("Help Information");

            AvailableActions();

            PrintHyphenLine();

            WriteLine();
            WriteLine("Available Answers to Yes/No Questions (Anything not on the list is treated as a no)");
            PrintHyphenLine("Available Answers to Yes/No Questions (Anything not on the list is treated as a no)");

            foreach (string acceptableYesAnswer in AcceptableYesAnswers)
                WriteLine($"-- {acceptableYesAnswer}");

            WriteLine();

            PrintHyphenLine();
            WriteLine("For more information, send me an email at dade.cook@ruralsourcing.com");
            PrintHyphenLine();

            AnimatedWriteLineSpeed = 5;
        }

        private void AvailableActions()
        {
            WriteLine();
            WriteLine("Available Actions");
            PrintHyphenLine("Available Actions");
            WriteLine();

            foreach ((string Action, string Description, Action Method, List<string> Aliases) availableAction in AvaiableActions)
            {
                string summary = $"-- Action: '{availableAction.Action}' | Description: '{availableAction.Description}'";
                PrintHyphenLine(summary);
                WriteLine(summary);

                if (availableAction.Aliases.Any())
                    WriteLine($"-- \t Action Aliases: '{string.Join(", ", availableAction.Aliases.ToArray())}'");
            }
        }

        private void Exit()
        {
            RemainOpen = false;

            Console.Clear();

            PrintHyphenLine();
            WriteLine("Goodbye!");
            WriteLine();
            WriteLine("If you have any feedback please share it with me at dade.cook@ruralsourcing.com");
            WriteLine();
            WriteLine("This window will now close in 5 seconds");
            PrintHyphenLine();

            Thread.Sleep(5000);
        }

        private void Clear()
        {
            Console.Clear();
            PrintHyphenLine();
        }

        private void ViewGames()
        {
            PrintActionHeading("View Games");

            foreach (Game game in Games)
            {
                if (game.WasAStalemate)
                    WriteLine($"-- Game #{game.Id} was between {game.Winner.FirstName} and {game.Loser.FirstName}. The outcome was a draw!");
                else
                    WriteLine($"-- Game #{game.Id} was between {game.Winner.FirstName} and {game.Loser.FirstName}. The outcome was a win for {game.Winner.FirstName}!");
            }

            PrintHyphenLine();
        }

        private void RefreshData()
        {
            Clear();

            WriteLine("Refreshing data to match the spreadsheet...");

            LoadData();

            WriteLine();
            WriteLine("Refresh Complete.");
            WriteLine();
            PrintHyphenLine();

        }

        private void ViewPlayers()
        {
            PrintActionHeading("View Players");

            foreach (Player player in Players)
                WriteLine($"-- {player.FirstName} is ranked #{player.CurrentRank} with an ELO rating of {player.Rating}");

            PrintHyphenLine();
        }

        private void UpdatePlayerRatings()
        {
            PrintActionHeading("Update Player Ratings");

            WriteLine("Should the spreadsheet automatically be updated to match the newly calculated ratings?");

            bool updateSpreadsheet = ReadYesNoAnswer();

            WriteLine();
            WriteLine("Updating Player Ratings...");

            if (Games.Any())
            {
                (double WinnerUpdatedRating, double LoserUpdatedRating) updatedRatings;
                var ratingCalculator = new RatingCalculator(kFactor: 100, baseRating: 400);

                foreach (Game game in Games)
                {
                    updatedRatings = ratingCalculator.GetNewRatings(game.WasAStalemate, game.Winner.Rating, game.Loser.Rating);

                    Players.First(player => player.RankOnLoad == game.Winner.RankOnLoad).Rating = updatedRatings.WinnerUpdatedRating;
                    Players.First(player => player.RankOnLoad == game.Loser.RankOnLoad).Rating = updatedRatings.LoserUpdatedRating;
                }
            }
            else
            {
                foreach (Player player in Players)
                    player.Rating = 400;
            }

            Players = Players.OrderByDescending(player => player.Rating).ThenBy(player => player.FirstName).ToList();

            for (int i = 0; i < Players.Count; i++)
                Players[i].CurrentRank = i + 1;

            if (updateSpreadsheet)
            {
                WriteLine();
                UpdateSpreadsheetRatingsCore();
            }

            WriteLine();
            WriteLine("Update Complete");

            WriteLine();
            WriteLine("Would you like to go ahead and view the updated player standings?");

            bool viewPlayers = ReadYesNoAnswer();

            if (viewPlayers)
                ViewPlayers();
        }

        private void UpdateSpreadsheetRatings()
        {
            PrintActionHeading("Update Spreadsheet Player Ratings");

            UpdateSpreadsheetRatingsCore();

            WriteLine("Update Complete");
        }

        private void UpdateSpreadsheetRatingsCore()
        {
            WriteLine("Updating the Spreadsheet with the new Player Ratings...");

            ChessScoreboardAPI.UpdateRatingsInSpreadsheet(Players);
        }

        private void ToggleWriteLineAnimation() => UseAnimatedWriteLine = !UseAnimatedWriteLine;
        #endregion User Actions

        #region Properties
        private List<Player> Players { get; set; }

        /// <summary>
        /// The list of games currently stored in memory for the user to interact with.
        /// </summary>
        private List<Game> Games { get; set; }

        /// <summary>
        /// The list of avaiable actions along with their description and method to be called.
        /// </summary>
        private List<(string Action, string Description, Action Method, List<string> Aliases)> AvaiableActions
        {
            get
            {
                return new List<(string Action, string Description, Action Method, List<string> Aliases)>()
                {
                    (nameof(Help), "View help information about using this Chess Scoreboard Interface", new Action(Help), new List<string>(){ "H" }),
                    (nameof(Exit), "Closes the application", new Action(Exit), new List<string>(){ "E" }),
                    (nameof(Clear), "Clears the console window of all previous text", new Action(Clear), new List<string>() { "C" }),
                    (nameof(ViewGames), "View games currently stored in memory", new Action(ViewGames), new List<string>() { "G", "Games" }),
                    (nameof(ViewPlayers), "View players currently stored in memory", new Action(ViewPlayers), new List<string>(){ "P", "Players" }),
                    (nameof(RefreshData),"Refreshes the list of games and players currently stored in memory to match those in the ChessScoreboard Spreadsheet.", new Action(RefreshData), new List<string>(){ "R", "Refresh"}),
                    (nameof(UpdatePlayerRatings), "Recalculates each players ratings based on the list of games currently in memory.", new Action(UpdatePlayerRatings), new List<string>(){ "RC", "Recalculate", "Calculate", "Calc", "Recalc" }),
                    (nameof(UpdateSpreadsheetRatings), "Updates the ChessScoreboard Spreadsheet with the ratings for each player stored in memory", new Action(UpdateSpreadsheetRatings), new List<string>(){"U", "Update"}),
                    (nameof(ToggleWriteLineAnimation), "Toggles the use of the animated WriteLine method used to output a character at a time", new Action(ToggleWriteLineAnimation), new List<string>(){ "T", "Toggle" })
                };
            }
        }
        #endregion Properties
    }
}
