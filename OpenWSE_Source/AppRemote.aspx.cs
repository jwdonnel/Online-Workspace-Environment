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

public partial class AppRemote : System.Web.UI.Page {

    #region private variables

    private IPWatch _ipwatch = new IPWatch(false);
    private ServerSettings _ss = new ServerSettings();
    private readonly StringBuilder _strScriptreg = new StringBuilder();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly Notifications _notifi = new Notifications();
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
        else if (!IsPostBack) {
            GetSiteRequests.AddHitCount();

            this.Page.MetaDescription = _ss.MetaTagDescription;
            this.Page.MetaKeywords = _ss.MetaTagKeywords;

            if (!string.IsNullOrEmpty(ServerSettings.RobotsMetaTag)) {
                HtmlMeta meta = new HtmlMeta();
                meta.Name = "robots";
                meta.Content = ServerSettings.RobotsMetaTag;
                this.Page.Header.Controls.Add(meta);
            }
        }

        // Check to see if social sign in is valid
        SocialSignIn.CheckSocialSignIn();

        IIdentity userId = HttpContext.Current.User.Identity;
        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userId, IsPostBack, false);
        pageLoadInit.CheckSSLRedirect();

        lblHomePageLink.Enabled = false;
        lblHomePageLink.Visible = false;

        if (!pageLoadInit.CheckIfLicenseIsValid()) {
            Page.Response.Redirect("~/SiteTools/ServerMaintenance/LicenseManager.aspx");
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "appRemote_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';");

        if ((userId.IsAuthenticated) && (userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            if (pageLoadInit.CanLoadPage) {
                _ipAddress = pageLoadInit.IpAddress;
                StartUpPage(userId);
            }
            else
                Page.Response.Redirect("~/ErrorPages/Error.html");
        }
        else if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            Page.Response.Redirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
        }
        else {
            top_title_bar.Visible = true;
            lblHomePageLink.Text = "<a href='Workspace.aspx?" + ServerSettings.OverrideMobileSessionString.ToLower() + "=true' class='cursor-pointer'>Home</a>";

            string sitename = CheckLicense.SiteName;
            if (!string.IsNullOrEmpty(sitename)) {
                top_title_bar.InnerHtml = sitename + " Sign In";

                Page.Title = sitename + " Login Portal";
                lbl_login_help.InnerHtml = "Sign in using your " + sitename + " Username and Password";
            }
            else {
                top_title_bar.InnerHtml = "Sign In / Register";
            }

            if (!_ss.AllowUserSignUp) {
                _strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
                top_title_bar.InnerHtml = "Have an account? Sign in below";
            }

            if (!_ss.EmailSystemStatus) {
                _strScriptreg.Append("$('#lnk_forgotpassword').remove();");
            }

            GetStartupScripts_JS();
            GetStartupScripts_CSS();

            SetMainLogo();

            pnl_login.Enabled = true;
            pnl_login.Visible = true;
            pnl_icons.Enabled = false;
            pnl_icons.Visible = false;
            pnl_options.Enabled = false;
            pnl_options.Visible = false;
            lbl_UserName.Enabled = false;
            lbl_UserName.Visible = false;
            lb_signoff.Enabled = false;
            lb_signoff.Visible = false;
            lblHomePageLink.Enabled = true;
            lblHomePageLink.Visible = true;
            workspace_selector.Visible = false;

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

            _strScriptreg.Append("appRemote_Config.siteTheme='Standard'; $('#notifications').remove(); $('#notifications-viewtable').remove();");
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }
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
                _groupname = _member.GroupList;

                if ((_groupname.Count == 0) && (!_member.IsNewMember)) {
                    if (ServerSettings.AdminPagesCheck("GroupOrg", _username))
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
            _strScriptreg.Append("appRemote_Config.siteTheme = '" + _sitetheme + "';");
            _strScriptreg.Append("appRemote_Config.animationSpeed=" + _member.AnimationSpeed + ";");

            _workspaceMode = _member.WorkspaceMode.ToString();
            if (!MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                _strScriptreg.Append("appRemote_Config.workspaceMode='" + _workspaceMode + "';");
            }

            lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(_member.AccountImage, _member.UserColor, _member.UserId, HelperMethods.MergeFMLNames(_member), _sitetheme);

            GetStartupScripts_JS();
            GetStartupScripts_CSS();
            LoadAppIcons();
            AreAppIconLocked();
            SetNumberOfWorkspaces();

            _strScriptreg.Append("appRemote.CheckForNewNotifications();");

            if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
                RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
                _strScriptreg.Clear();
            }
        }
    }
    private void AreAppIconLocked() {
        bool appsLocked = true;
        if (!_member.LockAppIcons)
            appsLocked = false;

        if (!appsLocked) {
            _strScriptreg.Append("appRemote.AppsSortUnlocked(true);");
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
            str.Append("$('#db-s').hide();");
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
            apps_header_btn.Visible = false;
            pnl_chat_users.Enabled = false;
            pnl_chat_users.Visible = false;
            pnl_chat_popup.Enabled = false;
            pnl_chat_popup.Visible = false;
        }
    }

    private void SetMainLogo() {
        img_LoginGroup.ImageUrl = "~/Standard_Images/logo.png";

        #region OPACITY - COMMENTED OUT
        //string logoopacity = _ss.LogoOpacity;
        //if (!string.IsNullOrEmpty(logoopacity)) {
        //    double tempOut = 0.0d;
        //    if (double.TryParse(logoopacity, out tempOut)) {
        //        string ieFilter = "alpha(opacity=" + (tempOut * 100.0d).ToString() + ")";
        //        _strScriptreg.Append("$('#img_LoginGroup').css('opacity', '" + logoopacity + "');$('#img_LoginGroup').css('filter', '" + ieFilter + "');");
        //    }
        //}
        #endregion
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
                        cookie.Expires = DateTime.Now.AddDays(-1d);
                        cookieColl.Add(cookie);
                    }
                }

                foreach (object c in cookieColl.Keys) {
                    HttpCookie cookie = Request.Cookies[c.ToString()];
                    Response.Cookies.Add(cookie);
                }
            }
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            int seconds = DateTime.Now.Second;

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
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day - 1;
            if (DateTime.Now.Day <= 1) {
                month = DateTime.Now.Month - 1;
                if (month <= 1) {
                    year = DateTime.Now.Year - 1;
                    month = 1;
                }
                day = 28;
            }

            try {
                var newDate = new DateTime(DateTime.Now.Year, month, day, hour, min, seconds);
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

            _uuf.deleteFlag_User(_username);

            LoginActivity la = new LoginActivity();
            la.AddItem(_username, true, ActivityType.Logout);

            FormsAuthentication.SignOut();
            HttpContext.Current.Response.Redirect("~/AppRemote.aspx");
        }
    }


    #region Build Icon List

    private int _totalApps = 0;
    private void LoadAppIcons() {
        var appScript = new StringBuilder();
        _apps.GetUserInstalledApps();

        if (_groupname.Count == 0)
            pnl_icons.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big pad-right'>Must be associated with a group</h3>"));
        else {
            if (_member.GroupIcons)
                appScript.Append(IconsCategory);
            else {
                bool hideAllIcons = _ss.HideAllAppIcons;
                List<string> tempAppList = _apps.DeleteDuplicateEnabledApps(_member);

                bool AssociateWithGroups = _ss.AssociateWithGroups;

                foreach (var w in tempAppList) {
                    if (!_apps.IconExists(w)) continue;
                    Apps_Coll dt = _apps.GetAppInformation(w);

                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(dt, _member)) {
                            continue;
                        }
                    }

                    if ((_username.ToLower() != dt.CreatedBy.ToLower()) && (dt.IsPrivate)) {
                        continue;
                    }

                    string id = dt.AppId;
                    string name = dt.AppName;
                    string iconname = dt.Icon;
                    string category = dt.Category;
                    var fi = new FileInfo(dt.filename);
                    if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif")
                        && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {
                        var popup = new StringBuilder();
                        appScript.Append(Icons_NoCategory(popup, id, fi, iconname, category, dt, name, hideAllIcons));

                        _totalApps++;
                    }
                }
            }

            if (_totalApps > 0)
                pnl_icons.Controls.Add(new LiteralControl(appScript.ToString()));
            else
                pnl_icons.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big'>No Apps Installed</h3>"));
        }
    }
    private string IconsCategory {
        get {
            var appList = new StringBuilder();
            var appScript = new StringBuilder();
            var appCategory = new AppCategory(true);
            var categories = new List<string>();

            appList.Append("<div id='Category-Back' style='display:none'>");
            appList.Append("<img alt='back' src='App_Themes/" + _sitetheme + "/Icons/prevpage.png' />");
            appList.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text if needed
            appList.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
            appList.Append("<div id='Category-Back-Name-id' style='display:none'></div>");

            List<string> tempAppList = _apps.DeleteDuplicateEnabledApps(_member);
            bool hideAllIcons = _ss.HideAllAppIcons;
            bool AssociateWithGroups = _ss.AssociateWithGroups;

            foreach (string w in tempAppList) {
                if (!_apps.IconExists(w)) continue;
                Apps_Coll dt = _apps.GetAppInformation(w);

                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(dt, _member)) {
                        continue;
                    }
                }

                if ((_username.ToLower() != dt.CreatedBy.ToLower()) && (dt.IsPrivate)) {
                    continue;
                }

                string id = dt.AppId;
                string name = dt.AppName;
                string iconname = dt.Icon;
                string category = dt.Category;
                string[] categorySplit = category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                foreach (string c in categorySplit) {
                    string cId = c;
                    string categoryname = appCategory.GetCategoryName(cId);

                    if (string.IsNullOrEmpty(cId))
                        cId = categoryname;

                    var fi = new FileInfo(dt.filename);
                    if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif")
                        && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {
                        var popup = new StringBuilder();
                        if (!categories.Contains(categoryname)) {
                            appList.Append(BuildCategory(cId, categoryname));
                            categories.Add(categoryname);
                        }
                        appScript.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                        appScript.Append(Icons_NoCategory(popup, id, fi, iconname, cId, dt, name, hideAllIcons));
                        appScript.Append("</div>");
                    }
                }
                _totalApps++;
            }
            return appList + appScript.ToString();
        }
    }
    private string Icons_NoCategory(StringBuilder popup, string id, FileInfo fi, string iconname, string category, Apps_Coll dt, string w, bool hideAllIcons) {
        var appScript = new StringBuilder();
        var appCategory = new AppCategory(false);

        string tooltip = string.Empty;
        if (!string.IsNullOrEmpty(dt.Description))
            tooltip = " title='" + dt.Description.Replace("'", "") + "'";

        string iconImg = "<img alt='' src='Standard_Images/App_Icons/" + iconname + "' />";
        if ((hideAllIcons) || (_member.HideAppIcon))
            iconImg = string.Empty;

        if (fi.Extension.ToLower() == ".ascx") {
            appScript.Append("<div data-appid='" + id + "' class='app-icon' runat='server'" + tooltip + " onclick=\"appRemote.LoadOptions('" + id + "', '" + w + "', true)\">");
            appScript.Append(iconImg);
            appScript.Append("<span class='app-icon-font'>" + w + "</span></div>");
        }
        else {
            appScript.Append("<div data-appid='" + id + "' class='app-icon'" + tooltip + " onclick=\"appRemote.LoadOptions('" + id + "', '" + w + "', true)\">");
            appScript.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
        }

        return appScript.ToString();
    }
    private string BuildCategory(string id, string category) {
        var str = new StringBuilder();

        int categoryCount = 0;
        if (id == category)
            categoryCount = GetAppCount_Category("");
        else
            categoryCount = GetAppCount_Category(id);


        if (categoryCount > 0) {
            string count = string.Empty;
            if (_member.ShowCategoryCount)
                count = " (" + categoryCount + ")";

            str.Append("<div id='" + id + "' class='app-icon-category-list' runat='server' onclick=\"appRemote.CategoryClick('" + id + "', '" + category + "', true)\">");
            str.Append("<span class='app-icon-font'>" + category + count + "</span>");
            str.Append("<img alt='forward' src='App_Themes/" + _sitetheme + "/Icons/nextpage.png' /></div>");
        }
        return str.ToString();
    }
    private int GetAppCount_Category(string id) {
        List<string> userapps = _member.EnabledApps;
        return _apps.GetApps_byCategory(id).Cast<Apps_Coll>().Count(dr => userapps.Contains(dr.AppId));
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

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, true, ActivityType.Login);

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
        string f = "DBFull_" + DateTime.Now.ToFileTime();
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
        string date = DateTime.Now.ToString(CultureInfo.InvariantCulture);
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
                        string link = "http:" + ServerSettings.GetSitePath(Request) + "/Default.aspx";

                        if (((msu.IsLockedOut) || (!msu.IsApproved)) && (_mTemp.IsNewMember))
                            link += "?ActivateUser=" + msu.UserName + "&ActivationCode=" + _mTemp.ActivationCode;

                        string fakelink = "http:" + ServerSettings.GetSitePath(Request) + "/Default.aspx";
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

    protected void lbtn_signinwith_Google_Click(object sender, EventArgs e) {
        SocialSignIn.GoogleSignIn();
    }
    protected void lbtn_signinwith_Twitter_Click(object sender, EventArgs e) {
        SocialSignIn.TwitterSignIn();
    }
    protected void lbtn_signinwith_Facebook_Click(object sender, EventArgs e) {
        SocialSignIn.FacebookSignIn();
    }

    #endregion
}