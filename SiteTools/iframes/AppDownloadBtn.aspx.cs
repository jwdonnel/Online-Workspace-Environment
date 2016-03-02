using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Principal;
using System.Web.Security;

public partial class SiteTools_AppDownloadBtn : System.Web.UI.Page {
    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated) {
            if (!ServerSettings.AdminPagesCheck("appmanager.aspx", userId.Name)) {
                lb_downloadApp.Enabled = false;
                lb_downloadApp.Visible = false;
            }
            else {
                MemberDatabase member = new MemberDatabase(userId.Name);
                if (!string.IsNullOrEmpty(Request.QueryString["id"])) {
                    hf_appID.Value = Request.QueryString["id"];
                    lb_downloadApp.Text = "<img alt='download' src='../../App_Themes/" + member.SiteTheme + "/Icons/download.png' class='float-left pad-right-sml' />Download App";
                }
                else if (!string.IsNullOrEmpty(Request.QueryString["backup"])) {
                    if (Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                        hf_appID.Value = "DownloadAll";
                        lb_downloadApp.Text = "<img alt='download' src='../../App_Themes/" + member.SiteTheme + "/Icons/download.png' class='float-left pad-right-sml' />Download All Local Apps";
                    }
                    else {
                        lb_downloadApp.Enabled = false;
                        lb_downloadApp.Visible = false;
                    }
                }
                else {
                    lb_downloadApp.Enabled = false;
                    lb_downloadApp.Visible = false;
                }

                CustomFonts.SetCustomValues(this.Page, member);
            }
        }
        else {
            lb_downloadApp.Enabled = false;
            lb_downloadApp.Visible = false;
        }
    }


    private List<string> dfl;
    protected void lb_downloadApp_Click(object sender, EventArgs e) {
        string tempFile = string.Empty;
        dfl = new List<string>();
        App app = new App(string.Empty);
        string downloadFilename = hf_appID.Value.Replace("app-", "");

        if (hf_appID.Value == "DownloadAll") {
            string filePath = ServerSettings.GetServerMapLocation + "Apps\\";
            DirectoryInfo di = new DirectoryInfo(filePath);
            for (int i = 0; i < di.GetDirectories().Length; i++) {
                for (int j = 0; j < di.GetDirectories()[i].GetFiles().Length; j++) {
                    string fileLoc = di.GetDirectories()[i].Name + "/" + di.GetDirectories()[i].GetFiles()[j].Name;
                    GetBinFiles(fileLoc, di.GetDirectories()[i].FullName + "\\" + di.GetDirectories()[i].GetFiles()[j].Name);
                }
                BuildDownloadFileList(di.GetDirectories()[i].FullName, di.GetDirectories()[i].Name);
            }
            for (int i = 0; i < di.GetFiles().Length; i++) {
                if (!dfl.Contains(di.GetFiles()[i].Name))
                    dfl.Add(di.GetFiles()[i].Name);
            }

            tempFile = CreateSOB();
            dfl.Add(tempFile);

            downloadFilename = OpenWSE.Core.Licensing.CheckLicense.SiteName.Replace(" ", "_") + "_Apps";
        }
        else {
            string fileLoc = app.GetAppFilename(hf_appID.Value);
            string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + fileLoc.Replace("/", "\\");

            if (fileLoc.Contains('/')) {
                GetBinFiles(fileLoc, filePath);
                DirectoryInfo di = new DirectoryInfo(filePath);
                BuildDownloadFileList(di.Parent.FullName, di.Parent.Name);
            }
            else {
                dfl.Add(fileLoc);
            }
        }

        Response.ContentType = "application/zip";
        // If the browser is receiving a mangled zipfile, IIS Compression may cause this problem. Some members have found that
        //    Response.ContentType = "application/octet-stream"     has solved this. May be specific to Internet Explorer.

        Response.AppendHeader("content-disposition", "attachment; filename=\"" + downloadFilename + ".zip\"");
        Response.CacheControl = "Private";
        Response.Cache.SetExpires(ServerSettings.ServerDateTime.AddMinutes(3)); // or put a timestamp in the filename in the content-disposition

        byte[] buffer = new byte[4096];

        using (ZipOutputStream zipOutputStream = new ZipOutputStream(Response.OutputStream)) {
            zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression

            foreach (string fileName in dfl) {
                string tempFilename = fileName;
                if (!File.Exists(tempFilename)) {
                    tempFilename = ServerSettings.GetServerMapLocation + "Apps\\" + fileName;
                }

                using (FileStream fs = File.OpenRead(tempFilename))    // or any suitable inputstream
                {
                    string cleanName = tempFilename.Replace(ServerSettings.GetServerMapLocation, string.Empty);
                    if (cleanName.StartsWith("Apps")) {
                        cleanName = cleanName.Replace("Apps", string.Empty);
                    }

                    ZipEntry entry = new ZipEntry(ZipEntry.CleanName(cleanName));
                    entry.Size = fs.Length;
                    // Setting the Size provides WinXP built-in extractor compatibility,
                    //  but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.

                    zipOutputStream.PutNextEntry(entry);

                    int count = fs.Read(buffer, 0, buffer.Length);
                    while (count > 0) {
                        zipOutputStream.Write(buffer, 0, count);
                        count = fs.Read(buffer, 0, buffer.Length);
                        if (!Response.IsClientConnected)
                            break;

                        Response.Flush();
                    }
                }
            }
        }
        dfl.Clear();

        if (!string.IsNullOrEmpty(tempFile))
            DeleteRestore(tempFile);

        Response.Flush();
        Response.End();
        //Response.Redirect("~/SiteTools/iframes/AppDownloadBtn.aspx", true);
    }

    private void BuildDownloadFileList(string filePath, string rootDir) {
        DirectoryInfo di = new DirectoryInfo(filePath);
        for (int i = 0; i < di.GetDirectories().Length; i++) {
            BuildDownloadFileList(di.GetDirectories()[i].FullName, rootDir + "/" + di.GetDirectories()[i].Name);
        }
        for (int i = 0; i < di.GetFiles().Length; i++) {
            if (!dfl.Contains(rootDir + "/" + di.GetFiles()[i].Name))
                dfl.Add(rootDir + "/" + di.GetFiles()[i].Name);
        }
    }

    private void GetBinFiles(string fileLoc, string filePath) {
        if (new FileInfo(fileLoc).Extension.ToLower() == ".ascx" || new FileInfo(fileLoc).Extension.ToLower() == ".aspx"
            || new FileInfo(fileLoc).Extension.ToLower() == ".asmx" || new FileInfo(fileLoc).Extension.ToLower() == ".ashx") {
            string[] innerContents = File.ReadAllLines(filePath);
            foreach (string line in innerContents) {
                if (line.ToLower().Contains("inherits=")) {
                    string subStr = line.Substring(line.ToLower().IndexOf("inherits=") + ("inherits=").Length + 1);
                    subStr = subStr.Remove(subStr.IndexOf("\""));
                    string[] splitInherits = subStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    string[] binFiles = new string[0];
                    if (Directory.Exists(ServerSettings.GetServerMapLocation + "bin")) {
                        binFiles = Directory.GetFiles(ServerSettings.GetServerMapLocation + "bin");
                    }

                    foreach (string inheritFile in splitInherits) {
                        foreach (string file in binFiles) {
                            FileInfo fi = new FileInfo(file);
                            string fileName = fi.Name.ToLower().Replace(fi.Extension.ToLower(), string.Empty);
                            if (fileName.Trim().ToLower() == inheritFile.Trim().ToLower()) {
                                dfl.Add(file);
                            }
                        }
                    }
                }
            }
        }
    }

    private string CreateSOB() {
        string currTable = "AppList";
        string date = ServerSettings.ServerDateTime.ToFileTime().ToString();
        string fileName = currTable + "_" + date + "Temp" + ServerSettings.BackupFileExt;
        string realfileName = currTable + "_" + date + ServerSettings.BackupFileExt;
        string loc = ServerSettings.GetServerMapLocation + "Backups\\" + fileName;
        var sb = new ServerBackup(HttpContext.Current.User.Identity.Name, loc);
        DBViewer _dbviewer = new DBViewer(false);
        _dbviewer.GetTableData(currTable);
        sb.BinarySerialize_Ind(_dbviewer.dt, currTable);

        return ServerSettings.GetServerMapLocation + "Backups\\" + realfileName;
    }

    private static void DeleteRestore(string filename) {
        try {
            if (File.Exists(filename))
                File.Delete(filename);
        }
        catch { }
    }
}