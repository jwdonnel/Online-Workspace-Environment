<%@ WebService Language="C#" Class="TwitterStationService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.IO;
using System.Text.RegularExpressions;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class TwitterStationService  : System.Web.Services.WebService {

    private Dictionary<string, string> _params = new Dictionary<string, string>();

    private string GetUsername {
        get {
            string userFeedName = HttpContext.Current.User.Identity.Name;
            string groupId = GroupSessions.GetUserGroupSessionName(userFeedName);
            if (!string.IsNullOrEmpty(groupId)) {
                userFeedName = groupId;
            }

            return userFeedName;
        }
    }
    
    [WebMethod]
    public string AddUserFeed(string title, string caption, string search, string display, string searchType) {
        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            TwitterFeeds tf = new TwitterFeeds(GetUsername, false);
            tf.addItem(Guid.NewGuid().ToString(), GetUsername, title.Trim(), caption.Trim(), search.Trim(), display.Trim(), searchType.Trim());
        }
        
        return string.Empty;
    }

    [WebMethod]
    public string[] EditUserFeed(string id) {
        List<string> returnObj = new List<string>();
        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
            Dictionary<string, string> row = tf.GetRow(id);
            if (row.Count > 0) {
                returnObj.Add(row["Title"]);
                returnObj.Add(row["Caption"]);
                returnObj.Add(row["TwitterSearch"]);
                returnObj.Add(row["Type"]);
                returnObj.Add(row["Display"]);
            }
        }

        return returnObj.ToArray();
    }

    [WebMethod]
    public string UpdateUserFeed(string id, string title, string caption, string search, string display, string searchType) {
        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
            tf.UpdateItem(id, title.Trim(), caption.Trim(), search.Trim(), display.Trim(), searchType.Trim());
        }
        
        return string.Empty;
    }

    [WebMethod]
    public string DeleteUserFeed(string id) {
        if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name)) {
            TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
            tf.deleteFeed(id);
        }
        
        return string.Empty;
    }
    
    [WebMethod]
    public object[] GetUserFeeds() {            
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeParms();

            List<object> returnObj = new List<object>();
            returnObj.Add(OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(HttpContext.Current.User.Identity.Name).ToString().ToLower());
            TwitterFeeds feeds = new TwitterFeeds(GetUsername, true);
            
            foreach (Dictionary<string, string> entry in feeds.twitter_list) {
                List<object> obj = new List<object>();
                obj.Add(entry["ID"]);
                obj.Add(entry["Title"]);
                obj.Add(entry["Caption"]);

                int _count = 10;
                if (!string.IsNullOrEmpty(entry["Display"]) && !int.TryParse(entry["Display"], out _count)) {
                    _count = 10;
                }

                if (entry["Type"] == "Profile") {
                    object[] userObject = CallUserTweets(entry["TwitterSearch"], _count, !string.IsNullOrEmpty(entry["Title"]), !string.IsNullOrEmpty(entry["Caption"]));

                    if (!string.IsNullOrEmpty(userObject[1].ToString()) && string.IsNullOrEmpty(obj[1].ToString())) {
                        obj[1] = userObject[1];
                    }

                    if (!string.IsNullOrEmpty(userObject[2].ToString()) && string.IsNullOrEmpty(obj[2].ToString())) {
                        obj[2] = userObject[2];
                    }
                    
                    obj.Add(userObject[0]);
                    obj.Add(userObject[3]);
                    obj.Add(userObject[4]);
                }
                else {
                    if (string.IsNullOrEmpty(obj[1].ToString())) {
                        obj[1] = "Twitter Search: " + entry["TwitterSearch"];
                    }
                    obj.Add(CallSearchTweets(entry["TwitterSearch"], _count));
                }
                
                returnObj.Add(obj);
            }

            return returnObj.ToArray();
        }

        return new object[0];
    }

    private object[] CallUserTweets(string user, int count, bool hasTitle, bool hasCaption) {
        List<object> returnObj = new List<object>();

        string userImage = string.Empty;
        string title = string.Empty;
        string caption = string.Empty;
        string screenName = string.Empty;
        
        if (user.Length > 0 && user[0] == '@') {
            user = user.Remove(0, 1);
        }
        
        string requestUri = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=" + user + "&count=" + count;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
        request.Method = "GET";
        request.ContentType = "application/json";

        string authHeader = TwitterOAuth1(request);
        request.Headers.Add("Authorization", authHeader);

        try {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {
                    string jsonResponse = streamReader.ReadToEnd();
                    JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
                    object[] obj = js.Deserialize<object[]>(jsonResponse);

                    for (int i = 0; i < obj.Length; i++) {
                        if (obj[i] is Dictionary<string, object>) {
                            Dictionary<string, object> objDic = obj[i] as Dictionary<string, object>;
                            string serializedStr = js.Serialize(objDic);
                            TwitterStatus userStatus = js.Deserialize<TwitterStatus>(serializedStr);

                            List<string> strList = new List<string>();
                            strList.Add(ConvertUrlsToLinks(userStatus.text));

                            DateTime createdAt = DateTime.ParseExact(userStatus.created_at, "ddd MMM dd HH:mm:ss zzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            strList.Add(HelperMethods.GetPrettyDate(createdAt));
                            strList.Add(createdAt.Ticks.ToString());
                            
                            if (!hasTitle && string.IsNullOrEmpty(title)) {
                                title = userStatus.user.name;
                            }

                            if (!hasCaption && string.IsNullOrEmpty(caption)) {
                                caption = userStatus.user.description;
                            }

                            if (string.IsNullOrEmpty(screenName)) {
                                screenName = userStatus.user.screen_name;
                            }
                            
                            if (string.IsNullOrEmpty(userImage)) {
                                userImage = userStatus.user.profile_image_url;
                            }
                            returnObj.Add(strList.ToArray());
                        }
                    }
                }
            }
        }
        catch { }

        List<object> newReturnObj = new List<object>();
        newReturnObj.Add(userImage);
        newReturnObj.Add(title);
        newReturnObj.Add(caption);
        newReturnObj.Add(screenName);
        newReturnObj.Add(returnObj);
        return newReturnObj.ToArray();
    }

    private object[] CallSearchTweets(string search, int count) {
        List<object> returnObj = new List<object>();
        
        string requestUri = "https://api.twitter.com/1.1/search/tweets.json?q=" + search + "&count=" + count;
        
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
        request.Method = "GET";
        request.ContentType = "application/json";

        string authHeader = TwitterOAuth1(request);
        request.Headers.Add("Authorization", authHeader);

        try {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {
                    string jsonResponse = streamReader.ReadToEnd();
                    TwitterUserMessage obj = ServerSettings.CreateJavaScriptSerializer().Deserialize<TwitterUserMessage>(jsonResponse);
                    
                    for (int i = 0; i < obj.statuses.Length; i++) {
                        List<string> strList = new List<string>();
                        strList.Add(ConvertUrlsToLinks(obj.statuses[i].text));
                        
                        DateTime createdAt = DateTime.ParseExact(obj.statuses[i].created_at, "ddd MMM dd HH:mm:ss zzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        strList.Add(HelperMethods.GetPrettyDate(createdAt));
                        
                        strList.Add(obj.statuses[i].user.name);
                        strList.Add(obj.statuses[i].user.profile_image_url);
                        strList.Add(createdAt.Ticks.ToString());
                        strList.Add(obj.statuses[i].user.screen_name);
                        
                        returnObj.Add(strList.ToArray());
                    }
                }
            }
        }
        catch { }

        return returnObj.ToArray();
    }

    private static string ConvertUrlsToLinks(string msg) {
        const string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
        var r = new Regex(regex, RegexOptions.IgnoreCase);
        return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" style=\"text-decoration: underline; color: #0077CC;\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
    }

    private void InitializeParms() {
        string oauthtimestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();

        Random _random = new Random();
        string nonce = _random.Next(123400, 9999999).ToString();

        Dictionary<string, string> authParms = GetAuthParms();

        _params.Add("consumer_key", authParms["Consumer_Key"]);
        _params.Add("consumer_secret", authParms["Consumer_Secret"]);
        _params.Add("timestamp", oauthtimestamp);
        _params.Add("nonce", nonce);
        _params.Add("signature_method", "HMAC-SHA1");
        _params.Add("signature", string.Empty);
        _params.Add("token", authParms["Access_Token"]);
        _params.Add("token_secret", authParms["Access_Token_Secret"]);
        _params.Add("version", "1.0");
    }

    private string TwitterOAuth1(HttpWebRequest request) {
        string oauthsignature = GetSignatureBase(request.RequestUri.OriginalString, request.Method);
        
        var hash = GetHash();
        byte[] dataBuffer = Encoding.ASCII.GetBytes(oauthsignature);
        byte[] hashBytes = hash.ComputeHash(dataBuffer);

        _params["signature"] = Convert.ToBase64String(hashBytes);

        var sb = new System.Text.StringBuilder();
        foreach (KeyValuePair<String, String> item in _params.OrderBy(x => x.Key)) {
            if (!String.IsNullOrEmpty(item.Value) && !item.Key.EndsWith("secret")) {
                sb.AppendFormat("oauth_{0}=\"{1}\", ", item.Key, UrlEncode(item.Value));
            }
        }

        return "OAuth " + sb.ToString().TrimEnd(' ').TrimEnd(',');
    }
    
    private HashAlgorithm GetHash() {
        string keyString = string.Format("{0}&{1}", UrlEncode(_params["consumer_secret"]), UrlEncode(_params["token_secret"]));
        var hmacsha1 = new HMACSHA1();
        hmacsha1.Key = Encoding.ASCII.GetBytes(keyString);
        return hmacsha1;
    }

    private string GetSignatureBase(string url, string method) {
        var uri = new Uri(url);
        var normUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);
        if (!((uri.Scheme == "http" && uri.Port == 80) ||
              (uri.Scheme == "https" && uri.Port == 443)))
            normUrl += ":" + uri.Port;

        normUrl += uri.AbsolutePath;

        var sb = new System.Text.StringBuilder();
        sb.Append(method)
            .Append('&')
            .Append(UrlEncode(normUrl))
            .Append('&');

        var p = ExtractQueryParameters(uri.Query); 
        foreach (var p1 in _params) {
            if (!String.IsNullOrEmpty(_params[p1.Key]) &&
                !p1.Key.EndsWith("_secret") &&
                !p1.Key.EndsWith("signature"))
                p.Add("oauth_" + p1.Key, p1.Value);
        }

        var sb1 = new System.Text.StringBuilder();
        foreach (KeyValuePair<String, String> item in p.OrderBy(x => x.Key)) {
            sb1.AppendFormat("{0}={1}&", item.Key, item.Value);
        }

        sb.Append(UrlEncode(sb1.ToString().TrimEnd('&')));
        var result = sb.ToString();
        return result;
    }

    private Dictionary<String, String> ExtractQueryParameters(string queryString) {
        if (queryString.StartsWith("?"))
            queryString = queryString.Remove(0, 1);

        var result = new Dictionary<String, String>();

        if (string.IsNullOrEmpty(queryString))
            return result;

        foreach (string s in queryString.Split('&')) {
            if (!string.IsNullOrEmpty(s) && !s.StartsWith("oauth_")) {
                if (s.IndexOf('=') > -1) {
                    string[] temp = s.Split('=');
                    result.Add(temp[0], temp[1]);
                }
                else
                    result.Add(s, string.Empty);
            }
        }

        return result;
    }

    public static string UrlEncode(string value) {
        string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        
        StringBuilder stringBuilder = new StringBuilder();
        string str = value;
        for (int i = 0; i < str.Length; i++) {
            char chr = str[i];
            if (unreservedChars.IndexOf(chr) == -1) {
                stringBuilder.Append(string.Concat('%', string.Format("{0:X2}", (int)chr)));
            }
            else {
                stringBuilder.Append(chr);
            }
        }
        return stringBuilder.ToString();
    }

    private Dictionary<string, string> GetAuthParms() {
        string[] delim = { "=" };
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp("app-twitterstation");
        Dictionary<string, string> dicParams = new Dictionary<string, string>();
        foreach (Dictionary<string, string> dr in appParams.listdt) {
            string[] paramSplit = dr["Parameter"].Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (paramSplit.Length == 2) {
                string key = paramSplit[0];
                string val = paramSplit[1];
                if (!dicParams.ContainsKey(key)) {
                    dicParams.Add(key, val);
                }
            }
        }

        Dictionary<string, string> authParms = new Dictionary<string, string>();
        authParms.Add("Access_Token", string.Empty);
        authParms.Add("Access_Token_Secret", string.Empty);
        authParms.Add("Consumer_Key", string.Empty);
        authParms.Add("Consumer_Secret", string.Empty);
        
        if (dicParams.ContainsKey("Access_Token")) {
            authParms["Access_Token"] = dicParams["Access_Token"];
        }
        if (dicParams.ContainsKey("Access_Token_Secret")) {
            authParms["Access_Token_Secret"] = dicParams["Access_Token_Secret"];
        }
        if (dicParams.ContainsKey("Consumer_Key")) {
            authParms["Consumer_Key"] = dicParams["Consumer_Key"];
        }
        if (dicParams.ContainsKey("Consumer_Secret")) {
            authParms["Consumer_Secret"] = dicParams["Consumer_Secret"];
        }

        return authParms;
    }
    
}

public class TwitterUserMessage {
    public TwitterStatus[] statuses { get; set; }
}

public class TwitterStatus {
    public string text { get; set; }
    public string created_at { get; set; }
    public TwitterUserProfile user { get; set; }
}

public class TwitterUserProfile {
    public string id { get; set; }
    public string name { get; set; }
    public string screen_name { get; set; }
    public string description { get; set; }
    public string profile_image_url { get; set; }
}