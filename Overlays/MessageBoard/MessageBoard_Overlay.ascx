<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MessageBoard_Overlay.ascx.cs"
    Inherits="Overlays_MessageBoard_Overlay" ClientIDMode="Static" %>
<div id="messageboard_overlay_pnl" runat="server" clientidmode="Static" class="message-board-workspace">
    <h4 id="loadmessageboardposts" class="pad-all">
        Loading Message Board Posts...</h4>
    <asp:Panel ID="message_board_pnl_entries" runat="server" CssClass="overlay-entries message-board-pnl-entries">
    </asp:Panel>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Overlays/MessageBoard/messageboardoverlay.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Overlays/MessageBoard/messageboardoverlay.js" />
