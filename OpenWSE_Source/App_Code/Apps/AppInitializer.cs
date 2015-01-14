using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Data;
using System.Text;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.Security;

namespace OpenWSE_Tools.Apps {

    /// <summary>
    /// Initialize the scripts and style sheets for a app
    /// </summary>
    public class AppInitializer : Page {

        #region Variables

        private readonly IPWatch _ipwatch = new IPWatch(true);
        private readonly StartupScripts _startupscripts = new StartupScripts(true);
        private readonly StartupStyleSheets _startupStyleSheets = new StartupStyleSheets(true);
        private ServerSettings _ss = new ServerSettings();
        private readonly IPListener listener = new IPListener(false);

        private MembershipUser _membershipuser;
        private MemberDatabase _member;
        private App apps = new App();
        private Page _page;
        private IIdentity _userId;

        private string _appName;
        private string _appId;
        private string _sitetheme;
        private string _username;
        private string _secondApplyTo;

        #endregion


        /// <summary>
        /// Create a new instance of a app at load time.
        /// </summary>
        /// <param name="appId">App Id to load</param>
        /// <param name="userName">Current username</param>
        /// <param name="page">Current page to load</param>
        /// <param name="secondApplyTo">If you have another apply to such as Database Import or Custom Tables (Optional)</param>
        public AppInitializer(string appId, string username, Page page, string secondApplyTo = "") {
            _appId = appId;
            _membershipuser = Membership.GetUser(username);
            _member = new MemberDatabase(username);
            _page = page;
            _appName = apps.GetAppName(_appId);
            _username = username;
            _userId = _page.User.Identity;

            _secondApplyTo = secondApplyTo;
            if (string.IsNullOrEmpty(_secondApplyTo))
                _secondApplyTo = _appId;
        }


        /// <summary>
        /// Use before any iframe based app to see if user is authenticated
        /// </summary>
        /// <returns>If member allowed to access app, return true. Else return false</returns>
        public bool TryLoadPageEvent {
            get {
                OpenWSE.Core.Licensing.CheckLicense.ValidateLicense();

                CheckSSLRedirect();

                NameValueCollection n = _page.Request.ServerVariables;
                string ipaddress = n["REMOTE_ADDR"];
                if (ipaddress == "::1")
                    ipaddress = "127.0.0.1";

                if ((_ipwatch.CheckIfBlocked(ipaddress)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower()))
                    _page.Response.Redirect("~/ErrorPages/Blocked.html");

                else if ((_ss.SiteOffLine) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower()))
                    _page.Response.Redirect("~/ErrorPages/Maintenance.html");

                else {
                    if (!listener.TableEmpty) {
                        if (listener.CheckIfActive(ipaddress))
                            return StartUpPage;
                        else {
                            if (_username.ToLower() != ServerSettings.AdminUserName.ToLower())
                                _page.Response.Redirect("~/ErrorPages/Blocked.html");
                            else
                                return StartUpPage;
                        }
                    }
                    else
                        return StartUpPage;
                }

                return false;
            }
        }
        private bool StartUpPage {
            get {
                if ((!_userId.IsAuthenticated) || ((_membershipuser.IsLockedOut) || (!_membershipuser.IsApproved)))
                    _page.Response.Redirect("~/Default.aspx");
                else {
                    _sitetheme = _member.SiteTheme;

                    if (_member.UserHasApp(_appId)) {
                        StringBuilder strJs = new StringBuilder();
                        strJs.Append("try { openWSE_Config.animationSpeed=" + _member.AnimationSpeed + ";");
                        strJs.Append("openWSE_Config.siteTheme='" + _member.SiteTheme + "';");
                        strJs.Append("openWSE_Config.siteRootFolder='" + _page.ResolveUrl("~/").Replace("/", "") + "';");
                        if (OpenWSE_Tools.Notifications.Notifications.CheckIfErrorNotificationIsOn(_username)) {
                            strJs.Append("openWSE_Config.reportAlert=false;");  
                        }
                        strJs.Append("openWSE_Config.siteName='" + OpenWSE.Core.Licensing.CheckLicense.SiteName + "'; }catch (evt) { }");
                        RegisterPostbackScripts.RegisterStartupScript(_page, strJs.ToString());

                        return true;
                    }
                    else
                        _page.Response.Redirect("~/ErrorPages/Blocked.html");
                }

                return false;
            }
        }


