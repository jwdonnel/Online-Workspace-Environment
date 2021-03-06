﻿#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Configuration;
using OpenWSE.Core;
using System.Data;

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
        dbCall.NeedToLog = false;
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
            if (!list.ContainsKey(dr["TABLE_NAME"].ToString())) {
                list.Add(dr["TABLE_NAME"].ToString(), tempviewer.dt);
            }
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

                if (local != null) {
                    // Delete current table entries before re-inserting them
                    deleteEntries(key);

                    // Re-insert data into table
                    foreach (DataRow dr in t.Rows) {
                        List<DatabaseQuery> query = BuildQuery(dr, local.Columns);

                        bool removeApplicationId = false;
                        int removeApplicationIdIndex = 0;
                        foreach (DatabaseQuery q in query) {
                            if (q.Column == DatabaseCall.ApplicationIdString && q.Value != ServerSettings.ApplicationID) {
                                removeApplicationId = true;
                                break;
                            }

                            removeApplicationIdIndex++;
                        }

                        if (removeApplicationId) {
                            query.RemoveAt(removeApplicationIdIndex);
                            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
                        }

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
                if (column.ColumnName.ToLower() == DatabaseCall.ApplicationIdString.ToLower()) {
                    value = ServerSettings.ApplicationID;
                }
                else {
                    value = Guid.NewGuid().ToString();
                }
            }
            else if (dr.Table.Columns.Contains(column.ColumnName)) {
                value = dr[column.ColumnName].ToString();
            }

            if (column.DataType.Name.ToLower() == "datetime" && string.IsNullOrEmpty(value)) {
                value = DateTime.Now.ToString();
            }

            if (column.DataType.Name.ToLower() == "int32" && string.IsNullOrEmpty(value)) {
                value = "0";
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