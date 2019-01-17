var Calculator;
var GamesTable;
var PlayersTable;
var AddGamePlayersTable;
var ChessScoreboardSheet;
var Constants = new ChessScoreboardConstants();

function init() {
    Calculator = new RatingCalculator(Constants.KFactor, Constants.BaseRating);
    ChessScoreboardSheet = new Sheet(Constants.APIKey, Constants.SpreadsheetId, Constants.ClientId, Constants.AppScriptUrl);
    ChessScoreboardSheet.Connect(function () {
        if (ChessScoreboardSheet.UserIsSignedIn) {
            GamesTable = new SheetsTable('Games!$B$2:$D$201', ChessScoreboardSheet, '#Games', ['#', 'Winner', 'Loser', 'Was Stalemate'], GamesTableRefreshCallback);
            PlayersTable = new SheetsTable('Players!$A$1:$G$201', ChessScoreboardSheet, '#Players', undefined, PlayersTableRefreshCallback);
            AddGamePlayersTable = new SheetsTable('Players!$A$2:$B$201', ChessScoreboardSheet, '#AddGamesPlayers', ['#', 'Name']);
        }
    })
}

function PlayersTableRefreshCallback() {
    var faIcon;
    var lastIndex = $('#Players tbody tr').length - 1;

    $('#Players tbody td:nth-of-type(1)').each(function (index) {
        faIcon = 'fa-chess-';
        switch (index) {
            case 0:
                faIcon += 'king';
                break;
            case 1:
                faIcon += 'queen';
                break;
            case 2:
                faIcon += 'rook';
                break;
            case 3:
                faIcon += 'bishop';
                break;
            case 4:
                faIcon += 'knight';
                break;
            case lastIndex:
                faIcon = 'fa-dumpster-fire';
                break;
            default:
                faIcon += 'pawn';
        }

        $(this).empty().prepend(`<i class="fas fa-2x text-primary pl-2 ${faIcon}"></i>`);
    });
}

function GamesTableRefreshCallback() {

    $('#Games tbody tr').each(function (index) {
        $(this).prepend(`<td class="text-center align-middle">${index + 1}</td>`);
    });
}

function OpenAddGameModal(event) {
    event.preventDefault();

    $('#AddGameModal').modal('show');

    $("input").attr("max", AddGamePlayersTable.Values.length);
}

function AddGameClick(event) {
    event.preventDefault();

    var winnerNumber = parseInt($('#WinnerNumber').val());
    var loserNumber = parseInt($('#LoserNumber').val());
    var wasStalemate = $('#WasStalemate').prop('checked');

    var winner = AddGamePlayersTable.Values[winnerNumber - 1][1];
    var loser = AddGamePlayersTable.Values[loserNumber - 1][1];

    var newRow = [winner, loser, wasStalemate]

    GamesTable.AddRow(newRow).then(Recalculate);

    $('#AddGameModal').modal('hide');
}

function ToggleUpDownCaret(anchor) {
    var icon;
    $(anchor).children('i').each(function () {
        icon = $(this);
        icon.toggleClass('d-none');
        icon.toggleClass('d-block');
    });
}

function RecalculateClick(event) {
    event.preventDefault();
    Recalculate();
}

function Recalculate() {
    var games = GamesTable.Values;
    var players = PlayersTable.Values;

    players.shift(); //Get rid of header row

    for (var i = 0; i < players.length; i++)
        players[i][Constants.PlayersRatingIndex] = Calculator.BaseRating;

    var winnersIndex, losersIndex, winnersRating, losersRating, wasStalemate;
    for (var i = 0; i < games.length; i++) {
        winnersIndex = FindPlayersIndexByName(players, games[i][Constants.GamesWinnerIndex]);
        winnersRating = players[winnersIndex][Constants.PlayersRatingIndex];

        losersIndex = FindPlayersIndexByName(players, games[i][Constants.GamesLoserIndex]);
        losersRating = players[losersIndex][Constants.PlayersRatingIndex];

        wasStalemate = games[i][Constants.GamesWasStalemateIndex].toString().toLowerCase() === 'true';

        var newRatings = Calculator.GetNewRatings(wasStalemate, new Ratings(winnersRating, losersRating));

        players[winnersIndex][Constants.PlayersRatingIndex] = newRatings.WinnersRating;
        players[losersIndex][Constants.PlayersRatingIndex] = newRatings.LosersRating;
    }

    UpdatePlayerRatingsInSpreadsheet(players);
}

function UpdatePlayerRatingsInSpreadsheet(players) {
    var ratings = players.map(function (value) {
        return [value[Constants.PlayersRatingIndex]];
    });

    var valueRangeBody = {
        values: ratings
    };


    PlayersTable.Sheet.Put('Players!$F$2:$F$201', valueRangeBody).then(function (response) {
        PlayersTable.SendGetRequestToAppScriptUrl().always(function () {
            setTimeout(function () {
                PlayersTable.Refresh();
                GamesTable.Refresh();
                AddGamePlayersTable.Refresh();
            }, 1000);
        });
    });
}


function FindPlayersIndexByName(players, name) {
    for (var i = 0; i < players.length; i++)
        if (players[i][Constants.PlayersNameIndex].toLowerCase() === name.toLowerCase())
            return i;
}

class Game {
    constructor(winner, loser, wasStalemate) {
        this.WinnersName = winner;
        this.LosersName = loser;
        this.WasStalemate = wasStalemate;
    }
}