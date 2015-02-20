#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using System.Web;
using System.Web.UI;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using System.Data.SqlServerCe;
using System.IO;

#endregion

[Serializable]
public struct AppLog_Coll {
    private readonly string _dateposted;
    private readonly string _eventcomment;
    private readonly string _eventname;
    private readonly Guid _id;
    private string _appPath;
    private string _machineName;
    private string _requestUrl;
    private string _exceptionType;
    private string _stackTrace;
    private string _userName;

    public AppLog_Coll(Guid id, string en, string ec, string date, string appPath, string machineName, string requestUrl, string exceptionType, string stackTrace, string userName) {
        _id = id;
        _eventname = en;
        _eventcomment = ec;
        _dateposted = date;
        _appPath = appPath;
        _machineName = machineName;
        _requestUrl = requestUrl;
        _exceptionType = exceptionType;
        _stackTrace = stackTrace;
        _userName = userName;
    }

    public Guid EventID {
        get { return _id; }
    }

    public string EventName {
        get { return _eventname; }
    }

    public string EventComment {
        get { return _eventcomment; }
    }

    public string DatePosted {
        get { return _dateposted; }
    }

    public string ApplicationPath {
        get { return _appPath; }
    }

    public string MachineName {
        get { return _machineName; }
    }

    public string RequestUrl {
        get { return _requestUrl; }
    }

    public string ExceptionType {
        get { return _exceptionType; }
    }

    public string StackTrace {
        get { return _stackTrace; }
    }

    public string UserName {
        get { return _userName; }
    }
}

[Serializable]
public class AppLog {
    public static bool AddingError = false;
    private readonly List<AppLog_Coll> _appColl = new List<AppLog_Coll>();
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();

    public AppLog(bool getvalues) {
        if (getvalues) {
            _appColl.Clear();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_WebEvent_Events", "", null, "DatePosted DESC");
            foreach (Dictionary<string, string> row in dbSelect) {
                var id = Guid.Parse(row["EventId"]);
                string en = row["EventName"];
                string ec = HttpUtility.UrlDecode(row["EventComment"]);
                string d = row["DatePosted"];
                string ap = row["ApplicationPath"];
                string mn = row["MachineName"];
                string rurl = row["RequestUrl"];
                string et = row["ExceptionType"];
                string st = row["StackTrace"];
                string un = row["UserName"];

                var coll = new AppLog_Coll(id, en, ec, d, ap, mn, rurl, et, st, un);
                updateSlots(coll);
            }
        }
    }

    public List<AppLog_Coll> app_coll {
        get { return _appColl; }
    }

