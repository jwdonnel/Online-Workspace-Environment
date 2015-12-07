<%@ control language="C#" autoeventwireup="true" inherits="Apps_RSSFeed_RSSFeed, App_Web_5bdcjnne" %>
<div class="pad-all app-title-bg-color" style="display: none; position: relative;">
    <div class="float-left pad-bottom">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
    </div>
    <div class="clear"></div>
</div>
<div id="rssfeed_overflow">
    <div class="pad-all rss-menu-bar" style="border-bottom: 1px solid #DDD; background: #F5F5F5;">
        <div class="img-appmenu app-menu-btn" onclick="RSSFeedApp.MenuClick();" title="View Menu"></div>
        <a href="#refresh" class="img-refresh margin-left float-right" onclick="RSSFeedApp.GetRSSFeeds(true);return false;" title="Refresh Feeds"></a>
        <div class="clear"></div>
    </div>
    <div class="clear"></div>
    <div class="loading-message"></div>
    <div id="rssfeed_Holder">
    </div>
</div>
<div id="rssfeed-sidebar-menu" class="customtable-sidebar-menu">
    <div class="pad-all customtable-sidebar-innercontent">
        <div class="img-close-dark app-menu-btn" onclick="RSSFeedApp.MenuClick();" title="Close Menu"></div>
        <a href="#" class="float-right img-refresh" onclick="RSSFeedApp.GetRSSFeeds(true);return false;" title="Refresh Feeds"></a>
        <div class="clear-space"></div>
        <div class="pad-top-big pad-bottom">
            <div class="searchwrapper" style="width: 100%;">
                <input id="tb_search_rssfeed" type="text" class="searchbox" onfocus="if(this.value=='Search current feeds')this.value=''"
                    onblur="if(this.value=='')this.value='Search current feeds'" onkeypress="RSSFeedApp.KeyPressSearch(event)"
                    value="Search current feeds" />
                <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_rssfeed').val('Search current feeds');RSSFeedApp.MenuClick();RSSFeedApp.GetRSSFeeds(true);return false;"></a><a href="#" class="searchbox_submit" onclick="RSSFeedApp.MenuClick();RSSFeedApp.GetRSSFeeds(true);return false;"></a>
            </div>
        </div>
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <div id="Saved-RSSFeeds">
            <h3 class="pad-top pad-bottom">Loading Categories...</h3>
        </div>
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <input id="btn_AddRemoveFeeds" runat="server" type="button" class="input-buttons-create" value="Edit Feeds" onclick="RSSFeedApp.BuildADDRSSList()" />
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <span class="pad-right font-bold">Feeds To Pull</span>
        <select id="dd_feedstopull" onchange="RSSFeedApp.MenuClick();RSSFeedApp.GetRSSFeeds(true);">
            <option value="25">25</option>
            <option value="50" selected="selected">50</option>
            <option value="75">75</option>
            <option value="100">100</option>
            <option value="125">125</option>
        </select>
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <span class="pad-right font-bold">View Mode</span>
        <select id="dd_rssviewmode" onchange="RSSFeedApp.UpdateViewMode();RSSFeedApp.MenuClick();">
            <option value="cards">Card Previews</option>
            <option value="full">Full Articles</option>
        </select>
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <input type="checkbox" id="cb_rssfeed_topbarfixedscroll" onchange="RSSFeedApp.SetTopMenuBarToFixedScroll(this);" /><label for="cb_rssfeed_topbarfixedscroll">&nbsp;Always show menu bar on scroll</label>
        <div class="clear-space"></div>
        <div class="clear-space"></div>
        <div class="pad-top-big pad-bottom" id="rss-feed-update-interval-text" style="font-size: 11px;">
        </div>
        <asp:Panel ID="pnl_AdminRSSFeedSettings" runat="server" Visible="false" Enabled="false" CssClass="pad-top-big margin-top-big border-top">
            <h3 class="font-bold">More Settings</h3>
            <div class="clear-space"></div>
            <a href="#" onclick="RSSFeedApp.Admin_GrabLatestFeeds();return false;">Grab the latest feeds</a>
            <div class="clear-space-five"></div>
            <a href="#" onclick="RSSFeedApp.Admin_ClearFeedList();return false;">Clear Loaded Feed List</a>
            <div class="clear-space-five"></div>
            <a href="#" onclick="RSSFeedApp.Admin_LoadStoredFeedList();return false;">Load From Stored File</a>
            <div class="clear-space-five"></div>
            <a href="#" onclick="RSSFeedApp.Admin_GetLoadStoredFeedListCount();return false;">Get Total Loaded Feed Count</a>
            <div class="clear-space"></div>
        </asp:Panel>
    </div>
</div>
<div id="RSS-Feed-Selector-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="620">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'RSS-Feed-Selector-element', '');$('#AddRSSFeedHolder').html('');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <asp:Panel ID="pnl_addcustomerrss" runat="server" DefaultButton="btn_addcustomrss">
                            <input id="tb_addcustomrss" type="text"
                                class="textEntry" onfocus="if(this.value=='Link to RSS feed')this.value=''" onblur="if(this.value=='')this.value='Link to RSS feed'"
                                value="Link to RSS feed" style="width: 90%;" />
                            <div class="clear-space"></div>
                            <asp:Button ID="btn_addcustomrss" runat="server" CssClass="input-buttons"
                                OnClientClick="RSSFeedApp.AddCustomRSSUrl();return false;" Text="Add Custom Feed" />
                        </asp:Panel>
                        <div id="rssadderror" class="clear" style="height: 25px;">
                        </div>
                        <div id="AddRSSFeedHolder">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
