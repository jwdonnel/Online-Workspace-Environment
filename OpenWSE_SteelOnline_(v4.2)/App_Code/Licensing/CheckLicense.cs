using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Web.Security;
using System.Xml;


/// <summary>
/// Summary description for GetLicense
/// </summary>
public class CheckLicense {
    public const string TrialKey = "70b19681-b8c7-4d2c-9d6c-cfb2c923448d";

    private static bool _licenseValid = false; // Set to true to override license check
    private static bool _isExpired = false;
    private static bool _isTrial = false;
    private static bool _trialActivated = false;
    private static string _daysLeftBeforeExpired = "";

    public const string LicenseFileNameExt = ".lf";
    public static string LicenseFileName {
        get{
            return "ProductLicense" + LicenseFileNameExt;
        }
    }

    public static void ValidateLicense() {
        try {
            string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
            if (!File.Exists(_solFile)) {
                _licenseValid = false;
                HttpContext.Current.Response.Redirect("~/ErrorPages/LicenseInvalid.html");
            }

            if (_trialActivated || !_licenseValid) {
                LicenseFile lf = GetLicenseObject(_solFile);
                DateTime expirationDate = new DateTime();


                #region Set Trial Activation Page
                string currPage = HttpContext.Current.Request.Url.Segments[HttpContext.Current.Request.Url.Segments.Length - 1];
                if (lf.LicenseId == TrialKey) {
                    _isTrial = true;

                    if (DateTime.TryParse(lf.ExpirationDate, out expirationDate)) {
                        TimeSpan ts = expirationDate.Subtract(DateTime.Now);
                        GetDaysLeftBeforeExpired(ts);
                    }

                    if (currPage.ToLower() != "licensemanager.aspx") {
                        HttpContext.Current.Response.Redirect("~/SiteSettings/ServerMaintenance/LicenseManager.aspx");
                    }
                    else {
                        return;
                    }
                }

                _isTrial = false;
                #endregion


                if ((string.IsNullOrEmpty(lf.Host)) || (string.IsNullOrEmpty(lf.ExpirationDate))
                    || (string.IsNullOrEmpty(lf.WebsiteUrl)) || (string.IsNullOrEmpty(lf.EmailAddress))
                    || (string.IsNullOrEmpty(lf.DateIssued)) || (string.IsNullOrEmpty(lf.LicenseId))
                    || (string.IsNullOrEmpty(lf.WebsiteName)) || (string.IsNullOrEmpty(lf.CCLicenseType))) {
                    _licenseValid = false;
                }

                _isExpired = false;
                _trialActivated = false;
                if (DateTime.TryParse(lf.ExpirationDate, out expirationDate)) {
                    TimeSpan ts = expirationDate.Subtract(DateTime.Now);
                    if (ts.TotalSeconds < 0) {
                        _isExpired = true;

                        if (lf.LicenseId.Contains("-TRIAL")) {
                            _trialActivated = true;
                            if (currPage.ToLower() != "licensemanager.aspx") {
                                HttpContext.Current.Response.Redirect("~/SiteSettings/ServerMaintenance/LicenseManager.aspx");
                            }
                            else {
                                return;
                            }
                        }
                    }

                    GetDaysLeftBeforeExpired(ts);
                }

                // Dont even bother continuing if the license is expired
                if (!_isExpired) {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lf.Host);
                    request.KeepAlive = false;

                    StringBuilder dataString = new StringBuilder();

                    string sentFromUrl = GetSentFromUrl();

                    dataString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    dataString.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                    dataString.Append("<soap:Body>");
                    dataString.Append("<ValidateLicense xmlns=\"http://tempuri.org/\">");
                    dataString.Append("<siteUrl>" + lf.WebsiteUrl + "</siteUrl>");
                    dataString.Append("<fromUrl>" + sentFromUrl + "</fromUrl>");
                    dataString.Append("<siteName>" + lf.WebsiteName + "</siteName>");
                    dataString.Append("<emailAddress>" + lf.EmailAddress + "</emailAddress>");
                    dataString.Append("<issued>" + lf.DateIssued + "</issued>");
                    dataString.Append("<expiration>" + lf.ExpirationDate + "</expiration>");
                    dataString.Append("<licenseId>" + lf.LicenseId + "</licenseId>");
                    dataString.Append("<ccLicenseType>" + lf.CCLicenseType + "</ccLicenseType>");
                    dataString.Append("</ValidateLicense>");
                    dataString.Append("</soap:Body>");
                    dataString.Append("</soap:Envelope>");

                    byte[] byteArray = Encoding.UTF8.GetBytes(dataString.ToString());

                    request.ContentLength = byteArray.Length;
                    request.Method = "POST";
                    request.Headers.Add("SOAPAction", "\"http://tempuri.org/ValidateLicense\"");
                    request.ContentType = "text/xml;charset=\"utf-8\"";

                    try {
                        using (Stream dataStream = request.GetRequestStream()) {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                        }

                        var response = request.GetResponse() as HttpWebResponse;
                        StreamReader sr = new StreamReader(response.GetResponseStream());

                        try {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(sr.ReadToEnd());

                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                            nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");

                            response.Close();

                            var validationResult = xmlDoc.SelectSingleNode("/env:Envelope/env:Body/*/*[local-name()='ValidateLicenseResult']", nsmgr);
                            if (HelperMethods.ConvertBitToBoolean(validationResult.FirstChild.Value)) {
                                _licenseValid = true;
                                string siteName = ServerSettings.SiteName;
                                if (siteName != lf.WebsiteName) {
                                    ServerSettings.update_SiteName(lf.WebsiteName);
                                }

                                if (lf.LicenseId.Contains("-TRIAL")) {
                                    _trialActivated = true;
                                }
                            }
                            else {
                                _licenseValid = false;
                            }
                        }
                        catch {
                            _licenseValid = false;
                        }
                        finally {
                            response.Close();
                        }
                    }
                    catch {
                        _licenseValid = false;
                    }
                }
            }
        }
        catch {
            _licenseValid = false;
        }

