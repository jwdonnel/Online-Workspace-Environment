<%@ WebService Language="C#" Class="StockViewerService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class StockViewerService  : System.Web.Services.WebService {

    [WebMethod]
    public object[] GetTickers() {
        string username = GetCurrentUsername();
        if (!string.IsNullOrEmpty(username)) {
            StockViewer sv = new StockViewer(username);
            return sv.GetTickers();    
        }
        
        return new object[0];
    }

    [WebMethod]
    public void SaveTicker(string id, string tickerJson) {
        string username = GetCurrentUsername();
        if (!string.IsNullOrEmpty(username)) {
            StockViewer sv = new StockViewer(username);
            sv.AddTicker(id, tickerJson);
        }
    }

    [WebMethod]
    public string UpdateTicker(string id, string tickerJson) {
        string username = GetCurrentUsername();
        if (!string.IsNullOrEmpty(username)) {
            StockViewer sv = new StockViewer(username);
            sv.UpdateTicker(id, tickerJson);
            return tickerJson;
        }

        return string.Empty;
    }

    [WebMethod]
    public void DeleteTicker(string id) {
        string username = GetCurrentUsername();
        if (!string.IsNullOrEmpty(username)) {
            StockViewer sv = new StockViewer(username);
            sv.DeleteTicker(id);
        }
    }

    private string GetCurrentUsername() {
        string _username = string.Empty;
        
        if (HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated) {
            _username = HttpContext.Current.User.Identity.Name;
            if (OpenWSE_Tools.Apps.AppInitializer.IsGroupAdminSession(_username)) {
                _username = GroupSessions.GetUserGroupSessionName(_username);
            }
            else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                _username = GroupSessions.GetUserGroupSessionName(_username);
            }
        }

        return _username;
    }
    
}