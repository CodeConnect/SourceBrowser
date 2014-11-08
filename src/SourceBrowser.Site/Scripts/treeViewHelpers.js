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