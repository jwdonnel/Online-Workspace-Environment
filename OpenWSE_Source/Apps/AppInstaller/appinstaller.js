var currCategory = "";
var arrBulkInstall = new Array();

$(document).ready(function () {
    StartAppInstaller();
});

function LoadingMessage_AppInstaller(message) {
    var x = "<div class='update-element-appinstaller'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#appinstaller-load").append(x);
    $(".update-element-appinstaller").show();
}

$(document.body).on("click", "#app-appinstaller .exit-button-app, #app-appinstaller-min-bar .exit-button-app-min", function () {
    cookie.del("category-AppInstaller");
    cookie.del("category-AppInstaller-id");
    cookie.del("search-AppInstaller");
    cookie.del("tab-AppInstaller");
});

function RemoveLoadingMessage_AppInstaller() {
    $(".update-element-appinstaller").each(function () {
        $(this).remove();
    });
}

function StartAppInstaller() {
    $("#tb_search_AppInstaller").autocomplete({
        minLength: 1,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetAppSearchList_Installer",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        };
                    }));
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    GetCategories_AppInstaller();
}

$(document.body).on("keydown", "#tb_search_AppInstaller", function (event) {
    try {
        if (event.which == 13) {
            Search_appinstaller();
            return false;
        }
    } catch (evt) {
        if (event.keyCode == 13) {
            Search_appinstaller();
            return false;
        }
        delete evt;
    }
});

function Search_appinstaller() {
    var searchField = $("#tb_search_AppInstaller").val();
    if ((searchField != "") && (searchField != "Search Apps/Plugins")) {
        cookie.set("search-AppInstaller", searchField, "30");
    }
    else {
        cookie.del("search-AppInstaller");
    }
    GetApps_AppInstaller();
    if ($("#hdl2_plugins").hasClass("active")) {
        LoadPlugins();
    }
}

function SetCategory(_this, category) {
    currCategory = category;
    try {
        $("#appinstaller-load").find(".tsactive").each(function () {
            $(this).removeClass("tsactive");
        });
        $(_this).addClass("tsactive");
        cookie.set("category-AppInstaller", $(_this).find("h4").html(), "30");
        cookie.set("category-AppInstaller-id", currCategory, "30");
    }
    catch (evt) { }
    if (currCategory.toLowerCase() == "all") {
        var searchField = cookie.get('search-AppInstaller');
        if ((searchField != null) && (searchField != "") && (searchField != undefined)) {
            $("#tb_search_AppInstaller").val(searchField);
            GetApps_AppInstaller();
        }
        else {
            $("#apps_holder").hide();
            $("#all_apps_holder").show();

            $("#hyp_selectedAll_appinstaller").show();
            $("#hyp_deselectAll_appinstaller").hide();
            if ($("#all_apps_holder").find(".cb-appinstaller").length == 0) {
                $("#hyp_selectedAll_appinstaller").hide();
                $("#hyp_deselectAll_appinstaller").hide();
            }

            if (arrBulkInstall.length > 0) {
                $("#hyp_installSelected_appinstaller").show();
                for (var i = 0; i < arrBulkInstall.length; i++) {
                    var id = ".cb_" + arrBulkInstall[i];
                    if ($(id).is(":checked")) {
                        $("#hyp_selectedAll_appinstaller").hide();
                        $("#hyp_deselectAll_appinstaller").show();
                    }
                }
            }
        }
    }
    else {
        GetApps_AppInstaller();
    }
}

