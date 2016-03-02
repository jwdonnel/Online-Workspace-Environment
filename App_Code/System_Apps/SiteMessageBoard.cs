#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using System.Data.SqlServerCe;

#endregion

[Serializable]
public class SiteMessageBoard {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly MemberDatabase member;
    private readonly string username;
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private List<Dictionary<string, string>> dataTable;
    public List<string> groupname;

    public SiteMessageBoard(string username) {
        this.username = username;
        member = new MemberDatabase(username);
        groupname = member.GroupList;
    }

    public List<Dictionary<string, string>> post_dt {
        get { return dataTable; }
    }

    public string addPost(string userFrom, string post, string date, string group) {
        bool setFlags = false;
        string id = Guid.NewGuid().ToString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("UserName", username));
        query.Add(new DatabaseQuery("Post", post));
        query.Add(new DatabaseQuery("Date", date));
        query.Add(new DatabaseQuery("GroupName", group));

        if (dbCall.CallInsert("MessageBoard", query)) {
            setFlags = true;
        }

        if (setFlags) {
            if (!string.IsNullOrEmpty(group)) {
                uuf.addFlag("app-messageboard", group);
                uuf.addFlag("workspace", group);
            }
        }

        return id;
    }

    public void getEntriesByGroup(string groupname) {
        dataTable = dbCall.CallSelect("MessageBoard", "", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Date DESC");
    }

    public void getEntries(string sortDir = "DESC") {
        if (((sortDir.ToLower() != "asc") && (sortDir.ToLower() != "desc")) || (string.IsNullOrEmpty(sortDir))) {
            sortDir = "DESC";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

        dataTable = dbCall.CallSelect("MessageBoard", "", query, "Date " + sortDir);
    }

    public string getGroupName_byMessageID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("MessageBoard", "GroupName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public void updatePostGroup(string id, string groupname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupName", groupname));

        dbCall.CallUpdate("MessageBoard", updateQuery, query);
    }

    public void deletePost(Guid id, string groupname) {
        bool setFlags = false;
        if (dbCall.CallDelete("MessageBoard", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ID", id.ToString()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) })) {
            setFlags = true;
        }

        if (setFlags) {
            if (!string.IsNullOrEmpty(groupname)) {
                uuf.addFlag("app-messageboard", groupname);
                uuf.addFlag("workspace", groupname);
            }
        }
    }

    public void deleteUserPosts(string username) {
        dbCall.CallDelete("MessageBoard", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    private string ActivityTimeConvert(DateTime postDate) {
        string returnVal = "Not Available";
        DateTime now = ServerSettings.ServerDateTime;
        TimeSpan final = now.Subtract(postDate);
        string time;
        if (final.Days > 2) {
            time = postDate.ToShortDateString();
            returnVal = "<span>" + time + "</span>";
        }
        else {
            if (final.Days == 0) {
                if (final.Hours == 0) {
                    if (final.Minutes == 1)
                        time = final.Minutes.ToString() + " minute ago";
                    else if (final.Minutes == 0)
                        time = "Less than a minute ago";
                    else
                        time = final.Minutes.ToString() + " minutes ago";
                    returnVal = "<span>" + time + "</span>";
                }
                else {
                    if (final.Hours == 1)
                        time = final.Hours.ToString() + " hour ago";
                    else
                        time = final.Hours.ToString() + " hours ago";
                    returnVal = "<span>" + time + "</span>";
                }
            }
            else {
                if (final.Days == 1)
                    time = final.Days.ToString() + " day ago";
                else
                    time = final.Days.ToString() + " days ago";
                returnVal = "<span>" + time + "</span>";
            }
        }

        return returnVal;
    }
}