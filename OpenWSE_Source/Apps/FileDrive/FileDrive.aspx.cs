#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_FileDrive : Page {

    #region private variables

    private const string app_id = "app-documents";
    private readonly IPWatch ipwatch = new IPWatch(true);
    private ServerSettings ss = new ServerSettings();
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly Groups _groups = new Groups();
    private string _username;
    private FileDrive docs;
    private string hf_sortcolprev;
    private MemberDatabase member;
    private GridViewRow row;
    private string _sitetheme;
    private AppInitializer _appInitializer;

    #endregion


    #region public variables

    public string absolutePath;

    #endregion


    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            _username = _appInitializer.UserName;
            member = _appInitializer.memberDatabase;
            _sitetheme = _appInitializer.siteTheme;

            docs = new FileDrive(userId.Name);
            absolutePath = ServerSettings.GetSitePath(Request);

            if (absolutePath.LastIndexOf('/') != absolutePath.Length - 1)
                absolutePath += "/";

            string equalizer = "<img alt='equalizer' src='" + absolutePath + "App_Themes/" + _sitetheme + "/App/equalizer.gif' />";
            RegisterPostbackScripts.RegisterStartupScript(this, "equalizerImage=\"" + equalizer + "\";");

            ScriptManager sm = ScriptManager.GetCurrent(Page);
            string ctlId = sm.AsyncPostBackSourceElementID;

            if (!IsPostBack) {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, app_id, this);
                aus.StartAutoUpdates();

                ResetSelected();

                LoadGroups();
                ViewState["sortOrder"] = "asc";
                hf_sortcol_documents.Value = "0";
                LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, "asc");
                row = GV_Files_documents.HeaderRow;
                var x = (HtmlTableCell)row.FindControl("td_filename");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x.Attributes["class"] += " active asc";
                else
                    x.Attributes["class"] += " active desc";
            }

            HideImages();
            LoadDocuments();
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        bool cancontinue = false;
        if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
            string id = uuf.getFlag_AppID(hf_UpdateAll.Value);
            if (id == app_id) {
                uuf.deleteFlag(hf_UpdateAll.Value);
                cancontinue = true;
            }
        }

        if (cancontinue) {
            InitializeSort(false);
        }
        hf_UpdateAll.Value = "";
    }

    protected void lbtn_HideImages_Click(object sender, EventArgs e) {
        if ((ViewState["HideImages"] == null) || ((bool)ViewState["HideImages"] == false)) {
            ViewState["HideImages"] = true;
            lbtn_HideImages.Text = "Show Images";
        }
        else {
            ViewState["HideImages"] = false;
            lbtn_HideImages.Text = "Hide Images";
        }

        InitializeSort(false);
    }
    private void HideImages() {
        if ((ViewState["HideImages"] != null) && ((bool)ViewState["HideImages"] == true)) {
            lbtn_HideImages.Text = "Show Images";
        }
        else {
            lbtn_HideImages.Text = "Hide Images";
        }
    }

    private void LoadGroups() {
        string groupArray = "";
        List<string> gns = member.GroupList;
        foreach (string group in gns) {
            string groupName = _groups.GetGroupName_byID(group);
            var item = new ListItem(groupName, group);
            if (!dd_groups.Items.Contains(item)) {
                if (string.IsNullOrEmpty(hf_ddgroups.Value)) {
                    hf_ddgroups.Value = group;
                }
                dd_groups.Items.Add(item);
                groupArray += group + ServerSettings.StringDelimiter;
            }
        }

        var itemNone = new ListItem("None", string.Empty);
        dd_groups.Items.Add(itemNone);
    }

    private void LoadDocuments() {
        pnl_ect_documents.Enabled = true;
        pnl_ect_documents.Visible = true;

        docs = new FileDrive(_username);
        docs.GetAllFiles(hf_ddgroups.Value);
        docs.GetAllFolders(hf_ddgroups.Value);
        var docsPersonal = new FileDrive(_username);
        docsPersonal.GetPersonalFiles(member.UserId, hf_ddgroups.Value);
        string countPersonal = docsPersonal.documents_coll.Count + " file";
        if ((docsPersonal.documents_coll.Count == 0) || (docsPersonal.documents_coll.Count > 1))
            countPersonal += "s";

        var str = new StringBuilder();
        str.Append("<div class='inline-block'><div class='clear-margin'><b class='pad-right'>Files Uploaded</b>" + docs.documents_coll.Count + "</div>");
        str.Append("<div class='clear-margin'><b class='pad-right'>Folders</b>" + (docs.folders_coll.Count) + "</div>");
        str.Append("<div class='clear-margin'><input id='tb_newfolderentry' type='text' maxlength='15' value='Create New Folder' class='textEntry margin-right-sml' style='width: 150px;' ");
        str.Append("onfocus=\"if(this.value=='Create New Folder')this.value=''\" onblur=\"if(this.value=='')this.value='Create New Folder'\" />");
        str.Append("<a href='#newfolder' onclick='createnewfolder();return false;' class='margin-left'>Create</a><br />");
        str.Append("<span id='foldermessage'></span></div>");

        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            // str.Append("<div class='clear-space'></div><input id='btn_refreshdatabase_documents' type='button' value='Refresh Database' class='input-buttons RandomActionBtns-docs' />");
        }

        str.Append("<div class='clear-space'></div><div class='clear-margin'><h3>List of Folders</h3><div class='clear' style='height: 5px;'></div>");

        string addCssStart = "";
        if (!IsPostBack)
            addCssStart = " tsactive";

        str.Append("<div id='expand_View_All_Documents' class='tsdiv RandomActionBtns-docs" + addCssStart + "' onclick='folderChange(\"0\")'><span class='first-node float-left' style='padding: 3px'>View All Documents in Group</span></div>");
        str.Append("<div class='clear-space-five'></div>");
        str.Append("<div id='expand_PersonalFolder_Documents' class='tsdiv RandomActionBtns-docs' onclick='folderChange(\"1\")'><span class='first-node float-left' style='padding: 3px'>My Personal Folder</span><span class='float-left' style='padding-top: 3px; padding-left: 5px'>(" + countPersonal + ")</span></div>");
        str.Append("<div class='sidebar-divider'></div><div class='clear-space-five'></div>");
        if (docs.folders_coll.Count == 0) {
            str.Append("There are no folders in group");
        }
        else {
            int count = 0;
            string[] groupList = hf_ddgroups.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string memberId = member.UserId;
            foreach (var t in docs.folders_coll) {
                foreach (string gr in groupList) {
                    Guid outGuid = new Guid();
                    if ((gr != t.GroupName) || (memberId == t.FolderName) || Guid.TryParse(t.FolderName, out outGuid)) continue;
                    string currfolder = t.FolderName;
                    string temp_currfolder = currfolder;
                    if (currfolder == "-") {
                        temp_currfolder = "Root Directory";
                    }
                    else {
                        try {
                            temp_currfolder = currfolder.Replace("_", " ");
                        }
                        catch {
                        }
                    }
                    string xdelete = "x_" + t.ID;
                    string xedit = "e_" + t.ID;
                    var smwtemp = new FileDrive(_username);
                    smwtemp.GetFilesByFolderName(currfolder, gr);
                    string plural = string.Empty;

                    if (groupList.Length > 1) {
                        Groups group = new Groups();
                        string realName = group.GetGroupName_byID(gr);
                        temp_currfolder = currfolder + "<span class='third-node'> - " + realName + "</span>";
                    }

                    if ((smwtemp.documents_coll.Count == 0) || (smwtemp.documents_coll.Count > 1))
                        plural = "s";

                    str.Append("<div id='expand_" + t.ID + "' class='tsdiv' onmouseover=\"showdelete('" + xdelete +
                               "','" + xedit + "')\" onmouseout=\"hidedelete('" + xdelete + "','" + xedit +
                               "')\"><div class='tsdivclick RandomActionBtns-docs'><span class='first-node float-left' style='padding: 3px'>" +
                               temp_currfolder + "</span><span class='second-node' style='display:none;'>" + t.ID + "</span>");
                    str.Append("<span class='float-left' style='padding-top: 3px; padding-left: 5px'>(" +
                               smwtemp.documents_coll.Count + " file" + plural +
                               ")</span></div><span class='float-right pad-right-sml'>");
                    if (temp_currfolder != "-") {
                        str.Append("<a id='" + xdelete + "' href='#delete_" + t.ID + "' title='Delete folder' onclick='deletefolder(this)' class='td-delete-btn float-right margin-right-sml margin-top-sml' style='display: none; padding: 0px!important;'></a>");
                        str.Append("<a id='" + xedit + "' href='#edit_" + t.ID + "' title='Edit folder' onclick=\"editfolder(this,'" + currfolder + "')\" class='td-edit-btn float-right margin-right margin-top-sml' style='display: none; padding: 0px!important;'></a></span>");
                    }
                    str.Append("</div><div class='clear-space-five'></div>");
                    count++;
                }
            }

            if (count == 0) {
                str.Append("There are no folders for this group");
            }
        }
        str.Append("</div></div>");
        ltl_ect_documents.Text = str.ToString();
    }

    protected void hf_folderchange_ValueChanged(object sender, EventArgs e) {
        if (hf_folderchange_documents.Value == "0") {
            hf_category_documents.Value = "";
            InitializeSort(false);
            lbl_currFolderName.Text = "Root Directory";
        }
        else if (hf_folderchange_documents.Value == "1") {
            hf_category_documents.Value = "UserFolder";
            docs.GetFilesByFolderName(member.UserId, hf_ddgroups.Value);
            InitializeSort(false);
            lbl_currFolderName.Text = "My Personal Folder";
        }
        else if (hf_folderchange_documents.Value.StartsWith("#")) {
            string foldername = hf_folderchange_documents.Value.Substring(1);
            docs.GetFilesByFolderName(foldername, hf_ddgroups.Value);
            InitializeSort2(false);
            lbl_currFolderName.Text = foldername;
        }
        else {
            hf_category_documents.Value = hf_folderchange_documents.Value;
            docs.GetFilesByFolderName(hf_folderchange_documents.Value, hf_ddgroups.Value);
            InitializeSort(false);
            lbl_currFolderName.Text = hf_folderchange_documents.Value;
        }

        hf_folderchange_documents.Value = "";
    }

    #endregion


    #region GridView Properties Methods

    protected void GV_Files_RowCreated(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Pager) {
            var PnlPager = (Panel)e.Row.FindControl("PnlPager");
            for (var i = 0; i < GV_Files_documents.PageCount; i++) {
                if (i % 10 == 0) {
                    PnlPager.Controls.Add(new LiteralControl("<div class=\"clear\" style=\"height: 10px;\"></div>"));
                }
                var lbtn_page = new LinkButton();
                lbtn_page.CommandArgument = i.ToString();
                lbtn_page.CommandName = "PageNo";
                if (GV_Files_documents.PageIndex == i) {
                    lbtn_page.CssClass = "GVPagerNumActive RandomActionBtns";
                }
                else {
                    lbtn_page.CssClass = "GVPagerNum RandomActionBtns";
                }
                lbtn_page.Text = (i + 1).ToString();
                PnlPager.Controls.Add(lbtn_page);
                ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(lbtn_page);
            }
        }
        else if (e.Row.RowType == DataControlRowType.Header) {
            if (e.Row.Cells.Count > 0) {
                LinkButton GV_Files_documents_lbtn_album = (LinkButton)e.Row.Cells[0].FindControl("lbtn_album");
                if (GV_Files_documents_lbtn_album != null) {
                    if (string.IsNullOrEmpty(hf_ddgroups.Value)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#updatepnl_ect_documents, .ajaxCall_Modal_documents').hide();");
                        GV_Files_documents_lbtn_album.Text = "Group";
                        lbl_movetofolder_documents.Text = "Move Selected Files to a Group:";
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#updatepnl_ect_documents, .ajaxCall_Modal_documents').show();");
                        GV_Files_documents_lbtn_album.Text = "Folder";
                        lbl_movetofolder_documents.Text = "Move Selected Files to a Folder:";
                    }
                }
            }
        }
    }

    protected void GV_Files_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Pager) {
            var lbl2 = (Label)e.Row.FindControl("pglbl2");
            var btnFirst = (LinkButton)e.Row.FindControl("btnFirst");
            var btnPrevious = (LinkButton)e.Row.FindControl("btnPrevious");
            var btnNext = (LinkButton)e.Row.FindControl("btnNext");
            var btnLast = (LinkButton)e.Row.FindControl("btnLast");
            var tb_page = (TextBox)e.Row.FindControl("tb_pageManual");
            lbl2.Text = GV_Files_documents.PageCount.ToString();
            tb_page.Text = (GV_Files_documents.PageIndex + 1).ToString();

            if ((GV_Files_documents.PageIndex + 1) == 1) {
                btnFirst.Visible = false;
                btnPrevious.Visible = false;
            }
            if ((GV_Files_documents.PageIndex + 1) == GV_Files_documents.PageCount) {
                btnNext.Visible = false;
                btnLast.Visible = false;
            }
            InitializeSort(true);
        }
    }

    protected void GV_Files_RowEdit(object sender, GridViewEditEventArgs e) {
        ResetSelected();
        GV_Files_documents.EditIndex = e.NewEditIndex;
        InitializeSort(false);
        var dd_editFolderName = (DropDownList)GV_Files_documents.Rows[e.NewEditIndex].FindControl("dd_editFolderName");
        var hf_editFolderName = (HiddenField)GV_Files_documents.Rows[e.NewEditIndex].FindControl("hf_editFolderName");

        if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
            docs.GetAllFolders(hf_ddgroups.Value);
            dd_editFolderName.Items.Clear();
            var itemstart = new ListItem("Root Directory", "-");
            dd_editFolderName.Items.Add(itemstart);

            string memberId = member.UserId;
            var itemMyFolder = new ListItem("Personal Folder", memberId);
            dd_editFolderName.Items.Add(itemMyFolder);
            dd_editFolderName.SelectedIndex = 0;

            if (hf_editFolderName.Value == "-")
                dd_editFolderName.SelectedIndex = 1;

            int count = 2;
            foreach (FileDriveFolders_Coll t in docs.folders_coll) {
                if (t.FolderName != memberId) {
                    var item = new ListItem(GetAlbum(t.FolderName), t.FolderName);
                    if (!dd_editFolderName.Items.Contains(item)) {
                        dd_editFolderName.Items.Add(item);
                        if ((item.Value == hf_editFolderName.Value) || (item.Value == hf_editFolderName.Value.Replace(" ", "_")))
                            dd_editFolderName.SelectedIndex = count;

                        count++;
                    }
                }
            }
        }
        else {
            List<string> userGroups = member.GroupList;
            dd_editFolderName.Items.Clear();
            foreach (string g in userGroups) {
                string groupName = _groups.GetGroupName_byID(g);
                var itemstart = new ListItem(groupName, g);
                dd_editFolderName.Items.Add(itemstart);
            }
        }
    }

    protected void GV_Files_RowUpdate(object sender, GridViewUpdateEventArgs e) {
        var id = (HiddenField)GV_Files_documents.Rows[e.RowIndex].FindControl("hf_editID");
        var oldname = (HiddenField)GV_Files_documents.Rows[e.RowIndex].FindControl("hf_editFileName");
        var oldfolder = (HiddenField)GV_Files_documents.Rows[e.RowIndex].FindControl("hf_editFolderName");
        var hfEditExt = (HiddenField)GV_Files_documents.Rows[e.RowIndex].FindControl("hf_editExt");
        var newname = (TextBox)GV_Files_documents.Rows[e.RowIndex].FindControl("tb_editFileName");
        var ddEditFolderName = (DropDownList)GV_Files_documents.Rows[e.RowIndex].FindControl("dd_editFolderName");
        if ((oldname.Value != newname.Text) && (!string.IsNullOrEmpty(newname.Text))) {
            string pathsql = ServerSettings.GetServerMapLocation;
            string newfilenamesql = newname.Text;
            string oldfilenamesql = oldname.Value + hfEditExt.Value;

            newfilenamesql += hfEditExt.Value;
            docs.updateFileName(Guid.Parse(id.Value), newfilenamesql);
        }

        if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
            if (oldfolder.Value != ddEditFolderName.SelectedValue)
                docs.updateFolderName(Guid.Parse(id.Value), ddEditFolderName.SelectedValue, hf_ddgroups.Value);
        }
        else {
            docs.updateFolderName(Guid.Parse(id.Value), "-", ddEditFolderName.SelectedValue);
        }

        GV_Files_documents.EditIndex = -1;
        InitializeSort(false);
        string autoupdate = "document.getElementById('" + hf_searchFiles_documents.ClientID + "').value = 'delete';";
        autoupdate += "__doPostBack('" + hf_searchFiles_documents.ClientID + "', '');";
        RegisterPostbackScripts.RegisterStartupScript(this, autoupdate);
    }

    protected void GV_Files_CancelEdit(object sender, GridViewCancelEditEventArgs e) {
        GV_Files_documents.EditIndex = -1;
        InitializeSort(false);
    }

    protected void GV_Files_RowCommand(object sender, GridViewCommandEventArgs e) {
        switch (e.CommandName) {
            case "downloadFile":
                DownloadFile(Guid.Parse(e.CommandArgument.ToString()));
                InitializeSort(false);
                break;
            case "deleteFile":
                try {
                    string filePath = docs.GetFileNamePath(Guid.Parse(e.CommandArgument.ToString()));
                    var infoDelete = new FileInfo(filePath);
                    string realFilePath = infoDelete.FullName.Replace(infoDelete.Name, e.CommandArgument.ToString() + FileDrive.NewFileExt);
                    if (FileDrive.FileExtOk(infoDelete.Extension)) {
                        realFilePath = infoDelete.FullName.Replace(infoDelete.Name, e.CommandArgument.ToString() + infoDelete.Extension);
                    }
                    if (File.Exists(realFilePath)) {
                        File.Delete(realFilePath);
                    }
                    docs.deleteFile(Guid.Parse(e.CommandArgument.ToString()));
                    uuf.addFlag("app-filedrive", "");
                }
                catch { }
                InitializeSort(false);
                string autoupdate = "document.getElementById('" + hf_searchFiles_documents.ClientID + "').value = 'delete';";
                autoupdate += "__doPostBack('" + hf_searchFiles_documents.ClientID + "', '');";
                RegisterPostbackScripts.RegisterStartupScript(this, autoupdate);
                break;
        }
    }

    public void LoadFiles(ref GridView gv, string sortExp, string sortDir) {
        bool personalFolder = false;
        if (string.IsNullOrEmpty(hf_category_documents.Value)) {
            docs.GetAllFiles(hf_ddgroups.Value);
            personalFolder = true;
        }
        else {
            if ((hf_category_documents.Value == "UserFolder") || (string.IsNullOrEmpty(hf_category_documents.Value))) {
                docs.GetPersonalFiles(member.UserId, hf_ddgroups.Value);
                personalFolder = true;
            }
            else
                docs.GetFilesByFolderName(hf_category_documents.Value, hf_ddgroups.Value);
        }
        DataView dvFiles = GetFiles(docs.documents_coll, personalFolder);
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
        if (dvFiles.Count == 0) {
            lbtn_selectAll_documents.Enabled = false;
            lbtn_selectAll_documents.Visible = false;
        }
        else {
            lbtn_selectAll_documents.Enabled = true;
            lbtn_selectAll_documents.Visible = true;
        }
    }

    public void LoadFiles2(ref GridView gv, string sortExp, string sortDir) {
        bool personalFolder = false;
        if (hf_category_documents.Value == "UserFolder") {
            docs.GetPersonalFiles(member.UserId, hf_ddgroups.Value);
            personalFolder = true;
        }
        DataView dvFiles = GetFiles(docs.documents_coll, personalFolder);
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
        if (dvFiles.Count == 0) {
            lbtn_selectAll_documents.Enabled = false;
            lbtn_selectAll_documents.Visible = false;
        }
        else {
            lbtn_selectAll_documents.Enabled = true;
            lbtn_selectAll_documents.Visible = true;
        }
    }

    public void LoadFilesSearch(ref GridView gv, string sortExp, string sortDir, List<FileDriveDocuments_Coll> temp) {
        bool personalFolder = false;
        if (hf_category_documents.Value == "UserFolder") {
            docs.GetPersonalFiles(member.UserId, hf_ddgroups.Value);
            personalFolder = true;
        }
        DataView dvFiles = GetFiles(temp, personalFolder);
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
        if (dvFiles.Count == 0) {
            lbtn_selectAll_documents.Enabled = false;
            lbtn_selectAll_documents.Visible = false;
        }
        else {
            lbtn_selectAll_documents.Enabled = true;
            lbtn_selectAll_documents.Visible = true;
        }
    }

    public DataView GetFiles(List<FileDriveDocuments_Coll> file_coll, bool personalFolder) {
        var dtFiles = new DataTable();
        Type type = typeof(long);
        dtFiles.Columns.Add(new DataColumn("TitleSort"));
        dtFiles.Columns.Add(new DataColumn("Type"));
        dtFiles.Columns.Add(new DataColumn("SizeSorted", type));
        dtFiles.Columns.Add(new DataColumn("UploadDate"));
        dtFiles.Columns.Add(new DataColumn("Album"));
        dtFiles.Columns.Add(new DataColumn("ID"));
        dtFiles.Columns.Add(new DataColumn("Path"));
        dtFiles.Columns.Add(new DataColumn("Size"));
        dtFiles.Columns.Add(new DataColumn("TitleEdit"));
        dtFiles.Columns.Add(new DataColumn("Title"));
        string memberId = member.UserId;

        foreach (var x in file_coll) {
            DataRow drFile = dtFiles.NewRow();
            string[] groupList = hf_ddgroups.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            bool isInGroup = false;
            foreach (string _g in groupList) {
                if (_g == x.GroupName) {
                    isInGroup = true;
                }
            }

            if ((groupList.Length == 0) && (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))) {
                isInGroup = true;
            }

            if (isInGroup) {
                int findext = x.FileName.LastIndexOf(x.FileExtension, System.StringComparison.Ordinal);
                if (findext < 0) {
                    continue;
                }

                var tempFi = new FileInfo(ss.ResolvedDocumentFolder);
                string path = absolutePath + tempFi.Name + "/" + x.ID;
                string fileTitle = x.FileName.Remove(findext);
                if (IsImageFileType(x.FileExtension)) {
                    if ((ViewState["HideImages"] != null) && ((bool)ViewState["HideImages"] == true)) {
                        fileTitle = "<a href='" + path + x.FileExtension + "' target='_blank' class='cursor-pointer hidden-img-lnk' title='View Image'>" + fileTitle + "</a>";
                    }
                    else {
                        string imgLink = "<a href='" + path + x.FileExtension + "' target='_blank' class='cursor-pointer' title='View Image'><img alt='preview' src='" + path + x.FileExtension + "' style='max-height: 100px;' /></a><br />";
                        fileTitle = imgLink + fileTitle;
                    }
                }
                else if (IsAudioVideoFileType(x.FileExtension)) {
                    string tagType = "audio";
                    string mediaType = "audio/mpeg";
                    string height = "50";

                    string imagePath = absolutePath + "App_Themes/" + _sitetheme + "/App/play.png";
                    string onClick = "onclick='PlaySong(this, \"" + tagType + "\", \"" + mediaType + "\", \"" + path + x.FileExtension + "\", \"" + height + "\");'";
                    string fn = "<div class='float-left'>" + fileTitle + "</div>";
                    fileTitle = "<div class='float-right pad-right pad-left equalizer'></div>" + fn + "<img id='" + x.ID + "' alt='Play_Stop' src='" + imagePath + "' title='Play/Stop' class='margin-left float-left cursor-pointer audio' " + onClick + " style='margin-top: 1px;' />";
                }

                bool isGuid = false;
                Guid gTemp = new Guid();
                if (Guid.TryParse(x.Folder, out gTemp)) {
                    if (x.Folder != memberId)
                        isGuid = true;
                }

                if (!isGuid) {
                    drFile["TitleSort"] = x.FileName;
                    drFile["Size"] = x.FileSize;
                    drFile["ID"] = x.ID;
                    drFile["UploadDate"] = getTime(x.UploadDate);
                    drFile["Type"] = x.FileExtension;

                    if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
                        drFile["Album"] = GetAlbum(x.Folder);
                    }
                    else {
                        drFile["Album"] = "No Group";
                    }

                    drFile["Path"] = path + FileDrive.NewFileExt;
                    drFile["SizeSorted"] = CalculateFileSizeSort(x.FileSize);
                    drFile["TitleEdit"] = x.FileName.Remove(findext);
                    drFile["Title"] = GetFileImage(x.FileExtension) + fileTitle;
                    dtFiles.Rows.Add(drFile);
                }
            }
            //dtFiles.Rows.Add(drFile);
        }
        var dvFiles = new DataView(dtFiles);
        return dvFiles;
    }

    private static double CalculateFileSizeSort(string FileInBytes) {
        double outBytes = 0.0d;
        string strSize = "00";
        if (FileInBytes.ToLower().Contains("kb")) {
            FileInBytes = FileInBytes.ToLower().Replace("kb", "").Trim();
            if (double.TryParse(FileInBytes, out outBytes))
                strSize = Math.Round((outBytes * 1024), 2).ToString();
        }
        else if (FileInBytes.ToLower().Contains("mb")) {
            FileInBytes = FileInBytes.ToLower().Replace("mb", "").Trim();
            if (double.TryParse(FileInBytes, out outBytes))
                strSize = Math.Round((outBytes * 1024) * 1024, 2).ToString();
        }
        else if (FileInBytes.ToLower().Contains("gb")) {
            FileInBytes = FileInBytes.ToLower().Replace("gb", "").Trim();
            if (double.TryParse(FileInBytes, out outBytes))
                strSize = Math.Round((outBytes * 1024) * 1024 * 1024, 2).ToString();
        }
        else if (FileInBytes.ToLower().Contains("tb")) {
            FileInBytes = FileInBytes.ToLower().Replace("tb", "").Trim();
            if (double.TryParse(FileInBytes, out outBytes))
                strSize = Math.Round((outBytes * 1024) * 1024 * 1024 * 1024, 2).ToString();
        }
        else {
            if (FileInBytes.ToLower().Contains("b"))
                strSize = FileInBytes.ToLower().Replace("b", "").Trim();
            else
                strSize = FileInBytes;
        }

        if (double.TryParse(strSize, out outBytes))
            return outBytes;
        return 0.0d;
    }

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
        string path = absolutePath + "Standard_Images/FileTypes/";
        if ((extension.ToLower() == ".png") || (extension.ToLower() == ".bmp") || (extension.ToLower() == ".jpg")
                 || (extension.ToLower() == ".jpeg") || (extension.ToLower() == ".jpe") || (extension.ToLower() == ".jfif")
                 || (extension.ToLower() == ".tif") || (extension.ToLower() == ".tiff") || (extension.ToLower() == ".gif")
                 || (extension.ToLower() == ".tga")) {
            if ((ViewState["HideImages"] != null) && ((bool)ViewState["HideImages"] == true)) {
                return "<img alt='filetype' src='" + path + "image.png' class='float-left pad-right' style='height:16px' />";
            }
            else {
                return "";
            }
        }
        else if ((extension.ToLower() == ".doc") || (extension.ToLower() == ".docx")
                 || (extension.ToLower() == ".dotx") || (extension.ToLower() == ".dotm")) {
            return "<img alt='filetype' src='" + path + "word.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".xlsx") || (extension.ToLower() == ".xlsm") || (extension.ToLower() == ".xlam")
                 || (extension.ToLower() == ".xltx") || (extension.ToLower() == ".xlsb") || (extension.ToLower() == ".xls")) {
            return "<img alt='filetype' src='" + path + "excel.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".pptx") || (extension.ToLower() == ".pptm") || (extension.ToLower() == ".potx")
                || (extension.ToLower() == ".potm") || (extension.ToLower() == ".ppam") || (extension.ToLower() == ".ppsm")) {
            return "<img alt='filetype' src='" + path + "powerpoint.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".pptx") || (extension.ToLower() == ".pptm") || (extension.ToLower() == ".potx")
                || (extension.ToLower() == ".potm") || (extension.ToLower() == ".ppam") || (extension.ToLower() == ".ppsm")
                || (extension.ToLower() == ".ppt")) {
            return "<img alt='filetype' src='" + path + "powerpoint.png' class='float-left pad-right' style='height:16px' />";
        }
        else if (extension.ToLower() == ".pdf") {
            return "<img alt='filetype' src='" + path + "pdf.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".html") || (extension.ToLower() == ".htm")) {
            return "<img alt='filetype' src='" + path + "html.png' class='float-left pad-right' style='height:16px' />";
        }
        else if (extension.ToLower() == ".txt") {
            return "<img alt='filetype' src='" + path + "page_code.png' class='float-left pad-right' style='height:16px' />";
        }
        else if ((extension.ToLower() == ".zip") || (extension.ToLower() == ".rar") || (extension.ToLower() == ".iso")) {
            return "<img alt='filetype' src='" + path + "zip.png' class='float-left pad-right' style='height:16px' />";
        }
        else if (extension.ToLower() == ".mp3") {
            return "<img alt='filetype' src='" + path + "music.png' class='float-left pad-right' style='height:16px' />";
            return "";
        }
        else if ((extension.ToLower() == ".avi") || (extension.ToLower() == ".mp4")) {
            return "<img alt='filetype' src='" + path + "video.png' class='float-left pad-right' style='height:16px' />";
        }
        return "<img alt='filetype' src='" + path + "unknown.png' class='float-left pad-right' style='height:16px' />";
    }

    private string GetAlbum(string album) {
        string temp = "";

        if (string.IsNullOrEmpty(album))
            temp = "Root Directory";
        else switch (album) {
                case "-":
                    temp = "Root Directory";
                    break;
                default:
                    temp = album == member.UserId ? "-" : album.Replace("_", " ");
                    break;
            }

        return temp;
    }

    private string getTime(DateTime postDate) {
        DateTime now = DateTime.Now;
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

    protected void DownloadFile(Guid id) {
        bool canContinue = true;
        string filePath = docs.GetFileNamePath(id);
        FileInfo fi = new FileInfo(filePath);
        string realFilePath = filePath.Replace(fi.Name, id.ToString() + FileDrive.NewFileExt);

        if (FileDrive.FileExtOk(fi.Extension)) {
            realFilePath = filePath.Replace(fi.Name, id.ToString() + fi.Extension);
        }

        if (canContinue) {
            string strFileName = Path.GetFileName(filePath);
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
            Response.Clear();
            Response.TransmitFile(realFilePath);
            Response.Flush();
            Response.End();
        }
        else {
            Response.Redirect("~/Apps/FileDrive/FileDrive.aspx");
        }
    }

    private void ResetSelected() {
        HiddenField1_documents.Value = string.Empty;
        lbl_movetofolder_documents.Enabled = false;
        lbl_movetofolder_documents.Visible = false;
        dd_myAlbums_documents.Enabled = false;
        dd_myAlbums_documents.Visible = false;
        lbtn_moveFile_documents.Enabled = false;
        lbtn_moveFile_documents.Visible = false;
        lbtn_selectAll_documents.Text = "Select All";
    }

    protected void dd_groups_Changed(object sender, EventArgs e) {
        hf_ddgroups.Value = dd_groups.SelectedValue;

        hf_folderchange_documents.Value = "0";
        hf_category_documents.Value = "";
        lbl_currFolderName.Text = "Root Directory";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, "asc");
        LoadDocuments();
        InitializeSort(false);

        dd_myAlbums_documents.Items.Clear();
        HiddenField1_documents.Value = string.Empty;
        lbl_movetofolder_documents.Enabled = false;
        lbl_movetofolder_documents.Visible = false;
        dd_myAlbums_documents.Enabled = false;
        dd_myAlbums_documents.Visible = false;
        lbtn_moveFile_documents.Enabled = false;
        lbtn_moveFile_documents.Visible = false;
        lbtn_selectAll_documents.Text = "Select All";
    }

    protected void CheckBoxIndv_CheckChanged(object sender, EventArgs e) {
        var chk = (CheckBox)sender;
        string filename = chk.Text;
        string[] filelist = HiddenField1_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        if (filelist.Contains(filename)) {
            var templist = new List<string>();
            for (var i = 0; i < filelist.Length; i++) {
                if (!filelist[i].Contains(filename)) {
                    templist.Add(filelist[i]);
                }
            }
            HiddenField1_documents.Value = "";
            if (templist.Count > 0) {
                foreach (string file in templist) {
                    HiddenField1_documents.Value += file + ServerSettings.StringDelimiter;
                }
            }
        }
        else {
            HiddenField1_documents.Value += filename + ServerSettings.StringDelimiter;
        }

        if (!string.IsNullOrEmpty(HiddenField1_documents.Value)) {
            lbl_movetofolder_documents.Enabled = true;
            lbl_movetofolder_documents.Visible = true;
            dd_myAlbums_documents.Enabled = true;
            dd_myAlbums_documents.Visible = true;
            lbtn_moveFile_documents.Enabled = true;
            lbtn_moveFile_documents.Visible = true;
            dd_myAlbums_documents.Items.Clear();

            if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
                docs.GetAllFolders(hf_ddgroups.Value);
                BuildFolderDropDowns(dd_myAlbums_documents);
            }
            else {
                List<string> userGroups = member.GroupList;
                foreach (string g in userGroups) {
                    string groupName = _groups.GetGroupName_byID(g);
                    var itemstart = new ListItem(groupName, g);
                    dd_myAlbums_documents.Items.Add(itemstart);
                }
            }
        }
        else {
            lbl_movetofolder_documents.Enabled = false;
            lbl_movetofolder_documents.Visible = false;
            dd_myAlbums_documents.Enabled = false;
            dd_myAlbums_documents.Visible = false;
            lbtn_moveFile_documents.Enabled = false;
            lbtn_moveFile_documents.Visible = false;
        }
    }

    #endregion


    #region GridView Sorting methods

    public string SortOrder {
        get {
            if (hf_sortcol_documents.Value == hf_sortcolprev) {
                if (ViewState["sortOrder"].ToString() == "desc") {
                    ViewState["sortOrder"] = "asc";
                }
                else {
                    ViewState["sortOrder"] = "desc";
                }
            }
            else if (hf_sortcolprev != "") {
                ViewState["sortOrder"] = "asc";
            }

            if (ViewState["sortOrder"] != null) {
                return ViewState["sortOrder"].ToString();
            }
            else {
                ViewState["sortOrder"] = "asc";
                return ViewState["sortOrder"].ToString();
            }
        }
        set { ViewState["sortOrder"] = value; }
    }

    private void InitializeSort(bool pagerevent) {
        hf_sortcolprev = "";
        if (!pagerevent) {
            LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        }
        if (!string.IsNullOrEmpty(HiddenField1_documents.Value)) {
            lbl_movetofolder_documents.Enabled = true;
            lbl_movetofolder_documents.Visible = true;
            dd_myAlbums_documents.Enabled = true;
            dd_myAlbums_documents.Visible = true;
            lbtn_moveFile_documents.Enabled = true;
            lbtn_moveFile_documents.Visible = true;
            dd_myAlbums_documents.Items.Clear();
            docs.GetAllFolders(hf_ddgroups.Value);
            BuildFolderDropDowns(dd_myAlbums_documents);
        }
        else {
            lbl_movetofolder_documents.Enabled = false;
            lbl_movetofolder_documents.Visible = false;
            dd_myAlbums_documents.Enabled = false;
            dd_myAlbums_documents.Visible = false;
            lbtn_moveFile_documents.Enabled = false;
            lbtn_moveFile_documents.Visible = false;
        }
        GetNewFileNotification();
        row = GV_Files_documents.HeaderRow;
        if (hf_sortcol_documents.Value == "0") {
            var x = (HtmlTableCell)row.FindControl("td_filename");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "1") {
            var x = (HtmlTableCell)row.FindControl("td_ext");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "2") {
            var x = (HtmlTableCell)row.FindControl("td_size");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "3") {
            var x = (HtmlTableCell)row.FindControl("td_date");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "5") {
            var x = (HtmlTableCell)row.FindControl("td_album");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
    }

    private void InitializeSort2(bool pagerevent) {
        hf_sortcolprev = "";
        if (!pagerevent) {
            LoadFiles2(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        }
        if (!string.IsNullOrEmpty(HiddenField1_documents.Value)) {
            lbl_movetofolder_documents.Enabled = true;
            lbl_movetofolder_documents.Visible = true;
            dd_myAlbums_documents.Enabled = true;
            dd_myAlbums_documents.Visible = true;
            lbtn_moveFile_documents.Enabled = true;
            lbtn_moveFile_documents.Visible = true;
            dd_myAlbums_documents.Items.Clear();
            docs.GetAllFolders(hf_ddgroups.Value);
            BuildFolderDropDowns(dd_myAlbums_documents);
        }
        else {
            lbl_movetofolder_documents.Enabled = false;
            lbl_movetofolder_documents.Visible = false;
            dd_myAlbums_documents.Enabled = false;
            dd_myAlbums_documents.Visible = false;
            lbtn_moveFile_documents.Enabled = false;
            lbtn_moveFile_documents.Visible = false;
        }
        GetNewFileNotification();
        row = GV_Files_documents.HeaderRow;
        if (hf_sortcol_documents.Value == "0") {
            var x = (HtmlTableCell)row.FindControl("td_filename");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "1") {
            var x = (HtmlTableCell)row.FindControl("td_ext");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "2") {
            var x = (HtmlTableCell)row.FindControl("td_size");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "3") {
            var x = (HtmlTableCell)row.FindControl("td_date");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
        else if (hf_sortcol_documents.Value == "5") {
            var x = (HtmlTableCell)row.FindControl("td_album");
            if (ViewState["sortOrder"].ToString() == "asc")
                x.Attributes["class"] += " active asc";
            else
                x.Attributes["class"] += " active desc";
        }
    }

    protected void lbtn_filename_Click(object sender, EventArgs e) {
        hf_sortcolprev = hf_sortcol_documents.Value;
        hf_sortcol_documents.Value = "0";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_filename");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_ext_Click(object sender, EventArgs e) {
        hf_sortcolprev = hf_sortcol_documents.Value;
        hf_sortcol_documents.Value = "1";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_ext");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_size_Click(object sender, EventArgs e) {
        hf_sortcolprev = hf_sortcol_documents.Value;
        hf_sortcol_documents.Value = "2";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_size");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_date_Click(object sender, EventArgs e) {
        hf_sortcolprev = hf_sortcol_documents.Value;
        hf_sortcol_documents.Value = "3";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_date");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_album_Click(object sender, EventArgs e) {
        hf_sortcolprev = hf_sortcol_documents.Value;
        hf_sortcol_documents.Value = "4";

        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, SortOrder);
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_album");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    private void BuildFolderDropDowns(DropDownList dd) {
        var itemmain = new ListItem("Root Directory", "-");
        dd.Items.Add(itemmain);
        dd.SelectedIndex = 0;

        string memberId = member.UserId;
        var itemMyFolder = new ListItem("Personal Folder", memberId);
        dd.Items.Add(itemMyFolder);

        foreach (FileDriveFolders_Coll t in docs.folders_coll) {
            bool isGuid = false;
            Guid gTemp = new Guid();
            if (Guid.TryParse(t.FolderName, out gTemp))
                isGuid = true;

            if ((t.FolderName != memberId) && (!isGuid)) {
                var item = new ListItem(GetAlbum(t.FolderName), t.FolderName);
                if (!dd.Items.Contains(item)) {
                    dd.Items.Add(item);
                }
            }
        }
    }

    #endregion


    #region Top GridView Controls

    protected void hf_searchFiles_ValueChanged(object sender, EventArgs e) {
        ResetSelected();
        if ((hf_searchFiles_documents.Value == "delete") || (hf_searchFiles_documents.Value == "movefiles")
            || (hf_searchFiles_documents.Value == "cancel") || (hf_searchFiles_documents.Value == "update")
            || (hf_searchFiles_documents.Value == "deletefolder") || (hf_searchFiles_documents.Value == "refreshfiles")) {
            InitializeSort(false);
        }
        else if (hf_searchFiles_documents.Value == "create") {
            InitializeSort(false);
        }
        else if ((string.IsNullOrEmpty(tb_search_documents.Text)) || (tb_search_documents.Text == "Search for documents")) {
            InitializeSort(false);
        }
        else {
            docs.GetAllFiles(hf_ddgroups.Value);
            var temp = new List<FileDriveDocuments_Coll>();
            foreach (var x in docs.documents_coll) {
                char[] trimBoth = { ' ' };
                string findValue = tb_search_documents.Text.ToLower().TrimEnd(trimBoth);
                findValue = findValue.TrimStart(trimBoth);
                if (x.FileName.ToLower().Contains(findValue)) {
                    var coll = new FileDriveDocuments_Coll(x.ID, x.FileName, x.FileExtension, x.FileSize, x.FilePath,
                                                     x.Comment, x.UploadDate.ToString(), x.Folder, x.owner,
                                                     x.GroupName);
                    temp.Add(coll);
                }

                else if (x.Folder.ToLower().Contains(findValue)) {
                    var coll = new FileDriveDocuments_Coll(x.ID, x.FileName, x.FileExtension, x.FileSize, x.FilePath,
                                                     x.Comment, x.UploadDate.ToString(), x.Folder, x.owner,
                                                     x.GroupName);
                    temp.Add(coll);
                }
            }
            LoadFilesSearch(ref GV_Files_documents, hf_sortcol_documents.Value, "asc", temp);
        }

        hf_searchFiles_documents.Value = "";
    }

    protected void imgbtn_del_Click(object sender, EventArgs e) {
        string[] filelist = HiddenField1_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < filelist.Length; i++) {
            try {
                string p = docs.GetFileNamePath(Guid.Parse(filelist[i]));
                var info = new FileInfo(p);
                string realFilePath = info.FullName.Replace(info.Name, filelist[i] + FileDrive.NewFileExt);
                if (FileDrive.FileExtOk(info.Extension)) {
                    realFilePath = info.FullName.Replace(info.Name, filelist[i] + info.Extension);
                }
                if (File.Exists(realFilePath)) {
                    File.Delete(realFilePath);
                }
            }
            catch {
            }
            docs.deleteFile(Guid.Parse(filelist[i]));
        }
            uuf.addFlag("app-filedrive", "");
        ResetSelected();
        InitializeSort(false);
        string autoupdate = "document.getElementById('" + hf_searchFiles_documents.ClientID + "').value = 'delete';";
        autoupdate += "__doPostBack('" + hf_searchFiles_documents.ClientID + "', '');";
        RegisterPostbackScripts.RegisterStartupScript(this, autoupdate);
    }

    protected void lbtn_moveFile_Click(object sender, EventArgs e) {
        string[] filelist = HiddenField1_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < filelist.Length; i++) {
            if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
                if (docs.GetFolderForMove(Guid.Parse(filelist[i])) != dd_myAlbums_documents.SelectedValue) {
                    docs.updateFolderName(Guid.Parse(filelist[i]), dd_myAlbums_documents.SelectedValue, hf_ddgroups.Value);
                }
            }
            else {
                docs.updateFolderName(Guid.Parse(filelist[i]), "-", dd_myAlbums_documents.SelectedValue);
            }
        }
        ResetSelected();
        InitializeSort(false);
        string autoupdate = "document.getElementById('" + hf_searchFiles_documents.ClientID + "').value = 'movefiles';";
        autoupdate += "__doPostBack('" + hf_searchFiles_documents.ClientID + "', '');";
        RegisterPostbackScripts.RegisterStartupScript(this, autoupdate);
    }

    protected void lbtn_selectAll_Click(object sender, EventArgs e) {
        if (lbtn_selectAll_documents.Text == "Select All") {
            HiddenField1_documents.Value = string.Empty;
            foreach (
                var chk in from GridViewRow r in GV_Files_documents.Rows select (CheckBox)r.FindControl("CheckBoxIndv")
                ) {
                chk.Checked = true;
                string filename = chk.Text;
                string[] filelist = HiddenField1_documents.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if (filelist.Contains(filename)) {
                    var templist = new List<string>();
                    for (int i = 0; i < filelist.Length; i++) {
                        if (!filelist[i].Contains(filename)) {
                            templist.Add(filelist[i]);
                        }
                    }
                    HiddenField1_documents.Value = "";
                    if (templist.Count > 0) {
                        foreach (string file in templist) {
                            HiddenField1_documents.Value += file + ServerSettings.StringDelimiter;
                        }
                    }
                }
                else {
                    HiddenField1_documents.Value += filename + ServerSettings.StringDelimiter;
                }
                lbl_movetofolder_documents.Enabled = true;
                lbl_movetofolder_documents.Visible = true;
                dd_myAlbums_documents.Enabled = true;
                dd_myAlbums_documents.Visible = true;
                lbtn_moveFile_documents.Enabled = true;
                lbtn_moveFile_documents.Visible = true;
                dd_myAlbums_documents.Items.Clear();

                if (!string.IsNullOrEmpty(hf_ddgroups.Value)) {
                    docs.GetAllFolders(hf_ddgroups.Value);
                    BuildFolderDropDowns(dd_myAlbums_documents);
                }
                else {
                    List<string> userGroups = member.GroupList;
                    foreach (string g in userGroups) {
                        string groupName = _groups.GetGroupName_byID(g);
                        var itemstart = new ListItem(groupName, g);
                        dd_myAlbums_documents.Items.Add(itemstart);
                    }
                }
            }
            lbtn_selectAll_documents.Text = "Deselect All";
        }

        else {
            foreach (
                var chk in from GridViewRow r in GV_Files_documents.Rows select (CheckBox)r.FindControl("CheckBoxIndv")
                ) {
                chk.Checked = false;
            }
            ResetSelected();
        }
    }

    protected void btn_refresh_Click(object sender, EventArgs e) {
        ResetSelected();
        ViewState["sortOrder"] = "asc";
        hf_sortcol_documents.Value = "0";
        LoadFiles(ref GV_Files_documents, hf_sortcol_documents.Value, "asc");
        row = GV_Files_documents.HeaderRow;
        var x = (HtmlTableCell)row.FindControl("td_filename");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    private void GetNewFileNotification() {
        int count =
            (from t in docs.documents_coll let now = DateTime.Now select now.Subtract(t.UploadDate)).Count(
                final => final.Days <= 1);

        lbl_fileNoti_documents.Text = count + " file(s) have been uploaded recently";
    }

    #endregion


    #region FileUpload

    protected void btnFileUpload_OnClick(object sender, EventArgs e) {
        if (FileUploadControl.HasFile) {
            try {
                IIdentity userID = HttpContext.Current.User.Identity;
                foreach (HttpPostedFile file in FileUploadControl.PostedFiles) {
                    string groupname = hf_ddgroups.Value;
                    var filesql = new FileDrive(userID.Name);
                    string pathsql = ss.ResolvedDocumentFolder;
                    string fileName_temp1sql = Path.GetFileName(file.FileName);
                    string fileNamesql = removeRegex(fileName_temp1sql);
                    string tempext = Path.GetExtension(fileNamesql);
                    string fileNameId = Guid.NewGuid().ToString();

                    string p = Path.Combine(pathsql, fileNameId + FileDrive.NewFileExt);
                    if (FileDrive.FileExtOk(tempext)) {
                        p = Path.Combine(pathsql, fileNameId + tempext);
                    }

                    file.SaveAs(p);
                    var info = new FileInfo(p);
                    if (info.Exists) {
                        filesql.addFile(fileNameId, fileNamesql, tempext, HelperMethods.FormatBytes(info.Length), pathsql, string.Empty, "-", groupname, false);
                    }
                }
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }

        Response.Redirect(Request.RawUrl);
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

}