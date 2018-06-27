$(document.body).on("change", "#ddl_categories", function () {
    loadingPopup.Message("Updating...");
});

function AppMoreDetails(appId) {
    loadingPopup.Message("Loading...");
    $("#hf_ViewDetails").val(appId);
    openWSE.CallDoPostBack("hf_ViewDetails", "");
}

function UninstallApp(appId) {
    if (appId != "") {
        var $appHolder = $(".app-item-installer[data-appid='" + appId + "']");
        if ($appHolder.length > 0) {
            var appName = $.trim($appHolder.find(".app-name").html());
            if (appName == "") {
                appName = "this app";
            }

            openWSE.ConfirmWindow("Are you sure you want to uninstall " + appName + "?",
                function () {
                    loadingPopup.Message("Removing...");
                    $("#hf_UninstallApp").val(appId);
                    openWSE.CallDoPostBack("hf_UninstallApp", "");
                }, null);
        }
    }
}

function InstallApp(appId) {
    loadingPopup.Message("Installing...");
    $("#hf_InstallApp").val(appId);
    openWSE.CallDoPostBack("hf_InstallApp", "");
}
