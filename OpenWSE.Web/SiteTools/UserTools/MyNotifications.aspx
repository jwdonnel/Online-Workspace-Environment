<%@ Page Title="My Notifications" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="MyNotifications.aspx.cs" Inherits="SiteTools_MyNotifications" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            My Notifications
        </div>
        <div class="title-line"></div>
        <div class="td-settings-ctrl">
            <asp:UpdatePanel ID="updatePnl_Notifications" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hf_deleteNotification" runat="server" ClientIDMode="Static" OnValueChanged="hf_deleteNotification_ValueChanged" />
                    <asp:HiddenField ID="hf_deleteAllNotification" runat="server" ClientIDMode="Static" OnValueChanged="hf_deleteAllNotification_ValueChanged" />
                    <a class="float-right" onclick="deleteAllNotifications();return false;">Delete All Notifications</a>
                    <div class="clear-space"></div>
                    <asp:Panel ID="pnl_myNotificationList" runat="server" ClientIDMode="Static"></asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>
