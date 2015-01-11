// -----------------------------------------------------------------------------------
//
//	App Remote v3.4
//	by John Donnelly
//	Last Modification: 1/5/2015
//
//	Licensed under the Creative Commons Attribution 2.5 License - http://creativecommons.org/licenses/by/2.5/
//  	- Free for use in both personal and commercial projects
//		- Attribution requires leaving author name, author link, and the license info intact.
//
//  Requirements:
//      jquery v1.11.0 - http://code.jquery.com/jquery-1.11.1.min.js
//      migrate.jquery.min.js - http://code.jquery.com/jquery-migrate-1.2.1.min.js
//
// -----------------------------------------------------------------------------------


var appRemote_Config = {
    animationSpeed: 150,
    closeTimeout: 2000,
    backOnAction: false,
    remoteId: "",
    remoteName: "",
    categoryId: "",
    categoryName: "",
    currUrl: "",
    desktopCSS: "remote_desktop.css",
    mobileCSS: "remote_mobile.css",
    winMinWidth: 800,
    workspaceMode: "",
    notiInterval: 7000,
    siteTheme: "Standard",
    siteRootFolder: ""
};

var appRemote = function () {

    var appMinWidth = 0;
    var appMinHeight = 0;
    var canSaveSort = false;
    var performingAction = false;
    var currWinMode = "";


    function ConvertBitToBoolean(value) {
        if (value != null && value != undefined) {
            var _value = $.trim(value.toString().toLowerCase());

            if (_value != "true" && _value != "false" && _value != "0" && _value != "1" && _value != "") {
                return true;
            }

            if (_value == "1" || _value == "true" || _value == "") {
                return true;
            }
            else if (_value == "0" || _value == "false") {
                return false;
            }
        }

        return false;
    }
    function IsComplexWorkspaceMode() {
        if (appRemote_Config.workspaceMode.toLowerCase() == "complex" || appRemote_Config.workspaceMode == "") {
            return true;
        }

        return false;
    }


    $(document.body).on("click", "#db-b", function () {
        appRemote.HideApp();
        $("#Category-Back-options").trigger("click");
    });
    $(document.body).on("click", "#db-c", function () {
        $("#pnl_options").find("#" + appRemote_Config.remoteId + "-loadedapp").remove();
        $("#db-b").hide();
        $("#db-c").hide();
        $(".loading-background-holder").remove();
        $("#apps_header_btn").show();
        $("#chat_header_btn").show();
        $("#" + appRemote_Config.remoteId + "-pnl-icons").find(".app-icon-font").removeClass("mobile-open");
        $("#load-option-text").html("<b>" + appRemote_Config.remoteName + "</b> Load Options <span class='img-menudropdown' style='float: right!important; margin-right: 3px!important; margin-top: 2px!important;'></span>");
        $("#pnl_options-minHeight").show();
    });
    $(document.body).on("click", "#db-s", function () {
        if (!performingAction) {
            $("#loading-message-modal").html("");
            if (($("#workspace-selector-overlay").css("display") == "none") || ($("#workspace-selector-modal").css("display") == "none")) {
                performingAction = true;
                var loadOverlayto = setTimeout(function () {
                    StartLoadingOverlay("Getting Current...");
                }, 1500);
                $.ajax({
                    url: "WebServices/AcctSettings.asmx/GetCurrentWorkspace_Remote",
                    type: "POST",
                    data: '{ }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        performingAction = false;
                        clearTimeout(loadOverlayto);
                        CloseStartLoadingOverlay();
                        if (data.d != "") {
                            $("#dropdownSelector").val(data.d);
                        }

                        $("#workspace-selector-overlay, #workspace-selector-modal").fadeIn(appRemote_Config.animationSpeed);
                    },
                    error: function () {
                        $('#loading-message').html("<span style='color: Red;'>Error! Try again.</span>");
                        setTimeout(function () {
                            CloseStartLoadingOverlay();
                        }, appRemote_Config.closeTimeout);
                        performingAction = false;
                    }
                });
            }
            else {
                $("#workspace-selector-overlay, #workspace-selector-modal").fadeOut(appRemote_Config.animationSpeed);
            }
        }
    });
    $(document.body).on("click", "#cc-s", function () {
        $("#workspace-selector-overlay, #workspace-selector-modal").fadeOut(appRemote_Config.animationSpeed);
        if (!$("#pnl_chat_users").is(":visible")) {
            $("#pnl_icons, #pnl_appMoveResize").hide();
            if (currWinMode != "desktop") {
                appRemote_Config.categoryId = "chatClient";
                appRemote_Config.categoryName = "userSelect";
                appRemote_Config.remoteId = "";
                appRemote_Config.remoteName = "";
                UpdateURL();
            }
            $("#pnl_chat_users").fadeIn(appRemote_Config.animationSpeed);
        }
    });
    $(document.body).on("click", "#wl-s", function () {
        $("#pnl_chat_users, #pnl_options, #pnl_chat_popup, #pnl_appMoveResize").hide();
        appRemote_Config.categoryId = "";
        appRemote_Config.categoryName = "";
        appRemote_Config.remoteId = "";
        appRemote_Config.remoteName = "";
        UpdateURL();
        $("#pnl_icons").fadeIn(appRemote_Config.animationSpeed);
    });
    $(document.body).on("click", "#workspace-selector-overlay", function () {
        if (!performingAction) {
            if (($("#workspace-selector-overlay").css("display") != "none") || ($("#workspace-selector-modal").css("display") != "none")) {
                $("#loading-message-modal").html("");
                $("#workspace-selector-overlay, #workspace-selector-modal").fadeOut(appRemote_Config.animationSpeed);
            }
        }
    });
    $(document.body).on("keydown", "#moveresize-top, #moveresize-left, #moveresize-width, #moveresize-height", function (event) {
        try {
            if (event.which == 13) {
                event.preventDefault();
                OpenApp();
                return false;
            }
        } catch (evt) {
            if (event.keyCode == 13) {
                event.preventDefault();
                OpenApp();
                return false;
            }
            delete evt;
        }
    });
    $(document.body).on("change", "#dropdownSelector", function () {
        OnWorkspaceClick($(this).val());
    });
    $(document.body).on("click", "#Category-Back", function () {
        CategoryBack(true);
    });
    $(document.body).on("click", "#Category-Back-options", function () {
        GetAllOpenedApps();
        ResetControls(true);
    });
    $(document.body).on("click", "#Category-Back-moveresize", function () {
        $("#pnl_appMoveResize").hide();
        $("#pnl_options").fadeIn(appRemote_Config.animationSpeed);
    });
    $(document.body).on("click", "#Category-Back-ChatRemote-Close", function () {
        $("#pnl_chat_popup").hide();
        $(".iFrame-chat").each(function () {
            $(this).remove();
        });
        if (currWinMode != "desktop") {
            appRemote_Config.categoryId = "chatClient";
            appRemote_Config.categoryName = "userSelect";
            appRemote_Config.remoteId = "";
            appRemote_Config.remoteName = "";
            UpdateURL();
        }
        $("#pnl_chat_users").fadeIn(appRemote_Config.animationSpeed);
    });
    $(document.body).on("click", "#notifications", function () {
        if ($(this).hasClass("has-notifications")) {
            var nextParmChar = "?";
            var loc = window.location.href.split("#?");
            if (loc.length > 0) {
                loc = loc[0];
            }

            if (loc.indexOf("#") != loc.length - 1) {
                loc += "#";
            }

            if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
                loc += "?id=" + appRemote_Config.categoryId + "&category=" + appRemote_Config.categoryName;
                nextParmChar = "&";
            }

            if ((appRemote_Config.remoteId != "") && (appRemote_Config.remoteName != "")) {
                if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
                    loc += "&";
                }
                else {
                    loc += "?";
                }

                loc += "appId=" + appRemote_Config.remoteId + "&name=" + appRemote_Config.remoteName;
                nextParmChar = "&";
            }

            if ($("#notifications-viewtable").css("display") != "block") {
                window.location = loc + nextParmChar + "notiOn=true";
            }
            else {
                window.location = loc;
            }
        }
    });


    function init() {
        SetWindowMode();
        LoadViewPort();
    };
    function SetWindowMode() {
        // Set mobile or desktop view
        if (($(window).width() <= appRemote_Config.winMinWidth) || ($("#pnl_chat_users").length == 0)) {
            if (currWinMode != "mobile") {
                currWinMode = "mobile";
                unloadCSS(appRemote_Config.desktopCSS);
                loadCSS(appRemote_Config.mobileCSS);

                var loc = window.location.href.split("#");
                if (loc.length == 1) {
                    window.location += "#";
                }

                var url = location.hash;
                load(url);
            }
        }
        else {
            if (currWinMode != "desktop") {
                currWinMode = "desktop";
                unloadCSS(appRemote_Config.mobileCSS);
                loadCSS(appRemote_Config.desktopCSS);
            }
        }
    }

    /* Desktop - Mobile Site */
    function loadCSS(url) {
        var fullUrl = openWSE.siteRoot() + "App_Themes/" + appRemote_Config.siteTheme + "/" + url;
        $("link[href='" + fullUrl + "']").remove();
        var head = document.getElementsByTagName('head')[0];
        link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = fullUrl;
        head.appendChild(link);
    }
    function unloadCSS(url) {
        var fullUrl = openWSE.siteRoot() + "App_Themes/" + appRemote_Config.siteTheme + "/" + url;
        $("link[href='" + fullUrl + "']").remove();
    }
    function LoadViewPort() {
        if (navigator.userAgent.match(/Android/i) || navigator.userAgent.match(/webOS/i) || navigator.userAgent.match(/iPhone/i) || navigator.userAgent.match(/iPad/i) || navigator.userAgent.match(/iPod/i) || navigator.userAgent.match(/BlackBerry/i) || navigator.userAgent.match(/Windows Phone/i)) {
            var head = document.getElementsByTagName('head')[0];
            meta = document.createElement('meta');
            meta.name = 'viewport';
            meta.id = 'mobileViewport';
            meta.content = 'initial-scale=0.90, user-scalable=no';
            head.appendChild(meta);
        }
        else {
            if ($("#mobileViewport").length > 0) {
                $("#mobileViewport").remove();
            }
        }
    }

    function load(url) {
        if (!performingAction) {
            currUrl = url;
            $("#workspace-selector-overlay, #workspace-selector-modal").fadeOut(appRemote_Config.animationSpeed);
            $("#loading-message-modal").html("");

            if (url == "") {
                CategoryBack(false);
            }
            else if (url.indexOf("notiOn=true") != -1) {
                GetUserNotifications(true);
                $("#notifications-viewtable").show();
            }
            else {
                if ($("#notifications-viewtable").css("display") == "block") {
                    $("#notifications-viewtable").hide();
                    $("#NotificationHolder").html("");
                }

                var loc = url.split("?id=");
                if (loc.length > 1) {
                    loc = loc[1].split("&category=");
                    if (loc.length == 2) {
                        if (loc[0] == "chatClient") {
                            LoadChatClientPnl(loc);
                        }
                        else {
                            if ($("#pnl_chat_users").length > 0) {
                                $("#pnl_chat_users").hide();
                                if (currWinMode != "desktop") {
                                    $("#pnl_chat_popup").hide();
                                }
                            }

                            appRemote_Config.categoryId = loc[0];
                            var tempR = loc[1].split("&appId=");
                            if (tempR.length == 2) {
                                appRemote_Config.categoryName = tempR[0];
                                tempR = tempR[1].split("&name=");

                                if (tempR[0].indexOf("app-") == 0) {
                                    appRemote_Config.remoteId = tempR[0];
                                    appRemote_Config.remoteName = tempR[1];
                                    GetAppRemoteProps(appRemote_Config.remoteId, appRemote_Config.remoteName);
                                }
                                else {
                                    LoadChatClientPnl(loc);
                                }
                            }
                            else {
                                if ($("#pnl_options").css("display") == "block") {
                                    GetAllOpenedApps();
                                    ResetControls(false);
                                }
                                else if (loc[1].indexOf("&appId=") == -1) {
                                    CategoryClick(loc[0], loc[1], false);
                                }
                            }
                        }
                    }
                }
                else {
                    if ($("#pnl_chat_users").length > 0) {
                        $("#pnl_chat_users").hide();
                    }

                    var tempR = loc[0].split("?appId=");
                    if (tempR.length == 2) {
                        tempR = tempR[1].split("&name=");

                        if (tempR[0].indexOf("app-") == 0) {
                            appRemote_Config.remoteId = tempR[0];
                            appRemote_Config.remoteName = tempR[1];
                            GetAppRemoteProps(appRemote_Config.remoteId, appRemote_Config.remoteName);
                        }
                    }
                    else {
                        ResetControls(false);
                    }
                }
            }
        }
        else {
            if ((currUrl != "#") && (currUrl != "") && (currUrl != "1")) {
                var loc = window.location.href.split("#");
                if (loc.length > 0) {
                    for (var i = 1; i < loc.length; i++) {
                        if ($.trim(loc[i]) != "") {
                            currUrl = $.trim(loc[i]);
                            break;
                        }
                    }

                    loc = loc[0];
                }

                window.location = loc + "#" + currUrl;
            }
        }
    }
    function LoadChatClientPnl(loc) {
        if ($("#pnl_chat_popup").length > 0) {
            if (currWinMode != "desktop") {
                $("#pnl_icons, #pnl_options").hide();
            }
            try {
                appRemote_Config.categoryId = loc[0];
                var tempR = loc[1].split("&appId=");
                if (tempR.length == 2) {
                    appRemote_Config.categoryName = tempR[0];
                    tempR = tempR[1].split("&name=");
                    if (tempR[0].indexOf("app-") == -1) {
                        $("#pnl_chat_users").hide();
                        try {
                            chatClient.BuildChatWindowMobile(tempR[0]);
                            $("#Category-Back-ChatRemote-Close").find("h4").html("Close " + tempR[1] + " Chat Session");
                        }
                        catch (evt) { }
                    }
                    else {
                        appRemote_Config.categoryId = "";
                        appRemote_Config.categoryName = "";
                        UpdateURL();
                    }
                }
                else {
                    $("#pnl_chat_popup").hide();
                    $("#pnl_chat_users").fadeIn(appRemote_Config.animationSpeed);
                }
            }
            catch (evt) { }
        }
        else {
            GetAllOpenedApps();
            ResetControls(false);
            UpdateURL();
        }
    }
    function UpdateURL() {
        var loc = window.location.href.split("#?");
        if (loc.length > 0) {
            loc = loc[0];
        }

        if (loc.indexOf("#") != loc.length - 1) {
            loc += "#";
        }

        if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
            loc += "?id=" + appRemote_Config.categoryId + "&category=" + appRemote_Config.categoryName;
        }

        if ((appRemote_Config.remoteId != "") && (appRemote_Config.remoteName != "")) {
            if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
                loc += "&";
            }
            else {
                loc += "?";
            }

            loc += "appId=" + appRemote_Config.remoteId + "&name=" + appRemote_Config.remoteName;
        }

        window.location = loc;
    }
    function StartLoadingOverlay(message) {
        if (message.indexOf("...") != -1) {
            message = message.replace("...", "");
        }

        if ($("#loadoptions-selector-overlay").css("display") == "block") {
            $("#loadoptions-selector-overlay, #loadoptions-selector-modal").hide();
            $("#loading-message").html("");
        }

        $("#loading-message").html(message);
        $("#loadoptions-selector-overlay, #loadoptions-selector-modal").show();
    }
    function CloseStartLoadingOverlay() {
        $("#loadoptions-selector-overlay, #loadoptions-selector-modal").hide();
        $("#loading-message").html("");
    }
    function OnWorkspaceClick(num) {
        if (!performingAction) {
            performingAction = true;
            $("#loading-message-modal").html("Switching Workspaces...");
            $.ajax({
                url: "WebServices/AcctSettings.asmx/UpdateAppRemote",
                type: "POST",
                data: '{ "id": "workspace-selector","options": "' + num + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (appRemote.ConvertBitToBoolean(data.d)) {
                        $("#workspace-selector-overlay, #workspace-selector-modal").fadeOut(appRemote_Config.animationSpeed);
                        $("#loading-message-modal").html("");
                    }
                    else {
                        $('#loading-message-modal').html("<span style='color: Red;'>Error! Try again.</span>");
                    }
                    performingAction = false;
                },
                error: function () {
                    $('#loading-message-modal').html("<span style='color: Red;'>Error! Try again.</span>");
                    performingAction = false;
                }
            });
        }
    }
    function LoadOptions(id, name, updateUrl) {
        appRemote_Config.remoteId = id;
        appRemote_Config.remoteName = name;
        if (updateUrl) {
            UpdateURL();
        }
    }
    function GetAppRemoteProps(id, name) {
        if (!performingAction) {
            if ($("#" + id + "-loadedapp").length > 0) {
                $("#pnl_options-minHeight").hide();
                $("#" + appRemote_Config.remoteId + "-loadedapp").css({
                    visibility: "visible",
                    position: "",
                    top: ""
                });

                $("#db-b").show();
                $("#db-c").show();
                $("#apps_header_btn").hide();
                $("#chat_header_btn").hide();
                $("#pnl_options").show();
                $("#pnl_icons").hide();
                return;
            }
            else {
                $("#pnl_options-minHeight").show();
                $("#load-option-text").html("<b>" + appRemote_Config.remoteName + "</b> Load Options <span class='img-menudropdown' style='float: right!important; margin-right: 3px!important; margin-top: 2px!important;'></span>");
            }

            performingAction = true;
            var loadOverlayto = setTimeout(function () {
                StartLoadingOverlay("Loading Options...");
            }, 1500);
            $.ajax({
                url: "WebServices/AcctSettings.asmx/GetAppRemoteProps",
                type: "POST",
                data: '{ "appId": "' + id + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    clearTimeout(loadOverlayto);
                    CloseStartLoadingOverlay();
                    if (appRemote.IsComplexWorkspaceMode()) {
                        $("#options-btn-open").show();
                    }

                    $("#options-btn-update").hide();
                    $("#pnl_appMoveResize").hide();

                    if (data.d[0].length == 16) {
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

                        var allowPopout = data.d[0][11];
                        var popoutLoc = data.d[0][12];
                        var allowMax = data.d[0][13];
                        var allowResize = data.d[0][14];
                        var allowStats = data.d[0][15];

                        if ((width == "0") || (width == "")) {
                            width = minWidth;
                        }
                        if ((height == "0") || (height == "")) {
                            height = minHeight;
                        }

                        $(".accordion-header").each(function () {
                            $(this).removeClass("active");
                        });

                        $("#load-option-about").attr("onclick", "appRemote.AccordionClick_About('" + id + "')");
                        $("#load-option-about").next(".accordion-content").html("");
                        $("#load-option-about").next(".accordion-content").hide();

                        $("#load-option-stats").next(".accordion-content").html("");
                        $("#load-option-stats").next(".accordion-content").hide();
                        if (appRemote.ConvertBitToBoolean(allowStats)) {
                            $("#load-option-stats").attr("onclick", "appRemote.AccordionClick_Stats('" + id + "')");
                            $("#load-option-stats").show();
                        }
                        else {
                            $("#load-option-stats").hide();
                        }

                        $("#load-option-text").addClass("active");
                        $("#load-option-text").attr("onclick", "appRemote.AccordionClick_Options('" + id + "')");
                        $("#load-option-text").next(".accordion-content").show();

                        appMinWidth = minWidth;
                        appMinHeight = minHeight;

                        $("#moveresize-left").val(poxX);
                        $("#moveresize-top").val(poxY);
                        $("#moveresize-width").val(width);
                        $("#moveresize-height").val(height);

                        $("#moveresize-left").spinner({ min: 0 });
                        $("#moveresize-top").spinner({ min: 0 });
                        $("#moveresize-width").spinner({ min: parseInt(minWidth) });
                        $("#moveresize-height").spinner({ min: parseInt(minHeight) });

                        if (appRemote.IsComplexWorkspaceMode()) {
                            $("#app-load-options").show();
                            if (appRemote.ConvertBitToBoolean(closed)) {
                                $("#options-btn-close").hide();
                            }
                            else {
                                $("#options-btn-close").show();
                                $("#options-btn-open").hide();
                                $("#options-btn-update").show();
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

                        $("#rb_norm").prop("checked", true);
                        if (!appRemote.ConvertBitToBoolean(closed)) {
                            $("#pnl_appMoveResize").hide();
                            $("#resize-div").hide();

                            if ((!appRemote.ConvertBitToBoolean(allowMax)) && (!appRemote.ConvertBitToBoolean(allowResize))) {
                                $("#pnl_appMoveResize").show();
                            }
                            else if (appRemote.ConvertBitToBoolean(allowResize)) {
                                $("#pnl_appMoveResize").show();
                                $("#resize-div").show();
                            }

                            if (appRemote.ConvertBitToBoolean(max)) {
                                $("#rb_max").prop("checked", true);
                            }
                            if (appRemote.ConvertBitToBoolean(min)) {
                                $("#rb_min").prop("checked", true);
                            }
                        }
                        else {
                            $("#rb_norm").prop("checked", true);
                            $("#pnl_appMoveResize").hide();
                        }

                        if (updated != "") {
                            $("#last-updated").html("<b class='pad-right-sml'>Last Updated:</b>" + updated);
                        }

                        if (appRemote.ConvertBitToBoolean(allowMax)) {
                            $("#div_max_rb_holder").show();
                        }
                        else {
                            $("#div_max_rb_holder").hide();
                        }

                        $("#options-btn-device").hide();
                        if ((appRemote.ConvertBitToBoolean(allowPopout)) && (popoutLoc != "")) {
                            $("#options-btn-device").attr("href", popoutLoc);
                            $("#options-btn-device").show();
                        }
                    }
                    else {
                        $("#options-btn-device").hide();
                        $("#options-btn-close").hide();
                        $("#pnl_appMoveResize").hide();
                        $("#ddl_appDropdownSelector").val("1");
                        $("#rb_norm").prop("checked", true);
                        $("#last-updated").html("");
                    }


                    $(document).tooltip({ disabled: true });
                    $("#pnl_icons").fadeOut(appRemote_Config.animationSpeed, function () {
                        $(document).tooltip({ disabled: false });
                    });
                    $("#pnl_options").show();
                    performingAction = false;
                },
                error: function () {
                    alert("Error trying to access account. Please try again.");
                    performingAction = false;
                }
            });
        }
    }
    function GetAllOpenedApps() {
        HideApp();

        if (appRemote.IsComplexWorkspaceMode()) {
            var loadOverlayto = setTimeout(function () {
                StartLoadingOverlay("Loading Remote...");
            }, 1500);

            $(".app-icon").removeClass("active");
            $(".workspace-reminder").each(function () {
                $(this).remove();
            });

            setTimeout(function () {
                $.ajax({
                    url: "WebServices/AcctSettings.asmx/GetAllOpenedApps",
                    type: "POST",
                    data: '{ }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        clearTimeout(loadOverlayto);
                        CloseStartLoadingOverlay();
                        if (data.d != null) {
                            for (var i = 0; i < data.d.length; i++) {
                                var $this = $("#" + data.d[i][0] + "-pnl-icons");
                                $this.addClass("active");
                                if (data.d[i][1] != "") {
                                    var db = "<span class='workspace-reminder font-no-bold'>" + GetWorkspaceNumber(data.d[i][1]) + "</span>";
                                    $this.append(db);
                                }
                            }
                        }
                    }
                });
            }, 250);
        }
        else {
            setTimeout(function () {
                clearTimeout(loadOverlayto);
                CloseStartLoadingOverlay();
            }, 250);
        }
    }
    function ResetControls(updateUrl) {
        appRemote_Config.remoteId = "";
        appRemote_Config.remoteName = "";
        $("#pnl_appMoveResize").hide();
        $("#pnl_options").fadeOut(appRemote_Config.animationSpeed, function () {
            if (currWinMode != "desktop") {
                $("#pnl_chat_users, #pnl_chat_popup").hide();
            }
            $("#pnl_icons").show();
            $("#rb_norm").prop("checked", true);
            $("#ddl_appDropdownSelector").val("1");
            $("#loading-message").html("");
            $("#load-option-text").html("");

            if (updateUrl) {
                UpdateURL();
            }
        });
    }
    function GetWorkspaceNumber(db) {
        return db.replace("workspace_", "");
    }
    function AccordionClick_About(id) {
        if (!$("#load-option-about").hasClass("active")) {
            RemoveActiveAccordion();
            SetActiveAccordion("load-option-about");
            $("#load-option-about").next(".accordion-content").html("<h4>Loading...</h4>");
            $.ajax({
                url: "WebServices/AcctSettings.asmx/GetAppRemoteAboutStats",
                type: "POST",
                data: '{ "id": "' + appRemote_Config.remoteId + '","option": "about" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d != null) {
                        $("#load-option-about").next(".accordion-content").html(data.d[0]);
                        if (data.d.length == 3) {
                            appRemote.RatingStyleInit(".app-rater", data.d[1], true, id, true);
                            for (var i = 0; i < data.d[2].length; i++) {
                                var rating = data.d[2][i];
                                appRemote.RatingStyleInit($(".ratingreviews-div").eq(i)[0], rating.Rating, true, rating.AppID, false);
                            }
                        }
                    }
                    performingAction = false;
                },
                error: function () {
                    $("#load-option-about").next(".accordion-content").html("<h4>Error! Could not load.</h4>");
                    performingAction = false;
                }
            });
        }
    }
    function AccordionClick_Stats(id) {
        if (!$("#load-option-stats").hasClass("active")) {
            RemoveActiveAccordion();
            SetActiveAccordion("load-option-stats");
            $("#load-option-stats").next(".accordion-content").html("<h4>Loading...</h4>");
            AppStats(id);
        }
    }
    function AppStats(id) {
        $.ajax({
            url: "WebServices/AcctSettings.asmx/GetAppRemoteAboutStats",
            type: "POST",
            data: '{ "id": "' + appRemote_Config.remoteId + '","option": "stats" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#load-option-stats").next(".accordion-content").html(data.d);
                performingAction = false;
            },
            error: function () {
                $("#load-option-stats").next(".accordion-content").html("<h4>Error! Could not load.</h4>");
                performingAction = false;
            }
        });
    }
    function AccordionClick_Options(id) {
        if (!$("#load-option-text").hasClass("active")) {
            RemoveActiveAccordion();
            SetActiveAccordion("load-option-text");
        }
    }
    function RemoveActiveAccordion() {
        $(".accordion-header").each(function () {
            $(this).removeClass("active");
            $(this).next(".accordion-content").slideUp(appRemote_Config.animationSpeed);
        });
    }
    function SetActiveAccordion(tabId) {
        $("#" + tabId).addClass("active");
        setTimeout(function () {
            $("#" + tabId).next(".accordion-content").slideDown(appRemote_Config.animationSpeed);
        }, appRemote_Config.animationSpeed);
    }
    function OpenApp() {
        if (!performingAction) {
            performingAction = true;
            var options = "";
            var isMaxed = false;

            options += $("#ddl_appDropdownSelector").val() + ";";

            if ($("#rb_max").is(":checked")) {

                options += "maximize;";
                isMaxed = true;
            }
            else if ($("#rb_min").is(":checked")) {
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
                StartLoadingOverlay("Refreshing App...");
            }
            else {
                StartLoadingOverlay("Loading App...");
            }

            $.ajax({
                url: "WebServices/AcctSettings.asmx/UpdateAppRemote",
                type: "POST",
                data: '{ "id": "' + appRemote_Config.remoteId + '","options": "' + escape(options) + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    performingAction = false;
                    if (appRemote.ConvertBitToBoolean(data.d)) {
                        CloseStartLoadingOverlay();
                        if (appRemote_Config.backOnAction) {
                            ResetControls(true);
                        }
                        else {
                            setTimeout(function () {
                                GetAppRemoteProps(appRemote_Config.remoteId, appRemote_Config.remoteName);
                            }, 250);
                        }
                    }
                    else {
                        $('#loading-message').html("<span style='color: Red;'>Could not load! Try again.</span>");
                        setTimeout(function () {
                            CloseStartLoadingOverlay();
                        }, appRemote_Config.closeTimeout);
                    }
                },
                error: function () {
                    $('#loading-message').html("<span style='color: Red;'>Could not load! Try again.</span>");
                    setTimeout(function () {
                        CloseStartLoadingOverlay();
                    }, appRemote_Config.closeTimeout);
                    performingAction = false;
                }
            });
        }
    }
    function LoadApp() {
        var $iframe = $("#" + appRemote_Config.remoteId + "-loadedapp");
        if ($iframe.length > 0) {
            $iframe.remove();
        }

        $("#pnl_options-minHeight").hide();

        var href = $("#options-btn-device").attr("href");
        if (href.toLowerCase().indexOf("externalappholder.aspx?appid") > 0) {
            href += "&hidetoolbar=true";
        }

        var iframeHt = Math.abs($(window).height() - ($("#always-visible").outerHeight() + $("#container-footer").outerHeight() + $("#notifications").outerHeight()));

        $("#db-b").show();
        $("#db-c").show();
        $("#apps_header_btn").hide();
        $("#chat_header_btn").hide();

        $("#" + appRemote_Config.remoteId + "-pnl-icons").find(".app-icon-font").addClass("mobile-open");

        $("#pnl_options").append("<div class='loading-background-holder'></div>");
        $("#pnl_options").append("<iframe id='" + appRemote_Config.remoteId + "-loadedapp' src='" + href + "' frameborder='0' class='loaded-app-holder' style='height: " + iframeHt + "px;'></iframe>");
        $(".loaded-app-holder").one("load", (function () {
            $(".loading-background-holder").remove();
        }));
    }
    function HideApp() {
        var topPos = -($("#" + appRemote_Config.remoteId + "-loadedapp").outerHeight() * 2);
        $("#" + appRemote_Config.remoteId + "-loadedapp").css({
            visibility: "hidden",
            position: "absolute",
            top: topPos
        });
        $("#db-b").hide();
        $("#db-c").hide();
        $("#apps_header_btn").show();
        $("#chat_header_btn").show();
    }
    function CloseApp() {
        if (!performingAction) {
            performingAction = true;
            StartLoadingOverlay("Closing App...");
            $.ajax({
                url: "WebServices/AcctSettings.asmx/UpdateAppRemote",
                type: "POST",
                data: '{ "id": "' + appRemote_Config.remoteId + '","options": "' + "close" + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    performingAction = false;
                    if (appRemote.ConvertBitToBoolean(data.d)) {
                        CloseStartLoadingOverlay();
                        ResetControls(true);
                        GetAllOpenedApps();
                    }
                    else {
                        $('#loading-message').html("<span style='color: Red;'>Error! Try again.</span>");
                        setTimeout(function () {
                            CloseStartLoadingOverlay();
                        }, appRemote_Config.closeTimeout);
                    }
                },
                error: function () {
                    $('#loading-message').html("<span style='color: Red;'>Error! Try again.</span>");
                    setTimeout(function () {
                        CloseStartLoadingOverlay();
                    }, appRemote_Config.closeTimeout);
                    performingAction = false;
                }
            });
        }
    }
    function AppsSortUnlocked(canSave) {
        if (canSave) {
            canSaveSort = true;
        }

        $('#pnl_icons').sortable({
            axis: 'y',
            cancel: '.app-icon-category-list, #Category-Back, #notifications',
            containment: '#pnl_icons',
            opacity: 0.6,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                $(document).tooltip({ disabled: true });
            },
            stop: function (event, ui) {
                var listorder = '';
                $('.app-icon').each(function () {
                    var temp = $(this).attr('id').replace("-pnl-icons", "");
                    if (temp != '') {
                        listorder += (temp + ',');
                    }
                });

                if (canSaveSort) {
                    $.ajax({
                        url: 'WebServices/SaveControls.asmx/App_UpdateIcons',
                        type: 'POST', data: '{ "appList": "' + escape(listorder) + '" }',
                        contentType: 'application/json; charset=utf-8'
                    });
                }

                $(document).tooltip({ disabled: false });
            }
        });
        $('#pnl_icons').disableSelection();
    }
    function CategoryClick(id, category, updateUrl) {
        appRemote_Config.categoryId = id;
        appRemote_Config.categoryName = category;
        $(".app-icon-category-list").hide();
        $("#Category-Back").fadeIn(appRemote_Config.animationSpeed);
        $("." + appRemote_Config.categoryId).fadeIn(appRemote_Config.animationSpeed);
        $("#Category-Back-Name").html(appRemote_Config.categoryName);
        $("#Category-Back-Name-id").html(appRemote_Config.categoryId);

        if (updateUrl) {
            UpdateURL();
        }
    }
    function CategoryBack(updateUrl) {
        appRemote_Config.categoryId = "";
        appRemote_Config.categoryName = "";
        appRemote_Config.remoteId = "";
        appRemote_Config.remoteName = "";
        var category = $("#Category-Back-Name-id").html();
        $("#Category-Back").fadeOut(appRemote_Config.animationSpeed);
        if ((category != "") && (category != undefined) && (category != null)) {
            if ($("." + category).length > 0) {
                $("." + category).fadeOut(appRemote_Config.animationSpeed, function () {
                    $(".app-icon-category-list").show();
                    $("#Category-Back-Name").html("");
                    $("#Category-Back-Name-id").html("");
                });
            }
        }
        else {
            if ($("#pnl_chat_users").css("display") == "block") {
                $("#pnl_chat_users").hide();
            }
            if ($("#pnl_options").css("display") == "block") {
                $("#pnl_options").hide();
            }
            if ($("#pnl_icons").css("display") == "none") {
                $("#pnl_icons").fadeIn(appRemote_Config.animationSpeed);
            }
            $(".app-icon-category-list").show();
            $("#Category-Back-Name").html("");
            $("#Category-Back-Name-id").html("");
        }
        if (updateUrl) {
            UpdateURL();
        }
    }

    /* Notifications */
    var runningNotiCheck = false;
    var runningNoti = false;
    var runningMoreNoti = false;
    var ddNotiLoading = "<div class='ddLoadingMessage'><h3>Loading Notifications. Please Wait...</h3></div>";
    function CheckForNewNotifications() {
        if (!runningNotiCheck) {
            runningNotiCheck = true;
            $.ajax({
                url: "WebServices/NotificationRetrieve.asmx/CheckForNewNotifications",
                type: "POST",
                data: '{ }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var total = parseInt(data.d);

                    $("#notifications").find("#total-noti").html(total);
                    if (total == 0) {
                        $("#notifications").removeClass("has-notifications").addClass("no-notifications");
                    }
                    else {
                        $("#notifications").removeClass("no-notifications").addClass("has-notifications");
                    }

                    runningNotiCheck = false;
                    setTimeout(function () {
                        CheckForNewNotifications();
                    }, appRemote_Config.notiInterval);
                },
                error: function () {
                    runningNotiCheck = false;
                    setTimeout(function () {
                        CheckForNewNotifications();
                    }, appRemote_Config.notiInterval);
                }
            });
        }
    }
    function GetUserNotifications(showLoading) {
        if (!runningNoti) {
            runningNoti = true;
            if ($(".ddLoadingMessage").length == 0) {
                if (showLoading) {
                    $(".table-notiMessages-div").prepend(ddNotiLoading);
                }
            }

            var myIds = new Array();
            $("#table_NotiMessages tr").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/LoadUserNotifications";
            $.ajax({
                url: notiHandler,
                type: "POST",
                data: '{ "_currIds": "' + myIds + '","siteTheme": "' + appRemote_Config.siteTheme + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".ddLoadingMessage").remove();
                    if ((data.d != null) && (data.d != "")) {
                        $("#NotificationHolder").html($.trim(data.d));
                        if ($("#no-notifications-id").length > 0) {
                            $("#lb_clearNoti").hide();
                        }
                        else {
                            $("#lb_clearNoti").show();
                        }
                    }
                    else {
                        $("#lb_clearNoti").hide();
                    }

                    runningNoti = false;
                    SetNotificationScrollShadow();
                    CheckIfCanAddMore();
                },
                error: function () {
                    $(".ddLoadingMessage").remove();
                    SetNotificationScrollShadow();
                    $("#NotificationHolder").html("<h3 class='pad-top-big pad-bottom-big' style='color: Red; text-align: center;'>Error retrieving notifications!</h3>");
                    $("#lb_clearNoti").hide();
                    runningNoti = false;
                }
            });
        }
    }
    function GetMoreUserNotifications() {
        if ((!runningMoreNoti) && (!runningNoti)) {
            runningMoreNoti = true;
            var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/LoadMoreUserNotifications";

            var myIds = new Array();
            $("#table_NotiMessages tr").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            $.ajax({
                url: notiHandler,
                type: "POST",
                data: '{ "_currIds": "' + myIds + '","_currCount": "' + parseInt($("#table_NotiMessages tr").length) + '","siteTheme": "' + appRemote_Config.siteTheme + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".ddLoadingMessage").remove();
                    if ((data.d != null) && (data.d != "")) {
                        $("#table_NotiMessages").append($.trim(data.d));
                    }

                    SetNotificationScrollShadow();
                    runningMoreNoti = false;
                    CheckIfCanAddMore();
                },
                error: function () {
                    $(".ddLoadingMessage").remove();
                    SetNotificationScrollShadow();
                    $("#NotificationHolder").append("<h3 class='pad-bottom-big' style='color: Red; text-align: center;'>Error retrieving notifications!</h3>");
                    runningMoreNoti = false;
                }
            });
        }
    }
    function CheckIfCanAddMore() {
        var elem = document.getElementById("notifications-viewtable");
        if (elem != null) {
            var innerHeight = $("#notifications-viewtable").innerHeight();
            var maxHeight = $("#notifications-viewtable").outerHeight();
            if ((innerHeight >= elem.scrollHeight) || (elem.scrollHeight < maxHeight)) {
                var totalMessages = parseInt($("#notifications").find("#total-noti").html());
                var currMessages = parseInt($("#table_NotiMessages tr").length);
                if (currMessages < totalMessages) {
                    $(".ddLoadingMessage").remove();
                    $("#NotificationHolder").append(ddNotiLoading);
                    GetMoreUserNotifications();
                }
            }
        }
    }
    function SetNotificationScrollShadow() {
        $("#notifications-viewtable").scroll(function () {
            var elem = document.getElementById("notifications-viewtable");
            if (elem != null) {
                var $_scrollBar = $("#notifications-viewtable");
                var temp = $_scrollBar.scrollTop();
                if (temp > 0) {
                    if (temp + $_scrollBar.height() >= elem.scrollHeight) {
                        var totalMessages = parseInt($("#notifications").find("#total-noti").html());
                        var currMessages = parseInt($("#table_NotiMessages tr").length);
                        if (currMessages < totalMessages) {
                            $(".ddLoadingMessage").remove();
                            $("#NotificationHolder").append(ddNotiLoading);
                            GetMoreUserNotifications();
                        }
                    }
                }
            }
        });
    }
    function NotiActionsClearAll() {
        StartLoadingOverlay("Clearing...");
        var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/DeleteNotifications";
        $.ajax({
            url: notiHandler,
            type: "POST",
            data: '{ "id": "' + "ClearAll" + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (appRemote.ConvertBitToBoolean(data.d)) {
                    CloseStartLoadingOverlay();
                    ResetNoti();
                }
            }
        });
    }
    function ResetNoti() {
        $("#notifications").find("#total-noti").html("0");
        $("#notifications").removeClass("has-notifications").addClass("no-notifications");

        var loc = window.location.href.split("#?");
        if (loc.length > 0) {
            loc = loc[0];
        }

        if (loc.indexOf("#") != loc.length - 1) {
            loc += "#";
        }

        if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
            loc += "?id=" + appRemote_Config.categoryId + "&category=" + appRemote_Config.categoryName;
        }

        if ((appRemote_Config.remoteId != "") && (appRemote_Config.remoteName != "")) {
            if ((appRemote_Config.categoryId != "") && (appRemote_Config.categoryName != "")) {
                loc += "&";
            }
            else {
                loc += "?";
            }

            loc += "appId=" + appRemote_Config.remoteId + "&name=" + appRemote_Config.remoteName;
        }

        window.location = loc;
    }
    function RefreshNotifications() {
        $("#NotificationHolder").html("");
        runningNoti = false;
        GetUserNotifications(true);
    }


    function LoadCreateAccountHolder() {
        if ($("#Login-holder").css("display") != "none") {
            $("#Login-holder").hide();
            $("#iframe-createaccount-holder").html("<iframe id='iframe-demo' src='SiteSettings/iframes/CreateAccount.aspx' frameborder='0' width='100%' style='visibility: hidden;'></iframe>");
            $("#iframe-createaccount-holder").append("<div style='text-align: center;'><h3 id='loadingControls'>Loading Controls. Please Wait...</h3></div>");
            $("#CreateAccount-holder").show();
            $("#iframe-demo").load(function () {
                $("#loadingControls").remove();
                $("#iframe-demo").css({
                    height: "400px",
                    visibility: "visible"
                });
            });
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


    /* Rating Style Initalize */
    function RatingStyleInit(div, rating, disabled, appId, useLargeStars) {
        try {
            var _disabled = false;
            if (disabled) {
                _disabled = true;
            }

            var imagePath = openWSE.siteRoot() + "App_Themes/Standard";

            $(div).attr("data-average", rating);
            $(div).attr("data-id", "1");

            var sizeType = "big";
            if (!useLargeStars) {
                sizeType = "small";
            }

            $(div).jRating({
                step: true,
                type: sizeType,
                showRateInfo: false,
                canRateAgain: true,
                nbRates: 100,
                bigStarsPath: imagePath + "/Icons/star-rating-lrg.png",
                smallStarsPath: imagePath + "/Icons/star-rating-sml.png",
                isDisabled: _disabled,
                decimalLength: 2,
                length: 4,
                rateMax: 4,
                rateMin: 0,
                sendRequest: true
            });

            if (!useLargeStars) {
                $(div).find(".jStar").addClass("jStar-Small");
            }
        }
        catch (evt) { }
    }

    return {
        init: init,
        ConvertBitToBoolean: ConvertBitToBoolean,
        IsComplexWorkspaceMode: IsComplexWorkspaceMode,
        SetWindowMode: SetWindowMode,
        StartLoadingOverlay: StartLoadingOverlay,
        CloseStartLoadingOverlay: CloseStartLoadingOverlay,
        GetAllOpenedApps: GetAllOpenedApps,
        AppsSortUnlocked: AppsSortUnlocked,
        LoadOptions: LoadOptions,
        load: load,
        CategoryClick: CategoryClick,
        OnWorkspaceClick: OnWorkspaceClick,
        AccordionClick_Options: AccordionClick_Options,
        AccordionClick_About: AccordionClick_About,
        AccordionClick_Stats: AccordionClick_Stats,
        AppStats: AppStats,
        OpenApp: OpenApp,
        LoadApp: LoadApp,
        HideApp: HideApp,
        CloseApp: CloseApp,
        ResetControls: ResetControls,
        RefreshNotifications: RefreshNotifications,
        CheckForNewNotifications: CheckForNewNotifications,
        NotiActionsClearAll: NotiActionsClearAll,
        ResetNoti: ResetNoti,
        UpdateURL: UpdateURL,
        LoadCreateAccountHolder: LoadCreateAccountHolder,
        LoadRecoveryPassword: LoadRecoveryPassword,
        RatingStyleInit: RatingStyleInit
    };

}();
var openWSE = function () {
    return {
        NotiActionsHideInd: function (_this) {
            appRemote.StartLoadingOverlay("Deleting...");
            var $this = $(_this).closest("tr");
            var id = $this.attr("id");
            var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/DeleteNotifications";
            $.ajax({
                url: notiHandler,
                type: "POST",
                data: '{ "id": "' + id + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (appRemote.ConvertBitToBoolean(data.d)) {
                        $this.fadeOut(appRemote_Config.animationSpeed, function () {
                            appRemote.CloseStartLoadingOverlay();
                            $this.remove();
                            if ($("#notifications").find("#total-noti").html() == "1") {
                                appRemote.ResetNoti();
                            }
                            else {
                                var currTotal = parseInt($("#notifications").find("#total-noti").html());
                                currTotal -= 1;
                                $("#notifications").find("#total-noti").html(currTotal);
                            }
                        });
                    }
                }
            });
        },
        AcceptGroupNotification: function (_this, groupId) {
            appRemote.StartLoadingOverlay("Accepting...");
            var $this = $(_this).closest("tr");
            var id = $this.attr("id");
            var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/AcceptInviteNotifications";
            $.ajax({
                url: notiHandler,
                type: "POST",
                data: '{ "id": "' + id + '", "groupId": "' + groupId + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (appRemote.ConvertBitToBoolean(data.d)) {
                        $this.fadeOut(appRemote_Config.animationSpeed, function () {
                            appRemote.CloseStartLoadingOverlay();
                            $this.remove();
                            if ($("#notifications").find("#total-noti").html() == "1") {
                                appRemote.ResetNoti();
                            }
                            else {
                                var currTotal = parseInt($("#notifications").find("#total-noti").html());
                                currTotal -= 1;
                                $("#notifications").find("#total-noti").html(currTotal);
                            }
                        });
                    }
                }
            });
        },
        siteRoot: function () {
            var sitePath = "";
            if (appRemote_Config.siteRootFolder != "") {
                sitePath = appRemote_Config.siteRootFolder + "/";
            }

            return window.location.protocol + "//" + window.location.host + "/" + sitePath;
        }
    };
}();

