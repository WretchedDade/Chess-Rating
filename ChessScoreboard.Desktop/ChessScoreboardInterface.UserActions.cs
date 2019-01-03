using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChessScoreboard.Core;
using ChessScoreboard.Core.Models;

namespace ChessScoreboard.Desktop
{
    public partial class ChessScoreboardInterface
    {
        private List<UserAction> UserActions => new List<UserAction>()
        {
            new UserAction(nameof(Help), "View help information about using this Chess Scoreboard Interface", new Action(Help), new List<string>(){ "H" }),
            new UserAction(nameof(Exit), "Closes the application", new Action(Exit), new List<string>(){ "E" }),
            new UserAction(nameof(Clear), "Clears the console window of all previous text", new Action(Clear), new List<string>() { "C" }),
            new UserAction(nameof(ViewGames), "View games currently stored in memory", new Action(ViewGames), new List<string>() { "G", "Games" }),
            new UserAction(nameof(ViewPlayers), "View players currently stored in memory", new Action(ViewPlayers), new List<string>(){ "P", "Players" }),
            new UserAction(nameof(RefreshData),"Refreshes the list of games and players currently stored in memory to match those in the ChessScoreboard Spreadsheet.", new Action(RefreshData), new List<string>(){ "R", "Refresh"}),
            new UserAction(nameof(UpdatePlayerRatings), "Recalculates each players ratings based on the list of games currently in memory.", new Action(UpdatePlayerRatings), new List<string>(){ "RC", "Recalculate", "Calculate", "Calc", "Recalc" }),
            new UserAction(nameof(ConsoleUtility.ToggleWriteLineAnimated), "Toggles the use of the animated WriteLine method used to output a character at a time", new Action(ConsoleUtility.ToggleWriteLineAnimated), new List<string>(){ "T", "Toggle" }),
            new UserAction(nameof(AddGame), "Add's a new game to the collection of games!", new Action(AddGame), new List<string>(){ "AG" }),
            new UserAction(nameof(AddPlayer), "Add's a new game to the collection of games!", new Action(AddPlayer), new List<string>(){ "AP" }),
            new UserAction(nameof(ClearGames), "Add's a new game to the collection of games!", new Action(ClearGames), new List<string>(){ "CG" }),
        };

        private void AddGame()
        {
            ConsoleUtility.WriteLineAsHeading("Add Game");

            var game = new Game();

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Was the game a stalemate?");
            game.WasAStalemate = ConsoleUtility.ReadYesNoAnswer();

            ConsoleUtility.WriteHyphenLine();
            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Avaiable Players");
            ConsoleUtility.WriteHyphenLine("Avaiable Players");

            foreach (Player player in Players)
                ConsoleUtility.WriteLine($"{player.CurrentRank}. {player.FirstName}");

            ConsoleUtility.WriteHyphenLine();

            int winnerNumber, loserNumber;

            if (game.WasAStalemate)
            {
                ConsoleUtility.WriteLine();
                ConsoleUtility.WriteLine("In the above list, what number was player one?");
                winnerNumber = ConsoleUtility.ReadInt();

                ConsoleUtility.WriteLine();
                ConsoleUtility.WriteLine("In the above list, what number was player two?");
                loserNumber = ConsoleUtility.ReadInt();
            }
            else
            {
                ConsoleUtility.WriteLine();
                ConsoleUtility.WriteLine("In the above list, what number was the winner?");
                winnerNumber = ConsoleUtility.ReadInt();

                ConsoleUtility.WriteLine();
                ConsoleUtility.WriteLine("In the above list, what number was the loser?");
                loserNumber = ConsoleUtility.ReadInt();
            }

            Player winner, loser;
            winner = Players.FirstOrDefault(player => player.CurrentRank == winnerNumber);
            loser = Players.FirstOrDefault(player => player.CurrentRank == loserNumber);

            while (winner == null || loser == null)
            {
                Clear();

                ConsoleUtility.WriteHyphenLine();
                ConsoleUtility.WriteLine("Unable to find one or both of the players for the numbers specified. If you do not see a player in the list please add one using the add player action");
                ConsoleUtility.WriteLine("Try again?");
                bool tryAgain = ConsoleUtility.ReadYesNoAnswer();

                if (tryAgain)
                {
                    if (game.WasAStalemate)
                    {
                        ConsoleUtility.WriteLine();
                        ConsoleUtility.WriteLine("In the above list, what number was player one?");
                        winnerNumber = ConsoleUtility.ReadInt();

                        ConsoleUtility.WriteLine();
                        ConsoleUtility.WriteLine("In the above list, what number was player two?");
                        loserNumber = ConsoleUtility.ReadInt();
                    }
                    else
                    {
                        ConsoleUtility.WriteLine();
                        ConsoleUtility.WriteLine("In the above list, what number was the winner?");
                        winnerNumber = ConsoleUtility.ReadInt();

                        ConsoleUtility.WriteLine();
                        ConsoleUtility.WriteLine("In the above list, what number was the loser?");
                        loserNumber = ConsoleUtility.ReadInt();
                    }

                    winner = Players.FirstOrDefault(player => player.CurrentRank == winnerNumber);
                    loser = Players.FirstOrDefault(player => player.CurrentRank == loserNumber);
                }
                else
                {
                    Clear();
                    return;
                }
            }

            game.Winner = winner;
            game.Loser = loser;

            Games.Add(game);

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating ratings based on added game...");
            UpdatePlayerRatingsCore();
            ConsoleUtility.WriteLine("Complete");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating collection of games in spreadsheet...");
            ChessScoreboardAPI.UpdateGamesInSpreadsheet(Games);
            ConsoleUtility.WriteLine("Complete");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating collection of players in spreadsheet...");
            ChessScoreboardAPI.UpdateRatingsInSpreadsheet(Players);
            ConsoleUtility.WriteLine("Complete");

            Clear();
        }

