using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using ChessScoreboard.Core.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace ChessScoreboard.Core
{
    public class ChessScoreboardAPI
    {
        /// <summary>
        /// The service used to make calls to the Google Sheets API
        /// </summary>
        private SheetsService SheetsService;

        public ChessScoreboardAPI() => EstablishConnectionToSpreasheet();

        /// <summary>
        /// Obtains a <see cref="UserCredential"/> and sets the <see cref="SheetsService"/> field
        /// </summary>
        private void EstablishConnectionToSpreasheet()
        {
            UserCredential userCredential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var dataStore = new FileDataStore(Constants.CredDataStoreLocation, true);
                var scopes = new List<string>() { SheetsService.Scope.Spreadsheets };
                ClientSecrets clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "user", CancellationToken.None, dataStore).Result;
            }

            var baseClientInitializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = Constants.AppName
            };

            SheetsService = new SheetsService(baseClientInitializer);
        }

        /// <summary>
        /// Makes a call to the Google Sheets API pulling back the data from the Games sheet in the ChessScoreboard and turning it into Games
        /// </summary>
        /// <returns>All valid games in the ChessScoreboard</returns>
        public IEnumerable<Game> GetGamesPlayed() => GetGamesPlayed(GetPlayers().ToList());

        /// <summary>
        /// Makes a call to the Google Sheets API pulling back the data from the Games sheet in the ChessScoreboard and turning it into Games
        /// </summary>
        /// <param name="players">List of players to get games for</param>
        /// <returns>All valid games belonging to any player in the passed list</returns>
        public IEnumerable<Game> GetGamesPlayed(List<Player> players)
        {
            ValueRange response = SheetsService.Spreadsheets.Values.Get(Constants.SpreadsheetId, Constants.Games.FullRange).Execute();

            foreach (IList<object> row in response.Values)
            {
                if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0].ToString()) || string.IsNullOrWhiteSpace(row[1].ToString()) || string.IsNullOrWhiteSpace(row[2].ToString()))
                    break;

                int id = Convert.ToInt32(row[0]);

                string winnerName = Convert.ToString(row[1]);
                string loserName = Convert.ToString(row[2]);

                Player winner = players.FirstOrDefault(player => player.Name.Equals(winnerName, StringComparison.OrdinalIgnoreCase));
                Player loser = players.FirstOrDefault(player => player.Name.Equals(loserName, StringComparison.OrdinalIgnoreCase));

                bool wasStalemate = Convert.ToBoolean(row[3]);

                yield return new Game(id, winner, loser, wasStalemate);
            }
        }

        /// <summary>
        /// Makes a call to the Google Sheets API pulling back the data from the Players sheet in the ChessScoreboard and turning it into Players
        /// </summary>
        /// <returns>All available players on the ChessScoreboard</returns>
        public IEnumerable<Player> GetPlayers()
        {
            GetRequest request = SheetsService.Spreadsheets.Values.Get(Constants.SpreadsheetId, Constants.Players.FullRange);

            ValueRange response = request.Execute();

            foreach (IList<object> row in response.Values)
            {
                if (string.IsNullOrWhiteSpace(row[0].ToString()) || string.IsNullOrWhiteSpace(row[1].ToString()))
                    break;

                int id = Convert.ToInt32(row[0]);

                double rating = Constants.BaseRating;
                if (row.Count >= 6 && !string.IsNullOrWhiteSpace(row[5].ToString()))
                    rating = Convert.ToDouble(row[5]);

                yield return new Player(id, rating, row[1].ToString());
            }
        }

        public void UpdateRatingsInSpreadsheet(List<Player> players)
        {
            players = players.OrderBy(player => player.RankOnLoad).ToList();

            var requestBody = new ValueRange
            {
                Values = players.Select(player => new List<object>() { player.Rating } as IList<object>).ToList()
            };

            string ratingsRange = $"{Constants.Players.SheetName}!{Constants.Players.RatingColumn}$2:{Constants.Players.RatingColumn}${players.Count + 1}";

            UpdateSpreadsheet(requestBody, ratingsRange);
        }

        public void UpdatePlayersInSpreadsheet(List<Player> players)
        {
            players = players.OrderBy(player => player.RankOnLoad).ToList();

            var requestBody = new ValueRange
            {
                Values = players.Select(player => new List<object>() { player.Name } as IList<object>).ToList()
            };

            string namesRange = $"{Constants.Players.SheetName}!{Constants.Players.NameColumn}$2:{Constants.Players.NameColumn}${players.Count + 1}";

            UpdateSpreadsheet(requestBody, namesRange);

            UpdateRatingsInSpreadsheet(players);
        }

        public void UpdateGamesInSpreadsheet(List<Game> games)
        {
            string gamesRange = $"{Constants.Games.SheetName}!{Constants.Games.WinnerColumn}$2:{Constants.Games.StalemateColumn}$";
            gamesRange = games.Any() ? $"{gamesRange}{games.Count + 1}" : $"{gamesRange}{Constants.Games.LastRow}";


            var requestBody = new ValueRange { Values = new List<IList<object>> { new List<object> { "", "", "" } } };
            requestBody = new ValueRange { Values = games.Select(game => new List<object> { game.Winner.Name, game.Loser.Name, game.WasAStalemate } as IList<object>).ToList() };

            UpdateSpreadsheet(requestBody, gamesRange);
        }

        private void UpdateSpreadsheet(ValueRange valueRange, string range)
        {
            UpdateRequest request = SheetsService.Spreadsheets.Values.Update(valueRange, Constants.SpreadsheetId, range);
            request.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;

            request.Execute();

            FormatSpreadsheet();
        }

        private async void FormatSpreadsheet()
        {
            using (var httpClient = new HttpClient())
                await httpClient.GetAsync(Constants.AppScriptExecURL);
        }
    }
}