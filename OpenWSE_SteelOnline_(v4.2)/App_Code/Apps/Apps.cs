#region

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Web.Security;
using System.Data.SqlServerCe;
using OpenWSE_Tools.Overlays;

#endregion

[Serializable]
public class Apps_Coll {
    private readonly string _id;
    private readonly string _appId;
    private readonly string _name;
    private readonly string _filename;
    private readonly string _icon;
    private readonly bool _allowResize;
    private readonly bool _allowMaximize;
    private readonly string _about;
    private readonly string _description;
    private readonly string _cssClass;
    private readonly bool _autoLoad;
    private readonly bool _autoFullScreen;
    private readonly string _category;
    private readonly string _minHeight;
    private readonly string _minWidth;
    private readonly string _createdBy;
    private readonly bool _allowParams;
    private readonly bool _allowPopOut;
    private readonly string _popOutLoc;
    private readonly string _overlayID;
    private readonly string _notificationID;
    private readonly bool _displayNav;
    private readonly bool _allowStats;
    private readonly bool _autoOpen;
    private readonly string _defaultWorkspace;
    private readonly bool _isPrivate;

    public Apps_Coll() { }
    public Apps_Coll(string id, string appId, string name, string filename, string icon, string allowResize, string allowMaximize, string about, string description, string cssClass, string autoLoad, string autoFullScreen, string category, string minHeight, string minWidth, string createdBy, string allowParams, string allowPopOut, string popOutLoc, string overlayID, string notificationID, string displayNav, string allowStats, string autoOpen, string defaultWorkspace, string isPrivate) {
        _id = id;
        _appId = appId;
        _name = name;
        _filename = filename;
        _icon = icon;
        _allowResize = HelperMethods.ConvertBitToBoolean(allowResize);
        _allowMaximize = HelperMethods.ConvertBitToBoolean(allowMaximize);
        _about = about;
        _description = description;
        _cssClass = cssClass;
        _autoLoad = HelperMethods.ConvertBitToBoolean(autoLoad);
        _autoFullScreen = HelperMethods.ConvertBitToBoolean(autoFullScreen);
        _category = category;
        _minHeight = minHeight;
        _minWidth = minWidth;
        _createdBy = createdBy;
        _allowParams = HelperMethods.ConvertBitToBoolean(allowParams);
        _allowPopOut = HelperMethods.ConvertBitToBoolean(allowPopOut);
        _popOutLoc = popOutLoc;
        _overlayID = overlayID;
        _notificationID = notificationID;
        _displayNav = HelperMethods.ConvertBitToBoolean(displayNav);
        _allowStats = HelperMethods.ConvertBitToBoolean(allowStats);
        _autoOpen = HelperMethods.ConvertBitToBoolean(autoOpen);
        _defaultWorkspace = defaultWorkspace;
        _isPrivate = HelperMethods.ConvertBitToBoolean(isPrivate);
    }

    public string ID {
        get { return _id; }
    }

    public string AppId {
        get { return _appId; }
    }

    public string AppName {
        get { return _name; }
    }

    public string filename {
        get { return _filename; }
    }

    public string Icon {
        get { return _icon; }
    }

    public bool AllowResize {
        get { return _allowResize; }
    }

    public bool AllowMaximize {
        get { return _allowMaximize; }
    }

    public string About {
        get { return _about; }
    }

    public string Description {
        get { return _description; }
    }

    public string CssClass {
        get { return _cssClass; }
    }

    public bool AutoLoad {
        get { return _autoLoad; }
    }

    public bool AutoFullScreen {
        get { return _autoFullScreen; }
    }

    public string Category {
        get { return _category; }
    }

    public string MinHeight {
        get { return _minHeight; }
    }

    public string MinWidth {
        get { return _minWidth; }
    }

    public string CreatedBy {
        get { return _createdBy; }
    }

    public bool AllowParams {
        get { return _allowParams; }
    }

    public bool AllowPopOut {
        get { return _allowPopOut; }
    }

    public string PopOutLoc {
        get { return _popOutLoc; }
    }

    public string OverlayID {
        get { return _overlayID; }
    }

    public string NotificationID {
        get { return _notificationID; }
    }

    public bool DisplayNav {
        get { return _displayNav; }
    }

    public bool AllowStats {
        get { return _allowStats; }
    }

    public bool AutoOpen {
        get { return _autoOpen; }
    }

    public string DefaultWorkspace {
        get { return _defaultWorkspace; }
    }

    public bool IsPrivate {
        get { return _isPrivate; }
    }
}

[Serializable]
public class UserApps_Coll {
    private readonly string _id;
    private readonly string _userName;
    private readonly string _appId;
    private readonly string _appName;
    private readonly string _posX;
    private readonly string _posY;
    private readonly string _width;
    private readonly string _height;
    private readonly bool _closed;
    private readonly bool _minimized;
    private readonly bool _maximized;
    private readonly string _workSpace;
    private DateTime _dateTime = DateTime.Now;

    public UserApps_Coll() { }
    public UserApps_Coll(string id, string userName, string appId, string appName, string posX, string posY, string width, string height, string closed, string minimized, string maximized, string workSpace, string dateTime) {
        _id = id;
        _userName = userName;
        _appId = appId;
        _appName = appName;
        _posX = posX;
        _posY = posY;
        _width = width;
        _height = height;
        _closed = HelperMethods.ConvertBitToBoolean(closed);
        _minimized = HelperMethods.ConvertBitToBoolean(minimized);
        _maximized = HelperMethods.ConvertBitToBoolean(maximized);
        _workSpace = workSpace;
        DateTime.TryParse(dateTime, out _dateTime);
    }

    public string ID {
        get { return _id; }
    }

    public string Username {
        get { return _userName; }
    }

    public string AppId {
        get { return _appId; }
    }

    public string AppName {
        get { return _appName; }
    }

    public string PosX {
        get { return _posX; }
    }

    public string PosY {
        get { return _posY; }
    }

    public string Width {
        get { return _width; }
    }

    public string Height {
        get { return _height; }
    }

    public bool Closed {
        get { return _closed; }
    }

    public bool Minimized {
        get { return _minimized; }
    }

    public bool Maximized {
        get { return _maximized; }
    }

    public string Workspace {
        get { return _workSpace; }
    }

    public DateTime DateUpdated {
        get { return _dateTime; }
    }
}

[Serializable]
public class App {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly string _username;
    private List<Apps_Coll> _dataTable_apps = new List<Apps_Coll>();
    private List<UserApps_Coll> _dataTable_users = new List<UserApps_Coll>();

    public App(string username) {
        _username = username;
    }

    public App() { }

    public List<Apps_Coll> AppList {
        get { return _dataTable_apps; }
    }

    public List<UserApps_Coll> UserAppList {
        get { return _dataTable_users; }
    }

