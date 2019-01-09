class RatingCalculator {

    /**
     * Creates a new RatingCalculator object
     * @param {number} kFactor K Factor is what determines how valuable a game is. The higher the value, the more ratings are changed by a win or loss.
     * @param {number} baseRating The base rating for new players in the system. 
     */
    constructor(kFactor, baseRating) {
        this.KFactor = kFactor ? kFactor : 100;
        this.BaseRating = baseRating ? baseRating : 400;
    }

    /**
     * Returns the new rating for a winner and loser of a match based on the players' original ratings and the outcome.
     * @param {boolean} gameWasADraw Whether the match being evaluated was a draw
     * @param {Ratings} ratings The original ratings of the players involved in the match. If the match was a draw, winner and loser is irrelevant
     */
    GetNewRatings(gameWasADraw, ratings) {
        var wTransformed = this.GetTransformedRating(ratings.WinnersRating);
        var lTransformed = this.GetTransformedRating(ratings.LosersRating);

        var expectedScores = this.GetExpectedScores(wTransformed, lTransformed);

        var actualScores = this.GetActualScores(gameWasADraw);

        var wRating = this.GetNewRating(ratings.WinnersRating, actualScores.Winners, expectedScores.Winners);
        var lRating = this.GetNewRating(ratings.LosersRating, actualScores.Losers, expectedScores.Losers);

        return new Ratings(parseFloat(wRating.toFixed(2)), parseFloat(lRating.toFixed(2)));
    }

    /**
     * Transforms a player's current rating to one to be used for calculating the expected score
     * @param {number} currentRating A player's current ELO rating
     */
    GetTransformedRating(currentRating) {
        return Math.pow(10, currentRating / this.BaseRating);
    }

    /**
     * Determines the expected outcome of match based on two players' transformed ratings
     * @param {number} winnersTransformedRating The "winners's" transformed rating
     * @param {number} losersTransformedRating The "loser's" transformed rating
     */
    GetExpectedScores(winnersTransformedRating, losersTransformedRating) {
        var wExpected = winnersTransformedRating / (winnersTransformedRating + losersTransformedRating);
        var lExpected = losersTransformedRating / (winnersTransformedRating + losersTransformedRating);

        return new Scores(wExpected, lExpected);
    }

    /**
     * Determines the actual score for the winner and loser of a match. If the match was a draw each player get's half a win.
     * @param {boolean} gameWasADraw Whether the game was a draw.
     */
    GetActualScores(gameWasADraw) {
        return gameWasADraw ? new Scores(.5, .5) : new Scores(1, 0);
    }

    /**
     * Calculates the new rating for a player based on their rating prior to the match, the outcome, and the expected outcome.
     * @param {number} originalRating The players orignal rating
     * @param {number} actualScore The players actual score as determined by GetActualScores()
     * @param {number} expectedScore The players exptected score as determined by GetExpectedScores()
     */
    GetNewRating(originalRating, actualScore, expectedScore) {
        return originalRating + (this.KFactor * (actualScore - expectedScore));
    }

    Round(value, numPlaces) {
        return Number(Math.round(value + 'e' + numPlaces) + 'e-' + numPlaces);
    }
}

class Ratings {
    constructor(winner, loser) {
        this.WinnersRating = winner;
        this.LosersRating = loser;
    }
}

class Scores {
    constructor(winners, losers) {
        this.Winners = winners;
        this.Losers = losers;
    }
}