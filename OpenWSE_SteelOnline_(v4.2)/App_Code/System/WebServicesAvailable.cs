using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.IO;
using System.Data.SqlServerCe;


[Serializable]
public struct WebService_Coll {
    private string _id;
    private string _fileName;
    private string _description;
    private string _uploadDate;
    private string _uploadedBy;

    public WebService_Coll(string id, string fileName, string description, string uploadDate, string uploadedBy) {
        _id = id;
        _fileName = fileName;
        _description = description;
        _uploadDate = uploadDate;
        _uploadedBy = uploadedBy;
    }

    public string ID {
        get { return _id; }
    }

    public string Filename {
        get { return _fileName; }
    }

    public string Description {
        get { return _description; }
    }

    public string UploadDate {
        get { return _uploadDate; }
    }

    public string UploadedBy {
        get { return _uploadedBy; }
    }
}


public class WebServicesAvailable
{
    private readonly string _username;
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<WebService_Coll> wsColl = new List<WebService_Coll>();

	public WebServicesAvailable()
	{
        _username = HttpContext.Current.User.Identity.Name;
	}

    public List<WebService_Coll> WS_Collection {
        get { return wsColl; }
    }

    public void AddItem(string id, string filename, string description) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Filename", filename));
        query.Add(new DatabaseQuery("Description", description));
        query.Add(new DatabaseQuery("UploadDate", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("UploadedBy", _username));

        dbCall.CallInsert("WebServices", query);
    }

    public void DeleteItem(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete("WebServices", query);
    }

    public void UpdateItem(string id, string filename, string description) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Filename", filename));
        updateQuery.Add(new DatabaseQuery("Description", description));

        dbCall.CallUpdate("WebServices", updateQuery, query);
    }

    public void GetWebServices() {
        wsColl.Clear();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("WebServices", "", null, "Filename ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string fileName = row["Filename"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            WebService_Coll coll = new WebService_Coll(id, fileName, description, uploadDate, uploadedBy);
            wsColl.Add(coll);
        }
    }

    public WebService_Coll GetWebService(string id) {
        WebService_Coll coll = new WebService_Coll();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("WebServices", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string fileName = row["Filename"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            coll = new WebService_Coll(id, fileName, description, uploadDate, uploadedBy);
            break;
        }

        return coll;
    }

    public string GetFileExt(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("WebServices", "Filename", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        FileInfo fi = new FileInfo(dbSelect.Value);
        return fi.Extension;
    }

}