        if (!_licenseValid) {
            try {
                string url = HttpContext.Current.Request.Url.AbsolutePath.Substring(HttpContext.Current.Request.Url.AbsolutePath.LastIndexOf('/') + 1);
                if ((HttpContext.Current.User.Identity.IsAuthenticated) && (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName))) {
                    if (url.ToLower() != "licensemanager.aspx") {
                        HttpContext.Current.Response.Redirect("~/SiteSettings/ServerMaintenance/LicenseManager.aspx");
                    }
                }
                else {
                    if (url.ToLower() != "default.aspx") {
                        HttpContext.Current.Response.Redirect("~/ErrorPages/LicenseInvalid.html");
                    }
                }
            }
            catch {
            }
        }
    }

    public static void ResetTrial() {
        _isExpired = false;
        _isTrial = false;
        _trialActivated = false;
    }

    public static bool GetTrialLicense(string url, string websiteName, string email, out string errorMessage) {
        try {
            errorMessage = string.Empty;
            string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
            if (File.Exists(_solFile)) {
                LicenseFile lf = GetLicenseObject(_solFile);
                if (lf.LicenseId != TrialKey || string.IsNullOrEmpty(lf.Host)) {
                    return false;
                }

                string dur = CalculateExpireLength(lf.DateIssued, lf.ExpirationDate);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lf.Host);
                request.KeepAlive = false;

                StringBuilder dataString = new StringBuilder();

                dataString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                dataString.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                dataString.Append("<soap:Body>");
                dataString.Append("<IssueTrialLicense xmlns=\"http://tempuri.org/\">");
                dataString.Append("<websiteUrl>" + url + "</websiteUrl>");
                dataString.Append("<siteName>" + websiteName + "</siteName>");
                dataString.Append("<emailAddress>" + email + "</emailAddress>");
                dataString.Append("<dur>" + dur + "</dur>");
                dataString.Append("</IssueTrialLicense>");
                dataString.Append("</soap:Body>");
                dataString.Append("</soap:Envelope>");

                byte[] byteArray = Encoding.UTF8.GetBytes(dataString.ToString());

                request.ContentLength = byteArray.Length;
                request.Method = "POST";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/IssueTrialLicense\"");
                request.ContentType = "text/xml;charset=\"utf-8\"";

                using (Stream dataStream = request.GetRequestStream()) {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                var response = request.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(response.GetResponseStream());

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                response.Close();

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");

                var validationResult = xmlDoc.SelectSingleNode("/env:Envelope/env:Body/*/*[local-name()='IssueTrialLicenseResult']", nsmgr);
                if (!string.IsNullOrEmpty(validationResult.FirstChild.Value)) {
                    string[] vals = validationResult.FirstChild.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    if (vals.Length == 5) {
                        LicenseUpload lu = new LicenseUpload();
                        if (HelperMethods.ConvertBitToBoolean(lu.UploadLicense(url, vals[0], websiteName, email, vals[3], vals[2], vals[1], "Issue", vals[4]))) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        errorMessage = "License has already been issued or an unkown error occured.";
                        return false;
                    }
                }
                else if (validationResult.FirstChild.Value == "issuedalready") {
                    errorMessage = "License has already been issued for this domain.";
                    return false;
                }
            }
        }
        catch {
        }


        errorMessage = "Your license could not be issued or has already been issued before.";
        return false;
    }

    public static bool CheckValidationCode(string valCode, out string errorMessage) {
        try {
            errorMessage = string.Empty;
            string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
            if (File.Exists(_solFile)) {
                LicenseFile lf = GetLicenseObject(_solFile);
                if (string.IsNullOrEmpty(lf.Host)) {
                    return false;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lf.Host);
                request.KeepAlive = false;

                StringBuilder dataString = new StringBuilder();

                dataString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                dataString.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                dataString.Append("<soap:Body>");
                dataString.Append("<CheckValidationCode xmlns=\"http://tempuri.org/\">");
                dataString.Append("<valCode>" + valCode + "</valCode>");
                dataString.Append("</CheckValidationCode>");
                dataString.Append("</soap:Body>");
                dataString.Append("</soap:Envelope>");

                byte[] byteArray = Encoding.UTF8.GetBytes(dataString.ToString());

                request.ContentLength = byteArray.Length;
                request.Method = "POST";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/CheckValidationCode\"");
                request.ContentType = "text/xml;charset=\"utf-8\"";

                using (Stream dataStream = request.GetRequestStream()) {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                var response = request.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(response.GetResponseStream());

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                response.Close();

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");

                var validationResult = xmlDoc.SelectSingleNode("/env:Envelope/env:Body/*/*[local-name()='CheckValidationCodeResult']", nsmgr);
                if (HelperMethods.ConvertBitToBoolean(validationResult.FirstChild.Value)) {
                    return true;
                }
            }
        }
        catch {
        }


        errorMessage = "Your validation code could not be found or is invalid.";
        return false;
    }

    public static bool GetFullLicense(string valCode, string url, string websiteName, string email, out string errorMessage) {
        try {
            errorMessage = string.Empty;
            string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
            if (File.Exists(_solFile)) {
                LicenseFile lf = GetLicenseObject(_solFile);
                if (string.IsNullOrEmpty(lf.Host)) {
                    return false;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lf.Host);
                request.KeepAlive = false;

                StringBuilder dataString = new StringBuilder();

                dataString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                dataString.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                dataString.Append("<soap:Body>");
                dataString.Append("<IssueFullLicense xmlns=\"http://tempuri.org/\">");
                dataString.Append("<valCode>" + valCode + "</valCode>");
                dataString.Append("<websiteUrl>" + url + "</websiteUrl>");
                dataString.Append("<siteName>" + websiteName + "</siteName>");
                dataString.Append("<emailAddress>" + email + "</emailAddress>");
                dataString.Append("</IssueFullLicense>");
                dataString.Append("</soap:Body>");
                dataString.Append("</soap:Envelope>");

                byte[] byteArray = Encoding.UTF8.GetBytes(dataString.ToString());

                request.ContentLength = byteArray.Length;
                request.Method = "POST";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/IssueFullLicense\"");
                request.ContentType = "text/xml;charset=\"utf-8\"";

                using (Stream dataStream = request.GetRequestStream()) {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                var response = request.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(response.GetResponseStream());

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                response.Close();

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");

                var validationResult = xmlDoc.SelectSingleNode("/env:Envelope/env:Body/*/*[local-name()='IssueFullLicenseResult']", nsmgr);
                if (!string.IsNullOrEmpty(validationResult.FirstChild.Value)) {
                    string[] vals = validationResult.FirstChild.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    if (vals.Length == 5) {
                        LicenseUpload lu = new LicenseUpload();
                        if (HelperMethods.ConvertBitToBoolean(lu.UploadLicense(url, vals[0], websiteName, email, vals[3], vals[2], vals[1], "Issue", vals[4]))) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }
                else if (validationResult.FirstChild.Value == "issuedalready") {
                    errorMessage = "License has already been issued for this domain.";
                    return false;
                }
            }
        }
        catch {
        }


        errorMessage = "Your license could not be issued or has already been issued before.";
        return false;
    }

    private static string GetSentFromUrl() {
        string orgUrl = HttpContext.Current.Request.Url.OriginalString;
        if (orgUrl.IndexOf(HttpContext.Current.Request.Url.AbsolutePath) != -1) {
            orgUrl = orgUrl.Remove(orgUrl.IndexOf(HttpContext.Current.Request.Url.AbsolutePath));
        }

        return orgUrl;
    }


    #region Get Creative Commons License

    public static string GetLicenseTermLinks(string licenseTerms = "") {
        if (string.IsNullOrEmpty(licenseTerms)) {
            try {
                string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
                if (!File.Exists(_solFile)) {
                    HttpContext.Current.Response.Redirect("~/ErrorPages/LicenseInvalid.html");
                }

                LicenseFile lf = GetLicenseObject(_solFile);
                if (lf != null) {
                    licenseTerms = lf.CCLicenseType;
                }
            }
            catch { }
        }

        switch (licenseTerms.ToLower()) {
            default:
            case "cc_by_nd":
                return CC_BY_ND;

            case "cc_by":
                return CC_BY;
                break;

            case "cc_by_sa":
                return CC_BY_SA;
                break;
        }
    }
    private static string CC_BY_ND {
        get {
            StringBuilder str = new StringBuilder();
            str.Append("<a rel='license' href='http://creativecommons.org/licenses/by-nd/4.0/'><img alt='Creative Commons License' style='border-width: 0' src='https://i.creativecommons.org/l/by-nd/4.0/88x31.png' /></a>");
            str.Append("<div style='clear: both; height: 10px;'></div>");
            str.Append("<span xmlns:dct='http://purl.org/dc/terms/' property='dct:title'>OpenWSE</span> by <a xmlns:cc='http://creativecommons.org/ns#' href='http://openwse.com' property='cc:attributionName' rel='cc:attributionURL'>John Donnelly</a> ");
            str.Append("is licensed under a <a rel='license' href='http://creativecommons.org/licenses/by-nd/4.0/'>Creative Commons Attribution-NoDerivatives 4.0 International License</a>.");
            return str.ToString();
        }
    }
    private static string CC_BY {
        get {
            StringBuilder str = new StringBuilder();
            str.Append("<a rel='license' href='http://creativecommons.org/licenses/by/4.0/'><img alt='Creative Commons License' style='border-width:0' src='https://i.creativecommons.org/l/by/4.0/88x31.png' /></a>");
            str.Append("<div style='clear: both; height: 10px;'></div>");
            str.Append("<span xmlns:dct='http://purl.org/dc/terms/' property='dct:title'>OpenWSE</span> by <a xmlns:cc='http://creativecommons.org/ns#' href='http://openwse.com' property='cc:attributionName' rel='cc:attributionURL'>John Donnelly</a> ");
            str.Append("is licensed under a <a rel='license' href='http://creativecommons.org/licenses/by/4.0/'>Creative Commons Attribution 4.0 International License</a>.");
            return str.ToString();
        }
    }
    private static string CC_BY_SA {
        get {
            StringBuilder str = new StringBuilder();
            str.Append("<a rel='license' href='http://creativecommons.org/licenses/by-sa/4.0/'><img alt='Creative Commons License' style='border-width:0' src='https://i.creativecommons.org/l/by-sa/4.0/88x31.png' /></a>");
            str.Append("<div style='clear: both; height: 10px;'></div>");
            str.Append("<span xmlns:dct='http://purl.org/dc/terms/' property='dct:title'>OpenWSE</span> by <a xmlns:cc='http://creativecommons.org/ns#' href='http://openwse.com' property='cc:attributionName' rel='cc:attributionURL'>John Donnelly</a> ");
            str.Append("is licensed under a <a rel='license' href='http://creativecommons.org/licenses/by-sa/4.0/'>Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            return str.ToString();
        }
    }

    #endregion


    public static bool LicenseValid {
        get {
            return _licenseValid;
        }
        set {
            _licenseValid = value;
        }
    }
    public static bool IsExpired {
        get {
            return _isExpired;
        }
    }
    public static bool IsTrial {
        get {
            return _isTrial;
        }
    }
    public static bool TrialActivated {
        get {
            return _trialActivated;
        }
    }
    public static string DaysLeftBeforeExpired {
        get {
            return _daysLeftBeforeExpired;
        }
    }

    public static LicenseFile LicenseFile {
        get {
            string _solFile = ServerSettings.GetServerMapLocation + "App_Data\\" + LicenseFileName;
            return GetLicenseObject(_solFile);
        }
    }

    private static LicenseFile GetLicenseObject(string solFile) {
        LicenseFile lf;
        BinaryDeserialize(solFile, out lf);
        return lf;
    }

    private static void BinaryDeserialize(string fileLoc, out LicenseFile lf) {
        lf = new LicenseFile();
        try {
            if (File.Exists(fileLoc)) {
                var d = new DataEncryption();
                MemoryStream a = d.DecryptFile(fileLoc, ServerBackup.EncryptPassword);
                using (var str = new BinaryReader(a)) {
                    var bf = new BinaryFormatter();
                    str.BaseStream.Position = 0;
                    bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                    lf = (LicenseFile)bf.Deserialize(str.BaseStream);
                    a.Close();
                    str.Close();
                }
            }
        }
        catch {
        }
    }

    private static string MemoryStreamToString(MemoryStream ms, Encoding enc) {
        return enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }

    private static string CalculateExpireLength(string startDate, string expireDate) {
        DateTime sDate = new DateTime();
        DateTime eDate = new DateTime();
        if (DateTime.TryParse(startDate, out sDate) && DateTime.TryParse(expireDate, out eDate)) {
            TimeSpan ts = eDate.Subtract(sDate);
            if (ts.TotalSeconds > 0) {
                return Math.Round(ts.TotalDays).ToString();
            }
        }

        return _daysLeftBeforeExpired;
    }

    private static void GetDaysLeftBeforeExpired(TimeSpan ts) {
        string pluralTime = string.Empty;
        if (Math.Round(ts.TotalHours) == 0) {
            _daysLeftBeforeExpired = Math.Round(ts.TotalMinutes).ToString();
            pluralTime = "minute";
            if (Math.Round(ts.TotalMinutes) != 1) {
                pluralTime = "minutes";
            }
        }
        else if (Math.Round(ts.TotalDays) == 0) {
            _daysLeftBeforeExpired = Math.Round(ts.TotalHours).ToString();
            pluralTime = "hour";
            if (Math.Round(ts.TotalHours) != 1) {
                pluralTime = "hours";
            }
        }
        else {
            _daysLeftBeforeExpired = Math.Round(ts.TotalDays).ToString();
            pluralTime = "day";
            if (Math.Round(ts.TotalDays) != 1) {
                pluralTime = "days";
            }
        }

        _daysLeftBeforeExpired += " " + pluralTime;
    }

    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder {
        public override Type BindToType(string assemblyName, string typeName) {
            Type typeToDeserialize = null;

            String currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }
}