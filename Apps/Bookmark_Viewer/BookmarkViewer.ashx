<%@ WebHandler Language="C#" Class="BookmarkViewer" %>

using System;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Net.Mail;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;
using System.Collections.Generic;

public class BookmarkViewer : IHttpHandler {
    private HttpRequest _request;
    private HttpResponse _response;
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private Bookmarks _bookmarks;
    private MemberDatabase _member;
    private const string noBookmarks = "<h3 class='no-bookmarks pad-right-big pad-left-big pad-top-big'>No bookmarks available</h3>";

    public void ProcessRequest(HttpContext context) {
        string result = "";

        IIdentity userId = HttpContext.Current.User.Identity;
        _request = context.Request;
        _response = context.Response;
        _response.ContentType = "application/json";

        if (userId.IsAuthenticated) {

            bool canEdit = true;
            string _username = userId.Name;
            if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(userId.Name)) {
                _username = GroupSessions.GetUserGroupSessionName(userId.Name);
            }
            else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                _username = GroupSessions.GetUserGroupSessionName(_username);
                canEdit = false;
            }

            _bookmarks = new Bookmarks(_username);
            _member = new MemberDatabase(userId.Name);
            string sortBystr = "DateAdded DESC";

            string sortBy = _request.QueryString["sortby"];
            if (!string.IsNullOrEmpty(sortBy)) {
                switch (sortBy) {
                    case "1":
                        sortBystr = "DateAdded DESC";
                        break;
                    case "2":
                        sortBystr = "DateAdded ASC";
                        break;
                    case "3":
                        sortBystr = "BookmarkName DESC";
                        break;
                    case "4":
                        sortBystr = "BookmarkName ASC";
                        break;
                }
            }
            
