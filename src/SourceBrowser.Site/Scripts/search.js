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

    if (results.length == 0) {
        $("#search-results").hide();
        $("#no-results").show();
    }
    else {
        $("#no-results").hide();
        $("#search-results").show();
        var htmlResults = buildResults(results);
        $("#search-results").empty().append(htmlResults);
    }
}

function buildResults(results) {
    var htmlResults = "";
    for (var i = 0; i < results.length; i++) {
        var searchResult = results[i];
        console.log(searchResult);
        var lineNumber = searchResult["LineNumber"];
        var link = "/Browse/" + searchResult["Path"] + "#" + lineNumber;
        html = "";
        html += "<a href='" + link + "'>";
        html += "<span>"
        html += searchResult["Name"];
        html += "</span>";
        html += "<span>"
        html += searchResult["FullName"];
        html += "</span>";
        html += "</a>";

        htmlResults += html;
    }
    return htmlResults;
}


function searchSite(query) {
}

