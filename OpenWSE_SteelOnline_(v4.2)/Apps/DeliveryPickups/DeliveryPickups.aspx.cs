#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_iFrames_DeliveryPickups : Page
{
    #region private variables

    private const string AppId = "app-deliverypickups";
    private readonly IPWatch _ipwatch = new IPWatch(true);
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<Scheduler_Coll> _schedulerColl;
    private AppLog _applog;
    private MemberDatabase _member;
    private int _pageSize;
    private Scheduler _schedule;
    private AppInitializer _appInitializer;

    #endregion

    #region public variables

    public string a;
    public string b;
    public string c;
    public string class1;
    public string class2;
    public string class3;
    public string class4;
    public string class5;
    public string d;
    public string e;
    public string hf_rt;
    public string s1;
    public string s2;

    #endregion

    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(AppId, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent)
        {
            _member = _appInitializer.memberDatabase;

            updateJavaScript();

            _applog = new AppLog(false);
            _schedule = new Scheduler();
            if (!IsPostBack)
            {
                // Initialize all the scripts and style sheets
                App apps = new App();
                lbl_title.Text = apps.GetAppName(AppId);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_refreshTimer_deliverypickups.ClientID, AppId, this);
                aus.StartAutoUpdates();

                setPageSize(1);
                _schedulerColl = new List<Scheduler_Coll>();
                double ver = getInternetExplorerVersion();
                if (ver > 0.0)
                {
                    GV_Schedule_deliverypickups.GridLines = ver <= 7.0 ? GridLines.Both : GridLines.None;
                }
                else
                {
                    GV_Schedule_deliverypickups.GridLines = GridLines.None;
                }
                hf_dateselected_deliverypickups.Value = DateTime.Now.Month + "/" + DateTime.Now.Day + "/" +
                                                        DateTime.Now.Year;
                Calendar1_deliverypickups.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                                                      DateTime.Now.Day);
                lbl_currDate_deliverypickups.Text = DateTime.Now.Month + "/" + DateTime.Now.Day + "/" +
                                                    DateTime.Now.Year;
                string x = DateTime.Now.ToLongDateString();
                int endIndex = x.IndexOf(',');
                string xday = x.Substring(0, endIndex);
                string xdate = x.Substring(endIndex + 1);
                lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
                hf_category_deliverypickups.Value = Request.QueryString["cat"];
                LoadSchedule(ref GV_Schedule_deliverypickups);
            }
            else
            {
                _pageSize = Convert.ToInt32(dd_display_deliverypickups.SelectedValue);
                setPageSize(_pageSize);
                setCategoryIndex();
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void getNextApp()
    {
        pnl_nextApp_deliverypickups.Controls.Clear();
        DateTime currTime = DateTime.Now;
        var str = new StringBuilder();
        var dates = (from t in _schedule.scheduler_coll let final = currTime.Subtract(Convert.ToDateTime(t.ScheduleDate)) where (final.Ticks < 0) && (!t.Complete) select Convert.ToDateTime(t.ScheduleDate)).ToList();
        if (dates.Count > 0)
        {
            try
            {
                dates.Sort();
                string d = dates[0].ToString();
                string ampm = d.Substring(d.Length - 2);
                d = d.Substring(0, d.Length - 6) + " " + ampm;
                var timeslot = new Scheduler(d);
                var ltl_nextApp = new Literal();
                str.Append("<span class='pad-right'><b style='color: #707070'>Company:</b></span><span>" +
                           timeslot.scheduler_coll[0].Company + "</span><br />");
                str.Append("<span class='pad-right'><b style='color: #707070'>Truck #:</b></span><span>" +
                           timeslot.scheduler_coll[0].TruckNum + "</span><br />");
                str.Append("<span class='pad-right'><b style='color: #707070'>Del. From:</b></span><span>" +
                           timeslot.scheduler_coll[0].DeliveryFrom + "</span><br />");
                str.Append("<span class='pad-right'><b style='color: #707070'>Type:</b></span><span>" +
                           timeslot.scheduler_coll[0].ScheduleType + "</span><br />");
                str.Append("<span class='pad-right'><b style='color: #707070'>Date:</b></span><span>" +
                           timeslot.scheduler_coll[0].ScheduleDate + "</span>");
                ltl_nextApp.Text = str.ToString();
                pnl_nextApp_deliverypickups.Controls.Add(ltl_nextApp);
            }
            catch
            {
            }
        }
        else
        {
            var emptyDates = new Label { Text = "No upcoming deliveries or pickups." };
            pnl_nextApp_deliverypickups.Controls.Add(emptyDates);
        }
    }

    private float getInternetExplorerVersion()
    {
        float rv = -1;
        HttpBrowserCapabilities browser = Request.Browser;
        if (browser.Browser == "IE")
            rv = (float)(browser.MajorVersion + browser.MinorVersion);
        return rv;
    }

    public void updateJavaScript()
    {
        a = hf_dateselected_deliverypickups.ClientID;
        b = schedule_items_deliverypickups.ClientID;
        c = btn_finish_deliverypickups.ClientID;
        d = lbl_SelectedDate_deliverypickups.ClientID;
        e = cb_sendEmail_deliverypickups.ClientID;
        s1 = Step1_deliverypickups.ClientID;
        s2 = Step2_deliverypickups.ClientID;
        hf_rt = hf_refreshTimer_deliverypickups.ClientID;
    }

    private void initializeCookies()
    {
        if (cb_viewslots_deliverypickups.Checked)
            hf_viewTimes_deliverypickups.Value = "Filled";
        getNextApp();
    }

    private void setCategoryIndex()
    {
        class1 = "";
        class2 = "";
        class3 = "";
        class4 = "";
        class5 = "";

        if (hf_category_deliverypickups.Value == "Delivery")
        {
            class1 = "active";
        }
        else if (hf_category_deliverypickups.Value == "Pickup")
        {
            class2 = "active";
        }
        else if (hf_category_deliverypickups.Value == "Completed")
        {
            class3 = "active";
        }
        else if (hf_category_deliverypickups.Value == "Opened")
        {
            class4 = "active";
        }
        else if (hf_category_deliverypickups.Value == "All")
        {
            class5 = "active";
        }
        else
        {
            hf_category_deliverypickups.Value = "All";
            class5 = "active";
        }
    }

    #endregion

    #region GridView Properties Methods

    protected void GV_Schedule_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Pager)
        {
            var PnlPager = (Panel)e.Row.FindControl("PnlPager_deliverypickups");
            for (int i = 0; i < GV_Schedule_deliverypickups.PageCount; i++)
            {
                var lbtn_page = new LinkButton
                    {
                        CommandArgument = i.ToString(CultureInfo.InvariantCulture),
                        CommandName = "PageNo",
                        CssClass =
                            GV_Schedule_deliverypickups.PageIndex == i
                                ? "GVPagerNumActive deliverypickup-update"
                                : "GVPagerNum deliverypickup-update",
                        Text = (i + 1).ToString(CultureInfo.InvariantCulture)
                    };
                PnlPager.Controls.Add(lbtn_page);
            }
        }
    }

    protected void GV_Schedule_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            try
            {
                var schDiv = (Panel)e.Row.FindControl("schDiv");
                if (schDiv.CssClass == "sch_slotfilled")
                {
                    var lbedit = (LinkButton)e.Row.FindControl("lbedit");
                    var lbComplete = (LinkButton)e.Row.FindControl("lbComplete");
                    var imgcomplete = (HtmlGenericControl)e.Row.FindControl("imgcomplete");
                    string date = lbedit.CommandArgument;
                    var temp = new Scheduler(date);
                    if (temp.scheduler_coll[0].Complete)
                    {
                        imgcomplete.Visible = true;
                        lbedit.Enabled = false;
                        lbedit.Visible = false;
                        lbComplete.Enabled = false;
                        lbComplete.Visible = false;
                    }
                    else
                    {
                        imgcomplete.Visible = false;
                        lbedit.Enabled = true;
                        lbedit.Visible = true;
                        lbComplete.Enabled = true;
                        lbComplete.Visible = true;
                    }
                }
            }
            catch
            {
            }
        }

        if (e.Row.RowType == DataControlRowType.Pager)
        {
            var lbl2 = (Label)e.Row.FindControl("pglbl2_deliverypickups");
            var btnFirst = (LinkButton)e.Row.FindControl("btnFirst_deliverypickups");
            var btnPrevious = (LinkButton)e.Row.FindControl("btnPrevious_deliverypickups");
            var btnNext = (LinkButton)e.Row.FindControl("btnNext_deliverypickups");
            var btnLast = (LinkButton)e.Row.FindControl("btnLast_deliverypickups");
            var tb_page = (TextBox)e.Row.FindControl("tb_pageManual_deliverypickups");

            lbl2.Text = GV_Schedule_deliverypickups.PageCount.ToString();
            tb_page.Text = (GV_Schedule_deliverypickups.PageIndex + 1).ToString();

            if ((GV_Schedule_deliverypickups.PageIndex + 1) == 1)
            {
                btnFirst.Visible = false;
                btnPrevious.Visible = false;
            }
            if ((GV_Schedule_deliverypickups.PageIndex + 1) == GV_Schedule_deliverypickups.PageCount)
            {
                btnNext.Visible = false;
                btnLast.Visible = false;
            }
        }
    }

    protected void GV_Schedule_RowEdit(object sender, GridViewEditEventArgs e)
    {
        GV_Schedule_deliverypickups.EditIndex = e.NewEditIndex;
        LoadSchedule(ref GV_Schedule_deliverypickups);
        var type = (HiddenField)GV_Schedule_deliverypickups.Rows[e.NewEditIndex].FindControl("hfEdit_Type");
        var dd_scheduleType =
            (DropDownList)GV_Schedule_deliverypickups.Rows[e.NewEditIndex].FindControl("dd_scheduleType");
        dd_scheduleType.SelectedIndex = type.Value == "Delivery" ? 0 : 1;
    }

    protected void GV_Schedule_RowUpdate(object sender, GridViewUpdateEventArgs e)
    {
        bool updated = true;
        var date = (HiddenField)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("hfEdit_Date");
        var tb_company = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_company");
        var tb_trucknum = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_trucknum");
        var tb_phonenumber = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_phonenumber");
        var dd_scheduleType = (DropDownList)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("dd_scheduleType");
        var tb_email = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_email");
        var tb_from = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_from");
        var tb_items = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_items");
        var tb_comment = (TextBox)GV_Schedule_deliverypickups.Rows[e.RowIndex].FindControl("tb_comment");
        try
        {
            if ((string.IsNullOrEmpty(tb_company.Text)) || (string.IsNullOrEmpty(tb_from.Text)))
            {
                updated = false;
            }
            else
            {
                _schedule.updateCompany(tb_company.Text, date.Value);
                _schedule.updateTruckNum(tb_trucknum.Text, date.Value);
                _schedule.updateDriver(tb_from.Text, date.Value);
                _schedule.updateItems(tb_items.Text, date.Value);
                _schedule.updatePhoneNumber(tb_phonenumber.Text, date.Value);
                _schedule.updateType(dd_scheduleType.SelectedValue, date.Value);
                _schedule.updateEmail(tb_email.Text, date.Value);
                _schedule.updateComment(tb_comment.Text, date.Value);
            }
        }
        catch (Exception ex) {
            _applog.AddError(ex);
        }
        if (updated)
        {
            GV_Schedule_deliverypickups.EditIndex = -1;
            LoadSchedule(ref GV_Schedule_deliverypickups);
        }
    }

    protected void GV_Schedule_CancelEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GV_Schedule_deliverypickups.EditIndex = -1;
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void GV_Schedule_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "PageNo":
                _pageSize = Convert.ToInt32(dd_display_deliverypickups.SelectedValue);
                setPageSize(_pageSize);
                LoadSchedule(ref GV_Schedule_deliverypickups);

                GV_Schedule_deliverypickups.PageIndex = Convert.ToInt32(e.CommandArgument.ToString());
                GV_Schedule_deliverypickups.DataBind();
                break;
            case "deleteSlot":
                try
                {
                    _schedule.deleteSlot(e.CommandArgument.ToString());
                }
                catch
                {
                }
                LoadSchedule(ref GV_Schedule_deliverypickups);
                break;
            case "markComplete":
                try
                {
                    _schedule.updateComplete(e.CommandArgument.ToString());
                }
                catch
                {
                }
                LoadSchedule(ref GV_Schedule_deliverypickups);
                break;
            case "newApp":
                try
                {
                    string[] temp = e.CommandArgument.ToString().Split('/');
                    string[] temp2 = temp[2].Split(' ');
                    tb_Date1_deliverypickups.Text = temp[0] + "/" + temp[1] + "/" + temp2[0];
                    buildTimeSlot(Convert.ToInt32(temp[0]), Convert.ToInt32(temp[1]),
                                  Convert.ToInt32(temp2[0]));
                    string timeSelected = temp2[1] + " " + temp2[2];
                    try
                    {
                        for (int i = 0; i < dd_schTimeSlot_deliverypickups.Items.Count; i++)
                        {
                            if (dd_schTimeSlot_deliverypickups.Items[i].Value == timeSelected)
                            {
                                dd_schTimeSlot_deliverypickups.SelectedIndex = i;
                                break;
                            }
                        }
                        lbl_SelectedDate_deliverypickups.Enabled = true;
                        lbl_SelectedDate_deliverypickups.Visible = true;
                        btn_finish_deliverypickups.Enabled = true;
                        btn_finish_deliverypickups.Visible = true;
                        lbl_SelectedDate_deliverypickups.Text = "<h3>You have selected a " + getSchType() + " for " +
                                                                Convert.ToDateTime(tb_Date1_deliverypickups.Text)
                                                                       .Date.ToLongDateString() + " at " +
                                                                dd_schTimeSlot_deliverypickups.SelectedValue + "</h3>";
                        btn_finish_deliverypickups.Text = "Schedule " + getSchType();
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
                setCategoryIndex();
                initializeCookies();
                reInitializeModal();
                Step1_deliverypickups.Attributes["style"] = "display: block;";
                Step2_deliverypickups.Attributes["style"] = "display: none;";
                Step3_deliverypickups.Attributes["style"] = "display: none;";
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.LoadModalWindow(true, 'NewDelPickup_element', 'Schedule New Delivery/Pickup');", true);
                break;
        }
    }

    protected void GV_Schedule_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        _pageSize = Convert.ToInt32(dd_display_deliverypickups.SelectedValue);
        setPageSize(_pageSize);
        LoadSchedule(ref GV_Schedule_deliverypickups);

        GV_Schedule_deliverypickups.PageIndex = e.NewPageIndex;
        GV_Schedule_deliverypickups.DataBind();
    }

    public void LoadSchedule(ref GridView gv)
    {
        if (tb_search_deliverypickups.Text.Trim().ToLower() != "search for schedule")
            SearchTable();

        setCategoryIndex();
        if (_schedulerColl != null)
        {
            if (_schedulerColl.Count > 0)
            {
                DataView dvFiles = GetTimeSlots();
                gv.DataSource = dvFiles;
                gv.DataBind();
            }
            else
            {
                initializeCookies();
                string[] date = hf_dateselected_deliverypickups.Value.Split('/');
                DataView dvFiles = GetTimeSlots((Convert.ToInt32(date[0])), Convert.ToInt32(date[1]),
                                                Convert.ToInt32(date[2]));
                gv.DataSource = dvFiles;
                gv.DataBind();
            }
        }
        else
        {
            initializeCookies();
            string[] date = hf_dateselected_deliverypickups.Value.Split('/');
            if (date.Length > 1)
            {
                DataView dvFiles = GetTimeSlots((Convert.ToInt32(date[0])), Convert.ToInt32(date[1]),
                                                Convert.ToInt32(date[2]));
                gv.DataSource = dvFiles;
                gv.DataBind();
            }
        }
    }

    public DataView GetTimeSlots(int m, int d, int y)
    {
        int apps_del = 0;
        int apps_pu = 0;
        int complete = 0;
        var dtsch = new DataTable();
        dtsch.Columns.Add(new DataColumn("TimeSlot"));
        dtsch.Columns.Add(new DataColumn("TruckNum"));
        dtsch.Columns.Add(new DataColumn("Company"));
        dtsch.Columns.Add(new DataColumn("DeliveryFrom"));
        dtsch.Columns.Add(new DataColumn("PhoneNumber"));
        dtsch.Columns.Add(new DataColumn("Email"));
        dtsch.Columns.Add(new DataColumn("Type"));
        dtsch.Columns.Add(new DataColumn("SchType"));
        dtsch.Columns.Add(new DataColumn("Comment"));
        dtsch.Columns.Add(new DataColumn("ConfirmationNum"));
        dtsch.Columns.Add(new DataColumn("Items"));
        dtsch.Columns.Add(new DataColumn("Created"));
        dtsch.Columns.Add(new DataColumn("Scheduled"));
        dtsch.Columns.Add(new DataColumn("SlotFilled"));
        dtsch.Columns.Add(new DataColumn("NewSlot"));
        dtsch.Columns.Add(new DataColumn("MarkedComp"));
        int count = 0;
        int hour = 6;
        int hourLong = 6;
        int min = 30;
        string pmam = "AM";
        string tempmin = string.Empty;
        while (hourLong <= 22)
        {
            DataRow drsch = dtsch.NewRow();
            if (hour >= 12)
            {
                pmam = "PM";
            }
            if (hour >= 13)
            {
                hour = 1;
            }
            tempmin = min == 0 ? "00" : min.ToString();
            string date = m.ToString(CultureInfo.InvariantCulture) + "/" + d.ToString(CultureInfo.InvariantCulture) + "/" + y.ToString(CultureInfo.InvariantCulture) + " " + hour.ToString() + ":" + tempmin + " " + pmam;

            DateTime _tempDate = new DateTime();
            if (!DateTime.TryParse(date, out _tempDate))
            {
                date = (m + 1).ToString(CultureInfo.InvariantCulture) + "/1/" + y.ToString(CultureInfo.InvariantCulture) + " " + hour.ToString() + ":" + tempmin +
                          " " + pmam;
            }

            if ((Convert.ToDateTime(date).DayOfWeek != DayOfWeek.Saturday) &&
                (Convert.ToDateTime(date).DayOfWeek != DayOfWeek.Sunday))
            {
                Scheduler timeslot;
                bool ok = false;
                if (hf_category_deliverypickups.Value == "All")
                {
                    timeslot = new Scheduler(date);
                    ok = timeslot.checkTimeSlot(date);
                }
                else if (hf_category_deliverypickups.Value == "Opened")
                {
                    timeslot = new Scheduler(date, false);
                    ok = timeslot.checkTimeSlot(date, false);
                }
                else if (hf_category_deliverypickups.Value == "Completed")
                {
                    timeslot = new Scheduler(date, true);
                    ok = timeslot.checkTimeSlot(date, true);
                }
                else
                {
                    timeslot = new Scheduler(date, hf_category_deliverypickups.Value);
                    ok = timeslot.checkTimeSlot(date, hf_category_deliverypickups.Value);
                }
                if (ok)
                {
                    try
                    {
                        drsch["Company"] = timeslot.scheduler_coll[0].Company;
                        drsch["TruckNum"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].TruckNum)
                                                ? "--"
                                                : timeslot.scheduler_coll[0].TruckNum;

                        drsch["PhoneNumber"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].PhoneNumber)
                                                   ? "--"
                                                   : timeslot.scheduler_coll[0].PhoneNumber;

                        drsch["ConfirmationNum"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].ConfirmationNum)
                                                       ? "--"
                                                       : timeslot.scheduler_coll[0].ConfirmationNum;

                        drsch["Email"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].Email)
                                             ? "--"
                                             : timeslot.scheduler_coll[0].Email;

                        drsch["DeliveryFrom"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].DeliveryFrom)
                                                    ? "--"
                                                    : timeslot.scheduler_coll[0].DeliveryFrom;

                        drsch["Items"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].ConfirmationNum)
                                             ? (object)"0"
                                             : timeslot.scheduler_coll[0].Items;
                        drsch["Created"] = getTime(Convert.ToDateTime(timeslot.scheduler_coll[0].TimeScheduled));
                        drsch["Comment"] = string.IsNullOrEmpty(timeslot.scheduler_coll[0].Comment)
                                               ? "--"
                                               : timeslot.scheduler_coll[0].Comment;
                        drsch["SchType"] = timeslot.scheduler_coll[0].ScheduleType.Trim();
                        drsch["SlotFilled"] = "sch_slotfilled";
                        drsch["NewSlot"] = "display-none";
                        if (timeslot.scheduler_coll[0].ScheduleType.Trim() == "Delivery")
                        {
                            drsch["Type"] = "sch_delivery";
                            apps_del++;
                        }
                        else
                        {
                            drsch["Type"] = "sch_pickup";
                            apps_pu++;
                        }

                        drsch["Scheduled"] = date;
                        if (timeslot.scheduler_coll[0].Complete)
                        {
                            complete++;
                            drsch["TimeSlot"] = hour + "&nbsp;<span class='gridviewnumrowampm'>" + tempmin + "&nbsp;" +
                                                pmam + "<br /><i>(Complete)</i></span>";
                            drsch["MarkedComp"] = "GridViewNumRowComp";
                        }
                        else
                        {
                            drsch["TimeSlot"] = hour + "&nbsp;<span class='gridviewnumrowampm'>" + tempmin + "&nbsp;" +
                                                pmam + "</span>";
                            drsch["MarkedComp"] = "GridViewNumRow";
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    drsch["SlotFilled"] = "display-none";
                    TimeSpan newTime = DateTime.Now.Subtract(Convert.ToDateTime(date));
                    if ((newTime.Ticks > 0) || (timeslot.checkTimeSlot(date)))
                    {
                        drsch["NewSlot"] = "display-none";
                    }
                    else
                    {
                        drsch["NewSlot"] = "sch_slotfilled";
                    }
                    drsch["TimeSlot"] = hour + "&nbsp;<span class='gridviewnumrowampm'>" + tempmin + "&nbsp;" + pmam +
                                        "</span>";
                    drsch["Scheduled"] = date;
                    drsch["MarkedComp"] = "GridViewNumRow";
                }
                if (hf_viewTimes_deliverypickups.Value == "All")
                {
                    dtsch.Rows.Add(drsch);
                }
                else if (hf_viewTimes_deliverypickups.Value == "Filled" && ok)
                {
                    dtsch.Rows.Add(drsch);
                }
            }

            if ((hour + ":" + tempmin + " " + pmam) == "10:00 PM")
            {
                break;
            }
            min = min + 15;
            if (min == 60)
            {
                min = 0;
                hour = hour + 1;
                hourLong = hourLong + 1;
            }
            count++;
        }
        var ttlAppsdel = (Label)Page.FindControl("lbl_ttlAppsDel_deliverypickups");
        ttlAppsdel.Text = apps_del.ToString();
        var ttlAppspu = (Label)Page.FindControl("lbl_ttlAppsPU_deliverypickups");
        ttlAppspu.Text = apps_pu.ToString();
        var lbl_ttlComplete = (Label)Page.FindControl("lbl_ttlComplete_deliverypickups");
        lbl_ttlComplete.Text = complete.ToString();
        var dvsch = new DataView(dtsch);
        return dvsch;
    }

    public DataView GetTimeSlots()
    {
        int apps_del = 0;
        int apps_pu = 0;
        int complete = 0;
        var dtsch = new DataTable();
        dtsch.Columns.Add(new DataColumn("TimeSlot"));
        dtsch.Columns.Add(new DataColumn("TruckNum"));
        dtsch.Columns.Add(new DataColumn("Company"));
        dtsch.Columns.Add(new DataColumn("DeliveryFrom"));
        dtsch.Columns.Add(new DataColumn("PhoneNumber"));
        dtsch.Columns.Add(new DataColumn("Email"));
        dtsch.Columns.Add(new DataColumn("Type"));
        dtsch.Columns.Add(new DataColumn("SchType"));
        dtsch.Columns.Add(new DataColumn("Comment"));
        dtsch.Columns.Add(new DataColumn("ConfirmationNum"));
        dtsch.Columns.Add(new DataColumn("Items"));
        dtsch.Columns.Add(new DataColumn("Created"));
        dtsch.Columns.Add(new DataColumn("Scheduled"));
        dtsch.Columns.Add(new DataColumn("SlotFilled"));
        dtsch.Columns.Add(new DataColumn("NewSlot"));
        dtsch.Columns.Add(new DataColumn("MarkedComp"));
        foreach (Scheduler_Coll t in _schedulerColl)
        {
            try
            {
                DataRow drsch = dtsch.NewRow();
                drsch["Company"] = t.Company;
                drsch["TruckNum"] = string.IsNullOrEmpty(t.TruckNum) ? "--" : t.TruckNum;

                drsch["PhoneNumber"] = string.IsNullOrEmpty(t.PhoneNumber) ? "--" : t.PhoneNumber;

                drsch["ConfirmationNum"] = string.IsNullOrEmpty(t.ConfirmationNum) ? "--" : t.ConfirmationNum;

                drsch["Email"] = string.IsNullOrEmpty(t.Email) ? "--" : t.Email;

                drsch["DeliveryFrom"] = string.IsNullOrEmpty(t.DeliveryFrom) ? "--" : t.DeliveryFrom;

                drsch["Items"] = string.IsNullOrEmpty(t.ConfirmationNum) ? (object)"0" : t.Items;
                drsch["Created"] = getTime(Convert.ToDateTime(t.TimeScheduled));
                drsch["Comment"] = string.IsNullOrEmpty(t.Comment) ? "--" : t.Comment;
                drsch["SchType"] = t.ScheduleType.Trim();
                drsch["SlotFilled"] = "sch_slotfilled";
                drsch["NewSlot"] = "display-none";
                switch (t.ScheduleType.Trim())
                {
                    case "Delivery":
                        drsch["Type"] = "sch_delivery";
                        apps_del++;
                        break;
                    default:
                        drsch["Type"] = "sch_pickup";
                        apps_pu++;
                        break;
                }
                drsch["TimeSlot"] = t.ScheduleDate;
                drsch["Scheduled"] = t.ScheduleDate;
                if (t.Complete)
                {
                    complete++;
                    drsch["MarkedComp"] = "GridViewNumRowComp";
                }
                else
                {
                    drsch["MarkedComp"] = "GridViewNumRow";
                }
                dtsch.Rows.Add(drsch);
            }
            catch
            {
            }
        }
        var ttlAppsdel = (Label)Page.FindControl("lbl_ttlAppsDel_deliverypickups");
        ttlAppsdel.Text = apps_del.ToString();
        var ttlAppspu = (Label)Page.FindControl("lbl_ttlAppsPU_deliverypickups");
        ttlAppspu.Text = apps_pu.ToString();
        var lbl_ttlComplete = (Label)Page.FindControl("lbl_ttlComplete_deliverypickups");
        lbl_ttlComplete.Text = complete.ToString();
        var dvsch = new DataView(dtsch);
        return dvsch;
    }

    private string getTime(DateTime postDate)
    {
        DateTime now = DateTime.Now;
        TimeSpan final = now.Subtract(postDate);
        string time = string.Empty;
        if (final.Days > 2)
        {
            time = postDate.ToShortDateString();
        }
        else
        {
            if (final.Days == 0)
            {
                if (final.Hours == 0)
                {
                    time = final.Minutes.ToString() + " minute(s) ago";
                }
                else
                {
                    time = final.Hours.ToString() + " hour(s) ago";
                }
            }
            else
            {
                time = final.Days.ToString() + " day(s) ago";
            }
        }
        return time;
    }

    protected void tb_pageManual_TextChanged(object sender, EventArgs e)
    {
        if (_schedule.scheduler_coll.Count > 0)
        {
            _pageSize = Convert.ToInt32(dd_display_deliverypickups.SelectedValue);
            setPageSize(_pageSize);
            setCategoryIndex();

            int page = Convert.ToInt32(((TextBox)sender).Text);
            if (page > GV_Schedule_deliverypickups.PageCount)
            {
                page = GV_Schedule_deliverypickups.PageCount;
            }
            else if (page <= 0)
            {
                page = 1;
            }

            GV_Schedule_deliverypickups.PageIndex = page - 1;
            GV_Schedule_deliverypickups.DataBind();
        }
    }

    #endregion

    #region Top GridView Controls

    protected void dd_display_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (dd_display_deliverypickups.SelectedValue == "10")
        {
            setPageSize(10);
        }
        else if (dd_display_deliverypickups.SelectedValue == "20")
        {
            setPageSize(20);
        }
        else if (dd_display_deliverypickups.SelectedValue == "30")
        {
            setPageSize(30);
        }
        else if (dd_display_deliverypickups.SelectedValue == "40")
        {
            setPageSize(40);
        }
        else if (dd_display_deliverypickups.SelectedValue == "1")
        {
            setPageSize(1);
        }
        hf_category_deliverypickups.Value = Request.QueryString["cat"];
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    private void setPageSize(int size)
    {
        switch (size)
        {
            case 10:
                GV_Schedule_deliverypickups.PageSize = 10;
                dd_display_deliverypickups.SelectedIndex = 0;
                break;
            case 20:
                GV_Schedule_deliverypickups.PageSize = 20;
                dd_display_deliverypickups.SelectedIndex = 1;
                break;
            case 30:
                GV_Schedule_deliverypickups.PageSize = 30;
                dd_display_deliverypickups.SelectedIndex = 2;
                break;
            case 40:
                GV_Schedule_deliverypickups.PageSize = 40;
                dd_display_deliverypickups.SelectedIndex = 3;
                break;
            default:
                GV_Schedule_deliverypickups.PageSize = 200;
                dd_display_deliverypickups.SelectedIndex = 4;
                break;
        }
    }

    protected void hf_refreshTimer_ValueChanged(object sender, EventArgs e)
    {
        bool cancontinue = false;
        
        if (!string.IsNullOrEmpty(hf_refreshTimer_deliverypickups.Value))
        {
            string id = _uuf.getFlag_AppID(hf_refreshTimer_deliverypickups.Value);
            if (id == AppId)
            {
                _uuf.deleteFlag(hf_refreshTimer_deliverypickups.Value);
                cancontinue = true;
            }
        }

        if (cancontinue)
        {
            if (lbl_currDate_deliverypickups.Text != "N/A")
            {
                try
                {
                    string[] date = hf_dateselected_deliverypickups.Value.Split('/');
                    lbl_currDate_deliverypickups.Text = (Convert.ToInt32(date[0])) + "/" + Convert.ToInt32(date[1]) +
                                                        "/" + Convert.ToInt32(date[2]);
                    string x =
                        new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[0]), Convert.ToInt32(date[1]))
                            .ToLongDateString();
                    int endIndex = x.IndexOf(',');
                    string xday = x.Substring(0, endIndex);
                    string xdate = x.Substring(endIndex + 1);
                    lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
                }
                catch
                {
                }
                hf_category_deliverypickups.Value = Request.QueryString["cat"];
                LoadSchedule(ref GV_Schedule_deliverypickups);
            }
        }
        hf_refreshTimer_deliverypickups.Value = "";
    }

    protected void hf_dateselected_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            string[] date = hf_dateselected_deliverypickups.Value.Split('/');
            string _d = date[0] + "/" + date[1] + "/" + date[2];

            DateTime _tempDate = new DateTime();
            if (!DateTime.TryParse(_d, out _tempDate))
            {
                _d = (Convert.ToInt32(date[0]) + 1).ToString() + "/1/" + date[2];
                date[0] = (Convert.ToInt32(date[0]) + 1).ToString();
                date[1] = "1";
            }

            lbl_currDate_deliverypickups.Text = _d;
            string x = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[0]), Convert.ToInt32(date[1])).ToLongDateString();
            int endIndex = x.IndexOf(',');
            string xday = x.Substring(0, endIndex);
            string xdate = x.Substring(endIndex + 1);
            lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
        }
        catch
        {
        }
        hf_category_deliverypickups.Value = Request.QueryString["cat"];
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void hf_category_ValueChanged(object sender, EventArgs e)
    {
        _pageSize = Convert.ToInt32(dd_display_deliverypickups.SelectedValue);
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void cb_viewslots_Changed(object sender, EventArgs e)
    {
        hf_viewTimes_deliverypickups.Value = cb_viewslots_deliverypickups.Checked ? "Filled" : "All";

        string[] date = hf_dateselected_deliverypickups.Value.Split('/');
        lbl_currDate_deliverypickups.Text = (Convert.ToInt32(date[0])) + "/" + Convert.ToInt32(date[1]) + "/" +
                                            Convert.ToInt32(date[2]);
        string x =
            new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[0]), Convert.ToInt32(date[1])).ToLongDateString();
        int endIndex = x.IndexOf(',');
        string xday = x.Substring(0, endIndex);
        string xdate = x.Substring(endIndex + 1);
        lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void btn_refresh_Click(object sender, EventArgs e)
    {
        try
        {
            string[] date = hf_dateselected_deliverypickups.Value.Split('/');
            lbl_currDate_deliverypickups.Text = (Convert.ToInt32(date[0])) + "/" + Convert.ToInt32(date[1]) + "/" +
                                                Convert.ToInt32(date[2]);
            string x =
                new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[0]), Convert.ToInt32(date[1]))
                    .ToLongDateString();
            int endIndex = x.IndexOf(',');
            string xday = x.Substring(0, endIndex);
            string xdate = x.Substring(endIndex + 1);
            lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
        }
        catch
        {
        }
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void Calendar1_Changed(object sender, EventArgs e)
    {
        try
        {
            lbl_currDate_deliverypickups.Text = (Calendar1_deliverypickups.SelectedDate.Month + "/" +
                                                 Calendar1_deliverypickups.SelectedDate.Day + "/" +
                                                 Calendar1_deliverypickups.SelectedDate.Year);
            string x =
                new DateTime(Calendar1_deliverypickups.SelectedDate.Year, Calendar1_deliverypickups.SelectedDate.Month,
                             Calendar1_deliverypickups.SelectedDate.Day).ToLongDateString();
            int endIndex = x.IndexOf(',');
            string xday = x.Substring(0, endIndex);
            string xdate = x.Substring(endIndex + 1);
            lbl_currDate2_deliverypickups.Text = xday + "<br />" + xdate;
            hf_dateselected_deliverypickups.Value = lbl_currDate_deliverypickups.Text;
            schedule_name_deliverypickups.Value = string.Empty;
            schedule_items_deliverypickups.Value = string.Empty;
            schedule_phone1_deliverypickups.Value = string.Empty;
            schedule_phone2_deliverypickups.Value = string.Empty;
            schedule_phone3_deliverypickups.Value = string.Empty;
            schedule_from_deliverypickups.Value = string.Empty;
            schedule_email_deliverypickups.Value = string.Empty;
            schedule_comment_deliverypickups.Value = string.Empty;
            schedule_trucknum_deliverypickups.Value = string.Empty;
            spd_deliverypickups.SelectedIndex = 0;
            cb_sendEmail_deliverypickups.Checked = false;
            disableControls();
            setCategoryIndex();
            initializeCookies();
            Step1_deliverypickups.Attributes["style"] = "display: block;";
            Step2_deliverypickups.Attributes["style"] = "display: none;";
            Step3_deliverypickups.Attributes["style"] = "display: none;";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.LoadModalWindow(false, 'NewDelPickup_element', '');", true);
        }
        catch
        {
        }
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    protected void imgbtn_search_Click(object sender, EventArgs e)
    {
        LoadSchedule(ref GV_Schedule_deliverypickups);
    }

    private void SearchTable()
    {
        _schedule = new Scheduler();
        _schedulerColl = new List<Scheduler_Coll>();
        foreach (var x in _schedule.scheduler_coll)
        {
            string findValue = tb_search_deliverypickups.Text.ToLower().Replace("\n", "");
            string evt_company = x.Company.Replace("\n", "");
            string evt_DeliveryFrom = x.DeliveryFrom.Replace("\n", "");
            string evt_Email = x.Email.Replace("\n", "");
            string evt_PhoneNumber = x.PhoneNumber.Replace("\n", "");
            Scheduler_Coll coll;
            if (evt_company.ToLower().Contains(findValue))
            {
                coll = new Scheduler_Coll(x.ID, x.Company, x.TruckNum, x.DeliveryFrom, x.Items, x.PhoneNumber, x.Email,
                                          x.ScheduleType, x.TimeScheduled, x.ScheduleDate, x.Comment, x.ConfirmationNum,
                                          x.Complete);
                updateSlots(coll);
            }
            else if (evt_DeliveryFrom.ToLower().Contains(findValue))
            {
                coll = new Scheduler_Coll(x.ID, x.Company, x.TruckNum, x.DeliveryFrom, x.Items, x.PhoneNumber, x.Email,
                                          x.ScheduleType, x.TimeScheduled, x.ScheduleDate, x.Comment, x.ConfirmationNum,
                                          x.Complete);
                updateSlots(coll);
            }
            else if (evt_Email.ToLower().Contains(findValue))
            {
                coll = new Scheduler_Coll(x.ID, x.Company, x.TruckNum, x.DeliveryFrom, x.Items, x.PhoneNumber, x.Email,
                                          x.ScheduleType, x.TimeScheduled, x.ScheduleDate, x.Comment, x.ConfirmationNum,
                                          x.Complete);
                updateSlots(coll);
            }
            else if (evt_PhoneNumber.ToLower().Contains(findValue))
            {
                coll = new Scheduler_Coll(x.ID, x.Company, x.TruckNum, x.DeliveryFrom, x.Items, x.PhoneNumber, x.Email,
                                          x.ScheduleType, x.TimeScheduled, x.ScheduleDate, x.Comment, x.ConfirmationNum,
                                          x.Complete);
                updateSlots(coll);
            }
        }
        hf_dateselected_deliverypickups.Value = DateTime.Now.Month - 1 + "/" + DateTime.Now.Day + "/" +
                                                DateTime.Now.Year;
        lbl_currDate_deliverypickups.Text = "N/A";
        hf_category_deliverypickups.Value = Request.QueryString["cat"];
    }

    private void updateSlots(Scheduler_Coll coll)
    {
        _schedulerColl.Add(coll);
    }

    #endregion

    #region Schedule Handler

    private readonly Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");
    private ConnectionStringSettings connString;

    protected void tb_Date1_TextChanged(object sender, EventArgs e)
    {
        dd_schTimeSlot_deliverypickups.Items.Clear();
        lbl_slotsopen_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Enabled = false;
        dd_schTimeSlot_deliverypickups.Visible = false;
        btn_selectTime_deliverypickups.Enabled = false;
        btn_selectTime_deliverypickups.Visible = false;
        lbl_slotsopen_deliverypickups.Enabled = false;
        lbl_slotsopen_deliverypickups.Visible = false;
        lbl_SelectedDate_deliverypickups.Enabled = false;
        lbl_SelectedDate_deliverypickups.Visible = false;
        btn_finish_deliverypickups.Enabled = false;
        btn_finish_deliverypickups.Visible = false;
        try
        {
            string[] temp = tb_Date1_deliverypickups.Text.Split('/');
            buildTimeSlot(Convert.ToInt32(temp[0]), Convert.ToInt32(temp[1]), Convert.ToInt32(temp[2]));
        }
        catch
        {
        }
        reInitializeModal();
        setCategoryIndex();
        initializeCookies();
        Step1_deliverypickups.Attributes["style"] = "display: none;";
        Step2_deliverypickups.Attributes["style"] = "display: block;";
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void lb_clearDate_Click(object sender, EventArgs e)
    {
        disableControls();
        reInitializeModal();
        setCategoryIndex();
        initializeCookies();
        Step1_deliverypickups.Attributes["style"] = "display: none;";
        Step2_deliverypickups.Attributes["style"] = "display: block;";
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void btn_selectTime_Click(object sender, EventArgs e)
    {
        try
        {
            lbl_SelectedDate_deliverypickups.Enabled = true;
            lbl_SelectedDate_deliverypickups.Visible = true;
            btn_finish_deliverypickups.Enabled = true;
            btn_finish_deliverypickups.Visible = true;
            lbl_SelectedDate_deliverypickups.Text = "<h3>You have selected a " + getSchType() + " for " +
                                                    Convert.ToDateTime(tb_Date1_deliverypickups.Text)
                                                           .Date.ToLongDateString() + " at " +
                                                    dd_schTimeSlot_deliverypickups.SelectedValue + "</h3>";
            btn_finish_deliverypickups.Text = "Schedule " + getSchType();
        }
        catch
        {
        }
        reInitializeModal();
        setCategoryIndex();
        initializeCookies();
        Step1_deliverypickups.Attributes["style"] = "display: none;";
        Step2_deliverypickups.Attributes["style"] = "display: block;";
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void btn_finish_Click(object sender, EventArgs e)
    {
        failMessage_deliverypickups.InnerHtml = string.Empty;
        if ((string.IsNullOrEmpty(schedule_name_deliverypickups.Value)) ||
            (string.IsNullOrEmpty(schedule_from_deliverypickups.Value)))
        {
            failMessage_deliverypickups.InnerHtml =
                "<span style='color: #FF0000;'>Please enter all the required fields</span>";
            reInitializeModal();
            setCategoryIndex();
            initializeCookies();
            Step1_deliverypickups.Attributes["style"] = "display: block;";
            Step2_deliverypickups.Attributes["style"] = "display: none;";
            Step3_deliverypickups.Attributes["style"] = "display: none;";
        }
        else
        {
            string date = tb_Date1_deliverypickups.Text + " " + dd_schTimeSlot_deliverypickups.SelectedValue;
            string phonenum = schedule_phone1_deliverypickups.Value + "-" + schedule_phone2_deliverypickups.Value + "-" +
                              schedule_phone3_deliverypickups.Value;
            string cn = ConfirmationNumber();
            string date2 = Convert.ToDateTime(tb_Date1_deliverypickups.Text).Date.ToShortDateString();
            if (cb_sendEmail_deliverypickups.Checked)
            {
                add_sendEmail(schedule_name_deliverypickups.Value, schedule_trucknum_deliverypickups.Value,
                              schedule_from_deliverypickups.Value, schedule_items_deliverypickups.Value, phonenum,
                              schedule_email_deliverypickups.Value, getSchType(), date,
                              schedule_comment_deliverypickups.Value, cn);
                displaySuccess(date2, cn);
            }
            else
            {
                _schedule.addItem(schedule_name_deliverypickups.Value, schedule_trucknum_deliverypickups.Value,
                                 schedule_from_deliverypickups.Value, schedule_items_deliverypickups.Value, phonenum,
                                 schedule_email_deliverypickups.Value, getSchType(), DateTime.Now.ToString(), date,
                                 schedule_comment_deliverypickups.Value, cn);
                displaySuccess(date2, cn);
            }
        }
    }

    private void displaySuccess(string date, string cn)
    {
        string success =
            "<span style='padding-top: 5px;'>Your appointment for " +
            getSchType() + " has been booked for " + date + " at " + dd_schTimeSlot_deliverypickups.SelectedValue +
            "<br />Confirmation number: <strong>" + cn + "</strong></span>";
        disableControls();
        CompleteSch_deliverypickups.InnerHtml = success;
        Step1_deliverypickups.Attributes["style"] = "display: none;";
        Step2_deliverypickups.Attributes["style"] = "display: none;";
        Step3_deliverypickups.Attributes["style"] = "display: block;";
        reInitializeModal();
    }

    protected void btn_newApp_Click(object sender, EventArgs e)
    {
        schedule_name_deliverypickups.Value = string.Empty;
        schedule_items_deliverypickups.Value = string.Empty;
        schedule_phone1_deliverypickups.Value = string.Empty;
        schedule_phone2_deliverypickups.Value = string.Empty;
        schedule_phone3_deliverypickups.Value = string.Empty;
        schedule_from_deliverypickups.Value = string.Empty;
        schedule_email_deliverypickups.Value = string.Empty;
        schedule_comment_deliverypickups.Value = string.Empty;
        schedule_trucknum_deliverypickups.Value = string.Empty;
        spd_deliverypickups.SelectedIndex = 0;
        cb_sendEmail_deliverypickups.Checked = false;
        disableControls();
        reInitializeModal();
        setCategoryIndex();
        initializeCookies();
        Step1_deliverypickups.Attributes["style"] = "display: block;";
        Step2_deliverypickups.Attributes["style"] = "display: none;";
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    private string getSchType()
    {
        string type = "Pickup";
        if (spd_deliverypickups.SelectedIndex == 0)
        {
            type = "Delivery";
        }

        return type;
    }

    private void disableControls()
    {
        tb_Date1_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Items.Clear();
        lbl_slotsopen_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Enabled = false;
        dd_schTimeSlot_deliverypickups.Visible = false;
        btn_selectTime_deliverypickups.Enabled = false;
        btn_selectTime_deliverypickups.Visible = false;
        lbl_slotsopen_deliverypickups.Enabled = false;
        lbl_slotsopen_deliverypickups.Visible = false;
        lbl_SelectedDate_deliverypickups.Enabled = false;
        lbl_SelectedDate_deliverypickups.Visible = false;
        btn_finish_deliverypickups.Enabled = false;
        btn_finish_deliverypickups.Visible = false;
        failMessage_deliverypickups.InnerHtml = string.Empty;
        CompleteSch_deliverypickups.InnerHtml = string.Empty;
    }

    private void reInitializeModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$('#NewDelPickup_element').show();", true);
    }

    protected void btn_modalClose_Click(object sender, EventArgs e)
    {
        schedule_name_deliverypickups.Value = string.Empty;
        schedule_items_deliverypickups.Value = string.Empty;
        schedule_phone1_deliverypickups.Value = string.Empty;
        schedule_phone2_deliverypickups.Value = string.Empty;
        schedule_phone3_deliverypickups.Value = string.Empty;
        schedule_from_deliverypickups.Value = string.Empty;
        schedule_email_deliverypickups.Value = string.Empty;
        schedule_comment_deliverypickups.Value = string.Empty;
        schedule_trucknum_deliverypickups.Value = string.Empty;
        spd_deliverypickups.SelectedIndex = 0;
        cb_sendEmail_deliverypickups.Checked = false;
        disableControls();
        setCategoryIndex();
        initializeCookies();
        Step1_deliverypickups.Attributes["style"] = "display: block;";
        Step2_deliverypickups.Attributes["style"] = "display: none;";
        Step3_deliverypickups.Attributes["style"] = "display: none;";
        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.LoadModalWindow(false, 'NewDelPickup_element', '');", true);
    }

    private string ConfirmationNumber()
    {
        string cn = "SM";
        cn += RandomNumber(100, 999);
        cn += HelperMethods.RandomString(3);
        return cn;
    }

    private string RandomNumber(int min, int max)
    {
        var random = new Random();
        return random.Next(min, max).ToString(CultureInfo.InvariantCulture);
    }

    private void add_sendEmail(string company, string trucknum, string Delfrom, string items, string phonenumber,
                               string email, string type, string schdate, string comment, string cn)
    {
        try
        {
            try
            {
                var message = new MailMessage();
                message.To.Add(email);

                string messagebody = string.Empty;
                messagebody += "<h3 style='color: #555;'><u>Confirmation for " + type +
                               "</u></h3><p>You have scheduled a " + type + " for " + schdate +
                               "<br />Confirmation Number: <strong>" + cn + "</strong></p><br />";
                messagebody +=
                    "<p> Attn Drivers:<br /> Please DO NOT arrive no more than 15-30 minutes before you scheduled appointment time. If you can not meet the appointment time and date please contact 816-581-6222 as they will be able to reschedule.<br /> <br /> Incoming shipments are checked againsted ASN's.<br /> We ask that you do not block out more times then needed.<br /> <br /> Thank you. </p>";
                messagebody +=
                    "<br /><span style='color: #777777; font-size: 10px;'><strong style='padding-right: 3px;'>Note:</strong>";
                messagebody += "Please contact the sales department if you have any questions or concerns regaring this email.<br />Please do not reply to this message.";

                ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Delivery/Pickup Confirmation</h1>", "Steel-MFG: Confirmation Email for " + type, messagebody);
                _schedule.addItem(company, trucknum, Delfrom, items, phonenumber, email, type, DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                 schdate, comment, cn);
            }
            catch (Exception ex) {
                _applog.AddError(ex);
            }
        }
        catch (Exception ex) {
            _applog.AddError(ex);
        }
    }

    private void buildTimeSlot(int m, int d, int y)
    {
        string tempDate = m.ToString(CultureInfo.InvariantCulture) + "/" + d.ToString(CultureInfo.InvariantCulture) + "/" + y.ToString(CultureInfo.InvariantCulture);
        DateTime checkWeekends = Convert.ToDateTime(tempDate);
        if ((((m == DateTime.Now.Month) && (d >= DateTime.Now.Day) && (y >= DateTime.Now.Year))
             || ((m > DateTime.Now.Month) && (y >= DateTime.Now.Year)))
            && ((checkWeekends.DayOfWeek != DayOfWeek.Saturday) && (checkWeekends.DayOfWeek != DayOfWeek.Sunday)))
        {
            int openSpots = 0;
            int hour = 6;
            int hl = 6;
            int min = 30;
            int minLong = 30;
            string pmam = "AM";
            while (hl <= 22)
            {
                if (hour >= 12)
                {
                    pmam = "PM";
                }
                if (hour >= 13)
                {
                    hour = hour - 12;
                }
                string tempmin = min == 0 ? "00" : min.ToString(CultureInfo.InvariantCulture);
                if (minLong == 60)
                {
                    minLong = 0;
                }

                string date = m.ToString(CultureInfo.InvariantCulture) + "/" + d.ToString(CultureInfo.InvariantCulture) + "/" + y.ToString(CultureInfo.InvariantCulture) + " " + hour.ToString(CultureInfo.InvariantCulture) + ":" +
                              tempmin + " " + pmam;
                if (checkTimeSlot(date))
                {
                    if ((m == DateTime.Now.Month) && (d == DateTime.Now.Day) && (y == DateTime.Now.Year))
                    {
                        if ((hl == DateTime.Now.Hour) && (minLong > DateTime.Now.Minute))
                        {
                            var item = new ListItem(hour + ":" + tempmin + " " + pmam, hour + ":" + tempmin + " " + pmam);
                            dd_schTimeSlot_deliverypickups.Items.Add(item);
                            openSpots++;
                        }
                        else
                        {
                            if (hl > DateTime.Now.Hour)
                            {
                                var item = new ListItem(hour + ":" + tempmin + " " + pmam,
                                                        hour + ":" + tempmin + " " + pmam);
                                dd_schTimeSlot_deliverypickups.Items.Add(item);
                                openSpots++;
                            }
                        }
                    }
                    else
                    {
                        var item = new ListItem(hour + ":" + tempmin + " " + pmam, hour + ":" + tempmin + " " + pmam);
                        dd_schTimeSlot_deliverypickups.Items.Add(item);
                        openSpots++;
                    }
                }
                if ((hour + ":" + tempmin + " " + pmam) == "10:00 PM")
                {
                    break;
                }
                else
                {
                    min = min + 15;
                    minLong = minLong + 15;
                    if (min == 60)
                    {
                        min = 0;
                        hour = hour + 1;
                        hl = hl + 1;
                        if (minLong != 60)
                        {
                            minLong = 60;
                        }
                    }
                }
            }
            lbl_slotsopen_deliverypickups.Text = "<i>There are currently " + openSpots +
                                                 " time slots open (Max: 63)</i>";
            dd_schTimeSlot_deliverypickups.Enabled = true;
            dd_schTimeSlot_deliverypickups.Visible = true;
            btn_selectTime_deliverypickups.Enabled = true;
            btn_selectTime_deliverypickups.Visible = true;
            lbl_slotsopen_deliverypickups.Enabled = true;
            lbl_slotsopen_deliverypickups.Visible = true;
        }
        else
        {
            lbl_slotsopen_deliverypickups.Text = "<i>The selected date is not a valid chose</i>";
            lbl_slotsopen_deliverypickups.Enabled = true;
            lbl_slotsopen_deliverypickups.Visible = true;
        }
    }

    private bool checkTimeSlot(string date)
    {
        connString = rootWebConfig.ConnectionStrings.ConnectionStrings["ApplicationServices"];
        var connection = new SqlConnection(connString.ConnectionString);
        bool canContinue = true;
        try
        {
            connection.Open();
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(
                "SELECT * FROM Scheduler WHERE ScheduleDate='" + date + "'", connection))
            {
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    canContinue = false;
                    break;
                }
            }
            myReader.Close();
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        return canContinue;
    }

    #endregion
}