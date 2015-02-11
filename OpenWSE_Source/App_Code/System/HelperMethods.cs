using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HtmlAgilityPack;

public class HelperMethods
{
    public HelperMethods() { }

    /// <summary>
    /// Converts a given string or integer to a boolean value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool ConvertBitToBoolean(object value) {
        bool returnVal = false;
        if (value != null) {
            string _value = value.ToString().Trim().ToLower();

            if (_value != "true" && _value != "false" && _value != "0" && _value != "1" && !string.IsNullOrEmpty(_value)) {
                return true;
            }

            if (_value == "1") {
                return true;
            }
            else if (_value == "0") {
                return false;
            }
            else {
                if (!bool.TryParse(_value, out returnVal)) {
                    if (_value == "true") {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
        }

        return returnVal;
    }

    public static bool DoesPageContainStr(string str) {
        if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.RawUrl.ToLower().Contains(str.ToLower())) {
            return true;
        }
        return false;
    }

    private const int ImageMinimumBytes = 512;
    public static bool IsImage(object file) {
        if (file.GetType() == typeof(HttpPostedFile)) {
            HttpPostedFile postedFile = (HttpPostedFile)file;

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png") {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg") {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try {
                if (!postedFile.InputStream.CanRead) {
                    return false;
                }

                if (postedFile.ContentLength < ImageMinimumBytes) {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline)) {
                    return false;
                }
            }
            catch (Exception) {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream)) {
                }
            }
            catch (Exception) {
                return false;
            }

