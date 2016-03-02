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
///     Summary description for Bookmarks
/// </summary>
public class UserCalendar
{
    private readonly string _username;
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public UserCalendar(string username)
    {
        _username = username;
    }

    public List<Dictionary<string, string>> calendar_dt
    {
        get { return dataTable; }
    }

    public void GetEvents()
    {
        dataTable = dbCall.CallSelect("aspnet_UserCalendar", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "StartDate DESC");
    }

    public void GetEvent(string id)
    {
        dataTable = dbCall.CallSelect("aspnet_UserCalendar", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void AddEvent(string startdate, string enddate, string desc, string title, string color)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _username.ToLower()));
        query.Add(new DatabaseQuery("StartDate", startdate.Trim()));
        query.Add(new DatabaseQuery("EndDate", enddate.Trim()));
        query.Add(new DatabaseQuery("Description", desc.Trim()));
        query.Add(new DatabaseQuery("Title", title.Trim()));
        query.Add(new DatabaseQuery("ColorCode", color.Trim()));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("aspnet_UserCalendar", query);
    }

    public void DeleteEvent(string id)
    {
        dbCall.CallDelete("aspnet_UserCalendar", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteEvents(string userName) {
        dbCall.CallDelete("aspnet_UserCalendar", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void UpdateEvent(string id, string startdate, string enddate, string desc, string title, string color)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("StartDate", startdate.Trim()));
        updateQuery.Add(new DatabaseQuery("EndDate", enddate.Trim()));
        updateQuery.Add(new DatabaseQuery("ColorCode", color.Trim()));
        updateQuery.Add(new DatabaseQuery("Description", desc.Trim()));
        updateQuery.Add(new DatabaseQuery("Title", title.Trim()));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("aspnet_UserCalendar", updateQuery, query);
    }
}