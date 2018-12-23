namespace Chess_Rating
{
    class Program
    {

        static void Main(string[] args)
        {
            var chessScoreboardInterface = new ChessScoreboardInterface();
            chessScoreboardInterface.Start();
            //var players = GetPlayers().ToList();
            //var chessGames = GetGames(players).ToList();

            //UpdatePlayerRatings(ref players, chessGames.ToList());

            //WritePlayers(players);
            ////WriteGames(chessGames);

            //Console.ReadKey();
        }

        

        
    }
}
