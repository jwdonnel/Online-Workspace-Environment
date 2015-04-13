var twitterTimeOut = null;
function pageLoad() {
    if (twitterTimeOut == null) {
        twitterStation.Init(5);
    }
}

var twitterStation = function () {
    var collapsedItems = new Array();
    var twitterService = openWSE.siteRoot() + "Apps/Twitter/TwitterStationService.asmx";
    var editId = "";

    function Init(refreshInt) {
        if ($("#twitterstation-load").length > 0) {

            twitterStation.GetFeeds(true);

            if (refreshInt > 0) {
                var minutePlural = "minutes";
                if (refreshInt == 1) {
                    minutePlural = "minute";
                }
                $("#update-int-text").html("Feeds are automatically updated every " + refreshInt + " " + minutePlural);

                var interval = refreshInt * 60000;
                twitterTimeOut = setTimeout(function () {
                    twitterStation.GetFeeds(false);
                }, interval);
            }

            $("#TwitterAdd-element").find("input[type='text']").keypress(function (e) {
                if (e.which == 13 || e.keyCode == 13) {
                    if (editId != "") {
                        twitterStation.UpdateFeed(editId);
                    }
                    else {
                        twitterStation.FinishAdd();
                    }
                }
            });
        }
    }

    function AddFeed() {
        $("#lbl_errorTwitter").html("");
        $("#must-have-twitter-search").hide();
        $("#btn_add").show();
        $("#btn_update").hide();
        $("#hf_editID").val("");
        $("#tb_title").val("");
        $("#tb_caption").val("");
        $("#tb_twitteraccount").val("");
        editId = "";
        openWSE.LoadModalWindow(true, "TwitterAdd-element", "Add Twitter Feed");
        $("#tb_twitteraccount").focus();
    }
    function FinishAdd() {
        if ($.trim($("#tb_twitteraccount").val()) != "") {
            $("#must-have-twitter-search").hide();
            openWSE.LoadingMessage1("Adding Feed...");
            $.ajax({
                url: twitterService + "/AddUserFeed",
                type: "POST",
                data: '{ "title": "' + $("#tb_title").val() + '","caption": "' + $("#tb_caption").val() + '","search": "' + $("#tb_twitteraccount").val() + '","display": "' + $("#dd_display_amount").val() + '","searchType": "' + $("#dd_mode").val() + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    editId = "";
                    openWSE.LoadModalWindow(false, "TwitterAdd-element", "");
                    openWSE.RemoveUpdateModal();
                    GetFeeds(true);
                },
                error: function (err) {
                    openWSE.AlertWindow(err, window.location.href);
                    openWSE.RemoveUpdateModal();
                }
            });
        }
        else {
            $("#must-have-twitter-search").show();
        }
    }

    function EditFeed(id) {
        if (id != "") {
            editId = id;
            openWSE.LoadingMessage1("Loading...");
            $.ajax({
                url: twitterService + "/EditUserFeed",
                type: "POST",
                data: '{ "id": "' + id + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $("#must-have-twitter-search").hide();
                    $("#btn_add").hide();
                    $("#btn_update").show();
                    $("#lbl_errorTwitter").html("");

                    $("#tb_title").val(data.d[0]);
                    $("#tb_caption").val(data.d[1]);
                    $("#tb_twitteraccount").val(data.d[2]);
                    $("#dd_mode").val(data.d[3]);
                    $("#dd_display_amount").val(data.d[4]);

                    openWSE.LoadModalWindow(true, "TwitterAdd-element", "Edit Twitter Feed");
                    openWSE.RemoveUpdateModal();
                    $("#tb_twitteraccount").focus();
                },
                error: function (err) {
                    openWSE.AlertWindow(err, window.location.href);
                    openWSE.RemoveUpdateModal();
                }
            });
        }
    }
    function UpdateFeed() {
        if ($.trim($("#tb_twitteraccount").val()) != "") {
            $("#must-have-twitter-search").hide();
            if (editId != "") {
                openWSE.LoadingMessage1("Updating...");
                $.ajax({
                    url: twitterService + "/UpdateUserFeed",
                    type: "POST",
                    data: '{ "id": "' + editId + '","title": "' + $("#tb_title").val() + '","caption": "' + $("#tb_caption").val() + '","search": "' + $("#tb_twitteraccount").val() + '","display": "' + $("#dd_display_amount").val() + '","searchType": "' + $("#dd_mode").val() + '" }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        editId = "";
                        openWSE.LoadModalWindow(false, "TwitterAdd-element", "");
                        openWSE.RemoveUpdateModal();
                        GetFeeds(true);
                    },
                    error: function (err) {
                        openWSE.AlertWindow(err, window.location.href);
                        openWSE.RemoveUpdateModal();
                    }
                });
            }
        }
        else {
            $("#must-have-twitter-search").show();
        }
    }

    function DeleteFeed(id) {
        if (id != "") {
            openWSE.ConfirmWindow("Are you sure you want to delete this feed?",
                function () {
                    openWSE.LoadingMessage1("Deleting...");
                    $.ajax({
                        url: twitterService + "/DeleteUserFeed",
                        type: "POST",
                        data: '{ "id": "' + id + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            twitterStation.GetFeeds(true);
                            openWSE.RemoveUpdateModal();
                        },
                        error: function (err) {
                            openWSE.AlertWindow(err, window.location.href);
                            openWSE.RemoveUpdateModal();
                        }
                    });
                }, null);
        }
    }

    function CloseModal() {
        editId = "";
        $("#must-have-twitter-search").hide();
        openWSE.LoadModalWindow(false, "TwitterAdd-element", "");
    }

    function GetFeeds(showLoading) {
        if (showLoading) {
            openWSE.LoadingMessage1("Loading Tweets...");
        }

        $.ajax({
            url: twitterService + "/GetUserFeeds",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    var tweetHolder = "";

                    for (var i = 0; i < data.d.length; i++) {
                        var id = data.d[i][0];
                        tweetHolder += "<div id='" + id + "' class='clear'>";
                        tweetHolder += "<div class='twitter-header'><table style='width: 100%;'><tr>";

                        var name = data.d[i][1];
                        var description = data.d[i][2];
                        var feeds = data.d[i][3];

                        if (data.d[i].length == 5) {
                            tweetHolder += "<td style='width: 40px;'><img src='" + data.d[i][3] + "' alt='' /></td>";
                            feeds = data.d[i][4];
                        }

                        tweetHolder += "<td><h3>" + name + "</h3><div class='clear-space-two'></div>";
                        tweetHolder += "<h4>" + description + "</h4></td>";
                        tweetHolder += "<td style='width: 90px;'><div class='float-right'><a href='#collapse-expand' class='collapse-expand-btn td-subtract-btn margin-right' onclick=\"twitterStation.CollapseExpand(this);return false;\" title='Collapse/Expand'></a>";
                        tweetHolder += "<a href='#edit' class='td-edit-btn margin-right' onclick=\"twitterStation.EditFeed('" + id + "');return false;\" title='Edit'></a>";
                        tweetHolder += "<a href='#delete' class='td-cancel-btn' onclick=\"twitterStation.DeleteFeed('" + id + "');return false;\" title='Delete'></a>";
                        tweetHolder += "</div></td></tr></table></div>";
                        tweetHolder += "<div class='twitter-feed-list'><ul>";

                        if (feeds.length > 0) {
                            for (var j = 0; j < feeds.length; j++) {
                                if (feeds[j].length == 2) {
                                    tweetHolder += "<li>" + feeds[j][0] + "<div class='twitter-date'>" + feeds[j][1] + "</div></li>";
                                }
                                else {
                                    tweetHolder += "<li><table><tr><td valign='top'><img alt='' src='" + feeds[j][3] + "' /></td><td><b>" + feeds[j][2] + "</b><div class='clear-space-two'></div>" + feeds[j][0] + "<div class='twitter-date'>" + feeds[j][1] + "</div></td></tr></table></li>";
                                }
                            }
                        }
                        else {
                            tweetHolder += "<h3 class='pad-all'>No Tweets found for this feed.</h3>";
                        }

                        tweetHolder += "</ul></div>";
                        tweetHolder += "</div>";
                    }

                    $("#twitterstation-load").find("#twitterstation-posts").html(tweetHolder);
                    
                    for (var i = 0; i < collapsedItems.length; i++) {
                        $("#" + collapsedItems[i]).find(".twitter-feed-list").hide();
                        $("#" + collapsedItems[i]).find(".twitter-feed-list").addClass("collapsed");
                        $("#" + collapsedItems[i]).find(".collapse-expand-btn").removeClass("td-subtract-btn").addClass("td-add-btn");
                    }
                }
                else {
                    $("#twitterstation-load").find("#twitterstation-posts").html("<h3 class='pad-all'>No Twitter feeds found.</h3>");
                }

                openWSE.RemoveUpdateModal();
            },
            error: function (err) {
                openWSE.AlertWindow(err, window.location.href);
                $("#twitterstation-load").find("#twitterstation-posts").html("<h3 class='pad-all'>There was an error getting your feeds.</h3>");
                openWSE.RemoveUpdateModal();
            }
        });
    }

    function CollapseExpand(_this) {
        var $holder = $(_this).closest(".twitter-header").parent().find(".twitter-feed-list");
        if ($holder.length > 0) {
            if ($(_this).hasClass("td-subtract-btn")) {
                $(_this).removeClass("td-subtract-btn").addClass("td-add-btn");
                $holder.addClass("collapsed");
                $holder.slideUp(openWSE_Config.animationSpeed);
            }
            else {
                $(_this).removeClass("td-add-btn").addClass("td-subtract-btn");
                $holder.removeClass("collapsed");
                $holder.slideDown(openWSE_Config.animationSpeed);
            }
        }

        collapsedItems = new Array();
        $(".twitter-feed-list").each(function () {
            if ($(this).hasClass("collapsed")) {
                collapsedItems.push($(this).parent().attr("id"));
            }
        });
    }

    return {
        Init: Init,
        AddFeed: AddFeed,
        FinishAdd:FinishAdd,
        EditFeed: EditFeed,
        UpdateFeed: UpdateFeed,
        DeleteFeed: DeleteFeed,
        CloseModal: CloseModal,
        GetFeeds: GetFeeds,
        CollapseExpand: CollapseExpand
    }
}();