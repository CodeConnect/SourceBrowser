function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

$(function () {
    $("#browserTree").click(
        function (e) {
            var target = e.originalEvent.target.href;
            if (!(endsWith(target, "#"))) {
                window.location.href = target;
            }
        }
    );
});

$(document).ready(function () {
    var path = window.location.pathname;
    expandTreeView(path);
});

function expandTreeView(path) {
    var parts = path.split("/");
    var pathToDate = "";

    // Ignore the first three elements from the path:
    // Browse/UserName/RepoName/Folder1/Folder2/FileName
    for (var partNumber = 4; partNumber < parts.length; partNumber++)
    {
        pathToDate = pathToDate + "/" + parts[partNumber];
        expandNode(pathToDate);
    }
}

function expandNode(path) {
    var selector = "li[id='" + path + "']";
    var node = $(selector);
    var child = $(selector + " > ul");
    if (node.length) {
        node.removeClass("collapsed");
    }
    if (child.length) {
        child.show();
    }
}
