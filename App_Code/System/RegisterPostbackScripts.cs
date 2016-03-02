using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Overlays;

public class RegisterPostbackScripts {

    /// <summary>
    /// Call this on every page that uses the Site.Master in the Page_Load. Place at the end of the method.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="isPostback"></param>
    /// <param name="username"></param>
    /// <param name="demoCustomizations"></param>
    public static void CallPostbackControl(Page page, bool isPostback, MemberDatabase member, Dictionary<string, string> demoMemberDatabase) {
        if (isPostback) {
            StringBuilder _strScriptreg = new StringBuilder();

            #region Initialization

            string username = string.Empty;
            string workspaceMode = string.Empty;
            App _apps = new App("DemoNoLogin");
            Dictionary<string, string> _demoMemberDatabase = new Dictionary<string, string>();

            if (demoMemberDatabase != null && demoMemberDatabase.Count > 0) {
                workspaceMode = demoMemberDatabase["WorkspaceMode"];
                _demoMemberDatabase = demoMemberDatabase;
            }
            else if (member != null) {
                username = member.Username;
                workspaceMode = member.WorkspaceMode.ToString();
                _apps = new App(username);
            }

            #endregion

            // Place all control cases below
            string controlName = page.Request.Params["__EVENTTARGET"];
            switch (controlName) {
                case "hf_loadApp1":
                    _strScriptreg.Append(LoadApp1(page, _apps, username));
                    break;

                case "hf_loadOverlay1":
                    _strScriptreg.Append(LoadOverlay1(page, _apps, username, workspaceMode));
                    break;

                case "hf_SearchSite":
                    _strScriptreg.Append(SearchSite(page, _apps, username));
                    break;

                case "hf_ReloadApp":
                    _strScriptreg.Append(ReloadApp(page, _apps, username, _demoMemberDatabase));
                    break;

                case "hf_aboutstatsapp":
                    _strScriptreg.Append(Aboutstatsapp(page, member, _apps, username));
                    break;
            }

            if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
                RegisterStartupScript(page, _strScriptreg.ToString());
            }
        }
    }

    /// <summary>
    /// Registers a StartupScript using the ScriptManager
    /// </summary>
    /// <param name="ctrl">Control or Page object (Set to null if not sure)</param>
    /// <param name="cmd">Script Command</param>
    /// <param name="key">Script Key (If available)</param>
    public static void RegisterStartupScript(Control ctrl, string cmd, string key = "") {
        if (ctrl == null) {
            System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;
            if (page != null) {
                ctrl = page;
            }
            else {
                return;
            }
        }

        if (string.IsNullOrEmpty(key)) {
            key = Guid.NewGuid().ToString();
        }
        else {
            try {
                if (ctrl.Page.ClientScript.IsStartupScriptRegistered(key)) {
                    return;
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        ScriptManager.RegisterStartupScript(ctrl, ctrl.GetType(), key, cmd, true);
    }

    /// <summary>
    /// Call this on the Page_Init to reload the overlays and apps
    /// </summary>
    /// <param name="page"></param>
    /// <param name="isPostback"></param>
    /// <param name="AssociateWithGroups"></param>
    /// <param name="member"></param>
    /// <param name="demoMemberDatabase"></param>
    public static void ReInitOverlaysAndApps(Page page, bool isPostback, bool AssociateWithGroups, MemberDatabase member, Dictionary<string, string> demoMemberDatabase) {
        if (isPostback) {
            StringBuilder _strScriptreg = new StringBuilder();

            #region Initialization

            string username = string.Empty;
            string workspaceMode = string.Empty;
            App _apps = new App("DemoNoLogin");
            Dictionary<string, string> _demoMemberDatabase = new Dictionary<string, string>();
            List<Apps_Coll> _userAppList = new List<Apps_Coll>();
            if (demoMemberDatabase != null && demoMemberDatabase.Count > 0) {
                workspaceMode = demoMemberDatabase["WorkspaceMode"];
                _userAppList = RebuildDemoAppList(_apps, demoMemberDatabase);
                _demoMemberDatabase = demoMemberDatabase;
            }
            else {
                username = member.Username;
                workspaceMode = member.WorkspaceMode.ToString();
                _apps = new App(username);
                _userAppList = RebuildUserAppList(_apps, member, AssociateWithGroups);
            }

            string controlName = page.Request.Params["__EVENTTARGET"];

            #endregion

            #region Reinitialize the Apps and Overlays and Register Startup Script

            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                if (HelperMethods.DoesPageContainStr("workspace.aspx")) {
                    _strScriptreg.Append(ReInitApps_NoLogin(controlName, page, _apps, _userAppList, _demoMemberDatabase));
                }

                if (!_demoMemberDatabase.ContainsKey("HideAllOverlays") || !HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAllOverlays"])) {
                    ReInitOverlays_NoLogin(page, _apps, workspaceMode);
                }
            }
            else {
                if (HelperMethods.DoesPageContainStr("workspace.aspx")) {
                    _strScriptreg.Append(ReInitApps(controlName, page, _apps, _userAppList, member));
                }

                if (!member.HideAllOverlays) {
                    ReInitOverlays(page, _apps, username, workspaceMode);
                }
            }

            if (!string.IsNullOrEmpty(_strScriptreg.ToString())) {
                RegisterStartupScript(page, _strScriptreg.ToString());
            }

            #endregion
        }
    }

    private static List<Apps_Coll> RebuildUserAppList(App _apps, MemberDatabase _member, bool AssociateWithGroups) {
        List<string> memberApps = _apps.DeleteDuplicateEnabledApps(_member);
        List<Apps_Coll> _userAppList = new List<Apps_Coll>();

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

            if ((_member.Username.ToLower() != dt.CreatedBy.ToLower()) && (dt.IsPrivate)) {
                continue;
            }

            _userAppList.Add(dt);
        }

        return _userAppList;
    }
    private static List<Apps_Coll> RebuildDemoAppList(App _apps, Dictionary<string, string> _demoMemberDatabase) {
        List<Apps_Coll> _userAppList = new List<Apps_Coll>();

        AppPackages package = new AppPackages(false);
        string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);

        foreach (var w in appList) {
            Apps_Coll dt = _apps.GetAppInformation(w);
            _userAppList.Add(dt);
        }

        return _userAppList;
    }


    #region Register Postback Scripts

    private static string LoadApp1(Page _page, App _apps, string _username) {
        string returnVal = string.Empty;

        HiddenField hf_loadApp1 = (HiddenField)_page.Master.FindControl("hf_loadApp1");
        PlaceHolder PlaceHolder1 = (PlaceHolder)_page.Master.FindControl("PlaceHolder1");

        if ((hf_loadApp1 != null) && (PlaceHolder1 != null)) {
            if (!string.IsNullOrEmpty(hf_loadApp1.Value)) {
                string id = hf_loadApp1.Value;
                string filename = _apps.GetAppFilename(id);
                if (!string.IsNullOrEmpty(filename) && HelperMethods.IsValidAscxFile(filename)) {
                    try {
                        UserControl uc = (UserControl)_page.LoadControl("~/Apps/" + filename);
                        uc.ID = "UC" + id.Replace("-", "_");
                        PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + id.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                        PlaceHolder1.Controls.Add(uc);
                        PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
                        returnVal = "openWSE.LoadUserControl('" + id + "');";
                    }
                    catch (Exception ex) {
                        if (!string.IsNullOrEmpty(_username))
                            _apps.DeleteAppLocal(id, _username);

                        AppLog.AddError(ex);
                    }
                }
            }

            hf_loadApp1.Value = string.Empty;
        }
        return returnVal;
    }
    private static string LoadOverlay1(Page _page, App _apps, string _username, string _workspaceMode) {
        string returnVal = string.Empty;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(_page, _workspaceMode);

        if (!string.IsNullOrEmpty(pnlId)) {
            HiddenField hf_loadOverlay1 = (HiddenField)_page.Master.FindControl("hf_loadOverlay1");
            if (hf_loadOverlay1 != null) {
                WorkspaceOverlays _do = new WorkspaceOverlays();
                WorkspaceOverlay_Coll coll = _do.GetWorkspaceOverlay(hf_loadOverlay1.Value);
                if (!string.IsNullOrEmpty(coll.OverlayName)) {
                    if (_do.IsUserOverlayEnabled(_username, hf_loadOverlay1.Value)) {
                        _do.DeleteUserOverlay(_username, hf_loadOverlay1.Value);
                    }
                    else {
                        _do.AddUserOverlay(_username, hf_loadOverlay1.Value);
                    }
                }

                RefreshOverlays _ro = new RefreshOverlays(_username, ServerSettings.GetServerMapLocation, _page, pnlId);
                returnVal += _ro.DynamicReloadOverlays();
                hf_loadOverlay1.Value = string.Empty;
            }
        }

        return returnVal;
    }
    private static string SearchSite(Page _page, App _apps, string _username) {
        string returnVal = string.Empty;
        HiddenField hf_SearchSite = (HiddenField)_page.Master.FindControl("hf_SearchSite");

        if (hf_SearchSite != null) {
            string search = hf_SearchSite.Value;
            if (!string.IsNullOrEmpty(search)) {
                string id = _apps.GetAppID(search);
                if (!string.IsNullOrEmpty(id)) {
                    returnVal = "openWSE.OpenAppNoti(\"" + id + "\");";
                }
                else if (search.ToLower().Contains(".html") || search.ToLower().Contains(".aspx")) {
                    string toolViewQuery = "?toolView=true";
                    if (search.ToLower().Contains("?")) {
                        toolViewQuery = "&toolView=true";
                    }

                    search += toolViewQuery;
                    if (search.StartsWith("~/")) {
                        search = _page.ResolveUrl(search);
                    }

                    returnVal = "openWSE.PopOutToolSearch(\"" + search + "\");";
                }
                else {
                    List<string> pages = ServerSettings.AdminPages();
                    foreach (string p in pages) {
                        if (p.ToLower() == search.ToLower()) {
                            if (ServerSettings.AdminPagesCheck(p, _username)) {
                                string[] pageInfo = ServerSettings.GetAdminPageLink(p);
                                if (pageInfo.Length == 2) {
                                    returnVal = "openWSE.PopOutTool(\"" + pageInfo[1] + "\", \"" + pageInfo[0] + "?toolView=true" + "\");";
                                }
                            }
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(returnVal)) {
                    returnVal = "openWSE.SearchExternalSite(\"" + HttpUtility.UrlEncode(search) + "\");";
                }
            }

            hf_SearchSite.Value = string.Empty;
        }

        return returnVal;
    }
    private static string ReloadApp(Page _page, App _apps, string _username, Dictionary<string, string> _demoMemberDatabase) {
        string returnVal = string.Empty;

        HiddenField hf_ReloadApp = (HiddenField)_page.Master.FindControl("hf_ReloadApp");
        PlaceHolder PlaceHolder1 = (PlaceHolder)_page.Master.FindControl("PlaceHolder1");

        if ((hf_ReloadApp != null) && (PlaceHolder1 != null)) {
            string appId = hf_ReloadApp.Value;
            if (!string.IsNullOrEmpty(appId)) {
                var appScript = new StringBuilder();

                if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                    #region NoLoginRequired Users
                    string _demoPackage = _demoMemberDatabase["AppPackage"];
                    AppPackages package = new AppPackages(false);
                    string[] appList = package.GetAppList(_demoPackage);

                    var templist = new List<string>();
                    foreach (string x in appList) {

                        Apps_Coll row = _apps.GetAppInformation(x);
                        if (row.AppId == appId) {
                            string fileName = _apps.GetAppFilename(row.AppId);
                            if (!HelperMethods.IsValidAscxFile(fileName)) {
                                if (!string.IsNullOrEmpty(fileName) && !HelperMethods.IsValidHttpBasedAppType(fileName)) {
                                    fileName = ServerSettings.ResolveUrl("~/Apps/" + fileName);
                                }

                                string css = _apps.GetAppCssClass(row.AppId);
                                if (string.IsNullOrEmpty(css))
                                    css = "app-main";

                                appScript.Append("openWSE.CreateSOApp('" + row.AppId + "','" + row.AppName + "',");
                                appScript.Append("'" + fileName + "','75px','75px");
                                appScript.Append("','" + row.MinWidth + "','" + row.MinHeight + "','0','");
                                appScript.Append("0','" + css + "');");
                            }
                            else if (HelperMethods.IsValidAppFileType(fileName) && !HelperMethods.IsValidHttpBasedAppType(fileName)) {
                                if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + fileName)) {
                                    var advPanel = new Panel { ID = appId.Replace("-", "_") + "_advPanel" };
                                    advPanel.Controls.Clear();
                                    UserControl uc = (UserControl)_page.LoadControl("~/Apps/" + fileName);
                                    PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + row.AppId.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                                    PlaceHolder1.Controls.Add(uc);
                                    PlaceHolder1.Controls.Add(new LiteralControl("</div>"));

                                    appScript.Append("openWSE.LoadUserControl('" + appId + "');");
                                }
                            }

                            break;
                        }
                    }

                    #endregion
                }
                else {
                    #region Authenticated Users

                    _apps.GetUserInstalledApps();
                    foreach (UserApps_Coll row in _apps.UserAppList) {
                        if (string.IsNullOrEmpty(row.AppId))
                            _apps.DeleteAppLocal("");
                        else {
                            if (row.AppId == appId) {
                                string fileName = _apps.GetAppFilename(row.AppId);

                                if (!HelperMethods.IsValidAscxFile(fileName)) {
                                    if (!string.IsNullOrEmpty(fileName) && !HelperMethods.IsValidHttpBasedAppType(fileName)) {
                                        fileName = ServerSettings.ResolveUrl("~/Apps/" + fileName);
                                    }

                                    string css = _apps.GetAppCssClass(row.AppId);
                                    if (string.IsNullOrEmpty(css))
                                        css = "app-main";

                                    appScript.Append("openWSE.CreateSOApp('" + row.AppId + "','" + row.AppName + "',");
                                    appScript.Append("'" + fileName + "','" + row.PosX + "','" + row.PosY);
                                    appScript.Append("','" + row.Width + "','" + row.Height + "','" + row.Minimized + "','");
                                    appScript.Append(row.Maximized + "','" + css + "');");
                                }
                                else if (HelperMethods.IsValidAppFileType(fileName) && !HelperMethods.IsValidHttpBasedAppType(fileName)) {
                                    try {
                                        if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + fileName)) {
                                            UserControl uc = (UserControl)_page.LoadControl("~/Apps/" + fileName);
                                            uc.ID = "UC" + appId.Replace("-", "_");
                                            PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + row.AppId.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                                            PlaceHolder1.Controls.Add(uc);
                                            PlaceHolder1.Controls.Add(new LiteralControl("</div>"));

                                            appScript.Append("openWSE.LoadUserControl('" + appId + "');");
                                        }
                                    }
                                    catch (Exception ex) {
                                        if (!string.IsNullOrEmpty(_username))
                                            _apps.DeleteAppLocal(appId, _username);

                                        AppLog.AddError(ex);
                                    }
                                }
                                break;
                            }
                        }
                    }

                    #endregion
                }

                if (!string.IsNullOrEmpty(appScript.ToString()))
                    returnVal = appScript.ToString();
            }

            hf_ReloadApp.Value = string.Empty;
        }

        return returnVal;
    }
    private static string Aboutstatsapp(Page _page, MemberDatabase _member, App _apps, string _username) {
        string returnVal = string.Empty;
        HiddenField hf_aboutstatsapp = (HiddenField)_page.Master.FindControl("hf_aboutstatsapp");

        if (hf_aboutstatsapp != null) {
            string[] vals = hf_aboutstatsapp.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            if (vals.Length == 2) {
                string type = vals[0];
                string appId = vals[1];
                if (!string.IsNullOrEmpty(appId)) {
                    UpdatePanel updatepnl_aboutHolder = (UpdatePanel)_page.Master.FindControl("updatepnl_aboutHolder");
                    Panel pnl_aboutHolder = (Panel)_page.Master.FindControl("pnl_aboutHolder");
                    HtmlAnchor btn_uninstallApp = (HtmlAnchor)_page.Master.FindControl("btn_uninstallApp");
                    if (btn_uninstallApp != null && pnl_aboutHolder != null && updatepnl_aboutHolder != null) {
                        btn_uninstallApp.Visible = false;
                        string title = "";
                        if (type == "about") {
                            if (_member != null && !GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                                if ((ServerSettings.AdminPagesCheck("ASP.sitetools_appmaintenance_appinstaller_aspx", _username)) && (!appId.Contains("app-ChatClient-"))) {
                                    btn_uninstallApp.Visible = true;
                                    btn_uninstallApp.Attributes["onclick"] = "openWSE.UninstallApp('" + appId + "');return false;";
                                }
                            }
                            else
                                btn_uninstallApp.Visible = false;

                            _apps.BuildAboutApp(pnl_aboutHolder, appId, _username);

                            ServerSettings ss = new ServerSettings();
                            if (!appId.Contains("app-ChatClient")) {
                                if (ss.AllowAppRating) {
                                    AppRatings ratings = new AppRatings();
                                    returnVal = "openWSE.RatingStyleInit('.app-rater-" + appId + "', '" + ratings.GetAverageRating(appId) + "', false, '" + appId + "', true);";
                                }

                                title = "'About " + _apps.GetAppName(appId) + "'";
                            }
                            else {
                                title = "'About Chat Client'";
                            }

                            updatepnl_aboutHolder.Update();
                            returnVal += "openWSE.LoadModalWindow(true, 'aboutApp-element', " + title + ");";
                        }
                        else if (type == "uninstall") {
                            _member.RemoveEnabledApp(appId);
                            _apps.DeleteAppLocal(appId, _username);

                            AppIconBuilder aib = new AppIconBuilder(_page, _member);
                            aib.BuildAppsForUser();

                            updatepnl_aboutHolder.Update();
                            returnVal += "openWSE.LoadModalWindow(false, 'aboutApp-element', '');";
                        }
                    }
                }
            }

            hf_aboutstatsapp.Value = string.Empty;
        }

        return returnVal;
    }

    #endregion


    #region Re-Register Overlays/Apps

    private static string ReInitApps(string controlName, Page _page, App _apps, List<Apps_Coll> _userAppList, MemberDatabase member) {
        if ((controlName == "hf_loadapp1") || (controlName == "hf_reloadapp") || (_userAppList.Count == 0)) {
            return string.Empty;
        }

        StringBuilder returnVal = new StringBuilder();
        _apps.GetAllAscxApps();

        foreach (Apps_Coll dr in _apps.AppList) {
            string id = dr.AppId;
            Apps_Coll temp = _userAppList.Find(item => item.AppId == id);
            if ((temp == null) || (string.IsNullOrEmpty(temp.AppId))) {
                continue;
            }

            string filename = dr.filename;
            bool closed = _apps.GetClosedState(id);

            if ((dr.AutoLoad) || (dr.AutoOpen) || (!closed)) {
                if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                    UserControl uc = (UserControl)_page.LoadControl("~/Apps/" + filename);
                    bool _isUpdatePnl = HasUpdatePanel(uc.Controls, false);
                    if (_isUpdatePnl) {
                        returnVal.Append(ReBuildPostbackApp(dr, _apps, _page, member));
                    }
                }
            }
        }

        return returnVal.ToString();
    }
    private static void ReInitOverlays(Page _page, App _apps, string _username, string _workspaceMode) {
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(_page, _workspaceMode);
        if (!string.IsNullOrEmpty(pnlId)) {
            WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
            _workspaceOverlays.GetWorkspaceOverlays();

            string tempUsername = GroupSessions.GetUserGroupSessionName(_username);
            _workspaceOverlays.GetUserOverlays(tempUsername);

            foreach (UserOverlay_Coll userOverlay in _workspaceOverlays.UserOverlays) {
                string fileLoc = _workspaceOverlays.GetWorkspaceFileLoc(userOverlay.OverlayID);
                if (!string.IsNullOrEmpty(fileLoc)) {
                    if (File.Exists(ServerSettings.GetServerMapLocation + fileLoc)) {
                        UserControl uc = (UserControl)_page.LoadControl("~/" + fileLoc);
                        bool _isUpdatePnl = HasUpdatePanel(uc.Controls, false);
                        if (_isUpdatePnl) {
                            RefreshOverlays _ro = new RefreshOverlays(_username, ServerSettings.GetServerMapLocation, _page, pnlId);
                            _ro.DynamicReloadOverlay(userOverlay);
                        }
                    }
                }
            }
        }
    }

    private static string ReInitApps_NoLogin(string controlName, Page _page, App _apps, List<Apps_Coll> _userAppList, Dictionary<string, string> _demoMemberDatabase) {
        if ((controlName == "hf_loadapp1") || (controlName == "hf_reloadapp") || (_userAppList.Count == 0)) {
            return string.Empty;
        }

        StringBuilder returnVal = new StringBuilder();
        _apps.GetAllAscxApps();

        foreach (Apps_Coll dr in _apps.AppList) {
            string id = dr.AppId;
            if (_userAppList == null) {
                continue;
            }
            else {
                Apps_Coll foundColl = _userAppList.Find(item => item != null && item.AppId == id);
                if (foundColl == null || string.IsNullOrEmpty(foundColl.AppId)) {
                    continue;
                }
            }

            try {
                string filename = dr.filename;
                if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                    UserControl uc = (UserControl)_page.LoadControl("~/Apps/" + filename);
                    bool _isUpdatePnl = HasUpdatePanel(uc.Controls, false);
                    if (_isUpdatePnl) {
                        returnVal.Append(ReBuildPostbackApp_NoLogin(dr, _apps, _page, _demoMemberDatabase));
                    }
                }
            }
            catch { }
        }

        return returnVal.ToString();
    }
    private static void ReInitOverlays_NoLogin(Page _page, App _apps, string _workspaceMode) {
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(_page, _workspaceMode);

        if (!string.IsNullOrEmpty(pnlId)) {
            WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
            _workspaceOverlays.GetWorkspaceOverlays();

            NewUserDefaults _dc = new NewUserDefaults("DemoNoLogin");
            AppPackages package = new AppPackages(false);
            string[] userApps = package.GetAppList(_dc.GetDemoAppPackage);

            List<string> LoadedList = new List<string>();
            foreach (string w in userApps) {
                var table = _apps.GetAppInformation(w);
                if (table != null) {
                    string overlayId = table.OverlayID;
                    string[] splitOverlays = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oId in splitOverlays) {
                        WorkspaceOverlay_Coll userOverlay = _workspaceOverlays.GetWorkspaceOverlay(oId);
                        if (!string.IsNullOrEmpty(userOverlay.FileLocation)) {
                            if (File.Exists(ServerSettings.GetServerMapLocation + userOverlay.FileLocation)) {
                                UserControl uc = (UserControl)_page.LoadControl("~/" + userOverlay.FileLocation);
                                bool _isUpdatePnl = HasUpdatePanel(uc.Controls, false);
                                if (_isUpdatePnl) {
                                    RefreshOverlays _ro = new RefreshOverlays(string.Empty, ServerSettings.GetServerMapLocation, _page, pnlId);
                                    _ro.DynamicReloadOverlay_NoLogin(userOverlay);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static string ReBuildPostbackApp(Apps_Coll dr, App _apps, Page _page, MemberDatabase member) {
        StringBuilder _strScriptreg = new StringBuilder();

        string _username = member.Username;

        string id = dr.AppId;
        string w = dr.AppName;

        int ar = 0;
        int am = 0;
        if (dr.AllowMaximize)
            am = 1;
        if (dr.AllowResize)
            ar = 1;

        string css = dr.CssClass;
        if (string.IsNullOrEmpty(css))
            css = "app-main";

        string image = dr.Icon;
        string filename = dr.filename;
        if (!string.IsNullOrEmpty(filename)) {
            var fi = new FileInfo(filename);
            if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif")
                 && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {
                string workspace = _apps.GetCurrentworkspace(id);
                if (string.IsNullOrEmpty(workspace)) {
                    if (dr.DefaultWorkspace != "0") {
                        workspace = "workspace_" + dr.DefaultWorkspace;
                    }
                    else {
                        workspace = "workspace_1";
                    }
                }

                AppBuilder wb = new AppBuilder((ContentPlaceHolder)_page.Master.FindControl("MainContent"), member);
                wb.AppDivGenerator(id, w, image, ar, am, css, workspace, filename, fi.Extension.ToLower() == ".ascx",
                                   dr.MinHeight, dr.MinWidth, dr.AllowPopOut,
                                   dr.PopOutLoc, dr.AutoFullScreen, dr.AutoLoad, dr.AutoOpen, dr.AppBackgroundColor);

                _strScriptreg.Append(wb.StrScriptreg);
            }
        }

        return _strScriptreg.ToString();
    }
    private static string ReBuildPostbackApp_NoLogin(Apps_Coll dr, App _apps, Page _page, Dictionary<string, string> _demoMemberDatabase) {
        StringBuilder _strScriptreg = new StringBuilder();

        string id = dr.AppId;
        string w = dr.AppName;

        int ar = 0;
        int am = 0;
        if (dr.AllowMaximize)
            am = 1;
        if (dr.AllowResize)
            ar = 1;

        string css = dr.CssClass;
        if (string.IsNullOrEmpty(css))
            css = "app-main";

        string image = dr.Icon;
        string filename = dr.filename;
        if (!string.IsNullOrEmpty(filename)) {
            var fi = new FileInfo(filename);
            if ((fi.Extension.ToLower() != ".exe") && (fi.Extension.ToLower() != ".com") && (fi.Extension.ToLower() != ".pif")
                 && (fi.Extension.ToLower() != ".bat") && (fi.Extension.ToLower() != ".scr")) {
                string workspace = _apps.GetCurrentworkspace(id);

                if (string.IsNullOrEmpty(workspace)) {
                    workspace = "workspace_" + dr.DefaultWorkspace;
                    if (dr.DefaultWorkspace != "0") {
                        workspace = "workspace_" + dr.DefaultWorkspace;
                    }
                    else {
                        workspace = "workspace_1";
                    }
                }

                AppBuilder wb = new AppBuilder((ContentPlaceHolder)_page.Master.FindControl("MainContent"), _demoMemberDatabase);
                wb.AppDivGenerator_NoLogin(id, w, image, ar, am, css, workspace, filename, fi.Extension.ToLower() == ".ascx",
                                            dr.MinHeight, dr.MinWidth, dr.AllowPopOut,
                                            dr.PopOutLoc, dr.AutoFullScreen, dr.AutoLoad, dr.AutoOpen, dr.AppBackgroundColor);

                _strScriptreg.Append(wb.StrScriptreg);
            }
        }

        return _strScriptreg.ToString();
    }

    private static bool HasUpdatePanel(ControlCollection controls, bool _isUpdatePnl) {
        foreach (Control control in controls) {
            if (control is UpdatePanel) {
                return true;
            }
            else if (control.HasControls()) {
                _isUpdatePnl = HasUpdatePanel(control.Controls, _isUpdatePnl);
            }

            if (_isUpdatePnl) {
                return true;
            }
        }

        return _isUpdatePnl;
    }

    #endregion

}