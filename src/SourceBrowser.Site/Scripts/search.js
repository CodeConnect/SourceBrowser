$(document).ready(function () {
    var path = window.location.pathname;
    var splitPath = path.split('/');
    splitPath = splitPath.filter(function (v) { return v !== '' });
    if (splitPath.length <= 2)
    {
        $("#search-form").hide();
    }
});

search = {
    //Keeps track of whether a search is ongoing.
    //We only want to send one query at a time.
    isSearching: false,
    //Retains the state of the most recent query. 
    //When we're done searching, we check the text box to see 
    //if the query has changed.
    lastQuery: "",
    beginSearch: function () {
        if (search.isSearching)
            return;
        var query = $("#search-box").val();
        //If there's no query, just clear everything
        if (query == "") {
            search.clearSearch();
            return;
        }
        search.lastQuery = query;
        //Split path, removing empty entries
        var path = window.location.pathname;
        var splitPath = path.split('/');
        splitPath = splitPath.filter(function (v) { return v !== '' });

        //Only search if we're in a repository
        if (splitPath.length > 2) {
            var username = splitPath[1];
            var repository = splitPath[2];

            search.searchRepository(username, repository, query);
        }

        return false;
    },


    searchRepository: function (username, repository, query) {
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
            data: "username=" + username + "&repository=" + repository + "&query=" + query,
            success: search.handleSuccess,
            error: search.handleError
        });
    },

    handleError: function (e) {
        search.isSearching = false;
        search.checkIfSearchTextChanged();
    },

    handleSuccess: function (results) {
        $("#tree-view").hide();
        //If we can't find any results, tell the user.
        if (results.length == 0) {
            $("#search-results").hide();
            $("#no-results").show();
        }
        else {
            $("#no-results").hide();
            $("#search-results").show();
            var htmlResults = search.buildResults(results);
            $("#search-results").empty().append(htmlResults);
            $("#search-results a").click(search.handlSearchClick);
        }
        search.isSearching = false;
        search.checkIfSearchTextChanged();
    },

    buildResults: function (results) {
        var htmlResults = "";
        for (var i = 0; i < results.length; i++) {
            var searchResult = results[i];
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
    },

    //After searching, we need to check to see if the user has typed
    //any additional characters into search while we were waiting
    //for the server
    checkIfSearchTextChanged: function () {
        var currentQuery = $("#search-box").val();
        if (currentQuery != search.lastQuery)
            search.beginSearch();
    },

    clearSearch: function () {
        $("#search-results").hide();
        $("#no-results").hide();
        $("#tree-view").show();
    },

    handlSearchClick: function (ex) {
        //If we're already on the page, allow the browser to scroll.
        var currentPath= window.location.pathname;
        var newPath = ex.currentTarget.pathname

        if (currentPath== newPath)
            return;

        //Stop navigation. We'll take it from here.
        ex.preventDefault();

        var url = ex.currentTarget.href;
        var host = ex.currentTarget.host;

        var splitByHash = url.split('#');
        if(splitByHash.length > 2)
            throw "Too many #'s in path";

        var newUrl = splitByHash[0];
        var lineNumber = Number(splitByHash[1]);


        window.History.pushState({ lineNumber: lineNumber }, null, newUrl);
    }
}

// Bind to StateChange Event
$(document).ready(function () {
    window.History.Adapter.bind(window, 'statechange', handleStateChange);
});

function handleStateChange(e) {
    //TODOa
    var state = History.getState();
    var cleanUrl = state.url;
    var data = state.data;
    lineNumber = data["lineNumber"];
    console.log(lineNumber);
    console.log(cleanUrl);


    $.ajax({
        type: "GET",
        url: cleanUrl,
        success: handlePageLoadSuccess,
        error: handlePageLoadError
    });
}

function handlePageLoadSuccess(args) {
    $(".source-code").html(args["SourceCode"]);
    var numberOfLines = args["NumberOfLines"];
    var lineNumberHtml = "";
    for(var i = 1; i < numberOfLines; i++)
    {
        lineNumberHtml += '<a href="' + i + '" name="' + i + '">' + i + '</a>\n';
    }
    $("#line-numbers").html(lineNumberHtml);
}

function handlePageLoadError(args) {
    console.log(args);
}




$("#search-box").keyup(function () {
    search.beginSearch();
});











