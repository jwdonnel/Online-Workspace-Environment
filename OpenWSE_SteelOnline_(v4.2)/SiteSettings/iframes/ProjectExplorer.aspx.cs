using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

public partial class SiteSettings_ProjectExplorer : System.Web.UI.Page {

    #region Variables

    private ServerSettings _ss = new ServerSettings();
    private readonly AppLog _applog = new AppLog(false);
    private IIdentity _userId;
    private string _username;
    private string _ctrlname;
    private MemberDatabase _member;
    private string _sitetheme = "Standard";
    private CustomProjects customProjects;
    private string errorMessage = string.Empty;

    #endregion


    #region Viewstates

    private List<FileExplorerList> PageList {
        get {
            if (ViewState["PageList"] == null) {
                ViewState["PageList"] = new List<FileExplorerList>();
                return new List<FileExplorerList>();
            }

            return ViewState["PageList"] as List<FileExplorerList>;
        }
        set {
            ViewState["PageList"] = value;
        }
    }
    private string DefaultDir {
        get {
            if (ViewState["DefaultDir"] == null) {
                ViewState["DefaultDir"] = string.Empty;
                return string.Empty;
            }

            string temp = Convert.ToString(ViewState["DefaultDir"]);
            if (temp.Length > 0) {
                if (!CustomProjects.IsFtpFolder(temp)) {
                    if (temp[temp.Length - 1] != '\\') {
                        temp += "\\";
                    }
                }
                else {
                    if (temp[temp.Length - 1] != '/') {
                        temp += "/";
                    }
                }
            }

            return temp;
        }
        set {
            ViewState["DefaultDir"] = value;
        }
    }
    private string CurrentProjectID {
        get {
            if (ViewState["CurrentProjectID"] == null) {
                ViewState["CurrentProjectID"] = string.Empty;
                return string.Empty;
            }

            return Convert.ToString(ViewState["CurrentProjectID"]);
        }
        set {
            ViewState["CurrentProjectID"] = value;
        }
    }
    private FTPActions FtpActions {
        get {
            if (ViewState["FtpActions"] == null) {
                return new FTPActions(DefaultDir, string.Empty, string.Empty);
            }

            return (ViewState["FtpActions"] as FTPActions);
        }
        set {
            ViewState["FtpActions"] = value;
        }
    }

    #endregion


