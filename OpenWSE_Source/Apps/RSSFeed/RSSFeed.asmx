<%@ WebService Language="C#" Class="RSSFeed" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.XPath;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class RSSFeed : System.Web.Services.WebService {

    private MemberDatabase _member;
    private string _username;

    public RSSFeed() {
        _username = HttpContext.Current.User.Identity.Name;
    }

    [WebMethod]
    public string GetRSSFeed(string _url, string _show, string search) {
        StringBuilder sb = new StringBuilder();
        _url = HttpUtility.UrlDecode(_url);
        search = HttpUtility.UrlDecode(search);
        if ((!string.IsNullOrEmpty(_url)) && (_url != "undefined")) {
            try {
                search = search.ToLower().Trim();
                if (search == "search feeds")
                    search = string.Empty;

                int numberToShow = 10;
                int.TryParse(_show, out numberToShow);

                WebRequest request = WebRequest.Create(_url);
                WebResponse response = request.GetResponse();
                Stream rssStream = response.GetResponseStream();
                XmlDocument rssDoc = new XmlDocument();
                rssDoc.Load(rssStream);

                XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/item");
                if (rssItems.Count == 0) {
                    var nsmgr = new XmlNamespaceManager(new NameTable());
                    nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                    rssItems = rssDoc.SelectNodes("/atom:feed/atom:entry/atom:content", nsmgr);
                    if (rssItems.Count == 0) {
                        rssItems = rssDoc.SelectNodes("/atom:feed/atom:entry", nsmgr);
                        if (rssItems.Count == 0) {
                            for (int i = 0; i < rssDoc.ChildNodes.Count; i++) {
                                XmlNode x = rssDoc.ChildNodes[i];
                                if (x.Name.ToLower() == "feed")
                                    rssItems = rssDoc.ChildNodes[i].ChildNodes;
                            }
                        }
                    }
                }

                int ii = 0;
                for (int i = 0; i < rssItems.Count; i++) {
                    if (ii >= numberToShow)
                        break;

                    string title = "";
                    string link = "";
                    string description = "";
                    string summary = "";
                    string content = "";
                    string pubDate = "";
                    string creator = "";
                    string comments = "";
                    XmlNode rssDetail;

                    string text = GetRSSChildFeeds(rssItems[i].ChildNodes, search, title, link, description, summary, content, pubDate, creator, comments);
                    if (!string.IsNullOrEmpty(text)) {
                        sb.Append(text);
                        ii++;
                    }
                }
            }
            catch {
                if ((search.ToLower() == "search feeds") || (string.IsNullOrEmpty(search))) {
                    sb.Append("<li class='remove-rss-li'><div class='pad-all'>Could not load " + _url + ".");
                    sb.Append("<div class='remove-rss-q pad-top clear'>Would you want to remove it from your list? <input type='button' value='Yes' class='input-buttons margin-left' onclick='RemoveNotFoundRssFeed(this, \"" + _url + "\")' style='width: 50px;' />");
                    sb.Append("<input type='button' value='No' class='input-buttons' onclick='CancelNotFoundRssFeed(this)' style='width: 50px;' /></div></div></li>");
                }
            }
        }
        return sb.ToString();
    }

    private string GetRSSChildFeeds(XmlNodeList childList, string search, string title, string link, string description, string summary, string content, string pubDate, string creator, string comments) {
        StringBuilder sb = new StringBuilder();
        foreach (XmlNode node in childList) {
            if ((node.HasChildNodes) && (node.FirstChild.Name.ToLower() != "#text") && (DoesntContainSection(node.Name.ToLower()))) {
                GetRSSChildFeeds(node.ChildNodes, search, title, link, description, summary, content, pubDate, creator, comments);
            }
            else {
                string nodeName = node.Name.ToLower();
                if ((nodeName == "#text") && (node.ParentNode != null) && (!DoesntContainSection(node.ParentNode.Name.ToLower()))) {
                    nodeName = node.ParentNode.Name.ToLower();
                }
                switch (nodeName) {
                    case "title":
                        title = node.InnerText;
                        break;
                    case "link":
                        link = node.InnerText;
                        break;
                    case "description":
                        description = node.InnerText;
                        break;
                    case "summary":
                        summary = node.InnerText;
                        break;
                    case "pubdate":
                        pubDate = node.InnerText;
                        break;
                    case "published":
                        pubDate = node.InnerText;
                        break;
                    case "dc:date":
                        pubDate = node.InnerText;
                        break;
                    case "content:encoded":
                        content = node.InnerText;
                        break;
                    case "content":
                        content = node.InnerText;
                        break;
                    case "dc:creator":
                        creator = node.InnerText;
                        break;
                    case "creator":
                        creator = node.InnerText;
                        break;
                    case "author":
                        creator = node.InnerText;
                        break;
                    case "comments":
                        comments = node.InnerText;
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(creator))
            creator = "N/A";

        if ((!string.IsNullOrEmpty(description)) && (!string.IsNullOrEmpty(summary)) && (description != summary))
            description = description + "<br />";

        if (description == summary)
            summary = "";

        if (description == content)
            content = "";

        if (!string.IsNullOrEmpty(content))
            content = "<div class='clear-space'></div>" + content;


        // Search for match
        if (!string.IsNullOrEmpty(search)) {
            if ((!link.ToLower().Contains(search))
                && (!title.ToLower().Contains(search))
                && (!pubDate.ToLower().Contains(search))
                && (!creator.ToLower().Contains(search))
                && (!description.ToLower().Contains(search))
                && (!summary.ToLower().Contains(search))
                && (!content.ToLower().Contains(search)))
                return string.Empty;
            else {
                string replacement = string.Empty;
                string backcolor = "style='background-color: #FFE97F;'";
                if (title.ToLower().Contains(search)) {
                    replacement = title.Substring(title.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    title = Regex.Replace(title, search, replacement, RegexOptions.IgnoreCase);
                }
                if (pubDate.ToLower().Contains(search)) {
                    replacement = pubDate.Substring(pubDate.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    pubDate = Regex.Replace(pubDate, search, replacement, RegexOptions.IgnoreCase);
                }
                if (creator.ToLower().Contains(search)) {
                    replacement = creator.Substring(creator.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    creator = Regex.Replace(creator, search, replacement, RegexOptions.IgnoreCase);
                }
                if (description.ToLower().Contains(search)) {
                    replacement = description.Substring(description.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    description = Regex.Replace(description, search, replacement, RegexOptions.IgnoreCase);
                }
                if (summary.ToLower().Contains(search)) {
                    replacement = summary.Substring(summary.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    summary = Regex.Replace(summary, search, replacement, RegexOptions.IgnoreCase);
                }
                if (content.ToLower().Contains(search)) {
                    replacement = content.Substring(content.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                    replacement = "<span " + backcolor + ">" + replacement + "</span>";
                    content = Regex.Replace(content, search, replacement, RegexOptions.IgnoreCase);
                }
            }
        }


        DateTime publishedDate;
        if (DateTime.TryParse(pubDate, out publishedDate))
            pubDate = publishedDate.ToString();

        if (!string.IsNullOrEmpty(title)) {
            if (!string.IsNullOrEmpty(link))
                sb.Append("<li><a class='rss-title' href='" + link + "' target='_blank'>" + title + "</a><br />");
            else
                sb.Append("<li><span class='rss-title'>" + title + "</span><br />");
        }

        string commentsStr = string.Empty;
        if (!string.IsNullOrEmpty(comments)) {
            commentsStr = "<div class='clear-space'></div><b>Comments</b><div class='clear-space-two'></div>" + comments;
        }

        sb.Append("<div class='rss-author'><b class='pad-right-sml'>Posted:</b>" + pubDate + " by " + creator + "</div>");
        sb.Append("<div class='rss-description'>" + description + summary + "</div>");
        sb.Append("<div class='rss-content'>" + content + commentsStr + "</div></li>");

        return sb.ToString();
    }

    private bool DoesntContainSection(string nodeName) {
        if ((nodeName.Contains("title")) || (nodeName.Contains("link")) || (nodeName.Contains("description")) || (nodeName.Contains("summary"))
                || (nodeName.Contains("pubdate")) || (nodeName.Contains("published")) || (nodeName.Contains("dc:date"))
                || (nodeName.Contains("content:encoded")) || (nodeName.Contains("content")) || (nodeName.Contains("dc:creator"))
                || (nodeName.Contains("creator")) || (nodeName.Contains("author")) || (nodeName.Contains("comments"))) {
                    return false;
        }
        return true;
    }

    [WebMethod]
    public string DeleteFeedFromList(string _url) {
        if (!string.IsNullOrEmpty(_username)) {
            _url = HttpUtility.UrlDecode(_url);
            RSSFeeds feeds = new RSSFeeds(_username);
            feeds.DeleteRowByURL(_url);
        }
        return "true";
    }

    [WebMethod]
    public string AddRemoveFeed(string _title, string _url, string _rssid, string _needAdd) {
        if (!string.IsNullOrEmpty(_username)) {
            _title = HttpUtility.UrlDecode(_title);
            _url = HttpUtility.UrlDecode(_url);
            RSSFeeds feeds = new RSSFeeds(_username);
            if (HelperMethods.ConvertBitToBoolean(_needAdd))
                feeds.AddItem(_title, _url, _rssid, false);
            else
                feeds.DeleteRowByRSSID(_rssid);
        }
        return "";
    }

    [WebMethod]
    public object[] GetUserFeeds() {
        object[] obj = new object[4];
        List<RSSFeeds_Coll> feedColl = new List<RSSFeeds_Coll>();

        RSSFeeds feeds = null;
        
        if (!string.IsNullOrEmpty(_username)) {
            feeds = new RSSFeeds(_username);
            feeds.BuildEntriesAll();
            feedColl = feeds.RSSFeedCollection;
        }
        else {
            feedColl = GetDemoFeeds();
        }
        
        
        List<string> list1 = new List<string>();
        List<string> list2 = new List<string>();
        List<string> list3 = new List<string>();
        List<string> list4 = new List<string>();
        foreach (RSSFeeds_Coll feed in feedColl) {
            string feedUrl = feed.URL;
            if ((!feedUrl.StartsWith("http://")) && (!feedUrl.StartsWith("https://"))) {
                feedUrl = "http:" + feedUrl;
            }

            if (feed.IsCustomFeed) {
                try {
                    bool pageExists = true;
                    HttpWebResponse response = null;

                    var request = (HttpWebRequest)WebRequest.Create(feedUrl);
                    request.Method = "HEAD";

                    try {
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    catch (WebException ex) {
                        pageExists = false;
                    }
                    finally {
                        // Don't forget to close your response.
                        if (response != null)
                            response.Close();
                    }

                    if (!pageExists) {
                        if (feeds != null) {
                            feeds.DeleteRowByID(feed.ID);
                        }
                    }

                }
                catch { }
            }

            if (!list1.Contains(feed.Title)) {
                list1.Add(feed.Title);
                list2.Add(feedUrl);
                list3.Add(feed.IsCustomFeed.ToString().ToLower());
                list4.Add(feed.RSSID);
            }
        }

        obj[0] = list1.ToArray();
        obj[1] = list2.ToArray();
        obj[2] = list3.ToArray();
        obj[3] = list4.ToArray();
        
        return obj;
    }

    private List<RSSFeeds_Coll> GetDemoFeeds() {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(ServerSettings.GetServerMapLocation + "Apps\\RSSFeed\\RSSFeeds.xml");

        List<RSSFeeds_Coll> coll = new List<RSSFeeds_Coll>();
        
        if (xmlDoc != null) {
            XmlNodeList nodeList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
            foreach (XmlNode node in nodeList) {
                if (node.ChildNodes.Count >= 3) {
                    XmlNodeList childNodes = node.ChildNodes;
                    RSSFeeds_Coll feed = new RSSFeeds_Coll(Guid.NewGuid().ToString(), string.Empty, "false", childNodes[1].InnerText, childNodes[2].InnerText, childNodes[0].InnerText, DateTime.Now.ToString());
                    coll.Add(feed);
                }
            }
        }

        return coll;
    }

    [WebMethod]
    public object[] AddCustomFeed(string _url) {
        object[] obj = new object[2];
        if (!string.IsNullOrEmpty(_username)) {
            string title = "";
            string id = Guid.NewGuid().ToString();
            _url = HttpUtility.UrlDecode(_url);
            StringBuilder sb = new StringBuilder();
            if ((!string.IsNullOrEmpty(_url)) && (_url != "undefined")) {
                try {
                    RSSFeeds feeds = new RSSFeeds(_username);
                    WebRequest request = WebRequest.Create(_url);
                    WebResponse response = request.GetResponse();
                    Stream rssStream = response.GetResponseStream();
                    XmlDocument rssDoc = new XmlDocument();
                    rssDoc.Load(rssStream);

                    title = _url;
                    XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/title");
                    if (rssItems.Count == 1)
                        title = rssItems.Item(0).InnerText;

                    feeds.AddItem(title, _url, id, true);
                }
                catch {
                    obj[0] = "error";
                    obj[1] = "error";
                    return obj;
                }
            }

            obj[0] = title;
            obj[1] = id;
        }
        return obj;
    }
}