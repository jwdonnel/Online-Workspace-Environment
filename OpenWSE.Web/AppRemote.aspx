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
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
</head>
<body id="site_mainbody" runat="server" class="app-remote-container" data-remoteonly="true">
    <!--[if lt IE 9]>
        <div class="lt-ie9-bg">
            <p class="browsehappy">You are using an <strong>outdated</strong> browser.</p>
            <p>Please <a href="http://browsehappy.com/">upgrade your browser</a> to improve your experience.</p>
        </div>
    <![endif]-->
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_Main" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="top_bar_toolview_holder" runat="server" class="iframe-top-bar">
            <div id="iframe_title_logo" runat="server" class="iframe-title-logo"></div>
            <div id="sidebar_menu_toggle_btn" runat="server" class="sidebar-menu-toggle" title="Menu" onclick="appRemote.AppRemoteSidebar.OnMenuSidebarClick();">Menu</div>
            <div id="user_profile_tab" runat="server" class="top-bar-userinfo-button">
                <div class="active-div"></div>
                <ul>
                    <li class="a">
                        <asp:Label ID="lbl_UserName" runat="server" CssClass="username-top-info" ToolTip="Account Options"></asp:Label></li>
                    <li class="b">
                        <div class="li-header">
                            My Account
                        </div>
                        <div class="pad-all">
                            <div class="pad-all">
                                <div class="float-left margin-right profile-tab-acctimg">
                                    <asp:Image ID="img_Profile" runat="server" CssClass="acct-image"></asp:Image>
                                    <div class="btn_changephoto" onclick="openWSE.ChangeUserProfileImage();">
                                        Update
                                    </div>
                                </div>
                                <div class="float-left pad-left">
                                    <asp:Label ID="lbl_UserFullName" runat="server" Text="" CssClass="title-dd-name"></asp:Label>
                                    <div class="clear-space-two">
                                    </div>
                                    <asp:Label ID="lbl_UserEmail" runat="server" Text=""></asp:Label>
                                    <div class="clear-space-five"></div>
                                    <asp:HyperLink ID="hyp_accountSettings" runat="server" Text="My Account Settings" NavigateUrl="~/SiteTools/UserTools/AcctSettings.aspx" CssClass="account-link-style margin-right"></asp:HyperLink>
                                </div>
                                <div class="clear">
                                </div>
                            </div>
                        </div>
                        <div id="profile_tab_Buttons" class="li-footer">
                            <asp:LinkButton ID="lbtn_signoff" runat="server" OnClick="lbtn_signoff_Click">Log Out</asp:LinkButton>
                            <div class="clear"></div>
                        </div>
                    </li>
                </ul>
            </div>
            <a id="Close_tab" runat="server" class="close-iframe" data-tabid="Close_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;" title="Close" style="display: none;"></a>
            <a id="Minimize_tab" runat="server" class="minimize-iframe" data-tabid="Minimize_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;" title="Minimize" style="display: none;"></a>
            <a id="About_tab" runat="server" class="about-iframe" data-tabid="About_tab" onclick="appRemote.AboutApp();return false;" title="About app" style="display: none;"></a>
            <div class="clear"></div>
        </div>
        <div id="remote_sidebar" runat="server">
            <div class="sidebar-innercontent">
                <span class="remote-sidebar-title">Menu</span>
                <div class="close-app-menu-toggle" title="Close menu" onclick="appRemote.AppRemoteSidebar.OnMenuSidebarClick();"></div>
                <div class="clear"></div>
                <a id="Apps_tab" runat="server" class="section-pad section-link" data-tabid="Apps_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="app-img"></span>Applications</a>
                <a id="Login_tab" runat="server" visible="false" class="section-pad section-link" data-tabid="Login_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="login-img"></span>
                    <asp:Literal ID="ltl_LoginLabel" runat="server"></asp:Literal></a>
                <a id="Chat_tab" runat="server" class="section-pad section-link" data-tabid="Chat_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="chat-img"></span>Chat</a>
                <a id="AdminLinks_tab" runat="server" class="section-pad section-link" data-tabid="AdminLinks_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="tools-img"></span>Settings and Tools</a>
                <a id="Notifications_tab" runat="server" class="section-pad section-link" data-tabid="Notifications_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span id="lbl_notifications"></span>Notifications</a>
                <a id="Layout_tab" runat="server" class="section-pad section-link" data-tabid="Layout_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="layout-img"></span>Layout Options</a>
                <a id="Groups_tab" runat="server" class="section-pad section-link" data-tabid="Groups_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="grouptab-img"></span>Groups</a>
                <a id="Overlay_tab" runat="server" class="section-pad section-link" data-tabid="Overlay_tab" onclick="appRemote.AppRemoteSidebar.MenuTabNavigation(this);return false;"><span class="overlay-img"></span>Overlays</a>
                <a id="connect_header_btn" runat="server" class="section-pad section-link" onclick="appRemote.SyncWithWorkspaceClick(); return false;"><span class="disconnected-img"></span>Sync with Workspace</a>
                <div id="workspace_header_btn" runat="server" class="pad-top-big" style="display: none;">
                    <div class="clear-space"></div>
                    <span class="remote-sidebar-title">Workspace Selector
                    </span>
                    <div class="clear-space-five"></div>
                    <span class="currentWorkspace-selectorText">Current:</span>
                    <asp:DropDownList ID="dropdownSelector" runat="server" CssClass="margin-left"></asp:DropDownList>
                    <div class="clear"></div>
                </div>
                <div id="opened_apps_header" runat="server" class="pad-top-big margin-top" style="display: none;">
                    <div class="clear-space"></div>
                    <span class="remote-sidebar-title float-left">Opened Apps</span>
                    <a href="#" class="close-all-opened float-right margin-right" onclick="appRemote.CloseAllOpened();return false;">Close All</a>
                    <div class="clear-space-five"></div>
                    <div id="opened_apps_holder"></div>
                    <div class="clear-space"></div>
                </div>
                <div class="clear"></div>
            </div>
        </div>
        <div id="remote_containerholder" runat="server" class="fixed-container-holder-background">
            <div id="remote_maincontainer" runat="server" class="fixed-container-holder">
                <div id="main_container" runat="server">
                    <asp:UpdatePanel ID="MainContent_updatepnl_apploader" runat="server" ClientIDMode="Static"
                        EnableViewState="False" ViewStateMode="Disabled">
                        <ContentTemplate>
                            <asp:HiddenField ID="hf_loadOverlay1" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                            <asp:HiddenField ID="hf_noti_update_hiddenField" runat="server" />
                            <asp:HiddenField ID="hf_aboutstatsapp" runat="server" ClientIDMode="Static" />
                            <div class="move-holder" style="display: none; visibility: hidden;">
                                <asp:PlaceHolder ID="PlaceHolder1" runat="server" ClientIDMode="Static" ViewStateMode="Disabled"
                                    EnableViewState="false"></asp:PlaceHolder>
                            </div>
                            <div id="noti-update-element">
                                <div class="noti-update-element-align">
                                    <div class="noti-update-element-modal">
                                        <asp:Image ID="img_noti_update_image" runat="server" />
                                        <div class="float-left">
                                            <h1 class="inline-block">You have a new Notification</h1>
                                            <div class="clear">
                                            </div>
                                            <h2 class="inline-block">
                                                <asp:Label ID="lbl_noti_update_message" runat="server"></asp:Label>
                                            </h2>
                                            <asp:Label ID="lbl_noti_update_popup_Description" runat="server" Style="display: none;"></asp:Label>
                                        </div>
                                        <div class="clear"></div>
                                    </div>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <asp:UpdatePanel ID="MainContent_updatepnl_notificationpopup" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Label ID="lbl_noti_update_popup_CurrCount" runat="server" Visible="false"></asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div id="pnlContent_Apps" runat="server" class="main-content-panels">
                        <asp:Panel ID="pnl_icons" runat="server">
                            <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:PlaceHolder ID="ph_iconList" runat="server"></asp:PlaceHolder>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </asp:Panel>
                    </div>
                    <div id="pnlContent_Chat" runat="server" class="main-content-panels">
                        <asp:Panel ID="pnl_chat_users" runat="server">
                            <div id="statusDiv">
                                <a href="#" class="chatstatus_mid"><span id="currentStatus" style="width: 130px;"></span></a>
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
                                <div style="display: none;">
                                    <div class="clear-space-two"></div>
                                    <span class="pad-right"><b>Users Online:</b></span><span id="header-total-online"></span>
                                </div>
                                <div class="clear"></div>
                            </div>
                            <div id="chatuserlist">
                                <h3 class="pad-left-big pad-bottom-big">Loading users....
                                </h3>
                            </div>
                        </asp:Panel>
                    </div>
                    <div id="pnlContent_AdminLinks" runat="server" class="main-content-panels">
                        <asp:Panel ID="sidebar_accordian" runat="server">
                            <asp:PlaceHolder ID="placeHolder_SettingsTabs" runat="server"></asp:PlaceHolder>
                        </asp:Panel>
                    </div>
                    <div id="pnlContent_Notifications" runat="server" class="main-content-panels">
                        <div id="notification-tab-b">
                            <a id="lb_clearNoti" href="#clear" class="float-right margin-right margin-top" onclick="openWSE.NotiActionsClearAll();return false;" style="display: none;">Dismiss All</a>
                            <div class="clear"></div>
                            <div class="li-pnl-tab">
                                <div id="NotificationHolder"></div>
                            </div>
                        </div>
                    </div>
                    <div id="pnlContent_Layout" runat="server" class="main-content-panels">
                        <div id="div_ColorOptionsHolder" runat="server" class="pad-all-sml"></div>
                    </div>
                    <div id="pnlContent_Groups" runat="server" class="main-content-panels">
                        <div id="grouplistdiv"></div>
                        <div class="group-loginhint">
                            You must be apart of that group in order to login. Once logged in, all default settings for that group will be applied to your account. These settings will be removed once you log out of the group. Your Apps, Overlays, and Notifications will be overridden with the group's settings.
                        </div>
                        <div id="divGroupLogoff" class="li-footer" style="display: none;">
                            <asp:LinkButton ID="aGroupLogoff" runat="server" OnClick="aGroupLogoff_Click" CssClass="RandomActionBtns">Log out of Group</asp:LinkButton>
                            <div class="clear"></div>
                        </div>
                    </div>
                    <div id="pnlContent_Overlay" runat="server" class="main-content-panels">
                        <a id="btn_addOverlayButton" runat="server" class="addOverlay-bg float-right margin-right margin-top" onclick="openWSE.CallOverlayList();return false;">Add</a>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_OverlaysAll" runat="server" ViewStateMode="Disabled" EnableViewState="false">
                        </asp:Panel>
                        <div id="overlayEdit-element" class="Modal-element">
                            <div class="Modal-element-align">
                                <div class="Modal-element-modal" data-setwidth="700">
                                    <div class="ModalHeader">
                                        <div>
                                            <div class="app-head-button-holder-admin">
                                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'overlayEdit-element', '');return false;"
                                                    class="ModalExitButton"></a>
                                            </div>
                                            <span class="Modal-title"></span>
                                        </div>
                                    </div>
                                    <div class="ModalScrollContent">
                                        <div class="ModalPadContent">
                                            Overlays will be automatically updated upon click.
                                                            <div class="clear-space"></div>
                                            <div id="overlay-edit-list"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="pnlContent_ChatPopup" runat="server" class="main-content-panels"></div>
                    <div id="pnlContent_AppOptions" runat="server" class="main-content-panels">
                        <div id="pnlContent_AppOptions-Holder">
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
                                                <span>Workspace</span>
                                                <div class="clear-space-two"></div>
                                                <asp:DropDownList ID="ddl_appDropdownSelector" runat="server"></asp:DropDownList>
                                            </div>
                                            <span>App State</span>
                                            <div class="clear-space-two"></div>
                                            <asp:DropDownList ID="ddl_appstate" runat="server">
                                                <asp:ListItem Value="Normal" Text="Normal"></asp:ListItem>
                                                <asp:ListItem Value="Maximize" Text="Maximize"></asp:ListItem>
                                                <asp:ListItem Value="Minimize" Text="Minimize"></asp:ListItem>
                                            </asp:DropDownList>
                                            <div class="clear-space"></div>
                                        </div>
                                        <div class="clear">
                                        </div>
                                        <asp:Panel ID="pnl_appMoveResize" runat="server" Style="display: none;">
                                            <div class="pad-all">
                                                <span>Top Position</span>
                                                <div class="clear-space-two"></div>
                                                <input type="number" id="moveresize-top" class="textEntry" style="width: 150px;" /><span class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                                <span>Left Position</span>
                                                <div class="clear-space-two"></div>
                                                <input type="number" id="moveresize-left" class="textEntry" style="width: 150px;" /><span class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                            </div>
                                            <div id="resize-div">
                                                <div class="clear">
                                                </div>
                                                <div class="pad-all">
                                                    <span>Width</span>
                                                    <div class="clear-space-two"></div>
                                                    <input type="number" id="moveresize-width" class="textEntry" style="width: 150px;" /><span class="pad-left">px</span>
                                                    <div class="clear-space">
                                                    </div>
                                                    <span>Height</span>
                                                    <div class="clear-space-two"></div>
                                                    <input type="number" id="moveresize-height" class="textEntry" style="width: 150px;" /><span class="pad-left">px</span>
                                                    <div class="clear-space">
                                                    </div>
                                                </div>
                                                <div class="clear-space"></div>
                                            </div>
                                            <div class="clear">
                                            </div>
                                        </asp:Panel>
                                    </div>
                                    <div id="load-option-btn-holder" class="margin-top">
                                        <a id="options-btn-device" href="#" class="option-buttons" onclick="return false;">
                                            <span class="img-open img-option"></span>Open App</a>
                                        <div class="clear">
                                        </div>
                                        <a id="options-btn-open" href="#" class="option-buttons" onclick="appRemote.LoadOnWorkspace();return false;">
                                            <span class="img-workspace img-option"></span>Load On Workspace</a>
                                        <div class="clear">
                                        </div>
                                        <a id="options-btn-update" href="#" class="option-buttons" onclick="appRemote.LoadOnWorkspace();return false;"
                                            style="display: none;"><span class="img-update img-option"></span>Refresh App</a>
                                        <div class="clear">
                                        </div>
                                        <a href="#" id="options-btn-close" class="option-buttons" onclick="appRemote.CloseAppOnWorkspace();return false;">
                                            <span class="img-close img-option"></span>Close App</a>
                                        <h3 id="no-options-available" class="pad-all" style="display: none;">No Options Available</h3>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="pnlContent_Login" runat="server" class="main-content-panels" visible="false">
                        <div id="Login-holder">
                            <asp:Label ID="lbl_LoginMessage" runat="server" Text=""></asp:Label>
                            <span id="account_active" style="text-align: center"></span>
                            <asp:Literal ID="ltl_logingrouperror" runat="server"></asp:Literal>
                            <div class="clear"></div>
                            <div class="loginwith-api-text">
                                Log Into Your Account
                            </div>
                            <div class="clear"></div>
                            <div class="login_panel_margins">
                                <asp:Login ID="Login1" runat="server" FailureText="<div style='color: #D80000; padding-bottom: 5px;'>Invalid Username or Password</div>"
                                    LoginButtonText="Login" TitleText="" Width="100%" DestinationPageUrl="Default.aspx"
                                    OnLoggingIn="Login_LoggingIn" OnLoggedIn="Login_Loggedin" OnLoginError="Login_error"
                                    Style="position: relative;">
                                    <LayoutTemplate>
                                        <asp:Panel ID="pnl_Login" runat="server" DefaultButton="LoginButton">
                                            <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                                            <div class="clear"></div>
                                            <div class="textbox-group-padding">
                                                <div class="textbox-group">
                                                    <div class="username-login-img"></div>
                                                    <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username" autocomplete="off"></asp:TextBox>
                                                </div>
                                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                    CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Username is required."
                                                    ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="clear-space"></div>
                                            <div class="textbox-group-padding">
                                                <div class="textbox-group">
                                                    <div class="password-login-img"></div>
                                                    <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox" placeholder="Password" autocomplete="off"></asp:TextBox>
                                                </div>
                                                <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                    CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Password is required."
                                                    ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="clear-space"></div>
                                            <asp:Button ID="LoginButton" runat="server" CommandName="Login" ValidationGroup="ctl00$Login1"
                                                Text="Log in" CssClass="input-buttons-create input-buttons-login" />
                                            <div class="clear"></div>
                                            <div id="rememberme-holder">
                                                <asp:CheckBox ID="RememberMe" runat="server" Text=" Remember me" />
                                            </div>
                                        </asp:Panel>
                                    </LayoutTemplate>
                                    <TextBoxStyle BackColor="White" />
                                    <ValidatorTextStyle Font-Bold="True" ForeColor="Red" />
                                </asp:Login>
                            </div>
                            <div class="pad-top-big margin-top-big" align="center">
                                <div class="loginwith-api-text">
                                    <span class="inline-block">Or Login with...</span>
                                </div>
                                <div class="clear"></div>
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
                            <asp:UpdatePanel ID="updatepnl_LoginWith" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div class="clear-space"></div>
                                    <div align="center">
                                        <a id="hyp_groupLogin" runat="server" href="#?tab=Groups_tab">Select a group to login to</a>
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
                                            CssClass="input-buttons-create input-buttons-login" Text="Send Email" ValidationGroup="UsernameRecovery" />
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
            </div>
            <div id="aboutApp-element" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="700">
                            <div class="ModalHeader">
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#close" onclick="openWSE.LoadModalWindow(false, 'aboutApp-element', '');return false;" class="ModalExitButton"></a>
                                    </div>
                                    <span class="img-about float-left margin-right-sml"></span>
                                    <span class="Modal-title"></span>
                                </div>
                            </div>
                            <asp:UpdatePanel ID="updatepnl_aboutHolder" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div class="ModalScrollContent">
                                        <div class="ModalPadContent">
                                            <asp:Panel ID="pnl_aboutHolder" runat="server">
                                            </asp:Panel>
                                        </div>
                                    </div>
                                    <div class="ModalButtonHolder">
                                        <a href="#" id="btn_uninstallApp" class="modal-ok-btn" runat="server" visible="false">
                                            <span class="td-subtract-btn float-left margin-right-sml" style="padding: 0px 2px 0 0!important; margin-top: 0px!important;"></span>Uninstall</a>
                                        <input type="button" value="Close" onclick="openWSE.LoadModalWindow(false, 'aboutApp-element', '');" class="input-buttons modal-cancel-btn" />
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
            <div id="footer_container" runat="server">
                <div id="copyright-footer" class="float-left"><asp:Literal ID="ltl_footercopyright" runat="server"></asp:Literal></div>
                <div class="help-icon" title="Need help?">
                </div>
                <div id="footer-signdate" class="float-right">
                    <a href="Default.aspx">Home</a><a href="About.aspx?redirect=AppRemote.aspx">About</a><a href="About.aspx?a=termsofuse&redirect=AppRemote.aspx">Terms</a>
                </div>
            </div>
            <asp:HiddenField ID="hf_chatsound" runat="server" ClientIDMode="Static" />
        </div>
    </form>
</body>
</html>
