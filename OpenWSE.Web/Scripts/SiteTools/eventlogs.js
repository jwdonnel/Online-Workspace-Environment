function OpenLogFolderModal() {
    loadingPopup.Message('Clearing. Please Wait...');
    $("#hf_logfolder").val(new Date().toString());
    openWSE.CallDoPostBack("hf_logfolder", "");
}

function OnFileClick(file) {
    loadingPopup.Message("Loading File...");
    $("#hf_FileContent").val(file);
    openWSE.CallDoPostBack("hf_FileContent", "");
}

function FinishFileLoad(fileId) {
    openWSE.LoadModalWindow(false, "logFolder-element", "");
    openWSE.LoadModalWindow(true, "logFile-element", fileId);
    loadingPopup.RemoveMessage();
}

function DeleteFile(file) {
    loadingPopup.Message("Deleting File...");
    $("#hf_DeleteFile").val(file);
    openWSE.CallDoPostBack("hf_DeleteFile", "");
}

function ConfirmClearLogAll(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear all errors?",
        function () {
            loadingPopup.Message('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

function ConfirmClearLogFolder(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete all error log files in the Logging folder?",
        function () {
            loadingPopup.Message('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

function ConfirmClearAllIgnored(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear the ignored errors?",
        function () {
            loadingPopup.Message('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

$(document.body).on("change", "#cb_ViewErrorsOnly, #cb_ViewMoreDetails", function () {
    loadingPopup.Message('Updating. Please Wait...');
});

function ResetHitCount(id) {
    openWSE.ConfirmWindow("Are you sure you want to reset the hit count?",
        function () {
            loadingPopup.Message("Resetting...");
            $("#MainContent_hf_resetHitCount").val(id);
            openWSE.CallDoPostBack("MainContent_hf_resetHitCount", "");
        }, null);
}

function IgnoreError(id) {
    openWSE.ConfirmWindow("Are you sure you want to ignore this event?",
        function () {
            loadingPopup.Message("Updating Event...");
            $("#MainContent_hf_updateIgnore").val(id);
            openWSE.CallDoPostBack("MainContent_hf_updateIgnore", "");
        }, null);
}

function RefreshPageOnError(id) {
    $("#MainContent_hf_updateRefreshOnError").val(id);
    openWSE.CallDoPostBack("MainContent_hf_updateRefreshOnError", "");
}

function AllowError(id) {
    openWSE.ConfirmWindow("Are you sure you want to allow this event?",
        function () {
            loadingPopup.Message("Updating Event...");
            $("#MainContent_hf_updateAllow").val(id);
            openWSE.CallDoPostBack("MainContent_hf_updateAllow", "");
        }, null);
}

function DeleteEvent(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this event?",
        function () {
            loadingPopup.Message("Deleting Event...");
            $("#MainContent_hf_deleteError").val(id);
            openWSE.CallDoPostBack("MainContent_hf_deleteError", "");
        }, null);
}
