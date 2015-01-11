var rssTimer;
$(document.body).on("click", "#app-rssfeed .exit-button-app, #app-rssfeed-min-bar .exit-button-app-min", function () {
    cookie.del("rssfeed-current-viewable");
    cookie.del("rssfeed-current");
});

$(document).ready(function () {
    StartRSSFeeder();
});

var currRSSselected = "";
function StartRSSFeeder() {
    if (openWSE_Config.demoMode) {
        $("#btn_AddRemoveFeeds").remove();
    }

    if (!openWSE_Config.demoMode) {
        _savedRssSelViewable = cookie.get("rssfeed-current-viewable");
        if ((_savedRssSelViewable != "") && (_savedRssSelViewable != null) && (_savedRssSelViewable != undefined)) {
            $("#RSSFeedsToPull").val(_savedRssSelViewable);
        }
    }

    LoadingRSSFeed("Building RSS Feed...");
    GetRSSHeaders(true);

    if ((rssTimer == null) || (rssTimer == undefined)) {
        StartRSSTimer();
    }
}

function StartRSSFeederOverlay() {
    $("#rssfeeds_pnl_entries").html("<h4>Loading...</h4>");
    ViewAllNewFeedsOverlay(0);
    if ((rssTimer == null) || (rssTimer == undefined)) {
        StartRSSTimer();
    }
}


function KeyPressSearch_rssfeed(event) {
    try {
        if (event.which == 13) {
            GetRSSHeaders(true);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            GetRSSHeaders(true);
        }
        delete evt;
    }
}

$(window).resize(function () {
    SetRSSOverlayMaxHeight();
});

function StartRSSTimer() {
    rssTimer = setTimeout(function () {
        if (!$("#app-rssfeed").hasClass("selected")) {
            if ($("#rssfeed-load").length > 0) {
                GetRSSHeaders(false);
            }
            if ($("#rssfeeds_pnl_entries").length > 0) {
                ViewAllNewFeedsOverlay(0);
            }
        }
    }, 60000 * 5);
}

function GetRSSHeaders(loading) {
    if ($("#rssfeed-load").length > 0) {
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetUserFeeds",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                RemoveLoadingOverlayRSS();
                if (data.d != null) {
                    if (data.d[0].length == 0) {
                        $("#Saved-RSSFeeds").html("");
                        $("#rssfeed_Holder").html("<div class='pad-all font-color-light-black'>No RSS feeds available. Click Add RSS Feeds to view available feeds.</div>");
                    }
                    else {
                        var x = document.getElementById("Saved-RSSFeeds");

                        // Clear out the dropdown before rebuilding
                        $("#Saved-RSSFeeds").html("");
                        var firstOption = document.createElement("option");
                        firstOption.text = "- View all most recent";
                        firstOption.value = "-MostRecent-";
                        try {
                            x.add(firstOption, x.options[null]);
                        }
                        catch (e) {
                            x.add(firstOption, null);
                        }

                        for (var i = 0; i < data.d[0].length; i++) {
                            var option = document.createElement("option");
                            option.text = data.d[0][i];
                            option.value = data.d[1][i];
                            try {
                                x.add(option, x.options[null]);
                            }
                            catch (e) {
                                x.add(option, null);
                            }

                            if (!openWSE_Config.demoMode) {
                                currRSSselected = cookie.get("rssfeed-current");
                                if (((i == 0) && (loading)) || (currRSSselected == "")) {
                                    if ((currRSSselected == "") || (currRSSselected == null) || (currRSSselected == undefined)) {
                                        currRSSselected = data.d[1][i];
                                    }

                                    cookie.set("rssfeed-current", currRSSselected, "30");
                                }
                            }
                            else {
                                currRSSselected = data.d[1][0];
                            }
                        }

                        if (currRSSselected != "") {
                            $("#Saved-RSSFeeds").val(currRSSselected);
                            if (loading) {
                                LoadingRSSFeed("Building RSS Feed...");
                            }

                            if (currRSSselected == "-MostRecent-") {
                                ViewAllNewFeeds(0);
                            }
                            else {
                                GetRSSFeeds(currRSSselected, false, 0);
                            }
                        }
                    }
                }
            },
            error: function (data) {
                RemoveLoadingOverlayRSS();
                $("#rssfeed_Holder").html("<h3 class='pad-all' style='color: Red'>Error loading news feeds. Please close app and try again.</h3>");
            }
        });
    }
}

$(document.body).on("change", "#Saved-RSSFeeds", function () {
    LoadingRSSFeed("Building RSS Feed...");
    if (!openWSE_Config.demoMode) {
        cookie.set("rssfeed-current", $(this).val(), "30");
    }
    currRSSselected = $(this).val();
    if ($(this).val() == "-MostRecent-") {
        ViewAllNewFeeds(0);
    }
    else {
        GetRSSFeeds($(this).val(), false, 0);
    }
});

