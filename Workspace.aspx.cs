#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Overlays;

#endregion

public partial class Workspace : Page {

    #region Private Variables

    private NewUserDefaults _demoCustomizations;
    private ServerSettings _ss = new ServerSettings();
    private readonly StringBuilder _strScriptreg = new StringBuilder();
    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private List<string> _groupname;
    private Groups _groups;
    private MemberDatabase _member;
    private IIdentity _userId;
    private string _username;
    private App _apps;
    private int _numWorkspaces = 4;
    private string _ctlId;
    private bool _noLoginRequired;
    private string _demoPackage;
    private string _sitetheme = "Standard";
    private bool AssociateWithGroups = false;
    private string _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

    #endregion


    #region Load Methods

    protected void Page_Load(object sender, EventArgs e) {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm == null) {
            return;
        }
        _ctlId = sm.AsyncPostBackSourceElementID;

        if (!IsPostBack) {
            if ((_noLoginRequired) && (!_userId.IsAuthenticated)) {
                int.TryParse(_demoCustomizations.DefaultTable["TotalWorkspaces"], out _numWorkspaces);

                if (MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                    AppContainer();
                    LoadUserCustomizations();
                }

                GetAdminNote();

                _strScriptreg.Append("openWSE_Config.demoMode=true;");
                _strScriptreg.Append("$('#MainContent_workspace_1').css({ visibility:'visible',opacity:1.0,filter:'alpha(opacity=100)' });");

                if ((HelperMethods.ConvertBitToBoolean(Request.QueryString["activation"])) && (!string.IsNullOrEmpty(Request.QueryString["user"]))) {
                    string message = "<div align='center'>Your account for " + Request.QueryString["user"] + " has been activated. You may now login to your account.</div>";
                    string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
                    _strScriptreg.Append("openWSE.ShowActivationPopup('" + encodedMessage + "');");
                }
            }
            else if ((_userId.IsAuthenticated) && (_member != null)) {
                _groupname = _member.GroupList;

                if ((_groupname.Count == 0) && (!_member.IsNewMember)) {
                    if (ServerSettings.AdminPagesCheck("GroupOrg", _username))
                        Page.Response.Redirect("~/SiteTools/UserMaintenance/GroupOrg.aspx");

                    HtmlTableCell app_search_tab = (HtmlTableCell)Master.FindControl("app_search_tab");
                    if (app_search_tab != null) {
                        app_search_tab.Visible = false;
                    }
                }

                if (Session["DemoMode"] != null) {
                    if (HelperMethods.ConvertBitToBoolean(Session["DemoMode"])) {
                        Session.Remove("DemoMode");
                    }
                }

                if (!_member.IsNewMember) {
                    _numWorkspaces = _member.TotalWorkspaces;
                    if (_groupname.Count > 0) {
                        AssociateWithGroups = _ss.AssociateWithGroups;

                        SetLoginGroup();

                        if (MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                            AppContainer();
                            LoadUserCustomizations();
                        }
                    }

                    GetAdminNote();
                }
            }
        }

        // Register all startup scripts at the same time
        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        _strScriptreg.Clear();


