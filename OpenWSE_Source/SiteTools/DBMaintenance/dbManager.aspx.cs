#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

#endregion

public partial class SiteTools_DbManager : Page {
    private readonly AppLog _applog = new AppLog(false);
    private readonly DBViewer _dbviewer = new DBViewer(true);
    private AutoBackupSystem _abs;
    private string _ctrlname;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if ((ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) || (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower())) {
                _abs = new AutoBackupSystem(ServerSettings.GetServerMapLocation);
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null) {
                    string ctlId = sm.AsyncPostBackSourceElementID;
                    _ctrlname = ctlId;
                }

                _username = userId.Name;

                LoadAutoBackupSystem();

                ServerSettings ss = new ServerSettings();
                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    if (!_ctrlname.Contains("cbAutoFixDB")) {
                        btn_checkDatabase.Enabled = !ss.AutoFixDBIssues;
                        btn_checkDatabase.Visible = !ss.AutoFixDBIssues;
                        cbAutoFixDB.Checked = ss.AutoFixDBIssues;
                        cbAutoFixDB.Enabled = true;
                        cbAutoFixDB.Visible = true;
                    }
                }
                else {
                    btn_checkDatabase.Enabled = false;
                    btn_checkDatabase.Visible = false;
                }

                BuildDatabaseChecker();

                if (!IsPostBack) {
                    LoadBackupList(ref GV_BackupList);
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    #region Database backup/restore/download Confirm

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        string passwordUser = ServerSettings.AdminUserName;
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            passwordUser = _username;
        }
        bool isGood = Membership.ValidateUser(passwordUser, tb_passwordConfirm.Text);
        if (isGood) {
            RegisterPostbackScripts.RegisterStartupScript(this, "BeginWork();");
        }
        else {
            tb_passwordConfirm.Text = "";
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
        }
    }
    protected void hf_StartWork_Changed(object sender, EventArgs e) {

        switch (hf_buRestore_type.Value) {
            case "Restore":
                string val = hf_buRestoreCommand_Value.Value;
                if (!string.IsNullOrEmpty(val)) {
                    StartRunningRestore(val);
                }
                break;
            case "Delete":
                try {
                    DeleteRestore(HiddenField1_sitesettings.Value);
                    ServerSettings.PageToolViewRedirect(this.Page, "dbManager.aspx?date=" + DateTime.Now.Ticks);
                }
                catch (Exception ex) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error deleting file. " + ex.Message + "');");
                }
                break;
        }
    }

    #endregion


    #region DB Maintenance

    protected void GV_BackupList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                var gvBackupListLbRestorethis = (LinkButton)e.Row.FindControl("lb_restorethis");
                gvBackupListLbRestorethis.Enabled = false;
                gvBackupListLbRestorethis.Visible = false;
            }
        }
    }
    protected void GV_BackupList_RowCommand(object sender, GridViewCommandEventArgs e) {
        switch (e.CommandName) {
            case "PageNo":
                LoadBackupList(ref GV_BackupList);
                GV_BackupList.PageIndex = Convert.ToInt32(e.CommandArgument.ToString());
                GV_BackupList.DataBind();
                break;
            case "Restore":
                hf_buRestoreCommand_Value.Value = e.CommandArgument.ToString();
                break;
            case "Download":
                string loc = ServerSettings.GetServerMapLocation + "Backups\\" + e.CommandArgument.ToString();
                if (File.Exists(loc))
                    DownloadFile(loc);
                break;
        }
    }
    public void LoadBackupList(ref GridView gv) {
        try {
            gv.DataSource = XmlDataSource_1;
            gv.DataBind();
        }
        catch { }
        BuildDatabaseChecker();
    }
    protected void lbtn_uploaddb_Click(object sender, EventArgs e) {
        if (FileUpload1.HasFile) {
            var info = new FileInfo(FileUpload1.FileName);
            if (info.Extension.ToLower() == ServerSettings.BackupFileExt.ToLower()) {
                string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";

                string f = "DBUpload_" + DateTime.Now.ToFileTime() + ServerSettings.BackupFileExt;
                string loc = ServerSettings.GetServerMapLocation + "Backups\\" + f;
                FileUpload1.SaveAs(loc);
                Thread.Sleep(200);
                string tDesc = tb_upload_desc.Text;
                if ((tDesc == "") || (tDesc.ToLower() == "upload file description"))
                    tDesc = "Uploaded File";
                WriteToXml(backupfile, loc, tDesc);
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "dbManager.aspx?date=" + DateTime.Now.Ticks);
    }
    protected void lbtn_buchat_Click(object sender, EventArgs e) {
        StartRunningBackup();
        ServerSettings.PageToolViewRedirect(this.Page, "dbManager.aspx?date=" + DateTime.Now.Ticks);
    }
    private void StartRunningBackup() {
        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Database_" +
                     DateTime.Now.ToFileTime() + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(_username, loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize(dbviewer.dt);
        Thread.Sleep(200);
        string tDesc = tb_backup_databse.Text;
        if ((tDesc == "") || (tDesc.ToLower() == "backup file description"))
            tDesc = "Non ASP.Net Backup";
        WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);
    }
    private void StartRunningRestore(string filename) {
        cblist_tables.Items.Clear();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\" + filename;
        var sb = new ServerBackup(_username, loc);
        List<string> tables = sb.GetRestoreTableList();
        tables.Sort();
        DatabaseCall dbCall = new DatabaseCall();

        List<string> updatableTables = DefaultDBTables.UpdatableTables(sb.DataColl);

        foreach (string table in tables) {
            if (updatableTables.Contains(table)) {
                ListItem item = new ListItem(table, table);
                item.Selected = true;
                cblist_tables.Items.Add(item);
            }
        }

        hf_filename_tablesrestore.Value = loc;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'tablestorestore-element', 'Table Selection');openWSE.RemoveUpdateModal();");
    }
    private void WriteToXml(string backupfile, string path, string desc) {
        try {
            if (File.Exists(backupfile)) {
                var fi = new FileInfo(path);
                var reader = new XmlTextReader(backupfile);
                var doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();

                XmlElement root = doc.DocumentElement;
                XmlElement newBackup = doc.CreateElement("Backup");
                var mem = new MemberDatabase(_username);
                var str = new StringBuilder();
                str.Append("<Filename>" + fi.Name + "</Filename>");
                str.Append("<Description>" + desc + "</Description>");
                str.Append("<BackupDate>" + fi.CreationTime.ToString(CultureInfo.InvariantCulture) + "</BackupDate>");
                str.Append("<RestoreDate>N/A</RestoreDate>");
                str.Append("<Size>" + HelperMethods.FormatBytes(fi.Length) + "</Size>");
                str.Append("<User>" + HelperMethods.MergeFMLNames(mem) + "</User>");

                newBackup.InnerXml = str.ToString();

                if (root != null) root.PrependChild(newBackup);

                //save the output to a file
                doc.Save(backupfile);
            }
            else {
                var doc = new XmlDocument();
                doc.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><Backups></Backups>");
                var writer = new XmlTextWriter(backupfile, null) { Formatting = Formatting.Indented };
                doc.Save(writer);
                writer.Close();
                WriteToXml(backupfile, path, desc);
            }
        }
        catch {
        }
    }
    private static void DeleteRestore(string filename) {
        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        try {
            if (File.Exists(backupfile)) {
                var reader = new XmlTextReader(backupfile);
                var doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();

                if (doc.DocumentElement != null)
                    foreach (
                        var node in
                            doc.DocumentElement.ChildNodes.Cast<XmlNode>()
                               .Where(
                                   node => node.ChildNodes.Cast<XmlElement>().Any(elm => elm.InnerText == filename))
                        ) {
                        node.RemoveAll();
                        if (node.ParentNode != null) node.ParentNode.RemoveChild(node);
                        if (File.Exists(ServerSettings.GetServerMapLocation + "Backups\\" + filename)) {
                            File.Delete(ServerSettings.GetServerMapLocation + "Backups\\" + filename);
                        }

                        doc.Save(backupfile);
                    }
            }
        }
        catch {
        }
    }
    private static void UpdateRestore(string filename) {
        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        try {
            if (File.Exists(backupfile)) {
                var reader = new XmlTextReader(backupfile);
                var doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                bool correctnode = false;
                if (doc.DocumentElement != null)
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                        foreach (XmlElement elm in node.ChildNodes) {
                            if (elm.InnerText == filename) {
                                correctnode = true;
                            }
                            if ((elm.Name == "RestoreDate") && correctnode) {
                                elm.InnerText = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                                doc.Save(backupfile);
                                correctnode = false;
                                break;
                            }
                        }
                    }
            }
        }
        catch {
        }
    }

    protected void btn_finish_addtables_Click(object sender, EventArgs e) {
        try {
            string loc = hf_filename_tablesrestore.Value;
            if (File.Exists(loc)) {
                var sb = new ServerBackup(_username, loc);

                List<string> tableList = new List<string>();
                foreach (ListItem item in cblist_tables.Items) {
                    if ((item.Selected) && (!tableList.Contains(item.Value))) {
                        tableList.Add(item.Value);
                    }
                }

                if (tableList.Count > 0) {
                    sb.RestoreBackup(tableList);
                    FileInfo fi = new FileInfo(loc);
                    UpdateRestore(fi.Name);
                    ServerSettings.PageToolViewRedirect(this.Page, "dbManager.aspx?date=" + DateTime.Now.Ticks);
                }
                else {
                    cblist_tables.Items.Clear();
                    hf_filename_tablesrestore.Value = string.Empty;
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'tablestorestore-element', '');openWSE.RemoveUpdateModal();");
                }
            }
        }
        catch {
        }
    }
    protected void btn_canceltables_Click(object sender, EventArgs e) {
        cblist_tables.Items.Clear();
        hf_filename_tablesrestore.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'tablestorestore-element', '');openWSE.RemoveUpdateModal();");
    }
    #endregion


    #region Download Current Database

    protected void lbtn_downloaddb_Click(object sender, EventArgs e) {
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Backups\\Temp\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Backups\\Temp\\");

        try {
            foreach (
                string filename in
                    Directory.GetFiles(ServerSettings.GetServerMapLocation + "Backups\\Temp")) {
                if (File.Exists(filename)) {
                    File.Delete(filename);
                }
            }
        }
        catch {
        }
        string f = "DBFull_" + DateTime.Now.ToFileTime();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Temp\\" + f + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(_username, loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize_Current(dbviewer.dt);

        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        Thread.Sleep(200);
        string tDesc = tb_download_backup.Text;
        if (string.IsNullOrEmpty(tDesc)) {
            tDesc = "Full Database Download";
        }
        WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);

        if (File.Exists(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt),
                     ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
        }
        ServerSettings.DeleteBackupTempFolderFiles();
        DownloadFile(loc.Replace("Temp\\", "").Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt));
    }
    protected void DownloadFile(string filename) {
        var info = new FileInfo(filename);
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=" + info.Name);
        Response.Clear();
        Response.WriteFile(filename);
        Response.End();
    }

    #endregion


    #region Auto Backup System

    protected void rb_autoBackuState_on_CheckedChanged(object sender, EventArgs e) {
        ServerSettings.update_AutoBackupSystem(true);
        _abs.StartBackupSystem();
        rb_autoBackuState_on.Checked = true;
        rb_autoBackuState_off.Checked = false;
        pnl_AutoBackup.Enabled = true;
        pnl_AutoBackup.Visible = true;
        LoadAutoBackupDates();
    }
    protected void rb_autoBackuState_off_CheckedChanged(object sender, EventArgs e) {
        ServerSettings.update_AutoBackupSystem(false);
        _abs.StopBackupSystem();
        rb_autoBackuState_on.Checked = false;
        rb_autoBackuState_off.Checked = true;
        pnl_AutoBackup.Enabled = false;
        pnl_AutoBackup.Visible = false;
    }

    protected void hf_EditSlot_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_EditSlot.Value)) {
            hf_dayofweek_edit.Value = "";
            hf_timeofweek_edit.Value = "";
            hf_backuptype_edit.Value = "";
            LoadAutoBackupDatesEdit(hf_EditSlot.Value);
        }
        else {
            LoadAutoBackupDates();
        }

        hf_EditSlot.Value = "";
    }
    protected void hf_DeleteSlot_Changed(object sender, EventArgs e) {
        string id = hf_DeleteSlot.Value;
        if (!string.IsNullOrEmpty(id))
            _abs.deleteItem(id);

        LoadAutoBackupDates();
        hf_DeleteSlot.Value = "";
    }
    protected void hf_CancelSlot_Changed(object sender, EventArgs e) {
        LoadAutoBackupDates();
        hf_CancelSlot.Value = "";
    }
    protected void hf_UpdateSlot_Changed(object sender, EventArgs e) {
        string id = hf_UpdateSlot.Value;
        if (!string.IsNullOrEmpty(id)) {
            if (!string.IsNullOrEmpty(hf_dayofweek_edit.Value))
                _abs.updateBackupDay(id, hf_dayofweek_edit.Value);

            if (!string.IsNullOrEmpty(hf_timeofweek_edit.Value))
                _abs.updateBackupTime(id, hf_timeofweek_edit.Value);

            if (!string.IsNullOrEmpty(hf_backuptype_edit.Value))
                _abs.updateBackupType(id, hf_backuptype_edit.Value);
        }

        LoadAutoBackupDates();
        hf_dayofweek_edit.Value = "";
        hf_timeofweek_edit.Value = "";
        hf_backuptype_edit.Value = "";
        hf_UpdateSlot.Value = "";
    }

    protected void lbtn_addAbs_Clicked(object sender, EventArgs e) {
        string time = ddl_absBackupTimeHour.SelectedValue + ":" + ddl_absBackupTimeMin.SelectedValue + " " + ddl_absBackupTimeAmPm.SelectedValue;
        _abs.addItem(ddl_absDaytoRun.SelectedValue, time, ddl_absBackupType.SelectedValue);
        LoadAutoBackupDates();
    }
    private void LoadAutoBackupDates() {
        _abs.GetEntries();
        pnl_addEntry.Enabled = true;
        pnl_addEntry.Visible = true;
        pnl_Entries.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table class='myHeaderStyle' cellpadding='0' cellspacing='0' style='width: 750px;'>");
        str.Append("<tr><td width='45px'></td><td width='150px'>Day to Run</td>");
        str.Append("<td>Time to Run</td>");
        str.Append("<td width='150px'>Backup Type</td>");
        str.Append("<td width='75px'>Actions</td></tr></table></div>");

        int count = 0;
        foreach (AutoBackupSystem_Coll coll in AutoBackupSystem.AutoBackupColl) {
            str.Append("<table class='myItemStyle GridNormalRow' cellpadding='0' cellspacing='0' style='width: 750px;'>");
            str.Append("<tr><td class='GridViewNumRow' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
            str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupDay + "</h3></td>");
            str.Append("<td align='center' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupTime + "</h3></td>");
            str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupType + "</h3></td>");

            string editButtons = BuildEditButtons(coll.ID);
            str.Append("<td align='center' width='75px' style='border-right: 1px solid #CCC;'>" + editButtons + "</td></tr></table>");
            count++;
        }

        pnl_Entries.Controls.Add(new LiteralControl(str.ToString()));
    }
    private void LoadAutoBackupDatesEdit(string id) {
        _abs.GetEntries();
        pnl_addEntry.Enabled = false;
        pnl_addEntry.Visible = false;
        pnl_Entries.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table class='myHeaderStyle' cellpadding='0' cellspacing='0' style='width: 750px;'>");
        str.Append("<tr><td width='45px'></td><td width='150px'>Day to Run</td>");
        str.Append("<td>Time to Run</td>");
        str.Append("<td width='150px'>Backup Type</td>");
        str.Append("<td width='75px'>Actions</td></tr></table></div>");

        int count = 0;
        foreach (AutoBackupSystem_Coll coll in AutoBackupSystem.AutoBackupColl) {
            if (id == coll.ID) {
                str.Append("<table class='myItemStyle GridNormalRow' cellpadding='0' cellspacing='0' style='width: 750px;'>");
                str.Append("<tr><td class='GridViewNumRow' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
                str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'>" + BuildDayofWeekDD(coll.BackupDay) + "</td>");
                str.Append("<td align='center' style='border-right: 1px solid #CCC;'>" + BuildTimeofWeekDD(coll.BackupTime) + "</td>");
                str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'>" + BuildBackupTypeDD(coll.BackupType) + "</td>");
                str.Append("<td align='center' width='75px' style='border-right: 1px solid #CCC;'>" + BuildUpdateButtons(coll.ID) + "</td></tr></table>");
            }
            else {
                str.Append("<table class='myItemStyle GridNormalRow' cellpadding='0' cellspacing='0' style='width: 750px;'>");
                str.Append("<tr><td class='GridViewNumRow' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
                str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupDay + "</h3></td>");
                str.Append("<td align='center' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupTime + "</h3></td>");
                str.Append("<td align='center' width='150px' style='border-right: 1px solid #CCC;'><h3>" + coll.BackupType + "</h3></td>");
                str.Append("<td align='center' width='75px' style='border-right: 1px solid #CCC;'></td></tr></table>");
            }
            count++;
        }

        pnl_Entries.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string BuildDayofWeekDD(string dayofweek) {
        var str = new StringBuilder();
        str.Append("<select id='ddl_dayofweek_Edit'>");
        str.Append("<option value='Sunday'>Sunday</option>");
        str.Append("<option value='Monday'>Monday</option>");
        str.Append("<option value='Tuesday'>Tuesday</option>");
        str.Append("<option value='Wednesday'>Wednesday</option>");
        str.Append("<option value='Thursday'>Thursday</option>");
        str.Append("<option value='Friday'>Friday</option>");
        str.Append("<option value='Saturday'>Saturday</option>");
        str.Append("</select>");
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddl_dayofweek_Edit').val('" + dayofweek + "');");
        return str.ToString();
    }
    private string BuildTimeofWeekDD(string timeofweek) {
        string[] delims = { ":", " " };
        string[] splitString = timeofweek.Split(delims, StringSplitOptions.RemoveEmptyEntries);

        var str = new StringBuilder();
        str.Append("<select id='ddl_timeofweekHour_Edit'>");
        str.Append("<option value='1'>1</option>");
        str.Append("<option value='2'>2</option>");
        str.Append("<option value='3'>3</option>");
        str.Append("<option value='4'>4</option>");
        str.Append("<option value='5'>5</option>");
        str.Append("<option value='6'>6</option>");
        str.Append("<option value='7'>7</option>");
        str.Append("<option value='8'>8</option>");
        str.Append("<option value='9'>9</option>");
        str.Append("<option value='10'>10</option>");
        str.Append("<option value='11'>11</option>");
        str.Append("<option value='12'>12</option>");
        str.Append("</select>");

        str.Append("<select id='ddl_timeofweekMin_Edit'>");
        str.Append("<option value='00'>00</option>");
        str.Append("<option value='05'>05</option>");
        str.Append("<option value='10'>10</option>");
        str.Append("<option value='15'>15</option>");
        str.Append("<option value='20'>20</option>");
        str.Append("<option value='25'>25</option>");
        str.Append("<option value='30'>30</option>");
        str.Append("<option value='35'>35</option>");
        str.Append("<option value='40'>40</option>");
        str.Append("<option value='45'>45</option>");
        str.Append("<option value='50'>50</option>");
        str.Append("<option value='55'>55</option>");
        str.Append("</select>");

        str.Append("<select id='ddl_timeofweekAmPm_Edit'>");
        str.Append("<option value='am'>AM</option>");
        str.Append("<option value='pm'>PM</option>");
        str.Append("</select>");
        if (splitString.Length == 3)
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddl_timeofweekHour_Edit').val('" + splitString[0] + "');$('#ddl_timeofweekMin_Edit').val('" + splitString[1] + "');$('#ddl_timeofweekAmPm_Edit').val('" + splitString[2] + "');");

        return str.ToString();
    }
    private string BuildBackupTypeDD(string backupType) {
        var str = new StringBuilder();
        str.Append("<select id='ddl_backuptype_Edit'>");
        str.Append("<option value='partial'>Partial</option>");
        str.Append("<option value='full'>Full</option>");
        str.Append("</select>");
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddl_backuptype_Edit').val('" + backupType + "');");
        return str.ToString();
    }
    private string BuildEditButtons(string id) {
        var str = new StringBuilder();
        str.Append("<a href='#edit' class='td-edit-btn RandomActionBtns margin-right' onclick='EditSlot(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a href='#delete' class='td-delete-btn' onclick='DeleteSlot(\"" + id + "\");return false;' title='Delete'></a>");

        return str.ToString();
    }
    private string BuildUpdateButtons(string id) {
        var str = new StringBuilder();
        str.Append("<a href='#update' class='td-update-btn RandomActionBtns margin-right' onclick='UpdateSlot(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a href='#cancel' class='td-cancel-btn RandomActionBtns' onclick='CancelSlot();return false;' title='Cancel'></a>");

        return str.ToString();
    }
    private void LoadAutoBackupSystem() {
        if (AutoBackupSystem.AutoBackupState) {
            rb_autoBackuState_on.Checked = true;
            rb_autoBackuState_off.Checked = false;
            pnl_AutoBackup.Enabled = true;
            pnl_AutoBackup.Visible = true;
        }
        else {
            rb_autoBackuState_on.Checked = false;
            rb_autoBackuState_off.Checked = true;
            pnl_AutoBackup.Enabled = false;
            pnl_AutoBackup.Visible = false;
        }
        LoadAutoBackupDates();
    }

    #endregion


    #region Database Checker

    private void BuildDatabaseChecker() {
        if (!_ctrlname.Contains("cbAutoFixDB")) {
            pnl_databaseChecker.Controls.Clear();

            DefaultDBTables.CheckIfDatabaseUpToDate();
            DatabaseCall dbCall = new DatabaseCall();

            btn_UpdateDatabase.Enabled = false;
            btn_UpdateDatabase.Visible = false;
            lbl_updatedbHint.Enabled = false;
            lbl_updatedbHint.Visible = false;

            string alignText = "left";
            string verticalAlign = "top";

            StringBuilder str = new StringBuilder();
            str.Append("<table cellpadding='5' cellspacing='5'><tbody>");

            #region Database Provider

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Provider</td>");
            str.Append("<td valign='" + verticalAlign + "'>" + dbCall.DataProvider + "</td>");
            str.Append("</tr>");

            #endregion

            #region Up-To-Date

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Up-To-Date</td>");
            string uptoDate = "No";
            if (DefaultDBTables.DatabaseUpToDate) {
                uptoDate = "Yes";
            }
            str.Append("<td valign='" + verticalAlign + "'>" + uptoDate + "</td");
            str.Append("</tr>");

            #endregion

            #region Table Count

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Table Count</td>");
            DataTable tables = dbCall.CallGetSchema("Tables");
            string count = "0";
            if (tables != null) {
                count = tables.Rows.Count.ToString();
            }
            else {
                count = "Could not determine number of tables.";
            }
            str.Append("<td valign='" + verticalAlign + "'>" + count + "</td>");
            str.Append("</tr>");

            #endregion

            #region Number of Columns

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Column Count</td>");
            str.Append("<td valign='" + verticalAlign + "'>" + DefaultDBTables.TotalNumberOfColumns.ToString() + "</td>");
            str.Append("</tr>");

            #endregion

            #region Number of Rows

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Row Count</td>");
            str.Append("<td valign='" + verticalAlign + "'>" + DefaultDBTables.TotalNumberOfRows.ToString() + "</td>");
            str.Append("</tr>");

            #endregion

            string dbPath = string.Empty;
            bool dbIsLocal = DefaultDBTables.CheckIfDatabaseIsLocal(dbCall, out dbPath);

            #region Database is Local

            str.Append("<tr>");
            str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Is Local</td>");
            string isLocal = "No";


            if (dbIsLocal) {
                isLocal = "Yes";
            }
            str.Append("<td valign='" + verticalAlign + "'>" + isLocal + "</td>");
            str.Append("</tr>");

            #endregion

            #region Database File Size

            if (dbIsLocal) {
                str.Append("<tr>");
                str.Append("<td class='settings-name-column' style='padding-top: 0px!important;'>Database Size</td>");
                str.Append("<td valign='" + verticalAlign + "'>" + DatabaseSize(dbPath) + "</td>");
                str.Append("</tr>");
            }

            #endregion

            str.Append("</tbody></table>");
            ServerSettings ss = new ServerSettings();

            if (DefaultDBTables.DefaultTableXmlMissing) {
                str.Append("<div class='pad-left'><span style='color: red;'>The DatabaseDefaults.xml seems to be missing.<br />In order to properly scan the database, this file must be in the App_Data folder.</span><div class='clear-space-five'></div></div>");
            }
            if (!DefaultDBTables.DatabaseUpToDate) {
                if (!ss.AutoFixDBIssues) {
                    str.Append("<div class='pad-left'><span style='color: red;'>Your Database seems to be out of date.<br />Press the 'Fix' button to update the database.</span><div class='clear-space-five'></div></div>");
                }

                if (dbCall.DataProvider != "System.Data.SqlClient" && dbCall.DataProvider != "System.Data.SqlServerCe.4.0") {
                    btn_checkDatabase.Enabled = false;
                    btn_checkDatabase.Visible = false;
                }
                else {
                    if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        btn_UpdateDatabase.Enabled = !ss.AutoFixDBIssues;
                        btn_UpdateDatabase.Visible = !ss.AutoFixDBIssues;
                        lbl_updatedbHint.Enabled = !ss.AutoFixDBIssues;
                        lbl_updatedbHint.Visible = !ss.AutoFixDBIssues;
                        btn_checkDatabase.Enabled = !ss.AutoFixDBIssues;
                        btn_checkDatabase.Visible = !ss.AutoFixDBIssues;
                        cbAutoFixDB.Checked = ss.AutoFixDBIssues;
                        cbAutoFixDB.Enabled = true;
                        cbAutoFixDB.Visible = true;
                    }
                }
            }

            pnl_databaseChecker.Controls.Add(new LiteralControl(str.ToString()));
        }
    }

    protected void cbAutoFixDB_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            ServerSettings.update_AutoFixDBIssues(cbAutoFixDB.Checked);

            ServerSettings ss = new ServerSettings();
            btn_checkDatabase.Enabled = !ss.AutoFixDBIssues;
            btn_checkDatabase.Visible = !ss.AutoFixDBIssues;

            _ctrlname = string.Empty;
            BuildDatabaseChecker();
        }
    }
    protected void btn_checkDatabase_Click(object sender, EventArgs e) {
        BuildDatabaseChecker();
    }
    protected void btn_UpdateDatabase_Click(object sender, EventArgs e) {
        // Updates each table in the database with the current DatabaseDefaults.xml
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            DefaultDBTables.UpdateDefaults();
            BuildDatabaseChecker();
            if (!DefaultDBTables.DatabaseUpToDate) {
                DefaultDBTables.UpdateDefaults();
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You are not authorized to perform this action!');");
        }

        BuildDatabaseChecker();
    }
    private string DatabaseSize(string dbPath) {
        try {
            DatabaseCall dbCall = new DatabaseCall();
            FileInfo fi = new FileInfo(dbPath);
            return HelperMethods.FormatBytes(fi.Length);
        }
        catch { }

        return "N/A";
    }

    #endregion

}