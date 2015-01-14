using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using OpenWSE_Tools.AutoUpdates;
using System.Data.SqlServerCe;


[Serializable]
public class RSSFeeds_Coll {
    private string _id;
    private string _username;
    private bool _customFeed = false;
    private string _title;
    private string _loc;
    private string _rssid;
    private DateTime _dateAdded = new DateTime();

    public RSSFeeds_Coll(string id, string username, string customFeed, string title, string loc, string rssid, string dateAdded) {
        _id = id;
        _username = username;
        if (HelperMethods.ConvertBitToBoolean(customFeed))
            _customFeed = true;

        _title = title;
        _loc = loc;
        _rssid = rssid;
        DateTime.TryParse(dateAdded, out _dateAdded);
    }

    public string ID {
        get { return _id; }
    }

    public string UserName {
        get { return _username; }
    }

    public bool IsCustomFeed {
        get { return _customFeed; }
    }

    public string Title {
        get { return _title; }
    }

    public string URL {
        get { return _loc; }
    }

    public string RSSID {
        get { return _rssid; }
    }

    public DateTime DateAdded {
        get { return _dateAdded; }
    }
}

/// <summary>
/// Summary description for RSSFeeds
/// </summary>
public class RSSFeeds {
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<RSSFeeds_Coll> _RSSFeeds_Coll = new List<RSSFeeds_Coll>();
    private string _userName;

    public RSSFeeds(string userName) {
        _userName = userName;
    }

    public void AddItem(string title, string url, string rssid, bool customFeed) {
        string _customFeed = "0";
        if (customFeed)
            _customFeed = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _userName));
        query.Add(new DatabaseQuery("CustomFeed", _customFeed));
        query.Add(new DatabaseQuery("RSSFeedTitle", title));
        query.Add(new DatabaseQuery("RSSFeedLoc", url));
        query.Add(new DatabaseQuery("RSSID", rssid));
        query.Add(new DatabaseQuery("DateAdded", DateTime.Now.ToString()));

        dbCall.CallInsert("RSSFeeds", query);
    }

    public void BuildEntriesAll() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("RSSFeeds", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName) }, "RSSFeedTitle ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string username = row["UserName"];
            string customFeed = row["CustomFeed"];
            string title = row["RSSFeedTitle"];
            string loc = row["RSSFeedLoc"];
            string rssid = row["RSSID"];
            string date = row["DateAdded"];
            var coll = new RSSFeeds_Coll(id, username, customFeed, title, loc, rssid, date);
            _RSSFeeds_Coll.Add(coll);
        }
    }

    public void DeleteRowByID(string id) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public void DeleteUserFeeds(string userName) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName) });
    }

    public void DeleteRowByRSSID(string rssid) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("RSSID", rssid), new DatabaseQuery("UserName", _userName) });
    }

    public void DeleteRowByURL(string loc) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("RSSFeedLoc", loc), new DatabaseQuery("UserName", _userName) });
    }

    public List<RSSFeeds_Coll> RSSFeedCollection {
        get { return _RSSFeeds_Coll; }
    }
}