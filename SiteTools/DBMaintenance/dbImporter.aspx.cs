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

public partial class SiteTools_dbImporter : Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private List<DBImporter_Coll> _coll = new List<DBImporter_Coll>();
    private DBViewer _dbviewer = new DBViewer(true);
    private string _username;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;
    private DBImporter dbImporter = new DBImporter();

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            _username = userId.Name;
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                AssociateWithGroups = _ss.AssociateWithGroups;
                _member = new MemberDatabase(_username);
                HelperMethods.SetIsSocialUserForDeleteItems(Page, _username);

                PageLoadInit.BuildLinks(pnlLinkBtns, _username, this.Page);

                if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
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

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    cb_InstallAfterLoad.Enabled = false;
                    cb_InstallAfterLoad.Visible = false;
                    cb_isPrivate.Enabled = false;
                    cb_isPrivate.Visible = false;
                    cb_InstallAfterLoad.Checked = false;
                    cb_isPrivate.Checked = false;
                }

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
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
                }

                BuildTableList();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
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
        if (dd_provider_connectionstring.Items.Count == 0) {
            foreach (string providerVal in DatabaseProviders.ProviderList) {
                dd_provider_connectionstring.Items.Add(new ListItem(providerVal, providerVal));
            }
        }

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
            if (user.ToLower() == ServerSettings.AdminUserName.ToLower()) {
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

            string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
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
                    if (user.ToLower() == ServerSettings.AdminUserName.ToLower()) {
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

                    string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
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

        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
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
                if (!dr["TABLE_NAME"].ToString().ToLower().StartsWith("ct_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("membership")) {
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
    private static string BuildTextboxForSelect(string name, bool allowNull) {
        return "<input type='text' class='column-namechange textEntry float-right margin-left' maxlength='100' value='" + name + "' style='width: 210px; margin-top: -5px;' /><span class='column-allownull' style='display: none;'>" + allowNull.ToString().ToLower() + "</span>";
    }
    private void SetTableData() {
        try {
            var colnames = new List<string>();
            string tablename = dd_ddtables.SelectedValue;
            _dbviewer.GetTableData(tablename, GetCorrectConnectionString()[1], GetCorrectConnectionString()[0]);

            cb_ddselect.Items.Clear();

            for (int i = 0; i < _dbviewer.dt.Columns.Count; i++) {
                var item = new ListItem("&nbsp;" + _dbviewer.dt.Columns[i].ColumnName + BuildTextboxForSelect(_dbviewer.dt.Columns[i].ColumnName, _dbviewer.dt.Columns[i].AllowDBNull), _dbviewer.dt.Columns[i].ColumnName);
                if (!cb_ddselect.Items.Contains(item)) {
                    cb_ddselect.Items.Add(item);
                    item.Selected = true;
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
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading! This table is not allowed.');$('#error_onupdatecreate').css('color', 'red');");
                    }
                    else if (tb_selectcomm.Text.ToLower().Contains("aspnet_")) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading! This table is not allowed.');$('#error_onupdatecreate').css('color', 'red');");
                    }
                    else if ((tb_selectcomm.Text.ToLower().Substring(0, 6) == "select") && (!tb_selectcomm.Text.ToLower().Contains("aspnet_"))) {
                        string randomId = HelperMethods.RandomString(10);

                        dbImporter.AddImport(randomId, tb_Databasename.Text.Trim(), tb_description.Text.Trim(), GetCorrectConnectionString()[0], tb_selectcomm.Text.Trim(), GetCorrectConnectionString()[1],
                            HttpContext.Current.User.Identity.Name, cb_AllowEditAdd.Checked, hf_usersAllowedToEdit.Value, cb_allowNotifi.Checked,
                            hf_columnOverrides.Value.Trim(), hf_customizations.Value.Trim());

                        dbImporter.GetImportList();
                        _coll = dbImporter.DBColl;

                        SaveConnections();
                        CreateApp(cb_AllowEditAdd.Checked, randomId);
                        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                            AppIconBuilder aib = new AppIconBuilder(Page, _member);
                            aib.BuildAppsForUser();
                        }

                        CancelWizard();
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'ImportWizard-element', '');");
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading! Only SELECT statements allowed.');$('#error_onupdatecreate').css('color', 'red');");
                    }
                }
                catch {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading!');$('#error_onupdatecreate').css('color', 'red');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('All fields must be filled out.');$('#error_onupdatecreate').css('color', 'red');");
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
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading! Only SELECT statements allowed.');$('#error_onupdatecreate').css('color', 'red');");
                }
                else if (tb_conditional.Text.ToLower().Contains("aspnet_") || tb_conditional.Text.ToLower().Contains("membership")) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading! This table is not allowed.');$('#error_onupdatecreate').css('color', 'red');");
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
                        hf_columnOverrides.Value.Trim(), hf_customizations.Value.Trim());

                    dbImporter.GetImportList();
                    _coll = dbImporter.DBColl;

                    CreateApp(cb_AllowEditAdd.Checked, randomId);
                    if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                        AppIconBuilder aib = new AppIconBuilder(Page, _member);
                        aib.BuildAppsForUser();
                    }

                    CancelWizard();
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'ImportWizard-element', '');");
                }
            }
            catch {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('Error Uploading!');$('#error_onupdatecreate').css('color', 'red');");
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#error_onupdatecreate').html('All fields must be filled out.');$('#error_onupdatecreate').css('color', 'red');");
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
        const string picname = "database.png";
        var apps = new App(string.Empty);
        string categoryid = AppCategoryID();

        if (string.IsNullOrEmpty(wName))
            wName = tb_Databasename.Text.Trim();

        var member = new MemberDatabase(_username);
        dbImportAppCreator dbiwc = new dbImportAppCreator(_username, allowEdit);
        if (pnl_txtselect.Enabled && pnl_txtselect.Visible && (pnl_ddselect.Enabled == false) && (pnl_ddselect.Visible == false)) {

            bool _isPrivate = cb_isPrivate.Checked;
            if (!cb_InstallAfterLoad.Checked) {
                _isPrivate = false;
            }

            dbiwc.CreateApp(wName, categoryid, "Imported Database. Table Name: " + wName + ". Select Statment: " + tb_selectcomm.Text + ".", picname, _isPrivate, true, fileName);

            if (cb_InstallAfterLoad.Checked)
                member.UpdateEnabledApps("app-" + fileName);
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

            dbiwc.CreateApp(wName, categoryid, "Imported Database. Table Name: " + wName + ". Select Statment: " + command + ".", picname, _isPrivate, true, fileName);
            if (cb_InstallAfterLoad.Checked)
                member.UpdateEnabledApps("app-" + fileName);
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
    protected void btn_addconnectionstring_Click(object sender, EventArgs e) {
        if ((!string.IsNullOrEmpty(tb_connectionname.Text.Trim())) &&
            (tb_connectionname.Text.Trim().ToLower() != "connection name")
            && (!string.IsNullOrEmpty(tb_connectionstring.Text.Trim())) &&
            (tb_connectionstring.Text.Trim().ToLower() != "connection string")) {

            dbImporter.AddConnection(tb_connectionstring.Text.Trim(), tb_connectionname.Text.Trim(), dd_provider_connectionstring.SelectedValue, _username);

            tb_connectionname.Text = "Name";
            tb_connectionstring.Text = "Connection String";

            var str = new StringBuilder();
            str.Append("CloseAddConnectionModal();");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
            GetConnections();
        }
        else {
            var str = new StringBuilder();
            str.Append("$('#savedconnections_postmessage').html('<span style=\"color: Red;\">Cannot add new connection string. Fields cannot be empty.</span>');");
            str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }
    }

    private void GetConnections() {
        pnl_savedconnections_holder.Controls.Clear();
        var str = new StringBuilder();
        dbImporter.GetSavedConnectionList();

        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='width: 100%;'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='175px' align='left'>Connection Name</td>");
        str.Append("<td align='left'>Connection String</td>");
        str.Append("<td width='225px'>Database Provider</td>");
        str.Append("<td width='80px'>Actions</td></tr>");

        int count = 1;
        if (dbImporter.SavedConnections_Coll.Count > 0) {
            foreach (var sc in dbImporter.SavedConnections_Coll) {
                string ds = DBImporter.HideUsernameAndPasswordForConnectionString(sc.ConnectionString);

                if (hf_editstring.Value == sc.ID) {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td class='GridViewNumRow border-bottom' align='center'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                    str.Append("<td class='border-right border-bottom'><input id='tb_connNameedit' type='text' value='" + sc.ConnectionName + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 98%;' /></td>");
                    str.Append("<td class='border-right border-bottom'><input id='tb_connStringedit' type='text' value='" + ds + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 98%;' /></td>");

                    string options = "<select id='edit-databaseProvider' style='min-width: 150px;'>";
                    foreach (string providerVal in DatabaseProviders.ProviderList) {
                        string selected = "";
                        if (providerVal == sc.DatabaseProvider) {
                            selected = " selected='selected'";
                        }
                        options += "<option" + selected + " value='" + providerVal + "'>" + providerVal + "</option>";
                    }
                    options += "</select>";

                    str.Append("<td class='border-right border-bottom'>" + options + "</td>");

                    var str2 = new StringBuilder();
                    str2.Append("<a href='#update' onclick='UpdateConnectionString(\"" + sc.ID + "\");return false;' class='td-update-btn' title='Update'></a>");
                    str2.Append("<a href='#cancel' onclick='EditConnectionString(\"reset\");return false;' class='td-cancel-btn margin-left' title='Cancel'></a>");

                    str.Append("<td align='center' class='border-right border-bottom'>" + str2 + "</td>");
                    str.Append("</tr>");
                }
                else {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td class='GridViewNumRow border-bottom' align='center'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                    str.Append("<td class='border-right border-bottom'>" + sc.ConnectionName + "</td>");
                    str.Append("<td class='border-right border-bottom'>" + ds + "</td>");
                    str.Append("<td class='border-right border-bottom' align='center'>" + sc.DatabaseProvider + "</td>");

                    var str2 = new StringBuilder();
                    // str2.Append("<a href='#use' onclick='UseConnectionString(\"" + sc.ID + "\", \"" + sc.ConnectionName + "\");return false;' class='td-download-btn' title='Use String'></a>");
                    if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || (_username.ToLower() == sc.Username.ToLower())) {
                        str2.Append("<a href='#edit' onclick='EditConnectionString(\"" + sc.ID + "\");return false;' class='td-edit-btn' title='Edit'></a>");
                        str2.Append("<a href='#delete' onclick='DeleteConnectionString(\"" + sc.ID + "\");return false;' class='td-delete-btn margin-left' title='Delete'></a>");
                    }

                    str.Append("<td align='center' class='border-right border-bottom'>" + str2 + "</td>");
                    str.Append("</tr>");
                }
                count++;
            }
            str.Append("</tbody></table></div>");
        }
        else {
            str.Append("</tbody></table></div>");
            str.Append("<div class='emptyGridView'>No Saved Connection Strings.</div>");
        }

        pnl_savedconnections_holder.Controls.Add(new LiteralControl(str.ToString()));

        if (!string.IsNullOrEmpty(hf_editstring.Value)) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#tb_connNameedit').focus();");
        }
    }

    private void SaveConnections() {
        if ((!string.IsNullOrEmpty(tb_connstring.Text.Trim())) &&
            (!string.IsNullOrEmpty(tb_Databasename.Text.Trim()))) {
            bool found = false;
            dbImporter.GetSavedConnectionList();
            foreach (SavedConnections sc in dbImporter.SavedConnections_Coll) {
                if (DBImporter.HideUsernameAndPasswordForConnectionString(sc.ConnectionString) == tb_connstring.Text.Trim()) {
                    found = true;
                    break;
                }
            }

            if (!found && !tb_connstring.Text.Contains("Use connection string")) {
                dbImporter.AddConnection(GetCorrectConnectionString()[0], "DB Import " + (dbImporter.SavedConnections_Coll.Count + 1).ToString(), GetCorrectConnectionString()[1], _username);
                tb_connectionname.Text = "Name";
                tb_connectionstring.Text = "Connection String";

                var str = new StringBuilder();
                str.Append("$('#savedconnections_postmessage').html(\"<span style='color: Green;'>Connection string added successfully</span>\");");
                str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
                RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
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
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string customizations = serializer.Serialize(coll.TableCustomizations);
                string overrides = serializer.Serialize(coll.ColumnOverrides);
                
                BuildUsersAllowedToEdit_ForEdit(id);

                tb_connstring.Text = coll.ConnString;
                foreach (ListItem item in dd_provider.Items) {
                    if (item.Value == coll.Provider) {
                        item.Selected = true;
                    }
                    else {
                        item.Selected = false;
                    }
                }

                if (TestSettings()) {
                    var connString = new ConnectionStringSettings("ExternalDB", GetCorrectConnectionString()[0], GetCorrectConnectionString()[1]);
                    _dbviewer = new DBViewer(true, connString);
                    BuildDropDown_External();

                    try {
                        var colnames = new List<string>();
                        DatabaseCall dbCall = new DatabaseCall(GetCorrectConnectionString()[1], GetCorrectConnectionString()[0]);
                        DataTable dt = dbCall.CallGetDataTableBySelectStatement(coll.SelectCommand);
                        DataTable dtTemp = dt;
                        if (!string.IsNullOrEmpty(dt.TableName)) {
                            dtTemp = dbCall.CallGetDataTableBySelectStatement("SELECT * FROM " + dt.TableName);
                        }

                        cb_ddselect.Items.Clear();

                        foreach (ListItem item in dd_ddtables.Items) {
                            if (item.Value == dt.TableName) {
                                item.Selected = true;
                            }
                            else {
                                item.Selected = false;
                            }
                        }

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

                        string jsScript = string.Format("LoadEditImportWizard('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, {7}, '{8}');",
                            HttpUtility.JavaScriptStringEncode(coll.TableName),
                            HttpUtility.JavaScriptStringEncode(coll.Description),
                            HttpUtility.JavaScriptStringEncode(coll.SelectCommand),
                            HttpUtility.JavaScriptStringEncode(coll.ConnString),
                            HttpUtility.JavaScriptStringEncode(coll.Provider),
                            HttpUtility.JavaScriptStringEncode(customizations),
                            coll.AllowEdit.ToString().ToLower(),
                            coll.NotifyUsers.ToString().ToLower(),
                            HttpUtility.JavaScriptStringEncode(coll.TableID));

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

                dbImporter.GetImportList();
                _coll = dbImporter.DBColl;
            }

            hf_updateimport.Value = string.Empty;
            hf_editimport.Value = string.Empty;

            CancelWizard();
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'ImportWizard-element', '');");
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
                dbImportAppCreator dbiwc = new dbImportAppCreator(_username, allowEdit);
                bool insertIntoAppList = true;
                App apps = new App(string.Empty);
                if (!string.IsNullOrEmpty(apps.GetAppInformation("app-" + fileName).ID)) {
                    insertIntoAppList = false;
                }
                dbiwc.CreateApp(appName, categoryid, "", "", false, insertIntoAppList, fileName);

                if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
                    AppIconBuilder aib = new AppIconBuilder(Page, _member);
                    aib.BuildAppsForUser();
                }
            }
        }

        hf_createAppImport.Value = string.Empty;
        BuildTableList();
    }

    private void BuildTableList() {
        pnl_ImportedTables.Controls.Clear();
        App apps = new App(string.Empty);

        StringBuilder str = new StringBuilder();
        int count = 1;
        foreach (DBImporter_Coll row in _coll) {
            MemberDatabase tempMember = new MemberDatabase(row.ImportedBy);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckDBImportsGroupAssociation(row, _member)) {
                    continue;
                }
            }

            string search = tb_search.Text.Trim().ToLower();
            if (search != "search imports" && !string.IsNullOrEmpty(search)) {
                if (!search.Contains(row.Description.ToLower()) && !search.Contains(row.ImportedBy.ToLower()) && !search.Contains(row.TableName.ToLower())) {
                    continue;
                }
            }

            str.Append("<div data-id='" + row.TableID + "' class='import-entry'>");
            str.Append("<table class='import-entry-table'>");

            #region Non-editable selection

            string description = "No description available";
            if (!string.IsNullOrEmpty(row.Description)) {
                description = row.Description;
            }

            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Name/Description", row.TableName + " - " + description);
            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Updated By", HelperMethods.MergeFMLNames(tempMember) + " on " + row.Date);
            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "", "<a href='#' class='showhidedetails' data-rowid='" + row.ID + "' onclick=\"ShowHideTableDetails('" + row.ID + "');return false;\">Show Details</a>");

            string _checkedNotifyUsers = string.Empty;
            if (row.NotifyUsers) {
                _checkedNotifyUsers = "checked='checked'";
            }

            string _isEditable = string.Empty;
            if (row.AllowEdit) {
                _isEditable = "checked='checked'";
            }

            string chartType = dbImporter.GetChartTypeFromCustomizations(row.TableCustomizations).ToString();
            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Is Editable", "<input type='checkbox' disabled='disabled' " + _isEditable + " />");
            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Notify Users", "<input type='checkbox' disabled='disabled' " + _checkedNotifyUsers + " />");

            string chartImg = string.Empty;
            if (chartType != ChartType.None.ToString()) {
                chartImg = "<img src='" + ResolveUrl("~/Standard_Images/ChartTypes/" + chartType.ToLower() + ".png") + "' class='float-left pad-right' style='height: 16px;' />";
            }

            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Chart Type", chartImg + chartType);
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
                chartColumnStr += "</ul>";

                str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}<div class='clear'></div></td></tr>", "Chart Title", chartTitle);
                str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}<div class='clear'></div></td></tr>", "Chart Columns", chartColumnStr);
            }

            #region Customizations
            string fontFamily = GetCustomizations("FontFamily", row);
            if (!string.IsNullOrEmpty(fontFamily)) {
                fontFamily = "font-family: " + fontFamily.Replace("'", "\"") + "!important;";
            }
            #endregion

            string overrideColumns = "<table cellspacing='0' cellpadding='0' style='width: 850px; border-collapse: collapse; " + fontFamily + "'><tbody>";
            overrideColumns += AddHeaderTableViewer(row);

            int i = 0;
            foreach (KeyValuePair<string, string> keyPair in row.ColumnOverrides) {
                overrideColumns += AddItemTableViewer(i + 1, keyPair, row);
                i++;
            }
            overrideColumns += "</tbody></table>";

            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Column Data", overrideColumns);
            #endregion

            str.Append("</table>");

            bool canAddEditButtons = true;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (_username.ToLower() != row.ImportedBy.ToLower()) {
                    canAddEditButtons = false;
                }
            }

            if (canAddEditButtons) {
                str.Append("<div class='import-entry-editbtns'>");
                str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Edit' onclick=\"EditEntry('" + row.TableID + "');\" />");
                if (AppExists(row.TableID)) {
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Delete' onclick=\"DeleteEntry('" + row.TableID + "', '" + row.TableName + "');\" />");
                }
                else {
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Delete' onclick=\"DeleteEntry('" + row.TableID + "', '" + row.TableName + "');\" />");
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Create App' onclick=\"RecreateApp('" + row.TableID + "', '" + row.TableName + "', '" + row.Description + "');\" />");
                }
                str.Append("</div>");
            }

            str.Append("</div>");
            count++;
        }

        if (!string.IsNullOrEmpty(str.ToString()) && count > 1) {
            pnl_ImportedTables.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_ImportedTables.Controls.Add(new LiteralControl("<div class='emptyGridView'>No imported tables found.</div>"));
        }
    }

    private static string GetCustomizations(string name, DBImporter_Coll row) {
        foreach (CustomTableCustomizations ctc in row.TableCustomizations) {
            if (ctc.customizeName == name) {
                return ctc.customizeValue;
            }
        }

        return string.Empty;
    }
    private static string AddHeaderTableViewer(DBImporter_Coll row) {
        string column = "<tr class='myHeaderStyle'>";

        string headerColor = GetCustomizations("HeaderColor", row);
        if (!string.IsNullOrEmpty(headerColor)) {
            string color = "customization-light-color";
            if (HelperMethods.UseDarkTextColorWithBackground(headerColor)) {
                color = "customization-dark-color";
            }
            headerColor = "background: " + headerColor + "!important;";
            column = "<tr class='myHeaderStyle " + color + "' style='" + headerColor + "'>";
        }

        column += "<td width='45px'></td>";
        column += "<td width='50%'>Real Name</td>";
        column += "<td>Shown Name</td>";
        column += "</tr>";

        return column;
    }
    private static string AddItemTableViewer(int index, KeyValuePair<string, string> data, DBImporter_Coll row) {
        string color = string.Empty;
        string alternativeRowColor = GetCustomizations("AlternativeRowColor", row);
        if (!string.IsNullOrEmpty(alternativeRowColor)) {
            color = "customization-light-color";
            if (HelperMethods.UseDarkTextColorWithBackground(alternativeRowColor)) {
                color = "customization-dark-color";
            }
            alternativeRowColor = "background: " + alternativeRowColor + "!important;";
        }

        string column = "<tr class='GridNormalRow myItemStyle " + color + "' style='" + alternativeRowColor + "'>";
        if ((index - 1) % 2 == 0) {
            string primaryRowColor = GetCustomizations("PrimaryRowColor", row);
            if (!string.IsNullOrEmpty(primaryRowColor)) {
                color = "customization-light-color";
                if (HelperMethods.UseDarkTextColorWithBackground(primaryRowColor)) {
                    color = "customization-dark-color";
                }
                primaryRowColor = "background: " + primaryRowColor + "!important;";
            }

            column = "<tr class='GridNormalRow myItemStyle " + color + "' style='" + primaryRowColor + "'>";
        }

        column += "<td align='center' class='GridViewNumRow cursor-default'>" + index.ToString() + "</td>";
        column += "<td class='border-right'><span class='float-left pad-top-sml pad-bottom-sml'>" + data.Key + "</span></td>";
        column += "<td class='border-right'>" + data.Value + "</td>";
        column += "</tr>";

        return column;
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

            if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
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

            MemberDatabase tempMember = new MemberDatabase(_username);
            if (tempMember.IsSocialAccount && passwordUser.ToLower() == _username.ToLower()) {
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
                if (tempMember.IsSocialAccount) {
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
            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
        }

        hf_deleteimport.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'password-element', '');openWSE.RemoveUpdateModal();");
    }
    #endregion


    #region Search Table List

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildTableList();
    }
    protected void imgbtn_clearsearch_Click(object sender, EventArgs e) {
        tb_search.Text = "Search Imports";
        BuildTableList();
    }
    private bool SearchFilterValid(string name) {
        string searchText = tb_search.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(searchText) || searchText == "search imports"
            || searchText.Contains(name.ToLower()) || name.ToLower().Contains(searchText)) {
            return true;
        }

        return false;
    }

    #endregion

}