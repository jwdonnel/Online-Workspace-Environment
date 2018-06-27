// -----------------------------------------------------------------------------------
//
//	openWSE v6.4
//	by John Donnelly
//	Last Modification: 2/18/2018
//
//	Licensed under the Creative Commons Attribution 2.5 License - http://creativecommons.org/licenses/by/2.5/
//  	- Free for use in both personal and commercial projects
//		- Attribution requires leaving author name, author link, and the license info intact.
//
// -----------------------------------------------------------------------------------


/* Variable Assignments */
var openWSE_Config = {
    siteName: "",
    demoMode: false,
    siteTheme: "Standard",
    animationSpeed: 200,
    hoverPreviewWorkspace: false,
    taskBarShowAll: true,
    ShowWorkspaceNumApp: true,
    winMinWidth: 800,
    minPasswordLength: 6,
    workspaceMode: "",
    overlayPanelId: "pnl_OverlaysAll",
    reportAlert: true,
    reportOnError: false,
    siteRootFolder: "",
    displayLoadingOnRedirect: true,
    saveCookiesAsSessions: false,
    defaultBackgroundColor: "#FFFFFF",
    defaultBackgroundPosition: "right center",
    defaultBackgroundSize: "auto",
    defaultBackgroundRepeat: "repeat",
    appSnapHelper: false,
    appStyle: "Style_1",
    showToolTips: true,
    siteTipsOnPageLoad: false,
    ShowLoginModalOnDemoMode: false,
    multipleBackgrounds: false,
    backgroundTimerLoop: 30,
    appendTimestampOnScripts: true,
    timestampQuery: "vertimestamp=",
    useSlimScrollPlugin: true,
    groupLoginName: "",
    showScrollToTop: true,
    allowNavMenuCollapseToggle: false,
    allowNavMenuCollapseToggleOnlyOne: true,
    canvasWorkspaceAppPreviews: false,
    hideSearchBarInTopBar: false
};

var cookieFunctions = {
    set: function (e, t, n, promiseFunc) {
        if (openWSE_Config.saveCookiesAsSessions) {
            loadingPopup.Message("Saving. Please Wait...");
            openWSE.AjaxCall("WebServices/SaveControls.asmx/SetCookie", '{ "name": "' + e + '","value": "' + t + '" }', {
                promise: function () {
                    loadingPopup.RemoveMessage();
                    if (promiseFunc && typeof (promiseFunc) === "function") {
                        promiseFunc(t);
                    }
                }
            });
        }
        else {
            if (n) {
                var i = new Date;
                i.setTime(i.getTime() + n * 24 * 60 * 60 * 1e3);
                var s = "; expires=" + i.toGMTString()
            }
            else {
                var s = "";
            }

            document.cookie = e + "=" + t + s + "; path=/"

            if (promiseFunc && typeof (promiseFunc) === "function") {
                promiseFunc(t);
            }
        }
    },
    get: function (e, promiseFunc) {
        var cookieVal = null;
        if (openWSE_Config.saveCookiesAsSessions) {
            loadingPopup.Message("Loading. Please Wait...");
            openWSE.AjaxCall("WebServices/SaveControls.asmx/GetCookie", '{ "name": "' + e + '" }', {
                promise: function (result) {
                    loadingPopup.RemoveMessage();
                    if (result) {
                        cookieVal = result.d;
                    }

                    if (promiseFunc && typeof (promiseFunc) === "function") {
                        promiseFunc(cookieVal);
                    }
                }
            });
        }
        else {
            var t = e + "=";
            var n = document.cookie.split(";");
            for (var r = 0; r < n.length; r++) {
                var i = n[r];
                while (i.charAt(0) == " ") i = i.substring(1, i.length);
                if (i.indexOf(t) == 0) {
                    cookieVal = i.substring(t.length, i.length);
                    break;
                }
            }

            if (promiseFunc && typeof (promiseFunc) === "function") {
                promiseFunc(cookieVal);
            }
        }
    },
    del: function (e) {
        if (openWSE_Config.saveCookiesAsSessions) {
            loadingPopup.Message("Deleting. Please Wait...");
            openWSE.AjaxCall("WebServices/SaveControls.asmx/DelCookie", '{ "name": "' + e + '" }', null, null, null, function () {
                loadingPopup.RemoveMessage();
            });
        }
        else {
            cookieFunctions.set(e, "", -1)
        }
    }
}

var openWSE = function () {

    /* Private Variable Holders (DO NOT MODIFY) */
    var hf_r = "";
    var handler = "";
    var updateAppId = "";
    var aboutHolder = "";
    var autoupdaterunning = 0;
    var innerScrollPos = 0;
    var runningNoti = false;
    var runningMoreNoti = false;
    var innerModalContent = new Array();
    var totalHelpPages = 0;
    var canSortMyAppOverlay = false;
    var canSaveSortedMyAppOverlay = false;
    var saveHandler = "WebServices/SaveControls.asmx";
    var loadingMessage = "<div class='loading-background-holder' data-usespinner='true'><div></div></div>";
    var appMainClicked = 0;
    var needpostback = 0;
    var previewAppID = "";
    var previewHover = false;
    var appsToLoad = new Array();
    var maxBtn_InProgress = false;
    var minBtn_InProgress = false;
    var exitBtn_InProgress = false;
    var _topAboutPos = 0;
    var _leftAboutPos = 0;
    var canSaveSort = false;
    var uednTimeout;
    var pageLoadingTimeout;
    var topBarHt = 34;
    var bottomBarHt = 28;
    var currWinMode = "";
    var pagedIconClicked = false;
    var outsideAppModal = "-outside-modal-window";
    var resizingAppInProgress = false;
    var allOverlaysDisabled = false;
    var isMobileMode = false;
    var showAppPreviewInWorkspaceSelector = true;

    function init() {
        // openWSE.LogConsoleMessage(openWSE_Config);

        // Need to get correct path
        saveHandler = openWSE.siteRoot() + "WebServices/SaveControls.asmx";

        cookieFunctions.get("siteTipsOnPageLoad", function (siteTipCookie) {
            if ((siteTipCookie != "") && (siteTipCookie != null) && (siteTipCookie != undefined) && (openWSE_Config.demoMode)) {
                openWSE_Config.siteTipsOnPageLoad = ConvertBitToBoolean(siteTipCookie);
            }

            LoadViewPort();
            GetCurrentPage();
            LoadSidebarShowHideCookie();
            SidebarNavToggleInit();
            SiteTipsOnPageLoad();

            $(document).tooltip({ disabled: !openWSE_Config.showToolTips });

            if (openWSE_Config.ShowLoginModalOnDemoMode) {
                LoadLoginModal();
            }

            if ($("#top-logo-holder img").width() > 100) {
                $("#top-logo-holder span").hide();
            }
            if ($(".iframe-top-bar .iframe-title-logo img").width() > 100) {
                $(".iframe-top-bar .iframe-title-logo span.title-text").hide();
            }

            getCurrentScrollTopForMainContainer();
            RadioButtonStyle();
            InitializeSlimScroll();

            if (openWSE_Config.showScrollToTop && $("#main_container").length > 0) {
                var canContinue = true;
                if (window.location.href.toLowerCase().indexOf("filemanager") !== -1 && window.location.href.toLowerCase().indexOf("edit=") !== -1) {
                    canContinue = false;
                }

                if (canContinue) {
                    if ($(".container-main-bg-simple").length > 0 || $(".app-remote-container").length > 0) {
                        $("#main_container").css("padding-bottom", "65px");
                    }

                    $("#main_container").scroll(function () {
                        if ($(this).scrollTop() > 0) {
                            if ($("#main_container").find(".go-to-top").length === 0) {
                                $("#main_container").append("<a class=\"go-to-top\" title=\"Back to top\" onclick=\"openWSE.AnimateToTop();return false;\"></a>");
                            }

                            var footerHttemp = 0;
                            if ($("#footer_container").length > 0 && $("#footer_container").is(":visible")) {
                                footerHttemp = $("#footer_container").outerHeight();
                            }

                            $("#main_container").find(".go-to-top").css("bottom", (footerHttemp + 10) + "px");
                        }
                        else {
                            $("#main_container").find(".go-to-top").remove();
                        }
                    });
                }
            }

            SetPageTdSettingsTitle();
            SetupAccordions();

            if ($("#sidebar_accordian").length > 0 && $("#sidebar_accordian").attr("data-icononly") === "true") {
                $("#sidebar_accordian").find(".app-icon-links").each(function () {
                    var title = $.trim($(this).find(".app-icon-font").attr("data-pagetitle"));
                    if (title) {
                        $(this).attr("title", title);
                    }
                });
            }

            if ($("#pnl_icons").length > 0) {
                var $iconList = $("#pnl_icons").find(".app-icon.Icon_Only");
                if ($("#pnl_icons").find(".app-icon.Icon_And_Color_Only").length > 0) {
                    $iconList = $("#pnl_icons").find(".app-icon.Icon_And_Color_Only");
                }

                if ($iconList.length > 0) {
                    $iconList.each(function () {
                        var title = $.trim($(this).find(".app-icon-font").html());
                        if (title) {
                            $(this).attr("title", title);
                        }
                    });
                }
            }

            if (openWSE_Config.canvasWorkspaceAppPreviews && isCanvasSupported() && CheckIfOnWorkspace()) {
                openWSE.GetScriptFunction(openWSE.siteRoot() + "Scripts/jquery/html2canvas.js", null);
            }
        });

        openWSE.UpdateLogoSizeOnLoad($("#lnk_BackToWorkspace"));
    }
    function InitializeSlimScroll() {
        if (!resizingAppInProgress) {
            cookieFunctions.get("left-panel-scrolltop", function (scrollTopValue) {
                if (!scrollTopValue) {
                    scrollTopValue = 0;
                }
                else {
                    scrollTopValue = parseInt(scrollTopValue);
                }

                if (openWSE_Config.useSlimScrollPlugin) {
                    $("#sidebar_container").slimScroll({
                        color: "#AAA",
                        width: "",
                        height: $("#main_container").outerHeight() + $("#footer_container").outerHeight()
                    });

                    setTimeout(function () {
                        $("#sidebar_container").slimScroll({ scrollTo: scrollTopValue + "px" });
                    }, 1);
                }
                else {
                    setTimeout(function () {
                        $("#sidebar_container").scrollTop(scrollTopValue);
                    }, 1);
                }

                if ($("#sidebar_container").length > 0) {
                    var sidebarWidth = 0;
                    if ($("#main_container").length > 0) {
                        sidebarWidth = $("#main_container").position().left;
                    }

                    $("#sidebar_container, .slimScrollDiv").css("width", sidebarWidth + "px");
                }
            });
        }
    }
    function SetLeftSidebarScrollTop(adjustment) {
        var scrollTop = $("#sidebar_container").scrollTop();
        if (adjustment) {
            scrollTop += adjustment;
        }
        cookieFunctions.set("left-panel-scrolltop", scrollTop, "30");
    }
    function UpdateAppSelector() {
        $(".clear-applist").remove();

        if ($(".app-icon.Icon_Only").length > 0) {
            $("#updatePnl_AppList").append("<div class='clear-applist'></div>");

            if ($(".app-category-div").length > 0) {
                $(".app-icon.Icon_Only").each(function () {
                    if ($(this).parent().hasClass("app-category-div")) {
                        $(this).parent().addClass("inline-block");
                    }
                });
            }
        }
        else if ($(".app-icon.Icon_And_Text_Only").length > 0) {
            $("#updatePnl_AppList").append("<div class='clear-applist'></div>");

            if ($(".app-category-div").length > 0) {
                $(".app-icon.Icon_And_Text_Only").each(function () {
                    if ($(this).parent().hasClass("app-category-div")) {
                        $(this).parent().addClass("inline-block");
                    }
                });
            }
        }
        else if ($(".app-icon.Icon_And_Color_Only").length > 0) {
            $("#updatePnl_AppList").append("<div class='clear-applist'></div>");

            if ($(".app-category-div").length > 0) {
                $(".app-icon.Icon_And_Color_Only").each(function () {
                    if ($(this).parent().hasClass("app-category-div")) {
                        $(this).parent().addClass("inline-block");
                    }
                });
            }
        }
        else if ($(".app-icon.Icon_Plus_Color_And_Text").length > 0) {
            $("#updatePnl_AppList").append("<div class='clear-applist'></div>");

            if ($(".app-category-div").length > 0) {
                $(".app-icon.Icon_Plus_Color_And_Text").each(function () {
                    if ($(this).parent().hasClass("app-category-div")) {
                        $(this).parent().addClass("inline-block");
                    }
                });
            }
        }
    }
    function SetPageTdSettingsTitle() {
        var $pageTdSettingsTitle = $("#MainContent_pageTdSettingsTitle");
        if ($pageTdSettingsTitle.length === 0) {
            $pageTdSettingsTitle = $("#pageTdSettingsTitle");
        }

        if ($pageTdSettingsTitle.length > 0 && openWSE.ConvertBitToBoolean($pageTdSettingsTitle.attr("data-customimage")) && $pageTdSettingsTitle.parent().find(".title-line").length > 0) {
            $pageTdSettingsTitle.parent().find(".title-line").attr("data-customimage", "true");
        }
    }

    function SetDuplicateAppIcons() {
        var $appList = $("#updatePnl_AppList");
        if ($appList.length > 0) {
            $appList.find(".app-icon").each(function () {
                var appId = $(this).attr("data-appid");
                if ($appList.find(".app-icon[data-appid='" + appId + "']").length > 1) {
                    for (var i = 1; i < $appList.find(".app-icon[data-appid='" + appId + "']").length; i++) {
                        $appList.find(".app-icon[data-appid='" + appId + "']").eq(i).addClass("app-icon-duplicate");
                    }
                }
            });
        }
    }

    function LoadLoginModal() {
        if ($("#pnl_Login_NonAuth").length > 0) {
            var ele = "<div id='LoginModalPopup-element' class='Modal-element' style='display: none;'>";
            ele += "<div class='Modal-overlay'>";
            ele += "<div class='Modal-element-align'>";
            ele += "<div class='Modal-element-modal'>";

            // Header
            ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
            ele += "<a href='#' onclick=\"openWSE.CloseLoginModalWindow();return false;\" class='ModalExitButton'></a>";
            ele += "</div><span class='Modal-title'></span></div></div>";

            // Body

            ele += "<div class='ModalScrollContent'><div class='ModalPadContent'></div></div>";
            ele += "</div></div></div></div>";

            $("body").find("form[id='ct101']").append(ele);

            $("#pnl_Login_NonAuth").hide();

            $("#LoginModalPopup-element").find(".ModalPadContent").append($("#login-modal-div"));
            LoadModalWindow(true, "LoginModalPopup-element", $.trim($("#span_signinText").html()));

            $("#LoginModalPopup-element").find(".Modal-overlay").on("click", function (e) {
                if (e.target.className == "Modal-overlay") {
                    openWSE.CloseLoginModalWindow();
                }
            });

            openWSE.SetLeftSidebarScrollTop();
            $(window).resize();
        }
    }
    function CloseLoginModalWindow() {
        LoadModalWindow(false, "LoginModalPopup-element", "");

        if ($("#pnl_Login_NonAuth").length > 0) {
            $("#pnl_Login_NonAuth").find(".b").append($("#login-modal-div"));
        }

        $("#LoginModalPopup-element").remove();
        $("#pnl_Login_NonAuth").show();
    }

    function OnError(error, url) {
        openWSE.AjaxCall("WebServices/AppLog_Errors.asmx/AddError", '{ "message": "' + escape(error) + '","url": "' + escape(url) + '" }');
    }
    function SetContainerTopPos(adjustAll) {
        topBarHt = 0;
        bottomBarHt = $("#footer_container").outerHeight();

        if ($("#top_bar").css("display") !== "none" && $("#top_bar").length > 0) {
            topBarHt = $("#top_bar").outerHeight();
        }
        else if ($("#top_bar_toolview_holder").length > 0) {
            topBarHt = $("#top_bar_toolview_holder").outerHeight();
        }


        if ($("#footer_container").css("display") == "none") {
            bottomBarHt = 0;
        }

        $(".fixed-container-holder, .fixed-container-holder-background, .fixed-container-border-left, .fixed-container-border-right").css("top", topBarHt);
        $("#main_container, .administrator-workspace-note").css("bottom", bottomBarHt);

        if (adjustAll) {
            if ($("#iframe-content-src").length > 0) {
                $("#iframe-content-src").css("height", $(window).height() - $(".remove-on-iframe-close").outerHeight());
                $("#iframe-content-src").css("width", $(window).width());
            }

            openWSE.CheckIfCanAddMore();
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

                    var ele = "<div id='SiteTip-element-modal'>";

                    // Body
                    var nextTipButton = "<input class='input-buttons nextprev-button' type='button' value='Next Tip' onclick=\"openWSE.NextSiteTip();\" />";
                    var prevTipButton = "<input class='input-buttons nextprev-button' type='button' value='Previous Tip' onclick=\"openWSE.PreviousSiteTip();\" />";
                    if (tipArray.length == 1) {
                        nextTipButton = "";
                        prevTipButton = "";
                    }

                    var closeButton = "<input class='input-buttons confirm-close-button' type='button' value='Close' onclick=\"openWSE.CloseSiteTip();\" />";
                    var dontShowAgain = "<div class='dont-show-again'><input id='dont-show-again-cb' type='checkbox' checked='checked' /><label for='dont-show-again-cb'>Show Tips on Page Load</label></div>";

                    if (openWSE_Config.groupLoginName !== "") {
                        dontShowAgain = "";
                    }

                    if (openWSE_Config.siteTheme == "") {
                        openWSE_Config.siteTheme = "Standard";
                    }

                    var img = "<img alt='confirm' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/Icons/sitetip.png' />";
                    var tipMessage = tipArray[tipIndex];

                    ele += "<div class='tip-content-padding'>" + img + "<span class='tip-title'>Did you know?</span><div class='clear'></div><div class='message-text'>" + tipMessage + "</div>";
                    ele += "<div class='button-holder'>" + dontShowAgain + "<div class='clear-space'></div><div class='clear-space'></div>" + prevTipButton + nextTipButton + closeButton + "<div class='clear'></div></div></div>";
                    ele += "</div>";

                    $("body").append(ele);
                    $("#SiteTip-element-modal").fadeIn(openWSE_Config.animationSpeed * 2);

                    if (getParameterByName("mobileMode") === "true") {
                        $(document.body).on("click", "#main_container", function () {
                            if ($("#SiteTip-element-modal").length > 0) {
                                openWSE.CloseSiteTip();
                            }
                        });
                    }
                }
            });
        }
    }
    function NextSiteTip() {
        tipIndex++;
        if (tipIndex >= tipArray.length) {
            tipIndex = 0;
        }

        $("#SiteTip-element-modal").find(".message-text").html(tipArray[tipIndex]);
    }
    function PreviousSiteTip() {
        tipIndex--;
        if (tipIndex < 0) {
            tipIndex = tipArray.length - 1;
        }
        $("#SiteTip-element-modal").find(".message-text").html(tipArray[tipIndex]);
    }
    function CloseSiteTip() {
        if (!$("#dont-show-again-cb").prop("checked")) {
            if (!openWSE_Config.demoMode) {
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/TurnOffSiteTipsOnPageLoad");
            }
            else {
                cookieFunctions.set("siteTipsOnPageLoad", "false", "30");
            }
        }

        tipIndex = 0;
        tipArray = new Array();

        $('#SiteTip-element-modal').remove();
    }


    /* Set the Paged version of the Workspace */
    function PagedWorkspace(appId) {
        LoadCurrentWorkspace("1");
        if (appId != "") {
            if ($(".app-icon[data-appid='" + appId + "']").length != 0) {
                pagedIconClicked = true;
                var $this = $($(".app-icon[data-appid='" + appId + "']")[0]);
                $(".app-main-holder[data-appid='" + appId + "']").addClass("auto-full-page");
                $(".app-main-holder[data-appid='" + appId + "']").find(".app-head, .app-head-button-holder").hide();
                $this.trigger("click");

                var aboutBtnStr = "<div class='top-bar-menu-button'><ul><li class='a about-app-menu-toggle' title='About app' onclick=\"openWSE.AboutApp_PagedVersion('" + appId + "');return false;\"></li></ul></div>";
                var closeBtnStr = "<div class='top-bar-menu-button'><ul><li class='a close-app-menu-toggle' title='Close app' onclick=\"window.location.href='Default.aspx';return false;\"></li></ul></div>";
                if ($("#top-button-holder").find(".searchwrapper-tools-search").length > 0) {
                    $("#top-button-holder").find(".searchwrapper-tools-search").before(aboutBtnStr + closeBtnStr);
                }
                else {
                    $("#top-button-holder").append(closeBtnStr);
                }
            }
        }
    }
    function AboutApp_PagedVersion(appId) {
        if (appId) {
            $("#MainContent_pnl_aboutHolder").html("");
            $("#hf_aboutstatsapp").val("about;" + appId);
            loadingPopup.Message("Loading. Please Wait...");
            openWSE.CallDoPostBack("hf_aboutstatsapp", "");
        }
    }
    function IsComplexWorkspaceMode() {
        if (openWSE_Config.workspaceMode.toLowerCase() == "complex" || openWSE_Config.workspaceMode == "") {
            return true;
        }

        return false;
    }


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
    function CheckIfOnWorkspace() {
        if (window.location.href.toLowerCase().indexOf("/default.aspx") !== -1 || window.location.pathname.lastIndexOf("/") === window.location.pathname.length - 1) {
            return true;
        }

        return false;
    }
    function OpenMobileWorkspace() {
        var loc = openWSE.siteRoot() + 'AppRemote.aspx';
        if (location.href.indexOf('SiteTools/') > 0) {
            var tempSearchParms = "";
            loc = location.href;
            if (loc.indexOf("#?") != -1) {
                tempSearchParms = loc.split("#?")[1];
                loc = loc.replace("#?" + tempSearchParms, "");
            }

            loc = loc.replace("#", "");
            if (location.search == '') {
                loc += '?mobileMode=true&fromAppRemote=true';
            }
            else {
                loc += '&mobileMode=true&fromAppRemote=true';
            }

            if (tempSearchParms != "") {
                loc += "#?" + tempSearchParms;
            }
        }
        window.open(loc, '_blank', 'toolbar=no, scrollbars=yes, resizable=yes, width=340, height=550');
        return false;
    }

    function CheckIfOverlaysExistsOnNonComplex() {
        if ($(".no-overlays-found").length > 0) {
            $(".no-overlays-found").remove();
        }

        var message = "You have no overlays being shown. Click the add button to add overlays.";
        if (openWSE_Config.demoMode || $("#btn_addOverlayButton").length == 0) {
            message = "You have no overlays being shown.";
        }

        if (allOverlaysDisabled) {
            message = "Select an application to run";
        }

        if (getParameterByName("AppPage") == "" || getParameterByName("AppPage") == null) {
            if (openWSE_Config.overlayPanelId != "pnl_OverlaysAll" && !openWSE_Config.demoMode && !allOverlaysDisabled) {
                $("#btn_addOverlayButton").remove();
                $("#" + openWSE_Config.overlayPanelId).append("<div id='btn_addOverlayButton'><div class='clear-space'></div><a class='float-right margin-all' onclick='openWSE.CallOverlayList();return false;'>Add</a></div>");
                message = "You have no overlays being shown. Click the add button to add overlays.";
            }

            if ($(".workspace-overlay-selector").length == 0) {
                $("#" + openWSE_Config.overlayPanelId).append("<div class='pad-all-big no-overlays-found'>" + message + "</div>");
            }
        }
    }
    function DisableOverlaysOnPagedWorkspace() {
        allOverlaysDisabled = true;
        $("#btn_addOverlayButton").remove();
    }


    /* Auto Update System */
    var activeElementBeforeRequest;
    var activeElementTextBeforeRequest;
    var currScrollPos_main_container = 0;
    Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
        if (($("#pnl_aboutHolder").length > 0) && ($.trim($("#pnl_aboutHolder").html()) != "") && ($("#aboutApp-element").css("display") == "block")) {
            aboutHolder = $.trim($("#pnl_aboutHolder").html());
        }

        SaveInnerModalContent(args);

        if ($("#main_container").length > 0) {
            currScrollPos_main_container = $("#main_container").scrollTop();
        }

        activeElementBeforeRequest = null;
        activeElementTextBeforeRequest = null;

        try {
            if (document.activeElement && document.activeElement.localName.toLowerCase() != "body") {
                activeElementBeforeRequest = $(document.activeElement).attr("id");
                if (activeElementBeforeRequest) {
                    activeElementBeforeRequest = "#" + activeElementBeforeRequest;
                    if (document.activeElement.localName.toLowerCase() == "input" && document.activeElement.type.toLowerCase() == "text") {
                        activeElementTextBeforeRequest = document.activeElement.value;
                    }
                }
            }
        }
        catch (evt) { }

        setCurrentScrollTopForMainContainer();
        SaveAccordionState(true);
        SetLeftSidebarScrollTop();
    });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        $(".ui-tooltip-content").parents('div').remove();
        if (aboutHolder != "") {
            $("#MainContent_pnl_aboutHolder").html(aboutHolder);
            aboutHolder = "";
        }

        loadingPopup.RemoveMessage();

        // Load Saved Modal Content if needed
        LoadSavedInnerModalContent();

        // Refresh Notifications if available
        RefreshNotifications();

        // Start the Auto Update System
        autoupdate(hf_r, handler, updateAppId);

        ShowNewNotificationPopup();

        // AdjustTableSettingsBox();

        CheckIfOverlaysExistsOnNonComplex();

        if ($("#main_container").length > 0 && currScrollPos_main_container >= 0) {
            setTimeout(function () {
                $("#main_container").scrollTop(currScrollPos_main_container);
                currScrollPos_main_container = 0;
            }, 1);
        }

        setTimeout(function () {
            try {
                if (activeElementBeforeRequest && activeElementBeforeRequest != null) {
                    $(activeElementBeforeRequest).focus();
                    if (activeElementTextBeforeRequest && activeElementTextBeforeRequest != null) {
                        $(activeElementBeforeRequest).val(activeElementTextBeforeRequest);
                    }
                }
            }
            catch (evt) { }

            activeElementBeforeRequest = null;
            activeElementTextBeforeRequest = null;
        }, 1);

        getCurrentScrollTopForMainContainer();
        RadioButtonStyle();

        openWSE.SetDuplicateAppIcons();
        openWSE.ToggleMinimizedAppBar();
        getCurrentPostitionForModalWindow();
        SetupAccordions(true);
        ReapplyActiveIcons();
        $(window).resize();
    });
    function autoupdate(_hf_r, _handler, _updateAppId) {
        if ((hf_r == "") && (_hf_r != "")) {
            hf_r = _hf_r;
        }
        if ((handler == "") && (_handler != "")) {
            handler = _handler;
        }
        if ((updateAppId == "") && (_updateAppId != "")) {
            updateAppId = _updateAppId;
        }

        if ((hf_r != "") && (handler != "") && (updateAppId != "")) {
            if (autoupdaterunning == 0) {
                openWSE.AjaxCall(_handler, '{ "_appId": "' + escape(updateAppId) + '" }', null, function (msg) {
                    var response = msg.d[0];
                    if (response == "TURNOFF") {
                        autoupdaterunning = 1;
                        return;
                    }
                    else if (response == "refresh") {
                        autoupdaterunning = 0;
                        document.getElementById(hf_r).value = "refresh";
                        openWSE.CallDoPostBack(hf_r, "");
                    }
                    else if (response == "workspace-check") {
                        autoupdaterunning = 0;
                        autoupdate(hf_r, handler, updateAppId);
                    }
                    else if ((response != "false") && (response != "")) {
                        try {
                            if (openWSE.ConvertBitToBoolean(msg.d[1])) {
                                autoupdaterunning = 0;
                                loadingPopup.Message("Receiving Request...");
                                StartRemoteLoad(response, hf_r, handler, updateAppId);
                            }
                            else {
                                autoupdaterunning = 0;
                                document.getElementById(hf_r).value = response;
                                openWSE.CallDoPostBack(hf_r, "");
                            }
                        }
                        catch (evt) {
                            autoupdaterunning = 0;
                            autoupdate(hf_r, handler, updateAppId);
                        }
                    }
                    else {
                        autoupdaterunning = 0;
                        autoupdate(hf_r, handler, updateAppId);
                    }
                }, function (e) {
                    autoupdaterunning = 0;
                    autoupdate(hf_r, handler, updateAppId);
                });
                autoupdaterunning = 1;
            }
        }
    };
    function AdjustTableSettingsBox() {
        if ($(".pnl-section").length == 0 && $(".table-settings-box").length > 0) {
            $(".table-settings-box").eq($(".table-settings-box").length - 1).addClass("no-border");
        }
        else if ($(".pnl-section").length > 0 && $(".table-settings-box").length > 0) {
            $(".pnl-section").each(function () {
                var lastIndex = $(this).find(".table-settings-box").length;
                $(this).find(".table-settings-box").eq(lastIndex - 1).addClass("no-border");
            });
        }
    }

    function setCurrentScrollTopForMainContainer() {
        var $currDiv = $("#main_container");
        if ($currDiv.length === 0) {
            $currDiv = $("body");
        }

        cookieFunctions.set("currScrollTop", $currDiv.scrollTop(), "30");
    }
    function getCurrentScrollTopForMainContainer() {
        setTimeout(function () {
            var $currDiv = $("#main_container");
            if ($currDiv.length === 0) {
                $currDiv = $("body");
            }

            cookieFunctions.get("currScrollTop", function (currScrollTop) {
                if (currScrollTop) {
                    $currDiv.scrollTop(currScrollTop);
                    cookieFunctions.del("currScrollTop");
                }
            });
        }, 150);
    }

    $(document.body).on("click", ".RandomActionBtns", function () {
        if ($(this).closest(".searchwrapper").length != 0) {
            loadingPopup.Message("Searching...");
        }
        else {
            loadingPopup.Message("Updating. Please Wait...");
        }
    });
    $(document.body).on("click", "#lbtn_signoff", function () {
        loadingPopup.Message("Logging Off. Please Wait...");
    });
    $(document.body).on("keypress", ".searchwrapper-tools-search > input[type='text']", function (e) {
        if (e.which == 13) {
            $(this).focus();
            e.preventDefault();
            SearchSite();
        }
    });
    $(document.body).on("click", ".content-main, .Modal-overlay, #iframe-container-helper, .fixed-container-holder-background", function (e) {
        if ($(e.target).closest("#overlayEdit-element").length === 0) {
            CloseTopDropDowns();
        }
    });
    $(document.body).on("click", ".top-bar-menu-button li.a, .top-bar-userinfo-button li.a", function () {
        var eleId = $(this).closest(".top-bar-menu-button, .top-bar-userinfo-button").attr("id");
        var $b = $(this).next();
        if ($b.length == 1) {
            if (!$b.is(":visible")) {
                CloseTopDropDowns();

                if (isMobileDevice() || window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1) {
                    $("body").append("<div class='sidebar-overlay' onclick='openWSE.CloseTopDropDowns();' style='display: block;'></div>");
                }

                $b.show();
                ResizeTopDropDowns();

                switch (eleId) {
                    case "notifications_tab":
                        openWSE.GetUserNotifications();
                        break;
                    case "background_tab":
                        openWSE.BackgroundSelector(true);
                        break;
                    case "group_tab":
                        openWSE.GroupLoginModal();
                        break;
                    case "settings_tab":
                        openWSE.InitializeThemeColorOption("div_ColorOptionsHolder");
                        break;
                    case "workspace-selector":
                        openWSE.SetBackgroundForWorkspaceDropdown();
                        break;
                }

                $(this).closest(".top-bar-menu-button, .top-bar-userinfo-button").addClass("active");
            }
            else {
                CloseTopDropDowns();
            }
        }
        else {
            CloseTopDropDowns();
        }
    });
    $(document.body).on("click", ".help-icon", function () {
        HelpOverlay(false);
    });
    $(document.body).on("click", ".workspace-reminder", function () {
        var $_this = $(this).parent();
        $(".app-popup").css("display", "none");
        $popup = $_this.find(".app-popup");
        $popup.css("display", "block");
        $popup.fadeIn(openWSE_Config.animationSpeed);
        return false;
    });

    $(document.body).on("mouseover", ".app-icon", function (e) {
        var $options = $(this).find(".app-options");
        if ($options.length > 0) {
            $options.css("visibility", "visible");
        }
    });
    $(document.body).on("mouseleave", ".app-icon", function (e) {
        var $options = $(this).find(".app-options");
        var $popup = $(this).find(".app-popup");
        if ($options.length > 0 && $popup.length > 0) {
            $options.css("visibility", "hidden");
            $popup.hide();
        }
    });
    $(document.body).on("mouseleave", ".app-popup", function (e) {
        $(this).hide();
    });

    $(document.body).on("click", ".app-icon", function () {
        if (window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1) {
            return true;
        }

        var $this = $(this);
        CloseTopDropDowns();

        if ($this.hasClass("app-icon-parms")) {
            return false;
        }

        if (!openWSE.IsComplexWorkspaceMode() && !pagedIconClicked && window.location.href.toLowerCase().indexOf("appmanager.aspx") == -1) {
            var queryHref = "?AppPage=" + $this.attr("data-appId");
            var splitHref = window.location.href.split(/default.aspx/i);
            if (CheckIfOnWorkspace() && splitHref.length === 1) {
                splitHref.push("");
            }

            if (splitHref.length > 1 && splitHref[splitHref.length - 1]) {
                if (splitHref[splitHref.length - 1].toLowerCase().indexOf("apppage=") === -1) {
                    queryHref = splitHref[splitHref.length - 1] + "&AppPage=" + $this.attr("data-appId");
                }
                else {
                    var splitHref2 = splitHref[splitHref.length - 1].substring(1).split("&");
                    if (splitHref2.length > 1) {
                        queryHref = "";
                        for (var i = 0; i < splitHref2.length; i++) {
                            if (splitHref2[i] && splitHref2[i].toLowerCase().indexOf("apppage") === -1) {
                                if (queryHref) {
                                    queryHref += "&" + splitHref2[i];
                                }
                                else {
                                    queryHref += "?" + splitHref2[i];
                                }
                            }
                        }

                        if (queryHref) {
                            queryHref += "&AppPage=" + $this.attr("data-appId");
                        }
                        else {
                            queryHref += "?AppPage=" + $this.attr("data-appId");
                        }
                    }
                }
            }

            window.location.href = openWSE.siteRoot() + "Default.aspx" + queryHref;
        }
        else if ($("#workspace_holder").length > 0) {
            var workspace = Getworkspace();

            var _appId = $this.attr("data-appid");
            var $thisApp = $(".app-main-holder[data-appid='" + _appId + "']");
            if ($thisApp.length > 0 && $thisApp.hasClass("app-min-bar-preview")) {
                $thisApp.css("opacity", "0.0");
                $thisApp.css("filter", "alpha(opacity=0)");
                $thisApp.removeClass("app-min-bar-preview");
                ResizeAppBody($thisApp);
            }

            LoadApp($this, workspace);
            if ($this.hasClass("active") == false) {
                $this.addClass("active");
            }
        }
        else if ($("#pnl_icons").length > 0) {
            BuildOpenAppPopup(this);
        }

        pagedIconClicked = false;

        return false;
    });
    $(document.body).on("click", ".minimize-button-app", function () {
        if (!minBtn_InProgress) {
            CloseTopDropDowns();
            minBtn_InProgress = true;
            var id = $(this).attr("href").replace("#", "");
            var $_id = $(".app-main-holder[data-appid='" + id + "']");

            var name = $_id.find(".app-title").eq(0).text();

            var _leftPos = $_id.css("left");
            var _topPos = $_id.css("top");
            var _width = $_id.width();
            var _height = $_id.height();
            var workspace = Getworkspace();

            if ($(".app-min-bar[data-appid='" + id + "']").length == 0) {
                var leftAppBarPos = 0;
                var topAppBarPos = 0;
                if (!$_id.hasClass("app-maximized") && !$_id.hasClass("auto-full-page")) {
                    topAppBarPos = (($(window).height() - $("#top_bar").outerHeight()) / 2) - ($_id.outerHeight() / 2);
                    topAppBarPos = topAppBarPos - 25;
                    if (topAppBarPos < 0) {
                        topAppBarPos = 0;
                    }
                }

                $_id.animate({ opacity: 0.0, left: leftAppBarPos, top: topAppBarPos }, openWSE_Config.animationSpeed, function () {
                    BuildAppMinIcon($_id, name, _leftPos, _topPos);
                    MoveOffScreen($_id);
                    SetContainerTopPos(true);

                    openWSE.AjaxCall(saveHandler + "/App_Minimize", '{ "appId": "' + id + '","name": "' + name + '","x": "' + _leftPos + '","y": "' + _topPos + '","width": "' + _width + '","height": "' + _height + '","workspace": "' + workspace + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                        minBtn_InProgress = false;
                    }, function (data) {
                        minBtn_InProgress = false;
                    });
                });
            }
        }
        return false;
    });
    $(document.body).on("dblclick", ".app-head-dblclick", function () {
        if (!maxBtn_InProgress) {
            maxBtn_InProgress = true;
            MaximizeApp($(this).closest(".app-main-holder"));
        }
        return false;
    });
    $(document.body).on("click", ".maximize-button-app", function () {
        if (!maxBtn_InProgress) {
            CloseTopDropDowns();
            maxBtn_InProgress = true;
            MaximizeApp($(this).closest(".app-main-holder"));
        }
        return false;
    });
    $(document.body).on("click", ".exit-button-app, .exit-button-app-min", function () {
        if (!exitBtn_InProgress) {
            CloseTopDropDowns();
            exitBtn_InProgress = true;
            var id = $(this).attr("href").replace("#", "");

            $(".app-snap-helper[data-appid='" + id + "']").remove();

            var $_id = $(".app-main-holder[data-appid='" + id + "']");

            if ($(".app-min-bar[data-appid='" + id + "']").length > 0) {
                $(".app-min-bar[data-appid='" + id + "']").remove();
                SetContainerTopPos(true);
            }

            var name = $_id.find(".app-title").eq(0).text();

            if ($_id.hasClass("app-min-bar-preview")) {
                $_id.css("opacity", "0.0");
                $_id.css("filter", "alpha(opacity=0)");
                $_id.removeClass("app-min-bar-preview");
                previewHover = false;
                previewAppID = "";
                SetAppMinToMax(id);
            }

            RemoveworkspaceAppNum($_id);
            RemoveAppIconActive($_id);

            $_id.fadeOut(openWSE_Config.animationSpeed, function () {
                if ($_id.hasClass("app-maximized")) {
                    $_id.removeClass("app-maximized");
                }

                if ($_id.attr("data-appid").indexOf("app-ChatClient-") != -1) {
                    $_id.remove();
                }

                var canclose = 1;
                var hfcanclose = document.getElementById("hf_" + id.replace(/#/gi, ""));

                if (hfcanclose != null) {
                    canclose = 0;
                }
                if ($_id.find(".app-body").find("div").html() == null) {
                    if (canclose == 1) {
                        $_id.find(".app-body").html("");
                        // AppendLoadingMessage($_id.find(".app-body"));
                    }
                }
                else {
                    if (canclose == 1) {
                        $_id.find(".app-body").find("div").html("");
                        // AppendLoadingMessage($_id.find(".app-body").find("div"));
                    }
                }

                $_id.css({
                    visibility: "hidden",
                    left: "",
                    top: "",
                    width: "",
                    height: ""
                });


                $_id.find(".app-body").css({
                    height: "",
                    width: ""
                });

                if ($_id.find(".options-button-app").length > 0) {
                    $_id.find(".options-button-app").removeClass("active");
                    $_id.find(".app-popup-inner-app").hide();
                    if (openWSE_Config.appStyle == "Style_3") {
                        $_id.find(".app-head-button-holder").hide();
                        optionsOpen_Style3 = false;
                    }
                }

                var modalClasses = $_id.attr("data-appid") + outsideAppModal;
                $("." + modalClasses).each(function () {
                    $(this).remove();
                });

                RemoveCSSFilesOnAppClose($_id.attr("data-appid"));

                openWSE.AjaxCall(saveHandler + "/App_Close", '{ "appId": "' + id + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                    exitBtn_InProgress = false;
                }, function (data) {
                    exitBtn_InProgress = false;
                });
            });
        }

        openWSE.ToggleMinimizedAppBar();
        return false;
    });
    $(document.body).on("click", ".app-min-bar", function () {
        CloseTopDropDowns();

        var _appId = $(this).attr("data-appid");
        var $thisApp = $(".app-main-holder[data-appid='" + _appId + "']");
        if ($thisApp.hasClass("app-min-bar-preview")) {
            $thisApp.css("opacity", "0.0");
            $thisApp.css("filter", "alpha(opacity=0)");
            $thisApp.removeClass("app-min-bar-preview");
        }

        var workspace = Getworkspace();
        openWSE.SetLeftSidebarScrollTop();
        LoadApp($thisApp, workspace);
        $(window).resize();
        return false;
    });
    $(document.body).on("click", ".app-main-holder", function (e) {
        SetActiveApp(this);
        appMainClicked = 1;
    });
    $(document.body).on("click", ".workspace-holder", function () {
        if (openWSE.IsComplexWorkspaceMode()) {
            if (appMainClicked == 0) {
                $(".app-main-holder").removeClass("selected");
                SetDeactiveAll();
            }
        }

        appMainClicked = 0;
    });
    $(document.body).on("click", ".options-button-app", function () {
        CloseTopDropDowns();

        var $_id = $(this);
        var $_parent = $_id.parent();
        if ($_parent.find(".app-popup-inner-app").css("display") == "block") {
            $_id.removeClass("active");
            $_parent.find(".app-popup-inner-app").slideUp(openWSE_Config.animationSpeed);
            if (openWSE_Config.appStyle == "Style_3") {
                optionsOpen_Style3 = false;
            }
        }
        else {
            $_id.addClass("active");
            var $ddSelector = $_parent.find(".app-popup-inner-app").find(".app-options-workspace-switch");
            if ($ddSelector.length > 0) {
                var currDb = $_id.closest(".workspace-holder").attr("id").replace("MainContent_workspace_", "");
                $ddSelector.val(currDb);
            }

            $_parent.find(".app-popup-inner-app").slideDown(openWSE_Config.animationSpeed);
            SetActiveApp($_parent.parent());

            if (openWSE_Config.appStyle == "Style_3") {
                optionsOpen_Style3 = true;
            }
        }

        return false;
    });
    $(document.body).on("change", ".app-popup-selector, .app-options-workspace-switch", function () {
        if ($.trim($(this).val()) != "" && $.trim($(this).val()) != "-") {
            if ($("#workspace_holder").length > 0) {
                MoveAppToworkspace(this);
                var $_id = $(this).closest(".app-head-button-holder").parent().find(".options-button-app");
                if ($_id.length > 0) {
                    var $_parent = $_id.parent();
                    $_id.removeClass("active");
                    $_parent.find(".app-popup-inner-app").hide();

                    if (openWSE_Config.appStyle == "Style_3") {
                        $_parent.find(".app-head-button-holder").hide();
                        optionsOpen_Style3 = false;
                    }
                }
            }
            else {
                var $this = $(this).closest(".app-icon");
                BuildOpenAppPopup($this);
            }
        }
    });
    $(document.body).on("click", ".app-options", function () {
        CloseTopDropDowns();
        $(".app-popup").css("display", "none");
        $(".app-popup-holder").css("margin-left", "");

        var $popup = $(this).find(".app-popup");
        $popup.show();
        var popupWidthPos = $popup.offset().left + $popup.outerWidth();
        if (popupWidthPos + 20 > $("#sidebar_container").outerWidth() + $("#sidebar_container").offset().left) {
            $popup.find(".app-popup-holder").css("margin-left", "-" + (Math.abs((popupWidthPos - ($("#sidebar_container").outerWidth() + $("#sidebar_container").offset().left)) + 20)).toString() + "px");
        }

        $popup.find(".app-popup-selector").val("-");

        $(".ui-tooltip").remove();

        return false;
    });
    $(document.body).on("keypress", "input[type='text'], textarea", function (e) {
        if (!$(this).hasClass("mce-textbox") && !$(this).parent().hasClass("ace_editor")) {
            var allowAllChars = $(this).attr("data-allowallchars");
            if (!allowAllChars) {
                var code = (e.which) ? e.which : e.keyCode;
                var val = String.fromCharCode(code);

                if (!val.match('^.*?(?=[\^"#%&$\*<>\?\{\|\}]).*$')) {
                    return false;
                }
            }
        }
    });


    /* Admin Online Information */
    function SiteStatusAdmin_Clicked() {
        loadingPopup.Message("Loading Information...");
        openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/AdminOnlineInformation", "{ }", {
            dataFilter: function (data) { return data; }
        }, function (data) {
            loadingPopup.RemoveMessage();
            AdminOnlineInformationWindow(data);
        }, function (data) {
            openWSE.AlertWindow("An error occurred. Please try again.");
        });
    }
    function AdminOnlineInformationWindow(data) {
        var dataStr = $.trim(data.d);
        if (dataStr == "") {
            dataStr = "No information available";
        }

        var ele = "<div id='AdminOnlineInformation-element' class='Modal-element' style='display: none;'>";
        ele += "<div class='Modal-overlay'>";
        ele += "<div class='Modal-element-align'>";
        ele += "<div class='Modal-element-modal' data-setwidth='600'>";

        // Header
        ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
        ele += "<a href='#' onclick=\"openWSE.CloseAdminOnlineInformationWindow();return false;\" class='ModalExitButton'></a>";
        ele += "</div><span class='Modal-title'></span></div></div>";

        // Body
        var topMessage = "You can view more information regarding the status of the site by clicking <a href='" + openWSE.siteRoot() + "SiteTools/NetworkMaintenance/Analytics.aspx'>here</a><div class='clear-space'></div>";
        var okButton = "<input class='input-buttons modal-cancel-btn' type='button' value='Close' onclick=\"openWSE.CloseAdminOnlineInformationWindow();\" />";
        ele += "<div class='ModalScrollContent'><div class='ModalPadContent'>" + topMessage + dataStr + "<div class='clear'></div></div></div>";
        ele += "<div class='ModalButtonHolder'>" + okButton + "<div class='clear'></div></div>";
        ele += "</div></div></div></div>";

        $("body").append(ele);
        LoadModalWindow(true, "AdminOnlineInformation-element", "Site Information");
    }
    function CloseAdminOnlineInformationWindow() {
        LoadModalWindow(false, "AdminOnlineInformation-element", "");
        $("#AdminOnlineInformation-element").remove();
    }


    /* Alert Window */
    function AlertWindow(error, url) {
        if (error && error.statusText) {
            error = error.statusText;
        } 
        
        if (!error || typeof (error) !== "string") {
            error = "An error has occured that was not specified.";
        }

        if (error === "error" || error === "Script error." || error === "Internal Server Error") {
            return;
        }
        
        var ele = "<div id='AlertWindow-element' class='Modal-element' style='display: none;'>";
        ele += "<div class='Modal-overlay'>";
        ele += "<div class='Modal-element-align'>";
        ele += "<div class='Modal-element-modal' data-setwidth='370'>";

        // Header
        ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
        ele += "<a href='#' onclick=\"openWSE.CloseAlertWindow();return false;\" class='ModalExitButton'></a>";
        ele += "</div><span class='Modal-title'></span></div></div>";

        // Body
        var okButton = "<input class='input-buttons confirm-ok-button modal-ok-btn' type='button' value='Ok' onclick=\"openWSE.CloseAlertWindow();\" />";
        var reportBtn = "";
        if (openWSE_Config.reportAlert) {
            reportBtn = "<input class='input-buttons confirm-report-button modal-cancel-btn' type='button' value='Report' onclick=\"openWSE.ReportAlert('" + escape(error) + "','" + escape(url) + "');\" />";
        }

        if (openWSE_Config.siteTheme == "") {
            openWSE_Config.siteTheme = "Standard";
        }

        var img = "<img alt='confirm' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/Icons/SiteMaster/alert.png' />";

        ele += "<div class='ModalScrollContent'><div class='ModalPadContent'><div class='message-text'><table cellpadding='0' cellspacing='0' width='100%'><tr><td valign='top' style='width: 50px;'>" + img + "</td><td valign='middle'>" + error + "</td></tr></table><div class='clear'></div></div></div></div>";
        ele += "<div class='ModalButtonHolder'>" + okButton + reportBtn + "<div class='clear'></div></div>";
        ele += "</div></div></div></div>";

        $("body").append(ele);
        LoadModalWindow(true, "AlertWindow-element", "Message");
        setTimeout(function () {
            $("#AlertWindow-element").find(".confirm-ok-button").focus();
            loadingPopup.RemoveMessage();
            loadingPopup.BindF5KeyPress();
        }, 250);
    }
    function CloseAlertWindow() {
        LoadModalWindow(false, "AlertWindow-element", "");
        if ($("#AlertWindow-element").length > 0) {
            $("#AlertWindow-element").hide();
            $("#AlertWindow-element").remove();
        }

        $("body").removeClass("modal-fixed-position-body");
        if ($("#site_mainbody").length > 0 && $("#site_mainbody").attr("data-layoutoption") === "Boxed") {
            $(".fixed-container-border-left").find(".Modal-overlay").remove();
            $(".fixed-container-border-right").find(".Modal-overlay").remove();
            $(".fixed-footer-container-left").find(".Modal-overlay").remove();
            $(".fixed-footer-container-right").find(".Modal-overlay").remove();
        }
        loadingPopup.UnbindF5KeyPress();
    }
    function ReportAlert(error, url) {
        if (url == "" || url == null) {
            url = window.location.href;
        }
        OnError(unescape(error), unescape(url));
        CloseAlertWindow();
    }


    /* Confirm Window */
    function ConfirmWindow(message, okCallback, cancelCallback) {
        if (message == "" || message == null) {
            message = "Are you sure you want to continue?";
        }

        var ele = "<div id='ConfirmWindow-element' class='Modal-element' style='display: none;'>";
        ele += "<div class='Modal-overlay'>";
        ele += "<div class='Modal-element-align'>";
        ele += "<div class='Modal-element-modal' data-setwidth='370'>";

        // Header
        ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
        ele += "<a href='#' onclick=\"openWSE.CloseConfirmWindow();return false;\" class='ModalExitButton confirm-cancel-button-header'></a>";
        ele += "</div><span class='Modal-title'></span></div></div>";

        // Body
        var okButton = "<input class='input-buttons confirm-ok-button modal-ok-btn' type='button' value='Ok' />";
        var cancelButton = "<input class='input-buttons confirm-cancel-button modal-cancel-btn' type='button' value='Cancel' />";

        if (openWSE_Config.siteTheme == "") {
            openWSE_Config.siteTheme = "Standard";
        }

        var img = "<img alt='confirm' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/Icons/SiteMaster/confirm.png' />";
        ele += "<div class='ModalScrollContent'><div class='ModalPadContent'><div class='message-text'><table cellpadding='0' cellspacing='0' width='100%'><tr><td valign='top' style='width: 50px;'>" + img + "</td><td valign='middle'>" + message + "</td></tr></table><div class='clear'></div></div></div></div>";
        ele += "<div class='ModalButtonHolder'>" + okButton + cancelButton + "<div class='clear'></div></div>";
        ele += "</div></div></div></div>";

        $("body").append(ele);

        $("#ConfirmWindow-element").find(".confirm-ok-button").one("click", function () {
            openWSE.CloseConfirmWindow();
            if (okCallback != null) {
                okCallback();
            }
        });
        $("#ConfirmWindow-element").find(".confirm-cancel-button, .confirm-cancel-button-header").one("click", function () {
            openWSE.CloseConfirmWindow();
            if (cancelCallback != null) {
                cancelCallback();
            }
        });

        LoadModalWindow(true, "ConfirmWindow-element", "Confirmation");
        setTimeout(function () {
            $("#ConfirmWindow-element").find(".confirm-ok-button").focus();
            loadingPopup.RemoveMessage();
            loadingPopup.BindF5KeyPress();
        }, 250);
    }
    function ConfirmWindowAltBtns(message, okCallback, cancelCallback) {
        if (message == "" || message == null) {
            message = "Are you sure you want to continue?";
        }

        var ele = "<div id='ConfirmWindow-element' class='Modal-element' style='display: none;'>";
        ele += "<div class='Modal-overlay'>";
        ele += "<div class='Modal-element-align'>";
        ele += "<div class='Modal-element-modal' data-setwidth='370'>";

        // Header
        ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
        ele += "<a href='#' onclick=\"openWSE.CloseConfirmWindow();return false;\" class='ModalExitButton confirm-cancel-button-header'></a>";
        ele += "</div><span class='Modal-title'></span></div></div>";

        // Body
        var okButton = "<input class='input-buttons confirm-ok-button modal-ok-btn' type='button' value='Yes' />";
        var cancelButton = "<input class='input-buttons confirm-cancel-button modal-cancel-btn' type='button' value='No' />";

        if (openWSE_Config.siteTheme == "") {
            openWSE_Config.siteTheme = "Standard";
        }

        var img = "<img alt='confirm' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/Icons/SiteMaster/confirm.png' />";
        ele += "<div class='ModalScrollContent'><div class='ModalPadContent'><div class='message-text'><table cellpadding='0' cellspacing='0' width='100%'><tr><td valign='top' style='width: 50px;'>" + img + "</td><td valign='middle'>" + message + "</td></tr></table><div class='clear'></div></div></div></div>";
        ele += "<div class='ModalButtonHolder'>" + okButton + cancelButton + "<div class='clear'></div></div>";
        ele += "</div></div></div></div>";

        $("body").append(ele);

        $("#ConfirmWindow-element").find(".confirm-ok-button").one("click", function () {
            openWSE.CloseConfirmWindow();
            if (okCallback != null) {
                okCallback();
            }
        });
        $("#ConfirmWindow-element").find(".confirm-cancel-button, .confirm-cancel-button-header").one("click", function () {
            openWSE.CloseConfirmWindow();
            if (cancelCallback != null) {
                cancelCallback();
            }
        });

        LoadModalWindow(true, "ConfirmWindow-element", "Confirmation");
        setTimeout(function () {
            $("#ConfirmWindow-element").find(".confirm-ok-button").focus();
            loadingPopup.RemoveMessage();
        }, 250);
    }
    function CloseConfirmWindow() {
        LoadModalWindow(false, "ConfirmWindow-element", "");
        if ($("#ConfirmWindow-element").length > 0) {
            $("#ConfirmWindow-element").hide();
            $("#ConfirmWindow-element").remove();
        }

        $("body").removeClass("modal-fixed-position-body");
        if ($("#site_mainbody").length > 0 && $("#site_mainbody").attr("data-layoutoption") === "Boxed") {
            $(".fixed-container-border-left").find(".Modal-overlay").remove();
            $(".fixed-container-border-right").find(".Modal-overlay").remove();
            $(".fixed-footer-container-left").find(".Modal-overlay").remove();
            $(".fixed-footer-container-right").find(".Modal-overlay").remove();
        }
        loadingPopup.UnbindF5KeyPress();
    }


    /* Window Load and Resize */
    function OnBrowserClose() {
        openWSE.AjaxCall("WebServices/AcctSettings.asmx/OnBrowserClose", "", {
            dataFilter: function (data) { return data; },
            promise: function () { }
        });
    }
    function CheckIfWorkspaceLinkAvailable() {
        var hasWorkspaceLink = false;
        if ($("#pnl_Login_NonAuth").length > 0) {
            hasWorkspaceLink = true;
        }
        else {
            $(".site-tools-tablist").each(function (index) {
                $(".site-tools-tablist").eq(index).find(".app-icon-links").each(function () {
                    var $this = $(this).find(".app-icon-font");
                    if ($this.attr("data-pagetitle").toLowerCase() == "workspace") {
                        hasWorkspaceLink = true;
                    }
                });
            });
        }

        return hasWorkspaceLink;
    }


    /* Radio Button Styling */
    var triggerRadioButtonClick = false;
    $(document.body).on("click", ".RadioButton-Toggle-Overlay", function () {
        var $switch = $(this).closest(".switch-slider");

        triggerRadioButtonClick = true;
        if ($switch.find(".cb-disable").hasClass("selected")) {
            if ($switch.find(".cb-enable").hasClass("no-postback")) {
                $switch.find(".cb-enable").find("input").prop("checked", true);
                $switch.find(".cb-disable").find("input").prop("checked", false);
                $switch.find(".cb-enable").addClass("selected");
                $switch.find(".cb-disable").removeClass("selected");
            }

            $switch.animate({
                left: 0
            }, openWSE_Config.animationSpeed, function () {
                $switch.find(".cb-enable").find('input').trigger('click');
            });
        }
        else {
            if ($switch.find(".cb-disable").hasClass("no-postback")) {
                $switch.find(".cb-enable").find("input").prop("checked", false);
                $switch.find(".cb-disable").find("input").prop("checked", true);
                $switch.find(".cb-enable").removeClass("selected");
                $switch.find(".cb-disable").addClass("selected");
            }

            var disabledPos = ($switch.parent().outerWidth() - $switch.find(".RadioButton-Toggle-Overlay").outerWidth()) + 2;
            $switch.animate({
                left: -disabledPos
            }, openWSE_Config.animationSpeed, function () {
                $switch.find(".cb-disable").find('input').trigger('click');
            });
        }
    });
    $(document.body).on("click", ".cb-enable, .cb-disable", function () {
        if (!triggerRadioButtonClick) {
            return false;
        }

        if (!$(this).hasClass("no-postback")) {
            loadingPopup.Message("Updating...");
        }

        triggerRadioButtonClick = false;
    });
    function RadioButtonStyle() {
        $(".RadioButton-Toggle-Overlay").remove();
        $('.switch').each(function () {
            var $thisSwitch = $(this);

            if ($thisSwitch.find(".RadioButton-Toggle-Overlay").length == 0) {
                var $cbEnabled = $(this).find(".cb-enable");
                var $cbDisabled = $(this).find(".cb-disable");

                if ($thisSwitch.find(".switch-slider").length == 0) {

                    // Append the slider switch
                    $thisSwitch.append("<div class='switch-slider'><table class='switch-slider-table' cellpadding='0' cellspacing='0'><tbody><tr><td class='td-enable'></td><td class='td-switch' title='Click or Drag'></td><td class='td-disable'></td></tr></tbody></table></div>");

                    $thisSwitch.find(".td-enable").append($cbEnabled);
                    $thisSwitch.find(".td-switch").html("<span class='RadioButton-Toggle-Overlay'></span>");
                    $thisSwitch.find(".td-disable").append($cbDisabled);

                    $cbEnabled.removeClass("RandomActionBtns");
                    $cbDisabled.removeClass("RandomActionBtns");
                }
                else {
                    $thisSwitch.find(".td-switch").html("<span class='RadioButton-Toggle-Overlay'></span>");
                }

                // Set the width of the switch
                var ctrlWidth = $thisSwitch.find(".cb-enable").outerWidth();
                if (ctrlWidth == 0) {
                    ctrlWidth = $thisSwitch.find(".cb-enable").find("label").outerWidth()
                }
                var switchWidth = ctrlWidth + $thisSwitch.find(".RadioButton-Toggle-Overlay").outerWidth();
                $thisSwitch.width(switchWidth - 2);

                var disabledPos = ($thisSwitch.outerWidth() - $thisSwitch.find(".RadioButton-Toggle-Overlay").outerWidth()) + 2;

                var $inputval_cbEnabled = $cbEnabled.find("input");
                var $inputval_cbDisabled = $cbDisabled.find("input");

                if ((RadioButtonHasCheckedAttr($inputval_cbEnabled) && $inputval_cbEnabled.prop("checked")) || ($cbEnabled.hasClass("selected"))) {
                    $cbEnabled.addClass("selected");
                    $cbDisabled.removeClass("selected");
                    $inputval_cbEnabled.prop("checked", true);
                    $inputval_cbDisabled.prop("checked", false);
                    $thisSwitch.find(".switch-slider").css("left", 0);
                }
                else {
                    $cbDisabled.addClass("selected");
                    $cbEnabled.removeClass("selected");
                    $inputval_cbDisabled.prop("checked", true);
                    $inputval_cbEnabled.prop("checked", false);
                    $thisSwitch.find(".switch-slider").css("left", -disabledPos);
                }

                $thisSwitch.find(".switch-slider").draggable({
                    cancel: ".cb-enable, .cb-disable",
                    axis: "x",
                    drag: function (event, ui) {
                        var parentWt = ($(this).parent().outerWidth() - $(this).find(".RadioButton-Toggle-Overlay").outerWidth()) + 2;
                        if (ui.position.left < -parentWt) {
                            $(this).css("left", -parentWt);
                            return false;
                        }

                        if (ui.position.left >= 0) {
                            $(this).css("left", 0);
                            return false;
                        }
                    },
                    stop: function (event, ui) {
                        var mainWidth = ($(this).parent().outerWidth() - $(this).find(".RadioButton-Toggle-Overlay").outerWidth()) + 2;
                        var parentWt = mainWidth / 2;

                        var isEnabled = false;
                        var cbEnableChecked = true;
                        $thisSwitch.find(".cb-disable").each(function () {
                            var $inputval = $(this).find("input");
                            var checked = $inputval.attr("checked");

                            if ($(this).hasClass("selected")) {
                                cbEnableChecked = false;
                            }
                        });

                        if (ui.position.left > -parentWt) {
                            isEnabled = true;
                            $(this).animate({
                                left: 0
                            }, openWSE_Config.animationSpeed);
                        }
                        else {
                            $(this).animate({
                                left: -(parentWt * 2)
                            }, openWSE_Config.animationSpeed);
                        }

                        if (cbEnableChecked && !isEnabled) {
                            triggerRadioButtonClick = true;
                            if ($thisSwitch.find(".cb-disable").hasClass("no-postback")) {
                                $thisSwitch.find(".cb-enable").find("input").prop("checked", false);
                                $thisSwitch.find(".cb-disable").find("input").prop("checked", true);
                                $thisSwitch.find(".cb-enable").removeClass("selected");
                                $thisSwitch.find(".cb-disable").addClass("selected");
                            }
                            $thisSwitch.find(".cb-disable").find('input').trigger('click');
                        }
                        else if (!cbEnableChecked && isEnabled) {
                            triggerRadioButtonClick = true;
                            if ($thisSwitch.find(".cb-enable").hasClass("no-postback")) {
                                $thisSwitch.find(".cb-enable").find("input").prop("checked", true);
                                $thisSwitch.find(".cb-disable").find("input").prop("checked", false);
                                $thisSwitch.find(".cb-enable").addClass("selected");
                                $thisSwitch.find(".cb-disable").removeClass("selected");
                            }
                            $thisSwitch.find(".cb-enable").find('input').trigger('click');
                        }
                    }
                });
            }
        });
    }
    function RadioButtonHasCheckedAttr(_this) {
        if ($(_this)[0] && $(_this)[0].attributes !== null) {
            var attr = $(_this)[0].attributes.checked;

            // For some browsers, `attr` is undefined; for others,
            // `attr` is false.  Check for both.
            if (typeof attr !== typeof undefined && attr !== false) {
                return true;
            }
        }

        return false;
    }


    /* Rating Style Initalize */
    function RatingStyleInit(div, rating, disabled, appId, useLargeStars) {
        try {
            var _disabled = false;
            if (disabled) {
                _disabled = true;
            }

            var imagePath = openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme;

            $(div).attr("data-average", rating);
            $(div).attr("data-id", "1");

            var sizeType = "big";
            if (!useLargeStars) {
                sizeType = "small";
            }

            eval(function (p, a, c, k, e, r) {
                e = function (c) {
                    return (c < a ? '' : e(parseInt(c / a))) + ((c = c % a) > 35 ? String.fromCharCode(c + 29) : c.toString(36))
                };
                if (!''.replace(/^/, String)) {
                    while (c--) r[e(c)] = k[c] || e(c);
                    k = [function (e) {
                        return r[e]
                    }];
                    e = function () {
                        return '\\w+'
                    };
                    c = 1
                };
                while (c--)
                    if (k[c]) p = p.replace(new RegExp('\\b' + e(c) + '\\b', 'g'), k[c]);
                return p
            }('(8($){$.1F.1t=8(1p){6 1o={1n:\'1g/1f/26.1e\',1d:\'1g/1f/1c.1e\',1b:\'1a/1t.1a\',1y:\'1A\',18:m,17:m,w:y,14:m,13:y,V:5,Z:0,n:20,W:-2g,11:5,1j:1,K:J,T:J,I:J};7(4.V>0)F 4.1C(8(){6 3=$.2d(1o,1p),a=0,h=0,g=0,z=\'\',L=m,M=0,N=3.1j;7($(4).2h(\'o\')||3.17)6 o=y;H 6 o=m;1l();$(4).1k(g);6 f=19($(4).15(\'j-f\')),r=t($(4).15(\'j-1E\')),q=h*3.V,1h=f/3.n*q,2e=$(\'<X>\',{\'k\':\'2m\',9:{c:1h}}).A($(4)),f=$(\'<X>\',{\'k\':\'1D\',9:{c:0,U:-g}}).A($(4)),2p=$(\'<X>\',{\'k\':\'1H\',9:{c:q,1k:g,U:-(g*2),1N:\'1P(\'+z+\') 1S-x\'}}).A($(4));$(4).9({c:q,1Y:\'21\',22:1,24:\'25\'});7(!o)$(4).16().2a({2c:8(e){6 s=G(4);6 i=e.D-s;7(3.w)6 1z=$(\'<p>\',{\'k\':\'B\',l:u(i)+\' <E k="Y">/ \'+3.n+\'</E>\',9:{U:(e.1G+3.11),1i:(e.D+3.W)}}).A(\'1I\').1J()},1K:8(e){$(4).9(\'S\',\'1L\')},1M:8(){$(4).9(\'S\',\'R\');7(L)f.c(M);H f.c(0)},1O:8(e){6 s=G(4);6 i=e.D-s;7(3.18)a=Q.1Q(i/h)*h+h;H a=i;f.c(a);7(3.w)$("p.B").9({1i:(e.D+3.W)}).l(u(a)+\' <E k="Y">/ \'+3.n+\'</E>\')},1R:8(){$("p.B").1m()},1T:8(e){6 C=4;L=y;M=a;N--;7(!3.14||t(N)<=0)$(4).16().9(\'S\',\'R\').1U(\'o\');7(3.w)$("p.B").1V(\'1W\',8(){$(4).1m()});e.1X();6 d=u(a);f.c(a);$(\'.1Z p\').l(\'<b>r : </b>\'+r+\'<1q /><b>d : </b>\'+d+\'<1q /><b>1r :</b> 1s\');$(\'.P p\').l(\'<b>27...</b>\');7(3.I)3.I(C,d);7(3.13){$.28(3.1b,{r:r,d:d,1r:\'1s\'},8(j){7(!j.29){$(\'.P p\').l(j.1u);7(3.K)3.K(C,d)}H{$(\'.P p\').l(j.1u);7(3.T)3.T(C,d)}},\'2b\')}}});8 u(i){6 1v=19((i*1w/q)*t(3.n)/1w);6 O=Q.2f(10,t(3.Z));6 1x=Q.2i(1v*O)/O;F 1x};8 1l(){2j(3.1y){2k\'1c\':h=12;g=10;z=3.1d;2l;R:h=23;g=20;z=3.1n}};8 G(v){7(!v)F 0;F v.2n+G(v.2o)}})}})(1B);', 62, 150, '|||opts|this||var|if|function|css|newWidth|strong|width|rate||average|starHeight|starWidth|relativeX|data|class|html|false|rateMax|jDisabled||widthRatingContainer|idBox|realOffsetLeft|parseInt|getNote|obj|showRateInfo||true|bgPath|appendTo|jRatingInfos|element|pageX|span|return|findRealLeft|else|onClick|null|onSuccess|hasRated|globalWidth|nbOfRates|dec|serverResponse|Math|default|cursor|onError|top|length|rateInfosX|div|maxRate|decimalLength||rateInfosY||sendRequest|canRateAgain|attr|unbind|isDisabled|step|parseFloat|php|phpPath|small|smallStarsPath|png|icons|jquery|widthColor|left|nbRates|height|getStarWidth|remove|bigStarsPath|defaults|op|br|action|rating|jRating|server|noteBrut|100|note|type|tooltip|big|jQuery|each|jRatingAverage|id|fn|pageY|jStar|body|show|mouseover|pointer|mouseout|background|mousemove|url|floor|mouseleave|repeat|click|addClass|fadeOut|fast|preventDefault|overflow|datasSent||hidden|zIndex||position|relative|stars|Loading|post|error|bind|json|mouseenter|extend|quotient|pow|45|hasClass|round|switch|case|break|jRatingColor|offsetLeft|offsetParent|jstar'.split('|'), 0, {}));

            $(div).jRating({
                step: true,
                type: sizeType,
                showRateInfo: false,
                canRateAgain: true,
                nbRates: 100,
                bigStarsPath: imagePath + "/Icons/Star_Rating/star-rating-lrg.png",
                smallStarsPath: imagePath + "/Icons/Star_Rating/star-rating-sml.png",
                isDisabled: _disabled,
                decimalLength: 2,
                length: 4,
                rateMax: 4,
                rateMin: 0,
                phpPath: "",
                sendRequest: true,
                onClick: function (element, rate) {
                    $("#AppRating-element").remove();

                    var modalHtml = "<div id='AppRating-element' class='Modal-element'><div class='Modal-overlay'><div class='Modal-element-align'><div class='Modal-element-modal' style='min-width: 350px;'>";
                    modalHtml += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'></div><span class='Modal-title'></span></div></div>";
                    modalHtml += "<div class='ModalScrollContent'><div class='ModalPadContent'></div></div></div></div></div></div>";
                    $("body").append(modalHtml);

                    var x = "<div class='pad-bottom'>Add a comment to your rating or just press Submit.</div>";
                    x += "<textarea id='app-rating-comment' rows='4' cols='40' style='width: 96%; font-family: arial; font-size: 14px; padding: 5px; border: 1px solid #DDD;'></textarea>";
                    x += "<div class='clear-space'></div>";
                    x += "<input class='float-right no-margin input-buttons' type='button' value='Cancel' onclick='openWSE.ResetRating(\"" + div + "\", \"" + rating + "\", " + disabled + ", \"" + appId + "\", " + useLargeStars + ");$(\"#AppRating-element\").remove();' />";
                    x += "<input class='float-left no-margin input-buttons' type='button' value='Submit' onclick='openWSE.UpdateAppRating(\"" + div + "\", \"" + rate + "\", " + disabled + ", \"" + appId + "\", " + useLargeStars + ");' />";
                    x += "<div class='clear-space-five'></div>";

                    $("#AppRating-element").find(".ModalPadContent").html(x);
                    LoadModalWindow(true, "AppRating-element", "Rating Comment");
                }
            });

            if (!useLargeStars) {
                $(div).find(".jStar").addClass("jStar-Small");
            }
        }
        catch (evt) { }
    }
    function UpdateAppRating(div, rating, disabled, appId, useLargeStars) {
        loadingPopup.Message("Updating rating...");
        openWSE.AjaxCall("WebServices/SaveControls.asmx/UpdateAppRating", '{ "rating": "' + rating + '","appId": "' + appId + '","description": "' + $("#app-rating-comment").val() + '" }', null, function (data) {
            $("#AppRating-element").remove();

            loadingPopup.RemoveMessage();
            openWSE.ResetRating(div, rating, disabled, appId, useLargeStars);
            if ($(".app-main-holder[data-appid='" + appId + "']").find(".app-popup-inner-app").length > 0) {
                loadingPopup.Message("Updating rating...");
                openWSE.AboutApp($(".app-main-holder[data-appid='" + appId + "']").find(".app-popup-inner-app")[0]);
            }
            else if ($("#hf_refreshAppAbout").length > 0) {
                loadingPopup.Message("Updating rating...");
                if ($("#aboutApp-element").css("display") === "block") {
                    $("#hf_refreshAppAbout").val(appId);
                }
                else {
                    $("#hf_refreshAppAbout").val("refresh");
                }
                openWSE.CallDoPostBack("hf_refreshAppAbout", "");
            }
        });
    }
    function ResetRating(div, rating, disabled, appId, useLargeStars) {
        $(div).html("");
        RatingStyleInit(div, rating, disabled, appId, useLargeStars);
    }


    /* App Open */
    function OpenAppNoti(id) {
        CloseTopDropDowns();

        if (openWSE.IsComplexWorkspaceMode()) {
            if ($("#workspace_holder").length > 0) {
                if ($(".app-main-holder[data-appid='" + id + "']").css('display') == 'none') {
                    needpostback = 1;
                    var workspace = Getworkspace();
                    LoadApp($(".app-main-holder[data-appid='" + id + "']"), workspace);
                }
            }
            else {
                BuildOpenAppPopup($(".app-icon[data-appid='" + id + "']"));
            }
        }
        else {
            window.location.href = openWSE.siteRoot() + "Default.aspx?AppPage=" + id;
        }
    }
    function SearchSite() {
        var x = $.trim($(".searchwrapper-tools-search").find("input[type='text']").val());

        if (x != "") {
            loadingPopup.Message("Searching. Please Wait...");
            document.getElementById("hf_SearchSite").value = x;

            CloseTopDropDowns();

            $('.searchwrapper-tools-search').find("input[type='text']").val("");
            openWSE.CallDoPostBack("hf_SearchSite", "");
        }
    }
    function SearchExternalSite(search) {
        CloseTopDropDowns();
        window.open("http://google.com/search?q=" + search);
    }
    function BuildOpenAppPopup(_this) {
        var $this = $(_this);

        var x = "<div id='ConfirmApp-element' class='Modal-element' style='display: none;'>";
        x += "<div class='Modal-overlay'>";
        x += "<div class='Modal-element-align'>";
        x += "<div class='Modal-element-modal' style='width: 400px; min-width: 400px;'>";
        x += "<div class='ModalHeader'><div><span class='Modal-title'></span></div></div>";


        var name = $this.find(".app-icon-font").text();
        if (name == "") {
            name = $this.find(".app-title").eq(0).text();
            if (name == "") {
                name = $this.find("span").text();
            }
        }

        var _appId = $this.attr("data-appid");

        var popOutBtn = "";
        if ($this.attr("popoutloc") != "" && $this.attr("popoutloc") != undefined && $this.attr("popoutloc") != null) {
            var popoutloc = $this.attr("popoutloc");
            popOutBtn = "<div class='clear-space'></div><a href='#' onclick='openWSE.PopOutFrameFromSiteTools(\"" + name + "\", \"" + popoutloc + "\");return false;'>Open in new window</a><div class='clear'></div>";
        }

        var currDB = "1";
        if ($this.attr("currentworkspace") != "" && $this.attr("currentworkspace") != undefined && $this.attr("currentworkspace") != null) {
            currDB = $this.attr("currentworkspace");
        }

        if ($this.find(".app-popup-selector").val() !== "-") {
            currDB = $this.find(".app-popup-selector").val();
        }

        var buttonYes = "<input type='button' class='input-buttons modal-ok-btn' value='Yes' onclick='openWSE.LoadAppFromSiteTools(\"" + _appId + "\", \"" + name + "\", \"workspace_" + currDB + "\");' />";
        var buttonNo = "<input type='button' class='input-buttons modal-cancel-btn' value='No' onclick=\"$('#ConfirmApp-element').remove();return false;\" />";
        var messageStr = "You must be on the workspace to load this app. Would you like to go back to your workspace?";

        if (!openWSE.CheckIfWorkspaceLinkAvailable()) {
            buttonYes = "";
            var buttonNo = "<input type='button' class='input-buttons modal-cancel-btn' value='Close' onclick=\"$('#ConfirmApp-element').remove();return false;\" />";
            messageStr = "You are not authorized to use the workspace. ";
            if (popOutBtn != "") {
                messageStr += "However, you can open this app in a seperate window.";
            }
        }

        x += "<div class='ModalScrollContent'><div class='ModalPadContent'>";
        x += messageStr + popOutBtn + "<div class='clear'></div>";
        x += "</div></div>";
        x += "<div class='ModalButtonHolder'>" + buttonYes + buttonNo + "<div class='clear'></div></div>";
        x += "</div></div></div></div>";

        $("body").append(x);
        openWSE.LoadModalWindow(true, "ConfirmApp-element", "Open " + name);
    }


    /* Modal Loader */
    function LoadModalWindow(open, element, title, container) {
        if (element.indexOf("#") != 0) {
            element = "#" + element;
        }

        var $thisElement = $(element);
        if (open) {
            $thisElement.show();
            var $modalElement = $thisElement.find(".Modal-element-modal");

            var modalDragCancel = ".ModalScrollContent, .ModalPadContent, .ModalExitButton, .ModalButtonHolder";

            if (openWSE_Config.appStyle == "Style_2") {
                $thisElement.addClass("modal-style2");
                $thisElement.find(".ModalExitButton").html("<span></span>");
                $modalElement.append($thisElement.find(".ModalHeader"));
            }
            else if (openWSE_Config.appStyle == "Style_3") {
                $thisElement.addClass("modal-style3");
                $thisElement.find(".ModalHeader-hoverclass").remove();
                $thisElement.find(".move-button-modal").remove();
                $thisElement.find(".Modal-element-modal").prepend("<div class='ModalHeader-hoverclass'></div>");
                $thisElement.find(".ModalHeader > div").prepend("<a class='move-button-modal' title='Move'></a>");
                $thisElement.find(".ModalHeader-hoverclass").unbind("mouseenter");
                $thisElement.find(".ModalHeader").unbind("mouseleave");
                $thisElement.find(".ModalHeader-hoverclass").bind("mouseenter", function () {
                    $(this).parent().find(".ModalHeader").show();
                });
                $thisElement.find(".ModalHeader").bind("mouseleave", function () {
                    $(this).hide();
                });

                modalDragCancel += ", .Modal-title";
            }
            else {
                $thisElement.addClass("modal-style1");
            }

            if (!container) {
                container = "body";
            }

            var tempWidth = parseInt($modalElement.attr("data-setwidth"));

            if ($modalElement.attr("data-setwidth") != null && $modalElement.attr("data-setwidth") != "") {
                var x = Math.round(parseInt($modalElement.attr("data-setwidth")) / 1.7);
                if ($modalElement.attr("data-setmaxheight") != null && $modalElement.attr("data-setmaxheight") != "") {
                    x = parseInt($modalElement.attr("data-setmaxheight"));
                }

                $modalElement.find(".ModalScrollContent").css("max-height", x);
                $modalElement.find(".ModalScrollContent").attr("data-tempmaxheight", x);

                $modalElement.css({
                    width: parseInt($modalElement.attr("data-setwidth")),
                    minWidth: parseInt($modalElement.attr("data-setwidth"))
                });
            }
            else {
                if ($(container).width() > 100) {
                    $modalElement.css("max-width", $(container).width() - 100);
                }

                if ($(container).height() > 100) {
                    $modalElement.find(".ModalScrollContent").css("max-height", $(container).height() - 100);
                    $modalElement.find(".ModalScrollContent").attr("data-tempmaxheight", $(container).height() - 100);
                }
            }

            $thisElement.attr("data-containment", container);

            $modalElement.draggable({
                containment: container,
                cancel: modalDragCancel,
                drag: function (event, ui) {
                    var $this = $(this);
                    $this.css("opacity", "0.6");
                    $this.css("filter", "alpha(opacity=60)");

                    // Apply an overlay over app
                    // This fixes the issues when dragging iframes
                    if ($this.find("iframe").length > 0) {
                        var $_id = $this.find(".ModalScrollContent");
                        $wo = $_id.find(".app-overlay-fix");
                        if ($wo.length == 0) {
                            if ($_id.length == 1) {
                                $_id.append("<div class='app-overlay-fix'></div>");
                            }
                        }
                    }
                },
                stop: function (event, ui) {
                    var $this = $(this);
                    $this.css("opacity", "1.0");
                    $this.css("filter", "alpha(opacity=100)");
                    $wo = $(this).find(".app-overlay-fix");
                    if ($wo.length == 1) {
                        $wo.remove();
                    }
                }
            });

            if (title != "") {
                $thisElement.find(".Modal-title").html(title);
            }

            FinishModalLoad(element);
        }
        else {
            $thisElement.hide();
            $thisElement.css("visibility", "hidden");
            $thisElement.find(".Modal-title").html("");

            $("body").removeClass("modal-fixed-position-body");
            if ($("#site_mainbody").length > 0 && $("#site_mainbody").attr("data-layoutoption") === "Boxed") {
                $(".fixed-container-border-left").find(".Modal-overlay").remove();
                $(".fixed-container-border-right").find(".Modal-overlay").remove();
                $(".fixed-footer-container-left").find(".Modal-overlay").remove();
                $(".fixed-footer-container-right").find(".Modal-overlay").remove();
            }

            setTimeout(function () {
                if ($thisElement.hasClass('outside-main-app-div')) {
                    var _classList = GetElementClassList($thisElement[0]);
                    for (var i = 0; i < _classList.length; i++) {
                        if (_classList[i].indexOf(outsideAppModal) > 0) {
                            var appId = _classList[i].replace(outsideAppModal, "");
                            SetActiveApp($(".app-main-holder[data-appid='" + appId + "']"));
                            break;
                        }
                    }
                }
            }, 25);
        }
    }
    function SaveInnerModalContent(args) {
        try {
            var elem = args.get_postBackElement();
            if (elem != null) {
                if ((elem.id == "hf_UpdateAll") || (elem.id == "MainContent_hf_UpdateAll")) {
                    if (innerModalContent.length == 0) {
                        innerModalContent = new Array();
                        $(".Modal-element").each(function () {
                            var $this = $(this);
                            if (($this.css("display") == "block") && ($this.find("iframe").length == 0)) {
                                var innerModalContentString = escape($.trim($this.find(".ModalScrollContent").html()));
                                if ((innerModalContentString != "") && (innerModalContentString != "undefined") && (innerModalContentString != null)) {
                                    var innerMCArray = new Array();
                                    if ($this.attr("id") != "") {
                                        innerMCArray[0] = $this.attr("id");
                                        innerMCArray[1] = innerModalContentString;

                                        var inputs = new Array();
                                        $this.find(".ModalScrollContent").find("input").each(function () {
                                            if ($(this).attr("type") == "checkbox") {
                                                inputs.push($(this).prop("checked"));
                                            }
                                            else {
                                                inputs.push($(this).val());
                                            }
                                        });
                                        innerMCArray[2] = inputs;

                                        var textAreas = new Array();
                                        $this.find(".ModalScrollContent").find("textarea").each(function () {
                                            textAreas.push($(this).val());
                                        });
                                        innerMCArray[3] = textAreas;
                                        
                                        innerModalContent.push(innerMCArray);
                                    }
                                }
                            }
                        });
                    }
                }
            }
        }
        catch (evt) { }

        if ($(".Modal-element").css("display") != "none") {
            var $innerScroll = $(".Modal-element").find(".ModalScrollContent");
            if ($innerScroll.length > 0) {
                innerScrollPos = $innerScroll.scrollTop();
            }
        }
    }
    function LoadSavedInnerModalContent() {
        if (innerModalContent.length > 0) {
            $(".Modal-element").each(function () {
                var $this = $(this);
                if (($this.css("display") == "block") && ($this.find("iframe").length == 0)) {
                    for (var i = 0; i < innerModalContent.length; i++) {
                        if (innerModalContent[i][0] == $this.attr("id")) {
                            var tempContent = unescape(innerModalContent[i][1]);
                            $this.find(".ModalScrollContent").html(tempContent);

                            $this.find(".ModalScrollContent").find("input").each(function (index) {
                                if ($(this).attr("type") == "checkbox") {
                                    $(this).prop("checked", false);
                                    if (innerModalContent[i][2][index]) {
                                        $(this).prop("checked", true);
                                    }
                                }
                                else {
                                    if (innerModalContent[i][2][index]) {
                                        $(this).val(innerModalContent[i][2][index]);
                                    }
                                }
                            });

                            $this.find(".ModalScrollContent").find("textarea").each(function (index) {
                                if (innerModalContent[i][3][index]) {
                                    $(this).val(innerModalContent[i][3][index]);
                                }
                            });

                            break;
                        }
                    }
                }
            });

            innerModalContent = new Array();
        }

        if ($(".Modal-element").css("display") == "block") {
            var $innerScroll = $(".Modal-element").find(".ModalScrollContent");
            if ($innerScroll.length > 0) {
                $innerScroll.scrollTop(innerScrollPos);
                innerScrollPos = 0;
            }
        }
    }
    function FinishModalLoad(ele) {
        if (ele != null) {
            $("body").addClass("modal-fixed-position-body");
            if ($("#site_mainbody").length > 0 && $("#site_mainbody").attr("data-layoutoption") === "Boxed") {
                $(".fixed-container-border-left").append("<div class='Modal-overlay' style='position: absolute!important;'></div>");
                $(".fixed-container-border-right").append("<div class='Modal-overlay' style='position: absolute!important;'></div>");
                $(".fixed-footer-container-left").append("<div class='Modal-overlay' style='position: absolute!important; top: -1px!important;'></div>");
                $(".fixed-footer-container-right").append("<div class='Modal-overlay' style='position: absolute!important; top: -1px!important;'></div>");
            }

            var $thisElement = $(ele);
            $thisElement.find(".Modal-element-align").css({
                marginTop: -($thisElement.find(".Modal-element-modal").height() / 2),
                marginLeft: -($thisElement.find(".Modal-element-modal").width() / 2)
            });

            if ($thisElement.find(".ModalButtonHolder").length > 0) {
                var $ModalButtonHolder = $thisElement.find(".ModalButtonHolder");
                if ($ModalButtonHolder.find(".modal-button-holder-clear").length > 0) {
                    $ModalButtonHolder.find(".modal-button-holder-clear").remove();
                }
                $ModalButtonHolder.append("<div class=\"clear modal-button-holder-clear\"></div>");
            }

            $thisElement.find(".app-head-button-holder-admin").show();
            if ($.trim($thisElement.find(".app-head-button-holder-admin").html()) === "") {
                $thisElement.find(".app-head-button-holder-admin").hide();
            }

            $thisElement.css("visibility", "visible");
            AdjustModalWindowView();
            CloseTopDropDowns();

            if (openWSE_Config.appStyle == "Style_3") {
                $thisElement.find(".ModalHeader").show();
                $thisElement.find(".ModalHeader-hoverclass").css("width", $thisElement.find(".ModalHeader").outerWidth());
                $thisElement.find(".ModalHeader").hide();
            }
        }
    }
    function AdjustModalWindowView() {
        $(".Modal-element").each(function () {
            var currScrollTop = $(this).find(".ModalScrollContent").scrollTop();
            $(this).removeClass("modal-fixed-position");
            $(this).find(".Modal-element-modal").css("height", "");
            $(this).find(".ModalScrollContent").css("max-height", "");

            if ($(this).css("display") == "block") {
                var windowHt = $(window).height();
                var windowWt = $(window).width();

                var containment = $(this).attr("data-containment");
                if (containment) {
                    windowHt = $(containment).outerHeight();
                    windowWt = $(containment).outerWidth();
                }

                if ($(this).find(".ModalScrollContent").attr("data-tempmaxheight")) {
                    $(this).find(".ModalScrollContent").css("max-height", $(this).find(".ModalScrollContent").attr("data-tempmaxheight") + "px");
                }

                var $modal = $(this).find(".Modal-element-modal");
                if ($modal.length > 0 && $modal.outerWidth() > windowWt || $modal.outerHeight() > windowHt) {
                    $(this).addClass("modal-fixed-position");
                    $(this).find(".ModalScrollContent").css("max-height", "");
                }
                else {
                    if ($(this).find(".ModalScrollContent").attr("data-tempmaxheight")) {
                        $(this).find(".ModalScrollContent").css("max-height", $(this).find(".ModalScrollContent").attr("data-tempmaxheight") + "px");
                    }
                }

                $(this).find(".ModalScrollContent").scrollTop(currScrollTop);
            }
        });
    }
    function getCurrentPostitionForModalWindow() {
        for (var i = 0; i < $(".Modal-element").length; i++) {
            var $thisElement = $(".Modal-element").eq(i);
            if ($thisElement.css("display") == "block") {
                var $modalElement = $thisElement.find(".Modal-element-modal");
                if ($modalElement.attr("data-setwidth") != null && $modalElement.attr("data-setwidth") != "") {
                    var x = Math.round(parseInt($modalElement.attr("data-setwidth")) / 1.7);
                    if ($modalElement.attr("data-setmaxheight") != null && $modalElement.attr("data-setmaxheight") != "") {
                        x = parseInt($modalElement.attr("data-setmaxheight"));
                    }

                    $modalElement.find(".ModalScrollContent").css("max-height", x);
                    $modalElement.find(".ModalScrollContent").attr("data-tempmaxheight", x);

                    $modalElement.css({
                        width: parseInt($modalElement.attr("data-setwidth")),
                        minWidth: parseInt($modalElement.attr("data-setwidth"))
                    });
                }

                if ($thisElement.find(".ModalButtonHolder").length > 0) {
                    var $ModalButtonHolder = $thisElement.find(".ModalButtonHolder");
                    if ($ModalButtonHolder.find(".modal-button-holder-clear").length > 0) {
                        $ModalButtonHolder.find(".modal-button-holder-clear").remove();
                    }
                    $ModalButtonHolder.append("<div class=\"clear modal-button-holder-clear\"></div>");
                }
            }
        }
    }


    /* Dropdown Menus */
    function CloseTopDropDowns() {
        $(".top-bar-menu-button, .top-bar-userinfo-button").removeClass("active");
        $(".top-bar-menu-button li.b, .top-bar-userinfo-button li.b").hide();
        if (window.location.href.toLowerCase().indexOf("appremote.aspx") === -1) {
            CloseNoti();
            $("#background-selector-holder").html("");
            CloseGroupLoginModal();
        }

        if (isMobileDevice() || window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1) {
            $(".sidebar-overlay").remove();
        }
    }
    function ResizeTopDropDowns() {
        var $a = null;
        var $b = null;
        var eleId = "";

        var $topBar = $("#top_bar");
        if ($topBar.length == 0) {
            $topBar = $("#top_bar_toolview_holder");
        }

        for (var i = 0; i < $topBar.find("li.b").length; i++) {
            var $this = $topBar.find("li.b").eq(i);
            if ($this.is(":visible")) {
                $a = $this.parent().find(".a");
                $b = $this;
                eleId = $this.parent().parent().attr("id");
                break;
            }
        }

        if ($a !== null && $a.length > 0 && $b !== null && $b.length > 0) {
            var offsetVar = 10;

            $b.css("right", "");
            $b.css("left", "");
            $b.find(".li-pnl-tab").css("max-width", $(window).outerWidth() - (offsetVar * 2));

            var mainContainerWidth = $("#main_container").outerWidth();
            if (getParameterByName("mobileMode") === "true") {
                mainContainerWidth -= $(".iframe-title-logo").outerWidth();
            }

            try {
                if ($a.offset().left > ($(window).outerWidth() / 2)) {
                    var offsetRight = ($(window).outerWidth() - $a.offset().left) - $a.outerWidth();
                    $b.css("right", offsetRight + offsetVar);

                    var bOuterWidth = $b.outerWidth();
                    if (bOuterWidth + offsetRight >= mainContainerWidth) {
                        $b.css("right", $(".fixed-container-holder").offset().left + offsetVar);
                        if ($("#top-logo-holder").length > 0 && $("#top-logo-holder").width() !== $(window).width()) {
                            $b.find(".li-pnl-tab").css("max-width", (mainContainerWidth - 2) - (offsetVar * 3));
                        }
                    }
                }
                else {
                    var offsetLeft = $a.offset().left;
                    $b.css("left", offsetLeft + offsetVar);

                    var bOuterWidth = $b.outerWidth();
                    if (bOuterWidth + (offsetLeft - $("#main_container").offset().left) >= mainContainerWidth) {
                        if ($("#top-logo-holder").width() !== $(window).width()) {
                            $b.css("left", $("#main_container").offset().left + offsetVar);
                            $b.find(".li-pnl-tab").css("max-width", (mainContainerWidth - 2) - (offsetVar * 2));
                        }
                        else {
                            $b.css("left", $(".fixed-container-holder").offset().left + offsetVar);
                        }
                    }
                    else if ((mainContainerWidth - 2) - (offsetVar * 2) > ($(window).outerWidth() - (offsetVar * 2))) {
                        $b.find(".li-pnl-tab").css("max-width", (mainContainerWidth - 2) - (offsetVar * 2));
                    }
                }
            }
            catch (evt) { }

            setTimeout(function () {
                var maxHeight = $(window).height() - ($b.offset().top + $("#footer_container").outerHeight() + $b.find(".li-header").outerHeight() + $b.find(".li-footer").outerHeight() + 1);
                if (isNaN(maxHeight)) {
                    maxHeight = $(window).height() - ($b.offset().top + $("#footer_container").outerHeight() + $b.find(".li-header").outerHeight() + 1);
                }

                $b.find(".li-pnl-tab").css("max-height", maxHeight - (offsetVar * 2));
            }, 1);
        }
    }


    /* Help Dialog */
    var emailFocus = false;
    var focusPassword = false;
    var newUserHelp = false;
    var needEmailChange = false;
    var adminPasswordChange = false;
    var showIntroPage = true;
    var introPageNumber = 0;
    function HelpOverlay(NewUser) {
        if ($("#help_main_holder").css("display") == "none") {

            if ($("#iframe-container-helper").length > 0) {
                CloseIFrameContent();
            }

            newUserHelp = NewUser;
            AddUrlHashQueryPath("help", newUserHelp.toString());
        }
        else {
            CloseHelpOverlay();
        }
    }
    function HelpOverlayHistory() {
        if (!newUserHelp && CheckIfOnWorkspace()) {
            HelpMenuPageLoadWorkspace();
            $("#help_main_holder").fadeIn(openWSE_Config.animationSpeed);
        }
        else {
            openWSE.AjaxCall(saveHandler + "/GetTotalHelpPages", '{ "currentPage": "' + document.location.href + '" }', null, function (data) {
                var count = parseInt(data.d);
                if (count > 0) {
                    totalHelpPages = count;
                    HelpMenuPageLoad(0, newUserHelp);
                    $("#help_main_holder").fadeIn(openWSE_Config.animationSpeed);
                }
                else {
                    if (!newUserHelp) {
                        openWSE.AlertWindow("There are no help pages available for this webpage.");
                        CloseHelpOverlay();
                    }
                    else {
                        NewUserPageLoad();
                        $("#help_main_holder").fadeIn(openWSE_Config.animationSpeed);
                    }
                }
            }, function (data) {
                if (!newUserHelp) {
                    openWSE.AlertWindow("There was an error retrieving the help pages. Please try again.");
                    CloseHelpOverlay();
                }
                else {
                    NewUserPageLoad();
                    $("#help_main_holder").fadeIn(openWSE_Config.animationSpeed);
                }
            });
        }
    }
    function HelpMenuPageLoad(pagenum, NewUser) {
        var titleHeader = "<span class='pad-left'>Welecome to " + openWSE_Config.siteName + "</span>";
        if (!NewUser) {
            titleHeader = "<span class='pad-left'>" + openWSE_Config.siteName + " Help Pages</span>";
        }

        var btns = "";
        if (!NewUser) {
            btns += "<a onclick='openWSE.HelpOverlay(false, " + adminPasswordChange + ")' class='close-help float-right' title='Close'></a>";
        }
        if ((pagenum + 1) < totalHelpPages) {
            btns += "<input type='button' class='input-buttons-create float-right margin-left' onclick='openWSE.HelpMenuPageLoad(" + (pagenum + 1).toString() + "," + NewUser + ")' value='Next' />";
        }
        else if (((pagenum + 1) == totalHelpPages) && (NewUser)) {
            btns += "<input type='button' class='input-buttons-create float-right margin-left' onclick='openWSE.NewUserPageLoad()' value='Next' />";
        }

        if (pagenum > 0) {
            btns += "<input type='button' class='input-buttons-create float-right margin-left' onclick='openWSE.HelpMenuPageLoad(" + (pagenum - 1).toString() + "," + NewUser + ")' value='Back' />";
        }

        $("#helpmenu_title").html("<div class='help-Title-Top'>" + titleHeader + btns + "</div>");
        pagenum += 1;

        var currentPage = document.location.href;
        currentPage = currentPage.substring(currentPage.lastIndexOf("/") + 1);
        currentPage = currentPage.replace(currentPage.substring(currentPage.indexOf(".")), "");
        loadingPopup.Message("Loading...");
        $("#helpdiv_pageholder").load(openWSE.siteRoot() + "HelpPages/" + currentPage + "/HelpPage" + pagenum + ".html", function () {
            loadingPopup.RemoveMessage();
            $(".help-images-workspace-img").each(function () {
                if (!$(this).hasClass("ignoreSrc")) {
                    var imgSrc = $(this).attr("src");
                    $(this).attr("src", openWSE.siteRoot() + "HelpPages/" + currentPage + "/images/" + imgSrc);
                }
            });
        });
    }
    function HelpMenuPageLoadWorkspace() {
        var titleHeader = "<span class='pad-left'>" + openWSE_Config.siteName + " Help Pages</span>";
        var btns = "<a onclick='openWSE.HelpOverlay(false, " + adminPasswordChange + ")' class='close-help float-right' title='Close'></a>";
        $("#helpmenu_title").html("<div class='help-Title-Top'>" + titleHeader + btns + "</div>");
        $("#helpdiv_pageholder").html(BuildHelpIntro());
    }
    function NewUserPageLoad() {
        var titleHeader = "<span class='pad-left'>Welecome to " + openWSE_Config.siteName + "</span>";
        var btns = "";
        btns += "<input id='btnFinish' type='button' class='input-buttons-create float-right display-none' onclick='openWSE.NewUserfinsh()' value='Finish' />";
        if (totalHelpPages > 0) {
            btns += "<input type='button' class='input-buttons-create float-right margin-left' onclick='openWSE.HelpMenuPageLoad(" + (totalHelpPages - 1).toString() + ",true)' value='Back' />";
        }

        $("#helpmenu_title").html("<div class='help-Title-Top'>" + titleHeader + btns + "</div>");
        $("#helpdiv_pageholder").html("<div style='position: absolute; top: 50%; left: 50%; margin-left: -95px; margin-top: -45px; padding: 20px;'><div class='loading-icon-lrg margin-top-sml'></div><h3 style='color: #FFF; float: left; font-weight: bold;'>Loading Step....</h3></div>");
        loadingPopup.Message("Loading...");
        openWSE.AjaxCall('WebServices/AcctSettings.asmx/CheckForEmailAddress', '', null, function (data) {
            needEmailChange = openWSE.ConvertBitToBoolean(data.d[0]);
            adminPasswordChange = openWSE.ConvertBitToBoolean(data.d[1]);
            var newmember_text = "";

            if (!needEmailChange && !adminPasswordChange) {
                newmember_text = UserSetupContainer();
            }
            else {
                newmember_text = NewUserEmailContainer();
            }

            $("#helpdiv_pageholder").html(newmember_text);
            setTimeout(function () {
                if (emailFocus) {
                    $("#tb_emailaddress_update").focus();
                }
                else if (focusPassword) {
                    $("#tb_newpassword_update").focus();
                }
            }, 500);
            loadingPopup.RemoveMessage();
        });
    }
    function UpdateEmail() {
        var tb = $.trim($("#tb_emailaddress_update").val());
        if (tb != "") {
            loadingPopup.Message("Updating...");
            openWSE.AjaxCall('WebServices/AcctSettings.asmx/AddEmailAddress', '{"email": "' + escape(tb) + '"}', null, function (data) {
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    needEmailChange = false;
                    $("#update-email-helpdiv").remove();
                }

                if ($("#update-password-helpdiv").length == 0) {
                    NewUserPageLoad();
                }

                loadingPopup.RemoveMessage();
            });
        }
    }
    function UpdateAdminPassword() {
        var tb_pw1 = $.trim($("#tb_newpassword_update").val());
        var tb_pw2 = $.trim($("#tb_confirmnewpassword_update").val());

        $("#span_passwordmismatch").html("");

        if (tb_pw1 == "") {
            $("#span_newpassword_update").show();
        }
        else {
            $("#span_newpassword_update").hide();
        }

        if (tb_pw2 == "") {
            $("#span_confirmnewpassword_update").show();
        }
        else {
            $("#span_confirmnewpassword_update").hide();
        }

        if (tb_pw1 == tb_pw2 && tb_pw1 != "" && tb_pw2 != "") {
            if (tb_pw1.length < openWSE_Config.minPasswordLength) {
                $("#span_passwordmismatch").html("Password does not meet requirements.");
            }
            else {
                loadingPopup.Message("Updating...");
                openWSE.AjaxCall('WebServices/AcctSettings.asmx/UpdateAdminPassword', '{"password1": "' + escape(tb_pw1) + '"}', null, function (data) {
                    if (openWSE.ConvertBitToBoolean(data.d)) {
                        adminPasswordChange = false;
                        $("#update-password-helpdiv").remove();
                    }

                    if ($("#update-email-helpdiv").length == 0) {
                        NewUserPageLoad();
                    }

                    loadingPopup.RemoveMessage();
                });
            }
        }
        else {
            if (tb_pw1 != "" && tb_pw2 != "") {
                $("#span_passwordmismatch").html("Passwords do not match.");
            }
        }
    }
    function NewUserEmailContainer() {
        var newmember_text = "";

        if (needEmailChange) {
            emailFocus = true;
            focusPassword = false;
            newmember_text += "<div id='update-email-helpdiv' class='pad-all' align='center'>";
            newmember_text += "<h3>Before you can continue, you must provide a valid email address.<br />This email address will be used for notifications and password recovery.<br />If you fail to provide a valid email address, you will not be able to use the password recovery feature on the login page.</h3>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<h3>Please provide a valid email address</h3>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<span class='pad-right font-bold'>Email Address</span><input type='text' id='tb_emailaddress_update' onkeypress='openWSE.OnEmailUpdate_KeyPress(event)' class='textEntry margin-right' style='width: 200px;' />";
            newmember_text += "<input type='button' class='input-buttons' onclick='openWSE.UpdateEmail()' value='Update' />";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "</div>";
        }
        else if (adminPasswordChange) {
            emailFocus = false;
            focusPassword = true;
            newmember_text += "<div id='update-password-helpdiv' class='pad-all' align='center'>";
            newmember_text += "<h3>You are currently signed in as Administrator. Create a new password for your Administrator account.</h3>";
            newmember_text += "<div class='clear-space-two'></div><small><b class='pad-right-sml'>Note:</b>Password must be at least " + openWSE_Config.minPasswordLength + " characters long.</small>";
            newmember_text += "<div class='clear-space'></div><table border='0' cellpadding='0' cellspacing='0'>";
            newmember_text += "<tr><td align='right'><span class='pad-right font-bold'>New Password:</span></td>";
            newmember_text += "<td align='left'><input type='password' id='tb_newpassword_update' onkeypress='openWSE.OnPasswordUpdate_KeyPress(event)' class='textEntry margin-right' style='width: 200px;' /><span id='span_newpassword_update' style='display: none; color: Red;'>*</span></td></tr>";
            newmember_text += "<tr><td></td><td><div class='clear-space'></div></td></tr>";
            newmember_text += "<tr><td align='right'><span class='pad-right font-bold'>Confirm New Password:</span></td>";
            newmember_text += "<td align='left'><input type='password' id='tb_confirmnewpassword_update' onkeypress='openWSE.OnPasswordUpdate_KeyPress(event)' class='textEntry margin-right' style='width: 200px;' /><span id='span_confirmnewpassword_update' style='display: none; color: Red;'>*</span></td></tr></table>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<input type='button' class='input-buttons no-margin' onclick='openWSE.UpdateAdminPassword()' value='Change Password' />";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<span id='span_passwordmismatch' style='color: Red;'></div>";
            newmember_text += "</div>";
        }

        return newmember_text;
    }
    function OnEmailUpdate_KeyPress(e) {
        if (e.which == 13 || e.keyCode == 13) {
            openWSE.UpdateEmail();
            e.preventDefault();
        }
    }
    function OnPasswordUpdate_KeyPress(e) {
        if (e.which == 13 || e.keyCode == 13) {
            openWSE.UpdateAdminPassword();
            e.preventDefault();
        }
    }
    function UserSetupContainer() {
        var newmember_text = "";

        if (showIntroPage) {
            newmember_text = BuildHelpIntro();
        }
        else {
            newmember_text = "<div class='pad-all' align='center'>";
            newmember_text += "<h3>You are now setup. Please click finish to load your workspace. You can change your email address within your 'Account Settings' page. If you need help in the future, click on the help icon at the bottom left hand corner of the workspace.</h3>";
            newmember_text += "<h3>By using this site you agree that OpenWSE can save cookies to your device. These cookies contain no information regarding your personal information.</h3>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "<a href='#' onclick='openWSE.HelpIntroRestart();return false;'>Go back to the Intro Pages</a>";
            newmember_text += "<div class='clear-space'></div>";
            newmember_text += "</div>";
            $("#btnFinish").removeClass("display-none");
        }

        return newmember_text;
    }
    function NewUserfinsh() {
        openWSE.AjaxCall('WebServices/AcctSettings.asmx/UpdateAcctNewMember', '', null, function (data) {
            var response = data.d;
            if (openWSE.ConvertBitToBoolean(response)) {
                if (getParameterByName("mobileMode") === "true") {
                    window.location = openWSE.siteRoot() + "AppRemote.aspx";
                }
                else {
                    window.location = openWSE.siteRoot() + "Default.aspx";
                }
            }
        });
    }
    function CloseHelpOverlay() {
        newUserHelp = false;
        adminPasswordChange = false;
        if ($("#help_main_holder").css("display") == "block") {
            $("#help_main_holder").fadeOut(openWSE_Config.animationSpeed, function () {
                $("#helpmenu_title, #helpdiv_1, #helpdiv_2").html("");
            });
        }

        RemoveUrlHashQueryPath("help");

        try {
            var url = window.location.href;
            if (url.indexOf("?help") != -1) {
                var loc = url.split("?help");
                if (loc.length > 1) {
                    var fullLoc = "?help" + loc[1];
                    url = url.replace(fullLoc, "");
                    window.location = url;
                }
                else {
                    url = url.replace("?help", "");
                }

                window.location = url;
            }
            else if (url.indexOf("&help") != -1) {
                var loc = url.split("&help");
                if (loc.length > 1) {
                    var fullLoc = "&help" + loc[1];
                    url = url.replace(fullLoc, "");

                }
                else {
                    url = url.replace("&help", "");
                }

                window.location = url;
            }
        }
        catch (evt) { }
    }
    function BuildHelpIntro() {
        var newmember_text = "";

        if (newUserHelp) {
            newmember_text += "<input type='button' class='input-buttons float-right' value='Next' onclick='openWSE.HelpIntroNext();' style='margin-top:-20px; margin-right: 7%;' />";
            if (introPageNumber > 0) {
                newmember_text += "<input type='button' class='input-buttons float-left' value='Back' onclick='openWSE.HelpIntroBack();' style='margin-top:-20px; margin-left: 7%;' />";
            }
            else {
                newmember_text += "<input type='button' class='input-buttons float-left' value='Back' onclick='openWSE.HelpIntroBack();' style='margin-top:-20px; margin-left: 7%; visibility: hidden;' />";
            }
        }
        else {
            if (introPageNumber > 3) {
                newmember_text += "<input type='button' class='input-buttons float-right' value='Next' onclick='openWSE.HelpIntroNext();' style='margin-top:-20px; margin-right: 7%; visibility: hidden;' />";
            }
            else {
                newmember_text += "<input type='button' class='input-buttons float-right' value='Next' onclick='openWSE.HelpIntroNext();' style='margin-top:-20px; margin-right: 7%;' />";
            }
            if (introPageNumber > 0) {
                newmember_text += "<input type='button' class='input-buttons float-left' value='Back' onclick='openWSE.HelpIntroBack();' style='margin-top:-20px; margin-left: 7%;' />";
            }
            else {
                newmember_text += "<input type='button' class='input-buttons float-left' value='Back' onclick='openWSE.HelpIntroBack();' style='margin-top:-20px; margin-left: 7%; visibility: hidden;' />";
            }
        }

        switch (introPageNumber) {
            case 0: // Workspace
                newmember_text += "<span class='intro-title'>Workspace App List</span>";
                newmember_text += "<div class='clear-space'></div>";
                newmember_text += "<div class='workspace-intro'>";
                newmember_text += "<img alt='workspace' src='" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/workspace.jpg' />";
                newmember_text += "<div class='help-intro-overlay'>";
                newmember_text += "<div class='help-intro-circle' style='top: 7%; left: 0px; height: 22%; width: 10%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-selector.png\");'></div>";
                newmember_text += "<div class='help-intro-text' style='top: 10%; left: 15%;'><ul>";
                newmember_text += "<li>All your available apps will open in the workspace portion of the page (where your custom background appears).</li>";
                newmember_text += "<li>You can load as many apps as you want and move them anywhere you like. The site will record the position and save it to the database where it can retrieve it on page loads.</li>";
                newmember_text += "<li>If you are logged into a group, you only see the apps availble to that group. Once you log out of that group, all your apps will appear again.</li>";
                newmember_text += "<li>If you have more than 1 total workspaces, When hovering over an app, you will see a box to the right that you can select. This will bring up a dropdown that will allow you to pick which workspace to load that app on.</li>";
                newmember_text += "</ul></div>";
                newmember_text += "</div>";
                newmember_text += "</div>";
                break;

            case 1: // Workspace Buttons
                newmember_text += "<span class='intro-title'>Workspace Controls</span>";
                newmember_text += "<div class='clear-space'></div>";
                newmember_text += "<div class='workspace-intro'>";
                newmember_text += "<img alt='workspace' src='" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/workspace.jpg' />";
                newmember_text += "<div class='help-intro-overlay'>";
                newmember_text += "<div class='help-intro-circle' style='top: 0%; left: 24%; height: 51%; width: 37.5%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/top-right-btns.png\");'></div>";
                newmember_text += "<div class='help-intro-text' style='top: 5%; right: 0; width: 23%;'>You can select the workspace you want to work on. Apps can be placed in each workspace anyway you want.</div>";
                newmember_text += "<div class='help-intro-text' style='top: 5%; right: 30%; width: 15%;'>The top buttons to the left will show you different features such as:<br /><ul class='pad-left-big'><li>Group Login</li><li>Login/Logout</li><li>Overlay View</li><li>Notifications</li><li>Search</li></ul></div>";
                newmember_text += "</div>";
                newmember_text += "</div>";
                break;

            case 2: // Workspace App
                newmember_text += "<span class='intro-title'>Workspace Opened App</span>";
                newmember_text += "<div class='clear-space'></div>";
                newmember_text += "<div class='workspace-intro'>";
                newmember_text += "<img alt='workspace' src='" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/workspace.jpg' />";
                newmember_text += "<div class='help-intro-overlay'>";
                newmember_text += "<div class='help-intro-circle' style='top: 7%; left: 0px; height: 22%; width: 10%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-selector-with-openapp.png\");'></div>";
                newmember_text += "<div class='help-intro-circle' style='top: 7%; left: 15%; height: 50%; width: 15.5%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app.png\");'></div>";
                newmember_text += "<div class='help-intro-text' style='bottom: 5%; left: 15%;'><ul>";
                newmember_text += "<li>All apps are configurable to your liking. Each app will also come with option buttons, which can be found at the top right of the app header (If App Style is Style 2)</li>";
                newmember_text += "<li>Moving, resizing, minimizing, maximizing, and closing will be saved to your profile. This way each time you login, these settings will be applied to make it look and feel like you never left.</li>";
                newmember_text += "</ul></div>";
                newmember_text += "</div>";
                newmember_text += "</div>";
                break;

            case 3: // App Options
                newmember_text += "<span class='intro-title'>App Options</span>";
                newmember_text += "<div class='clear-space'></div>";
                newmember_text += "<div class='workspace-intro'>";
                newmember_text += "<img alt='workspace' src='" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/workspace.jpg' />";
                newmember_text += "<div class='help-intro-overlay'>";
                newmember_text += "<div class='help-intro-no-circle' style='top: 7%; left: 0px; height: 22%; width: 10%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-selector-with-openapp.png\");'><div class='help-intro-overlay'></div></div>";
                newmember_text += "<div class='help-intro-no-circle' style='top: 7%; left: 15%; height: 50%; width: 15.5%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app.png\");'><div class='help-intro-overlay'></div></div>";
                newmember_text += "<div class='help-intro-circle' style='top: 7%; left: 24%; height: 20%; width: 6%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-options.png\");'></div>";
                newmember_text += "<div class='help-intro-text' style='bottom: 5%; left: 15%;'><ul>";
                newmember_text += "<li>Each app will have the options menu at the top right of app header bar.</li>";
                newmember_text += "<li>You will have up to 4 options to choose from: Refresh, Pop app out, About, and Move to Workspace Number.</li>";
                newmember_text += "</ul></div>";
                newmember_text += "</div>";
                newmember_text += "</div>";
                break;

            case 4: // Minimized App
                newmember_text += "<span class='intro-title'>Minimized App</span>";
                newmember_text += "<div class='clear-space'></div>";
                newmember_text += "<div class='workspace-intro'>";
                newmember_text += "<img alt='workspace' src='" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/workspace.jpg' />";
                newmember_text += "<div class='help-intro-overlay'>";
                newmember_text += "<div class='help-intro-no-circle' style='top: 7%; left: 0; height: 22%; width: 10%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-selector-with-openapp.png\");'><div class='help-intro-overlay'></div></div>";
                newmember_text += "<div class='help-intro-circle' style='top: 9%; left: 0; height: 4%; width: 2%; background-image: url(\"" + GetSiteRoot() + "Standard_Images/NewUserIntro/Workspace/app-minimized.png\");'></div>";
                newmember_text += "<div class='help-intro-text' style='top: 10%; left: 15%;'><ul>";
                newmember_text += "<li>When apps are minimized, they will be moved completely off the screen.</li>";
                newmember_text += "<li>If you hover over the minimized app, a preview will be shown with the exact place the app was before being minimized. (Only if option is on in settings)</li>";
                newmember_text += "<li>You can close out of the app by clicking on the 'x' to the right of the minimized button.</li>";
                newmember_text += "<li>To restore the app, simply click on the minimized button at the top. You can also click on the app icon under your Available Apps.</li>";
                newmember_text += "</ul></div>";
                newmember_text += "</div>";
                newmember_text += "</div>";
                break;

            default:
                if (newUserHelp) {
                    showIntroPage = false;
                    NewUserPageLoad();
                }
                break;
        }

        return newmember_text;
    }
    function HelpIntroRestart() {
        introPageNumber = 0;
        showIntroPage = true;
        NewUserPageLoad();
    }
    function HelpIntroNext() {
        introPageNumber++;
        if (newUserHelp) {
            NewUserPageLoad();
        }
        else {
            $("#helpdiv_pageholder").html(BuildHelpIntro());
        }
    }
    function HelpIntroBack() {
        introPageNumber--;
        if (introPageNumber < 0) {
            introPageNumber = 0;
        }
        if (newUserHelp) {
            NewUserPageLoad();
        }
        else {
            $("#helpdiv_pageholder").html(BuildHelpIntro());
        }
    }


    /* Message Popup */
    var showUpdatesModalTriggered = false;
    function ShowUpdatesPopup(message) {
        setTimeout(function () {
            if (!showUpdatesModalTriggered) {
                var decodedMessage = unescape(message);
                var x = "<div id='OpenWSEUpdates-element' class='Modal-element' style='display: none;'>";
                x += "<div class='Modal-overlay'>";
                x += "<div class='Modal-element-align'>";
                x += "<div class='Modal-element-modal' data-setwidth='550'>";
                x += "<div class='ModalHeader'><div><span class='Modal-title'></span></div></div>";
                x += "<div class='ModalScrollContent'><div class='ModalPadContent'>" + decodedMessage + "</div></div>";
                x += "</div></div></div></div></div>";

                $("body").append(x);
                openWSE.LoadModalWindow(true, "OpenWSEUpdates-element", "Site Updates");
                showUpdatesModalTriggered = true;
            }
        }, 100);
    }
    var showActivationModalTriggered = false;
    function ShowActivationPopup(message) {
        setTimeout(function () {
            if (!showActivationModalTriggered) {
                var decodedMessage = unescape(message);
                var x = "<div id='Activation-element' class='Modal-element' style='display: none;'>";
                x += "<div class='Modal-overlay'>";
                x += "<div class='Modal-element-align'>";
                x += "<div class='Modal-element-modal' data-setwidth='500'>";
                x += "<div class='ModalHeader'><div><span class='Modal-title'></span></div></div>";

                var img = "<img alt='' class='float-left pad-right' src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/approve.png' style='height: 17px;' />";
                var text = "<span style='font-size: 15px'>Got it</span>";
                var button = "<a href='" + openWSE.siteRoot() + "Default.aspx' class='input-buttons no-margin' style='text-decoration: none!important;'>" + img + " " + text + "</a>";

                x += "<div class='ModalScrollContent'><div class='ModalPadContent'>" + decodedMessage + "<div class='clear-space'></div><div align='center'>" + button + "</div>";
                x += "</div></div></div></div></div></div></div>";

                $("body").append(x);

                openWSE.LoadModalWindow(true, "Activation-element", "Activation");
                showActivationModalTriggered = true;
            }
        }, 100);
    }
    function CloseUpdatesPopup() {
        LoadModalWindow(false, "OpenWSEUpdates-element", "");
        $("#OpenWSEUpdates-element").remove();
    }


    /* Overlay Menu Controls */
    $(document).keydown(function (e) {
        e = e || window.event;

        // ALT + Up Arrow || ALT + Down Arrow
        if ($("#workspace_holder").length > 0) {
            if ((e.altKey && e.which == 38) || (e.altKey && e.which == 40)) {
                var current = parseInt(Getworkspace().replace("workspace_", ""));
                var totalWorkspaces = $(".dropdown-db-selector").find("ul").find("li").length;

                var $workspaceSelect = null;

                if (e.which == 40) {
                    if (current + 1 <= totalWorkspaces) {
                        $workspaceSelect = $(".workspace-selection-item").eq(current);
                    }
                    else {
                        $workspaceSelect = $(".workspace-selection-item").eq(0);
                    }
                }
                else {
                    if (current - 1 >= 1) {
                        $workspaceSelect = $(".workspace-selection-item").eq(current - 2);
                    }
                    else {
                        $workspaceSelect = $(".workspace-selection-item").eq(totalWorkspaces - 1);
                    }
                }

                if ($workspaceSelect != null) {
                    $workspaceSelect.trigger("click");
                    var oldid = "#MainContent_" + openWSE.Getworkspace();
                    var newid = "#MainContent_workspace_" + $workspaceSelect.attr("data-number");
                    openWSE.HoverWorkspacePreview(oldid, newid);
                }
            }
        }
    });
    function OverlayDisable(_this) {
        loadingPopup.Message("Closing Overlay...");
        var $main = $(_this).closest(".workspace-overlay-selector");
        if ($main.length == 0) {
            $main = $("." + _this);
        }

        $main.remove();

        openWSE.AjaxCall(saveHandler + "/Overlay_Disable", '{ "classes": "' + $main.attr("class") + '" }', null, function (data) {
            TryRemoveLoadOverlay(data.d);
            loadingPopup.RemoveMessage();
        });

        CheckIfOverlaysExistsOnNonComplex();
    }
    function TryAddLoadOverlay(ids) {
        var splitIds = ids.split(',');
        var dn = "#" + openWSE_Config.overlayPanelId;
        for (var i = 0; i < splitIds.length; i++) {
            if (splitIds[i] != "") {
                if ($(dn).find('.' + splitIds[i]).length > 0) {
                    $(dn).find('.' + splitIds[i]).remove();
                }

                $(dn).hide().prepend($('.move-holder').find('.' + splitIds[i])).show();
            }
        }

        if (ids != "") {
            UpdateOverlayTable();
        }
    }
    function TryRemoveLoadOverlay(id) {
        var $overlay = $("." + id);
        if ($overlay.length > 0) {
            $overlay.remove();
        }

        RemoveCSSFilesOnAppClose(id);

        UpdateOverlayTable();
        CheckIfOverlaysExistsOnNonComplex();
    }
    function CallOverlayList() {
        loadingPopup.Message("Loading...");
        openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetUserOverlays", "{ }", null, function (data) {
            var str = "";
            if (data.d != null) {
                str += "<div class='add-item-list-header'>Available Overlays</div>";
                for (var i = 0; i < data.d.length; i++) {
                    var title = "Add Overlay";
                    var imgClass = "";
                    if (openWSE.ConvertBitToBoolean(data.d[i][3])) {
                        title = "Remove Overlay";
                        imgClass = "add-item-list-item-hasitem";
                    }

                    var table = "<table width='100%'><tr><td style='width: 170px;'><div class='pad-right-big'>" + data.d[i][0] + "</div></td><td align='left'>" + data.d[i][2] + "</td></tr></table>";
                    str += "<div class='add-item-list-item " + imgClass + "' title='" + title + "' onclick=\"openWSE.AddRemoveOverlayClick('" + data.d[i][1] + "', this);\">" + table + "</div>";
                }

                if (data.d.length == 0) {
                    str += "<div class='emptyGridView'>No overlays available</div>";
                }
            }

            if ($("body").find("#overlayEdit-element").length === 0) {
                $("body").append($("#overlayEdit-element"));
            }

            $("#overlayEdit-element").find("#overlay-edit-list").html(str);
            loadingPopup.RemoveMessage();
            LoadModalWindow(true, 'overlayEdit-element', 'Add/Remove Overlay');
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("There was an error retrieving your overlays. Please try again.");
        });
    }
    function AddRemoveOverlayClick(id, _this) {
        if ($(_this).hasClass("add-item-list-item-hasitem")) {
            OverlayDisable(id);
            $(_this).removeClass("add-item-list-item-hasitem");
        }
        else {
            loadingPopup.Message("Adding Overlay...");
            $(_this).addClass("add-item-list-item-hasitem");
            $("#hf_loadOverlay1").val(id);
            openWSE.CallDoPostBack("hf_loadOverlay1", "");
        }
    }
    function UpdateOverlayTable() {
        var $pnlId = $("#" + openWSE_Config.overlayPanelId);
        if ($pnlId.length > 0) {
            var tableCount = 0;
            $(".workspace-overlay-selector").each(function (index) {
                // Load the js and css files
                LoadCSSFilesInApp($(this).attr("data-overlayid"));
            });
        }
    }


    /* Notifications */
    function GetUserNotifications() {
        if (!openWSE_Config.demoMode) {
            if ($("#notification-tab-b").is(":visible") || $("#Notifications_tab").hasClass("active-tab")) {
                if (!runningNoti) {
                    runningNoti = true;
                    var myIds = new Array();
                    $("#table_NotiMessages tr").each(function (index) {
                        myIds[index] = $(this).attr("id");
                    });

                    var notiHandler = openWSE.siteRoot() + "WebServices/NotificationRetrieve.asmx/LoadUserNotifications";
                    openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/LoadUserNotifications", '{ "_currIds": "' + myIds + '" }', null, function (data) {
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
                        CheckIfCanAddMore();
                        SetNotificationScroll();
                    }, function (data) {
                        SetNotificationScroll();
                        $("#NotificationHolder").html("<div class='pad-top-big pad-bottom' style='color: Red; text-align: center;'>Error retrieving notifications!</div>");
                        $("#lb_clearNoti").hide();
                        runningNoti = false;
                    });
                }
            }
        }
    }
    function CheckIfCanAddMore() {
        var elem = $("#notification-tab-b").find(".li-pnl-tab")[0];
        if (elem != null) {
            var innerHeight = $("#notification-tab-b").find(".li-pnl-tab").innerHeight();
            var maxHeight = parseInt($("#notification-tab-b").find(".li-pnl-tab").css("max-height"));
            if ((innerHeight >= elem.scrollHeight) || (elem.scrollHeight < maxHeight)) {
                var totalMessages = parseInt($("#lbl_notifications").find("span").html());
                var currMessages = parseInt($("#table_NotiMessages tr").length);
                if (currMessages < totalMessages) {
                    GetMoreUserNotifications();
                }
            }
        }
    }
    function GetMoreUserNotifications() {
        if (!openWSE_Config.demoMode) {
            if ($("#notification-tab-b").is(":visible") || $("#Notifications_tab").hasClass("active-tab")) {
                if ((!runningMoreNoti) && (!runningNoti)) {
                    runningMoreNoti = true;
                    var myIds = new Array();
                    $("#table_NotiMessages tr").each(function (index) {
                        myIds[index] = $(this).attr("id");
                    });

                    openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/LoadMoreUserNotifications", '{ "_currIds": "' + myIds + '","_currCount": "' + parseInt($("#table_NotiMessages tr").length) + '" }', null, function (data) {
                        if ((data.d != null) && (data.d != "")) {
                            $("#table_NotiMessages").append($.trim(data.d));
                        }
                        SetNotificationScroll();
                        runningMoreNoti = false;
                        CheckIfCanAddMore();
                    }, function (data) {
                        SetNotificationScroll();
                        $("#NotificationHolder").append("<div class='pad-top-big pad-bottom' style='color: Red; text-align: center;'>Error retrieving notifications!</div>");
                        runningMoreNoti = false;
                    });
                }
            }
        }
    }
    function RefreshNotifications() {
        if (!openWSE_Config.demoMode) {
            if ($("#notification-tab-b").is(":visible") || $("#Notifications_tab").hasClass("active-tab")) {
                if (!runningMoreNoti) {
                    runningMoreNoti = true;

                    var myIds = new Array();
                    $("#table_NotiMessages tr").each(function (index) {
                        myIds[index] = $(this).attr("id");
                    });

                    openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/RefreshUserNotifications", '{ "_currIds": "' + myIds + '" }', null, function (data) {
                        if ((data.d[0] != null) && (data.d[0] != "")) {
                            if ($("#table_NotiMessages").length > 0) {
                                $("#table_NotiMessages").prepend($.trim(data.d[0]));
                            }
                            else {
                                var table = "<table ID='table_NotiMessages' class='table-notiMessages' cellpadding='5' cellspacing='5'>";
                                table += $.trim(data.d[0]) + "</table>";
                                $("#NotificationHolder").html(table);
                            }
                        }

                        if (data.d[1] != null) {
                            for (var i = 0; i < data.d[1].length; i++) {
                                var tempId = data.d[1][i];
                                $("#" + tempId).fadeOut(openWSE_Config.animationSpeed, function () {
                                    $("#" + tempId).remove();
                                });
                            }
                        }

                        if ($("#lbl_notifications").find("span").html() == "0") {
                            $("#NotificationHolder").html("<div id='no-notifications-id'>No notifications found</div>");
                            $("#lb_clearNoti").hide();
                        }
                        else {
                            $("#lb_clearNoti").show();
                        }

                        SetNotificationScroll();
                        runningMoreNoti = false;
                    }, function (data) {
                        SetNotificationScroll();
                        $("#NotificationHolder").html("<div class='pad-top-big pad-bottom' style='color: Red; text-align: center;'>Error retrieving notifications!</div>");
                        runningMoreNoti = false;
                    });
                }
            }
        }
    }
    function CloseNoti() {
        $("#NotificationHolder").html("<div class='pad-all'>Loading notifications...</div>");
        $("#lb_clearNoti").hide();
        runningNoti = false;
    }
    function ResetNoti() {
        $("#lbl_notifications").removeClass("notifications-new");
        $("#lbl_notifications").addClass("notifications-none");
        $("#lbl_notifications").html("<span>0</span>");
        $("#NotificationHolder").html("<div id='no-notifications-id'>No notifications found</div>");
        $("#lb_clearNoti").hide();
    }
    function NotiActionsClearAll() {
        if (!openWSE_Config.demoMode) {
            openWSE.ConfirmWindow("Are you sure you want to dismiss all notifications?", function () {
                loadingPopup.Message("Clearing Notifications...");
                openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/DeleteNotifications", '{ "id": "' + "ClearAll" + '" }', null, function (data) {
                    if (openWSE.ConvertBitToBoolean(data.d)) {
                        loadingPopup.RemoveMessage();
                        ResetNoti();
                        CloseTopDropDowns();
                    }
                });
            }, null);
        }
    }
    function NotiActionsHideInd(_this) {
        if (!openWSE_Config.demoMode) {
            loadingPopup.Message("Deleting. Please Wait...");
            var $this = $(_this).closest("tr");
            var id = $this.attr("id");
            openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/DeleteNotifications", '{ "id": "' + id + '" }', null, function (data) {
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    $this.fadeOut(openWSE_Config.animationSpeed, function () {
                        loadingPopup.RemoveMessage();
                        $this.remove();
                        if ($("#lbl_notifications").find("span").html() == "1") {
                            ResetNoti();
                        }
                        else {
                            var currTotal = parseInt($("#lbl_notifications").find("span").html());
                            currTotal -= 1;
                            if (currTotal < 0) {
                                currTotal = 0;
                            }
                            $("#lbl_notifications").html("<span>" + currTotal + "</span>");
                        }

                        CheckIfCanAddMore();
                    });
                }
            });
        }
    }
    function SetNotificationScroll() {
        var $scrollContainer = $("#notification-tab-b").find(".li-pnl-tab");
        if (window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1 & $("#Notifications_tab").length > 0 && $("#Notifications_tab").hasClass("active-tab")) {
            $scrollContainer = $("#main_container");
        }

        $scrollContainer.scroll(function () {
            var elem = this;
            if (elem != null) {
                var $_scrollBar = $(this);
                var temp = $_scrollBar.scrollTop();
                var innerHeight = $_scrollBar.innerHeight();
                if (temp > 0) {
                    if (temp + innerHeight >= elem.scrollHeight) {
                        var totalMessages = parseInt($("#lbl_notifications").find("span").html());
                        var currMessages = parseInt($("#table_NotiMessages tr").length);
                        if (currMessages < totalMessages) {
                            GetMoreUserNotifications();
                        }
                    }
                }
            }
        });

        openWSE.ResizeTopDropDowns();
    }
    function ShowNewNotificationPopup() {
        if ($("#pnl_myNotificationList").length === 0 && $("#lbl_notifications").length > 0) {
            try {
                var hasNewNoti = document.getElementById("hf_noti_update_hiddenField").value;

                if (hasNewNoti == "") {
                    hasNewNoti = "0";
                }

                if (openWSE.ConvertBitToBoolean(hasNewNoti)) {
                    var _lbl_noti_update_message = $.trim($("#lbl_noti_update_message").html());
                    var _lbl_noti_update_popup_Description = $.trim($("#lbl_noti_update_popup_Description").html());

                    if (_lbl_noti_update_message) {
                        $("#noti-update-element").find(".noti-update-element-align").css("top", "");
                        if ($("#top_bar_toolview_holder").length > 0 && $("#top_bar_toolview_holder").is(":visible")) {
                            $("#noti-update-element").find(".noti-update-element-align").css("top", ($("#top_bar_toolview_holder").outerHeight() + 10) + "px");
                        }

                        $("#noti-update-element").show();
                        if (_lbl_noti_update_popup_Description) {
                            notifyMeFromBrowser(_lbl_noti_update_message, $("#noti-update-element").find("#img_noti_update_image").attr("src"), _lbl_noti_update_popup_Description);
                        }

                        setTimeout(function () {
                            $("#noti-update-element").hide();
                            $("#hf_noti_update_hiddenField").val("false");
                        }, 2500);
                    }
                }
            }
            catch (evt) { }
        }
    }
    function UpdateNotificationCount(count) {
        count = parseInt(count);
        if (count > 0) {
            $("#lbl_notifications").html("<span>" + count + "</span>");
            $("#lbl_notifications").removeClass("notifications-none").addClass("notifications-new");
        }
        else {
            $("#lbl_notifications").html("<span>0</span>");
            $("#lbl_notifications").removeClass("notifications-new").addClass("notifications-none");
        }
    }
    function notifyMeFromBrowser(title, image, message) {
        try {
            // Let's check if the browser supports notifications
            if ("Notification" in window) {
                var options = {
                    body: message,
                    icon: image
                }

                var newNotif;
                if (Notification.permission === "granted") {
                    // If it's okay let's create a notification
                    newNotif = new Notification(title, options);
                }

                else if (Notification.permission !== 'denied') {
                    Notification.requestPermission(function (permission) {
                        // If the user accepts, let's create a notification
                        if (permission === "granted") {
                            newNotif = new Notification(title, options);
                        }
                    });
                }

                if (newNotif !== null && newNotif !== undefined) {
                    newNotif.onclick = function (event) {
                        event.preventDefault();
                        window.open(GetSiteRoot() + 'SiteTools/UserTools/MyNotifications.aspx', '_blank');
                        try {
                            if (event.target) {
                                event.target.close();
                            }
                        }
                        catch (evt) { }
                    };
                }
            }
        }
        catch (ex) { }
    }


    /* Create Account (NoLoginRequired) */
    function LoadCreateAccountHolder() {
        if ($("#Login-holder").css("display") != "none") {
            $("#Login-holder").hide();

            var fullUrl = GetSiteRoot() + "SiteTools/iframes/CreateAccount.aspx";
            if (GetSiteRoot() == "/Scripts/SiteCalls/Full/" || GetSiteRoot() == "/Scripts/SiteCalls/Min/") {
                fullUrl = "/SiteTools/iframes/CreateAccount.aspx";
            }

            $("#iframe-createaccount-holder").html("<iframe id='iframe-demo' src='" + fullUrl + "' frameborder='0' width='595px' style='visibility: hidden;'></iframe>");
            $("#iframe-createaccount-holder").append("<div style='text-align: center;'><h3 id='loadingControls'>Loading Controls. Please Wait...</h3></div>");
            $("#CreateAccount-holder").fadeIn(openWSE_Config.animationSpeed);
            document.getElementById("iframe-demo").onload = function () {
                $("#loadingControls").remove();
                $("#register_password_cancel").show();
                $("#iframe-demo").css({
                    height: "290px",
                    visibility: "visible"
                });
                AdjustLoginModalPopup();
            };
        }
        else {
            $("#CreateAccount-holder, #ForgotPassword-holder, #register_password_cancel").hide();
            $("#iframe-createaccount-holder").html("");
            $("#Login-holder").fadeIn(openWSE_Config.animationSpeed);
        }

        AdjustLoginModalPopup();
    }
    function LoadRecoveryPassword() {
        $("#tb_username_recovery").val("");
        $("#lbl_passwordResetMessage").html("");
        $("#UserNameRequired_recovery").css("visibility", "hidden");
        $("#Login-holder").hide();
        $("#register_password_cancel").show();
        $("#ForgotPassword-holder").fadeIn(openWSE_Config.animationSpeed);
        AdjustLoginModalPopup();
    }
    function AdjustLoginModalPopup() {
        if ($("#LoginModalPopup-element").length > 0 && $("#LoginModalPopup-element").css("display") == "block") {
            var top = $("#LoginModalPopup-element").find(".Modal-element-modal").css("top");
            if (top == "auto") {
                $("#LoginModalPopup-element").find(".Modal-element-align").css({
                    marginTop: -($("#LoginModalPopup-element").find(".Modal-element-modal").height() / 2),
                    marginLeft: -($("#LoginModalPopup-element").find(".Modal-element-modal").width() / 2)
                });
            }
        }
    }


    /* IFrame screens */
    function LoadIFrameContent(url) {
        AddUrlHashQueryPath("iframecontent", url);
    }
    function LoadIFrameContentHistory(url) {
        $("#iframe-container-helper").remove();
        $(".remove-on-iframe-close").remove();

        url = unescape(url);
        var iframeHeight = $(window).height() - 40;
        if (!isExternal(url)) {
            url = openWSE.siteRoot() + url;
        }

        var iframeTitle = url;
        if (iframeTitle.indexOf(".") > 0) {
            iframeTitle_split = url.split(".");
            iframeTitle = iframeTitle_split[iframeTitle_split.length - 2];
            if (iframeTitle.indexOf("/") >= 0) {
                iframeTitle_split = iframeTitle.split("/");
                iframeTitle = iframeTitle_split[iframeTitle_split.length - 1];

                var result = iframeTitle.replace(/([A-Z]+)/g, ",$1").replace(/^,/, "");
                iframeTitle_split = result.split(",");

                if (iframeTitle_split.length > 0) {
                    iframeTitle = "";
                    for (var i = 0; i < iframeTitle_split.length; i++) {
                        iframeTitle += iframeTitle_split[i] + " ";
                    }

                    iframeTitle = $.trim(iframeTitle);
                }
            }
        }

        if (window.location.href.toLowerCase().indexOf("grouporg.aspx") > 0 && iframeTitle.toLowerCase() === "acct settings") {
            iframeTitle = "Group Defaults";
        }
        else if (window.location.href.toLowerCase().indexOf("useraccounts.aspx") > 0 && window.location.href.toLowerCase().indexOf("demouser") > 0 && iframeTitle.toLowerCase() === "acct settings") {
            iframeTitle = "Demo User Defaults";
        }
        else if (window.location.href.toLowerCase().indexOf("useraccounts.aspx") > 0 && window.location.href.toLowerCase().indexOf("newuserdefaults") > 0 && iframeTitle.toLowerCase() === "acct settings") {
            iframeTitle = "New User Defaults";
        }
        else if (window.location.href.toLowerCase().indexOf("useraccounts.aspx") > 0 && window.location.href.toLowerCase().indexOf("acctsettings.aspx") > 0) {
            iframeTitle = "Account Settings";
        }

        var siteNameText = openWSE_Config.siteName;
        if (openWSE_Config.groupLoginName !== "" && $("#lnk_BackToWorkspace").find(".title-text").length > 0) {
            var _tempTitle = $.trim($("#lnk_BackToWorkspace").find(".title-text").html());
            if (_tempTitle) {
                siteNameText = _tempTitle;
            }
        }

        CloseTopDropDowns();

        var titleBgLogo = $("#lnk_BackToWorkspace").find(".title-logo").css("background-image");
        if (!titleBgLogo) {
            titleBgLogo = openWSE.siteRoot() + "Standard_Images/logo.png";
        }

        if (titleBgLogo.indexOf("url(") !== 0) {
            titleBgLogo = "url(" + titleBgLogo + ")";
        }

        var lrgImgClass = "";
        if ($("#lnk_BackToWorkspace").find(".title-logo").hasClass("large-img") || $("#top_bar_toolview_holder").find(".title-logo").hasClass("large-img")) {
            lrgImgClass = " large-img";
        }

        var topBarHolder = "<div class='iframe-top-bar remove-on-iframe-close'><div class='iframe-title-logo" + lrgImgClass + "'><span class='title-logo' style='background-image: " + titleBgLogo + ";'></span><span class='title-text'>" + siteNameText + "</span><div class='clear'></div></div><span class='iframe-title-top-bar'>" + iframeTitle + "</span><a onclick='openWSE.CloseIFrameContent();return false;' class='close-iframe' title='Close'></a><div class='clear'></div></div>"
        var iframe = "<iframe id='iframe-content-src' src='" + url + "' width='100%' frameborder='0' style='height: " + iframeHeight + "px;'></iframe>";
        var holder = topBarHolder + "<div id='iframe-container-helper' style='top: 41px; bottom: 0;'>" + iframe + "<div class='loading-background-holder' data-usespinner='true'><div></div></div></div>";
        $("body").append(holder);
        $("form").hide();
        $("#iframe-container-helper").fadeIn(openWSE_Config.animationSpeed);

        document.getElementById("iframe-content-src").onload = function () {
            $(document).ready(function () {
                setTimeout(function () {
                    $("#iframe-container-helper").find(".loading-background-holder").remove();
                }, 150);
            });
        };
        $(".ui-helper-hidden-accessible").find("div").hide();

        SetContainerTopPos(true);
    }
    function CloseIFrameContent() {
        $("form").show();
        $("#iframe-container-helper").remove();
        $(".remove-on-iframe-close").remove();
        SetContainerTopPos(true);

        RemoveUrlHashQueryPath("iframecontent");
        openWSE.SetLeftSidebarScrollTop();
        $(window).resize();
    }
    function isExternal(url) {
        var match = url.match(/^([^:\/?#]+:)?(?:\/\/([^\/?#]*))?([^?#]+)?(\?[^#]*)?(#.*)?/);
        if (typeof match[1] === "string" && match[1].length > 0 && match[1].toLowerCase() !== location.protocol) return true;
        if (typeof match[2] === "string" && match[2].length > 0 && match[2].replace(new RegExp(":(" + { "http:": 80, "https:": 443 }[location.protocol] + ")?$"), "") !== location.host) return true;
        return false;
    }


    /* Workspace Background Modal */
    var selectedImgFolder = "user";
    var imgFolderChange = false;
    function BackgroundSelector(showLoading) {
        try {
            if ($("#background_selector_overlay").length > 0) {
                $("#background_selector_overlay").remove();
            }

            if (showLoading) {
                $("#background-selector-holder").html("<div class='pad-all-sml'>Loading background settings...</div>");
            }
            else {
                loadingPopup.Message("Loading...");
            }

            var workspaceNum = Getworkspace().replace("workspace_", "");
            if (!openWSE_Config.multipleBackgrounds) {
                workspaceNum = "1";
            }

            openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetServerImageList", '{ "_workspace": "' + workspaceNum + '", "folder": "' + selectedImgFolder + '" }', null, function (data) {
                if (data.d.length == 9) {
                    $("#background-selector-holder").html("");
                    $(".background-more-settings-holder").html("");

                    if (data.d[0] != "") {
                        var target = "";
                        if ($("#hyp_AccountCustomizations").attr("target")) {
                            target = " target='" + $("#hyp_AccountCustomizations").attr("target") + "'";
                        }
                        $(".background-more-settings-holder").html("<a href='" + GetSiteRoot() + "SiteTools/UserTools/AcctSettings.aspx#?tab=pnl_WorkspaceContainer'" + target + " onclick='openWSE.CloseTopDropDowns();'>More Settings</a>");
                    }

                    var modal = "<div id='background-settings-div'>";
                    modal += "<div class='clear-space'></div>";
                    modal += "<h3>Background</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";

                    if (openWSE_Config.demoMode) {
                        modal += "<div class='current-background-list-holder'><small><i>Current background list cannot be viewed in demo mode</i></small><div class='clear-space'></div></div>";
                    }
                    else {
                        modal += "<div class='current-background-list-holder'>" + data.d[8] + "</div>";
                    }
                    modal += "<div class='clear'></div>";

                    // Add Color textbox
                    modal += "<div class='input-settings-holder float-left' style='clear: none!important; margin-right: 30px;'><span class='font-bold'>Solid Color</span><div class='clear-space-two'></div>";
                    modal += "<input id='tb_solidColorBg' type='color' class='textEntry float-left margin-right' style='width: 75px;' value='#FFFFFF' />";
                    modal += "<input id='btn_urlbgcolor' type='button' value='Set Color' class='input-buttons float-left' onclick='openWSE.updateBackgroundColor()' />";
                    modal += "<div class='clear'></div></div>";

                    // Add Url Link textbox
                    modal += "<div class='input-settings-holder float-left' style='clear: none!important;'><span class='font-bold'>Url Image</span><div class='clear-space-two'></div>";
                    modal += "<input id='tb_imageurl' type='text' class='textEntry'>";
                    modal += "<input id='btn_urlupdate' type='button' value='Set Url Image' class='input-buttons margin-left' onclick='openWSE.updateBackgroundURL()' />";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<small>Copy and paste any link that contains an image</small>";
                    modal += "</div>";

                    // Add Upload iframe
                    if (data.d[0] != "") {
                        modal += "<div class='clear'></div><div class='input-settings-holder'><span class='font-bold'>Upload Image</span><div class='clear'></div>";
                        modal += "<iframe src='" + data.d[0] + "' frameborder='0' style='width: 100%; height: 50px; overflow: hidden;'></iframe>";
                        modal += "</div>";
                        modal += "<div class='clear-space'></div>";
                        modal += "<div class='float-left'><span class='font-bold pad-right'>Folder</span><select id='dd_userimagefolder' onchange='openWSE.ChangeImageFolder();'><option value='public'>Public</option><option value='user'>User Uploads</options></select></div>";
                        modal += "<div class='clear-space'></div>";
                    }
                    else {
                        selectedImgFolder = "user";
                    }

                    modal += "<div class='img-list-selector' style='width: 630px; overflow: auto; margin-right: 10px; clear: both; white-space: nowrap;'>" + data.d[1] + "</div>";

                    modal += "<div class='clear-space'></div>";
                    modal += "<a onclick='openWSE.ClearBackground();return false;'>Clear Backgrounds</a>";
                    modal += "<div class='clear'></div>";
                    modal += "</div>";

                    modal += "<div class='clear-space'></div>";
                    modal += "<div class='border-top'></div>";
                    modal += "<div class='clear-space'></div>";

                    var marginStyleOptions = "50px";

                    // Background Position
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Background Position</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<select class='background-setting-select' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundposition'>";
                    modal += "<option value='left top'>Left Top</option>";
                    modal += "<option value='left center'>Left Center</option>";
                    modal += "<option value='left bottom'>Left Bottom</option>";
                    modal += "<option value='right top'>Right Top</option>";
                    modal += "<option value='right center'>Right Center</option>";
                    modal += "<option value='right bottom'>Right Bottom</option>";
                    modal += "<option value='center top'>Center Top</option>";
                    modal += "<option value='center center'>Center Center</option>";
                    modal += "<option value='center bottom'>Center Bottom</option>";
                    modal += "</select>";
                    modal += "</div></div>";

                    // Background Size
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Background Size</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<select class='background-setting-select' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundsize'>";
                    modal += "<option value='auto'>Normal</option>";
                    modal += "<option value='100% 100%'>Stretch</option>";
                    modal += "<option value='cover'>Cover</option>";
                    modal += "<option value='contain'>Contain</option>";
                    modal += "</select>";
                    modal += "</div></div>";

                    // Background Repeat
                    modal += "<div id='background-repeat-holder'>";
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Repeat Background</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<div class='field switch inline-block'>";
                    modal += "<span class='background-setting-select cb-enable selected'><input id='rb_backgroundrepeat_on' type='radio' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundrepeat' checked='checked'><label for='rb_backgroundrepeat_on'>Yes</label></span>";
                    modal += "<span class='background-setting-select cb-disable'><input id='rb_backgroundrepeat_off' type='radio' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundrepeat'><label for='rb_backgroundrepeat_off'>No</label></span>";
                    modal += "</div>";
                    modal += "</div></div>";
                    modal += "</div>";

                    // Background Color
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Default Background</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<div class='float-left'><input id='tb_defaultbackgroundcolor' type='color' class='textEntry float-left margin-right' style='width: 75px;' /></div>";
                    modal += "<input id='btn_defaultbackgroundcolor' type='button' value='Update' class='input-buttons float-left background-setting-color' onclick='openWSE.UpdateBackgroundSetting(this)' data-name='backgroundcolor' />";
                    modal += "<div class='clear-space'></div>";
                    modal += "</div></div>";

                    // Background Timer
                    modal += "<div id='background-timer-holder'>";
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Background Timer</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<div class='float-left'><input id='tb_defaultbackgroundtimer' type='number' class='textEntry float-left margin-right' min='1' max='999' style='width: 55px;' /></div>";
                    modal += "<input id='btn_defaultbackgroundtimer' type='button' value='Update' class='input-buttons float-left background-setting-timer' onclick='openWSE.UpdateBackgroundSetting(this)' data-name='backgroundlooptimer' />";
                    modal += "<div class='clear-space'></div>";
                    modal += "</div></div>";
                    modal += "</div>";

                    // Individual Workspace Backgrounds
                    modal += "<div id='background-individual-workspace-holder'>";
                    modal += "<div class='float-left margin-bottom-big' style='padding-right: " + marginStyleOptions + ";'><h3>Individual Workspace Backgrounds</h3>";
                    modal += "<div class='clear-space-two'></div>";
                    modal += "<div class='pad-top pad-bottom'>";
                    modal += "<div class='field switch inline-block'>";
                    modal += "<span class='background-setting-select cb-enable selected'><input id='rb_backgroundindividual_on' type='radio' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundindividual' checked='checked'><label for='rb_backgroundindividual_on'>Yes</label></span>";
                    modal += "<span class='background-setting-select cb-disable'><input id='rb_backgroundindividual_off' type='radio' onchange=\"openWSE.UpdateBackgroundSetting(this);\" data-name='backgroundindividual'><label for='rb_backgroundindividual_off'>No</label></span>";
                    modal += "</div>";
                    modal += "</div></div>";
                    modal += "</div>";

                    modal += "<div class='clear'></div>";

                    modal += "</div>";
                    $("#background-selector-holder").html(modal);

                    if (data.d[0] != "") {
                        $("#background-selector-holder").find("#dd_userimagefolder").val(selectedImgFolder);
                    }

                    if (data.d[2] != "") {
                        $(".background-setting-select[data-name='backgroundposition']").val(data.d[2]);
                    }
                    if (data.d[3] != "") {
                        $(".background-setting-select[data-name='backgroundsize']").val(data.d[3]);
                        if (data.d[3] == "100% 100%" || data.d[3] == "cover") {
                            $("#background-repeat-holder").hide();
                        }
                    }
                    if (data.d[4] != "") {
                        if (data.d[4] == "true" || data.d[4] == "1") {
                            $("#rb_backgroundrepeat_on").attr("checked", "checked");
                            $("#rb_backgroundrepeat_off").removeAttr("checked");
                            $("#rb_backgroundrepeat_on").parent().addClass("selected");
                            $("#rb_backgroundrepeat_off").parent().removeClass("selected");
                        }
                        else {
                            $("#rb_backgroundrepeat_on").removeAttr("checked");
                            $("#rb_backgroundrepeat_off").attr("checked", "checked");
                            $("#rb_backgroundrepeat_on").parent().removeClass("selected");
                            $("#rb_backgroundrepeat_off").parent().addClass("selected");
                        }
                    }
                    if (data.d[5] != "") {
                        $("#tb_defaultbackgroundcolor").val(data.d[5]);
                    }

                    if (data.d[6] != "") {
                        $("#tb_defaultbackgroundtimer").val(data.d[6]);
                    }

                    if (data.d[7] != "") {
                        if (data.d[7] == "true" || data.d[7] == "1") {
                            $("#rb_backgroundindividual_on").attr("checked", "checked");
                            $("#rb_backgroundindividual_off").removeAttr("checked");
                            $("#rb_backgroundindividual_on").parent().addClass("selected");
                            $("#rb_backgroundindividual_off").parent().removeClass("selected");
                        }
                        else {
                            $("#rb_backgroundindividual_on").removeAttr("checked");
                            $("#rb_backgroundindividual_off").attr("checked", "checked");
                            $("#rb_backgroundindividual_on").parent().removeClass("selected");
                            $("#rb_backgroundindividual_off").parent().addClass("selected");
                        }
                    }

                    if ($("#workspace-selector").is(":visible")) {
                        $("#background-individual-workspace-holder").show();
                    }
                    else {
                        $("#background-individual-workspace-holder").hide();
                    }

                    RadioButtonStyle();

                    $("#background-timer-holder").hide();
                    if (GetBackgroundArray().length > 1) {
                        $("#background-timer-holder").show();
                    }

                    if ($("#background-selector-holder").find(".img-list-selector").find(".image-selector").length > 0 && $("#background-selector-holder").find(".img-list-selector").find(".no-images-found").length > 0) {
                        $("#background-selector-holder").find(".img-list-selector").find(".no-images-found").remove();
                    }

                    loadingPopup.RemoveMessage();
                    openWSE.ResizeTopDropDowns();

                    $("#background-selector-holder").find(".img-list-selector .image-selector").each(function () {
                        $(this).attr("onclick", "openWSE.BackgroundImageSelect_Clicked(this, event);");
                    });
                    $("#background-selector-holder").find(".img-list-selector .delete-uploadedimg").each(function () {
                        $(this).attr("onclick", "openWSE.BackgroundImageDelete_Clicked(this);");
                    });
                    $("#background-selector-holder").find(".current-background-list-holder .remove-selectedimg").each(function () {
                        $(this).attr("onclick", "openWSE.BackgroundImageAddRemove_Clicked(this);");
                    });
                }
                else {
                    AlertWindow("There was an error pulling the backgrounds down from the server. Please try again.", window.location.href);
                }
            });
        }
        catch (evt) {
            $("#background-selector-holder").html("<div class='pad-all-sml'>No settings found</div>");
        }

        openWSE.ResizeTopDropDowns();
    }
    function ClearBackground() {
        loadingPopup.Message("Updating...");

        var workspaceNum = Getworkspace();
        if (!openWSE_Config.multipleBackgrounds) {
            workspaceNum = "workspace_1";
        }

        $("#MainContent_" + workspaceNum).attr("data-backgroundimg", "");
        SetNewBackground();

        openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum + '","_img": "' + "" + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
            loadingPopup.RemoveMessage();
            BackgroundSelector(false);
        });
    }
    function updateBackgroundURL() {
        var img = $.trim($("#background-selector-holder").find("#tb_imageurl").val());
        if (img) {
            SaveNewBg(img);
        }
    }
    function updateBackgroundColor() {
        var img = $("#background-selector-holder").find("#tb_solidColorBg").val();
        if (img.length == 7) {
            SaveNewBg(img);
        }
    }
    function SaveNewBg(img) {
        var workspaceNum = Getworkspace();
        if (!openWSE_Config.multipleBackgrounds) {
            workspaceNum = "workspace_1";
        }

        var dataBackgrounds = $("#MainContent_" + workspaceNum).attr("data-backgroundimg");
        if (dataBackgrounds) {
            ConfirmWindowAltBtns("Do you want to remove the previous selected background(s)? Click No to add the new background to the existing list.", function () {
                if (!openWSE_Config.demoMode) {
                    loadingPopup.Message("Updating...");
                    $("#MainContent_" + workspaceNum).attr("data-backgroundimg", "");
                    openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum.replace("workspace_", "") + '","_img": "' + "" + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
                        loadingPopup.RemoveMessage();
                        loadingPopup.Message("Saving Background");
                        openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum.replace("workspace_", "") + '","_img": "' + img + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
                            loadingPopup.RemoveMessage();
                            BackgroundSelector(false);
                        });
                    });
                }

                UpdateDataBackgroundImageAttr(img);
                SetNewBackground();
            }, function () {
                if (!openWSE_Config.demoMode) {
                    loadingPopup.Message("Saving Background");
                    openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum.replace("workspace_", "") + '","_img": "' + img + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
                        loadingPopup.RemoveMessage();
                        BackgroundSelector(false);
                    });
                }

                UpdateDataBackgroundImageAttr(img);
                SetNewBackground();
            });
        }
        else {
            if (!openWSE_Config.demoMode) {
                loadingPopup.Message("Saving Background");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum.replace("workspace_", "") + '","_img": "' + img + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
                    loadingPopup.RemoveMessage();
                    BackgroundSelector(false);
                });
            }

            UpdateDataBackgroundImageAttr(img);
            SetNewBackground();
        }
    }
    function UpdateDataBackgroundImageAttr(img) {
        var dataArray = GetBackgroundArray();
        if (img.indexOf("http://") == -1 && img.indexOf("https://") === -1 && img.indexOf("www://") === -1) {
            if (img.length > 6 && img.indexOf("#") !== 0) {
                if (img.indexOf("/" + openWSE_Config.siteRootFolder) == -1) {
                    if (img.indexOf("/") !== 0) {
                        img = "/" + img;
                    }

                    img = "/" + openWSE_Config.siteRootFolder + img;
                }
            }
            else {
                img = "#" + img.replace("#", "");
            }
        }

        var tempImageList = "";
        var foundImage = false;
        for (var i = 0; i < dataArray.length; i++) {
            if (dataArray[i] == img) {
                foundImage = true;
            }
            else {
                if (dataArray[i].indexOf("App_Themes/" + openWSE_Config.siteTheme + "/Body/default-bg.jpg") === -1) {
                    tempImageList += dataArray[i] + "|";
                }
            }
        }

        if (!foundImage) {
            tempImageList += img + "|";
        }

        var workspaceNum = Getworkspace();
        if (!openWSE_Config.multipleBackgrounds) {
            workspaceNum = "workspace_1";
        }

        $("#MainContent_" + workspaceNum).attr("data-backgroundimg", tempImageList);
    }
    function ChangeImageFolder() {
        selectedImgFolder = $("#background-selector-holder").find("#dd_userimagefolder").val();
        imgFolderChange = true;
        BackgroundSelector(false);
    }
    function UpdateBackgroundSetting(_this) {
        if ($(_this).length > 0) {
            var name = $(_this).attr("data-name");
            var value = $(_this).val();
            if ($(_this).hasClass("background-setting-color")) {
                value = $.trim($("#tb_defaultbackgroundcolor").val());
                openWSE_Config.defaultBackgroundColor = "#" + value;
            }
            else if ($(_this).hasClass("background-setting-timer")) {
                value = $.trim($("#tb_defaultbackgroundtimer").val());
                openWSE_Config.backgroundTimerLoop = value;
            }
            else {
                switch (name) {
                    case "backgroundposition":
                        openWSE_Config.defaultBackgroundPosition = value;
                        break;
                    case "backgroundsize":
                        openWSE_Config.defaultBackgroundSize = value;
                        if (value == "100% 100%" || value == "cover") {
                            $("#background-repeat-holder").hide();
                        }
                        else {
                            $("#background-repeat-holder").show();
                        }
                        break;
                    case "backgroundrepeat":
                        if ($(_this).attr("id") == "rb_backgroundrepeat_off") {
                            openWSE_Config.defaultBackgroundRepeat = "no-repeat";
                            $("#rb_backgroundrepeat_on").removeAttr("checked");
                            $("#rb_backgroundrepeat_off").attr("checked", "checked");
                            $("#rb_backgroundrepeat_on").parent().removeClass("selected");
                            $("#rb_backgroundrepeat_off").parent().addClass("selected");
                            value = "0";
                        }
                        else {
                            openWSE_Config.defaultBackgroundRepeat = "repeat";
                            $("#rb_backgroundrepeat_on").attr("checked", "checked");
                            $("#rb_backgroundrepeat_off").removeAttr("checked");
                            $("#rb_backgroundrepeat_on").parent().addClass("selected");
                            $("#rb_backgroundrepeat_off").parent().removeClass("selected");
                            value = "1";
                        }
                        break;
                    case "backgroundindividual":
                        if ($(_this).attr("id") == "rb_backgroundindividual_off") {
                            openWSE_Config.multipleBackgrounds = false;
                            $("#rb_backgroundindividual_on").removeAttr("checked");
                            $("#rb_backgroundindividual_off").attr("checked", "checked");
                            $("#rb_backgroundindividual_on").parent().removeClass("selected");
                            $("#rb_backgroundindividual_off").parent().addClass("selected");
                            value = "0";
                        }
                        else {
                            openWSE_Config.multipleBackgrounds = true;
                            $("#rb_backgroundindividual_on").attr("checked", "checked");
                            $("#rb_backgroundindividual_off").removeAttr("checked");
                            $("#rb_backgroundindividual_on").parent().addClass("selected");
                            $("#rb_backgroundindividual_off").parent().removeClass("selected");
                            value = "1";
                        }
                        break;
                }
            }

            SetCurrentWorkspaceBackground();

            if (!openWSE_Config.demoMode) {
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveBackgroundSetting", '{ "name": "' + name + '","value": "' + value + '" }', null, function (data) {
                    loadingPopup.RemoveMessage();
                    if (name == "backgroundindividual") {
                        BackgroundSelector(false);
                    }
                });
            }
            else {
                setTimeout(function () {
                    loadingPopup.RemoveMessage();
                }, 100);
                if (name == "backgroundindividual") {
                    BackgroundSelector(false);
                }
            }
        }
    }
    function BackgroundImageSelect_Clicked(_this, event) {
        if (event && event.target && $(event.target).hasClass("delete-uploadedimg")) {
            return;
        }

        var showConfirm = false;
        var workspaceNum = Getworkspace();
        if (!openWSE_Config.multipleBackgrounds) {
            workspaceNum = "workspace_1";
        }

        var dataBackgrounds = $("#MainContent_" + workspaceNum).attr("data-backgroundimg");
        if (dataBackgrounds) {
            showConfirm = true;
        }

        if (showConfirm) {
            ConfirmWindowAltBtns("Do you want to remove the current selected background(s)? Click No to add the new background to the existing list.", function () {
                loadingPopup.Message("Updating...");
                $("#MainContent_" + workspaceNum).attr("data-backgroundimg", "");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum + '","_img": "","folder": "' + selectedImgFolder + '" }', null, function (data) {
                    loadingPopup.RemoveMessage();
                    openWSE.BackgroundImageAddRemove_Clicked(_this);
                });
            }, function () {
                openWSE.BackgroundImageAddRemove_Clicked(_this);
            });
        }
        else {
            openWSE.BackgroundImageAddRemove_Clicked(_this);
        }
    }
    function BackgroundImageDelete_Clicked(_this) {
        var img = $(_this).attr("data-imgsrc");
        if (!openWSE_Config.demoMode && img) {
            ConfirmWindowAltBtns("Are you sure you want to permanently delete " + img + "?", function () {
                loadingPopup.Message("Deleting Background...");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/DeleteUploadedImage", '{ "_img": "' + img + '" }', null, function (data) {
                    loadingPopup.RemoveMessage();
                    BackgroundSelector(false);
                });
            }, null);
        }
    }
    function BackgroundImageAddRemove_Clicked(_this) {
        var $this = $(_this);
        if ($this.hasClass("remove-selectedimg")) {
            $this = $(_this).parent();
        }

        var img = "";
        if ($this.find(".color-bg-div").length > 0) {
            img = $this.find(".color-bg-div").attr("data-color");
        }
        else {
            img = $this.find("img").attr("data-imgsrc");
        }

        if (img.indexOf("Standard_Images/") === 0) {
            img = "/" + img;
        }

        if (!openWSE_Config.demoMode) {
            var workspaceNum = Getworkspace().replace("workspace_", "");
            if (!openWSE_Config.multipleBackgrounds) {
                workspaceNum = "1";
            }

            loadingPopup.Message("Saving Background");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/SaveNewBackground", '{ "_workspace": "' + workspaceNum + '","_img": "' + img + '","folder": "' + selectedImgFolder + '" }', null, function (data) {
                loadingPopup.RemoveMessage();
                BackgroundSelector(false);
                SetCurrentWorkspaceBackground();
            });
        }

        UpdateDataBackgroundImageAttr(img);
        SetNewBackground();
    }
    function SetBackgroundForWorkspaceDropdown() {
        var divsRendered = 0;
        var totalDivsToRender = 0;

        if ($("#workspace-selector").hasClass("active") || $("#workspace-selector").find(".b").css("display") !== "none") {
            $("#workspace-selector").find(".dropdown-db-selector").find(".workspace-selection-item").each(function () {
                var backgroundNumber = "1";
                if (openWSE_Config.multipleBackgrounds) {
                    backgroundNumber = $(this).attr("data-number");
                }

                if ($(this).find(".app-preview-holder").length > 0) {
                    $(this).find(".app-preview-holder").remove();
                }

                var $mainContainer = $("#main_container");

                var $workspaceSelector = $("#MainContent_workspace_" + backgroundNumber);
                if ($workspaceSelector.length > 0) {
                    var dataImages = $workspaceSelector.attr("data-backgroundimg");
                    if (!dataImages) {
                        dataImages = "/" + openWSE_Config.siteRootFolder + "/App_Themes/" + openWSE_Config.siteTheme + "/Body/default-bg.jpg";
                        if (!openWSE_Config.siteRootFolder) {
                            dataImages = "/App_Themes/" + openWSE_Config.siteTheme + "/Body/default-bg.jpg";
                        }
                    }
                    else {
                        dataImages = dataImages.split('|')[0];
                    }

                    $(this).css("background-color", "");
                    $(this).css("background-image", "");
                    if (dataImages.indexOf("#") == 0) {
                        $(this).css("background-color", dataImages);
                    }
                    else {
                        $(this).css("background-image", "url('" + dataImages + "')");
                    }
                }

                if (showAppPreviewInWorkspaceSelector) {
                    $workspaceSelector = $("#MainContent_workspace_" + $(this).attr("data-number"));
                    if ($workspaceSelector.length > 0) {
                        var previewHtml = "";
                        for (var i = 0; i < $workspaceSelector.find(".app-main-holder").length; i++) {
                            var $appMainHolder = $workspaceSelector.find(".app-main-holder").eq(i);
                            if ($appMainHolder.css("visibility") === "visible" && $appMainHolder.css("display") !== "none") {
                                var _offsetLeft = (parseInt($appMainHolder.css("left").replace("px", "")) / $mainContainer.outerWidth()) * $(this).outerWidth();
                                var _offsetTop = (parseInt($appMainHolder.css("top").replace("px", "")) / $mainContainer.outerHeight()) * $(this).outerHeight();
                                var _width = ($appMainHolder.outerWidth() / $mainContainer.outerWidth()) * $(this).outerWidth();
                                var _height = ($appMainHolder.outerHeight() / $mainContainer.outerHeight()) * $(this).outerHeight();
                                var _zIndex = $appMainHolder.css("z-index");
                                if ($appMainHolder.hasClass("app-maximized") || $appMainHolder.hasClass("auto-full-page")) {
                                    _offsetLeft = 0;
                                    _offsetTop = 0;
                                    _width = $(this).outerWidth();
                                    _height = $(this).outerHeight();
                                }

                                _width = _width - 8;
                                _height = _height - 8.5;

                                var backgroundImage = "";
                                if (typeof (html2canvas) === "function" && openWSE_Config.canvasWorkspaceAppPreviews) {
                                    totalDivsToRender++;
                                    if ($workspaceSelector.css("visibility") === "hidden") {
                                        $workspaceSelector.addClass("workspace-selector-clicked");
                                    }
                                    html2canvas($appMainHolder, {
                                        allowTaint: true,
                                        onrendered: function (canvas) {
                                            canvas.className = "app-preview-holder";
                                            canvas.style.width = this.width;
                                            canvas.style.height = this.height;
                                            canvas.style.top = this.offsetTop;
                                            canvas.style.left = this.offsetLeft;
                                            canvas.style.zIndex = this.zIndex;
                                            canvas.setAttribute("data-appid", this.appId);
                                            if ($(this.li).find("div.app-preview-holder[data-appid='" + this.appId + "']").length > 0) {
                                                $(this.li).find("div.app-preview-holder[data-appid='" + this.appId + "']").remove();
                                            }
                                            $(this.li).append(canvas);
                                            divsRendered++;
                                        }.bind({
                                            li: this,
                                            width: _width + "px",
                                            height: _height + "px",
                                            offsetTop: _offsetTop + "px",
                                            offsetLeft: _offsetLeft + "px",
                                            zIndex: _zIndex,
                                            appId: $appMainHolder.attr("data-appid")
                                        })
                                    });
                                }
                                else {
                                    if ($appMainHolder.find(".app-header-icon").length > 0 && $appMainHolder.find(".app-header-icon").css("display") != "none") {
                                        backgroundImage = "url('" + $appMainHolder.find(".app-header-icon").attr("src") + "')";
                                    }
                                    else if ($(".app-icon[data-appid='" + $appMainHolder.attr("data-appid") + "']").find("img").length > 0) {
                                        backgroundImage = "url('" + $(".app-icon[data-appid='" + $appMainHolder.attr("data-appid") + "']").find("img").attr("src") + "')";
                                    }
                                }

                                previewHtml += "<div class=\"app-preview-holder\" data-appid=\"" + $appMainHolder.attr("data-appid") + "\" style=\"width: " + _width + "px; height: " + _height + "px; top: " + _offsetTop + "px; left: " + _offsetLeft + "px; z-index: " + _zIndex + ";";
                                if (backgroundImage) {
                                    previewHtml += " background-image: " + backgroundImage + ";";
                                }
                                previewHtml += "\"></div>";
                            }
                        }

                        $(this).append(previewHtml);
                    }
                }
            });
        }

        if (totalDivsToRender > 0) {
            var renderInterval = setInterval(function () {
                if (divsRendered >= totalDivsToRender) {
                    $(".workspace-holder").removeClass("workspace-selector-clicked");
                    clearInterval(renderInterval);
                }
            }, 1);
        }
    }


    // Background Loop - Set
    var backgroundLoopTimer = null;
    var backgroundLoopIndex = 0;
    function BackgroundLoop() {
        if ($("#workspace_holder").length > 0) {
            clearInterval(backgroundLoopTimer);
            backgroundLoopIndex = 0;

            backgroundLoopTimer = setInterval(function () {
                var dataArray = GetBackgroundArray();
                if (dataArray.length > 1) {
                    backgroundLoopIndex = backgroundLoopIndex + 1;
                    if (backgroundLoopIndex >= dataArray.length) {
                        backgroundLoopIndex = 0;
                    }

                    SetNewBackground(dataArray[backgroundLoopIndex]);
                }
                else {
                    clearInterval(backgroundLoopTimer);
                }
            }, parseInt(openWSE_Config.backgroundTimerLoop) * 1000);
        }
    }
    function GetBackgroundArray() {
        var tempArray = new Array();
        if ($("#workspace_holder").length > 0) {
            var workspaceNum = Getworkspace();
            if (!openWSE_Config.multipleBackgrounds) {
                workspaceNum = "workspace_1";
            }

            var dataImages = $("#MainContent_" + workspaceNum).attr("data-backgroundimg");
            if (!dataImages) {
                dataImages = "/" + openWSE_Config.siteRootFolder + "/App_Themes/" + openWSE_Config.siteTheme + "/Body/default-bg.jpg";
                if (!openWSE_Config.siteRootFolder) {
                    dataImages = "/App_Themes/" + openWSE_Config.siteTheme + "/Body/default-bg.jpg";
                }
            }

            var tempImg = dataImages;
            var imgArray = tempImg.split('|');
            for (var i = 0; i < imgArray.length; i++) {
                if (imgArray[i] != "") {
                    tempArray.push(imgArray[i]);
                }
            }
        }

        return tempArray;
    }
    function SetCurrentWorkspaceBackground() {
        if ($("#workspace_holder").length > 0) {
            var tempArray = GetBackgroundArray();
            var currImage = tempArray[0];
            SetNewBackground(currImage);
            openWSE.BackgroundLoop();
        }
    }
    function SetNewBackground(img) {
        if (!img) {
            var tempData = GetBackgroundArray();
            img = tempData[tempData.length - 1];
        }

        if (img.length > 6 && img.indexOf("#") !== 0) {
            $("#site_mainbody").css("background-image", "url('" + img + "')");
            $("#site_mainbody").css("background-color", openWSE_Config.defaultBackgroundColor);
            $("#site_mainbody").css("background-position", openWSE_Config.defaultBackgroundPosition);
            $("#site_mainbody").css("background-repeat", openWSE_Config.defaultBackgroundRepeat);
            $("#site_mainbody").css("background-size", openWSE_Config.defaultBackgroundSize);
        }
        else if (img.indexOf("#") == 0) {
            $("#site_mainbody").css("background-image", "");
            $("#site_mainbody").css("background-color", img);
        }
        else {
            $("#site_mainbody").css("background-image", "");
            $("#site_mainbody").css("background-color", "#" + img);
        }
    }


    jQuery.fn.outerHTML = function (s) {
        return s ? this.before(s).remove() : jQuery("<p>").append(this.eq(0).clone()).html();
    };


    /* Workspace Selector Functions */
    function HideTasks(_this) {
        $(_this).find(".app-main-holder").each(function (index) {
            var id = $(this).attr("data-appid");
            if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").length != 0) {
                if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").css("display") != "none") {
                    $("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").hide();
                }
            }
        });

        openWSE.ToggleMinimizedAppBar();
    }
    function ShowTasks(_this) {
        $(_this).find(".app-main-holder").each(function (index) {
            var id = $(this).attr("data-appid");
            if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").length != 0) {
                if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").css("display") == "none") {
                    $("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").show();
                }
            }
        });

        openWSE.ToggleMinimizedAppBar();
    }
    function LoadCurrentWorkspace(workspace) {
        SetWorkspaceNumber(workspace);
        id = "#MainContent_workspace_" + workspace;
        ResizeAllAppBody($(id));

        $(id).css({
            visibility: "visible",
            opacity: 1.0,
            filter: "alpha(opacity=100)"
        });

        $('#workspace_holder .workspace-holder').each(function (index) {
            if ($(this).css("visibility") != "visible") {
                MoveOffScreen(this);
            }
        });

        SetCurrentWorkspaceBackground();
    }
    function Getworkspace() {
        var $this = $('#workspace_holder').find(".workspace-holder");
        var len = $this.length;
        for (var i = 0; i < len; i++) {
            if ($this.eq(i).css("visibility") == "visible") {
                var id = $this.eq(i).attr("id");
                return "workspace_" + id.substring(id.lastIndexOf("_") + 1);
            }
        }
        return "workspace_1";
    }
    $(document.body).on("click", "#workspace-selector", function (e) {
        var $selector = $(this).find(".dropdown-db-selector");

        if ($selector.length > 0) {
            if (!$(e.target).hasClass("workspace-selection-item") && !$(e.target).hasClass("workspace-selection-item-span") && !$(e.target).hasClass("app-preview-holder")) {
                var currentWorkspacenum = $("#workspace-selector").find(".workspace-menu-toggle > span").html();
                for (var i = 0; i < $selector.find(".workspace-selection-item").length; i++) {
                    var $this = $selector.find(".workspace-selection-item").eq(i);
                    if ($this.attr("data-number") == currentWorkspacenum) {
                        $this.addClass("selected");
                    }
                    else {
                        $this.removeClass("selected");
                    }
                }

                if (openWSE_Config.hoverPreviewWorkspace) {
                    $(".workspace-selection-item").hover(
                        function () {
                            var oldid = "#MainContent_" + openWSE.Getworkspace();
                            var newid = "#MainContent_workspace_" + $(this).attr("data-number");
                            if (oldid !== newid) {
                                openWSE.HoverWorkspacePreview(oldid, newid);
                            }
                        },
                        function () {
                            if ($selector.find("ul").css("display") == "block") {
                                var oldid = "#MainContent_workspace_" + $(this).attr("data-number");
                                var newid = "#MainContent_workspace_" + $("#workspace-selector").find(".workspace-menu-toggle > span").html();
                                if (oldid !== newid) {
                                    openWSE.HoverWorkspacePreview(oldid, newid);
                                }
                            }
                        }
                    );
                }

                $("#ddl_WorkspaceSelector").addClass("active");
                $("#ddl_WorkspaceSelector").parent().addClass("active");
            }
        }
    });
    $(document.body).on("click", ".workspace-selection-item", function () {
        var currentWorkspacenum = $("#workspace-selector").find(".workspace-menu-toggle > span").html();
        var workspacenum = $(this).attr("data-number");

        $(".workspace-selection-item").removeClass("selected");
        $(this).addClass("selected");

        if (currentWorkspacenum != workspacenum) {
            var id = "#MainContent_workspace_" + workspacenum;
            openWSE.SetWorkspaceNumber(workspacenum);

            if (!openWSE_Config.hoverPreviewWorkspace) {
                var oldid = "#MainContent_workspace_" + currentWorkspacenum;
                var newid = "#MainContent_workspace_" + workspacenum;
                openWSE.HoverWorkspacePreview(oldid, newid);
            }

            openWSE.AjaxCall(saveHandler + "/App_CurrentWorkspace", '{ "workspace": "' + workspacenum + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
        }

        CloseTopDropDowns();
    });
    function HoverWorkspacePreview(oldid, newid) {
        MoveOffScreen($(oldid));
        MoveOnScreen_WorkspaceOnly($(newid));

        if (!openWSE_Config.taskBarShowAll) {
            HideTasks($(oldid));
            ShowTasks($(newid));
        }

        $(oldid).css({
            opacity: 0.0,
            filter: "alpha(opacity=0)"
        });

        if (openWSE_Config.animationSpeed > 0) {
            $(newid).fadeTo(openWSE_Config.animationSpeed, 1.0);
        }
        else {
            $(newid).css({
                opacity: 1.0,
                filter: "alpha(opacity=100)"
            });
        }

        if (openWSE_Config.multipleBackgrounds) {
            SetCurrentWorkspaceBackground();
        }

        ResizeAllAppBody($(newid));
        openWSE.ToggleMinimizedAppBar();
    }
    function SetWorkspaceNumber(num) {
        $("#workspace-selector").find(".workspace-menu-toggle").html("<span>" + num + "</span>");
    }


    /* Workspace Auto Rotate */
    function AutoRotateWorkspace(interval, workspace, screens, autoRefresh) {
        setTimeout(function () {
            if (workspace == screens) {
                workspace = 1;
            }
            else {
                workspace++;
            }

            var id = "#MainContent_workspace_" + workspace;
            SetWorkspaceNumber(workspace);

            var isOn_fadeOut = false;
            var isOn_fadeIn = false;
            $('#workspace_holder .workspace-holder').each(function () {
                if ($(this).css("visibility") == "visible") {
                    if (!openWSE_Config.taskBarShowAll && openWSE.ConvertBitToBoolean($("#minimized_app_bar").attr("data-show"))) {
                        $("#minimized_app_bar").show();
                    }
                    $(this).fadeTo(openWSE_Config.animationSpeed, 0.0, function () {
                        if (!isOn_fadeOut) {
                            isOn_fadeOut = true;

                            // Move off screen
                            MoveOffScreen(this);

                            // Move onto screen
                            MoveOnScreen_WorkspaceOnly($(id));
                            if (!openWSE_Config.taskBarShowAll) {
                                HideTasks(this);
                                ShowTasks($(id));
                            }

                            $(id).find(".app-main-holder").each(function (index) {
                                if (autoRefresh) {
                                    AutoUpdateOnRotate(this);
                                }
                            });

                            if (openWSE_Config.multipleBackgrounds) {
                                SetCurrentWorkspaceBackground();
                            }

                            ResizeAllAppBody($(id));

                            $(id).fadeTo(openWSE_Config.animationSpeed, 1.0, function () {
                                if (!isOn_fadeIn) {
                                    isOn_fadeIn = true;
                                }
                            });

                            return false;
                        }
                    });
                }
            });

            AutoRotateWorkspace(interval, workspace, screens, autoRefresh);
        }, (interval * 1000));
    }
    function AutoUpdateOnRotate(_this) {
        var name = $(_this).find(".app-title").eq(0).text();
        var $_id = $(_this);
        var id = $_id.attr("data-appid");

        if ($_id.css("display") == "block") {
            ResizeAppBody($(".app-main-holder[data-appid='" + id + "']"));
            $(".app-main-holder").css("z-index", "1000");
            $_id.css("z-index", "3000");

            if ($_id.find(".iFrame-apps").length > 0) {
                var _content = $_id.find(".iFrame-apps");
                if (_content && _content != null && _content.src != null) {
                    _content.src = _content.src;
                }
            }
            else {
                $("#hf_ReloadApp").val(id);
                openWSE.CallDoPostBack('hf_ReloadApp', '');
            }
        }
    }

    function ToggleMinimizedAppBar() {
        if ($("#minimized_app_bar_holder").find(".app-min-bar").length === 0) {
            $("#minimized_app_bar").hide();
        }
        else {
            var foundDisplayedItem = false;
            var $appMinBar = $("#minimized_app_bar_holder").find(".app-min-bar");
            for (var i = 0; i < $appMinBar.length; i++) {
                if ($appMinBar.eq(i).css("display") !== "none") {
                    foundDisplayedItem = true;
                    break;
                }
            }

            if (foundDisplayedItem && openWSE.ConvertBitToBoolean($("#minimized_app_bar").attr("data-show"))) {
                $("#minimized_app_bar").show();
            }
            else {
                $("#minimized_app_bar").hide();
            }
        }
    }

    /* Build and Load App Functions */
    function AppsSortUnlocked(canSave) {
        canSortMyAppOverlay = true;
        if (canSave) {
            canSaveSort = true;
            canSaveSortedMyAppOverlay = true;
        }

        $('#updatePnl_AppList').sortable({
            cancel: '.app-icon-category-list, .app-popup',
            containment: '#updatePnl_AppList',
            scrollSensitivity: 40,
            scrollSpeed: 40,
            tolerance: "pointer",
            start: function (event, ui) {
                $(document).tooltip({ disabled: true });
                $('.app-icon').each(function () {
                    $(this).css("-moz-transition", "none");
                    $(this).css("-webkit-transition", "none");
                    $(this).css("transition", "none");
                });
            },
            stop: function (event, ui) {
                var listorder = '';
                $('.app-icon').each(function () {
                    $(this).css("-moz-transition", "");
                    $(this).css("-webkit-transition", "");
                    $(this).css("transition", "");

                    var temp = $(this).attr('data-appid');
                    if (temp != '' && listorder.indexOf(temp + ',') == - 1) {
                        listorder += (temp + ',');
                    }
                });

                if (canSaveSort) {
                    openWSE.AjaxCall(saveHandler + '/App_UpdateIcons', '{ "appList": "' + escape(listorder) + '" }');
                }

                if (openWSE_Config.showToolTips) {
                    $(document).tooltip({ disabled: false });
                }
            }
        });

        if ($(".app-icon.Icon_Only").length == 0 && $(".app-icon.Icon_And_Text_Only").length == 0 && $(".app-icon.Icon_And_Color_Only").length == 0 && $(".app-icon.Icon_Plus_Color_And_Text").length == 0) {
            $("#updatePnl_AppList").sortable("option", "axis", "y");
        }

        $('#updatePnl_AppList').disableSelection();
    }
    function CreateSOApp(id, title, content, x, y, width, height, min, max) {
        var $_id = $(".app-main-holder[data-appid='" + id + "']");
        ApplyStyle3HeightFix($_id);

        if ((content != null) && (content != "")) {
            content = unescape(content);
            $_id.find(".app-title").text(title);
            if (openWSE.ConvertBitToBoolean(max)) {
                $_id.addClass("app-maximized");
                $_id.find(".maximize-button-app").addClass("active");
            }

            if (width != "") {
                $_id.css("width", width);
            }
            if (height != "") {
                $_id.css("height", height);
            }

            if (openWSE.ConvertBitToBoolean(min)) {
                BuildAppMinIcon($_id, title, x, y);
                MoveOffScreen($_id);
            }
            else {
                $_id.css({
                    visibility: "visible",
                    display: "block",
                    zIndex: 3000
                });

                if (!openWSE.ConvertBitToBoolean(max)) {
                    $_id.removeClass("app-maximized");
                    $_id.find(".maximize-button-app").removeClass("active");
                    if (parseInt(y) < 0) {
                        y = "0";
                    }
                    if (parseInt(x) < 0) {
                        x = "0";
                    }
                    $_id.css({
                        left: x,
                        top: y
                    });
                }
            }

            if (!IsValidAscxFile(content)) {
                if (content.indexOf("ChatClient/ChatWindow.html") != -1) {
                    $_id.find(".app-body").html("<iframe class='iFrame-apps' src='" + openWSE.siteRoot() + content + "' width='100%' frameborder='0'></iframe>");
                    ResizeAppBody($_id);
                    $_id.find("iframe").one('load', (function () {
                        ResizeAppBody($_id);
                        $_id.find(".loading-background-holder").each(function () {
                            $(this).remove();
                        });
                    }));
                }
                else {
                    if (IsValidAspxFile(content) || IsValidHttpBasedAppType(content)) {
                        $_id.find(".app-body").html("<iframe class='iFrame-apps' src='" + content + "' width='100%' frameborder='0'></iframe>");
                        ResizeAppBody($_id);
                        $_id.find("iframe").one('load', (function () {
                            ResizeAppBody($_id);
                            $_id.find(".loading-background-holder").each(function () {
                                $(this).remove();
                            });
                        }));
                    }
                    else {
                        $_id.find(".app-body").load(content, function () {
                            LoadCSSFilesInApp($_id.attr("data-appid"));
                            if ($_id.find(".app-body").find("iframe").length > 0) {
                                if ($_id.find(".app-body").find(".loading-background-holder").length <= 0) {
                                    AppendLoadingMessage($_id.find(".app-body"));
                                }

                                ResizeAppBody($_id);
                                $_id.find(".app-body").find("iframe").one('load', (function () {
                                    ResizeAppBody($_id);
                                    $_id.find(".app-body").find(".loading-background-holder").each(function () {
                                        $(this).remove();
                                    });
                                }));
                            }
                        });
                    }
                }
            }
            else {
                LoadCSSFilesInApp($_id.attr("data-appid"));
            }

            SetAppIconActive($_id);
            var workspaceId = $_id.parent().attr("id");
            if ((workspaceId != undefined) && (workspaceId != null) && (workspaceId != "")) {
                AddworkspaceAppNum(workspaceId, id);
            }
        }
    }
    function BuildAppMinIcon(_this, title, x, y) {
        var $_id = $(_this);
        if ($_id.length > 0) {
            var id = $_id.attr("data-appid");
            var $imgsrc = $_id.find(".app-header-icon");
            if (($imgsrc.length === 0 || $imgsrc.hasClass("display-none")) && $(".app-icon[data-appid='" + id + "']").find("img").length > 0) {
                $imgsrc = $(".app-icon[data-appid='" + id + "']").find("img").eq(0);
            }

            var $titlesrc = $_id.find(".app-title").eq(0);
            if (($titlesrc.length === 0 || $titlesrc.hasClass("display-none")) && $(".app-icon[data-appid='" + id + "']").find(".app-icon-font").length > 0) {
                $titlesrc = $(".app-icon[data-appid='" + id + "']").find(".app-icon-font").eq(0);
            }

            var needIconOn = false;
            var needToolTip = false;

            var chatUsername = "";
            var classMinBar = "app-min-bar";
            if (id.replace(/#/gi, "").indexOf("app-ChatClient-") != -1) {
                classMinBar = "app-min-bar chat-modal";
                chatUsername = " chat-username='" + $_id.attr("chat-username") + "'";
            }

            var str = "<div data-appid='" + id + "' class='" + classMinBar + "'" + chatUsername + " data-x='" + x + "' data-y='" + y + "'>";

            if (((!$imgsrc.hasClass("display-none")) && ($imgsrc.length != 0)) && (!$titlesrc.hasClass("display-none"))) {
                str += $imgsrc.outerHTML();
                if ($imgsrc.css("display") == "none") {
                    needIconOn = true;
                }

                str += "<span class='app-title pad-right'>" + title + "</span>";
                str += "<a href='#" + id + "' class='exit-button-app-min'></a></div>";
            }
            else if ((($imgsrc.hasClass("display-none")) || ($imgsrc.length == 0)) && ($titlesrc.hasClass("display-none"))) {
                str += "<span class='app-title pad-right'>" + title + "</span>";
                str += "<a href='#" + id + "' class='exit-button-app-min'></a></div>";
            }
            else {
                var imgIsOn = false;
                var titleIsOn = true;
                if (!$imgsrc.hasClass("display-none")) {
                    str += $imgsrc.outerHTML();
                    imgIsOn = true;
                    if ($imgsrc.css("display") == "none") {
                        needIconOn = true;
                    }
                }

                if (!$titlesrc.hasClass("display-none")) {
                    str += "<span class='app-title pad-right'>" + title + "</span>";
                }
                else {
                    str += "<span class='app-title pad-right display-none'>" + title + "</span>";
                    needToolTip = true;
                    titleIsOn = false;
                }

                var marginLeft_iconOnly = "";
                if ((imgIsOn) && (!titleIsOn)) {
                    marginLeft_iconOnly = " style='margin-left: 5px;'";
                }

                str += "<a href='#" + id + "' class='exit-button-app-min'" + marginLeft_iconOnly + "></a></div>";
            }

            if ($(".app-min-bar[data-appid='" + id + "']").length == 0) {
                $("#minimized_app_bar_holder").append(str);
            }

            if (needIconOn) {
                $(".app-min-bar[data-appid='" + id + "']").find(".app-header-icon").show();
            }

            if (needToolTip) {
                $(".app-min-bar[data-appid='" + id + "']").attr("title", title);
            }

            var $appIcon = $(".app-icon[data-appid='" + id + "']");
            if ($appIcon.length > 0 && ($appIcon.hasClass("Color_And_Description") || $appIcon.hasClass("Icon_And_Color_Only") || $appIcon.hasClass("Icon_Plus_Color_And_Text"))) {
                var bgColor = $(".app-icon[data-appid='" + id + "']").css("background");
                $(".app-min-bar[data-appid='" + id + "']").css("background", bgColor);

                var $appTitle = $(".app-min-bar[data-appid='" + id + "']").find(".app-title").eq(0);
                if ($appTitle.length > 0) {
                    var fontColor = $appIcon.find(".app-icon-font").css("color");
                    $appTitle.css("color", fontColor);
                }
            }

            $_id.css({
                visibility: "hidden",
                display: "block"
            });
        }

        openWSE.ToggleMinimizedAppBar();
    }
    function LoadApp(_this, workspace) {
        if (_this.length && _this.length > 0) {
            _this = $(_this[0]);
        }

        var name = $(_this).find(".app-icon-font").text();
        if (name == "") {
            name = $(_this).find(".app-title").eq(0).text();
            if (name == "") {
                name = $(_this).find("span").text();
            }
        }

        var _appId = $(_this).attr("data-appid");
        if ((_appId != undefined) && (_appId != null) && (_appId != "")) {
            var $_id = $(".app-main-holder[data-appid='" + _appId + "']");
            ApplyStyle3HeightFix($_id);

            var content = $_id.attr("data-content");
            if (content == "" || content == null || content == undefined) {
                openWSE.AjaxCall(saveHandler + "/App_Open", '{ "appId": "' + _appId + '","name": "' + name + '","workspace": "' + workspace + '","width": "' + $_id.width() + '","height": "' + $_id.height() + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                    if (data.d != "") {
                        FinishContentLoad($_id, _appId, workspace, data.d, name);
                    }
                });
            }
            else {
                FinishContentLoad($_id, _appId, workspace, content, name);
                openWSE.AjaxCall(saveHandler + "/App_Open_NoContent", '{ "appId": "' + _appId + '","name": "' + name + '","workspace": "' + workspace + '","width": "' + $_id.width() + '","height": "' + $_id.height() + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
            }
        }

        openWSE.ToggleMinimizedAppBar();
    }
    function FinishContentLoad(_this, _appId, workspace, content, name) {
        if (_this.length && _this.length > 0) {
            _this = $(_this[0]);
        }

        var $_id = $(_this);
        if ($_id.length === 0) {
            $(".app-icon[data-appid='" + _appId + "']").removeClass("active");
            return;
        }

        SetActiveApp($_id);
        MoveToCurrworkspace(workspace, _appId);
        AddworkspaceAppNum(workspace, _appId);

        var appWidth = $_id.width();
        var appHeight = $_id.height();

        if ($_id.css("display") != "block" || $_id.css("visibility") != "visible" || previewHover) {
            var body = "";
            if ($_id.find(".app-body").find("div").html() == null) {
                body = $.trim($_id.find(".app-body").html());
            }
            else {
                body = $.trim($_id.find(".app-body").find("div").html());
            }

            if (body == "") {
                if (_appId.indexOf("app-ChatClient-") != -1) {
                    var chatUser = $_id.attr("chat-username");
                    content = "ChatClient/ChatWindow.html?user=" + chatUser + "&displayVersion=workspace";
                }

                if ((($_id.css("left") == null) && ($_id.css("top") == null)) || (($_id.css("left") == "auto") && ($_id.css("top") == "auto"))) {
                    CreateSOApp(_appId, name, content, "50px", "50px", appWidth, appHeight, "1", "0");
                }
                else {
                    if (parseInt($_id.css("top")) < 0) {
                        $_id.css("top", "50px");
                    }
                    if (parseInt($_id.css("left")) < 0) {
                        $_id.css("left", "50px");
                    }
                    CreateSOApp(_appId, name, content, $_id.css("left"), $_id.css("top"), appWidth, appHeight, "1", "0");
                }
            }

            $_id.css("display", "block");
            $_id.css("visibility", "visible");
            $_id.css("z-index", "3000");

            if ($(".app-min-bar[data-appid='" + _appId + "']").length != 0) {
                if ($_id.find(".loading-background-holder").length <= 0 && body == "") {
                    AppendLoadingMessage($_id.find(".app-body"));
                }
                if ((!$_id.hasClass("auto-full-page")) && (!$_id.hasClass("auto-full-page-min")) && (!$_id.hasClass("app-maximized")) && (!$_id.hasClass("app-maximized-min"))) {
                    $_id.find(".maximize-button-app").removeClass("active");
                    $_id.css("width", appWidth);
                    $_id.css("height", appHeight);
                    $_id.css("top", topBarHt);
                }
                else {
                    SetAppMinToMax($_id);
                    $_id.find(".maximize-button-app").addClass("active");
                    $_id.css("top", "0px");
                }

                var xData = $(".app-min-bar[data-appid='" + _appId + "']").attr("data-x");
                var yData = $(".app-min-bar[data-appid='" + _appId + "']").attr("data-y");


                if (previewHover && previewAppID == _appId) {
                    $_id.css({
                        visibility: "visible",
                        display: "block",
                        opacity: 1.0,
                        left: xData,
                        top: yData
                    });
                }
                else {
                    $_id.css({
                        visibility: "visible",
                        display: "block"
                    }).animate({
                        opacity: 1.0,
                        filter: "alpha(opacity=100)",
                        left: xData,
                        top: yData
                    }, openWSE_Config.animationSpeed);
                }

                openWSE.AjaxCall(saveHandler + "/App_Move", '{ "appId": "' + _appId + '","name": "' + name + '","x": "' + xData + '","y": "' + yData + '","width": "' + appWidth + '","height": "' + appHeight + '","workspace": "' + workspace + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
                ResizeAllAppBody($_id);
            }
            else {
                $_id.css({
                    top: topBarHt,
                    visibility: "visible",
                    display: "block"
                }).fadeIn(openWSE_Config.animationSpeed);
            }

            $(".app-min-bar[data-appid='" + _appId + "']").remove();

            previewHover = false;
            previewAppID = "";

            if (needpostback == 1) {
                var hf_loadApp1 = document.getElementById("hf_loadApp1");
                if (appsToLoad.length == 0) {
                    appsToLoad[0] = _appId;
                    hf_loadApp1.value = _appId;
                    openWSE.CallDoPostBack("hf_loadApp1", "");
                }
                else {
                    appsToLoad[appsToLoad.length - 1] = _appId;
                }
            }
            else if ((IsValidAscxFile(content)) && (needpostback == 0)) {
                $_id.find(".loading-background-holder").each(function () {
                    $(this).remove();
                });
            }
        }

        openWSE.ToggleMinimizedAppBar();
    }

    function ApplyStyle3HeightFix(ele) {
        var $_id = $(ele);
        if ($_id.length > 0) {
            if ($_id.hasClass("app-main-style3") && !$_id.hasClass("style3-resize-applied") && $_id.find(".app-head").length > 0) {
                var cssHeight = window.getComputedStyle($_id.find(".app-head")[0], null).getPropertyValue("height");
                if (cssHeight) {
                    var tempHt = parseInt(cssHeight.replace("px", "").replace("em", ""));
                    if (tempHt) {
                        var currminHt = parseInt($_id.css("min-height").replace("px", "").replace("em", ""));
                        if (currminHt) {
                            $_id.css("min-height", currminHt - tempHt);
                        }
                        var currHt = parseInt($_id.css("height").replace("px", "").replace("em", ""));
                        if (currHt) {
                            $_id.css("height", currHt - tempHt);
                        }
                        var currappBodyHt = parseInt($_id.find(".app-body").css("height").replace("px", "").replace("em", ""));
                        if (currappBodyHt) {
                            $_id.find(".app-body").css("height", currappBodyHt - tempHt);
                        }
                    }
                }

                $_id.addClass("style3-resize-applied");
            }
        }
    }

    function LoadAppFromSiteTools(appId, name, workspace) {
        if ((appId != undefined) && (appId != null) && (appId != "")) {
            $('#ConfirmApp-element').remove();

            loadingPopup.Message("Loading. Please Wait...");
            cookieFunctions.set("active_app", appId, 30, function () {

                var appWidth = $(".app-main-holder[data-appid='" + appId + "']").width();
                var appHeight = $(".app-main-holder[data-appid='" + appId + "']").height();

                openWSE.AjaxCall(saveHandler + "/App_Open_ChangeWorkspace", '{ "appId": "' + appId + '","name": "' + name + '","workspace": "' + workspace + '","width": "' + appWidth + '","height": "' + appHeight + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                    window.location = openWSE.siteRoot() + "Default.aspx";
                });
            });
        }
    }
    function DetermineNeedPostBack(_this, npb) {
        if ($(".app-main-holder[data-appid='" + $(_this).attr("data-appid") + "']").css('display') == 'none') {
            needpostback = npb;
        }
    }
    function WatchForLoad(app) {
        var $app = $(".app-main-holder[data-appid='" + app + "']");
        $app.find("iframe").one('load', (function () {
            $app.find(".loading-background-holder").each(function () {
                $(this).remove();
            });
        }));
    }
    function LoadUserControl(id) {
        var hf_loadApp1 = document.getElementById("hf_loadApp1");
        for (var i = 0; i < appsToLoad.length; i++) {
            if (appsToLoad[i] == id) {
                appsToLoad.splice(i, 1);
            }
        }

        var a = "#MainContent_" + id.replace(/-/gi, "_") + "_advPanel";
        $(a).html("");
        if ($('.move-holder').find('.main-div-app-bg').length > 0) {
            $(a).hide().append($('.move-holder').find('.main-div-app-bg')).show();
        }

        var $app = $(".app-main-holder[data-appid='" + id + "']");

        var $workspace = $(a).closest(".app-body").parent().parent();
        if ($workspace.length > 0) {
            LoadCSSFilesInApp(id);

            if ($app.length > 0) {
                $app.find('.outside-main-app-div').each(function () {
                    $(this).addClass(id + outsideAppModal);
                    $workspace.append($(this));
                });
            }

            if ($('.move-holder').find('.outside-main-app-div').length > 0) {
                $('.move-holder').find('.outside-main-app-div').each(function () {
                    if ($app.length == 1) {
                        $(this).addClass(id + outsideAppModal);
                        $workspace.append($(this));
                    }
                });
            }
        }

        ResizeAppBody($app);

        $app.find(".loading-background-holder").each(function () {
            $(this).remove();
        });

        if (appsToLoad.length > 0) {
            hf_loadApp1.value = appsToLoad[0];
            openWSE.CallDoPostBack("hf_loadApp1", "");
        }

        needpostback = 0;
    }
    function MoveOutsideModalWindows() {
        $('.outside-main-app-div').each(function () {
            var $workspace = $(this).closest(".app-body").parent().parent();
            if ($workspace.length > 0 && $("#" + $(this).attr("id")).length == 1) {
                var modalClass = $(this).closest(".app-body").parent().attr("data-appid");
                $(this).addClass(modalClass + outsideAppModal);
                $workspace.append($(this));
            }
        });
    }
    function MoveOutSideModalWindowToWorkspace(appId) {
        var $workspace = $(".app-main-holder[data-appid='" + appId + "']").parent();
        $("." + appId + outsideAppModal).each(function () {
            if ($workspace.length > 0 && $("#" + $(this).attr("id")).length == 1) {
                $workspace.append($(this));
            }
        });
    }


    function LoadCSSFilesInApp(id) {
        var $app = $(".app-main-holder[data-appid='" + id + "']");
        if ($app.length === 0) {
            $app = $(".workspace-overlay-selector[data-overlayid='" + id + "']");
        }

        if ($app.length > 0) {
            var headElement = document.head || document.getElementsByTagName("head")[0];

            $app.find("link").each(function () {
                var href = $.trim($(this).attr("href"));
                if (href) {
                    if ($("link[data-id='" + id + "'][href='" + href + "']").length === 0) {
                        href = AppendTimestampToUrl(href);

                        if (href.indexOf("~/") == 0) {
                            href = openWSE.siteRoot() + href.replace("~/", "");
                            $(this).attr("href", href);
                        }
                        else if (href.indexOf("App/") == 0) {
                            href = openWSE.siteRoot() + href;
                            $(this).attr("href", href);
                        }

                        $(this).attr("data-id", id);
                        $(document.head).append($(this));
                    }
                }
                else {
                    if ($("link[data-id='" + id + "']").length === 0) {
                        $(this).attr("data-id", id);
                        $(document.head).append($(this));
                    }
                }
            });
            $app.find("style").each(function () {
                if ($("style[data-id='" + id + "']").length === 0) {
                    $(this).attr("data-id", id);
                    $(document.head).append($(this));
                }
            });
            $app.find("script").each(function () {
                var src = $.trim($(this).attr("src"));
                if (src) {
                    if ($("script[data-id='" + id + "'][src='" + src + "']").length === 0) {
                        src = AppendTimestampToUrl(src);

                        if (src.indexOf("~/") == 0) {
                            src = openWSE.siteRoot() + src.replace("~/", "");
                        }
                        else if (src.indexOf("App/") == 0) {
                            src = openWSE.siteRoot() + src;
                        }

                        var scriptElement = document.createElement("script");
                        scriptElement.src = src;
                        scriptElement.type = $(this).attr("type");
                        scriptElement.setAttribute("data-id", id);
                        scriptElement.async = false;

                        $(this).remove();

                        headElement.insertBefore(scriptElement, headElement.firstChild);
                    }
                }
                else {
                    if ($("script[data-id='" + id + "']").length === 0) {
                        $(this).attr("data-id", id);
                        $(document.head).append($(this));
                    }
                }
            });
            $app.find("input[data-scriptelement='true']").each(function () {
                var tagName = $(this).attr("data-tagname");
                if (tagName) {
                    tagName = $.trim(tagName).toLowerCase();

                    var tagType = $(this).attr("data-tagtype");
                    if (tagType) {
                        tagType = $.trim(tagType);
                    }

                    var tagRel = $(this).attr("data-tagrel");
                    if (tagRel) {
                        tagRel = $.trim(tagRel);
                    }

                    var tagSrc = $(this).attr("data-tagsrc");
                    if (tagSrc) {
                        tagSrc = $.trim(tagSrc);
                        tagSrc = AppendTimestampToUrl(tagSrc);

                        if (tagSrc.indexOf("~/") == 0) {
                            tagSrc = openWSE.siteRoot() + tagSrc.replace("~/", "");
                        }
                        else if (tagSrc.indexOf("App/") == 0) {
                            tagSrc = openWSE.siteRoot() + tagSrc;
                        }
                    }

                    var element = document.createElement(tagName);
                    if (element) {
                        element.setAttribute("data-id", id);
                        element.type = tagType;
                        if (tagRel) {
                            element.rel = tagRel;
                        }

                        switch (tagName) {
                            case "script":
                                if ($("script[data-id='" + id + "'][src='" + tagSrc + "']").length === 0) {
                                    element.src = tagSrc;
                                    var tagAsync = $(this).attr("data-tagasync");
                                    if (tagAsync) {
                                        element.async = $.trim(tagAsync) == "true";
                                    }
                                    else {
                                        element.async = false;
                                    }
                                    headElement.insertBefore(element, headElement.firstChild);
                                }
                                break;

                            case "link":
                                if ($("link[data-id='" + id + "'][href='" + tagSrc + "']").length === 0) {
                                    element.href = tagSrc;
                                    headElement.insertBefore(element, headElement.firstChild);
                                }
                                break;
                        }
                    }
                }
            });
        }
    }
    function RemoveCSSFilesOnAppClose(id) {
        if ($("link[data-id='" + id + "']").length > 0) {
            $("link[data-id='" + id + "']").remove();
        }
        if ($("style[data-id='" + id + "']").length > 0) {
            $("style[data-id='" + id + "']").remove();
        }
        if ($("script[data-id='" + id + "']").length > 0) {
            $("script[data-id='" + id + "']").remove();
        }
    }
    function AppendTimestampToUrl(url) {
        if (openWSE_Config.appendTimestampOnScripts) {
            var timeStamp = Math.floor(Date.now() / 1000);
            if (url.indexOf("?") > 0) {
                url += "&" + openWSE_Config.timestampQuery + timeStamp
            }
            else {
                url += "?" + openWSE_Config.timestampQuery + timeStamp
            }
        }

        return url;
    }


    function IsValidAspxFile(filename) {
        filename = filename.toLowerCase();
        if (filename.length > 5 && filename.substring(filename.length - 5) == ".aspx") {
            return true;
        }

        return false;
    }
    function IsValidAscxFile(filename) {
        filename = filename.toLowerCase();
        if (filename.length > 5 && filename.substring(filename.length - 5) == ".ascx") {
            return true;
        }
        else if (filename.length > 5 && filename.substring(filename.length - 4) == ".dll") {
            return true;
        }

        return false;
    }
    function IsValidHttpBasedAppType(filename) {
        filename = filename.toLowerCase();
        if (filename.indexOf("//") === 0 || filename.indexOf("http://") != -1 || filename.indexOf("https://") != -1 || filename.indexOf("www.") != -1) {
            return true;
        }

        return false;
    }

    /* Fixes, Customizations, and Position */
    function ResizeAllAppBody(_this) {
        $(_this).find(".app-main-holder").each(function (index) {
            ResizeAppBody(this);
        });
    }
    function ResizeAppBody(ele) {
        var $this = $(ele);
        if ($this.length > 0 && $this.hasClass("app-main-holder")) {
            var appHt = $this.outerHeight();

            var $appHead = $this.find(".app-head");
            var headerHt = 0;
            if ($appHead.length > 0 && $appHead.css("display") != "none") {
                headerHt = $appHead.outerHeight();
            }

            var borders = 0;
            if ($this.css("border") != "") {
                var borderWt = $this.css("border").split("px")[0];
                borderWt = parseInt(borderWt);
                if (borderWt && borderWt.toString() != "NaN") {
                    borders = (borderWt * 2);
                }
            }
            else {
                if ($this.css("border-top") != "") {
                    var borderWt = $this.css("border-top").split("px")[0];
                    borderWt = parseInt(borderWt);
                    if (borderWt && borderWt.toString() != "NaN") {
                        borders = borderWt;
                    }
                }
                if ($this.css("border-bottom") != "") {
                    var borderWt = $this.css("border-bottom").split("px")[0];
                    borderWt = parseInt(borderWt);
                    if (borderWt && borderWt.toString() != "NaN") {
                        borders += borderWt;
                    }
                }
            }

            var bodyHt = appHt - (headerHt + borders);
            $this.find(".app-body").css({
                height: bodyHt,
                width: $this.width()
            });

            if ($this.find(".iFrame-apps").length > 0) {
                var adjustmentHt = 0;
                if ($this.find(".app-title-bg-color").length > 0) {
                    adjustmentHt = $this.find(".app-title-bg-color").outerHeight();
                }

                $this.find(".iFrame-apps").css("height", bodyHt - adjustmentHt);
            }
        }
    }
    function ApplyOverlayFix(_this) {
        var $_id = $(_this);
        if ($_id.length > 0) {
            $wo = $_id.find(".app-overlay-fix");
            if ($wo.length == 0) {
                $wb = $_id.find(".app-body");
                if ($wb.length == 1) {
                    $wb.append("<div class='app-overlay-fix'></div>");
                }
            }
        }
    }
    function RemoveOverlayFix(_this) {
        $wo = $(_this).find(".app-overlay-fix");
        if ($wo.length == 1) {
            $wo.remove();
        }
    }
    function MoveOffScreen(_this) {
        var $_id = $(_this);

        var appHt = $_id.height();
        var bottomPos = $(window).height();
        var topPos = -(appHt + bottomPos);
        $_id.css({
            visibility: "hidden",
            top: topPos,
            bottom: bottomPos,
            zIndex: -1
        });

        SetAppMaxToMin(_this);
    }
    function MoveOnScreen_WorkspaceOnly(_this) {
        var $_id = $(_this);
        $_id.css({
            visibility: "visible",
            top: 0,
            bottom: 0,
            zIndex: ""
        });
        SetAppMinToMax($_id);
    }
    function MoveToCurrworkspace(workspace, app) {
        var $app = $(".app-main-holder[data-appid='" + app + "']");
        SetAppMinToMax($app);
        var currentworkspace = $app.parent().attr("id");
        var newworkspace = 'MainContent_' + workspace;
        if (currentworkspace != newworkspace) {
            var loadscreen = $app.find("iframe").length;
            if (loadscreen > 0) {
                if ($app.find(".loading-background-holder").length <= 0) {
                    AppendLoadingMessage($app.find(".app-body"));
                }

                WatchForLoad(app);
            }
            $('#' + newworkspace).prepend($app);
            AddworkspaceAppNum(workspace, app);
            MoveOutSideModalWindowToWorkspace(app);
            ResizeAppBody($app);
        }
    }
    function SetAppMaxToMin(_this) {
        var $app = $(_this);
        if ($app.hasClass("app-maximized")) {
            $app.removeClass("app-maximized");
            $app.addClass("app-maximized-min");
        }

        if ($app.hasClass("auto-full-page")) {
            $app.removeClass("auto-full-page");
            $app.addClass("auto-full-page-min");
        }
    }
    function SetAppMinToMax(_this) {
        var $app = $(_this);
        if ($app.hasClass("app-maximized-min")) {
            $app.removeClass("app-maximized-min");
            $app.addClass("app-maximized");
        }

        if ($app.hasClass("auto-full-page-min")) {
            $app.removeClass("auto-full-page-min");
            $app.addClass("auto-full-page");
        }

        ResizeAppBody(_this);
    }
    function HoverOverAppMin(_this) {
        if (!previewHover) {
            if ($(".app-min-bar[data-appid='" + $(_this).attr('data-appid') + "']").length > 0) {
                previewHover = true;
                previewAppID = $(_this).attr('data-appid');
                var workspace = Getworkspace();
                var $this = $('#MainContent_' + workspace).find(".app-main-holder[data-appid='" + previewAppID + "']");
                if ($this.length > 0) {
                    var xVal = $(".app-min-bar[data-appid='" + previewAppID + "']").attr("data-x");
                    var yVal = $(".app-min-bar[data-appid='" + previewAppID + "']").attr("data-y");

                    if (xVal == null || xVal == "") {
                        xVal = 0;
                    }
                    if (yVal == null || yVal == "") {
                        yVal = 0;
                    }

                    SetAppMinToMax($this);
                    $this.css('left', xVal);
                    $this.css('top', yVal);
                    $this.addClass('app-min-bar-preview');
                    ResizeAppBody($this);
                    $this.fadeTo(openWSE_Config.animationSpeed, 0.65);
                }
            }
        }
    }
    function HoverOutAppMin() {
        var $this = $(".app-main-holder[data-appid='" + previewAppID + "']");
        if ($this.hasClass('app-min-bar-preview')) {
            $this.css("opacity", "0.0");
            $this.css("filter", "alpha(opacity=0)");
            $this.removeClass('app-min-bar-preview');
            SetAppMaxToMin($this);
            MoveOffScreen($this);
            previewAppID = '';
            previewHover = false;
        }
        else {
            var workspace = Getworkspace();
            var $this = $('#MainContent_' + workspace).find(".app-main-holder[data-appid='" + previewAppID + "']");
            if ($this.length == 0) {
                previewAppID = '';
                previewHover = false;
            }
        }

    }
    function SetActiveApp(_this) {
        if (openWSE.IsComplexWorkspaceMode()) {
            var $_id = $(_this);

            SetDeactiveApps($_id);

            if ($_id.length > 0) {
                $_id.addClass("selected");
                $_id.css("z-index", "3000");
                var id = $_id.attr("data-appid");
                cookieFunctions.set("active_app", id, "30");
            }
        }
    }
    function SetDeactiveApps(_this) {
        if (openWSE.IsComplexWorkspaceMode()) {
            var id = $(_this).attr("data-appid");
            $(".app-main-holder").css("z-index", "1000");
            $(".app-main-holder").removeClass("selected");
            $(".app-main-holder").each(function (index) {
                var tempId = $(this).attr("data-appid");
                if ($(this).css("display") == "block") {
                    if (tempId != id) {
                        ApplyOverlayFix(this);
                    }
                    else if (tempId == id) {
                        $(this).find(".app-overlay-fix").remove();
                    }
                }
            });
        }
    }
    function SetDeactiveAll() {
        cookieFunctions.del("active_app");
        $(".app-main-holder").each(function (index) {
            if ($(this).css("display") == "block") {
                ApplyOverlayFix(this);
            }
        });
    }


    /* Load App Cookies */
    function LoadActiveAppCookie() {
        if (openWSE.IsComplexWorkspaceMode()) {
            cookieFunctions.get('active_app', function (id) {
                if ((id != null) && (id != "") && (id != undefined)) {
                    SetActiveApp($(".app-main-holder[data-appid='" + id + "']"));
                }
                else {
                    SetDeactiveAll();
                }
            });
        }
    }


    /* App Remote Load Functions */
    function StartRemoteLoad(id, hf_r, handler, updateAppId) {
        openWSE.AjaxCall(saveHandler + "/App_RemoteLoad", '{ "_Id": "' + escape(id) + '" }', null, function (data) {
            try {
                if (data.d != null) {
                    LoadRemotely(unescape(data.d[0]), unescape(data.d[1]), unescape(data.d[2]));
                }
            }
            catch (evt) { }
            loadingPopup.RemoveMessage();
            autoupdate(hf_r, handler, updateAppId);
        }, function (data) {
            loadingPopup.RemoveMessage();
            autoupdate(hf_r, handler, updateAppId);
        });
    }
    function LoadRemotely(appId, options, npb) {
        if (options == "close") {
            var $this = $(".app-main-holder[data-appid='" + appId + "']");
            if ($this.css("display") == "block") {
                if ($this.find(".exit-button-app").length > 0) {
                    exitBtn_InProgress = false;
                    $this.find(".exit-button-app").click();
                }
            }
        }
        if (options == "close-all") {
            $(".app-main-holder").each(function () {
                if ($(this).css("display") == "block") {
                    if ($(this).find(".exit-button-app").length > 0) {
                        exitBtn_InProgress = false;
                        $(this).find(".exit-button-app").click();
                    }
                }
            });
        }
        else if (appId == "workspace-selector") {
            try {
                $(".workspace-selection-item").eq(parseInt(options) - 1).trigger("click");
            }
            catch (evt) { }
            var oldid = "#MainContent_" + Getworkspace();
            var newid = "#MainContent_workspace_" + options;
            SetWorkspaceNumber(options);
            HoverWorkspacePreview(oldid, newid);
        }
        else {
            var optArray = options.split(";");
            var workspace = "1";
            if (optArray[0] != "" && optArray[0] != "0") {
                workspace = optArray[0];
            }
            else {
                workspace = Getworkspace().replace("workspace_", "");
            }

            if (openWSE.ConvertBitToBoolean(npb)) {
                needpostback = 1;
            }
            else {
                needpostback = 0;
            }

            if (appId.indexOf("app-ChatClient-") != -1) {
                var userId = appId.replace("app-ChatClient-", "");
                for (var j = 0; j < $(".ChatUserNotSelected").length; j++) {
                    var $userSelect = $(".ChatUserNotSelected").eq(j);
                    if ($userSelect.length == 1 && $userSelect.find(".usersclick[chat-userid='" + userId + "']").length == 1) {
                        var fullName = $.trim($userSelect.find(".usersclick[chat-userid='" + userId + "']").html());
                        var user = $userSelect.attr("chat-username");
                        chatClient.BuildChatWindow(user, userId, fullName);
                        return;
                    }
                }
            }

            var $this = $(".app-icon[data-appid='" + appId + "']");

            if ($this.length == 0) {
                return;
            }

            var _posY = optArray[2].replace("px", "");
            var _posX = optArray[3].replace("px", "");
            var _width = optArray[4].replace("px", "");
            var _height = optArray[5].replace("px", "");

            if ($("#hf_appContainer").val() != "") {
                var _posY = parseInt(_posY);
                var _posX = parseInt(_posX);
                var _width = parseInt(_width);
                var _height = parseInt(_height);

                if (_posX + _width > $("#main_container").outerWidth()) {
                    _posX = Math.abs($("#main_container").outerWidth() - _width) - 2;
                }

                if (_posY + _height > $("#main_container").outerHeight()) {
                    _posY = Math.abs($("#main_container").outerHeight() - _height) - 2;
                }
            }

            if ($(".app-main-holder[data-appid='" + appId + "']").css("display") == "block") {
                $(".app-main-holder[data-appid='" + appId + "']").css({
                    top: _posY + "px",
                    left: _posX + "px"
                });
            }
            else {
                $(".app-main-holder[data-appid='" + appId + "']").css({
                    top: "auto",
                    left: "auto"
                });
            }

            if ($(".app-main-holder[data-appid='" + appId + "']").hasClass("ui-resizable")) {
                $(".app-main-holder[data-appid='" + appId + "']").css({
                    width: _width + "px",
                    height: _height + "px"
                });
            }

            if ($(".app-main-holder[data-appid='" + appId + "']").css("display") == "block") {
                var _this = $(".app-main-holder[data-appid='" + appId + "']").find(".reload-button-app").parent();
                ReloadApp(_this);
            }

            LoadApp($this, "workspace_" + workspace);
            if ($this.hasClass("active") == false) {
                $this.addClass("active");
            }

            SetRemoteLoadingOptions(appId, workspace, optArray[1]);
        }
    }
    function SetRemoteLoadingOptions(appId, option1, option2) {
        var $this = $(".app-main-holder[data-appid='" + appId + "']");
        if ($this.css("display") == "block") {
            option1 = option1.substring(option1.lastIndexOf("_") + 1);

            try {
                $(".workspace-selection-item").eq(parseInt(option1) - 1).trigger("click");
            }
            catch (evt) { }

            var oldid = "#MainContent_" + Getworkspace();
            var newid = "#MainContent_workspace_" + option1;
            SetWorkspaceNumber(option1);
            HoverWorkspacePreview(oldid, newid);

            MoveAppToworkspace(option1);

            if (option2 != "") {
                var propSaved = false;
                switch (option2) {
                    case "minimize":
                        if ($(".app-min-bar[data-appid='" + appId + "']").length == 0) {
                            if ($this.find(".minimize-button-app").length > 0) {
                                $this.find(".minimize-button-app").click();
                                propSaved = true;
                            }
                        }
                        else {
                            var minAgain = setInterval(function () {
                                if ($this.css("visibility") == "visible") {
                                    $this.find(".minimize-button-app").click();
                                    propSaved = true;
                                    clearInterval(minAgain);
                                }
                            }, 50);
                        }
                        break;
                    case "maximize":
                        if (($this.find(".maximize-button-app").length > 0) && (!$this.hasClass("app-maximized"))) {
                            $this.find(".maximize-button-app").click();
                            propSaved = true;
                        }
                        break;
                    case "normal":
                        if ($this.hasClass("app-maximized")) {
                            if ($this.find(".maximize-button-app").length > 0) {
                                $this.find(".maximize-button-app").click();
                                propSaved = true;
                            }
                        }
                        break;
                }

                if (!propSaved) {
                    setTimeout(function () {
                        openWSE.AjaxCall(saveHandler + "/App_Position", '{ "appId": "' + $this.attr("data-appid") + '","posX": "' + $this.css("left") + '","posY": "' + $this.css("top") + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
                    }, openWSE_Config.animationSpeed);
                }
            }
        }
        else {
            setTimeout(function () {
                SetRemoteLoadingOptions(appId, option1, option2);
            }, 100);
        }
    }


    /* App Functions */
    function MaximizeApp(_this) {
        var workspace = Getworkspace();

        var $_id = $(_this);

        var name = $_id.find(".app-title").eq(0).text();
        var _leftPos = $_id.css("left").replace("px", "");
        var _topPos = $_id.css("top").replace("px", "");
        var _width = $_id.width();
        var _height = $_id.height();

        SetActiveApp(_this);

        if ($_id.hasClass("app-maximized")) {
            $_id.removeClass("app-maximized");
            ResizeAppBody(_this);

            $_id.find(".maximize-button-app").removeClass("active");
            openWSE.AjaxCall(saveHandler + "/App_Maximize", '{ "appId": "' + $_id.attr("data-appid") + '","name": "' + name + '","x": "' + _leftPos + '","y": "' + _topPos + '","width": "' + _width + '","height": "' + _height + '","workspace": "' + workspace + '","ismax": "0","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                maxBtn_InProgress = false;
                // ResizeAppBody(_this);

                openWSE.SetLeftSidebarScrollTop();
                $(window).resize();
            }, function (data) {
                maxBtn_InProgress = false;
            });
        }
        else {
            $_id.addClass("app-maximized");
            ResizeAppBody(_this);

            $_id.find(".maximize-button-app").addClass("active");
            openWSE.AjaxCall(saveHandler + "/App_Maximize", '{ "appId": "' + $_id.attr("data-appid") + '","name": "' + name + '","x": "' + _leftPos + '","y": "' + _topPos + '","width": "' + _width + '","height": "' + _height + '","workspace": "' + workspace + '","ismax": "1","workspaceMode": "' + openWSE_Config.workspaceMode + '" }', null, function (data) {
                maxBtn_InProgress = false;
                // ResizeAppBody(_this);

                openWSE.SetLeftSidebarScrollTop();
                $(window).resize();
            }, function (data) {
                maxBtn_InProgress = false;
            });
        }
    }
    function SetAppIconActive(_this) {
        var id = $(_this).attr("data-appid");
        if ((id != null) && (id != undefined)) {
            if (id.indexOf("app-ChatClient-") == -1) {
                var spanactive = $(".app-icon").find("span").length;
                for (var index = 0; index < spanactive; index++) {
                    var temp = $(".app-icon").eq(index).attr("data-appid");
                    if (temp == id) {
                        if ($(".app-icon").eq(index).hasClass("active") == false) {
                            $(".app-icon").eq(index).addClass("active");
                        }
                    }
                }
            }
            else {
                var chatUserName = $(".app-main-holder[data-appid='" + id + "']").attr("chat-username");
                if ($(".ChatUserNotSelected[chat-username='" + chatUserName + "']").length > 0) {
                    $(".ChatUserNotSelected[chat-username='" + chatUserName + "']").addClass("ChatUserSelected");
                }
                else {
                    $(".ChatUserNotSelected[chat-username='" + chatUserName.toLowerCase() + "']").addClass("ChatUserSelected");
                }
            }
        }
    }
    function AddworkspaceAppNum(workspace, app) {
        if (openWSE_Config.ShowWorkspaceNumApp && openWSE.IsComplexWorkspaceMode()) {
            var $appIcon = $(".app-icon[data-appid='" + app + "']");
            if ($appIcon.length > 0) {
                var numberworkspace = "";
                var ndID = app + "-workspace-reminder";
                var ndClasses = "workspace-reminder font-no-bold " + ndID;
                var style = "display:block;";

                if ($appIcon.find(".app-icon-font").length > 0 && ($appIcon.hasClass("Icon_And_Color_Only") || $appIcon.hasClass("Color_And_Description") || $appIcon.hasClass("Icon_Plus_Color_And_Text"))) {
                    var ftColor = $appIcon.find(".app-icon-font").css("color");
                    style += "color:" + ftColor + "!important;";
                }

                if ($("." + ndID).length > 0) {
                    $("." + ndID).remove();
                }

                var dn = workspace.substring(workspace.lastIndexOf("_") + 1);
                numberworkspace = "<span class='" + ndClasses + "' style='" + style + "'>" + dn + "</span>";

                if (numberworkspace != "") {
                    $appIcon.append(numberworkspace);
                }
            }
        }
    }
    function RemoveworkspaceAppNum(_this) {
        if (openWSE.IsComplexWorkspaceMode()) {
            var id = $(_this).attr("data-appid");
            var $appIcon = $(".app-icon[data-appid='" + id + "']");
            if ($appIcon.length > 0) {
                var ndID = id + "-workspace-reminder";
                if ($("." + ndID).length > 0) {
                    $("." + ndID).remove();
                }
            }
        }
    }
    function RemoveAppIconActive(_this) {
        var id = $(_this).attr("data-appid")
        $(".app-main-holder[data-appid='" + id + "']").find(".maximize-button-app").removeClass("active");
        if (id.indexOf("app-ChatClient-") == -1) {
            var spanactive = $(".app-icon").find("span").length;
            for (var index = 0; index < spanactive; index++) {
                var temp = $(".app-icon").eq(index).attr("data-appid");
                if (temp == id) {
                    if ($(".app-icon").eq(index).hasClass("active") == true) {
                        $(".app-icon").eq(index).removeClass("active");
                    }
                }
            }
        }
        else {
            var chatUserName = $(".app-main-holder[data-appid='" + id + "']").attr("chat-username");
            chatClient.displayMessageNoti(chatUserName);

            var $userIcon = $(".ChatUserNotSelected[chat-username='" + chatUserName + "']");
            if ($userIcon.length > 0) {
                setTimeout(function () {
                    $userIcon.removeClass("ChatUserSelected");
                    $userIcon.removeClass("chatisNew");
                }, openWSE_Config.animationSpeed);
            }

            chatClient.notificationCleared = 1;
            document.title = chatClient.currTitle;
        }
    }
    function CategoryClick(id, category) {
        CollapseAllAdminLinks();

        var $categoryHolder = $("." + id + "-category-holder");
        if ($categoryHolder.length > 0) {
            if (!$categoryHolder.hasClass("show-category")) {
                CollapseAllAppCategories();
                $(".app-icon-category-list[data-appid='" + id + "']").addClass("show-category");
                $categoryHolder.addClass("show-category");
                $categoryHolder.slideDown(openWSE_Config.animationSpeed, function () {
                    $categoryHolder.removeClass("hide-category");
                });
            }
            else {
                $(".app-icon-category-list[data-appid='" + id + "']").removeClass("show-category");
                $categoryHolder.removeClass("show-category");
                $categoryHolder.slideUp(openWSE_Config.animationSpeed, function () {
                    $categoryHolder.addClass("hide-category");
                });
            }

            var categoriesExpanded = "";
            $(".app-icon-category-list.show-category").each(function () {
                categoriesExpanded += $(this).attr("data-appid") + "~";
            });

            if (!categoriesExpanded) {
                cookieFunctions.del("categories-expanded");
            }
            else {
                cookieFunctions.set("categories-expanded", categoriesExpanded, 30);
            }
        }
    }
    function CollapseAllAppCategories() {
        $(".app-icon-category-list.show-category").each(function () {
            var tempId = $(this).attr("data-appid");
            $(".app-icon-category-list[data-appid='" + tempId + "']").removeClass("show-category");
            $("." + tempId + "-category-holder").removeClass("show-category");
            $("." + tempId + "-category-holder").slideUp(openWSE_Config.animationSpeed, function () {
                $("." + tempId + "-category-holder").addClass("hide-category");
            });
        });

        cookieFunctions.del("categories-expanded");
    }
    function LoadCategorySections() {
        if ($(".app-icon-category-list").length > 0) {
            $(".app-icon-category-list").each(function () {
                var categoryId = $(this).attr("data-appid");
                if (categoryId) {
                    $("." + categoryId + "-category-holder").append($(".app-category-div." + categoryId));
                    $(".app-category-div." + categoryId).show();
                }
            });
        }
    }
    function LoadCategoryCookies() {
        LoadCategorySections();

        cookieFunctions.get("categories-expanded", function (categoriesExpanded) {
            if (categoriesExpanded) {
                var splitList = categoriesExpanded.split("~");
                for (var i = 0; i < splitList.length; i++) {
                    if (splitList[i]) {
                        $(".app-icon-category-list[data-appid='" + splitList[i] + "']").addClass("show-category");
                        var $categoryHolder = $("." + splitList[i] + "-category-holder");
                        $categoryHolder.addClass("show-category");
                        $categoryHolder.removeClass("hide-category");
                        $categoryHolder.show();
                    }
                }
            }
        });
    }


    var previousWidth = 0;
    var previousHeight = 0;
    var resizeAxis = null;
    var dragStart_Style3 = false;
    var optionsOpen_Style3 = false;

    function ApplyAppDragResize() {
        if (openWSE.IsComplexWorkspaceMode()) {
            var cancelCtrls = '.app-body, .exit-button-app, .minimize-button-app, .maximize-button-app, .options-button-app, .app-maximized, .auto-full-page, .app-popup-inner-app, .app-head-style3';
            if (openWSE_Config.appStyle != "Style_3") {
                cancelCtrls += ', .app-head-button-holder';
            }
            else if (openWSE_Config.appStyle == "Style_3") {
                $(".app-main-style3 .app-head-hover-button").unbind("mouseenter");
                $(".app-main-style3 .app-head-button-holder").unbind("mouseleave");
                $(".app-main-style3 .app-head-hover-button").bind("mouseenter", function () {
                    $(this).parent().find(".app-head-button-holder").show();
                });
                $(".app-main-style3 .app-head-button-holder").bind("mouseleave", function () {
                    if (!dragStart_Style3 && !optionsOpen_Style3) {
                        $(this).parent().find(".app-head-button-holder").hide();
                    }
                });
                $(document.body).on("click", ".move-button-app", function () {
                    return false;
                });
            }

            $(".app-main-holder").draggable({
                scroll: true,
                cancel: cancelCtrls,
                start: function (event, ui) {
                    CloseTopDropDowns();

                    var $this = $(this);
                    SetActiveApp($this);
                    CreateDragSnapObjects(this);

                    $this.css("opacity", "0.6");
                    $this.css("filter", "alpha(opacity=60)");

                    if (openWSE_Config.appStyle == "Style_3") {
                        $this.find(".app-head-button-holder").show();
                        dragStart_Style3 = true;
                    }

                    // Apply an overlay over app
                    // This fixes the issues when dragging iframes
                    $(".app-main-holder").each(function (index) {
                        if ($(this).css("display") == "block") {
                            ApplyOverlayFix(this);
                        }
                    });
                    event.stopPropagation();
                }
            }).resizable({
                handles: "se, s, e",
                minWidth: 150,
                minHeight: 150,
                create: function (event, ui) {
                    var $this = $(this);

                    if ($this.hasClass('no-resize')) {
                        if (($this.hasClass('app-main')) && (!$this.hasClass('auto-full-page')) && (!$this.hasClass('auto-full-page-min'))) {
                            $this.className = "app-main-holder app-main ui-draggable";
                        }
                        else if (($this.hasClass('app-main')) && (($this.hasClass('auto-full-page')) || ($this.hasClass('auto-full-page-min')))) {
                            if ($this.hasClass('auto-full-page-min')) {
                                $this.className = "app-main-holder app-main auto-full-page-min ui-draggable";
                            }
                            else {
                                $this.className = "app-main-holder app-main auto-full-page ui-draggable";
                            }
                        }
                        else if (($this.hasClass('app-main-nobg')) && (!$this.hasClass('auto-full-page')) && (!$this.hasClass('auto-full-page-min'))) {
                            $this.className = "app-main-holder app-main-nobg ui-draggable";
                        }
                        else {
                            $this.className = "app-main-holder app-main-nobg auto-full-page ui-draggable";
                        }
                        $this.find(".ui-resizable-handle").remove();
                    }
                }
            });

            if (openWSE_Config.appSnapHelper) {
                $(".app-main-holder").draggable({
                    snap: ".app-snap-helper",
                    snapped: function (event, ui) {
                        AddSnapHelperClass(ui.snapElement);
                    },
                    drag: function (event, ui) {
                        var draggable = $(this).data("ui-draggable");
                        $.each(draggable.snapElements, function (index, element) {
                            if (element.snapping) {
                                draggable._trigger("snapped", event, $.extend({}, ui, {
                                    snapElement: $(element.item)
                                }));
                            }
                        });
                    }
                });
            }

            if ($("#hf_appContainer").val() != "") {
                $(".app-main-holder").draggable("option", "containment", $("#hf_appContainer").val());
                $(".app-main-holder").resizable("option", "containment", "parent");
            }

            $(".app-main-holder").on("dragstop", function (event, ui) {
                var $this = $(this);

                if (openWSE_Config.appStyle == "Style_3") {
                    dragStart_Style3 = false;
                }

                $this.css("opacity", "1.0");
                $this.css("filter", "alpha(opacity=100)");
                if (this.offsetTop < 0) {
                    $this.animate({
                        top: 0
                    }, openWSE_Config.animationSpeed, function () { $this.css("top", "0"); });
                }
                if (this.offsetLeft < 0) {
                    $this.animate({
                        left: 0
                    }, openWSE_Config.animationSpeed, function () { $this.css("left", "0"); });
                }
                var name = $this.find(".app-title").eq(0).text();
                var workspace = Getworkspace();

                var width = $this.width();
                var height = $this.height();

                RemoveOverlayFix(this);

                openWSE.AjaxCall(saveHandler + "/App_Move", '{ "appId": "' + $this.attr("data-appid") + '","name": "' + name + '","x": "' + $this.css("left") + '","y": "' + $this.css("top") + '","width": "' + width + '","height": "' + height + '","workspace": "' + workspace + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
            });

            $(".app-main-holder").on("resize", function (event, ui) {
                if (ui != null) {
                    var $this = $(this);

                    if (!$this.hasClass('no-resize')) {
                        var w = ui.size.width;
                        var minw = parseInt($this.css("min-width"));
                        if (w < minw) {
                            w = minw;
                        }

                        var h = ui.size.height;
                        var minh = parseInt($this.css("min-height"));
                        if (h < minh) {
                            h = minh;
                        }

                        $this.css({
                            'width': w,
                            'height': h
                        });

                        previousWidth = w;
                        previousHeight = h;

                        ResizeSnapHelper(this, w, h);
                    }
                    else {
                        $this.css({
                            'width': previousWidth,
                            'height': previousHeight
                        });
                    }

                    ResizeAppBody(this);
                }
            });
            $(".app-main-holder").on("resizestart", function (event, ui) {
                resizingAppInProgress = true;
                CloseTopDropDowns();

                if ($(this).hasClass("app-maximized")) {
                    $(this).removeClass("app-maximized");
                    var $maxBtn = $(this).find(".maximize-button-app");
                    if ($maxBtn.length > 0) {
                        $maxBtn.removeClass("active");
                    }
                }

                if ($(this).data("ui-resizable") != null) {
                    resizeAxis = $(this).data("ui-resizable").axis;
                }

                SetActiveApp(this);
                CreateDragSnapObjects(this);

                // Apply an overlay over app
                // This fixes the issues when dragging iframes
                $(".app-main-holder").each(function (index) {
                    if ($(this).css("display") == "block") {
                        ApplyOverlayFix(this);
                    }
                });

                event.stopPropagation();
            });
            $(".app-main-holder").on("resizestop", function (event, ui) {
                resizingAppInProgress = false;
                if (!$(this).hasClass('no-resize')) {
                    ResizeAppBody(this);
                    previousWidth = 0;
                    previousHeight = 0;
                    RemoveOverlayFix(this);

                    openWSE.AjaxCall(saveHandler + "/App_Resize", '{ "appId": "' + $(this).attr("data-appid") + '","width": "' + ui.size.width + '","height": "' + ui.size.height + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
                }
            });
        }
    }
    function ResizeSnapHelper(_this, w, h) {
        if (openWSE_Config.appSnapHelper) {
            var $this = $(_this);
            var topPos = $this.position().top;
            var leftPos = $this.position().left;

            var currWorkspace = Getworkspace();
            var $workspace = $("#MainContent_" + currWorkspace);

            for (var i = 0; i < $workspace.find(".app-snap-helper").length; i++) {
                var $snapHelper = $workspace.find(".app-snap-helper").eq(i);
                if (ResizeSnapHelperLeftRight(_this, $snapHelper, leftPos, w) || ResizeSnapHelperTopBottom(_this, $snapHelper, topPos, h)) {
                    ResizeSnapHelperActive(_this, $snapHelper);
                    break;
                }
            }
        }
    }
    function ResizeSnapHelperLeftRight(_this, _snapHelper, leftPos, w) {
        var snapDataType = $(_snapHelper).attr("data-type");
        var snapPosLeft = $(_snapHelper).position().left;
        var snapOuterWidth = $(_snapHelper).outerWidth();

        if ((resizeAxis == "se" || resizeAxis == "e") && (snapDataType == "left" || snapDataType == "right")) {
            var thisPos = leftPos + w;
            if (thisPos >= (snapPosLeft + snapOuterWidth) - 10 && thisPos <= (snapPosLeft + snapOuterWidth) + 10) {

                var borderLeftWt = 1;
                if ($(_this).css("border") != "") {
                    var borderWt = $(_this).css("border").split("px")[0];
                    borderWt = parseInt(borderWt);
                    if (borderWt && borderWt.toString() != "NaN") {
                        borderLeftWt = borderWt - 2;
                    }
                }
                else {
                    if ($(_this).css("border-left") != "") {
                        var borderWt = $(_this).css("border-left").split("px")[0];
                        borderWt = parseInt(borderWt);
                        if (borderWt && borderWt.toString() != "NaN") {
                            borderLeftWt = borderWt - 2;
                        }
                    }
                }


                previousWidth = previousWidth - (thisPos - (snapPosLeft + borderLeftWt));
                return true;
            }
        }

        return false;
    }
    function ResizeSnapHelperTopBottom(_this, _snapHelper, topPos, h) {
        var snapDataType = $(_snapHelper).attr("data-type");
        var snapPosTop = $(_snapHelper).position().top;
        var snapOuterHeight = $(_snapHelper).outerHeight();

        if ((resizeAxis == "se" || resizeAxis == "s") && (snapDataType == "top" || snapDataType == "bottom")) {
            var thisPos = topPos + h;
            if (thisPos >= (snapPosTop + snapOuterHeight) - 10 && thisPos <= (snapPosTop + snapOuterHeight) + 10) {

                var borderTopWt = 1;
                if ($(_this).css("border") != "") {
                    var borderWt = $(_this).css("border").split("px")[0];
                    borderWt = parseInt(borderWt);
                    if (borderWt && borderWt.toString() != "NaN") {
                        borderTopWt = borderWt - 2;
                    }
                }
                else {
                    if ($(_this).css("border-top") != "") {
                        var borderWt = $(_this).css("border-top").split("px")[0];
                        borderWt = parseInt(borderWt);
                        if (borderWt && borderWt.toString() != "NaN") {
                            borderTopWt = borderWt - 2;
                        }
                    }
                }

                previousHeight = previousHeight - (thisPos - (snapPosTop + borderTopWt));
                return true;
            }
        }

        return false;
    }
    function ResizeSnapHelperActive(_this, _snapHelper) {
        AddSnapHelperClass(_snapHelper);
        $(_this).addClass("no-resize");
        setTimeout(function () {
            $(_this).removeClass("no-resize");
        }, 200);
    }

    function ReloadApp(_this) {
        var name = $(_this).closest(".app-head-button-holder").parent().find(".app-title").eq(0).text();
        var $_id = $(_this).closest(".app-head-button-holder").parent();
        var id = $_id.attr("data-appid");

        ResizeAppBody($_id);
        $(".app-main-holder").css("z-index", "1000");
        $_id.css("z-index", "3000");

        var $_idOptions = $_id.find(".options-button-app");
        var $_parent = $_idOptions.parent();
        $_idOptions.removeClass("active");
        $_parent.find(".app-popup-inner-app").hide();
        if (openWSE_Config.appStyle == "Style_3") {
            $_parent.find(".app-head-button-holder").hide();
            optionsOpen_Style3 = false;
        }

        var modalClasses = id + outsideAppModal;
        $("." + modalClasses).each(function () {
            $(this).remove();
        });

        if ($_id.find(".iFrame-apps").length > 0) {
            var _content = $_id.find(".iFrame-apps")[0];
            if (_content.src != null) {
                var tempContentSrc = _content.src;
                _content.src = "";

                setTimeout(function () {
                    _content.src = tempContentSrc;
                    if ($_id.find(".app-body").find("div").html() == null) {
                        if ($_id.find(".app-body").find(".loading-background-holder").length <= 0) {
                            AppendLoadingMessage($_id.find(".app-body"));
                        }
                        $_id.find(".iFrame-apps").one('load', (function () {
                            $_id.find(".app-body").find(".loading-background-holder").each(function () {
                                $(this).remove();
                            });
                        }));
                    }
                    else {
                        if ($_id.find(".app-body").find("div").find(".loading-background-holder").length <= 0) {
                            AppendLoadingMessage($_id.find(".app-body").find("div"));
                        }

                        $_id.find(".iFrame-apps").one('load', (function () {
                            $_id.find(".app-body").find("div").find(".loading-background-holder").each(function () {
                                $(this).remove();
                            });
                        }));
                    }
                }, 100);
            }
        }
        else {
            if ($("#MainContent_" + id.replace(/-/g, "_") + "_advPanel").length > 0) {
                $("#MainContent_" + id.replace(/-/g, "_") + "_advPanel").hide();
            }

            if ($_id.find(".app-body").find(".loading-background-holder").length <= 0) {
                AppendLoadingMessage($_id.find(".app-body"));
            }

            $("#hf_ReloadApp").val(id);
            openWSE.CallDoPostBack('hf_ReloadApp', '');
        }
    }
    function AboutApp(_this) {
        var $_id = $(_this).closest(".app-head-button-holder").parent();
        if ($_id.length === 0) {
            $_id = $(_this);
        }

        var appId = $_id.attr("data-appid");
        $("#hf_aboutstatsapp").val("about;" + appId);

        var $_idOptions = $_id.find(".options-button-app");
        var $_parent = $_idOptions.parent();
        $_idOptions.removeClass("active");
        $_parent.find(".app-popup-inner-app").hide();
        if (openWSE_Config.appStyle == "Style_3") {
            $_parent.find(".app-head-button-holder").hide();
            optionsOpen_Style3 = false;
        }

        $("#MainContent_pnl_aboutHolder").html("");

        loadingPopup.Message("Loading. Please Wait...");
        openWSE.CallDoPostBack("hf_aboutstatsapp", "");
    }
    function UninstallApp(appId) {
        openWSE.ConfirmWindow("Are you sure you want to remove this app?",
          function () {
              $("#hf_aboutstatsapp").val("uninstall;" + appId);
              $(".app-main-holder[data-appid='" + appId + "']").find(".exit-button-app").trigger("click");
              $(".app-main-holder[data-appid='" + appId + "']").remove();
              loadingPopup.Message('Removing App');
              openWSE.CallDoPostBack("hf_aboutstatsapp", "");
          }, null);
    }
    function PopOutFrame(_this, url) {
        var $_id = $(_this).closest(".app-head-button-holder").parent();

        var _width = 315;
        var _height = 425;

        if ($_id.length > 0) {
            var name = $_id.find(".app-title").eq(0).text();
            _width = $_id.width();
            _height = $_id.height();
        }

        $_id.fadeOut(openWSE_Config.animationSpeed, function () {
            if ($_id.hasClass("app-maximized")) {
                $_id.removeClass("app-maximized");
            }

            RemoveworkspaceAppNum($_id);
            RemoveAppIconActive($_id);

            var canclose = 1;
            var hfcanclose = document.getElementById("hf_" + $_id.attr("id"));

            if (hfcanclose != null) {
                canclose = 0;
            }
            if ($_id.find(".app-body").find("div").html() == null) {
                if (canclose == 1) {
                    $_id.find(".app-body").html("");
                }
            }
            else {
                if (canclose == 1) {
                    $_id.find(".app-body").find("div").html("");
                }
            }

            $_id.css({
                visibility: "hidden",
                left: "",
                top: "",
                width: "",
                height: ""
            });

            if ($_id.find(".options-button-app").length > 0) {
                $_id.find(".options-button-app").removeClass("active");
                $_id.find(".app-popup-inner-app").hide();
                if (openWSE_Config.appStyle == "Style_3") {
                    $_id.find(".app-head-button-holder").hide();
                    optionsOpen_Style3 = false;
                }
            }

            openWSE.AjaxCall(saveHandler + "/App_Close", '{ "appId": "' + $_id.attr("data-appid") + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');
        });

        var specs = "width=" + _width + "px,height=" + _height + "px,location=no,menubar=no,toolbar=no,status=no,resizable=yes,scrollbars=yes";
        myWindow = window.open(url, name, specs);
        myWindow.focus();
    }
    function PopOutFrameFromSiteTools(name, url) {
        var specs = "width=400px,height=400px,location=no,menubar=no,toolbar=no,status=no,resizable=yes,scrollbars=yes";
        myWindow = window.open(url, name, specs);
        myWindow.focus();

        openWSE.LoadModalWindow(false, "ConfirmApp-element", "");
        $("#ConfirmApp-element").remove();
    }
    function PopOutTool(url) {
        window.location.href = url;
    }
    function MoveAppToworkspace(id) {
        var d = "";
        if ($(id).length > 0) {
            if (($(id).hasClass("app-popup-selector")) || ($(id).hasClass("app-options-workspace-switch"))) {
                d = $.trim($(id).val());
            }
            else {
                d = $.trim($(id).text());
            }
        }
        else {
            d = id;
        }

        if (d == "" || d == "-") {
            return;
        }

        SetWorkspaceNumber(d);

        var workspacenum = d;
        d = "workspace_" + d;

        var current = Getworkspace();
        $_name = $(id).closest('.app-icon');
        if ($_name.length == 0) {
            $_name = $(id).closest(".app-head-button-holder").parent();
        }

        var isOn = 0;
        var tempId = $_name.attr('data-appid');
        if ((tempId != "") && (tempId != undefined) && (tempId != null)) {
            AddworkspaceAppNum(d, tempId);
            if ($('#' + tempId).css('display') == 'block') {
                isOn = 1;
            }
        }

        if ((current != d) || (isOn == 0) || ((isOn == 1) && (current == d))) {
            openWSE.AjaxCall(saveHandler + "/App_CurrentWorkspace", '{ "workspace": "' + workspacenum + '","workspaceMode": "' + openWSE_Config.workspaceMode + '" }');

            LoadApp($_name, d);
            if (current != d) {
                _id = "#MainContent_" + d;
                $('#workspace_holder .workspace-holder').each(function () {
                    if ($(this).css("visibility") == "visible") {
                        if (!openWSE_Config.taskBarShowAll && openWSE.ConvertBitToBoolean($("#minimized_app_bar").attr("data-show"))) {
                            $("#minimized_app_bar").hide();
                        }
                        $(this).fadeTo(openWSE_Config.animationSpeed, 0.0, function () {
                            // Move off screen
                            MoveOffScreen(this);

                            if (!openWSE_Config.taskBarShowAll) {
                                HideTasks(this);
                                ShowTasks(_id);
                            }
                        });
                    }
                });

                $(_id).fadeTo(openWSE_Config.animationSpeed, 1.0);

                // Move selected onto screen
                MoveOnScreen_WorkspaceOnly($(_id));
                ResizeAllAppBody($(_id));

                MoveOutSideModalWindowToWorkspace(tempId);

                $(".app-options").css("visibility", "hidden");
                setTimeout(function () {
                    $(".app-popup").hide();
                }, openWSE_Config.animationSpeed);
            }
        }
    }
    function ReapplyActiveIcons() {
        $(".app-main-holder").each(function () {
            if ($(this).css("display") == "block") {
                SetAppIconActive(this);
                var workspaceId = $(this).parent().attr("id");
                if ((workspaceId != undefined) && (workspaceId != null) && (workspaceId != "")) {
                    AddworkspaceAppNum(workspaceId, $(this).attr("data-appid"));
                }
            }
        });
    }
    function AppendLoadingMessage(_this) {
        $(_this).append(loadingMessage);
        var $appMain = $(_this).closest(".app-main-holder");
        if ($appMain.length > 0) {
            var $appIcon = $(".app-icon[data-appid='" + $appMain.attr("data-appid") + "']");
            if ($appIcon.length > 0 && $appIcon.find("img").length > 0 && $appIcon.find("img").css("display") != "none" && !$appIcon.find("img").hasClass("display-none")) {
                $(_this).find(".loading-background-holder").css("background-image", "url('" + $appIcon.find("img").attr("src") + "')");
                $(_this).find(".loading-background-holder").attr("data-usespinner", "false");
            }
        }
    }
        

    function CreateDragSnapObjects(_this) {
        if (openWSE_Config.appSnapHelper) {
            RemoveSnapHelperClass(_this);
            if (_this != null) {
                var draggable = $(_this).data("ui-draggable");

                var currWorkspace = Getworkspace();
                var $workspace = $("#MainContent_" + currWorkspace);

                if ($workspace.length > 0) {
                    $workspace.find(".app-main-holder").each(function () {
                        if (!$(this).hasClass("app-maximized") && !$(this).hasClass("auto-full-page")) {
                            var appId = $(this).attr("data-appid");
                            if ($(this).css("display") == "block" && $(this).css("visibility") != "hidden" && $(_this).attr("data-appid") != appId) {
                                if ($(".app-snap-helper[data-appid='" + appId + "']").length == 0) {
                                    var borderLeftWt = GetBorderWidthForHelper(this, "left", 0, 0);
                                    var borderRightWt = GetBorderWidthForHelper(this, "right", 0, 0);
                                    var borderTopWt = GetBorderWidthForHelper(this, "top", 0, 0);
                                    var borderBottomWt = GetBorderWidthForHelper(this, "bottom", 0, 0);

                                    var leftPos = parseInt($(this).css("left").replace("px", "")) + (borderLeftWt - 1);
                                    var rightPos = leftPos + $(this).outerWidth() - (borderRightWt + borderLeftWt) + 1;
                                    var topPos = parseInt($(this).css("top").replace("px", "")) + (borderTopWt - 1);
                                    var bottomPos = topPos + $(this).outerHeight() - (borderBottomWt + borderTopWt) + 1;

                                    $workspace.append("<div class='app-snap-helper' data-appid='" + appId + "' data-type='left' style='left: " + leftPos + "px; top: 0; bottom: 0; width: 1px;'></div>");
                                    $workspace.append("<div class='app-snap-helper' data-appid='" + appId + "' data-type='right' style='left: " + rightPos + "px; top: 0; bottom: 0; width: 1px;'></div>");
                                    $workspace.append("<div class='app-snap-helper' data-appid='" + appId + "' data-type='top' style='top: " + topPos + "px; left: 0; right: 0; height: 1px;'></div>");
                                    $workspace.append("<div class='app-snap-helper' data-appid='" + appId + "' data-type='bottom' style='top: " + bottomPos + "px; left: 0; right: 0; height: 1px;'></div>");

                                    if (draggable) {
                                        AddItemToSnapElements(appId, leftPos, rightPos, topPos, bottomPos, draggable);
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }
    }
    function AddItemToSnapElements(appId, leftPos, rightPos, topPos, bottomPos, draggable) {
        var $thisApp = $(".app-main-holder[data-appid='" + appId + "']");
        var borderLeftWt = GetBorderWidthForHelper($thisApp, "left", -1, 1);
        var borderRightWt = GetBorderWidthForHelper($thisApp, "right", -1, -1);
        var borderTopWt = GetBorderWidthForHelper($thisApp, "top", -1, 1);
        var borderBottomWt = GetBorderWidthForHelper($thisApp, "bottom", -1, -1);

        var object1_left = {
            height: $(".app-snap-helper[data-appid='" + appId + "'][data-type='left']").height(),
            item: $(".app-snap-helper[data-appid='" + appId + "'][data-type='left']")[0],
            left: (leftPos + $("#main_container").position().left) + borderLeftWt,
            snapping: false,
            top: $("#top_bar").outerHeight(),
            width: 1
        }

        var object2_right = {
            height: $(".app-snap-helper[data-appid='" + appId + "'][data-type='right']").height(),
            item: $(".app-snap-helper[data-appid='" + appId + "'][data-type='right']")[0],
            left: (rightPos + $("#main_container").position().left) + borderRightWt,
            snapping: false,
            top: $("#top_bar").outerHeight(),
            width: 1
        }

        var object3_top = {
            height: 1,
            item: $(".app-snap-helper[data-appid='" + appId + "'][data-type='top']")[0],
            left: $("#main_container").position().left,
            snapping: false,
            top: (topPos + $("#top_bar").outerHeight()) + borderTopWt,
            width: $(".app-snap-helper[data-appid='" + appId + "'][data-type='top']").width()
        }

        var object4_bottom = {
            height: 1,
            item: $(".app-snap-helper[data-appid='" + appId + "'][data-type='bottom']")[0],
            left: $("#main_container").position().left,
            snapping: false,
            top: (bottomPos + $("#top_bar").outerHeight()) + borderBottomWt,
            width: $(".app-snap-helper[data-appid='" + appId + "'][data-type='bottom']").width()
        }

        draggable.snapElements.push(object1_left);
        draggable.snapElements.push(object2_right);
        draggable.snapElements.push(object3_top);
        draggable.snapElements.push(object4_bottom);
    }
    function AddSnapHelperClass(_this) {
        if (_this) {
            $(_this).addClass("app-snap-helper-highlight");
            setTimeout(function () {
                $(_this).removeClass("app-snap-helper-highlight");
            }, 1500);
        }
    }
    function RemoveSnapHelperClass(_this) {
        var draggable = $(_this).data("ui-draggable");
        if (draggable) {
            draggable.snapElements = new Array();
        }

        $(".app-snap-helper").remove();
    }

    function GetBorderWidthForHelper(ele, posDir, difVal, defaultVal) {
        var $thisApp = $(ele);
        if ($thisApp.length > 0) {
            if ($thisApp.css("border") != "") {
                var borderWt = $thisApp.css("border").split("px")[0];
                borderWt = parseInt(borderWt);
                if (borderWt && borderWt.toString() != "NaN") {
                    return borderWt + difVal;
                }
            }
            else {
                if ($thisApp.css("border-" + posDir) != "") {
                    var borderWt = $thisApp.css("border-" + posDir).split("px")[0];
                    borderWt = parseInt(borderWt);
                    if (borderWt && borderWt.toString() != "NaN") {
                        return borderWt + difVal;
                    }
                }
            }
        }

        return defaultVal;
    }


    /* Group Login Modal */
    function GroupLoginModal() {
        if ($("#group_tab").hasClass("active")) {
            $("#group_tab").find("#grouplistdiv").html("");
        }
        else {
            GetandBuildGroupList();
        }
    }
    function GetandBuildGroupList() {
        var $groupListDiv = $("#group_tab").find("#grouplistdiv");
        if ($groupListDiv.length === 0 && $("#pnlContent_Groups").length > 0) {
            $groupListDiv = $("#pnlContent_Groups").find("#grouplistdiv");
        }

        $groupListDiv.html("<div class='pad-all'>Loading groups...</div>");
        $("#divGroupLogoff").hide();

        openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetUserGroups", '{ }', null, function (data) {
            var x = "";
            try {
                for (var i = 0; i < data.d[0].length; i++) {
                    var groupId = data.d[0][i][0];
                    var groupName = data.d[0][i][1];
                    var image = data.d[0][i][2];
                    var owner = data.d[0][i][3];
                    var background = data.d[0][i][4];
                    var groupApps = data.d[0][i][5];

                    var styleBackground = "style=\"background: url('" + background + "'); background-size: cover;\"";

                    x += "<div class='group-selection-entry' onclick='openWSE.LoginAsGroup(\"" + groupId + "\")' title='Click to login' " + styleBackground + ">";
                    x += "<div class='overlay'></div>";
                    x += "<div class='group-selection-info'>";
                    x += "<img class='group-img' alt='Group Logo' src='" + image + "' />";
                    x += "<div class='group-name-info'>";
                    x += "<span class='group-name'>" + groupName + "</span><div class='clear-space-five'></div>";
                    x += "<span class='group-info'><b class='pad-right-sml'>Owner:</b>" + owner + "</span>";
                    x += "</div><div class='clear'></div>";

                    var appList = "<div class='group-selection-applist'>";
                    for (var j = 0; j < groupApps.length; j++) {
                        appList += "<div class='group-applist-item'>";
                        appList += "<img alt='' src='" + GetSiteRoot() + groupApps[j].Icon + "' />";
                        appList += "<span>" + groupApps[j].AppName + "</span>";
                        appList += "<div class='clear'></div></div>";
                    }
                    appList += "</div><div class='clear'></div>";

                    x += appList;
                    x += "</div></div>";
                }

                x += "<div class='clear'></div>";
            }
            catch (evt) { }

            if (data.d[2]) {
                x += "<div class=\"clear\"></div><h3 class=\"pad-all\">You are currently logged into " + data.d[2] + "</h3><div class=\"clear\"></div>";
                $("#divGroupLogoff").show();
            }

            $groupListDiv.html(x);

            openWSE.ResizeTopDropDowns();
        });
    }
    function LoginAsGroup(id) {
        if (id != "") {
            loadingPopup.Message("Logging into Group");
        }
        else {
            loadingPopup.Message("Logging out of Group");
        }

        CloseTopDropDowns();

        openWSE.AjaxCall("WebServices/AcctSettings.asmx/LoginUnderGroup", '{ "id": "' + id + '" }', null, function (data) {
            if (data.d == "true") {
                location.reload();
            }
            else {
                if (window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1) {
                    window.location.href = "AppRemote.aspx?group=" + id;
                }
                else {
                    window.location.href = "Login.aspx?group=" + id;
                }
            }
        });
    }
    function CloseGroupLoginModal() {
        if (window.location.href.toLowerCase().indexOf("appremote.aspx") === -1) {
            $("#group_tab").find("#grouplistdiv").html("");
        }
    }


    /* Hash Change Functions */
    function HashChange() {
        var url = location.hash;
        if (url.indexOf("iframecontent") != -1) {
            openWSE.LoadIFrameContentHistory(getUrlParameterByName("iframecontent"));
        }
        if ((url.indexOf("help") != -1)) {
            HelpOverlayHistory();
            return;
        }

        // Close any if possible
        if (url.indexOf("iframecontent") === -1 && $("#iframe-container-helper").length > 0) {
            openWSE.CloseIFrameContent();
        }
        if ($("#help_main_holder").css("display") == "block") {
            CloseHelpOverlay();
        }
    }
    function getUrlParameterByName(e, x) {
        e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var t = "[\\?&]" + e + "=([^&#]*)";
        var n = new RegExp(t);
        var url = window.location.href.replace("#", "");
        if (x) {
            url = x.replace("#", "");
        }
        var r = n.exec(url);
        if (r == null)
            return "";
        else
            return decodeURIComponent(r[1].replace(/\+/g, " "));
    }
    function AddUrlHashQueryPath(name, value) {
        var fullQuery = "";
        var loc = "";

        if (isMobileMode) {
            if (name === "iframecontent") {
                openWSE.CloseTopDropDowns();
            }
        }

        if (window.location.href.indexOf(name) === -1) {
            fullQuery = name + "=" + escape(value);
            loc = window.location.href.replace("?" + fullQuery, "").replace("&" + fullQuery, "");
            if (loc.indexOf("#") === -1) {
                loc += "#";
            }

            if (loc.indexOf("#?") !== -1) {
                loc += "&" + fullQuery;
            }
            else if (loc.indexOf("&") === -1) {
                loc += "?" + fullQuery;
            }
            else {
                loc += "&" + fullQuery;
            }
            window.location.href = loc;
        }
        else {
            fullQuery = name + "=" + escape(getUrlParameterByName(name));
            loc = window.location.href.replace("?" + fullQuery, "").replace("&" + fullQuery, "");

            fullQuery = name + "=" + escape(value);
            if (loc.indexOf("#") === -1) {
                loc += "#";
            }

            if (loc.indexOf("#?") !== -1) {
                loc += "&" + fullQuery;
            }
            else if (loc.indexOf("&") === -1) {
                loc += "?" + fullQuery;
            }
            else {
                loc += "&" + fullQuery;
            }
            window.location.href = loc;
        }
    }
    function RemoveUrlHashQueryPath(name) {
        if (window.location.href.indexOf(name) !== -1) {
            var fullQuery = name + "=" + getUrlParameterByName(name);
            if (window.location.href.indexOf(fullQuery) === -1) {
                fullQuery = name + "=" + escape(getUrlParameterByName(name));
            }
            
            window.location.href = window.location.href.replace("?" + fullQuery, "").replace("&" + fullQuery, "");
        }
    }


    /* Desktop - Mobile Site */
    function loadCSS(url) {
        var fullUrl = openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/" + url;
        $("link[href='" + fullUrl + "']").remove();
        var head = document.getElementsByTagName('head')[0];
        link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = fullUrl;
        head.appendChild(link);
    }
    function unloadCSS(url) {
        var fullUrl = openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/" + url;
        $("link[href='" + fullUrl + "']").remove();
    }
    function LoadViewPort() {
        if (isMobileDevice()) {
            if ($("#mobileViewport").length == 0) {
                var head = document.getElementsByTagName('head')[0];
                meta = document.createElement('meta');
                meta.name = 'viewport';
                meta.id = 'mobileViewport';
                meta.content = 'initial-scale=1.0, user-scalable=no';
                head.appendChild(meta);
            }
            $(document).tooltip({ disabled: true });
        }
        else {
            if ($("#mobileViewport").length > 0) {
                $("#mobileViewport").remove();
            }
        }
    }


    /* Group Invite Notification Actions */
    function AcceptGroupNotification(_this, groupId) {
        if (!openWSE_Config.demoMode) {
            loadingPopup.Message("Accepting. Please Wait...");
            var $this = $(_this).closest("tr");
            var id = $this.attr("id");
            openWSE.AjaxCall("WebServices/NotificationRetrieve.asmx/AcceptInviteNotifications", '{ "id": "' + id + '", "groupId": "' + groupId + '" }', null, function (data) {
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    $this.fadeOut(openWSE_Config.animationSpeed, function () {
                        loadingPopup.RemoveMessage();
                        $this.remove();
                        if ($("#lbl_notifications").find("span").html() == "1") {
                            ResetNoti();
                        }
                        else {
                            var currTotal = parseInt($("#lbl_notifications").find("span").html());
                            currTotal -= 1;
                            $("#lbl_notifications").html("<span>" + currTotal + "</span>");
                        }
                    });
                }
            });
        }
    }


    /* Sidebar and Resizing Container */
    function MobileMode() {
        isMobileMode = true;

        if (window.location.href.indexOf("?mobileMode=true#?tab=pnl_IconSelector") == -1) {
            $("#lnk_BackToWorkspace").attr("href", GetSiteRoot() + "AppRemote.aspx#?id=adminPages&category=pageSelect");
        }
        else {
            $("#lnk_BackToWorkspace").attr("href", GetSiteRoot() + "AppRemote.aspx#");
        }

        $(".my-app-remote-link").remove();
        $(".sitemenu-selection").addClass("mobile-mode");

        $("#hyp_accountSettings").attr("href", $("#hyp_accountSettings").attr("href") + "?mobileMode=true");
        $("#footer-signdate").prepend("<a href='" + GetSiteRoot() + "AppRemote.aspx'>Home</a>");

        var noHeader = false;
        var noHeaderModeParm = getParameterByName("noHeader");
        if (noHeaderModeParm && noHeaderModeParm == "true") {
            noHeader = true;
        }

        if (noHeader) {
            $("#workspace-selector").find("#lnk_BackToWorkspace").hide();
        }
        else {
            $("#workspace-selector").find("#lnk_BackToWorkspace").attr("onclick", "loadingPopup.Message('Loading...');return true;");
        }

        if ($("#MainContent_pnlLinkBtns").length > 0) {
            $("#MainContent_pnlLinkBtns").find("a").each(function () {
                var href = $(this).attr("href");
                if (href.indexOf("#?") == -1) {
                    if (href.indexOf("?") == -1) {
                        href += "?mobileMode=true";
                    }
                    else {
                        href += "&mobileMode=true";
                    }

                    if (noHeader) {
                        href += "&noHeader=true";
                    }

                    if (window.location.href.toLowerCase().indexOf("appremote.aspx") !== -1) {
                        href += "&fromAppRemote=true";
                    }

                    $(this).attr("href", href);
                }
            });
        }

        if ($("#user_profile_tab").length > 0) {
            if ($(".close-iframe").length > 0) {
                $(".close-iframe").before($("#user_profile_tab"));
            }
            else {
                $(".iframe-title-top-bar").after($("#user_profile_tab"));
            }
        }

        ResizeContainer();
    }
    function GetCurrentPage() {
        $(".app-icon-links").removeClass("active");
        $(".app-icon-sub-links").removeClass("active");

        var $currThis = null;

        $(".app-icon-links").each(function () {
            if ($(this).attr("href")) {
                var thisHref = $(this).attr("href").toLowerCase();
                if (CheckIfOnWorkspace() && thisHref.indexOf("default.aspx") != -1) {
                    $(this).addClass("active");
                }
                else {
                    if (window.location.href.toLowerCase().indexOf(thisHref) != -1) {
                        $(this).addClass("active");
                        if ($(this).hasClass("has-sub-links")) {
                            $currThis = $(this);
                        }

                        if ($(this).next().hasClass("app-icon-sub-link-holder")) {
                            $(this).next().show();
                            $(this).addClass("expanded-links");
                            var $expand = $(this).find(".img-expand-sml");
                            if ($expand.length == 1) {
                                $expand.removeClass("img-expand-sml");
                                $expand.addClass("img-collapse-sml");
                            }
                        }
                    }
                }
            }
        });

        var foundSubLinkActive = false;
        $(".app-icon-sub-links").each(function () {
            var thisHref = $(this).attr("href").toLowerCase();
            var currPage = window.location.href.toLowerCase();
            if (thisHref.indexOf("#?tab=") >= 0 && currPage.indexOf("#?tab=") === -1) {
                currPage = currPage.replace("?tab=", "#?tab=");
            }

            if (currPage.split("?tab=").length === 3) {
                var splitPage = currPage.split("?tab=");
                currPage = "";
                for (var i = 0; i < splitPage.length; i++) {
                    if (splitPage[i].indexOf("#") !== splitPage[i].length - 1) {
                        currPage += splitPage[i];
                        if (i === 0) {
                            currPage += "#?tab=";
                        }
                    }
                }
            }

            if (currPage.indexOf(thisHref) != -1) {
                $(".app-icon-links").removeClass("active");
                $(this).addClass("active");
                $(this).parent().show();
                var $expand = $(this).parent().prev().find(".img-expand-sml");
                if ($expand.length == 1) {
                    $expand.removeClass("img-expand-sml");
                    $expand.addClass("img-collapse-sml");
                }
                foundSubLinkActive = true;
            }
        });

        if ($currThis != null && $currThis.length > 0 && !foundSubLinkActive) {
            // $currThis.removeClass("active");
            $currThis.next().find(".app-icon-sub-links").eq(0).addClass("active");
        }
        else if ($currThis != null && $currThis.length > 0 && foundSubLinkActive) {
            $currThis.addClass("active");
        }
    }
    function ResizeContainer() {
        if (!resizingAppInProgress) {
            $(".app-snap-helper").remove();
        }

        if ($("#sidebar_container").length === 0) {
            $("#main_container").css("left", "0");
            $("#footer_container").css("left", "0");
        }
    }
    function ExpandAdminLinks(_this) {
        CollapseAllAppCategories();

        if ($("#sidebar_container").hasClass("sidebar-minimized")) {
            return true;
        }
        else {
            var div = $(_this).attr("data-subdiv");
            var $this = $(_this).find(".expand-collapse-btn");
            if (!$(_this).hasClass("expanded-links")) {
                CollapseAllAdminLinks();
                $(_this).addClass("expanded-links");
                $this.closest(".app-icon-links").parent().find("." + div).slideDown(openWSE_Config.animationSpeed, function () {
                    if ($(_this).offset().top + $this.closest(".app-icon-links").parent().find("." + div).outerHeight() > $("#sidebar_container").outerHeight()) {
                        openWSE.SetLeftSidebarScrollTop($this.closest(".app-icon-links").parent().find("." + div).outerHeight() + 20);
                        openWSE.InitializeSlimScroll();
                    }
                });
            }
            else {
                $(_this).removeClass("expanded-links");
                $this.closest(".app-icon-links").parent().find("." + div).slideUp(openWSE_Config.animationSpeed);
            }
        }

        return false;
    }
    function CollapseAllAdminLinks() {
        $(".has-sub-links").each(function () {
            $(this).removeClass("expanded-links");
            $(this).find(".expand-collapse-btn").closest(".app-icon-links").parent().find("." + $(this).attr("data-subdiv")).slideUp(openWSE_Config.animationSpeed);
        });
    }

    function GetSiteRoot() {
        var sitePath = "";
        if (openWSE_Config.siteRootFolder != "") {
            sitePath = openWSE_Config.siteRootFolder + "/";
        }

        return window.location.protocol + "//" + window.location.host + "/" + sitePath;
    }

    function GetElementClassList(ele) {
        if ($(ele).length == 1) {
            var classList = $(ele)[0].className.split(/\s+/);
            return classList;
        }
        return new Array();
    }


    /* Sidebar Controls */
    function ShowHideAccordianSidebar() {
        if (!$("#main_container").hasClass("sidebar-minimized")) {
            HideSidebar();
            if (openWSE_Config.demoMode) {
                cookieFunctions.set("sidebar-menu", "hide", 30);
            }
            else {
                loadingPopup.Message("Updating...");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/SetSidebarContainerState", "{ 'state': 'hide' }", null, null, null, function (data) {
                    loadingPopup.RemoveMessage();
                });
            }
        }
        else {
            ShowSidebar();
            if (openWSE_Config.demoMode) {
                cookieFunctions.del("sidebar-menu");
            }
            else {
                loadingPopup.Message("Updating...");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/SetSidebarContainerState", "{ 'state': '' }", null, null, null, function (data) {
                    loadingPopup.RemoveMessage();
                });
            }
        }

        $(window).resize();
    }
    function ShowSidebar() {
        $("#sidebar_container").removeClass("sidebar-minimized");
        if ($("#sidebar_container").parent().hasClass("slimScrollDiv")) {
            $("#sidebar_container").parent().removeClass("sidebar-minimized");
        }
        $("#main_container, #footer_container, #top_bar").removeClass("sidebar-minimized");

        if ($("#sidebar_accordian").attr("data-icononly") === "true") {
            $("#sidebar_accordian").find(".app-icon-links").each(function () {
                var title = $.trim($(this).find(".app-icon-font").attr("data-pagetitle"));
                if (title) {
                    $(this).attr("title", title);
                }
            });
        }
        else {
            $("#sidebar_accordian").find(".app-icon-links").each(function () {
                $(this).attr("title", "");
            });
        }

        $("#updatePnl_AppList").find(".app-icon > img").each(function () {
            $(this).attr("title", "");
        });
    }
    function HideSidebar() {
        $("#sidebar_container").addClass("sidebar-minimized");
        if ($("#sidebar_container").parent().hasClass("slimScrollDiv")) {
            $("#sidebar_container").parent().addClass("sidebar-minimized");
        }
        $("#main_container, #footer_container, #top_bar").addClass("sidebar-minimized");

        $("#sidebar_accordian").find(".app-icon-links").each(function () {
            var title = $.trim($(this).find(".app-icon-font").attr("data-pagetitle"));
            if (title) {
                $(this).attr("title", title);
            }
        });

        $("#updatePnl_AppList").find(".app-icon > img").each(function () {
            var title = $.trim($(this).parent().find(".app-icon-font").html());
            if (title) {
                $(this).attr("title", title);
            }
        });
    }
    function LoadSidebarShowHideCookie() {
        if (openWSE_Config.demoMode) {
            if ($("#sidebar_container").is(":visible")) {
                cookieFunctions.get("sidebar-menu", function (state) {
                    if (state && state == "hide") {
                        HideSidebar();
                        $(window).resize();
                    }
                });
            }
        }
    }

    function SidebarNavToggleInit() {
        if (openWSE_Config.allowNavMenuCollapseToggle && $(".sidebar-linkpanels").length > 0) {
            var userName = "";
            if ($("#lbl_UserFullName").length > 0) {
                userName = "-" + $.trim($("#lbl_UserFullName").html()).toLowerCase();
            }

            cookieFunctions.get("sidebar-navtitle-expanded" + userName, function (cookieVals) {
                if (cookieVals) {
                    cookieVals = cookieVals.split("~");
                }

                $(".nav-title-toggle-btn").remove();
                $(".sidebar-linkpanels").each(function () {
                    $(this).find(".nav-title").each(function () {
                        var navDataTitle = $(this).text();
                        if ($(this).find("a").length > 0) {
                            navDataTitle = navDataTitle.replace($.trim($(this).find("a").html()), "");
                        }

                        var navDataTitle = $.trim(navDataTitle);
                        var triggerClick = false;

                        if (cookieVals) {
                            for (var i = 0; i < cookieVals.length; i++) {
                                if (cookieVals[i] === navDataTitle) {
                                    triggerClick = true;
                                    break;
                                }
                            }
                        }

                        $(this).prepend("<span title=\"Toggle menu\" class=\"nav-title-toggle-btn\" data-title=\"" + escape(navDataTitle) + "\"></span>");
                        $(this).attr("onclick", "return openWSE.SidebarNavToggleClicked(this, true, event);");
                        $(this).addClass("cursor-pointer");

                        SidebarNavToggleCollapseIndividual(false, this);

                        if (triggerClick) {
                            openWSE.SidebarNavToggleClicked(this, false, null);
                        }
                    });
                });
            });
        }
    }
    function SidebarNavToggleClicked(_this, setCookie, e) {
        if (openWSE_Config.allowNavMenuCollapseToggle && $(".sidebar-linkpanels").length > 0) {
            if (e && !$(e.target).hasClass("nav-title-toggle-btn") && !$(e.target).hasClass("nav-title")) {
                return true;
            }

            var $this = $(_this).find(".nav-title-toggle-btn").eq(0);
            if ($this.hasClass("expanded")) {
                SidebarNavToggleCollapseIndividual(setCookie, _this);
            }
            else {
                if (openWSE_Config.allowNavMenuCollapseToggleOnlyOne) {
                    SidebarNavToggleCollapseAll(setCookie, _this);
                }

                $(_this).addClass("nav-title-active");
                $this.addClass("expanded");
                var $nextDiv = $this.parent().next("div");
                if ($nextDiv.attr("id") === "statusDiv") {
                    if (setCookie) {
                        $nextDiv.slideDown(openWSE_Config.animationSpeed);
                        $this.parent().parent().find("#chatuserlist").slideDown(openWSE_Config.animationSpeed);
                    }
                    else {
                        $nextDiv.show();
                        $this.parent().parent().find("#chatuserlist").show();
                    }
                }
                else {
                    if (setCookie) {
                        $nextDiv.slideDown(openWSE_Config.animationSpeed);
                    }
                    else {
                        $nextDiv.show();
                    }
                }
            }

            if (setCookie) {
                var cookieVal = "";
                $(".sidebar-linkpanels").each(function () {
                    $(this).find(".nav-title").each(function () {
                        var navDataTitle = $(this).find(".nav-title-toggle-btn").attr("data-title");
                        if ($(this).find(".nav-title-toggle-btn").hasClass("expanded") && navDataTitle) {
                            cookieVal += unescape(navDataTitle) + "~";
                        }
                    });
                });

                var userName = "";
                if ($("#lbl_UserFullName").length > 0) {
                    userName = "-" + $.trim($("#lbl_UserFullName").html()).toLowerCase();
                }

                if (cookieVal) {
                    cookieFunctions.set("sidebar-navtitle-expanded" + userName, cookieVal, 30);
                }
                else {
                    cookieFunctions.del("sidebar-navtitle-expanded" + userName);
                }
            }

            return false;
        }

        return true;
    }
    function SidebarNavToggleCollapseAll(animation) {
        $(".sidebar-linkpanels").each(function () {
            $(this).find(".nav-title").each(function () {
                SidebarNavToggleCollapseIndividual(animation, this);
            });
        });
    }
    function SidebarNavToggleCollapseIndividual(animation, _this) {
        $(_this).removeClass("nav-title-active");

        var $this = $(_this).find(".nav-title-toggle-btn").eq(0);

        $this.removeClass("expanded");

        var $nextDiv = $this.parent().next("div");
        if (animation) {
            $nextDiv.slideUp(openWSE_Config.animationSpeed);
        }
        else {
            $nextDiv.hide();
        }
        $nextDiv.addClass("nav-title-toggle-padding");
    }

    function ReloadPage() {
        try {
            window.parent.location.reload();
        }
        catch (evt) {
            window.location.reload();
        }
    }

    function ApplyMobileModeForMenuBar() {
        if ((location.href.toLowerCase().indexOf("sitetools/") > 0 || location.href.toLowerCase().indexOf("about.aspx") > 0) && (location.href.toLowerCase().indexOf("appremote.aspx") == -1 || location.href.toLowerCase().indexOf("redirect=appremote.aspx") > 0) && $(".sitemenu-selection").length > 0) {
            $(".sitemenu-selection").removeClass("mobile-mode");

            var liWidth = 0;
            var longestLi = 0;
            $(".sitemenu-selection").find("li").each(function(){
                liWidth += $(this).outerWidth();
                if ($(this).outerWidth() > longestLi) {
                    longestLi = $(this).outerWidth();
                }
            });

            liWidth += longestLi;

            if ($("#main_container").length > 0) {
                if ($("#main_container").outerWidth() < liWidth) {
                    $(".sitemenu-selection").addClass("mobile-mode");
                }
            }
            else {
                if ($("body").outerWidth() < liWidth) {
                    $(".sitemenu-selection").addClass("mobile-mode");
                }
            }
        }
    }

    function ChangeUserProfileImage() {
        var div = "<div id='UserProfileImageUpdate-element' class='Modal-element'>";
        div += "<div class='Modal-overlay'>";
        div += "<div class='Modal-element-align'>";
        div += "<div class='Modal-element-modal' data-setwidth='520px'>";
        div += "<div class='ModalHeader'><div>";
        div += "<div class='app-head-button-holder-admin'>";
        div += "<a href='#' onclick=\"openWSE.LoadModalWindow(false, 'UserProfileImageUpdate-element', ''); setTimeout(function () { $('#UserProfileImageUpdate-element').remove(); }, 100); return false;\" class='ModalExitButton'></a>";
        div += "</div><span class='Modal-title'></span></div></div>";
        div += "<div class='ModalScrollContent'>";
        div += "<div class='ModalPadContent'>";
        div += "<h3 id='UserProfileImageUpload_loading' class='pad-all'>Loading. Please Wait...</h3>";
        div += "<iframe id='UserProfileImageUpload_frame' src='" + GetSiteRoot() + "SiteTools/iframes/UserProfileImageUpload.aspx' frameborder='0' style='visibility: hidden;' />";
        div += "</div></div>";
        div += "<div class='ModalButtonHolder'><input type='button' class='input-buttons no-margin' value='Close' onclick=\"openWSE.LoadModalWindow(false, 'UserProfileImageUpdate-element', ''); setTimeout(function () { $('#UserProfileImageUpdate-element').remove(); }, 100); return false;\" /></div>";
        div += "</div></div></div></div>";

        if ($("#UserProfileImageUpdate-element").length > 0) {
            $("#UserProfileImageUpdate-element").remove();
        }

        $("body").append(div);

        $("#UserProfileImageUpload_frame").one("load", (function () {
            $("#UserProfileImageUpdate-element").find("#UserProfileImageUpload_loading").remove();
            $("#UserProfileImageUpload_frame").css({
                visibility: "visible"
            });
        }));

        LoadModalWindow(true, "UserProfileImageUpdate-element", "Update Profile Image");
        CloseTopDropDowns();
    }

    /* A replacement function for $.getScript */
    function GetScriptFunction(loc, callback) {
        if (loc && $("head").find("script[src='" + loc + "']").length == 0) {
            var scriptTag = document.createElement("script");
            scriptTag.type = "text/javascript";
            if (callback && typeof (callback) == "function") {
                scriptTag.onload = function () {
                    window.setTimeout(function () {
                        callback();
                    }, 1);
                }
            }
            scriptTag.src = loc;
            document.getElementsByTagName("head")[0].appendChild(scriptTag);
        }
        else if (callback && typeof (callback) == "function") {
            try {
                window.setTimeout(function () {
                    try {
                        callback();
                    }
                    catch (evt) { }
                }, 500);
            }
            catch (evt) {
                openWSE.AlertWindow(evt.message, loc);
            }
        }
    }


    /* Site Menu Tab Functions */
    function InitializeSiteMenuTabs() {
        var overwriteEvent = false;
        if ($("#MainContent_pnlLinkBtns").attr("data-overwriteevent") === "true") {
            overwriteEvent = true;
        }

        if ($(".sitemenu-selection").length > 0 && ($(".pnl-section").length > 0 || overwriteEvent)) {
            ValidateSiteTabLinks();
            if (!overwriteEvent) {
                openWSE.LoadSiteMenuTab(window.location.href == "" ? "1" : window.location.href);

                $(".sitemenu-selection").find("li").on("click", function () {
                    openWSE.LoadSiteMenuTab($(this).find("a").attr("href"));
                    if (openWSE.ConvertBitToBoolean($(this).closest(".sitemenu-selection").attr("data-nohrefupdate"))) {
                        return false;
                    }
                });

                $(window).hashchange(function () {
                    var url = window.location.href;
                    openWSE.LoadSiteMenuTab(url == "" ? "1" : url);
                });
            }
            else {
                $(".sitemenu-selection").find("li").on("click", function () {
                    loadingPopup.Message("Loading Tab...");
                });
            }
        }
        else if ($("#MainContent_pnlLinkBtns").length > 0) {
            $("#MainContent_pnlLinkBtns").hide();
        }
        else if ($(".sitemenu-selection").length > 0) {
            $(".sitemenu-selection").hide();
        }
    }
    function ValidateSiteTabLinks() {
        if ($(".pnl-section").length == 1) {
            if (($(".sitemenu-selection").length == 0 || $(".sitemenu-selection").parent().attr("id") == "MainContent_pnlLinkBtns") && $("#MainContent_pnlLinkBtns").length > 0) {
                $("#MainContent_pnlLinkBtns").hide();
            }
            else {
                $(".sitemenu-selection").hide();
            }
        }
        else {
            var arrLIToremove = new Array();
            $(".sitemenu-selection").find("li").each(function (index) {
                if ($(this).find("a").length > 0) {
                    var tabName = $(this).find("a").attr("href").replace("#?tab=", "");
                    if (tabName.toLowerCase().indexOf("appmanager.aspx") == -1) {
                        if (tabName.indexOf("&") != -1) {
                            tabName = tabName.replace(tabName.substring(tabName.indexOf("&")), "");
                        }

                        if (tabName && $("#" + tabName).length === 0) {
                            arrLIToremove.push(index);
                        }
                    }
                }
            });

            for (var i = arrLIToremove.length - 1; i >= 0; i--) {
                $(".sitemenu-selection").find("li").eq(arrLIToremove[i]).remove();
            }
        }
    }
    function LoadSiteMenuTab(url) {
        $(".pnl-section").hide();
        $(".sitemenu-selection").find("li").removeClass("active");

        var tabName = getUrlParameterByName("tab", url);
        if (!tabName) {
            tabName = getParameterByName("tab");
        }

        if (tabName.indexOf("?tab=") > 0) {
            var splitTabName = tabName.split("?tab=");
            if (splitTabName.length > 0) {
                tabName = splitTabName[splitTabName.length - 1];
            }
        }
        else if (tabName.indexOf("&tab=") > 0) {
            var splitTabName = tabName.split("&tab=");
            if (splitTabName.length > 0) {
                tabName = splitTabName[splitTabName.length - 1];
            }
        }

        var index = GetPnlSectionIndex(tabName);
        if (index < 0) {
            index = 0;
        }

        CollapseAllAppCategories();

        $(".has-sub-links").each(function () {
            $(this).removeClass("expanded-links");
            $(this).find(".expand-collapse-btn").closest(".app-icon-links").parent().find("." + $(this).attr("data-subdiv")).hide();
        });

        $(".pnl-section").eq(index).show();
        $(".sitemenu-selection").find("li").eq(index).addClass("active");
        GetCurrentPage();
    }
    function GetPnlSectionIndex(ele) {
        if (ele.indexOf("&") != -1) {
            ele = ele.replace(ele.substring(ele.indexOf("&")), "");
        }

        if ($(".pnl-section[id='" + ele + "']").length == 0) {
            ele = "pnl_" + ele;
        }

        for (var i = 0; i < $(".pnl-section").length; i++) {
            if ($(".pnl-section").eq(i).attr("id") === ele) {
                return i;
            }
        }
        return 0;
    }

    function ScrollToElement(toEle, showAnimation) {
        setTimeout(function () {
            if (toEle.indexOf("#") == -1 && toEle.indexOf(".") == -1) {
                toEle = "#" + toEle;
            }

            if ($(toEle).length > 0) {
                var $mainDiv = $("#main_container");
                if ($mainDiv.length === 0) {
                    $mainDiv = $("body");
                }

                var elePos = $(toEle).offset().top;
                if (elePos > $(toEle).outerHeight()) {
                    elePos = elePos - $(toEle).outerHeight();
                }

                if (showAnimation) {
                    $mainDiv.animate({
                        scrollTop: elePos
                    }, openWSE_Config.animationSpeed);
                }
                else {
                    $mainDiv.scrollTop(elePos);
                }
            }
        }, 100);
    }

    /* Table Formula code for both Table Imports and Custom Tables */
    var TableFormulas = function () {

        function SetSummaryData(id) {
            if ($("#div_" + id + "_summaryholder").length > 0) {
                var $table = $("#" + id + "-complete-table");
                var isMobileView = IsMobileViewMode(id);

                $("#div_" + id + "_summaryholder").find(".custom-table-summary-item").each(function () {
                    var $valueHolder = $(this).find(".custom-table-summary-value");

                    var formulaType = $(this).attr("data-formulatype");
                    var formula = GetFormula(formulaType);
                    formulaType = GetFormulaType(formulaType, formula).toLowerCase().replace(/if/g, "");
                    formula = BuildFormulaConditions(formula);

                    var summaryValues = BuildSummaryArray($(this).attr("data-summarycolumn"), $table, isMobileView, formula);

                    try {
                        var finalVal = FormulaFunctions[formulaType](summaryValues);
                        $valueHolder.html(finalVal);
                    }
                    catch (evt) {
                        $valueHolder.html("-");
                    }
                });
            }
        }

        /* -- Formula Functions Below -- */
        var FormulaFunctions = {
            sum: function (summaryValues) {
                var finalVal = 0;
                for (var i = 0; i < summaryValues.length; i++) {
                    finalVal += summaryValues[i];
                }

                return +finalVal.toFixed(2);
            },
            min: function (summaryValues) {
                return Math.min.apply(null, summaryValues);
            },
            max: function (summaryValues) {
                return Math.max.apply(null, summaryValues);
            },
            average: function (summaryValues) {
                var finalVal = 0;
                for (var i = 0; i < summaryValues.length; i++) {
                    finalVal += summaryValues[i];
                }

                finalVal = finalVal / summaryValues.length;
                return +finalVal.toFixed(2);
            }
        };

        function BuildSummaryArray(summaryColumn, _table, isMobileView, formula) {
            var summaryValues = new Array();

            var $dataColumn = $(_table).find(".data-td[data-columnname='" + summaryColumn + "']");
            if (isMobileView) {
                $dataColumn = $(_table).find(".headerName[data-columnname='" + summaryColumn + "']").parent().find(".data-td");
            }

            if ($dataColumn.length > 0) {
                for (var i = 0; i < $dataColumn.length; i++) {
                    if (CanAddSummaryItem(_table, isMobileView, formula, i)) {
                        var dataType = $dataColumn.eq(i).attr("data-type");

                        if (dataType) {
                            var val = 0;
                            if (isMobileView) {
                                var $tempDataColumn = $dataColumn.eq(i).parent().find(".data-td");
                                val = $.trim($tempDataColumn.html());
                                if ($tempDataColumn.find(".td-columnValue-edit").length > 0) {
                                    val = $.trim($tempDataColumn.find(".td-columnValue-edit").html());
                                }
                            }
                            else {
                                val = $.trim($dataColumn.eq(i).html());
                                if ($dataColumn.eq(i).find(".td-columnValue-edit").length > 0) {
                                    val = $.trim($dataColumn.eq(i).find(".td-columnValue-edit").html());
                                }
                            }

                            if (val) {
                                switch (dataType.toLowerCase()) {
                                    case "int32":
                                    case "number":
                                    case "integer":
                                        summaryValues.push(parseInt(val));
                                        break;

                                    case "float":
                                    case "decimal":
                                        if (parseFloat(val) != NaN) {
                                            summaryValues.push(parseFloat(val));
                                        }
                                        else if (parseInt(val) != NaN) {
                                            summaryValues.push(parseInt(val));
                                        }
                                        break;

                                    case "money":
                                        val = val.replace(/\$/g, "").replace(/,/g, "").replace(/ /g, "");
                                        if (parseFloat(val) != NaN) {
                                            summaryValues.push(parseFloat(val));
                                        }
                                        else if (parseInt(val) != NaN) {
                                            summaryValues.push(parseInt(val));
                                        }
                                        break;

                                    default:
                                        if (!isNaN(val)) {
                                            if (val.indexOf(".") != -1) {
                                                val = val.replace(/\$/g, "").replace(/,/g, "").replace(/ /g, "");
                                                if (parseFloat(val) != NaN) {
                                                    summaryValues.push(parseFloat(val));
                                                }
                                                else if (parseInt(val) != NaN) {
                                                    summaryValues.push(parseInt(val));
                                                }
                                            }
                                            else {
                                                summaryValues.push(parseInt(val));
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return summaryValues;
        }

        function CanAddSummaryItem(_table, isMobileView, formula, i) {
            var canAdd = true;

            if (formula.columnName && formula.conditions && formula.conditions.length > 0) {
                var $formulaColum = $(_table).find(".data-td[data-columnname='" + formula.columnName + "']");
                if (isMobileView) {
                    $formulaColum = $(_table).find(".headerName[data-columnname='" + formula.columnName + "']").parent().find(".data-td");
                }

                if ($formulaColum.eq(i).length > 0) {
                    var dataType = $formulaColum.eq(i).attr("data-type");
                    if (dataType) {
                        var val = "";
                        if (isMobileView) {
                            var $tempDataColumn = $formulaColum.eq(i).parent().find(".data-td");
                            val = $.trim($tempDataColumn.html());
                            if ($tempDataColumn.find(".td-columnValue-edit").length > 0) {
                                val = $.trim($tempDataColumn.find(".td-columnValue-edit").html());
                            }
                            else if (val.indexOf("<input") == 0 && $formulaColum.eq(i).find("input[type='checkbox']").length > 0) {
                                val = $formulaColum.eq(i).find("input[type='checkbox']").is(":checked");
                            }
                        }
                        else {
                            val = $.trim($formulaColum.eq(i).html());
                            if ($formulaColum.eq(i).find(".td-columnValue-edit").length > 0) {
                                val = $.trim($formulaColum.eq(i).find(".td-columnValue-edit").html());
                            }
                            else if (val.indexOf("<input") == 0 && $formulaColum.eq(i).find("input[type='checkbox']").length > 0) {
                                val = $formulaColum.eq(i).find("input[type='checkbox']").is(":checked");
                            }
                        }

                        val = GetConditionValueForSummaryItem(val, dataType);
                        for (var j = 0; j < formula.conditions.length; j++) {
                            var conditionVal = GetConditionValueForSummaryItem(formula.conditions[j].value, dataType);
                            canAdd = IsValidCondition(formula.conditions[j].condition, conditionVal, val);
                            if (!canAdd && formula.conditions[j].conditionSeperator === "&&") {
                                break;
                            }
                        }
                    }
                }
            }

            return canAdd;
        }
        function GetConditionValueForSummaryItem(val, dataType) {
            switch (dataType.toLowerCase()) {
                case "int32":
                case "number":
                case "integer":
                    val = parseInt(val);
                    break;

                case "float":
                case "decimal":
                    if (parseFloat(val) != NaN) {
                        val = parseFloat(val);
                    }
                    else if (parseInt(val) != NaN) {
                        val = parseInt(val);
                    }
                    break;

                case "boolean":
                case "bool":
                case "bit":
                    val = openWSE.ConvertBitToBoolean(val);
                    break;

                case "money":
                    val = val.replace(/\$/g, "").replace(/,/g, "").replace(/ /g, "");
                    if (parseFloat(val) != NaN) {
                        val = parseFloat(val);
                    }
                    else if (parseInt(val) != NaN) {
                        val = parseInt(val);
                    }
                    break;

                default:
                    if (!isNaN(val)) {
                        if (val.indexOf(".") != -1) {
                            val = val.replace(/\$/g, "").replace(/,/g, "").replace(/ /g, "");
                            if (parseFloat(val) != NaN) {
                                val = parseFloat(val);
                            }
                            else if (parseInt(val) != NaN) {
                                val = parseInt(val);
                            }
                        }
                        else {
                            val = parseInt(val);
                        }
                    }
                    break;
            }

            return val;
        }
        function IsValidCondition(condition, conditionVal, value) {
            if (condition == ">=") {
                if (value >= conditionVal) {
                    return true;
                }
            }
            else if (condition == "<=") {
                if (value <= conditionVal) {
                    return true;
                }
            }
            else if (condition == "!=") {
                if (value != conditionVal) {
                    return true;
                }
            }
            else if (condition == ">") {
                if (value > conditionVal) {
                    return true;
                }
            }
            else if (condition == "<") {
                if (value < conditionVal) {
                    return true;
                }
            }
            else {
                if (value == conditionVal) {
                    return true;
                }
            }

            return false;
        }


        function GetFormula(formulaType) {
            var formula = "";
            if (formulaType.indexOf("(") != -1) {
                formula = formulaType.substring(formulaType.indexOf("(") + 1);
                if (formula.lastIndexOf(")") === formula.length - 1) {
                    formula = formula.substring(0, formula.length - 1);
                }
            }
            
            return formula;
        }
        function GetFormulaType(formulaType, formula) {
            return formulaType.replace("(" + formula + ")", "");
        }


        /* -- Get Formula Condition Parts -- */
        function BuildFormulaConditions(formula) {
            var formulaConditions = {
                "columnName": "",
                "conditions": []
            }

            if (formula) {
                formula = $.trim(unescape(formula));
                var formulaSplit = formula.split(",");
                if (formulaSplit.length === 2) {
                    var conditionsArray = new Array();
                    var formulaPart1 = GetFormulaSectionsWithoutQuotes(formulaSplit[1]);

                    if (formulaPart1.indexOf("||") > 0) {
                        conditionsArray = AddFormulaPart(formulaPart1, "||", conditionsArray);
                    }
                    else {
                        conditionsArray = AddFormulaPart(formulaPart1, "&&", conditionsArray);
                    }

                    formulaConditions = {
                        "columnName": GetFormulaSectionsWithoutQuotes(formulaSplit[0]),
                        "conditions": conditionsArray
                    }
                }
            }

            return formulaConditions;
        }
        function AddFormulaPart(formulaPart1, conditionSeperator, conditionsArray) {
            var orSplit = formulaPart1.split(conditionSeperator);
            for (var i = 0; i < orSplit.length; i++) {
                var x = $.trim(orSplit[i]);
                var condition = GetFormulaCondition(x);
                var conditionSplit = x.split(condition);
                if (conditionSplit.length > 0) {
                    conditionsArray.push({
                        "condition": condition,
                        "value": $.trim(conditionSplit[conditionSplit.length - 1]),
                        "conditionSeperator": conditionSeperator
                    });
                }
            }

            return conditionsArray;
        }
        function GetFormulaCondition(part) {
            var condition = "=";
            if (part.indexOf(">=") >= 0) {
                condition = ">=";
            }
            else if (part.indexOf("<=") >= 0) {
                condition = "<=";
            }
            else if (part.indexOf("!=") >= 0 || part.indexOf("<>") >= 0) {
                condition = "!=";
            }
            else if (part.indexOf(">") >= 0) {
                condition = ">";
            }
            else if (part.indexOf("<") >= 0) {
                condition = "<";
            }

            return condition;
        }
        function GetFormulaSectionsWithoutQuotes(formulaVal) {
            formulaVal = $.trim(formulaVal);
            if (formulaVal.indexOf("\"") === 0) {
                formulaVal = formulaVal.substring(1);
            }

            if (formulaVal.lastIndexOf("\"") === formulaVal.length - 1) {
                formulaVal = formulaVal.substring(0, formulaVal.length - 1);
            }

            return $.trim(formulaVal);
        }


        function BuildSummaryHolder(id) {
            var summaryHolder = "";
            var summaryData = unescape($("#hf_" + id + "_summaryData").val());
            if (summaryData) {
                summaryHolder += "<div id='div_" + id + "_summaryholder' class='customtables-summary-margin'>";

                var summaryArray = $.parseJSON(summaryData);
                for (var i = 0; i < summaryArray.length; i++) {
                    summaryHolder += "<div class='custom-table-summary-item' data-summarycolumn='" + summaryArray[i].columnName.replace(/&nbsp;/g, " ") + "' data-formulatype='" + summaryArray[i].formulaType.replace(/&nbsp;/g, " ") + "'>";
                    summaryHolder += "<div class='custom-table-summary-name'>" + summaryArray[i].summaryName.replace(/&nbsp;/g, " ") + "</div>";
                    summaryHolder += "<div class='custom-table-summary-value'>" + "</div>";
                    summaryHolder += "<div class='clear'></div></div>";
                }

                summaryHolder += "<div class='clear'></div></div>";
            }

            return summaryHolder;
        }

        function IsMobileViewMode(id) {
            if (isMobileDevice() || $("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                return true;
            }

            return false;
        }

        return {
            SetSummaryData: SetSummaryData,
            BuildSummaryHolder: BuildSummaryHolder,
            IsMobileViewMode: IsMobileViewMode,
            GetFormula: GetFormula,
            GetFormulaType: GetFormulaType
        }
    }();

    /* Gridview methods being used when using the TableBuilder class to create a new table */
    var GridViewMethods = function () {
        var pageIndexArray = new Array();
        var pageSortArray = new Array();
        var searchInfoArray = new Array();
        var rowClickEventActive = false;

        function InitializeTable(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                StartTableSearch($gridview);
                SetSortColumnData($gridview);
                MoveInsertRow($gridview);

                if (rowClickEventActive) {
                    $gridview.find("tr.myItemStyle").unbind("click");
                    $gridview.find("tr.myItemStyle").bind("click", function (e) {
                        if (e && e.target && (e.target.tagName.toLowerCase() == "tr" || e.target.tagName.toLowerCase() == "td")) {
                            if ($(this).hasClass("activeClick")) {
                                $(this).removeClass("activeClick");
                            }
                            else {
                                $gridview.find("tr.myItemStyle").removeClass("activeClick");
                                $(this).addClass("activeClick");
                            }
                        }
                        else {
                            $gridview.find("tr.myItemStyle").removeClass("activeClick");
                        }
                    });
                }
            }
        }

        function PageSizeChange(ele) {
            var $gridview = $(ele).closest(".gridview-table-holder").find(".gridview-table");

            if ($gridview.length > 0) {
                UpdatePageSizeCookie($gridview, $gridview.attr("data-cookiename"));
                GoToPageNumber_Click($gridview.attr("data-tableid"));
            }
        }
        function UpdatePageSizeCookie(gridview, cookieName) {
            try {
                var pageSize = gridview.closest(".gridview-table-holder").find(".table-pagesize-selector > select").val();
                openWSE.AjaxCall("WebServices/SaveControls.asmx/SetCookie", "{ 'name': '" + cookieName + "', 'value': '" + pageSize + "'}");
            }
            catch (evt) { }
        }

        function SetPageSize(gridview) {
            gridview.find("tr.myItemStyle:not(.search-hide)").each(function () {
                $(this).removeClass("hide-table-row");
            });

            var pageSize = gridview.closest(".gridview-table-holder").find(".table-pagesize-selector > select").val();
            if (pageSize === "all") {
                pageSize = gridview.find("tr.myItemStyle:not(.search-hide)").length;
            }

            pageSize = parseInt(pageSize);

            var pageIndex = GetPageIndexItem(gridview);
            if (!pageIndex) {
                pageIndex = 0;
            }
            else {
                pageIndex = parseInt(pageIndex);
            }

            var pageCount = gridview.find("tr.myItemStyle:not(.search-hide)").length / pageSize;
            if (pageCount.toString().indexOf(".") !== -1) {
                pageCount = pageCount.toString().replace(pageCount.toString().substring(pageCount.toString().indexOf(".")), "");
                pageCount = parseInt(pageCount) + 1;
            }

            var startIndex = pageIndex * pageSize;
            startIndex = startIndex >= 0 ? startIndex : 0;
            var maxCount = startIndex + pageSize;

            gridview.find("tr.myItemStyle:not(.search-hide)").each(function (index) {
                if (index < startIndex || index >= maxCount) {
                    $(this).addClass("hide-table-row");
                }
            });

            var tableId = gridview.attr("data-tableid");

            var firstPage = "<span class=\"pg-first-btn GridViewPager-Btns cursor-pointer\" onclick=\"openWSE.GridViewMethods.FirstPage_Click('" + tableId + "');\"></span>";
            var prevPage = "<span class=\"pg-prev-btn GridViewPager-Btns cursor-pointer\" onclick=\"openWSE.GridViewMethods.PrevPage_Click('" + tableId + "');\"></span>";
            var nextPage = "<span class=\"pg-next-btn GridViewPager-Btns cursor-pointer\" onclick=\"openWSE.GridViewMethods.NextPage_Click('" + tableId + "');\"></span>";
            var lastPage = "<span class=\"pg-last-btn GridViewPager-Btns cursor-pointer\" onclick=\"openWSE.GridViewMethods.LastPage_Click('" + tableId + "');\"></span>";

            var minPageCount = 1;
            if (pageCount === 0) {
                minPageCount = 0;
            }

            var pageIndexTextbox = "<span class=\"float-right margin-top-sml\">of " + pageCount + "</span><input class=\"input-buttons float-right no-margin\" type=\"button\" value=\"Go\" onclick=\"openWSE.GridViewMethods.GoToPageNumber_Click('" + tableId + "');\" style=\"margin-top: -1px!important; margin-right: 5px!important;\" /><input type=\"number\" class=\"textEntry page-index-textbox margin-right-sml float-right\" min=\"" + minPageCount + "\" max=\"" + pageCount + "\" style=\"width: 60px;\" />";

            if (pageCount === 0) {
                pageIndex = -1;
            }

            gridview.find(".gridview-pager-holder").html(lastPage + nextPage + pageIndexTextbox + prevPage + firstPage);
            gridview.find(".gridview-pager-holder").find(".page-index-textbox").val(pageIndex + 1);

            gridview.find(".gridview-pager-holder").show();
            if (pageCount <= 1) {
                gridview.find(".gridview-pager-holder").hide();
            }

            var rows = gridview.find("tr.myItemStyle:not(.search-hide)").get();
            var currentPageView = (pageIndex * pageSize) + 1;
            var outOf = (currentPageView + pageSize) - 1;
            if (outOf > rows.length) {
                outOf = rows.length;
            }

            if (currentPageView < 0) {
                currentPageView = 0;
            }

            gridview.find(".table-pagesize-outof").html("View " + currentPageView + " - " + outOf + " of " + rows.length);
        }

        function SetSortColumnData(gridview) {
            var sortData = GetPageSortItem(gridview);

            var $thisColumn = gridview.find(".myHeaderStyle").find("td[data-columnname='" + sortData.column + "']");
            if ($thisColumn.length > 0) {
                gridview.find(".myHeaderStyle").find("td").each(function () {
                    $(this).removeClass("asc");
                    $(this).removeClass("desc");
                    $(this).removeClass("active");
                });

                $thisColumn.addClass("active");
                $thisColumn.addClass(sortData.dir.toLowerCase());

                SortData(gridview, sortData);
            }
            else if (openWSE.ConvertBitToBoolean(gridview.attr("data-allowpaging"))) {
                SetPageSize(gridview);
            }

            MoveInsertRow(gridview);
        }
        function SortData(gridview, sortData) {
            var rows = gridview.find("tr.myItemStyle:not(.search-hide)").get();
            rows.sort(function (a, b) {
                var $a = $(a).find("td[data-columnname='" + sortData.column + "']");
                var $b = $(b).find("td[data-columnname='" + sortData.column + "']");

                var x = "";
                var y = "";

                if ($a.find("input[type='checkbox']").length > 0) {
                    x = $a.find("input").prop("checked").toString();
                }
                else if ($a.find(".sort-value-class").length > 0) {
                    x = $a.find(".sort-value-class").attr("data-sortvalue");
                }
                else if ($a.find("input").length > 0) {
                    x = $a.find("input").val();
                    if (!x && $a.find("input").attr("data-value")) {
                        x = unescape($a.find("input").attr("data-value"));
                    }
                }
                else if ($a.find("select").length > 0) {
                    x = $a.find("select").val();
                }
                else {
                    x = $a.text();
                }

                if ($b.find("input[type='checkbox']").length > 0) {
                    y = $b.find("input").prop("checked").toString();
                }
                else if ($b.find(".sort-value-class").length > 0) {
                    y = $b.find(".sort-value-class").attr("data-sortvalue");
                }
                else if ($b.find("input").length > 0) {
                    y = $b.find("input").val();
                    if (!y && $b.find("input").attr("data-value")) {
                        y = unescape($b.find("input").attr("data-value"));
                    }
                }
                else if ($b.find("select").length > 0) {
                    y = $b.find("select").val();
                }
                else {
                    y = $b.text();
                }

                x = $.trim(x);
                y = $.trim(y);

                if (!isNaN(x) && !isNaN(y)) {
                    if (x.indexOf(".") !== -1) {
                        x = parseFloat(x);
                    }
                    else {
                        x = parseInt(x);
                    }

                    if (y.indexOf(".") !== -1) {
                        y = parseFloat(y);
                    }
                    else {
                        y = parseInt(y);
                    }
                }
                else if (new Date(x).toString() !== "Invalid Date" && new Date(y).toString() !== "Invalid Date") {
                    x = new Date(x);
                    y = new Date(y);
                }
                else {
                    x = x.toLowerCase();
                    y = y.toLowerCase();
                }

                if (sortData.dir == "desc") {
                    return ((x < y) ? -1 : ((x > y) ? 1 : 0));
                }

                return ((x > y) ? -1 : ((x < y) ? 1 : 0));
            });

            $.each(rows, function (index, row) {
                gridview.find("tbody > tr.myHeaderStyle").after(row);
            });

            var altRowClass = gridview.attr("data-altrowclass");
            if (altRowClass) {
                gridview.find("tr.myItemStyle:not(.search-hide)").removeClass("GridNormalRow");
                gridview.find("tr.myItemStyle:not(.search-hide)").removeClass(altRowClass);
            }

            gridview.find("tr.myItemStyle:not(.search-hide)").each(function (index) {
                if ($(this).find(".GridViewNumRow").length > 0) {
                    $(this).find(".GridViewNumRow").html(index + 1);
                }

                if (altRowClass) {
                    if (index % 2 !== 0) {
                        $(this).addClass(altRowClass);
                    }
                    else {
                        $(this).addClass("GridNormalRow");
                    }
                }
            });

            if (openWSE.ConvertBitToBoolean(gridview.attr("data-allowpaging"))) {
                SetPageSize(gridview);
            }
        }

        function MoveInsertRow(gridview) {
            if (gridview.find("tr.addItemRow").length > 0 && ConvertBitToBoolean(gridview.attr("data-putinsertattop"))) {
                $("tr.myHeaderStyle").after(gridview.find("tr.addItemRow"));
            }
        }

        function FirstPage_Click(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                UpdatePageIndexItem($gridview, 0);
                SetPageSize($gridview);
            }
        }
        function PrevPage_Click(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                var currentIndex = parseInt(GetPageIndexItem($gridview));
                if (currentIndex - 1 >= 0) {
                    UpdatePageIndexItem($gridview, currentIndex - 1);
                    SetPageSize($gridview);
                }
            }
        }
        function NextPage_Click(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                var pageCount = GetTotalPageCount($gridview);
                var currentIndex = parseInt(GetPageIndexItem($gridview));
                if (currentIndex + 1 < pageCount) {
                    UpdatePageIndexItem($gridview, currentIndex + 1);
                    SetPageSize($gridview);
                }
            }
        }
        function LastPage_Click(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                var pageCount = GetTotalPageCount($gridview);
                UpdatePageIndexItem($gridview, pageCount - 1);
                SetPageSize($gridview);
            }
        }

        function GetPageIndexItem(gridview) {
            var tableId = gridview.attr("data-tableid");
            if (tableId) {
                for (var i = 0; i < pageIndexArray.length; i++) {
                    if (pageIndexArray[i].name === tableId) {
                        if (parseInt(pageIndexArray[i].value) > (GetTotalPageCount(gridview) - 1)) {
                            return 0;
                        }
                        else {
                            return pageIndexArray[i].value;
                        }
                    }
                }
            }

            return 0;
        }
        function UpdatePageIndexItem(gridview, value) {
            var tableId = gridview.attr("data-tableid");
            if (tableId) {
                var foundItem = false;
                for (var i = 0; i < pageIndexArray.length; i++) {
                    if (pageIndexArray[i].name === tableId) {
                        pageIndexArray[i].value = value;
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem) {
                    pageIndexArray.push({
                        "name": tableId,
                        "value": value
                    });
                }
            }
        }

        function GetPageSortItem(gridview) {
            var tableId = gridview.attr("data-tableid");
            if (tableId) {
                for (var i = 0; i < pageSortArray.length; i++) {
                    if (pageSortArray[i].name === tableId) {
                        return pageSortArray[i];
                    }
                }
            }

            return {
                "name": gridview.attr("data-tableid"),
                "column": gridview.attr("data-initalsortcolumn"),
                "dir": gridview.attr("data-initalsortdir").toLowerCase()
            }
        }
        function UpdatePageSortItem(gridview, column, dir) {
            var tableId = gridview.attr("data-tableid");
            if (tableId) {
                var foundItem = false;
                for (var i = 0; i < pageSortArray.length; i++) {
                    if (pageSortArray[i].name === tableId) {
                        pageSortArray[i].column = column;
                        pageSortArray[i].dir = dir;
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem) {
                    pageSortArray.push({
                        "name": tableId,
                        "column": column,
                        "dir": dir
                    });
                }
            }
        }

        function GoToPageNumber_Click(tableId) {
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");

            if ($gridview.length > 0) {
                var currentIndex = parseInt($gridview.find(".page-index-textbox").val()) - 1;
                if (!isNaN(currentIndex)) {
                    var pageCount = GetTotalPageCount($gridview);
                    if (currentIndex < 0) {
                        currentIndex = 0;
                    }
                    else if (currentIndex >= pageCount) {
                        currentIndex = pageCount - 1;
                    }

                    UpdatePageIndexItem($gridview, currentIndex);
                }

                SetPageSize($gridview);
            }
        }

        function SortColumn_Click(ele) {
            var $this = $(ele);
            var columnName = $this.attr("data-columnname");
            var $gridview = $this.closest(".gridview-table");

            var sortDir = "asc";
            if ($this.hasClass("asc")) {
                sortDir = "desc";
            }

            UpdatePageSortItem($gridview, columnName, sortDir);
            StartTableSearch($gridview);
            SetSortColumnData($gridview);
        }

        function GetTotalPageCount(gridview) {
            var pageSize = gridview.closest(".gridview-table-holder").find(".table-pagesize-selector > select").val();
            if (pageSize !== "all") {
                pageSize = parseInt(pageSize);
                var pageCount = gridview.find("tr.myItemStyle:not(.search-hide)").length / pageSize;
                if (pageCount.toString().indexOf(".") !== -1) {
                    pageCount = pageCount.toString().replace(pageCount.toString().substring(pageCount.toString().indexOf(".")), "");
                    pageCount = parseInt(pageCount) + 1;
                }

                return pageCount;
            }

            return 1;
        }

        function OnInsertRow(tableId, functionCall) {
            functionCall = unescape(functionCall).replace(/&nbsp;/g, " ");
            var functionNames = functionCall.split(";");

            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");
            if ($gridview.length > 0 && $gridview.find(".addItemRow").length > 0) {
                $gridview.find(".addItemRow").find("input").blur();
            }

            for (var i = 0; i < functionNames.length; i++) {
                var functionNamePart = $.trim(functionNames[i]);
                if (functionNamePart) {
                    try {
                        eval(functionNamePart);
                    }
                    catch (evt) {
                        try {
                            var functionParts = functionNamePart.split("(");
                            if (functionParts.length > 1) {
                                var functionName = functionParts[0];
                                var functionArgs = functionParts[1].substring(0, functionParts[1].length - 1);

                                var fn = window[functionName];
                                if (typeof fn === "function") {
                                    fn(functionArgs);
                                }
                            }
                        }
                        catch (evt2) {
                            openWSE.LogConsoleMessage(evt2);
                        }
                    }
                }
            }
        }
        function OnInsertRow_KeyPress(event, tableId, functionCall) {
            if ((event.which == 13 || event.keyCode == 13) && functionCall) {
                OnInsertRow(tableId, functionCall);
                return false;
            }
        }

        function SearchTable(tableId) {
            var ele = "<div id='SearchTable-element' class='Modal-element' data-tableid='" + tableId + "' style='display: none;'>";
            ele += "<div class='Modal-overlay' onclick=\"openWSE.GridViewMethods.SearchOverlayClick(event, '" + tableId + "');\">";
            ele += "<div class='Modal-element-align'>";
            ele += "<div class='Modal-element-modal' data-setwidth='370'>";

            // Header
            ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
            ele += "<a href='#' onclick=\"openWSE.GridViewMethods.CloseSearchTable('" + tableId + "');return false;\" class='ModalExitButton'></a>";
            ele += "</div><span class='Modal-title'></span></div></div>";

            // Body
            var searchButton = "<input class='input-buttons modal-ok-btn' type='button' value='Find' onclick=\"openWSE.GridViewMethods.InitialSearch('" + tableId + "');\" />";
            var closeButton = "<input class='input-buttons modal-cancel-btn' type='button' value='Close' onclick=\"openWSE.GridViewMethods.CloseSearchTable('" + tableId + "');\" />";
            var clearButton = "<input class='input-buttons modal-ok-btn' type='button' value='Clear' onclick=\"openWSE.GridViewMethods.ClearSearch('" + tableId + "', true);\" />";

            var str = "<div class=\"input-settings-holder\"><span class=\"font-bold\">Filter</span><div class='clear-space-two'></div>";
            str += "<select class=\"search-filter\">";
            str += "<option value=\"contains\">Contains</option>";
            str += "<option value=\"startswith\">Starts with</option>";
            str += "<option value=\"endswith\">Ends with</option>";
            str += "<option value=\"equals\">Equals</option>";
            str += "<option value=\"notequals\">Does not equal</option>";
            str += "</select>";
            str += "</div>";
            str += "<div class=\"input-settings-holder\"><span class=\"font-bold\">Text</span><div class='clear-space-two'></div><input type=\"text\" class=\"textEntry search-text\" onkeypress=\"openWSE.GridViewMethods.InitialSearch_KeyPress(event, '" + tableId + "');\" />";
            str += "</div>";
            str += "<input type=\"checkbox\" id=\"cb_search-match-case\" class=\"search-match-case\" /><label for=\"cb_search-match-case\">Match Case</label>";
            str += "<div class=\"clear\"></div>";

            ele += "<div class='ModalPadContent'>" + str + "</div>";
            ele += "<div class='ModalButtonHolder'>" + searchButton + clearButton + closeButton + "<div class='clear'></div></div>";
            ele += "</div></div></div></div>";

            $("body").append(ele);
            LoadModalWindow(true, "SearchTable-element", "Search Table");
            setTimeout(function () {
                var $searchModal = $("#SearchTable-element[data-tableid='" + tableId + "']");
                if ($searchModal.length > 0) {
                    var searchCriteria = GetSearchCriteria(tableId);
                    $searchModal.find(".search-filter").val(searchCriteria.filter);
                    $searchModal.find(".search-text").val(searchCriteria.text);
                    $searchModal.find(".search-match-case").prop("checked", searchCriteria.matchCase);
                    $searchModal.find(".search-text").focus();
                }
            }, openWSE_Config.animationSpeed);
        }
        function InitialSearch(tableId) {
            var $searchModal = $("#SearchTable-element[data-tableid='" + tableId + "']");
            if ($searchModal.length > 0) {
                var searchFilter = $searchModal.find(".search-filter").val();
                var searchText = $.trim($searchModal.find(".search-text").val());
                var searchmatchCase = $searchModal.find(".search-match-case").is(":checked");

                var foundArrayItem = false;
                for (var i = 0; i < searchInfoArray.length; i++) {
                    if (searchInfoArray[i].tableId == tableId) {
                        searchInfoArray[i].filter = searchFilter;
                        searchInfoArray[i].text = searchText;
                        searchInfoArray[i].matchCase = searchmatchCase;
                        foundArrayItem = true
                        break;
                    }
                }

                if (!foundArrayItem) {
                    searchInfoArray.push({
                        "tableId": tableId,
                        "filter": searchFilter,
                        "text": searchText,
                        "matchCase": searchmatchCase
                    });
                }

                InitializeTable(tableId);
            }
        }
        function InitialSearch_KeyPress(event, tableId) {
            if (event && (event.which == 13 || event.keyCode == 13)) {
                event.preventDefault();
                InitialSearch(tableId);
            }
        }

        function StartTableSearch(gridview) {
            if (gridview.length > 0) {
                var tableId = gridview.attr("data-tableid");

                var searchCriteria = GetSearchCriteria(tableId);
                var searchFilter = searchCriteria.filter;
                var searchText = searchCriteria.text;
                var searchmatchCase = searchCriteria.matchCase;

                if (searchText) {
                    gridview.find("tr.myItemStyle").removeClass("search-hide");
                    if (!searchmatchCase) {
                        searchText = searchText.toLowerCase();
                    }

                    for (var i = 0; i < gridview.find("tr.myItemStyle").length; i++) {
                        var $thisRow = gridview.find("tr.myItemStyle").eq(i);

                        var rowData = new Array();

                        $thisRow.find("td").each(function () {
                            var $thisCell = $(this);
                            var dataColumnName = $thisCell.attr("data-columnname");
                            if (dataColumnName) {
                                var x = "";
                                if ($thisCell.find("input[type='checkbox']").length > 0) {
                                    x = $thisCell.find("input").prop("checked").toString();
                                }
                                else if ($thisCell.find("input").length > 0) {
                                    x = $thisCell.find("input").val();
                                    if (!x && $thisCell.find("input").attr("data-value")) {
                                        x = unescape($thisCell.find("input").attr("data-value"));
                                    }
                                }
                                else if ($thisCell.find("select").length > 0) {
                                    x = $thisCell.find("select").val();
                                }
                                else if ($thisCell.find(".sort-value-class").length > 0) {
                                    x = $thisCell.find(".sort-value-class").attr("data-sortvalue");
                                }
                                else {
                                    x = $thisCell.text();
                                }

                                if (x) {
                                    rowData.push(x);
                                }
                            }
                        });

                        var hideRow = true;
                        for (var j = 0; j < rowData.length; j++) {
                            var rowDataValue = rowData[j];
                            if (!searchmatchCase) {
                                rowDataValue = rowDataValue.toLowerCase();
                            }

                            if (searchFilter === "startswith") {
                                if (rowDataValue.indexOf(searchText) === 0) {
                                    hideRow = false;
                                    break;
                                }
                            }
                            else if (searchFilter === "endswith") {
                                if (rowDataValue.indexOf(searchText) !== -1 && rowDataValue.indexOf(searchText) === rowDataValue.length - searchText.length) {
                                    hideRow = false;
                                    break;
                                }
                            }
                            else if (searchFilter === "equals") {
                                if (rowDataValue === searchText) {
                                    hideRow = false;
                                    break;
                                }
                            }
                            else if (searchFilter === "notequals") {
                                if (rowDataValue !== searchText) {
                                    hideRow = false;
                                    break;
                                }
                            }
                            else {
                                if (rowDataValue.indexOf(searchText) !== -1) {
                                    hideRow = false;
                                    break;
                                }
                            }
                        }

                        if (hideRow) {
                            $thisRow.addClass("search-hide");
                        }
                    }

                    if (gridview.find(".emptyGridView-search-td").length > 0) {
                        gridview.find(".emptyGridView-search-td").parent().remove();
                    }

                    if (gridview.find(".search-hide").length === gridview.find("tr.myItemStyle").length && gridview.find("tr.addItemRow").length === 0) {
                        gridview.find(".myHeaderStyle").after("<tr><td class=\"emptyGridView-search-td\" colspan=\"" + gridview.attr("data-columnspan") + "\"><div class=\"emptyGridView\">No search results found</div></td></tr>");

                        if (gridview.find("tbody").find(".emptyGridView-td").length > 0) {
                            gridview.find("tbody").find(".emptyGridView-td").parent().hide();
                            gridview.find("tbody").find(".emptyGridView-td").parent().attr("data-hidden", "true");
                        }
                    }
                }
                else {
                    ClearSearch(tableId, false);
                }
            }
        }
        function CloseSearchTable(tableId) {
            $("#SearchTable-element[data-tableid='" + tableId + "']").remove();

            $("body").removeClass("modal-fixed-position-body");
            if ($("#site_mainbody").length > 0 && $("#site_mainbody").attr("data-layoutoption") === "Boxed") {
                $(".fixed-container-border-left").find(".Modal-overlay").remove();
                $(".fixed-container-border-right").find(".Modal-overlay").remove();
                $(".fixed-footer-container-left").find(".Modal-overlay").remove();
                $(".fixed-footer-container-right").find(".Modal-overlay").remove();
            }
        }
        function SearchOverlayClick(event, tableId) {
            if (event && event.target && event.target.className == "Modal-overlay") {
                CloseSearchTable(tableId);
            }
        }
        function ClearSearch(tableId, reInitializeTable) {
            $("#SearchTable-element[data-tableid='" + tableId + "']").find(".search-text").val("");
            $("#SearchTable-element[data-tableid='" + tableId + "']").find(".search-match-case").prop("checked", false)
            var $gridview = $(".gridview-table[data-tableid='" + tableId + "']");
            if ($gridview.length > 0) {
                if ($gridview.find("tbody").find(".emptyGridView-search-td").length > 0) {
                    $gridview.find("tbody").find(".emptyGridView-search-td").parent().remove();
                }

                if ($gridview.find("tbody").find(".emptyGridView-td").length > 0) {
                    if ($gridview.find("tbody").find(".emptyGridView-td").parent().attr("data-hidden") == "true") {
                        $gridview.find("tbody").find(".emptyGridView-td").parent().show();
                        $gridview.find("tbody").find(".emptyGridView-td").parent().attr("data-hidden", "");
                    }
                }

                $gridview.find("tr.myItemStyle").removeClass("search-hide");

                for (var i = 0; i < searchInfoArray.length; i++) {
                    if (searchInfoArray[i].tableId == tableId) {
                        searchInfoArray[i].filter = "contains";
                        searchInfoArray[i].text = "";
                        searchInfoArray[i].matchCase = false;
                        break;
                    }
                }

                if (reInitializeTable) {
                    InitializeTable(tableId);
                }
            }
        }

        function GetSearchCriteria(tableId) {
            for (var i = 0; i < searchInfoArray.length; i++) {
                if (searchInfoArray[i].tableId == tableId) {
                    return searchInfoArray[i];
                }
            }

            return {
                "tableId": tableId,
                "filter": "contains",
                "text": "",
                "matchCase": false
            };
        }

        return {
            InitializeTable: InitializeTable,
            PageSizeChange: PageSizeChange,
            FirstPage_Click: FirstPage_Click,
            PrevPage_Click: PrevPage_Click,
            NextPage_Click: NextPage_Click,
            LastPage_Click: LastPage_Click,
            GoToPageNumber_Click: GoToPageNumber_Click,
            SortColumn_Click: SortColumn_Click,
            OnInsertRow: OnInsertRow,
            OnInsertRow_KeyPress: OnInsertRow_KeyPress,
            SearchTable: SearchTable,
            InitialSearch: InitialSearch,
            CloseSearchTable: CloseSearchTable,
            SearchOverlayClick: SearchOverlayClick,
            InitialSearch_KeyPress: InitialSearch_KeyPress,
            ClearSearch: ClearSearch
        }
    }();

    function CreateXMLHttpRequest() {
        try { return new XMLHttpRequest(); } catch (e) { }
        try { return new ActiveXObject("Msxml2.XMLHTTP"); } catch (e) { }
        return null;
    }

    var newThemeColorOption_Format = "";
    var defaultLightFontColorVal = "";
    var defaultDarkFontColorVal = "";
    function SetNewThemeColorOptionFormat(str, fontColor_Light, fontColor_Dark) {
        newThemeColorOption_Format = str;
        defaultLightFontColorVal = fontColor_Light;
        defaultDarkFontColorVal = fontColor_Dark;
    }
    function InitializeThemeColorOption(idStr) {
        var $this = $("#" + idStr).find(".theme-color-option.selected");
        if ($this.length > 0) {
            var resetColor = false;
            var color = $this.attr("data-color");
            if (!color) {
                color = $this.find(".color-option-toplogo").css("background-color");
                resetColor = true;
            }

            if (color.indexOf("rgba(") === 0 || color.indexOf("rgb(") === 0) {
                color = openWSE.RgbToHex(color);
            }
            color = color.replace("#", "").toUpperCase();

            $this.parent().find("input[name='cb_usedefaultthemecolor']").prop("checked", false);
            $this.parent().find(".color-option-picker").find("input").prop("disabled", false);

            $this.parent().find(".color-option-input").val(CreateFormattedHexColor(color));
            $this.parent().find(".color-option-input").attr("value", CreateFormattedHexColor(color));
            if (resetColor) {
                $this.parent().find("input[name='cb_usedefaultthemecolor']").prop("checked", true);
                $this.parent().find(".color-option-picker").find("input").prop("disabled", true);
            }
        }

        if ($(".cb_usedefaultthemecolor_class").length > 1) {
            $(".cb_usedefaultthemecolor_class").each(function (index) {
                var newId = "cb_usedefaultthemecolor" + index.toString();
                $(this).attr("id", newId);
                $(this).parent().find("label[for='cb_usedefaultthemecolor']").attr("for", newId);
            });
        }
    }
    function ColorOption_Changed(_this) {
        var $this = $(_this).closest(".color-option-picker").parent().find(".theme-color-option.selected");
        if ($this.length > 0) {
            if ($("#pnl_ColorOptions").length > 0) {
                $("#pnl_ColorOptions").find(".theme-color-option.selected").attr("data-color", $.trim($(_this).parent().find(".color-option-input").val()).replace("#", ""));
            }

            if ($("#div_ColorOptionsHolder").length > 0) {
                $("#div_ColorOptionsHolder").find(".theme-color-option.selected").attr("data-color", $.trim($(_this).parent().find(".color-option-input").val()).replace("#", ""));
            }

            ThemeColorOption_Clicked($this);
        }
        else {
            openWSE.AlertWindow("No color option is selected");
        }
    }
    function ThemeColorOption_Clicked(_this, cancelUpdate) {
        if (_this === undefined || _this == null) {
            return;
        }

        var colorOption = $(_this).attr("data-option");
        var color = $(_this).attr("data-color");
        var resetColor = false;
        if (!color) {
            color = $(_this).find(".color-option-toplogo").css("background-color");
            resetColor = true;
        }

        if (color.indexOf("rgba(") === 0 || color.indexOf("rgb(") === 0) {
            color = openWSE.RgbToHex(color);
        }
        color = color.replace("#", "").toUpperCase();

        $(_this).parent().find("input[name='cb_usedefaultthemecolor']").prop("checked", false);
        $(_this).parent().find(".color-option-picker").find("input").prop("disabled", false);

        $(_this).parent().find(".color-option-input").val(CreateFormattedHexColor(color));
        $(_this).parent().find(".color-option-input").attr("value", CreateFormattedHexColor(color));
        if (resetColor) {
            color = "";
            $(_this).parent().find("input[name='cb_usedefaultthemecolor']").prop("checked", true);
            $(_this).parent().find(".color-option-picker").find("input").prop("disabled", true);
        }


        var parentId = $(_this).parent().attr("id");
        if (!parentId) {
            parentId = $(_this).parent().parent().attr("id");
        }

        if (parentId !== "pnl_ColorOptions") {
            $("#div_ColorOptionsHolder").find(".theme-color-option").removeClass("selected");
        }
        else {
            $("#pnl_ColorOptions").find(".theme-color-option").removeClass("selected");
        }

        $(_this).addClass("selected");
        
        $(_this).find(".color-option-toplogo").css("background-color", "");
        $(_this).find(".color-option-topbar").css("background-color", "");

        if (color !== "") {
            $(_this).find(".color-option-toplogo").css("background-color", "#" + color);
            if (colorOption === "1" || colorOption === "3") {
                $(_this).find(".color-option-topbar").css("background-color", "#" + color);
            }
        }

        if (!cancelUpdate) {
            $("body").attr("data-coloroption", colorOption);
            SetNewThemeColorOption(colorOption, color);

            if (parentId !== "pnl_ColorOptions") {
                if ($("#pnl_ColorOptions").length > 0) {
                    openWSE.ThemeColorOption_Clicked($("#pnl_ColorOptions").find(".theme-color-option[data-option='" + colorOption + "']")[0], true);
                }

                if (!openWSE_Config.demoMode) {
                    loadingPopup.Message("Updating...");
                    openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateColorOption", '{ "option": "' + colorOption + '", "color": "' + color + '" }', null, null, null, function (data) {
                        loadingPopup.RemoveMessage();
                    });
                }
            }
            else if (parentId === "pnl_ColorOptions" && $("#hf_ColorOptions").length > 0) {
                openWSE.ThemeColorOption_Clicked($("#div_ColorOptionsHolder").find(".theme-color-option[data-option='" + colorOption + "']")[0], true);
                loadingPopup.Message("Updating...");
                $("#hf_ColorOptions").val(colorOption + "~" + color);
                openWSE.CallDoPostBack("hf_ColorOptions", "");
            }
        }
    }
    function ResetColorOption_Clicked(_this, cancelUpdate) {
        if (_this === undefined || _this == null) {
            return;
        }

        var $this = $(_this).parent().parent().find(".theme-color-option.selected");
        if ($this.length > 0) {
            if ($(_this).prop("checked")) {
                $(_this).parent().parent().find(".color-option-picker").find("input").prop("disabled", true);

                var colorOption = $this.attr("data-option");
                $this.attr("data-color", "");
                $this.find(".color-option-toplogo").css("background-color", "");
                $this.find(".color-option-topbar").css("background-color", "");

                var color = $this.find(".color-option-toplogo").css("background-color");
                if (color.indexOf("rgba(") === 0 || color.indexOf("rgb(") === 0) {
                    color = openWSE.RgbToHex(color);
                }
                color = color.replace("#", "").toUpperCase();
                $this.parent().find(".color-option-input").val(CreateFormattedHexColor(color));
                $this.parent().find(".color-option-input").attr("value", CreateFormattedHexColor(color));


                var parentId = $(_this).parent().attr("id");
                if (!parentId) {
                    parentId = $(_this).parent().parent().attr("id");
                }

                if (parentId !== "pnl_ColorOptions") {
                    $("#div_ColorOptionsHolder").find(".theme-color-option").removeClass("selected");
                }
                else {
                    $("#pnl_ColorOptions").find(".theme-color-option").removeClass("selected");
                }

                $this.addClass("selected");

                if (!cancelUpdate) {
                    $("body").attr("data-coloroption", colorOption);
                    SetNewThemeColorOption(colorOption, "");

                    if (parentId !== "pnl_ColorOptions") {
                        if ($("#pnl_ColorOptions").length > 0) {
                            $("#pnl_ColorOptions").find(".cb_usedefaultthemecolor_class").prop("checked", true);
                            openWSE.ResetColorOption_Clicked($("#pnl_ColorOptions").find(".cb_usedefaultthemecolor_class")[0], true);
                        }

                        if (!openWSE_Config.demoMode) {
                            loadingPopup.Message("Updating...");
                            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateColorOption", '{ "option": "' + colorOption + '", "color": "" }', null, null, null, function (data) {
                                loadingPopup.RemoveMessage();
                            });
                        }
                    }
                    else if (parentId === "pnl_ColorOptions" && $("#hf_ColorOptions").length > 0) {
                        $("#div_ColorOptionsHolder").find(".cb_usedefaultthemecolor_class").prop("checked", true);
                        openWSE.ResetColorOption_Clicked($("#div_ColorOptionsHolder").find(".cb_usedefaultthemecolor_class")[0], true);
                        loadingPopup.Message("Updating...");
                        $("#hf_ColorOptions").val(colorOption + "~");
                        openWSE.CallDoPostBack("hf_ColorOptions", "");
                    }
                }
            }
            else {
                ColorOption_Changed($(_this).parent().parent().find(".color-option-picker").find("input[type='button']"), cancelUpdate);
            }

            
        }
        else {
            openWSE.AlertWindow("No color option is selected");
        }
    }
    function SetNewThemeColorOption(selectedIndex, selectedColor) {
        var $head = $("head");
        if ($head.length > 0) {
            $head.find("style[data-id='theme-coloroption-style']").remove();

            if (selectedColor) {
                if (selectedColor.indexOf("#") !== 0) {
                    selectedColor = "#" + selectedColor;
                }

                var altColor1 = openWSE.GetAltColorFromHex(selectedColor, 40);
                var altColor2 = openWSE.GetAltColorFromHex(selectedColor, 70);
                var fontColor = defaultLightFontColorVal;
                var defaultBrightnessFilter = "brightness(500%)";
                var defaultSpanIndicatorBrightnessFilter = "brightness(20%)";
                var topBorderColor = altColor1;
                if (UseDarkTextColorWithBackground(selectedColor)) {
                    fontColor = defaultDarkFontColorVal;
                    defaultBrightnessFilter = "brightness(100%)";
                    defaultSpanIndicatorBrightnessFilter = "brightness(100%)";
                    topBorderColor = "inherit";
                }

                if (newThemeColorOption_Format) {
                    var tempStyle = newThemeColorOption_Format;
                    tempStyle = tempStyle.replace(/\{0\}/g, selectedIndex);
                    tempStyle = tempStyle.replace(/\{1\}/g, selectedColor);
                    tempStyle = tempStyle.replace(/\{2\}/g, fontColor);
                    tempStyle = tempStyle.replace(/\{3\}/g, altColor1);
                    tempStyle = tempStyle.replace(/\{4\}/g, altColor2);
                    tempStyle = tempStyle.replace(/\{5\}/g, defaultBrightnessFilter);
                    tempStyle = tempStyle.replace(/\{6\}/g, topBorderColor);
                    tempStyle = tempStyle.replace(/\{7\}/g, defaultSpanIndicatorBrightnessFilter);
                    tempStyle = tempStyle.replace(/{{/g, "{");
                    tempStyle = tempStyle.replace(/}}/g, "}");

                    $head.append(tempStyle);
                }
            }
        }
    }
    function CreateFormattedHexColor(color) {
        if (color.indexOf("#") !== 0) {
            return "#" + color;
        }
        return color;
    }

    function RgbToHex(rgb) {
        rgb = rgb.match(/^rgba?[\s+]?\([\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?/i);
        return (rgb && rgb.length === 4) ? "#" +
         ("0" + parseInt(rgb[1], 10).toString(16)).slice(-2) +
         ("0" + parseInt(rgb[2], 10).toString(16)).slice(-2) +
         ("0" + parseInt(rgb[3], 10).toString(16)).slice(-2) : '';
    }
    function HexToRgb(hex) {
        // Expand shorthand form (e.g. "03F") to full form (e.g. "0033FF")
        var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
        hex = hex.replace(shorthandRegex, function (m, r, g, b) {
            return r + r + g + g + b + b;
        });

        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    }
    function UseDarkTextColorWithBackground(color) {
        var useDarkColor = true;

        if (color !== "inherit" && color !== "#inherit") {
            var _color = HexToRgb(color);
            if (_color) {
                if (_color.r + _color.g + _color.b < 475) {
                    useDarkColor = false;
                }
            }
        }

        return useDarkColor;
    }
    function GetAltColorFromHex(hex, colorDiff) {
        var rgb = openWSE.HexToRgb(hex);
        if (rgb) {
            try {
                var newR = rgb.r;
                var newG = rgb.g;
                var newB = rgb.b;

                if (newR > colorDiff) {
                    newR -= colorDiff;
                }
                else {
                    newR = 0;
                }

                if (newG > colorDiff) {
                    newG -= colorDiff;
                }
                else {
                    newG = 0;
                }

                if (newB > colorDiff) {
                    newB -= colorDiff;
                }
                else {
                    newB = 0;
                }

                var newHex = openWSE.RgbToHex("rgb(" + newR + ", " + newG + ", " + newB + ")");
                if (newHex.indexOf("#") !== 0) {
                    newHex = "#" + newHex;
                }

                return newHex.toUpperCase();
            }
            catch (evt) { }
        }

        if (hex.indexOf("#") !== 0) {
            hex = "#" + hex;
        }

        return hex.toUpperCase();
    }

    $(document.body).on("change", "#rb_BoxedLayout", function () {
        $("#site_mainbody").attr("data-layoutoption", "Boxed");
        openWSE.SetLeftSidebarScrollTop();
        $(window).resize();

        if ($("#rb_BoxedLayout_acctOptions").length > 0 && $("#rb_WideLayout_acctOptions").length > 0) {
            $("#rb_BoxedLayout_acctOptions").prop("checked", true);
            $("#rb_WideLayout_acctOptions").prop("checked", false);
        }

        if (!openWSE_Config.demoMode) {
            loadingPopup.Message("Updating...");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateLayoutOption", '{ "option": "Boxed" }', null, null, null, function (data) {
                loadingPopup.RemoveMessage();
            });
        }
    });
    $(document.body).on("change", "#rb_WideLayout", function () {
        $("#site_mainbody").attr("data-layoutoption", "Wide");
        openWSE.SetLeftSidebarScrollTop();
        $(window).resize();

        if ($("#rb_BoxedLayout_acctOptions").length > 0 && $("#rb_WideLayout_acctOptions").length > 0) {
            $("#rb_BoxedLayout_acctOptions").prop("checked", false);
            $("#rb_WideLayout_acctOptions").prop("checked", true);
        }

        if (!openWSE_Config.demoMode) {
            loadingPopup.Message("Updating...");
            openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateLayoutOption", '{ "option": "Wide" }', null, null, null, function (data) {
                loadingPopup.RemoveMessage();
            });
        }
    });

    function CloseWindow() {
        if (getParameterByName("mobileMode") === "true" || window.location.href.indexOf("mobileMode=true") !== -1) {
            if (getParameterByName("fromAppRemote") === "true" || window.location.href.indexOf("fromAppRemote=true") !== -1) {
                window.location.href = openWSE.siteRoot() + "AppRemote.aspx#?tab=AdminLinks_tab";
            }
            else {
                window.location.href = window.location.href.replace("?mobileMode=true", "").replace("&mobileMode=true", "");
            }
        }
        else if (window.location.href.toLowerCase().indexOf("externalappholder.aspx") !== -1) {
            window.close();
        }
        else {
            window.location.href = openWSE.siteRoot() + "Default.aspx";
        }

        return false;
    }
    function AnimateToTop() {
        $("#main_container").find(".go-to-top").hide();
        $("#main_container").animate({ scrollTop: 0 }, openWSE_Config.animationSpeed * 3, function () {
            $("#main_container").find(".go-to-top").remove();
        });
    }

    function LogConsoleMessage(message) {
        if (typeof (message) === "string") {
            message = unescape(message);
            message = message.replace(/\+/g, " ");
        }
        console.log(message);
    }

    function SetupAccordions(loadLocally) {
        var needtoToggleBack = false;
        if (openWSE_Config.saveCookiesAsSessions && loadLocally) {
            openWSE_Config.saveCookiesAsSessions = false;
            needtoToggleBack = true;
        }

        $(".custom-accordion").each(function () {
            var $this = $(this);
            if (!openWSE.ConvertBitToBoolean($this.attr("data-nocookie")) && $this.attr("id")) {
                cookieFunctions.get("custom-accordion__" + $this.attr("id"), function (activeAccordion) {
                    if (activeAccordion === null || activeAccordion === undefined || activeAccordion === "") {
                        activeAccordion = 0;
                    }
                    else {
                        activeAccordion = parseInt(activeAccordion);
                        if (isNaN(activeAccordion)) {
                            activeAccordion = 0;
                        }
                    }

                    cookieFunctions.del("custom-accordion__" + $this.attr("id"));

                    $this.accordion({
                        heightStyle: "content",
                        active: activeAccordion
                    });
                });
            }
            else {
                $this.accordion({
                    heightStyle: "content",
                    active: 0
                });
            }
        });

        if (needtoToggleBack) {
            openWSE_Config.saveCookiesAsSessions = true;
        }
    }
    function SaveAccordionState(saveLocally) {
        var needtoToggleBack = false;
        if (openWSE_Config.saveCookiesAsSessions && saveLocally) {
            openWSE_Config.saveCookiesAsSessions = false;
            needtoToggleBack = true;
        }

        $(".custom-accordion").each(function () {
            var $this = $(this);
            if (!openWSE.ConvertBitToBoolean($this.attr("data-nocookie")) && $this.attr("id")) {
                cookieFunctions.set("custom-accordion__" + $this.attr("id"), $this.accordion("option", "active"), 30);
            }
        });

        if (needtoToggleBack) {
            openWSE_Config.saveCookiesAsSessions = true;
        }
    }

    var CalendarViewApps = function () {
        var eventEleClass;
        var monthList = new Array(12);
        monthList[0] = "January";
        monthList[1] = "February";
        monthList[2] = "March";
        monthList[3] = "April";
        monthList[4] = "May";
        monthList[5] = "June";
        monthList[6] = "July";
        monthList[7] = "August";
        monthList[8] = "September";
        monthList[9] = "October";
        monthList[10] = "November";
        monthList[11] = "December";

        var calendarApps = {};

        function InitializeCalendar(id, options) {
            if (calendarApps[id] === null || calendarApps[id] === undefined) {
                calendarApps[id] = {
                    dataObj: null,
                    container: $(options.container),
                    selectedMonth: null,
                    selectedYear: null,
                    AddNewCalendarFunction: options.AddNewCalendarFunction,
                    GetMonthInfoFunction: options.GetMonthInfoFunction,
                    ViewEventFunction: options.ViewEventFunction,
                    searchVal: "",
                    reloading: false,
                    eventList: null,
                    viewMode: "0",
                    sidebarActive: false,
                    sidebarLinkHtml: ""
                };
            }
            else {
                calendarApps[id].container = $(options.container);
                calendarApps[id].AddNewCalendarFunction = options.AddNewCalendarFunction;
                calendarApps[id].GetMonthInfoFunction = options.GetMonthInfoFunction;
                calendarApps[id].ViewEventFunction = options.ViewEventFunction;
                calendarApps[id].reloading = "";
                calendarApps[id].eventList = null;
            }

            if (calendarApps[id].selectedMonth === null && calendarApps[id].selectedYear === null) {
                var d_temp = new Date();
                buildCalendar(id, d_temp.getMonth(), d_temp.getFullYear());
            }
            else {
                buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
            }
        }
        function ResizeCalendars() {
            $(".calendarView").each(function () {
                var tempId = $(this).attr("data-calendarid");
                calendarApps[tempId].container.find(".calendarViewHolder").css("height", getAppHeight(tempId) + "px");
                refreshEvents(tempId, true);
            });
        }


        /* Build Calendar Table */
        function buildCalendar(id, m, y) {
            if (!calendarApps[id].reloading) {
                calendarApps[id].reloading = true;

                loadingPopup.Message("Loading...", GetAppContainer(id));

                calendarApps[id].selectedMonth = m;
                calendarApps[id].selectedYear = y;

                calendarApps[id].dataObj = new Date(y, m, 1, new Date().getHours(), new Date().getMinutes(), new Date().getSeconds(), new Date().getMilliseconds());
                var toolbarStr = buildToolbar(id);

                var str = "";
                if (calendarApps[id].viewMode === "0" || calendarApps[id].viewMode === "1") {
                    str = buildWorkWeekAndFullWeek(id);
                }

                calendarApps[id].container.addClass("calendarView");
                calendarApps[id].container.attr("data-calendarid", id);
                calendarApps[id].container.html(toolbarStr + "<div class='calendarViewHolder' data-viewmode='" + calendarApps[id].viewMode + "' onclick=\"openWSE.CalendarViewApps.CalendarViewHolder_Click('" + id + "');\">" + str + "</div>");
                if (calendarApps[id].sidebarActive) {
                    calendarApps[id].container.append("<div class='calendarView-sidebar-open-overlay' onclick=\"openWSE.CalendarViewApps.CalendarViewHolder_Click('" + id + "');\"></div>");
                }

                calendarApps[id].container.find(".calendarViewHolder").css("height", getAppHeight(id) + "px");
                calendarApps[id].container.find(".calendarView-select").val(calendarApps[id].viewMode);

                if (calendarApps[id].viewMode === "0" || calendarApps[id].viewMode === "1") {
                    removeEmptyListCalendarRows(id);
                }

                if (calendarApps[id].GetMonthInfoFunction !== null && typeof (calendarApps[id].GetMonthInfoFunction) === "function") {
                    calendarApps[id].GetMonthInfoFunction(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear, calendarApps[id].searchVal);
                }
                else {
                    loadingPopup.RemoveMessage(GetAppContainer(id));
                    calendarApps[id].reloading = false;
                }
            }
        }
        function getAppHeight(id) {
            return calendarApps[id].container.parent().outerHeight() - (calendarApps[id].container.find(".calendarView-toolbar").outerHeight() + 1);
        }
        function daysInMonth(m, y) {
            return new Date(y, m + 1, 0).getDate() + 1;
        }
        function findStartofCal(id) {
            var d = new Date(monthList[calendarApps[id].dataObj.getMonth()] + "1, " + calendarApps[id].dataObj.getFullYear());
            return d.getDay();
        }
        function RefreshCalendar(id) {
            buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
        }


        /* Build Month Events */
        function BuildMonthEvents(id, response) {
            if (response.length > 0) {
                calendarApps[id].eventList = response;
                refreshEvents(id, false);
            }

            calendarApps[id].reloading = false;
            loadingPopup.RemoveMessage(GetAppContainer(id));
        }
        function refreshEvents(id, resizing) {
            var response = calendarApps[id].eventList;
            if (response !== null) {
                if (response.length === 2) {
                    if (!resizing) {
                        calendarApps[id].container.find(".calendarView-sidebar").find(".calendarEvents-upcoming").html(response[1]);
                    }

                    response = response[0];
                }
                else {
                    calendarApps[id].container.find(".calendarView-sidebar").find(".upcomingEvents-li").hide();
                }

                if (calendarApps[id].viewMode === "0" || calendarApps[id].viewMode === "1") {
                    for (var i = 0; i < response.length; i++) {
                        if (response[i].length == 6) {
                            var startDate = formatDateId(response[i][2]);
                            var len = response[i][3];

                            if (calendarApps[id].viewMode === "1" && len > 1) {
                                var $startDateCell = calendarApps[id].container.find("td.calendary-td[data-id='" + startDate + "']");
                                if ($startDateCell.attr("data-dayofweek") === "0") {
                                    startDate = calendarApps[id].container.find("td.calendary-td[data-id='" + startDate + "']").next().attr("data-id");
                                    if (len > 6) {
                                        len = len - 2;
                                    }
                                    else {
                                        len = len - 1;
                                    }
                                }
                            }

                            var tdWidth = calendarApps[id].container.find("td.calendary-td[data-id='" + startDate + "']").width();

                            var bufferLen = len;
                            if (len > 1) {
                                var $nextTd = calendarApps[id].container.find("td.calendary-td[data-id='" + startDate + "']").next("td");
                                for (var j = 0; j < len - 1; j++) {
                                    if ($nextTd.length > 0) {
                                        if (!$nextTd.hasClass("not-apart-of-month")) {
                                            if (calendarApps[id].viewMode === "0" || (calendarApps[id].viewMode === "1" && $nextTd.attr("data-dayofweek") !== "6")) {
                                                tdWidth += $nextTd.width();
                                                $nextTd = $nextTd.next("td");
                                            }
                                        }
                                        else {
                                            bufferLen--;
                                        }
                                    }
                                }

                                tdWidth += bufferLen;
                            }

                            buildCalendarEvent(id, response[i][0], response[i][1], tdWidth, response[i][4], response[i][5], len, startDate);
                        }
                    }

                    setMarginEvents(id);
                    setTableCellHeights(id);
                }
                else {
                    if (!resizing) {
                        calendarApps[id].container.find(".calendarViewHolder").html("");
                        for (var i = 0; i < response.length; i++) {
                            if (response[i].length == 6) {
                                buildListViewEvent(id, response[i][0], response[i][1], response[i][4], response[i][5]);
                            }
                        }
                    }
                }
            }
        }
        function buildCalendarEvent(appId, id, usercolor, width, reason, daterange, length, startDate) {
            var $tdCell = calendarApps[appId].container.find("td.calendary-td[data-id='" + startDate + "']");

            if (canCreateCalendarEvent(id, $tdCell, length)) {
                var str = "<div class='calendar-event " + id + "' style='background: " + usercolor + "; width: " + width + "px;' onmouseenter=\"openWSE.CalendarViewApps.CalendarEvent_MouseEnter('" + appId + "', this);\" onmouseleave=\"openWSE.CalendarViewApps.CalendarEvent_MouseLeave('" + appId + "');\" onclick=\"openWSE.CalendarViewApps.ViewEvent_Click('" + appId + "', '" + id + "');\" title='" + reason.replace("/n", " ") + "'>";
                var _reason = reason.replace("/n", " - ");
                if (length == 1) {
                    _reason = reason.replace("/n", "<br />");
                }
                str += "<div class='calendar-event-font inline-block'>" + _reason + "</div></div>";
                $tdCell.append(str);
            }
            else if (calendarApps[appId].container.find("." + id).length > 0) {
                $tdCell.find("." + id).css("width", width + "px");
            }
        }
        function canCreateCalendarEvent(id, tdCell, length) {
            if (length > 0 && tdCell.find("." + id).length === 0) {
                return true;
            }
            return false;
        }
        function buildListViewEvent(appId, id, usercolor, reason, daterange) {
            var str = "<div class='calendar-event-list-view " + id + "' onclick=\"openWSE.CalendarViewApps.ViewEvent_Click('" + appId + "', '" + id + "');\">";
            str += "<span class='sch_ColorCode rounded-corners-15' style='background: " + usercolor + ";'></span>";
            str += "<span class='calendarEvent-reason'>" + reason.replace("/n", " ") + "</span>" + "<span class='calendarEvent-daterange'>" + daterange + "</span>";
            str += "</div>";
            calendarApps[appId].container.find(".calendarViewHolder").append(str);
        }
        function formatDateId(val) {
            var valSplit = val.split("_");
            if (valSplit.length === 3) {
                var monthVar = parseInt(valSplit[0]) + 1;
                if (monthVar.toString().length === 1) {
                    monthVar = "0" + monthVar.toString();
                }

                var dayVar = valSplit[1];
                if (dayVar.toString().length === 1) {
                    dayVar = "0" + dayVar.toString();
                }

                var yearVar = valSplit[2];

                val = monthVar + "/" + dayVar + "/" + yearVar;
            }

            return val;
        }


        /* Build Toolbar */
        function buildToolbar(id) {
            var toolbarStr = "<div class='calendarView-toolbar'>";
            toolbarStr += "<a class='calendarView-menubtn' onclick=\"openWSE.CalendarViewApps.ToolbarMenu_Click('" + id + "'); return false;\"></a>";

            var ddMonth = "<select class='dd_monthSelect' onchange=\"openWSE.CalendarViewApps.onMonthYear_Change('" + id + "');\">";
            for (var i = 0; i < monthList.length; i++) {
                var selected = "";
                if (calendarApps[id].dataObj.getMonth() == i) {
                    selected = " selected='selected'";
                }
                ddMonth += "<option value='" + i + "' " + selected + ">" + monthList[i] + "</option>";
            }
            ddMonth += "</select>";

            var ddYear = "<select class='dd_yearSelect' onchange=\"openWSE.CalendarViewApps.onMonthYear_Change('" + id + "');\">";
            for (var i = 2010; i < 2025; i++) {
                var selected = "";
                if (calendarApps[id].dataObj.getFullYear() == i) {
                    selected = " selected='selected'";
                }
                ddYear += "<option value='" + i + "' " + selected + ">" + i + "</option>";
            }
            ddYear += "</select>";

            var prevBtn = "<a onclick=\"openWSE.CalendarViewApps.onPrevMonth_Click('" + id + "');return false;\" class='prevMonthBtn' title='Previous Month'></a>";
            var nextBtn = "<a onclick=\"openWSE.CalendarViewApps.onNextMonth_Click('" + id + "');return false;\" class='nextMonthBtn' title='Next Month'></a>";

            toolbarStr += "<div class='dateSelector'>" + nextBtn + prevBtn + ddYear + ddMonth + "</div>";
            toolbarStr += "</div>";
            toolbarStr += buildSidebar(id);

            return toolbarStr;
        }
        function buildSidebar(id) {
            var sidebarActiveClass = "";
            if (calendarApps[id].sidebarActive) {
                sidebarActiveClass = " active";
            }

            var sidebarStr = "<div class='calendarView-sidebar" + sidebarActiveClass + "'><div class='pad-all'>";
            sidebarStr += "<div class='img-close-dark app-menu-btn' onclick=\"openWSE.CalendarViewApps.ToolbarMenu_Click('" + id + "');\" title='Close Menu'></div>";
            sidebarStr += "<a href='#' class='float-right img-refresh' onclick=\"openWSE.CalendarViewApps.RefreshCalendar('" + id + "');return false;\" title='Refresh Calendar'></a>";
            sidebarStr += "<div class='clear-space'></div>";

            // Searchwrapper
            sidebarStr += "<div class='searchwrapper'>";
            sidebarStr += "<div class='searchboxholder'><input type='text' class='searchbox' placeholder='Search current month...' value=\"" + calendarApps[id].searchVal + "\" onkeypress=\"openWSE.CalendarViewApps.SearchCalendar_KeyPress('" + id + "', this, event)\" /></div>";
            sidebarStr += "<a class='searchbox_submit' onclick=\"openWSE.CalendarViewApps.SearchCalendar('" + id + "', this); return false;\"></a><a onclick=\"openWSE.CalendarViewApps.ClearSearch('" + id + "', this); return false;\" class='searchbox_clear'></a>";
            sidebarStr += "</div>";
            sidebarStr += "<div class='clear'></div>";

            sidebarStr += "<ul>";
            sidebarStr += "<li><div class='section-pad'>";
            if (calendarApps[id].AddNewCalendarFunction !== null && typeof (calendarApps[id].AddNewCalendarFunction) === "function") {
                sidebarStr += "<a class=\"sidebar-menu-buttons\" onclick=\"openWSE.CalendarViewApps.onDay_Click('" + id + "', null); return false;\"><span class='td-add-btn sidebar-menu-img'></span>Add Event</a>";
            }
            sidebarStr += calendarApps[id].sidebarLinkHtml;
            sidebarStr += "</div></li>";

            sidebarStr += "<li><div class='section-pad'><h3 class='section-pad-title' style='padding-top: 3px!important;'>View Mode</h3>";
            sidebarStr += "<select class='calendarView-select' onchange=\"openWSE.CalendarViewApps.ChangeViewMode_Change('" + id + "', this);\">";
            sidebarStr += "<option value='0'>Week</option>";
            sidebarStr += "<option value='1'>Work Week</option>";
            sidebarStr += "<option value='2'>List</option>";
            sidebarStr += "</select><div class='clear'></div>";
            sidebarStr += "</div></li>";

            sidebarStr += "<li class='upcomingEvents-li' style='border-bottom: none!important;'><div class='section-pad'><h3 class='section-pad-title' style='padding-top: 3px!important;'>Upcoming Events</h3><div class='clear-space'></div>";
            sidebarStr += "<div class='calendarEvents-upcoming'></div>";
            sidebarStr += "</div></li>";

            sidebarStr += "</ul>";
            sidebarStr += "</div></div>";

            return sidebarStr;
        }


        /* Search Events */
        function SearchCalendar(id, _this) {
            calendarApps[id].searchVal = $.trim($(_this.parentNode).find(".searchbox").val());
            buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
        }
        function SearchCalendar_KeyPress(id, _this, event) {
            if (event.which == 13 || event.keyCode == 13) {
                calendarApps[id].searchVal = $.trim($(_this).val());
                buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
            }
        }
        function ClearSearch(id, _this) {
            calendarApps[id].searchVal = "";
            $(_this.parentNode).find(".searchbox").val("");
            buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
        }


        /* Build Full Week Calendar (Plus Work Week) */
        function buildWorkWeekAndFullWeek(id) {
            var dinm = daysInMonth(calendarApps[id].dataObj.getMonth(), calendarApps[id].dataObj.getFullYear());
            var count = 1;

            var str = "<table><tbody><tr class='days-of-week'><td data-dayofweek='0'>Sunday</td><td data-dayofweek='1'>Monday</td><td data-dayofweek='2'>Tuesday</td><td data-dayofweek='3'>Wednesday</td><td data-dayofweek='4'>Thursday</td><td data-dayofweek='5'>Friday</td><td data-dayofweek='6'>Saturday</td></tr>";
            for (var r = 0; r < 6; r++) {
                str += "<tr class='table-Days'>";
                for (var c = 0; c < 7; c++) {
                    if (count == dinm) {
                        str += "<td class='not-apart-of-month' data-dayofweek='" + c + "'></td>";
                    } else {
                        if (count == 1) {
                            if (c == findStartofCal(id)) {
                                str += createDay(id, count, c);
                                count++;
                            } else {
                                str += "<td class='not-apart-of-month' data-dayofweek='" + c + "'></td>";
                            }
                        } else {
                            str += createDay(id, count, c);
                            count++;
                        }

                    }
                }
                str += "</tr>";
            }
            str += "</tbody></table>";

            return str;
        }
        function createDay(id, d, c) {
            var dateNow = new Date();
            var currDayClass = "";
            if (dateNow.getMonth() == calendarApps[id].dataObj.getMonth() && dateNow.getDate() == d && dateNow.getFullYear() == calendarApps[id].dataObj.getFullYear()) {
                currDayClass = " current-day";
            }

            var monthVar = (calendarApps[id].dataObj.getMonth() + 1);
            if (monthVar.toString().length === 1) {
                monthVar = "0" + monthVar.toString();
            }

            var dayVar = d;
            if (dayVar.toString().length === 1) {
                dayVar = "0" + dayVar.toString();
            }

            return "<td data-id='" + monthVar + "/" + dayVar + "/" + calendarApps[id].dataObj.getFullYear() + "' class='calendary-td' data-dayofweek='" + c + "'><span class='calendar-td-day" + currDayClass + "' onclick=\"openWSE.CalendarViewApps.onDay_Click('" + id + "', this);\" title='Click to add event'>" + d + "</span></td>";
        }
        function setMarginEvents(id) {
            var td_width = 0;
            var length_temp = 0;
            var bufferMargin = 2;
            calendarApps[id].container.find(".table-Days").each(function (index) {
                var margintop = bufferMargin;
                var margintopnext = bufferMargin;
                var index2_temp = 0;
                var dayLength = 0;
                $(this).find("td").each(function (index2) {
                    if (index2_temp > dayLength) {
                        margintop = bufferMargin;
                    }
                    else {
                        margintop = margintopnext;
                    }
                    td_width = $(this).width();
                    $(this).find(".calendar-event").each(function (index3) {
                        $(this).css("margin-top", margintop + "px");
                        var event_width = $(this).width();
                        var event_height = $(this).height();
                        var length = Math.round(event_width / td_width);
                        margintop += event_height + 1;
                        if ((length > 1) && (length_temp >= index3)) {
                            length_temp = length;
                            dayLength = length + index2;
                            if ((index2_temp == 1) && (length_temp == 2)) {
                                margintopnext = bufferMargin;
                            }
                            else {
                                margintopnext += event_height;
                            }
                        }
                        else {
                            if (index2_temp >= length_temp) {
                                margintopnext = bufferMargin;
                            }
                            index2_temp++;
                        }
                    });
                });
            });
        }
        function setTableCellHeights(id) {
            calendarApps[id].container.find(".table-Days").each(function (index) {
                var $this = $(this).find(".calendar-event");
                if ($this.length > 0) {
                    var total_height = 0;
                    var marginList = new Array();
                    $this.each(function (index) {
                        var marginTopVal = parseInt($this.eq(index).css("margin-top").replace("px", ""));
                        if (!containsMarginValue(marginList, marginTopVal)) {
                            marginList.push(marginTopVal);
                            total_height += $this[index].clientHeight;
                        }
                    });
                    total_height += 40;
                    $(this).css("min-height", total_height + "px");
                }
            });
        }
        function containsMarginValue(arr, val) {
            for (var i = 0; i < arr.length; i++) {
                if (arr[i] == val) {
                    return true;
                }
            }

            return false;
        }
        function removeEmptyListCalendarRows(id) {
            calendarApps[id].container.find("tr.table-Days").each(function () {
                if (calendarApps[id].viewMode === "0") {
                    var totalCells = $(this).find("td").length;
                    var totalNotApartOfMothCells = $(this).find("td.not-apart-of-month").length;
                    if (totalCells === totalNotApartOfMothCells) {
                        $(this).remove();
                    }
                }
                else if (calendarApps[id].viewMode === "1") {
                    var totalCells = 5;
                    var totalNotApartOfMothCells = 0;
                    $(this).find("td").each(function () {
                        if ($(this).attr("data-dayofweek") !== "0" && $(this).attr("data-dayofweek") !== "6" && $(this).hasClass("not-apart-of-month")) {
                            totalNotApartOfMothCells++;
                        }
                    });
                    if (totalCells === totalNotApartOfMothCells) {
                        $(this).remove();
                    }
                }
            });
        }


        /* Month Click Events */
        function onPrevMonth_Click(id) {
            var m = parseInt(calendarApps[id].container.find(".dd_monthSelect").val());
            var y = parseInt(calendarApps[id].container.find(".dd_yearSelect").val());
            if (m == 0) {
                y = y - 1;
                m = 11;
            }
            else {
                m = m - 1;
            }

            buildCalendar(id, m, y);
        }
        function onNextMonth_Click(id) {
            var m = parseInt(calendarApps[id].container.find(".dd_monthSelect").val());
            var y = parseInt(calendarApps[id].container.find(".dd_yearSelect").val());
            if (m == 11) {
                y = y + 1;
                m = 0;
            }
            else {
                m = m + 1;
            }

            buildCalendar(id, m, y);
        }
        function onMonthYear_Change(id) {
            var m = parseInt(calendarApps[id].container.find(".dd_monthSelect").val());
            var y = parseInt(calendarApps[id].container.find(".dd_yearSelect").val());
            buildCalendar(id, m, y);
        }
        function onDay_Click(id, _this) {
            if (calendarApps[id].AddNewCalendarFunction !== null && typeof (calendarApps[id].AddNewCalendarFunction) === "function") {
                if (_this === null || _this === undefined) {
                    var dateNow = new Date();
                    var monthVar = (dateNow.getMonth() + 1);
                    if (monthVar.toString().length === 1) {
                        monthVar = "0" + monthVar.toString();
                    }

                    var dayVar = dateNow.getDate();
                    if (dayVar.toString().length === 1) {
                        dayVar = "0" + dayVar.toString();
                    }

                    var yearVar = dateNow.getFullYear();
                    calendarApps[id].AddNewCalendarFunction(monthVar + "/" + dayVar + "/" + yearVar, false);
                }
                else {
                    calendarApps[id].AddNewCalendarFunction(_this.parentNode.getAttribute("data-id"), false);
                }
            }
        }
        function ViewEvent_Click(id, eventId) {
            if (calendarApps[id].ViewEventFunction !== null && typeof (calendarApps[id].ViewEventFunction) === "function") {
                calendarApps[id].ViewEventFunction(eventId);
            }
        }
        function CalendarEvent_MouseEnter(id, _this) {
            var classNames = openWSE.GetElementClassList(_this);
            for (var i = 0; i < classNames.length; i++) {
                if (classNames[i] != "calendar-event" && classNames[i] != "calendar-event-hover") {
                    eventEleClass = classNames[i];
                    break;
                }
            }
            if (!calendarApps[id].container.find("." + eventEleClass).hasClass("calendar-event-hover")) {
                calendarApps[id].container.find("." + eventEleClass).addClass("calendar-event-hover");
            }
        }
        function CalendarEvent_MouseLeave(id) {
            if (eventEleClass) {
                calendarApps[id].container.find("." + eventEleClass).removeClass("calendar-event-hover");
            }
        }
        function ChangeViewMode_Change(id, _this) {
            calendarApps[id].viewMode = _this.value;
            buildCalendar(id, calendarApps[id].selectedMonth, calendarApps[id].selectedYear);
        }
        function ToolbarMenu_Click(id) {
            var sidebar = calendarApps[id].container.find(".calendarView-sidebar");
            if (sidebar.hasClass("active")) {
                sidebar.removeClass("active");
                calendarApps[id].container.find(".calendarView-sidebar-open-overlay").remove();
                calendarApps[id].sidebarActive = false;
            }
            else {
                sidebar.addClass("active");
                calendarApps[id].container.append("<div class='calendarView-sidebar-open-overlay' onclick=\"openWSE.CalendarViewApps.CalendarViewHolder_Click('" + id + "');\"></div>");
                calendarApps[id].sidebarActive = true;
            }
        }
        function CalendarViewHolder_Click(id) {
            var sidebar = calendarApps[id].container.find(".calendarView-sidebar");
            if (sidebar.hasClass("active")) {
                sidebar.removeClass("active");
                calendarApps[id].container.find(".calendarView-sidebar-open-overlay").remove();
                calendarApps[id].sidebarActive = false;
            }
        }

        function GetAppContainer(id) {
            if (id) {
                id = id.replace("#", "");
                if (id.indexOf("app-") === 0) {
                    return "#" + id.substring(4) + "-load";
                }
            }

            return null;
        }

        return {
            InitializeCalendar: InitializeCalendar,
            ResizeCalendars: ResizeCalendars,
            BuildMonthEvents: BuildMonthEvents,
            SearchCalendar: SearchCalendar,
            SearchCalendar_KeyPress: SearchCalendar_KeyPress,
            ClearSearch: ClearSearch,
            onPrevMonth_Click: onPrevMonth_Click,
            onNextMonth_Click: onNextMonth_Click,
            onMonthYear_Change: onMonthYear_Change,
            onDay_Click: onDay_Click,
            ViewEvent_Click: ViewEvent_Click,
            CalendarEvent_MouseEnter: CalendarEvent_MouseEnter,
            CalendarEvent_MouseLeave: CalendarEvent_MouseLeave,
            ChangeViewMode_Change: ChangeViewMode_Change,
            ToolbarMenu_Click: ToolbarMenu_Click,
            CalendarViewHolder_Click: CalendarViewHolder_Click,
            RefreshCalendar: RefreshCalendar
        }

    }();

    function isCanvasSupported() {
        var elem = document.createElement('canvas');
        return !!(elem.getContext && elem.getContext('2d'));
    }

    function noAnimationForSortableRows(add) {
        if (add) {
            $(".ui-sortable").find("tr.myItemStyle").each(function () {
                $(this).addClass('no-animation-hover');
            });
        }
        else {
            $(".ui-sortable").find("tr.myItemStyle").each(function () {
                $(this).removeClass('no-animation-hover');
            });
        }
    }

    function AjaxCall(_url, _data, _options, successFuntion, errorFunction, completeFunction) {
        if (!_url) {
            return;
        }
        else if (_url.indexOf("http") === -1 && _url.indexOf("www.") === -1 && _url.indexOf("//") === -1) {
            _url = openWSE.siteRoot() + _url;
        }

        var ajaxObj = { url: _url };

        if (_data) {
            ajaxObj["data"] = _data;
        }

        if (!_options) {
            _options = {
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8"
            };
        }

        ajaxObj["dataType"] = _options.dataType ? _options.dataType : "json";
        ajaxObj["type"] = _options.type ? _options.type : "POST";
        ajaxObj["contentType"] = _options.contentType ? _options.contentType : "application/json; charset=utf-8";
        if (_options.dataFilter !== null && _options.dataFilter !== undefined) {
            ajaxObj["dataFilter"] = _options.dataFilter;
        }
        if (_options.cache !== null && _options.cache !== undefined) {
            ajaxObj["cache"] = _options.cache;
        }
        if (_options.processData !== null && _options.processData !== undefined) {
            ajaxObj["processData"] = _options.processData;
        }

        if (successFuntion && typeof (successFuntion) === "function") {
            ajaxObj["success"] = successFuntion;
        }

        if (errorFunction && typeof (errorFunction) === "function") {
            ajaxObj["error"] = errorFunction;
        }
        else {
            ajaxObj["error"] = function (data) {
                loadingPopup.RemoveMessage();
                if (data && data.statusText && data.statusText !== "OK") {
                    openWSE.AlertWindow(data);
                }
            };
        }

        if (completeFunction && typeof (completeFunction) === "function") {
            ajaxObj["complete"] = completeFunction;
        }

        if (_options.promise && typeof (_options.promise) === "function") {
            $.ajax(ajaxObj).done(_options.promise);
        }
        else {
            $.ajax(ajaxObj);
        }
    }

    function CallDoPostBack(a, b) {
        if (!a) {
            a = "";
        }
        if (!b) {
            b = "";
        }

        setTimeout(function () {
            __doPostBack(a, b);
        }, 1);
    }

    function UpdateLogoSizeOnLoad(ele) {
        if (ele && ele.find(".title-logo").length > 0) {
            var bgSrc = ele.find(".title-logo").css("background-image");
            if (bgSrc) {
                bgSrc = bgSrc.substring(4);
                bgSrc = bgSrc.substring(0, bgSrc.length - 1);
                if (bgSrc.indexOf("'") === -1 && bgSrc.indexOf("\"") === -1) {
                    bgSrc = "'" + bgSrc + "'";
                }
                $("body").append("<img id='tempTitleLogoEle' alt='' src=" + bgSrc + " style='visibility: hidden; position: fixed; z-index: -1; top: 0; left: 0;' />");
                document.getElementById("tempTitleLogoEle").onload = function () {
                    var bgSrcWt = $("#tempTitleLogoEle").outerWidth();
                    var bgSrcHt = $("#tempTitleLogoEle").outerHeight();
                    if (bgSrcHt < bgSrcWt) {
                        ele.find(".title-text").addClass("large-img");
                        ele.find(".title-logo").addClass("large-img");
                        if ($(".iframe-top-bar").length > 0) {
                            $(".iframe-top-bar").find(".iframe-title-logo").addClass("large-img");
                        }

                        $("#tempTitleLogoEle").remove();
                    }
                };
                document.getElementById("tempTitleLogoEle").onerror = function () {
                    $("#tempTitleLogoEle").remove();
                };
            }
        }
    }

    return {
        siteRoot: GetSiteRoot,
        topBarHt: topBarHt,
        AdjustTableSettingsBox: AdjustTableSettingsBox,
        IsComplexWorkspaceMode: IsComplexWorkspaceMode,
        PagedWorkspace: PagedWorkspace,
        SetContainerTopPos: SetContainerTopPos,
        AdjustModalWindowView: AdjustModalWindowView,
        ConvertBitToBoolean: ConvertBitToBoolean,
        OpenMobileWorkspace: OpenMobileWorkspace,
        init: init,
        InitializeSlimScroll: InitializeSlimScroll,
        SetLeftSidebarScrollTop: SetLeftSidebarScrollTop,
        UpdateAppSelector: UpdateAppSelector,
        SetDuplicateAppIcons: SetDuplicateAppIcons,
        NextSiteTip: NextSiteTip,
        PreviousSiteTip: PreviousSiteTip,
        CloseSiteTip: CloseSiteTip,
        CheckIfOverlaysExistsOnNonComplex: CheckIfOverlaysExistsOnNonComplex,
        DisableOverlaysOnPagedWorkspace: DisableOverlaysOnPagedWorkspace,
        CloseLoginModalWindow: CloseLoginModalWindow,
        OnError: OnError,
        autoupdate: autoupdate,
        SiteStatusAdmin_Clicked: SiteStatusAdmin_Clicked,
        CloseAdminOnlineInformationWindow: CloseAdminOnlineInformationWindow,
        AlertWindow: AlertWindow,
        CloseAlertWindow: CloseAlertWindow,
        ReportAlert: ReportAlert,
        ConfirmWindow: ConfirmWindow,
        ConfirmWindowAltBtns: ConfirmWindowAltBtns,
        CloseConfirmWindow: CloseConfirmWindow,
        OnBrowserClose: OnBrowserClose,
        SaveAccordionState: SaveAccordionState,
        CheckIfWorkspaceLinkAvailable: CheckIfWorkspaceLinkAvailable,
        RadioButtonStyle: RadioButtonStyle,
        RatingStyleInit: RatingStyleInit,
        UpdateAppRating: UpdateAppRating,
        ResetRating: ResetRating,
        OpenAppNoti: OpenAppNoti,
        SearchSite: SearchSite,
        SearchExternalSite: SearchExternalSite,
        LoadModalWindow: LoadModalWindow,
        SaveInnerModalContent: SaveInnerModalContent,
        LoadSavedInnerModalContent: LoadSavedInnerModalContent,
        ExpandAdminLinks: ExpandAdminLinks,
        HelpOverlay: HelpOverlay,
        HelpMenuPageLoad: HelpMenuPageLoad,
        NewUserPageLoad: NewUserPageLoad,
        NewUserfinsh: NewUserfinsh,
        HelpMenuPageLoadWorkspace: HelpMenuPageLoadWorkspace,
        HelpIntroRestart: HelpIntroRestart,
        HelpIntroNext: HelpIntroNext,
        HelpIntroBack: HelpIntroBack,
        OnEmailUpdate_KeyPress: OnEmailUpdate_KeyPress,
        OnPasswordUpdate_KeyPress: OnPasswordUpdate_KeyPress,
        UpdateEmail: UpdateEmail,
        UpdateAdminPassword: UpdateAdminPassword,
        ShowUpdatesPopup: ShowUpdatesPopup,
        ShowActivationPopup: ShowActivationPopup,
        CloseUpdatesPopup: CloseUpdatesPopup,
        OverlayDisable: OverlayDisable,
        TryAddLoadOverlay: TryAddLoadOverlay,
        TryRemoveLoadOverlay: TryRemoveLoadOverlay,
        CallOverlayList: CallOverlayList,
        AddRemoveOverlayClick: AddRemoveOverlayClick,
        UpdateOverlayTable: UpdateOverlayTable,
        GetUserNotifications: GetUserNotifications,
        NotiActionsHideInd: NotiActionsHideInd,
        UpdateNotificationCount: UpdateNotificationCount,
        CloseNoti: CloseNoti,
        LoadCreateAccountHolder: LoadCreateAccountHolder,
        LoadRecoveryPassword: LoadRecoveryPassword,
        LoadIFrameContent: LoadIFrameContent,
        LoadIFrameContentHistory: LoadIFrameContentHistory,
        CloseIFrameContent: CloseIFrameContent,
        BackgroundSelector: BackgroundSelector,
        ClearBackground: ClearBackground,
        updateBackgroundURL: updateBackgroundURL,
        updateBackgroundColor: updateBackgroundColor,
        BackgroundImageSelect_Clicked: BackgroundImageSelect_Clicked,
        BackgroundImageDelete_Clicked: BackgroundImageDelete_Clicked,
        BackgroundImageAddRemove_Clicked: BackgroundImageAddRemove_Clicked,
        SetBackgroundForWorkspaceDropdown: SetBackgroundForWorkspaceDropdown,
        HideTasks: HideTasks,
        ShowTasks: ShowTasks,
        LoadCurrentWorkspace: LoadCurrentWorkspace,
        Getworkspace: Getworkspace,
        HoverWorkspacePreview: HoverWorkspacePreview,
        SetWorkspaceNumber: SetWorkspaceNumber,
        AutoRotateWorkspace: AutoRotateWorkspace,
        AutoUpdateOnRotate: AutoUpdateOnRotate,
        ToggleMinimizedAppBar: ToggleMinimizedAppBar,
        AppsSortUnlocked: AppsSortUnlocked,
        CreateSOApp: CreateSOApp,
        BuildAppMinIcon: BuildAppMinIcon,
        LoadApp: LoadApp,
        LoadAppFromSiteTools: LoadAppFromSiteTools,
        DetermineNeedPostBack: DetermineNeedPostBack,
        WatchForLoad: WatchForLoad,
        LoadUserControl: LoadUserControl,
        MoveOutsideModalWindows: MoveOutsideModalWindows,
        ResizeAllAppBody: ResizeAllAppBody,
        ResizeAppBody: ResizeAppBody,
        ApplyOverlayFix: ApplyOverlayFix,
        RemoveOverlayFix: RemoveOverlayFix,
        MoveOffScreen: MoveOffScreen,
        MoveOnScreen_WorkspaceOnly: MoveOnScreen_WorkspaceOnly,
        MoveToCurrworkspace: MoveToCurrworkspace,
        SetAppMaxToMin: SetAppMaxToMin,
        SetAppMinToMax: SetAppMinToMax,
        HoverOverAppMin: HoverOverAppMin,
        HoverOutAppMin: HoverOutAppMin,
        NotiActionsClearAll: NotiActionsClearAll,
        CheckIfCanAddMore: CheckIfCanAddMore,
        SetActiveApp: SetActiveApp,
        SetDeactiveApps: SetDeactiveApps,
        SetDeactiveAll: SetDeactiveAll,
        LoadActiveAppCookie: LoadActiveAppCookie,
        MaximizeApp: MaximizeApp,
        StartRemoteLoad: StartRemoteLoad,
        LoadRemotely: LoadRemotely,
        SetRemoteLoadingOptions: SetRemoteLoadingOptions,
        SetAppIconActive: SetAppIconActive,
        AddworkspaceAppNum: AddworkspaceAppNum,
        RemoveworkspaceAppNum: RemoveworkspaceAppNum,
        RemoveAppIconActive: RemoveAppIconActive,
        CategoryClick: CategoryClick,
        LoadCategorySections: LoadCategorySections,
        LoadCategoryCookies: LoadCategoryCookies,
        ApplyAppDragResize: ApplyAppDragResize,
        ReloadApp: ReloadApp,
        AboutApp: AboutApp,
        AboutApp_PagedVersion: AboutApp_PagedVersion,
        UninstallApp: UninstallApp,
        PopOutFrame: PopOutFrame,
        PopOutFrameFromSiteTools: PopOutFrameFromSiteTools,
        PopOutTool: PopOutTool,
        MoveAppToworkspace: MoveAppToworkspace,
        GroupLoginModal: GroupLoginModal,
        GetandBuildGroupList: GetandBuildGroupList,
        LoginAsGroup: LoginAsGroup,
        HashChange: HashChange,
        AcceptGroupNotification: AcceptGroupNotification,
        MobileMode: MobileMode,
        ResizeContainer: ResizeContainer,
        GetElementClassList: GetElementClassList,
        ShowHideAccordianSidebar: ShowHideAccordianSidebar,
        ReloadPage: ReloadPage,
        ApplyMobileModeForMenuBar: ApplyMobileModeForMenuBar,
        BackgroundLoop: BackgroundLoop,
        ChangeImageFolder: ChangeImageFolder,
        UpdateBackgroundSetting: UpdateBackgroundSetting,
        ChangeUserProfileImage: ChangeUserProfileImage,
        CloseTopDropDowns: CloseTopDropDowns,
        ResizeTopDropDowns: ResizeTopDropDowns,
        RgbToHex: RgbToHex,
        HexToRgb: HexToRgb,
        UseDarkTextColorWithBackground: UseDarkTextColorWithBackground,
        GetAltColorFromHex: GetAltColorFromHex,
        GetScriptFunction: GetScriptFunction,
        SetCurrentWorkspaceBackground: SetCurrentWorkspaceBackground,
        LoadCSSFilesInApp: LoadCSSFilesInApp,
        InitializeSiteMenuTabs: InitializeSiteMenuTabs,
        LoadSiteMenuTab: LoadSiteMenuTab,
        ScrollToElement: ScrollToElement,
        TableFormulas: TableFormulas,
        GridViewMethods: GridViewMethods,
        CreateXMLHttpRequest: CreateXMLHttpRequest,
        InitializeThemeColorOption: InitializeThemeColorOption,
        ThemeColorOption_Clicked: ThemeColorOption_Clicked,
        ColorOption_Changed: ColorOption_Changed,
        ResetColorOption_Clicked: ResetColorOption_Clicked,
        SetNewThemeColorOptionFormat: SetNewThemeColorOptionFormat,
        HideSidebar: HideSidebar,
        CloseWindow: CloseWindow,
        AnimateToTop: AnimateToTop,
        SidebarNavToggleClicked: SidebarNavToggleClicked,
        LogConsoleMessage: LogConsoleMessage,
        CalendarViewApps: CalendarViewApps,
        noAnimationForSortableRows: noAnimationForSortableRows,
        AjaxCall: AjaxCall,
        CallDoPostBack: CallDoPostBack,
        UpdateLogoSizeOnLoad: UpdateLogoSizeOnLoad
    }
}();

$(window).resize(function () {
    openWSE.SetContainerTopPos(true);
    openWSE.ResizeContainer();
    var current = openWSE.Getworkspace();
    if ($("#MainContent_" + current).length > 0) {
        openWSE.ResizeAllAppBody($("#MainContent_" + current));
    }
    else {
        openWSE.ResizeAllAppBody($("body"));
    }

    openWSE.ApplyMobileModeForMenuBar();
    openWSE.AdjustModalWindowView();
    openWSE.InitializeSlimScroll();
    openWSE.ResizeTopDropDowns();
    openWSE.CalendarViewApps.ResizeCalendars();
});

$(document).ready(function () {
    openWSE.init();
    openWSE.SetContainerTopPos(false);
    openWSE.LoadCategoryCookies();
    openWSE.GetUserNotifications();
    openWSE.HashChange();
    openWSE.ResizeContainer();
    openWSE.SetDuplicateAppIcons();

    var current = openWSE.Getworkspace();
    openWSE.ResizeAllAppBody($("#MainContent_" + current));

    // Auto complete for app searching
    if ($(".searchwrapper-tools-search").find("input[type='text']").length > 0) {
        $.widget("custom.catcomplete", $.ui.autocomplete, {
            _create: function () {
                this._super();
                this.widget().menu("option", "items", "> :not(.ui-autocomplete-category)");
            },
            _renderMenu: function (ul, items) {
                var that = this;
                var currentCategory = "";
                $.each(items, function (index, item) {
                    var li;
                    if (item.category != currentCategory) {
                        ul.append("<li class='ui-autocomplete-category'>" + item.category + "</li>");
                        currentCategory = item.category;
                    }
                    li = that._renderItemData(ul, item);
                    if (item.category) {
                        li.attr("aria-label", item.value);
                        li.html(item.label);
                    }
                });
            }
        });

        $(".searchwrapper-tools-search").find("input[type='text']").catcomplete({
            minLength: 1,
            autoFocus: true,
            open: function (event, ui) {
                $(this).catcomplete("widget").css({
                    width: "auto",
                    minWidth: "235px",
                    maxWidth: "500px"
                });
            },
            source: function (request, response) {
                openWSE.AjaxCall("WebServices/AutoComplete.asmx/GetAppSearchList", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                    dataFilter: function (data) { return data; }
                }, function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item[1],
                            value: item[0],
                            category: item[2]
                        }
                    }));
                });
            },
            select: function (event, ui) {
                setTimeout(function () {
                    openWSE.SearchSite();
                }, 1);
            }
        }).focus(function () {
            $(this).catcomplete("search", "");
        });
    }

    // openWSE.AdjustTableSettingsBox();

    if (openWSE_Config.hideSearchBarInTopBar) {
        $(".searchwrapper-tools-search").addClass("hide-searchwarapper-tools-search");
    }

    openWSE.CheckIfOverlaysExistsOnNonComplex();
});

$(function () {
    $(window).hashchange(function () {
        openWSE.HashChange();
    });

    if (openWSE_Config.displayLoadingOnRedirect) {
        $("#lnk_BackToWorkspace, .app-icon-links, .app-icon-sub-links, .account-link-style").click("click", function (e) {
            if (e.target.className != "img-expand-sml" && e.target.className != "img-collapse-sml") {
                var onSamePage = false;

                try {
                    var thisHref = $(this).attr("href").substring($(this).attr("href").lastIndexOf("/") + 1).toLowerCase();
                    var thisLocation = window.location.href.substring(window.location.href.lastIndexOf("/") + 1).toLowerCase();

                    thisHref = thisHref.split("?")[0];
                    thisLocation = thisLocation.split("?")[0];

                    if (thisHref.indexOf(thisLocation) != -1 || thisLocation.indexOf(thisHref) != -1) {
                        onSamePage = true;
                    }
                }
                catch (evt) { }

                if ($(this).attr("target") != "_blank" && !onSamePage && !$(this).attr("onclick")) {
                    loadingPopup.Message("Loading...");
                }
            }
        });

        if (!openWSE.IsComplexWorkspaceMode()) {
            $(".app-icon").click("click", function (e) {
                if (e.target.className != "app-options" && $(this).attr("target") != "_blank") {
                    loadingPopup.Message("Loading...");
                }
            });
        }
    }
});

window.onerror = function (errorMsg, url) {
    loadingPopup.RemoveMessage();

    url = window.location.href;

    // Small hack for the Network Log
    if (url.toLowerCase().indexOf("analytics.aspx") != -1 && (errorMsg == "Uncaught TypeError: boolean is not a function" || errorMsg == "Script error.")) {
        return;
    }

    if (errorMsg === "error" || errorMsg === "Script error." || errorMsg === "Internal Server Error") {
        return;
    }

    if (!openWSE_Config.reportAlert) {
        openWSE.OnError(errorMsg, url);
    }
    else if (openWSE_Config.reportOnError) {
        openWSE.AlertWindow(errorMsg, url);
    }

    openWSE.LogConsoleMessage(errorMsg);
}

window.onload = function () {
    openWSE.ApplyAppDragResize();
    var current = openWSE.Getworkspace();
    openWSE.ResizeAllAppBody($("#MainContent_" + current));

    openWSE.LoadActiveAppCookie();
    if (!openWSE_Config.taskBarShowAll) {
        $(".app-min-bar").hide();
        $("#MainContent_" + current).find(".app-main-holder").each(function (index) {
            var id = $(this).attr("data-appid");
            if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").length != 0) {
                if ($("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").css("display") == "none") {
                    $("#minimized_app_bar_holder").find(".app-min-bar[data-appid='" + id + "']").show();
                }
            }
        });
    }

    openWSE.ToggleMinimizedAppBar();
    openWSE.MoveOutsideModalWindows();
    openWSE.ApplyMobileModeForMenuBar();
    openWSE.SetCurrentWorkspaceBackground();
};

(function (e) {
    e.fn.extend({
        slimScroll: function (f) {
            var a = e.extend({ width: "auto", height: "250px", size: "7px", color: "#000", position: "right", distance: "1px", start: "top", opacity: .4, alwaysVisible: !1, disableFadeOut: !1, railVisible: !1, railColor: "#333", railOpacity: .2, railDraggable: !0, railClass: "slimScrollRail", barClass: "slimScrollBar", wrapperClass: "slimScrollDiv", allowPageScroll: !1, wheelStep: 20, touchScrollStep: 200, borderRadius: "7px", railBorderRadius: "7px" }, f); this.each(function () {
                function v(d) {
                    if (r) {
                        d = d || window.event;
                        var c = 0; d.wheelDelta && (c = -d.wheelDelta / 120); d.detail && (c = d.detail / 3); e(d.target || d.srcTarget || d.srcElement).closest("." + a.wrapperClass).is(b.parent()) && n(c, !0); d.preventDefault && !k && d.preventDefault(); k || (d.returnValue = !1)
                    }
                } function n(d, g, e) {
                    k = !1; var f = b.outerHeight() - c.outerHeight(); g && (g = parseInt(c.css("top")) + d * parseInt(a.wheelStep) / 100 * c.outerHeight(), g = Math.min(Math.max(g, 0), f), g = 0 < d ? Math.ceil(g) : Math.floor(g), c.css({ top: g + "px" })); l = parseInt(c.css("top")) / (b.outerHeight() - c.outerHeight()); g =
                    l * (b[0].scrollHeight - b.outerHeight()); e && (g = d, d = g / b[0].scrollHeight * b.outerHeight(), d = Math.min(Math.max(d, 0), f), c.css({ top: d + "px" })); b.scrollTop(g); b.trigger("slimscrolling", ~~g); w(); p()
                } function x() { u = Math.max(b.outerHeight() / b[0].scrollHeight * b.outerHeight(), 30); c.css({ height: u + "px" }); var a = u == b.outerHeight() ? "none" : "block"; c.css({ display: a }) } function w() {
                    x(); clearTimeout(B); l == ~~l ? (k = a.allowPageScroll, C != l && b.trigger("slimscroll", 0 == ~~l ? "top" : "bottom")) : k = !1; C = l; u >= b.outerHeight() ? k = !0 : (c.stop(!0,
                    !0).fadeIn("fast"), a.railVisible && m.stop(!0, !0).fadeIn("fast"))
                } function p() { a.alwaysVisible || (B = setTimeout(function () { a.disableFadeOut && r || y || z || (c.fadeOut("slow"), m.fadeOut("slow")) }, 1E3)) } var r, y, z, B, A, u, l, C, k = !1, b = e(this); if (b.parent().hasClass(a.wrapperClass)) {
                    var q = b.scrollTop(), c = b.siblings("." + a.barClass), m = b.siblings("." + a.railClass); x(); if (e.isPlainObject(f)) {
                        if ("height" in f && "auto" == f.height) {
                            b.parent().css("height", "auto"); b.css("height", "auto"); var h = b.parent().parent().height(); b.parent().css("height",
                            h); b.css("height", h)
                        } else "height" in f && (h = f.height, b.parent().css("height", h), b.css("height", h)); if ("scrollTo" in f) q = parseInt(a.scrollTo); else if ("scrollBy" in f) q += parseInt(a.scrollBy); else if ("destroy" in f) { c.remove(); m.remove(); b.unwrap(); return } n(q, !1, !0)
                    }
                } else if (!(e.isPlainObject(f) && "destroy" in f)) {
                    a.height = "auto" == a.height ? b.parent().height() : a.height; q = e("<div></div>").addClass(a.wrapperClass).css({ position: "relative", overflow: "hidden", width: a.width, height: a.height }); b.css({
                        overflow: "hidden",
                        width: a.width, height: a.height
                    }); var m = e("<div></div>").addClass(a.railClass).css({ width: a.size, height: "100%", position: "absolute", top: 0, display: a.alwaysVisible && a.railVisible ? "block" : "none", "border-radius": a.railBorderRadius, background: a.railColor, opacity: a.railOpacity, zIndex: 90 }), c = e("<div></div>").addClass(a.barClass).css({
                        background: a.color, width: a.size, position: "absolute", top: 0, opacity: a.opacity, display: a.alwaysVisible ? "block" : "none", "border-radius": a.borderRadius, BorderRadius: a.borderRadius, MozBorderRadius: a.borderRadius,
                        WebkitBorderRadius: a.borderRadius, zIndex: 99
                    }), h = "right" == a.position ? { right: a.distance } : { left: a.distance }; m.css(h); c.css(h); b.wrap(q); b.parent().append(c); b.parent().append(m); a.railDraggable && c.bind("mousedown", function (a) { var b = e(document); z = !0; t = parseFloat(c.css("top")); pageY = a.pageY; b.bind("mousemove.slimscroll", function (a) { currTop = t + a.pageY - pageY; c.css("top", currTop); n(0, c.position().top, !1) }); b.bind("mouseup.slimscroll", function (a) { z = !1; p(); b.unbind(".slimscroll") }); return !1 }).bind("selectstart.slimscroll",
                    function (a) { a.stopPropagation(); a.preventDefault(); return !1 }); m.hover(function () { w() }, function () { p() }); c.hover(function () { y = !0 }, function () { y = !1 }); b.hover(function () { r = !0; w(); p() }, function () { r = !1; p() }); b.bind("touchstart", function (a, b) { a.originalEvent.touches.length && (A = a.originalEvent.touches[0].pageY) }); b.bind("touchmove", function (b) { k || b.originalEvent.preventDefault(); b.originalEvent.touches.length && (n((A - b.originalEvent.touches[0].pageY) / a.touchScrollStep, !0), A = b.originalEvent.touches[0].pageY) });
                    x(); "bottom" === a.start ? (c.css({ top: b.outerHeight() - c.outerHeight() }), n(0, !0)) : "top" !== a.start && (n(e(a.start).position().top, null, !0), a.alwaysVisible || c.hide()); window.addEventListener ? (this.addEventListener("DOMMouseScroll", v, !1), this.addEventListener("mousewheel", v, !1)) : document.attachEvent("onmousewheel", v)
                }
            }); return this
        }
    }); e.fn.extend({ slimscroll: e.fn.slimScroll })
})(jQuery);
