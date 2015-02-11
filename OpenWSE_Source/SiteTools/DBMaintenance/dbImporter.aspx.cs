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

#endregion

public partial class SiteTools_dbImporter : Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private List<DBImporter_Coll> _coll = new List<DBImporter_Coll>();
    private DBViewer _dbviewer = new DBViewer(true);
    private string _username;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            _username = userId.Name;
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                AssociateWithGroups = _ss.AssociateWithGroups;
                _member = new MemberDatabase(_username);
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

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    lbtn_uselocaldatasource.Visible = false;
                    lbtn_uselocaldatasource.Enabled = false;
                }

                BuildChartTypeList();

                var db = new DBImporter();
                db.BinaryDeserialize();
                _coll = db.DBColl;
                GetConnections();

                if (!IsPostBack) {
                    BuildUsersAllowedToEdit();
                    rb_adv_disabled.Checked = true;
                    rb_adv_enabled.Checked = false;
                    pnl_lbl.Visible = false;
                    pnl_lbl.Enabled = false;
                    pnl_txtselect.Visible = false;
                    pnl_txtselect.Enabled = false;
                    LoadAllDatabases(ref GV_Imports, "0", "desc");
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildUsersAllowedToEdit() {
        pnl_usersAllowedToEdit.Controls.Clear();

        StringBuilder str = new StringBuilder();
        List<string> groupList = _member.GroupList;
        string checkboxInput = "<div class='checkbox-new-click float-left pad-right-big pad-bottom-big' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";
        Groups groups = new Groups(HttpContext.Current.User.Identity.Name);

        foreach (string group in groupList) {
            List<string> users = groups.GetMembers_of_Group(group);
            string groupImg = groups.GetGroupImg_byID(group);

            if (groupImg.StartsWith("~/")) {
                groupImg = ResolveUrl(groupImg);
            }

            string groupImgHtmlCtrl = "<img alt='' src='" + groupImg + "' class='float-left margin-right' style='max-height: 24px;' />";
            str.Append("<h3 class='pad-bottom'>" + groupImgHtmlCtrl + groups.GetGroupName_byID(group) + "</h3><div class='clear-space'></div><div class='clear-space'></div>");
            foreach (string user in users) {
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

                string marginTop = "3px";
                string userNameTitle = "<h4>" + un + "</h4>";
                string acctImage = tempMember.AccountImage;
                if (!string.IsNullOrEmpty(acctImage)) {
                    userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                    marginTop = "8px";
                }

                string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
                str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
            }
            str.Append("<div class='clear-space'></div><div class='clear-space'></div><div class='clear-space'></div>");
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        pnl_usersAllowedToEdit.Controls.Add(new LiteralControl(str.ToString()));
    }

    private void BuildChartTypeList() {
        ddl_ChartType.Items.Clear();
        Array chartTypes = Enum.GetValues(typeof(ChartType));
        foreach (ChartType type in chartTypes) {
            if (type == ChartType.None) {
                continue;
            }

            ddl_ChartType.Items.Add(new ListItem(type.ToString(), type.ToString()));
        }
    }

    private void CreateApp(bool allowEdit, string fileName, string wName = "") {
        const string picname = "database.png";
        var apps = new App();
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

    private string AppCategoryID() {
        string categoryid = string.Empty;
        var widcat = new AppCategory(true);
        foreach (Dictionary<string, string> drTemp in widcat.category_dt) {
            if (drTemp["Category"].ToLower() == "database imports") {
                categoryid = drTemp["ID"];
                break;
            }
        }

        if (string.IsNullOrEmpty(categoryid)) {
            widcat.addItem("Database Imports");
            widcat = new AppCategory(true);
        }

        foreach (Dictionary<string, string> drTemp in widcat.category_dt) {
            if (drTemp["Category"].ToLower() == "database imports") {
                categoryid = drTemp["ID"];
                break;
            }
        }

        return categoryid;
    }

    protected void rb_adv_enabled_Checked(object sender, EventArgs e) {
        rb_adv_disabled.Checked = false;
        rb_adv_enabled.Checked = true;

        pnl_lbl.Visible = true;
        pnl_lbl.Enabled = true;
        pnl_lbl2.Visible = false;
        pnl_lbl2.Enabled = false;
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

        pnl_lbl.Visible = false;
        pnl_lbl.Enabled = false;
        pnl_lbl2.Visible = true;
        pnl_lbl2.Enabled = true;
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
        }
    }

    protected void lbtn_uselocaldatasource_Click(object sender, EventArgs e) {
        ConnectionStringSettings connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
        tb_connstring.Text = connString.ConnectionString;

        for (int i = 0; i < dd_provider.Items.Count; i++) {
            if (dd_provider.Items[i].Value == connString.ProviderName) {
                dd_provider.SelectedIndex = i;
                break;
            }
        }

        if (TestSettings()) {
            pnl_ddselect.Enabled = true;
            pnl_ddselect.Visible = true;
            pnl_lbl.Enabled = true;
            pnl_lbl.Visible = true;
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
            pnl_lbl.Enabled = true;
            pnl_lbl.Visible = true;
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

    private void BuildDropDown() {
        dd_ddtables.Items.Clear();
        try {
            var dv = new DataView(_dbviewer.dt);
            dv.Sort = string.Format("{0} {1}", dv.Table.Columns[2], "asc");
            DataTable dt = dv.ToTable();
            foreach (DataRow dr in dt.Rows) {
                if (!dr["TABLE_NAME"].ToString().ToLower().Contains("aspnet_") && !dr["TABLE_NAME"].ToString().ToLower().Contains("membership")) {
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

    private void SetTableData() {
        try {
            var colnames = new List<string>();
            string tablename = dd_ddtables.SelectedValue;
            _dbviewer.GetTableData(tablename, GetCorrectConnectionString()[1], GetCorrectConnectionString()[0]);

            cb_ddselect.Items.Clear();

            for (int i = 0; i < _dbviewer.dt.Columns.Count; i++) {
                var item = new ListItem("&nbsp;" + _dbviewer.dt.Columns[i].ColumnName,
                                        _dbviewer.dt.Columns[i].ColumnName);
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

    private void BuildColumns(IEnumerable<string> colnames) {
        dd_orderby.Items.Clear();
        foreach (string x in colnames) {
            var item = new ListItem(x, x);
            dd_orderby.Items.Add(item);
        }
    }

    private void TryGetExternalDb() {
        var connString = new ConnectionStringSettings("ExternalDB", GetCorrectConnectionString()[0], GetCorrectConnectionString()[1]);
        _dbviewer = new DBViewer(true, connString);
        BuildDropDown_External();
        SetTableData();
    }

    protected void dd_ddtables_Changed(object sender, EventArgs e) {
        SetTableData();
    }

    protected void btn_clear_Click(object sender, EventArgs e) {
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        tb_Databasename.Text = string.Empty;
        tb_connstring.Text = string.Empty;
        tb_selectcomm.Text = string.Empty;
        cb_ddselect.Items.Clear();
        if ((rb_adv_enabled.Checked) && (!rb_adv_disabled.Checked)) {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
            pnl_lbl.Enabled = true;
            pnl_lbl.Visible = true;
            pnl_txtselect.Enabled = true;
            pnl_txtselect.Visible = true;
        }
        else {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
            pnl_lbl.Enabled = false;
            pnl_lbl.Visible = false;
        }
        dd_ddtables.Items.Clear();
    }

    protected void btn_test_Click(object sender, EventArgs e) {
        TestSettings();
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
                return false;
            }
        }

        if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked)) {
            pnl_ddselect.Enabled = false;
            pnl_ddselect.Visible = false;
            pnl_lbl.Enabled = false;
            pnl_lbl.Visible = false;
            pnl_txtselect.Enabled = false;
            pnl_txtselect.Visible = false;
        }

        if (!string.IsNullOrEmpty(tb_connstring.Text)) {
            string provider = dd_provider.SelectedValue;
            bool success = false;
            try {
                DbProviderFactory defaultfactory = DbProviderFactories.GetFactory(provider);
                using (DbConnection defaultconn = defaultfactory.CreateConnection()) {
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
                }
                else {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.ForeColor = Color.Red;
                    successfull = false;
                }

            }
            catch (Exception e) {
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
                lbl_error.Text = e.Message;
                lbl_error.ForeColor = Color.Red;
                successfull = false;
            }
        }
        else {
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
            lbl_error.Text = "Connection String must be filled out.";
            lbl_error.ForeColor = Color.Red;
            successfull = false;
        }

        LoadAllDatabases(ref GV_Imports, "0", "desc");

        if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked) && (successfull)) {
            pnl_ddselect.Enabled = true;
            pnl_ddselect.Visible = true;
            pnl_lbl.Enabled = true;
            pnl_lbl.Visible = true;
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
            var importer = new DBImporter();
            importer.SavedConnectionsDeserialize();
            int indexof = tb_connstring.Text.IndexOf("'");
            if (indexof != -1) {
                string id = tb_connstring.Text.Substring(indexof + 1);
                id = id.Replace("'", "");
                foreach (var sc in importer.SavedConnections_Coll) {
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
            cs[0] = tb_connstring.Text.Trim();
            cs[1] = dd_provider.SelectedValue;
        }

        return cs;
    }

    private string GetCorrectProvider() {
        string dp = string.Empty;
        var importer = new DBImporter();
        importer.SavedConnectionsDeserialize();
        foreach (var sc in importer.SavedConnections_Coll) {
            if (sc.ConnectionString == GetCorrectConnectionString()[0]) {
                dp = sc.DatabaseProvider;
                break;
            }
        }

        return dp;
    }

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
            if ((!string.IsNullOrEmpty(tb_Databasename.Text)) && (!string.IsNullOrEmpty(tb_connstring.Text)) &&
                (!string.IsNullOrEmpty(tb_selectcomm.Text))) {
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
                        lbl_error.Enabled = true;
                        lbl_error.Visible = true;
                        lbl_error.Text = "Error Uploading! Only SELECT statements allowed.";
                        lbl_error.ForeColor = Color.Red;
                    }
                    else if (tb_selectcomm.Text.ToLower().Contains("aspnet_")) {
                        lbl_error.Enabled = true;
                        lbl_error.Visible = true;
                        lbl_error.Text = "Error Uploading! This table is not allowed.";
                        lbl_error.ForeColor = Color.Red;
                    }
                    else if ((tb_selectcomm.Text.ToLower().Substring(0, 6) == "select") &&
                             (!tb_selectcomm.Text.ToLower().Contains("aspnet_"))) {
                        string randomId = HelperMethods.RandomString(10);

                        string chartTitle = tb_chartTitle.Text.Trim();
                        if (string.IsNullOrEmpty(chartTitle)) {
                            chartTitle = tb_Databasename.Text.Trim();
                        }

                        var coll = new DBImporter_Coll(randomId, DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                                       tb_Databasename.Text.Trim(), GetCorrectConnectionString()[0],
                                                       tb_selectcomm.Text.Trim(), GetCorrectConnectionString()[1],
                                                       HttpContext.Current.User.Identity.Name, cb_AllowEditAdd.Checked.ToString(),
                                                       ddl_ChartType.SelectedValue, chartTitle, hf_usersAllowedToEdit.Value, cb_allowNotifi.Checked.ToString());
                        if (!_coll.Contains(coll)) {
                            _coll.Add(coll);
                            var db = new DBImporter();
                            db.BinarySerialize(_coll);
                            SaveConnections();
                            CreateApp(cb_AllowEditAdd.Checked, randomId);
                            lbl_error.Enabled = true;
                            lbl_error.Visible = true;
                            lbl_error.Text = tb_Databasename.Text + " has been saved.";
                            lbl_error.ForeColor = Color.Green;
                            tb_Databasename.Text = string.Empty;
                            tb_connstring.Text = string.Empty;
                            tb_selectcomm.Text = string.Empty;
                            cb_ddselect.Items.Clear();
                            dd_ddtables.Items.Clear();
                            if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked)) {
                                pnl_lbl.Enabled = false;
                                pnl_lbl.Visible = false;
                                pnl_lbl2.Enabled = true;
                                pnl_lbl2.Visible = true;
                                pnl_ddselect.Enabled = false;
                                pnl_ddselect.Visible = false;
                                pnl_txtselect.Enabled = false;
                                pnl_txtselect.Visible = false;
                            }
                            else {
                                pnl_lbl.Enabled = true;
                                pnl_lbl.Visible = true;
                                pnl_lbl2.Enabled = false;
                                pnl_lbl2.Visible = false;
                                pnl_ddselect.Enabled = false;
                                pnl_ddselect.Visible = false;
                                pnl_txtselect.Enabled = true;
                                pnl_txtselect.Visible = true;
                            }
                        }
                        else {
                            lbl_error.Enabled = true;
                            lbl_error.Visible = true;
                            lbl_error.Text = "Error Uploading!";
                            lbl_error.ForeColor = Color.Red;
                        }
                    }
                    else {
                        lbl_error.Enabled = true;
                        lbl_error.Visible = true;
                        lbl_error.Text = "Error Uploading! Only SELECT statements allowed.";
                        lbl_error.ForeColor = Color.Red;
                    }
                }
                catch {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.Text = "Error Uploading!";
                    lbl_error.ForeColor = Color.Red;
                }
            }
            else {
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
                lbl_error.Text = "All fields must be filled out.";
                lbl_error.ForeColor = Color.Red;
            }
        }
        else {
            SaveConnections();
            ImportDatabaseFromLocal();
        }
        LoadAllDatabases(ref GV_Imports, "0", "desc");
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
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.Text = "Error Uploading! Only SELECT statements allowed.";
                    lbl_error.ForeColor = Color.Red;
                }
                else if (tb_conditional.Text.ToLower().Contains("aspnet_") || tb_conditional.Text.ToLower().Contains("membership")) {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                    lbl_error.Text = "Error Uploading! This table is not allowed.";
                    lbl_error.ForeColor = Color.Red;
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

                    string chartTitle = tb_chartTitle.Text.Trim();
                    if (string.IsNullOrEmpty(chartTitle)) {
                        chartTitle = tb_Databasename.Text.Trim();
                    }

                    var coll = new DBImporter_Coll(randomId, DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                                   tb_Databasename.Text.Trim(),
                                                   GetCorrectConnectionString()[0], command, GetCorrectConnectionString()[1],
                                                   HttpContext.Current.User.Identity.Name, cb_AllowEditAdd.Checked.ToString(),
                                                   ddl_ChartType.SelectedValue, chartTitle, hf_usersAllowedToEdit.Value, cb_allowNotifi.Checked.ToString());
                    if (!_coll.Contains(coll)) {
                        _coll.Add(coll);
                        var db = new DBImporter();
                        db.BinarySerialize(_coll);
                        CreateApp(cb_AllowEditAdd.Checked, randomId);
                        lbl_error.Enabled = true;
                        lbl_error.Visible = true;
                        lbl_error.Text = tb_Databasename.Text + " has been saved.";
                        lbl_error.ForeColor = Color.Green;
                        tb_Databasename.Text = string.Empty;
                        tb_connstring.Text = string.Empty;
                        tb_selectcomm.Text = string.Empty;
                        cb_ddselect.Items.Clear();
                        dd_ddtables.Items.Clear();
                        if ((!rb_adv_enabled.Checked) && (rb_adv_disabled.Checked)) {
                            pnl_lbl.Enabled = false;
                            pnl_lbl.Visible = false;
                            pnl_lbl2.Enabled = true;
                            pnl_lbl2.Visible = true;
                            pnl_ddselect.Enabled = false;
                            pnl_ddselect.Visible = false;
                            pnl_txtselect.Enabled = false;
                            pnl_txtselect.Visible = false;
                        }
                        else {
                            pnl_lbl.Enabled = true;
                            pnl_lbl.Visible = true;
                            pnl_lbl2.Enabled = false;
                            pnl_lbl2.Visible = false;
                            pnl_ddselect.Enabled = false;
                            pnl_ddselect.Visible = false;
                            pnl_txtselect.Enabled = true;
                            pnl_txtselect.Visible = true;
                        }
                    }
                    else {
                        lbl_error.Enabled = true;
                        lbl_error.Visible = true;
                        lbl_error.Text = "Error Uploading!";
                        lbl_error.ForeColor = Color.Red;
                    }
                }
            }
            catch {
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
                lbl_error.Text = "Error Uploading!";
                lbl_error.ForeColor = Color.Red;
            }
        }
        else {
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
            lbl_error.Text = "All fields must be filled out.";
            lbl_error.ForeColor = Color.Red;
        }
    }

    protected void lbtn_selectAll_Click(object sender, EventArgs e) {
        if (lbtn_selectAll.Text == "Select All") {
            HiddenField1.Value = string.Empty;
            foreach (GridViewRow r in GV_Imports.Rows) {
                var chk = (CheckBox)r.FindControl("CheckBoxIndv");
                chk.Checked = true;
                string filename = chk.Text;
                string[] filelist = HiddenField1.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if (filelist.Contains(filename)) {
                    var templist = new List<string>();
                    for (int i = 0; i < filelist.Length; i++) {
                        if (!filelist[i].Contains(filename)) {
                            templist.Add(filelist[i]);
                        }
                    }
                    HiddenField1.Value = "";
                    if (templist.Count > 0) {
                        foreach (string file in templist) {
                            HiddenField1.Value += file + ServerSettings.StringDelimiter;
                        }
                    }
                }
                else {
                    HiddenField1.Value += filename + ServerSettings.StringDelimiter;
                }
            }
            lbtn_selectAll.Text = "Deselect All";
        }

        else {
            foreach (GridViewRow r in GV_Imports.Rows) {
                var chk = (CheckBox)r.FindControl("CheckBoxIndv");
                chk.Checked = false;
            }
            ResetSelected();
        }
    }

    protected void btn_refresh_Click(object sender, EventArgs e) {
        ResetSelected();
        LoadAllDatabases(ref GV_Imports, "0", "desc");
    }


    #region App Delete Password Check
    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        string[] list = HiddenField1.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        if (list.Length == 0) {
            RegisterPostbackScripts.RegisterStartupScript(this, "CancelRequest();");
        }
        else {
            string passwordUser = ServerSettings.AdminUserName;

            if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                var apps = new App();
                try {
                    for (var i = 0; i < list.Length; i++) {
                        string appid = apps.GetAppIDbyFilename("Database_Imports/" + list[i] + ".ascx");
                        string createdBy = apps.GetAppCreatedBy(appid);
                        if (!string.IsNullOrEmpty(createdBy)) {
                            MembershipUser u = Membership.GetUser(createdBy);
                            if (u != null) {
                                passwordUser = u.UserName;
                                break;
                            }
                        }
                    }
                }
                catch { }
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
    }

    protected void hf_StartDelete_Changed(object sender, EventArgs e) {
        var db = new DBImporter();
        var apps = new App();
        string[] list = HiddenField1.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        try {
            for (var i = 0; i < list.Length; i++) {
                db.DeleteEntry(list[i]);
                string appid = apps.GetAppIDbyFilename("Database_Imports/" + list[i] + ".ascx");
                apps.DeleteAppComplete(appid, ServerSettings.GetServerMapLocation);
                apps.DeleteAppLocal(appid);
            }
        }
        catch {
        }
        ResetSelected();
        _coll = db.DBColl;
        LoadAllDatabases(ref GV_Imports, "0", "desc");

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RemoveUpdateModal();");
    }
    #endregion


    /// Get Saved Connection Strings
    #region saved connection strings
    protected void btn_addconnectionstring_Click(object sender, EventArgs e) {
        if ((!string.IsNullOrEmpty(tb_connectionname.Text.Trim())) &&
            (tb_connectionname.Text.Trim().ToLower() != "[connection name]")
            && (!string.IsNullOrEmpty(tb_connectionstring.Text.Trim())) &&
            (tb_connectionstring.Text.Trim().ToLower() != "[connection string]")) {
            var importer = new DBImporter();
            importer.SavedConnectionsDeserialize();
            var sc = new SavedConnections(Guid.NewGuid().ToString(), tb_connectionstring.Text.Trim(),
                                          tb_connectionname.Text.Trim(), dd_provider_connectionstring.SelectedValue,
                                          DateTime.Now.ToString(CultureInfo.InvariantCulture), _username);
            if (!importer.SavedConnections_Coll.Contains(sc)) {
                importer.SavedConnections_Coll.Add(sc);
                importer.SavedConnectionsSerialize(importer.SavedConnections_Coll);

                tb_connectionname.Text = "[CONNECTION NAME]";
                tb_connectionstring.Text = "[CONNECTION STRING]";

                var str = new StringBuilder();
                str.Append(
                    "$('#savedconnections_postmessage').html(\"<span style='color: Green;\">Connection string added successfully</span>');");
                str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
                RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
            }
            else {
                var str = new StringBuilder();
                str.Append(
                    "$('#savedconnections_postmessage').html(\"<span style='color: Red;\">Connection string already exists</span>');");
                str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
                RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
            }
            GetConnections();
        }
        else {
            var str = new StringBuilder();
            str.Append(
                "$('#savedconnections_postmessage').html('<span style=\"color: Red;\">Cannot add new connection string</span>');");
            str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }
    }

    private void GetConnections() {
        var importer = new DBImporter();
        pnl_savedconnections_holder.Controls.Clear();
        var str = new StringBuilder();
        importer.SavedConnectionsDeserialize();

        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='min-width: 100%;'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px' align='left'>Connection Name</td>");
        str.Append("<td align='left'>Connection String</td>");
        str.Append("<td width='155px'>Database Provider</td>");
        str.Append("<td width='100px'>Actions</td></tr>");

        int count = 1;
        if (importer.SavedConnections_Coll.Count > 0) {
            foreach (var sc in importer.SavedConnections_Coll) {
                string ds;

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

                    ds = sc.ConnectionString.Replace(un, "*********");
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

                    ds = sc.ConnectionString.Replace(un, "*********");
                }
                else {
                    ds = sc.ConnectionString;
                }

                // Change Password
                connectionstring = ds;
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

                    ds = ds.Replace(password, "*********");
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

                    ds = ds.Replace(password, "*********");
                }

                if (hf_editstring.Value == sc.ID) {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td width='45px' class='GridViewNumRow border-bottom' align='center'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                    str.Append("<td width='150px' class='border-right border-bottom'><input id='tb_connNameedit' type='text' value='" + sc.ConnectionName + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 98%;' /></td>");
                    str.Append("<td class='border-right border-bottom'><input id='tb_connStringedit' type='text' value='" + ds + "' class='textEntry' onkeypress=\"KeyPressEdit_Connection(event, '" + sc.ID + "');\" style='width: 98%;' /></td>");

                    string options = "<select id='edit-databaseProvider' style='width: 150px;'>";
                    if (sc.DatabaseProvider == "System.Data.SqlServerCe.4.0") {
                        options += "<option selected='selected' value='System.Data.SqlServerCe.4.0'>System.Data.SqlServerCe.4.0</option>";
                    }
                    else {
                        options += "<option value='System.Data.SqlServerCe.4.0'>System.Data.SqlServerCe.4.0</option>";
                    }
                    if (sc.DatabaseProvider == "System.Data.SqlClient") {
                        options += "<option selected='selected' value='System.Data.SqlClient'>System.Data.SqlClient</option>";
                    }
                    else {
                        options += "<option value='System.Data.SqlClient'>System.Data.SqlClient</option>";
                    }
                    if (sc.DatabaseProvider == "System.Data.Odbc") {
                        options += "<option selected='selected' value='System.Data.Odbc'>System.Data.Odbc</option>";
                    }
                    else {
                        options += "<option value='System.Data.Odbc'>System.Data.Odbc</option>";
                    }
                    if (sc.DatabaseProvider == "System.Data.OracleClient") {
                        options += "<option selected='selected' value='System.Data.OracleClient'>System.Data.OracleClient</option>";
                    }
                    else {
                        options += "<option value='System.Data.OracleClient'>System.Data.OracleClient</option>";
                    }
                    if (sc.DatabaseProvider == "System.Data.OleDb") {
                        options += "<option selected='selected' value='System.Data.OleDb'>System.Data.OleDb</option>";
                    }
                    else {
                        options += "<option value='System.Data.OleDb'>System.Data.OleDb</option>";
                    }
                    options += "</select>";

                    str.Append("<td width='155px' class='border-right border-bottom'>" + options + "</td>");

                    var str2 = new StringBuilder();
                    str2.Append("<a href='#update' onclick='UpdateConnectionString(\"" + sc.ID + "\");return false;' class='td-update-btn' title='Update'></a>");
                    str2.Append("<a href='#cancel' onclick='EditConnectionString(\"reset\");return false;' class='td-cancel-btn margin-left' title='Cancel'></a>");

                    str.Append("<td align='center' width='100px' class='border-right border-bottom'>" + str2 + "</td>");
                    str.Append("</tr>");
                }
                else {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td width='45px' class='GridViewNumRow border-bottom' align='center'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                    str.Append("<td width='150px' class='border-right border-bottom'>" + sc.ConnectionName + "</td>");
                    str.Append("<td class='border-right border-bottom'>" + ds + "</td>");
                    str.Append("<td width='155px' class='border-right border-bottom'>" + sc.DatabaseProvider + "</td>");

                    var str2 = new StringBuilder();
                    str2.Append("<a href='#use' onclick='UseConnectionString(\"" + sc.ID + "\", \"" + sc.ConnectionName + "\");return false;' class='td-download-btn' title='Use String'></a>");
                    if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || (_username.ToLower() == sc.Username.ToLower())) {
                        str2.Append("<a href='#edit' onclick='EditConnectionString(\"" + sc.ID + "\");return false;' class='td-edit-btn margin-left' title='Edit'></a>");
                        str2.Append("<a href='#delete' onclick='DeleteConnectionString(\"" + sc.ID + "\");return false;' class='td-delete-btn margin-left' title='Delete'></a>");
                    }

                    str.Append("<td align='center' width='100px' class='border-right border-bottom'>" + str2 + "</td>");
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
            var importer = new DBImporter();
            importer.SavedConnectionsDeserialize();
            bool cont =
                importer.SavedConnections_Coll.All(
                    saved =>
                    (saved.ConnectionString != GetCorrectConnectionString()[0]) ||
                    (saved.DatabaseProvider != dd_provider.SelectedValue));

            if (cont) {
                var sc = new SavedConnections(Guid.NewGuid().ToString(), GetCorrectConnectionString()[0],
                                              "DB Import " +
                                              (importer.SavedConnections_Coll.Count + 1).ToString(
                                                  CultureInfo.InvariantCulture),
                                              GetCorrectConnectionString()[1],
                                              DateTime.Now.ToString(CultureInfo.InvariantCulture), _username);
                if (!importer.SavedConnections_Coll.Contains(sc)) {
                    importer.SavedConnections_Coll.Add(sc);
                    importer.SavedConnectionsSerialize(importer.SavedConnections_Coll);

                    tb_connectionname.Text = "[CONNECTION NAME]";
                    tb_connectionstring.Text = "[CONNECTION STRING]";

                    var str = new StringBuilder();
                    str.Append(
                        "$('#savedconnections_postmessage').html(\"<span style='color: Green;'>Connection string added successfully</span>\");");
                    str.Append("setTimeout(function(){$('#savedconnections_postmessage').html('');}, 4000);");
                    RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
                }
                GetConnections();
            }
        }
    }

    protected void hf_deletestring_Changed(object sender, EventArgs e) {
        var importer = new DBImporter();
        importer.SavedConnectionsDeserialize();
        var temp = (from sc in importer.SavedConnections_Coll
                    where sc.ID != hf_deletestring.Value
                    select
                        new SavedConnections(sc.ID, sc.ConnectionString, sc.ConnectionName, sc.DatabaseProvider,
                                             sc.Date, sc.Username)).ToList();
        importer.SavedConnectionsSerialize(temp);
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
        var importer = new DBImporter();
        importer.SavedConnectionsDeserialize();

        List<SavedConnections> savedConn = new List<SavedConnections>();
        foreach (SavedConnections sc in importer.SavedConnections_Coll) {
            if (sc.ID == hf_updatestring.Value) {
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

                savedConn.Add(new SavedConnections(sc.ID, str, name, provider, DateTime.Now.ToString(), _username));
            }
            else {
                savedConn.Add(new SavedConnections(sc.ID, sc.ConnectionString, sc.ConnectionName, sc.DatabaseProvider, sc.Date, sc.Username));
            }
        }

        importer.SavedConnectionsSerialize(savedConn);

        hf_updatestring.Value = string.Empty;
        hf_connectionNameEdit.Value = string.Empty;
        hf_connectionStringEdit.Value = string.Empty;
        hf_databaseProviderEdit.Value = string.Empty;
        hf_editstring.Value = string.Empty;
        GetConnections();
    }

    protected void hf_usestring_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_connstring.Text)) {
            int count =
                dd_provider.Items.Cast<ListItem>().TakeWhile(item => item.Value != GetCorrectProvider()).Count();

            dd_provider.SelectedIndex = count;

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
        }
        else {
            lbl_error.Enabled = false;
            lbl_error.Visible = false;
        }
    }

    #endregion


    #region GridView Properties Methods

    protected void GV_Imports_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                var hfImportedby = (HiddenField)e.Row.FindControl("hf_importedby");
                if (hfImportedby != null) {
                    if (_username.ToLower() != hfImportedby.Value.ToLower()) {
                        var linkButton1 = (LinkButton)e.Row.FindControl("LinkButton1");
                        var LinkButton2 = (LinkButton)e.Row.FindControl("LinkButton2");
                        var LinkButton3 = (LinkButton)e.Row.FindControl("LinkButton3");
                        if (linkButton1 != null && LinkButton2 != null && LinkButton3 != null) {
                            linkButton1.Enabled = false;
                            linkButton1.Visible = false;
                            LinkButton2.Enabled = false;
                            LinkButton2.Visible = false;
                            LinkButton3.Enabled = false;
                            LinkButton3.Visible = false;
                        }
                    }
                }
            }

            var hf_appID = (HiddenField)e.Row.FindControl("hf_appID");
            if (hf_appID != null) {
                var LinkButton2 = (LinkButton)e.Row.FindControl("LinkButton2");
                if (LinkButton2 != null) {
                    LinkButton2.CommandName = "create-" + hf_appID.Value;
                    if (AppExists(hf_appID.Value)) {
                        LinkButton2.Enabled = false;
                        LinkButton2.Visible = false;
                    }
                    else {
                        LinkButton2.Enabled = true;
                        LinkButton2.Visible = true;
                    }
                }
            }
        }
    }
    private bool AppExists(string appId) {
        App apps = new App();
        string fileName = apps.GetAppFilename("app-" + appId);
        var fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + appId + ".ascx");

        if (!string.IsNullOrEmpty(fileName)) {
            return true;
        }
        else if (fi.Exists) {
            try {
                fi.Delete();
            }
            catch { }
        }

        return false;
    }

    protected void GV_Imports_RowEdit(object sender, GridViewEditEventArgs e) {
        ResetSelected();
        GV_Imports.EditIndex = e.NewEditIndex;
        LoadAllDatabases(ref GV_Imports, "0", "desc");

        var id = (HiddenField)GV_Imports.Rows[e.NewEditIndex].FindControl("hf_editID");
        var AllowEditAdd = (CheckBox)GV_Imports.Rows[e.NewEditIndex].FindControl("cb_AllowEditAdd_edit");
        var NotifyUsers = (CheckBox)GV_Imports.Rows[e.NewEditIndex].FindControl("cb_NotifyUsers_Edit");
        var ddl_chartType_edit = (DropDownList)GV_Imports.Rows[e.NewEditIndex].FindControl("ddl_chartType_edit");
        if ((id != null) && (AllowEditAdd != null) && (ddl_chartType_edit != null)) {
            var db = new DBImporter();
            db.BinaryDeserialize();
            foreach (DBImporter_Coll coll in db.DBColl) {
                if (coll.ID == id.Value) {
                    AllowEditAdd.Checked = coll.AllowEdit;
                    NotifyUsers.Checked = coll.NotifyUsers;

                    ddl_chartType_edit.Items.Clear();
                    Array chartTypes = Enum.GetValues(typeof(ChartType));

                    int count = 0;
                    int selectedIndex = 0;
                    foreach (ChartType type in chartTypes) {
                        ddl_chartType_edit.Items.Add(new ListItem(type.ToString(), type.ToString()));
                        if (coll.Chart_Type == type) {
                            selectedIndex = count;
                        }

                        count++;
                    }

                    ddl_chartType_edit.SelectedIndex = selectedIndex;
                    break;
                }
            }
        }
    }

    protected void GV_Imports_RowUpdate(object sender, GridViewUpdateEventArgs e) {
        var id = (HiddenField)GV_Imports.Rows[e.RowIndex].FindControl("hf_editID");
        var name = (TextBox)GV_Imports.Rows[e.RowIndex].FindControl("tb_editName");
        var command = (TextBox)GV_Imports.Rows[e.RowIndex].FindControl("tb_editCommand");
        var AllowEditAdd = (CheckBox)GV_Imports.Rows[e.RowIndex].FindControl("cb_AllowEditAdd_edit");
        var NotifyUsers = (CheckBox)GV_Imports.Rows[e.RowIndex].FindControl("cb_NotifyUsers_Edit");
        var ddl_chartType_edit = (DropDownList)GV_Imports.Rows[e.RowIndex].FindControl("ddl_chartType_edit");
        var tb_chartTitle_edit = (TextBox)GV_Imports.Rows[e.RowIndex].FindControl("tb_chartTitle_edit");
        var db = new DBImporter();

        if ((command.Text.ToLower().Contains("delete ")) ||
            (command.Text.ToLower().Contains("update ")) ||
            (command.Text.ToLower().Contains("insert ")) ||
            (command.Text.ToLower().Contains("create ")) ||
            (command.Text.ToLower().Contains("commit ")) ||
            (command.Text.ToLower().Contains("rollback ")) ||
            (command.Text.ToLower().Contains("grant revoke "))) {
            lbl_error2.Enabled = true;
            lbl_error2.Visible = true;
            lbl_error2.Text = "Error Uploading! Only SELECT statements allowed.";
            lbl_error2.ForeColor = Color.Red;
        }
        else if (command.Text.ToLower().Contains("aspnet_")) {
            lbl_error2.Enabled = true;
            lbl_error2.Visible = true;
            lbl_error2.Text = "Error Uploading! This table is not allowed.";
            lbl_error2.ForeColor = Color.Red;
        }
        else {
            lbl_error2.Enabled = false;
            lbl_error2.Visible = false;
            lbl_error2.Text = "";

            string chartTitle = tb_chartTitle_edit.Text.Trim();
            if (string.IsNullOrEmpty(chartTitle)) {
                chartTitle = name.Text.Trim();
            }

            ChartType chartType = ChartType.None;
            try {
                chartType = (ChartType)Enum.Parse(typeof(ChartType), ddl_chartType_edit.SelectedValue);
            }
            catch { }

            db.UpdateEntry(id.Value, name.Text.Trim(), command.Text.Trim(), AllowEditAdd.Checked, chartType, chartTitle, NotifyUsers.Checked);

            App _app = new App();
            _app.UpdateAppName("app-" + id.Value, name.Text.Trim());
            _app.UpdateAppLocal("app-" + id.Value, name.Text.Trim());

            string filePath = ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + id.Value + ".ascx";
            if (File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                    dbImportAppCreator dbiwc = new dbImportAppCreator(_username, AllowEditAdd.Checked);
                    dbiwc.CreateApp("", "", "", "", false, false, id.Value);
                }
                catch { }
            }

            GV_Imports.EditIndex = -1;
            _coll = db.DBColl;
            ResetSelected();
            LoadAllDatabases(ref GV_Imports, "0", "desc");
        }
    }

    protected void GV_Imports_RowDelete(object sender, GridViewDeleteEventArgs e) {
        LoadAllDatabases(ref GV_Imports, "0", "desc");
    }

    protected void GV_Imports_CancelEdit(object sender, GridViewCancelEditEventArgs e) {
        lbl_error2.Enabled = false;
        lbl_error2.Visible = false;
        lbl_error2.Text = "";
        GV_Imports.EditIndex = -1;
        LoadAllDatabases(ref GV_Imports, "0", "desc");
    }

    protected void GV_Imports_RowCommand(object sender, GridViewCommandEventArgs e) {
        var db = new DBImporter();
        switch (e.CommandName) {
            case "Delete":
                string script = "$('#MainContent_HiddenField1').val('" + e.CommandArgument.ToString() + ";');dbType = 'delete';";
                RegisterPostbackScripts.RegisterStartupScript(this, script);
                break;
        }

        if (e.CommandName.Contains("create-")) {
            string appId = e.CommandName.Replace("create-", "");
            db.BinaryDeserialize();
            bool allowEdit = false;
            string fileName = "";
            string appName = "";
            string categoryid = AppCategoryID();
            foreach (DBImporter_Coll coll in db.DBColl) {
                if (coll.ID == appId) {
                    fileName = coll.ID;
                    appName = coll.TableName;
                    allowEdit = coll.AllowEdit;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(fileName)) {
                dbImportAppCreator dbiwc = new dbImportAppCreator(_username, allowEdit);
                bool insertIntoAppList = true;
                App apps = new App();
                if (!string.IsNullOrEmpty(apps.GetAppInformation("app-" + fileName).ID)) {
                    insertIntoAppList = false;
                }
                dbiwc.CreateApp(appName, categoryid, "", "", false, insertIntoAppList, fileName);
            }

            ResetSelected();
            LoadAllDatabases(ref GV_Imports, "0", "desc");
        }
    }

    public void LoadAllDatabases(ref GridView gv, string sortExp, string sortDir) {
        lbl_error2.Enabled = false;
        lbl_error2.Visible = false;
        lbl_error2.Text = "";
        DataView dvlist = GetList();
        if (dvlist.Count > 0) {
            if (sortExp != string.Empty) {
                dvlist.Sort = string.Format("{0} {1}", dvlist.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvlist;
        gv.DataBind();
        if (dvlist.Count == 0) {
            lbtn_selectAll.Enabled = false;
            lbtn_selectAll.Visible = false;
        }
        else {
            lbtn_selectAll.Enabled = true;
            lbtn_selectAll.Visible = true;
        }
    }

    public DataView GetList() {
        var dtlist = new DataTable();
        dtlist.Columns.Add(new DataColumn("dateLong"));
        dtlist.Columns.Add(new DataColumn("date"));
        dtlist.Columns.Add(new DataColumn("tablename"));
        dtlist.Columns.Add(new DataColumn("chartType"));
        dtlist.Columns.Add(new DataColumn("chartTitle"));
        dtlist.Columns.Add(new DataColumn("connstring"));
        dtlist.Columns.Add(new DataColumn("selectcomm"));
        dtlist.Columns.Add(new DataColumn("provider"));
        dtlist.Columns.Add(new DataColumn("notify"));
        dtlist.Columns.Add(new DataColumn("importedby"));
        dtlist.Columns.Add(new DataColumn("importedby_username"));
        dtlist.Columns.Add(new DataColumn("ID"));
        dtlist.Columns.Add(new DataColumn("AllowEdit"));

        App apps = new App();

        foreach (var row in _coll) {

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckDBImportsGroupAssociation(row, _member)) {
                    continue;
                }
            }

            if (AppExists(row.ID)) {
                if ((_username.ToLower() != row.ImportedBy.ToLower()) && (apps.GetIsPrivate(row.ID)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    continue;
                }
            }

            DataRow drlist = dtlist.NewRow();
            var member = new MemberDatabase(row.ImportedBy);
            drlist["dateLong"] = Convert.ToDateTime(row.Date).Ticks;
            drlist["date"] = row.Date;
            drlist["tablename"] = row.TableName;

            drlist["chartType"] = row.Chart_Type.ToString();
            drlist["chartTitle"] = row.ChartTitle;

            string datasource = row.ConnString;
            try {
                DbProviderFactory factory = DbProviderFactories.GetFactory(row.Provider);
                using (DbConnection conn = factory.CreateConnection()) {
                    if (conn != null) {
                        conn.ConnectionString = datasource;
                        datasource = conn.DataSource;
                    }
                }
            }
            catch {
            }
            if (datasource != null) drlist["connstring"] = datasource.Trim();
            drlist["selectcomm"] = row.SelectCommand;
            drlist["provider"] = row.Provider;

            string _checked = string.Empty;
            if (row.NotifyUsers) {
                _checked = "checked='checked'";
            }

            drlist["notify"] = "<input type='checkbox' disabled='disabled' " + _checked + " />";
            drlist["importedby"] = HelperMethods.MergeFMLNames(member);
            drlist["importedby_username"] = member.Username;
            drlist["ID"] = row.ID;
            drlist["AllowEdit"] = row.AllowEdit.ToString();

            dtlist.Rows.Add(drlist);
        }
        var dvlist = new DataView(dtlist);
        return dvlist;
    }

    private void ResetSelected() {
        HiddenField1.Value = string.Empty;
        lbtn_selectAll.Text = "Select All";
    }

    protected void CheckBoxIndv_CheckChanged(object sender, EventArgs e) {
        var chk = (CheckBox)sender;
        string filename = chk.Text;
        string[] filelist = HiddenField1.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        if (filelist.Contains(filename)) {
            var templist = new List<string>();
            for (int i = 0; i < filelist.Length; i++) {
                if (!filelist[i].Contains(filename)) {
                    templist.Add(filelist[i]);
                }
            }
            HiddenField1.Value = "";
            if (templist.Count > 0) {
                foreach (string file in templist) {
                    HiddenField1.Value += file + ServerSettings.StringDelimiter;
                }
            }
        }
        else {
            HiddenField1.Value += filename + ServerSettings.StringDelimiter;
        }
    }

    #endregion

}