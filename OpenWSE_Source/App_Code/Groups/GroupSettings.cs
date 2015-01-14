using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for GroupSettings
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class GroupSettings : System.Web.Services.WebService {

    [WebMethod]
    public List<string> CheckIfGroupNameExists() {
        List<string> temp = new List<string>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            Groups tempGroups = new Groups();
            tempGroups.getEntries();
            foreach (Dictionary<string, string> dr in tempGroups.group_dt) {
                string gn = dr["GroupName"].Trim().ToLower();
                if (!temp.Contains(gn)) {
                    temp.Add(gn);
                }
            }
        }
        return temp;
    }
    
}
