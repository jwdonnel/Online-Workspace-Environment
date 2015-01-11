using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlServerCe;


[Serializable]
public class CustomProjects_Coll {
    private string _ProjectID;
    private string _folder;
    private string _description;
    private DateTime _uploadDate = new DateTime();
    private string _uploadedBy;
    private string _uploadName;
    private string _defaultPage;

    public CustomProjects_Coll(string ProjectID, string folder, string description, string uploadDate, string uploadedBy, string uploadName, string defaultPage) {
        _ProjectID = ProjectID;
        _folder = folder;
        _description = description;
        DateTime.TryParse(uploadDate, out _uploadDate);
        _uploadedBy = uploadedBy;
        _uploadName = uploadName;
        _defaultPage = defaultPage;
    }

    public string ProjectID {
        get { return _ProjectID; }
    }

    public string Folder {
        get { return _folder; }
    }

    public string Description {
        get { return _description; }
    }

    public DateTime UploadDate {
        get { return _uploadDate; }
    }

    public string UploadedBy {
        get { return _uploadedBy; }
    }

    public string UploadName {
        get { return _uploadName; }
    }

    public string DefaultPage {
        get { return _defaultPage; }
    }
}

[Serializable]
public class FileExplorerList {
    private string _id;
    private string _filename;
    private FileInfo _fi;
    private DirectoryInfo _di;

    public FileExplorerList(string id, string filename, FileInfo fi, DirectoryInfo di) {
        _id = id;
        _filename = filename;
        _fi = fi;
        _di = di;
    }

    public string ID {
        get { return _id; }
    }

    public string Filename {
        get { return _filename; }
    }

    public FileInfo File_Info {
        get { return _fi; }
    }

    public DirectoryInfo Directory_Info {
        get { return _di; }
    }
}

public class CustomProjects {
    public const string SessionName = "ProjectFileUpload_DefaultDir";
    private const string dbTable = "CustomProjects";
    public const string customPageFolder = "CustomProjects";
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<CustomProjects_Coll> _intColl = new List<CustomProjects_Coll>();
    private string _userName;

    public CustomProjects(string userName) {
        _intColl.Clear();
        _userName = userName;
    }

    public void AddItem(string ProjectID, string folder, string description, string uploadName, string defaultPage) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ProjectID", ProjectID));
        query.Add(new DatabaseQuery("Folder", folder));
        query.Add(new DatabaseQuery("Description", description));
        query.Add(new DatabaseQuery("UploadDate", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("UploadedBy", _userName));
        query.Add(new DatabaseQuery("UploadName", uploadName));
        query.Add(new DatabaseQuery("DefaultPage", defaultPage));
        dbCall.CallInsert(dbTable, query);
    }

    public CustomProjects_Coll GetEntry(string ProjectID) {
        CustomProjects_Coll coll = null;
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", new List<DatabaseQuery>() { new DatabaseQuery("ProjectID", ProjectID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ProjectID"];
            string folder = row["Folder"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            string uploadName = row["UploadName"];
            string defaultPage = row["DefaultPage"];
            coll = new CustomProjects_Coll(id, folder, description, uploadDate, uploadedBy, uploadName, defaultPage);
            break;
        }

        return coll;
    }

    public string GetDefaultPage(string ProjectID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(dbTable, "DefaultPage", new List<DatabaseQuery>() { new DatabaseQuery("ProjectID", ProjectID) });
        return dbSelect.Value;
    }

    public string GetFolder(string ProjectID) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(dbTable, "Folder", new List<DatabaseQuery>() { new DatabaseQuery("ProjectID", ProjectID) });
        return dbSelect.Value;
    }

    public void BuildEntriesByFolder(string folder) {
        _intColl.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder) }, "UploadName ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ProjectID"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            string uploadName = row["UploadName"];
            string defaultPage = row["DefaultPage"];
            CustomProjects_Coll coll = new CustomProjects_Coll(id, folder, description, uploadDate, uploadedBy, uploadName, defaultPage);
            _intColl.Add(coll);
        }
    }

    public static bool IsFtpFolder(string folder) {
        if (folder.StartsWith("ftp://") || folder.StartsWith("ftps://")) {
            return true;
        }
        return false;
    }

    public List<string> BuildEntryNameListByFolder(string folder) {
        List<string> returnList = new List<string>();

        string mainDir = ServerSettings.GetServerMapLocation + customPageFolder + "\\" + folder;
        BuildEntryNameListByFolder(ref returnList, mainDir, mainDir);

        return returnList;
    }
    private void BuildEntryNameListByFolder(ref List<string> returnList, string folder, string mainDir) {
        string[] fileEntries = Directory.GetFiles(folder);
        foreach (string fileName in fileEntries) {
            string fn = fileName.Replace(mainDir + "\\", "");
            if (!returnList.Contains(fn)) {
                returnList.Add(fn);
            }
        }

        string[] subdirectoryEntries = Directory.GetDirectories(folder);
        foreach (string subdirectory in subdirectoryEntries) {
            BuildEntryNameListByFolder(ref returnList, subdirectory, mainDir);
        }
    }

    public void BuildEntryNamesAll() {
        _intColl.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", null, "UploadName ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ProjectID"];
            string folder = row["Folder"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            string uploadName = row["UploadName"];
            string defaultPage = row["DefaultPage"];
            CustomProjects_Coll coll = new CustomProjects_Coll(id, folder, description, uploadDate, uploadedBy, uploadName, defaultPage);
            _intColl.Add(coll);
        }
    }

    public void BuildEntriesForCurrentUser() {
        _intColl.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", new List<DatabaseQuery>() { new DatabaseQuery("UploadedBy", _userName) }, "UploadName ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ProjectID"];
            string folder = row["Folder"];
            string description = row["Description"];
            string uploadDate = row["UploadDate"];
            string uploadedBy = row["UploadedBy"];
            string uploadName = row["UploadName"];
            string defaultPage = row["DefaultPage"];
            CustomProjects_Coll coll = new CustomProjects_Coll(id, folder, description, uploadDate, uploadedBy, uploadName, defaultPage);
            _intColl.Add(coll);
        }
    }

    public void UpdateRow(string ProjectID, string description, string uploadName) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ProjectID", ProjectID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Description", description));
        updateQuery.Add(new DatabaseQuery("UploadName", uploadName));

        dbCall.CallUpdate(dbTable, updateQuery, query);
    }

    public void UpdateDefaultPage(string ProjectID, string defaultPage) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ProjectID", ProjectID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DefaultPage", defaultPage));

        dbCall.CallUpdate(dbTable, updateQuery, query);
    }

    public void DeleteRow(string ProjectID) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ProjectID", ProjectID));

        dbCall.CallDelete(dbTable, query);
    }

    public List<CustomProjects_Coll> CustomPageCollection {
        get { return _intColl; }
    }
}