using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OpenWSE_Tools.GroupOrganizer;
using SocialSignInApis;

/// <summary>
/// Summary description for BasePage
/// </summary>
public class BasePage : Page {

    #region Private Properties

    private bool PageIsLoaded = false;
    private string _currentUsername = string.Empty;
    private MemberDatabase _currentUserMemberDatabase = null;
    private MembershipUser _currentUserMembership = null;
    private NewUserDefaults _demoCustomizations = null;
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

    #endregion

    #region Load Methods

    protected override void OnInit(EventArgs e) {
        LoadPageValues();
        base.OnInit(e);
    }
    protected override void OnLoad(EventArgs e) {
        ServerSettings.AddMetaTagsToPage(Page);
        LoadPageValues();
        base.OnLoad(e);
    }
    protected override void OnPreInit(EventArgs e) {
        base.OnPreInit(e);
    }
    protected override void OnUnload(EventArgs e) {
        base.OnUnload(e);
    }

    private void LoadPageValues() {
        if (!PageIsLoaded) {
            if (HelperMethods.DoesPageContainStr("default.aspx")) {
                InitializeWorkspacePage();
            }
            else if (HelperMethods.DoesPageContainStr("appremote.aspx")) {
                InitializeAppRemote();
            }
            else {
                InitializeSiteSettingsPage();
            }

            PageIsLoaded = true;
        }
    }

    #endregion

    #region Initialize Base

