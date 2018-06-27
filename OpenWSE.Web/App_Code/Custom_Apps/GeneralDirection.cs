#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for GeneralDirection
/// </summary>
[Serializable]
public class GeneralDirection {
    private readonly AppLog applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;
    private const string TableName = "GeneralDirections";

    public GeneralDirection(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect(TableName, "", null, "GeneralDirection ASC");
        }
    }

    public List<Dictionary<string, string>> generaldirection {
        get { return dataTable; }
    }

    public void addItem(string direction) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("GeneralDirection", direction));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert(TableName, query);
    }

    public void updateDirection(string direction, string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GeneralDirection", direction));
        updateQuery.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public bool deleteDirection(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        return dbCall.CallDelete(TableName, query);
    }
}