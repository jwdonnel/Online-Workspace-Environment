#region

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Web.Configuration;
using System.Collections.Generic;

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

    public void GetTableData(string tablename, bool recordErrors = true, int rowsToSelect = -1) {
        dataTable = new DataTable();

        dbCall.NeedToLog = recordErrors;

        List<Dictionary<string, string>> dbSelect = new List<Dictionary<string,string>>();
        if (rowsToSelect > 0) {
            dbSelect = dbCall.CallSelect(string.Format("SELECT TOP({0}) * FROM {1}", rowsToSelect.ToString(), tablename));
        }
        else {
            dbSelect = dbCall.CallSelect(tablename, "", null);
        }

        if (dbSelect.Count >= 0) {
            dataTable = dbCall.CallGetDataTable(tablename, rowsToSelect);
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
                catch {
                    #region Try to manually set the data if it fails (Not ideal)
                    try {
                        DataRow newRow = tempTable.NewRow();

                        newRow[DatabaseCall.ApplicationIdString] = dr[DatabaseCall.ApplicationIdString];
                        newRow["EntryID"] = dr["EntryID"];
                        newRow["TimeStamp"] = dr["TimeStamp"];

                        foreach (CustomTableColumnData columnData in tableData) {
                            if (!string.IsNullOrEmpty(dr[columnData.realName].ToString())) {
                                newRow[columnData.realName] = dr[columnData.realName].ToString();
                            }
                            else if (string.IsNullOrEmpty(dr[columnData.realName].ToString()) && !columnData.nullable) {
                                string newValue = "-";
                                switch (columnData.dataType) {
                                    case "Integer":
                                        newValue = "0";
                                        break;

                                    case "DateTime":
                                    case "Date":
                                        newValue = DateTime.Now.ToString();
                                        break;

                                    case "Money":
                                    case "Decimal":
                                        newValue = "0.0";
                                        break;
                                }

                                newRow[columnData.realName] = newValue;
                            }
                        }

                        tempTable.Rows.Add(newRow);
                    }
                    catch { }
                    #endregion
                }
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

    public void GetTableData(string tablename, string provider, string connectionstring, int rowsToSelect = -1) {
        DatabaseCall temp_dbCall = new DatabaseCall(provider, connectionstring);
        dataTable = new DataTable();
        List<Dictionary<string, string>> dbSelect = new List<Dictionary<string,string>>();
        if (rowsToSelect > 0) {
            dbSelect = temp_dbCall.CallSelect(string.Format("SELECT TOP({0}) * FROM {1}", rowsToSelect.ToString(), tablename));
        }
        else {
            dbSelect = temp_dbCall.CallSelect(tablename, "", null);
        }

        if (dbSelect.Count >= 0) {
            dataTable = temp_dbCall.CallGetDataTable(tablename, rowsToSelect);
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

    public void GetImportedTableData(DBImporter_Coll dbcoll, int rowsToSelect = -1) {
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

        if (rowsToSelect > 0 && dataTable.Rows.Count > rowsToSelect) {
            DataTable tempTable = dataTable.Copy();
            tempTable.Rows.Clear();

            int rowCount = 0;
            foreach (DataRow r in dataTable.Rows) {
                tempTable.ImportRow(r);
                rowCount++;
                if (rowCount == rowsToSelect) {
                    break;
                }
            }

            dataTable = tempTable;
        }
    }

}