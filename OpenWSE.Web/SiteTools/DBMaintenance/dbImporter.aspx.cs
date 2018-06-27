#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using OpenWSE_Tools.GroupOrganizer;
using System.Web.Script.Serialization;

#endregion

public partial class SiteTools_dbImporter : BasePage {

    private List<DBImporter_Coll> _coll = new List<DBImporter_Coll>();
    private DBViewer _dbviewer = new DBViewer(true);
    private DBImporter dbImporter = new DBImporter();

    protected void Page_Load(object sender, EventArgs e) {
        HelperMethods.SetIsSocialUserForDeleteItems(Page, CurrentUsername);
        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);

        if (MainServerSettings.LockCustomTables) {
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
            importNewWizardBtn.Visible = false;

            dbImporter.GetImportList();
            _coll = dbImporter.DBColl;
            GetConnections();

            if (!IsPostBack) {
                BuildTableList();
            }
        }
        else {
            if (IsUserNameEqualToAdmin()) {
                cb_InstallAfterLoad.Visible = false;
                cb_InstallAfterLoad.Enabled = false;
                cb_isPrivate.Visible = false;
                cb_isPrivate.Enabled = false;
            }
            else {
                cb_InstallAfterLoad.Visible = true;
                cb_InstallAfterLoad.Enabled = true;
                cb_isPrivate.Visible = true;
                cb_isPrivate.Enabled = true;
            }

            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                cb_InstallAfterLoad.Enabled = false;
                cb_InstallAfterLoad.Visible = false;
                cb_isPrivate.Enabled = false;
                cb_isPrivate.Visible = false;
                cb_InstallAfterLoad.Checked = false;
                cb_isPrivate.Checked = false;
            }

            if (!IsUserInAdminRole()) {
                lbtn_uselocaldatasource.Visible = false;
                lbtn_uselocaldatasource.Enabled = false;
            }

            BuildChartTypeList();
            BuildProviderList();

            dbImporter.GetImportList();
            _coll = dbImporter.DBColl;
            GetConnections();

