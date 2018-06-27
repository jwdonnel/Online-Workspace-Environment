<%@ WebService Language="C#" Class="TestServices" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Web.Script.Serialization;


[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class TestServices  : System.Web.Services.WebService {

    [WebMethod]
    public string GetUniversalTime() {
        return ServerSettings.ServerDateTime.ToUniversalTime().ToString();
    }

    [WebMethod]
    public string GetLocalTime() {
        return ServerSettings.ServerDateTime.ToLocalTime().ToString();
    }

    [WebMethod]
    public string GetUrlEncoded(string url) {
        return HttpUtility.UrlEncode(url);
    }

    [WebMethod]
    public string GetUrlDecoded(string url) {
        return HttpUtility.UrlDecode(url);
    }
    
}