        #region -- Cache Home --
        if (!IsPostBack) {
            if (_ss.CacheHomePage) {
                //HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(true);
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(60));
                Response.Cache.SetCacheability(HttpCacheability.Private);
                Response.Cache.SetValidUntilExpires(true);
            }
            else {
                HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                HttpContext.Current.Response.Cache.SetNoStore();
                Response.Cache.SetExpires(ServerSettings.ServerDateTime.AddSeconds(60));
                Response.Cache.SetValidUntilExpires(true);
            }
        }
        #endregion

    }
    protected void Page_Init(object sender, EventArgs e) {
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile()) {
            return;
        }

        _userId = HttpContext.Current.User.Identity;
        CheckIfDemo();

        if ((_noLoginRequired) && (!_userId.IsAuthenticated)) {
            _apps = new App("DemoNoLogin");
            _demoCustomizations = new NewUserDefaults("DemoNoLogin");

            _demoCustomizations.GetDefaults();
            if (_demoCustomizations.DefaultTable.Count > 0) {
                _sitetheme = _demoCustomizations.DefaultTable["Theme"];
                _workspaceMode = _demoCustomizations.DefaultTable["WorkspaceMode"];
                if (string.IsNullOrEmpty(_workspaceMode)) {
                    _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
                }
            }
        }
        else if ((!_noLoginRequired) && (!_userId.IsAuthenticated)) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if ((_userId.IsAuthenticated) && (_userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower())) {
            Page.Response.Redirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
        }
        else if (_userId.IsAuthenticated) {
            _username = _userId.Name;
            _member = new MemberDatabase(_username);

            CheckIfAdminPageEnabled(_member);

            _groups = new Groups(_username);
            _apps = new App(_username);

            _sitetheme = _member.SiteTheme;
            _workspaceMode = _member.WorkspaceMode.ToString();
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Error.html");
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
                    string imgUrl = string.Empty;
                    if (HelperMethods.ConvertBitToBoolean(g["IsURL"])) {
                        imgUrl = g["Image"];
                        if (imgUrl.StartsWith("~/")) {
                            imgUrl = ResolveUrl(imgUrl);
                        }
                    }
                    else if (!string.IsNullOrEmpty(g["Image"])) {
                        imgUrl = ResolveUrl("~/Standard_Images/Groups/Logo/" + g["Image"]);
                    }

                    if (!string.IsNullOrEmpty(imgUrl)) {
                        container_logo.ImageUrl = imgUrl;
                    }
                }
            }
            catch { }
        }
    }
    private void CheckIfAdminPageEnabled(MemberDatabase member) {
        if (!IsPostBack) {
            if (!ServerSettings.AdminPagesCheck(Page.ToString(), member.Username)) {
                string[] adminPages = member.AdminPagesList;
                foreach (string adminpage in adminPages) {
                    foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes) {
                        string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                        string[] urlSplit = url.Split('.');
                        if (urlSplit.Length != 2) {
                            continue;
                        }

                        if (urlSplit[0].ToLower() == adminpage.ToLower()) {
                            Page.Response.Redirect(node.Url);
                        }
                    }
                }
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    #endregion


    #region Customizations

    private void AppContainer() {
        bool snapHelper = false;
        bool snapToGrid = false;
        bool appContainer = false;
        string gridSize = "20";
        bool rightClick = false;

        string arg = string.Empty;
        string arg2 = string.Empty;
        hf_appContainer.Value = "";
        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            appContainer = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["AppContainer"]);
            snapHelper = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["AppSnapHelper"]) && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["AppSnapHelper"]);
            snapToGrid = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["SnapToGrid"]) && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["SnapToGrid"]);
            gridSize = _demoCustomizations.DefaultTable["AppGridSize"];
            if (string.IsNullOrEmpty(gridSize)) {
                gridSize = "20";
            }
        }
        else if (_member != null) {
            appContainer = _member.AppContainer;
            snapHelper = _member.AppSnapHelper;
            snapToGrid = _member.AppSnapToGrid;
            gridSize = _member.AppGridSize;
        }

        if (appContainer) {
            hf_appContainer.Value = "#maincontent_overflow";
        }

        if (snapHelper) {
            _strScriptreg.Append("openWSE_Config.appSnapHelper=true;");
        }

        if (snapToGrid) {
            arg2 = "grid: [ " + gridSize + "," + gridSize + " ]";
            if (!string.IsNullOrEmpty(arg))
                arg += ", grid: [ " + gridSize + "," + gridSize + " ]";
            else
                arg += "grid: [ " + gridSize + "," + gridSize + " ]";
        }

        if (!string.IsNullOrEmpty(arg)) {
            string js = "$(function () { $(\".app-main-holder\").draggable({ " + arg + " })";
            if (!string.IsNullOrEmpty(arg2)) {
                js += ".resizable({ " + arg2 + " })";
            }
            js += "; });";
            _strScriptreg.Append(js);
        }
        else if (!string.IsNullOrEmpty(arg2)) {
            string js = "$(function () { $(\".app-main-holder\").draggable({ " + arg2 + " })";
            js += ".resizable({ " + arg2 + " }); });";
            _strScriptreg.Append(js);
        }

        string logoopacity = _ss.LogoOpacity;
        if (!string.IsNullOrEmpty(logoopacity)) {
            double tempOut = 0.0d;
            if (double.TryParse(logoopacity, out tempOut)) {
                string ieFilter = "alpha(opacity=" + (tempOut * 100.0d).ToString() + ")";
                _strScriptreg.Append("$('#container_logo').css('opacity', '" + logoopacity + "');$('#container_logo').css('filter', '" + ieFilter + "');");
            }
        }
    }
    private void LoadUserCustomizations() {
        bool workspaceRotate = false;
        bool rotateAutoRefresh = false;
        string workspaceRotateInt = string.Empty;
        string workspaceRotateScreens = string.Empty;
        bool presentationMode = false;

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            workspaceRotate = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["WorkspaceRotate"]);
            rotateAutoRefresh = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["RotateAutoRefresh"]);
            workspaceRotateInt = _demoCustomizations.DefaultTable["WorkspaceRotateInterval"];
            workspaceRotateScreens = _demoCustomizations.DefaultTable["WorkspaceRotateScreens"];
            presentationMode = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["PresentationMode"]);
        }
        else if (_member != null) {
            if (!_member.UserCustomizationsEnabled) return;
            workspaceRotate = _member.WorkspaceRotate;
            rotateAutoRefresh = _member.RotateAutoRefresh;
            workspaceRotateInt = _member.WorkspaceRotateInterval;
            workspaceRotateScreens = _member.WorkspaceRotateScreens;
            presentationMode = _member.PresentationMode;
        }


        #region Workspace Rotate
        if (workspaceRotate && workspaceRotateInt != "0" && !string.IsNullOrEmpty(workspaceRotateInt)) {
            string autoRefresh_rotate = "false";
            if (rotateAutoRefresh)
                autoRefresh_rotate = "true";

            _strScriptreg.Append("openWSE.AutoRotateWorkspace(" + workspaceRotateInt + ", 1, " + workspaceRotateScreens + ", " + autoRefresh_rotate + ");");
        }
        #endregion


        #region Presentation
        if (presentationMode) {
            _strScriptreg.Append("openWSE.PresentationMode.init();");
        }
        #endregion

    }

    #endregion


    #region Admin Note Overlay

    private void GetAdminNote() {
        string note = _ss.AdminNote;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, _workspaceMode);
        if (!string.IsNullOrEmpty(note) && !string.IsNullOrEmpty(pnlId)) {
            pnl_adminnote.Enabled = true;
            pnl_adminnote.Visible = true;
            lbl_adminnote.Text = "<div class='float-left margin-right-big margin-bottom img-administrator'></div>" + note;
            lbl_adminnoteby.Text = _ss.AdminNoteBy;
        }
        else {
            pnl_adminnote.Enabled = false;
            pnl_adminnote.Visible = false;
        }
    }

    #endregion


    #region Check if need to show updates

    private void CheckIfDemo() {
        _noLoginRequired = _ss.NoLoginRequired;
        if ((HelperMethods.ConvertBitToBoolean(Request.QueryString["Demo"])) && (_ss.ShowPreviewButtonLogin)) {
            CheckUpdatesPopup_Demo();
            _noLoginRequired = true;
            Session["DemoMode"] = "true";
        }
        else if (Session["DemoMode"] != null) {
            if (HelperMethods.ConvertBitToBoolean(Session["DemoMode"].ToString())) {
                Session.Remove("DemoMode");
                _noLoginRequired = _ss.NoLoginRequired;
            }
        }
    }

    private void CheckUpdatesPopup_Demo() {
        ShowUpdatePopup sup = new ShowUpdatePopup();
        string message = sup.GetNewUpdateMessage(ServerSettings.GetServerMapLocation, "Standard", false);
        string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
        _strScriptreg.Append("openWSE.ShowUpdatesPopup('" + encodedMessage + "');");
    }

    #endregion

}