using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for StockViewer
/// </summary>
public class StockViewer {

    private const string TableName = "StockViewer";
    private string _username = string.Empty;
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public StockViewer(string username) {
        _username = username;
    }

    public object[] GetTickers() {
        List<object> returnObj = new List<object>();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("Username", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        if (dataTable != null && dataTable.Count > 0) {
            foreach (Dictionary<string, string> data in dataTable) {
                returnObj.Add(data["JsonObject"]);
            }
        }

        return returnObj.ToArray();
    }

    public void AddTicker(string id, string jsonStr) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", _username));
        query.Add(new DatabaseQuery("JsonObject", jsonStr));

        dbCall.CallInsert(TableName, query);
    }

    public void UpdateTicker(string id, string jsonStr) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("JsonObject", jsonStr));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void DeleteTicker(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", _username));

        dbCall.CallDelete(TableName, query);
    }

}