using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlServerCe;

namespace OpenWSE_Tools.Notifications {

    [Serializable]
    public struct Notifications_Coll {
        private readonly string _id;
        private readonly string _notifiName;
        private readonly string _notifiImg;
        private readonly string _username;
        private readonly string _description;

        public Notifications_Coll(string id, string notifiName, string notifiImg, string username, string description) {
            _id = id;
            _notifiName = notifiName;
            _notifiImg = notifiImg;
            _username = username;
            _description = description;
        }

        public string ID {
            get { return _id; }
        }

        public string NotificationName {
            get { return _notifiName; }
        }

        public string NotificationImage {
            get { return _notifiImg; }
        }

        public string UserName {
            get { return _username; }
        }

        public string Description {
            get { return _description; }
        }
    }

    [Serializable]
    public struct UserNotifications_Coll {
        private readonly string _notifiId;
        private readonly bool _email;

        public UserNotifications_Coll(string notifiId, bool email) {
            _notifiId = notifiId;
            _email = email;
        }

        public string NotificationID {
            get { return _notifiId; }
        }

        public bool Email {
            get { return _email; }
        }
    }

    public class Notifications {

        #region Private Variables

        private readonly List<Notifications_Coll> _notifiList = new List<Notifications_Coll>();
        private readonly List<UserNotifications_Coll> _userNotifi = new List<UserNotifications_Coll>();
        private readonly DatabaseCall dbCall = new DatabaseCall();

        #endregion


        #region Constructor

        public Notifications() { }

        #endregion


        #region Main Notifications

        public void GetNotifications() {
            _notifiList.Clear();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("Notifications", "", null, "NotificationName ASC");
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string nn = row["NotificationName"];
                string ni = row["NotificationImage"];
                string un = row["UserName"];
                string des = row["Description"];
                Notifications_Coll coll = new Notifications_Coll(id, nn, ni, un, des);
                _notifiList.Add(coll);
            }
        }

        public Notifications_Coll GetNotification(string _id) {
            Notifications_Coll coll = new Notifications_Coll();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("Notifications", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", _id) });
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string nn = row["NotificationName"];
                string ni = row["NotificationImage"];
                string un = row["UserName"];
                string des = row["Description"];
                coll = new Notifications_Coll(id, nn, ni, un, des);
                break;
            }

            return coll;
        }

        public List<Notifications_Coll> NotificationList {
            get { return _notifiList; }
        }

        public string AddNotification(string username, string notifiName, string notifiImg, string desc, string id = "") {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", id));
            query.Add(new DatabaseQuery("NotificationName", notifiName));
            query.Add(new DatabaseQuery("NotificationImage", notifiImg));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));
            query.Add(new DatabaseQuery("Description", desc));

            dbCall.CallInsert("Notifications", query);

            return id;
        }

        public void UpdateNotificationName(string id, string name) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("NotificationName", name));

            dbCall.CallUpdate("Notifications", updateQuery, query);
        }

        public void UpdateNotificationDescription(string id, string desc) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Description", desc));

            dbCall.CallUpdate("Notifications", updateQuery, query);
        }

        public void UpdateNotificationImg(string id, string img) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("NotificationImage", img));

            dbCall.CallUpdate("Notifications", updateQuery, query);
        }

        public void DeleteNotification(string id) {
            dbCall.CallDelete("Notifications", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        }

        #endregion


        #region User Notifications

        public static bool CheckIfErrorNotificationIsOn(string userName) {
            if (System.Web.Security.Roles.IsUserInRole(userName, ServerSettings.AdminUserName)) {
                Notifications _notifi = new Notifications();
                Notifications_Coll coll = _notifi.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
                if (!string.IsNullOrEmpty(coll.ID)) {
                    _notifi.GetUserNotifications(userName);
                    foreach (UserNotifications_Coll un in _notifi.UserNotifications) {
                        if (un.NotificationID == coll.ID) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void GetUserNotifications(string userName) {
            _userNotifi.Clear();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotifications", "NotificationID, Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName) });
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
        }

        public void AddUserNotification(string username, string notifiId, bool email) {
            string _email = "0";
            if (email)
                _email = "1";

            List<DatabaseQuery> query = new List<DatabaseQuery>();
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
            query.Add(new DatabaseQuery("NotificationID", id));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Email", _email));

            dbCall.CallUpdate("aspnet_UserNotifications", updateQuery, query);
        }

        public bool IsUserNotificationEnabled(string username, string notifiId) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserNotifications", "Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("NotificationID", notifiId) });
            if (dbSelect.Count > 0) {
                return true;
            }

            return false;
        }

        public string IsEmailNotificationEnabled(string username, string notifiId) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_UserNotifications", "Email", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("NotificationID", notifiId) });
            return dbSelect.Value;
        }

        public void DeleteUserNotification(string username, string notifiId) {
            dbCall.CallDelete("aspnet_UserNotifications", new List<DatabaseQuery>() { new DatabaseQuery("NotificationID", notifiId), new DatabaseQuery("UserName", username.ToLower()) });
        }

        public List<UserNotifications_Coll> UserNotifications {
            get { return _userNotifi; }
        }

        #endregion

    }

}