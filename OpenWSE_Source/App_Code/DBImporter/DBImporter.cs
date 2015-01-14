#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using OpenWSE.Core;
using OpenWSE.Core.Licensing;

#endregion

/// <summary>
///     Summary description for DBImporter
/// </summary>
[Serializable]
public class DBImporter_Coll
{
    private readonly string _cs;
    private readonly string _date;
    private readonly string _ib;
    private readonly string _id;
    private readonly string _p;
    private string _sc;
    private string _tn;
    private string _allowEdit;

    public DBImporter_Coll(string id, string date, string tn, string cs, string sc, string p, string ib, string allowEdit)
    {
        _id = id;
        _date = date;
        _tn = tn;
        _cs = cs;
        _sc = sc;
        _p = p;
        _ib = ib;
        _allowEdit = allowEdit;
    }

    public string ID
    {
        get { return _id; }
    }

    public string Date
    {
        get { return _date; }
    }

    public string TableName
    {
        get { return _tn; }
        set { _tn = value; }
    }

    public string ConnString
    {
        get { return _cs; }
    }

    public string SelectCommand
    {
        get { return _sc; }
        set { _sc = value; }
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
        set { _allowEdit = value.ToString(); }
        get 
        {
            return HelperMethods.ConvertBitToBoolean(_allowEdit); 
        }
    }
}


[Serializable]
public class DBImporter
{
    private readonly AppLog applog = new AppLog(false);
    private readonly string encryptloc;
    private readonly string loc;
    private List<DBImporter_Coll> _coll = new List<DBImporter_Coll>();

    public DBImporter()
    {
        loc = ServerSettings.GetServerMapLocation + "App_Data\\Imports" + ServerSettings.SavedDataFilesExt;
        encryptloc = ServerSettings.GetServerMapLocation + "App_Data\\Imports_Encrypted" + ServerSettings.SavedDataFilesExt;
    }

    public List<DBImporter_Coll> DBColl
    {
        get { return _coll; }
    }

    #region Saved Connections

    private readonly string encryptloc_SavedConnections = ServerSettings.GetServerMapLocation + "App_Data\\SavedConnections_Encrypted" + ServerSettings.SavedDataFilesExt;

    private readonly string loc_SavedConnections = ServerSettings.GetServerMapLocation + "App_Data\\SavedConnections" + ServerSettings.SavedDataFilesExt;

    private List<SavedConnections> _coll_SavedConnections = new List<SavedConnections>();

    public List<SavedConnections> SavedConnections_Coll
    {
        get { return _coll_SavedConnections; }
    }

    public void SavedConnectionsSerialize(List<SavedConnections> list)
    {
        try
        {
            DataEncryption.SerializeFile(list, loc_SavedConnections);
            DataEncryption.EncryptFile(loc_SavedConnections, encryptloc_SavedConnections);
        }
        catch (Exception e)
        {
            applog.AddError(e);
        }
    }

    public void SavedConnectionsDeserialize()
    {
        try
        {
            if (File.Exists(encryptloc_SavedConnections))
            {
                MemoryStream a = DataEncryption.DecryptFile(encryptloc_SavedConnections);
                using (var str = new BinaryReader(a))
                {
                    var bf = new BinaryFormatter();
                    str.BaseStream.Position = 0;
                    _coll_SavedConnections = (List<SavedConnections>) bf.Deserialize(str.BaseStream);
                    a.Close();
                    str.Close();
                }
            }
        }
        catch (Exception e)
        {
            applog.AddError(e);
        }
    }

    #endregion

    public void BinarySerialize(List<DBImporter_Coll> list)
    {
        try
        {
            DataEncryption.SerializeFile(list, loc);
            DataEncryption.EncryptFile(loc, encryptloc);
        }
        catch (Exception e)
        {
            applog.AddError(e);
        }
    }

    public void BinaryDeserialize()
    {
        try
        {
            if (File.Exists(encryptloc))
            {
                MemoryStream a = DataEncryption.DecryptFile(encryptloc);
                using (var str = new BinaryReader(a))
                {
                    var bf = new BinaryFormatter();
                    str.BaseStream.Position = 0;
                    _coll = (List<DBImporter_Coll>) bf.Deserialize(str.BaseStream);
                    a.Close();
                    str.Close();
                }
            }
        }
        catch (Exception e)
        {
            applog.AddError(e);
        }
    }

    public void DeleteEntry(string id)
    {
        if (_coll.Count == 0)
        {
            BinaryDeserialize();
        }

        for (int i = 0; i < _coll.Count; i++)
        {
            if (_coll[i].ID == id)
            {
                _coll.RemoveAt(i);
                BinarySerialize(_coll);
                break;
            }
        }
    }

    public void UpdateEntry(string id, string tablename, string selectcommand, bool allowEdit)
    {
        if (_coll.Count == 0)
        {
            BinaryDeserialize();
        }

        for (int i = 0; i < _coll.Count; i++)
        {
            if (_coll[i].ID == id)
            {
                _coll[i].TableName = tablename;
                _coll[i].SelectCommand = selectcommand;
                _coll[i].AllowEdit = allowEdit;
                BinarySerialize(_coll);
                break;
            }
        }
    }
}


[Serializable]
public class SavedConnections
{
    private string _connectionstring;
    private string _date;
    private string _dbprovider;
    private string _id;
    private string _name;
    private string _username;

    public SavedConnections(string id, string connectionstring, string name, string dbprovider, string date,
                            string username)
    {
        _id = id;
        _connectionstring = connectionstring;
        _name = name;
        _dbprovider = dbprovider;
        _date = date;
        _username = username;
    }

    public string ID
    {
        get { return _id; }
        set { _id = value; }
    }

    public string ConnectionString
    {
        get { return _connectionstring; }
        set { _connectionstring = value; }
    }

    public string ConnectionName
    {
        get { return _name; }
        set { _name = value; }
    }

    public string DatabaseProvider
    {
        get { return _dbprovider; }
        set { _dbprovider = value; }
    }

    public string Date
    {
        get { return _date; }
        set { _date = value; }
    }

    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }
}