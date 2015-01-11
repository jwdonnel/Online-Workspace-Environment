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

    public FTPConnect() { }

    [WebMethod]
    public string TryConnect(string ftpLocation, string username, string password) {
        try {
            string returnStr = "0";
            string errorMessage = string.Empty;

            ftpLocation = HttpUtility.UrlDecode(ftpLocation);
            username = HttpUtility.UrlDecode(username);
            password = HttpUtility.UrlDecode(password);

            FTPActions ftpActions = new FTPActions(ftpLocation, username, password);
            if (ftpActions.TryConnect(out errorMessage)) {
                returnStr = "1";
            }

            return returnStr;
        }
        catch { }

        return "0";
    }

    [WebMethod]
    public string AddCustomFTPLocation(string ftpLocation, string name, string description) {
        try {
            ftpLocation = HttpUtility.UrlDecode(ftpLocation);
            name = HttpUtility.UrlDecode(name);
            description = HttpUtility.UrlDecode(description);

            string currUser = HttpContext.Current.User.Identity.Name;
            if (HttpContext.Current.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(ftpLocation) && !string.IsNullOrEmpty(name)) {
                CustomProjects cp = new CustomProjects(currUser);
                string ProjectID = Guid.NewGuid().ToString();
                cp.AddItem(ProjectID, ftpLocation, description, name, string.Empty);
                return ProjectID;
            }
            else {
                return "Error!";
            }
        }
        catch { }

        return "";
    }

}
