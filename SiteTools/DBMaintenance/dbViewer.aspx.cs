using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Web.Security;
using OpenWSE_Tools.AppServices;

public partial class SiteTools_dbViewer : System.Web.UI.Page {
    private ServerSettings _ss = new ServerSettings();
    private readonly DBViewer _dbviewer = new DBViewer(true);
    private AutoBackupSystem _abs;
    private string _ctrlname;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        else {
            if ((ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) || (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower())) {
                _abs = new AutoBackupSystem(ServerSettings.GetServerMapLocation);
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null) {
                    string ctlId = sm.AsyncPostBackSourceElementID;
                    _ctrlname = ctlId;
                }

                _username = userId.Name;

                lbl_tablesaved_msg.Enabled = false;
                lbl_tablesaved_msg.Visible = false;

                if (!IsPostBack) {
                    hf_current.Value = string.Empty;
                    BuildDropDown(_dbviewer.dt);
                    SetTableData();
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    #region DB Viewer

    private void BuildDropDown(DataTable dt) {
        App apps = new App(string.Empty);
        dd_table.Items.Clear();
        try {
            var dv = new DataView(dt);
            dv.Sort = string.Format("{0} {1}", dv.Table.Columns[2], "asc");
            dt = dv.ToTable();

            var db = new DBImporter();
            db.GetImportList();
            foreach (var dbcoll in db.DBColl) {
                if (AppExists_Import(dbcoll.TableID)) {
                    if ((_username.ToLower() != dbcoll.ImportedBy.ToLower()) && (apps.GetIsPrivate(dbcoll.TableID))) {
                        continue;
                    }
                }

                if (!dbcoll.TableName.ToLower().Contains("aspnet_") && !dbcoll.TableName.ToLower().Contains("membership")) {
                    var item = new ListItem(dbcoll.TableName + " (Imported)", dbcoll.TableID + "(Imported)");
                    if (!dd_table.Items.Contains(item)) {
                        dd_table.Items.Add(item);
                    }
                }
            }

            foreach (DataRow dr in dt.Rows) {
                if (!dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("membership")) {
                    string tableName = dr["TABLE_NAME"].ToString();

                    bool isCustomTable = false;
                    if (dr["TABLE_NAME"].ToString().StartsWith("CT_")) {
                        CustomTableViewer ctv = new CustomTableViewer(_username);
                        CustomTable_Coll cInfo = ctv.GetTableInfoByTableId(tableName);

                        if (!string.IsNullOrEmpty(cInfo.ID)) {
                            if (AppExists_Custom(cInfo.AppID)) {
                                if ((_username.ToLower() != cInfo.UpdatedBy.ToLower()) && (apps.GetIsPrivate(cInfo.AppID))) {
                                    continue;
                                }
                            }

                            tableName = cInfo.TableName + " (Custom Table)";
                            if (string.IsNullOrEmpty(tableName)) {
                                tableName = dr["TABLE_NAME"].ToString() + " (Custom Table)";
                            }

                            isCustomTable = true;
                        }
                    }

                    bool canAddTable = true;
                    if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                        if (!isCustomTable) {
                            canAddTable = false;
                        }
                    }

                    if (canAddTable) {
                        var item = new ListItem(tableName, dr["TABLE_NAME"].ToString());
                        if (!dd_table.Items.Contains(item)) {
                            dd_table.Items.Add(item);
                        }
                    }
                }
            }
        }
        catch { }
    }

    private bool AppExists_Custom(string appId) {
        if (!string.IsNullOrEmpty(appId)) {
            var fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\" + appId.Replace("app-", "") + ".ascx");
            if (fi.Exists)
                return true;
        }

        return false;
    }

    private bool AppExists_Import(string appId) {
        if (!string.IsNullOrEmpty(appId)) {
            var fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + appId + ".ascx");
            if (fi.Exists)
                return true;
        }

        return false;
    }

    protected void lbtn_refresh_Click(object sender, EventArgs e) {
        SetTableData();
    }
    protected void dd_table_Changed(object sender, EventArgs e) {
        SetTableData();
        hf_current.Value = dd_table.SelectedValue;
        UpdatePanel3.Update();
    }
    protected void imgbtn_clearsearch_Click(object sender, EventArgs e) {
        tb_search.Text = "Search Database Table";
        SetTableData();
        hf_current.Value = dd_table.SelectedValue;
    }
    protected void imgbtn_search_Click(object sender, EventArgs e) {
        SetTableData();
        hf_current.Value = dd_table.SelectedValue;
    }