$(document.body).on("change", "#RSSFeedsToPull", function () {
    if (!openWSE_Config.demoMode) {
        cookie.set("rssfeed-current-viewable", $(this).val(), "30");
    }
    var x = document.getElementById("Saved-RSSFeeds");
    if (x.options.length > 0) {
        LoadingRSSFeed("Building RSS Feed...");
        if (currRSSselected == "-MostRecent-") {
            ViewAllNewFeeds(0);
        }
        else {
            GetRSSFeeds(currRSSselected, false, 0);
        }
    }
});

function ViewAllNewFeeds(index) {
    var x = document.getElementById("Saved-RSSFeeds");
    if (index == 0) {
        $("#rssfeed_Holder").html("<ul id='viewallul' class='rssFeed'></ul>");
    }

    if (index < x.length) {
        RemoveLoadingOverlayRSS();
        LoadingRSSFeed("Building RSS Feeds...");
        if (x.options[index].value != "-MostRecent-") {
            GetRSSFeeds(x.options[index].value, true, index);
        }
        else {
            index = index + 1;
            ViewAllNewFeeds(index);
        }
    }

    CorrectHrefLinks_RssFeeds("rssfeed_Holder");
}

var gettingFeeds = false;
function GetRSSFeeds(url, viewAll, index) {
    if (!gettingFeeds) {
        var search = "";
        if ($("#tb_search_rssfeed").val().length > 0) {
            search = $("#tb_search_rssfeed").val();
        }
        var _show = 5;
        if ($("#RSSFeedsToPull").length > 0) {
            _show = $("#RSSFeedsToPull").val();
        }

        if ((url == null) || (url == undefined)) {
            url = "";
        }

        if (viewAll) {
            _show = 1;
        }

        gettingFeeds = true;
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetRSSFeed",
            type: "POST",
            data: '{ "_url": "' + escape(url) + '","_show": "' + _show + '","search": "' + escape(search) + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = $.trim(data.d);
                RemoveLoadingOverlayRSS();
                gettingFeeds = false;
                if (viewAll) {
                    $("#viewallul").append(response);
                    var x = document.getElementById("Saved-RSSFeeds");
                    index = index + 1;
                    if (index < x.length) {
                        ViewAllNewFeeds(index);
                    }
                    else {
                        if ($.trim($("#viewallul").html()) == "") {
                            $("#rssfeed_Holder").html("<h3 class='pad-all'>No feeds founds.</h3>");
                        }
                        else {
                            CorrectHrefLinks_RssFeeds("rssfeed_Holder");
                        }
                    }
                }
                else {
                    if (response == "") {
                        $("#rssfeed_Holder").html("<h3 class='pad-all'>No feeds founds.</h3>");
                    }
                    else {
                        $("#rssfeed_Holder").html("<ul class='rssFeed'>" + response + "</ul>");
                        CorrectHrefLinks_RssFeeds("rssfeed_Holder");
                    }
                }
            },
            error: function (data) {
                $("#rssadderror").html("<h3 class='pad-all' style='color: Red'>There seems to be an error getting the requested feeds. Try changing the Feeds to Show to a smaller number.</h3>");
                gettingFeeds = false;
                RemoveLoadingOverlayRSS();
            }
        });
    }
}

function RemoveNotFoundRssFeed(_this, url) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/DeleteFeedFromList",
        type: "POST",
        data: '{ "_url": "' + escape(url) + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = $.trim(data.d);
            if (openWSE.ConvertBitToBoolean(response)) {
                var $parent = $(_this).parent();
                if ($parent.hasClass("remove-rss-q")) {
                    var $li = $parent.closest(".remove-rss-li");
                    if ($li.length > 0) {
                        $li.remove();
                    }
                }

                if (currRSSselected != "-MostRecent-") {
                    currRSSselected = "-MostRecent-";
                    if (!openWSE_Config.demoMode) {
                        cookie.set("rssfeed-current", "-MostRecent-", "30");
                    }
                    GetRSSHeaders(false);
                    ViewAllNewFeeds(0);
                    ViewAllNewFeedsOverlay(0);
                }
            }
        },
        error: function (data) {
            openWSE.AlertWindow("Could not delete feed! Try again.");
        }
    });
}

function CancelNotFoundRssFeed(_this) {
    var $parent = $(_this).parent();
    if ($parent.hasClass("remove-rss-q")) {
        $parent.remove();
    }
}

