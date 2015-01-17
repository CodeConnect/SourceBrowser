$(document).ready(function () {
    resetUI();
    registerButtonClickEvent();
});

function registerButtonClickEvent() {
    getButtonFromDom().click(prepareForChange);
}

function prepareForChange() {
    getButtonFromDom().prop("disabled", true);
    getMessageDivFromDom().css("visibility", "visible");
}

function resetUI() {
    getButtonFromDom().prop("disabled", false);
    getMessageDivFromDom().css("visibility", "hidden");
}

/*
*  DOM access methods
*/
function getButtonFromDom() {
    return $(".btn");
}

function getMessageDivFromDom() {
    return $("#uploadMessageDiv");
}