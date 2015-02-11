<%@ Page Title="App Categories" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AppCategory.aspx.cs" Inherits="SiteTools_AppCategory" %>

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
    <div class="maincontent-padding pad-top-big margin-top">
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
        <small><b class="pad-right-sml">Note:</b>Apps can be in as many categories as you want. Adding a app
        to a category will remove it from the previous category.</small>
        <div class="clear-space">
        </div>
        <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <a class="sb-links margin-right-big" onclick="openWSE.LoadModalWindow(true, 'NewCategory-element', 'Create New App Category');$('#MainContent_tb_categoryname').focus();return false;">
                    <span class="td-add-btn float-left margin-right-sml" style="padding: 0!important;"></span>Create a Category</a> <b class="pad-right">Category Count</b><asp:Label ID="lbl_packagecount"
                        runat="server" Text="0"></asp:Label>
                <div class="clear-space">
                </div>
                <div class="clear-space">
                </div>
                <asp:Panel ID="pnl_packageholder" runat="server">
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="CategoryEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" style="width: 650px;">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'CategoryEdit-element', '');RefreshList();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_editName" runat="server" DefaultButton="btn_edit_name">
                                        <div class="float-left pad-right pad-top">
                                            <b>Edit Name</b>
                                        </div>
                                        <asp:TextBox ID="tb_edit_name" runat="server" CssClass="textEntry" MaxLength="100"></asp:TextBox>
                                        <asp:Button ID="btn_edit_name" runat="server" Text="Update Name" CssClass="input-buttons RandomActionBtns margin-left-big"
                                            OnClick="btn_edit_name_Click" CausesValidation="false" />
                                        <div class="float-right">
                                            <asp:Label ID="lbl_error_edit" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                        </div>
                                        <asp:HiddenField ID="hf_edit_name" runat="server" />
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <div class="clear-space-two">
                            </div>
                            <div class="clear-margin">
                                <p>
                                    <small>Add or remove apps for this category by selecting the plus or minus image
                                    next to the app name and icon.</small>
                                </p>
                                <div class="clear-space">
                                </div>
                            </div>
                            <div class="clear-space">
                            </div>
                            <asp:UpdatePanel ID="updatepnl_viewapps" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_w" runat="server" CssClass="modal-inner-scroll">
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="NewCategory-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" style="width: 535px;">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalPadContent" style="height: 125px;">
                            <div class="clear-margin">
                                <p>
                                    Enter the app category name and assign the apps to this category.
                                </p>
                                <div class="clear-space">
                                </div>
                                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_newCategory" runat="server" DefaultButton="btn_finish_add">
                                            <asp:TextBox ID="tb_categoryname" runat="server" CssClass="textEntry" onfocus="if(this.value=='Category Name')this.value=''"
                                                onblur="if(this.value=='')this.value='Category Name'" Text="Category Name" MaxLength="100"></asp:TextBox>
                                            <asp:Button ID="btn_finish_add" runat="server" Text="Save Category" CssClass="input-buttons RandomActionBtns margin-left-big"
                                                OnClick="btn_finish_add_Click" CausesValidation="false" />
                                            <asp:Button ID="Button1" runat="server" Text="Cancel" CssClass="input-buttons RandomActionBtns"
                                                OnClick="btn_cancel_add_Click" CausesValidation="false" />
                                        </asp:Panel>
                                        <div class="clear-space">
                                        </div>
                                        <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear-margin">
                                    <small>This dialog will close upon save. To add apps to this category, click on
                                    the edit button in the new category.</small>
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
