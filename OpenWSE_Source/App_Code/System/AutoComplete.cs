using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Data;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for AutoComplete
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AutoComplete : System.Web.Services.WebService {

    public AutoComplete() {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    public string Strip(string text) {
        //Regex regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
        string temp = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        string temp2 = temp.Replace("&nbsp;", "");
        string temp3 = temp2.Replace("\n", "");
        string temp4 = temp3.Replace("\t", "");
        return temp4;
    }


    [WebMethod]
    public string[] GetListOfAppLogEvents(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var applog = new AppLog(true);
            if (!string.IsNullOrEmpty(prefixText)) {
                foreach (AppLog_Coll t in applog.app_coll) {
                    if ((!t.DatePosted.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.EventName.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.EventComment.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.StackTrace.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.ExceptionType.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.RequestUrl.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.ApplicationPath.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.MachineName.ToLower().Contains(prefixText.ToLower())) &&
                        (!t.UserName.ToLower().Contains(prefixText.ToLower()))) continue;
                    string x = t.EventComment;
                    if (x.Length > 115) {
                        x = x.Substring(0, 115) + "...";
                    }

                    if (!temp.Contains(x)) {
                        temp.Add(x);
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetListOfUsers(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            MembershipUserCollection coll = Membership.GetAllUsers();
            ServerSettings _ss = new ServerSettings();
            if (string.IsNullOrEmpty(prefixText)) {
                if (prefixText.ToLower() == "me") {
                    temp.Add(HttpContext.Current.User.Identity.Name);
                }
                else {
                    foreach (MembershipUser u in coll) {
                        MemberDatabase tempmember = new MemberDatabase(u.UserName.ToLower());
                        if ((u.UserName.ToLower().Contains(prefixText.ToLower())) || (u.Email.ToLower().Contains(prefixText.ToLower())) || (tempmember.FirstName.ToLower().Contains(prefixText.ToLower())) || (tempmember.LastName.ToLower().Contains(prefixText.ToLower()))) {
                            bool canContinue = true;

                            if (_ss.AllowPrivacy) {
                                if ((tempmember.PrivateAccount) && (u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                                    && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved))) {
                                    canContinue = false;
                                }
                            }

                            if (canContinue) {
                                if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                                    if (!temp.Contains(u.UserName.ToLower())) temp.Add(u.UserName);
                                }
                            }
                        }
                    }
                }
            }
            else {
                foreach (MembershipUser u in coll) {
                    MemberDatabase tempmember = new MemberDatabase(u.UserName.ToLower());
                    if ((u.UserName.ToLower().Contains(prefixText.ToLower())) || (u.Email.ToLower().Contains(prefixText.ToLower())) || (tempmember.FirstName.ToLower().Contains(prefixText.ToLower())) || (tempmember.LastName.ToLower().Contains(prefixText.ToLower()))) {
                        bool canContinue = true;

                        if (_ss.AllowPrivacy) {
                            if ((tempmember.PrivateAccount) && (u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                                && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved))) {
                                canContinue = false;
                            }
                        }

                        if (canContinue) {
                            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                                if (!temp.Contains(u.UserName.ToLower())) temp.Add(u.UserName);
                            }
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetListOfUsersByFullName(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            MembershipUserCollection coll = Membership.GetAllUsers();
            ServerSettings _ss = new ServerSettings();
            if (string.IsNullOrEmpty(prefixText)) {
                foreach (MembershipUser u in coll) {
                    MemberDatabase tempmember = new MemberDatabase(u.UserName.ToLower());
                    if ((u.UserName.ToLower().Contains(prefixText.ToLower())) || (u.Email.ToLower().Contains(prefixText.ToLower())) || (tempmember.FirstName.ToLower().Contains(prefixText.ToLower())) || (tempmember.LastName.ToLower().Contains(prefixText.ToLower()))) {
                        bool canContinue = true;

                        if (_ss.AllowPrivacy) {
                            if ((tempmember.PrivateAccount) && (u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                                && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved))) {
                                canContinue = false;
                            }
                        }

                        if (canContinue) {
                            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                                string fullname = HelperMethods.MergeFMLNames(tempmember);
                                if (!temp.Contains(fullname)) temp.Add(fullname);
                            }
                        }
                    }
                }
            }
            else {
                foreach (MembershipUser u in coll) {
                    MemberDatabase tempmember = new MemberDatabase(u.UserName.ToLower());
                    if ((u.UserName.ToLower().Contains(prefixText.ToLower())) || (u.Email.ToLower().Contains(prefixText.ToLower())) || (tempmember.FirstName.ToLower().Contains(prefixText.ToLower())) || (tempmember.LastName.ToLower().Contains(prefixText.ToLower()))) {
                        bool canContinue = true;

                        if (_ss.AllowPrivacy) {
                            if ((tempmember.PrivateAccount) && (u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())
                                && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved))) {
                                canContinue = false;
                            }
                        }

                        if (canContinue) {
                            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                                string fullname = HelperMethods.MergeFMLNames(tempmember);
                                if (!temp.Contains(fullname)) temp.Add(fullname);
                            }
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetFiles(string prefixText, int count) {
        var smw = new FileDrive(HttpContext.Current.User.Identity.Name.ToLower());
        smw.GetAllFiles();
        smw.GetAllFolders();
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            int cc = 0;
            foreach (var x in smw.documents_coll) {
                if (x.FileName.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(x.FileName)) {
                        temp.Add(x.FileName);
                        cc++;
                    }
                }
                else if (x.Folder.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(x.Folder)) {
                        temp.Add(x.Folder);
                        cc++;
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetToolList(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(prefixText)) {
                List<string> pages = ServerSettings.AdminPages();
                if (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                    pages = new MemberDatabase(HttpContext.Current.User.Identity.Name).AdminPagesList.ToList();
                }

                foreach (string p in pages) {
                    if (p.ToLower().Contains(prefixText.ToLower())) {
                        string testP = p;
                        if (testP.Length > 115) {
                            testP = testP.Substring(0, 115) + "...";
                        }

                        if (!temp.Contains(testP)) {
                            temp.Add(testP);
                        }
                    }
                }
            }
        }

        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetAppSearchList(string prefixText, int count) {
        ServerSettings _ss = new ServerSettings();
        var temp = new List<string>();
        string user = HttpContext.Current.User.Identity.Name;
        MemberDatabase m = new MemberDatabase(user);
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var apps = new App(user);
            List<string> appList = apps.DeleteDuplicateEnabledApps(m);

            bool AssociateWithGroups = _ss.AssociateWithGroups;

            if (!string.IsNullOrEmpty(prefixText)) {
                for (int i = 0; i < appList.Count; i++) {
                    Apps_Coll row = apps.GetAppInformation(appList[i]);
                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(row, m)) {
                            continue;
                        }
                    }

                    if ((apps.AppList.Count > 0) && ((HttpContext.Current.User.Identity.Name.ToLower() != apps.AppList[i].CreatedBy.ToLower()) && (apps.AppList[i].IsPrivate))) {
                        continue;
                    }

                    if ((row.AppName.ToLower().Contains(prefixText.ToLower()))
                        && (row.AppId.ToLower() != "app-appinstaller")) {

                        string appName = row.AppName;
                        if (appName.Length > 115) {
                            appName = appName.Substring(0, 115) + "...";
                        }

                        if (!temp.Contains(appName)) {
                            temp.Add(appName);
                        }
                    }
                }

                List<string> pages = ServerSettings.AdminPages();
                if (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                    pages = new MemberDatabase(HttpContext.Current.User.Identity.Name).AdminPagesList.ToList();
                }

                foreach (string p in pages) {
                    if (p.ToLower().Contains(prefixText.ToLower())) {
                        string testP = p;
                        if (testP.Length > 115) {
                            testP = testP.Substring(0, 115) + "...";
                        }

                        if (!temp.Contains(testP)) {
                            temp.Add(testP);
                        }
                    }
                }
            }
        }
        else if (_ss.NoLoginRequired) {
            var appsTemp = new App();
            NewUserDefaults _dc = new NewUserDefaults("DemoNoLogin");
            AppPackages package = new AppPackages(false);
            string[] appList = package.GetAppList(_dc.GetDemoAppPackage);
            if (!string.IsNullOrEmpty(prefixText)) {
                for (int i = 0; i < appList.Length; i++) {
                    try {
                        Apps_Coll row = appsTemp.GetAppInformation(appList[i]);
                        if ((row.AppName.ToLower().Contains(prefixText.ToLower()))
                            && (row.AppId != "app-appinstaller")) {
                            string x = row.AppName;
                            if (x.Length > 115) {
                                x = x.Substring(0, 115) + "...";
                            }

                            if (!temp.Contains(x)) {
                                temp.Add(x);
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetListOfGroups(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            Groups groups = new Groups();
            groups.getEntries();
            foreach (Dictionary<string, string> row in groups.group_dt) {
                if (!string.IsNullOrEmpty(prefixText)) {
                    if (row["GroupName"].ToLower().Contains(prefixText.ToLower().Trim())) {
                        if (!temp.Contains(row["GroupName"])) {
                            temp.Add(row["GroupName"]);
                        }
                    }
                }
            }

            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetAppSearchList_Installer(string prefixText, int count) {
        ServerSettings _ss = new ServerSettings();
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var apps = new App();
            apps.GetAllApps();

            bool AssociateWithGroups = _ss.AssociateWithGroups;

            MemberDatabase _member = new MemberDatabase(HttpContext.Current.User.Identity.Name);

            if (!string.IsNullOrEmpty(prefixText)) {
                for (int i = 0; i < apps.AppList.Count; i++) {
                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(apps.AppList[i], _member)) {
                            continue;
                        }
                    }

                    if ((HttpContext.Current.User.Identity.Name.ToLower() != apps.AppList[i].CreatedBy.ToLower()) && (apps.AppList[i].IsPrivate)) {
                        continue;
                    }

                    if (((apps.AppList[i].AppName.ToLower().Contains(prefixText.ToLower())) || (apps.AppList[i].CreatedBy.ToLower().Contains(prefixText.ToLower())))
                        && (apps.AppList[i].AppId != "app-appinstaller")) {
                        string x = apps.AppList[i].AppName;
                        if (x.Length > 115) {
                            x = x.Substring(0, 115) + "...";
                        }

                        if (!temp.Contains(x)) {
                            temp.Add(x);
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetBookmarks(string prefixText, int count) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string user = HttpContext.Current.User.Identity.Name;
            if (user != null) {
                Bookmarks bookmarks = new Bookmarks(user);
                bookmarks.GetBookmarks("BookmarkName ASC");
                if (!string.IsNullOrEmpty(prefixText)) {
                    for (int i = 0; i < bookmarks.bookmarks_dt.Count; i++) {
                        Dictionary<string, string> row = bookmarks.bookmarks_dt[i];
                        if ((row["BookmarkName"].ToLower().Contains(prefixText.ToLower()))
                            && (row["BookmarkHTML"] != "app-appinstaller")) {
                            string x = HttpUtility.HtmlDecode(row["BookmarkName"]);
                            if (x.Length > 115) {
                                x = x.Substring(0, 115) + "...";
                            }

                            if (!temp.Contains(x)) {
                                temp.Add(x);
                            }
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetCustomTableData(string prefixText, int count, string id, string columnName) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(prefixText)) {
                CustomTableViewer ctv = new CustomTableViewer(HttpContext.Current.User.Identity.Name);
                string tableName = ctv.GetTableIDByAppID("app-" + id);
                DBViewer dbViewer = new DBViewer(false);
                List<string> list = dbViewer.GetTableDataColumn(tableName, columnName);

                for (int i = 0; i < list.Count; i++) {
                    if (list[i].ToLower().Contains(prefixText.ToLower())) {
                        if (!temp.Contains(list[i])) {
                            temp.Add(list[i]);
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetdbImportData(string prefixText, int count, string id, string columnName) {
        var temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (!string.IsNullOrEmpty(prefixText)) {
                string tableName = string.Empty;
                DBImporter_Coll dbcoll_temp = GetDataBase_Coll(id);
                if (dbcoll_temp != null) {
                    tableName = dbcoll_temp.SelectCommand.Substring(dbcoll_temp.SelectCommand.IndexOf(" FROM ") + (" FROM ").Length);
                    tableName = tableName.Replace(dbcoll_temp.SelectCommand.Substring(dbcoll_temp.SelectCommand.ToLower().IndexOf("order by")), "").Trim();
                }

                if (!string.IsNullOrEmpty(tableName)) {
                    DBViewer dbViewer = new DBViewer(false);
                    List<string> list = dbViewer.GetTableDataColumnImport(tableName, columnName, dbcoll_temp);

                    for (int i = 0; i < list.Count; i++) {
                        if (list[i].ToLower().Contains(prefixText.ToLower())) {
                            if (!temp.Contains(list[i])) {
                                temp.Add(list[i]);
                            }
                        }
                    }
                }
            }
            temp.Sort();
        }
        return temp.ToArray();
    }
    private DBImporter_Coll GetDataBase_Coll(string id) {
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

}