function AddCustomRSSUrl() {
    var url = $("#tb_addcustomrss").val();
    if ((url != "") && (url != "Link to RSS feed")) {
        LoadingRSSFeed("Adding RSS Feed...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/AddCustomFeed",
            type: "POST",
            data: '{ "_url": "' + escape(url) + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var title = data.d[0];
                var id = data.d[1];
                RemoveLoadingOverlayRSS();
                if ((title != "error") && (id != "")) {
                    $("#myfeedsmessage").remove();
                    $("#myfeedsholder").prepend("<div id='" + id + "' class='add-rss-list-item add-rss-list-item-hasitem' title='Add " + title + "' onclick=\"AddFeed(this, '" + url + "')\">" + title + "</div>");
                    $("#rssadderror").html("<h4 style='color: Green;'>" + title + " has been added successfully.</h4>");
                    $("#tb_addcustomrss").val("Link to RSS feed");
                    GetRSSHeaders(false);
                }
                else {
                    $("#rssadderror").html("<h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                }
                setTimeout(function () {
                    $("#rssadderror").html("");
                }, 3000);
            },
            error: function (data) {
                RemoveLoadingOverlayRSS();
                $("#rssadderror").html("<h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                setTimeout(function () {
                    $("#rssadderror").html("");
                }, 3000);
            }
        });
    }
}

function LoadingRSSFeed(message) {
    RemoveLoadingOverlayRSS();
    var x = "<div id='update-element-rss'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#rssfeed-load").append(x);
    $("#update-element-rss").show();
}

function BuildADDRSSList() {
    var x = "<div class='add-rss-list-holder'>";
    x += "<div class='add-rss-list-header'>My Feeds</div><div id='myfeedsholder'>";
    $.ajax({
        url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetUserFeeds",
        type: "POST",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var count = 0;
            RemoveLoadingOverlayRSS();
            if (data.d[0].length > 0) {
                for (var i = 0; i < data.d[0].length; i++) {
                    if (openWSE.ConvertBitToBoolean(data.d[2][i])) {
                        var title = data.d[0][i];
                        var url = data.d[1][i];
                        var id = "";
                        if (data.d[3][i] != "") {
                            id = "id = '" + data.d[3][i] + "'";
                        }
                        x += "<div " + id + " class='add-rss-list-item' title='Add " + title + "' onclick=\"AddFeed(this, '" + url + "')\">" + title + "</div>";
                        count++;
                    }
                }
            }

            if (count == 0) {
                x += "<h4 id='myfeedsmessage' class='pad-all'>No custom feeds added.</h4>";
            }
            x += "</div>";
            LoadStandardRSSList(data, x);
        },
        error: function (data) {
            RemoveLoadingOverlayRSS();
            x += "<h4 id='myfeedsmessage' class='pad-all' style='color: Red'>Error loading your rss feeds. Please close app and try again.</h4>";
            x += "</div>";
        }
    });
}

function LoadStandardRSSList(data, x) {
    $.ajax({
        type: "GET",
        dataType: "xml",
        url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeeds.xml",
        cache: false,
        complete: function (xml) {
            $(xml.responseText).find('Items').each(function () {
                $(this).find("Item").each(function () {
                    if ($(this).find("Category").length != 0) {
                        var category = $(this).find("Category").text();
                        x += "<div class='add-rss-list-header'>" + category + "</div>";
                    }
                    else {
                        var id = $(this).find("RSSID").text();
                        var title = $(this).find("RSSTitle").text();
                        var url = $(this).find("RSSUrl").text();
                        x += "<div id='" + id + "' class='add-rss-list-item' title='Add " + title + "' onclick=\"AddFeed(this, '" + url + "')\">" + title + "</div>";
                    }
                });
            });

            x += "</div>";
            $("#AddRSSFeedHolder").html(x);
            if (data != null) {
                for (var i = 0; i < data.d[0].length; i++) {
                    $(".add-rss-list-item").each(function () {
                        if ($(this).attr("id") == data.d[3][i]) {
                            $(this).addClass("add-rss-list-item-hasitem");
                            $(this).attr("title", "Remove " + $(this).html());
                        }
                    });
                }
            }

            if ($("#RSS-Feed-Selector-element").css("display") != "block") {
                openWSE.LoadModalWindow(true, 'RSS-Feed-Selector-element', 'Add RSS Feed');
            }
        }
    });
}

