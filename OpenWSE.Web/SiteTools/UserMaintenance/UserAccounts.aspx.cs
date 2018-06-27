#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Image = System.Web.UI.WebControls.Image;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.Overlays;

#endregion

public partial class SiteTools_UserAccounts : BasePage {

    #region private variables

    private App CurrentAppObject = new App(string.Empty);
    private readonly AppCategory _appCategory = new AppCategory(false);

    #endregion


    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsUserInAdminRole()) {
            pnl_admin_tools.Enabled = false;
            pnl_admin_tools.Visible = false;
            btn_CreateNewUsers.Visible = false;
        }

        LoadAppsAndUsers();
        BuildRolesDropDownList(IsPostBack);

        if (!IsPostBack || (!PostbackControlContainsString("hf_lockUser") && !PostbackControlContainsString("hf_deleteUser")
            && !PostbackControlContainsString("hf_refreshList") && !PostbackControlContainsString("hf_resetPassword"))) {
            BuildUsers();
        }

        if (!IsPostBack) {
            TextBox color1Tb = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("color1");
            if (color1Tb != null) {
                color1Tb.Text = "#" + UserRegistration.RandomColor().Replace("#", string.Empty);
            }
        }

        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);

        LoadSettings();
        GetPostBack();
    }
    private void GetPostBack() {
        switch (AsyncPostBackSourceElementID) {
            case "MainContent_btn_roleDelete":
                DeleteRole_Click();
                break;
        }
    }
    private void LoadSettings() {
        pnl_usersettings.Visible = true;
        if (!IsUserInAdminRole()) {
            pnl_usersettings.Visible = false;
        }

        rb_usersignup_holder.Visible = true;
        if (IsUserInAdminRole() && MainServerSettings.CustomizationsLocked) {
            rb_usersignup_holder.Visible = false;
        }

        pnl_ConfirmationEmailSignUp.Visible = false;
        if (MainServerSettings.EmailSystemStatus && MainServerSettings.AllowUserSignUp) {
            pnl_ConfirmationEmailSignUp.Visible = true;
        }

        if (IsUserInAdminRole()) {
            pnl_socialSignIn_UserSignup_tab.Visible = true;
            pnl_socialSignIn_UserSignup.Visible = true;
        }

        UpdateUserSignUpRoleDropdown();

        if (MainServerSettings.UserSignUpEmailChecker) {
            pnl_emailassociation.Enabled = true;
            pnl_emailassociation.Visible = true;
            rb_emailassociation_on.Checked = true;
            rb_emailassociation_off.Checked = false;
        }
        else {
            pnl_emailassociation.Enabled = false;
            pnl_emailassociation.Visible = false;
            rb_emailassociation_on.Checked = false;
            rb_emailassociation_off.Checked = true;
        }
        tb_EmailAssociation.Text = MainServerSettings.UserSignUpEmailAssociation;

        if (MainServerSettings.AllowUserSignUp) {
            pnl_UserSignUp.Enabled = true;
            pnl_UserSignUp.Visible = true;
            rb_allowusersignup_on.Checked = true;
            rb_allowusersignup_off.Checked = false;
            if (MainServerSettings.EmailSystemStatus) {
                pnl_ConfirmationEmailSignUp.Enabled = true;
                pnl_ConfirmationEmailSignUp.Visible = true;
            }
        }
        else {
            pnl_UserSignUp.Enabled = false;
            pnl_UserSignUp.Visible = false;
            rb_allowusersignup_on.Checked = false;
            rb_allowusersignup_off.Checked = true;
            pnl_ConfirmationEmailSignUp.Enabled = false;
            pnl_ConfirmationEmailSignUp.Visible = false;
        }

        if (MainServerSettings.SignInWithGoogle) {
            rb_googlePlusSignIn_on.Checked = true;
            rb_googlePlusSignIn_off.Checked = false;
            pnl_googleSettings.Visible = true;
            pnl_googleSettings.Enabled = true;
        }
        else {
            rb_googlePlusSignIn_on.Checked = false;
            rb_googlePlusSignIn_off.Checked = true;
            pnl_googleSettings.Visible = false;
            pnl_googleSettings.Enabled = false;
        }

        if (MainServerSettings.SignInWithTwitter) {
            rb_twitterSignIn_on.Checked = true;
            rb_twitterSignIn_off.Checked = false;
            pnl_twitterSettings.Visible = true;
            pnl_twitterSettings.Enabled = true;
        }
        else {
            rb_twitterSignIn_on.Checked = false;
            rb_twitterSignIn_off.Checked = true;
            pnl_twitterSettings.Visible = false;
            pnl_twitterSettings.Enabled = false;
        }

        if (MainServerSettings.SignInWithFacebook) {
            rb_facebookSignIn_on.Checked = true;
            rb_facebookSignIn_off.Checked = false;
            pnl_facebookSettings.Visible = true;
            pnl_facebookSettings.Enabled = true;
        }
        else {
            rb_facebookSignIn_on.Checked = false;
            rb_facebookSignIn_off.Checked = true;
            pnl_facebookSettings.Visible = false;
            pnl_facebookSettings.Enabled = false;
        }

        if (MainServerSettings.SignUpConfirmationEmail) {
            rb_SignUpConfirmationEmail_on.Checked = true;
            rb_SignUpConfirmationEmail_off.Checked = false;
        }
        else {
            rb_SignUpConfirmationEmail_on.Checked = false;
            rb_SignUpConfirmationEmail_off.Checked = true;
        }

        if (MainServerSettings.ForceGroupLogin) {
            rb_ForceGroupLogin_on.Checked = true;
            rb_ForceGroupLogin_off.Checked = false;
            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            pnl_nologinrequired.Enabled = false;
            pnl_nologinrequired.Visible = false;
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
        else {
            rb_ForceGroupLogin_on.Checked = false;
            rb_ForceGroupLogin_off.Checked = true;
        }

        bool noLoginrequired = MainServerSettings.NoLoginRequired;
        bool showPreviewBtn = MainServerSettings.ShowPreviewButtonLogin;
        if (noLoginrequired) {
            rb_nologinrequired_on.Checked = true;
            rb_nologinrequired_off.Checked = false;
            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            pnl_showloginmodalondemomode.Enabled = true;
            pnl_showloginmodalondemomode.Visible = true;
        }
        else {
            rb_nologinrequired_on.Checked = false;
            rb_nologinrequired_off.Checked = true;
            pnl_showpreviewbutton.Enabled = true;
            pnl_showpreviewbutton.Visible = true;
            pnl_showloginmodalondemomode.Enabled = false;
            pnl_showloginmodalondemomode.Visible = false;
        }

        if (MainServerSettings.ShowLoginModalOnDemoMode) {
            rb_ShowLoginModalOnDemoMode_on.Checked = true;
            rb_ShowLoginModalOnDemoMode_off.Checked = false;
        }
        else {
            rb_ShowLoginModalOnDemoMode_on.Checked = false;
            rb_ShowLoginModalOnDemoMode_off.Checked = true;
        }

        if ((!noLoginrequired) && (!showPreviewBtn)) {
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
            pnl_showpreviewbutton.Enabled = true;
            pnl_showpreviewbutton.Visible = true;
        }
        else if ((noLoginrequired) && (showPreviewBtn)) {
            if (!MainServerSettings.ForceGroupLogin) {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
            }
            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
        }
        else if (!MainServerSettings.ForceGroupLogin) {
            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;
        }

        if (showPreviewBtn) {
            rb_ShowPreviewButtonLogin_on.Checked = true;
            rb_ShowPreviewButtonLogin_off.Checked = false;
        }
        else {
            rb_ShowPreviewButtonLogin_on.Checked = false;
            rb_ShowPreviewButtonLogin_off.Checked = true;
        }

        string controlName = string.Empty;
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null && !string.IsNullOrEmpty(sm.AsyncPostBackSourceElementID)) {
            controlName = sm.AsyncPostBackSourceElementID;
        }

        if (!IsPostBack) {
            LoadTwitterSettings();
            LoadGoogleSettings();
            LoadFacebookSettings();
        }

        if (string.IsNullOrEmpty(controlName) || !controlName.Contains("btn_loginPageMessage")) {
            tb_loginPageMessage.Text = MainServerSettings.LoginMessage;
            lbl_loginMessageDate.Text = "<b class='pad-right-sml'>Date Updated:</b>" + MainServerSettings.LoginMessageDate;
        }
    }

    #endregion


    #region User Properties

    private void BuildUsers() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 3, "UserList_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Image", "55px", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Username", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Full Name", "200px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Email", "200px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Joined", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Last Login", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Role", "135px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            if (u != null) {
                if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    MemberDatabase member = new MemberDatabase(u.UserName);
                    string userMe = string.Empty;
                    if (u.UserName.ToLower() == CurrentUsername.ToLower()) {
                        userMe = " - <small><i>You</i></small>";
                    }

                    string userRole = string.Empty;
                    string[] userRoles = Roles.GetRolesForUser(u.UserName);
                    foreach (string role in userRoles) {
                        userRole += role + ",";
                    }

                    if (userRole.Length > 0 && userRole[userRole.Length - 1] == ',') {
                        userRole = userRole.Remove(userRole.Length - 1);
                    }


                    List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues(string.Empty, UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40, member.SiteTheme), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Username", u.UserName + userMe, TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Full Name", HelperMethods.MergeFMLNames(member), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Email", u.Email, TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date Joined", u.CreationDate.ToShortDateString(), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Login", u.LastLoginDate.ToString(), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Role", userRole, TableBuilderColumnAlignment.Left));
                    tableBuilder.AddBodyRow(bodyColumnValues, GetUserButtons(member, u));
                }
            }
        }
        #endregion

        pnl_Users.Controls.Clear();
        pnl_Users.Controls.Add(tableBuilder.CompleteTableLiteralControl("No users found"));
    }
    private string GetUserButtons(MemberDatabase member, MembershipUser u) {
        var strUsers = new StringBuilder();
        string unlockUser = "<a href='#' title='Lock User' class='td-unlock-btn RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
        string lockUser = "<a href='#' title='Unlock User' class='td-lock-btn RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
        string deleteUser = "<a href='#' title='Delete User' class='td-delete-btn' onclick='DeleteUser(\"" + u.UserName + "\");return false;'></a>";
        string editUser = "<a href='#' title='Edit User' class='td-edit-btn' onclick='openWSE.LoadIFrameContent(\"SiteTools/UserTools/AcctSettings.aspx?u=" + u.UserName + "&iframeMode=true\");return false;'></a>";
        string password = string.Empty;
        if (!member.IsSocialAccount) {
            password = "<a href='#' title='Reset User Password' class='td-password-btn' onclick='ResetPassword(\"" + u.UserName + "\");return false;'></a>";
        }

        string mail = "<a href='#' title='Email me on login' class='td-mail-btn' onclick='EmailUser(\"" + u.UserName + "\");return false;'></a>";
        string nomail = "<a href='#' title='Dont email me on login' class='td-nomail-btn' onclick='CancelEmailUser(\"" + u.UserName + "\");return false;'></a>";

        if (u.UserName.ToLower() == CurrentUsername.ToLower()) {
            strUsers.Append(editUser);
            return strUsers.ToString();
        }

        if (!MainServerSettings.EmailSystemStatus) {
            mail = string.Empty;
            nomail = string.Empty;
        }

        if (u.UserName.ToLower() != CurrentUsername.ToLower()) {
            if (member.EmailUponLoginList.Any(un => un.ToLower() == CurrentUsername.ToLower()))
                strUsers.Append(nomail);
            else
                strUsers.Append(mail);
        }

        if (IsUserNameEqualToAdmin()) {
            strUsers.Append(password);
            if ((u.IsLockedOut) || (!u.IsApproved))
                strUsers.Append(lockUser);
            else
                strUsers.Append(unlockUser);

            strUsers.Append(editUser);
            strUsers.Append(deleteUser);
        }
        else {
            if (u.UserName.ToLower() == CurrentUsername.ToLower()) {
                strUsers.Append(password);
                strUsers.Append(editUser);
            }
            else {
                if (!IsUserInAdminRole()) {
                    if (!BasePage.IsUserInAdminRole(u.UserName)) {
                        strUsers.Append(password);
                        strUsers.Append(editUser);
                    }
                }
                else {
                    strUsers.Append(password);
                    if (CurrentUsername.ToLower() != u.UserName.ToLower() && !BasePage.IsUserInAdminRole(u.UserName)) {
                        if ((u.IsLockedOut) || (!u.IsApproved))
                            strUsers.Append(lockUser);
                        else
                            strUsers.Append(unlockUser);
                    }

                    if (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) {
                        strUsers.Append(editUser);
                    }

                    if (CurrentUsername.ToLower() != u.UserName.ToLower() && !BasePage.IsUserInAdminRole(u.UserName))
                        strUsers.Append(deleteUser);
                }
            }
        }

        return strUsers.ToString();
    }
    protected void ChangePasswordPushButton_accountsettings_Clicked(object sender, EventArgs e) {
        string username = ChangeUserPassword.UserName;
        if (!string.IsNullOrEmpty(username)) {
            MembershipUser msu = Membership.GetUser(username);
            if (msu != null) {
                string oldPassword = msu.ResetPassword();
                string newPassword = ChangeUserPassword.NewPassword;
                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= Membership.MinRequiredPasswordLength && msu.ChangePassword(oldPassword, newPassword)) {
                    lbl_passwordReset.Text = "";
                    txt_PasswordFinishedText.Text = "<h3>Password has been changed for <b>" + username + "</b></h3><div class='clear' style='height: 45px'></div>";
                    ChangeUserPassword.Enabled = false;
                    ChangeUserPassword.Visible = false;
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, '" + pwreset_overlay.ClientID + "', 'Password Reset Complete');");
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is not valid. Please type in different password. Passowrds must be at least " + Membership.MinRequiredPasswordLength.ToString() + " characters long.');");
                }
            }
        }
    }

    #endregion


    #region Add User

    protected void hf_createMultiUsers_ValueChanged(object sender, EventArgs e) {
        string email = txt_multiUser_email.Text.Trim();
        string password = txt_multiUser_password.Text;
        string role = dd_role_multiUser.SelectedValue;

        List<string> roleList = MemberDatabase.GetListOfAvailableRoles();
        List<string> defaultList = NewUserDefaults.GetListOfNewUserDefaults();

        string[] userList = hf_createMultiUsers.Value.Trim().Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string user in userList) {
            string tempUser = user.Replace(",", string.Empty);
            tempUser = tempUser.Trim();
            if ((!roleList.Contains(tempUser, StringComparer.OrdinalIgnoreCase)) && (!defaultList.Contains(tempUser, StringComparer.OrdinalIgnoreCase))) {
                MembershipUser existingUser = Membership.GetUser(tempUser);
                if (existingUser == null) {
                    Membership.CreateUser(tempUser, password, email);
                    UserRegistration ur = new UserRegistration(tempUser, role);

                    string firstName = tempUser;
                    string lastName = "";

                    string[] userNames = tempUser.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (userNames.Length > 2) {
                        firstName = userNames[0];
                        lastName = tempUser.Replace(firstName, string.Empty).Trim();
                    }

                    string color = UserRegistration.RandomColor();
                    ur.RegisterNewUser(firstName, lastName, email, color);
                    ur.RegisterDefaults();
                }
            }
        }

        ServerSettings.RefreshPage(Page, string.Empty);
    }

    protected void btn_rebuild_uc_Clicked(object sender, EventArgs e) {
        CurrentUserMemberDatabase.ClearUserTable();

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            MemberDatabase m = new MemberDatabase(u.UserName);
            if (!m.UserCustomizationsEnabled) {
                m.AddUserCustomizationRow(u.UserName);
            }
        }

        BuildUsers();
    }

    protected void RegisterUser_CreatingUser(object sender, LoginCancelEventArgs e) {
        List<string> roleList = MemberDatabase.GetListOfAvailableRoles();
        List<string> defaultList = NewUserDefaults.GetListOfNewUserDefaults();

        string tempUserName = RegisterUser.UserName.Replace(",", string.Empty).Trim();

        if ((roleList.Contains(tempUserName, StringComparer.OrdinalIgnoreCase))
            || (defaultList.Contains(tempUserName, StringComparer.OrdinalIgnoreCase))) {
            e.Cancel = true;
            hf_creatingNewUser.Value = "false";
        }
        else {
            if (!IsUserInAdminRole()) {
                e.Cancel = true;
                hf_creatingNewUser.Value = "false";
            }
            else {
                e.Cancel = false;
                hf_creatingNewUser.Value = "true";
            }
        }
    }
    protected void RegisterUser_CreatedUser(object sender, EventArgs e) {
        string username = RegisterUser.UserName.Replace(",", string.Empty).Trim();
        string email = RegisterUser.Email;
        var tb_firstnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_firstnamereg");
        var tb_lastnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_lastnamereg");
        var color1 = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("color1");
        var ddRole = (DropDownList)RegisterUserWizardStep.ContentTemplateContainer.FindControl("dd_role");

        string role = ddRole.SelectedValue;

        UserRegistration ur = new UserRegistration(username, role);
        ur.RegisterNewUser(tb_firstnamereg.Text, tb_lastnamereg.Text, email, color1.Text.Replace("#", string.Empty));
        ur.RegisterDefaults();

        ServerSettings.PageIFrameRedirect(this.Page, "UserAccounts.aspx");
    }

    #endregion


    #region Edit Buttons

    protected void hf_lockUser_Changed(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            string user = hf_lockUser.Value.Trim();
            if (!string.IsNullOrEmpty(user)) {
                MembershipUser _userLock = Membership.GetUser(user);
                if (_userLock != null) {
                    if ((_userLock.IsLockedOut) || (!_userLock.IsApproved)) {
                        _userLock.UnlockUser();
                        _userLock.IsApproved = true;
                    }
                    else
                        _userLock.IsApproved = false;

                    Membership.UpdateUser(_userLock);
                }
            }
        }
        hf_lockUser.Value = string.Empty;
        BuildUsers();
    }
    protected void hf_deleteUser_Changed(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            string user = hf_deleteUser.Value.Trim();
            if (user != CurrentUsername && !BasePage.IsUserNameEqualToAdmin(user)) {
                Membership.DeleteUser(user, true);
                MemberDatabase m = new MemberDatabase(user);
                m.DeleteUserCustomizations(user);
            }
        }
        hf_deleteUser.Value = string.Empty;

        BuildUsers();
    }
    protected void hf_refreshList_Changed(object sender, EventArgs e) {
        BuildUsers();
        hf_refreshList.Value = string.Empty;
    }
    protected void hf_resetPassword_Changed(object sender, EventArgs e) {
        string user = hf_resetPassword.Value.Trim();
        try {
            string str = "<h3>Set new password for <b>" + user + "</b></h3>";
            lbl_passwordReset.Text = str;
            txt_PasswordFinishedText.Text = string.Empty;
            ChangeUserPassword.Enabled = true;
            ChangeUserPassword.Visible = true;
            ChangeUserPassword.UserName = user;
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, '" + pwreset_overlay.ClientID + "', 'Password Reset');");
            TextBox tb_newPassword = (TextBox)ChangeUserPassword.ChangePasswordTemplateContainer.FindControl("NewPassword");
            if (tb_newPassword != null)
                tb_newPassword.Focus();
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }

        BuildUsers();
        hf_resetPassword.Value = string.Empty;
    }
    protected void hf_emailUser_Changed(object sender, EventArgs e) {
        var msu = new MemberDatabase(hf_emailUser.Value);
        msu.UpdateEmailUponLogin(CurrentUsername, false);
        BuildUsers();
        hf_emailUser.Value = string.Empty;
    }
    protected void hf_noemailUser_Changed(object sender, EventArgs e) {
        var msu = new MemberDatabase(hf_noemailUser.Value);
        msu.UpdateEmailUponLogin(CurrentUsername, true);
        BuildUsers();
        hf_noemailUser.Value = string.Empty;
    }

    private void BuildRolesDropDownList(bool postBack) {
        if (!postBack) {
            List<string> roleList = MemberDatabase.GetListOfAvailableRoles();
            DropDownList dd_role = (DropDownList)RegisterUserWizardStep.ContentTemplateContainer.FindControl("dd_role");
            if (dd_role != null) {
                dd_role.Items.Clear();
                foreach (string role in roleList) {
                    dd_role.Items.Add(new ListItem(role, role));
                }

                if (dd_role.Items.Count > 0) {
                    dd_role.SelectedIndex = 0;
                }
            }

            dd_role_multiUser.Items.Clear();
            foreach (string role in roleList) {
                dd_role_multiUser.Items.Add(new ListItem(role, role));
            }

            if (dd_role_multiUser.Items.Count > 0) {
                dd_role_multiUser.SelectedIndex = 0;
            }
        }
    }

    #endregion


    #region Manage Roles

    private void BuildManagedRolesDDL() {
        List<string> roleList = MemberDatabase.GetListOfAvailableRoles();
        ddl_roleNameSelect.Items.Clear();
        foreach (string role in roleList) {
            if (role.ToLower() != "standard" && role.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                ddl_roleNameSelect.Items.Add(new ListItem(role, role));
            }
        }
    }
    private void ShowHideManagedRoleButtons(bool editMode) {
        ddl_roleNameSelect.Enabled = !editMode;
        ddl_roleNameSelect.Visible = !editMode;
        tb_roleNameEdit.Enabled = editMode;
        tb_roleNameEdit.Visible = editMode;
        btn_roleEdit.Enabled = !editMode;
        btn_roleEdit.Visible = !editMode;
        btn_roleDelete.Enabled = !editMode;
        btn_roleDelete.Visible = !editMode;
        btn_roleEditUpdate.Enabled = editMode;
        btn_roleEditUpdate.Visible = editMode;
        btn_roleEditCancel.Enabled = editMode;
        btn_roleEditCancel.Visible = editMode;

        if (ddl_roleNameSelect.Items.Count == 0) {
            btn_roleEdit.Enabled = false;
            btn_roleEdit.Visible = false;
            btn_roleDelete.Enabled = false;
            btn_roleDelete.Visible = false;
            btn_roleEditUpdate.Enabled = false;
            btn_roleEditUpdate.Visible = false;
            btn_roleEditCancel.Enabled = false;
            btn_roleEditCancel.Visible = false;
        }
    }

    protected void btn_manageRoles_Click(object sender, EventArgs e) {
        BuildManagedRolesDDL();

        pnl_createCustomRole.Enabled = false;
        pnl_createCustomRole.Visible = false;
        UpdatePanel21.Update();

        pnl_manageRoles.Enabled = true;
        pnl_manageRoles.Visible = true;

        ShowHideManagedRoleButtons(false);
        UpdatePanel22.Update();

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'ManageRoles-element', 'Manage Custom Roles');");
    }
    protected void btn_roleEdit_Click(object sender, EventArgs e) {
        ShowHideManagedRoleButtons(true);
        tb_roleNameEdit.Text = ddl_roleNameSelect.SelectedValue;

        tb_roleNameEdit.Focus();

        lbtn_CreateNewRole.Enabled = false;
        lbtn_CreateNewRole.Visible = false;

        UpdatePanel22.Update();
    }
    private void DeleteRole_Click() {
        // Delete the role
        NewUserDefaults userDefaults = new NewUserDefaults(ddl_roleNameSelect.SelectedValue);
        userDefaults.DeleteDefault();

        string[] usersInRole = Roles.GetUsersInRole(ddl_roleNameSelect.SelectedValue);
        if (usersInRole.Length > 0) {
            Roles.RemoveUsersFromRole(usersInRole, ddl_roleNameSelect.SelectedValue);
            Roles.AddUsersToRole(usersInRole, "Standard");
        }

        Roles.DeleteRole(ddl_roleNameSelect.SelectedValue);

        BuildRolesDropDownList(false);

        BuildManagedRolesDDL();
        ShowHideManagedRoleButtons(false);
        UpdatePanel22.Update();

        BuildUsers();
        LoadSettings();
    }
    protected void btn_roleEditUpdate_Click(object sender, EventArgs e) {
        if (tb_roleNameEdit.Enabled && tb_roleNameEdit.Visible) {
            lbl_roleEditError.Text = string.Empty;
            string newRole = tb_roleNameEdit.Text.Trim();

            if (newRole.ToLower() == "demonologin") {
                lbl_roleEditError.Text = "Role already exists. Please enter a new role name.";
                UpdatePanel22.Update();
                return;
            }
            else if (string.IsNullOrEmpty(newRole)) {
                lbl_roleEditError.Text = "Role cannot be blank. Please enter a new role name.";
                UpdatePanel22.Update();
                return;
            }

            List<string> listOfRoles = MemberDatabase.GetListOfAvailableRoles();
            foreach (string role in listOfRoles) {
                if (role.ToLower() == newRole.ToLower()) {
                    lbl_roleEditError.Text = "Role already exists. Please enter a new role name.";
                    UpdatePanel22.Update();
                    return;
                }
            }

            // Update role name
            string[] usersInRole = Roles.GetUsersInRole(ddl_roleNameSelect.SelectedValue);
            if (MemberDatabase.CreateRole(newRole)) {
                NewUserDefaults userDefaults = new NewUserDefaults(ddl_roleNameSelect.SelectedValue);
                userDefaults.UpdateDefaults("Role", newRole);

                if (usersInRole.Length > 0) {
                    Roles.RemoveUsersFromRole(usersInRole, ddl_roleNameSelect.SelectedValue);
                    Roles.AddUsersToRole(usersInRole, newRole);
                }
                Roles.DeleteRole(ddl_roleNameSelect.SelectedValue);
            }

            BuildRolesDropDownList(false);

            BuildManagedRolesDDL();
            ShowHideManagedRoleButtons(false);

            lbtn_CreateNewRole.Enabled = true;
            lbtn_CreateNewRole.Visible = true;

            UpdatePanel22.Update();

            UpdateUserSignUpRoleDropdown();
        }
    }
    protected void btn_roleEditCancel_Click(object sender, EventArgs e) {
        lbl_roleEditError.Text = string.Empty;
        BuildManagedRolesDDL();
        ShowHideManagedRoleButtons(false);

        lbtn_CreateNewRole.Enabled = true;
        lbtn_CreateNewRole.Visible = true;

        UpdatePanel22.Update();
    }
    protected void lbtn_CreateNewRole_Click(object sender, EventArgs e) {
        tb_NewRoleName.Text = string.Empty;
        lbl_NewRoleNameError.Text = string.Empty;

        tb_NewRoleName.Focus();

        pnl_createCustomRole.Enabled = true;
        pnl_createCustomRole.Visible = true;
        UpdatePanel21.Update();

        pnl_manageRoles.Enabled = false;
        pnl_manageRoles.Visible = false;
        UpdatePanel22.Update();

        UpdateUserSignUpRoleDropdown();
    }
    protected void btn_NewRoleName_Click(object sender, EventArgs e) {
        lbl_NewRoleNameError.Text = string.Empty;
        string newRole = tb_NewRoleName.Text.Trim();

        if (newRole.ToLower() == "demonologin") {
            lbl_NewRoleNameError.Text = "Role already exists. Please enter a new role name.";
            UpdatePanel21.Update();
            return;
        }
        else if (string.IsNullOrEmpty(newRole)) {
            lbl_NewRoleNameError.Text = "Role cannot be blank. Please enter a new role name.";
            UpdatePanel21.Update();
            return;
        }

        List<string> listOfRoles = MemberDatabase.GetListOfAvailableRoles();
        foreach (string role in listOfRoles) {
            if (role.ToLower() == newRole.ToLower()) {
                lbl_NewRoleNameError.Text = "Role already exists. Please enter a new role name.";
                UpdatePanel21.Update();
                return;
            }
        }

        if (MemberDatabase.CreateRole(newRole)) {
            NewUserDefaults newDefaults = new NewUserDefaults(newRole);
            newDefaults.GetDefaults();
        }

        tb_NewRoleName.Text = string.Empty;
        lbl_NewRoleNameError.Text = string.Empty;

        pnl_createCustomRole.Enabled = false;
        pnl_createCustomRole.Visible = false;

        BuildRolesDropDownList(false);

        UpdatePanel21.Update();

        pnl_manageRoles.Enabled = true;
        pnl_manageRoles.Visible = true;

        BuildManagedRolesDDL();
        ShowHideManagedRoleButtons(false);
        UpdatePanel22.Update();

        UpdateUserSignUpRoleDropdown();
    }
    protected void btn_CancelNewRole_Click(object sender, EventArgs e) {
        tb_NewRoleName.Text = string.Empty;
        lbl_NewRoleNameError.Text = string.Empty;

        pnl_createCustomRole.Enabled = false;
        pnl_createCustomRole.Visible = false;
        UpdatePanel21.Update();

        pnl_manageRoles.Enabled = true;
        pnl_manageRoles.Visible = true;
        UpdatePanel22.Update();
    }

    private void UpdateUserSignUpRoleDropdown() {
        List<string> listOfRoles = MemberDatabase.GetListOfAvailableRoles();
        dd_usersignuprole.Items.Clear();
        foreach (string roleName in listOfRoles) {
            dd_usersignuprole.Items.Add(new ListItem(roleName, roleName));
        }

        string role = MainServerSettings.UserSignUpRole;
        int index = 0;
        foreach (ListItem item in dd_usersignuprole.Items) {
            if (item.Value == role) {
                dd_usersignuprole.SelectedIndex = index;
                break;
            }
            index++;
        }
    }

    #endregion


    #region Settings

    protected void dd_usersignuprole_Changed(object sender, EventArgs e) {
        ServerSettings.update_UserSignUpRole(dd_usersignuprole.SelectedValue);
    }

    protected void rb_allowusersignup_on_Checked(object sender, EventArgs e) {
        pnl_UserSignUp.Enabled = true;
        pnl_UserSignUp.Visible = true;
        rb_allowusersignup_on.Checked = true;
        rb_allowusersignup_off.Checked = false;
        if (MainServerSettings.EmailSystemStatus) {
            pnl_ConfirmationEmailSignUp.Enabled = true;
            pnl_ConfirmationEmailSignUp.Visible = true;
        }
        ServerSettings.update_AllowUserSignUp(true);
    }
    protected void rb_allowusersignup_off_Checked(object sender, EventArgs e) {
        pnl_UserSignUp.Enabled = false;
        pnl_UserSignUp.Visible = false;
        rb_allowusersignup_off.Checked = true;
        rb_allowusersignup_on.Checked = false;
        pnl_ConfirmationEmailSignUp.Enabled = false;
        pnl_ConfirmationEmailSignUp.Visible = false;
        ServerSettings.update_AllowUserSignUp(false);
    }

    protected void rb_googlePlusSignIn_on_Checked(object sender, EventArgs e) {
        rb_googlePlusSignIn_on.Checked = true;
        rb_googlePlusSignIn_off.Checked = false;
        pnl_googleSettings.Visible = true;
        pnl_googleSettings.Enabled = true;
        LoadGoogleSettings();
        ServerSettings.update_SignInWithGoogle(true);
    }
    protected void rb_googlePlusSignIn_off_Checked(object sender, EventArgs e) {
        rb_googlePlusSignIn_on.Checked = false;
        rb_googlePlusSignIn_off.Checked = true;
        pnl_googleSettings.Visible = false;
        pnl_googleSettings.Enabled = false;
        ServerSettings.update_SignInWithGoogle(false);
    }

    protected void rb_twitterSignIn_on_Checked(object sender, EventArgs e) {
        rb_twitterSignIn_on.Checked = true;
        rb_twitterSignIn_off.Checked = false;
        pnl_twitterSettings.Visible = true;
        pnl_twitterSettings.Enabled = true;
        LoadTwitterSettings();
        ServerSettings.update_SignInWithTwitter(true);
    }
    protected void rb_twitterSignIn_off_Checked(object sender, EventArgs e) {
        rb_twitterSignIn_on.Checked = false;
        rb_twitterSignIn_off.Checked = true;
        pnl_twitterSettings.Visible = false;
        pnl_twitterSettings.Enabled = false;
        ServerSettings.update_SignInWithTwitter(false);
    }

    protected void rb_facebookSignIn_on_Checked(object sender, EventArgs e) {
        rb_facebookSignIn_on.Checked = true;
        rb_facebookSignIn_off.Checked = false;
        pnl_facebookSettings.Visible = true;
        pnl_facebookSettings.Enabled = true;
        LoadFacebookSettings();
        ServerSettings.update_SignInWithFacebook(true);
    }
    protected void rb_facebookSignIn_off_Checked(object sender, EventArgs e) {
        rb_facebookSignIn_on.Checked = false;
        rb_facebookSignIn_off.Checked = true;
        pnl_facebookSettings.Visible = false;
        pnl_facebookSettings.Enabled = false;
        ServerSettings.update_SignInWithFacebook(false);
    }

    protected void rb_SignUpConfirmationEmail_on_Checked(object sender, EventArgs e) {
        rb_SignUpConfirmationEmail_on.Checked = true;
        rb_SignUpConfirmationEmail_off.Checked = false;
        ServerSettings.update_SignUpConfirmationEmail(true);
    }
    protected void rb_SignUpConfirmationEmail_off_Checked(object sender, EventArgs e) {
        rb_SignUpConfirmationEmail_off.Checked = true;
        rb_SignUpConfirmationEmail_on.Checked = false;
        ServerSettings.update_SignUpConfirmationEmail(false);
    }

    protected void rb_emailassociation_on_Checked(object sender, EventArgs e) {
        pnl_emailassociation.Enabled = true;
        pnl_emailassociation.Visible = true;
        rb_emailassociation_on.Checked = true;
        rb_emailassociation_off.Checked = false;
        ServerSettings.update_UserSignUpEmailChecker(true);
    }
    protected void rb_emailassociation_off_Checked(object sender, EventArgs e) {
        pnl_emailassociation.Enabled = false;
        pnl_emailassociation.Visible = false;
        rb_emailassociation_off.Checked = true;
        rb_emailassociation_on.Checked = false;
        ServerSettings.update_UserSignUpEmailChecker(false);
    }

    protected void btn_UpdateEmailAssociation_Click(object sender, EventArgs e) {
        string email = tb_EmailAssociation.Text.Trim();
        if (!string.IsNullOrEmpty(email)) {
            if (tb_EmailAssociation.Text.Contains("@")) {
                email = email.Substring(email.IndexOf('@') + 1);
            }
            ServerSettings.update_UserSignUpEmailAssociation(email);

            tb_EmailAssociation.Text = email;
        }
        else {
            string messageError = "$('#emailassociation_error').html('<small>Email cannot be empty.</small>');setTimeout(function(){$('#emailassociation_error').html('');},4000);";
            RegisterPostbackScripts.RegisterStartupScript(this, messageError);
        }
    }

    protected void rb_ForceGroupLogin_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_ForceGroupLogin_on.Checked = true;
            rb_ForceGroupLogin_off.Checked = false;
            ServerSettings.update_ForceGroupLogin(true);

            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            pnl_nologinrequired.Enabled = false;
            pnl_nologinrequired.Visible = false;
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
    }
    protected void rb_ForceGroupLogin_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_ForceGroupLogin_on.Checked = false;
            rb_ForceGroupLogin_off.Checked = true;
            ServerSettings.update_ForceGroupLogin(false);

            pnl_nologinrequired.Enabled = true;
            pnl_nologinrequired.Visible = true;

            if ((!MainServerSettings.NoLoginRequired) && (!MainServerSettings.ShowPreviewButtonLogin)) {
                pnl_NoLoginMainPage.Enabled = false;
                pnl_NoLoginMainPage.Visible = false;
                pnl_showpreviewbutton.Enabled = true;
                pnl_showpreviewbutton.Visible = true;
            }
            else if ((MainServerSettings.NoLoginRequired) && (MainServerSettings.ShowPreviewButtonLogin)) {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
                pnl_showpreviewbutton.Enabled = false;
                pnl_showpreviewbutton.Visible = false;
            }
            else {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
            }
        }
    }

    protected void rb_ShowPreviewButtonLogin_on_CheckedChanged(object sender, EventArgs e) {
        rb_ShowPreviewButtonLogin_on.Checked = true;
        rb_ShowPreviewButtonLogin_off.Checked = false;
        ServerSettings.update_ShowPreviewButtonLogin(true);
        if ((!MainServerSettings.NoLoginRequired) && (!MainServerSettings.ShowPreviewButtonLogin)) {
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
        else {
            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;
        }


    }
    protected void rb_ShowPreviewButtonLogin_off_CheckedChanged(object sender, EventArgs e) {
        rb_ShowPreviewButtonLogin_on.Checked = false;
        rb_ShowPreviewButtonLogin_off.Checked = true;
        ServerSettings.update_ShowPreviewButtonLogin(false);
        if ((!MainServerSettings.NoLoginRequired) && (!MainServerSettings.ShowPreviewButtonLogin)) {
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
        else {
            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;
        }


    }

    protected void rb_nologinrequired_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_nologinrequired_on.Checked = true;
            rb_nologinrequired_off.Checked = false;
            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            ServerSettings.update_NoLoginRequired(true);

            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;

            pnl_showloginmodalondemomode.Enabled = true;
            pnl_showloginmodalondemomode.Visible = true;
        }
    }
    protected void rb_nologinrequired_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_nologinrequired_on.Checked = false;
            rb_nologinrequired_off.Checked = true;
            pnl_showpreviewbutton.Enabled = true;
            pnl_showpreviewbutton.Visible = true;
            ServerSettings.update_NoLoginRequired(false);
            if (!MainServerSettings.ShowPreviewButtonLogin) {
                pnl_NoLoginMainPage.Enabled = false;
                pnl_NoLoginMainPage.Visible = false;
            }
            else {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
            }

            pnl_showloginmodalondemomode.Enabled = false;
            pnl_showloginmodalondemomode.Visible = false;
        }
    }

    protected void rb_ShowLoginModalOnDemoMode_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_ShowLoginModalOnDemoMode_on.Checked = true;
            rb_ShowLoginModalOnDemoMode_off.Checked = false;
            ServerSettings.update_ShowLoginModalOnDemoMode(true);

        }
    }
    protected void rb_ShowLoginModalOnDemoMode_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_ShowLoginModalOnDemoMode_on.Checked = false;
            rb_ShowLoginModalOnDemoMode_off.Checked = true;
            ServerSettings.update_ShowLoginModalOnDemoMode(false);

        }
    }

    protected void btn_loginPageMessage_Click(object sender, EventArgs e) {
        string message = tb_loginPageMessage.Text.Trim();
        if (!string.IsNullOrEmpty(message)) {
            string date = ServerSettings.ServerDateTime.ToString();
            ServerSettings.UpdateLoginMessage(message, date);

            lbl_loginMessageDate.Text = "<b class='pad-right'>Date Updated:</b>" + date;
        }
    }

    protected void lbtn_loginPageMessage_clear_Click(object sender, EventArgs e) {
        string date = ServerSettings.ServerDateTime.ToString();
        ServerSettings.UpdateLoginMessage("", date);

        tb_loginPageMessage.Text = "";
        lbl_loginMessageDate.Text = "<b class='pad-right'>Date Updated:</b>" + date;
    }

    #endregion


    #region API Settings

    private void LoadTwitterSettings() {
        tb_updateTwitterAccessToken.Text = MainServerSettings.TwitterAccessToken;

        tb_updateTwitterAccessTokenSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(MainServerSettings.TwitterAccessToken)) {
            tb_updateTwitterAccessTokenSecret.Text = "***************************************";
        }

        tb_updateTwitterConsumerKey.Text = MainServerSettings.TwitterConsumerKey;

        tb_updateTwitterConsumerSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(MainServerSettings.TwitterConsumerSecret)) {
            tb_updateTwitterConsumerSecret.Text = "***************************************";
        }
    }
    private void LoadGoogleSettings() {
        txt_GoogleClientId.Text = MainServerSettings.GoogleClientId;

        txt_GoogleClientSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(MainServerSettings.GoogleClientSecret)) {
            txt_GoogleClientSecret.Text = "***************************************";
        }

        StringBuilder strUrls = new StringBuilder();
        List<string> redirectUrls = GetRedirectUrls(SocialSignInApis.SocialSignIn.GoogleRedirectQuery);
        foreach (string url in redirectUrls) {
            strUrls.Append("<h4>" + url + "</h4><div class='clear-space-two'></div>");
        }

        ltl_googleRedirect.Text = strUrls.ToString();
    }
    private void LoadFacebookSettings() {
        txt_facebookAppId.Text = MainServerSettings.FacebookAppId;

        txt_facebookAppSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(MainServerSettings.FacebookAppSecret)) {
            txt_facebookAppSecret.Text = "***************************************";
        }

        StringBuilder strUrls = new StringBuilder();
        List<string> redirectUrls = GetRedirectUrls(SocialSignInApis.SocialSignIn.FacebookRedirectQuery);
        foreach (string url in redirectUrls) {
            strUrls.Append("<h4>" + url + "</h4><div class='clear-space-two'></div>");
        }

        ltl_facebookRedirect.Text = strUrls.ToString();
    }
    private List<string> GetRedirectUrls(string query) {
        string mainUrl = Request.Url.AbsoluteUri.Split('?')[0].Replace("SiteTools/UserMaintenance/UserAccounts.aspx", "");

        List<string> urls = new List<string>();
        urls.Add(mainUrl + "Default.aspx" + query);
        urls.Add(mainUrl + ServerSettings.DefaultStartupPage + query);
        urls.Add(mainUrl + "AppRemote.aspx" + query);

        return urls;
    }

    #endregion


    #region Twitter Update Buttons

    protected void btn_updateTwitterAccessToken_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            ServerSettings.update_TwitterAccessToken(tb_updateTwitterAccessToken.Text.Trim());

        }
    }
    protected void btn_updateTwitterAccessTokenSecret_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            if (tb_updateTwitterAccessTokenSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_TwitterAccessTokenSecret(tb_updateTwitterAccessTokenSecret.Text.Trim());
                if (tb_updateTwitterAccessTokenSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    tb_updateTwitterAccessTokenSecret.Text = "***************************************";
                }
            }
        }
    }
    protected void btn_updateTwitterConsumerKey_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            ServerSettings.update_TwitterConsumerKey(tb_updateTwitterConsumerKey.Text.Trim());

        }
    }
    protected void btn_updateTwitterConsumerSecret_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            if (tb_updateTwitterConsumerSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_TwitterConsumerSecret(tb_updateTwitterConsumerSecret.Text.Trim());
                if (tb_updateTwitterConsumerSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    tb_updateTwitterConsumerSecret.Text = "***************************************";
                }
            }
        }
    }

    #endregion


    #region Google Update Buttons

    protected void btn_updateGoogleClientId_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            ServerSettings.update_GoogleClientId(txt_GoogleClientId.Text.Trim());

        }
    }
    protected void btn_updateGoogleClientSecret_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            if (txt_GoogleClientSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_GoogleClientSecret(txt_GoogleClientSecret.Text.Trim());
                if (txt_GoogleClientSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    txt_GoogleClientSecret.Text = "***************************************";
                }
            }
        }
    }

    #endregion


    #region Facebook Update Buttons

    protected void btn_updateFacebookAppId_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            ServerSettings.update_FacebookAppId(txt_facebookAppId.Text.Trim());

        }
    }
    protected void btn_updateFacebookAppSecret_Click(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            if (txt_facebookAppSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_FacebookAppSecret(txt_facebookAppSecret.Text.Trim());
                if (txt_facebookAppSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    txt_facebookAppSecret.Text = "***************************************";
                }
            }

        }
    }

    #endregion


    #region Build User Apps and Plugins
    private void LoadAppsAndUsers() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2, "UserAppsAndPlugins_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Image", "55px", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Username", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Apps and Plugins", "500px", true, false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                MemberDatabase member = new MemberDatabase(u.UserName);
                bool canContinue = true;

                if (MainServerSettings.AllowPrivacy) {
                    if ((member.PrivateAccount) && (u.UserName.ToLower() != CurrentUsername.ToLower())
                        && (!IsUserNameEqualToAdmin()) && ((!u.IsLockedOut) || (u.IsApproved))) {
                        canContinue = false;
                    }
                }

                if (canContinue) {
                    CurrentAppObject = new App(u.UserName);
                    if (hf_editUserApps.Value != member.Username) {
                        List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();
                        bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40, member.SiteTheme), TableBuilderColumnAlignment.Left));
                        bodyColumns.Add(new TableBuilderBodyColumnValues("Username", u.UserName, TableBuilderColumnAlignment.Left));

                        string appListHolder = "<div class='input-settings-holder'><span class='font-bold'>Apps</span><div class='clear-space-two'></div>" + LoadAppIcons(member, CurrentAppObject) + "<div class='clear'></div></div>";
                        string pluginListHolder = "<div class='input-settings-holder'><span class='font-bold'>Plugins</span><div class='clear-space-two'></div>" + LoadPlugins(member) + "<div class='clear'></div></div>";

                        bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, appListHolder + pluginListHolder, TableBuilderColumnAlignment.Left));
                        tableBuilder.AddBodyRow(bodyColumns, "<a class='td-edit-btn' title='Edit' onclick=\"EditUserApps('" + member.Username + "');return false;\"></a>");
                    }
                    else {
                        List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();
                        bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40, member.SiteTheme), TableBuilderColumnAlignment.Left));
                        bodyColumns.Add(new TableBuilderBodyColumnValues("Username", u.UserName, TableBuilderColumnAlignment.Left));

                        string removeAllAppsBtn = "<a class='float-right margin-right-big margin-top-sml' onclick=\"RemoveAllApp();return false;\">Remove all apps</a>";
                        string removeAllPluginsBtn = "<a class='float-right' onclick=\"RemoveAllPlugins();return false;\">Remove all plugins</a>";

                        string selectControl = "<input type='button' class='float-right input-buttons no-margin' value='Install Package' onclick='AppPackageInstall()' /><select id='ddl_appPackages' class='float-right margin-right'><span class='float-right pad-right font-bold pad-top-sml'>App Package</span>";
                        AppPackages wp = new AppPackages(true);
                        List<Dictionary<string, string>> dt = wp.listdt;
                        foreach (Dictionary<string, string> dr in dt) {
                            string packId = dr["ID"];
                            string packName = dr["PackageName"];
                            selectControl += "<option value='" + packId + "'>" + packName + "</option>";
                        }
                        selectControl += "</select>";

                        string appListHolder = "<div class='input-settings-holder'><span class='font-bold float-left'>Apps</span>" + selectControl + removeAllAppsBtn + "<div class='clear-space-two'></div>" + LoadAppIconsEdit(member, CurrentAppObject) + "<div class='clear'></div></div>";
                        string pluginListHolder = "<div class='input-settings-holder'><span class='font-bold float-left'>Plugins</span>" + removeAllPluginsBtn + "<div class='clear-space'></div>" + LoadPluginListEdit(member) + "<div class='clear'></div></div>";

                        bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, appListHolder + pluginListHolder, TableBuilderColumnAlignment.Left));
                        tableBuilder.AddBodyRow(bodyColumns, "<a class='td-cancel-btn' title='Close edit' onclick=\"CancelUserApps();return false;\"></a>");
                    }
                }
            }
        }
        #endregion

        pnl_UserAppsAndPluginList.Controls.Clear();
        pnl_UserAppsAndPluginList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No users found"));

        if (hf_editUserApps.Value == "cancel") {
            hf_editUserApps.Value = string.Empty;
            updatepnl_UserAppsAndPlugins_Edit.Update();
        }
    }

    private string LoadAppIcons(MemberDatabase member, App apps) {
        var appScript = new StringBuilder();
        List<string> appList = member.EnabledApps;

        foreach (string w in appList) {
            Apps_Coll coll = apps.GetAppInformation(w);

            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(coll, member)) {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(coll.ID)) {
                continue;
            }
            if ((CurrentUsername.ToLower() != coll.CreatedBy.ToLower()) && (coll.IsPrivate)) {
                continue;
            }

            string image = "<img alt='' src='" + ResolveUrl("~/" + coll.Icon) + "' class='app-icon-admin-icon' />";
            if (string.IsNullOrEmpty(coll.Icon)) {
                image = string.Empty;
            }

            appScript.Append("<div class='app-icon-admin'>" + image);
            appScript.Append("<span class='app-span-modify'>" + coll.AppName + "</span>");
            appScript.Append("<div class='clear'></div></div>");
        }

        return !string.IsNullOrEmpty(appScript.ToString())
                   ? appScript.ToString()
                   : "<div class='float-left'>No apps installed for user</div>";
    }
    private string LoadPlugins(MemberDatabase member) {
        StringBuilder pluginScriptInstalled = new StringBuilder();

        string username = member.Username;

        SitePlugins _plugins = new SitePlugins(username);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, member)) {
                        continue;
                    }
                }

                bool isInstalled = false;
                foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                    if (userColl.PluginID == coll.ID) {
                        isInstalled = true;
                        break;
                    }
                }

                if (isInstalled) {
                    pluginScriptInstalled.Append("<div class='app-icon-admin'>");
                    pluginScriptInstalled.Append("<span class='app-span-modify '>" + coll.PluginName + "</span>");
                    pluginScriptInstalled.Append("<div class='clear'></div></div>");
                }
            }
        }

        return !string.IsNullOrEmpty(pluginScriptInstalled.ToString())
                   ? pluginScriptInstalled.ToString()
                   : "<div class='float-left'>No plugins installed for user</div>";
    }

    private string LoadAppIconsEdit(MemberDatabase member, App apps) {
        var appScriptInstalled = new StringBuilder();
        var appScriptUninstalled = new StringBuilder();

        apps.GetAllApps();
        var dt = apps.AppList;
        foreach (Apps_Coll dr in dt) {
            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, member)) {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(dr.ID)) {
                continue;
            }

            if ((member.Username != dr.CreatedBy.ToLower()) && (dr.IsPrivate)) {
                continue;
            }

            string appName = dr.AppName;
            string appId = dr.AppId;
            string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
            if (string.IsNullOrEmpty(dr.Icon)) {
                image = string.Empty;
            }

            if (member.UserHasApp(appId)) {
                appScriptInstalled.Append("<div id='app-icon-" + appId + "' class='app-icon-admin'>");
                appScriptInstalled.Append("<a onclick=\"RemoveApp('" + appId + "');return false;\" title='Remove " + appName + "' class='float-left img-collapse-sml cursor-pointer'></a>");
                appScriptInstalled.Append(image + "<span class='app-span-modify'>" + appName + "</span>");
                appScriptInstalled.Append("<div class='clear'></div></div>");
            }
            else {
                appScriptUninstalled.Append("<div id='app-icon-" + appId + "' class='app-icon-admin'>");
                appScriptUninstalled.Append("<a onclick=\"AddApp('" + appId + "');return false;\" title='Add " + appName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                appScriptUninstalled.Append(image + "<span class='app-span-modify'>" + appName + "</span>");
                appScriptUninstalled.Append("<div class='clear'></div></div>");
            }
        }

        return HelperMethods.TableAddRemove(appScriptUninstalled.ToString(), appScriptInstalled.ToString(), "Apps Available to Install", "Apps Installed", false, true);
    }
    private string LoadPluginListEdit(MemberDatabase member) {
        StringBuilder pluginScriptUninstalled = new StringBuilder();
        StringBuilder pluginScriptInstalled = new StringBuilder();

        string username = member.Username;

        SitePlugins _plugins = new SitePlugins(username);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, member)) {
                        continue;
                    }
                }

                string userPluginID = "";
                bool isInstalled = false;
                foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                    if (userColl.PluginID == coll.ID) {
                        userPluginID = userColl.ID;
                        isInstalled = true;
                        break;
                    }
                }

                if (isInstalled) {
                    pluginScriptInstalled.Append("<div class='app-icon-admin'>");
                    pluginScriptInstalled.Append("<a class='float-left img-collapse-sml cursor-pointer' title='Uninstall " + coll.PluginName + " Plugin' onclick='RemovePlugin(\"" + userPluginID + "\");return false;'></a>");
                    pluginScriptInstalled.Append("<span class='app-span-modify'>" + coll.PluginName + "</span>");
                    pluginScriptInstalled.Append("<div class='clear'></div></div>");
                }
                else {
                    pluginScriptUninstalled.Append("<div class='app-icon-admin'>");
                    pluginScriptUninstalled.Append("<a class='float-left img-expand-sml cursor-pointer' title='Install " + coll.PluginName + " Plugin' onclick='AddPlugin(\"" + coll.ID + "\");return false;'></a>");
                    pluginScriptUninstalled.Append("<span class='app-span-modify'>" + coll.PluginName + "</span>");
                    pluginScriptUninstalled.Append("<div class='clear'></div></div>");
                }
            }
        }

        return HelperMethods.TableAddRemove(pluginScriptUninstalled.ToString(), pluginScriptInstalled.ToString(), "Plugins Available to Install", "Plugins Installed", false, true);
    }

    protected void hf_editUserApps_Changed(object sender, EventArgs e) {
        LoadAppsAndUsers();
    }
    protected void hf_addApp_Changed(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        if (!string.IsNullOrEmpty(hf_addApp.Value)) {
            MemberDatabase member = new MemberDatabase(forUser);
            member.UpdateEnabledApps(hf_addApp.Value);
            LoadAppsAndUsers();
            UpdateSidebarAppList(forUser);
        }

        hf_addApp.Value = string.Empty;
    }
    protected void hf_removeApp_Changed(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        if (!string.IsNullOrEmpty(hf_removeApp.Value)) {
            MemberDatabase member = new MemberDatabase(forUser);
            member.RemoveEnabledApp(hf_removeApp.Value);
            LoadAppsAndUsers();
            UpdateSidebarAppList(forUser);
        }

        hf_removeApp.Value = string.Empty;
    }
    protected void hf_removeAllApp_Changed(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        MemberDatabase member = new MemberDatabase(forUser);
        member.RemoveAllEnabledApps();
        LoadAppsAndUsers();
        UpdateSidebarAppList(forUser);

        hf_removeAllApp.Value = string.Empty;
    }
    protected void hf_appPackage_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_appPackage.Value)) {
            string forUser = hf_editUserApps.Value;

            MemberDatabase member = new MemberDatabase(forUser);
            AppPackages wp = new AppPackages(false);
            string[] wpList = wp.GetAppList(hf_appPackage.Value);

            foreach (string appId in wpList) {
                if (!member.UserHasApp(appId)) {
                    member.UpdateEnabledApps(appId);
                }
            }

            LoadAppsAndUsers();
            UpdateSidebarAppList(forUser);
        }

        hf_appPackage.Value = string.Empty;
    }

    private void UpdateSidebarAppList(string forUser) {
        if (forUser.ToLower() == CurrentUsername.ToLower()) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser();
        }
    }

    protected void hf_removePlugin_ValueChanged(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        if (!string.IsNullOrEmpty(hf_removePlugin.Value)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.deleteItemForUser(hf_removePlugin.Value);
            LoadAppsAndUsers();
        }

        hf_removePlugin.Value = string.Empty;
    }
    protected void hf_addPlugin_ValueChanged(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        if (!string.IsNullOrEmpty(hf_addPlugin.Value)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.addItemForUser(hf_addPlugin.Value);
            LoadAppsAndUsers();
        }

        hf_addPlugin.Value = string.Empty;
    }
    protected void hf_removeAllPlugins_ValueChanged(object sender, EventArgs e) {
        string forUser = hf_editUserApps.Value;

        if (!string.IsNullOrEmpty(forUser)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.BuildSitePluginsForUser();
            foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                _plugins.deleteItemForUser(userColl.ID);
            }

            LoadAppsAndUsers();
        }

        hf_removeAllPlugins.Value = string.Empty;
    }
    #endregion

}