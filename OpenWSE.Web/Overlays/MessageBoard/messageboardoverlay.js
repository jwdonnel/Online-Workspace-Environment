var messageBoardOverlay = function () {
    var runningMoreMBOverlay = false;
    var noPostsAvailable = "<h3 class='no-messages-posted pad-all'>There are no message board posts available.</h3>";

    function GetMessageBoardOverlayPosts() {
        if ($("#messageboard_overlay_pnl").length > 0) {
            if (!runningMoreMBOverlay) {
                runningMoreMBOverlay = true;

                openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/LoadMessageBoardOverlay", '{ }', null, function (data) {
                    $(".mb-no-message").remove();
                    $("#loadmessageboardposts").remove();

                    if ($("#message_board_pnl_entries").find(".no-messages-posted").length > 0) {
                        $("#message_board_pnl_entries").find(".no-messages-posted").remove();
                    }

                    if (data.d.length > 0) {
                        FinishLoadingPosts(data.d);
                    }
                    else {
                        $("#message_board_pnl_entries").html(noPostsAvailable);
                    }

                    $("#messageboard_overlay_pnl").find(".message-button-holder").remove();

                    runningMoreMBOverlay = false;
                    $("#message_board_pnl_entries").find(".img-quote").remove();
                }, function () {
                    $("#message_board_pnl_entries").html("<h3 class='pad-bottom-big' style='color: Red;'>Error retrieving message board!</h3>");
                    runningMoreMBOverlay = false;
                });
            }
        }
    }
    function FinishLoadingPosts(data, appendToEnd) {
        postStr = "";
        if (data.length > 0) {
            buildPosts(data, 0);
            $("#message_board_pnl_entries").html(postStr);
        }
        else {
            $("#MessageList_messageboard").html(noPostsAvailable);
        }
    }
    function buildPosts(data, depth) {
        for (var i = 0; i < data.length; i++) {
            postStr += buildPostDiv(data[i].post, depth);
            if (data[i].responses.length > 0) {
                buildPosts(data[i].responses, depth + 1);
            }

            postStr += "</div></div>";
        }
    }
    function buildPostDiv(data, depth) {
        var str = "<div class='message-entry' data-id='" + data.ID + "' data-depth='" + depth + "' data-groupid='" + data.GroupName + "'>";
        str += data.Post;
        str += "<div class='message-responses'>";

        return str;
    }
    return {
        GetMessageBoardOverlayPosts: GetMessageBoardOverlayPosts
    }
}();

$(document).ready(function () {
    messageBoardOverlay.GetMessageBoardOverlayPosts();
});

Sys.Application.add_load(function () {
    messageBoardOverlay.GetMessageBoardOverlayPosts();
});
