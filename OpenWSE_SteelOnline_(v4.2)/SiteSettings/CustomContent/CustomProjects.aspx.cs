using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Web.Security;
using ICSharpCode.SharpZipLib.Zip;

public partial class SiteSettings_CustomProjects : System.Web.UI.Page {
    #region private variables

    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private CustomProjects customProjects;
    private string _username;
    private bool AssociateWithGroups = false;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;

                AssociateWithGroups = _ss.AssociateWithGroups;

                customProjects = new CustomProjects(_username);
                if (Request.QueryString["error"] == "invalidformat") {
                    lbl_Error.Text = "Invalid file format!";
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#" + lbl_Error.ClientID + "').html(''); }, 3000);");
                }
                else if (Request.QueryString["error"] == "couldnotupload") {
                    lbl_Error.Text = "Error. Could not upload web service file!";
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#" + lbl_Error.ClientID + "').html(''); }, 3000);");
                }
                else if (!string.IsNullOrEmpty(Request.QueryString["ProjectID"])) {
                    string ProjectID = Request.QueryString["ProjectID"];
                    CustomProjects_Coll cpColl = customProjects.GetEntry(ProjectID);
                    if (cpColl != null) {
                        if (!CustomProjects.IsFtpFolder(cpColl.Folder) && cpColl.UploadedBy.ToLower() == _username.ToLower()) {
                            if (string.IsNullOrEmpty(cpColl.DefaultPage)) {
                                hf_defaultProjectID.Value = cpColl.ProjectID + "~NewEntry";
                                BuildDefaultPageList(cpColl.DefaultPage, cpColl.Folder);
                                RegisterPostbackScripts.RegisterStartupScript(this, "LoadDefaultPageSelector();");
                            }
                        }
                    }
                }
                BuildTable();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildTable() {
        customProjects.BuildEntryNamesAll();

        pnl_CustomPageList.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td style='min-width: 200px;'>Name</td>");
        str.Append("<td>Description</td>");
        str.Append("<td width='250px'>Default Page</td>");
        str.Append("<td width='150px'>Upload Date</td>");
        str.Append("<td width='150px'>Uploaded By</td>");
        str.Append("<td width='50px'>FTP</td>");
        str.Append("<td width='105px'>Actions</td></tr>");

        int fileCount = 0;
        MemberDatabase member = new MemberDatabase(_username);
        foreach (CustomProjects_Coll cp in customProjects.CustomPageCollection) {

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckCustomProjectsGroupAssociation(cp, member)) {
                    continue;
                }
            }

            string defaultPage = cp.DefaultPage.Replace(ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + cp.Folder + "\\", "");

            string downloadBtn = "<a href= '#download' class='td-download-btn margin-right-sml' onclick='DownloadCP(\"" + cp.ProjectID + "\"); return false;', title='Download'></a>";
            string deleteBtn = "<a href='#delete' class='td-delete-btn' onclick='DeleteCustomPage(\"" + cp.ProjectID + "\", \"" + cp.UploadName + "\");return false;' title='Delete'></a>";
            string editBtn = "<a href='#edit' class='td-edit-btn margin-right-sml' onclick='EditCustomPage(\"" + cp.ProjectID + "\");return false;' title='Edit'></a>";
            string linkBtn = "<a href='" + ResolveUrl("~/" + CustomProjects.customPageFolder + "/" + cp.Folder + "/" + defaultPage) + "' target='_blank' class='td-view-btn margin-right-sml' title='View'></a>";

            if ((!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) && (cp.UploadedBy.ToLower() != _username.ToLower())) {
                downloadBtn = string.Empty;
                editBtn = string.Empty;
                deleteBtn = string.Empty;
            }

            MemberDatabase tempMember = new MemberDatabase(cp.UploadedBy);

