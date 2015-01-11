<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppLog_Overlay.ascx.cs"
    Inherits="Overlays_AppLog_Overlay" ClientIDMode="Static" %>
<div id="AppLog_Overlay_Position" runat="server">
    <asp:UpdatePanel ID="updatepnl_applog_overlay" runat="server">
        <ContentTemplate>
            <div class="clear-space-five">
            </div>
            <asp:LinkButton ID="btn_refresh_applog_overlay" runat="server" Text="Refresh" CssClass="margin-left margin-right RandomActionBtns"
                OnClick="refresh_applog_overlay"></asp:LinkButton>
            <div class="clear-space-two">
            </div>
            <asp:Panel ID="applog_pnl_entries" runat="server" CssClass="overlay-entries" Width="200px">
                <div class="font-color-light-black pad-all">
                    <small><i>If overlay fails to load, refresh the page</i></small></div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btn_refresh_applog_overlay" />
        </Triggers>
    </asp:UpdatePanel>
</div>
