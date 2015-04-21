<%@ page title="App Categories" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_AppCategory, App_Web_wwykq1g1" %>

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
                        <a href="#" class="margin-right-big input-buttons" onclick="openWSE.LoadModalWindow(true, 'NewCategory-element', 'Create New App Category');$('#MainContent_tb_categoryname').focus();return false;">
                            <span class="td-add-btn float-left margin-right-sml" style="padding: 0!important;"></span>Create a Category</a> <b class="pad-right">Category Count</b><asp:Label ID="lbl_packagecount"
                                runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="td-settings-desc">
                        Apps can be in as many categories as you want. Adding a app to a category will remove it from the previous category.
                    </div>
                </div>
                <asp:Panel ID="pnl_packageholder" runat="server">
                </asp:Panel>
            </ContentTemplate>
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
                                        Add or remove apps for this category by selecting the plus or minus image next to the app name and icon.
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
                    </div>
                </div>
            </div>
        </div>
        <div id="NewCategory-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="535">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div class="clear-margin">
                                    Enter the app category name and assign the apps to this category.
                                    <div class="clear-space">
                                    </div>
                                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                        <ContentTemplate>
                                            <asp:Panel ID="pnl_newCategory" runat="server" DefaultButton="btn_finish_add">
                                                <asp:TextBox ID="tb_categoryname" runat="server" CssClass="textEntry" onfocus="if(this.value=='Category Name')this.value=''"
                                                    onblur="if(this.value=='')this.value='Category Name'" Text="Category Name" MaxLength="100"></asp:TextBox>
                                                <asp:Button ID="btn_finish_add" runat="server" Text="Save Category" CssClass="input-buttons RandomActionBtns margin-left-big"
                                                    OnClick="btn_finish_add_Click" CausesValidation="false" />
                                                <input type="button" value="Cancel" class="input-buttons" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', ''); $('#MainContent_tb_categoryname').val('Category Name'); return false;" />
                                            </asp:Panel>
                                            <div class="clear-space">
                                            </div>
                                            <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div class="clear-margin">
                                        This dialog will close upon save. To add apps to this category, click on the edit button in the new category.
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
