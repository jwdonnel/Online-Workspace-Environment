<%@ Page Title="Group Organizer" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="GroupOrg.aspx.cs" Inherits="SiteTools_GroupOrg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:UpdatePanel ID="updatepnl_refresh" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hf_edit" runat="server" OnValueChanged="hf_edit_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_delete" runat="server" OnValueChanged="hf_delete_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_reset" runat="server" OnValueChanged="hf_reset_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_viewusers" runat="server" OnValueChanged="hf_viewusers_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_viewusers_Standard" runat="server" OnValueChanged="hf_viewusers_Standard_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_removeuser" runat="server" OnValueChanged="hf_removeuser_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_adduser" runat="server" OnValueChanged="hf_adduser_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_inviteUser" runat="server" OnValueChanged="hf_inviteUser_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_inviteUserList" runat="server" Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_finishInviteUser" runat="server" OnValueChanged="hf_finishInviteUser_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_logoutGroup" runat="server" OnValueChanged="hf_logoutGroup_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_loginGroup" runat="server" OnValueChanged="hf_loginGroup_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_groupNetwork" runat="server" OnValueChanged="hf_groupNetwork_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_groupNetwork_Update" runat="server" OnValueChanged="hf_groupNetwork_Update_ValueChanged"
                Value="" ClientIDMode="Static" />
            <asp:HiddenField ID="hf_groupNetwork_Delete" runat="server" OnValueChanged="hf_groupNetwork_Delete_ValueChanged"
                Value="" ClientIDMode="Static" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Group Organizer
        </div>
        <div class="title-line"></div>
        <div class="td-settings-ctrl">
            <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_addgroupbtn" runat="server">
                        <a class="input-buttons-create" onclick="ResetControls();return false;">Create Group</a>
                    </asp:Panel>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_companyholder" runat="server">
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="td-settings-desc">
            The groups created are used to determine how to pull data from each app. Each user created MUST be associated with a group. When sending a user an invite, that user must have their group notification enabled. Otherwise, you will not see the user in the invite list.<br />
            You can edit the default settings of a group (after you create a group), by clicking the pencel edit button and selecting the Change Default User Settings link in the popup window.
        </div>
    </div>
    <div id="NewEdit-Group-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="580">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewEdit-Group-element', '');return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent" style="height: 300px;">
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="updatepnl_editmode_1" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div class="input-settings-holder">
                                    <span class="font-bold">Group Name</span><div class="clear-space-two"></div>
                                    <asp:TextBox ID="tb_companyname" runat="server" CssClass="textEntry-noWidth" Width="100%" MaxLength="500"></asp:TextBox>
                                    <asp:Label ID="lbl_error" runat="server" Text="" Font-Size="X-Small" ForeColor="Red"></asp:Label>
                                    <asp:Label ID="lbl_tempcompanyname" runat="server" Text="" Visible="false" Enabled="false"></asp:Label>
                                    </div>
                                    <div class="input-settings-holder">
                                    <span class="font-bold">Description</span><div class="clear-space-two"></div>
                                    <asp:TextBox ID="tb_description" runat="server" CssClass="textEntry-noWidth" Width="100%" MaxLength="500"></asp:TextBox>
                                    </div>
                                    <div class="clear-space-five"></div>
                                    <asp:CheckBox ID="cb_isprivate" runat="server" Text="&nbsp;Is Private" CssClass="radiobutton-style" />
                                    <div class="clear-space-two">
                                    </div>
                                    <small>If you have a URL that you would like to use for your logo, enter it in the textbox below.
                                    </small>
                                    <div class="clear-space">
                                    </div>
                                    <div class="input-settings-holder">
                                    <span class="font-bold">Link to image</span><div class="clear-space-two"></div>
                                    <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" onfocus="if(this.value=='Link to image')this.value=''"
                                        onblur="if(this.value=='')this.value='Link to image'" Width="515px"></asp:TextBox>
                                    </div>
                                    <span class="font-bold">Upload a Logo</span>
                                    <div class="clear-space-two">
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <asp:FileUpload ID="fu_image_create" runat="server" />
                            <div class="clear-space">
                            </div>
                            <asp:UpdatePanel ID="updatepnl_editmode_2" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <a href="#" id="group-userdefaults-button" title="Change the user default settings for users that log into this group." class="margin-top margin-bottom"><span class="img-customize float-left pad-right"></span>Change Default User Settings</a>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Image ID="img_logo" runat="server" Style="display: none; max-width: 100%;" />
                                    <div class="clear-space">
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <asp:Button ID="btn_finish_add" runat="server" Text="Save" CssClass="input-buttons modal-ok-btn"
                            OnClick="btn_finish_add_Click" CausesValidation="false" />
                        <input type="button" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'NewEdit-Group-element', '');" value="Close" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="GroupEdit-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'GroupEdit-element', '');RefreshList();return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="updatepnl_viewusers" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_modalTitle" runat="server">
                                    </asp:Panel>
                                    <asp:Panel ID="pnl_users" runat="server">
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Close" onclick="openWSE.LoadModalWindow(false, 'GroupEdit-element', ''); RefreshList();" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="GroupInviteUser-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'GroupInviteUser-element', '');return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <div id="invite-innermodal">
                            </div>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Close" onclick="openWSE.LoadModalWindow(false, 'GroupInviteUser-element', '');" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="GroupNetwork-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="700">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="$('#MainContent_ipMessage').html('');$('#tb_ipAdd').val('');openWSE.LoadModalWindow(false, 'GroupNetwork-element', '');return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="updatepnl_groupNetwork" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_groupNetworkAdd" runat="server">
                                        <asp:Image ID="imgGroupNetwork" runat="server" CssClass="float-left margin-right" Style="max-height: 30px;" />
                                        <asp:Label ID="lblGroupNetworkName" runat="server" Font-Size="Large"></asp:Label>
                                        <div class="clear-space"></div>
                                        Adding an Ip address and activating it will cause any user that is not on that Ip address to be blocked. This will only apply if the user is attempting to login to the group.
                                        <asp:HiddenField ID="hf_addIp" runat="server" OnValueChanged="btn_addIp_Click" ClientIDMode="Static" />
                                        <span id="ipMessage" runat="server"></span>
                                        <div class="clear-space"></div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnl_groupNetwork" runat="server">
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Close" onclick="$('#MainContent_ipMessage').html(''); $('#tb_ipAdd').val(''); openWSE.LoadModalWindow(false, 'GroupNetwork-element', '');" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
