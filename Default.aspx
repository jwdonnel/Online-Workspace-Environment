<%@ page language="C#" autoeventwireup="true" inherits="Default, App_Web_2ux1dlqd" %>

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
    <style type="text/css">
        .ModalButtonHolder
        {
            cursor: move;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <div id="always-visible">
            <div id="workspace-selector">
                <a id="lnk_BackToWorkspace" href="#">Close</a>
            </div>
            <h4 id="header_site_title" runat="server">Login Portal</h4>
        </div>
        <div id="container">
            <div id="Login_element" runat="server" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal">
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    <div class="logo-holder">
                                        <img id="groupLoginLogo" runat="server" alt="Logo" visible="false" />
                                        <div id="mainLoginLogo" runat="server" class="main-logo"></div>
                                    </div>
                                    <div class="clear"></div>
                                    <asp:Label ID="lbl_LoginMessage" runat="server" Text=""></asp:Label>
                                    <div class="clear"></div>
                                    <span id="account_active" style="text-align: center"></span>
                                    <asp:Literal ID="ltl_logingrouperror" runat="server"></asp:Literal>
                                    <table border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto;">
                                        <tr>
                                            <td valign="top">
                                                <div id="SocialLogin_borderSep">
                                                    <div class="loginwith-api-text">
                                                        <div><span class="inline-block">Log Into Your Account</span></div>
                                                        <hr class="soften" />
                                                    </div>
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
                                                                            <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username"></asp:TextBox>
                                                                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                                CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Username is required."
                                                                                ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                                            <div class="username-login-img"></div>
                                                                            <div class="clear-space"></div>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="signintextbox" placeholder="Password"></asp:TextBox>
                                                                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                                                CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Password is required."
                                                                                ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator>
                                                                            <div class="password-login-img"></div>
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
                                                            <div><span class="inline-block">Or Login With...</span></div>
                                                            <hr class="soften" />
                                                        </div>
                                                        <div class="clear-space"></div>
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
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <div id="pnl_purchaseFullVer" runat="server" class="panel-link" onclick="window.location.href='SiteTools/ServerMaintenance/LicenseManager.aspx?purchase=true';" visible="false" title="Purchase Full Version">
                                    <span class="btn-sprite purchase-lrg"></span>
                                </div>
                                <div id="pnl_GroupLogin" runat="server" class="panel-link" onclick="UpdateModalUrl('GroupSession_element');" title="Log into a Group">
                                    <span class="btn-sprite group-login"></span>
                                </div>
                                <div id="pnl_FooterRegister" runat="server" onclick="UpdateModalUrl('Register_element');" class="panel-link" title="Create New Account">
                                    <span class="btn-sprite add-user"></span>
                                </div>
                                <div id="pnl_FooterPasswordRec" runat="server" class="panel-link" onclick="UpdateModalUrl('PasswordRecovery_element');" title="Forgot your password?">
                                    <div class="btn-sprite password"></div>
                                </div>
                                <div id="pnl_preview" runat="server" onclick="OpeniframePage('Workspace.aspx?Demo=true&iframeName=Site Preview&iframeFullScreen=true');" class="panel-link" title="Preview Site">
                                    <div class="btn-sprite preview"></div>
                                </div>
                                <div id="pnl_CancelGroupLogin" runat="server" onclick="window.location.href='Default.aspx';" class="panel-link" visible="false" title="Cancel Group Login">
                                    <div class="btn-sprite cancel-lrg"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="GroupSession_element" runat="server" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="550">
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
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
                            <div class="ModalButtonHolder">
                                <div class="panel-link" onclick="UpdateModalUrl('');">
                                    <div class="btn-sprite back-lrg"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="Register_element" runat="server" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="550">
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
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
                                                                        <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                                            CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="User Name is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <asp:TextBox ID="Email" runat="server" CssClass="signintextbox" placeholder="email"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                                            CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="E-mail is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <asp:TextBox ID="Password" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Password"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                                            CssClass="failureNotification" ErrorMessage="" ForeColor="Red" ToolTip="Password is required."
                                                                            ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="signintextbox" TextMode="Password" placeholder="Confirm Password"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                                            Display="Dynamic" ErrorMessage="" ForeColor="Red" ID="ConfirmPasswordRequired" runat="server"
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
                                                                        <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="signintextbox" placeholder="First Name"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="signintextbox" placeholder="Last Name"></asp:TextBox>

                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <div class="img-colors float-left">
                                                                        </div>
                                                                        <asp:TextBox runat="server" ID="Color1" CssClass="signintextbox float-left color"
                                                                            MaxLength="6" AutoCompleteType="None" Width="65px" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup"
                                                                            CssClass="input-buttons-login" Text="Register"></asp:Button>
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
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <div class="panel-link" onclick="UpdateModalUrl('');">
                                    <div class="btn-sprite back-lrg"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="PasswordRecovery_element" runat="server" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="525">
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    Please provide your username. An email will be sent to this account using the email
                                    address associated with your account. Click the link in the email to recover your
                                    account. Always change your password after recovering your account.
                                    <div class="clear-space"></div>
                                    <asp:Panel ID="pnl_password_recovery" runat="server" DefaultButton="btn_passwordrecovery">
                                        <table cellpadding="5" cellspacing="5" style="margin: 0 auto;">
                                            <tr>
                                                <td style="text-align: left;">
                                                    <asp:UpdatePanel ID="updatepnl_passwordrecovery" runat="server">
                                                        <ContentTemplate>
                                                            <asp:TextBox ID="tb_username_recovery" runat="server" CssClass="signintextbox focus-textbox" placeholder="Username"></asp:TextBox>
                                                            <asp:RequiredFieldValidator ID="UserNameRequired_recovery" runat="server" ControlToValidate="tb_username_recovery"
                                                                ErrorMessage="" ValidationGroup="UsernameRecovery" ForeColor="Red">*</asp:RequiredFieldValidator>
                                                            <asp:Button ID="btn_passwordrecovery" runat="server" OnClick="btn_passwordrecovery_Click"
                                                                CssClass="input-buttons margin-left-sml" Text="Send Email" ValidationGroup="UsernameRecovery" />
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <div class="panel-link" onclick="UpdateModalUrl('');">
                                    <div class="btn-sprite back-lrg"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="container-footer" class="footer">
            <div class="footer-padding">
                <div id="copyright-footer" class="float-left">&copy; 2015 John Donnelly</div>
                <div id="footer-signdate" class="float-right">
                    <a href="AppRemote.aspx" title="Open Mobile Workspace">Mobile</a> | 
                    <a href="#" onclick="OpeniframePage('About.aspx?iframeName=About&iframeFullScreen=false');return false;">About</a>
                </div>
            </div>
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
        <script type="text/javascript" src="Scripts/jquery/combined-scripts.min.js"></script>
        <script type="text/javascript" src="WebControls/jscolor/jscolor.js"></script>
        <script type="text/javascript">
        var siteName = "";
        var animationSpeed = 150;
        var currentEle = "";
        var allowDrag = true;

        $(function () {
            $(window).hashchange(function () {
                HashChange();
            });
        });

        $(window).resize(function () {
            UpdateContainerHeight();

            if ($("#iframe-content-src").length > 0) {
                $("#iframe-content-src").css("height", $(window).height());
            }

            if (currentEle != null && currentEle != "") {
                var $thisElement = $(currentEle);
                $thisElement.find(".Modal-element-align").css({
                    marginTop: -($thisElement.find(".Modal-element-modal").height() / 2),
                    marginLeft: -($thisElement.find(".Modal-element-modal").width() / 2)
                });
            }
        });

        $(document).ready(function () {
            UpdateContainerHeight();
            HashChange();
        });

        function UpdateContainerHeight() {
            var ht = $(window).height() - ($("#always-visible").outerHeight() + $("#container-footer").outerHeight());
            $("#container").css("height", ht);
        }

        function colorChanged(sender) {
            sender.get_element().style.color = "#" + sender.get_selectedColor();
        }
        function SetTrialText(exp) {
            var text = "<div class='trial-version-text'><span>Trial Version</span><span class='float-right'>Expires in " + exp + "</span></div>";
            $("#container").prepend(text);
        }

        /* Modal Loader */
        function UpdateModalUrl(currWindow) {
            var fullUrl = "";
            var tempUrl = window.location.hash;

            if (currWindow != null && currWindow != "") {
                if ((tempUrl.indexOf("#") == -1) && (window.location.href.charAt(window.location.href.length - 1) != "#")) {
                    fullUrl = "#?";
                }
                else if (tempUrl != "") {
                    fullUrl = "&";
                }
                else {
                    fullUrl = "?";
                }
                fullUrl += "window=" + currWindow; // + "&contentName=" + $.trim($(_this).text());
                window.location += fullUrl;
            }
            else {
                window.location = window.location.href.replace(tempUrl, "") + "#";
            }
        }
        function SetModalWindow(ele, name) {
            $(".Modal-element").hide();
            $("#header_site_title").html(name);
            LoadModalWindow(true, ele);
            $("#" + ele).find(".focus-textbox").focus();
        }
        function LoadModalWindow(open, element) {
            if (element.indexOf("#") != 0) {
                element = "#" + element;
            }

            var $thisElement = $(element);
            if (open) {
                currentEle = element;
                $thisElement.show();

                var $modalElement = $thisElement.find(".Modal-element-modal");

                var container = "#container";
                $modalElement.find(".ModalScrollContent").css("max-height", $(container).height() - $(".ModalButtonHolder").outerHeight());

                if ($modalElement.outerWidth() > $(container).width()) {
                    $modalElement.css({
                        width: $(container).width(),
                        minWidth: 50
                    });
                }
                else if ($modalElement.attr("data-setwidth") != null && $modalElement.attr("data-setwidth") != "") {
                    if ($modalElement.attr("data-setmaxheight") != null && $modalElement.attr("data-setmaxheight") != "") {
                        $modalElement.find(".ModalScrollContent").css("max-height", parseInt($modalElement.attr("data-setmaxheight")));
                    }
                    $modalElement.css({
                        width: parseInt($modalElement.attr("data-setwidth")),
                        minWidth: parseInt($modalElement.attr("data-setwidth"))
                    });
                }
                else {
                    if ($(container).width() > 100) {
                        $modalElement.css("max-width", $(container).width() - 100);
                    }

                    if ($(container).height() > 100) {
                        $modalElement.find(".ModalScrollContent").css("max-height", $(container).height() - 100);
                    }
                }

                if (allowDrag) {
                    $modalElement.draggable({
                        containment: container,
                        cancel: ".ModalScrollContent, .ModalPadContent, .ModalExitButton, .panel-link",
                        drag: function (event, ui) {
                            var $this = $(this);
                            $this.css("opacity", "0.6");
                            $this.css("filter", "alpha(opacity=60)");

                            // Apply an overlay over app
                            // This fixes the issues when dragging iframes
                            if ($this.find("iframe").length > 0) {
                                var $_id = $this.find(".ModalScrollContent");
                                $wo = $_id.find(".app-overlay-fix");
                                if ($wo.length == 0) {
                                    if ($_id.length == 1) {
                                        $_id.append("<div class='app-overlay-fix'></div>");
                                    }
                                }
                            }
                        },
                        stop: function (event, ui) {
                            var $this = $(this);
                            $this.css("opacity", "1.0");
                            $this.css("filter", "alpha(opacity=100)");
                            $wo = $(this).find(".app-overlay-fix");
                            if ($wo.length == 1) {
                                $wo.remove();
                            }
                        }
                    });
                }

                FinishModalLoad(element);
            }
            else {
                currentEle = "";
                $thisElement.hide();
                $thisElement.css("visibility", "hidden");
            }
        }
        function FinishModalLoad(ele) {
            if (ele != null) {
                var $thisElement = $(ele);
                $thisElement.find(".Modal-element-align").css({
                    marginTop: -($thisElement.find(".Modal-element-modal").height() / 2),
                    marginLeft: -($thisElement.find(".Modal-element-modal").width() / 2)
                });

                $thisElement.css("visibility", "visible");
            }
        }

        function OpeniframePage(url) {
            CloseIFrameContent();
            var fullUrl = "";
            var tempUrl = window.location.hash;
            if ((tempUrl.indexOf("#") == -1) && (window.location.href.charAt(window.location.href.length - 1) != "#")) {
                fullUrl = "#?";
            }
            else if (tempUrl != "") {
                fullUrl = "&";
            }
            else {
                fullUrl = "?";
            }

            fullUrl += "iframecontent=" + url; // + "&contentName=" + $.trim($(_this).text());

            window.location += fullUrl;
        }
        function CloseIFrameContent() {
            $("#iframe-container-helper").remove();
            $("#container").show();

            try {
                var url = window.location.href;
                if (url.indexOf("?iframecontent=") != -1) {
                    var loc = url.split("?iframecontent=");
                    if (loc.length > 1) {
                        var fullLoc = "?iframecontent=" + loc[1];
                        url = url.replace(fullLoc, "");
                        window.location = url;
                    }
                }
                else if (url.indexOf("&iframecontent=") != -1) {
                    var loc = url.split("&iframecontent=");
                    if (loc.length > 1) {
                        var fullLoc = "&iframecontent=" + loc[1];
                        url = url.replace(fullLoc, "");
                        window.location = url;
                    }
                }
            }
            catch (evt) { }
        }
        function FinishIframeLoad(url, name, fullScreen) {
            var iframeHeight = $(window).height();
            var topPos = 0;
            var bottomPos = 0;

            if (fullScreen == "false") {
                iframeHeight -= ($("#always-visible").height() + $("#container-footer").height());
                topPos = $("#always-visible").height();
                bottomPos = $("#container-footer").height();
                $("#workspace-selector").show();
            }

            var iframe = "<iframe id='iframe-content-src' src='" + url + "' width='100%' frameborder='0' style='height: " + iframeHeight + "px;'></iframe>";
            var holder = "<div id='iframe-container-helper' style='top: " + topPos + "px; bottom: " + bottomPos + "px; z-index: 10002;'>" + iframe + "<div class='loading-background-holder'></div></div>";
            $("#container").hide();

            if (name != null && name != "") {
                name = "Close " + name;
            }
            else {
                name = "Close";
            }

            $("#workspace-selector").find("a").html(name);
            $("body").append(holder);
            $("#iframe-container-helper").fadeIn(animationSpeed);

            document.getElementById("iframe-content-src").onload = function () {
                $(document).ready(function () {
                    setTimeout(function () {
                        $("#iframe-container-helper").find(".loading-background-holder").remove();
                    }, animationSpeed);
                });
            };
        }

        function HashChange() {
            var url = location.hash;
            $("#workspace-selector").hide();

            var content = GetUrlParameterByName("iframecontent");
            if (content != null && content != "") {
                var name = GetUrlParameterByName("iframeName");
                var fullScreen = GetUrlParameterByName("iframeFullScreen");
                FinishIframeLoad(content, name, fullScreen);
            }
            else {
                var modalWindow = GetUrlParameterByName("window");
                if (modalWindow != null && modalWindow != "") {
                    if ($("#" + modalWindow).length == 0) {
                        window.location = "Default.aspx";
                    }

                    switch (modalWindow) {
                        default:
                        case "Login_element":
                            SetModalWindow("Login_element", siteName + " Login Portal");
                            break;

                        case "GroupSession_element":
                            GroupLoginModal();
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
                    SetModalWindow("Login_element", siteName + " Login Portal");
                }

                $("#iframe-container-helper").remove();
                $("#container").show();
            }

            $(window).resize();
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


        /* Group Login Modal */
        function GroupLoginModal() {
            LoadingMessage1("Loading Groups...");
            $.ajax({
                url: "WebServices/AcctSettings.asmx/GetUserGroups",
                type: "POST",
                data: '{ }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var x = "";
                    try {
                        for (var i = 0; i < data.d[0].length; i++) {
                            var groupId = data.d[0][i][0];
                            var groupName = data.d[0][i][1];
                            var image = data.d[0][i][2];
                            var owner = data.d[0][i][3];
                            var background = data.d[0][i][4];

                            var styleBackground = "style=\"background: url('" + background + "') 100% 50% repeat;\"";

                            x += "<div class='group-selection-entry' onclick='LoginAsGroup(\"" + groupId + "\")' title='Click to login' " + styleBackground + ">";
                            x += "<div class='overlay'></div>";
                            x += "<div class='group-selection-info'>";
                            x += "<div class='group-name-info'>";
                            x += "<span class='group-name'>" + groupName + "</span><div class='clear-space-five'></div>";
                            x += "<span class='group-info'><b class='pad-right-sml'>Owner:</b>" + owner + "</span>";
                            x += "</div>";
                            x += "<img class='group-img' alt='Group Logo' src='" + image + "' />";
                            x += "</div></div>";
                        }

                        if (ConvertBitToBoolean(data.d[1])) {
                            x += "<div class='logingroupselector-logout' onclick='LoginAsGroup(\"\")'>";
                            x += "<div>Log Out of Group</div>";
                            x += "</div>";
                        }

                        x += "<div class='clear-space'></div>";
                    }
                    catch (evt) { }

                    $("#group-list").html(x);
                    RemoveUpdateModal();
                    SetModalWindow("GroupSession_element", siteName + " Group Login");
                }
            });
        }
        function LoginAsGroup(id) {
            LoadingMessage1("Loading...");
            $.ajax({
                url: "WebServices/AcctSettings.asmx/LoginUnderGroup",
                type: "POST",
                data: '{ "id": "' + id + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d == "true") {
                        location.reload();
                    }
                    else {
                        window.location = "Default.aspx?group=" + id;
                    }
                },
                error: function (e) {
                    RemoveUpdateModal();
                }
            });
        }

        function ConvertBitToBoolean(value) {
            if (value != null && value != undefined) {
                var _value = $.trim(value.toString().toLowerCase());

                if (_value != "true" && _value != "false" && _value != "0" && _value != "1" && _value != "") {
                    return true;
                }

                if (_value == "1" || _value == "true" || _value == "") {
                    return true;
                }
                else if (_value == "0" || _value == "false") {
                    return false;
                }
            }

            return false;
        }


        /* Loading And Updating Modals */
        var intervalCount = 0;
        var messageLoadInterval;
        function LoadingMessage1(message) {
            RemoveUpdateModal();

            if (message.indexOf("...") != -1) {
                message = message.replace("...", "");
            }

            message = message + "<span class='progress inline-block'></span>";

            var x = "<div id='update-element'><div class='update-element-overlay'><div class='update-element-align'>";
            x += "<div class='update-element-modal'><h3 class='inline-block'>" + message + "</h3></div></div></div></div>";
            $("body").append(x);

            StartMessageTickInterval($("#update-element").find(".progress")[0]);
            $("#update-element").show();

            var $modalWindow = $("#update-element").find(".update-element-modal");
            var currUpdateWidth = -($modalWindow.outerWidth() / 2);
            var currUpdateHeight = -($modalWindow.outerHeight() / 2);
            $modalWindow.css({
                marginLeft: currUpdateWidth,
                marginTop: currUpdateHeight
            });
        }
        function StartMessageTickInterval(elem) {
            messageLoadInterval = setInterval(function () {
                var messageWithTrail = "";
                switch (intervalCount) {
                    case 0:
                        messageWithTrail = ".";
                        intervalCount++;
                        break;
                    case 1:
                        messageWithTrail = "..";
                        intervalCount++;
                        break;
                    case 2:
                        messageWithTrail = "...";
                        intervalCount++;
                        break;
                    default:
                        messageWithTrail = "";
                        intervalCount = 0;
                        break;
                }
                $(elem).html(messageWithTrail);
            }, 400);
        }
        function RemoveUpdateModal() {
            intervalCount = 0;
            if (messageLoadInterval != null) {
                clearInterval(messageLoadInterval);
            }
            $("#update-element").remove();
        }
        </script>
    </form>
</body>
</html>
