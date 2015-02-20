#region

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Data;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.GroupOrganizer;
using SocialSignInApis;
using OpenWSE.Core.Licensing;
using System.Web.UI.HtmlControls;

#endregion

public partial class Default : Page {
    #region Private Variables

    private readonly Notifications _notifications = new Notifications();
    private IPWatch _ipwatch = new IPWatch(false);
    private ServerSettings _ss = new ServerSettings();
    private bool _noLoginRequired;
    private bool _Activation = false;
    private string _ActivationUser = "";

    #endregion


    #region Page Load and Page Checks

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile()) {
            return;
        }

        if (!IsPostBack) {
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

        _noLoginRequired = _ss.NoLoginRequired;
        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userId, IsPostBack, _noLoginRequired);
        pageLoadInit.CheckSSLRedirect();

        if (CheckLicense.TrialActivated && CheckLicense.LicenseValid) {
            pnl_purchaseFullVer.Visible = true;
            RegisterPostbackScripts.RegisterStartupScript(this, "SetTrialText('" + CheckLicense.DaysLeftBeforeExpired + "');");
        }

        bool groupFound = false;
        string requestGroup = Request.QueryString["group"];
        if (!string.IsNullOrEmpty(requestGroup)) {
            Groups group = new Groups();
            group.getEntries(requestGroup);
            if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                Dictionary<string, string> g = group.group_dt[0];
                SetGroupScreen(g);
                groupFound = true;
            }
            else {
                SetScreen();
            }
        }
        else {
            SetScreen();
        }

        var listener = new IPListener(false);
        bool siteOffline = _ss.SiteOffLine;

        CheckLicense.ValidateLicense();
        CheckIfMobileOverride();

        if (!CheckLicense.LicenseValid) {
            DisableCreateAndPreview();
        }
        else {
            MembershipUser isUser = Membership.GetUser(userId.Name);
            if ((userId.IsAuthenticated) && (!siteOffline) && (isUser != null)) {
                Unsubscribe(userId.Name);

                if (_ipwatch.CheckIfBlocked(pageLoadInit.IpAddress)) {
                    DisableCreateAndPreview();
                }
                else {
                    MemberDatabase _member = new MemberDatabase(userId.Name);
                    if (!listener.TableEmpty) {
                        if (listener.CheckIfActive(pageLoadInit.IpAddress)) {
                            ChoosePage(userId.Name);
                        }
                        else {
                            DisableCreateAndPreview();
                        }
                    }
                    else {
                        ChoosePage(userId.Name);
                    }
                }
            }
            else {
                if (_ipwatch.CheckIfBlocked(pageLoadInit.IpAddress)) {
                    DisableCreateAndPreview();
                }
                else {
                    bool canContinue = true;
                    if (!listener.TableEmpty) {
                        if (!listener.CheckIfActive(pageLoadInit.IpAddress)) {
                            DisableCreateAndPreview();
                            canContinue = false;
                        }
                    }

                    if (canContinue) {
                        if (_ss.SignUpConfirmationEmail) {
                            RegisterUser.CompleteSuccessText = "Your account has been successfully created. Please check your email to activate your account. If you do not recieve an email to activate your account, please click on the Forgot Password to left.";
                        }

                        if ((_noLoginRequired) && (!siteOffline) && (!groupFound)) {
                            WriteGroupSession(_ActivationUser);
                            if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["noredirect"]) || string.IsNullOrEmpty(Request.QueryString["noredirect"])) {
                                if (_Activation) {
                                    Page.Response.Redirect("~/Workspace.aspx?activation=true&user=" + _ActivationUser);
                                }
                                else {
                                    Page.Response.Redirect("~/Workspace.aspx");
                                }
                            }
                            else {
                                DisableCreateAndPreview();
                                pnl_FooterPasswordRec.Visible = false;
                            }
                        }
                        else if (siteOffline) {
                            DisableCreateAndPreview();
                        }
                    }
                }
            }
        }
    }
    private void Unsubscribe(string username) {
        string id = Request.QueryString["UnregisterEmails"];
        if (!string.IsNullOrEmpty(id)) {
            _notifications.GetUserNotifications(username);
            foreach (UserNotifications_Coll coll in _notifications.UserNotifications) {
                if ((coll.NotificationID.ToLower() == id.ToLower()) && (coll.Email)) {
                    _notifications.UpdateUserNotificationEmail(id, false, username);
                    break;
                }
            }
        }
    }
    private void ChoosePage(string username) {
        MemberDatabase _customization = new MemberDatabase(username);
        MembershipUser _membershipuser = Membership.GetUser(username);
        if (_membershipuser != null) {
            if ((!_membershipuser.IsLockedOut) && (_membershipuser.IsApproved)) {
                WriteGroupSession(username);
                if (!string.IsNullOrEmpty(Request["redirect"])) {
                    Page.Response.Redirect("~/" + Request["redirect"]);
                }
                else {
                    if (_Activation) {
                        Page.Response.Redirect("~/Workspace.aspx?activation=true&user=" + _ActivationUser);
                    }
                    else {
                        Page.Response.Redirect("~/Workspace.aspx");
                    }
                }
            }
        }
    }
    protected void Page_PreInit(object sender, EventArgs e) {
        string theme = _ss.LoginScreenTheme;
        Page.Theme = theme;
    }
    private void SetScreen() {
        DeleteGroupSession();
        string sitename = CheckLicense.SiteName;
        string theme = _ss.LoginScreenTheme;

        Page.Title = sitename + " Login Portal";
        RegisterPostbackScripts.RegisterStartupScript(this, "siteName='" + sitename + "';");

        lbl_login_help.InnerHtml = "Sign in using your " + sitename + " Username and Password";

        if (!string.IsNullOrEmpty(_ss.LoginMessage)) {
            lbl_LoginMessage.Enabled = true;
            lbl_LoginMessage.Visible = true;

            StringBuilder lm = new StringBuilder();
            lm.Append("<div class='Login-Message'>");
            lm.Append("<b>Site Message:</b>" + _ss.LoginMessage + "</div>");
            lbl_LoginMessage.Text = lm.ToString();
        }
        else {
            lbl_LoginMessage.Enabled = false;
            lbl_LoginMessage.Visible = false;
        }

        bool emailStatus = _ss.EmailSystemStatus;
        if (!_ss.AllowUserSignUp) {
            pnl_FooterRegister.Visible = false;
            registerdiv.Visible = false;
        }

        if (!emailStatus) {
            pnl_FooterPasswordRec.Visible = false;
            passwordrecoverydiv.Visible = false;
        }

        if (!_ss.ShowPreviewButtonLogin) {
            pnl_preview.Visible = false;
        }

        if (_ss.UserSignUpEmailChecker) {
            lbl_email_assocation.Text = "Must have a valid <b>@" + _ss.UserSignUpEmailAssociation + "</b> email address to activate account";
        }

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

        if (_ss.SignInWithGoogle || _ss.SignInWithTwitter || _ss.SignInWithFacebook) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#SocialLogin_borderSep').addClass('loginwith-api-borderseperate');");
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('.loginwith-api-text').remove();");
        }

        if ((!string.IsNullOrEmpty(Request.QueryString["ActivateUser"])) && (!string.IsNullOrEmpty(Request.QueryString["ActivationCode"]))) {
            string _u = Request.QueryString["ActivateUser"];
            string _code = Request.QueryString["ActivationCode"];
            MembershipUser member = Membership.GetUser(_u);
            MemberDatabase memData = new MemberDatabase(_u);
            if (member != null) {
                if (memData.ActivationCode == _code) {
                    member.UnlockUser();
                    if (!member.IsApproved) {
                        member.IsApproved = true;
                        string script = "$('#account_active').html('<b style=\"color:#006600\">Your account is now active. You may sign in now.</b><div class=\"clear-space\"></div>');";
                        RegisterPostbackScripts.RegisterStartupScript(this, script);
                    }

                    Membership.UpdateUser(member);

                    TextBox tb_login_username = (TextBox)Login1.FindControl("UserName");
                    if (tb_login_username != null) {
                        tb_login_username.Text = member.UserName;
                        Login1.UserName = member.UserName;
                    }

                    _Activation = true;
                    _ActivationUser = _u;
                }
                else {
                    string script = "$('#account_active').html('<b style=\"color:#FF0000\">Activation code does not match. Please try again.</b>');";
                    RegisterPostbackScripts.RegisterStartupScript(this, script);
                }
            }
        }
    }
    private void SetGroupScreen(Dictionary<string, string> group) {
        string sitename = CheckLicense.SiteName;
        string theme = _ss.LoginScreenTheme;

        if (HelperMethods.ConvertBitToBoolean(group["IsURL"])) {
            string imgUrl = group["Image"];
            if (imgUrl.StartsWith("~/")) {
                imgUrl = ResolveUrl(imgUrl);
            }
            mainLoginLogo.Src = imgUrl;
        }
        else if (!string.IsNullOrEmpty(group["Image"])) {
            mainLoginLogo.Src = ResolveUrl("~/Standard_Images/Groups/Logo/" + group["Image"]);
        }

        Page.Title = sitename + " Login Portal";
        lbl_login_help.InnerHtml = "Sign in using your " + sitename + "/" + group["GroupName"] + " Username/E-mail and Password";

        if (!string.IsNullOrEmpty(_ss.LoginMessage)) {
            lbl_LoginMessage.Enabled = true;
            lbl_LoginMessage.Visible = true;

            StringBuilder lm = new StringBuilder();
            lm.Append("<div class='Login-Message'>");
            lm.Append("<b>Site Message:</b>" + _ss.LoginMessage + "</div>");
            lbl_LoginMessage.Text = lm.ToString();
        }
        else {
            lbl_LoginMessage.Enabled = false;
            lbl_LoginMessage.Visible = false;
        }

        if (!_ss.EmailSystemStatus) {
            pnl_FooterPasswordRec.Visible = false;
            passwordrecoverydiv.Visible = false;
        }

        lblMainLoginLink.Text = " | <a href='Default.aspx'>Site Login Page</a>";

        pnl_FooterRegister.Visible = false;
        registerdiv.Visible = false;
        pnl_preview.Visible = false;

        if (_ss.UserSignUpEmailChecker) {
            lbl_email_assocation.Text = "Must have a valid <b>@" + _ss.UserSignUpEmailAssociation + "</b> email address to activate account";
        }

        if ((!string.IsNullOrEmpty(Request.QueryString["ActivateUser"])) && (!string.IsNullOrEmpty(Request.QueryString["ActivationCode"]))) {
            string _u = Request.QueryString["ActivateUser"];
            string _code = Request.QueryString["ActivationCode"];
            MembershipUser member = Membership.GetUser(_u);
            MemberDatabase memData = new MemberDatabase(_u);
            if (member != null) {
                if (memData.ActivationCode == _code) {
                    member.UnlockUser();
                    if (!member.IsApproved) {
                        member.IsApproved = true;
                        string script = "$('#account_active').html('<b style=\"color:#60F221\">Your account is now active. You may sign in now.</b>');";
                        RegisterPostbackScripts.RegisterStartupScript(this, script);
                    }

                    Membership.UpdateUser(member);

                    TextBox tb_login_username = (TextBox)Login1.FindControl("UserName");
                    if (tb_login_username != null) {
                        tb_login_username.Text = member.UserName;
                        Login1.UserName = member.UserName;
                    }

                    _Activation = true;
                    _ActivationUser = _u;
                }
                else {
                    string script = "$('#account_active').html('<b style=\"color:#FF0000\">Activation code does not match. Please try again.</b>');";
                    RegisterPostbackScripts.RegisterStartupScript(this, script);
                }
            }
        }
    }
    private void DisableCreateAndPreview() {
        RegisterPostbackScripts.RegisterStartupScript(this, "$('.loginwith-api-text').remove();$('#SocialLogin_borderSep').removeClass('loginwith-api-borderseperate');");

        lbtn_signinwith_Google.Enabled = false;
        lbtn_signinwith_Google.Visible = false;
        lbtn_signinwith_Twitter.Enabled = false;
        lbtn_signinwith_Twitter.Visible = false;
        lbtn_signinwith_Facebook.Enabled = false;
        lbtn_signinwith_Facebook.Visible = false;

        pnl_FooterRegister.Visible = false;
        registerdiv.Visible = false;
        pnl_preview.Visible = false;
    }

    private void WriteGroupSession(string username) {
        string requestGroup = Request.QueryString["group"];
        if ((!string.IsNullOrEmpty(username)) && (!string.IsNullOrEmpty(requestGroup))) {
            if (ServerSettings.CanLoginToGroup(username, requestGroup, HttpContext.Current)) {
                Session["LoginGroup"] = requestGroup;
            }
        }
    }
    private void DeleteGroupSession() {
        if (Session["LoginGroup"] != null) {
            Session.Remove("LoginGroup");
        }
    }

    private void CheckIfMobileOverride() {
        if (HelperMethods.IsMobileDevice) {
            if (Session[ServerSettings.OverrideMobileSessionString] == null || (Session[ServerSettings.OverrideMobileSessionString] != null && !HelperMethods.ConvertBitToBoolean(Session[ServerSettings.OverrideMobileSessionString].ToString()))) {
                Response.Redirect("~/AppRemote.aspx");
            }
        }
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

        if (!ServerSettings.CanLoginToGroup(Login1.UserName, Request.QueryString["group"], HttpContext.Current)) {
            ltl_logingrouperror.Text = "<div style='color: #D80000;'>Not authorized for group. Please try again</div>";
            e.Cancel = true;
        }
    }
    protected void Login_Loggedin(object sender, EventArgs e) {
        NameValueCollection n = Request.ServerVariables;
        string remoteaddress = n["REMOTE_ADDR"];
        if (remoteaddress == "::1")
            remoteaddress = "127.0.0.1";

        if (CheckLicense.LicenseValid) {
            bool cancontinue = false;
            var listener = new IPListener(false);

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
            Unsubscribe(Login1.UserName);
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

            ChoosePage(Login1.UserName);
        }
        else {
            if (Roles.IsUserInRole(Login1.UserName, ServerSettings.AdminUserName)) {

                LoginActivity la = new LoginActivity();
                la.AddItem(Login1.UserName, true, ActivityType.Login);

                HttpContext.Current.Response.Redirect("~/SiteTools/ServerMaintenance/LicenseManager.aspx");
            }
            else {

                LoginActivity la = new LoginActivity();
                la.AddItem(Login1.UserName, false, ActivityType.Login);

                HttpContext.Current.Response.Redirect("~/ErrorPages/LicenseInvalid.html");
            }
        }
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
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt),
                      ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
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


    #region password recovery

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
                    }
                }
                catch {
                    sendError = true;
                }

                if (sendError) {
                    string str2 = "We were unable to recovery your account. It appears that your account has been locked and can only be released by an administrator.";
                    str2 += "<br />Contact your administrator to have your account unlocked. <b style='padding-right:3px'>Note:</b>To avoid issues like this in the ";
                    str2 += "future, please do not enter the wrong password multiple times. - " + CheckLicense.SiteName;
                    SendEmails(msu, str2);
                }
            }

            string script = "QikPages.goBack();$('#tb_username_recovery').val('');";
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }
    }
    private void SendEmails(MembershipUser u, string newpassword) {
        var message = new MailMessage();
        message.To.Add(u.Email);

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Password Recovery</h1>", CheckLicense.SiteName + " : Password Recovery", newpassword);
    }

    #endregion


    #region Email User

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

    #endregion


    #region Register New User

    protected void RegisterUser_CreatedUser(object sender, EventArgs e) {
        lbl_email_assocation.Visible = false;

        string username = RegisterUser.UserName;
        string email = RegisterUser.Email;
        var tb_firstnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_firstnamereg");
        var tb_lastnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_lastnamereg");
        var color1 = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("color1");
        string role = _ss.UserSignUpRole;

        UserRegistration ur = new UserRegistration(username, role);
        ur.RegisterNewUser(tb_firstnamereg.Text, tb_lastnamereg.Text, email, color1.Text);
        ur.RegisterDefaults();

        bool signUpConfirmEmail = _ss.SignUpConfirmationEmail;

        MembershipUser newuser = Membership.GetUser(username);
        if (newuser != null) {
            if (signUpConfirmEmail) {
                newuser.IsApproved = false;
            }
            else {
                newuser.IsApproved = true;
            }
            Membership.UpdateUser(newuser);
        }

        MemberDatabase member = new MemberDatabase(username);
        string code = Guid.NewGuid().ToString();

        member.UpdateActivationCode(code);

        if (signUpConfirmEmail) {
            string url = ServerSettings.GetSitePath(Request);
            if (url.LastIndexOf('/') != url.Length - 1)
                url += "/";

            string logo = "<img alt='logo' src='" + url + "Standard_Images/logo.png' style='padding-right:5px;max-height:60px!important;height:60px!important;float:left!important' />";
            logo += "<h1 style='color:#555'>Registration Confirmation</h1><div style='clear:both;height:20px'></div>";

            string str = "<p><h2 style='color:#353535'>Thank you for registering an account with " + CheckLicense.SiteName + ". Please click on the link below to activate your account.</h2></p>";
            string link = "http:" + url + "Default.aspx?ActivateUser=" + username + "&ActivationCode=" + code;
            string fakelink = "http:" + url + "Default.aspx";
            str += "<p>Click on the link to activate account: <a href='" + link + "'>" + fakelink + "</a></p>";
            SendEmail_ActivateUser(newuser, str);
        }

        if (_ss.EmailOnRegister) {
            MembershipUser adminUser = Membership.GetUser(ServerSettings.AdminUserName.ToLower());
            if (!string.IsNullOrEmpty(adminUser.Email)) {
                var message = new MailMessage();
                message.To.Add(adminUser.Email);
                StringBuilder newUserText = new StringBuilder();
                newUserText.Append("The user " + newuser.UserName + " has been created. Username created by " + tb_firstnamereg.Text + " " + tb_lastnamereg.Text + ".");
                newUserText.Append("<br /><b>User email:</b> " + email + "<br /><b>Date/Time Created:</b> " + DateTime.Now.ToString());
                newUserText.Append("<br /><b><span style='float:left;'>User Color: </span></b>");
                newUserText.Append("<div style='float:left;height:12px;width:12px;background:#" + color1.Text + ";margin-left:5px;margin-top:2px;-webkit-border-radius:50%;-moz-border-radius:50%;border-radius:50%;' title='" + color1.Text + "'></div>");
                newUserText.Append("<br /><b>User Role:</b> " + role);
                ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>New User Created</h1>", CheckLicense.SiteName + " : New Account Created", newUserText.ToString());
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "QikPages.registerFinish();");
    }
    protected void RegisterUser_CreatingUser(object sender, LoginCancelEventArgs e) {
        string script = "register();";
        if (_ss.AllowUserSignUp) {
            bool canContinue = true;
            if (_ss.UserSignUpEmailChecker) {
                string emailAssociation = _ss.UserSignUpEmailAssociation.ToLower();
                if (!RegisterUser.Email.ToLower().Contains(emailAssociation)) {
                    e.Cancel = true;
                    script += "$('#failureMessage').html('User must have a valid " + emailAssociation + " email address');";
                    canContinue = false;
                }
            }

            if ((RegisterUser.UserName.ToLower() == "newuserdefaults") || (RegisterUser.UserName.ToLower() == "demouser")) {
                canContinue = false;
                e.Cancel = true;
                script += "$('#failureMessage').html('User is already registered');";
            }

            if (canContinue) {
                MembershipUser _user = Membership.GetUser(RegisterUser.UserName);
                if (_user != null) {
                    e.Cancel = true;
                    script += "$('#failureMessage').html('User is already registered');";
                }
                else {
                    e.Cancel = false;
                    script = "";
                }
            }
        }
        else {
            e.Cancel = true;
            script += "$('#failureMessage').html('Not allowed to register user.');";
        }

        if (!string.IsNullOrEmpty(script)) {
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }
    }
    protected void RegisterUser_Continue(object sender, EventArgs e) {
        Page.Response.Redirect("~/Default.aspx");
    }
    private void SendEmail_ActivateUser(MembershipUser u, string text) {
        var message = new MailMessage();
        message.To.Add(u.Email);

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Account Activation</h1>", CheckLicense.SiteName + " : Account Activation", text);
    }

    #endregion
}