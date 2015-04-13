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

    private ServerSettings _ss = new ServerSettings();
    private Dictionary<string, string> _params = new Dictionary<string, string>();

    [WebMethod]
    public string AddUserFeed(string title, string caption, string search, string display, string searchType) {
        TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
        tf.addItem(Guid.NewGuid().ToString(), HttpContext.Current.User.Identity.Name, title.Trim(), caption.Trim(), search.Trim(), display.Trim(), searchType.Trim());
        
        return string.Empty;
    }

    [WebMethod]
    public string[] EditUserFeed(string id) {
        TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
        Dictionary<string, string> row = tf.GetRow(id);

        List<string> returnObj = new List<string>();
        if (row.Count > 0) {
            returnObj.Add(row["Title"]);
            returnObj.Add(row["Caption"]);
            returnObj.Add(row["TwitterSearch"]);
            returnObj.Add(row["Type"]);
            returnObj.Add(row["Display"]);
        }

        return returnObj.ToArray();
    }

    [WebMethod]
    public string UpdateUserFeed(string id, string title, string caption, string search, string display, string searchType) {
        TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
        tf.UpdateItem(id, title.Trim(), caption.Trim(), search.Trim(), display.Trim(), searchType.Trim());
        
        return string.Empty;
    }

    [WebMethod]
    public string DeleteUserFeed(string id) {
        TwitterFeeds tf = new TwitterFeeds(HttpContext.Current.User.Identity.Name, false);
        tf.deleteFeed(id);

        return string.Empty;
    }
    
    [WebMethod]
    public object[] GetUserFeeds() {            
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeParms();

            List<object> returnObj = new List<object>();
            TwitterFeeds feeds = new TwitterFeeds(HttpContext.Current.User.Identity.Name, true);
            
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
                    bool hasTitle = false;
                    bool hasCaption = false;
                    
                    if (!string.IsNullOrEmpty(entry["Title"])) {
                        hasTitle = true;
                    }
                    if (!string.IsNullOrEmpty(entry["Caption"])) {
                        hasCaption = true;
                    }
                    
                    object[] userObject = CallUserTweets(entry["TwitterSearch"], _count, hasTitle, hasCaption);

                    if (!string.IsNullOrEmpty(userObject[1].ToString()) && string.IsNullOrEmpty(obj[1].ToString())) {
                        obj[1] = userObject[1];
                    }

                    if (!string.IsNullOrEmpty(userObject[2].ToString()) && string.IsNullOrEmpty(obj[2].ToString())) {
                        obj[2] = userObject[2];
                    }
                    
                    obj.Add(userObject[0]);
                    obj.Add(userObject[3]);
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
                    object[] obj = new JavaScriptSerializer().Deserialize<object[]>(jsonResponse);

                    for (int i = 0; i < obj.Length; i++) {
                        if (obj[i] is Dictionary<string, object>) {
                            Dictionary<string, object> objDic = obj[i] as Dictionary<string, object>;
                            string serializedStr = new JavaScriptSerializer().Serialize(objDic);
                            TwitterStatus userStatus = new JavaScriptSerializer().Deserialize<TwitterStatus>(serializedStr);

                            List<string> strList = new List<string>();
                            strList.Add(ConvertUrlsToLinks(userStatus.text));

                            DateTime createdAt = DateTime.ParseExact(userStatus.created_at, "ddd MMM dd HH:mm:ss zzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            strList.Add(GetPrettyDate(createdAt));

                            if (!hasTitle && string.IsNullOrEmpty(title)) {
                                title = userStatus.user.name;
                            }

                            if (!hasCaption && string.IsNullOrEmpty(caption)) {
                                caption = userStatus.user.description;
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
                    TwitterUserMessage obj = new JavaScriptSerializer().Deserialize<TwitterUserMessage>(jsonResponse);
                    
                    for (int i = 0; i < obj.statuses.Length; i++) {
                        List<string> strList = new List<string>();
                        strList.Add(ConvertUrlsToLinks(obj.statuses[i].text));
                        
                        DateTime createdAt = DateTime.ParseExact(obj.statuses[i].created_at, "ddd MMM dd HH:mm:ss zzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        strList.Add(GetPrettyDate(createdAt));
                        
                        strList.Add(obj.statuses[i].user.screen_name);
                        strList.Add(obj.statuses[i].user.profile_image_url);
                        returnObj.Add(strList.ToArray());
                    }
                }
            }
        }
        catch { }

        return returnObj.ToArray();
    }

    private static string GetPrettyDate(DateTime d) {
        // 1.
        // Get time span elapsed since the date.
        TimeSpan s = DateTime.Now.Subtract(d);

        // 2.
        // Get total number of days elapsed.
        int dayDiff = (int)s.TotalDays;

        // 3.
        // Get total number of seconds elapsed.
        int secDiff = (int)s.TotalSeconds;

        // 4.
        // Don't allow out of range values.
        if (dayDiff < 0 || dayDiff >= 31) {
            return null;
        }

        // 5.
        // Handle same-day times.
        if (dayDiff == 0) {
            // A.
            // Less than one minute ago.
            if (secDiff < 60) {
                return "just now";
            }
            // B.
            // Less than 2 minutes ago.
            if (secDiff < 120) {
                return "1 minute ago";
            }
            // C.
            // Less than one hour ago.
            if (secDiff < 3600) {
                return string.Format("{0} minutes ago",
                    Math.Floor((double)secDiff / 60));
            }
            // D.
            // Less than 2 hours ago.
            if (secDiff < 7200) {
                return "1 hour ago";
            }
            // E.
            // Less than one day ago.
            if (secDiff < 86400) {
                return string.Format("{0} hours ago",
                    Math.Floor((double)secDiff / 3600));
            }
        }
        // 6.
        // Handle previous days.
        if (dayDiff == 1) {
            return "yesterday";
        }
        if (dayDiff < 7) {
            return string.Format("{0} days ago",
            dayDiff);
        }
        if (dayDiff < 31) {
            return string.Format("{0} weeks ago",
            Math.Ceiling((double)dayDiff / 7));
        }
        return null;
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

        _params.Add("consumer_key", _ss.TwitterConsumerKey);
        _params.Add("consumer_secret", _ss.TwitterConsumerSecret);
        _params.Add("timestamp", oauthtimestamp);
        _params.Add("nonce", nonce);
        _params.Add("signature_method", "HMAC-SHA1");
        _params.Add("signature", string.Empty);
        _params.Add("token", _ss.TwitterAccessToken);
        _params.Add("token_secret", _ss.TwitterAccessTokenSecret);
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