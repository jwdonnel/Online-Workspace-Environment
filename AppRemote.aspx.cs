using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Web.Security;
using System.Text;
using System.IO;
using System.Data;
using System.Threading;
using System.Globalization;
using System.Xml;
using System.Net.Mail;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using SocialSignInApis;
using OpenWSE.Core.Licensing;
using OpenWSE_Tools.GroupOrganizer;
using System.Drawing;

public partial class AppRemote : System.Web.UI.Page {

    #region private variables

    private IPWatch _ipwatch = new IPWatch(false);
    private ServerSettings _ss = new ServerSettings();
    private readonly StringBuilder _strScriptreg = new StringBuilder();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly Notifications _notifi = new Notifications();
    private Dictionary<string, string> _demoMemberDatabase;
    private NewUserDefaults _demoCustomizations;
    private List<string> _groupname;
    private MemberDatabase _member;
    private MembershipUser _membershipuser;
    private string _username;
    private App _apps;
    private string _sitetheme;
    private string _ipAddress;
    private string _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

    #endregion


    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile()) {
            Page.Response.Redirect("~/SiteTools/ServerMaintenance/LicenseManager.aspx");
        }

        ServerSettings.AddMetaTagsToPage(this.Page);

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();

        IIdentity userId = HttpContext.Current.User.Identity;
        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userId, IsPostBack, _ss.NoLoginRequired);
        pageLoadInit.CheckSSLRedirect();

        if (!pageLoadInit.CheckIfLicenseIsValid()) {
            Page.Response.Redirect("~/SiteTools/ServerMaintenance/LicenseManager.aspx");
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "appRemote_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';appRemote_Config.ShowLoginModalOnDemoMode=" + _ss.ShowLoginModalOnDemoMode.ToString().ToLower() + ";");

        lblHomePageLink.Text = "<a href='Workspace.aspx?" + ServerSettings.OverrideMobileSessionString.ToLower() + "=true' class='cursor-pointer'>Home</a>";

        if (userId.IsAuthenticated) {
            if (pageLoadInit.CanLoadPage) {
                _ipAddress = pageLoadInit.IpAddress;

                if (userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    if (Session["SocialGroupLogin"] != null) {
                        string requestGroup = Session["SocialGroupLogin"].ToString();
                        Groups group = new Groups();
                        group.getEntries(requestGroup);
                        if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                            WriteGroupSession(userId.Name);
                        }
                    }
                }

                StartUpPage(userId);

                if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    chat_header_btn.Visible = false;
                    apps_header_btn.Visible = false;
                    pnl_chat_users.Enabled = false;
                    pnl_chat_users.Visible = false;
                    pnl_icons.Enabled = false;
                    pnl_icons.Visible = false;
                    pnl_chat_popup.Enabled = false;
                    pnl_chat_popup.Visible = false;
                    pnl_options.Enabled = false;
                    pnl_options.Visible = false;
                    pages_header_btn.Visible = false;
                    RegisterPostbackScripts.RegisterStartupScript(this, "appRemote.SetAdminMode();");
                }
            }
            else
                Page.Response.Redirect("~/ErrorPages/Error.html");
        }
        else {
            if (_ss.AddBackgroundToLogo) {
                _strScriptreg.Append("appRemote.AddBackgroundColorToLogo('" +  _ss.LogoBackgroundColor + "');");
            }

            string sitename = CheckLicense.SiteName;
            if (!string.IsNullOrEmpty(sitename)) {
                lbl_UserName.Text = sitename + " Login";
                lbl_UserName.Style["padding-left"] = "10px";
                Page.Title = sitename + " Login Portal";
            }
            else {
                lbl_UserName.Text = "Login / Register";
                lbl_UserName.Style["padding-left"] = "10px";
            }

            if (!_ss.AllowUserSignUp) {
                _strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
            }

            if (!_ss.EmailSystemStatus) {
                _strScriptreg.Append("$('#lnk_forgotpassword').remove();");
            }

            GetStartupScripts_JS();
            GetStartupScripts_CSS();

            pnl_login.Enabled = true;
            pnl_login.Visible = true;
            pnl_icons.Enabled = false;
            pnl_icons.Visible = false;
            pnl_options.Enabled = false;
            pnl_options.Visible = false;
            lbl_UserName.Enabled = true;
            lbl_UserName.Visible = true;
            pnl_adminPages.Enabled = false;
            pnl_adminPages.Visible = false;
            pnl_chat_users.Enabled = false;
            pnl_chat_users.Visible = false;
            pnl_adminPages.Enabled = false;
            pnl_adminPages.Visible = false;
            pnl_adminPage_iframe.Enabled = false;
            pnl_adminPage_iframe.Visible = false;
            pnl_chat_popup.Enabled = false;
            pnl_chat_popup.Visible = false;
            lb_signoff.Enabled = false;
            lb_signoff.Visible = false;
            apps_header_btn.Visible = false;
            chat_header_btn.Visible = false;
            pages_header_btn.Visible = false;
            login_header_btn.Visible = false;

            if (!_ss.SignInWithGoogle) {
                lbtn_signinwith_Google.Enabled = false;
                lbtn_signinwith_Google.Visible = false;
            }

            if (!_ss.SignInWithTwitter) {
                lbtn_signinwith_Twitter.Enabled = false;
                lbtn_signinwith_Twitter.Visible = false;
            }

            if (!_ss.SignInWithFacebook) {
                lbtn_signinwith_Facebook.Enabled = false;
                lbtn_signinwith_Facebook.Visible = false;
            }

            if (!_ss.SignInWithGoogle && !_ss.SignInWithTwitter && !_ss.SignInWithFacebook) {
                _strScriptreg.Append("$('.loginwith-api-text').remove();");
            }

            if ((_ss.NoLoginRequired) && (!userId.IsAuthenticated) && (!_ss.ForceGroupLogin)) {
                if (pageLoadInit.CanLoadPage) {
                    Page.Title = sitename + " Mobile Workspace";

                    _ipAddress = pageLoadInit.IpAddress;
                    _demoCustomizations = pageLoadInit.DemoCustomizations;
                    if (_demoCustomizations != null) {
                        _demoMemberDatabase = _demoCustomizations.DefaultTable;
                    }

                    _sitetheme = pageLoadInit.SiteTheme;
                    if (string.IsNullOrEmpty(_sitetheme))
                        _sitetheme = "Standard";

                    UserIsNotAuthenticated();
                }
            }
            else {
                CustomFonts.SetCustomValues(Page);
                menu_header_btn.Visible = false;
            }

            string requestGroup = Request.QueryString["group"];
            if (!string.IsNullOrEmpty(requestGroup)) {
                if (requestGroup.Contains("#")) {
                    requestGroup = requestGroup.Replace(requestGroup.Substring(requestGroup.IndexOf("#")), "");
                }

                Groups group = new Groups();
                group.getEntries(requestGroup);
                if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                    Dictionary<string, string> g = group.group_dt[0];
                    SetGroupLogo(g);

                    NewUserDefaults tempDefaults = new NewUserDefaults(requestGroup);
                    tempDefaults.GetDefaults();
                    if (tempDefaults.DefaultTable.Count > 0) {
                        _demoCustomizations = tempDefaults;
                        _demoMemberDatabase = tempDefaults.DefaultTable;
                    }
                }
                else {
                    SetMainLogo();
                }
            }
            else if (Session["SocialGroupLogin"] != null) {
                requestGroup = Session["SocialGroupLogin"].ToString();
                Groups group = new Groups();
                group.getEntries(requestGroup);
                if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                    Dictionary<string, string> g = group.group_dt[0];
                    SetGroupLogo(g);

                    NewUserDefaults tempDefaults = new NewUserDefaults(requestGroup);
                    tempDefaults.GetDefaults();
                    if (tempDefaults.DefaultTable.Count > 0) {
                        _demoCustomizations = tempDefaults;
                        _demoMemberDatabase = tempDefaults.DefaultTable;
                    }
                }
                else {
                    SetMainLogo();
                }
            }
            else {
                SetMainLogo();
            }

            _strScriptreg.Append("appRemote_Config.siteTheme='Standard'; $('#notifications').remove(); $('#notifications-viewtable').remove();");
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }

        LoadUserBackground();
        SetSidebarMenuIcons();
    }
    private void StartUpPage(IIdentity userId) {
        string sitename = CheckLicense.SiteName;
        _sitetheme = "Standard";
        if (userId.IsAuthenticated) {
            _username = userId.Name;
            _membershipuser = Membership.GetUser(_username);
            if ((!userId.IsAuthenticated) || ((_membershipuser.IsLockedOut) || (!_membershipuser.IsApproved)))
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            else {
                _member = new MemberDatabase(_username);

                if (_member.AdminPagesList.Length == 0 && Roles.IsUserInRole(_member.Username, "Standard")) {
                    FormsAuthentication.SignOut();
                    Page.Response.Redirect("~/AppRemote.aspx");
                }

                if (_ss.ForceGroupLogin && _member.GroupList.Count == 1 && !GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    GroupSessions.AddOrSetNewGroupLoginSession(_username, _member.GroupList[0]);
                    _member = new MemberDatabase(_username);
                }

                _groupname = _member.GroupList;

                if ((_groupname.Count == 0) && (!_member.IsNewMember)) {
                    if (!ServerSettings.AdminPagesCheck("GroupOrg", _username))
                        Page.Response.Redirect("~/ErrorPages/Blocked.html");
                }

                _sitetheme = _member.SiteTheme;
                if (!string.IsNullOrEmpty(_sitetheme))
                    _sitetheme = "Standard";

                Membership.GetAllUsers();
                _apps = new App(_username);

                if (!IsPostBack) {
                    UserIsAuthenticated();
                }
            }
        }
    }
    protected void UserIsAuthenticated() {
        if (!IsPostBack) {
            _strScriptreg.Append("appRemote_Config.saveCookiesAsSessions = " + _ss.SaveCookiesAsSessions.ToString().ToLower() + ";");
            _strScriptreg.Append("appRemote_Config.siteTheme = '" + _sitetheme + "';");
            _strScriptreg.Append("appRemote_Config.animationSpeed=" + _member.AnimationSpeed + ";");
            _strScriptreg.Append("appRemote_Config.siteTipsOnPageLoad=" + _member.SiteTipsOnPageLoad.ToString().ToLower() + ";");
            _strScriptreg.Append("appRemote_Config.showToolTips=" + _member.ShowToolTips.ToString().ToLower() + ";");
            if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                _strScriptreg.Append("appRemote_Config.autoSync=" + _member.MobileAutoSync.ToString().ToLower() + ";");
            }

            _workspaceMode = _member.WorkspaceMode.ToString();
            if (!MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                _strScriptreg.Append("appRemote_Config.workspaceMode='" + _workspaceMode + "';");
                connect_header_btn.Visible = false;
            }

            string acctImg = _member.AccountImage;
            lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(_member.AccountImage, _member.UserColor, _member.UserId, HelperMethods.MergeFMLNames(_member), _sitetheme, _member.ProfileLinkStyle);
            UserImageColorCreator.ApplyProfileLinkStyle(_member.ProfileLinkStyle, _member.UserColor, this.Page);
            lbl_UserFullName.Text = _member.Username;
            lbl_UserEmail.Text = _membershipuser.Email;
            if (!string.IsNullOrEmpty(acctImg)) {
                string imgLoc = ResolveUrl("~/Standard_Images/AcctImages/" + _member.UserId + "/" + acctImg);
                if (acctImg.ToLower().Contains("http") || acctImg.ToLower().Contains("www.")) {
                    imgLoc = acctImg;
                }
                img_Profile.ImageUrl = imgLoc;
            }
            else {
                img_Profile.ImageUrl = ResolveUrl("~/Standard_Images/EmptyUserImg.png");
            }

            GetStartupScripts_JS();
            GetStartupScripts_CSS();
            CustomFonts.SetCustomValues(Page, _member);

            if (!ServerSettings.AdminPagesCheck("acctsettings_aspx", _username)) {
                hyp_accountSettings.Enabled = false;
                hyp_accountSettings.Visible = false;
            }

            if (_ss.ForceGroupLogin && !GroupSessions.DoesUserHaveGroupLoginSessionKey(_username) && _username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                workspace_selector_btn.Visible = false;
                workspace_selector.Visible = false;
                pnl_icons.Visible = false;
                pnl_adminPages.Visible = false;
                pnl_adminPage_iframe.Visible = false;
                _strScriptreg.Append("$('#group-btns-holder, #notifications-viewtable').remove();");
                _strScriptreg.Append("appRemote_Config.forceGroupLogin=true;");
            }
            else {
                AppIconBuilder _aib = new AppIconBuilder(Page, _member);
                _aib.BuildAppsForUser();

                BuildAdminPages();
                SetNumberOfWorkspaces();
                GetUserPlugins();

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    string groupName = new Groups().GetGroupName_byID(GroupSessions.GetUserGroupSessionName(_username));

                    if (_member.GetCompleteUserGroupList.Count == 1) {
                        groupLogout.Visible = true;
                        if (string.IsNullOrEmpty(groupName)) {
                            groupName = "<span class='grouplogin-img'></span>Log out of Group";
                        }
                        else {
                            groupName = "<span class='grouplogin-img'></span>Log out of " + groupName;
                        }
                        groupLogout.InnerHtml = groupName;
                    }
                    else {
                        changeGroupLogin.Visible = true;
                        changeGroupLogin.InnerHtml = "<span class='grouplogin-img'></span>Change (" + groupName + ")";
                    }
                    groupLogin.Visible = false;

                    aGroupLogoff.Enabled = true;
                    aGroupLogoff.Visible = true;

                    Groups tempGroup = new Groups();
                    tempGroup.getEntries(GroupSessions.GetUserGroupSessionName(_username));

                    if (tempGroup.group_dt.Count > 0) {
                        SetGroupLogo(tempGroup.group_dt[0]);
                    }
                }
                else {
                    groupLogout.Visible = false;
                    groupLogin.Visible = true;
                }

                if ((_member != null && _member.GetCompleteUserGroupList.Count == 1 && _ss.ForceGroupLogin) || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    groupLogout.Visible = false;
                    groupLogin.Visible = false;
                }

                _strScriptreg.Append("appRemote.CheckForNewNotifications();");
            }

            CheckIfNeedToHideNotificationsButton();

            if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                aGroupLogoff.Visible = false;
                aGroupLogoff.Enabled = false;
                menu_header_btn.Visible = false;
            }

            if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
                RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
                _strScriptreg.Clear();
            }
        }
    }
    protected void UserIsNotAuthenticated() {
        if (!IsPostBack) {
            connect_header_btn.Visible = false;
            pnl_icons.Enabled = true;
            pnl_icons.Visible = true;
            pnl_options.Enabled = true;
            pnl_options.Visible = true;
            apps_header_btn.Visible = true;
            login_header_btn.Visible = true;
            workspace_selector.Visible = true;
            workspace_header_btn.Visible = false;

            if (!string.IsNullOrEmpty(Request.QueryString["group"])) {
                pnl_icons.Enabled = false;
                pnl_icons.Visible = false;
                apps_header_btn.Visible = false;
            }

            bool siteTips = false;
            if ((HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["SiteTipsOnPageLoad"])) || (string.IsNullOrEmpty(_demoMemberDatabase["SiteTipsOnPageLoad"])))
                siteTips = true;

            _strScriptreg.Append("appRemote.SetDemoMode();");
            _strScriptreg.Append("appRemote_Config.saveCookiesAsSessions = " + _ss.SaveCookiesAsSessions.ToString().ToLower() + ";");
            _strScriptreg.Append("appRemote_Config.siteTheme = '" + _sitetheme + "';");
            _strScriptreg.Append("appRemote_Config.animationSpeed=" + _demoMemberDatabase["AnimationSpeed"] + ";");
            _strScriptreg.Append("appRemote_Config.siteTipsOnPageLoad=" + siteTips.ToString().ToLower() + ";");
            _strScriptreg.Append("appRemote_Config.workspaceMode='" + MemberDatabase.UserWorkspaceMode.Simple.ToString() + "';");
            _strScriptreg.Append("appRemote_Config.showToolTips=" + HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ToolTips"]).ToString().ToLower() + ";");

            AppIconBuilder _aib = new AppIconBuilder(Page, _demoMemberDatabase);
            _aib.BuildAppsForDemo();

            GetUserPlugins();

            if (_ss.NoLoginRequired) {
                CustomFonts.SetCustomValues(Page, _demoMemberDatabase);
            }
            else {
                CustomFonts.SetCustomValues(Page);
            }
        }
    }
    private void SetNumberOfWorkspaces() {
        dropdownSelector.Items.Clear();
        ddl_appDropdownSelector.Items.Clear();

        int count = _member.TotalWorkspaces;
        if (count == 1) {
            ListItem item = new ListItem("1", "1");
            dropdownSelector.Items.Add(item);
            ddl_appDropdownSelector.Items.Add(item);

            StringBuilder str = new StringBuilder();
            str.Append("$('#workspace_header_btn').hide();");
            str.Append("$('#app-workspace-selector').hide();");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }
        else {
            for (int i = 0; i < count; i++) {
                string val = (i + 1).ToString();
                ListItem item = new ListItem(val, val);
                dropdownSelector.Items.Add(item);
                ddl_appDropdownSelector.Items.Add(item);
            }
        }
    }

    private void SetSidebarMenuIcons() {
        string appIcon = _ss.GetSidebarCategoryIcon("My Apps");
        string appImg = SideBarItems.EmptySidebarImg;

        string chatIcon = _ss.GetSidebarCategoryIcon("Chat");
        string chatImg = SideBarItems.EmptySidebarImg;

        string toolIcon = _ss.GetSidebarCategoryIcon("Custom Tools");
        string adminPagesImg = SideBarItems.EmptySidebarImg;

        if (!_ss.HideAllAppIcons && (_member == null || !_member.HideSidebarMenuIcons)) {
            if (!string.IsNullOrEmpty(appIcon)) {
                if (appIcon.StartsWith("~/")) {
                    appIcon = ResolveUrl(appIcon.Trim());
                }
                else {
                    appIcon = appIcon.Trim();
                }
                appImg = "<img alt='' src='" + appIcon + "' class='menu-img' />";
            }

            if (!string.IsNullOrEmpty(chatIcon)) {
                if (chatIcon.StartsWith("~/")) {
                    chatIcon = ResolveUrl(chatIcon.Trim());
                }
                else {
                    chatIcon = chatIcon.Trim();
                }
                chatImg = "<img alt='' src='" + chatIcon + "' class='menu-img' />";
            }

            if (!string.IsNullOrEmpty(toolIcon)) {
                if (toolIcon.StartsWith("~/")) {
                    toolIcon = ResolveUrl(toolIcon.Trim());
                }
                else {
                    toolIcon = toolIcon.Trim();
                }
                adminPagesImg = "<img alt='' src='" + toolIcon + "' class='menu-img' />";
            }
        }

        if (!HttpContext.Current.User.Identity.IsAuthenticated) {
            apps_header_btn.InnerHtml = "<div id='wl-s' class='section-pad section-link'>" + appImg + "Available Apps</div>";
        }
        else if (HttpContext.Current.User.Identity.IsAuthenticated && GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
            apps_header_btn.InnerHtml = "<div id='wl-s' class='section-pad section-link'>" + appImg + "Group Apps</div>";
        }
        else {
            string acctSettingsEdit = string.Empty;
            if (ServerSettings.AdminPagesCheck("acctsettings", HttpContext.Current.User.Identity.Name)) {
                acctSettingsEdit = "<a href='" + ResolveUrl("~/SiteTools/UserMaintenance/AcctSettings.aspx?mobileMode=true#?tab=pnl_IconSelector") + "' class='sidebar-edit-btn' title='Edit App Selector Style' onclick=\"appRemote.StartLoadingOverlay('Loading');return true;\"></a>";
            }

            apps_header_btn.InnerHtml = "<div id='wl-s' class='section-pad section-link'>" + appImg + "My Apps" + acctSettingsEdit + "</div>";
        }

        chat_header_btn.InnerHtml = "<div id='cc-s' class='section-pad section-link'>" + chatImg + "Chat</div>";
        pages_header_btn.InnerHtml = "<div id='ap-s' class='section-pad section-link'>" + adminPagesImg + " Settings and Tools</div>";

        if (string.IsNullOrEmpty(_sitetheme)) {
            _sitetheme = "Standard";
        }
    }

    private void CheckIfNeedToHideNotificationsButton() {
        if (!string.IsNullOrEmpty(_username)) {
            int total = 0;
            Notifications tempNotifications = new Notifications();
            UserNotificationMessages _usernoti = new UserNotificationMessages(_username);
            _usernoti.getEntries("ASC");

            foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                Notifications_Coll coll = tempNotifications.GetNotification(x.NotificationID);
                if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!string.IsNullOrEmpty(x.ID))) {
                    total++;
                }
            }

            if (total == 0) {
                tempNotifications.GetUserNotifications(_username);
                if (tempNotifications.UserNotifications.Count == 0) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#notifications').remove(); $('#notifications-viewtable').remove();");
                }
            }
        }
    }

    private void GetStartupScripts_JS() {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            try {
                if ((coll.ApplyTo == "All Components") || (coll.ApplyTo == "App Remote")) {
                    var sref = new ScriptReference(coll.ScriptPath);
                    if (sm != null)
                        sm.Scripts.Add(sref);
                }
                else if (coll.ApplyTo == "Chat Client") {
                    EnableDisableChat(coll.ScriptPath);
                }
            }
            catch { }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }
    private void GetStartupScripts_CSS() {
        var startupscripts = new StartupStyleSheets(true);

        if (string.IsNullOrEmpty(_sitetheme))
            _sitetheme = "Standard";

        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            string scripttheme = coll.Theme;
            if (string.IsNullOrEmpty(scripttheme))
                scripttheme = "Standard";

            if ((scripttheme == _sitetheme) || (scripttheme == "All")) {
                if ((string.IsNullOrEmpty(coll.ApplyTo)) || (coll.ApplyTo == "All Components")
                    || (coll.ApplyTo == "Base/Workspace") || (coll.ApplyTo == "App Remote"))
                    startupscripts.AddCssToPage(coll.ScriptPath, Page);
            }
        }
    }
    private void EnableDisableChat(string script) {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        bool turnOffChat = false;
        if (_ss.ChatEnabled) {
            if ((_member.ChatEnabled) && (_groupname.Count > 0)) {
                hf_chatsound.Value = _member.ChatSoundNoti .ToString().ToLower();

                var sref = new ScriptReference(script);
                if (sm != null) sm.Scripts.Add(sref);

                if (_member.IsAway) {
                    ChatService cs = new ChatService();
                    cs.UpdateStatus("Available");
                }
            }
            else
                turnOffChat = true;
        }
        else
            turnOffChat = true;

        if (turnOffChat) {
            chat_header_btn.Visible = false;
            pnl_chat_users.Enabled = false;
            pnl_chat_users.Visible = false;
            pnl_chat_popup.Enabled = false;
            pnl_chat_popup.Visible = false;
        }
    }

    private void SetMainLogo() {
        #region OPACITY
        string logoopacity = _ss.LogoOpacity;
        if (!string.IsNullOrEmpty(logoopacity)) {
            double tempOut = 0.0d;
            if (double.TryParse(logoopacity, out tempOut)) {
                string ieFilter = "alpha(opacity=" + (tempOut * 100.0d).ToString() + ")";
                _strScriptreg.Append("$('#img_icon_logo').css('opacity', '" + logoopacity + "');$('#img_icon_logo').css('filter', '" + ieFilter + "');");
            }
        }
        #endregion
    }
    private void SetGroupLogo(Dictionary<string, string> group) {
        if (group.Count == 0) {
            SetMainLogo();
            return;
        }

        string sitename = group["GroupName"];
        NewUserDefaults tempDefaults = new NewUserDefaults(group["GroupID"]);
        tempDefaults.GetDefaults();

        if (tempDefaults.DefaultTable.Count == 0) {
            SetMainLogo();
            return;
        }

        string theme = tempDefaults.DefaultTable["Theme"];
        if (string.IsNullOrEmpty(theme)) {
            theme = "Standard";
        }

        if (HelperMethods.ConvertBitToBoolean(group["IsURL"])) {
            string imgUrl = group["Image"];
            if (imgUrl.StartsWith("~/")) {
                imgUrl = ResolveUrl(imgUrl);
            }

            img_icon_logo.ImageUrl = imgUrl;
        }
        else if (!string.IsNullOrEmpty(group["Image"])) {
            img_icon_logo.ImageUrl = ResolveUrl("~/Standard_Images/Groups/Logo/" + group["Image"]);
        }

        if (!User.Identity.IsAuthenticated) {
            hyp_groupLogin.Visible = false;
            hyp_cancelGroupLogin.Enabled = true;
            hyp_cancelGroupLogin.Visible = true;

            _strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");

            lbl_UserName.Text = "Log into " + sitename;
            lbl_UserName.Style["padding-left"] = "10px";
        }

        SetMainLogo();
    }

    protected void SignOff_Clicked(object sender, EventArgs e) {
        if (_member != null) {
            Thread.Sleep(500);
            if (_member.ClearPropOnSignOff) {
                var app = new App(_username);
                app.DeleteUserProperties(_username);

                HttpCookieCollection cookieColl = new HttpCookieCollection();
                foreach (object key in Request.Cookies.Keys) {
                    HttpCookie cookie = Request.Cookies[key.ToString()];
                    if (cookie != null) {
                        cookie.Expires = ServerSettings.ServerDateTime.AddDays(-1d);
                        cookieColl.Add(cookie);
                    }
                }

                foreach (object c in cookieColl.Keys) {
                    HttpCookie cookie = Request.Cookies[c.ToString()];
                    Response.Cookies.Add(cookie);
                }
            }
            int hour = ServerSettings.ServerDateTime.Hour;
            int min = ServerSettings.ServerDateTime.Minute;
            int seconds = ServerSettings.ServerDateTime.Second;

            if (min >= 20)
                min = min - 20;
            else {
                int tempmin = min - 20;
                min = 60 + tempmin;
                if (hour > 1)
                    hour = hour - 1;
                else
                    hour = 24 - hour - 1;
            }
            int year = ServerSettings.ServerDateTime.Year;
            int month = ServerSettings.ServerDateTime.Month;
            int day = ServerSettings.ServerDateTime.Day - 1;
            if (ServerSettings.ServerDateTime.Day <= 1) {
                month = ServerSettings.ServerDateTime.Month - 1;
                if (month <= 1) {
                    year = ServerSettings.ServerDateTime.Year - 1;
                    month = 1;
                }
                day = 28;
            }

            try {
                var newDate = new DateTime(ServerSettings.ServerDateTime.Year, month, day, hour, min, seconds);
                if (_membershipuser != null) {
                    _membershipuser.LastActivityDate = newDate;
                    if (!string.IsNullOrEmpty(_membershipuser.Email))
                        Membership.UpdateUser(_membershipuser);
                }
            }
            catch { }

            _member.UpdateChatTimeStamp();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser msu in coll) {
                string un = msu.UserName.ToLower();
                if ((msu.IsOnline) && (un != _username.ToLower())) {
                    if (ChatService._needRefreshUsers.ContainsKey(un))
                        ChatService._needRefreshUsers[un] = true;
                    else
                        ChatService._needRefreshUsers.Add(un, true);

                    if (ChatService._needRefreshMessager.ContainsKey(un))
                        ChatService._needRefreshMessager[un] = true;
                    else
                        ChatService._needRefreshMessager.Add(un, true);
                }
            }

            MemberDatabase.DeleteUserSessionId(_username);

            FormsAuthentication.SignOut();
            string groupName = string.Empty;
            if (ClearGroupSession(out groupName)) {
                Response.Redirect("~/AppRemote.aspx?group=" + groupName);
            }

            LoginActivity la = new LoginActivity();
            la.AddItem(_username, true, ActivityType.Logout);

            HttpContext.Current.Response.Redirect("~/AppRemote.aspx");
        }
    }
    protected void aGroupLogoff_Click(object sender, EventArgs e) {
        try {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                LoginActivity la = new LoginActivity();
                la.AddItem(_username, true, ActivityType.Logout);
                GroupSessions.RemoveGroupLoginSession(_username);
            }
        }
        catch { }

        Page.Response.Redirect("~/AppRemote.aspx");
    }

    private void LoadUserBackground() {
        string timer = "30";
        bool noBgStyle = false;

        if (_demoMemberDatabase != null) {
            if (_demoMemberDatabase.ContainsKey("BackgroundLoopTimer") && !string.IsNullOrEmpty(_demoMemberDatabase["BackgroundLoopTimer"])) {
                timer = _demoMemberDatabase["BackgroundLoopTimer"];
            }
            if (_demoMemberDatabase.ContainsKey("NoBGOnAppRemote") && (string.IsNullOrEmpty(_demoMemberDatabase["NoBGOnAppRemote"]) || HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["NoBGOnAppRemote"]))) {
                noBgStyle = true;
            }
        }
        else if (_member != null) {
            timer = _member.BackgroundLoopTimer;
            noBgStyle = _member.NoBGOnAppRemote;
        }
        else {
            noBgStyle = true;
        }

        string backgroundColor = "#FFFFFF";
        string backgroundPosition = "right center";
        string backgroundSize = "auto";
        string backgroundRepeat = "no-repeat";
        if (_demoMemberDatabase != null) {
            backgroundColor = _demoMemberDatabase["BackgroundColor"];
            backgroundPosition = _demoMemberDatabase["BackgroundPosition"];
            backgroundSize = _demoMemberDatabase["BackgroundSize"];
            if (string.IsNullOrEmpty(_demoMemberDatabase["BackgroundRepeat"]) || HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["BackgroundRepeat"])) {
                backgroundRepeat = "repeat";
            }
        }
        else if (_member != null) {
            backgroundColor = _member.BackgroundColor;
            backgroundPosition = _member.BackgroundPosition;
            backgroundSize = _member.BackgroundSize;
            if (_member.BackgroundRepeat) {
                backgroundRepeat = "repeat";
            }
        }

        if (string.IsNullOrEmpty(backgroundColor)) {
            backgroundColor = "#FFFFFF";
        }

        if (!backgroundColor.StartsWith("#")) {
            backgroundColor = "#" + backgroundColor;
        }

        if (noBgStyle) {
            LoadSolidUserColorBG(backgroundColor);
            SetMainBackgrounds(pnl_icon_toplogo_banner, backgroundColor, backgroundRepeat, backgroundPosition, backgroundSize, timer);
        }
        else {
            SetMainBackgrounds(main_body, backgroundColor, backgroundRepeat, backgroundPosition, backgroundSize, timer);
        }
    }
    private void LoadSolidUserColorBG(string backgroundColor) {
        if (main_body != null) {
            main_body.Style["background-color"] = backgroundColor;
        }

        string foreColor = "#252525";
        if (_demoMemberDatabase != null) {
            foreColor = _demoMemberDatabase["AppRemoteForeColor"];
        }
        else if (_member != null) {
            foreColor = _member.AppRemoteForeColor;
        }

        if (string.IsNullOrEmpty(foreColor)) {
            foreColor = "#252525";
        }

        if (!foreColor.StartsWith("#")) {
            foreColor = "#" + foreColor;
        }

        StringBuilder str = new StringBuilder();

        bool useDarkColor = HelperMethods.UseDarkTextColorWithBackground(backgroundColor);

        str.Append("appRemote_Config.foreColor='" + foreColor + "';");
        str.Append("appRemote_Config.useDarkColor=" + useDarkColor.ToString().ToLower() + ";");
        str.Append("appRemote_Config.needToSetColorMode=true;");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }
    private void SetMainBackgrounds(HtmlGenericControl control, string backgroundColor, string backgroundRepeat, string backgroundPosition, string backgroundSize, string timer) {
        string img = string.Empty;
        if (_demoMemberDatabase != null && _demoCustomizations != null) {
            img = _demoCustomizations.GetBackgroundImg(1);
        }
        else if (_member != null) {
            img = _member.GetBackgroundImg(1);
        }

        if (control != null) {
            if (!string.IsNullOrEmpty(img)) {
                string tempImg = img;
                List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (backgroundList.Count > 0) {
                    img = backgroundList[0];
                }

                if (img.Length == 6) {
                    control.Style["background"] = "#" + img;
                }
                else {
                    if (AcctSettings.IsValidHttpUri(img)) {
                        control.Style["background"] = backgroundColor + " url('" + img + "') " + backgroundRepeat + " " + backgroundPosition;
                    }
                    else {
                        control.Style["background"] = backgroundColor + " url('" + ResolveUrl("~/" + img) + "') " + backgroundRepeat + " " + backgroundPosition;
                    }
                    control.Style["background-size"] = backgroundSize;
                }

                RegisterPostbackScripts.RegisterStartupScript(this, "appRemote_Config.backgroundTimerLoop=" + timer + ";openWSE.BackgroundLoop('" + tempImg + "', '#" + control.ClientID + "');");
            }
            else {
                string userRole = _ss.UserSignUpRole;
                string theme = new NewUserDefaults(userRole).GetDefault("Theme");
                if (string.IsNullOrEmpty(theme)) {
                    theme = "Standard";
                }

                control.Style["background-image"] = "url('App_Themes/" + theme + "/Body/default-bg.jpg')";
                control.Style["background-color"] = backgroundColor;
                control.Style["background-size"] = backgroundSize;
                control.Style["background-repeat"] = backgroundRepeat;
                control.Style["background-position"] = backgroundPosition;
            }
        }
    }


    #region Build Admin Pages

    private void BuildAdminPages() {
        bool loadLinksToNewPage = _member.LoadLinksBlankPage;
        SideBarItems sidebar = new SideBarItems(_username);

        string liStructure = "<li id='{0}' class='a-body site-tools-tablist'><div class='menu-title'>{1}</span>{2}</div><div class='sidebar-divider'></div>{3}</li>";

        StringBuilder strTabs = new StringBuilder();
        strTabs.Append("<ul id='sidebar-accordian'>");

        if (_member.ShowSiteToolsInCategories) {
            SortedDictionary<string, string> adminPageCategories = sidebar.ListOfAdminPageCategories();
            foreach (KeyValuePair<string, string> keyVal in adminPageCategories) {
                string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, keyVal.Key);
                if (!string.IsNullOrEmpty(innerTabList)) {
                    string id = keyVal.Key.Replace(" ", "_") + "_tab_body";
                    innerTabList = "<div id='pnl_" + keyVal.Key.Replace(" ", "_") + "' class='li-pnl-tab'>" + innerTabList + "</div>";
                    string img = SideBarItems.EmptySidebarImg;
                    if (!_ss.HideAllAppIcons && (_member == null || !_member.HideSidebarMenuIcons) && !string.IsNullOrEmpty(keyVal.Value)) {
                        img = string.Format("<img alt='' src='{0}' class='menu-img'>", ResolveUrl(keyVal.Value));
                    }
                    strTabs.AppendFormat(liStructure, id, img, keyVal.Key, innerTabList);
                }
            }
        }
        else {
            string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, string.Empty);
            if (!string.IsNullOrEmpty(innerTabList)) {
                string noCategoryName = "Settings and Tools";
                string keyValImgNoCategory = _ss.GetSidebarCategoryIcon(noCategoryName);
                string id = noCategoryName.Replace(" ", "_") + "_tab_body";
                innerTabList = "<div id='pnl_" + noCategoryName.Replace(" ", "_") + "' class='li-pnl-tab'>" + innerTabList + "</div>";

                string img = SideBarItems.EmptySidebarImg;
                if (!_ss.HideAllAppIcons && (_member == null || !_member.HideSidebarMenuIcons) && !string.IsNullOrEmpty(keyValImgNoCategory)) {
                    img = string.Format("<img alt='' src='{0}' class='menu-img'>", Page.ResolveUrl(keyValImgNoCategory));
                }
                strTabs.AppendFormat(liStructure, id, img, "Settings/Tools", innerTabList);
            }
        }
        strTabs.Append("</ul>");

        ph_adminPageList.Controls.Clear();
        ph_adminPageList.Controls.Add(new LiteralControl(strTabs.ToString()));
    }

    #endregion


    #region Login

    protected void Login_LoggingIn(object sender, LoginCancelEventArgs e) {
        string email = Login1.UserName;
        MembershipUserCollection coll = Membership.GetAllUsers();

        int count = coll.Cast<MembershipUser>().Count(user => user.Email.ToLower() == email);

        if (count == 1) {
            string username = Membership.GetUserNameByEmail(email);

            if (!string.IsNullOrEmpty(username)) {
                Login1.UserName = username;
            }
        }

        MembershipUser mUser = Membership.GetUser(Login1.UserName);
        if (mUser != null) {
            if (new MemberDatabase(Login1.UserName).IsSocialAccount) {
                e.Cancel = true;
            }
        }

        if (!ServerSettings.CanLoginToGroup(Login1.UserName, Request.QueryString["group"], HttpContext.Current) && Login1.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            ltl_logingrouperror.Text = "<div style='color: #D80000;'>Not authorized for group. Please try again</div>";
            e.Cancel = true;
        }
    }
    protected void Login_Loggedin(object sender, EventArgs e) {
        bool cancontinue = false;
        var listener = new IPListener(false);
        NameValueCollection n = Request.ServerVariables;
        string remoteaddress = n["REMOTE_ADDR"];
        if (remoteaddress == "::1")
            remoteaddress = "127.0.0.1";
        if (Login1.UserName.ToLower() == ServerSettings.AdminUserName.ToLower())
            cancontinue = true;
        else if ((_ipwatch.CheckIfBlocked(remoteaddress)) && (Login1.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
        else {
            if (!listener.TableEmpty) {
                if (listener.CheckIfActive(remoteaddress))
                    cancontinue = true;
                else {
                    Page.Response.Redirect("~/ErrorPages/Blocked.html");
                }
            }
            else
                cancontinue = true;
        }

        if (!cancontinue) return;
        var member = new MemberDatabase(Login1.UserName);
        member.UpdateUserIpAddress(remoteaddress);
        EmailUser(member);

        member.UpdateChatTimeStamp();
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser msu in coll) {
            string un = msu.UserName.ToLower();
            if ((msu.IsOnline) && (un != Login1.UserName.ToLower())) {
                if (ChatService._needRefreshUsers.ContainsKey(un))
                    ChatService._needRefreshUsers[un] = true;
                else
                    ChatService._needRefreshUsers.Add(un, true);

                if (ChatService._needRefreshMessager.ContainsKey(un))
                    ChatService._needRefreshMessager[un] = true;
                else
                    ChatService._needRefreshMessager.Add(un, true);
            }
        }

        if (member.Username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            WriteGroupSession(member.Username);
        }

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, true, ActivityType.Login);

        if (!string.IsNullOrEmpty(member.DefaultLoginGroup) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(member.Username)) {
            GroupSessions.AddOrSetNewGroupLoginSession(member.Username, member.DefaultLoginGroup);
        }

        MemberDatabase.AddUserSessionIds(member.Username);
        ServerSettings.SetRememberMeOnLogin(Login1, Response);

        Page.Response.Redirect("~/AppRemote.aspx");
    }
    protected void Login_error(object sender, EventArgs e) {
        NameValueCollection n = Request.ServerVariables;
        string remoteaddress = n["REMOTE_ADDR"];
        if (remoteaddress == "::1")
            remoteaddress = "127.0.0.1";

        if (Login1.UserName.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            MembershipUser mu = Membership.GetUser(ServerSettings.AdminUserName);
            if (mu != null && ((mu.IsLockedOut) || (!mu.IsApproved))) {
                if (!mu.IsApproved) {
                    mu.IsApproved = true;
                }
                if (mu.IsLockedOut) {
                    mu.UnlockUser();
                }
                Membership.UpdateUser(mu);
            }
        }

        var listener = new IPListener(false);
        bool cancontinue = false;
        if (!listener.TableEmpty) {
            if (!listener.CheckIfActive(remoteaddress))
                cancontinue = true;
        }
        else
            cancontinue = true;

        if (!cancontinue) return;
        bool foundIp = false;
        _ipwatch = new IPWatch(true);
        for (var i = 0; i < _ipwatch.ipwatchdt.Count; i++) {
            if (_ipwatch.ipwatchdt[i]["IPAddress"].ToString() == remoteaddress) {
                int attempts = Convert.ToInt32(_ipwatch.ipwatchdt[i]["Attempts"].ToString()) + 1;
                _ipwatch.updateAttempts(_ipwatch.ipwatchdt[i]["IPAddress"].ToString(), attempts);

                if (attempts >= _ss.AutoBlockIPCount) {
                    BackupSite();
                }
                foundIp = true;
            }
        }

        if (!foundIp) {
            _ipwatch.addItem(remoteaddress, 1, false);

            if (1 >= _ss.AutoBlockIPCount) {
                BackupSite();
            }
        }

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, false, ActivityType.Login);
    }
    private void BackupSite() {
        try {
            if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Backups\\Temp")) {
                Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Backups\\Temp");
            }

            foreach (
                string filename in
                    Directory.GetFiles(ServerSettings.GetServerMapLocation + "Backups\\Temp")) {
                if (File.Exists(filename)) {
                    File.Delete(filename);
                }
            }
        }
        catch {
        }
        string f = "DBFull_" + ServerSettings.ServerDateTime.ToFileTime();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Temp\\" + f + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(ServerSettings.AdminUserName.ToLower(), loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize_Current(dbviewer.dt);

        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        string tDesc = "Full Database Download";
        WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);

        if (File.Exists(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
        }

        ServerSettings.DeleteBackupTempFolderFiles();
    }
    private void WriteToXml(string backupfile, string path, string desc) {
        try {
            if (File.Exists(backupfile)) {
                var fi = new FileInfo(path);
                var reader = new XmlTextReader(backupfile);
                var doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();

                XmlElement root = doc.DocumentElement;
                XmlElement newBackup = doc.CreateElement("Backup");
                var mem = new MemberDatabase(ServerSettings.AdminUserName.ToLower());
                var str = new StringBuilder();
                str.Append("<Filename>" + fi.Name + "</Filename>");
                str.Append("<Description>" + desc + "</Description>");
                str.Append("<BackupDate>" + fi.CreationTime.ToString(CultureInfo.InvariantCulture) + "</BackupDate>");
                str.Append("<RestoreDate>N/A</RestoreDate>");
                str.Append("<Size>" + HelperMethods.FormatBytes(fi.Length) + "</Size>");
                str.Append("<User>" + HelperMethods.MergeFMLNames(mem) + "</User>");

                newBackup.InnerXml = str.ToString();

                if (root != null) root.PrependChild(newBackup);

                //save the output to a file
                doc.Save(backupfile);
            }
            else {
                var doc = new XmlDocument();
                doc.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><Backups></Backups>");
                var writer = new XmlTextWriter(backupfile, null) { Formatting = Formatting.Indented };
                doc.Save(writer);
                writer.Close();
                WriteToXml(backupfile, path, desc);
            }
        }
        catch {
        }
    }
    private void EmailUser(MemberDatabase member) {
        string ipaddress = member.IpAddress;
        string date = ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture);
        foreach (var user in member.EmailUponLoginList) {
            MembershipUser u = Membership.GetUser(user);
            if (u != null && string.IsNullOrEmpty(u.Email)) continue;
            string fullname = HelperMethods.MergeFMLNames(member);
            if (u != null) SendEmailsForLogin(u.Email, member.Username, fullname, ipaddress, date);
        }
    }
    private void SendEmailsForLogin(string emailto, string loggedinusername, string loggedinuser, string ipaddress, string date) {
        try {
            var messagebody = new StringBuilder();
            messagebody.Append(loggedinuser + " has logged in:<br />");
            messagebody.Append("<b style='padding-right:5px'>Username:</b>" + loggedinusername + "<br />");
            messagebody.Append("<b style='padding-right:5px'>IP Address:</b>" + ipaddress + "<br />");
            messagebody.Append("<b style='padding-right:5px'>Date/Time:</b>" + date + "<br />");
            var message = new MailMessage();
            message.To.Add(emailto);

            ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>User Logon Notification</h1>", CheckLicense.SiteName + ": " + loggedinuser + " has Logged In", messagebody.ToString());
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
    protected void btn_passwordrecovery_Click(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_username_recovery.Text)) {
            MembershipUser msu = Membership.GetUser(tb_username_recovery.Text.Trim());
            if ((msu != null) && (!string.IsNullOrEmpty(msu.UserName))) {
                bool sendError = false;
                try {
                    MemberDatabase _mTemp = new MemberDatabase(msu.UserName);
                    if (((msu.IsLockedOut) || (!msu.IsApproved)) && (!_mTemp.IsNewMember)) {
                        sendError = true;
                    }
                    else {
                        string password = msu.ResetPassword();
                        string str = "<div><b>Username: </b>" + msu.UserName + "<br /><b>New Password: </b>" + password;
                        //str += "<br /><input type='button' onClick='CopiedTxt=document.selection.createRange();CopiedTxt.execCommand(\"" + password + "\");' value='Copy password to clipboard' />";
                        str += "<br /><p>Copy and paste the new password above in the login screen. Make sure to reset your password after logging in.</p>";
                        string link = "http:" + ServerSettings.GetSitePath(Request) + "/" + ServerSettings.DefaultStartupPage;

                        if (((msu.IsLockedOut) || (!msu.IsApproved)) && (_mTemp.IsNewMember))
                            link += "?ActivateUser=" + msu.UserName + "&ActivationCode=" + _mTemp.ActivationCode;

                        string fakelink = "http:" + ServerSettings.GetSitePath(Request) + "/" + ServerSettings.DefaultStartupPage;
                        str += "<p>Click on the link to continue: <a href='" + link + "'>" + fakelink + "</a></p>";
                        SendEmails(msu, str);

                        lbl_passwordResetMessage.Text = "<div class='clear-space'></div>Password has been sent to your email address.";
                        lbl_passwordResetMessage.ForeColor = System.Drawing.Color.Green;
                    }
                }
                catch {
                    sendError = true;
                }

                if (sendError) {
                    lbl_passwordResetMessage.Text = "<div class='clear-space'></div>Failed to send email. Please try again.";
                    lbl_passwordResetMessage.ForeColor = System.Drawing.Color.Red;

                    string str2 = "We were unable to recovery your account. It appears that your account has been locked and can only be released by an administrator.";
                    str2 += "<br />Contact your administrator to have your account unlocked. <b style='padding-right:3px'>Note:</b>To avoid issues like this in the ";
                    str2 += "future, please do not enter the wrong password multiple times. - " + CheckLicense.SiteName;
                    SendEmails(msu, str2);
                }
            }
        }

        tb_username_recovery.Text = string.Empty;
        updatepnl_forgotPassword.Update();
    }
    private void SendEmails(MembershipUser u, string newpassword) {
        var message = new MailMessage();
        message.To.Add(u.Email);

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Password Recovery</h1>", CheckLicense.SiteName + " : Password Recovery", newpassword);
    }

    protected void hf_LogoutOfGroup_ValueChanged(object sender, EventArgs e) {
        string groupName = string.Empty;
        if (ClearGroupSession(out groupName)) {
            Response.Redirect("~/AppRemote.aspx");
        }
    }

    private void WriteGroupSession(string username) {
        bool canTrySetSession = false;
        string requestGroup = string.Empty;
        if (!string.IsNullOrEmpty(username) && username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            if (Session["SocialGroupLogin"] != null) {
                requestGroup = Session["SocialGroupLogin"].ToString();
                canTrySetSession = true;
            }

            if (string.IsNullOrEmpty(requestGroup)) {
                if (requestGroup.Contains("#")) {
                    requestGroup = requestGroup.Replace(requestGroup.Substring(requestGroup.IndexOf("#")), "");
                }

                requestGroup = Request.QueryString["group"];
                canTrySetSession = true;
            }
        }

        if (canTrySetSession) {
            if (ServerSettings.CanLoginToGroup(username, requestGroup, HttpContext.Current)) {
                GroupSessions.AddOrSetNewGroupLoginSession(username, requestGroup);

                if (Session["SocialGroupLogin"] != null) {
                    Session.Remove("SocialGroupLogin");
                }
            }
            else {
                FormsAuthentication.SignOut();
            }
        }
    }
    private bool ClearGroupSession(out string groupName) {
        groupName = string.Empty;
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            groupName = GroupSessions.GetUserGroupSessionName(_username);
            GroupSessions.RemoveGroupLoginSession(_username);
            if (!string.IsNullOrEmpty(groupName)) {
                return true;
            }
        }

        return false;
    }

    protected void lbtn_signinwith_Google_Click(object sender, EventArgs e) {
        SetSocialGroupLoginSession();
        SocialSignIn.GoogleSignIn();
    }
    protected void lbtn_signinwith_Twitter_Click(object sender, EventArgs e) {
        SetSocialGroupLoginSession();
        SocialSignIn.TwitterSignIn();
    }
    protected void lbtn_signinwith_Facebook_Click(object sender, EventArgs e) {
        SetSocialGroupLoginSession();
        SocialSignIn.FacebookSignIn();
    }

    private void SetSocialGroupLoginSession() {
        string requestGroup = Request.QueryString["group"];
        if (!string.IsNullOrEmpty(requestGroup)) {
            if (requestGroup.Contains("#")) {
                requestGroup = requestGroup.Replace(requestGroup.Substring(requestGroup.IndexOf("#")), "");
            }

            Session["SocialGroupLogin"] = requestGroup;
        }
    }

    #endregion


    #region Site Plugins Load

    private void GetUserPlugins() {
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0 && _demoMemberDatabase.ContainsKey("PluginsToInstall")) {
            string[] pluginList = _demoMemberDatabase["PluginsToInstall"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string plId in pluginList) {
                AddPluginToPage(plId);
            }
        }
        else {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(_username);
                NewUserDefaults tempDefaults = new NewUserDefaults(sessionGroup);
                string pluginList = tempDefaults.GetDefault("PluginsToInstall");
                string[] groupPluginList = pluginList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string plId in groupPluginList) {
                    AddPluginToPage(plId);
                }
            }
            else {
                SitePlugins _plugins = new SitePlugins(_username);
                _plugins.BuildSitePluginsForUser();

                foreach (UserPlugins_Coll pl in _plugins.userplugins_dt) {
                    AddPluginToPage(pl.PluginID);
                }
            }
        }
    }
    private void GetPluginAssociations(string id) {
        SitePlugins _plugins = new SitePlugins(string.Empty);
        _plugins.BuildSitePlugins(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (!string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (coll.AssociatedWith == id)
                    AddPluginToPage(coll.ID);
            }
        }
    }
    private void AddPluginToPage(string id) {
        SitePlugins sp = new SitePlugins(string.Empty);
        SitePlugins_Coll plugin = sp.GetPlugin(id);

        if (_ss.AssociateWithGroups) {
            if (!ServerSettings.CheckPluginGroupAssociation(plugin, _member)) {
                return;
            }
        }

        if (!string.IsNullOrEmpty(plugin.ID)) {
            string loc = plugin.PluginLocation;
            bool canContinue = false;
            bool isJavascript = false;
            try {
                Uri uri = new Uri(loc);
                if (uri.IsAbsoluteUri) {
                    string x = uri.Segments[uri.Segments.Length - 1];
                    FileInfo fi = new FileInfo(x);
                    if (fi.Extension.ToLower() == ".js") {
                        canContinue = true;
                        isJavascript = true;
                    }
                    else if (fi.Extension.ToLower() == ".css") {
                        canContinue = true;
                        isJavascript = false;
                    }
                }
            }
            catch {
                try {
                    FileInfo fi = new FileInfo(loc);
                    if (fi.Extension.ToLower() == ".js") {
                        canContinue = true;
                        isJavascript = true;
                    }
                    else if (fi.Extension.ToLower() == ".css") {
                        canContinue = true;
                        isJavascript = false;
                    }
                }
                catch { }
            }

            if ((canContinue) && (plugin.Enabled)) {
                if (isJavascript) {
                    ScriptManager sm = ScriptManager.GetCurrent(Page);
                    if (sm != null) {
                        var sref = new ScriptReference(loc);
                        sm.Scripts.Add(sref);
                        string intiCode = HttpUtility.UrlDecode(plugin.InitCode);
                        intiCode = HttpUtility.UrlDecode(intiCode);
                        _strScriptreg.Append(intiCode);
                        sm.ScriptMode = ScriptMode.Release;
                    }
                }
                else {
                    StartupStyleSheets startupscripts = new StartupStyleSheets(false);
                    startupscripts.AddCssToPage(loc, Page);
                }

                if (string.IsNullOrEmpty(plugin.AssociatedWith))
                    GetPluginAssociations(plugin.ID);
            }
        }
    }

    #endregion

}