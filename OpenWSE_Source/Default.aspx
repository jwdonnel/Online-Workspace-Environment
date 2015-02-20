<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login Portal</title>
    <meta name="description" content="" />
    <meta name="keywords" content="apps,app,social,jquery draggables,john donnelly,workspace,workspaces,chat,messenger,so source,gemicon.net,iconfinder.com,media,file upload,file share,customize,social networking,groups,friend chat,friend hangout" />
    <meta name="author" content="John Donnelly" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <link rel="stylesheet" href="App_Themes/Standard/jqueryUI.css" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <div class="header">
            <a id="header_backbtn" href="#" onclick="QikPages.iframeSetup('','');return false;" style="display: none;">Close</a>
            <h3 id="header_site_title" runat="server"></h3>
        </div>
        <table id="table-container" cellpadding="0" cellspacing="0">
            <tr>
                <td class="left-panel">
                    <div id="pnl_purchaseFullVer" runat="server" class="panel-link" onclick="window.location.href='SiteTools/ServerMaintenance/LicenseManager.aspx?purchase=true';" visible="false">
                        <div class="btn-sprite purchase-lrg"></div>
                        <span>Purchase Full Version</span>
                    </div>
                    <div id="pnl_FooterPasswordRec" runat="server" class="panel-link" onclick="QikPages.UpdateURL('passwordrecoverydiv', false);">
                        <div class="btn-sprite password"></div>
                        <span>Forgot Password?</span>
                    </div>
                    <div id="pnl_FooterRegister" runat="server" onclick="QikPages.UpdateURL('registerdiv', false);" class="panel-link">
                        <div class="btn-sprite add-user"></div>
                        <span>Create New Account</span>
                    </div>
                    <div id="pnl_goback" runat="server" class="panel-link" onclick="QikPages.UpdateURL('logindiv', false);" style="display: none">
                        <div class="btn-sprite back-lrg"></div>
                        <span>Go Back</span>
                    </div>
                    <div id="pnl_preview" runat="server" onclick="QikPages.UpdateURL('Workspace.aspx?Demo=true&iframeName=Site Preview', true);" class="panel-link">
                        <div class="btn-sprite preview"></div>
                        <span>Preview Site</span>
                    </div>
                    <div class="panel-link" onclick="QikPages.UpdateURL('About.aspx&iframeName=About', true);">
                        <div class="btn-sprite about"></div>
                        <span>About OpenWSE</span>
                    </div>
                    <div class="footer">
                        <h3>John Donnelly | OpenWSE 2015 | <a href="AppRemote.aspx">Mobile</a><asp:Label ID="lblMainLoginLink" runat="server" Text=""></asp:Label></h3>
                    </div>
                </td>
                <td class="container">
                    <div class="logo-holder">
                        <img id="mainLoginLogo" runat="server" alt="Logo" src="Standard_Images/logo.png" />
                    </div>
                    <div id="logindiv" class="container-box">
                        <h4 id="lbl_login_help" runat="server"></h4>
                        <div class="clear-space"></div>
                        <span id="account_active" style="text-align: center"></span>
                        <asp:Literal ID="ltl_logingrouperror" runat="server"></asp:Literal>
                        <table border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto;">
                            <tr>
                                <td valign="top">
                                    <div id="SocialLogin_borderSep">
                                        <asp:Login ID="Login1" runat="server" FailureText="<div style='color: #D80000;'>Invalid Username or Password. Please try again</div>"
                                            LoginButtonText="Login" TitleText="" Width="100%" DestinationPageUrl="Workspace.aspx"
                                            OnLoggingIn="Login_LoggingIn" OnLoggedIn="Login_Loggedin" OnLoginError="Login_error"
                                            Style="position: relative; z-index: 5000">
                                            <LayoutTemplate>
                                                <asp:Panel ID="pnl_Login" runat="server" DefaultButton="LoginButton">
                                                    <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                                                    <table cellpadding="5" cellspacing="5" style="text-align: left">
                                                        <tr>
                                                            <td>
                                                                <span class="login-td-text">Username</span><br />
                                                                <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                    ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                                                    ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <div class="clear-space-five"></div>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <span class="login-td-text">Password</span><br />
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
                                                                    Text="Login" CssClass="input-buttons-login" />
                                                                <div id="rememberme-holder">
                                                                    <asp:CheckBox ID="RememberMe" runat="server" Text=" Remember me" />
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </asp:Panel>
                                            </LayoutTemplate>
                                            <TextBoxStyle BackColor="White" />
                                            <ValidatorTextStyle Font-Bold="True" ForeColor="Red" />
                                        </asp:Login>
                                    </div>
                                </td>
                                <td valign="top">
                                    <asp:UpdatePanel ID="updatepnl_socialLogin" runat="server">
                                        <ContentTemplate>
                                            <div class="loginwith-api-text">
                                                <div><span class="inline-block">Sign in with...</span></div>
                                                <hr class="soften" />
                                            </div>
                                            <div class="clear-space"></div>
                                            <asp:LinkButton ID="lbtn_signinwith_Google" runat="server" CssClass="loginwith-api google-btn margin-right" OnClick="lbtn_signinwith_Google_Click">
                                                                <span class="google-login-img"></span><span class="google-login-text">Sign in with Google</span>
                                            </asp:LinkButton>
                                            <div class="clear"></div>
                                            <asp:LinkButton ID="lbtn_signinwith_Twitter" runat="server" CssClass="loginwith-api twitter-btn margin-right" OnClick="lbtn_signinwith_Twitter_Click">
                                                                <span class="twitter-login-img"></span><span class="twitter-login-text">Sign in with Twitter</span>
                                            </asp:LinkButton>
                                            <div class="clear"></div>
                                            <asp:LinkButton ID="lbtn_signinwith_Facebook" runat="server" CssClass="loginwith-api facebook-btn margin-right" OnClick="lbtn_signinwith_Facebook_Click">
                                                                <span class="facebook-login-img"></span><span class="facebook-login-text">Sign in with Facebook</span>
                                            </asp:LinkButton>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </td>
                            </tr>
                        </table>
                        <asp:Label ID="lbl_LoginMessage" runat="server" Text=""></asp:Label>
                    </div>
                    <div id="passwordrecoverydiv" runat="server" class="container-box" style="display: none;">
                        <h4>Please provide your username. An email will be sent to this account using the email
                    address associated with your account. Click the link in the email to recover your
                    account. Always change your password after recovering your account.</h4>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_password_recovery" runat="server" DefaultButton="btn_passwordrecovery">
                            <table cellpadding="5" cellspacing="5" style="margin: 0 auto;">
                                <tr>
                                    <td style="text-align: left;">
                                        <asp:UpdatePanel ID="updatepnl_passwordrecovery" runat="server">
                                            <ContentTemplate>
                                                <span class="login-td-text">Username</span><br />
                                                <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                                    ErrorMessage="User Name is required." Font-Bold="True" ForeColor="Red" ToolTip="User Name is required."
                                                    ValidationGroup="UsernameRecovery">*</asp:RequiredFieldValidator>
                                                <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                                    CssClass="input-buttons margin-left-sml" Text="Send Email" ValidationGroup="UsernameRecovery" />
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </div>
                    <div id="registerdiv" runat="server" class="container-box" style="display: none;">
                        <asp:Panel ID="pnl_Register" runat="server">
                            <asp:Label ID="lbl_email_assocation" runat="server"></asp:Label>
                            <div class="clear-space">
                            </div>
                            <asp:CreateUserWizard ID="RegisterUser" runat="server" OnCreatedUser="RegisterUser_CreatedUser"
                                OnCreatingUser="RegisterUser_CreatingUser" OnContinueButtonClick="RegisterUser_Continue"
                                LoginCreatedUser="false" Width="100%" ContinueButtonStyle-CssClass="input-buttons input-buttons-continue-reg"
                                CompleteSuccessText="Your account has been successfully created. You may now login.">
                                <LayoutTemplate>
                                    <asp:PlaceHolder ID="wizardStepPlaceholder" runat="server"></asp:PlaceHolder>
                                    <asp:PlaceHolder ID="navigationPlaceholder" runat="server"></asp:PlaceHolder>
                                </LayoutTemplate>
                                <WizardSteps>
                                    <asp:CreateUserWizardStep ID="RegisterUserWizardStep" runat="server">
                                        <ContentTemplate>
                                            <div style="padding-bottom: 20px">
                                                Passwords are required to be a minimum of <%= Membership.MinRequiredPasswordLength %> characters in length.
                                            </div>
                                            <table class="float-left">
                                                <tbody>
                                                    <tr>
                                                        <td align="left">
                                                            <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName" CssClass="login-td-text">User Name</asp:Label>
                                                            <br />
                                                            <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox"></asp:TextBox>
                                                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="User Name is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <div class="clear-space">
                                                            </div>
                                                            <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email" CssClass="login-td-text">E-mail</asp:Label>
                                                            <br />
                                                            <asp:TextBox ID="Email" runat="server" CssClass="signintextbox"></asp:TextBox>
                                                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="E-mail is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <div class="clear-space">
                                                            </div>
                                                            <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password" CssClass="login-td-text">Password</asp:Label>
                                                            <br />
                                                            <asp:TextBox ID="Password" runat="server" CssClass="signintextbox" TextMode="Password"></asp:TextBox>
                                                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                                CssClass="failureNotification" ErrorMessage="" ToolTip="Password is required."
                                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <div class="clear-space">
                                                            </div>
                                                            <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword" CssClass="login-td-text">Confirm Password</asp:Label>
                                                            <br />
                                                            <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="signintextbox" TextMode="Password"></asp:TextBox>
                                                            <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                                Display="Dynamic" ErrorMessage="" ID="ConfirmPasswordRequired" runat="server"
                                                                ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                                ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                                                ErrorMessage="" ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <table class="float-right">
                                                <tbody>
                                                    <tr>
                                                        <td align="left">
                                                            <span class="login-td-text">First Name</span>
                                                            <br />
                                                            <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="signintextbox"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <div class="clear-space">
                                                            </div>
                                                            <span class="login-td-text">Last Name</span>
                                                            <br />
                                                            <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="signintextbox"></asp:TextBox>

                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <div class="clear-space">
                                                            </div>
                                                            <span class="login-td-text">User Color</span>
                                                            <br />
                                                            <div class="img-colors float-left">
                                                            </div>
                                                            <asp:TextBox runat="server" ID="Color1" CssClass="signintextbox float-left color"
                                                                MaxLength="6" AutoCompleteType="None" Width="65px" />
                                                            <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup"
                                                                CssClass="input-buttons margin-left-big" Text="Register"></asp:Button>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-margin">
                                                <div class="clear-space">
                                                </div>
                                                <span class="failureNotification">
                                                    <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                                                    <span id="failureMessage"></span></span>
                                                <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
                                                    ValidationGroup="RegisterUserValidationGroup" />
                                            </div>
                                        </ContentTemplate>
                                        <CustomNavigationTemplate>
                                        </CustomNavigationTemplate>
                                    </asp:CreateUserWizardStep>
                                </WizardSteps>
                            </asp:CreateUserWizard>
                        </asp:Panel>
                        <div class="clear"></div>
                    </div>
                </td>
            </tr>
        </table>
        <div id="iframe-Holder" style="display: none;">
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
        <script type="text/javascript" src="Scripts/jquery/combined-scripts.min.js"></script>
        <script type="text/javascript" src="WebControls/jscolor/jscolor.js"></script>
        <script type="text/javascript">
            var siteName = "";
            var QikPages = function () {
                var siteTheme = "Standard";
                var animationSpeed = 200;

                function init() {
                    $("#Login1_UserName").focus();
                    HashChange();
                }
                function resizePage() {
                    var headerHt = $(".header").outerHeight();
                    var windowHt = $(window).height();
                    $("#table-container").innerHeight(windowHt - headerHt);
                }

                function passwordRecovery() {
                    if ($("#passwordrecoverydiv").length > 0) {
                        $("#pnl_FooterPasswordRec, #pnl_FooterRegister, .container-box").hide();
                        $("#pnl_goback").show();
                        $("#passwordrecoverydiv").fadeIn(animationSpeed, function () {
                            $("#header_site_title").html("Password Recovery");
                            $("#tb_username_recovery").focus();
                        });
                    }
                }
                function register() {
                    if ($("#registerdiv").length > 0) {
                        $("#pnl_FooterPasswordRec, #pnl_FooterRegister, .container-box").hide();
                        $("#pnl_goback").show();
                        $("#registerdiv").fadeIn(animationSpeed, function () {
                            $("#header_site_title").html("Register New Account");
                            $("#RegisterUser_CreateUserStepContainer_UserName").focus();
                        });
                    }
                }
                function registerFinish() {
                    $("#pnl_FooterPasswordRec, #pnl_FooterRegister, #pnl_goback, .container-box").hide();
                    $("#header_site_title").html("Register New Account");
                    $("#registerdiv").show();
                    UpdateURL("registerdiv", false);
                }
                function goBack() {
                    $("#pnl_goback, .container-box, #header_backbtn, #iframe-Holder").hide();
                    $("#table-container, #pnl_FooterPasswordRec, #pnl_FooterRegister").show();
                    $("#RegisterUser_CreateUserStepContainer_UserName,#RegisterUser_CreateUserStepContainer_Email,#RegisterUser_CreateUserStepContainer_tb_firstnamereg,#RegisterUser_CreateUserStepContainer_tb_lastnamereg,#RegisterUser_CreateUserStepContainer_Password,#RegisterUser_CreateUserStepContainer_ConfirmPassword,#RegisterUser_CreateUserStepContainer_Color1").val("");
                    $("#header_site_title").html(siteName + " Login Portal");
                    $("#iframe-Holder").html("");
                    $("#logindiv").fadeIn(animationSpeed, function () {
                        $("#Login1_UserName").focus();
                    });
                }

                function iframeSetup(name, iframeUrl) {
                    if ($("#iframe-Holder").css("display") == "none") {
                        $("#header_backbtn").show();
                        $("#table-container").hide();
                        $("#header_backbtn").html("Go Back");
                        $("#iframe-Holder").html("<iframe id='iframe-demo' src='" + iframeUrl + "' frameborder='0' width='100%' style='visibility: hidden;'></iframe>");
                        $("#iframe-Holder").append("<div id='loading-background-holder'><div>");
                        var iframeHeight = $(window).height() - $(".header").outerHeight();

                        $("#header_site_title").html(name);

                        $("#iframe-demo").load(function () {
                            $("#loading-background-holder").fadeOut(animationSpeed, function () {
                                $("#loading-background-holder").remove();
                            });

                            $("#iframe-demo").css({
                                height: iframeHeight,
                                visibility: "visible"
                            });
                        });

                        $(window).resize(function () {
                            var iframeHeight = $(window).height() - $(".header").outerHeight();
                            $("#iframe-demo").css({
                                height: iframeHeight
                            });
                        });
                        $("#iframe-Holder").fadeIn(animationSpeed);

                    }
                    else {
                        hideIframe();
                        UpdateURL("", false);
                    }
                }
                function hideIframe() {
                    if ($("#iframe-Holder").css("display") == "block") {
                        $("#iframe-Holder").fadeOut(animationSpeed, function () {
                            $("#header_site_title").html(siteName + " Login Portal");
                            $("#header_backbtn").hide();
                            $("#table-container").show();
                            $("#iframe-Holder").html("");
                        });
                    }
                }

                function HashChange() {
                    try {
                        var query = "";
                        var loc = window.location.href.split("#?");
                        if (loc.length > 1) {
                            loc = loc[1];
                            var locSplit = loc.split("=");
                            if (locSplit.length > 1) {
                                query = locSplit[0];
                                loc = "";
                                for (var i = 1; i < locSplit.length; i++) {
                                    loc += locSplit[i];
                                    if (i < locSplit.length - 1) {
                                        loc += "=";
                                    }
                                }
                            }
                        }

                        if (query == "tab") {
                            hideIframe();
                            switch (loc) {
                                case "logindiv":
                                    goBack();
                                    break;
                                case "passwordrecoverydiv":
                                    passwordRecovery();
                                    break;
                                case "registerdiv":
                                    register();
                                    break;
                            }
                        }
                        else if (query == "iframe") {
                            hideIframe();
                            var name = "";
                            var locSplit = loc.split("&iframeName=");
                            if (locSplit.length > 1) {
                                loc = locSplit[0];
                                name = locSplit[1];
                            }
                            iframeSetup(name, loc);
                        }
                        else {
                            goBack();
                        }
                    }
                    catch (evt) { }
                }

                function UpdateURL(tab, iframe) {
                    var loc = window.location.href.split("#?");
                    if (loc.length > 0) {
                        loc = loc[0];
                    }

                    if (loc.indexOf("#") != loc.length - 1) {
                        loc += "#";
                    }

                    var query = "tab";
                    if (iframe) {
                        query = "iframe";
                    }

                    if (tab != "") {
                        loc += "?" + query + "=" + tab;
                    }

                    window.location = loc;
                }

                return {
                    init: init,
                    resizePage: resizePage,
                    registerFinish: registerFinish,
                    iframeSetup: iframeSetup,
                    UpdateURL: UpdateURL,
                    HashChange: HashChange
                }
            }();

            $(window).resize(function () {
                QikPages.resizePage();
            });

            $(function () {
                $(window).hashchange(function () {
                    QikPages.HashChange();
                });
            });

            $(document).ready(function () {
                QikPages.init();
                QikPages.resizePage();
            });

            function colorChanged(sender) {
                sender.get_element().style.color = "#" + sender.get_selectedColor();
            }

            function SetTrialText(exp) {
                var text = "<div class='trial-version-text'><span>Trial Version</span><span class='float-right'>Expires in " + exp + "</span></div>";
                $(".container").prepend(text);
            }
        </script>
    </form>
    <noscript>
        <meta http-equiv="refresh" content="0; URL=ErrorPages/browserchecker_nojs.html" />
    </noscript>
</body>
</html>
