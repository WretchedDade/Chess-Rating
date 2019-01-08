const API_KEY = 'AIzaSyCdkEsPqNU-Xc_7CwQp-kM1hmq_F7rRhgw';
const SPREADSHEET_ID = '1EcjGl22n3CaxF9x0BJzPaNBxi8C3C-uMqWsxtmkkFTs';
const CLIENT_ID = '265091334368-ur3uolm45a8rg391kmuq212neieivkq5.apps.googleusercontent.com';
const APP_SCRIPT_URL = 'https://script.google.com/macros/s/AKfycbwmHZ1RfKVmOlUg6tbMHVQkTMWA-I-guaQm-U1dNiq6-7eisjg/exec';

var GamesTable;
var PlayersTable;
var AddGamePlayersTable;
var ChessScoreboardSheet;

function init() {
    var ChessScoreboardSheet = new Sheet(API_KEY, SPREADSHEET_ID, CLIENT_ID, APP_SCRIPT_URL);
    ChessScoreboardSheet.Connect(function () {
        if (ChessScoreboardSheet.UserIsSignedIn) {
            GamesTable = new SheetsTable('Games!$A$2:$D$201', ChessScoreboardSheet, '#Games', ['Rank', 'Winner', 'Loser', 'Was Stalemate'], GamesTableRefreshCallback);
            PlayersTable = new SheetsTable('Players!$A$1:$G$201', ChessScoreboardSheet, '#Players');
            AddGamePlayersTable = new SheetsTable('Players!$A$2:$B$201', ChessScoreboardSheet, '#AddGamesPlayers', ['#', 'Name']);
        }
    })
}

function GamesTableRefreshCallback() {
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

function addGameModal(event) {
    event.preventDefault();

    $('#AddGameModal').modal('show');

    $("input").attr("max", AddGamePlayersTable.Values.length);
}

function addGame(event) {
    event.preventDefault();

    var winnerNumber = parseInt($('#WinnerNumber').val());
    var loserNumber = parseInt($('#LoserNumber').val());
    var wasStalemate = $('#WasStalemate').prop('checked');

    var winner = AddGamePlayersTable.Values[winnerNumber - 1][1];
    var loser = AddGamePlayersTable.Values[loserNumber - 1][1];

    var values = [GamesTable.Values.length + 1, winner, loser, wasStalemate]

    GamesTable.AddRow(values);

    $('#AddGameModal').modal('hide');
}

function toggleUpDownCaret(anchor){
    var icon;
    $(anchor).children('i').each(function(){
        icon = $(this);
        icon.toggleClass('d-none');
        icon.toggleClass('d-block');
    });
}