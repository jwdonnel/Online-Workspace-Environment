using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Xml;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using SocialSignInApis;
using OpenWSE_Tools.Overlays;
using OpenWSE.Core.Licensing;

public partial class SiteMaster : MasterPage {

    #region private variables

    private IPWatch _ipwatch = new IPWatch(false);
    private ServerSettings _ss = new ServerSettings();
    private StringBuilder _strScriptreg = new StringBuilder();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly Notifications _notifi = new Notifications();
    private UserNotificationMessages _usernoti;
    private List<Apps_Coll> _userAppList = new List<Apps_Coll>();
    private string _ctlId;
    private Dictionary<string, string> _demoMemberDatabase;
    private MemberDatabase _member;
    private MembershipUser _membershipuser;
    private NewUserDefaults _demoCustomizations;
    private string _username;
    private App _apps = new App();
    private string _sitetheme = "Standard";
    private bool _noLoginRequired;
    private string _ipAddress;
    private bool AssociateWithGroups = false;
    private bool _onWorkspace = false;
    private bool _inAtLeastOneGroup = false;
    private AppBuilder _wb;
    private int _totalWorkspaces = 4;
    private string _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

    #endregion


    #region Authenticate user and load page

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack) {
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
    }
    protected void Page_PreRender(object sender, EventArgs e) {
        RegisterPostbackScripts.CallPostbackControl(this.Page, IsPostBack, _member, _demoMemberDatabase);
    }
    protected void Page_Init(object sender, EventArgs e) {
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile()) {
            return;
        }

        // Check to see if the database can be auto fixed if needed
        _ss.AutoUpdateDatabaseCheck();

        // Check to see if social sign in is valid
        SocialSignIn.CheckSocialSignIn();

        IIdentity userId = HttpContext.Current.User.Identity;

        CheckIfDemoOrPreview(userId);
        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userId, IsPostBack, _noLoginRequired);
        pageLoadInit.CheckSSLRedirect();

        if (pageLoadInit.CanLoadPage) {
            if (pageLoadInit.CheckIfLicenseIsValid()) {

                CheckIfMobileOverride();

                _ipAddress = pageLoadInit.IpAddress;
                _demoCustomizations = pageLoadInit.DemoCustomizations;
                if (_demoCustomizations != null) {
                    _demoMemberDatabase = _demoCustomizations.DefaultTable;
                }

                _sitetheme = pageLoadInit.SiteTheme;
                if (string.IsNullOrEmpty(_sitetheme))
                    _sitetheme = "Standard";

                StartUpPage(userId);

                if (!IsPostBack && CheckLicense.TrialActivated && CheckLicense.LicenseValid) {
                    string trialScript = "openWSE.SetTrialText('" + CheckLicense.DaysLeftBeforeExpired + "');";
                    trialScript += "$('.purchase-icon').remove();$('.footer-padding').append(\"<a class='purchase-icon' title='Purchase the full version' href='" + ResolveUrl("~/SiteTools/ServerMaintenance/LicenseManager.aspx?purchase=true") + "'></a>\");";
                    RegisterPostbackScripts.RegisterStartupScript(this, trialScript);
                }

                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);
                if (_inAtLeastOneGroup && (!HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) && (!string.IsNullOrEmpty(pnlId))) {
                    RegisterPostbackScripts.ReInitOverlaysAndApps(this.Page, IsPostBack, AssociateWithGroups, _member, _demoMemberDatabase);
                }
            }
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }
    private void StartUpPage(IIdentity userId) {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null) {
            _ctlId = sm.AsyncPostBackSourceElementID;
        }

        if (!IsPostBack) {
            SetHelpIcon();

            string sitename = CheckLicense.SiteName;
            if (!string.IsNullOrEmpty(sitename)) {
                _strScriptreg.Append("openWSE_Config.siteName='" + sitename + "';");
            }
        }

        if ((_noLoginRequired) && (!userId.IsAuthenticated)) {
            user_profile_tab.Visible = false;
            notifications_tab.Visible = false;
            chat_client_tab_body.Visible = false;
            app_search_tab.Visible = false;

            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                _workspaceMode = _demoMemberDatabase["WorkspaceMode"];
                if (string.IsNullOrEmpty(_workspaceMode)) {
                    _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
                }

                if (!IsPostBack) {
                    if (!Request.RawUrl.ToLower().Contains("workspace.aspx")) {
                        Page.Response.Redirect("~/Workspace.aspx");
                    }

                    UserIsAuthenticated_NoLogin();
                }
            }
        }
        else if (Request.RawUrl.ToLower().Contains("sitesettings.aspx") && ServerSettings.NeedToLoadAdminNewMemberPage) {
            FormsAuthentication.SetAuthCookie(ServerSettings.AdminUserName, true);
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString() + "$('#container').hide();$('#always-visible').hide();$('#container-footer').hide();openWSE_Config.minPasswordLength=" + Membership.MinRequiredPasswordLength + ";openWSE.HelpOverlay(true);");
        }
        else {
            _username = userId.Name;
            _membershipuser = Membership.GetUser(_username);
            if ((!userId.IsAuthenticated) || ((_membershipuser.IsLockedOut) || (!_membershipuser.IsApproved))) {
                Page.Response.Redirect("~/Default.aspx");
            }
            else {
                _member = new MemberDatabase(_username);
                _apps = new App(_username);
                _usernoti = new UserNotificationMessages(_username);

                _workspaceMode = _member.WorkspaceMode.ToString();

                _inAtLeastOneGroup = false;
                Groups groups = new Groups();
                List<string> ugArray = _member.GroupList;
                foreach (string g in ugArray) {
                    if (!string.IsNullOrEmpty(groups.GetGroupName_byID(g))) {
                        _inAtLeastOneGroup = true;
                        break;
                    }
                }

                if (!_member.IsNewMember) {
                    if (!IsPostBack) {
                        UserIsAuthenticated();
                    }
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString() + "openWSE_Config.minPasswordLength=" + Membership.MinRequiredPasswordLength + ";openWSE.HelpOverlay(true);");
                }
            }
        }
    }
    protected void UserIsAuthenticated() {
        string acctImg = _member.AccountImage;
        lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(acctImg, _member.UserColor, _member.UserId, HelperMethods.MergeFMLNames(_member), _sitetheme);
        lbl_UserFullName.Text = _member.Username;
        lbl_UserEmail.Text = _membershipuser.Email;
        img_Profile.Height = 50;
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

        AssociateWithGroups = _ss.AssociateWithGroups;

        _membershipuser.LastActivityDate = DateTime.Now;
        Membership.UpdateUser(_membershipuser);

        SetMainLogo();
        SetLoginGroup();
        CheckUpdatesPopup();
        SetCurrentPageTitle();
        LoadUserCustomizations();

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();

        if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (!HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"]))) {
            _wb = new AppBuilder(MainContent, _member);
            BuildApps();
            GetUserPlugins();

            bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(_workspaceMode);

            if (_inAtLeastOneGroup) {
                string GetPageOverlayBtns = string.Empty;
                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);

                if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
                    GetPageOverlayBtns = "openWSE.GetPagedAddOverlayAndModel();";
                }
                else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
                    pnl_OverlaysAll.Enabled = false;
                    pnl_OverlaysAll.Visible = false;
                }

                if (!string.IsNullOrEmpty(pnlId)) {
                    _strScriptreg.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.CreateOverlayTable();" + GetPageOverlayBtns);
                    RefreshOverlays _ro = new RefreshOverlays(_username, ServerSettings.GetServerMapLocation, this.Page, pnlId);
                    _ro.GetWorkspaceOverlays(_userAppList);
                }
            }

            if (_onWorkspace && isComplexMode) {
                _wb.LoadSavedChatSessions();
                GetCurrentWorkspace();
            }
            _strScriptreg.Append(_wb.StrScriptreg);
        }

        CountNotiMessages();

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
            sidebar_menulinks.Visible = false;
            _strScriptreg.Append("openWSE.ToolView();");
        }

        if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (HelperMethods.DoesPageContainStr("appmanager.aspx"))) {
            app_tab_body.Visible = false;
            if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                chat_client_tab_body.Visible = false;
            }

            if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                overlay_tab.Visible = false;
                lnk_BackToWorkspace.Visible = false;
                aGroupLogoff.Visible = false;
                aGroupLogoff.Enabled = false;
                aGroupLogin.Visible = false;

                string message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #FF0000;'>OFFLINE</span></div>\");";
                if (!_ss.SiteOffLine) {
                    message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #00E000;'>ONLINE</span></div>\");";
                }
                _strScriptreg.Append(message);
            }
        }

        if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
            _strScriptreg.Clear();
        }
    }
    protected void UserIsAuthenticated_NoLogin() {
        var scriptManager = ScriptManager.GetCurrent(Page);

        aGroupLogin.Visible = false;

        SetCurrentPageTitle();
        LoadUserCustomizations();

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();

        links_tab_body.Visible = false;

        _wb = new AppBuilder(MainContent, _demoMemberDatabase);
        BuildApps_NoLogin();

        bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(_workspaceMode);

        string GetPageOverlayBtns = string.Empty;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);

        if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
            GetPageOverlayBtns = "openWSE.GetPagedAddOverlayAndModel();";
        }
        else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
            pnl_OverlaysAll.Enabled = false;
            pnl_OverlaysAll.Visible = false;
        }

        if (!string.IsNullOrEmpty(pnlId)) {
            _strScriptreg.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.CreateOverlayTable();" + GetPageOverlayBtns);
            RefreshOverlays _ro = new RefreshOverlays(string.Empty, ServerSettings.GetServerMapLocation, this.Page, pnlId);
            _ro.GetWorkspaceOverlays_NoLogin(_userAppList);
        }

        _strScriptreg.Append("$('.addOverlay-bg').remove();");
        _strScriptreg.Append(_wb.StrScriptreg);

        if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
            _strScriptreg.Clear();
        }

        LoginActivity la = new LoginActivity();
        la.AddItem("Guest User", true, ActivityType.Visited);
    }
    private void SetCurrentPageTitle() {
        if (SiteMap.CurrentNode != null) {
            if (HelperMethods.DoesPageContainStr("workspace.aspx")) {
                ddl_WorkspaceSelector.Enabled = true;
                ddl_WorkspaceSelector.Visible = true;
                lnk_BackToWorkspace.Visible = false;
                app_title_bg.Visible = false;
                _onWorkspace = true;
            }
            else {
                ddl_WorkspaceSelector.Enabled = false;
                ddl_WorkspaceSelector.Visible = false;
                lnk_BackToWorkspace.Visible = true;
                app_title_bg.Visible = true;
                _onWorkspace = false;

                string title = SiteMap.CurrentNode.Title;
                if (SiteMap.CurrentNode.ParentNode != SiteMap.RootNode) {
                    title = SiteMap.CurrentNode.ParentNode.Title;
                }

                _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('" + title + "');");
            }
        }
    }
    private void LoadUserCustomizations() {
        bool showToolTips = false;
        bool showDateTime = false;
        bool autoHideMode = false;
        bool presentationMode = false;
        bool hoverPreviewWorkspace = false;
        string animationSpeed = "150";
        bool showMinimizedPreview = false;
        bool taskBarShowAll = false;
        bool showworkspaceNumApp = false;

        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            showToolTips = !HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ToolTips"]);
            showDateTime = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowDateTime"]);
            autoHideMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["AutoHideMode"]);
            animationSpeed = _demoMemberDatabase["AnimationSpeed"];
            presentationMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["PresentationMode"]);
            hoverPreviewWorkspace = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HoverPreviewWorkspace"]) || _demoMemberDatabase["HoverPreviewWorkspace"] == string.Empty;

            int.TryParse(_demoMemberDatabase["TotalWorkspaces"], out _totalWorkspaces);
            int totalAllowed = _ss.TotalWorkspacesAllowed;
            if (_totalWorkspaces > totalAllowed || _totalWorkspaces == 0) {
                _totalWorkspaces = totalAllowed;
            }

            if (!MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                _totalWorkspaces = 1;
            }

            if (string.IsNullOrEmpty(animationSpeed))
                animationSpeed = "150";

            showMinimizedPreview = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowMinimizedPreview"]);
            if ((HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["TaskBarShowAll"])) || (string.IsNullOrEmpty(_demoMemberDatabase["TaskBarShowAll"])))
                taskBarShowAll = true;

            if (((HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowWorkspaceNumApp"])) || (string.IsNullOrEmpty(_demoMemberDatabase["ShowWorkspaceNumApp"]))) && (_demoMemberDatabase["TotalWorkspaces"] != "1"))
                showworkspaceNumApp = true;
        }
        else {
            showToolTips = _member.ShowToolTips;
            showDateTime = _member.ShowDateTime;
            autoHideMode = _member.AutoHideMode;
            presentationMode = _member.PresentationMode;
            animationSpeed = _member.AnimationSpeed.ToString();
            hoverPreviewWorkspace = _member.HoverPreviewWorkspace;
            showMinimizedPreview = _member.ShowMinimizedPreview;
            _totalWorkspaces = _member.TotalWorkspaces;
            if (_member.TaskBarShowAll)
                taskBarShowAll = true;

            if (_member.ShowWorkspaceNumApp && _member.TotalWorkspaces != 1)
                showworkspaceNumApp = true;

            if (Notifications.CheckIfErrorNotificationIsOn(_username)) {
                _strScriptreg.Append("openWSE_Config.reportAlert=false;");
            }


            #region Set User Chat Status
            _member.UpdateChatTimeStamp();
            if ((_member.ChatStatus == "Offline") && (_member.IsAway)) {
                _member.UpdateStatusChanged(true);
                _member.UpdateChatStatus("Available");
            }
            #endregion


            #region Load Links In New Page
            bool loadLinksToNewPage = _member.LoadLinksBlankPage;
            SideBarItems sidebar = new SideBarItems(_username);
            if (!loadLinksToNewPage) {
                lbl_UserFullName.Attributes["target"] = "_self";
            }

            pnl_settingLinks.Controls.Add(new LiteralControl(string.Format("{0}", sidebar.BuildAdminPages(loadLinksToNewPage))));
            #endregion

        }


        #region Show Tool Tips
        if (!showToolTips) {
            _strScriptreg.Append("$(document).tooltip({ disabled: true });");
        }
        #endregion


        #region Show Datetime
        if (showDateTime) {
            DateDisplay.InnerText = DateTime.Now.ToString("dddd, MMM dd");
            _strScriptreg.Append("startCurrentTime();");
        }
        #endregion


        #region Animation Speed
        if (!string.IsNullOrEmpty(animationSpeed))
            _strScriptreg.Append("openWSE_Config.animationSpeed=" + animationSpeed + ";");
        #endregion


        #region Set Sitetheme
        _strScriptreg.Append("openWSE_Config.siteTheme = '" + _sitetheme + "';");
        #endregion


        #region AutoHide Mode
        if (!presentationMode && autoHideMode) {
            _strScriptreg.Append("openWSE.AutoHideMode.init();");
        }
        #endregion


        #region Workspace Count
        if (_onWorkspace) {
            BuildWorkspaceddl();
            BuildWorkspacePanels();

            if (_totalWorkspaces <= 1) {
                ddl_WorkspaceSelector.Enabled = false;
                ddl_WorkspaceSelector.Visible = false;
            }
        }
        #endregion


        #region Set User Background
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            _strScriptreg.Append(AcctSettings.LoadDemoBackground(_demoMemberDatabase, _demoCustomizations, this.Page));
        }
        else {
            _strScriptreg.Append(AcctSettings.LoadUserBackground(_username, _sitetheme, this.Page));
        }
        #endregion


        #region Minimize Preview
        if (showMinimizedPreview)
            InitMinimizedPreview();
        #endregion


        #region Task Bar Show All
        if (taskBarShowAll)
            _strScriptreg.Append("openWSE_Config.taskBarShowAll=true;");
        else
            _strScriptreg.Append("openWSE_Config.taskBarShowAll=false;");
        #endregion


        #region Show workspace Number App
        if (showworkspaceNumApp)
            _strScriptreg.Append("openWSE_Config.ShowWorkspaceNumApp=true;");
        else
            _strScriptreg.Append("openWSE_Config.ShowWorkspaceNumApp=false;");
        #endregion


        if (MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {

            #region Hover Preview Workspace
            if (hoverPreviewWorkspace) {
                _strScriptreg.Append("openWSE_Config.hoverPreviewWorkspace=true;");
            }
            #endregion

        }

    }
    private void InitMinimizedPreview() {
        StringBuilder str = new StringBuilder();

        str.Append("$(document.body).on('mouseover', '.app-min-bar', function () {");
        str.Append("openWSE.HoverOverAppMin(this); });");
        str.Append("$(document.body).on('mouseleave', '.app-min-bar', function () {");
        str.Append("openWSE.HoverOutAppMin(); });");

        _strScriptreg.Append(str.ToString());
    }
    private void CheckIfDemoOrPreview(IIdentity userId) {
        _noLoginRequired = _ss.NoLoginRequired;
        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["Demo"])) {
            pnl_Login_NonAuth.Visible = false;
            _noLoginRequired = true;
        }
        else if ((_noLoginRequired) && (!userId.IsAuthenticated)) {
            pnl_Login_NonAuth.Visible = true;

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
                _strScriptreg.Append("$('#SocialLogin_borderSep').addClass('loginwith-api-borderseperate');");
            }
            else {
                _strScriptreg.Append("$('.loginwith-api-text').remove();");
            }

            if (!_ss.AllowUserSignUp) {
                _strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
                LoginOrRegister.Text = "Have an account? Sign in below";
            }
            else {
                span_signinText.InnerHtml = "Sign In / Register";
            }

            if (!_ss.EmailSystemStatus) {
                _strScriptreg.Append("$('#lnk_forgotpassword').remove();");
            }
        }
    }
    private void SetMainLogo() {
        if (!HelperMethods.DoesPageContainStr("workspace.aspx") || !MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
            pnl_groupHolder.Enabled = true;
            pnl_groupHolder.Visible = true;
            img_LoginGroup.ImageUrl = "~/Standard_Images/logo.png";
        }
    }
    private void SetLoginGroup() {
        if (Session["LoginGroup"] != null) {
            try {
                string sessionGroup = Session["LoginGroup"].ToString();
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    Dictionary<string, string> g = tempGroup.group_dt[0];

                    if (HelperMethods.DoesPageContainStr("workspace.aspx")) {
                        pnl_LoginGroupText.Enabled = false;
                        pnl_LoginGroupText.Visible = false;
                    }
                    else {
                        pnl_LoginGroupText.Enabled = true;
                        pnl_LoginGroupText.Visible = true;
                    }

                    string imgUrl = string.Empty;
                    if (HelperMethods.ConvertBitToBoolean(g["IsURL"])) {
                        imgUrl = g["Image"];
                        if (imgUrl.StartsWith("~/")) {
                            imgUrl = ResolveUrl(imgUrl);
                        }
                    }
                    else if (!string.IsNullOrEmpty(g["Image"])) {
                        imgUrl = "~/Standard_Images/Groups/Logo/" + g["Image"];
                    }

                    lbl_LoginGroup.Text = "Logged in under " + g["GroupName"];
                    if (!string.IsNullOrEmpty(imgUrl)) {
                        pnl_groupHolder.Enabled = true;
                        pnl_groupHolder.Visible = true;
                        img_LoginGroup.ImageUrl = imgUrl;
                    }

                    aGroupLogoff.Enabled = true;
                    aGroupLogoff.Visible = true;
                    aGroupLogin.Visible = false;
                }
                else {
                    Session.Remove("LoginGroup");
                }
            }
            catch { }
        }
    }
    private void SetHelpIcon() {
        SaveControls sc = new SaveControls();
        if (sc.GetTotalHelpPages(Request.Url.AbsoluteUri) == "0") {
            _strScriptreg.Append("$('.help-icon').remove();");
        }
    }
    private void CheckIfMobileOverride() {
        if (HelperMethods.DoesPageContainStr("workspace.aspx") && HelperMethods.IsMobileDevice) {
            if (!string.IsNullOrEmpty(Request.QueryString[ServerSettings.OverrideMobileSessionString.ToLower()])) {
                if (!HelperMethods.ConvertBitToBoolean(Request.QueryString[ServerSettings.OverrideMobileSessionString.ToLower()])) {
                    if (Session[ServerSettings.OverrideMobileSessionString] != null) {
                        Session.Remove(ServerSettings.OverrideMobileSessionString);
                    }
                }
                else {
                    Session[ServerSettings.OverrideMobileSessionString] = "true";
                }
            }

            if (Session[ServerSettings.OverrideMobileSessionString] == null || (Session[ServerSettings.OverrideMobileSessionString] != null && !HelperMethods.ConvertBitToBoolean(Session[ServerSettings.OverrideMobileSessionString].ToString()))) {
                Response.Redirect("~/AppRemote.aspx");
            }
        }
    }
    private void SetSimpleWorkspaceMode() {
        if (!MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
            overlay_tab.Visible = false;

            string appToOpen = Request.QueryString["AppPage"];
            if (string.IsNullOrEmpty(appToOpen) && HelperMethods.DoesPageContainStr("workspace.aspx")) {
                lnk_BackToWorkspace.Visible = false;
                appToOpen = string.Empty;
            }
            else {
                lnk_BackToWorkspace.Visible = true;
            }

            if (HelperMethods.DoesPageContainStr("workspace.aspx")) {
                app_title_bg.Visible = true;
                string title = SiteMap.CurrentNode.Title;
                if (SiteMap.CurrentNode.ParentNode != SiteMap.RootNode) {
                    title = SiteMap.CurrentNode.ParentNode.Title;
                }

                if (string.IsNullOrEmpty(appToOpen)) {
                    _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('" + title + "');");
                }
                else {
                    Apps_Coll appInformation = _apps.GetAppInformation(appToOpen);

                    string description = appInformation.Description;
                    if (!string.IsNullOrEmpty(description)) {
                        description += " - " + appInformation.About;
                    }
                    else {
                        description = appInformation.About;
                    }

                    string pageTitle = "<div id='workspace-opened-paged-app'><span class='workspace-app-title'>" + appInformation.AppName + "</span><div class='clear-space-two'></div><span class='workspace-app-description'>" + description + "</span></div>";
                    _strScriptreg.Append("$('#app_title_bg').find('.page-title').html(\"" + pageTitle + "\");");
                }
            }

            _strScriptreg.Append("openWSE_Config.workspaceMode='" + _workspaceMode + "';openWSE.PagedWorkspace('" + appToOpen + "');");
        }
    }

    #endregion


    #region Build Dashbaords

    private void BuildWorkspaceddl() {
        StringBuilder strWorkspaces = new StringBuilder();
        ddl_WorkspaceSelector.Controls.Clear();

        for (int i = 0; i < _totalWorkspaces; i++) {
            string val = (i + 1).ToString();
            string workspaceSelect = string.Format("<li class='workspace-selection-item'>Workspace {0}</li>", val);
            strWorkspaces.Append(workspaceSelect);
        }

        if (!string.IsNullOrEmpty(strWorkspaces.ToString())) {
            string selectedDb = "<span class='selected-workspace' title='Select a Workspace'>Workspace 1</span>";
            ddl_WorkspaceSelector.Controls.Add(new LiteralControl(selectedDb + "<div class='dropdown-db-selector'><div class='li-header'>Select Workspace</div><ul>" + strWorkspaces.ToString() + "</ul></div>"));
        }
    }
    private void BuildWorkspacePanels() {
        StringBuilder str = new StringBuilder();
        Panel workspace_holder = (Panel)MainContent.FindControl("workspace_holder");
        if (workspace_holder != null) {
            workspace_holder.Controls.Clear();
            for (int i = 0; i < _totalWorkspaces; i++) {
                string val = (i + 1).ToString();
                Panel pnlDH = new Panel();
                pnlDH.ID = "workspace_" + val;
                pnlDH.CssClass = "workspace-holder";
                HtmlGenericControl bgWorkspace = new HtmlGenericControl();
                bgWorkspace.ID = "bg_workspace_" + val;
                bgWorkspace.Attributes["class"] = "workspace-backgrounds-fixed";

                pnlDH.Controls.Add(bgWorkspace);
                workspace_holder.Controls.Add(pnlDH);
            }
        }
    }

    #endregion


    #region Form Buttons

    protected void aGroupLogoff_Click(object sender, EventArgs e) {
        try {
            if (Session["LoginGroup"] != null) {
                LoginActivity la = new LoginActivity();
                la.AddItem(_username, true, ActivityType.Logout);

                Session.Remove("LoginGroup");
            }
        }
        catch { }

        string url = Context.Request.Url.OriginalString;
        Page.Response.Redirect(url);
    }
    protected void lbtn_signoff_Click(object sender, EventArgs e) {
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

            FormsAuthentication.SignOut();
            if (Session["LoginGroup"] != null) {
                string groupName = Session["LoginGroup"].ToString();
                Session.Remove("LoginGroup");
                if (!string.IsNullOrEmpty(groupName)) {
                    Response.Redirect("~/Default.aspx?group=" + groupName);
                }
            }

            LoginActivity la = new LoginActivity();
            la.AddItem(_username, true, ActivityType.Logout);

            FormsAuthentication.RedirectToLoginPage();
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

        Page.Response.Redirect("~/Workspace.aspx");
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

    protected void lbtn_signinwith_Google_Click(object sender, EventArgs e) {
        SocialSignIn.GoogleSignIn();
    }
    protected void lbtn_signinwith_Twitter_Click(object sender, EventArgs e) {
        SocialSignIn.TwitterSignIn();
    }
    protected void lbtn_signinwith_Facebook_Click(object sender, EventArgs e) {
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

    #endregion


    #region Auto Update

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        bool cancontinue = false;
        if (_member != null) {
            if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
                string id = _uuf.getFlag_AppID(hf_UpdateAll.Value);
                if (id == "workspace") {
                    _uuf.deleteFlag(hf_UpdateAll.Value);
                    cancontinue = true;
                }
                else if (hf_UpdateAll.Value == "refresh")
                    Response.Redirect("~/Workspace.aspx?wi=" + DateTime.Now.Ticks.ToString());
            }
        }

        if (cancontinue) {
            foreach (Control control in Page.Form.Controls) {
                if (control.HasControls())
                    RegisterAsynControls(control.Controls);
                if (control is UpdatePanel) {
                    var up = control as UpdatePanel;
                    if (up.UpdateMode == UpdatePanelUpdateMode.Conditional)
                        up.Update();
                    else {
                        if (control.HasControls())
                            RegisterAsynControls(control.Controls);
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


    #region Controls for Notifications

    private void CountNotiMessages() {
        int total = 0;
        _usernoti.getEntries("ASC");

        foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
            Notifications_Coll coll = _notifi.GetNotification(x.NotificationID);
            if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!string.IsNullOrEmpty(x.ID))) {
                total++;
                if (IsPostBack) {
                    hf_noti_update_hiddenField.Value = "true";
                    lbl_noti_update_message.Text = coll.NotificationName;
                    img_noti_update_image.ImageUrl = coll.NotificationImage;
                }
            }
        }

        if (total == 0) {
            lbl_notifications.Text = "0";
            lbl_notifications.CssClass = "notifications-none";
            lbl_notifications.ToolTip = "You have no new notifications";
        }
        else {
            lbl_notifications.Text = total.ToString(CultureInfo.InvariantCulture);
            lbl_notifications.CssClass = "notifications-new";
            string notiPlural = "notifications";
            if (total == 1)
                notiPlural = "notification";

            lbl_notifications.ToolTip = "You have " + total.ToString(CultureInfo.InvariantCulture) + " new " + notiPlural;
        }
    }

    #endregion


    #region Build Icon List

    private int _totalApps = 0;
    private void BuildApps() {
        var appCategoryScript = new StringBuilder();
        var appScript = new StringBuilder();
        var categories = new List<string>();
        var appCategory = new AppCategory(false);
        bool groupIcons = _member.GroupIcons;

        if (groupIcons) {
            appCategory = new AppCategory(true);
            appCategoryScript.Append("<div id='Category-Back' style='display:none'>");
            appCategoryScript.Append("<img alt='back' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/prevpage.png") + "' />");
            appCategoryScript.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text if needed
            appCategoryScript.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
            appCategoryScript.Append("<div id='Category-Back-Name-id' style='display:none'></div>");
        }

        if (pnl_icons == null) {
            return;
        }

        if (!_inAtLeastOneGroup) {
            pnl_icons.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big pad-right'>Must be associated with a group</h3>"));
        }
        else {
            _apps.GetUserInstalledApps();
            bool hideAllIcons = _ss.HideAllAppIcons;

            List<string> memberApps = _apps.DeleteDuplicateEnabledApps(_member);

            foreach (var w in memberApps) {
                Apps_Coll dt = _apps.GetAppInformation(w);

                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(dt, _member)) {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(dt.AppId)) {
                    _member.RemoveEnabledApp(w);
                    continue;
                }

                if ((_username.ToLower() != dt.CreatedBy.ToLower()) && (dt.IsPrivate)) {
                    continue;
                }

                _userAppList.Add(dt);

                int ar = 0;
                int am = 0;
                if (dt.AllowMaximize)
                    am = 1;
                if (dt.AllowResize)
                    ar = 1;

                string css = dt.CssClass;
                if (string.IsNullOrEmpty(css))
                    css = "app-main";

                if (!string.IsNullOrEmpty(dt.filename)) {
                    var fi = new FileInfo(dt.filename);

                    if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif") && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {

                        if (groupIcons) {
                            string categoryId = dt.Category;
                            string[] categorySplit = categoryId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string c in categorySplit) {
                                string cId = c;
                                string categoryname = appCategory.GetCategoryName(c);
                                if (categoryname == "Uncategorized") {
                                    cId = categoryname;
                                }

                                if (!categories.Contains(categoryname)) {
                                    appCategoryScript.Append(BuildCategory(cId, categoryname));
                                    categories.Add(categoryname);
                                }
                                appScript.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                                appScript.Append(BuildIcon(dt.AppId, fi, dt.Icon, cId, dt, dt.AppName, hideAllIcons));
                                appScript.Append("</div>");
                            }
                        }
                        else {
                            appScript.Append(BuildIcon(dt.AppId, fi, dt.Icon, dt.Category, dt, dt.AppName, hideAllIcons));
                        }

                        if (_onWorkspace) {
                            // Build the app while building the icon
                            string workspace = _apps.GetCurrentworkspace(dt.AppId);
                            if (string.IsNullOrEmpty(workspace)) {
                                if (dt.DefaultWorkspace != "0") {
                                    workspace = "workspace_" + dt.DefaultWorkspace;
                                }
                                else {
                                    workspace = "workspace_1";
                                }
                            }

                            bool canBuild = true;
                            if (dt.AppId != Request.QueryString["AppPage"] && !MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                                canBuild = false;
                            }

                            if (canBuild) {
                                _wb.AppDivGenerator(dt.AppId, dt.AppName, dt.Icon, ar, am, css, workspace, dt.filename, fi.Extension.ToLower() == ".ascx",
                                                   dt.MinHeight, dt.MinWidth, dt.AllowPopOut,
                                                   dt.PopOutLoc, dt.AutoFullScreen, dt.AutoLoad, dt.AutoOpen);
                            }
                        }
                        _totalApps++;
                    }
                }
            }

            pnl_icons.Controls.Clear();

            if (_totalApps > 0) {
                string xIcons = appScript.ToString();
                if (!string.IsNullOrEmpty(appCategoryScript.ToString())) {
                    xIcons = appCategoryScript.ToString() + xIcons;
                }

                pnl_icons.Controls.Add(new LiteralControl(xIcons));
            }
            else {
                pnl_icons.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big'>No Apps Installed</h3>"));
            }
        }

        AreAppIconLocked();
    }
    private void BuildApps_NoLogin() {
        var appCategoryScript = new StringBuilder();
        var appScript = new StringBuilder();
        var categories = new List<string>();
        var appCategory = new AppCategory(false);
        bool groupIcons = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["GroupIcons"]);

        if (groupIcons) {
            appCategory = new AppCategory(true);
            appCategoryScript.Append("<div id='Category-Back' style='display:none'>");
            appCategoryScript.Append("<img alt='back' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/prevpage.png") + "' />");
            appCategoryScript.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text if needed
            appCategoryScript.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
            appCategoryScript.Append("<div id='Category-Back-Name-id' style='display:none'></div>");
        }

        if (pnl_icons == null) {
            return;
        }

        AppPackages package = new AppPackages(false);
        string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);

        bool hideAllIcons = _ss.HideAllAppIcons;
        foreach (var w in appList) {
            Apps_Coll dt = _apps.GetAppInformation(w);

            _userAppList.Add(dt);

            int ar = 0;
            int am = 0;
            if (dt.AllowMaximize)
                am = 1;
            if (dt.AllowResize)
                ar = 1;

            string css = dt.CssClass;
            if (string.IsNullOrEmpty(css))
                css = "app-main";

            if (!string.IsNullOrEmpty(dt.filename)) {
                var fi = new FileInfo(dt.filename);

                if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif") && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {
                    if (groupIcons) {
                        string categoryId = dt.Category;
                        string[] categorySplit = categoryId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string c in categorySplit) {
                            string cId = c;
                            string categoryname = appCategory.GetCategoryName(cId);
                            if (categoryname == "Uncategorized") {
                                cId = categoryname;
                            }

                            if (!categories.Contains(categoryname)) {
                                appCategoryScript.Append(BuildCategory(cId, categoryname));
                                categories.Add(categoryname);
                            }
                            appScript.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                            appScript.Append(BuildIcon(dt.AppId, fi, dt.Icon, cId, dt, dt.AppName, hideAllIcons));
                            appScript.Append("</div>");
                        }
                    }
                    else {
                        appScript.Append(BuildIcon(dt.AppId, fi, dt.Icon, dt.Category, dt, dt.AppName, hideAllIcons));
                    }

                    if (_onWorkspace) {
                        // Build the app while building the icon
                        string workspace = "workspace_1";
                        if (dt.DefaultWorkspace != "0") {
                            workspace = "workspace_" + dt.DefaultWorkspace;
                        }

                        bool canBuild = true;
                        if (dt.AppId != Request.QueryString["AppPage"] && !MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                            canBuild = false;
                        }

                        if (canBuild) {
                            _wb.AppDivGenerator_NoLogin(dt.AppId, dt.AppName, dt.Icon, ar, am, css, workspace, dt.filename, fi.Extension.ToLower() == ".ascx",
                                               dt.MinHeight, dt.MinWidth, dt.AllowPopOut,
                                               dt.PopOutLoc, dt.AutoFullScreen, dt.AutoLoad, dt.AutoOpen);
                        }
                    }

                    _totalApps++;
                }
            }
        }

        pnl_icons.Controls.Clear();

        if (_totalApps > 0) {
            string xIcons = appScript.ToString();
            if (!string.IsNullOrEmpty(appCategoryScript.ToString())) {
                xIcons = appCategoryScript.ToString() + xIcons;
            }

            pnl_icons.Controls.Add(new LiteralControl(xIcons));
        }
        else {
            pnl_icons.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big'>No Apps Installed</h3>"));
        }

        AreAppIconLocked();
    }

    private string BuildIcon(string id, FileInfo fi, string iconname, string category, Apps_Coll dt, string w, bool hideAllIcons) {
        StringBuilder popup = new StringBuilder();
        var appScript = new StringBuilder();
        var appCategory = new AppCategory(false);
        popup.Append("<div class='app-popup'>");
        popup.Append("Workspace:<select class='app-popup-selector margin-left'>");
        popup.Append("<option>-</option>");
        for (int ii = 0; ii < _totalWorkspaces; ii++) {
            popup.Append("<option>" + (ii + 1).ToString() + "</option>");
        }
        popup.Append("</select></div>");

        string tooltip = string.Empty;
        if (!string.IsNullOrEmpty(dt.Description))
            tooltip = " title='" + dt.Description.Replace("'", "") + "'";

        string iconImg = "<img alt='' src='" + ResolveUrl("~/Standard_Images/App_Icons/" + iconname) + "' />";
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            if ((hideAllIcons) || (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAppIcon"])))
                iconImg = string.Empty;
        }
        else {
            if ((hideAllIcons) || (_member.HideAppIcon))
                iconImg = string.Empty;
        }

        string canPopoutAttr = string.Empty;
        if ((!_onWorkspace) && (dt.AllowPopOut) && (!string.IsNullOrEmpty(dt.PopOutLoc))) {
            canPopoutAttr = " popoutloc='" + _wb.CheckPopoutURL(dt.PopOutLoc) + "'";
        }

        string currWorkspace = string.Empty;
        if (!_onWorkspace) {
            currWorkspace = " currentWorkspace='" + _member.CurrentWorkspace + "'";
        }

        if (fi.Extension.ToLower() == ".ascx") {
            string needtoload = "1";
            if (_apps.GetAutoLoad(id)) {
                needtoload = "0";
                appScript.Append("<input type='hidden' id='hf_" + id + "' />");
            }
            appScript.Append("<div" + canPopoutAttr + currWorkspace + " data-appId='" + id + "' class='app-icon' runat='server'" + tooltip + " onclick=\"openWSE.DetermineNeedPostBack(this, " + needtoload + ")\">");
            if (_totalWorkspaces > 1) {
                appScript.Append("<span class='app-options' style='visibility: hidden;'>" + popup + "</span>");
            }
            appScript.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
        }
        else {
            appScript.Append("<div" + canPopoutAttr + currWorkspace + " data-appId='" + id + "' class='app-icon'" + tooltip + " onclick=\"openWSE.DetermineNeedPostBack(this, 0)\">");
            if (_totalWorkspaces > 1) {
                appScript.Append("<span class='app-options' style='visibility: hidden;'>" + popup + "</span>");
            }
            appScript.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
        }

        return appScript.ToString();
    }
    private string BuildCategory(string id, string category) {
        var str = new StringBuilder();

        int categoryCount = 0;
        if (id == category && id != "Uncategorized") {
            categoryCount = GetAppCount_Category("");
        }
        else if (id == "Uncategorized") {
            categoryCount = GetAppCount_Category_Uncategorized();
        }
        else {
            categoryCount = GetAppCount_Category(id);
        }


        if (categoryCount > 0) {
            string count = string.Empty;
            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                if (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["IconCategoryCount"]))
                    count = " (" + categoryCount + ")";
            }
            else {
                if (_member.ShowCategoryCount)
                    count = " (" + categoryCount + ")";
            }

            str.Append("<div data-appId='" + id + "' class='app-icon-category-list' runat='server' onclick=\"openWSE.CategoryClick('" + id + "', '" + category + "')\">");
            str.Append("<span class='app-icon-font'>" + category + count + "</span>");
            str.Append("<img alt='forward' src='" + ResolveUrl("~/App_Themes/" + _sitetheme + "/Icons/nextpage.png") + "' /></div>");
        }
        return str.ToString();
    }
    private int GetAppCount_Category(string id) {
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            AppPackages package = new AppPackages(false);
            string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);

            List<string> tempList = appList.ToList();
            return _apps.GetApps_byCategory(id).Cast<Apps_Coll>().Count(dr => tempList.Contains(dr.AppId));
        }
        else {
            List<string> userapps = _member.EnabledApps;
            return _apps.GetApps_byCategory(id).Cast<Apps_Coll>().Count(dr => userapps.Contains(dr.AppId));
        }

        return 0;
    }
    private int GetAppCount_Category_Uncategorized() {
        int count = 0;

        _apps.GetAllApps();
        List<Apps_Coll> appColl = _apps.AppList;

        AppCategory app_category = new AppCategory(false);

        foreach (Apps_Coll dr in appColl) {
            string categoryname = app_category.GetCategoryName(dr.Category);
            if (categoryname == "Uncategorized") {
                count++;
            }
        }
        return count;
    }

    private void AreAppIconLocked() {
        bool appsLocked = true;
        string canSave = "false";
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            if (!HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["LockAppIcons"]))
                appsLocked = false;
        }
        else {
            canSave = "true";
            if (!_member.LockAppIcons) {
                appsLocked = false;
            }
        }

        if (!appsLocked) {
            _strScriptreg.Append("openWSE.AppsSortUnlocked(" + canSave + ");");
        }
    }

    #endregion


    #region Build and Load Apps

    private void GetCurrentWorkspace() {
        int currentWorkspace = _member.CurrentWorkspace;

        if (currentWorkspace > _totalWorkspaces) {
            currentWorkspace = 1;
        }
        else if (currentWorkspace == 0) {
            currentWorkspace = 1;
        }

        _strScriptreg.Append("openWSE.LoadCurrentWorkspace('" + currentWorkspace + "');");
    }

    #endregion


    #region Site Plugins Load

    private void GetUserPlugins() {
        if (!_ss.SitePluginsLocked) {
            SitePlugins _plugins = new SitePlugins(_username);
            _plugins.BuildSitePluginsForUser();

            foreach (UserPlugins_Coll pl in _plugins.userplugins_dt) {
                AddPluginToPage(pl.PluginID);
            }
        }
    }
    private void GetPluginAssociations(string id) {
        SitePlugins _plugins = new SitePlugins(_username);
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
        SitePlugins sp = new SitePlugins(_username);
        SitePlugins_Coll plugin = sp.GetPlugin(id);

        if (AssociateWithGroups) {
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

    private void CheckUpdatesPopup() {
        ShowUpdatePopup sup = new ShowUpdatePopup();
        if (sup.isUserShowPopup(_username)) {
            if (string.IsNullOrEmpty(_sitetheme))
                _sitetheme = "Standard";

            string message = sup.GetNewUpdateMessage(ServerSettings.GetServerMapLocation, _sitetheme);
            string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
            sup.UpdateUser(false, _username);
            _strScriptreg.Append("openWSE.ShowUpdatesPopup('" + encodedMessage + "');");
        }
    }

}