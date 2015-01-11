var currGroup = "";
var showingNewMessageControls = false;
var ddMBOverlayLoading = "<div class='OverlayMBLoading'>" + openWSE.loadingImg + "<h3 class='float-left pad-left'>Loading Posts. Please Wait...</h3></div>";

function ClearPost() {
    tinyMCE.get('Editor_messageboard').setContent("");
    document.getElementById("lbl_errormessage_messageboard").innerText = "";
}

$(document.body).on("click", "#app-messageboard .exit-button-app, #app-messageboard-min-bar .exit-button-app-min", function () {
    cookie.del("messageboard-Group");
    cookie.del("messageboard-Controls");
});

$(document.body).on("click", ".app-popup-inner-app-selector li", function () {
    try {
        if (tinyMCE.get('Editor_messageboard').getContent() != null) {
            tempContentHolder = tinyMCE.get('Editor_messageboard').getContent();
        }
    }
    catch (evt) { }
    setTimeout(function () {
        StartMessageBoard();
        setTimeout(function () {
            try {
                if (tempContentHolder != "") {
                    tinyMCE.get('Editor_messageboard').setContent(tempContentHolder);
                    tempContentHolder = "";
                }
            }
            catch (evt2) { }
        }, openWSE_Config.animationSpeed * 10);
    }, openWSE_Config.animationSpeed * 5);
});

function GroupChanged_mb() {
    currGroup = $("#dd_groups_messageboard").val();
    cookie.set("messageboard-Group", currGroup, "30");
}

$(document.body).on("click", "#app-messageboard-dblclick", function () {
    SetMessageBoardScroll();
    CheckIfCanAddMoreMBPosts();
});

$(document.body).on("click", "#app-messageboard .maximize-button-app", function () {
    SetMessageBoardScroll();
    CheckIfCanAddMoreMBPosts();
});

$(function () {
    $("#app-messageboard").resizable({
        stop: function (event, ui) {
            SetMessageBoardScroll();
            CheckIfCanAddMoreMBPosts();
        }
    });
});

function HideMessageBoard() {
    if (showingNewMessageControls) {
        $("#Messageboard_Holder").slideUp(openWSE_Config.animationSpeed);
        $("#mb_newmessage_groups").hide();
        $("#showMessageboard").html("Show Message Controls");
        $("#MessageList_messageboard-overlay").remove();
        showingNewMessageControls = false;
        cookie.set("messageboard-Controls", "0", "30");
    }
    else {
        $("#Messageboard_Holder").slideDown(openWSE_Config.animationSpeed);
        $("#showMessageboard").html("Hide New Message Controls");
        $("#mb_newmessage_groups").show();
        if ($("#MessageList_messageboard-overlay").length == 0) {
            $("#messageboard-load").append("<div id='MessageList_messageboard-overlay'></div>");
        }

        showingNewMessageControls = true;
        cookie.set("messageboard-Controls", "1", "30");
    }
}

function PostMessage() {
    var button = document.getElementById("imgbtn_update");
    var error = document.getElementById("lbl_errormessage_messageboard");
    if (tinyMCE.get('Editor_messageboard').getContent() == "") {
        error.style.color = "Red";
        error.innerText = "Cannot post empty message";
        error.style.display = "block";
        button.disabled = false;
    }
    else {
        error.innerText = "";
        button.disabled = true;
        LoadingMessageBoard("Posting...");
        if ($("#MessageList_messageboard").length != 0) {
            $.ajax({
                url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/AddMessage",
                type: "POST",
                data: '{ "message": "' + escape(tinyMCE.get('Editor_messageboard').getContent()) + '","group": "' + currGroup + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    if (response != "") {
                        button.disabled = false;
                        error.style.display = "none";
                        tinyMCE.get('Editor_messageboard').setContent("");

                        var mbId = data.d[0];
                        var mbClass = data.d[1];
                        var mbMessage = $.trim(data.d[2]);
                        if ($("#" + mbId).length == 0) {
                            $("#MessageList_messageboard").prepend("<div id='" + mbId + "' class='" + mbClass + "'>" + mbMessage + "</div>");
                        }

                        if ($("#MessageList_messageboard").find(".no-messages-posted").length > 0) {
                            $("#MessageList_messageboard").find(".no-messages-posted").remove();
                        }

                        $("#Messageboard_Holder").fadeOut(openWSE_Config.animationSpeed);
                        $("#showMessageboard").html("Show Message Controls");
                        $("#MessageList_messageboard-overlay").remove();
                        showingNewMessageControls = false;
                        cookie.set("messageboard-Controls", "0", "30");
                    }
                    openWSE.RemoveUpdateModal();
                },
                error: function (data) {
                    button.disabled = false;
                    error.style.color = "#E20000";
                    error.innerText = "Error Posting Message";
                    error.style.display = "block";
                    openWSE.RemoveUpdateModal();
                }
            });
        }
    }
}

