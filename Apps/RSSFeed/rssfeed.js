$(window).resize(function () {
    RSSFeedApp.ResizeAppWindow();
});

function pageLoad() {
    RSSFeedApp.StartRSSFeeder();
    RSSFeedApp.StartRSSFeederOverlay();
}

var RSSFeedApp = function () {
    /* -- Private Variables -- */
    var rssTimer;
    var rssTimerInterval = (60000 * 5) // 5 minutes
    var gettingFeeds = false;
    var gettingFeeds_Overlay = false;
    var currRSSselected = "";
    var rssFeedIsOpen = false;
    var scrollPosBeforeClick = 0;
    var showShareBtns = true;
    var currViewMode = "";
    var topbarfixedscroll = false;

    function ResizeAppWindow() {
        try {
            RSSFeedApp.SetRSSOverlayMaxHeight();
            var scrolltop = 0;

            if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                var appHt = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].clientHeight;
                scrolltop = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].scrollTop;
                $(".close-rss-feed").css("top", appHt - ($(".close-rss-feed").outerHeight() + 10) + scrolltop);
                $("#rssfeed-sidebar-menu").css("margin-top", scrolltop);
                $("#rssfeed-sidebar-menu").css("height", appHt);

                RSSFeedApp.ShowBackToTop();
            }
            else {
                var appHt = $(".app-main-holder[data-appid='app-rssfeed']").find("#rssfeed-load")[0].clientHeight;
                scrolltop = $(".app-main-holder[data-appid='app-rssfeed']")[0].scrollTop;
                $(".close-rss-feed").css("top", appHt - ($(".close-rss-feed").outerHeight() + 10) + scrolltop);
                $("#rssfeed-sidebar-menu").css("margin-top", scrolltop);
                $("#rssfeed-sidebar-menu").css("height", appHt);

                RSSFeedApp.ShowBackToTop();
            }

            if (topbarfixedscroll) {
                if (scrolltop > GetAppTitleBgColorHt()) {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").addClass("scroll-shadow-rss-topmenubar");
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                        top: scrolltop - GetAppTitleBgColorHt()
                    });
                }
                else {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").removeClass("scroll-shadow-rss-topmenubar");
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                        top: ""
                    });
                }
            }
        }
        catch (evt) { }

        $(".app-main-holder[data-appid='app-rssfeed']").find(".rssFeed").removeClass("rss-feed-800-maxwidth");
        if ($(".app-main-holder[data-appid='app-rssfeed']").outerWidth() < 800) {
            $(".app-main-holder[data-appid='app-rssfeed']").find(".rssFeed").addClass("rss-feed-800-maxwidth");
        }
    }

    function GetAppTitleBgColorHt() {
        if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-title-bg-color").css("display") == "none") {
            return 0;
        }

        return $(".app-main-holder[data-appid='app-rssfeed']").find(".app-title-bg-color").outerHeight();
    }

    function StartRSSFeeder() {
        if ($("#rssfeed_Holder").length > 0) {
            if ($.trim($("#rssfeed_Holder").html()) == "") {
                LoadingRSSFeed("Loading...");
            }

            if (openWSE_Config.demoMode) {
                $("#UCapp_rssfeed_btn_AddRemoveFeeds").remove();
                $("#MainContent_UCapp_rssfeed_btn_AddRemoveFeeds").remove();
            }

            currRSSselected = cookie.get("rssfeed-current");

            var feedsToPull = cookie.get("rssfeed-feedstopull");
            if (feedsToPull && feedsToPull != null && feedsToPull != "") {
                $("#dd_feedstopull").val(feedsToPull);
            }

            var topbarfixedscrollCookie = cookie.get("rssfeed-topbarfixedscroll");
            if (topbarfixedscrollCookie && topbarfixedscrollCookie != null && topbarfixedscrollCookie != "") {
                topbarfixedscroll = openWSE.ConvertBitToBoolean(topbarfixedscrollCookie);
            }

            $("#cb_rssfeed_topbarfixedscroll").prop("checked", topbarfixedscroll);

            var viewMode = cookie.get("rssfeed-viewmode");
            if (!viewMode || viewMode == "" || viewMode == "null" || viewMode == "undefined") {
                viewMode = "cards";
            }

            if (viewMode && viewMode != null && viewMode != "") {
                $("#dd_rssviewmode").val(viewMode);
            }

            var showLoading = true;
            if ($("#rssfeed_Holder").find(".rssFeed").find("li").length > 0) {
                showLoading = false;
            }

            GetRSSFeeds(showLoading);

            if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scroll(function () {
                    var appHt = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].clientHeight;
                    var scrolltop = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].scrollTop;
                    $(".close-rss-feed").css("top", appHt - ($(".close-rss-feed").outerHeight() + 10) + scrolltop);
                    $("#rssfeed-sidebar-menu").css("margin-top", scrolltop);
                    $("#rssfeed-sidebar-menu").css("height", appHt);

                    RSSFeedApp.ShowBackToTop();

                    if (topbarfixedscroll) {
                        if (scrolltop > GetAppTitleBgColorHt()) {
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").addClass("scroll-shadow-rss-topmenubar");
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                                top: scrolltop - GetAppTitleBgColorHt()
                            });
                        }
                        else {
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").removeClass("scroll-shadow-rss-topmenubar");
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                                top: ""
                            });
                        }
                    }
                });
            }
            else {
                $(".app-main-holder[data-appid='app-rssfeed']").scroll(function () {
                    var appHt = $(".app-main-holder[data-appid='app-rssfeed']").find("#rssfeed-load")[0].clientHeight;
                    var scrolltop = $(".app-main-holder[data-appid='app-rssfeed']")[0].scrollTop;
                    $(".close-rss-feed").css("top", appHt - ($(".close-rss-feed").outerHeight() + 10) + scrolltop);
                    $("#rssfeed-sidebar-menu").css("margin-top", scrolltop);
                    $("#rssfeed-sidebar-menu").css("height", appHt);

                    RSSFeedApp.ShowBackToTop();

                    if (topbarfixedscroll) {
                        if (scrolltop > GetAppTitleBgColorHt()) {
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").addClass("scroll-shadow-rss-topmenubar");
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                                top: scrolltop - GetAppTitleBgColorHt()
                            });
                        }
                        else {
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").removeClass("scroll-shadow-rss-topmenubar");
                            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                                top: ""
                            });
                        }
                    }
                });
            }

            if ((rssTimer == null) || (rssTimer == undefined)) {
                StartRSSTimer();
            }
        }
    }
    function StartRSSFeederOverlay() {
        if ($("#rssfeeds_pnl_entries").length > 0) {
            $("#rssfeeds_pnl_entries").html("<h3 class='pad-all'>Loading RSS Feeds...</h3>");
            GetRSSFeeds_Overlay(true);
            if ((rssTimer == null) || (rssTimer == undefined)) {
                StartRSSTimer();
            }
        }
    }
    function StartRSSTimer() {
        rssTimer = setInterval(function () {
            if (!rssFeedIsOpen) {
                if ($("#rssfeed-load").length > 0) {
                    GetRSSFeeds(false);
                }
                if ($("#rssfeeds_pnl_entries").length > 0) {
                    GetRSSFeeds_Overlay(true);
                }
            }
        }, rssTimerInterval);
    }

    function Admin_GrabLatestFeeds() {
        openWSE.LoadingMessage1("Getting Latest...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Update_LoadedFeedList",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occurred. Please try again.");
            }
        });
    }
    function Admin_ClearFeedList() {
        openWSE.LoadingMessage1("Clearing List...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Clear_LoadedFeedList",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occurred. Please try again.");
            }
        });
    }
    function Admin_LoadStoredFeedList() {
        openWSE.LoadingMessage1("Loading From File...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Load_LoadedFeedList_FromFile",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d != "") {
                    openWSE.AlertWindow(data.d);
                }
                openWSE.RemoveUpdateModal();
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occurred. Please try again.");
            }
        });
    }
    function Admin_GetLoadStoredFeedListCount() {
        openWSE.LoadingMessage1("Getting Count...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Get_LoadedFeedList_Count",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d != "") {
                    openWSE.AlertWindow(data.d);
                }
                openWSE.RemoveUpdateModal();
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occurred. Please try again.");
            }
        });
    }

    function GetRSSFeeds(showLoading) {
        if (!openWSE_Config.demoMode) {
            cookie.set("rssfeed-feedstopull", $("#dd_feedstopull").val(), "30");
        }

        if (!gettingFeeds && !rssFeedIsOpen) {
            currViewMode = "";

            if ((currRSSselected == "null" || !currRSSselected) && !openWSE_Config.demoMode) {
                currRSSselected = "Highlights";
            }
            else if ((currRSSselected == "null" || !currRSSselected) && openWSE_Config.demoMode) {
                currRSSselected = "Video Games";
            }

            if (showLoading) {
                LoadingRSSFeed("Loading " + currRSSselected + "...");
            }

            var search = "";
            if ($("#tb_search_rssfeed").length > 0) {
                search = $("#tb_search_rssfeed").val();
            }

            gettingFeeds = true;
            $.ajax({
                url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetRSSFeed",
                type: "POST",
                data: '{ "category": "' + escape(currRSSselected) + '","search": "' + escape(search) + '", "feedsToPull": "' + $("#dd_feedstopull").val() + '", "forOverlay": "false" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d.length == 2) {
                        BuildRSSFeeds($.parseJSON(data.d[0]));
                        BuildRSSHeaders(data.d[1]);
                    }

                    RSSFeedApp.UpdateViewMode();
                    RSSFeedApp.HideRSSLoading();
                    gettingFeeds = false;

                    ResizeAppWindow();

                    if (data.d[1] && currRSSselected != "Highlights") {
                        var foundFeedHeader = false;
                        for (var i = 0; i < data.d[1].length; i++) {
                            if (data.d[1][i] == currRSSselected) {
                                foundFeedHeader = true;
                                break;
                            }
                        }

                        if (!foundFeedHeader) {
                            currRSSselected = "Highlights";
                            $("#rssfeed_Holder").html("");
                            GetRSSFeeds(true);
                        }
                    }
                },
                error: function (data) {
                    $("#rssadderror").html("<h3 class='pad-all' style='color: Red'>There seems to be an error getting the requested feeds.</h3>");
                    gettingFeeds = false;
                    RSSFeedApp.HideRSSLoading();
                }
            });
        }
    }
    function BuildRSSFeeds(data) {
        var feedList = "";
        for (var i = 0; i < data.length; i++) {
            var title = data[i].Title;
            var link = data[i].Link;
            var summary = data[i].Summary;
            var content = data[i].Content;
            var pubDate = new Date(parseInt(data[i].PubDate.replace("/Date(", "").replace(")/", "")));
            var creator = "";
            var bgStyleClass = data[i].BgStyleClass;
            if (bgStyleClass && bgStyleClass.toLowerCase().indexOf("http://images/icon-rss.png") != -1) {
                bgStyleClass = "class='feed-preview rss-has-no-image'";
            }

            if (pubDate && pubDate != null) {
                pubDate = pubDate.toLocaleString();
            }

            if (creator != "N/A" && creator != "") {
                creator = " by " + data[i].Creator;
            }

            feedList += "<div " + bgStyleClass + " onclick=\"RSSFeedApp.OpenRSSFeedContent(event, this);\"><div class='rss-inner-overlay'></div><div class='rss-inner-padding'>";

            if (title != "") {
                var tempTitle = title;
                if (tempTitle.Length > 125) {
                    tempTitle = tempTitle.Substring(0, 125) + "...";
                }

                if (summary != "" || content != "") {
                    feedList += "<span class='rss-title-preview'>" + tempTitle + "</span><br />";
                }
                else {
                    feedList += "<a class='rss-title-preview' href='" + link + "' target='_blank'>" + tempTitle + "</a><br />";
                }

                if (link != "") {
                    feedList += "<a class='rss-title' href='" + link + "' target='_blank'>" + title + "</a><br />";
                }
                else {
                    feedList += "<span class='rss-title'>" + title + "</span><br />";
                }
            }

            feedList += "<div class='rss-author'>" + pubDate + creator + "</div>";
            feedList += "<div class='rss-description'>" + summary + "</div>";
            feedList += "<div class='rss-content'>" + content + "</div>";
            if (data[i].Source != "" && data[i].Source != null) {
                feedList += "<div class='rss-article-source'> - " + data[i].Source + " - </div>";
            }
            else if (data[i].SourceImage != "" && data[i].SourceImage != null && data[i].SourceImage != "null") {
                feedList += "<div class='rss-article-source'><img alt='' src='" + data[i].SourceImage + "' /></div>";
            }
            feedList += "</div>";

            if (data[i].SourceImage != "" && data[i].SourceImage != null && data[i].SourceImage != "null") {
                feedList += "<span class='rss-source-preview'><img alt='' src='" + data[i].SourceImage + "' /></span>";
            }
            else if (data[i].Source != "" && data[i].Source != null) {
                feedList += "<span class='rss-source-preview'>" + data[i].Source + "</span>";
            }
            
            feedList += "</div>";
        }

        if (feedList == "") {
            $("#rssfeed_Holder").html("<div class='pad-all'>No feeds founds.</div>");
        }
        else {
            $("#rssfeed_Holder").html("<div class='rssFeed'>" + feedList + "</div>");
            CorrectHrefLinks_RssFeeds("rssfeed_Holder");
        }
    }
    function BuildRSSHeaders(data) {
        if ($("#rssfeed-load").length > 0) {
            // Clear out the dropdown before rebuilding
            $("#Saved-RSSFeeds").html("");

            if (data != null) {
                if (data.length == 0) {
                    $("#Saved-RSSFeeds").html("<div class='rss-feed-selector-none'>No Feeds Founds</div>");
                    $("#rssfeed_Holder").html("<div class='pad-all font-color-light-black'>No RSS feeds available. Click Edit Feeds to view available feeds.</div>");
                }
                else {
                    var feedOptions = "";
                    feedOptions += "<div class='rss-feed-selector' data-val='Highlights'>Highlights</div>";

                    for (var i = 0; i < data.length; i++) {
                        feedOptions += "<div class='rss-feed-selector' data-val='" + data[i] + "'>" + data[i] + "</div>";
                    }

                    $("#Saved-RSSFeeds").html(feedOptions);

                    if (currRSSselected != "") {
                        $("#Saved-RSSFeeds").find(".rss-feed-selector[data-val='" + currRSSselected + "']").addClass("selected-feed");
                    }
                    else if (currRSSselected == "" && $(".rss-feed-selector").length > 0) {
                        $("#Saved-RSSFeeds").find(".rss-feed-selector").eq(0).addClass("selected-feed");
                    }
                }
            }
        }
    }

    function GetRSSFeeds_Overlay(showLoading) {
        if (!gettingFeeds_Overlay) {
            if (showLoading) {
                $("#rssfeeds_pnl_entries").html("<h3 class='pad-all'>Loading RSS Feeds...</h3>");
            }

            SetRSSOverlayMaxHeight();

            gettingFeeds_Overlay = true;
            $.ajax({
                url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetRSSFeed",
                type: "POST",
                data: '{ "category": "' + "" + '","search": "", "feedsToPull": "1", "forOverlay": "true" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d.length > 0) {
                        BuildRSSOverlayFeed($.parseJSON(data.d[0]));
                    }

                    gettingFeeds_Overlay = false;
                },
                error: function (data) {
                    $("#rssfeeds_pnl_entries").html("<h3 class='pad-all' style='color: Red'>There seems to be an error getting the requested feeds.</h3>");
                    gettingFeeds_Overlay = false;
                }
            });
        }
    }
    function BuildRSSOverlayFeed(data) {
        var feedList = "";

        if (data) {
            for (var i = 0; i < data.length; i++) {
                var title = data[i].Title;
                var link = data[i].Link;
                var summary = data[i].Summary;
                var content = data[i].Content;
                var pubDate = new Date(parseInt(data[i].PubDate.replace("/Date(", "").replace(")/", "")));
                var creator = "";

                if (pubDate && pubDate != null) {
                    pubDate = pubDate.toLocaleString();
                }

                if (creator != "N/A" && creator != "") {
                    creator = " by " + data[i].Creator;
                }

                if (title != "") {
                    if (link != "") {
                        feedList += "<a class='rss-title' href='" + link + "' target='_blank'>" + title + "</a><br />";
                    }
                    else {
                        feedList += "<span class='rss-title'>" + title + "</span><br />";
                    }
                }

                feedList += "<div class='rss-author'>" + pubDate + creator + "</div>";

                var shareBtns = "";
                if (showShareBtns && title != "" && link != "") {
                    var rssShareLink = encodeURIComponent(link);
                    var rssShareTitle = encodeURIComponent(title);
                    shareBtns = BuildShareButtons(rssShareLink, rssShareTitle);
                }

                feedList += "<div class='rss-description'>" + shareBtns + summary + "</div>";
                feedList += "<div class='rss-content'>" + content + "</div>";
                feedList += "<div class='clear-space'></div>";
                if (data[i].SourceImage != "" && data[i].SourceImage != null && data[i].SourceImage != "null" && data[i].Source != "") {
                    feedList += "<div class='rss-article-source'><img alt='' src='" + data[i].SourceImage + "' /><div class='clear-space-five'></div>- " + data[i].Source + " -</div>";
                }
                else if (data[i].SourceImage != "" && data[i].SourceImage != null && data[i].SourceImage != "null") {
                    feedList += "<div class='rss-article-source'><img alt='' src='" + data[i].SourceImage + "' /><div class='clear'></div></div>";
                }
                else if (data[i].Source != "" && data[i].Source != null) {
                    feedList += "<div class='rss-article-source'> - " + data[i].Source + " - </div>";
                }
            }
        }
        var refreshBtn = "<a href='#' class='float-right' onclick='RSSFeedApp.GetRSSFeeds_Overlay(true);return false;'>Refresh</a>";

        if (feedList == "") {
            $("#rssfeeds_pnl_entries").html(refreshBtn + "<div class='clear-space-two'></div><div class='pad-all'>No feeds founds.</div>");
        }
        else {
            $("#rssfeeds_pnl_entries").html(refreshBtn + "<div class='clear-space-two'></div>" + feedList);
            CorrectHrefLinks_RssFeeds("rssfeeds_pnl_entries");
        }
    }

    function SetRSSOverlayMaxHeight() {
        if ($("#rssfeeds_pnl_entries").length > 0) {
            $("#rssfeeds_pnl_entries").css("max-height", $(window).height() - 120);
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

    function MenuClick() {
        $(".rss-feed-overlaymenu").remove();
        var $menu = $("#rssfeed-sidebar-menu");
        if ($menu.length > 0) {
            if (!$menu.hasClass("showmenu")) {
                $menu.show();
                $menu.addClass("showmenu");
                if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scroll();
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").css("overflow", "hidden");
                }
                else {
                    $(".app-main-holder[data-appid='app-rssfeed']").scroll();
                    $(".app-main-holder[data-appid='app-rssfeed']").css("overflow", "hidden");
                }
                $("#rssfeed_overflow").append("<div class='rss-feed-overlaymenu'></div>");
                $menu.find(".customtable-sidebar-innercontent").show();
                $menu.css("width", $("#rssfeed-load").outerWidth() - 50);
            }
            else {
                $menu.removeClass("showmenu");
                $menu.find(".customtable-sidebar-innercontent").hide();
                if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").css("overflow", "");
                }
                else {
                    $(".app-main-holder[data-appid='app-rssfeed']").css("overflow", "");
                }

                $menu.css("width", 0);
                $menu.hide();
            }
        }
    }

    function LoadingRSSFeed(message) {
        $("#rssfeed-load").find(".loading-message").show();
        $("#rssfeed-load").find(".loading-message").html(message);
    }
    function HideRSSLoading() {
        $("#rssfeed-load").find(".loading-message").hide();
        $("#rssfeed-load").find(".loading-message").html("");
    }

    function SetPositionOfRSSCloseButton() {
        if ($(".close-rss-feed").length > 0) {
            if ($(".app-main-holder[data-appid='app-rssfeed']").find("#rssfeed-load").length > 0) {
                if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scroll();
                }
                else {
                    $(".app-main-holder[data-appid='app-rssfeed']").scroll();
                }
            }
            else {
                $(".close-rss-feed").css("bottom", "10px");
            }
        }
    }
    function OpenRSSFeedContent(event, _this) {
        var $this = $(_this);

        if ($("#dd_rssviewmode").val() == "full") {
            return false;
        }

        if (!rssFeedIsOpen && (event.target.className.indexOf("rss-title-preview") != -1 || event.target.className.indexOf("rss-inner-padding") != -1 || event.target.className.indexOf("rss-inner-overlay") != -1)) {
            $(".rssFeed").find(".feed-preview").removeClass("feed-open");
            $(".rssFeed").find(".feed-preview").removeClass("feed-hidden");
            $(".close-rss-feed").remove();
            $(".share-buttons").remove();
            $(".rss-img-link-clear").remove();

            if ($.trim($this.find(".rss-content").html()) == "" && $.trim($this.find(".rss-description").html()) == "") {
                if ($this.find(".rss-title").attr("href") == "") {
                    return false;
                }
            }
            else {
                if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                    scrollPosBeforeClick = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scrollTop();
                }
                else {
                    scrollPosBeforeClick = $(".app-main-holder[data-appid='app-rssfeed']").scrollTop();
                }

                rssFeedIsOpen = true;
                $(".rssFeed").find(".feed-preview").addClass("feed-hidden");
                $this.addClass("feed-open");

                AdjustFeedContentParts($this);

                if (showShareBtns) {
                    if ($this.find(".rss-description").length > 0 && $this.find(".rss-title").attr("href") != "" && $this.find(".rss-title").attr("href") != null) {
                        var rssShareLink = encodeURIComponent($this.find(".rss-title").attr("href"));
                        var rssShareTitle = encodeURIComponent($.trim($this.find(".rss-title").html()));
                        $this.find(".rss-description").prepend(BuildShareButtons(rssShareLink, rssShareTitle));
                    }
                }
                $("#rssfeed-load").append("<div class='close-rss-feed' onclick='RSSFeedApp.CloseRSSFeedContent();'></div>");

                SetPositionOfRSSCloseButton();

                if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                    $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scrollTop(0);
                }
                else {
                    $(".app-main-holder[data-appid='app-rssfeed']").scrollTop(0);
                }
            }
        }
    }
    function CloseRSSFeedContent() {
        rssFeedIsOpen = false;
        $(".rssFeed").find(".feed-preview").removeClass("feed-open");
        $(".rssFeed").find(".feed-preview").removeClass("feed-hidden");
        $(".close-rss-feed").remove();
        $(".share-buttons").remove();
        $(".rss-img-link-clear").remove();
        if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
            $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scrollTop(scrollPosBeforeClick);
        }
        else {
            $(".app-main-holder[data-appid='app-rssfeed']").scrollTop(scrollPosBeforeClick);
        }
    }

    function AdjustFeedContentParts(_this) {
        var $this = $(_this);
        $this.find("iframe").each(function () {
            var $iframe = $(this);
            if ($iframe.width() > $(window).width()) {
                $iframe.css("width", $(window).width() - 50);
                $iframe.css("height", $iframe.height() / 2);
            }
        });
        $this.find(".rss-description").find("img").each(function () {
            if ($(this).parent().prop("tagName").toLowerCase() == "a") {
                $(this).parent().after("<div class='clear rss-img-link-clear'></div>");
            }

            $(this).attr("height", "");
        });
        $this.find(".rss-content").find("img").each(function () {
            if ($(this).parent().prop("tagName").toLowerCase() == "a") {
                $(this).parent().after("<div class='clear rss-img-link-clear'></div>");
            }

            $(this).attr("height", "");
        });
    }

    function KeyPressSearch(event) {
        try {
            if (event.which == 13) {
                RSSFeedApp.MenuClick();
                GetRSSFeeds(true);
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                RSSFeedApp.MenuClick();
                GetRSSFeeds(true);
            }
            delete evt;
        }
    }

    $(document.body).on("click", "#Saved-RSSFeeds .rss-feed-selector", function () {
        if (currRSSselected != $(this).attr("data-val")) {
            if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scrollTop(0);
            }
            else {
                $(".app-main-holder[data-appid='app-rssfeed']").scrollTop(0);
            }

            scrollPosBeforeClick = 0;

            $("#rssfeed_Holder").html("");
            currRSSselected = $(this).attr("data-val");
            RSSFeedApp.LoadingRSSFeed("Loading " + currRSSselected + "...");

            $("#Saved-RSSFeeds").find(".rss-feed-selector").removeClass("selected-feed");
            $(this).addClass("selected-feed");

            if (!openWSE_Config.demoMode) {
                cookie.set("rssfeed-current", currRSSselected, "30");
            }

            RSSFeedApp.GetRSSFeeds(true);
            MenuClick();
        }
    });
    $(document.body).on("click", ".app-main-holder[data-appid='app-rssfeed'] .exit-button-app, .app-min-bar[data-appid='app-rssfeed'] .exit-button-app-min, .app-min-bar[data-appid='app-rssfeed'] .reload-button-app", function () {
        rssFeedIsOpen = false;
        clearInterval(rssTimer);
        rssTimer = null;
    });
    $(document.body).on("click", ".rss-feed-overlaymenu", function (e) {
        MenuClick();
        $(".rss-feed-overlaymenu").remove();
        $("#rssfeed-sidebar-menu").removeClass("showmenu");
    });

    function BuildShareButtons(link, title) {
        var shareBtns = '<div class="clear"></div><ul class="share-buttons">';
        shareBtns += '<li><a href="https://www.facebook.com/sharer/sharer.php?u=' + link + '&t=' + title + '" title="Share on Facebook" target="_blank"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/Facebook.png"></a></li>';
        shareBtns += '<li><a href="https://twitter.com/intent/tweet?source=' + link + '&text=' + title + '" target="_blank" title="Tweet"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/Twitter.png"></a></li>';
        shareBtns += '<li><a href="https://plus.google.com/share?url=' + link + '" target="_blank" title="Share on Google+"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/GooglePlus.png"></a></li>';
        shareBtns += '<li><a href="https://getpocket.com/save?url=' + link + '&title=' + title + '" target="_blank" title="Add to Pocket"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/Pocket.png"></a></li>';
        shareBtns += '<li><a href="http://www.reddit.com/submit?url=' + link + '&title=' + title + '" target="_blank" title="Submit to Reddit"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/Reddit.png"></a></li>';
        shareBtns += '<li><a href="http://www.linkedin.com/shareArticle?mini=true&url=' + link + '&title=' + title + '&summary=&source=" target="_blank" title="Share on LinkedIn"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/LinkedIn.png"></a></li>';
        shareBtns += '<li><a href="https://pinboard.in/popup_login/?url=' + link + '&title=' + title + '&description=" target="_blank" title="Save to Pinboard"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/ShareIcons/Pinboard.png"></a></li>';
        shareBtns += '</ul><div class="clear"></div>';
        return shareBtns;
    }

    function BuildADDRSSList() {
        var x = "<div class='add-rss-list-holder'>";
        x += "<div class='add-rss-list-header' onclick=\"RSSFeedApp.EditFeedHeaderClick(this);\" data-myfeeds='true'>My Feeds</div><div id='myfeedsholder'>";
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/GetUserFeeds",
            type: "POST",
            data: '{ }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var count = 0;
                RSSFeedApp.HideRSSLoading();
                if (data.d[0].length > 0) {
                    for (var i = 0; i < data.d[0].length; i++) {
                        if (openWSE.ConvertBitToBoolean(data.d[2][i])) {
                            var title = data.d[0][i];
                            var url = data.d[1][i];
                            var id = "";
                            if (data.d[3][i] != "") {
                                id = "id = '" + data.d[3][i] + "'";
                            }
                            x += "<div " + id + " class='add-rss-list-item' title='Add " + title + "' onclick=\"RSSFeedApp.AddFeed(this, '" + url + "')\">" + title + "</div>";
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
                RSSFeedApp.HideRSSLoading();
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
                var categoryArray = new Array()
                $(xml.responseText).find('Items').each(function () {
                    $(this).find("Item").each(function () {
                        if ($(this).find("Category").length != 0) {
                            var category = $(this).find("Category").text();
                            var foundCategory = false;
                            for (var i = 0; i < categoryArray.length; i++) {
                                if (categoryArray[i] == category) {
                                    foundCategory = true;
                                    break;
                                }
                            }

                            if (!foundCategory) {
                                x += "<div class='add-rss-list-header' onclick=\"RSSFeedApp.EditFeedHeaderClick(this);\">" + category + "</div><div class='add-rss-list-contents' data-feedcategory='" + category + "'></div>";
                                categoryArray.push(category);
                            }
                        }
                    });
                });

                x += "</div>";
                $("#AddRSSFeedHolder").html(x);

                $(xml.responseText).find('Items').each(function () {
                    $(this).find("Item").each(function () {
                        var category = $(this).find("Category").text();
                        var id = $(this).find("RSSID").text();
                        var title = $(this).find("RSSTitle").text();
                        var url = $(this).find("RSSUrl").text();
                        $(".add-rss-list-contents[data-feedcategory='" + category + "']").append("<div id='" + id + "' class='add-rss-list-item' title='Add " + title + "' onclick=\"RSSFeedApp.AddFeed(this, '" + url + "')\">" + title + "</div>");
                    });
                });

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
    function EditFeedHeaderClick(_this) {
        var category = $.trim($(_this).html());
        var $listItem;

        if ($(_this).attr("data-myfeeds") == "true") {
            $listItem = $("#myfeedsholder");
        }
        else {
            $listItem = $(".add-rss-list-contents[data-feedcategory='" + category + "']");
        }

        if ($listItem.length > 0) {
            if ($listItem.css("display") == "block") {
                $listItem.hide();
                AdjustEditFeedModal();
            }
            else {
                $listItem.show();
                AdjustEditFeedModal();
            }
        }
    }
    function AdjustEditFeedModal() {
        var top = $("#RSS-Feed-Selector-element").find(".Modal-element-modal").css("top");
        if (top == "auto") {
            $("#RSS-Feed-Selector-element").find(".Modal-element-align").css({
                marginTop: -($("#RSS-Feed-Selector-element").find(".Modal-element-modal").height() / 2),
                marginLeft: -($("#RSS-Feed-Selector-element").find(".Modal-element-modal").width() / 2)
            });
        }
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

        RSSFeedApp.LoadingRSSFeed("Updating RSS Feed...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/RSSFeed/RSSFeed.asmx/AddRemoveFeed",
            type: "POST",
            data: '{ "_title": "' + escape(title) + '","_url": "' + escape(url) + '","_rssid": "' + rssid + '","_needAdd": "' + needAdd + '" }',
            contentType: "application/json; charset=utf-8",
            complete: function () {
                RSSFeedApp.HideRSSLoading();
                GetRSSFeeds(false);
            }
        });
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
                    RSSFeedApp.HideRSSLoading();
                    if ((title != "error") && (id != "")) {
                        $("#myfeedsmessage").remove();
                        $("#myfeedsholder").prepend("<div id='" + id + "' class='add-rss-list-item add-rss-list-item-hasitem' title='Add " + title + "' onclick=\"RSSFeedApp.AddFeed(this, '" + url + "')\">" + title + "</div>");
                        $("#rssadderror").html("<h4 style='color: Green;'>" + title + " has been added successfully.</h4>");
                        $("#tb_addcustomrss").val("Link to RSS feed");
                        GetRSSFeeds(false);
                    }
                    else {
                        $("#rssadderror").html("<h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                    }
                    setTimeout(function () {
                        $("#rssadderror").html("");
                    }, 3000);
                },
                error: function (data) {
                    RSSFeedApp.HideRSSLoading();
                    $("#rssadderror").html("<h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                    setTimeout(function () {
                        $("#rssadderror").html("");
                    }, 3000);
                }
            });
        }
    }

    function UpdateViewMode() {
        var viewMode = $("#dd_rssviewmode").val();
        cookie.set("rssfeed-viewmode", viewMode, "30");

        var $feeds = $(".rssFeed").find(".feed-preview");

        if (currViewMode != viewMode) {
            currViewMode = viewMode;

            rssFeedIsOpen = false;
            $(".close-rss-feed").remove();
            $(".share-buttons").remove();
            $(".rss-img-link-clear").remove();
            $feeds.removeClass("feed-hidden");

            switch (viewMode) {
                case "full":
                    $feeds.addClass("feed-open");
                    $feeds.css("border-bottom", "1px solid #CCC");
                    $("#rssfeed_Holder").append("<div class='go-to-top' title='Back to top' onclick=\"RSSFeedApp.BackToTop();\"></div>");

                    $feeds.each(function () {
                        var $this = $(this);
                        AdjustFeedContentParts($this);
                        if (showShareBtns) {
                            if ($this.find(".rss-description").length > 0 && $this.find(".rss-title").attr("href") != "" && $this.find(".rss-title").attr("href") != null) {
                                var rssShareLink = encodeURIComponent($this.find(".rss-title").attr("href"));
                                var rssShareTitle = encodeURIComponent($.trim($this.find(".rss-title").html()));
                                $this.find(".rss-description").prepend(BuildShareButtons(rssShareLink, rssShareTitle));
                            }
                        }
                    });
                    break;

                default:
                    $feeds.removeClass("feed-open");
                    $feeds.css("border-bottom", "");
                    $("#rssfeed_Holder").find(".go-to-top").remove();
                    break;
            }

            if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scroll();
            }
            else {
                $(".app-main-holder[data-appid='app-rssfeed']").scroll();
            }
        }
    }

    function ShowBackToTop() {
        if ($("#dd_rssviewmode").val() == "full") {
            var $backToTop = $("#rssfeed_Holder").find(".go-to-top");
            var appHt = 0;
            var scrolltop = 0;

            if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
                appHt = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].clientHeight;
                scrolltop = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body")[0].scrollTop;
            }
            else {
                appHt = $(".app-main-holder[data-appid='app-rssfeed']").find("#rssfeed-load")[0].clientHeight;
                scrolltop = $(".app-main-holder[data-appid='app-rssfeed']")[0].scrollTop;
            }

            var titleBarHt = 0;
            if (GetAppTitleBgColorHt() == 0) {
                titleBarHt = $(".app-main-holder[data-appid='app-rssfeed']").find(".app-title-bg-color").outerHeight();
            }

            $backToTop.css("top", (appHt - ($backToTop.outerHeight() + 15) + scrolltop) - ($backToTop.outerHeight() + 4) + titleBarHt);
            if (scrolltop > 0) {
                $backToTop.show();
            }
            else {
                $backToTop.hide();
            }
        }
    }
    function BackToTop() {
        if ($(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").length > 0) {
            $(".app-main-holder[data-appid='app-rssfeed']").find(".app-body").scrollTop(0);
        }
        else {
            $(".app-main-holder[data-appid='app-rssfeed']").scrollTop(0);
        }
    }

    function SetTimeFeedUpdateInterval(interval) {
        rssTimerInterval = (60000 * interval);
        if (interval == 0) {
            $("#rss-feed-update-interval-text").html("");
        }
        else if (interval == 1) {
            $("#rss-feed-update-interval-text").html("Feeds are pulled every minute");
        }
        else {
            $("#rss-feed-update-interval-text").html("Feeds are pulled every " + interval + " minutes");
        }
    }

    function SetTopMenuBarToFixedScroll(_this) {
        if ($(_this).is(":checked")) {
            cookie.set("rssfeed-topbarfixedscroll", "true", "30");
            topbarfixedscroll = true;
            ResizeAppWindow();
        }
        else {
            cookie.set("rssfeed-topbarfixedscroll", "false", "30");
            topbarfixedscroll = false;
            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").removeClass("scroll-shadow-rss-topmenubar");
            $(".app-main-holder[data-appid='app-rssfeed']").find(".rss-menu-bar").css({
                top: ""
            });
        }
    }

    return {
        ResizeAppWindow: ResizeAppWindow,
        StartRSSFeeder: StartRSSFeeder,
        StartRSSFeederOverlay: StartRSSFeederOverlay,
        MenuClick: MenuClick,
        Admin_GrabLatestFeeds: Admin_GrabLatestFeeds,
        Admin_ClearFeedList: Admin_ClearFeedList,
        Admin_LoadStoredFeedList: Admin_LoadStoredFeedList,
        Admin_GetLoadStoredFeedListCount: Admin_GetLoadStoredFeedListCount,
        GetRSSFeeds: GetRSSFeeds,
        GetRSSFeeds_Overlay: GetRSSFeeds_Overlay,
        OpenRSSFeedContent: OpenRSSFeedContent,
        CloseRSSFeedContent: CloseRSSFeedContent,
        SetPositionOfRSSCloseButton: SetPositionOfRSSCloseButton,
        KeyPressSearch: KeyPressSearch,
        LoadingRSSFeed: LoadingRSSFeed,
        HideRSSLoading: HideRSSLoading,
        BuildADDRSSList: BuildADDRSSList,
        EditFeedHeaderClick: EditFeedHeaderClick,
        AddFeed: AddFeed,
        AddCustomRSSUrl: AddCustomRSSUrl,
        SetRSSOverlayMaxHeight: SetRSSOverlayMaxHeight,
        UpdateViewMode: UpdateViewMode,
        ShowBackToTop: ShowBackToTop,
        BackToTop: BackToTop,
        SetTimeFeedUpdateInterval: SetTimeFeedUpdateInterval,
        SetTopMenuBarToFixedScroll: SetTopMenuBarToFixedScroll
    };

}();
