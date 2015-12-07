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

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class RSSFeed : System.Web.Services.WebService {

    private string _username;
    private int _forceUpdateInterval = 15;
    
    public RSSFeed() {
        _username = HttpContext.Current.User.Identity.Name;
        
        LoadAppParams();
        
        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            _username = GroupSessions.GetUserGroupSessionName(HttpContext.Current.User.Identity.Name);
        }
        else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            _username = GroupSessions.GetUserGroupSessionName(_username);
        }
    }

    private void LoadAppParams() {
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp("app-rssfeed");
        foreach (Dictionary<string, string> dr in appParams.listdt) {
            try {
                string param = dr["Parameter"];
                int indexOf = param.IndexOf("=") + 1;
                string subParam = param.Substring(indexOf);
                if (param.Replace("=" + subParam, string.Empty) == "OnlyUpdateInteveral") {
                    int tempOut = 15;
                    if (int.TryParse(subParam, out tempOut) && tempOut >= 0) {
                        _forceUpdateInterval = tempOut;
                    }
                }
            }
            catch { }
        }
    }

    [WebMethod]
    public object[] GetRSSFeed(string category, string search, string feedsToPull, string forOverlay) {
        object[] returnData = new object[2];
        List<RSSItem> nodeList = new List<RSSItem>();

        int totalFeedToPull = 0;
        int.TryParse(feedsToPull, out totalFeedToPull);
        if (totalFeedToPull == 0) {
            totalFeedToPull = 50;
        }

        category = HttpUtility.UrlDecode(category);
        if (category == "null" || category == "undefined") {
            category = string.Empty;
        }

        search = HttpUtility.UrlDecode(search).ToLower().Trim();
        if (search == "search current feeds") {
            search = string.Empty;
        }

        List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(category, _username);

        RSSFeeds.LoadRSSFeedListFile(feedUrls);
        
        bool needToUpdate = true;
        if (OpenWSE_Tools.AppServices.RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running ||
            OpenWSE_Tools.AppServices.RSSFeedUpdater.GetCurrentState == OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Sleeping) {
            needToUpdate = false;
        }
        else {
            TimeSpan timeSpan = ServerSettings.ServerDateTime.Subtract(RSSFeeds.FeedListDateUpdated);
            if (_forceUpdateInterval > 0 && timeSpan.TotalMinutes < _forceUpdateInterval) {
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

            if (!RSSFeeds.LoadedFeedList.ContainsKey(strItem[0]) && OpenWSE_Tools.AppServices.RSSFeedUpdater.GetCurrentState != OpenWSE_Library.Core.BackgroundServices.BackgroundStates.Running) {
                RSSFeeds.GetNewFeeds(strItem[0], strItem[1], strItem[2], nodeList, search);
            }
        }

        if (needToUpdate) {
            RSSFeeds.FeedListDateUpdated = ServerSettings.ServerDateTime;
        }

        if (string.IsNullOrEmpty(forOverlay) || forOverlay.ToLower() == "false") {
            nodeList.Sort((x, y) => DateTime.Compare(y.PubDate, x.PubDate));
            if (nodeList.Count > totalFeedToPull) {
                int totalToRemove = nodeList.Count - totalFeedToPull;
                nodeList.RemoveRange(totalFeedToPull, totalToRemove);
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            string jsonFeedList = js.Serialize(nodeList);

            returnData[0] = jsonFeedList;
            returnData[1] = RSSFeeds.GetFeedSelectionList(_username);
        }
        else {
            if (nodeList.Count > 0) {
                Random next = new Random();
                int indexNum = next.Next(0, nodeList.Count - 1);

                RSSItem overlayItem = nodeList[indexNum];

                JavaScriptSerializer js = new JavaScriptSerializer();
                string jsonFeedList = js.Serialize(new List<RSSItem> { overlayItem });

                returnData[0] = jsonFeedList;
                returnData[1] = string.Empty;
            }
        }

        return returnData;
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
            if ((!feedUrl.StartsWith("http://")) && (!feedUrl.StartsWith("https://"))) {
                feedUrl = "http:" + feedUrl;
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
                JavaScriptSerializer js = new JavaScriptSerializer();
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
                LoadAppParams();
                
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