    private void InitializeWorkspacePage() {
        bool NoLoginRequired = CheckIfDemo();
        if (NoLoginRequired && !UserIsAuthenticated) {
            InitializeNewUserDefaults();
        }
        else if (!NoLoginRequired && !UserIsAuthenticated) {
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if (UserIsAuthenticated && IsUserNameEqualToAdmin()) {
            HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
        }
        else if (UserIsAuthenticated) {
            InitializeCurrentUser();
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Error.html");
        }

        #region Set BackToWorkspace link
        if (Master != null) {
            HtmlAnchor lnk_BackToWorkspace = (HtmlAnchor)GetMasterPageControl(Page).FindControl("lnk_BackToWorkspace");
            if (lnk_BackToWorkspace != null) {
                lnk_BackToWorkspace.HRef = string.Empty;
                lnk_BackToWorkspace.Title = string.Empty;
                lnk_BackToWorkspace.Attributes["onclick"] = "return false";
                lnk_BackToWorkspace.Style["cursor"] = "default!important";
            }
        }
        #endregion

        CacheWorkspace();
    }
    private void InitializeSiteSettingsPage() {
        if (!UserIsAuthenticated) {
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (!ServerSettings.AdminPagesCheck(Page.ToString(), CurrentUsername) && !IsUserNameEqualToAdmin()) {
                HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
            }
        }

        #region Setup Page TD Settings Title (If available)
        ContentPlaceHolder MainContent = (ContentPlaceHolder)GetMasterPageControl(Page).FindControl("MainContent");
        if (MainContent != null) {
            HtmlGenericControl pageTdSettingsTitle = (HtmlGenericControl)MainContent.FindControl("pageTdSettingsTitle");
            if (pageTdSettingsTitle != null) {
                SiteMapNode currNode = SiteMap.CurrentNode;
                if (currNode != null) {
                    if (currNode.Url.ToLower().Contains("?tab=") || currNode.Url.ToLower().Contains("&tab=")) {
                        currNode = currNode.ParentNode;
                    }

                    if (!string.IsNullOrEmpty(currNode.Title)) {
                        pageTdSettingsTitle.InnerHtml = currNode.Title;
                        if (currNode["icon"] != null && !string.IsNullOrEmpty(currNode["icon"])) {
                            string iconUrl = currNode["icon"];
                            if (iconUrl.StartsWith("~/")) {
                                iconUrl = ServerSettings.ResolveUrl(iconUrl);
                            }
                            if (iconUrl.Contains("/[THEME]/")) {
                                iconUrl = iconUrl.Replace("/[THEME]/", "/" + CurrentSiteTheme + "/");
                            }

                            pageTdSettingsTitle.Attributes["data-customimage"] = "true";
                            pageTdSettingsTitle.InnerHtml += "<img alt='' class='custom-image-title' src='" + iconUrl + "' />";
                        }
                    }
                }
            }
        }
        #endregion
    }

    #endregion

    #region Setup Workspace Variables

    private bool CheckIfDemo() {
        bool noLoginRequired = MainServerSettings.NoLoginRequired;
        if (HelperMethods.ConvertBitToBoolean(CurrentRequest.QueryString["Demo"]) && MainServerSettings.ShowPreviewButtonLogin) {
            CheckUpdatesPopup_Demo();
            noLoginRequired = true;
            if (Session != null) {
                Session["DemoMode"] = "true";
            }
        }
        else if (Session != null && Session["DemoMode"] != null && HelperMethods.ConvertBitToBoolean(Session["DemoMode"].ToString())) {
            Session.Remove("DemoMode");
        }

        return noLoginRequired;
    }
    private void InitializeNewUserDefaults() {
        _demoCustomizations = new NewUserDefaults("DemoNoLogin");
        _demoCustomizations.GetDefaults();
    }
    private void InitializeCurrentUser() {
        CheckIfAdminPageEnabled();
    }
    private void CheckIfAdminPageEnabled() {
        if (!IsPostBack) {
            if (!ServerSettings.AdminPagesCheck(Page.ToString(), CurrentUsername)) {
                string[] adminPages = CurrentUserMemberDatabase.AdminPagesList;
                foreach (string adminpage in adminPages) {
                    foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes) {
                        string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                        string[] urlSplit = url.Split('.');
                        if (urlSplit.Length != 2) {
                            continue;
                        }

                        if (urlSplit[0].ToLower() == adminpage.ToLower()) {
                            HelperMethods.PageRedirect(node.Url);
                        }
                    }
                }

                HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
            }
        }
    }
    private void CheckUpdatesPopup_Demo() {
        if (CurrentRequest != null && string.IsNullOrEmpty(CurrentRequest.QueryString["AppPage"])) {
            ShowUpdatePopup sup = new ShowUpdatePopup();
            string message = sup.GetNewUpdateMessage(ServerSettings.GetServerMapLocation, CurrentSiteTheme, false);
            string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
            RegisterPostbackScripts.RegisterStartupScript(Page, string.Format("openWSE.ShowUpdatesPopup('{0}');", encodedMessage));
        }
    }
    private void CacheWorkspace() {
        if (!IsPostBack) {
            if (MainServerSettings.CacheHomePage) {
                //CurrentResponse.Cache.SetAllowResponseInBrowserHistory(true);
                CurrentResponse.Cache.SetExpires(DateTime.UtcNow.AddMinutes(60));
                CurrentResponse.Cache.SetCacheability(HttpCacheability.Private);
                CurrentResponse.Cache.SetValidUntilExpires(true);
            }
            else {
                CurrentResponse.Cache.SetAllowResponseInBrowserHistory(false);
                CurrentResponse.Cache.SetCacheability(HttpCacheability.NoCache);
                CurrentResponse.Cache.SetNoStore();
                CurrentResponse.Cache.SetExpires(ServerSettings.ServerDateTime.AddSeconds(60));
                CurrentResponse.Cache.SetValidUntilExpires(true);
            }
        }
    }

    #endregion

    #region Shortcut Methods

    protected bool IsUserInAdminRole() {
        if (!string.IsNullOrEmpty(CurrentUsername)) {
            return Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName);
        }

