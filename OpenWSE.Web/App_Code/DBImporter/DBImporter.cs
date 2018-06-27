#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using OpenWSE.Core;
using System.Linq;
using System.Data;
using System.Web.Script.Serialization;

#endregion

[Serializable]
public class DBImporter_Coll
{
    private readonly string _tableId;
    private readonly string _cs;
    private readonly string _date;
    private readonly string _ib;
    private readonly string _id;
    private readonly string _p;
    private string _sc;
    private string _tn;
    private string _description;
    private string _allowEdit;
    private List<string> _usersAllowedToEdit;
    private bool _notifyUsers = false;
    private Dictionary<string, string> _columnOverrides;
    private List<CustomTableCustomizations> _tableCustomizations;
    private List<TableSummaryData> _summaryData;

    public DBImporter_Coll() { }
    public DBImporter_Coll(string id, string tableId, string date, string tn, string description, string cs, string sc, string p, string ib, string allowEdit, string usersAllowedToEdit, string notifyUsers, string columnOverrides, string tableCustomizations, string summaryData) {
        _id = id;
        _tableId = tableId;
        _date = date;
        _tn = tn;
        _description = description;
        _cs = cs;
        _sc = sc;
        _p = p;
        _ib = ib;
        _allowEdit = allowEdit;

        _usersAllowedToEdit = usersAllowedToEdit.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (HelperMethods.ConvertBitToBoolean(notifyUsers)) {
            _notifyUsers = true;
        }

        _columnOverrides = new Dictionary<string, string>();
        List<string> tempOverrideList = columnOverrides.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string val in tempOverrideList) {
            string[] splitVal = val.Split('=');
            if (splitVal.Length == 2 && !_columnOverrides.ContainsKey(splitVal[0])) {
                _columnOverrides.Add(splitVal[0], splitVal[1]);
            }
        }

        JavaScriptSerializer serializer = ServerSettings.CreateJavaScriptSerializer();
        try {
            CustomTableCustomizations[] columnCustomizations = serializer.Deserialize<CustomTableCustomizations[]>(tableCustomizations);
            _tableCustomizations = columnCustomizations.ToList();
        }
        catch {
            _tableCustomizations = new List<CustomTableCustomizations>();
        }

        try {
            TableSummaryData[] summaryColumns = serializer.Deserialize<TableSummaryData[]>(summaryData);
            _summaryData = summaryColumns.ToList();
        }
        catch {
            _summaryData = new List<TableSummaryData>();
        }
    }
    public DBImporter_Coll(string id, string tableId, string date, string tn, string description, string cs, string sc, string p, string ib, string allowEdit, string usersAllowedToEdit, string notifyUsers, string columnOverrides, List<CustomTableCustomizations> tableCustomizations, List<TableSummaryData> summaryData) {
        _id = id;
        _tableId = tableId;
        _date = date;
        _tn = tn;
        _description = description;
        _cs = cs;
        _sc = sc;
        _p = p;
        _ib = ib;
        _allowEdit = allowEdit;

        _usersAllowedToEdit = usersAllowedToEdit.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (HelperMethods.ConvertBitToBoolean(notifyUsers)) {
            _notifyUsers = true;
        }

        _columnOverrides = new Dictionary<string, string>();
        List<string> tempOverrideList = columnOverrides.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string val in tempOverrideList) {
            string[] splitVal = val.Split('=');
            if (splitVal.Length == 2 && !_columnOverrides.ContainsKey(splitVal[0])) {
                _columnOverrides.Add(splitVal[0], splitVal[1]);
            }
        }

        _tableCustomizations = tableCustomizations;
        _summaryData = summaryData;
    }

    public string ID
    {
        get { return _id; }
    }

    public string TableID {
        get { return _tableId; }
    }

    public string Date
    {
        get { return _date; }
    }

    public string TableName
    {
        get { return _tn; }
    }

    public string Description {
        get { return _description; }
    }

    public string ConnString
    {
        get { return _cs; }
    }

    public string SelectCommand
    {
        get { return _sc; }
    }

    public string Provider
    {
        get { return _p; }
    }

    public string ImportedBy
    {
        get { return _ib; }
    }

    public bool AllowEdit
    {
        get 
        {
            return HelperMethods.ConvertBitToBoolean(_allowEdit); 
        }
    }

    public List<string> UsersAllowedToEdit {
        get { return _usersAllowedToEdit; }
    }

    public bool NotifyUsers {
        get { return _notifyUsers; }
    }

    public Dictionary<string, string> ColumnOverrides {
        get {
            try {
                string co = string.Empty;
                foreach (KeyValuePair<string, string> keyPair in _columnOverrides) {
                    co += keyPair.Key + "=" + keyPair.Value + ServerSettings.StringDelimiter;
                }

                DBViewer _dbviewer = new DBViewer(false);
                DBImporter_Coll _tempColl = new DBImporter_Coll(_id, _tableId, _date, _tn, _description, _cs, _sc, _p, _ib, _allowEdit, String.Join(ServerSettings.StringDelimiter, _usersAllowedToEdit), _notifyUsers.ToString(), co, _tableCustomizations, _summaryData);
                _dbviewer.GetImportedTableData(_tempColl);
                foreach (DataColumn column in _dbviewer.dt.Columns) {
                    if (!_columnOverrides.ContainsKey(column.ColumnName.Trim())) {
                        _columnOverrides.Add(column.ColumnName, column.ColumnName);
                    }
                }
            }
            catch { }
            return _columnOverrides;
        }
    }

    public List<CustomTableCustomizations> TableCustomizations {
        get { return _tableCustomizations; }
    }

    public List<TableSummaryData> SummaryData {
        get { return _summaryData; }
    }
}

