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

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class dbImportservice : System.Web.Services.WebService
{
    #region Variables

    private readonly DBViewer _dbviewer = new DBViewer(true);
    private readonly DatabaseCall dbCall;
    private readonly AppLog _applog = new AppLog(false);
    private IIdentity _userId;
    private string errorMessage = string.Empty;
    private string sortColOut = string.Empty;
    private string sortDirOut = string.Empty;

    #endregion

    public dbImportservice()
    {
        _userId = HttpContext.Current.User.Identity;
        ConnectionStringSettings _connString = ServerSettings.GetRootWebConfig.ConnectionStrings.ConnectionStrings[ServerSettings.DefaultConnectionStringName];
        dbCall = new DatabaseCall(_connString.ProviderName, _connString.ConnectionString);
    }

    [WebMethod]
    public object[] GetRecords(string id, string search, string recordstopull, string sortCol, string sortDir, string allowEdit)
    {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated)
        {
            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, sortCol, sortDir, out tableName, out coll);
            if (dt != null)
            {
                BuildRowHeader(dt.Columns, id, ref list, sortColOut, sortDirOut, allowEdit);
                BuildRowItem(dt, id, ref list, count, recordstopull, search, allowEdit);
            }
        }
        return list.ToArray();
    }
    private void BuildRowHeader(DataColumnCollection columns, string id, ref List<object> list1, string sortCol, string sortDir, string allowEdit)
    {
        StringBuilder str = new StringBuilder();
        foreach (DataColumn dc in columns)
        {
            string active = string.Empty;
            if (sortCol.ToLower() == dc.ColumnName.ToLower())
            {
                if (sortDir.ToLower() == "asc")
                    active = " active asc";
                else
                    active = " active desc";
            }
            str.Append("<td class=\"td-sort-click" + active + "\" onclick=\"OnSortClick_dbImports(this,'" + dc.ColumnName + "','" + id + "'," + allowEdit + ");\" title=\"Sort by " + dc.ColumnName.Replace("_", " ") + "\">" + dc.ColumnName.Replace("_", " ") + "</td>");
        }

        list1.Add(str.ToString());
    }
    private void BuildRowItem(DataTable dt, string id, ref List<object> list1, int count, string recordstopull, string search, string allowEdit)
    {
        StringBuilder str = new StringBuilder();
        if (HelperMethods.ConvertBitToBoolean(allowEdit))
        {
            // Add Entry Row
            foreach (DataColumn dc in dt.Columns)
            {
                string nullable = string.Empty;
                if (!dc.AllowDBNull)
                    nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                string val = string.Empty;
                if (dc.DataType.Name.ToLower() == "guid") {
                    val = Guid.NewGuid().ToString();
                }

                int maxLength = dc.MaxLength;
                if (maxLength > 5000)
                    maxLength = 5000;

                str.Append("<td class=\"border-right border-bottom\" align=\"center\"><span class=\"td-columnName-add\" style=\"display: none!important;\">" + dc.ColumnName + "</span>");
                str.Append("<span class=\"td-columnType-add\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>");
                str.Append("<input type='text' class='textEntry-noWidth' value=\"" + val + "\" maxlength='" + maxLength + "' onkeyup=\"AddRecordKeyPress_dbImports(event, '" + id + "');\" style='width: 85%;' />" + nullable + "</td>");
            }
        }
        list1.Add(str.ToString());


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows)
        {
            if (index >= count + 1)
            {
                if (recordstopull != "all")
                    break;
            }
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();
            bool canAdd = false;

            foreach (DataColumn dc in dt.Columns)
            {
                if ((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                    canAdd = true;

                list4.Add(dr[dc.ColumnName].ToString());
            }

            if (canAdd)
            {
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
    public object[] EditRecord(string id, object rowData, string search, string recordstopull, string sortCol, string sortDir)
    {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated)
        {
            int count = 50;
            int.TryParse(recordstopull, out count);

            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, sortCol, sortDir, out tableName, out coll);
            if (dt != null)
            {
                BuildRowHeader(dt.Columns, id, ref list, sortColOut, sortDirOut, "true");
                BuildRowItemEdit(dt, id, rowData as object[], ref list, count, recordstopull, search);
            }
        }
        return list.ToArray();
    }
    private void BuildRowItemEdit(DataTable dt, string id, object[] rowData, ref List<object> list1, int count, string recordstopull, string search)
    {
        StringBuilder str = new StringBuilder();

        // Add Entry Row
        foreach (DataColumn dc in dt.Columns)
        {
            str.Append("<td align=\"center\" class=\"border-right border-bottom\"></td>");
        }
        list1.Add(str.ToString());


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows)
        {
            if (index >= count + 1)
            {
                if (recordstopull != "all")
                    break;
            }
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();

            bool isRowToEdit = true;
            for (int i = 0; i < rowData.Length; i++)
            {
                string r = rowData[i].ToString().Trim().Replace("\n", "").Replace("\r", "");
                if (dr[i].ToString().Trim() != r)
                {
                    isRowToEdit = false;
                    break;
                }
            }

            if (isRowToEdit)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    string nullable = string.Empty;
                    if (!dc.AllowDBNull)
                        nullable = "<span class=\"not-nullable\" style=\"color: Red; padding: 0 0 0 10px!important;\">*</span>";

                    int maxLength = dc.MaxLength;
                    if (maxLength > 5000)
                        maxLength = 5000;

                    string inputText = "<span class=\"td-columnName-edit\" style=\"display: none!important;\">" + dc.ColumnName + "</span><span class=\"td-columnValue-edit\" style=\"display: none!important;\">" + dr[dc.ColumnName].ToString() + "</span>";
                    inputText += "<span class=\"td-columnType-edit\" style=\"display: none!important;\">" + dc.DataType.Name.ToLower() + "</span>";
                    inputText += "<input type='text' class='textEntry-noWidth' value=\"" + dr[dc.ColumnName].ToString() + "\" maxlength='" + maxLength + "' onkeyup=\"UpdateRecordKeyPress_dbImports(event, '" + id + "', this);\" style='width: 85%;' />" + nullable;

                    list4.Add(inputText);
                }


                // list3[0] = array row data
                // list3[1] = edit mode
                list3.Add(list4);
                list3.Add(true);

                list2.Add(list3);
                index++;
            }
            else
            {
                bool canAdd = false;
                foreach (DataColumn dc in dt.Columns)
                {
                    if ((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                        canAdd = true;

                    list4.Add(dr[dc.ColumnName].ToString());
                }

                if (canAdd)
                {
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
    public object[] DeleteRecord(string id, object rowData)
    {
        List<object> returnVals = new List<object>();

        if (_userId.IsAuthenticated)
        {
            string tableName = string.Empty;
            DBImporter_Coll coll;
            DataTable dt = GetTable(id, "", "", out tableName, out coll);
            if (DeleteEntry(tableName, dt.Columns, rowData as object[], coll, out errorMessage))
            {
                returnVals.Add("Success");
                returnVals.Add("");
                return returnVals.ToArray();
            }
            else
            {
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
    public object[] AddRecord(string id, object recordVals)
    {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals)
        {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }

        string tableName = string.Empty;
        DBImporter_Coll coll;
        DataTable dt = GetTable(id, "", "", out tableName, out coll);
        List<string> newcolumns = BuildColumnList(dt);
        if (AddEntry(tableName, dic, newcolumns, coll, out errorMessage))
        {
            returnVals.Add("Success");
            returnVals.Add("");
            return returnVals.ToArray();
        }
        else
        {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool AddEntry(string table, Dictionary<string, string> dic, List<string> columns, DBImporter_Coll dbcoll, out string errorMessage)
    {
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
    public object[] UpdateRecord(string id, object recordVals, object rowData)
    {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals)
        {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }


        string tableName = string.Empty;
        DBImporter_Coll coll;
        DataTable dt = GetTable(id, "", "", out tableName, out coll);

        List<string> newcolumns = BuildColumnList(dt);
        if (UpdateEntry(tableName, BuildColumnsForUpdate(dt), dt.Columns, rowData as object[], dic, newcolumns, coll, out errorMessage))
        {
            returnVals.Add("Success");
            returnVals.Add("");
            return returnVals.ToArray();
        }
        else
        {
            returnVals.Add("Error");
            returnVals.Add(errorMessage);
            return returnVals.ToArray();
        }
    }
    private bool UpdateEntry(string table, string columnString, DataColumnCollection colList, object[] rowData, Dictionary<string, string> dic, List<string> columns, DBImporter_Coll dbcoll, out string errorMessage)
    {
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
    public string ExportToExcel(string id)
    {
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
        try
        {
            if (dt != null)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    if (!temp.Columns.Contains(dc.ColumnName))
                        temp.Columns.Add(new DataColumn(dc.ColumnName));
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
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
        catch
        {
            return "File path wrong.";
        }
    }


    private string BuildColumns(DataTable t)
    {
        var columns = new StringBuilder();
        columns.Append("VALUES(");
        int count = t.Columns.Count - 1;
        for (int i = 0; i < t.Columns.Count; i++)
        {
            DataColumn c = t.Columns[i];
            columns.Append("@" + c.ColumnName);
            if (i < count)
            {
                columns.Append(", ");
            }
        }
        columns.Append(")");

        return columns.ToString();
    }
    private string BuildColumnsForUpdate(DataTable t)
    {
        var columns = new StringBuilder();
        int count = t.Columns.Count - 1;
        for (int i = 0; i < t.Columns.Count; i++)
        {
            DataColumn c = t.Columns[i];
            columns.Append(c.ColumnName + "=@" + c.ColumnName);
            if (i < count)
                columns.Append(" AND ");
        }

        return columns.ToString();
    }
    private List<string> BuildColumnList(DataTable t)
    {
        var list = new List<string>();
        for (int i = 0; i < t.Columns.Count; i++)
        {
            DataColumn c = t.Columns[i];
            list.Add(c.ColumnName);
        }

        return list;
    }

    private DBImporter_Coll GetDataBase_Coll(string id)
    {
        DBImporter_Coll dbcoll_temp = null;
        var db = new DBImporter();
        db.BinaryDeserialize();
        foreach (var dbcoll in db.DBColl)
        {
            if (dbcoll.ID == id)
            {
                dbcoll_temp = dbcoll;
                break;
            }
        }

        return dbcoll_temp;
    }
    private DataTable GetTable(string id, string sortCol, string sortDir, out string table, out DBImporter_Coll dbcoll_temp)
    {
        sortColOut = string.Empty;
        sortDirOut = string.Empty;
        table = string.Empty;
        DataTable dt = new DataTable();
        dbcoll_temp = GetDataBase_Coll(id);
        if (dbcoll_temp != null)
        {
            if ((!string.IsNullOrEmpty(sortCol)) && (!string.IsNullOrEmpty(sortDir)) && (sortCol != "undefined") && (sortDir != "undefined"))
            {
                string tableName = dbcoll_temp.TableName;
                int indexOf = dbcoll_temp.SelectCommand.ToLower().IndexOf("order by");
                string command = dbcoll_temp.SelectCommand.Replace(dbcoll_temp.SelectCommand.Substring(indexOf), "");

                command += "ORDER BY " + sortCol + " " + sortDir;

                sortColOut = sortCol;
                sortDirOut = sortDir;
                    
                DBImporter_Coll _tempColl = new DBImporter_Coll(dbcoll_temp.ID, dbcoll_temp.Date, tableName, dbcoll_temp.ConnString, command, dbcoll_temp.Provider, dbcoll_temp.ImportedBy, dbcoll_temp.AllowEdit.ToString());
                _dbviewer.GetImportedTableData(_tempColl);
                dt = _dbviewer.dt;
            }
            else
            {
                if (dbcoll_temp.SelectCommand.ToLower().Contains("order by"))
                {
                    try
                    {
                        int index = dbcoll_temp.SelectCommand.ToLower().IndexOf("order by");
                        if (index != -1)
                        {
                            string[] x = { " " };
                            string[] expressions = dbcoll_temp.SelectCommand.Substring(index).Split(x, StringSplitOptions.RemoveEmptyEntries);
                            if ((expressions[0].ToLower() == "order") && (expressions[1].ToLower() == "by") && (expressions.Length == 4))
                            {
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
}