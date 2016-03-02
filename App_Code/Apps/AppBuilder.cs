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
    private App _apps;
    private MemberDatabase _member;
    private int _totalWorkspaces = 4;
    private int posAutoOpen = 0;


    public AppBuilder(ContentPlaceHolder MainContent, MemberDatabase member) {
        _mainContent = MainContent;
        _member = member;
        _username = _member.Username;
        _showappimg = _member.AppHeaderIcon;
        _showapptitle = _member.ShowAppTitle;
        _apps = new App(_username);
        _totalWorkspaces = _member.TotalWorkspaces;
        posAutoOpen = 0;
    }

    public AppBuilder(ContentPlaceHolder MainContent, Dictionary<string, string> demoMemberDatabase) {
        _mainContent = MainContent;
        _demoMemberDatabase = demoMemberDatabase;
        _showappimg = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["AppHeaderIcon"]);
        _showapptitle = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["ShowAppTitle"]);
        _apps = new App("DemoNoLogin");

        posAutoOpen = 0;
        int.TryParse(_demoMemberDatabase["TotalWorkspaces"], out _totalWorkspaces);

        int totalAllowed = _ss.TotalWorkspacesAllowed;
        if (_totalWorkspaces > totalAllowed || _totalWorkspaces == 0) {
            _totalWorkspaces = 4;
        }
    }


    /// <summary>
    /// Apps for the Authenticated user
    /// </summary>
    public void AppDivGenerator(string id, string title, string img, int ar, int am, string css, string workspace, string filename, bool isUc, string height, string width, bool allowpopout, string popoutloc, bool autoFullScreen, bool autoLoad, bool autoOpen, string appColor) {
        Panel appHolder = Getworkspace(workspace);

        if (appHolder != null) {
            #region App Settings
            var tempApps = new App(_username);
            bool hideAllIcons = _ss.HideAllAppIcons;
            if (_member.HideAppIcon) {
                hideAllIcons = true;
            }

            if (autoFullScreen) {
                css += " auto-full-page";
            }

            string dataContent = filename;
            if (!string.IsNullOrEmpty(dataContent) && !HelperMethods.IsValidHttpBasedAppType(dataContent)) {
                dataContent = ServerSettings.ResolveUrl("~/Apps/" + dataContent);
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

            string divClass = "app-main-holder";
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_1) {
                divClass += " app-main-style1";
            }
            else if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_2) {
                divClass += " app-main-style2";
            }
            else if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_3) {
                divClass += " app-main-style3";
            }

            var widg = new StringBuilder();
            if (ar == 1 && !autoFullScreen) {
                widg.Append("<div data-appid='" + id + "' data-content='" + dataContent + "' class='" + divClass + " " + css + "' style='display: none;" + minheightMinwidth + "'>");
            }
            else {
                widg.Append("<div data-appid='" + id + "' data-content='" + dataContent + "' class='" + divClass + " " + css + " no-resize' style='display: none;" + minheightMinwidth + "'>");
            }
            #endregion

            appHolder.Controls.Add(new LiteralControl(widg.ToString()));

            #region Style 1
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_1) {
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _member.UserAppStyle.ToString(), hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
                BuildAppBody(false, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
            }
            #endregion

            #region Style 2
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_2) {
                BuildAppBody(false, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _member.UserAppStyle.ToString(), hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
            }
            #endregion

            #region Style 3
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_3) {
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _member.UserAppStyle.ToString(), hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
                BuildAppBody(false, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
            }
            #endregion

            appHolder.Controls.Add(new LiteralControl("</div>"));
        }
    }
    private void GetAppsToLoad(bool isUc, bool autoLoad, bool autoOpen, string filename, string id, string title, string minWidth, string minHeight, Panel appHolder) {
        StringBuilder appScript = new StringBuilder();

        bool closedState = true;
        string width = minWidth;
        string height = minHeight;
        string minimized = string.Empty;
        string maximized = string.Empty;
        string posX = "0";
        string posY = "0";

        UserApps_Coll dt = _apps.GetUserAppInformation(id);
        if (!string.IsNullOrEmpty(dt.ID)) {
            closedState = dt.Closed;
            width = dt.Width;
            height = dt.Height;
            minimized = dt.Minimized.ToString().ToLower();
            maximized = dt.Maximized.ToString().ToLower();
            posX = dt.PosX;
            posY = dt.PosY;
        }
        else if (autoOpen) {
            posAutoOpen += 50;
            posX = posAutoOpen + "px";
            posY = posAutoOpen + "px";
            minimized = "false";
            maximized = "false";
        }

        if (isUc) {
            var advPanel = new Panel { ID = id.Replace("-", "_") + "_advPanel" };
            if ((autoLoad) || (autoOpen) || (!closedState)) {
                try {
                    if (HelperMethods.IsValidAppFileType(filename) && !HelperMethods.IsValidHttpBasedAppType(filename)) {
                        if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                            UserControl uc = (UserControl)LoadControl("~/Apps/" + filename);
                            uc.ID = "UC" + id.Replace("-", "_");
                            advPanel.Controls.Add(new LiteralControl("<div id='" + id.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                            advPanel.Controls.Add(uc);
                            advPanel.Controls.Add(new LiteralControl("</div>"));
                        }
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
                if (!HelperMethods.IsValidHttpBasedAppType(filename)) {
                    filename = ServerSettings.ResolveUrl("~/Apps/" + filename);
                }

                appScript.Append("openWSE.CreateSOApp('" + id + "','" + title + "',");
                appScript.Append("'" + HttpUtility.UrlEncode(filename) + "','" + posX + "','" + posY);
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


    public void AppDivGenerator_NoLogin(string id, string title, string img, int ar, int am, string css, string workspace, string filename, bool isUc, string height, string width, bool allowpopout, string popoutloc, bool autoFullScreen, bool autoLoad, bool autoOpen, string appColor) {
        Panel appHolder = Getworkspace(workspace);

        if (appHolder != null) {
            #region App Settings
            bool hideAllIcons = _ss.HideAllAppIcons;

            if (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAppIcon"])) {
                hideAllIcons = true;
            }

            if (autoFullScreen) {
                css += " auto-full-page";
            }

            string dataContent = filename;
            if (!string.IsNullOrEmpty(dataContent) && !HelperMethods.IsValidHttpBasedAppType(dataContent)) {
                dataContent = ServerSettings.ResolveUrl("~/Apps/" + dataContent);
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

            string divClass = "app-main-holder";
            if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_1.ToString() || string.IsNullOrEmpty(_demoMemberDatabase["UserAppStyle"])) {
                divClass += " app-main-style1";
            }
            else if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_2.ToString()) {
                divClass += " app-main-style2";
            }
            else if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_3.ToString()) {
                divClass += " app-main-style3";
            }

            var widg = new StringBuilder();
            if (ar == 1 && !autoFullScreen) {
                widg.Append("<div data-appid='" + id + "' data-content='" + dataContent + "' class='" + divClass + " " + css + "' style='display: none;" + minheightMinwidth + "'>");
            }
            else {
                widg.Append("<div data-appid='" + id + "' data-content='" + dataContent + "' class='" + divClass + " " + css + " no-resize' style='display: none;" + minheightMinwidth + "'>");
            }
            #endregion

            appHolder.Controls.Add(new LiteralControl(widg.ToString()));

            #region Style 1
            if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_1.ToString() || string.IsNullOrEmpty(_demoMemberDatabase["UserAppStyle"])) {
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _demoMemberDatabase["UserAppStyle"], hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
                BuildAppBody(true, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
            }
            #endregion

            #region Style 2
            if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_2.ToString()) {
                BuildAppBody(true, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _demoMemberDatabase["UserAppStyle"], hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
            }
            #endregion

            #region Style 3
            if (_demoMemberDatabase["UserAppStyle"] == MemberDatabase.AppStyle.Style_3.ToString()) {
                BuildButtonHeader(allowpopout, id, popoutloc, am == 1, autoFullScreen, _demoMemberDatabase["UserAppStyle"], hideAllIcons, title, img, appHolder);
                BuildAppHeader(hideAllIcons, am == 1, autoFullScreen, id, title, img, appHolder);
                BuildAppBody(true, isUc, autoLoad, autoOpen, filename, id, title, width, height, appColor, appHolder);
            }
            #endregion

            appHolder.Controls.Add(new LiteralControl("</div>"));
        }
    }
    private void GetAppsToLoad_NoLogin(bool isUc, bool autoLoad, bool autoOpen, string filename, string id, string title, string width, string height, Panel appHolder) {
        bool needLoad = false;
        StringBuilder appScript = new StringBuilder();
        if (isUc) {
            var advPanel = new Panel { ID = id.Replace("-", "_") + "_advPanel" };
            if (autoLoad || autoOpen) {
                try {
                    if (HelperMethods.IsValidAppFileType(filename) && !HelperMethods.IsValidHttpBasedAppType(filename)) {
                        if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + filename)) {
                            UserControl uc = (UserControl)LoadControl("~/Apps/" + filename);
                            uc.ID = "UC" + id.Replace("-", "_");
                            advPanel.Controls.Add(new LiteralControl("<div id='" + id.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                            advPanel.Controls.Add(uc);
                            advPanel.Controls.Add(new LiteralControl("</div>"));

                            if (autoOpen)
                                needLoad = true;
                        }
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

        string posX = "50";
        string posY = "50";

        if (needLoad) {
            posAutoOpen += 50;
            posX = posAutoOpen + "px";
            posY = posAutoOpen + "px";
            needLoad = true;
        }

        if (needLoad && MemberDatabase.IsComplexWorkspaceMode(_demoMemberDatabase["WorkspaceMode"])) {
            if (!HelperMethods.IsValidHttpBasedAppType(filename)) {
                filename = ServerSettings.ResolveUrl("~/Apps/" + filename);
            }

            appScript.Append("openWSE.CreateSOApp('" + id + "','" + title + "',");
            appScript.Append("'" + HttpUtility.UrlEncode(filename) + "','" + posX + "','" + posY + "',");
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


    #region App Parts

    private void BuildButtonHeader(bool allowpopout, string id, string popoutloc, bool allowMaximize, bool autoFullScreen, string appStyle, bool hideAllIcons, string title, string img, Panel appHolder) {
        StringBuilder buttonHolder = new StringBuilder();
        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString()) {
            buttonHolder.Append("<div class='app-head-hover-button'></div>");
        }

        buttonHolder.Append("<div class='app-head-button-holder'>");

        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString()) {
            buttonHolder.Append("<a href='#" + id + "' class='move-button-app' title='Move app'><span></span></a>");
        }

        buttonHolder.Append("<a href='#" + id + "' class='options-button-app' title='View app options'><span></span></a>");
        buttonHolder.Append("<div class='app-popup-inner-app'>");
        buttonHolder.Append("<table><tbody>");

        buttonHolder.Append("<tr><td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>");
        buttonHolder.Append("<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a><span>Refresh</span></li>");
        string popoutClick = "openWSE.PopOutFrame(this,'" + CheckPopoutURL(popoutloc) + "')";
        if ((allowpopout) && (!string.IsNullOrEmpty(popoutloc))) {
            buttonHolder.Append("<li onclick=\"" + popoutClick + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a><span>Pop out</span></li>");
        }

        buttonHolder.Append("<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div><span>About</span></li>");
        buttonHolder.Append("</ul></td></tr>");


        if (_totalWorkspaces > 1) {
            buttonHolder.Append("<tr><td><div class='clear-space'></div><span class='workspace-selector-app-option'>Workspace:</span>");
            buttonHolder.Append("<select class='app-options-workspace-switch'>");
            for (int ii = 0; ii < _totalWorkspaces; ii++) {
                buttonHolder.Append("<option>" + (ii + 1).ToString() + "</option>");
            }
            buttonHolder.Append("</select></td></tr>");
        }

        buttonHolder.Append("</tbody></table></div>");

        buttonHolder.Append("<a href='#" + id + "' class='exit-button-app' title='Close'><span></span></a>");
        if (allowMaximize && !autoFullScreen) {
            buttonHolder.Append("<a href='#" + id + "' class='maximize-button-app' title='Maximize/Normal'><span></span></a>");
        }

        buttonHolder.Append("<a href='#" + id + "' class='minimize-button-app' title='Minimize'><span></span></a>");

        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString() && ((!hideAllIcons && !string.IsNullOrEmpty(img) && _showappimg) || _showapptitle)) {
            buttonHolder.Append("<div class='app-head-style3'>");

            if (!hideAllIcons && !string.IsNullOrEmpty(img) && _showappimg) {
                buttonHolder.Append("<img alt='" + title + "' src='" + ServerSettings.ResolveUrl("~/Standard_Images/App_Icons/" + img) + "' class='app-header-icon' />");
            }

            if (_showapptitle) {
                buttonHolder.Append("<span class='app-title'>" + title + "</span>");
            }

            buttonHolder.Append("</div>");
        }

        buttonHolder.Append("</div>");

        appHolder.Controls.Add(new LiteralControl(buttonHolder.ToString()));
    }
    private void BuildAppBody(bool isNoLogin, bool isUc, bool autoLoad, bool autoOpen, string filename, string id, string title, string width, string height, string appColor, Panel appHolder) {
        string bgStyle = " style='background-color: #FFF;'";
        if (!string.IsNullOrEmpty(appColor) && appColor != "inherit") {
            if (!appColor.StartsWith("#")) {
                appColor = "#" + appColor;
            }

            bgStyle = " style='background-color: " + appColor + ";'";
        }

        appHolder.Controls.Add(new LiteralControl("<div class='app-body'" + bgStyle + ">"));
        if (!isNoLogin) {
            GetAppsToLoad(isUc, autoLoad, autoOpen, filename, id, title, width, height, appHolder);
        }
        else {
            GetAppsToLoad_NoLogin(isUc, autoLoad, autoOpen, filename, id, title, width, height, appHolder); 
        }
        appHolder.Controls.Add(new LiteralControl("<div class='clear'></div></div>"));
    }
    private void BuildAppHeader(bool hideAllIcons, bool allowMaximize, bool autoFullScreen, string id, string title, string img, Panel appHolder) {
        StringBuilder appHeader = new StringBuilder();

        if (allowMaximize && !autoFullScreen) {
            appHeader.Append("<div class='app-head app-head-dblclick'>");
        }
        else {
            appHeader.Append("<div class='app-head'>");
        }

        if (!hideAllIcons && !string.IsNullOrEmpty(img)) {
            string appimgDisplay = "class='app-header-icon'";
            if (!_showappimg) {
                appimgDisplay = "class='app-header-icon display-none'";
            }

            appHeader.Append("<img alt='" + title + "' src='" + ServerSettings.ResolveUrl("~/Standard_Images/App_Icons/" + img) + "' " + appimgDisplay + " />");
        }

        if (_showapptitle) {
            appHeader.Append("<span class='app-title'>" + title + "</span>");
        }
        else {
            appHeader.Append("<span class='app-title display-none'>" + title + "</span>");
        }

        appHeader.Append("</div>");
        appHolder.Controls.Add(new LiteralControl(appHeader.ToString()));
    }

    #endregion


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
            string divClass = "app-main-holder app-main chat-modal";
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_1) {
                divClass += " app-main-style1";
            }
            else if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_2) {
                divClass += " app-main-style2";
            }
            else if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_3) {
                divClass += " app-main-style3";
            }

            widg.Append("<div data-appid='" + id + "' class='" + divClass + "' chat-username='" + chatUser + "' style='display: none; min-height: 400px; min-width: 300px;'>");
            appHolder.Controls.Add(new LiteralControl(widg.ToString()));

            #region Style 1
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_1) {
                BuildChatButtonHolder(id, chatUser, _member.UserAppStyle.ToString(), mdata, appHolder);
                BuildChatHeader(mdata, appHolder);
                BuildChatBody(appHolder);
            }
            #endregion

            #region Style 2
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_2) {
                BuildChatBody(appHolder);
                BuildChatButtonHolder(id, chatUser, _member.UserAppStyle.ToString(), mdata, appHolder);
                BuildChatHeader(mdata, appHolder);
            }
            #endregion

            #region Style 3
            if (_member.UserAppStyle == MemberDatabase.AppStyle.Style_3) {
                BuildChatButtonHolder(id, chatUser, _member.UserAppStyle.ToString(), mdata, appHolder);
                // BuildChatHeader(mdata, appHolder);
                BuildChatBody(appHolder);
            }
            #endregion

            appHolder.Controls.Add(new LiteralControl("</div>"));
        }
    }

    private void BuildChatButtonHolder(string id, string chatUser, string appStyle, MemberDatabase mdata, Panel appHolder) {
        StringBuilder widg = new StringBuilder();
        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString()) {
            widg.Append("<div class='app-head-hover-button'></div>");
        }

        widg.Append("<div class='app-head-button-holder'>");
        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString()) {
            widg.Append("<a href='#" + id + "' class='move-button-app' title='Move app'><span></span></a>");
        }
        widg.Append("<a href='#" + id + "' class='options-button-app' title='View app options'><span></span></a>");
        widg.Append("<div class='app-popup-inner-app'>");
        widg.Append("<table><tbody><tr>");

        widg.Append("<td valign='top'><h3>App Options</h3><div class='clear-space-five'></div><ul>");
        widg.Append("<li onclick='openWSE.ReloadApp(this)' title='Refresh'><a href='#" + id + "' class='reload-button-app'></a><span>Refresh</span></li>");
        string popoutClick = "openWSE.PopOutFrame(this,'" + CheckPopoutURL("~/ExternalAppHolder.aspx?chatuser=" + chatUser) + "');";
        widg.Append("<li onclick=\"" + popoutClick + "\" title='Pop Out'><a href='#" + id + "' class='popout-button-app'></a><span>Pop out</span></li>");
        widg.Append("<li onclick='openWSE.AboutApp(this)' title='About App'><div class='about-app'></div><span>About</span></li>");
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

        widg.Append("<a href='#" + id + "' class='exit-button-app' title='Close'><span></span></a>");
        widg.Append("<a href='#" + id + "' class='maximize-button-app' title='Maximize'><span></span></a>");
        widg.Append("<a href='#" + id + "' class='minimize-button-app' title='Minimize'><span></span></a>");

        if (appStyle == MemberDatabase.AppStyle.Style_3.ToString()) {
            widg.Append("<div class='app-head app-head-style3'>");
            widg.Append("<div class='app-header-icon statusUserDiv2 margin-right-sml statusUserOffline'></div>");
            widg.Append("<span class='app-title'>" + HelperMethods.MergeFMLNames(mdata) + "</span>");
            widg.Append("</div>");
        }

        widg.Append("</div>");
        appHolder.Controls.Add(new LiteralControl(widg.ToString()));
    }
    private void BuildChatHeader(MemberDatabase mdata, Panel appHolder) {
        StringBuilder widg = new StringBuilder();
        widg.Append("<div class='app-head app-head-dblclick'>");
        widg.Append("<div class='app-header-icon statusUserDiv2 margin-right-sml statusUserOffline'></div>");
        widg.Append("<span class='app-title'>" + HelperMethods.MergeFMLNames(mdata) + "</span>");
        widg.Append("</div>");
        appHolder.Controls.Add(new LiteralControl(widg.ToString()));
    }
    private void BuildChatBody(Panel appHolder) {
        appHolder.Controls.Add(new LiteralControl("<div class='app-body'></div>"));
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