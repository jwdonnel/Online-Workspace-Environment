using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using OpenWSE_Tools.AutoUpdates;
using System.Data.SqlServerCe;


[Serializable]
public class BlockedChats_Coll
{
    private string _id;
    private string _username;
    private string _chatuser;
    private DateTime _datetime = new DateTime();

    public BlockedChats_Coll(string id, string username, string chatuser, string datetime)
    {
        _id = id;
        _username = username;
        _chatuser = chatuser;
        DateTime.TryParse(datetime, out _datetime);
    }

    public string ID
    {
        get { return _id; }
    }

    public string UserName
    {
        get { return _username; }
    }

    public string ChatUser
    {
        get { return _chatuser; }
    }

    public DateTime DateEntered
    {
        get { return _datetime; }
    }
}


public class BlockedChats
{
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<BlockedChats_Coll> _Coll = new List<BlockedChats_Coll>();
    private string _userName;

    public BlockedChats(string userName)
	{
        _Coll.Clear();
        _userName = userName;
	}

    public void AddItem(string chatUser) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _userName));
        query.Add(new DatabaseQuery("ChatUser", chatUser));
        query.Add(new DatabaseQuery("DateTime", DateTime.Now.ToString()));
        dbCall.CallInsert("aspnet_BlockedChats", query);
    }

    public void BuildEntries()
    {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_BlockedChats", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string username = row["UserName"];
            string chatuser = row["ChatUser"];
            string datetime = row["DateTime"];
            var coll = new BlockedChats_Coll(id, username, chatuser, datetime);
            _Coll.Add(coll);
        }
    }

    public void DeleteEntryByUserName(string userName) {
        dbCall.CallDelete("aspnet_BlockedChats", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName) });
    }

    public void DeleteEntryByID(string id) {
        dbCall.CallDelete("aspnet_BlockedChats", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public void DeleteEntryByChatUser(string chatUser) {
        dbCall.CallDelete("aspnet_BlockedChats", new List<DatabaseQuery>() { new DatabaseQuery("ChatUser", chatUser), new DatabaseQuery("UserName", _userName) });
    }

    public bool CheckIfBlocked(string user)
    {
        foreach (BlockedChats_Coll blocked in _Coll)
        {
            if (blocked.ChatUser.ToLower() == user.ToLower())
                return true;
        }

        return false;
    }

    public bool CheckIfBeingBlocked(string user)
    {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_BlockedChats", "ID", new List<DatabaseQuery>() { new DatabaseQuery("UserName", user), new DatabaseQuery("ChatUser", _userName) });
        if (!string.IsNullOrEmpty(dbSelect.Value)) {
            return true;
        }

        return false;
    }

    public List<BlockedChats_Coll> BlockedChatsCollection
    {
        get { return _Coll; }
    }
}