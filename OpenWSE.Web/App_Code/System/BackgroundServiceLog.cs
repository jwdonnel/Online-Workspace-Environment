using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace OpenWSE_Tools.Logging {

    [Serializable]
    public class BackgroundServicesLog_Coll {
        private string _id = string.Empty;
        private string _fullNamespace = string.Empty;
        private string _message = string.Empty;
        private DateTime _dateAdded = new DateTime();

        public BackgroundServicesLog_Coll() { }
        public BackgroundServicesLog_Coll(string id, string fullNamespace, string message, string dateAdded) {
            _id = id;
            _fullNamespace = fullNamespace;
            _message = message;
            DateTime.TryParse(dateAdded, out _dateAdded);
        }

        public string ID {
            get { return _id; }
        }

        public string ServiceNamespace {
            get { return _fullNamespace; }
        }

        public string Message {
            get { return _message; }
        }

        public DateTime DateAdded {
            get { return _dateAdded; }
        }
    }

    /// <summary>
    /// Summary description for BackgroundServiceLog
    /// </summary>
    public class BackgroundServiceLog {
        private readonly DatabaseCall dbCall = new DatabaseCall();
        private const string TableName = "aspnet_BackgroundServicesLog";

        public BackgroundServiceLog() { }

        public void AddItem(string fullNamespace, string message) {
            if (dbCall.DataProvider == "System.Data.SqlServerCe.4.0") {
                if (message.Length > 3000) {
                    message = message.Substring(0, 3000);
                }
            }

            List<BackgroundServicesLog_Coll> tempList = GetBackgroundServiceLogs(fullNamespace);
            int[] total = { tempList.Count };
            int rtk = 100;
            if (WebConfigurationManager.AppSettings["RecordsToKeepInBackgroundServiceLog"] != null) {
                int.TryParse(WebConfigurationManager.AppSettings["RecordsToKeepInBackgroundServiceLog"], out rtk);
            }

            if (rtk <= 0) return;
            foreach (var a in tempList.TakeWhile(a => rtk <= total[0])) {
                DeleteSingleLogForService(a.ID);
                total[0]--;
            }

            if (!string.IsNullOrEmpty(fullNamespace) && !string.IsNullOrEmpty(message)) {
                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
                query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
                query.Add(new DatabaseQuery("ServiceNamespace", fullNamespace));
                query.Add(new DatabaseQuery("Message", HttpUtility.UrlEncode(message)));
                query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));
                dbCall.CallInsert(TableName, query);
            }
        }

        public List<BackgroundServicesLog_Coll> GetBackgroundServiceLogs(string fullNamespace) {
            List<BackgroundServicesLog_Coll> returnColl = new List<BackgroundServicesLog_Coll>();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("ServiceNamespace", fullNamespace), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "DateAdded DESC");
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                string id = dicVal["ID"];
                string message = HttpUtility.UrlDecode(dicVal["Message"]);
                string dateAdded = dicVal["DateAdded"];
                returnColl.Add(new BackgroundServicesLog_Coll(id, fullNamespace, message, dateAdded));
            }

            return returnColl;
        }

        public void DeleteLogForService(string fullNamespace) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ServiceNamespace", fullNamespace));
            dbCall.CallDelete(TableName, query);
        }

        public void DeleteSingleLogForService(string id) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));
            dbCall.CallDelete(TableName, query);
        }

        public bool DoesLogInformation(string fullNamespace) {
            return new OpenWSE_Tools.BackgroundServiceDatabaseCalls.BackgroundServices().DoesLogInformation(fullNamespace);
        }

        public void UpdateServiceLastUpdatedDate(string fullNamespace) {
            new OpenWSE_Tools.BackgroundServiceDatabaseCalls.BackgroundServices().UpdateState_DateOnly(fullNamespace);
        }

    }

}