    private void addItem(string eventname, string eventcomment, Exception e) {
        HttpContext Context = HttpContext.Current;
 
        if (!AddingError) {
            AddingError = true;
            var ss = new ServerSettings();
            if (ss.RecordActivity) {
                string currUser = "Not a system user.";
                if (HttpContext.Current.User != null) {
                    currUser = HttpContext.Current.User.Identity.Name;
                    if (string.IsNullOrEmpty(currUser)) {
                        currUser = "Not a system user.";
                    }
                }
                bool cont = true;
                AppLogIgnore ignores = new AppLogIgnore(true);
                if (ignores.appIgnore_coll.Count > 0) {
                    foreach (AppLogIgnore_Coll ec in ignores.appIgnore_coll) {
                        if (eventcomment.ToLower().Contains(ec.EventComment.ToLower())) {
                            int timesHit = ec.TimesHit + 1;
                            ignores.UpdateTimesHit(ec.EventID.ToString(), timesHit);

                            if (ec.RefreshOnError) {
                                string url = HttpContext.Current.Request.RawUrl;
                                HttpContext.Current.Response.Redirect(url);
                            }
                            cont = false;
                            break;
                        }
                    }
                }

                if (ss.AllowPrivacy) {
                    MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    if (member.PrivateAccount)
                        cont = false;
                }

                string date = DateTime.Now.ToString();

                // If cont == true, Insert new row into AppLog table
                if (cont) {

                    string type = "N/A";
                    if (Context.Error != null) {
                        type = Context.Error.GetType().FullName;
                    }
                    else if (e != null) {
                        type = e.GetType().FullName;
                    }

                    string stackTrace = "N/A";
                    if (Context.Error != null) {
                        stackTrace = Context.Error.StackTrace;
                    }
                    else if (e != null) {
                        stackTrace = e.StackTrace;
                    }

                    if (ss.RecordErrorsOnly && (string.IsNullOrEmpty(type) || type == "N/A")) {
                        AddingError = false;
                        return;
                    }

                    if (ss.EmailActivity && !string.IsNullOrEmpty(type) && type != "N/A") {
                        try {
                            SendErrorMessage(eventname, type, HttpContext.Current.Request.Url.OriginalString, currUser, eventcomment, date);
                        }
                        catch { }
                    }

                    DeleteTopRecords();

                    DatabaseCall tempdbCall = new DatabaseCall();
                    tempdbCall.NeedToLog = false;

                    List<DatabaseQuery> query = new List<DatabaseQuery>();
                    query.Add(new DatabaseQuery("EventId", Guid.NewGuid().ToString()));
                    query.Add(new DatabaseQuery("EventName", eventname));
                    query.Add(new DatabaseQuery("EventComment", HttpUtility.UrlEncode(eventcomment)));
                    query.Add(new DatabaseQuery("DatePosted", date));
                    query.Add(new DatabaseQuery("ApplicationPath", HttpContext.Current.Request.ApplicationPath));
                    query.Add(new DatabaseQuery("MachineName", System.Environment.MachineName));
                    query.Add(new DatabaseQuery("RequestUrl", HttpContext.Current.Request.Url.OriginalString));
                    query.Add(new DatabaseQuery("ExceptionType", type));
                    query.Add(new DatabaseQuery("StackTrace", stackTrace));
                    query.Add(new DatabaseQuery("UserName", currUser));


                    tempdbCall.CallInsert("aspnet_WebEvent_Events", query);

                    if (ss.RecordActivityToLogFile && !string.IsNullOrEmpty(type) && type != "N/A") {
                        CreateLogFile(eventname, eventcomment, date, HttpContext.Current.Request.ApplicationPath, System.Environment.MachineName, HttpContext.Current.Request.Url.OriginalString, type, stackTrace, currUser);
                    }
                }
            }
            AddingError = false;
        }
    }
    private void addItem(string eventname, string eventcomment) {
        if (!AddingError) {
            AddingError = true;
            var ss = new ServerSettings();
            if (ss.RecordActivity) {
                string currUser = "Not a system user.";
                if (HttpContext.Current.User != null) {
                    currUser = HttpContext.Current.User.Identity.Name;
                    if (string.IsNullOrEmpty(currUser)) {
                        currUser = "Not a system user.";
                    }
                }
                bool cont = true;
                AppLogIgnore ignores = new AppLogIgnore(true);
                if (ignores.appIgnore_coll.Count > 0) {
                    foreach (AppLogIgnore_Coll ec in ignores.appIgnore_coll) {
                        if (eventcomment.ToLower().Contains(ec.EventComment.ToLower())) {
                            int timesHit = ec.TimesHit + 1;
                            ignores.UpdateTimesHit(ec.EventID.ToString(), timesHit);

                            if (ec.RefreshOnError) {
                                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl);
                            }
                            cont = false;
                            break;
                        }
                    }
                }

                if (ss.AllowPrivacy) {
                    MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    if (member.PrivateAccount)
                        cont = false;
                }

                string date = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                // If cont == true, Insert new row into AppLog table
                if (cont) {

                    string type = "N/A";
                    string stackTrace = "N/A";

                    if (ss.RecordErrorsOnly && (string.IsNullOrEmpty(eventname) || eventname != "Javascript Error")) {
                        AddingError = false;
                        return;
                    }

                    if (ss.EmailActivity && eventname == "Javascript Error") {
                        try {
                            SendErrorMessage(eventname, type, HttpContext.Current.Request.Url.OriginalString, currUser, eventcomment, date);
                        }
                        catch { }
                    }

                    DeleteTopRecords();

                    DatabaseCall tempdbCall = new DatabaseCall();
                    tempdbCall.NeedToLog = false;

                    List<DatabaseQuery> query = new List<DatabaseQuery>();
                    query.Add(new DatabaseQuery("EventId", Guid.NewGuid().ToString()));
                    query.Add(new DatabaseQuery("EventName", eventname));
                    query.Add(new DatabaseQuery("EventComment", HttpUtility.UrlEncode(eventcomment)));
                    query.Add(new DatabaseQuery("DatePosted", date));
                    query.Add(new DatabaseQuery("ApplicationPath", HttpContext.Current.Request.ApplicationPath));
                    query.Add(new DatabaseQuery("MachineName", System.Environment.MachineName));
                    query.Add(new DatabaseQuery("RequestUrl", HttpContext.Current.Request.Url.OriginalString));
                    query.Add(new DatabaseQuery("ExceptionType", type));
                    query.Add(new DatabaseQuery("StackTrace", stackTrace));
                    query.Add(new DatabaseQuery("UserName", currUser));

                    tempdbCall.CallInsert("aspnet_WebEvent_Events", query);

                    if (ss.RecordActivityToLogFile && eventname == "Javascript Error") {
                        CreateLogFile(eventname, eventcomment, date, HttpContext.Current.Request.ApplicationPath, System.Environment.MachineName, HttpContext.Current.Request.Url.OriginalString, type, stackTrace, currUser);
                    }
                }
            }
            AddingError = false;
        }
    }

    public static void AddError(Exception e) {
        AppLog tempLog = new AppLog(false);

        string innerMessage = string.Empty;

        HttpContext Context = HttpContext.Current;
        if ((Context != null) && (Context.Error != null)) {
            if (Context.Error.InnerException != null)
                innerMessage = Context.Error.InnerException.Message;

            if (string.IsNullOrEmpty(Context.Error.Message)) return;

            if (!Context.Error.Message.ToLower().Contains("file does not exist")) {
                string errorMessage = Context.Error.Message + " - " + innerMessage;
                if (Context.Error.Message == innerMessage)
                    errorMessage = Context.Error.Message;

                tempLog.addItem(Context.Error.TargetSite.Name, errorMessage, e);
            }
        }
        else if (e != null) {
            if (e.InnerException != null)
                innerMessage = e.InnerException.Message;

            if (string.IsNullOrEmpty(e.Message)) return;

            if (!e.Message.ToLower().Contains("file does not exist")) {
                string errorMessage = e.Message + " - " + innerMessage;
                if (e.Message == innerMessage)
                    errorMessage = e.Message;

                tempLog.addItem(e.TargetSite.Name, errorMessage, e);
            }
        }
    }
    public static void AddError(string message) {
        if (string.IsNullOrEmpty(message)) return;

        AppLog tempLog = new AppLog(false);
        tempLog.addItem("Javascript Error", message);
    }

    public static void AddEvent(string title, string message) {
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message)) return;

        AppLog tempLog = new AppLog(false);
        tempLog.addItem(title, message);
    }

    #region Log Builder

    private static void CreateLogFile(string eventName, string eventComment, string date, string appPath, string machineName, string url, string type, string stackTrace, string currUser) {
        try {
            string logFolder = ServerSettings.GetServerMapLocation + "Logging";
            if (!Directory.Exists(logFolder)) {
                Directory.CreateDirectory(logFolder);
            }

            StringBuilder str = new StringBuilder();
            str.Append(BuildHeader(date, currUser));
            str.Append(BuildBody(eventName, url, type, eventComment, stackTrace));
            str.Append(BuildFooter());

            string fileName = eventName;
            if (fileName.Length > 15) {
                fileName = fileName.Substring(0, 15);
            }

            string timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
            File.WriteAllText(logFolder + "\\" + "EventLog_" + fileName + "_" + timestamp + ".log", str.ToString());
        }
        catch { }
    }
    private static string BuildHeader(string date, string user) {
        StringBuilder strHeader = new StringBuilder();
        strHeader.Append("---------------- START ----------------");
        strHeader.Append(Environment.NewLine + Environment.NewLine);
        strHeader.Append("Date Logged: " + date);
        strHeader.Append(Environment.NewLine);
        strHeader.Append("User: " + user);
        strHeader.Append(Environment.NewLine);

        return strHeader.ToString();
    }
    private static string BuildBody(string eventName, string requestUrl, string type, string message, string stackTrace) {
        StringBuilder strBody = new StringBuilder();
        strBody.Append("EventName: " + eventName);
        strBody.Append(Environment.NewLine);
        strBody.Append("EventComment: " + message);
        strBody.Append(Environment.NewLine);
        strBody.Append("RequestUrl: " + requestUrl);
        strBody.Append(Environment.NewLine);
        strBody.Append("ExceptionType: " + type);
        strBody.Append(Environment.NewLine);
        strBody.Append("StackTrace: " + stackTrace);

        return strBody.ToString();
    }
    private static string BuildFooter() {
        StringBuilder strFooter = new StringBuilder();
        strFooter.Append(Environment.NewLine + Environment.NewLine);
        strFooter.Append("---------------- END ----------------");

        return strFooter.ToString();
    }

    #endregion

    public string GetEventComment(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_WebEvent_Events", "EventComment", new List<DatabaseQuery>() { new DatabaseQuery("EventId", id) });
        return HttpUtility.UrlDecode(dbSelect.Value);
    }

    private void DeleteTopRecords() {
        var ss = new ServerSettings();
        List<AppLog_Coll> tempList = PopulateRecords();
        int[] total = { tempList.Count };
        int rtk = ss.WebEventsToKeep;

        if (rtk <= 0) return;
        foreach (var a in tempList.TakeWhile(a => rtk <= total[0])) {
            deleteRecord(a.EventID.ToString());
            total[0]--;
        }
    }

    private List<AppLog_Coll> PopulateRecords() {
        List<AppLog_Coll> temp = new List<AppLog_Coll>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_WebEvent_Events", "", null, "DatePosted DESC");
        foreach (Dictionary<string, string> row in dbSelect) {
            var id = Guid.Parse(row["EventId"]);
            string en = row["EventName"];
            string ec = HttpUtility.UrlDecode(row["EventComment"]);
            string d = row["DatePosted"];
            string ap = row["ApplicationPath"];
            string mn = row["MachineName"];
            string rurl = row["RequestUrl"];
            string et = row["ExceptionType"];
            string st = row["StackTrace"];
            string un = row["UserName"];
            var coll = new AppLog_Coll(id, en, ec, d, ap, mn, rurl, et, st, un);
            temp.Add(coll);
        }

        return temp;
    }

    private void SendErrorMessage(string eventname, string exceptionType, string requestUrl, string userName, string eventcomment, string date) {
        MailMessage mailTo = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<h3 style='color: #7F0000'>An error has occured</h3><div style='clear: both;'></div>");
        messagebody.Append("<p>An error message was reported on " + date + ". This error has been recorded into the Web Events. See details below for more information: ");
        messagebody.Append("<br /><b style='padding-right: 5px; padding-top: 3px;'>Event Name:</b>" + eventname);
        messagebody.Append("<br /><b style='padding-right: 5px; padding-top: 3px;'>Exception Type:</b>" + exceptionType);
        messagebody.Append("<br /><b style='padding-right: 5px; padding-top: 3px;'>Request Url:</b>" + requestUrl);
        messagebody.Append("<br /><b style='padding-right: 5px; padding-top: 3px;'>Username:</b>" + userName);
        messagebody.Append("<br /><b style='padding-right: 5px'>Event Comment:</b>" + eventcomment + "</p><br />");

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            if (Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) {
                var un = new UserNotificationMessages(u.UserName);
                string email = un.attemptAdd("236a9dc9-c92a-437f-8825-27809af36a3f", messagebody.ToString(), true);
                if (!string.IsNullOrEmpty(email))
                    mailTo.To.Add(email);
            }
        }

        UserNotificationMessages.finishAdd(mailTo, "236a9dc9-c92a-437f-8825-27809af36a3f", messagebody.ToString());
    }

    public void deleteLog() {
        dbCall.CallDelete("aspnet_WebEvent_Events", null);
    }

    public void deleteRecord(string id) {
        dbCall.CallDelete("aspnet_WebEvent_Events", new List<DatabaseQuery>() { new DatabaseQuery("EventId", id) });
    }

    private void updateSlots(AppLog_Coll coll) {
        _appColl.Add(coll);
    }
}