using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for AppBuilder
/// </summary>
public class AppBuilder : Page {

    private ServerSettings _ss = new ServerSettings();
    private StringBuilder _strScriptreg = new StringBuilder();
    private ContentPlaceHolder _mainContent;
    private Dictionary<string, string> _demoMemberDatabase;
    private string _username = string.Empty;
    private bool _showappimg;
    private bool _showapptitle;
    private App _apps = new App();
    private MemberDatabase _member;
    private int _totalWorkspaces = 4;


    public AppBuilder(ContentPlaceHolder MainContent, MemberDatabase member) {
        _mainContent = MainContent;
        _member = member;
        _username = _member.Username;
        _showappimg = _member.AppHeaderIcon;
        _showapptitle = _member.ShowAppTitle;
        _apps = new App(_username);
        _totalWorkspaces = _member.TotalWorkspaces;
    }

    public AppBuilder(ContentPlaceHolder MainContent, Dictionary<string, string> demoMemberDatabase) {
        _mainContent = MainContent;
        _demoMemberDatabase = demoMemberDatabase;
        _showappimg = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["AppHeaderIcon"]);
        _showapptitle = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowAppTitle"]);

        int.TryParse(_demoMemberDatabase["TotalWorkspaces"], out _totalWorkspaces);

        int totalAllowed = _ss.TotalWorkspacesAllowed;
        if (_totalWorkspaces > totalAllowed || _totalWorkspaces == 0) {
            _totalWorkspaces = 4;
        }
    }


    /// <summary>
    /// Apps for the Authenticated user
    /// </summary>
    public void AppDivGenerator(string id, string title, string img, int ar, int am, string css, string workspace, string filename, bool isUc, string height, string width, bool allowpopout, string popoutloc, bool autoFullScreen, bool autoLoad, bool autoOpen) {
        var widg = new StringBuilder();
        Panel appHolder = Getworkspace(workspace);

        if (appHolder != null) {
            var tempApps = new App(_username);
            bool hideAllIcons = _ss.HideAllAppIcons;

            if (_member.HideAppIcon) {
                hideAllIcons = true;
            }

            if (autoFullScreen) {
                css += " auto-full-page";
            }

            string minheightMinwidth = string.Empty;
            if (!string.IsNullOrEmpty(height)) {
                if (height != "0") {
                    minheightMinwidth += "min-height: " + height + "px;";
                }
            }
            if (!string.IsNullOrEmpty(width)) {
                if (width != "0") {
                    minheightMinwidth += "min-width: " + width + "px;";
                }
            }

            if (ar == 1) {
                widg.Append("<div data-appid='" + id + "' class='app-main-holder " + css + "' style='display: none;" + minheightMinwidth + "'>");
            }
            else {
                widg.Append("<div data-appid='" + id + "' class='app-main-holder " + css + " no-resize' style='display: none;" + minheightMinwidth + "'>");
            }


            #region Build Button Header
            widg.Append("<div class='app-head-button-holder'>");
            widg.Append("<a href='#" + id + "' class='options-button-app' title='View app options'></a>");
            widg.Append("<div class='app-popup-inner-app'>");
            widg.Append("<table><tbody>");

            widg.Append("<tr><td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>");
            widg.Append("<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a>Refresh</li>");
            string popoutClick = "openWSE.PopOutFrame(this,'" + CheckPopoutURL(popoutloc) + "')";
            if ((allowpopout) && (!string.IsNullOrEmpty(popoutloc)))
                widg.Append("<li onclick=\"" + popoutClick + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a>Pop out</li>");

            widg.Append("<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div>About</li>");
            widg.Append("</ul></td></tr>");


            if (_totalWorkspaces > 1) {
                widg.Append("<tr><td><div class='clear-space'></div><span class='workspace-selector-app-option'>Workspace:</span>");
                widg.Append("<select class='app-options-workspace-switch'>");
                for (int ii = 0; ii < _totalWorkspaces; ii++) {
                    widg.Append("<option>" + (ii + 1).ToString() + "</option>");
                }
                widg.Append("</select></td></tr>");
            }

            widg.Append("</tbody></table></div>");

            widg.Append("<a href='#" + id + "' class='exit-button-app' title='Close'></a>");
            if (am == 1)
                widg.Append("<a href='#" + id + "' class='maximize-button-app' title='Maximize/Normal'></a>");

            widg.Append("<a href='#" + id + "' class='minimize-button-app' title='Minimize'></a>");

            widg.Append("</div>");
            #endregion


            #region App Header
            if (am == 1)
                widg.Append("<div class='app-head app-head-dblclick'>");
            else
                widg.Append("<div class='app-head'>");

            if (!hideAllIcons) {
                string appimgDisplay = "class='app-header-icon'";
                if (!_showappimg)
                    appimgDisplay = "class='app-header-icon display-none'";

                widg.Append("<img alt='" + title + "' src='" + ServerSettings.ResolveUrl("~/Standard_Images/App_Icons/" + img) + "' " + appimgDisplay + " />");
            }

            if (_showapptitle) {
                widg.Append("<span class='app-title'>" + title + "</span>");
            }
            else {
                widg.Append("<span class='app-title display-none'>" + title + "</span>");
            }

            widg.Append("</div>");
            #endregion


            #region App Body
            widg.Append("<div class='app-body'>");
            appHolder.Controls.Add(new LiteralControl(widg.ToString()));
            GetAppsToLoad(isUc, autoLoad, autoOpen, filename, id, title, workspace, width, height, ref appHolder);
            appHolder.Controls.Add(new LiteralControl("</div></div>"));
            #endregion
        }
    }
    private void GetAppsToLoad(bool isUc, bool autoLoad, bool autoOpen, string filename, string id, string title, string workspace, string minWidth, string minHeight, ref Panel appHolder) {
        StringBuilder appScript = new StringBuilder();

        bool closedState = true;
        string currworkspace = workspace;
        string width = minWidth;
        string height = minHeight;
        string minimized = string.Empty;
        string maximized = string.Empty;
        string posX = "0";
        string posY = "0";

        UserApps_Coll dt = _apps.GetUserAppInformation(id);
        if (!string.IsNullOrEmpty(dt.ID)) {
            closedState = dt.Closed;
            currworkspace = dt.Workspace;
            width = dt.Width;
            height = dt.Height;
            minimized = dt.Minimized.ToString().ToLower();
            maximized = dt.Maximized.ToString().ToLower();
            posX = dt.PosX;
            posY = dt.PosY;
        }

        if (isUc) {
            var advPanel = new Panel { ID = id.Replace("-", "_") + "_advPanel" };
            if ((autoLoad) || (autoOpen) || (!closedState)) {
                try {
                    if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                        UserControl uc = (UserControl)LoadControl("~/Apps/" + filename);
                        uc.ID = "UC" + id.Replace("-", "_");
                        advPanel.Controls.Add(uc);
                    }
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
            appHolder.Controls.Add(advPanel);
        }

        if (((autoOpen) || (!closedState)) && MemberDatabase.IsComplexWorkspaceMode(_member.WorkspaceMode.ToString())) {
            if (!string.IsNullOrEmpty(id)) {
                appScript.Append("openWSE.CreateSOApp('" + id + "','" + title + "',");
                appScript.Append("'" + filename + "','" + posX + "','" + posY);
                appScript.Append("','" + width + "','" + height + "','" + minimized + "','" + maximized + "');");
            }
            else
                _apps.DeleteAppLocal(id);
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            _strScriptreg.Append(appScript);
            appScript.Clear();
        }
    }


    public void AppDivGenerator_NoLogin(string id, string title, string img, int ar, int am, string css, string workspace, string filename, bool isUc, string height, string width, bool allowpopout, string popoutloc, bool autoFullScreen, bool autoLoad, bool autoOpen) {
        var widg = new StringBuilder();
        Panel appHolder = Getworkspace(workspace);

        if (appHolder != null) {
            var tempApps = new App();
            bool hideAllIcons = _ss.HideAllAppIcons;

            if (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAppIcon"])) {
                hideAllIcons = true;
            }

            if (autoFullScreen)
                css += " auto-full-page";

            string minheightMinwidth = string.Empty;
            if (!string.IsNullOrEmpty(height)) {
                if (height != "0")
                    minheightMinwidth += "min-height: " + height + "px;";
            }
            if (!string.IsNullOrEmpty(width)) {
                if (width != "0")
                    minheightMinwidth += "min-width: " + width + "px;";
            }

            if (ar == 1)
                widg.Append("<div data-appid='" + id + "' class='app-main-holder " + css + "' style='display: none;" + minheightMinwidth + "'>");
            else
                widg.Append("<div data-appid='" + id + "' class='app-main-holder " + css + " no-resize' style='display: none;" + minheightMinwidth + "'>");


            #region Build Button Header
            widg.Append("<div class='app-head-button-holder'>");
            widg.Append("<a href='#" + id + "' class='options-button-app' title='View app options'></a>");
            widg.Append("<div class='app-popup-inner-app'>");
            widg.Append("<table><tbody>");

            widg.Append("<tr><td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>");
            widg.Append("<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a>Refresh</li>");
            string popoutClick = "openWSE.PopOutFrame(this,'" + CheckPopoutURL(popoutloc) + "')";
            if ((allowpopout) && (!string.IsNullOrEmpty(popoutloc)))
                widg.Append("<li onclick=\"" + popoutClick + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a>Pop out</li>");

            // widg.Append("<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div>About</li>");
            widg.Append("</ul></td></tr>");


            if (_totalWorkspaces > 1) {
                widg.Append("<tr><td><div class='clear-space'></div><span class='workspace-selector-app-option'>Workspace:</span>");
                widg.Append("<select class='app-options-workspace-switch'>");
                for (int ii = 0; ii < _totalWorkspaces; ii++) {
                    widg.Append("<option>" + (ii + 1).ToString() + "</option>");
                }
                widg.Append("</select></td></tr>");
            }

            widg.Append("</tbody></table></div>");

            widg.Append("<a href='#" + id + "' class='exit-button-app' title='Close'></a>");
            if (am == 1)
                widg.Append("<a href='#" + id + "' class='maximize-button-app' title='Maximize/Normal'></a>");

            widg.Append("<a href='#" + id + "' class='minimize-button-app' title='Minimize'></a>");

            widg.Append("</div>");
            #endregion


            #region App Header
            if (am == 1)
                widg.Append("<div class='app-head app-head-dblclick'>");
            else
                widg.Append("<div class='app-head'>");

            if (!hideAllIcons) {
                string appimgDisplay = "class='app-header-icon'";
                if (!_showappimg)
                    appimgDisplay = "class='app-header-icon display-none'";

                widg.Append("<img alt='" + title + "' src='" + ServerSettings.ResolveUrl("~/Standard_Images/App_Icons/" + img) + "' " + appimgDisplay + " />");
            }

            if (_showapptitle) {
                widg.Append("<span class='app-title'>" + title + "</span>");
            }
            else {
                widg.Append("<span class='app-title display-none'>" + title + "</span>");
            }

            widg.Append("</div>");
            #endregion


            #region App Body
            widg.Append("<div class='app-body'>");
            appHolder.Controls.Add(new LiteralControl(widg.ToString()));
            GetAppsToLoad_NoLogin(isUc, autoLoad, autoOpen, filename, id, ref appHolder, width, height, title);
            appHolder.Controls.Add(new LiteralControl("</div></div>"));
            #endregion
        }
    }
    private void GetAppsToLoad_NoLogin(bool isUc, bool autoLoad, bool autoOpen, string filename, string id, ref Panel appHolder, string width, string height, string title) {
        bool needLoad = false;
        StringBuilder appScript = new StringBuilder();
        if (isUc) {
            var advPanel = new Panel { ID = id.Replace("-", "_") + "_advPanel" };
            if (autoLoad) {
                try {
                    if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                        UserControl uc = (UserControl)LoadControl("~/Apps/" + filename);
                        uc.ID = "UC" + id.Replace("-", "_");
                        advPanel.Controls.Add(uc);

                        if (autoOpen)
                            needLoad = true;
                    }
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }

            appHolder.Controls.Add(advPanel);
        }
        else if (autoOpen)
            needLoad = true;

        if (needLoad && MemberDatabase.IsComplexWorkspaceMode(_demoMemberDatabase["WorkspaceMode"])) {
            appScript.Append("openWSE.CreateSOApp('" + id + "','" + title + "',");
            appScript.Append("'" + filename + "','75px','75px',");
            appScript.Append("'" + width + "','" + height + "','0','0');");
            _strScriptreg.Append(appScript);
        }

        appScript.Clear();
    }
    public string CheckPopoutURL(string loc) {
        int indexof = loc.IndexOf("~/");
        if (indexof == 0) {
            string tempLoc = loc.Substring(2);
            Uri uri = HttpContext.Current.Request.Url;

            if (HttpContext.Current.Request.ApplicationPath[HttpContext.Current.Request.ApplicationPath.Length - 1] != '/') {
                tempLoc = "/" + tempLoc;
            }

            return uri.Scheme + "://" + uri.Authority + HttpContext.Current.Request.ApplicationPath + tempLoc;
        }
        return loc;
    }


    #region Load Chat Apps

    public void LoadSavedChatSessions() {
        if ((_member.ChatEnabled) && (_ss.ChatEnabled)) {
            StringBuilder appScript = new StringBuilder();
            _apps.GetUserSavedChatApps();
            foreach (UserApps_Coll dr in _apps.UserAppList) {
                if (!dr.Closed) {
                    string appId = dr.AppId;
                    string appName = dr.AppName;
                    string currworkspace = dr.Workspace;
                    string width = dr.Width;
                    if (string.IsNullOrEmpty(width))
                        width = "300px";
                    string height = dr.Height;
                    if (string.IsNullOrEmpty(height))
                        height = "400px";
                    string minimized = dr.Minimized.ToString().ToLower();
                    string maximized = dr.Maximized.ToString().ToLower();
                    string posX = dr.PosX;
                    string posY = dr.PosY;

                    string userId = appId.Replace("app-ChatClient-", "");
                    string u = _member.GetUsernameFromUserId(userId);
                    var url = "ChatClient/ChatWindow.html?user=" + u;

                    appScript.Append("openWSE.CreateSOApp('" + appId + "','" + appName + "',");
                    appScript.Append("'" + url + "','" + posX + "','" + posY);
                    appScript.Append("','" + width + "','" + height + "','" + minimized + "','" + maximized + "');");

                    LoadChatApps(currworkspace, appId, u);
                }
            }

            if (!string.IsNullOrEmpty(appScript.ToString())) {
                _strScriptreg.Append(appScript);
                appScript.Clear();
            }
        }
    }
    private void LoadChatApps(string workspace, string id, string chatUser) {
        BlockedChats blockedChats = new BlockedChats(_username);
        blockedChats.BuildEntries();

        MemberDatabase mdata = new MemberDatabase(chatUser);
        if (!HelperMethods.CompareUserGroups(_member, mdata)) return;
        if (blockedChats.CheckIfBlocked(chatUser)) return;

        var widg = new StringBuilder();
        if (string.IsNullOrEmpty(workspace))
            workspace = "workspace_1";

        Panel appHolder = Getworkspace(workspace);
        if (appHolder != null) {
            widg.Append("<div data-appid='" + id + "' class='app-main-holder app-main chat-modal' chat-username='" + chatUser + "' style='display: none; min-height: 400px; min-width: 300px;'>");


            #region Build Button Header
            widg.Append("<div class='app-head-button-holder'>");
            widg.Append("<a href='#" + id + "' class='options-button-app' title='View app options'></a>");
            widg.Append("<div class='app-popup-inner-app'>");
            widg.Append("<table><tbody><tr>");

            widg.Append("<td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>");
            widg.Append("<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a>Refresh</li>");
            string popoutClick = "openWSE.PopOutFrame(this,'" + CheckPopoutURL("~/ExternalAppHolder.aspx?chatuser=" + chatUser) + "');";
            widg.Append("<li onclick=\"" + popoutClick + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a>Pop out</li>");
            widg.Append("<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div>About</li>");
            widg.Append("</ul></td></tr>");

            if (_totalWorkspaces > 1) {
                widg.Append("<tr><td><div class='clear-space'></div><span class='workspace-selector-app-option'>Workspace</span>");
                widg.Append("<select class='app-options-workspace-switch'>");
                for (int i = 0; i < _totalWorkspaces; i++) {
                    string val = (i + 1).ToString();
                    widg.Append("<option value='" + val + "'>" + val + "</option>");
                }
                widg.Append("</select></div></td></tr>");
            }

            widg.Append("</tr></tbody></table></div>");

            widg.Append("<a href='#" + id + "' class='exit-button-app' title='Close'></a>");
            widg.Append("<a href='#" + id + "' class='maximize-button-app' title='Maximize'></a>");
            widg.Append("<a href='#" + id + "' class='minimize-button-app' title='Minimize'></a>");

            widg.Append("</div>");
            #endregion


            #region App Header
            widg.Append("<div class='app-head app-head-dblclick'>");
            widg.Append("<div class='app-header-icon statusUserDiv2 margin-right-sml statusUserOffline'></div>");
            widg.Append("<span class='app-title'>" + HelperMethods.MergeFMLNames(mdata) + "</span>");
            widg.Append("</div>");
            #endregion


            widg.Append("<div class='app-body'></div></div>");
            appHolder.Controls.Add(new LiteralControl(widg.ToString()));
        }
    }

    #endregion


    private Panel Getworkspace(string workspace) {
        Panel d = (Panel)_mainContent.FindControl(workspace);
        if (d == null) {
            d = (Panel)_mainContent.FindControl("workspace_1");
        }
        return d;
    }

    public StringBuilder StrScriptreg {
        get { return _strScriptreg; }
    }
}