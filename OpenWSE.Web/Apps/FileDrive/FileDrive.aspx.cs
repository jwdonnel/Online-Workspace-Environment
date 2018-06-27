#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_FileDrive : Page {

    #region Private Variables

    private const string app_id = "app-documents";
    private ServerSettings ss = new ServerSettings();
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly Groups _groups = new Groups();
    private FileDrive _fileDrive;
    private AppInitializer _appInitializer;
    private string _absolutePath {
        get {
            string tempPath = ServerSettings.GetSitePath(Request);

            if (tempPath.LastIndexOf('/') != tempPath.Length - 1) {
                tempPath += "/";
            }

            return tempPath;
        }
    }
    private string _CurrentSelectedGroup = "";

    #endregion


    #region Private Properties

    private string CurrentSelectedFolder {
        get {
            if (Session["CurrentSelectedFolder"] != null) {
                var foundItem = false;
                var tempVal = Session["CurrentSelectedFolder"].ToString();
                List<ListItem> itemList = GetFolderList(_CurrentSelectedGroup);
                foreach (ListItem item in itemList) {
                    if (item.Value == tempVal) {
                        foundItem = true;
                        break;
                    }
                }

                if (foundItem) {
                    return tempVal;
                }
            }

            return string.Empty;
        }
        set {
            Session["CurrentSelectedFolder"] = value;
        }
    }
    private string CurrentSelectedGroup {
        get {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_appInitializer.UserName)) {
                return GroupSessions.GetUserGroupSessionName(_appInitializer.UserName);
            }

            if (Session["CurrentSelectedGroup"] != null) {
                bool foundItem = false;
                string tempVal = Session["CurrentSelectedGroup"].ToString();
                List<string> gns = _appInitializer.memberDatabase.GroupList;
                foreach (string item in gns) {
                    if (item == tempVal) {
                        foundItem = true;
                        break;
                    }
                }

                if (foundItem) {
                    return tempVal;
                }
            }

            return string.Empty;
        }
        set {
            Session["CurrentSelectedGroup"] = value;
        }
    }
    private string CurrentSelectedFolder_Edit {
        get {
            if (Session["CurrentSelectedFolder_Edit"] != null) {
                return Session["CurrentSelectedFolder_Edit"].ToString();
            }

            return string.Empty;
        }
        set {
            Session["CurrentSelectedFolder_Edit"] = value;
        }
    }
    private string CurrentFolderID_Edit {
        get {
            if (Session["CurrentFolderID_Edit"] != null) {
                return Session["CurrentFolderID_Edit"].ToString();
            }

            return string.Empty;
        }
        set {
            Session["CurrentFolderID_Edit"] = value;
        }
    }
    private string CurrentSelectedGroup_Move {
        get {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_appInitializer.UserName)) {
                return GroupSessions.GetUserGroupSessionName(_appInitializer.UserName);
            }

            if (Session["CurrentSelectedGroup_Move"] != null) {
                return Session["CurrentSelectedGroup_Move"].ToString();
            }

            return string.Empty;
        }
        set {
            Session["CurrentSelectedGroup_Move"] = value;
        }
    }

    #endregion


    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }

        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            _fileDrive = new FileDrive(_appInitializer.UserName);
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_appInitializer.UserName)) {
                pnl_grouplist.Enabled = false;
                pnl_grouplist.Visible = false;
                pnl_moveToGroup.Enabled = false;
                pnl_moveToGroup.Visible = false;
            }
            else {
                pnl_grouplist.Enabled = true;
                pnl_grouplist.Visible = true;
                pnl_moveToGroup.Enabled = true;
                pnl_moveToGroup.Visible = true;
            }

            _CurrentSelectedGroup = CurrentSelectedGroup;
            if (!IsPostBack) {
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();
                _appInitializer.LoadDefaultScripts();
                _appInitializer.LoadCustomFonts();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, app_id, this.Page);
                aus.StartAutoUpdates();
                BuildFileTable();
            }
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
    }

    #endregion


    #region Build Table, Folders, Groups

    private void BuildFileTable() {
        string _CurrentSelectedFolder = CurrentSelectedFolder;

        LoadGroups();
        LoadFolders(_CurrentSelectedFolder);

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 3);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns(string.Empty, "25", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("<input type='checkbox' onchange='OnSelectAllCheckChanged(this);' />", "15", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "250", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Ext", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Size", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Uploaded", "150", false));

        if (_CurrentSelectedFolder == "-" || string.IsNullOrEmpty(_CurrentSelectedFolder) || !string.IsNullOrEmpty(hf_fileEdit.Value)) {
            headerColumns.Add(new TableBuilderHeaderColumns("Folder", "200", false));
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        if (string.IsNullOrEmpty(_CurrentSelectedFolder) || _CurrentSelectedFolder == "-") {
            _fileDrive.GetAllFiles(_CurrentSelectedGroup);
        }
        else {
            if (_CurrentSelectedFolder == _appInitializer.memberDatabase.UserId) {
                _fileDrive.GetPersonalFiles(_appInitializer.memberDatabase.UserId, _CurrentSelectedGroup);
            }
            else {
                _fileDrive.GetFilesByFolderName(_CurrentSelectedFolder, _CurrentSelectedGroup);
            }
        }

        foreach (FileDriveDocuments_Coll item in _fileDrive.documents_coll) {
            string[] groupList = _CurrentSelectedGroup.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.None);
            List<string> fileGroupList = item.GroupName.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.None).ToList();

            bool isInGroup = false;
            foreach (string _g in groupList) {
                if (fileGroupList.Contains(_g)) {
                    isInGroup = true;
                }
            }

            if (isInGroup) {
                int findext = item.FileName.LastIndexOf(item.FileExtension, System.StringComparison.Ordinal);
                string fileTitle = item.FileName;
                if (findext != -1) {
                    fileTitle = item.FileName.Remove(findext);
                }

                if (hf_fileEdit.Value == item.ID.ToString()) {
                    #region Edit Mode
                    fileTitle = fileTitle.Replace(" ", "!_!");

                    List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();
                    bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, GetFileImage(item.FileExtension), TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, string.Empty, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Name", "<input type='text' class='textEntry editFilename' data-value=\"" + HttpUtility.UrlEncode(fileTitle) + "\" style='width: 100%;' />", TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Ext", item.FileExtension, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Size", item.FileSize, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Date Uploaded", getTime(item.UploadDate), TableBuilderColumnAlignment.Left));

                    string selectFolder = "<select class='editFolder' data-value='" + item.Folder + "'>";
                    List<ListItem> itemList = GetFolderList(_CurrentSelectedGroup);
                    foreach (ListItem tempItem in itemList) {
                        selectFolder += "<option value='" + tempItem.Value + "'>" + tempItem.Text + "</option>";
                    }
                    selectFolder += "</select>";

                    bodyColumns.Add(new TableBuilderBodyColumnValues("Folder", selectFolder, TableBuilderColumnAlignment.Left));

                    string updateBtn = "<a class='td-update-btn' title='Update details' onclick=\"UpdateFile('" + item.ID + "', '" + item.FileExtension + "');return false;\"></a>";
                    string cancelBtn = "<a class='td-cancel-btn' title='Cancel' onclick=\"EditFile('CANCEL');return false;\"></a>";

                    tableBuilder.AddBodyRow(bodyColumns, updateBtn + cancelBtn, "data-id='" + item.ID + "'");
                    #endregion
                }
                else {
                    #region Normal Mode
                    var tempFi = new FileInfo(FileDrive.DocumentsFolder);
                    string path = _absolutePath + tempFi.Name + "/" + item.ID;
                    if (IsImageFileType(item.FileExtension)) {
                        string imgLink = "<a href='" + path + item.FileExtension + "' target='_blank' class='cursor-pointer' title='View Image'><img alt='preview' src='" + path + item.FileExtension + "' style='max-height: 100px; max-width: 100%;' /></a><div class='clear-space-five'></div>";
                        fileTitle = imgLink + fileTitle;
                    }
                    else if (IsAudioVideoFileType(item.FileExtension)) {
                        string tagType = "audio";
                        string mediaType = "audio/mpeg";
                        string height = "50";

                        string imagePath = _absolutePath + "App_Themes/" + _appInitializer.siteTheme + "/App/play.png";
                        string onClick = "onclick='PlaySong(this, \"" + tagType + "\", \"" + mediaType + "\", \"" + path + item.FileExtension + "\", \"" + height + "\");'";
                        string fn = "<div class='float-left audio-file' data-src='" + path + item.FileExtension + "'>" + fileTitle + "</div>";
                        fileTitle = fn + "<img id='" + item.ID + "' alt='Play_Stop' src='" + imagePath + "' title='Play/Stop' class='margin-left float-left cursor-pointer audio' " + onClick + " /><div class='equalizer-holder'></div>";
                    }

                    string checkboxSelect = string.Empty;
                    if (string.IsNullOrEmpty(hf_fileEdit.Value)) {
                        checkboxSelect = "<input type='checkbox' data-id='" + item.ID + "' class='cb-movefile' onchange='OnFileCheckedChange(this);' />";
                    }

                    List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();
                    bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, GetFileImage(item.FileExtension), TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, checkboxSelect, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Name", fileTitle, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Ext", item.FileExtension, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Size", item.FileSize, TableBuilderColumnAlignment.Left));
                    bodyColumns.Add(new TableBuilderBodyColumnValues("Date Uploaded", getTime(item.UploadDate), TableBuilderColumnAlignment.Left));

                    if (_CurrentSelectedFolder == "-" || string.IsNullOrEmpty(_CurrentSelectedFolder) || !string.IsNullOrEmpty(hf_fileEdit.Value)) {
                        string folderName = item.Folder;
                        Guid tempGuid = new Guid();
                        if (Guid.TryParse(folderName, out tempGuid)) {
                            folderName = _fileDrive.GetFolderbyID(tempGuid);
                            if (item.Folder == _appInitializer.memberDatabase.UserId) {
                                folderName = "Personal Folder";
                            }
                        }
                        bodyColumns.Add(new TableBuilderBodyColumnValues("Folder", folderName, TableBuilderColumnAlignment.Left));
                    }

                    string editBtn = string.Empty;
                    string downloadBtn = string.Empty;
                    string deleteBtn = string.Empty;
                    if (string.IsNullOrEmpty(hf_fileEdit.Value)) {
                        editBtn = "<a class='td-edit-btn' title='Edit details' onclick=\"EditFile('" + item.ID + "');return false;\"></a>";
                        downloadBtn = "<a class='td-download-btn' title='Download' onclick=\"DownloadFile('" + item.ID + "');return false;\"></a>";
                        deleteBtn = "<a class='td-delete-btn' title='Delete' onclick=\"DeleteFile('" + item.ID + "');return false;\"></a>";
                    }

                    tableBuilder.AddBodyRow(bodyColumns, editBtn + downloadBtn + deleteBtn, "data-id='" + item.ID + "'");
                    #endregion
                }
            }
        }
        #endregion

        pnl_FilesDocuments.Controls.Clear();
        pnl_FilesDocuments.Controls.Add(tableBuilder.CompleteTableLiteralControl("There are no documents in folder"));

        GetNewFileNotification();

        UpdatePanel1_documents.Update();
        UpdatePanel2_documents.Update();

        if (!string.IsNullOrEmpty(hf_fileEdit.Value)) {
            RegisterPostbackScripts.RegisterStartupScript(this, "LoadEditValues('" + hf_fileEdit.Value + "');");
        }
    }
    private void LoadGroups() {
        dd_groups.Items.Clear();
        dd_moveGroup_documents.Items.Clear();

        if (!GroupSessions.DoesUserHaveGroupLoginSessionKey(_appInitializer.UserName)) {
            ListItem itemNone = new ListItem("- Public -", "-");
            dd_groups.Items.Add(itemNone);
            dd_moveGroup_documents.Items.Add(itemNone);
        }

        List<string> gns = _appInitializer.memberDatabase.GroupList;

        foreach (string group in gns) {
            string groupName = _groups.GetGroupName_byID(group);
            ListItem item = new ListItem(groupName, group);
            if (!dd_groups.Items.Contains(item)) {
                dd_groups.Items.Add(item);
                dd_moveGroup_documents.Items.Add(item);
            }
        }

        StringBuilder strJS = new StringBuilder();
        if (string.IsNullOrEmpty(_CurrentSelectedGroup)) {
            strJS.Append("$('#dd_groups').val('-');");
        }
        else {
            strJS.Append("$('#dd_groups').val('" + _CurrentSelectedGroup + "');");
        }

        if (string.IsNullOrEmpty(CurrentSelectedGroup_Move)) {
            strJS.Append("$('#dd_moveGroup_documents').val('-');");
        }
        else {
            strJS.Append("$('#dd_moveGroup_documents').val('" + CurrentSelectedGroup_Move + "');");
        }

        RegisterPostbackScripts.RegisterStartupScript(this, strJS.ToString());
    }
    private void LoadFolders(string _CurrentSelectedFolder) {
        _fileDrive = new FileDrive(_appInitializer.UserName);
        _fileDrive.GetAllFiles(_CurrentSelectedGroup);
        _fileDrive.GetAllFolders(_CurrentSelectedGroup);

        dd_folders.Items.Clear();
        dd_moveFolder_documents.Items.Clear();

        int currentIndex = 0;

        List<ListItem> itemList = GetFolderList(_CurrentSelectedGroup);
        for (int i = 0; i < itemList.Count; i++) {
            ListItem item = itemList[i];
            dd_folders.Items.Add(item);
            if (item.Value == _CurrentSelectedFolder) {
                currentIndex = i;
            }
        }

        List<ListItem> itemList2 = GetFolderList(CurrentSelectedGroup_Move);
        for (int i = 0; i < itemList2.Count; i++) {
            ListItem item = itemList2[i];
            dd_moveFolder_documents.Items.Add(item);
        }

        if (_CurrentSelectedFolder == "-" || string.IsNullOrEmpty(_CurrentSelectedFolder)) {
            dd_folders.SelectedIndex = 0;
            dd_moveFolder_documents.SelectedIndex = 0;
        }
        else if (_CurrentSelectedFolder == _appInitializer.memberDatabase.UserId) {
            _fileDrive.GetFilesByFolderName(_appInitializer.memberDatabase.UserId, _CurrentSelectedGroup);
            dd_folders.SelectedIndex = 1;
            dd_moveFolder_documents.SelectedIndex = 1;
        }
        else if (_CurrentSelectedFolder.StartsWith("#")) {
            _fileDrive.GetFilesByFolderName(_CurrentSelectedFolder, _CurrentSelectedGroup);
            dd_folders.SelectedIndex = currentIndex;
            if (_CurrentSelectedGroup == CurrentSelectedGroup_Move) {
                dd_moveFolder_documents.SelectedIndex = currentIndex;
            }
        }
        else {
            _fileDrive.GetFilesByFolderName(_CurrentSelectedFolder, _CurrentSelectedGroup);
            dd_folders.SelectedIndex = currentIndex;
            if (_CurrentSelectedGroup == CurrentSelectedGroup_Move) {
                dd_moveFolder_documents.SelectedIndex = currentIndex;
            }
        }
    }
    private void GetNewFileNotification() {
        int count = (from t in _fileDrive.documents_coll let now = ServerSettings.ServerDateTime select now.Subtract(t.UploadDate)).Count( final => final.Days <= 1);

        if (count > 0) {
            lbl_fileNoti_documents.Enabled = true;
            lbl_fileNoti_documents.Visible = true;
            lbl_fileNoti_documents.Text = count + " file(s) have been uploaded recently<div class='clear-space'></div>";
        }
        else {
            lbl_fileNoti_documents.Enabled = false;
            lbl_fileNoti_documents.Visible = false;
            lbl_fileNoti_documents.Text = string.Empty;
        }
    }
    private List<ListItem> GetFolderList(string groupId) {
        FileDrive tempFileDrive = new FileDrive(_appInitializer.UserName);
        tempFileDrive.GetAllFiles(groupId);
        tempFileDrive.GetAllFolders(groupId);

        List<ListItem> itemList = new List<ListItem>();

        #region All Files
        itemList.Add(new ListItem("- All Files -", "-"));
        #endregion

        #region Personal Folder
        itemList.Add(new ListItem("- Personal Folder -", _appInitializer.memberDatabase.UserId));
        #endregion

        if (tempFileDrive.folders_coll.Count > 0) {
            List<string> groupList = groupId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (groupList.Count == 0) {
                groupList.Add(string.Empty);
            }

            foreach (var t in tempFileDrive.folders_coll) {
                foreach (string gr in groupList) {
                    Guid outGuid = new Guid();
                    if (gr != t.GroupName || _appInitializer.memberDatabase.UserId == t.FolderName || Guid.TryParse(t.FolderName, out outGuid)) {
                        continue;
                    }

                    string currfolder = t.FolderName.Replace("_", " ");
                    itemList.Add(new ListItem(currfolder, t.ID.ToString()));
                }
            }
        }

        return itemList;
    }

    #endregion


    #region Helper Methods

    private static bool IsImageFileType(string extension) {
        var ok = (extension.ToLower() == ".png") || (extension.ToLower() == ".bmp") || (extension.ToLower() == ".jpg")
                 || (extension.ToLower() == ".jpeg") || (extension.ToLower() == ".jpe") || (extension.ToLower() == ".jfif")
                 || (extension.ToLower() == ".tif") || (extension.ToLower() == ".tiff") || (extension.ToLower() == ".gif")
                 || (extension.ToLower() == ".tga");
        return ok;
    }
    private static bool IsAudioVideoFileType(string extension) {
        var ok = (extension.ToLower() == ".mp3") || (extension.ToLower() == ".mp4");
        return ok;
    }
    private string GetFileImage(string extension) {
        string path = _absolutePath + "Apps/FileDrive/Images/FileTypes/";
        if ((extension.ToLower() == ".png") || (extension.ToLower() == ".bmp") || (extension.ToLower() == ".jpg")
                 || (extension.ToLower() == ".jpeg") || (extension.ToLower() == ".jpe") || (extension.ToLower() == ".jfif")
                 || (extension.ToLower() == ".tif") || (extension.ToLower() == ".tiff") || (extension.ToLower() == ".gif")
                 || (extension.ToLower() == ".tga")) {
            return "<img alt='filetype' src='" + path + "image.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".doc") || (extension.ToLower() == ".docx")
                 || (extension.ToLower() == ".dotx") || (extension.ToLower() == ".dotm")) {
            return "<img alt='filetype' src='" + path + "word.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".xlsx") || (extension.ToLower() == ".xlsm") || (extension.ToLower() == ".xlam")
                 || (extension.ToLower() == ".xltx") || (extension.ToLower() == ".xlsb") || (extension.ToLower() == ".xls")) {
            return "<img alt='filetype' src='" + path + "excel.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".pptx") || (extension.ToLower() == ".pptm") || (extension.ToLower() == ".potx")
                || (extension.ToLower() == ".potm") || (extension.ToLower() == ".ppam") || (extension.ToLower() == ".ppsm")) {
            return "<img alt='filetype' src='" + path + "powerpoint.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".pptx") || (extension.ToLower() == ".pptm") || (extension.ToLower() == ".potx")
                || (extension.ToLower() == ".potm") || (extension.ToLower() == ".ppam") || (extension.ToLower() == ".ppsm")
                || (extension.ToLower() == ".ppt")) {
            return "<img alt='filetype' src='" + path + "powerpoint.png' style='height:16px' />";
        }
        else if (extension.ToLower() == ".pdf") {
            return "<img alt='filetype' src='" + path + "pdf.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".html") || (extension.ToLower() == ".htm")) {
            return "<img alt='filetype' src='" + path + "html.png' style='height:16px' />";
        }
        else if (extension.ToLower() == ".txt") {
            return "<img alt='filetype' src='" + path + "page_code.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".zip") || (extension.ToLower() == ".rar") || (extension.ToLower() == ".iso")) {
            return "<img alt='filetype' src='" + path + "zip.png' style='height:16px' />";
        }
        else if (extension.ToLower() == ".mp3") {
            return "<img alt='filetype' src='" + path + "music.png' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".avi") || (extension.ToLower() == ".mp4")) {
            return "<img alt='filetype' src='" + path + "video.png' style='height:16px' />";
        }
        return "<img alt='filetype' src='" + path + "unknown.png' style='height:16px' />";
    }
    private string getTime(DateTime postDate) {
        DateTime now = ServerSettings.ServerDateTime;
        TimeSpan final = now.Subtract(postDate);
        string time;
        if (final.Days > 1) {
            time = postDate.ToShortDateString();
        }
        else {
            if (final.Days == 0) {
                if (final.Hours == 0) {
                    time = final.Minutes.ToString(CultureInfo.InvariantCulture) + " minute(s) ago";
                }
                else {
                    time = final.Hours.ToString(CultureInfo.InvariantCulture) + " hour(s) ago";
                }
            }
            else {
                time = final.Days.ToString(CultureInfo.InvariantCulture) + " day(s) ago";
            }
        }
        return time;
    }
    private void DownloadFile(Guid id) {
        bool canContinue = true;
        string filePath = _fileDrive.GetFileNamePath(id);
        FileInfo fi = new FileInfo(filePath);
        string realFilePath = filePath.Replace(fi.Name, id.ToString() + FileDrive.NewFileExt);

        if (FileDrive.FileExtOk(fi.Extension)) {
            realFilePath = filePath.Replace(fi.Name, id.ToString() + fi.Extension);
        }

        if (canContinue) {
            try {
                string strFileName = Path.GetFileName(filePath);
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
                Response.Clear();
                Response.TransmitFile(realFilePath);
                Response.Flush();
                Response.End();
            }
            catch (Exception e) {
                AppLog.AddError(e);
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + e.Message + "');");
            }
        }
        else {
            HelperMethods.PageRedirect("~/Apps/FileDrive/FileDrive.aspx");
        }
    }

    #endregion


    #region Postback Controls

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
            string id = uuf.getFlag_AppID(hf_UpdateAll.Value);
            if (id == app_id) {
                uuf.deleteFlag(hf_UpdateAll.Value);
            }
        }

        BuildFileTable();
        hf_UpdateAll.Value = "";
    }

    #endregion


    #region Top Controls

    protected void btn_moveFile_Click(object sender, EventArgs e) {
        string[] filelist = hf_moveFiles_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < filelist.Length; i++) {
            _fileDrive.updateFolderName(Guid.Parse(filelist[i]), dd_moveFolder_documents.SelectedValue);
            _fileDrive.updateDocumentGroup(filelist[i], CurrentSelectedGroup_Move);
        }

        uuf.addFlag("app-filedrive", "");

        CurrentSelectedGroup_Move = string.Empty;
        hf_moveFiles_documents.Value = string.Empty;
        BuildFileTable();

        RegisterPostbackScripts.RegisterStartupScript(this.Page, "selectedFileList = new Array();");
    }
    protected void btn_refresh_Click(object sender, EventArgs e) {
        BuildFileTable();
    }
    protected void hf_moveGroupChange_ValueChanged(object sender, EventArgs e) {
        if (hf_moveGroupChange.Value == "-") {
            CurrentSelectedGroup_Move = string.Empty;
        }
        else {
            CurrentSelectedGroup_Move = hf_moveGroupChange.Value;
        }

        BuildFileTable();
    }
    protected void hf_groupsChange_ValueChanged(object sender, EventArgs e) {
        if (hf_groupsChange.Value == "-") {
            CurrentSelectedGroup = string.Empty;
            _CurrentSelectedGroup = string.Empty;
            CurrentSelectedGroup_Move = string.Empty;
        }
        else {
            CurrentSelectedGroup = hf_groupsChange.Value;
            _CurrentSelectedGroup = hf_groupsChange.Value;
            CurrentSelectedGroup_Move = hf_groupsChange.Value;
        }

        CurrentSelectedFolder = string.Empty;
        hf_moveFiles_documents.Value = string.Empty;
        BuildFileTable();
        RegisterPostbackScripts.RegisterStartupScript(this.Page, "selectedFileList = new Array();");
    }
    protected void dd_folders_Changed(object sender, EventArgs e) {
        CurrentSelectedFolder = dd_folders.SelectedValue;
        hf_moveFiles_documents.Value = string.Empty;
        BuildFileTable();
        RegisterPostbackScripts.RegisterStartupScript(this.Page, "selectedFileList = new Array();");
    }

    #endregion


    #region FileUpload

    protected void btnFileUpload_OnClick(object sender, EventArgs e) {
        if (FileUploadControl.HasFile) {
            try {
                IIdentity userID = HttpContext.Current.User.Identity;
                foreach (HttpPostedFile file in FileUploadControl.PostedFiles) {
                    var filesql = new FileDrive(userID.Name);
                    string pathsql = FileDrive.DocumentsFolder;
                    string fileName_temp1sql = Path.GetFileName(file.FileName);
                    string fileNamesql = removeRegex(fileName_temp1sql);
                    string tempext = Path.GetExtension(fileNamesql);
                    string fileNameId = Guid.NewGuid().ToString();

                    string p = Path.Combine(pathsql, fileNameId + FileDrive.NewFileExt);
                    if (FileDrive.FileExtOk(tempext)) {
                        p = Path.Combine(pathsql, fileNameId + tempext);
                    }

                    string folder = CurrentSelectedFolder;
                    if (string.IsNullOrEmpty(folder)) {
                        folder = "-";
                    }

                    file.SaveAs(p);
                    var info = new FileInfo(p);
                    if (info.Exists) {
                        filesql.addFile(fileNameId, fileNamesql, tempext, HelperMethods.FormatBytes(info.Length), pathsql, string.Empty, folder, _CurrentSelectedGroup, false);
                    }

                    uuf.addFlag("app-filedrive", "");
                }
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }

        ServerSettings.RefreshPage(Page, string.Empty);
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

    #endregion


    #region File/Folder Edit

    protected void hf_fileEdit_ValueChanged(object sender, EventArgs e) {
        hf_moveFiles_documents.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this.Page, "selectedFileList = new Array();");
        if (hf_fileEdit.Value == "CANCEL") {
            hf_fileEdit.Value = string.Empty;
        }
        BuildFileTable();
        hf_fileEdit.Value = string.Empty;
    }
    protected void hf_fileDelete_ValueChanged(object sender, EventArgs e) {
        try {
            string filePath = _fileDrive.GetFileNamePath(Guid.Parse(hf_fileDelete.Value));
            var infoDelete = new FileInfo(filePath);
            string realFilePath = infoDelete.FullName.Replace(infoDelete.Name, hf_fileDelete.Value + FileDrive.NewFileExt);
            if (FileDrive.FileExtOk(infoDelete.Extension)) {
                realFilePath = infoDelete.FullName.Replace(infoDelete.Name, hf_fileDelete.Value + infoDelete.Extension);
            }
            if (File.Exists(realFilePath)) {
                File.Delete(realFilePath);
            }
            _fileDrive.deleteFile(Guid.Parse(hf_fileDelete.Value));
            uuf.addFlag("app-filedrive", "");
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }

        BuildFileTable();
        hf_fileDelete.Value = string.Empty;
    }
    protected void hf_fileDeleteSelected_ValueChanged(object sender, EventArgs e) {
        string[] filelist = hf_moveFiles_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < filelist.Length; i++) {
            _fileDrive.deleteFile(Guid.Parse(filelist[i]));
        }

        uuf.addFlag("app-filedrive", "");

        BuildFileTable();
        hf_fileDeleteSelected.Value = string.Empty;
        hf_moveFiles_documents.Value = string.Empty;

        RegisterPostbackScripts.RegisterStartupScript(this.Page, "selectedFileList = new Array();");
    }
    protected void btn_DownloadFile_Click(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_fileDownload.Value)) {
            DownloadFile(Guid.Parse(hf_fileDownload.Value));
        }

        BuildFileTable();
        hf_fileDownload.Value = string.Empty;
    }
    protected void hf_fileUpdate_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_fileUpdate.Value)) {
            if (!string.IsNullOrEmpty(hf_fileUpdateName.Value) && !string.IsNullOrEmpty(hf_fileUpdateExt.Value)) {
                _fileDrive.updateFileName(Guid.Parse(hf_fileUpdate.Value), HttpUtility.UrlDecode(hf_fileUpdateName.Value) + hf_fileUpdateExt.Value);
            }
            if (!string.IsNullOrEmpty(hf_fileUpdateFolder.Value)) {
                _fileDrive.updateFolderName(Guid.Parse(hf_fileUpdate.Value), hf_fileUpdateFolder.Value);
            }

            uuf.addFlag("app-filedrive", "");
        }

        hf_fileUpdateName.Value = string.Empty;
        hf_fileUpdateFolder.Value = string.Empty;
        hf_fileUpdate.Value = string.Empty;
        BuildFileTable();
    }

    protected void lbtn_editfolders_Click(object sender, EventArgs e) {
        CurrentFolderID_Edit = string.Empty;
        InitializeNewFolderDialog();
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'FolderEdit-element', 'Folder Edit');");
    }
    protected void btn_newFolder_Click(object sender, EventArgs e) {
        bool canAdd = true;

        string foldername = tb_newFolder.Text.Trim();
        FileDrive smw = new FileDrive(_appInitializer.UserName);
        Guid outGuid2 = new Guid();
        if (foldername == "-" || string.IsNullOrEmpty(foldername) || Guid.TryParse(foldername, out outGuid2)) {
            canAdd = false;
        }

        if (canAdd) {
            string newfolder = removeRegex(foldername);
            smw.GetAllFolders();

            bool folderExists = false;
            for (int i = 0; i < smw.folders_coll.Count; i++) {
                if (_CurrentSelectedGroup == smw.folders_coll[i].GroupName) {
                    if (smw.folders_coll[i].FolderName.ToLower() == newfolder.ToLower()) {
                        folderExists = true;
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Folder already exists. Please enter a new name.');");
                        break;
                    }
                }
            }

            if (!folderExists) {
                smw.addfolder(newfolder, _CurrentSelectedGroup);
            }
        }

        uuf.addFlag("app-filedrive", "");
        InitializeNewFolderDialog();
    }
    protected void dd_folderEditList_SelectedIndexChanged(object sender, EventArgs e) {
        CurrentSelectedFolder_Edit = dd_folderEditList.SelectedValue;
        InitializeNewFolderDialog();
    }
    protected void lbtn_EditFolder_Click(object sender, EventArgs e) {
        CurrentFolderID_Edit = CurrentSelectedFolder_Edit;
        InitializeNewFolderDialog();
    }
    protected void hf_DeleteFolder_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteFolder.Value)) {
            FileDrive smw = new FileDrive(_appInitializer.UserName);
            Guid id = Guid.Parse(hf_DeleteFolder.Value);
            smw.deleteFolder(id, _CurrentSelectedGroup);

            uuf.addFlag("app-filedrive", "");
        }

        hf_DeleteFolder.Value = string.Empty;
        InitializeNewFolderDialog();
    }
    protected void btn_UpdateFolder_Click(object sender, EventArgs e) {
        FileDrive smw = new FileDrive(_appInitializer.UserName);
        Guid outGuid1 = new Guid();
        bool canUpdate = true;

        string newname = tb_editFolder.Text.Trim();
        if (string.IsNullOrEmpty(newname) || newname == "-" || Guid.TryParse(newname, out outGuid1)) {
            canUpdate = false;
        }
        
        if (canUpdate) {
            newname = removeRegex(newname);
            smw.GetAllFolders();

            bool folderExists = false;
            for (int i = 0; i < smw.folders_coll.Count; i++) {
                if (_CurrentSelectedGroup == smw.folders_coll[i].GroupName) {
                    if (smw.folders_coll[i].FolderName.ToLower() == newname.ToLower()) {
                        folderExists = true;
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Folder already exists. Please enter a new name.');");
                        break;
                    }
                }
            }
            if (!folderExists) {
                string oldname = smw.GetFolderbyID(Guid.Parse(CurrentFolderID_Edit));
                smw.updateFolderNameMain(newname, oldname, _CurrentSelectedGroup);

                uuf.addFlag("app-filedrive", "");
            }
        }

        CurrentFolderID_Edit = string.Empty;
        InitializeNewFolderDialog();
    }
    protected void btn_cancelEditFolder_Click(object sender, EventArgs e) {
        CurrentFolderID_Edit = string.Empty;
        InitializeNewFolderDialog();
    }
    private void InitializeNewFolderDialog() {
        BuildFileTable();

        tb_newFolder.Text = string.Empty;

        int currentIndex = 0;
        dd_folderEditList.Items.Clear();
        List<ListItem> itemList = GetFolderList(_CurrentSelectedGroup);
        for (int i = 2; i < itemList.Count; i++) {
            dd_folderEditList.Items.Add(itemList[i]);
            if (itemList[i].Value == CurrentSelectedFolder_Edit) {
                currentIndex = i - 2;
            }
        }

        if (string.IsNullOrEmpty(CurrentSelectedFolder_Edit) && dd_folderEditList.Items.Count > 0) {
            CurrentSelectedFolder_Edit = dd_folderEditList.Items[0].Value;
        }

        if (dd_folderEditList.Items.Count > currentIndex) {
            dd_folderEditList.SelectedIndex = currentIndex;
        }

        pnl_EditFolder.Enabled = false;
        pnl_EditFolder.Visible = false;
        if (!string.IsNullOrEmpty(CurrentFolderID_Edit)) {
            pnl_EditFolder.Enabled = true;
            pnl_EditFolder.Visible = true;
            tb_editFolder.Text = _fileDrive.GetFolderbyID(Guid.Parse(CurrentFolderID_Edit));
        }

        pnl_folderEditList.Enabled = false;
        pnl_folderEditList.Visible = false;
        if (dd_folderEditList.Items.Count > 0) {
            pnl_folderEditList.Enabled = true;
            pnl_folderEditList.Visible = true;
        }

        updatePnl_NewFolder.Update();
    }

    #endregion

}