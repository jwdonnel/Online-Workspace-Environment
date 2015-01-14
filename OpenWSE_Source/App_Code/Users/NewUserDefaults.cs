using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlServerCe;

public class NewUserDefaults {
    private Dictionary<string, string> data;
    private string _role = ServerSettings.AdminUserName;
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public NewUserDefaults(string role) {
        _role = role;
    }

    public void GetDefaults() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("NewUserDefaults", "", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role) });
        if (dbSelect.Count == 1) {
            data = dbSelect[0];
        }

        if (data == null || data.Count == 0) {
            RecreateItem(_role);
            GetDefaults();
        }
    }

    public string GetDefault(string columnName) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("NewUserDefaults", columnName, new List<DatabaseQuery>() { new DatabaseQuery("Role", _role) });
        return dbSelect.Value;
    }

    public string GetDemoAppPackage {
        get {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("NewUserDefaults", "AppPackage", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role) });
            return dbSelect.Value;
        }
    }

    private void RecreateItem(string role) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Role", role));
        query.Add(new DatabaseQuery("Theme", "Standard"));
        query.Add(new DatabaseQuery("ShowDateTime", "1"));
        query.Add(new DatabaseQuery("LockAppIcons", "0"));
        query.Add(new DatabaseQuery("IconCategory", "0"));
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
        query.Add(new DatabaseQuery("ReceiveAll", "0"));
        query.Add(new DatabaseQuery("TaskBarShowAll", "1"));
        query.Add(new DatabaseQuery("HideAppIcon", "0"));
        query.Add(new DatabaseQuery("ShowWorkspaceNumApp", "1"));
        query.Add(new DatabaseQuery("AnimationSpeed", "125"));
        query.Add(new DatabaseQuery("ShowMinimizedPreview", "1"));
        query.Add(new DatabaseQuery("LoadLinksBlankPage", "1"));
        query.Add(new DatabaseQuery("AutoHideMode", "0"));
        query.Add(new DatabaseQuery("PresentationMode", "0"));
        query.Add(new DatabaseQuery("HoverPreviewWorkspace", "1"));
        query.Add(new DatabaseQuery("WorkspaceMode", "Complex"));
        query.Add(new DatabaseQuery("AppGridSize", "20"));
        query.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));

        dbCall.CallInsert("NewUserDefaults", query);
    }

    public void DeleteDefault() {
        dbCall.CallDelete("NewUserDefaults", new List<DatabaseQuery>() { new DatabaseQuery("Role", _role) }); 
    }

    public void UpdateDefaults(string key, string val) {
        if ((!key.ToLower().Contains("delete")) && (!val.ToLower().Contains("delete"))
            && (!key.ToLower().Contains("create")) && (!val.ToLower().Contains("create"))) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery(key, val));
            updateQuery.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));
            dbCall.CallUpdate("NewUserDefaults", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("Role", _role) });
        }
    }

    public Dictionary<string, string> DefaultTable {
        get { return data; }
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
                        returnVal.Add(outDb, splitVal[1].Trim());
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
}