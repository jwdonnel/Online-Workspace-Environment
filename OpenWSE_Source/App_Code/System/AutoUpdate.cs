using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Principal;
using System.Threading;
using OpenWSE_Tools.AutoUpdates;

/// <summary>
/// SO Source Auto Update System loops through the database to find changes
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AutoUpdate : System.Web.Services.WebService {

    #region Private Variables
    private ServerSettings _ss = new ServerSettings();
    private UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly object _sync = new object();
    private const int _loops = 15;
    private string _result;
    private bool _remoteLoad = false;
    private const int _timeOut = 1000;
    #endregion


    public AutoUpdate() { }

    [WebMethod]
    public object[] StartAUSystem(string _appId) {
        object[] obj = new object[2];

        if (!_ss.AutoUpdates) {
            obj[0] = "TURNOFF";
            return obj;
        }

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _appId = HttpUtility.UrlDecode(_appId);
            string username = HttpContext.Current.User.Identity.Name;
            _result = "false";
            SiteUpdater();
            for (var i = 0; i < _loops; i++) {
                if (_result == "false") {
                    Thread.Sleep(_timeOut);
                    _result = CheckData(username, _appId);
                }
                else {
                    break;
                }
            }
        }
        else {
            _result = "TURNOFF";
        }

        obj[0] = _result;
        obj[1] = _remoteLoad.ToString().ToLower();

        return obj;
    }


    #region Async Updater

    private SiteUpdaterDelegate _su;

    public void SiteUpdater() {
        _su = _SiteUpdater;
    }
    private string _SiteUpdater(string username, string appid) {
        var result = "false";
        if (appid.Contains(',')) {
            char[] delim = { ',' };
            string[] app_Split = appid.Split(delim);
            foreach (string w in app_Split) {
                var id = _uuf.getFlag(username, w);
                if (!string.IsNullOrEmpty(id)) {
                    result = id;
                    if (w == "appremote")
                        _remoteLoad = true;
                    break;
                }
            }
        }
        else {
            var id = _uuf.getFlag(username, appid);
            if (!string.IsNullOrEmpty(id))
                result = id;
        }

        return result;
    }
    public string CheckData(string username, string appid) {
        var result = _su.BeginInvoke(username, appid, null, @_sync);
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        while (!result.IsCompleted) {
            Thread.Sleep(_timeOut);
        }

        try {
            return _su.EndInvoke(result);
        }
        catch {
            return "false";
        }
    }

    private delegate string SiteUpdaterDelegate(string username, string appid);

    #endregion

}
