var messageBoardOverlay = function () {
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
                                var mbMessage = $.trim(data.d[0][i][1]);
                                if ($("#" + mbId).length == 0) {
                                    str += "<div id='" + mbId + "' class='message-board-workspace-entry'>" + mbMessage + "</div>";
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

                        $("#message_board_pnl_entries").find(".img-quote").remove();
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
                            var mbMessage = $.trim(data.d[i][1]);
                            if ($("#" + mbId).length == 0) {
                                $("#message_board_pnl_entries").append("<div id='" + mbId + "' class='message-board-workspace-entry'>" + mbMessage + "</div>");
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

    return {
        GetMessageBoardOverlayPosts: GetMessageBoardOverlayPosts,
        SetMessageBoardMaxHeight: SetMessageBoardMaxHeight,
        SetMessageBoardOverlayScroll: SetMessageBoardOverlayScroll
    }
}();

$(window).resize(function () {
    messageBoardOverlay.SetMessageBoardMaxHeight();
    messageBoardOverlay.SetMessageBoardOverlayScroll();
});

$(document).ready(function () {
    messageBoardOverlay.GetMessageBoardOverlayPosts();
});

Sys.Application.add_load(function () {
    messageBoardOverlay.GetMessageBoardOverlayPosts();
});
