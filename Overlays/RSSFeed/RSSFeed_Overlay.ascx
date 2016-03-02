<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSSFeed_Overlay.ascx.cs" Inherits="Overlays_RSSFeed_Overlay" ClientIDMode="Static" %>
<div id="RSSFeed_Overlay_Position" runat="server" class="rss-feed-workspace">
    <asp:Panel ID="rssfeeds_pnl_entries" runat="server" CssClass="overlay-entries rss-feed-pnl-entries">
        <div class="pad-all">If overlay fails to load, refresh the page</div>
    </asp:Panel>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Overlays/RSSFeed/rssfeedoverlay.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Overlays/RSSFeed/rssfeedoverlay.js" />
