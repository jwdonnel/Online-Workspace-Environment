using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for FTPConnect
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class FTPConnect : System.Web.Services.WebService {

    public FTPConnect() {
        GetSiteRequests.AddRequest();
    }

    [WebMethod]
    public string TryConnect(string ftpLocation, string username, string password) {
        try {
            string errorMessage = string.Empty;

            ftpLocation = HttpUtility.UrlDecode(ftpLocation);
            username = HttpUtility.UrlDecode(username);
            password = HttpUtility.UrlDecode(password);

            FTPActions ftpActions = new FTPActions(ftpLocation, username, password);
            if (ftpActions.TryConnect(out errorMessage)) {
                if (string.IsNullOrEmpty(errorMessage)) {
                    return string.Format("Connection to {0} was successful.", ftpLocation);
                }

                return errorMessage;
            }
        }
        catch (Exception e) {
            return e.Message;
        }

        return "Failed to connect to FTP Server!";
    }

}
