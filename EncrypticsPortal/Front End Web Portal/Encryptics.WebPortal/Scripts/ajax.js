var returnUrl;

function ajaxGetSetup(url, continueWith) {
    ajaxGet(url, function (result) {
        if (result.success !== undefined && result.success) {
            continueWith(result.token);
        } else {
            ajaxPostError(result, continueWith);
        }
    }, function (result) {
        ajaxPostError(result, continueWith);
    });
}

function ajaxPostJson(targetUrl, token, data, successCallback, errorCallback, completeCallback) {
    ajaxPost(targetUrl, token, JSON.stringify(data), 'application/json', 'json', successCallback, errorCallback, completeCallback);
}

function ajaxPostForm(targetUrl, token, data, successCallback, errorCallback, completeCallback) {
    $.ajax({
        type: 'POST',
        url: targetUrl,
        data: data,
        headers: {
            'X-XSRF-Token': token
        },
        xhrFields: {
            withCredentials: true
        },
        success: successCallback,
        error: errorCallback,
        statusCode: { 403: sessionTimeout },
        complete: completeCallback
    });
}

function ajaxPost(targetUrl, token, data, contentType, dataType, successCallback, errorCallback, completeCallback) {
    $.ajax({
        type: 'POST',
        url: targetUrl,
        data: data,
        contentType: contentType,
        dataType: dataType,
        accepts: {
            json: 'application/json'
        },
        headers: {
            'X-XSRF-Token': token
        },
        xhrFields: {
            withCredentials: true
        },
        success: successCallback,
        error: errorCallback,
        statusCode: { 403: sessionTimeout },
        complete: completeCallback
    });
}

function ajaxGet(targetUrl, successCallBack, errorCallback) {
    $.ajax({
        type: 'GET',
        url: targetUrl,
        dataType: 'json',
        xhrFields: {
            withCredentials: true
        },
        headers: { 'x-tzo': $.cookie('tzo'), 'x-dst': $.cookie('dst') },
        success: successCallBack,
        error: errorCallback,
        statusCode: { 403: sessionTimeout }
    });
}

function ajaxPostSuccessful(response, successCallback, errorCallback) {
    if (response.success == true) {
        successCallback();
    } else {
        errorCallback(response.errors);
    }
}

function sessionTimeout(errorResponse) {
    if (errorResponse.responseText !== undefined) {
        errorResponse = JSON.parse(errorResponse.responseText);
    }
    if (errorResponse.redirect !== undefined) {
        window.location = errorResponse.redirect;
    }
}

function ajaxPostError(errorResponse, errorCallback) {
    sessionTimeout(errorResponse);
    if (errorCallback !== undefined) {
        if (errorResponse.errors !== undefined) {
            errorCallback(errorResponse.errors);
        } else {
            errorCallback(errorResponse);
        }
    }
}

function ajaxResponseReturned(response, successCallback) {
    if (response.length == null && !response.success && response.errors[0] == "Session expired.") {
        showMessageBox("Session Ended", "Your session has ended. When you click the OK button you will be transferred to a login page where you can resume your session. Click OK to continue.", function () {
            window.location = returnUrl;
        });
    } else {
        successCallback(response);
    }
}

$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

function logErrors(errors) {
    if (errors.length === undefined) {
        console.debug(errors);

        return;
    }
    
    for (var i = 0; i < errors.length; i++) {
        console.debug(errors[i]);
    }
}

//function showErrors(errors) {
//    if (errors.length == 0) {
//        showMessageDialog("Error", errors);
//    } else {
//        showMessageDialog("Errors", errors.join('<br />'));
//    }
//}

//function showMessageDialog(dialogTitle, dialogMessage) {
//    $('#MessageDialog').html(dialogMessage).dialog({
//        title: dialogTitle,
//        width: "345",
//        modal: true,
//        resizable: false,
//        buttons: {
//            "OK": function () { $(this).dialog("close"); }
//        }
//    }).dialog("open");
//}

function getWaitPanel() {
    return $('<div/>').addClass('waiting');
}
