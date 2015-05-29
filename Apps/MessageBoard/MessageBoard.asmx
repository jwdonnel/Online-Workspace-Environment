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
public class MessageBoard : WebService
{
    private readonly AppLog _applog = new AppLog(false);
    private readonly SiteMessageBoard _siteMessageboard;
    private readonly IIdentity _userId;
    private readonly App _apps = new App(string.Empty);

    private string _username;
    private string _message;
    private const int maxNum = 8;
    
    public MessageBoard()
    {
        _userId = HttpContext.Current.User.Identity;
        _siteMessageboard = new SiteMessageBoard(_userId.Name);
    }

    [WebMethod]
    public string[] AddMessage(string message, string group)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _username = _userId.Name;
            _message = HttpUtility.UrlDecode(message);
            string[] _groups = group.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string _date = DateTime.Now.ToString();

            // Check if message is a link to an article and grab the contents of the url
            if (_message.IndexOf("<p><a href=\"") != -1) {
                try {
                    string _link = _message.Replace("<p><a href=\"", "");
                    string _tempMessage = _message.Replace("<p><a href=\"", "");
                    string _restofMessage = "";
                    if (_tempMessage.IndexOf("\"") > 0) {
                        int indexOf = _tempMessage.IndexOf("\"");
                        int length = Math.Abs(indexOf - (_tempMessage.IndexOf("</p>") + ("</p>").Length));
                        _tempMessage = _tempMessage.Replace(_tempMessage.Substring(indexOf, length), "");

                        _link = _link.Replace(_link.Substring(indexOf), "");
                        _restofMessage = _tempMessage.Replace(_link, "");
                    }

                    Uri uri = new Uri(_link);
                    if (uri.IsAbsoluteUri) {
                        StringBuilder newMessage = new StringBuilder();
                        string _title = HelperMethods.GetWebPageTitle(_link);
                        string _img = HelperMethods.GetWebPageImage(_link);
                        if (!string.IsNullOrEmpty(_title)) {
                            newMessage.Append("<h2><a href='" + _link + "' target='_blank'>" + _title + "</a></h2>");
                            newMessage.Append("<div style='clear: both;'></div>");
                            if (!string.IsNullOrEmpty(_img)) {
                                try {
                                    Uri uri2 = new Uri(_img);
                                    if (uri2.IsAbsoluteUri)
                                        newMessage.Append("<img alt='' src='" + _img + "' style='max-height: 150px;' />");
                                }
                                catch { }
                            }

                            if ((_link.Contains("http://www.youtube.com/watch")) || (_link.Contains("www.youtube.com/watch")))
                                newMessage.Append(ConvertToObjectEmbeded(_link));

                            newMessage.Append("<div style='clear: both; height: 10px;'></div>");
                            newMessage.Append(_restofMessage);
                            newMessage.Append("<div style='clear: both; height: 5px;'></div>");
                            newMessage.Append("<span style='font-size: 11px!important;'><a href='" + _link + "' target='_blank'>" + _link + "</a></span>");

                            _message = newMessage.ToString();
                        }
                    }
                }
                catch { }
            }

            Groups g = new Groups();

            string realGroupNames = "";

            string messageId = _siteMessageboard.addPost(_username, HttpUtility.UrlEncode(_message), _date, group);

