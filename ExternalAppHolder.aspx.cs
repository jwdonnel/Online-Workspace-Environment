using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Data;
using System.Globalization;
using System.Text;
using OpenWSE_Tools.AutoUpdates;
using System.IO;
using OpenWSE.Core.Licensing;

public partial class WebControls_ExternalAppHolder : System.Web.UI.Page {
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private App _apps = new App("DemoNoLogin");
    private MemberDatabase _member;
    private string _siteTheme = "Standard";
    private Apps_Coll appInfo;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = Page.User.Identity;
        if ((!userId.IsAuthenticated) && (!_ss.NoLoginRequired)) {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }

        if (!string.IsNullOrEmpty(Request.QueryString["hidetoolbar"]) && HelperMethods.ConvertBitToBoolean(Request.QueryString["hidetoolbar"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#always-visible').hide();");
            _apps = new App("DemoNoLogin");
        }

        if (userId.IsAuthenticated) {
            _member = new MemberDatabase(userId.Name);
            btn_ExternalOpen.Visible = true;
            _apps = new App(userId.Name);
        }

        string appId = Request.QueryString["appId"];
        if (!string.IsNullOrEmpty(appId)) {

            appInfo = _apps.GetAppInformation(appId);
            
            if (appInfo != null && !string.IsNullOrEmpty(appInfo.AppName)) {
                PlaceHolder1.Controls.Clear();

                try {
                    Page.Title = appInfo.AppName;
                    string fileName = appInfo.filename;

                    string appColor = appInfo.AppBackgroundColor;
                    string bgStyle = string.Empty;
                    if (!string.IsNullOrEmpty(appColor) && appColor != "inherit") {
                        if (!appColor.StartsWith("#")) {
                            appColor = "#" + appColor;
                        }

                        bgStyle = " style='background-color: " + appColor + ";'";
                    }

                    if ((!_ss.HideAllAppIcons && _member != null && !_member.HideAppIcon && !string.IsNullOrEmpty(appInfo.Icon)) ||
                        (!_ss.HideAllAppIcons && _member == null && !string.IsNullOrEmpty(appInfo.Icon))) {
                        div_loadingbackground_holder.Style["background-image"] = "url('" + ResolveUrl("~/Standard_Images/App_Icons/" + appInfo.Icon) + "')";
                        LoadFavIcon(appInfo.Icon);
                    }

                    if (!HelperMethods.IsValidHttpBasedAppType(fileName)) {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Extension.ToLower() == ".ascx") {
                            UserControl uc = (UserControl)LoadControl("~/Apps/" + fileName);
                            PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + appId + "' class='no-resize'><div id='container' class='app-main-holder' data-appid='" + appId + "'" + bgStyle + ">"));
                            PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + appId.Replace("app-", string.Empty) + "-load' class='main-div-app-bg'>"));
                            PlaceHolder1.Controls.Add(uc);
                            PlaceHolder1.Controls.Add(new LiteralControl("</div></div></div>"));
                        }
                        else if (fi.Extension.ToLower() == ".aspx") {
                            PlaceHolder1.Controls.Add(new LiteralControl("<div id='container' class='app-main-holder' data-appid='" + appId + "'" + bgStyle + ">"));
                            PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
                            RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').html(\"<iframe class='iFrame-apps' src='" + ResolveUrl("~/Apps/" + fileName) + "' width='100%' frameborder='0'></iframe>\");$(window).resize();");
                        }
                        else {
                            PlaceHolder1.Controls.Add(new LiteralControl("<div id='container' class='app-main-holder' data-appid='" + appId + "'" + bgStyle + ">"));
                            PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
                            RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').load('" + ResolveUrl("~/Apps/" + fileName) + "', function () { $(window).resize(); });");
                        }
                    }
                    else {
                        PlaceHolder1.Controls.Add(new LiteralControl("<div id='container' class='app-main-holder' data-appid='" + appId + "'" + bgStyle + ">"));
                        PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
                        if (HelperMethods.IsValidAspxFile(fileName) || HelperMethods.IsValidHttpBasedAppType(fileName)) {
                            RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').html(\"<iframe class='iFrame-apps' src='" + fileName + "' width='100%' frameborder='0'></iframe>\");$(window).resize();");
                        }
                        else {
                            RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').load('" + fileName + "', function () { $(window).resize(); });");
                        }
                    }

