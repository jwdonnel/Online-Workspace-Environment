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

    public void GetCustomDataSort(string tablename, string sortCol, string sortDir, List<CustomTableColumnData> tableData, string recordstopull) {
        dataTable = new DataTable();

        #region Set Columns
        if ((sortCol != "") && (sortDir != "")) {
            if (string.IsNullOrEmpty(sortCol) || sortCol == "undefined") {
                sortCol = "TimeStamp";
            }

            if (string.IsNullOrEmpty(sortDir) || sortDir == "undefined") {
                sortDir = "DESC";
            }
        }
        else {
            sortCol = "TimeStamp";
            sortDir = "DESC";
        }

        string recordstopullStr = string.Empty;
        if (!string.IsNullOrEmpty(recordstopull) && recordstopull.ToLower() != "all") {
            recordstopullStr = string.Format("TOP({0}) ", recordstopull);
        }

        #endregion

        string selectStatement = string.Format("SELECT {0}* FROM {1}", recordstopullStr, tablename);
        dataTable = dbCall.CallGetDataTableBySelectStatement(selectStatement);

        #region Sort and update data types
        if (tableData != null && tableData.Count > 0) {
            DataTable tempTable = dataTable.Clone();

            #region Loop through Table Data and update all datatypes
            foreach (CustomTableColumnData columnData in tableData) {
                if (tempTable.Columns.Contains(columnData.realName)) {
                    try {
                        string newDataType = "System.String";
                        switch (columnData.dataType) {
                            case "Integer":
                                newDataType = "System.Int32";
                                break;

                            case "Boolean":
                                newDataType = "System.Boolean";
                                break;

                            case "DateTime":
                            case "Date":
                                newDataType = "System.DateTime";
                                break;

                            case "Money":
                            case "Decimal":
                                newDataType = "System.Decimal";
                                break;
                        }

                        if (newDataType != "System.String") {
                            tempTable.Columns[columnData.realName].MaxLength = -1;
                        }

                        tempTable.Columns[columnData.realName].DataType = Type.GetType(newDataType);
                    }
                    catch { }
                }
            }
            #endregion

            // Import the current rows into the new table
            foreach (DataRow dr in dataTable.Rows) {
                try {
                    tempTable.ImportRow(dr);
                }
                catch { }
            }

            tempTable.AcceptChanges();
            DataView tempDataView = tempTable.DefaultView;
            try {
                tempDataView.Sort = sortCol + " " + sortDir;
            }
            catch { }
            dataTable = tempDataView.ToTable();
        }
        #endregion
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
            dataTable = temp_dbCall.CallGetDataTable(tablename);
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
            dataTable = temp_dbCall.CallGetDataTableBySelectStatement(dbcoll.SelectCommand);
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