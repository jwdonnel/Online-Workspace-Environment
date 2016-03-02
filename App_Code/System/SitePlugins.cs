#region

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Data.SqlServerCe;

#endregion


[Serializable]
public struct SitePlugins_Coll {
    private readonly string _id;
    private readonly string _name;
    private readonly string _loc;
    private readonly string _desc;
    private readonly bool _enabled;
    private readonly string _initcode;
    private readonly string _associatedwith;
    private readonly string _username;
    private readonly string _date;

    public SitePlugins_Coll(string id, string name, string loc, string desc, string enabled, string initcode, string associatedwith, string username, string date) {
        _id = id;
        _name = name;
        _loc = loc;
        _desc = desc;

        _enabled = false;
        if (HelperMethods.ConvertBitToBoolean(enabled))
            _enabled = true;

        _initcode = initcode;
        _associatedwith = associatedwith;
        _username = username;
        _date = date;
    }

    public string ID {
        get { return _id; }
    }

    public string PluginName {
        get { return _name; }
    }

    public string PluginLocation {
        get { return _loc; }
    }

    public string Description {
        get { return _desc; }
    }

    public bool Enabled {
        get { return _enabled; }
    }

    public string InitCode {
        get { return _initcode; }
    }

    public string AssociatedWith {
        get { return _associatedwith; }
    }

    public string CreatedBy {
        get { return _username; }
    }

    public string Date {
        get { return _date; }
    }
}


[Serializable]
public struct UserPlugins_Coll {
    private readonly string _id;
    private readonly string _user;
    private readonly string _plugin;
    private readonly string _date;

    public UserPlugins_Coll(string id, string user, string plugin, string date) {
        _id = id;
        _user = user;
        _plugin = plugin;
        _date = date;
    }

    public string ID {
        get { return _id; }
    }

    public string UserName {
        get { return _user; }
    }

    public string PluginID {
        get { return _plugin; }
    }

    public string DateAdded {
        get { return _date; }
    }
}


/// <summary>
/// Summary description for SitePlugins
/// </summary>
public class SitePlugins {
    private string currUser = "";
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<SitePlugins_Coll> dataTable = new List<SitePlugins_Coll>();
    private List<UserPlugins_Coll> userTable = new List<UserPlugins_Coll>();

    public SitePlugins(string username) {
        currUser = username;
    }

    public void BuildSitePlugins(bool onlyEnabled) {
        dataTable.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

        if (onlyEnabled) {
            query.Add(new DatabaseQuery("Enabled", "1"));
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SitePlugins", "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string name = row["PluginName"];
            string loc = row["PluginLocation"];
            string desc = row["Description"];
            string enabled = row["Enabled"];
            string initcode = row["InitCode"];
            string aw = row["AssociatedWith"];
            string username = row["CreatedBy"];
            string date = row["DateAdded"];
            var coll = new SitePlugins_Coll(id, name, loc, desc, enabled, initcode, aw, username, date);
            dataTable.Add(coll);
        }
    }

    public void BuildSitePluginsUploadByUser() {
        dataTable.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("CreatedBy", currUser));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SitePlugins", "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string name = row["PluginName"];
            string loc = row["PluginLocation"];
            string desc = row["Description"];
            string enabled = row["Enabled"];
            string initcode = row["InitCode"];
            string aw = row["AssociatedWith"];
            string username = row["CreatedBy"];
            string date = row["DateAdded"];
            var coll = new SitePlugins_Coll(id, name, loc, desc, enabled, initcode, aw, username, date);
            dataTable.Add(coll);
        }
    }

    public void BuildSitePluginsForUser() {
        userTable.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", currUser));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserPlugins", "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string user = row["UserName"];
            string plugin = row["Plugin"];
            string date = row["DateAdded"];
            var coll = new UserPlugins_Coll(id, user, plugin, date);
            userTable.Add(coll);
        }
    }

    public List<SitePlugins_Coll> siteplugins_dt {
        get { return dataTable; }
    }

    public List<UserPlugins_Coll> userplugins_dt {
        get { return userTable; }
    }

    public string addItem(string name, string loc, string desc, bool enabled, string initCode, string associatedwith, string username) {
        string _enabled = "0";
        if (enabled)
            _enabled = "1";

        string returnVal = Guid.NewGuid().ToString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", returnVal));
        query.Add(new DatabaseQuery("PluginName", name));
        query.Add(new DatabaseQuery("PluginLocation", loc));
        query.Add(new DatabaseQuery("Description", desc));
        query.Add(new DatabaseQuery("Enabled", _enabled));
        query.Add(new DatabaseQuery("InitCode", initCode));
        query.Add(new DatabaseQuery("AssociatedWith", associatedwith));
        query.Add(new DatabaseQuery("CreatedBy", username));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("SitePlugins", query);
        return returnVal;
    }

    public void addItem(string id, string name, string loc, string desc, bool enabled, string initCode, string associatedwith, string username) {
        string _enabled = "0";
        if (enabled)
            _enabled = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("PluginName", name));
        query.Add(new DatabaseQuery("PluginLocation", loc));
        query.Add(new DatabaseQuery("Description", desc));
        query.Add(new DatabaseQuery("Enabled", _enabled));
        query.Add(new DatabaseQuery("InitCode", initCode));
        query.Add(new DatabaseQuery("AssociatedWith", associatedwith));
        query.Add(new DatabaseQuery("CreatedBy", username));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("SitePlugins", query);
    }

    public void addItemForUser(string plugin) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", currUser));
        query.Add(new DatabaseQuery("Plugin", plugin));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("aspnet_UserPlugins", query);
    }

    public SitePlugins_Coll GetPlugin(string _id) {
        SitePlugins_Coll coll = new SitePlugins_Coll();
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", _id));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SitePlugins", "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string name = row["PluginName"];
            string loc = row["PluginLocation"];
            string desc = row["Description"];
            string enabled = row["Enabled"];
            string initcode = row["InitCode"];
            string aw = row["AssociatedWith"];
            string username = row["CreatedBy"];
            string date = row["DateAdded"];
            coll = new SitePlugins_Coll(id, name, loc, desc, enabled, initcode, aw, username, date);
            break;
        }

        return coll;
    }

    public void updateEnabled(string id, bool enabled) {
        string _enabled = "0";
        if (enabled)
            _enabled = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Enabled", _enabled));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public void updatePluginName(string id, string name) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PluginName", name));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public void updatePluginLocation(string id, string loc) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PluginLocation", loc));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public void updatePluginDescription(string id, string desc) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Description", desc));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public void updatePluginInitCode(string id, string initcode) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("InitCode", initcode));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public void updatePluginAssociatedWith(string id, string aw) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AssociatedWith", aw));

        dbCall.CallUpdate("SitePlugins", updateQuery, query);
    }

    public string GetPathLocation(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("SitePlugins", "PluginLocation", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public void deleteItem(string id) {
        dbCall.CallDelete("SitePlugins", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void deleteItemForUser(string id) {
        dbCall.CallDelete("aspnet_UserPlugins", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }
}