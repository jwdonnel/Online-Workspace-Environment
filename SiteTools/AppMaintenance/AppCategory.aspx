<%@ Page Title="App Categories" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AppCategory.aspx.cs" Inherits="SiteTools_AppCategory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
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
        <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <a href="#" class="margin-right-big input-buttons-create float-left" onclick="openWSE.LoadModalWindow(true, 'NewCategory-element', 'Create New App Category');$('#MainContent_tb_categoryname').focus();return false;">Create Category</a>
                <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
                    <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Categories')this.value=''" onblur="if(this.value=='')this.value='Search Categories'"
                            Text="Search Categories"></asp:TextBox>
                        <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                            OnClick="imgbtn_clearsearch_Click" />
                        <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                            OnClick="imgbtn_search_Click" />
                    </asp:Panel>
                </div>
                <div class="clear-space"></div>
                <div class="float-left margin-right-big">
                    <b class="pad-right">Category Count</b><asp:Label ID="lbl_packagecount"
                        runat="server" Text="0"></asp:Label>
                </div>
                <div class="float-left margin-left-big">
                    <asp:CheckBox ID="cb_ShowUserOverrides" runat="server" Text="&nbsp;View User Apps and Overrides" OnCheckedChanged="cb_ShowUserOverrides_CheckedChanged" AutoPostBack="true" />
                </div>
                <div class="clear"></div>
                <div class="table-settings-box no-border" style="padding-bottom: 0px!important;">
                    <div class="td-settings-ctrl">
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_packageholder" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        Apps can be in as many categories as you want. If you select "View User Apps and Overrides", only apps you have installed will be shown and changes made will only apply to your account.
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="cb_ShowUserOverrides" />
                <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
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
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons no-margin" value="Close" onclick="openWSE.LoadModalWindow(false, 'CategoryEdit-element', ''); RefreshList();" />
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
                                Enter the app category name and assign the apps to this category.
                                    <div class="clear-space">
                                    </div>
                                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_newCategory" runat="server" DefaultButton="btn_finish_add">
                                            <asp:TextBox ID="tb_categoryname" runat="server" CssClass="textEntry" onfocus="if(this.value=='Category Name')this.value=''"
                                                onblur="if(this.value=='')this.value='Category Name'" Text="Category Name" MaxLength="100"></asp:TextBox>
                                        </asp:Panel>
                                        <div class="clear-space">
                                        </div>
                                        <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="" Visible="false"></asp:Label>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_finish_add" />
                                    </Triggers>
                                </asp:UpdatePanel>
                                <div class="clear-space"></div>
                                This dialog will close upon save. To add apps to this category, click on the edit button in the new category.
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_finish_add" runat="server" Text="Create" CssClass="input-buttons RandomActionBtns no-margin float-left"
                                OnClick="btn_finish_add_Click" CausesValidation="false" />
                            <input type="button" value="Cancel" class="input-buttons no-margin float-right" onclick="openWSE.LoadModalWindow(false, 'NewCategory-element', ''); $('#MainContent_tb_categoryname').val('Category Name'); return false;" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