function PostMessageDeleted(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this message?",
        function () {
            var error = document.getElementById("lbl_errormessage_messageboard");
            LoadingMessageBoard("Deleting...");
            if ($("#MessageList_messageboard").length != 0) {
                $.ajax({
                    url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/DeleteMessage",
                    type: "POST",
                    data: '{ "id": "' + escape(id) + '","group": "' + currGroup + '" }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var response = data.d;
                        if (openWSE.ConvertBitToBoolean(response)) {
                            error.style.display = "none";
                            $("#" + id).fadeOut(openWSE_Config.animationSpeed, function () {
                                $("#" + id).remove();
                            });
                            $("#" + id + "-overlay").fadeOut(openWSE_Config.animationSpeed, function () {
                                $("#" + id + "-overlay").remove();
                            });
                        }
                        else {
                            error.style.color = "Red";
                            error.innerText = "Error Deleting Message";
                            error.style.display = "block";
                        }

                        openWSE.RemoveUpdateModal();
                    }
                });
            }
        }, null);
}

function PostMessageQuote(id) {
    if (!showingNewMessageControls) {
        $("#Messageboard_Holder").fadeIn(openWSE_Config.animationSpeed);
        $("#mb_newmessage_groups").show();
        $("#showMessageboard").html("Hide Controls");
        if ($("#MessageList_messageboard-overlay").length == 0) {
            $("#messageboard-load").append("<div id='MessageList_messageboard-overlay'></div>");
        }
        showingNewMessageControls = true;
        cookie.set("messageboard-Controls", "1", "30");
    }

    $("#messageboard-load").scrollTop(0);

    var message = $("#" + id).find(".messageText").html();
    var userInfo = $("#" + id).find(".userInfo").html();
    var userInfoDate = $("#" + id).find(".userInfoDate").html();
    var holder = "<div style='border: 1px solid #B7B7B7; color: #353535!important; float: left; clear: both; padding: 15px 20px; margin: 10px 15px; background: #F5F5F5; background: rgba(230, 230, 230, 0.7); border-radius: 5px; -moz-border-radius: 5px; -webkit-border-radius: 5px;'>";
    holder += "<span style='float: left; padding-right: 5px; font-size: 13px; font-weight: bold;'>Quoting " + userInfo + "</span><span style='float: left;'>&bull;  " + userInfoDate + "</span>";
    holder += "<div style='clear: both;'></div><div style='padding-left: 10px;'>" + message + "</div></div><div style='clear: both;'></div><br />";
    tinyMCE.get('Editor_messageboard').setContent(holder);
}

var tempContentHolder = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function (sender, args) {
    try {
        if (tinyMCE.get('Editor_messageboard').getContent() != null) {
            tempContentHolder = tinyMCE.get('Editor_messageboard').getContent();
        }
    }
    catch (evt) { }
});

prm.add_endRequest(function (sender, args) {
    if (tempContentHolder != "") {
        tinyMCE.get('Editor_messageboard').setContent(tempContentHolder);
        tempContentHolder = "";
    }

    LoadMessageBoardPosts();
    GetMessageBoardOverlayPosts();
    StartMessageBoard();
});

$(window).resize(function () {
    SetMessageBoardMaxHeight();
});

function StartMessageBoard() {
    var overlayNeeded = false;
    var id1 = cookie.get('messageboard-Controls');
    if (id1 == null) {
        cookie.set("messageboard-Controls", "1", "30");
    }
    else {
        if (id1 == "0") {
            $("#Messageboard_Holder").hide();
            $("#mb_newmessage_groups").hide();
            $("#showMessageboard").html("Show Message Controls");
            showingNewMessageControls = false;
        }
        else {
            $("#Messageboard_Holder").show();
            $("#mb_newmessage_groups").show();
            $("#showMessageboard").html("Hide Controls");
            showingNewMessageControls = true;
        }
    }
    InitPage();
}

