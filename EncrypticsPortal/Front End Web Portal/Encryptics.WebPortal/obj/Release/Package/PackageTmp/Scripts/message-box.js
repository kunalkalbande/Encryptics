function showMessageBox(dialogTitle, messageHtml, continueFunction) {
    $('#messageBoxDialog').html(messageHtml).dialog({
        title: dialogTitle,
        width: "345",
        modal: true,
        dialogClass: "alert",
        resizable: false,
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                if (continueFunction != null) continueFunction();
            }
        }
    }).dialog("open");
}

function showErrorMessage(dialogTitle, messageText) {
    var alertMessage = '<p><span class="ui-icon ui-icon-alert" style="float:left; margin:0 7px 20px 0;"></span>' + messageText + '</p>';
    showMessageBox(dialogTitle, alertMessage);
}