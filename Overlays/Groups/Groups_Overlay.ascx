<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Groups_Overlay.ascx.cs"
    Inherits="Overlays_Groups_Overlay" ClientIDMode="Static" %>
<div id="Groups_Overlay_Position" runat="server" style="min-width: 265px;">
    <asp:Panel ID="groups_pnl_entries" runat="server" CssClass="overlay-entries groups-workspace-entries">
    </asp:Panel>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Overlays/Groups/groupoverlay.css" />
