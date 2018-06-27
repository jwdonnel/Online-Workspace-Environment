var RSSFeedApp_Overlay = function () {
    /* -- Private Variables -- */
    var rssTimer;
    var rssTimerInterval = (60000 * 5) // 5 minutes
    var gettingFeeds_Overlay = false;
    var showShareBtns = true;

    function StartRSSFeederOverlay() {
        gettingFeeds_Overlay = false;
        if ($("#rssfeeds_pnl_entries").length > 0) {
            $("#rssfeeds_pnl_entries").html("<h3 class='pad-all'>Loading RSS Feeds...</h3>");
            GetRSSFeeds_Overlay(true);
            if ((rssTimer == null) || (rssTimer == undefined)) {
                rssTimer = setInterval(function () {
                    if ($("#rssfeeds_pnl_entries").length > 0) {
                        GetRSSFeeds_Overlay(true);
                    }
                }, rssTimerInterval);
            }
        }
    }

    function GetRSSFeeds_Overlay(showLoading) {
        if (!gettingFeeds_Overlay) {
            if (showLoading) {
                $("#rssfeeds_pnl_entries").html("<h3 class='pad-all'>Loading RSS Feeds...</h3>");
            }

            gettingFeeds_Overlay = true;
            openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/GetRSSFeedOverlay", '{ }', null, function (data) {
                if (data.d.length > 0) {
                    BuildRSSOverlayFeed($.parseJSON(data.d[0]));
                }

                $(window).resize();
                gettingFeeds_Overlay = false;
            }, function (data) {
                $("#rssfeeds_pnl_entries").html("<h3 class='pad-all' style='color: Red'>There seems to be an error getting the requested feeds.</h3>");
                gettingFeeds_Overlay = false;
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
        var refreshBtn = "<a href='#' class='float-right' onclick='RSSFeedApp_Overlay.GetRSSFeeds_Overlay(true);return false;'>Refresh</a>";

        if (feedList == "") {
            $("#rssfeeds_pnl_entries").html(refreshBtn + "<div class='clear-space-two'></div><div class='pad-all'>No feeds founds.</div>");
        }
        else {
            $("#rssfeeds_pnl_entries").html(refreshBtn + "<div class='clear-space-two'></div>" + feedList);
            CorrectHrefLinks_RssFeeds("rssfeeds_pnl_entries");
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

    function LoadingRSSFeed(message) {
        $("#rssfeed-load").find(".loading-message").show();
        $("#rssfeed-load").find(".loading-message").html(message);
    }
    function HideRSSLoading() {
        $("#rssfeed-load").find(".loading-message").hide();
        $("#rssfeed-load").find(".loading-message").html("");
    }

    function BuildShareButtons(link, title) {
        var shareBtns = '<div class="clear"></div><ul class="share-buttons">';
        shareBtns += '<li><a href="https://www.facebook.com/sharer/sharer.php?u=' + link + '&t=' + title + '" title="Share on Facebook" target="_blank"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/Facebook.png"></a></li>';
        shareBtns += '<li><a href="https://twitter.com/intent/tweet?source=' + link + '&text=' + title + '" target="_blank" title="Tweet"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/Twitter.png"></a></li>';
        shareBtns += '<li><a href="https://plus.google.com/share?url=' + link + '" target="_blank" title="Share on Google+"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/GooglePlus.png"></a></li>';
        shareBtns += '<li><a href="https://getpocket.com/save?url=' + link + '&title=' + title + '" target="_blank" title="Add to Pocket"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/Pocket.png"></a></li>';
        shareBtns += '<li><a href="http://www.reddit.com/submit?url=' + link + '&title=' + title + '" target="_blank" title="Submit to Reddit"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/Reddit.png"></a></li>';
        shareBtns += '<li><a href="http://www.linkedin.com/shareArticle?mini=true&url=' + link + '&title=' + title + '&summary=&source=" target="_blank" title="Share on LinkedIn"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/LinkedIn.png"></a></li>';
        shareBtns += '<li><a href="https://pinboard.in/popup_login/?url=' + link + '&title=' + title + '&description=" target="_blank" title="Save to Pinboard"><img src="' + openWSE.siteRoot() + 'Apps/RSSFeed/Images/ShareIcons/Pinboard.png"></a></li>';
        shareBtns += '</ul><div class="clear"></div>';
        return shareBtns;
    }

    return {
        StartRSSFeederOverlay: StartRSSFeederOverlay,
        GetRSSFeeds_Overlay: GetRSSFeeds_Overlay,
        LoadingRSSFeed: LoadingRSSFeed,
        HideRSSLoading: HideRSSLoading
    };

}();

$(document).ready(function () {
    RSSFeedApp_Overlay.StartRSSFeederOverlay();
});

Sys.Application.add_load(function () {
    RSSFeedApp_Overlay.StartRSSFeederOverlay();
});
