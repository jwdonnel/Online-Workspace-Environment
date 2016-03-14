using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public struct PageViewsToIgnore_Coll {
    private readonly string _id;
    private readonly string _username;
    private readonly string _updatedBy;
    private readonly string _date;

    public PageViewsToIgnore_Coll(string id, string username, string updatedBy, string date) {
        _id = id;
        _username = username;
        _updatedBy = updatedBy;
        _date = date;
    }

    public string ID {
        get { return _id; }
    }

    public string Username {
        get { return _username; }
    }

    public string UpdatedBy {
        get { return _updatedBy; }
    }

    public string DateAdded {
        get { return _date; }
    }
}

/// <summary>
/// Summary description for PageViewsUsersToIgnore
/// </summary>
public class PageViewsUsersToIgnore {

    #region Private Variables
    private const string TableName = "aspnet_PageViewsUsersToIgnore";
    private readonly DatabaseCall dbCall = new DatabaseCall();
    #endregion


    public PageViewsUsersToIgnore() { }

    public void AddItem(string username, string updatedBy) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Username", username.ToLower()));
        query.Add(new DatabaseQuery("UpdatedBy", updatedBy));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert(TableName, query);
    }

    public List<PageViewsToIgnore_Coll> GetListOfUsersToIgnore() {
        List<PageViewsToIgnore_Coll> coll = new List<PageViewsToIgnore_Coll>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, "DateAdded DESC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string username = row["Username"];
            string updatedBy = row["UpdatedBy"];
            string date = row["DateAdded"];
            coll.Add(new PageViewsToIgnore_Coll(id, username, updatedBy, date));
        }

        return coll;
    }

    public static bool UsernameIsIgnored(string username) {
        DatabaseCall _dbCall = new DatabaseCall();
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Username", username.ToLower()));

        List<Dictionary<string, string>> dbSelect = _dbCall.CallSelect(TableName, "", query);
        if (dbSelect.Count > 0) {
            return true;
        }

        return false;
    }

    public void UpdateItem(string id, string username, string updatedBy) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Username", username.ToLower()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", updatedBy));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void DeleteItemByID(string id) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteItemsByUsername(string username) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("Username", username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

}
