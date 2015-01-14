#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web.Security;
using System.Linq;
using System.Web;
using System.Data.SqlServerCe;

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

        public void addGroup(string id, string groupname, string address, string city, string state, string country, string postalcode,
                             string phonenumber, string image, string date, bool isurl, bool isPrivate) {
            int url = 0;
            if (isurl) {
                url = 1;
            }

            int _private = 0;
            if (isPrivate) {
                _private = 1;
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("GroupID", id));
            query.Add(new DatabaseQuery("GroupName", groupname));
            query.Add(new DatabaseQuery("Address", address));
            query.Add(new DatabaseQuery("City", city));
            query.Add(new DatabaseQuery("State", state));
            query.Add(new DatabaseQuery("Country", country));
            query.Add(new DatabaseQuery("PostalCode", postalcode));
            query.Add(new DatabaseQuery("PhoneNumber", phonenumber));
            query.Add(new DatabaseQuery("Date", date));
            query.Add(new DatabaseQuery("CreatedBy", username.ToLower()));
            query.Add(new DatabaseQuery("Image", image));
            query.Add(new DatabaseQuery("IsURL", url.ToString()));
            query.Add(new DatabaseQuery("IsPrivate", _private.ToString()));

            dbCall.CallInsert("GroupList", query);
        }

        public void getEntries() {
            dataTable = dbCall.CallSelect("GroupList", "", null, "GroupName ASC");

            try {
                List<int> removeAtList = new List<int>();
                if (HttpContext.Current.Session["LoginGroup"] != null) {
                    string currLoginGroup = HttpContext.Current.Session["LoginGroup"].ToString();
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

        public List<string> GetEntryList() {
            List<string> gs = new List<string>();
            string currLoginGroup = string.Empty;

            try {
                if (HttpContext.Current.Session["LoginGroup"] != null) {
                    currLoginGroup = HttpContext.Current.Session["LoginGroup"].ToString();
                }
            }
            catch { }

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("GroupList", "GroupID", null, "GroupName ASC");
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

            dataTable = dbCall.CallSelect("GroupList", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", groupId) }, "GroupName ASC");
        }

        public string GetGroupName_byID(string id) {
            Guid tempGuid = new Guid();
            if (!Guid.TryParse(id, out tempGuid)) {
                id = new MemberDatabase(HttpContext.Current.User.Identity.Name).FixGroupNameColumn(id);
            }

            DatabaseQuery dbSelect = dbCall.CallSelectSingle("GroupList", "GroupName", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id) });
            return dbSelect.Value;
        }

        public string GetOwner(string id) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("GroupList", "CreatedBy", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id) });
            return dbSelect.Value;
        }

        public string GetGroupImg_byID(string id) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("GroupList", "Image, IsURL", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id) });
            if (dbSelect.Count > 0) {
                string temp = dbSelect[0]["Image"].Trim();
                if (!HelperMethods.ConvertBitToBoolean(dbSelect[0]["IsURL"])) {
                    if (temp.StartsWith("~/")) {
                        temp = VirtualPathUtility.ToAbsolute(temp);
                    }
                }

                return temp;
            }

            return string.Empty;
        }

        public void UpdateItem(string id, string groupname, string address, string city, string state, string country, string postalcode, string phonenumber, string image, string date, bool isurl, bool isPrivate) {

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
            updateQuery.Add(new DatabaseQuery("Address", address));
            updateQuery.Add(new DatabaseQuery("City", city));
            updateQuery.Add(new DatabaseQuery("State", state));
            updateQuery.Add(new DatabaseQuery("Country", country));
            updateQuery.Add(new DatabaseQuery("PostalCode", postalcode));
            updateQuery.Add(new DatabaseQuery("PhoneNumber", phonenumber));
            updateQuery.Add(new DatabaseQuery("Date", date));
            updateQuery.Add(new DatabaseQuery("CreatedBy", username.ToLower()));
            updateQuery.Add(new DatabaseQuery("Image", image));
            updateQuery.Add(new DatabaseQuery("IsURL", url.ToString()));
            updateQuery.Add(new DatabaseQuery("IsPrivate", _private.ToString()));

            dbCall.CallUpdate("GroupList", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id) });
        }

        public void deleteGroup_byGroupName(string groupname) {
            dbCall.CallDelete("GroupList", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname) });
        }

        public void deleteGroup(string id) {
            dbCall.CallDelete("GroupList", new List<DatabaseQuery>() { new DatabaseQuery("GroupID", id) });
        }

        public void deleteGroup(Guid id, string groupname) {
            dbCall.CallDelete("GroupList", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname), new DatabaseQuery("GroupID", id.ToString()) });
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