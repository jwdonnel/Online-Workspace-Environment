using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;
using System.Web.Configuration;


public class CustomProjectDLLs {

    private readonly AppLog applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public CustomProjectDLLs() { }

    public void AddItem(string pageFolder) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("ProjectFolder", pageFolder));
        dbCall.CallInsert("aspnet_CustomProjectDLLs", query);
    }

    public void DeleteItem(string pageFolder) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ProjectFolder", pageFolder));
        dbCall.CallDelete("aspnet_CustomProjectDLLs", query);
    }

}