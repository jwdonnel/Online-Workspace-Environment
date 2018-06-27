using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;


public class OverlayDLLs {

    private readonly DatabaseCall dbCall = new DatabaseCall();

    public OverlayDLLs() { }

    public void AddItem(string overlayId, string folderPath) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("OverlayID", overlayId));
        query.Add(new DatabaseQuery("FolderPath", folderPath));
        dbCall.CallInsert("aspnet_OverlayDLLs", query);
    }

    public string GetFolderPath(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_OverlayDLLs", "FolderPath", new List<DatabaseQuery>() { new DatabaseQuery("OverlayID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public void DeleteItem(string overlayId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("OverlayID", overlayId));
        dbCall.CallDelete("aspnet_OverlayDLLs", query);
    }

}