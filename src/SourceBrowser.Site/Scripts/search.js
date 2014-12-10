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
        if (query == "")
        {
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
        }
        search.isSearching = false;
        search.checkIfSearchTextChanged();
    },

    buildResults: function(results) {
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
    checkIfSearchTextChanged : function() {
        var currentQuery = $("#search-box").val();
        if (currentQuery != search.lastQuery)
            search.beginSearch();
    },
    
    clearSearch: function () {
        $("#search-results").hide();
        $("#no-results").hide();
        $("#tree-view").show();
    }

}


$("#search-box").keyup(function () {
    search.beginSearch();
});











