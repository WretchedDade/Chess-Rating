function init() {
    connectToSheet(function () {
        if (userIsSignedIn)
            refreshGames();
    });
}