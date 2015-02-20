using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.SessionState;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Notifications;
using System.Security.Principal;
using OpenWSE_Tools.Overlays;
using System.Web.Security;

/// <summary>
/// Summary description for AcctSettings
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AcctSettings : System.Web.Services.WebService {

    public AcctSettings() {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string UpdateSignature(string s) {
        try {
            System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
            if (userId.IsAuthenticated) {
                var member = new MemberDatabase(userId.Name);
                member.UpdateSignature(HttpUtility.UrlDecode(s));
                return "true";
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }

    [WebMethod]
    public object[] GetUserOverlays() {
        List<object> objReturn = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            ServerSettings _ss = new ServerSettings();
            WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();

            bool AssociateWithGroups = _ss.AssociateWithGroups;
            string _username = HttpContext.Current.User.Identity.Name;
            MemberDatabase _member = new MemberDatabase(_username);
            var apps = new App(_username);
            _workspaceOverlays.GetUserOverlays(_username);
            List<string> userApps = _member.EnabledApps;

            List<string> LoadedList = new List<string>();

            if (_member.GroupList.Count > 0) {
                foreach (string w in userApps) {
                    var table = apps.GetAppInformation(w);
                    if (table != null) {
                        string overlayId = table.OverlayID;
                        if (!string.IsNullOrEmpty(overlayId)) {

                            if (AssociateWithGroups) {
                                if (!ServerSettings.CheckAppGroupAssociation(table, _member)) {
                                    continue;
                                }
                            }

                            string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string oId in oIds) {
                                WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                                if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                                    LoadedList.Add(oId);
                                    List<object> newObj = new List<object>();
                                    newObj.Add(coll.OverlayName);
                                    newObj.Add(coll.ID);
                                    newObj.Add(coll.Description);

                                    bool found = false;
                                    foreach (UserOverlay_Coll userOverlays in _workspaceOverlays.UserOverlays) {
                                        if (userOverlays.OverlayID == coll.ID) {
                                            newObj.Add("true");
                                            break;
                                        }
                                    }

                                    if (!found) {
                                        newObj.Add("false");
                                    }
                                    objReturn.Add(newObj.ToArray());
                                }
                            }
                        }
                    }
                }
            }
        }
        return objReturn.ToArray();
    }

    [WebMethod]
    public string[] CheckForEmailAddress() {
        string[] returnVals = new string[2];
        IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            try {
                MembershipUser mu = Membership.GetUser(userID.Name);
                if ((string.IsNullOrEmpty(mu.Email)) || (mu.Email.ToLower() == "n/a")) {
                    returnVals[0] = "true";
                }
                else {
                    returnVals[0] = "false";
                }

                if (userID.Name.ToLower() == ServerSettings.AdminUserName.ToLower() && new MemberDatabase(userID.Name).IsNewMember) {
                    returnVals[1] = "true";
                }
                else {
                    returnVals[1] = "false";
                }
            }
            catch { 
                returnVals[0] = "false";
                returnVals[1] = "false";
            }
        }
        else if (ServerSettings.NeedToLoadAdminNewMemberPage) {
            MembershipUser mu = Membership.GetUser(ServerSettings.AdminUserName);
            if ((string.IsNullOrEmpty(mu.Email)) || (mu.Email.ToLower() == "n/a")) {
                returnVals[0] = "true";
            }
            else {
                returnVals[0] = "false";
            }

            returnVals[1] = "true";
        }
        else {
            returnVals[0] = "false";
            returnVals[1] = "false";
        }

        return returnVals;
    }

    [WebMethod]
    public void OnBrowserClose() {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                string _username = userID.Name;
                MemberDatabase _member = new MemberDatabase(_username);
                ServerSettings _ss = new ServerSettings();
                UserUpdateFlags _uuf = new UserUpdateFlags();
                _uuf.deleteFlag_User(_username);
                if ((_ss.ChatEnabled) && (_member.ChatEnabled)) {
                    if (_member.ChatStatus == "Available") {
                        ChatService cs = new ChatService();
                        cs.UpdateStatus("Away");
                        _member.UpdateIsAway(true);
                    }
                }
            }
        }
        catch { }
    }

    [WebMethod]
    public string AddEmailAddress(string email) {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                System.Web.Security.MembershipUser mu = System.Web.Security.Membership.GetUser(userID.Name);
                email = HttpUtility.UrlDecode(email);
                if (email.Contains("@")) {
                    mu.Email = email;
                    System.Web.Security.Membership.UpdateUser(mu);
                    return "true";
                }
                else {
                    return "false";
                }
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }

    [WebMethod]
    public string UpdateAdminPassword(string password1) {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if ((userID.IsAuthenticated && userID.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) || (ServerSettings.NeedToLoadAdminNewMemberPage)) {
                System.Web.Security.MembershipUser mu = System.Web.Security.Membership.GetUser(ServerSettings.AdminUserName);
                mu.ChangePassword(mu.ResetPassword(), HttpUtility.UrlDecode(password1));
                new MemberDatabase(ServerSettings.AdminUserName).UpdateIsNewMember(false);
                return "true";
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }

    [WebMethod]
    public object[] GetAppRemoteProps(string appId) {
        List<object> returnList = new List<object>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                App apps = new App(userID.Name);
                returnList.Add(apps.GetUserInstalledApp(appId).ToArray());

                MemberDatabase member = new MemberDatabase(userID.Name);
                string db = member.CurrentWorkspace.ToString();

                returnList.Add(db);
            }
        }
        catch { }

        return returnList.ToArray();
    }

    [WebMethod]
    public object[] GetAppHeightWidthPos(string appId) {
        List<object> returnList = new List<object>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                App apps = new App(userID.Name);
                UserApps_Coll dt = apps.GetUserAppInformation(appId);
                returnList.Add(dt.Width);
                returnList.Add(dt.Height);
                returnList.Add(dt.PosX);
                returnList.Add(dt.PosY);
            }
        }
        catch { }

        return returnList.ToArray();
    }

    [WebMethod]
    public object[] GetAllOpenedApps() {
        List<object> returnList = new List<object>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                App apps = new App(userID.Name);
                MemberDatabase member = new MemberDatabase(userID.Name);
                returnList = apps.GetUserOpenedApps(member.ShowWorkspaceNumApp);
            }
        }
        catch { }

        return returnList.ToArray();
    }

    [WebMethod(EnableSession = true)]
    public object[] GetUserGroups() {
        List<object> returnList = new List<object>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                MemberDatabase member = new MemberDatabase(userID.Name);

                List<string> tempList = new List<string>();
                List<object> groupPropList = new List<object>();
                List<string> userGroups = member.GroupList;

                Groups group = new Groups();
                foreach (string g in userGroups) {
                    if (tempList.Contains(g)) {
                        continue;
                    }

                    group.getEntries(g);
                    List<string> groupProp = new List<string>();
                    if (group.group_dt.Count > 0) {
                        Dictionary<string, string> row = group.group_dt[0];
                        groupProp.Add(row["GroupID"]);
                        groupProp.Add(row["GroupName"]);
                        string imgurl = HelperMethods.ConvertBitToBoolean(row["IsURL"]) ? row["Image"] : VirtualPathUtility.ToAbsolute("~/Standard_Images/Groups/Logo/" + row["Image"]);
                        if (imgurl.StartsWith("~/")) {
                            imgurl = VirtualPathUtility.ToAbsolute(imgurl);
                        }
                        groupProp.Add(imgurl);
                        groupPropList.Add(groupProp.ToArray());
                        tempList.Add(g);
                    }
                }

                returnList.Add(groupPropList.ToArray());
                string loggedIn = "false";
                if (HttpContext.Current.Session["LoginGroup"] != null) {
                    loggedIn = "true";
                }
                returnList.Add(loggedIn);
            }
        }
        catch { }

        return returnList.ToArray();
    }

    [WebMethod(EnableSession = true)]
    public void LoginUnderGroup(string id) {
        try {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                if (!string.IsNullOrEmpty(id)) {
                    if (CanLoginToGroup(HttpContext.Current.User.Identity.Name, id)) {
                        HttpContext.Current.Session["LoginGroup"] = id;

                        LoginActivity la = new LoginActivity();
                        la.AddItem(HttpContext.Current.User.Identity.Name, true, ActivityType.Login);
                    }
                }
                else {
                    if (HttpContext.Current.Session["LoginGroup"] != null) {
                        LoginActivity la = new LoginActivity();
                        la.AddItem(HttpContext.Current.User.Identity.Name, true, ActivityType.Logout);

                        HttpContext.Current.Session.Remove("LoginGroup");
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);

            LoginActivity la = new LoginActivity();
            if (!string.IsNullOrEmpty(id)) {
                la.AddItem(HttpContext.Current.User.Identity.Name, false, ActivityType.Login);
            }
            else {
                la.AddItem(HttpContext.Current.User.Identity.Name, false, ActivityType.Logout);
            }
        }
    }
    private bool CanLoginToGroup(string username, string requestGroup) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(requestGroup)) {
                bool groupFound = false;
                Groups group = new Groups();
                group.getEntries();
                List<string> memberGroups = new MemberDatabase(username).GroupList;
                foreach (Dictionary<string, string> g in group.group_dt) {
                    if (g["GroupID"].ToLower() == requestGroup.ToLower()) {
                        groupFound = true;
                        break;
                    }
                }

                if ((groupFound) && (!string.IsNullOrEmpty(requestGroup))) {
                    if (!memberGroups.Contains(requestGroup)) {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    [WebMethod]
    public string GetCurrentWorkspace_Remote() {
        string returnStr = "1";
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                App apps = new App(userID.Name);
                MemberDatabase member = new MemberDatabase(userID.Name);
                returnStr = member.CurrentWorkspace.ToString();
            }
        }
        catch { }

        return returnStr;
    }

    [WebMethod]
    public object[] GetAppRemoteAboutStats(string id, string option) {
        List<object> obj = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            try {
                ServerSettings ss = new ServerSettings();
                System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
                if (userID.IsAuthenticated) {
                    App _apps = new App(userID.Name);
                    if (option.ToLower() == "about") {
                        obj.Add(_apps.BuildAboutApp(id, userID.Name));
                        if (ss.AllowAppRating) {
                            AppRatings ratings = new AppRatings();
                            obj.Add(ratings.GetAverageRating(id));
                            obj.Add(ratings.GetAppRatings(id));
                        }
                    }
                }
                else {
                    obj.Add("Not Authenticated");
                }
            }
            catch {
                obj.Add("Error! Try again.");
            }
        }

        return obj.ToArray();
    }

    [WebMethod]
    public string UpdateAppRemote(string id, string options) {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                MemberDatabase _member = new MemberDatabase(userID.Name);
                _member.UpdateAppRemoteIDAndOptions(id, options);

                UserUpdateFlags _uuf = new UserUpdateFlags();
                string _id = _uuf.getFlag(userID.Name, "appremote");
                if (string.IsNullOrEmpty(_id))
                    _uuf.addFlag(userID.Name, "appremote", "");

                if (CheckIfSent())
                    return "true";
                else
                    return "false";
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }

    int loops = 0;
    private bool CheckIfSent() {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            UserUpdateFlags _uuf = new UserUpdateFlags();
            while (loops < 8) {
                string _id = _uuf.getFlag(userID.Name, "appremote");
                if (string.IsNullOrEmpty(_id))
                    return true;
                else {
                    System.Threading.Thread.Sleep(1000);
                    loops++;
                }
            }

            string id = _uuf.getFlag(userID.Name, "appremote");
            if (!string.IsNullOrEmpty(id))
                _uuf.deleteFlag(id);

        }

        return false;
    }

    [WebMethod]
    public string UpdateAcctNewMember() {
        try {
            System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
            if (userId.IsAuthenticated) {
                var member = new MemberDatabase(userId.Name);
                member.UpdateIsNewMember(false);
                return "true";
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }

    [WebMethod]
    public string[] GetServerImageList(string _workspace) {
        List<string> strReturn = new List<string>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                _workspace = GetRealworkspaceNum(_workspace);
                MemberDatabase member = new MemberDatabase(userID.Name);
                int workspace = 1;
                int.TryParse(_workspace, out workspace);
                if ((workspace == 0) || (!member.MultipleBackgrounds))
                    workspace = 1;

                string background = member.GetBackgroundImg(workspace);

                if (IsValidHttpUri(background) || (background.Length == 6))
                    strReturn.Add(background);
                else
                    strReturn.Add(string.Empty);

                var str = new System.Text.StringBuilder();
                string[] files = System.IO.Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
                Array.Sort(files);

                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);

                foreach (string filename in files) {
                    var fi = new System.IO.FileInfo(filename);
                    if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                        || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                        string filepath = "Standard_Images/Backgrounds/" + fi.Name;
                        string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
                        string size;
                        using (var image = System.Drawing.Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                            size = image.Width + "x" + image.Height;
                        }
                        string c = "pad-all-sml inline-block ";
                        c += background == filepath ? "image-selector-active" : "image-selector";


                        string src = sitePath + "/" + filepath;
                        str.Append("<div class='" + c + "'><img alt='' title='" + size + "' src='" + src + "' class='boxshadow borderShadow2' style='max-width: 180px; max-height:120px;' /></div>");
                    }
                }

                strReturn.Add(str.ToString());
            }
        }
        catch { }

        return strReturn.ToArray();
    }

    [WebMethod]
    public string SaveNewBackground(string _workspace, string _img) {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
                _img = _img.Replace(sitePath + "/", "");
                MemberDatabase member = new MemberDatabase(userID.Name);
                int workspace = 1;
                _workspace = GetRealworkspaceNum(_workspace);
                int.TryParse(_workspace, out workspace);
                if ((workspace == 0) || (!member.MultipleBackgrounds))
                    workspace = 1;

                member.UpdateBackgroundImg(_img, workspace);

                if (_img.Contains(sitePath) || !_img.ToLower().Contains("http://")) {
                    return ServerSettings.ResolveUrl("~/" + _img);
                }

                return _img;
            }
        }
        catch { }

        return "";
    }

    [WebMethod]
    public string ClearProperties() {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                var app = new App(userID.Name);
                app.DeleteUserProperties(userID.Name);
            }
        }
        catch { }

        return "";
    }

    public static string LoadUserBackground(string user, string siteTheme, Control control, string div = "app_title_bg") {
        StringBuilder str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (string.IsNullOrEmpty(siteTheme)) {
                siteTheme = "Standard";
            }
            try {
                MemberDatabase member = new MemberDatabase(user);

                string img = member.GetBackgroundImg(1);
                if (!string.IsNullOrEmpty(img)) {
                    if (img.Length == 6) {
                        str.Append("$('#" + div + "').css('background', '#" + img + "');");
                    }
                    else {
                        if ((!img.Contains("http")) && (!img.Contains("www."))) {
                            img = control.ResolveUrl("~/" + img);
                        }
                        str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + img + "\") repeat top left');");
                    }
                }
                else {
                    str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + control.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + "\") repeat top left');");
                }
            }
            catch {
                str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + control.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + "\") repeat top left');");
            }
        }

        return str.ToString();
    }
    public static string LoadDemoBackground(Dictionary<string, string> _drDefaults, NewUserDefaults defaults, Control control, string div = "app_title_bg") {
        StringBuilder str = new StringBuilder();
        string siteTheme = _drDefaults["Theme"];
        if (string.IsNullOrEmpty(siteTheme)) {
            siteTheme = "Standard";
        }
        try {
            string img = defaults.GetBackgroundImg(1);
            if (!string.IsNullOrEmpty(img)) {
                if (img.Length == 6) {
                    str.Append("$('#" + div + "').css('background', '#" + img + "');");
                }
                else {
                    if ((!img.Contains("http")) && (!img.Contains("www."))) {
                        img = control.ResolveUrl("~/" + img);
                    }
                    str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + img + "\") repeat top left');");
                }
            }
            else {
                str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + control.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + "\") repeat top left');");
            }
        }
        catch {
            str.Append("$('#" + div + "').css('background', '#EFEFEF url(\"" + control.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + "\") repeat top left');");
        }
        return str.ToString();
    }

    public static bool IsValidHttpUri(string uriString) {
        Uri test;
        return Uri.TryCreate(uriString, UriKind.Absolute, out test) && test.Scheme == "http";
    }

    public string GetRealworkspaceNum(string workspace) {
        return workspace.Replace("workspace_", "");
    }

}