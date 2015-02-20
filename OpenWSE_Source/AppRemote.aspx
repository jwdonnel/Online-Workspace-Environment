<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AppRemote.aspx.cs" Inherits="AppRemote" %>

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
                        <td>
                            <ul>
                                <li id="db-s" class="a header-divider-right float-left">
                                    <div class="sidebar-pnlBtn-tab" title="Workspace Selector">
                                        <div class="pnlBtn-ShowWorkspace">
                                        </div>
                                    </div>
                                </li>
                                <li id="db-b" class="a float-left" style="display: none;">
                                    <div class="sidebar-pnlBtn-tab" title="Back without closing">
                                        <div class="pnlBtn-ShowBack">
                                        </div>
                                    </div>
                                </li>
                                <li id="db-c" class="a float-left" style="display: none;">
                                    <div class="sidebar-pnlBtn-tab" title="Close App">
                                        <div class="pnlBtn-ShowClose">
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </td>
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
                    </tr>
                </table>
                <div id="top_title_bar" runat="server" visible="false" class="top-main-bar-top-title float-left">
                </div>
                <asp:Label ID="lbl_UserName" runat="server" CssClass="username-top-info float-right"></asp:Label>
            </div>
        </div>
        <div id="workspace-selector-overlay" style="display: none;">
        </div>
        <div id="workspace-selector-modal" style="display: none;">
            <h3 class="font-bold">Workspace Selector</h3>
            <div class="clear-space">
            </div>
            <div id="workspace-selector">
                <span class="pad-right">Workspace:</span><asp:DropDownList ID="dropdownSelector" runat="server"></asp:DropDownList>
            </div>
            <h3 id="loading-message-modal"></h3>
        </div>
        <div id="loadoptions-selector-overlay" style="display: none;">
        </div>
        <div id="loadoptions-selector-modal" style="display: none;">
            <h3 id="loading-message" class="pad-top margin-top"></h3>
        </div>
        <div id="notifications" class="no-notifications">Notifications:<span id="total-noti" class="pad-left-sml">0</span></div>
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
        </asp:Panel>
        <asp:Panel ID="pnl_chat_popup" runat="server" Style="display: none;">
            <div id="Category-Back-ChatRemote-Close">
                <img alt="back" src="App_Themes/Standard/Icons/close.png" /><h4 class="float-left">Close Chat Session</h4>
            </div>
        </asp:Panel>
        <asp:HiddenField ID="hf_chatsound" runat="server" ClientIDMode="Static" />
        <asp:Panel ID="pnl_options" runat="server" Style="display: none; text-align: center;">
            <div id="pnl_options-minHeight">
                <div id="Category-Back-options">
                    <img alt="back" src="App_Themes/Standard/Icons/prevpage.png" /><h4 class="float-left">Back To App List</h4>
                </div>
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
                <asp:Image ID="img_LoginGroup" runat="server" Style="max-width: 90%;" />
                <div class="clear-space"></div>
                <h4 id="lbl_login_help" runat="server"></h4>
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
                                        <span class="login-td-text">Username</span>
                                        <div class="clear"></div>
                                        <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                            ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                            ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <div class="clear-space"></div>
                                        <span class="login-td-text">Password</span>
                                        <div class="clear"></div>
                                        <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                            ErrorMessage="Password is required." Font-Bold="True" ForeColor="Red" ToolTip="Password is required."
                                            ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <div class="clear-space"></div>
                                        <div class="clear-space"></div>
                                        <asp:Button ID="LoginButton" runat="server" CommandName="Login" ValidationGroup="ctl00$Login1"
                                            Text="Login" CssClass="input-buttons-login float-left" />
                                        <div class="float-right pad-right pad-top">
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
                                        <a href="#" id="lnk_forgotpassword" onclick="appRemote.LoadRecoveryPassword();return false;">Forgot your Password?</a>
                                        <div class="clear-space"></div>
                                        <a href="#" id="login_register_link" onclick="appRemote.LoadCreateAccountHolder();return false;">Create a new account</a>
                                    </div>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="LoginButton" />
                                    <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Google" />
                                    <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Twitter" />
                                    <asp:AsyncPostBackTrigger ControlID="lbtn_signinwith_Facebook" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </asp:Panel>
                    </LayoutTemplate>
                    <TextBoxStyle BackColor="White" />
                    <ValidatorTextStyle Font-Bold="True" ForeColor="Red" />
                </asp:Login>
                <div class="pad-top-big" align="center">
                    <div class="loginwith-api-text">
                        <div><span class="inline-block">Sign in with...</span></div>
                        <hr class="soften" />
                    </div>
                    <div class="clear-space"></div>
                    <asp:LinkButton ID="lbtn_signinwith_Google" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Google_Click" ToolTip="Sign in with Google">
                        <img id="google_login_logo" alt="" src="~/Standard_Images/ApiLoginImages/google_login.png" runat="server" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="lbtn_signinwith_Twitter" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Twitter_Click" ToolTip="Sign in with Twitter">
                        <img id="twitter_login_logo" alt="" src="~/Standard_Images/ApiLoginImages/twitter_login.png" runat="server" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="lbtn_signinwith_Facebook" runat="server" CssClass="loginwith-api" OnClick="lbtn_signinwith_Facebook_Click" ToolTip="Sign in with Facebook">
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
                        <asp:Panel ID="pnl_forgotPassword" runat="server" DefaultButton="btn_passwordrecovery" Style="margin: 0 auto; width: 270px;">
                            <span class="login-td-text">Username</span>
                            <div class="clear"></div>
                            <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                ValidationGroup="UsernameRecovery">*</asp:RequiredFieldValidator>
                            <div class="clear-space"></div>
                            <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                CssClass="input-buttons margin-left-sml" Text="Send Email" ValidationGroup="UsernameRecovery" />
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
                    John Donnelly | <a href="Workspace.aspx">OpenWSE</a> 2015 | 
                    <asp:LinkButton ID="lb_signoff" runat="server" Text="Sign Off" CssClass="cursor-pointer"
                        OnClick="SignOff_Clicked"></asp:LinkButton><asp:Label ID="lblHomePageLink" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
