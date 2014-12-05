//TODO: Rename this from treeViewHelper.j
$("#search-form").submit(function (event) {
    var query = $("#search-box").val();
    var path = window.location.pathname;
    //Split path, removing empty entries
    var splitPath = path.split('/');
    splitPath = splitPath.filter(function (v) { return v !== '' });

    if (splitPath.length <= 1) {
        searchSite(query)
    }
    else if (splitPath.length >= 2)
    {
        var username = splitPath[0];
        var repository = splitPath[1];
        searchRepository(username, repository, query);
    }
    console.log(splitPath.length);

    return false;
});


function searchRepository(username, repository, query) {
    console.log("search repo");
    console.log(username);
    console.log(repository);
    console.log(query);
}

function searchSite(query) {
    console.log("search site");
    console.log(query);
}

