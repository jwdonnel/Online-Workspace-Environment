#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using System.Linq;
using System.Web;

#endregion

namespace OpenWSE_Tools.GroupOrganizer {

    /// <summary>
    ///     Summary description for Companies
    /// </summary>
    [Serializable]
    public class Groups {
        private readonly DatabaseCall dbCall = new DatabaseCall();
        private string username;
        private List<Dictionary<string, string>> dataTable;

        public Groups() { }

        public Groups(string username) {
            this.username = username;
        }

        public List<Dictionary<string, string>> group_dt {
            get { return dataTable; }
        }

        public void addGroup(string id, string groupname, string description, string image, string date, bool isurl, bool isPrivate) {
            int url = 0;
            if (isurl) {
                url = 1;
            }

            int _private = 0;
            if (isPrivate) {
                _private = 1;
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("GroupID", id));
            query.Add(new DatabaseQuery("GroupName", groupname));
            query.Add(new DatabaseQuery("Description", description));
            query.Add(new DatabaseQuery("Date", date));
            query.Add(new DatabaseQuery("CreatedBy", username.ToLower()));
            query.Add(new DatabaseQuery("Image", image));
            query.Add(new DatabaseQuery("IsURL", url.ToString()));
            query.Add(new DatabaseQuery("IsPrivate", _private.ToString()));

            if (dbCall.CallInsert("GroupList", query)) {
                NewUserDefaults newUserDefaults = new NewUserDefaults(id);
                newUserDefaults.GetDefaults();
            }
        }

        public void getEntries() {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            dataTable = dbCall.CallSelect("GroupList", "", query, "GroupName ASC");

            try {
                List<int> removeAtList = new List<int>();
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                    string currLoginGroup = GroupSessions.GetUserGroupSessionName(username);
                    for (int i = 0; i < dataTable.Count; i++) {
                        if (dataTable[i]["GroupID"].ToString() != currLoginGroup) {
                            removeAtList.Add(i);
                        }
                    }

                    removeAtList.Sort();
                    removeAtList.Reverse();
                    foreach (int ra in removeAtList) {
                        dataTable.RemoveAt(ra);
                    }
                }
                else {
                    GroupIPListener groupIplistener = new GroupIPListener();
                    for (int i = 0; i < dataTable.Count; i++) {
                        if (!groupIplistener.CheckGroupNetwork(dataTable[i]["GroupID"].ToString())) {
                            removeAtList.Add(i);
                        }
                    }

                    removeAtList.Sort();
                    removeAtList.Reverse();
                    foreach (int ra in removeAtList) {
                        dataTable.RemoveAt(ra);
                    }
                }
            }
            catch { }
        }

