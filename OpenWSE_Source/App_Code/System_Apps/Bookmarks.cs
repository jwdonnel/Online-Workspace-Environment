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
public class Bookmarks {
    private readonly string _username;
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public Bookmarks(string username) {
        _username = username;
    }

    public List<Dictionary<string, string>> bookmarks_dt {
        get { return dataTable; }
    }

    public void GetBookmarks(string sortBy) {
        dataTable = dbCall.CallSelect("aspnet_UserBookmarks", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username) }, sortBy);
    }

    public string GetHTML_byID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_UserBookmarks", "BookmarkHTML", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetHTMLName_byID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_UserBookmarks", "BookmarkName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetUsername_byID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_UserBookmarks", "UserName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public void AddItem(string BookmarkName, string BookmarkHTML) {
        if (!CheckIfExists(BookmarkName, BookmarkHTML)) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("BookmarkName", BookmarkName));
            query.Add(new DatabaseQuery("BookmarkHTML", BookmarkHTML));
            query.Add(new DatabaseQuery("DateAdded", DateTime.Now.ToString()));
            query.Add(new DatabaseQuery("UserName", _username));

            dbCall.CallInsert("aspnet_UserBookMarks", query);
        }
    }

    public bool CheckIfExists(string BookmarkName, string BookmarkHTML) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("BookmarkName", BookmarkName));
        query.Add(new DatabaseQuery("BookmarkHTML", BookmarkHTML));
        query.Add(new DatabaseQuery("UserName", _username));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_UserBookmarks", "ID", query);
        if (string.IsNullOrEmpty(dbSelect.Value)) {
            return false;
        }

        return true;
    }

    public void deleteBookmark_byID(string id) {
        dbCall.CallDelete("aspnet_UserBookMarks", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public void deleteBookmarks(string userName) {
        dbCall.CallDelete("aspnet_UserBookMarks", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName) });
    }

    public void UpdateBookmark(string id, string name, string html) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BookmarkName", name.Trim()));
        updateQuery.Add(new DatabaseQuery("BookmarkHTML", html.Trim()));

        dbCall.CallUpdate("aspnet_UserBookmarks", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }
}