<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WeightTotals_Overlay.ascx.cs"
    Inherits="Overlays_WeightTotals_Overlay" ClientIDMode="Static" %>
<div id="WeightTotals_Overlay_Position" runat="server"
    style="width: 250px;">
    <asp:Panel ID="weighttotals_pnl_entries" runat="server" CssClass="overlay-entries weighttotals-workspace-entries" style="overflow: auto;">
    </asp:Panel>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Overlays/WeightTotals/weighttotalsoverlay.css" />