        public void getEntriesForGroupOrgPage() {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            dataTable = dbCall.CallSelect("GroupList", "", query, "GroupName ASC");

            try {
                List<int> removeAtList = new List<int>();
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                    string currLoginGroup = GroupSessions.GetUserGroupSessionName(username);
                    for (int i = 0; i < dataTable.Count; i++) {
                        if (dataTable[i]["GroupID"].ToString() != currLoginGroup) {
                            removeAtList.Add(i);
                        }
                    }

                    removeAtList.Sort();
                    removeAtList.Reverse();
                    foreach (int ra in removeAtList) {
                        dataTable.RemoveAt(ra);
                    }
                }
                else {
                    GroupIPListener groupIplistener = new GroupIPListener();
                    for (int i = 0; i < dataTable.Count; i++) {
                        if (!string.IsNullOrEmpty(username) && dataTable[i]["CreatedBy"].ToString().ToLower() == username.ToLower()) {
                            continue;
                        }

                        if (!groupIplistener.CheckGroupNetwork(dataTable[i]["GroupID"].ToString())) {
                            removeAtList.Add(i);
                        }
                    }

                    removeAtList.Sort();
                    removeAtList.Reverse();
                    foreach (int ra in removeAtList) {
                        dataTable.RemoveAt(ra);
                    }
                }
            }
            catch { }
        }

        public List<string> GetEntryList() {
            List<string> gs = new List<string>();
            string currLoginGroup = string.Empty;

            try {
                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                    currLoginGroup = GroupSessions.GetUserGroupSessionName(username);
                }
            }
            catch { }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("GroupList", "GroupID", query, "GroupName ASC");
            foreach (Dictionary<string, string> row in dbSelect) {
                if ((string.IsNullOrEmpty(currLoginGroup)) || (currLoginGroup == row["GroupID"])) {
                    GroupIPListener groupIplistener = new GroupIPListener();
                    if (groupIplistener.CheckGroupNetwork(row["GroupID"])) {
                        gs.Add(row["GroupID"]);
                    }
                }
            }

            return gs;
        }

        public static string RemoveUserFromGroupList(List<string> membergrouplist, string groupname) {
            return membergrouplist.Where(t => t != groupname).Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
        }

        public static string RemoveUserFromGroupList(MemberDatabase member, string groupname) {
            List<string> groups = member.GroupList;
            return groups.Where(t => t != groupname).Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
        }

        public void getEntries(string groupId) {
            dataTable = new List<Dictionary<string, string>>();

            GroupIPListener groupIplistener = new GroupIPListener();
            if (!groupIplistener.CheckGroupNetwork(groupId)) {
                return;
            }

            Guid outGuid = new Guid();
            if (!Guid.TryParse(groupId, out outGuid)) {
                return;
            }

            dataTable = dbCall.CallSelect("GroupList", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", groupId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "GroupName ASC");
        }

        public void getEntriesForGroupOrgPage(string groupId) {
            dataTable = new List<Dictionary<string, string>>();

            if (string.IsNullOrEmpty(username) || GetOwner(groupId).ToLower() != username.ToLower()) {
                GroupIPListener groupIplistener = new GroupIPListener();
                if (!groupIplistener.CheckGroupNetwork(groupId)) {
                    return;
                }
            }

            Guid outGuid = new Guid();
            if (!Guid.TryParse(groupId, out outGuid)) {
                return;
            }

            dataTable = dbCall.CallSelect("GroupList", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", groupId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "GroupName ASC");
        }

        public Dictionary<string, string> GetGroupDefaults(string groupId) {
            NewUserDefaults newUserDefaults = new NewUserDefaults(groupId);
            newUserDefaults.GetDefaults();
            return newUserDefaults.DefaultTable;
        }

        public string GetGroupName_byID(string id) {
            if (id.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                return ServerSettings.AdminUserName.ToLower();
            }

            Guid tempGuid = new Guid();
            if (!Guid.TryParse(id, out tempGuid)) {
                id = new MemberDatabase(HttpContext.Current.User.Identity.Name).FixGroupNameColumn(id);
            }

            DatabaseQuery dbSelect = dbCall.CallSelectSingle("GroupList", "GroupName", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        public string GetOwner(string id) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("GroupList", "CreatedBy", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        public string GetGroupImg_byID(string id) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("GroupList", "Image, IsURL", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            if (dbSelect.Count > 0) {
                string temp = dbSelect[0]["Image"].Trim();
                if (!HelperMethods.ConvertBitToBoolean(dbSelect[0]["IsURL"])) {
                    if (temp.StartsWith("~/")) {
                        temp = VirtualPathUtility.ToAbsolute(temp);
                    }
                    else if (temp.ToLower() == "default-group.png") {
                        temp = VirtualPathUtility.ToAbsolute("~/Standard_Images/Groups/Logo/" + temp);
                    }
                }
                else {
                    temp = HelperMethods.RemoveProtocolFromUrl(temp);
                }

                return temp;
            }

            return string.Empty;
        }

        public void UpdateItem(string id, string groupname, string description, string image, string date, bool isurl, bool isPrivate) {

            int url = 0;
            if (isurl) {
                url = 1;
            }

            int _private = 0;
            if (isPrivate) {
                _private = 1;
            }

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("GroupName", groupname));
            updateQuery.Add(new DatabaseQuery("Description", description));
            updateQuery.Add(new DatabaseQuery("Date", date));
            updateQuery.Add(new DatabaseQuery("CreatedBy", username.ToLower()));
            updateQuery.Add(new DatabaseQuery("Image", image));
            updateQuery.Add(new DatabaseQuery("IsURL", url.ToString()));
            updateQuery.Add(new DatabaseQuery("IsPrivate", _private.ToString()));

            dbCall.CallUpdate("GroupList", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        }

        public void deleteGroup(string id) {
            dbCall.CallDelete("GroupList", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            DeleteDefaults(id);
        }

        public void deleteGroup(Guid id, string groupname) {
            dbCall.CallDelete("GroupList", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname), new DatabaseQuery("GroupID", id.ToString()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            DeleteDefaults(id.ToString());
        }

        private void DeleteDefaults(string id) {
            NewUserDefaults newUserDefaults = new NewUserDefaults(id);
            newUserDefaults.DeleteDefault();
            Notifications.UserNotifications noti = new Notifications.UserNotifications();
            noti.DeleteAllUserNotification(id);
            Overlays.WorkspaceOverlays overlays = new Overlays.WorkspaceOverlays();
            overlays.DeleteAllUserOverlays(id);
        }

        public bool IsApartOfGroup(List<string> usergrouplist, string groupId) {
            string g = usergrouplist.Find(x => x == groupId);
            if (!string.IsNullOrEmpty(g)) {
                return true;
            }

            return false;
        }

        public List<string> GetMembers_of_Group(string group) {
            var temp = new List<string>();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser u in coll) {
                var temp_member = new MemberDatabase(u.UserName);
                if (IsApartOfGroup(temp_member.GroupList, group)) {
                    temp.Add(u.UserName);
                }
            }

            return temp;
        }
    }

}