$(document).ready(function () {
    try {
        TryLoadMBPage();
    }
    catch (evt) { }
    $("#Editor_messageboard_ifr").tooltip({ disabled: true });
});

function TryLoadMBPage() {
    try {
        LoadTinyMCEControls_Simple();
    }
    catch (evt) {
        window.tinymce.dom.Event.domLoaded = true;
        var ed = new tinymce.Editor("Editor_messageboard", {
            selector: "#Editor_messageboard",
            theme: "modern",
            height: 150,
            plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste", "autoresize"],
            autoresize_min_height: 150,
            toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
        }, tinymce.EditorManager);
        ed.render();
    }

    try {
        StartMessageBoard();
        LoadMessageBoardPosts();
    }
    catch (evt2) { }
}

function InitPage() {
    if ($("#MessageList_messageboard").length != 0) {

        var id2 = cookie.get('messageboard-Group');
        if (id2 != null) {
            $("#dd_groups_messageboard").val(id2)
        }

        currGroup = $("#dd_groups_messageboard").val();
    }
}

var runningMoreMB = false;
function LoadMessageBoardPosts() {
    if ($("#MessageList_messageboard").length != 0) {
        if (!runningMoreMB) {
            runningMoreMB = true;

            var myIds = new Array();
            $(".PostsComments").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            $.ajax({
                url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/GetPosts",
                type: "POST",
                data: '{ "_currIds": "' + myIds + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".mb-no-message").remove();
                    $(".OverlayMBLoading").remove();

                    if ($("#MessageList_messageboard").find(".no-messages-posted").length > 0) {
                        $("#MessageList_messageboard").find(".no-messages-posted").remove();
                    }

                    if (data.d[0].length == 0) {
                        $("#MessageList_messageboard").html("<h3 class='no-messages-posted pad-all'>There are no message board posts available.</h3>");
                    }
                    else {
                        var str = "";
                        for (var i = 0; i < data.d[0].length; i++) {
                            var mbId = data.d[0][i][0];
                            var mbClass = data.d[0][i][1];
                            var mbMessage = $.trim(data.d[0][i][2]);
                            if ($("#" + mbId).length == 0) {
                                str += "<div id='" + mbId + "' class='" + mbClass + "'>" + mbMessage + "</div>";
                            }
                        }

                        if (str != "") {
                            $("#MessageList_messageboard").html(str);
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

                    if ($("#Messageboard_Holder").css("display") == "block") {
                        if ($("#MessageList_messageboard-overlay").length == 0) {
                            $("#messageboard-load").append("<div id='MessageList_messageboard-overlay'></div>");
                        }
                    }
                    else {
                        $("#MessageList_messageboard-overlay").remove();
                    }

                    runningMoreMB = false;
                    SetMessageBoardScroll();
                    CheckIfCanAddMoreMBPosts();
                },
                error: function (data) {
                    $(".OverlayMBLoading").remove();
                    runningMoreMB = false;
                }
            });
        }
    }
}

function CheckIfCanAddMoreMBPosts() {
    try {
        var elem = document.getElementById("MessageList_messageboard");
        var innerHeight = $("#MessageList_messageboard").innerHeight();
        if ((innerHeight > elem.scrollHeight) && (elem.scrollHeight > 0) && (innerHeight > 0)) {
            $(".OverlayMBLoading").remove();
            $("#MessageList_messageboard").append(ddMBOverlayLoading);
            LoadMoreMessageBoardPosts();
        }
    }
    catch (evt) { }
}

function LoadMoreMessageBoardPosts() {
    if ($("#MessageList_messageboard").length > 0) {
        if (!runningMoreMB) {
            runningMoreMB = true;

            var myIds = new Array();
            $(".PostsComments").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            $.ajax({
                url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/GetMorePosts",
                type: "POST",
                data: '{ "_currIds": "' + myIds + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".OverlayMBLoading").remove();

                    for (var i = 0; i < data.d.length; i++) {
                        var mbId = data.d[i][0];
                        var mbClass = data.d[i][1];
                        var mbMessage = $.trim(data.d[i][2]);
                        if ($("#" + mbId).length == 0) {
                            $("#MessageList_messageboard").append("<div id='" + mbId + "' class='" + mbClass + "'>" + mbMessage + "</div>");
                        }
                    }

                    runningMoreMB = false;
                    SetMessageBoardScroll();
                    CheckIfCanAddMoreMBPosts();
                },
                error: function () {
                    $(".OverlayMBLoading").remove();
                    SetMessageBoardScroll();
                    runningMoreMB = false;
                }
            });
        }
    }
}

