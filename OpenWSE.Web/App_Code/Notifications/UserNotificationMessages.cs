#region

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Security;
using System.Web;
using OpenWSE_Tools.AutoUpdates;

#endregion

namespace OpenWSE_Tools.Notifications {

    [Serializable]
    public struct UserNotificationsMessage_Coll {
        private readonly string _id;
        private readonly string _notifiId;
        private readonly string _message;
        private readonly bool _dismissed;
        private readonly string _date;

        public UserNotificationsMessage_Coll(string id, string notifiId, string message, string dismissed, string date) {
            _id = id;
            _notifiId = notifiId;
            _message = message;
            _dismissed = false;
            if (HelperMethods.ConvertBitToBoolean(dismissed)) {
                _dismissed = true;
            }
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

        public bool Dismissed {
            get { return _dismissed; }
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

        public UserNotificationMessages(string username) {
            this.username = username;
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

            string tempUsername = GroupSessions.GetUserGroupSessionName(username);
            UserNotifications notifications = new UserNotifications();

            if (member.UserHasApp(appId)) {
                App apps = new App(username);
                if (apps.GetAllowNotifications(appId)) {
                    canContinue = true;
                }
            }
            else if (appId == UserNotifications.ErrorReportingID || appId == UserNotifications.GroupAlertID) {
                canContinue = true;
            }

            // If ok to continue, add and email alert
            if (canContinue && notifications.IsUserNotificationEnabled(tempUsername, appId)) {
                notifiId = appId;
                message = HttpUtility.UrlEncode(message);
                addNotification(notifiId, message, addFlag, date);

                if (notifications.IsEmailNotificationEnabled(tempUsername, notifiId)) {
                    MembershipUser msu = Membership.GetUser(username);
                    if ((msu.Email != null) && (msu.Email.Contains("@")) && (msu.IsApproved) && (!msu.IsLockedOut))
                        emailAddress = msu.Email;
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
            if (!string.IsNullOrEmpty(appId)) {
                if (mailTo.To.Count > 0) {
                    string notificationName = UserNotifications.GetNotificationName(appId);
                    if (string.IsNullOrEmpty(subject))
                        subject = ServerSettings.SiteName + ": " + notificationName;

                    string requestUrl = ServerSettings.GetSitePath(HttpContext.Current.Request);
                    if ((!requestUrl.Contains("http:")) && (!requestUrl.Contains("https:")))
                        requestUrl = "http:" + requestUrl;

                    if (requestUrl.LastIndexOf('/') != requestUrl.Length - 1)
                        requestUrl += "/";

                    if (appId.StartsWith("app-") || appId == UserNotifications.ErrorReportingID || appId == UserNotifications.GroupAlertID) {
                        string urlUnsubscribe = requestUrl + ServerSettings.DefaultStartupPage + "?UnregisterEmails=" + appId;
                        message += "<br /><br /><div style='clear:both;height:5px'></div><a href='" + urlUnsubscribe + "' target='_blank'>Click here</a> to unsubscribe from these emails.";
                    }

                    ServerSettings.SendNewEmail(mailTo, "<h1 style='color:#555'>" + notificationName + "</h1>", subject, message.Trim());
                }
            }
        }

        private void addNotification(string notifiID, string message, bool addFlag, string date = "") {
            if (date == "")
                date = ServerSettings.ServerDateTime.ToString();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("NotificationID", notifiID));
            query.Add(new DatabaseQuery("UserName", username));
            query.Add(new DatabaseQuery("Message", message));
            query.Add(new DatabaseQuery("Dismissed", "0"));
            query.Add(new DatabaseQuery("Date", date));

            if (dbCall.CallInsert("aspnet_UserNotificationMessages", query)) {
                if (addFlag) {
                    _uuf.addFlag(username, "workspace", "");
                }
            }
        }

        public List<UserNotificationsMessage_Coll> getNonDismissedEntries(string sortDir = "DESC") {
            List<UserNotificationsMessage_Coll> _messages = new List<UserNotificationsMessage_Coll>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotificationMessages", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date " + sortDir);
            foreach (Dictionary<string, string> row in dbSelect) {
                if (!HelperMethods.ConvertBitToBoolean(row["Dismissed"])) {
                    string id = row["ID"];
                    string nId = row["NotificationID"];
                    string message = HttpUtility.UrlDecode(row["Message"]);
                    string dismissed = row["Dismissed"];
                    string date = row["Date"];
                    UserNotificationsMessage_Coll coll = new UserNotificationsMessage_Coll(id, nId, message, dismissed, date);
                    _messages.Add(coll);
                }
            }

            return _messages;
        }

        public List<UserNotificationsMessage_Coll> getEntries(string sortDir = "DESC") {
            List<UserNotificationsMessage_Coll> _messages = new List<UserNotificationsMessage_Coll>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotificationMessages", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date " + sortDir);
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string nId = row["NotificationID"];
                string message = HttpUtility.UrlDecode(row["Message"]);
                string dismissed = row["Dismissed"];
                string date = row["Date"];
                UserNotificationsMessage_Coll coll = new UserNotificationsMessage_Coll(id, nId, message, dismissed, date);
                _messages.Add(coll);
            }

            return _messages;
        }

        public List<string> getMessagesByNotificationID(string notificationID) {
            List<string> _messages = new List<string>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotificationMessages", "Message", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("NotificationID", notificationID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date DESC");
            foreach (Dictionary<string, string> row in dbSelect) {
                _messages.Add(HttpUtility.UrlDecode(row["Message"]));
            }

            return _messages;
        }

        public void deleteAllUserNotification() {
            if (dbCall.CallDelete("aspnet_UserNotificationMessages", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) })) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void dismissAllUserNotification() {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", username));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Dismissed", "1"));

            if (dbCall.CallUpdate("aspnet_UserNotificationMessages", updateQuery, query)) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void deleteNotification(string id) {
            if (dbCall.CallDelete("aspnet_UserNotificationMessages", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) })) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void dismissNotification(string id) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", username));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Dismissed", "1"));

            if (dbCall.CallUpdate("aspnet_UserNotificationMessages", updateQuery, query)) {
                _uuf.addFlag(username, "workspace", "");
            }
        }

        public void deleteNotification_All() {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            dbCall.CallDelete("aspnet_UserNotificationMessages", query);
        }
    }

}