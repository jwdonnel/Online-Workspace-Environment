using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class LicenseFile {
    internal string _host;
    internal string _websiteUrl;
    internal string _websiteName;
    internal string _emailAddress;
    internal string _expireDate;
    internal string _dateIssued;
    internal string _licenseId;
    internal string _ccLicenseType;

    public LicenseFile() {
        this._host = string.Empty;
        this._websiteUrl = string.Empty;
        this._websiteName = string.Empty;
        this._emailAddress = string.Empty;
        this._expireDate = string.Empty;
        this._dateIssued = string.Empty;
        this._licenseId = string.Empty;
        this._ccLicenseType = string.Empty;
    }

    public LicenseFile(string host, string websiteUrl, string websiteName, string emailAddress, string expireDate, string dateIssued, string licenseId, string ccLicenseType) {
        this._host = host;
        this._websiteUrl = websiteUrl;
        this._websiteName = websiteName;
        this._emailAddress = emailAddress;
        this._expireDate = expireDate;
        this._dateIssued = dateIssued;
        this._licenseId = licenseId;
        this._ccLicenseType = ccLicenseType;
    }

    public string Host {
        get { return this._host; }
        set { this._host = value; }
    }

    public string WebsiteUrl {
        get { return this._websiteUrl; }
        set { this._websiteUrl = value; }
    }

    public string WebsiteName {
        get { return this._websiteName; }
        set { this._websiteName = value; }
    }

    public string EmailAddress {
        get { return this._emailAddress; }
        set { this._emailAddress = value; }
    }

    public string ExpirationDate {
        get { return this._expireDate; }
        set { this._expireDate = value; }
    }

    public string DateIssued {
        get { return this._dateIssued; }
        set { this._dateIssued = value; }
    }

    public string LicenseId {
        get { return this._licenseId; }
        set { this._licenseId = value; }
    }

    /// <summary>
    /// The Creative Common License Terms
    /// </summary>
    public string CCLicenseType {
        get { return this._ccLicenseType; }
        set { this._ccLicenseType = value; }
    }
}