function SetMessageBoardScroll() {
    $("#MessageList_messageboard").scroll(function () {
        try {
            var elem = document.getElementById("MessageList_messageboard");
            var $_scrollBar = $("#MessageList_messageboard");
            var temp = $_scrollBar.scrollTop();
            var innerHeight = $_scrollBar.innerHeight();
            if (temp > 0) {
                if (temp + innerHeight >= elem.scrollHeight) {
                    $(".OverlayMBLoading").remove();
                    $("#MessageList_messageboard").append(ddMBOverlayLoading);
                    LoadMoreMessageBoardPosts();
                }
            }
        }
        catch (evt) { }
    });
}

$(document.body).on("click", ".RandomActionBtns-mb", function () {
    LoadingMessageBoard("Please Wait...");
});

function LoadTinyMCEControls_Simple() {
    window.tinymce.dom.Event.domLoaded = true;
    var ed = new tinymce.Editor("Editor_messageboard", {
        selector: "#Editor_messageboard",
        theme: "modern",
        height: 150,
        plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste", "autoresize"],
        autoresize_min_height: 150,
        toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
    }, tinymce.EditorManager);
    ed.render();
}

function LoadingMessageBoard(message) {
    $.LoadingMessage("#messageboard-load", message);
}

function LoadMBFeeds() {
    openWSE.LoadingMessage1("Loading Feeds. Please Wait...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/FindMessageBoardGroups",
        type: "POST",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $(".OverlayMBLoading").remove();
            $("#AddMBRSSFeedHolder").html(data.d);
            if ($("#MBRSS-Feed-Selector-element").css("display") != "block") {
                openWSE.LoadModalWindow(true, 'MBRSS-Feed-Selector-element', 'Message Board to RSS');
            }
            openWSE.RemoveUpdateModal();
        },
        error: function (data) {
            $(".OverlayMBLoading").remove();
            $("#AddMBRSSFeedHolder").html("<h3 style='color: Red;'>Error loading groups! Please try again.</h3>");
        }
    });
}

function AddMBFeed(groupName) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/AddRSSMB",
        type: "POST",
        data: '{ "_groupName": "' + groupName + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            LoadMBFeeds();
        }
    });
}

function RemoveMBFeed(groupName) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/RemoveRSSMB",
        type: "POST",
        data: '{ "_groupName": "' + groupName + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            LoadMBFeeds();
        }
    });
}





/* Message Board Overlay
----------------------------------*/
var sbMessageBoardLoaded = false;
var shareElementOpen = false;
var runningMoreMBOverlay = false;
function GetMessageBoardOverlayPosts() {
    if ($("#messageboard_overlay_pnl").length > 0) {
        if (!runningMoreMBOverlay) {
            runningMoreMBOverlay = true;

            var myIds = new Array();
            $(".message-board-workspace-entry").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            $.ajax({
                url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/LoadMessageBoardOverlay",
                type: "POST",
                data: '{ "_currIds": "' + myIds + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".mb-no-message").remove();
                    $("#loadmessageboardposts").remove();

                    if ($("#message_board_pnl_entries").find(".no-messages-posted").length > 0) {
                        $("#message_board_pnl_entries").find(".no-messages-posted").remove();
                    }

                    if (data.d[0].length == 0) {
                        $("#message_board_pnl_entries").html("<h3 class='no-messages-posted pad-all'>There are no message board posts available.</h3>");
                    }
                    else {
                        var str = "";
                        for (var i = 0; i < data.d[0].length; i++) {
                            var mbId = data.d[0][i][0];
                            var mbClass = data.d[0][i][1];
                            var mbMessage = $.trim(data.d[0][i][2]);
                            if ($("#" + mbId).length == 0) {
                                str += "<div id='" + mbId + "' class='message-board-workspace-entry" + mbClass + "'>" + mbMessage + "</div>";
                            }
                        }

                        if (str != "") {
                            $("#message_board_pnl_entries").html(str);
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

                    runningMoreMBOverlay = false;
                    SetMessageBoardMaxHeight();
                    SetMessageBoardOverlayScroll();
                    CheckIfCanAddMoreMBOverlay();
                },
                error: function () {
                    SetMessageBoardOverlayScroll();
                    $("#message_board_pnl_entries").html("<h3 class='pad-bottom-big' style='color: Red;'>Error retrieving message board!</h3>");
                    runningMoreMBOverlay = false;
                }
            });
        }
    }
}

