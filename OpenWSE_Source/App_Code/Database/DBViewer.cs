#region

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlServerCe;

#endregion

/// <summary>
///     Summary description for GeneralDirection
/// </summary>
[Serializable]
public class DBViewer {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private AppLog applog = new AppLog(false);
    private DataTable dataTable;

    public DBViewer(bool gettables) {
        if (gettables) {
            dataTable = dbCall.CallGetSchema("Tables");
        }
    }

    public DBViewer(bool gettables, ConnectionStringSettings _connString) {
        dbCall = new DatabaseCall(_connString.ProviderName, _connString.ConnectionString);
        if (gettables) {
            dataTable = dbCall.CallGetSchema("Tables");
        }
    }

    public DataTable dt {
        get { return dataTable; }
    }

    public void GetTableData(string tablename, bool recordErrors = true) {
        dataTable = new DataTable();

        dbCall.NeedToLog = recordErrors;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(tablename, "", null);
        if (dbSelect.Count >= 0) {
            dataTable = dbCall.CallGetDataTable(tablename);
        }
        else {
            int count = 0;
            foreach (Dictionary<string, string> row in dbSelect) {
                DataRow dataRow = dataTable.NewRow();
                foreach (string key in row.Keys) {
                    if (count == 0) {
                        dataTable.Columns.Add(new DataColumn(key));
                    }
                    dataRow[key] = row[key];
                }
                dataTable.Rows.Add(dataRow);
                count++;
            }
        }

        dbCall.NeedToLog = true;
    }

    public void GetCustomDataSort(string tablename, string sortCol = "", string sortDir = "") {
        dataTable = new DataTable();

        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "TimeStamp";
            if (sortDir == "undefined")
                sortDir = "DESC";
        }
        else {
            sortCol = "TimeStamp";
            sortDir = "DESC";
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(tablename, "", null, sortCol + " " + sortDir);
        if (dbSelect.Count >= 0) {
            string selectStatement = string.Format("SELECT * FROM {0} ORDER BY {1} {2}", tablename, sortCol, sortDir);
            dataTable = dbCall.CallGetDataTableBySelectStatement(selectStatement);
        }
        else {
            int count = 0;
            foreach (Dictionary<string, string> row in dbSelect) {
                DataRow dataRow = dataTable.NewRow();
                foreach (string key in row.Keys) {
                    if (count == 0) {
                        dataTable.Columns.Add(new DataColumn(key));
                    }
                    dataRow[key] = row[key];
                }
                dataTable.Rows.Add(dataRow);
                count++;
            }
        }
    }

    public List<string> GetTableDataColumn(string tablename, string columnName) {
        List<string> list = new List<string>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(tablename, columnName, null);
        foreach (Dictionary<string, string> row in dbSelect) {
            if (!list.Contains(row[columnName])) {
                list.Add(row[columnName]);
            }
        }

        return list;
    }

    public List<string> GetTableDataColumnImport(string tablename, string columnName, DBImporter_Coll dbcoll) {
        List<string> list = new List<string>();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(tablename, columnName, null);
        foreach (Dictionary<string, string> row in dbSelect) {
            if (!list.Contains(row[columnName])) {
                list.Add(row[columnName]);
            }
        }

        return list;
    }

    public void GetTableData(string tablename, string provider, string connectionstring) {
        DatabaseCall temp_dbCall = new DatabaseCall(provider, connectionstring);
        dataTable = new DataTable();
        List<Dictionary<string, string>> dbSelect = temp_dbCall.CallSelect(tablename, "", null);
        if (dbSelect.Count >= 0) {
            dataTable = dbCall.CallGetDataTable(tablename);
        }
        else {
            int count = 0;
            foreach (Dictionary<string, string> row in dbSelect) {
                DataRow dataRow = dataTable.NewRow();
                foreach (string key in row.Keys) {
                    if (count == 0) {
                        dataTable.Columns.Add(new DataColumn(key));
                    }
                    dataRow[key] = row[key];
                }
                dataTable.Rows.Add(dataRow);
                count++;
            }
        }
    }

    public void GetImportedTableData(DBImporter_Coll dbcoll) {
        DatabaseCall temp_dbCall = new DatabaseCall(dbcoll.Provider, dbcoll.ConnString);
        dataTable = new DataTable();
        List<Dictionary<string, string>> dbSelect = temp_dbCall.CallSelect(dbcoll.SelectCommand);

        if (dbSelect.Count >= 0) {
            dataTable = dbCall.CallGetDataTableBySelectStatement(dbcoll.SelectCommand);
        }
        else {
            int count = 0;
            foreach (Dictionary<string, string> row in dbSelect) {
                DataRow dataRow = dataTable.NewRow();
                foreach (string key in row.Keys) {
                    if (count == 0) {
                        dataTable.Columns.Add(new DataColumn(key));
                    }
                    dataRow[key] = row[key];
                }
                dataTable.Rows.Add(dataRow);
                count++;
            }
        }
    }
}