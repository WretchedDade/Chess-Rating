namespace ChessScoreboard.Core.Models
{
    public class Game
    {
        public Game() { }

        public Game(int id, Player winner, Player loser, bool wasStalement)
        {
            Id = id;
            Winner = winner;
            Loser = loser;
            WasAStalemate = wasStalement;
        }

        public int Id { get; set; }
        public Player Winner { get; set; }
        public Player Loser { get; set; }
        public bool WasAStalemate { get; set; }
    }
}