            foreach (string _group in _groups) {
                if (!string.IsNullOrEmpty(_group)) {
                    if (realGroupNames != "") {
                        realGroupNames += ", ";
                    }

                    if (HasRSSFeed(_group)) {
                        RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _userId.Name);
                        rssCreator.Create(_group);
                    }

                    AddNotification(_message, _group);
                    realGroupNames += g.GetGroupName_byID(_group);
                }
            }

            // Build post
            StringBuilder str = new StringBuilder();
            MemberDatabase _member = new MemberDatabase(_username);
            string className = "PostsComments officePostsComments";

            if (_username.ToLower() == "shop")
                className = "PostsComments shopPostsComments";

            List<string> returnArray = new List<string>();

            returnArray.Add(messageId);
            returnArray.Add(className);

            str.Append("<div style='height: 25px; margin-top: -10px;padding-bottom: 10px;'>");
            str.Append("<div class='float-right'>");
            str.Append("<a href='#' class='img-quote pad-all-sml margin-left margin-right' onclick=\"PostMessageQuote('" + messageId + "');return false\" title='Quote Message'></a>");
            str.Append("<a href='#' class='td-delete-btn margin-left margin-right' onclick=\"PostMessageDeleted('" + messageId + "');return false\" title='Delete Message'></a>");
            str.Append("</div>");
            str.Append("<div class='float-left'>");
            str.Append(UserImageColorCreator.CreateImgColor(_member.AccountImage, _member.UserColor, _member.UserId, 35));
            str.Append("<span class='userInfo margin-right font-bold'>" + HelperMethods.MergeFMLNames(_member) + "</span>");
            str.Append("&bull; <span class='margin-right'>" + realGroupNames + "</span>&bull;<span class='userInfoDate pad-left-sml'><i>" + _date + "</i></span>");
            str.Append("</div></div><div class='clear'></div>");
            str.Append("<div class='messageText'>" + HttpUtility.UrlDecode(_message) + "</div>");
            returnArray.Add(str.ToString());

            return returnArray.ToArray();
        }
        return new string[0];
    }

    [WebMethod]
    public object[] GetPosts(string _currIds)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            List<object> returnArray = new List<object>();

            List<string> idList = CreateArray(_currIds);
            object[] returnVals = new object[2];
            List<string> deleteList = new List<string>();
            _username = _userId.Name;
            var posts = new SiteMessageBoard(_username);
            posts.getEntries();
            List<Dictionary<string, string>> dt = posts.post_dt;
            int total = 0;

            // New Message List
            for (int i = 0; i < dt.Count; i++) {
                if (total >= maxNum) break;
                Dictionary<string, string> row = dt[i];
                var tempmember = new MemberDatabase(row["UserName"]);

                Groups g = new Groups(row["UserName"]);
                string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                bool canContinue = false;
                foreach (string group in groupList) {
                    if (g.IsApartOfGroup(posts.groupname, group)) {
                        canContinue = true;
                        break;
                    }
                }

                if (canContinue) {
                    returnArray.Add(BuildMessageBoardAppRow(row, total, tempmember));
                    total++;
                }
            }

            // Delete Notification List
            foreach (string id in idList) {
                bool delete = true;
                for (int i = 0; i < dt.Count; i++) {
                    Dictionary<string, string> row = dt[i];
                    if (row["ID"] == id)
                        delete = false;
                }

                if (delete)
                    deleteList.Add(id);
            }

            returnVals[0] = returnArray.ToArray();
            returnVals[1] = deleteList.ToArray();

            return returnVals;
        }
        return new object[0];
    }

    [WebMethod]
    public object[] GetMorePosts(string _currIds)
    {
        List<object> returnArray = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            List<string> idList = CreateArray(_currIds);
            _username = _userId.Name;
            var posts = new SiteMessageBoard(_username);
            posts.getEntries();
            List<Dictionary<string, string>> dt = posts.post_dt;

            int total = 0;
            for (int i = 0; i < dt.Count; i++) {
                if (total >= maxNum) break;
                Dictionary<string, string> row = dt[i];
                var tempmember = new MemberDatabase(row["UserName"]);

                Groups g = new Groups(row["UserName"]);
                string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                bool canContinue = false;
                foreach (string group in groupList) {
                    if (g.IsApartOfGroup(posts.groupname, group)) {
                        canContinue = true;
                        break;
                    }
                }

                if ((canContinue) && (!idList.Contains(row["ID"]))) {
                    returnArray.Add(BuildMessageBoardAppRow(row, total, tempmember));
                    total++;
                }
            }
        }
        return returnArray.ToArray();
    }

    [WebMethod]
    public string DeleteMessage(string id, string group)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            try {
                _siteMessageboard.deletePost(Guid.Parse(HttpUtility.UrlDecode(id)), group);
                if (HasRSSFeed(group)) {
                    RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _userId.Name);
                    rssCreator.Create(group);
                }
                return "true";
            }
            catch {
                return "false";
            }
        }

        return "false";
    }

    [WebMethod]
    public string FindMessageBoardGroups()
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            Groups group = new Groups(_userId.Name);
            group.getEntries();
            StringBuilder str = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds";
            int count = 0;
            int feedsAvailable = 0;
            foreach (Dictionary<string, string> row in group.group_dt) {
                string groupName = row["GroupName"];
                string groupID = row["GroupID"];
                if (row["CreatedBy"].ToLower() == _userId.Name.ToLower()) {
                    string clickEvent = "AddMBFeed('" + groupID + "')";
                    string addRemoveText = "Add";
                    string hasClass = "";
                    string rssUrl = "<span class='float-right' style='font-size: 12px;'>No RSS Feed Created</span>";

                    if (System.IO.File.Exists(rssFeedsLoc + "\\" + groupID + ".rss")) {
                        string url = "MessageBoard/RSS_Feeds/" + groupID + ".rss";
                        string fakeurl = "View RSS Feed";
                        clickEvent = "RemoveMBFeed('" + groupID + "')";
                        addRemoveText = "Remove";
                        rssUrl = "<a href='" + ServerSettings.GetSitePath(HttpContext.Current.Request) + "/Apps/" + url + "' target='_blank' class='float-right' style='font-size: 12px;'>" + fakeurl + "</a>";
                        hasClass = "add-rss-list-item-hasitem";
                        feedsAvailable++;
                    }

                    str.Append("<div class='add-rss-list-item " + hasClass + "' style='cursor: default;'>");
                    str.Append("<span class='cursor-pointer pad-right-big' title='" + addRemoveText + " " + groupName + " to RSS list' onclick=\"" + clickEvent + "\">" + groupName + "</span>" + rssUrl);
                    str.Append("</div>");
                    count++;
                }
                else {
                    MemberDatabase member = new MemberDatabase(_userId.Name);
                    if (group.IsApartOfGroup(member.GroupList, groupID)) {
                        string rssUrl = "<span class='float-right' style='font-size: 12px;'>No RSS Feed Created</span>";
                        if (System.IO.File.Exists(rssFeedsLoc + "\\" + groupID + ".rss")) {
                            string url = "MessageBoard/RSS_Feeds/" + groupID + ".rss";
                            string fakeurl = "View RSS Feed";
                            rssUrl = "<a href='" + ServerSettings.GetSitePath(HttpContext.Current.Request) + "/Apps/" + url + "' target='_blank' class='float-right' style='font-size: 12px;'>" + fakeurl + "</a>";
                        }

                        str.Append("<div class='add-rss-list-item' style='cursor: default; background-image: none !important;'>");
                        str.Append(groupName + rssUrl);
                        str.Append("</div>");
                        count++;
                    }
                }
            }

            if (count == 0)
                str.Append("<h4 class='pad-all'>No created groups</h4>");

            str2.Append("<div class='pad-all'>Select the groups that you want to have an RSS Feed for. These feeds are automatically updated and can be removed at anytime. You must be the owner of the group to add an RSS feed for.</div>");
            str2.Append("<div class='add-rss-list-header'>My Groups<span class='float-right' style='font-weight: normal!important;'>" + feedsAvailable.ToString() + " feeds available</span></div>");
            str.Append("<div class='clear' style='height: 25px;'></div>");
            return str2.ToString() + str.ToString();
        }
        return string.Empty;
    }

    [WebMethod]
    public string AddRSSMB(string _groupName)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            RSSCreator rssCreator = new RSSCreator(ServerSettings.GetServerMapLocation, _userId.Name);
            rssCreator.Create(_groupName);
        }
        return "";
    }

    [WebMethod]
    public string RemoveRSSMB(string _groupName)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            try {
                RSSFeeds feeds = new RSSFeeds(_userId.Name.ToLower());
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
    public object[] LoadMessageBoardOverlay(string _currIds)
    {
        List<object> returnArray = new List<object>();
        object[] returnVals = new object[2];
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            List<string> idList = CreateArray(_currIds);
            List<string> deleteList = new List<string>();

            StringBuilder str = new StringBuilder();
            _username = _userId.Name;
            var posts = new SiteMessageBoard(_username);
            posts.getEntries();
            List<Dictionary<string, string>> dt = posts.post_dt;
            int total = 0;

            // New Message List
            for (int i = 0; i < dt.Count; i++) {
                if (total >= maxNum) break;
                Dictionary<string, string> row = dt[i];
                var tempmember = new MemberDatabase(row["UserName"]);

                Groups g = new Groups(row["UserName"]);
                string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                bool canContinue = false;
                foreach (string group in groupList) {
                    if (g.IsApartOfGroup(posts.groupname, group)) {
                        canContinue = true;
                        break;
                    }
                }

                if (canContinue) {
                    returnArray.Add(BuildMessageBoardOverlayRow(row, total, tempmember));
                    total++;
                }
            }

            // Delete Notification List
            foreach (string id in idList) {
                bool delete = true;
                for (int i = 0; i < dt.Count; i++) {
                    Dictionary<string, string> row = dt[i];
                    if (row["ID"] + "-overlay" == id)
                        delete = false;
                }

                if (delete)
                    deleteList.Add(id);
            }

            if (total == 0)
                str.Clear();

            returnVals[0] = returnArray.ToArray();
            returnVals[1] = deleteList.ToArray();
        }
        return returnVals;
    }

    [WebMethod]
    public object[] LoadMoreMessageBoardOverlay(string _currIds)
    {
        List<object> returnArray = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            List<string> idList = CreateArray(_currIds);
            _username = _userId.Name;
            var posts = new SiteMessageBoard(_username);
            posts.getEntries();
            List<Dictionary<string, string>> dt = posts.post_dt;
            int total = 0;

            for (int i = 0; i < dt.Count; i++) {
                if (total >= maxNum) break;
                Dictionary<string, string> row = dt[i];
                var tempmember = new MemberDatabase(row["UserName"]);

                Groups g = new Groups(row["UserName"]);
                string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                bool canContinue = false;
                foreach (string group in groupList) {
                    if (g.IsApartOfGroup(posts.groupname, group)) {
                        canContinue = true;
                        break;
                    }
                }

                if ((canContinue) && (!idList.Contains(row["ID"] + "-overlay"))) {
                    returnArray.Add(BuildMessageBoardOverlayRow(row, total, tempmember));
                    total++;
                }
            }
        }
        return returnArray.ToArray();
    }

    private bool HasRSSFeed(string _groupName)
    {
        string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds\\" + _groupName + ".rss";
        if (System.IO.File.Exists(rssFeedsLoc))
            return true;

        return false;
    }
    
    private List<string> CreateArray(string list)
    {
        string[] delims = { "," };
        string[] tempArray = list.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        return new List<string>(tempArray);
    }

    private void AddNotification(string m, string group)
    {
        var g = new Groups();
        MembershipUserCollection userlist = Membership.GetAllUsers();

        var message = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<b style='padding-right: 5px;'>" + _userId.Name + "</b>&bull;<small style='padding-left: 3px;'><i>" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "</i></small><div style='clear: both;'></div>");
        messagebody.Append("<b style='padding-right: 5px;'>Group:</b>" + @group + "<br /><br />" + m + "<br /><br />");
        foreach (MembershipUser u in from MembershipUser u in userlist
                                     where (u.UserName.ToLower() != _userId.Name.ToLower()) && (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())
                                     let member = new MemberDatabase(u.UserName)
                                     where (g.IsApartOfGroup(member.GroupList, @group))
                                     select u)
        {
            UserNotificationMessages un = new UserNotificationMessages(u.UserName);
            string email = un.attemptAdd("app-messageboard", messagebody.ToString(), true);
            if (!string.IsNullOrEmpty(email))
                message.To.Add(email);
        }

        UserNotificationMessages.finishAdd(message, "app-messageboard", messagebody.ToString());
    }

    private string[] BuildMessageBoardAppRow(Dictionary<string, string> row, int total, MemberDatabase tempmember, string messageId = "")
    {
        List<string> obj = new List<string>();
        StringBuilder str = new StringBuilder();
        string className = "PostsComments officePostsComments";

        if (row["UserName"].ToLower() == "shop")
            className = "PostsComments shopPostsComments";

        if (string.IsNullOrEmpty(messageId))
            messageId = row["ID"];

        string realGroupNames = "";
        Groups g = new Groups();
        string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string group in groupList) {
            if (!string.IsNullOrEmpty(group)) {
                if (realGroupNames != "") {
                    realGroupNames += ", ";
                }
                realGroupNames += g.GetGroupName_byID(group);
            }
        }

        obj.Add(messageId);
        obj.Add(className);
        str.Append("<div style='height: 25px; margin-top: -10px;padding-bottom: 10px;'>");
        str.Append("<div class='float-right'>");
        str.Append("<a href='#' class='img-quote pad-all-sml margin-left margin-right' onclick=\"PostMessageQuote('" + messageId + "');return false\" title='Quote Message'></a>");
        if (row["UserName"].ToLower() == _userId.Name.ToLower())
            str.Append("<a href='#' class='td-delete-btn margin-left margin-right' onclick=\"PostMessageDeleted('" + messageId + "');return false\" title='Delete Message'></a>");
        
        str.Append("</div>");
        str.Append("<div class='float-left'>");
        str.Append(UserImageColorCreator.CreateImgColor(tempmember.AccountImage, tempmember.UserColor, tempmember.UserId, 35));
        str.Append("<span class='userInfo margin-right font-bold'>" + HelperMethods.MergeFMLNames(tempmember) + "</span>");
        str.Append("&bull; <span class='margin-right'>" + realGroupNames + "</span>&bull;<span class='userInfoDate pad-left-sml'><i>" + row["Date"] + "</i></span>");
        str.Append("</div></div><div class='clear'></div>");
        str.Append("<div class='messageText'>" +  HttpUtility.UrlDecode(row["Post"]) + "</div>");

        obj.Add(str.ToString());
        return obj.ToArray();
    }

    private string[] BuildMessageBoardOverlayRow(Dictionary<string, string> row, int total, MemberDatabase tempmember) {
        List<string> returnArray = new List<string>();
        StringBuilder str = new StringBuilder();

        returnArray.Add(row["ID"] + "-overlay");

        string realGroupNames = "";
        Groups g = new Groups();
        string[] groupList = row["GroupName"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string group in groupList) {
            if (!string.IsNullOrEmpty(group)) {
                if (realGroupNames != "") {
                    realGroupNames += ", ";
                }
                realGroupNames += g.GetGroupName_byID(group);
            }
        }

        str.Append("<div style='height: 25px;'><div class='float-left'>" + UserImageColorCreator.CreateImgColor(tempmember.AccountImage, tempmember.UserColor, tempmember.UserId, 35));
        str.Append("<b><span class='margin-right'>" + HelperMethods.MergeFMLNames(tempmember) + "</span></b>&bull; <span class='margin-right'>" + realGroupNames + "</span>&bull; <small class='pad-left-sml'><i>" + row["Date"] + "</i></small></div></div>");
        str.Append("<div class='clear-space-two'></div><p>" + HttpUtility.UrlDecode(row["Post"]) + "</p>");
        returnArray.Add(str.ToString());

        return returnArray.ToArray();
    }

    private static string ConvertToObjectEmbeded(string msg)
    {
        string[] del = { "?v=" };
        string[] vidid = msg.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 0)
        {
            var str = new StringBuilder();

            // EMBEDED VERSION
            str.Append("<object width='450' height='350'>");
            str.Append("<param name='movie' value='https://www.youtube.com/v/" + vidid[1] +
                       "?version=3&autoplay=0'></param>");
            str.Append("<param name='allowScriptAccess' value='always'></param>");
            str.Append("<embed src='https://www.youtube.com/v/" + vidid[1] + "?version=3&autoplay=0' ");
            str.Append("type='application/x-shockwave-flash' ");
            str.Append("allowscriptaccess='always' ");
            str.Append("width='500' height='300'></embed> ");
            str.Append("</object>");

            // IFRAME VERSION (RECOMMENDED)
            //str.Append("<iframe id='ytplayer' type='text/html' width='250' height='150' ");
            //str.Append("src='http://www.youtube.com/embed/"+ vidid[1] + "?autoplay=0' ");
            //str.Append("frameborder='0' />");

            msg = str.ToString();
        }
        return msg;
    }
}