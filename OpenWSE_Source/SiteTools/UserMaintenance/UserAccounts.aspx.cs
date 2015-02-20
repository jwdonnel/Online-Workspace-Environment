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
            Page.Response.Redirect("~/Default.aspx");
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
                    }

                    string ctlId = sm.AsyncPostBackSourceElementID;
                    _ctrlName = ctlId;

                    BuildRolesDropDownList(IsPostBack);

                    if ((!IsPostBack) || ((_ctrlName != "hf_lockUser") && (_ctrlName != "hf_deleteUser")
                        && (_ctrlName != "hf_refreshList") && (_ctrlName != "hf_resetPassword")))
                        BuildUsers();
                }

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

    #endregion


    #region User Properties

    private void BuildUsers() {
        HttpCookie myCookie = new HttpCookie("ddl_pageSize_useraccounts");

        pnl_Users.Controls.Clear();

        string tbSearch = tb_search.Text.Trim().ToLower();

        MembershipUserCollection coll = Membership.GetAllUsers();
        IEnumerable<MembershipUser> sortedList;

        if (ViewState["SortBy"] == null) {
            ViewState["SortBy"] = "username";
        }

        if (ViewState["SortDirection"] == null) {
            ViewState["SortDirection"] = "asc";
        }

        switch (ViewState["SortBy"].ToString()) {
            case "username":
                if (ViewState["SortDirection"].ToString() == "asc") {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderBy(a => a.UserName);
                }
                else {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderByDescending(a => a.UserName);
                }
                break;
            case "email":
                if (ViewState["SortDirection"].ToString() == "asc") {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderBy(a => a.Email);
                }
                else {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderByDescending(a => a.Email);
                }
                break;
            case "date":
                if (ViewState["SortDirection"].ToString() == "asc") {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderBy(a => a.CreationDate);
                }
                else {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderByDescending(a => a.CreationDate);
                }
                break;
            default:
                if (ViewState["SortDirection"].ToString() == "asc") {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderBy(a => a.UserName);
                }
                else {
                    sortedList = coll.Cast<MembershipUser>().ToList().OrderByDescending(a => a.UserName);
                }
                break;
        }

        lbl_totalUsers.Text = "<b class='pad-right'>Total Users</b>" + (coll.Count - 1).ToString();

        if (Request.Cookies["ddl_pageSize_useraccounts"] != null) {
            myCookie = Request.Cookies["ddl_pageSize_useraccounts"];
        }

        if ((string.IsNullOrEmpty(myCookie.Value)) || (IsPostBack)) {
            myCookie.Value = ddl_pageSize.SelectedValue;
        }
        myCookie.Expires = DateTime.Now.AddDays(30);
        Response.Cookies.Add(myCookie);

        int _pageSize = 3;
        if (!IsPostBack) {
            for (int i = 0; i < ddl_pageSize.Items.Count; i++) {
                if (ddl_pageSize.Items[i].Value == myCookie.Value) {
                    ddl_pageSize.SelectedIndex = i;
                    break;
                }
            }
            int.TryParse(myCookie.Value, out _pageSize);
        }
        else {
            if ((tbSearch == "search users") || (string.IsNullOrEmpty(tbSearch))) {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddlPagesize_holder').show();");
                int.TryParse(myCookie.Value, out _pageSize);
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddlPagesize_holder').hide();");
                _pageSize = 0;
            }
        }

        int _pageNm = 1;
        int _count = 0;
        string page = Request.QueryString["page"];
        if (!string.IsNullOrEmpty(page)) {
            int.TryParse(page, out _count);
            int.TryParse(page, out _pageNm);
        }

        if (((_count - 1) * _pageSize) > coll.Count - 1) {
            pnl_Users.Controls.Add(new LiteralControl("Page index is out of range. <a href='UserAccounts.aspx?page=1'>Click here</a> to return to page 1."));
            return;
        }

        if ((_count > 1) && (_pageSize != 0)) {
            _count = (_pageSize * _count) - _pageSize;
        }
        else {
            _count = 0;
        }

        int userCount = 0;
        bool chatEnabled = _ss.ChatEnabled;
        StringBuilder strUsers = new StringBuilder();

        int ii = 0;
        foreach (MembershipUser u in sortedList) {
            if (u != null) {
                if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    if (tbSearch.ToLower() == "me") {
                        tbSearch = _username;
                    }
                    if ((tbSearch == "search users") || (string.IsNullOrEmpty(tbSearch)) || (u.UserName.ToLower().Contains(tbSearch.ToLower()))) {
                        if ((userCount >= _pageSize) && (_pageSize != 0)) {
                            break;
                        }

                        MemberDatabase member = new MemberDatabase(u.UserName);

                        if (ii < _count) {
                            ii++;
                            continue;
                        }

                        strUsers.Append("<div class='table-settings-box contact-card-main'>");

                        strUsers.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
                        strUsers.Append("<tr>");

                        #region -- Username / Color / Role --

                        strUsers.Append("<td valign='top' style='min-width: 250px;'>");

                        string userMe = string.Empty;
                        if (u.UserName.ToLower() == _username.ToLower())
                            userMe = " - <small><i>You</i></small>";

                        string adminPages = string.Empty;
                        string userRole = string.Empty;
                        string[] userRoles = Roles.GetRolesForUser(u.UserName);
                        foreach (string role in userRoles) {
                            userRole += role + ",";
                        }

                        if (userRole[userRole.Length - 1] == ',') {
                            userRole = userRole.Remove(userRole.Length - 1);
                        }

                        if (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) {
                            adminPages = GetAdminPages(member);
                        }

                        strUsers.Append(UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 42));
                        strUsers.Append("<div class='float-left pad-left'><h2 class='float-left username'>" + u.UserName + userMe + "</h2>");
                        strUsers.Append("<div class='clear-space-five'></div>");
                        strUsers.Append("<h4>" + HelperMethods.MergeFMLNames(member) + "</h4>");
                        strUsers.Append("</div>");

                        strUsers.Append("</td>");

                        #endregion

                        #region -- Receive All / Chat Client --

                        strUsers.Append("<td valign='top' style='width: 250px;'>");

                        string receiveAll = "<img alt='disabled' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/false.png") + "' class='float-left pad-right-sml pad-top-sml' title='Disabled' />eRequests/Questions";
                        string userChat = "<img alt='disabled' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/false.png") + "' class='float-left pad-right-sml pad-top-sml' title='Disabled' />Chat Client";
                        if (member.ReceiveAll)
                            receiveAll = "<img alt='enabled' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/true.png") + "' class='float-left pad-right-sml pad-top-sml' title='Enabled' />eRequests/Questions";
                        if (member.ChatEnabled)
                            userChat = "<img alt='enabled' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/true.png") + "' class='float-left pad-right-sml pad-top-sml' title='Enabled' />Chat Client";
                        if (chatEnabled) {
                            strUsers.Append(userChat);
                            strUsers.Append("<div class='clear-space-five'></div>");
                        }
                        strUsers.Append(receiveAll);
                        strUsers.Append("<div class='clear-space-five'></div>");
                        strUsers.Append("<div class='float-left' style='width: 12px; height: 12px;'></div><span>" + userRole + " User</span>");
                        strUsers.Append("</td>");

                        #endregion

                        #region -- Summary 1 --

                        string ipAddress = member.IpAddress;
                        if (string.IsNullOrEmpty(ipAddress)) {
                            ipAddress = "n/a";
                        }

                        strUsers.Append("<td valign='top' style='width: 350px;'>");
                        strUsers.Append("<span class='float-left settings-name-column' style='padding-top: 0px!important;'>Date Joined</span>" + u.CreationDate.ToShortDateString() + "<div class='clear-space-five'></div>");
                        strUsers.Append("<span class='float-left settings-name-column' style='padding-top: 0px!important;'>Last Login</span>" + u.LastLoginDate.ToString() + "<div class='clear-space-five'></div>");
                        strUsers.Append("<span class='float-left settings-name-column' style='padding-top: 0px!important;'>Ip Address</span>" + ipAddress);
                        strUsers.Append("<div class='clear'></div>");
                        strUsers.Append("</td>");

                        #endregion

                        #region -- Summary 2 --

                        strUsers.Append("<td valign='top' style='width: 325px;'>");
                        strUsers.Append("<span class='float-left settings-name-column' style='padding-top: 0px!important;'>Email</span>" + u.Email + "<div class='clear-space-five'></div>");
                        strUsers.Append("<span class='float-left settings-name-column' style='padding-top: 0px!important;'>Workspace Mode</span>" + member.WorkspaceMode.ToString());
                        strUsers.Append("<div class='clear'></div>");
                        strUsers.Append("</td>");

                        #endregion

                        #region -- Buttons --

                        string appUser = "<a href='#' title='Edit User Apps' class='img-app-dark pad-all-sml margin-right' onclick='openWSE.LoadIFrameContent(\"SiteTools/iframes/UsersAndApps.aspx?u=" + u.UserName + "\", this);return false;'></a>";
                        string unlockUser = "<a href='#' title='Unlock User' class='img-unlock pad-all-sml margin-right RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
                        string lockUser = "<a href='#' title='Lock User' class='img-lock pad-all-sml margin-right RandomActionBtns' onclick='LockUser(\"" + u.UserName + "\");return false;'></a>";
                        string deleteUser = "<a href='#' title='Delete User' class='td-delete-btn margin-right' onclick='DeleteUser(\"" + u.UserName + "\");return false;'></a>";
                        string editUser = "<a href='#' title='Edit User' class='td-edit-btn margin-right' onclick='openWSE.LoadIFrameContent(\"SiteTools/UserMaintenance/AcctSettings.aspx?toolView=true&u=" + u.UserName + "\", this);return false;'></a>";
                        string password = string.Empty;
                        if (!member.IsSocialAccount) {
                            password = "<a href='#' title='Reset User Password' class='img-password pad-all-sml margin-right' onclick='ResetPassword(\"" + u.UserName + "\");return false;'></a>";
                        }

                        string mail = "<a href='#' title='Email me on login' class='img-mail pad-all-sml margin-right' onclick='EmailUser(\"" + u.UserName + "\");return false;'></a>";
                        string nomail = "<a href='#' title='Dont email me on login' class='img-nomail pad-all-sml margin-right' onclick='CancelEmailUser(\"" + u.UserName + "\");return false;'></a>";

                        if (!_ss.EmailSystemStatus) {
                            mail = string.Empty;
                            nomail = string.Empty;
                        }

                        strUsers.Append("<td align='right' valign='top' style='width: 120px;'>");

                        if (u.UserName.ToLower() != _username.ToLower()) {
                            if (member.EmailUponLoginList.Any(un => un.ToLower() == _username.ToLower()))
                                strUsers.Append(nomail);
                            else
                                strUsers.Append(mail);
                        }

                        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                            strUsers.Append(password);
                            if ((u.IsLockedOut) || (!u.IsApproved))
                                strUsers.Append(unlockUser);
                            else
                                strUsers.Append(lockUser);

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
                                            strUsers.Append(unlockUser);
                                        else
                                            strUsers.Append(lockUser);
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
                        strUsers.Append("</td>");

                        #endregion

                        strUsers.Append("</tr></tbody></table></div>");
                        userCount++;
                        ii++;
                    }
                }
            }
        }

        if (userCount > 0) {
            pnl_Users.Controls.Add(new LiteralControl(strUsers.ToString() + "<div class='clear-space'></div>"));
            if (_pageSize != 0) {
                StringBuilder strPaging = new StringBuilder();
                if (_pageNm > 1) {
                    strPaging.Append("<a href='UserAccounts.aspx?page=" + (_pageNm - 1) + "' class='float-left pad-right' onclick='openWSE.LoadingMessage1(\"Loading. Please Wait...\");'><div class='pg-prev-btn float-left' style='padding: 0px 5px 0px 0px!important;'></div>Previous Page</a>");
                }

                if ((_pageNm * _pageSize) < coll.Count - 1) {
                    strPaging.Append("<a href='UserAccounts.aspx?page=" + (_pageNm + 1) + "' class='float-right pad-left' onclick='openWSE.LoadingMessage1(\"Loading. Please Wait...\");'><div class='pg-next-btn float-right' style='padding: 0px 0px 0px 5px!important;'></div>Next Page</a>");
                }

                pnl_Users.Controls.Add(new LiteralControl(strPaging.ToString()));
            }
        }
        else {
            pnl_Users.Controls.Add(new LiteralControl("<h3 class='pad-all'>No users found</h3>"));
        }
    }
    private static string GetAdminPages(MemberDatabase member) {
        var str = new StringBuilder();
        str.Append("<table style='width: 100%;' cellpadding='0' cellspacing='0'><tbody><tr>");
        str.Append("<td valign='top' align='left' style='width: 100px;'><h4 class='font-bold pad-bottom-sml'>Admin Pages:</h4></td>");
        str.Append("<td valign='top' align='left'>");
        string[] adminpages = member.AdminPagesList;
        foreach (string page in adminpages) {
            str.Append("<div class='float-left pad-bottom-sml' style='width: 170px;'>" + page + "</div>");
        }

        if (adminpages.Length == 0)
            str.Append("<div class='float-left'>No Access to Admin Pages</div>");

        str.Append("</td></tr></tbody></table>");
        return str.ToString();
    }
    private string LoadAppIcons(string u) {
        int count = 0;
        var str = new StringBuilder();
        str.Append("<table style='width: 100%;' cellpadding='0' cellspacing='0'><tbody><tr>");
        str.Append("<td valign='top' align='left' style='width: 100px;'><h4 class='font-bold pad-bottom-sml'>Apps:</h4></td>");
        str.Append("<td valign='top' align='left'>");
        var member = new MemberDatabase(u);
        _apps.GetUserInstalledApps();
        foreach (string w in member.EnabledApps.Where(w => _apps.IconExists(w))) {
            string name = _apps.GetAppName(w);
            str.Append("<div class='float-left pad-bottom-sml' style='width: 170px;'>" + name + "</div>");
            count++;
        }

        if (count == 0)
            str.Append("<div class='float-left'>No apps installed for user</div>");

        str.Append("</td></tr></tbody></table>");
        return str.ToString();
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
                    }
                }
            }
        }
    }
    protected void btn_closepwreset_Click(object sender, EventArgs e) {
        pwreset_overlay.Attributes["style"] = "visibility: hidden; display: none;";
    }
    protected void ddl_pageSize_Changed(object sender, EventArgs e) {
        BuildUsers();
    }
    protected void hf_clearsearch_Changed(object sender, EventArgs e) {

    }
    protected void ddl_sort_Changed(object sender, EventArgs e) {
        switch (ddl_sortby.SelectedValue) {
            case "1":
                ViewState["SortBy"] = "username";
                ViewState["SortDirection"] = "asc";
                break;
            case "2":
                ViewState["SortBy"] = "username";
                ViewState["SortDirection"] = "desc";
                break;
            case "3":
                ViewState["SortBy"] = "email";
                ViewState["SortDirection"] = "asc";
                break;
            case "4":
                ViewState["SortBy"] = "email";
                ViewState["SortDirection"] = "desc";
                break;
            case "5":
                ViewState["SortBy"] = "date";
                ViewState["SortDirection"] = "asc";
                break;
            case "6":
                ViewState["SortBy"] = "date";
                ViewState["SortDirection"] = "desc";
                break;
        }

        BuildUsers();
    }

    #endregion


    #region Add User

    protected void hf_createMultiUsers_ValueChanged(object sender, EventArgs e) {
        string email = txt_multiUser_email.Text.Trim();
        string password = txt_multiUser_password.Text;
        string role = dd_role_multiUser.SelectedValue;

        string[] userList = hf_createMultiUsers.Value.Trim().Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string user in userList) {
            MembershipUser existingUser = Membership.GetUser(user);
            if (existingUser == null) {
                Membership.CreateUser(user, password, email);
                UserRegistration ur = new UserRegistration(user, role);

                string firstName = user;
                string lastName = "";

                string[] userNames = user.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (userNames.Length > 2) {
                    firstName = userNames[0];
                    lastName = userNames[userNames.Length - 1];
                }

                string color = ur.RandomColor();
                ur.RegisterNewUser(firstName, lastName, email, color);
                ur.RegisterDefaults();
            }
        }

        Response.Redirect(Request.RawUrl);
    }

    protected void btn_rebuild_uc_Clicked(object sender, EventArgs e) {
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
        string username = RegisterUser.UserName;
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

        RegisterPostbackScripts.RegisterStartupScript(this, script + "$('#NewUser-element').find('.Modal-element-modal').removeClass('add-user-modal');");
    }
    protected void RegisterUser_CreatingUser(object sender, LoginCancelEventArgs e) {
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            e.Cancel = true;
            hf_creatingNewUser.Value = "false";
        }
        else {
            e.Cancel = false;
            hf_creatingNewUser.Value = "true";
        }
    }
    protected void RegisterUser_Continue(object sender, EventArgs e) {
        hf_creatingNewUser.Value = "false";
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
            pwreset_overlay.Attributes["style"] = "visibility: visible; display: block;";
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
            NewUserDefaults userDefaults = new NewUserDefaults(ddl_roleNameSelect.SelectedValue);
            userDefaults.UpdateDefaults("Role", newRole);

            string[] usersInRole = Roles.GetUsersInRole(ddl_roleNameSelect.SelectedValue);
            Roles.CreateRole(newRole);
            if (usersInRole.Length > 0) {
                Roles.RemoveUsersFromRole(usersInRole, ddl_roleNameSelect.SelectedValue);
                Roles.AddUsersToRole(usersInRole, newRole);
            }
            Roles.DeleteRole(ddl_roleNameSelect.SelectedValue);

            BuildRolesDropDownList(false);

            BuildManagedRolesDDL();
            ShowHideManagedRoleButtons(false);

            lbtn_CreateNewRole.Enabled = true;
            lbtn_CreateNewRole.Visible = true;

            UpdatePanel22.Update();
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

        NewUserDefaults newDefaults = new NewUserDefaults(newRole);
        newDefaults.GetDefaults();

        Roles.CreateRole(newRole);

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

    #endregion

}