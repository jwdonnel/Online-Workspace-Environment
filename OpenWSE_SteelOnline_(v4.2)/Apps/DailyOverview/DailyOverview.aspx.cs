#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Apps;
using OpenWSE_Tools.AutoUpdates;

#endregion

public partial class Apps_DailyOverview_DailyOverview : Page
{
    #region private variables

    private const string app_id = "app-dailyoverview";
    private readonly IPWatch ipwatch = new IPWatch(true);
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly App apps = new App();
    private string ctrlname;
    private MemberDatabase member;
    private TruckSchedule truckschedule;
    private string _sitetheme;
    private AppInitializer _appInitializer;

    #endregion

    #region PageLoading methods

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent)
        {
            member = _appInitializer.memberDatabase;
            _sitetheme = _appInitializer.siteTheme;

            ScriptManager sm = ScriptManager.GetCurrent(Page);
            string ctlID = sm.AsyncPostBackSourceElementID;
            ctrlname = ctlID;

            if (!IsPostBack)
            {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, app_id, this);
                aus.StartAutoUpdates();

                lbl_current.Text = Calendar1_dailyoverview.SelectedDate.DayOfWeek.ToString() + ", " +
                                   MonthConverter.ToStringMonth(Calendar1_dailyoverview.SelectedDate.Month) +
                                   " " + Calendar1_dailyoverview.SelectedDate.Day + " " +
                                   Calendar1_dailyoverview.SelectedDate.Year;
                Session["Calendar1_dailyoverview"] = DateTime.Now.ToShortDateString();
                Session["viewmode_dailyoverview"] = "0";

                LoadAutoScrollParams(userId.Name);
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void LoadAutoScrollParams(string currUser)
    {
        string[] delim = { "=" };
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp(app_id);
        Dictionary<string, string> dicParams = new Dictionary<string, string>();
        foreach (Dictionary<string, string> dr in appParams.listdt)
        {
            string[] paramSplit = dr["Parameter"].Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (paramSplit.Length == 2)
            {
                string key = paramSplit[0];
                string val = paramSplit[1];
                if (!dicParams.ContainsKey(key))
                    dicParams.Add(key, val);
            }
        }

        bool canScroll = false;
        if (dicParams.ContainsKey("AutoScrollUser"))
        {
            string users = string.Empty;
            dicParams.TryGetValue("AutoScrollUser", out users);
            string[] userList = users.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string u in userList)
            {
                if (u.ToLower() == currUser.ToLower())
                {
                    canScroll = true;
                    break;
                }
            }
        }

        if (canScroll) {
            if (dicParams.ContainsKey("AutoScrollOn")) {
                string autoScrollOn = "";
                dicParams.TryGetValue("AutoScrollOn", out autoScrollOn);
                if ((!string.IsNullOrEmpty(autoScrollOn)) && (HelperMethods.ConvertBitToBoolean(autoScrollOn))) {
                    string incrScroll = "1";
                    string incrTime = "75";
                    dicParams.TryGetValue("incrScroll", out incrScroll);
                    dicParams.TryGetValue("incrTime", out incrTime);
                    if ((!string.IsNullOrEmpty(incrScroll)) && (!string.IsNullOrEmpty(incrTime)))
                        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$('#btn_pauseScrollPage').show();var incrScroll_temp=" + incrScroll + ";var incrScroll=" + incrScroll + ";var incrTime=" + incrTime + ";pageScroll();", true);
                }
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#btn_pauseScrollPage').hide();");
        }
    }

    // Auto Update

    protected void updb(object sender, EventArgs e)
    {
        truckschedule = new TruckSchedule();
        LoadWeightTotals();
        if ((Calendar1_dailyoverview.SelectedDate.Year == 1) || (Session["Calendar1_dailyoverview"] == null) || (!IsPostBack))
        {
            Calendar1_dailyoverview.VisibleDate = DateTime.Now.Date;
            Calendar1_dailyoverview.SelectedDate = DateTime.Now.Date;
            Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.SelectedDate.ToShortDateString();
            Session["viewmode_dailyoverview"] = "0";
            StartPage();
            InitializeParamSort();
        }
        else
        {
            CreateMultiSortControls();
            MultiSortCol();
        }
        updatepnl_dates_dailyoverview.Update();
        updatepnl_schDriver_gridview_dailyoverview.Update();
    }

    private void StartPage()
    {
        CreateMultiSortControls();
        try
        {
            if (Session["viewmode_dailyoverview"].ToString() == "0")
            {
                DataTable dataTable = truckschedule.GetDailyOverview(Session["Calendar1_dailyoverview"].ToString());

                string lastExpresion = ViewState["SortExpression"] as string;
                string lastDirection = ViewState["SortDirection"] as string;

                if ((!string.IsNullOrEmpty(lastDirection)) && (!string.IsNullOrEmpty(lastExpresion)))
                    dataTable.DefaultView.Sort = lastExpresion + " " + lastDirection;
                else
                    MultiSortCol();

                Session["TaskTable_dailyoverview"] = dataTable;
                GV_dbimport_dailyoverview.DataSource = dataTable;
                GV_dbimport_dailyoverview.DataBind();

                lbl_current.Text = Calendar1_dailyoverview.SelectedDate.DayOfWeek.ToString() + ", " +
                   MonthConverter.ToStringMonth(Calendar1_dailyoverview.SelectedDate.Month) + " " +
                   Calendar1_dailyoverview.SelectedDate.Day + " " + Calendar1_dailyoverview.SelectedDate.Year;
            }
            else
            {
                DataTable dataTable = truckschedule.GetDailyOverview_Month(Calendar1_dailyoverview.VisibleDate.Month + "/1/" + Calendar1_dailyoverview.VisibleDate.Year);

                string lastExpresion = ViewState["SortExpression"] as string;
                string lastDirection = ViewState["SortDirection"] as string;

                if ((!string.IsNullOrEmpty(lastDirection)) && (!string.IsNullOrEmpty(lastExpresion)))
                    dataTable.DefaultView.Sort = lastExpresion + " " + lastDirection;
                else
                    MultiSortCol();

                Session["TaskTable_dailyoverview"] = dataTable;
                GV_dbimport_dailyoverview.DataSource = dataTable;
                GV_dbimport_dailyoverview.DataBind();

                lbl_current.Text = "Month of " + MonthConverter.ToStringMonth(Calendar1_dailyoverview.VisibleDate.Month);
            }
        }
        catch
        {
        }
    }

    private void LoadWeightTotals()
    {
        try
        {
            string curr_viewmode = Session["viewmode_dailyoverview"].ToString();
            int totalweight = 0;
            pnl_overviewholder_dailyoverview.Controls.Clear();
            var ts = new TruckSchedule();
            var str = new StringBuilder();
            var gd = new GeneralDirection(true);
            string calDailyOverview = Session["Calendar1_dailyoverview"].ToString();
            foreach (Dictionary<string, string> row in gd.generaldirection)
            {
                if (curr_viewmode == "0")
                {
                    str.Append("<h4 class='pad-right float-left font-bold'>" + row["GeneralDirection"] +
                               ":</h4><h4 class='float-left'>" +
                               (ts.calTotalWeightGD(calDailyOverview,
                                                    row["GeneralDirection"].ToString()).ToString("#,##0")) +
                               " lbs</h4><div class='clear'></div>");
                    totalweight +=
                        Convert.ToInt32(ts.calTotalWeightGD(calDailyOverview,
                                                            row["GeneralDirection"].ToString()));
                }
                else
                {
                    str.Append("<h4 class='pad-right float-left font-bold'>" + row["GeneralDirection"] +
                               ":</h4><h4 class='float-left'>" +
                               (ts.calTotalWeightGD_Month(calDailyOverview,
                                                          row["GeneralDirection"].ToString()).ToString("#,##0")) +
                               " lbs</h4><div class='clear'></div>");
                    totalweight +=
                        Convert.ToInt32(ts.calTotalWeightGD_Month(calDailyOverview,
                                                                  row["GeneralDirection"].ToString()));
                }
            }
            if (curr_viewmode == "0")
            {
                str.Append(
                    "<div class='clear' style='width: 50%; margin: 8px 0; border-top: 1px solid #353535;'></div><h3 style='color: #353535;'>&nbsp;<b>Total:&nbsp;&nbsp;</b>" +
                    totalweight.ToString("#,##0") + " lbs</h3>");
            }
            else
            {
                str.Append(
                    "<div class='clear' style='width: 50%; margin: 8px 0; border-top: 1px solid #353535;'></div><h3 style='color: #353535;'>&nbsp;<b>Total:&nbsp;&nbsp;</b>" +
                    totalweight.ToString("#,##0") + " lbs (Entire Month)</h3>");
            }
            str.Append("<small>(Total = SMW Trucks + Common Carriers)</small>");

            pnl_overviewholder_dailyoverview.Controls.Add(new LiteralControl(str.ToString()));
        }
        catch
        {
        }
    }

    private void CreateMultiSortControls()
    {
        if ((pnl_multisort_dailyoverview.Controls.Count == 1) || (pnl_multisort_dailyoverview.Controls.Count == 0))
        {
            pnl_multisort_dailyoverview.Controls.Add(
                new LiteralControl("<span class='font-bold font-color-light-black pad-right pad-left'>Sort By:</span>"));
            var dt_multisort = Session["TaskTable_dailyoverview"] as DataTable;

            char[] trimBoth = {' '};
            string findValue = tb_search_dailyoverview.Text.ToLower().TrimEnd(trimBoth);
            findValue = findValue.TrimStart(trimBoth);
            if ((!string.IsNullOrEmpty(findValue)) && (findValue != "search for truck schedule"))
            {
                var ts = (from x in truckschedule.scheduler_coll
                          let coll = new TruckSchedule_Coll(x.ID, x.DriverName.Replace(" ", "_"), x.TruckLine, x.Date, x.Unit, x.CustomerName, x.City, x.OrderNumber, x.Sequence, x.GeneralDirection, x.Weight, x.AdditionalInfo, x.LastUpdated)
                          where (x.City.ToLower().Contains(findValue)) || (x.CustomerName.ToLower().Contains(findValue)) || (x.Date.ToLower().Contains(findValue)) || (x.DriverName.Replace("_", " ").ToLower().Contains(findValue)) || (x.GeneralDirection.ToLower().Contains(findValue)) || (x.DriverName.ToLower().Contains(findValue)) || (x.Unit.ToLower().Contains(findValue)) || (x.Weight.ToLower().Contains(findValue)) || (x.OrderNumber.ToLower().Contains(findValue))
                          select coll).ToList();
                DataTable dataTable = CreateSearchResults(ts);
                dt_multisort = dataTable;
            }

            try
            {
                if (dt_multisort != null)
                    foreach (var cbMultiSort in from DataColumn c in dt_multisort.Columns select new CheckBox
                        {
                            ID = "CheckBox_" + c.ColumnName + "_MultiSort_dailyoverview",
                            CssClass = "pad-right-big margin-right",
                            Text = "&nbsp;" + c.ColumnName
                        })
                    {
                        pnl_multisort_dailyoverview.Controls.Add(cbMultiSort);
                    }
            }
            catch
            {
            }

            var ddlMultiSort = new DropDownList
                {
                    ID = "dd_MultiSort_dailyoverview",
                    Width = 75,
                    CssClass = "margin-right-big margin-top-sml"
                };
            ddlMultiSort.Items.Add(new ListItem("ASC", "ASC"));
            ddlMultiSort.Items.Add(new ListItem("DESC", "DESC"));
            pnl_multisort_dailyoverview.Controls.Add(ddlMultiSort);
            var lbtnMultisort = new Button
                {
                    ID = "btn_MultiSort_dailyoverview",
                    Text = "Update Sort",
                    CssClass = "input-buttons dailyoverview-update-img"
                };
            lbtnMultisort.Click += lbtn_multisort_Click;
            ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(lbtnMultisort);
            pnl_multisort_dailyoverview.Controls.Add(lbtnMultisort);
        }
    }

    protected void lbtn_multisort_Click(object sender, EventArgs e)
    {
        ViewState["SortExpression"] = string.Empty;
        MultiSortCol();
    }

    private void MultiSortCol()
    {
        if (!AlreadySorted())
        {
            var dt = Session["TaskTable_dailyoverview"] as DataTable;

            char[] trimBoth = { ' ' };
            string findValue = tb_search_dailyoverview.Text.ToLower().TrimEnd(trimBoth);
            findValue = findValue.TrimStart(trimBoth);
            if ((!string.IsNullOrEmpty(findValue)) && (findValue != "search for truck schedule"))
            {
                var ts = (from x in truckschedule.scheduler_coll
                          let coll = new TruckSchedule_Coll(x.ID, x.DriverName.Replace(" ", "_"), x.TruckLine, x.Date, x.Unit, x.CustomerName, x.City, x.OrderNumber, x.Sequence, x.GeneralDirection, x.Weight, x.AdditionalInfo, x.LastUpdated)
                          where (x.City.ToLower().Contains(findValue)) || (x.CustomerName.ToLower().Contains(findValue)) || (x.Date.ToLower().Contains(findValue)) || (x.DriverName.Replace("_", " ").ToLower().Contains(findValue)) || (x.GeneralDirection.ToLower().Contains(findValue)) || (x.DriverName.ToLower().Contains(findValue)) || (x.Unit.ToLower().Contains(findValue)) || (x.Weight.ToLower().Contains(findValue)) || (x.OrderNumber.ToLower().Contains(findValue))
                          select coll).ToList();
                DataTable dataTable = CreateSearchResults(ts);
                dt = dataTable;
            }

            if (dt != null)
            {
                try
                {
                    var dd_MultiSort = (DropDownList)pnl_multisort_dailyoverview.FindControl("dd_MultiSort_dailyoverview");
                    string sortdir = dd_MultiSort.SelectedValue;
                    var str = new StringBuilder();
                    foreach (Control c in pnl_multisort_dailyoverview.Controls)
                    {
                        if (c is CheckBox)
                        {
                            var cb = c as CheckBox;
                            if (cb.Checked)
                            {
                                string text = cb.Text.Replace("&nbsp;", "").Trim();
                                if (!string.IsNullOrEmpty(str.ToString()))
                                {
                                    str.Append(", " + text + " " + sortdir);
                                }
                                else
                                {
                                    str.Append(text + " " + sortdir);
                                }
                            }
                        }
                        else if (c is DropDownList)
                        {
                            var dd = c as DropDownList;
                            dd.SelectedIndex = sortdir.ToLower() == "asc" ? 0 : 1;
                        }
                    }

                    if (!string.IsNullOrEmpty(str.ToString()))
                    {
                        SortColumns(dt, str);
                    }
                    else
                    {
                        InitializeParamSort();
                    }
                }
                catch
                {
                }
            }
        }
    }

    private void InitializeParamSort()
    {
        var dt = Session["TaskTable_dailyoverview"] as DataTable;
        if (dt != null)
        {
            var str = new StringBuilder();
            string sortBy = truckschedule.GetSortParamas;
            string _sortDirParam = "asc";
            if (sortBy.ToLower().Contains("desc"))
            {
                _sortDirParam = "desc";
            }
            sortBy = sortBy.Replace(_sortDirParam, "").Trim();
            foreach (Control c in pnl_multisort_dailyoverview.Controls)
            {
                if (c is CheckBox)
                {
                    var cb = c as CheckBox;
                    string text = cb.Text.Replace("&nbsp;", "").Trim();
                    if (sortBy.Contains(text))
                    {
                        cb.Checked = true;
                        if (!string.IsNullOrEmpty(str.ToString()))
                        {
                            str.Append(", " + text + " " + _sortDirParam);
                        }
                        else
                        {
                            str.Append(text + " " + _sortDirParam);
                        }
                    }
                    else
                        cb.Checked = false;
                }
                else if (c is DropDownList)
                {
                    var dd = c as DropDownList;
                    if (_sortDirParam.ToLower() == "asc")
                    {
                        dd.SelectedIndex = 0;
                    }
                    else
                    {
                        dd.SelectedIndex = 1;
                    }
                }
            }

            if (!string.IsNullOrEmpty(str.ToString()))
            {
                SortColumns(dt, str);
            }
        }
    }

    private void SortColumns(DataTable dt, StringBuilder str)
    {
        dt.DefaultView.Sort = str.ToString();
        GV_dbimport_dailyoverview.DataSource = dt;
        GV_dbimport_dailyoverview.DataBind();
        foreach (Control c in GV_dbimport_dailyoverview.HeaderRow.Controls)
        {
            if (c is DataControlFieldHeaderCell)
            {
                foreach (Control d in c.Controls)
                {
                    if (d is Image)
                    {
                        var img = d as Image;
                        img.ImageUrl = "~/App_Themes/" + _sitetheme + "/Images/transparent_image.png";
                    }
                }
            }
        }
    }

    private bool AlreadySorted()
    {
        foreach (Control c in pnl_multisort_dailyoverview.Controls)
        {
            if (c is CheckBox)
            {
                var cb = c as CheckBox;
                if (cb.Checked)
                {
                    return false;
                }
            }
        }

        return true;
    }

    #region Auto Update System

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e)
    {
        bool cancontinue = false;
        if (!string.IsNullOrEmpty(hf_UpdateAll.Value))
        {
            string id = uuf.getFlag_AppID(hf_UpdateAll.Value);
            if (id == app_id)
            {
                uuf.deleteFlag(hf_UpdateAll.Value);
                cancontinue = true;
            }
        }

        if (cancontinue)
        {
            truckschedule = new TruckSchedule();
            LoadWeightTotals();
            if ((Calendar1_dailyoverview.SelectedDate.Year == 1) || (Session["Calendar1_dailyoverview"] != null))
            {
                if (Session["Calendar1_dailyoverview"] == null)
                    Calendar1_dailyoverview.SelectedDate = DateTime.Now.Date;
                else
                {
                    DateTime outDate = DateTime.Now;
                    string dateSelected = Session["Calendar1_dailyoverview"].ToString();
                    DateTime.TryParse(dateSelected, out outDate);
                    Calendar1_dailyoverview.SelectedDate = outDate.Date;

                }
                Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.SelectedDate.ToShortDateString();
            }
            StartPage();
            updatepnl_dates_dailyoverview.Update();
            updatepnl_schDriver_gridview_dailyoverview.Update();
        }
        hf_UpdateAll.Value = "";
    }

    #endregion

    #endregion

    #region GridView Properties Methods

    protected void dd_display_dailyoverview_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (dd_display_dailyoverview.SelectedValue == "10")
        {
            setPageSize(10);
        }
        else if (dd_display_dailyoverview.SelectedValue == "20")
        {
            setPageSize(20);
        }
        else if (dd_display_dailyoverview.SelectedValue == "30")
        {
            setPageSize(30);
        }
        else if (dd_display_dailyoverview.SelectedValue == "40")
        {
            setPageSize(40);
        }
        else if (dd_display_dailyoverview.SelectedValue == "1")
        {
            setPageSize(1);
        }
        LoadWeightTotals();
        StartPage();
        MultiSortCol();
    }

    protected void btn_showallmonth_dailyoverview_Click(object sender, EventArgs e)
    {
        Session["viewmode_dailyoverview"] = "1";
        Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.VisibleDate.ToShortDateString();
        LoadWeightTotals();
        StartPage();
    }

    protected void GV_dbimport_dailyoverview_Sorting(object sender, GridViewSortEventArgs e)
    {
        var dt = Session["TaskTable_dailyoverview"] as DataTable;
        if (dt != null)
        {
            dt.DefaultView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            GV_dbimport_dailyoverview.DataSource = dt;
            GV_dbimport_dailyoverview.DataBind();

            foreach (Control c in pnl_multisort_dailyoverview.Controls)
            {
                if (c is CheckBox)
                {
                    var cb = c as CheckBox;
                    if (cb.Checked)
                    {
                        cb.Checked = false;
                    }
                }
            }
        }
    }

    protected void GV_dbimport_dailyoverview_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        StartPage();
        GV_dbimport_dailyoverview.PageIndex = e.NewPageIndex;
        GV_dbimport_dailyoverview.DataBind();
        MultiSortCol();
    }

    protected void GV_dbimport_dailyoverview_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            try
            {
                int count = 0;
                var sortExpression = ViewState["SortExpression"] as string;
                foreach (DataControlFieldCell tc in e.Row.Cells)
                {
                    tc.CssClass = "td-sort-click";
                    tc.ToolTip = "Sort by " + tc.ContainingField.HeaderText;
                    if (tc.ContainingField.HeaderText == sortExpression)
                        AddSortImage(count, e.Row);

                    count++;
                }
            }
            catch { }
        }
    }

    protected void GV_dbimport_dailyoverview_OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            foreach (DataControlFieldCell tc in e.Row.Cells)
            {
                string text = tc.Text.Replace(" 12:00:00 AM", "");
                tc.Text = text;
            }
        }
    }

    #endregion

    #region GridView Sorting methods

    private void setPageSize(int size)
    {
        switch (size)
        {
            case 10:
                GV_dbimport_dailyoverview.PageSize = 10;
                dd_display_dailyoverview.SelectedIndex = 0;
                break;
            case 20:
                GV_dbimport_dailyoverview.PageSize = 20;
                dd_display_dailyoverview.SelectedIndex = 1;
                break;
            case 30:
                GV_dbimport_dailyoverview.PageSize = 30;
                dd_display_dailyoverview.SelectedIndex = 2;
                break;
            case 40:
                GV_dbimport_dailyoverview.PageSize = 40;
                dd_display_dailyoverview.SelectedIndex = 3;
                break;
            default:
                GV_dbimport_dailyoverview.PageSize = 1000000;
                dd_display_dailyoverview.SelectedIndex = 4;
                break;
        }
    }

    private void AddSortImage(int columnIndex, GridViewRow headerRow)
    {
        try
        {
            string className = "";
            var lastDirection = ViewState["SortDirection"] as string;
            if ((lastDirection == "ASC") || (lastDirection == null))
                className = " active asc";
            else
                className = " active desc";

            headerRow.Cells[columnIndex].CssClass += className;
        }
        catch { }
    }

    private string GetSortDirection(string column)
    {
        string sortDirection = "DESC";
        var sortExpression = ViewState["SortExpression"] as string;
        if (sortExpression != null)
        {
            if (sortExpression == column)
            {
                var lastDirection = ViewState["SortDirection"] as string;
                if ((lastDirection != null) && (lastDirection == "DESC"))
                {
                    sortDirection = "ASC";
                }
            }
        }
        ViewState["SortDirection"] = sortDirection;
        ViewState["SortExpression"] = column;
        return sortDirection;
    }

    #endregion

    protected void Calendar1_Changed(object sender, EventArgs e)
    {
        Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.SelectedDate.ToShortDateString();
        truckschedule = new TruckSchedule();
        LoadWeightTotals();
        if ((Calendar1_dailyoverview.SelectedDate.Year == 1) ||
            (string.IsNullOrEmpty(Session["Calendar1_dailyoverview"].ToString())))
        {
            Calendar1_dailyoverview.SelectedDate = DateTime.Now.Date;
            Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.SelectedDate.ToShortDateString();
        }
        lbl_current.Text = Calendar1_dailyoverview.SelectedDate.DayOfWeek.ToString() + ", " +
                           MonthConverter.ToStringMonth(Calendar1_dailyoverview.SelectedDate.Month) + " " +
                           Calendar1_dailyoverview.SelectedDate.Day + " " + Calendar1_dailyoverview.SelectedDate.Year;
        Session["viewmode_dailyoverview"] = "0";
        StartPage();
        updatepnl_dates_dailyoverview.Update();
    }

    protected void Calendar1_Month_Changed(object sender, MonthChangedEventArgs e)
    {
        string buildDate = Calendar1_dailyoverview.VisibleDate.Month.ToString() + "/";
        if (Calendar1_dailyoverview.SelectedDate.Month == Calendar1_dailyoverview.VisibleDate.Month)
            buildDate += Calendar1_dailyoverview.SelectedDate.Day.ToString() + "/";
        else
            buildDate += Calendar1_dailyoverview.VisibleDate.Day.ToString() + "/";

        if (Calendar1_dailyoverview.SelectedDate.Year == Calendar1_dailyoverview.VisibleDate.Year)
            buildDate += Calendar1_dailyoverview.SelectedDate.Year.ToString();
        else
            buildDate += Calendar1_dailyoverview.VisibleDate.Year.ToString();


        Session["Calendar1_dailyoverview"] = buildDate;
        truckschedule = new TruckSchedule();
        if ((Calendar1_dailyoverview.SelectedDate.Year == 1) ||
            (string.IsNullOrEmpty(Session["Calendar1_dailyoverview"].ToString())))
        {
            Calendar1_dailyoverview.SelectedDate = DateTime.Now.Date;
            Session["Calendar1_dailyoverview"] = Calendar1_dailyoverview.VisibleDate.ToShortDateString();
        }
        Session["viewmode_dailyoverview"] = "0";
        LoadWeightTotals();
        StartPage();
        updatepnl_dates_dailyoverview.Update();
    }

    protected void imgbtn_search_dailyoverview_click(object sender, EventArgs e)
    {
        var ts = new List<TruckSchedule_Coll>();
        foreach (var x in truckschedule.scheduler_coll)
        {
            char[] trimBoth = {' '};
            string findValue = tb_search_dailyoverview.Text.ToLower().TrimEnd(trimBoth);
            findValue = findValue.TrimStart(trimBoth);
            var coll = new TruckSchedule_Coll(x.ID, x.DriverName.Replace(" ", "_"), x.TruckLine, x.Date, x.Unit,
                                              x.CustomerName, x.City, x.OrderNumber, x.Sequence, x.GeneralDirection,
                                              x.Weight, x.AdditionalInfo, x.LastUpdated);
            if ((x.City.ToLower().Contains(findValue)) || (x.CustomerName.ToLower().Contains(findValue)) ||
                (x.Date.ToLower().Contains(findValue))
                || (x.DriverName.Replace("_", " ").ToLower().Contains(findValue)) ||
                (x.GeneralDirection.ToLower().Contains(findValue))
                || (x.DriverName.ToLower().Contains(findValue)) || (x.Unit.ToLower().Contains(findValue))
                || (x.Weight.ToLower().Contains(findValue)) || (x.OrderNumber.ToLower().Contains(findValue)))
            {
                ts.Add(coll);
            }
        }
        setPageSize(1);
        DataTable dataTable = CreateSearchResults(ts);
        Session["TaskTable_dailyoverview"] = dataTable;
        GV_dbimport_dailyoverview.DataSource = dataTable;
        GV_dbimport_dailyoverview.DataBind();
        MultiSortCol();
    }

    private DataTable CreateSearchResults(IEnumerable<TruckSchedule_Coll> ts)
    {
        var dataTable = new DataTable();
        var dt = new DataTable();
        dt.Columns.Add(new DataColumn("Date"));
        dt.Columns.Add(new DataColumn("TruckLine"));
        dt.Columns.Add(new DataColumn("DriverName"));
        dt.Columns.Add(new DataColumn("Stop"));
        dt.Columns.Add(new DataColumn("Unit"));
        dt.Columns.Add(new DataColumn("CustomerName"));
        dt.Columns.Add(new DataColumn("City"));
        dt.Columns.Add(new DataColumn("OrderNumber"));
        dt.Columns.Add(new DataColumn("GeneralDirection"));
        dt.Columns.Add(new DataColumn("Weight"));

        foreach (var t in ts)
        {
            DataRow drts = dt.NewRow();
            int weight = 0;

            string da = t.Date;
            string tl = t.TruckLine;
            string dn = t.DriverName;
            string st = t.Sequence.ToString();
            string un = t.Unit;
            string cn = t.CustomerName;
            string c = t.City;
            string on = t.OrderNumber;
            string gd = t.GeneralDirection;
            int.TryParse(t.Weight, out weight);

            #region set initial values

            var _da = new DateTime();
            if ((string.IsNullOrEmpty(da)) || (!DateTime.TryParse(da, out _da)))
            {
                da = DateTime.Now.ToShortDateString();
            }
            if (string.IsNullOrEmpty(tl))
            {
                tl = "-";
            }
            if (string.IsNullOrEmpty(dn))
            {
                dn = "-";
            }
            if (string.IsNullOrEmpty(st))
            {
                st = "-";
            }
            if (string.IsNullOrEmpty(un))
            {
                un = "-";
            }
            if (string.IsNullOrEmpty(cn))
            {
                cn = "-";
            }
            if (string.IsNullOrEmpty(c))
            {
                c = "-";
            }
            if (string.IsNullOrEmpty(on))
            {
                on = "-";
            }
            if (string.IsNullOrEmpty(gd))
            {
                gd = "-";
            }

            #endregion

            drts["Date"] = da;
            drts["TruckLine"] = tl.Replace("_", " ");
            drts["DriverName"] = dn.Replace("_", " ");
            drts["Stop"] = st;
            drts["Unit"] = un;
            drts["CustomerName"] = cn.Replace("_", " ");
            drts["City"] = c.Replace("_", " ");
            drts["OrderNumber"] = on;
            drts["GeneralDirection"] = gd.Replace("_", " ");
            drts["Weight"] = weight.ToString("#,##0");
            dt.Rows.Add(drts);
        }

        dataTable = dt;
        var dv = new DataView(dataTable);
        dv.Sort = string.Format("{0} {1}", dv.Table.Columns[0], "asc");
        dataTable = dv.ToTable();

        return dataTable;
    }

    private string ParseHiddenField(string hf)
    {
        string parsed = hf;
        try
        {
            int i = hf.IndexOf("|~|");
            parsed = hf.Substring(0, i);
        }
        catch
        {
        }

        return parsed;
    }
}