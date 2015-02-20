#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

public partial class SiteTools_FileManager : Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();

    private List<FileInfo> _infolist;
    private string _viewtype = "all";

    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                    lbtn_close.NavigateUrl = "FileManager.aspx?toolView=true";
                }

                if (!IsPostBack) {
                    dd_viewtype.SelectedIndex = 0;
                    if (_ss.LockFileManager) {
                        ltl_locked.Text = HelperMethods.GetLockedByMessage();
                    }

                    if (!string.IsNullOrEmpty(Request.QueryString["edit"])) {
                        string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");
                        string filename = HttpUtility.UrlDecode(drive + Request.QueryString["file"]);
                        if (HelperMethods.ConvertBitToBoolean((Request.QueryString["edit"])) && (!_ss.LockFileManager)) {
                            pnl1.Enabled = false;
                            pnl1.Visible = false;
                            pnl2.Enabled = true;
                            pnl2.Visible = true;
                            var sr = new StreamReader(filename);

                            var fi = new FileInfo(filename);
                            if ((HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) ||
                                (fi.Extension.ToLower() == ".css")) {
                                lbtn_save.Enabled = true;
                                lbtn_save.Visible = true;
                                lbl_messageRead.Text = "Edit Mode";
                                string JavaScript = "$('#editor').css('display', 'block');LoadEditor();UnescapeJavascriptCode(" + HttpUtility.JavaScriptStringEncode(sr.ReadToEnd(), true) + ");";
                                RegisterPostbackScripts.RegisterStartupScript(this, JavaScript);
                            }
                            else {
                                lbtn_save.Enabled = false;
                                lbtn_save.Visible = false;
                                lbl_messageRead.Text = "Edit Mode Only Available to the Administrator";
                                ltl_code.Text = "<pre class=\"brush: js\">" + sr.ReadToEnd() + "</pre>";
                            }
                            lbl_currfile.Text = filename;
                            sr.Close();
                        }
                        else {
                            pnl1.Enabled = false;
                            pnl1.Visible = false;
                            pnl2.Enabled = true;
                            pnl2.Visible = true;
                            lbtn_save.Enabled = false;
                            lbtn_save.Visible = false;
                            lbl_currfile.Text = filename;
                            lbl_messageRead.Text = "Read Only Mode";
                            StreamReader sr;
                            try {
                                sr = new StreamReader(filename);
                                ltl_code.Text = "<pre class=\"brush: js\">" + sr.ReadToEnd() + "</pre>";
                                sr.Close();
                            }
                            catch { }
                        }
                    }
                    else {
                        LoadScripts(ref GV_Script, "0", "asc");
                    }
                }

                GetCallBack();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void GetCallBack() {
        string controlName = Request.Params["__EVENTTARGET"];
        switch (controlName) {
            case "MainContent_lbtn_save":
                string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");
                string path = drive + Request.QueryString["file"].Replace("!", "\\");
                var fi = new FileInfo(path);
                if (!_ss.LockFileManager) {
                    if ((fi.Directory != null) && (fi.Extension.ToLower() == ".css") && (fi.Directory.Name.ToLower() != "standard")) {
                        string editorText = HttpUtility.UrlDecode(hidden_editor.Value.Trim());
                        if (!string.IsNullOrEmpty(editorText)) {
                            if (File.Exists(path)) {
                                File.Delete(path);
                            }
                            var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
                            fs.Close();
                            var sw = new StreamWriter(path, true);
                            sw.Write(HttpUtility.UrlDecode(editorText));
                            sw.Close();
                        }

                        ServerSettings.PageToolViewRedirect(this.Page, "FileManager.aspx");
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You do not have permission to save this file');");
                    }
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You do not have permission to save this file');");
                }
                break;
        }
    }

    private void RefreshDatabase(string folder) {
        foreach (string f in Directory.GetFiles(folder)) {
            var info = new FileInfo(f);
            if (_infolist.Contains(info)) continue;
            if ((IsOkFileType(info.Extension)) && (!info.Name.ToLower().Contains(".min.js")))
                _infolist.Add(info);
        }
        string[] directories = Directory.GetDirectories(folder);
        foreach (string d in directories)
            RefreshDatabase(d);
    }

    #endregion

    #region GridView Properties Methods

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        LoadScripts(ref GV_Script, "0", "asc");
    }

    protected void dd_viewtype_SelectedIndexChanged(object sender, EventArgs e) {
        _viewtype = dd_viewtype.SelectedValue;
        LoadScripts(ref GV_Script, "0", "asc");
    }

    protected void GV_Script_RowCommand(object sender, GridViewCommandEventArgs e) {
        switch (e.CommandName) {
            case "DownloadScript":
                DownloadFile(e.CommandArgument.ToString());
                LoadScripts(ref GV_Script, "0", "asc");
                break;
            //case "DeleteScript":
            //    DeleteFile(e.CommandArgument.ToString());
            //    LoadScripts(ref GV_Script, "0", "asc");
            //    break;
        }
    }

    public void LoadScripts(ref GridView gv, string sortExp, string sortDir) {
        _infolist = new List<FileInfo>();
        RefreshDatabase(ServerSettings.GetServerMapLocation);
        DataView dvFiles = GetScripts();
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
    }

    public DataView GetScripts() {
        var dtFiles = new DataTable();
        dtFiles.Columns.Add(new DataColumn("Title"));
        dtFiles.Columns.Add(new DataColumn("Size"));
        dtFiles.Columns.Add(new DataColumn("Type"));
        dtFiles.Columns.Add(new DataColumn("UploadDate"));
        dtFiles.Columns.Add(new DataColumn("Path"));
        dtFiles.Columns.Add(new DataColumn("EditClass"));
        dtFiles.Columns.Add(new DataColumn("DownloadClass"));
        dtFiles.Columns.Add(new DataColumn("PreviewClass"));
        dtFiles.Columns.Add(new DataColumn("RowCount"));

        int count = 1;
        foreach (var t in _infolist) {
            if ((_viewtype == t.Extension) || (_viewtype == "all")) {
                DataRow drFile = dtFiles.NewRow();
                if (t.Directory != null) {
                    string parentFolder = t.FullName.Replace(t.Directory.Root.Name, "\\");

                    if (t.Directory.Parent != null && ((t.Directory.Parent.ToString() == "auth") ||
                                                       (t.Directory.Parent.ToString() == "auth"))) {
                        parentFolder = string.Empty;
                    }

                    if ((tb_search.Text.ToLower() != "search files") && (tb_search.Text != "")) {
                        if ((tb_search.Text.ToLower().Contains(parentFolder.ToLower()))
                            || (parentFolder.ToLower().Contains(tb_search.Text.ToLower()))
                            || (tb_search.Text.ToLower().Contains(t.Length.ToString(CultureInfo.InvariantCulture))) ||
                            (tb_search.Text.ToLower().Contains(t.LastAccessTime.ToString(CultureInfo.InvariantCulture)))) {
                            drFile["Title"] = parentFolder;
                            drFile["Size"] = HelperMethods.FormatBytes(t.Length);
                            drFile["Type"] = t.Extension;
                            drFile["UploadDate"] = getTime(t.LastAccessTime);
                            drFile["Path"] = HttpUtility.UrlEncode(parentFolder);
                            if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                                drFile["Path"] = HttpUtility.UrlEncode(parentFolder + "&toolView=true");
                            }

                            if (!_ss.LockFileManager) {
                                drFile["EditClass"] = "td-edit-btn margin-left-sml margin-right-sml RandomActionBtns";
                                drFile["DownloadClass"] = "img-download pad-all-sml";
                                drFile["PreviewClass"] = "img-view pad-all-sml margin-left-sml RandomActionBtns";
                            }
                            else {
                                drFile["EditClass"] = "display-none";
                                drFile["DownloadClass"] = "display-none";
                                drFile["PreviewClass"] = "display-none";
                            }

                            drFile["RowCount"] = count.ToString();
                            dtFiles.Rows.Add(drFile);

                            count++;
                        }
                    }
                    else {
                        drFile["Title"] = parentFolder;
                        drFile["Size"] = HelperMethods.FormatBytes(t.Length);
                        drFile["Type"] = t.Extension;
                        drFile["UploadDate"] = getTime(t.LastAccessTime);
                        drFile["Path"] = HttpUtility.UrlEncode(parentFolder);
                        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                            drFile["Path"] = HttpUtility.UrlEncode(parentFolder + "&toolView=true");
                        }

                        if (!_ss.LockFileManager) {
                            drFile["EditClass"] = "td-edit-btn margin-left-sml margin-right-sml RandomActionBtns";
                            drFile["DownloadClass"] = "img-download pad-all-sml";
                            drFile["PreviewClass"] = "img-view pad-all-sml margin-left-sml RandomActionBtns";
                        }
                        else {
                            drFile["EditClass"] = "display-none";
                            drFile["DownloadClass"] = "display-none";
                            drFile["PreviewClass"] = "display-none";
                        }

                        drFile["RowCount"] = count.ToString();
                        dtFiles.Rows.Add(drFile);

                        count++;
                    }
                }
            }
        }
        var dvFiles = new DataView(dtFiles);
        return dvFiles;
    }

    public void LoadScripts(ref GridView gv, string sortExp, string sortDir, string type, List<string> types) {
        _infolist = new List<FileInfo>();
        RefreshDatabase(ServerSettings.GetServerMapLocation);
        DataView dvFiles = GetScripts_Type(type, types);
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
    }

    public DataView GetScripts_Type(string type, List<string> types) {
        var dtFiles = new DataTable();
        dtFiles.Columns.Add(new DataColumn("Title"));
        dtFiles.Columns.Add(new DataColumn("Size"));
        dtFiles.Columns.Add(new DataColumn("Type"));
        dtFiles.Columns.Add(new DataColumn("UploadDate"));
        dtFiles.Columns.Add(new DataColumn("Path"));
        dtFiles.Columns.Add(new DataColumn("EditClass"));
        dtFiles.Columns.Add(new DataColumn("DownloadClass"));
        dtFiles.Columns.Add(new DataColumn("RowCount"));

        int count = 1;
        foreach (var t in _infolist) {
            DataRow drFile = dtFiles.NewRow();
            if (t.Directory != null) {
                string parentFolder = t.FullName.Replace(t.Directory.Root.Name, "\\");

                if (t.Directory.Parent != null && ((t.Directory.Parent.ToString() == "auth") ||
                                                   (t.Directory.Parent.ToString() == "auth"))) {
                    parentFolder = string.Empty;
                }

                bool cancontinue = types.All(x => !parentFolder.ToLower().Contains(x.ToLower()));

                if (cancontinue) {
                    if (parentFolder.ToLower().Contains(type.ToLower())) {
                        drFile["Title"] = parentFolder;
                        drFile["Size"] = HelperMethods.FormatBytes(t.Length);
                        drFile["Type"] = t.Extension;
                        drFile["UploadDate"] = getTime(t.LastAccessTime);
                        drFile["Path"] = HttpUtility.UrlEncode(parentFolder);
                        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                            drFile["Path"] = HttpUtility.UrlEncode(parentFolder + "&toolView=true");
                        }

                        if (!_ss.LockFileManager) {
                            drFile["EditClass"] = "td-edit-btn margin-left-sml margin-right-sml RandomActionBtns";
                            drFile["DownloadClass"] = "img-download pad-all-sml";
                        }
                        else {
                            drFile["EditClass"] = "display-none";
                            drFile["DownloadClass"] = "display-none";
                        }

                        drFile["RowCount"] = count.ToString();
                        dtFiles.Rows.Add(drFile);

                        count++;
                    }
                }
            }
        }
        var dvFiles = new DataView(dtFiles);
        return dvFiles;
    }

    private static bool IsOkFileType(string extension) {
        bool ok = (extension.ToLower() == ".css") || (extension.ToLower() == ".js");
        return ok;
    }

    private string getTime(DateTime postDate) {
        DateTime now = DateTime.Now;
        TimeSpan final = now.Subtract(postDate);
        string time;
        if (final.Days > 2) {
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

    protected void DownloadFile(string path) {
        string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");
        string filePath = drive + path;
        var fi = new FileInfo(filePath);
        if (fi.Directory != null) filePath = fi.Directory.Root.Name.Replace("\\", "") + path;
        if (!_ss.LockFileManager) {
            if (File.Exists(filePath)) {
                string strFileName = Path.GetFileName(filePath);
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
                Response.Clear();
                Response.WriteFile(filePath);
                Response.End();
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You do not have permission to download this file');");
        }
    }

    protected void DeleteFile(string path) {
        if (!_ss.LockFileManager) {
            string filePath = ServerSettings.GetServerMapLocation + path;
            if (File.Exists(filePath)) {
                File.Delete(path);
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You do not have permission to delete this file');");
        }
    }

    #endregion
}