<%@ WebService Language="C#" Class="GetPLT" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class GetPLT  : System.Web.Services.WebService 
{
    private MemberDatabase _member;
    private plt _prodLeadTime;
    
    [WebMethod]
    public string GetTimes()
    {
        System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
        _member = new MemberDatabase(userId.Name);
        int totalweight = 0;
        var str = new System.Text.StringBuilder();

        string line1 = string.Empty;
        string line2 = string.Empty;
        string line3 = string.Empty;
        string line4 = string.Empty;
        string line5 = string.Empty;
        _prodLeadTime = new plt(true);
        if (_prodLeadTime.pltcoll.Count > 0)
        {
            _prodLeadTime.pltcoll.Sort(
                (x, y) => Convert.ToDateTime(y.DateUpdated).Ticks.CompareTo(Convert.ToDateTime(x.DateUpdated).Ticks));
            System.Collections.Generic.List<string> dates = _prodLeadTime.CalculatePLT();
            const int i = 0;
            DateTime dateupdate;
            DateTime.TryParse(_prodLeadTime.pltcoll[i].DateUpdated, out dateupdate);
            if ((dates != null) && (dates.Count > 0))
            {
                line1 = _prodLeadTime.GetOverride("Override1")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1, dates[0]);


                line2 = _prodLeadTime.GetOverride("Override2")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2, dates[1]);


                line3 = _prodLeadTime.GetOverride("Override3")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear, dates[2]);


                line4 = _prodLeadTime.GetOverride("Override4")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear, dates[3]);


                line5 = _prodLeadTime.GetOverride("Override5")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable, dates[4]);
            }
        }
        else
        {
            line1 = "N/A";
            line2 = "N/A";
            line3 = "N/A";
            line4 = "N/A";
            line5 = "N/A";
        }

        str.Append("<b class='pad-right'>CTL 1(Light):</b>" + line1 + "<div class='clear-space'></div>");
        str.Append("<b class='pad-right'>CTL 2(Heavy):</b>" + line2 + "<div class='clear-space'></div>");
        str.Append("<b class='pad-right'>1/4\" Shear:</b>" + line3 + "<div class='clear-space'></div>");
        str.Append("<b class='pad-right'>1/2\" Shear:</b>" + line4 + "<div class='clear-space'></div>");
        str.Append("<b class='pad-right'>Burn Table:</b>" + line5.Trim());
        
        return str.ToString();
    }

    [WebMethod]
    public object[] GetTimes_ForViewer()
    {
        System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
        _member = new MemberDatabase(userId.Name);
        int totalweight = 0;
        var str = new System.Text.StringBuilder();

        string line1 = string.Empty;
        string line2 = string.Empty;
        string line3 = string.Empty;
        string line4 = string.Empty;
        string line5 = string.Empty;
        string timeUpdated = string.Empty;
        string updatedBy = string.Empty;
        
        _prodLeadTime = new plt(true);
        if (_prodLeadTime.pltcoll.Count > 0)
        {
            _prodLeadTime.pltcoll.Sort(
                (x, y) => Convert.ToDateTime(y.DateUpdated).Ticks.CompareTo(Convert.ToDateTime(x.DateUpdated).Ticks));
            System.Collections.Generic.List<string> dates = _prodLeadTime.CalculatePLT();
            const int i = 0;
            DateTime dateupdate = new DateTime();
            DateTime.TryParse(_prodLeadTime.pltcoll[i].DateUpdated, out dateupdate);
            if ((dates != null) && (dates.Count > 0))
            {
                timeUpdated = dateupdate.ToString();
                updatedBy = _prodLeadTime.pltcoll[i].UpdatedBy;
                line1 = _prodLeadTime.GetOverride("Override1")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1, dates[0]);


                line2 = _prodLeadTime.GetOverride("Override2")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2, dates[1]);


                line3 = _prodLeadTime.GetOverride("Override3")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear, dates[2]);


                line4 = _prodLeadTime.GetOverride("Override4")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear, dates[3]);


                line5 = _prodLeadTime.GetOverride("Override5")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable, dates[4]);
            }
        }
        else
        {
            line1 = "N/A";
            line2 = "N/A";
            line3 = "N/A";
            line4 = "N/A";
            line5 = "N/A";
            timeUpdated = "N/A";
            updatedBy = "N/A";
        }

        object[] obj = new object[7];
        obj[0] = line1.Trim();
        obj[1] = line2.Trim();
        obj[2] = line3.Trim();
        obj[3] = line4.Trim();
        obj[4] = line5.Trim();
        obj[5] = timeUpdated.Trim();
        MemberDatabase m = new MemberDatabase(updatedBy.Trim());
        obj[6] = HelperMethods.MergeFMLNames(m);

        return obj;
    }

    private static string Checkifoutofservice(string a)
    {
        return a.ToLower() == "out of service" ? "Out of Service" : a;
    }

    private static string Checkifoutofservice(string a, string b)
    {
        return a.ToLower() == "out of service" ? "Out of Service" : b;
    }
    
}