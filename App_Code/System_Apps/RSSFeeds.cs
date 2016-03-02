using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using OpenWSE_Tools.AutoUpdates;
using System.Data.SqlServerCe;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;
using System.Net;
using System.ServiceModel.Syndication;
using System.IO;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


[Serializable]
public class RSSFeeds_Coll {
    private string _id;
    private string _username;
    private bool _customFeed = false;
    private string _title;
    private string _loc;
    private string _rssid;
    private DateTime _dateAdded = new DateTime();

    public RSSFeeds_Coll(string id, string username, string customFeed, string title, string loc, string rssid, string dateAdded) {
        _id = id;
        _username = username;
        if (HelperMethods.ConvertBitToBoolean(customFeed))
            _customFeed = true;

        _title = title;
        _loc = loc;
        _rssid = rssid;
        DateTime.TryParse(dateAdded, out _dateAdded);
    }

    public string ID {
        get { return _id; }
    }

    public string UserName {
        get { return _username; }
    }

    public bool IsCustomFeed {
        get { return _customFeed; }
    }

    public string Title {
        get { return _title; }
    }

    public string URL {
        get { return _loc; }
    }

    public string RSSID {
        get { return _rssid; }
    }

    public DateTime DateAdded {
        get { return _dateAdded; }
    }
}

[Serializable]
public class RSSItem {
    private string _title = "";
    private string _link = "";
    private string _summary = "";
    private string _content = "";
    private DateTime _pubDate = new DateTime();
    private string _creator = "";
    private string _source = "";
    private string _sourceImage = "";
    private string _bgStyleClass = "";

    public RSSItem() { }
    public RSSItem(string title, string link, string summary, string content, string pubDate, string creator, string bgStyleClass) {
        _title = title;
        _link = link;
        _summary = summary;
        _content = content;
        pubDate = pubDate.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("EDT", string.Empty);
        DateTime.TryParse(pubDate, out _pubDate);
        _creator = creator;
        _bgStyleClass = bgStyleClass;
    }

    public string Title {
        get { return _title; }
        set { _title = value; }
    }

    public string Link {
        get { return _link; }
        set { _link = value; }
    }

    public string Summary {
        get { return _summary; }
        set { _summary = value; }
    }

    public string Content {
        get { return _content; }
        set { _content = value; }
    }

    public DateTime PubDate {
        get { return _pubDate; }
    }
    public void SetPubDate(string pubDate) {
        DateTime.TryParse(pubDate, out _pubDate);
    }

    public string Creator {
        get { return _creator; }
        set { _creator = value; }
    }

    public string BgStyleClass {
        get { return _bgStyleClass; }
        set { _bgStyleClass = value; }
    }

    public string Source {
        get { return _source; }
        set { _source = value; }
    }

    public string SourceImage {
        get { return _sourceImage; }
        set { _sourceImage = value; }
    }
}

/// <summary>
/// Summary description for RSSFeeds
/// </summary>
[Serializable]
public class RSSFeeds {
    public static DateTime FeedListDateUpdated = ServerSettings.ServerDateTime;
    public static Dictionary<string, KeyValuePair<DateTime, IList<RSSItem>>> LoadedFeedList = new Dictionary<string, KeyValuePair<DateTime, IList<RSSItem>>>(); 
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<RSSFeeds_Coll> _RSSFeeds_Coll = new List<RSSFeeds_Coll>();
    private string _userName;

    public RSSFeeds(string userName) {
        _userName = userName;
    }

