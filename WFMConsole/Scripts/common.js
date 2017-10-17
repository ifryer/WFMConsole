function toUrl(dest) {
    var basePath = location.pathname.substr(1).match(/[a-z_0-1]*/i);
    if (basePath == "")
        return "/" + dest
    basePath = "/" + basePath + "/";

    return basePath + dest;
}

function startLoading() {
    $(".loading-div, .loading-div-back").show();
}
function stopLoading() {
    $(".loading-div, .loading-div-back").hide();
}

function showSmallAlert(alertText) {
    $(".small-alert-box").removeClass("error-msg").addClass("success-msg")
    $(".small-alert-box").html(alertText).fadeIn();
    if (alertText.length > 100) {
        setTimeout(
            function () { $(".small-alert-box").fadeOut(); }, 5 * alertText.length
        );
    }
    else {
        setTimeout(
            function () { $(".small-alert-box").fadeOut(); }, 3000
        );
    }

}

function showSmallError(alertText) {
    $(".small-alert-box").removeClass("success-msg").addClass("error-msg")
    $(".small-alert-box").html(alertText).fadeIn();
    if (alertText.length > 100) {
        setTimeout(
            function () { $(".small-alert-box").fadeOut(); }, 5 * alertText.length
        );
    }
    else {
        setTimeout(
            function () { $(".small-alert-box").fadeOut(); }, 3000
        );
    }

}