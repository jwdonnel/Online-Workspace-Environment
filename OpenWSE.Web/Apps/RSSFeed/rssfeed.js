// -----------------------------------------------------------------------------------
//
//	feedStation
//	by John Donnelly
//	Last Modification: 12/15/2015
//
//	Licensed under the Creative Commons Attribution 2.5 License - http://creativecommons.org/licenses/by/2.5/
//  	- Free for use in both personal and commercial projects
//		- Attribution requires leaving author name, author link, and the license info intact.
//
// -----------------------------------------------------------------------------------

var currCssMode = "desktop";
var initialDesktopColumns = 0;

/* Variable Assignments */
var feedStation_Config = {
    animationSpeed: 150,
    showShareBtns: true,
    desktopColumns: 5,
    winMinWidth: 920,
    buildDirection: "ltr"
};

var feedStation = function () {
    /* -- Private Variables -- */
    var currViewMode = "";
    var gettingFeeds = false;
    var currentIndex = 0;
    var currColumn = 1;
    var currData = null;
    var scrollPosBeforeClick = 0;
    var feedIdOpen = "";
    var refreshTimerObj = null;
    var scrollThreshold = 300;
    var buildHeightThreshold = 0;


    /* -- Initialization Functions -- */
    function initializeSite() {
        cookieFunctions.get("desktop-columns", function (desktopColumnCountCookie) {
            if (desktopColumnCountCookie && !isNaN(parseInt(desktopColumnCountCookie))) {
                var tempCount = parseInt(desktopColumnCountCookie);
                if (tempCount >= 1 && tempCount <= 10) {
                    feedStation_Config.desktopColumns = tempCount;
                }
            }

            $(document).tooltip({
                disabled: true
            });

            initialDesktopColumns = feedStation_Config.desktopColumns;
            $("#select_desktopColumns").val(feedStation_Config.desktopColumns);

            $(window).resize();
            loadViewMode();

            cookieFunctions.get("rssfeed-tab", function (cookieTab) {
                var tempCurrHash = unescape(window.location.hash.replace(/#/g, ""));
                if (cookieTab !== null && cookieTab !== undefined && tempCurrHash === "") {
                    if ($("#categoryList-selector").find("div[data-value='" + cookieTab + "']").length === 0) {
                        cookieTab = "Highlights";
                    }

                    tryUpdateUrl(escape(cookieTab));
                }
                else {
                    if ($("#categoryList-selector").find("div[data-value='" + tempCurrHash + "']").length === 0) {
                        tryUpdateUrl("Highlights");
                    }
                    else {
                        hasUrlChange();
                    }
                }

                initializeScroll();
                setRefreshIntervalDropDown();
                setRefreshInterval();

                if (getParameterByName_NoHash("externalview") === "true") {
                    $(".top-logo").hide();
                    $("#main-content").css("bottom", "0");
                    $(".top-logo").attr("href", "index.html?externalview=true");
                    var $head = $("head");
                    var $styleCss = $head.find("style[type='text/css']");
                    var cssElements = ".go-to-top,.close-rss-feed{bottom:10px!important}";

                    if ($styleCss.length > 0) {
                        $styleCss.eq($styleCss.length - 1).append(cssElements);
                    }
                    else {
                        $head.append("<style type='text/css'>" + cssElements + "</style>");
                    }
                }

                try {
                    if (self !== top && ($("#hf_appviewstyle").val() === "Style_1" || !$("#hf_appviewstyle").val())) {
                        $("#always-visible").find(".top-logo").addClass("hide-title");
                        $("#rss-sidebar").find(".top-logo-sidebar").addClass("hide-title");
                    }
                    else {
                        $("#powered-by-div").remove();
                        $("#rss-sidebar").find(".rss-sidebar-topbar").after("<div id='powered-by-div'>Powered by <a href='//openwse.com' target='_blank'>OpenWSE</a></div>");
                    }
                }
                catch (evt) { }

                if (!inIframe()) {
                    if ($("#btn_LoginRegister").length > 0) {
                        $("#btn_LoginRegister").show();
                        $("#btn_loginRegister_sidebar").show();
                    }
                    else if ($("#lbtn_signoff").length > 0) {
                        $("#lbtn_signoff").show();
                        $("#lbtn_signoff_sidebar").show();
                    }
                }
            });
        });
    }
    function inIframe() {
        try {
            return window.self !== window.top;
        } catch (e) {
            return true;
        }
    }
    function initializeScroll() {
        $("#main-content").scroll(function () {
            var mainScrollTop = $("#main-content")[0].scrollTop;
            if (mainScrollTop > 0 && feedIdOpen === "") {
                $(".go-to-top").show();
            }
            else {
                $(".go-to-top").hide();
            }

            if ($(".feed-open-cardview").length === 0) {
                var endScrollTop = $("#main-content")[0].scrollHeight - $("#main-content")[0].clientHeight;
                if (mainScrollTop >= endScrollTop - scrollThreshold) {
                    buildFeedContentScrolling();
                }
            }
        });
    }
    function hasUrlChange() {
        var hashCategory = getCurrentHashCategory();

        if (feedIdOpen !== "") {
            if (hashCategory === getCurrentCategory()) {
                finishCloseFeed();
            }
            else {
                feedIdOpen = "";
            }
        }

        var foundHash = false;

        if (currData === null || currData.length === 0 || (hashCategory !== getCurrentCategory()) || (!getOpenFeedQuery() && hashCategory !== getCurrentCategory())) {
            $("#currentSelectedCategory").attr("data-value", hashCategory);
            $("#currentSelectedCategory").html($.trim($("#categoryList-selector div[data-value='" + hashCategory + "']").html()));
            getFeeds();
        }
        else if (getOpenFeedQuery()) {
            finishFeedOpen();
        }
    }
    function tryUpdateUrl(hashVal) {
        if (window.location.hash !== "#" + hashVal) {
            currData = null;
            window.location.hash = hashVal;
        }
    }
    function setRefreshIntervalDropDown() {
        var $this = $("#select_feedinterval");
        cookieFunctions.get("refresh-interval", function (cookieVal) {
            if (cookieVal && $this.length > 0) {
                var intVal = parseInt(cookieVal);
                if (!isNaN(intVal)) {
                    $this.val(cookieVal);
                }
            }
        });
    }
    function setRefreshInterval() {
        var intVal = parseInt($("#select_feedinterval").val());
        if (!isNaN(intVal)) {
            refreshTimerObj = setInterval(function () {
                if ($("#select_feedinterval").val() === "0") {
                    clearInterval(refreshTimerObj);
                }
                else {
                    refreshFeedsFromTimer();
                }
            }, (intVal * 1000) * 60);
        }
    }

    function loadCSS(dir) {
        var fullUrl = "App_Themes/" + dir + "/main.css";
        $("link[href='" + fullUrl + "']").remove();
        var head = document.getElementsByTagName('head')[0];
        link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = fullUrl;
        head.appendChild(link);
    }
    function unloadCSS(dir) {
        var fullUrl = "App_Themes/" + dir + "/main.css";
        $("link[href='" + fullUrl + "']").remove();
    }
    function updateTotalDesktopColumns_Changed(_this) {
        var tempCount = parseInt($(_this).val());
        if (!isNaN(tempCount) && currCssMode != "mobile") {
            if (tempCount >= 1 && tempCount <= 10) {
                cookieFunctions.set("desktop-columns", tempCount, "30", function () {
                    initialDesktopColumns = tempCount;
                    feedStation.updateToMobileMode();
                });
            }
            else {
                $(_this).val(initialDesktopColumns);
            }
        }
    }

    /* -- Jquery Click Events -- */
    $(document.body).on("click", "#close-sidebar-btn", function () {
        closeTopOptions();
    });
    $(document.body).on("click", "#currentSelectedCategory", function () {
        if ($(this).hasClass("active")) {
            closeTopOptions();
        }
        else {
            if ($(".options-overlay").length > 0) {
                $(".options-overlay").remove();
            }

            $(this).addClass("active");
            $("#categoryList-selector").css("min-width", $("#currentSelectedCategory").outerWidth() - 1);
            $("#categoryList-selector").css("max-height", $("#main-content").outerHeight());
            $("#categoryList-selector").slideDown(feedStation_Config.animationSpeed);
            $("body").append("<div class='options-overlay'></div>");
        }
    });
    $(document.body).on("click", "#categoryList-selector div", function () {
        closeTopOptions();

        feedIdOpen = "";

        var searchQuery = getSearchQuery();
        if (searchQuery) {
            searchQuery = "&search=" + escape(searchQuery);
        }

        cookieFunctions.set("rssfeed-tab", $(this).attr("data-value"), "30", function () {
            var hashVal = escape($(this).attr("data-value")) + searchQuery;
            tryUpdateUrl(hashVal);
        }.bind(this));
    });
    $(document.body).on("click", "#sidebar-menu-btn", function () {
        if ($("#sidebar-menu-btn").hasClass("active")) {
            closeTopOptions();
        }
        else {
            if ($(".options-overlay").length > 0) {
                $(".options-overlay").remove();
            }

            $("#sidebar-menu-btn").addClass("active");
            $("#rss-sidebar").addClass("active");
            $("body").append("<div class='options-overlay'></div>");
        }
    });
    $(document.body).on("click", "#main-content, .options-overlay", function (e) {
        closeTopOptions();
    });
    function closeTopOptions() {
        if ($(".options-overlay").length > 0) {
            $(".options-overlay").remove();
        }

        $("#sidebar-menu-btn").removeClass("active");
        $("#rss-sidebar").removeClass("active");
        $("#currentSelectedCategory").removeClass("active");
        $("#categoryList-selector").slideUp(feedStation_Config.animationSpeed);
    }
    function setMaxHeightOfOptions() {
        if ($("#currentSelectedCategory").hasClass("active")) {
            $("#categoryList-selector").css("max-height", $("#main-content").outerHeight());
        }
    }


    /* -- Search Functions -- */
    function keyPressSearch(event) {
        try {
            if (event.which == 13) {
                searchFeeds(event.target);
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                searchFeeds(event.target);
            }
            delete evt;
        }
    }
    function searchFeeds(_this) {
        feedIdOpen = "";
        var feedQuery = getOpenFeedQuery();
        if (feedQuery) {
            feedQuery = "&feed=" + escape(feedQuery);
        }

        var searchVal = $.trim($(_this).parent().find("input").val());
        $("#searchbox-feed-search").val(searchVal);
        $("#searchbox-feed-search-sidebar").val(searchVal);

        var hashVal = escape(getCurrentHashCategory()) + "&search=" + escape(searchVal) + feedQuery;
        tryUpdateUrl(hashVal);

        if ($("#rss-sidebar").outerWidth() === $(window).width()) {
            closeTopOptions();
        }
    }
    function clearSearch() {
        feedIdOpen = "";

        $("#searchbox-feed-search").val("");
        $("#searchbox-feed-search-sidebar").val("");

        var feedQuery = getOpenFeedQuery();
        if (feedQuery) {
            feedQuery = "&feed=" + escape(feedQuery);
        }

        var hashVal = escape(getCurrentHashCategory()) + feedQuery;
        tryUpdateUrl(hashVal);

        if ($("#rss-sidebar").outerWidth() === $(window).width()) {
            closeTopOptions();
        }
    }


    /* -- Desktop and Mobile Functions -- */
    function changeViewMode_Changed(ele) {
        currViewMode = $(ele).val();
        cookieFunctions.set("rssfeed-viewmode", currViewMode, "30", function () {
            updateToMobileMode();
        });
    }
    function updateToMobileMode() {
        if (currCssMode == "mobile") {
            feedStation_Config.desktopColumns = 1;
            buildHeightThreshold = scrollThreshold;
        }
        else {
            feedStation_Config.desktopColumns = initialDesktopColumns;
            if (currViewMode == "card") {
                buildHeightThreshold = 0;
            }
            else {
                buildHeightThreshold = scrollThreshold;
            }
        }

        if (currData) {
            $("#main-content").scrollTop(0);

            currentIndex = 0;
            currColumn = 1;

            $("#feed-holder").html("");
            buildRSSFeeds();

            updateViewMode();

            tempRSSFeedOpen = "";
            finishFeedOpen();
        }
    }


    /* -- Query Functions -- */
    function getParameterByName(e) {
        try {
            e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var t = "[\\?&]" + e + "=([^&#]*)";
            var n = new RegExp(t);
            var r = n.exec(window.location.hash);
            if (r == null) {
                return "";
            }
            else {
                return decodeURIComponent(r[1].replace(/\+/g, " "));
            }
        }
        catch (evt) {
            return "";
        }
    }
    function getSearchQuery() {
        return unescape(getParameterByName("search"));
    }
    function getOpenFeedQuery() {
        return unescape(getParameterByName("feed"));
    }
    function getParameterByName_NoHash(e) {
        try {
            e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var t = "[\\?&]" + e + "=([^&#]*)";
            var n = new RegExp(t);
            var r = n.exec(window.location.search);
            if (r == null) {
                return "";
            }
            else {
                return decodeURIComponent(r[1].replace(/\+/g, " "));
            }
        }
        catch (evt) { }
    }


    /* -- Category Functions -- */
    function getCurrentCategory() {
        var currSelected = $("#currentSelectedCategory").attr("data-value");
        if (!currSelected) {
            currSelected = "Highlights";
        }

        return currSelected;
    }
    function getCurrentHashCategory() {
        var hashCategory = unescape(window.location.hash.replace(/#/g, ""));
        if (hashCategory.indexOf("&search=") !== -1) {
            var splitHash = hashCategory.split("&search=");
            if (splitHash.length >= 2) {
                hashCategory = splitHash[0];
            }
        }
        else if (hashCategory.indexOf("&feed=") !== -1) {
            var splitHash = hashCategory.split("&feed=");
            if (splitHash.length >= 2) {
                hashCategory = splitHash[0];
            }
        }

        if (!hashCategory) {
            hashCategory = "Highlights";
        }

        return hashCategory;
    }


    /* -- View Mode Functions -- */
    function loadViewMode() {
        cookieFunctions.get("rssfeed-viewmode", function (result) {
            currViewMode = result;
            if (currViewMode) {
                $("#select_viewmode").val(currViewMode);
            }
        });
    }
    function updateViewMode() {
        var $feeds = $(".rssFeed").find(".feed-preview");

        $(".close-rss-feed").remove();
        $(".share-buttons").remove();
        $(".rss-img-link-clear").remove();
        $feeds.removeClass("feed-hidden");

        if ($("#main-content").find(".go-to-top").length === 0) {
            $("#main-content").append("<div class='go-to-top' title='Back to top' onclick=\"feedStation.backToTop();\"></div>");
        }

        switch (currViewMode) {
            case "full":
                $feeds.each(function () {
                    setFullViewForFeed(this);
                });

                $(".feed-column").addClass("fullview");

                feedIdOpen = "";
                var searchQuery = getSearchQuery();
                if (searchQuery) {
                    searchQuery = "&search=" + escape(searchQuery);
                }

                window.location.hash = escape(getCurrentHashCategory()) + searchQuery;
                break;

            default:
                $feeds.each(function () {
                    setCardViewForFeed(this);
                });
                $(".feed-column").removeClass("fullview");
                break;
        }

        $("#main-content").scroll();
    }
    function updateViewModeForFeed(ele) {
        switch (currViewMode) {
            case "full":
                setFullViewForFeed(ele);
                break;

            default:
                setCardViewForFeed(ele);
                break;
        }
    }
    function setFullViewForFeed(ele) {
        var $this = $(ele);
        $this.addClass("feed-open");
        $this.css("border-bottom", "1px solid #CCC");

        var description = $this.find(".rss-description").attr("data-description");
        var content = $this.find(".rss-content").attr("data-content");

        if (description) {
            $this.find(".rss-description").html(unescape(description));
        }
        if (content) {
            $this.find(".rss-content").html(unescape(content));
        }

        $this.find(".rss-description").attr("data-description", "");
        $this.find(".rss-content").attr("data-content", "");

        adjustFeedContentParts($this);

        if (feedStation_Config.showShareBtns) {
            if ($this.find(".rss-description").length > 0 && $this.find(".rss-title").attr("href") != "" && $this.find(".rss-title").attr("href") != null) {
                if ($this.find(".share-buttons").length === 0) {
                    var rssShareLink = encodeURIComponent($this.find(".rss-title").attr("href"));
                    var rssShareTitle = encodeURIComponent($.trim($this.find(".rss-title").html()));
                    $this.find(".rss-description").prepend(buildShareButtons(rssShareLink, rssShareTitle));
                }
            }
        }

        correctHrefLinks($this);
        removeUselessTags($this);
    }
    function setCardViewForFeed(ele) {
        var $this = $(ele);
        $this.removeClass("feed-open");
        $this.css("border-bottom", "");

        $this.find(".share-buttons").remove();
        $(".rss-img-link-clear").remove();

        var description = $.trim($this.find(".rss-description").html());
        var content = $.trim($this.find(".rss-content").html());

        if (description) {
            $this.find(".rss-description").attr("data-description", escape(description));
        }
        if (content) {
            $this.find(".rss-content").attr("data-content", escape(content));
        }

        $this.find(".rss-description").html("");
        $this.find(".rss-content").html("");
    }


    /* -- RSS Functions -- */
    function getFeeds() {
        if (!gettingFeeds) {
            gettingFeeds = true;
            var currSelected = getCurrentCategory();

            var searchText = getSearchQuery();
            if (searchText) {
                $("#searchbox-feed-search").val(searchText);
                $("#searchbox-feed-search-sidebar").val(searchText);
                loadingPopup.Message("Searching Feeds...");
            }
            else {
                $("#searchbox-feed-search").val("");
                $("#searchbox-feed-search-sidebar").val("");
                loadingPopup.Message("Loading " + currSelected + "...");
            }

            openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/GetRSSFeedStation", '{ "category": "' + escape(currSelected) + '","search": "' + escape(searchText) + '" }', null, function (data) {
                if (currSelected === "My Alerts") {
                    currData = null;

                    $("#select-viewmode-selector").addClass("rss-myalerts-div-sidebartoggles");
                    $("#desktop-column-selector").addClass("rss-myalerts-div-sidebartoggles");

                    if (data.d) {
                        var x = "";
                        var alertData = $.parseJSON(data.d);
                        for (var i = 0; i < alertData.length; i++) {
                            if (alertData[i]) {
                                x += "<div class='rss-myalerts-div'>" + alertData[i] + "</div>";
                            }
                        }
                    }

                    if ($("#main-content").find(".go-to-top").length === 0) {
                        $("#main-content").append("<div class='go-to-top' title='Back to top' onclick=\"feedStation.backToTop();\"></div>");
                    }

                    var acctLink = "<div class='pad-all'><small>To delete your alerts, go to <a href='../../SiteTools/UserTools/MyNotifications.aspx' target='_blank'>My Notifications</a>.</small></div><div class='clear-space'></div>";

                    if (x) {
                        $("#feed-holder").html(acctLink + x);
                    }
                    else {
                        $("#feed-holder").html(acctLink + "<div class='rssFeed-nonfound'>No feeds founds.</div>");
                    }
                }
                else {
                    $("#select-viewmode-selector").removeClass("rss-myalerts-div-sidebartoggles");
                    $("#desktop-column-selector").removeClass("rss-myalerts-div-sidebartoggles");

                    currentIndex = 0;
                    currColumn = 1;

                    if (data.d) {
                        currData = $.parseJSON(data.d);
                        buildRSSFeeds();
                    }

                    if (getOpenFeedQuery()) {
                        finishFeedOpen();
                    }
                    else {
                        updateViewMode();
                    }
                }

                loadingPopup.RemoveMessage();
                gettingFeeds = false;
            }, function (data) {
                $("#feed-holder").html("<div class='rssfeed-error'>There seems to be an error getting the requested feeds.</div>");
                gettingFeeds = false;
                loadingPopup.RemoveMessage();
            });
        }
    }
    function refreshFeeds() {
        feedIdOpen = "";
        currData = null;
        getFeeds();

        if ($("#rss-sidebar").outerWidth() === $(window).width()) {
            closeTopOptions();
        }
    }
    function refreshFeedsFromTimer() {
        if ($(".feed-open-cardview").length === 0) {
            currData = null;
            getFeeds();
        }
    }
    function buildRSSFeeds() {
        var hasContent = false;
        if (currData) {
            if (currData.length > 0) {
                hasContent = true;
                if (currentIndex === 0) {
                    $("#feed-holder").html("<div class='rssFeed'></div>");
                    var columnWidth = 100 / feedStation_Config.desktopColumns;
                    for (var i = 1; i <= feedStation_Config.desktopColumns; i++) {
                        $("#feed-holder").find(".rssFeed").append("<div data-column='" + i + "' class='feed-column' style='width: " + columnWidth + "%;'></div>");
                    }
                    $("#feed-holder").find(".rssFeed").append("<div class='clear'></div>");
                }
            }

            for (var i = 0; i < currData.length; i++) {
                currentIndex = i + 1;
                $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + currColumn + "']").append(buildFeedContent(currData[i]));

                var $ele = $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + currColumn + "']").find(".feed-preview").last();
                $ele.css("background-color", currData[i].BgColor);
                updateViewModeForFeed($ele);
                setFeedContentSize($ele);

                if (feedStation_Config.buildDirection != "ltr") {
                    // Will build the fees from top to bottom in each column.
                    if (!$ele.hasClass("feed-small") && $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + currColumn + "']").outerHeight() >= $("#main-content")[0].clientHeight) {
                        currColumn++;
                    }
                }
                else {
                    currColumn++;
                }

                if (currColumn > feedStation_Config.desktopColumns) {
                    currColumn = 1;
                    if ($("#feed-holder").find(".rssFeed").outerHeight() > $("#main-content").outerHeight()) {
                        break;
                    }
                }
            }

            buildFeedContentScrolling();
        }

        if (!hasContent) {
            $("#feed-holder").html("<div class='rssFeed-nonfound'>No feeds founds.</div>");
        }
    }
    function buildFeedContentScrolling() {
        if (currData && $(".feed-open-cardview").length === 0) {
            for (var i = currentIndex; i < currData.length; i++) {
                currColumn = getColumnNeededToFill();
                if (currColumn === -1) {
                    currColumn = 1;
                    break;
                }

                currentIndex = i + 1;
                $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + currColumn + "']").append(buildFeedContent(currData[i]));

                var $ele = $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + currColumn + "']").find(".feed-preview").last();
                $ele.css("background-color", currData[i].BgColor);
                updateViewModeForFeed($ele);
                setFeedContentSize($ele);
            }
        }
    }
    function getColumnNeededToFill() {
        for (var i = 1; i <= feedStation_Config.desktopColumns; i++) {
            var $ele = $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + i + "']").find(".feed-preview").last();
            if ($ele.hasClass("feed-small") || $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='" + i + "']").outerHeight() < $("#main-content")[0].scrollTop + $("#main-content")[0].clientHeight + buildHeightThreshold) {
                return i;
            }
        }

        return -1;
    }
    function buildFeedContent(data) {
        var feedList = "";
        var title = data.Title;
        var link = data.Link;

        var id = createRSSFeedId(title, link);
        var $this = $(".rssFeed").find(".feed-preview[data-id='" + id + "']");
        if ($this.length > 0) {
            return "";
        }

        var summary = data.Summary;
        var content = data.Content;
        var pubDate = new Date(parseInt(data.PubDate.replace("/Date(", "").replace(")/", "")));
        var creator = "";
        var bgStyleClass = data.BgStyleClass;
        if (bgStyleClass && bgStyleClass.toLowerCase().indexOf("//images/icon-rss.png") != -1) {
            bgStyleClass = "class='feed-preview rss-has-no-image'";
        }

        if (pubDate && pubDate != null) {
            pubDate = pubDate.toLocaleString();
        }

        if (creator != "N/A" && creator != "") {
            creator = " by " + data.Creator;
        }

        feedList += "<div data-id=\"" + id + "\" " + bgStyleClass + " onclick=\"feedStation.openRSSFeedContent(event, this);\"><div class='rss-inner-overlay'></div><div class='rss-inner-padding'>";

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
        if ($("#mySavedLink").length > 0) {
            if (getCurrentCategory() !== "Saved Feeds") {
                feedList += "<div class='rss-save' onclick=\"feedStation.SaveFeed('" + id + "');\"><span class='read-later-image'></span>Read Later</div>";
            }
            else {
                feedList += "<div class='rss-save' onclick=\"feedStation.RemoveSaveFeed('" + id + "');\">Delete</div>";
            }
        }
        feedList += "<div class='rss-clearspace'></div>";
        feedList += "<div class='rss-description' data-description='" + escape(summary) + "'></div>";
        feedList += "<div class='rss-content' data-content='" + escape(content) + "'></div>";
        if (data.SourceImage != "" && data.SourceImage != null && data.SourceImage != "null") {
            feedList += "<div class='rss-article-source'><img alt='' src='" + data.SourceImage + "' /></div>";
        }
        else if (data.Source != "" && data.Source != null) {
            feedList += "<div class='rss-article-source'> - " + data.Source + " - </div>";
        }
        feedList += "</div>";

        if (data.SourceImage != "" && data.SourceImage != null && data.SourceImage != "null") {
            feedList += "<span class='rss-source-preview'><img alt='' src='" + data.SourceImage + "' /></span>";
        }
        else if (data.Source != "" && data.Source != null) {
            feedList += "<span class='rss-source-preview'>" + data.Source + "</span>";
        }

        feedList += "</div>";
        return feedList;
    }
    function correctHrefLinks(ele) {
        $(ele).find("a").each(function () {
            var $this = $(this);
            if ($this.attr("target") != "_blank") {
                $this.attr("target", "_blank");
            }
        });
        $(ele).find("img").each(function () {
            if ($(this).attr("src").indexOf("http://") === 0) {
                $(this).attr("src", $(this).attr("src").substring(5));
            }
            else if ($(this).attr("src").indexOf("https://") === 0) {
                $(this).attr("src", $(this).attr("src").substring(6));
            }
        });
    }
    function removeUselessTags(ele) {
        $(ele).find("img").one("error", function () {
            try {
                if ($(this).parent().prop("tagName").toLowerCase() === "a") {
                    $(this).parent().next(".rss-img-link-clear").remove();

                    var $nextBr = $(this).parent().next("br");
                    while (true) {
                        if ($nextBr.length > 0) {
                            var $tempNextBr = $nextBr.next("br");
                            $nextBr.remove();
                            $nextBr = $tempNextBr;
                        }
                        else {
                            break;
                        }
                    }

                    $(this).parent().remove();
                }
                else {
                    $(this).remove();
                }
            }
            catch (evt) {
                // Do nothing
            }
        });
        $(ele).find("a[rel='nofollow']").each(function () {
            $(this).next(".rss-img-link-clear").remove();

            var $nextBr = $(this).next("br");
            while (true) {
                if ($nextBr.length > 0) {
                    var $tempNextBr = $nextBr.next("br");
                    $nextBr.remove();
                    $nextBr = $tempNextBr;
                }
                else {
                    break;
                }
            }

            $(this).remove();
        });
        $(ele).find("br[clear='all']").each(function () {
            $(this).remove();
        });
    }
    function buildShareButtons(link, title) {
        var shareBtns = '<ul class="share-buttons">';
        shareBtns += '<li><a href="//www.facebook.com/sharer/sharer.php?u=' + link + '&t=' + title + '" title="Share on Facebook" target="_blank"><img src="Images/ShareIcons/Facebook.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//twitter.com/intent/tweet?source=' + link + '&text=' + title + '" target="_blank" title="Tweet"><img src="Images/ShareIcons/Twitter.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//plus.google.com/share?url=' + link + '" target="_blank" title="Share on Google+"><img src="Images/ShareIcons/GooglePlus.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//getpocket.com/save?url=' + link + '&title=' + title + '" target="_blank" title="Add to Pocket"><img src="Images/ShareIcons/Pocket.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//www.reddit.com/submit?url=' + link + '&title=' + title + '" target="_blank" title="Submit to Reddit"><img src="Images/ShareIcons/Reddit.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//www.linkedin.com/shareArticle?mini=true&url=' + link + '&title=' + title + '&summary=&source=" target="_blank" title="Share on LinkedIn"><img src="Images/ShareIcons/LinkedIn.png" class="share-button-img" /></a></li>';
        shareBtns += '<li><a href="//pinboard.in/popup_login/?url=' + link + '&title=' + title + '&description=" target="_blank" title="Save to Pinboard"><img src="Images/ShareIcons/Pinboard.png" class="share-button-img" /></a></li>';
        shareBtns += '</ul>';
        return shareBtns;
    }
    function backToTop() {
        $("#main-content").animate({
            scrollTop: 0
        }, feedStation_Config.animationSpeed * 2);
    }
    function createRSSFeedId(title, link) {
        var id = title + "-" + link;
        return EscapeId(id);
    }
    function EscapeId(id) {
        id = id.replace(/'/g, "");
        id = id.replace(/’/g, "");
        id = id.replace(/"/g, "");
        id = id.replace(/–/g, "-");
        id = id.replace(/,/g, "");
        id = id.replace(/;/g, "");
        id = id.replace(/\+/g, "");
        id = id.replace(/<span style=background-color: #FFDD49>/g, "");
        id = id.replace(/<span style=background-color:#FFDD49>/g, "");
        id = id.replace(/<\/span>/, "");
        id = escape(id);

        return id;
    }


    /* -- Feed Size and Color Adjustment Functions -- */
    function setFeedContentSize(ele) {
        var title = $.trim($(ele).find(".rss-title-preview").text());

        var count = 0;
        var $prevEle = $(ele).prev();
        while (true) {
            if ($prevEle.length > 0 && $prevEle.hasClass("feed-small")) {
                count++;
                $prevEle = $prevEle.prev();
            }
            else {
                break;
            }
        }

        if (count % 2 === 1 || title.length < 40 || (title.length < 80 && $(ele).hasClass("rss-has-no-image"))) {
            $(ele).addClass("feed-small");
        }
        else if (title.length > 60 && $(ele).hasClass("rss-has-image")) {
            $(ele).addClass("feed-large");
        }
    }
    function adjustFeedContentParts(_this) {
        var $this = $(_this);
        $this.find("iframe").each(function () {
            var $iframe = $(this);
            if ($iframe.width() > $this.width()) {
                $iframe.css("width", $this.width() - 50);
                $iframe.css("height", $iframe.height() / 2);
            }
        });
        $this.find(".rss-description").find("img").each(function () {
            if ($(this).parent().prop("tagName") && $(this).parent().prop("tagName").toLowerCase() == "a") {
                $(this).parent().after("<div class='clear rss-img-link-clear'></div>");
            }

            $(this).attr("height", "");
        });
        $this.find(".rss-content").find("img").each(function () {
            if ($(this).parent().prop("tagName") && $(this).parent().prop("tagName").toLowerCase() == "a") {
                $(this).parent().after("<div class='clear rss-img-link-clear'></div>");
            }

            $(this).attr("height", "");
        });
    }


    /* -- RSS Open Functions -- */
    var tempRSSFeedOpen = "";
    function openRSSFeedContent(event, ele) {
        if (currViewMode == "full") {
            return false;
        }

        var $this = $(ele);

        if ((event.target.className.indexOf("rss-title-preview") != -1 || event.target.className.indexOf("rss-inner-padding") != -1 || event.target.className.indexOf("rss-inner-overlay") != -1)) {
            var description = $this.find(".rss-description").attr("data-description");
            var content = $this.find(".rss-content").attr("data-content");

            if (description == "" && content == "") {
                if ($this.find(".rss-title").attr("href") == "") {
                    return false;
                }
            }
            else {
                scrollPosBeforeClick = $("#main-content").scrollTop();
                var newHash = escape(getCurrentHashCategory()) + "&search=" + escape($.trim($("#searchbox-feed-search").val())) + "&feed=" + escape($this.attr("data-id"));
                if (window.location.hash !== "#" + newHash) {
                    window.location.hash = newHash;
                }
                else {
                    finishFeedOpen();
                }
            }
        }
    }
    function finishFeedOpen() {
        var id = unescape(getOpenFeedQuery());
        if (!id) {
            return false;
        }

        feedIdOpen = EscapeId(id);

        var $this = $(".rssFeed").find(".feed-preview[data-id='" + feedIdOpen + "']");
        if ($this.length > 0) {
            $(".rssFeed").find(".feed-preview").removeClass("feed-open");
            $(".rssFeed").find(".feed-preview").removeClass("feed-open-cardview");
            $(".rssFeed").find(".feed-preview").removeClass("feed-hidden");
            $(".rssFeed").find(".feed-column").removeClass("is-open");

            $(".close-rss-feed").remove();
            $(".share-buttons").remove();
            $(".rss-img-link-clear").remove();

            $(".rssFeed").find(".feed-preview").addClass("feed-hidden");
            $(".rssFeed").find(".feed-column").addClass("is-open");
            $this.addClass("feed-open");
            $this.addClass("feed-open-cardview");

            var description = $this.find(".rss-description").attr("data-description");
            var content = $this.find(".rss-content").attr("data-content");

            if (description) {
                $this.find(".rss-description").html(unescape(description));
            }
            if (content) {
                $this.find(".rss-content").html(unescape(content));
            }

            $this.find(".rss-description").attr("data-description", "");
            $this.find(".rss-content").attr("data-content", "");

            adjustFeedContentParts($this);

            if (feedStation_Config.showShareBtns) {
                if ($this.find(".rss-description").length > 0 && $this.find(".rss-title").attr("href") != "" && $this.find(".rss-title").attr("href") != null) {
                    if ($this.find(".share-buttons").length === 0) {
                        var rssShareLink = encodeURIComponent($this.find(".rss-title").attr("href"));
                        var rssShareTitle = encodeURIComponent($.trim($this.find(".rss-title").html()));
                        $this.find(".rss-description").prepend(buildShareButtons(rssShareLink, rssShareTitle));
                    }
                }
            }

            $("#main-content").append("<div class='close-rss-feed' onclick=\"feedStation.closeRSSFeedContent();\"></div>");
            $("#main-content").scrollTop(0);

            correctHrefLinks($this);
            removeUselessTags($this);
        }
        else if (currData && tempRSSFeedOpen === "" && currViewMode !== "full") {
            for (var i = 0; i < currData.length; i++) {
                var id = createRSSFeedId(currData[i].Title, currData[i].Link);

                if (id === feedIdOpen) {
                    tempRSSFeedOpen = id;
                    $("#feed-holder").find(".rssFeed").find(".feed-column[data-column='1']").append(buildFeedContent(currData[i]));
                    finishFeedOpen();
                    break;
                }
            }
        }
    }
    function closeRSSFeedContent() {
        var searchQuery = getSearchQuery();
        if (searchQuery) {
            searchQuery = "&search=" + escape(searchQuery);
        }

        if (tempRSSFeedOpen !== "") {
            var $this = $(".rssFeed").find(".feed-preview[data-id='" + tempRSSFeedOpen + "']");
            if ($this.length > 0) {
                $this.remove();
            }
        }

        window.location.hash = escape(getCurrentHashCategory()) + searchQuery;
    }
    function finishCloseFeed() {
        var $this = $(".rssFeed").find(".feed-preview[data-id='" + feedIdOpen + "']");
        $(".rssFeed").find(".feed-preview").removeClass("feed-open");
        $(".rssFeed").find(".feed-preview").removeClass("feed-hidden");
        $(".rssFeed").find(".feed-column").removeClass("is-open");

        $(".close-rss-feed").remove();
        $(".share-buttons").remove();
        $(".rss-img-link-clear").remove();

        if ($this.length > 0) {
            var description = $.trim($this.find(".rss-description").html());
            var content = $.trim($this.find(".rss-content").html());

            if (description) {
                $this.find(".rss-description").attr("data-description", escape(description));
            }
            if (content) {
                $this.find(".rss-content").attr("data-content", escape(content));
            }

            $this.find(".rss-description").html("");
            $this.find(".rss-content").html("");
        }

        $(".rssFeed").find(".feed-preview").removeClass("feed-open-cardview");
        $("#main-content").scrollTop(scrollPosBeforeClick);
        feedIdOpen = "";
    }

    function refreshInterval_Changed(ele) {
        var refreshVal = $(ele).val();
        cookieFunctions.set("refresh-interval", refreshVal, "30", function () {
            clearInterval(refreshTimerObj);
            setRefreshInterval();
        });
    }

    function editFeeds() {
        closeTopOptions();
        loadingPopup.Message("Loading...");
        var x = "<div class='add-item-list-holder'>";
        x += "<div class='add-item-list-header' onclick=\"feedStation.EditFeedHeaderClick(this);\" data-myfeeds='true'>My Feeds</div><div id='myfeedsholder'>";
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/GetUserFeeds", '{ }', null, function (data) {
            var count = 0;
            loadingPopup.RemoveMessage();
            if (data.d[0].length > 0) {
                for (var i = 0; i < data.d[0].length; i++) {
                    if (openWSE.ConvertBitToBoolean(data.d[2][i])) {
                        var title = data.d[0][i];
                        var url = data.d[1][i];
                        var id = "";
                        if (data.d[3][i] != "") {
                            id = "id = '" + data.d[3][i] + "'";
                        }
                        x += "<div " + id + " class='add-item-list-item' title='Add " + title + "' onclick=\"feedStation.AddFeed(this, '" + url + "')\">" + title + "</div>";
                        count++;
                    }
                }
            }

            if (count == 0) {
                x += "<h4 id='myfeedsmessage' class='pad-all'>No custom feeds added.</h4>";
            }
            x += "</div>";
            loadStandardRssList(data, x);
        }, function (data) {
            loadingPopup.RemoveMessage();
            x += "<h4 id='myfeedsmessage' class='pad-all' style='color: Red'>Error loading your rss feeds. Please close app and try again.</h4>";
            x += "</div>";
        });
    }
    function loadStandardRssList(data, x) {
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeeds.xml", null, {
            type: "GET",
            dataType: "xml",
            cache: false
        }, null, null, function (xml) {
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
                            x += "<div class='add-item-list-header' onclick=\"feedStation.EditFeedHeaderClick(this);\">" + category + "</div><div class='add-item-list-contents' data-feedcategory='" + category + "' style='display: block;'></div>";
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
                    $(".add-item-list-contents[data-feedcategory='" + category + "']").append("<div id='" + id + "' class='add-item-list-item' title='Add " + title + "' onclick=\"feedStation.AddFeed(this, '" + url + "')\">" + title + "</div>");
                });
            });

            if (data != null) {
                for (var i = 0; i < data.d[0].length; i++) {
                    $(".add-item-list-item").each(function () {
                        if ($(this).attr("id") == data.d[3][i]) {
                            $(this).addClass("add-item-list-item-hasitem");
                            $(this).attr("title", "Remove " + $(this).html());
                        }
                    });
                }
            }

            if ($("#RSS-Feed-Selector-element").css("display") != "block") {
                openWSE.LoadModalWindow(true, 'RSS-Feed-Selector-element', 'Edit Feeds');
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
            $listItem = $(".add-item-list-contents[data-feedcategory='" + category + "']");
        }

        if ($listItem.length > 0) {
            if ($listItem.css("display") == "block") {
                $listItem.hide();
            }
            else {
                $listItem.show();
            }
        }
    }
    function AddFeed(_this, url) {
        var title = $(_this).html();
        var needAdd = "true";
        if ($(_this).hasClass("add-item-list-item-hasitem")) {
            needAdd = "false";
            if ($(_this).parent().attr("id") == "myfeedsholder") {
                $(_this).remove();
                if ($.trim($("#myfeedsholder").html()) == "") {
                    $("#myfeedsholder").html("<h4 id='myfeedsmessage' class='pad-all'>No custom feeds added.</h4>");
                }
            }
            else {
                $(_this).removeClass("add-item-list-item-hasitem");
            }
        }
        else {
            $(_this).addClass("add-item-list-item-hasitem");
        }

        var rssid = $(_this).attr("id");

        loadingPopup.Message("Updating RSS Feed...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/AddRemoveFeed", '{ "_title": "' + escape(title) + '","_url": "' + escape(url) + '","_rssid": "' + rssid + '","_needAdd": "' + needAdd + '" }', null, null, null, function (data) {
            loadingPopup.RemoveMessage();
            refreshFeeds();
        });
    }
    function AddCustomRSSUrl() {
        var url = $("#tb_addcustomrss").val();
        if (url != "") {
            loadingPopup.Message("Adding RSS Feed...");
            openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/AddCustomFeed", '{ "_url": "' + escape(url) + '" }', null, function (data) {
                var title = data.d[0];
                var id = data.d[1];
                loadingPopup.RemoveMessage();
                if ((title != "error") && (id != "")) {
                    $("#myfeedsmessage").remove();
                    $("#myfeedsholder").prepend("<div id='" + id + "' class='add-item-list-item add-item-list-item-hasitem' title='Add " + title + "' onclick=\"feedStation.AddFeed(this, '" + url + "')\">" + title + "</div>");
                    $("#rssadderror").html("<div class='clear-space'></div><h4 style='color: Green;'>" + title + " has been added successfully.</h4>");
                    $("#tb_addcustomrss").val("");
                    refreshFeeds();
                }
                else {
                    $("#rssadderror").html("<div class='clear-space'></div><h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                }
                setTimeout(function () {
                    $("#rssadderror").html("");
                }, 3000);
            }, function (data) {
                loadingPopup.RemoveMessage();
                $("#rssadderror").html("<div class='clear-space'></div><h4 style='color: Red;'>Could not add feed. Check to make sure url points to RSS feed.</h4>");
                setTimeout(function () {
                    $("#rssadderror").html("");
                }, 3000);
            });
        }
    }


    function addAlert() {
        var newVal = $.trim($("#tb_alertKeyword").val());
        if (newVal) {
            loadingPopup.Message("Loading...");
            openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/AddFeedAlert", '{ "keyword" : "' + escape(newVal) + '" }', null, function (data) {
                loadingPopup.RemoveMessage();
                $("#tb_alertKeyword").val("");
                loadAlertList(data);
            }, function (data) {
                loadingPopup.RemoveMessage();
            });
        }
    }
    function addAlert_KeyPress(event) {
        try {
            if (event.which == 13) {
                addAlert();
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                addAlert();
            }
            delete evt;
        }
    }
    function editAlerts() {
        closeTopOptions();
        loadingPopup.Message("Loading...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/GetUserFeedAlerts", '{ }', null, function (data) {
            loadingPopup.RemoveMessage();
            loadAlertList(data);
        }, function (data) {
            loadingPopup.RemoveMessage();
        });
    }
    function editKeyword(id) {
        $(".rssfeed-alertlist[data-id='" + id + "']").find(".alert-view").hide();
        $(".rssfeed-alertlist[data-id='" + id + "']").find(".alert-edit").show();
    }
    function cancelKeyword(id) {
        $(".rssfeed-alertlist[data-id='" + id + "']").find(".alert-edit").hide();
        $(".rssfeed-alertlist[data-id='" + id + "']").find(".alert-view").show();
    }
    function deleteKeyword(id) {
        if (id) {
            openWSE.ConfirmWindow("Are you sure you want to delete this alert?",
                    function () {
                        loadingPopup.Message("Deleting...");
                        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/DeleteFeedAlert", '{ "id": "' + id + '" }', null, function (data) {
                            loadingPopup.RemoveMessage();
                            loadAlertList(data);
                        }, function (err) {
                            loadingPopup.RemoveMessage();
                        });
                    }, null);
        }
    }
    function updateKeyword(id) {
        var updatedVal = $.trim($(".rssfeed-alertlist[data-id='" + id + "']").find(".alert-edit-textbox").val());
        if (updatedVal) {
            loadingPopup.Message("Updating...");
            openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/UpdateFeedAlert", '{ "id": "' + id + '", "keyword": "' + escape(updatedVal) + '" }', null, function (data) {
                loadingPopup.RemoveMessage();
                loadAlertList(data);
            }, function (err) {
                loadingPopup.RemoveMessage();
            });
        }
    }
    function updateKeyword_KeyPress(event, id) {
        try {
            if (event.which == 13) {
                updateKeyword(id);
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                updateKeyword(id);
            }
            delete evt;
        }
    }
    function loadAlertList(data) {
        var x = "";
        var dataList = new Array();
        if (data.d) {
            dataList = JSON.parse(data.d);
        }

        if (dataList.length > 0) {
            for (var i = 0; i < dataList.length; i++) {
                var editBtn = "<a href='#' class='td-btns td-edit-btn' onclick=\"feedStation.editKeyword('" + dataList[i].Key + "');return false;\" title='Edit'></a>";
                var deleteBtn = "<a href='#' class='td-btns td-delete-btn' onclick=\"feedStation.deleteKeyword('" + dataList[i].Key + "');return false;\" title='Delete'></a>";
                var viewDiv = "<div class='alert-view' style='display: block;'><input type='text' value='" + dataList[i].Value + "' class='textEntry margin-right' disabled='disabled' />" + editBtn + deleteBtn + "<div class='clear'></div></div>";

                var updateBtn = "<a href='#' class='td-btns td-update-btn' onclick=\"feedStation.updateKeyword('" + dataList[i].Key + "');return false;\" title='Update'></a>";
                var cancelBtn = "<a href='#' class='td-btns td-cancel-btn' onclick=\"feedStation.cancelKeyword('" + dataList[i].Key + "');return false;\" title='Cancel'></a>";
                var editDiv = "<div class='alert-edit' style='display: none;'><input type='text' value='" + dataList[i].Value + "' onkeypress=\"feedStation.updateKeyword_KeyPress(event, '" + dataList[i].Key + "');\" class='textEntry margin-right alert-edit-textbox' />" + updateBtn + cancelBtn + "<div class='clear'></div></div>";

                x += "<div data-id='" + dataList[i].Key + "' class='rssfeed-alertlist'>" + viewDiv + editDiv + "</div>";
            }
        }
        else {
            x += "<h4 class='pad-top-big pad-bottom-big'>No alerts added.</h4>";
        }

        $("#rssKeywords").html(x + "<div class='clear'></div>");
        openWSE.LoadModalWindow(true, 'RSS-Feed-Alerts-element', 'Feed Alerts');
    }


    function Admin_GrabLatestFeeds() {
        loadingPopup.Message("Getting Latest...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Update_LoadedFeedList", '{ }', null, function (data) {
            loadingPopup.RemoveMessage();
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("An error occurred. Please try again.");
        });
    }
    function Admin_ClearFeedList() {
        loadingPopup.Message("Clearing List...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Clear_LoadedFeedList", '{ }', null, function (data) {
            loadingPopup.RemoveMessage();
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("An error occurred. Please try again.");
        });
    }
    function Admin_LoadStoredFeedList() {
        loadingPopup.Message("Loading From File...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Load_LoadedFeedList_FromFile", '{ }', null, function (data) {
            if (data.d != "") {
                openWSE.AlertWindow(data.d);
            }
            loadingPopup.RemoveMessage();
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("An error occurred. Please try again.");
        });
    }
    function Admin_GetLoadStoredFeedListCount() {
        loadingPopup.Message("Getting Count...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/RSSFeeds_Get_LoadedFeedList_Count", '{ }', null, function (data) {
            if (data.d != "") {
                openWSE.AlertWindow(data.d);
            }
            loadingPopup.RemoveMessage();
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("An error occurred. Please try again.");
        });
    }


    function SaveFeed(id) {
        loadingPopup.Message("Saving...");
        var currSelected = getCurrentCategory();
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/SaveFeed", '{ "category": "' + escape(currSelected) + '","id": "' + escape(id) + '" }', null, function (data) {
            var messageText = "Feed saved";
            $(".feed-saved-dialog").remove();
            if (data.d) {
                messageText = data.d;
            }

            if (messageText !== "Failed to save feed") {
                $("body").append("<div class='feed-saved-dialog'>" + messageText + "</div>");
                setTimeout(function () {
                    if ($(".feed-saved-dialog").length > 0) {
                        $(".feed-saved-dialog").fadeOut(feedStation_Config.animationSpeed, function () {
                            $(".feed-saved-dialog").remove();
                        });
                    }
                }, 3000);
            }
            else {
                openWSE.AlertWindow(messageText);
            }

            loadingPopup.RemoveMessage();
        });
    }
    function RemoveSaveFeed(id) {
        feedStation.closeRSSFeedContent();
        loadingPopup.Message("Deleting...");
        openWSE.AjaxCall("Apps/RSSFeed/RSSFeed.asmx/RemoveSavedFeed", '{ "id": "' + escape(id) + '" }', null, function (data) {
            loadingPopup.RemoveMessage();
            refreshFeeds();
        });
    }

    function LoginRegisterModal() {
        closeTopOptions();
        openWSE.LoadModalWindow(true, 'pnl_LoginRegister', $.trim($("#btn_LoginRegister").html()));
    }

    return {
        initializeSite: initializeSite,
        hasUrlChange: hasUrlChange,
        setMaxHeightOfOptions: setMaxHeightOfOptions,
        keyPressSearch: keyPressSearch,
        searchFeeds: searchFeeds,
        clearSearch: clearSearch,
        changeViewMode_Changed: changeViewMode_Changed,
        refreshFeeds: refreshFeeds,
        buildFeedContentScrolling: buildFeedContentScrolling,
        backToTop: backToTop,
        openRSSFeedContent: openRSSFeedContent,
        closeRSSFeedContent: closeRSSFeedContent,
        refreshInterval_Changed: refreshInterval_Changed,
        updateToMobileMode: updateToMobileMode,
        updateTotalDesktopColumns_Changed: updateTotalDesktopColumns_Changed,
        editFeeds: editFeeds,
        addAlert: addAlert,
        addAlert_KeyPress: addAlert_KeyPress,
        editAlerts: editAlerts,
        editKeyword: editKeyword,
        cancelKeyword: cancelKeyword,
        deleteKeyword: deleteKeyword,
        updateKeyword: updateKeyword,
        updateKeyword_KeyPress: updateKeyword_KeyPress,
        EditFeedHeaderClick: EditFeedHeaderClick,
        AddFeed: AddFeed,
        AddCustomRSSUrl: AddCustomRSSUrl,
        Admin_GrabLatestFeeds: Admin_GrabLatestFeeds,
        Admin_ClearFeedList: Admin_ClearFeedList,
        Admin_LoadStoredFeedList: Admin_LoadStoredFeedList,
        Admin_GetLoadStoredFeedListCount: Admin_GetLoadStoredFeedListCount,
        SaveFeed: SaveFeed,
        RemoveSaveFeed: RemoveSaveFeed,
        LoginRegisterModal: LoginRegisterModal
    }
}();

$(window).resize(function () {
    if ($(".ui-autocomplete").length > 0) {
        $(".ui-autocomplete").hide();
    }

    // Set mobile or desktop view
    var currWidth = $(window).width();
    if (currWidth <= feedStation_Config.winMinWidth) {
        if (currCssMode != "mobile") {
            currCssMode = "mobile";
            feedStation.updateToMobileMode();
        }
    }
    else {
        if (currCssMode != "desktop") {
            currCssMode = "desktop";
            feedStation.updateToMobileMode();
        }
    }

    feedStation.setMaxHeightOfOptions();
    feedStation.buildFeedContentScrolling();
});

$(document).ready(function () {
    feedStation.initializeSite();
});

$(function () {
    $(window).hashchange(function () {
        feedStation.hasUrlChange();
    });
});
