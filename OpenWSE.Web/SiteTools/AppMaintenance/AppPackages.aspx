<%@ Page Title="App Packages" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AppPackages.aspx.cs" Inherits="SiteTools_AppPackages" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:UpdatePanel ID="updatepnl_refresh" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hf_edit" runat="server" OnValueChanged="hf_edit_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_delete" runat="server" OnValueChanged="hf_delete_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                Value="" ClientIDMode="Static" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            App Packages
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <asp:Panel ID="pnl_packagelist" runat="server" ClientIDMode="Static" CssClass="pnl-section">
        <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <a class="input-buttons-create" onclick="openWSE.LoadModalWindow(true, 'NewPackage-element', 'Create New App Package');$('#MainContent_tb_packagename').focus();return false;">Create Package</a>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_packageholder" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        App packages are used for the app installer within the user accounts setup. They are also used to setup Group logins and Demo users.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="PackageEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'PackageEdit-element', '');RefreshList();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                    <ContentTemplate>
                                        <asp:Label ID="lbl_typeEdit_Name" ClientIDMode="Static" runat="server"></asp:Label>
                                        <div class="clear-space">
                                        </div>
                                        <div class="input-settings-holder">
                                            <span class="font-bold">Package Name</span>
                                            <div class="clear-space-two"></div>
                                            <asp:Panel ID="pnl_edit" runat="server" DefaultButton="btn_edit_name">
                                                <asp:TextBox ID="tb_edit_name" runat="server" CssClass="textEntry" MaxLength="100" Width="100%"></asp:TextBox>
                                                <asp:Label ID="lbl_error_edit" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                                <asp:HiddenField ID="hf_appAssocationList_added" runat="server" ClientIDMode="Static" />
                                                <asp:HiddenField ID="hf_appAssocationList_removed" runat="server" ClientIDMode="Static" />
                                            </asp:Panel>
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear-space">
                                </div>
                                <asp:UpdatePanel ID="updatepnl_viewapps" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_w" runat="server">
                                        </asp:Panel>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_edit_name" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_edit_name" runat="server" Text="Update" CssClass="input-buttons RandomActionBtns modal-ok-btn" OnClick="btn_edit_name_Click" CausesValidation="false" />
                            <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'PackageEdit-element', ''); RefreshList();" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="NewPackage-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewPackage-element', '');RefreshList();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                Enter the app package name and assign the apps to this package.
                            <div class="clear-space">
                            </div>
                                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                    <ContentTemplate>
                                        <div class="input-settings-holder">
                                            <span class="font-bold">Package Name</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_packagename" runat="server" CssClass="textEntry" MaxLength="100" Width="100%"></asp:TextBox>
                                            <div class="clear">
                                            </div>
                                            <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                        </div>
                                        <asp:Panel ID="pnl_appsInPackage" runat="server" ClientIDMode="Static">
                                        </asp:Panel>
                                        <div class="clear">
                                        </div>
                                        <asp:HiddenField ID="hf_newAppAssocationList_Checked" runat="server" ClientIDMode="Static" />
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_finish_add" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_finish_add" runat="server" Text="Create" CssClass="input-buttons RandomActionBtns modal-ok-btn" OnClick="btn_finish_add_Click" CausesValidation="false" />
                            <input type="button" value="Cancel" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'NewPackage-element', ''); RefreshList(); return false;" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl_packagesettings" runat="server" ClientIDMode="Static" CssClass="pnl-section" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_settings" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnl_app_installer" runat="server" DefaultButton="btn_updateinstaller">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            App Installer Package
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:DropDownList ID="dd_appinstaller" runat="server">
                            </asp:DropDownList>
                            <asp:Button ID="btn_updateinstaller" runat="server" Text="Update" CssClass="margin-left input-buttons RandomActionBtns"
                                OnClick="btn_updateinstaller_Click" />
                        </div>
                        <div class="td-settings-desc">
                            Select the package you would like to use for the App Installer.
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_demo_installer" runat="server" DefaultButton="btn_updatedemo">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            App Demo Package
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:DropDownList ID="dd_appdemo" runat="server">
                            </asp:DropDownList>
                            <asp:Button ID="btn_updatedemo" runat="server" Text="Update" CssClass="margin-left input-buttons RandomActionBtns"
                                OnClick="btn_updatedemo_Click" />
                        </div>
                        <div class="td-settings-desc">
                            Select the package you would like to use for the Demo Site and the Non-Authenticated
                        users (The Non-Authenticated has to be enabled by the administrator).
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
