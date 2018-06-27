function UninstallPlugin(id, name) {
    if (id != "") {
        if (name == "") {
            name = "this plugin";
        }

        openWSE.ConfirmWindow("Are you sure you want to uninstall " + name + "?",
            function () {
                loadingPopup.Message("Uninstalling...");
                $("#hf_UninstallPlugin").val(id);
                openWSE.CallDoPostBack("hf_UninstallPlugin", "");
            }, null);
    }
}

function InstallPlugin(id) {
    loadingPopup.Message("Installing...");
    $("#hf_InstallPlugin").val(id);
    openWSE.CallDoPostBack("hf_InstallPlugin", "");
}
