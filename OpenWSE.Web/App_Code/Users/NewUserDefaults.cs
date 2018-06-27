using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;

public class NewUserDefaults {
    private Dictionary<string, string> data;
    private string _role = ServerSettings.AdminUserName;
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public NewUserDefaults(string role) {
        _role = role;
    }

    public static List<string> GetListOfNewUserDefaults() {
        DatabaseCall tempDbCall = new DatabaseCall();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = tempDbCall.CallSelect("NewUserDefaults", "Role", query);
        List<string> defaultList = new List<string>();

        foreach (Dictionary<string, string> role in dbSelect) {
            if (!defaultList.Contains(role["Role"])) {
                defaultList.Add(role["Role"]);
            }
        }

        return defaultList;
    }

    private static int _getDefaultsLoopCount = 0;
    public void GetDefaults() {
        if (_getDefaultsLoopCount > 2 && HttpContext.Current != null && HttpContext.Current.Response != null) {
            _getDefaultsLoopCount = 0;
            HelperMethods.PageRedirect("~/ErrorPages/Error.html");
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("NewUserDefaults", "", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        if (dbSelect.Count >= 1) {
            data = dbSelect[0];
            _getDefaultsLoopCount = 0;
        }

        if (data == null || data.Count == 0) {
            _getDefaultsLoopCount++;
            System.Threading.Thread.Sleep(100);
            RecreateItem(_role);
            GetDefaults();
        }
    }

    public string GetDefault(string columnName) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("NewUserDefaults", columnName, new List<DatabaseQuery>() { new DatabaseQuery("Role", _role), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public string GetDemoAppPackage {
        get {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("NewUserDefaults", "AppPackage", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }
    }

    private void RecreateItem(string role) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Role", role));
        query.Add(new DatabaseQuery("Theme", "Standard"));
        query.Add(new DatabaseQuery("ShowDateTime", "1"));
        query.Add(new DatabaseQuery("LockAppIcons", "0"));
        query.Add(new DatabaseQuery("GroupIcons", "1"));
        query.Add(new DatabaseQuery("IconCategoryCount", "1"));
        query.Add(new DatabaseQuery("ToolTips", "1"));
        query.Add(new DatabaseQuery("WorkspaceRotate", "0"));
        query.Add(new DatabaseQuery("WorkspaceRotateInterval", "10"));
        query.Add(new DatabaseQuery("WorkspaceRotateScreens", "4"));
        query.Add(new DatabaseQuery("RotateAutoRefresh", "0"));
        query.Add(new DatabaseQuery("ShowAppTitle", "1"));
        query.Add(new DatabaseQuery("AppHeaderIcon", "1"));
        query.Add(new DatabaseQuery("ClearPropOnSignOff", "0"));
        query.Add(new DatabaseQuery("AppContainer", "1"));
        query.Add(new DatabaseQuery("EnableChat", "0"));
        query.Add(new DatabaseQuery("ChatTimeout", "0"));
        query.Add(new DatabaseQuery("ChatSoundNoti", "1"));
        query.Add(new DatabaseQuery("SnapToGrid", "0"));
        query.Add(new DatabaseQuery("BackgroundImgs", ""));
        query.Add(new DatabaseQuery("EnableBackgrounds", "0"));
        query.Add(new DatabaseQuery("AppPackage", ""));
        query.Add(new DatabaseQuery("UserSignUpAdminPages", ""));
        query.Add(new DatabaseQuery("Groups", ""));
        query.Add(new DatabaseQuery("TaskBarShowAll", "1"));
        query.Add(new DatabaseQuery("ShowWorkspaceNumApp", "1"));
        query.Add(new DatabaseQuery("AnimationSpeed", "125"));
        query.Add(new DatabaseQuery("ShowMinimizedPreview", "1"));
        query.Add(new DatabaseQuery("LoadLinksBlankPage", "1"));
        query.Add(new DatabaseQuery("HoverPreviewWorkspace", "1"));
        query.Add(new DatabaseQuery("WorkspaceMode", "Complex"));
        query.Add(new DatabaseQuery("AppGridSize", "20"));
        query.Add(new DatabaseQuery("AppSnapHelper", "0"));
        query.Add(new DatabaseQuery("PluginsToInstall", ""));
        query.Add(new DatabaseQuery("UserAppStyle", ""));
        query.Add(new DatabaseQuery("DefaultLoginGroup", ""));
        query.Add(new DatabaseQuery("MobileAutoSync", "0"));
        query.Add(new DatabaseQuery("BackgroundLoopTimer", "30"));
        query.Add(new DatabaseQuery("BackgroundPosition", "right center"));
        query.Add(new DatabaseQuery("BackgroundSize", "auto"));
        query.Add(new DatabaseQuery("BackgroundRepeat", "1"));
        query.Add(new DatabaseQuery("BackgroundColor", "#F2F2F2"));
        query.Add(new DatabaseQuery("SiteTipsOnPageLoad", "1"));
        query.Add(new DatabaseQuery("AppSelectorStyle", ""));
        query.Add(new DatabaseQuery("ProfileLinkStyle", ""));
        query.Add(new DatabaseQuery("HideAllOverlays", "0"));
        query.Add(new DatabaseQuery("DefaultBodyFontFamily", ""));
        query.Add(new DatabaseQuery("DefaultBodyFontSize", ""));
        query.Add(new DatabaseQuery("DefaultBodyFontColor", ""));
        query.Add(new DatabaseQuery("ShowSiteToolsPageDescription", "0"));
        query.Add(new DatabaseQuery("ShowSiteToolsInCategories", "1"));
        query.Add(new DatabaseQuery("SiteColorOption", "1~;2~"));
        query.Add(new DatabaseQuery("SiteLayoutOption", "Fixed"));
        query.Add(new DatabaseQuery("ShowDedicatedMinimizedArea", "0"));
        query.Add(new DatabaseQuery("AllowNavMenuCollapseToggle", "0"));
        query.Add(new DatabaseQuery("ShowRowCountGridViewTable", "0"));
        query.Add(new DatabaseQuery("UseAlternateGridviewRows", "0"));
        query.Add(new DatabaseQuery("SiteToolsIconOnly", "0"));
        query.Add(new DatabaseQuery("HideSearchBarInTopBar", "0"));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("NewUserDefaults", query);
    }

    public void DeleteDefault() {
        dbCall.CallDelete("NewUserDefaults", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }); 
    }

    public void UpdateDefaults(string key, string val) {
        if ((!key.ToLower().Contains("delete")) && (!val.ToLower().Contains("delete"))
            && (!key.ToLower().Contains("create")) && (!val.ToLower().Contains("create"))) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery(key, val));
            updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
            dbCall.CallUpdate("NewUserDefaults", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("Role", _role), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        }
    }

    public Dictionary<string, string> DefaultTable {
        get { return data; }
        set { data = value; }
    }

    public Dictionary<int, string> GetBackgroundImg() {
        Dictionary<int, string> returnVal = new Dictionary<int, string>();
        if (DefaultTable != null && DefaultTable.Count > 0) {
            string temp = DefaultTable["BackgroundImgs"];
            string[] splitTemp = temp.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitTemp.Length; i++) {
                string[] splitVal = splitTemp[i].Split(new string[] { ":=:" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitVal.Length == 2) {
                    int outDb = 1;
                    if (int.TryParse(splitVal[0], out outDb)) {
                        returnVal.Add(outDb, HelperMethods.RemoveProtocolFromUrl(splitVal[1].Trim()));
                    }
                }
            }
        }

        return returnVal;
    }

    public string GetBackgroundImg(int workspace) {
        string returnVal = string.Empty;
        if (DefaultTable != null && DefaultTable.Count > 0) {
            string temp = DefaultTable["BackgroundImgs"];
            string[] splitTemp = temp.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitTemp.Length; i++) {
                string[] vals = splitTemp[i].Trim().Split(new string[] { ":=:" }, StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length == 2) {
                    if (vals[0] == workspace.ToString()) {
                        returnVal = vals[1];
                        break;
                    }
                }
            }
        }

        returnVal = HelperMethods.RemoveProtocolFromUrl(returnVal);
        return returnVal;
    }

    public void updateBackgroundImg(string url, int workspace) {
        string newImg = string.Empty;
        Dictionary<int, string> bgs = GetBackgroundImg();
        foreach (KeyValuePair<int, string> keyPair in bgs) {
            if (workspace != keyPair.Key) {
                newImg += keyPair.Key.ToString() + ":=:" + keyPair.Value + ServerSettings.StringDelimiter;
            }
        }

        if (!string.IsNullOrEmpty(url)) {
            newImg += workspace.ToString() + ":=:" + url + ServerSettings.StringDelimiter;
        }

        UpdateDefaults("BackgroundImgs", newImg);
    }

    public void updateSiteColorOption(string option) {
        string tempOption = option + ServerSettings.StringDelimiter;

        if (DefaultTable != null && DefaultTable.Count > 0 && DefaultTable.ContainsKey("SiteColorOption")) {
            string SiteColorOption = DefaultTable["SiteColorOption"];
            string currentSelected = option.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] splitOptions = SiteColorOption.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            foreach (string splitOption in splitOptions) {
                string splitValIndex = splitOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (splitValIndex != currentSelected) {
                    tempOption += splitOption + ServerSettings.StringDelimiter;
                }
            }
        }

        UpdateDefaults("SiteColorOption", tempOption);
    }

    public static string GetRoleID(string rolename) {
        Guid tempGuid = new Guid();
        if (!Guid.TryParse(rolename, out tempGuid) && rolename != "DemoNoLogin") {
            DatabaseCall _dbCall = new DatabaseCall();
            string roleTableName = "Roles";
            if (_dbCall.DataProvider == "System.Data.SqlClient") {
                roleTableName = "aspnet_Roles";
            }
            DatabaseQuery dbSelect = _dbCall.CallSelectSingle(roleTableName, "RoleId", new List<DatabaseQuery>() { new DatabaseQuery("RoleName", rolename), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            if (!string.IsNullOrEmpty(dbSelect.Value)) {
                return dbSelect.Value;
            }
        }

        return rolename;
    }
}