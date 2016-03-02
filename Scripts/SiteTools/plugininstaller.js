function UninstallPlugin(id, name) {
    if (id != "") {
        if (name == "") {
            name = "this plugin";
        }

        openWSE.ConfirmWindow("Are you sure you want to uninstall " + name + "?",
            function () {
                openWSE.LoadingMessage1("Uninstalling...");
                $("#hf_UninstallPlugin").val(id);
                __doPostBack("hf_UninstallPlugin", "");
            }, null);
    }
}

function InstallPlugin(id) {
    openWSE.LoadingMessage1("Installing...");
    $("#hf_InstallPlugin").val(id);
    __doPostBack("hf_InstallPlugin", "");
}
