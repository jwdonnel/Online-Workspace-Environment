using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class RSSAlerts {

    private const string RSSFeedAlertsTableName = "aspnet_RSSFeedAlerts";
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private string _userName;

    public RSSAlerts(string userName) {
        _userName = userName.ToLower();
    }

    public void AddItem(string keyword) {
        if (!KeywordExists(keyword)) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("Username", _userName));
            query.Add(new DatabaseQuery("Keyword", keyword));
            query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

            dbCall.CallInsert(RSSFeedAlertsTableName, query);
        }
    }
    private bool KeywordExists(string keyword) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Username", _userName));
        query.Add(new DatabaseQuery("Keyword", keyword));

        List<Dictionary<string, string>> entries = dbCall.CallSelect(RSSFeedAlertsTableName, "Keyword", query);

        return entries.Count > 0;
    }

    public void UpdateItem(string id, string keyword) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        if (!string.IsNullOrEmpty(keyword)) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Keyword", keyword));

            dbCall.CallUpdate(RSSFeedAlertsTableName, updateQuery, query);
        }
        else {
            DeleteItem(id);
        }
    }

    public void DeleteItem(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete(RSSFeedAlertsTableName, query);
    }

    public Dictionary<string, string> GetKeywords() {
        Dictionary<string, string> keywords = new Dictionary<string, string>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Username", _userName));

        List<Dictionary<string, string>> entries = dbCall.CallSelect(RSSFeedAlertsTableName, "ID, Keyword", query, "DateAdded DESC");
        foreach (Dictionary<string, string> entry in entries) {
            if (!keywords.ContainsKey(entry["ID"])) {
                keywords.Add(entry["ID"], entry["Keyword"]);
            }
        }

        return keywords;
    }

    public List<string> GetKeywords_List() {
        List<string> keywords = new List<string>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Username", _userName));

        List<Dictionary<string, string>> entries = dbCall.CallSelect(RSSFeedAlertsTableName, "Keyword", query);
        foreach (Dictionary<string, string> entry in entries) {
            if (!keywords.Contains(entry["Keyword"])) {
                keywords.Add(entry["Keyword"]);
            }
        }

        return keywords;
    }
}