function GetCategories_AppInstaller() {
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/GetCategories",
        data: "{ }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                $("#app-category-holder").html(data.d);
                var id1 = cookie.get('category-AppInstaller');
                var searchField = cookie.get('search-AppInstaller');
                if ((id1 != null) && (id1 != "") && (id1 != undefined)) {
                    if (id1 != "All Apps") {
                        $('#plugins-holder').hide();
                        $('#all_apps_holder').hide();
                        $('#apps_holder').show();
                        var found = false;
                        $("#app-category-holder").find(".tsdiv").each(function () {
                            if (!found) {
                                $(this).removeClass("tsactive");
                                var c = $(this).find("h4").html();
                                if (id1.toLowerCase() == c.toLowerCase()) {
                                    found = true;
                                    $(this).addClass("tsactive");
                                }
                            }
                        });

                        if ((searchField != null) && (searchField != "") && (searchField != undefined)) {
                            $("#tb_search_AppInstaller").val(searchField);
                        }
                        currCategory = cookie.get('category-AppInstaller-id');
                        GetApps_AppInstaller();
                    }
                    else {
                        if ((searchField != null) && (searchField != "") && (searchField != undefined)) {
                            $("#tb_search_AppInstaller").val(searchField);
                            GetApps_AppInstaller();
                        }
                    }
                }
                else {
                    if ((searchField != null) && (searchField != "") && (searchField != undefined)) {
                        $("#tb_search_AppInstaller").val(searchField);
                        GetApps_AppInstaller();
                    }
                }

                $("#hyp_selectedAll_appinstaller").show();
                if (($("#apps_holder").find(".cb-appinstaller").length == 0) && ($("#apps_holder").css("display") == "block")) {
                    $("#hyp_selectedAll_appinstaller").hide();
                    $("#hyp_deselectAll_appinstaller").hide();
                }
                else if (($("#all_apps_holder").find(".cb-appinstaller").length == 0) && ($("#all_apps_holder").css("display") == "block")) {
                    $("#hyp_selectedAll_appinstaller").hide();
                    $("#hyp_deselectAll_appinstaller").hide();
                }

                var tab = cookie.get("tab-AppInstaller");
                if (tab == "plugins") {
                    PluginsTab_AppInstaller();
                }
            }
        }
    });
}

function GetApps_AppInstaller() {
    LoadingMessage_AppInstaller("Loading App List...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/GetApps",
        data: "{ 'category':'" + currCategory + "','search':'" + $("#tb_search_AppInstaller").val() + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var tab = cookie.get("tab-AppInstaller");
            if (tab != "plugins") {
                $("#all_apps_holder").hide();
                $("#apps_holder").show();
            }
            if (data.d != "") {
                $("#apps_holder").html(data.d);

                if (arrBulkInstall.length > 0) {
                    for (var i = 0; i < arrBulkInstall.length; i++) {
                        var id = ".cb_" + arrBulkInstall[i];
                        $(id).each(function () {
                            $(id).prop("checked", true);
                        });
                    }

                    $("#hyp_installSelected_appinstaller").show();
                    $("#hyp_deselectAll_appinstaller").hide();
                    var count = 0;
                    if ($("#apps_holder").css("display") == "block") {
                        $("#apps_holder").find(".cb-appinstaller").each(function () {
                            if ($(this).is(":checked")) {
                                count++;
                            }
                        });
                    }

                    if (count > 0) {
                        $("#hyp_selectedAll_appinstaller").hide();
                        $("#hyp_deselectAll_appinstaller").show();
                    }
                    else {
                        $("#hyp_selectedAll_appinstaller").show();
                        $("#hyp_deselectAll_appinstaller").hide();
                    }
                }
                else {
                    $("#hyp_installSelected_appinstaller").hide();
                    $("#hyp_selectedAll_appinstaller").show();
                    $("#hyp_deselectAll_appinstaller").hide();
                }

                if ($("#apps_holder").find(".cb-appinstaller").length == 0) {
                    $("#hyp_selectedAll_appinstaller").hide();
                    $("#hyp_deselectAll_appinstaller").hide();
                }
            }
            else {
                $("#apps_holder").html("<div class='clear-space'></div><h4 class='pad-left pad-top-big'>No Apps Available</h4>");
            }

            RemoveLoadingMessage_AppInstaller();
        },
        error: function () {
            RemoveLoadingMessage_AppInstaller();
        }
    });
}

