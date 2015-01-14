using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE.Core.Licensing;

/// <summary>
/// Summary description for PageLoadInit
/// </summary>
public class PageLoadInit {

    #region Private Variables

    private ServerSettings _ss = new ServerSettings();
    private readonly IPWatch _ipwatch = new IPWatch(false);
    private readonly IPListener _listener = new IPListener(false);
    private Dictionary<string, string> _demoMemberDatabase;
    private NewUserDefaults _demoCustomizations;
    private MemberDatabase _member;
    private Page _page;
    private IIdentity _userId;
    private bool _IsPostBack;
    private bool _noLoginRequired;
    private HttpRequest _Request;
    private HttpResponse _Response;
    private string _ipAddress;
    private string _sitetheme = "Standard";

    #endregion

    public PageLoadInit() {
    }

    public PageLoadInit(Page page, IIdentity userId, bool IsPostBack, bool noLoginRequired) {
        _page = page;
        _userId = userId;
        _IsPostBack = IsPostBack;
        _noLoginRequired = noLoginRequired;
        _Request = _page.Request;
        _Response = _page.Response;

        _ipAddress = _page.Request.ServerVariables["REMOTE_ADDR"];
        if (_ipAddress == "::1")
            _ipAddress = "127.0.0.1";
    }


    public bool CanLoadPage {
        get {
            // Validate the site license if not vaild
            CheckLicense.ValidateLicense();

            bool isValid = false;

            #region IP Watch and Listener
            if ((_ipwatch.CheckIfBlocked(_ipAddress)) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                if (string.IsNullOrEmpty(_userId.Name))
                    _Response.Redirect("~/Default.aspx");
                else
                    _Response.Redirect("~/ErrorPages/Blocked.html");
            }
            else if ((_ss.SiteOffLine) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                if ((_noLoginRequired) && (!_userId.IsAuthenticated))
                    _Response.Redirect("~/Default.aspx");
                else
                    _Response.Redirect("~/ErrorPages/Maintenance.html");
            }
            else {
                if (!_listener.TableEmpty) {
                    if (_listener.CheckIfActive(_ipAddress))
                        isValid = true;
                    else if (string.IsNullOrEmpty(_userId.Name))
                        _Response.Redirect("~/Default.aspx");
                    else if (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())
                        _Response.Redirect("~/ErrorPages/Blocked.html");
                    else
                        isValid = true;
                }
                else
                    isValid = true;
            }
            #endregion

            if (isValid) {
                if (CheckIfLicenseIsValid()) {
                    if ((_noLoginRequired) && (!_userId.IsAuthenticated)) {
                        _demoCustomizations = new NewUserDefaults("DemoNoLogin");
                        _demoCustomizations.GetDefaults();
                        _demoMemberDatabase = _demoCustomizations.DefaultTable;
                        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                            _sitetheme = _demoMemberDatabase["Theme"];
                        }
                    }
                    else if (_userId.IsAuthenticated && !string.IsNullOrEmpty(_userId.Name)) {
                        _member = new MemberDatabase(_userId.Name);
                        _sitetheme = _member.SiteTheme;
                        RegisterAutoScripts();
                    }
                    else if (!ServerSettings.NeedToLoadAdminNewMemberPage) {
                        _page.Response.Redirect("~/Default.aspx");
                    }
                }

                if (!_page.ToString().ToLower().Contains("appremote")) {
                    RegisterPostbackScripts.RegisterStartupScript(_page, "openWSE_Config.siteRootFolder='" + _page.ResolveUrl("~/").Replace("/", "") + "';");
                }

                if (_page.Master != null) {
                    // Need to load all the scripts in the StartupScripts tables
                    GetStartupScripts_JS();
                    GetStartupScripts_CSS();
                }

                try {
                    if (HttpContext.Current.Session["LoginGroup"] != null) {
                        string sessionGroup = HttpContext.Current.Session["LoginGroup"].ToString();

                        GroupIPListener groupNetwork = new GroupIPListener();

                        Groups groups = new Groups(_userId.Name);
                        MemberDatabase member = new MemberDatabase(_userId.Name);
                        if ((!groups.IsApartOfGroup(member.GroupList, sessionGroup)) || (!groupNetwork.CheckIfActive(sessionGroup, _ipAddress) && groupNetwork.HasAtLeastOneActive(sessionGroup))) {
                            HttpContext.Current.Session.Remove("LoginGroup");
                        }
                    }
                }
                catch { }
            }

            return isValid;
        }
    }
    public void CheckSSLRedirect() {
        if ((!_Request.IsLocal) && (!_Request.IsSecureConnection) && (!_IsPostBack)) {
            Uri httpType = _Request.Url;
            if (_ss.SSLRedirect) {
                if (httpType != null) {
                    if (httpType.Scheme.ToLower() == "http") {
                        string redirectUrl = _Request.Url.ToString().Replace("http:", "https:");
                        if (HelperMethods.UrlIsValid(redirectUrl, _userId.Name))
                            _Response.Redirect(redirectUrl);
                    }
                }
            }
        }
    }
    public string IpAddress {
        get {
            return _ipAddress;
        }
    }

    public bool CheckIfLicenseIsValid() {
        if ((!CheckLicense.IsTrial && !CheckLicense.IsExpired) && (!HelperMethods.ConvertBitToBoolean(_Request.QueryString["purchase"]) || (CheckLicense.LicenseValid && !CheckLicense.TrialActivated))) {
            return true;
        }
        return false;
    }

    public NewUserDefaults DemoCustomizations {
        get {
            return _demoCustomizations;
        }
    }
    public string SiteTheme {
        get {
            return _sitetheme;
        }
    }

    private void RegisterAutoScripts() {
        HiddenField hf_UpdateAll = null;
        if (_page.Master != null) {
            hf_UpdateAll = (HiddenField)_page.Master.FindControl("hf_UpdateAll");
        }
        else {
            hf_UpdateAll = (HiddenField)_page.FindControl("hf_UpdateAll");
        }

        if (hf_UpdateAll != null) {
            AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, "workspace,appremote", _page);
            aus.StartAutoUpdates();
        }
    }

    /// <summary>
    /// Gets the ID of the post back control.
    /// </summary>
    /// <param name = "page">The page.</param>
    /// <returns></returns>
    public string GetPostBackControlId(Page page) {
        if (!page.IsPostBack)
            return string.Empty;

        Control control = null;
        // first we will check the "__EVENTTARGET" because if post back made by the controls
        // which used "_doPostBack" function also available in Request.Form collection.
        string controlName = page.Request.Params["__EVENTTARGET"];
        if (!String.IsNullOrEmpty(controlName)) {
            control = page.FindControl(controlName);
        }
        else {
            // if __EVENTTARGET is null, the control is a button type and we need to
            // iterate over the form collection to find it

            string controlId;
            Control foundControl;

            foreach (string ctl in page.Request.Form) {
                // handle ImageButton they having an additional "quasi-property" 
                // in their Id which identifies mouse x and y coordinates
                if (ctl.EndsWith(".x") || ctl.EndsWith(".y")) {
                    controlId = ctl.Substring(0, ctl.Length - 2);
                    foundControl = page.FindControl(controlId);
                }
                else {
                    foundControl = page.FindControl(ctl);
                }

                if (!(foundControl is System.Web.UI.WebControls.Button || foundControl is System.Web.UI.WebControls.ImageButton)) continue;

                control = foundControl;
                break;
            }
        }

        return control == null ? String.Empty : control.ID;
    }


    #region Dynamically Load Scripts

    private void GetStartupScripts_JS() {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(_page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            if ((coll.ApplyTo == "Base/Workspace") || (coll.ApplyTo == "Workspace Only") || (coll.ApplyTo == "All Components")) {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
            else if ((coll.ApplyTo == "Chat Client") && ((string.IsNullOrEmpty(_page.Request.QueryString["toolview"])) || (!HelperMethods.ConvertBitToBoolean(_page.Request.QueryString["toolview"]))))
                EnableDisableChat(coll.ScriptPath);
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }
    private void GetStartupScripts_CSS() {
        var startupscripts = new StartupStyleSheets(true);

        if (string.IsNullOrEmpty(_sitetheme))
            _sitetheme = "Standard";

        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            string scripttheme = coll.Theme;
            if (string.IsNullOrEmpty(scripttheme))
                scripttheme = "Standard";

            if ((scripttheme == _sitetheme) || (scripttheme == "All")) {
                if ((coll.ApplyTo == "Base/Workspace") || (string.IsNullOrEmpty(coll.ApplyTo)) || (coll.ApplyTo == "Workspace Only") || (coll.ApplyTo == "Chat Client") || (coll.ApplyTo == "All Components"))
                    startupscripts.AddCssToPage(coll.ScriptPath, _page);
                else {
                    if (CheckIfLicenseIsValid()) {
                        if (_member != null) {
                            if (_member.UserHasApp(coll.ApplyTo))
                                startupscripts.AddCssToPage(coll.ScriptPath, _page);
                        }
                        else {
                            AppPackages package = new AppPackages(false);
                            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                                string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);
                                foreach (string x in appList) {
                                    if (x == coll.ApplyTo) {
                                        startupscripts.AddCssToPage(coll.ScriptPath, _page);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void EnableDisableChat(string script) {
        ScriptManager sm = ScriptManager.GetCurrent(_page);
        bool turnOffChat = false;
        if (_ss.ChatEnabled) {
            if (_member != null && _member.ChatEnabled) {
                if (_page.Master != null) {
                    HiddenField hf_chatsound = (HiddenField)_page.Master.FindControl("hf_chatsound");
                    if (hf_chatsound != null) {
                        hf_chatsound.Value = _member.ChatSoundNoti.ToString().ToLower();
                    }

                    var sref = new ScriptReference(script);
                    if (sm != null) sm.Scripts.Add(sref);

                    if (_member.IsAway) {
                        ChatService cs = new ChatService();
                        cs.UpdateStatus("Available");
                    }
                }
            }
            else
                turnOffChat = true;
        }
        else
            turnOffChat = true;

        if (turnOffChat) {
            HtmlGenericControl chat_client_tab_body = (HtmlGenericControl)_page.Master.FindControl("chat_client_tab_body");
            if (chat_client_tab_body != null) {
                chat_client_tab_body.Visible = false;
            }
        }
    }

    #endregion


}