        private void AddPlayer()
        {
            ConsoleUtility.WriteLineAsHeading("Add Player");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("What is the name of the player you would like to add?");

            string playerName = ConsoleUtility.ReadLine();

            var player = new Player(Players.Max(p => p.CurrentRank) + 1, Constants.BaseRating, playerName);
            Players.Add(player);

            ChessScoreboardAPI.UpdatePlayersInSpreadsheet(Players);

            Clear();
            ViewPlayers();
        }

        private void Clear()
        {
            Console.Clear();
            ConsoleUtility.WriteHyphenLine();
        }

        private void ClearGames()
        {
            ConsoleUtility.WriteLineAsHeading("Clear Games");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Clearing list of games...");
            Games = new List<Game>();
            ConsoleUtility.WriteLine("Complete");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Reseting player ratings");
            UpdatePlayerRatingsCore();
            ConsoleUtility.WriteLine("Complete");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating collection of games in spreadsheet...");
            ChessScoreboardAPI.UpdateGamesInSpreadsheet(Games);
            ConsoleUtility.WriteLine("Complete");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating collection of players in spreadsheet...");
            ChessScoreboardAPI.UpdateRatingsInSpreadsheet(Players);
            ConsoleUtility.WriteLine("Complete");

            Clear();
        }

        private void Exit()
        {
            RemainOpen = false;

            Console.Clear();

            ConsoleUtility.WriteHyphenLine();
            ConsoleUtility.WriteLine("Goodbye!");
            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("If you have any feedback please share it with me at dade.cook@ruralsourcing.com");
            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("This window will now close in 5 seconds");
            ConsoleUtility.WriteHyphenLine();

            Thread.Sleep(5000);
        }

        private void Help()
        {
            ConsoleUtility.WriteLineAsHeading("Help Information");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Available Actions");
            ConsoleUtility.WriteHyphenLine("Available Actions");
            ConsoleUtility.WriteLine();

            foreach (UserAction userAction in UserActions)
            {
                string summary = $"-- Action: '{userAction.Name}' | Description: '{userAction.Description}'";
                ConsoleUtility.WriteHyphenLine(summary);
                ConsoleUtility.WriteLine(summary);

                if (userAction.Aliases.Any())
                    ConsoleUtility.WriteLine($"-- \t Action Aliases: '{string.Join(", ", userAction.Aliases.ToArray())}'");
            }

            ConsoleUtility.WriteHyphenLine();

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Available Answers to Yes/No Questions (Anything not on the list is treated as a no)");
            ConsoleUtility.WriteHyphenLine("Available Answers to Yes/No Questions (Anything not on the list is treated as a no)");

            foreach (string acceptableYesAnswer in ConsoleUtility.AcceptableYesAnswers)
                ConsoleUtility.WriteLine($"-- {acceptableYesAnswer}");

            ConsoleUtility.WriteLine();

            ConsoleUtility.WriteHyphenLine();
            ConsoleUtility.WriteLine("For more information, send me an email at dade.cook@ruralsourcing.com");
            ConsoleUtility.WriteHyphenLine();
        }

        private void RefreshData()
        {
            Clear();

            ConsoleUtility.WriteLine("Refreshing data to match the spreadsheet...");

            LoadData();

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Refresh Complete.");
            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteHyphenLine();
        }

        private void UpdatePlayerRatings()
        {
            ConsoleUtility.WriteLineAsHeading("Update Player Ratings");

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating Player Ratings...");

            UpdatePlayerRatingsCore();

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Updating the Spreadsheet with the new Player Ratings...");
            ChessScoreboardAPI.UpdateRatingsInSpreadsheet(Players);

            ConsoleUtility.WriteLine();
            ConsoleUtility.WriteLine("Update Complete");

            Clear();
            ViewPlayers();
        }

        private void UpdatePlayerRatingsCore()
        {
            if (Games.Any())
            {
                Players.ForEach(player => player.Rating = Constants.BaseRating);

                (double WinnerUpdatedRating, double LoserUpdatedRating) updatedRatings;
                var ratingCalculator = new RatingCalculator(kFactor: 100, baseRating: Constants.BaseRating);

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
                    player.Rating = Constants.BaseRating;
            }

            Players = Players.OrderByDescending(player => player.Rating).ThenBy(player => player.FirstName).ToList();

            for (int i = 0; i < Players.Count; i++)
                Players[i].CurrentRank = i + 1;
        }

        private void ViewGames()
        {
            ConsoleUtility.WriteLineAsHeading("View Games");

            foreach (Game game in Games)
            {
                if (game.WasAStalemate)
                    ConsoleUtility.WriteLine($"-- Game #{game.Id} was between {game.Winner.FirstName} and {game.Loser.FirstName}. The outcome was a draw!");
                else
                    ConsoleUtility.WriteLine($"-- Game #{game.Id} was between {game.Winner.FirstName} and {game.Loser.FirstName}. The outcome was a win for {game.Winner.FirstName}!");
            }

            ConsoleUtility.WriteHyphenLine();
        }

        private void ViewPlayers()
        {
            ConsoleUtility.WriteLineAsHeading("View Players");

            foreach (Player player in Players)
                ConsoleUtility.WriteLine($"{player.CurrentRank}. {player.FirstName} is ranked #{player.CurrentRank} with an ELO rating of {player.Rating}");

            ConsoleUtility.WriteHyphenLine();
        }

        private class UserAction
        {
            public string Name;
            public string Description;
            public Action Method;
            public List<string> Aliases;

            public UserAction(string action, string description, Action method, List<string> aliases)
            {
                Name = action;
                Description = description;
                Method = method;
                Aliases = aliases;
            }
        }
    }
}