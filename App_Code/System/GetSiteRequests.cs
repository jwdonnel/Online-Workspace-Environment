#region

using System;
using System.Collections.Generic;

#endregion

/// <summary>
///     Summary description for GetSiteRequests
/// </summary>
public static class GetSiteRequests {
    private static List<DateTime> Siterequests = new List<DateTime>();
    private static int _hitCount = 0;
    private static string _currHitDate = "";
    private static bool _getrequests = true;

    public static List<DateTime> SiteRequests {
        get { return Siterequests; }
    }

    public static bool GetRequests {
        get { return _getrequests; }
    }

    public static void AddRequest() {
        if (_getrequests) {
            try {
                ShrinkRequests();

                DateTime now = ServerSettings.ServerDateTime;
                if (!Siterequests.Contains(now)) {
                    Siterequests.Add(now);
                }
            }
            catch {
                ClearRequests();
            }
        }
        else {
            ClearRequests();
        }
    }

    public static void AddHitCount() {
        string currDate = ServerSettings.ServerDateTime.ToShortDateString();
        if ((string.IsNullOrEmpty(_currHitDate)) || (_currHitDate == currDate)) {
            _hitCount++;
        }
        else {
            _hitCount = 0;
        }

        _currHitDate = currDate;
    }

    public static int HitCountTotal {
        get {
            return _hitCount;
        }
    }

    private static void ShrinkRequests() {
        try {
            if (Siterequests.Count > 400) {
                Siterequests.RemoveRange(0, 375);
            }
        }
        catch { }
    }

    public static void ClearRequests() {
        try {
            Siterequests.Clear();
        }
        catch { }

        _hitCount = 0;
    }

    public static void UpdateGetRequests(bool x) {
        _getrequests = x;
        if (!x) {
            ClearRequests();
        }
    }
}