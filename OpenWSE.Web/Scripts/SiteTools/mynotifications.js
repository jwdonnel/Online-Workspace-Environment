function deleteNotification(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this notification?", function () {
        loadingPopup.Message("Deleting. Please Wait...");
        $("#hf_deleteNotification").val(id);
        openWSE.CallDoPostBack("hf_deleteNotification", "");
    });
}

function deleteAllNotifications() {
    openWSE.ConfirmWindow("Are you sure you want to delete all notifications?", function () {
        loadingPopup.Message("Deleting. Please Wait...");
        $("#hf_deleteAllNotification").val(new Date().toString());
        openWSE.CallDoPostBack("hf_deleteAllNotification", "");
    });
}
