<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSSFeed.ascx.cs" Inherits="Apps_RSSFeed_RSSFeed" %>
<div id="rssfeed-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color" style="min-height: 40px; position: relative;">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <div class="pad-top" align="right">
            <span class="font-bold pad-right">Saved Feeds</span>
            <select id="Saved-RSSFeeds" class="margin-right">
            </select>
        </div>
    </div>
    <div id="rssfeed_overflow" class="pad-all">
        <div class="pad-top-big pad-left pad-right pad-bottom">
            <div id="searchwrapper" style="width: 100%;">
                <input id="tb_search_rssfeed" type="text" class="searchbox" onfocus="if(this.value=='Search feeds')this.value=''"
                    onblur="if(this.value=='')this.value='Search feeds'" onkeypress="KeyPressSearch_rssfeed(event)"
                    value="Search feeds" />
                <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_rssfeed').val('Search feeds');GetRSSHeaders(true);return false;"></a><a href="#" class="searchbox_submit" onclick="GetRSSHeaders(true);return false;"></a>
            </div>
        </div>
        <div class="pad-top pad-bottom-big" style="height: 30px;">
            <a href="#refresh" class="img-refresh margin-right-big float-right" onclick="GetRSSHeaders(true);return false;"
                title="Refresh Feeds" style="margin-top: 10px;"></a>
            <div class="float-right" style="padding-top: 5px;">
                <span class="font-bold pad-right">Feeds to Show</span>
                <select id="RSSFeedsToPull" class="margin-right">
                    <option value="5">5</option>
                    <option value="10">10</option>
                    <option value="15">15</option>
                    <option value="20">20</option>
                    <option value="25">25</option>
                    <option value="30">30</option>
                </select>
            </div>
            <div class="float-left">
                <input id="btn_AddRemoveFeeds" type="button" class="input-buttons margin-left margin-right float-left margin-bottom" value="Add / Remove RSS Feeds"
                    onclick="BuildADDRSSList()" />
                <div class="float-left pad-left">
                    Feeds are automatically updated every 5 minutes.<br />
                    <div style="font-size: 10px;">
                        <i>If feeds do not load, change the Feeds to Show to something smaller.</i>
                    </div>
                </div>
            </div>
        </div>
        <div class="clear">
        </div>
        <div id="rssfeed_Holder">
        </div>
    </div>
</div>
<div id="RSS-Feed-Selector-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'RSS-Feed-Selector-element', '');$('#AddRSSFeedHolder').html('');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    <asp:Panel ID="pnl_addcustomerrss" runat="server" DefaultButton="btn_addcustomrss">
                        <span class="pad-right font-bold">Add Feed</span><input id="tb_addcustomrss" type="text"
                            class="textEntry" onfocus="if(this.value=='Link to RSS feed')this.value=''" onblur="if(this.value=='')this.value='Link to RSS feed'"
                            value="Link to RSS feed" style="width: 400px;" />
                        <asp:Button ID="btn_addcustomrss" runat="server" CssClass="input-buttons margin-left margin-right"
                            OnClientClick="AddCustomRSSUrl();return false;" Text="Add" />
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