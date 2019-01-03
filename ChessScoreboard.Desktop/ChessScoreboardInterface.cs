using System;
using System.Collections.Generic;
using System.Linq;
using ChessScoreboard.Core;
using ChessScoreboard.Core.Models;

namespace ChessScoreboard.Desktop
{
    public partial class ChessScoreboardInterface
    {
        private bool RemainOpen;
        private List<Game> Games;
        private List<Player> Players;
        private ChessScoreboardAPI ChessScoreboardAPI;

        public ChessScoreboardInterface()
        {
            RemainOpen = true;
            Games = new List<Game>();
            Players = new List<Player>();
            ChessScoreboardAPI = new ChessScoreboardAPI();
        }

        public void Start()
        {
            ConsoleUtility.PreventResize();
            ConsoleUtility.ConfigureConsoleWidths();

            //Loading data...
            ConsoleUtility.WriteHyphenLine();
            ConsoleUtility.WriteLine("Loading Chess Scoreboard spreadsheet data into memory...");
            ConsoleUtility.WriteLine();

            LoadData();

            ConsoleUtility.WriteLine("Loading Complete.");

            Console.Clear();

            //Welcome Message
            ConsoleUtility.WriteHyphenLine();
            ConsoleUtility.WriteLine("Welcome to the RSI Augusta Chess Board Interface!");
            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Enter the action 'Help' to view information about how to use this application");
            ConsoleUtility.WriteHyphenLine();

            do
            {
                ConsoleUtility.WriteLine("What would you like to do?");

                ProcessActionInput(ConsoleUtility.ReadLine());

            } while (RemainOpen);
        }

        private void LoadData()
        {
            Players = ChessScoreboardAPI.GetPlayers().ToList();
            Games = ChessScoreboardAPI.GetGamesPlayed(Players).ToList();
        }

        private void ProcessActionInput(string input)
        {
            foreach (UserAction userAction in UserActions)
            {
                if (input.Equals(userAction.Name, StringComparison.OrdinalIgnoreCase) || userAction.Aliases.Any(alias => input.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    userAction.Method();
                    return;
                }
            }

            UnrecognizedCommand(input);
        }

        private void UnrecognizedCommand(string action)
        {
            ConsoleUtility.WriteLineAsHeading($"Unrecognized Action '{action}'!");

            ConsoleUtility.WriteLine("If you are unsure of what actions are available, please use the 'Help' action");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteHyphenLine();
        }
    }
}