[Serializable]
public class SavedConnections {
    private string _connectionstring;
    private string _date;
    private string _dbprovider;
    private string _id;
    private string _name;
    private string _username;

    public SavedConnections() { }
    public SavedConnections(string id, string connectionstring, string name, string dbprovider, string date, string username) {
        _id = id;
        _connectionstring = connectionstring;
        _name = name;
        _dbprovider = dbprovider;
        _date = date;
        _username = username;
    }

    public string ID {
        get { return _id; }
        set { _id = value; }
    }

    public string ConnectionString {
        get { return _connectionstring; }
        set { _connectionstring = value; }
    }

    public string ConnectionName {
        get { return _name; }
        set { _name = value; }
    }

    public string DatabaseProvider {
        get { return _dbprovider; }
        set { _dbprovider = value; }
    }

    public string Date {
        get { return _date; }
        set { _date = value; }
    }

    public string Username {
        get { return _username; }
        set { _username = value; }
    }
}

[Serializable]
public class DBImporter {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<DBImporter_Coll> _coll = new List<DBImporter_Coll>();
    private List<SavedConnections> _coll_SavedConnections = new List<SavedConnections>();
    private const string ImportTableName = "aspnet_TableImports";
    private const string SavedConnectionTableName = "aspnet_SavedConnections";
    public const string DefaultDatabaseIcon = "database.png";

    public DBImporter() { }

    public List<DBImporter_Coll> DBColl {
        get { return _coll; }
    }

    public List<SavedConnections> SavedConnections_Coll {
        get { return _coll_SavedConnections; }
    }

    public void AddConnection(string connectionString, string connectionName, string dbProvider, string username) {
        string id = Guid.NewGuid().ToString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("ConnectionString", StringEncryption.Encrypt(connectionString, "@" + id.Replace("-", "").Substring(0, 15))));
        query.Add(new DatabaseQuery("ConnectionName", connectionName));
        query.Add(new DatabaseQuery("DatabaseProvider", dbProvider));
        query.Add(new DatabaseQuery("Date", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("Username", username));

        dbCall.CallInsert(SavedConnectionTableName, query);
    }

    public void UpdateConnection(string id, string connectionString, string connectionName, string dbProvider) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ConnectionString", StringEncryption.Encrypt(connectionString, "@" + id.Replace("-", "").Substring(0, 15))));
        updateQuery.Add(new DatabaseQuery("ConnectionName", connectionName));
        updateQuery.Add(new DatabaseQuery("DatabaseProvider", dbProvider));

