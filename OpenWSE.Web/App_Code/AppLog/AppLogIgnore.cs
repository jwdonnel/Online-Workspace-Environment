#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using OpenWSE_Tools.AutoUpdates;

#endregion

[Serializable]
public struct AppLogIgnore_Coll {
    private readonly string _dateposted;
    private readonly string _eventcomment;
    private readonly Guid _id;
    private readonly bool _refresh;
    private readonly int _timesHit;

    public AppLogIgnore_Coll(Guid id, string ec, string date, bool refresh, int timesHit) {
        _id = id;
        _eventcomment = ec;
        _dateposted = date;
        _refresh = refresh;
        _timesHit = timesHit;
    }

    public Guid EventID {
        get { return _id; }
    }

    public string EventComment {
        get { return _eventcomment; }
    }

    public string DatePosted {
        get { return _dateposted; }
    }

    public bool RefreshOnError {
        get { return _refresh; }
    }

    public int TimesHit {
        get { return _timesHit; }
    }
}

[Serializable]
public class AppLogIgnore {
    private List<string> _appColl_eventcomment = new List<string>();
    private readonly List<AppLogIgnore_Coll> _appColl = new List<AppLogIgnore_Coll>();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public AppLogIgnore(bool getvalues) {
        if (getvalues) {
            _appColl.Clear();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_WebEvents_Ignored", "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "DatePosted DESC");
            foreach (Dictionary<string, string> row in dbSelect) {
                Guid id = new Guid();
                if (row.ContainsKey("EventId")) {
                    id = Guid.Parse(row["EventId"]);
                }
                else {
                    id = Guid.Parse(row["ID"]);
                }
                string ec = HttpUtility.UrlDecode(row["EventComment"]);
                int timesHit = 0;
                if (!string.IsNullOrEmpty(row["TimesHit"]))
                    timesHit = Convert.ToInt32(row["TimesHit"]);

                bool refresh = false;
                if (!string.IsNullOrEmpty(row["RefreshOnError"]))
                    refresh = HelperMethods.ConvertBitToBoolean(row["RefreshOnError"]);

                string d = row["DatePosted"];
                var coll = new AppLogIgnore_Coll(id, ec, d, refresh, timesHit);
                updateSlots(coll);
            }
        }
    }

    public void GetEventCommentListOnly() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_WebEvents_Ignored", "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "DatePosted DESC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string ec = HttpUtility.UrlDecode(row["EventComment"]);
            _appColl_eventcomment.Add(ec);
        }
    }

    public List<AppLogIgnore_Coll> appIgnore_coll {
        get { return _appColl; }
    }

    public List<string> appIgnore_coll_EventsOnly {
        get { return _appColl_eventcomment; }
    }

    public void addItem(string eventcomment) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("EventComment", HttpUtility.UrlEncode(eventcomment)));
        query.Add(new DatabaseQuery("TimesHit", "0"));
        query.Add(new DatabaseQuery("RefreshOnError", "0"));
        query.Add(new DatabaseQuery("DatePosted", ServerSettings.ServerDateTime.ToString()));
        dbCall.CallInsert("aspnet_WebEvents_Ignored", query);
    }

    public void deleteRecord(string id) {
        dbCall.CallDelete("aspnet_WebEvents_Ignored", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void deleteRecords() {
        dbCall.CallDelete("aspnet_WebEvents_Ignored", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void UpdateRefreshOnError(string id, bool refreshOnError) {
        int _refreshOnError = 0;
        if (refreshOnError)
            _refreshOnError = 1;

        dbCall.CallUpdate("aspnet_WebEvents_Ignored", new List<DatabaseQuery>() { new DatabaseQuery("RefreshOnError", _refreshOnError.ToString()) }, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public bool GetRefreshOnError(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_WebEvents_Ignored", "RefreshOnError", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public void UpdateTimesHit(string id, int count) {
        dbCall.CallUpdate("aspnet_WebEvents_Ignored", new List<DatabaseQuery>() { new DatabaseQuery("TimesHit", count.ToString()) }, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    private void updateSlots(AppLogIgnore_Coll coll) {
        _appColl.Add(coll);
    }
}