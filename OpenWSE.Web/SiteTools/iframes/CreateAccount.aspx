<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CreateAccount.aspx.cs" Inherits="SiteTools_CreateAccount" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Create Account</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <link href="../../App_Themes/Standard/StyleSheets/Main/main.css" rel="stylesheet" type="text/css" />
    <link href="../../App_Themes/Standard/StyleSheets/Main/sitemaster.css" rel="stylesheet" type="text/css" />
</head>
<body style="min-height: 255px!important; background: transparent!important;">
    <form id="form1" runat="server">
        <div class="pad-all">
            <div class="clear"></div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
            <asp:Label ID="lbl_email_assocation" runat="server"></asp:Label>
            <div class="clear">
            </div>
            <asp:CreateUserWizard ID="RegisterUser" runat="server" OnCreatedUser="RegisterUser_CreatedUser"
                OnCreatingUser="RegisterUser_CreatingUser" OnContinueButtonClick="RegisterUser_Continue"
                LoginCreatedUser="false" Width="100%" ContinueButtonStyle-CssClass="input-buttons"
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
                                            MaxLength="7" AutoCompleteType="None" TextMode="Color" />
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
        </div>
    </form>
</body>
</html>
