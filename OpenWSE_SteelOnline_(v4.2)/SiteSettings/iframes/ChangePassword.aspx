<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ChangePassword.aspx.cs" Inherits="SiteSettings_ChangePassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Change Password</title>
        <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    </head>
    <body class="body-admin" style="background: transparent !important; filter: none !important; min-width: 315px !important;">
        <form id="form1_changepassword" runat="server">
            <asp:ChangePassword ID="ChangeUserPassword" runat="server" CancelDestinationPageUrl="~/"
                                EnableViewState="false" RenderOuterTable="false" ContinueButtonStyle-CssClass="input-buttons" OnContinueButtonClick="OnContinueButtonClick">
                <ChangePasswordTemplate>
                    <div class="failureNotification clear-margin" style="width: 315px;">
                        <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                    </div>
                    <asp:ValidationSummary ID="ChangeUserPasswordValidationSummary" runat="server" CssClass="failureNotification"
                                           ValidationGroup="ChangeUserPasswordValidationGroup" />
                    <div class="accountInfo">
                        <asp:Label ID="CurrentPasswordLabel" runat="server" AssociatedControlID="CurrentPassword"
                                   CssClass="font-color-black font-bold">Old Password:</asp:Label>
                        <br />
                        <asp:TextBox ID="CurrentPassword" runat="server" CssClass="textEntry" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPassword"
                                                    CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Old Password is required."
                                                    ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="NewPasswordLabel" runat="server" AssociatedControlID="NewPassword"
                                   CssClass="font-color-black font-bold">New Password:</asp:Label>
                        <br />
                        <asp:TextBox ID="NewPassword" runat="server" CssClass="textEntry" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPassword"
                                                    CssClass="failureNotification" ErrorMessage="New Password is required." ToolTip="New Password is required."
                                                    ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="ConfirmNewPasswordLabel" runat="server" AssociatedControlID="ConfirmNewPassword"
                                   CssClass="font-color-black font-bold">Confirm New Password:</asp:Label>
                        <br />
                        <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="textEntry" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword"
                                                    CssClass="failureNotification" Display="Dynamic" ErrorMessage="Confirm New Password is required."
                                                    ToolTip="Confirm New Password is required." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                        <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword"
                                              ControlToValidate="ConfirmNewPassword" CssClass="failureNotification" Display="Dynamic"
                                              ErrorMessage="The Confirm New Password must match the New Password entry." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
                        <div class="clear-space">
                        </div>
                        <div class="clear-space-five">
                        </div>
                        <asp:Button ID="ChangePasswordPushButton_accountsettings" runat="server" CommandName="ChangePassword"
                                    Text="Change Password" ValidationGroup="ChangeUserPasswordValidationGroup" CssClass="input-buttons" />
                    </div>
                </ChangePasswordTemplate>
            </asp:ChangePassword>
        </form>
    </body>
</html>