<%@ WebService Language="C#" Class="UpdateGeneralDirections" %>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Text;
using System.Web.Services.Protocols;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for UpdateGeneralDirections
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class UpdateGeneralDirections : System.Web.Services.WebService {

    private string result = "";
    private string absolutePath = string.Empty;
    private GeneralDirection gd;

    public UpdateGeneralDirections() {
        string[] delim = { "Handler", "handler" };
        absolutePath = HttpContext.Current.Request.Url.AbsoluteUri.Split(delim, System.StringSplitOptions.RemoveEmptyEntries)[0];
        gd = new GeneralDirection(false);
    }

    [WebMethod]
    public string BuildEditor() {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            result = buildTable();
            return result;
        }
        return string.Empty;
    }

    [WebMethod]
    public string AddDirection(string gendir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            gendir = HttpUtility.UrlDecode(gendir);
            gd = new GeneralDirection(true);
            foreach (Dictionary<string, string> row in gd.generaldirection) {
                if (gendir.ToLower() == row["GeneralDirection"].ToLower()) {
                    result = "false";
                    break;
                }
            }
            if ((gendir.ToLower() == "new general direction") || (string.IsNullOrEmpty(gendir)))
                result = "blank";

            if ((result == "") && (result != "blank")) {
                gd.addItem(UppercaseFirst(gendir));
                result = buildTable();
            }

            return result;
        }
        return string.Empty;
    }

    [WebMethod]
    public string DeleteDirection(string id, string gendir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            gendir = HttpUtility.UrlDecode(gendir);
            if (gd.deleteDirection(id)) {
                RebuildTruckSchedule(gendir);
                result = buildTable();
            }
            else
                result = "false";

            return result;
        }
        return string.Empty;
    }

    [WebMethod]
    public string UpdateDirection(string id, string gendir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            gendir = HttpUtility.UrlDecode(gendir);
            gd = new GeneralDirection(true);
            foreach (Dictionary<string, string> row in gd.generaldirection) {
                if (gendir.ToLower() == row["GeneralDirection"].ToLower()) {
                    result = "false";
                    break;
                }
            }
            if ((gendir.ToLower() == "new general direction") || (string.IsNullOrEmpty(gendir)))
                result = "blank";

            if ((result == "") && (result != "blank")) {
                gd.updateDirection(gendir, id);
                result = buildTable();
            }

            return result;
        }
        return string.Empty;
    }

    [WebMethod]
    public string EditDirection(string gendir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            gendir = HttpUtility.UrlDecode(gendir);
            result = buildEditableTable(gendir);
            return result;
        }
        return string.Empty;
    }

    private static string UppercaseFirst(string s) {
        // Check for empty string.
        if (string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    private string buildTable() {
        var d = new List<string>();
        var str = new StringBuilder();
        gd = new GeneralDirection(true);

        // Build Header
        str.Append("<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">");
        str.Append("<tbody><tr class=\"myHeaderStyle\"><td>Direction</td><td style='width: 75px;' align='center'></td></tr>");

        str.Append("<tr class=\"GridNormalRow myItemStyle\">");
        str.Append("<td class='border-right border-bottom'><input id='tb_addgendir' maxlength='150' type='text' class='textEntry MarginRightSml' style='width: 95%;' /></td>");
        str.Append("<td class='border-bottom' align='center'><a href='#' onclick='steelTruckFunctions.addgd();return false;' class='td-add-btn' title='Add'></a></td>");

        // Build Gerneral Direction Entries
        foreach (Dictionary<string, string> row in gd.generaldirection) {
            str.Append("<tr class=\"GridNormalRow myItemStyle\">");
            str.Append("<td class='border-right border-bottom'>" + row["GeneralDirection"].ToString() + "</td>");
            str.Append("<td class='border-bottom myItemStyle-action-btns' align='center'>");
            str.Append("<a href='#' onclick=\"steelTruckFunctions.editgd('" + row["GeneralDirection"] + "');return false;\" class='td-edit-btn margin-right' title='Edit'></a>");
            str.Append("<a href='#' onclick=\"steelTruckFunctions.deletegd('" + row["ID"].ToString() + "','" + row["GeneralDirection"].ToString() + "');return false;\" class='td-delete-btn' title='Delete'></a></td></tr>");
        }
        str.Append("</tbody></table>");

        return str.ToString();
    }

    private string buildEditableTable(string direction) {
        var d = new List<string>();
        var str = new StringBuilder();
        gd = new GeneralDirection(true);

        // Build Header
        str.Append("<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">");
        str.Append("<tbody><tr class=\"myHeaderStyle\"><td>Direction</td><td style='width: 75px;' align='center'></td></tr>");

        // Build Gerneral Direction Entries
        foreach (Dictionary<string, string> row in gd.generaldirection) {
            if (row["GeneralDirection"].ToString().ToLower() != direction.ToLower()) {
                str.Append("<tr class=\"GridNormalRow myItemStyle\">");
                str.Append("<td class='border-right border-bottom'>" + row["GeneralDirection"].ToString() + "</td>");
                str.Append("<td class='myItemStyle-action-btns border-bottom' align='center'></td></tr>");
            }
            else {
                str.Append("<tr class=\"GridNormalRow myItemStyle\">");
                str.Append("<td class='border-right border-bottom'><input id='tb_editgendir' maxlength='150' type='text' value='" + row["GeneralDirection"].ToString() + "' class='textEntry margin-right-sml' style='width: 95%;' onkeypress='steelTruckFunctions.OnKeyPress_DirEditNew(event,\"" + row["ID"].ToString() + "\")' /></td>");
                str.Append("<td class='myItemStyle-action-btns border-bottom' align='center'><a href='#' onclick=\"steelTruckFunctions.updategd('" + row["ID"].ToString() + "');return false;\" class='td-update-btn margin-right' title='Update'></a><a href='#' onclick='steelTruckFunctions.canceleditgd();return false;' class='td-cancel-btn' title='Cancel'></a></td></tr>");
            }
        }
        str.Append("</tbody></table>");

        return str.ToString();
    }

    private void RebuildTruckSchedule(string direction) {
        var truckschedule = new TruckSchedule();
        foreach (var ts in truckschedule.scheduler_coll) {
            if (ts.GeneralDirection.ToLower() == direction.ToLower())
                truckschedule.updateGeneralDirection(ts.ID, "Kansas");
        }
    }
}