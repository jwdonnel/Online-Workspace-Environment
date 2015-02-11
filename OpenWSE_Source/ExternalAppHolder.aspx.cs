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
    private readonly App _apps = new App();
    private readonly AppLog _applog = new AppLog(false);
    private MemberDatabase _member;
    private string _siteTheme = "Standard";

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = Page.User.Identity;
        if ((!userId.IsAuthenticated) && (!_ss.NoLoginRequired))
            Page.Response.Redirect("~/ErrorPages/Blocked.html");

        if (userId.IsAuthenticated) {
            _member = new MemberDatabase(userId.Name);
        }

        if (!string.IsNullOrEmpty(Request.QueryString["hidetoolbar"]) && HelperMethods.ConvertBitToBoolean(Request.QueryString["hidetoolbar"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#always-visible').hide();");
        }

        string appId = Request.QueryString["appId"];
        if (!string.IsNullOrEmpty(appId)) {

            string appName = _apps.GetAppName(appId);
            if (!string.IsNullOrEmpty(appName)) {

                lbl_appName.Text = appName;
                PlaceHolder1.Controls.Clear();

                try {
                    Page.Title = appName;
                    string fileName = _apps.GetAppFilename(appId);
                    FileInfo fi = new FileInfo(fileName);
                    if (fi.Extension.ToLower() == ".ascx") {
                        UserControl uc = (UserControl)LoadControl("~/Apps/" + fileName);
                        PlaceHolder1.Controls.Add(new LiteralControl("<div id='" + appId + "' class='no-resize'><div id='container' class='app-main-holder'>"));
                        PlaceHolder1.Controls.Add(uc);
                        PlaceHolder1.Controls.Add(new LiteralControl("</div></div>"));
                    }
                    else {
                        PlaceHolder1.Controls.Add(new LiteralControl("<div id='container' class='app-main-holder'>"));
                        PlaceHolder1.Controls.Add(new LiteralControl("</div>"));
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#container').load('" + ResolveUrl("~/Apps/" + fileName) + "', function () { $(window).resize(); });");
                    }

                    LoadDefaultSettings(appId, fi.Extension);
                }
                catch (Exception ex) {
                    _applog.AddError(ex);
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

    private void LoadDefaultSettings(string appId, string fileExt) {
        if (!IsPostBack) {
            hf_appId.Value = appId;
            if (_member != null) {
                _siteTheme = _member.SiteTheme;
            }
            else {
                _siteTheme = "Standard";
            }

            StringBuilder _strScriptreg = new StringBuilder();
            GetStartupScripts_JS();
            GetStartupScripts_CSS();

            if (fileExt.ToLower() == ".ascx") {
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
            _strScriptreg.Append("$(window).one('load', function () { $(window).resize(); $('.loading-background-holder').hide(); });");
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }
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
                    Response.Redirect("~/Workspace.aspx?wi=" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
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