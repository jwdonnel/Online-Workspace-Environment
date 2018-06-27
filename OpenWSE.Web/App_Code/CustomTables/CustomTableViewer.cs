using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Script.Serialization;


[Serializable]
public class CustomTable_Coll {
    private string _id;
    private string _tableID;
    private string _tableName;
    private string _description;
    private string _appID;
    private List<CustomTableColumnData> _columnData;
    private List<TableSummaryData> _summaryData;
    private bool _notifyUsers = false;
    private List<string> _usersAllowedToEdit;
    private List<CustomTableCustomizations> _tableCustomizations;
    private string _updatedBy;
    private DateTime _dateUpdated = new DateTime();

    public CustomTable_Coll() { }
    public CustomTable_Coll(string id, string tableID, string tableName, string description, string appID, string columnData, string summaryData, string notifyUsers, string usersAllowedToEdit, string tableCustomizations, string updatedBy, string dateUpdated) {
        _id = id;
        _tableName = tableName;
        _description = description;
        _tableID = tableID;
        _appID = appID;

        JavaScriptSerializer serializer = ServerSettings.CreateJavaScriptSerializer();

        try {
            CustomTableColumnData[] columnCreater = serializer.Deserialize<CustomTableColumnData[]>(columnData);
            _columnData = columnCreater.ToList();
        }
        catch {
            _columnData = new List<CustomTableColumnData>();
        }

        try {
            TableSummaryData[] summaryCreator = serializer.Deserialize<TableSummaryData[]>(summaryData);
            _summaryData = summaryCreator.ToList();
        }
        catch {
            _summaryData = new List<TableSummaryData>();
        }

        _notifyUsers = HelperMethods.ConvertBitToBoolean(notifyUsers);
        _usersAllowedToEdit = usersAllowedToEdit.ToLower().Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();

        try {
            CustomTableCustomizations[] columnCustomizations = serializer.Deserialize<CustomTableCustomizations[]>(tableCustomizations);
            _tableCustomizations = columnCustomizations.ToList();
        }
        catch {
            _tableCustomizations = new List<CustomTableCustomizations>();
        }

        _updatedBy = updatedBy;
        DateTime.TryParse(dateUpdated, out _dateUpdated);
    }

    public string ID {
        get { return _id; }
    }

    public string TableID {
        get { return _tableID; }
    }

    public string TableName {
        get { return _tableName; }
    }

    public string Description {
        get { return _description; }
    }

    public string AppID {
        get { return _appID; }
    }

    public List<CustomTableColumnData> ColumnData {
        get { return _columnData; }
    }

    public List<TableSummaryData> SummaryData {
        get { return _summaryData; }
    }

    public bool NotifyUsers {
        get { return _notifyUsers; }
    }

    public List<string> UsersAllowedToEdit {
        get { return _usersAllowedToEdit; }
    }

    public List<CustomTableCustomizations> TableCustomizations {
        get { return _tableCustomizations; }
    }

    public string UpdatedBy {
        get { return _updatedBy; }
    }

    public DateTime DateUpdated {
        get { return _dateUpdated; }
    }
}


public class CustomTableViewer {

    #region -- Private Variables --

    private readonly AppLog _applog = new AppLog(false);
    List<CustomTable_Coll> _coll = new List<CustomTable_Coll>();
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private string _userName;
    private const string TableName = "aspnet_CustomTables";

    #endregion


    #region -- Constructor --

    public CustomTableViewer(string userName) {
        _coll.Clear();
        _userName = userName;
    }

    #endregion


    #region -- Insert/Delete/Drop Table --