        return false;
    }
    public static bool IsUserInAdminRole(string username) {
        if (!string.IsNullOrEmpty(username)) {
            return Roles.IsUserInRole(username, ServerSettings.AdminUserName);
        }

        return false;
    }

    protected bool IsUserNameEqualToAdmin() {
        if (!string.IsNullOrEmpty(CurrentUsername) && CurrentUsername.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            return true;
        }

        return false;
    }
    public static bool IsUserNameEqualToAdmin(string username) {
        if (!string.IsNullOrEmpty(username) && username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            return true;
        }

        return false;
    }

    protected bool PostbackControlIsUpdateAll() {
        return PostbackControlContainsString("hf_UpdateAll") || PostbackControlContainsString("hf_SearchSite");
    }
    protected bool PostbackControlContainsString(string controlName) {
        if (!string.IsNullOrEmpty(controlName) && AsyncPostBackSourceElementID.ToLower().Contains(controlName.ToLower())) {
            return true;
        }

        return false;
    }

    public static string GetCurrentPageIpAddress(HttpRequest request) {
        string ipAddress = "127.0.0.1";

        if (request != null && request.ServerVariables != null && request.ServerVariables["REMOTE_ADDR"] != null) {
            ipAddress = request.ServerVariables["REMOTE_ADDR"];
            if (ipAddress == "::1") {
                ipAddress = "127.0.0.1";
            }
        }

        return ipAddress;
    }
    public static Control GetMasterPageControl(Page thisPage) {
        if (thisPage.Master != null) {
            return thisPage.Master;
        }
        return thisPage;
    }

    #endregion

    #region Setup AppRemote Page

    private void InitializeAppRemote() {
        ServerSettings.AddMetaTagsToPage(Page);

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();

        BaseMaster.SetHelpIcon(Page);
        BaseMaster baseMaster = new BaseMaster();

        if (UserIsAuthenticated) {
            BaseMaster.RegisterAutoScripts(Page);
        }

        // Register ViewPorts
        BaseMaster.RegisterViewPorts(Page);

        string siteColorOption = "1~;2~";
        bool NoLoginRequired = CheckIfDemo();
        if (NoLoginRequired && !UserIsAuthenticated) {
            InitializeNewUserDefaults();
            baseMaster.LoadAllDefaultScriptsStyleSheets(Page, _demoCustomizations);

            if (_demoCustomizations.DefaultTable != null && _demoCustomizations.DefaultTable.ContainsKey("SiteColorOption") && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["SiteColorOption"])) {
                siteColorOption = _demoCustomizations.DefaultTable["SiteColorOption"];
            }
        }
        else if (UserIsAuthenticated) {
            baseMaster.LoadAllDefaultScriptsStyleSheets(Page, CurrentUserMemberDatabase);
            siteColorOption = CurrentUserMemberDatabase.SiteColorOption;
        }
        else {
            baseMaster.LoadAllDefaultScriptsStyleSheets(Page);
        }

        HtmlGenericControl iframe_title_logo = (HtmlGenericControl)Page.FindControl("iframe_title_logo");
        if (iframe_title_logo != null) {
            Dictionary<string, string> groupInformation = BaseMaster.GetGroupInformation(CurrentUsername);
            if (!string.IsNullOrEmpty(Request.QueryString["group"]) && string.IsNullOrEmpty(CurrentUsername) && !UserIsAuthenticated) {
                Groups group = new Groups();
                group.getEntries(Request.QueryString["group"]);
                if ((group.group_dt != null) && (group.group_dt.Count > 0)) {
                    groupInformation = group.group_dt[0];
                }
            }

            BaseMaster.SetTopLogoTags(Page, iframe_title_logo, groupInformation);
        }

        SetTopbarCustomizations(siteColorOption);

        StringBuilder strScripts = new StringBuilder();
        strScripts.Append("openWSE_Config.siteRootFolder='" + Page.ResolveUrl("~/").Replace("/", "") + "';");
        RegisterPostbackScripts.RegisterStartupScript(Page, strScripts.ToString());
    }
    private void SetTopbarCustomizations(string siteColorOption) {
        string selectedColorIndex = "1";
        string[] siteOptionSplit = siteColorOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
        if (siteOptionSplit.Length >= 1) {
            selectedColorIndex = siteOptionSplit[0];
        }

        HtmlGenericControl div_ColorOptionsHolder = (HtmlGenericControl)Page.FindControl("div_ColorOptionsHolder");
        HtmlGenericControl site_mainbody = (HtmlGenericControl)Page.FindControl("site_mainbody");
        if (site_mainbody != null) {
            site_mainbody.Attributes["data-coloroption"] = selectedColorIndex;
            if (div_ColorOptionsHolder != null) {
                div_ColorOptionsHolder.InnerHtml = HelperMethods.BuildColorOptionList("openWSE.ThemeColorOption_Clicked(this);", "openWSE.ColorOption_Changed(this);", "openWSE.ResetColorOption_Clicked(this);", siteColorOption, Page);
                RegisterPostbackScripts.RegisterStartupScript(Page, "openWSE.InitializeThemeColorOption('div_ColorOptionsHolder');");
            }
            else {
                HelperMethods.BuildColorOptionList(string.Empty, string.Empty, string.Empty, siteColorOption, Page);
            }
        }
    }

    #endregion

}