            return true;
        }
        else if (file.GetType() == typeof(HttpWebResponse)) {
            HttpWebResponse postedFile = (HttpWebResponse)file;

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png") {
                return false;
            }

            return true;
        }

        return false;
    }

    public static string GetFormatedTime(DateTime postDate) {
        DateTime now = DateTime.Now;
        TimeSpan final = now.Subtract(postDate);
        string time = string.Empty;
        if (final.Days >= 1) {
            time = postDate.ToShortDateString();
        }
        else {
            if (final.Hours == 0) {
                time = final.Minutes.ToString() + " minute(s) ago";
            }
            else {
                time = final.Hours.ToString() + " hour(s) ago";
            }
        }
        return time;
    }

    public static string MergeFMLNames(MemberDatabase member) {
        string name = (string.IsNullOrEmpty(member.LastName)) || (member.LastName.ToLower() == "n/a")
                          ? member.FirstName
                          : member.FirstName + " " + member.LastName;

        if ((name.ToLower() == "n/a n/a") || (name.ToLower() == "n/a") || (string.IsNullOrEmpty(name))) {
            name = member.Username;
        }
        return name;
    }

    public static string FormatBytes(float fileInBytes) {
        string strSize = "00";
        if (fileInBytes < 1024)
            strSize = fileInBytes + " B"; //Byte
        else if (fileInBytes > 1024 & fileInBytes < 1048576)
            strSize = Math.Round((fileInBytes / 1024), 2) + " KB"; //Kilobyte
        else if (fileInBytes > 1048576 & fileInBytes < 107341824)
            strSize = Math.Round((fileInBytes / 1024) / 1024, 2) + " MB"; //Megabyte
        else if (fileInBytes > 107341824 & fileInBytes < 1099511627776)
            strSize = Math.Round(((fileInBytes / 1024) / 1024) / 1024, 2) + " GB"; //Gigabyte
        else
            strSize = Math.Round((((fileInBytes / 1024) / 1024) / 1024) / 1024, 2) + " TB"; //Terabyte
        return strSize;
    }

    public static string RandomString(int size, bool upperCase = false) {
        Random Random = new Random((int)DateTime.Now.Ticks);
        var builder = new StringBuilder();
        for (var i = 0; i < size; i++) {
            char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
            builder.Append(ch);
        }

        return upperCase ? builder.ToString().ToUpper() : builder.ToString().ToLower();
    }

    public static bool CompareUserGroups(MemberDatabase member1, MemberDatabase member2) {
        List<string> group1 = member1.GroupList;
        List<string> group2 = member2.GroupList;

        return (from g1 in group1 from g2 in group2 where g2 == g1 select g1).Any();
    }

    /// <summary>
    /// This method will check a url to see that it does not return server or protocol errors
    /// </summary>
    /// <param name="url">The path to check</param>
    /// <returns></returns>
    public static bool UrlIsValid(string url, string username) {
        return new IpMethods().UrlIsValid(url, username);
    }

    public static string GetWebPageTitle(string url) {
        return new IpMethods().GetWebPageTitle(url);
    }

    public static string GetWebPageImage(string url) {
        return new IpMethods().GetWebPageImage(url);
    }

    public static bool IsMobileDevice {
        get {
            HttpRequest request = HttpContext.Current.Request;
            string userAgent = request.UserAgent;
            if (request.Browser.Type.ToUpper().Contains("IE")) {
                if (request.Browser.MajorVersion <= 7) {
                    return true;
                }
            }
            else if (!string.IsNullOrEmpty(userAgent)) {
                userAgent = userAgent.ToLower();

                // Mobile Device Array
                string[] MobileDevices = new[] { "iphone", "ppc", "windows ce", "blackberry", "opera mini", "mobile", "palm", "portable", "opera mobi", "android" };
                return MobileDevices.Any(x => userAgent.Contains(x));
            }
            return false;
        }
    }

    public static TimeSpan UpTime {
        get {
            using (var uptime = new PerformanceCounter("System", "System Up Time")) {
                uptime.NextValue(); //Call this an extra time before reading its value
                return TimeSpan.FromSeconds(uptime.NextValue());
            }
        }
    }

    public static IpCityState GetCityStateFromIP(string ip) {
        return new IpMethods().GetCityStateFromIp(ip);
    }

    public static bool IsValidCustomProjectFormat(string fileExt) {
        fileExt = fileExt.ToLower();
        if ((fileExt == ".aspx") || (fileExt == ".html") || (fileExt == ".php") || (fileExt == ".asp") ||
            (fileExt == ".htm") || (fileExt == ".xhtml") || (fileExt == ".jhtml") || (fileExt == ".php4") ||
            (fileExt == ".php3") || (fileExt == ".phtml") || (fileExt == ".xml") || (fileExt == ".rss") ||
            (fileExt == ".css") || (fileExt == ".js") || (fileExt == ".log") || (fileExt == ".conf") ||
            (fileExt == ".config") || (fileExt == ".master") || (fileExt == ".pdf") || (fileExt == ".txt")) {
            return true;
        }

        return false;
    }
    public static bool IsImageFileType(string extension) {
        extension = extension.ToLower();
        var ok = (extension == ".png") || (extension == ".bmp") || (extension == ".jpg")
                 || (extension == ".jpeg") || (extension == ".jpe") || (extension == ".jfif")
                 || (extension == ".tif") || (extension == ".tiff") || (extension == ".gif")
                 || (extension == ".tga");
        return ok;
    }
    public static bool IsValidDefaultPage(string fileExt) {
        fileExt = fileExt.ToLower();
        if ((fileExt == ".aspx") || (fileExt == ".html") || (fileExt == ".php") || (fileExt == ".asp") ||
            (fileExt == ".htm") || (fileExt == ".xhtml") || (fileExt == ".jhtml") || (fileExt == ".php4") ||
            (fileExt == ".php3") || (fileExt == ".phtml")) {
            return true;
        }

        return false;
    }

    public static string GetLockedByMessage() {
        string message = "<div class='clear-space-five'></div>";
        message += "<span class='img-lock float-left margin-right-sml'></span>";
        message += "<h3>Locked by " + ServerSettings.AdminUserName + "</h3>";
        message += "<div class='clear-space-five'></div>";
        return message;
    }
}