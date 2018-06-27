using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using OpenWSE_Tools.Apps;
using SocialSignInApis;
using OpenWSE_Tools.AppServices;

public partial class Apps_RSSFeed_RSSFeed : System.Web.UI.Page {

    #region Private Variables

    private const string app_id = "app-rssfeed";
    private readonly ServerSettings MainServerSettings = new ServerSettings();

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        AppInitializer appInitializer = new AppInitializer(app_id, userId.Name, Page);

        string animationSpeed = "150";
        string theme = "Standard";
        bool toolTips = true;

        if (!userId.IsAuthenticated) {
            NewUserDefaults _demoCustomizations = new NewUserDefaults("DemoNoLogin");
            _demoCustomizations.GetDefaults();

            if (_demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0) {
                animationSpeed = _demoCustomizations.DefaultTable["AnimationSpeed"];
                theme = _demoCustomizations.DefaultTable["Theme"];
                toolTips = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["ToolTips"]);
            }

            CheckIfNeedLogin();
        }
        else {
            if (BasePage.IsUserNameEqualToAdmin(userId.Name)) {
                HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
            }

            animationSpeed = appInitializer.memberDatabase.AnimationSpeed.ToString();
            theme = appInitializer.memberDatabase.SiteTheme;
            toolTips = appInitializer.memberDatabase.ShowToolTips;

            btn_LoginRegister.Visible = false;
            btn_loginRegister_sidebar.Visible = false;
            pnl_LoginRegister.Visible = false;
            lbtn_signoff.Visible = true;
            lbtn_signoff_sidebar.Visible = true;
        }

        StringBuilder strJs = new StringBuilder();
        strJs.Append("try { openWSE_Config.animationSpeed=" + animationSpeed + ";");
        strJs.Append("openWSE_Config.siteTheme='" + theme + "';");
        strJs.Append("openWSE_Config.siteRootFolder='" + Page.ResolveUrl("~/").Replace("/", "") + "';");
        strJs.Append("openWSE_Config.showToolTips=" + toolTips.ToString().ToLower() + ";");
        strJs.Append("openWSE_Config.appendTimestampOnScripts=" + new ServerSettings().AppendTimestampOnScripts.ToString().ToLower() + ";");
        strJs.Append("openWSE_Config.timestampQuery='" + ServerSettings.TimestampQuery + "';");
        strJs.Append("openWSE_Config.saveCookiesAsSessions=" + MainServerSettings.SaveCookiesAsSessions.ToString().ToLower() + ";");
        strJs.Append("$(document).tooltip({ disabled: " + (!toolTips).ToString().ToLower() + " });");

        strJs.Append("openWSE_Config.siteName='" + ServerSettings.SiteName + "'; }catch (evt) { }");
        RegisterPostbackScripts.RegisterStartupScript(Page, strJs.ToString());

        if (!IsPostBack) {
            if (userId.IsAuthenticated && appInitializer.memberDatabase != null) {
                hf_appviewstyle.Value = appInitializer.memberDatabase.UserAppStyle.ToString();
                pnl_EditFeeds.Visible = true;

                Apps_Coll appInfo = new App(string.Empty).GetAppInformation(app_id);
                if (appInfo.AllowNotifications) {
                    pnl_EditFeedAlerts.Visible = true;
                    myAlertsLink.Visible = true;
                }

                myFeedsLink.Visible = true;
                mySavedLink.Visible = true;

                if (BasePage.IsUserInAdminRole(userId.Name)) {
                    pnl_AdminRSSFeedSettings.Enabled = true;
                    pnl_AdminRSSFeedSettings.Visible = true;
                }
            }

            Header.Controls.Add(new LiteralControl("<link rel=\"Stylesheet\" href=\"" + ResolveUrl("~/App_Themes/" + theme + "/StyleSheets/Main/modals.css") + "\" />"));
            appInitializer.LoadScripts_JS(false);
            appInitializer.LoadCustomFonts();
        }
    }
    private void CheckIfNeedLogin() {
        #region Check LoginMessage
        if (!string.IsNullOrEmpty(MainServerSettings.LoginMessage)) {
            lbl_LoginMessage_Master.Enabled = true;
            lbl_LoginMessage_Master.Visible = true;
            lbl_LoginMessage_Master.Text = string.Format("<div class='Login-Message'><b>Site Message:</b>{0}</div>", MainServerSettings.LoginMessage);
        }
        else {
            lbl_LoginMessage_Master.Enabled = false;
            lbl_LoginMessage_Master.Visible = false;
        }
        #endregion

        #region Check Sign in with Google
        if (!MainServerSettings.SignInWithGoogle) {
            lbtn_signinwith_Google.Enabled = false;
            lbtn_signinwith_Google.Visible = false;
        }
        #endregion

        #region Check Sign in with Twitter
        if (!MainServerSettings.SignInWithTwitter) {
            lbtn_signinwith_Twitter.Enabled = false;
            lbtn_signinwith_Twitter.Visible = false;
        }
        #endregion

        #region Check Sign in with Facebook
        if (!MainServerSettings.SignInWithFacebook) {
            lbtn_signinwith_Facebook.Enabled = false;
            lbtn_signinwith_Facebook.Visible = false;
        }
        #endregion

        StringBuilder strScriptreg = new StringBuilder();
        if (MainServerSettings.SignInWithGoogle || MainServerSettings.SignInWithTwitter || MainServerSettings.SignInWithFacebook) {
            strScriptreg.Append("$('#SocialLogin_borderSep').addClass('loginwith-api-borderseperate');");
        }
        else {
            sociallogin_td.Visible = false;
            Login1.Style["width"] = "270px";
            Login1.Style["margin"] = "0 auto";
        }

        if (!MainServerSettings.AllowUserSignUp) {
            strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
        }
        else {
            btn_LoginRegister.InnerHtml = "Login / Register";
        }

        if (!MainServerSettings.EmailSystemStatus) {
            strScriptreg.Append("$('#lnk_forgotpassword').remove();");
        }

        if (!string.IsNullOrEmpty(strScriptreg.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(Page, strScriptreg.ToString());
        }
    }


    protected void lbtn_signoff_Click(object sender, EventArgs e) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            BaseMaster.SignUserOff(HttpContext.Current.User.Identity.Name);
        }
    }


    #region Login

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
    }
    protected void Login_Loggedin(object sender, EventArgs e) {
        bool cancontinue = false;
        IPListener listener = new IPListener(false);
        IPWatch _ipwatch = new IPWatch(false);
        string remoteaddress = BasePage.GetCurrentPageIpAddress(Request);

        if (BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
            cancontinue = true;
        }
        else if (_ipwatch.CheckIfBlocked(remoteaddress) && !BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
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

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, true, ActivityType.Login);

        if (!string.IsNullOrEmpty(member.DefaultLoginGroup) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(member.Username)) {
            GroupSessions.AddOrSetNewGroupLoginSession(member.Username, member.DefaultLoginGroup);
        }

        MemberDatabase.AddUserSessionIds(member.Username);
        ServerSettings.SetRememberMeOnLogin(Login1, Response);

        if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"])) {
            string redirectUrl = HttpUtility.UrlDecode(Request.QueryString["ReturnUrl"]);
            if (BasePage.IsUserInAdminRole(member.Username)) {
                HelperMethods.PageRedirect(redirectUrl);
            }
            else {
                foreach (string page in member.AdminPagesList) {
                    if (redirectUrl.ToLower().Contains(page)) {
                        HelperMethods.PageRedirect(redirectUrl);
                    }
                }
            }
        }

        HelperMethods.PageRedirect("~/Apps/RSSFeed/RSSFeed.aspx");
    }
    protected void Login_error(object sender, EventArgs e) {
        string remoteaddress = BasePage.GetCurrentPageIpAddress(Request);

        if (BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
            MembershipUser mu = Membership.GetUser(ServerSettings.AdminUserName);
            if (mu != null && (mu.IsLockedOut || !mu.IsApproved)) {
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
            if (!listener.CheckIfActive(remoteaddress)) {
                cancontinue = true;
            }
        }
        else {
            cancontinue = true;
        }

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

        HelperMethods.PageRedirect("~/Apps/RSSFeed/RSSFeed.aspx");
    }

    protected void lbtn_signinwith_Google_Click(object sender, EventArgs e) {
        Session["RSSFeedLogin"] = "true";
        SocialSignIn.GoogleSignIn();
    }
    protected void lbtn_signinwith_Twitter_Click(object sender, EventArgs e) {
        Session["RSSFeedLogin"] = "true";
        SocialSignIn.TwitterSignIn();
    }
    protected void lbtn_signinwith_Facebook_Click(object sender, EventArgs e) {
        Session["RSSFeedLogin"] = "true";
        SocialSignIn.FacebookSignIn();
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
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt),
                      ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
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

    #endregion


}