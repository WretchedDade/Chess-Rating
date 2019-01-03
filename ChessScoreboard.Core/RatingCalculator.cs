using System;

namespace ChessScoreboard.Core
{
    /// <summary>
    /// A class to calculate the new rating for a games winner and loser based on the Elo Rating System.
    /// Breakdown of rating method can be found at https://metinmediamath.wordpress.com/2013/11/27/how-to-calculate-the-elo-rating-including-example/
    /// </summary>
    public class RatingCalculator
    {
        /// <summary>
        /// K Factor is what determines how valuable a game is. The higher the value, the more ratings are changed by a win or loss.
        /// </summary>
        private readonly double KFactor;

        /// <summary>
        /// The assumed base rating for new players in the system.
        /// </summary>
        private readonly double BaseRating;

        public RatingCalculator() : this(100, 400) { }

        public RatingCalculator(double kFactor, double baseRating)
        {
            KFactor = kFactor;
            BaseRating = baseRating;
        }

        /// <summary>
        /// Returns the new rating for a winner and loser of a match based on the players' original ratings and the outcome.
        /// </summary>
        /// <param name="gameWasADraw">Whether the game was a draw</param>
        /// <param name="wOriginalRating">The winner's original rating. NOTE: The term winner is irrelevant if the game was draw</param>
        /// <param name="lOriginalRating">The losers's original rating. NOTE: The term loser is irrelevant if the game was draw</param>
        /// <returns>The "Winner's" and "Loser's" new ratings</returns>
        public (double winnerRating, double loserRating) GetNewRatings(bool gameWasADraw, double wOriginalRating, double lOriginalRating)
        {
            double wTransformed = GetTransformedRating(wOriginalRating);
            double lTransformed = GetTransformedRating(lOriginalRating);

            (double wExpected, double lExpected) = GetExpectedScores(wTransformed, lTransformed);

            (double wActual, double lActual) = GetActualScores(gameWasADraw);

            double wRating = GetNewRating(wOriginalRating, wActual, wExpected);
            double lRating = GetNewRating(lOriginalRating, lActual, lExpected);

            return (Math.Round(wRating, 2), Math.Round(lRating, 2));
        }

        /// <summary>
        /// Transforms a player's current rating to one to be used for calculating the expected score
        /// </summary>
        /// <param name="currentRating">A player's current ELO rating</param>
        /// <returns>A player's transformed rating</returns>
        private double GetTransformedRating(double currentRating) => Math.Pow(10, currentRating / BaseRating);

        /// <summary>
        /// Determines the expected outcome of match based on two players' transformed ratings
        /// </summary>
        /// <param name="wTransformedRating">The "winner's" transformed rating</param>
        /// <param name="lTransformedRating">The "loser's" transformed rating</param>
        /// <returns>The expected score for the "winner" and "loser"</returns>
        private (double wExpected, double lExpected) GetExpectedScores(double wTransformedRating, double lTransformedRating)
        {
            double wExpected = wTransformedRating / (wTransformedRating + lTransformedRating);
            double lExpected = lTransformedRating / (wTransformedRating + lTransformedRating);

            return (wExpected, lExpected);
        }

        /// <summary>
        /// Determines the actual score for the winner and loser of a match. If the match was a draw each player get's half a win.
        /// </summary>
        /// <param name="gameWasADraw">Whether the game was a draw.</param>
        /// <returns>The actual scores for a match.</returns>
        private (double wActual, double lActual) GetActualScores(bool gameWasADraw) => gameWasADraw ? (.5, .5) : (1, 0);

        /// <summary>
        /// Calculates the new rating for a player based on their rating prior to the match, the outcome, and the expected outcome.
        /// </summary>
        /// <param name="originalRating"></param>
        /// <param name="actualScore"></param>
        /// <param name="expectedScore"></param>
        /// <returns></returns>
        private double GetNewRating(double originalRating, double actualScore, double expectedScore)
            => originalRating + (KFactor * (actualScore - expectedScore));
    }
}
