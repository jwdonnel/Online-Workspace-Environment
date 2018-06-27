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
using OpenWSE_Tools.GroupOrganizer;
using System.Drawing;
using OpenWSE_Tools.Overlays;
using OpenWSE_Tools.AppServices;

public partial class AppRemote : BasePage {

    #region Private Variables

    private readonly StringBuilder PostBackScriptString = new StringBuilder();
    private AppIconBuilder _appIconBuilder;

    #endregion


    #region Properties

    protected AppIconBuilder CurrentAppIconBuilderObject {
        get {
            if (_appIconBuilder == null) {
                if (CurrentUserMemberDatabase != null) {
                    _appIconBuilder = new AppIconBuilder(Page, CurrentUserMemberDatabase);
                }
                else if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
                    _appIconBuilder = new AppIconBuilder(Page, DemoCustomizations.DefaultTable);
                }
            }

            return _appIconBuilder;
        }
    }

    #endregion


    #region Page Load Methods

    protected void Page_PreRender(object sender, EventArgs e) {
        Dictionary<string, string> demoDefaultTable = null;
        if (DemoCustomizations != null) {
            demoDefaultTable = DemoCustomizations.DefaultTable;
        }

        RegisterPostbackScripts.CallPostbackControl(Page, IsPostBack, CurrentUserMemberDatabase, demoDefaultTable);

        string controlName = Page.Request.Params["__EVENTTARGET"];
        if (controlName == "hf_aboutstatsapp" || controlName == "hf_loadOverlay1") {
            hf_noti_update_hiddenField.Value = "false";
        }

        ltl_footercopyright.Text = HelperMethods.GetCopyrightFooterText();
    }
    protected void Page_Init(object sender, EventArgs e) {
        ReinitializeOverlays();
    }
    protected void Page_Load(object sender, EventArgs e) {
        if (UserIsAuthenticated) {
            SetUpAuthenticatedUser();
        }
        else {
            LoadLoginPageControls();
        }

        LoadUserCustomizations();

        if (!string.IsNullOrEmpty(PostBackScriptString.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString());
        }
    }

    #endregion


    #region User Setup

    private void SetUpAuthenticatedUser() {
        if (BaseMaster.IsPageValid(Page, MainServerSettings.NoLoginRequired)) {
            if (!BasePage.IsUserNameEqualToAdmin(CurrentUsername)) {
                if (Session["SocialGroupLogin"] != null) {
                    string requestGroup = Session["SocialGroupLogin"].ToString();
                    Groups group = new Groups();
                    group.getEntries(requestGroup);
                    if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                        WriteGroupSession(CurrentUsername);
                    }
                }
            }

            if (!UserIsAuthenticated || CurrentUserMembership.IsLockedOut || !CurrentUserMembership.IsApproved) {
                HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
            }

            if (MainServerSettings.ForceGroupLogin && CurrentUserMemberDatabase.GroupList.Count == 1 && !GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                GroupSessions.AddOrSetNewGroupLoginSession(CurrentUsername, CurrentUserMemberDatabase.GroupList[0]);
            }

            if (CurrentUserMemberDatabase.GroupList.Count == 0 && !CurrentUserMemberDatabase.IsNewMember) {
                if (!ServerSettings.AdminPagesCheck("GroupOrg", CurrentUsername)) {
                    HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
                }
            }

            if (!IsUserNameEqualToAdmin()) {
                CurrentAppIconBuilderObject.BuildAppsForUser();
            }

            CountNotiMessages();
            SetupAdministratorUser();
            BuildAdminPages();
            SetNumberOfWorkspaces();
            LoadOverlays_Authenticated();
            CheckIfNeedToHideNotificationsButton();
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Error.html");
        }
    }
    private void SetupAdministratorUser() {
        if (IsUserNameEqualToAdmin()) {
            Apps_tab.Visible = false;
            Chat_tab.Visible = false;
            Groups_tab.Visible = false;
            Overlay_tab.Visible = false;
            connect_header_btn.Visible = false;
            workspace_header_btn.Visible = false;
            opened_apps_header.Visible = false;

            pnlContent_Apps.Visible = false;
            pnlContent_Chat.Visible = false;
            pnlContent_Groups.Visible = false;
            pnlContent_Overlay.Visible = false;
            pnlContent_ChatPopup.Visible = false;
            pnlContent_AppOptions.Visible = false;
        }
    }
    private void LoadUserCustomizations() {
        #region Declared Variables
        string animationSpeed = "150";
        bool siteTipsOnPageLoad = false;
        bool showToolTips = false;
        bool mobileAutoSync = false;
        bool isAdminMode = false;
        bool isDemoMode = false;
        #endregion

        if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
            #region Demo User
            animationSpeed = DemoCustomizations.DefaultTable["AnimationSpeed"];
            if (string.IsNullOrEmpty(animationSpeed)) {
                animationSpeed = "150";
            }

            if (HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["SiteTipsOnPageLoad"]) || string.IsNullOrEmpty(DemoCustomizations.DefaultTable["SiteTipsOnPageLoad"])) {
                siteTipsOnPageLoad = true;
            }

            showToolTips = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["ToolTips"]);
            mobileAutoSync = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["MobileAutoSync"]);

            isDemoMode = true;
            #endregion
        }
        else if (CurrentUserMemberDatabase != null) {
            #region Authenticated User
            animationSpeed = CurrentUserMemberDatabase.AnimationSpeed.ToString();
            siteTipsOnPageLoad = CurrentUserMemberDatabase.SiteTipsOnPageLoad;
            showToolTips = CurrentUserMemberDatabase.ShowToolTips;
            mobileAutoSync = CurrentUserMemberDatabase.MobileAutoSync;

            string acctImg = CurrentUserMemberDatabase.AccountImage;
            lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(CurrentUserMemberDatabase.AccountImage, CurrentUserMemberDatabase.UserColor, CurrentUserMemberDatabase.UserId, HelperMethods.MergeFMLNames(CurrentUserMemberDatabase), CurrentSiteTheme, CurrentUserMemberDatabase.ProfileLinkStyle);
            UserImageColorCreator.ApplyProfileLinkStyle(CurrentUserMemberDatabase.ProfileLinkStyle, CurrentUserMemberDatabase.UserColor, this.Page);
            lbl_UserFullName.Text = CurrentUserMemberDatabase.Username;
            lbl_UserEmail.Text = CurrentUserMembership.Email;
            if (!string.IsNullOrEmpty(acctImg)) {
                if (acctImg.ToLower().Contains("http") || acctImg.StartsWith("//") || acctImg.ToLower().Contains("www.")) {
                    img_Profile.ImageUrl = acctImg;
                }
                else {
                    if (File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + CurrentUserMemberDatabase.UserId + "/" + acctImg)) {
                        img_Profile.ImageUrl = ServerSettings.AccountImageLoc + CurrentUserMemberDatabase.UserId + "/" + acctImg;
                    }
                    else {
                        img_Profile.ImageUrl = ResolveUrl("~/App_Themes/" + CurrentUserMemberDatabase.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
                    }
                }
            }
            else {
                img_Profile.ImageUrl = ResolveUrl("~/App_Themes/" + CurrentUserMemberDatabase.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            }

            isAdminMode = BasePage.IsUserNameEqualToAdmin(CurrentUserMemberDatabase.Username);
            #endregion
        }

        if (!MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode) || !UserIsAuthenticated) {
            connect_header_btn.Visible = false;
        }

        GetUserPlugins();

        #region Register all Startup Scripts
        PostBackScriptString.Append("openWSE_Config.saveCookiesAsSessions= " + MainServerSettings.SaveCookiesAsSessions.ToString().ToLower() + ";");
        PostBackScriptString.Append("openWSE_Config.siteTheme= '" + CurrentSiteTheme + "';");
        PostBackScriptString.Append("openWSE_Config.animationSpeed=" + animationSpeed + ";");
        PostBackScriptString.Append("openWSE_Config.siteTipsOnPageLoad=" + siteTipsOnPageLoad.ToString().ToLower() + ";");
        PostBackScriptString.Append("openWSE_Config.showToolTips=" + showToolTips.ToString().ToLower() + ";");
        PostBackScriptString.Append("appRemote_Config.autoSync=" + mobileAutoSync.ToString().ToLower() + ";");
        PostBackScriptString.Append("appRemote_Config.isAdminMode=" + isAdminMode.ToString().ToLower() + ";");
        PostBackScriptString.Append("appRemote_Config.isDemoMode=" + isDemoMode.ToString().ToLower() + ";");
        PostBackScriptString.Append("appRemote_Config.forceGroupLogin=" + MainServerSettings.ForceGroupLogin.ToString().ToLower() + ";");
        PostBackScriptString.Append("openWSE_Config.workspaceMode='" + CurrentWorkspaceMode + "';");
        PostBackScriptString.Append("openWSE_Config.ShowLoginModalOnDemoMode=" + MainServerSettings.ShowLoginModalOnDemoMode.ToString().ToLower() + ";");
        #endregion
    }
    private void LoadLoginPageControls() {
        Login_tab.Visible = true;
        pnlContent_Login.Visible = true;
        user_profile_tab.Visible = false;

        if (DemoCustomizations == null || (!UserIsAuthenticated && !string.IsNullOrEmpty(Request.QueryString["group"]))) {
            remote_sidebar.Visible = false;
            sidebar_menu_toggle_btn.Visible = false;
            top_bar_toolview_holder.Style["margin-left"] = "0!important";
            remote_containerholder.Style["left"] = "0!important";
            remote_maincontainer.Style["left"] = "0!important";

            pnlContent_Apps.Visible = false;
            pnlContent_Chat.Visible = false;
            pnlContent_AdminLinks.Visible = false;
            pnlContent_Notifications.Visible = false;
            pnlContent_Layout.Visible = false;
            pnlContent_Overlay.Visible = false;
            pnlContent_ChatPopup.Visible = false;
            pnlContent_AppOptions.Visible = false;

            pnlContent_Login.Attributes["class"] += " active-panel";
            if (!string.IsNullOrEmpty(Request.QueryString["group"])) {
                hyp_groupLogin.Visible = false;
                hyp_cancelGroupLogin.Visible = true;
                iframe_title_logo.Attributes["class"] += " groupdemo-login-view";
            }
        }
        else {
            CurrentAppIconBuilderObject.BuildAppsForDemo();
            LoadOverlays_Demo();

            Chat_tab.Visible = false;
            AdminLinks_tab.Visible = false;
            Notifications_tab.Visible = false;

            pnlContent_Chat.Visible = false;
            pnlContent_AdminLinks.Visible = false;
            pnlContent_Notifications.Visible = false;
            pnlContent_ChatPopup.Visible = false;
            btn_addOverlayButton.Visible = false;
        }

        if (MainServerSettings.AllowUserSignUp) {
            ltl_LoginLabel.Text = "Login / Register";
        }
        else {
            ltl_LoginLabel.Text = "Login";
        }

        if (!MainServerSettings.AllowUserSignUp) {
            PostBackScriptString.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
        }

        if (!MainServerSettings.EmailSystemStatus) {
            PostBackScriptString.Append("$('#lnk_forgotpassword').remove();");
        }

        if (!MainServerSettings.SignInWithGoogle) {
            lbtn_signinwith_Google.Enabled = false;
            lbtn_signinwith_Google.Visible = false;
        }

        if (!MainServerSettings.SignInWithTwitter) {
            lbtn_signinwith_Twitter.Enabled = false;
            lbtn_signinwith_Twitter.Visible = false;
        }

        if (!MainServerSettings.SignInWithFacebook) {
            lbtn_signinwith_Facebook.Enabled = false;
            lbtn_signinwith_Facebook.Visible = false;
        }

        if (!string.IsNullOrEmpty(MainServerSettings.LoginMessage)) {
            lbl_LoginMessage.Enabled = true;
            lbl_LoginMessage.Visible = true;

            StringBuilder lm = new StringBuilder();
            lm.Append("<div class='Login-Message'>");
            lm.Append("<b>Site Message:</b>" + MainServerSettings.LoginMessage + "</div>");
            lbl_LoginMessage.Text = lm.ToString();
        }
        else {
            lbl_LoginMessage.Enabled = false;
            lbl_LoginMessage.Visible = false;
        }

        if (!MainServerSettings.SignInWithGoogle && !MainServerSettings.SignInWithTwitter && !MainServerSettings.SignInWithFacebook) {
            PostBackScriptString.Append("$('.loginwith-api-text').eq(1).remove();");
        }
    }
    private void SetNumberOfWorkspaces() {
        dropdownSelector.Items.Clear();
        ddl_appDropdownSelector.Items.Clear();

        int count = CurrentUserMemberDatabase.TotalWorkspaces;
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
    private void BuildAdminPages() {
        bool loadLinksToNewPage = CurrentUserMemberDatabase.LoadLinksBlankPage;
        SideBarItems sidebar = new SideBarItems(CurrentUsername);

        StringBuilder strTabs = new StringBuilder();
        if (CurrentUserMemberDatabase.ShowSiteToolsInCategories) {
            List<string> adminPageCategories = sidebar.ListOfAdminPageCategories();
            foreach (string keyVal in adminPageCategories) {
                string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, keyVal);
                if (!string.IsNullOrEmpty(innerTabList)) {
                    strTabs.AppendFormat("<div class=\"nav-title\">{0}</div><div class=\"site-tools-tablist\">", keyVal);
                    strTabs.AppendFormat("<div>{0}<div class='clear'></div></div>", innerTabList);
                    strTabs.Append("</div><div class=\"nav-divider\"></div>");
                }
            }
        }
        else {
            string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, string.Empty);
            if (!string.IsNullOrEmpty(innerTabList)) {
                string noCategoryName = "Settings and Tools";
                strTabs.AppendFormat("<div class=\"nav-title\">{0}</div><div class=\"site-tools-tablist\">", noCategoryName);
                strTabs.AppendFormat("<div>{0}<div class='clear'></div></div>", innerTabList);
                strTabs.Append("</div><div class=\"nav-divider\"></div>");
            }
        }

        placeHolder_SettingsTabs.Controls.Clear();
        placeHolder_SettingsTabs.Controls.Add(new LiteralControl(strTabs.ToString()));

        if (CurrentUserMemberDatabase.SiteToolsIconOnly) {
            sidebar_accordian.Attributes["data-icononly"] = "true";
        }

        if (!ServerSettings.AdminPagesCheck("acctsettings_aspx", CurrentUsername)) {
            hyp_accountSettings.Enabled = false;
            hyp_accountSettings.Visible = false;
        }
        else {
            hyp_accountSettings.NavigateUrl = "~/SiteTools/UserTools/AcctSettings.aspx?mobileMode=true&fromAppRemote=true#?tab=pnl_UserInformation";
            if (loadLinksToNewPage) {
                hyp_accountSettings.Target = "_blank";
            }
        }
    }

    #endregion


    #region Login/Logout

    protected void Login_LoggingIn(object sender, LoginCancelEventArgs e) {
        string email = Login1.UserName;
        MembershipUserCollection coll = Membership.GetAllUsers();

        int count = coll.Cast<MembershipUser>().Count(user => user.Email != null && user.Email.ToLower() == email);

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

        IPListener listener = new IPListener(false);
        IPWatch _ipwatch = new IPWatch(false);

        NameValueCollection n = Request.ServerVariables;
        string remoteaddress = n["REMOTE_ADDR"];
        if (remoteaddress == "::1") {
            remoteaddress = "127.0.0.1";
        }

        if (Login1.UserName.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            cancontinue = true;
        }

        else if ((_ipwatch.CheckIfBlocked(remoteaddress)) && (Login1.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
        else {
            if (!listener.TableEmpty) {
                if (listener.CheckIfActive(remoteaddress)) {
                    cancontinue = true;
                }
                else {
                    HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
                }
            }
            else {
                cancontinue = true;
            }
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

        HelperMethods.PageRedirect("~/AppRemote.aspx");
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
        IPWatch _ipwatch = new IPWatch(true);
        for (var i = 0; i < _ipwatch.ipwatchdt.Count; i++) {
            if (_ipwatch.ipwatchdt[i]["IPAddress"].ToString() == remoteaddress) {
                int attempts = Convert.ToInt32(_ipwatch.ipwatchdt[i]["Attempts"].ToString()) + 1;
                _ipwatch.updateAttempts(_ipwatch.ipwatchdt[i]["IPAddress"].ToString(), attempts);

                if (attempts >= MainServerSettings.AutoBlockIPCount) {
                    BackupSite();
                }
                foundIp = true;
            }
        }

        if (!foundIp) {
            _ipwatch.addItem(remoteaddress, 1, false);

            if (1 >= MainServerSettings.AutoBlockIPCount) {
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
        AutoBackupSystem.WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);

        if (File.Exists(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
        }

        ServerSettings.DeleteBackupTempFolderFiles();
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

            ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>User Logon Notification</h1>", ServerSettings.SiteName + ": " + loggedinuser + " has Logged In", messagebody.ToString());
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
                    else if (_mTemp.IsSocialAccount) {
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
                    str2 += "future, please do not enter the wrong password multiple times. - " + ServerSettings.SiteName;
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

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Password Recovery</h1>", ServerSettings.SiteName + " : Password Recovery", newpassword);
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
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            groupName = GroupSessions.GetUserGroupSessionName(CurrentUsername);
            GroupSessions.RemoveGroupLoginSession(CurrentUsername);
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

    protected void aGroupLogoff_Click(object sender, EventArgs e) {
        BaseMaster.GroupSignOff(CurrentUsername);
    }
    protected void lbtn_signoff_Click(object sender, EventArgs e) {
        BaseMaster.SignUserOff(CurrentUsername);
    }

    #endregion


    #region Jquery Plugins Load

    private void GetUserPlugins() {
        if (IsUserNameEqualToAdmin()) {
            return;
        }

        if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0 && DemoCustomizations.DefaultTable.ContainsKey("PluginsToInstall")) {
            string[] pluginList = DemoCustomizations.DefaultTable["PluginsToInstall"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string plId in pluginList) {
                AddPluginToPage(plId);
            }
        }
        else {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                NewUserDefaults tempDefaults = new NewUserDefaults(sessionGroup);
                string pluginList = tempDefaults.GetDefault("PluginsToInstall");
                string[] groupPluginList = pluginList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string plId in groupPluginList) {
                    AddPluginToPage(plId);
                }
            }
            else {
                SitePlugins _plugins = new SitePlugins(CurrentUsername);
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

        if (MainServerSettings.AssociateWithGroups) {
            if (!ServerSettings.CheckPluginGroupAssociation(plugin, CurrentUserMemberDatabase)) {
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
                        PostBackScriptString.Append(intiCode);
                        sm.ScriptMode = ScriptMode.Release;
                    }
                }
                else {
                    StartupStyleSheets startupscripts = new StartupStyleSheets(false);
                    startupscripts.AddCssToPage(loc, Page);
                }

                if (string.IsNullOrEmpty(plugin.AssociatedWith)) {
                    GetPluginAssociations(plugin.ID);
                }
            }
        }
    }

    #endregion


    #region Controls for Notifications

    private void CountNotiMessages() {
        int total = 0;
        UserNotificationMessages CurrentUserNotificationMessagesObject = new UserNotificationMessages(CurrentUsername);
        UserNotifications CurrentNotificationsObject = new UserNotifications();

        if (CurrentUserNotificationMessagesObject != null) {
            List<UserNotificationsMessage_Coll> messageList = CurrentUserNotificationMessagesObject.getNonDismissedEntries("DESC");
            total = messageList.Count;
            BaseMaster.SetNotificationPopup(Page, total, messageList, CurrentSiteTheme);
        }

        if (total == 0) {
            if (!IsPostBack && !string.IsNullOrEmpty(CurrentUsername)) {
                if (CurrentNotificationsObject.GetUserNotifications(CurrentUsername).Count == 0) {
                    Notifications_tab.Visible = false;
                    pnlContent_Notifications.Visible = false;
                }
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(Page, "openWSE.UpdateNotificationCount(" + total.ToString() + ");");
    }
    private void CheckUpdatesPopup() {
        ShowUpdatePopup sup = new ShowUpdatePopup();
        if (sup.isUserShowPopup(CurrentUsername)) {
            string message = sup.GetNewUpdateMessage(ServerSettings.GetServerMapLocation, CurrentSiteTheme);
            string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
            sup.UpdateUser(false, CurrentUsername);
            PostBackScriptString.Append("openWSE.ShowUpdatesPopup('" + encodedMessage + "');");
        }
    }
    private void CheckIfNeedToHideNotificationsButton() {
        if (CurrentUserMemberDatabase != null && !string.IsNullOrEmpty(CurrentUsername)) {
            UserNotificationMessages _usernoti = new UserNotificationMessages(CurrentUsername);
            if (_usernoti.getEntries("ASC").Count == 0) {
                UserNotifications tempNotifications = new UserNotifications();
                if (tempNotifications.GetUserNotifications(CurrentUsername).Count == 0) {
                    Notifications_tab.Visible = false;
                    pnlContent_Notifications.Visible = false;
                }
            }
        }
    }

    #endregion


    #region Auto Update

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        bool cancontinue = false;
        UserUpdateFlags CurrentUserUpdateFlagsObject = new UserUpdateFlags();

        if (CurrentUserMemberDatabase != null) {
            if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
                string id = CurrentUserUpdateFlagsObject.getFlag_AppID(hf_UpdateAll.Value);
                if (id == "workspace") {
                    CurrentUserUpdateFlagsObject.deleteFlag(hf_UpdateAll.Value);
                    cancontinue = true;
                }
                else if (hf_UpdateAll.Value == "refresh") {
                    HelperMethods.PageRedirect(Request.RawUrl);
                }
            }

            _appIconBuilder = null;
            CurrentAppIconBuilderObject.BuildAppsForUser();
        }

        if (cancontinue) {
            foreach (Control control in Page.Form.Controls) {
                if (control.HasControls()) {
                    RegisterAsynControls(control.Controls);
                }

                if (control is UpdatePanel) {
                    UpdatePanel up = control as UpdatePanel;
                    if (up.UpdateMode == UpdatePanelUpdateMode.Conditional) {
                        up.Update();
                    }
                    else {
                        if (control.HasControls()) {
                            RegisterAsynControls(control.Controls);
                        }
                    }
                }
            }
        }

        hf_UpdateAll.Value = "";
        CountNotiMessages();
    }
    private void RegisterAsynControls(ControlCollection page) {
        foreach (Control c in page) {
            if (c is UpdatePanel) {
                var up = c as UpdatePanel;
                if (up.UpdateMode == UpdatePanelUpdateMode.Conditional)
                    up.Update();
            }

            if (c.HasControls())
                RegisterAsynControls(c.Controls);
        }
    }

    #endregion


    #region Load Overlays

    private void ReinitializeOverlays() {
        if (!IsUserNameEqualToAdmin()) {
            // Hide the overlay button if logged into a group and no overlays exists
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                string tempUsername = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                WorkspaceOverlays wo = new WorkspaceOverlays();
                wo.GetUserOverlays(tempUsername);
                if (wo.UserOverlays.Count == 0) {
                    Overlay_tab.Visible = false;
                }

                PostBackScriptString.Append("openWSE_Config.groupLoginName=\"" + tempUsername + "\";");
            }
            else {
                string pnlId = WorkspaceOverlays.GetOverlayPanelId(Page, CurrentWorkspaceMode);
                if (CurrentAppIconBuilderObject != null && CurrentAppIconBuilderObject.InAtLeastOneGroup && !string.IsNullOrEmpty(pnlId)) {
                    Dictionary<string, string> demoDefaultTable = null;
                    if (DemoCustomizations != null) {
                        demoDefaultTable = DemoCustomizations.DefaultTable;
                    }

                    RegisterPostbackScripts.ReInitOverlaysAndApps(Page, IsPostBack, MainServerSettings.AssociateWithGroups, CurrentUserMemberDatabase, demoDefaultTable);
                }
            }
        }
    }
    private void LoadOverlays_Authenticated() {
        if (CurrentAppIconBuilderObject != null && !IsUserNameEqualToAdmin()) {
            if (CurrentAppIconBuilderObject.InAtLeastOneGroup) {
                string GetPageOverlayBtns = string.Empty;
                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, CurrentWorkspaceMode);

                if (!string.IsNullOrEmpty(pnlId)) {
                    if (CurrentUserMemberDatabase.HideAllOverlays) {
                        GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
                        Overlay_tab.Visible = false;
                    }
                }
                else if (string.IsNullOrEmpty(pnlId)) {
                    pnl_OverlaysAll.Enabled = false;
                    pnl_OverlaysAll.Visible = false;
                }

                if (!string.IsNullOrEmpty(pnlId)) {
                    PostBackScriptString.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.UpdateOverlayTable();" + GetPageOverlayBtns);
                    string tempUsername = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                    RefreshOverlays _ro = new RefreshOverlays(tempUsername, ServerSettings.GetServerMapLocation, this.Page, pnlId);
                    _ro.GetWorkspaceOverlays(CurrentAppIconBuilderObject.UserAppList);
                }
            }
        }
    }
    private void LoadOverlays_Demo() {
        if (CurrentAppIconBuilderObject != null && DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
            string GetPageOverlayBtns = string.Empty;
            string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, CurrentWorkspaceMode);

            if (!string.IsNullOrEmpty(pnlId)) {
                if (HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["HideAllOverlays"])) {
                    GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
                }
            }
            else if (string.IsNullOrEmpty(pnlId)) {
                pnl_OverlaysAll.Enabled = false;
                pnl_OverlaysAll.Visible = false;
            }

            if (!string.IsNullOrEmpty(pnlId)) {
                PostBackScriptString.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.UpdateOverlayTable();" + GetPageOverlayBtns);
                RefreshOverlays _ro = new RefreshOverlays(string.Empty, ServerSettings.GetServerMapLocation, this.Page, pnlId);
                _ro.GetWorkspaceOverlays_NoLogin(CurrentAppIconBuilderObject.UserAppList);
            }
        }
    }

    #endregion

}