<%@ page title="App Installer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_AppInstaller, App_Web_maxdjl0u" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="table-settings-box">
            <div class="td-settings-title">
                <div id="searchwrapper" style="width: 300px;">
                    <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Apps')this.value=''" onblur="if(this.value=='')this.value='Search Apps'"
                            Text="Search Apps"></asp:TextBox>
                        <a href="#" onclick="$('#MainContent_tb_search').val('Search Apps');return false;"
                            class="searchbox_clear" title="Clear search"></a>
                        <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                            OnClick="imgbtn_search_Click" />
                    </asp:Panel>
                </div>
                <div class="clear-space">
                </div>
                <asp:UpdatePanel ID="updatePnl_AppTotals" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Label ID="lbl_AppsAvailable" runat="server" ClientIDMode="Static"></asp:Label>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="float-right" style="margin-top: -20px;">
                    <b class="pad-right">Category</b>
                    <asp:DropDownList ID="ddl_categories" runat="server" ClientIDMode="Static">
                    </asp:DropDownList>
                </div>
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_AppList" runat="server" ClientIDMode="Static"></asp:Panel>
                        <h3 id="noItemsCategory" class="pad-top-big pad-bottom-big" style="display: none;">No items in category</h3>
                        <div class="clear-space"></div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Select an app above to view details
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
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/appinstaller.js")%>'></script>
</asp:Content>
