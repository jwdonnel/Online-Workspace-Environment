#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_CommonCarriers : Page {
    #region private variables

    private const string AppId = "app-commoncarriers";
    private readonly IPWatch _ipwatch = new IPWatch(true);
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private AppInitializer _appInitializer;
    private string _hfSortcolprev;
    private MemberDatabase _member;
    private int _pageSize;
    private GridViewRow _row;
    private TruckSchedule _truckschedule;
    private string _username;
    private string _sitetheme;

    #endregion


    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(AppId, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            _username = _appInitializer.UserName;
            _member = _appInitializer.memberDatabase;
            _sitetheme = _appInitializer.siteTheme;

            _truckschedule = new TruckSchedule(true);
            if (!IsPostBack) {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                hf_dateselected_steeltrucks.Value = DateTime.Now.Month + "_" + DateTime.Now.Year;
                ViewState["sortOrder"] = "desc";
                SortOrder = "desc";
                hf_sortcol_steeltrucks.Value = "0";
                loadTruckSchedule(true);
                ResetSelected();
                SetPageSize(20);
                ReLoadList();
                _row = GV_Header_steeltrucks.HeaderRow;
                var x = (HtmlTableCell)_row.FindControl("td_date");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x.Attributes["class"] += " active asc";
                else
                    x.Attributes["class"] += " active desc";
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void ReLoadList() {
        var ts = new TruckSchedule(true);
        int first = hf_dateselected_steeltrucks.Value.IndexOf('_');
        string month = hf_dateselected_steeltrucks.Value.Substring(0, first);
        int last = hf_dateselected_steeltrucks.Value.Length - 4;
        string year = hf_dateselected_steeltrucks.Value.Substring(last);

        lbl_schDriverUser_steeltrucks.Text = MonthConverter.ToStringMonth(Convert.ToInt16(month)) + " " + year + " Log";

        LoadScheduleHead_Search(ref GV_Header_steeltrucks, hf_sortcol_steeltrucks.Value, SortOrder,
                                ts.getOtherTruckDates(ts.scheduler_coll, month, year));
    }

    private void loadTruckSchedule(bool loaddates) {
        int totalweight = 0;
        var ts = new TruckSchedule(true);
        //ts.BinaryDeserialize();
        var str = new StringBuilder();
        str.Append("<h3><u>Weight Totals for Today</u></h3><div class='clear-space'></div>");
        var gd = new GeneralDirection(true);
        foreach (Dictionary<string, string> row in gd.generaldirection) {
            str.Append("<h4 class='pad-right float-left font-bold'>" + row["GeneralDirection"] +
                       ":</h4><h4 class='float-left'>" +
                       (ts.calTotalWeightGD(DateTime.Now.ToShortDateString(), row["GeneralDirection"].ToString())
                          .ToString("#,##0")) + " lbs</h4><div class='clear'></div>");
            totalweight +=
                Convert.ToInt32(ts.calTotalWeightGD(DateTime.Now.ToShortDateString(), row["GeneralDirection"].ToString()));
        }
        str.Append(
            "<div class='clear' style='width: 50%; margin: 8px 0; border-top: 1px solid #939393;'></div><h3 style='color: #353535;'>&nbsp;<b>Total:&nbsp;&nbsp;</b>" +
            totalweight.ToString("#,##0") + " lbs</h3>");
        str.Append(
            "<small>(Total = SMW Trucks + Common Carriers)</small><div class='clear-space'></div><div class='clear-space'></div>");
        str.Append(
            "<input type='button' onclick='javascript:LoadGenDirEditor();' class='input-buttons float-left' value='Direction Edit' />");
        if (loaddates) {
            str.Append("<div class='clear-space'></div><div id='sbar_steeltrucks'>");
            str.Append("<div class='clear-space'></div><div class='clear-margin'>");
            str.Append(
                "<h3>Monthly Logs</h3><div class='clear-space-five'></div>");
            if (ts.drivers_coll.Count == 0) {
                str.Append("There are no Common Carrier Logs");
            }
            else {
                ts.scheduler_coll.Sort(
                    (x, y) => Convert.ToDateTime(y.Date).Ticks.CompareTo(Convert.ToDateTime(x.Date).Ticks));
                var tempList = new List<string>();
                foreach (var t in ts.scheduler_coll) {
                    try {
                        int first = t.Date.IndexOf('/');
                        string month = t.Date.Substring(0, first);
                        int last = t.Date.Length - 4;
                        string year = t.Date.Substring(last);

                        string currdate = month + "_" + year;
                        if (tempList.Contains(currdate)) continue;
                        str.Append("<div id='expand_" + currdate + "' class='tsdiv RandomActionBtns' onclick=\"CommonCarrierDateSelecte('" + currdate + "')\">");
                        str.Append("<div class='pad-all-sml'><h3 class='font-bold float-left'>" + MonthConverter.ToStringMonth(Convert.ToInt32(month)) + "</h3>");
                        str.Append("<span class='float-right' font-size: 15px'>" + year + "</span>");
                        str.Append("</div></div>");
                        str.Append("<div class='sidebar-divider-no-margin'></div>");

                        tempList.Add(currdate);
                    }
                    catch (Exception) {
                    }
                }
            }
            str.Append("</div>");
        }
        ltl_ect.Text = str.ToString();
    }

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        var ts = new List<TruckSchedule_Coll>();
        foreach (var x in _truckschedule.scheduler_coll) {
            char[] trimBoth = { ' ' };
            string findValue = tb_search_steeltrucks.Text.ToLower().TrimEnd(trimBoth);
            findValue = findValue.TrimStart(trimBoth);
            var coll = new TruckSchedule_Coll(x.ID, x.DriverName.Replace(" ", "_"), x.TruckLine, x.Date, x.Unit,
                                              x.CustomerName, x.City, x.OrderNumber, x.Sequence, x.GeneralDirection,
                                              x.Weight, x.AdditionalInfo, x.LastUpdated);
            if ((x.City.ToLower().Contains(findValue)) || (x.CustomerName.ToLower().Contains(findValue)) ||
                (x.Date.ToLower().Contains(findValue))
                || (x.DriverName.Replace("_", " ").ToLower().Contains(findValue)) ||
                (x.GeneralDirection.ToLower().Contains(findValue))
                || (x.DriverName.ToLower().Contains(findValue)) || (x.Unit.ToLower().Contains(findValue))
                || (x.Weight.ToLower().Contains(findValue)) || (x.OrderNumber.ToLower().Contains(findValue))) {
                ts.Add(coll);
            }
        }
        loadTruckSchedule(true);
        LoadScheduleHead_Search(ref GV_Header_steeltrucks, hf_sortcol_steeltrucks.Value, SortOrder, ts);
    }

    protected void hf_dateselected_steeltrucks_Changed(object sender, EventArgs e) {
        ReLoadList();
    }

    #region Auto Update System

    private void UpdateUserFlags() {
        var apps = new App();
        foreach (string username in from MembershipUser user in Membership.GetAllUsers() select user.UserName) {
            var m = new MemberDatabase(username);
            bool hasapp1 = m.UserHasApp(AppId);
            bool hasapp2 = m.UserHasApp(AppId);
            if ((hasapp1) && (username.ToLower() != _username.ToLower()))
                _uuf.addFlag(username, AppId, "");
            if (hasapp2)
                _uuf.addFlag(username, "app-dailyoverview", "");
        }
    }

    #endregion


    #region GridView Properties Methods

    protected void GV_Header_RowEdit(object sender, GridViewEditEventArgs e) {
        GV_Header_steeltrucks.EditIndex = e.NewEditIndex;
        ReLoadList();

        var ddGdEdit = (DropDownList)GV_Header_steeltrucks.Rows[e.NewEditIndex].FindControl("dd_gd_edit");
        var hfGd = (HiddenField)GV_Header_steeltrucks.Rows[e.NewEditIndex].FindControl("hf_generaldirection_edit");
        var hfDate = (HiddenField)GV_Header_steeltrucks.Rows[e.NewEditIndex].FindControl("hf_date_edit");
        CalendarExtender defaultCalendarExtender = (CalendarExtender)GV_Header_steeltrucks.Rows[e.NewEditIndex].FindControl("defaultCalendarExtender");
        var gd = new GeneralDirection(true);
        int count = 0;
        DateTime outDate = DateTime.Now;
        if (DateTime.TryParse(hfDate.Value, out outDate))
            defaultCalendarExtender.SelectedDate = outDate;

        foreach (Dictionary<string, string> row in gd.generaldirection) {
            var item = new ListItem(row["GeneralDirection"].ToString(), row["GeneralDirection"].ToString());
            if (!ddGdEdit.Items.Contains(item))
                ddGdEdit.Items.Add(item);

            if (row["GeneralDirection"].ToString().ToLower() == hfGd.Value.ToLower())
                ddGdEdit.SelectedIndex = count;
            count++;
        }
    }

    protected void GV_Header_CancelEdit(object sender, GridViewCancelEditEventArgs e) {
        GV_Header_steeltrucks.EditIndex = -1;
        ReLoadList();
    }

    protected void GV_Header_RowUpdate(object sender, GridViewUpdateEventArgs e) {
        bool updated = true;
        var tb_date = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_date_edit");
        var tb_truckline = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_truckline_edit");
        var tb_customer = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_customer_edit");
        var tb_weight = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_weight_edit");
        var tb_salesorder = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_salesorder_edit");
        var tb_trucknumber = (TextBox)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("tb_trucknumber_edit");
        var hf_id = (HiddenField)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("hf_id_edit");
        var dd_gd_edit = (DropDownList)GV_Header_steeltrucks.Rows[e.RowIndex].FindControl("dd_gd_edit");
        if ((string.IsNullOrEmpty(tb_truckline.Text)) || (string.IsNullOrEmpty(tb_customer.Text))
            || (string.IsNullOrEmpty(tb_date.Text)) || (string.IsNullOrEmpty(tb_weight.Text))
            || (string.IsNullOrEmpty(tb_salesorder.Text))) {
            updated = false;
        }
        else {
            var temp = new TruckSchedule(true);
            var id = new Guid();
            if (!string.IsNullOrEmpty(hf_id.Value)) {
                id = Guid.Parse(hf_id.Value);
            }
            temp.updateDate(id, tb_date.Text);
            temp.updateTruckLine(id, tb_truckline.Text);
            temp.updateCustomerName(id, tb_customer.Text);
            temp.updateOrderNumber(id, tb_salesorder.Text);
            temp.updateWeight(id, tb_weight.Text);
            temp.updateUnit(id, tb_trucknumber.Text);
            temp.updateGeneralDirection(id, dd_gd_edit.SelectedValue);
        }
        if (updated) {
            UpdateUserFlags();
            GV_Header_steeltrucks.EditIndex = -1;
            ReLoadList();
        }
    }

    protected void GV_Header_RowCreated(object sender, GridViewRowEventArgs e) {
        //RegisterAsynControls(e.Row.Controls);
        if (e.Row.RowType != DataControlRowType.Pager) return;
        var pnlPager = (Panel)e.Row.FindControl("PnlPager_steeltrucks");
        for (int i = 0; i < GV_Header_steeltrucks.PageCount; i++) {
            if (i % 20 == 0) {
                pnlPager.Controls.Add(new LiteralControl("<div class=\"clear-space\"></div>"));
            }
            var lbtnPage = new LinkButton {
                CommandArgument = i.ToString(CultureInfo.InvariantCulture),
                CommandName = "PageNo",
                CssClass =
                    GV_Header_steeltrucks.PageIndex == i
                        ? "GVPagerNumActive RandomActionBtns"
                        : "GVPagerNum RandomActionBtns",
                Text = (i + 1).ToString(CultureInfo.InvariantCulture)
            };
            pnlPager.Controls.Add(lbtnPage);
        }
    }

    protected void GV_Header_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            try {
                var defaultCalendarExtender = (CalendarExtender)e.Row.FindControl("defaultCalendarExtender");
                int first = hf_dateselected_steeltrucks.Value.IndexOf('_');
                if (first < 0) {
                    defaultCalendarExtender.SelectedDate = DateTime.Now;
                }
                else {
                    string month = hf_dateselected_steeltrucks.Value.Substring(0, first);
                    int last = hf_dateselected_steeltrucks.Value.Length - 4;
                    string year = hf_dateselected_steeltrucks.Value.Substring(last);
                    defaultCalendarExtender.SelectedDate =
                        Convert.ToDateTime(month + "/" + DateTime.Now.Day.ToString() + "/" + year);
                }

                var ddGd = (DropDownList)e.Row.FindControl("dd_gd");
                var gd = new GeneralDirection(true);
                foreach (
                    var item in
                        gd.generaldirection.Cast<Dictionary<string, string>>()
                          .Select(
                              row =>
                              new ListItem(row["GeneralDirection"].ToString(), row["GeneralDirection"].ToString()))
                          .Where(item => !ddGd.Items.Contains(item)))
                    ddGd.Items.Add(item);
            }
            catch (Exception) {
            }

            var addBtn = (LinkButton)e.Row.FindControl("lb_add");
            var tbDate = (TextBox)e.Row.FindControl("tb_date");
            var tbTruckline = (TextBox)e.Row.FindControl("tb_truckline");
            var tbtrucknumber = (TextBox)e.Row.FindControl("tb_trucknumber");
            var tbCustomer = (TextBox)e.Row.FindControl("tb_customer");
            var ddDirection = (DropDownList)e.Row.FindControl("dd_gd");
            var tbSalesorder = (TextBox)e.Row.FindControl("tb_salesorder");
            var tbWeight = (TextBox)e.Row.FindControl("tb_weight");

            var editBtn = (LinkButton)e.Row.FindControl("lb_edit_steeltrucks");
            var deleteBtn = (LinkButton)e.Row.FindControl("lb_delete_steeltrucks");
            if (GV_Header_steeltrucks.EditIndex != -1) {
                if (addBtn != null) {
                    addBtn.Visible = false;
                    tbDate.Visible = false;
                    tbTruckline.Visible = false;
                    tbtrucknumber.Visible = false;
                    tbCustomer.Visible = false;
                    ddDirection.Visible = false;
                    tbSalesorder.Visible = false;
                    tbWeight.Visible = false;
                }
                if (editBtn != null)
                    editBtn.Visible = false;
                if (deleteBtn != null)
                    deleteBtn.Visible = false;
            }
            else {
                if (addBtn != null) {
                    addBtn.Visible = true;
                    tbDate.Visible = true;
                    tbTruckline.Visible = true;
                    tbtrucknumber.Visible = true;
                    tbCustomer.Visible = true;
                    ddDirection.Visible = true;
                    tbSalesorder.Visible = true;
                    tbWeight.Visible = true;
                }
                if (editBtn != null)
                    editBtn.Visible = true;
                if (deleteBtn != null)
                    deleteBtn.Visible = true;
            }
        }

        if (e.Row.RowType == DataControlRowType.Pager) {
            var lbl2 = (Label)e.Row.FindControl("pglbl2_steeltrucks");
            var btnFirst = (LinkButton)e.Row.FindControl("btnFirst");
            var btnPrevious = (LinkButton)e.Row.FindControl("btnPrevious");
            var btnNext = (LinkButton)e.Row.FindControl("btnNext");
            var btnLast = (LinkButton)e.Row.FindControl("btnLast");
            var tbPage = (TextBox)e.Row.FindControl("tb_pageManual_steeltrucks");
            lbl2.Text = GV_Header_steeltrucks.PageCount.ToString();
            tbPage.Text = (GV_Header_steeltrucks.PageIndex + 1).ToString();

            if ((GV_Header_steeltrucks.PageIndex + 1) == 1) {
                btnFirst.Visible = false;
                btnPrevious.Visible = false;
            }
            if ((GV_Header_steeltrucks.PageIndex + 1) == GV_Header_steeltrucks.PageCount) {
                btnNext.Visible = false;
                btnLast.Visible = false;
            }
            InitializeSort(true);
        }
    }

    protected void GV_Header_RowCommand(object sender, GridViewCommandEventArgs e) {
        switch (e.CommandName) {
            case "Add":
                string d = string.Empty;
                int i = Convert.ToInt32(e.CommandArgument);
                i = i - 1;
                var tbDate = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_date");
                var tbTruckline = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_truckline");
                var tbCustomer = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_customer");
                var tbWeight = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_weight");
                var tbSalesorder = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_salesorder");
                var tbTrucknumber = (TextBox)GV_Header_steeltrucks.Rows[i].FindControl("tb_trucknumber");
                var ddGd = (DropDownList)GV_Header_steeltrucks.Rows[i].FindControl("dd_gd");
                var addsch = new TruckSchedule(true);
                addsch.addItem(string.Empty, tbTruckline.Text, tbDate.Text, tbTrucknumber.Text, tbCustomer.Text,
                               string.Empty, tbSalesorder.Text, 0, ddGd.SelectedValue, tbWeight.Text,
                               string.Empty);
                loadTruckSchedule(true);
                UpdateUserFlags();
                ReLoadList();
                break;
            case "deleteSlot":
                var delsch = new TruckSchedule(true);
                Guid id2 = Guid.Parse(e.CommandArgument.ToString());
                delsch.deleteSlot(id2);
                ReLoadList();
                loadTruckSchedule(true);
                UpdateUserFlags();
                break;
            case "PageNo":
                _pageSize = Convert.ToInt32(dd_display_steeltrucks.SelectedValue);
                SetPageSize(_pageSize);
                InitializeSort(false);
                ResetSelected();
                GV_Header_steeltrucks.PageIndex = Convert.ToInt32(e.CommandArgument.ToString());
                GV_Header_steeltrucks.DataBind();
                break;
        }
    }

    protected void GV_Header_PageIndexChanging(object sender, GridViewPageEventArgs e) {
        _pageSize = Convert.ToInt32(dd_display_steeltrucks.SelectedValue);
        SetPageSize(_pageSize);
        InitializeSort(false);
        ResetSelected();
        GV_Header_steeltrucks.PageIndex = e.NewPageIndex;
        GV_Header_steeltrucks.DataBind();
    }

    public void LoadScheduleHead_Search(ref GridView gv, string sortExp, string sortDir, List<TruckSchedule_Coll> ts) {
        var dvFiles = new DataView();
        dvFiles = GetScheduleHead(_truckschedule.getUserData_noDup2(ts));
        if (dvFiles.Count > 0) {
            if (sortExp != string.Empty) {
                dvFiles.Sort = string.Format("{0} {1}", dvFiles.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }
        }
        gv.DataSource = dvFiles;
        gv.DataBind();
        if (dvFiles.Count == 0) {
            lbtn_selectAll_steeltrucks.Enabled = false;
            lbtn_selectAll_steeltrucks.Visible = false;
        }
        else {
            lbtn_selectAll_steeltrucks.Enabled = true;
            lbtn_selectAll_steeltrucks.Visible = true;
        }
    }

    public DataView GetScheduleHead(List<TruckSchedule_Coll> ts) {
        var dtsch = new DataTable();
        dtsch.Columns.Add(new DataColumn("DateSort", Type.GetType("System.DateTime")));
        dtsch.Columns.Add(new DataColumn("TruckLine"));
        dtsch.Columns.Add(new DataColumn("TruckNumber"));
        dtsch.Columns.Add(new DataColumn("Customer"));
        dtsch.Columns.Add(new DataColumn("GeneralDirection"));
        dtsch.Columns.Add(new DataColumn("SalesOrder"));
        dtsch.Columns.Add(new DataColumn("WeightSort"));
        dtsch.Columns.Add(new DataColumn("Weight"));
        dtsch.Columns.Add(new DataColumn("ID"));
        dtsch.Columns.Add(new DataColumn("Date"));
        dtsch.Columns.Add(new DataColumn("RowClass"));
        dtsch.Columns.Add(new DataColumn("RowClassAdd"));

        bool addNewRow = true;
        int weight = 0;
        for (var i = 0; i < ts.Count; i++) {
            DataRow drsch = dtsch.NewRow();
            if ((i == 0) && (string.IsNullOrEmpty(ts[i].CustomerName)) && (string.IsNullOrEmpty(ts[i].City)) &&
                (string.IsNullOrEmpty(ts[i].OrderNumber))) {
                addNewRow = false;
                drsch["DateSort"] = DateTime.Now.ToShortDateString();
                drsch["Date"] = "Date";
                drsch["TruckLine"] = "Truckline";
                drsch["TruckNumber"] = "Truck #";
                drsch["Customer"] = "Customer";
                drsch["GeneralDirection"] = "Direction";
                drsch["ID"] = string.Empty;
                drsch["SalesOrder"] = "Order #";
                drsch["Weight"] = "Weight";
                drsch["RowClassAdd"] = "myItemStyle";
                drsch["RowClass"] = "date-nodisplay";
            }
            else {
                try {
                    drsch["DateSort"] = ts[i].Date;
                    drsch["Date"] = ts[i].Date;
                }
                catch {
                    drsch["DateSort"] = 1;
                    drsch["Date"] = "N/A";
                }
                drsch["TruckLine"] = ts[i].TruckLine;
                drsch["TruckNumber"] = ts[i].Unit;
                drsch["Customer"] = ts[i].CustomerName;
                drsch["GeneralDirection"] = ts[i].GeneralDirection;
                drsch["ID"] = ts[i].ID;
                drsch["SalesOrder"] = addWhiteSpace(ts[i].OrderNumber);
                int tempWeight;
                if (Int32.TryParse(ts[i].Weight.Replace(",", ""), out tempWeight)) {
                    drsch["Weight"] = tempWeight.ToString("#,##0");
                    drsch["WeightSort"] = "a" + tempWeight.ToString("#,##0");
                }
                else {
                    drsch["Weight"] = ts[i].Weight.Replace(",", "");
                    drsch["WeightSort"] = "a" + ts[i].Weight.Replace(",", "");
                }
                try {
                    weight += Convert.ToInt32(ts[i].Weight.Replace(",", ""));
                }
                catch (Exception) {
                }
                drsch["RowClassAdd"] = "date-nodisplay";
                drsch["RowClass"] = "myItemStyle";
            }
            dtsch.Rows.Add(drsch);
        }
        if (addNewRow) {
            DataRow drsch = dtsch.NewRow();
            drsch["DateSort"] = DateTime.Now.AddYears(1).ToString();
            drsch["Date"] = "Date";
            drsch["TruckLine"] = "Truckline";
            drsch["TruckNumber"] = "Truck #";
            drsch["Customer"] = "Customer";
            drsch["GeneralDirection"] = "Direction";
            drsch["ID"] = string.Empty;
            drsch["SalesOrder"] = "Order #";
            drsch["Weight"] = "Weight";
            drsch["RowClassAdd"] = "myItemStyle";
            drsch["RowClass"] = "date-nodisplay";
            dtsch.Rows.Add(drsch);
        }
        lbl_smwtruckweight_steeltrucks.Text = weight.ToString("#,##0") + " lbs";
        var dvsch = new DataView(dtsch);
        return dvsch;
    }

    protected void tb_pageManual_TextChanged(object sender, EventArgs e) {
        int count = 0;
        count = !string.IsNullOrEmpty(hf_dateselected_steeltrucks.Value)
                    ? _truckschedule.getUserData(hf_dateselected_steeltrucks.Value.Replace(" ", "_")).Count
                    : _truckschedule.scheduler_coll.Count;

        if (count <= 0) return;
        _pageSize = Convert.ToInt32(dd_display_steeltrucks.SelectedValue);
        SetPageSize(_pageSize);
        InitializeSort(false);

        int page = Convert.ToInt32(((TextBox)sender).Text);
        if (page > GV_Header_steeltrucks.PageCount) {
            page = GV_Header_steeltrucks.PageCount;
        }
        else if (page <= 0) {
            page = 1;
        }

        ResetSelected();

        GV_Header_steeltrucks.PageIndex = page - 1;
        GV_Header_steeltrucks.DataBind();
    }

    private void ResetSelected() {
        HiddenField1_steeltrucks.Value = string.Empty;
        lbtn_selectAll_steeltrucks.Text = "Select All";
    }

    protected void CheckBoxIndv_CheckChanged(object sender, EventArgs e) {
        var chk = (CheckBox)sender;
        string filename = chk.Text;
        string[] filelist = HiddenField1_steeltrucks.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        if (filelist.Contains(filename)) {
            var templist = new List<string>();
            for (int i = 0; i < filelist.Length; i++) {
                if (!filelist[i].Contains(filename)) {
                    templist.Add(filelist[i]);
                }
            }
            HiddenField1_steeltrucks.Value = "";
            if (templist.Count > 0) {
                foreach (string file in templist) {
                    HiddenField1_steeltrucks.Value += file + ServerSettings.StringDelimiter;
                }
            }
        }
        else {
            HiddenField1_steeltrucks.Value += filename + ServerSettings.StringDelimiter;
        }
    }

    #endregion

    #region GridView Sorting methods

    public string SortOrder {
        get {
            if (hf_sortcol_steeltrucks.Value == _hfSortcolprev) {
                if (ViewState["sortOrder"].ToString() == "desc") {
                    ViewState["sortOrder"] = "asc";
                }
                else {
                    ViewState["sortOrder"] = "desc";
                }
            }
            else if (_hfSortcolprev != "") {
                ViewState["sortOrder"] = "desc";
            }
            if (ViewState["sortOrder"] != null) {
                return ViewState["sortOrder"].ToString();
            }
            ViewState["sortOrder"] = "desc";
            return ViewState["sortOrder"].ToString();
        }
        set { ViewState["sortOrder"] = value; }
    }

    protected void dd_display_SelectedIndexChanged(object sender, EventArgs e) {
        switch (dd_display_steeltrucks.SelectedValue) {
            case "10":
                SetPageSize(10);
                break;
            case "20":
                SetPageSize(20);
                break;
            case "30":
                SetPageSize(30);
                break;
            case "40":
                SetPageSize(40);
                break;
            case "2000":
                SetPageSize(20000);
                break;
        }

        ResetSelected();
        InitializeSort(false);
    }

    private void SetPageSize(int size) {
        switch (size) {
            case 10:
                GV_Header_steeltrucks.PageSize = 10;
                dd_display_steeltrucks.SelectedIndex = 0;
                break;
            case 20:
                GV_Header_steeltrucks.PageSize = 20;
                dd_display_steeltrucks.SelectedIndex = 1;
                break;
            case 30:
                GV_Header_steeltrucks.PageSize = 30;
                dd_display_steeltrucks.SelectedIndex = 2;
                break;
            case 40:
                GV_Header_steeltrucks.PageSize = 40;
                dd_display_steeltrucks.SelectedIndex = 3;
                break;
            default:
                GV_Header_steeltrucks.PageSize = 2000;
                dd_display_steeltrucks.SelectedIndex = 4;
                break;
        }
    }

    protected void imgbtn_del_Click(object sender, EventArgs e) {
        string[] schedulelist = HiddenField1_steeltrucks.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (var id in from x in schedulelist where !string.IsNullOrEmpty(x) select Guid.Parse(x))
            _truckschedule.deleteSlot(id);
        ResetSelected();
        InitializeSort(false);
        loadTruckSchedule(true);
    }

    protected void lbtn_selectAll_Click(object sender, EventArgs e) {
        if (lbtn_selectAll_steeltrucks.Text == "Select All") {
            HiddenField1_steeltrucks.Value = string.Empty;
            foreach (
                var chk in
                    from GridViewRow r in GV_Header_steeltrucks.Rows
                    select (CheckBox)r.FindControl("CheckBoxIndv_steeltrucks")) {
                chk.Checked = true;
                string filename = chk.Text;
                string[] filelist = HiddenField1_steeltrucks.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if (filelist.Contains(filename)) {
                    var templist = new List<string>();
                    for (int i = 0; i < filelist.Length; i++) {
                        if (!filelist[i].Contains(filename)) {
                            templist.Add(filelist[i]);
                        }
                    }
                    HiddenField1_steeltrucks.Value = "";
                    if (templist.Count > 0) {
                        foreach (string file in templist) {
                            HiddenField1_steeltrucks.Value += file + ServerSettings.StringDelimiter;
                        }
                    }
                }
                else {
                    HiddenField1_steeltrucks.Value += filename + ServerSettings.StringDelimiter;
                }
            }
            lbtn_selectAll_steeltrucks.Text = "Deselect All";
        }

        else {
            foreach (
                var chk in
                    from GridViewRow r in GV_Header_steeltrucks.Rows
                    select (CheckBox)r.FindControl("CheckBoxIndv_steeltrucks")) {
                chk.Checked = false;
            }
            ResetSelected();
        }
    }

    protected void btn_refresh_Click(object sender, EventArgs e) {
        ResetSelected();
        SetPageSize(20);
        ViewState["sortOrder"] = "desc";
        hf_sortcol_steeltrucks.Value = "0";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_date");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    private void InitializeSort(bool pagerevent) {
        _hfSortcolprev = "";
        if (!pagerevent) {
            ReLoadList();
        }
        _row = GV_Header_steeltrucks.HeaderRow;
        switch (hf_sortcol_steeltrucks.Value) {
            case "0":
                var x1 = (HtmlTableCell)_row.FindControl("td_date");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x1.Attributes["class"] += " active asc";
                else
                    x1.Attributes["class"] += " active desc";
                break;
            case "1":
                var x2 = (HtmlTableCell)_row.FindControl("td_truckline");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x2.Attributes["class"] += " active asc";
                else
                    x2.Attributes["class"] += " active desc";
                break;
            case "2":
                var x3 = (HtmlTableCell)_row.FindControl("td_trucknumber");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x3.Attributes["class"] += " active asc";
                else
                    x3.Attributes["class"] += " active desc";
                break;
            case "3":
                var x4 = (HtmlTableCell)_row.FindControl("td_customer");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x4.Attributes["class"] += " active asc";
                else
                    x4.Attributes["class"] += " active desc";
                break;
            case "4":
                var x5 = (HtmlTableCell)_row.FindControl("td_direction");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x5.Attributes["class"] += " active asc";
                else
                    x5.Attributes["class"] += " active desc";
                break;

            case "5":
                var x6 = (HtmlTableCell)_row.FindControl("td_ordernumber");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x6.Attributes["class"] += " active asc";
                else
                    x6.Attributes["class"] += " active desc";
                break;
            case "6":
                var x7 = (HtmlTableCell)_row.FindControl("td_weight");
                if (ViewState["sortOrder"].ToString() == "asc")
                    x7.Attributes["class"] += " active asc";
                else
                    x7.Attributes["class"] += " active desc";
                break;
        }
    }

    protected void lbtn_unit_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "2";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_trucknumber");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_name_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "4";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_customer");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_truckline_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "1";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_truckline");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_direction_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "4";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_direction");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_weight_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "6";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_weight");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_date_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "0";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_date");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    protected void lbtn_ordernumber_Click(object sender, EventArgs e) {
        _hfSortcolprev = hf_sortcol_steeltrucks.Value;
        hf_sortcol_steeltrucks.Value = "5";

        ReLoadList();

        _row = GV_Header_steeltrucks.HeaderRow;
        var x = (HtmlTableCell)_row.FindControl("td_ordernumber");
        if (ViewState["sortOrder"].ToString() == "asc")
            x.Attributes["class"] += " active asc";
        else
            x.Attributes["class"] += " active desc";
    }

    private static string addWhiteSpace(string x) {
        string ret = x.Replace(",", ", ");
        ret = ret.Replace(".", ", ");
        return ret;
    }

    #endregion

    #region Buttons

    protected void btnExportToExcel_Click(object sender, EventArgs e) {
        try {
            var ts = new TruckSchedule(true);
            int first = hf_dateselected_steeltrucks.Value.IndexOf('_');
            string month = hf_dateselected_steeltrucks.Value.Substring(0, first);
            int last = hf_dateselected_steeltrucks.Value.Length - 4;
            string year = hf_dateselected_steeltrucks.Value.Substring(last);

            string _Path = "CCLog_" + month + "-" + year + ".xls";
            string directory = ServerSettings.GetServerMapLocation + "Apps\\CommonCarriers\\Exports";
            string p = Path.Combine(directory, _Path);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var dvFiles = new DataView();
            dvFiles = GetScheduleHead(_truckschedule.getUserData_noDup2(ts.getOtherTruckDates(ts.scheduler_coll, month, year)));
            DataTable dt1 = dvFiles.ToTable();
            if (dt1 == null) {
                throw new Exception("No Records to Export");
            }
            else {
                var temp = new DataTable();
                temp.Columns.Add(new DataColumn("DateSort"));
                temp.Columns.Add(new DataColumn("Date"));
                temp.Columns.Add(new DataColumn("TruckLine"));
                temp.Columns.Add(new DataColumn("TruckNumber"));
                temp.Columns.Add(new DataColumn("Customer"));
                temp.Columns.Add(new DataColumn("GeneralDirection"));
                temp.Columns.Add(new DataColumn("SalesOrder"));
                temp.Columns.Add(new DataColumn("Weight"));

                foreach (DataRow dr in dt1.Rows) {
                    DataRow drsch = temp.NewRow();
                    var tempdate = new DateTime();
                    if (DateTime.TryParse(dr["Date"].ToString(), out tempdate)) {
                        drsch["DateSort"] = "a" + tempdate.ToShortDateString().Replace("/", "_");
                        drsch["Date"] = dr["Date"];
                        drsch["TruckLine"] = dr["TruckLine"];
                        drsch["TruckNumber"] = dr["TruckNumber"];
                        drsch["Customer"] = dr["Customer"];
                        drsch["GeneralDirection"] = dr["GeneralDirection"];
                        drsch["SalesOrder"] = dr["SalesOrder"];
                        drsch["Weight"] = dr["Weight"];

                        temp.Rows.Add(drsch);
                    }
                }

                var dvtemp = new DataView(temp);
                dvtemp.Sort = string.Format("{0} {1}", dvtemp.Table.Columns[0], "asc");
                temp = dvtemp.Table;

                var temp2 = new DataTable();
                temp2.Columns.Add(new DataColumn("Date"));
                temp2.Columns.Add(new DataColumn("Truck Line"));
                temp2.Columns.Add(new DataColumn("Truck Number"));
                temp2.Columns.Add(new DataColumn("Customer"));
                temp2.Columns.Add(new DataColumn("General Direction"));
                temp2.Columns.Add(new DataColumn("Sales Order"));
                temp2.Columns.Add(new DataColumn("Weight"));

                foreach (DataRow dr in temp.Rows) {
                    DataRow drsch = temp2.NewRow();
                    drsch["Date"] = dr["Date"];
                    drsch["Truck Line"] = dr["TruckLine"];
                    drsch["Truck Number"] = dr["TruckNumber"];
                    drsch["Customer"] = dr["Customer"];
                    drsch["General Direction"] = dr["GeneralDirection"];
                    drsch["Sales Order"] = dr["SalesOrder"];
                    drsch["Weight"] = dr["Weight"];

                    temp2.Rows.Add(drsch);
                }
                dt1 = temp2;
            }

            var stringWriter = new StringWriter();
            var htmlWrite = new HtmlTextWriter(stringWriter);
            var DataGrd = new DataGrid();
            DataGrd.DataSource = dt1;
            DataGrd.DataBind();

            DataGrd.RenderControl(htmlWrite);

            try {
                if (File.Exists(p)) {
                    File.Delete(p);
                }
            }
            catch { }

            var vw = new StreamWriter(p, true);
            stringWriter.ToString().Normalize();
            vw.Write(stringWriter.ToString());
            vw.Flush();
            vw.Close();

            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$.fileDownload('../../" + "Apps/CommonCarriers/Exports" + "/" + _Path + "');", true);
        }
        catch (Exception ex) {
            Page.Response.Redirect("~/Apps/CommonCarriers/CommonCarriers.aspx");
        }
    }

    protected void lb_delete_Click(object sender, EventArgs e) {
        DeleteSchedule();
    }

    private static void DeleteSchedule() {
        //string driver = hf_userselected_steeltrucks.Value;
        //string date = hf_userselecteddate_steeltrucks.Value;
        //string unit = hf_userselectedunit_steeltrucks.Value;
        //string gd = hf_userselectedgd_steeltrucks.Value;
        //string tempdriver = hf_userselected_steeltrucks.Value;
        //try
        //{
        //    tempdriver = tempdriver.Replace("_", " ");
        //}
        //catch { }
        //TruckSchedule ts = new TruckSchedule(driver, date, unit, gd);
        //for (int i = 0; i < ts.scheduler_coll.Count; i++)
        //{
        //    ts.deleteSlot(ts.scheduler_coll[i].ID);
        //}
        //closeData();
        //lb_close_steeltrucks.Enabled = false;
        //lb_close_steeltrucks.Visible = false;
        //lb_delete_steeltrucks.Enabled = false;
        //lb_delete_steeltrucks.Visible = false;
        //lb_print_steeltrucks.Enabled = false;
        //lb_print_steeltrucks.Visible = false;
        //loadTruckSchedule(true);
    }

    #endregion
}