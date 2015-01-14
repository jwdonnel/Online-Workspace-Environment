<%@ Page Title="Account Settings" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AcctSettings.aspx.cs" Inherits="SiteTools_AcctSettings"
    ClientIDMode="Static" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .ajax__slider_h_rail
        {
            width: 325px !important;
            margin-right: 10px;
        }

        #pnl_images
        {
            min-height: 400px !important;
        }

        .image-selector-acct
        {
            cursor: pointer;
        }

        .groupedit
        {
            margin-right: 30px;
        }

        .adminpageedit
        {
            margin-right: 10px;
            width: 140px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div id="pnl_topbackgroundTitleBar" runat="server" visible="false">
            <div id="app_title_bg_acct" runat="server" class="app-Settings-title-bg-color-main" style="margin-top: -20px; margin-left: -35px; padding-right: 75px;">
                <div class="pad-all">
                    <div class="app-Settings-title-user-info">
                        <div class="float-left">
                            <asp:Label ID="lbl_pageTitle" runat="server" Text="" CssClass="page-title"></asp:Label>
                        </div>
                        <div class="float-right pad-right">
                            <asp:Label ID="lbl_DateUpdated" runat="server" Font-Size="Small" Style="padding-top: 8px"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="clear-margin" style="margin-top: 10px">
                <small><b class="pad-right-sml">Note:</b>Settings will depend on the user Role, apps, and
            groups. Certain features may be visible but not used.<br />
                    For new users defaults, all overlays and notifications will be enabled by default
            (Depending on which apps are installed and user role).</small>
            </div>
        </div>

        <asp:Panel ID="pnl_UserInformation" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3 id="h3_setuserinfo" runat="server">User Information</h3>
            </div>
            <div class="clear">
            </div>
            <div class="actions" style="border-top: 0px solid #DDD !important; margin-top: 0 !important;">
                <asp:Panel ID="pnl_passwordchange" runat="server">
                    <div class="pad-left pad-right-big inline-block float-left">
                        <strong style="color: #555; font-size: 12px">Change Password</strong><br />
                        <small><i>Passwords requires minimum of
                            <%= Membership.MinRequiredPasswordLength %>
                            characters.</i></small>
                        <div id="iframe_changepassword_holder">
                        </div>
                    </div>
                </asp:Panel>
                <asp:UpdatePanel ID="updatepnl_UserInformation1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div id="pnl_usercreds" runat="server" class="inline-block float-left pad-right-big pad-top-sml">
                            <div class="clear" style="height: 34px;">
                            </div>
                            <div class="float-left">
                                <span class="font-color-black font-bold">First Name</span><br />
                                <asp:TextBox ID="tb_firstname_accountsettings" runat="server" CssClass="textEntry"></asp:TextBox>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="float-left">
                                <span class="font-color-black font-bold">Last Name</span><br />
                                <asp:TextBox ID="tb_lastname_accountsettings" runat="server" CssClass="textEntry"></asp:TextBox>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="float-left">
                                <span class="font-color-black font-bold">Email</span><br />
                                <asp:TextBox ID="tb_email" runat="server" CssClass="textEntry"></asp:TextBox>
                                <div class="clear-space-five">
                                </div>
                            </div>
                            <div class="clear-margin">
                                <div class="clear-space">
                                </div>
                                <asp:Button ID="btn_updateinfo_accountsettings" runat="server" Text="Update Name"
                                    CssClass="input-buttons updatesettings" OnClick="btn_updateinfo_Click" />
                            </div>
                        </div>
                        <asp:Panel ID="pnl_isSocialAccount" runat="server" CssClass="float-left pad-left-big pad-top-big margin-top-big" Enabled="false" Visible="false" Width="400px">
                            <h3 class="pad-top-big margin-top">This is a Social Network account which means you will not be able to change your password from this site. You must use the same network sign in every time you wish to access your account.
                            </h3>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btn_updateinfo_accountsettings" />
                        <asp:AsyncPostBackTrigger ControlID="btn_clear_acctImage" />
                    </Triggers>
                </asp:UpdatePanel>
                <div class="clear"></div>
                <asp:Panel ID="pnl_acctImage" runat="server">
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Account Image</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <table>
                                    <tr>
                                        <td>
                                            <asp:Image ID="imgAcctImage" runat="server" ImageUrl="~/Standard_Images/EmptyUserImg.png" CssClass="acct-image float-left margin-right" />
                                        </td>
                                        <td valign="middle">
                                            <div class="float-left pad-all-big margin-left-big">
                                                <asp:FileUpload ID="fileUpload_acctImage" runat="server" />
                                                <div class="clear-space"></div>
                                                <asp:Button ID="btn_fileUpload_acctImage" runat="server" CssClass="input-buttons" Text="Upload" OnClick="btn_fileUpload_acctImage_Clicked" />
                                                <asp:LinkButton ID="btn_clear_acctImage" runat="server" CssClass="RandomActionBtns" Text="Clear" OnClick="btn_clear_acctImage_Clicked" Style="font-size: 11px;" />
                                                <div class="clear-space"></div>
                                                <div class="clear-space"></div>
                                                <small><b class="pad-right-sml">Note:</b>Only .jpg, .png, .gif, and .jpeg file extentions allowed</small>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <div class="clear"></div>
                <asp:UpdatePanel ID="updatepnl_UserInformation2" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div id="pnl_userColor" runat="server">
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="td-settings-title">
                                        <span class="pad-right font-bold font-color-black">User Color</span>
                                    </td>
                                    <td class="td-settings-ctrl">
                                        <div class="float-left">
                                            <asp:TextBox runat="server" ID="txt_userColor" CssClass="textEntry float-left margin-right color"
                                                MaxLength="6" AutoCompleteType="None" Width="75px" />
                                        </div>
                                        <asp:Button ID="btn_updateusercolor" runat="server" CssClass="input-buttons margin-left RandomActionBtns" Text="Update Color" OnClick="btn_updateusercolor_Clicked" Style="margin-top: 2px;" />
                                        <asp:LinkButton ID="btn_resetUserColor" runat="server" CssClass="RandomActionBtns" Text="Clear" OnClick="btn_resetUserColor_Clicked" Style="font-size: 11px;" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="clear"></div>
                        <asp:Panel ID="pnl_adminpages_Holder" runat="server" Enabled="false" Visible="false">
                            <div class="clear-space">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="td-settings-title">
                                        <span class="pad-right font-bold font-color-black">Admin Pages<br />
                                            (Standard Role Only)</span>
                                    </td>
                                    <td class="td-settings-ctrl">
                                        <asp:HiddenField ID="hf_addAdminPage" runat="server" OnValueChanged="hf_addAdminPage_ValueChanged"
                                            Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_removeAdminPage" runat="server" OnValueChanged="hf_removeAdminPage_ValueChanged"
                                            Value="" ClientIDMode="Static" />
                                        <asp:Panel ID="pnl_adminpages" runat="server">
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <asp:Panel ID="pnl_groupEditor" runat="server">
                            <div class="clear-space">
                            </div>
                            <table cellpadding="10" cellspacing="10" style="width: 100%;">
                                <tr>
                                    <td class="td-settings-title">
                                        <span class="pad-right font-bold font-color-black">User Groups Edit</span>
                                    </td>
                                    <td class="td-settings-ctrl">
                                        <asp:HiddenField ID="hf_addGroup" runat="server" OnValueChanged="hf_addGroup_ValueChanged"
                                            Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_removeGroup" runat="server" OnValueChanged="hf_removeGroup_ValueChanged"
                                            Value="" ClientIDMode="Static" />
                                        <asp:Panel ID="pnl_groups" runat="server">
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <asp:Panel ID="pnl_EnableRecieveAll" runat="server" Enabled="false" Visible="false">
                            <div class="clear-space">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="td-settings-title">
                                        <span class="pad-right font-bold font-color-black">Enable Receive All</span>
                                    </td>
                                    <td class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_receiveall_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_receiveall_on_CheckedChanged" AutoPostBack="True" />
                                            <asp:RadioButton ID="rb_receiveall_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_receiveall_off_CheckedChanged" AutoPostBack="True" />
                                        </div>
                                    </td>
                                    <td class="td-settings-desc">
                                        <small>Disable this feature if you dont want emails to be sent to the given user through eRequests/Questions and Comments/Feedback.</small>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <asp:Panel ID="pnl_userRoleAssign" runat="server" Enabled="false" Visible="false">
                            <div class="clear-space"></div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="td-settings-title">
                                        <span class="pad-right font-bold font-color-black">User Role</span>
                                    </td>
                                    <td class="td-settings-ctrl">
                                        <asp:DropDownList ID="dd_roles" runat="server" CssClass="margin-right">
                                        </asp:DropDownList>
                                        <asp:Button ID="btn_roles" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="dd_roles_Changed" Text="Update" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black"></span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:Button ID="btn_markasnewuser" runat="server" CssClass="input-buttons updatesettings margin-top-sml"
                                        OnClick="btn_markasnewuser_Clicked" Text="Mark as new user" />
                                </td>
                            </tr>
                        </table>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="rb_receiveall_on" />
                        <asp:AsyncPostBackTrigger ControlID="rb_receiveall_off" />
                        <asp:AsyncPostBackTrigger ControlID="btn_roles" />
                        <asp:AsyncPostBackTrigger ControlID="btn_WorkspaceMode" />
                        <asp:AsyncPostBackTrigger ControlID="btn_markasnewuser" />
                        <asp:AsyncPostBackTrigger ControlID="btn_updateusercolor" />
                        <asp:AsyncPostBackTrigger ControlID="btn_resetUserColor" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnl_NotificationSettings" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Notification Settings</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_NotificationSettings" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="clear-margin pad-left float-left">
                        <small>Set up your notifications for each app. Notifications can be sent via email if
                checked. Notifications are based on the user role and apps installed.</small>
                    </div>
                    <div class="clear">
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="btn_DisableAll_notification" runat="server" Text="Disable All"
                            ClientIDMode="Static" CssClass="sb-links margin-right-big RandomActionBtns" OnClick="btn_DisableAll_notification_Clicked"></asp:LinkButton>
                        <span class="font-bold pad-right">Notifications Enabled:</span><asp:Label ID="lbl_NotifiEnabled"
                            runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="clear">
                    </div>
                    <asp:Panel ID="pnl_notifications" runat="server">
                    </asp:Panel>
                    <div class="clear" style="height: 20px;">
                    </div>
                    <asp:Panel ID="pnl_clearNoti" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Clear Notifications</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:Button ID="btn_clearnoti" runat="server" CssClass="input-buttons RandomActionBtns"
                                        OnClick="btn_clearnoti_Clicked" Text="Clear My Notifications" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Click the Clear Notifications button to clear out all your current archived and pending notifications.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear" style="height: 20px;">
                        </div>
                    </asp:Panel>
                    <asp:HiddenField ID="hf_updateEnabled_notification" runat="server" ClientIDMode="Static"
                        OnValueChanged="hf_updateEnabled_notification_Changed" />
                    <asp:HiddenField ID="hf_updateDisabled_notification" runat="server" ClientIDMode="Static"
                        OnValueChanged="hf_updateDisabled_notification_Changed" />
                    <asp:HiddenField ID="hf_updateEmail_notification" runat="server" ClientIDMode="Static"
                        OnValueChanged="hf_updateEmail_notification_Changed" />
                    <asp:HiddenField ID="hf_collId_notification" runat="server" ClientIDMode="Static" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btn_DisableAll_notification" />
                    <asp:AsyncPostBackTrigger ControlID="hf_updateEnabled_notification" />
                    <asp:AsyncPostBackTrigger ControlID="hf_updateDisabled_notification" />
                    <asp:AsyncPostBackTrigger ControlID="hf_updateEmail_notification" />
                    <asp:AsyncPostBackTrigger ControlID="btn_clearnoti" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_WorkspaceOverlays" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Workspace Overlays</h3>
            </div>
            <div class="clear-space">
            </div>
            <div class="clear-margin pad-left float-left">
                <small><b class="pad-right-sml">Note:</b>Workspace overlays show limited information. They
            are a non editable type of app.<br />
                    <b class="pad-right">Hint:</b>Position determines which side of the screen the overlay
            will move to.<br />
                    Set the Workspace dropdown to the display the overlay on.<br />
                    When disabling the overlay, it will clear out your current settings for that overlay
            and reset them back to the defaults.</small>
            </div>
            <div class="clear">
            </div>
            <asp:UpdatePanel ID="updatepnl_WorkspaceOverlays" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="float-right">
                        <asp:LinkButton ID="btn_DisableAll_overlay" runat="server" Text="Disable All" ClientIDMode="Static"
                            CssClass="sb-links margin-right-big RandomActionBtns" OnClick="btn_DisableAll_overlay_Clicked"></asp:LinkButton>
                        <span class="font-bold pad-right">Overlays Enabled:</span><asp:Label ID="lbl_overlaysEnabled"
                            runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_overlays" runat="server">
                    </asp:Panel>
                    <div class="clear" style="height: 20px;">
                    </div>
                    <asp:HiddenField ID="hf_updateEnabled_overlay" runat="server" ClientIDMode="Static"
                        OnValueChanged="hf_updateEnabled_overlay_Changed" />
                    <asp:HiddenField ID="hf_updateDisabled_overlay" runat="server" ClientIDMode="Static"
                        OnValueChanged="hf_updateDisabled_overlay_Changed" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btn_DisableAll_overlay" />
                    <asp:AsyncPostBackTrigger ControlID="hf_updateEnabled_overlay" />
                    <asp:AsyncPostBackTrigger ControlID="hf_updateDisabled_overlay" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_BackgroundEditor" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Background Editor</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:Panel ID="pnl_currentbackgroundselector" runat="server">
                <asp:UpdatePanel ID="updatepnl_BackgroundEditor" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="pad-right pad-bottom">
                            <small><b class="pad-right-sml">Note:</b>All backgrounds will repeat on the workspace.
                                Your connection speed will slow down with the larger sized images. Each image has
                                the size details when you hover over them. Solid color backgrounds will be the quickest
                                if you have a slower internet connection.</small>
                        </div>
                        <div class="clear"></div>
                        <table cellpadding="5px" cellspacing="5px">
                            <tr>
                                <td valign="top">
                                    <div id="CurrentBackground">
                                    </div>
                                    <div class="clear-space-two">
                                    </div>
                                    <div class="pad-left">
                                        <a href="#" onclick="BackgroundSelector();return false;" class="sb-links margin-left-sml">Select Background</a>
                                    </div>
                                    <div class="pad-top pad-bottom" style="padding-left: 17px;">
                                        <small>
                                            <asp:LinkButton ID="lb_clearbackground" runat="server" OnClick="lb_clearbackground_Click">Clear Background</asp:LinkButton>
                                        </small>
                                    </div>
                                </td>
                                <td valign="top">
                                    <div class="pad-top-big" style="padding-left: 30px;">
                                        <asp:HiddenField ID="hf_backgroundimg" runat="server" OnValueChanged="hf_backgroundimg_Changed" />
                                        <div class="float-left">
                                            <asp:TextBox runat="server" ID="txt_bgColor" CssClass="textEntry float-left margin-right color"
                                                MaxLength="6" AutoCompleteType="None" Width="75px" />
                                        </div>
                                        <asp:Button ID="btn_updateBGcolor" runat="server" CssClass="input-buttons margin-left RandomActionBtns" Text="Update Color" OnClick="btn_updateBGcolor_Clicked" Style="margin-top: 2px;" />
                                        <asp:LinkButton ID="btn_clearBGcolor" runat="server" CssClass="RandomActionBtns" Text="Clear" OnClick="btn_clearBGcolor_Clicked" Style="font-size: 11px;" />
                                    </div>
                                    <div class="clear-space"></div>
                                    <asp:Panel ID="pnl_backgroundurl" runat="server" DefaultButton="btn_urlupdate" CssClass="pad-left-big">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold font-color-black">Multiple Backgrounds</span>
                                                </td>
                                                <td class="td-settings-ctrl">
                                                    <div class="field switch inline-block">
                                                        <asp:RadioButton ID="rb_enablebackgrounds_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                            OnCheckedChanged="rb_enablebackgrounds_on_CheckedChanged" AutoPostBack="True" />
                                                        <asp:RadioButton ID="rb_enablebackgrounds_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                            OnCheckedChanged="rb_enablebackgrounds_off_CheckedChanged" AutoPostBack="True" />
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_backgroundSelector" runat="server">
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold font-color-black">Select Workspace</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <asp:DropDownList ID="dd_backgroundSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="dd_backgroundSelector_Changed">
                                                        </asp:DropDownList>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div class="clear-space"></div>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <div class="pad-left-big margin-left">
                                        <small>Allows the use of multiple backgrounds. One for each workspace/workspace.</small>
                                        <div class="clear-space">
                                        </div>
                                        <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" onfocus="if(this.value=='Link to image')this.value=''"
                                            onblur="if(this.value=='')this.value='Link to image'" Text="Link to image" Width="355px"></asp:TextBox>
                                        <asp:Button ID="btn_urlupdate" runat="server" Text="Update Url" CssClass="input-buttons margin-left updatesettings"
                                            OnClick="btn_urlupdate_Click" />
                                        <div class="pad-top pad-bottom">
                                            <small>Copy and paste any link that contains an image. </small>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <div class="pad-top pad-left-big">
                            <span class="font-color-black" id="backgroundsaved"></span>
                        </div>
                        <div id="Background-element" class="Modal-element">
                            <div class="Modal-overlay">
                                <div class="Modal-element-align">
                                    <div class="Modal-element-modal" style="width: 600px;">
                                        <div class="ModalHeader">
                                            <div>
                                                <div class="app-head-button-holder-admin">
                                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'Background-element', '');return false;"
                                                        class="ModalExitButton"></a>
                                                </div>
                                                <span class="Modal-title"></span>
                                            </div>
                                        </div>
                                        <div class="ModalPadContent" style="max-height: 550px; overflow: auto;">
                                            <div class="clear-space-five">
                                            </div>
                                            Click on the background that you would like to apply to your workspace.
                                            <div class="clear-space">
                                            </div>
                                            <asp:HiddenField ID="hf_backgroundselector" runat="server" OnValueChanged="hf_backgroundselector_ValueChanged"
                                                Value="" />
                                            <div id="pnl_images" align="center" class="modal-inner-scroll">
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="hf_backgroundimg" />
                        <asp:AsyncPostBackTrigger ControlID="lb_clearbackground" />
                        <asp:AsyncPostBackTrigger ControlID="btn_updateBGcolor" />
                        <asp:AsyncPostBackTrigger ControlID="btn_clearBGcolor" />
                        <asp:AsyncPostBackTrigger ControlID="rb_enablebackgrounds_on" />
                        <asp:AsyncPostBackTrigger ControlID="rb_enablebackgrounds_off" />
                        <asp:AsyncPostBackTrigger ControlID="dd_backgroundSelector" />
                        <asp:AsyncPostBackTrigger ControlID="btn_urlupdate" />
                        <asp:AsyncPostBackTrigger ControlID="hf_backgroundselector" />
                    </Triggers>
                </asp:UpdatePanel>
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnl_TopSideMenuBar" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Top/Side Menu Bar</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_TopSideMenuBar" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Date/Time</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_showdatetime_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_showdatetime_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_showdatetime_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_showdatetime_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Select Hide if you dont want to see the date/time in the top tool bar.</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_loadLinksOnNewPage" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Load links on&nbsp;&nbsp;<br />
                                        new page</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_linksnewpage_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_linksnewpage_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_linksnewpage_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_linksnewpage_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Set this to false if you don't want a new page to load when clicking the links
                                    in the top right hand corner.</small>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_autohidemode" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Auto Hide Mode</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_autohidemode_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_autohidemode_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_autohidemode_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_autohidemode_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Turn on this feature if you want to automatically hide the header and footer bars on the workspace.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_nonadminsettings" runat="server">
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show All Minimized</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_taskbarShowAll_On" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_taskbarShowAll_On_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_taskbarShowAll_Off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_taskbarShowAll_Off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select No to hide any app not on the currently selected workspace. (Set to
                                        Yes by default)</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show Workspace Preview</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showWorkspacePreview_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showWorkspacePreview_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showWorkspacePreview_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showWorkspacePreview_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select Yes to show a preview of the minimized workspace when hovering over in the selector dropdown.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show Minimized Preview</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showPreview_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showPreview_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showPreview_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showPreview_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select Yes to show a preview of the minimized app when hovered over.</small>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_showPreview_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showPreview_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showWorkspacePreview_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showWorkspacePreview_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showdatetime_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showdatetime_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_linksnewpage_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_linksnewpage_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_autohidemode_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_autohidemode_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_taskbarShowAll_On" />
                    <asp:AsyncPostBackTrigger ControlID="rb_taskbarShowAll_Off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_Privacy_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_Privacy_off" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_IconSelector" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Icon Selector</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_IconSelector" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_lockappicons" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Lock App Icons</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_LockAppIcons_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_LockAppIcons_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_LockAppIcons_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_LockAppIcons_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Set to Lock if you want to lock the order of the app icons. This disables
                                        the ability to sort the app icons. (Only on Workspace)</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Group Icons</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_groupicons_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_groupicons_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_groupicons_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_groupicons_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Enabling this will group the icons by category allowing for easier browsing.</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_categoryCount" runat="server" Enabled="false" Visible="false">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Icon Category Count</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showappcategoryCount_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showappcategoryCount_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showappcategoryCount_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showappcategoryCount_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select Hide if you dont want to see the total count per category.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_HideAppIcons" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Hide Icon Image</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_hideAppIcon_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_hideAppIcon_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_hideAppIcon_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_hideAppIcon_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select Yes to hide all icons. This includes taskbar and app header.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_ShowWorkspaceNum" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show Workspace Number&nbsp;&nbsp;<br />
                                        in App Icon</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_ShowWorkspaceNumApp_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_ShowWorkspaceNumApp_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_ShowWorkspaceNumApp_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_ShowWorkspaceNumApp_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select No to hide the workspace number that the app is currently on. The number
                                        can be seen to the right of the app icons.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_LockAppIcons_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_LockAppIcons_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showappcategoryCount_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showappcategoryCount_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_groupicons_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_groupicons_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_hideAppIcon_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_hideAppIcon_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_ShowWorkspaceNumApp_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_ShowWorkspaceNumApp_off" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_SiteCustomizations" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Site Customizations</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_SiteCustomizations" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_demoPackage" runat="server" Enabled="false" Visible="false">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">App Package</span>
                                </td>
                                <td align="left" style="width: 460px;">
                                    <asp:DropDownList ID="dd_appdemo" runat="server">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_updatedemo" runat="server" Text="Update" CssClass="input-buttons margin-left RandomActionBtns"
                                        OnClick="btn_updatedemo_Click" />
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_WorkspaceMode" runat="server" Enabled="false" Visible="false">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Workspace Mode</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:DropDownList ID="ddl_WorkspaceMode" runat="server" CssClass="margin-right">
                                        <asp:ListItem Value="Simple" Text="Page Based"></asp:ListItem>
                                        <asp:ListItem Value="Complex" Text="App Based"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_WorkspaceMode" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_WorkspaceMode_Click" Text="Update" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Switch to Simple mode if you do not like the use of the apps. This will turn your workspace into a more generic looking website.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <table cellpadding="10" cellspacing="10" style="width: 100%;">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold">Animation Speed</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="float-left pad-right pad-top-sml">
                                    <div id="Slider2" class="ajax__slider_h_rail">
                                    </div>
                                </div>
                                <input id="btnUpdateAnimiation" type="button" class="input-buttons margin-left" value="Update" onclick="UpdateAnimationSpeed();" style="display: none;" />
                                <input type="button" class="input-buttons margin-left" value="Reset" onclick="ResetAnimationSpeed();" />
                                <asp:HiddenField ID="hf_AnimationSpeed" runat="server" ClientIDMode="Static" OnValueChanged="hf_AnimationSpeed_Changed" />
                                <div id="currentAnimationSpeed">
                                </div>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_theme" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Account Theme</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:DropDownList ID="dd_theme" runat="server" CssClass="margin-right">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_UpdateTheme" runat="server" OnClick="dd_theme_Changed" Text="Update" CssClass="input-buttons RandomActionBtns" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Change the overall look of the site to fit your taste. Themes apply to the Workspace
                                and App Settings</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Site Tips</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_tooltips_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_tooltips_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_tooltips_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_tooltips_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Select Off if you dont want to see any site tool tips.</small>
                            </td>
                        </tr>
                    </table>
                    <asp:Panel ID="pnl_accountPrivacy" runat="server">
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Make Account Private</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_Privacy_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_Privacy_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_Privacy_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_Privacy_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Turning this on will stop any logging of the user. No users will be able to edit
                                or see your account.<br />
                                        The site administrator is the only one that can see your account, but still cannot
                                edit or alter any setting on it.</small><br />
                                    Click <a href="#learnmore" onclick="LearnMore();return false">HERE</a> to read more
                            about the private account setting.
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <div id="moreInfo-PrivateAccount" class="clear-margin pad-all" style="display: none">
                            <h3 class="float-left font-bold">
                                <u>More about the Private Account Feature</u></h3>
                            <a href="#close" onclick="LearnMore();return false" class="float-right sb-links">Close</a>
                            <div class="clear">
                            </div>
                            The Private Account feature was created to allow users to keep a more private profile.
                            Enabling this will block any log being added to the network log. This will also
                            hide your account in the Site Controls and block any user, including the Site Administrator,
                            from editing your account. To the basic admistrative user, your account will not
                            appear in the Manage Users page, and the Group Organizer page.<br />
                            <br />
                            Keeping the Private Account feature off will allow the site to record any activity
                            that you perform. This will identify bugs quicker and make the necessary fixes to
                            ensure a better experience.
                            <br />
                            <br />
                            Enabling this will not block any feature that you use on this site.
                            <div class="clear" style="height: 25px;">
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_clearproperties" runat="server">
                        <div class="clear-space"></div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Clear Properties&nbsp;&nbsp;<br />
                                        on Log Off</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_clearproperties_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_clearproperties_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_clearproperties_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_clearproperties_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Clear current app properties and ALL cookies created by this site everytime
                                                you Log off. (Set to No by default)</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_clearUserProp" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Clear User App&nbsp;&nbsp;<br />
                                        Properties</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:Button ID="btn_clearapps" runat="server" Text="Clear Properties" OnClick="btn_clearapps_Click"
                                        CssClass="updatesettings input-buttons" CausesValidation="False" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Delete all settings for for your apps (Size, loading, etc...). This will also
                                            delete ALL cookies created by this site<br />
                                        not including the current ASP.Net session.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_presentationMode" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Presentation Mode</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_presentationmode_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_presentationmode_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_presentationmode_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_presentationmode_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Turn on this feature if you want to automatically hide any app, header, footer and background controls.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_ShowAppTitle" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show App Title&nbsp;&nbsp;<br />
                                        in App Header</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showHeader_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showHeader_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showHeader_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showHeader_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Disable this if you do not want to see the app title in the app header.
                                            (Enabled by default)</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_showAppImage" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Show App Image&nbsp;&nbsp;<br />
                                        in App Header</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_AppHeaderIcon_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_AppHeaderIcon_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_AppHeaderIcon_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_AppHeaderIcon_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Disable this if you do not want to see the app image in the app header.
                                            (Enabled by default)</small>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_tooltips_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_tooltips_on" />
                    <asp:AsyncPostBackTrigger ControlID="hf_AnimationSpeed" />
                    <asp:AsyncPostBackTrigger ControlID="btn_UpdateTheme" />
                    <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_on" />
                    <asp:AsyncPostBackTrigger ControlID="btn_clearapps" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showHeader_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showHeader_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_presentationmode_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_presentationmode_off" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_WorkspaceContainer" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Workspace Container</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_WorkspaceContainer" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Total Number&nbsp;&nbsp;<br />
                                    of Workspaces</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <asp:DropDownList ID="ddl_totalWorkspaces" runat="server">
                                </asp:DropDownList>
                                <asp:Button ID="btn_updateTotalWorkspaces" runat="server" Text="Update" CssClass="margin-left input-buttons RandomActionBtns" OnClick="btn_updateTotalWorkspaces_Click" ClientIDMode="Static" />
                            </td>
                            <td class="td-settings-desc">
                                <small>You can select the total number of workspaces to show on your home workspace page.</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Keep Apps in&nbsp;&nbsp;<br />
                                    a container</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_appcontainer_enabled" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_appcontainer_enabled_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_appcontainer_disabled" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_appcontainer_disabled_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Select No if you want to be able to drag app windows outside the workspace.
                                            (Set to Yes by default)</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Snap Apps to Grid</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_snapapp_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_snapapp_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_snapapp_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_snapapp_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Select Yes if you want to be able to snap app windows to the container grid.
                                            (Set to No by default)</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_appGridSize" runat="server" DefaultButton="btn_AppGridSize">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Grid Size</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:TextBox ID="txt_AppGridSize" runat="server" CssClass="textEntry margin-right" MaxLength="3" Width="40px"></asp:TextBox>
                                    <asp:Button ID="btn_AppGridSize" runat="server" CssClass="input-buttons RandomActionBtns" Text="Update" OnClick="btn_AppGridSize_Click" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Set the grid size of the workspace to move the app on. (Value must be greater than 0)</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_autoRotateOnOff" runat="server">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Auto Rotate Workspace</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_enableautorotate_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_enableautorotate_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_enableautorotate_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_enableautorotate_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>This is a good tool to enable if you want to periodically change screens automatically.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlAutoRotateWorkspace" runat="server" DefaultButton="btn_updateintervals_rotate">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Update Upon Rotate</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_updateOnRotate_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_updateOnRotate_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_updateOnRotate_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_updateOnRotate_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Selecting Yes will refresh any control on the current workspace when rotating.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Auto Rotate Interval</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_autorotateinterval" runat="server" CssClass="textEntry" Width="45px"
                                        MaxLength="6"></asp:TextBox><span class="pad-left">seconds(s)</span>
                                    <asp:Button ID="btn_updateintervals_rotate" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                        Text="Update" OnClick="btn_updateintervals_rotate_Click" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>This value will represent the time to spend on each workspace before changing
                                                screens.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Auto Rotate Screens</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:DropDownList ID="ddl_autoRotateNumber" runat="server" CssClass="margin-right"></asp:DropDownList>
                                    <asp:Button ID="btn_screenRotateNumberUpdate" runat="server" Text="Update" OnClick="btn_screenRotateNumberUpdate_Click" CssClass="input-buttons RandomActionBtns" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>Choose the number of screens to rotate from.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_snapapp_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_snapapp_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_appcontainer_enabled" />
                    <asp:AsyncPostBackTrigger ControlID="rb_appcontainer_disabled" />
                    <asp:AsyncPostBackTrigger ControlID="rb_enableautorotate_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_enableautorotate_off" />
                    <asp:AsyncPostBackTrigger ControlID="btn_screenRotateNumberUpdate" />
                    <asp:AsyncPostBackTrigger ControlID="rb_updateOnRotate_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_updateOnRotate_off" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_ChatClient" CssClass="pnl-section" runat="server">
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Chat Client</h3>
            </div>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="updatepnl_ChatClient" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td class="td-settings-title">
                                <span class="pad-right font-bold font-color-black">Chat Client</span>
                            </td>
                            <td class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_chatclient_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_chatclient_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_chatclient_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_chatclient_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </td>
                            <td class="td-settings-desc">
                                <small>Turn Off if you dont want the chat feature. Turning this Off may boost performance
                                    if running slow.</small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_chattimeout" runat="server" DefaultButton="btn_updateintervals">
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Chat Sound Notification</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_chatsoundnoti_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_chatsoundnoti_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_chatsoundnoti_off" runat="server" Text="Mute" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_chatsoundnoti_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </td>
                                <td class="td-settings-desc">
                                    <small>Select Mute if you dont want to hear a sound when a new chat message comes in.</small>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="10" cellspacing="10">
                            <tr>
                                <td class="td-settings-title">
                                    <span class="pad-right font-bold font-color-black">Chat Timeout</span>
                                </td>
                                <td class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_updateintervals" runat="server" CssClass="textEntry" Width="35px"
                                        MaxLength="3"></asp:TextBox><span class="pad-left">minute(s)</span>
                                    <asp:Button ID="btn_updateintervals" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                        Text="Update" OnClick="btn_updateintervals_Click" />
                                </td>
                                <td class="td-settings-desc">
                                    <small>This value will represent the amount of time of inactivity before your chat status
                                        turns to away. (Default is 10 minutes)</small>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_chatclient_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_chatclient_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_chatsoundnoti_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_chatsoundnoti_off" />
                    <asp:AsyncPostBackTrigger ControlID="btn_updateintervals" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/acctsettings.js")%>'></script>
        <script type="text/javascript" src='<%=ResolveUrl("~/WebControls/jscolor/jscolor.js")%>'></script>
    </div>
</asp:Content>
