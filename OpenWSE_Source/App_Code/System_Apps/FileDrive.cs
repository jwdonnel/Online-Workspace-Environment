#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Web;
using System.Web.Configuration;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;

#endregion

[Serializable]
public class FileDriveFolders_Coll {
    private readonly DateTime _date;
    private readonly string _foldername;
    private readonly string _groupname;
    private readonly Guid _id;

    public FileDriveFolders_Coll(Guid id, string foldername, DateTime date, string groupname) {
        _id = id;
        _foldername = foldername;
        _date = date;
        _groupname = groupname;
    }

    public Guid ID {
        get { return _id; }
    }

    public string FolderName {
        get { return _foldername; }
    }

    public DateTime Date {
        get { return _date; }
    }

    public string GroupName {
        get { return _groupname; }
    }
}

[Serializable]
public class FileDriveDocuments_Coll {
    private readonly string _comment;
    private readonly DateTime _date;
    private readonly string _ext;
    private readonly string _filename;
    private readonly string _folder;
    private readonly string _groupname;
    private readonly Guid _id;
    private readonly string _owner;
    private string _path;
    private readonly string _size;

    public FileDriveDocuments_Coll(Guid id, string filename, string ext, string size, string path, string comment, string date,
                             string folder, string owner, string groupname) {
        _id = id;
        _filename = filename;
        _ext = ext;
        _size = size;
        _path = path;
        _comment = comment;
        _date = Convert.ToDateTime(date);
        _folder = folder;
        _owner = owner;
        _groupname = groupname;
    }

    public Guid ID {
        get { return _id; }
    }

    public string FileName {
        get { return _filename; }
    }

    public string FileExtension {
        get { return _ext; }
    }

    public string FileSize {
        get { return _size; }
    }

    public string FilePath {
        get {
            if (_path.Contains("CloudFiles"))
                _path = ServerSettings.GetServerMapLocation + _path;

            return _path;
        }
    }

    public string Comment {
        get { return _comment; }
    }

    public DateTime UploadDate {
        get { return _date; }
    }

    public string Folder {
        get { return _folder; }
    }

    public string owner {
        get { return _owner; }
    }

    public string GroupName {
        get { return _groupname; }
    }
}

/// <summary>
///     Summary description for FileDriveDocuments
/// </summary>
[Serializable]
public class FileDrive {
    public const string NewFileExt = ".file";

    private readonly AppLog applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly string username;
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private List<FileDriveDocuments_Coll> _documents_coll = new List<FileDriveDocuments_Coll>();
    private List<FileDriveFolders_Coll> _folders_coll = new List<FileDriveFolders_Coll>();
    private MemberDatabase member = new MemberDatabase();

    public FileDrive(string username) {
        this.username = username;
    }

    public List<FileDriveDocuments_Coll> documents_coll {
        get { return _documents_coll; }
    }

    public List<FileDriveFolders_Coll> folders_coll {
        get { return _folders_coll; }
    }

