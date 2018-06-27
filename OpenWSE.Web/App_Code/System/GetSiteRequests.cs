#region

using System;
using System.Collections.Generic;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for GetSiteRequests
/// </summary>
public static class GetSiteRequests {

    private static List<DateTime> _siteRequests = new List<DateTime>();

    private static int _maxSiteRequestSize = -1;
    private static int MaxSiteRequestSize {
        get {
            if (_maxSiteRequestSize == -1) {
                try {
                    ServerSettings ss = new ServerSettings();
                    _maxSiteRequestSize = ss.MaxRequestRecordSize;
                }
                catch { }
            }
            return _maxSiteRequestSize;
        }
    }

    private static object _lockObj = new object();

    private static string _enableSiteRequestTracking = string.Empty;
    private static bool EnableSiteRequestTracking {
        get {
            if (string.IsNullOrEmpty(_enableSiteRequestTracking)) {
                try {
                    ServerSettings ss = new ServerSettings();
                    _enableSiteRequestTracking = ss.RecordSiteRequests.ToString().ToLower();
                }
                catch { }
            }

            return _enableSiteRequestTracking == "true";
        }
    }

    public static List<DateTime> SiteRequests {
        get { return _siteRequests; }
    }

    public static void SetEnableSiteRequestTracking(bool record) {
        _enableSiteRequestTracking = record.ToString().ToLower();
    }

    public static void SetMaxSiteRequestSize(int maxVal) {
        _maxSiteRequestSize = maxVal;
    }

    public static void AddRequest() {
        lock (_lockObj) {
            if (EnableSiteRequestTracking) {
                try {
                    ShrinkRequests();

                    DateTime now = ServerSettings.ServerDateTime;
                    if (!_siteRequests.Contains(now)) {
                        _siteRequests.Add(now);
                    }
                }
                catch {
                    ClearRequests();
                }
            }
            else {
                if (_siteRequests == null || (_siteRequests != null && _siteRequests.Count > 0)) {
                    _siteRequests = new List<DateTime>();
                }
            }
        }
    }

    private static void ShrinkRequests() {
        try {
            if (_siteRequests.Count > MaxSiteRequestSize) {
                _siteRequests.RemoveRange(0, MaxSiteRequestSize - 1);
            }
        }
        catch { }
    }

    public static void ClearRequests() {
        try {
            _siteRequests.Clear();
        }
        catch { }
    }

}