// -----------------------------------------------------------------------------------
//
//	appRemote v6.0
//	by John Donnelly
//	Last Modification: 11/7/2017
//
//	Licensed under the Creative Commons Attribution 2.5 License - http://creativecommons.org/licenses/by/2.5/
//  	- Free for use in both personal and commercial projects
//		- Attribution requires leaving author name, author link, and the license info intact.
//
// -----------------------------------------------------------------------------------

var appRemote_Config = {
    forceGroupLogin: false,
    autoSync: false,
    isAdminMode: false,
    isDemoMode: false
};

var appRemote = function () {
    var appMinWidth = 0;
    var appMinHeight = 0;
    var canSaveSort = false;
    var performingAction = false;

    function Initialize() {
        Load();
        InitializeSwipeEvent();
        if (appRemote_Config.autoSync) {
            CanConnectToWorkspace();
        }

        openWSE.UpdateLogoSizeOnLoad($("#top_bar_toolview_holder"));
    }

    function Load() {
        if ($(".app-icon-links").length > 0 || $(".app-icon-sub-links").length > 0) {
            $(".app-icon-links, .app-icon-sub-links").each(function () {
                var _href = $(this).attr("href");
                if (_href) {
                    if (_href.indexOf("#?") > 0) {
                        var tempHref = _href.split("#?")[1];
                        _href = _href.split("#?")[0] + "?mobileMode=true&fromAppRemote=true#?" + tempHref;
                    }
                    else if (_href.indexOf("?") > 0) {
                        _href += "&mobileMode=true&fromAppRemote=true";
                    }
                    else {
                        _href += "?mobileMode=true&fromAppRemote=true";
                    }

                    $(this).attr("href", _href);
                }
            });
        }

        if ($(".iframe-top-bar .iframe-title-logo img").width() > 100) {
            $(".iframe-top-bar .iframe-title-logo span").hide();
        }

        UpdateConnectToWorkspace();
        LoadHashValues();
    }
    function LoadHashValues() {
        var loc = window.location.href.split("#");
        if (loc.length <= 1) {
            window.location += "#";
        }

        var currTabHash = appRemote.AppRemoteSidebar.GetHashParm("tab");
        var currIdHash = appRemote.AppRemoteSidebar.GetHashParm("id");

        if ($("#main_container").attr("data-paddingbottom")) {
            $("#main_container").css("padding-bottom", $("#main_container").attr("data-paddingbottom"));
            $("#main_container").attr("data-paddingbottom", "");
        }

        $(".iFrame-chat").remove();

        if (currTabHash && $("#" + currTabHash).length > 0) {
            if (currTabHash == "Notifications_tab") {
                appRemote.AppRemoteSidebar.MenuTabNavigation($("#" + currTabHash)[0]);
                openWSE.GetUserNotifications();
            }
            else if (currTabHash == "Apps_tab" && currIdHash) {
                FinishLoadAppOptions();
            }
            else if (currTabHash == "Chat_tab" && currIdHash) {
                FinishLoadChatMessenger(currIdHash);
            }
            else {
                appRemote.AppRemoteSidebar.MenuTabNavigation($("#" + currTabHash)[0]);
            }
        }
        else if ($(".section-link").length > 0) {
            appRemote.AppRemoteSidebar.MenuTabNavigation($(".section-link").eq(0)[0]);
        }
        else if (currTabHash === "Groups_tab") {
            openWSE.GetandBuildGroupList();
            $(".main-content-panels").removeClass("active-panel");
            $("#pnlContent_Groups").addClass("active-panel");
            $("#Close_tab").show();
        }
        else if (!currTabHash && $("#pnlContent_Login").length > 0) {
            $(".main-content-panels").removeClass("active-panel");
            $("#pnlContent_Login").addClass("active-panel");
            $("#Close_tab").hide();
        }
    }
    
    function ResizeContent() {
        appRemote.AppRemoteSidebar.ResizeSidebar();
        $("#notification-tab-b").find(".li-pnl-tab").css({
            width: "100%",
            maxHeight: $("#main_container").outerHeight() - $("#footer_container").outerHeight()
        });

        $(".loaded-app-holder").each(function () {
            if ($(this).css("visibility") !== "hidden") {
                $(this).css({
                    width: $("#main_container").width(),
                    height: ($("#main_container").outerHeight() - 1)
                });
            }
        });

        if ($("#SiteTip-element").length > 0) {
            $("#SiteTip-element").find(".Modal-element-modal").css("min-width", $(window).width() - 50);
            $("#SiteTip-element").find(".Modal-element-modal").css("max-height", ($(window).height() - ($("#top_bar_toolview_holder").outerHeight() + $("#footer_container").outerHeight() + 50)));
            appRemote.AdjustSiteTipModal();
        }

        if ($(".iFrame-chat").length > 0) {
            var h1 = $(window).height();
            var h2 = $("#top_bar_toolview_holder").height();
            var h3 = $("#container_footer").height();
            var finalHeight = h1 - (h2 + h3);
            $(".iFrame-chat").height(finalHeight - 27);
        }

        if ($("#remote_sidebar").outerWidth() > 0) {
            GetOpenedAppsForMenu();
        }
    }


    // Auto Sync Workspace
    var canConnect = false;
    var skipAutoConnect = false;
    var refreshSyncTimer;
    var refreshSyncTimer_Interval = 5000;
    function CanConnectToWorkspace() {
        if (!appRemote_Config.forceGroupLogin) {
            if (!openWSE.IsComplexWorkspaceMode()) {
                $("#workspace_header_btn").remove();
                canConnect = true;
            }
            else {
                if (!appRemote_Config.isDemoMode && !appRemote_Config.isAdminMode) {
                    openWSE.AjaxCall("WebServices/AcctSettings.asmx/CanConnectToWorkspace", '{ }', null, function (data) {
                        var workspaceSelected = 1;
                        loadingPopup.RemoveMessage();
                        if (data.d == "simple") {
                            canConnect = false;
                        }
                        else {
                            if (data.d.indexOf("true~") === 0) {
                                canConnect = true;
                                workspaceSelected = data.d.replace("true~", "");
                            }
                            else {
                                canConnect = false;
                            }
                        }

                        if (canConnect) {
                            GetAllOpenedApps();
                            UpdateConnectToWorkspace(workspaceSelected);
                            refreshSyncTimer = setTimeout(function () {
                                CanConnectToWorkspace();
                            }, refreshSyncTimer_Interval);
                        }
                        else {
                            UpdateConnectToWorkspace(workspaceSelected);
                            clearTimeout(refreshSyncTimer);
                        }
                    }, function () {
                        canConnect = false;
                        loadingPopup.RemoveMessage();
                        UpdateConnectToWorkspace();
                    });
                }
            }
        }
    }
    function UpdateConnectToWorkspace(workspaceSelected) {
        if (canConnect) {
            $("#workspace_header_btn").show();
            $("#connect_header_btn").html("<span class='connected-img'></span>Desync with Workspace");
            $("#connect_header_btn").addClass("synced");

            var tab = appRemote.AppRemoteSidebar.GetHashParm("tab");
            var id = appRemote.AppRemoteSidebar.GetHashParm("id");
            if (tab === "Apps_tab" && id && $("#no-options-available").css("display") !== "none") {
                GetAppProps();
            }
        }
        else {
            $("#workspace_header_btn").hide();
            $("#connect_header_btn").html("<span class='disconnected-img'></span>Sync with Workspace");
            $("#connect_header_btn").removeClass("synced");
        }

        $("#dropdownSelector").val(workspaceSelected);
    }
    $(document.body).on("change", "#dropdownSelector", function () {
        if (!performingAction) {
            performingAction = true;
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateAppRemote", '{ "id": "workspace-selector","options": "' + $(this).val() + '" }', null, function (data) {
                performingAction = false;
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    $("#workspace-selector-overlay, #workspace-selector-modal").hide();
                }
                else {
                    openWSE.AlertWindow("Error! Try again.");
                    if (canConnect) {
                        canConnect = false;
                        Load();
                    }
                }
            }, function (data) {
                performingAction = false;
                openWSE.AlertWindow("Error! Try again.");
                if (canConnect) {
                    canConnect = false;
                    Load();
                }
            });
        }
    });


    function InitializeSwipeEvent() {
        if ($("#remote_sidebar").length > 0) {
            try {
                if ($("#site_mainbody").length > 0) {
                    document.id("site_mainbody").addEvent('swipe', SwipeEvent);
                }
            }
            catch (evt) { }
        }
    }
    function SwipeEvent(event) {
        if (event) {
            switch (event.direction) {
                case "left":
                    AppRemoteSidebar.CloseSidebarMenu();
                    break;

                case "right":
                    AppRemoteSidebar.OpenSidebarMenu();
                    break;
            }
        }
    }


    // App Options
    function LoadAppOptions(id) {
        AppRemoteSidebar.CloseSidebarMenu();
        if (appRemote_Config.autoSync && !canConnect) {
            skipAutoConnect = true;
        }

        window.location.hash = "#?tab=Apps_tab&id=" + id;
    }
    function FinishLoadAppOptions() {
        if (appRemote_Config.autoSync && !canConnect && !skipAutoConnect) {
            CanConnectToWorkspace();
        }

        skipAutoConnect = false;

        $(".section-link").removeClass("active-tab");
        $("#Apps_tab").addClass("active-tab");
        $(".main-content-panels").removeClass("active-panel");
        $("#Minimize_tab").hide();
        $("#Close_tab").hide();
        $("#About_tab").hide();
        $(".loaded-app-holder").each(function () {
            $(this).css({
                visibility: "hidden",
                width: 0,
                height: 0
            });
        });

        GetAppProps();
    }
    function GetAppProps() {
        if (!performingAction) {
            var id = appRemote.AppRemoteSidebar.GetHashParm("id");

            $(".app-icon[data-appid='" + id + "']").removeClass("active");
            if ($("#" + id + "-loadedapp").length > 0) {
                loadingPopup.RemoveMessage();
                $(".loading-background-holder").remove();
                LoadOpenedApp(id);
                GetAllOpenedApps();
                return;
            }

            $("#pnlContent_AppOptions-Holder").show();

            var name = $.trim($(".app-icon[data-appid='" + id + "']").find(".app-icon-font").html());
            if (name) {
                $("#load-option-text").html("<b>" + name + "</b> Options");
            }
            else {
                $("#load-option-text").html("Options");
            }

            performingAction = true;
            loadingPopup.Message("Loading Options...");

            openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetAppRemoteProps", '{ "appId": "' + id + '" }', null, function (data) {
                loadingPopup.RemoveMessage();
                $(".loading-background-holder").remove();
                if (openWSE.IsComplexWorkspaceMode()) {
                    $("#options-btn-open").show();
                }

                $("#options-btn-update").hide();
                $("#pnl_appMoveResize").hide();
                $("#Close_tab").show();
                $("#About_tab").show();

                if (data.d.length > 0 && data.d[0].length == 14) {
                    var closed = data.d[0][0];
                    var min = data.d[0][1];
                    var max = data.d[0][2];
                    var db = data.d[0][3];
                    var updated = data.d[0][4];

                    var poxX = data.d[0][5].replace("px", "");
                    var poxY = data.d[0][6].replace("px", "");
                    var width = data.d[0][7].replace("px", "");
                    var height = data.d[0][8].replace("px", "");

                    var minHeight = data.d[0][9].replace("px", "");
                    var minWidth = data.d[0][10].replace("px", "");

                    if (!width || width === "null") {
                        width = minWidth;
                    }
                    if (!height || height === "null") {
                        height = minHeight;
                    }

                    var popoutLoc = data.d[0][11];
                    var allowMax = data.d[0][12];
                    var allowResize = data.d[0][13];

                    if ((width == "0") || (width == "")) {
                        width = minWidth;
                    }
                    if ((height == "0") || (height == "")) {
                        height = minHeight;
                    }

                    if ($("#app-load-options").length > 0) {
                        appMinWidth = minWidth;
                        appMinHeight = minHeight;

                        $("#moveresize-left").val(poxX);
                        $("#moveresize-top").val(poxY);
                        $("#moveresize-width").val(width);
                        $("#moveresize-height").val(height);

                        if (openWSE.IsComplexWorkspaceMode()) {
                            if (canConnect) {
                                $("#app-load-options").show();
                            }
                            else {
                                $("#app-load-options").hide();
                            }

                            if (openWSE.ConvertBitToBoolean(closed)) {
                                $("#options-btn-close").hide();
                                $("#app-load-options").hide();
                            }
                            else {
                                $("#app-load-options").show();
                                $("#options-btn-close").show();
                                $("#options-btn-open").hide();
                                $("#options-btn-update").show();
                                if (canConnect) {
                                    $(".app-icon[data-appid='" + id + "']").addClass("active");
                                }
                            }
                        }
                        else {
                            $("#options-btn-close").hide();
                            $("#options-btn-open").hide();
                            $("#options-btn-update").hide();
                            $("#app-load-options").hide();
                        }

                        if (db != "") {
                            $("#ddl_appDropdownSelector").val(GetWorkspaceNumber(db));
                        }
                        else {
                            $("#ddl_appDropdownSelector").val(GetWorkspaceNumber(data.d[1]));
                        }

                        $("#ddl_appstate").val("Normal");
                        if (!openWSE.ConvertBitToBoolean(closed)) {
                            $("#pnl_appMoveResize").hide();
                            $("#resize-div").hide();

                            if ((!openWSE.ConvertBitToBoolean(allowMax)) && (!openWSE.ConvertBitToBoolean(allowResize))) {
                                $("#pnl_appMoveResize").show();
                            }
                            else if (openWSE.ConvertBitToBoolean(allowResize)) {
                                $("#pnl_appMoveResize").show();
                                $("#resize-div").show();
                            }

                            if (openWSE.ConvertBitToBoolean(max)) {
                                $("#ddl_appstate").val("Maximize");
                            }
                            if (openWSE.ConvertBitToBoolean(min)) {
                                $("#ddl_appstate").val("Minimize");
                            }
                        }
                        else {
                            $("#ddl_appstate").val("Normal");
                            $("#pnl_appMoveResize").hide();
                        }

                        if (updated != "") {
                            $("#last-updated").html("<b class='pad-right-sml'>Last Updated:</b>" + updated);
                        }

                        if (openWSE.ConvertBitToBoolean(allowMax)) {
                            $("#ddl_appstate > option[value='Maximize']").show();
                        }
                        else {
                            $("#ddl_appstate > option[value='Maximize']").hide();
                        }
                    }

                    $("#no-options-available").hide();
                    $("#options-btn-device").hide();
                    if (popoutLoc != "" && ($("#options-btn-close").css("display") === "none" || appRemote_Config.isDemoMode || !canConnect)) {
                        $("#options-btn-device").attr("onclick", "appRemote.LoadApp('" + id + "');return false;");
                        $("#options-btn-device").attr("href", popoutLoc);
                        $("#options-btn-device").show();
                    }

                    if (appRemote_Config.isDemoMode || !canConnect || !openWSE.IsComplexWorkspaceMode()) {
                        $("#options-btn-close").hide();
                        $("#options-btn-open").hide();
                        $("#options-btn-update").hide();
                        if ($("#options-btn-device").css("display") == "none") {
                            $("#no-options-available").show();
                        }
                    }
                }
                else {
                    $("#no-options-available").hide();
                    $("#options-btn-device").hide();
                    $("#options-btn-close").hide();
                    $("#pnl_appMoveResize").hide();
                    $("#ddl_appDropdownSelector").val("1");
                    $("#ddl_appstate").val("Normal");
                    $("#last-updated").html("");
                }

                var totalOptionsShown = 0;
                $("#load-option-btn-holder").find(".option-buttons").each(function () {
                    if ($(this).css("display") != "none") {
                        totalOptionsShown++;
                    }
                });

                performingAction = false;
                if (totalOptionsShown == 1 && ($("#options-btn-device").css("display") == "inline-block" || $("#options-btn-device").css("display") == "block")) {
                    LoadApp(id);
                }
                else {
                    $("#load-option-text").next(".accordion-content").show();
                }

                $("#pnlContent_AppOptions").addClass("active-panel");
                GetOpenedAppsForMenu();
            }, function () {
                loadingPopup.RemoveMessage();
                $(".loading-background-holder").remove();
                openWSE.AlertWindow("Error trying to access account. Please try again.");
                if (canConnect) {
                    canConnect = false;
                    Load();
                }
                performingAction = false;
            });
        }
    }

    function LoadApp(id) {
        var $iframe = $("#" + id + "-loadedapp");
        if ($iframe.length > 0) {
            $iframe.remove();
        }

        $("#pnlContent_AppOptions-Holder").hide();
        $("#Minimize_tab").show();
        $("#Close_tab").show();
        $("#About_tab").show();

        var href = $("#options-btn-device").attr("href");
        if (href.toLowerCase().indexOf("externalappholder.aspx?appid") > 0) {
            href += "&hidetoolbar=true";
        }

        $(".app-icon[data-appid='" + id + "']").addClass("active");
        AppendLoadingMessage($("#pnlContent_AppOptions")[0], id);

        $("#main_container").addClass("app-open-containerpadding");
        $("#pnlContent_AppOptions").addClass("active-panel");
        $("#pnlContent_AppOptions").append("<iframe id='" + id + "-loadedapp' src='" + href + "' frameborder='0' class='loaded-app-holder' style='width: " + $("#main_container").width() + "px; height: " + ($("#main_container").outerHeight() - 1) + "px;'></iframe>");
        $(".loaded-app-holder").one("load", (function () {
            $(".loading-background-holder").remove();
        }));
    }
    function LoadOpenedApp(id) {
        $("#pnlContent_AppOptions-Holder").hide();
        $("#Minimize_tab").show();
        $("#Close_tab").show();
        $("#About_tab").show();

        $("#main_container").addClass("app-open-containerpadding");
        $("#pnlContent_AppOptions").addClass("active-panel");
        $(".app-icon[data-appid='" + id + "']").addClass("active");
        $("#" + id + "-loadedapp").css({
            visibility: "",
            width: $("#main_container").width(),
            height: ($("#main_container").outerHeight() - 1)
        });
        ResizeContent();
    }


    function LoadOnWorkspace() {
        if (!performingAction) {
            performingAction = true;
            var options = "";
            var isMaxed = false;

            options += $("#ddl_appDropdownSelector").val() + ";";

            if ($("#ddl_appstate").val() === "Maximize") {

                options += "maximize;";
                isMaxed = true;
            }
            else if ($("#ddl_appstate").val() === "Minimize") {
                options += "minimize;";
            }
            else {
                options += "normal;";
            }

            var top = parseInt($("#moveresize-top").val());
            var left = parseInt($("#moveresize-left").val());
            if ((top < 0) || (top == null) || ((top.toString() == "NaN") && (top != null))) {
                top = 0;
            }
            if ((left < 0) || (left == null) || ((left.toString() == "NaN") && (left != null))) {
                left = 0;
            }

            var width = parseInt($("#moveresize-width").val());
            var height = parseInt($("#moveresize-height").val());
            if ((width < appMinWidth) || (width == null) || ((width.toString() == "NaN") && (width != null))) {
                width = appMinWidth;
            }
            if ((height < appMinHeight) || (height == null) || ((height.toString() == "NaN") && (height != null))) {
                height = appMinHeight;
            }

            options += top + ";" + left + ";" + width + ";" + height + ";";

            if ($("#options-btn-update").css("display") == "block") {
                loadingPopup.Message("Refreshing App...");
            }
            else {
                loadingPopup.Message("Loading App...");
            }

            var id = appRemote.AppRemoteSidebar.GetHashParm("id");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateAppRemote", '{ "id": "' + id + '","options": "' + escape(options) + '" }', null, function (data) {
                performingAction = false;
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    loadingPopup.RemoveMessage();
                    GetAppProps();
                }
                else {
                    loadingPopup.RemoveMessage();
                    openWSE.AlertWindow("Could not load");
                    if (canConnect) {
                        canConnect = false;
                        Load();
                    }
                }
            }, function (data) {
                performingAction = false;
                loadingPopup.RemoveMessage();
                openWSE.AlertWindow("Could not load");
                if (canConnect) {
                    canConnect = false;
                    Load();
                }
            });
        }
    }
    function CloseAppOnWorkspace() {
        if (!performingAction) {
            performingAction = true;
            loadingPopup.Message("Closing App...");
            var id = appRemote.AppRemoteSidebar.GetHashParm("id");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateAppRemote", '{ "id": "' + id + '","options": "' + "close" + '" }', null, function (data) {
                performingAction = false;
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    loadingPopup.RemoveMessage();
                    GetAppProps();
                }
                else {
                    loadingPopup.RemoveMessage();
                    openWSE.AlertWindow("Error! Try again.");
                    if (canConnect) {
                        canConnect = false;
                        Load();
                    }
                }
            }, function () {
                performingAction = false;
                loadingPopup.RemoveMessage();
                openWSE.AlertWindow("Error! Try again.");
                if (canConnect) {
                    canConnect = false;
                    Load();
                }
            });
        }
    }


    function GetWorkspaceNumber(db) {
        return db.replace("workspace_", "");
    }
    function AppendLoadingMessage(_this, id) {
        $(_this).append("<div class='loading-background-holder' data-usespinner='true'><div></div></div>");
        var $appIcon = $(".app-icon[data-appid='" + id + "']");
        if ($appIcon.length > 0 && $appIcon.find("img").length > 0 && $appIcon.find("img").css("display") != "none" && !$appIcon.find("img").hasClass("display-none")) {
            $(_this).find(".loading-background-holder").css("background-image", "url('" + $appIcon.find("img").attr("src") + "')");
            if ($appIcon.attr("data-appbgcolor")) {
                $(_this).find(".loading-background-holder").css("background-color", $appIcon.attr("data-appbgcolor"));
                $(_this).find(".loading-background-holder").attr("data-usespinner", "false");
            }
        }
    }
    function GetAllOpenedApps() {
        $(".app-icon").removeClass("active");
        $("#pnlContent_AppOptions").find(".loaded-app-holder").each(function () {
            var loadedAppId = $(this).attr("id").replace("-loadedapp", "");
            $(".app-icon[data-appid='" + loadedAppId + "']").addClass("active");
        });

        if (openWSE.IsComplexWorkspaceMode()) {
            $(".workspace-reminder").each(function () {
                $(this).remove();
            });

            if (canConnect) {
                var loadOverlayto = setTimeout(function () {
                    loadingPopup.Message("Loading Remote...");
                }, 1500);

                openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetAllOpenedApps", '{ }', null, function (data) {
                    clearTimeout(loadOverlayto);
                    loadingPopup.RemoveMessage();
                    if (data.d != null) {
                        for (var i = 0; i < data.d.length; i++) {
                            var $this = $(".app-icon[data-appid='" + data.d[i][0] + "']");
                            $this.addClass("active");

                            if (data.d[i][1] != "") {
                                var style = "";
                                if ($this.find(".app-icon-font").length > 0 && ($this.hasClass("Icon_And_Color_Only") || $this.hasClass("Color_And_Description") || $this.hasClass("Icon_Plus_Color_And_Text"))) {
                                    var ftColor = $this.find(".app-icon-font").css("color");
                                    style = " style='color:" + ftColor + "!important;'";
                                }

                                var db = "<span class='workspace-reminder font-no-bold'" + style + ">" + GetWorkspaceNumber(data.d[i][1]) + "</span>";
                                $this.append(db);
                            }
                        }
                    }

                    GetOpenedAppsForMenu();
                });
            }
            else {
                GetOpenedAppsForMenu();
            }
        }
        else {
            clearTimeout(loadOverlayto);
            loadingPopup.RemoveMessage();
        }
    }


    // Opened Apps Displayed in Menu
    function GetOpenedAppsForMenu() {
        var appArray = new Array();

        $("#opened_apps_header").hide();
        $("#opened_apps_holder").html("");

        $("#pnlContent_AppOptions").find(".loaded-app-holder").each(function () {
            var id = $(this).attr("id").replace("-loadedapp", "");
            var $app = $(".app-icon[data-appid='" + id + "']");
            if ($app.length > 0 && !ArrayContainsItem(id, appArray)) {
                $app = $app.eq(0);
                var appHtml = $.trim($app.html());

                var backagroundStyle = "";
                if ($app.hasClass("Color_And_Description") || $app.hasClass("Icon_And_Color_Only") || $app.hasClass("Icon_Plus_Color_And_Text")) {
                    backagroundStyle = "style='background: " + $app.css("background") + "'";
                }

                var onClick = $app.attr("onclick");
                $("#opened_apps_holder").append("<div data-appid='" + id + "' class='opened-app-icon' onclick=\"" + onClick + "\" " + backagroundStyle + ">" + appHtml + "</div>");
                appArray.push(id);
            }
        });

        $("#pnl_icons").find(".app-icon").each(function () {
            var id = $(this).attr("data-appid");
            if ($(this).hasClass("active") && !ArrayContainsItem(id, appArray)) {
                var appHtml = $.trim($(this).html());

                var backagroundStyle = "";
                if ($(this).hasClass("Color_And_Description") || $(this).hasClass("Icon_And_Color_Only") || $(this).hasClass("Icon_Plus_Color_And_Text")) {
                    backagroundStyle = "style='background: " + $(this).css("background") + "'";
                }

                var onClick = $(this).attr("onclick");
                $("#opened_apps_holder").append("<div data-appid='" + id + "' class='opened-app-icon' onclick=\"" + onClick + "\" " + backagroundStyle + ">" + appHtml + "</div>");
                appArray.push(id);
            }
        });

        if (appArray.length > 0) {
            $("#opened_apps_header").show();
        }
    }
    function ArrayContainsItem(item, appArray) {
        for (var i = 0; i < appArray.length; i++) {
            if (item == appArray[i]) {
                return true;
            }
        }

        return false;
    }
    function CloseAllOpened() {
        $("#pnlContent_AppOptions").find(".loaded-app-holder").each(function () {
            var id = $(this).attr("id").replace("-loadedapp", "");
            $("#pnlContent_AppOptions").find("#" + id + "-loadedapp").remove();
            $("#load-option-text").html("<b>" + $.trim($("#" + id).find(".app-icon-font").html()) + "</b> Options");
        });

        $(".app-icon").removeClass("active");
        $(".app-icon").find(".workspace-reminder").remove();

        if (!performingAction && canConnect) {
            performingAction = true;
            loadingPopup.Message("Closing Apps...");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/CloseAllAppsFromAppRemote", '{ }', null, null, null, function (data) {
                performingAction = false;
                loadingPopup.RemoveMessage();
                $("#pnlContent_AppOptions-Holder").show();
                $("#options-btn-device").show();

                $("#opened_apps_header").hide();
                $("#opened_apps_holder").html("");

                AppRemoteSidebar.RefresTabUrl();
            });
        }
        else if (!canConnect) {
            loadingPopup.RemoveMessage();
            $("#pnlContent_AppOptions-Holder").show();
            $("#options-btn-device").show();

            $("#opened_apps_header").hide();
            $("#opened_apps_holder").html("");

            AppRemoteSidebar.RefresTabUrl();
        }
    }

    function SyncWithWorkspaceClick() {
        if (!canConnect) {
            loadingPopup.Message("Trying to Connect");
            CanConnectToWorkspace();
        }
        else {
            skipAutoConnect = true;
            canConnect = false;
            clearTimeout(refreshSyncTimer);
            Load();
        }
    }


    // Chat Messenger
    function FinishLoadChatMessenger(user) {
        chatClient.BuildChatWindowMobile(user);
        $(".section-link").removeClass("active-tab");
        $("#Chat_tab").addClass("active-tab");
        $(".main-content-panels").removeClass("active-panel");
        $("#pnlContent_ChatPopup").addClass("active-panel");
        $("#Minimize_tab").hide();
        $("#Close_tab").show();
        $("#About_tab").show();

        if ($("#main_container").css("padding-bottom")) {
            $("#main_container").attr("data-paddingbottom", $("#main_container").css("padding-bottom"));
            $("#main_container").css("padding-bottom", "");
        }
    }


    // AppRemote Sidebar Functions
    var AppRemoteSidebar = function () {
        var maxSidebarWidth = 250;
        function OnMenuSidebarClick() {
            openWSE.CloseTopDropDowns();
            if ($("#remote_sidebar").length > 0) {
                if (!$("#remote_sidebar").hasClass("active")) {
                    OpenSidebarMenu();
                }
                else {
                    CloseSidebarMenu();
                }
            }
        }
        function OpenSidebarMenu() {
            GetOpenedAppsForMenu();
            $("#remote_sidebar").addClass("active");
            $(".sidebar-overlay").remove();
            $("body").append("<div class='sidebar-overlay' onclick='appRemote.AppRemoteSidebar.OnMenuSidebarClick();'></div>");
            ResizeSidebar();
        }
        function CloseSidebarMenu() {
            $("#opened_apps_header").hide();
            $("#opened_apps_holder").html("");

            $(".sidebar-overlay").remove();
            $(".user-profile-options-holder").hide();
            $("#remote_sidebar").css("width", "");
            $("#remote_sidebar").removeClass("active");
        }
        function ResizeSidebar() {
            if ($("#remote_sidebar").length > 0 && $("#remote_sidebar").hasClass("active")) {
                if ($(window).width() <= maxSidebarWidth) {
                    $("#remote_sidebar").css("width", "100%");
                }
            }
        }

        function MenuTabNavigation(_this) {
            var _tabId = $(_this).attr("data-tabid");

            $(".section-link").removeClass("active-tab");
            $(".main-content-panels").removeClass("active-panel");
            $("#Minimize_tab").hide();
            $("#Close_tab").hide();
            $("#About_tab").hide();
            $("#main_container").removeClass("app-open-containerpadding");

            if (_tabId == "Close_tab") {
                var id = GetHashParm("id");
                if (GetHashParm("tab") === "Chat_tab" && $(".section-link[data-tabid='Chat_tab']").length > 0) {
                    $(".iFrame-chat").remove();
                    MenuTabNavigation($(".section-link[data-tabid='Chat_tab']")[0]);
                }
                else {
                    if (GetHashParm("tab") === "Groups_tab" && $("#pnlContent_Login").length > 0) {
                        window.location.href = openWSE.siteRoot() + "AppRemote.aspx#";
                    }
                    else {
                        if (id && $("#" + id + "-loadedapp").length > 0) {
                            $(".app-icon[data-appid='" + id + "']").removeClass("active");
                            $("#" + id + "-loadedapp").remove();
                        }

                        MenuTabNavigation($(".section-link").eq(0)[0]);
                    }
                }

                GetOpenedAppsForMenu();
            }
            else if (_tabId == "Minimize_tab") {
                MenuTabNavigation($(".section-link").eq(0)[0]);
                GetOpenedAppsForMenu();
            }
            else if (_tabId) {
                $(_this).addClass("active-tab");
                var thisId = "pnlContent_" + _tabId.replace("_tab", "");
                LoadMenuTabData(_tabId);

                if ($("#" + thisId).length > 0) {
                    $("#" + thisId).addClass("active-panel");
                    window.location.hash = "#?tab=" + _tabId;
                }
                else {
                    window.location.href = openWSE.siteRoot() + "AppRemote.aspx#?tab=" + _tabId;
                }
            }

            if ($(".fixed-container-holder-background").offset().left === 0) {
                CloseSidebarMenu();
            }
            ResizeSidebar();
        }
        function GetHashParm(name) {
            var hashParm = window.location.hash;
            if (hashParm) {
                if (hashParm.indexOf("#") === 0) {
                    hashParm = hashParm.substring(1);
                }

                var splitHashParm = hashParm.split(/[?&]+/);
                for (var i = 0; i < splitHashParm.length; i++) {
                    if (splitHashParm[i]) {
                        var splitItem = splitHashParm[i].split("=");
                        if (splitItem.length >= 2 && splitItem[0] === name) {
                            return unescape(splitItem[1]);
                        }
                    }
                }
            }

            return "";
        }

        function LoadMenuTabData(tabId) {
            // Close tab data
            $("#pnlContent_Groups").find("#grouplistdiv").html("");
            $("#divGroupLogoff").hide();
            openWSE.CloseNoti();

            if (tabId == "Groups_tab") {
                openWSE.GetandBuildGroupList();
            }
            if (tabId == "Notifications_tab") {
                appRemote.ResizeContent();
                openWSE.GetUserNotifications();
            }
        }
        function RefresTabUrl() {
            var currTab = AppRemoteSidebar.GetHashParm("tab");
            if (currTab && $(".section-link[data-tabid='" + "']").length > 0) {
                MenuTabNavigation($(".section-link[data-tabid='" + "']")[0]);
            }
            else {
                MenuTabNavigation($(".section-link").eq(0)[0]);
            }
        }

        return {
            MenuTabNavigation: MenuTabNavigation,
            OnMenuSidebarClick: OnMenuSidebarClick,
            ResizeSidebar: ResizeSidebar,
            GetHashParm: GetHashParm,
            OpenSidebarMenu: OpenSidebarMenu,
            CloseSidebarMenu: CloseSidebarMenu,
            RefresTabUrl: RefresTabUrl
        }
    }();


    $(document.body).on("change", "#ddl_appDropdownSelector, #ddl_appstate", function (event) {
        LoadOnWorkspace();
    });
    $(document.body).on("keydown", "#moveresize-top, #moveresize-left, #moveresize-width, #moveresize-height", function (event) {
        try {
            if (event.which == 13) {
                event.preventDefault();
                LoadOnWorkspace();
                return false;
            }
        } catch (evt) {
            if (event.keyCode == 13) {
                event.preventDefault();
                LoadOnWorkspace();
                return false;
            }
            delete evt;
        }
    });

    var initialPropVal = "";
    $(document.body).on("focus", "#moveresize-top, #moveresize-left, #moveresize-width, #moveresize-height", function (event) {
        this.select();
        initialPropVal = $.trim($(this).val());
    });
    $(document.body).on("blur", "#moveresize-top, #moveresize-left, #moveresize-width, #moveresize-height", function (event) {
        if (initialPropVal != $.trim($(this).val())) {
            LoadOnWorkspace();
        }

        initialPropVal = "";
    });


    function LoadCreateAccountHolder() {
        if ($("#Login-holder").css("display") != "none") {
            $("#Login-holder").hide();

            var fullUrl = "SiteTools/iframes/CreateAccount.aspx";
            $("#iframe-createaccount-holder").html("<iframe id='iframe-demo' src='" + fullUrl + "' frameborder='0' width='100%' style='visibility: hidden;'></iframe>");
            $("#iframe-createaccount-holder").append("<div style='text-align: center;'><h3 id='loadingControls'>Loading Controls. Please Wait...</h3></div>");
            $("#CreateAccount-holder").show();
            document.getElementById("iframe-demo").onload = function () {
                $("#loadingControls").remove();
                $("#iframe-demo").css({
                    height: "475px",
                    visibility: "visible"
                });
            };
        }
        else {
            $("#CreateAccount-holder, #ForgotPassword-holder").hide();
            $("#iframe-createaccount-holder").html("");
            $("#Login-holder").show();
        }
    }
    function LoadRecoveryPassword() {
        if ($("#ForgotPassword-holder").css("display") != "block") {
            $("#tb_username_recovery").val("");
            $("#lbl_passwordResetMessage").html("");
            $("#UserNameRequired_recovery").css("visibility", "hidden");
            $("#Login-holder").hide();
            $("#ForgotPassword-holder").show();
        }
        else {
            $("#ForgotPassword-holder").hide();
            $("#Login-holder").show();
        }
    }


    var tipIndex = 0;
    var tipArray = new Array();
    function SiteTipsOnPageLoad() {
        if (openWSE_Config.siteTipsOnPageLoad) {
            openWSE.AjaxCall("SiteTips.xml", null, {
                type: "GET",
                dataType: "xml",
                cache: false
            }, null, null, function (xml) {
                if (tipArray.length == 0) {
                    $(xml.responseText).find("Hint").each(function () {
                        var tipText = $.trim($(this).html());
                        if (tipText != "") {
                            tipArray.push(tipText);
                        }
                    });
                }

                if (tipArray.length > 0) {
                    tipIndex = Math.round(Math.random() * (tipArray.length - 1));

                    var ele = "<div id='SiteTip-element' class='Modal-element' style='display: block; visibility: visible; opacity: 0.0; filter: alpha(opacity=0);'>";
                    ele += "<div class='Modal-overlay'>";
                    ele += "<div class='Modal-element-align'>";
                    ele += "<div class='Modal-element-modal' style='overflow:auto; min-width: " + ($(window).width() - 50) + "px; max-height: " + ($(window).height() - ($("#always_visible").outerHeight() + $("#container_footer").outerHeight() + 50)) + "px;'>";

                    // Body
                    var nextTipButton = "<input class='input-buttons nextprev-button' type='button' value='Next Tip' onclick=\"appRemote.NextSiteTip();\" /><div class='clear-space-five'></div>";
                    var prevTipButton = "<input class='input-buttons nextprev-button margin-bottom' type='button' value='Previous Tip' onclick=\"appRemote.PreviousSiteTip();\" />";
                    if (tipArray.length == 1) {
                        nextTipButton = "";
                        prevTipButton = "";
                    }

                    var closeButton = "<input class='input-buttons confirm-close-button float-left' type='button' value='Close' onclick=\"appRemote.CloseSiteTip();\" />";
                    var dontShowAgain = "<div class='dont-show-again'><input id='dont-show-again-cb' type='checkbox' checked='checked' /><label for='dont-show-again-cb'>Show Tips on Page Load</label></div>";

                    if (openWSE_Config.siteTheme == "") {
                        openWSE_Config.siteTheme = "Standard";
                    }

                    var img = "<img alt='light-bulb' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/Icons/sitetip.png' />";
                    var tipMessage = tipArray[tipIndex];

                    ele += "<div class='ModalPadContent'>" + img + "<span class='tip-title'>Did you know?</span><div class='clear'></div><div class='message-text'>" + tipMessage + "</div>";
                    ele += "<div class='button-holder'>" + dontShowAgain + "<div class='clear-space'></div><div class='clear-space'></div>" + prevTipButton + nextTipButton + closeButton + "<div class='clear'></div></div></div>";
                    ele += "</div></div></div></div>";

                    $("body").append(ele);
                    AdjustSiteTipModal();
                    $("#SiteTip-element").fadeTo(openWSE_Config.animationSpeed * 2, 1.0, function () {
                        AdjustSiteTipModal();
                    });

                    $("#SiteTip-element").find(".Modal-overlay").on("click", function (e) {
                        if (e.target.className == "Modal-overlay") {
                            appRemote.CloseSiteTip();
                        }
                    });
                }
            });
        }
    }
    function NextSiteTip() {
        tipIndex++;
        if (tipIndex >= tipArray.length) {
            tipIndex = 0;
        }

        $("#SiteTip-element").find(".message-text").html(tipArray[tipIndex]);
        AdjustSiteTipModal();
    }
    function PreviousSiteTip() {
        tipIndex--;
        if (tipIndex < 0) {
            tipIndex = tipArray.length - 1;
        }
        $("#SiteTip-element").find(".message-text").html(tipArray[tipIndex]);
        AdjustSiteTipModal();
    }
    function CloseSiteTip() {
        if (!$("#dont-show-again-cb").prop("checked")) {
            if (!appRemote_Config.isDemoMode) {
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/TurnOffSiteTipsOnPageLoad");
            }
            else {
                cookieFunctions.set("siteTipsOnPageLoad", "false", "30");
            }
        }

        tipIndex = 0;
        tipArray = new Array();

        $('#SiteTip-element').remove();
    }
    function AdjustSiteTipModal() {
        $("#SiteTip-element").find(".Modal-element-align").css({
            marginTop: -($("#SiteTip-element").find(".Modal-element-modal").height() / 2),
            marginLeft: -($("#SiteTip-element").find(".Modal-element-modal").width() / 2)
        });
    }

    function AboutApp() {
        var currTabHash = appRemote.AppRemoteSidebar.GetHashParm("tab");
        var currIdHash = appRemote.AppRemoteSidebar.GetHashParm("id");

        if (currTabHash == "Chat_tab" && currIdHash) {
            currIdHash = "app-ChatClient-";
        }

        $("#MainContent_pnl_aboutHolder").html("");
        $("#hf_aboutstatsapp").val("about;" + currIdHash);
        loadingPopup.Message("Loading. Please Wait...");
        openWSE.CallDoPostBack("hf_aboutstatsapp", "");
    }

    return {
        Initialize: Initialize,
        Load: Load,
        LoadHashValues: LoadHashValues,
        ResizeContent: ResizeContent,
        LoadAppOptions: LoadAppOptions,
        LoadApp: LoadApp,
        SyncWithWorkspaceClick: SyncWithWorkspaceClick,
        AppRemoteSidebar: AppRemoteSidebar,
        CloseAllOpened: CloseAllOpened,
        LoadOnWorkspace: LoadOnWorkspace,
        CloseAppOnWorkspace: CloseAppOnWorkspace,
        LoadCreateAccountHolder: LoadCreateAccountHolder,
        LoadRecoveryPassword: LoadRecoveryPassword,
        NextSiteTip: NextSiteTip,
        PreviousSiteTip: PreviousSiteTip,
        CloseSiteTip: CloseSiteTip,
        AdjustSiteTipModal: AdjustSiteTipModal,
        AboutApp: AboutApp
    };
}();

