$(document).ready(function () {
    getFormFromDom().submit(prepareForUpload);
});

function prepareForUpload() {
    getButtonFromDom().prop("disabled", true);
    var errorMessageElement = getErrorMessageFromDom();
    if (errorMessageElement) {
        errorMessageElement.css("visibility", "hidden");
    }
    getMessageDivFromDom().css("visibility", "visible");
}

//
//  DOM access methods
//
function getButtonFromDom() {
    return $(".btn");
}

function getFormFromDom() {
    return $("#upload-form");
}

function getMessageDivFromDom() {
    return $(".upload-message");
}

function getErrorMessageFromDom() {
    return $("#upload-error");
}