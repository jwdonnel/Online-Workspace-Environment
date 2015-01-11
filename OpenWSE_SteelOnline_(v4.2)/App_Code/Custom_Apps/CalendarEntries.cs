#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

#endregion

[Serializable]
public class CalendarEntries {
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private const string TableName = "CalendarEntries";
    private string _username;
    private List<Dictionary<string, string>> _dataTable = new List<Dictionary<string, string>>();

    public CalendarEntries(string username) {
        _username = username;
    }

    public List<Dictionary<string, string>> ce_dt {
        get { return _dataTable; }
    }

    public void addEntry(string user, string startdate, string enddate, string hours, string reason, string desc, string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Username", user.Trim()));
        query.Add(new DatabaseQuery("StartDate", startdate.Trim()));
        query.Add(new DatabaseQuery("EndDate", enddate.Trim()));
        query.Add(new DatabaseQuery("Hours", hours.Trim()));
        query.Add(new DatabaseQuery("Reason", reason.Trim()));
        query.Add(new DatabaseQuery("Description", desc.Trim()));
        query.Add(new DatabaseQuery("Enteredby", _username));
        query.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("Approved", "0"));
        query.Add(new DatabaseQuery("GroupName", group));

        dbCall.CallInsert(TableName, query);
    }

    public void addEntry(string user, string startdate, string enddate, string hours, string reason, string desc, string group, string approved) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Username", user.Trim()));
        query.Add(new DatabaseQuery("StartDate", startdate.Trim()));
        query.Add(new DatabaseQuery("EndDate", enddate.Trim()));
        query.Add(new DatabaseQuery("Hours", hours.Trim()));
        query.Add(new DatabaseQuery("Reason", reason.Trim()));
        query.Add(new DatabaseQuery("Description", desc.Trim()));
        query.Add(new DatabaseQuery("Enteredby", _username));
        query.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("Approved", approved));
        query.Add(new DatabaseQuery("GroupName", group));

        dbCall.CallInsert(TableName, query);
    }

    public void getEntries(string user, string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Username", user));
        query.Add(new DatabaseQuery("GroupName", group));

        _dataTable = dbCall.CallSelect(TableName, "", query);
    }

    public void getEntries_ByMonth(string month, string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("StartDate", month));
        query.Add(new DatabaseQuery("EndDate", month));
        query.Add(new DatabaseQuery("GroupName", group));

        _dataTable = dbCall.CallSelect(TableName, "", "StartDate LIKE @StartDate OR EndDate LIKE @EndDate AND GroupName=@GroupName", query);
    }

    public void getEntry(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        _dataTable = dbCall.CallSelect(TableName, "", query);
    }

    public void getEntries(string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("GroupName", group));

        _dataTable = dbCall.CallSelect(TableName, "", query);
    }

    public void GetPendingApprovals(string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("GroupName", group));
        query.Add(new DatabaseQuery("Approved", "0"));

        _dataTable = dbCall.CallSelect(TableName, "", query, "DateUpdated ASC");
    }

    public void deleteEntry(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete(TableName, query);
    }

    public void updateEntry(string id, string approval) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Approved", approval));
        updateQuery.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void updateceGroup(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupName", groupname));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void updateEntry(string id, string startdate, string enddate, string hours, string reason, string desc, string employee) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Username", employee));
        updateQuery.Add(new DatabaseQuery("StartDate", startdate));
        updateQuery.Add(new DatabaseQuery("EndDate", enddate));
        updateQuery.Add(new DatabaseQuery("Hours", hours));
        updateQuery.Add(new DatabaseQuery("Reason", reason));
        updateQuery.Add(new DatabaseQuery("Description", desc));
        updateQuery.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public string GetUsername(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "Username", query);
        return dbSelect.Value;
    }
}