            switch (_request.QueryString["action"]) {
                case "add":
                    string name = HttpUtility.UrlDecode(_request.QueryString["name"]);
                    if ((string.IsNullOrEmpty(name)) || (name.ToLower() == "new bookmark name"))
                        name = HttpUtility.UrlDecode(_request.QueryString["url"]);
                    result = Add(name, HttpUtility.UrlDecode(_request.QueryString["url"]), sortBystr);
                    break;
                case "load":
                    _bookmarks.GetBookmarks(sortBystr);
                    result = LoadBookmarks();
                    break;
                case "count":
                    _bookmarks.GetBookmarks(sortBystr);
                    result = _bookmarks.bookmarks_dt.Count.ToString();
                    break;
                case "edit":
                    if (canEdit) {
                        if (!string.IsNullOrEmpty(_request.QueryString["id"])) {
                            result = Edit(_request.QueryString["id"], sortBystr);
                        }
                    }
                    break;
                case "finishEdit":
                    if (canEdit) {
                        if ((!string.IsNullOrEmpty(_request.QueryString["id"])) && (!string.IsNullOrEmpty(_request.QueryString["name"])) && (!string.IsNullOrEmpty(_request.QueryString["html"]))) {
                            _bookmarks.UpdateBookmark(HttpUtility.UrlDecode(_request.QueryString["id"]), HttpUtility.UrlDecode(_request.QueryString["name"]), HttpUtility.UrlDecode(_request.QueryString["html"]));
                            _bookmarks.GetBookmarks(sortBystr);
                            result = LoadBookmarks();
                        }
                    }
                    break;
                case "share":
                    if (!string.IsNullOrEmpty(_request.QueryString["id"])) {
                        result = Share(_request.QueryString["id"]);
                    }
                    break;
                case "finishShare":
                    if (!string.IsNullOrEmpty(_request.QueryString["id"])) {
                        result = FinishShare(_request.QueryString["id"], _request.QueryString["listusers"], userId.Name);
                    }
                    break;
                case "remove":
                    if (canEdit) {
                        if (!string.IsNullOrEmpty(_request.QueryString["id"])) {
                            result = Remove(_request.QueryString["id"]);
                        }
                    }
                    break;
                case "search":
                    if (!string.IsNullOrEmpty(_request.QueryString["val"])) {
                        string searchVal = HttpUtility.UrlDecode(_request.QueryString["val"].Trim());
                        _bookmarks.GetBookmarks(sortBystr);
                        result = SearchBookmarks(searchVal);
                    }
                    else {
                        _bookmarks.GetBookmarks(sortBystr);
                        result = LoadBookmarks();
                    }
                    break;
                case "import":
                    if (canEdit) {
                        HttpPostedFile oFile = _request.Files["Filedata"];
                        if ((oFile != null) && (oFile.ContentLength > 0)) {
                            try {
                                System.IO.FileInfo fi = new System.IO.FileInfo(oFile.FileName);
                                if (fi.Extension.ToLower() == ".html") {
                                    System.IO.StreamReader reader = new System.IO.StreamReader(oFile.InputStream);
                                    string sText;
                                    while ((sText = reader.ReadLine()) != null) {
                                        string dt = sText.ToLower().Trim().Substring(0, 4);
                                        if (dt == "<dt>") {
                                            string link = sText.Trim().Substring(4);
                                            string beginA = link.ToLower().Trim().Substring(0, 8);
                                            if (beginA == "<a href=")
                                                ParseLink(link);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    break;
            }
        }

        _response.Clear();
        _response.Write(result);
        _response.End();
        context.ApplicationInstance.CompleteRequest();
    }

    private void ParseLink(string link) {
        string name = string.Empty;
        string html = string.Empty;
        link = link.Substring(9);
        int indexOf = link.IndexOf("\"");
        if (indexOf != -1)
            html = link.Substring(0, indexOf);

        link = link.Substring(html.Length + 2).Trim();
        indexOf = link.LastIndexOf("\"");
        if (indexOf != -1) {
            link = link.Substring(indexOf + 2);
            link = StripTagsRegex(link);
            name = link;
        }

        if ((!string.IsNullOrEmpty(html)) && (!string.IsNullOrEmpty(name))) {
            _bookmarks.AddItem(name, html);
        }
    }

    private static string StripTagsRegex(string source) {
        return string.IsNullOrEmpty(source) ? source : System.Text.RegularExpressions.Regex.Replace(source, "<.*?>", string.Empty);
    }

    private string Add(string name, string url, string sortBystr) {
        var str = new StringBuilder();
        if ((!string.IsNullOrEmpty(url)) &&
            (url.ToLower() != "bookmark url")) {
            bool cancontinue = false;
            try {
                var uri = new Uri(url);
                if (uri.IsAbsoluteUri) {
                    url = uri.AbsoluteUri;
                    cancontinue = true;
                }
            }
            catch {
                try {
                    var uri = new Uri("http://" + url);
                    if (uri.IsAbsoluteUri) {
                        url = uri.AbsoluteUri;
                        cancontinue = true;
                    }
                }
                catch {
                    return "";
                }
            }

            if (cancontinue) {
                if (string.IsNullOrEmpty(name) || name == url) {
                    name = new IpMethods().GetWebPageTitle(url);
                    if (string.IsNullOrEmpty(name)) {
                        name = url;
                    }
                    else {
                        name = HttpUtility.HtmlDecode(name);
                    }
                }
                
                _bookmarks.AddItem(name, url);

                _bookmarks.GetBookmarks(sortBystr);
                foreach (Dictionary<string, string> dr in _bookmarks.bookmarks_dt) {
                    string name2 = dr["BookmarkName"];
                    string html2 = dr["BookmarkHTML"];
                    if ((name2 == name) && (html2 == url)) {
                        str.Append(BuildLink(dr));
                        break;
                    }
                }

                return str.ToString();
            }
        }
        else {
            return "";
        }

        return "";
    }

    private string LoadBookmarks() {
        int count = 1;
        var str = new StringBuilder();
        foreach (Dictionary<string, string> dr in _bookmarks.bookmarks_dt) {
            str.Append(BuildLink(dr));
            count++;
        }

        if (count == 1)
            str.Append(noBookmarks);

        return str.ToString();
    }

    private string SearchBookmarks(string val) {
        int count = 1;
        var str = new StringBuilder();
        foreach (Dictionary<string, string> dr in _bookmarks.bookmarks_dt) {
            string name = HttpUtility.HtmlDecode(dr["BookmarkName"]);
            string html = HttpUtility.HtmlDecode(dr["BookmarkHTML"]);
            if ((name.ToLower().Contains(val.ToLower())) || (html.ToLower().Contains(val.ToLower()))) {
                str.Append(BuildLink(dr));
                count++;
            }
        }

        if (count == 1)
            str.Append(noBookmarks);

        return str.ToString();
    }

    public string BuildLink(Dictionary<string, string> dr) {
        var str = new StringBuilder();
        string id = dr["ID"];
        string name = dr["BookmarkName"];
        string html = dr["BookmarkHTML"];
        string date = dr["DateAdded"];
        string favicon = "App_Themes/" + _member.SiteTheme + "/Icons/unknown.png";
        string width = "25px";
        bool isYouTube = false;
        try {
            var uri = new Uri(html);
            if ((uri.Host.ToLower().Contains("youtube.com")) && (uri.LocalPath == "/watch") && (uri.IsAbsoluteUri)) {
                string queryString = uri.Query.Replace("?v=", "");
                if (uri.Query.IndexOf('&') != -1)
                    queryString = queryString.Replace(uri.Query.Substring(uri.Query.IndexOf('&')), "");

                favicon = "http://img.youtube.com/vi/" + queryString + "/default.jpg";
                width = "65px";
                isYouTube = true;
            }
            else if (uri.IsAbsoluteUri)
                favicon = "http://www.google.com/s2/favicons?domain=" + uri.OriginalString;
        }
        catch { }

        string nametitle = string.Empty;
        int length = name.Length;
        string youtubeClass = "";
        string view = "";

        if ((html.Contains("http://www.youtube.com/watch")) || (html.Contains("www.youtube.com/watch")) || (html.Contains("youtube.com/watch"))) {
            youtubeClass = " bookmark-table-YoutubeLink";
            favicon = "<img alt='favicon' src='" + favicon + "' title='Play YouTube Video' class='pad-right' onclick='embedded_video_bookmark(\"" + ConvertToObjectEmbeded(html) + "\", \"" + id + "\");' style='width:" + width + "' />";
            view = "<a href='#' class='bookmark-html-link' title='Play YouTube Video' onclick='embedded_video_bookmark(\"" + ConvertToObjectEmbeded(html) + "\", \"" + id + "\");return false;'>" + name + "</a>";
        }
        else {
            view = "<a href='" + html + "' target='_blank' class='bookmark-html-link' title='Open Link'>" + name + "</a>";
            favicon = "<img alt='favicon' src='" + favicon + "' class='pad-right' style='width:" + width + "' />";
            favicon = "<a href='" + html + "' target='_blank' class='bookmark-html-link' title='Open Link'>" + favicon + "</a>";
        }

        str.Append("<table id='" + id + "' class='bookmark-table-styles" + youtubeClass + "'><tbody><tr>");
        str.Append("<td width='30px' align='center'>" + favicon + "</td>");
        str.Append("<td><div class='pad-top-sml pad-bottom-sml' style='overflow:hidden'><span class='pad-left-sml' " + nametitle + "><b>" + view + "</b></span>");
        str.Append("<div class='clear'></div><span class='pad-left-sml' style='font-size:10px;color:#888'><b class='pad-right-sml'>Date Added:</b>" + date + "</span></div></td>");

        // Add bm-hidden to the class list to make use of the hover visibility
        string remove = "<a href='#Remove' class='td-delete-btn margin-right float-right' onclick='remove_in_iFrame(\"" + id + "\");return false;' title='Delete'></a>";
        string share = "<a href='#Share' class='td-add-btn margin-right float-right' onclick='share_in_iFrame(\"" + id + "\");return false;' title='Share'></a>";
        string edit = "<a href='#Edit' class='td-edit-btn margin-right float-right' onclick='edit_in_iFrame(\"" + id + "\");return false;' title='Edit'></a>";

        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name) && GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
            share = string.Empty;
        }
        else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
            remove = string.Empty;
            share = string.Empty;
            edit = string.Empty;
        }
        
        str.Append("<td class='bookmark-edit-btn-holder'>" + remove + share + edit + "</td></tr></tbody></table>");

        return str.ToString();
    }

    private string GetFavIcon(Uri url) {
        using (System.Net.WebClient client = new System.Net.WebClient()) {
            string htmlCode = client.DownloadString(url.OriginalString);
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(htmlCode, "<link .*? href=\"(.*?.ico)\"")) {
                return match.Groups[1].Value;
            }
            return url.Host + "/favicon.ico";
        }
    }

