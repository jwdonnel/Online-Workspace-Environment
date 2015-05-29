<%@ page title="App Packages" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_AppPackages, App_Web_nh1ggown" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .app-span-modify
        {
            color: #555 !important;
        }

        .app-icon-admin
        {
            margin-left: 0px !important;
            margin-right: 0px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:UpdatePanel ID="updatepnl_refresh" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_edit" runat="server" OnValueChanged="hf_edit_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_delete" runat="server" OnValueChanged="hf_delete_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_addapp" runat="server" OnValueChanged="hf_addapp_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_removeapp" runat="server" OnValueChanged="hf_removeapp_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_currPackage" runat="server" Value="" ClientIDMode="Static" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box no-border" style="padding-bottom: 0px!important;">
                    <div class="td-settings-ctrl">
                        <a href="#" class="margin-right-big input-buttons-create" onclick="openWSE.LoadModalWindow(true, 'NewPackage-element', 'Create New App Package');$('#MainContent_tb_packagename').focus();return false;">Create Package</a>
                        <div class="float-right pad-top">
                            <b class="pad-right">Package Count</b><asp:Label ID="lbl_packagecount"
                                runat="server" Text="0"></asp:Label>
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        These app packages are used for the app installer
                within the user accounts setup.
                    </div>
                </div>
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
                <div class="clear-space">
                </div>
                <div class="clear-space">
                </div>
                <div class="searchwrapper" style="width: 350px;">
                    <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Packages')this.value=''" onblur="if(this.value=='')this.value='Search Packages'"
                            Text="Search Packages"></asp:TextBox>
                        <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                            OnClick="imgbtn_clearsearch_Click" />
                        <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                            OnClick="imgbtn_search_Click" />
                    </asp:Panel>
                </div>
                <asp:Panel ID="pnl_packageholder" runat="server">
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
            </Triggers>
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
                                        <asp:Panel ID="pnl_edit" runat="server" DefaultButton="btn_edit_name">
                                            <div class="float-left pad-right pad-top">
                                                <b>Edit Name</b>
                                            </div>
                                            <asp:Panel ID="pnl_editName" runat="server" DefaultButton="btn_edit_name">
                                                <asp:TextBox ID="tb_edit_name" runat="server" CssClass="textEntry" MaxLength="100"></asp:TextBox>
                                                <asp:Button ID="btn_edit_name" runat="server" Text="Update Name" CssClass="input-buttons RandomActionBtns margin-left-big"
                                                    OnClick="btn_edit_name_Click" CausesValidation="false" />
                                                <div class="float-right">
                                                    <asp:Label ID="lbl_error_edit" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                                </div>
                                            </asp:Panel>
                                            <asp:HiddenField ID="hf_edit_name" runat="server" />
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear-space-two">
                                </div>
                                <div class="clear-margin">
                                    Add or remove apps for this package by selecting the plus or minus image next to the app name and icon.
                                <div class="clear-space">
                                </div>
                                </div>
                                <div class="clear-space">
                                </div>
                                <asp:UpdatePanel ID="updatepnl_viewapps" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_w" runat="server">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons" value="Close" onclick="openWSE.LoadModalWindow(false, 'PackageEdit-element', ''); RefreshList();" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="NewPackage-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="535">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewPackage-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div class="clear-margin">
                                    Enter the app package name and assign the apps to this package.
                                    <div class="clear-space">
                                    </div>
                                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                        <ContentTemplate>
                                            <asp:Panel ID="pnl_newPackageName" runat="server" DefaultButton="btn_finish_add">
                                                <asp:TextBox ID="tb_packagename" runat="server" CssClass="textEntry" onfocus="if(this.value=='Package Name')this.value=''"
                                                    onblur="if(this.value=='')this.value='Package Name'" Text="Package Name" MaxLength="100"></asp:TextBox>
                                                <asp:Button ID="btn_finish_add" runat="server" Text="Save Package" CssClass="input-buttons RandomActionBtns margin-left-big"
                                                    OnClick="btn_finish_add_Click" CausesValidation="false" />
                                                <input type="button" value="Cancel" class="input-buttons" onclick="openWSE.LoadModalWindow(false, 'NewPackage-element', ''); $('#MainContent_tb_packagename').val('Package Name'); return false;" />
                                            </asp:Panel>
                                            <div class="clear-space">
                                            </div>
                                            <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div class="clear-margin">
                                        This dialog will close upon save. To add apps to this package, click on the edit button in the new package.
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/packagecategory.js")%>'> </script>
    </div>
</asp:Content>
