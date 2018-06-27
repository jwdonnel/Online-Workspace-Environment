using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Security.Principal;
using System.Text;
using System.Web.Configuration;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Notifications;
using System.Web.Security;
using System.Net.Mail;
using System.Data;

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
        GetSiteRequests.AddRequest();

        _userId = HttpContext.Current.User.Identity;
        ctv = new CustomTableViewer(_userId.Name);
    }

    [WebMethod]
    public object[] GetRecords(string id, string search, string recordstopull, string sortCol, string sortDir, string date, string monthSelector) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            if (string.IsNullOrEmpty(sortCol) || sortCol == "undefined") {
                sortCol = "TimeStamp";
            }

            if (string.IsNullOrEmpty(sortDir) || sortDir == "undefined") {
                sortDir = "DESC";
            }

            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);

            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableInfo.TableID, sortCol, sortDir, tableInfo.ColumnData, recordstopull);
            DataTable dt = dbViewer.dt;

            date = GetMostCurrentDate(date, monthSelector, dt);

            List<object> list1 = new List<object>();
            if (dt != null) {
                BuildRowHeader(dt.Columns, id, ref list1, sortCol, sortDir, tableInfo);
                BuildRowItem(dt, id, ref list1, search, date, tableInfo, monthSelector);
            }
            list.Add(list1);

            if (tableInfo.UsersAllowedToEdit != null) {
                string userAllowed = tableInfo.UsersAllowedToEdit.Find(x => x.ToLower() == _userId.Name.ToLower());
                if (!string.IsNullOrEmpty(userAllowed)) {
                    list.Add("true");
                }
                else {
                    list.Add("false");
                }
            }

            DataChartsOpened dco = new DataChartsOpened(_userId.Name);
            list.Add(dco.IsChartOpenedForUser("app-" + id).ToString().ToLower());
        }
        return list.ToArray();
    }
    private void BuildRowHeader(DataColumnCollection columns, string id, ref List<object> list1, string sortCol, string sortDir, CustomTable_Coll tableInfo) {
        List<object> headerObj = new List<object>();
        foreach (DataColumn dc in columns) {
            if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                string active = string.Empty;
                if (sortCol.ToLower() == dc.ColumnName.ToLower()) {
                    if (sortDir.ToLower() == "asc")
                        active = " active asc";
                    else
                        active = " active desc";
                }

                string nullable = "true";
                string shownName = dc.ColumnName;
                CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                    if (!columnData.nullable) {
                        nullable = "false";
                    }

                    shownName = columnData.shownName;
                }

                List<string> strObj = new List<string>();
                strObj.Add(active);
                strObj.Add(dc.ColumnName);
                strObj.Add(id);
                strObj.Add(nullable);
                strObj.Add(shownName);
                headerObj.Add(strObj);
            }
        }

        list1.Add(headerObj);
    }
    private void BuildRowItem(DataTable dt, string id, ref List<object> list1, string search, string date, CustomTable_Coll tableInfo, string monthSelector) {
        // Add Entry Row
        List<object> rowAddObj = new List<object>();
        foreach (DataColumn dc in dt.Columns) {
            if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                string nullable = string.Empty;
                string dataType = "string";
                int maxLength = 100;
                string shownName = dc.ColumnName;

                CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                    if (!columnData.nullable) {
                        nullable = "<span class=\"not-nullable\">*</span>";
                    }
                    dataType = columnData.dataType.ToLower();
                    maxLength = columnData.dataLength;
                    shownName = columnData.shownName;
                }

                List<string> strObj = new List<string>();
                strObj.Add(dc.ColumnName);
                strObj.Add(maxLength.ToString());
                strObj.Add(id);
                strObj.Add(dataType);
                strObj.Add(nullable);
                strObj.Add(shownName);
                rowAddObj.Add(strObj);
            }
        }
        list1.Add(rowAddObj);


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows) {
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();
            bool canAdd = false;
            foreach (DataColumn dc in dt.Columns) {
                if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                    DateTime dateTime = new DateTime();

                    try {
                        string timeStamp = dr[monthSelector].ToString();
                        if (string.IsNullOrEmpty(timeStamp)) {
                            timeStamp = DateTime.Now.ToString();
                        }

                        DateTime.TryParse(timeStamp, out dateTime);
                        string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                        if (((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                            && ((string.IsNullOrEmpty(date)) || (currDateSel == date)))
                            canAdd = true;

                        string dataType = "string";
                        CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                        if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                            dataType = columnData.dataType.ToLower();
                        }

                        List<object> list5 = new List<object>();
                        list5.Add(dr[dc.ColumnName].ToString());
                        list5.Add(dataType);
                        list4.Add(list5);
                    }
                    catch {
                        // Do Nothing
                    }
                }
            }

            if (canAdd) {
                // list3[0] = column id
                // list3[1] = array row data
                // list3[2] = edit mode
                list3.Add(dr["EntryID"].ToString());
                list3.Add(list4);
                list3.Add(false);

                list2.Add(list3);
                index++;
            }
        }
        list1.Add(list2);
    }


    [WebMethod]
    public object[] GetSidebar(string id, string monthSelector) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            string tableName = ctv.GetTableIDByAppID("app-" + id);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName, string.Empty, string.Empty, null, string.Empty);
            DataTable dt = dbViewer.dt;
            if (dt != null) {
                List<string> listofDates = new List<string>();
                foreach (DataRow dr in dt.Rows) {
                    try {
                        string timeStamp = dr[monthSelector].ToString();
                        if (string.IsNullOrEmpty(timeStamp)) {
                            timeStamp = DateTime.Now.ToString();
                        }

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
                    catch {
                        // Do nothing
                    }
                }
            }
        }

        return list.ToArray();
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
    public object[] EditRecord(string id, string cid, string search, string recordstopull, string sortCol, string sortDir, string date, string monthSelector) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            if (sortCol == "") {
                sortCol = "TimeStamp";
            }

            if (sortDir == "") {
                sortDir = "DESC";
            }

            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);
            string tableName = tableInfo.TableID;

            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName, sortCol, sortDir, tableInfo.ColumnData, recordstopull);
            DataTable dt = dbViewer.dt;

            date = GetMostCurrentDate(date, monthSelector, dt);

            if (dt != null) {
                BuildRowHeader(dt.Columns, id, ref list, sortCol, sortDir, tableInfo);
                BuildRowItemEdit(dt, id, cid, ref list, search, date, tableInfo, monthSelector);
            }
        }
        return list.ToArray();
    }
    private void BuildRowItemEdit(DataTable dt, string id, string cid, ref List<object> list1, string search, string date, CustomTable_Coll tableInfo, string monthSelector) {
        // Add Entry Row
        List<string> rowAddObj = new List<string>();
        foreach (DataColumn dc in dt.Columns) {
            if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                rowAddObj.Add(string.Empty);
            }
        }
        list1.Add(rowAddObj);


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows) {
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();


            if (dr["EntryID"].ToString() == cid) {
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        int maxLength = 100;
                        string dataType = "string";
                        string nullable = string.Empty;

                        CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                        if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                            maxLength = columnData.dataLength;
                            dataType = columnData.dataType.ToLower();
                            if (!columnData.nullable && dataType != "boolean") {
                                nullable = "<span class=\"not-nullable\">*</span>";
                            }
                        }

                        string columnValue = dr[dc.ColumnName].ToString();
                        if (dataType == "date") {
                            DateTime tempDateTime = new DateTime();
                            if (DateTime.TryParse(dr[dc.ColumnName].ToString(), out tempDateTime)) {
                                columnValue = tempDateTime.ToShortDateString();
                            }
                        }
                        else if (dataType == "money") {
                            decimal tempOutDecimal = 0.0m;
                            if (decimal.TryParse(dr[dc.ColumnName].ToString(), out tempOutDecimal)) {
                                columnValue = tempOutDecimal.ToString("N2");
                            }
                        }

                        string inputText = "<span class=\"td-columnName-edit\" style=\"display: none!important;\">" + dc.ColumnName + "</span>";
                        inputText += "<span class=\"td-columnValue-edit\" style=\"display: none!important;\">" + columnValue + "</span>";

                        string inputType = "text";
                        if (dataType == "integer" || dataType == "decimal" || dataType == "money") {
                            inputType = "number";
                        }
                        else if (dataType == "boolean") {
                            inputType = "checkbox";
                        }

                        inputText += "<div class='input-customtable-holder'><input type='" + inputType + "' class='textEntry-noWidth' maxlength='" + maxLength + "' onkeyup=\"customTables.UpdateRecordKeyPress(event, '" + id + "', '" + cid + "');\" data-type='" + dataType + "' style='width: 100%;' />" + nullable + "</div>";

                        List<object> list5 = new List<object>();
                        list5.Add(inputText);
                        list5.Add(dataType);
                        list5.Add(columnValue);
                        list4.Add(list5);
                    }
                }


                // list3[0] = column id
                // list3[1] = array row data
                // list3[2] = edit mode
                list3.Add(dr["EntryID"].ToString());
                list3.Add(list4);
                list3.Add(true);

                list2.Add(list3);
                index++;
            }
            else {
                bool canAdd = false;
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        DateTime dateTime = new DateTime();
                        DateTime.TryParse(dr["TimeStamp"].ToString(), out dateTime);
                        string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                        if (!string.IsNullOrEmpty(monthSelector)) {
                            string timeStamp = dr[monthSelector].ToString();
                            if (string.IsNullOrEmpty(timeStamp)) {
                                timeStamp = DateTime.Now.ToString();
                            }

                            DateTime.TryParse(timeStamp, out dateTime);
                            currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();
                        }
                        
                        if (((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                            && ((string.IsNullOrEmpty(date)) || (currDateSel == date)))
                            canAdd = true;

                        string dataType = "string";
                        CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                        if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                            dataType = columnData.dataType.ToLower();
                        }

                        List<object> list5 = new List<object>();
                        list5.Add(dr[dc.ColumnName].ToString());
                        list5.Add(dataType);
                        list4.Add(list5);
                    }
                }

                if (canAdd) {
                    // list3[0] = column id
                    // list3[1] = array row data
                    // list3[2] = edit mode
                    list3.Add(dr["EntryID"].ToString());
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
    public object[] EditRecordColumn(string id, string cid, string search, string recordstopull, string sortCol, string sortDir, string date, string columnName, string monthSelector) {
        List<object> list = new List<object>();
        if (_userId.IsAuthenticated) {
            if (sortCol == "") {
                sortCol = "TimeStamp";
            }

            if (sortDir == "") {
                sortDir = "DESC";
            }

            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);
            string tableName = tableInfo.TableID;

            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetCustomDataSort(tableName, sortCol, sortDir, tableInfo.ColumnData, recordstopull);
            DataTable dt = dbViewer.dt;

            date = GetMostCurrentDate(date, monthSelector, dt);

            if (dt != null) {
                BuildRowHeader(dt.Columns, id, ref list, sortCol, sortDir, tableInfo);
                BuildRowItemColumnEdit(dt, id, cid, ref list, search, date, tableInfo, columnName, monthSelector);
            }
        }
        return list.ToArray();
    }
    private void BuildRowItemColumnEdit(DataTable dt, string id, string cid, ref List<object> list1, string search, string date, CustomTable_Coll tableInfo, string columnName, string monthSelector) {
        // Add Entry Row
        List<string> rowAddObj = new List<string>();
        foreach (DataColumn dc in dt.Columns) {
            if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                rowAddObj.Add(string.Empty);
            }
        }
        list1.Add(rowAddObj);


        int index = 1;
        List<object> list2 = new List<object>();
        foreach (DataRow dr in dt.Rows) {
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();


            if (dr["EntryID"].ToString() == cid) {
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        if (dc.ColumnName.ToLower() == columnName.ToLower()) {
                            int maxLength = 100;
                            string dataType = "string";
                            string nullable = string.Empty;

                            CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                            if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                                maxLength = columnData.dataLength;
                                dataType = columnData.dataType.ToLower();
                                if (!columnData.nullable && dataType != "boolean") {
                                    nullable = "<span class=\"not-nullable\">*</span>";
                                }
                            }

                            string columnValue = dr[dc.ColumnName].ToString();
                            if (dataType == "date") {
                                DateTime tempDateTime = new DateTime();
                                if (DateTime.TryParse(dr[dc.ColumnName].ToString(), out tempDateTime)) {
                                    columnValue = tempDateTime.ToShortDateString();
                                }
                            }
                            else if (dataType == "money") {
                                decimal tempOutDecimal = 0.0m;
                                if (decimal.TryParse(dr[dc.ColumnName].ToString(), out tempOutDecimal)) {
                                    columnValue = tempOutDecimal.ToString("N2");
                                }
                            }

                            string inputText = "<span class=\"td-columnName-edit\" style=\"display: none!important;\">" + dc.ColumnName + "</span>";
                            inputText += "<span class=\"td-columnValue-edit\" style=\"display: none!important;\">" + columnValue + "</span>";

                            string inputType = "text";
                            if (dataType == "integer" || dataType == "decimal" || dataType == "money") {
                                inputType = "number";
                            }
                            else if (dataType == "boolean") {
                                inputType = "checkbox";
                            }

                            inputText += "<div class='input-customtable-holder'><input type='" + inputType + "' class='textEntry-noWidth' maxlength='" + maxLength + "' onkeyup=\"customTables.UpdateRecordKeyPress(event, '" + id + "', '" + cid + "');\" onblur=\"customTables.UpdateRecord('" + id + "', '" + cid + "');\" data-type='" + dataType + "' style='width: 100%;' />" + nullable + "</div>";

                            List<object> list5 = new List<object>();
                            list5.Add(inputText);
                            list5.Add(dataType);
                            list5.Add(columnValue);
                            list4.Add(list5);
                        }
                        else {
                            DateTime dateTime = new DateTime();
                            DateTime.TryParse(dr["TimeStamp"].ToString(), out dateTime);
                            string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                            string dataType = "string";
                            CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                            if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                                dataType = columnData.dataType.ToLower();
                            }

                            List<object> list5 = new List<object>();
                            list5.Add(dr[dc.ColumnName].ToString());
                            list5.Add(dataType);
                            list4.Add(list5);
                        }
                    }
                }


                // list3[0] = column id
                // list3[1] = array row data
                // list3[2] = edit mode
                list3.Add(dr["EntryID"].ToString());
                list3.Add(list4);
                list3.Add(true);

                list2.Add(list3);
                index++;
            }
            else {
                bool canAdd = false;
                foreach (DataColumn dc in dt.Columns) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                        DateTime dateTime = new DateTime();
                        DateTime.TryParse(dr["TimeStamp"].ToString(), out dateTime);
                        string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();

                        if (!string.IsNullOrEmpty(monthSelector)) {
                            string timeStamp = dr[monthSelector].ToString();
                            if (string.IsNullOrEmpty(timeStamp)) {
                                timeStamp = DateTime.Now.ToString();
                            }

                            DateTime.TryParse(timeStamp, out dateTime);
                            currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();
                        }

                        if (((string.IsNullOrEmpty(search)) || (search.ToLower() == "search this table") || (dr[dc.ColumnName].ToString().ToLower().Contains(search.ToLower())))
                            && ((string.IsNullOrEmpty(date)) || (currDateSel == date)))
                            canAdd = true;

                        string dataType = "string";
                        CustomTableColumnData columnData = tableInfo.ColumnData.Find(x => x.realName == dc.ColumnName);
                        if (columnData != null && !string.IsNullOrEmpty(columnData.realName)) {
                            dataType = columnData.dataType.ToLower();
                        }

                        List<object> list5 = new List<object>();
                        list5.Add(dr[dc.ColumnName].ToString());
                        list5.Add(dataType);
                        list4.Add(list5);
                    }
                }

                if (canAdd) {
                    // list3[0] = column id
                    // list3[1] = array row data
                    // list3[2] = edit mode
                    list3.Add(dr["EntryID"].ToString());
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
            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);

            if (CanUserMakeChanges(tableInfo)) {
                string tableName = tableInfo.TableID;
                if (DeleteEntry(tableName, cid, out errorMessage)) {
                    returnVals.Add("Success");
                    returnVals.Add("");
                    AddNotification(tableInfo, string.Empty, "delete");
                    return returnVals.ToArray();
                }
            }
            else {
                errorMessage = "User not allowed to make changes";
            }
        }

        returnVals.Add("Error");
        returnVals.Add(errorMessage);
        return returnVals.ToArray();
    }
    private bool DeleteEntry(string table, string cid, out string errorMessage) {
        errorMessage = string.Empty;
        bool didDelete = true;

        if (!dbCall.CallDelete(table, new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID), new DatabaseQuery("EntryID", cid) })) {
            didDelete = false;
            errorMessage = "Error deleting entry! Please try again";
        }

        return didDelete;
    }

    [WebMethod]
    public object[] DeleteMonth(string id, string selectedDate, string monthSelector) {
        List<object> returnVals = new List<object>();
        if (_userId.IsAuthenticated && !string.IsNullOrEmpty(selectedDate) && !string.IsNullOrEmpty(monthSelector)) {
            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);

            if (CanUserMakeChanges(tableInfo)) {
                DBViewer dbViewer = new DBViewer(false);
                dbViewer.GetCustomDataSort(tableInfo.TableID, "TimeStamp", "DESC", tableInfo.ColumnData, "all");
                DataTable dt = dbViewer.dt;
                List<object> list1 = new List<object>();
                if (dt != null) {
                    foreach (DataRow dr in dt.Rows) {
                        try {
                            DateTime dateTime = new DateTime();
                            string timeStamp = dr[monthSelector].ToString();
                            DateTime.TryParse(timeStamp, out dateTime);
                            string currDateSel = (dateTime.Month + "_" + dateTime.Year).ToString();
                            if (currDateSel == selectedDate) {
                                if (DeleteEntry(tableInfo.TableID, dr["EntryID"].ToString(), out errorMessage)) {
                                    AddNotification(tableInfo, string.Empty, "delete");
                                }
                                else {
                                    returnVals.Add("Error");
                                    returnVals.Add(errorMessage);
                                    return returnVals.ToArray();
                                }
                            }
                        }
                        catch (Exception e) {
                            AppLog.AddError(e);
                        }
                    }
                }
            }
            else {
                returnVals.Add("Error");
                returnVals.Add("User not allowed to make changes");
                return returnVals.ToArray();
            }
        }

        returnVals.Add("Success");
        returnVals.Add("");
        return returnVals.ToArray();
    }

    [WebMethod]
    public object[] AddRecord(string id, object recordVals) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        string changeMade = CustomTableViewer.BuildChangeText(vals);

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID);
        dic.Add("EntryID", Guid.NewGuid().ToString());
        dic.Add("TimeStamp", ServerSettings.ServerDateTime.ToString());
        foreach (object obj in vals) {
            object[] objArray = obj as object[];
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }

        CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);
        if (CanUserMakeChanges(tableInfo)) {
            string tableName = tableInfo.TableID;

            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetTableData(tableName);
            List<string> newcolumns = BuildColumnList(dbViewer.dt, dic);
            if (AddEntry(tableName, dic, newcolumns, out errorMessage)) {
                returnVals.Add("Success");
                returnVals.Add("");
                AddNotification(tableInfo, changeMade, "add");
                return returnVals.ToArray();
            }
        }
        else {
            errorMessage = "User not allowed to make changes";
        }

        returnVals.Add("Error");
        returnVals.Add(errorMessage);
        return returnVals.ToArray();
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
    public object[] CopyData(string id, object recordVals) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        if (_userId.IsAuthenticated) {
            CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);

            if (CanUserMakeChanges(tableInfo)) {
                string tableName = tableInfo.TableID;
                DBViewer dbViewer = new DBViewer(false);

                try {
                    foreach (object val in vals) {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID);
                        dic.Add("EntryID", Guid.NewGuid().ToString());
                        dic.Add("TimeStamp", ServerSettings.ServerDateTime.ToString());

                        object[] rowVals = val as object[];
                        foreach (object obj in rowVals) {
                            object[] objArray = obj as object[];
                            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
                        }

                        string changeMade = CustomTableViewer.BuildChangeText(rowVals);

                        dbViewer.GetTableData(tableName);
                        List<string> newcolumns = BuildColumnList(dbViewer.dt, dic);
                        if (AddEntry(tableName, dic, newcolumns, out errorMessage)) {
                            AddNotification(tableInfo, changeMade, "add");
                        }
                        else {
                            returnVals.Add("Error");
                            returnVals.Add(errorMessage);
                            return returnVals.ToArray();
                        }
                    }
                }
                catch (Exception e) {
                    returnVals.Add("Error");
                    returnVals.Add(e.Message);
                    return returnVals.ToArray();
                }
            }
            else {
                returnVals.Add("Error");
                returnVals.Add("User not allowed to make changes");
                return returnVals.ToArray();
            }
        }

        returnVals.Add("Success");
        returnVals.Add("");
        return returnVals.ToArray();
    }

    [WebMethod]
    public object[] UpdateRecord(string id, object recordVals, string cid) {
        List<object> returnVals = new List<object>();
        object[] vals = recordVals as object[];

        string changeMade = CustomTableViewer.BuildChangeText(vals);

        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (object obj in vals) {
            if (obj == null) {
                continue;
            }

            object[] objArray = obj as object[];
            if (string.IsNullOrEmpty(objArray[0].ToString().Trim())) {
                continue;
            }
            dic.Add(objArray[0].ToString().Trim(), objArray[1].ToString().Trim());
        }

        CustomTable_Coll tableInfo = ctv.GetTableInfoByAppId("app-" + id);
        if (CanUserMakeChanges(tableInfo)) {
            string tableName = tableInfo.TableID;

            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetTableData(tableName);
            List<string> newcolumns = BuildColumnList(dbViewer.dt, dic);
            if (UpdateEntry(tableName, cid, dic, newcolumns, out errorMessage)) {
                returnVals.Add("Success");
                returnVals.Add("");
                AddNotification(tableInfo, changeMade, "update");
                return returnVals.ToArray();
            }
        }
        else {
            errorMessage = "User not allowed to make changes";
        }

        returnVals.Add("Error");
        returnVals.Add(errorMessage);
        return returnVals.ToArray();
    }
    private bool UpdateEntry(string table, string rowId, Dictionary<string, string> dic, List<string> columns, out string errorMessage) {
        errorMessage = string.Empty;
        bool didUpdate = true;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("EntryID", rowId));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        foreach (string column in columns) {
            if ((column.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (column.ToLower() != "entryid") && (column.ToLower() != "timestamp")) {
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
    public string ExportToExcel(string id, string startDate, string endDate, string monthSelector) {
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
            dbViewer.GetCustomDataSort(tableName, "TimeStamp", "ASC", null, string.Empty);
            DataTable dt = dbViewer.dt;

            foreach (DataColumn dc in dt.Columns) {
                if (!temp.Columns.Contains(dc.ColumnName)) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp"))
                        temp.Columns.Add(new DataColumn(dc.ColumnName));
                }
            }

            DateTime StartDate = Convert.ToDateTime(startDate);
            DateTime EndDate = Convert.ToDateTime(endDate);

            foreach (DateTime day in EachDay(StartDate, EndDate)) {
                if (dt != null) {
                    if (dt.Rows.Count > 0) {
                        foreach (DataRow dr in dt.Rows) {
                            try {
                                string timeStamp = dr[monthSelector].ToString();
                                DateTime tempStamp = DateTime.Parse(timeStamp);
                                if (day.ToShortDateString() == tempStamp.ToShortDateString()) {
                                    DataRow drsch = temp.NewRow();
                                    foreach (DataColumn dc in temp.Columns) {
                                        drsch[dc.ColumnName] = dr[dc.ColumnName];
                                    }
                                    temp.Rows.Add(drsch);
                                }
                            }
                            catch { }
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

    [WebMethod]
    public string ExportToExcelAll(string id) {
        string tableName = ctv.GetTableNameByAppID("app-" + id);

        string _Path = tableName.Replace(" ", "_") + "-" + HelperMethods.GetTimestamp() + ".xls";
        string directory = ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        var temp = new DataTable();
        try {
            DBViewer dbViewer = new DBViewer(false);
            tableName = ctv.GetTableIDByAppID("app-" + id);
            dbViewer.GetCustomDataSort(tableName, "TimeStamp", "ASC", null, string.Empty);
            DataTable dt = dbViewer.dt;

            foreach (DataColumn dc in dt.Columns) {
                if (!temp.Columns.Contains(dc.ColumnName)) {
                    if ((dc.ColumnName.ToLower() != DatabaseCall.ApplicationIdString.ToLower()) && (dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp"))
                        temp.Columns.Add(new DataColumn(dc.ColumnName));
                }
            }

            if (dt != null) {
                if (dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        DataRow drsch = temp.NewRow();
                        foreach (DataColumn dc in temp.Columns) {
                            drsch[dc.ColumnName] = dr[dc.ColumnName];
                        }
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

            return "Apps/Custom_Tables/Exports/" + _Path;
        }
        catch {
            return "File path wrong.";
        }
    }

    private List<string> BuildColumnList(DataTable t, Dictionary<string, string> vals) {
        var list = new List<string>();
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];

            bool canAddColumn = false;
            foreach (KeyValuePair<string, string> obj in vals) {
                if (obj.Key == c.ColumnName) {
                    canAddColumn = true;
                    break;
                }
            }

            if (canAddColumn) {
                list.Add(c.ColumnName);
            }
        }

        return list;
    }

    private void AddNotification(CustomTable_Coll tableInfo, string changeMade, string type) {
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
                    MemberDatabase tempMember = new MemberDatabase(u.UserName);
                    var un = new UserNotificationMessages(u.UserName);
                    string email = un.attemptAdd(tableInfo.AppID, messagebody.ToString(), true);
                    if (!string.IsNullOrEmpty(email))
                        mailTo.To.Add(email);
                }
            }

            UserNotificationMessages.finishAdd(mailTo, tableInfo.AppID, messagebody.ToString());
        }
    }

    private bool CanUserMakeChanges(CustomTable_Coll tableInfo) {
        string userAllowed = tableInfo.UsersAllowedToEdit.Find(x => x.ToLower() == _userId.Name.ToLower());
        if (!string.IsNullOrEmpty(userAllowed)) {
            return true;
        }

        return false;
    }

    private string GetMostCurrentDate(string date, string monthSelector, DataTable dt) {
        if (string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(monthSelector) && monthSelector != "TimeStamp") {
            if (dt != null) {
                List<long> listofDates = new List<long>();
                foreach (DataRow dr in dt.Rows) {
                    try {
                        string timeStamp = dr[monthSelector].ToString();
                        if (string.IsNullOrEmpty(timeStamp)) {
                            timeStamp = DateTime.Now.ToString();
                        }

                        DateTime dateTime = new DateTime();
                        DateTime.TryParse(timeStamp, out dateTime);
                        if (!listofDates.Contains(dateTime.Ticks)) {
                            listofDates.Add(dateTime.Ticks);
                        }
                    }
                    catch {
                        // Do nothing
                    }
                }

                if (listofDates.Count > 0) {
                    try {
                        listofDates.Sort();
                        listofDates.Reverse();
                        long mostCurrentDateNum = listofDates[0];
                        DateTime newDate = new DateTime(listofDates[0]);
                        return (newDate.Month + "_" + newDate.Year).ToString();
                    }
                    catch (Exception e) {
                        AppLog.AddError(e);
                    }
                }
            }
        }

        if (date.ToLower() == "all") {
            date = string.Empty;
        }

        return date;
    }
}
