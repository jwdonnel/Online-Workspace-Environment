using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;
using System.Web.Configuration;


public class AppDLLs {

    private readonly DatabaseCall dbCall = new DatabaseCall();

    public AppDLLs() { }

    public void AddItem(string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("AppID", appId));
        dbCall.CallInsert("aspnet_AppDLLs", query);
    }

    public void DeleteItem(string appId) {
        dbCall.CallDelete("aspnet_AppDLLs", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

}