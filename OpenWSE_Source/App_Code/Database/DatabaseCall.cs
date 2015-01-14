using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;


[Serializable]
public class DatabaseQuery {
    private string _col = string.Empty;
    private string _val = string.Empty;
    private string _dataType = string.Empty;

    public DatabaseQuery(string col, string val) {
        _col = col;
        _val = val;
    }
    public DatabaseQuery(string col, string val, string dataType) {
        _col = col;
        _val = val;
        _dataType = dataType;
    }

    public string Column {
        get { return _col; }
    }
    public string Value {
        get { return _val; }
    }
    public string DataType {
        get { return _dataType; }
    }
}

/// <summary>
/// Creates the call to a database
/// </summary>
public class DatabaseCall {

    #region -- Private Variables

    private const string QueryType = "_Query";
    private const string UpdateType = "_Update";

    private string _dataProvider = "System.Data.SqlServerCe.4.0";
    private string _connString;
    private DbConnection _connection;
    private DbCommand _command;

    #endregion


    #region -- Public Variables --

    public bool NeedToLogErrors = true;

    /// <summary>
    /// Gets the current dataprovider
    /// </summary>
    public string DataProvider {
        get { return _dataProvider; }
    }

    /// <summary>
    /// Gets the current Connection String
    /// </summary>
    public string ConnectionString {
        get { return _connString; }
    }

    #endregion


    #region -- Constructor --

    public DatabaseCall() {
        ConnectionStringSettings _connSett = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
        if (_connSett != null) {
            _dataProvider = _connSett.ProviderName;
            _connString = _connSett.ConnectionString;

            InitiateConnection();
        }
    }
    public DatabaseCall(string dataProvider, string connectionString) {
        if (!string.IsNullOrEmpty(dataProvider) && !string.IsNullOrEmpty(connectionString)) {
            _dataProvider = dataProvider;
            _connString = connectionString;

            InitiateConnection();
        }
    }
    private void InitiateConnection() {
        DbProviderFactory factory = DbProviderFactories.GetFactory(_dataProvider);
        if (factory != null) {
            _connection = factory.CreateConnection();
            _connection.ConnectionString = _connString;
        }
    }

    #endregion


    #region -- Private Query Builders --

    private string BuildQueryString(List<DatabaseQuery> query) {
        string returnStr = string.Empty;
        for (int i = 0; i < query.Count; i++) {
            DatabaseQuery dbSelect = query[i];
            returnStr += string.Format("{0}=@{1}", dbSelect.Column, dbSelect.Column.Substring(dbSelect.Column.IndexOf('.') + 1) + QueryType);
            if (i < query.Count - 1) {
                returnStr += " AND ";
            }
        }

        if (!string.IsNullOrEmpty(returnStr)) {
            returnStr = " WHERE " + returnStr;
        }


        return returnStr;
    }
    private string BuildInsertQueryString(List<DatabaseQuery> query) {
        string returnStr = string.Empty;
        for (int i = 0; i < query.Count; i++) {
            DatabaseQuery dbSelect = query[i];
            returnStr += string.Format("@{0}", dbSelect.Column.Substring(dbSelect.Column.IndexOf('.') + 1));
            if (i < query.Count - 1) {
                returnStr += ", ";
            }
        }

        return returnStr;
    }
    private string BuildUpdateItemsString(List<DatabaseQuery> updateItems) {
        string returnStr = string.Empty;
        for (int i = 0; i < updateItems.Count; i++) {
            DatabaseQuery dbSelect = updateItems[i];
            returnStr += string.Format("{0}=@{1}", dbSelect.Column, dbSelect.Column.Substring(dbSelect.Column.IndexOf('.') + 1) + UpdateType);
            if (i < updateItems.Count - 1) {
                returnStr += ", ";
            }
        }
        return returnStr;
    }
    private void AddSqlParameters(List<DatabaseQuery> parms, string type) {
        for (int i = 0; i < parms.Count; i++) {
            DatabaseQuery dbSelect = parms[i];

            string val = string.Empty;
            if (dbSelect.Value != null) {
                val = dbSelect.Value.ToString().Trim();
            }

            DbParameter parm = _command.CreateParameter();

            if (!string.IsNullOrEmpty(dbSelect.DataType)) {
                parm.DbType = GetDatabaseType(dbSelect.DataType);
            }
            else if (string.IsNullOrEmpty(val)) {
                parm.DbType = DbType.String;
            }

            parm.ParameterName = dbSelect.Column.Substring(dbSelect.Column.IndexOf('.') + 1) + type;

            if (_dataProvider == "System.Data.SqlServerCe.4.0" || _dataProvider == "System.Data.SqlClient") {
                int tempInt = 0;
                if (int.TryParse(val, out tempInt)) {
                    parm.DbType = DbType.Int32;
                    parm.Value = tempInt;
                }
                else {
                    parm.Value = val;
                }
            }
            else {
                parm.Value = val;
            }

            if (!_command.Parameters.Contains(parm)) {
                _command.Parameters.Add(parm);
            }
        }
    }
    private List<DatabaseQuery> SetNullQuery(List<DatabaseQuery> query) {
        if (query == null) {
            query = new List<DatabaseQuery>();
        }
        return query;
    }
    private bool CheckConnectionFirst() {
        try {
            if (_connection != null) {
                if ((_connection.State == ConnectionState.Open) || (_connection.State == ConnectionState.Executing) || (_connection.State == ConnectionState.Broken) || (_connection.State == ConnectionState.Connecting) || (_connection.State == ConnectionState.Fetching)) {
                    _connection.Close();
                    _connection.Dispose();

                    InitiateConnection();
                }
            }

            if (_connection != null) {
                _connection.Open();
                _command = _connection.CreateCommand();
                return true;
            }
        }
        catch { }

        return false;
    }
    private void CloseConnection() {
        if (_connection != null && _connection.State == ConnectionState.Open) {
            _connection.Close();
        }
    }
    private DbType GetDatabaseType(string type) {
        switch (type.ToLower()) {
            case "int16":
                return DbType.Int16;
            case "int32":
                return DbType.Int32;
            case "boolean":
                return DbType.Boolean;
            case "datetime":
                return DbType.DateTime;
            case "string":
            default:
                return DbType.String;
        }
    }

