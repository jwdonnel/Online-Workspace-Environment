<%@ WebService Language="C#" Class="MessageBoard" %>
#region

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Data;
using System.Collections.Generic;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;

#endregion

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class MessageBoard : WebService {

    private readonly ServerSettings _ss = new ServerSettings();
    private SiteMessageBoard _siteMessageboard;
    private Groups _groups;
    private readonly MemberDatabase _member;
    private bool _isAuthenticated = false;
    private string _username;
    private const int _maxNum = 1;
    private int _totalNum = 0;

    public MessageBoard() {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _isAuthenticated = true;
            _username = HttpContext.Current.User.Identity.Name;
            _member = new MemberDatabase(_username);
            _groups = new Groups();
            _siteMessageboard = new SiteMessageBoard(_username);
        }
    }

    [WebMethod]
    public object[] AddMessage(string title, string message, string group, string responseToID) {
        if (_isAuthenticated) {
            title = HttpUtility.UrlDecode(title);
            message = HttpUtility.UrlDecode(message);
            _siteMessageboard.AddPost(title, message, group, responseToID);

            if (HasRSSFeed(group)) {
                RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _username);
                rssCreator.Create(group);
            }

            AddNotification(message, group);

            return GetPosts(string.Empty, group, title);
        }

        return new object[0];
    }

    [WebMethod]
    public object LoadDiscussions(string selectedGroup, string searchVal) {
        List<object> returnArray = new List<object>();
        List<Dictionary<string, object>> dataArray = new List<Dictionary<string, object>>();

        searchVal = HttpUtility.UrlDecode(searchVal);

        if (_isAuthenticated) {
            List<Dictionary<string, string>> messageEntries = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(selectedGroup)) {
                messageEntries = _siteMessageboard.GetEntries();
            }
            else {
                messageEntries = _siteMessageboard.GetEntriesByGroup(selectedGroup);
            }

            foreach (Dictionary<string, string> entry in messageEntries) {
                if (string.IsNullOrEmpty(selectedGroup) || _groups.IsApartOfGroup(_member.GroupList, entry["GroupName"])) {
                    if (CheckSearch(searchVal, entry)) {
                        Dictionary<string, object> obj = new Dictionary<string, object>();
                        obj.Add("Title", entry["Title"]);
                        obj.Add("Count", 0);
                        DateTime _date = ServerSettings.ServerDateTime;
                        DateTime.TryParse(entry["Date"], out _date);
                        obj.Add("Date", HelperMethods.GetPrettyDate(_date));
                        obj.Add("DateLong", _date.Ticks);

                        MemberDatabase tempmember = new MemberDatabase(entry["UserName"]);
                        obj.Add("Username", "<span class=\"sort-value-class\" data-sortvalue=\"" + entry["UserName"] + "\"></span>" + UserImageColorCreator.CreateImgColor(tempmember.AccountImage, tempmember.UserColor, tempmember.UserId, 24, tempmember.SiteTheme) + "<span class=\"float-left\" style=\"padding-left: 5px; padding-top: 4px;\">" + HelperMethods.MergeFMLNames(tempmember) + "</span>");

                        if (!ContainsObject(dataArray, entry["Title"])) {
                            dataArray.Add(obj);
                            dataArray[dataArray.Count - 1]["Count"] = GetTotalReplies(entry["ID"], Convert.ToInt32(dataArray[dataArray.Count - 1]["Count"]), entry["Title"]);
                        }
                        else {
                            for (int i = 0; i < dataArray.Count; i++) {
                                try {
                                    if (dataArray[i]["Title"].ToString() == entry["Title"].ToString()) {
                                        var newCount = Convert.ToInt32(dataArray[i]["Count"]);
                                        newCount = newCount + 1;
                                        dataArray[i]["Count"] = newCount;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        returnArray.Add(dataArray);
        returnArray.Add(TableBuilder.ShowRowCountGridViewTable.ToString().ToLower());
        returnArray.Add(TableBuilder.UseAlternateGridviewRows.ToString().ToLower());

        return returnArray;
    }
    private int GetTotalReplies(string id, int currentCount, string title) {
        List<Dictionary<string, string>> messageEntriesResponses = _siteMessageboard.GetResponseEntries(id);
        foreach (Dictionary<string, string> entryResponse in messageEntriesResponses) {
            if (entryResponse["Title"] == title) {
                currentCount = currentCount + 1;
                currentCount = GetTotalReplies(entryResponse["ID"], currentCount, title);
            }
        }

        return currentCount;
    }
    private bool ContainsObject(List<Dictionary<string, object>> array, string title) {
        for (int i = 0; i < array.Count; i++) {
            try {
                if (array[i]["Title"].ToString() == title) {
                    return true;
                }
            }
            catch { }
        }
        return false;
    }
    private bool CheckSearch(string searchVal, Dictionary<string, string> entry) {
        searchVal = searchVal.ToLower();
        if (string.IsNullOrEmpty(searchVal)) {
            return true;
        }

        if (searchVal.Contains(entry["Title"].ToLower()) || entry["Title"].ToLower().Contains(searchVal.ToLower())
            || searchVal.Contains(entry["OriginalPost"].ToLower()) || entry["OriginalPost"].ToLower().Contains(searchVal.ToLower())
            || searchVal.Contains(entry["UserName"].ToLower()) || entry["UserName"].ToLower().Contains(searchVal.ToLower())) {
            return true;
        }

        List<Dictionary<string, string>> messageEntriesResponses = _siteMessageboard.GetResponseEntries(entry["ID"]);
        foreach (Dictionary<string, string> entryResponse in messageEntriesResponses) {
            if (CheckSearch(searchVal, entryResponse)) {
                return true;
            }
        }

        return false;
    }


    [WebMethod]
    public object[] GetPosts(string _currIds, string selectedGroup, string selectedDiscussion) {
        List<object> returnArray = new List<object>();

        if (_isAuthenticated) {
            selectedDiscussion = HttpUtility.UrlDecode(selectedDiscussion);
            List<string> idList = CreateArray(_currIds);
            List<Dictionary<string, string>> messageEntries = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(selectedGroup)) {
                messageEntries = _siteMessageboard.GetEntries();
            }
            else {
                messageEntries = _siteMessageboard.GetEntriesByGroup(selectedGroup);
            }

            foreach (Dictionary<string, string> entry in messageEntries) {
                if (_totalNum >= _maxNum) {
                    break;
                }

                if ((string.IsNullOrEmpty(selectedGroup) || _groups.IsApartOfGroup(_member.GroupList, entry["GroupName"])) && !idList.Contains(entry["ID"])) {
                    if (entry["Title"] == selectedDiscussion) {
                        _totalNum++;

                        Dictionary<string, object> rowDictionary = new Dictionary<string, object>();
                        rowDictionary.Add("post", entry);

                        rowDictionary.Add("responses", AddResponsePosts(entry["ID"], selectedDiscussion));
                        returnArray.Add(rowDictionary);
                    }
                }
            }
        }

        return returnArray.ToArray();
    }
    private object AddResponsePosts(string id, string selectedDiscussion) {
        List<object> responseList = new List<object>();
        List<Dictionary<string, string>> messageEntries = _siteMessageboard.GetResponseEntries(id);

        foreach (Dictionary<string, string> entry in messageEntries) {
            if (entry["Title"] == selectedDiscussion) {
                Dictionary<string, object> rowDictionary = new Dictionary<string, object>();
                rowDictionary.Add("post", entry);

                rowDictionary.Add("responses", AddResponsePosts(entry["ID"], selectedDiscussion));
                responseList.Add(rowDictionary);
            }
        }

        return responseList.ToArray();
    }

    [WebMethod]
    public string DeleteMessage(string id) {
        if (_isAuthenticated) {
            try {
                string groupname = _siteMessageboard.GetGroupNameFromMessage(id);
                _siteMessageboard.DeletePost(HttpUtility.UrlDecode(id), groupname);

                if (HasRSSFeed(groupname)) {
                    RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _username);
                    rssCreator.Create(groupname);
                }

                return "true";
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

        return "false";
    }

    [WebMethod]
    public string FindMessageBoardGroups() {
        if (_isAuthenticated) {
            Groups group = new Groups(_username);
            group.getEntries();
            StringBuilder str = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds";
            int count = 0;
            int feedsAvailable = 0;
            foreach (Dictionary<string, string> row in group.group_dt) {
                string groupName = row["GroupName"];
                string groupID = row["GroupID"];
                if (row["CreatedBy"].ToLower() == _username.ToLower()) {
                    string clickEvent = "messageBoardApp.AddMBFeed('" + groupID + "')";
                    string addRemoveText = "Add";
                    string hasClass = "";
                    string rssUrl = "<span class='float-right' style='font-size: 12px;'>No RSS Feed Created</span>";

                    if (System.IO.File.Exists(rssFeedsLoc + "\\" + groupID + ".rss")) {
                        string url = "MessageBoard/RSS_Feeds/" + groupID + ".rss";
                        string fakeurl = "View RSS Feed";
                        clickEvent = "messageBoardApp.RemoveMBFeed('" + groupID + "')";
                        addRemoveText = "Remove";
                        rssUrl = "<a href='" + ServerSettings.GetSitePath(HttpContext.Current.Request) + "/Apps/" + url + "' target='_blank' class='float-right' style='font-size: 12px;'>" + fakeurl + "</a>";
                        hasClass = "add-item-list-item-hasitem";
                        feedsAvailable++;
                    }

                    str.Append("<div class='add-item-list-item " + hasClass + "' style='cursor: default; position: relative;'>");
                    str.Append("<span style='position: absolute; left: 0; top: 0; bottom: 0; width: 26px; z-index: 1; cursor: pointer!important;' title='" + addRemoveText + " " + groupName + " to RSS list' onclick=\"" + clickEvent + "\"></span><span class='pad-right-big' style='text-decoration: none!important;'>" + groupName + "</span>" + rssUrl);
                    str.Append("</div>");
                    count++;
                }
                else {
                    if (group.IsApartOfGroup(_member.GroupList, groupID)) {
                        string rssUrl = "<span class='float-right' style='font-size: 12px;'>No RSS Feed Created</span>";
                        if (System.IO.File.Exists(rssFeedsLoc + "\\" + groupID + ".rss")) {
                            string url = "MessageBoard/RSS_Feeds/" + groupID + ".rss";
                            string fakeurl = "View RSS Feed";
                            rssUrl = "<a href='" + ServerSettings.GetSitePath(HttpContext.Current.Request) + "/Apps/" + url + "' target='_blank' class='float-right' style='font-size: 12px;'>" + fakeurl + "</a>";
                        }

                        str.Append("<div class='add-item-list-item' style='cursor: default; background-image: none !important;'>");
                        str.Append(groupName + rssUrl);
                        str.Append("</div>");
                        count++;
                    }
                }
            }

            if (count == 0)
                str.Append("<h4 class='pad-all'>No created groups</h4>");

            str2.Append("<div class='pad-all'>Select the groups that you want to have an RSS Feed for. These feeds are automatically updated and can be removed at anytime. You must be the owner of the group to add an RSS feed for.</div>");
            str2.Append("<div class='add-item-list-header'>My Groups<span class='float-right' style='font-weight: normal!important;'>" + feedsAvailable.ToString() + " feeds available</span></div>");
            str.Append("<div class='clear' style='height: 25px;'></div>");
            return str2.ToString() + str.ToString();
        }
        return string.Empty;
    }

    [WebMethod]
    public string AddRSSMB(string _groupName) {
        if (_isAuthenticated) {
            RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _username);
            rssCreator.Create(_groupName);
        }
        return "";
    }

    [WebMethod]
    public string RemoveRSSMB(string _groupName) {
        if (_isAuthenticated) {
            try {
                RSSFeeds feeds = new RSSFeeds(_username.ToLower());
                feeds.BuildEntriesAll();
                foreach (RSSFeeds_Coll feed in feeds.RSSFeedCollection) {
                    if ((feed.URL.Contains(_groupName + ".rss")) && (feed.IsCustomFeed)) {
                        feeds.DeleteRowByID(feed.ID);
                    }
                }

                string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds\\" + _groupName + ".rss";
                if (System.IO.File.Exists(rssFeedsLoc))
                    System.IO.File.Delete(rssFeedsLoc);
            }
            catch { }
        }
        return "";
    }

    [WebMethod]
    public object[] LoadMessageBoardOverlay() {
        List<object> returnArray = new List<object>();
        if (_isAuthenticated) {

            string selectedGroup = GroupSessions.GetUserGroupSessionName(_username);
            if (selectedGroup == _username) {
                selectedGroup = string.Empty;
            }
            
            List<Dictionary<string, object>> dataArray = new List<Dictionary<string, object>>();

            List<Dictionary<string, string>> messageEntries = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(selectedGroup)) {
                messageEntries = _siteMessageboard.GetEntries();
            }
            else {
                messageEntries = _siteMessageboard.GetEntriesByGroup(selectedGroup);
            }

            foreach (Dictionary<string, string> entry in messageEntries) {
                if (string.IsNullOrEmpty(selectedGroup) || _groups.IsApartOfGroup(_member.GroupList, entry["GroupName"])) {
                    return GetPosts(string.Empty, selectedGroup, entry["Title"]);
                }
            }
        }
        
        return returnArray.ToArray();
    }

    private bool HasRSSFeed(string _groupName) {
        string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds\\" + _groupName + ".rss";
        if (System.IO.File.Exists(rssFeedsLoc))
            return true;

        return false;
    }

    private List<string> CreateArray(string list) {
        string[] delims = { "," };
        string[] tempArray = list.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        return new List<string>(tempArray);
    }

    private void AddNotification(string m, string group) {
        var g = new Groups();
        MembershipUserCollection userlist = Membership.GetAllUsers();

        var message = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<b style='padding-right: 5px;'>" + _username + "</b>&bull;<small style='padding-left: 3px;'><i>" + ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture) + "</i></small><div style='clear: both;'></div>");
        messagebody.Append("<b style='padding-right: 5px;'>Group:</b>" + @group + "<br /><br />" + m + "<br /><br />");
        foreach (MembershipUser u in from MembershipUser u in userlist
                                     where (u.UserName.ToLower() != _username.ToLower()) && (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())
                                     let member = new MemberDatabase(u.UserName)
                                     where (g.IsApartOfGroup(member.GroupList, @group))
                                     select u) {
            UserNotificationMessages un = new UserNotificationMessages(u.UserName);
            string email = un.attemptAdd("app-messageboard", messagebody.ToString(), true);
            if (!string.IsNullOrEmpty(email))
                message.To.Add(email);
        }

        UserNotificationMessages.finishAdd(message, "app-messageboard", messagebody.ToString());
    }

}