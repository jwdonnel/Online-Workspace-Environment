using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Principal;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Text;
using System.IO;
using System.Data.SqlServerCe;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for CustomTableCreator
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class CustomTableCreator : System.Web.Services.WebService {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly AppLog _applog = new AppLog(false);
    private CustomTableAppCreator ctwc;
    private bool _canContinue = false;
    private string _userName;

    public CustomTableCreator() {
        IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            _canContinue = true;
            _userName = userID.Name;
            ctwc = new CustomTableAppCreator(_userName);
        }
    }

    [WebMethod]
    public string GetFileUploadContents() {
        if (HttpContext.Current.Request.Files.Count == 1) {
            HttpPostedFile postedFile = HttpContext.Current.Request.Files[0];
            FileInfo fi = new FileInfo(postedFile.FileName);
            string fileExtension = fi.Extension.ToLower();
            if (fileExtension != ".xls" && fileExtension != ".xlsx") {
                return "false";
            }

            string tempFile = ServerSettings.GetServerMapLocation + "Apps/Custom_Tables/Exports/" + DateTime.Now.Ticks.ToString() + fileExtension;

            try {
                postedFile.SaveAs(tempFile);
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            try {
                if (File.Exists(tempFile)) {
                    File.Delete(tempFile);
                }
            }
            catch { }

            return "true";
        }

        return "false";
    }

    [WebMethod]
    public string Create(object columns, string tableName, string installForUser, string showSidebar, string notifyUsers, string isPrivate, string chartType, string chartTitle, string usersAllowed) {
        if (_canContinue) {
            string tableID = "CT_" + HelperMethods.RandomString(10);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetTableData(tableID, false);
            if (dbViewer.dt != null) {
                if ((dbViewer.dt.Columns.Count > 0) || (dbViewer.dt.Rows.Count > 0))
                    return "false";
            }

            string columnList = CreateColumnList(columns as object[]);
            if (!string.IsNullOrEmpty(columnList)) {
                string tempTableName = tableID;
                if (CreateCustomTable(tempTableName, columnList)) {
                    string categoryId = Guid.NewGuid().ToString();
                    bool categoryFound = false;
                    AppCategory category = new AppCategory(true);
                    foreach (Dictionary<string, string> dr in category.category_dt) {
                        if (dr["Category"].ToLower() == "custom tables") {
                            categoryId = dr["ID"];
                            categoryFound = true;
                            break;
                        }
                    }
                    if (!categoryFound)
                        category.addItem("Custom Tables", categoryId);

                    bool _isPrivate = HelperMethods.ConvertBitToBoolean(isPrivate);
                    if (!HelperMethods.ConvertBitToBoolean(installForUser)) {
                        _isPrivate = false;
                    }

                    string fileName = ctwc.CreateApp(HelperMethods.ConvertBitToBoolean(showSidebar), tableName, categoryId, "", "database.png", true, _isPrivate, chartType);
                    CustomTableViewer ctv = new CustomTableViewer(_userName);
                    ctv.AddItem(tableName, _userName, tableID, "app-" + fileName, DateTime.Now.ToString(), HelperMethods.ConvertBitToBoolean(showSidebar), HelperMethods.ConvertBitToBoolean(notifyUsers), chartType, chartTitle, usersAllowed);

                    if (HelperMethods.ConvertBitToBoolean(installForUser)) {
                        var member = new MemberDatabase(_userName);
                        member.UpdateEnabledApps("app-" + fileName);
                    }

                    return "";
                }
            }
        }

        return "false";
    }

    [WebMethod]
    public string RecreateApp(string appId, string tableName, string showSidebar, string chartType) {
        if (_canContinue) {
            try {
                if (chartType.ToLower() == "none") {
                    chartType = string.Empty;
                }

                string serverpath = ServerSettings.GetServerMapLocation;
                var fi = new FileInfo(serverpath + "Apps\\Custom_Tables\\" + appId + ".ascx");
                if (fi.Exists) {
                    try {
                        App w = new App();
                        w.DeleteAppComplete("app-" + appId, ServerSettings.GetServerMapLocation);
                    }
                    catch { }
                }

                string categoryId = Guid.NewGuid().ToString();
                bool categoryFound = false;
                AppCategory category = new AppCategory(true);
                foreach (Dictionary<string, string> dr in category.category_dt) {
                    if (dr["Category"].ToLower() == "custom tables") {
                        categoryId = dr["ID"];
                        categoryFound = true;
                        break;
                    }
                }
                if (!categoryFound)
                    category.addItem("Custom Tables", categoryId);

                bool insertIntoAppList = true;
                App apps = new App();
                if (!string.IsNullOrEmpty(apps.GetAppInformation("app-" + appId).ID)) {
                    insertIntoAppList = false;
                }

                ctwc.CreateApp(HelperMethods.ConvertBitToBoolean(showSidebar), tableName, categoryId, "", "database.png", insertIntoAppList, false, chartType, appId);
                CustomTableViewer ctv = new CustomTableViewer(_userName);

                string id = "";
                ctv.BuildEntriesAll();
                foreach (CustomTable_Coll coll in ctv.CustomTableList) {
                    if (coll.AppID == "app-" + appId) {
                        id = coll.ID;
                        break;
                    }
                }

                return string.Empty;
            }
            catch { }
        }

        return "false";
    }

    [WebMethod]
    public string Update(object columns, string tableName, string tableID, string chartTitle) {
        if (_canContinue) {
            string columnList = CreateColumnList(columns as object[]);
            if (HelperMethods.ConvertBitToBoolean(columnList)) {
                DBViewer dbViewer1 = new DBViewer(false);
                dbViewer1.GetTableData(tableID);
                DataTable tempTable1 = dbViewer1.dt; //Save the table before deleting it.

                CustomTableViewer ctv = new CustomTableViewer(_userName);
                ctv.DropTable(tableID);
                ctv.UpdateTableNameAndChartTitle(tableID, tableName, chartTitle);

                if (CreateCustomTable(tableID, columnList)) {
                    DBViewer dbViewer2 = new DBViewer(false);
                    dbViewer2.GetTableData(tableID);
                    DataTable tempTable2 = dbViewer2.dt; //Get new table

                    List<string> newcolumns = BuildColumnList(tempTable2);

                    foreach (DataRow dr in tempTable1.Rows) {
                        if (!AddEntry(tableID, dr, newcolumns)) {
                            RebuildOriginal(tempTable1, tableID);
                            return "false";
                        }
                    }

                    return "";
                }
            }
        }

        return "false";
    }
    public void RebuildOriginal(DataTable dt, string tableName) {
        string columnList = CreateColumnList(BuildOriginalColumns(dt));

        CustomTableViewer ctv = new CustomTableViewer(_userName);
        ctv.DropTable(tableName);

        if (CreateCustomTable(tableName, columnList)) {
            List<string> newcolumns = BuildColumnList(dt);

            foreach (DataRow dr in dt.Rows) {
                AddEntry(tableName, dr, newcolumns);
            }
        }
    }


    [WebMethod]
    public string GetEditableUsers(string appId) {
        CustomTableViewer ctv = new CustomTableViewer(_userName);
        CustomTable_Coll tempColl = ctv.GetTableInfoByAppId(appId);

        StringBuilder str = new StringBuilder();

        string createdBy = tempColl.CreatedBy;

        if (!string.IsNullOrEmpty(createdBy)) {
            MemberDatabase createdByMember = new MemberDatabase(createdBy);
            List<string> groupList = createdByMember.GroupList;
            string checkboxInput = "<div class='checkbox-click float-left pad-right-big pad-bottom-big' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";
            Groups groups = new Groups(createdBy);

            foreach (string group in groupList) {
                List<string> users = groups.GetMembers_of_Group(group);
                string groupImg = groups.GetGroupImg_byID(group);

                if (groupImg.StartsWith("~/")) {
                    groupImg = ServerSettings.ResolveUrl(groupImg);
                }

                string groupImgHtmlCtrl = "<img alt='' src='" + groupImg + "' class='float-left margin-right' style='max-height: 24px;' />";
                str.Append("<h3 class='pad-bottom'>" + groupImgHtmlCtrl + groups.GetGroupName_byID(group) + "</h3><div class='clear-space'></div><div class='clear-space'></div>");
                foreach (string user in users) {
                    string isChecked = string.Empty;
                    bool foundUser = !string.IsNullOrEmpty(tempColl.UsersAllowedToEdit.Find(_x => _x.ToLower() == user.ToLower()));
                    if (foundUser) {
                        isChecked = "checked='checked'";
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
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        return str.ToString();
    }


    [WebMethod]
    public void UpdateEditableUsers(string id, string usersAllowed) {
        CustomTableViewer ctv = new CustomTableViewer(_userName);
        ctv.UpdateUsersAllowedToEdit(id, usersAllowed);
    }


    [WebMethod]
    public object[] GetCustomTableList(string table) {
        DBViewer dbViewer = new DBViewer(false);
        dbViewer.GetTableData(table);
        if (dbViewer.dt.Columns.Count > 0) {
            List<object> obj1 = new List<object>();

            CustomTableViewer ctv = new CustomTableViewer(_userName);
            CustomTable_Coll coll = ctv.GetTableInfo(table);

            List<object> objTemp = new List<object>();
            objTemp.Add(coll.ID);
            objTemp.Add(coll.ChartTitle);
            obj1.Add(objTemp);

            foreach (DataColumn dc in dbViewer.dt.Columns) {
                if ((dc.ColumnName.ToLower() != "entryid") && (dc.ColumnName.ToLower() != "timestamp")) {
                    List<object> obj2 = new List<object>();
                    obj2.Add(dc.ColumnName);
                    obj2.Add(GetReversedDataType(dc.DataType.Name));
                    obj2.Add(dc.AllowDBNull.ToString().ToLower());
                    obj1.Add(obj2.ToArray());
                }
            }

            return obj1.ToArray();
        }

        return null;
    }

    [WebMethod]
    public string UpdateSidebarChartType(string id, string sidebar, string notifyUsers, string chartType) {
        if (!string.IsNullOrEmpty(id)) {
            CustomTableViewer ctv = new CustomTableViewer(_userName);
            ctv.UpdateSidebarActiveAndChartTypeAndNotifyUsers(id, HelperMethods.ConvertBitToBoolean(sidebar), HelperMethods.ConvertBitToBoolean(notifyUsers), chartType);
        }

        return string.Empty;
    }


    public string BuildColumns(DataTable t) {
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
    public List<string> BuildColumnList(DataTable t) {
        var list = new List<string>();
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            list.Add(c.ColumnName);
        }

        return list;
    }
    public object[] BuildOriginalColumns(DataTable t) {
        List<object> obj1 = new List<object>();
        foreach (DataColumn col in t.Columns) {
            if ((col.ColumnName.ToLower() != "entryid") && (col.ColumnName.ToLower() != "timestamp")) {
                List<object> obj2 = new List<object>();
                obj2.Add(col.ColumnName);
                obj2.Add(GetReversedDataType(col.DataType.Name));
                obj2.Add(col.AllowDBNull.ToString().ToLower());
                obj1.Add(obj2.ToArray());
            }
        }

        return obj1.ToArray();
    }

    public bool AddEntry(string table, DataRow dr, List<string> columns) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        foreach (string column in columns) {
            if (dr.Table.Columns.Contains(column))
                query.Add(new DatabaseQuery(column, dr[column].ToString()));
            else
                query.Add(new DatabaseQuery(column, string.Empty));
        }

        if (dbCall.CallInsert(table, query)) {
            return true;
        }

        return false;
    }

    public bool CreateCustomTable(string tableName, string columnList) {
        if (dbCall.CallCreateTable(tableName, columnList)) {
            return true;
        }

        return false;
    }
    public string CreateColumnList(object[] columns) {
        StringBuilder columnText = new StringBuilder();

        columnText.Append("EntryID uniqueidentifier NOT NULL,");
        columnText.Append("TimeStamp datetime NOT NULL,");

        foreach (object row in columns) {
            object[] rowCol = row as object[];
            string columnName = rowCol[0].ToString();
            string dataType = GetDataType(rowCol[1].ToString());
            string nullable = string.Empty;
            if (!HelperMethods.ConvertBitToBoolean(rowCol[2].ToString()))
                nullable = " NOT NULL";

            if (NameGood(columnName))
                columnText.Append(columnName + " " + dataType + nullable + ",");
            else
                return "false";
        }

        columnText.Append("PRIMARY KEY (EntryID)");

        return columnText.ToString();
    }
    private static bool NameGood(string Name) {
        string tempName = Name.Replace("_", " ").ToLower();
        if ((tempName.Contains("delete ")) ||
            (tempName.Contains("update ")) ||
            (tempName.Contains("insert ")) ||
            (tempName.Contains("create ")) ||
            (tempName.Contains("commit ")) ||
            (tempName.Contains("rollback ")) ||
            (tempName.Contains("check ")) ||
            (tempName.Contains("grant revoke "))) {
            return false;
        }
        else if ((tempName == "delete") ||
            (tempName == "update") ||
            (tempName == "insert") ||
            (tempName == "create") ||
            (tempName == "commit") ||
            (tempName == "rollback") ||
            (tempName == "check") ||
            (tempName == "grant revoke") ||
            (tempName == "aspnet")) {
            return false;
        }
        else if (tempName.Contains("aspnet_")) {
            return false;
        }

        return true;
    }
    private static string GetDataType(string type) {
        string dataType = "nvarchar(4000)";
        switch (type.ToLower()) {
            case "nvarchar":
                dataType = "nvarchar(4000)";
                break;
            case "datetime":
                dataType = "datetime";
                break;
            case "integer":
                dataType = "int";
                break;
            case "decimal":
                dataType = "decimal(18, 0)";
                break;
            case "boolean":
                dataType = "bit";
                break;
        }

        return dataType;
    }
    private static string GetReversedDataType(string type) {
        string dataType = "nvarchar";
        switch (type.ToLower()) {
            case "datetime":
                dataType = "DateTime";
                break;
            case "int32":
                dataType = "Integer";
                break;
            case "decimal":
                dataType = "Decimal";
                break;
            case "boolean":
                dataType = "Boolean";
                break;
        }

        return dataType;
    }

}
