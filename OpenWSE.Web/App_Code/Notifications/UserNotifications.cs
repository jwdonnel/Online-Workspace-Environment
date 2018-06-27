using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace OpenWSE_Tools.Notifications {

    [Serializable]
    public struct UserNotifications_Coll {
        private readonly string _notifiId;
        private readonly bool _email;

        public UserNotifications_Coll(string notifiId, bool email) {
            _notifiId = notifiId;

            Guid tempGuid = new Guid();
            if (Guid.TryParse(_notifiId, out tempGuid)) {
                _notifiId = _notifiId.ToLower();
            }

            _email = email;
        }

        public string NotificationID {
            get { return _notifiId; }
        }

        public bool Email {
            get { return _email; }
        }
    }

    public class UserNotifications {

        #region Private Variables

        private readonly DatabaseCall dbCall = new DatabaseCall();

        #endregion


        #region Public Constants

        public const string ErrorReportingID = "ErrorReporting";
        public const string GroupAlertID = "GroupAlerts";

        #endregion


        #region Constructor

        public UserNotifications() { }

        #endregion


        #region Notification Methods

        public static string GetNotificationIcon(string id, string theme) {
            if (string.IsNullOrEmpty(theme)) {
                theme = "Standard";
            }

            switch (id) {
                case UserNotifications.ErrorReportingID:
                    return "~/App_Themes/" + theme + "/Icons/Notifications/error.png";

                case UserNotifications.GroupAlertID:
                    return "~/App_Themes/" + theme + "/Icons/Notifications/group.png";

                default:
                    if (id.StartsWith("app-")) {
                        return "~/" + new App(string.Empty).GetAppIconName(id);
                    }
                    break;
            }

            return string.Empty;

        }
        public static string GetNotificationName(string id) {
            switch (id) {
                case UserNotifications.ErrorReportingID:
                    return "Error Report";

                case UserNotifications.GroupAlertID:
                    return "Group Alert";

                default:
                    if (id.StartsWith("app-")) {
                        return new App(string.Empty).GetAppName(id);
                    }
                    break;
            }

            return id;
        }
        public static string GetNotificationDescription(string id) {
            switch (id) {
                case UserNotifications.ErrorReportingID:
                    return "Alerts when an error occurs on the site. (For Administrators only)";

                case UserNotifications.GroupAlertID:
                    return "Alerts you when you've been added/removed or invited to a group. (If turned off, you will not get invites to groups)";

                default:
                    if (id.StartsWith("app-")) {
                        Apps_Coll info = new App(string.Empty).GetAppInformation(id);
                        if (info != null) {
                            return info.Description;
                        }
                    }
                    break;
            }

            return string.Empty;
        }
        public static string GetCorrectNotificationImage(string description, string id, string originalImage) {
            if (id == UserNotifications.ErrorReportingID) {
                if (description.Contains("CPU Usage Alert")) {
                    originalImage = originalImage.Replace("error.png", "cpu.png");
                }
                else if (description.Contains("Memory Usage Alert")) {
                    originalImage = originalImage.Replace("error.png", "memory.png");
                }
            }
            return originalImage;
        }

        #endregion


        #region User Notifications

        public List<UserNotifications_Coll> GetUserNotifications(string userName) {
            List<UserNotifications_Coll> _userNotifi = new List<UserNotifications_Coll>();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotifications", "NotificationID, Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["NotificationID"];
                string email = row["Email"];
                bool _email = false;
                if ((string.IsNullOrEmpty(email)) || (HelperMethods.ConvertBitToBoolean(email)))
                    _email = true;

                var coll = new UserNotifications_Coll(id, _email);
                if (!_userNotifi.Contains(coll))
                    _userNotifi.Add(coll);
            }

            return _userNotifi;
        }

        public void AddUserNotification(string username, string notifiId, bool email) {
            string _email = "0";
            if (email)
                _email = "1";

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));
            query.Add(new DatabaseQuery("NotificationID", notifiId));
            query.Add(new DatabaseQuery("Email", _email));

            dbCall.CallInsert("aspnet_UserNotifications", query);
        }
        public void UpdateUserNotificationEmail(string id, bool email, string username) {
            string _email = "0";
            if (email)
                _email = "1";

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("NotificationID", id));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Email", _email));

            dbCall.CallUpdate("aspnet_UserNotifications", updateQuery, query);
        }

        public bool IsUserNotificationEnabled(string username, string notifiId) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotifications", "Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("NotificationID", notifiId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            if (dbSelect.Count > 0) {
                return true;
            }

            return false;
        }
        public bool IsEmailNotificationEnabled(string username, string notifiId) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotifications", "Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("NotificationID", notifiId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            if (dbSelect.Count > 0) {
                return HelperMethods.ConvertBitToBoolean(dbSelect[0]["Email"]);
            }

            return false;
        }
        public static bool CheckIfErrorNotificationIsOn(string userName) {
            if (System.Web.Security.Roles.IsUserInRole(userName, ServerSettings.AdminUserName)) {
                ServerSettings serverSettings = new ServerSettings();
                if (serverSettings.RecordActivity) {
                    UserNotifications _notifi = new UserNotifications();
                    List<UserNotifications_Coll> coll = _notifi.GetUserNotifications(userName);
                    foreach (UserNotifications_Coll un in coll) {
                        if (un.NotificationID == UserNotifications.ErrorReportingID) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void DeleteUserNotification(string username, string notifiId) {
            dbCall.CallDelete("aspnet_UserNotifications", new List<DatabaseQuery>() { new DatabaseQuery("NotificationID", notifiId), new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        }
        public void DeleteAllUserNotification(string username) {
            dbCall.CallDelete("aspnet_UserNotifications", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        }

        private static Regex _htmlAlertRegex = new Regex("<.*?>", RegexOptions.Compiled);
        public static string TrimBrowserPopupMessage(string message) {
            if (string.IsNullOrEmpty(message)) {
                return string.Empty;
            }

            message = _htmlAlertRegex.Replace(message, " ");
            if (message.Length > 150) {
                message = message.Substring(0, 150) + "...";
            }

            return message.Trim();
        }

        #endregion

    }

}