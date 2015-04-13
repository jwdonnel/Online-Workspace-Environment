<%@ WebService Language="C#" Class="PersonalCalendar" %>

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

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class PersonalCalendar  : System.Web.Services.WebService 
{
    private readonly UserCalendar ce;
    private readonly IIdentity userID;
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly string _sitetheme;


    public PersonalCalendar()
    {
        userID = HttpContext.Current.User.Identity;
        ce = new UserCalendar(userID.Name);
        var member = new MemberDatabase(userID.Name);

        _sitetheme = member.SiteTheme;
    }

    [WebMethod]
    public string AddEvent(string title, string desc, string startdate, string enddate, string color)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                DateTime _start = new DateTime();
                DateTime.TryParse(startdate, out _start);
                DateTime _end = new DateTime();
                DateTime.TryParse(enddate, out _end);

                ce.AddEvent(_start.ToString(), _end.ToString(), desc, title, color);
                uuf.addFlag(userID.Name, "workspace", "");
            }
            catch
            {
                return "false";
            }
        }
        return "false";
    }

    [WebMethod]
    public string UpdateEvent(string id, string title, string desc, string startdate, string enddate, string color)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                DateTime _start = new DateTime();
                DateTime.TryParse(startdate, out _start);
                DateTime _end = new DateTime();
                DateTime.TryParse(enddate, out _end);

                ce.UpdateEvent(id, _start.ToString(), _end.ToString(), desc, title, color);
                uuf.addFlag(userID.Name, "workspace", "");
            }
            catch
            {
                return "false";
            }
        }
        return "false";
    }

    [WebMethod]
    public string GetOverview()
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                ce.GetEvents();
                StringBuilder str = new StringBuilder();
                int count = 0;
                foreach (Dictionary<string, string> dr in ce.calendar_dt)
                {
                    var startmonth = new DateTime();
                    if (DateTime.TryParse(dr["StartDate"], out startmonth))
                    {
                        var endmonth = new DateTime();
                        if (DateTime.TryParse(dr["EndDate"], out endmonth))
                        {

                            if (InBetweenDaysInclusive(DateTime.Now.Date, startmonth, endmonth))
                            {
                                str.Append("<div class='personalcalendar-overlay-entry'>");
                                str.Append("<div class='personalcalendar-entry-bg' style='background-color:" + dr["ColorCode"] + "'></div>");
                                str.Append("<h4>" + dr["Title"] + "</h4>");
                                str.Append("<div class='clear'></div>");
                                str.Append(dr["Description"]);
                                str.Append("<div class='clear'></div>");
                                str.Append("<div class='float-left font-bold' style='width: 50px'>From</div>" + dr["StartDate"]);
                                str.Append("<div class='clear'></div>");
                                str.Append("<div class='float-left font-bold' style='width: 50px'>To</div>" + dr["EndDate"]);
                                str.Append("</div>");

                                if ((count % 2 == 0) && (count > 0))
                                    str.Append("<div class='clear-space'></div>");
                                
                                count++;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(str.ToString()))
                    return str.ToString();
            }
            catch
            {
                return "<h4 class='pad-all'>No events available</h4>";
            }
        }
        return "<h4 class='pad-all'>No events available</h4>";
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
                ce.GetEvent(id);
                if (ce.calendar_dt.Count == 1)
                {
                    Dictionary<string, string> dr = ce.calendar_dt[0];
                    string usercolor = "<div class='sch_ColorCode float-left rounded-corners-15' style='border:1px solid #DDD;margin-top:5px;filter:alpha(opacity=50);opacity:0.50;color:transparent;background-color:" + dr["ColorCode"] + "'></div>";
                    str.Append("<div class='float-left pad-right'><img alt='' src='App_Themes/" + _sitetheme + "/App/plan.png' /></div>");
                    str.Append("<div class='float-left'><h2><b>" + usercolor + dr["Title"] + "</b></h2><div class='clear-space-two'></div>");
                    str.Append("<small><b class='pad-right-sml'>Updated:</b>" + dr["DateUpdated"] + "</small></div>");
                    str.Append("<div class='clear'></div>");
                    str.Append("<div class='clear-space'></div>");
                    str.Append("<b class='pad-right-sml'>Start Time:</b>" + dr["StartDate"] + "<div class='clear-space-two'></div>");
                    str.Append("<b class='pad-right-sml'>End Time:</b>" + dr["EndDate"] + "<div class='clear-space-two'></div>");
                    str.Append("<div class='clear-space'></div><b>Description:</b><div class='clear'></div>");
                    string desc = dr["Description"];
                    
                    if (string.IsNullOrEmpty(desc))
                        desc = "No description available";
                    
                    str.Append(desc);
                    str.Append("<div class='clear-space'></div><div class='clear-space'></div><div class='clear-space'></div>");
                    str.Append("<input type='button' class='input-buttons' value='Close' onclick=\"ViewNewCalendarEvent_pc();\" />");
                    str.Append("<input type='button' class='input-buttons float-right no-margin' value='Delete' onclick=\"DeleteEventInfo_pc('" + dr["ID"] + "');\" />");

                    string editClick = " onclick=\"EditCalendarEvent_pc('" + dr["Title"] + "','" + dr["Description"] + "','" + dr["ColorCode"] + "','" + dr["StartDate"] + "','" + dr["EndDate"] + "');\"";
                    str.Append("<input type='button' class='input-buttons float-right margin-right' value='Edit' " + editClick + " />");
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
    public string DeleteEventInfo(string id)
    {
        if (userID.IsAuthenticated)
        {
            try
            {
                ce.GetEvent(id);
                if (ce.calendar_dt.Count == 1)
                {
                    ce.DeleteEvent(id);
                    uuf.addFlag(userID.Name, "workspace", "");
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
    public object[] GetMonthInfo(string month, string year, string search)
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

                ce.GetEvents();
                string str = string.Empty;
                foreach (Dictionary<string, string> dr in ce.calendar_dt)
                {
                    var startDate = new DateTime();
                    if (DateTime.TryParse(dr["StartDate"], out startDate))
                    {
                        var endDate = new DateTime();
                        if (DateTime.TryParse(dr["EndDate"], out endDate))
                        {
                            if ((!string.IsNullOrEmpty(search)) && (!dr["Title"].ToLower().Contains(search)) && (!dr["Description"].ToLower().Contains(search)))
                                continue;
                            
                            var member = new MemberDatabase(dr["UserName"]);
                            string usercolor = dr["ColorCode"];
                            string daterange = dr["StartDate"] + " - " + dr["EndDate"];

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
                                        AddMonthInfo(dr["ID"].ToString(), usercolor, fulldate, Math.Abs(7 - dayofweek), dr["Title"], dr["Description"], daterange, ref returnObj);
                                        fulldate = ((startDate.Month - 1) + "_" + startday + "_" + startDate.Year);
                                        length = Math.Abs(startday - endday) + 1;
                                        if ((length + dayofweek) > 7)
                                            dayofweek = 0;
                                    }
                                    else
                                    {
                                        if (length <= 0)
                                            break;

                                        AddMonthInfo(dr["ID"], usercolor, fulldate, length, dr["Title"], dr["Description"], daterange, ref returnObj);
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
                                        AddMonthInfo(dr["ID"], usercolor, fulldate, Math.Abs(7 - dayofweek), dr["Title"], dr["Description"], daterange, ref returnObj);
                                        fulldate = ((endDate.Month - 1) + "_" + startday + "_" + endDate.Year);
                                        length = Math.Abs(startday - endday) + 1;
                                        if ((length + dayofweek) > 7)
                                            dayofweek = 0;
                                    }
                                    else
                                    {
                                        if (length <= 0)
                                            break;

                                        AddMonthInfo(dr["ID"], usercolor, fulldate, Math.Abs(7 - dayofweek), dr["Title"], dr["Description"], daterange, ref returnObj);
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
                                            AddMonthInfo(dr["ID"], usercolor, fulldate, Math.Abs(7 - dayofweek), dr["Title"], dr["Description"], daterange, ref returnObj);
                                            fulldate = ((Convert.ToInt32(month) - 1) + "_" + startday + "_" + year);
                                            length = Math.Abs(startday - endday) + 1;
                                            if ((length + dayofweek) > 7)
                                                dayofweek = 0;
                                        }
                                        else
                                        {
                                            if (length <= 0)
                                                break;

                                            AddMonthInfo(dr["ID"], usercolor, fulldate, length, dr["Title"], dr["Description"], daterange, ref returnObj);
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

    public static bool InBetweenDaysInclusive(DateTime datetime, DateTime start, DateTime end)
    {
        if (datetime.Ticks >= start.Ticks && datetime.Ticks <= end.Ticks)
            return true;
        
        return false;
    }

    private void AddMonthInfo(string id, string usercolor, string fulldate, int len, string title, string desc, string daterange, ref List<object> returnObj)
    {
        List<object> schList = new List<object>();
        schList.Add(id);
        schList.Add(usercolor);
        schList.Add(fulldate);
        schList.Add(len);
        schList.Add(title + "/n" + ShowDescription(desc, daterange));
        schList.Add(daterange);
        returnObj.Add(schList); 
    }
    
}