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

public partial class SiteTools_dbViewer : BasePage {

    #region Private Variables

    private readonly DBViewer _dbviewer = new DBViewer(true);
    private AutoBackupSystem _abs;
    private string CurrentSelected {
        get {
            if (ViewState["CurrentTableSelected"] != null) {
                return ViewState["CurrentTableSelected"].ToString();
            }

            return dd_table.SelectedValue;
        }
        set {
            ViewState["CurrentTableSelected"] = value;
        }
    }

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        _abs = new AutoBackupSystem(ServerSettings.GetServerMapLocation);

        lbl_tablesaved_msg.Enabled = false;
        lbl_tablesaved_msg.Visible = false;

        BuildDropDown(_dbviewer.dt);

        if (!AsyncPostBackSourceElementID.Contains("btn_updateRowsToSelect")) {
            SetTableData();
        }
    }

    #region Build Dropdown List

    private void BuildDropDown(DataTable dt) {
        App apps = new App(string.Empty);

        if (dd_table.Items.Count > 0) {
            CurrentSelected = dd_table.SelectedValue;
        }

        dd_table.Items.Clear();
        try {
            var dv = new DataView(dt);
            dv.Sort = string.Format("{0} {1}", dv.Table.Columns[2], "asc");
            dt = dv.ToTable();

            var db = new DBImporter();
            db.GetImportList();
            foreach (var dbcoll in db.DBColl) {
                if (AppExists_Import(dbcoll.TableID)) {
                    if ((CurrentUsername.ToLower() != dbcoll.ImportedBy.ToLower()) && (apps.GetIsPrivate(dbcoll.TableID))) {
                        continue;
                    }
                }

                if (!dbcoll.TableName.ToLower().Contains("aspnet_") && !dbcoll.TableName.ToLower().Contains("membership") && !dbcoll.TableName.ToLower().Contains("__migrationhistory")) {
                    var item = new ListItem(dbcoll.TableName + " (Imported)", dbcoll.TableID + "(Imported)");
                    if (!dd_table.Items.Contains(item)) {
                        dd_table.Items.Add(item);
                    }
                }
            }

            foreach (DataRow dr in dt.Rows) {
                if (!dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("membership") && !dr["TABLE_NAME"].ToString().ToLower().Contains("__migrationhistory")) {
                    string tableName = dr["TABLE_NAME"].ToString();

                    bool isCustomTable = false;
                    if (dr["TABLE_NAME"].ToString().StartsWith("CT_")) {
                        CustomTableViewer ctv = new CustomTableViewer(CurrentUsername);
                        CustomTable_Coll cInfo = ctv.GetTableInfoByTableId(tableName);

                        if (!string.IsNullOrEmpty(cInfo.ID)) {
                            if (AppExists_Custom(cInfo.AppID)) {
                                if ((CurrentUsername.ToLower() != cInfo.UpdatedBy.ToLower()) && (apps.GetIsPrivate(cInfo.AppID))) {
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
                    if (!IsUserInAdminRole()) {
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

        for (int i = 0; i < dd_table.Items.Count; i++) {
            if (dd_table.Items[i].Value == CurrentSelected) {
                dd_table.SelectedIndex = i;
                break;
            }
        }
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

    #endregion

    #region Set Table Data

    private void SetTableData() {
        try {
            bool found = false;
            DBImporter_Coll dbcollTemp = null;
            string tablename = CurrentSelected;
            if ((!string.IsNullOrEmpty(tablename)) && (!tablename.ToLower().Contains("delete"))) {
                if (tablename.Contains("(Imported)")) {
                    var db = new DBImporter();
                    dbcollTemp = db.GetImportTableByTableId(tablename.Replace("(Imported)", "").Trim());
                    if (!string.IsNullOrEmpty(dbcollTemp.ID)) {
                        found = true;
                    }
                }

                if (ViewState["RowsToSelect"] == null) {
                    ViewState["RowsToSelect"] = "1000";
                }

                tb_rowsToSelect.Text = ViewState["RowsToSelect"].ToString();
                int rowsToSelect = 0;
                if (!int.TryParse(tb_rowsToSelect.Text, out rowsToSelect)) {
                    rowsToSelect = 1000;
                }

                if ((found) && (dbcollTemp != null)) {
                    _dbviewer.GetImportedTableData(dbcollTemp, rowsToSelect);
                    lbtn_savetable.Enabled = false;
                    lbtn_savetable.Visible = false;
                    lbl_tablesaved_msg.Enabled = false;
                    lbl_tablesaved_msg.Visible = false;
                }
                else {
                    _dbviewer.GetTableData(tablename, true, rowsToSelect);
                    if (tablename.StartsWith("CT_")) {
                        string tempTablename = tablename;
                        CustomTableViewer ctv = new CustomTableViewer(CurrentUsername);
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

                    lbtn_savetable.Text = "Backup " + tablename;
                    lbl_tablesaved_msg.Enabled = false;
                    lbl_tablesaved_msg.Visible = false;


                    if (!IsUserInAdminRole() && !ServerSettings.AdminPagesCheck("dbManager", CurrentUsername)) {
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
        var dtlist = new DataTable();
        foreach (DataColumn dc in dt.Columns) {
            dtlist.Columns.Add(new DataColumn(dc.ColumnName));
        }

        foreach (DataRow dr in dt.Rows) {
            int count = 0;
            bool cancontinue = true;
            while (count < dt.Columns.Count) {
                if (dt.Columns[count].ColumnName == DatabaseCall.ApplicationIdString && dr[count].ToString() != ServerSettings.ApplicationID) {
                    cancontinue = false;
                    break;
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

        TableBuilder tableBuilder = BuildTable(dtlist, dt.Columns);

        pnl_gridviewholder.Controls.Clear();
        pnl_gridviewholder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No information found"));
    }
    private void BuildTableSchema(DataTable dt, string tableName) {
        if (tableName.Contains("(Imported)")) {
            DBImporter dbImporter = new DBImporter();
            DBImporter_Coll tempColl = dbImporter.GetImportTableByTableId(tableName.Replace("(Imported)", string.Empty));
            if (!string.IsNullOrEmpty(tempColl.ID)) {
                tableName = tempColl.TableName + " (Imported)";
            }
        }

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, false, 0, "TableSchema_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Column Name", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Type", "125", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Nullable", "50", false));
        tableBuilder.AddHeaderRow(headerColumns , false);
        #endregion

        #region Build Body
        if (dt.Columns.Count > 0) {
            foreach (DataColumn dc in dt.Columns) {
                string dataType = dc.DataType.Name;
                if (dc.MaxLength > 0) {
                    if (dc.MaxLength > 4000) {
                        dataType += "(MAX)";
                    }
                    else {
                        dataType += "(" + dc.MaxLength.ToString() + ")";
                    }
                }

                string isNullable = "<span style='width:16px; height: 16px; float: left;'></span>";
                if (dc.AllowDBNull) {
                    isNullable = "<span class='img-checkmark'></span>";
                }

                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Column Name", dc.ColumnName, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Type", dataType, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Nullable", isNullable, TableBuilderColumnAlignment.Center));
                tableBuilder.AddBodyRow(bodyColumnValues);
            }
        }
        #endregion

        StringBuilder str2 = new StringBuilder();
        str2.Append("<div id='schematable-holder' class='pad-left pad-right'>" + tableBuilder.CompleteTableString("No data found", false) + "</div>");

        pnl_tableSchema.Controls.Clear();
        pnl_tableSchema.Controls.Add(new LiteralControl(str2.ToString()));

        UpdatePanel4.Update();
    }
    private TableBuilder BuildTable(DataTable dt, DataColumnCollection columns) {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, false, 0);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        foreach (DataColumn dc in dt.Columns) {
            if (dc.ColumnName != DatabaseCall.ApplicationIdString) {
                headerColumns.Add(new TableBuilderHeaderColumns(dc.ColumnName, string.Empty, false));
            }
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (DataRow dr in dt.Rows) {
            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            foreach (DataColumn dc in columns) {
                try {
                    if (dc.ColumnName != DatabaseCall.ApplicationIdString) {
                        string value = dr[dc.ColumnName].ToString();
                        if (dc.DataType.Name.ToLower() == "boolean") {
                            string isChecked = "";
                            if (HelperMethods.ConvertBitToBoolean(value)) {
                                isChecked = " checked='checked'";
                            }
                            value = "<input type='checkbox' disabled='disabled'" + isChecked + " />";
                        }
                        else {
                            if (value.Contains(";")) {
                                string[] splitValue = value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                                value = string.Empty;
                                for (int i = 0; i < splitValue.Length; i++) {
                                    value += splitValue[i] + ";";
                                    if (i < splitValue.Length - 1) {
                                        value += "<br />";
                                    }
                                }
                            }
                        }
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues(dc.ColumnName, value, TableBuilderColumnAlignment.Left));
                    }
                }
                catch (Exception e) {
                    if (!string.IsNullOrEmpty(e.Message)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LogConsoleMessage('" + HttpUtility.UrlEncode(e.Message) + "');");
                    }
                }
            }
            tableBuilder.AddBodyRow(bodyColumnValues);
        }
        #endregion

        return tableBuilder;
    }

    #endregion

    #region Event Methods

    protected void lbtn_refresh_Click(object sender, EventArgs e) {
        SetTableData();
    }
    protected void dd_table_Changed(object sender, EventArgs e) {
        SetTableData();
    }
    protected void hf_updatetable_Changed(object sender, EventArgs e) {
        SetTableData();
        RegisterPostbackScripts.RegisterStartupScript(this, "UpdateTables();");
    }

    protected void btn_updateRowsToSelect_Click(object sender, EventArgs e) {
        ViewState["RowsToSelect"] = tb_rowsToSelect.Text;
        SetTableData();
    }

    #endregion

    #region Backup Methods

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
        AutoBackupSystem.WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), currTable + " table backup");
    }

    #endregion

}