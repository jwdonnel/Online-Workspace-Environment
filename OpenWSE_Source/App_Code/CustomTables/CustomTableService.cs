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
using System.Data.SqlServerCe;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class CustomTableService : System.Web.Services.WebService {
    #region Variables

    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly AppLog _applog = new AppLog(false);
    private CustomTableViewer ctv;
    private IIdentity _userId;
    private string errorMessage = string.Empty;

    #endregion

    public CustomTableService() {
        _userId = HttpContext.Current.User.Identity;
        ctv = new CustomTableViewer(_userId.Name);
    }

    [WebMethod]
    public object[] GetRecords(string id, string search, string recordstopull, string sortCol, string sortDir, string date) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            if (sortCol == "")
                sortCol = "TimeStamp";

            if (sortDir == "")
                sortDir = "DESC";

            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = ctv.GetTableIDByAppID("app-" + id);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName, sortCol, sortDir);
            DataTable dt = dbViewer.dt;
            if (dt != null) {
                BuildRowHeader(dt.Columns, id, ref list, sortCol, sortDir);
                BuildRowItem(dt, id, ref list, count, recordstopull, search, date);
            }
        }
        return list.ToArray();
    }
    private void BuildRowHeader(DataColumnCollection columns, string id, ref List<object> list1, string sortCol, string sortDir) {
        StringBuilder str = new StringBuilder();
        foreach (DataColumn dc in columns) {
            if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
                string active = string.Empty;
                if (sortCol.ToLower() == dc.ColumnName.ToLower()) {
                    if (sortDir.ToLower() == "asc")
                        active = " active asc";
                    else
                        active = " active desc";
                }
                str.Append("<td class=\"td-sort-click" + active + "\" onclick=\"OnSortClick_CustomTables(this,'" + dc.ColumnName + "','" + id + "');\" title=\"Sort by " + dc.ColumnName.Replace("_", " ") + "\">" + dc.ColumnName.Replace("_", " ") + "</td>");
            }
        }

        list1.Add(str.ToString());
    }
    private void BuildRowItem(DataTable dt, string id, ref List<object> list1, int count, string recordstopull, string search, string date) {
        StringBuilder str = new StringBuilder();

        // Add Entry Row
        foreach (DataColumn dc in dt.Columns) {
            if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
                string nullable = string.Empty;
                if (!dc.AllowDBNull)
                    nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                int maxLength = dc.MaxLength;
                if (maxLength > 5000)
                    maxLength = 5000;

                str.Append("<td class=\"border-right border-bottom\" align=\"center\"><span class=\"td-columnName-add\" style=\"display: none!important;\">" + dc.ColumnName + "</span>");
                str.Append("<span class=\"td-columnType-add\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>");
                str.Append("<input type='text' class='textEntry-noWidth' maxlength='" + maxLength + "' onkeyup=\"AddRecordKeyPress_CustomTables(event, '" + id + "');\" style='width: 85%;' />" + nullable + "</td>");
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
                if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
                    DateTime dateTime = new DateTime();
                    DateTime.TryParse(dr["TimeStamp"].ToString(), out dateTime);
                    string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                    if (((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                        && ((string.IsNullOrEmpty(date)) || (currDateSel == date)))
                        canAdd = true;

                    list4.Add(dr[dc.ColumnName].ToString());
                }
            }

            if (canAdd) {
                // list3[0] = column id
                // list3[1] = array row data
                // list3[2] = edit mode
                list3.Add(dr["ColumnID"].ToString());
                list3.Add(list4);
                list3.Add(false);

                list2.Add(list3);
                index++;
            }
        }
        list1.Add(list2);
    }


    [WebMethod]
    public object[] GetSidebar(string id) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            string tableName = ctv.GetTableIDByAppID("app-" + id);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName);
            DataTable dt = dbViewer.dt;
            if (dt != null) {
                List<string> listofDates = new List<string>();
                foreach (DataRow dr in dt.Rows) {
                    string timeStamp = dr["TimeStamp"].ToString();
                    DateTime dateTime = new DateTime();
                    DateTime.TryParse(timeStamp, out dateTime);
                    List<object> obj1 = new List<object>();
                    obj1.Add(dateTime.Month + "_" + dateTime.Year);
                    obj1.Add("<h4 class='font-bold float-left'>" + MonthConverter.ToStringMonth(dateTime.Month) + "</h4><span class='float-right' style='font-size: 12px'>" + dateTime.Year + "</span>");
                    if (!listofDates.Contains((dateTime.Month + "_" + dateTime.Year).ToString())) {
                        list.Add(obj1);
                        listofDates.Add((dateTime.Month + "_" + dateTime.Year).ToString());
                    }
                }
            }
        }
        return list.ToArray();
    }


    [WebMethod]
    public object[] EditRecord(string id, string cid, string search, string recordstopull, string sortCol, string sortDir, string date) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            if (sortCol == "")
                sortCol = "TimeStamp";

            if (sortDir == "")
                sortDir = "DESC";

            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = ctv.GetTableIDByAppID("app-" + id);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName, sortCol, sortDir);
            DataTable dt = dbViewer.dt;
            if (dt != null) {
                BuildRowHeader(dt.Columns, id, ref list, sortCol, sortDir);
                BuildRowItemEdit(dt, id, cid, ref list, count, recordstopull, search, date);
            }
        }
        return list.ToArray();
    }
    private void BuildRowItemEdit(DataTable dt, string id, string cid, ref List<object> list1, int count, string recordstopull, string search, string date) {
        StringBuilder str = new StringBuilder();

        // Add Entry Row
        foreach (DataColumn dc in dt.Columns) {
            if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp"))
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


            if (dr["ColumnID"].ToString() == cid) {
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        string nullable = string.Empty;
                        if (!dc.AllowDBNull)
                            nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                        int maxLength = dc.MaxLength;
                        if (maxLength > 5000)
                            maxLength = 5000;

                        string inputText = "<span class=\"td-columnName-edit\" style=\"display: none!important;\">" + dc.ColumnName + "</span>";
                        inputText += "<span class=\"td-columnType-edit\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>";
                        inputText += "<input type='text' class='textEntry-noWidth' value=\"" + dr[dc.ColumnName].ToString() + "\" maxlength='" + maxLength + "' onkeyup=\"UpdateRecordKeyPress_CustomTables(event, '" + id + "', '" + cid + "');\" style='width: 85%;' />" + nullable;
                        list4.Add(inputText);
                    }
                }


                // list3[0] = column id
                // list3[1] = array row data
                // list3[2] = edit mode
                list3.Add(dr["ColumnID"].ToString());
                list3.Add(list4);
                list3.Add(true);

                list2.Add(list3);
                index++;
            }
            else {
                bool canAdd = false;
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        DateTime dateTime = new DateTime();
                        DateTime.TryParse(dr["TimeStamp"].ToString(), out dateTime);
                        string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                        if (((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                            && ((string.IsNullOrEmpty(date)) || (currDateSel == date)))
                            canAdd = true;

                        list4.Add(dr[dc.ColumnName].ToString());
                    }
                }

                if (canAdd) {
                    // list3[0] = column id
                    // list3[1] = array row data
                    // list3[2] = edit mode
                    list3.Add(dr["ColumnID"].ToString());
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
    public object[] DeleteRecord(string id, string cid) {
        List<object> returnVals = new List<object>();

        if (_userId.IsAuthenticated) {
            string tableName = ctv.GetTableIDByAppID("app-" + id);
            if (DeleteEntry(tableName, cid, out errorMessage)) {
                returnVals.Add("Success");
                returnVals.Add("");
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
    private bool DeleteEntry(string table, string cid, out string errorMessage) {
        errorMessage = string.Empty;
        bool didDelete = true;

        if (!dbCall.CallDelete(table, new List<DatabaseQuery>() { new DatabaseQuery("ColumnID", cid) })) {
            didDelete = false;
            errorMessage = "Error deleting entry! Please try again";
        }

        return didDelete;
    }


    [WebMethod]
    public object[] AddRecord(string id, object recordVals) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("ColumnID", Guid.NewGuid().ToString());
        dic.Add("TimeStamp", DateTime.Now.ToString());
        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }


        string tableName = ctv.GetTableIDByAppID("app-" + id);
        DBViewer dbViewer = new DBViewer(false);
        dbViewer.GetTableData(tableName);
        List<string> newcolumns = BuildColumnList(dbViewer.dt);
        if (AddEntry(tableName, dic, newcolumns, out errorMessage)) {
            returnVals.Add("Success");
            returnVals.Add("");
            return returnVals.ToArray();
        }
        else {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool AddEntry(string table, Dictionary<string, string> dic, List<string> columns, out string errorMessage) {
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
    public object[] UpdateRecord(string id, object recordVals, string cid) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }


        string tableName = ctv.GetTableIDByAppID("app-" + id);
        DBViewer dbViewer = new DBViewer(false);
        dbViewer.GetTableData(tableName);
        List<string> newcolumns = BuildColumnList(dbViewer.dt);
        if (UpdateEntry(tableName, cid, dic, newcolumns, out errorMessage)) {
            returnVals.Add("Success");
            returnVals.Add("");
            return returnVals.ToArray();
        }
        else {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool UpdateEntry(string table, string rowId, Dictionary<string, string> dic, List<string> columns, out string errorMessage) {
        errorMessage = string.Empty;
        bool didUpdate = true;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ColumnID", rowId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        foreach (string column in columns) {
            if ((column.ToLower() != "columnid") && (column.ToLower() != "timestamp")) {
                string val = "";
                dic.TryGetValue(column, out val);
                updateQuery.Add(new DatabaseQuery(column, val));
            }
        }

        if (!dbCall.CallUpdate(table, updateQuery, query)) {
            didUpdate = false;
            errorMessage = "Error updating entry! Please try again";
        }

        return didUpdate;
    }


    [WebMethod]
    public string ExportToExcel(string id, string startDate, string endDate) {
        string tableName = ctv.GetTableNameByAppID("app-" + id);

        string _Path = tableName.Replace(" ", "_") + "-" + startDate.Replace("/", "_") + "-" + endDate.Replace("/", "_") + ".xls";
        string directory = ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        var temp = new DataTable();
        try {
            DBViewer dbViewer = new DBViewer(false);
            tableName = ctv.GetTableIDByAppID("app-" + id);
            dbViewer.GetCustomDataSort(tableName, "TimeStamp", "ASC");
            DataTable dt = dbViewer.dt;

            foreach (DataColumn dc in dt.Columns) {
                if (!temp.Columns.Contains(dc.ColumnName)) {
                    if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp"))
                        temp.Columns.Add(new DataColumn(dc.ColumnName));
                }
            }

            DateTime StartDate = Convert.ToDateTime(startDate);
            DateTime EndDate = Convert.ToDateTime(endDate);

            foreach (DateTime day in EachDay(StartDate, EndDate)) {
                if (dt != null) {
                    if (dt.Rows.Count > 0) {
                        foreach (DataRow dr in dt.Rows) {
                            string timeStamp = dr["TimeStamp"].ToString();
                            DateTime tempStamp = DateTime.Parse(timeStamp);
                            if (day.ToShortDateString() == tempStamp.ToShortDateString()) {
                                DataRow drsch = temp.NewRow();
                                foreach (DataColumn dc in temp.Columns) {
                                    drsch[dc.ColumnName] = dr[dc.ColumnName];
                                }
                                temp.Rows.Add(drsch);
                            }
                        }
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

            return "Apps/Custom_Tables/Exports/" + _Path;
        }
        catch {
            return "File path wrong.";
        }
    }
    public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru) {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }

    private string BuildColumnsForUpdate(DataTable t) {
        var columns = new StringBuilder();
        int count = t.Columns.Count - 1;
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            if ((c.ColumnName.ToLower() != "columnid") && (c.ColumnName.ToLower() != "timestamp")) {
                columns.Append(c.ColumnName + "=@" + c.ColumnName);
                if (i < count) {
                    columns.Append(", ");
                }
            }
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
}