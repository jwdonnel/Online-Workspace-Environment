#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Configuration;
using OpenWSE.Core;
using OpenWSE.Core.Licensing;

#endregion

/// <summary>
///     Summary description for ServerBackup
/// </summary>
public class ServerBackup {

    #region Private Variables

    private string loc = string.Empty;
    private List<Dictionary<string, DataTable>> _coll = new List<Dictionary<string,DataTable>>();
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private string username;
    private string _currUserId;

    #endregion

    public List<Dictionary<string, DataTable>> DataColl {
        get {
            return _coll;
        }
    }

    public ServerBackup(string username, string sobLocation) {
        this.username = username;
        loc = sobLocation;
        _coll = new List<Dictionary<string, DataTable>>();
        dbCall.NeedToLogErrors = false;
    }

    public void BinarySerialize(DataTable dt) {
        try {
            string encryptloc = loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt);
            BuildArrayList(dt);
            DataEncryption.SerializeFile(_coll, loc);
            DataEncryption.EncryptFile(loc, encryptloc);
        }
        catch {
        }
    }

    public void BinarySerialize_Ind(DataTable dt, string tablename) {
        try {
            string encryptloc = loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt);
            BuildArrayList_Ind(dt, tablename);
            DataEncryption.SerializeFile(_coll, loc);
            DataEncryption.EncryptFile(loc, encryptloc);
        }
        catch {
        }
    }

    public void BinarySerialize_Current(DataTable dt) {
        try {
            string encryptloc = loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt);
            BuildArrayList_Current(dt);
            DataEncryption.SerializeFile(_coll, loc);
            DataEncryption.EncryptFile(loc, encryptloc);
        }
        catch {
        }
    }

    private void BuildArrayList(DataTable dt) {
        var tempviewer = new DBViewer(false);
        var list = new Dictionary<string, DataTable>();
        foreach (DataRow dr in dt.Rows) {
            try {
                if (!dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet")) {
                    tempviewer.GetTableData(dr["TABLE_NAME"].ToString());
                    list.Add(dr["TABLE_NAME"].ToString(), tempviewer.dt);
                }
            }
            catch {
            }
        }
        _coll.Add(list);
    }

    private void BuildArrayList_Ind(DataTable dt, string tablename) {
        var list = new Dictionary<string, DataTable>();
        try {
            if (!tablename.ToLower().Contains("aspnet")) {
                list.Add(tablename, dt);
            }
        }
        catch {
        }
        _coll.Add(list);
    }

    private void BuildArrayList_Current(DataTable dt) {
        var tempviewer = new DBViewer(false);
        var list = new Dictionary<string, DataTable>();
        foreach (DataRow dr in dt.Rows) {
            tempviewer.GetTableData(dr["TABLE_NAME"].ToString());
            list.Add(dr["TABLE_NAME"].ToString(), tempviewer.dt);
        }
        _coll.Add(list);
    }

    private DataTable GetLocalTable(string table) {
        var tempviewer = new DBViewer(false);
        tempviewer.GetTableData(table);
        return tempviewer.dt;
    }

    private void BinaryDeserialize() {
        try {
            if (File.Exists(loc)) {
                MemoryStream a = DataEncryption.DecryptFile(loc);
                using (var str = new BinaryReader(a)) {
                    var bf = new BinaryFormatter();
                    str.BaseStream.Position = 0;
                    _coll = (List<Dictionary<string, DataTable>>)bf.Deserialize(str.BaseStream);
                    a.Close();
                    str.Close();
                }
            }
        }
        catch {
        }
    }

    public List<string> GetRestoreTableList() {
        List<string> tables = new List<string>();
        BinaryDeserialize();
        foreach (var d in _coll) {
            foreach (string key in d.Keys) {
                if (!tables.Contains(key)) {
                    tables.Add(key);
                }
            }
        }

        return tables;
    }

    public void RestoreBackup(List<string> tableList) {
        MemberDatabase m = new MemberDatabase(username);
        _currUserId = m.UserId;
        BinaryDeserialize();
        foreach (var d in _coll) {
            foreach (string key in d.Keys) {
                string table = key;

                bool tableInList = false;
                foreach (string tabletorestore in tableList) {
                    if (table == tabletorestore) {
                        tableInList = true;
                        break;
                    }
                }

                if (!tableInList) {
                    continue;
                }

                DataTable t = d[table];
                DataTable local = GetLocalTable(table);

                if (local == null) {
                    #region Rebuild Custom Table
                    CustomTableCreator ctc = new CustomTableCreator();
                    string columnList = ctc.CreateColumnList(ctc.BuildOriginalColumns(t));
                    if (ctc.CreateCustomTable(table, columnList)) {
                        List<string> newcolumns = ctc.BuildColumnList(t);

                        foreach (DataRow dr in t.Rows) {
                            ctc.AddEntry(table, dr, newcolumns);
                        }
                    }

                    if ((table.Contains("CT_")) && (table.IndexOf("CT_") == 0)) {
                        CustomTableAppCreator ctwc = new CustomTableAppCreator(username);

                        string categoryId = Guid.NewGuid().ToString();
                        bool categoryFound = false;
                        AppCategory category = new AppCategory(true);
                        foreach (Dictionary<string, string> Categorydr in category.category_dt) {
                            if (Categorydr["Category"].ToLower() == "custom tables") {
                                categoryId = Categorydr["ID"];
                                categoryFound = true;
                                break;
                            }
                        }
                        if (!categoryFound)
                            category.addItem("Custom Tables", categoryId);

                        string fileName = ctwc.CreateApp(true, table.Replace("CT_", "").Replace("_", " "), categoryId, "", "database.png", true, false);
                        CustomTableViewer ctv = new CustomTableViewer(username);
                        ctv.AddItem(table.Replace("CT_", ""), username, table, "app-" + fileName, DateTime.Now.ToString(), true);
                    }
                    #endregion
                }
                else {
                    // Delete current table entries before re-inserting them
                    deleteEntries(key);

                    // Re-insert data into table
                    foreach (DataRow dr in t.Rows) {
                        List<DatabaseQuery> query = BuildQuery(dr, local.Columns);
                        AddEntry(table, query);
                    }
                }
            }
        }

        // Reinitialize all static tables
        var chat = new Chat(true);
    }

    private List<DatabaseQuery> BuildQuery(DataRow dr, DataColumnCollection columns) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();

        foreach (DataColumn column in columns) {
            string value = string.Empty;
            if ((column != null) && (column.DataType.Name.ToLower() == "guid") && ((!dr.Table.Columns.Contains(column.ColumnName)) || (string.IsNullOrEmpty(dr[column.ColumnName].ToString())))) {
                if (column.ColumnName.ToLower() == "applicationid") {
                    DatabaseQuery dbSelect = dbCall.CallSelectSingle("Applications", "ApplicationId", null);
                    if (string.IsNullOrEmpty(dbSelect.Value)) {
                        value = Guid.NewGuid().ToString();
                    }
                    else {
                        value = dbSelect.Value;
                    }
                }
                else {
                    value = Guid.NewGuid().ToString();
                }
            }
            else if (dr.Table.Columns.Contains(column.ColumnName)) {
                value = dr[column.ColumnName].ToString();
            }


            query.Add(new DatabaseQuery(column.ColumnName, value, column.DataType.Name));
        }

        return query;
    }

    private void AddEntry(string table, List<DatabaseQuery> query) {
        dbCall.CallInsert(table, query);
    }

    private void deleteEntries(string table) {
        dbCall.CallDelete(table, null);
    }

    private void deleteEntriesByRow(string table, string key, string value) {
        dbCall.CallDelete(table, new List<DatabaseQuery>() { new DatabaseQuery(key, value) });
    }
}