                    LoadDefaultSettings(appId, fileName);
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Error.html");
            }
        }
        else if (!string.IsNullOrEmpty(Request.QueryString["chatuser"])) {
            MemberDatabase chatUser = new MemberDatabase(Request.QueryString["chatuser"]);

            string loc = ResolveUrl("~/ChatClient/ChatWindow.html?user=" + Request.QueryString["chatuser"]);
            PlaceHolder1.Controls.Add(new LiteralControl("<div id='container' class='app-main-holder'>"));
            PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
            LoadDefaultSettings(string.Empty, ".html");

            hf_appId.Value = "app-ChatClient-" + chatUser.UserId;
            Page.Title = HelperMethods.MergeFMLNames(chatUser);

            string iframe = "<iframe class='iFrame-apps' frameborder='0' marginheight='0' marginwidth='0' src='" + loc + "' style='border: 0px;' width='100%'></iframe>";
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').html(\"" + iframe + "\");");
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Error.html");
        }
    }

    private void LoadDefaultSettings(string appId, string fileName) {
        if (!IsPostBack) {
            bool showToolTips = true;
            hf_appId.Value = appId;

            bool showapptitle = true;
            bool showappimg = true;

            if (_member != null) {
                _siteTheme = _member.SiteTheme;
                showToolTips = _member.ShowToolTips;
                showapptitle = _member.ShowAppTitle;
                showappimg = _member.AppHeaderIcon;
            }
            else {
                _siteTheme = "Standard";
                NewUserDefaults userDefaults = new NewUserDefaults("DemoNoLogin");
                userDefaults.GetDefaults();

                if (userDefaults.DefaultTable != null) {
                    if (userDefaults.DefaultTable.ContainsKey("AppHeaderIcon")) {
                        showappimg = HelperMethods.ConvertBitToBoolean(userDefaults.DefaultTable["AppHeaderIcon"]);
                    }

                    if (userDefaults.DefaultTable.ContainsKey("ShowAppTitle")) {
                        showapptitle = HelperMethods.ConvertBitToBoolean(userDefaults.DefaultTable["ShowAppTitle"]);
                    }

                    if (userDefaults.DefaultTable.ContainsKey("Theme")) {
                        _siteTheme = userDefaults.DefaultTable["Theme"];
                    }

                    if (userDefaults.DefaultTable.ContainsKey("ToolTips")) {
                        showToolTips = HelperMethods.ConvertBitToBoolean(userDefaults.DefaultTable["ToolTips"]);
                    }
                }
            }

            LoadTitleAndIcon(showapptitle, showappimg);

            StringBuilder _strScriptreg = new StringBuilder();
            GetStartupScripts_JS();
            GetStartupScripts_CSS();
            GetCustomFonts();

            if (fileName.ToLower().Contains(".ascx")) {
                RegisterAutoScripts();

                string animationSpeed = "150";
                if (_member != null) {
                    animationSpeed = _member.AnimationSpeed.ToString();
                }
                if (!string.IsNullOrEmpty(animationSpeed))
                    _strScriptreg.Append("openWSE_Config.animationSpeed=" + animationSpeed + ";");
            }

            _strScriptreg.Append("openWSE_Config.siteTheme='" + _siteTheme + "';");
            _strScriptreg.Append("openWSE_Config.siteName='" + CheckLicense.SiteName + "';");
            _strScriptreg.Append("openWSE_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';");
            _strScriptreg.Append("openWSE_Config.showToolTips=" + showToolTips.ToString().ToLower() + ";");
            _strScriptreg.Append("openWSE_Config.reportOnError=" + (!_ss.DisableJavascriptErrorAlerts).ToString().ToLower() + ";");
            _strScriptreg.Append("$(window).one('load', function () { $(window).resize(); $('.loading-background-holder').hide(); });");
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }
    }

    private void LoadTitleAndIcon(bool showapptitle, bool showappimg) {
        if (appInfo != null && !string.IsNullOrEmpty(appInfo.AppName)) {
            if (!_ss.HideAllAppIcons && showappimg && !string.IsNullOrEmpty(appInfo.Icon)) {
                string appImg = "<img alt='" + appInfo.AppName + "' src='" + ServerSettings.ResolveUrl("~/Standard_Images/App_Icons/" + appInfo.Icon) + "' />";
                if (showapptitle) {
                    lbl_appName.Text = appImg + "<span class='app-title-ext'>" + appInfo.AppName + "</span><div class='clear'></div>";
                }
                else {
                    lbl_appName.Text = appImg;
                }
                lbl_appName.CssClass = "external-title-with-image";
            }
            else if (!showapptitle && (!showappimg || _ss.HideAllAppIcons)) {
                lbl_appName.Enabled = false;
                lbl_appName.Visible = false;
            }
            else {
                lbl_appName.Text = appInfo.AppName;
                lbl_appName.CssClass = "external-title";
            }
        }
    }

    private void LoadFavIcon(string appIcon) {
        Link1.Href = ResolveUrl("~/Standard_Images/App_Icons/" + appIcon);
        Link2.Href = ResolveUrl("~/Standard_Images/App_Icons/" + appIcon);
    }

    #region Dynamically Load Scripts

    private void GetStartupScripts_JS() {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            if ((coll.ApplyTo == "Base/Workspace") || (coll.ApplyTo == "Workspace Only") || (coll.ApplyTo == "All Components")) {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }
    private void GetStartupScripts_CSS() {
        var startupscripts = new StartupStyleSheets(true);

        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            string scripttheme = coll.Theme;
            if (string.IsNullOrEmpty(scripttheme))
                scripttheme = "Standard";

            if ((scripttheme == _siteTheme) || (scripttheme == "All")) {
                if ((coll.ApplyTo == "Base/Workspace") || (string.IsNullOrEmpty(coll.ApplyTo)) || (coll.ApplyTo == "Workspace Only") || (coll.ApplyTo == "Chat Client") || (coll.ApplyTo == "All Components"))
                    startupscripts.AddCssToPage(coll.ScriptPath, Page);
                else {
                    if (_member != null) {
                        if (_member.UserHasApp(coll.ApplyTo))
                            startupscripts.AddCssToPage(coll.ScriptPath, Page);
                    }
                    else {
                        AppPackages package = new AppPackages(false);
                        NewUserDefaults dc = new NewUserDefaults("DemoNoLogin");
                        string[] appList = package.GetAppList(dc.GetDemoAppPackage);
                        foreach (string x in appList) {
                            if (x == coll.ApplyTo) {
                                startupscripts.AddCssToPage(coll.ScriptPath, Page);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    private void GetCustomFonts() {
        IIdentity _userId = Page.User.Identity;
        if ((_ss.NoLoginRequired) && (!_userId.IsAuthenticated)) {
            NewUserDefaults _demoCustomizations = new NewUserDefaults("DemoNoLogin");
            _demoCustomizations.GetDefaults();
            Dictionary<string, string> _demoMemberDatabase = _demoCustomizations.DefaultTable;
            CustomFonts.SetCustomValues(Page, _demoMemberDatabase);
        }
        else if (_userId.IsAuthenticated && !string.IsNullOrEmpty(_userId.Name)) {
            CustomFonts.SetCustomValues(Page, _member);
        }
        else {
            CustomFonts.SetCustomValues(Page);
        }
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
                    Response.Redirect("~/Workspace.aspx?wi=" + ServerSettings.ServerDateTime.Ticks.ToString(CultureInfo.InvariantCulture));
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
    private void RegisterAutoScripts() {
        AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, "workspace", this);
        aus.StartAutoUpdates();
    }

    #endregion

}