function AddFeed(_this, url) {
    var title = $(_this).html();
    var needAdd = "true";
    if ($(_this).hasClass("add-rss-list-item-hasitem")) {
        needAdd = "false";
        if (currRSSselected == url) {
            currRSSselected = "";
        }

        if ($(_this).parent().attr("id") == "myfeedsholder") {
            $(_this).remove();
            if ($.trim($("#myfeedsholder").html()) == "") {
                $("#myfeedsholder").html("<h4 id='myfeedsmessage' class='pad-all'>No custom feeds added.</h4>");
            }
        }
        else {
            $(_this).removeClass("add-rss-list-item-hasitem");
        }
    }
    else {
        $(_this).addClass("add-rss-list-item-hasitem");
    }

    var rssid = $(_this).attr("id");

    LoadingRSSFeed("Updating RSS Feed...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/AddRemoveFeed",
        type: "POST",
        data: '{ "_title": "' + escape(title) + '","_url": "' + escape(url) + '","_rssid": "' + rssid + '","_needAdd": "' + needAdd + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            RemoveLoadingOverlayRSS();
            GetRSSHeaders(false);
        },
        error: function (data) {
            RemoveLoadingOverlayRSS();
        }
    });
}




// OVERLAY CODE
var feedListOverlay = new Array();
function ViewAllNewFeedsOverlay(index) {
    if ($("#rssfeeds_pnl_entries").css("display") != "none") {
        if (index == 0) {
            $.ajax({
                url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetUserFeeds",
                type: "POST",
                data: '{ }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var refreshBtn = "<a href='#' class='float-right' onclick='LoadingNewFeedOverlay();setTimeout(function() { ViewAllNewFeedsOverlay(0); }, 250);return false;'>Refresh</a>";
                    $("#rssfeeds_pnl_entries").html(refreshBtn + "<div class='clear-space-two'></div><ul id='viewalluloverlay' class='rssFeed'></ul>");
                    if (data.d[0].length > 0) {
                        for (var i = 0; i < data.d[0].length; i++) {
                            feedListOverlay[i] = data.d[1][i];
                        }
                    }
                    else if ((data.d[0].length == 0) && (index == 0)) {
                        $("#viewalluloverlay").html("<h4 class='pad-all'>You have no saved feeds. Open the RSS News Feed app to add some.</h4>");
                    }

                    if (index < feedListOverlay.length) {
                        index = Math.floor(Math.random() * feedListOverlay.length);
                        GetRSSFeedsOverlay(feedListOverlay[index], index);
                    }
                }
            });
        }
        else {
            if (index < feedListOverlay.length) {
                index = Math.floor(Math.random() * feedListOverlay.length);
                GetRSSFeedsOverlay(feedListOverlay[index], index);
            }
        }

        CorrectHrefLinks_RssFeeds("rssfeeds_pnl_entries");
    }
}
function LoadingNewFeedOverlay() {
    $("#viewalluloverlay").html("<li style='border-bottom: 1px solid transparent!important;'><h4 class='pad-all'>Loading next feed. Please wait...</h4></li>");
}

var gettingFeedsOverlay = false;
function GetRSSFeedsOverlay(url, index) {
    if (!gettingFeedsOverlay) {
        if ((url == null) || (url == undefined)) {
            url = "";
        }
        SetRSSOverlayMaxHeight();
        gettingFeedsOverlay = true;

        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetRSSFeed",
            type: "POST",
            data: '{ "_url": "' + escape(url) + '","_show": "' + 1 + '","search": "' + "" + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                gettingFeedsOverlay = false;
                if (response != "") {
                    $("#viewalluloverlay").append(response);
                    CorrectHrefLinks_RssFeeds("rssfeeds_pnl_entries");
                    $("#RSSFeed_Overlay_Position").find(".rssFeed li").css("border-bottom", "none");
                }
            },
            error: function (data, textStatus, errorThrown) {
                gettingFeedsOverlay = false;
            }
        });
    }
}

function RemoveLoadingOverlayRSS() {
    $("#update-element-rss").each(function (index) {
        $(this).remove();
    });
}

function SetRSSOverlayMaxHeight() {
    if ($("#RSSFeed_Overlay_Position").css("display") != "none") {
        var bufferBottom = 90;
        if ($("#MainContent_pnl_adminnote").length > 0) {
            bufferBottom = $("#MainContent_pnl_adminnote").height() + 75;
        }
        var headerHeight = $("#RSSFeed_Overlay_Position").find(".overlay-header").height();
        var padTop = parseFloat($("#RSSFeed_Overlay_Position").find(".overlay-header").css("padding-top"));
        var padBottom = parseFloat($("#RSSFeed_Overlay_Position").find(".overlay-header").css("padding-bottom"));

        var sidebarWidth = $("#side-bar-controls").width();
        var windowWidth = $(window).width();
    }
}

function CorrectHrefLinks_RssFeeds(elem) {
    $("#" + elem).find("a").each(function () {
        var $this = $(this);
        if ($this.attr("target") != "_blank") {
            $this.attr("target", "_blank");
        }
    });
}