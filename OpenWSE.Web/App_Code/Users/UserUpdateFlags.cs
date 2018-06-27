#region

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using System.Web;
using System.Collections.Generic;

#endregion

namespace OpenWSE_Tools.AutoUpdates {

    /// <summary>
    ///     Summary description for UserUpdateFlags
    /// </summary>
    public class UserUpdateFlags {
        private static List<Dictionary<string, string>> UserUpdateFlagTable = new List<Dictionary<string, string>>();

        public UserUpdateFlags() {
            if (UserUpdateFlagTable == null) {
                UserUpdateFlagTable = new List<Dictionary<string, string>>();
            }
        }

        /// <summary>
        /// Updates all users with the given app and groupname
        /// </summary>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <param name="groupname">The groupname. Leave blank if none.</param>
        /// <param name="updateForCurrUser">Update the current user</param>
        public void addFlag(string appID, string groupname, bool updateForCurrUser = true) {
            try {
                if (string.IsNullOrEmpty(appID)) {
                    appID = "workspace";
                }

                string currUser = HttpContext.Current.User.Identity.Name.ToLower();

                MembershipUserCollection coll = Membership.GetAllUsers();
                foreach (MembershipUser u in coll) {
                    if (u == null || (!updateForCurrUser && u.UserName.ToLower() == currUser))
                        continue;

                    if (u.IsOnline) {
                        List<string> userSessionIds = MemberDatabase.GetUserSessionIds(u.UserName);
                        foreach (string session in userSessionIds) {
                            string check = getFlag_SessionID(u.UserName, appID, session);

                            if ((appID != "workspace") && (u.UserName.ToLower() == ServerSettings.AdminUserName.ToLower()) && (!appID.Contains("app-")))
                                check = "";
                            else if ((u.UserName.ToLower() == ServerSettings.AdminUserName.ToLower()) && (appID.Contains("app-")))
                                check = "-";

                            if (check == "") {
                                Dictionary<string, string> query = new Dictionary<string, string>();
                                query.Add(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID);
                                query.Add("ID", Guid.NewGuid().ToString());
                                query.Add("SessionID", session);
                                query.Add("UserName", u.UserName.ToLower());
                                query.Add("AppID", appID);
                                query.Add("GroupName", groupname);

                                UserUpdateFlagTable.Add(query);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        /// <summary>
        /// Adds a flag to the given user to update their current state
        /// </summary>
        /// <param name="username"></param>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <param name="groupname">The groupname. Leave blank if none.</param>
        public void addFlag(string username, string appID, string groupname) {
            try {
                MembershipUser m = Membership.GetUser(username);
                if (m != null && m.IsOnline) {
                    List<string> userSessionIds = MemberDatabase.GetUserSessionIds(username);
                    foreach (string session in userSessionIds) {
                        string check = getFlag_SessionID(m.UserName, appID, session);

                        if ((appID != "workspace") && (username.ToLower() == ServerSettings.AdminUserName.ToLower())) {
                            check = "";
                        }

                        if (check == "") {
                            if (string.IsNullOrEmpty(appID)) {
                                appID = "workspace";
                            }

                            Dictionary<string, string> query = new Dictionary<string, string>();
                            query.Add(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID);
                            query.Add("ID", Guid.NewGuid().ToString());
                            query.Add("SessionID", session);
                            query.Add("UserName", username.ToLower());
                            query.Add("AppID", appID);
                            query.Add("GroupName", groupname);

                            UserUpdateFlagTable.Add(query);
                        }
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        /// <summary>
        /// Get all user flags only if user is in admin door
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetAllFlags(string username) {
            if (BasePage.IsUserInAdminRole(username)) {
                return UserUpdateFlagTable.FindAll(item => item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID);
            }

            return new List<Dictionary<string, string>>();
        }

        /// <summary>
        /// Gets the update flag for the given user for the workspace
        /// </summary>
        /// <param name="username">Any given user in the database</param>
        /// <returns></returns>
        public string getFlag(string username) {
            try {
                Dictionary<string, string> result = new Dictionary<string, string>();
                string sessionId = MemberDatabase.GetUserSessionId(username);
                if (string.IsNullOrEmpty(sessionId)) {
                    result = UserUpdateFlagTable.Find(item =>
                        item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                        item["UserName"] == username.ToLower() &&
                        item["AppID"] == "workspace");
                }
                else {
                    result = UserUpdateFlagTable.Find(item =>
                        item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                        item["SessionID"] == sessionId &&
                        item["UserName"] == username.ToLower() &&
                        item["AppID"] == "workspace");
                }

                if (result != null && result.Count > 0 && result.ContainsKey("ID")) {
                    return result["ID"];
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the update flag for the given user for a app
        /// </summary>
        /// <param name="username">Any given user in the database</param>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <returns></returns>
        public string getFlag(string username, string appID) {
            if (!string.IsNullOrEmpty(appID)) {
                try {
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    string sessionId = MemberDatabase.GetUserSessionId(username);
                    if (string.IsNullOrEmpty(sessionId)) {
                        result = UserUpdateFlagTable.Find(item =>
                            item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                            item["UserName"] == username.ToLower() &&
                            item["AppID"] == appID);
                    }
                    else {
                        result = UserUpdateFlagTable.Find(item =>
                            item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                            item["SessionID"] == sessionId &&
                            item["UserName"] == username.ToLower() &&
                            item["AppID"] == appID);
                    }

                    if (result != null && result.Count > 0 && result.ContainsKey("ID")) {
                        return result["ID"];
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the update flag for the given user on a given workspace for a given group
        /// </summary>
        /// <param name="username">Any given user in the database</param>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <param name="groupname">The groupname. Leave blank if none.</param>
        /// <returns></returns>
        public string getFlag(string username, string appID, string groupname) {
            try {
                Dictionary<string, string> result = new Dictionary<string, string>();
                string sessionId = MemberDatabase.GetUserSessionId(username);
                if (string.IsNullOrEmpty(sessionId)) {
                    result = UserUpdateFlagTable.Find(item =>
                        item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                        item["UserName"] == username.ToLower() &&
                        item["AppID"] == appID &&
                        item["GroupName"] == groupname);
                }
                else {
                    result = UserUpdateFlagTable.Find(item =>
                        item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                        item["SessionID"] == sessionId &&
                        item["UserName"] == username.ToLower() &&
                        item["AppID"] == appID &&
                        item["GroupName"] == groupname);
                }

                if (result != null && result.Count > 0 && result.ContainsKey("ID")) {
                    return result["ID"];
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the update flag for the given user for a app from the given SessionID
        /// </summary>
        /// <param name="username">Any given user in the database</param>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <returns></returns>
        public string getFlag_SessionID(string username, string appID, string sessionId) {
            if (!string.IsNullOrEmpty(appID)) {
                try {
                    Dictionary<string, string> result = UserUpdateFlagTable.Find(item =>
                            item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                            item["SessionID"] == sessionId &&
                            item["UserName"] == username.ToLower() &&
                            item["AppID"] == appID);

                    if (result != null && result.Count > 0 && result.ContainsKey("ID")) {
                        return result["ID"];
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the update flag for the given user for a app without using the SessionID
        /// </summary>
        /// <param name="username">Any given user in the database</param>
        /// <param name="appID">The app to update. Use "workspace" to update everything.</param>
        /// <returns></returns>
        public string getFlag_NoSessionID(string username, string appID) {
            if (!string.IsNullOrEmpty(appID)) {
                try {
                    Dictionary<string, string> result = UserUpdateFlagTable.Find(item =>
                            item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                            item["UserName"] == username.ToLower() &&
                            item["AppID"] == appID);

                    if (result != null && result.Count > 0 && result.ContainsKey("ID")) {
                        return result["ID"];
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }

            return string.Empty;
        }

        public string getFlag_AppID(string id) {
            if (!string.IsNullOrEmpty(id) && id.ToLower() != "refresh" && id.ToLower() != "undefined" && id.ToLower() != "request format is invalid: .\r\n") {
                try {
                    Dictionary<string, string> result = UserUpdateFlagTable.Find(item =>
                            item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID &&
                            item["ID"] == id);

                    if (result != null && result.Count > 0 && result.ContainsKey("AppID")) {
                        return result["AppID"];
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
            return string.Empty;
        }

        public void deleteFlag(string id) {
            try {
                UserUpdateFlagTable.RemoveAll(item => item["ID"] == id && item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID);
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        public void deleteFlag_User_And_SessionId(string sessionId, string username) {
            try {
                UserUpdateFlagTable.RemoveAll(item => item["SessionID"] == sessionId && item["UserName"] == username && item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID);
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        public void deleteAllFlags() {
            try {
                UserUpdateFlagTable.RemoveAll(item => item[DatabaseCall.ApplicationIdString] == ServerSettings.ApplicationID);
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

    }

}