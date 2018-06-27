#region

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Web.Security;
using OpenWSE_Tools.Overlays;
using System.Text.RegularExpressions;

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
    private readonly bool _autoOpen;
    private readonly string _defaultWorkspace;
    private readonly bool _isPrivate;
    private readonly bool _allowUserOverride;
    private readonly string _appBackgroundColor;
    private readonly string _iconBackgroundColor;
    private readonly bool _allowNotifications;

    public Apps_Coll() { }
    public Apps_Coll(string id, string appId, string name, string filename, string icon, string allowResize, string allowMaximize, string about, string description, string cssClass, string autoLoad, string autoFullScreen, string category, string minHeight, string minWidth, string createdBy, string allowParams, string allowPopOut, string popOutLoc, string overlayID, string autoOpen, string defaultWorkspace, string isPrivate, string allowUserOverride, string appBackgroundColor, string iconBackgroundColor, string allowNotifications) {
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
        _autoOpen = HelperMethods.ConvertBitToBoolean(autoOpen);
        _defaultWorkspace = defaultWorkspace;
        _isPrivate = HelperMethods.ConvertBitToBoolean(isPrivate);
        if (string.IsNullOrEmpty(allowUserOverride)) {
            _allowUserOverride = true;
        }
        else {
            _allowUserOverride = HelperMethods.ConvertBitToBoolean(allowUserOverride);
        }
        _appBackgroundColor = appBackgroundColor;
        _iconBackgroundColor = iconBackgroundColor;
        _allowNotifications = HelperMethods.ConvertBitToBoolean(allowNotifications);
    }

    public string ID {
        get { return _id; }
    }

    public string AppId {
        get {
            return App.GetFullAppId(_appId);
        }
    }

    public string AppName {
        get { return _name; }
    }

    public string filename {
        get { return _filename; }
    }

    public string Icon {
        get {
            return App.GetFullAppIconPath(_filename, _icon);
        }
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
        get {
            if (!string.IsNullOrEmpty(_category) && _category.Contains(";")) {
                List<string> tempList = new List<string>();
                string[] splitList = _category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string categoryItem in splitList) {
                    if (!tempList.Contains(categoryItem)) {
                        tempList.Add(categoryItem);
                    }
                }

                return string.Join(ServerSettings.StringDelimiter, tempList.ToArray());
            }

            return _category;
        }
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

    public bool AutoOpen {
        get { return _autoOpen; }
    }

    public string DefaultWorkspace {
        get { return _defaultWorkspace; }
    }

    public bool IsPrivate {
        get { return _isPrivate; }
    }

    public bool AllowUserOverrides {
        get {
            return _allowUserOverride;
        }
    }

    public string AppBackgroundColor {
        get {
            if (string.IsNullOrEmpty(_appBackgroundColor)) {
                return "inherit";
            }
            return _appBackgroundColor;
        }
    }

    public string IconBackgroundColor {
        get {
            if (string.IsNullOrEmpty(_iconBackgroundColor)) {
                return "inherit";
            }
            return _iconBackgroundColor;
        }
    }

    public bool AllowNotifications {
        get { return _allowNotifications; }
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
    private DateTime _dateTime = ServerSettings.ServerDateTime;

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
        get {
            return App.GetFullAppId(_appId);
        }
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
    private UserAppSettings _userAppSettings;

    public const string DefaultAppIcon = "default.png";
    public const string DefaultAppIconLocation = "Standard_Images/App_Icons/";

    public App(string username, bool allowUserOverrides = true) {
        _username = username;
        if (string.IsNullOrEmpty(username)) {
            allowUserOverrides = false;
        }

        _userAppSettings = new UserAppSettings(_username, allowUserOverrides);
        _userAppSettings.BuildUserAppSettings();
    }

    public List<Apps_Coll> AppList {
        get { return _dataTable_apps; }
    }

    public List<UserApps_Coll> UserAppList {
        get { return _dataTable_users; }
    }

    public void CreateItem(string appid, string name, string filename, string icon, string ar, string am,
                           string about, string description, string css, string category, string height, string width,
                           bool allowparams, bool allowpopout, string popoutloc, string overlayId, bool autoOpen, 
                           string defaultWorkspace, bool isPrivate, bool allowUserOverrides, string appBackgroundColor, 
                           string iconBackgroundColor, bool allowNotifications) {
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

        int _autoOpen = 0;
        if (autoOpen) {
            _autoOpen = 1;
        }

        int _isPrivate = 0;
        if (isPrivate) {
            _isPrivate = 1;
        }

        int _allowUserOverrides = 0;
        if (allowUserOverrides) {
            _allowUserOverrides = 1;
        }

        int _allowNotifications = 0;
        if (allowNotifications) {
            _allowNotifications = 1;
        }

        name = name.Trim();
        if (name.Length > 20)
            name = name.Substring(0, 20);

        appid = App.GetFullAppId(appid);
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery("AutoOpen", _autoOpen.ToString()));
        query.Add(new DatabaseQuery("DefaultWorkspace", defaultWorkspace));
        query.Add(new DatabaseQuery("IsPrivate", _isPrivate.ToString()));
        query.Add(new DatabaseQuery("AllowUserOverrides", _allowUserOverrides.ToString()));
        query.Add(new DatabaseQuery("AppBackgroundColor", appBackgroundColor));
        query.Add(new DatabaseQuery("IconBackgroundColor", iconBackgroundColor));
        query.Add(new DatabaseQuery("AllowNotifications", _allowNotifications.ToString()));

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
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
            query.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString()));

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
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
            query.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString()));

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
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", _username));
            query.Add(new DatabaseQuery("AppID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("PosX", x.Trim()));
            updateQuery.Add(new DatabaseQuery("PosY", y.Trim()));
            updateQuery.Add(new DatabaseQuery("Closed", c.Trim()));
            updateQuery.Add(new DatabaseQuery("Minimized", m.Trim()));
            updateQuery.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString().Trim()));
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
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", _username));
            query.Add(new DatabaseQuery("AppID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Closed", c.Trim()));
            updateQuery.Add(new DatabaseQuery("Minimized", m.Trim()));
            updateQuery.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString().Trim()));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString().Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void UpdateSize(string id, string width, string height) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Maximized", maximized.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public void Updateworkspace(string id, string workspace) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Workspace", workspace.Trim()));

        dbCall.CallUpdate("UserApps", updateQuery, query);
    }

    public string GetAppID(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("Name", name), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public string GetAppIDbyFilename(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("Filename", name), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public string GetAppCssClass(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "CssClass", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public bool GetAppAutoOpen(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoOpen", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool GetAllowNotifications(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowNotifications", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetCurrentworkspace(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Workspace", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public bool GetAppMax(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Maximized", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetAppWidth(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Width", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        return string.Empty;
    }

    public string GetAppHeight(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Height", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        return string.Empty;
    }

    public bool GetClosedState(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "Closed", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
        }

        return true;
    }

    public Apps_Coll GetAppInformation(string appid) {
        appid = App.GetFullAppId(appid);
        Apps_Coll db = new Apps_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> val in dbSelect) {
            Dictionary<string, string> row = _userAppSettings.OverrideValues(val);
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
            string autoOpen = row["AutoOpen"];
            string defaultWorkspace = row["DefaultWorkspace"];
            string isPrivate = row["IsPrivate"];
            string allowUserOverrides = row["AllowUserOverrides"];
            string appBackgroundColor = row["AppBackgroundColor"];
            string iconBackgroundColor = row["IconBackgroundColor"];
            string allowNotifications = row["AllowNotifications"];

            db = new Apps_Coll(id, appID, appName, filename, icon, allowResize, allowMaximize, about, description, cssClass, autoLoad, autoFullScreen, category, minHeight, minWidth, createdBy, allowParams, allowPopOut, popOutLoc, overlayID, autoOpen, defaultWorkspace, isPrivate, allowUserOverrides, appBackgroundColor, iconBackgroundColor, allowNotifications);
            break;
        }

        return db;
    }

    public UserApps_Coll GetUserAppInformation(string appid) {
        appid = App.GetFullAppId(appid);
        UserApps_Coll db = new UserApps_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Filename", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public bool AllowMaximize(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowMaximize", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool AllowResize(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowResize", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool AllowParams(string appId) {
        appId = App.GetFullAppId(appId);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowParams", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool GetAutoLoad(string AppID) {
        AppID = App.GetFullAppId(AppID);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoLoad", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool GetAutoFullScreen(string AppID) {
        AppID = App.GetFullAppId(AppID);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AutoFullScreen", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public bool ItemInDatabase(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool ItemInDatabase(string user, string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", user), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool ItemInDatabase_name(string name) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("UserApps", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("AppName", name), new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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
                        catch (Exception e) {
                            AppLog.AddError(e);
                        }
                    }
                }
                else if (directoryname.ToLower() == "custom_tables") {
                    if (File.Exists(serverpath + "Apps\\Custom_Tables\\" + id.Replace("app-", "") + ".ascx")) {
                        try {
                            File.Delete(serverpath + "Apps\\Custom_Tables\\" + id.Replace("app-", "") + ".ascx");
                        }
                        catch (Exception e) {
                            AppLog.AddError(e);
                        }
                    }
                }
                else {
                    string appFn = GetAppFilename(id);
                    if (!HelperMethods.IsValidHttpBasedAppType(appFn)) {
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
                                    catch (Exception e) {
                                        AppLog.AddError(e);
                                    }
                                }
                            }

                            for (int j = 0; j < di.GetFiles().Length; j++) {
                                string filename = di.GetFiles()[j].Name;
                                if (filename == appFn) {
                                    try {
                                        di.GetFiles()[j].Delete();
                                    }
                                    catch (Exception e) {
                                        AppLog.AddError(e);
                                    }
                                }
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

            string appIcon = GetAppIconName(id);
            if (appIcon.ToLower() != App.DefaultAppIconLocation.ToLower() + App.DefaultAppIcon.ToLower() && appIcon.ToLower() != App.DefaultAppIconLocation.ToLower() + DBImporter.DefaultDatabaseIcon.ToLower()) {
                if (File.Exists(serverpath + appIcon.Replace("/", "\\"))) {
                    try {
                        File.Delete(serverpath + appIcon.Replace("/", "\\"));
                    }
                    catch (Exception e) {
                        AppLog.AddError(e);
                    }
                }
            }
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
            AppLog.AddError(e);
        }

        dbCall.CallDelete("AppList", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void DeleteAppMain(string id) {
        dbCall.CallDelete("AppList", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });

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
            AppLog.AddError(e);
        }
    }

    public void DeleteAppLocal(string id) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void DeleteAppLocal(string id, string username) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("AppID", id), new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void UpdateAppFilename(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Filename", name));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppName(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Name", name));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAllowNotification(string id, bool enable) {
        string _enable = "0";
        if (enable) {
            _enable = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AllowNotifications", _enable));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppOverlayID(string id, string overlayId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("OverlayID", overlayId));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppLocal(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AutoLoad", l));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateCategory(string id, string category) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AutoFullScreen", l));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public string[] GetAppOverlayIds(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "OverlayID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });

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
        appid = App.GetFullAppId(appid);
        string filename = string.Empty;
        string icon = string.Empty;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "Icon, Filename", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> val in dbSelect) {
            Dictionary<string, string> row = _userAppSettings.OverrideValues(val);
            filename = row["Filename"];
            icon = row["Icon"];
        }

        return App.GetFullAppIconPath(filename, icon);
    }

    public Dictionary<string, string> GetCategoriesForApp(string appId) {
        appId = App.GetFullAppId(appId);
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Category", query);
        string categoryList = _userAppSettings.GetUserOverrideCategories(appId, dbSelect.Value);

        AppCategory appCategory = new AppCategory(false);
        Dictionary<string, string> categoryDictionary = appCategory.BuildCategoryDictionary(categoryList);

        return categoryDictionary;
    }

    public List<Apps_Coll> GetApps_byCategory(string categoryID) {
        List<Apps_Coll> db = new List<Apps_Coll>();
        if (_userAppSettings.AllowOverrides) {
            db = GetApps_byCategoryForSidebar(categoryID);
        }
        else {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SELECT * FROM AppList WHERE Category LIKE '%" + categoryID + "%' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
            foreach (Dictionary<string, string> row in dbSelect) {
                Dictionary<string, string> overriddenRow = _userAppSettings.OverrideValues(row);
                BuildAppColl(overriddenRow, ref db);
            }
        }

        return db;
    }

    public List<Apps_Coll> GetApps_byCategoryForSidebar(string categoryID) {
        List<Apps_Coll> db = new List<Apps_Coll>();
        List<string> appsOverridden = new List<string>();
        if (!string.IsNullOrEmpty(_username)) {
            string tempUsername = _username;
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                tempUsername = GroupSessions.GetUserGroupSessionName(_username);
            }

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SELECT AppID, Category FROM aspnet_UserAppSettings WHERE Username='" + tempUsername + "' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
            foreach (Dictionary<string, string> row in dbSelect) {
                Apps_Coll coll = GetAppInformation(row["AppID"]);
                if (coll != null && !string.IsNullOrEmpty(coll.ID) && coll.AllowUserOverrides) {
                    if (!appsOverridden.Contains(row["AppID"]) && row["Category"].Contains(categoryID)) {
                        db.Add(coll);
                    }

                    appsOverridden.Add(row["AppID"]);
                }
            }

            List<Dictionary<string, string>> dbSelect2 = dbCall.CallSelect("SELECT * FROM AppList WHERE Category LIKE '%" + categoryID + "%' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
            foreach (Dictionary<string, string> row in dbSelect2) {
                if (!appsOverridden.Contains(row["AppID"])) {
                    Dictionary<string, string> overriddenRow = _userAppSettings.OverrideValues(row);
                    BuildAppColl(overriddenRow, ref db);
                    appsOverridden.Add(row["AppID"]);
                }
            }
        }

        return db;
    }

    public string GetAppName(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "Name", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public bool GetIsPrivate(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "IsPrivate", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public string GetAppCreatedBy(string appid) {
        appid = App.GetFullAppId(appid);
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "CreatedBy", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public void GetUserInstalledApps() {
        _dataTable_users.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref _dataTable_users);
        }
    }

    public UserApps_Coll GetUserInstalledApp_Coll(string appId) {
        appId = App.GetFullAppId(appId);
        UserApps_Coll coll = new UserApps_Coll();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("AppID", appId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "AppID, Workspace", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("Closed", "0"), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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
        appId = App.GetFullAppId(appId);
        List<string> list = new List<string>();

        string selectCols = "PosX, PosY, Width, Height, Closed, Minimized, Maximized, Workspace, DateTime";

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", selectCols, new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("AppID", appId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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
        appId = App.GetFullAppId(appId);
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", string.Empty, new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> val in dbSelect) {
            Dictionary<string, string> row = _userAppSettings.OverrideValues(val);
            string MinHeight = row["MinHeight"];
            string MinWidth = row["MinWidth"];
            string Filename = row["Filename"];
            string AllowMaximize = row["AllowMaximize"];
            string AllowResize = row["AllowResize"];

            if (HelperMethods.IsValidAscxOrDllFile(Filename) || (!HelperMethods.IsValidHttpBasedAppType(Filename) && (Filename.ToLower().EndsWith(".html") || Filename.ToLower().EndsWith(".htm")))) {
                Filename = ServerSettings.ResolveUrl("~/ExternalAppHolder.aspx?appId=" + appId);
            }
            else if (!string.IsNullOrEmpty(Filename) && !HelperMethods.IsValidHttpBasedAppType(Filename) && HelperMethods.IsValidAspxFile(Filename)) {
                Filename = ServerSettings.ResolveUrl("~/Apps/" + Filename);
            }

            list.Add(MinHeight);
            list.Add(MinWidth);
            list.Add(Filename);
            list.Add(AllowMaximize);
            list.Add(AllowResize);
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

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", "UserName=@UserName AND AppID LIKE 'app-ChatClient-%'", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref _dataTable_users);
        }
    }

    public void GetAllApps() {
        _dataTable_apps.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Name ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            Dictionary<string, string> overriddenRow = _userAppSettings.OverrideValues(row);
            BuildAppColl(overriddenRow, ref _dataTable_apps);
        }
    }

    public void GetAllAscxApps() {
        _dataTable_apps.Clear();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AppList", "", "LOWER(Filename) LIKE '%.ascx'", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });

        foreach (Dictionary<string, string> row in dbSelect) {
            Dictionary<string, string> overriddenRow = _userAppSettings.OverrideValues(row);
            BuildAppColl(overriddenRow, ref _dataTable_apps);
        }
    }

    public void UpdateAppImage(string id, string img) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AllowParams", ap.ToString()));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void UpdateAppList(string id, string name, string ar, string am, string about, string description, string css, string height, string width, bool allowpopout, string popoutloc, bool autoOpen, string defaultWorkspace, bool isPrivate, bool allowUserOverrides, string appBackgroundColor, string iconBackgroundColor, bool allowNotifications) {
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

        string _autoOpen = "0";
        if (autoOpen) {
            _autoOpen = "1";
        }

        string _isPrivate = "0";
        if (isPrivate) {
            _isPrivate = "1";
        }

        string _allowUserOverrides = "0";
        if (allowUserOverrides) {
            _allowUserOverrides = "1";
        }

        string _allowNotifications = "0";
        if (allowNotifications) {
            _allowNotifications = "1";
        }

        int h = 0;
        int w = 0;
        int.TryParse(height, out h);
        int.TryParse(width, out w);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        updateQuery.Add(new DatabaseQuery("AutoOpen", _autoOpen));
        updateQuery.Add(new DatabaseQuery("DefaultWorkspace", defaultWorkspace));
        updateQuery.Add(new DatabaseQuery("IsPrivate", _isPrivate));
        updateQuery.Add(new DatabaseQuery("AllowUserOverrides", _allowUserOverrides));
        updateQuery.Add(new DatabaseQuery("AppBackgroundColor", appBackgroundColor));
        updateQuery.Add(new DatabaseQuery("IconBackgroundColor", iconBackgroundColor));
        updateQuery.Add(new DatabaseQuery("AllowNotifications", _allowNotifications));

        dbCall.CallUpdate("AppList", updateQuery, query);
    }

    public void DeleteUserProperties(string username) {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.Trim()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void DeleteUserProperties() {
        dbCall.CallDelete("UserApps", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
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

        if (dup > 0 && !_m.DoesHaveGroupDefaults) {
            _m.RemoveAllEnabledApps();
            foreach (string d in temp) {
                _m.UpdateEnabledApps(d);
            }
        }

        return temp;
    }

    public bool HasApp(string appid) {
        appid = App.GetFullAppId(appid);
        UserApps_Coll coll = GetUserInstalledApp_Coll(appid);
        if (!string.IsNullOrEmpty(coll.ID)) {
            return true;
        }

        return false;
    }

    public List<UserApps_Coll> GetUserApps_AllUsers(string appid) {
        appid = App.GetFullAppId(appid);
        List<UserApps_Coll> db = new List<UserApps_Coll>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appid));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("UserApps", "", query, "DATETIME ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            BuildUserAppColl(row, ref db);
        }

        return db;
    }

    public void BuildAboutApp(System.Web.UI.WebControls.Panel wlmd_holder, string appId, string username) {
        appId = App.GetFullAppId(appId);
        wlmd_holder.Controls.Clear();
        string AboutStr = BuildAboutApp(appId, username);
        wlmd_holder.Controls.Add(new System.Web.UI.LiteralControl(AboutStr));
    }
    public string BuildAboutApp(string appId, string username) {
        appId = App.GetFullAppId(appId);
        var str = new System.Text.StringBuilder();
        ServerSettings _ss = new ServerSettings();
        if (appId.Contains("app-ChatClient-")) {
            string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
            string imageIcon = "<img alt='icon' src='" + sitePath + "/Standard_Images/About Logos/chat.png' class='pad-right-big float-left' style='height: 48px;' />";

            str.Append("<div class='app-item-about'><div class='app-about-section'>" + imageIcon + "<div class='app-about-name' style='padding-top: 15px;'>Chat Client</div><div class='clear-space'></div></div>");

            #region Description, About, and Overlay
            str.Append("<div class='app-about-section'><div class='about-section-title'>Description</div>An instant messaging (IM) client that features real-time chat sessions with other users of an associated group.</div>");
            str.Append("<div class='app-about-section'><div class='about-section-title'>About</div>" + ServerSettings.SiteName + " - " + ServerSettings.ServerDateTime.Year + " John Donnelly</div>");
            #endregion

            #region App Properties
            str.Append("<div class='app-about-section'><div class='about-section-title'>App Properties</div>");
            str.Append("<table cellpadding='0' cellspacing='0'><tr><td class='pad-right-big' valign='top'>");
            str.Append("<div class='about-property'><span class='property-title'>Minimum Width</span><span class='property-value'>300px</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Minimum Height</span><span class='property-value'>400px</div>");
            str.Append("<div class='about-property'><span class='property-title'>Allow Maximize</span><span class='property-value'>True</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Allow Resize</span><span class='property-value'>True</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Default Workspace</span><span class='property-value'>1</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Background</span><span class='property-value'>Visible</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Allow Pop Out</span><span class='property-value'>True</span></div>");

            str.Append("</td><td class='pad-left-big' valign='top'>");

            str.Append("<div class='about-property'><span class='property-title'>Maximize on Load</span><span class='property-value'>False</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Allow Params</span><span class='property-value'>False</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Auto Open</span><span class='property-value'>False</span></div>");
            str.Append("<div class='about-property'><span class='property-title'>Allow Notifications</span><span class='property-value'>False</span></div>");

            str.Append("</td></tr></table>");
            str.Append("</div>");
            #endregion
        }
        else {
            App _apps = new App(_username, _userAppSettings.AllowOverrides);
            AppCategory _category = new AppCategory(false);
            Apps_Coll db = _apps.GetAppInformation(appId);
            if (db != null) {
                string about = db.About;
                string description = db.Description;
                if (string.IsNullOrEmpty(description))
                    description = "No description available";

                if (string.IsNullOrEmpty(about)) {
                    string tempuser = "John Donnelly";
                    if (string.IsNullOrEmpty(db.CreatedBy)) {
                        var tempmember = new MemberDatabase(db.CreatedBy);
                        tempuser = HelperMethods.MergeFMLNames(tempmember);
                    }
                    about = ServerSettings.SiteName + " | " + ServerSettings.ServerDateTime.Year + " | " + tempuser;
                }

                string background = "Visible";
                if (db.CssClass.ToLower() == "app-main-nobg")
                    background = "Hidden";

                string width = db.MinWidth;
                if (string.IsNullOrEmpty(width))
                {
                    width = "N/A";
                }
                else
                {
                    width = width + "px";
                }
                 
                string height = db.MinHeight;
                if (string.IsNullOrEmpty(height))
                {
                    height = "N/A";
                }
                else
                {
                    height = height + "px";
                }

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
                            overlay += "<div class='about-property'><span class='property-title'>" + tempColl.OverlayName + "</span><span class='property-value'>" + tempColl.Description + "</span></div>";
                        }
                    }
                }
                if (string.IsNullOrEmpty(overlay)) {
                    overlay = "None";
                }

                string sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);
                string imageIcon = "<img alt='icon' src='" + sitePath + "/" + db.Icon + "?" + ServerSettings.TimestampQuery + HelperMethods.GetTimestamp() + "' class='app-about-icon' />";

                if (string.IsNullOrEmpty(db.Icon))
                    imageIcon = string.Empty;

                bool allowAppRating = _ss.AllowAppRating;

                if (allowAppRating && !string.IsNullOrEmpty(_username) && _username.ToLower() != "demonologin") {
                    str.Append("<div class='app-item-about'><div class='app-about-section'>" + imageIcon + "<div class='float-left'><div class='app-about-name'>" + db.AppName + "</div>");
                }
                else {
                    str.Append("<div class='app-item-about'><div class='app-about-section'>" + imageIcon + "<div class='float-left'><div class='app-about-name' style='padding-top: 15px;'>" + db.AppName + "</div>");
                }

                if (allowAppRating && !string.IsNullOrEmpty(_username) && _username.ToLower() != "demonologin") {
                    str.Append("<div class='clear'></div><div class='app-rater-holder app-rater-" + db.AppId + "'></div>");
                }

                str.Append("<div class='clear'></div></div>");

                string categoryNames = string.Empty;
                Dictionary<string, string> categoryList = _category.BuildCategoryDictionary(db.Category);
                foreach (KeyValuePair<string, string> categoryItem in categoryList) {
                    categoryNames += categoryItem.Value + ", ";
                }

                categoryNames = categoryNames.Trim();
                if (categoryNames.EndsWith(",")) {
                    categoryNames = categoryNames.Remove(categoryNames.Length - 1);
                }

                str.Append("<div class='app-about-category'>" + categoryNames + "</div><div class='clear'></div></div><div class='clear'></div>");

                #region Description, About, and Overlay
                str.Append("<div class='app-about-section'><div class='about-section-title'>Description</div>" + description + "</div>");
                str.Append("<div class='app-about-section'><div class='about-section-title'>About</div>" + about + "</div>");
                str.Append("<div class='app-about-section'><div class='about-section-title'>Associated Overlays</div>" + overlay + "</div>");
                #endregion

                string appColor = db.AppBackgroundColor;
                if (string.IsNullOrEmpty(appColor)) {
                    appColor = "#FFFFFF";
                }

                string appColorBlock = string.Empty;
                if (appColor.ToLower() != "inherit") {
                    appColorBlock = "<div class='margin-right-sml float-left boxshadow' style='height:14px; width: 14px; background-color: " + appColor + ";'></div>";
                }

                #region App Properties
                str.Append("<div class='app-about-section'><div class='about-section-title'>App Properties</div>");
                str.Append("<table cellpadding='0' cellspacing='0'><tr><td class='pad-right-big' valign='top'>");
                str.Append("<div class='about-property'><span class='property-title'>Minimum Width</span><span class='property-value'>" + width + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Minimum Height</span><span class='property-value'>" + height + "</div>");
                str.Append("<div class='about-property'><span class='property-title'>Allow Maximize</span><span class='property-value'>" + db.AllowMaximize.ToString() + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Allow Resize</span><span class='property-value'>" + db.AllowResize.ToString() + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Default Workspace</span><span class='property-value'>" + defaultWorkspace + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Background</span><span class='property-value'>" + background + "</span></div>");
                if (background.ToLower() == "visible") {
                    str.Append("<div class='about-property'><span class='property-title'>Background Color</span><span class='property-value'>" + appColor + appColorBlock + "</span></div>");
                }
                str.Append("<div class='about-property'><span class='property-title'>Allow Pop Out</span><span class='property-value'>" + db.AllowPopOut.ToString() + "</span></div>");

                str.Append("</td><td class='pad-left-big' valign='top'>");



                string iconColor = db.IconBackgroundColor;
                if (string.IsNullOrEmpty(iconColor)) {
                    iconColor = "#FFFFFF";
                }

                string iconColorBlock = string.Empty;
                if (iconColor.ToLower() != "inherit") {
                    iconColorBlock = "<div class='margin-right-sml float-left boxshadow' style='height:14px; width: 14px; background-color: " + iconColor + ";'></div>";
                }

                str.Append("<div class='about-property'><span class='property-title'>Icon Color</span><span class='property-value'>" + iconColor + iconColorBlock + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Maximize on Load</span><span class='property-value'>" + db.AutoFullScreen + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Allow Params</span><span class='property-value'>" + db.AllowParams.ToString() + "</span></div>");
                str.Append("<div class='about-property'><span class='property-title'>Auto Open</span><span class='property-value'>" + db.AutoOpen + "</span></div>");

                if (!HelperMethods.IsValidHttpBasedAppType(db.filename)) {
                    string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + db.filename.Replace("/", "\\");
                    FileInfo tempFi = new FileInfo(filePath);
                    if ((tempFi.Directory.Name.ToLower() != "apps") && (db.filename.Contains('/'))) {
                        str.Append("<div class='about-property'><span class='property-title'>Allow Notifications</span><span class='property-value'>" + db.AllowNotifications.ToString() + "</span></div>");
                    }
                }

                str.Append("<div class='about-property'><span class='property-title'>Allow Overrides</span><span class='property-value'>" + db.AllowUserOverrides + "</span></div>");

                if (!string.IsNullOrEmpty(username)) {
                    if (_username.ToLower() == db.CreatedBy.ToLower() || BasePage.IsUserNameEqualToAdmin(_username)) {
                        str.Append("<div class='about-property'><span class='property-title'>Is Private</span><span class='property-value'>" + db.IsPrivate.ToString() + "</span></div>");
                    }

                    if (!BasePage.IsUserInAdminRole(username)) {
                        if (db.CreatedBy.ToLower() == username.ToLower()) {
                            if (HelperMethods.IsValidAscxOrDllFile(db.filename)) {
                                str.Append("<div class='about-property'><span class='property-title'>Auto Create</span><span class='property-value'>" + db.AutoLoad.ToString() + "</span></div>");
                            }
                        }
                        else {
                            if (HelperMethods.IsValidAscxOrDllFile(db.filename)) {
                                str.Append("<div class='about-property'><span class='property-title'>Auto Create</span><span class='property-value'>" + db.AutoLoad.ToString() + "</span></div>");
                            }
                        }
                    }
                    else {
                        if (HelperMethods.IsValidAscxOrDllFile(db.filename))
                            str.Append("<div class='about-property'><span class='property-title'>Auto Create</span><span class='property-value'>" + db.AutoLoad.ToString() + "</span></div>");
                    }
                }
                else {
                    if (HelperMethods.IsValidAscxOrDllFile(db.filename))
                        str.Append("<div class='about-property'><span class='property-title'>Auto Create</span><span class='property-value'>" + db.AutoLoad.ToString() + "</span></div>");
                }

                string createdBy_text = string.Empty;
                if (!string.IsNullOrEmpty(db.CreatedBy)) {
                    MemberDatabase tempm = new MemberDatabase(db.CreatedBy);
                    createdBy_text = HelperMethods.MergeFMLNames(tempm);
                }

                str.Append("<div class='about-property'><span class='property-title'>Created By</span><span class='property-value'>" + createdBy_text + "</span></div>");
                str.Append("</td></tr><tr><td valign='top' colspan='2'>");

                #region Popout Location
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

                    str.Append("<div class='about-property'><span class='property-title'>Pop Out Location</span><span class='property-value'>" + popoutloc + "</span></div>");
                }
                #endregion

                string _tempFilename = SpliceText(db.filename, 70);
                str.Append("<div class='about-property'><span class='property-title'>Location</span><span class='property-value'>" + _tempFilename + "</span></div>");

                str.Append("</td></tr></table>");
                str.Append("</div>");
                #endregion

                if (allowAppRating && !string.IsNullOrEmpty(_username)) {
                    // Get Reviews
                    str.Append("<div id='" + appId + "-rating-reviews'>" + GetReviews(appId) + "</div>");
                }
            }
        }
        return str.ToString() + "</div>";
    }
    public string GetReviews(string appId) {
        appId = App.GetFullAppId(appId);
        System.Text.StringBuilder str = new System.Text.StringBuilder();

        AppRatings ratings = new AppRatings();

        List<AppRatings_Coll> ratingColl = ratings.GetAppRatings(appId);
        foreach (AppRatings_Coll rating in ratingColl) {
            if (!string.IsNullOrEmpty(rating.Rating)) {
                str.Append("<div class='ratings-padding-div'>");

                string starId = Guid.NewGuid().ToString();
                MemberDatabase tempMember = new MemberDatabase(rating.UserName);
                string name = HelperMethods.MergeFMLNames(tempMember);
                if (rating.UserName.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    name = "You";
                }

                str.Append("<h4 class='float-left font-bold'>" + name + "</h4>");
                str.Append("<div class='float-left pad-left pad-top-sml ratingreviews-div'><div id='" + starId + "'></div></div>");

                RegisterPostbackScripts.RegisterStartupScript(null, "openWSE.RatingStyleInit('#" + starId + "', '" + rating.Rating + "', true, '" + appId + "', false);");

                string ratingDesc = rating.Description;
                if (string.IsNullOrEmpty(ratingDesc)) {
                    ratingDesc = "No comment available";
                }

                str.Append("<div class='clear-space-two'></div>" + ratingDesc);
                str.Append("<div class='clear-space-two'></div><span style='font-size: 10px;'>" + rating.DateRated + "</span>");
                str.Append("</div>");
            }
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            return "<h4>Reviews</h4><div class='clear-space-two'></div>" + str.ToString() + "<div class='clear-space'></div>";
        }
        else {
            return "<h4>No Reviews Available</h4><div class='clear-space'></div>";
        }
    }
    public static string SpliceText(string text, int lineLength) {
        return Regex.Replace(text, "(.{" + lineLength + "})", "$1" + "<br />");
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
        string autoOpen = myReader["AutoOpen"];
        string defaultWorkspace = myReader["DefaultWorkspace"];
        string isPrivate = myReader["IsPrivate"];
        string allowUserOverrides = myReader["AllowUserOverrides"];
        string appBackgroundColor = myReader["AppBackgroundColor"];
        string iconBackgroundColor = myReader["IconBackgroundColor"];
        string allowNotifications = myReader["AllowNotifications"];

        Apps_Coll coll = new Apps_Coll(id, appID, appName, filename, icon, allowResize, allowMaximize, about, description, cssClass, autoLoad, autoFullScreen, category, minHeight, minWidth, createdBy, allowParams, allowPopOut, popOutLoc, overlayID, autoOpen, defaultWorkspace, isPrivate, allowUserOverrides, appBackgroundColor, iconBackgroundColor, allowNotifications);
        if (!_appColl.Contains(coll)) {
            _appColl.Add(coll);
        }
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
        if (!_userappColl.Contains(coll)) {
            _userappColl.Add(coll);
        }
    }

    public static string GetFullAppIconPath(string filename, string icon) {
        if (!string.IsNullOrEmpty(filename) && !HelperMethods.IsValidHttpBasedAppType(filename)) {
            try {
                FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps/" + filename);
                string rootName = fi.Directory.Name;
                if (!string.IsNullOrEmpty(rootName) && rootName != "Apps") {
                    if (!string.IsNullOrEmpty(icon) && File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + rootName + "/" + icon)) {
                        return "Apps/" + rootName + "/" + icon;
                    }
                    else if (File.Exists(ServerSettings.GetServerMapLocation + "Apps/" + rootName + "/icon.png")) {
                        return "Apps/" + rootName + "/icon.png";
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        if (string.IsNullOrEmpty(icon) || !File.Exists(ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation + icon)) {
            icon = App.DefaultAppIcon;
        }

        return App.DefaultAppIconLocation + icon; 
    }

    public static string CreateFullAppIconPath(string filename, string icon) {
        icon = App.GetAppIconNameOnly(icon);
        if (!string.IsNullOrEmpty(filename) && !HelperMethods.IsValidHttpBasedAppType(filename)) {
            try {
                FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps/" + filename);
                string rootName = fi.Directory.Name;
                if (!string.IsNullOrEmpty(rootName) && rootName != "Apps") {
                    if (!string.IsNullOrEmpty(icon)) {
                        return "Apps/" + rootName + "/" + icon;
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        if (string.IsNullOrEmpty(icon) || !File.Exists(ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation + icon)) {
            icon = App.DefaultAppIcon;
        }

        return App.DefaultAppIconLocation + icon;
    }

    public static string GetAppIconNameOnly(string icon) {
        if (!string.IsNullOrEmpty(icon) && icon.Contains("/")) {
            string[] splitIconPath = icon.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitIconPath.Length > 0) {
                return splitIconPath[splitIconPath.Length - 1];
            }
        }
        return icon;
    }

    public static string GetFullAppId(string appid) {
        if (!string.IsNullOrEmpty(appid) && !appid.StartsWith("app-")) {
            return "app-" + appid;
        }

        return appid;
    }
}