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
    private bool _notifyUsers = false;
    private ChartType _chartType;
    private string _chartTitle;
    private List<string> _usersAllowedToEdit;
    private DateTime _dateCreated = new DateTime();

    public CustomTable_Coll() { }

    public CustomTable_Coll(string id, string tableName, string createdBy, string tableID, string appID, string sidebar, string notifyUsers, string chartType, string chartTitle, string usersAllowedToEdit, string dateCreated) {
        _id = id;
        _tableName = tableName;
        _createdBy = createdBy;
        _tableID = tableID;
        _appID = appID;
        if (HelperMethods.ConvertBitToBoolean(sidebar)) {
            _sideBar = true;
        }

        if (HelperMethods.ConvertBitToBoolean(notifyUsers)) {
            _notifyUsers = true;
        }

        try {
            _chartType = (ChartType)Enum.Parse(typeof(ChartType), chartType);
        }
        catch {
            _chartType = ChartType.None;
        }

        _chartTitle = chartTitle;
        _usersAllowedToEdit = usersAllowedToEdit.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
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

    public bool NotifyUsers {
        get { return _notifyUsers; }
    }

    public ChartType Chart_Type {
        get { return _chartType; }
    }

    public string ChartTitle {
        get { return _chartTitle; }
    }

    public List<string> UsersAllowedToEdit {
        get { return _usersAllowedToEdit; }
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

    public void AddItem(string tableName, string createdBy, string tableID, string appId, string dateCreated, bool sidebar, bool notifyUsers, string chartType, string chartTitle, string usersAllowToEdit) {
        string _sb = "0";
        if (sidebar)
            _sb = "1";

        string _nu = "0";
        if (notifyUsers)
            _nu = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("TableName", tableName));
        query.Add(new DatabaseQuery("CreatedBy", createdBy));
        query.Add(new DatabaseQuery("TableID", tableID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("Sidebar", _sb));
        query.Add(new DatabaseQuery("NotifyUsers", _nu));
        query.Add(new DatabaseQuery("ChartType", chartType));
        query.Add(new DatabaseQuery("ChartTitle", chartTitle));
        query.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowToEdit));
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

    public CustomTable_Coll GetTableInfoByAppId(string AppID) {
        CustomTable_Coll cInfo = new CustomTable_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("CustomTables", "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableName = row["TableName"];
            string createdBy = row["CreatedBy"];
            string tableID = row["TableID"];
            string appID = row["AppID"];
            string sidebar = row["Sidebar"];
            string notifyUsers = row["NotifyUsers"];
            string chartType = row["ChartType"];
            string chartTitle = row["ChartTitle"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string dateCreated = row["DateCreated"];
            cInfo = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, notifyUsers, chartType, chartTitle, usersAllowedToEdit, dateCreated);
            break;
        }
        return cInfo;
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
            string notifyUsers = row["NotifyUsers"];
            string chartType = row["ChartType"];
            string chartTitle = row["ChartTitle"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string dateCreated = row["DateCreated"];
            cInfo = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, notifyUsers, chartType, chartTitle, usersAllowedToEdit, dateCreated);
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
            string notifyUsers = row["NotifyUsers"];
            string chartType = row["ChartType"];
            string chartTitle = row["ChartTitle"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string dateCreated = row["DateCreated"];
            var coll = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, notifyUsers, chartType, chartTitle, usersAllowedToEdit, dateCreated);
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
            string notifyUsers = row["NotifyUsers"];
            string chartType = row["ChartType"];
            string chartTitle = row["ChartTitle"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string dateCreated = row["DateCreated"];
            var coll = new CustomTable_Coll(id, tableName, createdBy, tableID, appID, sidebar, notifyUsers, chartType, chartTitle, usersAllowedToEdit, dateCreated);
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

    public void UpdateTableNameAndChartTitle(string tableID, string tableName, string chartTitle) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("TableID", tableID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableName", tableName));
        updateQuery.Add(new DatabaseQuery("ChartTitle", chartTitle));

        dbCall.CallUpdate("CustomTables", updateQuery, query);
    }

    public void UpdateSidebarActiveAndChartTypeAndNotifyUsers(string id, bool sidebar, bool notifyUsers, string chartType) {
        string _sb = "0";
        if (sidebar)
            _sb = "1";

        string _nu = "0";
        if (notifyUsers)
            _nu = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Sidebar", _sb));
        updateQuery.Add(new DatabaseQuery("NotifyUsers", _nu));
        updateQuery.Add(new DatabaseQuery("ChartType", chartType));

        dbCall.CallUpdate("CustomTables", updateQuery, query);
    }

    public void UpdateUsersAllowedToEdit(string id, string usersAllowedToEdit) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowedToEdit));

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

    public static string BuildChangeText(object[] vals) {
        string changeMade = "<table cellpadding='0' cellspacing='0'><tr>";
        int count = 0;

        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            if (count == 0) {
                changeMade += "<td style='border-right: 1px solid #CCC; border-left: 1px solid #CCC; border-bottom: 1px solid #CCC; border-top: 1px solid #CCC; background: #EFEFEF; padding: 5px 8px;'>";
            }
            else {
                changeMade += "<td style='border-right: 1px solid #CCC; border-bottom: 1px solid #CCC; border-top: 1px solid #CCC; background: #EFEFEF; padding: 5px 8px;'>";
            }
            changeMade += objArray[0].ToString().Trim() + "</td>";
            count++;
        }

        count = 0;
        changeMade += "</tr><tr>";

        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            if (count == 0) {
                changeMade += "<td style='border-right: 1px solid #CCC; border-left: 1px solid #CCC; border-bottom: 1px solid #CCC; padding: 5px 8px;'>";
            }
            else {
                changeMade += "<td style='border-right: 1px solid #CCC; border-bottom: 1px solid #CCC; padding: 5px 8px;'>";
            }
            changeMade += objArray[1].ToString().Trim() + "</td>";
            count++;
        }

        changeMade += "</tr></table>";
        return changeMade;
    }

}