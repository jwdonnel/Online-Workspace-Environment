using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
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

    public static bool IsValidAppFileType(string filename) {
        filename = filename.ToLower();
        if (!filename.Contains(".exe") && !filename.Contains(".com") && !filename.Contains(".pif") && !filename.Contains(".bat") && !filename.Contains(".scr")) {
            return true;
        }
        else if (filename.Contains("http://") || filename.Contains("https://") || filename.Contains("www.")) {
            return true;
        }

        return false;
    }
    public static bool IsValidHttpBasedAppType(string filename) {
        filename = filename.ToLower();
        if (filename.Contains("http://") || filename.Contains("https://") || filename.Contains("www.")) {
            return true;
        }

        return false;
    }
    public static bool IsValidAscxFile(string filename) {
        filename = filename.ToLower();
        if (filename.Length > 5 && filename.Substring(filename.Length - 5) == ".ascx") {
            return true;
        }

        return false;
    }
    public static bool IsValidAspxFile(string filename) {
        filename = filename.ToLower();
        if (filename.Length > 5 && filename.Substring(filename.Length - 5) == ".aspx") {
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
        DateTime now = ServerSettings.ServerDateTime;
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
        Random Random = new Random((int)ServerSettings.ServerDateTime.Ticks);
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

    public static String GetTimestamp() {
        return DateTime.Now.ToString("yyyyMMddHHmmssffff");
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
            try {
                using (var uptime = new PerformanceCounter("System", "System Up Time")) {
                    uptime.NextValue(); //Call this an extra time before reading its value
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
            catch { }

            return TimeSpan.Zero;
        }
    }

    public static IpCityState GetCityStateFromIP(string ip) {
        return new IpMethods().GetCityStateFromIp(ip);
    }

    public static bool IsValidFileFolderFormat(string fileExt) {
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
        message += "<div class='clear-space'></div>";
        return message;
    }

    public static void SetLogoOpacity(Page page, System.Web.UI.WebControls.Image img_Logo) {
        if (img_Logo != null) {
            double opacity = 1.0d;
            ServerSettings _ss = new ServerSettings();
            double.TryParse(_ss.LogoOpacity, out opacity);

            string id = img_Logo.ClientID;
            string script = "$('#" + id + "').css('opacity', '" + opacity.ToString() + "');";
            script += "$('#" + id + "').css('filter', 'alpha(opacity=" + (opacity * 100.0d).ToString() + ")');";
            RegisterPostbackScripts.RegisterStartupScript(page, script);
        }
    }

    public static string TableAddRemove(string itemsRemoved, string itemsAdded, string titleRemoved, string titleAdded, bool addExtraTopPadding, bool addBoxShadow = false) {
        string classBoxShadow = string.Empty;
        if (addBoxShadow) {
            classBoxShadow = " boxshadow";
        }

        string classPadTop = string.Empty;
        if (addExtraTopPadding) {
            classPadTop = " pad-top-big pad-bottom";
        }

        string titles = "<tr><td class='title-column'>" + titleRemoved + "</td><td class='split-column'></td><td class='title-column'>" + titleAdded + "</td></tr>";
        string strAdded = "<td id='package-added' class='item-column" + classPadTop + classBoxShadow + "'>" + itemsAdded + "</td>";
        string strRemoved = "<td id='package-removed' class='item-column" + classPadTop + classBoxShadow + "'>" + itemsRemoved + "</td>";
        return "<table class='add-remove-columntable'>" + titles + "<tr>" + strRemoved + "<td class='split-column'></td>" + strAdded + "</tr></table>";
    }

    /// <summary>
    /// Creates a javascript variable called UserIsSocialAccount if the user has a Social Network account
    /// </summary>
    /// <param name="page"></param>
    /// <param name="userName"></param>
    public static void SetIsSocialUserForDeleteItems(Page page, string userName) {
        MemberDatabase member = new MemberDatabase(userName);
        if (page != null) {
            RegisterPostbackScripts.RegisterStartupScript(page, "UserIsSocialAccount=" + member.IsSocialAccount.ToString().ToLower() + ";");
        }
    }

    public static string GetPrettyDate(DateTime d) {
        // 1.
        // Get time span elapsed since the date.
        TimeSpan s = ServerSettings.ServerDateTime.Subtract(d);

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

        return string.Empty;
    }

    public static string GetCSSGradientStyles(string primaryColor, string secondaryColor, bool reverse = false) {
        if (primaryColor.Length == 6 || (primaryColor.StartsWith("#") && primaryColor != "#")) {
            if (!primaryColor.StartsWith("#")) {
                primaryColor = "#" + primaryColor;
            }

            if (primaryColor == "inherit" || primaryColor == "#inherit") {
                primaryColor = "#FFFFFF";
            }

            if (secondaryColor == "inherit" || secondaryColor == "#inherit") {
                secondaryColor = "#FFFFFF";
            }

            int colorDifference = 40;
            if (string.IsNullOrEmpty(secondaryColor)) {
                Color _color = ColorTranslator.FromHtml(primaryColor);
                int r = _color.R;
                int g = _color.G;
                int b = _color.B;
                if (r > colorDifference) {
                    r -= colorDifference;
                }
                else {
                    r = 0;
                }

                if (g > colorDifference) {
                    g -= colorDifference;
                }
                else {
                    g = 0;
                }

                if (b > colorDifference) {
                    b -= colorDifference;
                }
                else {
                    b = 0;
                }

                secondaryColor = ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
            }

            if (!secondaryColor.StartsWith("#")) {
                secondaryColor = "#" + secondaryColor;
            }

            if (reverse) {
                string tempPrimaryColor = primaryColor;
                primaryColor = secondaryColor;
                secondaryColor = tempPrimaryColor;
            }

            string cssStyle = string.Format("background:{0};", primaryColor);
            cssStyle += string.Format("background:-moz-linear-gradient(-45deg, {0} 0%, {1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-webkit-gradient(linear, left top, right bottom, color-stop(0%,{0}), color-stop(100%,{1}));", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-webkit-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-o-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-ms-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:linear-gradient(135deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='{0}', endColorstr='{1}',GradientType=1 );", primaryColor, secondaryColor);

            return cssStyle;
        }

        if (primaryColor == "#") {
            primaryColor = "#FFFFFF";
        }

        return primaryColor;
    }

    public static bool UseDarkTextColorWithBackground(string backgroundColor) {
        bool useDarkColor = true;

        if (backgroundColor != "inherit" && backgroundColor != "#inherit") {
            Color _color = ColorTranslator.FromHtml(backgroundColor);
            if (_color.R + _color.G + _color.B < 425) {
                useDarkColor = false;
            }
        }

        return useDarkColor;
    }

    /// <summary>Save a session state for scrolling to an element before the postback happens 
    /// </summary>
    /// <param name="jqueryElement"></param>
    /// <param name="session"></param>
    public static void SaveSessionScrollToElement(string jqueryElement, HttpSessionState session) {
        if (session != null) {
            try {
                session["ScrollToJqueryElement"] = jqueryElement;
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    /// <summary>Load the ScrollToPostBack session state if available
    /// </summary>
    /// <param name="session"></param>
    /// <param name="showAnimation"></param>
    public static void LoadSavedScrollToElementSession(HttpSessionState session, Page page, bool showAnimation = false) {
        if (session != null && page != null) {
            try {
                if (session["ScrollToJqueryElement"] != null && !string.IsNullOrEmpty(session["ScrollToJqueryElement"].ToString())) {
                    RegisterPostbackScripts.RegisterStartupScript(page, "openWSE.ScrollToElement('" + session["ScrollToJqueryElement"].ToString() + "', " + showAnimation.ToString().ToLower() + ");");
                    session["ScrollToJqueryElement"] = null;
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }
}