function CheckBoxSelect_AppInstaller(_this, id) {
    var tempArray = new Array();
    var index1 = 0;
    for (var i = 0; i < arrBulkInstall.length; i++) {
        if ((arrBulkInstall[i] != id) && (arrBulkInstall[i] != undefined) && (arrBulkInstall[i] != null) && (arrBulkInstall[i] != "")) {
            tempArray[index1] = arrBulkInstall[i];
            index1++;
        }
    }

    var c = "." + $(_this).attr("id");
    if ($(_this).is(":checked")) {
        tempArray[tempArray.length] = id;
        $("#all_apps_holder").find(c).prop("checked", true);
    }
    else {
        $("#all_apps_holder").find(c).prop("checked", false);
    }

    arrBulkInstall = new Array();
    for (var i = 0; i < tempArray.length; i++) {
        arrBulkInstall[i] = tempArray[i];
    }

    if (arrBulkInstall.length > 0) {
        $("#hyp_installSelected_appinstaller").show();
        var count = 0;
        if ($("#all_apps_holder").css("display") == "block") {
            $("#all_apps_holder").find(".cb-appinstaller").each(function () {
                if ($(this).is(":checked")) {
                    count++;
                }
            });
        }
        else if ($("#apps_holder").css("display") == "block") {
            $("#apps_holder").find(".cb-appinstaller").each(function () {
                if ($(this).is(":checked")) {
                    count++;
                }
            });
        }

        if (count > 0) {
            $("#hyp_selectedAll_appinstaller").hide();
            $("#hyp_deselectAll_appinstaller").show();
        }
        else {
            $("#hyp_selectedAll_appinstaller").show();
            $("#hyp_deselectAll_appinstaller").hide();
        }
    }
    else {
        $("#hyp_installSelected_appinstaller").hide();
        $("#hyp_selectedAll_appinstaller").show();
        $("#hyp_deselectAll_appinstaller").hide();
    }
}

function AddApp(id) {
    openWSE.LoadingMessage1("Installing App. Please Wait...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/AddApp",
        data: "{ 'id':'" + id + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                window.location.href = window.location.href;
            }
            else {
                openWSE.AlertWindow("There was an error installing the requested app(s). Please contact administrator for more assistance");
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.RemoveUpdateModal();
        }
    });
}

function RemoveApp(id) {
    openWSE.LoadingMessage1("Uninstalling App. Please Wait...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/RemoveApp",
        data: "{ 'id':'" + id + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                window.location.href = window.location.href;
            }
            else {
                openWSE.AlertWindow("There was an error uninstalling the requested app. Please contact administrator for more assistance");
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.RemoveUpdateModal();
        }
    });
}

function AboutApp_AppInstaller(name, id) {
    openWSE.LoadingMessage1("Loading About. Please Wait...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/AboutApp",
        data: "{ 'appId':'" + id + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != null) {
                $("#AboutAppdHolder").html(data.d[0]);
                if (data.d.length == 3)
                {
                    openWSE.RatingStyleInit(".app-rater", data.d[1], true, id, true);
                    for (var i = 0; i < data.d[2].length; i++) {
                        var rating = data.d[2][i];
                        openWSE.RatingStyleInit($(".ratingreviews-div").eq(i)[0], rating.Rating, true, rating.AppID, false);
                    }
                }
                openWSE.LoadModalWindow(true, "appinstaller-About-element", "About " + name);
                openWSE.RemoveUpdateModal();
            }
            else {
                openWSE.AlertWindow("There was an error with the request. Could not get information regarding app. Please contact administrator for more assistance");
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.RemoveUpdateModal();
        }
    });
}

function SelectAll_AppInstaller() {
    if ($("#all_apps_holder").css("display") == "block") {
        $("#all_apps_holder").find(".cb-appinstaller").each(function () {
            $(this).prop("checked", true);
            CheckBoxSelect_AppInstaller(this, $(this).attr("id").replace("cb_", ""));
        });
    }
    else if ($("#apps_holder").css("display") == "block") {
        $("#apps_holder").find(".cb-appinstaller").each(function () {
            var c = "." + $(this).attr("id");
            $(c).prop("checked", true);
            CheckBoxSelect_AppInstaller(this, $(this).attr("id").replace("cb_", ""));
        });
    }
}

