using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Security.Principal;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlServerCe;
using System.Web.Security;
using OpenWSE_Tools.Notifications;
using System.Net.Mail;
using System.Linq;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class dbImportservice : System.Web.Services.WebService {
    #region Variables

    private readonly DBViewer _dbviewer = new DBViewer(true);
    private readonly DatabaseCall dbCall;
    private readonly AppLog _applog = new AppLog(false);
    private IIdentity _userId;
    private string errorMessage = string.Empty;
    private string sortColOut = string.Empty;
    private string sortDirOut = string.Empty;

    #endregion

    public dbImportservice() {
        _userId = HttpContext.Current.User.Identity;
        ConnectionStringSettings _connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
        dbCall = new DatabaseCall(_connString.ProviderName, _connString.ConnectionString);
    }


    [WebMethod]
    public object[] GetRecords(string id, string search, string recordstopull, string sortCol, string sortDir) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, sortCol, sortDir, out tableName, out coll);

            List<object> list1 = new List<object>();
            if (dt != null) {
                List<string> allowedCols = GetAllowedColumns(coll);
                BuildRowHeader(dt.Columns, id, ref list1, sortColOut, sortDirOut, coll, allowedCols);
                BuildRowItem(dt, id, ref list1, count, recordstopull, search, allowedCols);
            }
            list.Add(list1);

            if (coll.AllowEdit) {
                string userAllowed = coll.UsersAllowedToEdit.Find(x => x.ToLower() == _userId.Name.ToLower());
                if (!string.IsNullOrEmpty(userAllowed)) {
                    list.Add("true");
                }
                else {
                    list.Add("false");
                }
            }
            else {
                list.Add("false");
            }

            DataChartsOpened dco = new DataChartsOpened(_userId.Name);
            list.Add(dco.IsChartOpenedForUser("app-" + id).ToString().ToLower());
        }
        return list.ToArray();
    }
    private void BuildRowHeader(DataColumnCollection columns, string id, ref List<object> list1, string sortCol, string sortDir, DBImporter_Coll coll, List<string> allowedColumns) {
        StringBuilder str = new StringBuilder();
        foreach (DataColumn dc in columns) {
            string active = string.Empty;
            if (allowedColumns.Count == 0 || allowedColumns.Contains(dc.ColumnName.ToLower())) {
                if (sortCol.ToLower() == dc.ColumnName.ToLower()) {
                    if (sortDir.ToLower() == "asc")
                        active = " active asc";
                    else
                        active = " active desc";
                }
                str.Append("<td class=\"td-sort-click" + active + "\" onclick=\"dbImport.OnSortClick(this,'" + dc.ColumnName + "','" + id + "');\" title=\"Sort by " + dc.ColumnName.Replace("_", " ") + "\">" + dc.ColumnName.Replace("_", " ") + "</td>");
            }
        }

        list1.Add(str.ToString());
    }
    private void BuildRowItem(DataTable dt, string id, ref List<object> list1, int count, string recordstopull, string search, List<string> allowedColumns) {
        StringBuilder str = new StringBuilder();
        // Add Entry Row
        foreach (DataColumn dc in dt.Columns) {
            if (allowedColumns.Count == 0 || allowedColumns.Contains(dc.ColumnName.ToLower())) {
                string nullable = string.Empty;
                if (!dc.AllowDBNull)
                    nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                string val = string.Empty;
                if (dc.DataType.Name.ToLower() == "guid") {
                    val = Guid.NewGuid().ToString();
                }

                int maxLength = dc.MaxLength;
                if (maxLength > 5000 || maxLength <= 0)
                    maxLength = 5000;

                str.Append("<td class=\"border-right border-bottom\" align=\"center\">");
                str.AppendLine("<span class=\"td-columnName-add\" style=\"display: none!important;\">" + dc.ColumnName + "</span>");
                str.Append("<span class=\"td-columnType-add\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>");
                str.Append("<span class=\"td-columnValue-add\" style=\"display: none!important;\">" + val + "</span>");
                str.Append("<input type='text' class='textEntry-noWidth' maxlength='" + maxLength + "' onkeyup=\"dbImport.AddRecordKeyPress(event, '" + id + "');\" style='width: 85%;' />" + nullable + "</td>");
            }
        }
        list1.Add(str.ToString());


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows) {
            if (index >= count + 1) {
                if (recordstopull != "all")
                    break;
            }
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();
            bool canAdd = false;

            foreach (DataColumn dc in dt.Columns) {
                if (allowedColumns.Count == 0 || allowedColumns.Contains(dc.ColumnName.ToLower())) {
                    if ((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                        canAdd = true;

                    List<object> list5 = new List<object>();
                    list5.Add(dr[dc.ColumnName].ToString());
                    list5.Add(dc.DataType.Name.ToLower());
                    list4.Add(list5);
                }
            }

            if (canAdd) {
                // list3[0] = array row data
                // list3[1] = edit mode
                list3.Add(list4);
                list3.Add(false);

                list2.Add(list3);
                index++;
            }
        }
        list1.Add(list2);
    }


    [WebMethod]
    public object[] EditRecord(string id, object rowData, string search, string recordstopull, string sortCol, string sortDir) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, sortCol, sortDir, out tableName, out coll);
            if (dt != null) {
                List<string> allowedCols = GetAllowedColumns(coll);
                BuildRowHeader(dt.Columns, id, ref list, sortColOut, sortDirOut, coll, allowedCols);
                BuildRowItemEdit(dt, id, rowData as object[], ref list, count, recordstopull, search, allowedCols);
            }
        }
        return list.ToArray();
    }
    private void BuildRowItemEdit(DataTable dt, string id, object[] rowData, ref List<object> list1, int count, string recordstopull, string search, List<string> allowedColumns) {
        StringBuilder str = new StringBuilder();

        // Add Entry Row
        foreach (DataColumn dc in dt.Columns) {
            str.Append("<td align=\"center\" class=\"border-right border-bottom\"></td>");
        }
        list1.Add(str.ToString());


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows) {
            if (index >= count + 1) {
                if (recordstopull != "all")
                    break;
            }
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();

            bool isRowToEdit = true;
            if (rowData != null) {
                for (int i = 0; i < rowData.Length; i++) {
                    string r = rowData[i].ToString().Trim().Replace("\n", "").Replace("\r", "");
                    if (dr[i].ToString().Trim() != r) {
                        isRowToEdit = false;
                        break;
                    }
                }
            }
            else {
                isRowToEdit = false;
            }

            if (isRowToEdit) {
                foreach (DataColumn dc in dt.Columns) {
                    if (allowedColumns.Count == 0 || allowedColumns.Contains(dc.ColumnName.ToLower())) {
                        string nullable = string.Empty;
                        if (!dc.AllowDBNull)
                            nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                        int maxLength = dc.MaxLength;
                        if (maxLength > 5000)
                            maxLength = 5000;

                        string inputText = "<span class=\"td-columnName-edit\" style=\"display: none!important;\">" + dc.ColumnName + "</span>";
                        inputText += "<span class=\"td-columnValue-edit\" style=\"display: none!important;\">" + dr[dc.ColumnName].ToString() + "</span>";
                        inputText += "<span class=\"td-columnType-edit\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>";
                        inputText += "<input type='text' class='textEntry-noWidth' maxlength='" + maxLength + "' onkeyup=\"dbImport.UpdateRecordKeyPress(event, '" + id + "', this);\" style='width: 85%;' />" + nullable;

                        list4.Add(inputText);
                    }
                }


                // list3[0] = array row data
                // list3[1] = edit mode
                list3.Add(list4);
                list3.Add(true);

                list2.Add(list3);
                index++;
            }
            else {
                bool canAdd = false;
                foreach (DataColumn dc in dt.Columns) {
                    if ((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                        canAdd = true;

                    list4.Add(dr[dc.ColumnName].ToString());
                }

                if (canAdd) {
                    // list3[0] = array row data
                    // list3[1] = edit mode
                    list3.Add(list4);
                    list3.Add(false);

                    list2.Add(list3);
                    index++;
                }
            }
        }
        list1.Add(list2);
    }


    [WebMethod]
    public void SetChartView(string id, string view) {
        if (_userId.IsAuthenticated) {
            DataChartsOpened dco = new DataChartsOpened(_userId.Name);
            if (view == "true") {
                dco.AddItem("app-" + id);
            }
            else {
                dco.DeleteItem("app-" + id);
            }
        }
    }


    [WebMethod]
    public object[] DeleteRecord(string id, object rowData) {
        List<object> returnVals = new List<object>();

        if (_userId.IsAuthenticated) {
            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, "", "", out tableName, out coll);
            if (DeleteEntry(tableName, dt.Columns, rowData as object[], coll, out errorMessage)) {
                returnVals.Add("Success");
                returnVals.Add("");
                AddNotification(coll, string.Empty, "delete");
                return returnVals.ToArray();
            }
            else {
                returnVals.Add("Error");
                returnVals.Add(errorMessage);
                return returnVals.ToArray();
            }
        }

        returnVals.Add("Error");
        returnVals.Add(errorMessage);
        return returnVals.ToArray();
    }
    private bool DeleteEntry(string table, DataColumnCollection colList, object[] rowData, DBImporter_Coll dbcoll, out string errorMessage) {
        errorMessage = string.Empty;
        bool didDelete = true;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        for (int i = 0; i < rowData.Length; i++) {
            string r = rowData[i].ToString().Trim().Replace("\n", "").Replace("\r", "");
            query.Add(new DatabaseQuery(colList[i].ColumnName, r));
        }

        if (!dbCall.CallDelete(table, query)) {
            didDelete = false;
            errorMessage = "Error deleting entry! Please try again";
        }

        return didDelete;
    }


    [WebMethod]
    public object[] AddRecord(string id, object recordVals) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        string changeMade = CustomTableViewer.BuildChangeText(vals);

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }

        string tableName = string.Empty;
        DBImporter_Coll coll;
        DataTable dt = GetTable(id, "", "", out tableName, out coll);
        List<string> newcolumns = BuildColumnList(dt);
        if (AddEntry(tableName, dic, newcolumns, coll, out errorMessage)) {
            returnVals.Add("Success");
            returnVals.Add("");
            AddNotification(coll, changeMade, "add");
            return returnVals.ToArray();
        }
        else {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool AddEntry(string table, Dictionary<string, string> dic, List<string> columns, DBImporter_Coll dbcoll, out string errorMessage) {
        errorMessage = string.Empty;
        bool didAdd = true;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        foreach (string column in columns) {
            string val = "";
            dic.TryGetValue(column, out val);
            query.Add(new DatabaseQuery(column, val));
        }

        if (!dbCall.CallInsert(table, query)) {
            didAdd = false;
            errorMessage = "Error inserting entry! Please try again";
        }

        return didAdd;
    }


    [WebMethod]
    public object[] UpdateRecord(string id, object recordVals, object rowData) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        string changeMade = CustomTableViewer.BuildChangeText(vals);

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }

        string tableName = string.Empty;
        DBImporter_Coll coll;
        DataTable dt = GetTable(id, "", "", out tableName, out coll);

        List<string> newcolumns = BuildColumnList(dt);
        if (UpdateEntry(tableName, BuildColumnsForUpdate(dt), dt.Columns, rowData as object[], dic, newcolumns, coll, out errorMessage)) {
            returnVals.Add("Success");
            returnVals.Add("");
            AddNotification(coll, changeMade, "update");
            return returnVals.ToArray();
        }
        else {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool UpdateEntry(string table, string columnString, DataColumnCollection colList, object[] rowData, Dictionary<string, string> dic, List<string> columns, DBImporter_Coll dbcoll, out string errorMessage) {
        errorMessage = string.Empty;
        bool didUpdate = true;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        for (int i = 0; i < rowData.Length; i++) {
            string r = rowData[i].ToString().Trim().Replace("\n", "").Replace("\r", "");
            query.Add(new DatabaseQuery(colList[i].ColumnName, r));
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        foreach (string column in columns) {
            string val = "";
            dic.TryGetValue(column, out val);
            updateQuery.Add(new DatabaseQuery(column, val));
        }

        if (!dbCall.CallUpdate(table, updateQuery, query)) {
            didUpdate = false;
            errorMessage = "Error updating entry! Please try again";
        }

        return didUpdate;
    }


    [WebMethod]
    public string ExportToExcel(string id) {
        string tableName = string.Empty;
        DBImporter_Coll coll;
        DataTable dt = GetTable(id, "", "", out tableName, out coll);

        if (string.IsNullOrEmpty(tableName))
            return "";

        string _Path = tableName.Replace(" ", "_") + "-" + DateTime.Now.Ticks.ToString() + ".xls";
        string directory = ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        var temp = new DataTable();
        try {
            if (dt != null) {
                foreach (DataColumn dc in dt.Columns) {
                    if (!temp.Columns.Contains(dc.ColumnName))
                        temp.Columns.Add(new DataColumn(dc.ColumnName));
                }

                if (dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        DataRow drsch = temp.NewRow();
                        foreach (DataColumn dc in temp.Columns)
                            drsch[dc.ColumnName] = dr[dc.ColumnName];

                        temp.Rows.Add(drsch);
                    }
                }
            }

            if (temp.Rows.Count == 0)
                return "";

            var stringWriter = new StringWriter();
            var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWriter);

            var DataGrd = new DataGrid();
            DataGrd.DataSource = temp;
            DataGrd.DataBind();

            DataGrd.RenderControl(htmlWrite);

            try {
                if (File.Exists(p)) {
                    File.Delete(p);
                }
            }
            catch { }

            var vw = new StreamWriter(p, true);
            stringWriter.ToString().Normalize();
            vw.Write(stringWriter.ToString());
            vw.Flush();
            vw.Close();

            return "Apps/Database_Imports/Exports/" + _Path;
        }
        catch {
            return "File path wrong.";
        }
    }


    private string BuildColumns(DataTable t) {
        var columns = new StringBuilder();
        columns.Append("VALUES(");
        int count = t.Columns.Count - 1;
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            columns.Append("@" + c.ColumnName);
            if (i < count) {
                columns.Append(", ");
            }
        }
        columns.Append(")");

        return columns.ToString();
    }
    private string BuildColumnsForUpdate(DataTable t) {
        var columns = new StringBuilder();
        int count = t.Columns.Count - 1;
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            columns.Append(c.ColumnName + "=@" + c.ColumnName);
            if (i < count)
                columns.Append(" AND ");
        }

        return columns.ToString();
    }
    private List<string> BuildColumnList(DataTable t) {
        var list = new List<string>();
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            list.Add(c.ColumnName);
        }

        return list;
    }

    public static DBImporter_Coll GetDataBase_Coll(string id) {
        DBImporter_Coll dbcoll_temp = null;
        var db = new DBImporter();
        db.BinaryDeserialize();
        foreach (var dbcoll in db.DBColl) {
            if (dbcoll.ID == id) {
                dbcoll_temp = dbcoll;
                break;
            }
        }

        return dbcoll_temp;
    }
    private DataTable GetTable(string id, string sortCol, string sortDir, out string table, out DBImporter_Coll dbcoll_temp) {
        sortColOut = string.Empty;
        sortDirOut = string.Empty;
        table = string.Empty;
        DataTable dt = new DataTable();
        dbcoll_temp = GetDataBase_Coll(id);
        if (dbcoll_temp != null) {
            if ((!string.IsNullOrEmpty(sortCol)) && (!string.IsNullOrEmpty(sortDir)) && (sortCol != "undefined") && (sortDir != "undefined")) {
                string tableName = dbcoll_temp.TableName;
                int indexOf = dbcoll_temp.SelectCommand.ToLower().IndexOf("order by");
                string command = dbcoll_temp.SelectCommand.Replace(dbcoll_temp.SelectCommand.Substring(indexOf), "");

                command += "ORDER BY " + sortCol + " " + sortDir;

                sortColOut = sortCol;
                sortDirOut = sortDir;

                DBImporter_Coll _tempColl = new DBImporter_Coll(dbcoll_temp.ID, dbcoll_temp.Date, tableName, dbcoll_temp.ConnString, command, dbcoll_temp.Provider, dbcoll_temp.ImportedBy, dbcoll_temp.AllowEdit.ToString(), dbcoll_temp.Chart_Type.ToString(), dbcoll_temp.ChartTitle, String.Join(ServerSettings.StringDelimiter, dbcoll_temp.UsersAllowedToEdit), dbcoll_temp.NotifyUsers.ToString());
                _dbviewer.GetImportedTableData(_tempColl);
                dt = _dbviewer.dt;
            }
            else {
                if (dbcoll_temp.SelectCommand.ToLower().Contains("order by")) {
                    try {
                        int index = dbcoll_temp.SelectCommand.ToLower().IndexOf("order by");
                        if (index != -1) {
                            string[] x = { " " };
                            string[] expressions = dbcoll_temp.SelectCommand.Substring(index).Split(x, StringSplitOptions.RemoveEmptyEntries);
                            if ((expressions[0].ToLower() == "order") && (expressions[1].ToLower() == "by") && (expressions.Length == 4)) {
                                sortCol = expressions[2];
                                sortDir = expressions[3];
                            }
                        }
                    }
                    catch { }
                }

                sortColOut = sortCol;
                sortDirOut = sortDir;

                _dbviewer.GetImportedTableData(dbcoll_temp);
                table = dbcoll_temp.SelectCommand.Substring(dbcoll_temp.SelectCommand.IndexOf(" FROM ") + (" FROM ").Length);
                table = table.Replace(dbcoll_temp.SelectCommand.Substring(dbcoll_temp.SelectCommand.ToLower().IndexOf("order by")), "").Trim();
                dt = _dbviewer.dt;
            }
        }

        return dt;
    }

    private static List<string> GetAllowedColumns(DBImporter_Coll coll) {
        string selectCols = coll.SelectCommand.Substring(coll.SelectCommand.ToLower().IndexOf("select ") + ("select ").Length);
        selectCols = selectCols.Replace(coll.SelectCommand.Substring(coll.SelectCommand.ToLower().IndexOf(" from ")), string.Empty);
        selectCols = selectCols.Trim().ToLower();

        List<string> allowedColumns = new List<string>();
        if (!string.IsNullOrEmpty(selectCols) && selectCols != "*") {
            List<string> tempallowedColumns = selectCols.Split(new string[] { ",", ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string col in tempallowedColumns) {
                allowedColumns.Add(col.Trim());
            }
        }

        return allowedColumns;
    }

    private void AddNotification(DBImporter_Coll tableInfo, string changeMade, string type) {
        if (tableInfo.NotifyUsers) {
            string tableName = tableInfo.TableName;

            MailMessage mailTo = new MailMessage();
            var messagebody = new StringBuilder();
            string userChanged = HelperMethods.MergeFMLNames(new MemberDatabase(_userId.Name));
            string message = "<b>" + userChanged + "</b> has {0} an item {1} the table {2}";
            if (type == "add") {
                message = string.Format(message, "created", "for", tableName + ":<div class='clear-space-two'></div>");
            }
            else if (type == "delete") {
                message = string.Format(message, "deleted", "from", tableName);
            }
            else {
                message = string.Format(message, "edited", "for", tableName + ":<div class='clear-space-two'></div>");
            }

            messagebody.Append(message + changeMade + "<div class='clear-space'></div>");

            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser u in coll) {
                if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower() && u.UserName.ToLower() != _userId.Name.ToLower()) {
                    var un = new UserNotificationMessages(u.UserName);
                    string email = un.attemptAdd("707ecc6c-2480-4080-bad6-fb135bb5cf13", messagebody.ToString(), true);
                    if (!string.IsNullOrEmpty(email))
                        mailTo.To.Add(email);
                }
            }

            UserNotificationMessages.finishAdd(mailTo, "707ecc6c-2480-4080-bad6-fb135bb5cf13", messagebody.ToString());
        }
    }
}