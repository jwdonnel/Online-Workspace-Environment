#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for IPWatch
/// </summary>
[Serializable]
public class IPListener {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public IPListener(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect("IPListener", "", null, "IPAddress DESC");
        }
    }

    public bool TableEmpty {
        get {
            bool empty = true;
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("IPListener", "", new List<DatabaseQuery>() { new DatabaseQuery("Active", "1") });
            if (dbSelect.Count > 0) {
                empty = false;
            }
            return empty;
        }
    }

    public int TotalActive {
        get {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("IPListener", "", new List<DatabaseQuery>() { new DatabaseQuery("Active", "1") });
            return dbSelect.Count;
        }
    }

    public List<Dictionary<string, string>> iplistener_table {
        get { return dataTable; }
    }

    public void addItem(string IPAddress, bool active, string updatedby) {
        int _active = 0;
        if (active) {
            _active = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("IPAddress", IPAddress.Trim()));
        query.Add(new DatabaseQuery("Active", _active.ToString()));
        query.Add(new DatabaseQuery("UpdatedBy", updatedby));
        query.Add(new DatabaseQuery("Date", DateTime.Now.ToString()));

        dbCall.CallInsert("IPListener", query);
    }

    public bool CheckIfActive(string ip) {
        bool act = false;
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("IPListener", "", new List<DatabaseQuery>() { new DatabaseQuery("IPAddress", ip), new DatabaseQuery("Active", "1") });
        if (dbSelect.Count > 0) {
            act = true;
        }
        return act;
    }

    public bool CheckIfExists(string ip) {
        bool ex = false;
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("IPListener", "", new List<DatabaseQuery>() { new DatabaseQuery("IPAddress", ip) });
        if (dbSelect.Count > 0) {
            ex = true;
        }
        return ex;
    }

    public void updateActive(string ip, bool active, string user) {
        int _active = 0;
        if (active) {
            _active = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("IPAddress", ip.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Active", _active.ToString()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("Date", DateTime.Now.ToString()));

        dbCall.CallUpdate("IPListener", updateQuery, query);
    }

    public void deleteIP(string ip) {
        dbCall.CallDelete("IPListener", new List<DatabaseQuery>() { new DatabaseQuery("IPAddress", ip) });
    }
}