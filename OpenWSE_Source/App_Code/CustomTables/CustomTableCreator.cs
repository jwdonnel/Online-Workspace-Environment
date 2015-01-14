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
    public string Create(object columns, string tableName, string installForUser, string showSidebar, string isPrivate) {
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

                    string fileName = ctwc.CreateApp(HelperMethods.ConvertBitToBoolean(showSidebar), tableName, categoryId, "", "database.png", true, _isPrivate);
                    CustomTableViewer ctv = new CustomTableViewer(_userName);
                    ctv.AddItem(tableName, _userName, tableID, "app-" + fileName, DateTime.Now.ToString(), HelperMethods.ConvertBitToBoolean(showSidebar));

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
    public string RecreateApp(string appId, string tableName, string showSidebar) {
        if (_canContinue) {
            try {
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

                ctwc.CreateApp(HelperMethods.ConvertBitToBoolean(showSidebar), tableName, categoryId, "", "database.png", insertIntoAppList, false, appId);
                CustomTableViewer ctv = new CustomTableViewer(_userName);

                string id = "";
                ctv.BuildEntriesAll();
                foreach (CustomTable_Coll coll in ctv.CustomTableList) {
                    if (coll.AppID == "app-" + appId) {
                        id = coll.ID;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(id))
                    ctv.UpdateSidebarActive(id, HelperMethods.ConvertBitToBoolean(showSidebar));

                return string.Empty;
            }
            catch { }
        }

        return "false";
    }

    [WebMethod]
    public string Update(object columns, string tableName, string tableID) {
        if (_canContinue) {
            string columnList = CreateColumnList(columns as object[]);
            if (HelperMethods.ConvertBitToBoolean(columnList)) {
                DBViewer dbViewer1 = new DBViewer(false);
                dbViewer1.GetTableData(tableID);
                DataTable tempTable1 = dbViewer1.dt; //Save the table before deleting it.

                CustomTableViewer ctv = new CustomTableViewer(_userName);
                ctv.DropTable(tableID);
                ctv.UpdateTableName(tableID, tableName);

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
    public object[] GetCustomTableList(string table) {
        DBViewer dbViewer = new DBViewer(false);
        dbViewer.GetTableData(table);
        if (dbViewer.dt.Columns.Count > 0) {
            List<object> obj1 = new List<object>();
            foreach (DataColumn dc in dbViewer.dt.Columns) {
                if ((dc.ColumnName.ToLower() != "columnid") && (dc.ColumnName.ToLower() != "timestamp")) {
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
            if ((col.ColumnName.ToLower() != "columnid") && (col.ColumnName.ToLower() != "timestamp")) {
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

        columnText.Append("ColumnID uniqueidentifier NOT NULL,");
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

        columnText.Append("PRIMARY KEY (ColumnID)");

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
