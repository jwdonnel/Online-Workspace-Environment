<%@ WebService Language="C#" Class="WorkCalendar" %>

#region

using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Collections.Generic;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.GroupOrganizer;

#endregion

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class WorkCalendar : WebService
{
    private readonly CalendarEntries ce;
    private readonly IIdentity userID;
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly string _sitetheme;

    private const string leftarrow = "";
    private const string rightarrow = "";

    public WorkCalendar()
    {
        userID = HttpContext.Current.User.Identity;
        ce = new CalendarEntries(userID.Name);
        var member = new MemberDatabase(userID.Name);

        _sitetheme = member.SiteTheme;
    }

    [WebMethod]
    public string AddEvent(string employee, string title, string desc, string startdate, string enddate, string hours, string group)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                DateTime _start = Convert.ToDateTime(startdate);
                DateTime _end = Convert.ToDateTime(enddate);

                if (IsApprover(userID.Name))
                {
                    ce.addEntry(employee, _start.ToString(), _end.ToString(), hours, title, desc, group, "1");
                }
                else
                {
                    ce.addEntry(employee, _start.ToString(), _end.ToString(), hours, title, desc, group);
                }
                uuf.addFlag("workspace", group);
            }
            catch
            {
                return "false";
            }
        }
        return "true";
    }

    [WebMethod]
    public string UpdateEvent(string id, string employee, string title, string desc, string startdate, string enddate, string hours)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                if (IsApprover(userID.Name))
                {
                    if ((startdate[0] != '0') && (startdate[0] != '1'))
                        startdate = "0" + startdate;
                    if ((enddate[0] != '0') && (enddate[0] != '1'))
                        enddate = "0" + enddate;
                    
                    DateTime _start = new DateTime();
                    DateTime.TryParse(startdate, out _start);
                    DateTime _end = new DateTime();
                    DateTime.TryParse(enddate, out _end);

                    ce.updateEntry(id, _start.ToString(), _end.ToString(), hours, title, desc, employee);
                    uuf.addFlag("workspace", "");
                }
            }
            catch
            {
                return "false";
            }
        }
        return "true";
    }

    [WebMethod]
    public string GetEventInfo(string id)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) {
                    return string.Empty;
                }
                
                var str = new StringBuilder();
                ce.getEntry(id);
                if (ce.ce_dt.Count == 1)
                {
                    Dictionary<string, string> dr = ce.ce_dt[0];
                    var m = new MemberDatabase(dr["Username"].ToString());

                    string un = HelperMethods.MergeFMLNames(m);
                    if (string.IsNullOrEmpty(un))
                        un = m.Username;

                    string color = m.UserColor;
                    if (string.IsNullOrEmpty(color))
                        color = "#EFEFEF";
                    
                    string reason = un + " - " + dr["Reason"].ToString();
                    if ((string.IsNullOrEmpty(dr["Reason"].ToString())) || (dr["Reason"].ToString() == "null")) {
                        reason = un;
                    }

                    string usercolor = UserImageColorCreator.CreateImgColor(m.AccountImage, color, m.UserId, 35, m.SiteTheme);

                    str.Append("<div class='float-left pad-right'><img alt='' src='Apps/WorkCalendar/plan.png' style='height: 35px;' /></div>");
                    str.Append("<div class='float-left margin-top-sml'>" + usercolor + "<div class='float-left pad-left' style='margin-top: -2px;'><h3><b>" + reason + "</b></h3><div class='clear'></div>");
                    str.Append("<small><b class='pad-right-sml'>Updated:</b>" + dr["DateUpdated"].ToString() + "</small></div></div>");
                    str.Append("<div class='clear'></div>");
                    str.Append("<div class='clear-space'></div>");
                    str.Append("<b class='pad-right-sml'>Start Time:</b>" + dr["StartDate"].ToString() + "<div class='clear-space-two'></div>");
                    str.Append("<b class='pad-right-sml'>End Time:</b>" + dr["EndDate"].ToString() + "<div class='clear-space-two'></div>");
                    str.Append("<b class='pad-right-sml'>Hours Used:</b>" + dr["Hours"].ToString() + "<div class='clear-space-two'></div>");
                    str.Append("<div class='clear-space'></div><b>Description:</b><div class='clear'></div>");
                    string desc = dr["Description"].ToString();

                    if (string.IsNullOrEmpty(desc))
                        desc = "No description available";

                    str.Append(desc);
                    str.Append("<div class='clear-space'></div><div class='clear-space'></div><div class='clear-space'></div>");
                    str.Append("<input type='button' class='input-buttons' value='Close' onclick=\"ViewNewCalendarEvent_wc('');\" />");
                    DateTime targetDt = ServerSettings.ServerDateTime;
                    var date1 = new DateTime();
                    var date2 = new DateTime();
                    if ((DateTime.TryParse(dr["StartDate"].ToString(), out date1)) &&
                        (DateTime.TryParse(dr["EndDate"].ToString(), out date2)))
                    {
                        if ((!(targetDt.Ticks >= date1.Ticks && targetDt.Ticks <= date2.Ticks)) &&
                            (targetDt.Ticks <= date2.Ticks))
                        {
                            if (m.Username == userID.Name.ToLower())
                            {
                                str.Append("<input type='button' class='input-buttons float-right no-margin' value='Cancel' onclick=\"CancelEventInfo_wc('" + dr["ID"] + "');\" />");
                                if (IsApprover(userID.Name))
                                    str.Append("<input type='button' class='input-buttons float-right' value='Edit' onclick=\"EditCalendarEvent_wc('" + dr["Reason"] + "','" + dr["Username"] + "','" + dr["Description"] + "','" + dr["StartDate"] + "','" + dr["EndDate"] + "','" + dr["Hours"] + "');\" />");
                            }
                            else
                            {
                                var apps = new App(string.Empty);
                                var member = new MemberDatabase(userID.Name);
                                if (IsApprover(userID.Name))
                                {
                                    str.Append("<input type='button' class='input-buttons float-right no-margin' value='Cancel' onclick=\"CancelEventInfo_wc('" + dr["ID"] + "');\" />");
                                    str.Append("<input type='button' class='input-buttons float-right' value='Edit' onclick=\"EditCalendarEvent_wc('" + dr["Reason"] + "','" + dr["Username"] + "','" + dr["Description"] + "','" + dr["StartDate"] + "','" + dr["EndDate"] + "','" + dr["Hours"] + "');\" />");
                                }
                            }
                        }
                        else
                        {
                            if (m.Username == userID.Name.ToLower())
                            {
                                str.Append("<input type='button' class='input-buttons float-right no-margin' value='Delete' onclick=\"DeleteEventInfo_wc('" + dr["ID"] + "');\" />");
                                if (IsApprover(userID.Name))
                                    str.Append("<input type='button' class='input-buttons float-right' value='Edit' onclick=\"EditCalendarEvent_wc('" + dr["Reason"] + "','" + dr["Username"] + "','" + dr["Description"] + "','" + dr["StartDate"] + "','" + dr["EndDate"] + "','" + dr["Hours"] + "');\" />");
                            }
                            else
                            {
                                var apps = new App(string.Empty);
                                var member = new MemberDatabase(userID.Name);
                                if (IsApprover(userID.Name))
                                {
                                    str.Append("<input type='button' class='input-buttons float-right no-margin' value='Delete' onclick=\"DeleteEventInfo_wc('" + dr["ID"] + "');\" />");
                                    str.Append("<input type='button' class='input-buttons float-right' value='Edit' onclick=\"EditCalendarEvent_wc('" + dr["Reason"] + "','" + dr["Username"] + "','" + dr["Description"] + "','" + dr["StartDate"] + "','" + dr["EndDate"] + "','" + dr["Hours"] + "');\" />");
                                }
                            }
                        }
                    }
                    return str.ToString();
                }
            }
            catch
            {
                return "false";
            }
        }
        return "false";
    }

    [WebMethod]
    public string CancelEventInfo(string id, string groupname)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                ce.getEntry(id);
                if (ce.ce_dt.Count == 1)
                {
                    string u = ce.GetUsername(id);
                    ce.updateEntry(id, "3");

                    MembershipUserCollection coll = Membership.GetAllUsers();
                    var apps = new App(string.Empty);
                    foreach (MembershipUser _u in from MembershipUser _u in coll
                                                  where _u.IsOnline
                                                  let temp_member = new MemberDatabase(_u.UserName)
                                                  where (temp_member.UserHasApp("app-workcalendar")) && (_u.UserName.ToLower() != userID.Name.ToLower()) && (temp_member.GroupList.Contains(groupname)) select _u)
                    {
                        uuf.addFlag(_u.UserName, "workspace", groupname);
                    }
                    return "";
                }
            }
            catch
            {
                return "false";
            }
        }
        return "false";
    }

    [WebMethod]
    public string DeleteEventInfo(string id, string groupname)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                ce.getEntry(id);
                if (ce.ce_dt.Count == 1)
                {
                    string u = ce.GetUsername(id);
                    ce.deleteEntry(id, u);

                    MembershipUserCollection coll = Membership.GetAllUsers();
                    var apps = new App(string.Empty);
                    foreach (MembershipUser _u in from MembershipUser _u in coll
                                                  where _u.IsOnline
                                                  let temp_member = new MemberDatabase(_u.UserName)
                                                  where (temp_member.UserHasApp("app-workcalendar")) && (_u.UserName.ToLower() != userID.Name.ToLower()) && (temp_member.GroupList.Contains(groupname)) select _u)
                    {
                        uuf.addFlag(_u.UserName, "workspace", groupname);
                    }
                    return "";
                }
            }
            catch
            {
                return "false";
            }
        }
        return "false";
    }

    [WebMethod]
    public object[] GetMonthInfo(string month, string year, string group, string search)
    {
        List<object> returnObj = new List<object>();
        if (userID.IsAuthenticated)
        {
            try
            {
                int m = 1;
                int.TryParse(month, out m);
                m = m + 1;
                month = m.ToString(CultureInfo.InvariantCulture);

                ce.getEntries(group);
                string str = string.Empty;
                foreach (Dictionary<string, string> dr in ce.ce_dt)
                {
                    var startDate = new DateTime();
                    if (DateTime.TryParse(dr["StartDate"].ToString(), out startDate))
                    {
                        var endDate = new DateTime();
                        if (DateTime.TryParse(dr["EndDate"].ToString(), out endDate))
                        {
                            if ((dr["Approved"].ToString() != "1") && (dr["Approved"].ToString() != "4"))
                                continue;

                            if ((!string.IsNullOrEmpty(search)) && (!dr["Reason"].ToString().ToLower().Contains(search)) && (!dr["Username"].ToString().ToLower().Contains(search)) && (!dr["Description"].ToString().ToLower().Contains(search)))
                                continue;

                            var member = new MemberDatabase(dr["Username"].ToString());
                            string usercolor = member.UserColor;
                            string daterange = dr["StartDate"].ToString() + " - " + dr["EndDate"].ToString();

                            string rUser = HelperMethods.MergeFMLNames(member);
                            if (string.IsNullOrEmpty(rUser))
                            {
                                rUser = member.FirstName;
                                if (!string.IsNullOrEmpty(member.LastName))
                                    rUser += member.LastName[0] + ".";
                                if (string.IsNullOrEmpty(rUser))
                                    rUser = member.Username;
                            }
                            
                            string reason = rUser + " - " + dr["Reason"].ToString();
                            if ((string.IsNullOrEmpty(dr["Reason"].ToString())) || (dr["Reason"].ToString() == "null")) {
                                reason = rUser;
                            }
                            
                            if (((startDate.Month.ToString() == month) && (startDate.Year.ToString() == year)))
                            {
                                string fulldate = (startDate.Month - 1).ToString() + "_" + startDate.Day.ToString() + "_" + startDate.Year.ToString();
                                int dayofweek = (int)Convert.ToDateTime(startDate.Year + "-" + startDate.Month + "-" + startDate.Day).DayOfWeek;
                                int startday = startDate.Day;

                                int endMonthDay = endDate.Day;
                                if (endDate.Month != startDate.Month)
                                    endMonthDay = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                                int endday = endMonthDay;
                                int length = Math.Abs(startday - endday) + 1;
                                if (endday < startday)
                                    length = Math.Abs(DateTime.DaysInMonth(startDate.Year, startDate.Month) - startday);

                                int dim = Math.Abs(DateTime.DaysInMonth(startDate.Year, startDate.Month));
                                while (length < dim)
                                {
                                    if ((Math.Abs(7 - dayofweek) < length) && (Math.Abs(length + dayofweek) >= 7))
                                    {
                                        startday += Math.Abs(7 - dayofweek);
                                        AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, Math.Abs(7 - dayofweek), reason, dr["Description"].ToString(), daterange, ref returnObj);
                                        fulldate = ((startDate.Month - 1) + "_" + startday + "_" + startDate.Year);
                                        length = Math.Abs(startday - endday) + 1;
                                        if ((length + dayofweek) > 7)
                                            dayofweek = 0;
                                    }
                                    else
                                    {
                                        if (length <= 0)
                                            break;

                                        AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, length, reason, dr["Description"].ToString(), daterange, ref returnObj);
                                        break;
                                    }
                                }
                            }
                            else if ((endDate.Month.ToString() == month) && (endDate.Year.ToString() == year))
                            {
                                string fulldate = (endDate.Month - 1).ToString() + "_1_" + endDate.Year.ToString();
                                int dayofweek = (int)Convert.ToDateTime(endDate.Year + "-" + endDate.Month + "-1").DayOfWeek;
                                int startday = 1;
                                int endday = endDate.Day;
                                int length = Math.Abs(startday - endday) + 1;

                                if (endday < startday)
                                    length = Math.Abs(DateTime.DaysInMonth(endDate.Year, endDate.Month) - startday);

                                while (length < Math.Abs(DateTime.DaysInMonth(endDate.Year, endDate.Month)))
                                {
                                    if (((7 - dayofweek) < length) && ((length + dayofweek) >= 7))
                                    {
                                        startday += Math.Abs(7 - dayofweek);
                                        AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, Math.Abs(7 - dayofweek), reason, dr["Description"].ToString(), daterange, ref returnObj);
                                        fulldate = ((endDate.Month - 1) + "_" + startday + "_" + endDate.Year);
                                        length = Math.Abs(startday - endday) + 1;
                                        if ((length + dayofweek) > 7)
                                            dayofweek = 0;
                                    }
                                    else
                                    {
                                        if (length <= 0)
                                            break;

                                        AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, Math.Abs(7 - dayofweek), reason, dr["Description"].ToString(), daterange, ref returnObj);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                DateTime currSelected = Convert.ToDateTime(year + "-" + month + "-1");
                                if (InBetweenDaysInclusive(currSelected, startDate, endDate))
                                {
                                    string fulldate = (Convert.ToInt32(month) - 1) + "_1_" + year;
                                    int dayofweek = (int)currSelected.DayOfWeek;
                                    int startday = 1;

                                    int endMonthDay = endDate.Day;
                                    if (endDate.Month != startDate.Month)
                                        endMonthDay = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                                    int endday = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));
                                    int length = Math.Abs(startday - endday) + 1;
                                    if (endday < startday)
                                        length = Math.Abs(DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month)) - startday);

                                    int dim = endday;
                                    while (length <= dim)
                                    {
                                        if ((Math.Abs(7 - dayofweek) < length) && (Math.Abs(length + dayofweek) >= 7))
                                        {
                                            startday += Math.Abs(7 - dayofweek);
                                            AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, Math.Abs(7 - dayofweek), reason, dr["Description"].ToString(), daterange, ref returnObj);
                                            fulldate = ((Convert.ToInt32(month) - 1) + "_" + startday + "_" + year);
                                            length = Math.Abs(startday - endday) + 1;
                                            if ((length + dayofweek) > 7)
                                                dayofweek = 0;
                                        }
                                        else
                                        {
                                            if (length <= 0)
                                                break;

                                            AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, length, reason, dr["Description"].ToString(), daterange, ref returnObj);
                                            break;
                                        }


                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        return returnObj.ToArray();
    }

    [WebMethod]
    public object[] GetUserListForGroup(string group)
    {
        List<object> objReturn = new List<object>();
        if (IsApprover(userID.Name)) {
            List<System.Web.UI.WebControls.ListItem> items = new List<System.Web.UI.WebControls.ListItem>();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (var item in from MembershipUser u in coll let apps = new App(string.Empty) let member = new MemberDatabase(u.UserName) let g = new Groups(u.UserName) where member.UserHasApp("app-workcalendar") where g.IsApartOfGroup(member.GroupList, @group) select new System.Web.UI.WebControls.ListItem(HelperMethods.MergeFMLNames(member), u.UserName) into item where !items.Contains(item) select item) {
                if (!items.Contains(item))
                    items.Add(item);

                object[] obj = { item.Text, item.Value };
                objReturn.Add(obj);
            }
        }
        else {
            object[] obj = { HelperMethods.MergeFMLNames(new MemberDatabase(userID.Name)), userID.Name };
            objReturn.Add(obj);
        }

        return objReturn.ToArray();
    }

    [WebMethod]
    public object[] GetReasonsForGroup(string group)
    {
        List<object> objReturn = new List<object>();
        VacationTypes vacatypes = new VacationTypes(true);
        
        // Build Vacation Type Entries
        foreach (Dictionary<string, string> row in vacatypes.vactypes_dt.Cast<Dictionary<string, string>>().Where(row => group == row["GroupName"].ToString()))
        {
            if (!objReturn.Contains(row["PTOType"].ToString()))
                objReturn.Add(row["PTOType"].ToString());
        }

        return objReturn.ToArray();
    }

    [WebMethod]
    public object[] GetPendingRequests(string group, string countOnly)
    {
        List<object> objReturn = new List<object>();
        if (IsApprover(userID.Name))
        {
            ce.GetPendingApprovals(group);

            DateTime date1;
            DateTime date2;
            foreach (Dictionary<string, string> dr in ce.ce_dt)
            {
                StringBuilder str1 = new StringBuilder();
                if ((DateTime.TryParse(dr["StartDate"].ToString(), out date1)) &&
                    (DateTime.TryParse(dr["EndDate"].ToString(), out date2)))
                {
                    if (!HelperMethods.ConvertBitToBoolean(countOnly))
                    {
                        string desc = dr["Description"].ToString();
                        if (string.IsNullOrEmpty(dr["Description"].ToString()))
                        {
                            desc = "No descrition available";
                        }
                        var mb = new MemberDatabase(dr["Username"].ToString());
                        string accept = "<a href='#' onclick='ApproveRequest(\"" + dr["ID"].ToString() + "\");return false;' title='Approve' class='cursor-pointer margin-right'><img alt='approve' src='App_Themes/" + _sitetheme + "/App/approve.png' class='pad-all-sml' style='height: 16px;' /></a>";
                        string reject = "<a href='#' onclick='RejectRequest(\"" + dr["ID"].ToString() + "\");return false;' title='Reject' class='cursor-pointer'><img alt='approve' src='App_Themes/" + _sitetheme + "/App/reject.png' class='pad-all-sml' style='height: 16px;' /></a>";

                        string userColor = UserImageColorCreator.CreateImgColor(mb.AccountImage, mb.UserColor, mb.UserId, 35, mb.SiteTheme);
                       
                        str1.Append("<div class='clear-margin'>");
                        str1.Append("<div class='pad-all-sml float-left'>" + userColor + HelperMethods.MergeFMLNames(mb) + "</div><div class='float-right'>" + accept + reject + "</div><div class='clear-space-two'></div>");
                        str1.Append("<b class='pad-right-sml'>Reason:</b>" + dr["Reason"].ToString() + "<div class='clear-space-two'></div>");
                        str1.Append("<b class='pad-right-sml'>Start Date/Time:</b>" + date1.ToShortDateString() + " - " + date2.ToShortDateString() + "<div class='clear-space-two'></div>");
                        str1.Append("<b class='pad-right-sml'>End Date/Time:</b>" + date1.ToShortTimeString() + " - " + date2.ToShortTimeString() + "<div class='clear-space-two'></div>");
                        str1.Append("<b class='pad-right-sml'>Hours:</b>" + dr["Hours"].ToString() + "<div class='clear-space-two'></div>");
                        str1.Append(desc + "<div class='clear-space-two'></div>");
                        str1.Append("</div><div class='clear-space'></div>");
                    }
                    objReturn.Add(str1.ToString());
                }
            }
        }
        return objReturn.ToArray();
    }

    [WebMethod]
    public object[] ApproveRequest(string group, string id)
    {
        bool cancontinue = true;
        ce.getEntry(id);
        if (ce.ce_dt.Count > 0)
        {
            if (HelperMethods.ConvertBitToBoolean(ce.ce_dt[0]["Approved"]))
            {
                cancontinue = false;
            }
        }

        if (cancontinue)
        {
            string u = ce.GetUsername(id);
            ce.updateEntry(id, "1");
            if (!string.IsNullOrEmpty(u))
            {
                ce.getEntry(id);

                if (ce.ce_dt.Count == 1)
                {
                    System.Net.Mail.MailMessage mailTo = new System.Net.Mail.MailMessage();
                    string messageBody = "Your vacation request has been approved for dates <b>" + ce.ce_dt[0]["StartDate"].ToString() + "</b> to <b>" + ce.ce_dt[0]["EndDate"].ToString() + "</b>.";
                    UserNotificationMessages un = new UserNotificationMessages(u);
                    string email = un.attemptAdd("app-workcalendar", messageBody, false);
                    if (!string.IsNullOrEmpty(email))
                        mailTo.To.Add(email);

                    UserNotificationMessages.finishAdd(mailTo, "app-workcalendar", messageBody);

                    MembershipUserCollection coll = Membership.GetAllUsers();
                    foreach (MembershipUser xUser in from MembershipUser _u in coll
                                                     where _u.IsOnline
                                                     let temp_member = new MemberDatabase(_u.UserName)
                                                     where (temp_member.UserHasApp("app-workcalendar")) && (_u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower()) && (temp_member.GroupList.Contains(@group))
                                                     select _u)
                    {
                        uuf.addFlag(u, "workspace", group);
                    }
                }
            }
        }
        
        return GetPendingRequests(group, "false");
    }

    [WebMethod]
    public object[] RejectRequest(string group, string id)
    {
        bool cancontinue = true;
        ce.getEntry(id);
        if (ce.ce_dt.Count > 0)
        {
            if (HelperMethods.ConvertBitToBoolean(ce.ce_dt[0]["Approved"]))
            {
                cancontinue = false;
            }
        }

        if (cancontinue)
        {
            string u = ce.GetUsername(id);
            ce.updateEntry(id, "2");
            if (!string.IsNullOrEmpty(u))
            {
                ce.getEntry(id);

                if (ce.ce_dt.Count == 1)
                {
                    System.Net.Mail.MailMessage mailTo = new System.Net.Mail.MailMessage();
                    string messageBody = "Your vacation request has been denied for dates <b>" + ce.ce_dt[0]["StartDate"].ToString() + "</b> to <b>" + ce.ce_dt[0]["EndDate"].ToString() + "</b>.";
                    UserNotificationMessages un = new UserNotificationMessages(u);
                    string email = un.attemptAdd("app-workcalendar", messageBody, false);
                    if (!string.IsNullOrEmpty(email))
                        mailTo.To.Add(email);

                    UserNotificationMessages.finishAdd(mailTo, "app-workcalendar", messageBody);

                    MembershipUserCollection coll = Membership.GetAllUsers();
                    foreach (MembershipUser xUser in from MembershipUser _u in coll
                                                     where _u.IsOnline
                                                     let temp_member = new MemberDatabase(_u.UserName)
                                                     where (temp_member.UserHasApp("app-workcalendar")) && (_u.UserName.ToLower() != HttpContext.Current.User.Identity.Name.ToLower()) && (temp_member.GroupList.Contains(@group))
                                                     select _u)
                    {
                        uuf.addFlag(u, "workspace", group);
                    }
                }
            }
        }
        
        return GetPendingRequests(group, "false");
    }

    [WebMethod]
    public object[] CalculateTotalHours(string fulltimeStart, string fulltimeEnd, string userName, string reason, string group)
    {
        List<object> objReturn = new List<object>();
        if (userName.ToLower() == "loading...") {
            return objReturn.ToArray();
        }

        objReturn.Add(IsApprover(userID.Name).ToString().ToLower());
        
        DateTime start;
        DateTime end;
        if ((DateTime.TryParse(fulltimeStart, out start)) && (DateTime.TryParse(fulltimeEnd, out end)))
        {
            double days = end.Subtract(start).Days;

            TimeSpan ts_start = start.TimeOfDay;
            TimeSpan ts_end = end.TimeOfDay;

            TimeSpan ts_final = ts_end.Subtract(ts_start);
            double hours = ts_final.TotalHours;

            if (hours / 8 >= 1)
            {
                hours--;
            }

            if (hours <= 0)
            {
                objReturn.Add("false");
                return objReturn.ToArray();
            }
            else
            {
                var vt = new VacationTypes(false);
                var m = new MemberDatabase(userName);
                if (CalculateRemainingHours(m, group) < hours)
                {
                    if (vt.ce_Deduct(reason, group))
                    {
                        objReturn.Add("false");
                        return objReturn.ToArray();
                    }
                }
            }
            
            if (days > 0)
                hours = hours * (days + 1);

            objReturn.Add(hours.ToString());
            return objReturn.ToArray();
        }

        objReturn.Add("false");
        return objReturn.ToArray();
    }

    [WebMethod]
    public object[] Loadce_Overview(string group)
    {
        var str = new StringBuilder();
        var str2 = new StringBuilder();
        MembershipUserCollection coll = Membership.GetAllUsers();
        var apps = new App(string.Empty);
        int count = 0;

        str.Append("<table cellpadding='5' cellspacing='5' style='width: 100%;'>");
        str2.Append("<table cellpadding='5' cellspacing='5' style='width: 100%;'>");
        
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
                        bool canadd = false;
                        if (IsApprover(userID.Name))
                        {
                            canadd = true;
                        }
                        else
                        {
                            if (u.UserName.ToLower() == userID.Name.ToLower())
                                canadd = true;
                        }

                        if (canadd)
                        {
                            string name = HelperMethods.MergeFMLNames(mb);
                            str.Append("<tr>");
                            str.Append("<td>" + name + "</td><td>" + mb.VacationTime + " hour(s)</td>");
                            str.Append("</tr>");

                            str2.Append("<tr>");
                            str2.Append("<td>" + name + "</td><td>" + CalculateRemainingHours(mb, group) + " hour(s)</td>");
                            str2.Append("</tr>");

                            count++;
                        }
                    }
                }
            }
        }

        str.Append("</table>");
        str2.Append("</table>");
        
        if (count == 0)
        {
            str.Append("<small class='font-color-black'><i>No users available</i></small>");
        }

        List<object> objReturn = new List<object>();
        objReturn.Add(str.ToString());
        objReturn.Add(str2.ToString());
        return objReturn.ToArray();
    }

    [WebMethod]
    public object[] BuildTextBase(string group)
    {
        var str1 = new StringBuilder();
        var str2 = new StringBuilder();
        var str3 = new StringBuilder();
        var str4 = new StringBuilder();

        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        int count4 = 0;

        CalendarEntries _ce = new CalendarEntries(HttpContext.Current.User.Identity.Name);
        _ce.getEntries(group);

        foreach (Dictionary<string, string> dr in _ce.ce_dt.Cast<Dictionary<string, string>>().Where(dr => dr["GroupName"].ToString() == group))
        {
            DateTime date1;
            DateTime date2;
            #region Cancelled

            if (dr["Approved"].ToString() == "3")
            {
                if ((DateTime.TryParse(dr["StartDate"].ToString(), out date1)) &&
                    (DateTime.TryParse(dr["EndDate"].ToString(), out date2)))
                {
                    DateTime date3;
                    if (DateTime.TryParse(dr["DateUpdated"].ToString(), out date3))
                    {
                        if (IsRecent(date3))
                        {
                            string desc = dr["Description"].ToString();
                            if (string.IsNullOrEmpty(dr["Description"].ToString()))
                            {
                                desc = "No descrition available";
                            }
                            var mb = new MemberDatabase(dr["Username"].ToString());
                            string userColor = UserImageColorCreator.CreateImgColor(mb.AccountImage, mb.UserColor, mb.UserId, 35, mb.SiteTheme);
                            
                            str1.Append("<tr class='myItemStyle GridNormalRow'>");
                            str1.Append("<td width='45px' align='center' class='GridViewNumRow'>" + (count1 + 1).ToString() + "</td>");
                            str1.Append("<td valign='middle' class='border-right border-bottom'><div class='pad-all-sml'>" + userColor + HelperMethods.MergeFMLNames(mb) + "</div></td>");
                            str1.Append("<td class='border-right border-bottom' align='center'>" + dr["Reason"] + "</td>");
                            str1.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortDateString() + " - " + date2.ToShortDateString() + "</td>");
                            str1.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortTimeString() + " - " + date2.ToShortTimeString() + "</td>");
                            str1.Append("<td class='border-right border-bottom' align='center'>" + dr["Hours"] + "</td>");
                            str1.Append("<td class='border-right border-bottom'>" + desc + "</td>");
                            str1.Append("</tr>");
                            count1++;
                        }
                    }
                }
            }

            #endregion

            #region Off Now/Off Tomorrow

            if (HelperMethods.ConvertBitToBoolean(dr["Approved"]))
            {
                if ((DateTime.TryParse(dr["StartDate"].ToString(), out date1)) &&
                    (DateTime.TryParse(dr["EndDate"].ToString(), out date2)))
                {
                    DateTime targetDt = ServerSettings.ServerDateTime;
                    if (targetDt.Ticks >= date1.Ticks && targetDt.Ticks <= date2.Ticks)
                    {
                        string desc = dr["Description"].ToString();
                        if (string.IsNullOrEmpty(dr["Description"].ToString()))
                        {
                            desc = "No descrition available";
                        }
                        var mb = new MemberDatabase(dr["Username"].ToString());
                        string userColor = UserImageColorCreator.CreateImgColor(mb.AccountImage, mb.UserColor, mb.UserId, 35, mb.SiteTheme);
                            
                        str2.Append("<tr class='myItemStyle GridNormalRow'>");
                        str2.Append("<td width='45px' align='center' class='GridViewNumRow'>" + (count2 + 1).ToString() + "</td>");
                        str2.Append("<td valign='middle' class='border-right border-bottom'><div class='pad-all-sml'>" + userColor + HelperMethods.MergeFMLNames(mb) + "</div></td>");
                        str2.Append("<td class='border-right border-bottom' align='center'>" + dr["Reason"] + "</td>");
                        str2.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortDateString() + " - " + date2.ToShortDateString() + "</td>");
                        str2.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortTimeString() + " - " + date2.ToShortTimeString() + "</td>");
                        str2.Append("<td class='border-right border-bottom' align='center'>" + dr["Hours"] + "</td>");
                        str2.Append("<td class='border-right border-bottom'>" + desc + "</td>");
                        str2.Append("</tr>");
                        count2++;
                    }

                    int currYear = ServerSettings.ServerDateTime.Year;
                    int currMonth = ServerSettings.ServerDateTime.Month;
                    int dayOfMonth = ServerSettings.ServerDateTime.Day;
                    int numDaysInMonth = DateTime.DaysInMonth(ServerSettings.ServerDateTime.Year, ServerSettings.ServerDateTime.Month);
                    if (dayOfMonth < numDaysInMonth)
                        dayOfMonth = ServerSettings.ServerDateTime.Day + 1;
                    else if (dayOfMonth >= numDaysInMonth)
                    {
                        dayOfMonth = 1;
                        currMonth += 1;
                        if (currMonth > 12)
                        {
                            currMonth = 1;
                            currYear += 1;
                        }
                    }

                    var nextDay = new DateTime(currYear, currMonth, dayOfMonth, date1.Hour, date1.Minute, date1.Second);
                    if (nextDay.Ticks >= date1.Ticks && nextDay.Ticks <= date2.Ticks)
                    {
                        string desc = dr["Description"].ToString();
                        if (string.IsNullOrEmpty(dr["Description"].ToString()))
                        {
                            desc = "No descrition available";
                        }
                        var mb = new MemberDatabase(dr["Username"].ToString());
                        string userColor = UserImageColorCreator.CreateImgColor(mb.AccountImage, mb.UserColor, mb.UserId, 35, mb.SiteTheme);
                            
                        str3.Append("<tr class='myItemStyle GridNormalRow'>");
                        str3.Append("<td width='45px' align='center' class='GridViewNumRow'>" + (count3 + 1).ToString() + "</td>");
                        str3.Append("<td valign='middle' class='border-right border-bottom'><div class='pad-all-sml'>" + userColor + HelperMethods.MergeFMLNames(mb) + "</div></td>");
                        str3.Append("<td class='border-right border-bottom' align='center'>" + dr["Reason"] + "</td>");
                        str3.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortDateString() + " - " + date2.ToShortDateString() + "</td>");
                        str3.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortTimeString() + " - " + date2.ToShortTimeString() + "</td>");
                        str3.Append("<td class='border-right border-bottom' align='center'>" + dr["Hours"] + "</td>");
                        str3.Append("<td class='border-right border-bottom'>" + desc + "</td>");
                        str3.Append("</tr>");
                        count3++;
                    }
                }
            }

            #endregion

            #region Rejected

            if (dr["Approved"].ToString() == "2")
            {
                if ((DateTime.TryParse(dr["StartDate"].ToString(), out date1)) &&
                    (DateTime.TryParse(dr["EndDate"].ToString(), out date2)))
                {
                    DateTime date3;
                    if (DateTime.TryParse(dr["DateUpdated"].ToString(), out date3))
                    {
                        if (IsRecent(date3))
                        {
                            bool canadd = false;
                            if (IsApprover(userID.Name))
                            {
                                canadd = true;
                            }
                            else
                            {
                                if (dr["Username"].ToString().ToLower() == userID.Name.ToLower())
                                    canadd = true;
                            }

                            if (canadd)
                            {
                                string desc = dr["Description"].ToString();
                                if (string.IsNullOrEmpty(dr["Description"].ToString()))
                                {
                                    desc = "No descrition available";
                                }
                                var mb = new MemberDatabase(dr["Username"].ToString());
                                string userColor = UserImageColorCreator.CreateImgColor(mb.AccountImage, mb.UserColor, mb.UserId, 35, mb.SiteTheme);
                            
                                str4.Append("<tr class='myItemStyle GridNormalRow'>");
                                str4.Append("<td width='45px' align='center' class='GridViewNumRow'>" + (count4 + 1).ToString() + "</td>");
                                str4.Append("<td valign='middle' class='border-right border-bottom'><div class='pad-all-sml'>" + userColor + HelperMethods.MergeFMLNames(mb) + "</div></td>");
                                str4.Append("<td class='border-right border-bottom' align='center'>" + dr["Reason"] + "</td>");
                                str4.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortDateString() + " - " + date2.ToShortDateString() + "</td>");
                                str4.Append("<td class='border-right border-bottom' align='center'>" + date1.ToShortTimeString() + " - " + date2.ToShortTimeString() + "</td>");
                                str4.Append("<td class='border-right border-bottom' align='center'>" + dr["Hours"] + "</td>");
                                str4.Append("<td class='border-right border-bottom'>" + desc + "</td>");
                                str4.Append("</tr>");
                                count4++;
                            }
                        }
                    }
                }
            }

            #endregion
        }


        StringBuilder header1 = new StringBuilder();
        StringBuilder header2 = new StringBuilder();

        header1.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
        header1.Append("<tr class='myHeaderStyle'><td width='54px'></td><td>Employee Name</td><td>Reason</td><td>Date Range</td><td>Time Range</td><td>Hours</td><td>Description</td><td width='75px'></td></tr>");

        header2.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
        header2.Append("<tr class='myHeaderStyle'><td width='54px'></td><td>Employee Name</td><td>Reason</td><td>Date Range</td><td>Time Range</td><td>Hours</td><td>Description</td></tr>");

        string empty1 = string.Empty;
        string empty2 = string.Empty;
        string empty3 = string.Empty;
        string empty4 = string.Empty;
        if (count1 == 0)
            empty1 = "<div class='emptyGridView'>No Cancelled Requests</div>";
        if (count2 == 0)
            empty2 = "<div class='emptyGridView'>No events right now</div>";
        if (count3 == 0)
            empty3 = "<div class='emptyGridView'>No Rejected Requests</div>";
        if (count4 == 0)
            empty4 = "<div class='emptyGridView'>No events tomorrow</div>";

        List<object> objReturn = new List<object>();
        objReturn.Add(header1.ToString() + str1.ToString() + "</tbody></table>" + empty1);
        objReturn.Add(header2.ToString() + str2.ToString() + "</tbody></table>" + empty2);
        objReturn.Add(header2.ToString() + str3.ToString() + "</tbody></table>" + empty3);
        objReturn.Add(header2.ToString() + str4.ToString() + "</tbody></table>" + empty4);
        
        return objReturn.ToArray();
    }

    private bool IsRecent(DateTime postDate)
    {
        DateTime now = ServerSettings.ServerDateTime;
        TimeSpan final = now.Subtract(postDate);
        bool x = false;
        if (final.Days <= 1)
        {
            if (final.Days == 0)
            {
                x = true;
            }
        }
        return x;
    }
    
    private int GetDaysInMonth(int month, int year)
    {
        return Math.Abs(DateTime.DaysInMonth(year, month));
    }

    private static string ShowDescription(string desc, string daterange)
    {
        string str = string.Empty;
        if (!string.IsNullOrEmpty(desc))
        {
            if ((desc.Length < 50) && (desc.ToLower() != "no description available"))
            {
                str += desc;
            }
        }

        try {
            string[] x = daterange.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            DateTime start = Convert.ToDateTime(x[0]);
            DateTime end = Convert.ToDateTime(x[1]);
            str += " " + start.ToShortTimeString() + " - " + end.ToShortTimeString();
        }
        catch { }

        return str;
    }

    private double CalculateRemainingHours(MemberDatabase m, string group)
    {
        string totalhours = m.VacationTime;
        double remaininghours;
        double.TryParse(totalhours, out remaininghours);
        int count = 0;
        var vt = new VacationTypes(false);
        CalendarEntries _ce = new CalendarEntries(m.Username);
        _ce.getEntries(group);
        foreach (Dictionary<string, string> dr in _ce.ce_dt)
        {
            if ((HelperMethods.ConvertBitToBoolean(dr["Approved"])) &&
                (vt.ce_Deduct(dr["Reason"].ToString(), dr["GroupName"].ToString()))
                && (dr["Username"].ToString() == m.Username))
            {
                int h;
                int.TryParse(dr["Hours"].ToString(), out h);
                remaininghours = remaininghours - h;
                count++;
            }
        }

        return remaininghours;
    }

    public static bool InBetweenDaysInclusive(DateTime datetime, DateTime start, DateTime end)
    {
        if (datetime.Ticks >= start.Ticks && datetime.Ticks <= end.Ticks)
            return true;

        return false;
    }

    private void AddMonthInfo(string id, string usercolor, string fulldate, int len, string title, string desc, string daterange, ref List<object> returnObj)
    {
        if ((string.IsNullOrEmpty(usercolor)) || (usercolor == "#"))
            usercolor = "#EFEFEF";
        List<object> schList = new List<object>();
        schList.Add(id);
        schList.Add(usercolor);
        schList.Add(fulldate);
        schList.Add(len);
        schList.Add(title + "/n" + ShowDescription(desc, daterange));
        schList.Add(daterange);
        returnObj.Add(schList);
    }

    private bool IsApprover(string currUser)
    {
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp("app-workcalendar");
        foreach (Dictionary<string, string> dr in appParams.listdt)
        {
            try
            {
                string param = dr["Parameter"];
                int indexOf = param.IndexOf("=") + 1;
                string subParam = param.Substring(indexOf);
                string[] splitUsers = subParam.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string user in splitUsers)
                {
                    if (currUser.ToLower() == user.ToLower())
                        return true;
                }
            }
            catch { }
        }

        return false;
    }
}