    #region Page Loading

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userID = HttpContext.Current.User.Identity;

        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userID, IsPostBack, _ss.NoLoginRequired);
        if (pageLoadInit.CanLoadPage) {
            ScriptManager sm = ScriptManager.GetCurrent(Page);
            if (sm != null) {
                string ctlId = sm.AsyncPostBackSourceElementID;
                _ctrlname = ctlId;
            }

            StartUpPage(userID);
            BuildSavedPages();

            #region AjaxUpload

            if (AjaxFileUpload1.IsInFileUploadPostBack) {
                // do for ajax file upload partial postback request
            }
            else {
                // do for normal page request

                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["preview"]) && !string.IsNullOrEmpty(Request.QueryString["fileId"])) {
                    var fileId = Request.QueryString["fileId"];
                    string fileContentType = null;
                    byte[] fileContents = null;

                    if (AjaxFileUpload1.StoreToAzure) {

                    #if NET45 || NET40
                    using (var stream = new MemoryStream())
                    {
                        AjaxFileUploadBlobInfo blobInfo;
                        AjaxFileUploadAzureHelper.DownloadStream(Request.QueryString["uri"], stream, out blobInfo);

                        fileContentType = blobInfo.Extension;
                        fileContents = stream.ToArray();
                    }
                    #endif

                    }
                    else {
                        fileContents = (byte[])Session["fileContents_" + fileId];
                        fileContentType = (string)Session["fileContentType_" + fileId];
                    }

                    if (fileContents != null) {
                        Response.Clear();
                        Response.ContentType = fileContentType;
                        Response.BinaryWrite(fileContents);
                        Response.End();
                    }

                }
            }

            #endregion
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }
    private void StartUpPage(IIdentity userId) {
        _userId = userId;
        _username = userId.Name;
        if (ServerSettings.AdminPagesCheck("CustomProjects", userId.Name)) {
            _member = new MemberDatabase(_username);
            if (!IsPostBack) {
                TryLoadSavedPage();
                LoadUserBackground();
                GetStartupScripts_JS();
            }
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }
    private void LoadUserBackground() {
        string str = AcctSettings.LoadUserBackground(_username, _sitetheme, this.Page);
        if (!string.IsNullOrEmpty(str)) {
            RegisterPostbackScripts.RegisterStartupScript(this, str);
        }
    }

    #endregion


    #region Load Saved

    private void TryLoadSavedPage() {
        string projectId = CurrentProjectID;
        if (Request["projectId"] != null) {
            projectId = Request["projectId"];
        }

        customProjects = new CustomProjects(_username);
        if (!string.IsNullOrEmpty(projectId)) {
            CustomProjects_Coll coll = customProjects.GetEntry(projectId);
            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UploadedBy.ToLower() == _username.ToLower()) {
                pnl_nosavedproject.Visible = false;
                pnl_nosavedproject.Enabled = false;
                pnl_pageControls.Visible = true;
                pnl_pageControls.Enabled = true;

                tb_projectName.Text = coll.UploadName;
                tb_description.Text = coll.Description;

                if (CustomProjects.IsFtpFolder(coll.Folder)) {
                    DefaultDir = coll.Folder;
                }
                else {
                    string tempDir = ServerSettings.GetServerMapLocation.Replace("/", "\\");
                    DefaultDir = tempDir + "CustomProjects" + GetBackBrackets + coll.Folder;
                }

                CurrentProjectID = projectId;

                BuildFileList(DefaultDir);
                BuildDefaultPageDDL(coll.DefaultPage);
            }
        }
        else {
            pnl_nosavedproject.Visible = true;
            pnl_nosavedproject.Enabled = true;
            pnl_pageControls.Visible = false;
            pnl_pageControls.Enabled = false;
        }
    }
    private void BuildSavedPages() {
        pnl_pages.Controls.Clear();

        StringBuilder str = new StringBuilder();
        SortPageList();
        if ((PageList != null) && (PageList.Count > 0)) {
            str.Append("<ul class='page-list' style='list-style: none;'>");
            foreach (FileExplorerList page in PageList) {
                if (page.File_Info == null) {
                    string tempDefaultDir = DefaultDir;
                    string diPath = page.Directory_Info.FullName;

                    if (CustomProjects.IsFtpFolder(DefaultDir)) {
                        diPath = diPath.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir).Replace("//", "\\"));
                        tempDefaultDir = tempDefaultDir.Replace("/", "\\").Replace("\\\\", "\\");
                    }

                    string folderPath = diPath.Replace(tempDefaultDir, "");
                    folderPath = folderPath.Replace("\\", "/");

                    if (folderPath.Length > 0 && folderPath[folderPath.Length - 1] == '/') {
                        folderPath = folderPath.Remove(folderPath.Length - 1);
                    }

                    str.Append(string.Format("<li folder='{0}' type='folder' style='display: none;'><a id='{1}' href='#' onclick='LoadSavedPageFolder(this);return false;'><div class='img-folder float-left margin-right'></div>{2}</a></li>", folderPath, page.ID, page.Directory_Info.Name));
                }
                else {
                    string classFolder = page.File_Info.FullName.Replace(page.File_Info.Name, "").Replace(DefaultDir, "").Replace("\\", "/");

                    if (CustomProjects.IsFtpFolder(DefaultDir)) {
                        classFolder = classFolder.Replace(ServerSettings.SystemFilePathPrefix.Replace("\\", "/"), FTPActions.GetFTPPrefix(DefaultDir));
                        classFolder = classFolder.Replace(DefaultDir, "");
                    }

                    if (classFolder.Length > 0 && classFolder[classFolder.Length - 1] == '/') {
                        classFolder = classFolder.Remove(classFolder.Length - 1);
                    }

                    string imgClass = "img-file";
                    if (HelperMethods.IsImageFileType(page.File_Info.Extension)) {
                        imgClass = "img-imgFile";
                    }

                    str.Append(string.Format("<li folder='{1}' type='file' style='display: none;'><a href='#' id='{0}' onclick='LoadSavedPage(this);return false;'><div class='" + imgClass + " float-left margin-right'></div>{2}</a></li>", page.ID, classFolder, page.Filename));
                }
            }
            str.Append("</ul>");
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("No Pages Found");
        }

        pnl_pages.Controls.Add(new LiteralControl(str.ToString()));
        RegisterPostbackScripts.RegisterStartupScript(this, "AddEditControlsToPageList();");
    }
    private void ReloadList() {
        customProjects = new CustomProjects(_username);
        string projectId = CurrentProjectID;
        if (Request["projectId"] != null) {
            projectId = Request["projectId"];
        }

        if (!string.IsNullOrEmpty(projectId)) {
            CustomProjects_Coll coll = customProjects.GetEntry(projectId);
            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UploadedBy.ToLower() == _username.ToLower()) {
                BuildDefaultPageDDL(coll.DefaultPage);
            }
        }
    }
    private void BuildFileList(string folder) {
        if (CustomProjects.IsFtpFolder(folder)) {
            if (!FtpActions.TryConnect(out errorMessage)) {
                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + folder + "');");
            }
            else {
                PageList = FtpActions.GetListOfFilesAndDir(DefaultDir);
            }
        }
        else {
            if (Directory.Exists(folder)) {
                string[] files = Directory.GetFiles(folder);
                foreach (string file in files) {
                    FileInfo fi = new FileInfo(file);
                    if (HelperMethods.IsValidCustomProjectFormat(fi.Extension) || HelperMethods.IsImageFileType(fi.Extension)) {
                        FileExplorerList editor = new FileExplorerList(Guid.NewGuid().ToString(), fi.Name, fi, fi.Directory);
                        if (!PageList.Contains(editor)) {
                            PageList.Add(editor);
                        }
                    }
                }

                string[] dirs = Directory.GetDirectories(folder);
                foreach (string dir in dirs) {
                    FileExplorerList editor = new FileExplorerList(Guid.NewGuid().ToString(), "", null, new DirectoryInfo(dir));
                    if (!PageList.Contains(editor)) {
                        PageList.Add(editor);
                    }

                    BuildFileList(dir);
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('The folder and/or files were not found.');");
            }
        }
    }
    private void BuildDefaultPageDDL(string defaultPage) {
        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            pnl_defaultPage.Enabled = true;
            pnl_defaultPage.Visible = true;

            ddl_defaultPage.Items.Clear();
            SortPageList();
            if (PageList != null) {
                bool hasDefaultPage = false;

                ListItem itemTemp = new ListItem("-- Select Default Page --", "");
                ddl_defaultPage.Items.Add(itemTemp);

                foreach (FileExplorerList page in PageList) {
                    if ((page.File_Info != null) && (HelperMethods.IsValidDefaultPage(page.File_Info.Extension))) {
                        ListItem item = new ListItem(page.Filename, page.ID);
                        string pageName = page.File_Info.FullName.Replace(DefaultDir + "\\", "");
                        if (pageName.ToLower() == defaultPage.ToLower()) {
                            string tempUrl = page.File_Info.FullName.Replace(ServerSettings.GetServerMapLocation.Replace("/", "\\"), "").Replace("\\", "/");
                            if (tempUrl[0] != '/') {
                                tempUrl = "/" + tempUrl;
                            }

                            string pageLoc = ResolveUrl("~" + tempUrl);
                            ltl_previewButton.Text = string.Format("<div class='clear-space'></div><a href='{0}' target='_blank' class='float-right margin-right'><span class='td-view-btn float-left margin-right-sml' style='padding: 0px;'></span>Preview Site</a><div class='clear-space'></div>", pageLoc);
                            item.Selected = true;
                            hasDefaultPage = true;
                        }

                        if (!ddl_defaultPage.Items.Contains(item)) {
                            ddl_defaultPage.Items.Add(item);
                        }
                    }
                }

                if (!hasDefaultPage) {
                    ltl_previewButton.Text = string.Empty;
                    customProjects = new CustomProjects(_username);
                    customProjects.UpdateDefaultPage(CurrentProjectID, string.Empty);
                }
            }
        }
        else {
            pnl_defaultPage.Enabled = false;
            pnl_defaultPage.Visible = false;
        }
    }
    private void SortPageList() {
        if ((PageList != null) && (PageList.Count > 1)) {
            PageList.Sort(delegate(FileExplorerList c1, FileExplorerList c2) { return c1.Filename.CompareTo(c2.Filename); });
        }
    }

    #endregion


    #region Dynamically Load Scripts

    private void GetStartupScripts_JS() {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            if (coll.ApplyTo == "All Components") {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }

    #endregion


    #region Page Buttons

    protected void btn_ftpLogin_Click(object sender, EventArgs e) {
        ltl_ftpLogin.Text = string.Empty;
        string errorMessage = string.Empty;

        FtpActions = new FTPActions(DefaultDir, tb_ftpUsername.Text.Trim(), tb_ftpPassword.Text.Trim());
        if (FtpActions.TryConnect(out errorMessage)) {
            BuildFileList(DefaultDir);
            BuildSavedPages();
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#MessageActivationPopup').hide();");
        }
        else {
            ltl_ftpLogin.Text = "<div class='clear-space'></div><span style='color: Red;'>Invalid Username/Password</span>";
            tb_ftpPassword.Text = string.Empty;
        }

        updatepnl_FtpLogin.Update();
    }

    protected void lbtn_addNewPage_Click(object sender, EventArgs e) {
        try {
            string currFolder = GetCurrentDirectory();

            List<string> fileList = new List<string>();
            if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                fileList = Directory.GetFiles(DefaultDir + currFolder).ToList();
            }
            else {
                fileList = FtpActions.GetDirList(DefaultDir + currFolder);
            }

            int fileCount = 0;
            for (int i = 0; i < fileList.Count; i++) {
                FileInfo tempFi = new FileInfo(fileList[i]);
                if (tempFi.Name.ToLower() == "new_page.html" || tempFi.Name.ToLower() == "new_page(" + fileCount.ToString() + ").html") {
                    fileCount++;
                }
            }

            string newPageName = "New_Page.html";
            if (fileCount > 0) {
                newPageName = "New_Page(" + fileCount.ToString() + ").html";
            }

            string filename = DefaultDir + currFolder + newPageName;

            string newLine = Environment.NewLine;
            string indent = "    ";

            StringBuilder strNewFile = new StringBuilder();
            strNewFile.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + newLine);
            strNewFile.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">" + newLine);
            strNewFile.Append("<head>" + newLine);
            strNewFile.Append(indent + "<title></title>" + newLine);
            strNewFile.Append("</head>" + newLine);
            strNewFile.Append("<body>");
            strNewFile.Append(newLine + newLine);
            strNewFile.Append("</body>");

            FileInfo fi = null;

            if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                File.WriteAllText(filename, strNewFile.ToString());
                fi = new FileInfo(filename);
            }
            else {
                fi = new FileInfo(filename.Replace("ftp:/", "").Replace("ftps:/", ""));
                byte[] fileContents = Encoding.UTF8.GetBytes(strNewFile.ToString());
                if (!FtpActions.UploadFiles(filename, fileContents)) {
                    if (!FtpActions.TryConnect(out errorMessage)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                    }
                }
            }

            FileExplorerList editor = new FileExplorerList(Guid.NewGuid().ToString(), fi.Name, fi, fi.Directory);
            PageList.Add(editor);

            string tempUrl = fi.FullName.Replace(ServerSettings.GetServerMapLocation.Replace("/", "\\"), "").Replace("\\", "/");
            if (tempUrl[0] != '/') {
                tempUrl = "/" + tempUrl;
            }

            StringBuilder str = new StringBuilder();
            string url = ResolveUrl("~" + tempUrl);
            if (CustomProjects.IsFtpFolder(DefaultDir)) {
                url = fi.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                url += "/" + fi.Name;
            }

            str.Append("$('#lbtn_previewPage').attr('href', '" + url + "'); currSelected = '" + editor.ID + "';");
            str.Append("LoadEditor(" + HttpUtility.JavaScriptStringEncode(strNewFile.ToString(), true) + ");");
            str.Append("$('#selectpagehint').hide();$('#imgEditor').hide();$('#htmlEditor').show();");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }
        catch (Exception ex) {
            _applog.AddError(ex);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an error creating your new page. Page was not added.');");
        }

        ReloadList();
        BuildSavedPages();
    }
    protected void lbtn_addNewFolder_Click(object sender, EventArgs e) {
        try {
            string currFolder = GetCurrentDirectory();

            List<string> dirList = new List<string>();
            if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                dirList = Directory.GetDirectories(DefaultDir + currFolder).ToList();
            }
            else {
                dirList = FtpActions.GetDirList(DefaultDir + currFolder);
            }

            int folderCount = 0;
            for (int i = 0; i < dirList.Count; i++) {
                DirectoryInfo tempDi = new DirectoryInfo(dirList[i]);
                if (tempDi.Name.ToLower() == "new_folder" || tempDi.Name.ToLower() == "new_folder(" + folderCount.ToString() + ")") {
                    folderCount++;
                }
            }

            string newFolderName = "New_Folder";
            if (folderCount > 0) {
                newFolderName = "New_Folder(" + folderCount.ToString() + ")";
            }

            string folderName = DefaultDir + currFolder + newFolderName;

            DirectoryInfo di = null;
            if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                Directory.CreateDirectory(folderName);
                di = new DirectoryInfo(folderName);
            }
            else {
                di = new DirectoryInfo(folderName.Replace("ftp:/", "").Replace("ftps:/", ""));
                if (!FtpActions.CreateNewFolder(folderName)) {
                    if (!FtpActions.TryConnect(out errorMessage)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                    }
                }
            }

            FileExplorerList editor = new FileExplorerList(Guid.NewGuid().ToString(), "", null, di);
            PageList.Add(editor);
        }
        catch (Exception ex) {
            _applog.AddError(ex);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an error creating your new folder. Folder was not added.');");
        }
        ReloadList();
        BuildSavedPages();
    }

    protected void btn_saveProject_Click(object sender, EventArgs e) {
        string defaultPage = ddl_defaultPage.SelectedValue;
        string projectName = tb_projectName.Text.Trim();
        string description = tb_description.Text.Trim();

        if (!string.IsNullOrEmpty(projectName)) {
            if (CustomProjects.IsFtpFolder(DefaultDir)) {
                defaultPage = string.Empty;
            }
            else {
                if (!string.IsNullOrEmpty(defaultPage)) {
                    FileExplorerList dpage = PageList.Find(x => x.ID == defaultPage);
                    if (dpage != null) {
                        defaultPage = dpage.File_Info.FullName.Replace(DefaultDir + "\\", "");
                    }
                }
            }

            customProjects = new CustomProjects(_username);
            if (!string.IsNullOrEmpty(CurrentProjectID)) {
                customProjects.UpdateRow(CurrentProjectID, description, projectName);
                if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                    customProjects.UpdateDefaultPage(CurrentProjectID, defaultPage);
                    BuildDefaultPageDDL(defaultPage);
                }
            }
            else if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                CurrentProjectID = Guid.NewGuid().ToString();

                string tempPath = ServerSettings.GetServerMapLocation.Replace("/", "\\");
                if (tempPath[tempPath.Length - 1] != '\\') {
                    tempPath += "\\";
                }

                string cpFolder = tempPath + "\\" + CustomProjects.customPageFolder;
                if (!Directory.Exists(cpFolder)) {
                    try {
                        Directory.CreateDirectory(cpFolder);
                    }
                    catch { }
                }

                string pageFolder = HelperMethods.RandomString(10);
                try {
                    if (!Directory.Exists(cpFolder + "\\" + pageFolder)) {
                        Directory.CreateDirectory(cpFolder + "\\" + pageFolder);
                    }
                }
                catch { }

                customProjects.AddItem(CurrentProjectID, pageFolder, description, projectName, "");
                TryLoadSavedPage();
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Project name cannot be blank.');");
        }
    }
    protected void hf_LoadSavedPage_ValueChanged(object sender, EventArgs e) {
        string id = hf_LoadSavedPage.Value.Trim();
        if (PageList == null) {
            hf_LoadSavedPage.Value = string.Empty;
            return;
        }

        FileExplorerList page = PageList.Find(x => x.ID == id);
        StringBuilder str = new StringBuilder();

        if (page != null) {
            if (CustomProjects.IsFtpFolder(DefaultDir)) {
                string loc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                loc += "/" + page.Filename;

                byte[] fileBytes = FtpActions.GetFileContents(loc, out errorMessage);

                if (HelperMethods.IsImageFileType(page.File_Info.Extension)) {
                    str.Append("$('#imgEditor').attr('src', 'data:image/" + page.File_Info.Extension.Replace(".", "").ToLower() + ";base64," + Convert.ToBase64String(fileBytes) + "');");
                    str.Append("$('#selectpagehint').hide();$('#htmlEditor').hide();$('#imgEditor').show();");
                }
                else {
                    if (!string.IsNullOrEmpty(errorMessage)) {
                        if (!FtpActions.TryConnect(out errorMessage)) {
                            RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                        }
                    }

                    string ftpContents = Encoding.Default.GetString(fileBytes);
                    str.Append("$('#lbtn_previewPage').attr('href', '" + loc + "');");
                    str.Append("LoadEditor(" + HttpUtility.JavaScriptStringEncode(ftpContents, true) + ");");
                    str.Append("$('#selectpagehint').hide();$('#imgEditor').hide();$('#htmlEditor').show();");
                }
            }
            else if (File.Exists(page.File_Info.FullName)) {
                string tempUrl = page.File_Info.FullName.Replace(ServerSettings.GetServerMapLocation.Replace("/", "\\"), "").Replace("\\", "/");
                if (tempUrl[0] != '/') {
                    tempUrl = "/" + tempUrl;
                }

                string url = ResolveUrl("~" + tempUrl);
                if (HelperMethods.IsImageFileType(page.File_Info.Extension)) {
                    str.Append("$('#imgEditor').attr('src', '" + url + "');");
                    str.Append("$('#selectpagehint').hide();$('#htmlEditor').hide();$('#imgEditor').show();");
                }
                else if (HelperMethods.IsValidCustomProjectFormat(page.File_Info.Extension)) {
                    string fileText = File.ReadAllText(page.File_Info.FullName);
                    str.Append("$('#lbtn_previewPage').attr('href', '" + url + "');");
                    str.Append("LoadEditor(" + HttpUtility.JavaScriptStringEncode(fileText, true) + ");");
                    str.Append("$('#selectpagehint').hide();$('#imgEditor').hide();$('#htmlEditor').show();");
                }
            }
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }

        hf_LoadSavedPage.Value = string.Empty;
    }

    protected void hidden_editor_ValueChanged(object sender, EventArgs e) {
        string fileText = HttpUtility.UrlDecode(hidden_editor.Value);
        string id = hf_editorID.Value;
        if (!string.IsNullOrEmpty(id)) {
            FileExplorerList page = PageList.Find(x => x.ID == id);
            if (page != null) {
                try {
                    if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                        File.WriteAllText(page.File_Info.FullName, fileText);
                    }
                    else {
                        string loc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                        loc += "/" + page.Filename;
                        if (!FtpActions.DeleteFile(loc)) {
                            if (!FtpActions.TryConnect(out errorMessage)) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                            }
                        }
                        else if (!FtpActions.UploadFiles(loc, Encoding.Default.GetBytes(fileText))) {
                            if (!FtpActions.TryConnect(out errorMessage)) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + ex.Message + "');");
                }
            }
        }
        hidden_editor.Value = "";
    }

    protected void hf_deleteFile_ValueChanged(object sender, EventArgs e) {
        string id = hf_deleteFile.Value;
        if (!string.IsNullOrEmpty(id)) {
            FileExplorerList page = PageList.Find(x => x.ID == id);
            if (page != null) {
                try {
                    if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                        File.Delete(page.File_Info.FullName);
                    }
                    else {
                        string loc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                        if (loc[loc.Length - 1] != '/') {
                            loc += "/";
                        }

                        if (!FtpActions.DeleteFile(loc + page.Filename)) {
                            if (!FtpActions.TryConnect(out errorMessage)) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + ex.Message + "');");
                }
            }
        }
        hf_deleteFile.Value = string.Empty;

        PageList.Clear();

        TryLoadSavedPage();
        BuildSavedPages();
    }
    protected void hf_updatePageName_ValueChanged(object sender, EventArgs e) {
        string id = hf_editorID.Value;
        if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(hf_updatePageName.Value))) {
            FileExplorerList page = PageList.Find(x => x.ID == id);
            if (page != null) {
                try {
                    string fileName = FixName(hf_updatePageName.Value);
                    if (!FileNameExists(fileName, page) && fileName != page.File_Info.Name) {
                        FileInfo newfi = new FileInfo(fileName);

                        if ((HelperMethods.IsValidCustomProjectFormat(newfi.Extension)) || (HelperMethods.IsImageFileType(newfi.Extension))) {
                            if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                                string newName = page.File_Info.FullName.Replace(page.File_Info.Name, fileName);
                                File.Move(page.File_Info.FullName, newName);
                                File.Delete(page.File_Info.FullName);
                            }
                            else {
                                string loc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                                if (loc[loc.Length - 1] != '/') {
                                    loc += "/";
                                }

                                if (!FtpActions.RenameFileOrFolder(loc + page.Filename, fileName)) {
                                    if (!FtpActions.TryConnect(out errorMessage)) {
                                        RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                                    }
                                }
                            }

                            PageList.Clear();

                            TryLoadSavedPage();
                            BuildSavedPages();
                        }
                        else {
                            RegisterPostbackScripts.RegisterStartupScript(this, "EditPageName('" + id + "');openWSE.AlertWindow('Filename extension is invalid.');");
                        }
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "EditPageName('" + id + "');openWSE.AlertWindow('A file with the same name already exists.');");
                    }
                }
                catch (Exception ex) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "EditPageName('" + id + "');openWSE.AlertWindow('" + ex.Message + "');");
                }
            }
        }
        hf_updatePageName.Value = string.Empty;
        hf_editorID.Value = string.Empty;
    }
    protected void hf_moveFile_ValueChanged(object sender, EventArgs e) {
        try {
            string filePath = hf_moveFile.Value;

            FileExplorerList page = null;
            if (!string.IsNullOrEmpty(hf_editorID.Value)) {
                page = PageList.Find(x => x.ID == hf_editorID.Value);
            }

            if (filePath == "root") {
                filePath = string.Empty;
            }

            if (page != null) {
                if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                    if (!string.IsNullOrEmpty(filePath)) {
                        filePath = filePath.Replace("/", "\\");
                        if (filePath.Length > 0 && filePath[filePath.Length - 1] != '\\') {
                            filePath += "\\";
                        }
                    }

                    page.File_Info.CopyTo(DefaultDir + filePath + page.File_Info.Name, true);
                    page.File_Info.Delete();
                }
                else {
                    string loc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                    if (loc[loc.Length - 1] != '/') {
                        loc += "/";
                    }

                    byte[] fileBytes = FtpActions.GetFileContents(loc + page.File_Info.Name, out errorMessage);
                    if (string.IsNullOrEmpty(errorMessage)) {
                        if (filePath.Length > 0 && filePath[filePath.Length - 1] != '/') {
                            filePath += "/";
                        }

                        if (FtpActions.UploadFiles(DefaultDir + filePath + page.File_Info.Name, fileBytes)) {
                            FtpActions.DeleteFile(loc + page.File_Info.Name);
                        }
                    }
                }
            }
        }
        catch (Exception ex) {
            _applog.AddError(ex);
        }

        hf_moveFile.Value = string.Empty;
        hf_editorID.Value = string.Empty;

        PageList.Clear();
        TryLoadSavedPage();
        BuildSavedPages();
    }

    protected void hf_deleteFolder_ValueChanged(object sender, EventArgs e) {
        string id = hf_deleteFolder.Value;
        if (!string.IsNullOrEmpty(id)) {
            try {
                FileExplorerList page = PageList.Find(x => x.ID == id);
                if (page != null) {
                    if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                        if (page.Directory_Info.Exists) {
                            page.Directory_Info.Delete(true);
                        }
                    }
                    else {
                        string loc = page.Directory_Info.FullName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                        if (!FtpActions.DeleteFolder(loc)) {
                            if (!FtpActions.TryConnect(out errorMessage)) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + ex.Message + "');");
            }
        }
        hf_deleteFolder.Value = string.Empty;

        PageList.Clear();

        TryLoadSavedPage();
        BuildSavedPages();
    }
    protected void hf_updateFolderName_ValueChanged(object sender, EventArgs e) {
        string id = hf_editorID.Value;
        FileExplorerList page = PageList.Find(x => x.ID == id);
        if (page != null) {
            if (!string.IsNullOrEmpty(hf_updateFolderName.Value)) {
                string folderName = FixName(hf_updateFolderName.Value);
                if (!FolderNameExists(folderName, page) && folderName != page.Directory_Info.Name) {
                    if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                        page.Directory_Info.MoveTo(page.Directory_Info.Parent.FullName + "\\" + folderName);
                    }
                    else {
                        string loc = page.Directory_Info.Parent.FullName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                        if (loc[loc.Length - 1] != '/') {
                            loc += "/";
                        }

                        if (!FtpActions.RenameFileOrFolder(loc + page.Directory_Info.Name, folderName)) {
                            if (!FtpActions.TryConnect(out errorMessage)) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                            }
                        }
                    }
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "EditFolderName('" + id + "');openWSE.AlertWindow('A folder with the same name already exists.');");
                }
            }
        }

        hf_updateFolderName.Value = string.Empty;
        hf_editorID.Value = string.Empty;

        PageList.Clear();

        TryLoadSavedPage();
        BuildSavedPages();
    }
    protected void hf_moveFolder_ValueChanged(object sender, EventArgs e) {
        try {
            string filePath = hf_moveFolder.Value;

            FileExplorerList page = null;
            if (!string.IsNullOrEmpty(hf_editorID.Value)) {
                page = PageList.Find(x => x.ID == hf_editorID.Value);
            }

            if (page != null) {
                if (filePath == "root") {
                    filePath = string.Empty;
                }

                if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                    if (!string.IsNullOrEmpty(filePath)) {
                        filePath = filePath.Replace("/", "\\");
                        if (filePath.Length > 0 && filePath[filePath.Length - 1] != '\\') {
                            filePath += "\\";
                        }
                    }

                    MoveDirectory(page.Directory_Info.FullName, DefaultDir + filePath);
                }
                else {
                    if (!string.IsNullOrEmpty(filePath)) {
                        if (filePath.Length > 0 && filePath[filePath.Length - 1] != '/') {
                            filePath += "/";
                        }
                    }

                    string loc = page.Directory_Info.FullName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
                    if (loc[loc.Length - 1] != '/') {
                        loc += "/";
                    }

                    MoveDirectory(loc, DefaultDir + filePath);
                }
            }
        }
        catch (Exception ex) {
            _applog.AddError(ex);
        }

        hf_moveFolder.Value = string.Empty;
        hf_editorID.Value = string.Empty;

        PageList.Clear();

        TryLoadSavedPage();
        BuildSavedPages();
    }

    protected void hfRefreshAllFilesAfterUpload_ValueChanged(object sender, EventArgs e) {
        hfRefreshAllFilesAfterUpload.Value = string.Empty;
        if (!string.IsNullOrEmpty(DefaultDir)) {
            if (Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] != null) {
                List<FileContentUpload> files = Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] as List<FileContentUpload>;
                foreach (FileContentUpload file in files) {
                    string currFolder = GetCurrentDirectory();

                    List<string> fileList = new List<string>();
                    if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                        fileList = Directory.GetFiles(DefaultDir + currFolder).ToList();
                    }
                    else {
                        fileList = FtpActions.GetDirList(DefaultDir + currFolder);
                    }

                    int fileCount = 0;
                    for (int i = 0; i < fileList.Count; i++) {
                        FileInfo tempFi = new FileInfo(fileList[i]);
                        if (!string.IsNullOrEmpty(tempFi.Extension)) {
                            if (tempFi.Name.ToLower() == file.FileName || tempFi.Name.ToLower() == file.FileName.Replace(tempFi.Extension, "") + "(" + fileCount.ToString() + ")" + tempFi.Extension) {
                                fileCount++;
                            }
                        }
                    }

                    string uploadFileName = file.FileName;
                    if (fileCount > 0) {
                        FileInfo tempInfo = new FileInfo(file.FileName);
                        uploadFileName = file.FileName.Replace(tempInfo.Extension, "") + "(" + fileCount.ToString() + ")" + tempInfo.Extension;
                    }

                    string filename = DefaultDir + currFolder + uploadFileName;
                    try {
                        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
                            File.WriteAllBytes(filename, file.FileBytes);
                        }
                        else {
                            if (!FtpActions.UploadFiles(filename, file.FileBytes)) {
                                if (!FtpActions.TryConnect(out errorMessage)) {
                                    RegisterPostbackScripts.RegisterStartupScript(this, "PromptFTPCredentials('" + DefaultDir + "');");
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            PageList.Clear();
            BuildFileList(DefaultDir);
            BuildSavedPages();
        }

        Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] = null;
    }

    #endregion


    #region Helper Methods

    private void MoveDirectory(string source, string target) {
        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            DirectoryInfo sourceDi = new DirectoryInfo(source);

            // Create the directory if it doesn't exist already
            if (!Directory.Exists(target + "\\" + sourceDi.Name)) {
                Directory.CreateDirectory(target + "\\" + sourceDi.Name);
            }

            MoveFilesAndDirectories(source, target + "\\" + sourceDi.Name);
            Directory.Delete(source);
        }
        else {
            string[] splitFolders = source.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            string name = splitFolders[splitFolders.Length - 1];

            if (target[target.Length - 1] != '/') {
                target += "/";
            }

            FtpActions.CreateNewFolder(target + name);
            MoveFilesAndDirectories(source, target + name);
            FtpActions.DeleteFolder(source);
        }
    }
    private void MoveFilesAndDirectories(string source, string target) {
        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            DirectoryInfo sourceDi = new DirectoryInfo(source);
            DirectoryInfo[] directories = sourceDi.GetDirectories();
            FileInfo[] files = sourceDi.GetFiles();
            foreach (DirectoryInfo dir in directories) {
                MoveDirectory(dir.FullName, target);
            }
            foreach (FileInfo file in files) {
                file.CopyTo(target + "\\" + file.Name, true);
                file.Delete();
            }
        }
        else {
            List<string> dirList = FtpActions.GetDirList(source);
            foreach (string dir in dirList) {
                FileInfo fi = new FileInfo(dir);
                if (string.IsNullOrEmpty(fi.Extension)) {
                    MoveDirectory(source + dir, target);
                }
                else {
                    byte[] fileBytes = FtpActions.GetFileContents(source + dir, out errorMessage);
                    if (string.IsNullOrEmpty(errorMessage)) {
                        if (target[target.Length - 1] != '/') {
                            target += "/";
                        }
                        FtpActions.UploadFiles(target + dir, fileBytes);
                    }
                }
            }
        }
    }

    private string GetCurrentDirectory() {
        string currFolder = hf_currFolder.Value.Trim();

        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            currFolder = currFolder.Replace("/", "\\");
        }

        if (!string.IsNullOrEmpty(currFolder)) {
            if (currFolder[currFolder.Length - 1].ToString() != GetBackBrackets) {
                currFolder = currFolder + GetBackBrackets;
            }
        }

        return currFolder;
    }

    private string FixName(string name) {
        char[] invalid = new char[] { ':', '"', '<', '>', '[', ']', ';', '/', '|', '\\', '?', '*' };
        foreach (char c in invalid) {
            name = name.Replace(c, '_');
        }

        return name;
    }
    private string GetBackBrackets {
        get {
            string backBracket = "\\";
            if (CustomProjects.IsFtpFolder(DefaultDir)) {
                backBracket = "/";
            }

            return backBracket;
        }
    }
    private bool FileNameExists(string fileName, FileExplorerList page) {
        bool fileExists = false;
        List<string> fileList = new List<string>();
        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            fileList = Directory.GetFiles(page.File_Info.FullName.Replace(page.File_Info.Name, "")).ToList();
        }
        else {
            string tempLoc = page.File_Info.DirectoryName.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
            fileList = FtpActions.GetDirList(tempLoc);
        }

        foreach (string file in fileList) {
            FileInfo fi = new FileInfo(file);
            if (fi.Name.ToLower() == fileName.ToLower()) {
                fileExists = true;
                break;
            }
        }

        return fileExists;
    }
    private bool FolderNameExists(string folderName, FileExplorerList page) {
        bool fileExists = false;

        List<string> folderList = new List<string>();
        if (!CustomProjects.IsFtpFolder(DefaultDir)) {
            folderList = Directory.GetDirectories(page.Directory_Info.FullName).ToList();
        }
        else {
            string tempLoc = DefaultDir.Replace(ServerSettings.SystemFilePathPrefix, FTPActions.GetFTPPrefix(DefaultDir)).Replace("\\", "/");
            folderList = FtpActions.GetDirList(tempLoc);
        }

        bool folderExists = false;
        foreach (string folder in folderList) {
            DirectoryInfo di = new DirectoryInfo(folder);
            if (di.Name.ToLower() == folderName.ToLower()) {
                folderExists = true;
                break;
            }
        }

        return fileExists;
    }

    #endregion


    #region FileUpload

    protected void AjaxFileUpload1_OnUploadComplete(object sender, AjaxFileUploadEventArgs file) {
        try {
            FileInfo fi = new FileInfo(file.FileName);
            if (HelperMethods.IsValidCustomProjectFormat(fi.Extension) || HelperMethods.IsImageFileType(fi.Extension)) {
                if (Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] == null) {
                    Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] = new List<FileContentUpload>();
                }

                FileContentUpload fileContents = new FileContentUpload(file.FileName, file.GetContents());
                (Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] as List<FileContentUpload>).Add(fileContents);
            }
        }
        catch (Exception e) {
            AppLog applog = new AppLog(false);
            applog.AddError(e);
        }
    }
    protected void AjaxFileUpload1_UploadCompleteAll(object sender, AjaxFileUploadCompleteAllEventArgs e) {
        var startedAt = (DateTime)Session["uploadTime"];
        var now = DateTime.Now;
        e.ServerArguments = new JavaScriptSerializer()
            .Serialize(new {
                duration = (now - startedAt).Seconds,
                time = DateTime.Now.ToShortTimeString()
            });
    }
    protected void AjaxFileUpload1_UploadStart(object sender, AjaxFileUploadStartEventArgs e) {
        var now = DateTime.Now;
        e.ServerArguments = now.ToShortTimeString();
        Session["uploadTime"] = now;

        Session[_username + "_" + CustomProjects.SessionName + "_FileUpload"] = new List<FileContentUpload>();
    }

    #endregion

}