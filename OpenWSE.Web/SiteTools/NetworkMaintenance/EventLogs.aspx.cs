using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.AutoUpdates;
using System.Collections.Generic;
using System.IO;
using OpenWSE_Tools.Notifications;

public partial class SiteTools_EventLogs : BasePage {

    protected AppLog MainAppLog = new AppLog(true);
    private IPWatch _ipwatch = new IPWatch(true);
    private LoginActivity _la = new LoginActivity();

    protected void Page_Load(object sender, EventArgs e) {
        LoadEvents();
        LoadIgnoredEvents();

        if (!IsPostBack) {
            BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);
        }

        if (!IsUserInAdminRole()) {
            pnl_rtk_E.Enabled = false;
            pnl_rtk_E.Visible = false;
            LinkButton1.Enabled = false;
            LinkButton1.Visible = false;
            lbtnClearAllIgnored.Enabled = false;
            lbtnClearAllIgnored.Visible = false;
            networkSettings.Visible = false;
        }

        if (!IsPostBack) {
            string ertk = MainServerSettings.WebEventsToKeep.ToString(CultureInfo.InvariantCulture);
            if (ertk == "0")
                ertk = "All";

            tb_Records_to_keep_E.Text = ertk;
        }

        LoadSettings();
        GetCallBack();
    }

    private void GetCallBack() {
        switch (AsyncPostBackSourceElementID) {
            case "MainContent_LinkButton1":
                MainAppLog.deleteLog();
                MainAppLog = new AppLog(true);
                LoadEvents();
                LoadIgnoredEvents();
                break;
            case "MainContent_LinkButton2":
                DeleteLogFiles();
                MainAppLog = new AppLog(true);
                LoadEvents();
                LoadIgnoredEvents();
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'logFolder-element', '');");
                break;
            case "MainContent_lbtnClearAllIgnored":
                AppLogIgnore ignore = new AppLogIgnore(false);
                ignore.deleteRecords();
                LoadEvents();
                LoadIgnoredEvents();
                break;
        }
    }

    private void DeleteLogFiles() {
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\" + ServerSettings.ApplicationID;
        if (Directory.Exists(filePath)) {
            string[] fileList = Directory.GetFiles(filePath);
            foreach (string file in fileList) {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension.ToLower() == ".log") {
                    try {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }
    }

    private void LoadSettings() {
        if (MainServerSettings.RecordActivity) {
            cb_netactOn.Checked = true;
            cb_netactOff.Checked = false;
            pnl_RecordLogFile.Enabled = true;
            pnl_RecordLogFile.Visible = true;
            tableEmailAct.Style["display"] = "block";
            if (MainServerSettings.EmailActivity) {
                cb_emailon.Checked = true;
                cb_emailoff.Checked = false;
            }
            else {
                cb_emailon.Checked = false;
                cb_emailoff.Checked = true;
            }
        }
        else {
            cb_netactOn.Checked = false;
            cb_netactOff.Checked = true;
            pnl_RecordLogFile.Enabled = false;
            pnl_RecordLogFile.Visible = false;
            tableEmailAct.Style["display"] = "none";
        }

        if (MainServerSettings.DisableJavascriptErrorAlerts) {
            rb_DisableJavascriptErrorAlerts_on.Checked = true;
            rb_DisableJavascriptErrorAlerts_off.Checked = false;
        }
        else {
            rb_DisableJavascriptErrorAlerts_on.Checked = false;
            rb_DisableJavascriptErrorAlerts_off.Checked = true;
        }

        if (MainServerSettings.RecordActivityToLogFile) {
            rb_recordLogFile_on.Checked = true;
            rb_recordLogFile_off.Checked = false;
        }
        else {
            rb_recordLogFile_on.Checked = false;
            rb_recordLogFile_off.Checked = true;
        }

        if (MainServerSettings.RecordErrorsOnly) {
            rb_RecordErrorsOnly_on.Checked = true;
            rb_RecordErrorsOnly_off.Checked = false;
        }
        else {
            rb_RecordErrorsOnly_on.Checked = false;
            rb_RecordErrorsOnly_off.Checked = true;
        }
    }

    protected void hf_searchreset_Changed(object sender, EventArgs e) {
        MainAppLog = new AppLog(true);
        LoadEvents();
        LoadIgnoredEvents();
    }

    protected void lbtn_refreshignore_Click(object sender, EventArgs e) {
        LoadIgnoredEvents();
    }

    protected void lbtn_refresherrors_Click(object sender, EventArgs e) {
        MainAppLog = new AppLog(true);
        LoadEvents();
    }


    #region Log Folder Events

    private void LoadLogFiles() {
        int fileCount = 0;
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\" + ServerSettings.ApplicationID;

        StringBuilder str = new StringBuilder();
        str.Append("<ul>");

        if (Directory.Exists(filePath)) {
            var fileList = new DirectoryInfo(filePath).GetFiles()
                        .OrderByDescending(f => f.LastWriteTime)
                        .Select(f => f.Name)
                        .ToList();

            foreach (string file in fileList) {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension.ToLower() == ".log") {
                    str.Append("<li><div class='float-left font-bold pad-right-sml margin-right-sml'>" + (fileCount + 1).ToString() + ".</div><span class='float-left' onclick=\"OnFileClick('" + HttpUtility.UrlEncode(file) + "');\">" + fi.Name + "</span><a class='td-delete-btn' onclick=\"DeleteFile('" + HttpUtility.UrlEncode(file) + "');return false;\" style='width: 12px; height: 12px;'></a><div class='clear'></div></li>");
                    fileCount++;
                }
            }
        }

        str.Append("</ul>");

        if (fileCount == 0) {
            str.Append("<h3>No Log Files Found</h3>");
        }

        pnl_logfolderHolder.Controls.Clear();
        pnl_logfolderHolder.Controls.Add(new LiteralControl(str.ToString()));

        updatepnl_logfolderHolder.Update();
    }
    protected void hf_logfolder_ValueChanged(object sender, EventArgs e) {
        LoadLogFiles();

        hf_logfolder.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'logFolder-element', 'Error Folder');");
    }
    protected void hf_FileContent_ValueChanged(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\" + ServerSettings.ApplicationID + "\\";
        string fileName = HttpUtility.UrlDecode(hf_FileContent.Value.Trim());
        pnl_fileContent.Controls.Clear();
        if (!string.IsNullOrEmpty(fileName) && File.Exists(filePath + fileName)) {
            StringBuilder str = new StringBuilder();
            string[] lines = File.ReadAllLines(filePath + fileName);
            foreach (string line in lines) {
                if (line.Contains("- START -") || line.Contains("- END -")) {
                    continue;
                }
                else {
                    int firstColonIndex = line.IndexOf(':');
                    if (firstColonIndex != -1 && firstColonIndex < 20) {
                        string tempLine = line;
                        string startStr = tempLine.Substring(0, firstColonIndex + 1);
                        tempLine = tempLine.Replace(startStr, "<h3 class='font-bold'>" + startStr + "</h3><div class='clear-space-two'></div>");
                        str.Append("<div class='clear-space'></div>" + tempLine);
                    }
                    else {
                        str.Append(line);
                    }
                }
            }

            str.Append("<div class='clear-space'></div>");
            pnl_fileContent.Controls.Add(new LiteralControl(str.ToString()));
        }

        LoadLogFiles();

        hf_FileContent.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "FinishFileLoad('" + fileName + "');");
    }
    protected void hf_DeleteFile_ValueChanged(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "Logging\\" + ServerSettings.ApplicationID + "\\" + HttpUtility.UrlDecode(hf_DeleteFile.Value.Trim());
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) {
            FileInfo fi = new FileInfo(filePath);
            if (fi.Extension.ToLower() == ".log") {
                try {
                    File.Delete(filePath);
                }
                catch { }
            }
        }

        LoadLogFiles();

        hf_DeleteFile.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'logFolder-element', 'Error Folder');");
    }

    #endregion


    #region Event List

    public void LoadEvents() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 3, "EventsTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Event Information", "", false));
        tableBuilder.AddHeaderRow(headerColumns, false);
        #endregion

        #region Build Body
        AppLogIgnore ignores = new AppLogIgnore(true);
        foreach (var al in MainAppLog.app_coll) {
            if (cb_ViewErrorsOnly.Checked && (string.IsNullOrEmpty(al.ExceptionType) || al.ExceptionType.ToLower() == "n/a")) {
                if (!al.EventComment.ToLower().Contains("error")) {
                    continue;
                }
            }

            string entryDiv = "<div class=\"event-entry-holder\"><div class=\"event-entry-title\">{0}</div><div class=\"event-entry-text\">{1}</div></div>";

            string dateEntry = string.Format(entryDiv, "Date", al.DatePosted);
            string eventnameEntry = string.Format(entryDiv, "Event Name", al.EventName);
            string eventmessageEntry = string.Format(entryDiv, "Event Message", al.EventComment);

            string stacktraceEntry = string.Empty;
            string exceptiontypeEntry = string.Empty;
            string machinenameEntry = string.Empty;
            string requesturlEntry = string.Empty;
            string usernameEntry = string.Empty;
            string ipaddressEntry = string.Empty;

            if (cb_ViewMoreDetails.Checked) {
                stacktraceEntry = string.Format(entryDiv, "Stack Trace", al.StackTrace);
                if (al.StackTrace == "N/A") {
                    stacktraceEntry = string.Empty;
                }
                exceptiontypeEntry = string.Format(entryDiv, "Exception Type", al.ExceptionType);
                if (al.ExceptionType == "N/A") {
                    exceptiontypeEntry = string.Empty;
                }
                machinenameEntry = string.Format(entryDiv, "Machine Name", al.MachineName);

                #region Get Request Url
                string requestUrl = al.RequestUrl;
                try {
                    Uri uri = new Uri(requestUrl);
                    if (!string.IsNullOrEmpty(uri.PathAndQuery)) {
                        requestUrl = ".." + uri.PathAndQuery;
                    }
                }
                catch { }

                if (!string.IsNullOrEmpty(requestUrl)) {
                    requesturlEntry = string.Format(entryDiv, "Request Url", requestUrl);
                }
                #endregion

                #region Get Username
                string currUser = al.UserName;
                if (string.IsNullOrEmpty(currUser)) {
                    currUser = "Not a system user.";
                }

                usernameEntry = string.Format(entryDiv, "Username", currUser);
                #endregion

                if (!string.IsNullOrEmpty(al.IPAddress) && al.IPAddress != "N/A") {
                    ipaddressEntry = string.Format(entryDiv, "IP Address", al.IPAddress);
                }
            }

            string ignoreBtn = string.Empty;
            string refreshBtn = string.Empty;
            string deleteBtn = string.Empty;

            #region Create Buttons
            if (IsUserInAdminRole()) {
                if (al.EventName != "CPU Usage Alert" && al.EventName != "Memory Usage Alert") {
                    ignoreBtn = "<a href='#ignore' class='td-allow-btn' onclick=\"IgnoreError('" + al.EventID + "');return false;\" title='Click to Ignore'></a>";
                    if (ignores.appIgnore_coll.Count > 0) {
                        foreach (AppLogIgnore_Coll aliIgnore in ignores.appIgnore_coll) {
                            if (al.EventComment.ToLower().Contains(aliIgnore.EventComment.ToLower())) {
                                if (aliIgnore.RefreshOnError) {
                                    refreshBtn = "<a href='#NoRefresh' class='td-refresh-btn RandomActionBtns' onclick=\"RefreshPageOnError('" + aliIgnore.EventID + "');return false;\" title='Click to cancel refresh page'></a>";
                                }
                                else {
                                    refreshBtn = "<a href='#Refresh' class='td-norefresh-btn RandomActionBtns' onclick=\"RefreshPageOnError('" + aliIgnore.EventID + "');return false;\" title='Click to allow refresh of page upon this error'></a>";
                                }

                                ignoreBtn = "<a href='#Allow' class='td-ignore-btn' onclick=\"AllowError('" + aliIgnore.EventID + "');return false;\" title='Click to Allow'></a>";
                                break;
                            }
                        }
                    }
                }
                deleteBtn = "<a href='#Delete' class='td-delete-btn' onclick=\"DeleteEvent('" + al.EventID + "');return false;\" title='Delete Event'></a>";
            }
            #endregion

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Event Information", dateEntry + eventnameEntry + eventmessageEntry + stacktraceEntry + exceptiontypeEntry + machinenameEntry + requesturlEntry + usernameEntry + ipaddressEntry, TableBuilderColumnAlignment.Left));
            tableBuilder.AddBodyRow(bodyColumnValues, ignoreBtn + refreshBtn + deleteBtn);
        }
        #endregion

        pnl_gridviewrequests.Controls.Clear();
        pnl_gridviewrequests.Controls.Add(tableBuilder.CompleteTableLiteralControl("No events available"));
    }
    protected void btn_Update_rtk_E_Clicked(object sender, EventArgs e) {
        string number = tb_Records_to_keep_E.Text;
        if (!string.IsNullOrEmpty(number)) {
            if (number.ToLower() == "all") {
                ServerSettings.update_WebEventsToKeep(0);
                tb_Records_to_keep_E.Text = "All";
            }
            else {
                int tempOut;
                if (int.TryParse(number, out tempOut)) {
                    ServerSettings.update_WebEventsToKeep(tempOut);

                    if (tempOut < 0) {
                        tb_Records_to_keep_E.Text = "All";
                    }
                }
            }
        }
    }
    protected void hf_updateAllow_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateAllow.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);
            ignore.deleteRecord(hf_updateAllow.Value);
            hf_updateAllow.Value = string.Empty;
        }

        LoadEvents();
        LoadIgnoredEvents();
    }
    protected void hf_deleteError_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_deleteError.Value)) {
            MainAppLog.deleteRecord(hf_deleteError.Value);
            hf_deleteError.Value = string.Empty;
        }

        MainAppLog = new AppLog(true);
        LoadEvents();
        LoadIgnoredEvents();
    }
    protected void cb_ViewErrorsOnly_CheckedChanged(object sender, EventArgs e) { }
    protected void cb_ViewMoreDetails_CheckedChanged(object sender, EventArgs e) { }

    #endregion


    #region Ignored Errors

    public void LoadIgnoredEvents() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 2, "IgnoreTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Date", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Hits", "70", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Message to Ignore", "", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        AppLogIgnore ignores = new AppLogIgnore(true);
        foreach (var al in ignores.appIgnore_coll) {
            string hitCount = string.Empty;
            string refreshBtn = string.Empty;
            string deleteBtn = string.Empty;

            if (IsUserInAdminRole()) {
                hitCount = "<div class='sort-value-class' data-sortvalue='" + al.TimesHit.ToString() + "'>" + al.TimesHit.ToString() + "<div class='clear-space-two'></div><a href='#' onclick='ResetHitCount(\"" + al.EventID + "\");return false;' title='Reset Hit Count' style='font-size: 11px;'>Reset</a></div>";
                deleteBtn = "<a href='#Delete' class='td-delete-btn' onclick=\"AllowError('" + al.EventID + "');return false;\" title='Delete Ignored Event'></a>";
                if (al.RefreshOnError)
                    refreshBtn = "<a href='#NoRefresh' class='td-refresh-btn RandomActionBtns' onclick=\"RefreshPageOnError('" + al.EventID + "');return false;\" title='Click to cancel refresh page'></a>";
                else
                    refreshBtn = "<a href='#Refresh' class='td-norefresh-btn RandomActionBtns' onclick=\"RefreshPageOnError('" + al.EventID + "');return false;\" title='Click to allow refresh of page upon this error'></a>";
            }
            else {
                hitCount = "<div class='sort-value-class' data-sortvalue='" + al.TimesHit.ToString() + "'>" + al.TimesHit.ToString() + "</div>";
            }

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date", al.DatePosted, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Hits", hitCount, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Message to Ignore", al.EventComment, TableBuilderColumnAlignment.Left));
            tableBuilder.AddBodyRow(bodyColumnValues, refreshBtn + deleteBtn);
        }
        #endregion

        pnl_gridviewignore.Controls.Clear();
        pnl_gridviewignore.Controls.Add(tableBuilder.CompleteTableLiteralControl("No events available"));

    }
    protected void hf_updateIgnore_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateIgnore.Value)) {
            string comment = string.Empty;
            try {
                comment = MainAppLog.GetEventComment(hf_updateIgnore.Value);
            }
            catch { }

            if (comment != "") {
                int urloccurred = comment.IndexOf("URL Occurred");
                if (urloccurred != -1) {
                    string tempComment = comment.Substring(urloccurred);
                    comment = comment.Replace(tempComment, "");
                }

                AppLogIgnore ignore = new AppLogIgnore(false);
                ignore.addItem(comment);
                hf_updateIgnore.Value = string.Empty;
            }

            LoadEvents();
            LoadIgnoredEvents();
        }
    }
    protected void hf_resetHitCount_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_resetHitCount.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);
            ignore.UpdateTimesHit(hf_resetHitCount.Value, 0);
        }

        LoadEvents();
        LoadIgnoredEvents();

        hf_resetHitCount.Value = string.Empty;
    }
    protected void hf_updateRefreshOnError_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateRefreshOnError.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);

            bool temp = ignore.GetRefreshOnError(hf_updateRefreshOnError.Value);
            bool refresh = true;
            if (temp)
                refresh = false;

            ignore.UpdateRefreshOnError(hf_updateRefreshOnError.Value, refresh);
            hf_updateRefreshOnError.Value = string.Empty;
        }

        LoadEvents();
        LoadIgnoredEvents();
    }

    #endregion


    #region Settings

    protected void cb_netactOn_CheckedChanged(object sender, EventArgs e) {
        cb_netactOff.Checked = false;
        cb_netactOn.Checked = true;
        ServerSettings.update_RecordActivity(true);

        pnl_RecordLogFile.Enabled = true;
        pnl_RecordLogFile.Visible = true;

        if (MainServerSettings.EmailActivity) {
            cb_emailon.Checked = true;
            cb_emailoff.Checked = false;
        }
        else {
            cb_emailon.Checked = false;
            cb_emailoff.Checked = true;
        }

        tableEmailAct.Style["display"] = "block";
    }
    protected void cb_netactOff_CheckedChanged(object sender, EventArgs e) {
        cb_netactOff.Checked = true;
        cb_netactOn.Checked = false;

        pnl_RecordLogFile.Enabled = false;
        pnl_RecordLogFile.Visible = false;

        tableEmailAct.Style["display"] = "none";
        ServerSettings.update_RecordActivity(false);
    }

    protected void rb_DisableJavascriptErrorAlerts_on_CheckedChanged(object sender, EventArgs e) {
        rb_DisableJavascriptErrorAlerts_off.Checked = false;
        rb_DisableJavascriptErrorAlerts_on.Checked = true;
        ServerSettings.update_DisableJavascriptErrorAlerts(true);

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.reportOnError=false;");
    }
    protected void rb_DisableJavascriptErrorAlerts_off_CheckedChanged(object sender, EventArgs e) {
        rb_DisableJavascriptErrorAlerts_off.Checked = true;
        rb_DisableJavascriptErrorAlerts_on.Checked = false;
        ServerSettings.update_DisableJavascriptErrorAlerts(false);

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.reportOnError=true;");
    }

    protected void rb_RecordErrorsOnly_on_CheckedChanged(object sender, EventArgs e) {
        rb_RecordErrorsOnly_off.Checked = false;
        rb_RecordErrorsOnly_on.Checked = true;
        ServerSettings.update_RecordErrorsOnly(true);
    }
    protected void rb_RecordErrorsOnly_off_CheckedChanged(object sender, EventArgs e) {
        rb_RecordErrorsOnly_off.Checked = true;
        rb_RecordErrorsOnly_on.Checked = false;
        ServerSettings.update_RecordErrorsOnly(false);
    }

    protected void rb_recordLogFile_on_CheckedChanged(object sender, EventArgs e) {
        rb_recordLogFile_off.Checked = false;
        rb_recordLogFile_on.Checked = true;
        ServerSettings.update_RecordActivityToLogFile(true);
    }
    protected void rb_recordLogFile_off_CheckedChanged(object sender, EventArgs e) {
        rb_recordLogFile_off.Checked = true;
        rb_recordLogFile_on.Checked = false;
        ServerSettings.update_RecordActivityToLogFile(false);
    }

    protected void cb_emailon_CheckedChanged(object sender, EventArgs e) {
        cb_emailoff.Checked = false;
        cb_emailon.Checked = true;
        ServerSettings.update_EmailActivity(true);
    }
    protected void cb_emailoff_CheckedChanged(object sender, EventArgs e) {
        cb_emailoff.Checked = true;
        cb_emailon.Checked = false;
        ServerSettings.update_EmailActivity(false);
    }

    #endregion

}