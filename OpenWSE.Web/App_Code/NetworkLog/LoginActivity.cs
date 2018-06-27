using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

[Serializable]
public struct LoginActivity_Coll {
    private string _id;
    private string _ipAddress;
    private string _nameUsed;
    private string _loginGroup;
    private bool _success;
    private ActivityType _actType;
    private string _httpReferer;
    private DateTime _dateAdded;

    public LoginActivity_Coll(string id, string ipAddress, string nameUsed, string loginGroup, string success, string actType, string httpReferer, string dateAdded) {
        _id = id;
        _ipAddress = ipAddress;
        _nameUsed = nameUsed;
        _loginGroup = loginGroup;
        _success = HelperMethods.ConvertBitToBoolean(success);
        if ((actType.ToString().ToLower() == "login") || (string.IsNullOrEmpty(actType.ToString()))) {
            _actType = ActivityType.Login;
        }
        else if (actType.ToString().ToLower() == "visited" || actType.ToString().ToLower() == "guest") {
            _actType = ActivityType.Guest;
        }
        else if (actType.ToString().ToLower() == "social") {
            _actType = ActivityType.Social;
        }
        else {
            _actType = ActivityType.Logout;
        }
        _httpReferer = httpReferer;
        DateTime.TryParse(dateAdded, out _dateAdded);
    }

    public string ID {
        get { return _id; }
    }

    public string IpAddress {
        get { return _ipAddress; }
    }

    public string UserName {
        get { return _nameUsed; }
    }

    public string LoginGroup {
        get { return _loginGroup; }
    }

    public bool IsSuccessful {
        get { return _success; }
    }

    public ActivityType ActType {
        get { return _actType; }
    }

    public string HttpReferer {
        get { return _httpReferer; }
    }

    public DateTime DateAdded {
        get { return _dateAdded; }
    }
}

public enum ActivityType {
    Login,
    Logout,
    Guest,
    Social
}


/// <summary>
/// Summary description for LoginActivity
/// </summary>
public class LoginActivity
{
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<LoginActivity_Coll> dataTable = new List<LoginActivity_Coll>();

    public LoginActivity() { }

    public List<LoginActivity_Coll> ActivityList {
        get { return dataTable; }
    }

    public void AddItem(string nameUsed, bool success, ActivityType actType) {
        ServerSettings ss = new ServerSettings();
        if (ss.RecordActivity && ss.RecordLoginActivity) {
            string tempSuccess = "0";
            if (success) {
                tempSuccess = "1";
            }

            string remoteaddress = "Unknown";
            string loginGroup = string.Empty;
            string httpReferer = string.Empty;
            try {
                string username = HttpContext.Current.User.Identity.Name;
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                    loginGroup = GroupSessions.GetUserGroupSessionName(username);
                }

                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.ServerVariables != null) {
                    NameValueCollection n = HttpContext.Current.Request.ServerVariables;
                    remoteaddress = n["REMOTE_ADDR"];
                    if (remoteaddress == "::1") {
                        remoteaddress = "127.0.0.1";
                    }

                    httpReferer = n["HTTP_REFERER"];
                    if (string.IsNullOrEmpty(httpReferer) && HttpContext.Current.Request.UrlReferrer != null) {
                        httpReferer = HttpContext.Current.Request.UrlReferrer.OriginalString;
                    }
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(httpReferer) && httpReferer.Length > 2000) {
                httpReferer = httpReferer.Substring(0, 1995) + "...";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("IPAddress", remoteaddress));
            query.Add(new DatabaseQuery("NameUsed", nameUsed));
            query.Add(new DatabaseQuery("LoginGroup", loginGroup));
            query.Add(new DatabaseQuery("Success", tempSuccess));
            query.Add(new DatabaseQuery("ActType", actType.ToString()));
            query.Add(new DatabaseQuery("HttpReferer", httpReferer));
            query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

            dbCall.CallInsert("aspnet_LoginActivity", query);
        }
    }

    public void GetActivity() {
        dataTable.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_LoginActivity", "", query, "DateAdded DESC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string ipAddress = row["IPAddress"];
            string nameUsed = row["NameUsed"];
            string loginGroup = row["LoginGroup"];
            string success = row["Success"];
            string actType = row["ActType"];
            string httpReferer = row["HttpReferer"];
            string dateAdded = row["DateAdded"];
            LoginActivity_Coll coll = new LoginActivity_Coll(id, ipAddress, nameUsed, loginGroup, success, actType, httpReferer, dateAdded);
            dataTable.Add(coll);
        }

        DeleteOldRecords();
    }

    private void DeleteOldRecords() {
        ServerSettings ss = new ServerSettings();
        if (ss.DeleteOldLoginActivity) {
            int daysToKeep = ss.LoginActivityToKeepInDays;
            DateTime now = ServerSettings.ServerDateTime;
            List<LoginActivity_Coll> tempList = new List<LoginActivity_Coll>();
            foreach (var la in dataTable) {
                TimeSpan diff = now.Subtract(la.DateAdded);
                if (diff.TotalDays <= daysToKeep) {
                    tempList.Add(la);
                }
                else {
                    DeleteItem(la.ID);
                }
            }

            dataTable = tempList;
        }
    }

    public LoginActivity_Coll GetActivity(string id) {
        LoginActivity_Coll coll = new LoginActivity_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_LoginActivity", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string ipAddress = row["IPAddress"];
            string nameUsed = row["NameUsed"];
            string loginGroup = row["LoginGroup"];
            string success = row["Success"];
            string actType = row["ActType"];
            string httpReferer = row["HttpReferer"];
            string dateAdded = row["DateAdded"];
            coll = new LoginActivity_Coll(id, ipAddress, nameUsed, loginGroup, success, actType, httpReferer, dateAdded);
            break;
        }

        return coll;
    }

    public int GetTotalIPAttempts(string ipAddress) {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_LoginActivity", "ID", new List<DatabaseQuery>() { new DatabaseQuery("IPAddress", ipAddress), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Count;
    }

    public void DeleteItem(string id) {
        dbCall.CallDelete("aspnet_LoginActivity", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void DeleteAllItems() {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        dbCall.CallDelete("aspnet_LoginActivity", query);
    }
}