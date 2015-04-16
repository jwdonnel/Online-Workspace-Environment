<%@ page title="User Accounts" async="true" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_UserAccounts, App_Web_lo5bbrui" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .ui-autocomplete
        {
            max-height: 400px;
        }

        .sch_ColorCode
        {
            top: 3px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div id="useraccounts">
            <asp:Panel ID="pnl_admin_tools" runat="server" CssClass="float-left">
                <a href="#" class="float-left input-buttons" onclick="ClearNewUserFields();openWSE.LoadModalWindow(true, 'NewUser-element', 'Create New User');return false;">
                    <span class="td-add-btn float-left margin-right-sml" style="padding: 0!important;"></span>Create a User</a>
                <div id="customizeBtns" class="float-left">
                    <a id="btn_customizeua" href="#iframecontent" class="display-inline input-buttons" onclick="openWSE.LoadIFrameContent('SiteTools/UserMaintenance/AcctSettings.aspx?toolview=true&u=NewUserDefaults', this);return false;">
                        <span class="img-customize margin-right-sml float-left"></span>Customize New Users</a>
                    <a id="btn_appsusers" href="#iframecontent" class="input-buttons"
                        onclick="openWSE.LoadIFrameContent('SiteTools/iframes/UsersAndApps.aspx', this);return false;">
                        <span class="img-app-dark margin-right-sml float-left"></span>User Apps and Plugins</a>
                </div>
                <div class="float-left">
                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                        <ContentTemplate>
                            <asp:LinkButton ID="btn_manageRoles" runat="server" CssClass="RandomActionBtns input-buttons"
                                OnClick="btn_manageRoles_Click"><span class="td-edit-btn float-left margin-right-sml" style="padding: 0!important;"></span>Manage Custom Roles</asp:LinkButton>
                            <asp:LinkButton ID="btn_rebuild_uc" runat="server" CssClass="RandomActionBtns input-buttons"
                                OnClick="btn_rebuild_uc_Clicked"><span class="img-refresh float-left margin-right-sml"></span>Rebuild Users</asp:LinkButton>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div id="ManageRoles-element" class="Modal-element">
                    <div class="Modal-overlay">
                        <div class="Modal-element-align">
                            <div class="Modal-element-modal" data-setwidth="510">
                                <div class="ModalHeader">
                                    <div>
                                        <div class="app-head-button-holder-admin">
                                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'ManageRoles-element', '');return false;"
                                                class="ModalExitButton"></a>
                                        </div>
                                        <span class="Modal-title"></span>
                                    </div>
                                </div>
                                <div class="ModalScrollContent">
                                    <div class="ModalPadContent">
                                        <asp:UpdatePanel ID="UpdatePanel21" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <asp:Panel ID="pnl_createCustomRole" runat="server" DefaultButton="btn_NewRoleName" Visible="false" Enabled="false">
                                                    Create a new role for new and existing users. After you finish creating your new role,<br />
                                                    you can go to the Customize New Users link to customize the role.
                                                <div class="clear-space">
                                                </div>
                                                    <table cellpadding="0" cellspacing="0" border="0">
                                                        <tr>
                                                            <td><span class="font-bold pad-right">Role Name</span></td>
                                                            <td>
                                                                <asp:TextBox ID="tb_NewRoleName" runat="server" CssClass="textEntry margin-right" Width="150px" MaxLength="25"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:Button ID="btn_NewRoleName" runat="server" OnClick="btn_NewRoleName_Click" CssClass="input-buttons no-margin RandomActionBtns" Text="Create" Style="margin-right: 5px!important;" />
                                                                <asp:Button ID="btn_CancelNewRole" runat="server" OnClick="btn_CancelNewRole_Click" CssClass="input-buttons no-margin RandomActionBtns" Text="Cancel" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div class="clear-space"></div>
                                                    <asp:Label ID="lbl_NewRoleNameError" runat="server" ForeColor="Red"></asp:Label>
                                                    <div class="clear-space"></div>
                                                </asp:Panel>
                                            </ContentTemplate>
                                            <Triggers>
                                                <asp:AsyncPostBackTrigger ControlID="btn_NewRoleName" />
                                                <asp:AsyncPostBackTrigger ControlID="btn_CancelNewRole" />
                                            </Triggers>
                                        </asp:UpdatePanel>
                                        <asp:UpdatePanel ID="UpdatePanel22" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <asp:Panel ID="pnl_manageRoles" runat="server" DefaultButton="btn_roleEditUpdate">
                                                    Manage your custom user roles here. Custom roles will have the Standard Role applied to it.<br />
                                                    Deleting a custom role will move all users in that role to the Standard role.
                                                <div class="clear-space">
                                                </div>
                                                    <table cellpadding="0" cellspacing="0" border="0">
                                                        <tr>
                                                            <td><span class="font-bold pad-right">Role Name</span></td>
                                                            <td>
                                                                <asp:DropDownList ID="ddl_roleNameSelect" runat="server" CssClass="margin-right"></asp:DropDownList>
                                                                <asp:TextBox ID="tb_roleNameEdit" runat="server" CssClass="textEntry margin-right-big" Width="150px" MaxLength="25" Visible="false" Enabled="false"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:LinkButton ID="btn_roleEdit" runat="server" OnClick="btn_roleEdit_Click" CssClass="td-edit-btn RandomActionBtns margin-right-sml" Text="" />
                                                                <asp:LinkButton ID="btn_roleDelete" runat="server" OnClientClick="return ConfirmDeleteRole(this);" CssClass="td-delete-btn RandomActionBtns" Text="" />
                                                                <asp:Button ID="btn_roleEditUpdate" runat="server" OnClick="btn_roleEditUpdate_Click" CssClass="input-buttons no-margin RandomActionBtns" Text="Update" Visible="false" Enabled="false" Style="margin-right: 5px!important;" />
                                                                <asp:Button ID="btn_roleEditCancel" runat="server" OnClick="btn_roleEditCancel_Click" CssClass="input-buttons no-margin RandomActionBtns" Text="Cancel" Visible="false" Enabled="false" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <asp:LinkButton ID="lbtn_CreateNewRole" runat="server" OnClick="lbtn_CreateNewRole_Click" CssClass="RandomActionBtns margin-left input-buttons float-right" Style="margin-top: -25px;">Create New Role</asp:LinkButton>
                                                    <div class="clear-space"></div>
                                                    <asp:Label ID="lbl_roleEditError" runat="server" ForeColor="Red"></asp:Label>
                                                    <div class="clear-space"></div>
                                                </asp:Panel>
                                            </ContentTemplate>
                                            <Triggers>
                                                <asp:AsyncPostBackTrigger ControlID="btn_roleEdit" />
                                                <asp:AsyncPostBackTrigger ControlID="btn_roleDelete" />
                                                <asp:AsyncPostBackTrigger ControlID="btn_roleEditUpdate" />
                                                <asp:AsyncPostBackTrigger ControlID="btn_roleEditCancel" />
                                                <asp:AsyncPostBackTrigger ControlID="lbtn_CreateNewRole" />
                                            </Triggers>
                                        </asp:UpdatePanel>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </asp:Panel>
            <div id="ddlPagesize_holder" class="float-right">
                <span class="font-bold pad-right">Page Size</span>
                <asp:DropDownList ID="ddl_pageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddl_pageSize_Changed">
                    <asp:ListItem Text="3" Value="3"></asp:ListItem>
                    <asp:ListItem Text="5" Value="5"></asp:ListItem>
                    <asp:ListItem Text="10" Value="10" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="15" Value="15"></asp:ListItem>
                    <asp:ListItem Text="20" Value="20"></asp:ListItem>
                    <asp:ListItem Text="All" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="clear-space"></div>
            <div class="float-right">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <span class="font-bold pad-right">Sort By</span>
                        <asp:DropDownList ID="ddl_sortby" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddl_sort_Changed">
                            <asp:ListItem Text="Username (Ascending)" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Username (Descending)" Value="2"></asp:ListItem>
                            <asp:ListItem Text="Email (Ascending)" Value="3"></asp:ListItem>
                            <asp:ListItem Text="Email (Descending)" Value="4"></asp:ListItem>
                            <asp:ListItem Text="Date Joined (Ascending)" Value="5"></asp:ListItem>
                            <asp:ListItem Text="Date Joined (Descending)" Value="6"></asp:ListItem>
                        </asp:DropDownList>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="clear" style="height: 30px">
            </div>
            <div class="clear-margin">
                <asp:Label ID="lbl_totalUsers" runat="server"></asp:Label>
            </div>
            <div id="UserList">
                <asp:UpdatePanel ID="updatepnl_Users" runat="server">
                    <ContentTemplate>
                        <div class="float-left">
                            <div id="searchwrapper">
                                <asp:Panel ID="Panel1_usersearch" runat="server" DefaultButton="imgbtn_search">
                                    <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                        onfocus="if(this.value=='Search Users')this.value=''" onblur="if(this.value=='')this.value='Search Users'"
                                        Text="Search Users"></asp:TextBox>
                                    <a href="#" onclick="return false;" class="searchbox_clear" title="Clear search"></a>
                                    <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                        OnClick="imgbtn_search_Click" />
                                </asp:Panel>
                            </div>
                        </div>
                        <asp:HiddenField ID="hf_clearsearch" runat="server" ClientIDMode="Static" OnValueChanged="hf_clearsearch_Changed" />
                        <div class="float-right margin-left-big pad-top">
                            <small>Leave search textbox blank to search all users</small>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_Users" runat="server">
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <asp:HiddenField ID="hf_deleteUser" runat="server" OnValueChanged="hf_deleteUser_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_lockUser" runat="server" OnValueChanged="hf_lockUser_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_resetPassword" runat="server" OnValueChanged="hf_resetPassword_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_emailUser" runat="server" OnValueChanged="hf_emailUser_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_noemailUser" runat="server" OnValueChanged="hf_noemailUser_Changed"
                            ClientIDMode="Static" />
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddl_pageSize" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div id="NewUser-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal add-user-modal" data-setwidth="570">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewUser-element', '');ClearNewUserFields();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_NewUser" runat="server">
                                    <ContentTemplate>
                                        <asp:HiddenField ID="hf_creatingNewUser" runat="server" Value="false" />
                                        <asp:CreateUserWizard ID="RegisterUser" runat="server" OnCreatedUser="RegisterUser_CreatedUser"
                                            OnCreatingUser="RegisterUser_CreatingUser" LoginCreatedUser="false" Width="100%">
                                            <LayoutTemplate>
                                                <asp:PlaceHolder ID="wizardStepPlaceholder" runat="server"></asp:PlaceHolder>
                                                <asp:PlaceHolder ID="navigationPlaceholder" runat="server"></asp:PlaceHolder>
                                            </LayoutTemplate>
                                            <WizardSteps>
                                                <asp:CreateUserWizardStep ID="RegisterUserWizardStep" runat="server">
                                                    <ContentTemplate>
                                                        <div style="padding: 5px 0;">
                                                            <p style="font-size: 12px;">
                                                                Passwords are required to be a minimum of
                                                        <%= Membership.MinRequiredPasswordLength %>
                                                        characters in length. <b class="pad-right-sml">Note:</b>Users must be setup manually
                                                        through the edit button. The new users will not be able to access any features until
                                                        the user is assigned to a group.
                                                            </p>
                                                        </div>
                                                        <table class="tableLogin">
                                                            <tbody>
                                                                <tr>
                                                                    <td valign="top" width="265px">
                                                                        <asp:TextBox ID="UserName" runat="server" CssClass="textEntryReg" Width="215px" placeholder="Username"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                            CssClass="failureNotification" ErrorMessage="" ToolTip="User Name is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="textEntryReg" Width="200px" placeholder="First Name"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                        <asp:TextBox ID="Email" runat="server" CssClass="textEntryReg" Width="215px" placeholder="Email"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                                            CssClass="failureNotification" ErrorMessage="" ToolTip="E-mail is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                        <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="textEntryReg" Width="200px" placeholder="Last Name"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                        <asp:TextBox ID="Password" runat="server" CssClass="textEntryReg" TextMode="Password"
                                                                            Width="215px" placeholder="Password"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                                            CssClass="failureNotification" ErrorMessage="" ToolTip="Password is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                        <asp:Label ID="lbl_role" runat="server" CssClass="pad-right" Style="color: #555;" AssociatedControlID="dd_role">User Role</asp:Label>
                                                                        <asp:DropDownList ID="dd_role" runat="server">
                                                                        </asp:DropDownList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                        <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="textEntryReg" TextMode="Password"
                                                                            Width="215px" placeholder="Confirm Password"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                                            Display="Dynamic" ErrorMessage="" ID="ConfirmPasswordRequired" runat="server"
                                                                            ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                        <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                                            ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                                                            ErrorMessage="" ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <div class="clear-space"></div>
                                                                        <span style="color: #555;">User Color</span>
                                                                        <div class="clear-space-two"></div>
                                                                        <span class="img-colors float-left margin-right-sml" style="margin-top: -2px"></span>
                                                                        <asp:TextBox runat="server" ID="Color1" MaxLength="6" CssClass="textEntryReg float-left color"
                                                                            AutoCompleteType="None" Width="75px" />
                                                                        <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup"
                                                                            CssClass="input-buttons margin-left float-right continue-create-user" Text="Create"></asp:Button>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                        <div class="clear-margin">
                                                            <span class="failureNotification">
                                                                <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                                                            </span>
                                                            <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
                                                                ValidationGroup="RegisterUserValidationGroup" />
                                                        </div>
                                                        <div class="clear-space"></div>
                                                    </ContentTemplate>
                                                    <CustomNavigationTemplate>
                                                    </CustomNavigationTemplate>
                                                </asp:CreateUserWizardStep>
                                                <asp:CompleteWizardStep>
                                                    <ContentTemplate>
                                                        <div class="clear-space-five">
                                                        </div>
                                                        Account has been created. Click continue to refresh page.
                                                        <div class="clear-space"></div>
                                                        <asp:Button ID="ContinueCreateUserButton" runat="server" OnClick="RegisterUser_Continue"
                                                            CssClass="input-buttons RandomActionBtns margin-bottom float-right" Text="Continue"
                                                            Style="margin-top: -4px;"></asp:Button>
                                                        <div class="clear">
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:CompleteWizardStep>
                                            </WizardSteps>
                                        </asp:CreateUserWizard>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <a href="#" style="font-size: 12px;" onclick="CreateMultipleUsers();return false;">Click Here to Create Multiple Users</a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="Multiple-User-Create-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="570">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'Multiple-User-Create-element', '');ClearMultiUserFields();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div class="pad-bottom">
                                    <small><b class="pad-right-sml">Note:</b>User colors will be automatically generated for each user. The username will also be used as the name. If a space is in the username, the first name will be assigned to the first word of the username and the last word will be assigned to the last name.</small>
                                </div>
                                <table>
                                    <tbody>
                                        <tr>
                                            <td class="pad-right-big" valign="top">
                                                <asp:TextBox ID="txt_multiUser_email" runat="server" CssClass="textEntryReg" Width="215px" placeholder="Email"></asp:TextBox>
                                                <div class="clear-space"></div>
                                            </td>
                                            <td valign="top">
                                                <span style="color: #555;">Select a User Role</span><br />
                                                <asp:DropDownList ID="dd_role_multiUser" runat="server" Width="120px">
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td valign="top">
                                                <asp:TextBox ID="txt_multiUser_password" TextMode="Password" runat="server" CssClass="textEntryReg" Width="215px" placeholder="Password"></asp:TextBox>
                                                <span id="multiuserpasswordrequired" title="Password is required." class="failureNotification" style="display: none;">*</span>
                                            </td>
                                            <td valign="top">
                                                <asp:TextBox ID="txt_multiUser_confirmpassword" TextMode="Password" runat="server" CssClass="textEntryReg" Width="215px" placeholder="Confirm Password"></asp:TextBox>
                                                <span id="multiuserconfirmpasswordrequired" title="Confirm Password is required." class="failureNotification" style="display: none;">*</span>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div class="clear-space"></div>
                                <span style="color: #555;">User List</span>
                                <span id="userlistrequired" title="At least one user is required." class="failureNotification pad-left-sml" style="display: none;">*</span>
                                <div id="multiusercreate"></div>
                                <input type="button" class="input-buttons float-right" value="Create Users" onclick="FinishCreateMultiUsers()" style="margin-top: -27px;" />
                                <div class="clear-space"></div>
                                <a href="#" style="font-size: 12px;" onclick="CancelMultipleUsers();return false;">Cancel Multiple User Creation</a>
                                <div class="clear-space-five"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="pwreset_overlay" class="Modal-element" runat="server" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="350">
                        <asp:UpdatePanel ID="updatepnl_pwreset" runat="server">
                            <ContentTemplate>
                                <asp:HiddenField ID="hf_createMultiUsers" runat="server" OnValueChanged="hf_createMultiUsers_ValueChanged" />
                                <div class="ModalHeader">
                                    <div>
                                        <div class="app-head-button-holder-admin">
                                            <asp:LinkButton ID="btn_closepwreset" runat="server" Text="" OnClick="btn_closepwreset_Click"
                                                CssClass="ModalExitButton RandomActionBtns" OnClientClick="$('#MainContent_pwreset_overlay').hide();" />
                                        </div>
                                        <span class="Modal-title">Password Reset</span>
                                    </div>
                                </div>
                                <div class="ModalScrollContent">
                                    <div class="ModalPadContent">
                                        <asp:Literal ID="lbl_passwordReset" runat="server"></asp:Literal>
                                        <asp:ChangePassword ID="ChangeUserPassword" runat="server" EnableViewState="false"
                                            RenderOuterTable="false">
                                            <ChangePasswordTemplate>
                                                <asp:Panel ID="pnl_changePassword" runat="server" DefaultButton="ChangePasswordPushButton_accountsettings">
                                                    <div class="failureNotification clear-margin" style="width: 295px;">
                                                        <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                                                    </div>
                                                    <asp:ValidationSummary ID="ChangeUserPasswordValidationSummary" runat="server" CssClass="failureNotification"
                                                        ValidationGroup="ChangeUserPasswordValidationGroup" Style="padding-left: 18px" />
                                                    <div class="accountInfo">
                                                        <asp:TextBox ID="CurrentPassword" runat="server" Visible="false"></asp:TextBox>
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
                                                            ErrorMessage="Confirm New Password must match the New Password." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
                                                        <div class="clear-space">
                                                        </div>
                                                        <div class="clear-space-five">
                                                        </div>
                                                        <asp:Button ID="ChangePasswordPushButton_accountsettings" runat="server" OnClick="ChangePasswordPushButton_accountsettings_Clicked"
                                                            Text="Change Password" ValidationGroup="ChangeUserPasswordValidationGroup" CssClass="input-buttons" OnClientClick="ShowPasswordChangeLoadingMessage();" />
                                                    </div>
                                                </asp:Panel>
                                            </ChangePasswordTemplate>
                                        </asp:ChangePassword>
                                        <asp:Literal ID="txt_PasswordFinishedText" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/WebControls/jscolor/jscolor.js")%>'></script>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/useraccounts.js")%>'></script>
    </div>
</asp:Content>
