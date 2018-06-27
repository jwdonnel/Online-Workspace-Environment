var messageBoardApp = function () {
    var selectedDiscussion = "";
    var searchVal = "";
    var loadingDiv = "<h3 class=\"pad-all\">Loading...</h3>";

    function Initialize() {
        $("#MessageList_messageboard").scroll(function () {
            try {
                var elem = document.getElementById("MessageList_messageboard");
                var $_scrollBar = $("#MessageList_messageboard");
                var temp = $_scrollBar.scrollTop();
                var innerHeight = $_scrollBar.innerHeight();
                if (temp > 0) {
                    if (temp + innerHeight >= elem.scrollHeight) {
                        LoadPosts(true);
                    }
                }
            }
            catch (evt) { }
        });

        if ($("#dd_currentGroup_messageboard").css("display") === "none") {
            $("#dd_currentGroup_messageboard").parent().hide();
        }
    }
    function ResizeMessageBoard() {
        var topPos = ($(".message-board-toolbar").outerHeight() + 10);
        if ($("#message-board-currentdiscussion").hasClass("discussion-show")) {
            topPos += $("#message-board-currentdiscussion").outerHeight();
        }
        
        $("#MessageList_messageboard").css("top", topPos + "px");
    }

    var postStr = "";
    var runningLoad = false;
    function LoadPosts(appendToEnd) {
        if (!selectedDiscussion && $(".discussion-entry").length === 0) {
            if (!runningLoad) {
                $("#message-board-backbtn").hide();

                runningLoad = true;
                loadingPopup.Message("Loading...", "#messageboard-load");
                ResizeMessageBoard();

                openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/LoadDiscussions", '{ "selectedGroup": "' + $("#dd_currentGroup_messageboard").val() + '","searchVal": "' + escape(searchVal) + '" }', null, function (data) {
                    runningLoad = false;
                    $("#message-board-createbtn").html("<span class=\"discussion-add-button\"></span>Create Discussion");
                    FinishDiscussionList(data.d);
                    loadingPopup.RemoveMessage("#messageboard-load");
                }, function (data) {
                    runningLoad = false;
                    loadingPopup.RemoveMessage("#messageboard-load");
                });
            }
        }
        else {
            if (!runningLoad) {
                $("#message-board-backbtn").show();

                runningLoad = true;
                loadingPopup.Message("Loading...", "#messageboard-load");
                ResizeMessageBoard();

                var myIds = new Array();
                $("#MessageList_messageboard").find(".message-entry").each(function () {
                    myIds.push($(this).attr("data-id"));
                });

                if (selectedDiscussion) {
                    openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/GetPosts", '{ "_currIds": "' + myIds + '","selectedGroup": "' + $("#dd_currentGroup_messageboard").val() + '","selectedDiscussion": "' + escape(selectedDiscussion) + '" }', null, function (data) {
                        runningLoad = false;
                        $("#message-board-createbtn").html("<span class=\"discussion-add-button\"></span>Create Message");
                        FinishLoadingPosts(data.d, appendToEnd);
                        loadingPopup.RemoveMessage("#messageboard-load");
                        CheckIfCanAddMorePosts();
                    }, function (data) {
                        runningLoad = false;
                        loadingPopup.RemoveMessage("#messageboard-load");
                    });
                }
                else {
                    loadingPopup.RemoveMessage("#messageboard-load");
                }
            }
        }
    }
    function CheckIfCanAddMorePosts() {
        try {
            var elemScrollHeight = document.getElementById("MessageList_messageboard").scrollHeight;
            var entryHeight = 0;
            $(".message-entry[data-depth='0']").each(function () {
                entryHeight += $(this).outerHeight();
            });

            var buffer = elemScrollHeight - document.getElementById("MessageList_messageboard").clientHeight;
            if (buffer > 0 && entryHeight + buffer < elemScrollHeight - buffer && elemScrollHeight > 0 && entryHeight > 0) {
                LoadPosts(true);
            }
        }
        catch (evt) { }
    }

    function FinishDiscussionList(data) {
        var str = "";
        $("#message-board-currentdiscussion").html("");
        $("#message-board-currentdiscussion").removeClass("discussion-show");

        var altRowClass = "";
        if (openWSE.ConvertBitToBoolean(data[1])) {
            altRowClass = "GridAlternate";
        }

        var str = "<table data-tableid=\"messageboard-app-aspxGridview\" data-columnspan=\"5\" data-cookiename=\"messageboard-app-aspxGridviewPageSize\" data-allowpaging=\"true\" data-initalsortcolumn=\"Date\" data-initalsortdir=\"DESC\" data-altrowclass=\"" + altRowClass + "\" class=\"gridview-table\">";
        str += "<tbody><tr class=\"myHeaderStyle\">";
        if (openWSE.ConvertBitToBoolean(data[1])) {
            str += "<td width=\"45px\" style=\"padding-left: 10px;\">#</td>";
        }
        str += "<td data-columnname=\"Date\" class=\"td-sort-click active desc\" title=\"Sort by latest activity\" onclick=\"openWSE.GridViewMethods.SortColumn_Click(this);\" style=\"width: 175px;\">Last Activity</td>";
        str += "<td data-columnname=\"Discussion\" class=\"td-sort-click\" title=\"Sort by discussion\" onclick=\"openWSE.GridViewMethods.SortColumn_Click(this);\">Discussion Thread</td>";
        str += "<td data-columnname=\"Username\" class=\"td-sort-click\" title=\"Sort by user\" onclick=\"openWSE.GridViewMethods.SortColumn_Click(this);\" style=\"width: 250px;\">Username</td>";
        str += "<td data-columnname=\"Replies\" class=\"td-sort-click\" title=\"Sort by replies\" onclick=\"openWSE.GridViewMethods.SortColumn_Click(this);\" align=\"center\" style=\"width: 100px;\">Replies</td></tr>";

        for (var i = 0; i < data[0].length; i++) {
            str += "<tr class=\"myItemStyle GridNormalRow cursor-pointer discussion-entry\" data-title='" + data[0][i].Title + "' onclick='messageBoardApp.DiscussionClick(this);'>";
            if (openWSE.ConvertBitToBoolean(data[1])) {
                str += "<td class=\"GridViewNumRow\">" + (i + 1).toString() + "</td>";
            }
            str += "<td data-columnname=\"Date\" align=\"left\"><span class=\"sort-value-class\" data-sortvalue=\"" + data[0][i].DateLong + "\"></span>" + data[0][i].Date + "</td>";
            str += "<td data-columnname=\"Discussion\" align=\"left\">" + data[0][i].Title + "</td>";
            str += "<td data-columnname=\"Username\" align=\"left\">" + data[0][i].Username + "</td>";
            str += "<td data-columnname=\"Replies\" align=\"center\">" + data[0][i].Count + "</td>";
            str += "</tr>";
        }

        if (data[0].length === 0) {
            str += "<tr class=\"myItemStyle GridNormalRow\">";
            str += "<td colspan=\"4\">No discussions found</td>";
            str += "</tr>";
        }

        str += "</tbody></table>";

        var searchStr = "<div class=\"searchwrapper float-right\"><div class=\"searchboxholder\"><input type=\"text\" class=\"searchbox\" placeholder=\"Search discussions...\" value=\"" + searchVal + "\" onkeypress=\"messageBoardApp.SearchDiscussions_KeyPress(this, event)\"></div><a class=\"searchbox_submit\" onclick=\"messageBoardApp.SearchDiscussions(this); return false;\"></a><a onclick=\"messageBoardApp.ClearSearch(this); return false;\" class=\"searchbox_clear\"></a></div>";

        $("#MessageList_messageboard").html("<div class=\"pad-all\">" + searchStr + "<div class=\"clear-space\"></div>" + str + "</div>");

        openWSE.GridViewMethods.InitializeTable("messageboard-app-aspxGridview");
        ResizeMessageBoard();
    }

    function FinishLoadingPosts(data, appendToEnd) {
        postStr = "";
        $("#message-board-currentdiscussion").html(selectedDiscussion);
        $("#message-board-currentdiscussion").addClass("discussion-show");
        if (data.length > 0) {
            buildPosts(data, 0);
            if (!appendToEnd) {
                $("#MessageList_messageboard").html(postStr);
            }
            else {
                $("#MessageList_messageboard").append(postStr);
            }
        }
        else {
            if (!appendToEnd) {
                $("#MessageList_messageboard").html("<h3 class='pad-all'>No posts found</h3>");
            }
        }

        ResizeMessageBoard();
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

    var respondToId = "";
    function PostMessage() {
        if ($.trim(tinyMCE.get('Editor_messageboard').getContent()) == "") {
            openWSE.AlertWindow("Cannot post empty message.");
        }
        else if ($.trim($("#messageBoard_newTitle").val()) == "") {
            openWSE.AlertWindow("Title cannot be blank");
        }
        else {
            loadingPopup.Message("Posting...", "#messageboard-load");
            openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/AddMessage", '{ "title": "' + escape($.trim($("#messageBoard_newTitle").val())) + '","message": "' + escape($.trim(tinyMCE.get('Editor_messageboard').getContent())) + '","group": "' + $("#dd_groups_messageboard").val() + '","responseToID": "' + respondToId + '" }', null, function (data) {
                respondToId = "";
                $("#message-board-backbtn").show();
                selectedDiscussion = $.trim($("#messageBoard_newTitle").val());
                CloseNewMessageBoard();
                FinishLoadingPosts(data.d);
                loadingPopup.RemoveMessage("#messageboard-load");
                CheckIfCanAddMorePosts();
            }, function (data) {
                openWSE.AlertWindow("Error posting message.");
                loadingPopup.RemoveMessage("#messageboard-load");
            });
        }
    }
    function RespondToMessage(id) {
        var userInfo = $.trim($(".message-entry[data-id='" + id + "']").find(".message-fullname-text").html());
        respondToId = id;
        AddNewMessageDialog("Reply to " + userInfo);
    }

    function DeleteMessage(id) {
        openWSE.ConfirmWindow("Are you sure you want to delete this message?",
        function () {
            loadingPopup.Message("Deleting...", "#messageboard-load");
            openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/DeleteMessage", '{ "id": "' + escape(id) + '" }', null, function (data) {
                var response = data.d;
                if (openWSE.ConvertBitToBoolean(response)) {
                    $(".message-entry[data-id='" + id + "']").remove();
                    $(".message-entry-overlay[data-id='" + id + "']").remove();

                    if ($(".message-entry").length === 0) {
                        CloseDiscussion();
                    }
                }
                else {
                    openWSE.AlertWindow("Error deleting message.");
                }

                loadingPopup.RemoveMessage("#messageboard-load");
            });
        }, null);
    }

    function AddNewMessageDialog(title) {
        if (!title) {
            title = "Create New Message";
            if (selectedDiscussion) {
                title = "Create New Discussion";
            }
        }

        openWSE.LoadModalWindow(true, "NewMessageBoard-element", title);

        if (selectedDiscussion) {
            $("#messageBoard_newTitle").val(selectedDiscussion);
            $("#messageBoard_newTitle").prop("disabled", true);
        }
        else {
            $("#messageBoard_newTitle").val("");
            $("#messageBoard_newTitle").prop("disabled", false);
        }

        try {
            window.tinymce.dom.Event.domLoaded = true;
            var ed = new tinymce.Editor("Editor_messageboard", {
                selector: "#Editor_messageboard",
                theme: "modern",
                height: 100,
                plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste", "autoresize"],
                autoresize_min_height: 100,
                toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
            }, tinymce.EditorManager);
            ed.render();
        }
        catch (evt) {
            window.tinymce.dom.Event.domLoaded = true;
            var ed = new tinymce.Editor("Editor_messageboard", {
                selector: "#Editor_messageboard",
                theme: "modern",
                height: 100,
                plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste", "autoresize"],
                autoresize_min_height: 100,
                toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
            }, tinymce.EditorManager);
            ed.render();
        }

        $("#Editor_messageboard_ifr").tooltip({ disabled: true });

        setTimeout(function () {
            var $thisElement = $("#NewMessageBoard-element");
            $thisElement.find(".Modal-element-align").css({
                marginTop: -($thisElement.find(".Modal-element-modal").height() / 2),
                marginLeft: -($thisElement.find(".Modal-element-modal").width() / 2)
            });
        }, 200);
    }
    function CloseNewMessageBoard() {
        openWSE.LoadModalWindow(false, "NewMessageBoard-element", "");
        ClearNewMessage();
        $("#messageBoard_newTitle").val("");
        tinyMCE.get('Editor_messageboard').destroy();
    }
    function ClearNewMessage() {
        tinyMCE.get('Editor_messageboard').setContent("");
    }

    function GroupChange() {
        $("#MessageList_messageboard").html(loadingDiv);
        LoadPosts();
    }

    function DiscussionClick(_this) {
        selectedDiscussion = unescape($(_this).attr("data-title"));
        LoadPosts();
    }
    function CloseDiscussion() {
        selectedDiscussion = "";
        $("#MessageList_messageboard").html(loadingDiv);
        LoadPosts();
    }

    function LoadMBFeeds() {
        loadingPopup.Message("Loading Feeds. Please Wait...");
        openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/FindMessageBoardGroups", '{ }', null, function (data) {
            $(".OverlayMBLoading").remove();
            $("#AddMBRSSFeedHolder").html(data.d);
            if ($("#MBRSS-Feed-Selector-element").css("display") != "block") {
                openWSE.LoadModalWindow(true, 'MBRSS-Feed-Selector-element', 'Message Board to RSS');
            }
            loadingPopup.RemoveMessage();
        }, function (data) {
            $(".OverlayMBLoading").remove();
            $("#AddMBRSSFeedHolder").html("<h3 style='color: Red;'>Error loading groups! Please try again.</h3>");
            loadingPopup.RemoveMessage();
        });
    }
    function AddMBFeed(groupName) {
        openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/AddRSSMB", '{ "_groupName": "' + groupName + '" }', null, function (data) {
            LoadMBFeeds();
        });
    }
    function RemoveMBFeed(groupName) {
        openWSE.AjaxCall("Apps/MessageBoard/MessageBoard.asmx/RemoveRSSMB", '{ "_groupName": "' + groupName + '" }', null, function (data) {
            LoadMBFeeds();
        });
    }

    function SearchDiscussions(_this) {
        searchVal = $.trim($(_this).parent().find(".searchbox").val());
        $("#MessageList_messageboard").html(loadingDiv);
        LoadPosts(false);
    }
    function ClearSearch(_this) {
        $(_this).parent().find(".searchbox").val("");
        searchVal = "";
        $("#MessageList_messageboard").html(loadingDiv);
        LoadPosts(false);
    }
    function SearchDiscussions_KeyPress(_this, event) {
        if (event && (event.which == 13 || event.keyCode == 13)) {
            event.preventDefault();
            SearchDiscussions(_this);
        }
    }

    return {
        Initialize: Initialize,
        ResizeMessageBoard: ResizeMessageBoard,
        LoadPosts: LoadPosts,
        CheckIfCanAddMorePosts: CheckIfCanAddMorePosts,
        PostMessage: PostMessage,
        RespondToMessage: RespondToMessage,
        DeleteMessage: DeleteMessage,
        AddNewMessageDialog: AddNewMessageDialog,
        CloseNewMessageBoard: CloseNewMessageBoard,
        ClearNewMessage: ClearNewMessage,
        GroupChange: GroupChange,
        DiscussionClick: DiscussionClick,
        CloseDiscussion: CloseDiscussion,
        LoadMBFeeds: LoadMBFeeds,
        AddMBFeed: AddMBFeed,
        RemoveMBFeed: RemoveMBFeed,
        SearchDiscussions: SearchDiscussions,
        ClearSearch: ClearSearch,
        SearchDiscussions_KeyPress: SearchDiscussions_KeyPress
    }

}();

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
        try {
            tinyMCE.get('Editor_messageboard').setContent(tempContentHolder);
            tempContentHolder = "";
        }
        catch (evt) { }
    }

    messageBoardApp.ResizeMessageBoard();
    if ($(".message-entry").length === 0 && $(".discussion-entry").length === 0) {
        messageBoardApp.Initialize();
        messageBoardApp.LoadPosts();
    }
});
$(document).ready(function () {
    messageBoardApp.Initialize();
    messageBoardApp.ResizeMessageBoard();
    messageBoardApp.LoadPosts();
});
$(window).resize(function () {
    messageBoardApp.ResizeMessageBoard();
    messageBoardApp.CheckIfCanAddMorePosts();
});
