using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlServerCe;



[Serializable]
public class CustomTable_Coll {
    private string _id;
    private string _tableName;
    private string _createdBy;
    private string _tableID;
    private string _appID;
    private bool _sideBar = false;
    private DateTime _dateCreated = new DateTime();

    public CustomTable_Coll() { }

    public CustomTable_Coll(string id, string tableName, string createdBy, string tableID, string appID, string sidebar, string dateCreated) {
        _id = id;
        _tableName = tableName;
        _createdBy = createdBy;
        _tableID = tableID;
        _appID = appID;
        if (HelperMethods.ConvertBitToBoolean(sidebar))
            _sideBar = true;

        DateTime.TryParse(dateCreated, out _dateCreated);
    }

    public string ID {
        get { return _id; }
    }

    public string TableName {
        get { return _tableName; }
    }

    public string CreatedBy {
        get { return _createdBy; }
    }

    public string TableID {
        get { return _tableID; }
    }

    public string AppID {
        get { return _appID; }
    }

    public bool Sidebar {
        get { return _sideBar; }
    }

    public DateTime DateCreated {
        get { return _dateCreated; }
    }
}


public class CustomTableViewer {
    private readonly AppLog _applog = new AppLog(false);
    List<CustomTable_Coll> _coll = new List<CustomTable_Coll>();
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private string _userName;

    public CustomTableViewer(string userName) {
        _coll.Clear();
        _userName = userName;
    }

    public void AddItem(string tableName, string createdBy, string tableID, string appId, string dateCreated, bool sidebar) {
        string _sb = "0";
        if (sidebar)
            _sb = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("TableName", tableName));
        query.Add(new DatabaseQuery("CreatedBy", createdBy));
        query.Add(new DatabaseQuery("TableID", tableID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("Sidebar", _sb));
        query.Add(new DatabaseQuery("DateCreated", dateCreated));
        dbCall.CallInsert("CustomTables", query);
    }

    private string GetTableNameByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "TableName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    private string GetTableIDByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "TableID", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetCreatedByByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "CreatedBy", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetAppIDByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public string GetIDByAppID(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "ID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        return dbSelect.Value;
    }

    public string GetTableNameByAppID(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "TableName", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        return dbSelect.Value;
    }

    public CustomTable_Coll GetTableInfo(string TableID) {
        CustomTable_Coll cInfo = new CustomTable_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("CustomTables", "", new List<DatabaseQuery>() { new DatabaseQuery("TableID", TableID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableName = row["TableName"];
            string createdBy = row["CreatedBy"];
            string tableID = row["TableID"];
            string appID = row["AppID"];
            string sidebar = row["Sidebar"];
            string dateCreated = row["DateCreated"];
            cInfo = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, dateCreated);
            break;
        }
        return cInfo;
    }

    public string GetTableNameByTableID(string TableID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "TableName", new List<DatabaseQuery>() { new DatabaseQuery("TableID", TableID) });
        return dbSelect.Value;
    }

    public string GetAppIdByTableID(string TableID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "AppID", new List<DatabaseQuery>() { new DatabaseQuery("TableID", TableID) });
        return dbSelect.Value;
    }

    public string GetTableIDByAppID(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("CustomTables", "TableID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        return dbSelect.Value;
    }

    public void BuildEntriesAll(string sortCol = "", string sortDir = "") {
        _coll.Clear();
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "DateCreated";
            if (sortDir == "undefined")
                sortDir = "DESC";
        }
        else {
            sortCol = "DateCreated";
            sortDir = "DESC";
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("CustomTables", "", null, sortCol + " " + sortDir);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableName = row["TableName"];
            string createdBy = row["CreatedBy"];
            string tableID = row["TableID"];
            string appID = row["AppID"];
            string sidebar = row["Sidebar"];
            string dateCreated = row["DateCreated"];
            var coll = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, dateCreated);
            _coll.Add(coll);
        }
    }

    public void BuildEntriesForUser(string sortCol = "", string sortDir = "") {
        _coll.Clear();
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "DateCreated";
            if (sortDir == "undefined")
                sortDir = "DESC";
        }
        else {
            sortCol = "DateCreated";
            sortDir = "DESC";
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("CustomTables", "", new List<DatabaseQuery>() { new DatabaseQuery("CreatedBy", _userName) }, sortCol + " " + sortDir);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableName = row["TableName"];
            string createdBy = row["CreatedBy"];
            string tableID = row["TableID"];
            string appID = row["AppID"];
            string sidebar = row["Sidebar"];
            string dateCreated = row["DateCreated"];
            var coll = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, dateCreated);
            _coll.Add(coll);
        }
    }

    public void UpdateRow(string id, string tableName, string createdBy, string dateCreated) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableName", tableName));
        updateQuery.Add(new DatabaseQuery("CreatedBy", createdBy));
        updateQuery.Add(new DatabaseQuery("DateCreated", dateCreated));

        dbCall.CallUpdate("CustomTables", updateQuery, query);
    }

    public void UpdateTableName(string tableID, string tableName) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("TableID", tableID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableName", tableName));

        dbCall.CallUpdate("CustomTables", updateQuery, query);
    }

    public void UpdateSidebarActive(string id, bool sidebar) {
        string _sb = "0";
        if (sidebar)
            _sb = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Sidebar", _sb));

        dbCall.CallUpdate("CustomTables", updateQuery, query);
    }

    public void DeleteRowByID(string id, string appID) {
        string tableName = GetTableIDByID(id);
        bool tableDeleted = dbCall.CallDelete("CustomTables", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });

        if (tableDeleted) {
            App apps = new App(_userName);
            apps.DeleteAppComplete(appID, ServerSettings.GetServerMapLocation);
            apps.DeleteAppLocal(appID);
            DropTable(tableName);
        }
    }

    public void DropTable(string tableName) {
        dbCall.CallDropTable(tableName);
    }

    public List<CustomTable_Coll> CustomTableList {
        get { return _coll; }
    }
}