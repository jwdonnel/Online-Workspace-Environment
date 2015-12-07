// -----------------------------------------------------------------------------------
//
//	Chat Client v3.2
//	by John Donnelly
//	Last Modification: 7/27/2015
//
//	Licensed under the Creative Commons Attribution 2.5 License - http://creativecommons.org/licenses/by/2.5/
//  	- Free for use in both personal and commercial projects
//		- Attribution requires leaving author name, author link, and the license info intact.
//
//  Min-Requirements:
//      jquery v1.11.0 - http://code.jquery.com/jquery-1.11.1.min.js
//      migrate.jquery.min.js - http://code.jquery.com/jquery-migrate-1.2.1.min.js
//
// -----------------------------------------------------------------------------------


var chatClient = function () {

    var newtitleMessage;
    var notificationCleared = 0;
    var currTitle = document.title;
    var pageUrl = 'ChatClient/ChatService.asmx';
    var isPostBack = 1;
    var chatDividers = true;
    var _isUpdateUsersCall = false;
    var _isUpdateNewCall = false;
    var _isUpdateFocusCall = false;
    var _isUpdateStatusCall = false;

    function init() {
        pageUrl = GetSiteRoot() + 'ChatClient/ChatService.asmx';
    }

    function ConvertBitToBoolean(value) {
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

        return false;
    }

    $(document.body).on("click", "#container, #container-logo, .workspace-holder", function () {
        if (($("#ul_StatusList").css("display") == "block") || ($("#ul_StatusList").css("display") == "")) {
            $("#ul_StatusList").fadeOut(openWSE_Config.animationSpeed);
        }

        if ($("#span_currStatus").text() == "Away") {
            updateWindowFocus();
        }
    });
    $(document.body).on("click", ".chat-modal", function () {
        var user = $(this).attr("chat-username");
        var userId = $(this).attr("data-appid");
        
        RemoveIsNew(user, userId);
    });
    $(document.body).on("click", ".ChatUserNotSelected, .ChatUserSelected", function () {
        var fullName = $(this).find("span").html();
        var user = $(this).attr("chat-username");
        var userId = $(this).find("span").attr("chat-userid");

        try {
            if ($("#workspace_holder").length > 0 && openWSE.IsComplexWorkspaceMode()) {
                BuildChatWindow(user, userId, fullName);
            }
            else {
                openWSE.PopOutFrame(this, GetSiteRoot() + "ChatClient/ChatWindow.html?user=" + user);
            }
        }
        catch (evt) {
            appRemote_Config.categoryId = "chatClient";
            appRemote_Config.categoryName = "userSelect";
            appRemote_Config.remoteId = user;
            appRemote_Config.remoteName = fullName;
            appRemote.UpdateURL();
            BuildChatWindowMobile(user);
        }

        RemoveIsNew(user, userId);

        return false;
    });
    $(document.body).on("click", ".updatestatus, .chatstatus_mid", function () {
        var _status;
        if ($(this).find("a").length > 0) {
            _status = $(this).find("a").attr("href").split("#")[1];
        }
        else {
            _status = $(this).attr("href").split("#")[1];
        }

        if ((_status != "#") && (_status != "")) {
            updateCurrStatus(_status);
        }
        var cf = document.getElementById("ul_StatusList");
        if (cf.style.display == "none")
            $("#ul_StatusList").fadeIn(150);
        else
            $("#ul_StatusList").fadeOut(150);

        return false;
    });
    $.extend({
        playSound: function () {
            var filename = arguments[0];
            var x = '<div style="visibility:hidden"><audio autoplay="autoplay"><source src="' + filename + '" type="audio/x-wav" />';
            x += '<embed hidden="true" autostart="true" loop="false" src="' + filename + '" /></audio></div>';
            $('#playSound').html(x);
        }
    });

    function GetSiteRoot() {
        if (typeof openWSE == "object" || typeof openWSE == "function") {
            return openWSE.siteRoot();
        }

        return "";
    }

    function BuildChatWindow(user, userId, fullName) {
        var workspace = openWSE.Getworkspace();
        var id = "app-ChatClient-" + userId;
        if ($(".app-main-holder[data-appid='" + id + "']").length == 0) {
            var currStatus = $(".ChatUserNotSelected[chat-username='" + user + "']").find(".statusUserDiv").attr("class");
            if ((currStatus != "") && (currStatus != null) && (currStatus != undefined)) {
                currStatus = currStatus.replace("statusUserDiv ", "");
            }

            var appStyle = "app-main-style1";
            if (openWSE_Config != null) {
                appStyle = "app-main-style" + openWSE_Config.appStyle.toLowerCase().replace("style_", "");
            }

            var modal = "<div data-appid='" + id + "' chat-username='" + user + "' class='app-main-holder app-main chat-modal " + appStyle + "' style='display: none; min-height: 425px; min-width: 315px;'>";
            
            if (appStyle == "app-main-style1") {
                modal += BuildChatButtonHolder(id, user);
                modal += BuildChatHeader(fullName, currStatus);
                modal += BuildChatBody();
            }
            else if (appStyle == "app-main-style2") {
                modal += BuildChatBody();
                modal += BuildChatButtonHolder(id, user);
                modal += BuildChatHeader(fullName, currStatus);
            }
            else if (appStyle == "app-main-style3") {
                modal += BuildChatBody();
                modal += BuildChatButtonHolder(id, user);
                modal += BuildChatHeader(fullName, currStatus);
            }

            modal += "</div>";
            $("#MainContent_" + workspace).append(modal);
        }

        if ($(".app-main-holder[data-appid='" + id + "']").length > 0) {
            openWSE.ApplyAppDragResize();

            var $this = $(".app-main-holder[data-appid='" + id + "']");
            openWSE.LoadApp($this, workspace);
            if ($this.hasClass("active") == false) {
                $this.addClass("active");
            }

            updateisNewChat(user);
        }
    }

    function BuildChatButtonHolder(id, user) {
        var modal = "";

        if (openWSE_Config && openWSE_Config.appStyle == "Style_3") {
            modal += "<div class='app-head-hover-button'></div>";
        }

        modal += "<div class='app-head-button-holder'>";
        if (openWSE_Config && openWSE_Config.appStyle == "Style_3") {
            modal += "<a href='#" + id + "' class='move-button-app' title='Move app'><span></span></a>";
        }
        modal += "<a href='#" + id + "' class='options-button-app' title='View app options'><span></span></a>";
        modal += "<div class='app-popup-inner-app'>";
        modal += "<table><tbody><tr><td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>";
        modal += "<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a><span>Refresh</span></li>";
        modal += "<li onclick=\"openWSE.PopOutFrame(this,'ExternalAppHolder.aspx?chatuser=" + user + "')" + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a><span>Pop out</span></li>";
        modal += "<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div><span>About</span></li></ul></td></tr>";

        if ($("#ddl_WorkspaceSelector").length > 0) {
            modal += "<tr><td><div class='clear-space'></div><span class='workspace-selector-app-option'>Workspace</span>";
            modal += "<select class='app-options-workspace-switch'>";
            var totalWorkspaces = $("#ddl_WorkspaceSelector").find(".dropdown-db-selector").find(".workspace-selection-item").length;
            for (var ii = 0; ii < totalWorkspaces; ii++) {
                modal += "<option value='" + (ii + 1).toString() + "'>" + (ii + 1).toString() + "</option>";
            }
            modal += "</select></td></tr>";
        }

        modal += "</tbody></table></div>";
        modal += "<a href='#" + id + "' class='exit-button-app' title='Close'><span></span></a>";
        modal += "<a href='#" + id + "' class='maximize-button-app' title='Maximize'><span></span></a>";
        modal += "<a href='#" + id + "' class='minimize-button-app' title='Minimize'><span></span></a></div>";

        return modal;
    }
    function BuildChatHeader(fullName, currStatus) {
        var modal = "<div class='app-head app-head-dblclick'>";
        modal += "<div class='app-header-icon statusUserDiv2 margin-right-sml " + currStatus + "'></div>";
        modal += "<span class='app-title'>" + fullName + "</span></div>";

        return modal;
    }
    function BuildChatBody() {
        return "<div class='app-body'></div>";
    }

    function BuildChatWindowMobile(user) {
        $("#pnl_icons, #pnl_chat_users").hide();
        $("#pnl_chat_popup").fadeIn(150);
        if ($(".iFrame-chat").length > 0) {
            $(".iFrame-chat").each(function () {
                $(this).remove();
            });
        }
        $("#pnl_chat_popup").append("<iframe class='iFrame-chat' src='" + GetSiteRoot() + "ChatClient/ChatWindow.html?user=" + user + "' width='100%' frameborder='0'></iframe>");
        updateisNewChat(user);
        $(window).resize();
    }
    function getUsersOnline() {
        var total;
        total = $get("chattersOnline").innerHTML;
        return total;
    }
    function updateChatUsers() {
        if (!_isUpdateUsersCall) {
            _isUpdateUsersCall = true;
            $.ajax({
                type: "POST",
                url: pageUrl + "/CallUserList",
                data: '{ "currstatus": "' + $("#span_currStatus").html() + '","isPostBack": "' + isPostBack + '" }',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                cache: false,
                success: function (data) {
                    if (data.d[0] != null)
                        getCurrStatus(data.d[0]);
                    if (data.d[1] != null)
                        updateUserList(data.d[1]);
                    if (data.d[2] != null) {
                        GetNewChatsworkspace(data.d[2]);
                    }

                    isPostBack = 0;
                    _isUpdateUsersCall = false;

                    if (!chatDividers) {
                        $('.ChatUserNotSelected, .ChatUserSelected').css('background-image', 'none');
                    }

                    updateChatUsers();
                },
                error: function (data) {
                    isPostBack = 0;
                    _isUpdateUsersCall = false;

                    if (!chatDividers) {
                        $('.ChatUserNotSelected, .ChatUserSelected').css('background-image', 'none');
                    }

                    updateChatUsers();
                }
            });
        }
    }
    function updateUserList(data) {
        if (document.getElementById("chatuserlist") == null) {
            return;
        }

        var chatuserlist = document.getElementById("chatuserlist").innerHTML;

        // Remove Selected User Styles
        chatuserlist = chatuserlist.replace(/"/g, "'");
        chatuserlist = chatuserlist.replace(/ ChatUserSelected/g, "");
        chatuserlist = chatuserlist.replace(/ChatUserSelected /g, "");
        chatuserlist = chatuserlist.replace(/ chatisNew/g, "");
        chatuserlist = chatuserlist.replace(/chatisNew/g, "");
        chatuserlist = chatuserlist.replace(/;/g, "");
        chatuserlist = chatuserlist.replace(/%2F/g, "");

        if ((data != "") && (data.toLowerCase() != chatuserlist.toLowerCase())) {
            $('#chatuserlist').html(data);

            var totalusers = $('.listofusers li').size();
            var chatusersNone = document.getElementById("chatusers_none");
            if ((totalusers == "0") && ((chatusersNone == null) || (chatusersNone == undefined))) {
                $("#header-total-online").html("0");
                $('#chatuserlist').html("<div id='chatusers_none'>No users available</div>");
            }
            else {
                var elem = document.getElementById("chatuserlist");
                elem.scrollTop = elem.scrollHeight;
                var totalonline = getUsersOnline();
                if (totalonline == "none")
                    totalonline = "no users ";

                $("#header-total-online").html(totalonline);

                $(".chat-modal").each(function () {
                    var thisId = $(this).attr("data-appid");
                    thisId = thisId.replace("app-ChatClient-", "");
                    var currStatus = $(".ChatUserNotSelected").find("span[chat-userid='" + thisId + "']").parent().find(".statusUserDiv").attr("class");
                    if ((currStatus != "") && (currStatus != null)) {
                        var n = currStatus.replace("statusUserDiv ", "");
                        $(this).find(".app-header-icon").attr("class", "app-header-icon statusUserDiv2 margin-right-sml " + n);
                    }

                    if ($(this).css("display") == "block") {
                        openWSE.SetAppIconActive(this);
                    }
                });
            }
        }
    }
    function GetNewChatsworkspace(data) {
        var tabSelected = true;
        var current = "";
        if (typeof Getworkspace == "function") {
            current = Getworkspace();
        }

        for (var i = 0; i < data.length; i++) {
            var splitData = data[i].split("~|~");
            if (splitData.length == 2) {
                var user = splitData[0];
                var fullName = splitData[1];
                var $chatSession = $(".app-main-holder[data-appid='app-ChatClient-" + user + "']");
                var $chatSessionMin = $(".app-min-bar[data-appid='app-ChatClient-" + user + "']");
                var appworkspace = "";
                if ($chatSession.closest(".workspace-holder").length > 0) {
                    appworkspace = $chatSession.closest(".workspace-holder").attr("id").replace("MainContent_", "");
                }

                if ($chatSessionMin.length > 0) {
                    if (!$chatSessionMin.hasClass("chatisNew")) {
                        $chatSessionMin.addClass("chatisNew");
                        if ((!$(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().hasClass("chatisNew")) && (!tabSelected)) {
                            $(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().addClass("chatisNew");
                        }
                        flashTitle(fullName);
                    }
                }
                else if (($chatSession.length > 0) && ($chatSession.css("display") == "block")) {
                    if ((!$chatSession.find(".app-head").hasClass("chatisNew")) && ((!$chatSession.hasClass("selected")) || (current != appworkspace))) {
                        $chatSession.find(".app-head").addClass("chatisNew");
                        if ((!$(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().hasClass("chatisNew")) && ((!tabSelected) || (current != appworkspace))) {
                            $(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().addClass("chatisNew");
                        }
                        flashTitle(fullName);
                    }
                }
                else {
                    var canFlash = true;
                    if ($(".iFrame-chat").length > 0) {
                        canFlash = false;
                    }

                    if (canFlash) {
                        if (!$(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().hasClass("chatisNew")) {
                            $(".ChatUserNotSelected").find("span[chat-userid='" + user + "']").parent().addClass("chatisNew");
                            flashTitle(fullName);
                        }
                    }
                }
            }
        }
    }
    function RemoveIsNew(user, userId) {
        if ($(".app-main-holder[data-appid='" + userId + "']").find(".app-head").hasClass("chatisNew")) {
            $(".app-main-holder[data-appid='" + userId + "']").find(".app-head").removeClass("chatisNew");
            notificationCleared = 1;
        }

        if ($(".ChatUserNotSelected[chat-username='" + user + "']").hasClass("chatisNew")) {
            $(".ChatUserNotSelected[chat-username='" + user + "']").removeClass("chatisNew");
            notificationCleared = 1;
        }
    }
    function displayMessageNoti(user) {
        try {
            updateisNewChat(user);
            if (document.title != currTitle) {
                notificationCleared = 1;
                document.title = currTitle;
            }
        } catch (evt) {
            delete evt;
        }
    }
    function getCurrStatus(data) {
        var response = data;
        if (response != null) {
            response = response.replace(/"/gi, "'");
            var elem = document.getElementById("currentStatus");
            if (elem != null) {
                if ((elem.innerHTML.replace(/"/gi, "'") != response) && (response != "")) {
                    elem.innerHTML = response;
                }
            }
        }
    }
    function updateisNewChat(userto) {
        if (!_isUpdateNewCall) {
            _isUpdateNewCall = true;
            $.ajax({
                type: "POST",
                url: pageUrl + "/CallUpdate",
                data: '{ "userto": "' + userto + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (!chatClient.ConvertBitToBoolean(data.d)) {
                        RemoveIsNew(userto);
                        if (document.title != currTitle) {
                            notificationCleared = 1;
                            document.title = currTitle;
                        }
                    }

                    _isUpdateNewCall = false;
                },
                error: function (data) {
                    _isUpdateNewCall = false;
                }
            });
        }
    }
    function updateCurrStatus(status) {
        if (!_isUpdateStatusCall) {
            _isUpdateStatusCall = true;
            $.ajax({
                type: "POST",
                url: pageUrl + "/CallUpdateStatus",
                data: '{ "status": "' + status + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    response = response.replace(/"/gi, "'");
                    var elem = document.getElementById("currentStatus");
                    if ((elem.innerHTML.replace(/"/gi, "'") != response) && (response != ""))
                        elem.innerHTML = response;

                    _isUpdateStatusCall = false;
                },
                error: function (data) {
                    _isUpdateStatusCall = false;
                }
            });
        }
    }
    function updateWindowFocus() {
        if (!_isUpdateFocusCall) {
            _isUpdateFocusCall = true;
            $.ajax({
                type: "POST",
                url: pageUrl + "/CallWindowFocus",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    response = response.replace(/"/gi, "'");
                    var elem = document.getElementById("currentStatus");
                    if ((elem.innerHTML.replace(/"/gi, "'") != response) && (response != ""))
                        elem.innerHTML = response;

                    _isUpdateFocusCall = false;
                },
                error: function (data) {
                    _isUpdateFocusCall = false;
                }
            });
        }
    }
    function flashTitle(user) {
        playSound();
        newtitleMessage = setInterval(function () {
            if (notificationCleared == 1) {
                document.title = currTitle;
                clearInterval(newtitleMessage);
            }
            else {
                if (document.title != user + " Says...")
                    document.title = user + " Says...";
                else
                    document.title = currTitle;

            }
        }, 1500);
    }
    function playSound() {
        if (chatClient.ConvertBitToBoolean($("#hf_chatsound").val())) {
            $.playSound(GetSiteRoot() + 'ChatClient/Notifications/notification.wav');
        }
    }

    function updateChatUsers_NoLoop() {
        $.ajax({
            type: "POST",
            url: pageUrl + "/CallUserList",
            data: '{ "currstatus": "' + $("#span_currStatus").html() + '","isPostBack": "1" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data.d[0] != null)
                    getCurrStatus(data.d[0]);
                if (data.d[1] != null)
                    updateUserList(data.d[1]);
                if (data.d[2] != null) {
                    GetNewChatsworkspace(data.d[2]);
                }

                if (!chatDividers) {
                    $('.ChatUserNotSelected, .ChatUserSelected').css('background-image', 'none');
                }
            },
            error: function (data) {
                if (!chatDividers) {
                    $('.ChatUserNotSelected, .ChatUserSelected').css('background-image', 'none');
                }
            }
        });
    }

    return {
        init: init,
        ConvertBitToBoolean: ConvertBitToBoolean,
        BuildChatWindow: BuildChatWindow,
        BuildChatWindowMobile: BuildChatWindowMobile,
        getUsersOnline: getUsersOnline,
        updateChatUsers: updateChatUsers,
        updateUserList: updateUserList,
        GetNewChatsworkspace: GetNewChatsworkspace,
        RemoveIsNew: RemoveIsNew,
        displayMessageNoti: displayMessageNoti,
        getCurrStatus: getCurrStatus,
        updateisNewChat: updateisNewChat,
        updateCurrStatus: updateCurrStatus,
        updateWindowFocus: updateWindowFocus,
        flashTitle: flashTitle,
        playSound: playSound,
        notificationCleared: notificationCleared,
        currTitle: currTitle,
        updateChatUsers_NoLoop: updateChatUsers_NoLoop
    }
}();

$(document).ready(function () {
    Array.prototype.indexOf;
    if (!Array.prototype.indexOf) {
        Array.prototype.indexOf = function (obj, start) {
            for (var i = (start || 0), j = this.length; i < j; i++) {
                if (this[i] === obj) {
                    return i;
                }
            }
            return -1;
        };
    }
    $('body').append('<span id="playSound"></span>');

    chatClient.init();
    chatClient.getCurrStatus();
    chatClient.updateChatUsers();
});