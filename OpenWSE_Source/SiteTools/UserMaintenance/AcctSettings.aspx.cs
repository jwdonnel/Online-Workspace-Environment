#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using Image = System.Drawing.Image;
using System.Net.Mail;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Overlays;

#endregion

public partial class SiteTools_AcctSettings : Page {

    #region View States

    private string Username {
        get {
            if (ViewState["Username"] != null && ViewState["Username"].ToString() != "") {
                return ViewState["Username"].ToString();
            }

            return string.Empty;
        }
        set {
            ViewState["Username"] = value;
        }
    }
    private string RoleSelect {
        get {
            if (ViewState["SelectedRole"] != null && ViewState["SelectedRole"].ToString() != "") {
                return ViewState["SelectedRole"].ToString();
            }

            return ServerSettings.AdminUserName;
        }
        set {
            ViewState["SelectedRole"] = value;
        }
    }
    private Dictionary<string, string> DrDefaults {
        get {
            if (ViewState["DrDefaults"] == null) {
                ViewState["DrDefaults"] = new Dictionary<string, string>();
            }

            return ViewState["DrDefaults"] as Dictionary<string, string>;
        }
        set {
            ViewState["DrDefaults"] = value;
        }
    }
    private bool IsIframe {
        get {
            if (ViewState["IsIframe"] != null && ViewState["IsIframe"].ToString() != "") {
                return Convert.ToBoolean(ViewState["IsIframe"].ToString());
            }

            return false;
        }
        set {
            ViewState["IsIframe"] = value;
        }
    }

    #endregion


    #region Private Variables

    private StringBuilder _strJSRegister = new StringBuilder();
    private Notifications _notifications = new Notifications();
    private ServerSettings _ss = new ServerSettings();
    private WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private string _siteTheme = "Standard";
    private bool AssociateWithGroups = false;
    private MemberDatabase Member {
        get {
            return new MemberDatabase(Username);
        }
    }
    private MembershipUser UserMembership {
        get {
            return Membership.GetUser(Username);
        }
    }
    private NewUserDefaults NewDefaults;

    #endregion


