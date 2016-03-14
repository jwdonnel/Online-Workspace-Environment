using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Principal;
using System.Web.Security;
using System.IO;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Overlays;
using OpenWSE.Core;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class SaveControls : System.Web.Services.WebService {
    #region Private Variables

    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private string _username = "";
    private App _app;
    private MemberDatabase _member;

    #endregion


    public SaveControls() {
        GetSiteRequests.AddRequest();

        IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated) {
            _username = userId.Name;
            _app = new App(_username);
            _member = new MemberDatabase(_username);
        }
        else {
            _app = new App("DemoNoLogin");
        }
    }


    #region App/Icons/Workspace Controls

    [WebMethod]
    public string App_Minimize(string appId, string name, string x, string y, string width, string height, string workspace, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username)) {
                    if (_app.ItemInDatabase(appId))
                        _app.UpdateItem(appId, name, x, y, width, height, false, true, _app.GetAppMax(appId), workspace);
                    else
                        _app.SaveItem(appId, name, x, y, width, height, false, true, false, workspace);
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_Maximize(string appId, string name, string x, string y, string width, string height, string workspace, string ismax, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username)) {
                    if (_app.ItemInDatabase(appId))
                        _app.UpdateMaximized(appId, ismax);
                    else
                        _app.SaveItem(appId, name, x, y, width, height, false, false, true, workspace);
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_Resize(string appId, string width, string height, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username))
                    _app.UpdateSize(appId, width, height);
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_Position(string appId, string posX, string posY, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username))
                    _app.UpdatePosition(appId, posX, posY);
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_Open(string appId, string name, string workspace, string width, string height, string workspaceMode) {
        string result = string.Empty;
        if (_app != null) {
            if (HttpContext.Current.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(_username) && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                bool max = _app.GetAppMax(appId);
                result = _app.ItemInDatabase(appId) ? _app.UpdateItem(appId, name, false, false, max, workspace, width, height) : _app.SaveItem(appId, name, false, false, max, workspace, width, height);
            }
            else
                result = _app.GetAppFilename(appId);
        }

        if (!string.IsNullOrEmpty(result) && !HelperMethods.IsValidHttpBasedAppType(result)) {
            result = ServerSettings.ResolveUrl("~/Apps/" + result);
        }

        return result;
    }

    [WebMethod]
    public void App_Open_NoContent(string appId, string name, string workspace, string width, string height, string workspaceMode) {
        if (_app != null) {
            if (HttpContext.Current.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(_username) && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                bool max = _app.GetAppMax(appId);
                if (_app.ItemInDatabase(appId)) {
                    _app.UpdateItem(appId, name, false, false, max, workspace, width, height);
                }
                else {
                    _app.SaveItem(appId, name, false, false, max, workspace, width, height);
                }
            }
        }
    }

    [WebMethod]
    public void App_Open_ChangeWorkspace(string appId, string name, string workspace, string width, string height, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username)) {
                    bool max = _app.GetAppMax(appId);
                    if (_app.ItemInDatabase(appId)) {
                        _app.UpdateItem(appId, name, false, false, max, workspace, width, height);
                    }
                    else {
                        _app.SaveItem(appId, name, false, false, max, workspace, width, height);
                    }

                    MemberDatabase _member = new MemberDatabase(_username);
                    if ((!string.IsNullOrEmpty(_username)) && (!string.IsNullOrEmpty(workspace))) {
                        workspace = workspace.Replace("workspace_", "");
                        _member.UpdateCurrentWorkspace(Convert.ToInt32(workspace));
                    }
                }
            }
        }
    }

    [WebMethod]
    public string App_Close(string appId, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username)) {
                    _app.DeleteAppLocal(appId, _username);
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_Move(string appId, string name, string x, string y, string width, string height, string workspace, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                if (!string.IsNullOrEmpty(_username)) {
                    bool max = _app.GetAppMax(appId);
                    if (_app.ItemInDatabase(appId))
                        _app.UpdateItem(appId, name, x, y, width, height, false, false, max, workspace);
                    else
                        _app.SaveItem(appId, name, x, y, width, height, false, false, max, workspace);
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_UpdateIcons(string appList) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null) {
                if (!string.IsNullOrEmpty(_username)) {
                    appList = HttpUtility.UrlDecode(appList);
                    if (!string.IsNullOrEmpty(appList))
                        _member.ReorderAppList(appList);
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string App_CurrentWorkspace(string workspace, string workspaceMode) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null && MemberDatabase.IsComplexWorkspaceMode(workspaceMode)) {
                MemberDatabase _member = new MemberDatabase(_username);
                if ((!string.IsNullOrEmpty(_username)) && (!string.IsNullOrEmpty(workspace)))
                    _member.UpdateCurrentWorkspace(Convert.ToInt32(workspace));
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string[] App_RemoteLoad(string _Id) {
        string[] result = new string[3];
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _Id = HttpUtility.UrlDecode(_Id);
            if (_Id != null) {
                UserUpdateFlags _uuf = new UserUpdateFlags();
                _uuf.deleteFlag(_Id);
                string[] wo = _member.AppRemoteIDAndOptions;

                result[0] = wo[0];
                result[1] = wo[1];

                if (result[0] != "workspace-selector") {
                    string filename = _app.GetAppFilename(result[0]);
                    try {
                        FileInfo fi = new FileInfo(filename);
                        if (fi.Extension.ToLower() == ".ascx")
                            result[2] = "true";
                        else
                            result[2] = "false";
                    }
                    catch {
                        result[2] = "false";
                    }
                }
                else
                    result[2] = "false";
            }
        }
        return result;
    }

    #endregion


    #region Overlay Controls

    [WebMethod]
    public string Overlay_Disable(string classes) {
        string overlayId = string.Empty;

        classes = HttpUtility.UrlDecode(classes);
        overlayId = GetOverlayID(classes);

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null) {
                if (!string.IsNullOrEmpty(overlayId))
                    _workspaceOverlays.DeleteUserOverlay(_username, overlayId);
            }
        }
        return overlayId;
    }

    #endregion


    #region User Comment Controls

    [WebMethod]
    public string Comment_UpdateMessage(string comment) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (_app != null) {
                MembershipUser msu = Membership.GetUser(_username);
                if (msu != null) {
                    if (string.IsNullOrEmpty(comment)) {
                        comment = string.Empty;
                    }
                    else {
                        comment = StringEncryption.Encrypt(comment, "@" + new MemberDatabase(_username).UserId.Replace("-", "").Substring(0, 15));
                    }
                    msu.Comment = comment;
                    try {
                        Membership.UpdateUser(msu);
                    }
                    catch (Exception e) {
                        AppLog.AddError(e);
                    }
                }
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string Comment_GetMessage() {
        string result = string.Empty;
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            MembershipUser msu = Membership.GetUser(_username);
            if (msu != null) {
                if (!string.IsNullOrEmpty(msu.Comment)) {
                    result = StringEncryption.Decrypt(msu.Comment, "@" + new MemberDatabase(_username).UserId.Replace("-", "").Substring(0, 15));
                }
                result = HttpUtility.UrlDecode(result);
            }
        }
        return result;
    }

    #endregion


    [WebMethod]
    public string GetTotalHelpPages(string currentPage) {
        int count = 0;

        if (!currentPage.Contains(".") && HttpContext.Current.Request != null) {
            currentPage = HttpContext.Current.Request.Url.AbsolutePath;
        }

        try {
            currentPage = currentPage.Substring(currentPage.LastIndexOf("/") + 1);
            currentPage = currentPage.Replace(currentPage.Substring(currentPage.IndexOf(".")), "");
            currentPage = currentPage.ToLower();

            if (Directory.Exists(ServerSettings.GetServerMapLocation + "HelpPages\\" + currentPage)) {
                DirectoryInfo di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "HelpPages\\" + currentPage);
                FileInfo[] files = di.GetFiles();
                foreach (FileInfo file in files) {
                    if (file.Extension.ToLower() == ".html" && file.Name.ToLower().StartsWith("helppage"))
                        count++;
                }
            }
        }
        catch { }

        return count.ToString();
    }

    [WebMethod]
    public void UpdateAppRating(string rating, string appId, string description) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(appId)) {
                if (description.Length > 500) {
                    description = description.Substring(0, 500);
                }
                AppRatings ratings = new AppRatings(_username);
                ratings.AddRating(appId, rating, description);
            }
        }
    }

    [WebMethod]
    public string GetAppRatingReviews(string appId) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            return _app.GetReviews(appId);
        }

        return string.Empty;
    }

    [WebMethod(EnableSession = true)]
    public void SetCookie(string name, string value) {
        if (User.Identity.IsAuthenticated) {
            try {
                Session[name + "_" + User.Identity.Name.ToLower()] = value;
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    [WebMethod(EnableSession = true)]
    public object GetCookie(string name) {
        if (User.Identity.IsAuthenticated) {
            try {
                object value = Session[name + "_" + User.Identity.Name.ToLower()];
                return value;
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        return null;
    }

    [WebMethod(EnableSession = true)]
    public void DelCookie(string name) {
        if (User.Identity.IsAuthenticated) {
            try {
                if (Session[name + "_" + User.Identity.Name.ToLower()] != null) {
                    Session.Remove(name + "_" + User.Identity.Name.ToLower());
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    #region Private Methods

    private string GetOverlayID(string classes) {
        string id = "";
        string[] delimClasses = { " " };
        string[] classList = classes.Split(delimClasses, StringSplitOptions.RemoveEmptyEntries);

        foreach (string c in classList) {
            Guid outGuid = new Guid();
            if (Guid.TryParse(c.Trim(), out outGuid)) {
                id = outGuid.ToString();
                break;
            }
        }

        return id;
    }

    #endregion

}
