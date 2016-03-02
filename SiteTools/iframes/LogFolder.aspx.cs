using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_iframes_LogFolder : System.Web.UI.Page
{
    private ServerSettings _ss = new ServerSettings();

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity _userId = HttpContext.Current.User.Identity;
        if (!_userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck("ASP.sitetools_servermaintenance_networklog_aspx", _userId.Name)) {
                if (!IsPostBack) {
                    LoadFiles();
                    LoadUserBackground();
                    CustomFonts.SetCustomValues(this.Page, new MemberDatabase(_userId.Name));
                    updatepnl_FileContent.Update();
                }
            }
        }
    }

    private void LoadUserBackground() {
        MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
        AcctSettings.LoadUserBackground(this.Page, null, member);
    }

    private void LoadFiles() {
        int fileCount = 0;
        string filePath = ServerSettings.GetServerMapLocation + "Logging";

        StringBuilder str = new StringBuilder();
        str.Append("<ol>");

        if (Directory.Exists(filePath)) {
            var fileList = new DirectoryInfo(filePath).GetFiles()
                        .OrderByDescending(f => f.LastWriteTime)
                        .Select(f => f.Name)
                        .ToList();

            foreach (string file in fileList) {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension.ToLower() == ".log") {
                    str.Append("<li><span onclick=\"OnFileClick('" + HttpUtility.UrlEncode(file) + "');\">" + fi.Name + "</span><a href='#delete' class='margin-left-big' onclick=\"DeleteFile('" + HttpUtility.UrlEncode(file) + "');return false;\">Delete</a></li>");
                    fileCount++;
                }
            }
        }

        str.Append("</ol>");

        if (fileCount == 0) {
            str.Append("<h3>No Log Files Found</h3>");
        }

        pnl_fileSection.Controls.Clear();
        pnl_fileSection.Controls.Add(new LiteralControl(str.ToString()));
        RegisterPostbackScripts.RegisterStartupScript(this, "$('.file-count').html('<b class=\"pad-right\">File Count</b>" + fileCount + "');");
    }

    protected void hf_FileContent_ValueChanged(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\";
        string fileName = HttpUtility.UrlDecode(hf_FileContent.Value.Trim());
        pnl_fileContent.Controls.Clear();
        if (!string.IsNullOrEmpty(fileName) && File.Exists(filePath + fileName)) {
            StringBuilder str = new StringBuilder();
            string[] lines = File.ReadAllLines(filePath + fileName);
            foreach (string line in lines) {
                str.Append(line + "<br />");
            }

            pnl_fileContent.Controls.Add(new LiteralControl(str.ToString()));
        }

        hf_FileContent.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "FinishFileLoad();");
        LoadFiles();
    }

    protected void hf_DeleteFile_ValueChanged(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\" + HttpUtility.UrlDecode(hf_DeleteFile.Value.Trim());
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) {
            FileInfo fi = new FileInfo(filePath);
            if (fi.Extension.ToLower() == ".log") {
                try {
                    File.Delete(filePath);
                }
                catch { }
            }
        }

        LoadFiles();
        hf_DeleteFile.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "RemoveUpdateModal();");
        updatepnl_FileContent.Update();
    }
}