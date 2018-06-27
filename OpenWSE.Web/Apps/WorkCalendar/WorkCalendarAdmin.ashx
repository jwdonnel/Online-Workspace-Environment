<%@ WebHandler Language="C#" Class="WorkCalendarAdmin" %>

#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using OpenWSE_Tools.GroupOrganizer;

#endregion

public sealed class WorkCalendarAdmin : IHttpHandler
{
    private HttpRequest Request;
    private HttpResponse Response;

    public void ProcessRequest(HttpContext context)
    {
        string result = "";
        Request = context.Request;
        Response = context.Response;
        Response.ContentType = "application/json";
        var vacatypes = new VacationTypes(false);
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            switch (Request.QueryString["action"]) {
                case "ptotypes":
                    result = buildTable(Request.QueryString["group"]);
                    break;
                case "addtype":
                    vacatypes = new VacationTypes(true);
                    string deduct_add = Request.QueryString["deduct"];
                    int d_add = 0;

                    if (HelperMethods.ConvertBitToBoolean(deduct_add)) {
                        d_add = 1;
                    }
                    if (vacatypes.vactypes_dt.Cast<Dictionary<string, string>>().Where(row => Request.QueryString["group"] == row["GroupName"].ToString()).Any(row => (Request.QueryString["type"].ToLower() == row["PTOType"].ToString().ToLower()) && (row["Deduct"].ToString() == d_add.ToString()))) {
                        result = "false";
                    }
                    if ((Request.QueryString["type"].ToLower() == "new reason") ||
                        (string.IsNullOrEmpty(Request.QueryString["type"]))) {
                        result = "blank";
                    }
                    if ((result == "") && (result != "blank")) {
                        vacatypes.addItem(UppercaseFirst(Request.QueryString["type"]), Request.QueryString["group"], d_add);
                        result = buildTable(Request.QueryString["group"]);
                    }
                    break;
                case "deletetype":
                    if (vacatypes.deleteItem(Request.QueryString["typeid"])) {
                        vacatypes.deleteItem(Request.QueryString["type"]);
                        result = buildTable(Request.QueryString["group"]);
                    }
                    else {
                        result = "false";
                    }
                    break;
                case "updatetype":
                    vacatypes = new VacationTypes(true);
                    string deduct = Request.QueryString["deduct"];
                    int d = 0;

                    if (HelperMethods.ConvertBitToBoolean(deduct)) {
                        d = 1;
                    }
                    if (vacatypes.vactypes_dt.Cast<Dictionary<string, string>>().Where(row => Request.QueryString["group"] == row["GroupName"].ToString()).Any(row => (Request.QueryString["type"].ToLower() == row["PTOType"].ToString().ToLower())
                                                                                                                                                        && (row["Deduct"].ToString() == d.ToString()))) {
                        result = "false";
                    }
                    if ((Request.QueryString["type"].ToLower() == "new reason") ||
                        (string.IsNullOrEmpty(Request.QueryString["type"]))) {
                        result = "blank";
                    }
                    if ((result == "") && (result != "blank")) {
                        vacatypes.updateItem(Request.QueryString["type"], Request.QueryString["typeid"], d);
                        result = buildTable(Request.QueryString["group"]);
                    }
                    break;
                case "edittype":
                    result = buildEditableTable(Request.QueryString["type"], Request.QueryString["group"]);
                    break;
                case "ptohours":
                    result = buildTable_Userhours(Request.QueryString["group"]);
                    break;
                case "updateUser":
                    string hours = Request.QueryString["hours"];
                    double h;
                    int h2;
                    if ((string.IsNullOrEmpty(hours)) || (!double.TryParse(hours, out h)) || (!int.TryParse(hours, out h2))) {
                        result = "false";
                        break;
                    }
                    var uc = new MemberDatabase(Request.QueryString["user"]);
                    uc.UpdateVacationTime(hours);
                    break;
                default:
                    result = "done";
                    break;
            }
        }
        