    private string ConvertToObjectEmbeded(string msg) {
        try {
            string[] del = { "?v=" };
            string[] vidid = msg.Split(del, StringSplitOptions.RemoveEmptyEntries);
            if (vidid.Length > 0) {
                msg = vidid[1];
                if (msg.Contains("&")) {
                    msg = msg.Replace(msg.Substring(msg.IndexOf("&")), string.Empty);
                }
            }
        }
        catch {
        }
        return msg;
    }

    private bool FavIconExists(string url) {
        bool exists = false;
        System.Net.HttpWebResponse response = null;
        var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        request.Method = "HEAD";

        try {
            response = (System.Net.HttpWebResponse)request.GetResponse();
            exists = true;
        }
        catch (System.Net.WebException ex) {
            exists = false;
        }
        finally {
            // Don't forget to close your response.
            if (response != null) {
                response.Close();
            }
        }

        return exists;
    }

    private string Share(string id) {
        var str = new StringBuilder();
        string bookmarkName = _bookmarks.GetHTMLName_byID(id);
        str.Append("<h4 class='float-left pad-top-sml font-bold'>Share '" + bookmarkName + "' With:</h4>");
        str.Append("<div class='clear-space'></div><div class='listofusers'>");
        var str_list = new StringBuilder();
        int count = 0;
        foreach (System.Web.Security.MembershipUser u in System.Web.Security.Membership.GetAllUsers()) {
            if ((u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) && (u.UserName.ToLower() != _member.Username.ToLower())) {
                var m = new MemberDatabase(u.UserName);
                if (HelperMethods.CompareUserGroups(_member, m)) {
                    str_list.Append("<div class='float-left' style='width:190px'><input class='share-tb-list' type='checkbox' value='" + u.UserName + "'>&nbsp;" + HelperMethods.MergeFMLNames(m) + "</div>");
                    count++;
                }
            }
        }
        if (count == 0) {
            str.Append("<h3>No users available</h3>");
        }
        else {
            str.Append(str_list + "</div>");
            str.Append("<div class='clear' style='height: 25px;'></div>");
            str.Append("<input type='button' class='input-buttons' value='Share' onclick='share_click(\"" + id + "\")' />");
        }

        return str.ToString();
    }