            if (!IsPostBack) {
                CancelWizard();
                BuildUsersAllowedToEdit();
                rb_adv_disabled.Checked = true;
                rb_adv_enabled.Checked = false;
                pnl_txtselect.Visible = false;
                pnl_txtselect.Enabled = false;

                BuildTableList();
            }
        }
    }


    #region Import Wizard - Base

    protected void btn_CancelWizard_Click(object sender, EventArgs e) {
        CancelWizard();
    }
    private void CancelWizard() {
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        tb_connstring.Text = string.Empty;
        tb_selectcomm.Text = string.Empty;
        cb_ddselect.Items.Clear();
        if ((rb_adv_enabled.Checked) && (!rb_adv_disabled.Checked)) {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
            pnl_txtselect.Enabled = true;
            pnl_txtselect.Visible = true;
        }
        else {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
        }
        dd_ddtables.Items.Clear();

        BuildUsersAllowedToEdit();
        RegisterPostbackScripts.RegisterStartupScript(this, "ResetImportWizardControls();");
    }

    private void BuildProviderList() {
        if (dd_provider.Items.Count == 0) {
            foreach (string providerVal in DatabaseProviders.ProviderList) {
                dd_provider.Items.Add(new ListItem(providerVal, providerVal));
            }
        }
    }
    private void BuildUsersAllowedToEdit() {
        pnl_usersAllowedToEdit.Controls.Clear();

        StringBuilder str = new StringBuilder();
        string checkboxInput = "<div class='checkbox-new-click float-left pad-right-big pad-top pad-bottom' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";

        MembershipUserCollection userColl = Membership.GetAllUsers();
        foreach (MembershipUser membershipUser in userColl) {
            string user = membershipUser.UserName;
            if (IsUserNameEqualToAdmin(user)) {
                continue;
            }

            string isChecked = string.Empty;
            if (user.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                isChecked = "checked='checked'";
                hf_usersAllowedToEdit.Value += user.ToLower() + ServerSettings.StringDelimiter;
            }
            MemberDatabase tempMember = new MemberDatabase(user);

            string un = HelperMethods.MergeFMLNames(tempMember);
            if ((user.Length > 15) && (!string.IsNullOrEmpty(tempMember.LastName)))
                un = tempMember.FirstName + " " + tempMember.LastName[0].ToString() + ".";

            if (un.ToLower() == "n/a")
                un = user;

            string marginTop = "8px";
            string userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
            string acctImage = tempMember.AccountImage;

            string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30, tempMember.SiteTheme);
            str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        pnl_usersAllowedToEdit.Controls.Add(new LiteralControl(str.ToString()));
        updatePnl_UsersAllowedToEdit.Update();
    }
    private void BuildUsersAllowedToEdit_ForEdit(string id) {
        pnl_usersAllowedToEdit.Controls.Clear();

        StringBuilder str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            DBImporter db = new DBImporter();
            DBImporter_Coll coll = db.GetImportTableByTableId(id);

            if (!string.IsNullOrEmpty(coll.ID)) {
                string checkboxInput = "<div class='checkbox-edit-click float-left pad-right-big pad-top pad-bottom' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";

                MembershipUserCollection userColl = Membership.GetAllUsers();
                foreach (MembershipUser membershipUser in userColl) {
                    string user = membershipUser.UserName;
                    if (IsUserNameEqualToAdmin(user)) {
                        continue;
                    }

                    string isChecked = string.Empty;
                    bool foundUser = !string.IsNullOrEmpty(coll.UsersAllowedToEdit.Find(_x => _x.ToLower() == user.ToLower()));
                    if (foundUser) {
                        isChecked = "checked='checked'";
                    }
                    MemberDatabase tempMember = new MemberDatabase(user);

                    string un = HelperMethods.MergeFMLNames(tempMember);
                    if ((user.Length > 15) && (!string.IsNullOrEmpty(tempMember.LastName)))
                        un = tempMember.FirstName + " " + tempMember.LastName[0].ToString() + ".";

                    if (un.ToLower() == "n/a")
                        un = user;

                    string marginTop = "8px";
                    string userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                    string acctImage = tempMember.AccountImage;

                    string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30, tempMember.SiteTheme);
                    str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
                }
            }
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        pnl_usersAllowedToEdit.Controls.Add(new LiteralControl(str.ToString()));
        updatePnl_UsersAllowedToEdit.Update();
    }
    private void BuildChartTypeList() {
        ddl_ChartType.Items.Clear();
        Array chartTypes = Enum.GetValues(typeof(ChartType));
        foreach (ChartType type in chartTypes) {
            ddl_ChartType.Items.Add(new ListItem(type.ToString(), type.ToString()));
        }
    }

    #endregion


    #region Import Wizard - Database Entry

    protected void lbtn_uselocaldatasource_Click(object sender, EventArgs e) {
        ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
        string ds = DBImporter.HideUsernameAndPasswordForConnectionString(connString.ConnectionString);
        tb_connstring.Text = ds;

        for (int i = 0; i < dd_provider.Items.Count; i++) {
            if (dd_provider.Items[i].Value == connString.ProviderName) {
                dd_provider.SelectedIndex = i;
                break;
            }
        }

        if (TestSettings()) {
            pnl_ddselect.Enabled = true;
            pnl_ddselect.Visible = true;
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;

            rb_adv_enabled.Checked = false;
            rb_adv_disabled.Checked = true;

            BuildDropDown();
            SetTableData();
        }
    }
    protected void lbtn_loadselectcommand_Click(object sender, EventArgs e) {
        if (TestSettings()) {
            pnl_ddselect.Enabled = true;
            pnl_ddselect.Visible = true;
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;
            ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
            if (GetCorrectConnectionString()[0] == connString.ConnectionString) {
                //BuildDropDown();
                SetTableData();
            }
            else {
                TryGetExternalDb();
            }
        }
    }

    protected void btn_test_Click(object sender, EventArgs e) {
        if (TestSettings()) {
            TryGetExternalDb();
        }
    }
    private bool TestSettings() {
        bool successfull;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;

        if (!IsUserInAdminRole()) {
            if (CheckIfLocalTableAndStandard()) {
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
                lbl_error.Text = "You do not have permission to import a local table.";
                lbl_error.ForeColor = Color.Red;
                RegisterPostbackScripts.RegisterStartupScript(this, "testConnectionGood=false;");
                return false;
            }
        }

        if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked)) {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;
        }

        if (!string.IsNullOrEmpty(tb_connstring.Text)) {
            string provider = dd_provider.SelectedValue;
            bool success = false;
            try {
                using (DbConnection defaultconn = DatabaseProviders.CreateConnectionObject(provider)) {
                    try {
                        if (defaultconn != null) {
                            defaultconn.ConnectionString = GetCorrectConnectionString()[0];
                            defaultconn.Open();
                            success = true;
                        }
                    }
                    catch (Exception e) {
                        success = false;
                        lbl_error.Text = e.Message;
                    }
                }

                if (success) {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.Text = "Successful Connection";
                    lbl_error.ForeColor = Color.Green;
                    successfull = true;
                    RegisterPostbackScripts.RegisterStartupScript(this, "testConnectionGood=true;");
                }
                else {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.ForeColor = Color.Red;
                    successfull = false;
                    RegisterPostbackScripts.RegisterStartupScript(this, "testConnectionGood=false;");
                }

            }
            catch (Exception e) {
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
                lbl_error.Text = e.Message;
                lbl_error.ForeColor = Color.Red;
                successfull = false;
                RegisterPostbackScripts.RegisterStartupScript(this, "testConnectionGood=false;");
            }
        }
        else {
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
            lbl_error.Text = "Connection String must be filled out.";
            lbl_error.ForeColor = Color.Red;
            successfull = false;
            RegisterPostbackScripts.RegisterStartupScript(this, "testConnectionGood=false;");
        }

        BuildTableList();

        if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked) && (successfull)) {
            pnl_ddselect.Enabled = true;
            pnl_ddselect.Visible = true;
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;
        }
        else if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked) && (!successfull)) {
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;
        }

        return successfull;
    }
    private string[] GetCorrectConnectionString() {
        string[] cs = new string[2];
        if (tb_connstring.Text.ToLower().Contains("use connection string")) {
            dbImporter.GetSavedConnectionList();
            int indexof = tb_connstring.Text.IndexOf("'");
            if (indexof != -1) {
                string id = tb_connstring.Text.Substring(indexof + 1);
                id = id.Replace("'", "");
                foreach (var sc in dbImporter.SavedConnections_Coll) {
                    if (sc.ID == id) {
                        cs[0] = sc.ConnectionString;
                        cs[1] = sc.DatabaseProvider;
                        break;
                    }
                }
            }
            else {
                cs[0] = tb_connstring.Text.Trim();
                cs[1] = dd_provider.SelectedValue;
            }
        }
        else {
            ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
            if (DBImporter.HideUsernameAndPasswordForConnectionString(tb_connstring.Text.Trim()) == DBImporter.HideUsernameAndPasswordForConnectionString(connString.ConnectionString)) {
                cs[0] = connString.ConnectionString;
            }
            else {
                cs[0] = tb_connstring.Text.Trim();
            }

            cs[1] = dd_provider.SelectedValue;
        }

        return cs;
    }
    private string GetCorrectProvider() {
        string dp = string.Empty;
        dbImporter.GetSavedConnectionList();
        foreach (var sc in dbImporter.SavedConnections_Coll) {
            if (sc.ConnectionString == GetCorrectConnectionString()[0]) {
                dp = sc.DatabaseProvider;
                break;
            }
        }

        return dp;
    }

    #endregion


    #region Import Wizard - Column Setup

    protected void rb_adv_enabled_Checked(object sender, EventArgs e) {
        rb_adv_disabled.Checked = false;
        rb_adv_enabled.Checked = true;
        pnl_txtselect.Visible = true;
        pnl_txtselect.Enabled = true;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        pnl_ddselect.Enabled = false;
        pnl_ddselect.Visible = false;

        string conditional = " ";
        if ((!string.IsNullOrEmpty(tb_conditional.Text)) &&
            (tb_conditional.Text.ToLower() != "[conditional statement]")) {
            conditional = " " + tb_conditional.Text + " ";
        }

        string tbDdselect = string.Empty;
        int i = 0;
        foreach (ListItem li in cb_ddselect.Items) {
            if (li.Selected) {
                tbDdselect += li.Value + ", ";
                i++;
            }
        }
        if ((i == 0)) {
            tbDdselect = "*";
        }
        else {
            try {
                string temp = tbDdselect;
                temp = temp.Remove(temp.Length - 2);
                tbDdselect = temp;
            }
            catch {
            }
        }

        string command = "SELECT " + tbDdselect + " FROM " + dd_ddtables.SelectedValue + conditional +
                         "ORDER BY " + dd_orderby.SelectedValue + " " + dd_orderdirection.SelectedValue;

        tb_selectcomm.Text = command;
        TestSettings();
    }
    protected void rb_adv_disabled_Checked(object sender, EventArgs e) {
        rb_adv_disabled.Checked = true;
        rb_adv_enabled.Checked = false;

        pnl_txtselect.Visible = false;
        pnl_txtselect.Enabled = false;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;

        if (TestSettings()) {
            ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
            if (GetCorrectConnectionString()[0] == connString.ConnectionString) {
                BuildDropDown();
                SetTableData();
            }
            else {
                TryGetExternalDb();
            }

            if (!string.IsNullOrEmpty(tb_selectcomm.Text)) {
                try {
                    DatabaseCall dbCall = new DatabaseCall(GetCorrectConnectionString()[1], GetCorrectConnectionString()[0]);
                    DataTable dt = dbCall.CallGetDataTableBySelectStatement(tb_selectcomm.Text);
                    if (!string.IsNullOrEmpty(dt.TableName)) {
                        foreach (ListItem item in dd_ddtables.Items) {
                            if (item.Value == dt.TableName) {
                                item.Selected = true;
                            }
                            else {
                                item.Selected = false;
                            }
                        }
                    }

                    SetTableData();
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
        }
    }

    private void BuildDropDown() {
        dd_ddtables.Items.Clear();
        try {
            var dv = new DataView(_dbviewer.dt);
            dv.Sort = string.Format("{0} {1}", dv.Table.Columns[2], "asc");
            DataTable dt = dv.ToTable();
            foreach (DataRow dr in dt.Rows) {
                if (!dr["TABLE_NAME"].ToString().ToLower().StartsWith("ct_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("membership") && !dr["TABLE_NAME"].ToString().ToLower().Contains("__migrationhistory")) {
                    var item = new ListItem(dr["TABLE_NAME"].ToString(), dr["TABLE_NAME"].ToString());
                    if (!dd_ddtables.Items.Contains(item)) {
                        dd_ddtables.Items.Add(item);
                    }
                }
            }
        }
        catch (Exception) {
        }
    }
    private void BuildDropDown_External() {
        dd_ddtables.Items.Clear();
        try {
            var dv = new DataView(_dbviewer.dt);
            dv.Sort = string.Format("{0} {1}", dv.Table.Columns[2], "asc");
            DataTable dt = dv.ToTable();
            foreach (DataRow dr in dt.Rows) {
                var item = new ListItem(dr["TABLE_NAME"].ToString(), dr["TABLE_NAME"].ToString());
                if (!dd_ddtables.Items.Contains(item)) {
                    dd_ddtables.Items.Add(item);
                }
            }
        }
        catch {
        }
    }

    protected void dd_ddtables_Changed(object sender, EventArgs e) {
        SetTableData();
    }
    protected void cb_ddselect_Changed(object sender, EventArgs e) {
        try {
            var colnames = new List<string>();
            int i = 0;
            foreach (var item in cb_ddselect.Items.Cast<ListItem>().Where(item => item.Selected)) {
                colnames.Add(item.Value);
                i++;
            }

            if (i == 0) {
                colnames.Clear();
                colnames.AddRange(from ListItem item in cb_ddselect.Items select item.Value);
            }

            BuildColumns(colnames);
        }
        catch {
        }
    }

    private void BuildColumns(IEnumerable<string> colnames) {
        dd_orderby.Items.Clear();
        foreach (string x in colnames) {
            var item = new ListItem(x, x);
            dd_orderby.Items.Add(item);
        }
    }
    private static string BuildTextboxForSelect(string name, bool allowNull, bool disablBox = false) {
        string isDiabledTbStyle = string.Empty;
        string willBeHiddenText = string.Empty;
        if (disablBox) {
            isDiabledTbStyle = " disabled='disabled'";
            willBeHiddenText = "<div class='clear'></div><i class='pad-top-sml float-left' style='font-size: 10px;'>Column will be hidden but is required</i><div class='clear'></div>";
        }

        return "<input type='text' class='column-namechange textEntry float-right' maxlength='100' value='" + name + "'" + isDiabledTbStyle + " style='width: 210px; margin-top: -5px; margin-left: 35px;' /><span class='column-allownull' style='display: none;'>" + allowNull.ToString().ToLower() + "</span>" + willBeHiddenText;
    }
    private void SetTableData() {
        try {
            var colnames = new List<string>();
            string tablename = dd_ddtables.SelectedValue;

            string[] connectionStringInfo = GetCorrectConnectionString();
            _dbviewer.GetTableData(tablename, connectionStringInfo[1], connectionStringInfo[0]);

            DatabaseCall dbCall = new DatabaseCall();
            bool isLocal = false;
            if (dbCall.DataProvider == connectionStringInfo[1] && dbCall.ConnectionString == connectionStringInfo[0]) {
                isLocal = true;
            }

            cb_ddselect.Items.Clear();

            for (int i = 0; i < _dbviewer.dt.Columns.Count; i++) {
                var item = new ListItem("&nbsp;" + _dbviewer.dt.Columns[i].ColumnName + BuildTextboxForSelect(_dbviewer.dt.Columns[i].ColumnName, _dbviewer.dt.Columns[i].AllowDBNull, isLocal && _dbviewer.dt.Columns[i].ColumnName == DatabaseCall.ApplicationIdString), _dbviewer.dt.Columns[i].ColumnName);
                if (!cb_ddselect.Items.Contains(item)) {
                    cb_ddselect.Items.Add(item);
                    item.Selected = true;

                    if (isLocal && _dbviewer.dt.Columns[i].ColumnName == DatabaseCall.ApplicationIdString) {
                        item.Enabled = false;
                    }
                }

                colnames.Add(_dbviewer.dt.Columns[i].ColumnName);
            }
            BuildColumns(colnames);
        }
        catch {
        }
    }
    private void TryGetExternalDb() {
        var connString = new ConnectionStringSettings("ExternalDB", GetCorrectConnectionString()[0], GetCorrectConnectionString()[1]);
        _dbviewer = new DBViewer(true, connString);
        BuildDropDown_External();
        SetTableData();
    }

    #endregion


    #region Import Wizard - Create

    protected void btn_import_Click(object sender, EventArgs e) {
        if (!TestSettings()) {
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
            lbl_error.Text = "Failed. Check your connection string.";
            lbl_error.ForeColor = Color.Red;
            return;
        }

        if (pnl_txtselect.Enabled && pnl_txtselect.Visible && (pnl_ddselect.Enabled == false) &&
            (pnl_ddselect.Visible == false)) {
            if ((!string.IsNullOrEmpty(tb_Databasename.Text)) && (!string.IsNullOrEmpty(tb_connstring.Text)) && (!string.IsNullOrEmpty(tb_selectcomm.Text))) {
                lbl_error.Enabled = false;
                lbl_error.Visible = false;
                try {
                    if ((tb_selectcomm.Text.ToLower().Contains("delete ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("update ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("insert ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("create ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("commit ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("rollback ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("check ")) ||
                        (tb_selectcomm.Text.ToLower().Contains("grant revoke "))) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading! This table is not allowed.');");
                    }
                    else if (tb_selectcomm.Text.ToLower().Contains("aspnet_")) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading! This table is not allowed.');");
                    }
                    else if ((tb_selectcomm.Text.ToLower().Substring(0, 6) == "select") && (!tb_selectcomm.Text.ToLower().Contains("aspnet_"))) {
                        string randomId = HelperMethods.RandomString(10);

                        dbImporter.AddImport(randomId, tb_Databasename.Text.Trim(), tb_description.Text.Trim(), GetCorrectConnectionString()[0], tb_selectcomm.Text.Trim(), GetCorrectConnectionString()[1],
                            HttpContext.Current.User.Identity.Name, cb_AllowEditAdd.Checked, hf_usersAllowedToEdit.Value, cb_allowNotifi.Checked,
                            hf_columnOverrides.Value.Trim(), hf_customizations.Value.Trim(), hf_summaryData.Value.Trim());

                        dbImporter.GetImportList();
                        _coll = dbImporter.DBColl;

                        SaveConnections();
                        CreateApp(cb_AllowEditAdd.Checked, randomId);
                        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
                            aib.BuildAppsForUser(true);
                        }

                        CancelWizard();
                        RegisterPostbackScripts.RegisterStartupScript(this, "FinishEditCreate('" + randomId + "');");
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading! Only SELECT statements allowed.');");
                    }
                }
                catch {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading!');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('All fields must be filled out.');");
            }
        }
        else {
            SaveConnections();
            ImportDatabaseFromLocal();
        }
        BuildTableList();
    }
    private void ImportDatabaseFromLocal() {
        if ((!string.IsNullOrEmpty(tb_Databasename.Text)) && (!string.IsNullOrEmpty(tb_connstring.Text))) {
            lbl_error.Enabled = false;
            lbl_error.Visible = false;
            try {
                if ((tb_conditional.Text.ToLower().Contains("delete")) ||
                    (tb_conditional.Text.ToLower().Contains("update")) ||
                    (tb_conditional.Text.ToLower().Contains("insert")) ||
                    (tb_conditional.Text.ToLower().Contains("create")) ||
                    (tb_conditional.Text.ToLower().Contains("commit")) ||
                    (tb_conditional.Text.ToLower().Contains("rollback")) ||
                    (tb_conditional.Text.ToLower().Contains("grant revoke"))) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading! Only SELECT statements allowed.');");
                }
                else if (tb_conditional.Text.ToLower().Contains("aspnet_") || tb_conditional.Text.ToLower().Contains("membership")) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading! This table is not allowed.');");
                }
                else {
                    string randomId = HelperMethods.RandomString(10);
                    string conditional = " ";
                    if ((!string.IsNullOrEmpty(tb_conditional.Text)) &&
                        (tb_conditional.Text.ToLower() != "[conditional statement]")) {
                        conditional = " " + tb_conditional.Text + " ";
                    }

                    string tbDdselect = string.Empty;
                    int i = 0;
                    foreach (ListItem li in cb_ddselect.Items) {
                        if (li.Selected) {
                            tbDdselect += li.Value + ", ";
                            i++;
                        }
                    }
                    if ((i == 0)) {
                        tbDdselect = "*";
                    }
                    else {
                        try {
                            string temp = tbDdselect;
                            temp = temp.Remove(temp.Length - 2);
                            tbDdselect = temp;
                        }
                        catch {
                        }
                    }

                    string command = "SELECT " + tbDdselect + " FROM " + dd_ddtables.SelectedValue + conditional +
                                     "ORDER BY " + dd_orderby.SelectedValue + " " + dd_orderdirection.SelectedValue;

                    dbImporter.AddImport(randomId, tb_Databasename.Text.Trim(), tb_description.Text.Trim(), GetCorrectConnectionString()[0], command, GetCorrectConnectionString()[1],
                        HttpContext.Current.User.Identity.Name, cb_AllowEditAdd.Checked, hf_usersAllowedToEdit.Value, cb_allowNotifi.Checked,
                        hf_columnOverrides.Value.Trim(), hf_customizations.Value.Trim(), hf_summaryData.Value.Trim());

                    dbImporter.GetImportList();
                    _coll = dbImporter.DBColl;

                    CreateApp(cb_AllowEditAdd.Checked, randomId);
                    if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                        AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
                        aib.BuildAppsForUser(true);
                    }

                    CancelWizard();
                    RegisterPostbackScripts.RegisterStartupScript(this, "FinishEditCreate('" + randomId + "');");
                }
            }
            catch {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error Uploading!');");
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('All fields must be filled out.');");
        }
    }
    private bool CheckIfLocalTableAndStandard() {
        ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];

        string localString = connString.ConnectionString;
        string cs = GetCorrectConnectionString()[0];

        string[] localStringSplit = localString.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        string[] csSplit = cs.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        string reBuiltString = "";
        foreach (string a in localStringSplit) {
            string aX = Regex.Replace(a, @"\s+", "").ToLower();
            foreach (string b in csSplit) {
                string bX = Regex.Replace(b, @"\s+", "").ToLower();
                if (aX == bX) {
                    reBuiltString += aX;
                    break;
                }
            }
        }

        if (Regex.Replace(localString, @"\s+", "").Replace(ServerSettings.StringDelimiter, "").ToLower() == reBuiltString) {
            return true;
        }

        return false;
    }
    private void CreateApp(bool allowEdit, string fileName, string wName = "") {
        string picname = DBImporter.DefaultDatabaseIcon;
        var apps = new App(string.Empty);
        string categoryid = AppCategoryID();

        if (string.IsNullOrEmpty(wName))
            wName = tb_Databasename.Text.Trim();

        dbImportAppCreator dbiwc = new dbImportAppCreator(CurrentUsername, allowEdit);
        if (pnl_txtselect.Enabled && pnl_txtselect.Visible && (pnl_ddselect.Enabled == false) && (pnl_ddselect.Visible == false)) {

            bool _isPrivate = cb_isPrivate.Checked;
            if (!cb_InstallAfterLoad.Checked) {
                _isPrivate = false;
            }

            string description = tb_description.Text.Trim();
            if (string.IsNullOrEmpty(description)) {
                description = "Imported Database. Table Name: " + wName + ". Select Statment: " + tb_selectcomm.Text + ".";
            }

            dbiwc.CreateApp(wName, categoryid, description, picname, _isPrivate, cb_allowNotifi.Checked, true, fileName);

            if (cb_InstallAfterLoad.Checked)
                CurrentUserMemberDatabase.UpdateEnabledApps("app-" + fileName);
        }
        else {
            string conditional = " ";
            if ((!string.IsNullOrEmpty(tb_conditional.Text)) &&
                (tb_conditional.Text.ToLower() != "[conditional statement]")) {
                conditional = " " + tb_conditional.Text + " ";
            }

            string tbDdselect = string.Empty;
            int i = 0;
            foreach (var li in cb_ddselect.Items.Cast<ListItem>().Where(li => li.Selected)) {
                tbDdselect += li.Value + ", ";
                i++;
            }

            if ((i == 0)) {
                tbDdselect = "*";
            }
            else {
                try {
                    string temp = tbDdselect;
                    temp = temp.Remove(temp.Length - 2);
                    tbDdselect = temp;
                }
                catch { }
            }

            string command = "SELECT " + tbDdselect + " FROM " + dd_ddtables.SelectedValue + conditional +
                             "ORDER BY " + dd_orderby.SelectedValue + " " + dd_orderdirection.SelectedValue;

            bool _isPrivate = cb_isPrivate.Checked;
            if (!cb_InstallAfterLoad.Checked) {
                _isPrivate = false;
            }

            string description = tb_description.Text.Trim();
            if (string.IsNullOrEmpty(description)) {
                description = "Imported Database. Table Name: " + wName + ". Select Statment: " + command + ".";
            }

            dbiwc.CreateApp(wName, categoryid, description, picname, _isPrivate, cb_allowNotifi.Checked, true, fileName);
            if (cb_InstallAfterLoad.Checked)
                CurrentUserMemberDatabase.UpdateEnabledApps("app-" + fileName);
        }
    }
    private string AppCategoryID() {
        string categoryid = string.Empty;
        var widcat = new AppCategory(true);
        foreach (Dictionary<string, string> drTemp in widcat.category_dt) {
            if (drTemp["Category"].ToLower() == "table imports") {
                categoryid = drTemp["ID"];
                break;
            }
        }

        if (string.IsNullOrEmpty(categoryid)) {
            widcat.addItem("Table Imports");
            widcat = new AppCategory(true);
        }

        foreach (Dictionary<string, string> drTemp in widcat.category_dt) {
            if (drTemp["Category"].ToLower() == "table imports") {
                categoryid = drTemp["ID"];
                break;
            }
        }

        return categoryid;
    }

    #endregion


    #region saved connection strings
    protected void hf_addconnectionstring_ValueChanged(object sender, EventArgs e) {
        string connectionName = HttpUtility.UrlDecode(hf_connectionNameEdit.Value.Trim());
        string connectionString = HttpUtility.UrlDecode(hf_connectionStringEdit.Value.Trim());
        string databaseProvider = HttpUtility.UrlDecode(hf_databaseProviderEdit.Value.Trim());

        if (!string.IsNullOrEmpty(connectionName) && !string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseProvider)) {
            dbImporter.AddConnection(connectionString, connectionName, databaseProvider, CurrentUsername);
            GetConnections();
        }
        else {
            var str = new StringBuilder();
            str.Append("openWSE.AlertWindow('Cannot add new connection string. Fields cannot be empty.');");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }

        hf_connectionNameEdit.Value = string.Empty;
        hf_connectionStringEdit.Value = string.Empty;
        hf_databaseProviderEdit.Value = string.Empty;
        hf_addconnectionstring.Value = string.Empty;
    }

    private void GetConnections() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2, "SavedConnections_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Connection Name", "200px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Connection String", "400px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Database Provider", "225px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        bool foundEdit = false;
        dbImporter.GetSavedConnectionList();
        foreach (var sc in dbImporter.SavedConnections_Coll) {
            string ds = DBImporter.HideUsernameAndPasswordForConnectionString(sc.ConnectionString);

            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

            if (hf_editstring.Value == sc.ID) {
                bodyColumns.Add(new TableBuilderBodyColumnValues("Connection Name", "<input id='tb_connNameedit' type='text' value='" + sc.ConnectionName + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 100%;' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Connection String", "<input id='tb_connStringedit' type='text' value='" + ds + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 100%;' />", TableBuilderColumnAlignment.Left));

                string options = "<select id='edit-databaseProvider' style='min-width: 150px;'>";
                foreach (string providerVal in DatabaseProviders.ProviderList) {
                    string selected = "";
                    if (providerVal == sc.DatabaseProvider) {
                        selected = " selected='selected'";
                    }
                    options += "<option" + selected + " value='" + providerVal + "'>" + providerVal + "</option>";
                }
                options += "</select>";

                bodyColumns.Add(new TableBuilderBodyColumnValues("Database Provider", options, TableBuilderColumnAlignment.Left));

                var str2 = new StringBuilder();
                str2.Append("<a onclick='UpdateConnectionString(\"" + sc.ID + "\");return false;' class='td-update-btn' title='Update'></a>");
                str2.Append("<a onclick='EditConnectionString(\"reset\");return false;' class='td-cancel-btn ' title='Cancel'></a>");

                tableBuilder.AddBodyRow(bodyColumns, str2.ToString());
                foundEdit = true;
            }
            else {
                bodyColumns.Add(new TableBuilderBodyColumnValues("Connection Name", sc.ConnectionName, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Connection String", ds, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Database Provider", sc.DatabaseProvider, TableBuilderColumnAlignment.Left));

                var str2 = new StringBuilder();
                if (!MainServerSettings.LockCustomTables) {
                    // str2.Append("<a href='#use' onclick='UseConnectionString(\"" + sc.ID + "\", \"" + sc.ConnectionName + "\");return false;' class='td-download-btn' title='Use String'></a>");
                    if (IsUserInAdminRole() || CurrentUsername.ToLower() == sc.Username.ToLower()) {
                        str2.Append("<a href='#edit' onclick='EditConnectionString(\"" + sc.ID + "\");return false;' class='td-edit-btn' title='Edit'></a>");
                        str2.Append("<a href='#delete' onclick='DeleteConnectionString(\"" + sc.ID + "\");return false;' class='td-delete-btn' title='Delete'></a>");
                    }
                }

                tableBuilder.AddBodyRow(bodyColumns, str2.ToString());
            }
        }
        #endregion

        #region Build Insert Row
        if (!foundEdit && !MainServerSettings.LockCustomTables) {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
            insertColumns.Add(new TableBuilderInsertColumnValues("Connection Name", "<input type=\"text\" id=\"tb_connectionname\" onkeypress=\"KeyPress_AddConnectionString(event);\" class=\"textEntry\" maxlength=\"50\" style=\"width: 100%;\" />", TableBuilderColumnAlignment.Left));
            insertColumns.Add(new TableBuilderInsertColumnValues("Connection String", "<input type=\"text\" id=\"tb_connectionstring\" onkeypress=\"KeyPress_AddConnectionString(event);\" class=\"textEntry\" style=\"width: 100%;\" />", TableBuilderColumnAlignment.Left));

            string options2 = "<select id=\"dd_provider_connectionstring\">";
            foreach (string providerVal in DatabaseProviders.ProviderList) {
                options2 += "<option value=\"" + providerVal + "\">" + providerVal + "</option>";
            }
            options2 += "</select>";

            insertColumns.Add(new TableBuilderInsertColumnValues("Database Provider", options2, TableBuilderColumnAlignment.Left));
            tableBuilder.AddInsertRow(insertColumns, "AddConnectionString_Clicked();");
        }
        #endregion

        pnl_savedconnections_holder.Controls.Clear();
        pnl_savedconnections_holder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No saved connections found"));

        if (!string.IsNullOrEmpty(hf_editstring.Value)) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#tb_connNameedit').focus();");
        }
    }

    private void SaveConnections() {
        if ((!string.IsNullOrEmpty(tb_connstring.Text.Trim())) && (!string.IsNullOrEmpty(tb_Databasename.Text.Trim()))) {
            bool found = false;
            dbImporter.GetSavedConnectionList();
            foreach (SavedConnections sc in dbImporter.SavedConnections_Coll) {
                if (DBImporter.HideUsernameAndPasswordForConnectionString(sc.ConnectionString) == tb_connstring.Text.Trim()) {
                    found = true;
                    break;
                }
            }

            if (!found && !tb_connstring.Text.Contains("Use connection string")) {
                dbImporter.AddConnection(GetCorrectConnectionString()[0], "DB Import " + (dbImporter.SavedConnections_Coll.Count + 1).ToString(), GetCorrectConnectionString()[1], CurrentUsername);
            }

            GetConnections();
        }
    }

    protected void lbtn_useSavedDatasource_Click(object sender, EventArgs e) {
        pnl_loadSavedConnection.Controls.Clear();

        var str = new StringBuilder();
        dbImporter.GetSavedConnectionList();

        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='min-width: 100%;'><tbody>");
        str.Append("<tr class='myHeaderStyle'>");
        str.Append("<td width='45px'></td>");
        str.Append("<td align='left'>Connection Name</td>");
        str.Append("<td width='225px'>Database Provider</td>");
        str.Append("<td width='50px'></td></tr>");

        int count = 1;
        if (dbImporter.SavedConnections_Coll.Count > 0) {
            foreach (var sc in dbImporter.SavedConnections_Coll) {
                string ds = DBImporter.HideUsernameAndPasswordForConnectionString(sc.ConnectionString);
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='GridViewNumRow border-bottom' align='center'>" + count + "</td>");
                str.Append("<td class='border-right border-bottom'>" + sc.ConnectionName + "</td>");
                str.Append("<td class='border-right border-bottom' align='center'>" + sc.DatabaseProvider + "</td>");

                var str2 = new StringBuilder();
                str2.Append("<a href='#use' onclick='UseConnectionString(\"" + sc.ID + "\", \"" + sc.ConnectionName + "\");return false;' class='td-download-btn' title='Use String'></a>");

                str.Append("<td align='center' class='border-right border-bottom'>" + str2 + "</td>");
                str.Append("</tr>");
                count++;
            }
            str.Append("</tbody></table></div>");
        }
        else {
            str.Append("</tbody></table></div>");
            str.Append("<div class='emptyGridView'>No Saved Connection Strings.</div>");
        }

        pnl_loadSavedConnection.Controls.Add(new LiteralControl(str.ToString()));
        pnl_loadSavedConnection.Enabled = true;
        pnl_loadSavedConnection.Visible = true;
        pnl_enterConnectionstring.Enabled = false;
        pnl_enterConnectionstring.Visible = false;

        lbtn_uselocaldatasource.Enabled = false;
        lbtn_uselocaldatasource.Visible = false;
        lbtn_useSavedDatasource.Enabled = false;
        lbtn_useSavedDatasource.Visible = false;
        btn_test.Enabled = false;
        btn_test.Visible = false;

        lbtn_CancelUseSavedDatasource.Enabled = true;
        lbtn_CancelUseSavedDatasource.Visible = true;
    }

    protected void hf_deletestring_Changed(object sender, EventArgs e) {
        dbImporter.DeleteConnection(hf_deletestring.Value);
        hf_deletestring.Value = string.Empty;
        GetConnections();
    }

    protected void hf_editstring_Changed(object sender, EventArgs e) {
        if (hf_editstring.Value == "reset") {
            hf_editstring.Value = string.Empty;
        }
        GetConnections();
    }

    protected void hf_updatestring_Changed(object sender, EventArgs e) {
        SavedConnections sc = dbImporter.GetSavedConnectionListByID(hf_updatestring.Value);
        if (!string.IsNullOrEmpty(sc.ID)) {
            string _un = string.Empty;
            string _pwd = string.Empty;

            string _unPrefix = string.Empty;
            string _pwdPrefix = string.Empty;

            // Change Username
            string connectionstring = sc.ConnectionString;
            int dsIndex1 = connectionstring.ToLower().IndexOf("user id=", StringComparison.Ordinal);
            int dsIndex2 = connectionstring.ToLower().IndexOf("uid=", StringComparison.Ordinal);
            if (dsIndex1 != -1) {
                string tempDs = sc.ConnectionString.Substring((dsIndex1 + ("User Id=").Length));
                string un = string.Empty;
                foreach (char c in tempDs) {
                    if (c != ServerSettings.StringDelimiter[0])
                        un += c.ToString(CultureInfo.InvariantCulture);
                    else
                        break;
                }

                _un = un;
                _unPrefix = "User Id=";
            }
            else if (dsIndex2 != -1) {
                string tempDs = sc.ConnectionString.Substring((dsIndex2 + ("Uid=").Length));
                string un = string.Empty;
                foreach (char c in tempDs) {
                    if (c != ServerSettings.StringDelimiter[0])
                        un += c.ToString(CultureInfo.InvariantCulture);
                    else
                        break;
                }

                _un = un;
                _unPrefix = "Uid=";
            }

            // Change Password
            dsIndex1 = connectionstring.ToLower().IndexOf("password=", StringComparison.Ordinal);
            dsIndex2 = connectionstring.ToLower().IndexOf("pwd=", StringComparison.Ordinal);
            if (dsIndex1 != -1) {
                string tempDs = connectionstring.Substring((dsIndex1 + ("Password=").Length));
                string password = string.Empty;
                foreach (char c in tempDs) {
                    if (c != ServerSettings.StringDelimiter[0])
                        password += c.ToString(CultureInfo.InvariantCulture);
                    else
                        break;
                }

                _pwd = password;
                _pwdPrefix = "Password=";
            }
            else if (dsIndex2 != -1) {
                string tempDs = connectionstring.Substring((dsIndex2 + ("Pwd=").Length));
                string password = string.Empty;
                foreach (char c in tempDs) {
                    if (c != ServerSettings.StringDelimiter[0])
                        password += c.ToString(CultureInfo.InvariantCulture);
                    else
                        break;
                }

                _pwd = password;
                _pwdPrefix = "Pwd=";
            }

            string name = HttpUtility.UrlDecode(hf_connectionNameEdit.Value);
            string str = HttpUtility.UrlDecode(hf_connectionStringEdit.Value);
            string provider = HttpUtility.UrlDecode(hf_databaseProviderEdit.Value);

            if ((!string.IsNullOrEmpty(_un)) && (!string.IsNullOrEmpty(_pwd))) {
                str = str.Replace(_unPrefix + "*********", _unPrefix + _un);
                str = str.Replace(_pwdPrefix + "*********", _pwdPrefix + _pwd);
            }

            dbImporter.UpdateConnection(sc.ID, str, name, provider);
        }

        hf_updatestring.Value = string.Empty;
        hf_connectionNameEdit.Value = string.Empty;
        hf_connectionStringEdit.Value = string.Empty;
        hf_databaseProviderEdit.Value = string.Empty;
        hf_editstring.Value = string.Empty;
        GetConnections();
    }

    protected void hf_usestring_Changed(object sender, EventArgs e) {
        tb_connstring.Text = hf_usestring.Value;
        dd_provider.SelectedIndex = dd_provider.Items.Cast<ListItem>().TakeWhile(item => item.Value != GetCorrectProvider()).Count();

        if (TestSettings()) {
            if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked)) {
                pnl_ddselect.Enabled = true;
                pnl_ddselect.Visible = true;
                pnl_txtselect.Enabled = false;
                pnl_txtselect.Visible = false;

                ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
                if (GetCorrectConnectionString()[0] == connString.ConnectionString) {
                    var _connString = new ConnectionStringSettings("InternalDB", GetCorrectConnectionString()[0], GetCorrectConnectionString()[1]);
                    _dbviewer = new DBViewer(true, _connString);
                    BuildDropDown();
                    SetTableData();
                }
                else {
                    TryGetExternalDb();
                }
            }
        }

        pnl_loadSavedConnection.Controls.Clear();
        pnl_enterConnectionstring.Enabled = true;
        pnl_enterConnectionstring.Visible = true;
        pnl_loadSavedConnection.Enabled = false;
        pnl_loadSavedConnection.Visible = false;

        lbtn_uselocaldatasource.Enabled = true;
        lbtn_uselocaldatasource.Visible = true;
        lbtn_useSavedDatasource.Enabled = true;
        lbtn_useSavedDatasource.Visible = true;
        btn_test.Enabled = true;
        btn_test.Visible = true;

        lbtn_CancelUseSavedDatasource.Enabled = false;
        lbtn_CancelUseSavedDatasource.Visible = false;

        hf_usestring.Value = string.Empty;
    }

    protected void lbtn_CancelUseSavedDatasource_Click(object sender, EventArgs e) {
        pnl_loadSavedConnection.Controls.Clear();
        pnl_enterConnectionstring.Enabled = true;
        pnl_enterConnectionstring.Visible = true;
        pnl_loadSavedConnection.Enabled = false;
        pnl_loadSavedConnection.Visible = false;

        lbtn_uselocaldatasource.Enabled = true;
        lbtn_uselocaldatasource.Visible = true;
        lbtn_useSavedDatasource.Enabled = true;
        lbtn_useSavedDatasource.Visible = true;
        btn_test.Enabled = true;
        btn_test.Visible = true;

        lbtn_CancelUseSavedDatasource.Enabled = false;
        lbtn_CancelUseSavedDatasource.Visible = false;
    }

    #endregion


    #region Edit/Refresh/Build

    protected void hf_editimport_ValueChanged(object sender, EventArgs e) {
        if (hf_editimport.Value == "cancel") {
            hf_editimport.Value = string.Empty;
        }

        string id = hf_editimport.Value;
        if (!string.IsNullOrEmpty(id)) {
            DBImporter_Coll coll = dbImporter.GetImportTableByTableId(id);
            if (!string.IsNullOrEmpty(coll.ID)) {
                JavaScriptSerializer serializer = ServerSettings.CreateJavaScriptSerializer();
                string customizations = serializer.Serialize(coll.TableCustomizations);
                string summaryData = serializer.Serialize(coll.SummaryData);
                string overrides = serializer.Serialize(coll.ColumnOverrides);
                
                BuildUsersAllowedToEdit_ForEdit(id);

                tb_connstring.Text = DBImporter.HideUsernameAndPasswordForConnectionString(coll.ConnString);
                foreach (ListItem item in dd_provider.Items) {
                    if (item.Value == coll.Provider) {
                        item.Selected = true;
                    }
                    else {
                        item.Selected = false;
                    }
                }

                if (TestSettings()) {
                    ConnectionStringSettings connString = new ConnectionStringSettings("ExternalDB", GetCorrectConnectionString()[0], GetCorrectConnectionString()[1]);
                    _dbviewer = new DBViewer(true, connString);

                    if (GetCorrectConnectionString()[0] == connString.ConnectionString) {
                        BuildDropDown();
                    }
                    else {
                        BuildDropDown_External();
                    }

                    try {
                        var colnames = new List<string>();
                        DatabaseCall dbCall = new DatabaseCall(GetCorrectConnectionString()[1], GetCorrectConnectionString()[0]);

                        dbCall.NeedToLog = false;
                        DataTable dt = dbCall.CallGetDataTableBySelectStatement(coll.SelectCommand);
                        DataTable dtTemp = dt;
                        string tableName = dt.TableName;

                        if (dt.Columns.Count > 0) {
                            if (string.IsNullOrEmpty(tableName)) {
                                string splitChar = "from";
                                if (coll.SelectCommand.Contains("FROM")) {
                                    splitChar = "FROM";
                                }

                                string[] splitCommand = coll.SelectCommand.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
                                if (splitCommand.Length > 1) {
                                    tableName = splitCommand[1].Trim().Split(' ')[0];
                                }

                                if (tableName != null) {
                                    tableName = tableName.Trim();
                                }
                            }

                            if (!string.IsNullOrEmpty(tableName)) {
                                dtTemp = dbCall.CallGetDataTableBySelectStatement("SELECT * FROM " + tableName);
                            }
                        }

                        cb_ddselect.Items.Clear();

                        bool foundOneSelected = false;
                        foreach (ListItem item in dd_ddtables.Items) {
                            if (item.Value == tableName) {
                                item.Selected = true;
                                foundOneSelected = true;
                            }
                            else {
                                item.Selected = false;
                            }
                        }

                        if (foundOneSelected) {
                            for (int i = 0; i < dtTemp.Columns.Count; i++) {
                                bool selectedItem = false;
                                string overrideName = dtTemp.Columns[i].ColumnName;
                                if (coll.ColumnOverrides.ContainsKey(dtTemp.Columns[i].ColumnName)) {
                                    overrideName = coll.ColumnOverrides[dtTemp.Columns[i].ColumnName];
                                    selectedItem = true;
                                }

                                var item = new ListItem("&nbsp;" + dtTemp.Columns[i].ColumnName + BuildTextboxForSelect(overrideName, dtTemp.Columns[i].AllowDBNull), dtTemp.Columns[i].ColumnName);
                                if (!cb_ddselect.Items.Contains(item)) {
                                    cb_ddselect.Items.Add(item);
                                    item.Selected = selectedItem;
                                }
                                colnames.Add(dtTemp.Columns[i].ColumnName);
                            }

                            BuildColumns(colnames);
                        }
                        else {
                            if (dd_ddtables.Items.Count > 0) {
                                dd_ddtables.Items[0].Selected = true;
                            }
                            SetTableData();
                        }

                        string jsScript = string.Format("LoadEditImportWizard('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, {7}, '{8}', '{9}');",
                            HttpUtility.JavaScriptStringEncode(coll.TableName),
                            HttpUtility.JavaScriptStringEncode(coll.Description),
                            HttpUtility.JavaScriptStringEncode(coll.SelectCommand),
                            HttpUtility.JavaScriptStringEncode(coll.ConnString),
                            HttpUtility.JavaScriptStringEncode(coll.Provider),
                            HttpUtility.JavaScriptStringEncode(customizations),
                            coll.AllowEdit.ToString().ToLower(),
                            coll.NotifyUsers.ToString().ToLower(),
                            HttpUtility.JavaScriptStringEncode(coll.TableID),
                            HttpUtility.JavaScriptStringEncode(summaryData));

                        RegisterPostbackScripts.RegisterStartupScript(this, jsScript);
                    }
                    catch (Exception ex) {
                        AppLog.AddError(ex);
                    }
                }
            }
        }

        BuildTableList();

        hf_editimport.Value = string.Empty;
    }
    protected void hf_updateimport_ValueChanged(object sender, EventArgs e) {
        if ((!string.IsNullOrEmpty(tb_Databasename.Text)) && (!string.IsNullOrEmpty(tb_connstring.Text))) {
            string id = hf_updateimport.Value;
            string appName = tb_Databasename.Text.Trim();
            string description = tb_description.Text.Trim();
            bool editable = cb_AllowEditAdd.Checked;
            string columnOverrides = hf_columnOverrides.Value.Trim();
            string customizations = hf_customizations.Value.Trim();
            string summaryData = hf_summaryData.Value.Trim();
            bool notifyUsers = cb_allowNotifi.Checked;
            string usersAllowedToEdit = hf_usersAllowedToEdit.Value;

            string conditional = " ";
            if ((!string.IsNullOrEmpty(tb_conditional.Text)) &&
                (tb_conditional.Text.ToLower() != "[conditional statement]")) {
                conditional = " " + tb_conditional.Text + " ";
            }

            string tbDdselect = string.Empty;
            int i = 0;
            foreach (var li in cb_ddselect.Items.Cast<ListItem>().Where(li => li.Selected)) {
                tbDdselect += li.Value + ", ";
                i++;
            }

            if ((i == 0)) {
                tbDdselect = "*";
            }
            else {
                try {
                    string temp = tbDdselect;
                    temp = temp.Remove(temp.Length - 2);
                    tbDdselect = temp;
                }
                catch { }
            }

            string selectCommand = "SELECT " + tbDdselect + " FROM " + dd_ddtables.SelectedValue + conditional +
                             "ORDER BY " + dd_orderby.SelectedValue + " " + dd_orderdirection.SelectedValue;

            if ((selectCommand.ToLower().Contains("delete ")) ||
                (selectCommand.ToLower().Contains("update ")) ||
                (selectCommand.ToLower().Contains("insert ")) ||
                (selectCommand.ToLower().Contains("create ")) ||
                (selectCommand.ToLower().Contains("commit ")) ||
                (selectCommand.ToLower().Contains("rollback ")) ||
                (selectCommand.ToLower().Contains("grant revoke "))) {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error updating! Only SELECT statements allowed.');");
            }
            else if (selectCommand.ToLower().Contains("aspnet_")) {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Error updating! This table is not allowed.');");
            }
            else {
                dbImporter.UpdateEntry(id, appName, description, selectCommand, editable, notifyUsers);
                dbImporter.UpdateColumnOverrides(id, columnOverrides);
                dbImporter.UpdateTableCustomizations(id, customizations);
                dbImporter.UpdateUsersAllowedToEdit(id, usersAllowedToEdit);
                dbImporter.UpdateSummaryData(id, summaryData);

                dbImporter.GetImportList();
                _coll = dbImporter.DBColl;
            }

            hf_updateimport.Value = string.Empty;
            hf_editimport.Value = string.Empty;

            CancelWizard();
            RegisterPostbackScripts.RegisterStartupScript(this, "FinishEditCreate('" + id + "');");
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Could not update import. Please make sure that everything is correct and try again.');");
        }

        BuildTableList();
    }
    protected void hf_createAppImport_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_createAppImport.Value)) {
            DBImporter_Coll coll = dbImporter.GetImportTableByTableId(hf_createAppImport.Value);
            bool allowEdit = false;
            string fileName = "";
            string appName = "";
            string categoryid = AppCategoryID();

            fileName = coll.TableID;
            appName = coll.TableName;
            allowEdit = coll.AllowEdit;

            if (!string.IsNullOrEmpty(fileName)) {
                dbImportAppCreator dbiwc = new dbImportAppCreator(CurrentUsername, allowEdit);
                bool insertIntoAppList = true;
                App apps = new App(string.Empty);
                if (!string.IsNullOrEmpty(apps.GetAppInformation("app-" + fileName).ID)) {
                    insertIntoAppList = false;
                }
                dbiwc.CreateApp(appName, categoryid, coll.Description, "", false, coll.NotifyUsers, insertIntoAppList, fileName);

                if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                    AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
                    aib.BuildAppsForUser(true);
                }
            }
        }

        hf_createAppImport.Value = string.Empty;
        BuildTableList();
    }
    protected void hf_updateList_ValueChanged(object sender, EventArgs e) {
        BuildTableList();
        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser(true);
        }
        hf_updateList.Value = string.Empty;
    }

    private void BuildTableList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 4, "ImportedTables_Gridview");

        StringBuilder detailModals = new StringBuilder();

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns(string.Empty, "30px", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "250px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "400px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Table ID", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Notify Users", "100px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "150px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        App apps = new App(string.Empty);

        foreach (DBImporter_Coll row in _coll) {
            MemberDatabase tempMember = new MemberDatabase(row.ImportedBy);

            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckDBImportsGroupAssociation(row, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

            #region Detail Modal
            detailModals.Append(CreateDetailsModal(row));
            #endregion

            #region App Icon
            Apps_Coll appColl = apps.GetAppInformation(row.TableID);
            string appIcon = ResolveUrl("~/" + appColl.Icon);
            string querySeperator = "?";
            if (appIcon.Contains("?")) {
                querySeperator = "&";
            }

            appIcon += string.Format("{0}{1}{2}", querySeperator, ServerSettings.TimestampQuery, HelperMethods.GetTimestamp());
            appIcon = "<img alt='' src='" + appIcon + "' style='width: 22px; height: 22px;' />";

            bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, appIcon, TableBuilderColumnAlignment.Left));
            #endregion

            #region Table Name
            bodyColumns.Add(new TableBuilderBodyColumnValues("Name", row.TableName, TableBuilderColumnAlignment.Left));
            #endregion

            #region Description
            string description = "No description available";
            if (!string.IsNullOrEmpty(row.Description)) {
                description = row.Description;
            }
            bodyColumns.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
            #endregion

            #region Table ID
            bodyColumns.Add(new TableBuilderBodyColumnValues("Table ID", row.TableID, TableBuilderColumnAlignment.Left));
            #endregion

            #region Notify Users
            string _checkedNotifyUsers = string.Empty;
            if (row.NotifyUsers) {
                _checkedNotifyUsers = "checked='checked'";
            }
            bodyColumns.Add(new TableBuilderBodyColumnValues("Notify Users", "<input type='checkbox' disabled='disabled' " + _checkedNotifyUsers + " />", TableBuilderColumnAlignment.Left));
            #endregion

            #region Updated By
            bodyColumns.Add(new TableBuilderBodyColumnValues("Updated By", HelperMethods.MergeFMLNames(tempMember), TableBuilderColumnAlignment.Left));
            #endregion

            #region Date Updated
            bodyColumns.Add(new TableBuilderBodyColumnValues("Date Updated", row.Date.ToString(), TableBuilderColumnAlignment.Left));
            #endregion

            #region Action Buttons
            StringBuilder str = new StringBuilder();
            bool canAddEditButtons = true;
            if (!IsUserInAdminRole()) {
                if (CurrentUsername.ToLower() != row.ImportedBy.ToLower()) {
                    canAddEditButtons = false;
                }
            }

            if (canAddEditButtons && !MainServerSettings.LockCustomTables) {
                str.Append("<a class='td-edit-btn' title='Edit' onclick=\"EditEntry('" + row.TableID + "');return false;\"></a>");
                str.Append("<a class='td-details-btn' title='More Details' onclick=\"ShowHideTableDetails('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
                if (AppExists(row.TableID)) {
                    str.Append("<a class='td-delete-btn' title='Delete' onclick=\"DeleteEntry('" + row.TableID + "', '" + row.TableName + "');return false;\"></a>");
                }
                else {
                    str.Append("<a class='td-delete-btn' title='Delete' onclick=\"DeleteEntry('" + row.TableID + "', '" + row.TableName + "');return false;\"></a>");
                    str.Append("<a class='td-restore-btn' title='Recreate App' onclick=\"RecreateApp('" + row.TableID + "', '" + row.TableName + "', '" + row.Description + "');return false;\"></a>");
                }
            }
            else if (canAddEditButtons && MainServerSettings.LockCustomTables) {
                str.Append("<a class='td-details-btn' title='More Details' onclick=\"ShowHideTableDetails('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
            }
            #endregion

            tableBuilder.AddBodyRow(bodyColumns, str.ToString(), "data-id='" + row.TableID + "'");
        }
        #endregion

        pnl_ImportedTables.Controls.Clear();
        pnl_ImportedTables.Controls.Add(tableBuilder.CompleteTableLiteralControl("No imported tables found"));

        pnl_tableDetailList.Controls.Clear();
        pnl_tableDetailList.Controls.Add(new LiteralControl(detailModals.ToString()));

        UpdatePanel2.Update();
    }
    private string CreateDetailsModal(DBImporter_Coll row) {
        StringBuilder str = new StringBuilder();

        str.Append("<div id=\"" + row.ID + "Modal-element\" class=\"Modal-element\">");
        str.Append("<div class=\"Modal-overlay\">");
        str.Append("<div class=\"Modal-element-align\">");
        str.Append("<div class=\"Modal-element-modal\" data-setwidth=\"800\">");

        str.Append("<div class=\"ModalHeader\"><div><div class=\"app-head-button-holder-admin\"><a href=\"#close\" onclick=\"CloseTableDetailsModal('" + row.ID + "');return false;\" class=\"ModalExitButton\"></a></div><span class=\"Modal-title\"></span></div></div>");

        str.Append("<div class=\"ModalScrollContent\">");
        str.Append("<div class=\"ModalPadContent\">");

        str.Append("<h2><span style='font-weight: normal!important;'>" + row.TableName + "</span></h2><div class='clear-space'></div><div class='clear-space'></div>");

        str.Append("<div data-id='" + row.TableID + "' class='import-entry'>");
        str.Append("<div class='import-entry-table'>");

        #region Month Selector
        string monthSelector = GetCustomizations("MonthSelector", row);
        if (!string.IsNullOrEmpty(monthSelector)) {
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Month Selector", monthSelector);
        }
        #endregion

        #region Show Row Counts
        string showRowCounts = GetCustomizations("ShowRowCounts", row);
        if (!string.IsNullOrEmpty(showRowCounts)) {
            string _checkedShowRowCounts = "";
            if (HelperMethods.ConvertBitToBoolean(showRowCounts)) {
                _checkedShowRowCounts = "checked='checked'";
            }
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Show Row Counts", "<input type='checkbox' disabled='disabled' " + _checkedShowRowCounts + " />");
        }
        #endregion

        #region Show Description
        string showDescriptionOnApp = GetCustomizations("ShowDescriptionOnApp", row);
        if (!string.IsNullOrEmpty(showDescriptionOnApp)) {
            string _checkedShowDescriptionOnApp = "";
            if (HelperMethods.ConvertBitToBoolean(showDescriptionOnApp)) {
                _checkedShowDescriptionOnApp = "checked='checked'";
            }
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Show Description", "<input type='checkbox' disabled='disabled' " + _checkedShowDescriptionOnApp + " />");
        }
        #endregion

        #region Table View Style
        string tableViewStyle = GetCustomizations("TableViewStyle", row);
        if (!string.IsNullOrEmpty(tableViewStyle)) {
            switch (tableViewStyle) {
                case "excel":
                    tableViewStyle = "Excel Spreadsheet";
                    break;
                default:
                    tableViewStyle = "Default";
                    break;
            }

            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table View Style", tableViewStyle);
        }
        #endregion

        #region Title Color
        string appStyleTitleColor = GetCustomizations("AppStyleTitleColor", row);
        if (!string.IsNullOrEmpty(appStyleTitleColor)) {
            if (!appStyleTitleColor.StartsWith("#")) {
                appStyleTitleColor = "#" + appStyleTitleColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + appStyleTitleColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Title Color", titleColorEle + appStyleTitleColor);
        }
        #endregion

        #region Background Color
        string appStyleBackgroundColor = GetCustomizations("AppStyleBackgroundColor", row);
        if (!string.IsNullOrEmpty(appStyleBackgroundColor)) {
            if (!appStyleBackgroundColor.StartsWith("#")) {
                appStyleBackgroundColor = "#" + appStyleBackgroundColor;
            }

            string bgColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + appStyleBackgroundColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Header Color", bgColorEle + appStyleBackgroundColor);
        }
        #endregion

        #region Background Image
        string appStyleBackgroundImage = GetCustomizations("AppStyleBackgroundImage", row);
        if (!string.IsNullOrEmpty(appStyleBackgroundImage)) {
            string imgEle = "<img alt='' src='" + appStyleBackgroundImage + "' style='max-height: 100px; max-width: 100px; />";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "' style='clear: both;'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Header Image", imgEle);
        }
        #endregion

        #region Table Header Color
        string headerColor = GetCustomizations("HeaderColor", row);
        if (!string.IsNullOrEmpty(headerColor)) {
            if (!headerColor.StartsWith("#")) {
                headerColor = "#" + headerColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + headerColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Header Color", titleColorEle + headerColor);
        }
        #endregion

        #region Table Primary Row Color
        string primaryRowColor = GetCustomizations("PrimaryRowColor", row);
        if (!string.IsNullOrEmpty(primaryRowColor)) {
            if (!primaryRowColor.StartsWith("#")) {
                primaryRowColor = "#" + primaryRowColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + primaryRowColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Primary Row Color", titleColorEle + primaryRowColor);
        }
        #endregion

        #region Table Alternative Row Color
        string alternativeRowColor = GetCustomizations("AlternativeRowColor", row);
        if (!string.IsNullOrEmpty(alternativeRowColor)) {
            if (!alternativeRowColor.StartsWith("#")) {
                alternativeRowColor = "#" + alternativeRowColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + alternativeRowColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Alternative Row Color", titleColorEle + alternativeRowColor);
        }
        #endregion

        #region Font Family
        string fontFamily = GetCustomizations("FontFamily", row);
        if (!string.IsNullOrEmpty(fontFamily)) {
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Font Family", fontFamily);
        }
        #endregion

        #region Chart Information
        string chartType = dbImporter.GetChartTypeFromCustomizations(row.TableCustomizations).ToString();
        string chartImg = string.Empty;
        if (chartType != ChartType.None.ToString()) {
            chartImg = "<img src='" + ResolveUrl("~/Standard_Images/ChartTypes/" + chartType.ToLower() + ".png") + "' class='float-left pad-right' style='height: 16px;' />";
        }

        str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Type", chartImg + chartType);
        if (chartType != ChartType.None.ToString()) {
            string chartTitle = dbImporter.GetChartTitleFromCustomizations(row.TableCustomizations);
            if (string.IsNullOrEmpty(chartTitle)) {
                chartTitle = "<i>(No title available)</i>";
            }

            string[] chartColumns = dbImporter.GetChartColumnsFromCustomizations(row.TableCustomizations).Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string chartColumnStr = "<ul class='table-columns-forchart'>";
            foreach (string cc in chartColumns) {
                chartColumnStr += "<li>" + cc + "</li>";
            }
            chartColumnStr += "</ul><div class='clear'></div>";

            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Title", chartTitle);
            str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Columns", chartColumnStr);
        }
        #endregion

        #region Column Data
        TableBuilder tableBuilder1 = new TableBuilder(this.Page, true, false, 0, "ColumnDataDetails_Gridview");

        List<TableBuilderHeaderColumns> headerColumns1 = new List<TableBuilderHeaderColumns>();
        headerColumns1.Add(new TableBuilderHeaderColumns("Real Name", string.Empty, false));
        headerColumns1.Add(new TableBuilderHeaderColumns("Shown Name", string.Empty, false));
        tableBuilder1.AddHeaderRow(headerColumns1, false);

        foreach (KeyValuePair<string, string> data in row.ColumnOverrides) {
            List<TableBuilderBodyColumnValues> bodyColumns1 = new List<TableBuilderBodyColumnValues>();
            bodyColumns1.Add(new TableBuilderBodyColumnValues("Real Name", data.Key, TableBuilderColumnAlignment.Left));
            bodyColumns1.Add(new TableBuilderBodyColumnValues("Shown Name", data.Value, TableBuilderColumnAlignment.Left));
            tableBuilder1.AddBodyRow(bodyColumns1);
        }

        str.AppendFormat("<div class='clear'></div><div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Column Data", tableBuilder1.CompleteTableString("No table data found", false));
        #endregion

        #region Summary Data
        if (row.SummaryData.Count > 0) {
            TableBuilder tableBuilder2 = new TableBuilder(this.Page, true, false, 0, "SummaryDataDetails_Gridview");

            List<TableBuilderHeaderColumns> headerColumns2 = new List<TableBuilderHeaderColumns>();
            headerColumns2.Add(new TableBuilderHeaderColumns("Summary Name", string.Empty, false));
            headerColumns2.Add(new TableBuilderHeaderColumns("Column Name", string.Empty, false));
            headerColumns2.Add(new TableBuilderHeaderColumns("Formula", string.Empty, false));
            tableBuilder2.AddHeaderRow(headerColumns2, false);

            for (int i = 0; i < row.SummaryData.Count; i++) {
                List<TableBuilderBodyColumnValues> bodyColumns2 = new List<TableBuilderBodyColumnValues>();
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Summary Name", row.SummaryData[i].summaryName, TableBuilderColumnAlignment.Left));
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Column Name", row.SummaryData[i].columnName, TableBuilderColumnAlignment.Left));
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Formula", HttpUtility.UrlDecode(row.SummaryData[i].formulaType), TableBuilderColumnAlignment.Left));
                tableBuilder2.AddBodyRow(bodyColumns2);
            }

            str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Summary Data", tableBuilder2.CompleteTableString("No summary data found", false));
        }
        #endregion

        #region Default Values
        string defaultValues = GetCustomizations("DefaultValues", row);
        if (!string.IsNullOrEmpty(defaultValues)) {
            JavaScriptSerializer columnsSerializer = ServerSettings.CreateJavaScriptSerializer();
            Dictionary<string, string>[] defaultValuesArray = columnsSerializer.Deserialize<Dictionary<string, string>[]>(defaultValues);

            if (defaultValuesArray.Length > 0) {
                TableBuilder tableBuilder3 = new TableBuilder(this.Page, true, false, 0, "DefaultValuesDetails_Gridview");

                List<TableBuilderHeaderColumns> headerColumns3 = new List<TableBuilderHeaderColumns>();
                headerColumns3.Add(new TableBuilderHeaderColumns("Column Name", string.Empty, false));
                headerColumns3.Add(new TableBuilderHeaderColumns("Default Value", string.Empty, false));
                tableBuilder3.AddHeaderRow(headerColumns3, false);

                for (int i = 0; i < defaultValuesArray.Length; i++) {
                    List<TableBuilderBodyColumnValues> bodyColumns3 = new List<TableBuilderBodyColumnValues>();
                    bodyColumns3.Add(new TableBuilderBodyColumnValues("Column Name", defaultValuesArray[i]["name"], TableBuilderColumnAlignment.Left));
                    bodyColumns3.Add(new TableBuilderBodyColumnValues("Default Value", defaultValuesArray[i]["value"], TableBuilderColumnAlignment.Left));
                    tableBuilder3.AddBodyRow(bodyColumns3);
                }

                str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Default Values", tableBuilder3.CompleteTableString("No default values found", false));
            }
        }
        #endregion

        str.Append("</div></div></div></div>");
        str.Append("<div class=\"ModalButtonHolder\"><input type=\"button\" class=\"input-buttons modal-cancel-btn\" value=\"Close\" onclick=\"CloseTableDetailsModal('" + row.ID + "');\" /><div class=\"clear\"></div></div>");
        str.Append("</div></div></div></div>");

        return str.ToString();
    }

    private static string GetCustomizations(string name, DBImporter_Coll row) {
        foreach (CustomTableCustomizations ctc in row.TableCustomizations) {
            if (ctc.customizeName == name) {
                return ctc.customizeValue;
            }
        }

        return string.Empty;
    }
    private bool AppExists(string appId) {
        FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + appId + ".ascx");
        if (fi.Exists) {
            return true;
        }

        return false;
    }

    #endregion


    #region App Delete Password Check
    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_deleteimport.Value)) {
            RegisterPostbackScripts.RegisterStartupScript(this, "CancelRequest();");
        }
        else {
            string passwordUser = ServerSettings.AdminUserName;

            if (!IsUserNameEqualToAdmin()) {
                var apps = new App(string.Empty);
                string appid = apps.GetAppIDbyFilename("Database_Imports/" + hf_deleteimport.Value + ".ascx");
                string createdBy = apps.GetAppCreatedBy(appid);
                if (!string.IsNullOrEmpty(createdBy)) {
                    MembershipUser u = Membership.GetUser(createdBy);
                    if (u != null) {
                        passwordUser = u.UserName;
                    }
                }
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
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You are not authorized to delete this table.');");
                }
                else {
                    tb_passwordConfirm.Text = "";
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
                }
            }
        }
    }

    protected void hf_StartDelete_Changed(object sender, EventArgs e) {
        var apps = new App(string.Empty);
        try {
            dbImporter.DeleteEntry(hf_deleteimport.Value);
            string appid = apps.GetAppIDbyFilename("Database_Imports/" + hf_deleteimport.Value + ".ascx");
            apps.DeleteAppComplete(appid, ServerSettings.GetServerMapLocation);
            apps.DeleteAppLocal(appid);
        }
        catch {
        }

        dbImporter.GetImportList();
        _coll = dbImporter.DBColl;
        BuildTableList();

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser(true);
        }

        hf_deleteimport.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'password-element', '');loadingPopup.RemoveMessage();");
    }
    #endregion


}