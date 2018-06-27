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
using OpenWSE_Tools.AppServices;

#endregion

public partial class SiteTools_DbManager : BasePage {

    private readonly DBViewer _dbviewer = new DBViewer(true);
    private AutoBackupSystem _abs;

    protected void Page_Load(object sender, EventArgs e) {
        _abs = new AutoBackupSystem(ServerSettings.GetServerMapLocation);
        HelperMethods.SetIsSocialUserForDeleteItems(Page, CurrentUsername);
        LoadAutoBackupSystem();

        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);

        ServerSettings ss = new ServerSettings();
        if (IsUserNameEqualToAdmin()) {
            if (!PostbackControlContainsString("cbAutoFixDB")) {
                btn_checkDatabase.Enabled = !ss.AutoFixDBIssues;
                btn_checkDatabase.Visible = !ss.AutoFixDBIssues;
                cbAutoFixDB.Checked = ss.AutoFixDBIssues;
                cbAutoFixDB.Enabled = true;
                cbAutoFixDB.Visible = true;
            }

            pnl_adminOnly_RestoreDefaults.Enabled = true;
            pnl_adminOnly_RestoreDefaults.Visible = true;
            if (dd_defaultTableList.Items.Count == 0) {
                BuildDefaultTableValueDropDown();
            }
            else {
                UpdateDefaultTableViewer();
            }
        }
        else {
            btn_checkDatabase.Enabled = false;
            btn_checkDatabase.Visible = false;
        }

