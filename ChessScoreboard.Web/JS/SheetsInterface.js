const API_KEY = 'AIzaSyCdkEsPqNU-Xc_7CwQp-kM1hmq_F7rRhgw';
const SPREADSHEET_ID = '1EcjGl22n3CaxF9x0BJzPaNBxi8C3C-uMqWsxtmkkFTs';
const CLIENT_ID = '265091334368-ur3uolm45a8rg391kmuq212neieivkq5.apps.googleusercontent.com';

const SCOPES = "https://www.googleapis.com/auth/spreadsheets";
const DISCOVERY_DOCS = ["https://sheets.googleapis.com/$discovery/rest?version=v4"];

var userIsSignedIn = false;

function connectToSheet(callback) {
  gapi.load('client:auth2', function () {
    var initRequest = {
      apiKey: API_KEY,
      clientId: CLIENT_ID,
      discoveryDocs: DISCOVERY_DOCS,
      scope: SCOPES
    };

    gapi.client.init(initRequest).then(function () {
      // Listen for sign-in state changes.
      gapi.auth2.getAuthInstance().isSignedIn.listen(updateSignInStatus);

      // Handle the initial sign-in state.
      updateSignInStatus(gapi.auth2.getAuthInstance().isSignedIn.get());

      if (!userIsSignedIn)
        gapi.auth2.getAuthInstance().signIn();

      callback();
    }, console.error);
  });
}

function updateSignInStatus(isSignedIn) {
  userIsSignedIn = isSignedIn
}

function appendPre(message) {
  var pre = document.getElementById('content');
  var textContent = document.createTextNode(message + '\n');
  pre.appendChild(textContent);
}

function get(request, successCallback) {
  gapi.client.sheets.spreadsheets.values.get(request).then(successCallback, console.error);
}

var games;

function refreshGames() {
  var getGamesRequest = {
    spreadsheetId: SPREADSHEET_ID,
    range: 'Games!$A$2:$D$201',
  }

  get(getGamesRequest, function (response) {
    games = response.result.values;
    displayGames();
  });
}

function displayGames() {
  //TODO Inform user there was no data
  if (!games || games.length == 0)
    return;

  appendHeaderRow(['Rank', 'Winner', 'Loser', 'Was Stalemate'], '#Games thead');

  var game;
  for (i = 1; i < games.length; i++) {
    game = games[i];

    if (game[0] != '')
      appendRow(game, '#Games tbody');
  }
}

function appendHeaderRow(headerRow, tHeadSelector) {
  var ths = '';
  for (var i = 0; i < headerRow.length; i++)
    ths += `<th>${headerRow[i]}</th>`;

  appendTr(ths, tHeadSelector);
}

function appendRow(row, tbodySelector) {
  console.log(row);

  var tds = '';
  for (var j = 0; j < row.length; j++)
    tds += `<td>${row[j]}</td>`;

  appendTr(tds, tbodySelector);
}

function appendTr(content, selector) {
  $(selector).append(`<tr>${content}</tr>`);
}