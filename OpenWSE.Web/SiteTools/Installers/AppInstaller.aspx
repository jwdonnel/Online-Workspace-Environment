<%@ Page Title="App Installer" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AppInstaller.aspx.cs" Inherits="SiteTools_AppInstaller" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            App Installer
        </div>
        <div class="title-line"></div>
        <div class="td-settings-ctrl">
            <b class="pad-right">Category</b>
            <asp:DropDownList ID="ddl_categories" runat="server" ClientIDMode="Static" AutoPostBack="true" OnSelectedIndexChanged="ddl_categories_Changed">
            </asp:DropDownList>
            <asp:HiddenField ID="hf_refreshAppAbout" runat="server" ClientIDMode="Static" OnValueChanged="hf_refreshAppAbout_ValueChanged" />
            <div class="clear-space"></div>
            <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_AppList" runat="server" ClientIDMode="Static"></asp:Panel>
                    <h3 id="noItemsCategory" class="pad-top-big pad-bottom-big" style="display: none;">No items in category</h3>
                    <div class="clear-space"></div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ddl_categories" />
                    <asp:AsyncPostBackTrigger ControlID="hf_refreshAppAbout" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <div id="aboutApp-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#close" onclick="openWSE.LoadModalWindow(false, 'aboutApp-element', '');return false;" class="ModalExitButton"></a>
                            </div>
                            <span class="img-about float-left margin-right-sml"></span>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <asp:UpdatePanel ID="updatepnl_aboutHolder" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    <asp:Panel ID="pnl_aboutHolder" runat="server">
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <input type="button" value="Close" onclick="openWSE.LoadModalWindow(false, 'aboutApp-element', '');" class="input-buttons no-margin" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <asp:UpdatePanel ID="updatePnl_Postbacks" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hf_ViewDetails" runat="server" ClientIDMode="Static" OnValueChanged="hf_ViewDetails_ValueChanged" />
            <asp:HiddenField ID="hf_UninstallApp" runat="server" ClientIDMode="Static" OnValueChanged="hf_UninstallApp_ValueChanged" />
            <asp:HiddenField ID="hf_InstallApp" runat="server" ClientIDMode="Static" OnValueChanged="hf_InstallApp_ValueChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
