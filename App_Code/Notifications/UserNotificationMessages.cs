#region

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Security;
using System.Web;
using OpenWSE_Tools.AutoUpdates;
using System.Data.SqlServerCe;

#endregion

namespace OpenWSE_Tools.Notifications {

    [Serializable]
    public struct UserNotificationsMessage_Coll {
        private readonly string _id;
        private readonly string _notifiId;
        private readonly string _message;
        private readonly string _date;

        public UserNotificationsMessage_Coll(string id, string notifiId, string message, string date) {
            _id = id;
            _notifiId = notifiId;
            _message = message;
            _date = date;
        }

        public string ID {
            get { return _id; }
        }

        public string NotificationID {
            get { return _notifiId; }
        }

        public string Message {
            get { return _message; }
        }

        public string Date {
            get { return _date; }
        }
    }


    public class UserNotificationMessages {
        private readonly DatabaseCall dbCall = new DatabaseCall();
        private readonly string username;
        private readonly MailMessage message = new MailMessage();
        private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
        private List<UserNotificationsMessage_Coll> _messages = new List<UserNotificationsMessage_Coll>();


        public UserNotificationMessages(string username) {
            this.username = username;
        }

        public List<UserNotificationsMessage_Coll> Messages {
            get { return _messages; }
        }

        /// <summary>
        /// Determine if a notification can be logged in for a user
        /// </summary>
        /// <param name="appId">The app id that the notification needs to identify with</param>
        /// <param name="message">Message to post</param>
        /// <param name="addFlag">Add the flag for the Auto Update System</param>
        /// <param name="date">Date posted - Defaults to current date</param>
        /// <returns>The email address of the current user</returns>
        public string attemptAdd(string appId, string message, bool addFlag, string date = "") {
            MemberDatabase member = new MemberDatabase(username);
            string emailAddress = "";
            string notifiId = "";
            bool canContinue = false;

            if (member.UserHasApp(appId)) {
                App apps = new App(username);
                notifiId = apps.GetAppNotificationID(appId);
                canContinue = true;
            }
            else if ((appId == "236a9dc9-c92a-437f-8825-27809af36a3f")
                || (appId == "1159aca6-2449-4aff-bacb-5f29e479e2d7")
                || (appId == "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f")
                || (appId == "707ecc6c-2480-4080-bad6-fb135bb5cf13")) {
                // Error Report Notification ID / eRequests 
                notifiId = appId;
                canContinue = true;
            }

            // If ok to continue, add and email alert
            if (canContinue) {
                string tempUsername = GroupSessions.GetUserGroupSessionName(username);

                Notifications notifications = new Notifications();
                string[] nIds = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nId in nIds) {
                    string emailNotification = notifications.IsEmailNotificationEnabled(tempUsername, nId);
                    if (!string.IsNullOrEmpty(emailNotification)) {
                        message = HttpUtility.UrlEncode(message);
                        addNotification(nId, message, addFlag, date);
                        if (HelperMethods.ConvertBitToBoolean(emailNotification)) {
                            MembershipUser msu = Membership.GetUser(username);
                            if ((msu.Email.Contains("@")) && (msu.IsApproved) && (!msu.IsLockedOut))
                                emailAddress = msu.Email;
                        }
                    }
                }
            }

            return emailAddress;
        }

        /// <summary>
        /// Finish adding all the notifications to all the users in the mailTo list
        /// </summary>
        /// <param name="mailTo">MailMessage list</param>
        /// <param name="appId">App Id to associate notification with</param>
        /// <param name="message">Message body to post</param>
        /// <param name="subject">The subject or title of the notification</param>
        public static void finishAdd(MailMessage mailTo, string appId, string message, string subject = "") {
            App apps = new App(string.Empty);
            string notifiId = apps.GetAppNotificationID(appId);
            if (string.IsNullOrEmpty(notifiId))
                notifiId = appId;

            Notifications notifications = new Notifications();
            string[] nIds = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string nId in nIds) {
                Notifications_Coll coll = notifications.GetNotification(nId);
                if (!string.IsNullOrEmpty(coll.ID)) {
                    if (mailTo.To.Count > 0) {
                        if (string.IsNullOrEmpty(subject))
                            subject = OpenWSE.Core.Licensing.CheckLicense.SiteName + ": " + coll.NotificationName;

                        string requestUrl = ServerSettings.GetSitePath(HttpContext.Current.Request);
                        if ((!requestUrl.Contains("http:")) && (!requestUrl.Contains("https:")))
                            requestUrl = "http:" + requestUrl;

                        if (requestUrl.LastIndexOf('/') != requestUrl.Length - 1)
                            requestUrl += "/";

                        string urlUnsubscribe = requestUrl + ServerSettings.DefaultStartupPage + "?UnregisterEmails=" + nId;
                        message += "<br /><br /><div style='clear:both;height:5px'></div><a href='" + urlUnsubscribe + "' target='_blank'>Click here</a> to unsubscribe from these emails.";

                        ServerSettings.SendNewEmail(mailTo, "<h1 style='color:#555'>" + coll.NotificationName + "</h1>", subject, message.Trim());
                    }
                }
            }
        }

        public void forceAdd(string notifiID, string message, string date = "") {
            addNotification(notifiID, message, true, date);
        }

        private void addNotification(string notifiID, string message, bool addFlag, string date = "") {
            if (date == "")
                date = ServerSettings.ServerDateTime.ToString();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("NotificationID", notifiID));
            query.Add(new DatabaseQuery("UserName", username));
            query.Add(new DatabaseQuery("Message", message));
            query.Add(new DatabaseQuery("Date", date));

            if (dbCall.CallInsert("aspnet_UserNotificationMessages", query)) {
                if (addFlag) {
                    _uuf.addFlag(username, "workspace", "");
                }
            }
        }

        public void getEntries(string sortDir = "DESC") {
            _messages.Clear();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotificationMessages", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Date " + sortDir);
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string nId = row["NotificationID"];
                string message = HttpUtility.UrlDecode(row["Message"]);
                string date = row["Date"];
                UserNotificationsMessage_Coll coll = new UserNotificationsMessage_Coll(id, nId, message, date);
                _messages.Add(coll);
            }
        }

        public void deleteAllUserNotification() {
            if (dbCall.CallDelete("aspnet_UserNotificationMessages", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) })) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void deleteNotification(string id) {
            if (dbCall.CallDelete("aspnet_UserNotificationMessages", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) })) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void deleteNotification_All() {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

            dbCall.CallDelete("aspnet_UserNotificationMessages", query);
        }
    }

}