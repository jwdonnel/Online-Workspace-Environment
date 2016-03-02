<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalCalendar_Overlay.ascx.cs" Inherits="Overlays_PersonalCalendar_Overlay" ClientIDMode="Static" %>
<div id="PersonalCalendar_Overlay_Position" runat="server" style="min-width: 265px;">
    <asp:Panel ID="PersonalCalendar_pnl_entries" runat="server" CssClass="overlay-entries">
        <h4 class="pad-all">Loading Events. Please Wait...</h4>
    </asp:Panel>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Overlays/PersonalCalendar/personalcalendaroverlay.css" />
