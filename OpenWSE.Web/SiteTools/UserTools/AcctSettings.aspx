<%@ Page Title="Account Settings" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AcctSettings.aspx.cs" Inherits="SiteTools_AcctSettings"
    ClientIDMode="Static" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Account Settings
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="MainContent_pnlLinkBtns" runat="server">
    </asp:Panel>

    <asp:Literal ID="ltl_UserCustomizationPageTitle" runat="server"></asp:Literal>
    <div id="pnl_topbackgroundTitleBar" runat="server" visible="false">
        <asp:Label ID="lbl_DateUpdated" runat="server" CssClass="pad-top"></asp:Label>
        <asp:Panel ID="non_grouplogin" runat="server">
            Settings will depend on the user Role, apps, and
            groups. Certain features may be visible but not used.<br />
            For new users defaults, all overlays and notifications will be enabled by default
            (Depending on which apps are installed and user role).
            <div class="clear-space"></div>
        </asp:Panel>
        <asp:Panel ID="grouplogin" runat="server" Visible="false" Enabled="false">
            Users logging into this group will inherit all the account settings below. These settings will are only applied for users who are logged into the group session.
            <div class="clear-space"></div>
        </asp:Panel>
    </div>
    <asp:Literal ID="UserGroupLoginMessage" runat="server"></asp:Literal>

    <asp:Panel ID="pnl_UserInformation" ClientIDMode="Static" CssClass="pnl-section" runat="server">
        <asp:Panel ID="pnl_passwordchange" runat="server">
            <div class="table-settings-box">
                <div class="td-settings-title">
                    Change Password
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
                    <asp:UpdatePanel ID="updatePanl_ChangePassword" runat="server">
                        <ContentTemplate>
                            <asp:ChangePassword ID="ChangeUserPassword" runat="server" CancelDestinationPageUrl="~/"
                                EnableViewState="false" RenderOuterTable="false" ContinueButtonStyle-CssClass="input-buttons" OnContinueButtonClick="OnContinueButtonClick">
                                <ChangePasswordTemplate>
                                    <asp:ValidationSummary ID="ChangeUserPasswordValidationSummary" runat="server" CssClass="failureNotification"
                                        ValidationGroup="ChangeUserPasswordValidationGroup" Visible="false" />
                                    <div class="accountInfo">
                                        <div class="float-left">
                                            <div class="textbox-group-padding">
                                                <div class="textbox-group">
                                                    <div class="password-login-img"></div>
                                                    <asp:TextBox ID="CurrentPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Old Password"></asp:TextBox>
                                                </div>
                                                <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPassword"
                                                    CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Old Password is required."
                                                    ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <div class="textbox-group-padding">
                                                <div class="textbox-group">
                                                    <div class="password-login-img"></div>
                                                    <asp:TextBox ID="NewPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="New Password"></asp:TextBox>
                                                </div>
                                                <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPassword"
                                                    CssClass="failureNotification" ErrorMessage="New Password is required." ToolTip="New Password is required."
                                                    ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <div class="textbox-group-padding">
                                                <div class="textbox-group">
                                                    <div class="password-login-img"></div>
                                                    <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Confirm New Password"></asp:TextBox>
                                                </div>
                                                <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword"
                                                    CssClass="failureNotification" Display="Dynamic" ErrorMessage="Confirm New Password is required."
                                                    ToolTip="Confirm New Password is required." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                                <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword"
                                                    ControlToValidate="ConfirmNewPassword" CssClass="failureNotification" Display="Dynamic"
                                                    ErrorMessage="The Confirm New Password must match the New Password entry." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                        <div class="clear-space">
                                        </div>
                                        <div class="clear-space-five">
                                        </div>
                                        <div class="failureNotification clear-margin" style="width: 315px;">
                                            <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                                        </div>
                                        <asp:Button ID="ChangePasswordPushButton_accountsettings" runat="server" CommandName="ChangePassword"
                                            Text="Change Password" ValidationGroup="ChangeUserPasswordValidationGroup" CssClass="input-buttons" />
                                    </div>
                                </ChangePasswordTemplate>
                            </asp:ChangePassword>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="td-settings-desc">
                    Passwords requires minimum of
                            <%= Membership.MinRequiredPasswordLength %>
                            characters.
                </div>
            </div>
        </asp:Panel>
        <asp:UpdatePanel ID="updatepnl_UserInformation1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="pnl_usercreds" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">User Information</div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="float-left">
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="empty-login-img"></div>
                                        <asp:TextBox ID="tb_firstname_accountsettings" runat="server" CssClass="signintextbox" placeholder="First Name"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="clear-space"></div>
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="empty-login-img"></div>
                                        <asp:TextBox ID="tb_lastname_accountsettings" runat="server" CssClass="signintextbox" placeholder="Last Name"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="clear-space"></div>
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="email-login-img"></div>
                                        <asp:TextBox ID="tb_email" runat="server" CssClass="signintextbox" placeholder="Email"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="clear"></div>
                            </div>
                            <div class="clear">
                            </div>
                            <div class="clear-margin">
                                <div class="clear-space">
                                </div>
                                <asp:Button ID="btn_updateinfo_accountsettings" runat="server" Text="Update Name"
                                    CssClass="input-buttons updatesettings" OnClick="btn_updateinfo_Click" />
                                <asp:Button ID="btn_markasnewuser" runat="server" CssClass="input-buttons updatesettings margin-top-sml"
                                    OnClick="btn_markasnewuser_Clicked" Text="Mark as new user" />
                            </div>
                        </div>
                    </div>
                </div>
                <asp:Panel ID="pnl_isSocialAccount" runat="server" CssClass="pad-bottom-big margin-bottom-big pad-top-big margin-top-big" Enabled="false" Visible="false">
                    <h3>This is a Social Network account which means you will not be able to change your password from this site. You must use the same network login every time you wish to access your account.
                    </h3>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btn_updateinfo_accountsettings" />
                <asp:AsyncPostBackTrigger ControlID="hf_clear_acctImage" />
                <asp:AsyncPostBackTrigger ControlID="btn_markasnewuser" />
            </Triggers>
        </asp:UpdatePanel>
        <asp:Panel ID="pnl_acctImage" runat="server">
            <div class="table-settings-box">
                <div class="td-settings-title">
                    User Image
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
                    <asp:Image ID="imgAcctImage" runat="server" ImageUrl="~/App_Themes/Standard/Icons/SiteMaster/EmptyUserImg.png" CssClass="acct-image float-left margin-top margin-right-big" />
                    <div class="float-left pad-top-big pad-bottom-big">
                        <asp:FileUpload ID="fileUpload_acctImage" runat="server" />
                        <div class="clear-space"></div>
                        <asp:Button ID="btn_fileUpload_acctImage" runat="server" CssClass="input-buttons" Text="Upload" OnClick="btn_fileUpload_acctImage_Clicked" />
                        <a id="btn_clear_acctImage" style="font-size: 11px;">Clear</a>
                        <asp:HiddenField ID="hf_clear_acctImage" runat="server" OnValueChanged="btn_clear_acctImage_Clicked" />
                    </div>
                    <div class="clear-space"></div>
                </div>
                <div class="td-settings-desc">
                    Only .jpg, .png, .gif, and .jpeg file extentions allowed
                </div>
            </div>
        </asp:Panel>
        <asp:UpdatePanel ID="updatepnl_UserInformation2" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnl_userRoleAssign" runat="server" Enabled="false" Visible="false">
                    <div class="table-settings-box">
                        <div id="userRoleAssign_text" runat="server" class="td-settings-title">
                            User Role
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:DropDownList ID="dd_roles" runat="server" CssClass="margin-right">
                            </asp:DropDownList>
                            <asp:Button ID="btn_roles" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="dd_roles_Changed" Text="Update" />
                        </div>
                        <div id="userRoleAssign_tip_text" runat="server" class="td-settings-desc">
                            Assign a role to this user. Each role will have custom defaults assigned to it. Assigning the User Role to Administrator will give the user full access to the site.
                        </div>
                    </div>
                </asp:Panel>
                <div id="pnl_userColor" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            User Color
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="float-left">
                                <asp:TextBox runat="server" ID="txt_userColor" CssClass="textEntry float-left margin-right"
                                    MaxLength="7" AutoCompleteType="None" Width="75px" TextMode="Color" />
                            </div>
                            <asp:Button ID="btn_updateusercolor" runat="server" CssClass="input-buttons margin-left RandomActionBtns" Text="Update Color" OnClick="btn_updateusercolor_Clicked" Style="margin-top: 2px;" />
                            <asp:LinkButton ID="btn_resetUserColor" runat="server" CssClass="RandomActionBtns" Text="Clear" OnClick="btn_resetUserColor_Clicked" Style="font-size: 11px;" />
                        </div>
                    </div>
                </div>
                <asp:Panel ID="pnl_adminpages_Holder" runat="server" Enabled="false" Visible="false">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Admin Pages
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:HiddenField ID="hf_addAdminPage" runat="server" OnValueChanged="hf_addAdminPage_ValueChanged"
                                Value="" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_removeAdminPage" runat="server" OnValueChanged="hf_removeAdminPage_ValueChanged"
                                Value="" ClientIDMode="Static" />
                            <asp:Panel ID="pnl_adminpages" runat="server">
                            </asp:Panel>
                            <div class="clear"></div>
                        </div>
                        <div class="td-settings-desc">
                            Standard Role Only
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_groupEditor" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            User Groups
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:HiddenField ID="hf_addGroup" runat="server" OnValueChanged="hf_addGroup_ValueChanged"
                                Value="" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_removeGroup" runat="server" OnValueChanged="hf_removeGroup_ValueChanged"
                                Value="" ClientIDMode="Static" />
                            <asp:Panel ID="pnl_groups" runat="server">
                            </asp:Panel>
                            <div class="clear"></div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_defaultLoginGroup" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Default Login Group
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:DropDownList ID="ddl_defaultGroupLogin" runat="server" CssClass="margin-right">
                            </asp:DropDownList>
                            <asp:Button ID="btn_defaultGroupLogin" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_defaultGroupLogin_Click" Text="Update" />
                        </div>
                        <div class="td-settings-desc">
                            You can set a default group to log into when signing in.
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_accountPrivacy" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Make Account Private
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_Privacy_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_Privacy_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_Privacy_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_Privacy_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Turning this on will stop any logging of the user. No users will be able to edit
                                or see your account.<br />
                            The site administrator is the only one that can see your account, but still cannot
                                edit or alter any setting on it.</small><br />
                            Click <a href="#learnmore" onclick="LearnMore();return false">HERE</a> to read more
                            about the private account setting.
                                                            <div class="clear-space">
                                                            </div>
                            <div id="moreInfo-PrivateAccount" class="clear-margin pad-all" style="display: none">
                                <h3 class="float-left font-bold">
                                    <u>More about the Private Account Feature</u></h3>
                                <a href="#close" onclick="LearnMore();return false" class="float-right">Close</a>
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
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_DeleteAccount" runat="server" Enabled="false" Visible="false">
                    <div class="table-settings-box">
                        <div class="td-settings-title-important">
                            Delete Account
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <input type="button" class="input-buttons" value="Delete Your Account" onclick="DeleteUserAccount();" />
                            <asp:HiddenField ID="hf_DeleteUserAccount" runat="server" OnValueChanged="hf_DeleteUserAccount_ValueChanged" ClientIDMode="Static" />
                        </div>
                        <div class="td-settings-desc">
                            Deleting your account will completely remove any information regarding you. Please be certain.
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btn_roles" />
                <asp:AsyncPostBackTrigger ControlID="btn_updateusercolor" />
                <asp:AsyncPostBackTrigger ControlID="btn_resetUserColor" />
                <asp:AsyncPostBackTrigger ControlID="rb_Privacy_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_Privacy_off" />
                <asp:AsyncPostBackTrigger ControlID="hf_DeleteUserAccount" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnl_NotificationSettings" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_NotificationSettings" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Notification List
                        <asp:LinkButton ID="btn_DisableAll_notification" runat="server" Text="Disable All"
                            ClientIDMode="Static" CssClass="RandomActionBtns float-right" OnClick="btn_DisableAll_notification_Clicked"></asp:LinkButton>
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:Panel ID="pnl_notifications" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        Set up your notifications for each app. Notifications can be sent via email if checked. Notifications are based on the user role and apps installed.
                    </div>
                </div>
                <asp:Panel ID="pnl_clearNoti" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Clear Notifications
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:Button ID="btn_clearnoti" runat="server" CssClass="input-buttons RandomActionBtns"
                                OnClick="btn_clearnoti_Clicked" Text="Clear My Notifications" />
                        </div>
                        <div class="td-settings-desc">
                            Click the Clear Notifications button to clear out all your current archived and pending notifications.
                        </div>
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

    <asp:Panel ID="pnl_WorkspaceOverlays" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_WorkspaceOverlays" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnl_useroverlaylist" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Overlay List
                            <asp:LinkButton ID="btn_DisableAll_overlay" runat="server" Text="Disable All" ClientIDMode="Static"
                                CssClass="RandomActionBtns float-right" OnClick="btn_DisableAll_overlay_Clicked"></asp:LinkButton>
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:Panel ID="pnl_overlays" runat="server">
                            </asp:Panel>
                        </div>
                        <div class="td-settings-desc">
                            Workspace overlays show limited information. They are a non editable type of app. Position determines which side of the screen the overlay will move to. Set the Workspace dropdown to the display the overlay on. When disabling the overlay, it will clear out your current settings for that overlay and reset them back to the defaults.
                        </div>
                    </div>
                </asp:Panel>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Disable Overlays
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <div class="field switch inline-block">
                            <asp:RadioButton ID="rb_hidealloverlays_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                OnCheckedChanged="rb_hidealloverlays_on_CheckedChanged" AutoPostBack="True" />
                            <asp:RadioButton ID="rb_hidealloverlays_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                OnCheckedChanged="rb_hidealloverlays_off_CheckedChanged" AutoPostBack="True" />
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        Select No if you want to be able to view overlays. (Page may have to refresh to show changes)
                    </div>
                </div>
                <asp:HiddenField ID="hf_updateEnabled_overlay" runat="server" ClientIDMode="Static"
                    OnValueChanged="hf_updateEnabled_overlay_Changed" />
                <asp:HiddenField ID="hf_updateDisabled_overlay" runat="server" ClientIDMode="Static"
                    OnValueChanged="hf_updateDisabled_overlay_Changed" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rb_hidealloverlays_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_hidealloverlays_off" />
                <asp:AsyncPostBackTrigger ControlID="btn_DisableAll_overlay" />
                <asp:AsyncPostBackTrigger ControlID="hf_updateEnabled_overlay" />
                <asp:AsyncPostBackTrigger ControlID="hf_updateDisabled_overlay" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnl_UserAppOverrides" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_UserAppOverrides" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        App List
                        <asp:LinkButton ID="lbtn_RefreshOverrides" runat="server" Text="Refresh" CssClass="RandomActionBtns float-right" OnClick="lbtn_RefreshOverrides_Click"></asp:LinkButton>
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <a id="lbtn_DeleteAllOverrides" runat="server" onclick="DeleteAllOverrides();return false;" class="float-right">Delete All Overrides</a>
                        <asp:HiddenField ID="hf_DeleteUserAppOverrides" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteUserAppOverrides_ValueChanged" />
                        <asp:HiddenField ID="hf_EditUserAppOverrides" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditUserAppOverrides_ValueChanged" />
                        <asp:HiddenField ID="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" runat="server" ClientIDMode="Static" OnValueChanged="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_ValueChanged" />
                        <asp:HiddenField ID="hf_UpdateUserAppOverrides" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateUserAppOverrides_ValueChanged" />
                        <div class="clear">
                        </div>
                        <asp:Panel ID="pnl_UserAppOverrideList" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        You can override certain settings to each of your apps installed. Apps must have the Allow User Overrides property set to true on the app in order for users to override the settings.
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="hf_DeleteUserAppOverrides" />
                <asp:AsyncPostBackTrigger ControlID="lbtn_RefreshOverrides" />
            </Triggers>
        </asp:UpdatePanel>
        <div id="App-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="700">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'App-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:Panel ID="pnl_appeditor" runat="server">
                                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                        <ContentTemplate>
                                            <asp:Image ID="img_edit" ImageUrl="" runat="server" CssClass='pad-right-big float-left'
                                                Style='height: 50px;' />
                                            <div class="float-left">
                                                <asp:Label ID="lbl_appId" runat="server" Font-Size="Large"></asp:Label>
                                                <div class="clear-space">
                                                </div>
                                                <b class='float-left pad-top-sml pad-right'>Category</b>
                                                <asp:CheckBoxList ID="dd_category_edit" runat="server" RepeatDirection="Vertical" RepeatColumns="3">
                                                </asp:CheckBoxList>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div class='clear-space'>
                                    </div>
                                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                        <ContentTemplate>
                                            <div class="float-left">
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Min Width
                                                </div>
                                                <asp:TextBox ID="tb_minwidth_edit" runat="server" CssClass="textEntry-noWidth" Width="70px" TextMode="Number"></asp:TextBox><span
                                                    class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Min Height
                                                </div>
                                                <asp:TextBox ID="tb_minheight_edit" runat="server" CssClass="textEntry-noWidth" Width="70px" TextMode="Number"></asp:TextBox><span
                                                    class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Background
                                                </div>
                                                <asp:DropDownList ID="dd_enablebg_edit" runat="server">
                                                    <asp:ListItem Text="Visible" Value="app-main"></asp:ListItem>
                                                    <asp:ListItem Text="Hidden" Value="app-main-nobg"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div id="backgroundcolorholder_edit">
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Background Color
                                                    </div>
                                                    <asp:TextBox ID="tb_backgroundColor_edit" runat="server" CssClass="textEntry-noWidth" MaxLength="7" Width="75px" TextMode="Color"></asp:TextBox>
                                                    <div class="clear-space-five"></div>
                                                    <div class="div_usedefaultcolor">
                                                        <asp:CheckBox ID="cb_backgroundColor_edit_default" runat="server" />
                                                        <asp:Label ID="lbl_backgroundColor_edit_default" runat="server" AssociatedControlID="cb_backgroundColor_edit_default" Text="Use Inherit"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Maximize
                                                </div>
                                                <asp:DropDownList ID="dd_allowmax_edit" runat="server">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Pop Out
                                                </div>
                                                <asp:DropDownList ID="dd_allowpopout_edit" runat="server">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                            <div class="float-left" style="padding-left: 75px;">
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Icon Color
                                                </div>
                                                <asp:TextBox ID="tb_iconColor_edit" runat="server" CssClass="textEntry-noWidth" MaxLength="7" Width="75px" TextMode="Color"></asp:TextBox>
                                                <div class="clear-space-five"></div>
                                                <div class="div_usedefaultcolor">
                                                    <asp:CheckBox ID="cb_iconColor_edit_default" runat="server" />
                                                    <asp:Label ID="lbl_iconColor_edit_default" runat="server" AssociatedControlID="cb_iconColor_edit_default" Text="Use Inherit"></asp:Label>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Max on Load
                                                </div>
                                                <asp:DropDownList ID="dd_maxonload_edit" runat="server">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Resize
                                                </div>
                                                <asp:DropDownList ID="dd_allowresize_edit" runat="server">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="float-left">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Default Workspace
                                                    </div>
                                                    <asp:DropDownList ID="dd_defaultworkspace_edit" runat="server" Style="width: 65px;">
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Auto Open
                                                </div>
                                                <asp:DropDownList ID="dd_autoOpen_edit" runat="server">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                Pop Out Location
                                            </div>
                                            <asp:TextBox ID="tb_allowpopout_edit" runat="server" CssClass="textEntry-noWidth" Width="465px"></asp:TextBox>
                                            <div class="clear-space"></div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div class="clear-space">
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons modal-ok-btn" onclick="UpdateOverrides();" value="Save" />
                            <input id="btn_undoOverrides" type="button" class="input-buttons modal-cancel-btn" onclick="DeleteOverrides('');"
                                value="Reset" />
                            <input type="button" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'App-element', '');"
                                value="Close" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnl_WorkspaceContainer" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_WorkspaceContainer" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="workspaceAccordion" class="custom-accordion">
                    <h3>Workspace Settings</h3>
                    <div>
                        <asp:Panel ID="pnl_WorkspaceMode" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Workspace Mode
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="ddl_WorkspaceMode" runat="server" CssClass="margin-right">
                                        <asp:ListItem Value="Simple" Text="Page Based"></asp:ListItem>
                                        <asp:ListItem Value="Complex" Text="Window Based"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_WorkspaceMode" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_WorkspaceMode_Click" Text="Update" />
                                </div>
                                <div class="td-settings-desc">
                                    Switch to Simple mode if you do not like the use of the apps. This will turn your workspace into a more generic looking website.
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_TotalWorkspaces" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Total Number of Workspaces
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="ddl_totalWorkspaces" runat="server">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_updateTotalWorkspaces" runat="server" Text="Update" CssClass="margin-left input-buttons RandomActionBtns" OnClick="btn_updateTotalWorkspaces_Click" ClientIDMode="Static" />
                                </div>
                                <div class="td-settings-desc">
                                    You can select the total number of workspaces to show on your home workspace page.
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_ShowWorkspacePreview" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show Workspace Preview
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showWorkspacePreview_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showWorkspacePreview_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showWorkspacePreview_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showWorkspacePreview_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select Yes to show a preview of the minimized workspace when hovering over in the selector dropdown.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3 id="pnl_AutoRotateFeatures_tab" runat="server">Auto Rotate Features</h3>
                    <div id="pnl_AutoRotateFeatures" runat="server">
                        <asp:Panel ID="pnl_autoRotateOnOff" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Auto Rotate Workspace
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_enableautorotate_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_enableautorotate_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_enableautorotate_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_enableautorotate_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    This is a good tool to enable if you want to periodically change screens automatically.
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlAutoRotateWorkspace" runat="server" DefaultButton="btn_updateintervals_rotate">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Update Upon Rotate
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_updateOnRotate_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_updateOnRotate_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_updateOnRotate_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_updateOnRotate_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Selecting Yes will refresh any control on the current workspace when rotating.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Auto Rotate Interval
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_autorotateinterval" runat="server" CssClass="textEntry" Width="65px"
                                        MaxLength="7" TextMode="Number"></asp:TextBox><span class="pad-left">seconds(s)</span>
                                    <asp:Button ID="btn_updateintervals_rotate" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                        Text="Update" OnClick="btn_updateintervals_rotate_Click" />
                                </div>
                                <div class="td-settings-desc">
                                    This value will represent the time to spend on each workspace before changing
                                                screens.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Auto Rotate Screens
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="ddl_autoRotateNumber" runat="server" CssClass="margin-right"></asp:DropDownList>
                                    <asp:Button ID="btn_screenRotateNumberUpdate" runat="server" Text="Update" OnClick="btn_screenRotateNumberUpdate_Click" CssClass="input-buttons RandomActionBtns" />
                                </div>
                                <div class="td-settings-desc">
                                    Choose the number of screens to rotate from.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3 id="pnl_AppsOnWorkspace_tab" runat="server">Apps On Workspace</h3>
                    <div id="pnl_AppsOnWorkspace" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Keep Apps in a container
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_appcontainer_enabled" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_appcontainer_enabled_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_appcontainer_disabled" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_appcontainer_disabled_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select No if you want to be able to drag app windows outside the workspace.
                                            (Set to Yes by default)
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                App Snap Helper
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_snapapphelper_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_snapapphelper_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_snapapphelper_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_snapapphelper_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Yes if you want to be able to snaps apps to the current axis of other opened apps as you drag an app along the workspace.(Set to No by default)
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Snap Apps to Grid
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_snapapp_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_snapapp_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_snapapp_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_snapapp_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Yes if you want to be able to snap app windows to the container grid.
                                            (Set to No by default)
                            </div>
                        </div>
                        <asp:Panel ID="pnl_appGridSize" runat="server" DefaultButton="btn_AppGridSize">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Grid Size
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:TextBox ID="txt_AppGridSize" runat="server" CssClass="textEntry margin-right" MaxLength="3" Width="55px" TextMode="Number"></asp:TextBox>
                                    <asp:Button ID="btn_AppGridSize" runat="server" CssClass="input-buttons RandomActionBtns" Text="Update" OnClick="btn_AppGridSize_Click" />
                                </div>
                                <div class="td-settings-desc">
                                    Set the grid size of the workspace to move the app on. (Value must be greater than 0)
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_clearproperties" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Clear Properties on Log Out
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_clearproperties_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_clearproperties_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_clearproperties_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_clearproperties_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Clear current app properties and ALL cookies created by this site everytime
                                                you log out. (Set to No by default)
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_clearUserProp" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Clear User App Properties
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Button ID="btn_clearapps" runat="server" Text="Clear Properties" OnClick="btn_clearapps_Click"
                                        CssClass="updatesettings input-buttons" CausesValidation="False" />
                                </div>
                                <div class="td-settings-desc">
                                    Delete all settings for for your apps (Size, loading, etc...). This will also
                                            delete ALL cookies created by this site<br />
                                    not including the current ASP.Net session.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3 id="pnl_currentbackgroundselector_tab" runat="server">Backgrounds</h3>
                    <div id="pnl_currentbackgroundselector" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Workspace Background
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <table cellpadding="5px" cellspacing="5px">
                                    <tr>
                                        <td valign="top">
                                            <div id="CurrentBackground">
                                            </div>
                                            <asp:Panel ID="pnl_backgroundSelector" runat="server">
                                                <div class="clear-space"></div>
                                                <span class="font-bold pad-right">Workspace</span>
                                                <asp:DropDownList ID="dd_backgroundSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="dd_backgroundSelector_Changed">
                                                </asp:DropDownList>
                                                <div class="pad-bottom">
                                                    <small>Select a workspace to edit the background for.</small>
                                                </div>
                                            </asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top">
                                            <asp:HiddenField ID="hf_backgroundimg" runat="server" OnValueChanged="hf_backgroundimg_Changed" />
                                            <asp:HiddenField ID="hf_backgroundimgClearAdd" runat="server" OnValueChanged="hf_backgroundimgClearAdd_Changed" />
                                            <asp:HiddenField ID="hf_removebackgroundimgEdit" runat="server" OnValueChanged="hf_removebackgroundimgEdit_Changed" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_removebackgroundimg" runat="server" OnValueChanged="hf_removebackgroundimg_Changed" ClientIDMode="Static" />
                                            <div class="input-settings-holder float-left" style="clear: none!important; margin-right: 30px;">
                                                <span class="font-bold">Solid Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox runat="server" ID="txt_bgColor" CssClass="textEntry float-left margin-right" MaxLength="7" AutoCompleteType="None" Width="75px" TextMode="Color" Text="#FFFFFF" />
                                                <asp:Button ID="btn_updateBGcolor" runat="server" CssClass="input-buttons float-left RandomActionBtns" Text="Set Color" OnClick="btn_updateBGcolor_Clicked" />
                                                <div class="clear"></div>
                                            </div>
                                            <div class="input-settings-holder float-left" style="clear: none!important;">
                                                <span class="font-bold">Url Image</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry"></asp:TextBox>
                                                <asp:Button ID="btn_urlupdate" runat="server" Text="Set Url Image" CssClass="input-buttons margin-left updatesettings" OnClick="btn_urlupdate_Click" />
                                                <div class="clear-space-two"></div>
                                                <small>Copy and paste any link that contains an image.</small>
                                            </div>
                                            <div class="clear"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Upload Image</span>
                                                <div class="clear"></div>
                                                <asp:Panel ID="pnl_iframeUserImageUpload" runat="server"></asp:Panel>
                                            </div>
                                            <div class="clear-space"></div>
                                            <a href="#" onclick="BackgroundSelector();return false;" class="input-buttons" style="padding-right: 15px!important;">Select a Background</a>
                                            <asp:LinkButton ID="lb_clearbackground" runat="server" OnClick="lb_clearbackground_Click" Text="Clear Backgrounds" Font-Size="11px"></asp:LinkButton>
                                            <div class="clear"></div>
                                        </td>
                                    </tr>
                                </table>
                                <span class="font-color-black" id="backgroundsaved"></span>
                            </div>
                            <div class="td-settings-desc">
                                Your connection speed will slow down with the larger images. Each image has
                                the size details when you hover over them. Solid color backgrounds will be the quickest
                                if you have a slower internet connection.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Background Position
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_backgroundposition" runat="server">
                                    <asp:ListItem Text="Left Top" Value="left top"></asp:ListItem>
                                    <asp:ListItem Text="Left Center" Value="left center"></asp:ListItem>
                                    <asp:ListItem Text="Left Bottom" Value="left bottom"></asp:ListItem>
                                    <asp:ListItem Text="Right Top" Value="right top"></asp:ListItem>
                                    <asp:ListItem Text="Right Center" Value="right center"></asp:ListItem>
                                    <asp:ListItem Text="Right Bottom" Value="right bottom"></asp:ListItem>
                                    <asp:ListItem Text="Center Top" Value="center top"></asp:ListItem>
                                    <asp:ListItem Text="Center Center" Value="center center"></asp:ListItem>
                                    <asp:ListItem Text="Center Bottom" Value="center bottom"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:Button ID="btn_backgroundposition" runat="server" Text="Update" OnClick="btn_backgroundposition_Click"
                                    CssClass="RandomActionBtns input-buttons margin-left-big" />
                            </div>
                            <div class="td-settings-desc">Select the position of the background. Applys for all workspaces and background images.</div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Background Size
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_backgroundsize" runat="server">
                                    <asp:ListItem Text="Normal" Value="auto"></asp:ListItem>
                                    <asp:ListItem Text="Stretch" Value="100% 100%"></asp:ListItem>
                                    <asp:ListItem Text="Cover" Value="cover"></asp:ListItem>
                                    <asp:ListItem Text="Contain" Value="contain"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:Button ID="btn_backgroundsize" runat="server" Text="Update" OnClick="btn_backgroundsize_Click"
                                    CssClass="RandomActionBtns input-buttons margin-left-big" />
                            </div>
                            <div class="td-settings-desc">Select the size of the background. Applys for all workspaces and background images.</div>
                        </div>
                        <div id="background_repeat_holder_acctsettings" runat="server" class="table-settings-box">
                            <div class="td-settings-title">
                                Repeat Background
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_backgroundrepeat_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_backgroundrepeat_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_backgroundrepeat_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_backgroundrepeat_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Turn on/off repeating the background image. Applys for all workspaces and background images.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Default Background Color
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:Panel ID="pnl_defaultbackgroundcolor" runat="server" DefaultButton="btn_defaultbackgroundcolor">
                                    <div class="float-left">
                                        <asp:TextBox runat="server" ID="tb_defaultbackgroundcolor" CssClass="textEntry float-left margin-right"
                                            MaxLength="7" AutoCompleteType="None" Width="75px" TextMode="Color" />
                                    </div>
                                    <asp:Button ID="btn_defaultbackgroundcolor" runat="server" CssClass="input-buttons margin-left RandomActionBtns" Text="Update" OnClick="btn_defaultbackgroundcolor_Clicked" Style="margin-top: 2px;" />
                                </asp:Panel>
                            </div>
                            <div class="td-settings-desc">
                                Set the default background color. Applys for all workspaces and background images.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_BackgroundLoopTimer" runat="server" DefaultButton="btn_backgroundlooptimer">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Background Loop Timer
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_backgroundlooptimer" runat="server" TextMode="Number" CssClass="textEntry" Width="60px"></asp:TextBox>&nbsp;Second(s)
                                        <asp:Button ID="btn_backgroundlooptimer" runat="server" Text="Update" OnClick="btn_backgroundlooptimer_Click"
                                            CssClass="RandomActionBtns input-buttons margin-left-big" />
                                </div>
                                <div class="td-settings-desc">Change the amount of time to loop through each background you have selected.</div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_backgroundurl" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Individual Workspace Backgrounds
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_enablebackgrounds_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_enablebackgrounds_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_enablebackgrounds_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_enablebackgrounds_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Allows the use of multiple backgrounds, one for each workspace.
                                </div>
                            </div>
                        </asp:Panel>
                        <div id="Background-element" class="Modal-element">
                            <div class="Modal-overlay">
                                <div class="Modal-element-align">
                                    <div class="Modal-element-modal" data-setwidth="700">
                                        <div class="ModalHeader">
                                            <div>
                                                <div class="app-head-button-holder-admin">
                                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'Background-element', '');return false;"
                                                        class="ModalExitButton"></a>
                                                </div>
                                                <span class="Modal-title"></span>
                                            </div>
                                        </div>
                                        <div class="ModalScrollContent">
                                            <div class="ModalPadContent">
                                                <div class="clear-space-five">
                                                </div>
                                                <small class="float-right">Click on the background that you would like to apply to your workspace.</small>
                                                <span class="font-bold pad-right">Folder</span>
                                                <asp:DropDownList ID="dd_imageFolder" runat="server" ClientIDMode="Static" OnSelectedIndexChanged="dd_imageFolder_SelectedIndexChanged" AutoPostBack="true">
                                                    <asp:ListItem Text="User Uploads" Value="user"></asp:ListItem>
                                                    <asp:ListItem Text="Public" Value="public"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <asp:HiddenField ID="hf_backgroundselector" runat="server" OnValueChanged="hf_backgroundselector_ValueChanged"
                                                    Value="" />
                                                <asp:HiddenField ID="hf_deleteUploadedImage" runat="server" OnValueChanged="hf_deleteUploadedImage_ValueChanged" ClientIDMode="Static" />
                                                <div id="pnl_images" align="center">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btn_WorkspaceMode" />
                <asp:AsyncPostBackTrigger ControlID="rb_snapapp_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_snapapp_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_appcontainer_enabled" />
                <asp:AsyncPostBackTrigger ControlID="rb_appcontainer_disabled" />
                <asp:AsyncPostBackTrigger ControlID="rb_enableautorotate_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_enableautorotate_off" />
                <asp:AsyncPostBackTrigger ControlID="btn_screenRotateNumberUpdate" />
                <asp:AsyncPostBackTrigger ControlID="rb_updateOnRotate_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_updateOnRotate_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_showWorkspacePreview_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showWorkspacePreview_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_on" />
                <asp:AsyncPostBackTrigger ControlID="btn_clearapps" />
                <asp:AsyncPostBackTrigger ControlID="hf_backgroundimg" />
                <asp:AsyncPostBackTrigger ControlID="hf_backgroundimgClearAdd" />
                <asp:AsyncPostBackTrigger ControlID="lb_clearbackground" />
                <asp:AsyncPostBackTrigger ControlID="btn_updateBGcolor" />
                <asp:AsyncPostBackTrigger ControlID="btn_backgroundlooptimer" />
                <asp:AsyncPostBackTrigger ControlID="rb_enablebackgrounds_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_enablebackgrounds_off" />
                <asp:AsyncPostBackTrigger ControlID="dd_backgroundSelector" />
                <asp:AsyncPostBackTrigger ControlID="btn_urlupdate" />
                <asp:AsyncPostBackTrigger ControlID="hf_backgroundselector" />
                <asp:AsyncPostBackTrigger ControlID="hf_removebackgroundimgEdit" />
                <asp:AsyncPostBackTrigger ControlID="hf_removebackgroundimg" />
                <asp:AsyncPostBackTrigger ControlID="btn_backgroundposition" />
                <asp:AsyncPostBackTrigger ControlID="btn_backgroundsize" />
                <asp:AsyncPostBackTrigger ControlID="rb_backgroundrepeat_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_backgroundrepeat_off" />
                <asp:AsyncPostBackTrigger ControlID="btn_defaultbackgroundcolor" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnl_IconSelector" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_IconSelector" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="iconSelectorAccordion" class="custom-accordion">
                    <h3>Styles</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                App Selector Style
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_AppSelectorStyle" runat="server" CssClass="margin-right">
                                </asp:DropDownList>
                                <asp:Button ID="btn_AppSelectorStyle" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_AppSelectorStyle_Click" Text="Update" />
                            </div>
                            <div class="td-settings-desc">
                                You can select different types of App Selectors to customize your account.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_ShowWorkspaceNum" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show Workspace Number in App Icon
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_ShowWorkspaceNumApp_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_ShowWorkspaceNumApp_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_ShowWorkspaceNumApp_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_ShowWorkspaceNumApp_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select No to hide the workspace number that the app is currently on. The number
                                        can be seen to the right of the app icons.
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_lockappicons" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Lock App Icons
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_LockAppIcons_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_LockAppIcons_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_LockAppIcons_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_LockAppIcons_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Set to Lock if you want to lock the order of the app icons. This disables
                                        the ability to sort the app icons.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3>Category Settings</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Categorize App List
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_groupicons_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_groupicons_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_groupicons_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_groupicons_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Enabling this will group the icons by category allowing for easier browsing. (Note: Sorting may not work properly when this is enabled)
                            </div>
                        </div>
                        <asp:Panel ID="pnl_categoryCount" runat="server" Enabled="false" Visible="false">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    App Category Count
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showappcategoryCount_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showappcategoryCount_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showappcategoryCount_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showappcategoryCount_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select Hide if you dont want to see the total count per category.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3 id="pnl_MinimizedApps_tab" runat="server">Minimized Apps</h3>
                    <div id="pnl_MinimizedApps" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Show Dedicated Minimized Area
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_showdedicatedminimizedarea_On" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_showdedicatedminimizedarea_On_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_showdedicatedminimizedarea_Off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_showdedicatedminimizedarea_Off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select No to hide any app not on the currently selected workspace. (Set to
                                        Yes by default)
                            </div>
                        </div>
                        <asp:Panel ID="pnl_ShowAllMinimized" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show All Minimized
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_taskbarShowAll_On" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_taskbarShowAll_On_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_taskbarShowAll_Off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_taskbarShowAll_Off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select No to hide any app not on the currently selected workspace. (Set to
                                        Yes by default)
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Show Minimized Preview
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_showPreview_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_showPreview_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_showPreview_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_showPreview_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Yes to show a preview of the minimized app when hovered over.
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rb_LockAppIcons_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_LockAppIcons_off" />
                <asp:AsyncPostBackTrigger ControlID="btn_AppSelectorStyle" />
                <asp:AsyncPostBackTrigger ControlID="rb_showappcategoryCount_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showappcategoryCount_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_groupicons_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_groupicons_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowWorkspaceNumApp_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowWorkspaceNumApp_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_taskbarShowAll_On" />
                <asp:AsyncPostBackTrigger ControlID="rb_taskbarShowAll_Off" />
                <asp:AsyncPostBackTrigger ControlID="rb_showPreview_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showPreview_off" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnl_SiteCustomizations" ClientIDMode="Static" CssClass="pnl-section" runat="server" Style="display: none;">
        <asp:UpdatePanel ID="updatepnl_SiteCustomizations" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="customizationAccordion" class="custom-accordion">
                    <h3>Look and Feel</h3>
                    <div>
                        <asp:Panel ID="pnl_demoPackage" runat="server" Enabled="false" Visible="false">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    App Package
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="dd_appdemo" runat="server">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_updatedemo" runat="server" Text="Update" CssClass="input-buttons margin-left RandomActionBtns"
                                        OnClick="btn_updatedemo_Click" />
                                </div>
                                <div class="td-settings-desc">
                                    Select the App Package you want to install for this user when online.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Plugins To Install
                                <a href="#" class="float-right" onclick="RemoveAllPlugins();return false;" style="font-weight: normal!important;">Uninstall All Plugins</a>
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_overlayList" runat="server">
                                    </asp:Panel>
                                    <asp:HiddenField ID="hf_addPlugin" runat="server" OnValueChanged="hf_addPlugin_ValueChanged" />
                                    <asp:HiddenField ID="hf_removePlugin" runat="server" OnValueChanged="hf_removePlugin_ValueChanged" />
                                    <asp:HiddenField ID="hf_removeAllPlugins" runat="server" OnValueChanged="hf_removeAllPlugins_ValueChanged" />
                                </div>
                                <div class="td-settings-desc">
                                    Select the plugins you want to install for this user when online. (Does not apply to Mobile)
                                </div>
                            </div>
                        </asp:Panel>
                        <div id="fontcustomizations">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Default Site Font Family
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_defaultbodyfontfamily" runat="server" DefaultButton="btn_defaultbodyfontfamily">
                                        <asp:DropDownList ID="dd_defaultbodyfontfamily" runat="server" CssClass="margin-right">
                                        </asp:DropDownList>
                                        <asp:Button ID="btn_defaultbodyfontfamily" runat="server" Text="Update" OnClick="btn_defaultbodyfontfamily_Click"
                                            CssClass="RandomActionBtns input-buttons margin-left" />
                                        <asp:LinkButton ID="lbtn_defaultbodyfontfamily_reset" runat="server" Text="Reset to server default" OnClick="lbtn_defaultbodyfontfamily_reset_Click"
                                            CssClass="RandomActionBtns" />
                                    </asp:Panel>
                                    <div class="clear-space"></div>
                                    <div id="span_fontfamilypreview"></div>
                                </div>
                                <div class="td-settings-desc">
                                    Override the default site font family. Some fonts may not work with certain browsers. (Must refresh the page to see changes)
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Default Site Font Size
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_defaultfontsize" runat="server" DefaultButton="btn_defaultfontsize">
                                        <asp:TextBox ID="tb_defaultfontsize" runat="server" CssClass="textEntry" Width="55px" TextMode="Number"></asp:TextBox><span class="pad-left-sml">px</span>
                                        <asp:Button ID="btn_defaultfontsize" runat="server" Text="Update" OnClick="btn_defaultfontsize_Click"
                                            CssClass="RandomActionBtns input-buttons margin-left" />
                                        <asp:LinkButton ID="lbtn_defaultfontsize_clear" runat="server" Text="Reset to server default" OnClick="lbtn_defaultfontsize_clear_Click"
                                            CssClass="RandomActionBtns" />
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Override the default site font size. Empty values will inherit the font-size from the site theme. (Must refresh the page to see changes)
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Default Site Font Color
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_defaultfontcolor" runat="server" DefaultButton="btn_defaultfontcolor">
                                        <asp:TextBox ID="tb_defaultfontcolor" runat="server" CssClass="textEntry" MaxLength="7" Width="75px" TextMode="Color"></asp:TextBox>
                                        <asp:Button ID="btn_defaultfontcolor" runat="server" Text="Update" OnClick="btn_defaultfontcolor_Click"
                                            CssClass="RandomActionBtns input-buttons margin-left" />
                                        <asp:LinkButton ID="lbtn_defaultfontcolor_clear" runat="server" Text="Reset to server default" OnClick="lbtn_defaultfontcolor_clear_Click"
                                            CssClass="RandomActionBtns" />
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Override the default site font color. (Must refresh the page to see changes)
                                </div>
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Show Row Count in Tables
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_ShowRowCountGridViewTable_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_ShowRowCountGridViewTable_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_ShowRowCountGridViewTable_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_ShowRowCountGridViewTable_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Enable to show the row count for all tables on site. (Might not apply to all tables. Custom Tables and Imported Tables will have their own customizations.)
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Use Alternate Table Rows
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_UseAlternateGridviewRows_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_UseAlternateGridviewRows_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_UseAlternateGridviewRows_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_UseAlternateGridviewRows_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Enable to show alternate row style for all tables on site. (Might not apply to all tables. Custom Tables and Imported Tables will have their own customizations.)
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Animation Speed
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:TextBox ID="tb_animationSpeed" runat="server" ClientIDMode="Static" CssClass="textEntry" MaxLength="4" Width="75px" TextMode="Number" step="5"></asp:TextBox><span class="margin-left-sml margin-right">ms</span>
                                <input id="btnUpdateAnimiation" type="button" class="input-buttons margin-bottom" value="Update" onclick="UpdateAnimationSpeed();" />
                                <a onclick="ResetAnimationSpeed();return false;">Reset</a>
                                <asp:HiddenField ID="hf_AnimationSpeed" runat="server" ClientIDMode="Static" OnValueChanged="hf_AnimationSpeed_Changed" />
                            </div>
                            <div class="td-settings-desc">
                                Changing the animation speed will not effect all elements with a transition.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_theme" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Account Theme
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="dd_theme" runat="server" CssClass="margin-right">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_UpdateTheme" runat="server" OnClick="dd_theme_Changed" Text="Update" CssClass="input-buttons RandomActionBtns" />
                                </div>
                                <div class="td-settings-desc">
                                    Change the overall look of the site to fit your taste. Themes apply to the Workspace
                                and App Settings.
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Layout Option
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:RadioButton ID="rb_BoxedLayout_acctOptions" runat="server" Text="&nbsp;Boxed" CssClass="radiobutton-style" GroupName="layoutstyles" AutoPostBack="true" OnCheckedChanged="rb_BoxedLayout_acctOptions_Changed" />
                                <div class="clear-space">
                                </div>
                                <asp:RadioButton ID="rb_WideLayout_acctOptions" runat="server" Text="&nbsp;Wide" CssClass="radiobutton-style" GroupName="layoutstyles" AutoPostBack="true" OnCheckedChanged="rb_WideLayout_acctOptions_Changed" />
                                <div class="clear"></div>
                            </div>
                            <div class="td-settings-desc">
                                Select the site layout you wish to have for your account
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Color Option
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:Panel ID="pnl_ColorOptions" runat="server"></asp:Panel>
                                <div class="clear"></div>
                                <asp:HiddenField ID="hf_ColorOptions" runat="server" OnValueChanged="hf_ColorOptions_ValueChanged" />
                            </div>
                            <div class="td-settings-desc">
                                Select the color theme you would like to apply for all pages. This is not the same as the Account Theme as each theme has multiple color options within it.
                            </div>
                        </div>
                    </div>
                    <h3>Top bar Customizations</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Show Date/Time
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_showdatetime_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_showdatetime_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_showdatetime_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_showdatetime_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select No if you dont want to see the date/time in the top tool bar.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Hide Search Bar In Top Bar
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_HideSearchBarInTopBar_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_HideSearchBarInTopBar_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_HideSearchBarInTopBar_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_HideSearchBarInTopBar_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Yes to always hide the search bar in the top bar.
                            </div>
                        </div>
                        <div id="userprofilelinkstyle_div" runat="server" class="table-settings-box">
                            <div class="td-settings-title">
                                User Profile Link Style
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_ProfileLinkStyle" runat="server" CssClass="margin-right">
                                </asp:DropDownList>
                                <asp:Button ID="btn_ProfileLinkStyle" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_ProfileLinkStyle_Click" Text="Update" />
                            </div>
                            <div class="td-settings-desc">
                                You can change the style of your profile link which is displayed at the top right of the screen.
                            </div>
                        </div>
                        <div class="clear"></div>
                    </div>
                    <h3 id="sidebarCustomizations_title" runat="server">Sidebar Customizations</h3>
                    <div id="sidebarCustomizations_div" runat="server">
                        <asp:Panel ID="pnl_ShowSiteToolsInCategories" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show Site Tools in Categories
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_ShowSiteToolsInCategories_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_ShowSiteToolsInCategories_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_ShowSiteToolsInCategories_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_ShowSiteToolsInCategories_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select no if you wish to view all the site tool pages available in a single section. (You must refresh the page when updated)
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_loadLinksOnNewPage" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Load links on new page
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_linksnewpage_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_linksnewpage_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_linksnewpage_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_linksnewpage_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Set this to false if you don't want a new page to load when clicking the links
                                    in the top right hand corner.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show Icon Only (Settings/Tools)
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_SiteToolsIconOnly_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_SiteToolsIconOnly_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_SiteToolsIconOnly_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_SiteToolsIconOnly_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Set this to Yes to only show the icon of the settings/tools without any text.
                                </div>
                            </div>
                            <asp:Panel ID="pnl_ShowPageDescriptions" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Show Page Descriptions (Settings/Tools)
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_showpagedescriptions_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_showpagedescriptions_on_CheckedChanged" AutoPostBack="True" />
                                            <asp:RadioButton ID="rb_showpagedescriptions_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_showpagedescriptions_off_CheckedChanged" AutoPostBack="True" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Set this to Yes to show a small page description below the links in the sidebar.
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Allow Navigation Menu Sections To Collapse
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_AllowNavMenuCollapseToggle_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_AllowNavMenuCollapseToggle_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_AllowNavMenuCollapseToggle_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_AllowNavMenuCollapseToggle_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Set this to Yes to allow the user to collapse the sidebar sections.
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="clear"></div>
                    </div>
                    <h3>Miscellaneous Settings</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Hover Tool Tips
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_tooltips_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_tooltips_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_tooltips_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_tooltips_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Off if you dont want to see any site tool tips when hovering over certain objects and links.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Site Tips on Page Load
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_sitetipsonload_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_sitetipsonload_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_sitetipsonload_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_sitetipsonload_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                You can view tips and tricks on page load.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_ShowAppTitle" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show App Title in App Header
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_showHeader_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_showHeader_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_showHeader_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_showHeader_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Disable this if you do not want to see the app title in the app header.
                                            (Enabled by default)
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_showAppImage" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Show App Image in App Header
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_AppHeaderIcon_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_AppHeaderIcon_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_AppHeaderIcon_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_AppHeaderIcon_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Disable this if you do not want to see the app image in the app header.
                                            (Enabled by default)
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="table-settings-box">
                            <div id="lbl_appmodalstyle_title" runat="server" class="td-settings-title">
                                App and Modal Window Style
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_appStyle" runat="server" CssClass="margin-right">
                                </asp:DropDownList>
                                <asp:Button ID="btn_appStyle" runat="server" OnClick="dd_appStyle_Changed" Text="Update" CssClass="input-buttons RandomActionBtns" />
                                <asp:Label ID="lbl_appstyleexample_space" runat="server" ClientIDMode="Static" CssClass="clear-space" Style="display: block;"></asp:Label>
                                <asp:Image ID="img_appstyleexample" runat="server" ClientIDMode="Static" />
                            </div>
                            <div id="lbl_appmodalstyle_desc" runat="server" class="td-settings-desc">
                                Change the look of your app window and modal popups.
                            </div>
                        </div>
                        <div class="clear"></div>
                    </div>
                    <h3 id="pnl_ChatClient_tab" runat="server">Chat Client</h3>
                    <div id="pnl_ChatClient" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Chat Client
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_chatclient_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_chatclient_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_chatclient_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_chatclient_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Turn Off if you dont want the chat feature. Turning this Off may boost performance
                                    if running slow.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_chattimeout" runat="server" DefaultButton="btn_updateintervals">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Chat Sound Notification
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_chatsoundnoti_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_chatsoundnoti_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_chatsoundnoti_off" runat="server" Text="Mute" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_chatsoundnoti_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select Mute if you dont want to hear a sound when a new chat message comes in.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Chat Timeout
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_updateintervals" runat="server" CssClass="textEntry" Width="55px" TextMode="Number"></asp:TextBox><span class="pad-left">minute(s)</span>
                                    <asp:Button ID="btn_updateintervals" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                        Text="Update" OnClick="btn_updateintervals_Click" />
                                </div>
                                <div class="td-settings-desc">
                                    This value will represent the amount of time of inactivity before your chat status
                                        turns to away. (Default is 10 minutes)
                                </div>
                            </div>
                        </asp:Panel>
                    </div>

                    <h3 id="pnl_AppRemoteContainer_tab" runat="server">Mobile</h3>
                    <div id="pnl_AppRemoteContainer" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Try to Auto Connect to Workspace when using Mobile
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_MobileAutoSync_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_MobileAutoSync_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_MobileAutoSync_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_MobileAutoSync_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Set this to Yes to automatically sync your workspace with your Mobile.
                            </div>
                        </div>
                    </div>
                </div>
                <div class="clear-space"></div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rb_tooltips_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_tooltips_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_sitetipsonload_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_sitetipsonload_off" />
                <asp:AsyncPostBackTrigger ControlID="hf_AnimationSpeed" />
                <asp:AsyncPostBackTrigger ControlID="btn_UpdateTheme" />
                <asp:AsyncPostBackTrigger ControlID="btn_appStyle" />
                <asp:AsyncPostBackTrigger ControlID="rb_showHeader_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showHeader_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_off" />
                <asp:AsyncPostBackTrigger ControlID="hf_addPlugin" />
                <asp:AsyncPostBackTrigger ControlID="hf_removePlugin" />
                <asp:AsyncPostBackTrigger ControlID="hf_removeAllPlugins" />
                <asp:AsyncPostBackTrigger ControlID="btn_defaultbodyfontfamily" />
                <asp:AsyncPostBackTrigger ControlID="lbtn_defaultbodyfontfamily_reset" />
                <asp:AsyncPostBackTrigger ControlID="btn_defaultfontsize" />
                <asp:AsyncPostBackTrigger ControlID="lbtn_defaultfontsize_clear" />
                <asp:AsyncPostBackTrigger ControlID="btn_defaultfontcolor" />
                <asp:AsyncPostBackTrigger ControlID="lbtn_defaultfontcolor_clear" />
                <asp:AsyncPostBackTrigger ControlID="btn_ProfileLinkStyle" />
                <asp:AsyncPostBackTrigger ControlID="rb_showdatetime_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showdatetime_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_HideSearchBarInTopBar_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_HideSearchBarInTopBar_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowSiteToolsInCategories_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowSiteToolsInCategories_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_linksnewpage_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_linksnewpage_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_SiteToolsIconOnly_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_SiteToolsIconOnly_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_showpagedescriptions_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_showpagedescriptions_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_AllowNavMenuCollapseToggle_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_AllowNavMenuCollapseToggle_off" />
                <asp:AsyncPostBackTrigger ControlID="hf_ColorOptions" />
                <asp:AsyncPostBackTrigger ControlID="rb_BoxedLayout_acctOptions" />
                <asp:AsyncPostBackTrigger ControlID="rb_WideLayout_acctOptions" />
                <asp:AsyncPostBackTrigger ControlID="rb_chatclient_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_chatclient_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_chatsoundnoti_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_chatsoundnoti_off" />
                <asp:AsyncPostBackTrigger ControlID="btn_updateintervals" />
                <asp:AsyncPostBackTrigger ControlID="rb_MobileAutoSync_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_MobileAutoSync_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowRowCountGridViewTable_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_ShowRowCountGridViewTable_off" />
                <asp:AsyncPostBackTrigger ControlID="rb_UseAlternateGridviewRows_on" />
                <asp:AsyncPostBackTrigger ControlID="rb_UseAlternateGridviewRows_off" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
