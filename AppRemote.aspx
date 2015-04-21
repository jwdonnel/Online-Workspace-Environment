<%@ page language="C#" autoeventwireup="true" inherits="AppRemote, App_Web_iv0v2cts" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Mobile Workspace</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
</head>
<body style="overflow: hidden; background-color: #FFF;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_Main" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="always-visible">
            <div id="top-main-bar-top">
                <table id="workspace_selector" runat="server" class="top-options" cellpadding="0"
                    cellspacing="0">
                    <tr>
                        <td id="apps_header_btn" runat="server">
                            <ul>
                                <li id="wl-s" class="a">
                                    <div class="sidebar-pnlBtn-tab" title="Apps">
                                        <div class="pnlBtn-ShowApps">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
                        <td id="chat_header_btn" runat="server">
                            <ul>
                                <li id="cc-s" class="a">
                                    <div id="SteelmfgHeader" class="sidebar-pnlBtn-tab" title="Chat">
                                        <div class="pnlBtn-ShowChat">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
                        <td id="pages_header_btn" runat="server">
                            <ul>
                                <li id="ap-s" class="a">
                                    <div class="sidebar-pnlBtn-tab" title="Apps">
                                        <div class="pnlBtn-ShowAdminPages">
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
                                <li id="db-s" class="a float-left">
                                    <div class="sidebar-pnlBtn-tab" title="Workspace Selector">
                                        <div class="pnlBtn-ShowWorkspace">
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
            <h3 id="loading-message" class="pad-top margin-top"></h3>
        </div>
        <div id="top-btns-holder">
            <div id="tryconnect" style="display: none;">Sync with Workspace</div>
            <div id="groupLogin" runat="server" visible="false">Group Login</div>
            <div id="changeGroupLogin" runat="server" visible="false">Change Group</div>
            <div id="groupLogout" runat="server" visible="false"></div>
            <div id="notifications" class="no-notifications">Notifications:<span id="total-noti" class="pad-left-sml">0</span></div>
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
        <asp:Panel ID="pnl_chat_users" runat="server" Style="display: none;">
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
        </asp:Panel>
        <asp:Panel ID="pnl_icons" runat="server">
            <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:PlaceHolder ID="ph_iconList" runat="server"></asp:PlaceHolder>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>
        <asp:Panel ID="pnl_adminPages" runat="server" Style="display: none;">
        </asp:Panel>
        <asp:Panel ID="pnl_adminPage_iframe" runat="server" Style="display: none;">
        </asp:Panel>
        <asp:Panel ID="pnl_chat_popup" runat="server" Style="display: none;">
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
                                <table cellpadding="5" cellspacing="5" border="0" width="100%">
                                    <tr>
                                        <td id="app-workspace-selector" valign="top" style="width: 50%;">
                                            <span class="pad-right">Workspace:</span><asp:DropDownList ID="ddl_appDropdownSelector" runat="server"></asp:DropDownList>
                                        </td>
                                        <td valign="top" style="text-align: left; width: 50%;">
                                            <div class="pad-all float-left margin-right margin-left">
                                                <asp:RadioButton ID="rb_norm" runat="server" GroupName="rbminmax" Text="&nbsp;Normal" />
                                                <div class="clear-space">
                                                </div>
                                                <div id="div_max_rb_holder">
                                                    <asp:RadioButton ID="rb_max" runat="server" GroupName="rbminmax" Text="&nbsp;Maximize" />
                                                    <div class="clear-space">
                                                    </div>
                                                </div>
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
                                    <input type="text" id="moveresize-top" /><span class="pad-left">px</span>
                                    <div class="clear-space">
                                    </div>
                                    <span class="pad-right">Left Pos:</span>
                                    <input type="text" id="moveresize-left" /><span class="pad-left">px</span>
                                    <div class="clear-space">
                                    </div>
                                </div>
                                <div id="resize-div">
                                    <div class="divider">
                                    </div>
                                    <h3 class="pad-left pad-right pad-bottom">App Size</h3>
                                    <div class="pad-all">
                                        <span class="pad-right">Width: </span>
                                        <input type="text" id="moveresize-width" /><span class="pad-left">px</span>
                                        <div class="clear-space">
                                        </div>
                                        <span class="pad-right">Height:</span>
                                        <input type="text" id="moveresize-height" /><span class="pad-left">px</span>
                                        <div class="clear-space">
                                        </div>
                                    </div>
                                </div>
                                <div class="divider">
                                </div>
                            </asp:Panel>
                        </div>
                        <div id="load-option-btn-holder">
                            <a id="options-btn-open" href="#" class="option-buttons" onclick="appRemote.OpenApp();return false;">
                                <span class="img-workspace img-option"></span>Load On Workspace</a>
                            <div class="clear">
                            </div>
                            <a id="options-btn-update" href="#" class="option-buttons" onclick="appRemote.OpenApp();return false;"
                                style="display: none;"><span class="img-update img-option"></span>Refresh App</a>
                            <div class="clear">
                            </div>
                            <a id="options-btn-device" href="#" class="option-buttons" onclick="appRemote.LoadApp();return false;">
                                <span class="img-open img-option"></span>Open App</a>
                            <div class="clear">
                            </div>
                            <a href="#" id="options-btn-close" class="option-buttons" onclick="appRemote.CloseApp();return false;">
                                <span class="img-close img-option"></span>Close App</a>
                            <h3 id="no-options-available" class="pad-all" style="display: none;">No Options Available</h3>
                        </div>
                    </div>
                    <h3 id="load-option-about" class="accordion-header">
                        <span class="about-app" style="float: right!important;"></span>
                        About
                    </h3>
                    <div class="accordion-content pad-all" align="left" style="display: none;">
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_login" runat="server" CssClass="pad-all-big" Enabled="false" Visible="false">
            <div align="center" class="pad-top pad-bottom-big">
                <div class="logo-holder">
                    <img id="groupLoginLogo" runat="server" alt="Logo" visible="false" />
                    <div id="mainLoginLogo" runat="server" class="main-logo"></div>
                </div>
                <div class="clear"></div>
            </div>
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
                                        <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox" placeholder="Username"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                            ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                            ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                        <div class="clear-space"></div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox" placeholder="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                            ErrorMessage="Password is required." Font-Bold="True" ForeColor="Red" ToolTip="Password is required."
                                            ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
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
                            <a id="hyp_groupLogin" runat="server" href="#?groupLogin=true">Select a Group to Login To</a>
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
        </asp:Panel>
        <div id="container-footer" class="footer">
            <div class="footer-padding" align="center">
                <div id="footer-signdate">
                    &copy; 2015 John Donnelly | 
                    <asp:LinkButton ID="lb_signoff" runat="server" Text="Log Out" CssClass="cursor-pointer"
                        OnClick="SignOff_Clicked"></asp:LinkButton><asp:Label ID="lblHomePageLink" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
