<%@ Page Title="Mobile Workspace" Language="C#" AutoEventWireup="true" CodeFile="AppRemote.aspx.cs" Inherits="AppRemote" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="mobile-web-app-capable" content="yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <script type="text/javascript" src="Scripts/jquery/mootools.js"></script>
</head>
<body id="main_body" runat="server">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_Main" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="always-visible">
            <div id="top-main-bar-top">
                <table id="workspace_selector" runat="server" class="top-options" cellpadding="0"
                    cellspacing="0">
                    <tr>
                        <td id="menu_header_btn" runat="server">
                            <ul>
                                <li id="menu-s" class="a">
                                    <div class="sidebar-pnlBtn-tab" title="Menu">
                                        <div class="pnlBtn-ShowMenu">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
                        <td id="login_header_btn" runat="server" visible="false">
                            <ul>
                                <li id="login-s" class="a">
                                    <div class="sidebar-pnlBtn-tab" title="Login">
                                        <div class="pnlBtn-ShowLogin">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
                        <td id="workspace_selector_btn" runat="server">
                            <ul class="float-left">
                                <li id="notifications" class="a float-left">
                                    <div class="sidebar-pnlBtn-tab" title="Notifications">
                                        <div class="notification-icon notifications-none">
                                        </div>
                                        <span id="total-noti">0</span>
                                    </div>
                                </li>
                                <li id="db-b" class="a float-left" style="display: none;">
                                    <div class="sidebar-pnlBtn-tab" title="Back without closing">
                                        <div class="pnlBtn-ShowBack">
                                        </div>
                                    </div>
                                </li>
                                <li id="db-c" class="a float-left" style="display: none;">
                                    <div class="sidebar-pnlBtn-tab" title="Close">
                                        <div class="pnlBtn-ShowClose">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
                    </tr>
                </table>
                <asp:Label ID="lbl_UserName" runat="server" CssClass="username-top-info float-right"></asp:Label>
            </div>
        </div>
        <div id="workspace-selector-overlay" style="display: none;">
        </div>
        <div id="workspace-selector-modal" style="display: none;">
            <h3 class="font-bold">Workspace Selector</h3>
            <div class="clear-space">
            </div>
            <span class="pad-right">Workspace:</span><asp:DropDownList ID="dropdownSelector" runat="server"></asp:DropDownList>
            <h3 id="loading-message-modal"></h3>
        </div>
        <div id="loadoptions-selector-overlay" style="display: none;">
        </div>
        <div id="loadoptions-selector-modal" style="display: none;">
            <h3 id="loading-message"></h3>
        </div>
        <asp:HiddenField ID="hf_LogoutOfGroup" runat="server" OnValueChanged="hf_LogoutOfGroup_ValueChanged" />
        <div id="notifications-viewtable">
            <ul style="list-style: none;">
                <li id="notification-tab-b" class="b">
                    <a id="lb_refreshNoti" href="#refresh" class="float-left margin-top margin-left"
                        onclick="appRemote.RefreshNotifications();return false;">Click to Refresh</a>
                    <a id="lb_clearNoti" href="#clear" class="float-right margin-top margin-right"
                        onclick="appRemote.NotiActionsClearAll();return false;" style="display: none;">Clear All</a>
                    <div class="clear-space-two"></div>
                    <div class="pad-all-sml">
                        <div id="table-notiMessages-div-id" class="table-notiMessages-div">
                            <div style="padding: 0 15px">
                                <div id="NotificationHolder">
                                </div>
                            </div>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
        <div id="grouplogin-list">
            <div class="pad-left pad-right pad-top-sml">
                <div class="table-settings-box no-border no-margin no-padding">
                    <div class="td-settings-title">Select a Group</div>
                    <div class="td-settings-ctrl">
                        <div id="group-list"></div>
                    </div>
                    <div class="td-settings-desc">
                        You must be apart of that group in order to login. Once logged in, all default settings for that group will be applied to your account. These settings will be removed once you log out of the group. Your Apps, Overlays, and Notifications will be overridden with the group's settings.
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="pnl_AccountInfo" runat="server" Style="display: none;">
            <div class="pad-top" style="text-align: center;">
                <div class="pad-all inline-block">
                    <div class="float-left margin-right profile-tab-acctimg">
                        <asp:Image ID="img_Profile" runat="server" CssClass="acct-image"></asp:Image>
                    </div>
                    <div class="float-left pad-left">
                        <asp:Label ID="lbl_UserFullName" runat="server" Text="" CssClass="title-dd-name"></asp:Label>
                        <div class="clear-space-two">
                        </div>
                        <asp:Label ID="lbl_UserEmail" runat="server" Text=""></asp:Label>
                        <div class="clear-space-five"></div>
                        <div class="clear-space-two"></div>
                        <asp:HyperLink ID="hyp_accountSettings" runat="server" Text="My Account Settings" NavigateUrl="~/SiteTools/UserMaintenance/AcctSettings.aspx?mobileMode=true" CssClass="account-link-style margin-right"></asp:HyperLink>
                    </div>
                </div>
            </div>
            <div class="clear">
            </div>
            <div id="profile_tab_Buttons">
                <asp:LinkButton ID="aGroupLogoff" runat="server" OnClick="aGroupLogoff_Click" CssClass="RandomActionBtns margin-left margin-right" Enabled="false" Visible="false">Log out of Group</asp:LinkButton>
                <asp:LinkButton ID="lb_signoff" runat="server" OnClick="SignOff_Clicked" CssClass="margin-left margin-right"><span class="img-logoff"></span>Log Out</asp:LinkButton>
            </div>
        </asp:Panel>

        <div id="pnl_icon_toplogo_banner" class="pnl_toplogo_banner" runat="server" style="display: none;">
            <asp:Image ID="img_icon_logo" runat="server" ImageUrl="~/Standard_Images/logo.png" />
        </div>

        <asp:Panel ID="pnl_icons" runat="server">
            <div class="pnl_overflowHolder">
                <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:PlaceHolder ID="ph_iconList" runat="server"></asp:PlaceHolder>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_chat_users" runat="server" Style="display: none;">
            <div class="pnl_overflowHolder">
                <div id="statusDiv" class="pad-all-big">
                    <b class="float-left pad-right">Your Status:</b><a href="#" class="chatstatus_mid">
                        <span id="currentStatus" style="width: 130px;"></span></a>
                    <div id="ul_StatusList" style="display: none;">
                        <ul style="list-style: none;">
                            <li class="updatestatus">
                                <div class='statusUserDiv statusUserOnline'>
                                </div>
                                <a href="#Available">Available</a></li>
                            <li class="updatestatus">
                                <div class='statusUserDiv statusUserBusy'>
                                </div>
                                <a href="#Busy">Busy</a></li>
                            <li class="updatestatus">
                                <div class='statusUserDiv statusUserAway'>
                                </div>
                                <a href="#Away">Away</a></li>
                            <li class="updatestatus">
                                <div class='statusUserDiv statusUserOffline'>
                                </div>
                                <a href="#Out">Offline</a></li>
                        </ul>
                    </div>
                    <div class="clear-margin">
                        <span class="pad-right"><b>Users Online:</b></span><span id="header-total-online"></span>
                    </div>
                </div>
                <div id="chatuserlist">
                    <h3 class="pad-left-big pad-bottom-big">Loading users....
                    </h3>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_adminPages" runat="server" Style="display: none;">
            <div class="pnl_overflowHolder">
                <div class="clear-space"></div>
                <asp:PlaceHolder ID="ph_adminPageList" runat="server"></asp:PlaceHolder>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_chat_popup" runat="server" Style="display: none;">
        </asp:Panel>
        <asp:Panel ID="pnl_adminPage_iframe" runat="server" Style="display: none;">
        </asp:Panel>
        <asp:HiddenField ID="hf_chatsound" runat="server" ClientIDMode="Static" />
        <asp:Panel ID="pnl_options" runat="server" Style="display: none; text-align: center;">
            <div id="pnl_options-minHeight">
                <div id="accordion">
                    <h3 id="load-option-text" class="accordion-header active">Options</h3>
                    <div class="accordion-content">
                        <div id="app-load-options">
                            <div class="pad-all">
                                <div id="last-updated">
                                </div>
                                <div class="clear-space">
                                </div>
                                <div id="app-workspace-selector" class="pad-top-sml pad-bottom-big">
                                    <span class="pad-right">Workspace:</span><asp:DropDownList ID="ddl_appDropdownSelector" runat="server"></asp:DropDownList>
                                </div>
                                <table cellpadding="5" cellspacing="5" border="0" width="100%">
                                    <tr>
                                        <td align="right">
                                            <div class="pad-right-big">
                                                <asp:RadioButton ID="rb_norm" runat="server" GroupName="rbminmax" Text="&nbsp;Normal" />
                                            </div>
                                        </td>
                                        <td id="div_max_rb_holder" align="center">
                                            <asp:RadioButton ID="rb_max" runat="server" GroupName="rbminmax" Text="&nbsp;Maximize" />
                                        </td>
                                        <td align="left">
                                            <div class="pad-left-big">
                                                <asp:RadioButton ID="rb_min" runat="server" GroupName="rbminmax" Text="&nbsp;Minimize" />
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div class="divider">
                            </div>
                            <asp:Panel ID="pnl_appMoveResize" runat="server" Style="display: none;">
                                <h3 class="pad-left pad-right pad-bottom">App Position</h3>
                                <div class="pad-all">
                                    <span class="pad-right">Top Pos:</span>
                                    <input type="number" id="moveresize-top" /><span class="pad-left">px</span>
                                    <div class="clear-space">
                                    </div>
                                    <span class="pad-right">Left Pos:</span>
                                    <input type="number" id="moveresize-left" /><span class="pad-left">px</span>
                                    <div class="clear-space">
                                    </div>
                                </div>
                                <div id="resize-div">
                                    <div class="divider">
                                    </div>
                                    <h3 class="pad-left pad-right pad-bottom">App Size</h3>
                                    <div class="pad-all">
                                        <span class="pad-right">Width: </span>
                                        <input type="number" id="moveresize-width" /><span class="pad-left">px</span>
                                        <div class="clear-space">
                                        </div>
                                        <span class="pad-right">Height:</span>
                                        <input type="number" id="moveresize-height" /><span class="pad-left">px</span>
                                        <div class="clear-space">
                                        </div>
                                    </div>
                                </div>
                                <div class="divider">
                                </div>
                            </asp:Panel>
                        </div>
                        <div id="load-option-btn-holder">
                            <a id="options-btn-device" href="#" class="option-buttons" onclick="appRemote.LoadApp();return false;">
                                <span class="img-open img-option"></span>Open App</a>
                            <div class="clear">
                            </div>
                            <a id="options-btn-open" href="#" class="option-buttons" onclick="appRemote.OpenApp();return false;">
                                <span class="img-workspace img-option"></span>Load On Workspace</a>
                            <div class="clear">
                            </div>
                            <a id="options-btn-update" href="#" class="option-buttons" onclick="appRemote.OpenApp();return false;"
                                style="display: none;"><span class="img-update img-option"></span>Refresh App</a>
                            <div class="clear">
                            </div>
                            <a href="#" id="options-btn-close" class="option-buttons" onclick="appRemote.CloseApp();return false;">
                                <span class="img-close img-option"></span>Close App</a>
                            <h3 id="no-options-available" class="pad-all" style="display: none;">No Options Available</h3>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_login" runat="server" Enabled="false" Visible="false">
            <div class="pnl_overflowHolder">
                <div class="margin-top-big">
                    <div id="Login-holder">
                        <asp:Login ID="Login1" runat="server" FailureText="<span style='color: #D80000;'>Invalid Username or Password.</span>"
                            LoginButtonText="Login" TitleText="" Width="100%" DestinationPageUrl="AppRemote.aspx"
                            OnLoggingIn="Login_LoggingIn" OnLoggedIn="Login_Loggedin" OnLoginError="Login_error"
                            Style="position: relative; z-index: 5000">
                            <LayoutTemplate>
                                <asp:Panel ID="pnl_Login" runat="server" DefaultButton="LoginButton">
                                    <table cellpadding="5" cellspacing="5" style="margin: 0 auto;">
                                        <tr>
                                            <td>
                                                <div style="position: relative;">
                                                    <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox" placeholder="Username"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                        ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                                        ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                    <div class="username-login-img"></div>
                                                    <div class="clear-space"></div>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <div style="position: relative;">
                                                    <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox" placeholder="Password"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                        ErrorMessage="Password is required." Font-Bold="True" ForeColor="Red" ToolTip="Password is required."
                                                        ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                    <div class="password-login-img"></div>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <div class="clear-space"></div>
                                                <asp:Button ID="LoginButton" runat="server" CommandName="Login" ValidationGroup="ctl00$Login1"
                                                    Text="Login" CssClass="input-buttons-login" />
                                                <div class="clear-space"></div>
                                                <div id="rememberme-holder">
                                                    <asp:CheckBox ID="RememberMe" runat="server" Text=" Remember me" />
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <asp:UpdatePanel ID="updatepnl_Login" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div align="center">
                                                <div class="clear-space"></div>
                                                <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                                                <asp:Literal ID="ltl_logingrouperror" runat="server"></asp:Literal>
                                                <div class="clear-space"></div>
                                            </div>
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="LoginButton" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </asp:Panel>
                            </LayoutTemplate>
                            <TextBoxStyle BackColor="White" />
                            <ValidatorTextStyle Font-Bold="True" ForeColor="Red" />
                        </asp:Login>
                        <asp:UpdatePanel ID="updatepnl_LoginWith" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div align="center">
                                    <a id="hyp_groupLogin" runat="server" href="#?groupLogin=true">Select a group to login to</a>
                                    <asp:HyperLink ID="hyp_cancelGroupLogin" runat="server" NavigateUrl="~/AppRemote.aspx" Text="Cancel Group Login" Visible="false" Enabled="false"></asp:HyperLink>
                                    <div class="clear-space"></div>
                                    <a href="#" id="lnk_forgotpassword" onclick="appRemote.LoadRecoveryPassword();return false;">Forgot your Password?</a>
                                    <div class="clear-space"></div>
                                    <a href="#" id="login_register_link" onclick="appRemote.LoadCreateAccountHolder();return false;">Create a new account</a>
                                </div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Google" />
                                <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Twitter" />
                                <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Facebook" />
                            </Triggers>
                        </asp:UpdatePanel>
                        <div class="pad-top-big" align="center">
                            <div class="loginwith-api-text">
                                <div><span class="inline-block">Or Login with...</span></div>
                                <hr class="soften" />
                            </div>
                            <div class="clear-space"></div>
                            <asp:LinkButton ID="lbtn_signinwith_Google" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Google_Click" ToolTip="Login with Google">
                                <img id="google_login_logo" alt="" src="~/Standard_Images/ApiLoginImages/google_login.png" runat="server" />
                            </asp:LinkButton>
                            <asp:LinkButton ID="lbtn_signinwith_Twitter" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Twitter_Click" ToolTip="Login with Twitter">
                                <img id="twitter_login_logo" alt="" src="~/Standard_Images/ApiLoginImages/twitter_login.png" runat="server" />
                            </asp:LinkButton>
                            <asp:LinkButton ID="lbtn_signinwith_Facebook" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Facebook_Click" ToolTip="Login with Facebook">
                                <img id="facebook_login_logo" alt="" src="~/Standard_Images/ApiLoginImages/facebook_login.png" runat="server" />
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div id="CreateAccount-holder" style="display: none;">
                        <div id="iframe-createaccount-holder">
                        </div>
                        <div align="center" class="pad-top-big">
                            <a href="#" onclick="appRemote.LoadCreateAccountHolder();return false;">Cancel</a>
                        </div>
                    </div>
                    <div id="ForgotPassword-holder" style="display: none;">
                        <div style="text-align: center;">
                            <h4>Please provide your username. An email will be sent to this account using the email address associated with your account. Click the link in the email to recover your account. Always change your password after recovering your account.</h4>
                        </div>
                        <div class="clear-space"></div>
                        <asp:UpdatePanel ID="updatepnl_forgotPassword" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnl_forgotPassword" runat="server" DefaultButton="btn_passwordrecovery" Style="margin: 0 auto; width: 270px; text-align: center;">
                                    <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox margin-left" placeholder="Username"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                        ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                        ValidationGroup="UsernameRecovery">*</asp:RequiredFieldValidator>
                                    <div class="clear-space"></div>
                                    <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                        CssClass="input-buttons-login" Text="Send Email" ValidationGroup="UsernameRecovery" />
                                    <div class="clear-space"></div>
                                    <div align="center" class="pad-top-big">
                                        <a href="#" onclick="appRemote.LoadRecoveryPassword();return false;">Cancel</a>
                                    </div>
                                </asp:Panel>
                                <asp:Label ID="lbl_passwordResetMessage" runat="server" Text=""></asp:Label>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btn_passwordrecovery" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <div id="appremote-sidebar-menu">
            <div class="pad-all appremote-sidebar-innercontent">
                <ul>
                    <li id="apps_header_btn" runat="server"></li>
                    <li id="chat_header_btn" runat="server"></li>
                    <li id="pages_header_btn" runat="server"></li>
                    <li id="connect_header_btn" runat="server">
                        <div id="tryconnect" class="section-pad section-link">
                            <span class="disconnected-img"></span>Connect to Workspace
                        </div>
                    </li>
                    <li id="workspace_header_btn" runat="server">
                        <div id="db-s" class="section-pad section-link">
                            <span class="workspace-img"></span>Workspace Selector
                        </div>
                    </li>
                    <li id="group-btns-holder">
                        <div class="section-pad section-link">
                            <div id="groupLogin" runat="server"><span class="grouplogin-img"></span>Group Login</div>
                            <div id="changeGroupLogin" runat="server" visible="false"><span class="grouplogin-img"></span>Change Group</div>
                            <div id="groupLogout" runat="server" visible="false"></div>
                        </div>
                    </li>
                    <li id="opened_apps_header" style="display: none;">
                        <div class="section-pad">
                            <h3 class="float-left">Opened Apps</h3>
                            <a href="#" class="close-all-opened" onclick="appRemote.CloseAllOpened();return false;">Close All</a>
                            <div class="clear-space-five"></div>
                            <div id="opened_apps_holder"></div>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
        <div id="appremote-menu-overlay"></div>
        <div id="container-footer" class="footer">
            <div class="footer-padding" align="center">
                <div id="footer-signdate">
                    &copy; 2016 OpenWSE | <a href="About.aspx?redirect=AppRemote.aspx">About</a> | <a href="About.aspx?a=termsofuse&redirect=AppRemote.aspx">Terms</a> | 
                    <asp:Label ID="lblHomePageLink" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
