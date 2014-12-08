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
        var repository = splitPath[2];
        searchRepository(username, repository, query);
    }

    return false;
});


function searchRepository(username, repository, query) {

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
        success: handleSearch
    });
}

function handleSearch(results) {
    $("#tree-view").hide();
    $("#search-results").show();

    if (results.length == 0) {
        $("#no-results").show();
    }
    else {
        $("#no-results").hide();
        var htmlResults = buildResults(results);
    }
}

function buildResults(results) {
    for (var i = 0; i < results.length; i++) {
        var searchResult = results[i];
        console.log(searchResult);
    }
}


function searchSite(query) {
}

