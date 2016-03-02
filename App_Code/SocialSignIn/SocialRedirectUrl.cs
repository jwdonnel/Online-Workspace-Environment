using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SocialRedirectUrl
/// </summary>
public class SocialRedirectUrl {

    public static string GetRedirectUrl(string query) {
        string result = string.Empty;
        HttpRequest Request = HttpContext.Current.Request;

        if (Request != null) {
            string currUrl = ServerSettings.GetSitePath(Request);

            if (currUrl[currUrl.Length - 1] != '/') {
                currUrl += "/";
            }

            string absoluteUri = Request.Url.AbsoluteUri.Split('?')[0].ToLower();
            if (absoluteUri.Contains("Workspace.aspx")) {
                result = Request.Url.Scheme + ":" + currUrl + "Workspace.aspx" + query;
            }
            else if (absoluteUri.Contains("appremote.aspx")) {
                result = Request.Url.Scheme + ":" + currUrl + "AppRemote.aspx" + query;
            }
            else {
                result = Request.Url.Scheme + ":" + currUrl + ServerSettings.DefaultStartupPage + query;
            }
        }

        return result;
    }
}