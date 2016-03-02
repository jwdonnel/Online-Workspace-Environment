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
using System.IO;
using System.Drawing;
using System.Web.UI.HtmlControls;

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
    public void TurnOffSiteTipsOnPageLoad() {
        try {
            System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
            if (userId.IsAuthenticated) {
                var member = new MemberDatabase(userId.Name);
                if (!GroupSessions.DoesUserHaveGroupLoginSessionKey(userId.Name)) {
                    member.UpdateSiteTipsOnPageLoad(false);
                }
            }
        }
        catch { }
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

                string sessionId = MemberDatabase.GetUserSessionId(_username);
                if (!string.IsNullOrEmpty(sessionId)) {
                    _uuf.deleteFlag_User_And_SessionId(_username, sessionId);
                }
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
            ServerSettings ss = new ServerSettings();
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                App apps = new App(userID.Name);
                returnList.Add(apps.GetUserInstalledApp(appId).ToArray());

                MemberDatabase member = new MemberDatabase(userID.Name);
                string db = member.CurrentWorkspace.ToString();

                returnList.Add(db);
            }
            else if ((ss.NoLoginRequired) && (!userID.IsAuthenticated)) {
                App apps = new App("DemoNoLogin");
                returnList.Add(apps.GetUserInstalledApp(appId).ToArray());
                returnList.Add("1");
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

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
        catch (Exception e) {
            AppLog.AddError(e);
        }

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
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return returnList.ToArray();
    }

    [WebMethod]
    public object[] GetUserGroups() {
        List<object> returnList = new List<object>();
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;

            Groups group = new Groups();

            List<string> tempList = new List<string>();
            List<object> groupPropList = new List<object>();
            List<string> userGroups = new List<string>();

            string currentGroup = string.Empty;
            string loggedIn = "false";

            if (userID.IsAuthenticated) {
                MemberDatabase member = new MemberDatabase(userID.Name);
                userGroups = member.GetCompleteUserGroupList;
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(userID.Name)) {
                    currentGroup = GroupSessions.GetUserGroupSessionName(userID.Name);
                    loggedIn = "true";
                }
            }
            else {
                userGroups = group.GetEntryList();
            }

            foreach (string g in userGroups) {
                if (tempList.Contains(g) || g == currentGroup) {
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
                    groupProp.Add(row["CreatedBy"]);

                    NewUserDefaults newUserDefaults = new NewUserDefaults(row["GroupID"]);
                    newUserDefaults.GetDefaults();
                    string background = newUserDefaults.GetBackgroundImg(1);
                    if (string.IsNullOrEmpty(background)) {
                        background = "App_Themes/" + newUserDefaults.DefaultTable["Theme"] + "/Body/default-bg.jpg";
                    }
                    else {
                        background = background.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }

                    if (!background.ToLower().StartsWith("http") && !background.ToLower().StartsWith("www")) {
                        background = ServerSettings.ResolveUrl("~/" + background);
                    }

                    groupProp.Add(background);
                    groupPropList.Add(groupProp.ToArray());
                    tempList.Add(g);
                }
            }

            returnList.Add(groupPropList.ToArray());
            returnList.Add(loggedIn);
        }
        catch { }

        return returnList.ToArray();
    }

    [WebMethod]
    public string LoginUnderGroup(string id) {
        try {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                MemberDatabase _member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                if (!string.IsNullOrEmpty(id)) {
                    if (CanLoginToGroup(HttpContext.Current.User.Identity.Name, id)) {
                        GroupSessions.AddOrSetNewGroupLoginSession(HttpContext.Current.User.Identity.Name, id);

                        LoginActivity la = new LoginActivity();
                        la.AddItem(HttpContext.Current.User.Identity.Name, true, ActivityType.Login);
                    }
                }
                else {
                    if (GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
                        LoginActivity la = new LoginActivity();
                        la.AddItem(HttpContext.Current.User.Identity.Name, true, ActivityType.Logout);
                        GroupSessions.RemoveGroupLoginSession(HttpContext.Current.User.Identity.Name);
                    }
                }

                return "true";
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

        return "false";
    }
    private bool CanLoginToGroup(string username, string requestGroup) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(requestGroup)) {
                bool groupFound = false;
                Groups group = new Groups();
                group.getEntries();
                List<string> memberGroups = new MemberDatabase(username).GetCompleteUserGroupList;
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
            loops = 0;
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                MemberDatabase _member = new MemberDatabase(userID.Name);
                _member.UpdateAppRemoteIDAndOptions(id, options);

                UserUpdateFlags _uuf = new UserUpdateFlags();
                string _id = _uuf.getFlag(userID.Name, "appremote");
                if (string.IsNullOrEmpty(_id))
                    _uuf.addFlag(userID.Name, "appremote", "");

                if (CheckIfSent(10, "appremote"))
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

    [WebMethod]
    public string CanConnectToWorkspace() {
        try {
            loops = 0;
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                ServerSettings ss = new ServerSettings();
                if (new MemberDatabase(userID.Name).WorkspaceMode == MemberDatabase.UserWorkspaceMode.Complex && ss.AutoUpdates) {
                    UserUpdateFlags _uuf = new UserUpdateFlags();
                    string _id = _uuf.getFlag(userID.Name, "workspace-main");
                    if (!string.IsNullOrEmpty(_id)) {
                        _uuf.deleteFlag(_id);
                    }

                    _uuf.addFlag(userID.Name, "workspace-main", "");
                    if (CheckIfSent(10, "workspace-main")) {
                        DeleteRemoteConnectFlag(userID.Name);
                        return "true";
                    }
                    else {
                        DeleteRemoteConnectFlag(userID.Name);
                        return "false";
                    }
                }
                else {
                    return "simple";
                }
            }
            else {
                return "false";
            }
        }
        catch { return "false"; }
    }
    private void DeleteRemoteConnectFlag(string username) {
        UserUpdateFlags _uuf = new UserUpdateFlags();
        string sessionId = MemberDatabase.GetUserSessionId(username);
        _uuf.deleteFlag_User_And_SessionId(sessionId, username);
    }

    private int loops = 0;

    private bool CheckIfSent(int loopsToTake, string flag) {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            UserUpdateFlags _uuf = new UserUpdateFlags();

            string _id = string.Empty;

            while (loops < loopsToTake) {
                _id = _uuf.getFlag(userID.Name, flag);
                if (string.IsNullOrEmpty(_id))
                    return true;
                else {
                    System.Threading.Thread.Sleep(1000);
                    loops++;
                }
            }

            if (!string.IsNullOrEmpty(_id))
                _uuf.deleteFlag(_id);

        }

        return false;
    }

    [WebMethod]
    public void CloseAllAppsFromAppRemote() {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                ServerSettings ss = new ServerSettings();

                var app = new App(userID.Name);
                app.DeleteUserProperties(userID.Name);
                MemberDatabase _member = new MemberDatabase(userID.Name);

                if (_member.WorkspaceMode == MemberDatabase.UserWorkspaceMode.Complex && ss.AutoUpdates) {
                    UserUpdateFlags _uuf = new UserUpdateFlags();

                    _member.UpdateAppRemoteIDAndOptions(string.Empty, "close-all");

                    string _id = _uuf.getFlag(userID.Name, "appremote");
                    if (string.IsNullOrEmpty(_id))
                        _uuf.addFlag(userID.Name, "appremote", "");
                }
            }
        }
        catch { }
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
    public string[] GetServerImageList(string _workspace, string folder) {
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
                string url = ServerSettings.ResolveUrl("~/SiteTools/iframes/UserImageUpload.aspx?id=" + member.UserId + "&javascriptpostback=true");
                strReturn.Add(url);
                strReturn.Add(GetServerImages(background, folder));
                strReturn.Add(member.BackgroundPosition);
                strReturn.Add(member.BackgroundSize);
                strReturn.Add(member.BackgroundRepeat.ToString().ToLower());
                strReturn.Add(member.BackgroundColor);
            }
            else {
                strReturn.Add(string.Empty); // iframe
                strReturn.Add(GetServerImages(string.Empty, "public"));
                strReturn.Add("right center");
                strReturn.Add("auto");
                strReturn.Add("1");
                strReturn.Add("FFFFFF");
            }
        }
        catch { }

        return strReturn.ToArray();
    }
    private string GetServerImages(string background, string folder) {
        var str = new System.Text.StringBuilder();

        List<string> backgroundSelected = background.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

        string[] files = System.IO.Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        Array.Sort(files);

        string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);

        if (folder == "public") {
            int totalPublic = 0;
            if (Directory.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds")) {
                string[] fileDir = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
                foreach (string filename in fileDir) {
                    var fi = new FileInfo(filename);
                    if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                        || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                        string filepath = "Standard_Images/Backgrounds/" + fi.Name;
                        string filepath2 = "Standard_Images\\Backgrounds\\" + fi.Name;
                        string size;
                        using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                            size = image.Width + "x" + image.Height;
                        }

                        string c = "pad-all-sml inline-block ";
                        c += backgroundSelected.Contains(filepath) ? "image-selector-active" : "image-selector-acct";
                        str.Append("<div class='image-selector " + c + "'><img alt='' title='" + size + "' src='" + ServerSettings.ResolveUrl("~/" + filepath) + "' data-imgsrc='" + filepath + "' class='boxshadow borderShadow2' /></div>");
                        totalPublic++;
                    }
                }
            }

            if (totalPublic == 0) {
                str.Append("<h3 class='pad-all'>No public images found.</h3>");
            }
        }
        else {
            int totalUploaded = 0;
            if (Directory.Exists(GetUploadedBackgroundDir)) {
                string[] uploadfileDir = Directory.GetFiles(GetUploadedBackgroundDir);
                foreach (string filename in uploadfileDir) {
                    var fi = new FileInfo(filename);
                    if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                        || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {

                        string size;
                        using (var image = Image.FromFile(filename)) {
                            size = image.Width + "x" + image.Height;
                        }

                        string filepath = filename.Replace(ServerSettings.GetServerMapLocation, string.Empty).Replace("\\", "/");

                        string c = "pad-all-sml inline-block ";
                        c += backgroundSelected.Contains(filepath) ? "image-selector-active" : "image-selector-acct";

                        string deleteBtn = "<div class='delete-uploadedimg' data-imgsrc='" + fi.Name + "' title='Delete Image'></div>";
                        str.Append("<div class='user-uploadedimages image-selector " + c + "'>" + deleteBtn + "<img alt='' title='" + size + "' src='" + ServerSettings.ResolveUrl("~/" + filepath) + "' data-imgsrc='" + filepath + "' class='boxshadow borderShadow2' /></div>");
                        totalUploaded++;
                    }
                }
            }

            if (totalUploaded == 0) {
                str.Append("<h3 class='pad-all'>No uploaded images found.</h3>");
            }
        }

        foreach (string b in backgroundSelected) {
            if (b.Length == 6) {
                str.Append("<div class='pad-all-sml inline-block image-selector image-selector-active'><div style='background: #" + b + ";' data-color='" + b + "' class='inline-block boxshadow borderShadow2 color-bg-div'></div></div>");
            }
            else if (IsValidHttpUri(b)) {
                str.Append("<div class='pad-all-sml inline-block image-selector image-selector-active'><img alt='' src='" + b + "' data-imgsrc='" + b + "' class='boxshadow borderShadow2' /></div>");
            }
        }

        return str.ToString();
    }

    private string GetUploadedBackgroundDir {
        get {
            string fullPath = ServerSettings.GetServerMapLocation + "Standard_Images\\AcctImages\\";
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                MemberDatabase tempMember = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                fullPath += tempMember.UserId + "\\UploadedBackgrounds\\";
            }

            return fullPath;
        }
    }

    [WebMethod]
    public void SaveBackgroundSetting(string name, string value) {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            MemberDatabase member = new MemberDatabase(userID.Name);

            switch (name) {
                case "backgroundposition":
                    member.UpdateBackgroundPosition(value);
                    break;
                case "backgroundsize":
                    member.UpdateBackgroundSize(value);
                    break;
                case "backgroundrepeat":
                    member.UpdateBackgroundRepeat(HelperMethods.ConvertBitToBoolean(value));
                    break;
                case "backgroundcolor":
                    member.UpdateBackgroundColor(value);
                    break;
            }
        }
    }

    [WebMethod]
    public object[] DeleteUploadedImage(string _workspace, string _img) {
        List<object> returnStr = new List<object>();
        string currBackgrounds = string.Empty;

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);

            string imageToRemove = GetUploadedBackgroundDir + _img;
            if (!string.IsNullOrEmpty(imageToRemove)) {
                try {
                    if (File.Exists(imageToRemove)) {
                        File.Delete(imageToRemove);
                    }
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }

                imageToRemove = imageToRemove.Replace(ServerSettings.GetServerMapLocation, string.Empty).Replace("\\", "/");

                for (int i = 0; i < member.TotalWorkspaces; i++) {
                    string currImg = member.GetBackgroundImg((i + 1));
                    currBackgrounds = RemoveBackgroundString(currImg, imageToRemove);
                    if (_workspace == (i + 1).ToString() && !string.IsNullOrEmpty(currBackgrounds)) {
                        returnStr.Add(currBackgrounds);
                    }
                    member.UpdateBackgroundImg(currBackgrounds, (i + 1));
                }
            }

            if (returnStr.Count == 0) {
                returnStr.Add("App_Themes/" + member.SiteTheme + "/Body/default-bg.jpg");
            }

            returnStr.Add(member.BackgroundLoopTimer);
            returnStr.Add(GetServerImageList(_workspace, "user")); 
        }

        return returnStr.ToArray();
    }

    [WebMethod]
    public object[] SaveNewBackground(string _workspace, string _img, string folder) {
        List<object> returnStr = new List<object>();
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

                if (string.IsNullOrEmpty(_img)) {
                    member.UpdateBackgroundImg(string.Empty, workspace);
                    returnStr.Add("App_Themes/" + member.SiteTheme + "/Body/default-bg.jpg");
                }
                else {
                    string currImg = member.GetBackgroundImg(workspace);
                    if (HasBackgroundImageInList(currImg, _img)) {
                        currImg = RemoveBackgroundString(currImg, _img);
                    }
                    else {
                        currImg += MemberDatabase.BackgroundSeperator + _img;
                    }
                    member.UpdateBackgroundImg(currImg, workspace);

                    if (string.IsNullOrEmpty(currImg)) {
                        currImg = "App_Themes/" + member.SiteTheme + "/Body/default-bg.jpg";
                    }

                    returnStr.Add(currImg);
                }

                returnStr.Add(member.BackgroundLoopTimer);
                returnStr.Add(GetServerImageList(_workspace, folder)); 
            }
        }
        catch { }

        return returnStr.ToArray();
    }

    private static bool HasBackgroundImageInList(string currentBackagrounds, string image) {
        List<string> images = currentBackagrounds.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (images.Contains(image)) {
            return true;
        }

        return false;
    }

    public  static string RemoveBackgroundString(string currentBackagrounds, string imageToRemove) {
        List<string> images = currentBackagrounds.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (images.Contains(imageToRemove)) {
            images.Remove(imageToRemove);

            return string.Join(MemberDatabase.BackgroundSeperator, images.ToArray());
        }

        return currentBackagrounds;
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

    public static void LoadUserBackground(Page page, NewUserDefaults _demoCustomizations, MemberDatabase _member) {
        string img = string.Empty;
        string siteTheme = "Standard";
        bool multiEnabled = false;
        string timer = "30";
        string workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            siteTheme = _demoCustomizations.DefaultTable["Theme"];
            multiEnabled = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["EnableBackgrounds"]);
            if (_demoCustomizations.DefaultTable.ContainsKey("BackgroundLoopTimer") && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundLoopTimer"])) {
                timer = _demoCustomizations.DefaultTable["BackgroundLoopTimer"];
            }

            workspaceMode = _demoCustomizations.DefaultTable["WorkspaceMode"];
        }
        else if (_member != null) {
            siteTheme = _member.SiteTheme;
            multiEnabled = _member.MultipleBackgrounds;
            timer = _member.BackgroundLoopTimer;
            workspaceMode = _member.WorkspaceMode.ToString();
        }

        if (string.IsNullOrEmpty(workspaceMode)) {
            workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
        }

        Control masterPage = page;
        if (page.Master != null) {
            masterPage = page.Master;
        }

        System.Web.UI.WebControls.Image container_logo = (System.Web.UI.WebControls.Image)page.FindControl("container_logo");
        HtmlGenericControl container = (HtmlGenericControl)masterPage.FindControl("site_mainbody");
        HtmlGenericControl sidebar_backgroundblur = (HtmlGenericControl)masterPage.FindControl("sidebar_backgroundblur");
        HtmlGenericControl alwaysvisible_backgroundblur = (HtmlGenericControl)masterPage.FindControl("alwaysvisible_backgroundblur");

        if (!MemberDatabase.IsComplexWorkspaceMode(workspaceMode) || !page.Request.Url.OriginalString.ToLower().Contains("workspace.aspx")) {
            if (container_logo != null) {
                container_logo.Visible = false;
            }

            if (container != null) {
                if (container.Attributes["class"] == null) {
                    container.Attributes["class"] = "container-main-bg-simple";
                }
                else if (!container.Attributes["class"].Contains("container-main-bg-simple")) {
                    container.Attributes["class"] += "container-main-bg-simple";
                }
            }
        }

        if (container == null) {
            container = (HtmlGenericControl)masterPage.FindControl("app_title_bg");
        }

        int numWorkspaces = 4;
        string backgroundColor = "#FFFFFF";
        string backgroundPosition = "right center";
        string backgroundSize = "auto";
        string backgroundRepeat = "no-repeat";

        bool topbarTransparencyMode = false;
        bool sidebarTransparencyMode = false;

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            int.TryParse(_demoCustomizations.DefaultTable["TotalWorkspaces"], out numWorkspaces);
            backgroundColor = _demoCustomizations.DefaultTable["BackgroundColor"];
            backgroundPosition = _demoCustomizations.DefaultTable["BackgroundPosition"];
            backgroundSize = _demoCustomizations.DefaultTable["BackgroundSize"];
            if (string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundRepeat"]) || HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["BackgroundRepeat"])) {
                backgroundRepeat = "repeat";
            }

            topbarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["TopbarTransparencyMode"]);
            sidebarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["SidebarTransparencyMode"]);
        }
        else if (_member != null) {
            numWorkspaces = _member.TotalWorkspaces;
            backgroundColor = _member.BackgroundColor;
            backgroundPosition = _member.BackgroundPosition;
            backgroundSize = _member.BackgroundSize;
            if (_member.BackgroundRepeat) {
                backgroundRepeat = "repeat";
            }

            topbarTransparencyMode = _member.TopbarTransparencyMode;
            sidebarTransparencyMode = _member.SidebarTransparencyMode;
        }

        if (string.IsNullOrEmpty(backgroundColor)) {
            backgroundColor = "#FFFFFF";
        }

        if (!backgroundColor.StartsWith("#")) {
            backgroundColor = "#" + backgroundColor;
        }

        #region Multi-enabled Backgrounds
        if (multiEnabled) {
            RegisterPostbackScripts.RegisterStartupScript(page, "openWSE_Config.multipleBackgrounds=true;");
            for (int i = 1; i <= numWorkspaces; i++) {
                if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
                    img = _demoCustomizations.GetBackgroundImg(i);
                }
                else if (_member != null) {
                    img = _member.GetBackgroundImg(i);
                }

                System.Web.UI.WebControls.Panel workspace = Getworkspace_BG(i, page);

                if (workspace != null) {
                    List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    string dataBgImg = "";
                    foreach (string bg in backgroundList) {
                        if (!string.IsNullOrEmpty(bg)) {
                            if (bg.Length == 6) {
                                dataBgImg += "#" + bg + MemberDatabase.BackgroundSeperator;
                            }
                            else {
                                if (AcctSettings.IsValidHttpUri(bg)) {
                                    dataBgImg += bg + MemberDatabase.BackgroundSeperator;
                                }
                                else {
                                    dataBgImg += page.ResolveUrl("~/" + bg) + MemberDatabase.BackgroundSeperator;
                                }
                            }
                        }
                        else {
                            dataBgImg += page.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + MemberDatabase.BackgroundSeperator;
                        }
                    }

                    workspace.Attributes["data-backgroundimg"] = dataBgImg;
                }
            }
        }
        #endregion

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            img = _demoCustomizations.GetBackgroundImg(1);
        }
        else if (_member != null) {
            img = _member.GetBackgroundImg(1);
        }

        if (container != null) {
            if ((topbarTransparencyMode || page.Request.Url.OriginalString.ToLower().Contains("acctsettings.aspx")) && alwaysvisible_backgroundblur != null) {
                alwaysvisible_backgroundblur.Visible = true;
            }

            if ((sidebarTransparencyMode || page.Request.Url.OriginalString.ToLower().Contains("acctsettings.aspx")) && sidebar_backgroundblur != null) {
                sidebar_backgroundblur.Visible = true;
            }

            if (!string.IsNullOrEmpty(img)) {
                string tempImg = img;
                List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (backgroundList.Count > 0) {
                    img = backgroundList[0];
                }

                if (img.Length == 6) {
                    container.Style["background-image"] = "";
                    container.Style["background-color"] = "#" + img;

                    if (sidebarTransparencyMode && sidebar_backgroundblur != null) {
                        sidebar_backgroundblur.Style["background-image"] = "";
                        sidebar_backgroundblur.Style["background-color"] = "#" + img;
                    }

                    if (topbarTransparencyMode && alwaysvisible_backgroundblur != null) {
                        alwaysvisible_backgroundblur.Style["background-image"] = "";
                        alwaysvisible_backgroundblur.Style["background-color"] = "#" + img;
                    }
                }
                else {
                    container.Style["background-color"] = backgroundColor;
                    if (AcctSettings.IsValidHttpUri(img)) {
                        container.Style["background-image"] = "url('" + img + "')";
                    }
                    else {
                        container.Style["background-image"] = "url('" + page.ResolveUrl("~/" + img) + "')";
                    }

                    if (sidebarTransparencyMode && sidebar_backgroundblur != null) {
                        sidebar_backgroundblur.Style["background-color"] = container.Style["background-color"];
                        sidebar_backgroundblur.Style["background-image"] = container.Style["background-image"];
                    }

                    if (topbarTransparencyMode && alwaysvisible_backgroundblur != null) {
                        alwaysvisible_backgroundblur.Style["background-color"] = container.Style["background-color"];
                        alwaysvisible_backgroundblur.Style["background-image"] = container.Style["background-image"];
                    }
                }

                RegisterPostbackScripts.RegisterStartupScript(page, "openWSE_Config.backgroundTimerLoop=" + timer + ";openWSE.BackgroundLoop('" + tempImg + "', '#" + container.ClientID + "');");
            }
            else {
                container.Style["background-image"] = "url('" + page.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg") + "')";
                container.Style["background-color"] = backgroundColor;

                if (sidebarTransparencyMode && sidebar_backgroundblur != null) {
                    sidebar_backgroundblur.Style["background-image"] = container.Style["background-image"];
                    sidebar_backgroundblur.Style["background-color"] = backgroundColor;
                }

                if (topbarTransparencyMode && alwaysvisible_backgroundblur != null) {
                    alwaysvisible_backgroundblur.Style["background-image"] = container.Style["background-image"];
                    alwaysvisible_backgroundblur.Style["background-color"] = backgroundColor;
                }
            }

            container.Style["background-size"] = backgroundSize;
            container.Style["background-repeat"] = backgroundRepeat;
            container.Style["background-position"] = backgroundPosition;

            if (sidebarTransparencyMode && sidebar_backgroundblur != null) {
                sidebar_backgroundblur.Style["background-size"] = backgroundSize;
                sidebar_backgroundblur.Style["background-repeat"] = backgroundRepeat;
                sidebar_backgroundblur.Style["background-position"] = backgroundPosition;
            }

            if (topbarTransparencyMode && alwaysvisible_backgroundblur != null) {
                alwaysvisible_backgroundblur.Style["background-size"] = backgroundSize;
                alwaysvisible_backgroundblur.Style["background-repeat"] = backgroundRepeat;
                alwaysvisible_backgroundblur.Style["background-position"] = backgroundPosition;
            }
        }
    }
    public static void LoadUserBackground_ForAcctSettingsPage(Page page, NewUserDefaults _demoCustomizations, MemberDatabase _member) {
        StringBuilder strJS = new StringBuilder();

        string img = string.Empty;
        string siteTheme = "Standard";
        bool multiEnabled = false;
        string timer = "30";
        string workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            siteTheme = _demoCustomizations.DefaultTable["Theme"];
            multiEnabled = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["EnableBackgrounds"]);
            if (_demoCustomizations.DefaultTable.ContainsKey("BackgroundLoopTimer") && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundLoopTimer"])) {
                timer = _demoCustomizations.DefaultTable["BackgroundLoopTimer"];
            }

            workspaceMode = _demoCustomizations.DefaultTable["WorkspaceMode"];
        }
        else if (_member != null) {
            siteTheme = _member.SiteTheme;
            multiEnabled = _member.MultipleBackgrounds;
            timer = _member.BackgroundLoopTimer;
            workspaceMode = _member.WorkspaceMode.ToString();
        }

        if (string.IsNullOrEmpty(workspaceMode)) {
            workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
        }

        string container = "#site_mainbody";
        string sidebar_backgroundblur = "#sidebar_backgroundblur";
        string alwaysvisible_backgroundblur = "#alwaysvisible_backgroundblur";

        if (page.Request.Url.OriginalString.ToLower().Contains("acctsettings.aspx") && !string.IsNullOrEmpty(page.Request.QueryString["u"])) {
            container = "#app_title_bg_acct";
        }

        #region Setup Variables

        int numWorkspaces = 4;
        string backgroundColor = "#FFFFFF";
        string backgroundPosition = "right center";
        string backgroundSize = "auto";
        string backgroundRepeat = "no-repeat";

        bool topbarTransparencyMode = false;
        bool sidebarTransparencyMode = false;

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            int.TryParse(_demoCustomizations.DefaultTable["TotalWorkspaces"], out numWorkspaces);
            backgroundColor = _demoCustomizations.DefaultTable["BackgroundColor"];
            backgroundPosition = _demoCustomizations.DefaultTable["BackgroundPosition"];
            backgroundSize = _demoCustomizations.DefaultTable["BackgroundSize"];
            if (string.IsNullOrEmpty(_demoCustomizations.DefaultTable["BackgroundRepeat"]) || HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["BackgroundRepeat"])) {
                backgroundRepeat = "repeat";
            }

            topbarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["TopbarTransparencyMode"]);
            sidebarTransparencyMode = HelperMethods.ConvertBitToBoolean(_demoCustomizations.DefaultTable["SidebarTransparencyMode"]);
        }
        else if (_member != null) {
            numWorkspaces = _member.TotalWorkspaces;
            backgroundColor = _member.BackgroundColor;
            backgroundPosition = _member.BackgroundPosition;
            backgroundSize = _member.BackgroundSize;
            if (_member.BackgroundRepeat) {
                backgroundRepeat = "repeat";
            }

            topbarTransparencyMode = _member.TopbarTransparencyMode;
            sidebarTransparencyMode = _member.SidebarTransparencyMode;
        }

        if (string.IsNullOrEmpty(backgroundColor)) {
            backgroundColor = "#FFFFFF";
        }

        if (!backgroundColor.StartsWith("#")) {
            backgroundColor = "#" + backgroundColor;
        }

        #endregion

        if (_demoCustomizations != null && _demoCustomizations.DefaultTable != null) {
            img = _demoCustomizations.GetBackgroundImg(1);
        }
        else if (_member != null) {
            img = _member.GetBackgroundImg(1);
        }

        if (!string.IsNullOrEmpty(img)) {
            string tempImg = img;
            List<string> backgroundList = img.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (backgroundList.Count > 0) {
                img = backgroundList[0];
            }

            if (img.Length == 6) {
                strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"\");", container);
                strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"#{1}\");", container, img);

                if (sidebarTransparencyMode) {
                    strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"\");", sidebar_backgroundblur);
                    strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"#{1}\");", sidebar_backgroundblur, img);
                }

                if (topbarTransparencyMode) {
                    strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"\");", alwaysvisible_backgroundblur);
                    strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"#{1}\");", alwaysvisible_backgroundblur, img);
                }
            }
            else {
                string imageLoc = "";
                if (AcctSettings.IsValidHttpUri(img)) {
                    imageLoc = img;
                }
                else {
                    imageLoc = page.ResolveUrl("~/" + img);
                }

                strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", container, backgroundColor);
                strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", container, imageLoc);

                if (sidebarTransparencyMode) {
                    strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", sidebar_backgroundblur, backgroundColor);
                    strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", sidebar_backgroundblur, imageLoc);
                }

                if (topbarTransparencyMode) {
                    strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", alwaysvisible_backgroundblur, backgroundColor);
                    strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", alwaysvisible_backgroundblur, imageLoc);
                }
            }

            strJS.AppendFormat("openWSE_Config.backgroundTimerLoop={0};openWSE.BackgroundLoop('{1}', '{2}');", timer, tempImg, container);
        }
        else {
            strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", container, page.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg"));
            strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", container, backgroundColor);

            if (sidebarTransparencyMode) {
                strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", sidebar_backgroundblur, page.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg"));
                strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", sidebar_backgroundblur, backgroundColor);
            }

            if (topbarTransparencyMode) {
                strJS.AppendFormat("$(\"{0}\").css(\"background-image\", \"url('{1}')\");", alwaysvisible_backgroundblur, page.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg"));
                strJS.AppendFormat("$(\"{0}\").css(\"background-color\", \"{1}\");", alwaysvisible_backgroundblur, backgroundColor);
            }
        }

        strJS.AppendFormat("$(\"{0}\").css(\"background-size\", \"{1}\");", container, backgroundSize);
        strJS.AppendFormat("$(\"{0}\").css(\"background-repeat\", \"{1}\");", container, backgroundRepeat);
        strJS.AppendFormat("$(\"{0}\").css(\"background-position\", \"{1}\");", container, backgroundPosition);

        if (sidebarTransparencyMode) {
            strJS.AppendFormat("$(\"{0}\").css(\"background-size\", \"{1}\");", sidebar_backgroundblur, backgroundSize);
            strJS.AppendFormat("$(\"{0}\").css(\"background-repeat\", \"{1}\");", sidebar_backgroundblur, backgroundRepeat);
            strJS.AppendFormat("$(\"{0}\").css(\"background-position\", \"{1}\");", sidebar_backgroundblur, backgroundPosition);
        }

        if (topbarTransparencyMode) {
            strJS.AppendFormat("$(\"{0}\").css(\"background-size\", \"{1}\");", alwaysvisible_backgroundblur, backgroundSize);
            strJS.AppendFormat("$(\"{0}\").css(\"background-repeat\", \"{1}\");", alwaysvisible_backgroundblur, backgroundRepeat);
            strJS.AppendFormat("$(\"{0}\").css(\"background-position\", \"{1}\");", alwaysvisible_backgroundblur, backgroundPosition);
        }

        if (!string.IsNullOrEmpty(strJS.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(page, strJS.ToString());
        }
    }
    private static System.Web.UI.WebControls.Panel Getworkspace_BG(int workspaceNumber, Page page) {
        System.Web.UI.WebControls.Panel d = (System.Web.UI.WebControls.Panel)page.Master.FindControl("MainContent").FindControl("workspace_" + workspaceNumber);
        if (d == null) {
            d = (System.Web.UI.WebControls.Panel)page.Master.FindControl("MainContent").FindControl("workspace_1");
        }
        return d;
    }

    public static bool IsValidHttpUri(string uriString) {
        Uri test;
        return Uri.TryCreate(uriString, UriKind.Absolute, out test) && test.Scheme == "http";
    }

    public string GetRealworkspaceNum(string workspace) {
        return workspace.Replace("workspace_", "");
    }

}