namespace ChessScoreboard.Core.Models
{
    public class Player
    {
        public Player(int rankOnLoad, double rating, string name)
        {
            RankOnLoad = rankOnLoad;
            CurrentRank = rankOnLoad;
            Rating = rating;
            Name = name;
        }

        public int RankOnLoad { get; set; }
        public int CurrentRank { get; set; }
        public double Rating { get; set; }
        public string Name { get; set; }

        public string FirstName => Name.Substring(0, Name.IndexOf(' '));
    }
}
