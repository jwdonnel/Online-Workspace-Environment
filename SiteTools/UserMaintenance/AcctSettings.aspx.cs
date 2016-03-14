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
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            string controlName = HttpContext.Current.Request.Params["__EVENTTARGET"];
            if (string.IsNullOrEmpty(controlName)) {
                controlName = string.Empty;
            }

            BuildAppStyleList();
            LoadSettingsPage(controlName);

            HelperMethods.LoadSavedScrollToElementSession(Session, this.Page);
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
                            if (Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
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
                    if (Request.QueryString["u"].ToLower() == "newuserdefaults" && ServerSettings.AdminPagesCheck("GroupOrg", userId.Name) && HasGroupIdQuery()) {
                        LoadNewUserDefaults();
                    }
                    else if ((userId.Name.ToLower() == Request.QueryString["u"].ToLower()) || (ServerSettings.AdminPagesCheck("UserAccounts", userId.Name))) {
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
            LoadAppRemoteContainer();
            LoadIconSelector();
            LoadSiteCustomizations();
            LoadWorkspaceContainer();
            LoadChatClient();
            LoadUserAppOverrides();
        }
        else {
            InitNewDefaults();
        }
    }

    private void InitNewDefaults() {
        if (!string.IsNullOrEmpty(Request.QueryString["u"])) {
            switch (Request.QueryString["u"].ToLower()) {
                case "newuserdefaults":
                    NewDefaults = new NewUserDefaults(RoleSelect);
                    NewDefaults.DefaultTable = DrDefaults;
                    break;
                case "demouser":
                    if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                        NewDefaults = new NewUserDefaults(RoleSelect);
                        NewDefaults.DefaultTable = DrDefaults;
                    }
                    break;
            }
        }
    }

    private void LoadAllUserSettings(string userName) {
        Username = userName;

        PageLoadInit.BuildLinks(MainContent_pnlLinkBtns, Username, this.Page);

        _siteTheme = Member.SiteTheme;
        if (string.IsNullOrEmpty(_siteTheme)) {
            _siteTheme = "Standard";
        }

        DisableUserSettingsLoggedIntoGroup();
        AssociateWithGroups = _ss.AssociateWithGroups;

        if (userName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            pnl_DeleteAccount.Enabled = true;
            pnl_DeleteAccount.Visible = true;
        }

        btn_markasnewuser.Enabled = false;
        btn_markasnewuser.Visible = false;

        if (Roles.IsUserInRole(userName, ServerSettings.AdminUserName)) {
            pnl_EnableRecieveAll.Enabled = true;
            pnl_EnableRecieveAll.Visible = true;
            string u = GroupSessions.GetUserGroupSessionName(userName);
            if (!string.IsNullOrEmpty(u) && u.ToLower() != userName.ToLower()) {
                pnl_EnableRecieveAll.Enabled = false;
                pnl_EnableRecieveAll.Visible = false;
            }
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

        PageLoadInit.BuildLinks(MainContent_pnlLinkBtns, Username, this.Page);

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
        }
        else {
            pnl_passwordchange.Enabled = false;
            pnl_passwordchange.Visible = false;
        }

        pnl_EnableRecieveAll.Enabled = true;
        pnl_EnableRecieveAll.Visible = true;

        DisableUserSettingsLoggedIntoGroup();
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
            if (HasGroupIdQuery()) {
                SetGroupRoleSelect();
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + lbl_pageTitle.ClientID + "').html('New User Defaults for " + _ss.UserSignUpRole + "');");

                RoleSelect = _ss.UserSignUpRole;
                pnl_userRoleAssign.Enabled = false;
                pnl_userRoleAssign.Visible = false;
            }
        }
        else {
            if (HasGroupIdQuery()) {
                SetGroupRoleSelect();
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + lbl_pageTitle.ClientID + "').html('New User Defaults for " + RoleSelect + "');");

                RoleSelect = dd_roles.SelectedValue;
                pnl_userRoleAssign.Enabled = true;
                pnl_userRoleAssign.Visible = true;
            }
        }

        InitNewDefaults();
        NewDefaults.GetDefaults();
        DrDefaults = NewDefaults.DefaultTable;

        PageLoadInit.BuildLinks(MainContent_pnlLinkBtns, string.Empty, this.Page);

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + lbl_DateUpdated.ClientID + "').html('Last Updated: " + DrDefaults["DateUpdated"] + "');");

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
        pnl_WorkspaceOverlays.Enabled = true;
        pnl_WorkspaceOverlays.Visible = true;
        pnl_usercreds.Visible = false;
        pnl_userColor.Visible = false;
        pnl_NotificationSettings.Enabled = true;
        pnl_NotificationSettings.Visible = true;
        pnl_accountPrivacy.Enabled = false;
        pnl_accountPrivacy.Visible = false;
        pnl_clearUserProp.Enabled = false;
        pnl_clearUserProp.Visible = false;
        pnl_clearNoti.Enabled = false;
        pnl_clearNoti.Visible = false;
        btn_clearnoti.Enabled = false;
        btn_clearnoti.Visible = false;
        #endregion

        if (RoleSelect.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            pnl_adminpages_Holder.Enabled = false;
            pnl_adminpages_Holder.Visible = false;
        }

        if (HasGroupIdQuery()) {
            pnl_NotificationSettings.Enabled = true;
            pnl_NotificationSettings.Visible = true;
            pnl_WorkspaceOverlays.Enabled = true;
            pnl_WorkspaceOverlays.Visible = true;

            LoadNotificationsSettings();
            LoadWorkspaceOverlays();
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
        pnl_WorkspaceOverlays.Enabled = true;
        pnl_WorkspaceOverlays.Visible = true;
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
        pnl_MobileAutoSync.Enabled = false;
        pnl_MobileAutoSync.Visible = false;
        pnl_ShowSiteToolsInCategories.Enabled = false;
        pnl_ShowSiteToolsInCategories.Visible = false;
        #endregion

        PageLoadInit.BuildLinks(MainContent_pnlLinkBtns, string.Empty, this.Page);

        if (NewDefaults.DefaultTable.Count > 0) {
            DrDefaults = NewDefaults.DefaultTable;
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + lbl_DateUpdated.ClientID + "').html('Last Updated: " + DrDefaults["DateUpdated"] + "');");
        }
    }

    private void DisableUserSettingsLoggedIntoGroup() {
        string currentUser = HttpContext.Current.User.Identity.Name.ToLower();
        if (!string.IsNullOrEmpty(Request.QueryString["u"]) && Request.QueryString["u"].ToLower() != currentUser) {
            return;
        }

        string u = GroupSessions.GetUserGroupSessionName(currentUser);
        if (u != currentUser) {
            UserGroupLoginMessage.Text = "<div class='clear-space'></div>You are currently logged into a group. Some settings will be disabled until you log out of that group.<div class='clear-space'></div><div class='clear-space'></div>";
            pnl_NotificationSettings.Enabled = false;
            pnl_NotificationSettings.Visible = false;
            pnl_WorkspaceOverlays.Enabled = false;
            pnl_WorkspaceOverlays.Visible = false;
            pnl_UserAppOverrides.Enabled = false;
            pnl_UserAppOverrides.Visible = false;
            pnl_WorkspaceContainer.Enabled = false;
            pnl_WorkspaceContainer.Visible = false;
            pnl_BackgroundEditor.Enabled = false;
            pnl_BackgroundEditor.Visible = false;
            pnl_IconSelector.Enabled = false;
            pnl_IconSelector.Visible = false;
            pnl_SiteCustomizations.Enabled = false;
            pnl_SiteCustomizations.Visible = false;
            pnl_ChatClient.Enabled = false;
            pnl_ChatClient.Visible = false;
            pnl_AppRemoteContainer.Enabled = false;
            pnl_AppRemoteContainer.Visible = false;
        }
    }

    private bool HasGroupIdQuery() {
        Groups tempGroups = new Groups();
        string tempUser = HttpContext.Current.User.Identity.Name.ToLower();
        if (!string.IsNullOrEmpty(Request.QueryString["groupid"]) && (tempGroups.GetOwner(Request.QueryString["groupid"]).ToLower() == tempUser || tempUser == ServerSettings.AdminUserName.ToLower())) {
            return true;
        }
        return false;
    }
    private void SetGroupRoleSelect() {
        RoleSelect = Request.QueryString["groupid"];
        string groupName = new Groups().GetGroupName_byID(RoleSelect);
        if (string.IsNullOrEmpty(groupName)) {
            Page.Response.Redirect("~/ErrorPages/Error.html");
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#" + lbl_pageTitle.ClientID + "').html('User Defaults for the Group " + groupName + "');");

            pnl_UserInformation.Enabled = false;
            pnl_UserInformation.Visible = false;
            pnl_lockappicons.Enabled = false;
            pnl_lockappicons.Visible = false;
            non_grouplogin.Enabled = false;
            non_grouplogin.Visible = false;
            grouplogin.Enabled = true;
            grouplogin.Visible = true;
        }
    }

    private void AdminVersion() {
        pnl_ChatClient.Enabled = false;
        pnl_ChatClient.Visible = false;
        pnl_WorkspaceContainer.Enabled = false;
        pnl_WorkspaceContainer.Visible = false;

        pnl_demoPackage.Enabled = false;
        pnl_demoPackage.Visible = false;
        pnl_WorkspaceMode.Enabled = false;
        pnl_WorkspaceMode.Visible = false;
        pnl_MobileAutoSync.Enabled = false;
        pnl_MobileAutoSync.Visible = false;
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
        pnl_UserAppOverrides.Enabled = false;
        pnl_UserAppOverrides.Visible = false;

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
        pnl_defaultLoginGroup.Enabled = false;
        pnl_defaultLoginGroup.Visible = false;
        pnl_adminpages_Holder.Enabled = false;
        pnl_adminpages_Holder.Visible = false;
        pnl_userRoleAssign.Enabled = false;
        pnl_userRoleAssign.Visible = false;
        pnl_accountPrivacy.Enabled = false;
        pnl_accountPrivacy.Visible = false;

        lbl_appmodalstyle_title.InnerHtml = "Modal Window Style";
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

    private void BuildAppStyleList() {
        if (!IsPostBack) {
            Array array = Enum.GetValues(typeof(MemberDatabase.AppStyle));
            dd_appStyle.Items.Clear();
            for (int i = 0; i < array.Length; i++) {
                string value = array.GetValue(i).ToString();
                dd_appStyle.Items.Add(new ListItem(value.Replace("_", " "), value));
            }
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

            if ((!string.IsNullOrEmpty(Username) && !Roles.IsUserInRole(Username, ServerSettings.AdminUserName))
                || (string.IsNullOrEmpty(Username) && RoleSelect.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                BuildAdminPageEditor();
            }
            if (ServerSettings.AdminPagesCheck("GroupOrg", Username) || Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                if (Username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    pnl_groupEditor.Enabled = true;
                    pnl_groupEditor.Visible = true;
                    BuildGroupsEditor();
                }
            }
            else {
                pnl_groupEditor.Enabled = false;
                pnl_groupEditor.Visible = false;
            }

            if (!ServerSettings.AdminPagesCheck("Workspace", Username) && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                pnl_WorkspaceContainer.Enabled = false;
                pnl_WorkspaceContainer.Visible = false;
                pnl_WorkspaceMode.Enabled = false;
                pnl_WorkspaceMode.Visible = false;
                pnl_ShowAppTitle.Enabled = false;
                pnl_ShowAppTitle.Visible = false;
                pnl_showAppImage.Enabled = false;
                pnl_showAppImage.Visible = false;
                pnl_presentationMode.Enabled = false;
                pnl_presentationMode.Visible = false;
                pnl_clearproperties.Enabled = false;
                pnl_clearproperties.Visible = false;
                pnl_clearUserProp.Enabled = false;
                pnl_clearUserProp.Visible = false;
                pnl_ShowWorkspaceNum.Enabled = false;
                pnl_ShowWorkspaceNum.Visible = false;
                pnl_nonadminsettings.Enabled = false;
                pnl_nonadminsettings.Visible = false;
                pnl_backgroundurl.Enabled = false;
                pnl_backgroundurl.Visible = false;
            }

            BuildRolesDropDownList();
            AssignRolesDropDownList();
            LoadPrivacySettings();

            if (DrDefaults.Count > 0 && string.IsNullOrEmpty(Member.Username)) {
                userRoleAssign_text.InnerHtml = "User Role to Edit";
                userRoleAssign_tip_text.InnerHtml = "Change this to the role you wish to edit.";
            }

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
                if (File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + Member.UserId + "/" + acctImg)) {
                    imgAcctImage.ImageUrl = ServerSettings.AccountImageLoc + Member.UserId + "/" + acctImg;
                }
                else {
                    imgAcctImage.ImageUrl = ResolveUrl("~/Standard_Images/EmptyUserImg.png");
                }
            }
        }
    }
    private void SetUserImage() {
        string acctImg = Member.AccountImage;
        StringBuilder _strScriptreg = new StringBuilder();
        string mergedName = HelperMethods.MergeFMLNames(Member);
        _strScriptreg.Append("$('#lbl_UserName').html(\"" + UserImageColorCreator.CreateImgColorTopBar(acctImg, Member.UserColor, Member.UserId, mergedName, _siteTheme, Member.ProfileLinkStyle) + "\");");
        if (!string.IsNullOrEmpty(acctImg)) {
            if (acctImg.StartsWith("http://") || acctImg.StartsWith("https://")) {
                _strScriptreg.Append("$('#img_Profile').attr('src', '" + acctImg + "');");
            }
            else {
                _strScriptreg.Append("$('#img_Profile').attr('src', '" + ResolveUrl(ServerSettings.AccountImageLoc + Member.UserId + "/" + acctImg) + "');");
            }
        }
        else {
            _strScriptreg.Append("$('#img_Profile').attr('src', '" + ResolveUrl("~/Standard_Images/EmptyUserImg.png") + "');");
        }

        _strScriptreg.Append("$('#img_Profile').show();");
        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        UserImageColorCreator.ApplyProfileLinkStyle(Member.ProfileLinkStyle, Member.UserColor, this.Page);
    }
    private void BuildAdminPageEditor() {
        if (RoleSelect.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            pnl_adminpages_Holder.Enabled = true;
            pnl_adminpages_Holder.Visible = true;

            pnl_adminpages.Controls.Clear();
            var str = new StringBuilder();
            var str2 = new StringBuilder();

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
                    str.Append("<span class='img-collapse-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 3px;'></span></a>");
                    str.Append("</div></div>");
                }
                else {
                    str2.Append("<div class='float-left pad-all adminpageedit'>");
                    str2.Append("<div class='float-left pad-top-sml'>" + page);
                    str2.Append("<a href='#' onclick='addAdminPage(\"" + page + "\");return false;' class='RandomActionBtns float-left' title='Add " + page + "'>");
                    str2.Append("<span class='img-expand-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 3px;'></span></a>");
                    str2.Append("</div></div>");
                }
            }

            pnl_adminpages.Controls.Add(new LiteralControl(HelperMethods.TableAddRemove(str2.ToString(), str.ToString(), "Blocked", "Allowed", false, true)));
        }
        else {
            pnl_adminpages_Holder.Enabled = false;
            pnl_adminpages_Holder.Visible = false;
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

    #region Build Groups

    private void BuildGroupsEditor() {
        pnl_groups.Controls.Clear();

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(Username)) {
            try {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(Username);
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    pnl_groupEditor.Enabled = false;
                    pnl_groupEditor.Visible = false;
                }
            }
            catch { }
        }

        var str = new StringBuilder();
        var str2 = new StringBuilder();

        string[] groupsRegister = { };

        if (DrDefaults.Count > 0) {
            groupsRegister = DrDefaults["Groups"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        }
        else
            groupsRegister = Member.GetCompleteUserGroupList.ToArray();

        string defaultGroupLogin = string.Empty;
        if (DrDefaults.Count > 0 && DrDefaults.ContainsKey("DefaultLoginGroup")) {
            defaultGroupLogin = DrDefaults["DefaultLoginGroup"];
        }
        else {
            defaultGroupLogin = Member.DefaultLoginGroup;
        }

        Groups groups = new Groups();
        groups.getEntries();
        List<Dictionary<string, string>> groupList = groups.group_dt;
        List<Dictionary<string, string>> apartOfList = new List<Dictionary<string, string>>();

        foreach (Dictionary<string, string> dr in groupList) {
            string g = dr["GroupName"];
            string gid = dr["GroupID"];

            string imgUrl = string.Empty;
            if (HelperMethods.ConvertBitToBoolean(dr["IsURL"])) {
                imgUrl = dr["Image"];
                if (imgUrl.StartsWith("~/")) {
                    imgUrl = ResolveUrl(imgUrl);
                }
            }
            else if (!string.IsNullOrEmpty(dr["Image"])) {
                imgUrl = ResolveUrl("~/Standard_Images/Groups/Logo/" + dr["Image"]);
            }

            bool inGroup = groupsRegister.Contains(gid);
            if (DrDefaults.Count == 0)
                inGroup = groupsRegister.Contains(gid);

            if (inGroup) {
                str.Append("<div class='float-left pad-all groupedit'>");
                str.Append("<div class='float-left pad-top-sml'><img alt='' src='" + imgUrl + "' class='group-img-edit' />");
                if (groupsRegister.Length > 1 || Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                    str.Append("<a href='#' onclick='removeGroup(\"" + gid + "\");return false;' class='RandomActionBtns float-left' title='Remove " + g + "'>");
                    str.Append("<span class='img-collapse-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 3px;'></span></a>");
                }
                str.Append(g);
                str.Append("</div></div>");
                apartOfList.Add(dr);
            }
            else {
                str2.Append("<div class='float-left pad-all groupedit'>");
                str2.Append("<div class='float-left pad-top-sml'><img alt='' src='" + imgUrl + "' class='group-img-edit' />");
                str2.Append("<a href='#' onclick='addGroup(\"" + gid + "\");return false;' class='RandomActionBtns float-left' title='Add " + g + "'>");
                str2.Append("<span class='img-expand-sml cursor-pointer float-left' style='margin-right: 5px; margin-top: 3px;'></span></a>");
                str2.Append(g);
                str2.Append("</div></div>");
            }
        }

        ddl_defaultGroupLogin.Items.Clear();
        ddl_defaultGroupLogin.Items.Add(new ListItem("", ""));
        foreach (Dictionary<string, string> dr in apartOfList) {
            ListItem item = new ListItem(dr["GroupName"], dr["GroupID"]);
            if (!ddl_defaultGroupLogin.Items.Contains(item)) {
                ddl_defaultGroupLogin.Items.Add(item);
            }
        }

        foreach (ListItem ddlItem in ddl_defaultGroupLogin.Items) {
            if (ddlItem.Value == defaultGroupLogin) {
                ddlItem.Selected = true;
                break;
            }
        }

        string table = HelperMethods.TableAddRemove(str2.ToString(), str.ToString(), "Groups Available", "Associated Groups", false, true);
        pnl_groups.Controls.Add(new LiteralControl(table));
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
                    ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_UserInformation");
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

    protected void btn_defaultGroupLogin_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("DefaultLoginGroup", ddl_defaultGroupLogin.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateDefaultLoginGroup(ddl_defaultGroupLogin.SelectedValue);
        }

        LoadUserInformation();
    }

    #endregion

    protected void OnContinueButtonClick(object sender, EventArgs e) {
        ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_UserInformation");
    }

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
                AppLog.AddError(ex);
            }
        }

        ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_UserInformation");
    }

    protected void btn_clear_acctImage_Clicked(object sender, EventArgs e) {
        try {
            string imgPath = ServerSettings.AccountImageServerLoc + Member.UserId;
            if (Directory.Exists(imgPath)) {
                string[] fileList = Directory.GetFiles(imgPath);
                foreach (string file in fileList) {
                    File.Delete(file);
                }
            }

            Member.UpdateAcctImage(string.Empty);
            string defaultImg = ResolveUrl("~/Standard_Images/EmptyUserImg.png");

            RegisterPostbackScripts.RegisterStartupScript(this, "$('#img_Profile').hide();$('#imgAcctImage').attr('src','" + defaultImg + "');");
            SetUserImage();
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
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

    protected void dd_roles_Changed(object sender, EventArgs e) {
        if (DrDefaults.Count == 0) {
            string user = Request.QueryString["u"];
            if (Roles.IsUserInRole(Username, RoleSelect)) {
                Roles.RemoveUserFromRole(Username, RoleSelect);
            }

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

        string roles = "";
        for (int i = 0; i < Roles.GetRolesForUser(Username).Length; i++) {
            roles += Roles.GetRolesForUser(Username)[i];
            if (i < Roles.GetRolesForUser(Username).Length - 1) {
                roles += ", ";
            }
        }
        string textTitle = Username + " Customizations: " + roles + " Role";
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#lbl_pageTitle').html(\"" + textTitle + "\");");

        LoadSettingsPage("hf_UpdateAll");
    }

    protected void btn_markasnewuser_Clicked(object sender, EventArgs e) {
        string user = Request.QueryString["u"];
        var msu = new MemberDatabase(user);
        msu.UpdateIsNewMember(true);
        LoadUserInformation();
    }

    protected void rb_Privacy_on_CheckedChanged(object sender, EventArgs e) {
        Member.UpdatePrivateAccount(true);
        LoadUserInformation();
    }
    protected void rb_Privacy_off_CheckedChanged(object sender, EventArgs e) {
        Member.UpdatePrivateAccount(false);
        LoadUserInformation();
    }

    protected void hf_DeleteUserAccount_ValueChanged(object sender, EventArgs e) {
        string user = HttpContext.Current.User.Identity.Name;
        if (user.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            Membership.DeleteUser(user, true);
            MemberDatabase m = new MemberDatabase(user);
            m.DeleteUserCustomizations(user);
            FormsAuthentication.SignOut();
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
    }

    #endregion


    #region -- Notifications Settings --

    private void LoadNotificationsSettings() {
        if (pnl_NotificationSettings.Enabled && pnl_NotificationSettings.Visible) {
            ReceiveAllSettings();
            LoadNotifications();

            updatepnl_NotificationSettings.Update();
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
        if (DrDefaults.Count > 0 && DrDefaults.ContainsKey("AppPackage")) {
            string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            _notifications.GetUserNotifications(tempUsername);
            apps = new App(tempUsername);
            userApps = new AppPackages(false).GetAppList(DrDefaults["AppPackage"]).ToList();
        }

        LoadedList.Clear();
        countEnabled = 0;
        count = 0;

        if (DrDefaults.Count == 0) {
            if (Member.ReceiveAll) {
                Notifications_Coll coll3 = _notifications.GetNotification("1159aca6-2449-4aff-bacb-5f29e479e2d7");
                if (!string.IsNullOrEmpty(coll3.ID))
                    str = AddRow(str, coll3, "1159aca6-2449-4aff-bacb-5f29e479e2d7", emailOn);
            }

            if (Roles.IsUserInRole(Username, ServerSettings.AdminUserName)) {
                Notifications_Coll coll1 = _notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
                if (!string.IsNullOrEmpty(coll1.ID))
                    str = AddRow(str, coll1, "236a9dc9-c92a-437f-8825-27809af36a3f", emailOn);
            }
        }
        else {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ReceiveAll"])) {
                Notifications_Coll coll3 = _notifications.GetNotification("1159aca6-2449-4aff-bacb-5f29e479e2d7");
                if (!string.IsNullOrEmpty(coll3.ID))
                    str = AddRow(str, coll3, "1159aca6-2449-4aff-bacb-5f29e479e2d7", emailOn);
            }

            if (RoleSelect == ServerSettings.AdminUserName) {
                Notifications_Coll coll1 = _notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
                if (!string.IsNullOrEmpty(coll1.ID))
                    str = AddRow(str, coll1, "236a9dc9-c92a-437f-8825-27809af36a3f", emailOn);
            }
        }

        Notifications_Coll coll2 = _notifications.GetNotification("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f");
        if (!string.IsNullOrEmpty(coll2.ID) && Username.ToLower() != ServerSettings.AdminUserName.ToLower())
            str = AddRow(str, coll2, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", emailOn);

        if (Username != ServerSettings.AdminUserName.ToLower()) {
            Notifications_Coll coll4 = _notifications.GetNotification("707ecc6c-2480-4080-bad6-fb135bb5cf13");
            if (!string.IsNullOrEmpty(coll4.ID) && Username.ToLower() != ServerSettings.AdminUserName.ToLower())
                str = AddRow(str, coll4, "707ecc6c-2480-4080-bad6-fb135bb5cf13", emailOn);
        }

        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string notifiId = table.NotificationID;
                if (!string.IsNullOrEmpty(notifiId)) {

                    if (AssociateWithGroups && DrDefaults.Count == 0) {
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
        if (count == 0) {
            btn_DisableAll_notification.Style["display"] = "none";
            str.Append("<div class='emptyGridView'>No Notifications Available</div>");
        }
        else {
            btn_DisableAll_notification.Style["display"] = "";
            if (countEnabled == 0) {
                btn_DisableAll_notification.Text = "Enable All";
            }
            else {
                btn_DisableAll_notification.Text = "Disable All";
            }
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
            string disabled = "disabled='disabled'";
            if (active) {
                disabled = "";
            }

            string cb = "<input type='checkbox' value='1' onchange='UpdateEmail_notification(this, \"" + coll.ID + "\")'" + _checked + " title='Turn on/off' " + disabled + " />";
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

            string tempUsername = Username;
            if (DrDefaults.Count > 0) {
                tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            }

            _notifications.UpdateUserNotificationEmail(hf_collId_notification.Value, _email, tempUsername);
        }

        hf_collId_notification.Value = "";
        hf_updateEmail_notification.Value = "";
        LoadNotificationsSettings();
    }

    protected void hf_updateEnabled_notification_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateEnabled_notification.Value)) {
            string tempUsername = Username;
            if (DrDefaults.Count > 0) {
                tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            }

            if (!_notifications.IsUserNotificationEnabled(tempUsername, hf_updateEnabled_notification.Value)) {
                Notifications_Coll tempColl = _notifications.GetNotification(hf_updateEnabled_notification.Value);
                if (!string.IsNullOrEmpty(tempColl.ID))
                    _notifications.AddUserNotification(tempUsername, hf_updateEnabled_notification.Value, true);
            }
        }

        hf_updateEnabled_notification.Value = "";

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#notifications_tab').show();");
        }

        LoadNotificationsSettings();
    }

    protected void hf_updateDisabled_notification_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateDisabled_notification.Value)) {
            string tempUsername = Username;
            if (DrDefaults.Count > 0) {
                tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            }

            if (_notifications.IsUserNotificationEnabled(tempUsername, hf_updateDisabled_notification.Value))
                _notifications.DeleteUserNotification(tempUsername, hf_updateDisabled_notification.Value);
        }

        hf_updateDisabled_notification.Value = "";

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            int total = 0;
            UserNotificationMessages _usernoti = new UserNotificationMessages(Username);
            _usernoti.getEntries("ASC");

            Notifications noti = new Notifications();

            foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                Notifications_Coll coll = noti.GetNotification(x.NotificationID);
                if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!string.IsNullOrEmpty(x.ID))) {
                    total++;
                }
            }

            noti.GetUserNotifications(Username);
            if (total == 0 && noti.UserNotifications.Count == 0) {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#notifications_tab').hide();");
            }
        }

        LoadNotificationsSettings();
    }

    protected void btn_DisableAll_notification_Clicked(object sender, EventArgs e) {
        string tempUsername = Username;
        List<string> userApps = Member.EnabledApps;
        if (DrDefaults.Count > 0 && DrDefaults.ContainsKey("AppPackage")) {
            userApps = new AppPackages(false).GetAppList(DrDefaults["AppPackage"]).ToList();
            tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
        }

        _notifications.GetUserNotifications(tempUsername);

        if (btn_DisableAll_notification.Text == "Enable All") {
            App _apps = new App(tempUsername);

            if (DrDefaults.Count == 0) {
                if (Roles.IsUserInRole(Username, ServerSettings.AdminUserName)) {
                    _notifications.AddUserNotification(tempUsername, "236a9dc9-c92a-437f-8825-27809af36a3f", true);
                }

                if (Member.ReceiveAll) {
                    _notifications.AddUserNotification(tempUsername, "1159aca6-2449-4aff-bacb-5f29e479e2d7", true);
                }
            }
            else {
                if (RoleSelect == ServerSettings.AdminUserName) {
                    _notifications.AddUserNotification(tempUsername, "236a9dc9-c92a-437f-8825-27809af36a3f", true);
                }

                if (HelperMethods.ConvertBitToBoolean(DrDefaults["ReceiveAll"])) {
                    _notifications.AddUserNotification(tempUsername, "1159aca6-2449-4aff-bacb-5f29e479e2d7", true);
                }
            }

            _notifications.AddUserNotification(tempUsername, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", true);

            if (Username != ServerSettings.AdminUserName.ToLower()) {
                _notifications.AddUserNotification(tempUsername, "707ecc6c-2480-4080-bad6-fb135bb5cf13", true);
            }

            foreach (string w in userApps) {
                var table = _apps.GetAppInformation(w);
                if (table != null) {
                    string notifiId = table.NotificationID;
                    if (!string.IsNullOrEmpty(notifiId)) {
                        string[] notifiArray = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string notifiId_temp in notifiArray) {
                            Notifications_Coll coll = _notifications.GetNotification(notifiId_temp);
                            if (!string.IsNullOrEmpty(coll.NotificationName))
                                _notifications.AddUserNotification(tempUsername, notifiId_temp, true);
                        }
                    }
                }
            }
        }
        else {
            foreach (UserNotifications_Coll coll in _notifications.UserNotifications) {
                _notifications.DeleteUserNotification(tempUsername, coll.NotificationID);
            }
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            int total = 0;
            UserNotificationMessages _usernoti = new UserNotificationMessages(Username);
            _usernoti.getEntries("ASC");

            Notifications noti = new Notifications();

            foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                Notifications_Coll coll = noti.GetNotification(x.NotificationID);
                if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!string.IsNullOrEmpty(x.ID))) {
                    total++;
                }
            }

            if (total == 0) {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#notifications_tab').hide();");
            }
        }

        LoadNotificationsSettings();
    }

    protected void btn_clearnoti_Clicked(object sender, EventArgs e) {
        UserNotificationMessages noti = new UserNotificationMessages(Username);
        noti.deleteAllUserNotification();
        LoadNotificationsSettings();
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
    }

    #endregion


    #region -- Workspace Overlays --

    private void LoadWorkspaceOverlays() {
        if (pnl_WorkspaceOverlays.Enabled && pnl_WorkspaceOverlays.Visible) {
            LoadHideAllOverlaySettings();
            LoadOverlays();

            updatepnl_WorkspaceOverlays.Update();
        }
    }

    private void LoadHideAllOverlaySettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["HideAllOverlays"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.HideAllOverlays) {
                settingOn = true;
            }
        }
        if (settingOn) {
            rb_hidealloverlays_on.Checked = true;
            rb_hidealloverlays_off.Checked = false;
            pnl_useroverlaylist.Enabled = false;
            pnl_useroverlaylist.Visible = false;
        }
        else {
            rb_hidealloverlays_on.Checked = false;
            rb_hidealloverlays_off.Checked = true;
            pnl_useroverlaylist.Enabled = true;
            pnl_useroverlaylist.Visible = true;
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
        if (DrDefaults.Count > 0 && DrDefaults.ContainsKey("AppPackage")) {
            string tempRoleSelect = NewUserDefaults.GetRoleID(RoleSelect);
            _workspaceOverlays.GetUserOverlays(tempRoleSelect);
            userApps = new AppPackages(false).GetAppList(DrDefaults["AppPackage"]).ToList();
        }

        int total = 0;
        int count = 1;
        int countEnabled = 0;
        List<string> LoadedList = new List<string>();

        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string overlayId = table.OverlayID;
                if (!string.IsNullOrEmpty(overlayId)) {

                    if (AssociateWithGroups && DrDefaults.Count == 0) {
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
                            total++;
                        }
                    }
                }
            }
        }

        str.Append("</tbody></table></div>");

        if (total == 0) {
            str.Append("<div class='emptyGridView'>No overlays available</div>");
        }

        if (countEnabled == 0) {
            btn_DisableAll_overlay.Text = "Enable All";
        }
        else {
            btn_DisableAll_overlay.Text = "Disable All";
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

    protected void rb_hidealloverlays_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideAllOverlays", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideAllOverlays(true);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_WorkspaceOverlays");
        }

        LoadWorkspaceOverlays();
    }
    protected void rb_hidealloverlays_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideAllOverlays", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideAllOverlays(false);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_WorkspaceOverlays");
        }

        LoadWorkspaceOverlays();
    }

    protected void hf_updateEnabled_overlay_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateEnabled_overlay.Value)) {
            string tempUsername = Username;
            if (DrDefaults.Count > 0) {
                tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            }

            if (!_workspaceOverlays.IsUserOverlayEnabled(tempUsername, hf_updateEnabled_overlay.Value)) {
                WorkspaceOverlay_Coll tempColl = _workspaceOverlays.GetWorkspaceOverlay(hf_updateEnabled_overlay.Value);
                _workspaceOverlays.AddUserOverlay(tempUsername, hf_updateEnabled_overlay.Value);
            }
        }

        hf_updateEnabled_overlay.Value = "";
        LoadWorkspaceOverlays();
    }

    protected void hf_updateDisabled_overlay_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateDisabled_overlay.Value)) {
            string tempUsername = Username;
            if (DrDefaults.Count > 0) {
                tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            }

            if (_workspaceOverlays.IsUserOverlayEnabled(tempUsername, hf_updateDisabled_overlay.Value))
                _workspaceOverlays.DeleteUserOverlay(tempUsername, hf_updateDisabled_overlay.Value);
        }

        hf_updateDisabled_overlay.Value = "";
        LoadWorkspaceOverlays();
    }

    protected void btn_DisableAll_overlay_Clicked(object sender, EventArgs e) {
        string tempUsername = Username;
        if (DrDefaults.Count > 0) {
            tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
        }

        if (btn_DisableAll_overlay.Text == "Enable All") {
            List<string> userApps = Member.EnabledApps;
            if (DrDefaults.Count > 0 && DrDefaults.ContainsKey("AppPackage")) {
                string tempRoleSelect = NewUserDefaults.GetRoleID(RoleSelect);
                _workspaceOverlays.GetUserOverlays(tempRoleSelect);
                userApps = new AppPackages(false).GetAppList(DrDefaults["AppPackage"]).ToList();
            }

            var apps = new App(tempUsername);
            List<string> LoadedList = new List<string>();
            foreach (string w in userApps) {
                var table = apps.GetAppInformation(w);
                if (table != null) {
                    string overlayId = table.OverlayID;
                    if (!string.IsNullOrEmpty(overlayId)) {

                        if (AssociateWithGroups && DrDefaults.Count == 0) {
                            if (!ServerSettings.CheckAppGroupAssociation(table, Member)) {
                                continue;
                            }
                        }

                        string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string oId in oIds) {
                            WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                            if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                                LoadedList.Add(oId);
                                _workspaceOverlays.AddUserOverlay(tempUsername, coll.ID);
                            }
                        }
                    }
                }
            }
        }
        else {
            _workspaceOverlays.GetUserOverlays(tempUsername);
            foreach (UserOverlay_Coll coll in _workspaceOverlays.UserOverlays) {
                _workspaceOverlays.DeleteUserOverlay(tempUsername, coll.OverlayID);
            }
        }
        LoadWorkspaceOverlays();
    }

    #endregion


    #region -- User App Overrides --

    private void LoadUserAppOverrides() {
        if (pnl_UserAppOverrides.Enabled && pnl_UserAppOverrides.Visible) {
            LoadAppList();

            updatepnl_UserAppOverrides.Update();
        }
    }
    private void LoadAppList() {
        pnl_UserAppOverrideList.Controls.Clear();
        StringBuilder str = new StringBuilder();
        StringBuilder strJavascript = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td>");
        str.Append("<td style='width: 150px;'>App Name</td>");
        if (!_ss.HideAllAppIcons && !rb_hideAppIcon_on.Checked) {
            str.Append("<td style='width: 45px;'>Image</td>");
        }
        else {
            str.Append("<td style='width: 45px;'>Color</td>");
        }

        str.Append("<td>Description</td>");
        str.Append("<td width='75px'>Actions</td></tr>");

        var apps = new App(string.Empty);
        UserAppSettings appSettings = new UserAppSettings(Username, true);

        List<string> userApps = new List<string>();
        if (DrDefaults.Count > 0) {
            string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            appSettings = new UserAppSettings(tempUsername, true);
            userApps = new AppPackages(false).GetAppList(DrDefaults["AppPackage"]).ToList();
        }
        else {
            appSettings = new UserAppSettings(Username, true);
            userApps = Member.EnabledApps;
        }

        int total = 0;
        int count = 1;
        int countEnabled = 0;
        List<string> LoadedList = new List<string>();

        AppRatings ratings = new AppRatings();
        appSettings.BuildUserAppSettings();

        foreach (string w in userApps) {
            var coll = apps.GetAppInformation(w);
            if (coll != null) {
                if (!coll.AllowUserOverrides) {
                    continue;
                }

                string editBtn = "<a href='#edit' class='td-edit-btn' onclick=\"EditOverrides('" + coll.AppId + "');return false;\" title='Edit Overrides'></a>";
                string removeBtn = "";

                bool hasOverride = appSettings.HasOverrides(coll.AppId, coll.AllowUserOverrides);
                if (hasOverride) {
                    countEnabled++;
                    removeBtn = "<a href='#remove' class='td-cancel-btn margin-left' onclick=\"DeleteOverrides('" + coll.AppId + "');return false;\" title='Delete Overrides'></a>";
                }

                string description = coll.Description;
                if (!string.IsNullOrEmpty(description)) {
                    description += " - ";
                }
                description += coll.About;

                string appColorCss = string.Empty;
                if (!string.IsNullOrEmpty(coll.AppBackgroundColor) && coll.AppBackgroundColor.ToLower() != "inherit") {
                    string appColor = coll.AppBackgroundColor;

                    if (hasOverride) {
                        Dictionary<string, string> appSettingColl = appSettings.GetUserAppSettings(coll.AppId);
                        appColor = appSettingColl["AppBackgroundColor"];
                    }

                    if (!string.IsNullOrEmpty(appColor)) {
                        if (!appColor.StartsWith("#")) {
                            appColor = "#" + appColor;
                        }

                        string fontColor = "color: #515151;";
                        if (!HelperMethods.UseDarkTextColorWithBackground(appColor)) {
                            fontColor = "color: #FFFFFF;";
                        }

                        appColorCss = " style=\"" + fontColor + "background: " + appColor + ";\"";
                    }
                }

                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + count.ToString() + "</td>");
                str.Append("<td align='left' class='border-right border-bottom'" + appColorCss + ">" + coll.AppName + "</td>");

                string iconColor = coll.IconBackgroundColor;
                if (hasOverride) {
                    Dictionary<string, string> appSettingColl = appSettings.GetUserAppSettings(coll.AppId);
                    iconColor = appSettingColl["IconBackgroundColor"];
                }

                if (!iconColor.StartsWith("#")) {
                    iconColor = "#" + iconColor;
                }

                if (!_ss.HideAllAppIcons && !rb_hideAppIcon_on.Checked) {
                    string img = "<img alt='' src='" + ResolveUrl("~/Standard_Images/App_Icons/" + coll.Icon) + "' style='height: 25px;' />";
                    str.Append("<td align='center' class='border-right border-bottom' style='" + HelperMethods.GetCSSGradientStyles(iconColor, string.Empty) + "'>" + img + "</td>");
                }
                else {
                    str.Append("<td align='center' class='border-right border-bottom' style='" + HelperMethods.GetCSSGradientStyles(iconColor, string.Empty) + "'></td>");
                }

                str.Append("<td align='left' class='border-right border-bottom'>" + description + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + editBtn + removeBtn + "</td></tr>");
                total++;
                count++;
            }
        }

        str.Append("</tbody></table></div>");

        if (total == 0) {
            str.Append("<div class='emptyGridView'>No apps available</div>");
        }

        if (countEnabled > 0) {
            lbtn_DeleteAllOverrides.Visible = true;
        }
        else {
            lbtn_DeleteAllOverrides.Visible = false;
        }

        lbl_TotalOverridenApps.Text = countEnabled.ToString();
        pnl_UserAppOverrideList.Controls.Add(new LiteralControl(str.ToString()));
    }

    protected void hf_DeleteUserAppOverrides_ValueChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            UserAppSettings userAppSettings = new UserAppSettings(tempUsername, true);
            userAppSettings.DeleteAllAppSetting();
        }
        else {
            UserAppSettings userAppSettings = new UserAppSettings(Username, true);
            userAppSettings.DeleteAllAppSetting();
        }

        LoadUserAppOverrides();

        if (DrDefaults.Count == 0) {
            AppIconBuilder aib = new AppIconBuilder(Page, Member);
            aib.BuildAppsForUser();
        }
    }
    protected void hf_EditUserAppOverrides_ValueChanged(object sender, EventArgs e) {
        string id = hf_EditUserAppOverrides.Value;
        BuildEditAppBox(id);
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'App-element', 'App Overrides');");
        hf_EditUserAppOverrides.Value = string.Empty;

        LoadUserAppOverrides();
    }
    protected void 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_ValueChanged(object sender, EventArgs e) {
        string id = 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.Value;

        if (DrDefaults.Count > 0) {
            string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
            UserAppSettings userAppSettings = new UserAppSettings(tempUsername, true);
            userAppSettings.DeleteAppSetting(id);
        }
        else {
            UserAppSettings userAppSettings = new UserAppSettings(Username, true);
            userAppSettings.DeleteAppSetting(id);
        }

        BuildEditAppBox(id);
        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.Value = string.Empty;

        LoadUserAppOverrides();

        if (DrDefaults.Count == 0) {
            AppIconBuilder aib = new AppIconBuilder(Page, Member);
            aib.BuildAppsForUser();
        }
    }
    protected void hf_UpdateUserAppOverrides_ValueChanged(object sender, EventArgs e) {
        string id = hf_UpdateUserAppOverrides.Value;
        if (!string.IsNullOrEmpty(id)) {
            UserAppSettings userAppSettings;
            if (DrDefaults.Count > 0) {
                string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
                userAppSettings = new UserAppSettings(tempUsername, true);
            }
            else {
                userAppSettings = new UserAppSettings(Username, true);
            }


            string category = string.Empty;
            foreach (ListItem item in dd_category_edit.Items) {
                if (item.Selected) {
                    category += item.Value + ServerSettings.StringDelimiter;
                }
            }

            string minWidth = tb_minwidth_edit.Text.Trim();
            string minHeight = tb_minheight_edit.Text.Trim();
            bool autoFullScreen = HelperMethods.ConvertBitToBoolean(dd_maxonload_edit.SelectedValue);
            bool allowResize = HelperMethods.ConvertBitToBoolean(dd_allowresize_edit.SelectedValue);
            string cssClass = dd_enablebg_edit.SelectedValue;
            string defaultWorkspace = dd_defaultworkspace_edit.SelectedValue;
            bool allowMaximize = HelperMethods.ConvertBitToBoolean(dd_allowmax_edit.SelectedValue);
            bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_edit.SelectedValue);
            bool allowPopOut = HelperMethods.ConvertBitToBoolean(dd_allowpopout_edit.SelectedValue);
            string popOutLoc = tb_allowpopout_edit.Text.Trim();
            string appColor = tb_backgroundColor_edit.Text.Trim();
            if (!appColor.StartsWith("#")) {
                appColor = "#" + appColor;
            }

            string iconColor = tb_iconColor_edit.Text.Trim();
            if (!iconColor.StartsWith("#")) {
                iconColor = "#" + iconColor;
            }

            userAppSettings.AddNewAppSetting(id, allowResize, allowMaximize, cssClass, autoFullScreen, minHeight, minWidth, allowPopOut, popOutLoc, autoOpen, defaultWorkspace, category, appColor, iconColor);
        }

        BuildEditAppBox(id);
        hf_UpdateUserAppOverrides.Value = string.Empty;

        LoadUserAppOverrides();

        if (DrDefaults.Count == 0) {
            AppIconBuilder aib = new AppIconBuilder(Page, Member);
            aib.BuildAppsForUser();
        }
    }

    protected void lbtn_RefreshOverrides_Click(object sender, EventArgs e) {
        LoadUserAppOverrides();
    }

    private void BuildEditAppBox(string id) {
        if (!string.IsNullOrEmpty(id)) {
            App userApps;
            UserAppSettings userAppSettings;
            if (DrDefaults.Count > 0) {
                string tempUsername = NewUserDefaults.GetRoleID(RoleSelect);
                userAppSettings = new UserAppSettings(tempUsername, true);
                userApps = new App(tempUsername, false);
            }
            else {
                userAppSettings = new UserAppSettings(Username, true);
                userApps = new App(Username, false);
            }
            Dictionary<string, string> appSettings = userAppSettings.GetUserAppSettings(id);
            Apps_Coll coll = userApps.GetAppInformation(id);

            if (coll != null) {
                bool autoFullScreen = false;
                if (appSettings.ContainsKey("AutoFullScreen") && HelperMethods.ConvertBitToBoolean(appSettings["AutoFullScreen"])) {
                    autoFullScreen = true;
                }

                int totalPropertiesOverriden = 0;
                lbl_appId.Text = coll.AppName;
                if (!_ss.HideAllAppIcons && !rb_hideAppIcon_on.Checked) {
                    img_edit.Visible = true;
                    img_edit.ImageUrl = "~/Standard_Images/App_Icons/" + coll.Icon;
                }
                else {
                    img_edit.Visible = false;
                }

                #region Category
                dd_category_edit.Items.Clear();
                AppCategory _category = new AppCategory(true);
                foreach (Dictionary<string, string> dr in _category.category_dt) {
                    var item = new ListItem("&nbsp;" + dr["Category"], dr["ID"]);
                    if (!dd_category_edit.Items.Contains(item)) {
                        dd_category_edit.Items.Add(item);
                    }
                }

                List<string> originalCategoryList = coll.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (appSettings.ContainsKey("Category") && appSettings["Category"] != coll.Category) {
                    List<string> categoryList = appSettings["Category"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (ListItem item in dd_category_edit.Items) {
                        item.Selected = false;
                        if (originalCategoryList.Contains(item.Value) || categoryList.Contains(item.Value)) {
                            item.Selected = true;
                        }

                        if (originalCategoryList.Contains(item.Value) && !categoryList.Contains(item.Value)) {
                            item.Text = "<span style='color: Red;'>" + item.Text + "</span>";
                            totalPropertiesOverriden++;
                        }
                        else if (categoryList.Contains(item.Value)) {
                            item.Text = "<span style='color: Red;'>" + item.Text + "</span>";
                            totalPropertiesOverriden++;
                        }
                    }
                }
                else {
                    foreach (ListItem item in dd_category_edit.Items) {
                        if (originalCategoryList.Contains(item.Value)) {
                            item.Selected = true;
                        }
                        else {
                            item.Selected = false;
                        }
                    }
                }
                #endregion

                #region MinWidth
                if (appSettings.ContainsKey("MinWidth") && appSettings["MinWidth"] != coll.MinWidth) {
                    tb_minwidth_edit.Text = appSettings["MinWidth"];
                    tb_minwidth_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    tb_minwidth_edit.Text = coll.MinWidth;
                    tb_minwidth_edit.ForeColor = System.Drawing.Color.Black;
                }

                tb_minwidth_edit.Enabled = true;
                if (autoFullScreen) {
                    tb_minwidth_edit.Enabled = false;
                }
                #endregion

                #region MinHeight
                if (appSettings.ContainsKey("MinHeight") && appSettings["MinHeight"] != coll.MinHeight) {
                    tb_minheight_edit.Text = appSettings["MinHeight"];
                    tb_minheight_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    tb_minheight_edit.Text = coll.MinHeight;
                    tb_minheight_edit.ForeColor = System.Drawing.Color.Black;
                }

                tb_minheight_edit.Enabled = true;
                if (autoFullScreen) {
                    tb_minheight_edit.Enabled = false;
                }
                #endregion

                #region CssClass
                if (appSettings.ContainsKey("CssClass") && appSettings["CssClass"] != coll.CssClass) {
                    dd_enablebg_edit.SelectedIndex = appSettings["CssClass"] == "app-main" ? 0 : 1;
                    dd_enablebg_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_enablebg_edit.SelectedIndex = coll.CssClass == "app-main" ? 0 : 1;
                    dd_enablebg_edit.ForeColor = System.Drawing.Color.Black;
                }
                #endregion

                #region App Background Color
                if (appSettings.ContainsKey("AppBackgroundColor") && appSettings["AppBackgroundColor"] != coll.AppBackgroundColor) {
                    tb_backgroundColor_edit.Text = appSettings["AppBackgroundColor"].Replace("#", string.Empty);
                    totalPropertiesOverriden++;
                }
                else {
                    tb_backgroundColor_edit.Text = coll.AppBackgroundColor;
                }

                if (coll.CssClass == "app-main") {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#backgroundcolorholder_edit').show();");
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#backgroundcolorholder_edit').hide();");
                }
                #endregion

                #region Icon Background Color
                if (appSettings.ContainsKey("IconBackgroundColor") && appSettings["IconBackgroundColor"] != coll.IconBackgroundColor) {
                    tb_iconColor_edit.Text = appSettings["IconBackgroundColor"].Replace("#", string.Empty);
                    totalPropertiesOverriden++;
                }
                else {
                    tb_iconColor_edit.Text = coll.IconBackgroundColor;
                }
                #endregion

                #region AllowMaximize
                if (appSettings.ContainsKey("AllowMaximize") && HelperMethods.ConvertBitToBoolean(appSettings["AllowMaximize"]) != coll.AllowMaximize) {
                    dd_allowmax_edit.SelectedIndex = HelperMethods.ConvertBitToBoolean(appSettings["AllowMaximize"]) ? 0 : 1;
                    dd_allowmax_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_allowmax_edit.SelectedIndex = coll.AllowMaximize ? 0 : 1;
                    dd_allowmax_edit.ForeColor = System.Drawing.Color.Black;
                }

                dd_allowmax_edit.Enabled = true;
                if (autoFullScreen) {
                    dd_allowmax_edit.Enabled = false;
                }
                #endregion

                #region AllowPopOut
                if (appSettings.ContainsKey("AllowPopOut") && HelperMethods.ConvertBitToBoolean(appSettings["AllowPopOut"]) != coll.AllowPopOut) {
                    dd_allowpopout_edit.SelectedIndex = HelperMethods.ConvertBitToBoolean(appSettings["AllowPopOut"]) ? 0 : 1;
                    dd_allowpopout_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_allowpopout_edit.SelectedIndex = coll.AllowPopOut ? 0 : 1;
                    dd_allowpopout_edit.ForeColor = System.Drawing.Color.Black;
                }
                #endregion

                #region AutoFullScreen
                if (autoFullScreen != coll.AutoFullScreen) {
                    dd_maxonload_edit.SelectedIndex = autoFullScreen ? 0 : 1;
                    dd_maxonload_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_maxonload_edit.SelectedIndex = coll.AutoFullScreen ? 0 : 1;
                    dd_maxonload_edit.ForeColor = System.Drawing.Color.Black;
                }
                #endregion

                #region AllowResize
                if (appSettings.ContainsKey("AllowResize") && HelperMethods.ConvertBitToBoolean(appSettings["AllowResize"]) != coll.AllowResize) {
                    dd_allowresize_edit.SelectedIndex = HelperMethods.ConvertBitToBoolean(appSettings["AllowResize"]) ? 0 : 1;
                    dd_allowresize_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_allowresize_edit.SelectedIndex = coll.AllowResize ? 0 : 1;
                    dd_allowresize_edit.ForeColor = System.Drawing.Color.Black;
                }

                dd_allowresize_edit.Enabled = true;
                if (autoFullScreen) {
                    dd_allowresize_edit.Enabled = false;
                }
                #endregion

                #region DefaultWorkspace
                dd_defaultworkspace_edit.Items.Clear();
                for (int i = 0; i < ddl_totalWorkspaces.Items.Count; i++) {
                    string iStr = (i + 1).ToString();
                    dd_defaultworkspace_edit.Items.Add(new ListItem(iStr, iStr));
                }

                int selectedIndex = 0;
                if (appSettings.ContainsKey("DefaultWorkspace") && appSettings["DefaultWorkspace"] != coll.DefaultWorkspace) {
                    int.TryParse(appSettings["DefaultWorkspace"], out selectedIndex);
                    if (selectedIndex > 0) {
                        dd_defaultworkspace_edit.SelectedIndex = selectedIndex - 1;
                        dd_defaultworkspace_edit.ForeColor = System.Drawing.Color.Red;
                        totalPropertiesOverriden++;
                    }
                }
                else {
                    int.TryParse(coll.DefaultWorkspace, out selectedIndex);
                    if (selectedIndex > 0) {
                        dd_defaultworkspace_edit.SelectedIndex = selectedIndex - 1;
                        dd_defaultworkspace_edit.ForeColor = System.Drawing.Color.Black;
                    }
                }
                #endregion

                #region AutoOpen
                if (appSettings.ContainsKey("AutoOpen") && HelperMethods.ConvertBitToBoolean(appSettings["AutoOpen"]) != coll.AutoOpen) {
                    dd_autoOpen_edit.SelectedIndex = HelperMethods.ConvertBitToBoolean(appSettings["AutoOpen"]) ? 0 : 1;
                    dd_autoOpen_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    dd_autoOpen_edit.SelectedIndex = coll.AutoOpen ? 0 : 1;
                    dd_autoOpen_edit.ForeColor = System.Drawing.Color.Black;
                }
                #endregion

                #region PopOutLoc
                if (appSettings.ContainsKey("PopOutLoc") && appSettings["PopOutLoc"] != coll.PopOutLoc) {
                    tb_allowpopout_edit.Text = appSettings["PopOutLoc"];
                    tb_allowpopout_edit.ForeColor = System.Drawing.Color.Red;
                    totalPropertiesOverriden++;
                }
                else {
                    tb_allowpopout_edit.Text = coll.PopOutLoc;
                    tb_allowpopout_edit.ForeColor = System.Drawing.Color.Black;
                }
                #endregion

                if (totalPropertiesOverriden > 0) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#btn_undoOverrides').show();");
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#btn_undoOverrides').hide();");
                }
            }
        }
    }

    #endregion


    #region -- Workspace Container --

    private void LoadWorkspaceContainer() {
        if (pnl_WorkspaceContainer.Enabled && pnl_WorkspaceContainer.Visible) {
            TotalWorkspacesSettings();
            AppContainerSettings();
            AppSnapHelperSettings();
            SnapAppToGridSettings();
            AppGridSizeSettings();
            AutoRotateWorkspaceSettings();
            PresentationModeSettings();
            ClearPropertiesOnSignOffSettings();

            updatepnl_WorkspaceContainer.Update();
        }
    }

    private int GetTotalWorkspacesByUser() {
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

        return td;
    }

    private void TotalWorkspacesSettings() {
        int td = GetTotalWorkspacesByUser();

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
    private void AppSnapHelperSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["AppSnapHelper"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.AppSnapHelper) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_snapapphelper_off.Checked = false;
            rb_snapapphelper_on.Checked = true;
        }
        else {
            rb_snapapphelper_off.Checked = true;
            rb_snapapphelper_on.Checked = false;
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

        updatepnl_SiteCustomizations.Update();
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

    protected void rb_snapapphelper_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppSnapHelper", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppSnapHelper(true);
        }

        LoadWorkspaceContainer();
    }
    protected void rb_snapapphelper_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppSnapHelper", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppSnapHelper(false);
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

    protected void rb_presentationmode_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("PresentationMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdatePresentationMode(true);
        }

        LoadWorkspaceContainer();
        LoadSiteCustomizations();
    }
    protected void rb_presentationmode_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("PresentationMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdatePresentationMode(false);
        }

        LoadWorkspaceContainer();
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

        LoadWorkspaceContainer();
    }
    protected void rb_clearproperties_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ClearPropOnSignOff", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateClearPropOnSignOff(false);
        }

        LoadWorkspaceContainer();
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
                        cookie.Expires = ServerSettings.ServerDateTime.AddDays(-1d);
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

        LoadWorkspaceContainer();
    }

    #endregion


    #region -- AppRemote --

    private void LoadAppRemoteContainer() {
        if (pnl_AppRemoteContainer.Enabled && pnl_AppRemoteContainer.Visible) {
            MobileAutoSyncSettings();
            LoadNoBGOnAppRemote();
            LoadDefaultBackgroundColor_Mobile();
            LoadAppRemoteForeColor();

            updatepnl_AppRemoteContainer.Update();
        }
    }

    private void MobileAutoSyncSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["MobileAutoSync"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.MobileAutoSync) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_MobileAutoSync_off.Checked = false;
            rb_MobileAutoSync_on.Checked = true;
        }
        else {
            rb_MobileAutoSync_off.Checked = true;
            rb_MobileAutoSync_on.Checked = false;
        }

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

            if (!MemberDatabase.IsComplexWorkspaceMode(mode)) {
                pnl_MobileAutoSync.Enabled = false;
                pnl_MobileAutoSync.Visible = false;
            }
        }
    }
    private void LoadNoBGOnAppRemote() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (string.IsNullOrEmpty(DrDefaults["NoBGOnAppRemote"]) || HelperMethods.ConvertBitToBoolean(DrDefaults["NoBGOnAppRemote"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.NoBGOnAppRemote) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_NoBGOnAppRemote_on.Checked = true;
            rb_NoBGOnAppRemote_off.Checked = false;
            pnl_appremoteForeColor.Enabled = true;
            pnl_appremoteForeColor.Visible = true;
        }
        else {
            rb_NoBGOnAppRemote_on.Checked = false;
            rb_NoBGOnAppRemote_off.Checked = true;
            pnl_appremoteForeColor.Enabled = false;
            pnl_appremoteForeColor.Visible = false;
        }
    }
    private void LoadDefaultBackgroundColor_Mobile() {
        string val = "";
        if (DrDefaults.Count > 0) {
            val = DrDefaults["BackgroundColor"];
        }
        else {
            val = Member.BackgroundColor;
        }

        if (string.IsNullOrEmpty(val)) {
            val = "FFFFFF";
        }

        val = val.Replace("#", string.Empty);
        tb_defaultbackgroundcolor_mobile.Text = val;
    }
    private void LoadAppRemoteForeColor() {
        string val = "";
        if (DrDefaults.Count > 0) {
            val = DrDefaults["AppRemoteForeColor"];
        }
        else {
            val = Member.AppRemoteForeColor;
        }

        if (string.IsNullOrEmpty(val)) {
            val = "FFFFFF";
        }

        val = val.Replace("#", string.Empty);
        tb_appremoteforecolor.Text = val;
    }

    protected void rb_MobileAutoSync_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("MobileAutoSync", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateMobileAutoSync(true);
        }

        LoadAppRemoteContainer();
    }
    protected void rb_MobileAutoSync_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("MobileAutoSync", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateMobileAutoSync(false);
        }

        LoadAppRemoteContainer();
    }

    protected void rb_NoBGOnAppRemote_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("NoBGOnAppRemote", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateNoBGOnAppRemote(true);
        }

        LoadAppRemoteContainer();
    }
    protected void rb_NoBGOnAppRemote_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("NoBGOnAppRemote", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateNoBGOnAppRemote(false);
        }

        LoadAppRemoteContainer();
    }

    protected void btn_defaultbackgroundcolor_mobile_Clicked(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundColor", tb_defaultbackgroundcolor_mobile.Text.Trim());
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundColor(tb_defaultbackgroundcolor_mobile.Text.Trim());
        }

        LoadAppRemoteContainer();
        LoadBackgroundEditor();
    }

    protected void btn_appremoteforecolor_Clicked(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppRemoteForeColor", tb_appremoteforecolor.Text.Trim());
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppRemoteForeColor(tb_appremoteforecolor.Text.Trim());
        }

        LoadAppRemoteContainer();
    }

    #endregion


    #region -- Background Editor --

    private void LoadBackgroundEditor() {
        GetCurrentBackground();

        if (IsPostBack || !string.IsNullOrEmpty(Request.QueryString["u"])) {
            AcctSettings.LoadUserBackground_ForAcctSettingsPage(this.Page, NewDefaults, Member);
        }

        LoadBackgroundLoopTimer();

        if (pnl_BackgroundEditor.Enabled && pnl_BackgroundEditor.Visible) {
            EnabledBackgroundsSettings();
            LoadUserUploadIframe();
            LoadBackgroundPosition();
            LoadBackgroundSize();
            LoadBackgroundRepeat();
            LoadDefaultBackgroundColor();

            if (Username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                pnl_backgroundurl.Enabled = false;
                pnl_backgroundurl.Visible = false;
            }

            updatepnl_BackgroundEditor.Update();
        }
    }

    private void LoadUserUploadIframe() {
        string id = string.Empty;
        if (DrDefaults.Count > 0) {
            id = NewUserDefaults.GetRoleID(RoleSelect);
        }
        else {
            id = Member.UserId;
        }

        pnl_iframeUserImageUpload.Controls.Clear();
        if (!string.IsNullOrEmpty(id)) {
            string url = ResolveUrl("~/SiteTools/iframes/UserImageUpload.aspx?id=" + id + "&javascriptpostback=false");
            string iframe = "<iframe src='" + url + "' frameborder='0' style='width: 540px; height: 80px; overflow: hidden;'></iframe>";
            pnl_iframeUserImageUpload.Controls.Add(new LiteralControl(iframe));
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

        List<string> backgroundSelected = new List<string>();

        string removeImg = "<div class='remove-background-img' data-imgsrc='{0}' title='Click to remove'></div>";

        pnl_BackgroundLoopTimer.Enabled = false;
        pnl_BackgroundLoopTimer.Visible = false;

        if (string.IsNullOrEmpty(background)) {
            background = ResolveUrl("~/App_Themes/" + _siteTheme + "/Body/default-bg.jpg");
            activeimg += "<div class='image-selector-current'>Default<br /><img src='" + background + "' /></div>";
        }
        else {
            backgroundSelected = background.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (backgroundSelected.Count > 1) {
                pnl_BackgroundLoopTimer.Enabled = true;
                pnl_BackgroundLoopTimer.Visible = true;
            }
        }

        tb_imageurl.Text = "Link to image";
        foreach (string b in backgroundSelected) {
            if (b.Length == 6) {
                activeimg += "<div class='image-selector-current'>" + string.Format(removeImg, b) + "<div style='background: #" + b + ";' class='color-bg-div'></div></div>";
            }
            else if (IsValidHttpUri(b)) {
                activeimg += "<div class='image-selector-current'>" + string.Format(removeImg, b) + "<img alt='' src='" + b + "' /></div>";
            }
            else {
                string size = string.Empty;
                if (File.Exists(ServerSettings.GetServerMapLocation + b)) {
                    using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + b)) {
                        size = image.Width + "x" + image.Height;
                    }
                }
                activeimg += "<div class='image-selector-current'>" + string.Format(removeImg, b) + "<img alt='' title='" + size + "' src='../../" + b + "' /></div>";
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#CurrentBackground').html(\"" + activeimg + "\");");
    }
    private void LoadBackgroundLoopTimer() {
        int timer = 0;

        if (DrDefaults.Count > 0) {
            string val = DrDefaults["BackgroundLoopTimer"];
            if (string.IsNullOrEmpty(val)) {
                val = "30";
            }

            int.TryParse(val, out timer);
        }
        else {
            int.TryParse(Member.BackgroundLoopTimer, out timer);
        }

        if (timer >= 1000) {
            timer = timer / 1000;
        }
        else {
            timer = 30;
        }

        tb_backgroundlooptimer.Text = timer.ToString();
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

        if (GetTotalWorkspacesByUser() <= 1) {
            pnl_backgroundurl.Enabled = false;
            pnl_backgroundurl.Visible = false;
            pnl_backgroundSelector.Enabled = false;
            pnl_backgroundSelector.Visible = false;
        }
        else {
            pnl_backgroundurl.Enabled = true;
            pnl_backgroundurl.Visible = true;
        }
    }
    private void LoadBackgroundPosition() {
        string val = "";
        if (DrDefaults.Count > 0) {
            val = DrDefaults["BackgroundPosition"];
        }
        else {
            val = Member.BackgroundPosition;
        }

        foreach (ListItem item in dd_backgroundposition.Items) {
            if (item.Value == val) {
                item.Selected = true;
            }
            else {
                item.Selected = false;
            }
        }
    }
    private void LoadBackgroundSize() {
        string val = "";
        if (DrDefaults.Count > 0) {
            val = DrDefaults["BackgroundSize"];
        }
        else {
            val = Member.BackgroundSize;
        }

        foreach (ListItem item in dd_backgroundsize.Items) {
            if (item.Value == val) {
                item.Selected = true;
            }
            else {
                item.Selected = false;
            }
        }
    }
    private void LoadBackgroundRepeat() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (string.IsNullOrEmpty(DrDefaults["BackgroundRepeat"]) || HelperMethods.ConvertBitToBoolean(DrDefaults["BackgroundRepeat"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.BackgroundRepeat) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_backgroundrepeat_on.Checked = true;
            rb_backgroundrepeat_off.Checked = false;
        }
        else {
            rb_backgroundrepeat_on.Checked = false;
            rb_backgroundrepeat_off.Checked = true;
        }
    }
    private void LoadDefaultBackgroundColor() {
        string val = "";
        if (DrDefaults.Count > 0) {
            val = DrDefaults["BackgroundColor"];
        }
        else {
            val = Member.BackgroundColor;
        }

        if (string.IsNullOrEmpty(val)) {
            val = "FFFFFF";
        }

        val = val.Replace("#", string.Empty);
        tb_defaultbackgroundcolor.Text = val;
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
                    string currImg = NewDefaults.GetBackgroundImg(1);
                    currImg += MemberDatabase.BackgroundSeperator + color;
                    NewDefaults.updateBackgroundImg(currImg, 1);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(1);
                    currImg += MemberDatabase.BackgroundSeperator + color;
                    Member.UpdateBackgroundImg(currImg, 1);
                }
                break;
            default: {
                    int workspace = 1;
                    if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                        workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                    }
                    if (DrDefaults.Count > 0) {
                        string currImg = NewDefaults.GetBackgroundImg(workspace);
                        currImg += MemberDatabase.BackgroundSeperator + color;
                        NewDefaults.updateBackgroundImg(currImg, workspace);
                        UpdateDrDefaults();
                    }
                    else {
                        string currImg = Member.GetBackgroundImg(workspace);
                        currImg += MemberDatabase.BackgroundSeperator + color;
                        Member.UpdateBackgroundImg(currImg, workspace);
                    }
                }
                break;
        }

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
                    string currImg = NewDefaults.GetBackgroundImg(1);
                    currImg += MemberDatabase.BackgroundSeperator + tb_imageurl.Text.Trim();
                    NewDefaults.updateBackgroundImg(currImg, 1);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(1);
                    currImg += MemberDatabase.BackgroundSeperator + tb_imageurl.Text.Trim();
                    Member.UpdateBackgroundImg(currImg, 1);
                }
                BuildBackgrounds(1);
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(workspace);
                    currImg += MemberDatabase.BackgroundSeperator + tb_imageurl.Text.Trim();
                    NewDefaults.updateBackgroundImg(currImg, workspace);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(workspace);
                    currImg += MemberDatabase.BackgroundSeperator + tb_imageurl.Text.Trim();
                    Member.UpdateBackgroundImg(currImg, workspace);
                }
                BuildBackgrounds(workspace);
            }
        }

        LoadBackgroundEditor();
    }

    protected void hf_backgroundimg_Changed(object sender, EventArgs e) {
        var image = hf_backgroundimg.Value.Replace("../", "");

        if (pnl_backgroundSelector.Enabled == false) {
            if (DrDefaults.Count > 0) {
                string currImg = NewDefaults.GetBackgroundImg(1);
                currImg += MemberDatabase.BackgroundSeperator + image;
                NewDefaults.updateBackgroundImg(currImg, 1);
                UpdateDrDefaults();
            }
            else {
                string currImg = Member.GetBackgroundImg(1);
                currImg += MemberDatabase.BackgroundSeperator + image;
                Member.UpdateBackgroundImg(currImg, 1);
            }

            BuildBackgrounds(1);
        }
        else {
            int workspace = 1;
            if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
            }
            if (DrDefaults.Count > 0) {
                string currImg = NewDefaults.GetBackgroundImg(workspace);
                currImg += MemberDatabase.BackgroundSeperator + image;
                NewDefaults.updateBackgroundImg(currImg, workspace);
                UpdateDrDefaults();
            }
            else {
                string currImg = Member.GetBackgroundImg(workspace);
                currImg += MemberDatabase.BackgroundSeperator + image;
                Member.UpdateBackgroundImg(currImg, workspace);
            }

            BuildBackgrounds(workspace);
        }

        LoadBackgroundEditor();
        hf_backgroundimg.Value = "";
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'Background-element', 'Workspace Background Selector');");
    }
    protected void hf_removebackgroundimgEdit_Changed(object sender, EventArgs e) {
        var imageToRemove = hf_removebackgroundimgEdit.Value.Replace("../", "");

        if (!string.IsNullOrEmpty(imageToRemove)) {
            if (pnl_backgroundSelector.Enabled == false) {
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(1);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(1);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                }

                BuildBackgrounds(1);
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(workspace);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(workspace);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                }

                BuildBackgrounds(workspace);
            }
        }

        LoadBackgroundEditor();
        hf_removebackgroundimgEdit.Value = "";
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'Background-element', 'Workspace Background Selector');");
    }
    private void BuildBackgrounds(int workspace) {
        string background = string.Empty;
        if (DrDefaults.Count > 0) {
            background = NewDefaults.GetBackgroundImg(workspace);
        }
        else {
            background = Member.GetBackgroundImg(workspace);
        }

        List<string> backgroundSelected = background.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

        var str = new StringBuilder();
        if (dd_imageFolder.SelectedValue == "public") {
            int totalPublic = 0;
            if (Directory.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds")) {
                string[] fileDir = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
                foreach (string filename in fileDir) {
                    var fi = new FileInfo(filename);
                    if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                        || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                        string filepath = "Standard_Images/Backgrounds/" + fi.Name;
                        string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
                        string size;
                        using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                            size = image.Width + "x" + image.Height;
                        }

                        string c = "pad-all-sml inline-block ";
                        c += backgroundSelected.Contains(filepath) ? "image-selector-active" : "image-selector-acct";
                        str.Append("<div class='" + c + "'><img alt='' title='" + size + "' src='../../" + filepath + "' class='boxshadow borderShadow2' /></div>");
                        totalPublic++;
                    }
                }
            }

            if (totalPublic == 0) {
                str.Append("<h3 class='pad-all'>No public images found.</h3>");
            }
        }
        else {
            int totalUploaded = 0;
            if (Directory.Exists(GetUploadedBackgroundDir)) {
                string[] uploadfileDir = Directory.GetFiles(GetUploadedBackgroundDir);
                string deleteImg = "<div class='delete-uploadedimg' data-imgsrc='{0}' title='Delete Image'></div>";
                foreach (string filename in uploadfileDir) {
                    var fi = new FileInfo(filename);
                    if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                        || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {

                        string size;
                        using (var image = Image.FromFile(filename)) {
                            size = image.Width + "x" + image.Height;
                        }

                        string filepath = filename.Replace(ServerSettings.GetServerMapLocation, string.Empty).Replace("\\", "/");

                        string c = "pad-all-sml inline-block ";
                        c += backgroundSelected.Contains(filepath) ? "image-selector-active" : "image-selector-acct";
                        str.Append("<div class='user-uploadedimages " + c + "'>" + string.Format(deleteImg, fi.Name) + "<img alt='' title='" + size + "' src='../../" + filepath + "' class='boxshadow borderShadow2' /></div>");
                        totalUploaded++;
                    }
                }
            }

            if (totalUploaded == 0) {
                str.Append("<h3 class='pad-all'>No uploaded images found.</h3>");
            }
        }

        foreach (string b in backgroundSelected) {
            if (b.Length == 6) {
                str.Append("<div class='pad-all-sml inline-block image-selector-active'><div style='background: #" + b + ";' class='inline-block boxshadow borderShadow2 color-bg-div'></div></div>");
            }
            else if (IsValidHttpUri(b)) {
                str.Append("<div class='pad-all-sml inline-block image-selector-active'><img alt='' src='" + b + "' class='boxshadow borderShadow2' /></div>");
            }
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

    protected void btn_backgroundlooptimer_Click(object sender, EventArgs e) {
        int timer = 0;
        int.TryParse(tb_backgroundlooptimer.Text.Trim(), out timer);

        if (timer > 0) {
            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("BackgroundLoopTimer", timer.ToString());
            }
            else {
                Member.UpdateBackgroundLoopTimer(timer.ToString());
            }
        }

        LoadBackgroundEditor();
    }

    protected void hf_removebackgroundimg_Changed(object sender, EventArgs e) {
        string imageToRemove = hf_removebackgroundimg.Value.Trim();
        if (!string.IsNullOrEmpty(imageToRemove)) {
            if (pnl_backgroundSelector.Enabled == false) {
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(1);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(1);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                }

                BuildBackgrounds(1);
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(workspace);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(workspace);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                }

                BuildBackgrounds(workspace);
            }
        }

        hf_removebackgroundimg.Value = string.Empty;

        LoadBackgroundEditor();
    }

    private string GetUploadedBackgroundDir {
        get {
            string fullPath = ServerSettings.GetServerMapLocation + "Standard_Images\\AcctImages\\";
            if (DrDefaults.Count > 0) {
                fullPath += NewUserDefaults.GetRoleID(RoleSelect);
            }
            else {
                fullPath += Member.UserId;
            }
            fullPath += "\\UploadedBackgrounds\\";

            return fullPath;
        }
    }

    protected void dd_imageFolder_SelectedIndexChanged(object sender, EventArgs e) {
        LoadBackgroundEditor();

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

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'Background-element', 'Workspace Background Selector');");
    }
    protected void hf_deleteUploadedImage_ValueChanged(object sender, EventArgs e) {
        string imageToRemove = GetUploadedBackgroundDir + hf_deleteUploadedImage.Value;
        if (!string.IsNullOrEmpty(imageToRemove)) {
            try {
                if (File.Exists(imageToRemove)) {
                    File.Delete(imageToRemove);
                }
            }
            catch(Exception ex) {
                AppLog.AddError(ex);
            }

            imageToRemove = imageToRemove.Replace(ServerSettings.GetServerMapLocation, string.Empty).Replace("\\", "/");

            if (pnl_backgroundSelector.Enabled == false) {
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(1);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(1);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), 1);
                }

                BuildBackgrounds(1);
            }
            else {
                int workspace = 1;
                if (!string.IsNullOrEmpty(dd_backgroundSelector.SelectedValue)) {
                    workspace = Convert.ToInt16(dd_backgroundSelector.SelectedValue);
                }
                if (DrDefaults.Count > 0) {
                    string currImg = NewDefaults.GetBackgroundImg(workspace);
                    NewDefaults.updateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                    UpdateDrDefaults();
                }
                else {
                    string currImg = Member.GetBackgroundImg(workspace);
                    Member.UpdateBackgroundImg(AcctSettings.RemoveBackgroundString(currImg, imageToRemove), workspace);
                }

                BuildBackgrounds(workspace);
            }
        }

        hf_deleteUploadedImage.Value = string.Empty;
        LoadBackgroundEditor();

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'Background-element', 'Workspace Background Selector');");
    }

    protected void btn_backgroundposition_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundPosition", dd_backgroundposition.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundPosition(dd_backgroundposition.SelectedValue);
        }

        LoadBackgroundEditor();
    }
    protected void btn_backgroundsize_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundSize", dd_backgroundsize.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundSize(dd_backgroundsize.SelectedValue);
        }

        LoadBackgroundEditor();
    }

    protected void rb_backgroundrepeat_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundRepeat", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundRepeat(true);
        }

        LoadBackgroundEditor();
    }
    protected void rb_backgroundrepeat_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundRepeat", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundRepeat(false);
        }

        LoadBackgroundEditor();
    }

    protected void btn_defaultbackgroundcolor_Clicked(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("BackgroundColor", tb_defaultbackgroundcolor.Text.Trim());
            UpdateDrDefaults();
        }
        else {
            Member.UpdateBackgroundColor(tb_defaultbackgroundcolor.Text.Trim());
        }

        LoadBackgroundEditor();
        LoadAppRemoteContainer();
    }

    protected void hf_refreshbackgroundList_Changed(object sender, EventArgs e) {
        hf_refreshbackgroundList.Value = string.Empty;
        LoadBackgroundEditor();
    }

    #endregion


    #region -- Icon Selector --

    private void LoadIconSelector() {
        if (pnl_IconSelector.Enabled && pnl_IconSelector.Visible) {
            LockAppIconsSettings();
            AppSelectorStyleSettings();
            GroupIconsSettings();
            AppCategoryCountSettings();
            HideAppIconSettings();
            ShowWorkspaceNumAppSettings();

            updatepnl_IconSelector.Update();

            if (Member != null && HttpContext.Current.User.Identity.Name.ToLower() == Member.Username.ToLower() 
                && Request.QueryString["toolView"] != "true" && Request.QueryString["mobileMode"] != "true") {
                AppIconBuilder appIconBuilder = new AppIconBuilder(this.Page, Member);
                appIconBuilder.BuildAppsForUser();
            }
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
    private void AppSelectorStyleSettings() {
        bool canContinue = true;
        string controlId = ScriptManager.GetCurrent(this.Page).AsyncPostBackSourceElementID;
        if (controlId != null && controlId.ToLower().Contains("btn_appselectorstyle")) {
            canContinue = false;
        }

        if (canContinue) {
            dd_AppSelectorStyle.Items.Clear();
            Array vals = Enum.GetValues(typeof(MemberDatabase.AppIconSelectorStyle));
            string style = MemberDatabase.AppIconSelectorStyle.Default.ToString();

            if (DrDefaults.Count > 0) {
                if (!string.IsNullOrEmpty(DrDefaults["AppSelectorStyle"])) {
                    style = DrDefaults["AppSelectorStyle"];
                }
            }
            else {
                style = Member.AppSelectorStyle.ToString();
            }

            if (_ss.HideAllAppIcons) {
                style = MemberDatabase.AppIconSelectorStyle.Default.ToString();
            }

            int currentIndex = 0;
            int selectedIndex = 0;
            for (int i = 0; i < vals.Length; i++) {
                string itemText = vals.GetValue(i).ToString().Replace("_Plus_", ", ").Replace("_", " ");
                string itemVal = vals.GetValue(i).ToString();
                if (_ss.HideAllAppIcons && (itemVal == MemberDatabase.AppIconSelectorStyle.Icon_Only.ToString() || itemVal == MemberDatabase.AppIconSelectorStyle.Icon_And_Text_Only.ToString()
                    || itemVal == MemberDatabase.AppIconSelectorStyle.Icon_And_Color_Only.ToString() || itemVal == MemberDatabase.AppIconSelectorStyle.Icon_Plus_Color_And_Text.ToString())) {
                    continue;
                }

                dd_AppSelectorStyle.Items.Add(new ListItem(itemText, itemVal));
                if (itemVal == style) {
                    selectedIndex = currentIndex;
                }

                currentIndex++;
            }

            if (dd_AppSelectorStyle.Items.Count > 0) {
                dd_AppSelectorStyle.SelectedIndex = selectedIndex;
            }
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
        if (!ServerSettings.AdminPagesCheck("Workspace", Username) && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
            pnl_ShowAppTitle.Enabled = false;
            pnl_ShowAppTitle.Visible = false;
            pnl_showAppImage.Enabled = false;
            pnl_showAppImage.Visible = false;
        }
        else {
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

                if (dd_AppSelectorStyle.SelectedValue == MemberDatabase.AppIconSelectorStyle.Icon_Only.ToString() || dd_AppSelectorStyle.SelectedValue == MemberDatabase.AppIconSelectorStyle.Icon_And_Text_Only.ToString()
                    || dd_AppSelectorStyle.SelectedValue == MemberDatabase.AppIconSelectorStyle.Icon_And_Color_Only.ToString() || dd_AppSelectorStyle.SelectedValue == MemberDatabase.AppIconSelectorStyle.Icon_Plus_Color_And_Text.ToString()) {
                    pnl_HideAppIcons.Enabled = false;
                    pnl_HideAppIcons.Visible = false;
                    settingOn = false;
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

                updatepnl_SiteCustomizations.Update();
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

    protected void btn_AppSelectorStyle_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AppSelectorStyle", dd_AppSelectorStyle.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAppSelectorStyle(dd_AppSelectorStyle.SelectedValue);
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
        LoadUserAppOverrides();
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
        LoadUserAppOverrides();
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
                    if (!Member.AdminChatControlled || Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
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
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_ChatClient");
            }
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
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_ChatClient");
            }
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


    #region -- Site Customizations --

    private void LoadSiteCustomizations() {
        if (pnl_SiteCustomizations.Enabled && pnl_SiteCustomizations.Visible) {
            if (DrDefaults.Count > 0) {
                BuildAppPackagesSettings();
                LoadPluginListEdit();
                BuildPackageList_DemoDefaults();
            }

            LoadWorkspaceMode();
            GetAnimationSpeedSettings();
            SiteThemeSettings();
            ShowToolTipsSettings();
            ShowSiteTipsSettings();
            ShowAppTitleSettings();
            AppHeaderIconSettings();
            UserAppStyleSettings();
            CustomFontSettings();
            ShowDateTimeSettings();
            ProfileLinkStyleSettings();
            ShowTopSearchSettings();
            LoadLinksBlankPageSettings();
            LoadPageDescriptionSettings();
            LoadShowSiteToolsIconsSettings();
            UseAltSidebarSettings();
            SidebarTransparencyModeSettings();
            TopbarTransparencyModeSettings();
            AutoHideModeSettings();
            TaskbarShowAllSettings();
            HoverPreviewWorkspaceSettings();
            MinPreviewSettings();
            SidebarAccordionSettings();
            SidebarAccordionMutliOpenAllowedSettings();
            HideSidebarMenuIconsSettings();
            ShowSiteToolsInCategoriesSettings();

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
    private void LoadPluginListEdit() {
        StringBuilder pluginScriptUninstalled = new StringBuilder();
        StringBuilder pluginScriptInstalled = new StringBuilder();

        SitePlugins _plugins = new SitePlugins(string.Empty);
        _plugins.BuildSitePlugins(true);
        List<string> pluginList = DrDefaults["PluginsToInstall"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (pluginList.Contains(coll.ID)) {
                    pluginScriptInstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                    pluginScriptInstalled.Append("<a href='#' class='img-collapse-sml cursor-pointer float-left' title='Uninstall " + coll.PluginName + " Plugin' onclick='RemovePlugin(\"" + coll.ID + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                    pluginScriptInstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + coll.PluginName + "</span>");
                    pluginScriptInstalled.Append("</div>");
                }
                else {
                    pluginScriptUninstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                    pluginScriptUninstalled.Append("<a href='#' class='img-expand-sml cursor-pointer float-left' title='Install " + coll.PluginName + " Plugin' onclick='AddPlugin(\"" + coll.ID + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                    pluginScriptUninstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + coll.PluginName + "</span>");
                    pluginScriptUninstalled.Append("</div>");
                }
            }
        }

        string table = HelperMethods.TableAddRemove(pluginScriptUninstalled.ToString(), pluginScriptInstalled.ToString(), "Plugins Available to Install", "Plugins Installed", false, true);
        pnl_overlayList.Controls.Clear();
        pnl_overlayList.Controls.Add(new LiteralControl(table));
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

                lbl_appmodalstyle_title.InnerHtml = "Modal Window Style";

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
    private void ShowSiteTipsSettings() {
        bool settingOn = false;
        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["SiteTipsOnPageLoad"]) || string.IsNullOrEmpty(DrDefaults["SiteTipsOnPageLoad"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.SiteTipsOnPageLoad) {
                settingOn = true;
            }
        }
        if (settingOn) {
            rb_sitetipsonload_on.Checked = true;
            rb_sitetipsonload_off.Checked = false;
        }
        else {
            rb_sitetipsonload_on.Checked = false;
            rb_sitetipsonload_off.Checked = true;
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
    private void UserAppStyleSettings() {
        string appStyle = MemberDatabase.AppStyle.Style_1.ToString();

        string titleBarLook = "_icon";
        if (DrDefaults.Count > 0) {
            appStyle = DrDefaults["UserAppStyle"];
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowAppTitle"]) && HelperMethods.ConvertBitToBoolean(DrDefaults["AppHeaderIcon"])
                && !_ss.HideAllAppIcons && !HelperMethods.ConvertBitToBoolean(DrDefaults["HideAppIcon"])) {
                titleBarLook = "_icon_and_name";
            }
            else if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowAppTitle"]) && (!HelperMethods.ConvertBitToBoolean(DrDefaults["AppHeaderIcon"])
                || _ss.HideAllAppIcons || HelperMethods.ConvertBitToBoolean(DrDefaults["HideAppIcon"]))) {
                titleBarLook = "_name";
            }
            else if (!HelperMethods.ConvertBitToBoolean(DrDefaults["ShowAppTitle"]) && (!HelperMethods.ConvertBitToBoolean(DrDefaults["AppHeaderIcon"])
                || _ss.HideAllAppIcons || HelperMethods.ConvertBitToBoolean(DrDefaults["HideAppIcon"]))) {
                titleBarLook = "_none";
            }
        }
        else {
            appStyle = Member.UserAppStyle.ToString();
            if (Member.ShowAppTitle && Member.AppHeaderIcon && !_ss.HideAllAppIcons && !Member.HideAppIcon) {
                titleBarLook = "_icon_and_name";
            }
            else if (Member.ShowAppTitle && (!Member.AppHeaderIcon || _ss.HideAllAppIcons || Member.HideAppIcon)) {
                titleBarLook = "_name";
            }
            else if (!Member.ShowAppTitle && (!Member.AppHeaderIcon || _ss.HideAllAppIcons || Member.HideAppIcon)) {
                titleBarLook = "_none";
            }
        }

        for (int i = 0; i < dd_appStyle.Items.Count; i++) {
            if (dd_appStyle.Items[i].Value == appStyle) {
                dd_appStyle.SelectedIndex = i;
                break;
            }
        }

        string currTheme = "Standard";
        if (DrDefaults.Count > 0) {
            currTheme = DrDefaults["Theme"];
        }
        else {
            currTheme = Member.SiteTheme;
        }

        string exampleImg = "App_Themes/" + currTheme + "/App/AppStyle_Examples/Style_" + (dd_appStyle.SelectedIndex + 1).ToString() + "/Style_" + (dd_appStyle.SelectedIndex + 1).ToString() + titleBarLook + ".png";
        if (File.Exists(ServerSettings.GetServerMapLocation + exampleImg)) {
            img_appstyleexample.ImageUrl = ResolveUrl("~/" + exampleImg);
            lbl_appstyleexample_space.Visible = true;
            img_appstyleexample.Visible = true;
        }
        else {
            lbl_appstyleexample_space.Visible = false;
            img_appstyleexample.Visible = false;
        }
    }
    private void CustomFontSettings() {
        string defaultBodyFontFamily = string.Empty;
        if (DrDefaults.Count > 0) {
            defaultBodyFontFamily = DrDefaults["DefaultBodyFontFamily"];
        }
        else {
            defaultBodyFontFamily = Member.DefaultBodyFontFamily;
        }

        if (string.IsNullOrEmpty(defaultBodyFontFamily)) {
            defaultBodyFontFamily = _ss.DefaultBodyFontFamily;
        }

        string defaultBodyFontSize = string.Empty;
        if (DrDefaults.Count > 0) {
            defaultBodyFontSize = DrDefaults["DefaultBodyFontSize"];
        }
        else {
            defaultBodyFontSize = Member.DefaultBodyFontSize;
        }

        if (string.IsNullOrEmpty(defaultBodyFontSize)) {
            defaultBodyFontSize = _ss.DefaultBodyFontSize;
        }

        string defaultBodyFontColor = string.Empty;
        if (DrDefaults.Count > 0) {
            defaultBodyFontColor = DrDefaults["DefaultBodyFontColor"];
        }
        else {
            defaultBodyFontColor = Member.DefaultBodyFontColor;
        }

        if (string.IsNullOrEmpty(defaultBodyFontColor)) {
            defaultBodyFontColor = _ss.DefaultBodyFontColor;
        }

        if (dd_defaultbodyfontfamily.Items.Count == 0) {
            dd_defaultbodyfontfamily.Items.Add(new ListItem("Theme Default", "inherit"));
            dd_defaultbodyfontfamily.SelectedIndex = 0;

            string path = ServerSettings.GetServerMapLocation + "CustomFonts";
            if (Directory.Exists(path)) {
                string[] fileList = Directory.GetFiles(path);
                foreach (string file in fileList) {
                    if (!string.IsNullOrEmpty(file)) {
                        FileInfo fi = new FileInfo(file);
                        if (fi.Extension.ToLower() == ".css") {
                            string name = fi.Name.Replace("_", " ").Replace(".css", string.Empty);
                            ListItem item = new ListItem(name, fi.Name);
                            if (!dd_defaultbodyfontfamily.Items.Contains(item)) {
                                dd_defaultbodyfontfamily.Items.Add(item);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < dd_defaultbodyfontfamily.Items.Count; i++) {
            if (dd_defaultbodyfontfamily.Items[i].Value == defaultBodyFontFamily) {
                dd_defaultbodyfontfamily.SelectedIndex = i;
                break;
            }
        }

        tb_defaultfontsize.Text = defaultBodyFontSize;
        tb_defaultfontcolor.Text = defaultBodyFontColor.Replace("#", string.Empty);
        RegisterPostbackScripts.RegisterStartupScript(this, "SetDefaultStyles();");
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
    private void ProfileLinkStyleSettings() {
        bool canContinue = true;
        string controlId = ScriptManager.GetCurrent(this.Page).AsyncPostBackSourceElementID;
        if (controlId != null && controlId.ToLower().Contains("btn_ProfileLinkStyle")) {
            canContinue = false;
        }

        if (canContinue) {
            dd_ProfileLinkStyle.Items.Clear();
            Array vals = Enum.GetValues(typeof(MemberDatabase.UserProfileLinkStyle));
            string style = MemberDatabase.UserProfileLinkStyle.Default.ToString();

            if (DrDefaults.Count > 0) {
                if (!string.IsNullOrEmpty(DrDefaults["ProfileLinkStyle"])) {
                    style = DrDefaults["ProfileLinkStyle"];
                }
            }
            else {
                style = Member.ProfileLinkStyle.ToString();
            }

            int currentIndex = 0;
            int selectedIndex = 0;
            for (int i = 0; i < vals.Length; i++) {
                string itemText = vals.GetValue(i).ToString().Replace("_Plus_", ", ").Replace("_", " ");
                string itemVal = vals.GetValue(i).ToString();

                dd_ProfileLinkStyle.Items.Add(new ListItem(itemText, itemVal));
                if (itemVal == style) {
                    selectedIndex = currentIndex;
                }

                currentIndex++;
            }

            if (dd_ProfileLinkStyle.Items.Count > 0) {
                dd_ProfileLinkStyle.SelectedIndex = selectedIndex;
            }
        }
    }
    private void ShowTopSearchSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowTopSearch"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowTopSearch) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_showtopsearch_on.Checked = true;
            rb_showtopsearch_off.Checked = false;
        }
        else {
            rb_showtopsearch_on.Checked = false;
            rb_showtopsearch_off.Checked = true;
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
    private void LoadPageDescriptionSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowSiteToolsPageDescription"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowSiteToolsPageDescription) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_showpagedescriptions_off.Checked = false;
            rb_showpagedescriptions_on.Checked = true;
        }
        else {
            rb_showpagedescriptions_off.Checked = true;
            rb_showpagedescriptions_on.Checked = false;
        }
    }
    private void LoadShowSiteToolsIconsSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["ShowSiteToolsIcons"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.ShowSiteToolsIcons) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_ShowSiteToolsIcons_off.Checked = false;
            rb_ShowSiteToolsIcons_on.Checked = true;
        }
        else {
            rb_ShowSiteToolsIcons_off.Checked = true;
            rb_ShowSiteToolsIcons_on.Checked = false;
        }

        if (_ss.HideAllAppIcons) {
            pnl_ShowPageIcons.Enabled = false;
            pnl_ShowPageIcons.Visible = false;
        }
        else {
            pnl_ShowPageIcons.Enabled = true;
            pnl_ShowPageIcons.Visible = true;
        }
    }
    private void UseAltSidebarSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["UseAltSidebarTheme"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.UseAltSidebarTheme) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_UseAltSidebar_Off.Checked = false;
            rb_UseAltSidebar_On.Checked = true;
        }
        else {
            rb_UseAltSidebar_Off.Checked = true;
            rb_UseAltSidebar_On.Checked = false;
        }
    }
    private void SidebarTransparencyModeSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["SidebarTransparencyMode"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.SidebarTransparencyMode) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_SidebarTransparencyMode_Off.Checked = false;
            rb_SidebarTransparencyMode_On.Checked = true;
        }
        else {
            rb_SidebarTransparencyMode_Off.Checked = true;
            rb_SidebarTransparencyMode_On.Checked = false;
        }
    }
    private void TopbarTransparencyModeSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["TopbarTransparencyMode"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.TopbarTransparencyMode) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_TopbarTransparencyMode_Off.Checked = false;
            rb_TopbarTransparencyMode_On.Checked = true;
        }
        else {
            rb_TopbarTransparencyMode_Off.Checked = true;
            rb_TopbarTransparencyMode_On.Checked = false;
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
    private void SidebarAccordionSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["SidebarAccordion"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.SidebarAccordion) {
                settingOn = true;
            }
        }

        if (settingOn) {
            rb_SidebarAccordion_on.Checked = true;
            rb_SidebarAccordion_off.Checked = false;
            pnl_AccordianOpen.Enabled = true;
            pnl_AccordianOpen.Visible = true;
        }
        else {
            rb_SidebarAccordion_on.Checked = false;
            rb_SidebarAccordion_off.Checked = true;
            pnl_AccordianOpen.Enabled = false;
            pnl_AccordianOpen.Visible = false;
        }
    }
    private void SidebarAccordionMutliOpenAllowedSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (!HelperMethods.ConvertBitToBoolean(DrDefaults["SidebarAccordionMutliOpenAllowed"])) {
                settingOn = true;
            }
        }
        else {
            if (!Member.SidebarAccordionMutliOpenAllowed) {
                settingOn = true;
            }
        }


        if (settingOn) {
            rb_SidebarAccordionMutliOpenAllowed_on.Checked = true;
            rb_SidebarAccordionMutliOpenAllowed_off.Checked = false;
        }
        else {
            rb_SidebarAccordionMutliOpenAllowed_on.Checked = false;
            rb_SidebarAccordionMutliOpenAllowed_off.Checked = true;
        }
    }
    private void HideSidebarMenuIconsSettings() {
        bool settingOn = false;

        if (DrDefaults.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(DrDefaults["HideSidebarMenuIcons"])) {
                settingOn = true;
            }
        }
        else {
            if (Member.HideSidebarMenuIcons) {
                settingOn = true;
            }
        }


        if (settingOn) {
            rb_HideSidebarMenuIcons_on.Checked = true;
            rb_HideSidebarMenuIcons_off.Checked = false;
        }
        else {
            rb_HideSidebarMenuIcons_on.Checked = false;
            rb_HideSidebarMenuIcons_off.Checked = true;
        }

        if (_ss.HideAllAppIcons) {
            pnl_HideSidebarMenuIcons.Enabled = false;
            pnl_HideSidebarMenuIcons.Visible = false;
        }
        else {
            pnl_HideSidebarMenuIcons.Enabled = true;
            pnl_HideSidebarMenuIcons.Visible = true;
        }
    }
    private void ShowSiteToolsInCategoriesSettings() {
        bool settingOn = true;

        if (DrDefaults.Count > 0) {
            if (!HelperMethods.ConvertBitToBoolean(DrDefaults["ShowSiteToolsInCategories"])) {
                settingOn = false;
            }
        }
        else {
            if (!Member.ShowSiteToolsInCategories) {
                settingOn = false;
            }
        }

        if (settingOn) {
            rb_ShowSiteToolsInCategories_on.Checked = true;
            rb_ShowSiteToolsInCategories_off.Checked = false;
        }
        else {
            rb_ShowSiteToolsInCategories_on.Checked = false;
            rb_ShowSiteToolsInCategories_off.Checked = true;
        }
    }

    protected void btn_updatedemo_Click(object sender, EventArgs e) {
        NewDefaults.UpdateDefaults("AppPackage", dd_appdemo.SelectedValue);
        UpdateDrDefaults();
        LoadSiteCustomizations();
        LoadNotificationsSettings();
        LoadWorkspaceOverlays();
        LoadUserAppOverrides();
    }

    protected void hf_removePlugin_ValueChanged(object sender, EventArgs e) {
        string pluginId = hf_removePlugin.Value;
        string pluginsToInstall = DrDefaults["PluginsToInstall"];

        if (!string.IsNullOrEmpty(pluginId)) {
            pluginsToInstall = string.Empty;
            string[] pluginList = DrDefaults["PluginsToInstall"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            foreach (string plId in pluginList) {
                if (plId != pluginId) {
                    pluginsToInstall += plId + ServerSettings.StringDelimiter;
                }
            }
        }

        NewDefaults.UpdateDefaults("PluginsToInstall", pluginsToInstall);
        UpdateDrDefaults();
        LoadSiteCustomizations();

        hf_removePlugin.Value = string.Empty;
    }
    protected void hf_addPlugin_ValueChanged(object sender, EventArgs e) {
        string pluginId = hf_addPlugin.Value;
        string pluginsToInstall = DrDefaults["PluginsToInstall"];

        if (!string.IsNullOrEmpty(pluginId)) {
            pluginsToInstall += pluginId + ServerSettings.StringDelimiter;
        }

        NewDefaults.UpdateDefaults("PluginsToInstall", pluginsToInstall);
        UpdateDrDefaults();
        LoadSiteCustomizations();

        hf_addPlugin.Value = string.Empty;
    }
    protected void hf_removeAllPlugins_ValueChanged(object sender, EventArgs e) {
        NewDefaults.UpdateDefaults("PluginsToInstall", string.Empty);
        UpdateDrDefaults();
        LoadSiteCustomizations();

        hf_removeAllPlugins.Value = string.Empty;
    }

    protected void btn_WorkspaceMode_Click(object sender, EventArgs e) {
        if (DrDefaults.Count == 0) {
            Member.UpdateWorkspaceMode(ddl_WorkspaceMode.SelectedValue);
        }
        else {
            NewDefaults.UpdateDefaults("WorkspaceMode", ddl_WorkspaceMode.SelectedValue);
            UpdateDrDefaults();
        }

        string redirectUrl = Request.RawUrl;
        if (string.IsNullOrEmpty(Request.QueryString["tab"])) {
            if (Request.QueryString.Count > 0) {
                redirectUrl += "&tab=pnl_SiteCustomizations";
            }
            else {
                redirectUrl += "?tab=pnl_SiteCustomizations";
            }
        }

        Response.Redirect(redirectUrl);
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
                    if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.animationSpeed=" + tempOut.ToString() + ";");
                    }
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
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
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

    protected void rb_sitetipsonload_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SiteTipsOnPageLoad", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSiteTipsOnPageLoad(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_sitetipsonload_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SiteTipsOnPageLoad", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSiteTipsOnPageLoad(false);
        }

        LoadSiteCustomizations();
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
    }

    protected void dd_appStyle_Changed(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("UserAppStyle", dd_appStyle.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateUserAppStyle(dd_appStyle.SelectedValue);
        }

        LoadSiteCustomizations();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
    }

    protected void btn_defaultbodyfontfamily_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("DefaultBodyFontFamily", dd_defaultbodyfontfamily.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateDefaultBodyFontFamily(dd_defaultbodyfontfamily.SelectedValue);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void lbtn_defaultbodyfontfamily_reset_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("DefaultBodyFontFamily", string.Empty);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateDefaultBodyFontFamily(string.Empty);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void btn_defaultfontsize_Click(object sender, EventArgs e) {
        string fontSize = tb_defaultfontsize.Text;
        if (!string.IsNullOrEmpty(fontSize)) {
            int tempSize = 0;
            if (int.TryParse(fontSize, out tempSize) && tempSize > 0) {
                if (DrDefaults.Count > 0) {
                    NewDefaults.UpdateDefaults("DefaultBodyFontSize", fontSize);
                    UpdateDrDefaults();
                }
                else {
                    Member.UpdateDefaultBodyFontSize(fontSize);
                    if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                        ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
                    }
                }
            }
        }
        else {
            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("DefaultBodyFontSize", string.Empty);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateDefaultBodyFontSize(string.Empty);
                if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
                }
            }
        }

        LoadSiteCustomizations();
    }
    protected void lbtn_defaultfontsize_clear_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("DefaultBodyFontSize", string.Empty);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateDefaultBodyFontSize(string.Empty);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void btn_defaultfontcolor_Click(object sender, EventArgs e) {
        string fontColor = tb_defaultfontcolor.Text;
        if (!string.IsNullOrEmpty(fontColor)) {
            if (!fontColor.StartsWith("#")) {
                fontColor = "#" + fontColor;
            }

            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("DefaultBodyFontColor", fontColor);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateDefaultBodyFontColor(fontColor);
                if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=SiteCustomizations");
                }
            }
        }
        else {
            if (DrDefaults.Count > 0) {
                NewDefaults.UpdateDefaults("DefaultBodyFontColor", string.Empty);
                UpdateDrDefaults();
            }
            else {
                Member.UpdateDefaultBodyFontColor(string.Empty);
                if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=SiteCustomizations");
                }
            }
        }

        LoadSiteCustomizations();
    }
    protected void lbtn_defaultfontcolor_clear_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("DefaultBodyFontColor", string.Empty);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateDefaultBodyFontColor(string.Empty);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
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
            string script = "$('#DateDisplay').hide();$('#localtime').hide();$('#DateDisplay').html('" + ServerSettings.ServerDateTime.ToString("dddd, MMM dd") + "');startCurrentTime();";
            script += "$('#DateDisplay').show();$('#localtime').show();";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadSiteCustomizations();
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
            string script = "$('#DateDisplay').hide();$('#localtime').hide();";
            script += "clearInterval(startCurrentTime_interval);$('#DateDisplay').html('');$('#localtime').html('');";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadSiteCustomizations();
    }

    protected void btn_ProfileLinkStyle_Click(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ProfileLinkStyle", dd_ProfileLinkStyle.SelectedValue);
            UpdateDrDefaults();
        }
        else {
            Member.UpdateProfileLinkStyle(dd_ProfileLinkStyle.SelectedValue);
        }

        LoadSiteCustomizations();
        SetUserImage();
    }

    protected void rb_showtopsearch_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowTopSearch", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowTopSearch(true);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            string script = "$('#app_search_tab').show();";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadSiteCustomizations();
    }
    protected void rb_showtopsearch_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowTopSearch", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowTopSearch(false);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            string script = "$('#app_search_tab').hide();";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }

        LoadSiteCustomizations();
    }

    protected void rb_SidebarAccordion_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarAccordion", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarAccordion(true);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_SidebarAccordion_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarAccordion", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarAccordion(false);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_SidebarAccordionMutliOpenAllowed_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarAccordionMutliOpenAllowed", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarAccordionMutliOpenAllowed(false);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_SidebarAccordionMutliOpenAllowed_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarAccordionMutliOpenAllowed", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarAccordionMutliOpenAllowed(true);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_HideSidebarMenuIcons_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideSidebarMenuIcons", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideSidebarMenuIcons(true);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_HideSidebarMenuIcons_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HideSidebarMenuIcons", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHideSidebarMenuIcons(false);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_ShowSiteToolsInCategories_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsInCategories", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsInCategories(true);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_ShowSiteToolsInCategories_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsInCategories", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsInCategories(false);
            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_linksnewpage_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LoadLinksBlankPage", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLoadLinksBlankPage(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_linksnewpage_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("LoadLinksBlankPage", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateLoadLinksBlankPage(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_showpagedescriptions_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsPageDescription", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsPageDescription(true);

            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_showpagedescriptions_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsPageDescription", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsPageDescription(false);

            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_ShowSiteToolsIcons_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsIcons", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsIcons(true);

            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }
    protected void rb_ShowSiteToolsIcons_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowSiteToolsIcons", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowSiteToolsIcons(false);

            if (Member.Username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                ServerSettings.PageToolViewRedirect(Page, "AcctSettings.aspx?tab=pnl_SiteCustomizations");
            }
        }

        LoadSiteCustomizations();
    }

    protected void rb_UseAltSidebar_On_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("UseAltSidebarTheme", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateUseAltSidebarTheme(true);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#sidebar_menulinks, #accordian-sidebar, #sidebar-accordian').addClass('sidebar-alt-view');");
        }

        LoadSiteCustomizations();
    }
    protected void rb_UseAltSidebar_Off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("UseAltSidebarTheme", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateUseAltSidebarTheme(false);
        }

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#sidebar_menulinks, #accordian-sidebar, #sidebar-accordian').removeClass('sidebar-alt-view');");
        }

        LoadSiteCustomizations();
    }

    protected void rb_SidebarTransparencyMode_On_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarTransparencyMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarTransparencyMode(true);
        }

        LoadSiteCustomizations();

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.setSidebarTransparency();");
        }
    }
    protected void rb_SidebarTransparencyMode_Off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("SidebarTransparencyMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateSidebarTransparencyMode(false);
        }

        LoadSiteCustomizations();

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.removeSidebarTransparency();");
        }
    }

    protected void rb_TopbarTransparencyMode_On_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TopbarTransparencyMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTopbarTransparencyMode(true);
        }

        LoadSiteCustomizations();

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.setTopbarTransparency();");
        }
    }
    protected void rb_TopbarTransparencyMode_Off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TopbarTransparencyMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTopbarTransparencyMode(false);
        }

        LoadSiteCustomizations();

        if (HttpContext.Current.User.Identity.Name.ToLower() == Username.ToLower()) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.removeTopbarTransparency();");
        }
    }

    protected void rb_autohidemode_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AutoHideMode", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoHideMode(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_autohidemode_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("AutoHideMode", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateAutoHideMode(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_taskbarShowAll_On_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TaskBarShowAll", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTaskBarShowAll(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_taskbarShowAll_Off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("TaskBarShowAll", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateTaskBarShowAll(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_showWorkspacePreview_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HoverPreviewWorkspace", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHoverPreviewWorkspace(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_showWorkspacePreview_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("HoverPreviewWorkspace", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateHoverPreviewWorkspace(false);
        }

        LoadSiteCustomizations();
    }

    protected void rb_showPreview_on_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowMinimizedPreview", "1");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowMinimizedPreview(true);
        }

        LoadSiteCustomizations();
    }
    protected void rb_showPreview_off_CheckedChanged(object sender, EventArgs e) {
        if (DrDefaults.Count > 0) {
            NewDefaults.UpdateDefaults("ShowMinimizedPreview", "0");
            UpdateDrDefaults();
        }
        else {
            Member.UpdateShowMinimizedPreview(false);
        }

        LoadSiteCustomizations();
    }

    #endregion

}
