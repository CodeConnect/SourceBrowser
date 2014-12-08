$("#search-form").submit(function (event) {
    var query = $("#search-box").val();
    var path = window.location.pathname;
    //Split path, removing empty entries
    var splitPath = path.split('/');
    splitPath = splitPath.filter(function (v) { return v !== '' });

    if (splitPath.length <= 1) {
        searchSite(query);
    }
    else if (splitPath.length >= 2)
    {
        var username = splitPath[1];
        console.log(username);
        var repository = splitPath[2];
        console.log(repository);
        searchRepository(username, repository, query);
    }
    console.log(splitPath.length);

    return false;
});


function searchRepository(username, repository, query) {
    console.log("search repo");

    data = JSON.stringify({
        "username": username,
        "repository": repository,
        "query": query
    });

    $.ajax({
        type: "POST",
        url: "/Search/Repository/",
        //TODO: If anyone knows a better way to do this, please share it.
        //My Javascript is not strong...
        data: "username=" + username + "&repository=" + repository + "&query="+ query,
        success: results
    });
}

function results(e) {
    alert(e);
}

function searchSite(query) {
    console.log("search site");
    console.log(query);
}

