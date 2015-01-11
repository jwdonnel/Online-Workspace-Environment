using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using OpenWSE_Tools.GroupOrganizer;

[Serializable]
public struct GroupIPListener_Coll {
    private string _id;
    private string _groupId;
    private string _ipAddress;
    private bool _active;
    private DateTime _dateUpdated;
    private string _addedBy;

    public GroupIPListener_Coll(string id, string groupId, string ipAddress, string active, string dateUpdated, string addedBy) {
        _id = id;
        _groupId = groupId;
        _ipAddress = ipAddress;
        _active = HelperMethods.ConvertBitToBoolean(active);
        DateTime timeOut = new DateTime();
        DateTime.TryParse(dateUpdated, out timeOut);
        _dateUpdated = timeOut;
        _addedBy = addedBy;
    }

    public string ID {
        get { return _id; }
    }

    public string GroupId {
        get { return _groupId; }
    }

    public string IPAddress {
        get { return _ipAddress; }
    }

    public bool Active {
        get { return _active; }
    }

    public DateTime DateUpdated {
        get { return _dateUpdated; }
    }

    public string AddedBy {
        get { return _addedBy; }
    }
}


/// <summary>
/// Summary description for GroupIPListener
/// </summary>
[Serializable]
public class GroupIPListener {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<GroupIPListener_Coll> dataTable = new List<GroupIPListener_Coll>();

    public GroupIPListener() { }

    public List<GroupIPListener_Coll> GroupIPListenerColl {
        get { return dataTable; }
    }

    public void AddGroupIp(string groupId, string ipAddress, bool active, string username) {
        string _active = "0";
        if (active) {
            _active = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("GroupId", groupId));
        query.Add(new DatabaseQuery("IPAddress", ipAddress));
        query.Add(new DatabaseQuery("Active", _active));
        query.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("AddedBy", username));

        dbCall.CallInsert("aspnet_GroupIPListener", query);
    }

    public bool CheckIfActive(string groupId, string ipAddress) {
        bool act = false;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("GroupId", groupId));
        query.Add(new DatabaseQuery("IPAddress", ipAddress));
        query.Add(new DatabaseQuery("Active", "1"));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "ID", query);
        string id = dbSelect.Value;
        if (!string.IsNullOrEmpty(id)) {
            act = true;
        }

        return act;
    }

    public bool CheckIfExists(string groupId, string ipAddress) {
        bool act = false;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("GroupId", groupId));
        query.Add(new DatabaseQuery("IPAddress", ipAddress));

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "ID", query);
        string id = dbSelect.Value;
        if (!string.IsNullOrEmpty(id)) {
            act = true;
        }

        return act;
    }

    public void GetGroupIPs(string groupId) {
        dataTable = new List<GroupIPListener_Coll>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_GroupIPListener", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupId", groupId) }, "IPAddress ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string ipAddress = row["IPAddress"];
            string active = row["Active"];
            string dateUpdated = row["DateUpdated"];
            string addedBy = row["AddedBy"];
            GroupIPListener_Coll coll = new GroupIPListener_Coll(id, groupId, ipAddress, active, dateUpdated, addedBy);
            dataTable.Add(coll);
        }
    }

    public bool IpIsActive(string id) {
        bool isActive = false;

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "Active", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        string active = dbSelect.Value;
        if (HelperMethods.ConvertBitToBoolean(active)) {
            isActive = true;
        }

        return isActive;
    }

    public bool IpIsActive(string groupId, string ipAddress) {
        bool isActive = false;

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "Active", new List<DatabaseQuery>() { new DatabaseQuery("GroupId", groupId), new DatabaseQuery("IPAddress", ipAddress) });
        string active = dbSelect.Value;
        if (HelperMethods.ConvertBitToBoolean(active)) {
            isActive = true;
        }

        return isActive;
    }

    public string GetIPAddress(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "IPAddress", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetGroupIdFromId(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_GroupIPListener", "GroupId", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public bool HasAtLeastOneActive(string GroupId) {
        bool result = false;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_GroupIPListener", "ID", new List<DatabaseQuery>() { new DatabaseQuery("GroupId", GroupId), new DatabaseQuery("Active", "1") });
        if (dbSelect.Count > 0) {
            result = true;
        }

        return result;
    }

    public bool HasAtLeastOneActive(string GroupId, string ipAddress) {
        bool result = false;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_GroupIPListener", "IPAddress", new List<DatabaseQuery>() { new DatabaseQuery("GroupId", GroupId), new DatabaseQuery("Active", "1") });
        foreach (Dictionary<string, string> row in dbSelect) {
            if (row["IPAddress"].Trim() != ipAddress.Trim()) {
                result = true;
                break;
            }
        }

        return result;
    }

    public void UpdateRow(string id, bool active) {
        string _active = "0";
        if (active) {
            _active = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Active", _active));
        updateQuery.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));

        dbCall.CallUpdate("aspnet_GroupIPListener", updateQuery, query);
    }

    public void DeleteRow(string id) {
        dbCall.CallDelete("aspnet_GroupIPListener", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    /// <summary>
    /// Checks to make sure that if the group is listening for IP addresses and 
    /// returns whether or not your current IP is active on that list.
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public bool CheckGroupNetwork(string groupId) {
        try {
            string _ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (_ipAddress == "::1") {
                _ipAddress = "127.0.0.1";
            }

            if (HttpContext.Current.Request.Url.AbsolutePath.ToLower().Contains("sitesettings")) {
                return true;
            }

            bool isActive = false;
            bool hasOneActive = false;

            List<DatabaseQuery> query = new List<DatabaseQuery>();

            Guid tempGuid = new Guid();
            if (!Guid.TryParse(groupId, out tempGuid)) {
                return true;
            }

            query.Add(new DatabaseQuery("GroupId", groupId));
            query.Add(new DatabaseQuery("Active", "1"));

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_GroupIPListener", "IPAddress", query);
            foreach (Dictionary<string, string> row in dbSelect) {
                hasOneActive = true;
                if (row["IPAddress"].Trim() == _ipAddress.Trim()) {
                    isActive = true;
                    break;
                }
            }

            if (!isActive && hasOneActive) {
                return false;
            }
        }
        catch { }

        return true;
    }

}