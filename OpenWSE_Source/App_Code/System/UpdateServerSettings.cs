using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Web.Security;
using System.Text;
using System.Net.Mail;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class UpdateServerSettings : System.Web.Services.WebService
{
    #region Private variables

    private IIdentity userID;
    private string result = "false";
    private readonly IPListener listener = new IPListener(false);
    private readonly StartupScripts startupscripts = new StartupScripts(false);
    private readonly StartupStyleSheets startupscripts_CSS = new StartupStyleSheets(false);

    #endregion


    public UpdateServerSettings()
    {
        userID = HttpContext.Current.User.Identity;
    }


    #region IP

    [WebMethod]
    public string AddIP(string ip)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(ip)) {
                if (ParseIP(ip)) {
                    if (!listener.CheckIfExists(ip)) {
                        listener.addItem(ip, false, userID.Name);
                        result = "true";
                    }
                    else
                        result = "duplicate";
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string DeleteIP(string ip)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(ip)) {
                if (ParseIP(ip)) {
                    if (listener.CheckIfExists(ip)) {
                        listener.deleteIP(ip);
                        result = "true";
                    }
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string UpdateIP(string ip, string activeIP)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(ip)) {
                if (ParseIP(ip)) {
                    if (listener.CheckIfExists(ip)) {
                        bool active = HelperMethods.ConvertBitToBoolean(activeIP);
                        if ((!active) && (listener.TotalActive > 1)) {
                            if (CurrentAddress == ip) {
                                if (userID.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                    listener.updateActive(ip, active, userID.Name);
                                    result = "true";
                                }
                                else
                                    result = "sameip";
                            }
                            else {
                                listener.updateActive(ip, active, userID.Name);
                                result = "true";
                            }
                        }
                        else {
                            if ((CurrentAddress != ip) &&
                                (!listener.CheckIfActive(CurrentAddress))) {
                                if (userID.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                    listener.updateActive(ip, active, userID.Name);

                                    if (active) {
                                        MembershipUserCollection coll = Membership.GetAllUsers();
                                        foreach (MembershipUser uColl in coll) {
                                            MemberDatabase mb = new MemberDatabase(uColl.UserName);
                                            if ((mb.IpAddress != ip) && (uColl.IsOnline) && (uColl.UserName.ToLower() != userID.Name.ToLower())) {
                                                UserUpdateFlags uFlags = new UserUpdateFlags();
                                                uFlags.addFlag(uColl.UserName, "", "");
                                            }
                                        }
                                    }

                                    result = "true";
                                }
                                else
                                    result = "differentip";
                            }
                            else {
                                listener.updateActive(ip, active, userID.Name);
                                result = "true";
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    #endregion


    #region JS Scripts

    [WebMethod]
    public string AddScriptJS(string jsScript, string applyTo, string sequence)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _scriptAdd = HttpUtility.UrlDecode(jsScript.Trim());
            if (!string.IsNullOrEmpty(_scriptAdd)) {
                if (ParseScript_JS(_scriptAdd)) {
                    if (string.IsNullOrEmpty(applyTo))
                        applyTo = "Base/Workspace";

                    if (!startupscripts.CheckIfExists(_scriptAdd, applyTo)) {
                        startupscripts.addItem(_scriptAdd, Convert.ToInt32(sequence), userID.Name, applyTo);
                        result = "true";
                    }
                    else
                        result = "duplicate";
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string EditScriptJS(string id, string path, string applyTo)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _pathEdit = HttpUtility.UrlDecode(path.Trim());
            string _applyTo = HttpUtility.UrlDecode(applyTo);

            if ((!string.IsNullOrEmpty(_pathEdit)) &&
                (!string.IsNullOrEmpty(id))) {
                startupscripts.updateAppliesTo(id, _applyTo, userID.Name);
                if (ParseScript_JS(_pathEdit)) {
                    string applyto = startupscripts.GetAppliesToFromID(id);
                    //if (!startupscripts.CheckIfExists(_pathEdit, applyto))
                    startupscripts.updateScriptPath(id, _pathEdit, userID.Name);
                    result = "true";
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string DeleteScriptJS(string id)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(id)) {
                startupscripts.deleteStartupScript(id);
                result = "true";
            }
        }
        return result;
    }

    [WebMethod]
    public string UpdateSeqScriptJS(string sequence)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(sequence)) {
                string[] del_js = { "," };
                string[] list_js = sequence.Split(del_js, StringSplitOptions.RemoveEmptyEntries);
                startupscripts.updateSequence_List(list_js, userID.Name);
                result = "true";
            }
        }
        return result;
    }

    #endregion


    #region CSS Scripts

    [WebMethod]
    public string AddScriptCSS(string cssScript, string applyTo, string sequence, string theme)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _scriptAdd_CSS = HttpUtility.UrlDecode(cssScript.Trim());
            if (!string.IsNullOrEmpty(_scriptAdd_CSS)) {
                if (ParseScript_CSS(_scriptAdd_CSS)) {
                    if (string.IsNullOrEmpty(applyTo))
                        applyTo = "Base/Workspace";

                    if (!startupscripts_CSS.CheckIfExists(_scriptAdd_CSS, applyTo)) {
                        startupscripts_CSS.addItem(_scriptAdd_CSS, Convert.ToInt32(sequence), userID.Name, applyTo, theme);
                        result = "true";
                    }
                    else
                        result = "duplicate";
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string EditScriptCSS(string id, string path, string applyTo, string theme)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _pathEdit_CSS = HttpUtility.UrlDecode(path.Trim());
            string _applyTo_CSS = HttpUtility.UrlDecode(applyTo);
            string _theme_CSS = HttpUtility.UrlDecode(theme);

            if ((!string.IsNullOrEmpty(_pathEdit_CSS)) &&
                (!string.IsNullOrEmpty(id))) {
                startupscripts_CSS.updateAppliesTo(id, _applyTo_CSS, userID.Name);
                startupscripts_CSS.updateTheme(id, _theme_CSS, userID.Name);
                if (ParseScript_CSS(_pathEdit_CSS)) {
                    string applyto = startupscripts_CSS.GetAppliesToFromID(id);
                    //if (!startupscripts_CSS.CheckIfExists(_pathEdit_CSS, applyto))
                    startupscripts_CSS.updateScriptPath(id, _pathEdit_CSS, userID.Name);
                    result = "true";
                }
            }
        }
        return result;
    }

    [WebMethod]
    public string DeleteScriptCSS(string id)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(id)) {
                startupscripts_CSS.deleteStartupScript(id);
                result = "true";
            }
        }
        return result;
    }

    [WebMethod]
    public string UpdateSeqScriptCSS(string sequence)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(sequence)) {
                string[] del_js = { "," };
                string[] list_js = sequence.Split(del_js, StringSplitOptions.RemoveEmptyEntries);
                startupscripts_CSS.updateSequence_List(list_js, userID.Name);
                result = "true";
            }
        }
        return result;
    }

    #endregion


    #region Feedback System

    [WebMethod]
    public void SubmitFeedback(string text)
    {
        if (!string.IsNullOrEmpty(text) && text.ToLower() != "enter your comments here...")
        {
            var messagebody = new StringBuilder();

            string currUser = HttpContext.Current.User.Identity.Name;
            if (string.IsNullOrEmpty(currUser))
            {
                currUser = "user";
            }

            messagebody.Append("<b>You have new feedback from a " + currUser + ":</b>");
            messagebody.Append("<br /><br />" + text);
            var message = new MailMessage();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser mu in coll) {
                MemberDatabase memberData = new MemberDatabase(mu.UserName);
                if (memberData.ReceiveAll) {
                    var un = new UserNotificationMessages(mu.UserName);
                    string email = un.attemptAdd("1159aca6-2449-4aff-bacb-5f29e479e2d7", messagebody.ToString(), true);
                    if (!string.IsNullOrEmpty(email))
                        message.To.Add(email);
                }
            }

            if (message.To.Count > 0) {
                UserNotificationMessages.finishAdd(message, "1159aca6-2449-4aff-bacb-5f29e479e2d7", messagebody.ToString(), OpenWSE.Core.Licensing.CheckLicense.SiteName + ": User Feedback");
            }
        }
    }

    #endregion


    #region Support Methods

    private static bool ParseIP(string ipAddress)
    {
        bool isvalid = false;
        try
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            isvalid = true;
        }
        catch { }

        return isvalid;
    }
    private bool ParseScript_JS(string script)
    {
        bool isvalid = false;
        try
        {
            var uri = new Uri(script);

            if (uri.IsFile)
            {
                isvalid = true;
            }
            else if (uri.IsAbsoluteUri)
            {
                isvalid = true;
            }
        }
        catch { }

        if (script.Length > 2)
        {
            if (((script[0] == '/') && (script[1] == '/')) || ((script[0] == '~') && (script[1] == '/')))
                isvalid = true;
        }

        return isvalid;
    }
    private bool ParseScript_CSS(string script)
    {
        bool isvalid = false;
        try
        {
            var uri = new Uri(script);

            if (uri.IsFile)
            {
                isvalid = true;
            }
            else if (uri.IsAbsoluteUri)
            {
                isvalid = true;
            }
        }
        catch { }

        if (script.Length > 2)
        {
            if (((script[0] == '/') && (script[1] == '/')) || ((script[0] == '~') && (script[1] == '/')))
                isvalid = true;
        }

        return isvalid;
    }
    private string CurrentAddress
    {
        get
        {
            NameValueCollection n = HttpContext.Current.Request.ServerVariables;
            string ipaddress = n["REMOTE_ADDR"];
            if (ipaddress == "::1")
            {
                ipaddress = "127.0.0.1";
            }

            return ipaddress;
        }
    }

    #endregion
}