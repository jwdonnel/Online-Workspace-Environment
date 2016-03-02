#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for AppParams
/// </summary>
public class AppParams {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public AppParams(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect("AppParams", "", new List<DatabaseQuery>() { new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Parameter ASC");
        }
    }

    public List<Dictionary<string, string>> listdt {
        get { return dataTable; }
    }

    public void addItem(string appid, string parameter, string description, string user) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("AppID", appid));
        query.Add(new DatabaseQuery("UserName", user));
        query.Add(new DatabaseQuery("Parameter", parameter.Trim()));
        query.Add(new DatabaseQuery("Description", description.Trim()));
        query.Add(new DatabaseQuery("CreatedBy", user));
        query.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("AppParams", query);
    }

    public void GetAllParameters_ForApp(string appid) {
        dataTable = dbCall.CallSelect("AppParams", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appid), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Parameter ASC");
    }

    public void updateParameter(string id, string appid, string parameter, string description, string user) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Parameter", parameter));
        updateQuery.Add(new DatabaseQuery("Description", description));
        updateQuery.Add(new DatabaseQuery("UserName", user));
        updateQuery.Add(new DatabaseQuery("DateTime", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("AppParams", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void deleteParameter(string id) {
        dbCall.CallDelete("AppParams", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public bool CheckParameter(string parameter, string username) {
        int indexof = parameter.IndexOf("!");
        string a = parameter.Substring(0, indexof);
        if (a.ToLower() == username.ToLower()) {
            return true;
        }
        else {
            return false;
        }
    }

    public string GetParameterVar(string parameter) {
        int indexof = parameter.IndexOf("!");
        string a = parameter.Substring(0, indexof);

        string temp = parameter.Replace(a + "!", "");
        indexof = temp.IndexOf("=");
        a = temp.Substring(0, indexof);

        return a.Trim();
    }

    public string GetParameterVal(string parameter) {
        int indexof = parameter.IndexOf("!");
        string a = parameter.Substring(0, indexof);

        string temp = parameter.Replace(a + "!", "");
        indexof = temp.IndexOf("=");
        a = temp.Substring(indexof + 1);

        return a.Trim();
    }
}