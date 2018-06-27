<%@ Page Title="Login Portal" Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        .Modal-element-static { transform: none!important; }
            .Modal-element-static.small-screen .ModalHeader-static { text-align: left!important; }
        #Login_element.small-screen td { display: block; clear: both; }
        #Login_element.small-screen .loginwith-api-text { text-align: left; }
        #Login_element.small-screen .loginwith-api-borderseperate { border-right: none!important; padding-right: 0!important; margin-right: 0!important; padding-bottom: 20px; margin-bottom: 20px; border-bottom: 1px solid #DDD; }
    </style>
</head>
<body id="site_mainbody" runat="server" class="login-page">
    <form id="form1" runat="server" autocomplete="off">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <h1 style="display: none;">Login Portal</h1>
        <div id="top_bar" runat="server">
            <div id="top-logo-holder">
                <a id="lnk_BackToWorkspace" runat="server" onclick="return false;" style="cursor: default!important;"></a>
            </div>
            <div id="top-button-holder">
                <div id="group_tab" runat="server" class="top-bar-menu-button">
                    <div class="active-div"></div>
                    <ul>
                        <li class="a group-menu-toggle" title="View and log into a group"></li>
                        <li class="b">
                            <div class="li-header">
                                Available Groups
                            </div>
                            <div class="li-pnl-tab">
                                <div id="grouplistdiv">
                                </div>
                            </div>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="clear"></div>
        </div>
        <div class="fixed-container-border-left"></div>
        <div class="fixed-container-border-right"></div>
        <div class="fixed-container-holder-background">
            <div class="fixed-container-holder">
                <div id="main_container" runat="server" style="z-index: 1; left: 0;">
                    <div id="Login_element" runat="server" class="Modal-element-static">
                        <div class="ModalHeader-static"></div>
                        <asp:Label ID="lbl_LoginMessage" runat="server" Text=""></asp:Label>
                        <span id="account_active" style="text-align: center"></span>
                        <asp:Literal ID="ltl_logingrouperror" runat="server"></asp:Literal>
                        <table cellpadding="0" cellspacing="0" border="0" width="100%">
                            <tr>
                                <td valign="top">
                                    <div id="SocialLogin_borderSep">
                                        <div class="loginwith-api-text">
                                            Log Into Your Account
                                        </div>
                                        <div class="clear"></div>
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
                        <div class="clear"></div>
                        <div class="login-link-holder">
                            <div id="pnl_FooterPasswordRec" runat="server" class="login-links" data-elementid="PasswordRecovery_element" onclick="UpdateModalUrl('PasswordRecovery_element');">
                                Forgot your password?
                            </div>
                            <div id="pnl_FooterRegister" runat="server" data-elementid="Register_element" onclick="UpdateModalUrl('Register_element');" class="login-links">
                                Sign up
                            </div>
                            <div id="pnl_preview" runat="server" onclick="openWSE.LoadIFrameContent('Default.aspx?Demo=true&fullscreen=true');" class="login-links">
                                Preview site
                            </div>
                            <div id="pnl_CancelGroupLogin" runat="server" class="login-links" visible="false">
                                Cancel group login
                            </div>
                        </div>
                        <div class="clear"></div>
                    </div>
                    <div id="Register_element" runat="server" class="Modal-element-static" style="width: 560px;">
                        <div class="ModalHeader-static"></div>
                        <asp:Panel ID="pnl_Register" runat="server">
                            <asp:Label ID="lbl_email_assocation" runat="server"></asp:Label>
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
                                            <small>Passwords are required to be a minimum of <%= Membership.MinRequiredPasswordLength %> characters in length.</small>
                                            <div class="clear"></div>
                                            <div class="float-left margin-right margin-top">
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="username-login-img"></div>
                                                        <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username"></asp:TextBox>
                                                    </div>
                                                    <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                        CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="User Name is required."
                                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                </div>
                                                <div class="clear-space"></div>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="email-login-img"></div>
                                                        <asp:TextBox ID="Email" runat="server" CssClass="signintextbox" placeholder="email"></asp:TextBox>
                                                    </div>
                                                    <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                        CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="E-mail is required."
                                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                </div>
                                                <div class="clear-space"></div>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="password-login-img"></div>
                                                        <asp:TextBox ID="Password" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Password"></asp:TextBox>
                                                    </div>
                                                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                        CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Password is required."
                                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                </div>
                                                <div class="clear-space"></div>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="password-login-img"></div>
                                                        <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Confirm Password"></asp:TextBox>
                                                    </div>
                                                    <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                        Display="Dynamic" ErrorMessage="" ForeColor="Red" ID="ConfirmPasswordRequired" runat="server"
                                                        ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                    <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                        ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                                        ErrorMessage="" ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                                </div>
                                            </div>
                                            <div class="float-left margin-top">
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="empty-login-img"></div>
                                                        <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="signintextbox" placeholder="First Name"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="clear-space"></div>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="empty-login-img"></div>
                                                        <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="signintextbox" placeholder="Last Name"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="clear-space"></div>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="colors-login-img">
                                                        </div>
                                                        <asp:TextBox runat="server" ID="Color1" CssClass="signintextbox"
                                                            MaxLength="6" AutoCompleteType="None" TextMode="Color" />
                                                    </div>
                                                </div>
                                                <div class="clear-space"></div>
                                                <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup"
                                                    CssClass="input-buttons-create input-buttons-login" Text="Register"></asp:Button>
                                            </div>
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
                        <div class="login-links" data-elementid="Login_element" onclick="UpdateModalUrl('');" style="text-align: center;">
                            Back to Login
                        </div>
                        <div class="clear"></div>
                    </div>
                    <div id="PasswordRecovery_element" runat="server" class="Modal-element-static" style="width: 450px;">
                        <div class="ModalHeader-static"></div>
                        Enter your email address that you used to register. An email will be sent to this account using the email address associated with your account. Click the link in the email to recover your account. Always change your password after recovering your account.
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_password_recovery" runat="server" DefaultButton="btn_passwordrecovery">
                            <table cellpadding="5" cellspacing="5" style="margin: 0 auto;">
                                <tr>
                                    <td style="text-align: left;">
                                        <asp:UpdatePanel ID="updatepnl_passwordrecovery" runat="server">
                                            <ContentTemplate>
                                                <div class="textbox-group-padding">
                                                    <div class="textbox-group">
                                                        <div class="username-login-img"></div>
                                                        <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username"></asp:TextBox>
                                                    </div>
                                                    <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                                        ErrorMessage="" ValidationGroup="UsernameRecovery" CssClass="failureNotification" ForeColor="Red">*</asp:RequiredFieldValidator>
                                                </div>
                                                <div class="clear-space"></div>
                                                <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                                    CssClass="input-buttons-create input-buttons-login" Text="Send Email" ValidationGroup="UsernameRecovery" />
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div class="login-links" data-elementid="Login_element" onclick="UpdateModalUrl('');" style="text-align: center;">
                            Back to Login
                        </div>
                        <div class="clear"></div>
                    </div>
                </div>
                <div id="footer_container" runat="server" style="left: 0;">
                    <div id="copyright-footer" class="float-left">
                        <asp:Literal ID="ltl_footercopyright" runat="server"></asp:Literal>
                    </div>
                    <div id="footer-signdate" class="float-right">
                        <a href="#" title="Open Mobile Workspace" class="my-app-remote-link" onclick="return openWSE.OpenMobileWorkspace();">Mobile</a><a href="#iframecontent" onclick="openWSE.LoadIFrameContent('About.aspx');return false;">About</a><a href="#iframecontent" onclick="openWSE.LoadIFrameContent('About.aspx?a=termsofuse');return false;">Terms</a>
                    </div>
                </div>
            </div>
        </div>
        <div class="fixed-footer-container-left"></div>
        <div class="fixed-footer-container-right"></div>
        <asp:UpdatePanel ID="updatepnl_GroupSessionLogoff" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_GroupSessionLogoff" runat="server" OnValueChanged="hf_GroupSessionLogoff_ValueChanged" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <script type="text/javascript">
            $(document).ready(function () {
                HashChange();
                $(window).resize();
            });
            $(document.body).on("click", "#pnl_CancelGroupLogin", function () {
                $("#hf_GroupSessionLogoff").val(new Date().toString());
                setTimeout(function () {
                    __doPostBack("hf_GroupSessionLogoff", "");
                }, 1);
            });
            $(window).resize(function () {
                $(".Modal-element-static").each(function () {
                    var $this = $(this);
                    if ($this.is(":visible")) {
                        $this.removeClass("small-screen");
                        $this.css({
                            marginTop: "",
                            marginLeft: ""
                        });

                        if ($(window).outerWidth() <= 750 || $this.outerHeight() >= $("#main_container").outerHeight() - 10) {
                            $this.addClass("small-screen");
                        }
                        else {
                            $this.css({
                                marginTop: -($this.outerHeight() / 2),
                                marginLeft: -($this.outerWidth() / 2)
                            });
                        }
                    }
                });
            });


            /* Url Change */
            $(function () {
                $(window).hashchange(function () {
                    HashChange();
                });
            });
            function HashChange() {
                var url = location.hash;
                var content = GetUrlParameterByName("iframecontent");
                if (!content) {
                    var modalWindow = GetUrlParameterByName("window");
                    if (modalWindow != null && modalWindow != "") {
                        if ($("#" + modalWindow).length == 0) {
                            window.location = "Login.aspx";
                        }

                        switch (modalWindow) {
                            default:
                            case "Login_element":
                                SetModalWindow("Login_element", openWSE_Config.siteName + " Login Portal");
                                break;

                            case "Register_element":
                                SetModalWindow("Register_element", "Create New Account");
                                break;

                            case "PasswordRecovery_element":
                                SetModalWindow("PasswordRecovery_element", "Password Reset");
                                break;
                        }
                    }
                    else {
                        SetModalWindow("Login_element", openWSE_Config.siteName + " Login Portal");
                    }
                }
            }
            function GetUrlParameterByName(e) {
                e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
                var t = "[\\?&]" + e + "=([^&#]*)";
                var n = new RegExp(t);
                var url = window.location.href.replace("#", "");
                var r = n.exec(url);
                if (r == null)
                    return "";
                else
                    return decodeURIComponent(r[1].replace(/\+/g, " "));
            }


            /* Modal Loader */
            var currentEle = "";
            function UpdateModalUrl(currWindow) {
                if (currWindow != null && currWindow != "") {
                    window.location = window.location.href.replace(window.location.hash, "") + "#?window=" + currWindow;
                }
                else {
                    window.location = window.location.href.replace(window.location.hash, "") + "#";
                }
            }
            function SetModalWindow(ele, name) {
                $(".Modal-element-static").hide();
                $("#" + ele).find(".ModalHeader-static").html(name);
                $("#" + ele).show();
                $(window).resize();

                setTimeout(function () {
                    $("#" + ele).find(".focus-textbox").focus();
                }, 100);
            }
        </script>
    </form>
</body>
</html>