        dbCall.CallUpdate(SavedConnectionTableName, updateQuery, query);
    }

    public void DeleteConnection(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete(SavedConnectionTableName, query);
    }

    public void GetSavedConnectionList() {
        _coll_SavedConnections = new List<SavedConnections>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(SavedConnectionTableName, "", query);

        foreach (Dictionary<string, string> entry in dbSelect) {
            string id = entry["ID"];
            string connectionString = StringEncryption.Decrypt(entry["ConnectionString"], "@" + id.Replace("-", "").Substring(0, 15));
            string connectionName = entry["ConnectionName"];
            string databaseProvider = entry["DatabaseProvider"];
            string date = entry["Date"];
            string username = entry["Username"];

            SavedConnections coll = new SavedConnections(id, connectionString, connectionName, databaseProvider, date, username);
            _coll_SavedConnections.Add(coll);
        }
    }

    public SavedConnections GetSavedConnectionListByID(string id) {
        SavedConnections coll = new SavedConnections();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(SavedConnectionTableName, "", query);

        foreach (Dictionary<string, string> entry in dbSelect) {
            string connectionString = StringEncryption.Decrypt(entry["ConnectionString"], "@" + id.Replace("-", "").Substring(0, 15));
            string connectionName = entry["ConnectionName"];
            string databaseProvider = entry["DatabaseProvider"];
            string date = entry["Date"];
            string username = entry["Username"];

            coll = new SavedConnections(id, connectionString, connectionName, databaseProvider, date, username);
            break;
        }

        return coll;
    }

    public void AddImport(string tableId, string tn, string desc, string cs, string sc, string p, string ib, bool allowEdit, string usersAllowedToEdit, bool notifyUsers, string columnOverrides, string tableCustomizations, string summaryData) {
        if (!string.IsNullOrEmpty(tn)) {

            string _allowEdit = "0";
            if (allowEdit) {
                _allowEdit = "1";
            }

            string _notifyUsers = "0";
            if (notifyUsers) {
                _notifyUsers = "1";
            }

            string id = Guid.NewGuid().ToString();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));
            query.Add(new DatabaseQuery("TableID", tableId));
            query.Add(new DatabaseQuery("Date", ServerSettings.ServerDateTime.ToString()));
            query.Add(new DatabaseQuery("TableName", tn));
            query.Add(new DatabaseQuery("Description", desc));
            query.Add(new DatabaseQuery("ConnString", StringEncryption.Encrypt(cs, "@" + id.Replace("-", "").Substring(0, 15))));
            query.Add(new DatabaseQuery("SelectCommand", sc));
            query.Add(new DatabaseQuery("Provider", p));
            query.Add(new DatabaseQuery("ImportedBy", ib));
            query.Add(new DatabaseQuery("AllowEdit", _allowEdit));
            query.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowedToEdit));
            query.Add(new DatabaseQuery("NotifyUsers", _notifyUsers));
            query.Add(new DatabaseQuery("ColumnOverrides", columnOverrides));
            query.Add(new DatabaseQuery("TableCustomizations", tableCustomizations));
            query.Add(new DatabaseQuery("SummaryData", summaryData));

            dbCall.CallInsert(ImportTableName, query);
        }
    }

    public void GetImportList() {
        _coll = new List<DBImporter_Coll>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(ImportTableName, "", query);

        foreach (Dictionary<string, string> entry in dbSelect) {
            string id = entry["ID"];
            string tableID = entry["TableID"];
            string date = entry["Date"];
            string tableName = entry["TableName"];
            string description = entry["Description"];
            string connString = entry["ConnString"];
            string selectCommand = entry["SelectCommand"];
            string provider = entry["Provider"];
            string importedBy = entry["ImportedBy"];
            string allowEdit = entry["AllowEdit"];
            string usersAllowedToEdit = entry["UsersAllowedToEdit"];
            string notifyUsers = entry["NotifyUsers"];
            string columnOverrides = entry["ColumnOverrides"];
            string tableCustomizations = entry["TableCustomizations"];
            string summaryData = entry["SummaryData"];

            DBImporter_Coll coll = new DBImporter_Coll(id, tableID, date, tableName, description, connString, selectCommand, provider, importedBy, allowEdit, usersAllowedToEdit, notifyUsers, columnOverrides, tableCustomizations, summaryData);
            _coll.Add(coll);
        }
    }

    public DBImporter_Coll GetImportTableByTableId(string tableId) {
        DBImporter_Coll coll = new DBImporter_Coll();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(ImportTableName, "", query);

        foreach (Dictionary<string, string> entry in dbSelect) {
            string id = entry["ID"];
            string tableName = entry["TableName"];
            string description = entry["Description"];
            string date = entry["Date"];
            string connString = StringEncryption.Decrypt(entry["ConnString"], "@" + id.Replace("-", "").Substring(0, 15));
            string selectCommand = entry["SelectCommand"];
            string provider = entry["Provider"];
            string importedBy = entry["ImportedBy"];
            string allowEdit = entry["AllowEdit"];
            string usersAllowedToEdit = entry["UsersAllowedToEdit"];
            string notifyUsers = entry["NotifyUsers"];
            string columnOverrides = entry["ColumnOverrides"];
            string tableCustomizations = entry["TableCustomizations"];
            string summaryData = entry["SummaryData"];

            coll = new DBImporter_Coll(id, tableId, date, tableName, description, connString, selectCommand, provider, importedBy, allowEdit, usersAllowedToEdit, notifyUsers, columnOverrides, tableCustomizations, summaryData);
            break;
        }

        return coll;
    }

    public void DeleteEntry(string tableId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        dbCall.CallDelete(ImportTableName, query);
    }

    public void UpdateEntry(string tableId, string tablename, string description, string selectcommand, bool allowEdit, bool notifyUsers) {
        string _allowEdit = "0";
        if (allowEdit) {
            _allowEdit = "1";
        }

        string _notifyUsers = "0";
        if (notifyUsers) {
            _notifyUsers = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableName", tablename));
        updateQuery.Add(new DatabaseQuery("Description", description));
        updateQuery.Add(new DatabaseQuery("SelectCommand", selectcommand));
        updateQuery.Add(new DatabaseQuery("AllowEdit", _allowEdit));
        updateQuery.Add(new DatabaseQuery("NotifyUsers", _notifyUsers));

        dbCall.CallUpdate(ImportTableName, updateQuery, query);
    }

    public void UpdateUsersAllowedToEdit(string tableId, string usersAllowed) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UsersAllowedToEdit", usersAllowed));

        dbCall.CallUpdate(ImportTableName, updateQuery, query);
    }

    public void UpdateColumnOverrides(string tableId, string columnOverrides) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ColumnOverrides", columnOverrides));

        dbCall.CallUpdate(ImportTableName, updateQuery, query);
    }

    public void UpdateTableCustomizations(string tableId, string tableCustomizations) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TableCustomizations", tableCustomizations));

        dbCall.CallUpdate(ImportTableName, updateQuery, query);
    }

    public void UpdateSummaryData(string tableId, string summaryData) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("TableId", tableId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("SummaryData", summaryData));

        dbCall.CallUpdate(ImportTableName, updateQuery, query);
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

    public static string HideUsernameAndPasswordForConnectionString(string connectionString) {
        string ds = string.Empty;

        // Change Username
        string connectionstring = connectionString;
        int dsIndex1 = connectionstring.ToLower().IndexOf("user id=", StringComparison.Ordinal);
        int dsIndex2 = connectionstring.ToLower().IndexOf("uid=", StringComparison.Ordinal);
        if (dsIndex1 != -1) {
            string tempDs = connectionString.Substring((dsIndex1 + ("User Id=").Length));
            string un = string.Empty;
            foreach (char c in tempDs) {
                if (c != ServerSettings.StringDelimiter[0])
                    un += c.ToString();
                else
                    break;
            }

            ds = connectionString.Replace(un, "*********");
        }
        else if (dsIndex2 != -1) {
            string tempDs = connectionString.Substring((dsIndex2 + ("Uid=").Length));
            string un = string.Empty;
            foreach (char c in tempDs) {
                if (c != ServerSettings.StringDelimiter[0])
                    un += c.ToString();
                else
                    break;
            }

            ds = connectionString.Replace(un, "*********");
        }
        else {
            ds = connectionString;
        }

        // Change Password
        connectionstring = ds;
        dsIndex1 = connectionstring.ToLower().IndexOf("password=", StringComparison.Ordinal);
        dsIndex2 = connectionstring.ToLower().IndexOf("pwd=", StringComparison.Ordinal);
        if (dsIndex1 != -1) {
            string tempDs = connectionstring.Substring((dsIndex1 + ("Password=").Length));
            string password = string.Empty;
            foreach (char c in tempDs) {
                if (c != ServerSettings.StringDelimiter[0])
                    password += c.ToString();
                else
                    break;
            }

            ds = ds.Replace(password, "*********");
        }
        else if (dsIndex2 != -1) {
            string tempDs = connectionstring.Substring((dsIndex2 + ("Pwd=").Length));
            string password = string.Empty;
            foreach (char c in tempDs) {
                if (c != ServerSettings.StringDelimiter[0])
                    password += c.ToString();
                else
                    break;
            }

            ds = ds.Replace(password, "*********");
        }

        return ds;
    }

}
