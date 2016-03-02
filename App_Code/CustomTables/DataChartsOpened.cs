using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class DataChartsOpened_Coll {
    private string _id;
    private string _username;
    private string _appId;

    public DataChartsOpened_Coll(string id, string username, string appId) {
        _id = id;
        _username = username;
        _appId = appId;
    }

    public string ID {
        get { return _id; }
    }

    public string UserName {
        get { return _username; }
    }

    public string AppID {
        get { return _appId; }
    }
}

/// <summary>
/// Summary description for DataChartsOpened
/// </summary>
public class DataChartsOpened {

    private const string TableName = "aspnet_UserCustomTableOpenedCharts";
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private string _userName;

    public DataChartsOpened(string username) {
        _userName = username;
    }

    public void AddItem(string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _userName));
        query.Add(new DatabaseQuery("AppID", appId));
        dbCall.CallInsert(TableName, query);
    }

    public List<DataChartsOpened_Coll> GetAllUserChartsOpened() {
        List<DataChartsOpened_Coll> cInfo = new List<DataChartsOpened_Coll>();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "ID, AppID", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            DataChartsOpened_Coll item = new DataChartsOpened_Coll(row["ID"], _userName, row["AppID"]);
            cInfo.Add(item);
        }
        return cInfo;
    }

    public bool IsChartOpenedForUser(string appId) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "ID", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName), new DatabaseQuery("AppID", appId), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        if (dbSelect != null && !string.IsNullOrEmpty(dbSelect.Value)) {
            return true;
        }

        return false;
    }

    public void DeleteItem(string appID) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName), new DatabaseQuery("AppID", appID), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteItem(string appID, string username) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("AppID", appID), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteAllItemsForApp(string appID) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("AppID", appID), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteAllItemsForUser(string username) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteAllItemsForEveryone() {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

}