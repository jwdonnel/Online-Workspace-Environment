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
    private AppIconBuilder _aib;
    private string _ctlId;
    private Dictionary<string, string> _demoMemberDatabase;
    private MemberDatabase _member;
    private MembershipUser _membershipuser;
    private NewUserDefaults _demoCustomizations;
    private string _username;
    private string _sitetheme = "Standard";
    private bool _noLoginRequired;
    private string _ipAddress;
    private bool AssociateWithGroups = false;
    private bool _onWorkspace = false;
    private bool _inAtLeastOneGroup = false;
    private int _totalWorkspaces = 4;
    private string _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

    #endregion


    #region Authenticate user and load page

    protected void Page_Load(object sender, EventArgs e) {
        ServerSettings.AddMetaTagsToPage(this.Page);

        #region Rename Image Files
        //string[] files = Directory.GetFiles(@"C:\Users\John\Desktop\Website Projects\OpenWSE Code\OpenWSE_SteelOnline_(v4.2)\Standard_Images\Backgrounds");
        //int count = 1;
        //foreach (string file in files) {
        //    FileInfo fi = new FileInfo(file);
        //    fi.MoveTo(@"C:\Users\John\Desktop\Website Projects\OpenWSE Code\OpenWSE_SteelOnline_(v4.2)\Standard_Images\Backgrounds\Background_" + count.ToString() + ".jpg");
        //    count++;
        //}
        #endregion
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
        
        IIdentity userId = HttpContext.Current.User.Identity;

        if (_ss.ForceGroupLogin && !userId.IsAuthenticated) {
            Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if (_ss.ForceGroupLogin && userId.IsAuthenticated && userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower() && !GroupSessions.DoesUserHaveGroupLoginSessionKey(userId.Name)) {
            MemberDatabase tempMember = new MemberDatabase(userId.Name);
            if (tempMember.GetCompleteUserGroupList.Count == 1) {
                GroupSessions.AddOrSetNewGroupLoginSession(userId.Name, tempMember.GroupList[0]);
            }
            else {
                Response.Redirect("~/GroupLogin.aspx");
            }
        }

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();

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

                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);
                if (_inAtLeastOneGroup && (!HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) && (!string.IsNullOrEmpty(pnlId))) {
                    RegisterPostbackScripts.ReInitOverlaysAndApps(this.Page, IsPostBack, AssociateWithGroups, _member, _demoMemberDatabase);
                }
            }
            else if (!userId.IsAuthenticated && ServerSettings.CheckWebConfigFile() 
                && HelperMethods.ConvertBitToBoolean(Request.QueryString["purchase"]) 
                && Request.RawUrl.ToLower().Contains("licensemanager.aspx")) {
                return;
            }
            else if (!CheckLicense.LicenseIsLoaded && !ServerSettings.CheckWebConfigFile() 
                && (CheckLicense.IsTrial || CheckLicense.IsDeveloper || CheckLicense.LicenseValid) 
                && Request.RawUrl.ToLower().Contains("licensemanager.aspx")) {
                return;
            }
            else if (!userId.IsAuthenticated && ServerSettings.CheckWebConfigFile()) {
                Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
            }
        }
        else if (!CheckLicense.LicenseIsLoaded && !ServerSettings.CheckWebConfigFile()
            && (CheckLicense.IsTrial || CheckLicense.IsDeveloper || CheckLicense.LicenseValid)
            && Request.RawUrl.ToLower().Contains("licensemanager.aspx")) {
                return;
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

            if (_ss.AddBackgroundToLogo) {
                _strScriptreg.Append("openWSE.AddBackgroundColorToLogo('" + _ss.LogoBackgroundColor + "');");
            }

            _strScriptreg.Append("openWSE_Config.reportOnError=" + (!_ss.DisableJavascriptErrorAlerts).ToString().ToLower() + ";");
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
            if (_membershipuser == null) {
                FormsAuthentication.SignOut();
                Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
            }

            if ((!userId.IsAuthenticated) || ((_membershipuser.IsLockedOut) || (!_membershipuser.IsApproved))) {
                Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
            }
            else {
                _member = new MemberDatabase(_username);
                _usernoti = new UserNotificationMessages(_username);

                _workspaceMode = _member.WorkspaceMode.ToString();

                if (!_member.IsNewMember) {
                    if (!IsPostBack) {
                        UserIsAuthenticated();
                    }
                    else {
                        _inAtLeastOneGroup = false;
                        Groups groups = new Groups();
                        List<string> ugArray = _member.GroupList;
                        foreach (string g in ugArray) {
                            if (!string.IsNullOrEmpty(groups.GetGroupName_byID(g))) {
                                _inAtLeastOneGroup = true;
                                break;
                            }
                        }
                    }
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString() + "openWSE_Config.minPasswordLength=" + Membership.MinRequiredPasswordLength + ";openWSE.HelpOverlay(true);");
                }
            }
        }
    }
    protected void UserIsAuthenticated() {
        if (_member.AdminPagesList.Length == 0 && Roles.IsUserInRole(_member.Username, "Standard")) {
            FormsAuthentication.SignOut();
            Page.Response.Redirect("~/Default.aspx");
        }

        string acctImg = _member.AccountImage;
        lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(acctImg, _member.UserColor, _member.UserId, HelperMethods.MergeFMLNames(_member), _sitetheme, _member.ProfileLinkStyle);
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

        AssociateWithGroups = _ss.AssociateWithGroups;

        _membershipuser.LastActivityDate = ServerSettings.ServerDateTime;
        Membership.UpdateUser(_membershipuser);

        SetMainLogo();
        SetLoginGroup();
        CheckUpdatesPopup();
        SetCurrentPageTitle();
        LoadUserCustomizations();
        SetAppAndChatSidebarIcons(_member.HideSidebarMenuIcons);

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();
        _aib = new AppIconBuilder(Page, _member);

        if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (!HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"]))) {
            _aib.BuildAppsForUser();
            _inAtLeastOneGroup = _aib.InAtLeastOneGroup;

            GetUserPlugins();

            bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(_workspaceMode);

            if (_inAtLeastOneGroup) {
                string GetPageOverlayBtns = string.Empty;
                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);

                if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
                    if (_member.HideAllOverlays) {
                        GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
                    }
                    else {
                        GetPageOverlayBtns = "openWSE.GetPagedAddOverlayAndModel();";
                    }
                }
                else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
                    pnl_OverlaysAll.Enabled = false;
                    pnl_OverlaysAll.Visible = false;
                }

                if (!string.IsNullOrEmpty(pnlId)) {
                    _strScriptreg.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.CreateOverlayTable();" + GetPageOverlayBtns);
                    string tempUsername = GroupSessions.GetUserGroupSessionName(_username);
                    RefreshOverlays _ro = new RefreshOverlays(tempUsername, ServerSettings.GetServerMapLocation, this.Page, pnlId);
                    _ro.GetWorkspaceOverlays(_aib.UserAppList);
                }
            }

            if (_onWorkspace && isComplexMode) {
                _aib.AppBuilderCaller.LoadSavedChatSessions();
                GetCurrentWorkspace();
            }
            _strScriptreg.Append(_aib.AppBuilderCaller.StrScriptreg);
        }

        CountNotiMessages();
        HideOverlaySettings();

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
            sidebar_menulinks.Visible = false;
            _strScriptreg.Append("openWSE.ToolView();");
        }
        else if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            sidebar_menulinks.Visible = false;
            ct101.Controls.Add(new LiteralControl("<div class='loading-background-holder-mobile'></div>"));
            _strScriptreg.Append("openWSE.ToolView();openWSE.MobileMode();");
        }

        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            app_tab_body.Visible = false;
            chat_client_tab_body.Visible = false;
            overlay_tab.Visible = false;
            aGroupLogoff.Visible = false;
            aGroupLogoff.Enabled = false;
            group_tab.Visible = false;
            lnk_BackToWorkspace.Visible = false;

            if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                string message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #FF0000;'>OFFLINE</span></div>\");";
                if (!_ss.SiteOffLine) {
                    message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #00E000;'>ONLINE</span></div>\");";
                }
                _strScriptreg.Append(message);
            }
        }

        // Hide the overlay button if logged into a group and no overlays exists
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            string tempUsername = GroupSessions.GetUserGroupSessionName(_username);
            WorkspaceOverlays wo = new WorkspaceOverlays();
            wo.GetUserOverlays(tempUsername);
            if (wo.UserOverlays.Count == 0) {
                overlay_tab.Visible = false;
            }
        }

        if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
            _strScriptreg.Clear();
        }
    }
    protected void UserIsAuthenticated_NoLogin() {
        var scriptManager = ScriptManager.GetCurrent(Page);

        SetCurrentPageTitle();
        LoadUserCustomizations();

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();

        GetUserPlugins();

        _aib = new AppIconBuilder(Page, _demoMemberDatabase);
        _aib.BuildAppsForDemo();

        bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(_workspaceMode);

        string GetPageOverlayBtns = string.Empty;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);

        if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
            if (_demoMemberDatabase.ContainsKey("HideAllOverlays") && HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAllOverlays"])) {
                GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
            }
            else {
                GetPageOverlayBtns = "openWSE.GetPagedAddOverlayAndModel();";
            }
        }
        else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
            pnl_OverlaysAll.Enabled = false;
            pnl_OverlaysAll.Visible = false;
        }

        if (!string.IsNullOrEmpty(pnlId)) {
            _strScriptreg.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.CreateOverlayTable();" + GetPageOverlayBtns);
            RefreshOverlays _ro = new RefreshOverlays(string.Empty, ServerSettings.GetServerMapLocation, this.Page, pnlId);
            _ro.GetWorkspaceOverlays_NoLogin(_aib.UserAppList);
        }

        _strScriptreg.Append("openWSE_Config.ShowLoginModalOnDemoMode=" + _ss.ShowLoginModalOnDemoMode.ToString().ToLower() + ";");

        _strScriptreg.Append("$('.addOverlay-bg').remove();");
        _strScriptreg.Append(_aib.AppBuilderCaller.StrScriptreg);

        WorkspaceOverlays wo = new WorkspaceOverlays();
        wo.GetUserOverlays("DemoNoLogin");
        if (wo.UserOverlays.Count == 0) {
            overlay_tab.Visible = false;
        }

        if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
            _strScriptreg.Clear();
        }

        HideOverlaySettings();

        bool hideSidebarMenuIcons = false;
        if (_demoMemberDatabase.ContainsKey("HideSidebarMenuIcons")) {
            hideSidebarMenuIcons = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideSidebarMenuIcons"]);
        }
        

        LoginActivity la = new LoginActivity();
        la.AddItem(ServerSettings.GuestUsername, true, ActivityType.Guest);
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
                    this.Page.Title = title;
                }

                _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('" + title + "');");
            }
        }
    }
    private void LoadUserCustomizations() {
        bool showToolTips = false;
        bool showDateTime = false;
        bool autoHideMode = false;
        bool sidebarAccordionMutliOpenAllowed = false;
        bool presentationMode = false;
        bool hoverPreviewWorkspace = false;
        string animationSpeed = "150";
        bool showMinimizedPreview = false;
        bool taskBarShowAll = false;
        bool showworkspaceNumApp = false;
        bool sidebarAccordion = true;
        bool showTopSearch = false;
        bool siteTips = false;
        bool useAltSidebarTheme = false;
        bool topbarTransparencyMode = false;
        bool sidebarTransparencyMode = false;
        bool hideSidebarMenuIcons = false;

        #region Initialize Variables
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            showToolTips = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ToolTips"]);
            showDateTime = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowDateTime"]);
            autoHideMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["AutoHideMode"]);
            sidebarAccordionMutliOpenAllowed = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["SidebarAccordionMutliOpenAllowed"]);
            animationSpeed = _demoMemberDatabase["AnimationSpeed"];
            presentationMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["PresentationMode"]);

            if (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowTopSearch"]) && (!string.IsNullOrEmpty(_demoMemberDatabase["ShowTopSearch"]))) {
                showTopSearch = true;
            }

            hoverPreviewWorkspace = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HoverPreviewWorkspace"]) || _demoMemberDatabase["HoverPreviewWorkspace"] == string.Empty;

            if ((!HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["SidebarAccordion"])) && (!string.IsNullOrEmpty(_demoMemberDatabase["SidebarAccordion"])))
                sidebarAccordion = false;

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

            if ((HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["SiteTipsOnPageLoad"])) || (string.IsNullOrEmpty(_demoMemberDatabase["SiteTipsOnPageLoad"])))
                siteTips = true;

            useAltSidebarTheme = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["UseAltSidebarTheme"]);
            topbarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["TopbarTransparencyMode"]);
            sidebarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["SidebarTransparencyMode"]);
            hideSidebarMenuIcons = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideSidebarMenuIcons"]);
        }
        else {
            showToolTips = _member.ShowToolTips;
            showDateTime = _member.ShowDateTime;
            autoHideMode = _member.AutoHideMode;
            sidebarAccordionMutliOpenAllowed = _member.SidebarAccordionMutliOpenAllowed;
            presentationMode = _member.PresentationMode;
            showTopSearch = _member.ShowTopSearch;
            animationSpeed = _member.AnimationSpeed.ToString();
            hoverPreviewWorkspace = _member.HoverPreviewWorkspace;
            showMinimizedPreview = _member.ShowMinimizedPreview;
            sidebarAccordion = _member.SidebarAccordion;
            _totalWorkspaces = _member.TotalWorkspaces;
            if (_member.TaskBarShowAll)
                taskBarShowAll = true;

            if (_member.ShowWorkspaceNumApp && _member.TotalWorkspaces != 1)
                showworkspaceNumApp = true;

            if (Notifications.CheckIfErrorNotificationIsOn(_username)) {
                _strScriptreg.Append("openWSE_Config.reportAlert=false;");
            }

            siteTips = _member.SiteTipsOnPageLoad;
            hideSidebarMenuIcons = _member.HideSidebarMenuIcons;

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
                hyp_accountSettings.Attributes["target"] = "_self";
                lb_acctNotifications.Attributes["target"] = "_self";
                lb_manageGroups.Attributes["target"] = "_self";
            }

            if (!ServerSettings.AdminPagesCheck("acctsettings_aspx", _username)) {
                hyp_accountSettings.Enabled = false;
                hyp_accountSettings.Visible = false;
                lb_acctNotifications.Visible = false;
            }

            if (ServerSettings.AdminPagesCheck("grouporg_aspx", _username)) {
                lb_manageGroups.Visible = true;
            }

            StringBuilder strTabs = new StringBuilder();
            strTabs.Append("<ul>");
            string liStructure = "<li id='{0}' class='a-body site-tools-tablist'><div class='menu-title' title='Collapse Menu'>{1}</span>{2}</div><div class='sidebar-divider'></div>{3}</li>";

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

            placeHolder_SettingsTabs.Controls.Clear();
            placeHolder_SettingsTabs.Controls.Add(new LiteralControl(strTabs.ToString()));
            #endregion

            useAltSidebarTheme = _member.UseAltSidebarTheme;
            topbarTransparencyMode = _member.TopbarTransparencyMode;
            sidebarTransparencyMode = _member.SidebarTransparencyMode;
        }
        #endregion

        SetAppAndChatSidebarIcons(hideSidebarMenuIcons);

        #region Show Tool Tips
        _strScriptreg.Append("openWSE_Config.showToolTips=" + showToolTips.ToString().ToLower() + ";");
        #endregion


        #region Show Datetime
        if (showDateTime) {
            DateDisplay.InnerText = ServerSettings.ServerDateTime.ToString("dddd, MMM dd");
            _strScriptreg.Append("startCurrentTime();");
        }
        #endregion


        #region Show Top Search
        if (showTopSearch) {
            _strScriptreg.Append("$('#app_search_tab').show();");
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


        #region SidebarAccordionMutliOpenAllowed Mode
        if (!sidebarAccordionMutliOpenAllowed) {
            _strScriptreg.Append("openWSE_Config.onlyAllowOneAccordionOpen=true;");
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
        AcctSettings.LoadUserBackground(this.Page, _demoCustomizations, _member);
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


        #region Sidebar Accordion
        if (!sidebarAccordion) {
            _strScriptreg.Append("openWSE_Config.sidebarAccordionOn=false;");
        }
        #endregion


        #region Workspace Tips
        _strScriptreg.Append("openWSE_Config.siteTipsOnPageLoad=" + siteTips.ToString().ToLower() + ";");
        #endregion


        #region Sidebar Alt View
        if (useAltSidebarTheme) {
            _strScriptreg.Append("$('#sidebar_menulinks, #accordian-sidebar, #sidebar-accordian').addClass('sidebar-alt-view');");
        }
        #endregion


        #region Sidebar Transparency Mode
        if (sidebarTransparencyMode) {
            _strScriptreg.Append("openWSE.setSidebarTransparency();");
        }
        #endregion


        #region Topbar Transparency Mode
        if (topbarTransparencyMode) {
            _strScriptreg.Append("openWSE.setTopbarTransparency();");
        }
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
                LoginOrRegister.Text = "Have an account? Login below";
            }
            else {
                span_signinText.InnerHtml = "Login / Register";
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
            HelperMethods.SetLogoOpacity(Page, img_LoginGroup);
        }
    }
    private void SetLoginGroup() {
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            try {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(_username);
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    Dictionary<string, string> g = tempGroup.group_dt[0];

                    pnl_LoginGroupText.Enabled = true;
                    pnl_LoginGroupText.Visible = true;

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

                    lbl_LoginGroup.Text = "Logged into " + g["GroupName"];
                    if (!string.IsNullOrEmpty(imgUrl)) {
                        pnl_groupHolder.Enabled = true;
                        pnl_groupHolder.Visible = true;
                        img_LoginGroup.ImageUrl = imgUrl;
                        HelperMethods.SetLogoOpacity(Page, img_LoginGroup);
                    }

                    aGroupLogoff.Enabled = true;
                    aGroupLogoff.Visible = true;

                    AssociateWithGroups = false;
                    _strScriptreg.Append("$('.bgchange-icon').remove();$('.addOverlay-bg').remove();");
                }
                else {
                    GroupSessions.RemoveGroupLoginSession(_username);
                }
            }
            catch { }
        }
    }
    private void SetHelpIcon() {
        SaveControls sc = new SaveControls();
        if (!Request.Url.AbsoluteUri.ToLower().Contains("workspace.aspx") && sc.GetTotalHelpPages(Request.Url.AbsoluteUri) == "0") {
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
                    App _apps = new App(string.Empty);
                    Apps_Coll appInformation = _apps.GetAppInformation(appToOpen);

                    string description = appInformation.Description;
                    if (!string.IsNullOrEmpty(description)) {
                        description += " - " + appInformation.About;
                    }
                    else {
                        description = appInformation.About;
                    }

                    string pageTitle = "<div id='workspace-opened-paged-app'><span class='workspace-app-title'>" + appInformation.AppName + "</span></div>";
                    _strScriptreg.Append("$('#app_title_bg').find('.page-title').html(\"" + pageTitle + "\");");
                }
            }

            _strScriptreg.Append("openWSE_Config.workspaceMode='" + _workspaceMode + "';openWSE.PagedWorkspace('" + appToOpen + "');");
        }
    }
    private void SetAppAndChatSidebarIcons(bool hideSidebarIcons) {
        string appIcon = _ss.GetSidebarCategoryIcon("My Apps");
        string appImg = SideBarItems.EmptySidebarImg;

        string chatIcon = _ss.GetSidebarCategoryIcon("Chat");
        string chatImg = SideBarItems.EmptySidebarImg;

        MemberDatabase tempMember = null;
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            tempMember = new MemberDatabase(HttpContext.Current.User.Identity.Name);
        }

        if (!_ss.HideAllAppIcons && !hideSidebarIcons) {
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
        }

        if (!HttpContext.Current.User.Identity.IsAuthenticated) {
            app_menu_title.InnerHtml = appImg + "Available Apps";
        }
        else if (HttpContext.Current.User.Identity.IsAuthenticated && GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
            app_menu_title.InnerHtml = appImg + "Group Apps";
        }
        else {
            string acctSettingsEdit = string.Empty;
            if (ServerSettings.AdminPagesCheck("acctsettings", HttpContext.Current.User.Identity.Name)) {
                string targetPage = string.Empty;
                if (_member != null && _member.LoadLinksBlankPage) {
                    targetPage = " target='_blank'";
                }
                acctSettingsEdit = "<a href='" + ResolveUrl("~/SiteTools/UserMaintenance/AcctSettings.aspx?tab=pnl_IconSelector") + "' class='sidebar-edit-btn' title='Edit App Selector Style'" + targetPage + "></a>";
            }

            app_menu_title.InnerHtml = appImg + "My Apps" + acctSettingsEdit;
        }

        chatclient_menu_title.InnerHtml = chatImg + "Chat";
    }
    private void HideOverlaySettings() {
        if (_member != null && _member.HideAllOverlays) {
            overlay_tab.Visible = false;
        }
        else if (_demoMemberDatabase != null && _demoMemberDatabase.ContainsKey("HideAllOverlays") && HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAllOverlays"])) {
            overlay_tab.Visible = false;
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
                pnlDH.Attributes["data-backgroundimg"] = "";
                workspace_holder.Controls.Add(pnlDH);
            }
        }
    }

    #endregion


    #region Form Buttons

    protected void aGroupLogoff_Click(object sender, EventArgs e) {
        try {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                LoginActivity la = new LoginActivity();
                la.AddItem(_username, true, ActivityType.Logout);
                GroupSessions.RemoveGroupLoginSession(_username);
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
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                string groupName = GroupSessions.GetUserGroupSessionName(_username);
                GroupSessions.RemoveGroupLoginSession(_username);
                if (!string.IsNullOrEmpty(groupName)) {
                    Response.Redirect("~/" + ServerSettings.DefaultStartupPage + "?group=" + groupName);
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

        if (!string.IsNullOrEmpty(member.DefaultLoginGroup) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(member.Username)) {
            GroupSessions.AddOrSetNewGroupLoginSession(member.Username, member.DefaultLoginGroup);
        }

        MemberDatabase.AddUserSessionIds(member.Username);
        ServerSettings.SetRememberMeOnLogin(Login1, Response);

        if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"])) {
            string redirectUrl = HttpUtility.UrlDecode(Request.QueryString["ReturnUrl"]);
            if (Roles.IsUserInRole(member.Username, ServerSettings.AdminUserName)) {
                Page.Response.Redirect(redirectUrl);
            }
            else {
                foreach (string page in member.AdminPagesList) {
                    if (redirectUrl.ToLower().Contains(page)) {
                        Page.Response.Redirect(redirectUrl);
                    }
                }
            }
        }

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
        string f = "DBFull_" + ServerSettings.ServerDateTime.ToFileTime();
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
                    Response.Redirect("~/Workspace.aspx?wi=" + ServerSettings.ServerDateTime.Ticks.ToString());
            }

            _aib = new AppIconBuilder(Page, _member);
            _aib.BuildAppsForUser();
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
            lbl_notifications.Text = "<span>0</span>";
            lbl_notifications.CssClass = "notifications-none";
            lbl_notifications.ToolTip = "You have no new notifications";

            if (!IsPostBack && !string.IsNullOrEmpty(_username)) {
                _notifi.GetUserNotifications(_username);

                bool setVisiblityToFalse = true;
                if (Page.ToString().ToLower().Contains("acctsettings_aspx")) {
                    setVisiblityToFalse = false;
                }

                if (_notifi.UserNotifications.Count == 0) {
                    if (!setVisiblityToFalse) {
                        _strScriptreg.Append("$('#notifications_tab').hide();");
                    }
                    else {
                        notifications_tab.Visible = false;
                    }
                }
                else {
                    if (!setVisiblityToFalse) {
                        _strScriptreg.Append("$('#notifications_tab').show();");
                    }
                    else {
                        notifications_tab.Visible = true;
                    }
                }
            }
        }
        else {
            lbl_notifications.Text = "<span>" + total.ToString(CultureInfo.InvariantCulture) + "</span>";
            lbl_notifications.CssClass = "notifications-new";
            string notiPlural = "notifications";
            if (total == 1)
                notiPlural = "notification";

            lbl_notifications.ToolTip = "You have " + total.ToString(CultureInfo.InvariantCulture) + " new " + notiPlural;
        }
    }
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
                if (coll.AssociatedWith == id && coll.Enabled)
                    AddPluginToPage(coll.ID);
            }
        }
    }
    private void AddPluginToPage(string id) {
        SitePlugins sp = new SitePlugins(string.Empty);
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

}