    public string FinishShare(string id, string listusers, string currUser) {
        try {
            string[] list = listusers.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string linkID = id;
            if (!string.IsNullOrEmpty(linkID)) {
                string htmlname = _bookmarks.GetHTMLName_byID(linkID);
                string html = _bookmarks.GetHTML_byID(linkID);
                MailMessage mailTo = new MailMessage();

                string _html = html;
                if (html.Length > 50)
                    _html = html.Substring(0, 49) + "...";

                string message = _member.Username + " has shared a bookmark with you.<br />Link: <a href='" + html + "' target='_blank'>" + _html + "</a>";

                foreach (string user in list) {
                    if (!string.IsNullOrEmpty(user)) {
                        var un = new UserNotificationMessages(user);

                        var bm = new Bookmarks(user);
                        bm.AddItem(htmlname, html);

                        string email = un.attemptAdd("app-bookmarkviewer", message, true);
                        if (!string.IsNullOrEmpty(email))
                            mailTo.To.Add(email);
                    }
                }

                UserNotificationMessages.finishAdd(mailTo, "app-bookmarkviewer", message);
            }
            return "true";
        }
        catch {
            return "";
        }
    }

    private string Edit(string id, string sortBystr) {
        var str = new StringBuilder();

        _bookmarks.GetBookmarks(sortBystr);
        foreach (Dictionary<string, string> dr in _bookmarks.bookmarks_dt) {
            if (id == dr["ID"].ToString()) {
                string bookmarkName = dr["BookmarkName"];
                str.Append("<h4 class='float-left pad-top-sml font-bold'>Edit '" + bookmarkName + "'</h4>");
                str.Append("<div class='clear-space'></div>");
                str.Append("<input type='hidden' id='editBookmarkid' value='" + id + "' />");
                str.Append("<table cellspacing='5' cellpadding='5' style='width: 100%;'>");
                str.Append("<tr><td style='width: 50px;'><span class='pad-right font-bold'>Url</span></td>");
                str.Append("<td><input type='text' id='editBookmarkHtml' class='textEntry margin-right-sml' value='" + dr["BookmarkHTML"].ToString() + "' style='width:100%' /></td></tr>");
                str.Append("<tr><td style='width: 50px;'><span class='pad-right font-bold'>Name</span></td>");
                str.Append("<td><input type='text' id='editBookmarkName' class='textEntry margin-right-sml' value='" + dr["BookmarkName"].ToString() + "' style='width:100%' /></td></tr>");
                str.Append("</table>");
                break;
            }
        }
        str.Append("<div class='clear-space'></div>");
        str.Append("<input type='button' class='input-buttons' value='Save' onclick='edit_click()' />");

        return str.ToString();
    }

    public string Remove(string id) {
        try {
            _bookmarks.deleteBookmark_byID(id);
            return id;
        }
        catch {
            return "";
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}