using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Xml;
using HtmlAgilityPack;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class IpMethods : System.Web.Services.WebService {

    public IpMethods () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public IpCityState GetCityStateFromIp(string ip) {
        IpCityState content = new IpCityState();
        if (string.IsNullOrEmpty(ip)) {
            content.IpAddress = "N/A";
            content.Country = "N/A";
            content.City = "N/A";
        }
        else if (ip == "127.0.0.1") {
            content.IpAddress = ip;
            content.Country = "UNITED STATES (US)";
            content.City = "Current Address";
        }
        else {
            string url = String.Format("http://api.hostip.info/get_html.php?ip={0}", ip.Trim());
            WebClient client = new WebClient();
            string end = client.DownloadString(url);
            string[] delim = { "\n" };
            string[] lines = end.Split(delim, StringSplitOptions.RemoveEmptyEntries);

            content.IpAddress = ip;

            foreach (string line in lines) {
                if (line.Contains("Country:")) {
                    string country = line.Replace("Country:", "").Trim();
                    content.Country = country;
                }
                else if (line.Contains("City:")) {
                    string city = line.Replace("City:", "").Trim();
                    content.City = city;
                }
            }
        }

        return content;
    }

    /// <summary>
    /// This method will check a url to see that it does not return server or protocol errors
    /// </summary>
    /// <param name="url">The path to check</param>
    /// <returns></returns>
    [WebMethod]
    public bool UrlIsValid(string url, string username) {
        try {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                DatabaseQuery dbSelect = new DatabaseCall().CallSelectSingle("aspnet_ServerSettings", "URLValidation", null);
                if (HelperMethods.ConvertBitToBoolean(dbSelect.Value)) {
                    HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                    request.Timeout = 5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                    request.Method = "HEAD"; //Get only the header information -- no need to download any content

                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                        return true;
                    else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                        return false;
                }
                else {
                    if (username.ToLower() != ServerSettings.AdminUserName.ToLower())
                        return true;
                }
            }
        }
        catch (WebException ex) {
            if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
                return false;
        }
        catch { }
        return false;
    }

    [WebMethod]
    public string GetWebPageTitle(string url) {
        string title = "";
        try {
            HttpWebRequest request = (HttpWebRequest.Create(url) as HttpWebRequest);
            HttpWebResponse response = (request.GetResponse() as HttpWebResponse);

            using (Stream stream = response.GetResponseStream()) {
                // compiled regex to check for <title></title> block
                Regex titleCheck = new Regex(@"<title>\s*(.+?)\s*</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                int bytesToRead = 8092;
                byte[] buffer = new byte[bytesToRead];
                string contents = "";
                int length = 0;
                while ((length = stream.Read(buffer, 0, bytesToRead)) > 0) {
                    // convert the byte-array to a string and add it to the rest of the
                    // contents that have been downloaded so far
                    contents += Encoding.UTF8.GetString(buffer, 0, length);

                    Match m = titleCheck.Match(contents);
                    if (m.Success) {
                        // we found a <title></title> match =]
                        title = m.Groups[1].Value.ToString();
                        break;
                    }
                    else if (contents.Contains("</head>")) {
                        // reached end of head-block; no title found =[
                        break;
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return title;
    }

    [WebMethod]
    public string GetWebPageImage(string url) {
        if (!string.IsNullOrEmpty(url)) {
            string imgUrl = string.Empty;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 1500;
                ServicePointManager.Expect100Continue = false;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader sr = new StreamReader(stream)) {
                            string responseString = sr.ReadToEnd();

                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(responseString);

                            string attrNode = "content";
                            HtmlNode imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                            if (imageNode != null && imageNode.HasAttributes &&
                                imageNode.Attributes.Contains(attrNode) && !string.IsNullOrEmpty(imageNode.Attributes[attrNode].Value)) {
                                imgUrl = imageNode.Attributes[attrNode].Value;
                            }

                            if (string.IsNullOrEmpty(imgUrl)) {
                                List<string> imgs = (from x in doc.DocumentNode.Descendants()
                                                     where x.Name.ToLower() == "img"
                                                     select x.Attributes["src"].Value).ToList<string>();

                                if (imgs.Count >= 1) {
                                    imgUrl = imgs[0];
                                }
                            }
                        }
                    }
                }
            }
            catch {
                // Do Nothing
            }

            return imgUrl;
        }

        return string.Empty;
    }

    [WebMethod]
    public string GetSiteRoot() {
        return ServerSettings.GetSitePath(HttpContext.Current.Request);
    }

}

public class IpCityState {
    public IpCityState() { }

    public string IpAddress {
        get;
        set;
    }
    public string Country {
        get;
        set;
    }
    public string City {
        get;
        set;
    }

}
