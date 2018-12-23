using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Chess_Rating.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace Chess_Rating
{
    public class ChessScoreboardAPI
    {
        /// <summary>
        /// Range containing the list of games played
        /// </summary>
        private readonly string GamesRange;

        /// <summary>
        /// Range containting the list of available players
        /// </summary>
        private readonly string PlayersRange;

        /// <summary>
        /// Id for the Chess Scoreboard Spreadsheet
        /// </summary>
        private readonly string SpreadsheetId;

        /// <summary>
        /// Name of the app
        /// </summary>
        private readonly string AppName;

        /// <summary>
        /// Name of the DataStore to be creating on obtaining a user credential
        /// </summary>
        private readonly string CredDataStoreLocation;

        /// <summary>
        /// The service used to make calls to the Google Sheets API
        /// </summary>
        private SheetsService SheetsService;

        public ChessScoreboardAPI()
        {
            AppName = "Chess Scoreboard";
            GamesRange = "Games!$A$2:$D$201";
            PlayersRange = "Data!$A$2:$F$201";
            CredDataStoreLocation = "token.json";
            SpreadsheetId = "1EcjGl22n3CaxF9x0BJzPaNBxi8C3C-uMqWsxtmkkFTs";

            EstablishConnectionToSpreasheet();
        }

        /// <summary>
        /// Obtains a <see cref="UserCredential"/> and sets the <see cref="SheetsService"/> field
        /// </summary>
        private void EstablishConnectionToSpreasheet()
        {
            UserCredential userCredential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var dataStore = new FileDataStore(CredDataStoreLocation, true);
                var scopes = new List<string>() { SheetsService.Scope.Spreadsheets };
                ClientSecrets clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "user", CancellationToken.None, dataStore).Result;
            }

            var baseClientInitializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = AppName
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
            ValueRange response = SheetsService.Spreadsheets.Values.Get(SpreadsheetId, GamesRange).Execute();

            foreach (IList<object> row in response.Values)
            {
                if (string.IsNullOrWhiteSpace(row[0].ToString()) || string.IsNullOrWhiteSpace(row[1].ToString()) || string.IsNullOrWhiteSpace(row[2].ToString()))
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
            GetRequest request = SheetsService.Spreadsheets.Values.Get(SpreadsheetId, PlayersRange);

            ValueRange response = request.Execute();

            foreach (IList<object> row in response.Values)
            {
                if (string.IsNullOrWhiteSpace(row[0].ToString()) || string.IsNullOrWhiteSpace(row[1].ToString()))
                    break;

                int id = Convert.ToInt32(row[0]);

                double rating = 400;
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

            string ratingsRange = $"Data!$F$2:$F${players.Count + 1}";

            UpdateRequest request = SheetsService.Spreadsheets.Values.Update(requestBody, SpreadsheetId, ratingsRange);
            request.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;

            UpdateValuesResponse response = request.Execute();
        }
    }
}