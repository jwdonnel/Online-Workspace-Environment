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
///     Summary description for TwitterFeeds
/// </summary>
public class TwitterFeeds
{
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public TwitterFeeds(string username, bool getvalues)
    {
        if (getvalues)
        {
            dataTable = dbCall.CallSelect("TwitterFeeds", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "DATE ASC");
        }
    }

    public Dictionary<string, string> GetRow(string id)
    {
        List<Dictionary<string, string>> row = dbCall.CallSelect("TwitterFeeds", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "DATE ASC");
        if (row.Count > 0) {
            return row[0];
        }

        return null;
    }

    public List<Dictionary<string, string>> twitter_list
    {
        get { return dataTable; }
    }

    public void addItem(string id, string username, string title, string caption, string search, string display, string type)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Title", title.Trim()));
        query.Add(new DatabaseQuery("Caption", caption));
        query.Add(new DatabaseQuery("TwitterSearch", search));
        query.Add(new DatabaseQuery("Display", display));
        query.Add(new DatabaseQuery("Type", type));
        query.Add(new DatabaseQuery("UserName", username));
        query.Add(new DatabaseQuery("Date", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("TwitterFeeds", query);
    }

    public void UpdateItem(string id, string title, string caption, string search, string display, string type)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Title", title.Trim()));
        updateQuery.Add(new DatabaseQuery("Caption", caption));
        updateQuery.Add(new DatabaseQuery("TwitterSearch", search));
        updateQuery.Add(new DatabaseQuery("Display", display));
        updateQuery.Add(new DatabaseQuery("Type", type));

        dbCall.CallUpdate("TwitterFeeds", updateQuery, query);
    }

    public void deleteFeed(string id)
    {
        dbCall.CallDelete("TwitterFeeds", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void deleteUserFeeds(string userName) {
        dbCall.CallDelete("TwitterFeeds", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }
}