        LoadBackupList();
    }

    #region Database backup/restore/download Confirm

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        string passwordUser = ServerSettings.AdminUserName;
        if (IsUserInAdminRole() && !CurrentUserMemberDatabase.IsSocialAccount) {
            passwordUser = CurrentUsername;
        }

        bool isGood = false;

        if (CurrentUserMemberDatabase.IsSocialAccount && passwordUser.ToLower() == CurrentUsername.ToLower()) {
            isGood = true;
        }
        else {
            bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(passwordUser);
            isGood = Membership.ValidateUser(passwordUser, tb_passwordConfirm.Text);
            MemberDatabase.UnlockUserIfNeeded(userLockedOut, passwordUser);
        }

        if (isGood) {
            RegisterPostbackScripts.RegisterStartupScript(this, "BeginWork();");
        }
        else {
            if (CurrentUserMemberDatabase.IsSocialAccount) {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You are not authorized to modify/delete this database file.');");
            }
            else {
                tb_passwordConfirm.Text = "";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
            }
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
                    ServerSettings.PageIFrameRedirect(this.Page, "dbManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
                }
                catch (Exception ex) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error deleting file. " + ex.Message + "');");
                }
                break;
        }
    }

    #endregion


    #region DB Maintenance

    protected void lb_restorethis_Changed(object sender, EventArgs e) {
        hf_buRestoreCommand_Value.Value = lb_restorethis.Value;
        lb_restorethis.Value = string.Empty;
    }

    public void LoadBackupList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 3, "backup_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "300", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Backup Date", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Restore Date", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Size", "80", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Backed up by", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        string backupFilePath = string.Format("{0}Backups\\BackupLog.xml", ServerSettings.GetServerMapLocation);
        if (File.Exists(backupFilePath)) {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(backupFilePath);
            XmlNode rootNode = xmlDoc.SelectSingleNode("//Backups");
            if (rootNode != null && rootNode.ChildNodes.Count > 0) {
                foreach (XmlNode childNode in rootNode.ChildNodes) {
                    try {
                        string fileName = childNode.ChildNodes[0].InnerXml.Trim();
                        string description = childNode.ChildNodes[1].InnerXml.Trim();
                        string backupDate = childNode.ChildNodes[2].InnerXml.Trim();
                        string restoreDate = childNode.ChildNodes[3].InnerXml.Trim();
                        string size = childNode.ChildNodes[4].InnerXml.Trim();
                        string user = childNode.ChildNodes[5].InnerXml.Trim();
                        if (childNode.ChildNodes.Count == 7) {
                            string appId = childNode.ChildNodes[6].InnerXml.Trim();
                            if (appId != ServerSettings.ApplicationID) {
                                continue;
                            }
                        }

                        List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Name", fileName.Replace(ServerSettings.BackupFileExt, string.Empty), TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Backup Date", backupDate, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Restore Date", restoreDate, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Size", size, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Backed up by", user, TableBuilderColumnAlignment.Left));

                        string filePath = ResolveUrl("~/Backups/" + fileName);

                        string restoreBtn = "<a class=\"td-restore-btn cursor-pointer\" onclick=\"OnRestore('" + fileName + "');\" title=\"Restore\"></a>";
                        string downloadBtn = "<a class=\"td-download-btn cursor-pointer\" onclick=\"OnDownload('" + HttpUtility.UrlEncode(filePath) + "');\" title=\"Download\"></a>";
                        string deleteBtn = "<a class=\"td-delete-btn cursor-pointer\" onclick=\"OnDelete('" + fileName + "');\" title=\"Delete\"></a>";

                        string editButtons = restoreBtn + downloadBtn + deleteBtn;

                        tableBuilder.AddBodyRow(bodyColumnValues, editButtons);
                    }
                    catch (Exception e) {
                        AppLog.AddError(e);
                    }
                }
            }
        }
        #endregion

        pnl_backuplist.Controls.Clear();
        pnl_backuplist.Controls.Add(tableBuilder.CompleteTableLiteralControl("No backups found"));

        BuildDatabaseChecker("cbAutoFixDB");
    }
    protected void lbtn_uploaddb_Click(object sender, EventArgs e) {
        if (FileUpload1.HasFile) {
            var info = new FileInfo(FileUpload1.FileName);
            if (info.Extension.ToLower() == ServerSettings.BackupFileExt.ToLower()) {
                string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";

                string f = "DBUpload_" + ServerSettings.ServerDateTime.ToFileTime() + ServerSettings.BackupFileExt;
                string loc = ServerSettings.GetServerMapLocation + "Backups\\" + f;
                FileUpload1.SaveAs(loc);
                Thread.Sleep(200);
                string tDesc = tb_upload_desc.Text;
                if ((tDesc == "") || (tDesc.ToLower() == "upload file description"))
                    tDesc = "Uploaded File";
                AutoBackupSystem.WriteToXml(backupfile, loc, tDesc);
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "dbManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
    }
    protected void lbtn_buchat_Click(object sender, EventArgs e) {
        StartRunningBackup();
        ServerSettings.PageIFrameRedirect(this.Page, "dbManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
    }
    private void StartRunningBackup() {
        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Database_" +
                     ServerSettings.ServerDateTime.ToFileTime() + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(CurrentUsername, loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize(dbviewer.dt);
        Thread.Sleep(200);
        string tDesc = tb_backup_databse.Text;
        if ((tDesc == "") || (tDesc.ToLower() == "backup file description"))
            tDesc = "Non ASP.Net Backup";
        AutoBackupSystem.WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);
    }
    private void StartRunningRestore(string filename) {
        cblist_tables.Items.Clear();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\" + filename;
        var sb = new ServerBackup(CurrentUsername, loc);
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
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'tablestorestore-element', 'Table Selection');loadingPopup.RemoveMessage();");
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
                                elm.InnerText = ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture);
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
                var sb = new ServerBackup(CurrentUsername, loc);

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
                    ServerSettings.PageIFrameRedirect(this.Page, "dbManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
                }
                else {
                    cblist_tables.Items.Clear();
                    hf_filename_tablesrestore.Value = string.Empty;
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'tablestorestore-element', '');loadingPopup.RemoveMessage();");
                }
            }
        }
        catch {
        }
    }
    protected void btn_canceltables_Click(object sender, EventArgs e) {
        cblist_tables.Items.Clear();
        hf_filename_tablesrestore.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'tablestorestore-element', '');loadingPopup.RemoveMessage();");
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
        string f = "DBFull_" + ServerSettings.ServerDateTime.ToFileTime();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Temp\\" + f + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(CurrentUsername, loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize_Current(dbviewer.dt);

        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        Thread.Sleep(200);
        string tDesc = tb_download_backup.Text;
        if (string.IsNullOrEmpty(tDesc)) {
            tDesc = "Full Database Download";
        }
        AutoBackupSystem.WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);

        if (File.Exists(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt),
                     ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
        }
        ServerSettings.DeleteBackupTempFolderFiles();
        ServerSettings.PageIFrameRedirect(this.Page, "dbManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
    }

    #endregion


    #region Auto Backup System

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
        _abs.addItem(hf_autoBackup_day.Value, hf_autoBackup_time.Value, hf_autoBackup_type.Value);
        LoadAutoBackupDates();

        hf_autoBackup_day.Value = string.Empty;
        hf_autoBackup_time.Value = string.Empty;
        hf_autoBackup_type.Value = string.Empty;
        lbtn_addAbs.Value = string.Empty;
    }
    private void LoadAutoBackupDates() {
        _abs.GetEntries();

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 2, "autobackup_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Day to Run", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Time to Run", "300", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Backup Type", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (AutoBackupSystem_Coll coll in AutoBackupSystem.AutoBackupColl) {
            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Day to Run", coll.BackupDay, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Time to Run", coll.BackupTime, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Backup Type", coll.BackupType, TableBuilderColumnAlignment.Left));

            string editButtons = BuildEditButtons(coll.ID);
            tableBuilder.AddBodyRow(bodyColumnValues, editButtons);
        }
        #endregion

        #region Build Insert
        List<TableBuilderInsertColumnValues> insertColumnValues = new List<TableBuilderInsertColumnValues>();

        #region Build Day to Run
        string dayToRunHtml = "<select id=\"ddl_absDaytoRun\">";
        dayToRunHtml += "<option value=\"Sunday\">Sunday</option>";
        dayToRunHtml += "<option value=\"Monday\">Monday</option>";
        dayToRunHtml += "<option value=\"Tuesday\">Tuesday</option>";
        dayToRunHtml += "<option value=\"Wednesday\">Wednesday</option>";
        dayToRunHtml += "<option value=\"Thursday\">Thursday</option>";
        dayToRunHtml += "<option value=\"Friday\">Friday</option>";
        dayToRunHtml += "<option value=\"Saturday\">Saturday</option>";
        dayToRunHtml += "</select>";
        #endregion

        #region Build Time to Run
        string timeToRunHtml = "<select id=\"ddl_absBackupTimeHour\">";
        timeToRunHtml += "<option value=\"1\">1</option>";
        timeToRunHtml += "<option value=\"2\">2</option>";
        timeToRunHtml += "<option value=\"3\">3</option>";
        timeToRunHtml += "<option value=\"4\">4</option>";
        timeToRunHtml += "<option value=\"5\">5</option>";
        timeToRunHtml += "<option value=\"6\">6</option>";
        timeToRunHtml += "<option value=\"7\">7</option>";
        timeToRunHtml += "<option value=\"8\">8</option>";
        timeToRunHtml += "<option value=\"9\">9</option>";
        timeToRunHtml += "<option value=\"10\">10</option>";
        timeToRunHtml += "<option value=\"11\">11</option>";
        timeToRunHtml += "<option value=\"12\">12</option>";
        timeToRunHtml += "</select>";

        timeToRunHtml += "<span class=\"font-bold\">&nbsp;:&nbsp;</span>";

        timeToRunHtml += "<select id=\"ddl_absBackupTimeMin\" class=\"margin-right\">";
        timeToRunHtml += "<option value=\"00\">00</option>";
        timeToRunHtml += "<option value=\"05\">05</option>";
        timeToRunHtml += "<option value=\"10\">10</option>";
        timeToRunHtml += "<option value=\"15\">15</option>";
        timeToRunHtml += "<option value=\"20\">20</option>";
        timeToRunHtml += "<option value=\"25\">25</option>";
        timeToRunHtml += "<option value=\"30\">30</option>";
        timeToRunHtml += "<option value=\"35\">35</option>";
        timeToRunHtml += "<option value=\"40\">40</option>";
        timeToRunHtml += "<option value=\"45\">45</option>";
        timeToRunHtml += "<option value=\"50\">50</option>";
        timeToRunHtml += "<option value=\"55\">55</option>";
        timeToRunHtml += "</select>";

        timeToRunHtml += "<select id=\"ddl_absBackupTimeAmPm\">";
        timeToRunHtml += "<option value=\"am\">am</option>";
        timeToRunHtml += "<option value=\"pm\">pm</option>";
        timeToRunHtml += "</select>";
        #endregion

        #region Build Backup Type
        string backupTypeHtml = "<select id=\"ddl_absBackupType\">";
        backupTypeHtml += "<option value=\"partial\">partial</option>";
        backupTypeHtml += "<option value=\"full\">full</option>";
        backupTypeHtml += "</select>";
        #endregion

        insertColumnValues.Add(new TableBuilderInsertColumnValues("Day to Run", dayToRunHtml, TableBuilderColumnAlignment.Left));
        insertColumnValues.Add(new TableBuilderInsertColumnValues("Time to Run", timeToRunHtml, TableBuilderColumnAlignment.Left));
        insertColumnValues.Add(new TableBuilderInsertColumnValues("Backup Type", backupTypeHtml, TableBuilderColumnAlignment.Left));

        tableBuilder.AddInsertRow(insertColumnValues, "AddNewAutoBackupClick()");
        #endregion

        pnl_Entries.Controls.Clear();
        pnl_Entries.Controls.Add(tableBuilder.CompleteTableLiteralControl("No auto backups found"));
    }
    private void LoadAutoBackupDatesEdit(string id) {
        _abs.GetEntries();

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 2, "autobackup_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Day to Run", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Time to Run", "300", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Backup Type", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (AutoBackupSystem_Coll coll in AutoBackupSystem.AutoBackupColl) {
            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

            string editButtons = string.Empty;
            if (id == coll.ID) {
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Day to Run", BuildDayofWeekDD(coll.BackupDay), TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Time to Run", BuildTimeofWeekDD(coll.BackupTime), TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Backup Type", BuildBackupTypeDD(coll.BackupType), TableBuilderColumnAlignment.Left));
                editButtons = BuildUpdateButtons(coll.ID);
            }
            else {
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Day to Run", coll.BackupDay, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Time to Run", coll.BackupTime, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Backup Type", coll.BackupType, TableBuilderColumnAlignment.Left));
            }

            tableBuilder.AddBodyRow(bodyColumnValues, editButtons);
        }
        #endregion

        pnl_Entries.Controls.Clear();
        pnl_Entries.Controls.Add(tableBuilder.CompleteTableLiteralControl("No auto backups found"));
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

        str.Append("<span class=\"font-bold\">&nbsp;:&nbsp;</span>");

        str.Append("<select id='ddl_timeofweekMin_Edit' class='margin-right'>");
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
        str.Append("<option value='am'>am</option>");
        str.Append("<option value='pm'>pm</option>");
        str.Append("</select>");
        if (splitString.Length == 3)
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddl_timeofweekHour_Edit').val('" + splitString[0] + "');$('#ddl_timeofweekMin_Edit').val('" + splitString[1] + "');$('#ddl_timeofweekAmPm_Edit').val('" + splitString[2] + "');");

        return str.ToString();
    }
    private string BuildBackupTypeDD(string backupType) {
        var str = new StringBuilder();
        str.Append("<select id='ddl_backuptype_Edit'>");
        str.Append("<option value='partial'>partial</option>");
        str.Append("<option value='full'>full</option>");
        str.Append("</select>");
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#ddl_backuptype_Edit').val('" + backupType + "');");
        return str.ToString();
    }
    private string BuildEditButtons(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-edit-btn RandomActionBtns' onclick='EditSlot(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a class='td-delete-btn' onclick='DeleteSlot(\"" + id + "\");return false;' title='Delete'></a>");

        return str.ToString();
    }
    private string BuildUpdateButtons(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-update-btn RandomActionBtns' onclick='UpdateSlot(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a class='td-cancel-btn RandomActionBtns' onclick='CancelSlot();return false;' title='Cancel'></a>");

        return str.ToString();
    }
    private void LoadAutoBackupSystem() {
        if (AutoBackupSystem.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running) {
            lbl_autoBackupsystem_status.Text = OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running.ToString();
            lbl_autoBackupsystem_status.ForeColor = System.Drawing.Color.Green;
            pnl_AutoBackup.Enabled = true;
            pnl_AutoBackup.Visible = true;
        }
        else {
            lbl_autoBackupsystem_status.Text = OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Stopped.ToString();
            lbl_autoBackupsystem_status.ForeColor = System.Drawing.Color.FromArgb(119, 119, 119);
            pnl_AutoBackup.Enabled = false;
            pnl_AutoBackup.Visible = false;
        }
        LoadAutoBackupDates();
    }

    #endregion


    #region Database Checker

    private void BuildDatabaseChecker(string postbackCtrl) {
        if (string.IsNullOrEmpty(postbackCtrl) || !PostbackControlContainsString(postbackCtrl)) {
            pnl_databaseChecker.Controls.Clear();

            DefaultDBTables.CheckIfDatabaseUpToDate();
            DatabaseCall dbCall = new DatabaseCall();

            StringBuilder str = new StringBuilder();

            btn_UpdateDatabase.Enabled = false;
            btn_UpdateDatabase.Visible = false;
            lbl_updatedbHint.Enabled = false;
            lbl_updatedbHint.Visible = false;

            #region Up-To-Date

            if (DefaultDBTables.DatabaseUpToDate) {
                str.Append("<div class='up-to-date-text'>Database is Up-To-Date</div><div class='clear'></div>");
            }
            else {
                str.Append("<div class='out-of-date-text'>Database is Out-Of-Date</div><div class='clear'></div>");
            }

            #endregion

            #region Database Provider

            str.Append("<div class='input-settings-holder'>");
            str.Append("<span class='font-bold'>Provider</span><div class='clear-space-two'></div>");
            str.Append(dbCall.DataProvider);
            str.Append("</div>");

            #endregion

            #region Table Count

            str.Append("<div class='input-settings-holder'>");
            str.Append("<span class='font-bold'>Table Count</span><div class='clear-space-two'></div>");
            DataTable tables = dbCall.CallGetSchema("Tables");
            string count = "0";
            if (tables != null) {
                count = tables.Rows.Count.ToString();
            }
            else {
                count = "Could not determine number of tables.";
            }
            str.Append(count);
            str.Append("</div>");

            #endregion

            #region Number of Columns

            str.Append("<div class='input-settings-holder'>");
            str.Append("<span class='font-bold'>Column Count</span><div class='clear-space-two'></div>");
            str.Append(DefaultDBTables.TotalNumberOfColumns.ToString());
            str.Append("</div>");

            #endregion

            #region Number of Rows

            str.Append("<div class='input-settings-holder'>");
            str.Append("<span class='font-bold'>Row Count</span><div class='clear-space-two'></div>");
            str.Append(DefaultDBTables.TotalNumberOfRows.ToString());
            str.Append("</div>");

            #endregion

            string dbPath = string.Empty;
            bool dbIsLocal = DefaultDBTables.CheckIfDatabaseIsLocal(dbCall, out dbPath);

            #region Database is Local

            str.Append("<div class='input-settings-holder'>");
            str.Append("<span class='font-bold'>Is Local</span><div class='clear-space-two'></div>");
            string isLocal = "No";


            if (dbIsLocal) {
                isLocal = "Yes";
            }
            str.Append(isLocal);
            str.Append("</div>");

            #endregion

            #region Database File Size

            if (dbIsLocal) {
                str.Append("<div class='input-settings-holder'>");
                str.Append("<span class='font-bold'>Database Size</span><div class='clear-space-two'></div>");
                str.Append(DatabaseSize(dbPath));
                str.Append("</div>");
            }

            #endregion

            ServerSettings ss = new ServerSettings();

            if (DefaultDBTables.DefaultTableXmlMissing) {
                str.Append("<div class='pad-left'><span style='color: red;'>The DatabaseDefaults.xml seems to be missing.<br />In order to properly scan the database, this file must be in the App_Data folder.</span><div class='clear-space-five'></div></div>");
            }
            if (!DefaultDBTables.DatabaseUpToDate) {
                if (!ss.AutoFixDBIssues) {
                    str.Append("<div class='pad-left'><small style='color: red;'>Your Database seems to be out of date. Press the 'Fix' button to update the database.</small><div class='clear-space-five'></div></div>");
                }

                if (dbCall.DataProvider != "System.Data.SqlClient" && dbCall.DataProvider != "System.Data.SqlServerCe.4.0") {
                    btn_checkDatabase.Enabled = false;
                    btn_checkDatabase.Visible = false;
                }
                else {
                    if (IsUserNameEqualToAdmin()) {
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

            BuildDatabaseIssueList();
        }
    }

    protected void cbAutoFixDB_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            ServerSettings.update_AutoFixDBIssues(cbAutoFixDB.Checked);

            ServerSettings ss = new ServerSettings();
            btn_checkDatabase.Enabled = !ss.AutoFixDBIssues;
            btn_checkDatabase.Visible = !ss.AutoFixDBIssues;
            BuildDatabaseChecker(string.Empty);
        }
    }
    protected void btn_checkDatabase_Click(object sender, EventArgs e) {
        BuildDatabaseChecker("cbAutoFixDB");
    }
    protected void btn_UpdateDatabase_Click(object sender, EventArgs e) {
        // Updates each table in the database with the current DatabaseDefaults.xml
        if (IsUserNameEqualToAdmin()) {
            DefaultDBTables.UpdateDefaults();
            BuildDatabaseChecker("cbAutoFixDB");
            if (!DefaultDBTables.DatabaseUpToDate) {
                DefaultDBTables.UpdateDefaults();
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You are not authorized to perform this action!');");
        }

        BuildDatabaseChecker("cbAutoFixDB");
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
    private void BuildDatabaseIssueList() {
        pnl_databaseissues.Controls.Clear();

        if (DefaultDBTables.DatabaseScannerIssues.Count > 0) {
            StringBuilder str = new StringBuilder();
            str.Append("<div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<h3 class='font-bold'>Database Issues</h3><div class='clear-space-two'></div><ul style='padding-left: 20px;'>");
            foreach (string issue in DefaultDBTables.DatabaseScannerIssues) {
                str.Append("<li class='margin-top-sml margin-bottom-sml'>" + issue + "</li>");
            }
            str.Append("</ul><div class='clear-space'></div>");
            pnl_databaseissues.Controls.Add(new LiteralControl(str.ToString()));
        }
    }

    #endregion


    #region Database Default Values

    private void BuildDefaultTableValueDropDown() {
        dd_defaultTableList.Items.Clear();

        string defaultsXml = ServerSettings.GetServerMapLocation + "App_Data\\DatabaseDefaultValues.xml";
        if (File.Exists(defaultsXml)) {
            DatabaseCall dbCall = new DatabaseCall();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(defaultsXml);

            if (xmlDoc != null) {
                try {
                    foreach (XmlNode tableNode in xmlDoc.LastChild.ChildNodes) {
                        if (tableNode.Attributes != null && tableNode.Attributes["name"] != null) {
                            if (tableNode.Attributes["for"] != null && tableNode.Attributes["for"].Value != dbCall.DataProvider) {
                                continue;
                            }

                            ListItem item = new ListItem(tableNode.Attributes["name"].Value, tableNode.Attributes["name"].Value);
                            if (!dd_defaultTableList.Items.Contains(item)) {
                                dd_defaultTableList.Items.Add(item);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        UpdateDefaultTableViewer();
    }
    protected void hf_restoreDefaults_ValueChanged(object sender, EventArgs e) {
        if (HttpContext.Current.User.Identity.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            DefaultDBTables.InsertDefaultDataIntoTable(dd_defaultTableList.SelectedValue, true);
        }

        UpdateDefaultTableViewer();
    }
    protected void dd_defaultTableList_SelectedIndexChanged(object sender, EventArgs e) {
        UpdateDefaultTableViewer();
    }
    protected void lbtn_updateDefaultTableList_Click(object sender, EventArgs e) {
        UpdateDefaultTableViewer();
    }
    private void UpdateDefaultTableViewer() {
        pnl_defaultTableHolder.Controls.Clear();
        string tableName = dd_defaultTableList.SelectedValue;

        string defaultsXml = ServerSettings.GetServerMapLocation + "App_Data\\DatabaseDefaultValues.xml";
        if (!File.Exists(defaultsXml)) {
            pnl_defaultTableHolder.Controls.Add(new LiteralControl("<div class='clear-margin'>No Data Available.</div>"));
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(defaultsXml);

        if (xmlDoc == null) {
            pnl_defaultTableHolder.Controls.Add(new LiteralControl("<div class='clear-margin'>No Data Available.</div>"));
            return;
        }

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, false, 0, "dbDefault_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        try {
            foreach (XmlNode tableNode in xmlDoc.LastChild.ChildNodes) {
                if (tableNode.Attributes != null && tableNode.Attributes["name"] != null && tableNode.Attributes["name"].Value == tableName) {
                    foreach (XmlNode rowNode in tableNode.ChildNodes) {
                        for (int i = 0; i < rowNode.Attributes.Count; i++) {
                            if (rowNode.Attributes[i].Name == DatabaseCall.ApplicationIdString || rowNode.Attributes[i].Name == "ApplicationName" || rowNode.Attributes[i].Name == "LoweredApplicationName") {
                                continue;
                            }

                            headerColumns.Add(new TableBuilderHeaderColumns(rowNode.Attributes[i].Name, string.Empty, false));
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        try {
            foreach (XmlNode tableNode in xmlDoc.LastChild.ChildNodes) {
                if (tableNode.Attributes != null && tableNode.Attributes["name"] != null && tableNode.Attributes["name"].Value == tableName) {
                    foreach (XmlNode rowNode in tableNode.ChildNodes) {
                        List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

                        for (int i = 0; i < rowNode.Attributes.Count; i++) {
                            if (rowNode.Attributes[i].Name == DatabaseCall.ApplicationIdString || rowNode.Attributes[i].Name == "ApplicationName" || rowNode.Attributes[i].Name == "LoweredApplicationName") {
                                continue;
                            }

                            string cellValue = rowNode.Attributes[i].Value;
                            bodyColumnValues.Add(new TableBuilderBodyColumnValues(rowNode.Attributes[i].Name, cellValue, TableBuilderColumnAlignment.Left));
                        }

                        tableBuilder.AddBodyRow(bodyColumnValues);
                    }

                    break;
                }
            }
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
        }
        #endregion

        pnl_defaultTableHolder.Controls.Add(new LiteralControl("<div class='clear-space'></div><div class='clear-space'></div>"));
        pnl_defaultTableHolder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No Data Available"));

        updatepnl_tableDefaults.Update();
    }

    #endregion

}