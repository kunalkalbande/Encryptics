function showConfirmationDialog(dialogTitle, confirmationText, confirmFunction) {
    $('#confirmationDialog').text(confirmationText).dialog({
        title: dialogTitle,
        width: "345",
        modal: true,
        resizable: false,
        buttons: {
            "Yes": function() {
                confirmFunction();
                $(this).dialog("close");
            },
            "No": function () { $(this).dialog("close"); }
        }
    }).dialog("open");
}