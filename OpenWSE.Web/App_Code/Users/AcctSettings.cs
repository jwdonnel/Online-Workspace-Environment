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

/// <summary>
/// Summary description for AcctSettings
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AcctSettings : System.Web.Services.WebService {

    public AcctSettings() {
        GetSiteRequests.AddRequest();

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

                MemberDatabase.SetUserUnloadSessionId(_username);

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
                int currWorkspace = member.CurrentWorkspace;
                if (currWorkspace > ss.TotalWorkspacesAllowed) {
                    currWorkspace = ss.TotalWorkspacesAllowed;
                }
                else if (currWorkspace == 0) {
                    currWorkspace = 1;
                }

                returnList.Add(currWorkspace.ToString());
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

            string currentGroupName = string.Empty;
            string currentGroup = string.Empty;
            string loggedIn = "false";

            if (userID.IsAuthenticated) {
                MemberDatabase member = new MemberDatabase(userID.Name);
                userGroups = member.GetCompleteUserGroupList;
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(userID.Name)) {
                    currentGroup = GroupSessions.GetUserGroupSessionName(userID.Name);
                    currentGroupName = group.GetGroupName_byID(currentGroup);
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
                List<object> groupProp = new List<object>();
                if (group.group_dt.Count > 0) {
                    Dictionary<string, string> row = group.group_dt[0];
                    groupProp.Add(row["GroupID"]);
                    groupProp.Add(row["GroupName"]);
                    string imgurl = HelperMethods.ConvertBitToBoolean(row["IsURL"]) ? row["Image"] : VirtualPathUtility.ToAbsolute("~/Standard_Images/Groups/Logo/" + row["Image"]);
                    if (imgurl.StartsWith("~/")) {
                        imgurl = VirtualPathUtility.ToAbsolute(imgurl);
                    }

                    imgurl = HelperMethods.RemoveProtocolFromUrl(imgurl);

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

                    if (!background.StartsWith("//") && !background.ToLower().StartsWith("http") && !background.ToLower().StartsWith("www")) {
                        background = ServerSettings.ResolveUrl("~/" + background);
                    }

                    groupProp.Add(background);

                    AppPackages appPackages = new AppPackages(false);
                    string[] appList = appPackages.GetAppList(newUserDefaults.DefaultTable["AppPackage"]);
                    List<object> appListObj = new List<object>();
                    foreach (string appid in appList) {
                        App tempApp = new App(string.Empty);
                        Apps_Coll appInfo = tempApp.GetAppInformation(appid);
                        if (!string.IsNullOrEmpty(appInfo.AppId)) {
                            appListObj.Add(appInfo);
                        }
                    }

                    groupProp.Add(appListObj);

                    groupPropList.Add(groupProp.ToArray());
                    tempList.Add(g);
                }
            }

            returnList.Add(groupPropList.ToArray());
            returnList.Add(loggedIn);
            returnList.Add(currentGroupName);
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
                ServerSettings ss = new ServerSettings();
                MemberDatabase member = new MemberDatabase(userID.Name);
                int currWorkspace = member.CurrentWorkspace;
                if (currWorkspace > ss.TotalWorkspacesAllowed) {
                    currWorkspace = ss.TotalWorkspacesAllowed;
                }
                else if (currWorkspace == 0) {
                    currWorkspace = 1;
                }

                returnStr = currWorkspace.ToString();
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

                if (CheckIfSent(10, "appremote")) {
                    System.Threading.Thread.Sleep(250);
                    return "true";
                }
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
                MemberDatabase tempMember = new MemberDatabase(userID.Name);
                if (tempMember.WorkspaceMode == MemberDatabase.UserWorkspaceMode.Complex && ss.AutoUpdates) {
                    UserUpdateFlags _uuf = new UserUpdateFlags();
                    string _id = _uuf.getFlag(userID.Name, "workspace-main");
                    if (!string.IsNullOrEmpty(_id)) {
                        _uuf.deleteFlag(_id);
                    }

                    _uuf.addFlag(userID.Name, "workspace-main", "");
                    if (CheckIfSent(10, "workspace-main")) {
                        DeleteRemoteConnectFlag(userID.Name);

                        int currWorkspace = tempMember.CurrentWorkspace;
                        if (currWorkspace > ss.TotalWorkspacesAllowed) {
                            currWorkspace = ss.TotalWorkspacesAllowed;
                        }
                        else if (currWorkspace == 0) {
                            currWorkspace = 1;
                        }
                        return "true~" + currWorkspace.ToString();
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
            _workspace = GetRealworkspaceNum(_workspace);
            int workspace = 1;
            int.TryParse(_workspace, out workspace);
            if (userID.IsAuthenticated) {
                MemberDatabase member = new MemberDatabase(userID.Name);
                if ((workspace == 0) || (!member.MultipleBackgrounds)) {
                    workspace = 1;
                }

                string background = member.GetBackgroundImg(workspace);
                string url = ServerSettings.ResolveUrl("~/SiteTools/iframes/UserBackgroundImageUpload.aspx?id=" + member.UserId + "&javascriptpostback=true");
                strReturn.Add(url);
                strReturn.Add(GetServerImages(background, folder));
                strReturn.Add(member.BackgroundPosition);
                strReturn.Add(member.BackgroundSize);
                strReturn.Add(member.BackgroundRepeat.ToString().ToLower());
                strReturn.Add(member.BackgroundColor);
                strReturn.Add(member.BackgroundLoopTimer);
                strReturn.Add(member.MultipleBackgrounds.ToString().ToLower());
                strReturn.Add(GetUserSelectedImages(background));
            }
            else {
                NewUserDefaults newUserDefaults = new NewUserDefaults("DemoNoLogin");
                newUserDefaults.GetDefaults();
                if ((workspace == 0) || (!HelperMethods.ConvertBitToBoolean(newUserDefaults.DefaultTable["EnableBackgrounds"]))) {
                    workspace = 1;
                }

                string background = newUserDefaults.GetBackgroundImg(workspace);

                strReturn.Add(string.Empty); // iframe
                strReturn.Add(GetServerImages(background, "public"));
                strReturn.Add(newUserDefaults.DefaultTable["BackgroundPosition"]);
                strReturn.Add(newUserDefaults.DefaultTable["BackgroundSize"]);
                strReturn.Add(newUserDefaults.DefaultTable["BackgroundRepeat"]);
                strReturn.Add(newUserDefaults.DefaultTable["BackgroundColor"]);
                strReturn.Add(newUserDefaults.DefaultTable["BackgroundLoopTimer"]);
                strReturn.Add(newUserDefaults.DefaultTable["EnableBackgrounds"]);
                strReturn.Add(string.Empty);
            }
        }
        catch {
            strReturn.Add(string.Empty); // iframe
            strReturn.Add(GetServerImages(string.Empty, "public"));
            strReturn.Add("right center");
            strReturn.Add("auto");
            strReturn.Add("1");
            strReturn.Add("FFFFFF");
            strReturn.Add("30");
            strReturn.Add("0");
            strReturn.Add(string.Empty);
        }

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
                        if (!backgroundSelected.Contains(filepath)) {
                            string size;
                            using (var image = Image.FromFile(ServerSettings.GetServerMapLocation + filepath2)) {
                                size = image.Width + "x" + image.Height;
                            }

                            str.Append("<div class='image-selector'><img alt='' title='" + size + "' src='" + ServerSettings.ResolveUrl("~/" + filepath) + "' data-imgsrc='" + filepath + "' /></div>");
                            totalPublic++;
                        }
                    }
                }
            }

            if (totalPublic == 0) {
                str.Append("<div class='pad-top pad-bottom no-images-found'>No public images available</div>");
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
                        string filepath = filename.Replace(ServerSettings.GetServerMapLocation, string.Empty).Replace("\\", "/");
                        if (!backgroundSelected.Contains(filepath)) {
                            string size;
                            using (var image = Image.FromFile(filename)) {
                                size = image.Width + "x" + image.Height;
                            }

                            str.Append("<div class='image-selector'><div class='delete-uploadedimg' data-imgsrc='" + fi.Name + "' title='Delete Image'></div><img alt='' title='" + size + "' src='" + ServerSettings.ResolveUrl("~/" + filepath) + "' data-imgsrc='" + filepath + "' /></div>");
                            totalUploaded++;
                        }
                    }
                }
            }

            if (totalUploaded == 0) {
                str.Append("<div class='pad-top pad-bottom no-images-found'>No uploaded images available</div>");
            }
        }

        return str.ToString();
    }
    private string GetUserSelectedImages(string background) {
        var str = new System.Text.StringBuilder();
        List<string> backgroundSelected = background.Split(new[] { MemberDatabase.BackgroundSeperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string b in backgroundSelected) {
            string removeBtn = "<div class='remove-selectedimg' title='Remove Image'></div>";
            if (b.Length == 6) {
                str.Append("<div class='image-selector selected'>" + removeBtn + "<div style='background: #" + b + ";' data-color='" + b + "' class='color-bg-div'></div></div>");
            }
            else if (b.StartsWith("#") && b.Length == 7) {
                str.Append("<div class='image-selector selected'>" + removeBtn + "<div style='background: " + b + ";' data-color='" + b.Replace("#", string.Empty) + "' class='color-bg-div'></div></div>");
            }
            else if (IsValidHttpUri(b)) {
                str.Append("<div class='image-selector selected'>" + removeBtn + "<img alt='' src='" + b + "' data-imgsrc='" + b + "' /></div>");
            }
            else {
                str.Append("<div class='image-selector selected'>" + removeBtn + "<img alt='' src='" + ServerSettings.ResolveUrl("~/" + b) + "' data-imgsrc='" + b + "' /></div>");
            }
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            string siteTheme = "Standard";
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                siteTheme = new MemberDatabase(HttpContext.Current.User.Identity.Name).SiteTheme;
            }
            else {
                NewUserDefaults newUserDefaults = new NewUserDefaults("DemoNoLogin");
                siteTheme = newUserDefaults.GetDefault("Theme");
            }

            background = ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Body/default-bg.jpg");
            str.Append("<div class='image-selector-default'>Default<br /><img src='" + background + "' /></div>");
        }

        str.Append("<div class='clear-space'></div>");
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
                case "backgroundlooptimer":
                    member.UpdateBackgroundLoopTimer(value);
                    break;
                case"backgroundindividual":
                    member.UpdateEnableBackgrounds(HelperMethods.ConvertBitToBoolean(value));
                    break;
            }
        }
    }

    [WebMethod]
    public void DeleteUploadedImage(string _img) {
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
                    member.UpdateBackgroundImg(currBackgrounds, (i + 1));
                }
            }
        }
    }

    [WebMethod]
    public void SaveNewBackground(string _workspace, string _img, string folder) {
        try {
            System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
            if (userID.IsAuthenticated) {
                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
                _img = _img.Replace(sitePath + "/", "");
                MemberDatabase member = new MemberDatabase(userID.Name);
                int workspace = 1;
                _workspace = GetRealworkspaceNum(_workspace);
                int.TryParse(_workspace, out workspace);
                if ((workspace == 0) || (!member.MultipleBackgrounds)) {
                    workspace = 1;
                }

                if (string.IsNullOrEmpty(_img)) {
                    member.UpdateBackgroundImg(string.Empty, workspace);
                }
                else {
                    string currImg = member.GetBackgroundImg(workspace);
                    if (HasBackgroundImageInList(currImg, _img)) {
                        currImg = RemoveBackgroundString(currImg, _img);
                    }
                    else {
                        if (_img.Length == 6) {
                            currImg = RemoveBackgroundString(currImg, string.Format("#{0}", _img));
                        }
                        else {
                            currImg += MemberDatabase.BackgroundSeperator + _img;
                        }
                    }
                    member.UpdateBackgroundImg(currImg, workspace);
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
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

    public static bool IsValidHttpUri(string uriString) {
        Uri test;
        return Uri.TryCreate(uriString, UriKind.Absolute, out test) && (test.Scheme == "http" || test.Scheme == "https");
    }

    public string GetRealworkspaceNum(string workspace) {
        return workspace.Replace("workspace_", "");
    }

    [WebMethod]
    public void UpdateColorOption(string option, string color) {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            color = color.Replace("#", string.Empty);
            MemberDatabase member = new MemberDatabase(userID.Name);
            member.UpdateSiteColorOption(option + "~" + color);
        }
    }

    [WebMethod(EnableSession = true)]
    public void SetSidebarContainerState(string state) {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated && HttpContext.Current.Session != null) {
            string sessionKey = userID.Name + "_" + ServerSettings.ApplicationID + "_SidebarState";
            if (string.IsNullOrEmpty(state)) {
                HttpContext.Current.Session[sessionKey] = null;
            }
            else {
                HttpContext.Current.Session[sessionKey] = state;
            }
        }
    }

    [WebMethod]
    public void UpdateLayoutOption(string option) {
        System.Security.Principal.IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            MemberDatabase member = new MemberDatabase(userID.Name);
            member.UpdateSiteLayoutOption(option);
        }
    }

}