$(document).ready(function () {
    appRemote.StartLoadingOverlay("Loading Remote...");
    appRemote.init();
    var loc = window.location.href.split("#");
    if (loc.length == 1) {
        window.location += "#";
    }

    var notiHt = 0;
    if ($("#notifications").length > 0 && $("#notifications").css("display") != "none") {
        $("#notifications").css("top", $("#always-visible").outerHeight());
        notiHt = $("#notifications").outerHeight();
    }

    $("#pnl_chat_popup, #pnl_chat_users, #pnl_icons, #pnl_login, #pnl_options, #notifications-viewtable").css({
        top: $("#always-visible").outerHeight() + notiHt,
        bottom: $("#container-footer").outerHeight()
    });

    appRemote.GetAllOpenedApps();

    var url = location.hash;
    appRemote.load(url == "" ? "1" : url);
});

$(function () {
    $(window).hashchange(function () {
        var url = location.hash;
        appRemote.load(url == "" ? "1" : url);
    });
});

$(window).resize(function () {
    appRemote.SetWindowMode();
    if ($(".iFrame-chat").length > 0) {
        var h1 = $(window).height();
        var h2 = $("#Category-Back-ChatRemote-Close").height() + 16;
        var h3 = $("#always-visible").height();
        var h4 = $("#container-footer").height();
        var h5 = $("#notifications").height();
        var finalHeight = h1 - (h2 + h3 + h4 + h5);
        $(".iFrame-chat").height(finalHeight);
    }
    $(".loaded-app-holder").each(function () {
        var iframeHt = Math.abs($(window).height() - ($("#always-visible").outerHeight() + $("#container-footer").outerHeight() + $("#notifications").outerHeight()));
        $(this).css({
            height: iframeHt
        });
    });
});