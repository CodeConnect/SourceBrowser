$(document).ready(function () {
    var path = window.location.pathname;
    var splitPath = path.split('/');
    splitPath = splitPath.filter(function (v) { return v !== '' });
    if (splitPath.length > 2)
    {
        //Only bind if the page is searchable. (ie. we're in a repository)
        window.History.Adapter.bind(window, 'statechange', handleStateChange);
    }

    // Drag-to-resize search results pane
    // Adapted from http://www.catchmyfame.com/2010/08/12/adjustable-columns-with-jquery/

    var stopTwoLeft = parseInt($('#main-content').offset().left) + 300;
    var stopTwoRight = parseInt($('#browser').offset().left) + $('#browser').width() - 200;

    $("#drag-handle").draggable({
        axis: 'x',
        start: function (event, ui) {
            leftOneStart = $('#main-content').width();
            leftThreeStart = $('#browser').width();
        },
        drag: function (event, ui) {
            $('#main-content').width(leftOneStart + (ui.position.left - ui.originalPosition.left));
            $('#browser').width(leftThreeStart - (ui.position.left - ui.originalPosition.left));
        },
        containment: [stopTwoLeft, 0, stopTwoRight, 0]
    });
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
        if (query === "") {
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
        if (results.length === 0) {
            $("#search-results").hide();
            $("#no-results").show();
        }
        else {
            $("#no-results").hide();
            $("#search-results").show();
            var htmlResults = search.buildResults(results);
            $("#search-results").empty().append(htmlResults);
            $("#search-results a").click(search.handleSearchClick);
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
            html += searchResult["DisplayName"];
            html += "</span>";
            html += "<span>"
            html += searchResult["FullyQualifiedName"];
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

    handleSearchClick: function (ex) {
        //If we're already on the page, allow the browser to scroll.
        var currentPath= window.location.pathname;
        var newPath = ex.currentTarget.pathname

        if (currentPath === newPath)
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

        var splitBySlash = newUrl.split('/');
        var fileName = splitBySlash[splitBySlash.length - 1];

        window.History.pushState({ lineNumber: lineNumber }, null, newUrl);
        document.title = fileName + " | Source Browser";
    }
}

function handleStateChange(e) {
    var state = History.getState();
    var cleanUrl = state.url;
    var data = state.data;
    lineNumber = data["lineNumber"];

    $.ajax({
        type: "GET",
        url: cleanUrl,
        success: function (args) {
            $("#main-content").html(args["SourceCode"]);
             var newUrl = cleanUrl + "#" + lineNumber;
             window.history.replaceState(null, null, newUrl);
             window.location = newUrl;
        },
        error: handlePageLoadError
    });
}

function handlePageLoadError(args) {
    var errorHtml = "<h2>There was an error processing your request.</h2>";
    $("#line-numbers").empty();
    $(".source-code").html(errorHtml);
}

$("#search-box").keyup(function () {
    search.beginSearch();
});

$("#search-button").click(function () {
    search.beginSearch();
});

