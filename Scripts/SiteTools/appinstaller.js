﻿$(document.body).on("change", "#ddl_categories", function () {
    GetAppsInCategory();
});

function GetAppsInCategory() {
    var selected = $("#ddl_categories").val();
    $("#noItemsCategory").hide();

    if (selected == "") {
        $(".app-item-installer").show();
    }
    else {
        var found = 0;
        $(".app-item-installer").each(function () {
            var categoryId = $(this).attr("data-category").split(';');
            if (categoryId.indexOf(selected) == -1) {
                $(this).hide();
            }
            else {
                $(this).show();
                found++;
            }
        });

        if (found == 0) {
            $("#noItemsCategory").show();
        }
    }
}

function AppMoreDetails(appId) {
    openWSE.LoadingMessage1("Loading...");
    $("#hf_ViewDetails").val(appId);
    __doPostBack("hf_ViewDetails", "");
}

function UninstallApp(appId) {
    openWSE.LoadingMessage1("Removing...");
    $("#hf_UninstallApp").val(appId);
    __doPostBack("hf_UninstallApp", "");
}

function InstallApp(appId) {
    openWSE.LoadingMessage1("Installing...");
    $("#hf_InstallApp").val(appId);
    __doPostBack("hf_InstallApp", "");
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    GetAppsInCategory();
});