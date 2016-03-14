#region

using System;
using System.Collections.Generic;

#endregion

/// <summary>
///     Summary description for GetSiteRequests
/// </summary>
public static class GetSiteRequests {
    private static List<DateTime> _siteRequests = new List<DateTime>();
    private const int maxSiteRequestSize = 4000;

    public static List<DateTime> SiteRequests {
        get { return _siteRequests; }
    }

    public static void AddRequest() {
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

    private static void ShrinkRequests() {
        try {
            if (_siteRequests.Count > maxSiteRequestSize) {
                _siteRequests.RemoveRange(0, maxSiteRequestSize - 1);
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