        /// <summary>
        /// Check to see if site requires ssl and if so redirect page
        /// </summary>
        private void CheckSSLRedirect() {
            HttpRequest request = _page.Request;
            if (!string.IsNullOrEmpty(request.QueryString["iframe"])) {
                if (!HelperMethods.ConvertBitToBoolean(request.QueryString["iframe"])) {
                    #region SSL Redirect
                    if ((!request.IsLocal) && (!request.IsSecureConnection) && (!IsPostBack)) {
                        Uri httpType = request.Url;
                        if (_ss.SSLRedirect) {
                            if (httpType != null) {
                                if (httpType.Scheme.ToLower() == "http") {
                                    string redirectUrl = request.Url.ToString().Replace("http:", "https:");
                                    if (HelperMethods.UrlIsValid(redirectUrl, _userId.Name))
                                        Response.Redirect(redirectUrl);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }


        /// <summary>
        /// Load all javascript files associated with the current app
        /// </summary>
        /// <param name="ascxPage">False if page is an iframe</param>
        /// <param name="loadFunction">Function call for when the script is loaded into the DOM. (Only if ascxPage == true)</param>
        public void LoadScripts_JS(bool ascxPage, string loadFunction = "") {
            if (ascxPage) {
                foreach (StartupScripts_Coll coll in _startupscripts.StartupscriptsList) {
                    if ((coll.ApplyTo == _appId) || (coll.ApplyTo == _secondApplyTo)) {
                        string sPath = coll.ScriptPath;
                        if (coll.ScriptPath.Length > 0 && coll.ScriptPath[0] == '~') {
                            sPath = ServerSettings.ResolveUrl(sPath);
                        }

                        string script = "$.getScript('" + sPath + "')";
                        if (!string.IsNullOrEmpty(loadFunction)) {
                            if (loadFunction.LastIndexOf(";") == loadFunction.Length - 1)
                                loadFunction = loadFunction.Remove(loadFunction.Length - 1);
                            script += ".done(function( script, textStatus ) { " + loadFunction + "; })";
                        }
                        script += ";";

                        RegisterPostbackScripts.RegisterStartupScript(_page, script);
                    }
                }
            }
            else {
                ScriptManager sm = ScriptManager.GetCurrent(_page);
                foreach (StartupScripts_Coll coll in _startupscripts.StartupscriptsList) {
                    if ((coll.ApplyTo == "Base/Workspace") || (coll.ApplyTo == _appId)
                        || (coll.ApplyTo == "All Components") || (coll.ApplyTo == _secondApplyTo)) {
                        var sref = new ScriptReference(coll.ScriptPath);
                        if (sm != null) sm.Scripts.Add(sref);
                    }
                }
                if (sm != null)
                    sm.ScriptMode = ScriptMode.Auto;
            }
        }


        /// <summary>
        /// Load all style sheets associated with the current app
        /// </summary>
        public void LoadScripts_CSS() {
            string usertheme = _member.SiteTheme;
            if (string.IsNullOrEmpty(usertheme))
                usertheme = "Standard";

            foreach (StartupScriptsSheets_Coll coll in _startupStyleSheets.StartupScriptsSheetsList) {
                string scripttheme = coll.Theme;
                if (string.IsNullOrEmpty(scripttheme))
                    scripttheme = "Standard";

                if ((scripttheme == usertheme) || (scripttheme == "All")) {
                    if ((coll.ApplyTo == "Base/Workspace") || (coll.ApplyTo == _appId)
                        || (coll.ApplyTo == "All Components") || (coll.ApplyTo == _secondApplyTo))
                        _startupStyleSheets.AddCssToPage(coll.ScriptPath, _page);
                }
            }
        }


        /// <summary>
        /// Call to set the app title and image (If available)
        /// </summary>
        /// <param name="lblTitle">App title text page control</param>
        /// <param name="image">App image page control</param>
        /// <param name="iconName">(Optional) Specify a different icon for the app</param>
        public void SetHeaderLabelImage(Label lblTitle, Image image, string iconName = "") {
            string appIcon = apps.GetAppIconName(_appId);
            if (!string.IsNullOrEmpty(iconName))
                appIcon = iconName;

            lblTitle.Text = _appName;

            if (!_ss.HideAllAppIcons) {
                image.Visible = true;
                image.ImageUrl = "~/Standard_Images/App_Icons/" + appIcon;
            }
            else
                image.Visible = false;
        }


        #region Get Variables

        public MemberDatabase memberDatabase {
            get { return _member; }
        }

        public string siteTheme {
            get { return _sitetheme; }
        }

        public string UserName {
            get { return _username; }
        }

        #endregion

    }

}