            fileCount++;
            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + fileCount.ToString() + "</td>");

            string desc = cp.Description;

            if (string.IsNullOrEmpty(desc)) {
                desc = "No description given";
            }

            if (string.IsNullOrEmpty(defaultPage)) {
                linkBtn = "";
                defaultPage = "No Default Page";
            }

            string uploadName = cp.UploadName;
            string isFtp = string.Empty;
            if (CustomProjects.IsFtpFolder(cp.Folder)) {
                isFtp = "<span class='img-checkmark'></span>";
                downloadBtn = string.Empty;
                linkBtn = string.Empty;
                defaultPage = "Not Available";
                uploadName += "<div class='clear-space-two'></div><small><i>" + cp.Folder + "</i></small>";
            }

            str.Append("<td align='left' class='border-right border-bottom'>" + uploadName + "</td>");
            str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + defaultPage + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + cp.UploadDate + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + isFtp + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + linkBtn + downloadBtn + editBtn + deleteBtn + "</td></tr>");
        }

        str.Append("</tbody></table></div>");
        if (fileCount == 0) {
            str.Append("<div class='emptyGridView'>No Sites Found</div>");
        }

        lbl_TotalWebServices.Text = "<span class='font-bold pad-right-sml'>Total Sites:</span>" + fileCount.ToString();
        pnl_CustomPageList.Controls.Add(new LiteralControl(str.ToString()));
    }
    private void BuildDefaultPageList(string defaultPage, string folder) {
        if (!CustomProjects.IsFtpFolder(folder)) {
            List<string> entries = customProjects.BuildEntryNameListByFolder(folder);
            foreach (string entry in entries) {
                FileInfo fi = new FileInfo(entry);
                string fileExt = fi.Extension;
                if ((fileExt.ToLower() != ".aspx") && (fileExt.ToLower() != ".html") && (fileExt.ToLower() != ".php") && (fileExt.ToLower() != ".asp") && (fileExt.ToLower() != ".htm") && (fileExt.ToLower() != ".xhtml")
                    && (fileExt.ToLower() != ".jhtml") && (fileExt.ToLower() != ".php4") && (fileExt.ToLower() != ".php3") && (fileExt.ToLower() != ".phtml") && (fileExt.ToLower() != ".xml") && (fileExt.ToLower() != ".rss")) {
                    continue;
                }

                ListItem item = new ListItem(entry, entry);
                if (!radioButton_FileList.Items.Contains(item)) {
                    radioButton_FileList.Items.Add(item);
                }
            }

            for (int i = 0; i < radioButton_FileList.Items.Count; i++) {
                if (radioButton_FileList.Items[i].Value == defaultPage) {
                    radioButton_FileList.Items[i].Selected = true;
                    break;
                }
            }
        }
    }
    protected void lbtn_refresh_Clicked(object sender, EventArgs e) {
        BuildTable();
    }


    #region Edit Buttons

    protected void hf_DeleteCP_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteCP.Value)) {

            bool canDelete = true;
            CustomProjects_Coll cpColl = customProjects.GetEntry(hf_DeleteCP.Value);
            string folder = cpColl.Folder;

            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (cpColl.UploadedBy.ToLower() != _username.ToLower()) {
                    canDelete = false;
                }
            }

            if (canDelete) {
                customProjects.DeleteRow(hf_DeleteCP.Value);

                if (!CustomProjects.IsFtpFolder(cpColl.Folder)) {
                    CustomProjectDLLs dlls = new CustomProjectDLLs();
                    dlls.DeleteItem(folder);
                    try {
                        // Delete the dll folder
                        ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + folder);

                        if (!string.IsNullOrEmpty(folder)) {
                            if (Directory.Exists(ServerSettings.GetServerMapLocation + "Bin\\" + folder)) {
                                Directory.Delete(ServerSettings.GetServerMapLocation + "Bin\\" + folder, true);
                            }

                            // Delete the custom project folder
                            if (Directory.Exists(ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + folder)) {
                                Directory.Delete(ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + folder, true);

                                // Might need to check again
                                if (Directory.Exists(ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + folder)) {
                                    Directory.Delete(ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + folder, true);

                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        BuildTable();
        hf_DeleteCP.Value = string.Empty;
    }
    protected void hf_CancelCP_Changed(object sender, EventArgs e) {
        hf_CancelCP.Value = string.Empty;
        BuildTable();
    }
    protected void btn_updateDefaultFile_Clicked(object sender, EventArgs e) {
        string[] splitId = hf_defaultProjectID.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        if (splitId.Length > 0) {
            if (!string.IsNullOrEmpty(splitId[0])) {
                string fileName = radioButton_FileList.SelectedValue;
                customProjects.UpdateDefaultPage(splitId[0], fileName);

                if ((splitId.Length > 1) && (splitId[1].ToLower() == "newentry")) {
                    BuildTable();
                    hf_defaultProjectID.Value = string.Empty;
                }
            }
            RegisterPostbackScripts.RegisterStartupScript(this, "$('.loaderApp-overlay').fadeOut(150);");
        }
    }

    #endregion

    protected void btn_uploadFile_Clicked(object sender, EventArgs e) {
        string cpFolder = ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder;
        if (!Directory.Exists(cpFolder)) {
            try {
                Directory.CreateDirectory(cpFolder);
            }
            catch { }
        }

        string ProjectID= Guid.NewGuid().ToString();
        string pageFolder = HelperMethods.RandomString(10);

        if (fu_newWebServiceFile.HasFile) {
            FileInfo fi = new FileInfo(fu_newWebServiceFile.FileName);

            switch (fi.Extension.ToLower()) {
                #region ZIP FILES

                case ".zip":
                    if (HasAtLeastOneValidPage(fu_newWebServiceFile.FileContent)) {
                        fu_newWebServiceFile.FileContent.Position = 0;
                        using (Stream fileStreamIn = fu_newWebServiceFile.FileContent) {
                            using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)) {
                                ZipEntry entry;
                                string tmpEntry = String.Empty;

                                List<string> dlls = new List<string>();
                                while ((entry = zipInStream.GetNextEntry()) != null) {
                                    string fn = Path.GetFileName(entry.Name);

                                    string filePath = cpFolder + "\\" + pageFolder + "\\";
                                    if (!Directory.Exists(filePath)) {
                                        Directory.CreateDirectory(filePath);
                                    }

                                    if (string.IsNullOrEmpty(fn)) {
                                        continue;
                                    }

                                    var tempfi = new FileInfo(fn);
                                    if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                                         && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                                        if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                                            string en = entry.Name;

                                            if (tempfi.Extension.ToLower() == ".pdb") {
                                                continue;
                                            }

                                            if ((tempfi.Extension.ToLower() == ".dll") || (tempfi.Extension.ToLower() == ".compiled")) {
                                                filePath = ServerSettings.GetServerMapLocation + "Bin\\" + pageFolder + "\\";
                                                if (!Directory.Exists(filePath)) {
                                                    Directory.CreateDirectory(filePath);
                                                    ServerSettings.AddRuntimeAssemblyBinding("Bin\\" + pageFolder);
                                                }

                                                en = tempfi.Name;
                                                dlls.Add(en);
                                            }
                                            else {
                                                if (!HelperMethods.IsValidCustomProjectFormat(tempfi.Extension)) {
                                                    continue;
                                                }
                                            }

                                            FileInfo fnTemp = new FileInfo(en);
                                            string tempPath = en.Replace(fnTemp.Name, "").Replace("/", "\\");
                                            if (!Directory.Exists(filePath + tempPath)) {
                                                Directory.CreateDirectory(filePath + tempPath);
                                            }

                                            string fullPath = filePath + en;
                                            fullPath = fullPath.Replace("\\ ", "\\").Replace("/", "\\");

                                            FileStream streamWriter = File.Create(fullPath);
                                            int size = 2048;
                                            byte[] data = new byte[2048];
                                            while (true) {
                                                size = zipInStream.Read(data, 0, data.Length);
                                                if (size > 0)
                                                    streamWriter.Write(data, 0, size);
                                                else
                                                    break;
                                            }
                                            streamWriter.Close();
                                        }
                                    }
                                }

                                customProjects.AddItem(ProjectID, pageFolder, tb_descriptionUpload.Text.Trim(), txt_UploadName.Text.Trim(), "");
                                if (dlls.Count > 0) {
                                    CustomProjectDLLs cpDLLs = new CustomProjectDLLs();
                                    cpDLLs.AddItem(pageFolder);
                                }
                            }
                        }
                    }
                    else {
                        ServerSettings.PageToolViewRedirect(this.Page, "CustomProjects.aspx?error=couldnotupload");
                    }
                    break;
                #endregion

                default:
                    if (HelperMethods.IsValidCustomProjectFormat(fi.Extension)) {
                        customProjects.AddItem(ProjectID, pageFolder, tb_descriptionUpload.Text.Trim(), txt_UploadName.Text.Trim(), fu_newWebServiceFile.FileName);
                        try {
                            if (!Directory.Exists(cpFolder + "\\" + pageFolder)) {
                                Directory.CreateDirectory(cpFolder + "\\" + pageFolder);
                            }
                        }
                        catch { }
                        fu_newWebServiceFile.SaveAs(cpFolder + "\\" + pageFolder + "\\" + fu_newWebServiceFile.FileName);
                    }
                    else {
                        ServerSettings.PageToolViewRedirect(this.Page, "CustomProjects.aspx?error=invalidformat");
                    }

                    break;
            }

        }
        else {
            ServerSettings.PageToolViewRedirect(this.Page, "CustomProjects.aspx?error=couldnotupload");
        }

        ServerSettings.PageToolViewRedirect(this.Page, "CustomProjects.aspx?ProjectID=" + ProjectID);
    }

    private bool HasAtLeastOneValidPage(Stream str) {
        bool returnVal = false;
        ZipInputStream zipInStream = new ZipInputStream(str);
        ZipEntry entry;
        while ((entry = zipInStream.GetNextEntry()) != null) {
            string fn = Path.GetFileName(entry.Name);
            if (string.IsNullOrEmpty(fn)) {
                continue;
            }

            var tempfi = new FileInfo(fn);
            if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                 && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                    string en = entry.Name;

                    if (tempfi.Extension.ToLower() == ".pdb") {
                        continue;
                    }

                    if (HelperMethods.IsValidCustomProjectFormat(tempfi.Extension)) {
                        returnVal = true;
                        break;
                    }
                    else {
                        continue;
                    }
                }
            }
        }

        zipInStream.Flush();

        return returnVal;
    }

    protected void DownloadProject(object sender, EventArgs e) {
        CustomProjects_Coll page = customProjects.GetEntry(hf_download.Value);

        hf_download.Value = string.Empty;

        if (page != null) {
            DownloadFiles(page);
        }
    }
    private void DownloadFiles(CustomProjects_Coll page) {
        if (!CustomProjects.IsFtpFolder(page.Folder)) {
            dfl = new List<string>();
            string mainFilePath = ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + page.Folder;
            DirectoryInfo di = new DirectoryInfo(mainFilePath);

            BuildDownloadFileList(di.FullName, di.Name);

            Response.ContentType = "application/zip";
            // If the browser is receiving a mangled zipfile, IIS Compression may cause this problem. Some members have found that
            //    Response.ContentType = "application/octet-stream"     has solved this. May be specific to Internet Explorer.

            Response.AppendHeader("content-disposition", "attachment; filename=\"" + page.UploadName.Replace(" ", "_") + ".zip\"");
            Response.CacheControl = "Private";
            Response.Cache.SetExpires(DateTime.Now.AddMinutes(3)); // or put a timestamp in the filename in the content-disposition

            try {
                byte[] buffer = new byte[4096];

                using (ZipOutputStream zipOutputStream = new ZipOutputStream(HttpContext.Current.Response.OutputStream)) {
                    zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    foreach (string fileName in dfl) {
                        string fStream = ServerSettings.GetServerMapLocation + CustomProjects.customPageFolder + "\\" + fileName.Replace("/", "\\");
                        using (FileStream fs = File.OpenRead(fStream))    // or any suitable inputstream
                        {
                            string tempFileName = fileName.Replace(di.Name + "/", "");
                            ZipEntry entry = new ZipEntry(ZipEntry.CleanName(tempFileName));
                            entry.Size = fs.Length;
                            // Setting the Size provides WinXP built-in extractor compatibility,
                            //  but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.

                            zipOutputStream.PutNextEntry(entry);

                            int count = fs.Read(buffer, 0, buffer.Length);
                            while (count > 0) {
                                zipOutputStream.Write(buffer, 0, count);
                                count = fs.Read(buffer, 0, buffer.Length);
                                if (!HttpContext.Current.Response.IsClientConnected)
                                    break;
                            }
                        }
                    }
                }
                dfl.Clear();

                Response.Flush();
                Response.End();

                // Response.Redirect("~/SiteSettings/CustomContent/CustomProjects.aspx");
            }
            catch (Exception ex) {
            }
        }
    }

    private List<string> dfl;
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
}