    #region Page Load and Load User Settings

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            string controlName = HttpContext.Current.Request.Params["__EVENTTARGET"];
            if (string.IsNullOrEmpty(controlName)) {
                controlName = string.Empty;
            }
            LoadSettingsPage(controlName);
        }
    }

    private void LoadSettingsPage(string controlName) {
        if (!IsPostBack || controlName.Contains("hf_UpdateAll")) {
            IIdentity userId = HttpContext.Current.User.Identity;

            #region Load Settings
            if (Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                if (!string.IsNullOrEmpty(Request.QueryString["u"])) {
                    switch (Request.QueryString["u"].ToLower()) {
                        case "newuserdefaults":
                            LoadNewUserDefaults();
                            break;
                        case "demouser":
                            if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                LoadDemoUserSettings();
                            }
                            else {
                                Page.Response.Redirect("~/ErrorPages/Blocked.html");
                            }
                            break;
                        default:
                            LoadUserCustomize(Request.QueryString["u"]);
                            break;
                    }
                }
                else {
                    LoadAllUserSettings(userId.Name);
                }
            }
            else if (ServerSettings.AdminPagesCheck("AcctSettings", userId.Name)) {
                if (!string.IsNullOrEmpty(Request.QueryString["u"])) {
                    if ((userId.Name.ToLower() == Request.QueryString["u"].ToLower()) || (ServerSettings.AdminPagesCheck("UserAccounts", userId.Name))) {
                        LoadUserCustomize(Request.QueryString["u"]);
                    }
                    else {
                        LoadAllUserSettings(userId.Name);
                    }
                }
                else {
                    LoadAllUserSettings(userId.Name);
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
            #endregion

            LoadUserInformation();
            LoadNotificationsSettings();
            LoadWorkspaceOverlays();
            LoadBackgroundEditor();
            LoadTopSideMenuBar();
            LoadIconSelector();
            LoadSiteCustomizations();
            LoadWorkspaceContainer();
            LoadChatClient();

        }
        else {
            InitNewDefaults();
        }
    }

    private void InitNewDefaults() {
        if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
            if (!string.IsNullOrEmpty(Request.QueryString["u"])) {
                switch (Request.QueryString["u"].ToLower()) {
                    case "newuserdefaults":
                        NewDefaults = new NewUserDefaults(RoleSelect);
                        break;
                    case "demouser":
                        if (HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                            NewDefaults = new NewUserDefaults(RoleSelect);
                        }
                        break;
                }
            }
        }
    }

    private void LoadAllUserSettings(string userName) {
        Username = userName;

        _siteTheme = Member.SiteTheme;
        if (string.IsNullOrEmpty(_siteTheme)) {
            _siteTheme = "Standard";
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe_changepassword_holder').html(\"<iframe src='../iframes/ChangePassword.aspx' frameborder='0' height='265px' width='320px' scrolling='auto'></iframe>\");");
        AssociateWithGroups = _ss.AssociateWithGroups;

        btn_markasnewuser.Enabled = false;
        btn_markasnewuser.Visible = false;

        if (Roles.IsUserInRole(userName, ServerSettings.AdminUserName)) {
            pnl_EnableRecieveAll.Enabled = true;
            pnl_EnableRecieveAll.Visible = true;
        }

        if (userName.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            AdminVersion();
            pnl_WorkspaceMode.Enabled = false;
            pnl_WorkspaceMode.Visible = false;
        }
        else {
            pnl_WorkspaceMode.Enabled = true;
            pnl_WorkspaceMode.Visible = true;
        }

        if (Member.IsSocialAccount) {
            pnl_passwordchange.Enabled = false;
            pnl_passwordchange.Visible = false;
            pnl_isSocialAccount.Enabled = true;
            pnl_isSocialAccount.Visible = true;
        }
    }
    private void LoadUserCustomize(string userName) {
        string pageTitle = userName + " Customizations";

        HideAllSiteMasterControls();

        string roles = "";
        int count = 0;
        for (int i = 0; i < Roles.GetRolesForUser(userName).Length; i++) {
            roles += Roles.GetRolesForUser(userName)[i];
            if (i < Roles.GetRolesForUser(userName).Length - 1) {
                roles += ", ";
            }
        }
        pageTitle = userName + " Customizations: " + roles + " Role";

        lbl_pageTitle.Text = pageTitle;
        pnl_topbackgroundTitleBar.Visible = true;
        pnl_WorkspaceMode.Enabled = true;
        pnl_WorkspaceMode.Visible = true;

        Username = userName;

        if (Roles.IsUserInRole(userName, ServerSettings.AdminUserName)) {
            RoleSelect = ServerSettings.AdminUserName;
            pnl_adminpages_Holder.Enabled = false;
            pnl_adminpages_Holder.Visible = false;
        }
        else {
            string[] roleList = Roles.GetRolesForUser(userName);
            if (roleList.Length == 0) {
                RoleSelect = "Standard";
            }
            else {
                RoleSelect = roleList[0];
            }
        }

        if (!Member.IsNewMember || (HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower() && !Member.IsNewMember)) {
            btn_markasnewuser.Enabled = true;
            btn_markasnewuser.Visible = true;
        }
        else {
            btn_markasnewuser.Enabled = false;
            btn_markasnewuser.Visible = false;
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == userName.ToLower()) {
            pnl_userRoleAssign.Visible = false;
            pnl_userRoleAssign.Enabled = false;
            btn_markasnewuser.Enabled = false;
            btn_markasnewuser.Visible = false;
        }
        else if ((HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) || ((Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) && (RoleSelect.ToLower() != ServerSettings.AdminUserName.ToLower()))) {
            pnl_userRoleAssign.Visible = true;
            pnl_userRoleAssign.Enabled = true;
        }
        else {
            pnl_userRoleAssign.Visible = false;
            pnl_userRoleAssign.Enabled = false;
        }

        _siteTheme = Member.SiteTheme;
        if (string.IsNullOrEmpty(_siteTheme)) {
            _siteTheme = "Standard";
        }

        if ((UserMembership == null) || (userName.ToLower() == ServerSettings.AdminUserName.ToLower())) {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower() && !Member.IsSocialAccount) {
            pnl_passwordchange.Enabled = true;
            pnl_passwordchange.Visible = true;
            _strJSRegister.Append("$('#iframe_changepassword_holder').html(\"<iframe src='../iframes/ChangePassword.aspx' frameborder='0' height='265px' width='320px' scrolling='auto'></iframe>\");");
        }
        else {
            pnl_passwordchange.Enabled = false;
            pnl_passwordchange.Visible = false;
        }

        pnl_EnableRecieveAll.Enabled = true;
        pnl_EnableRecieveAll.Visible = true;

        AssociateWithGroups = _ss.AssociateWithGroups;
        if (userName.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            AdminVersion();
        }

        RegisterPostbackScripts.RegisterStartupScript(this, _strJSRegister.ToString());
    }
    private void LoadNewUserDefaults() {
        HideAllSiteMasterControls();

        bool SiteTools = false;
        if (Request.QueryString.Count > 0) {
            if (!string.IsNullOrEmpty(Request.QueryString["NoRegistration"])) {
                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["NoRegistration"])) {
                    SiteTools = true;
                }
            }
        }

        if (SiteTools) {
            lbl_pageTitle.Text = "New User Defaults for " + _ss.UserSignUpRole;
            RoleSelect = _ss.UserSignUpRole;
            pnl_userRoleAssign.Enabled = false;
            pnl_userRoleAssign.Visible = false;
        }
        else {
            lbl_pageTitle.Text = "New User Defaults for " + RoleSelect;
            RoleSelect = dd_roles.SelectedValue;
            pnl_userRoleAssign.Enabled = true;
            pnl_userRoleAssign.Visible = true;
        }

        InitNewDefaults();
        NewDefaults.GetDefaults();
        DrDefaults = NewDefaults.DefaultTable;

        lbl_DateUpdated.Text = "Last Updated: " + DrDefaults["DateUpdated"];

        #region Disable/Hide Panels
        pnl_passwordchange.Enabled = false;
        pnl_passwordchange.Visible = false;
        pnl_WorkspaceMode.Enabled = true;
        pnl_WorkspaceMode.Visible = true;
        pnl_acctImage.Enabled = false;
        pnl_acctImage.Visible = false;
        pnl_EnableRecieveAll.Enabled = true;
        pnl_EnableRecieveAll.Visible = true;
        pnl_demoPackage.Enabled = true;
        pnl_demoPackage.Visible = true;
        btn_markasnewuser.Enabled = false;
        btn_markasnewuser.Visible = false;
        pnl_WorkspaceOverlays.Enabled = false;
        pnl_WorkspaceOverlays.Visible = false;
        pnl_usercreds.Visible = false;
        pnl_userColor.Visible = false;
        pnl_NotificationSettings.Enabled = false;
        pnl_NotificationSettings.Visible = false;
        pnl_accountPrivacy.Enabled = false;
        pnl_accountPrivacy.Visible = false;
        pnl_clearUserProp.Enabled = false;
        pnl_clearUserProp.Visible = false;
        pnl_clearNoti.Enabled = false;
        pnl_clearNoti.Visible = false;
        #endregion

        h3_setuserinfo.InnerHtml = "New User Information";

        if (RoleSelect.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            pnl_adminpages_Holder.Enabled = false;
            pnl_adminpages_Holder.Visible = false;
        }
    }
    private void LoadDemoUserSettings() {
        HideAllSiteMasterControls();

        lbl_pageTitle.Text = "No Login/Demo Customizations";
        RoleSelect = "DemoNoLogin";

        InitNewDefaults();
        NewDefaults.GetDefaults();

        #region Disable/Hide Panels
        pnl_userRoleAssign.Enabled = false;
        pnl_userRoleAssign.Visible = false;
        pnl_WorkspaceMode.Enabled = true;
        pnl_WorkspaceMode.Visible = true;
        btn_markasnewuser.Enabled = false;
        btn_markasnewuser.Visible = false;
        pnl_UserInformation.Enabled = false;
        pnl_UserInformation.Visible = false;
        pnl_NotificationSettings.Enabled = false;
        pnl_NotificationSettings.Visible = false;
        pnl_WorkspaceOverlays.Enabled = false;
        pnl_WorkspaceOverlays.Visible = false;
        pnl_ChatClient.Enabled = false;
        pnl_ChatClient.Visible = false;
        pnl_accountPrivacy.Enabled = false;
        pnl_accountPrivacy.Visible = false;
        pnl_clearproperties.Enabled = false;
        pnl_clearproperties.Visible = false;
        pnl_clearUserProp.Enabled = false;
        pnl_clearUserProp.Visible = false;
        pnl_demoPackage.Enabled = true;
        pnl_demoPackage.Visible = true;
        pnl_loadLinksOnNewPage.Enabled = false;
        pnl_loadLinksOnNewPage.Visible = false;
        pnl_clearNoti.Enabled = false;
        pnl_clearNoti.Visible = false;
        #endregion

        if (NewDefaults.DefaultTable.Count > 0) {
            DrDefaults = NewDefaults.DefaultTable;
            lbl_DateUpdated.Text = "Last Updated: " + DrDefaults["DateUpdated"];
        }
    }

    private void AdminVersion() {
        pnl_ChatClient.Enabled = false;
        pnl_ChatClient.Visible = false;
        pnl_WorkspaceContainer.Enabled = false;
        pnl_WorkspaceContainer.Visible = false;
        pnl_SiteCustomizations.Enabled = false;
        pnl_SiteCustomizations.Visible = false;
        pnl_WorkspaceOverlays.Enabled = false;
        pnl_WorkspaceOverlays.Visible = false;
        pnl_IconSelector.Enabled = false;
        pnl_IconSelector.Visible = false;
        pnl_BackgroundEditor.Enabled = true;
        pnl_BackgroundEditor.Visible = true;
        pnl_nonadminsettings.Enabled = false;
        pnl_nonadminsettings.Visible = false;
        pnl_groupEditor.Enabled = false;
        pnl_groupEditor.Visible = false;
        pnl_adminpages_Holder.Enabled = false;
        pnl_adminpages_Holder.Visible = false;
        pnl_userRoleAssign.Enabled = false;
        pnl_userRoleAssign.Visible = false;
    }
    private void HideAllSiteMasterControls() {
        HtmlGenericControl app_title_bg_master = (HtmlGenericControl)Master.FindControl("app_title_bg");
        if (app_title_bg_master != null) {
            app_title_bg_master.Visible = false;
        }

        pnl_topbackgroundTitleBar.Visible = true;
        IsIframe = true;

        StringBuilder str = new StringBuilder();
        str.Append("$('#always-visible, #app_title_bg, #container-footer').hide();");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    private void UpdateDrDefaults() {
        if (NewDefaults != null) {
            NewDefaults.GetDefaults();
            DrDefaults = NewDefaults.DefaultTable;
        }
    }

    #endregion


    #region -- User Information --

    private void LoadUserInformation() {
        if (pnl_UserInformation.Enabled && pnl_UserInformation.Visible) {
            if (!string.IsNullOrEmpty(Member.Username)) {
                LoadUserNameEmail();
                LoadAcctImage();
                SetUserImage();

                txt_userColor.Text = Member.UserColor;
            }

            if (!Roles.IsUserInRole(Username, ServerSettings.AdminUserName)) {
                BuildAdminPageEditor();
            }
            if (ServerSettings.AdminPagesCheck("GroupOrg", Username) || Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                BuildGroupsEditor();
            }
            ReceiveAllSettings();
            BuildRolesDropDownList();
            AssignRolesDropDownList();

            updatepnl_UserInformation2.Update();
        }
    }

    private void LoadUserNameEmail() {
        tb_firstname_accountsettings.Text = Member.FirstName;
        tb_lastname_accountsettings.Text = Member.LastName;

        if (!string.IsNullOrEmpty(UserMembership.Email)) {
            if (!IsPostBack)
                tb_email.Text = UserMembership.Email;
        }

        updatepnl_UserInformation1.Update();
    }
    private void LoadAcctImage() {
        string acctImg = Member.AccountImage;
        if (!string.IsNullOrEmpty(acctImg)) {
            if (acctImg.ToLower().Contains("http") || acctImg.ToLower().Contains("www.")) {
                imgAcctImage.ImageUrl = acctImg;
            }
            else {
                imgAcctImage.ImageUrl = ServerSettings.AccountImageLoc + Member.UserId + "/" + acctImg;
            }
        }
    }
    private void SetUserImage() {
        string acctImg = Member.AccountImage;
        StringBuilder _strScriptreg = new StringBuilder();
        string mergedName = HelperMethods.MergeFMLNames(Member);
        _strScriptreg.Append("$('#lbl_UserName').html(\"" + UserImageColorCreator.CreateImgColorTopBar(acctImg, Member.UserColor, Member.UserId, mergedName, _siteTheme) + "\");");
        if (!string.IsNullOrEmpty(acctImg)) {
            _strScriptreg.Append("$('#img_Profile').attr('src', '" + ResolveUrl(ServerSettings.AccountImageLoc + Member.UserId + "/" + acctImg) + "');$('#img_Profile').show();");
        }
        else {
            _strScriptreg.Append("$('#img_Profile').attr('src', '" + ResolveUrl("~/Standard_Images/EmptyUserImg.png") + "');$('#img_Profile').show();");
        }
        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
    }
    private void BuildAdminPageEditor() {
        if (RoleSelect.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            pnl_adminpages_Holder.Enabled = true;
            pnl_adminpages_Holder.Visible = true;

            pnl_adminpages.Controls.Clear();
            var str = new StringBuilder();

            string[] adminPageListRegister = { };

            if (DrDefaults.Count > 0) {
                adminPageListRegister = DrDefaults["UserSignUpAdminPages"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                adminPageListRegister = Member.AdminPagesList;

            List<string> adminPageList = ServerSettings.AdminPages();

            foreach (string page in adminPageList) {
                if (page.ToLower() == "licensemanager") {
                    continue;
                }

                if (adminPageListRegister.Contains(page)) {
                    str.Append("<div class='float-left pad-all adminpageedit'>");
                    str.Append("<div class='float-left pad-top-sml'>" + page);
                    str.Append("<a href='#' onclick='removeAdminPage(\"" + page + "\");return false;' class='RandomActionBtns float-left' title='Remove " + page + "'>");
                    str.Append("<span class='img-collapse-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 2px;'></span></a>");
                    str.Append("</div></div>");
                }
                else {
                    str.Append("<div class='float-left pad-all adminpageedit'>");
                    str.Append("<div class='float-left pad-top-sml'>" + page);
                    str.Append("<a href='#' onclick='addAdminPage(\"" + page + "\");return false;' class='RandomActionBtns float-left' title='Add " + page + "'>");
                    str.Append("<span class='img-expand-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 2px;'></span></a>");
                    str.Append("</div></div>");
                }
            }

            pnl_adminpages.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_adminpages_Holder.Enabled = false;
            pnl_adminpages_Holder.Visible = false;
        }
    }
    private void ReceiveAllSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ReceiveAll"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ReceiveAll) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_receiveall_off.Checked = false;
            rb_receiveall_on.Checked = true;
        }
        else {
            rb_receiveall_off.Checked = true;
            rb_receiveall_on.Checked = false;
        }
    }
    private void BuildRolesDropDownList() {
        if (dd_roles.Items.Count == 0) {
            List<string> listOfRoles = MemberDatabase.GetListOfAvailableRoles();
            dd_roles.Items.Clear();
            foreach (string roleName in listOfRoles) {
                dd_roles.Items.Add(new ListItem(roleName, roleName));
            }

            if (dd_roles.Items.Count > 0) {
                dd_roles.SelectedIndex = 0;
            }
        }
    }
    private void AssignRolesDropDownList() {
        int count = 0;
        foreach (ListItem item in dd_roles.Items) {
            if (item.Value == RoleSelect) {
                dd_roles.SelectedIndex = count;
                break;
            }
            count++;
        }
    }

    #region Build Groups

    private void BuildGroupsEditor() {
        pnl_groups.Controls.Clear();

        if (Session["LoginGroup"] != null) {
            try {
                string sessionGroup = Session["LoginGroup"].ToString();
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    pnl_groupEditor.Enabled = false;
                    pnl_groupEditor.Visible = false;
                    return;
                }
            }
            catch { }
        }

        var str = new StringBuilder();

        string[] groupsRegister = { };

        if (DrDefaults.Count > 0) {
            groupsRegister = DrDefaults["Groups"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        }
        else
            groupsRegister = Member.GroupList.ToArray();


        Groups groups = new Groups();
        groups.getEntries();
        List<Dictionary<string, string>> groupList = groups.group_dt;

        foreach (Dictionary<string, string> dr in groupList) {
            string g = dr["GroupName"];
            string gid = dr["GroupID"];

            bool inGroup = groupsRegister.Contains(gid);
            if (DrDefaults.Count == 0)
                inGroup = groupsRegister.Contains(gid);

            if (inGroup) {
                str.Append("<div class='float-left pad-all groupedit'>");
                str.Append("<div class='float-left pad-top-sml'>" + g);
                str.Append("<a href='#' onclick='removeGroup(\"" + gid + "\");return false;' class='RandomActionBtns float-left' title='Remove " + g + "'>");
                str.Append("<span class='img-collapse-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 2px;'></span></a>");
                str.Append("</div></div>");
            }
            else {
                str.Append("<div class='float-left pad-all groupedit'>");
                str.Append("<div class='float-left pad-top-sml'>" + g);
                str.Append("<a href='#' onclick='addGroup(\"" + gid + "\");return false;' class='RandomActionBtns float-left' title='Add " + g + "'>");
                str.Append("<span class='img-expand-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 2px;'></span></a>");
                str.Append("</div></div>");
            }
        }

        pnl_groups.Controls.Add(new LiteralControl(str.ToString()));
    }
    protected void hf_addGroup_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_addGroup.Value)) {
            string group = hf_addGroup.Value;

            if (DrDefaults.Count > 0) {
                string[] groupsRegister = DrDefaults["Groups"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                string groupList = "";
                foreach (string g in groupsRegister) {
                    groupList += g + ServerSettings.StringDelimiter;
                }
                groupList += group;
                NewDefaults.UpdateDefaults("Groups", groupList);
                UpdateDrDefaults();
            }
            else {
                string templist = BuildUserGroupList();
                string groupname = new Groups(Member.Username).GetGroupName_byID(group);
                if (!string.IsNullOrEmpty(groupname)) {
                    Member.UpdateGroupName(templist + group);
                }

                if (Member.Username.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                    SendNotification("<span style='color: #2F9E00;'>Added</span> to", groupname, Member.Username);

                if (string.IsNullOrEmpty(templist) && Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        hf_addGroup.Value = "";
        LoadUserInformation();
    }
    protected void hf_removeGroup_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_removeGroup.Value)) {
            string group = hf_removeGroup.Value;

            if (DrDefaults.Count > 0) {
                string[] groupsRegister = DrDefaults["Groups"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                string groupList = "";
                foreach (string g in groupsRegister) {
                    if (g != group) {
                        groupList += g + ServerSettings.StringDelimiter;
                    }
                }
                NewDefaults.UpdateDefaults("Groups", groupList);
                UpdateDrDefaults();
            }
            else {
                string templist = RemoveUserFromGroupList(group);
                string groupname = new Groups(Member.Username).GetGroupName_byID(group);

                templist = templist.Replace(group, "");
                templist = templist.Replace(ServerSettings.StringDelimiter + ServerSettings.StringDelimiter, "");

                if ((Member.Username.ToLower() != HttpContext.Current.User.Identity.Name.ToLower()) || (templist != "~") && (!string.IsNullOrEmpty(templist))) {
                    Member.UpdateGroupName(templist);

                    if (Member.Username.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                        SendNotification("<span style='color: #D80000;'>Removed</span> from", groupname, Member.Username);
                }
                else if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot remove you from " + groupname + ". You must be in at least 1 group.');");
                }
            }
        }

        hf_removeGroup.Value = "";
        LoadUserInformation();
    }
    private string BuildUserGroupList() {
        List<string> groups = Member.GroupList;

        return groups.Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
    }
    private string RemoveUserFromGroupList(string groupId) {
        string groupname = new Groups(Member.Username).GetGroupName_byID(groupId);
        List<string> groups = Member.GroupList;

        return groups.Where(t => t != groupname).Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
    }
    private void SendNotification(string action, string group, string user) {
        MailMessage mailTo = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<h3>Group Notification</h3><br />");
        MemberDatabase tempMember = new MemberDatabase(Username);

        messagebody.Append("<p>You have been " + action + " <b>" + group + "</b> by " + HelperMethods.MergeFMLNames(tempMember) + ".</p>");


        var un = new UserNotificationMessages(user);
        string email = un.attemptAdd("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString(), true);
        if (!string.IsNullOrEmpty(email))
            mailTo.To.Add(email);

        UserNotificationMessages.finishAdd(mailTo, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString());
    }

    #endregion

    protected void btn_updateinfo_Click(object sender, EventArgs e) {
        try {
            if (Member.FirstName != tb_firstname_accountsettings.Text)
                Member.UpdateFirstName(tb_firstname_accountsettings.Text);
            if (Member.LastName != tb_lastname_accountsettings.Text)
                Member.UpdateLastName(tb_lastname_accountsettings.Text);

            MembershipUser u = Membership.GetUser(Username);
            if (!string.IsNullOrEmpty(tb_email.Text.Trim())) {
                if (u != null) {
                    u.Email = tb_email.Text.Trim();
                    Membership.UpdateUser(u);
                }
            }
            else {
                if (u != null) {
                    u.Email = "N/A";
                    Membership.UpdateUser(u);
                }
            }
        }
        catch { }

        LoadUserInformation();
    }

    protected void btn_fileUpload_acctImage_Clicked(object sender, EventArgs e) {
        if (fileUpload_acctImage.HasFile) {
            try {
                if (HelperMethods.IsImage(fileUpload_acctImage.PostedFile)) {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileUpload_acctImage.FileName).ToLower();
                    string imgPath = ServerSettings.AccountImageServerLoc + Member.UserId;
                    if (!Directory.Exists(imgPath)) {
                        Directory.CreateDirectory(imgPath);
                    }
                    else {
                        string[] files = Directory.GetFiles(imgPath);
                        foreach (string file in files) {
                            try {
                                File.Delete(file);
                            }
                            catch { }
                        }
                    }

                    ImageConverter ic = new ImageConverter();
                    ic.NeedRoundCorners = false;
                    ic.SaveNewImg(fileUpload_acctImage.PostedFile, imgPath + "\\" + fileName);
                    Member.UpdateAcctImage(fileName);
                }
            }
            catch (Exception ex) {
                AppLog log = new AppLog(false);
                log.AddError(ex);
            }
        }

        Response.Redirect(Request.RawUrl);
    }

    protected void btn_clear_acctImage_Clicked(object sender, EventArgs e) {
        try {
            string imgPath = ServerSettings.AccountImageServerLoc + Member.UserId;
            if (Directory.Exists(imgPath)) {
                Directory.Delete(imgPath, true);
            }

            Member.UpdateAcctImage(string.Empty);
            imgAcctImage.ImageUrl = "~/Standard_Images/EmptyUserImg.png";
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#img_Profile').hide();");
            SetUserImage();
        }
        catch (Exception ex) {
            AppLog log = new AppLog(false);
            log.AddError(ex);
        }

        LoadUserInformation();
    }

    protected void btn_updateusercolor_Clicked(object sender, EventArgs e) {
        string color = txt_userColor.Text.Replace("#", "");
        Member.UpdateColor(color);
        LoadUserInformation();
    }
    protected void btn_resetUserColor_Clicked(object sender, EventArgs e) {
        txt_userColor.Text = Member.UserColor;
        updatepnl_UserInformation2.Update();
    }

    protected void hf_addAdminPage_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_addAdminPage.Value)) {
            string page = hf_addAdminPage.Value;

            if (DrDefaults.Count > 0) {
                string[] adminPageListRegister = DrDefaults["UserSignUpAdminPages"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                string adminpages = "";
                foreach (string p in adminPageListRegister) {
                    adminpages += p + ServerSettings.StringDelimiter;
                }
                adminpages += page;
                NewDefaults.UpdateDefaults("UserSignUpAdminPages", adminpages);
                UpdateDrDefaults();
            }
            else
                Member.UpdateAdminPages(page, false);
        }

        hf_addAdminPage.Value = "";
        LoadUserInformation();
    }
    protected void hf_removeAdminPage_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_removeAdminPage.Value)) {
            string page = hf_removeAdminPage.Value;

            if (DrDefaults.Count > 0) {
                string[] adminPageListRegister = DrDefaults["UserSignUpAdminPages"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                string adminpages = "";
                foreach (string p in adminPageListRegister) {
                    if (p != page) {
                        adminpages += p + ServerSettings.StringDelimiter;
                    }
                }
                NewDefaults.UpdateDefaults("UserSignUpAdminPages", adminpages);
                UpdateDrDefaults();
            }
            else
                Member.UpdateAdminPages(page, true);
        }

        hf_removeAdminPage.Value = "";
        LoadUserInformation();
    }

    protected void rb_receiveall_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ReceiveAll", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateReceiveAll(true);
        }

        LoadNotificationsSettings();
        LoadUserInformation();
    }
    protected void rb_receiveall_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ReceiveAll", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateReceiveAll(false);
        }

        LoadNotificationsSettings();
        LoadUserInformation();
    }

    protected void dd_roles_Changed(object sender, EventArgs e) {
        if (DrDefaults.Count == 0) {
            string user = Request.QueryString["u"];
            Roles.RemoveUserFromRole(Username, RoleSelect);
            RoleSelect = dd_roles.SelectedValue;
            Roles.AddUserToRole(user, RoleSelect);

            if (RoleSelect == ServerSettings.AdminUserName) {
                pnl_adminpages_Holder.Enabled = false;
                pnl_adminpages_Holder.Visible = false;
            }
            else {
                pnl_adminpages_Holder.Enabled = true;
                pnl_adminpages_Holder.Visible = true;
            }
        }
        else {
            RoleSelect = dd_roles.SelectedValue;
        }

        LoadSettingsPage("hf_UpdateAll");
    }

    protected void btn_markasnewuser_Clicked(object sender, EventArgs e) {
        string user = Request.QueryString["u"];
        var msu = new MemberDatabase(user);
        msu.UpdateIsNewMember(true);
        LoadUserInformation();
    }

    #endregion


    #region -- Notifications Settings --

    private void LoadNotificationsSettings() {
        if (pnl_NotificationSettings.Enabled && pnl_NotificationSettings.Visible) {
            LoadNotifications();

            updatepnl_NotificationSettings.Update();
        }
    }

    private int countEnabled = 0;
    private int count = 0;
    private List<string> LoadedList = new List<string>();
    private void LoadNotifications() {
        pnl_notifications.Controls.Clear();
        var str = new StringBuilder();

        bool emailOn = _ss.EmailSystemStatus;

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px'>Notification Name</td>");
        str.Append("<td width='45px'>Image</td>");
        str.Append("<td>Description</td>");
        if (emailOn) {
            str.Append("<td width='45px'>Email</td>");
        }
        str.Append("<td width='110px'>Actions</td></tr>");

        var apps = new App(Username);
        _notifications.GetUserNotifications(Username);
        List<string> userApps = Member.EnabledApps;
        LoadedList.Clear();
        countEnabled = 0;
        count = 0;

        if (Roles.IsUserInRole(Username, ServerSettings.AdminUserName)) {
            Notifications_Coll coll1 = _notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
            if (!string.IsNullOrEmpty(coll1.ID))
                str = AddRow(str, coll1, "236a9dc9-c92a-437f-8825-27809af36a3f", emailOn);
        }

        Notifications_Coll coll2 = _notifications.GetNotification("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f");
        if (!string.IsNullOrEmpty(coll2.ID) && Username.ToLower() != ServerSettings.AdminUserName.ToLower())
            str = AddRow(str, coll2, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", emailOn);

        if (Member.ReceiveAll) {
            Notifications_Coll coll3 = _notifications.GetNotification("1159aca6-2449-4aff-bacb-5f29e479e2d7");
            if (!string.IsNullOrEmpty(coll3.ID))
                str = AddRow(str, coll3, "1159aca6-2449-4aff-bacb-5f29e479e2d7", emailOn);
        }

        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string notifiId = table.NotificationID;
                if (!string.IsNullOrEmpty(notifiId)) {

                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(table, Member)) {
                            continue;
                        }
                    }

                    string[] nIds = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string nId in nIds) {
                        Notifications_Coll coll = _notifications.GetNotification(nId);
                        if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!LoadedList.Contains(nId))) {
                            str = AddRow(str, coll, nId, emailOn);
                        }
                    }
                }
            }
        }

        str.Append("</tbody></table></div>");
        if (count == 0)
            str.Append("<div class='emptyGridView'>No Notifications Available</div>");

        if (countEnabled == 0)
            btn_DisableAll_notification.Style["display"] = "none";
        else {
            btn_DisableAll_notification.Text = "Disable All";
            btn_DisableAll_notification.Style["display"] = "";
        }

        lbl_NotifiEnabled.Text = countEnabled.ToString();
        pnl_notifications.Controls.Add(new LiteralControl(str.ToString()));
    }
    private StringBuilder AddRow(StringBuilder str, Notifications_Coll coll, string notifiId, bool emailOn) {
        LoadedList.Add(notifiId);
        str.Append("<tr class='myItemStyle GridNormalRow'>");
        str.Append("<td class='GridViewNumRow border-bottom' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
        str.Append("<td align='left' width='150px' class='border-right border-bottom'>" + coll.NotificationName + "</td>");

        string notifiImg = coll.NotificationImage;
        if (notifiImg.IndexOf("~/") == 0)
            notifiImg = "../" + notifiImg.Substring(2);

        str.Append("<td align='center' width='45px' class='border-right border-bottom'><img alt='' src='../" + notifiImg + "' style='height: 25px;' /></td>");

        string desc = coll.Description;
        if (string.IsNullOrEmpty(desc))
            desc = "No description available";
        str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");

        string _checked = "";
        bool active = false;
        foreach (UserNotifications_Coll un in _notifications.UserNotifications) {
            if (un.NotificationID == coll.ID) {
                if (un.Email)
                    _checked = " checked='checked'";

                active = true;
                countEnabled++;
                break;
            }
        }

        if (emailOn) {
            string cb = "";
            if (active) {
                cb = "<input type='checkbox' value='1' onchange='UpdateEmail_notification(this, \"" + coll.ID + "\")'" + _checked + " title='Turn on/off'>";
            }
            str.Append("<td width='45px' align='center' class='border-right border-bottom'>" + cb + "</td>");
        }

        str.Append("<td width='110px' class='border-right border-bottom'>" + CreateRadioButtonsEdit_notification(coll.ID, count, active) + "</td></tr>");
        count++;

        return str;
    }
    private string CreateRadioButtonsEdit_notification(string id, int count, bool active) {
        var str = new StringBuilder();
        str.Append("<div class='float-left pad-left'><div class='field switch'>");
        string enabledclass = "cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick=\"UpdateEnabled_notification('" + id + "')\"";
        string onclickDisable = "onclick='openWSE.RemoveUpdateModal();'";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "cb-disable";
            onclickEnable = "onclick='openWSE.RemoveUpdateModal();'";
            onclickDisable = "onclick=\"UpdateDisabled_notification('" + id + "')\"";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_script_active_" + count.ToString() + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_script_active_" + count.ToString() + "'>On</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_script_deactive_" + count.ToString() + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_script_deactive_" + count.ToString() + "'>Off</label></span>");
        str.Append("</div></div>");
        return str.ToString();
    }

    protected void hf_updateEmail_notification_Changed(object sender, EventArgs e) {
        if ((!string.IsNullOrEmpty(hf_updateEmail_notification.Value)) && (!string.IsNullOrEmpty(hf_collId_notification.Value))) {
            bool _email = false;
            if (HelperMethods.ConvertBitToBoolean(hf_updateEmail_notification.Value))
                _email = true;

            _notifications.UpdateUserNotificationEmail(hf_collId_notification.Value, _email, Username);
        }

        hf_collId_notification.Value = "";
        hf_updateEmail_notification.Value = "";
        LoadNotificationsSettings();
    }

    protected void hf_updateEnabled_notification_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateEnabled_notification.Value)) {
            if (!_notifications.IsUserNotificationEnabled(Username, hf_updateEnabled_notification.Value)) {
                Notifications_Coll tempColl = _notifications.GetNotification(hf_updateEnabled_notification.Value);
                if (!string.IsNullOrEmpty(tempColl.ID))
                    _notifications.AddUserNotification(Username, hf_updateEnabled_notification.Value, true);
            }
        }

        hf_updateEnabled_notification.Value = "";
        LoadNotificationsSettings();
    }

    protected void hf_updateDisabled_notification_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateDisabled_notification.Value)) {
            if (_notifications.IsUserNotificationEnabled(Username, hf_updateDisabled_notification.Value))
                _notifications.DeleteUserNotification(Username, hf_updateDisabled_notification.Value);
        }

        hf_updateDisabled_notification.Value = "";
        LoadNotificationsSettings();
    }

    protected void btn_DisableAll_notification_Clicked(object sender, EventArgs e) {
        _notifications.GetUserNotifications(Username);
        if (btn_DisableAll_notification.Text == "Enable All") {
            App _apps = new App(Username);
            List<string> userApps = Member.EnabledApps;
            if (Roles.IsUserInRole(Username, ServerSettings.AdminUserName))
                _notifications.AddUserNotification(Username, "236a9dc9-c92a-437f-8825-27809af36a3f", true);

            if (Member.ReceiveAll)
                _notifications.AddUserNotification(Username, "1159aca6-2449-4aff-bacb-5f29e479e2d7", true);

            _notifications.AddUserNotification(Username, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", true);

            foreach (string w in userApps) {
                var table = _apps.GetAppInformation(w);
                if (table != null) {
                    string notifiId = table.NotificationID;
                    if (!string.IsNullOrEmpty(notifiId)) {
                        string[] notifiArray = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string notifiId_temp in notifiArray) {
                            Notifications_Coll coll = _notifications.GetNotification(notifiId_temp);
                            if (!string.IsNullOrEmpty(coll.NotificationName))
                                _notifications.AddUserNotification(Username, notifiId_temp, true);
                        }
                    }
                }
            }
        }
        else {
            foreach (UserNotifications_Coll coll in _notifications.UserNotifications) {
                _notifications.DeleteUserNotification(Username, coll.NotificationID);
            }
        }

        LoadNotificationsSettings();
    }

    protected void btn_clearnoti_Clicked(object sender, EventArgs e) {
        UserNotificationMessages noti = new UserNotificationMessages(Username);
        noti.deleteAllUserNotification();
        LoadNotificationsSettings();
    }

    #endregion


    #region -- Workspace Overlays --

    private void LoadWorkspaceOverlays() {
        if (pnl_WorkspaceOverlays.Enabled && pnl_WorkspaceOverlays.Visible) {
            LoadOverlays();

            updatepnl_WorkspaceOverlays.Update();
        }
    }

    private void LoadOverlays() {
        pnl_overlays.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='min-width: 100%;'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px' align='left'>Overlay Name</td>");
        str.Append("<td align='left'>Description</td>");
        str.Append("<td width='110px'>Actions</td></tr>");

        var apps = new App(Username);
        _workspaceOverlays.GetUserOverlays(Username);
        List<string> userApps = Member.EnabledApps;

        int count = 1;
        int countEnabled = 0;
        List<string> LoadedList = new List<string>();

        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string overlayId = table.OverlayID;
                if (!string.IsNullOrEmpty(overlayId)) {

                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(table, Member)) {
                            continue;
                        }
                    }

                    string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oId in oIds) {
                        WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                        if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                            LoadedList.Add(oId);
                            str.Append("<tr class='myItemStyle GridNormalRow'><td class='GridViewNumRow border-bottom' width='45px' style='text-align: center'>" + count + "</td>");
                            str.Append("<td align='left' width='150px' class='border-right border-bottom'>" + coll.OverlayName + "</td>");

                            string desc = coll.Description;
                            if (string.IsNullOrEmpty(desc))
                                desc = "No description available";

                            str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");

                            bool active = false;
                            foreach (UserOverlay_Coll userOverlays in _workspaceOverlays.UserOverlays) {
                                if (userOverlays.OverlayID == coll.ID) {
                                    active = true;
                                    countEnabled++;
                                    break;
                                }
                            }

                            str.Append("<td width='110px' class='border-right border-bottom'>" + CreateRadioButtonsEdit_overlay(coll.ID, count, active) + "</td></tr>");
                            count++;
                        }
                    }
                }
            }
        }

        str.Append("</tbody></table></div>");

        if (countEnabled == 0) {
            btn_DisableAll_overlay.Enabled = false;
            btn_DisableAll_overlay.Visible = false;
        }
        else {
            btn_DisableAll_overlay.Enabled = true;
            btn_DisableAll_overlay.Visible = true;
        }

        lbl_overlaysEnabled.Text = countEnabled.ToString();
        pnl_overlays.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string CreateRadioButtonsEdit_overlay(string id, int count, bool active) {
        var str = new StringBuilder();
        str.Append("<div class='float-left pad-left'><div class='field switch'>");
        string enabledclass = "cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick=\"UpdateEnabled_overlay('" + id + "')\"";
        string onclickDisable = "onclick='openWSE.RemoveUpdateModal();'";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "cb-disable";
            onclickEnable = "onclick='openWSE.RemoveUpdateModal();'";
            onclickDisable = "onclick=\"UpdateDisabled_overlay('" + id + "')\"";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_script_active_" + count.ToString() + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_script_active_" + count.ToString() + "'>On</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_script_deactive_" + count.ToString() + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_script_deactive_" + count.ToString() + "'>Off</label></span>");
        str.Append("</div></div>");
        return str.ToString();
    }
    private static string UppercaseFirst(string s) {
        // Check for empty string.
        if (string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    protected void hf_updateEnabled_overlay_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateEnabled_overlay.Value)) {
            if (!_workspaceOverlays.IsUserOverlayEnabled(Username, hf_updateEnabled_overlay.Value)) {
                WorkspaceOverlay_Coll tempColl = _workspaceOverlays.GetWorkspaceOverlay(hf_updateEnabled_overlay.Value);
                _workspaceOverlays.AddUserOverlay(Username, hf_updateEnabled_overlay.Value);
            }
        }

        hf_updateEnabled_overlay.Value = "";
        LoadWorkspaceOverlays();
    }

    protected void hf_updateDisabled_overlay_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateDisabled_overlay.Value)) {
            if (_workspaceOverlays.IsUserOverlayEnabled(Username, hf_updateDisabled_overlay.Value))
                _workspaceOverlays.DeleteUserOverlay(Username, hf_updateDisabled_overlay.Value);
        }

        hf_updateDisabled_overlay.Value = "";
        LoadWorkspaceOverlays();
    }

    protected void btn_DisableAll_overlay_Clicked(object sender, EventArgs e) {
        _workspaceOverlays.GetUserOverlays(Username);
        foreach (UserOverlay_Coll coll in _workspaceOverlays.UserOverlays) {
            _workspaceOverlays.DeleteUserOverlay(Username, coll.OverlayID);
        }
        LoadWorkspaceOverlays();
    }

    #endregion


    #region -- Background Editor --

    private void LoadBackgroundEditor() {
        if (pnl_BackgroundEditor.Enabled && pnl_BackgroundEditor.Visible) {
            GetCurrentBackground();
            LoadBackgroundColor();
            LoadUserBackground();
            EnabledBackgroundsSettings();

            if (Username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                pnl_backgroundurl.Enabled = false;
                pnl_backgroundurl.Visible = false;
            }

            updatepnl_BackgroundEditor.Update();
        }
    }

    private void GetCurrentBackground() {
        string siteTheme = "Standard";
        string background;

        if (pnl_backgroundSelector.Enabled == false) {
            if (DrDefaults.Count > 0) {
                background = NewDefaults.GetBackgroundImg(1);
                siteTheme = DrDefaults["Theme"];
            }
            else {
                background = Member.GetBackgroundImg(1);
                siteTheme = Member.SiteTheme;
            }
        }
        else {
            int workspace = 1;
            if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
            }
            if (DrDefaults.Count > 0) {
                background = NewDefaults.GetBackgroundImg(workspace);
                siteTheme = DrDefaults["Theme"];
            }
            else {
                background = Member.GetBackgroundImg(workspace);
                siteTheme = Member.SiteTheme;
            }
        }

        if (string.IsNullOrEmpty(_siteTheme)) {
            _siteTheme = "Standard";
        }

        string activeimg = string.Empty;

        if (string.IsNullOrEmpty(background)) {
            background = ResolveUrl("~/App_Themes/" + _siteTheme + "/Body/default-bg.jpg");
            activeimg = "<div class='pad-all inline-block margin-right-sml image-selector-current'><img src='" + background + "' style='width: 155px; height: 90px;' class='inline-block boxshadow borderShadow2' /><br /><small>Default</small></div>";
        }

        string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        foreach (string filename in directories) {
            var fi = new FileInfo(filename);
            if ((fi.Extension.ToLower() != ".png") && (fi.Extension.ToLower() != ".jpg") &&
                (fi.Extension.ToLower() != ".jpeg") && (fi.Extension.ToLower() != ".gif")) continue;
            string filepath = "Standard_Images/Backgrounds/" + fi.Name;
            string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
            string size;
            using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                size = image.Width + "x" + image.Height;
            }
            if (background == filepath) {
                activeimg =
                    "<div class='pad-all inline-block margin-right-sml image-selector-current'><img alt='' title='" +
                    size + "' src='../../" + filepath +
                    "' class='boxshadow borderShadow2' style='width: 155px; height: 99px' /></div>";
            }
        }

        if (background.Length == 6) {
            activeimg =
                "<div class='pad-all inline-block margin-right-sml image-selector-current'><div style='background: #" +
                background +
                "; width: 155px; height: 85px;' class='inline-block boxshadow borderShadow2'></div><br /><small>Solid Color</small></div>";
        }

        if (IsValidHttpUri(background)) {
            activeimg = "<div class='pad-all inline-block margin-right-sml image-selector-current'><img alt='' src='" +
                        background + "' class='boxshadow borderShadow2' style='width: 155px; height: 99px' /></div>";
            tb_imageurl.Text = background;
        }
        else {
            tb_imageurl.Text = "Link to image";
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#CurrentBackground').html(\"" + activeimg + "\");");
    }
    private void LoadBackgroundColor() {
        string backgroundcolor = string.Empty;
        if (DrDefaults.Count > 0) {
            backgroundcolor = NewDefaults.GetBackgroundImg(1);
        }
        else {
            backgroundcolor = Member.GetBackgroundImg(1);
        }

        backgroundcolor = backgroundcolor.Replace("#", "");
        if (!string.IsNullOrEmpty(backgroundcolor)) {
            txt_bgColor.Text = backgroundcolor;
        }
        else {
            txt_bgColor.Text = "FFFFFF";
        }
    }
    private void LoadUserBackground() {
        string str = string.Empty;
        string div = "app_title_bg";
        if (IsIframe) {
            div = "app_title_bg_acct";
        }

        if (DrDefaults.Count > 0) {
            str = AcctSettings.LoadDemoBackground(DrDefaults, NewDefaults, this.Page, div);
            if (!string.IsNullOrEmpty(str)) {
                RegisterPostbackScripts.RegisterStartupScript(this, str);
            }
        }
        else {
            str = AcctSettings.LoadUserBackground(Username, Member.SiteTheme, this.Page, div);
            if (!string.IsNullOrEmpty(str)) {
                RegisterPostbackScripts.RegisterStartupScript(this, str);
            }
        }
    }
    private void EnabledBackgroundsSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["EnableBackgrounds"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.MultipleBackgrounds) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_enablebackgrounds_off.Checked = false;
            rb_enablebackgrounds_on.Checked = true;
            pnl_backgroundSelector.Enabled = true;
            pnl_backgroundSelector.Visible = true;
        }
        else {
            rb_enablebackgrounds_off.Checked = true;
            rb_enablebackgrounds_on.Checked = false;
            pnl_backgroundSelector.Enabled = false;
            pnl_backgroundSelector.Visible = false;
            if (dd_backgroundSelector.Items.Count > 0) {
                dd_backgroundSelector.SelectedIndex = 0;
            }
        }
    }

    protected void lb_clearbackground_Click(object sender, EventArgs e) {
        if (pnl_backgroundSelector.Enabled == false) {
            if (DrDefaults.Count > 0) {
                NewDefaults.updateBackgroundImg(string.Empty, 1);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateBackgroundImg(string.Empty, 1);
            }
        }
        else {
            int workspace = 1;
            if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
            }
            if (DrDefaults.Count > 0) {
                NewDefaults.updateBackgroundImg(string.Empty, workspace);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateBackgroundImg(string.Empty, workspace);
            }
        }

        LoadBackgroundEditor();
    }

    protected void btn_updateBGcolor_Clicked(object sender, EventArgs e) {
        string color = txt_bgColor.Text.Replace("#", "");
        switch (pnl_backgroundSelector.Enabled) {
            case false:
                if (DrDefaults.Count > 0) {
                    NewDefaults.updateBackgroundImg(color, 1);
                    UpdateDrDefaults();
                }
                else {
                    Member.UpdateBackgroundImg(color, 1);
                }
                break;
            default: {
                    int workspace = 1;
                    if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                        workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                    }
                    if (DrDefaults.Count > 0) {
                        NewDefaults.updateBackgroundImg(color, workspace);
                        UpdateDrDefaults();
                    }
                    else {
                        Member.UpdateBackgroundImg(color, workspace);
                    }
                }
                break;
        }

        LoadBackgroundEditor();
    }

    protected void btn_clearBGcolor_Clicked(object sender, EventArgs e) {
        txt_bgColor.Text = "FFFFFF";
        LoadBackgroundEditor();
    }

    protected void rb_enablebackgrounds_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("EnableBackgrounds", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateEnableBackgrounds(true);
        }

        LoadBackgroundEditor();
    }
    protected void rb_enablebackgrounds_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("EnableBackgrounds", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateEnableBackgrounds(false);
        }

        LoadBackgroundEditor();
    }

    protected void dd_backgroundSelector_Changed(object sender, EventArgs e) {
        LoadBackgroundEditor();
    }

    protected void btn_urlupdate_Click(object sender, EventArgs e) {
        if (IsValidHttpUri(tb_imageurl.Text)) {
            if (pnl_backgroundSelector.Enabled == false) {
                if (DrDefaults.Count > 0) {
                    NewDefaults.updateBackgroundImg(tb_imageurl.Text.Trim(), 1);
                    UpdateDrDefaults();
                }
                else {
                    Member.UpdateBackgroundImg(tb_imageurl.Text.Trim(), 1);
                }
                BuildBackgrounds(1);
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    NewDefaults.updateBackgroundImg(tb_imageurl.Text.Trim(), workspace);
                    UpdateDrDefaults();
                }
                else {
                    Member.UpdateBackgroundImg(tb_imageurl.Text.Trim(), workspace);
                }
                BuildBackgrounds(workspace);
            }

            LoadBackgroundEditor(); ;

            string background;

            if (pnl_backgroundSelector.Enabled == false) {
                if (DrDefaults.Count > 0) {
                    background = NewDefaults.GetBackgroundImg(1);
                }
                else {
                    background = Member.GetBackgroundImg(1);
                }
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    background = NewDefaults.GetBackgroundImg(workspace);
                }
                else {
                    background = Member.GetBackgroundImg(workspace);
                }
            }

            string activeimg = string.Empty;

            if (string.IsNullOrEmpty(_siteTheme)) {
                _siteTheme = "Standard";
            }

            if (string.IsNullOrEmpty(background)) {
                background = ResolveUrl("~/App_Themes/" + _siteTheme + "/Body/default-bg.jpg");

                activeimg = "<div class='pad-all inline-block margin-right-sml image-selector-current'><img src='" + background + "' style='width: 155px; height: 90px;' class='inline-block boxshadow borderShadow2' /><br /><small>Default</small></div>";
            }

            string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
            foreach (string filename in directories) {
                var fi = new FileInfo(filename);
                if ((fi.Extension.ToLower() != ".png") && (fi.Extension.ToLower() != ".jpg") &&
                    (fi.Extension.ToLower() != ".jpeg") && (fi.Extension.ToLower() != ".gif")) continue;
                string filepath = "Standard_Images/Backgrounds/" + fi.Name;
                string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
                string size;
                using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                    size = image.Width + "x" + image.Height;
                }
                if (background == filepath) {
                    activeimg =
                        "<div class='pad-all inline-block margin-right-sml image-selector-current'><img alt='' title='" +
                        size + "' src='../../" + filepath +
                        "' class='boxshadow borderShadow2' style='width: 155px; height: 99px' /></div>";
                }
            }

            if (background.Length == 6) {
                activeimg =
                    "<div class='pad-all inline-block margin-right-sml image-selector-current'><div style='background: #" +
                    background +
                    "; width: 155px; height: 85px;' class='inline-block boxshadow borderShadow2'></div><br /><small>Solid Color</small></div>";
            }

            if (IsValidHttpUri(background)) {
                activeimg = "<div class='pad-all inline-block margin-right-sml image-selector-current'><img alt='' src='" +
                            background + "' class='boxshadow borderShadow2' style='width: 155px; height: 99px' /></div>";
                tb_imageurl.Text = background;
            }
            else {
                tb_imageurl.Text = "Link to image";
            }

            string img = "$('#CurrentBackground').html(\"" + activeimg + "\");";
            RegisterPostbackScripts.RegisterStartupScript(this, img);
        }
    }

    protected void hf_backgroundimg_Changed(object sender, EventArgs e) {
        var image = hf_backgroundimg.Value.Replace("../", "");

        if (pnl_backgroundSelector.Enabled == false) {
            if (DrDefaults.Count > 0) {
                NewDefaults.updateBackgroundImg(image, 1);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateBackgroundImg(image, 1);
            }
        }
        else {
            int workspace = 1;
            if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
            }
            if (DrDefaults.Count > 0) {
                NewDefaults.updateBackgroundImg(image, workspace);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateBackgroundImg(image, workspace);
            }
        }

        LoadBackgroundEditor();
        hf_backgroundimg.Value = "";
    }
    private void BuildBackgrounds(int workspace) {
        string background = string.Empty;
        if (DrDefaults.Count > 0) {
            background = NewDefaults.GetBackgroundImg(workspace);
        }
        else {
            background = Member.GetBackgroundImg(workspace);
        }

        var str = new StringBuilder();
        string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        foreach (string filename in directories) {
            var fi = new FileInfo(filename);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                string filepath = "Standard_Images/Backgrounds/" + fi.Name;
                string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
                string size;
                using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2))
                    size = image.Width + "x" + image.Height;
                string c = "pad-all-sml inline-block ";
                c += background == filepath ? "image-selector-active" : "image-selector-acct";

                str.Append("<div class='" + c + "'><img alt='' title='" + size + "' src='../../" + filepath +
                           "' class='boxshadow borderShadow2' style='height: 50px;' /></div>");
            }
        }

        if (IsValidHttpUri(background)) {
            tb_imageurl.Text = background;
            str.Append("<div class='pad-all inline-block margin-right-sml image-selector-acct'><img alt='' src='" +
                       background + "' class='boxshadow borderShadow2' style='height: 50px;' /></div>");
        }

        string img = "$('#pnl_images').html(\"" + str + "\");";
        RegisterPostbackScripts.RegisterStartupScript(this, img);
    }

    public static bool IsValidHttpUri(string uriString) {
        Uri test;
        return Uri.TryCreate(uriString, UriKind.Absolute, out test) && test.Scheme == "http";
    }
    protected void hf_backgroundselector_ValueChanged(object sender, EventArgs e) {
        if (pnl_backgroundSelector.Enabled == false) {
            BuildBackgrounds(1);
        }
        else {
            int workspace = 1;
            if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
            }
            BuildBackgrounds(workspace);
        }

        LoadBackgroundEditor();
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'Background-element', 'Workspace Background Selector');");
    }

    #endregion


    #region -- Top/Side Menu Bar --

    private void LoadTopSideMenuBar() {
        if (pnl_TopSideMenuBar.Enabled && pnl_TopSideMenuBar.Visible) {
            ShowDateTimeSettings();
            LoadLinksBlankPageSettings();
            AutoHideModeSettings();
            TaskbarShowAllSettings();
            HoverPreviewWorkspaceSettings();
            MinPreviewSettings();

            updatepnl_TopSideMenuBar.Update();
        }
    }

    private void ShowDateTimeSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowDateTime"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowDateTime) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_showdatetime_on.Checked = true;
            rb_showdatetime_off.Checked = false;
        }
        else {
            rb_showdatetime_on.Checked = false;
            rb_showdatetime_off.Checked = true;
        }
    }
    private void LoadLinksBlankPageSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["LoadLinksBlankPage"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.LoadLinksBlankPage) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_linksnewpage_off.Checked = false;
            rb_linksnewpage_on.Checked = true;
        }
        else {
            rb_linksnewpage_off.Checked = true;
            rb_linksnewpage_on.Checked = false;
        }
    }
    private void AutoHideModeSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["AutoHideMode"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.AutoHideMode) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_autohidemode_off.Checked = false;
            rb_autohidemode_on.Checked = true;
        }
        else {
            rb_autohidemode_off.Checked = true;
            rb_autohidemode_on.Checked = false;
        }
    }
    private void TaskbarShowAllSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["TaskBarShowAll"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.TaskBarShowAll) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_taskbarShowAll_Off.Checked = false;
            rb_taskbarShowAll_On.Checked = true;
        }
        else {
            rb_taskbarShowAll_Off.Checked = true;
            rb_taskbarShowAll_On.Checked = false;
        }
    }
    private void HoverPreviewWorkspaceSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["HoverPreviewWorkspace"]) || string.IsNullOrEmpty(DrDefaults["HoverPreviewWorkspace"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.HoverPreviewWorkspace) {
                settingOn = true;
            }
        }


        if (settingOn) {
            rb_showWorkspacePreview_on.Checked = true;
            rb_showWorkspacePreview_off.Checked = false;
        }
        else {
            rb_showWorkspacePreview_on.Checked = false;
            rb_showWorkspacePreview_off.Checked = true;
        }
    }
    private void MinPreviewSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowMinimizedPreview"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowMinimizedPreview) {
                settingOn = true;
            }
        }


        if (settingOn) {
            rb_showPreview_on.Checked = true;
            rb_showPreview_off.Checked = false;
        }
        else {
            rb_showPreview_on.Checked = false;
            rb_showPreview_off.Checked = true;
        }
    }

    protected void rb_showdatetime_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowDateTime", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowDateTime(true);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            string script = "$('#DateDisplay').hide();$('#localtime').hide();$('#DateDisplay').html('" + DateTime.Now.ToString("dddd, MMM dd") + "');startCurrentTime();";
            script += "$('#DateDisplay').fadeIn(150);$('#localtime').fadeIn(150);";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_showdatetime_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowDateTime", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowDateTime(false);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            string script = "$('#DateDisplay').fadeOut(150);$('#localtime').fadeOut(150, function () {";
            script += "clearInterval(startCurrentTime_interval);$('#DateDisplay').html('');$('#localtime').html(''); });";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadTopSideMenuBar();
    }

    protected void rb_linksnewpage_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LoadLinksBlankPage", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLoadLinksBlankPage(true);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_linksnewpage_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LoadLinksBlankPage", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLoadLinksBlankPage(false);
        }

        LoadTopSideMenuBar();
    }

    protected void rb_autohidemode_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AutoHideMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoHideMode(true);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_autohidemode_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AutoHideMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoHideMode(false);
        }

        LoadTopSideMenuBar();
    }

    protected void rb_taskbarShowAll_On_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TaskBarShowAll", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTaskBarShowAll(true);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_taskbarShowAll_Off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TaskBarShowAll", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTaskBarShowAll(false);
        }

        LoadTopSideMenuBar();
    }

    protected void rb_showWorkspacePreview_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HoverPreviewWorkspace", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHoverPreviewWorkspace(true);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_showWorkspacePreview_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HoverPreviewWorkspace", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHoverPreviewWorkspace(false);
        }

        LoadTopSideMenuBar();
    }

    protected void rb_showPreview_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowMinimizedPreview", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowMinimizedPreview(true);
        }

        LoadTopSideMenuBar();
    }
    protected void rb_showPreview_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowMinimizedPreview", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowMinimizedPreview(false);
        }

        LoadTopSideMenuBar();
    }

    #endregion


    #region -- Icon Selector --

    private void LoadIconSelector() {
        if (pnl_IconSelector.Enabled && pnl_IconSelector.Visible) {
            LockAppIconsSettings();
            GroupIconsSettings();
            AppCategoryCountSettings();
            HideAppIconSettings();
            ShowWorkspaceNumAppSettings();

            updatepnl_IconSelector.Update();
        }
    }

    private void LockAppIconsSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["LockAppIcons"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.LockAppIcons) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_LockAppIcons_on.Checked = true;
            rb_LockAppIcons_off.Checked = false;
        }
        else {
            rb_LockAppIcons_on.Checked = false;
            rb_LockAppIcons_off.Checked = true;
        }
    }
    private void GroupIconsSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["GroupIcons"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.GroupIcons) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_groupicons_on.Checked = true;
            rb_groupicons_off.Checked = false;
            pnl_categoryCount.Enabled = true;
            pnl_categoryCount.Visible = true;
        }
        else {
            rb_groupicons_on.Checked = false;
            rb_groupicons_off.Checked = true;
            pnl_categoryCount.Enabled = false;
            pnl_categoryCount.Visible = false;
        }
    }
    private void AppCategoryCountSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["IconCategoryCount"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowCategoryCount) {
                settingOn = true;
            }
        }
        if (settingOn) {
            rb_showappcategoryCount_on.Checked = true;
            rb_showappcategoryCount_off.Checked = false;
        }
        else {
            rb_showappcategoryCount_on.Checked = false;
            rb_showappcategoryCount_off.Checked = true;
        }
    }
    private void HideAppIconSettings() {
        if (_ss.HideAllAppIcons) {
            pnl_HideAppIcons.Enabled = false;
            pnl_HideAppIcons.Visible = false;
            pnl_showAppImage.Enabled = false;
            pnl_showAppImage.Visible = false;
        }
        else {
            pnl_HideAppIcons.Enabled = true;
            pnl_HideAppIcons.Visible = true;

            bool settingOn = false;

            if (DrDefaults.Count > 0) {
                if (HelperMethods.ConvertBitToBoolean(DrDefaults["HideAppIcon"])) {
                    settingOn = true;
                }
            }
            else {
                if (Member.HideAppIcon) {
                    settingOn = true;
                }
            }

            if (settingOn) {
                rb_hideAppIcon_on.Checked = true;
                rb_hideAppIcon_off.Checked = false;
                pnl_showAppImage.Enabled = false;
                pnl_showAppImage.Visible = false;
            }
            else {
                rb_hideAppIcon_on.Checked = false;
                rb_hideAppIcon_off.Checked = true;
                pnl_showAppImage.Enabled = true;
                pnl_showAppImage.Visible = true;
            }
        }
    }
    private void ShowWorkspaceNumAppSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowWorkspaceNumApp"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowWorkspaceNumApp) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_ShowWorkspaceNumApp_on.Checked = true;
            rb_ShowWorkspaceNumApp_off.Checked = false;
        }
        else {
            rb_ShowWorkspaceNumApp_on.Checked = false;
            rb_ShowWorkspaceNumApp_off.Checked = true;
        }
    }

    protected void rb_LockAppIcons_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LockAppIcons", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLockAppIcons(true);
        }
        LoadIconSelector();
    }
    protected void rb_LockAppIcons_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LockAppIcons", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLockAppIcons(false);
        }

        LoadIconSelector();
    }

    protected void rb_groupicons_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("GroupIcons", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateGroupIcons(true);
        }

        LoadIconSelector();
    }
    protected void rb_groupicons_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("GroupIcons", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateGroupIcons(false);
        }

        LoadIconSelector();
    }

    protected void rb_showappcategoryCount_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("IconCategoryCount", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowCategoryCount(true);
        }

        LoadIconSelector();
    }
    protected void rb_showappcategoryCount_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("IconCategoryCount", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowCategoryCount(false);
        }

        LoadIconSelector();
    }

    protected void rb_hideAppIcon_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideAppIcon", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideAppIcon(true);
        }

        LoadIconSelector();
        LoadSiteCustomizations();
    }
    protected void rb_hideAppIcon_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideAppIcon", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideAppIcon(false);
        }

        LoadIconSelector();
        LoadSiteCustomizations();
    }

    protected void rb_ShowWorkspaceNumApp_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowWorkspaceNumApp", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowWorkspaceNumApp(true);
        }

        LoadIconSelector();
    }
    protected void rb_ShowWorkspaceNumApp_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowWorkspaceNumApp", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowWorkspaceNumApp(false);
        }

        LoadIconSelector();
    }

    #endregion


    #region -- Site Customizations --

    private void LoadSiteCustomizations() {
        if (pnl_SiteCustomizations.Enabled && pnl_SiteCustomizations.Visible) {
            if (DrDefaults.Count > 0) {
                BuildAppPackagesSettings();
                BuildPackageList_DemoDefaults();
            }
            LoadWorkspaceMode();
            GetAnimationSpeedSettings();
            SiteThemeSettings();
            ShowToolTipsSettings();
            LoadPrivacySettings();
            ClearPropertiesOnSignOffSettings();
            PresentationModeSettings();
            ShowAppTitleSettings();
            AppHeaderIconSettings();

            updatepnl_SiteCustomizations.Update();
        }
    }

    private void BuildAppPackagesSettings() {
        string currPackage = DrDefaults["AppPackage"];
        var wp = new AppPackages(true);
        if (wp.listdt.Count == 0) {
            NewDefaults.UpdateDefaults("AppPackage", "");
            UpdateDrDefaults();
        }
        else {
            int count = 0;
            dd_appdemo.Items.Clear();
            foreach (Dictionary<string, string> dr in wp.listdt) {
                var item = new ListItem(dr["PackageName"], dr["ID"]);
                dd_appdemo.Items.Add(item);

                if (dr["ID"] == currPackage)
                    dd_appdemo.SelectedIndex = count;

                count++;
            }
        }
    }
    private void BuildPackageList_DemoDefaults() {
        dd_appdemo.Items.Clear();
        var str = new StringBuilder();
        AppPackages _packs = new AppPackages(true);
        foreach (Dictionary<string, string> dr in _packs.listdt) {
            dd_appdemo.Items.Add(new ListItem(dr["PackageName"], dr["ID"]));
        }
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#dd_appdemo').val('" + NewDefaults.GetDemoAppPackage + "');");
    }
    private void LoadWorkspaceMode() {
        if (!IsPostBack) {
            string mode = "simple";
            if (DrDefaults.Count > 0) {
                mode = DrDefaults["WorkspaceMode"].ToLower();
            }
            else {
                mode = Member.WorkspaceMode.ToString().ToLower();
            }

            if (string.IsNullOrEmpty(mode)) {
                mode = MemberDatabase.UserWorkspaceMode.Complex.ToString().ToLower();
            }

            for (int i = 0; i < ddl_WorkspaceMode.Items.Count; i++) {
                if (ddl_WorkspaceMode.Items[i].Value.ToLower() == mode) {
                    ddl_WorkspaceMode.SelectedIndex = i;
                    break;
                }
            }

            if (!MemberDatabase.IsComplexWorkspaceMode(mode)) {
                pnl_nonadminsettings.Enabled = false;
                pnl_nonadminsettings.Visible = false;

                pnl_lockappicons.Enabled = false;
                pnl_lockappicons.Visible = false;
                pnl_ShowWorkspaceNum.Enabled = false;
                pnl_ShowWorkspaceNum.Visible = false;

                pnl_clearproperties.Enabled = false;
                pnl_clearproperties.Visible = false;
                pnl_clearUserProp.Enabled = false;
                pnl_clearUserProp.Visible = false;
                pnl_presentationMode.Enabled = false;
                pnl_presentationMode.Visible = false;
                pnl_ShowAppTitle.Enabled = false;
                pnl_ShowAppTitle.Visible = false;
                pnl_showAppImage.Enabled = false;
                pnl_showAppImage.Visible = false;

                pnl_WorkspaceContainer.Enabled = false;
                pnl_WorkspaceContainer.Visible = false;
            }
        }
    }
    private void GetAnimationSpeedSettings() {
        string _as = "150";
        if (DrDefaults.Count > 0) {
            _as = DrDefaults["AnimationSpeed"];
            if (string.IsNullOrEmpty(_as))
                _as = "150";
        }
        else {
            _as = Member.AnimationSpeed.ToString();
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "InitializeSiteAnimationSpeed(" + _as + ");");
    }
    private void SiteThemeSettings() {
        dd_theme.Items.Clear();
        int count = 0;
        string currTheme = "Standard";
        if (DrDefaults.Count > 0) {
            currTheme = DrDefaults["Theme"];
        }
        else {
            currTheme = Member.SiteTheme;
        }
        var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "App_Themes\\");
        foreach (var dir in di.GetDirectories()) {
            if (!dir.Name.ToLower().Contains("login")) {
                var item = new ListItem(dir.Name, dir.Name);
                dd_theme.Items.Add(item);

                if (currTheme.ToLower() == dir.Name.ToLower()) {
                    dd_theme.SelectedIndex = count;
                }

                count++;
            }
        }
    }
    private void ShowToolTipsSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ToolTips"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowToolTips) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_tooltips_on.Checked = true;
            rb_tooltips_off.Checked = false;
        }
        else {
            rb_tooltips_on.Checked = false;
            rb_tooltips_off.Checked = true;
        }
    }
    private void LoadPrivacySettings() {
        if (_ss.AllowPrivacy && (DrDefaults == null || DrDefaults.Count == 0)) {
            if (Member.PrivateAccount) {
                rb_Privacy_on.Checked = true;
                rb_Privacy_off.Checked = false;
            }
            else {
                rb_Privacy_on.Checked = false;
                rb_Privacy_off.Checked = true;
            }
        }
        else {
            pnl_accountPrivacy.Enabled = false;
            pnl_accountPrivacy.Visible = false;
        }
    }
    private void ClearPropertiesOnSignOffSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ClearPropOnSignOff"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ClearPropOnSignOff) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_clearproperties_off.Checked = false;
            rb_clearproperties_on.Checked = true;
        }
        else {
            rb_clearproperties_off.Checked = true;
            rb_clearproperties_on.Checked = false;
        }
    }
    private void PresentationModeSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["PresentationMode"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.PresentationMode) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_presentationmode_off.Checked = false;
            rb_presentationmode_on.Checked = true;
            pnl_autohidemode.Enabled = false;
            pnl_autohidemode.Visible = false;
        }
        else {
            rb_presentationmode_off.Checked = true;
            rb_presentationmode_on.Checked = false;
            pnl_autohidemode.Enabled = true;
            pnl_autohidemode.Visible = true;
        }
    }
    private void ShowAppTitleSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowAppTitle"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowAppTitle) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_showHeader_off.Checked = false;
            rb_showHeader_on.Checked = true;
        }
        else {
            rb_showHeader_off.Checked = true;
            rb_showHeader_on.Checked = false;
        }
    }
    private void AppHeaderIconSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["AppHeaderIcon"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.AppHeaderIcon) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_AppHeaderIcon_off.Checked = false;
            rb_AppHeaderIcon_on.Checked = true;
        }
        else {
            rb_AppHeaderIcon_off.Checked = true;
            rb_AppHeaderIcon_on.Checked = false;
        }
    }

    protected void btn_updatedemo_Click(object sender, EventArgs e) {
        NewDefaults.UpdateDefaults("AppPackage", dd_appdemo.SelectedValue);
        UpdateDrDefaults();
        LoadSiteCustomizations();
    }

    protected void btn_WorkspaceMode_Click(object sender, EventArgs e) {
        if (DrDefaults.Count == 0) {
            Member.UpdateWorkspaceMode(ddl_WorkspaceMode.SelectedValue);
        }
        else {
            NewDefaults.UpdateDefaults("WorkspaceMode", ddl_WorkspaceMode.SelectedValue);
            UpdateDrDefaults();
        }

        Response.Redirect(Request.RawUrl);
    }

    protected void hf_AnimationSpeed_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_AnimationSpeed.Value)) {
            int tempOut = 0;
            if (int.TryParse(hf_AnimationSpeed.Value, out tempOut)) {
                if (DrDefaults.Count > 0) {
                    NewDefaults.UpdateDefaults("AnimationSpeed", tempOut.ToString());
                    UpdateDrDefaults();
                }
                else {
                    Member.UpdateAnimationSpeed(tempOut);
                }
            }
        }

        hf_AnimationSpeed.Value = "";
        LoadSiteCustomizations();
    }

    protected void dd_theme_Changed(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("Theme", dd_theme.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTheme(dd_theme.SelectedValue);
        }

        LoadSiteCustomizations();
    }

    protected void rb_tooltips_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ToolTips", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowToolTip(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_tooltips_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ToolTips", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowToolTip(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_Privacy_on_CheckedChanged(object sender, EventArgs e) {
        Member.UpdatePrivateAccount(true);
        LoadSiteCustomizations();
    }
    protected void rb_Privacy_off_CheckedChanged(object sender, EventArgs e) {
        Member.UpdatePrivateAccount(false);
        LoadSiteCustomizations();
    }

    protected void rb_clearproperties_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ClearPropOnSignOff", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateClearPropOnSignOff(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_clearproperties_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ClearPropOnSignOff", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateClearPropOnSignOff(false);
        }

        LoadSiteCustomizations();
    }

    protected void btn_clearapps_Click(object sender, EventArgs e) {
        var app = new App(Username);
        app.DeleteUserProperties(Username);

        try {
            HttpCookieCollection cookieColl = new HttpCookieCollection();
            foreach (object key in Request.Cookies.Keys) {
                if ((key.ToString().ToLower() != ".aspxauth") && (key.ToString().ToLower() != "asp.net_sessionid")) {
                    HttpCookie cookie = Request.Cookies[key.ToString()];
                    if (cookie != null) {
                        cookie.Expires = DateTime.Now.AddDays(-1d);
                        cookieColl.Add(cookie);
                    }
                }
            }

            foreach (object c in cookieColl.Keys) {
                HttpCookie cookie = Request.Cookies[c.ToString()];
                Response.Cookies.Add(cookie);
            }
        }
        catch { }

        LoadSiteCustomizations();
    }

    protected void rb_presentationmode_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("PresentationMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdatePresentationMode(true);
        }

        LoadSiteCustomizations();
        LoadTopSideMenuBar();
    }
    protected void rb_presentationmode_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("PresentationMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdatePresentationMode(false);
        }

        LoadSiteCustomizations();
        LoadTopSideMenuBar();
    }

    protected void rb_showHeader_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowAppTitle", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowAppTitle(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_showHeader_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowAppTitle", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowAppTitle(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_AppHeaderIcon_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppHeaderIcon", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppHeaderIcon(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_AppHeaderIcon_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppHeaderIcon", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppHeaderIcon(false);
        }

        LoadSiteCustomizations();
    }

    #endregion


    #region -- Workspace Container --

    private void LoadWorkspaceContainer() {
        if (pnl_WorkspaceContainer.Enabled && pnl_WorkspaceContainer.Visible) {
            TotalWorkspacesSettings();
            AppContainerSettings();
            SnapAppToGridSettings();
            AppGridSizeSettings();
            AutoRotateWorkspaceSettings();

            updatepnl_WorkspaceContainer.Update();
        }
    }

    private void TotalWorkspacesSettings() {
        int td = 4;
        if (DrDefaults.Count > 0) {
            if (!string.IsNullOrEmpty(DrDefaults["TotalWorkspaces"])) {
                td = Convert.ToInt16(DrDefaults["TotalWorkspaces"]);
            }
        }
        else {
            td = Member.TotalWorkspaces;
        }

        if (td == 0) {
            td = 4;
        }

        int totalAllowed = _ss.TotalWorkspacesAllowed;
        ddl_totalWorkspaces.Items.Clear();

        for (int i = 0; i < totalAllowed; i++) {
            string val = (i + 1).ToString();
            ListItem item = new ListItem(val, val);
            ddl_totalWorkspaces.Items.Add(item);
        }


        dd_backgroundSelector.Items.Clear();
        ddl_autoRotateNumber.Items.Clear();
        for (int i = 0; i < td; i++) {
            string val = (i + 1).ToString();
            ListItem item = new ListItem(val, val);
            dd_backgroundSelector.Items.Add(item);
            ddl_autoRotateNumber.Items.Add(item);
        }

        if (td <= 1) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + pnl_backgroundurl.ClientID + "').hide();");
            pnl_autoRotateOnOff.Enabled = false;
            pnl_autoRotateOnOff.Visible = false;
            pnlAutoRotateWorkspace.Enabled = false;
            pnlAutoRotateWorkspace.Visible = false;

        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + pnl_backgroundurl.ClientID + "').show();");
            pnl_autoRotateOnOff.Enabled = true;
            pnl_autoRotateOnOff.Visible = true;
            if (rb_enableautorotate_on.Checked) {
                pnlAutoRotateWorkspace.Enabled = true;
                pnlAutoRotateWorkspace.Visible = true;
            }
            else {
                pnlAutoRotateWorkspace.Enabled = false;
                pnlAutoRotateWorkspace.Visible = false;
            }
        }

        if (td > 0) {
            td = td - 1;
        }

        ddl_totalWorkspaces.SelectedIndex = td;
    }
    private void AppContainerSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["AppContainer"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.AppContainer) {
                settingOn = true;
            }
        }
        if (settingOn) {
            rb_appcontainer_enabled.Checked = true;
            rb_appcontainer_disabled.Checked = false;
        }
        else {
            rb_appcontainer_enabled.Checked = false;
            rb_appcontainer_disabled.Checked = true;
        }
    }
    private void SnapAppToGridSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["SnapToGrid"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.AppSnapToGrid) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_snapapp_off.Checked = false;
            rb_snapapp_on.Checked = true;
            pnl_appGridSize.Enabled = true;
            pnl_appGridSize.Visible = true;
        }
        else {
            rb_snapapp_off.Checked = true;
            rb_snapapp_on.Checked = false;
            pnl_appGridSize.Enabled = false;
            pnl_appGridSize.Visible = false;
        }
    }
    private void AppGridSizeSettings() {
        if (DrDefaults.Count > 0) {
            string val = DrDefaults["AppGridSize"];
            if (string.IsNullOrEmpty(val)) {
                val = "20";
            }

            txt_AppGridSize.Text = val;
        }
        else {
            txt_AppGridSize.Text = Member.AppGridSize;
        }
    }
    private void AutoRotateWorkspaceSettings() {
        bool workspaceRotate = false;
        bool rotateAutoRefresh = false;
        string workspaceRotateInterval = "60";
        int totalAllowed = 4;

        if (DrDefaults.Count > 0) {
            workspaceRotate = HelperMethods.ConvertBitToBoolean(DrDefaults["WorkspaceRotate"]);
            rotateAutoRefresh = HelperMethods.ConvertBitToBoolean(DrDefaults["RotateAutoRefresh"]);
            workspaceRotateInterval = DrDefaults["WorkspaceRotateInterval"].Trim();
            int.TryParse(DrDefaults["WorkspaceRotateScreens"], out totalAllowed);
            if (totalAllowed <= 0) {
                totalAllowed = 1;
            }
        }
        else {
            workspaceRotate = Member.WorkspaceRotate;
            rotateAutoRefresh = Member.RotateAutoRefresh;
            workspaceRotateInterval = Member.WorkspaceRotateInterval.Trim();
            int.TryParse(Member.WorkspaceRotateScreens, out totalAllowed);
            if (totalAllowed <= 0) {
                totalAllowed = 1;
            }
        }

        if (workspaceRotate) {
            rb_enableautorotate_on.Checked = true;
            rb_enableautorotate_off.Checked = false;
            pnlAutoRotateWorkspace.Enabled = true;
            pnlAutoRotateWorkspace.Visible = true;

            if (rotateAutoRefresh) {
                rb_updateOnRotate_on.Checked = true;
                rb_updateOnRotate_off.Checked = false;
            }
            else {
                rb_updateOnRotate_on.Checked = false;
                rb_updateOnRotate_off.Checked = true;
            }


            tb_autorotateinterval.Text = workspaceRotateInterval;
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + ddl_autoRotateNumber.ClientID + "').val('" + totalAllowed + "');");
        }
        else {
            rb_enableautorotate_on.Checked = false;
            rb_enableautorotate_off.Checked = true;
            if (rotateAutoRefresh) {
                rb_updateOnRotate_on.Checked = true;
                rb_updateOnRotate_off.Checked = false;
            }
            else {
                rb_updateOnRotate_on.Checked = false;
                rb_updateOnRotate_off.Checked = true;
            }

            pnlAutoRotateWorkspace.Enabled = false;
            pnlAutoRotateWorkspace.Visible = false;
        }
    }

    protected void btn_updateTotalWorkspaces_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TotalWorkspaces", ddl_totalWorkspaces.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTotalWorkspaces(Convert.ToInt16(ddl_totalWorkspaces.SelectedValue));
        }

        LoadWorkspaceContainer();
        LoadBackgroundEditor();
    }

    protected void rb_appcontainer_enabled_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppContainer", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppContainer(true);
        }

        LoadWorkspaceContainer();
    }
    protected void rb_appcontainer_disabled_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppContainer", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppContainer(false);
        }

        LoadWorkspaceContainer();
    }

    protected void rb_snapapp_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SnapToGrid", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppSnapToGrid(true);
        }

        LoadWorkspaceContainer();
    }
    protected void rb_snapapp_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SnapToGrid", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppSnapToGrid(false);
        }

        LoadWorkspaceContainer();
    }

    protected void btn_AppGridSize_Click(object sender, EventArgs e) {
        string val = txt_AppGridSize.Text.Trim();
        int tempInt = 0;
        int.TryParse(val, out tempInt);
        if (tempInt > 0 && tempInt <= 100) {
            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("AppGridSize", val);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateAppGridSize(val);
            }
        }

        LoadWorkspaceContainer();
    }

    protected void rb_enableautorotate_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("WorkspaceRotate", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoRotateWorkspace(true);
        }

        LoadWorkspaceContainer();
    }
    protected void rb_enableautorotate_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("WorkspaceRotate", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoRotateWorkspace(false);
        }

        LoadWorkspaceContainer();
    }

    protected void rb_updateOnRotate_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("RotateAutoRefresh", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateRotateAutoRefresh(true);
        }

        LoadWorkspaceContainer();
    }
    protected void rb_updateOnRotate_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("RotateAutoRefresh", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateRotateAutoRefresh(false);
        }

        LoadWorkspaceContainer();
    }

    protected void btn_updateintervals_rotate_Click(object sender, EventArgs e) {
        string interval = tb_autorotateinterval.Text.Trim();
        string workspaceScreens = "4";

        if (DrDefaults.Count > 0) {
            workspaceScreens = DrDefaults["WorkspaceRotateScreens"];
            if ((!string.IsNullOrEmpty(interval)) && (interval != "0")) {
                NewDefaults.UpdateDefaults("WorkspaceRotateInterval", interval);
                UpdateDrDefaults();
            }
            else if (interval == "0") {
                tb_autorotateinterval.Text = "1";
            }
        }
        else {
            workspaceScreens = Member.WorkspaceRotateScreens;
            if ((!string.IsNullOrEmpty(interval)) && (interval != "0")) {
                Member.UpdateAutoRotateWorkspaceInterval(interval);
            }
            else if (interval == "0") {
                tb_autorotateinterval.Text = "1";
            }
        }

        LoadWorkspaceContainer();
    }
    protected void btn_screenRotateNumberUpdate_Click(object sender, EventArgs e) {
        string workspaceScreens = "4";
        if (DrDefaults.Count > 0) {
            workspaceScreens = DrDefaults["WorkspaceRotateScreens"];
            NewDefaults.UpdateDefaults("WorkspaceRotateScreens", ddl_autoRotateNumber.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            workspaceScreens = Member.WorkspaceRotateScreens;
            Member.UpdateWorkspaceRotateScreens(ddl_autoRotateNumber.SelectedValue);
        }

        LoadWorkspaceContainer();
    }

    #endregion


    #region -- Chat Client --

    private void LoadChatClient() {
        if (pnl_ChatClient.Enabled && pnl_ChatClient.Visible) {
            LoadChatSettings();
            SetChatSoundNotiSettings();

            updatepnl_ChatClient.Update();
        }
    }

    private void LoadChatSettings() {
        if (_ss.ChatEnabled) {
            if (DrDefaults.Count > 0) {
                if (HelperMethods.ConvertBitToBoolean(DrDefaults["EnableChat"])) {
                    rb_chatclient_off.Checked = false;
                    rb_chatclient_on.Checked = true;
                    pnl_chattimeout.Enabled = true;
                    pnl_chattimeout.Visible = true;
                }
                else {
                    rb_chatclient_off.Checked = true;
                    rb_chatclient_on.Checked = false;
                    pnl_chattimeout.Enabled = false;
                    pnl_chattimeout.Visible = false;
                }

                tb_updateintervals.Text = DrDefaults["ChatTimeout"];
            }
            else {
                if (!(Roles.IsUserInRole(Username, ServerSettings.AdminUserName))) {
                    if (!Member.AdminChatControlled || HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        pnl_ChatClient.Enabled = true;
                        pnl_ChatClient.Visible = true;
                        tb_updateintervals.Text = Member.ChatTimeout.ToString();

                        if (Member.ChatEnabled) {
                            rb_chatclient_on.Checked = true;
                            rb_chatclient_off.Checked = false;
                            pnl_chattimeout.Enabled = true;
                            pnl_chattimeout.Visible = true;
                        }
                        else {
                            rb_chatclient_on.Checked = false;
                            rb_chatclient_off.Checked = true;
                            pnl_chattimeout.Enabled = false;
                            pnl_chattimeout.Visible = false;
                        }
                    }
                    else {
                        pnl_ChatClient.Enabled = false;
                        pnl_ChatClient.Visible = false;
                    }
                }
                else {
                    pnl_ChatClient.Enabled = true;
                    pnl_ChatClient.Visible = true;
                    tb_updateintervals.Text = Member.ChatTimeout.ToString();

                    if (Member.ChatEnabled) {
                        rb_chatclient_on.Checked = true;
                        rb_chatclient_off.Checked = false;
                        pnl_chattimeout.Enabled = true;
                        pnl_chattimeout.Visible = true;
                    }
                    else {
                        rb_chatclient_on.Checked = false;
                        rb_chatclient_off.Checked = true;
                        pnl_chattimeout.Enabled = false;
                        pnl_chattimeout.Visible = false;
                    }
                }
            }
        }
        else {
            pnl_ChatClient.Enabled = false;
            pnl_ChatClient.Visible = false;
        }
    }
    private void SetChatSoundNotiSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ChatSoundNoti"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ChatSoundNoti) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_chatsoundnoti_on.Checked = true;
            rb_chatsoundnoti_off.Checked = false;
        }
        else {
            rb_chatsoundnoti_on.Checked = false;
            rb_chatsoundnoti_off.Checked = true;
        }
    }

    protected void rb_chatclient_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("EnableChat", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateEnableChat(true);
        }

        LoadChatClient();
    }
    protected void rb_chatclient_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("EnableChat", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateEnableChat(false);
        }

        LoadChatClient();
    }

    protected void rb_chatsoundnoti_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ChatSoundNoti", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateChatSoundNoti(true);
        }

        LoadChatClient();
    }
    protected void rb_chatsoundnoti_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ChatSoundNoti", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateChatSoundNoti(false);
        }

        LoadChatClient();
    }

    protected void btn_updateintervals_Click(object sender, EventArgs e) {
        string timeout = tb_updateintervals.Text.Trim();
        int time = 10;
        if (!string.IsNullOrEmpty(timeout)) {
            int.TryParse(timeout, out time);
            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("ChatTimeout", time.ToString());
                UpdateDrDefaults();
            }
            else {
                Chat chat = new Chat(false);
                chat.Load_usertimeout_list();

                Member.UpdateChatTimeout(time.ToString());
            }
        }

        LoadChatClient();
    }

    #endregion

}