    #endregion


    #region -- Database Commands --

    /// <summary>
    /// Calls the database to insert a new item into a table
    /// </summary>
    /// <param name="table"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public bool CallInsert(string table, List<DatabaseQuery> query) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                if (query == null || query.Count == 0) {
                    return false;
                }
                string queryString = BuildInsertQueryString(query);

                _command.CommandText = string.Format("INSERT INTO {0} VALUES({1})", table, queryString);

                AddSqlParameters(query, string.Empty);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return success;
    }

    /// <summary>
    /// Calls the database to select a single item from a table
    /// </summary>
    /// <param name="table"></param>
    /// <param name="selectParms"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public DatabaseQuery CallSelectSingle(string table, string selectParms, List<DatabaseQuery> query) {
        DatabaseQuery returnList = new DatabaseQuery("", "");
        try {
            if (CheckConnectionFirst()) {
                query = SetNullQuery(query);
                if (string.IsNullOrEmpty(selectParms)) {
                    return new DatabaseQuery("", "");
                }
                string queryString = BuildQueryString(query);

                _command.CommandText = string.Format("SELECT {0} FROM {1}{2}", selectParms, table, queryString);

                AddSqlParameters(query, QueryType);

                DbDataReader reader = _command.ExecuteReader();
                while (reader.Read()) {
                    returnList = new DatabaseQuery(reader.GetName(0), reader[0].ToString().Trim());
                    break;
                }

                _command.Dispose();

                reader.Close();
                reader.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return returnList;
    }

    /// <summary>
    /// Calls the database to select items from a table
    /// </summary>
    /// <param name="table"></param>
    /// <param name="selectParms"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public List<Dictionary<string, string>> CallSelect(string selectCmd) {
        List<Dictionary<string, string>> returnList = new List<Dictionary<string, string>>();
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = selectCmd;

                DbDataReader reader = _command.ExecuteReader();
                while (reader.Read()) {
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (!row.ContainsKey(reader.GetName(i))) {
                            row.Add(reader.GetName(i), reader[i].ToString().Trim());
                        }
                    }

                    returnList.Add(row);
                }

                _command.Dispose();

                reader.Close();
                reader.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return returnList;
    }
    public List<Dictionary<string, string>> CallSelect(string table, string selectParms, List<DatabaseQuery> query, string orderBy = "") {
        List<Dictionary<string, string>> returnList = new List<Dictionary<string, string>>();
        try {
            if (CheckConnectionFirst()) {
                query = SetNullQuery(query);
                if (string.IsNullOrEmpty(selectParms)) {
                    selectParms = "*";
                }

                if (!string.IsNullOrEmpty(orderBy)) {
                    orderBy = " ORDER BY " + orderBy;
                }
                string queryString = BuildQueryString(query);

                if (table.IndexOf(',') > 0) {
                    string[] tableSplit = table.Split(',');
                    if (tableSplit.Length == 2 && query.Count > 0) {
                        string columnName = query[0].Column.Substring(query[0].Column.IndexOf('.') + 1);
                        table = tableSplit[0].Trim() + " INNER JOIN " + tableSplit[1].Trim() + " ON " + tableSplit[0].Trim() + "." + columnName + "=" + tableSplit[1].Trim() + "." + columnName;
                    }
                }

                _command.CommandText = string.Format("SELECT {0} FROM {1}{2}{3}", selectParms, table, queryString, orderBy);

                AddSqlParameters(query, QueryType);

                DbDataReader reader = _command.ExecuteReader();
                while (reader.Read()) {
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (!row.ContainsKey(reader.GetName(i))) {
                            row.Add(reader.GetName(i), reader[i].ToString().Trim());
                        }
                    }

                    returnList.Add(row);
                }

                _command.Dispose();

                reader.Close();
                reader.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return returnList;
    }
    public List<Dictionary<string, string>> CallSelect(string table, string selectParms, string queryString, List<DatabaseQuery> query, string orderBy = "") {
        List<Dictionary<string, string>> returnList = new List<Dictionary<string, string>>();
        try {
            if (CheckConnectionFirst()) {
                query = SetNullQuery(query);
                if (string.IsNullOrEmpty(selectParms)) {
                    selectParms = "*";
                }

                if (!string.IsNullOrEmpty(orderBy)) {
                    orderBy = " ORDER BY " + orderBy;
                }

                if (!string.IsNullOrEmpty(queryString)) {
                    queryString = " WHERE " + queryString;
                }

                _command.CommandText = string.Format("SELECT {0} FROM {1}{2}{3}", selectParms, table, queryString, orderBy);

                AddSqlParameters(query, string.Empty);

                DbDataReader reader = _command.ExecuteReader();
                while (reader.Read()) {
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (!row.ContainsKey(reader.GetName(i))) {
                            row.Add(reader.GetName(i), reader[i].ToString().Trim());
                        }
                    }

                    if (row.Count > 0) {
                        returnList.Add(row);
                    }
                }

                _command.Dispose();

                reader.Close();
                reader.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return returnList;
    }

    /// <summary>
    /// Calls the database to update a table
    /// </summary>
    /// <returns></returns>
    public bool CallUpdate(string updateCmd) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = updateCmd;
                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }
    public bool CallUpdate(string table, List<DatabaseQuery> updateItems, List<DatabaseQuery> query) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                query = SetNullQuery(query);
                string queryString = BuildQueryString(query);
                string updateItemsString = BuildUpdateItemsString(updateItems);

                _command.CommandText = string.Format("UPDATE {0} SET {1}{2}", table, updateItemsString, queryString);

                AddSqlParameters(query, QueryType);
                AddSqlParameters(updateItems, UpdateType);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }


    /// <summary>
    /// Calls the database to delete an item from a table
    /// </summary>
    /// <param name="cmdText"></param>
    public bool CallDelete(string table, List<DatabaseQuery> query) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                query = SetNullQuery(query);
                string queryString = BuildQueryString(query);

                _command.CommandText = string.Format("DELETE FROM {0}{1}", table, queryString);

                AddSqlParameters(query, QueryType);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }

    /// <summary>
    /// Calls the database to drop a table
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public bool CallDropTable(string table) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = string.Format("DROP TABLE {0}", table);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }

    /// <summary>
    /// Calls the database to create a new table
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public bool CallCreateTable(string table, string columnList) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = string.Format("CREATE TABLE {0} ({1})", table, columnList);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }

    /// <summary>
    /// Calls the database to alter an existing table
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public bool CallAlterTable(string table, string commandTxt) {
        bool success = true;
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = string.Format("ALTER TABLE {0} {1}", table, commandTxt);

                _command.ExecuteNonQuery();
                _command.Dispose();
            }
        }
        catch (Exception e) {
            success = false;
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return success;
    }

    /// <summary>
    /// Calls the database schema and returns a DataTable
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public DataTable CallGetSchema(string name) {
        DataTable dataTable = new DataTable();
        try {
            if (CheckConnectionFirst()) {
                dataTable = _connection.GetSchema(name);
                _command.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }
        return dataTable;
    }

    /// <summary>
    /// Calls the database to select a table and returns a DataTable
    /// </summary>
    /// <param name="table"></param>
    /// <param name="selectParms"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public DataTable CallGetDataTable(string table) {
        DataTable dataTable = new DataTable();
        try {
            if (CheckConnectionFirst()) {
                _command.CommandText = string.Format("SELECT * FROM {0}", table);

                DbDataReader reader = _command.ExecuteReader();
                dataTable.Load(reader);

                reader.Close();
                reader.Dispose();
            }
        }
        catch (Exception e) {
            if (NeedToLogErrors) {
                new AppLog(false).AddError(e);
            }
        }
        finally {
            CloseConnection();
        }

        return dataTable;
    }

    #endregion

}