function DeselectAll_AppInstaller() {
    if ($("#all_apps_holder").css("display") == "block") {
        $("#all_apps_holder").find(".cb-appinstaller").each(function () {
            $(this).prop("checked", false);
            CheckBoxSelect_AppInstaller(this, $(this).attr("id").replace("cb_", ""));
        });
    }
    else if ($("#apps_holder").css("display") == "block") {
        $("#apps_holder").find(".cb-appinstaller").each(function () {
            var c = "." + $(this).attr("id");
            $(c).prop("checked", false);
            CheckBoxSelect_AppInstaller(this, $(this).attr("id").replace("cb_", ""));
        });
    }
}

function InstallSelected_AppInstaller() {
    if (arrBulkInstall.length > 0) {
        openWSE.LoadingMessage1("Installing App(s). Please Wait...");
        $.ajax({
            url: "Apps/AppInstaller/AppInstallerService.asmx/InstallBulk",
            data: JSON.stringify({ "apps": arrBulkInstall }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d != "") {
                    window.location.href = window.location.href;
                }
                else {
                    openWSE.AlertWindow("There was an error installing the requested app(s). Please contact administrator for more assistance");
                    openWSE.RemoveUpdateModal();
                }
            },
            error: function () {
                openWSE.RemoveUpdateModal();
            }
        });
    }
    else {
        openWSE.AlertWindow("There are no selected apps to install.");
    }
}

function AppsTab_AppInstaller() {
    $("#plugin-info-sidebar").hide();
    $('#plugins_holder').hide();
    $("#plugins_holder").html("<h4 class='pad-left pad-top-big'>Loading Plugins...</h4>");
    $("#app-category-sidebar").show();
    if ((currCategory.toLowerCase() == "all") || (currCategory == "")) {
        $("#apps_holder").hide();
        $("#all_apps_holder").show();
    }
    else {
        $('#apps_holder').show();
        $('#all_apps_holder').hide();
    }

    $("#select-btns-appinstaller").show();

    cookie.del("tab-AppInstaller");

    $("#appinstaller-load").find("#hdl2_plugins").removeClass("active");
    $("#appinstaller-load").find("#hdl1_apps").addClass("active");
}

function PluginsTab_AppInstaller() {
    var searchField = cookie.get('search-AppInstaller');
    if ((searchField != null) && (searchField != "") && (searchField != undefined)) {
        $("#tb_search_AppInstaller").val(searchField);
    }

    $("#apps_holder").hide();
    $("#all_apps_holder").hide();
    $("#app-category-sidebar").hide();
    $("#plugin-info-sidebar").show();
    $('#plugins_holder').show();

    $("#select-btns-appinstaller").hide();

    $("#appinstaller-load").find("#hdl1_apps").removeClass("active");
    $("#appinstaller-load").find("#hdl2_plugins").addClass("active");

    cookie.set("tab-AppInstaller", "plugins", "30");

    LoadPlugins();
}

function LoadPlugins() {
    LoadingMessage_AppInstaller("Loading Plugin List...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/LoadPlugins",
        data: "{ 'search':'" + $("#tb_search_AppInstaller").val() + "','path':'" + location.pathname + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                $("#plugins_holder").html(data.d);
            }
            else {
                $("#plugins_holder").html("<div class='clear-space'></div><h4 class='pad-left pad-top-big'>No Plugins Available</h4>");
            }

            RemoveLoadingMessage_AppInstaller();
        },
        error: function () {
            RemoveLoadingMessage_AppInstaller();
        }
    });
}

function AddPlugin(id) {
    openWSE.LoadingMessage1("Installing Plugin. Please Wait...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/AddPlugin",
        data: "{ 'id':'" + id + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                window.location.href = window.location.href;
            }
            else {
                openWSE.AlertWindow("There was an error installing the requested plugin. Please contact administrator for more assistance");
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.RemoveUpdateModal();
        }
    });
}

function RemovePlugin(id) {
    openWSE.LoadingMessage1("Uninstalling Plugin. Please Wait...");
    $.ajax({
        url: "Apps/AppInstaller/AppInstallerService.asmx/RemovePlugin",
        data: "{ 'id':'" + id + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                window.location.href = window.location.href;
            }
            else {
                openWSE.AlertWindow("There was an error uninstalling the requested plugin. Please contact administrator for more assistance");
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.RemoveUpdateModal();
        }
    });
}