    public void AddItem(string tableID, string tableName, string description, string appId, string columnData, string summaryData, string updatedBy, bool notifyUsers, string usersAllowToEdit, string tableCustomizations) {
        string _nu = "0";
        if (notifyUsers) {
            _nu = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("TableID", tableID));
        query.Add(new DatabaseQuery("TableName", tableName));
        query.Add(new DatabaseQuery("Description", description));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("ColumnData", columnData));
        query.Add(new DatabaseQuery("SummaryData", summaryData));
        query.Add(new DatabaseQuery("NotifyUsers", _nu));
        query.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowToEdit));
        query.Add(new DatabaseQuery("TableCustomizations", tableCustomizations));
        query.Add(new DatabaseQuery("UpdatedBy", updatedBy));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
        dbCall.CallInsert(TableName, query);
    }
    public void DeleteRowByID(string id, string appID) {
        string tableName = GetTableIDByID(id);
        bool tableDeleted = dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });

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

    #endregion


    #region -- Get CustomTable_Coll Object Methods --

    public CustomTable_Coll GetTableInfoByAppId(string appID) {
        CustomTable_Coll cInfo = new CustomTable_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableID = row["TableID"];
            string tableName = row["TableName"];
            string description = row["Description"];
            string columnData = row["ColumnData"];
            string summaryData = row["SummaryData"];
            string notifyUsers = row["NotifyUsers"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string tableCustomizations = row["TableCustomizations"];
            string updatedBy = row["UpdatedBy"];
            string dateUpdated = row["DateUpdated"];
            cInfo = new CustomTable_Coll(id, tableID, tableName, description, appID, columnData, summaryData, notifyUsers, usersAllowedToEdit, tableCustomizations, updatedBy, dateUpdated);
            break;
        }
        return cInfo;
    }
    public CustomTable_Coll GetTableInfoByTableId(string tableID) {
        CustomTable_Coll cInfo = new CustomTable_Coll();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("TableID", tableID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableName = row["TableName"];
            string description = row["Description"];
            string appID = row["AppID"];
            string columnData = row["ColumnData"];
            string summaryData = row["SummaryData"];
            string notifyUsers = row["NotifyUsers"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string tableCustomizations = row["TableCustomizations"];
            string updatedBy = row["UpdatedBy"];
            string dateUpdated = row["DateUpdated"];
            cInfo = new CustomTable_Coll(id, tableID, tableName, description, appID, columnData, summaryData, notifyUsers, usersAllowedToEdit, tableCustomizations, updatedBy, dateUpdated);
            break;
        }
        return cInfo;
    }

    #endregion


    #region -- Get Single Value Methods --

    public string GetAppIdByTableID(string TableID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "AppID", new List<DatabaseQuery>() { new DatabaseQuery("TableID", TableID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetTableIDByAppID(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "TableID", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetTableIDByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "TableID", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetTableNameByAppID(string AppID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "TableName", new List<DatabaseQuery>() { new DatabaseQuery("AppID", AppID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetTableNameByTableID(string tableId) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "TableName", new List<DatabaseQuery>() { new DatabaseQuery("TableID", tableId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetUpdatedByByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "UpdatedBy", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public string GetAppIDByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "AppID", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }
    public ChartType GetChartTypeFromCustomizations(List<CustomTableCustomizations> customizations) {
        ChartType returnType = ChartType.None;
        foreach (CustomTableCustomizations val in customizations) {
            if (val.customizeName == "ChartType") {
                Enum.TryParse<ChartType>(val.customizeValue, out returnType);
                break;
            }
        }

        return returnType;
    }
    public string GetChartTitleFromCustomizations(List<CustomTableCustomizations> customizations) {
        foreach (CustomTableCustomizations val in customizations) {
            if (val.customizeName == "ChartTitle") {
                return val.customizeValue;
            }
        }

        return string.Empty;
    }
    public string GetChartColumnsFromCustomizations(List<CustomTableCustomizations> customizations) {
        foreach (CustomTableCustomizations val in customizations) {
            if (val.customizeName == "ChartColumns") {
                return val.customizeValue;
            }
        }

        return string.Empty;
    }

    #endregion


    #region -- Build Entry Methods --

    public void BuildEntriesAll(string sortCol = "", string sortDir = "") {
        _coll.Clear();
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "TableName";
            if (sortDir == "undefined")
                sortDir = "ASC";
        }
        else {
            sortCol = "TableName";
            sortDir = "ASC";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, sortCol + " " + sortDir);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableID = row["TableID"];
            string tableName = row["TableName"];
            string description = row["Description"];
            string appID = row["AppID"];
            string columnData = row["ColumnData"];
            string summaryData = row["SummaryData"];
            string notifyUsers = row["NotifyUsers"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string tableCustomizations = row["TableCustomizations"];
            string updatedBy = row["UpdatedBy"];
            string dateUpdated = row["DateUpdated"];
            CustomTable_Coll coll = new CustomTable_Coll(id, tableID, tableName, description, appID, columnData, summaryData, notifyUsers, usersAllowedToEdit, tableCustomizations, updatedBy, dateUpdated);
            _coll.Add(coll);
        }
    }
    public void BuildEntriesForUser(string sortCol = "", string sortDir = "") {
        _coll.Clear();
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "TableName";
            if (sortDir == "undefined")
                sortDir = "ASC";
        }
        else {
            sortCol = "TableName";
            sortDir = "ASC";
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("UpdatedBy", _userName), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, sortCol + " " + sortDir);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tableID = row["TableID"];
            string tableName = row["TableName"];
            string description = row["Description"];
            string appID = row["AppID"];
            string columnData = row["ColumnData"];
            string summaryData = row["SummaryData"];
            string notifyUsers = row["NotifyUsers"];
            string usersAllowedToEdit = row["UsersAllowedToEdit"];
            string tableCustomizations = row["TableCustomizations"];
            string updatedBy = row["UpdatedBy"];
            string dateUpdated = row["DateUpdated"];
            CustomTable_Coll coll = new CustomTable_Coll(id, tableID, tableName, description, appID, columnData, summaryData, notifyUsers, usersAllowedToEdit, tableCustomizations, updatedBy, dateUpdated);
            _coll.Add(coll);
        }
    }

    #endregion


    #region -- Update Methods --

    public void UpdateRow(string tableID, string tableName, string description, string columnData, string summaryData, bool notifyUsers, string usersAllowToEdit, string tableCustomizations) {
        string _nu = "0";
        if (notifyUsers) {
            _nu = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableID", tableID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableName", tableName));
        updateQuery.Add(new DatabaseQuery("Description", description));
        updateQuery.Add(new DatabaseQuery("ColumnData", columnData));
        updateQuery.Add(new DatabaseQuery("SummaryData", summaryData));
        updateQuery.Add(new DatabaseQuery("NotifyUsers", _nu));
        updateQuery.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowToEdit));
        updateQuery.Add(new DatabaseQuery("TableCustomizations", tableCustomizations));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", _userName));
        updateQuery.Add(new DatabaseQuery("DateUpdated", DateTime.Now.ToString()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }
    public void UpdateRowAppId(string tableID, string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableID", tableID));

        if (!string.IsNullOrEmpty(appId)) {
            if (!appId.StartsWith("app-")) {
                appId = "app-" + appId;
            }
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppID", appId));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    #endregion


    #region -- Get CustomTable_Coll List and BuildChangeText --

    public List<CustomTable_Coll> CustomTableList {
        get { return _coll; }
    }
    public static string BuildChangeText(object[] vals) {
        string changeMade = "<table cellpadding='0' cellspacing='0'><tr>";
        int count = 0;

        foreach (object obj in vals) {
            if (obj == null) {
                continue;
            }

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
            if (obj == null) {
                continue;
            }

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

    #endregion

}