var Utilities = (function() {

    function sendJsonData(url, data, successCallback, callText, failureCallback, alwaysCallback, crossDomain) {
        return serverMessage("POST", url, data, successCallback, callText, failureCallback, alwaysCallback, crossDomain, true);
    }

    function getJsonData(url, data, successCallback, callText, failureCallback, alwaysCallback, crossDomain) {
        return serverMessage("GET", url, data, successCallback, callText, failureCallback, alwaysCallback, crossDomain, false);
    }

    function serverMessage(method, url, data, successCallback, callText, failureCallback, alwaysCallback, crossDomain, jsonData) {
        var options = {
            type: method,
            dataType: "json",
            accepts: "application/json",
        };
        options["url"] = (url.substring(0, 4).toLowerCase() == "http" ? url : toUrl(url));

        if (data) options["data"] = data;
        if (jsonData) options["contentType"] = "application/json";
        if (crossDomain) {
            options["crossDomain"] = true;
            options["xhrFields"] = { withCredentials: true };
        }

        return $.ajax(options)
            .done(function(json) {
                // for the misbehavious ones that don't follow the convention
                // make sure it's json
                if (typeof json === "string") {
                    json = JSON.parse(json);
                }
                // now check if there is a success field.
                if ("success" in json) {
                    if (json.success) {
                        if ("msg" in json && json.msg.length > 0) {
                            toastr.success(json.msg);
                        }
                        if (successCallback) {
                            successCallback(json);
                        }
                    } else {
                        if ("msg" in json) {
                            toastr.error(json.msg);
                        } else if ("message" in json) {
                            toastr.error(json.message);
                        }
                        if (failureCallback) {
                            failureCallback(json);
                        }
                    }
                } else if (successCallback) {
                    successCallback(json);
                }
            })
            .fail(function(jqXHR, textStatus, errorThrown) {
                toastr.error("Error " + callText + " - " + errorThrown);
            })
            .always(function() {
                if (alwaysCallback) {
                    alwaysCallback();
                }
            });
    }
    return {
        GetJsonData: getJsonData,
        SendJsonData: sendJsonData
    }
})();