        Response.Write(result);
        context.ApplicationInstance.CompleteRequest();
    }

    public bool IsReusable
    {
        get { return false; }
    }

    private static string UppercaseFirst(string s)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(s))
            return string.Empty;
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    private static string buildTable(string group)
    {
        var d = new List<string>();
        var str = new StringBuilder();
        var vacatypes = new VacationTypes(true);
        // Build Header
        str.Append("<table cellpadding='5' cellspacing='0' style='width: 100%;'><tbody><tr class='myHeaderStyle'><td>Reason</td><td style='width: 136px;'>Date Updated</td>");
        str.Append("<td style='width: 35px;'>Deduct</td><td style='width: 75px;' align='center'>Actions</td></tr>");
        // Build Gerneral Direction Entries
        foreach (Dictionary<string, string> row in vacatypes.vactypes_dt)
        {
            if (group == row["GroupName"].ToString())
            {
                string deduct = string.Empty;
                if (HelperMethods.ConvertBitToBoolean(row["Deduct"]))
                {
                    deduct = "X";
                }
                str.Append("<tr class='myItemStyle GridNormalRow'><td class='border-right border-bottom'>" + row["PTOType"] + "</td>");
                str.Append("<td class='border-right border-bottom'>" + row["DateAdded"] + "</td>");
                str.Append("<td class='border-right border-bottom' align='center'>" + deduct + "</td>");
                    str.Append("<td class='border-bottom' align='center'><a href='#' onclick=\"editType('" + row["PTOType"] + "');return false;\" class='td-edit-btn margin-right'></a>");
                str.Append("<a href='#' onclick=\"deleteType('" + row["ID"] + "','" + row["PTOType"] + "');return false;\" class='td-delete-btn'></a></td></tr>");
            }
        }
        // Build Add New entry and then close table
        str.Append("<tr class='myItemStyle GridNormalRow'><td class='border-right border-bottom' align='center'><input id='tb_addtype' maxlength='150' type='text' ");
        str.Append("value='New reason' class='textEntry MarginRightSml' style='width: 95%;' onfocus=\"if(this.value=='New reason')this.value=''\" ");
        str.Append("onblur=\"if(this.value=='')this.value='New reason'\" /></td><td class='border-right border-bottom' align='center'><div id='typeerror'><hr style='width: 80%;' /></div></td>");
        str.Append("<td class='border-right border-bottom' align='center'><input id='add-reason-checkbox' type='checkbox' /></td>");
        str.Append("<td class='border-bottom' align='center'><a href='#' onclick='addType();return false;' class='td-add-btn'></a></td></tr>");
        str.Append("</tbody></table>");

        return str.ToString();
    }

    private static string buildEditableTable(string reason, string group)
    {
        var d = new List<string>();
        var str = new StringBuilder();
        var vacatypes = new VacationTypes(true);
        // Build Header
        str.Append("<table cellpadding='5' cellspacing='0' style='width: 100%;'><tbody><tr class='myHeaderStyle'><td>Reason</td><td style='width: 136px;'>Date Updated</td>");
        str.Append("<td style='width: 35px;'>Deduct</td><td style='width: 75px;' align='center'>Actions</td></tr>");

        // Build Vacation Type Entries
        foreach (Dictionary<string, string> row in vacatypes.vactypes_dt.Cast<Dictionary<string, string>>().Where(row => @group == row["GroupName"].ToString()))
        {
            if (row["PTOType"].ToString().ToLower() != reason.ToLower())
            {
                string deduct = string.Empty;
                if (HelperMethods.ConvertBitToBoolean(row["Deduct"]))
                {
                    deduct = "X";
                }
                str.Append("<tr class='myItemStyle GridNormalRow'><td class='border-right border-bottom'>" + row["PTOType"] + "</td>");
                str.Append("<td class='border-right border-bottom'>" + row["DateAdded"] + "</td>");
                str.Append("<td class='border-right border-bottom' align='center' style='width: 40px;'>" + deduct + "</td>");
                str.Append("<td class='border-bottom' align='center'></td></tr>");
                str.Append("</td></tr>");
            }
            else
            {
                string deduct = string.Empty;
                if (HelperMethods.ConvertBitToBoolean(row["Deduct"]))
                {
                    deduct = " checked";
                }
                str.Append("<tr class='myItemStyle GridNormalRow'><td class='border-right border-bottom' align='center'><input id='tb_edittype' maxlength='150' type='text' ");
                str.Append("value='" + row["PTOType"] + "' class='textEntry margin-right-sml' style='width: 95%;' />");
                str.Append("</td><td class='border-right border-bottom' align='center'><div id='typeerror'>" + row["DateAdded"] + "</div></td>");
                str.Append("<td class='border-right border-bottom' align='center'><input id='edit-reason-checkbox' type='checkbox'" + deduct + " /></td>");
                str.Append("<td class='border-bottom' align='center'><a href='#' onclick=\"updateType('" + row["ID"] + "');return false;\" class='td-update-btn margin-right'></a><a href='#' onclick='canceleditType();return false;' class='td-cancel-btn'></a></td></tr>");
            }
        }
        str.Append("</tbody></table>");

        return str.ToString();
    }

    private static string buildTable_Userhours(string group)
    {
        var d = new List<string>();
        var str = new StringBuilder();
        MembershipUserCollection coll = Membership.GetAllUsers();
        var apps = new App(string.Empty);
        // Build Header
        str.Append("<table cellpadding='5' cellspacing='0' style='width: 100%;'><tbody><tr class='myHeaderStyle'><td>Name</td>");
        str.Append("<td style='width: 150px;'>Username</td><td style='width: 100px;' align='center'>Hours</td></tr>");

        // Build Gerneral Direction Entries
        int count = 0;
        foreach (MembershipUser u in coll)
        {
            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())
            {
                var g = new Groups(u.UserName);
                var mb = new MemberDatabase(u.UserName);
                if (mb.UserHasApp("app-workcalendar"))
                {
                    if (g.IsApartOfGroup(mb.GroupList, group))
                    {
                        string name = HelperMethods.MergeFMLNames(mb);
                        if (u.UserName.ToLower() == HttpContext.Current.User.Identity.Name.ToLower())
                            name = "You";

                        str.Append("<tr class='myItemStyle GridNormalRow'><td class='border-right border-bottom'>" + name + "</td>");
                        str.Append("<td class='border-right border-bottom'>" + u.UserName + "</td>");
                        str.Append("<td class='border-bottom' align='center'><input id='tb_edithours_" + u.UserName + "' maxlength='4' type='text' ");
                        str.Append("value='" + mb.VacationTime + "' class='textEntry MarginRightSml' style='width: 40px;' onfocus=\"if(this.value=='Hours')this.value=''\" ");
                        str.Append("onblur=\"if(this.value=='')this.value='Hours'\" /><a href='#' onclick=\"updateUser('" + u.UserName + "');return false;\" class='td-update-btn margin-left'></a></td></tr>");
                        count++;
                    }
                }
            }
        }
        str.Append("</tbody></table>");

        if (count == 0)
        {
            str.Append("No users available");
        }

        return str.ToString();
    }
}