    public void AddItem(string title, string url, string rssid, bool customFeed) {
        string _customFeed = "0";
        if (customFeed)
            _customFeed = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _userName));
        query.Add(new DatabaseQuery("CustomFeed", _customFeed));
        query.Add(new DatabaseQuery("RSSFeedTitle", title));
        query.Add(new DatabaseQuery("RSSFeedLoc", url));
        query.Add(new DatabaseQuery("RSSID", rssid));
        query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert("RSSFeeds", query);
    }

    public void BuildEntriesAll() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("RSSFeeds", "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "RSSFeedTitle ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string username = row["UserName"];
            string customFeed = row["CustomFeed"];
            string title = row["RSSFeedTitle"];
            string loc = row["RSSFeedLoc"];
            string rssid = row["RSSID"];
            string date = row["DateAdded"];
            var coll = new RSSFeeds_Coll(id, username, customFeed, title, loc, rssid, date);
            _RSSFeeds_Coll.Add(coll);
        }
    }

    public void DeleteRowByID(string id) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteUserFeeds(string userName) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteRowByRSSID(string rssid) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("RSSID", rssid), new DatabaseQuery("UserName", _userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteRowByURL(string loc) {
        dbCall.CallDelete("RSSFeeds", new List<DatabaseQuery>() { new DatabaseQuery("RSSFeedLoc", loc), new DatabaseQuery("UserName", _userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public List<RSSFeeds_Coll> RSSFeedCollection {
        get { return _RSSFeeds_Coll; }
    }

    #region -- App Web Service Calls --

    public static RSSItem GetRSSChildFeedItem(XmlNodeList childList, string search) {
        string title = "";
        string link = "";
        string description = "";
        string summary = "";
        string content = "";
        string pubDate = "";
        string creator = "";

        foreach (XmlNode node in childList) {
            if ((node.HasChildNodes) && (node.FirstChild.Name.ToLower() != "#text") && (DoesntContainSection(node.Name.ToLower()))) {
                GetRSSChildFeedItem(node.ChildNodes, search);
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
                }
            }
        }

        if (string.IsNullOrEmpty(creator))
            creator = "N/A";

        if ((!string.IsNullOrEmpty(description)) && (!string.IsNullOrEmpty(summary)) && (description != summary))
            description = description + "<br />";

        if (description == summary)
            summary = "";

        if (!string.IsNullOrEmpty(content)) {
            description = string.Empty;
            content = content.Trim().Replace("<strong>", string.Empty).Replace("</strong>", string.Empty);
        }

        // Search for match
        if (!string.IsNullOrEmpty(search)) {
            if ((!link.ToLower().Contains(search))
                && (!title.ToLower().Contains(search))
                && (!pubDate.ToLower().Contains(search))
                && (!creator.ToLower().Contains(search))
                && (!description.ToLower().Contains(search))
                && (!summary.ToLower().Contains(search))
                && (!content.ToLower().Contains(search))) {
                return new RSSItem();
            }
        }

        string newTitle = SearchRSSText(search, title);
        string newCreator = SearchRSSText(search, creator);
        string newDescription = SearchRSSText(search, description);
        string newSummary = SearchRSSText(search, summary);
        string newContent = SearchRSSText(search, content);
        string bgStyleClass = GetBackgroundImageFromFeed(link, description + summary, content);

        return new RSSItem(newTitle, link, newDescription + newSummary, newContent, pubDate, newCreator, bgStyleClass);
    }
    public static string[] GetFeedSelectionList(string _username) {
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

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(ServerSettings.GetServerMapLocation + "Apps\\RSSFeed\\RSSFeeds.xml");

        List<string> list1 = new List<string>();

        try {
            foreach (RSSFeeds_Coll feed in feedColl) {
                string name = string.Empty;
                if (feed.IsCustomFeed) {
                    name = "My Custom Feeds";
                }
                else {
                    name = RSSFeeds.GetCategoryNameFromFeed(feed.RSSID, xmlDoc);
                }

                if (!list1.Contains(name) && !string.IsNullOrEmpty(name)) {
                    list1.Add(name);
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        list1.Sort();
        return list1.ToArray();
    }

    public static string SearchRSSText(string search, string text) {
        // Search for match
        if (!string.IsNullOrEmpty(search) && text.ToLower().Contains(search)) {
            string replacement = string.Empty;
            string backcolor = "style='background-color: #FFDD49;'";
            if (text.ToLower().Contains(search)) {
                replacement = text.Substring(text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase), search.Length);
                replacement = "<span " + backcolor + ">" + replacement + "</span>";
                text = Regex.Replace(text, search, replacement, RegexOptions.IgnoreCase);
            }
        }

        return text;
    }
    public static string GetBackgroundImageFromFeed(string url, string summary, string content) {
        string bg = string.Empty;

        Regex regx = new Regex("([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?.(?:jpg|jpeg|png)", RegexOptions.IgnoreCase);

        if (!string.IsNullOrEmpty(summary)) {
            MatchCollection matches = regx.Matches(summary);
            if (matches.Count > 0) {
                bg = matches[0].Value;
            }
        }

        if (!string.IsNullOrEmpty(content) && string.IsNullOrEmpty(bg)) {
            MatchCollection matches = regx.Matches(content);
            if (matches.Count > 0) {
                bg = matches[0].Value;
            }
        }

        if (string.IsNullOrEmpty(bg)) {
            IpMethods ipMethods = new IpMethods();
            bg = ipMethods.GetWebPageImage(url);
        }

        if (!string.IsNullOrEmpty(bg) && !bg.ToLower().StartsWith("http://") && !bg.ToLower().StartsWith("https://") && !bg.ToLower().StartsWith("www.")) {
            bg = "http://" + bg;
        }
        else if (!string.IsNullOrEmpty(bg) && bg.StartsWith("/")) {
            bg = string.Empty;
        }

        if (!string.IsNullOrEmpty(bg) && bg.ToLower().Substring(bg.Length - 4) == ".gif") {
            bg = string.Empty;
        }

        if (!string.IsNullOrEmpty(bg)) {
            bg = "style=\"background-image: url('" + bg + "'); background-repeat: no-repeat; background-position: center center; background-size: cover;\" class=\"feed-preview rss-has-image\"";
        }
        else {
            bg = "class=\"feed-preview rss-has-no-image\"";
        }

        return bg;
    }

    public static string GetCategoryNameFromFeed(string id, XmlDocument xmlDoc) {
        XmlNodeList nodeList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
        foreach (XmlNode node in nodeList) {
            if (node.ChildNodes.Count >= 4) {
                if (id == node.ChildNodes[1].InnerText.Trim()) {
                    return node.ChildNodes[0].InnerText.Trim();
                }
            }
        }

        return string.Empty;
    }
    public static string[] GetCategoryInfoFromFeed(string id, XmlDocument xmlDoc) {
        string[] itemInfo = new string[3];
        XmlNodeList nodeList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
        foreach (XmlNode node in nodeList) {
            if (node.ChildNodes.Count >= 4) {
                if (id == node.ChildNodes[1].InnerText.Trim()) {
                    itemInfo[0] = node.ChildNodes[0].InnerText.Trim();
                    itemInfo[1] = node.ChildNodes[2].InnerText.Trim();
                    if (node.ChildNodes.Count > 4) {
                        itemInfo[2] = node.ChildNodes[4].InnerText.Trim();
                    }
                    else {
                        itemInfo[2] = string.Empty;
                    }
                    break;
                }
            }
        }

        return itemInfo;
    }
    public static List<string[]> GetFeedLinksFromCategory(string category, string _username) {
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

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(ServerSettings.GetServerMapLocation + "Apps\\RSSFeed\\RSSFeeds.xml");

        List<string[]> list1 = new List<string[]>();

        try {
            foreach (RSSFeeds_Coll feed in feedColl) {
                string name = string.Empty;
                string articleName = string.Empty;
                string articleImg = string.Empty;

                if (feed.IsCustomFeed) {
                    name = "My Custom Feeds";
                    articleName = feed.Title;
                }
                else {
                    string[] info = RSSFeeds.GetCategoryInfoFromFeed(feed.RSSID, xmlDoc);
                    name = info[0];
                    articleName = info[1];
                    articleImg = info[2];
                }

                if (string.IsNullOrEmpty(category) || category == name || category == "Highlights") {
                    string feedUrl = feed.URL;
                    if ((!feedUrl.StartsWith("http://")) && (!feedUrl.StartsWith("https://"))) {
                        feedUrl = "http:" + feedUrl;
                    }

                    string[] feedItem = new string[3];
                    feedItem[0] = feedUrl;
                    feedItem[1] = articleName;
                    feedItem[2] = articleImg;

                    if (!list1.Contains(feedItem) && !string.IsNullOrEmpty(feedUrl)) {
                        list1.Add(feedItem);
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return list1;
    }

    public static List<RSSFeeds_Coll> GetDemoFeeds() {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(ServerSettings.GetServerMapLocation + "Apps\\RSSFeed\\RSSFeeds.xml");

        List<RSSFeeds_Coll> coll = new List<RSSFeeds_Coll>();

        if (xmlDoc != null) {
            XmlNodeList nodeList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
            foreach (XmlNode node in nodeList) {
                if (node.ChildNodes.Count >= 4) {
                    XmlNodeList childNodes = node.ChildNodes;
                    RSSFeeds_Coll feed = new RSSFeeds_Coll(Guid.NewGuid().ToString(), string.Empty, "false", childNodes[2].InnerText, childNodes[3].InnerText, childNodes[1].InnerText, ServerSettings.ServerDateTime.ToString());
                    coll.Add(feed);
                }
            }
        }

        return coll;
    }

    public static bool DoesntContainSection(string nodeName) {
        if ((nodeName.Contains("title")) || (nodeName.Contains("link")) || (nodeName.Contains("description")) || (nodeName.Contains("summary"))
                || (nodeName.Contains("pubdate")) || (nodeName.Contains("published")) || (nodeName.Contains("dc:date"))
                || (nodeName.Contains("content:encoded")) || (nodeName.Contains("content")) || (nodeName.Contains("dc:creator"))
                || (nodeName.Contains("creator")) || (nodeName.Contains("author"))) {
            return false;
        }
        return true;
    }

    public static void GetNewFeeds(string feedLink, string source, string sourceImage, List<RSSItem> nodeList, string search) {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.DtdProcessing = DtdProcessing.Parse;

        try {
            #region Get Feeds By HttpWebRequest

            XmlDocument rssDoc = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(feedLink, readerSettings)) {
                rssDoc.Load(reader);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            DateTime lastModified = ServerSettings.ServerDateTime;
            XmlNode lastBuildDate = rssDoc.SelectSingleNode("rss/channel/lastBuildDate");
            if (lastBuildDate != null) {
                DateTime.TryParse(lastBuildDate.InnerXml, out lastModified);
            }
            else {
                lastBuildDate = rssDoc.SelectSingleNode("rss/channel/pubDate");
                if (lastBuildDate != null) {
                    DateTime.TryParse(lastBuildDate.InnerXml, out lastModified);
                }
                else {
                    lastBuildDate = rssDoc.SelectSingleNode("/atom:feed/atom:updated", nsmgr);
                    if (lastBuildDate != null) {
                        DateTime.TryParse(lastBuildDate.InnerXml, out lastModified);
                    }
                }
            }

            bool isNewFeed = IsItemNew(feedLink, lastModified);
            if (isNewFeed) {
                XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/item");
                if (rssItems.Count == 0) {
                    rssItems = rssDoc.SelectNodes("/atom:feed/atom:entry", nsmgr);
                    if (rssItems.Count == 0) {
                        rssItems = rssDoc.SelectNodes("/atom:feed/atom:entry/atom:content", nsmgr);
                        if (rssItems.Count == 0) {
                            for (int i = 0; i < rssDoc.ChildNodes.Count; i++) {
                                XmlNode x = rssDoc.ChildNodes[i];
                                if (x.Name.ToLower() == "feed")
                                    rssItems = rssDoc.ChildNodes[i].ChildNodes;
                            }
                        }
                    }
                }

                for (int i = 0; i < rssItems.Count; i++) {
                    RSSItem rssItem = RSSFeeds.GetRSSChildFeedItem(rssItems[i].ChildNodes, search);
                    if (!string.IsNullOrEmpty(rssItem.Title)) {
                        rssItem.Source = source;
                        rssItem.SourceImage = sourceImage;
                        nodeList.Add(rssItem);

                        if (!string.IsNullOrEmpty(search)) {
                            rssItem = RSSFeeds.GetRSSChildFeedItem(rssItems[i].ChildNodes, string.Empty);
                        }

                        if (!RSSFeeds.LoadedFeedList[feedLink].Value.Contains(rssItem)) {
                            RSSFeeds.LoadedFeedList[feedLink].Value.Add(rssItem);
                        }
                    }
                }
            }
            else {
                UpdateNodeList(feedLink, nodeList, search);
            }

            #endregion
        }
        catch {
            #region Try with SyndicationFeed

            try {
                SyndicationFeed feed = null;
                using (XmlReader reader = XmlReader.Create(feedLink, readerSettings)) {
                    feed = SyndicationFeed.Load(reader);
                }

                bool isNewFeed = IsItemNew(feedLink, feed.LastUpdatedTime.DateTime);
                if (isNewFeed) {
                    foreach (SyndicationItem item in feed.Items) {
                        string link = string.Empty;
                        if (item.Links.Count > 0) {
                            link = item.Links[0].Uri.OriginalString;
                        }

                        string author = string.Empty;
                        if (item.Authors.Count > 0) {
                            author = item.Authors[0].Name;
                        }

                        string content = string.Empty;
                        if (item.Content != null) {
                            TextSyndicationContent tsc = (TextSyndicationContent)item.Content;
                            content = tsc.Text;
                        }

                        // Search for match
                        if (!string.IsNullOrEmpty(search)) {
                            if ((!link.ToLower().Contains(search))
                                && (item.Title != null && !item.Title.Text.ToLower().Contains(search))
                                && (item.PublishDate != null && !item.PublishDate.ToString().ToLower().Contains(search))
                                && (!author.ToLower().Contains(search))
                                && (item.Summary != null && !item.Summary.Text.ToLower().Contains(search))
                                && (!content.ToLower().Contains(search))) {
                                continue;
                            }
                        }

                        string title_nosearch = link;
                        string title = link;
                        if (item.Title != null) {
                            title_nosearch = item.Title.Text;
                            title = RSSFeeds.SearchRSSText(search, item.Title.Text);
                        }

                        string author_nosearch = author;
                        author = RSSFeeds.SearchRSSText(search, author);
                        string summary_nosearch = string.Empty;
                        string summary = string.Empty;
                        if (item.Summary != null) {
                            summary_nosearch = item.Summary.Text;
                            summary = RSSFeeds.SearchRSSText(search, item.Summary.Text);
                        }

                        string newContent_nosearch = content;
                        string newContent = RSSFeeds.SearchRSSText(search, content);

                        bool foundImage = false;
                        string bgStyleClass = "class=\"feed-preview rss-has-no-image\"";
                        foreach (SyndicationElementExtension extension in item.ElementExtensions) {
                            XElement element = extension.GetObject<XElement>();

                            if (element.HasAttributes) {
                                foreach (var attribute in element.Attributes()) {
                                    string value = attribute.Value.ToLower();
                                    if ((value.StartsWith("http://") || value.StartsWith("https://") || value.StartsWith("www.")) && (value.EndsWith(".jpg") || value.EndsWith(".png") || value.EndsWith(".gif"))) {
                                        bgStyleClass = "style=\"background-image: url('" + value + "'); background-repeat: no-repeat; background-position: center center; background-size: cover;\" class=\"feed-preview rss-has-image\"";
                                        foundImage = true;
                                        break;
                                    }
                                }

                                if (foundImage) {
                                    break;
                                }
                            }
                        }

                        RSSItem rssItem = new RSSItem(title, link, summary, newContent, item.PublishDate.ToString(), author, bgStyleClass);
                        if (!string.IsNullOrEmpty(title)) {
                            rssItem.Source = source;
                            rssItem.SourceImage = sourceImage;
                            nodeList.Add(rssItem);

                            if (!string.IsNullOrEmpty(search)) {
                                rssItem = new RSSItem(title_nosearch, link, summary_nosearch, newContent_nosearch, item.PublishDate.ToString(), author_nosearch, bgStyleClass);
                            }

                            if (!RSSFeeds.LoadedFeedList[feedLink].Value.Contains(rssItem)) {
                                RSSFeeds.LoadedFeedList[feedLink].Value.Add(rssItem);
                            }
                        }
                    }
                }
                else {
                    UpdateNodeList(feedLink, nodeList, search);
                }
            }
            catch {
                // Just continue
            }

            #endregion
        }

    }
    public static void UpdateNodeList(string link, List<RSSItem> nodeList, string search) {
        if (RSSFeeds.LoadedFeedList.ContainsKey(link) && RSSFeeds.LoadedFeedList[link].Value != null) {
            foreach (RSSItem itemObj in RSSFeeds.LoadedFeedList[link].Value) {
                // Search for match
                if (!string.IsNullOrEmpty(search)) {
                    if ((!itemObj.Link.ToLower().Contains(search))
                        && (!itemObj.Title.ToLower().Contains(search))
                        && (!itemObj.PubDate.ToString().ToLower().Contains(search))
                        && (!itemObj.Creator.ToLower().Contains(search))
                        && (!itemObj.Summary.ToLower().Contains(search))
                        && (!itemObj.Content.ToLower().Contains(search))) {
                        continue;
                    }
                    else {
                        string newTitle = RSSFeeds.SearchRSSText(search, itemObj.Title);
                        string newCreator = RSSFeeds.SearchRSSText(search, itemObj.Creator);
                        string newSummary = RSSFeeds.SearchRSSText(search, itemObj.Summary);
                        string newContent = RSSFeeds.SearchRSSText(search, itemObj.Content);
                        RSSItem searchItem = new RSSItem(newTitle, itemObj.Link, newSummary, newContent, itemObj.PubDate.ToString(), newCreator, itemObj.BgStyleClass);
                        nodeList.Add(searchItem);
                    }
                }
                else {
                    nodeList.Add(itemObj);
                }
            }
        }
    }
    private static bool IsItemNew(string link, DateTime lastModified) {
        if (RSSFeeds.LoadedFeedList.ContainsKey(link) && RSSFeeds.LoadedFeedList[link].Key.ToString() == lastModified.ToString()) {
            return false;
        }
        else if (RSSFeeds.LoadedFeedList.ContainsKey(link)) {
            KeyValuePair<DateTime, IList<RSSItem>> keyValPair = new KeyValuePair<DateTime, IList<RSSItem>>(lastModified, new List<RSSItem>());
            RSSFeeds.LoadedFeedList[link] = keyValPair;
        }
        else if (!RSSFeeds.LoadedFeedList.ContainsKey(link)) {
            KeyValuePair<DateTime, IList<RSSItem>> keyValPair = new KeyValuePair<DateTime, IList<RSSItem>>(lastModified, new List<RSSItem>());
            RSSFeeds.LoadedFeedList.Add(link, keyValPair);
        }

        return true;
    }

    private static object _feedSync = new object();
    private const string FeedListFile = "Apps/RSSFeed/RSSFeedList.data";

    public static void SaveOutLoadedList() {
        lock (_feedSync) {
            if (RSSFeeds.LoadedFeedList.Count > 0 && !string.IsNullOrEmpty(FeedListFile)) {
                try {
                    using (var str = new BinaryWriter(File.Create(ServerSettings.GetServerMapLocation + FeedListFile))) {
                        var bf = new BinaryFormatter();
                        bf.Serialize(str.BaseStream, RSSFeeds.LoadedFeedList);
                        str.Close();
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
        }
    }

    public static void LoadRSSFeedListFile(List<string[]> feedUrls) {
        lock (_feedSync) {
            bool didNotFind = false;
            foreach (string[] feed in feedUrls) {
                if (feed.Length > 0) {
                    if (!RSSFeeds.LoadedFeedList.ContainsKey(feed[0])) {
                        didNotFind = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(FeedListFile) && didNotFind && File.Exists(ServerSettings.GetServerMapLocation + FeedListFile)) {
                try {
                    FileStream a = new FileStream(ServerSettings.GetServerMapLocation + FeedListFile, FileMode.Open);
                    using (var str = new BinaryReader(a)) {
                        var bf = new BinaryFormatter();
                        str.BaseStream.Position = 0;
                        bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                        RSSFeeds.LoadedFeedList = (Dictionary<string, KeyValuePair<DateTime, IList<RSSItem>>>)bf.Deserialize(str.BaseStream);
                        a.Close();
                        str.Close();
                    }
                }
                catch (Exception e) {
                    try {
                        File.Delete(ServerSettings.GetServerMapLocation + FeedListFile);
                    }
                    catch { }
                    AppLog.AddError(e);
                }
            }
        }
    }

    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder {
        public override Type BindToType(string assemblyName, string typeName) {
            Type typeToDeserialize = null;

            String currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }

    #endregion

}