    private void SetTableData() {
        try {
            bool found = false;
            DBImporter_Coll dbcollTemp = null;
            string tablename = dd_table.SelectedValue;
            if ((!string.IsNullOrEmpty(tablename)) && (!tablename.ToLower().Contains("delete"))) {
                if (tablename.Contains("(Imported)")) {
                    var db = new DBImporter();
                    dbcollTemp = db.GetImportTableByTableId(tablename.Replace("(Imported)", "").Trim());
                    if (!string.IsNullOrEmpty(dbcollTemp.ID)) {
                        found = true;
                    }
                }

                if ((found) && (dbcollTemp != null)) {
                    _dbviewer.GetImportedTableData(dbcollTemp);
                    if (hf_current.Value != dd_table.SelectedValue) {
                        if (dbcollTemp.SelectCommand.ToLower().Contains("order by")) {
                            try {
                                int index = dbcollTemp.SelectCommand.ToLower().IndexOf("order by", StringComparison.Ordinal);
                                if (index != -1) {
                                    if (((_ctrlname != "hf_updatetable") || (!IsPostBack)) && (hf_current.Value != dd_table.SelectedValue)) {
                                        string[] x = { " " };
                                        string[] expressions = dbcollTemp.SelectCommand.Substring(index).Split(x, StringSplitOptions.RemoveEmptyEntries);
                                        if ((expressions[0].ToLower() == "order") && (expressions[1].ToLower() == "by") && (expressions.Length == 4) && (ViewState["SortExpression"] == null)) {
                                            ViewState["SortExpression"] = expressions[2];
                                            ViewState["SortDirection"] = expressions[3];
                                            _dbviewer.dt.DefaultView.Sort = expressions[2] + " " + expressions[3];
                                        }
                                        else {
                                            string se = ViewState["SortExpression"].ToString();
                                            string sd = ViewState["SortDirection"].ToString();
                                            _dbviewer.dt.DefaultView.Sort = se + " " + sd;
                                        }
                                    }
                                    else {
                                        string se = ViewState["SortExpression"].ToString();
                                        string sd = ViewState["SortDirection"].ToString();
                                        _dbviewer.dt.DefaultView.Sort = string.Format("{0} {1}", se, sd);
                                    }
                                }
                            }
                            catch {
                            }
                        }
                        lbtn_savetable.Enabled = false;
                        lbtn_savetable.Visible = false;
                        lbl_tablesaved_msg.Enabled = false;
                        lbl_tablesaved_msg.Visible = false;
                    }
                }
                else {
                    _dbviewer.GetTableData(tablename);
                    if (((_ctrlname != "hf_updatetable") || (!IsPostBack)) && (hf_current.Value != dd_table.SelectedValue)) {
                        hf_current.Value = dd_table.SelectedValue;
                        ViewState["SortExpression"] = _dbviewer.dt.Columns[0].ColumnName;
                        ViewState["SortDirection"] = "ASC";
                        _dbviewer.dt.DefaultView.Sort = string.Format("{0} {1}", _dbviewer.dt.Columns[0], "asc");
                    }
                    else {
                        string se = ViewState["SortExpression"].ToString();
                        string sd = ViewState["SortDirection"].ToString();
                        _dbviewer.dt.DefaultView.Sort = string.Format("{0} {1}", se, sd);
                    }

                    if (tablename.StartsWith("CT_")) {
                        string tempTablename = tablename;
                        CustomTableViewer ctv = new CustomTableViewer(_username);
                        tablename = ctv.GetTableNameByTableID(tablename);
                        if (string.IsNullOrEmpty(tablename)) {
                            tablename = tempTablename;
                        }
                        lbtn_savetable.Enabled = false;
                        lbtn_savetable.Visible = false;
                    }
                    else {
                        lbtn_savetable.Enabled = true;
                        lbtn_savetable.Visible = true;
                    }

                    lbtn_savetable.Text = "<span class='img-backup float-left margin-right-sml'></span>Backup " + tablename;
                    lbl_tablesaved_msg.Enabled = false;
                    lbl_tablesaved_msg.Visible = false;


                    if ((!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) && (!ServerSettings.AdminPagesCheck("dbManager", _username))) {
                        lbtn_savetable.Enabled = false;
                        lbtn_savetable.Visible = false;
                    }
                }

                Search_Found(_dbviewer.dt);
                BuildTableSchema(_dbviewer.dt, tablename);
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
    private void Search_Found(DataTable dt) {
        string x = tb_search.Text.ToLower().Trim();
        string rowCount = "0";

        var dtlist = new DataTable();
        foreach (DataColumn dc in dt.Columns) {
            dtlist.Columns.Add(new DataColumn(dc.ColumnName));
        }

        foreach (DataRow dr in dt.Rows) {
            int count = 0;
            bool cancontinue = false;
            while (count < dt.Columns.Count) {
                if (dt.Columns[count].ColumnName == "ApplicationId" && dr[count].ToString() != ServerSettings.ApplicationID) {
                    cancontinue = false;
                    break;
                }

                if ((x.ToLower() == "search database table") || (string.IsNullOrEmpty(x))) {
                    cancontinue = true;
                }
                else {
                    try {
                        string temp = dr[count].ToString().ToLower().Trim();
                        if (temp.Contains(x)) {
                            cancontinue = true;
                        }
                    }
                    catch {
                    }
                }
                count++;
            }

            if (cancontinue) {
                DataRow drlist = dtlist.NewRow();
                foreach (DataColumn dc in dt.Columns) {
                    drlist[dc.ColumnName] = dr[dc.ColumnName].ToString();
                }
                dtlist.Rows.Add(drlist);
            }
        }

        Session["TaskTable_dbviewer"] = dtlist;
        GV_dbviewer.DataSource = dtlist;
        GV_dbviewer.DataBind();

        rowCount = dtlist.Rows.Count.ToString();

        lbl_rowCount.Text = "<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Row Count</span>" + rowCount;
        UpdatePanel1.Update();
    }

    private void BuildTableSchema(DataTable dt, string tableName) {
        pnl_tableSchema.Controls.Clear();

        if (tableName.Contains("(Imported)")) {
            DBImporter dbImporter = new DBImporter();
            DBImporter_Coll tempColl = dbImporter.GetImportTableByTableId(tableName.Replace("(Imported)", string.Empty));
            if (!string.IsNullOrEmpty(tempColl.ID)) {
                tableName = tempColl.TableName + " (Imported)";
            }
        }

        StringBuilder str = new StringBuilder();
        if (dt.Columns.Count > 0) {
            str.Append("<table cellpadding='5' cellspacing='0'><tbody>");
            str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td style='min-width: 110px;'>Column Name</td><td style='min-width: 115px;'>Type</td><td>Nullable</td></tr>");

            int count = 1;
            foreach (DataColumn dc in dt.Columns) {
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center;'>" + count.ToString() + "</td>");
                str.Append("<td class='border-right border-bottom' style='text-align: left;'>" + dc.ColumnName + "</td>");

                string dataType = dc.DataType.Name;
                if (dc.MaxLength > 0) {
                    if (dc.MaxLength > 4000) {
                        dataType += "(MAX)";
                    }
                    else {
                        dataType += "(" + dc.MaxLength.ToString() + ")";
                    }
                }

                str.Append("<td class='border-right border-bottom' style='text-align: center;'>" + dataType + "</td>");

                string isNullable = "<span style='width:16px; height: 16px; float: left;'></span>";
                if (dc.AllowDBNull) {
                    isNullable = "<span class='img-checkmark'></span>";
                }

                str.Append("<td class='border-right border-bottom' style='text-align: center;'>" + isNullable + "</td></tr>");
                count++;
            }

            str.Append("</tbody></table>");
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            string collapseExpandBtn = "<a href='#' class='collapse-expand-schemebtn' onclick=\"CollapseExpandSchema();return false;\">Collapse</a>";

            StringBuilder str2 = new StringBuilder();
            str2.Append("<div class='table-settings-box'>");
            str2.Append("<div class='td-settings-title'>" + tableName + " Schema" + collapseExpandBtn + "<div class='clear'></div></div>");
            str2.Append("<div class='title-line'></div>");
            str2.Append("<div id='schematable-holder' class='td-settings-ctrl'>" + str.ToString() + "</div></div>");
            pnl_tableSchema.Controls.Add(new LiteralControl(str2.ToString()));
        }

        UpdatePanel4.Update();
    }

    protected void lbtn_savetable_Click(object sender, EventArgs e) {
        StartRunningBackup_Ind();

        lbl_tablesaved_msg.Enabled = true;
        lbl_tablesaved_msg.Visible = true;
    }
    private void StartRunningBackup_Ind() {
        string currTable = dd_table.SelectedValue;
        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        string loc = ServerSettings.GetServerMapLocation + "Backups\\" + currTable + "_" +
                     ServerSettings.ServerDateTime.ToFileTime() + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(HttpContext.Current.User.Identity.Name, loc);
        _dbviewer.GetTableData(currTable);
        sb.BinarySerialize_Ind(_dbviewer.dt, currTable);
        Thread.Sleep(200);
        WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), currTable + " table backup");
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

    protected void GV_dbviewer_PageIndexChanging(Object sender, GridViewPageEventArgs e) {
        GV_dbviewer.PageIndex = e.NewPageIndex;
        GV_dbviewer.DataBind();
        SetTableData();
    }
    protected void GV_dbviewer_RowCreated(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Header) {
            try {
                int count = 0;
                var sortExpression = ViewState["SortExpression"] as string;
                foreach (DataControlFieldCell tc in e.Row.Cells) {
                    tc.CssClass = "td-sort-click";
                    tc.ToolTip = "Sort by " + tc.ContainingField.HeaderText;
                    if (tc.ContainingField.HeaderText == sortExpression)
                        AddSortImage(count, e.Row);

                    count++;
                }
            }
            catch { }
        }
    }
    protected void dd_dbviewer_SelectedIndexChanged(object sender, EventArgs e) {
        if (dd_display.SelectedValue == "10") {
            SetPageSize(10);
        }
        else if (dd_display.SelectedValue == "20") {
            SetPageSize(20);
        }
        else if (dd_display.SelectedValue == "30") {
            SetPageSize(30);
        }
        else if (dd_display.SelectedValue == "40") {
            SetPageSize(40);
        }
        else if (dd_display.SelectedValue == "1") {
            SetPageSize(1);
        }
        GV_dbviewer.DataBind();
        SetTableData();
    }
    protected void GV_dbviewer_Sorting(object sender, GridViewSortEventArgs e) {
        var dt = Session["TaskTable_dbviewer"] as DataTable;
        if (dt != null) {
            dt.DefaultView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            GV_dbviewer.DataSource = dt;
            GV_dbviewer.DataBind();
        }
    }

    private void SetPageSize(int size) {
        switch (size) {
            case 10:
                GV_dbviewer.PageSize = 10;
                dd_display.SelectedIndex = 0;
                break;
            case 20:
                GV_dbviewer.PageSize = 20;
                dd_display.SelectedIndex = 1;
                break;
            case 30:
                GV_dbviewer.PageSize = 30;
                dd_display.SelectedIndex = 2;
                break;
            case 40:
                GV_dbviewer.PageSize = 40;
                dd_display.SelectedIndex = 3;
                break;
            default:
                GV_dbviewer.PageSize = 100000;
                dd_display.SelectedIndex = 4;
                break;
        }
    }
    private void AddSortImage(int columnIndex, GridViewRow headerRow) {
        try {
            string className = "";
            var lastDirection = ViewState["SortDirection"] as string;
            if ((lastDirection == "ASC") || (lastDirection == null))
                className = " active asc";
            else
                className = " active desc";

            headerRow.Cells[columnIndex].CssClass += className;
        }
        catch { }
    }
    private string GetSortDirection(string column) {
        string sortDirection = "ASC";
        var sortExpression = ViewState["SortExpression"] as string;
        if (sortExpression != null) {
            if (sortExpression == column) {
                var lastDirection = ViewState["SortDirection"] as string;
                if ((lastDirection != null) && (lastDirection == "ASC")) {
                    sortDirection = "DESC";
                }
            }
        }
        ViewState["SortDirection"] = sortDirection;
        ViewState["SortExpression"] = column;
        return sortDirection;
    }
    protected void hf_updatetable_Changed(object sender, EventArgs e) {
        SetTableData();
        RegisterPostbackScripts.RegisterStartupScript(this, "UpdateTables();");
    }

    #endregion
}