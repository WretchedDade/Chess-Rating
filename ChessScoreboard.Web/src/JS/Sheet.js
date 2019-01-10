class Sheet {
  constructor(apiKey, spreadsheetID, clientID, appScriptUrl) {
    this.ApiKey = apiKey;
    this.ClientID = clientID;
    this.SpreadsheetID = spreadsheetID;
    this.AppScriptUrl = appScriptUrl;
    this.Scopes = "https://www.googleapis.com/auth/spreadsheets";
    this.DiscoveryDocs = ["https://sheets.googleapis.com/$discovery/rest?version=v4"];

    this.UserIsSignedIn = false;
  }

  Connect(callback) {
    var parent = this;
    gapi.load('client:auth2', function () {
      var initRequest = {
        apiKey: parent.ApiKey,
        clientId: parent.ClientID,
        discoveryDocs: parent.DiscoveryDocs,
        scope: parent.Scopes
      };

      gapi.client.init(initRequest).then(function () {
        // Listen for sign-in state changes.
        gapi.auth2.getAuthInstance().isSignedIn.listen(parent.UpdateSignInStatus);

        // Handle the initial sign-in state.
        parent.UpdateSignInStatus(gapi.auth2.getAuthInstance().isSignedIn.get());

        if (parent.UserNeedsToSignIn)
          gapi.auth2.getAuthInstance().signIn();

        callback();
      }, console.error);
    });
  }

  get UserNeedsToSignIn() {
    return !this.UserIsSignedIn;
  }

  UpdateSignInStatus(isSignedIn) {
    this.UserIsSignedIn = isSignedIn;
  }

  Get(request) {
    return new Promise((resolve, reject) => {
      gapi.client.sheets.spreadsheets.values.get(request).then(resolve, reject);
    });
  }

  Put(range, valueRangeBody) {
    var params = {
      spreadsheetId: this.SpreadsheetID,
      range: range,
      valueInputOption: 'USER_ENTERED'
    }

    return gapi.client.sheets.spreadsheets.values.update(params, valueRangeBody);
  }

  GetAppScript() {
    $.ajax({
      type: 'GET',
      url: this.AppScriptUrl
    });
  }
}