$(document).ready(function () {
    appRemote.Initialize();
});

$(function () {
    $(window).hashchange(function () {
        appRemote.LoadHashValues();
    });
});

$(window).resize(function () {
    appRemote.ResizeContent();
});

window.onload = function () {
    if (openWSE_Config.ShowLoginModalOnDemoMode && $("#Login_tab").length > 0) {
        appRemote.AppRemoteSidebar.MenuTabNavigation(document.getElementById("Login_tab"));
    }
};


Function.prototype.startsWith = function (str) {
    return false;
}

// mootools.js
!function () { this.MooTools = { version: "1.3dev", build: "0a7aeabbbac5bc23b021b4c1aa9ba722c40e303d" }; var e = this.typeOf = function (e) { if (null == e) return "null"; if (e.$family) return e.$family(); if (e.nodeName) { if (1 == e.nodeType) return "element"; if (3 == e.nodeType) return /\S/.test(e.nodeValue) ? "textnode" : "whitespace" } else if ("number" == typeof e.length) { if (e.callee) return "arguments"; if ("item" in e) return "collection" } return typeof e }, t = (this.instanceOf = function (e, t) { if (null == e) return !1; for (var n = e.$constructor || e.constructor; n;) { if (n === t) return !0; n = n.parent } return e instanceof t }, this.Function), n = !0; for (var r in { toString: 1 }) n = null; n && (n = ["hasOwnProperty", "valueOf", "isPrototypeOf", "propertyIsEnumerable", "toLocaleString", "toString", "constructor"]), t.prototype.overloadSetter = function (e) { var t = this; return function (r, i) { if (null == r) return this; if (e || "string" != typeof r) { for (var o in r) t.call(this, o, r[o]); if (n) for (var s = n.length; s--;) o = n[s], r.hasOwnProperty(o) && t.call(this, o, r[o]) } else t.call(this, r, i); return this } }, t.prototype.overloadGetter = function (e) { var t = this; return function (n) { var r, i; if (e || "string" != typeof n ? r = n : arguments.length > 1 && (r = arguments), r) { i = {}; for (var o = 0; o < r.length; o++) i[r[o]] = t.call(this, r[o]) } else i = t.call(this, n); return i } }, t.prototype.extend = function (e, t) { this[e] = t }.overloadSetter(), t.prototype.implement = function (e, t) { this.prototype[e] = t }.overloadSetter(); var i = Array.prototype.slice; t.from = function (t) { return "function" == e(t) ? t : function () { return t } }, Array.from = function (t) { return null == t ? [] : o.isEnumerable(t) && "string" != typeof t ? "array" == e(t) ? t : i.call(t) : [t] }, Number.from = function (e) { var t = parseFloat(e); return isFinite(t) ? t : null }, String.from = function (e) { return e + "" }, t.implement({ hide: function () { return this.$hidden = !0, this }, protect: function () { return this.$protected = !0, this } }); var o = this.Type = function (t, n) { if (t) { var r = t.toLowerCase(); o["is" + t] = function (t) { return e(t) == r }, null != n && (n.prototype.$family = function () { return r }.hide()) } return null == n ? null : (n.extend(this), n.$constructor = o, n.prototype.$constructor = n, n) }, s = Object.prototype.toString; o.isEnumerable = function (e) { return null != e && "number" == typeof e.length && "[object Function]" != s.call(e) }; var a = {}, u = function (t) { var n = e(t.prototype); return a[n] || (a[n] = []) }, c = function (t, n) { if (n && n.$hidden) return this; for (var r = u(this), o = 0; o < r.length; o++) { var s = r[o]; "type" == e(s) ? c.call(s, t, n) : s.call(this, t, n) } var a = this.prototype[t]; return null != a && a.$protected || (this.prototype[t] = n), null == this[t] && "function" == e(n) && l.call(this, t, function (e) { return n.apply(e, i.call(arguments, 1)) }), this }, l = function (e, t) { if (t && t.$hidden) return this; var n = this[e]; return null != n && n.$protected || (this[e] = t), this }; o.implement({ implement: c.overloadSetter(), extend: l.overloadSetter(), alias: function (e, t) { c.call(this, e, this.prototype[t]) }.overloadSetter(), mirror: function (e) { return u(this).push(e), this } }), new o("Type", o); var h = function (e, t, n) { var r = t != Object, i = t.prototype; r && (t = new o(e, t)); for (var s = 0, a = n.length; s < a; s++) { var u = n[s], c = t[u], l = i[u]; c && c.protect(), r && l && (delete i[u], i[u] = l.protect()) } return r && t.implement(i), h }; h("String", String, ["charAt", "charCodeAt", "concat", "indexOf", "lastIndexOf", "match", "quote", "replace", "search", "slice", "split", "substr", "substring", "toLowerCase", "toUpperCase"])("Array", Array, ["pop", "push", "reverse", "shift", "sort", "splice", "unshift", "concat", "join", "slice", "indexOf", "lastIndexOf", "filter", "forEach", "every", "map", "some", "reduce", "reduceRight"])("Number", Number, ["toExponential", "toFixed", "toLocaleString", "toPrecision"])("Function", t, ["apply", "call", "bind"])("RegExp", RegExp, ["exec", "test"])("Object", Object, ["create", "defineProperty", "defineProperties", "keys", "getPrototypeOf", "getOwnPropertyDescriptor", "getOwnPropertyNames", "preventExtensions", "isExtensible", "seal", "isSealed", "freeze", "isFrozen"])("Date", Date, ["now"]), Object.extend = l.overloadSetter(), Date.extend("now", function () { return +new Date }), new o("Boolean", Boolean), Number.prototype.$family = function () { return isFinite(this) ? "number" : "null" }.hide(), Number.extend("random", function (e, t) { return Math.floor(Math.random() * (t - e + 1) + e) }), Object.extend("forEach", function (e, t, n) { for (var r in e) e.hasOwnProperty(r) && t.call(n, e[r], r, e) }), Object.each = Object.forEach, Array.implement({ forEach: function (e, t) { for (var n = 0, r = this.length; n < r; n++) n in this && e.call(t, this[n], n, this) }, each: function (e, t) { return Array.forEach(this, e, t), this } }); var f = function (t) { switch (e(t)) { case "array": return t.clone(); case "object": return Object.clone(t); default: return t } }; Array.implement("clone", function () { for (var e = this.length, t = new Array(e) ; e--;) t[e] = f(this[e]); return t }); var d = function (t, n, r) { switch (e(r)) { case "object": "object" == e(t[n]) ? Object.merge(t[n], r) : t[n] = Object.clone(r); break; case "array": t[n] = r.clone(); break; default: t[n] = r } return t }; Object.extend({ merge: function (t, n, r) { if ("string" == e(n)) return d(t, n, r); for (var i = 1, o = arguments.length; i < o; i++) { var s = arguments[i]; for (var a in s) d(t, a, s[a]) } return t }, clone: function (e) { var t = {}; for (var n in e) t[n] = f(e[n]); return t }, append: function (e) { for (var t = 1, n = arguments.length; t < n; t++) { var r = arguments[t] || {}; for (var i in r) e[i] = r[i] } return e } }), ["Object", "WhiteSpace", "TextNode", "Collection", "Arguments"].each(function (e) { new o(e) }); var p = Date.now(); String.extend("generateUID", function () { return (p++).toString(36) }) }(), Array.implement({ invoke: function (e) { var t = Array.slice(arguments, 1); return this.map(function (n) { return n[e].apply(n, t) }) }, every: function (e, t) { for (var n = 0, r = this.length; n < r; n++) if (n in this && !e.call(t, this[n], n, this)) return !1; return !0 }, filter: function (e, t) { for (var n = [], r = 0, i = this.length; r < i; r++) r in this && e.call(t, this[r], r, this) && n.push(this[r]); return n }, clean: function () { return this.filter(function (e) { return null != e }) }, indexOf: function (e, t) { for (var n = this.length, r = t < 0 ? Math.max(0, n + t) : t || 0; r < n; r++) if (this[r] === e) return r; return -1 }, map: function (e, t) { for (var n = [], r = 0, i = this.length; r < i; r++) r in this && (n[r] = e.call(t, this[r], r, this)); return n }, some: function (e, t) { for (var n = 0, r = this.length; n < r; n++) if (n in this && e.call(t, this[n], n, this)) return !0; return !1 }, associate: function (e) { for (var t = {}, n = Math.min(this.length, e.length), r = 0; r < n; r++) t[e[r]] = this[r]; return t }, link: function (e) { for (var t = {}, n = 0, r = this.length; n < r; n++) for (var i in e) if (e[i](this[n])) { t[i] = this[n], delete e[i]; break } return t }, contains: function (e, t) { return -1 != this.indexOf(e, t) }, append: function (e) { return this.push.apply(this, e), this }, getLast: function () { return this.length ? this[this.length - 1] : null }, getRandom: function () { return this.length ? this[Number.random(0, this.length - 1)] : null }, include: function (e) { return this.contains(e) || this.push(e), this }, combine: function (e) { for (var t = 0, n = e.length; t < n; t++) this.include(e[t]); return this }, erase: function (e) { for (var t = this.length; t--;) this[t] === e && this.splice(t, 1); return this }, empty: function () { return this.length = 0, this }, flatten: function () { for (var e = [], t = 0, n = this.length; t < n; t++) { var r = typeOf(this[t]); "null" != r && (e = e.concat("array" == r || "collection" == r || "arguments" == r || instanceOf(this[t], Array) ? Array.flatten(this[t]) : this[t])) } return e }, pick: function () { for (var e = 0, t = this.length; e < t; e++) if (null != this[e]) return this[e]; return null }, hexToRgb: function (e) { if (3 != this.length) return null; var t = this.map(function (e) { return 1 == e.length && (e += e), e.toInt(16) }); return e ? t : "rgb(" + t + ")" }, rgbToHex: function (e) { if (this.length < 3) return null; if (4 == this.length && 0 == this[3] && !e) return "transparent"; for (var t = [], n = 0; n < 3; n++) { var r = (this[n] - 0).toString(16); t.push(1 == r.length ? "0" + r : r) } return e ? t : "#" + t.join("") } }), String.implement({ test: function (e, t) { return ("regexp" == typeOf(e) ? e : new RegExp("" + e, t)).test(this) }, contains: function (e, t) { return t ? (t + this + t).indexOf(t + e + t) > -1 : this.indexOf(e) > -1 }, trim: function () { return this.replace(/^\s+|\s+$/g, "") }, clean: function () { return this.replace(/\s+/g, " ").trim() }, camelCase: function () { return this.replace(/-\D/g, function (e) { return e.charAt(1).toUpperCase() }) }, hyphenate: function () { return this.replace(/[A-Z]/g, function (e) { return "-" + e.charAt(0).toLowerCase() }) }, capitalize: function () { return this.replace(/\b[a-z]/g, function (e) { return e.toUpperCase() }) }, escapeRegExp: function () { return this.replace(/([-.*+?^${}()|[\]\/\\])/g, "\\$1") }, toInt: function (e) { return parseInt(this, e || 10) }, toFloat: function () { return parseFloat(this) }, hexToRgb: function (e) { var t = this.match(/^#?(\w{1,2})(\w{1,2})(\w{1,2})$/); return t ? t.slice(1).hexToRgb(e) : null }, rgbToHex: function (e) { var t = this.match(/\d{1,3}/g); return t ? t.rgbToHex(e) : null }, substitute: function (e, t) { return this.replace(t || /\\?\{([^{}]+)\}/g, function (t, n) { return "\\" == t.charAt(0) ? t.slice(1) : null != e[n] ? e[n] : "" }) } }), Function.extend({ attempt: function () { for (var e = 0, t = arguments.length; e < t; e++) try { return arguments[e]() } catch (e) { } return null } }), Function.implement({ attempt: function (e, t) { try { return this.apply(t, Array.from(e)) } catch (e) { } return null }, bind: function (e) { var t = this, n = arguments.length > 1 ? Array.slice(arguments, 1) : null; return function () { return n || arguments.length ? n && arguments.length ? t.apply(e, n.concat(Array.from(arguments))) : t.apply(e, n || arguments) : t.call(e) } }, pass: function (e, t) { var n = this; return null != e && (e = Array.from(e)), function () { return n.apply(t, e || arguments) } }, delay: function (e, t, n) { return setTimeout(this.pass(n, t), e) }, periodical: function (e, t, n) { return setInterval(this.pass(n, t), e) } }), Number.implement({ limit: function (e, t) { return Math.min(t, Math.max(e, this)) }, round: function (e) { return e = Math.pow(10, e || 0).toFixed(e < 0 ? -e : 0), Math.round(this * e) / e }, times: function (e, t) { for (var n = 0; n < this; n++) e.call(t, n, this) }, toFloat: function () { return parseFloat(this) }, toInt: function (e) { return parseInt(this, e || 10) } }), Number.alias("each", "times"), function (e) { var t = {}; e.each(function (e) { Number[e] || (t[e] = function () { return Math[e].apply(null, [this].concat(Array.from(arguments))) }) }), Number.implement(t) }(["abs", "acos", "asin", "atan", "atan2", "ceil", "cos", "exp", "floor", "log", "max", "min", "pow", "sin", "sqrt", "tan"]), function () { var e = this.Class = new Type("Class", function (r) { instanceOf(r, Function) && (r = { initialize: r }); var i = function () { if (n(this), i.$prototyping) return this; this.$caller = null; var e = this.initialize ? this.initialize.apply(this, arguments) : this; return this.$caller = this.caller = null, e }.extend(this).implement(r); return i.$constructor = e, i.prototype.$constructor = i, i.prototype.parent = t, i }), t = function () { if (!this.$caller) throw new Error('The method "parent" cannot be called.'); var e = this.$caller.$name, t = this.$caller.$owner.parent, n = t ? t.prototype[e] : null; if (!n) throw new Error('The method "' + e + '" has no parent.'); return n.apply(this, arguments) }, n = function (e) { for (var t in e) { var r = e[t]; switch (typeOf(r)) { case "object": var i = function () { }; i.prototype = r, e[t] = n(new i); break; case "array": e[t] = r.clone() } } return e }, r = function (e, t, n) { n.$origin && (n = n.$origin); var r = function () { if (n.$protected && null == this.$caller) throw new Error('The method "' + t + '" cannot be called.'); var e = this.caller, i = this.$caller; this.caller = i, this.$caller = r; var o = n.apply(this, arguments); return this.$caller = i, this.caller = e, o }.extend({ $owner: e, $origin: n, $name: t }); return r }, i = function (t, n, i) { if (e.Mutators.hasOwnProperty(t) && null == (n = e.Mutators[t].call(this, n))) return this; if ("function" == typeOf(n)) { if (n.$hidden) return this; this.prototype[t] = i ? n : r(this, t, n) } else Object.merge(this.prototype, t, n); return this }, o = function (e) { e.$prototyping = !0; var t = new e; return delete e.$prototyping, t }; e.implement("implement", i.overloadSetter()), e.Mutators = { Extends: function (e) { this.parent = e, this.prototype = o(e) }, Implements: function (e) { Array.from(e).each(function (e) { var t = new e; for (var n in t) i.call(this, n, t[n], !0) }, this) } } }(), function () { this.Chain = new Class({ $chain: [], chain: function () { return this.$chain.append(Array.flatten(arguments)), this }, callChain: function () { return !!this.$chain.length && this.$chain.shift().apply(this, arguments) }, clearChain: function () { return this.$chain.empty(), this } }); var e = function (e) { return e.replace(/^on([A-Z])/, function (e, t) { return t.toLowerCase() }) }; this.Events = new Class({ $events: {}, addEvent: function (t, n, r) { return t = e(t), this.$events[t] = (this.$events[t] || []).include(n), r && (n.internal = !0), this }, addEvents: function (e) { for (var t in e) this.addEvent(t, e[t]); return this }, fireEvent: function (t, n, r) { t = e(t); var i = this.$events[t]; return i ? (n = Array.from(n), i.each(function (e) { r ? e.delay(r, this, n) : e.apply(this, n) }, this), this) : this }, removeEvent: function (t, n) { t = e(t); var r = this.$events[t]; if (r && !n.internal) { var i = r.indexOf(n); -1 != i && delete r[i] } return this }, removeEvents: function (t) { var n; if ("object" == typeOf(t)) { for (n in t) this.removeEvent(n, t[n]); return this } t && (t = e(t)); for (n in this.$events) if (!t || t == n) for (var r = this.$events[n], i = r.length; i--;) this.removeEvent(n, r[i]); return this } }), this.Options = new Class({ setOptions: function () { var e = this.options = Object.merge.apply(null, [{}, this.options].append(arguments)); if (!this.addEvent) return this; for (var t in e) "function" == typeOf(e[t]) && /^on[A-Z]/.test(t) && (this.addEvent(t, e[t]), delete e[t]); return this } }) }(), function () { var e = this.document, t = e.window = this, n = 1; this.$uid = t.ActiveXObject ? function (e) { return (e.uid || (e.uid = [n++]))[0] } : function (e) { return e.uid || (e.uid = n++) }, $uid(t), $uid(e); var r = navigator.userAgent.toLowerCase(), i = navigator.platform.toLowerCase(), o = r.match(/(opera|ie|firefox|chrome|version)[\s\/:]([\w\d\.]+)?.*?(safari|version[\s\/:]([\w\d\.]+)|$)/) || [null, "unknown", 0], s = "ie" == o[1] && e.documentMode, a = this.Browser = { extend: Function.prototype.extend, name: "version" == o[1] ? o[3] : o[1], version: s || parseFloat("opera" == o[1] && o[4] ? o[4] : o[2]), Platform: { name: r.match(/ip(?:ad|od|hone)/) ? "ios" : (r.match(/(?:webos|android)/) || i.match(/mac|win|linux/) || ["other"])[0] }, Features: { xpath: !!e.evaluate, air: !!t.runtime, query: !!e.querySelector, json: !!t.JSON }, Plugins: {} }; a[a.name] = !0, a[a.name + parseInt(a.version, 10)] = !0, a.Platform[a.Platform.name] = !0, a.Request = function () { var e = function () { return new XMLHttpRequest }, t = function () { return new ActiveXObject("MSXML2.XMLHTTP") }, n = function () { return new ActiveXObject("Microsoft.XMLHTTP") }; return Function.attempt(function () { return e(), e }, function () { return t(), t }, function () { return n(), n }) }(), a.Features.xhr = !!a.Request; var u = (Function.attempt(function () { return navigator.plugins["Shockwave Flash"].description }, function () { return new ActiveXObject("ShockwaveFlash.ShockwaveFlash").GetVariable("$version") }) || "0 r0").match(/\d+/g); if (a.Plugins.Flash = { version: Number(u[0] || "0." + u[1]) || 0, build: Number(u[2]) || 0 }, a.exec = function (n) { if (!n) return n; if (t.execScript) t.execScript(n); else { var r = e.createElement("script"); r.setAttribute("type", "text/javascript"), r.text = n, e.head.appendChild(r), e.head.removeChild(r) } return n }, String.implement("stripScripts", function (e) { var t = "", n = this.replace(/<script[^>]*>([\s\S]*?)<\/script>/gi, function (e, n) { return t += n + "\n", "" }); return !0 === e ? a.exec(t) : "function" == typeOf(e) && e(t, n), n }), a.extend({ Document: this.Document, Window: this.Window, Element: this.Element, Event: this.Event }), this.Window = this.$constructor = new Type("Window", function () { }), this.$family = Function.from("window").hide(), Window.mirror(function (e, n) { t[e] = n }), this.Document = e.$constructor = new Type("Document", function () { }), e.$family = Function.from("document").hide(), Document.mirror(function (t, n) { e[t] = n }), e.html = e.documentElement, e.head = e.getElementsByTagName("head")[0], e.execCommand) try { e.execCommand("BackgroundImageCache", !1, !0) } catch (e) { } if (this.attachEvent && !this.addEventListener) { var c = function () { this.detachEvent("onunload", c), e.head = e.html = e.window = null }; this.attachEvent("onunload", c) } var l = Array.from; try { l(e.html.childNodes) } catch (e) { Array.from = function (e) { if ("string" != typeof e && Type.isEnumerable(e) && "array" != typeOf(e)) { for (var t = e.length, n = new Array(t) ; t--;) n[t] = e[t]; return n } return l(e) }; var h = Array.prototype, f = h.slice;["pop", "push", "reverse", "shift", "sort", "splice", "unshift", "concat", "join", "slice"].each(function (e) { var t = h[e]; Array[e] = function (e) { return t.apply(Array.from(e), f.call(arguments, 1)) } }) } }(), function () { function e(e, o, s, u, l, f, d, p, m, v, g, y, b, E, x) { if ((o || -1 === n) && (t.expressions[++n] = [], r = -1, o)) return ""; if (s || u || -1 === r) { s = s || " "; var w = t.expressions[n]; i && w[r] && (w[r].reverseCombinator = c(s)), w[++r] = { combinator: s, tag: "*" } } var S = t.expressions[n][r]; if (l) S.tag = l.replace(a, ""); else if (f) S.id = f.replace(a, ""); else if (d) d = d.replace(a, ""), S.classList || (S.classList = []), S.classes || (S.classes = []), S.classList.push(d), S.classes.push({ value: d, regexp: new RegExp("(^|\\s)" + h(d) + "(\\s|$)") }); else if (y) x = (x = x || E) ? x.replace(a, "") : null, S.pseudos || (S.pseudos = []), S.pseudos.push({ key: y.replace(a, ""), value: x }); else if (p) { p = p.replace(a, ""), g = (g || "").replace(a, ""); var T, k; switch (m) { case "^=": k = new RegExp("^" + h(g)); break; case "$=": k = new RegExp(h(g) + "$"); break; case "~=": k = new RegExp("(^|\\s)" + h(g) + "(\\s|$)"); break; case "|=": k = new RegExp("^" + h(g) + "(-|$)"); break; case "=": T = function (e) { return g == e }; break; case "*=": T = function (e) { return e && e.indexOf(g) > -1 }; break; case "!=": T = function (e) { return g != e }; break; default: T = function (e) { return !!e } } "" == g && /^[*$^]=$/.test(m) && (T = function () { return !1 }), T || (T = function (e) { return e && k.test(e) }), S.attributes || (S.attributes = []), S.attributes.push({ key: p, operator: m, value: g, test: T }) } return "" } var t, n, r, i, o = {}, s = {}, a = /\\/g, u = function (r, a) { if (null == r) return null; if (!0 === r.Slick) return r; r = ("" + r).replace(/^\s+|\s+$/g, ""); var c = (i = !!a) ? s : o; if (c[r]) return c[r]; for (t = { Slick: !0, expressions: [], raw: r, reverse: function () { return u(this.raw, !0) } }, n = -1; r != (r = r.replace(f, e)) ;); return t.length = t.expressions.length, c[r] = i ? l(t) : t }, c = function (e) { return "!" === e ? " " : " " === e ? "!" : /^!/.test(e) ? e.replace(/^!/, "") : "!" + e }, l = function (e) { for (var t = e.expressions, n = 0; n < t.length; n++) { for (var r = t[n], i = { parts: [], tag: "*", combinator: c(r[0].combinator) }, o = 0; o < r.length; o++) { var s = r[o]; s.reverseCombinator || (s.reverseCombinator = " "), s.combinator = s.reverseCombinator, delete s.reverseCombinator } r.reverse().push(i) } return e }, h = function (e) { return e.replace(/[-[\]{}()*+?.\\^$|,#\s]/g, "\\$&") }, f = new RegExp("^(?:\\s*(,)\\s*|\\s*(<combinator>+)\\s*|(\\s+)|(<unicode>+|\\*)|\\#(<unicode>+)|\\.(<unicode>+)|\\[\\s*(<unicode1>+)(?:\\s*([*^$!~|]?=)(?:\\s*(?:([\"']?)(.*?)\\9)))?\\s*\\](?!\\])|:+(<unicode>+)(?:\\((?:(?:([\"'])([^\\12]*)\\12)|((?:\\([^)]+\\)|[^()]*)+))\\))?)".replace(/<combinator>/, "[" + h(">+~`!@$%^&={}\\;</") + "]").replace(/<unicode>/g, "(?:[\\w\\u00a1-\\uFFFF-]|\\\\[^\\s0-9a-f])").replace(/<unicode1>/g, "(?:[:\\w\\u00a1-\\uFFFF-]|\\\\[^\\s0-9a-f])")), d = this.Slick || {}; d.parse = function (e) { return u(e) }, d.escapeRegExp = h, this.Slick || (this.Slick = d) }.apply("undefined" != typeof exports ? exports : this), function () { var e = {}; e.isNativeCode = function (e) { return /\{\s*\[native code\]\s*\}/.test("" + e) }, e.isXML = function (e) { return !!e.xmlVersion || !!e.xml || "[object XMLDocument]" === Object.prototype.toString.call(e) || 9 === e.nodeType && "HTML" !== e.documentElement.nodeName }, e.setDocument = function (e) { if (9 === e.nodeType); else if (e.ownerDocument) e = e.ownerDocument; else { if (!e.navigator) return; e = e.document } if (this.document !== e) { this.document = e; var t = this.root = e.documentElement; this.isXMLDocument = this.isXML(e), this.brokenStarGEBTN = this.starSelectsClosedQSA = this.idGetsName = this.brokenMixedCaseQSA = this.brokenGEBCN = this.brokenCheckedQSA = this.brokenEmptyAttributeQSA = this.isHTMLDocument = !1; var n, r, i, o, s, a, u = e.createElement("div"); t.appendChild(u); try { a = "slick_getbyid_test", u.innerHTML = '<a id="' + a + '"></a>', this.isHTMLDocument = !!e.getElementById(a) } catch (e) { } if (this.isHTMLDocument) { u.style.display = "none", u.appendChild(e.createComment("")), r = u.getElementsByTagName("*").length > 0; try { u.innerHTML = "foo</foo>", n = (s = u.getElementsByTagName("*")) && s.length && "/" == s[0].nodeName.charAt(0) } catch (e) { } if (this.brokenStarGEBTN = r || n, u.querySelectorAll) try { u.innerHTML = "foo</foo>", s = u.querySelectorAll("*"), this.starSelectsClosedQSA = s && s.length && "/" == s[0].nodeName.charAt(0) } catch (e) { } try { a = "slick_id_gets_name", u.innerHTML = '<a name="' + a + '"></a><b id="' + a + '"></b>', this.idGetsName = e.getElementById(a) === u.firstChild } catch (e) { } try { u.innerHTML = '<a class="MiXedCaSe"></a>', this.brokenMixedCaseQSA = !u.querySelectorAll(".MiXedCaSe").length } catch (e) { } try { u.innerHTML = '<a class="f"></a><a class="b"></a>', u.getElementsByClassName("b").length, u.firstChild.className = "b", o = 2 != u.getElementsByClassName("b").length } catch (e) { } try { u.innerHTML = '<a class="a"></a><a class="f b a"></a>', i = 2 != u.getElementsByClassName("a").length } catch (e) { } this.brokenGEBCN = o || i; try { u.innerHTML = '<select><option selected="selected">a</option></select>', this.brokenCheckedQSA = 0 == u.querySelectorAll(":checked").length } catch (e) { } try { u.innerHTML = '<a class=""></a>', this.brokenEmptyAttributeQSA = 0 != u.querySelectorAll('[class*=""]').length } catch (e) { } } t.removeChild(u), u = null, this.hasAttribute = t && this.isNativeCode(t.hasAttribute) ? function (e, t) { return e.hasAttribute(t) } : function (e, t) { return !(!(e = e.getAttributeNode(t)) || !e.specified && !e.nodeValue) }, this.contains = t && this.isNativeCode(t.contains) ? function (e, t) { return e.contains(t) } : t && t.compareDocumentPosition ? function (e, t) { return e === t || !!(16 & e.compareDocumentPosition(t)) } : function (e, t) { if (t) do { if (t === e) return !0 } while (t = t.parentNode); return !1 }, this.documentSorter = t.compareDocumentPosition ? function (e, t) { return e.compareDocumentPosition && t.compareDocumentPosition ? 4 & e.compareDocumentPosition(t) ? -1 : e === t ? 0 : 1 : 0 } : "sourceIndex" in t ? function (e, t) { return e.sourceIndex && t.sourceIndex ? e.sourceIndex - t.sourceIndex : 0 } : e.createRange ? function (e, t) { if (!e.ownerDocument || !t.ownerDocument) return 0; var n = e.ownerDocument.createRange(), r = t.ownerDocument.createRange(); return n.setStart(e, 0), n.setEnd(e, 0), r.setStart(t, 0), r.setEnd(t, 0), n.compareBoundaryPoints(Range.START_TO_END, r) } : null, this.getUID = this.isHTMLDocument ? this.getUIDHTML : this.getUIDXML } }, e.search = function (e, t, n, r) { var i = this.found = r ? null : n || []; if (!e) return i; if (e.navigator) e = e.document; else if (!e.nodeType) return i; var o, s, a = this.uniques = {}; this.document !== (e.ownerDocument || e) && this.setDocument(e); var u = !(!n || !n.length); if (u) for (s = i.length; s--;) this.uniques[this.getUID(i[s])] = !0; if ("string" == typeof t) { for (s = this.overrides.length; s--;) { var c = this.overrides[s]; if (c.regexp.test(t)) { var l = c.method.call(e, t, i, r); if (!1 === l) continue; return !0 === l ? i : l } } if (!(o = this.Slick.parse(t)).length) return i } else { if (null == t) return i; if (!t.Slick) return this.contains(e.documentElement || e, t) ? (i ? i.push(t) : i = t, i) : i; o = t } this.posNTH = {}, this.posNTHLast = {}, this.posNTHType = {}, this.posNTHTypeLast = {}, this.push = !u && (r || 1 == o.length && 1 == o.expressions[0].length) ? this.pushArray : this.pushUID, null == i && (i = []); var h, f, d, p, m, v, g, y, b, E, x, w, S, T, k = o.expressions; e: for (s = 0; w = k[s]; s++) for (h = 0; S = w[h]; h++) { if (p = "combinator:" + S.combinator, !this[p]) continue e; if (m = this.isXMLDocument ? S.tag : S.tag.toUpperCase(), v = S.id, g = S.classList, y = S.classes, b = S.attributes, E = S.pseudos, T = h === w.length - 1, this.bitUniques = {}, T ? (this.uniques = a, this.found = i) : (this.uniques = {}, this.found = []), 0 === h) { if (this[p](e, m, v, y, b, E, g), r && T && i.length) break e } else if (r && T) { for (f = 0, d = x.length; f < d; f++) if (this[p](x[f], m, v, y, b, E, g), i.length) break e } else for (f = 0, d = x.length; f < d; f++) this[p](x[f], m, v, y, b, E, g); x = this.found } return (u || o.expressions.length > 1) && this.sort(i), r ? i[0] || null : i }, e.uidx = 1, e.uidk = "slick:uniqueid", e.getUIDXML = function (e) { var t = e.getAttribute(this.uidk); return t || (t = this.uidx++, e.setAttribute(this.uidk, t)), t }, e.getUIDHTML = function (e) { return e.uniqueNumber || (e.uniqueNumber = this.uidx++) }, e.sort = function (e) { return this.documentSorter ? (e.sort(this.documentSorter), e) : e }, e.cacheNTH = {}, e.matchNTH = /^([+-]?\d*)?([a-z]+)?([+-]\d+)?$/, e.parseNTHArgument = function (e) { var t = e.match(this.matchNTH); if (!t) return !1; var n = t[2] || !1, r = t[1] || 1; "-" == r && (r = -1); var i = +t[3] || 0; return t = "n" == n ? { a: r, b: i } : "odd" == n ? { a: 2, b: 1 } : "even" == n ? { a: 2, b: 0 } : { a: 0, b: r }, this.cacheNTH[e] = t }, e.createNTHPseudo = function (e, t, n, r) { return function (i, o) { var s = this.getUID(i); if (!this[n][s]) { var a = i.parentNode; if (!a) return !1; var u = a[e], c = 1; if (r) { var l = i.nodeName; do { u.nodeName === l && (this[n][this.getUID(u)] = c++) } while (u = u[t]) } else do { 1 === u.nodeType && (this[n][this.getUID(u)] = c++) } while (u = u[t]) } o = o || "n"; var h = this.cacheNTH[o] || this.parseNTHArgument(o); if (!h) return !1; var f = h.a, d = h.b, p = this[n][s]; if (0 == f) return d == p; if (f > 0) { if (p < d) return !1 } else if (d < p) return !1; return (p - d) % f == 0 } }, e.pushArray = function (e, t, n, r, i, o) { this.matchSelector(e, t, n, r, i, o) && this.found.push(e) }, e.pushUID = function (e, t, n, r, i, o) { var s = this.getUID(e); !this.uniques[s] && this.matchSelector(e, t, n, r, i, o) && (this.uniques[s] = !0, this.found.push(e)) }, e.matchNode = function (e, t) { var n = this.Slick.parse(t); if (!n) return !0; if (1 == n.length && 1 == n.expressions[0].length) { var r = n.expressions[0][0]; return this.matchSelector(e, this.isXMLDocument ? r.tag : r.tag.toUpperCase(), r.id, r.classes, r.attributes, r.pseudos) } for (var i, o = this.search(this.document, n), s = 0; i = o[s++];) if (i === e) return !0; return !1 }, e.matchPseudo = function (e, t, n) { var r = "pseudo:" + t; if (this[r]) return this[r](e, n); var i = this.getAttribute(e, t); return n ? n == i : !!i }, e.matchSelector = function (e, t, n, r, i, o) { if (t) if ("*" == t) { if (e.nodeName < "@") return !1 } else if (e.nodeName != t) return !1; if (n && e.getAttribute("id") != n) return !1; var s, a, u; if (r) for (s = r.length; s--;) if (!(u = "className" in e ? e.className : e.getAttribute("class")) || !r[s].regexp.test(u)) return !1; if (i) for (s = i.length; s--;) if (a = i[s], a.operator ? !a.test(this.getAttribute(e, a.key)) : !this.hasAttribute(e, a.key)) return !1; if (o) for (s = o.length; s--;) if (a = o[s], !this.matchPseudo(e, a.key, a.value)) return !1; return !0 }; var t = { " ": function (e, t, n, r, i, o, s) { var a, u, c; if (this.isHTMLDocument) { e: if (n) { if (!(u = this.document.getElementById(n)) && e.all || this.idGetsName && u && u.getAttributeNode("id").nodeValue != n) { if (!(c = e.all[n])) return; for (c[0] || (c = [c]), a = 0; u = c[a++];) if (u.getAttributeNode("id").nodeValue == n) { this.push(u, t, null, r, i, o); break } return } if (!u) { if (this.contains(this.document.documentElement, e)) return; break e } if (this.document !== e && !this.contains(e, u)) return; return void this.push(u, t, null, r, i, o) }e: if (r && e.getElementsByClassName && !this.brokenGEBCN) { if (!(c = e.getElementsByClassName(s.join(" "))) || !c.length) break e; for (a = 0; u = c[a++];) this.push(u, t, n, null, i, o); return } } if ((c = e.getElementsByTagName(t)) && c.length) for (this.brokenStarGEBTN || (t = null), a = 0; u = c[a++];) this.push(u, t, n, r, i, o) }, ">": function (e, t, n, r, i, o) { if (e = e.firstChild) do { 1 === e.nodeType && this.push(e, t, n, r, i, o) } while (e = e.nextSibling) }, "+": function (e, t, n, r, i, o) { for (; e = e.nextSibling;) if (1 === e.nodeType) { this.push(e, t, n, r, i, o); break } }, "^": function (e, t, n, r, i, o) { (e = e.firstChild) && (1 === e.nodeType ? this.push(e, t, n, r, i, o) : this["combinator:+"](e, t, n, r, i, o)) }, "~": function (e, t, n, r, i, o) { for (; e = e.nextSibling;) if (1 === e.nodeType) { var s = this.getUID(e); if (this.bitUniques[s]) break; this.bitUniques[s] = !0, this.push(e, t, n, r, i, o) } }, "++": function (e, t, n, r, i, o) { this["combinator:+"](e, t, n, r, i, o), this["combinator:!+"](e, t, n, r, i, o) }, "~~": function (e, t, n, r, i, o) { this["combinator:~"](e, t, n, r, i, o), this["combinator:!~"](e, t, n, r, i, o) }, "!": function (e, t, n, r, i, o) { for (; e = e.parentNode;) e !== this.document && this.push(e, t, n, r, i, o) }, "!>": function (e, t, n, r, i, o) { (e = e.parentNode) !== this.document && this.push(e, t, n, r, i, o) }, "!+": function (e, t, n, r, i, o) { for (; e = e.previousSibling;) if (1 === e.nodeType) { this.push(e, t, n, r, i, o); break } }, "!^": function (e, t, n, r, i, o) { (e = e.lastChild) && (1 === e.nodeType ? this.push(e, t, n, r, i, o) : this["combinator:!+"](e, t, n, r, i, o)) }, "!~": function (e, t, n, r, i, o) { for (; e = e.previousSibling;) if (1 === e.nodeType) { var s = this.getUID(e); if (this.bitUniques[s]) break; this.bitUniques[s] = !0, this.push(e, t, n, r, i, o) } } }; for (var n in t) e["combinator:" + n] = t[n]; var r = { empty: function (e) { var t = e.firstChild; return !(t && 1 == t.nodeType || (e.innerText || e.textContent || "").length) }, not: function (e, t) { return !this.matchNode(e, t) }, contains: function (e, t) { return (e.innerText || e.textContent || "").indexOf(t) > -1 }, "first-child": function (e) { for (; e = e.previousSibling;) if (1 === e.nodeType) return !1; return !0 }, "last-child": function (e) { for (; e = e.nextSibling;) if (1 === e.nodeType) return !1; return !0 }, "only-child": function (e) { for (var t = e; t = t.previousSibling;) if (1 === t.nodeType) return !1; for (var n = e; n = n.nextSibling;) if (1 === n.nodeType) return !1; return !0 }, "nth-child": e.createNTHPseudo("firstChild", "nextSibling", "posNTH"), "nth-last-child": e.createNTHPseudo("lastChild", "previousSibling", "posNTHLast"), "nth-of-type": e.createNTHPseudo("firstChild", "nextSibling", "posNTHType", !0), "nth-last-of-type": e.createNTHPseudo("lastChild", "previousSibling", "posNTHTypeLast", !0), index: function (e, t) { return this["pseudo:nth-child"](e, "" + t + 1) }, even: function (e, t) { return this["pseudo:nth-child"](e, "2n") }, odd: function (e, t) { return this["pseudo:nth-child"](e, "2n+1") }, "first-of-type": function (e) { for (var t = e.nodeName; e = e.previousSibling;) if (e.nodeName === t) return !1; return !0 }, "last-of-type": function (e) { for (var t = e.nodeName; e = e.nextSibling;) if (e.nodeName === t) return !1; return !0 }, "only-of-type": function (e) { for (var t = e, n = e.nodeName; t = t.previousSibling;) if (t.nodeName === n) return !1; for (var r = e; r = r.nextSibling;) if (r.nodeName === n) return !1; return !0 }, enabled: function (e) { return !1 === e.disabled }, disabled: function (e) { return !0 === e.disabled }, checked: function (e) { return e.checked || e.selected }, focus: function (e) { return this.isHTMLDocument && this.document.activeElement === e && (e.href || e.type || this.hasAttribute(e, "tabindex")) }, root: function (e) { return e === this.root }, selected: function (e) { return e.selected } }; for (var i in r) e["pseudo:" + i] = r[i]; e.attributeGetters = { class: function () { return "className" in this ? this.className : this.getAttribute("class") }, for: function () { return "htmlFor" in this ? this.htmlFor : this.getAttribute("for") }, href: function () { return "href" in this ? this.getAttribute("href", 2) : this.getAttribute("href") }, style: function () { return this.style ? this.style.cssText : this.getAttribute("style") } }, e.getAttribute = function (e, t) { var n = this.attributeGetters[t]; if (n) return n.call(e); var r = e.getAttributeNode(t); return r ? r.nodeValue : null }, e.overrides = [], e.override = function (e, t) { this.overrides.push({ regexp: e, method: t }) }; var o = /\[.*[*$^]=(?:["']{2})?\]/; e.override(/./, function (t, n, r) { if (!this.querySelectorAll || 9 != this.nodeType || !e.isHTMLDocument || e.brokenMixedCaseQSA || e.brokenCheckedQSA && t.indexOf(":checked") > -1 || e.brokenEmptyAttributeQSA && o.test(t) || s.disableQSA) return !1; var i, a; try { if (r) return this.querySelector(t) || null; i = this.querySelectorAll(t) } catch (e) { return !1 } var u, c = !!n.length; if (e.starSelectsClosedQSA) for (u = 0; a = i[u++];) !(a.nodeName > "@") || c && e.uniques[e.getUIDHTML(a)] || n.push(a); else for (u = 0; a = i[u++];) c && e.uniques[e.getUIDHTML(a)] || n.push(a); return c && e.sort(n), !0 }), e.override(/^[\w-]+$|^\*$/, function (t, n, r) { var i = t; if ("*" == i && e.brokenStarGEBTN) return !1; var o = this.getElementsByTagName(i); if (r) return o[0] || null; var s, a, u = !!n.length; for (s = 0; a = o[s++];) u && e.uniques[e.getUID(a)] || n.push(a); return u && e.sort(n), !0 }), e.override(/^\.[\w-]+$/, function (t, n, r) { if (!e.isHTMLDocument || !this.getElementsByClassName && this.querySelectorAll) return !1; var i, o, a, u = !(!n || !n.length), c = t.substring(1); if (this.getElementsByClassName && !e.brokenGEBCN) { if (i = this.getElementsByClassName(c), r) return i[0] || null; for (a = 0; o = i[a++];) u && e.uniques[e.getUIDHTML(o)] || n.push(o) } else { var l = new RegExp("(^|\\s)" + s.escapeRegExp(c) + "(\\s|$)"); for (i = this.getElementsByTagName("*"), a = 0; o = i[a++];) if ((c = o.className) && l.test(c)) { if (r) return o; u && e.uniques[e.getUIDHTML(o)] || n.push(o) } } return u && e.sort(n), !r || null }), e.override(/^#[\w-]+$/, function (t, n, r) { if (!e.isHTMLDocument || 9 != this.nodeType) return !1; var i = t.substring(1), o = this.getElementById(i); if (!o) return n; if (e.idGetsName && o.getAttributeNode("id").nodeValue != i) return !1; if (r) return o || null; var s = !!n.length; return s && e.uniques[e.getUIDHTML(o)] || n.push(o), s && e.sort(n), !0 }), "undefined" != typeof document && e.setDocument(document); var s = e.Slick = this.Slick || {}; s.version = "0.9dev", s.search = function (t, n, r) { return e.search(t, n, r) }, s.find = function (t, n) { return e.search(t, n, null, !0) }, s.contains = function (t, n) { return e.setDocument(t), e.contains(t, n) }, s.getAttribute = function (t, n) { return e.getAttribute(t, n) }, s.match = function (t, n) { return !(!t || !n) && (!n || n === t || "string" == typeof n && (e.setDocument(t), e.matchNode(t, n))) }, s.defineAttributeGetter = function (t, n) { return e.attributeGetters[t] = n, this }, s.lookupAttributeGetter = function (t) { return e.attributeGetters[t] }, s.definePseudo = function (t, n) { return e["pseudo:" + t] = function (e, t) { return n.call(e, t) }, this }, s.lookupPseudo = function (t) { var n = e["pseudo:" + t]; return n ? function (e) { return n.call(this, e) } : null }, s.override = function (t, n) { return e.override(t, n), this }, s.isXML = e.isXML, s.uidOf = function (t) { return e.getUIDHTML(t) }, this.Slick || (this.Slick = s) }.apply("undefined" != typeof exports ? exports : this); var Element = function (e, t) { var n = Element.Constructors[e]; if (n) return n(t); if ("string" != typeof e) return document.id(e).set(t); if (t || (t = {}), !e.test(/^[\w-]+$/)) { var r = Slick.parse(e).expressions[0][0]; e = "*" == r.tag ? "div" : r.tag, r.id && null == t.id && (t.id = r.id); var i = r.attributes; if (i) for (var o = 0, s = i.length; o < s; o++) { var a = i[o]; null != a.value && "=" == a.operator && null == t[a.key] && (t[a.key] = a.value) } r.classList && null == t.class && (t.class = r.classList.join(" ")) } return document.newElement(e, t) }; Browser.Element && (Element.prototype = Browser.Element.prototype), new Type("Element", Element).mirror(function (e) { if (!Array.prototype[e]) { var t = {}; t[e] = function () { for (var t = [], n = arguments, r = !0, i = 0, o = this.length; i < o; i++) { var s = this[i], a = t[i] = s[e].apply(s, n); r = r && "element" == typeOf(a) } return r ? new Elements(t) : t }, Elements.implement(t) } }), Browser.Element || (Element.parent = Object, Element.Prototype = { $family: Function.from("element").hide() }, Element.mirror(function (e, t) { Element.Prototype[e] = t })), Element.Constructors = {}; var IFrame = new Type("IFrame", function () { var e, t = Array.link(arguments, { properties: Type.isObject, iframe: function (e) { return null != e } }), n = t.properties || {}; t.iframe && (e = document.id(t.iframe)); var r = n.onload || function () { }; delete n.onload, n.id = n.name = [n.id, n.name, e ? e.id || e.name : "IFrame_" + String.generateUID()].pick(), e = new Element(e || "iframe", n); var i = function () { r.call(e.contentWindow) }; return window.frames[n.id] ? i() : e.addListener("load", i), e }), Elements = this.Elements = function (e) { if (e && e.length) for (var t, n = {}, r = 0; t = e[r++];) { var i = Slick.uidOf(t); n[i] || (n[i] = !0, this.push(t)) } }; Elements.prototype = { length: 0 }, Elements.parent = Array, new Type("Elements", Elements).implement({ filter: function (e, t) { return e ? new Elements(Array.filter(this, "string" == typeOf(e) ? function (t) { return t.match(e) } : e, t)) : this }.protect(), push: function () { for (var e = this.length, t = 0, n = arguments.length; t < n; t++) { var r = document.id(arguments[t]); r && (this[e++] = r) } return this.length = e }.protect(), concat: function () { for (var e = new Elements(this), t = 0, n = arguments.length; t < n; t++) { var r = arguments[t]; Type.isEnumerable(r) ? e.append(r) : e.push(r) } return e }.protect(), append: function (e) { for (var t = 0, n = e.length; t < n; t++) this.push(e[t]); return this }.protect(), empty: function () { for (; this.length;) delete this[--this.length]; return this }.protect() }), function () { var e = Array.prototype.splice, t = { 0: 0, 1: 1, length: 2 }; e.call(t, 1, 1), 1 == t[1] && Elements.implement("splice", function () { var t = this.length; for (e.apply(this, arguments) ; t >= this.length;) delete this[t--]; return this }.protect()), Elements.implement(Array.prototype), Array.mirror(Elements); var n; try { var r = document.createElement("<input name=x>"); n = "x" == r.name } catch (e) { } var i = function (e) { return ("" + e).replace(/&/g, "&amp;").replace(/"/g, "&quot;") }; Document.implement({ newElement: function (e, t) { return t && null != t.checked && (t.defaultChecked = t.checked), n && t && (e = "<" + e, t.name && (e += ' name="' + i(t.name) + '"'), t.type && (e += ' type="' + i(t.type) + '"'), e += ">", delete t.name, delete t.type), this.id(this.createElement(e)).set(t) } }) }(), Document.implement({ newTextNode: function (e) { return this.createTextNode(e) }, getDocument: function () { return this }, getWindow: function () { return this.window }, id: function () { var e = { string: function (t, n, r) { return t = Slick.find(r, "#" + t.replace(/(\W)/g, "\\$1")), t ? e.element(t, n) : null }, element: function (e, t) { return $uid(e), t || e.$family || /^object|embed$/i.test(e.tagName) || Object.append(e, Element.Prototype), e }, object: function (t, n, r) { return t.toElement ? e.element(t.toElement(r), n) : null } }; return e.textnode = e.whitespace = e.window = e.document = function (e) { return e }, function (t, n, r) { if (t && t.$family && t.uid) return t; var i = typeOf(t); return e[i] ? e[i](t, n, r || document) : null } }() }), null == window.$ && Window.implement("$", function (e, t) { return document.id(e, t, this.document) }), Window.implement({ getDocument: function () { return this.document }, getWindow: function () { return this } }), [Document, Element].invoke("implement", { getElements: function (e) { return Slick.search(this, e, new Elements) }, getElement: function (e) { return document.id(Slick.find(this, e)) } }), null == window.$$ && Window.implement("$$", function (e) { if (1 == arguments.length) { if ("string" == typeof e) return Slick.search(this.document, e, new Elements); if (Type.isEnumerable(e)) return new Elements(e) } return new Elements(arguments) }), function () { var e = {}, t = {}, n = { input: "checked", option: "selected", textarea: "value" }, r = function (e) { return t[e] || (t[e] = {}) }, i = function (n) { n.removeEvents && n.removeEvents(), n.clearAttributes && n.clearAttributes(); var r = n.uid; return null != r && (delete e[r], delete t[r]), n }, o = ["defaultValue", "accessKey", "cellPadding", "cellSpacing", "colSpan", "frameBorder", "maxLength", "readOnly", "rowSpan", "tabIndex", "useMap"], s = ["compact", "nowrap", "ismap", "declare", "noshade", "checked", "disabled", "readOnly", "multiple", "selected", "noresize", "defer"], a = { html: "innerHTML", class: "className", for: "htmlFor", text: null == document.createElement("div").innerText ? "textContent" : "innerText" }, u = ["type"], c = ["value", "defaultValue"], l = /^(?:href|src|usemap)$/i; s = s.associate(s), o = o.associate(o.map(String.toLowerCase)), u = u.associate(u), Object.append(a, c.associate(c)); var h = { before: function (e, t) { var n = t.parentNode; n && n.insertBefore(e, t) }, after: function (e, t) { var n = t.parentNode; n && n.insertBefore(e, t.nextSibling) }, bottom: function (e, t) { t.appendChild(e) }, top: function (e, t) { t.insertBefore(e, t.firstChild) } }; h.inside = h.bottom; var f = function (e, t) { if (!e) return t; for (var n = (e = Slick.parse(e)).expressions, r = n.length; r--;) n[r][0].combinator = t; return e }; Element.implement({ set: function (e, t) { var n = Element.Properties[e]; n && n.set ? n.set.call(this, t) : this.setProperty(e, t) }.overloadSetter(), get: function (e) { var t = Element.Properties[e]; return t && t.get ? t.get.apply(this) : this.getProperty(e) }.overloadGetter(), erase: function (e) { var t = Element.Properties[e]; return t && t.erase ? t.erase.apply(this) : this.removeProperty(e), this }, setProperty: function (e, t) { if (e = o[e] || e, null == t) return this.removeProperty(e); var n = a[e]; return n ? this[n] = t : s[e] ? this[e] = !!t : this.setAttribute(e, "" + t), this }, setProperties: function (e) { for (var t in e) this.setProperty(t, e[t]); return this }, getProperty: function (e) { e = o[e] || e; var t = a[e] || u[e]; return t ? this[t] : s[e] ? !!this[e] : (l.test(e) ? this.getAttribute(e, 2) : (t = this.getAttributeNode(e)) ? t.nodeValue : null) || null }, getProperties: function () { var e = Array.from(arguments); return e.map(this.getProperty, this).associate(e) }, removeProperty: function (e) { e = o[e] || e; var t = a[e]; return t ? this[t] = "" : s[e] ? this[e] = !1 : this.removeAttribute(e), this }, removeProperties: function () { return Array.each(arguments, this.removeProperty, this), this }, hasClass: function (e) { return this.className.clean().contains(e, " ") }, addClass: function (e) { return this.hasClass(e) || (this.className = (this.className + " " + e).clean()), this }, removeClass: function (e) { return this.className = this.className.replace(new RegExp("(^|\\s)" + e + "(?:\\s|$)"), "$1"), this }, toggleClass: function (e, t) { return null == t && (t = !this.hasClass(e)), t ? this.addClass(e) : this.removeClass(e) }, adopt: function () { var e, t = this, n = Array.flatten(arguments), r = n.length; r > 1 && (t = e = document.createDocumentFragment()); for (var i = 0; i < r; i++) { var o = document.id(n[i], !0); o && t.appendChild(o) } return e && this.appendChild(e), this }, appendText: function (e, t) { return this.grab(this.getDocument().newTextNode(e), t) }, grab: function (e, t) { return h[t || "bottom"](document.id(e, !0), this), this }, inject: function (e, t) { return h[t || "bottom"](this, document.id(e, !0)), this }, replaces: function (e) { return (e = document.id(e, !0)).parentNode.replaceChild(this, e), this }, wraps: function (e, t) { return e = document.id(e, !0), this.replaces(e).grab(e, t) }, getPrevious: function (e) { return document.id(Slick.find(this, f(e, "!~"))) }, getAllPrevious: function (e) { return Slick.search(this, f(e, "!~"), new Elements) }, getNext: function (e) { return document.id(Slick.find(this, f(e, "~"))) }, getAllNext: function (e) { return Slick.search(this, f(e, "~"), new Elements) }, getFirst: function (e) { return document.id(Slick.search(this, f(e, ">"))[0]) }, getLast: function (e) { return document.id(Slick.search(this, f(e, ">")).getLast()) }, getParent: function (e) { return document.id(Slick.find(this, f(e, "!"))) }, getParents: function (e) { return Slick.search(this, f(e, "!"), new Elements) }, getSiblings: function (e) { return Slick.search(this, f(e, "~~"), new Elements) }, getChildren: function (e) { return Slick.search(this, f(e, ">"), new Elements) }, getWindow: function () { return this.ownerDocument.window }, getDocument: function () { return this.ownerDocument }, getElementById: function (e) { return document.id(Slick.find(this, "#" + ("" + e).replace(/(\W)/g, "\\$1"))) }, getSelected: function () { return this.selectedIndex, new Elements(Array.from(this.options).filter(function (e) { return e.selected })) }, toQueryString: function () { var e = []; return this.getElements("input, select, textarea").each(function (t) { var n = t.type; if (t.name && !t.disabled && "submit" != n && "reset" != n && "file" != n && "image" != n) { var r = "select" == t.get("tag") ? t.getSelected().map(function (e) { return document.id(e).get("value") }) : "radio" != n && "checkbox" != n || t.checked ? t.get("value") : null; Array.from(r).each(function (n) { void 0 !== n && e.push(encodeURIComponent(t.name) + "=" + encodeURIComponent(n)) }) } }), e.join("&") }, clone: function (e, t) { e = !1 !== e; var r, i = this.cloneNode(e), o = function (e, r) { if (t || e.removeAttribute("id"), Browser.ie && (e.clearAttributes(), e.mergeAttributes(r), e.removeAttribute("uid"), e.options)) for (var i = e.options, o = r.options, s = i.length; s--;) i[s].selected = o[s].selected; var a = n[r.tagName.toLowerCase()]; a && r[a] && (e[a] = r[a]) }; if (e) { var s = i.getElementsByTagName("*"), a = this.getElementsByTagName("*"); for (r = s.length; r--;) o(s[r], a[r]) } if (o(i, this), Browser.ie) { var u = this.getElementsByTagName("object"), c = i.getElementsByTagName("object"), l = u.length, h = c.length; for (r = 0; r < l && r < h; r++) c[r].outerHTML = u[r].outerHTML } return document.id(i) }, destroy: function () { var e = i(this).getElementsByTagName("*"); return Array.each(e, i), Element.dispose(this), null }, empty: function () { return Array.from(this.childNodes).each(Element.dispose), this }, dispose: function () { return this.parentNode ? this.parentNode.removeChild(this) : this }, match: function (e) { return !e || Slick.match(this, e) } }); var d = { contains: function (e) { return Slick.contains(this, e) } }; document.contains || Document.implement(d), document.createElement("div").contains || Element.implement(d), [Element, Window, Document].invoke("implement", { addListener: function (t, n) { if ("unload" == t) { var r = n, i = this; n = function () { i.removeListener("unload", n), r() } } else e[this.uid] = this; return this.addEventListener ? this.addEventListener(t, n, !1) : this.attachEvent("on" + t, n), this }, removeListener: function (e, t) { return this.removeEventListener ? this.removeEventListener(e, t, !1) : this.detachEvent("on" + e, t), this }, retrieve: function (e, t) { var n = r(this.uid), i = n[e]; return null != t && null == i && (i = n[e] = t), null != i ? i : null }, store: function (e, t) { return r(this.uid)[e] = t, this }, eliminate: function (e) { return delete r(this.uid)[e], this } }), window.attachEvent && !window.addEventListener && window.addListener("unload", function () { Object.each(e, i), window.CollectGarbage && CollectGarbage() }) }(), Element.Properties = {}, Element.Properties.style = { set: function (e) { this.style.cssText = e }, get: function () { return this.style.cssText }, erase: function () { this.style.cssText = "" } }, Element.Properties.tag = { get: function () { return this.tagName.toLowerCase() } }, function (e) { null != e && (Element.Properties.maxlength = Element.Properties.maxLength = { get: function () { var t = this.getAttribute("maxLength"); return t == e ? null : t } }) }(document.createElement("input").getAttribute("maxLength")), Element.Properties.html = function () { var e = Function.attempt(function () { document.createElement("table").innerHTML = "<tr><td></td></tr>" }), t = document.createElement("div"), n = { table: [1, "<table>", "</table>"], select: [1, "<select>", "</select>"], tbody: [2, "<table><tbody>", "</tbody></table>"], tr: [3, "<table><tbody><tr>", "</tr></tbody></table>"] }; n.thead = n.tfoot = n.tbody; var r = { set: function () { var r = Array.flatten(arguments).join(""), i = !e && n[this.get("tag")]; if (i) { var o = t; o.innerHTML = i[1] + r + i[2]; for (var s = i[0]; s--;) o = o.firstChild; this.empty().adopt(o.childNodes) } else this.innerHTML = r } }; return r.erase = r.set, r }(), function () { var e = document.html; Element.Properties.styles = { set: function (e) { this.setStyles(e) } }; var t = null != e.style.opacity, n = /alpha\(opacity=([\d.]+)\)/i, r = function (e, r) { if (e.currentStyle && e.currentStyle.hasLayout || (e.style.zoom = 1), t) e.style.opacity = r; else { r = 1 == r ? "" : "alpha(opacity=" + 100 * r + ")"; var i = e.style.filter || e.getComputedStyle("filter") || ""; e.style.filter = i.test(n) ? i.replace(n, r) : i + r } }; Element.Properties.opacity = { set: function (e) { var t = this.style.visibility; 0 == e && "hidden" != t ? this.style.visibility = "hidden" : 0 != e && "visible" != t && (this.style.visibility = "visible"), r(this, e) }, get: t ? function () { var e = this.style.opacity || this.getComputedStyle("opacity"); return "" == e ? 1 : e } : function () { var e, t = this.style.filter || this.getComputedStyle("filter"); return t && (e = t.match(n)), null == e || null == t ? 1 : e[1] / 100 } }; var i = null == e.style.cssFloat ? "styleFloat" : "cssFloat"; Element.implement({ getComputedStyle: function (e) { if (this.currentStyle) return this.currentStyle[e.camelCase()]; var t = Element.getDocument(this).defaultView, n = t ? t.getComputedStyle(this, null) : null; return n ? n.getPropertyValue(e == i ? "float" : e.hyphenate()) : null }, setOpacity: function (e) { return r(this, e), this }, getOpacity: function () { return this.get("opacity") }, setStyle: function (e, t) { switch (e) { case "opacity": return this.set("opacity", parseFloat(t)); case "float": e = i } if (e = e.camelCase(), "string" != typeOf(t)) { var n = (Element.Styles[e] || "@").split(" "); t = Array.from(t).map(function (e, t) { return n[t] ? "number" == typeOf(e) ? n[t].replace("@", Math.round(e)) : e : "" }).join(" ") } else t == String(Number(t)) && (t = Math.round(t)); return this.style[e] = t, this }, getStyle: function (e) { switch (e) { case "opacity": return this.get("opacity"); case "float": e = i } e = e.camelCase(); var t = this.style[e]; if (!t || "zIndex" == e) { t = []; for (var n in Element.ShortStyles) if (e == n) { for (var r in Element.ShortStyles[n]) t.push(this.getStyle(r)); return t.join(" ") } t = this.getComputedStyle(e) } if (t) { var o = (t = String(t)).match(/rgba?\([\d\s,]+\)/); o && (t = t.replace(o[0], o[0].rgbToHex())) } if (Browser.opera || Browser.ie && isNaN(parseFloat(t))) { if (e.test(/^(height|width)$/)) { var s = 0; return ("width" == e ? ["left", "right"] : ["top", "bottom"]).each(function (e) { s += this.getStyle("border-" + e + "-width").toInt() + this.getStyle("padding-" + e).toInt() }, this), this["offset" + e.capitalize()] - s + "px" } if (Browser.opera && -1 != String(t).indexOf("px")) return t; if (e.test(/(border(.+)Width|margin|padding)/)) return "0px" } return t }, setStyles: function (e) { for (var t in e) this.setStyle(t, e[t]); return this }, getStyles: function () { var e = {}; return Array.flatten(arguments).each(function (t) { e[t] = this.getStyle(t) }, this), e } }), Element.Styles = { left: "@px", top: "@px", bottom: "@px", right: "@px", width: "@px", height: "@px", maxWidth: "@px", maxHeight: "@px", minWidth: "@px", minHeight: "@px", backgroundColor: "rgb(@, @, @)", backgroundPosition: "@px @px", color: "rgb(@, @, @)", fontSize: "@px", letterSpacing: "@px", lineHeight: "@px", clip: "rect(@px @px @px @px)", margin: "@px @px @px @px", padding: "@px @px @px @px", border: "@px @ rgb(@, @, @) @px @ rgb(@, @, @) @px @ rgb(@, @, @)", borderWidth: "@px @px @px @px", borderStyle: "@ @ @ @", borderColor: "rgb(@, @, @) rgb(@, @, @) rgb(@, @, @) rgb(@, @, @)", zIndex: "@", zoom: "@", fontWeight: "@", textIndent: "@px", opacity: "@" }, Element.ShortStyles = { margin: {}, padding: {}, border: {}, borderWidth: {}, borderStyle: {}, borderColor: {} }, ["Top", "Right", "Bottom", "Left"].each(function (e) { var t = Element.ShortStyles, n = Element.Styles;["margin", "padding"].each(function (r) { var i = r + e; t[r][i] = n[i] = "@px" }); var r = "border" + e; t.border[r] = n[r] = "@px @ rgb(@, @, @)"; var i = r + "Width", o = r + "Style", s = r + "Color"; t[r] = {}, t.borderWidth[i] = t[r][i] = n[i] = "@px", t.borderStyle[o] = t[r][o] = n[o] = "@", t.borderColor[s] = t[r][s] = n[s] = "rgb(@, @, @)" }) }(), Object.extend({ subset: function (e, t) { for (var n = {}, r = 0, i = t.length; r < i; r++) { var o = t[r]; n[o] = e[o] } return n }, map: function (e, t, n) { var r = {}; for (var i in e) e.hasOwnProperty(i) && (r[i] = t.call(n, e[i], i, e)); return r }, filter: function (e, t, n) { var r = {}; return Object.each(e, function (i, o) { t.call(n, i, o, e) && (r[o] = i) }), r }, every: function (e, t, n) { for (var r in e) if (e.hasOwnProperty(r) && !t.call(n, e[r], r)) return !1; return !0 }, some: function (e, t, n) { for (var r in e) if (e.hasOwnProperty(r) && t.call(n, e[r], r)) return !0; return !1 }, keys: function (e) { var t = []; for (var n in e) e.hasOwnProperty(n) && t.push(n); return t }, values: function (e) { var t = []; for (var n in e) e.hasOwnProperty(n) && t.push(e[n]); return t }, getLength: function (e) { return Object.keys(e).length }, keyOf: function (e, t) { for (var n in e) if (e.hasOwnProperty(n) && e[n] === t) return n; return null }, contains: function (e, t) { return null != Object.keyOf(e, t) }, toQueryString: function (e, t) { var n = []; return Object.each(e, function (e, r) { t && (r = t + "[" + r + "]"); var i; switch (typeOf(e)) { case "object": i = Object.toQueryString(e, r); break; case "array": var o = {}; e.each(function (e, t) { o[t] = e }), i = Object.toQueryString(o, r); break; default: i = r + "=" + encodeURIComponent(e) } null != e && n.push(i) }), n.join("&") } }); var Event = new Type("Event", function (e, t) { t || (t = window); var n = t.document; if ((e = e || t.event).$extended) return e; this.$extended = !0; for (var r = e.type, i = e.target || e.srcElement, o = {}, s = {}; i && 3 == i.nodeType;) i = i.parentNode; if (-1 != r.indexOf("key")) { var a = e.which || e.keyCode, u = Object.keyOf(Event.Keys, a); if ("keydown" == r) { var c = a - 111; c > 0 && c < 13 && (u = "f" + c) } u || (u = String.fromCharCode(a).toLowerCase()) } else if (r.test(/click|mouse|menu/i)) { if (n = n.compatMode && "CSS1Compat" != n.compatMode ? n.body : n.html, o = { x: null != e.pageX ? e.pageX : e.clientX + n.scrollLeft, y: null != e.pageY ? e.pageY : e.clientY + n.scrollTop }, s = { x: null != e.pageX ? e.pageX - t.pageXOffset : e.clientX, y: null != e.pageY ? e.pageY - t.pageYOffset : e.clientY }, r.test(/DOMMouseScroll|mousewheel/)) var l = e.wheelDelta ? e.wheelDelta / 120 : -(e.detail || 0) / 3; var h = 3 == e.which || 2 == e.button, f = null; if (r.test(/over|out/)) { f = e.relatedTarget || e[("mouseover" == r ? "from" : "to") + "Element"]; var d = function () { for (; f && 3 == f.nodeType;) f = f.parentNode; return !0 }, p = Browser.firefox2 ? d.attempt() : d(); f = p ? f : null } } else if (r.test(/gesture|touch/i)) { this.rotation = e.rotation, this.scale = e.scale, this.targetTouches = e.targetTouches, this.changedTouches = e.changedTouches; var m = this.touches = e.touches; if (m && m[0]) { var v = m[0]; o = { x: v.pageX, y: v.pageY }, s = { x: v.clientX, y: v.clientY } } } return Object.append(this, { event: e, type: r, page: o, client: s, rightClick: h, wheel: l, relatedTarget: document.id(f), target: document.id(i), code: a, key: u, shift: e.shiftKey, control: e.ctrlKey, alt: e.altKey, meta: e.metaKey }) }); Event.Keys = { enter: 13, up: 38, down: 40, left: 37, right: 39, esc: 27, space: 32, backspace: 8, tab: 9, delete: 46 }, Event.implement({ stop: function () { return this.stopPropagation().preventDefault() }, stopPropagation: function () { return this.event.stopPropagation ? this.event.stopPropagation() : this.event.cancelBubble = !0, this }, preventDefault: function () { return this.event.preventDefault ? this.event.preventDefault() : this.event.returnValue = !1, this } }), function () { Element.Properties.events = { set: function (e) { this.addEvents(e) } }, [Element, Window, Document].invoke("implement", { addEvent: function (e, t) { var n = this.retrieve("events", {}); if (n[e] || (n[e] = { keys: [], values: [] }), n[e].keys.contains(t)) return this; n[e].keys.push(t); var r = e, i = Element.Events[e], o = t, s = this; i && (i.onAdd && i.onAdd.call(this, t), i.condition && (o = function (e) { return !i.condition.call(this, e) || t.call(this, e) }), r = i.base || r); var a = function () { return t.call(s) }, u = Element.NativeEvents[r]; return u && (2 == u && (a = function (e) { e = new Event(e, s.getWindow()), !1 === o.call(s, e) && e.stop() }), this.addListener(r, a)), n[e].values.push(a), this }, removeEvent: function (e, t) { var n = this.retrieve("events"); if (!n || !n[e]) return this; var r = n[e], i = r.keys.indexOf(t); if (-1 == i) return this; var o = r.values[i]; delete r.keys[i], delete r.values[i]; var s = Element.Events[e]; return s && (s.onRemove && s.onRemove.call(this, t), e = s.base || e), Element.NativeEvents[e] ? this.removeListener(e, o) : this }, addEvents: function (e) { for (var t in e) this.addEvent(t, e[t]); return this }, removeEvents: function (e) { var t; if ("object" == typeOf(e)) { for (t in e) this.removeEvent(t, e[t]); return this } var n = this.retrieve("events"); if (!n) return this; if (e) n[e] && (n[e].keys.each(function (t) { this.removeEvent(e, t) }, this), delete n[e]); else { for (t in n) this.removeEvents(t); this.eliminate("events") } return this }, fireEvent: function (e, t, n) { var r = this.retrieve("events"); return r && r[e] ? (t = Array.from(t), r[e].keys.each(function (e) { n ? e.delay(n, this, t) : e.apply(this, t) }, this), this) : this }, cloneEvents: function (e, t) { var n = (e = document.id(e)).retrieve("events"); if (!n) return this; if (t) n[t] && n[t].keys.each(function (e) { this.addEvent(t, e) }, this); else for (var r in n) this.cloneEvents(e, r); return this } }); try { "undefined" != typeof HTMLElement && (HTMLElement.prototype.fireEvent = Element.prototype.fireEvent) } catch (e) { } Element.NativeEvents = { click: 2, dblclick: 2, mouseup: 2, mousedown: 2, contextmenu: 2, mousewheel: 2, DOMMouseScroll: 2, mouseover: 2, mouseout: 2, mousemove: 2, selectstart: 2, selectend: 2, keydown: 2, keypress: 2, keyup: 2, orientationchange: 2, touchstart: 2, touchmove: 2, touchend: 2, touchcancel: 2, gesturestart: 2, gesturechange: 2, gestureend: 2, focus: 2, blur: 2, change: 2, reset: 2, select: 2, submit: 2, load: 2, unload: 1, beforeunload: 2, resize: 1, move: 1, DOMContentLoaded: 1, readystatechange: 1, error: 1, abort: 1, scroll: 1 }; var e = function (e) { var t = e.relatedTarget; return null == t || !!t && (t != this && "xul" != t.prefix && "document" != typeOf(this) && !this.contains(t)) }; Element.Events = { mouseenter: { base: "mouseover", condition: e }, mouseleave: { base: "mouseout", condition: e }, mousewheel: { base: Browser.firefox ? "DOMMouseScroll" : "mousewheel" } } }(), function (e, t) { var n, r, i, o, s = [], a = !0; try { a = null != e.frameElement } catch (e) { } var u = function () { clearTimeout(o), n || (Browser.loaded = n = !0, t.removeListener("DOMContentLoaded", u).removeListener("readystatechange", c), t.fireEvent("domready"), e.fireEvent("domready")) }, c = function () { for (var e = s.length; e--;) if (s[e]()) return u(), !0; return !1 }, l = function () { clearTimeout(o), c() || (o = setTimeout(l, 10)) }; t.addListener("DOMContentLoaded", u); var h = t.createElement("div"); h.doScroll && !a && (s.push(function () { try { return h.doScroll(), !0 } catch (e) { } return !1 }), i = !0), t.readyState && s.push(function () { var e = t.readyState; return "loaded" == e || "complete" == e }), "onreadystatechange" in t ? t.addListener("readystatechange", c) : i = !0, i && l(), Element.Events.domready = { onAdd: function (e) { n && e.call(this) } }, Element.Events.load = { base: "load", onAdd: function (t) { r && this == e && t.call(this) }, condition: function () { return this == e && (u(), delete Element.Events.load), !0 } }, e.addEvent("load", function () { r = !0 }) }(window, document), function () { [Element, Window, Document].invoke("implement", { hasEvent: function (e) { var t = this.retrieve("events"), n = t && t[e] ? t[e].values : null; if (n) for (var r = n.length; r--;) if (r in n) return !0; return !1 } }); var e = function (e, t, n, r) { return t = e[t], n = e[n], function (e, i) { i || (i = r), n && !this.hasEvent(i) && n.call(this, e, i), t && t.call(this, e, i) } }, t = function (e, t, n, r) { return function (i, o) { t[n].call(this, i, o || r), e[n].call(this, i, o || r) } }, n = Element.Events; Element.defineCustomEvent = function (r, i) { var o = n[i.base]; return i.onAdd = e(i, "onAdd", "onSetup", r), i.onRemove = e(i, "onRemove", "onTeardown", r), n[r] = o ? Object.append({}, i, { base: o.base, condition: function (e) { return (!o.condition || o.condition.call(this, e)) && (!i.condition || i.condition.call(this, e)) }, onAdd: t(i, o, "onAdd", r), onRemove: t(i, o, "onRemove", r) }) : i, this }; var r = function (e) { var t = "on" + e.capitalize(); return Element[e + "CustomEvents"] = function () { Object.each(n, function (e, n) { e[t] && e[t].call(e, n) }) }, r }; r("enable")("disable") }(), function () { var e, t, n = {}, r = function () { t = !1 }, i = { touchstart: function (e) { if (!(e.touches.length > 1)) { var r = e.touches[0]; t = !0, n = { x: r.pageX, y: r.pageY } } }, touchmove: function (r) { if (!e && t) { var i = r.changedTouches[0], o = { x: i.pageX, y: i.pageY }; if (this.retrieve("swipe:cancelVertical") && Math.abs(n.y - o.y) > 10) t = !1; else { var s = this.retrieve("swipe:distance", 50), a = o.x - n.x, u = a < -s; (a > s || u) && (r.preventDefault(), t = !1, r.direction = u ? "left" : "right", r.start = n, r.end = o, this.fireEvent("swipe", r)) } } }, touchend: r, touchcancel: r }; Element.defineCustomEvent("swipe", { onSetup: function () { this.addEvents(i) }, onTeardown: function () { this.removeEvents(i) }, onEnable: function () { e = !1 }, onDisable: function () { e = !0, r() } }) }();
