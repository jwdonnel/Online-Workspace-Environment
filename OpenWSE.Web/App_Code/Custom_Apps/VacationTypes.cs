#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for VacationTypes
/// </summary>
[Serializable]
public class VacationTypes {
    private List<Dictionary<string, string>> dataTable;
    private const string TableName = "VacationTypes";
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public VacationTypes(bool getvalues) {
        if (getvalues) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            dataTable = dbCall.CallSelect(TableName, "", query, "PTOType ASC");
        }
    }

    public List<Dictionary<string, string>> vactypes_dt {
        get { return dataTable; }
    }

    public void addItem(string t, string group, int d) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("PTOType", t));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("GroupName", group));
        query.Add(new DatabaseQuery("Deduct", d.ToString()));

        dbCall.CallInsert(TableName, query);
    }

    public bool ce_Deduct(string reason, string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("PTOType", reason));
        query.Add(new DatabaseQuery("GroupName", group));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "Deduct", query);

        foreach (Dictionary<string, string> row in dbSelect) {
            if (HelperMethods.ConvertBitToBoolean(row["Deduct"])) {
                return true;
            }
        }

        return false;
    }

    public void updateItem(string t, string id, int d) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PTOType", t));
        updateQuery.Add(new DatabaseQuery("Deduct", d.ToString()));
        updateQuery.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void updateVacationGroup(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupName", groupname));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public bool deleteItem(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        return dbCall.CallDelete(TableName, query);
    }
}