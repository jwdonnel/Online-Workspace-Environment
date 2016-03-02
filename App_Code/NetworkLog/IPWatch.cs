#region

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Text;
using System.Net.Mail;
using System.Web.Security;
using System.Data.SqlServerCe;
using System.Collections.Generic;

#endregion

/// <summary>
///     Summary description for IPWatch
/// </summary>
[Serializable]
public class IPWatch {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public IPWatch(bool getvalues) {
        if (getvalues) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

            dataTable = dbCall.CallSelect("aspnet_IPWatchList", "", query, "Attempts DESC");
        }
    }

    public List<Dictionary<string, string>> ipwatchdt {
        get { return dataTable; }
    }

    public void addItem(string IPAddress, int attempts, bool blocked) {
        string _blocked = "0";
        if (blocked) {
            _blocked = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("IPAddress", IPAddress.Trim()));
        query.Add(new DatabaseQuery("Attempts", attempts.ToString()));
        query.Add(new DatabaseQuery("Blocked", _blocked));
        query.Add(new DatabaseQuery("LastAttempt", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("aspnet_IPWatchList", query);
    }

    public bool CheckIfBlocked(string ip) {
        bool bl = false;
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("IPAddress", ip.Trim()));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_IPWatchList", "Blocked", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            if (HelperMethods.ConvertBitToBoolean(row["Blocked"])) {
                bl = true;
                break;
            }
        }

        return bl;
    }

    public bool CheckIfExists(string ip) {
        bool bl = false;
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("IPAddress", ip.Trim()));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_IPWatchList", "ID", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            bl = true;
            break;
        }

        return bl;
    }

    public void updateAttempts(string ip, int attempts) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("IPAddress", ip.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Attempts", attempts.ToString()));
        updateQuery.Add(new DatabaseQuery("LastAttempt", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("aspnet_IPWatchList", updateQuery, query);

        ServerSettings ss = new ServerSettings();
        if (attempts >= ss.AutoBlockIPCount) {
            if (ss.AutoBlockIP)
                updateBlocked(ip, true);

            var messagebody = new StringBuilder();
            messagebody.Append("IP Address " + ip + " has attempted " + attempts + " time(s) trying to gain access to the system.");
            messagebody.Append("<br />If you recognize this IP, you may ignore this message but otherwise need to take action immediately.");
            var message = new MailMessage();
            MembershipUser user = Membership.GetUser(ServerSettings.AdminUserName);
            if (!string.IsNullOrEmpty(user.Email)) {
                message.To.Add(user.Email);
                ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>IP Watch Alert</h1>", OpenWSE.Core.Licensing.CheckLicense.SiteName + ": IP Watch Alert", messagebody.ToString());
            }
        }
    }

    public void updateBlocked(string ip, bool blocked) {
        int b = 0;
        if (blocked) {
            b = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("IPAddress", ip.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Blocked", b.ToString()));

        dbCall.CallUpdate("aspnet_IPWatchList", updateQuery, query);
    }

    public void deleteIP(string ip) {
        dbCall.CallDelete("aspnet_IPWatchList", new List<DatabaseQuery>() { new DatabaseQuery("IPAddress", ip), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }
}