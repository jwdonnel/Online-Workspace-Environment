<%@ page title="Account Settings" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_AcctSettings, App_Web_lo5bbrui" clientidmode="Static" %>

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

        .group-img-edit
        {
            float: left;
            max-height: 24px;
            margin-top: -1px;
            margin-left: 0px;
            margin-right: 8px;
        }

        .adminpageedit
        {
            margin-right: 10px;
            width: 140px;
        }

        #pnl_overlayList .item-column
        {
            padding: 5px 10px;
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
            <div class="clear-space"></div>
            <asp:Panel ID="non_grouplogin" runat="server">
                Settings will depend on the user Role, apps, and
            groups. Certain features may be visible but not used.<br />
                For new users defaults, all overlays and notifications will be enabled by default
            (Depending on which apps are installed and user role).
            </asp:Panel>
            <asp:Panel ID="grouplogin" runat="server" Visible="false" Enabled="false">
                Users logging into this group will inherit all the account settings below. These settings will are only applied for users who are logged into the group session.
            </asp:Panel>
            <div class="clear-space"></div>
        </div>
        <asp:Literal ID="UserGroupLoginMessage" runat="server"></asp:Literal>
        <div class="clear-space">
        </div>
        <ul class="sitemenu-selection">
        </ul>
        <div class="clear-space">
        </div>

        <asp:Panel ID="pnl_UserInformation" CssClass="pnl-section" runat="server" data-title="Information">
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
                                            ValidationGroup="ChangeUserPasswordValidationGroup" />
                                        <div class="accountInfo">
                                            <asp:TextBox ID="CurrentPassword" runat="server" CssClass="textEntry" TextMode="Password" placeholder="Old Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPassword"
                                                CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Old Password is required."
                                                ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                            <div class="clear-space">
                                            </div>
                                            <asp:TextBox ID="NewPassword" runat="server" CssClass="textEntry" TextMode="Password" placeholder="New Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPassword"
                                                CssClass="failureNotification" ErrorMessage="New Password is required." ToolTip="New Password is required."
                                                ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                            <div class="clear-space">
                                            </div>
                                            <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="textEntry" TextMode="Password" placeholder="Confirm New Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword"
                                                CssClass="failureNotification" Display="Dynamic" ErrorMessage="Confirm New Password is required."
                                                ToolTip="Confirm New Password is required." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword"
                                                ControlToValidate="ConfirmNewPassword" CssClass="failureNotification" Display="Dynamic"
                                                ErrorMessage="The Confirm New Password must match the New Password entry." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
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
                                    <asp:TextBox ID="tb_firstname_accountsettings" runat="server" CssClass="textEntry" placeholder="First Name"></asp:TextBox>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="float-left">
                                    <asp:TextBox ID="tb_lastname_accountsettings" runat="server" CssClass="textEntry" placeholder="Last Name"></asp:TextBox>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="float-left">
                                    <asp:TextBox ID="tb_email" runat="server" CssClass="textEntry" placeholder="Email"></asp:TextBox>
                                    <div class="clear-space-five">
                                    </div>
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
                    <asp:AsyncPostBackTrigger ControlID="btn_clear_acctImage" />
                    <asp:AsyncPostBackTrigger ControlID="btn_markasnewuser" />
                </Triggers>
            </asp:UpdatePanel>
            <asp:Panel ID="pnl_acctImage" runat="server">
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Account Image
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:Image ID="imgAcctImage" runat="server" ImageUrl="~/Standard_Images/EmptyUserImg.png" CssClass="acct-image float-left margin-right margin-top-big" />
                        <div class="float-left pad-all-big margin-left-big">
                            <asp:FileUpload ID="fileUpload_acctImage" runat="server" />
                            <div class="clear-space"></div>
                            <asp:Button ID="btn_fileUpload_acctImage" runat="server" CssClass="input-buttons" Text="Upload" OnClick="btn_fileUpload_acctImage_Clicked" />
                            <asp:LinkButton ID="btn_clear_acctImage" runat="server" CssClass="RandomActionBtns" Text="Clear" OnClick="btn_clear_acctImage_Clicked" Style="font-size: 11px;" />
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
                    <div id="pnl_userColor" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                User Color
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="float-left">
                                    <asp:TextBox runat="server" ID="txt_userColor" CssClass="textEntry float-left margin-right color"
                                        MaxLength="6" AutoCompleteType="None" Width="75px" />
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
                    <asp:Panel ID="pnl_EnableRecieveAll" runat="server" Enabled="false" Visible="false">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Enable Receive All
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_receiveall_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_receiveall_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_receiveall_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_receiveall_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Disable this feature if you dont want emails to be sent to the given user through eRequests/Questions and Comments/Feedback.
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_userRoleAssign" runat="server" Enabled="false" Visible="false">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                User Role
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="dd_roles" runat="server" CssClass="margin-right">
                                </asp:DropDownList>
                                <asp:Button ID="btn_roles" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="dd_roles_Changed" Text="Update" />
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
                    <asp:AsyncPostBackTrigger ControlID="rb_receiveall_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_receiveall_off" />
                    <asp:AsyncPostBackTrigger ControlID="btn_roles" />
                    <asp:AsyncPostBackTrigger ControlID="btn_WorkspaceMode" />
                    <asp:AsyncPostBackTrigger ControlID="btn_updateusercolor" />
                    <asp:AsyncPostBackTrigger ControlID="btn_resetUserColor" />
                    <asp:AsyncPostBackTrigger ControlID="hf_DeleteUserAccount" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_NotificationSettings" CssClass="pnl-section" runat="server" data-title="Notifications">
            <asp:UpdatePanel ID="updatepnl_NotificationSettings" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="table-settings-box">
                        <div class="td-settings-ctrl">
                            <div class="float-left">
                                <asp:LinkButton ID="btn_DisableAll_notification" runat="server" Text="Disable All"
                                    ClientIDMode="Static" CssClass="margin-right-big RandomActionBtns" OnClick="btn_DisableAll_notification_Clicked"></asp:LinkButton>
                            </div>
                            <div class="float-right">
                                <span class="font-bold pad-right">Notifications Enabled</span><asp:Label ID="lbl_NotifiEnabled"
                                    runat="server" Text="0"></asp:Label>
                            </div>
                            <div class="clear-space">
                            </div>
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

        <asp:Panel ID="pnl_WorkspaceOverlays" CssClass="pnl-section" runat="server" data-title="Overlays">
            <asp:UpdatePanel ID="updatepnl_WorkspaceOverlays" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="table-settings-box">
                        <div class="td-settings-ctrl">
                            <div class="float-left">
                                <asp:LinkButton ID="btn_DisableAll_overlay" runat="server" Text="Disable All" ClientIDMode="Static"
                                    CssClass="RandomActionBtns" OnClick="btn_DisableAll_overlay_Clicked"></asp:LinkButton>
                            </div>
                            <div class="float-right">
                                <span class="font-bold pad-right">Overlays Enabled</span><asp:Label ID="lbl_overlaysEnabled"
                                    runat="server" Text="0"></asp:Label>
                            </div>
                            <div class="clear-space">
                            </div>
                            <asp:Panel ID="pnl_overlays" runat="server">
                            </asp:Panel>
                        </div>
                        <div class="td-settings-desc">
                            Workspace overlays show limited information. They are a non editable type of app. Position determines which side of the screen the overlay will move to. Set the Workspace dropdown to the display the overlay on. When disabling the overlay, it will clear out your current settings for that overlay and reset them back to the defaults.
                        </div>
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

        <asp:Panel ID="pnl_WorkspaceContainer" CssClass="pnl-section" runat="server" data-title="Workspace">
            <asp:UpdatePanel ID="updatepnl_WorkspaceContainer" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
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
                                <asp:TextBox ID="txt_AppGridSize" runat="server" CssClass="textEntry margin-right" MaxLength="3" Width="40px"></asp:TextBox>
                                <asp:Button ID="btn_AppGridSize" runat="server" CssClass="input-buttons RandomActionBtns" Text="Update" OnClick="btn_AppGridSize_Click" />
                            </div>
                            <div class="td-settings-desc">
                                Set the grid size of the workspace to move the app on. (Value must be greater than 0)
                            </div>
                        </div>
                    </asp:Panel>
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
                                <asp:TextBox ID="tb_autorotateinterval" runat="server" CssClass="textEntry" Width="45px"
                                    MaxLength="6"></asp:TextBox><span class="pad-left">seconds(s)</span>
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

        <asp:Panel ID="pnl_BackgroundEditor" CssClass="pnl-section" runat="server" data-title="Background">
            <asp:Panel ID="pnl_currentbackgroundselector" runat="server">
                <asp:UpdatePanel ID="updatepnl_BackgroundEditor" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Dasboard Background
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <table cellpadding="5px" cellspacing="5px">
                                    <tr>
                                        <td valign="top">
                                            <div id="CurrentBackground">
                                            </div>
                                            <div class="clear-space-two">
                                            </div>
                                            <div class="pad-left">
                                                <a href="#" onclick="BackgroundSelector();return false;" class="input-buttons margin-left-sml">Select a Background</a>
                                            </div>
                                            <div class="pad-top pad-bottom" style="padding-left: 14px;">
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
                                            <div class="pad-left-big margin-left">
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
                                <span class="font-color-black" id="backgroundsaved"></span>
                            </div>
                            <div class="td-settings-desc">
                                All backgrounds will repeat on the workspace.
                                Your connection speed will slow down with the larger images. Each image has
                                the size details when you hover over them. Solid color backgrounds will be the quickest
                                if you have a slower internet connection.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_backgroundurl" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Multiple Backgrounds
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
                                    Allows the use of multiple backgrounds. One for each workspace/workspace.
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_backgroundSelector" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Select Workspace
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="dd_backgroundSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="dd_backgroundSelector_Changed">
                                    </asp:DropDownList>
                                </div>
                                <div class="td-settings-desc">Select the workspace you want to change.</div>
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
                                                Click on the background that you would like to apply to your workspace.
                                                <div class="clear-space">
                                                </div>
                                                <asp:HiddenField ID="hf_backgroundselector" runat="server" OnValueChanged="hf_backgroundselector_ValueChanged"
                                                    Value="" />
                                                <div id="pnl_images" align="center">
                                                </div>
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

        <asp:Panel ID="pnl_TopSideMenuBar" CssClass="pnl-section" runat="server" data-title="Top/Side Menu Bar">
            <asp:UpdatePanel ID="updatepnl_TopSideMenuBar" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Date/Time
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_showdatetime_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_showdatetime_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_showdatetime_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_showdatetime_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Select Hide if you dont want to see the date/time in the top tool bar.
                        </div>
                    </div>
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
                    </asp:Panel>
                    <asp:Panel ID="pnl_autohidemode" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Auto Hide Mode
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_autohidemode_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_autohidemode_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_autohidemode_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_autohidemode_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Turn on this feature if you want to automatically hide the header and footer bars on the workspace.
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnl_nonadminsettings" runat="server">
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

        <asp:Panel ID="pnl_IconSelector" CssClass="pnl-section" runat="server" data-title="App Selector">
            <asp:UpdatePanel ID="updatepnl_IconSelector" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
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
                                        the ability to sort the app icons. (Only on Workspace)
                            </div>
                        </div>
                    </asp:Panel>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Group Icons
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
                            Enabling this will group the icons by category allowing for easier browsing.
                        </div>
                    </div>
                    <asp:Panel ID="pnl_categoryCount" runat="server" Enabled="false" Visible="false">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Icon Category Count
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
                    <asp:Panel ID="pnl_HideAppIcons" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Hide Icon Image
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_hideAppIcon_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_hideAppIcon_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_hideAppIcon_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_hideAppIcon_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Select Yes to hide all icons. This includes taskbar and app header.
                            </div>
                        </div>
                    </asp:Panel>
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

        <asp:Panel ID="pnl_SiteCustomizations" CssClass="pnl-section" runat="server" data-title="Customizations">
            <asp:UpdatePanel ID="updatepnl_SiteCustomizations" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
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
                    <asp:Panel ID="pnl_WorkspaceMode" runat="server" Enabled="false" Visible="false">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Workspace Mode
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:DropDownList ID="ddl_WorkspaceMode" runat="server" CssClass="margin-right">
                                    <asp:ListItem Value="Simple" Text="Page Based"></asp:ListItem>
                                    <asp:ListItem Value="Complex" Text="App Based"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:Button ID="btn_WorkspaceMode" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_WorkspaceMode_Click" Text="Update" />
                            </div>
                            <div class="td-settings-desc">
                                Switch to Simple mode if you do not like the use of the apps. This will turn your workspace into a more generic looking website.
                            </div>
                        </div>
                    </asp:Panel>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Animation Speed
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="float-left pad-right pad-top-sml">
                                <div id="Slider2" class="ajax__slider_h_rail">
                                </div>
                            </div>
                            <input id="btnUpdateAnimiation" type="button" class="input-buttons margin-left" value="Update" onclick="UpdateAnimationSpeed();" style="display: none;" />
                            <input type="button" class="input-buttons margin-left" value="Reset" onclick="ResetAnimationSpeed();" />
                            <asp:HiddenField ID="hf_AnimationSpeed" runat="server" ClientIDMode="Static" OnValueChanged="hf_AnimationSpeed_Changed" />
                            <div id="currentAnimationSpeed">
                            </div>
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
                            Site Tips
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
                            Select Off if you dont want to see any site tool tips.
                        </div>
                    </div>
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
                    <asp:Panel ID="pnl_presentationMode" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Presentation Mode
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_presentationmode_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_presentationmode_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_presentationmode_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_presentationmode_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Turn on this feature if you want to automatically hide any app, header, footer and background controls.
                            </div>
                        </div>
                    </asp:Panel>
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
                            </div>
                            <div id="lbl_appmodalstyle_desc" runat="server" class="td-settings-desc">
                                Change the look of your app window and modal popups.
                            </div>
                        </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_tooltips_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_tooltips_on" />
                    <asp:AsyncPostBackTrigger ControlID="hf_AnimationSpeed" />
                    <asp:AsyncPostBackTrigger ControlID="btn_UpdateTheme" />
                    <asp:AsyncPostBackTrigger ControlID="btn_appStyle" />
                    <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_clearproperties_on" />
                    <asp:AsyncPostBackTrigger ControlID="btn_clearapps" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showHeader_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_showHeader_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_AppHeaderIcon_off" />
                    <asp:AsyncPostBackTrigger ControlID="rb_presentationmode_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_presentationmode_off" />
                    <asp:AsyncPostBackTrigger ControlID="hf_addPlugin" />
                    <asp:AsyncPostBackTrigger ControlID="hf_removePlugin" />
                    <asp:AsyncPostBackTrigger ControlID="hf_removeAllPlugins" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:Panel ID="pnl_ChatClient" CssClass="pnl-section" runat="server" data-title="Chat Client">
            <asp:UpdatePanel ID="updatepnl_ChatClient" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
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
                                <asp:TextBox ID="tb_updateintervals" runat="server" CssClass="textEntry" Width="35px"
                                    MaxLength="3"></asp:TextBox><span class="pad-left">minute(s)</span>
                                <asp:Button ID="btn_updateintervals" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                    Text="Update" OnClick="btn_updateintervals_Click" />
                            </div>
                            <div class="td-settings-desc">
                                This value will represent the amount of time of inactivity before your chat status
                                        turns to away. (Default is 10 minutes)
                            </div>
                        </div>
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