    public void GetAllFolders(string groupname) {
        _folders_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveFolders", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string f = row["Folder"];
            DateTime d = Convert.ToDateTime(row["Date"]);
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveFolders_Coll(id, f, d, g);
                updateFolders(coll);
            }
        }
    }

    public static bool FileExtOk(string extension) {
        var ok = (extension.ToLower() == ".png") || (extension.ToLower() == ".bmp") || (extension.ToLower() == ".jpg")
                 || (extension.ToLower() == ".jpeg") || (extension.ToLower() == ".jpe") || (extension.ToLower() == ".jfif")
                 || (extension.ToLower() == ".tif") || (extension.ToLower() == ".tiff") || (extension.ToLower() == ".gif")
                 || (extension.ToLower() == ".tga") || (extension.ToLower() == ".mp3") || (extension.ToLower() == ".mp4");

        return ok;
    }

    public void GetAllFolders() {
        _folders_coll.Clear();
        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveFolders", "", null);
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string f = row["Folder"];
            DateTime d = Convert.ToDateTime(row["Date"]);
            string g = row["GroupName"];
            var coll = new FileDriveFolders_Coll(id, f, d, g);
            updateFolders(coll);
        }
    }

    public string GetFolderForMove(Guid id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("FileDriveDocuments", "Folder", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
        return dbSelect.Value;
    }

    public string GetFolderbyID(Guid id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("FileDriveFolders", "Folder", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
        return dbSelect.Value;
    }

    public string GetFilenamebyID(Guid id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("FileDriveDocuments", "FileName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
        return dbSelect.Value;
    }

    public void GetAllFiles(string groupname) {
        member = new MemberDatabase(username);
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", null);
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public void GetAllFiles() {
        _documents_coll.Clear();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", null);
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
            updateDocuments(coll);
        }
    }

    public void GetFilesByID(Guid id) {
        _documents_coll.Clear();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
        foreach (Dictionary<string, string> row in dataTable) {
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
            updateDocuments(coll);
        }
    }

    public void GetFilesByOwner(string owner, string groupname) {
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("Owner", owner) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public void GetPersonalFiles(string folder, string group) {
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(group, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public void GetFilesByFolderName(string folder, string groupname) {
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public List<FileDriveDocuments_Coll> GetFilesInFolderName(string folder, string groupname) {
        var temp = new List<FileDriveDocuments_Coll>();
        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                temp.Add(coll);
            }
        }

        return temp;
    }

    public void GetFilesByFilename(string filename, string groupname) {
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("FileName", filename) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public void GetFilesByExtension(string ext, string groupname) {
        _documents_coll.Clear();

        Groups groups = new Groups();
        List<string> groupList = groups.GetEntryList();

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "", new List<DatabaseQuery>() { new DatabaseQuery("FileExtension", ext) });
        foreach (Dictionary<string, string> row in dataTable) {
            var id = Guid.Parse(row["ID"]);
            string fn = row["FileName"];
            string fe = row["FileExtension"];
            string fs = row["FileSize"];
            string fp = row["FilePath"];
            string c = row["Comment"];
            string ud = row["UploadDate"];
            string a = row["Folder"];
            string o = row["Owner"];
            string g = row["GroupName"];

            if (CheckIfInGroup(groupname, g, groupList)) {
                var coll = new FileDriveDocuments_Coll(id, fn, fe, fs, fp, c, ud, a, o, g);
                updateDocuments(coll);
            }
        }
    }

    public string GetFileNamePath(Guid id) {
        string result = string.Empty;

        List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveDocuments", "FileName, FilePath", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
        foreach (Dictionary<string, string> row in dataTable) {
            string fn = row["FileName"];
            string fp = row["FilePath"];

            result = fp + "\\" + fn;
            result = ServerSettings.GetServerMapLocation.Replace("\\Apps\\FileDrive", "") + result;
        }

        return result;
    }

    public bool CheckFolderExists(string folder, string groupname) {
        bool exists = false;

        if (string.IsNullOrEmpty(groupname)) {
            List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveFolders", "ID", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder) });
            if (dataTable.Count > 0) {
                exists = true;
            }
        }
        else {
            string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string gr in groupList) {
                List<Dictionary<string, string>> dataTable = dbCall.CallSelect("FileDriveFolders", "ID", new List<DatabaseQuery>() { new DatabaseQuery("Folder", folder), new DatabaseQuery("GroupName", gr) });
                if (dataTable.Count > 0) {
                    exists = true;
                    break;
                }
            }
        }
        return exists;
    }

    public void addFile(string id, string filename, string ext, string size, string path, string comment, string Folder, string groupname, bool autoUpdate = true) {
        if (path.IndexOf(ServerSettings.GetServerMapLocation) == 0)
            path = "\\" + path.Replace(ServerSettings.GetServerMapLocation, "");

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("FileName", filename));
        query.Add(new DatabaseQuery("FileExtension", ext));
        query.Add(new DatabaseQuery("FileSize", size));
        query.Add(new DatabaseQuery("FilePath", path));
        query.Add(new DatabaseQuery("Comment", comment));
        query.Add(new DatabaseQuery("UploadDate", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("Folder", Folder));
        query.Add(new DatabaseQuery("Owner", username));
        query.Add(new DatabaseQuery("GroupName", groupname));

        dbCall.CallInsert("FileDriveDocuments", query);
    }

    public string addfolder(string foldername, string groupname) {
        string result = "false";
        member = new MemberDatabase(username);
        List<string> tempG = member.GroupList;
        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string g in groupList) {
            if (!CheckFolderExists(foldername, g)) {
                string x = g;
                if (string.IsNullOrEmpty(g)) {
                    if (tempG.Count > 0) {
                        x = tempG[0];
                    }
                }
                string date = DateTime.Now.ToString();

                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
                query.Add(new DatabaseQuery("Folder", foldername));
                query.Add(new DatabaseQuery("Date", date));
                query.Add(new DatabaseQuery("GroupName", x));

                if (dbCall.CallInsert("FileDriveFolders", query)) {
                    uuf.addFlag("app-filedrive", x);
                    result = "true";
                }
            }
        }
        return result;
    }

    public void deleteFile(Guid id) {
        dbCall.CallDelete("FileDriveDocuments", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
    }

    public void deleteFolder(Guid id, string groupname) {
        List<FileDriveDocuments_Coll> temp = GetFilesInFolderName(GetFolderbyID(id), groupname);
        foreach (FileDriveDocuments_Coll t in temp) {
            updateFolderName(t.ID, "-", groupname);
        }

        dbCall.CallDelete("FileDriveFolders", new List<DatabaseQuery>() { new DatabaseQuery("ID", id.ToString()) });
    }

    public void updateFileName(Guid id, string newname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id.ToString()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("FileName", newname));

        if (dbCall.CallUpdate("FileDriveDocuments", updateQuery, query)) {
            uuf.addFlag("app-filedrive", "");
        }
    }

    public void updateFolderName(Guid id, string newname, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id.ToString()));

        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        foreach (string gr in groupList) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Folder", newname));
            updateQuery.Add(new DatabaseQuery("GroupName", gr));

            if (dbCall.CallUpdate("FileDriveDocuments", updateQuery, query)) {
                uuf.addFlag("app-filedrive", "");
            }
        }
    }

    public void updateFilePath(Guid id, string newpath, string oldpath) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id.ToString()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("FilePath", newpath));

        if (dbCall.CallUpdate("FileDriveDocuments", updateQuery, query)) {
            uuf.addFlag("app-filedrive", "");
        }
    }

    public void updateDocumentGroup(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id.ToString()));

        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        foreach (string gr in groupList) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("GroupName", gr));

            if (dbCall.CallUpdate("FileDriveDocuments", updateQuery, query)) {
                uuf.addFlag("app-filedrive", "");
            }
        }
    }

    public void updateFolderGroup(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id.ToString()));

        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        foreach (string gr in groupList) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("GroupName", gr));

            dbCall.CallUpdate("FileDriveFolders", updateQuery, query);
        }
    }

    public string updateFolderNameMain(string newname, string oldname, string groupname) {
        string result = "false";
        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);



        foreach (string gr in groupList) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("GroupName", gr));
            query.Add(new DatabaseQuery("Folder", oldname));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Folder", newname));
            result = "true";
            dbCall.CallUpdate("FileDriveFolders", updateQuery, query);
            uuf.addFlag("app-filedrive", gr);

            if (HelperMethods.ConvertBitToBoolean(result)) {
                List<FileDriveDocuments_Coll> temp = GetFilesInFolderName(oldname, gr);
                foreach (FileDriveDocuments_Coll t in temp) {
                    updateFolderName(t.ID, newname, gr);
                }
            }
        }

        return result;
    }

    private void updateDocuments(FileDriveDocuments_Coll coll) {
        _documents_coll.Add(coll);
    }

    private void updateFolders(FileDriveFolders_Coll coll) {
        _folders_coll.Add(coll);
    }

    private bool CheckIfInGroup(string selectedGroup, string folderGroup, List<string> groupList) {
        string[] selectedGroupList = selectedGroup.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        List<string> folderGroupList = new List<string>(folderGroup.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries));

        if (selectedGroupList.Length == 0) {
            foreach (string _g in folderGroupList) {
                if (!groupList.Contains(_g)) {
                    return true;
                }
            }
        }
        else {
            foreach (string _g in selectedGroupList) {
                if (folderGroupList.Contains(_g)) {
                    return true;
                }
            }
        }

        return false;
    }
}