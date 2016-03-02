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

#endregion

public partial class SiteTools_FileManager : Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();

    private List<FileInfo> _infolist;
    private string _viewtype {
        get {
            if (ViewState["ViewType"] != null && !string.IsNullOrEmpty(ViewState["ViewType"].ToString())) {
                return ViewState["ViewType"].ToString();
            }
            return "all";
        }
        set {
            ViewState["ViewType"] = value;
        }
    }
    private string _searchValue {
        get {
            if (ViewState["SearchValue"] != null && !string.IsNullOrEmpty(ViewState["SearchValue"].ToString())) {
                return ViewState["SearchValue"].ToString();
            }
            return "";
        }
        set {
            ViewState["SearchValue"] = value;
        }
    }

    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                    lbtn_close.NavigateUrl = "FileManager.aspx?toolView=true";
                }

                if (_ss.LockFileManager) {
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                }

                if (!string.IsNullOrEmpty(Request.QueryString["edit"])) {
                    LoadEditFile();
                }
                else {
                    LoadScripts();
                }

                GetCallBack();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void LoadEditFile() {
        if (!IsPostBack) {
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
                    string JavaScript = "$('#editor').css('display', 'block');LoadEditor();UnescapeJavascriptCodeReadOnly(" + HttpUtility.JavaScriptStringEncode(sr.ReadToEnd(), true) + ");";
                    RegisterPostbackScripts.RegisterStartupScript(this, JavaScript);
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
                    string JavaScript = "$('#editor').css('display', 'block');LoadEditor();UnescapeJavascriptCodeReadOnly(" + HttpUtility.JavaScriptStringEncode(sr.ReadToEnd(), true) + ");";
                    RegisterPostbackScripts.RegisterStartupScript(this, JavaScript);
                    sr.Close();
                }
                catch { }
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
                    if ((fi.Directory != null) && ((fi.Extension.ToLower() == ".css") || (fi.Extension.ToLower() == ".js"))) {
                        string editorText = HttpUtility.UrlDecode(hidden_editor.Value.Trim());
                        if (!string.IsNullOrEmpty(editorText)) {
                            if (File.Exists(path)) {
                                File.Delete(path);
                            }
                            var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
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
        _searchValue = string.Empty;
        if (tb_search.Text.ToLower() != "search files") {
            _searchValue = tb_search.Text.ToLower();
        }

        LoadScripts();
    }

    protected void dd_viewtype_SelectedIndexChanged(object sender, EventArgs e) {
        _viewtype = dd_viewtype.SelectedValue;
        LoadScripts();
    }

    public void LoadScripts() {
        _infolist = new List<FileInfo>();
        RefreshDatabase(ServerSettings.GetServerMapLocation);
        IEnumerable<FileInfo> tempList = _infolist.OrderBy(x => x.FullName);

        #region Build File List
        StringBuilder str = new StringBuilder();
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td>");
        str.Append("<td style='min-width: 200px;'>Filename</td>");
        str.Append("<td width='65px'>Ext</td>");
        str.Append("<td width='100px'>Size</td>");
        str.Append("<td width='100px'>Last Accessed</td>");
        if (!_ss.LockFileManager) {
            str.Append("<td width='100px'>Actions</td>");
        }
        str.Append("</tr>");

        int count = 1;
        foreach (FileInfo t in tempList) {
            if ((_viewtype == t.Extension) || (_viewtype == "all")) {
                if (t.Directory != null) {
                    string parentFolder = t.FullName.Replace(t.Directory.Root.Name, "\\");
                    if (t.Directory.Parent != null && ((t.Directory.Parent.ToString() == "auth") || (t.Directory.Parent.ToString() == "auth"))) {
                        parentFolder = string.Empty;
                    }

                    string fileName = t.FullName.Replace(ServerSettings.GetServerMapLocation, "");

                    if ((_searchValue != "search files") && (_searchValue != "")) {
                        if ((_searchValue.Contains(parentFolder.ToLower()))
                            || (parentFolder.ToLower().Contains(_searchValue))
                            || (_searchValue.Contains(t.Length.ToString(CultureInfo.InvariantCulture))) ||
                            (_searchValue.Contains(t.LastAccessTime.ToString(CultureInfo.InvariantCulture)))) {

                            str.Append("<tr class='myItemStyle GridNormalRow'>");
                            str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + count.ToString() + "</td>");
                            str.Append("<td align='left' class='border-right border-bottom'>" + fileName + "</td>");
                            str.Append("<td align='center' class='border-right border-bottom'>" + t.Extension + "</td>");
                            str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.FormatBytes(t.Length) + "</td>");
                            str.Append("<td align='center' class='border-right border-bottom'>" + getTime(t.LastAccessTime) + "</td>");

                            string filePath = HttpUtility.UrlEncode(parentFolder);
                            if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                                filePath = HttpUtility.UrlEncode(parentFolder + "&toolView=true");
                            }

                            if (!_ss.LockFileManager) {
                                string editBtn = "<a href='FileManager.aspx?edit=true&file=" + filePath + "' class='td-edit-btn margin-left-sml margin-right-sml RandomActionBtns' title='Edit'></a>";
                                string downloadBtn = "<a href='#' class='img-download pad-all-sml' title='Download' onclick=\"DownloadFile('" + HttpUtility.UrlEncode(parentFolder) + "');return false;\"></a>";
                                string previewBtn = "<a href='FileManager.aspx?edit=false&file=" + filePath + "' class='img-view pad-all-sml margin-left-sml RandomActionBtns' title='View'></a>";
                                str.Append("<td align='center' class='border-right border-bottom'>" + previewBtn + editBtn + downloadBtn + "</td>");
                            }

                            str.Append("</tr>");
                            count++;
                        }
                    }
                    else {
                        str.Append("<tr class='myItemStyle GridNormalRow'>");
                        str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + count.ToString() + "</td>");
                        str.Append("<td align='left' class='border-right border-bottom'>" + fileName + "</td>");
                        str.Append("<td align='center' class='border-right border-bottom'>" + t.Extension + "</td>");
                        str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.FormatBytes(t.Length) + "</td>");
                        str.Append("<td align='center' class='border-right border-bottom'>" + getTime(t.LastAccessTime) + "</td>");

                        string filePath = HttpUtility.UrlEncode(parentFolder);
                        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                            filePath = HttpUtility.UrlEncode(parentFolder + "&toolView=true");
                        }

                        if (!_ss.LockFileManager) {
                            string editBtn = "<a href='FileManager.aspx?edit=true&file=" + filePath + "' class='td-edit-btn margin-left-sml margin-right-sml RandomActionBtns' title='Edit'></a>";
                            string downloadBtn = "<a href='#' class='img-download pad-all-sml' title='Download' onclick=\"DownloadFile('" + HttpUtility.UrlEncode(parentFolder) + "');return false;\"></a>";
                            string previewBtn = "<a href='FileManager.aspx?edit=false&file=" + filePath + "' class='img-view pad-all-sml margin-left-sml RandomActionBtns' title='View'></a>";
                            str.Append("<td align='center' class='border-right border-bottom'>" + previewBtn + editBtn + downloadBtn + "</td>");
                        }

                        str.Append("</tr>");
                        count++;
                    }
                }
            }
        }

        str.Append("</tbody></table></div>");
        #endregion

        pnl_filelist.Controls.Clear();
        if (count > 1) {
            pnl_filelist.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_filelist.Controls.Add(new LiteralControl("<div class='emptyGridView'>No Files Found</div>"));
        }
    }

    private static bool IsOkFileType(string extension) {
        bool ok = (extension.ToLower() == ".css") || (extension.ToLower() == ".js");
        return ok;
    }

    private string getTime(DateTime postDate) {
        DateTime now = ServerSettings.ServerDateTime;
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

    protected void hf_downloadFile_ValueChanged(object sender, EventArgs e) {
        string fileName = HttpUtility.UrlDecode(hf_downloadFile.Value);
        DownloadFile(fileName);
        LoadScripts();
        hf_downloadFile.Value = string.Empty;
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
            ServerSettings.RefreshPage(Page, string.Empty);
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