using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for AppLog_Errors
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AppLog_Errors : System.Web.Services.WebService {

    public AppLog_Errors() { }

    [WebMethod(EnableSession = true)]
    public void AddError(string message, string url) {
        try {
            url = HttpUtility.UrlDecode(url);
            if (url == "undefined") {
                url = string.Empty;
            }
            else {
                url = "<br />Url error occured at was " + url;
            }

            string comment = HttpUtility.UrlDecode(message) + url;
            AppLog.AddError(comment);
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
}
