using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class UserAppSettings {

    private string _username = string.Empty;
    private bool _allowOverrides;
    private const string UserSettingsTableName = "aspnet_UserAppSettings";
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public UserAppSettings(string username, bool allowOverrides) {
        if (!string.IsNullOrEmpty(username)) {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                username = GroupSessions.GetUserGroupSessionName(username);
            }
            _username = username.ToLower();
        }
        _allowOverrides = allowOverrides;
    }

    public bool AllowOverrides {
        get {
            return _allowOverrides;
        }
    }

    private List<Dictionary<string, string>> _userAppSettingList = new List<Dictionary<string, string>>();
    public List<Dictionary<string, string>> UserAppSettingList {
        get {
            return _userAppSettingList;
        }
    }

    public void AddNewAppSetting(string appId, bool allowResize, bool allowMax, string cssClass,  bool autoFullScreen, string minHeight, string minWidth, bool allowPopOut, string popOutLoc, bool autoOpen, string defaultWorkspace, string categories, string appBackgroundColor, string iconBackgroundColor) {
            if (!string.IsNullOrEmpty(_username)) {
                string _allowResize = "0";
                if (allowResize) {
                    _allowResize = "1";
                }

                string _allowMax = "0";
                if (allowMax) {
                    _allowMax = "1";
                }

                string _autoFullScreen = "0";
                if (autoFullScreen) {
                    _autoFullScreen = "1";
                }

                string _allowPopOut = "0";
                if (allowPopOut) {
                    _allowPopOut = "1";
                }

                string _autoOpen = "0";
                if (autoOpen) {
                    _autoOpen = "1";
                }

                if (string.IsNullOrEmpty(iconBackgroundColor)) {
                    iconBackgroundColor = "#FFFFFF";
                }

                bool isUpdateNeeded = HasOverrides(appId);

                List<DatabaseQuery> query = new List<DatabaseQuery>();
                List<DatabaseQuery> query2 = new List<DatabaseQuery>();
                if (!isUpdateNeeded) {
                    query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
                    query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
                    query.Add(new DatabaseQuery("AppID", appId));
                    query.Add(new DatabaseQuery("Username", _username));
                }
                else {
                    query2.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
                    query2.Add(new DatabaseQuery("Username", _username));
                    query2.Add(new DatabaseQuery("AppID", appId));
                }
                
                query.Add(new DatabaseQuery("AllowResize", _allowResize));
                query.Add(new DatabaseQuery("AllowMaximize", _allowMax));
                query.Add(new DatabaseQuery("CssClass", cssClass));
                query.Add(new DatabaseQuery("AutoFullScreen", _autoFullScreen));
                query.Add(new DatabaseQuery("MinHeight", minHeight));
                query.Add(new DatabaseQuery("MinWidth", minWidth));
                query.Add(new DatabaseQuery("AllowPopOut", _allowPopOut));
                query.Add(new DatabaseQuery("PopOutLoc", popOutLoc));
                query.Add(new DatabaseQuery("AutoOpen", _autoOpen));
                query.Add(new DatabaseQuery("DefaultWorkspace", defaultWorkspace));
                query.Add(new DatabaseQuery("Category", categories));
                query.Add(new DatabaseQuery("AppBackgroundColor", appBackgroundColor));
                query.Add(new DatabaseQuery("IconBackgroundColor", iconBackgroundColor));

                if (!isUpdateNeeded) {
                    dbCall.CallInsert(UserSettingsTableName, query);
                }
                else {
                    dbCall.CallUpdate(UserSettingsTableName, query, query2);
                }
            }
    }

    public void UpdateCategorySetting(string appId, string categories) {
        bool isUpdateNeeded = HasOverrides(appId);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        List<DatabaseQuery> query2 = new List<DatabaseQuery>();
        if (!isUpdateNeeded) {
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("AppID", appId));
            query.Add(new DatabaseQuery("Username", _username));

            App app = new App(_username, false);
            Apps_Coll appColl = app.GetAppInformation(appId);

            string _allowResize = "0";
            if (appColl.AllowResize) {
                _allowResize = "1";
            }

            string _allowMax = "0";
            if (appColl.AllowMaximize) {
                _allowMax = "1";
            }

            string _autoFullScreen = "0";
            if (appColl.AutoFullScreen) {
                _autoFullScreen = "1";
            }

            string _allowPopOut = "0";
            if (appColl.AllowPopOut) {
                _allowPopOut = "1";
            }

            string _autoOpen = "0";
            if (appColl.AutoOpen) {
                _autoOpen = "1";
            }

            string iconBackgroundColor = "#FFFFFF";
            if (!string.IsNullOrEmpty(appColl.IconBackgroundColor)) {
                iconBackgroundColor = appColl.IconBackgroundColor;
            }

            string appBackgroundColor = "#FFFFFF";
            if (!string.IsNullOrEmpty(appColl.AppBackgroundColor)) {
                appBackgroundColor = appColl.AppBackgroundColor;
            }

            query.Add(new DatabaseQuery("AllowResize", _allowResize));
            query.Add(new DatabaseQuery("AllowMaximize", _allowMax));
            query.Add(new DatabaseQuery("CssClass", appColl.CssClass));
            query.Add(new DatabaseQuery("AutoFullScreen", _autoFullScreen));
            query.Add(new DatabaseQuery("MinHeight", appColl.MinHeight));
            query.Add(new DatabaseQuery("MinWidth", appColl.MinWidth));
            query.Add(new DatabaseQuery("AllowPopOut", _allowPopOut));
            query.Add(new DatabaseQuery("PopOutLoc", appColl.PopOutLoc));
            query.Add(new DatabaseQuery("AutoOpen", _autoOpen));
            query.Add(new DatabaseQuery("DefaultWorkspace", appColl.DefaultWorkspace));
            query.Add(new DatabaseQuery("AppBackgroundColor", appBackgroundColor));
            query.Add(new DatabaseQuery("IconBackgroundColor", iconBackgroundColor));
        }
        else {
            query2.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query2.Add(new DatabaseQuery("Username", _username));
            query2.Add(new DatabaseQuery("AppID", appId));
        }

        query.Add(new DatabaseQuery("Category", categories));

        if (!isUpdateNeeded) {
            dbCall.CallInsert(UserSettingsTableName, query);
        }
        else {
            dbCall.CallUpdate(UserSettingsTableName, query, query2);
        }
    }

    public void DeleteAppSetting(string appId) {
        if (!string.IsNullOrEmpty(_username)) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("AppID", appId));
            query.Add(new DatabaseQuery("Username", _username));
            dbCall.CallDelete(UserSettingsTableName, query);
        }
    }

    public void DeleteAllAppSetting() {
        if (!string.IsNullOrEmpty(_username)) {
            dbCall.CallDelete(UserSettingsTableName, new List<DatabaseQuery>() { new DatabaseQuery("Username", _username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        }
    }

    public Dictionary<string, string> GetUserAppSettings(string appId) {
        if (!string.IsNullOrEmpty(_username) && _allowOverrides) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("AppID", appId));
            query.Add(new DatabaseQuery("Username", _username));

            List<Dictionary<string, string>> selectQuery = dbCall.CallSelect(UserSettingsTableName, "", query);
            if (selectQuery.Count == 1) {
                return selectQuery[0];
            }
        }

        return new Dictionary<string, string>();
    }

    public void BuildUserAppSettings() {
        if (!string.IsNullOrEmpty(_username) && _allowOverrides) {
            _userAppSettingList = dbCall.CallSelect(UserSettingsTableName, "", new List<DatabaseQuery>() { new DatabaseQuery("Username", _username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        }
    }

    public Dictionary<string, string> OverrideValues(Dictionary<string, string> defaultAppSettings) {
        if (!_allowOverrides || string.IsNullOrEmpty(_username)) {
            return defaultAppSettings;
        }

        foreach (Dictionary<string, string> settings in _userAppSettingList) {
            bool allowUserOverrides = HelperMethods.ConvertBitToBoolean(defaultAppSettings["AllowUserOverrides"]);
            if (string.IsNullOrEmpty(defaultAppSettings["AllowUserOverrides"])) {
                allowUserOverrides = true;
            }

            if (settings["AppID"] == defaultAppSettings["AppID"] && allowUserOverrides) {
                if (!string.IsNullOrEmpty(settings["AllowResize"])) {
                    defaultAppSettings["AllowResize"] = settings["AllowResize"];
                }

                if (!string.IsNullOrEmpty(settings["AllowMaximize"])) {
                    defaultAppSettings["AllowMaximize"] = settings["AllowMaximize"];
                }

                if (!string.IsNullOrEmpty(settings["CssClass"])) {
                    defaultAppSettings["CssClass"] = settings["CssClass"];
                }

                if (!string.IsNullOrEmpty(settings["AutoFullScreen"])) {
                    defaultAppSettings["AutoFullScreen"] = settings["AutoFullScreen"];
                }

                if (!string.IsNullOrEmpty(settings["MinHeight"])) {
                    defaultAppSettings["MinHeight"] = settings["MinHeight"];
                }

                if (!string.IsNullOrEmpty(settings["MinWidth"])) {
                    defaultAppSettings["MinWidth"] = settings["MinWidth"];
                }

                if (!string.IsNullOrEmpty(settings["AllowPopOut"])) {
                    defaultAppSettings["AllowPopOut"] = settings["AllowPopOut"];
                }

                if (!string.IsNullOrEmpty(settings["PopOutLoc"])) {
                    defaultAppSettings["PopOutLoc"] = settings["PopOutLoc"];
                }

                if (!string.IsNullOrEmpty(settings["AutoOpen"])) {
                    defaultAppSettings["AutoOpen"] = settings["AutoOpen"];
                }

                if (!string.IsNullOrEmpty(settings["DefaultWorkspace"])) {
                    defaultAppSettings["DefaultWorkspace"] = settings["DefaultWorkspace"];
                }

                if (!string.IsNullOrEmpty(settings["Category"])) {
                    defaultAppSettings["Category"] = settings["Category"];
                }

                if (!string.IsNullOrEmpty(settings["IconBackgroundColor"])) {
                    defaultAppSettings["IconBackgroundColor"] = settings["IconBackgroundColor"];
                }

                if (!string.IsNullOrEmpty(settings["AppBackgroundColor"])) {
                    defaultAppSettings["AppBackgroundColor"] = settings["AppBackgroundColor"];
                }
                break;
            }
        }

        return defaultAppSettings;
    }

    public string GetUserOverrideCategories(string appId, string originalVal) {
        if (!_allowOverrides || string.IsNullOrEmpty(_username)) {
            return originalVal;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppList", "AllowUserOverrides", query);
        bool allowUserOverrides = HelperMethods.ConvertBitToBoolean(dbSelect.Value);
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            allowUserOverrides = true;
        }

        foreach (Dictionary<string, string> settings in _userAppSettingList) {
            if (settings["AppID"] == appId && allowUserOverrides) {
                return settings["Category"];
            }
        }

        return originalVal;
    }

    public bool HasOverrides(string appId, bool allowUserOverrides) {
        foreach (Dictionary<string, string> settings in _userAppSettingList) {
            if (settings["AppID"] == appId && allowUserOverrides) {
                return true;
            }
        }

        return false;
    }

    private bool HasOverrides(string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("Username", _username));

        List<Dictionary<string, string>> selectQuery = dbCall.CallSelect(UserSettingsTableName, "", query);
        if (selectQuery.Count > 0) {
            return true;
        }

        return false;
    }
}