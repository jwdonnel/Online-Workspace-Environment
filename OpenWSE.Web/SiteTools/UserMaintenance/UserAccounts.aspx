<%@ Page Title="User Accounts" Async="true" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="UserAccounts.aspx.cs" Inherits="SiteTools_UserAccounts" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            User Accounts
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <div id="pnl_useraccounts" class="pnl-section">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <a href="#" id="btn_CreateNewUsers" runat="server" class="float-left input-buttons-create" onclick="ClearNewUserFields();openWSE.LoadModalWindow(true, 'NewUser-element', 'Create New User');return false;">Create User</a>
                <asp:Panel ID="pnl_admin_tools" runat="server" CssClass="float-right">
                    <div class="float-right">
                        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                            <ContentTemplate>
                                <asp:LinkButton ID="btn_rebuild_uc" runat="server" CssClass="RandomActionBtns margin-right-big margin-bottom"
                                    OnClick="btn_rebuild_uc_Clicked">Rebuild Users</asp:LinkButton>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </asp:Panel>
                <div class="clear-space"></div>
                <asp:UpdatePanel ID="updatepnl_Users" runat="server">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_Users" runat="server">
                        </asp:Panel>
                        <div class="clear"></div>
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
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <div id="pnl_userappsandplugins" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatepnl_UsersAppsAndPlugins" runat="server">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_UserAppsAndPluginList" runat="server">
                        </asp:Panel>
                        <asp:HiddenField ID="hf_addApp" runat="server" ClientIDMode="Static" OnValueChanged="hf_addApp_Changed" />
                        <asp:HiddenField ID="hf_removeApp" runat="server" ClientIDMode="Static" OnValueChanged="hf_removeApp_Changed" />
                        <asp:HiddenField ID="hf_removeAllApp" runat="server" ClientIDMode="Static" OnValueChanged="hf_removeAllApp_Changed" />
                        <asp:HiddenField ID="hf_appPackage" runat="server" ClientIDMode="Static" OnValueChanged="hf_appPackage_Changed" />
                        <asp:HiddenField ID="hf_addPlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_addPlugin_ValueChanged" />
                        <asp:HiddenField ID="hf_removePlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_removePlugin_ValueChanged" />
                        <asp:HiddenField ID="hf_removeAllPlugins" runat="server" ClientIDMode="Static" OnValueChanged="hf_removeAllPlugins_ValueChanged" />
                        <div class="clear-space"></div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:UpdatePanel ID="updatepnl_UserAppsAndPlugins_Edit" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:HiddenField ID="hf_editUserApps" runat="server" ClientIDMode="Static" OnValueChanged="hf_editUserApps_Changed" />
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="hf_editUserApps" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                This tab will show the current installed user apps and plugins. If viewing individual users, you can add and remove apps and plugins or install an app package.
            </div>
        </div>
    </div>

    <div id="pnl_usersettings" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div id="rb_usersignup_holder" runat="server">
                    <div id="userAccountsAccordion" class="custom-accordion">
                        <h3>User Customizations</h3>
                        <div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    User Customizations
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <a id="btn_customizeua" href="#iframecontent" class="input-buttons" onclick="openWSE.LoadIFrameContent('SiteTools/UserTools/AcctSettings.aspx?u=NewUserDefaults&iframeMode=true');return false;">Customize New Users</a>
                                    <div class="clear"></div>
                                </div>
                                <div class="td-settings-desc">
                                    Customize new users based on their role.
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    User Roles
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:LinkButton ID="btn_manageRoles" runat="server" CssClass="RandomActionBtns input-buttons"
                                        OnClick="btn_manageRoles_Click">Manage Custom Roles</asp:LinkButton>
                                    <div class="clear"></div>
                                </div>
                                <div class="td-settings-desc">
                                    Edit and create new custom roles to be assigned to new and current users.
                                </div>
                            </div>
                        </div>
                        <h3>Sign Up Settings</h3>
                        <div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Allow User Signup
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_allowusersignup_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_allowusersignup_on_Checked" AutoPostBack="true" />
                                        <asp:RadioButton ID="rb_allowusersignup_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_allowusersignup_off_Checked" AutoPostBack="true" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    Select On if you want to allow users to sign up for a new account on the login
                                                        screen.
                                </div>
                            </div>
                            <asp:Panel ID="pnl_UserSignUp" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Email Association
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_emailassociation_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_emailassociation_on_Checked" AutoPostBack="true" />
                                            <asp:RadioButton ID="rb_emailassociation_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_emailassociation_off_Checked" AutoPostBack="true" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Select On if you want to limit the user sign up to be associated with a certain
                                                            email address.
                                    </div>
                                </div>
                                <asp:Panel ID="pnl_emailassociation" runat="server" DefaultButton="btn_UpdateEmailAssociation">
                                    <div class="table-settings-box">
                                        <div class="td-settings-title">
                                            Email Address
                                        </div>
                                        <div class="title-line"></div>
                                        <div class="td-settings-ctrl">
                                            <b>@</b><asp:TextBox ID="tb_EmailAssociation" runat="server" CssClass="textEntry margin-left margin-right"
                                                Width="150px"></asp:TextBox><div class="clear-space-five">
                                                </div>
                                            <asp:Button ID="btn_UpdateEmailAssociation" runat="server" Text="Update" CssClass="input-buttons RandomActionBtns"
                                                OnClick="btn_UpdateEmailAssociation_Click" Style="margin-left: 22px" />
                                            <div id="emailassociation_error" style="color: Red; padding-left: 25px">
                                            </div>
                                        </div>
                                        <div class="td-settings-desc">
                                            Email address that can only be used during sign up. (Email is validated with
                                                            an activation link)
                                        </div>
                                    </div>
                                </asp:Panel>
                            </asp:Panel>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    User Sign Up Role
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:DropDownList ID="dd_usersignuprole" runat="server" CssClass="margin-right">
                                    </asp:DropDownList>
                                    <asp:Button ID="btn_usersignuprole" runat="server" OnClick="dd_usersignuprole_Changed" CssClass="input-buttons RandomActionBtns" Text="Update" />
                                </div>
                                <div class="td-settings-desc">
                                    Select an initial role for the user registering an account. (To create new roles, go to User Accounts and select Manage Roles at the top)
                                </div>
                            </div>
                            <asp:Panel ID="pnl_ConfirmationEmailSignUp" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Send Confirmation Email
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_SignUpConfirmationEmail_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_SignUpConfirmationEmail_on_Checked" AutoPostBack="true" />
                                            <asp:RadioButton ID="rb_SignUpConfirmationEmail_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_SignUpConfirmationEmail_off_Checked" AutoPostBack="true" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Select Yes if you want to send a confirmation email to the user to verify email. Emails will only be sent if creating new account (Not signing in through other service).
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                        <h3>Login Settings</h3>
                        <div>
                            <asp:Panel ID="pnlLoginMessage" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Login Page Message
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <asp:TextBox ID="tb_loginPageMessage" runat="server" CssClass="textEntry float-left"
                                            Width="100%" AutoPostBack="False" TextMode="MultiLine" Height="75px" Font-Names='"Arial"'
                                            BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                            ForeColor="#353535"></asp:TextBox>
                                        <div class="clear-space"></div>
                                        <asp:Button ID="btn_loginPageMessage" runat="server" CssClass="no-margin RandomActionBtns input-buttons float-left"
                                            Text="Update" OnClick="btn_loginPageMessage_Click" />
                                        <asp:LinkButton ID="lbtn_loginPageMessage" runat="server" Font-Size="Smaller" CssClass="RandomActionBtns float-left margin-left-big"
                                            OnClick="lbtn_loginPageMessage_clear_Click" Style="margin-top: 5px;">Clear</asp:LinkButton>
                                        <div class="clear"></div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Add a note for all users to see on the Login Page.
                                                    <asp:Label ID="lbl_loginMessageDate" runat="server" Text="N/A"></asp:Label>
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Force Group Login
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_ForceGroupLogin_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_ForceGroupLogin_on_CheckedChanged" AutoPostBack="True" />
                                        <asp:RadioButton ID="rb_ForceGroupLogin_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_ForceGroupLogin_off_CheckedChanged" AutoPostBack="True" />
                                    </div>
                                </div>
                                <div class="td-settings-desc">
                                    You can force users to login to a group by enabling this feature. Enabling this will force users to use the default login page. (No preview or demo users allowed)
                                </div>
                            </div>
                            <asp:Panel ID="pnl_showpreviewbutton" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Show Preview Button on Login Screen
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_ShowPreviewButtonLogin_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_ShowPreviewButtonLogin_on_CheckedChanged" AutoPostBack="True" />
                                            <asp:RadioButton ID="rb_ShowPreviewButtonLogin_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_ShowPreviewButtonLogin_off_CheckedChanged" AutoPostBack="True" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Select Show to show the preview button on the login screen.
                                                    (Disabled by default)
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnl_nologinrequired" runat="server">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        No Login Required
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_nologinrequired_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_nologinrequired_on_CheckedChanged" AutoPostBack="True" />
                                            <asp:RadioButton ID="rb_nologinrequired_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_nologinrequired_off_CheckedChanged" AutoPostBack="True" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Set to No to make the site available to anyone without
                                                an account.
                                    </div>
                                </div>
                                <asp:Panel ID="pnl_showloginmodalondemomode" runat="server">
                                    <div class="table-settings-box">
                                        <div class="td-settings-title">
                                            Show Login Modal on Page Load (No Login/Demo Mode)
                                        </div>
                                        <div class="title-line"></div>
                                        <div class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_ShowLoginModalOnDemoMode_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_ShowLoginModalOnDemoMode_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_ShowLoginModalOnDemoMode_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_ShowLoginModalOnDemoMode_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </div>
                                        <div class="td-settings-desc">
                                            Set to Yes to force the login modal window on page load. This will only apply when user is attempting to access the Workspace on No Login/Demo mode.
                                        </div>
                                    </div>
                                </asp:Panel>
                            </asp:Panel>
                            <asp:Panel ID="pnl_NoLoginMainPage" runat="server" Enabled="false" Visible="false">
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        No Login/Demo Customizations
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <a href="#iframecontent" class="input-buttons float-left" onclick="openWSE.LoadIFrameContent('SiteTools/UserTools/AcctSettings.aspx?u=demouser&iframeMode=true');return false;"
                                            style="display: block;">Customize Demo User</a>
                                        <div class="clear"></div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Customize the demo user settings.
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                        <h3 id="pnl_socialSignIn_UserSignup_tab" runat="server" visible="false">Social Login APIs</h3>
                        <div id="pnl_socialSignIn_UserSignup" runat="server" visible="false">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Google+ Login
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_googlePlusSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_googlePlusSignIn_on_Checked" AutoPostBack="true" />
                                        <asp:RadioButton ID="rb_googlePlusSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_googlePlusSignIn_off_Checked" AutoPostBack="true" />
                                    </div>
                                    <div class="clear"></div>
                                    <asp:Panel ID="pnl_googleSettings" CssClass="pnl-section-child margin-top-big pad-top-big" runat="server" Enabled="false" Visible="false">
                                        <h3 class="font-bold">
                                            <img id="google_logo" alt="" src="~/Standard_Images/ApiLoginImages/google_login.png" runat="server" class="float-left margin-right-sml" style="height: 17px;" />Google OAuth 2.0 Settings</h3>
                                        <div class="clear-space-two"></div>
                                        <small>Your Google OAuth 2.0 settings. Keep the "Client Secret" a secret. This key
                                    should never be human-readable. Make sure your redirect url(s) match your Google App.</small>
                                        <div class="clear-space"></div>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_GoogleClientId" runat="server" DefaultButton="btn_updateGoogleClientId">
                                            <span class="font-bold">Client ID</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="txt_GoogleClientId" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateGoogleClientId" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateGoogleClientId_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_GoogleClientSecret" runat="server" DefaultButton="btn_updateGoogleClientSecret">
                                            <span class="font-bold">Client Secret</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="txt_GoogleClientSecret" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateGoogleClientSecret" runat="server" CssClass="RandomActionBtns input-buttons" Text="Update" OnClick="btn_updateGoogleClientSecret_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <span class="font-bold">Redirect Url(s)</span>
                                        <div class="clear-space-two"></div>
                                        <asp:Literal ID="ltl_googleRedirect" runat="server"></asp:Literal>
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Allow users to login using their Google+ accounts. <a href="https://developers.google.com/+/quickstart/csharp" target="_blank">Integration Information</a>
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Twitter Login
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_twitterSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_twitterSignIn_on_Checked" AutoPostBack="true" />
                                        <asp:RadioButton ID="rb_twitterSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_twitterSignIn_off_Checked" AutoPostBack="true" />
                                    </div>
                                    <div class="clear"></div>
                                    <asp:Panel ID="pnl_twitterSettings" CssClass="pnl-section-child margin-top-big pad-top-big" runat="server" Enabled="false" Visible="false">
                                        <h3 class="font-bold">
                                            <img id="twitter_logo" alt="" src="~/Standard_Images/ApiLoginImages/twitter_login.png" runat="server" class="float-left margin-right-sml" style="height: 17px;" />Twitter OAuth Settings</h3>
                                        <div class="clear-space-two"></div>
                                        <small>Your application's OAuth settings. Keep the "Access Token Secret" and "Consumer Secret" a secret. This key
                                should never be human-readable.</small>
                                        <div class="clear-space">
                                        </div>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_TwitterAccessToken" runat="server" DefaultButton="btn_updateTwitterAccessToken">
                                            <span class="font-bold">Access Token</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_updateTwitterAccessToken" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateTwitterAccessToken" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateTwitterAccessToken_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_TwitterAccessTokenSecret" runat="server" DefaultButton="btn_updateTwitterAccessTokenSecret">
                                            <span class="font-bold">Access Token Secret</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_updateTwitterAccessTokenSecret" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateTwitterAccessTokenSecret" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateTwitterAccessTokenSecret_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_TwitterConsumerKey" runat="server" DefaultButton="btn_updateTwitterConsumerKey">
                                            <span class="font-bold">Consumer Key</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_updateTwitterConsumerKey" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateTwitterConsumerKey" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateTwitterConsumerKey_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_TwitterConsumerSecret" runat="server" DefaultButton="btn_updateTwitterConsumerSecret">
                                            <span class="font-bold">Consumer Secret</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_updateTwitterConsumerSecret" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateTwitterConsumerSecret" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateTwitterConsumerSecret_Click" />
                                        </asp:Panel>
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Allow users to login using their Twitter accounts. <a href="https://dev.twitter.com/web/sign-in/implementing" target="_blank">Integration Information</a>
                                </div>
                            </div>
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Facebook Login
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <div class="field switch inline-block">
                                        <asp:RadioButton ID="rb_facebookSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                            OnCheckedChanged="rb_facebookSignIn_on_Checked" AutoPostBack="true" />
                                        <asp:RadioButton ID="rb_facebookSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                            OnCheckedChanged="rb_facebookSignIn_off_Checked" AutoPostBack="true" />
                                    </div>
                                    <div class="clear"></div>
                                    <asp:Panel ID="pnl_facebookSettings" CssClass="pnl-section-child margin-top-big pad-top-big" runat="server" Enabled="false" Visible="false">
                                        <h3 class="font-bold">
                                            <img id="facebook_img" alt="" src="~/Standard_Images/ApiLoginImages/facebook_login.png" runat="server" class="float-left margin-right-sml" style="height: 17px;" />Facebook API Settings</h3>
                                        <div class="clear-space-two"></div>
                                        <small>Your Facebook Graph API settings. Keep the "App Secret" a secret. This key
                                should never be human-readable. Make sure your redirect url(s) match your Facebook App.</small>
                                        <div class="clear-space">
                                        </div>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_facebookAppId" runat="server" DefaultButton="btn_updateFacebookAppId">
                                            <span class="font-bold">App ID</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="txt_facebookAppId" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox>
                                            <asp:Button ID="btn_updateFacebookAppId" runat="server" CssClass="RandomActionBtns input-buttons"
                                                Text="Update" OnClick="btn_updateFacebookAppId_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_facebookAppSecret" runat="server" DefaultButton="btn_updateFacebookAppSecret">
                                            <span class="font-bold">App Secret</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="txt_facebookAppSecret" runat="server" CssClass="textEntry margin-right"
                                                Width="400px"></asp:TextBox><asp:Button ID="btn_updateFacebookAppSecret" runat="server" CssClass="RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateFacebookAppSecret_Click" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <span class="font-bold">Redirect Url(s)</span>
                                        <div class="clear-space-two"></div>
                                        <asp:Literal ID="ltl_facebookRedirect" runat="server"></asp:Literal>
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Allow users to login using their Facebook accounts. <a href="https://developers.facebook.com/docs/facebook-login/login-flow-for-web/v2.2" target="_blank">Integration Information</a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
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
                                        <asp:AsyncPostBackTrigger ControlID="btn_roleEditUpdate" />
                                        <asp:AsyncPostBackTrigger ControlID="btn_roleEditCancel" />
                                        <asp:AsyncPostBackTrigger ControlID="lbtn_CreateNewRole" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons modal-cancel-btn" value="Close" onclick="openWSE.LoadModalWindow(false, 'ManageRoles-element', '');" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="NewUser-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="615">
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
                                                    Passwords are required to be a minimum of <%= Membership.MinRequiredPasswordLength %> characters in length.<br />
                                                    Users must be setup manually through the edit button. The new users will not be able to access any features until the user is assigned to a group.
                                                    <div class="clear"></div>
                                                    <div class="float-left margin-right margin-top">
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="username-login-img"></div>
                                                                <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox" placeholder="Username" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="User Name is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="email-login-img"></div>
                                                                <asp:TextBox ID="Email" runat="server" CssClass="signintextbox" placeholder="Email" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="E-mail is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="password-login-img"></div>
                                                                <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox" placeholder="Password" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="Password is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="password-login-img"></div>
                                                                <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Confirm Password" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                            <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                                Display="Dynamic" ErrorMessage="" ID="ConfirmPasswordRequired" runat="server"
                                                                ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                                ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                                                ErrorMessage="" ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear"></div>
                                                    </div>
                                                    <div class="float-left margin-top">
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="empty-login-img"></div>
                                                                <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="signintextbox" placeholder="First Name" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="empty-login-img"></div>
                                                                <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="signintextbox" placeholder="Last Name" autocomplete="off"></asp:TextBox>
                                                            </div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="colors-login-img">
                                                                </div>
                                                                <asp:TextBox runat="server" ID="Color1" MaxLength="7" CssClass="signintextbox" AutoCompleteType="None" TextMode="Color" />
                                                            </div>
                                                        </div>
                                                        <div class="clear-space"></div>
                                                        <span class="font-bold float-left pad-right pad-top-sml">User Role</span>
                                                        <asp:DropDownList ID="dd_role" runat="server">
                                                        </asp:DropDownList>
                                                        <div class="clear"></div>
                                                    </div>
                                                    <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup" CssClass="input-buttons modal-ok-btn continue-create-user" Text="Create"></asp:Button>
                                                    <div class="clear-space"></div>
                                                    <div class="clear-space"></div>
                                                    <a href="#" onclick="CreateMultipleUsers();return false;">Click Here to Create Multiple Users</a>
                                                    <div class="clear-margin">
                                                        <span class="failureNotification">
                                                            <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                                                        </span>
                                                        <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
                                                            ValidationGroup="RegisterUserValidationGroup" />
                                                    </div>
                                                    <div class="clear"></div>
                                                </ContentTemplate>
                                                <CustomNavigationTemplate>
                                                </CustomNavigationTemplate>
                                            </asp:CreateUserWizardStep>
                                        </WizardSteps>
                                    </asp:CreateUserWizard>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'NewUser-element', ''); ClearNewUserFields();" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="Multiple-User-Create-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="615">
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
                            User colors will be automatically generated for each user. The username will also be used as the name. If a space is in the username, the first name will be assigned to the first word of the username and the last word will be assigned to the last name.
                            <div class="clear"></div>
                            <div class="float-left margin-right margin-top">
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="email-login-img"></div>
                                        <asp:TextBox ID="txt_multiUser_email" runat="server" CssClass="signintextbox" placeholder="Email" AutoCompleteType="None"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="clear-space"></div>
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="password-login-img"></div>
                                        <asp:TextBox ID="txt_multiUser_password" TextMode="Password" runat="server" CssClass="signintextbox" placeholder="Password" AutoCompleteType="None"></asp:TextBox>
                                    </div>
                                    <span id="multiuserpasswordrequired" title="Password is required." class="failureNotification" style="display: none;">*</span>
                                    <div class="clear"></div>
                                </div>
                                <div class="clear"></div>
                            </div>
                            <div class="float-left margin-top">
                                <div class="clear-space"></div>
                                <span class="font-bold float-left pad-right pad-top-sml">User Role</span>
                                <asp:DropDownList ID="dd_role_multiUser" runat="server">
                                </asp:DropDownList>
                                <div class="clear" style="height: 17px;"></div>
                                <div class="textbox-group-padding">
                                    <div class="textbox-group">
                                        <div class="password-login-img"></div>
                                        <asp:TextBox ID="txt_multiUser_confirmpassword" TextMode="Password" runat="server" CssClass="signintextbox" placeholder="Confirm Password" AutoCompleteType="None"></asp:TextBox>
                                    </div>
                                    <span id="multiuserconfirmpasswordrequired" title="Confirm Password is required." class="failureNotification" style="display: none;">*</span>
                                    <div class="clear"></div>
                                </div>
                                <div class="clear"></div>
                            </div>
                            <div class="clear-space"></div>
                            <div class="clear-space"></div>
                            <span class="font-bold">User List</span>
                            <span id="userlistrequired" title="At least one user is required." class="failureNotification pad-left-sml" style="display: none;">*</span>
                            <div class="clear-space-two"></div>
                            <div id="multiusercreate"></div>
                            <div class="clear-space"></div>
                            <a href="#" onclick="CancelMultipleUsers();return false;">Cancel Multiple User Creation</a>
                            <div class="clear"></div>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input id="btn_createmultiusers" type="button" class="input-buttons modal-ok-btn" value="Create" onclick="FinishCreateMultiUsers()" />
                        <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'Multiple-User-Create-element', ''); ClearMultiUserFields();" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="pwreset_overlay" class="Modal-element" runat="server" style="display: none;">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="350" data-setmaxheight="220">
                    <asp:UpdatePanel ID="updatepnl_pwreset" runat="server">
                        <ContentTemplate>
                            <asp:HiddenField ID="hf_createMultiUsers" runat="server" OnValueChanged="hf_createMultiUsers_ValueChanged" />
                            <div class="ModalHeader">
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#" onclick="openWSE.LoadModalWindow(false, 'MainContent_pwreset_overlay', '');return false;" class="ModalExitButton"></a>
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
                                                <div class="failureNotification clear-margin">
                                                    <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                                                </div>
                                                <asp:ValidationSummary ID="ChangeUserPasswordValidationSummary" runat="server" CssClass="failureNotification"
                                                    ValidationGroup="ChangeUserPasswordValidationGroup" Visible="false" />
                                                <div class="accountInfo">
                                                    <asp:TextBox ID="CurrentPassword" runat="server" Visible="false"></asp:TextBox>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="float-left">
                                                        <div class="textbox-group-padding">
                                                            <div class="textbox-group">
                                                                <div class="password-login-img"></div>
                                                                <asp:TextBox ID="NewPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="New Password" AutoCompleteType="None"></asp:TextBox>
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
                                                                <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Confirm New Password" AutoCompleteType="None"></asp:TextBox>
                                                            </div>
                                                            <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword"
                                                                CssClass="failureNotification" Display="Dynamic" ErrorMessage="Confirm New Password is required."
                                                                ToolTip="Confirm New Password is required." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword"
                                                                ControlToValidate="ConfirmNewPassword" CssClass="failureNotification" Display="Dynamic"
                                                                ErrorMessage="Confirm New Password must match the New Password." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear-space">
                                                        </div>
                                                        <asp:Button ID="ChangePasswordPushButton_accountsettings" runat="server" OnClick="ChangePasswordPushButton_accountsettings_Clicked"
                                                            Text="Change Password" ValidationGroup="ChangeUserPasswordValidationGroup" CssClass="input-buttons-create input-buttons-login" OnClientClick="ShowPasswordChangeLoadingMessage();" />
                                                    </div>
                                                    <div class="clear"></div>
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
</asp:Content>
