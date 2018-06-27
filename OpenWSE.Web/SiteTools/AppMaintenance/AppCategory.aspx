<%@ Page Title="App Categories" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AppCategory.aspx.cs" Inherits="SiteTools_AppCategory" %>

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
    <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="table-settings-box">
                <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
                    App Categories
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
                    <a class="input-buttons-create" onclick="openWSE.LoadModalWindow(true, 'NewCategory-element', 'Create New App Category');$('#MainContent_tb_categoryname').focus();return false;">Create Category</a>
                    <asp:CheckBox ID="cb_ShowUserOverrides" runat="server" Text="&nbsp;View User Apps and Overrides" OnCheckedChanged="cb_ShowUserOverrides_CheckedChanged" AutoPostBack="true" CssClass="radiobutton-style float-right" />
                    <div class="clear-space"></div>
                    <asp:Panel ID="pnl_categoryholder" runat="server">
                    </asp:Panel>
                </div>
                <div class="td-settings-desc">
                    Apps can be in as many categories as you want. If you select "View User Apps and Overrides", only apps you have installed will be shown and changes made will only apply to your account.
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="cb_ShowUserOverrides" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="CategoryEdit-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'CategoryEdit-element', '');RefreshList();return false;"
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
                                        <span class="font-bold">Category Name</span>
                                        <div class="clear-space-two"></div>
                                        <asp:Panel ID="pnl_editName" runat="server">
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
                        <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'CategoryEdit-element', ''); currPackage = ''; RefreshList();" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="NewCategory-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', '');RefreshList();return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            Enter the app category name and assign the apps to this category.
                            <div class="clear-space">
                            </div>
                            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                <ContentTemplate>
                                    <div class="input-settings-holder">
                                        <span class="font-bold">Category Name</span>
                                        <div class="clear-space-two"></div>
                                        <asp:TextBox ID="tb_categoryname" runat="server" CssClass="textEntry" MaxLength="100" Width="100%"></asp:TextBox>
                                        <div class="clear">
                                        </div>
                                        <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                    </div>
                                    <asp:Panel ID="pnl_appsInCategory" runat="server" ClientIDMode="Static">
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
                        <asp:Button ID="btn_finish_add" runat="server" Text="Create" CssClass="input-buttons RandomActionBtns modal-ok-btn"
                            OnClick="btn_finish_add_Click" CausesValidation="false" />
                        <input type="button" value="Cancel" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', ''); RefreshList(); return false;" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
