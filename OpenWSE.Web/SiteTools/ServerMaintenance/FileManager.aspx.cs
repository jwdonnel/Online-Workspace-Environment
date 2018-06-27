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

public partial class SiteTools_FileManager : BasePage {

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

    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e) {
        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            lbtn_close.NavigateUrl = "FileManager.aspx?mobileMode=true";
        }

        if (MainServerSettings.LockFileManager) {
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

    private void LoadEditFile() {
        if (!IsPostBack) {
            string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");
            string filename = HttpUtility.UrlDecode(drive + Request.QueryString["file"]);
            if (HelperMethods.ConvertBitToBoolean((Request.QueryString["edit"])) && (!MainServerSettings.LockFileManager)) {
                pnl1.Enabled = false;
                pnl1.Visible = false;
                pnl2.Enabled = true;
                pnl2.Visible = true;
                var sr = new StreamReader(filename);

                var fi = new FileInfo(filename);
                if (IsUserNameEqualToAdmin() || fi.Extension.ToLower() == ".css") {
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
        switch (AsyncPostBackSourceElementID) {
            case "MainContent_lbtn_save":
                string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");
                string path = drive + Request.QueryString["file"].Replace("!", "\\");
                var fi = new FileInfo(path);
                if (!MainServerSettings.LockFileManager) {
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

                        ServerSettings.PageIFrameRedirect(this.Page, "FileManager.aspx");
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

    protected void dd_viewtype_SelectedIndexChanged(object sender, EventArgs e) {
        _viewtype = dd_viewtype.SelectedValue;
        LoadScripts();
    }

    public void LoadScripts() {
        _infolist = new List<FileInfo>();
        RefreshDatabase(ServerSettings.GetServerMapLocation);

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, !MainServerSettings.LockFileManager, 2);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Filename", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Ext", "65", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Size", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Last Accessed", "100", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (FileInfo t in _infolist) {
            if ((_viewtype == t.Extension) || (_viewtype == "all")) {
                if (t.Directory != null) {
                    string parentFolder = t.FullName.Replace(t.Directory.Root.Name, "\\");
                    if (t.Directory.Parent != null && ((t.Directory.Parent.ToString() == "auth") || (t.Directory.Parent.ToString() == "auth"))) {
                        parentFolder = string.Empty;
                    }

                    string fileName = t.FullName.Replace(ServerSettings.GetServerMapLocation, "");

                    List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Filename", fileName, TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Ext", t.Extension, TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Size", HelperMethods.FormatBytes(t.Length), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Accessed", getTime(t.LastAccessTime), TableBuilderColumnAlignment.Left));

                    string editControls = string.Empty;
                    if (!MainServerSettings.LockFileManager) {
                        string filePath = HttpUtility.UrlEncode(parentFolder);
                        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                            filePath = HttpUtility.UrlEncode(parentFolder + "&mobileMode=true");
                        }

                        string editBtn = "<a href='FileManager.aspx?edit=true&file=" + filePath + "' class='td-edit-btn RandomActionBtns' title='Edit'></a>";
                        string previewBtn = "<a href='FileManager.aspx?edit=false&file=" + filePath + "' class='img-view pad-all-sml RandomActionBtns' title='View'></a>";
                        editControls = editBtn + previewBtn;
                    }

                    tableBuilder.AddBodyRow(bodyColumnValues, editControls);
                }
            }
        }
        #endregion

        pnl_filelist.Controls.Clear();
        pnl_filelist.Controls.Add(tableBuilder.CompleteTableLiteralControl("No Files Found"));
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

    protected void DeleteFile(string path) {
        if (!MainServerSettings.LockFileManager) {
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