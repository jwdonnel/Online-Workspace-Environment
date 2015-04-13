<%@ WebService Language="C#" Class="FileDriveService" %>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for FileDriveService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class FileDriveService : System.Web.Services.WebService {

    private ServerSettings ss = new ServerSettings();

    [WebMethod]
    public void DeleteFolder(string folderId, string group) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var smw = new FileDrive(HttpContext.Current.User.Identity.Name);
            string temp = folderId.Substring(8);
            Guid id = Guid.Parse(temp);
            smw.deleteFolder(id, group);
        }
    }

    [WebMethod]
    public string UpdateFolder(string oldname, string newname, string group) {
        string result = "false";
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            result = "true";
            var smw = new FileDrive(HttpContext.Current.User.Identity.Name);
            Guid outGuid1 = new Guid();
            if ((newname.ToLower() == "create new folder") || (string.IsNullOrEmpty(newname)) || (newname.ToLower() == "my personal folder")
                || (newname.ToLower() == "personal folder") || (newname.ToLower() == "userfolder") || (newname.ToLower() == "root directory")
                || (oldname.ToLower() == newname.ToLower()) || (newname == "-") || Guid.TryParse(newname, out outGuid1)) {
                result = "false";
            }
            else {
                newname = removeRegex(newname);
                smw.GetAllFolders();
                for (int i = 0; i < smw.folders_coll.Count; i++) {
                    if (group == smw.folders_coll[i].GroupName) {
                        if (smw.folders_coll[i].FolderName.ToLower() == newname.ToLower()) {
                            result = "false";
                            break;
                        }
                    }
                }
                if (HelperMethods.ConvertBitToBoolean(result)) {
                    result = smw.updateFolderNameMain(newname, oldname, group);
                }
            }
        }

        return result;
    }

    [WebMethod]
    public string NewFolder(string foldername, string group) {
        string result = "false";
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            result = "true";
            var smw = new FileDrive(HttpContext.Current.User.Identity.Name);
            Guid outGuid2 = new Guid();
            if ((foldername.ToLower() == "create new folder") ||
                (foldername.ToLower() == "root directory") || (foldername.ToLower() == "my personal folder") || (foldername == "-")
                || (foldername.ToLower() == "personal folder") || (foldername.ToLower() == "userfolder") || (string.IsNullOrEmpty(foldername))
                || Guid.TryParse(foldername, out outGuid2)) {
                result = "false";
            }
            else {
                string newfolder = removeRegex(foldername);
                smw.GetAllFolders();
                for (int i = 0; i < smw.folders_coll.Count; i++) {
                    if (group == smw.folders_coll[i].GroupName) {
                        if (smw.folders_coll[i].FolderName.ToLower() == newfolder.ToLower()) {
                            result = "false";
                            break;
                        }
                    }
                }
                if (HelperMethods.ConvertBitToBoolean(result)) {
                    result = smw.addfolder(newfolder, group);
                }
            }
        }

        return result;
    }

    [WebMethod]
    public void RefreshFolder(string group) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (System.Web.Security.Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                refreshDatabase(ss.ResolvedDocumentFolder, group);
            }
        }
    }

    private string removeRegex(string foldername) {
        string fnew1 = foldername.Replace("'", "");
        string fnew1_temp = fnew1;
        fnew1 = fnew1_temp.Replace("&", "and");
        string fnew2_temp = fnew1;
        fnew1 = fnew2_temp.Replace("%", "");
        string fnew3_temp = fnew1;
        fnew1 = fnew3_temp.Replace(">", "");
        string fnew4_temp = fnew1;
        fnew1 = fnew4_temp.Replace("<", "");
        string fnew5_temp = fnew1;
        fnew1 = fnew5_temp.Replace("/", "");
        string fnew7_temp = fnew1;
        fnew1 = Regex.Replace(fnew7_temp, @"<(.|\n)*?>", string.Empty);
        return fnew1;
    }

    private void refreshDatabase(string folder, string groupname) {
        if (!string.IsNullOrEmpty(folder)) {
            var uri = new Uri(folder);
            if (uri.IsWellFormedOriginalString()) {
                folder = ServerSettings.GetServerMapLocation + Path.GetDirectoryName(uri.LocalPath);
            }

            if (Directory.Exists(folder)) {
                foreach (string f in Directory.GetFiles(folder)) {
                    IIdentity userID = HttpContext.Current.User.Identity;
                    var filesql = new FileDrive(userID.Name);
                    string pathsql = ss.ResolvedDocumentFolder;
                    string fileName_temp1sql = Path.GetFileName(f);
                    string fileNamesql = removeRegex(fileName_temp1sql);
                    string tempext = Path.GetExtension(fileNamesql);

                    string tempFileName = fileNamesql.Replace(tempext, "");
                    Guid x = new Guid();
                    if (Guid.TryParse(tempFileName, out x)) {
                        filesql.GetFilesByID(x);
                        if (filesql.documents_coll.Count > 0) {
                            continue;
                        }
                    }

                    if (tempext.ToLower() == ".db") {
                        continue;
                    }

                    string fileNameId = Guid.NewGuid().ToString();

                    if (tempext == FileDrive.NewFileExt) {
                        continue;
                    }

                    string p = Path.Combine(pathsql, fileNameId + FileDrive.NewFileExt);
                    if (FileDrive.FileExtOk(tempext)) {
                        p = Path.Combine(pathsql, fileNameId + tempext);
                    }

                    try {
                        filesql.GetAllFiles();
                        var tempsql = new List<string>();
                        for (int j = 0; j < filesql.documents_coll.Count; j++) {
                            if (!tempsql.Contains(filesql.documents_coll[j].FileName))
                                tempsql.Add(filesql.documents_coll[j].FileName);
                        }
                        bool cont = true;
                        if (File.Exists(p)) {
                            filesql.GetFilesByFilename(fileNamesql, groupname);
                            bool tempLoad = true;
                            for (int ij = 0; ij < tempsql.Count; ij++) {
                                if (tempsql[ij] == fileNamesql) {
                                    tempLoad = false;
                                    break;
                                }
                            }

                            if (tempLoad) {
                                var info = new FileInfo(p);
                                filesql.addFile(fileNameId, fileNamesql, tempext, HelperMethods.FormatBytes(info.Length), pathsql, string.Empty, "-", groupname, false);
                            }

                            for (int i = 0; i < filesql.documents_coll.Count; i++) {
                                if ((filesql.documents_coll[i].FileName == fileNamesql) &&
                                    (filesql.documents_coll[i].Folder == "-")) {
                                    cont = false;
                                    break;
                                }
                            }
                        }

                        if (cont) {
                            try {
                                File.Copy(f, p);
                                File.Delete(f);
                                var info = new FileInfo(p);
                                if (info.Exists) {
                                    filesql.addFile(fileNameId, fileNamesql, tempext, HelperMethods.FormatBytes(info.Length), pathsql, string.Empty, "-", groupname, false);
                                }
                            }
                            catch {
                            }
                        }
                    }
                    catch {
                    }
                    string[] directories = Directory.GetDirectories(folder);
                    foreach (string d in directories) {
                        refreshDatabase(d, groupname);
                    }
                }
            }
        }
    }

}