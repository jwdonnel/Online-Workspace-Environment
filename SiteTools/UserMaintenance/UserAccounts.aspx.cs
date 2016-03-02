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

public partial class SiteTools_UserAccounts : Page {

    #region private variables

    private ServerSettings _ss = new ServerSettings();
    private readonly AppCategory _appCategory = new AppCategory(false);
    private string _sitetheme;
    private string _username;
    private App _apps;
    private string _ctrlName;

    #endregion


    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                _apps = new App(userId.Name);

                var member = new MemberDatabase(userId.Name);
                _sitetheme = member.SiteTheme;

                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null) {
                    if (!Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                        pnl_admin_tools.Enabled = false;
                        pnl_admin_tools.Visible = false;
                        btn_CreateNewUsers.Visible = false;
                    }

                    string ctlId = sm.AsyncPostBackSourceElementID;
                    _ctrlName = ctlId;

                    BuildRolesDropDownList(IsPostBack);

                    if ((!IsPostBack) || ((_ctrlName != "hf_sortusers") && (_ctrlName != "hf_lockUser") && (_ctrlName != "hf_deleteUser")
                        && (_ctrlName != "hf_refreshList") && (_ctrlName != "hf_resetPassword")))
                        BuildUsers();
                }

                PageLoadInit.BuildLinks(pnlLinkBtns, _username, this.Page);

                LoadSettings();
                GetPostBack();
            }
            else
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }
    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildUsers();
    }
    private void GetPostBack() {
        string controlName = Request.Params["__EVENTTARGET"];
        switch (controlName) {
            case "MainContent_btn_roleDelete":
                DeleteRole_Click();
                break;
        }
    }
    private void LoadSettings() {
        pnl_usersettings.Visible = true;
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            pnl_usersettings.Visible = false;
        }

        rb_usersignup_holder.Visible = true;
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) && _ss.CustomizationsLocked) {
            rb_usersignup_holder.Visible = false;
        }

        pnl_ConfirmationEmailSignUp.Visible = false;
        if (_ss.EmailSystemStatus && _ss.AllowUserSignUp) {
            pnl_ConfirmationEmailSignUp.Visible = true;
        }

        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            pnl_socialSignIn_UserSignup.Enabled = true;
            pnl_socialSignIn_UserSignup.Visible = true;
        }

        UpdateUserSignUpRoleDropdown();

        if (_ss.UserSignUpEmailChecker) {
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
        tb_EmailAssociation.Text = _ss.UserSignUpEmailAssociation;

        if (_ss.AllowUserSignUp) {
            pnl_UserSignUp.Enabled = true;
            pnl_UserSignUp.Visible = true;
            rb_allowusersignup_on.Checked = true;
            rb_allowusersignup_off.Checked = false;
            if (_ss.EmailSystemStatus) {
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

        if (_ss.SignInWithGoogle) {
            rb_googlePlusSignIn_on.Checked = true;
            rb_googlePlusSignIn_off.Checked = false;
        }
        else {
            rb_googlePlusSignIn_on.Checked = false;
            rb_googlePlusSignIn_off.Checked = true;
        }

        if (_ss.SignInWithTwitter) {
            rb_twitterSignIn_on.Checked = true;
            rb_twitterSignIn_off.Checked = false;
        }
        else {
            rb_twitterSignIn_on.Checked = false;
            rb_twitterSignIn_off.Checked = true;
        }

        if (_ss.SignInWithFacebook) {
            rb_facebookSignIn_on.Checked = true;
            rb_facebookSignIn_off.Checked = false;
        }
        else {
            rb_facebookSignIn_on.Checked = false;
            rb_facebookSignIn_off.Checked = true;
        }

        if (_ss.SignUpConfirmationEmail) {
            rb_SignUpConfirmationEmail_on.Checked = true;
            rb_SignUpConfirmationEmail_off.Checked = false;
        }
        else {
            rb_SignUpConfirmationEmail_on.Checked = false;
            rb_SignUpConfirmationEmail_off.Checked = true;
        }
    }

    #endregion


    #region User Properties

    private void BuildUsers() {
        pnl_Users.Controls.Clear();

        string tbSearch = tb_search.Text.Trim().ToLower();

        MembershipUserCollection coll = Membership.GetAllUsers();
        IEnumerable<Dictionary<string, string>> sortedList;

        if (ViewState["SortBy"] == null) {
            ViewState["SortBy"] = "fullname";
        }

        if (ViewState["SortDirection"] == null) {
            ViewState["SortDirection"] = "asc";
        }

        lbl_totalUsers.Text = "<b class='pad-right'>Total Users</b>" + (coll.Count - 1).ToString();

        int userCount = 0;
        bool chatEnabled = _ss.ChatEnabled;
        StringBuilder strUsers = new StringBuilder();

        strUsers.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        strUsers.Append("<tr class='myHeaderStyle'><td width='45px'></td>");
        strUsers.Append("<td width='55px'>Image</td>");

        #region fullname header
        if (ViewState["SortBy"].ToString() == "fullname") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='fullname' title='Sort by Full Name' style='min-width: 200px;'>Full Name</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='fullname' title='Sort by Full Name' style='min-width: 200px;'>Full Name</td>");
        }
        #endregion

        #region username header
        if (ViewState["SortBy"].ToString() == "username") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='username' title='Sort by Username' width='200px'>Username</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='username' title='Sort by Username' width='200px'>Username</td>");
        }
        #endregion

        #region email header
        if (ViewState["SortBy"].ToString() == "email") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='email' title='Sort by Email' width='200px'>Email</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='email' title='Sort by Email' width='200px'>Email</td>");
        }
        #endregion

        #region datejoined header
        if (ViewState["SortBy"].ToString() == "datejoined") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='datejoined' title='Sort by Date Joined' width='100px'>Date Joined</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='datejoined' title='Sort by Date Joined' width='100px'>Date Joined</td>");
        }
        #endregion

        #region lastlogin header
        if (ViewState["SortBy"].ToString() == "lastlogin") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='lastlogin' title='Sort by Last Login Date' width='150px'>Last Login</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='lastlogin' title='Sort by Last Login Date' width='150px'>Last Login</td>");
        }
        #endregion

        #region role header
        if (ViewState["SortBy"].ToString() == "role") {
            strUsers.Append("<td class='td-sort-click active " + ViewState["SortDirection"].ToString() + "' data-sortname='role' title='Sort by Role' width='135px'>Role</td>");
        }
        else {
            strUsers.Append("<td class='td-sort-click' data-sortname='role' title='Sort by Role' width='135px'>Role</td>");
        }
        #endregion

        strUsers.Append("<td width='105px'>Actions</td></tr>");

        List<Dictionary<string, string>> userList = new List<Dictionary<string, string>>();

        #region Build User List
        foreach (MembershipUser u in coll) {
            if (u != null) {
                if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    if (tbSearch.ToLower() == "me") {
                        tbSearch = _username;
                    }
                    if ((tbSearch == "search users") || (string.IsNullOrEmpty(tbSearch)) || (u.UserName.ToLower().Contains(tbSearch.ToLower()))) {

                        MemberDatabase member = new MemberDatabase(u.UserName);
                        string userMe = string.Empty;
                        if (u.UserName.ToLower() == _username.ToLower()) {
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

                        Dictionary<string, string> userEntry = new Dictionary<string, string>();
                        userEntry.Add("userimage", UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 38));
                        userEntry.Add("fullname", HelperMethods.MergeFMLNames(member));
                        userEntry.Add("username", u.UserName + userMe);
                        userEntry.Add("email", u.Email);
                        userEntry.Add("datejoined", u.CreationDate.ToShortDateString());
                        userEntry.Add("lastlogin", u.LastLoginDate.ToString());
                        userEntry.Add("role", userRole);
                        userEntry.Add("buttons", GetUserButtons(member, u));
                        userList.Add(userEntry);
                        userCount++;
                    }
                }
            }
        }

        if (ViewState["SortBy"].ToString() == "datejoined" || ViewState["SortBy"].ToString() == "lastlogin") {
            try {
                sortedList = userList.OrderBy(x => Convert.ToDateTime(x[ViewState["SortBy"].ToString()]));
            }
            catch {
                sortedList = userList.OrderBy(x => x[ViewState["SortBy"].ToString()]);
            }
        }
        else {
            sortedList = userList.OrderBy(x => x[ViewState["SortBy"].ToString()]);
        }
        if (ViewState["SortDirection"] == "desc") {
            userList.Reverse();
        }

        int ii = 0;
        foreach (Dictionary<string, string> x in userList) {
            strUsers.Append("<tr class='myItemStyle GridNormalRow'>");
            strUsers.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + (ii + 1).ToString() + "</td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["userimage"] + "</td>");
            strUsers.Append("<td align='left' class='border-right border-bottom'>" + x["fullname"] + "</td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["username"] + "</td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["email"] + "</td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["datejoined"] + "</span></td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["lastlogin"] + "</td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["role"] + "</span></td>");
            strUsers.Append("<td align='center' class='border-right border-bottom'>" + x["buttons"] + "</td></tr>");
            ii++;
        }
        #endregion

        strUsers.Append("</tbody></table>");
        if (userCount > 0) {
            pnl_Users.Controls.Add(new LiteralControl(strUsers.ToString() + "<div class='clear-space'></div>"));
        }
        else {
            pnl_Users.Controls.Add(new LiteralControl("<div class='emptyGridView'>No users found</div>"));
        }
    }
    private string GetUserButtons(MemberDatabase member, MembershipUser u) {
        var strUsers = new StringBuilder();
        string appUser = "<a href='#' title='Edit User Apps/Plugins' class='img-app-dark pad-all-sml margin-left-sml margin-right-sml' onclick='openWSE.LoadIFrameContent(\"SiteTools/iframes/UsersAndApps.aspx?u=" + u.UserName + "\", this);return false;'></a>";
        string unlockUser = "<a href='#' title='Lock User' class='img-unlock pad-all-sml margin-left-sml margin-right-sml RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
        string lockUser = "<a href='#' title='Unlock User' class='img-lock pad-all-sml margin-left-sml margin-right-sml RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
        string deleteUser = "<a href='#' title='Delete User' class='td-delete-btn margin-left-sml margin-right-sml' onclick='DeleteUser(\"" + u.UserName + "\");return false;'></a>";
        string editUser = "<a href='#' title='Edit User' class='td-edit-btn margin-left-sml margin-right-sml' onclick='openWSE.LoadIFrameContent(\"SiteTools/UserMaintenance/AcctSettings.aspx?toolView=true&u=" + u.UserName + "\", this);return false;'></a>";
        string password = string.Empty;
        if (!member.IsSocialAccount) {
            password = "<a href='#' title='Reset User Password' class='img-password pad-all-sml margin-left-sml margin-right-sml' onclick='ResetPassword(\"" + u.UserName + "\");return false;'></a>";
        }

        string mail = "<a href='#' title='Email me on login' class='img-mail pad-all-sml margin-left-sml margin-right-sml' onclick='EmailUser(\"" + u.UserName + "\");return false;'></a>";
        string nomail = "<a href='#' title='Dont email me on login' class='img-nomail pad-all-sml margin-left-sml margin-right-sml' onclick='CancelEmailUser(\"" + u.UserName + "\");return false;'></a>";

        if (u.UserName.ToLower() == _username.ToLower()) {
            strUsers.Append(appUser);
            strUsers.Append(editUser);
            return strUsers.ToString();
        }

        if (!_ss.EmailSystemStatus) {
            mail = string.Empty;
            nomail = string.Empty;
        }

        if (u.UserName.ToLower() != _username.ToLower()) {
            if (member.EmailUponLoginList.Any(un => un.ToLower() == _username.ToLower()))
                strUsers.Append(nomail);
            else
                strUsers.Append(mail);
        }

        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            strUsers.Append(password);
            if ((u.IsLockedOut) || (!u.IsApproved))
                strUsers.Append(lockUser);
            else
                strUsers.Append(unlockUser);

            strUsers.Append(appUser);
            strUsers.Append(editUser);
            strUsers.Append(deleteUser);
        }
        else {
            if (u.UserName.ToLower() == _username.ToLower()) {
                strUsers.Append(password);
                strUsers.Append(appUser);
                strUsers.Append(editUser);
            }
            else {
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    if (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) {
                        strUsers.Append(password);
                        strUsers.Append(appUser);
                        strUsers.Append(editUser);
                    }
                }
                else {
                    strUsers.Append(password);
                    if ((_username.ToLower() != u.UserName.ToLower()) && (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName))) {
                        if ((u.IsLockedOut) || (!u.IsApproved))
                            strUsers.Append(lockUser);
                        else
                            strUsers.Append(unlockUser);
                    }

                    if (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) {
                        strUsers.Append(appUser);
                        strUsers.Append(editUser);
                    }

                    if ((_username.ToLower() != u.UserName.ToLower()) && (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)))
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
                if (!string.IsNullOrEmpty(newPassword)) {
                    if (msu.ChangePassword(oldPassword, newPassword)) {
                        lbl_passwordReset.Text = "";
                        txt_PasswordFinishedText.Text = "<h3>Password has been changed for <b>" + username + "</b></h3><div class='clear' style='height: 45px'></div>";
                        ChangeUserPassword.Enabled = false;
                        ChangeUserPassword.Visible = false;
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, '" + pwreset_overlay.ClientID + "', 'Password Reset Complete');");
                    }
                }
            }
        }
    }
    protected void hf_clearsearch_Changed(object sender, EventArgs e) {

    }
    protected void hf_sortusers_Changed(object sender, EventArgs e) {
        if (ViewState["SortBy"] != null && ViewState["SortBy"].ToString() == hf_sortusers.Value) {
            if (ViewState["SortDirection"] != null && ViewState["SortDirection"].ToString() == "asc") {
                ViewState["SortDirection"] = "desc";
            }
            else {
                ViewState["SortDirection"] = "asc";
            }
        }
        else {
            ViewState["SortBy"] = hf_sortusers.Value;
            ViewState["SortDirection"] = "asc";
        }

        BuildUsers();
        hf_sortusers.Value = string.Empty;
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

                    string color = ur.RandomColor();
                    ur.RegisterNewUser(firstName, lastName, email, color);
                    ur.RegisterDefaults();
                }
            }
        }

        ServerSettings.RefreshPage(Page, string.Empty);
    }

    protected void btn_rebuild_uc_Clicked(object sender, EventArgs e) {
        MemberDatabase tempMember = new MemberDatabase(_username);
        tempMember.ClearUserTable();

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            MemberDatabase m = new MemberDatabase(u.UserName);
            if (!m.UserCustomizationsEnabled) {
                m.AddUserCustomizationRow(u.UserName);
            }
        }

        BuildUsers();
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
        ur.RegisterNewUser(tb_firstnamereg.Text, tb_lastnamereg.Text, email, color1.Text);
        ur.RegisterDefaults();

        BuildUsers();

        MembershipUserCollection coll = Membership.GetAllUsers();
        string messageCount = "<b class='pad-right'>Total Users</b>" + (coll.Count - 1).ToString();
        string script = "$('#MainContent_lbl_totalUsers').html(\"" + messageCount + "\");";

        RegisterPostbackScripts.RegisterStartupScript(this, script + "$('.ModalButtonHolder').hide();$('#NewUser-element').find('.Modal-element-modal').removeClass('add-user-modal');");
        hf_creatingNewUser.Value = "false";
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
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                e.Cancel = true;
                hf_creatingNewUser.Value = "false";
            }
            else {
                e.Cancel = false;
                hf_creatingNewUser.Value = "true";
            }
        }
    }
    protected void RegisterUser_Continue(object sender, EventArgs e) {
        ServerSettings.PageToolViewRedirect(this.Page, "UserAccounts.aspx");
    }

    #endregion


    #region Edit Buttons

    protected void hf_lockUser_Changed(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
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
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            string user = hf_deleteUser.Value.Trim();
            if ((user != _username) && (user.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                Membership.DeleteUser(user, true);
                MemberDatabase m = new MemberDatabase(user);
                m.DeleteUserCustomizations(user);
            }
        }
        hf_deleteUser.Value = string.Empty;

        BuildUsers();

        MembershipUserCollection coll = Membership.GetAllUsers();
        string messageCount = "<b class='pad-right'>Total Users</b>" + (coll.Count - 1).ToString();
        string script = "$('#MainContent_lbl_totalUsers').html(\"" + messageCount + "\");";

        RegisterPostbackScripts.RegisterStartupScript(this, script);
    }
    protected void hf_refreshList_Changed(object sender, EventArgs e) {
        BuildUsers();
        hf_refreshList.Value = string.Empty;
    }
    protected void hf_resetPassword_Changed(object sender, EventArgs e) {
        string user = hf_resetPassword.Value.Trim();
        try {
            string str = "<div class='clearMargin'><h3>Set new password for <b>" + user + "</b></h3></div>";
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
        msu.UpdateEmailUponLogin(_username, false);
        BuildUsers();
        hf_emailUser.Value = string.Empty;
    }
    protected void hf_noemailUser_Changed(object sender, EventArgs e) {
        var msu = new MemberDatabase(hf_noemailUser.Value);
        msu.UpdateEmailUponLogin(_username, true);
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

        string role = _ss.UserSignUpRole;
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
        if (_ss.EmailSystemStatus) {
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
        ServerSettings.update_SignInWithGoogle(true);
    }
    protected void rb_googlePlusSignIn_off_Checked(object sender, EventArgs e) {
        rb_googlePlusSignIn_on.Checked = false;
        rb_googlePlusSignIn_off.Checked = true;
        ServerSettings.update_SignInWithGoogle(false);
    }

    protected void rb_twitterSignIn_on_Checked(object sender, EventArgs e) {
        rb_twitterSignIn_on.Checked = true;
        rb_twitterSignIn_off.Checked = false;
        ServerSettings.update_SignInWithTwitter(true);
    }
    protected void rb_twitterSignIn_off_Checked(object sender, EventArgs e) {
        rb_twitterSignIn_on.Checked = false;
        rb_twitterSignIn_off.Checked = true;
        ServerSettings.update_SignInWithTwitter(false);
    }

    protected void rb_facebookSignIn_on_Checked(object sender, EventArgs e) {
        rb_facebookSignIn_on.Checked = true;
        rb_facebookSignIn_off.Checked = false;
        ServerSettings.update_SignInWithFacebook(true);
    }
    protected void rb_facebookSignIn_off_Checked(object sender, EventArgs e) {
        rb_facebookSignIn_on.Checked = false;
        rb_facebookSignIn_off.Checked = true;
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

    #endregion

}