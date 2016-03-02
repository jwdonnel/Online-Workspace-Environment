using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlServerCe;


[Serializable]
public class ChatLogsDeletedColl
{
    private string _id;
    private string _username;
    private DateTime _date;

    public ChatLogsDeletedColl(string id, string username, string date)
    {
        _id = id;
        _username = username;
        _date = new DateTime();
        DateTime.TryParse(date, out _date);
    }

    public string ID
    {
        get { return _id; }
    }

    public string UserName
    {
        get { return _username; }
    }

    public DateTime Date
    {
        get { return _date; }
    }
}


public class ChatLogsDeleted
{

    #region Private Variables

    private List<ChatLogsDeletedColl> _dataTable = new List<ChatLogsDeletedColl>();
    private string _username;
    private readonly DatabaseCall dbCall = new DatabaseCall();

    #endregion


	public ChatLogsDeleted(string username)
	{
        _username = username;
	}

    public void BuildLog()
    {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_ChatLogsDeleted", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "LogDate ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string username = row["UserName"].ToLower();
            string date = row["LogDate"];
            var coll = new ChatLogsDeletedColl(id, username, date);
            _dataTable.Add(coll);
        }
    }

    public void AddEntry(string date)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("LogDate", date));
        dbCall.CallInsert("aspnet_ChatLogsDeleted", query);
    }

    public void DeleteEntry(string date)
    {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("LogDate", date));
        query.Add(new DatabaseQuery("UserName", _username.ToLower()));
        dbCall.CallDelete("aspnet_ChatLogsDeleted", query);
    }

    public void DeleteUserEntries(string userName) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", userName.ToLower()));
        dbCall.CallDelete("aspnet_ChatLogsDeleted", query);
    }

    public List<ChatLogsDeletedColl> ChatLogs
    {
        get { return _dataTable; }
    }
}