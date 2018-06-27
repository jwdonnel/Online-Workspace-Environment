using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.Overlays;
using SocialSignInApis;

public class BaseMaster : MasterPage {

    #region Private Properties

    private MemberDatabase _currentUserMemberDatabase = null;
    private MembershipUser _currentUserMembership = null;
    private NewUserDefaults _demoCustomizations = null;
    private string _currentUsername = string.Empty;
    private AppIconBuilder _appIconBuilder = null;
    private UserNotificationMessages _userNotificationMessages = null;
    private bool _noLoginRequired = false;
    private bool isValidLoad = false;
    private HttpRequest CurrentRequest {
        get {
            if (HttpContext.Current != null) {
                return HttpContext.Current.Request;
            }

            return Page.Request;
        }
    }
    private HttpResponse CurrentResponse {
        get {
            if (HttpContext.Current != null) {
                return HttpContext.Current.Response;
            }

            return Page.Response;
        }
    }
    private HttpSessionState CurrentSession {
        get {
            if (HttpContext.Current != null && HttpContext.Current.Session != null) {
                return HttpContext.Current.Session;
            }

            return Page.Session;
        }
    }

    #endregion

    #region Protected Properties

    protected IIdentity CurrentUserIdentity {
        get {
            if (HttpContext.Current != null && HttpContext.Current.User != null) {
                return HttpContext.Current.User.Identity;
            }

            return null;
        }
    }
    protected readonly ServerSettings MainServerSettings = new ServerSettings();
    protected bool UserIsAuthenticated {
        get {
            if (CurrentUserIdentity != null && CurrentUserIdentity.IsAuthenticated) {
                return true;
            }
            return false;
        }
    }
    protected string CurrentUsername {
        get {
            if (UserIsAuthenticated && string.IsNullOrEmpty(_currentUsername)) {
                _currentUsername = CurrentUserIdentity.Name;
            }

            return _currentUsername;
        }
    }
    protected MemberDatabase CurrentUserMemberDatabase {
        get {
            if (UserIsAuthenticated && _currentUserMemberDatabase == null) {
                _currentUserMemberDatabase = new MemberDatabase(CurrentUserIdentity.Name);
            }

            return _currentUserMemberDatabase;
        }
    }
    protected MembershipUser CurrentUserMembership {
        get {
            if (!string.IsNullOrEmpty(CurrentUsername) && _currentUserMembership == null) {
                _currentUserMembership = Membership.GetUser(CurrentUsername);
            }

            return _currentUserMembership;
        }
    }
    protected NewUserDefaults DemoCustomizations {
        get {
            return _demoCustomizations;
        }
    }
    protected string AsyncPostBackSourceElementID {
        get {
            return BaseMaster.GetPostBackControlId(Page);
        }
    }
    protected string CurrentIpAddress {
        get {
            return BasePage.GetCurrentPageIpAddress(CurrentRequest);
        }
    }
    protected string CurrentSiteTheme {
        get {
            string siteTheme = string.Empty;
            if (CurrentUserMemberDatabase != null) {
                siteTheme = CurrentUserMemberDatabase.SiteTheme;
            }
            else if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0 && _demoCustomizations.DefaultTable.ContainsKey("Theme")) {
                siteTheme = _demoCustomizations.DefaultTable["Theme"];
            }

            if (string.IsNullOrEmpty(siteTheme)) {
                siteTheme = "Standard";
            }

            return siteTheme;
        }
    }
    protected string CurrentWorkspaceMode {
        get {
            string workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
            if (CurrentUserMemberDatabase != null) {
                workspaceMode = CurrentUserMemberDatabase.WorkspaceMode.ToString();
            }
            else if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0 && _demoCustomizations.DefaultTable.ContainsKey("WorkspaceMode")) {
                workspaceMode = _demoCustomizations.DefaultTable["WorkspaceMode"];
            }

            if (string.IsNullOrEmpty(workspaceMode)) {
                workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
            }

            return workspaceMode;
        }
    }
    protected List<string> CurrentUserGroupList {
        get {
            if (CurrentUserMemberDatabase != null) {
                return CurrentUserMemberDatabase.GetCompleteUserGroupList;
            }

            return new List<string>();
        }
    }
    protected AppIconBuilder CurrentAppIconBuilderObject {
        get {
            if (_appIconBuilder == null) {
                if (CurrentUserMemberDatabase != null) {
                    _appIconBuilder = new AppIconBuilder(Page, CurrentUserMemberDatabase);
                }
                else if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0) {
                    _appIconBuilder = new AppIconBuilder(Page, _demoCustomizations.DefaultTable);
                }
            }

            return _appIconBuilder;
        }
    }
    protected bool NoLoginRequired {
        get {
            return _noLoginRequired;
        }
    }
    protected UserNotificationMessages CurrentUserNotificationMessagesObject {
        get {
            if (UserIsAuthenticated && !string.IsNullOrEmpty(CurrentUsername) && _userNotificationMessages == null) {
                _userNotificationMessages = new UserNotificationMessages(CurrentUsername);
            }
            return _userNotificationMessages;
        }
    }
    protected readonly UserNotifications CurrentNotificationsObject = new UserNotifications();
    protected readonly UserUpdateFlags CurrentUserUpdateFlagsObject = new UserUpdateFlags();

    #endregion

    #region Load Methods

    protected override void OnInit(EventArgs e) {
        ServerSettings.StartServerApplication(IsPostBack);
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile) {
            return;
        }

        if (MainServerSettings.ForceGroupLogin && !UserIsAuthenticated) {
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if (MainServerSettings.ForceGroupLogin && UserIsAuthenticated && !BasePage.IsUserNameEqualToAdmin(CurrentUsername) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            if (CurrentUserGroupList.Count == 1) {
                GroupSessions.AddOrSetNewGroupLoginSession(CurrentUsername, CurrentUserGroupList[0]);
            }
            else {
                HelperMethods.PageRedirect("~/GroupLogin.aspx");
            }
        }

        // Check to see if the database can be auto fixed if needed
        MainServerSettings.AutoUpdateDatabaseCheck();

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();

        SetNoLoginRequiredVar();
        isValidLoad = BaseMaster.IsPageValid(Page, _noLoginRequired);

        #region Initialize User Customizations
        if (isValidLoad) {
            if (_noLoginRequired && !UserIsAuthenticated) {
                _demoCustomizations = new NewUserDefaults("DemoNoLogin");
                _demoCustomizations.GetDefaults();
                SetupDemoCustomizations();
            }
            else if (UserIsAuthenticated && !string.IsNullOrEmpty(CurrentUsername)) {
                SetupUserCustomizations();
            }
            else if (!ServerSettings.NeedToLoadAdminNewMemberPage) {
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
                return;
            }

            string pnlId = WorkspaceOverlays.GetOverlayPanelId(Page, CurrentWorkspaceMode);
            if (CurrentAppIconBuilderObject != null && CurrentAppIconBuilderObject.InAtLeastOneGroup && !string.IsNullOrEmpty(pnlId)) {
                Dictionary<string, string> demoDefaultTable = null;
                if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
                    demoDefaultTable = _demoCustomizations.DefaultTable;
                }

                RegisterPostbackScripts.ReInitOverlaysAndApps(Page, IsPostBack, MainServerSettings.AssociateWithGroups, CurrentUserMemberDatabase, demoDefaultTable);
            }

            RegisterOpenWSEConfigSettings();
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }

        #endregion

        // Register AutoUpdate scripts if User is Authenticated
        if (UserIsAuthenticated) {
            BaseMaster.RegisterAutoScripts(Page);
        }

        // Register ViewPorts
        BaseMaster.RegisterViewPorts(Page);

        // Load All Scripts
        LoadAllDefaultScriptsStyleSheets(Page);

        #region Remove Group Session if necessary
        try {
            if (UserIsAuthenticated && GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                GroupIPListener groupNetwork = new GroupIPListener();
                Groups groups = new Groups(CurrentUsername);
                if (!groups.IsApartOfGroup(CurrentUserGroupList, sessionGroup) || (!groupNetwork.CheckIfActive(sessionGroup, CurrentIpAddress) && groupNetwork.HasAtLeastOneActive(sessionGroup))) {
                    GroupSessions.RemoveGroupLoginSession(CurrentUsername);
                }
            }
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }
        #endregion

        // Set Help Icon
        BaseMaster.SetHelpIcon(Page);

        #region Load Workspace
        if (string.IsNullOrEmpty(AsyncPostBackSourceElementID) || !IsPostBack) {
            Control masterPage = BasePage.GetMasterPageControl(Page);
            HtmlGenericControl site_mainbody = (HtmlGenericControl)masterPage.FindControl("site_mainbody");
            if (site_mainbody != null) {
                if (HelperMethods.DoesPageContainStr("default.aspx")) {
                    BuildWorkspacePanels(masterPage);
                    BuildWorkspaceddl(masterPage);
                }

                LoadUserBackground(site_mainbody);
                if (!MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode) || !HelperMethods.DoesPageContainStr("default.aspx")) {
                    if (site_mainbody.Attributes["class"] == null) {
                        site_mainbody.Attributes["class"] = "container-main-bg-simple";
                    }
                    else if (!site_mainbody.Attributes["class"].Contains("container-main-bg-simple")) {
                        site_mainbody.Attributes["class"] += " container-main-bg-simple";
                    }

                    if (HelperMethods.ConvertBitToBoolean(CurrentRequest.QueryString["mobileMode"]) || HelperMethods.ConvertBitToBoolean(CurrentRequest.QueryString["iframeMode"])) {
                        site_mainbody.Attributes["class"] += " app-remote-container";
                    }
                }
            }
        }
        #endregion

        base.OnInit(e);
    }
    protected override void OnPreRender(EventArgs e) {
        Dictionary<string, string> demoDefaultTable = null;
        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            demoDefaultTable = _demoCustomizations.DefaultTable;
        }

        RegisterPostbackScripts.CallPostbackControl(Page, IsPostBack, CurrentUserMemberDatabase, demoDefaultTable);
        base.OnPreRender(e);
    }
    protected override void OnLoad(EventArgs e) {
        // Record page views
        if (isValidLoad) {
            BaseMaster.RecordPageViews(Page);
        }

        base.OnLoad(e);
    }

    #endregion

    #region Initialize Page Methods

    private string _appStyle = MemberDatabase.AppStyle.Style_1.ToString();
    private string _backgroundPosition = "right center";
    private string _backgroundSize = "auto";
    private string _backgroundRepeat = "no-repeat";
    private string _backgroundColor = "#FFFFFF";
    private bool _showToolTips = false;
    private bool _showDateTime = false;
    private string _animationSpeed = "150";
    private bool _siteTips = false;
    private int _totalNumberWorkspaces = 4;
    private bool _multiEnabled = false;
    private string _backgrounTimer = "30";
    private int _currentWorkspace = 1;

    private void SetupDemoCustomizations() {
        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0) {
            _appStyle = _demoCustomizations.DefaultTable["UserAppStyle"];
            _backgroundPosition = _demoCustomizations.DefaultTable["BackgroundPosition"];
            _backgroundSize = _demoCustomizations.DefaultTable["BackgroundSize"];
            _backgroundColor = _demoCustomizations.DefaultTable["BackgroundColor"];
            if (string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundRepeat"]) || HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["BackgroundRepeat"])) {
                _backgroundRepeat = "repeat";
            }

            _showToolTips = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["ToolTips"]);
            _showDateTime = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["ShowDateTime"]);
            _animationSpeed = _demoCustomizations.DefaultTable["AnimationSpeed"];
            if (string.IsNullOrEmpty(_animationSpeed)) {
                _animationSpeed = "150";
            }

            if (HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["SiteTipsOnPageLoad"]) || string.IsNullOrEmpty(_demoCustomizations.DefaultTable["SiteTipsOnPageLoad"])) {
                _siteTips = true;
            }

            if (string.IsNullOrEmpty(_backgroundColor)) {
                _backgroundColor = "#FFFFFF";
            }

            if (!_backgroundColor.StartsWith("#")) {
                _backgroundColor = "#" + _backgroundColor;
            }

            _multiEnabled = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["EnableBackgrounds"]);
            if (_demoCustomizations.DefaultTable.ContainsKey("BackgroundLoopTimer") && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundLoopTimer"])) {
                _backgrounTimer = _demoCustomizations.DefaultTable["BackgroundLoopTimer"];

                int timerOut = 30;
                int.TryParse(_backgrounTimer, out timerOut);

                if (timerOut <= 0) {
                    timerOut = 30;
                }
                _backgrounTimer = timerOut.ToString();
            }

            int.TryParse(DemoCustomizations.DefaultTable["TotalWorkspaces"], out _totalNumberWorkspaces);
            int totalAllowed = MainServerSettings.TotalWorkspacesAllowed;
            if (_totalNumberWorkspaces > totalAllowed || _totalNumberWorkspaces == 0) {
                _totalNumberWorkspaces = totalAllowed;
            }
            if (!MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                _totalNumberWorkspaces = 1;
            }
        }
    }
    private void SetupUserCustomizations() {
        _appStyle = CurrentUserMemberDatabase.UserAppStyle.ToString();
        _backgroundPosition = CurrentUserMemberDatabase.BackgroundPosition;
        _backgroundSize = CurrentUserMemberDatabase.BackgroundSize;
        _backgroundColor = CurrentUserMemberDatabase.BackgroundColor;
        if (CurrentUserMemberDatabase.BackgroundRepeat) {
            _backgroundRepeat = "repeat";
        }

        _showToolTips = CurrentUserMemberDatabase.ShowToolTips;
        _showDateTime = CurrentUserMemberDatabase.ShowDateTime;
        _animationSpeed = CurrentUserMemberDatabase.AnimationSpeed.ToString();
        _siteTips = CurrentUserMemberDatabase.SiteTipsOnPageLoad;

        if (string.IsNullOrEmpty(_backgroundColor)) {
            _backgroundColor = "#FFFFFF";
        }

        if (!_backgroundColor.StartsWith("#")) {
            _backgroundColor = "#" + _backgroundColor;
        }

        #region Set User Chat Status
        if (MainServerSettings.ChatEnabled && CurrentUserMemberDatabase.ChatEnabled) {
            CurrentUserMemberDatabase.UpdateChatTimeStamp();
            if (CurrentUserMemberDatabase.ChatStatus == "Offline" && CurrentUserMemberDatabase.IsAway) {
                CurrentUserMemberDatabase.UpdateStatusChanged(true);
                CurrentUserMemberDatabase.UpdateChatStatus("Available");
            }
        }
        #endregion

        _multiEnabled = CurrentUserMemberDatabase.MultipleBackgrounds;
        _backgrounTimer = CurrentUserMemberDatabase.BackgroundLoopTimer;
        _totalNumberWorkspaces = CurrentUserMemberDatabase.TotalWorkspaces;
        if (!MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
            _totalNumberWorkspaces = 1;
        }

        _currentWorkspace = CurrentUserMemberDatabase.CurrentWorkspace;
    }
    private void RegisterOpenWSEConfigSettings() {
        StringBuilder strScripts = new StringBuilder();
        strScripts.Append("openWSE_Config.siteRootFolder='" + Page.ResolveUrl("~/").Replace("/", "") + "';");
        strScripts.Append("openWSE_Config.saveCookiesAsSessions=" + MainServerSettings.SaveCookiesAsSessions.ToString().ToLower() + ";");
        strScripts.Append("openWSE_Config.defaultBackgroundColor='" + _backgroundColor + "';");
        strScripts.Append("openWSE_Config.defaultBackgroundPosition='" + _backgroundPosition + "';");
        strScripts.Append("openWSE_Config.defaultBackgroundSize='" + _backgroundSize + "';");
        strScripts.Append("openWSE_Config.defaultBackgroundRepeat='" + _backgroundRepeat + "';");
        strScripts.Append("openWSE_Config.appStyle='" + _appStyle + "';");
        strScripts.Append("openWSE_Config.appendTimestampOnScripts=" + MainServerSettings.AppendTimestampOnScripts.ToString().ToLower() + ";");
        strScripts.Append("openWSE_Config.timestampQuery='" + ServerSettings.TimestampQuery + "';");
        strScripts.Append("openWSE_Config.siteName='" + ServerSettings.SiteName + "';");
        strScripts.Append("openWSE_Config.reportOnError=" + (!MainServerSettings.DisableJavascriptErrorAlerts).ToString().ToLower() + ";");
        strScripts.Append("openWSE_Config.siteTheme = '" + CurrentSiteTheme + "';");
        strScripts.Append("openWSE_Config.showToolTips=" + _showToolTips.ToString().ToLower() + ";");
        strScripts.Append("openWSE_Config.multipleBackgrounds=" + _multiEnabled.ToString().ToLower() + ";");
        strScripts.Append("openWSE_Config.backgroundTimerLoop=" + _backgrounTimer + ";");

        if (HelperMethods.DoesPageContainStr("default.aspx")) {
            if (_totalNumberWorkspaces <= 1 || !MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                strScripts.Append("$('#workspace-selector').hide();");
            }
            else if (_totalNumberWorkspaces > 1 && MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                strScripts.Append("$('#workspace-selector').show();");
            }

            if (_currentWorkspace > _totalNumberWorkspaces) {
                _currentWorkspace = _totalNumberWorkspaces;
            }
            else if (_currentWorkspace == 0) {
                _currentWorkspace = 1;
            }

            strScripts.Append("openWSE.LoadCurrentWorkspace('" + _currentWorkspace + "');");
        }

        if (_showDateTime) {
            strScripts.Append("showCurrentTime();");
        }
        if (!string.IsNullOrEmpty(_animationSpeed)) {
            strScripts.Append("openWSE_Config.animationSpeed=" + _animationSpeed + ";");
        }
        strScripts.Append("openWSE_Config.siteTipsOnPageLoad=" + _siteTips.ToString().ToLower() + ";");
        if (!string.IsNullOrEmpty(CurrentUsername) && UserNotifications.CheckIfErrorNotificationIsOn(CurrentUsername)) {
            strScripts.Append("openWSE_Config.reportAlert=false;");
        }

        RegisterPostbackScripts.RegisterStartupScript(Page, strScripts.ToString());
    }

    private void BuildWorkspacePanels(Control masterPage) {
        if (masterPage != null) {
            Panel workspace_holder = (Panel)masterPage.FindControl("MainContent").FindControl("workspace_holder");
            if (workspace_holder != null) {
                StringBuilder str = new StringBuilder();
                workspace_holder.Controls.Clear();
                for (int i = 0; i < _totalNumberWorkspaces; i++) {
                    string val = (i + 1).ToString();
                    Panel pnlDH = new Panel();
                    pnlDH.ID = "workspace_" + val;
                    pnlDH.CssClass = "workspace-holder";
                    pnlDH.Attributes["data-backgroundimg"] = "";
                    workspace_holder.Controls.Add(pnlDH);
                }
            }
        }
    }
    private void LoadUserBackground(HtmlGenericControl site_mainbody) {
        #region Set Data-Backgrounds Attribute
        string nullWorkspace = string.Empty;
        for (int i = 1; i <= _totalNumberWorkspaces; i++) {
            string img = string.Empty;
            if (CurrentUserMemberDatabase != null) {
                img = CurrentUserMemberDatabase.GetBackgroundImg(i);
            }
            else if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0) {
                img = _demoCustomizations.GetBackgroundImg(i);
            }

            Panel workspace = Getworkspace_BG(i);

            List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

            string dataBgImg = "";
            foreach (string bg in backgroundList) {
                if (!string.IsNullOrEmpty(bg)) {
                    if (bg.Length == 6) {
                        dataBgImg += "#" + bg + MemberDatabase.BackgroundSeperator;
                    }
                    else if (bg.StartsWith("#") && bg.Length == 7) {
                        dataBgImg += bg + MemberDatabase.BackgroundSeperator;
                    }
                    else {
                        if (AcctSettings.IsValidHttpUri(bg)) {
                            dataBgImg += bg + MemberDatabase.BackgroundSeperator;
                        }
                        else {
                            dataBgImg += Page.ResolveUrl("~/" + bg) + MemberDatabase.BackgroundSeperator;
                        }
                    }
                }
                else {
                    dataBgImg += Page.ResolveUrl("~/App_Themes/" + CurrentSiteTheme + "/Body/default-bg.jpg") + MemberDatabase.BackgroundSeperator;
                }
            }

            if (workspace != null) {
                workspace.Attributes["data-backgroundimg"] = dataBgImg;
            }
            else {
                nullWorkspace = dataBgImg;
                if (i == 1) {
                    break;
                }
            }
        }
        #endregion

        #region Setup Main Background
        if (site_mainbody != null) {
            string img = nullWorkspace;

            Panel currentWorkspacePanel = Getworkspace_BG(_currentWorkspace);
            if (currentWorkspacePanel != null) {
                img = currentWorkspacePanel.Attributes["data-backgroundimg"];
            }
            if (!string.IsNullOrEmpty(img)) {
                List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (backgroundList.Count > 0) {
                    img = backgroundList[0];
                }

                if (img.Length == 6) {
                    site_mainbody.Style["background-image"] = "";
                    site_mainbody.Style["background-color"] = "#" + img;
                }
                else if (img.StartsWith("#") && img.Length == 7) {
                    site_mainbody.Style["background-image"] = "";
                    site_mainbody.Style["background-color"] = img;
                }
                else {
                    site_mainbody.Style["background-color"] = _backgroundColor;
                    if (AcctSettings.IsValidHttpUri(img) || img.StartsWith(Request.ApplicationPath)) {
                        site_mainbody.Style["background-image"] = "url('" + img + "')";
                    }
                    else {
                        site_mainbody.Style["background-image"] = "url('" + Page.ResolveUrl("~/" + img) + "')";
                    }
                }
            }
            else {
                site_mainbody.Style["background-image"] = "url('" + Page.ResolveUrl("~/App_Themes/" + CurrentSiteTheme + "/Body/default-bg.jpg") + "')";
                site_mainbody.Style["background-color"] = _backgroundColor;
            }

            site_mainbody.Style["background-size"] = _backgroundSize;
            site_mainbody.Style["background-repeat"] = _backgroundRepeat;
            site_mainbody.Style["background-position"] = _backgroundPosition;
        }
        #endregion
    }
    private Panel Getworkspace_BG(int workspaceNumber) {
        Control masterPage = BasePage.GetMasterPageControl(Page);
        Panel d = (Panel)masterPage.FindControl("MainContent").FindControl("workspace_" + workspaceNumber);
        if (d == null) {
            d = (Panel)masterPage.FindControl("MainContent").FindControl("workspace_1");
        }
        return d;
    }
    private void BuildWorkspaceddl(Control masterPage) {
        Panel ddl_WorkspaceSelector = (Panel)masterPage.FindControl("ddl_WorkspaceSelector");
        if (ddl_WorkspaceSelector != null) {
            StringBuilder strWorkspaces = new StringBuilder();
            ddl_WorkspaceSelector.Controls.Clear();

            for (int i = 0; i < _totalNumberWorkspaces; i++) {
                string val = (i + 1).ToString();
                string workspaceSelect = string.Format("<li class='workspace-selection-item' data-number='{0}'><span class='workspace-selection-item-span'>{0}</span></li>", val);
                strWorkspaces.Append(workspaceSelect);
            }

            if (!string.IsNullOrEmpty(strWorkspaces.ToString())) {
                ddl_WorkspaceSelector.Controls.Add(new LiteralControl("<div class='dropdown-db-selector'><ul>" + strWorkspaces.ToString() + "</ul></div>"));
            }
        }
    }

    private void SetNoLoginRequiredVar() {
        _noLoginRequired = MainServerSettings.NoLoginRequired;
        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["Demo"])) {
            _noLoginRequired = true;
        }
    }
    public static void SetHelpIcon(Page page) {
        if (page != null && page.Request != null && !page.IsPostBack) {
            SaveControls sc = new SaveControls();
            if (!HelperMethods.DoesPageContainStr("default.aspx") && sc.GetTotalHelpPages(page.Request.Url.AbsoluteUri) == "0") {
                RegisterPostbackScripts.RegisterStartupScript(page, "$('.help-icon').remove();");
            }
        }
    }

    protected void ResetCurrentAppIconBuilderObject() {
        _appIconBuilder = null;
    }

    #endregion

    #region Shortcut Methods

    public static void RegisterAutoScripts(Page page) {
        HiddenField hf_UpdateAll = null;
        if (page != null) {
            hf_UpdateAll = (HiddenField)BasePage.GetMasterPageControl(page).FindControl("hf_UpdateAll");
            if (hf_UpdateAll != null) {
                string apps = "workspace,appremote";
                if (HelperMethods.DoesPageContainStr("default.aspx")) {
                    apps += ",workspace-main";
                }

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, apps, page);
                aus.StartAutoUpdates();
            }
        }
    }
    public static void RegisterViewPorts(Page page) {
        if (HelperMethods.IsMobileDevice && page.Header != null) {
            HtmlMeta viewPortMeta = new HtmlMeta();
            viewPortMeta.Name = "viewport";
            viewPortMeta.ID = "mobileViewport";
            viewPortMeta.Content = "initial-scale=1.0, user-scalable=no";
            page.Header.Controls.Add(viewPortMeta);
            RegisterPostbackScripts.RegisterStartupScript(page, "$(document).tooltip({ disabled: true });");
        }
    }
    public static void RecordPageViews(Page page) {
        if (page != null && page.Request != null) {
            ServerSettings ss = new ServerSettings();
            if (!page.IsPostBack && ss.RecordPageViews) {
                string tempName = ServerSettings.GuestUsername;
                if (page.User != null && page.User.Identity != null && page.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(page.User.Identity.Name)) {
                    tempName = page.User.Identity.Name;
                }

                if (!string.IsNullOrEmpty(tempName)) {
                    PageViews pageViews = new PageViews();
                    pageViews.AddItem(BasePage.GetCurrentPageIpAddress(page.Request), tempName, page.ToString());
                }
            }
        }
    }
    public static void SetTopLogoTags(Page page, HtmlControl topControl, Dictionary<string, string> groupInformation) {
        if (page != null && topControl != null) {
            string topLogoImg = page.ResolveUrl("~/Standard_Images/logo.png");
            string siteName = ServerSettings.SiteName;

            if (groupInformation != null && groupInformation.Count > 0 && groupInformation.ContainsKey("GroupName") && groupInformation.ContainsKey("Image") && groupInformation.ContainsKey("IsURL")) {
                siteName = groupInformation["GroupName"];

                string imgUrl = string.Empty;
                if (HelperMethods.ConvertBitToBoolean(groupInformation["IsURL"])) {
                    imgUrl = groupInformation["Image"];
                    if (imgUrl.StartsWith("~/")) {
                        imgUrl = page.ResolveUrl(imgUrl);
                    }
                }
                else if (!string.IsNullOrEmpty(groupInformation["Image"])) {
                    imgUrl = page.ResolveUrl("~/Standard_Images/Groups/Logo/" + groupInformation["Image"]);
                }

                if (!string.IsNullOrEmpty(imgUrl)) {
                    topLogoImg = imgUrl;
                }
            }

            HtmlGenericControl genericControl = null;
            HtmlAnchor anchorControl = null;
            if (topControl is HtmlGenericControl) {
                genericControl = (HtmlGenericControl)topControl;
            }
            else if (topControl is HtmlAnchor) {
                anchorControl = (HtmlAnchor)topControl;
            }

            string logoAndNameStr = "<span class=\"title-logo\" style=\"background-image: url('{0}');\"></span><span class=\"title-text\">{1}</span><div class=\"clear\"></div>";
            string nameOnlyStr = "<span class=\"title-text\">{0}</span><div class=\"clear\"></div>";

            if (genericControl != null) {
                if (!string.IsNullOrEmpty(topLogoImg)) {
                    genericControl.InnerHtml = "<div class=\"inline-block\">" + string.Format(logoAndNameStr, topLogoImg, siteName) + "</div>";
                }
                else {
                    genericControl.InnerHtml = "<div class=\"inline-block\">" + string.Format(nameOnlyStr, siteName) + "</div>";
                }
            }
            else if (anchorControl != null) {
                if (!string.IsNullOrEmpty(topLogoImg)) {
                    anchorControl.InnerHtml = string.Format(logoAndNameStr, topLogoImg, siteName);
                }
                else {
                    anchorControl.InnerHtml = string.Format(nameOnlyStr, siteName);
                }
            }
        }
    }
    public static void BuildLinks(Panel pnlLinkBtns, string currUser, Page thisPage) {
        if (pnlLinkBtns != null && thisPage != null) {
            StringBuilder str = new StringBuilder();
            str.Append("<div class='clear'></div><ul class='sitemenu-selection'>");

            SiteMapNodeCollection nodes = SiteMap.CurrentNode.ChildNodes;
            if (nodes.Count == 0) {
                nodes = SiteMap.CurrentNode.ParentNode.ChildNodes;
            }

            int totalFound = 0;
            for (int i = 0; i < nodes.Count; i++) {
                if (!SideBarItems.CanCreateTabLink(nodes[i], currUser)) {
                    continue;
                }

                string url = nodes[i].Url;

                if (HelperMethods.DoesPageContainStr("appmanager.aspx")) {
                    url = url.Substring(url.LastIndexOf("/") + 1);
                }
                else {
                    string[] splitUrl = nodes[i].Url.Split(new string[] { "tab=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitUrl.Length > 0) {
                        url = "#?tab=" + splitUrl[1];
                    }
                    else {
                        url = "#?tab=" + splitUrl[0];
                    }
                }

                if (HelperMethods.ConvertBitToBoolean(thisPage.Request.QueryString["iframeMode"])) {
                    url += "&iframeMode=true";
                }

                str.Append("<li><a href='" + url + "'>" + nodes[i].Title + "</a></li>");
                totalFound++;
            }

            str.Append("</ul><div class='clear'></div>");

            if (totalFound == 0) {
                str.Clear();
            }

            pnlLinkBtns.Controls.Clear();
            pnlLinkBtns.Controls.Add(new LiteralControl(str.ToString()));

            RegisterPostbackScripts.RegisterStartupScript(thisPage, "$(document).ready(function () { openWSE.InitializeSiteMenuTabs(); });");
        }
    }
    public static string GetPostBackControlId(Page page) {
        string _asyncPostBackSourceElementID = string.Empty;
        using (ScriptManager sm = ScriptManager.GetCurrent(page)) {
            if (sm != null) {
                _asyncPostBackSourceElementID = sm.AsyncPostBackSourceElementID;
                if (_asyncPostBackSourceElementID == null) {
                    _asyncPostBackSourceElementID = string.Empty;
                }
            }
        }

        if (string.IsNullOrEmpty(_asyncPostBackSourceElementID) && page.Request != null && page.Request.Params != null && page.Request.Params["__EVENTTARGET"] != null) {
            _asyncPostBackSourceElementID = page.Request.Params["__EVENTTARGET"];
        }

        return _asyncPostBackSourceElementID;
    }
    public static Dictionary<string, string> GetGroupInformation(string username) {
        Dictionary<string, string> groupInformation = new Dictionary<string, string>();
        try {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(username);
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    groupInformation = tempGroup.group_dt[0];
                }
                else {
                    GroupSessions.RemoveGroupLoginSession(username);
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return groupInformation;
    }
    public static void SetNotificationPopup(Page page, int totalMessages, List<UserNotificationsMessage_Coll> messageList, string siteTheme) {
        try {
            Control mainControl = page;
            if (page.Master != null) {
                mainControl = page.Master;
            }

            UpdatePanel MainContent_updatepnl_notificationpopup = (UpdatePanel)mainControl.FindControl("MainContent_updatepnl_notificationpopup");
            Label lbl_noti_update_popup_CurrCount = (Label)mainControl.FindControl("lbl_noti_update_popup_CurrCount");
            HiddenField hf_noti_update_hiddenField = (HiddenField)mainControl.FindControl("hf_noti_update_hiddenField");
            Image img_noti_update_image = (Image)mainControl.FindControl("img_noti_update_image");
            Label lbl_noti_update_message = (Label)mainControl.FindControl("lbl_noti_update_message");
            Label lbl_noti_update_popup_Description = (Label)mainControl.FindControl("lbl_noti_update_popup_Description");

            int tempCurrCount = 0;
            int.TryParse(lbl_noti_update_popup_CurrCount.Text, out tempCurrCount);

            if (totalMessages > 0 && totalMessages > tempCurrCount && page.IsPostBack) {
                UserNotificationsMessage_Coll x = messageList[0];
                hf_noti_update_hiddenField.Value = "true";
                lbl_noti_update_message.Text = UserNotifications.GetNotificationName(x.NotificationID);
                lbl_noti_update_popup_Description.Text = UserNotifications.TrimBrowserPopupMessage(x.Message);
                img_noti_update_image.ImageUrl = UserNotifications.GetNotificationIcon(x.NotificationID, siteTheme);
            }

            lbl_noti_update_popup_CurrCount.Text = totalMessages.ToString();
            MainContent_updatepnl_notificationpopup.Update();
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }
    }

    #endregion

    #region User Sign Off

    public static void GroupSignOff(string userName) {
        if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Response != null) {
            try {
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(userName)) {
                    LoginActivity la = new LoginActivity();
                    la.AddItem(userName, true, ActivityType.Logout);
                    GroupSessions.RemoveGroupLoginSession(userName);
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            if (HttpContext.Current != null && HttpContext.Current.Request != null) {
                HelperMethods.PageRedirect(HttpContext.Current.Request.Url.OriginalString);
            }
        }
    }
    public static void SignUserOff(string userName) {
        if (string.IsNullOrEmpty(userName)) {
            return;
        }

        if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Response != null) {
            HttpRequest tempCurrentRequest = HttpContext.Current.Request;
            HttpResponse tempCurrentResponse = HttpContext.Current.Response;

            MemberDatabase currentMember = new MemberDatabase(userName);
            Thread.Sleep(500);

            try {
                if (currentMember.ClearPropOnSignOff) {
                    App appObject = new App(userName);
                    appObject.DeleteUserProperties(userName);

                    HttpCookieCollection cookieColl = new HttpCookieCollection();
                    foreach (object key in tempCurrentRequest.Cookies.Keys) {
                        HttpCookie cookie = tempCurrentRequest.Cookies[key.ToString()];
                        if (cookie != null) {
                            cookie.Expires = ServerSettings.ServerDateTime.AddDays(-1d);
                            cookieColl.Add(cookie);
                        }
                    }

                    foreach (object c in cookieColl.Keys) {
                        HttpCookie cookie = tempCurrentRequest.Cookies[c.ToString()];
                        tempCurrentResponse.Cookies.Add(cookie);
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            int hour = ServerSettings.ServerDateTime.Hour;
            int min = ServerSettings.ServerDateTime.Minute;
            int seconds = ServerSettings.ServerDateTime.Second;

            if (min >= 20) {
                min = min - 20;
            }
            else {
                int tempmin = min - 20;
                min = 60 + tempmin;
                if (hour > 1) {
                    hour = hour - 1;
                }
                else {
                    hour = 24 - hour - 1;
                }
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
                DateTime newDate = new DateTime(ServerSettings.ServerDateTime.Year, month, day, hour, min, seconds);
                MembershipUser membershipuser = Membership.GetUser(userName);
                if (membershipuser != null) {
                    membershipuser.LastActivityDate = newDate;
                    if (!string.IsNullOrEmpty(membershipuser.Email))
                        Membership.UpdateUser(membershipuser);
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            currentMember.UpdateChatTimeStamp();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser msu in coll) {
                string un = msu.UserName.ToLower();
                if (msu.IsOnline && un != userName.ToLower()) {
                    if (ChatService._needRefreshUsers.ContainsKey(un)) {
                        ChatService._needRefreshUsers[un] = true;
                    }
                    else {
                        ChatService._needRefreshUsers.Add(un, true);
                    }

                    if (ChatService._needRefreshMessager.ContainsKey(un)) {
                        ChatService._needRefreshMessager[un] = true;
                    }
                    else {
                        ChatService._needRefreshMessager.Add(un, true);
                    }
                }
            }

            try {
                MemberDatabase.DeleteUserSessionId(userName);
                FormsAuthentication.SignOut();

                if (tempCurrentRequest.RawUrl.ToLower().Contains("rssfeed.aspx")) {
                    HelperMethods.PageRedirect("~/Apps/RSSFeed/RSSFeed.aspx");
                }
                else {
                    string defaultLoginPage = ServerSettings.DefaultStartupPage;
                    string loginPageQuery = string.Empty;

                    if (GroupSessions.DoesUserHaveGroupLoginSessionKey(userName)) {
                        string groupName = GroupSessions.GetUserGroupSessionName(userName);
                        GroupSessions.RemoveGroupLoginSession(userName);
                        if (!string.IsNullOrEmpty(groupName)) {
                            loginPageQuery = "?group=" + groupName;
                        }
                    }

                    LoginActivity la = new LoginActivity();
                    la.AddItem(userName, true, ActivityType.Logout);

                    if (HelperMethods.ConvertBitToBoolean(tempCurrentRequest.QueryString["mobileMode"]) || tempCurrentRequest.RawUrl.ToLower().Contains("appremote.aspx")) {
                        defaultLoginPage = "AppRemote.aspx";
                    }

                    HelperMethods.PageRedirect(string.Format("~/{0}{1}", defaultLoginPage, loginPageQuery));
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    #endregion

    #region Dynamically Load Scripts

    public void LoadAllDefaultScriptsStyleSheets(Page page) {
        GetStartupScripts_JS(page);
        GetStartupScripts_CSS(page);
        LoadDefaultScriptFiles(page);

        if (CurrentUserMemberDatabase != null) {
            CustomFonts.SetCustomValues(page, CurrentUserMemberDatabase);
        }
        else if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0) {
            CustomFonts.SetCustomValues(page, _demoCustomizations.DefaultTable);
        }
        else {
            CustomFonts.SetCustomValues(page);
        }
    }
    public void LoadAllDefaultScriptsStyleSheets(Page page, MemberDatabase member) {
        _currentUserMemberDatabase = member;
        _currentUserMembership = Membership.GetUser(member.Username);

        GetStartupScripts_JS(page);
        GetStartupScripts_CSS(page);
        LoadDefaultScriptFiles(page);

        CustomFonts.SetCustomValues(page, member);
    }
    public void LoadAllDefaultScriptsStyleSheets(Page page, NewUserDefaults userDefaults) {
        _demoCustomizations = userDefaults;

        GetStartupScripts_JS(page);
        GetStartupScripts_CSS(page);
        LoadDefaultScriptFiles(page);

        CustomFonts.SetCustomValues(page, userDefaults.DefaultTable);
    }

    private void GetStartupScripts_JS(Page page) {
        StartupScripts startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(page);

        if (sm != null) {
            foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
                if (coll.ApplyTo == "Base/Workspace" || (coll.ApplyTo == "Workspace Only" && !HelperMethods.DoesPageContainStr("AppRemote")) || coll.ApplyTo == "All Components" || (coll.ApplyTo == "App Remote" && HelperMethods.DoesPageContainStr("AppRemote"))) {
                    ScriptReference sref = new ScriptReference(coll.ScriptPath);
                    sm.Scripts.Add(sref);
                }
                else if (coll.ApplyTo == "Chat Client" && (HelperMethods.DoesPageContainStr("AppRemote") || page.Master != null) && (string.IsNullOrEmpty(page.Request.QueryString["mobileMode"]) || !HelperMethods.ConvertBitToBoolean(page.Request.QueryString["mobileMode"]))) {
                    EnableDisableChat(page, coll.ScriptPath);
                }
            }

            sm.ScriptMode = ScriptMode.Release;
        }
    }
    private void GetStartupScripts_CSS(Page page) {
        StartupStyleSheets startupscripts = new StartupStyleSheets(true);
        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            string scriptTheme = coll.Theme;
            if (scriptTheme == CurrentSiteTheme || scriptTheme == "All") {
                if (coll.ApplyTo == "Base/Workspace" || string.IsNullOrEmpty(coll.ApplyTo) || (coll.ApplyTo == "Workspace Only" && !HelperMethods.DoesPageContainStr("AppRemote")) || coll.ApplyTo == "Chat Client" || coll.ApplyTo == "All Components" || (coll.ApplyTo == "App Remote" && HelperMethods.DoesPageContainStr("AppRemote"))) {
                    startupscripts.AddCssToPage(coll.ScriptPath, page);
                }
                else {
                    if (CurrentUserMemberDatabase != null) {
                        if (CurrentUserMemberDatabase.UserHasApp(coll.ApplyTo)) {
                            startupscripts.AddCssToPage(coll.ScriptPath, page);
                        }
                    }
                    else {
                        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.Count > 0 && _demoCustomizations.DefaultTable.ContainsKey("AppPackage")) {
                            AppPackages package = new AppPackages(false);
                            string[] appList = package.GetAppList(_demoCustomizations.DefaultTable["AppPackage"]);
                            foreach (string x in appList) {
                                if (x == coll.ApplyTo) {
                                    startupscripts.AddCssToPage(coll.ScriptPath, page);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void EnableDisableChat(Page page, string script) {
        if (page != null) {
            ScriptManager sm = ScriptManager.GetCurrent(page);

            bool turnOffChat = false;
            if (sm != null) {
                if (MainServerSettings.ChatEnabled) {
                    if (CurrentUserMemberDatabase != null && CurrentUserMemberDatabase.ChatEnabled) {
                        HiddenField hf_chatsound = (HiddenField)BasePage.GetMasterPageControl(page).FindControl("hf_chatsound");
                        if (hf_chatsound != null) {
                            hf_chatsound.Value = CurrentUserMemberDatabase.ChatSoundNoti.ToString().ToLower();
                        }

                        var sref = new ScriptReference(script);
                        sm.Scripts.Add(sref);

                        if (CurrentUserMemberDatabase.IsAway) {
                            ChatService cs = new ChatService();
                            cs.UpdateStatus("Available");
                        }
                    }
                    else {
                        turnOffChat = true;
                    }
                }
                else {
                    turnOffChat = true;
                }
            }

            if (turnOffChat) {
                if (HelperMethods.DoesPageContainStr("appremote.aspx", page)) {
                    HtmlGenericControl pnlContent_Chat = (HtmlGenericControl)BasePage.GetMasterPageControl(page).FindControl("pnlContent_Chat");
                    if (pnlContent_Chat != null) {
                        pnlContent_Chat.Visible = false;
                    }
                    HtmlGenericControl pnlContent_ChatPopup = (HtmlGenericControl)BasePage.GetMasterPageControl(page).FindControl("pnlContent_ChatPopup");
                    if (pnlContent_ChatPopup != null) {
                        pnlContent_ChatPopup.Visible = false;
                    }
                    HtmlAnchor Chat_tab = (HtmlAnchor)BasePage.GetMasterPageControl(page).FindControl("Chat_tab");
                    if (Chat_tab != null) {
                        Chat_tab.Visible = false;
                    }
                }
                else {
                    Panel pnl_chat_users = (Panel)BasePage.GetMasterPageControl(page).FindControl("pnl_chat_users");
                    if (pnl_chat_users != null) {
                        pnl_chat_users.Enabled = false;
                        pnl_chat_users.Visible = false;
                    }
                }
            }
        }
    }

    public void LoadDefaultScriptFiles(Page page, string siteTheme = "") {
        if (page != null) {
            if (string.IsNullOrEmpty(siteTheme)) {
                siteTheme = CurrentSiteTheme;
            }

            if (string.IsNullOrEmpty(siteTheme)) {
                siteTheme = "Standard";
            }

            string pageName = page.ToString().ToLower().Replace("asp.", string.Empty).Replace("_aspx", string.Empty);

            string[] pageParts = pageName.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (pageParts.Length > 1) {
                pageName = pageParts[pageParts.Length - 1];
            }

            bool appendTimestamp = MainServerSettings.AppendTimestampOnScripts;
            StartupStyleSheets startupStyleSheets = new StartupStyleSheets(false);

            string serverPath = ServerSettings.GetServerMapLocation;
            if (!string.IsNullOrEmpty(pageName) && pageName != "default") {

                // Load .css file if available
                if (File.Exists(serverPath + "App_Themes\\" + siteTheme + "\\StyleSheets\\SiteTools\\" + pageName + ".css")) {
                    string cssFilePath = AppendTimeStampForScript("~/App_Themes/" + siteTheme + "/StyleSheets/SiteTools/" + pageName + ".css", appendTimestamp);
                    startupStyleSheets.AddCssToPage(cssFilePath, page);
                }

                // Load .js file if available
                if (File.Exists(serverPath + "Scripts\\SiteTools\\" + pageName + ".js")) {
                    ScriptManager sm = ScriptManager.GetCurrent(page);
                    if (sm != null) {
                        string jsFilePath = AppendTimeStampForScript("~/Scripts/SiteTools/" + pageName + ".js", appendTimestamp);
                        ScriptReference sref = new ScriptReference(jsFilePath);
                        sm.Scripts.Add(sref);
                    }
                }
            }

            if (Directory.Exists(serverPath + "App_Themes\\" + siteTheme + "\\StyleSheets\\Main\\")) {
                string[] cssFileList = Directory.GetFiles(serverPath + "App_Themes\\" + siteTheme + "\\StyleSheets\\Main\\");
                foreach (string cssFile in cssFileList) {
                    FileInfo fi = new FileInfo(cssFile);
                    string cssFilePath = AppendTimeStampForScript("~/App_Themes/" + siteTheme + "/StyleSheets/Main/" + fi.Name, appendTimestamp);
                    startupStyleSheets.AddCssToPage(cssFilePath, page);
                }
            }
        }
    }
    private static string AppendTimeStampForScript(string scriptPath, bool appendTimestamp) {
        if (appendTimestamp) {
            string querySeperator = "?";
            if (scriptPath.Contains("?")) {
                querySeperator = "&";
            }

            scriptPath += string.Format("{0}{1}{2}", querySeperator, ServerSettings.TimestampQuery, HelperMethods.GetTimestamp());
        }

        return scriptPath;
    }

    #endregion

    #region Page Valid Methods

    public static bool IsPageValid(Page page, bool noLoginRequired) {
        BaseMaster.CheckSSLRedirect(page);

        if (!ServerSettings.CheckWebConfigFile) {
            return false;
        }

        if (BaseMaster.CheckIpListenerWatchlist(page, noLoginRequired)) {
            return true;
        }

        return false;
    }
    public static void CheckSSLRedirect(Page page) {
        if (page.Request != null && !HelperMethods.ConvertBitToBoolean(page.Request.QueryString["iframe"])) {
            if (!page.Request.IsLocal && !page.Request.IsSecureConnection && !page.IsPostBack) {
                Uri httpType = page.Request.Url;
                ServerSettings ss = new ServerSettings();

                if (ss.SSLRedirect) {
                    if (httpType != null) {
                        if (httpType.Scheme.ToLower() == "http") {
                            string redirectUrl = page.Request.Url.ToString().Replace("http:", "https:");

                            string username = string.Empty;
                            if (page.User != null && page.User.Identity != null && page.User.Identity.IsAuthenticated) {
                                username = page.User.Identity.Name;
                            }

                            if (HelperMethods.UrlIsValid(redirectUrl, username))
                                HelperMethods.PageRedirect(redirectUrl);
                        }
                    }
                }
            }
        }
    }
    private static bool CheckIpListenerWatchlist(Page page, bool noLoginRequired) {
        bool isValid = false;
        if (page == null || page.Request == null || page.Response == null) {
            return isValid;
        }

        IPWatch _ipwatch = new IPWatch(false);
        IPListener _listener = new IPListener(false);
        string _ipAddress = BasePage.GetCurrentPageIpAddress(page.Request);
        ServerSettings ss = new ServerSettings();

        string username = string.Empty;
        bool userIsAuthenticated = false;
        if (page.User != null && page.User.Identity != null && page.User.Identity.IsAuthenticated) {
            username = page.User.Identity.Name;
            userIsAuthenticated = page.User.Identity.IsAuthenticated;
        }

        if (_ipwatch.CheckIfBlocked(_ipAddress) && !BasePage.IsUserNameEqualToAdmin(username)) {
            if (string.IsNullOrEmpty(username)) {
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
            }
            else {
                HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
            }
        }
        else if (ss.SiteOffLine && !BasePage.IsUserNameEqualToAdmin(username)) {
            if (noLoginRequired && !userIsAuthenticated) {
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
            }
            else {
                HelperMethods.PageRedirect("~/ErrorPages/Maintenance.html");
            }
        }
        else {
            if (!_listener.TableEmpty) {
                if (_listener.CheckIfActive(_ipAddress)) {
                    isValid = true;
                }
                else if (string.IsNullOrEmpty(username)) {
                    HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
                }
                else if (!BasePage.IsUserNameEqualToAdmin(username)) {
                    HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
                }
                else {
                    isValid = true;
                }
            }
            else {
                isValid = true;
            }
        }

        return isValid;
    }

    #endregion

}
