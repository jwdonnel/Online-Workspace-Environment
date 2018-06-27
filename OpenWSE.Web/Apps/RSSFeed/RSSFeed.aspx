<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RSSFeed.aspx.cs" Inherits="Apps_RSSFeed_RSSFeed" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>RSS Feed Central</title>
    <meta name="application-name" content="RSS Feed Central" />
    <meta name="viewport" content="width=device-width, user-scalable=no" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta http-equiv="Language" content="en" />
    <meta http-equiv="Content-Language" content="en" />
    <meta name="author" content="John Donnelly" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="mobile-web-app-capable" content="yes" />
    <meta name="revisit-after" content="10 days" />
    <meta name="robots" content="INDEX, FOLLOW" />
    <meta name="description" content="A simple RSS Feed viewer that features top news sources with a simple and easy viewing experience." />
    <meta name="keywords" content="RSS,RSS news,news feed,RSS news feed,news veiwer,onlinewse.com,openwse.com,rss cards,rss full articles,rss search" />
    <link rel="Stylesheet" href="rssfeed.css" />
    <link rel="apple-touch-icon" sizes="57x57" href="Images/Favicon/apple-icon-57x57.png" />
    <link rel="apple-touch-icon" sizes="60x60" href="Images/Favicon/apple-icon-60x60.png" />
    <link rel="apple-touch-icon" sizes="72x72" href="Images/Favicon/apple-icon-72x72.png" />
    <link rel="apple-touch-icon" sizes="76x76" href="Images/Favicon/apple-icon-76x76.png" />
    <link rel="apple-touch-icon" sizes="114x114" href="Images/Favicon/apple-icon-114x114.png" />
    <link rel="apple-touch-icon" sizes="120x120" href="Images/Favicon/apple-icon-120x120.png" />
    <link rel="apple-touch-icon" sizes="144x144" href="Images/Favicon/apple-icon-144x144.png" />
    <link rel="apple-touch-icon" sizes="152x152" href="Images/Favicon/apple-icon-152x152.png" />
    <link rel="apple-touch-icon" sizes="180x180" href="Images/Favicon/apple-icon-180x180.png" />
    <link rel="icon" type="image/png" sizes="192x192" href="Images/Favicon/android-icon-192x192.png" />
    <link rel="icon" type="image/png" sizes="32x32" href="Images/Favicon/favicon-32x32.png" />
    <link rel="icon" type="image/png" sizes="96x96" href="Images/Favicon/favicon-96x96.png" />
    <link rel="icon" type="image/png" sizes="16x16" href="Images/Favicon/favicon-16x16.png" />
    <link rel="manifest" href="Images/Favicon/manifest.json" />
    <meta name="msapplication-TileColor" content="#ffffff" />
    <meta name="msapplication-TileImage" content="Images/Favicon/ms-icon-144x144.png" />
    <meta name="theme-color" content="#ffffff" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_rssfeed" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <asp:HiddenField ID="hf_appviewstyle" runat="server" ClientIDMode="Static" />
        <div id="always-visible">
            <div id="sidebar-menu-btn">
                <div class="sidebar-menubar-img-holder">
                    <div class="sidebar-menubar-img"></div>
                    <div class="sidebar-menubar-img"></div>
                    <div class="sidebar-menubar-img"></div>
                    <div class="clear"></div>
                </div>
            </div>
            <a href="RSSFeed.aspx#Highlights" class="top-logo">
                <img alt="logo-sml" src="Images/logo-20.png" />RSS Feed Central</a>
            <div id="currentSelectedCategory" data-value="Highlights">Highlights</div>
            <div id="categoryList-selector">
                <div data-value="Highlights">Highlights</div>
                <div data-value="Business / Stock Market">Business</div>
                <div data-value="World / U.S. News">World News</div>
                <div data-value="Technology">Technology</div>
                <div data-value="Sports">Sports</div>
                <div data-value="Video Games">Gaming</div>
                <div data-value="Entertainment">Entertainment</div>
                <div data-value="Science">Science</div>
                <div data-value="Education">Education</div>
                <div id="myFeedsLink" runat="server" visible="false" data-value="My Feeds">My Feeds</div>
                <div id="mySavedLink" runat="server" visible="false" data-value="Saved Feeds">Saved Feeds</div>
                <div id="myAlertsLink" runat="server" visible="false" data-value="My Alerts">My Alerts</div>
            </div>
            <a href="#Login" id="btn_LoginRegister" runat="server" onclick="feedStation.LoginRegisterModal(); return false;" style="display: none;">Login</a>
            <asp:LinkButton ID="lbtn_signoff" runat="server" OnClick="lbtn_signoff_Click" Visible="false" Style="display: none;">Log Out</asp:LinkButton>
            <div id="top-search">
                <div class="top-search-input-holder">
                    <input type="text" id="searchbox-feed-search" value="" placeholder="Search" onkeypress="feedStation.keyPressSearch(event);" />
                </div>
                <span class="start-search" onclick="feedStation.searchFeeds(this);"></span>
                <span class="clear-search" onclick="feedStation.clearSearch();"></span>
            </div>
        </div>
        <div id="main-content">
            <div id="feed-holder">
                <div class="loading-icon"></div>
            </div>
            <div class="clear"></div>
        </div>
        <div id="rss-sidebar">
            <div class="rss-sidebar-topbar">
                <div class="top-logo-sidebar">
                    <img alt="logo-sml" src="Images/logo-20.png" />RSS Feed Central
                </div>
                <div id="close-sidebar-btn" title="Close menu"></div>
                <div class="clear"></div>
            </div>
            <div class="pad-all">
                <div id="pnl_loginlogout_sidebar">
                    <a href="#Login" id="btn_loginRegister_sidebar" runat="server" onclick="feedStation.LoginRegisterModal(); return false;" style="display: none;">Login</a>
                    <asp:LinkButton ID="lbtn_signoff_sidebar" runat="server" OnClick="lbtn_signoff_Click" Visible="false" Style="display: none;">Log Out</asp:LinkButton>
                </div>
                <div class="clear"></div>
                <div id="top-search-sidebar">
                    <div class="top-search-input-holder">
                        <input type="text" id="searchbox-feed-search-sidebar" value="" placeholder="Search" onkeypress="feedStation.keyPressSearch(event);" />
                    </div>
                    <span class="start-search" onclick="feedStation.searchFeeds(this);"></span>
                    <span class="clear-search" onclick="feedStation.clearSearch();"></span>
                </div>
                <ul>
                    <li>
                        <div class="section-pad" style="text-align: center;">
                            <a id="rss-sidebar-refresh" class="sidebar-menu-buttons" onclick="feedStation.refreshFeeds();"><span class="refresh-image"></span>Refresh Feeds</a>
                            <div class="clear"></div>
                            <div id="pnl_EditFeeds" runat="server" visible="false">
                                <a class="sidebar-menu-buttons" onclick="feedStation.editFeeds(); return false;"><span class="edit-image"></span>Manage Sources</a>
                                <div class="clear"></div>
                            </div>
                            <div id="pnl_EditFeedAlerts" runat="server" visible="false">
                                <a class="sidebar-menu-buttons" onclick="feedStation.editAlerts(); return false;"><span class="alert-image"></span>Manage Alerts</a>
                                <div class="clear"></div>
                            </div>
                        </div>
                    </li>
                    <li>
                        <div class="section-pad">
                            <h3 class="section-pad-title" style="padding-top: 3px!important;">Interval</h3>
                            <select id="select_feedinterval" class="float-right" onchange="feedStation.refreshInterval_Changed(this);">
                                <option value="0">Off</option>
                                <option value="1">1 minute</option>
                                <option value="2">2 minutes</option>
                                <option value="5">5 minutes</option>
                                <option value="10">10 minutes</option>
                            </select>
                            <div class="clear"></div>
                        </div>
                    </li>
                    <li id="select-viewmode-selector">
                        <div class="section-pad">
                            <h3 class="section-pad-title" style="padding-top: 3px!important;">View</h3>
                            <select id="select_viewmode" class="float-right" onchange="feedStation.changeViewMode_Changed(this);">
                                <option value="card">Card Previews</option>
                                <option value="full">Full Articles</option>
                            </select>
                            <div class="clear"></div>
                        </div>
                    </li>
                    <li id="desktop-column-selector">
                        <div class="section-pad">
                            <h3 class="section-pad-title" style="padding-top: 3px!important;">Columns</h3>
                            <select id="select_desktopColumns" class="float-right" onchange="feedStation.updateTotalDesktopColumns_Changed(this);">
                                <option value="1">1</option>
                                <option value="2">2</option>
                                <option value="3">3</option>
                                <option value="4">4</option>
                                <option value="5">5</option>
                                <option value="6">6</option>
                                <option value="7">7</option>
                                <option value="8">8</option>
                                <option value="9">9</option>
                                <option value="10">10</option>
                            </select>
                            <div class="clear"></div>
                        </div>
                    </li>
                </ul>
                <div class="clear-space"></div>
                <asp:Panel ID="pnl_AdminRSSFeedSettings" runat="server" Visible="false" Enabled="false">
                    <h3 class="font-bold">More Settings</h3>
                    <div class="clear-space"></div>
                    <a href="#" onclick="feedStation.Admin_GrabLatestFeeds();return false;">Grab the latest feeds</a>
                    <div class="clear-space-five"></div>
                    <a href="#" onclick="feedStation.Admin_ClearFeedList();return false;">Clear Loaded Feed List</a>
                    <div class="clear-space-five"></div>
                    <a href="#" onclick="feedStation.Admin_LoadStoredFeedList();return false;">Load From Stored File</a>
                    <div class="clear-space-five"></div>
                    <a href="#" onclick="feedStation.Admin_GetLoadStoredFeedListCount();return false;">Get Total Loaded Feed Count</a>
                    <div class="clear-space"></div>
                </asp:Panel>
                <div class="clear"></div>
            </div>
        </div>
        <div id="RSS-Feed-Selector-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="620">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'RSS-Feed-Selector-element', '');$('#AddRSSFeedHolder').html('');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:Panel ID="pnl_addcustomerrss" runat="server" DefaultButton="btn_addcustomrss">
                                    <input id="tb_addcustomrss" type="text" class="textEntry" placeholder="Link to RSS feed" style="width: 100%;" />
                                    <div class="clear-space"></div>
                                    <asp:Button ID="btn_addcustomrss" runat="server" CssClass="input-buttons"
                                        OnClientClick="feedStation.AddCustomRSSUrl();return false;" Text="Add Custom Feed" />
                                </asp:Panel>
                                <div id="rssadderror" class="clear" style="height: 25px;">
                                </div>
                                <div id="AddRSSFeedHolder">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="pnl_LoginRegister" class="Modal-element" runat="server" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'pnl_LoginRegister', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div id="login-modal-div" class="pad-left pad-right pad-bottom pad-top-sml">
                                    <div id="Login-holder">
                                        <asp:Label ID="lbl_LoginMessage_Master" runat="server" Text=""></asp:Label>
                                        <table border="0" cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td valign="top">
                                                    <div class="pad-left">
                                                        <div id="SocialLogin_borderSep">
                                                            <div class="loginwith-api-text">
                                                                Log Into Your Account
                                                            </div>
                                                            <div class="clear"></div>
                                                            <asp:Login ID="Login1" runat="server" FailureText="<div class='clear-space'></div><div style='color: #D80000; padding-bottom: 5px;'>Invalid Username or Password.</div>"
                                                                LoginButtonText="Login" TitleText="" Width="100%" DestinationPageUrl="Default.aspx"
                                                                OnLoggingIn="Login_LoggingIn" OnLoggedIn="Login_Loggedin" OnLoginError="Login_error"
                                                                Style="position: relative; z-index: 5000">
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
                                                    </div>
                                                </td>
                                                <td id="sociallogin_td" runat="server" valign="top">
                                                    <asp:UpdatePanel ID="updatepnl_socialLogin" runat="server">
                                                        <ContentTemplate>
                                                            <div class="loginwith-api-text">
                                                                Or Login With
                                                            </div>
                                                            <div class="clear"></div>
                                                            <asp:LinkButton ID="lbtn_signinwith_Google" runat="server" CssClass="loginwith-api google-btn margin-right" OnClick="lbtn_signinwith_Google_Click">
                                                                <span class="google-login-img"></span><span class="google-login-text">Login with Google</span>
                                                            </asp:LinkButton>
                                                            <div class="clear"></div>
                                                            <asp:LinkButton ID="lbtn_signinwith_Twitter" runat="server" CssClass="loginwith-api twitter-btn margin-right" OnClick="lbtn_signinwith_Twitter_Click">
                                                                <span class="twitter-login-img"></span><span class="twitter-login-text">Login with Twitter</span>
                                                            </asp:LinkButton>
                                                            <div class="clear"></div>
                                                            <asp:LinkButton ID="lbtn_signinwith_Facebook" runat="server" CssClass="loginwith-api facebook-btn margin-right" OnClick="lbtn_signinwith_Facebook_Click">
                                                                <span class="facebook-login-img"></span><span class="facebook-login-text">Login with Facebook</span>
                                                            </asp:LinkButton>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </td>
                                            </tr>
                                        </table>
                                        <div align="center">
                                            <a href="#" id="lnk_forgotpassword" class="margin-left margin-right margin-top font-color-black" onclick="openWSE.LoadRecoveryPassword();return false;">Forgot your Password?</a>
                                            <a href="#" id="login_register_link" class="margin-left margin-right margin-top font-color-black" onclick="openWSE.LoadCreateAccountHolder();return false;">Create a new account</a>
                                        </div>
                                    </div>
                                    <div id="CreateAccount-holder" style="display: none;">
                                        <div id="iframe-createaccount-holder">
                                        </div>
                                    </div>
                                    <div id="ForgotPassword-holder" style="display: none;">
                                        <div class="clear-space"></div>
                                        <h4>Please provide your username. An email will be sent to this account using the email address associated with your account. Click the link in the email to recover your account. Always change your password after recovering your account.</h4>
                                        <div class="clear-space"></div>
                                        <table cellpadding="5" cellspacing="5" style="margin: 0 auto;">
                                            <tr>
                                                <td style="text-align: left;">
                                                    <asp:UpdatePanel ID="updatepnl_forgotPassword" runat="server" UpdateMode="Conditional">
                                                        <ContentTemplate>
                                                            <asp:Panel ID="pnl_forgotPassword" runat="server" DefaultButton="btn_passwordrecovery">
                                                                <div class="textbox-group-padding">
                                                                    <div class="textbox-group">
                                                                        <div class="username-login-img"></div>
                                                                        <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox" placeholder="Username"></asp:TextBox>
                                                                    </div>
                                                                    <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                                                        ErrorMessage="" Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                                                        ValidationGroup="UsernameRecovery">*</asp:RequiredFieldValidator>
                                                                </div>
                                                                <div class="clear-space"></div>
                                                                <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                                                    CssClass="input-buttons-create input-buttons-login" Text="Send Email" ValidationGroup="UsernameRecovery" />
                                                            </asp:Panel>
                                                            <asp:Label ID="lbl_passwordResetMessage" runat="server" Text=""></asp:Label>
                                                        </ContentTemplate>
                                                        <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="btn_passwordrecovery" />
                                                        </Triggers>
                                                    </asp:UpdatePanel>
                                                </td>
                                            </tr>
                                        </table>
                                        <div class="clear-space"></div>
                                        <div class="clear-space"></div>
                                    </div>
                                    <div style="text-align: center;">
                                        <a href="#" id="register_password_cancel" onclick="openWSE.LoadCreateAccountHolder();return false;" style="display: none;">Cancel</a>
                                        <div class="clear-space"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="RSS-Feed-Alerts-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="400">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'RSS-Feed-Alerts-element', '');$('#AddRSSFeedHolder').html('');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                Create alerts based off of keywords that will be searched when compiling the list of feeds. To enable alerts, go to your <a href="../../SiteTools/UserTools/AcctSettings.aspx#?tab=pnl_NotificationSettings" target="_blank">Account Settings</a>.
                                <div class="clear-space"></div>
                                <input id="tb_alertKeyword" type="text" onkeypress="feedStation.addAlert_KeyPress(event);" class="textEntry margin-right margin-bottom-sml float-left" placeholder="Keyword" style="width: 220px;" />
                                <a href="#" class="td-add-btn" onclick="feedStation.addAlert();return false;" title="Add Keyword"></a>
                                <div class="clear-space-five"></div>
                                <div id="rssKeywords"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script src="rssfeed.js" type="text/javascript"></script>
    </form>
</body>
</html>
