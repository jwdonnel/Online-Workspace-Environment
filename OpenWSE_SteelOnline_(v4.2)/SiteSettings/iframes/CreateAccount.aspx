<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CreateAccount.aspx.cs" Inherits="SiteSettings_CreateAccount" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Create Account</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
</head>
<body style="min-height: 255px!important;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <div class="pad-all">
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
                            <div style="text-align: center;">
                                <h4 style="padding-bottom: 20px">Passwords are required to be a minimum of <%= Membership.MinRequiredPasswordLength %> characters in length.
                                </h4>
                            </div>
                            <table class="tableLogin" style="margin: 0 auto;">
                                <tbody>
                                    <tr>
                                        <td>
                                            <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName" Font-Size="16px">User Name</asp:Label><br />
                                            <asp:TextBox ID="UserName" runat="server" CssClass="signintextbox"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                CssClass="failureNotification" ErrorMessage="" ToolTip="User Name is required."
                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pad-left-big">
                                            <div class="clear-space-two">
                                            </div>
                                            <span style="font-size: 16px">First Name</span>
                                            <br />
                                            <asp:TextBox ID="tb_firstnamereg" runat="server" CssClass="signintextbox" Width="200px"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <div class="clear-space-two">
                                            </div>
                                            <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email" Font-Size="16px">E-mail</asp:Label><br />
                                            <asp:TextBox ID="Email" runat="server" CssClass="signintextbox"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                CssClass="failureNotification" ErrorMessage="" ToolTip="E-mail is required."
                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pad-left-big">
                                            <div class="clear-space-two">
                                            </div>
                                            <span style="font-size: 16px">Last Name</span>
                                            <br />
                                            <asp:TextBox ID="tb_lastnamereg" runat="server" CssClass="signintextbox" Width="200px"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <div class="clear-space-two">
                                            </div>
                                            <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password" Font-Size="16px">Password</asp:Label><br />
                                            <asp:TextBox ID="Password" runat="server" CssClass="signintextbox" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                CssClass="failureNotification" ErrorMessage="" ToolTip="Password is required."
                                                ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pad-left-big">
                                            <div class="clear-space-two">
                                            </div>
                                            <span style="font-size: 16px">User Color</span>
                                            <br />
                                            <span class="img-colors pad-all-sml float-left margin-right-sml"></span>
                                            <asp:TextBox runat="server" ID="Color1" CssClass="signintextbox float-left color margin-right"
                                                MaxLength="6" AutoCompleteType="None" Width="65px" />
                                            <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" ValidationGroup="RegisterUserValidationGroup"
                                                CssClass="input-buttons float-left" Text="Register"></asp:Button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <div class="clear-space-two">
                                            </div>
                                            <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword"
                                                Font-Size="16px">Confirm Password</asp:Label><br />
                                            <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="signintextbox" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                                Display="Dynamic" ErrorMessage="" ID="ConfirmPasswordRequired" runat="server"
                                                ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                                ErrorMessage="" ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                        </td>
                                        <td class="pad-left-big">
                                            <div class="clear-space-two">
                                            </div>
                                            <span class="failureNotification">
                                                <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                                                <span id="failureMessage"></span></span>
                                            <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
                                                ValidationGroup="RegisterUserValidationGroup" />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </ContentTemplate>
                        <CustomNavigationTemplate>
                        </CustomNavigationTemplate>
                    </asp:CreateUserWizardStep>
                </WizardSteps>
            </asp:CreateUserWizard>
        </div>
        <script type="text/javascript" src="../../WebControls/jscolor/jscolor.js"></script>
    </form>
</body>
</html>
