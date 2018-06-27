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
using System.ServiceModel.Syndication;
using System.Web.Script.Serialization;
using OpenWSE_Tools.AppServices;
using OpenWSE_Tools.Notifications;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class RSSFeed : System.Web.Services.WebService {

    private string _username;

    public RSSFeed() {
        _username = HttpContext.Current.User.Identity.Name;

        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            _username = GroupSessions.GetUserGroupSessionName(HttpContext.Current.User.Identity.Name);
        }
        else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            _username = GroupSessions.GetUserGroupSessionName(_username);
        }
    }

    [WebMethod]
    public object[] GetRSSFeedOverlay() {
        object[] returnData = new object[2];
        List<RSSItem> nodeList = new List<RSSItem>();
        List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(string.Empty, _username);
        if (feedUrls.Count == 0) {
            feedUrls = RSSFeeds.GetFeedLinksFromCategory(string.Empty, string.Empty);
        }

        RSSFeeds.LoadRSSFeedListFile(feedUrls);

        bool needToUpdate = true;
        if (RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running ||
            RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Sleeping) {
            needToUpdate = false;
        }
        else {
            TimeSpan timeSpan = ServerSettings.ServerDateTime.Subtract(RSSFeeds.FeedListDateUpdated);
            if (RSSFeedUpdater.ForceUpdateInterval > 0 && timeSpan.TotalMinutes < RSSFeedUpdater.ForceUpdateInterval) {
                needToUpdate = false;
            }
        }

        foreach (string[] strItem in feedUrls) {
            if (!needToUpdate) {
                if (RSSFeeds.LoadedFeedList.ContainsKey(strItem[0])) {
                    RSSFeeds.UpdateNodeList(strItem[0], nodeList, string.Empty);
                    continue;
                }
            }

            if (!RSSFeeds.LoadedFeedList.ContainsKey(strItem[0]) && RSSFeedUpdater.GetCurrentState != OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running) {
                RSSFeeds.GetNewFeeds(strItem[0], strItem[1], strItem[2], nodeList, string.Empty);
            }
        }

        if (needToUpdate) {
            RSSFeeds.FeedListDateUpdated = ServerSettings.ServerDateTime;
        }

        if (nodeList.Count > 0) {
            Random next = new Random();
            int indexNum = next.Next(0, nodeList.Count - 1);

            RSSItem overlayItem = nodeList[indexNum];

            JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
            string jsonFeedList = js.Serialize(new List<RSSItem> { overlayItem });

            returnData[0] = jsonFeedList;
            returnData[1] = string.Empty;
        }

        return returnData;
    }

    [WebMethod]
    public string GetRSSFeedStation(string category, string search) {
        List<RSSItem> nodeList = new List<RSSItem>();
        category = HttpUtility.UrlDecode(category);
        if (category == "null" || category == "undefined") {
            category = string.Empty;
        }

        search = HttpUtility.UrlDecode(search).ToLower().Trim();

        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();

        if (category == "Saved Feeds" && !string.IsNullOrEmpty(_username)) {
            RSSFeeds feeds = new RSSFeeds(_username);
            List<Dictionary<string, string>> savedFeedList = feeds.GetSavedFeeds();
            foreach (Dictionary<string, string> savedFeed in savedFeedList) {
                try {
                    Dictionary<string, object> item = (Dictionary<string, object>)js.DeserializeObject(savedFeed["FeedData"]);
                    RSSItem itemObj = new RSSItem(item["Title"].ToString(), item["Link"].ToString(), item["Summary"].ToString(), item["Content"].ToString(), item["PubDate"].ToString(), item["Creator"].ToString(), item["BgStyleClass"].ToString());

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
                catch { }
            }
        }
        else if (category == "My Alerts" && !string.IsNullOrEmpty(_username)) {
            UserNotificationMessages un = new UserNotificationMessages(_username);
            List<string> alertList = un.getMessagesByNotificationID("app-rssfeed");
            List<string> returnList = new List<string>();

            if (!string.IsNullOrEmpty(search)) {
                foreach (string alertMessage in alertList) {
                    if (alertMessage.ToLower().Contains(search)) {
                        returnList.Add(alertMessage);
                    }
                }
            }
            else {
                returnList = alertList;
            }

            return js.Serialize(returnList);
        }
        else {
            List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(category, _username);
            if (feedUrls.Count == 0) {
                feedUrls = RSSFeeds.GetFeedLinksFromCategory(category, string.Empty);
            }

            RSSFeeds.LoadRSSFeedListFile(feedUrls);

            bool needToUpdate = true;
            if (RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running ||
                RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Sleeping ||
                (RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Stopped && RSSFeeds.LoadedFeedList != null && RSSFeeds.LoadedFeedList.Count > 0)) {
                needToUpdate = false;
            }
            else {
                TimeSpan timeSpan = ServerSettings.ServerDateTime.Subtract(RSSFeeds.FeedListDateUpdated);
                if (RSSFeedUpdater.ForceUpdateInterval > 0 && timeSpan.TotalMinutes < RSSFeedUpdater.ForceUpdateInterval) {
                    needToUpdate = false;
                }
            }

            foreach (string[] strItem in feedUrls) {
                if (!needToUpdate) {
                    if (RSSFeeds.LoadedFeedList.ContainsKey(strItem[0])) {
                        RSSFeeds.UpdateNodeList(strItem[0], nodeList, search);
                        continue;
                    }
                }

                if (!RSSFeeds.LoadedFeedList.ContainsKey(strItem[0]) && RSSFeedUpdater.GetCurrentState != OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running && RSSFeedUpdater.GetCurrentState != OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Stopped) {
                    RSSFeeds.GetNewFeeds(strItem[0], strItem[1], strItem[2], nodeList, search);
                }
            }

            if (needToUpdate) {
                RSSFeeds.FeedListDateUpdated = ServerSettings.ServerDateTime;
            }
        }

        try {
            nodeList.Sort((x, y) => DateTime.Compare(y.PubDate, x.PubDate));
            return js.Serialize(nodeList);
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return string.Empty;
    }

    [WebMethod]
    public object GetUserFeedAlerts() {
        List<KeyValuePair<string, string>> returnVals = new List<KeyValuePair<string, string>>();
        Dictionary<string, string> keywords = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(_username)) {
            RSSAlerts alerts = new RSSAlerts(_username);
            keywords = alerts.GetKeywords();
            foreach (KeyValuePair<string, string> entry in keywords) {
                returnVals.Add(entry);
            }
        }

        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
        return js.Serialize(returnVals);
    }

    [WebMethod]
    public object AddFeedAlert(string keyword) {
        if (!string.IsNullOrEmpty(_username)) {
            RSSAlerts alerts = new RSSAlerts(_username);
            keyword = HttpUtility.UrlDecode(keyword);
            alerts.AddItem(keyword);
        }

        return GetUserFeedAlerts();
    }

    [WebMethod]
    public object UpdateFeedAlert(string id, string keyword) {
        if (!string.IsNullOrEmpty(_username)) {
            RSSAlerts alerts = new RSSAlerts(_username);
            keyword = HttpUtility.UrlDecode(keyword);
            alerts.UpdateItem(id, keyword);
        }

        return GetUserFeedAlerts();
    }

    [WebMethod]
    public object DeleteFeedAlert(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            RSSAlerts alerts = new RSSAlerts(_username);
            alerts.DeleteItem(id);
        }

        return GetUserFeedAlerts();
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
            feedColl = RSSFeeds.GetDemoFeeds();
        }


        List<string> list1 = new List<string>();
        List<string> list2 = new List<string>();
        List<string> list3 = new List<string>();
        List<string> list4 = new List<string>();

        foreach (RSSFeeds_Coll feed in feedColl) {
            string feedUrl = feed.URL;
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

    [WebMethod]
    public void AddRemoveFeed(string _title, string _url, string _rssid, string _needAdd) {
        if (!string.IsNullOrEmpty(_username)) {
            _title = HttpUtility.UrlDecode(_title);
            _url = HttpUtility.UrlDecode(_url);
            RSSFeeds feeds = new RSSFeeds(_username);
            if (HelperMethods.ConvertBitToBoolean(_needAdd))
                feeds.AddItem(_title, _url, _rssid, false);
            else
                feeds.DeleteRowByRSSID(_rssid);
        }
    }

    [WebMethod]
    public string SaveFeed(string category, string id) {
        if (!string.IsNullOrEmpty(_username)) {
            category = HttpUtility.UrlDecode(category);
            id = HttpUtility.UrlDecode(id);
            string search = string.Empty;
            if (category == "null" || category == "undefined") {
                category = string.Empty;
            }

            List<RSSItem> nodeList = new List<RSSItem>();
            List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(category, _username);

            if (feedUrls.Count == 0) {
                feedUrls = RSSFeeds.GetFeedLinksFromCategory(category, string.Empty);
            }

            RSSFeeds.LoadRSSFeedListFile(feedUrls);

            foreach (string[] strItem in feedUrls) {
                if (RSSFeeds.LoadedFeedList.ContainsKey(strItem[0])) {
                    RSSFeeds.UpdateNodeList(strItem[0], nodeList, string.Empty);
                    continue;
                }
            }

            RSSFeeds feeds = new RSSFeeds(_username);
            JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
            List<Dictionary<string, string>> savedFeeds = feeds.GetSavedFeeds();

            foreach (RSSItem item in nodeList) {
                string tempId = CreateRSSFeedId(item.Title, item.Link);
                if (id == tempId) {
                    if (!ContainsSavedFeed(savedFeeds, id, js)) {
                        string data = js.Serialize(item);
                        feeds.AddSavedFeed(data);
                        return string.Empty;
                    }
                    else {
                        return "Feed already saved";
                    }
                }
            }
        }

        return "Failed to save feed";
    }
    private bool ContainsSavedFeed(List<Dictionary<string, string>> savedFeeds, string id, JavaScriptSerializer js) {
        foreach (Dictionary<string, string> feed in savedFeeds) {
            try {
                Dictionary<string, object> item = (Dictionary<string, object>)js.DeserializeObject(feed["FeedData"]);
                string tempId = CreateRSSFeedId(item["Title"].ToString(), item["Link"].ToString());
                if (id == tempId) {
                    return true;
                }
            }
            catch { }
        }

        return false;
    }
    private string CreateRSSFeedId(string title, string link) {
        string id = title + "-" + link;
        id = id.Replace("'", "");
        id = id.Replace("’", "");
        id = id.Replace("\"", "");
        id = id.Replace("–", "-");
        id = id.Replace(",", "");
        id = id.Replace(";", "");
        id = id.Replace(" ", "%20");
        id = id.Replace("+", "%43;");
        id = id.Replace("(", "%28");
        id = id.Replace(")", "%29");
        id = id.Replace(":", "%3A");
        id = id.Replace("~", "%7E");
        id = id.Replace("|", "%7C");
        id = id.Replace("?", "%3F");
        id = id.Replace("…", "%u2026");
        id = id.Replace("&nbsp;", "%A0");
        id = HttpUtility.HtmlEncode(id);
        id = id.Replace("&#160;", "%A0");
        id = HttpUtility.HtmlDecode(id);
        id = id.Replace("&", "%26");
        return id;
    }

    [WebMethod]
    public void RemoveSavedFeed(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            id = HttpUtility.UrlDecode(id);

            RSSFeeds feeds = new RSSFeeds(_username);
            JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
            List<Dictionary<string, string>> savedFeeds = feeds.GetSavedFeeds();

            foreach (Dictionary<string, string> feed in savedFeeds) {
                try {
                    Dictionary<string, object> item = (Dictionary<string, object>)js.DeserializeObject(feed["FeedData"]);
                    string tempId = CreateRSSFeedId(item["Title"].ToString(), item["Link"].ToString());
                    if (id == tempId) {
                        feeds.DeleteSavedFeed(feed["ID"]);
                        break;
                    }
                }
                catch { }
            }
        }
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

    [WebMethod(Description = "Clears the LoadedFeedList used for the RSS Feed app. Note, you must be either the administrator or have the app install in order to complete this service.")]
    public string RSSFeeds_Clear_LoadedFeedList() {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string username = HttpContext.Current.User.Identity.Name;
            MemberDatabase member = new MemberDatabase(username);
            if (username.ToLower() == "administrator" || member.UserHasApp("app-rssfeed")) {
                RSSFeeds.LoadedFeedList.Clear();
                return "LoadedFeedList has been cleared!";
            }
        }

        return "Not authorized to clear the LoadedFeedList.";
    }

    [WebMethod(Description = "Prints the LoadedFeedList used for the RSS Feed app in JSON format. Note, you must be either the administrator or have the app install in order to complete this service.")]
    public string RSSFeeds_Print_LoadedFeedList() {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string username = HttpContext.Current.User.Identity.Name;
            MemberDatabase member = new MemberDatabase(username);
            if (username.ToLower() == "administrator" || member.UserHasApp("app-rssfeed")) {
                JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
                string jsonFeedList = string.Empty;
                try {
                    jsonFeedList = js.Serialize(RSSFeeds.LoadedFeedList);
                }
                catch (Exception e) {
                    jsonFeedList = e.Message;
                }

                return jsonFeedList;
            }
        }

        return "Not authorized to print the LoadedFeedList.";
    }

    [WebMethod(Description = "Updates the LoadedFeedList used for the RSS Feed app by getting all the latest feeds. Note, you must be either the administrator or have the app install in order to complete this service.")]
    public string RSSFeeds_Update_LoadedFeedList() {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string username = HttpContext.Current.User.Identity.Name;
            MemberDatabase member = new MemberDatabase(username);
            if (username.ToLower() == "administrator" || member.UserHasApp("app-rssfeed")) {
                List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(string.Empty, string.Empty);

                RSSFeeds.FeedListDateUpdated = ServerSettings.ServerDateTime;

                List<RSSItem> nodeList = new List<RSSItem>();
                foreach (string[] strItem in feedUrls) {
                    RSSFeeds.GetNewFeeds(strItem[0], strItem[1], strItem[2], nodeList, string.Empty);
                }

                return "LoadedFeedList has been updated!";
            }
        }

        return "Not authorized to update the LoadedFeedList.";
    }

    [WebMethod(Description = "Get the total count of the Loaded RSS Feed list.")]
    public string RSSFeeds_Get_LoadedFeedList_Count() {
        return "Total feeds saved: " + RSSFeeds.LoadedFeedList.Count.ToString();
    }

    [WebMethod(Description = "Try to save the RSS Feed List to the data file stored in the Apps folder.")]
    public string RSSFeeds_Save_LoadedFeedList_ToFile() {
        string returnMessage = "No file found or no feeds loaded to save.";

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string username = HttpContext.Current.User.Identity.Name;
            MemberDatabase member = new MemberDatabase(username);
            if (username.ToLower() == "administrator" || member.UserHasApp("app-rssfeed")) {
                string FeedListFile = "Apps/RSSFeed/RSSFeedList.data";
                if (RSSFeeds.LoadedFeedList.Count > 0 && !string.IsNullOrEmpty(FeedListFile)) {
                    try {
                        using (var str = new BinaryWriter(File.Create(ServerSettings.GetServerMapLocation + FeedListFile))) {
                            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            bf.Serialize(str.BaseStream, RSSFeeds.LoadedFeedList);
                            str.Close();

                            returnMessage = "File has been saved as RSSFeedList.data. Total feeds saved: " + RSSFeeds.LoadedFeedList.Count.ToString();
                        }
                    }
                    catch (Exception e) {
                        return e.Message;
                    }
                }

                return returnMessage;
            }
        }

        return "Not authorized to save the LoadedFeedList.";
    }

    [WebMethod(Description = "Try to load the RSS Feed List from the data file stored in the Apps folder.")]
    public string RSSFeeds_Load_LoadedFeedList_FromFile() {
        string returnMessage = "No file found or no feeds loaded.";

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string username = HttpContext.Current.User.Identity.Name;
            MemberDatabase member = new MemberDatabase(username);
            if (username.ToLower() == "administrator" || member.UserHasApp("app-rssfeed")) {
                string FeedListFile = "Apps/RSSFeed/RSSFeedList.data";
                if (!string.IsNullOrEmpty(FeedListFile) && File.Exists(ServerSettings.GetServerMapLocation + FeedListFile)) {
                    try {
                        FileStream a = new FileStream(ServerSettings.GetServerMapLocation + FeedListFile, FileMode.Open);
                        using (var str = new BinaryReader(a)) {
                            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            str.BaseStream.Position = 0;
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                            RSSFeeds.LoadedFeedList = (Dictionary<string, KeyValuePair<DateTime, IList<RSSItem>>>)bf.Deserialize(str.BaseStream);
                            a.Close();
                            str.Close();

                            returnMessage = "Feeds Loaded: " + RSSFeeds.LoadedFeedList.Count.ToString();
                        }
                    }
                    catch (Exception e) {
                        return e.Message;
                    }
                }

                return returnMessage;
            }
        }

        return "Not authorized to load the LoadedFeedList.";
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
}