function CheckIfCanAddMoreMBOverlay() {
    var elem = document.getElementById("message_board_pnl_entries");
    if (elem != null) {
        try {
            var innerHeight = $("#message_board_pnl_entries").innerHeight();
            var maxHeight = parseInt($("#messageboard_overlay_pnl").css("max-height"));
            if ((innerHeight > elem.scrollHeight) && (elem.scrollHeight > 0) && (innerHeight > 0)) {
                GetMoreMessageBoardOverlayPosts();
            }
        }
        catch (evt) { }
    }
}

function GetMoreMessageBoardOverlayPosts() {
    if ($("#messageboard_overlay_pnl").css("display") == "block") {
        if (!runningMoreMBOverlay) {
            runningMoreMBOverlay = true;

            var myIds = new Array();
            $(".message-board-workspace-entry").each(function (index) {
                myIds[index] = $(this).attr("id");
            });

            $.ajax({
                url: openWSE.siteRoot() + "Apps/MessageBoard/MessageBoard.asmx/LoadMoreMessageBoardOverlay",
                type: "POST",
                data: '{ "_currIds": "' + myIds + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    for (var i = 0; i < data.d.length; i++) {
                        var mbId = data.d[i][0];
                        var mbClass = data.d[i][1];
                        var mbMessage = $.trim(data.d[i][2]);
                        if ($("#" + mbId).length == 0) {
                            $("#message_board_pnl_entries").append("<div id='" + mbId + "' class='message-board-workspace-entry" + mbClass + "'>" + mbMessage + "</div>");
                        }
                    }

                    runningMoreMBOverlay = false;
                    SetMessageBoardMaxHeight();
                    SetMessageBoardOverlayScroll();
                    CheckIfCanAddMoreMBOverlay();
                },
                error: function () {
                    SetMessageBoardOverlayScroll();
                    $("#message_board_pnl_entries").append("<h3 class='pad-bottom-big' style='color: Red;'>Error retrieving message board!</h3>");
                    runningMoreMBOverlay = false;
                }
            });
        }
    }
}

function SetMessageBoardMaxHeight() {
    if ($("#messageboard_overlay_pnl").css("display") != "none") {
        var bufferBottom = 90;
        if ($("#MainContent_pnl_adminnote").length > 0) {
            bufferBottom = $("#MainContent_pnl_adminnote").height() + 75;
        }

        var extendedHeight = $(window).height() - (bufferBottom + $("#top-main-bar-top").height());
        $("#messageboard_overlay_pnl").css("max-height", extendedHeight);

        var headerHeight = $("#messageboard_overlay_pnl").find(".overlay-header").height();
        var padTop = parseFloat($("#messageboard_overlay_pnl").find(".overlay-header").css("padding-top"));
        var padBottom = parseFloat($("#messageboard_overlay_pnl").find(".overlay-header").css("padding-bottom"));
        var diff = (headerHeight * 2) + padTop + padBottom + 1;
        $("#message_board_pnl_entries").css("max-height", extendedHeight - diff);
        CheckIfCanAddMoreMBOverlay();

        var sidebarWidth = $("#side-bar-controls").width();
        var windowWidth = $(window).width();
        $(".message-board-workspace").css("max-width", windowWidth - (sidebarWidth + 85));
    }
}

function SetMessageBoardOverlayScroll() {
    $("#message_board_pnl_entries").scroll(function () {
        var elem = document.getElementById("message_board_pnl_entries");
        if (elem != null) {
            try {
                var $_scrollBar = $("#message_board_pnl_entries");
                var temp = $_scrollBar.scrollTop();
                var innerHeight = $_scrollBar.innerHeight();
                if (temp > 0) {
                    if (temp + innerHeight >= elem.scrollHeight) {
                        GetMoreMessageBoardOverlayPosts();
                    }
                }
            }
            catch (evt) { }
        }
    });
}