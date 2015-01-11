using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

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
        if ((string.IsNullOrEmpty(siteUrl)) || (string.IsNullOrEmpty(host))
            || (string.IsNullOrEmpty(siteName)) || (string.IsNullOrEmpty(emailAddress))
            || (string.IsNullOrEmpty(issued)) || (string.IsNullOrEmpty(expiration))
            || (string.IsNullOrEmpty(licenseId))) {
            return "false";
        }
        else {
            string licenseFile = string.Empty;
            string garbageFile = string.Empty;
            LicenseFile lfissue = null;

            switch (type) {
                case "Issue":
                    licenseFile = ServerSettings.GetServerMapLocation + "App_Data\\" + CheckLicense.LicenseFileName;
                    garbageFile = ServerSettings.GetServerMapLocation + "App_Data\\" + CheckLicense.LicenseFileName.Replace(CheckLicense.LicenseFileNameExt, "") + "-garbage" + CheckLicense.LicenseFileNameExt;
                    try {
                        lfissue = new LicenseFile(host, siteUrl, siteName, emailAddress, expiration, issued, licenseId, ccLicenseType);
                        BinarySerialize(lfissue, licenseFile, garbageFile);
                        if (ServerSettings.SiteName != siteName) {
                            ServerSettings.update_SiteName(siteName);
                        }

                        CheckLicense.ValidateLicense();

                        return "true";
                    }
                    catch { }
                    break;

                case "Revoke":
                    lfissue = CheckLicense.LicenseFile;
                    if ((lfissue.WebsiteName == siteName) &&
                        (lfissue.EmailAddress == emailAddress) &&
                        (lfissue.DateIssued == issued) &&
                        (lfissue.ExpirationDate == expiration) &&
                        (lfissue.LicenseId == licenseId) &&
                        (lfissue.CCLicenseType == ccLicenseType)) {
                            CheckLicense.LicenseValid = false;
                        return "true";
                    }

                    break;
            }
        }

        return "false";
    }

    private void BinarySerialize(LicenseFile lf, string filename, string tempFilename) {
        try {
            if (File.Exists(filename)) {
                File.Delete(filename);
            }

            var d = new DataEncryption();
            d.SerializeFile(lf, tempFilename);
            d.EncryptFile(tempFilename, filename, ServerBackup.EncryptPassword);
        }
        catch { }
    }
}
