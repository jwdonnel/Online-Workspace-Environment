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
using System.Web.Script.Serialization;
using System.Web.Security;

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
        GetSiteRequests.AddRequest();

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

            string tempFile = ServerSettings.GetServerMapLocation + "Apps/Custom_Tables/Exports/" + ServerSettings.ServerDateTime.Ticks.ToString() + fileExtension;

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
    public string Create(string columns, string customizations, string tableName, string description, string installForUser, string notifyUsers, string isPrivate, string usersAllowed) {
        if (_canContinue) {
            string tableID = "CT_" + HelperMethods.RandomString(10);
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetTableData(tableID, false);
            if (dbViewer.dt != null) {
                if ((dbViewer.dt.Columns.Count > 0) || (dbViewer.dt.Rows.Count > 0))
                    return "false";
            }

            try {
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
                if (!categoryFound) {
                    category.addItem("Custom Tables", categoryId);
                }

                customizations = HttpUtility.UrlDecode(customizations);
                tableName = HttpUtility.UrlDecode(tableName);
                description = HttpUtility.UrlDecode(description);
                columns = HttpUtility.UrlDecode(columns);
                usersAllowed = HttpUtility.UrlDecode(usersAllowed);
                JavaScriptSerializer columnsSerializer = new JavaScriptSerializer();
                CustomTableColumnData[] columnCreater = columnsSerializer.Deserialize<CustomTableColumnData[]>(columns);

                if (columnCreater != null && columnCreater.Length > 0) {
                    string columnList = CreateColumnList(columnCreater);

                    if (CreateCustomTable(tableID, columnList)) {
                        string fileName = ctwc.CreateApp(tableName, categoryId, "", "database.png", true, HelperMethods.ConvertBitToBoolean(isPrivate));
                        CustomTableViewer ctv = new CustomTableViewer(_userName);
                        if (!string.IsNullOrEmpty(fileName)) {
                            fileName = "app-" + fileName;
                        }
                        ctv.AddItem(tableID, tableName, description, fileName, columns, _userName, HelperMethods.ConvertBitToBoolean(notifyUsers), usersAllowed, customizations);

                        if (HelperMethods.ConvertBitToBoolean(installForUser)) {
                            var member = new MemberDatabase(_userName);
                            member.UpdateEnabledApps(fileName);
                        }

                        return "";
                    }
                }
            }
            catch { }
        }

        return "false";
    }

    [WebMethod]
    public object[] EditTable(string id) {
        List<object> obj = new List<object>();
        if (_canContinue) {
            try {
                CustomTableViewer ctv = new CustomTableViewer(_userName);
                CustomTable_Coll coll = ctv.GetTableInfoByTableId(id);

                obj.Add(coll.TableName);
                obj.Add(coll.Description);
                obj.Add(GetEditableUsers(coll.TableID));
                obj.Add(coll.NotifyUsers);

                JavaScriptSerializer columnsSerializer = new JavaScriptSerializer();
                obj.Add(columnsSerializer.Serialize(coll.ColumnData));
                obj.Add(columnsSerializer.Serialize(coll.TableCustomizations));
            }
            catch { }
        }

        return obj.ToArray();
    }
    private string GetEditableUsers(string tableId) {
        CustomTableViewer ctv = new CustomTableViewer(_userName);
        CustomTable_Coll tempColl = ctv.GetTableInfoByTableId(tableId);

        StringBuilder str = new StringBuilder();

        string createdBy = tempColl.UpdatedBy;

        if (!string.IsNullOrEmpty(createdBy)) {
            MemberDatabase createdByMember = new MemberDatabase(createdBy);
            string checkboxInput = "<div class='checkbox-click float-left pad-right-big pad-top pad-bottom' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";

            MembershipUserCollection userColl = Membership.GetAllUsers();
            foreach (MembershipUser membershipUser in userColl) {
                string user = membershipUser.UserName;
                if (user.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    continue;
                }

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

                string marginTop = "8px";
                string userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                string acctImage = tempMember.AccountImage;
                string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
                str.AppendFormat(checkboxInput, isChecked, user.ToLower(), marginTop, userImageAndName + userNameTitle);
            }
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-top-big'>There are no usrs to select from</h4>");
        }

        return str.ToString();
    }

    [WebMethod]
    public string GetTableList() {
        string tableData = string.Empty;
        if (_canContinue) {
            List<Dictionary<string, object>> tableList = new List<Dictionary<string, object>>();

            CustomTableViewer ctv = new CustomTableViewer(_userName);
            ctv.BuildEntriesAll();

            MemberDatabase _member = new MemberDatabase(_userName);

            ServerSettings ss = new ServerSettings();
            bool AssociateWithGroups = ss.AssociateWithGroups;

            foreach (CustomTable_Coll table in ctv.CustomTableList) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckCustomTablesGroupAssociation(table, _member)) {
                        continue;
                    }
                }

                Dictionary<string, object> entry = new Dictionary<string,object>();
                entry.Add("name", table.TableName);
                entry.Add("id", table.TableID);
                entry.Add("data", table.ColumnData);
                tableList.Add(entry);
            }

            DBImporter importer = new DBImporter();
            importer.GetImportList();
            foreach (DBImporter_Coll dbColl in importer.DBColl) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckDBImportsGroupAssociation(dbColl, _member)) {
                        continue;
                    }
                }

                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry.Add("name", dbColl.TableName);
                entry.Add("id", dbColl.TableID);
                entry.Add("data", null);
                tableList.Add(entry);
            }

            try {
                JavaScriptSerializer columnsSerializer = new JavaScriptSerializer();
                tableData = columnsSerializer.Serialize(tableList);
            }
            catch { }
        }

        return tableData;
    }

    [WebMethod]
    public string RecreateApp(string appId, string tableId, string tableName, string description) {
        if (_canContinue) {

            appId = appId.Replace("app-", string.Empty);
            tableName = HttpUtility.UrlDecode(tableName);
            description = HttpUtility.UrlDecode(description);

            try {
                if (!string.IsNullOrEmpty(appId)) {
                    string serverpath = ServerSettings.GetServerMapLocation;
                    var fi = new FileInfo(serverpath + "Apps\\Custom_Tables\\" + appId + ".ascx");
                    if (fi.Exists) {
                        try {
                            App w = new App(_userName);
                            w.DeleteAppComplete("app-" + appId, ServerSettings.GetServerMapLocation);
                        }
                        catch { }
                    }
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
                App apps = new App(_userName);
                CustomTableViewer ctv = new CustomTableViewer(_userName);

                if (!string.IsNullOrEmpty(appId)) {
                    if (!string.IsNullOrEmpty(apps.GetAppInformation("app-" + appId).ID)) {
                        insertIntoAppList = false;
                    }

                    ctwc.CreateApp(tableName, categoryId, description, "database.png", insertIntoAppList, false, appId);
                }
                else {
                    appId = ctwc.CreateApp(tableName, categoryId, description, "database.png", insertIntoAppList, false);
                    ctv.UpdateRowAppId(tableId, appId);
                }

                return string.Empty;
            }
            catch { }
        }

        return "false";
    }

    [WebMethod]
    public string Update(string tableID, string columns, string customizations, string tableName, string description, string notifyUsers, string usersAllowed) {
        if (_canContinue) {
            bool foundTable = false;
            DBViewer dbViewer = new DBViewer(false);
            dbViewer.GetTableData(tableID, false);
            if (dbViewer.dt != null) {
                foundTable = true;
            }

            if (foundTable) {
                try {
                    tableName = HttpUtility.UrlDecode(tableName);
                    description = HttpUtility.UrlDecode(description);
                    columns = HttpUtility.UrlDecode(columns);
                    customizations = HttpUtility.UrlDecode(customizations);
                    usersAllowed = HttpUtility.UrlDecode(usersAllowed);
                    JavaScriptSerializer columnsSerializer = new JavaScriptSerializer();
                    CustomTableColumnData[] columnCreater = columnsSerializer.Deserialize<CustomTableColumnData[]>(columns);

                    if (columnCreater != null && columnCreater.Length > 0) {
                        string columnListStr = CreateColumnList(columnCreater);

                        List<string> columnList = new List<string>();
                        columnList.Add("EntryID");
                        columnList.Add("ApplicationId");
                        columnList.Add("TimeStamp");

                        foreach (CustomTableColumnData columnDataEntry in columnCreater) {
                            if (!columnList.Contains(columnDataEntry.realName)) {
                                columnList.Add(columnDataEntry.realName);
                            }
                        }

                        if (UpdateCustomTable(tableID, columnListStr, columnList, dbViewer.dt)) {
                            CustomTableViewer ctv = new CustomTableViewer(_userName);
                            ctv.UpdateRow(tableID, tableName, description, columns, HelperMethods.ConvertBitToBoolean(notifyUsers), usersAllowed, customizations);

                            App apps = new App(_userName);
                            string appId = ctv.GetAppIdByTableID(tableID);
                            if (!string.IsNullOrEmpty(appId)) {
                                Apps_Coll x = apps.GetAppInformation(appId);
                                if (!string.IsNullOrEmpty(x.ID)) {
                                    apps.UpdateAppName(x.ID, tableName);
                                }
                            }

                            return "";
                        }
                    }
                }
                catch { }
            }
        }

        return "false";
    }


    #region Private Methods

    private bool CreateCustomTable(string tableName, string columnList) {
        if (dbCall.CallCreateTable(tableName, columnList)) {
            return true;
        }

        return false;
    }
    private bool UpdateCustomTable(string tableName, string columnListStr, List<string> columnList, DataTable dt) {
        dbCall.CallDropTable(tableName);
        if (dbCall.CallCreateTable(tableName, columnListStr)) {
            foreach (DataRow row in dt.Rows) {
                AddEntry(tableName, row, columnList);
            }

            return true;
        }

        return false;
    }

    private string CreateColumnList(CustomTableColumnData[] columns) {
        StringBuilder columnText = new StringBuilder();

        columnText.Append("ApplicationId uniqueidentifier,");
        columnText.Append("EntryID uniqueidentifier NOT NULL,");
        columnText.Append("TimeStamp datetime NOT NULL,");

        foreach (CustomTableColumnData row in columns) {
            string columnName = row.realName;
            string dataType = "nvarchar(3999)";
            columnText.Append(columnName + " " + dataType + ",");
        }

        columnText.Append("PRIMARY KEY (EntryID)");

        return columnText.ToString();
    }
    private void AddEntry(string table, DataRow dr, List<string> columns) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        foreach (string column in columns) {
            if (dr.Table.Columns.Contains(column))
                query.Add(new DatabaseQuery(column, dr[column].ToString()));
            else
                query.Add(new DatabaseQuery(column, string.Empty));
        }

        dbCall.CallInsert(table, query);
    }
    private List<string> BuildColumnList(DataTable t) {
        var list = new List<string>();
        for (int i = 0; i < t.Columns.Count; i++) {
            DataColumn c = t.Columns[i];
            list.Add(c.ColumnName);
        }

        return list;
    }

    #endregion

}