    public void CreateItem(string appid, string name, string filename, string icon, string ar, string am,
                           string about, string description, string css, string category, string height, string width,
                           bool allowparams, bool allowpopout, string popoutloc, string overlayId, string notificationId,
                           bool displayNav, bool allowStats, bool autoOpen, string defaultWorkspace, bool isPrivate) {
        int h = 0;
        int w = 0;
        int.TryParse(height, out h);
        int.TryParse(width, out w);

        int _allowparams = 0;
        if (allowparams) {
            _allowparams = 1;
        }

        int _allowpopout = 0;
        if (allowpopout) {
            _allowpopout = 1;
        }

        int _displayNav = 0;
        if (displayNav) {
            _displayNav = 1;
        }

        int _allowStats = 0;
        if (allowStats) {
            _allowStats = 1;
        }

        int _autoOpen = 0;
        if (autoOpen) {
            _autoOpen = 1;
        }

        int _isPrivate = 0;
        if (isPrivate) {
            _isPrivate = 1;
        }

        name = name.Trim();
        if (name.Length > 20)
            name = name.Substring(0, 20);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("AppID", appid.Trim().ToLower()));
        query.Add(new DatabaseQuery("Name", name));
        query.Add(new DatabaseQuery("Filename", filename.Trim()));
        query.Add(new DatabaseQuery("Icon", icon.Trim()));
        query.Add(new DatabaseQuery("AllowResize", ar.Trim()));
        query.Add(new DatabaseQuery("AllowMaximize", am.Trim()));
        query.Add(new DatabaseQuery("About", about.Trim()));
        query.Add(new DatabaseQuery("Description", description.Trim()));
        query.Add(new DatabaseQuery("CssClass", css.Trim()));
        query.Add(new DatabaseQuery("AutoLoad", "0"));
        query.Add(new DatabaseQuery("AutoFullScreen", "0"));
        query.Add(new DatabaseQuery("Category", category));
        query.Add(new DatabaseQuery("MinHeight", h.ToString()));
        query.Add(new DatabaseQuery("MinWidth", w.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", HttpContext.Current.User.Identity.Name.ToLower()));
        query.Add(new DatabaseQuery("AllowParams", _allowparams.ToString()));
        query.Add(new DatabaseQuery("AllowPopOut", _allowpopout.ToString()));
        query.Add(new DatabaseQuery("PopOutLoc", popoutloc));
        query.Add(new DatabaseQuery("OverlayID", overlayId));
        query.Add(new DatabaseQuery("NotificationID", notificationId));
        query.Add(new DatabaseQuery("DisplayNav", _displayNav.ToString()));
        query.Add(new DatabaseQuery("AllowStats", _allowStats.ToString()));
        query.Add(new DatabaseQuery("AutoOpen", _autoOpen.ToString()));
        query.Add(new DatabaseQuery("DefaultWorkspace", defaultWorkspace));
        query.Add(new DatabaseQuery("IsPrivate", _isPrivate.ToString()));

        dbCall.CallInsert("AppList", query);
    }

    public void SaveItem(string id, string title, string x, string y, string width, string height, bool closed,
                         bool minimized, bool maximized, string workspace) {

        if (!string.IsNullOrEmpty(_username)) {
            string c = "0";
            if (closed) {
                c = "1";
            }
            string m = "0";
            if (minimized) {
                m = "1";
            }
            string m2 = "0";
            if (maximized) {
                m2 = "1";
            }
            if (string.IsNullOrEmpty(width)) {
                width = "0";
            }
            if (string.IsNullOrEmpty(height)) {
                height = "0";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("UserName", _username.Trim()));
            query.Add(new DatabaseQuery("AppID", id.Trim()));
            query.Add(new DatabaseQuery("AppName", title.Trim()));
            query.Add(new DatabaseQuery("PosX", x.Trim()));
            query.Add(new DatabaseQuery("PosY", y.Trim()));
            query.Add(new DatabaseQuery("Width", width.Trim()));
            query.Add(new DatabaseQuery("Height", height.Trim()));
            query.Add(new DatabaseQuery("Closed", c.Trim()));
            query.Add(new DatabaseQuery("Minimized", m.Trim()));
            query.Add(new DatabaseQuery("Maximized", m2.Trim()));
            query.Add(new DatabaseQuery("Workspace", workspace.Trim()));
            query.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString()));

            dbCall.CallInsert("UserApps", query);
        }
    }

    public string SaveItem(string id, string title, bool closed, bool minimized, bool maximized, string workspace, string width, string height) {
        if (!string.IsNullOrEmpty(_username)) {
            string c = "0";
            if (closed) {
                c = "1";
            }
            string m = "0";
            if (minimized) {
                m = "1";
            }
            string m2 = "0";
            if (maximized) {
                m2 = "1";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("UserName", _username.Trim()));
            query.Add(new DatabaseQuery("AppID", id.Trim()));
            query.Add(new DatabaseQuery("AppName", title.Trim()));
            query.Add(new DatabaseQuery("PosX", "75px"));
            query.Add(new DatabaseQuery("PosY", "75px"));
            query.Add(new DatabaseQuery("Width", width.Trim()));
            query.Add(new DatabaseQuery("Height", height.Trim()));
            query.Add(new DatabaseQuery("Closed", c.Trim()));
            query.Add(new DatabaseQuery("Minimized", m.Trim()));
            query.Add(new DatabaseQuery("Maximized", m2.Trim()));
            query.Add(new DatabaseQuery("Workspace", workspace.Trim()));
            query.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString()));

            dbCall.CallInsert("UserApps", query);

            if (id.Contains("app-ChatClient-"))
                return id;
            else
                return GetAppFilename(id);
        }

        return GetAppFilename(id);
    }

    public void UpdateItem(string id, string title, string x, string y, string width, string height, bool closed,
                           bool minimized, bool maximized, string workspace) {
        if (!string.IsNullOrEmpty(_username)) {
            string c = "0";
            if (closed) {
                c = "1";
            }
            string m = "0";
            if (minimized) {
                m = "1";
            }
            string max = "0";
            if (maximized) {
                max = "1";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserName", _username));
            query.Add(new DatabaseQuery("AppID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("PosX", x.Trim()));
            updateQuery.Add(new DatabaseQuery("PosY", y.Trim()));
            updateQuery.Add(new DatabaseQuery("Closed", c.Trim()));
            updateQuery.Add(new DatabaseQuery("Minimized", m.Trim()));
            updateQuery.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString().Trim()));
            updateQuery.Add(new DatabaseQuery("AppName", title.Trim()));
            updateQuery.Add(new DatabaseQuery("Maximized", max));
            updateQuery.Add(new DatabaseQuery("Workspace", workspace.Trim()));

            dbCall.CallUpdate("UserApps", updateQuery, query);
        }
    }

    public string UpdateItem(string id, string title, bool closed, bool minimized, bool maximized, string workspace, string width, string height) {
        if (!string.IsNullOrEmpty(_username)) {
            string c = "0";
            if (closed) {
                c = "1";
            }
            string m = "0";
            if (minimized) {
                m = "1";
            }
            string max = "0";
            if (maximized) {
                max = "1";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserName", _username));
            query.Add(new DatabaseQuery("AppID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Closed", c.Trim()));
            updateQuery.Add(new DatabaseQuery("Minimized", m.Trim()));
            updateQuery.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString().Trim()));
            updateQuery.Add(new DatabaseQuery("AppName", title.Trim()));
            updateQuery.Add(new DatabaseQuery("Maximized", max));
            updateQuery.Add(new DatabaseQuery("Workspace", workspace.Trim()));
            updateQuery.Add(new DatabaseQuery("Height", height.Trim()));
            updateQuery.Add(new DatabaseQuery("Width", width.Trim()));

            dbCall.CallUpdate("UserApps", updateQuery, query);

            if (id.Contains("app-ChatClient-"))
                return id;
            else
                return GetAppFilename(id);
        }

        return GetAppFilename(id);
    }

    public void UpdateAppDate(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString().Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void UpdateSize(string id, string width, string height) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Maximized", "0"));
        updateQuery.Add(new DatabaseQuery("Width", width.Trim()));
        updateQuery.Add(new DatabaseQuery("Height", height.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void UpdatePosition(string id, string posX, string posY) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PosX", posX.Trim()));
        updateQuery.Add(new DatabaseQuery("PosY", posY.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void UpdateMaximized(string id, string maximized) {
        if (string.IsNullOrEmpty(maximized)) {
            maximized = "0";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Maximized", maximized.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void Updateworkspace(string id, string workspace) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Workspace", workspace.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public string GetAppID(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("Name", name) });
        return dbSelect.Value;
    }

    public string GetAppIDbyFilename(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("Filename", name) });
        return dbSelect.Value;
    }

    public string GetAppCssClass(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "CssClass", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });
        return dbSelect.Value;
    }

    public bool GetAppAutoOpen(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoOpen", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetCurrentworkspace(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Workspace", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
        return dbSelect.Value;
    }

    public bool GetAppMax(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Maximized", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetAppWidth(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Width", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
            return dbSelect.Value;
        }

        return string.Empty;
    }

    public string GetAppHeight(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Height", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
            return dbSelect.Value;
        }

        return string.Empty;
    }

    public bool GetClosedState(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Closed", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
            return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
        }

        return true;
    }

    public Apps_Coll GetAppInformation(string appid) {
        Apps_Coll db = new Apps_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string appID = row["AppID"];
            string appName = row["Name"];
            string filename = row["Filename"];
            string icon = row["Icon"];
            string allowResize = row["AllowResize"];
            string allowMaximize = row["AllowMaximize"];
            string about = row["About"];
            string description = row["Description"];
            string cssClass = row["CssClass"];
            string autoLoad = row["AutoLoad"];
            string autoFullScreen = row["AutoFullScreen"];
            string category = row["Category"];
            string minHeight = row["MinHeight"];
            string minWidth = row["MinWidth"];
            string createdBy = row["CreatedBy"];
            string allowParams = row["AllowParams"];
            string allowPopOut = row["AllowPopOut"];
            string popOutLoc = row["PopOutLoc"];
            string overlayID = row["OverlayID"];
            string notificationID = row["NotificationID"];
            string displayNav = row["DisplayNav"];
            string allowStats = row["AllowStats"];
            string autoOpen = row["AutoOpen"];
            string defaultWorkspace = row["DefaultWorkspace"];
            string isPrivate = row["IsPrivate"];

            db = new Apps_Coll(id, appID, appName, filename, icon, allowResize, allowMaximize, about, description, cssClass, autoLoad, autoFullScreen, category, minHeight, minWidth, createdBy, allowParams, allowPopOut, popOutLoc, overlayID, notificationID, displayNav, allowStats, autoOpen, defaultWorkspace, isPrivate);
            break;
        }

        return db;
    }

    public UserApps_Coll GetUserAppInformation(string appid) {
        UserApps_Coll db = new UserApps_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery("UserName", _username) });
         foreach (Dictionary<string, string> row in dbSelect) {
             string id = row["ID"];
             string un = row["UserName"];
             string appID = row["AppID"];
             string appName = row["AppName"];
             string posX = row["PosX"];
             string posY = row["PosY"];
             string width = row["Width"];
             string height = row["Height"];
             string closed = row["Closed"];
             string minimized = row["Minimized"];
             string maximized = row["Maximized"];
             string workspace = row["Workspace"];
             string dateTime = row["DateTime"];
             db = new UserApps_Coll(id, un, appID, appName, posX, posY, width, height, closed, minimized, maximized, workspace, dateTime);
             break;
         }

        return db;
    }

    public string GetAppFilename(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Filename", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });
        return dbSelect.Value;
    }

    public bool AllowMaximize(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowMaximize", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool AllowStats(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowStats", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool AllowResize(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowResize", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool AllowParams(string appId) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowParams", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool GetAutoLoad(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoLoad", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool GetAutoFullScreen(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoFullScreen", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool ItemInDatabase(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username) });
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool ItemInDatabase(string user, string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", user) });
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool ItemInDatabase_name(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppName", name), new DatabaseQuery("UserName", _username) });
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }
        else {
            return true;
        }
    }

    public void DeleteAppComplete(string id, string serverpath, bool deleteFiles = true) {
        #region Delete file first
        var di = new DirectoryInfo(serverpath + "Apps");
        if (deleteFiles) {
            for (int i = 0; i < di.GetDirectories().Length; i++) {
                string directoryname = di.GetDirectories()[i].Name;
                var di2 = new DirectoryInfo(serverpath + "Apps\\" + directoryname);
                if (directoryname.ToLower() == "database_imports") {
                    if (File.Exists(serverpath + "Apps\\Database_Imports\\" + id.Replace("app-", "") + ".ascx")) {
                        try {
                            File.Delete(serverpath + "Apps\\Database_Imports\\" + id.Replace("app-", "") + ".ascx");
                        }
                        catch { }
                    }
                }
                else if (directoryname.ToLower() == "custom_tables") {
                    if (File.Exists(serverpath + "Apps\\Custom_Tables\\" + id.Replace("app-", "") + ".ascx")) {
                        try {
                            File.Delete(serverpath + "Apps\\Custom_Tables\\" + id.Replace("app-", "") + ".ascx");
                        }
                        catch { }
                    }
                }
                else {
                    string appFn = GetAppFilename(id);
                    if (appFn.Contains("/")) {
                        string[] splitFn = appFn.Split('/');
                        if (splitFn.Length > 0) {
                            string realDir = splitFn[0];
                            if ((serverpath + "Apps\\" + directoryname) == (serverpath + "Apps\\" + realDir)) {
                                if (Directory.Exists(serverpath + "Apps\\" + directoryname)) {
                                    Directory.Delete(serverpath + "Apps\\" + directoryname, true);
                                    break;
                                }
                            }
                        }
                    }
                    else {
                        for (int j = 0; j < di2.GetFiles().Length; j++) {
                            string filename = directoryname + "/" + di2.GetFiles()[j].Name;
                            if (filename == appFn) {
                                try {
                                    di2.Delete(true);
                                    break;
                                }
                                catch { }
                            }
                        }

                        for (int j = 0; j < di.GetFiles().Length; j++) {
                            string filename = di.GetFiles()[j].Name;
                            if (filename == appFn) {
                                try {
                                    di.GetFiles()[j].Delete();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        if (deleteFiles) {
            AppDLLs dlls = new AppDLLs();
            dlls.DeleteItem(id);
        }

        try {
            string dllFolder = id.Replace("app-", "");
            string tempPath = serverpath + "Bin\\" + dllFolder;

            ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + dllFolder);

            if (deleteFiles) {
                if (!string.IsNullOrEmpty(dllFolder)) {
                    if (Directory.Exists(tempPath)) {
                        Directory.Delete(tempPath, true);
                    }
                }
            }
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }

        dbCall.CallDelete("AppList", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });
    }

    public void DeleteAppMain(string id) {
        dbCall.CallDelete("AppList", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });

        AppDLLs dlls = new AppDLLs();
        dlls.DeleteItem(id);

        try {
            string dllFolder = id.Replace("app-", "");
            string tempPath = ServerSettings.GetServerMapLocation + "Bin\\" + dllFolder;

            ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + dllFolder);

            if (!string.IsNullOrEmpty(dllFolder)) {
                if (Directory.Exists(tempPath)) {
                    Directory.Delete(tempPath, true);
                }
            }
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }
    }

    public void DeleteAppLocal(string id) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id) });
    }

    public void DeleteAppLocal(string id, string username) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", username) });
    }

    public void UpdateAppFilename(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Filename", name));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppName(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Name", name));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppOverlayID(string id, string overlayId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("OverlayID", overlayId));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppNotificationID(string id, string notificationId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("NotificationID", notificationId));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppLocal(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppName", name));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void UpdateAutoLoad(string id, bool load) {
        string l = "0";
        if (load) {
            l = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AutoLoad", l));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateCategory(string id, string category) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Category", category));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAutoFullScreen(string id, bool load) {
        string l = "0";
        if (load) {
            l = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AutoFullScreen", l));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public string[] GetAppOverlayIds(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "OverlayID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });

        List<string> overlayIds = new List<string>();
        string oIds = dbSelect.Value;

        if (!string.IsNullOrEmpty(oIds)) {
            string[] splitOids = oIds.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string oId in splitOids) {
                if (!overlayIds.Contains(oId)) {
                    overlayIds.Add(oId);
                }
            }
        }

        return overlayIds.ToArray();
    }

    public string GetAppIconName(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Icon", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return dbSelect.Value;
    }

    public List<Apps_Coll> GetApps_byCategory(string categoryID) {
        List<Apps_Coll> db = new List<Apps_Coll>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Category", categoryID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildAppColl(row, ref db);
        }

        return db;
    }

    public bool IconExists(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Icon", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        if (!string.IsNullOrEmpty(dbSelect.Value)) {
            return true;
        }

        return false;
    }

    public string GetAppName(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Name", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return dbSelect.Value;
    }

    public bool GetIsPrivate(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "IsPrivate", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetAppCreatedBy(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "CreatedBy", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return dbSelect.Value;
    }

    public string GetAppNotificationID(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "NotificationID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });
        return dbSelect.Value;
    }

    public string[] GetAppNotificationIds(string appid) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "NotificationID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid) });

        List<string> notiIds = new List<string>();
        string nIds = dbSelect.Value;

        if (!string.IsNullOrEmpty(nIds)) {
            string[] splitNids = nIds.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string nId in splitNids) {
                if (!notiIds.Contains(nId)) {
                    notiIds.Add(nId);
                }
            }
        }

        return notiIds.ToArray();
    }

    public void GetUserInstalledApps() {
        _dataTable_users.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username) });
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref _dataTable_users);
        }
    }

    public UserApps_Coll GetUserInstalledApp_Coll(string appId) {
        UserApps_Coll coll = new UserApps_Coll();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("AppID", appId) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string un = row["UserName"];
            string appID = row["AppID"];
            string appName = row["AppName"];
            string posX = row["PosX"];
            string posY = row["PosY"];
            string width = row["Width"];
            string height = row["Height"];
            string closed = row["Closed"];
            string minimized = row["Minimized"];
            string maximized = row["Maximized"];
            string workspace = row["Workspace"];
            string dateTime = row["DateTime"];
            coll = new UserApps_Coll(id, un, appID, appName, posX, posY, width, height, closed, minimized, maximized, workspace, dateTime);
            break;
        }

        return coll;
    }

    public List<object> GetUserOpenedApps(bool showDb) {
        List<object> list = new List<object>();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "AppID, Workspace", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("Closed", "0") });
        foreach (Dictionary<string, string> row in dbSelect) {
            List<string> tempList = new List<string>();
            string appId = row["AppID"];
            string db = string.Empty;
            if (showDb) {
                db = row["Workspace"];
            }

            tempList.Add(appId);
            tempList.Add(db);
            list.Add(tempList);
        }

        return list;
    }

    public List<string> GetUserInstalledApp(string appId) {
        List<string> list = new List<string>();

        string selectCols = "PosX, PosY, Width, Height, Closed, Minimized, Maximized, Workspace, DateTime";

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", selectCols, new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("AppID", appId) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string Closed = row["Closed"];
            string Minimized = row["Minimized"];
            string Maximized = row["Maximized"];
            string Workspace = row["Workspace"];
            string DateUpdated = row["DateTime"];

            string posX = row["PosX"];
            string posY = row["PosY"];
            string width = row["Width"];
            string height = row["Height"];

            list.Add(Closed);
            list.Add(Minimized);
            list.Add(Maximized);
            list.Add(Workspace);
            list.Add(DateUpdated);

            list.Add(posX);
            list.Add(posY);
            list.Add(width);
            list.Add(height);

            break;
        }

        if (list.Count == 0) {
            list.Add("1");
            list.Add(string.Empty);
            list.Add(string.Empty);
            list.Add(string.Empty);
            list.Add("N/A");

            list.Add("0");
            list.Add("0");
            list.Add("0");
            list.Add("0");
        }

        GetExtraAppInfo(appId, ref list);
        return list;
    }
    private void GetExtraAppInfo(string appId, ref List<string> list) {
        string selectCols = "MinHeight, MinWidth, AllowPopOut, PopOutLoc, AllowMaximize, AllowResize, AllowStats";
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", selectCols, new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string MinHeight = row["MinHeight"];
            string MinWidth = row["MinWidth"];

            string AllowPopOut = row["AllowPopOut"];
            string PopOutLoc = row["PopOutLoc"];
            string AllowMaximize = row["AllowMaximize"];
            string AllowResize = row["AllowResize"];
            string AllowStats = row["AllowStats"];

            list.Add(MinHeight);
            list.Add(MinWidth);

            list.Add(AllowPopOut);

            PopOutLoc = CheckPopoutURL(PopOutLoc);

            list.Add(PopOutLoc);
            list.Add(AllowMaximize);
            list.Add(AllowResize);
            list.Add(AllowStats);
            break;
        }
    }
    private string CheckPopoutURL(string loc) {
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

    public void GetUserSavedChatApps() {
        _dataTable_users.Clear();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", "UserName=@UserName AND AppID LIKE 'app-ChatClient-%'", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username) });
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref _dataTable_users);
        }
    }

    public void GetAllApps() {
        _dataTable_apps.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", null, "Name ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildAppColl(row, ref _dataTable_apps);
        }
    }

    public void GetAllAscxApps() {
        _dataTable_apps.Clear();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", "LOWER(Filename) LIKE '%.ascx'", null);
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildAppColl(row, ref _dataTable_apps);
        }
    }

    public void UpdateAppImage(string id, string img) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Icon", img.Trim()));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAllowParams(string id, bool allowparams) {
        int ap = 0;
        if (allowparams) {
            ap = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AllowParams", ap.ToString()));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppList(string id, string name, string ar, string am, string about, string description,
                                 string css, string height, string width, bool allowpopout, string popoutloc,
                                 bool displayNav, bool allowStats, bool autoOpen, string defaultWorkspace, bool isPrivate) {
        string _ar = "0";
        string _am = "0";
        if (HelperMethods.ConvertBitToBoolean(ar)) {
            _ar = "1";
        }
        if (HelperMethods.ConvertBitToBoolean(am)) {
            _am = "1";
        }

        string _allowpopout = "0";
        if (allowpopout) {
            _allowpopout = "1";
        }

        string _displayNav = "0";
        if (displayNav) {
            _displayNav = "1";
        }

        string _allowStats = "0";
        if (allowStats) {
            _allowStats = "1";
        }

        string _autoOpen = "0";
        if (autoOpen) {
            _autoOpen = "1";
        }

        string _isPrivate = "0";
        if (isPrivate) {
            _isPrivate = "1";
        }

        int h = 0;
        int w = 0;
        int.TryParse(height, out h);
        int.TryParse(width, out w);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Name", name.Trim()));
        updateQuery.Add(new DatabaseQuery("AllowResize", _ar.Trim()));
        updateQuery.Add(new DatabaseQuery("AllowMaximize", _am.Trim()));
        updateQuery.Add(new DatabaseQuery("About", about.Trim()));
        updateQuery.Add(new DatabaseQuery("Description", description.Trim()));
        updateQuery.Add(new DatabaseQuery("CssClass", css.Trim()));
        updateQuery.Add(new DatabaseQuery("MinHeight", h.ToString()));
        updateQuery.Add(new DatabaseQuery("MinWidth", w.ToString()));
        updateQuery.Add(new DatabaseQuery("AllowPopOut", _allowpopout));
        updateQuery.Add(new DatabaseQuery("PopOutLoc", popoutloc));
        updateQuery.Add(new DatabaseQuery("DisplayNav", _displayNav));
        updateQuery.Add(new DatabaseQuery("AllowStats", _allowStats));
        updateQuery.Add(new DatabaseQuery("AutoOpen", _autoOpen));
        updateQuery.Add(new DatabaseQuery("DefaultWorkspace", defaultWorkspace));
        updateQuery.Add(new DatabaseQuery("IsPrivate", _isPrivate));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void DeleteUserProperties(string username) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.Trim()) });
    }

    public void DeleteUserProperties() {
        dbCall.CallDelete("UserApps", null);
    }

    public List<string> DeleteDuplicateEnabledApps(MemberDatabase _m) {
        int dup = 0;
        List<string> l = _m.EnabledApps;
        List<string> temp = new List<string>();
        foreach (string w in l) {
            if (!temp.Contains(w)) {
                temp.Add(w);
            }
            else {
                dup++;
            }
        }

        if (dup > 0) {
            _m.RemoveAllEnabledApps();
            foreach (string d in temp) {
                _m.UpdateEnabledApps(d);
            }
        }

        return temp;
    }

    public bool HasApp(string appid) {
        UserApps_Coll coll = GetUserInstalledApp_Coll(appid);
        if (!string.IsNullOrEmpty(coll.ID)) {
            return true;
        }

        return false;
    }

    public List<UserApps_Coll> GetUserApps_AllUsers(string appid) {
        List<UserApps_Coll> db = new List<UserApps_Coll>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AppID", appid));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", query, "DATETIME ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref db);
        }

        return db;
    }

    public List<string> GetAppsThatAllowStats() {
        List<string> list = new List<string>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("AllowStats", "1"));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", query, "Name ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string appId = row["AppID"];
            if (!list.Contains(appId))
                list.Add(appId);
        }

        return list;
    }

    public void BuildAboutApp(System.Web.UI.WebControls.Panel wlmd_holder, string appId, string username) {
        wlmd_holder.Controls.Clear();
        var str = new System.Text.StringBuilder();
        ServerSettings _ss = new ServerSettings();
        if (appId.Contains("app-ChatClient-")) {
            string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
            string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/About Logos/chat.png' class='pad-right-big float-left' style='height: 48px;' />";
            if (_ss.HideAllAppIcons)
                imageIcon = string.Empty;

            str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + ServerSettings.SiteName + " Chat Client</h2>");
            str.Append("</div><div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>Description</b><div class='clear-space-two'></div>An instant messaging (IM) client that features real-time chat sessions with other users of an associated group.<div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>About</b><div class='clear-space-two'></div>" + ServerSettings.SiteName + " - " + DateTime.Now.Year + " John Donnelly<div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>App Properties</b><div class='clear-space-two'></div><div class='float-left pad-right-big margin-right-big'>Min-Width:  300px");
            str.Append("<div class='clear-space-two'></div>Allow Maximize:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div>");
            str.Append("Allow Resize:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div>Background:  <span style='color: #1D1D1D;'><i>Visible</i></span><div class='clear-space-two'></div>");
            str.Append("Allow Pop Out:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div></div>");
            str.Append("<div class='inline-block'><div>Min-Height:  400px");
            str.Append("<div class='clear-space-two'></div>Maximize on Load:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Auto Open:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Nav Buttons:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Allow Params:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>");
            str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
            str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
        }
        else {
            App _apps = new App();
            AppCategory _category = new AppCategory(false);
            Apps_Coll db = _apps.GetAppInformation(appId);
            if (db != null) {
                string canResize = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string canMaximize = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string autoMax = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string autoOpen = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string displayNav = "<span style='color: #1D1D1D;'><i>Off</i></span>";
                string autoCreate = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string allowParams = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string AllowPopout = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string allowStats = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string isPrivate = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string background = "<span style='color: #1D1D1D;'><i>Visible</i></span>";
                string about = db.About;
                string description = db.Description;
                if (db.AllowResize)
                    canResize = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowMaximize)
                    canMaximize = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoLoad)
                    autoCreate = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoFullScreen)
                    autoMax = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoOpen)
                    autoOpen = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.DisplayNav)
                    displayNav = "<span style='color: #1D1D1D;'><i>On</i></span>";

                if (db.AllowParams)
                    allowParams = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowStats)
                    allowStats = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.IsPrivate)
                    isPrivate = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowPopOut)
                    AllowPopout = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (string.IsNullOrEmpty(description))
                    description = "No description available";

                if (string.IsNullOrEmpty(about)) {
                    string tempuser = "John Donnelly";
                    if (string.IsNullOrEmpty(db.CreatedBy)) {
                        var tempmember = new MemberDatabase(db.CreatedBy);
                        tempuser = HelperMethods.MergeFMLNames(tempmember);
                    }
                    about = ServerSettings.SiteName + " | " + DateTime.Now.Year + " | " + tempuser;
                }

                if (db.CssClass.ToLower() == "app-main-nobg")
                    background = "<span style='color: #1D1D1D;'><i>Hidden</i></span>";

                string width = db.MinWidth;
                string height = db.MinHeight;
                if (string.IsNullOrEmpty(width))
                    width = "N/A";
                else
                    width = "<i>" + width + "px</i>";

                if (string.IsNullOrEmpty(height))
                    height = "N/A";
                else
                    height = "<i>" + height + "px</i>";

                string defaultWorkspace = "1";
                if ((!string.IsNullOrEmpty(db.DefaultWorkspace)) && (db.DefaultWorkspace != "0")) {
                    defaultWorkspace = db.DefaultWorkspace;
                }

                string overlay = string.Empty;
                if (!string.IsNullOrEmpty(db.OverlayID)) {
                    string[] overlayIds = db.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oId in overlayIds) {
                        WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
                        WorkspaceOverlay_Coll tempColl = _workspaceOverlays.GetWorkspaceOverlay(oId);
                        if (!string.IsNullOrEmpty(tempColl.OverlayName)) {
                            overlay += "<div class='pad-bottom-sml'>" + tempColl.OverlayName + ": <i>" + tempColl.Description + "</i></div>";
                        }
                    }
                }
                if (string.IsNullOrEmpty(overlay)) {
                    overlay = "None";
                }

                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
                string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/App_Icons/" + db.Icon + "' class='pad-right-big float-left' style='height: 48px;' />";
                if (_ss.HideAllAppIcons) {
                    imageIcon = string.Empty;
                }

                bool allowAppRating = _ss.AllowAppRating;

                str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + db.AppName + "</h2>");

                if (allowAppRating) {
                    str.Append("<div class='float-left pad-left-big pad-top-big'><div class='app-rater'></div></div>");
                }

                str.Append("<div class='clear'></div><b style='color: #1D1D1D; font-size: 11px'>" + _category.GetCategoryName(db.Category) + "</b></div><div class='clear-space'></div><div class='clear-space'></div>");

                str.Append("<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>Description</b><div class='clear-space-two'></div>" + description + "<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>About</b><div class='clear-space-two'></div>" + about + "<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>Associated Overlays</b><div class='clear-space-two'></div>" + overlay + "<div class='clear-space'></div><div class='clear-space'></div>");

                str.Append("<b>App Properties</b><div class='clear-space-two'></div><div class='float-left pad-right-big margin-right-big'>");
                str.Append("Min-Width:  " + width);
                str.Append("<div class='clear-space-two'></div>Allow Maximize:  " + canMaximize);
                str.Append("<div class='clear-space-two'></div>Allow Resize:  " + canResize);
                str.Append("<div class='clear-space-two'></div>Allow Statistics:  " + allowStats);
                str.Append("<div class='clear-space-two'></div>Allow Params:  " + allowParams);
                str.Append("<div class='clear-space-two'></div>Default Workspace:  <i>" + defaultWorkspace + "</i>");
                if (_username.ToLower() == db.CreatedBy.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    str.Append("<div class='clear-space-two'></div>Is Private:  " + isPrivate);
                }
                str.Append("<div class='clear-space-two'></div>Allow Pop Out:  " + AllowPopout + "<div class='clear-space-two'></div></div>");

                str.Append("<div class='inline-block'>");
                str.Append("<div>Min-Height:  " + height);
                str.Append("<div class='clear-space-two'></div>Background:  " + background);
                str.Append("<div class='clear-space-two'></div>Maximize on Load:  " + autoMax);
                str.Append("<div class='clear-space-two'></div>Auto Open:  " + autoOpen);
                str.Append("<div class='clear-space-two'></div>Nav Buttons:  " + displayNav);
                str.Append("<div class='clear-space-two'></div>Filename:  <i>" + db.filename + "</i>");
                if (!string.IsNullOrEmpty(username)) {
                    if (!Roles.IsUserInRole(username, ServerSettings.AdminUserName)) {
                        if (db.CreatedBy.ToLower() == username.ToLower()) {
                            if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                                str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                        }
                        else {
                            if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                                str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                        }
                    }
                    else {
                        if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                            str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                    }
                }
                else {
                    if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                        str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                }

                str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
                if (db.AllowPopOut) {
                    string popoutloc = "";
                    if (string.IsNullOrEmpty(db.PopOutLoc))
                        popoutloc = "N/A";

                    int len = db.PopOutLoc.Length;
                    if (len > 60) {
                        int startOf = 0;
                        while (startOf < len) {
                            if (startOf + 60 < len) {
                                popoutloc += db.PopOutLoc.Substring(startOf, 60);
                                startOf += 60;
                            }
                            else {
                                popoutloc += db.PopOutLoc.Substring(startOf);
                                break;
                            }
                            popoutloc += "<br/>";
                        }
                    }
                    else
                        popoutloc = db.PopOutLoc;

                    str.Append("<div class='clear-space-two'></div>Pop Out Location:  <i>" + popoutloc + "</i>");
                }

                string createdBy_text = string.Empty;
                if (!string.IsNullOrEmpty(db.CreatedBy)) {
                    MemberDatabase tempm = new MemberDatabase(db.CreatedBy);
                    createdBy_text = "Created By: " + HelperMethods.MergeFMLNames(tempm);
                }

                str.Append("<div class='clear-space'></div>" + createdBy_text + "<div class='clear-space'></div>");

                if (allowAppRating) {
                    // Get Reviews
                    str.Append("<div id='" + appId + "-rating-reviews'>" + GetReviews(appId) + "</div>");
                }
            }
        }
        wlmd_holder.Controls.Add(new System.Web.UI.LiteralControl(str.ToString()));
    }
    public string BuildAboutApp(string appId, string username) {
        var str = new System.Text.StringBuilder();
        ServerSettings _ss = new ServerSettings();
        if (appId.Contains("app-ChatClient-")) {
            string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
            string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/About Logos/chat.png' class='pad-right-big float-left' style='height: 48px;' />";
            if (_ss.HideAllAppIcons)
                imageIcon = string.Empty;

            str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + ServerSettings.SiteName + " Chat Client</h2>");
            str.Append("</div><div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>Description</b><div class='clear-space-two'></div>An instant messaging (IM) client that features real-time chat sessions with other users of an associated group.<div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>About</b><div class='clear-space-two'></div>" + ServerSettings.SiteName + " - " + DateTime.Now.Year + " John Donnelly<div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<b>App Properties</b><div class='clear-space-two'></div><div class='float-left pad-right-big margin-right-big'>Min-Width:  300px");
            str.Append("<div class='clear-space-two'></div>Allow Maximize:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div>");
            str.Append("Allow Resize:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div>Background:  <span style='color: #1D1D1D;'><i>Visible</i></span><div class='clear-space-two'></div>");
            str.Append("Allow Pop Out:  <span style='color: #1D1D1D;'><i>True</i></span><div class='clear-space-two'></div></div>");
            str.Append("<div class='inline-block'><div>Min-Height:  400px");
            str.Append("<div class='clear-space-two'></div>Maximize on Load:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Auto Open:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Nav Buttons:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>Allow Params:  <span style='color: #1D1D1D;'><i>False</i></span>");
            str.Append("<div class='clear-space-two'></div>");
            str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
            str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
        }
        else {
            App _apps = new App();
            AppCategory _category = new AppCategory(false);
            Apps_Coll db = _apps.GetAppInformation(appId);
            if (db != null) {
                string canResize = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string canMaximize = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string autoMax = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string autoOpen = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string displayNav = "<span style='color: #1D1D1D;'><i>Off</i></span>";
                string autoCreate = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string allowParams = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string AllowPopout = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string allowStats = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string isPrivate = "<span style='color: #1D1D1D;'><i>False</i></span>";
                string background = "<span style='color: #1D1D1D;'><i>Visible</i></span>";
                string about = db.About;
                string description = db.Description;
                if (db.AllowResize)
                    canResize = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowMaximize)
                    canMaximize = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoLoad)
                    autoCreate = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoFullScreen)
                    autoMax = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AutoOpen)
                    autoOpen = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.DisplayNav)
                    displayNav = "<span style='color: #1D1D1D;'><i>On</i></span>";

                if (db.AllowParams)
                    allowParams = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowStats)
                    allowStats = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.IsPrivate)
                    isPrivate = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (db.AllowPopOut)
                    AllowPopout = "<span style='color: #1D1D1D;'><i>True</i></span>";

                if (string.IsNullOrEmpty(description))
                    description = "No description available";

                if (string.IsNullOrEmpty(about)) {
                    string tempuser = "John Donnelly";
                    if (string.IsNullOrEmpty(db.CreatedBy)) {
                        var tempmember = new MemberDatabase(db.CreatedBy);
                        tempuser = HelperMethods.MergeFMLNames(tempmember);
                    }
                    about = ServerSettings.SiteName + " | " + DateTime.Now.Year + " | " + tempuser;
                }

                if (db.CssClass.ToLower() == "app-main-nobg")
                    background = "<span style='color: #1D1D1D;'><i>Hidden</i></span>";

                string width = db.MinWidth;
                string height = db.MinHeight;
                if (string.IsNullOrEmpty(width))
                    width = "N/A";
                else
                    width = "<i>" + width + "px</i>";

                if (string.IsNullOrEmpty(height))
                    height = "N/A";
                else
                    height = "<i>" + height + "px</i>";

                string defaultWorkspace = "1";
                if ((!string.IsNullOrEmpty(db.DefaultWorkspace)) && (db.DefaultWorkspace != "0"))
                    defaultWorkspace = db.DefaultWorkspace;

                string overlay = string.Empty;
                if (!string.IsNullOrEmpty(db.OverlayID)) {
                    string[] overlayIds = db.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oId in overlayIds) {
                        WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
                        WorkspaceOverlay_Coll tempColl = _workspaceOverlays.GetWorkspaceOverlay(oId);
                        if (!string.IsNullOrEmpty(tempColl.OverlayName)) {
                            overlay += "<div class='pad-bottom-sml'>" + tempColl.OverlayName + ": <i>" + tempColl.Description + "</i></div>";
                        }
                    }
                }
                if (string.IsNullOrEmpty(overlay)) {
                    overlay = "None";
                }

                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
                string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/App_Icons/" + db.Icon + "' class='pad-right-big float-left' style='height: 48px;' />";
                if (_ss.HideAllAppIcons)
                    imageIcon = string.Empty;

                bool allowAppRating = _ss.AllowAppRating;

                str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + db.AppName + "</h2>");

                if (allowAppRating) {
                    str.Append("<div class='float-left pad-left-big pad-top-big'><div class='app-rater'></div></div>");
                }

                str.Append("<div class='clear'></div><b style='color: #1D1D1D; font-size: 11px'>" + _category.GetCategoryName(db.Category) + "</b></div><div class='clear-space'></div><div class='clear-space'></div>");

                str.Append("<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>Description</b><div class='clear-space-two'></div>" + description + "<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>About</b><div class='clear-space-two'></div>" + about + "<div class='clear-space'></div><div class='clear-space'></div>");
                str.Append("<b>Associated Overlays</b><div class='clear-space-two'></div>" + overlay + "<div class='clear-space'></div><div class='clear-space'></div>");

                str.Append("<b>App Properties</b><div class='clear-space-two'></div><div class='float-left pad-right-big margin-right-big'>");
                str.Append("Min-Width:  " + width);
                str.Append("<div class='clear-space-two'></div>Allow Maximize:  " + canMaximize);
                str.Append("<div class='clear-space-two'></div>Allow Resize:  " + canResize);
                str.Append("<div class='clear-space-two'></div>Allow Statistics:  " + allowStats);
                str.Append("<div class='clear-space-two'></div>Allow Params:  " + allowParams);
                str.Append("<div class='clear-space-two'></div>Default Workspace:  " + defaultWorkspace);
                if (_username.ToLower() == db.CreatedBy.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    str.Append("<div class='clear-space-two'></div>Is Private:  " + isPrivate);
                }
                str.Append("<div class='clear-space-two'></div>Allow Pop Out:  " + AllowPopout + "<div class='clear-space-two'></div></div>");

                str.Append("<div class='inline-block'>");
                str.Append("<div>Min-Height:  " + height);
                str.Append("<div class='clear-space-two'></div>Background:  " + background);
                str.Append("<div class='clear-space-two'></div>Maximize on Load:  " + autoMax);
                str.Append("<div class='clear-space-two'></div>Auto Open:  " + autoOpen);
                str.Append("<div class='clear-space-two'></div>Nav Buttons:  " + displayNav);
                str.Append("<div class='clear-space-two'></div>Filename:  " + db.filename);
                if (!string.IsNullOrEmpty(username)) {
                    if (!Roles.IsUserInRole(username, ServerSettings.AdminUserName)) {
                        if (db.CreatedBy.ToLower() == username.ToLower()) {
                            if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                                str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                        }
                        else {
                            if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                                str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                        }
                    }
                    else {
                        if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                            str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                    }
                }
                else {
                    if (new FileInfo(db.filename).Extension.ToLower() == ".ascx")
                        str.Append("<div class='clear-space-two'></div>Auto Create:  " + autoCreate);
                }

                str.Append("<div class='clear-space'></div><div class='clear-space'></div></div></div>");
                if (db.AllowPopOut) {
                    string popoutloc = "";
                    if (string.IsNullOrEmpty(db.PopOutLoc))
                        popoutloc = "N/A";

                    int len = db.PopOutLoc.Length;
                    if (len > 60) {
                        int startOf = 0;
                        while (startOf < len) {
                            if (startOf + 60 < len) {
                                popoutloc += db.PopOutLoc.Substring(startOf, 60);
                                startOf += 60;
                            }
                            else {
                                popoutloc += db.PopOutLoc.Substring(startOf);
                                break;
                            }
                            popoutloc += "<br/>";
                        }
                    }
                    else
                        popoutloc = db.PopOutLoc;

                    str.Append("<div class='clear-space-two'></div>Pop Out Location:  <i>" + popoutloc + "</i>");
                }

                string createdBy_text = string.Empty;
                if (!string.IsNullOrEmpty(db.CreatedBy)) {
                    MemberDatabase tempm = new MemberDatabase(db.CreatedBy);
                    createdBy_text = "Created By: " + HelperMethods.MergeFMLNames(tempm);
                }

                str.Append("<div class='clear-space'></div>" + createdBy_text + "<div class='clear-space'></div>");

                if (allowAppRating) {
                    // Get Reviews
                    str.Append("<div id='" + appId + "-rating-reviews'>" + GetReviews(appId) + "</div>");
                }
            }
        }
        return str.ToString();
    }
    public string GetReviews(string appId) {
        System.Text.StringBuilder str = new System.Text.StringBuilder();

        AppRatings ratings = new AppRatings();

        List<AppRatings_Coll> ratingColl = ratings.GetAppRatings(appId);
        foreach (AppRatings_Coll rating in ratingColl) {
            if (!string.IsNullOrEmpty(rating.Rating)) {
                str.Append("<div class='pad-top pad-bottom margin-bottom'>");

                string starId = Guid.NewGuid().ToString();
                MemberDatabase tempMember = new MemberDatabase(rating.UserName);
                string name = HelperMethods.MergeFMLNames(tempMember);
                if (rating.UserName.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    name = "<i>You</i>";
                }

                str.Append("<div class='float-left'>" + name + "</div>");
                str.Append("<div class='float-left pad-left pad-top-sml ratingreviews-div'><div id='" + starId + "'></div></div>");
                RegisterPostbackScripts.RegisterStartupScript(null, "openWSE.RatingStyleInit('#" + starId + "', '" + rating.Rating + "', true, '" + appId + "', false);");

                string ratingDesc = rating.Description;
                if (string.IsNullOrEmpty(ratingDesc)) {
                    ratingDesc = "No comment available";
                }

                str.Append("<div class='clear-space-two'></div>" + ratingDesc);
                str.Append("<div class='clear-space-two'></div>" + rating.DateRated);
                str.Append("</div>");
            }
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            return "<b>Reviews</b><div class='clear-space-two'></div>" + str.ToString() + "<div class='clear-space'></div>";
        }
        else {
            return "<b>No Reviews Available</b><div class='clear-space'></div>";
        }
    }

    public void BuildAppStats(System.Web.UI.WebControls.Panel wlmd_holder, string appId, string username) {
        wlmd_holder.Controls.Clear();
        var str = new System.Text.StringBuilder();

        ServerSettings _ss = new ServerSettings();
        List<UserApps_Coll> db = GetUserApps_AllUsers(appId);

        string appName = GetAppName(appId);
        int numberInstalled = GetTotalInstalledApp(appId);
        int numberOpened = 0;
        int numberClosed = 0;
        int numberMinimized = 0;
        string modified = "N/A";
        string appIcon = GetAppIconName(appId);

        if (db != null) {
            List<string> usersTemp = new List<string>();
            foreach (UserApps_Coll dr in db) {
                if (!usersTemp.Contains(dr.Username.ToLower())) {
                    usersTemp.Add(dr.Username.ToLower());
                    modified = dr.DateUpdated.ToString();
                    if (dr.Closed)
                        numberClosed++;
                    else
                        numberOpened++;

                    if (dr.Minimized)
                        numberMinimized++;
                }
            }
        }

        int[] pieChart = { numberInstalled, numberClosed, numberOpened, numberMinimized };

        MemberDatabase member = new MemberDatabase(_username);
        string siteTheme = member.SiteTheme;
        string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
        string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/App_Icons/" + appIcon + "' class='pad-right float-left' style='height: 32px;' />";
        if (_ss.HideAllAppIcons)
            imageIcon = string.Empty;

        str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + appName + " Statistics</h2></div><div class='clear-space'></div>");
        str.Append("<div class='clear-space'></div><div align='center'><img alt='' src='" + GoogleChartBuilder(pieChart, 570, 190) + "' /></div>");
        str.Append("<div class='clear-space'></div><div class='clear-space'></div>");
        str.Append("<div class='float-left' style='padding: 4px;'><div class='font-bold pad-right-sml float-left'>Note:</div>These statistics are based on all users current app properties.</div>");
        str.Append("<div class='float-right font-bold margin-right-sml'><a href='#refresh' onclick='openWSE.AppStats(\"" + appId + "\");return false;' class='sb-links'>Refresh</a></div>");
        str.Append("<div class='clear'></div><div style='padding: 4px;'><div class='font-bold pad-right-sml float-left'>Last Modified:</div>" + modified + "</div>");
        wlmd_holder.Controls.Add(new System.Web.UI.LiteralControl(str.ToString()));
    }
    public string BuildAppStats(string appId, string username) {
        var str = new System.Text.StringBuilder();

        ServerSettings _ss = new ServerSettings();
        List<UserApps_Coll> db = GetUserApps_AllUsers(appId);

        string appName = GetAppName(appId);
        int numberInstalled = GetTotalInstalledApp(appId);
        int numberOpened = 0;
        int numberClosed = 0;
        int numberMinimized = 0;
        string modified = "N/A";
        string appIcon = GetAppIconName(appId);

        if (db != null) {
            List<string> usersTemp = new List<string>();
            foreach (UserApps_Coll dr in db) {
                if (!usersTemp.Contains(dr.Username.ToLower())) {
                    usersTemp.Add(dr.Username.ToLower());
                    modified = dr.DateUpdated.ToString();
                    if (dr.Closed)
                        numberClosed++;
                    else
                        numberOpened++;

                    if (dr.Minimized)
                        numberMinimized++;
                }
            }
        }

        int[] pieChart = { numberInstalled, numberClosed, numberOpened, numberMinimized };

        MemberDatabase member = new MemberDatabase(_username);
        string siteTheme = member.SiteTheme;
        string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
        string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/App_Icons/" + appIcon + "' class='pad-right float-left' style='height: 32px;' />";
        if (_ss.HideAllAppIcons)
            imageIcon = string.Empty;

        str.Append(imageIcon + "<div class='float-left'><h2 class='float-left pad-top'>" + appName + " Statistics</h2></div><div class='clear-space'></div>");
        str.Append("<div class='clear-space'></div><div class='app-stats-holder-wr'><img alt='' src='" + GoogleChartBuilder(pieChart, 500, 170) + "' /></div>");
        str.Append("<div class='clear-space'></div><div class='clear-space'></div>");
        str.Append("<div class='float-left' style='padding: 4px;'><div class='font-bold pad-right-sml float-left'>Note:</div>These statistics are based on all users current app properties.</div>");
        str.Append("<div class='float-right font-bold margin-right-sml'><a href='#' onclick='appRemote.AppStats(\"" + appId + "\");return false;' class='sb-links'>Refresh</a></div>");
        str.Append("<div class='clear'></div><div style='padding: 4px;'><div class='font-bold pad-right-sml float-left'>Last Modified:</div>" + modified + "</div>");
        return str.ToString();
    }

    public int GetTotalInstalledApp(string appId) {
        int numberInstalled = 0;

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser m in coll) {
            MemberDatabase mem = new MemberDatabase(m.UserName);
            List<string> apps = mem.EnabledApps;
            foreach (string w in apps) {
                if (w == appId) {
                    numberInstalled++;
                    break;
                }
            }
        }

        return numberInstalled;
    }

    public static string GoogleChartBuilder(int[] chartData, int width, int height) {
        GoogleChartSharp.PieChart pieChart = new GoogleChartSharp.PieChart(width, height, GoogleChartSharp.PieChartType.TwoD);

        string[] xAxis = { "Installed (" + chartData[0] + ")", "Closed (" + chartData[1] + ")", "Opened (" + chartData[2] + ")", "Minimized (" + chartData[3] + ")" };

        pieChart.AddAxis(new GoogleChartSharp.ChartAxis(GoogleChartSharp.ChartAxisType.Bottom, xAxis));
        pieChart.AddAxis(new GoogleChartSharp.ChartAxis(GoogleChartSharp.ChartAxisType.Left));
        pieChart.SetData(chartData);

        pieChart.AddSolidFill(new GoogleChartSharp.SolidFill(GoogleChartSharp.ChartFillTarget.Background, "F9F9F9"));
        pieChart.SetDatasetColors(new string[] { "72BAE4", "D44122", "B2D357", "F4C643" });

        return pieChart.GetUrl();
    }

    private void BuildAppColl(Dictionary<string, string> myReader, ref List<Apps_Coll> _appColl) {
        string id = myReader["ID"];
        string appID = myReader["AppID"];
        string appName = myReader["Name"];
        string filename = myReader["Filename"];
        string icon = myReader["Icon"];
        string allowResize = myReader["AllowResize"];
        string allowMaximize = myReader["AllowMaximize"];
        string about = myReader["About"];
        string description = myReader["Description"];
        string cssClass = myReader["CssClass"];
        string autoLoad = myReader["AutoLoad"];
        string autoFullScreen = myReader["AutoFullScreen"];
        string category = myReader["Category"];
        string minHeight = myReader["MinHeight"];
        string minWidth = myReader["MinWidth"];
        string createdBy = myReader["CreatedBy"];
        string allowParams = myReader["AllowParams"];
        string allowPopOut = myReader["AllowPopOut"];
        string popOutLoc = myReader["PopOutLoc"];
        string overlayID = myReader["OverlayID"];
        string notificationID = myReader["NotificationID"];
        string displayNav = myReader["DisplayNav"];
        string allowStats = myReader["AllowStats"];
        string autoOpen = myReader["AutoOpen"];
        string defaultWorkspace = myReader["DefaultWorkspace"];
        string isPrivate = myReader["IsPrivate"];

        Apps_Coll coll = new Apps_Coll(id, appID, appName, filename, icon, allowResize, allowMaximize, about, description, cssClass, autoLoad, autoFullScreen, category, minHeight, minWidth, createdBy, allowParams, allowPopOut, popOutLoc, overlayID, notificationID, displayNav, allowStats, autoOpen, defaultWorkspace, isPrivate);
        _appColl.Add(coll);
    }

    private void BuildUserAppColl(Dictionary<string, string> myReader, ref List<UserApps_Coll> _userappColl) {
        string id = myReader["ID"];
        string un = myReader["UserName"];
        string appID = myReader["AppID"];
        string appName = myReader["AppName"];
        string posX = myReader["PosX"];
        string posY = myReader["PosY"];
        string width = myReader["Width"];
        string height = myReader["Height"];
        string closed = myReader["Closed"];
        string minimized = myReader["Minimized"];
        string maximized = myReader["Maximized"];
        string workspace = myReader["Workspace"];
        string dateTime = myReader["DateTime"];
        UserApps_Coll coll = new UserApps_Coll(id, un, appID, appName, posX, posY, width, height, closed, minimized, maximized, workspace, dateTime);
        _userappColl.Add(coll);
    }
}