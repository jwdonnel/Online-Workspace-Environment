using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using OpenWSE.Core.Licensing;

/// <summary>
/// Summary description for LicenseUpload
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class LicenseUpload : System.Web.Services.WebService {

    public LicenseUpload() { }

    [WebMethod]
    public string UploadLicense(string siteUrl, string host, string siteName, string emailAddress, string issued, string expiration, string licenseId, string type, string ccLicenseType) {
        return OpenWSE.Core.Licensing.LicenseUpload.UploadLicense(siteUrl, host, siteName, emailAddress, issued, expiration, licenseId, type, ccLicenseType);
    }
 
}
