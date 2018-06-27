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
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;
using System.Web.Script.Serialization;

#endregion

public partial class Apps_CommonCarriers : Page {

    #region private variables

    private const string AppId = "app-commoncarriers";
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private AppInitializer _appInitializer;
    private MemberDatabase _member;
    private TruckSchedule _truckschedule;
    private string _username;
    private string CurrentSelectedDate {
        get {
            if (ViewState["CurrentSelectedDate"] != null) {
                return ViewState["CurrentSelectedDate"].ToString();
            }

            ViewState["CurrentSelectedDate"] = ServerSettings.ServerDateTime.Month + "_" + ServerSettings.ServerDateTime.Year;
            return ViewState["CurrentSelectedDate"].ToString();
        }
        set {
            ViewState["CurrentSelectedDate"] = value;
        }
    }

    #endregion


    #region Load Controls

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }

        _appInitializer = new AppInitializer(AppId, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            _username = _appInitializer.UserName;
            _member = _appInitializer.memberDatabase;

            _truckschedule = new TruckSchedule(false, "", "");
            if (!IsPostBack) {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();
                _appInitializer.LoadDefaultScripts();
                _appInitializer.LoadCustomFonts();
                LoadControls();
            }
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
    }
    private void LoadControls() {
        string month = GetCurrentMonth();
        string year = GetCurrentYear();

        _truckschedule = new TruckSchedule(month, year);
        LoadTable(month, year);

        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
        string jsVal = js.Serialize(GetCommonCarrierAutoCompleteLists());
        jsVal = jsVal.Replace(" ", "~");
        hf_AutoCompleteList.Value = HttpUtility.UrlEncode(jsVal);

        for (int i = 0; i < dd_MonthSelector.Items.Count; i++) {
            if (dd_MonthSelector.Items[i].Value == month) {
                dd_MonthSelector.SelectedIndex = i;
                break;
            }
        }

        for (int i = 0; i < dd_YearSelector.Items.Count; i++) {
            if (dd_YearSelector.Items[i].Value == year) {
                dd_YearSelector.SelectedIndex = i;
                break;
            }
        }
    }
    private void LoadTodayTotals() {
        int totalweight = 0;

        var str = new StringBuilder();
        var gd = new GeneralDirection(true);
        foreach (Dictionary<string, string> row in gd.generaldirection) {
            int calculatedWeight = _truckschedule.calTotalWeightGD(ServerSettings.ServerDateTime.ToShortDateString(), row["GeneralDirection"].ToString());
            str.Append("<span class='float-left font-bold'>" + row["GeneralDirection"] + ":</span><span class='float-right'>" + calculatedWeight.ToString("#,##0") + " lbs</span><div class='clear-space-five'></div>");
            totalweight += calculatedWeight;
        }

        str.Append("<div class='clear' style='margin: 8px 0; border-top: 1px solid #939393;'></div><b>Total:&nbsp;&nbsp;</b><span class='float-right'>" + totalweight.ToString("#,##0") + " lbs</span><div class='clear'></div>");
        str.Append("<i style='font-size: 10px;'>(Total = SMW Trucks + Common Carriers)</i><div class='clear'></div>");
        ltl_ect.Text = str.ToString();
    }
    private void LoadTable(string month, string year) {
        List<TruckSchedule_Coll> otherList = _truckschedule.getOtherTruckDates(_truckschedule.scheduler_coll, month, year);
        otherList = _truckschedule.getUserData_noDup2(otherList);

        int totalWeight = 0;

        GeneralDirection gd = new GeneralDirection(true);
        TableBuilder tableBuilder = new TableBuilder(Page, true, true, 2, "CommonCarriersTable");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Date", "90px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Truck Line", "100px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Truck Number", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Customer", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Direction", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Order Number", "75px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Weight", "75px", false));

        tableBuilder.AddHeaderRow(headerColumns, true, "Date", "DESC");
        #endregion

        #region Build Body
        bool foundEdit = false;
        for (int i = 0; i < otherList.Count; i++) {
            TruckSchedule_Coll item = otherList[i];
            string date = string.Empty;
            DateTime newDate = new DateTime();
            DateTime.TryParse(item.Date, out newDate);

            string weightStr = string.Empty;
            int tempWeight;
            if (Int32.TryParse(item.Weight.Replace(",", ""), out tempWeight)) {
                    weightStr= tempWeight.ToString("#,##0");
            }
            else {
                weightStr = item.Weight.Replace(",", "");
            }

            try {
                totalWeight += Convert.ToInt32(item.Weight.Replace(",", ""));
            }
            catch {
            }

            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

            if (hf_EditItem.Value == item.ID.ToString()) {
                bodyColumns.Add(new TableBuilderBodyColumnValues("Date", "<input type='text' class='textEntry-noWidth cc-edit-control datepicker' value='" + newDate.ToShortDateString() + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" /><span class='sort-value-class' data-sortvalue='a" + (newDate.Ticks + i).ToString() + "'></span>", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Truck Line", "<input type='text' data-column='TruckLine' class='textEntry-noWidth cc-edit-control' value='" + item.TruckLine + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Truck Number", "<input type='text' data-column='TruckNumber' class='textEntry-noWidth cc-edit-control' value='" + item.Unit + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Customer", "<input type='text' data-column='Customer' class='textEntry-noWidth cc-edit-control' value='" + item.CustomerName + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Direction", BuildDirectionDropdown(item.GeneralDirection, gd, true), TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Order Number", "<input type='text' class='textEntry-noWidth cc-edit-control' value='" + addWhiteSpace(item.OrderNumber) + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Weight", "<input type='text' class='textEntry-noWidth cc-edit-control' value='" + weightStr + "' onkeypress=\"commonCarriers.UpdateItem_KeyPress(event, '" + item.ID.ToString() + "');\" />", TableBuilderColumnAlignment.Left));

                string updateBtn = "<a class='td-update-btn' title='Update' onclick=\"commonCarriers.UpdateItem('" + item.ID.ToString() + "');return false;\"></a>";
                string cancelBtn = "<a class='td-cancel-btn' title='Cancel' onclick=\"commonCarriers.CancelItem();return false;\"></a>";

                tableBuilder.AddBodyRow(bodyColumns, updateBtn + cancelBtn, "data-id='" + item.ID.ToString() + "'");
                foundEdit = true;
            }
            else {
                bodyColumns.Add(new TableBuilderBodyColumnValues("Date", newDate.ToShortDateString() + "<span class='sort-value-class' data-sortvalue='a" + (newDate.Ticks + i).ToString() + "'></span>", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Truck Line", item.TruckLine, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Truck Number", item.Unit, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Customer", item.CustomerName, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Direction", item.GeneralDirection, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Order Number", addWhiteSpace(item.OrderNumber), TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Weight", weightStr, TableBuilderColumnAlignment.Left));

                string editBtn = "<a class='td-edit-btn' title='Edit' onclick=\"commonCarriers.EditItem('" + item.ID.ToString() + "');return false;\"></a>";
                string deleteBtn = "<a class='td-delete-btn' title='Delete' onclick=\"commonCarriers.DeleteItem('" + item.ID.ToString() + "');return false;\"></a>";

                tableBuilder.AddBodyRow(bodyColumns, editBtn + deleteBtn, "data-id='" + item.ID.ToString() + "'");
            }
        }
        #endregion

        lbl_smwtruckweight_steeltrucks.Text = totalWeight.ToString("#,##0") + " lbs";

        if (!foundEdit) {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
            insertColumns.Add(new TableBuilderInsertColumnValues("Date", "<input type=\"text\" id=\"tb_Date_Insert\" class=\"textEntry-noWidth datepicker\" value=\"" + ServerSettings.ServerDateTime.Month + "/" + ServerSettings.ServerDateTime.Day + "/" + ServerSettings.ServerDateTime.Year + "\" onkeypress=\"openWSE.GridViewMethods.OnInsertRow_KeyPress(event,'ASP.apps_commoncarriers_commoncarriers_aspxGridview','commonCarriers.InsertItem()%3b');\">", TableBuilderColumnAlignment.Left));
            insertColumns.Add(new TableBuilderInsertColumnValues("Truck Line", "tb_TruckLine_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Truck Line"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Truck Number", "tb_TruckNumber_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Truck Number"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Customer", "tb_Customer_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Customer"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Direction", BuildDirectionDropdown(string.Empty, gd, false), TableBuilderColumnAlignment.Left));
            insertColumns.Add(new TableBuilderInsertColumnValues("Order Number", "tb_OrderNumber_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Order Number"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Weight", "tb_Weight_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Weight"));

            tableBuilder.AddInsertRow(insertColumns, "commonCarriers.InsertItem()");
        }
        else {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
            insertColumns.Add(new TableBuilderInsertColumnValues("Date", "<input type=\"text\" class=\"textEntry-noWidth\" style=\"visibility: hidden;\" />", TableBuilderColumnAlignment.Left));
            insertColumns.Add(new TableBuilderInsertColumnValues("Truck Line", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Truck Number", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Customer", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Direction", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Order Number", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Weight", "", TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));

            tableBuilder.AddInsertRow(insertColumns, "", "", "hide-addbtn");
        }

        pnl_SteelTrucks.Controls.Clear();
        pnl_SteelTrucks.Controls.Add(tableBuilder.CompleteTableLiteralControl("No Common Carrier logs for selected month", true));
    }
    private string BuildDirectionDropdown(string value, GeneralDirection gd, bool editMode) {
        StringBuilder str = new StringBuilder();
        if (editMode) {
            str.Append("<select class='cc-edit-control'>");
        }
        else {
            str.Append("<select id='dd_Direction_Insert'>");
        }

        foreach (Dictionary<string, string> row in gd.generaldirection) {
            string selected = string.Empty;
            if (value == row["GeneralDirection"]) {
                selected = " selected='selected'";
            }

            str.Append("<option value='" + row["GeneralDirection"] + "'" + selected + ">" + row["GeneralDirection"] + "</option>");
        }

        str.Append("</select>");
        return str.ToString();
    }
    private static string addWhiteSpace(string x) {
        string ret = x.Replace(",", ", ");
        ret = ret.Replace(".", ", ");
        return ret;
    }

    private string GetCurrentMonth() {
        int first = CurrentSelectedDate.IndexOf('_');
        return CurrentSelectedDate.Substring(0, first);
    }
    private string GetCurrentYear() {
        int last = CurrentSelectedDate.Length - 4;
        return CurrentSelectedDate.Substring(last);
    }

    #endregion


    #region AutoCompleteList

    private object[] GetCommonCarrierAutoCompleteLists() {
        List<object> returnObj = new List<object>();

        var ts2 = new TruckSchedule();

        returnObj.Add(GetTruckLinesCC());
        returnObj.Add(GetListCustomersTS(ts2));
        returnObj.Add(GetListOfSMWUnits(ts2));
        return returnObj.ToArray();
    }
    private string[] GetTruckLinesCC() {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in _truckschedule.scheduler_coll) {
            if (!temp.Contains(t.TruckLine) && !string.IsNullOrEmpty(t.TruckLine)) {
                temp.Add(t.TruckLine);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }
    private string[] GetListCustomersTS(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
            if (!temp.Contains(t.CustomerName) && !string.IsNullOrEmpty(t.CustomerName)) {
                temp.Add(t.CustomerName);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }
    private string[] GetListOfSMWUnits(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
            if (!temp.Contains(t.Unit) && !string.IsNullOrEmpty(t.Unit)) {
                temp.Add(t.Unit);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }

    #endregion


    #region Auto Update System

    private void UpdateUserFlags() {
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


    #region Postback Controls

    protected void btnExportToExcel_Click(object sender, EventArgs e) {
        try {
            string month = GetCurrentMonth();
            string year = GetCurrentYear();

            _truckschedule = new TruckSchedule(month, year);

            string _Path = "CCLog_" + month + "-" + year + ".xls";
            string directory = ServerSettings.GetServerMapLocation + "Apps\\CommonCarriers\\Exports";
            string p = Path.Combine(directory, _Path);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var dvFiles = new DataView();
            dvFiles = GetExportScheduleHead(_truckschedule.getUserData_noDup2(_truckschedule.getOtherTruckDates(_truckschedule.scheduler_coll, month, year)));
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


            LoadControls();
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$.fileDownload('../../" + "Apps/CommonCarriers/Exports" + "/" + _Path + "');", true);
        }
        catch {
            HelperMethods.PageRedirect("~/Apps/CommonCarriers/CommonCarriers.aspx");
        }
    }
    public DataView GetExportScheduleHead(List<TruckSchedule_Coll> ts) {
        var dtsch = new DataTable();
        dtsch.Columns.Add(new DataColumn("DateSort", Type.GetType("System.DateTime")));
        dtsch.Columns.Add(new DataColumn("TruckLine"));
        dtsch.Columns.Add(new DataColumn("TruckNumber"));
        dtsch.Columns.Add(new DataColumn("Customer"));
        dtsch.Columns.Add(new DataColumn("GeneralDirection"));
        dtsch.Columns.Add(new DataColumn("SalesOrder"));
        dtsch.Columns.Add(new DataColumn("Weight"));
        dtsch.Columns.Add(new DataColumn("ID"));
        dtsch.Columns.Add(new DataColumn("Date"));

        for (var i = 0; i < ts.Count; i++) {
            DataRow drsch = dtsch.NewRow();
            try {
                drsch["DateSort"] = Convert.ToDateTime(ts[i].Date).ToShortDateString();
                drsch["Date"] = Convert.ToDateTime(ts[i].Date).ToShortDateString();
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
            }
            else {
                drsch["Weight"] = ts[i].Weight.Replace(",", "");
            }
            dtsch.Rows.Add(drsch);
        }
        var dvsch = new DataView(dtsch);
        return dvsch;
    }
    protected void dd_dateselected_Changed(object sender, EventArgs e) {
        CurrentSelectedDate = dd_MonthSelector.SelectedValue + "_" + dd_YearSelector.SelectedValue;
        LoadControls();
    }
    protected void btn_refresh_Click(object sender, EventArgs e) {
        LoadControls();
    }

    protected void hf_InsertItem_ValueChanged(object sender, EventArgs e) {
        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
        string insertItemStr = HttpUtility.UrlDecode(hf_InsertItem.Value);
        Dictionary<string, object> insertObj = (Dictionary<string, object>)js.Deserialize(insertItemStr, typeof(Dictionary<string, object>));

        _truckschedule.addItem(string.Empty, insertObj["TruckLine"].ToString(), insertObj["Date"].ToString(), insertObj["TruckNumber"].ToString(), insertObj["Customer"].ToString(),
                                   string.Empty, insertObj["OrderNumber"].ToString(), 0, insertObj["Direction"].ToString(), insertObj["Weight"].ToString(), string.Empty);

        UpdateUserFlags();
        LoadControls();
        hf_InsertItem.Value = string.Empty;
    }
    protected void hf_EditItem_ValueChanged(object sender, EventArgs e) {
        LoadControls();
        hf_EditItem.Value = string.Empty;
    }
    protected void hf_DeleteItem_ValueChanged(object sender, EventArgs e) {
        Guid id2 = Guid.Parse(hf_DeleteItem.Value);
        _truckschedule.deleteSlot(id2);
        UpdateUserFlags();

        LoadControls();
        hf_DeleteItem.Value = string.Empty;
    }
    protected void hf_UpdateItem_ValueChanged(object sender, EventArgs e) {
        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
        string updateItemStr = HttpUtility.UrlDecode(hf_UpdateItem.Value);
        Dictionary<string, object> updateObj = (Dictionary<string, object>)js.Deserialize(updateItemStr, typeof(Dictionary<string, object>));

        var id = new Guid();
        if (!string.IsNullOrEmpty(updateObj["ID"].ToString())) {
            id = Guid.Parse(updateObj["ID"].ToString());
        }
        _truckschedule.updateDate(id, updateObj["Date"].ToString());
        _truckschedule.updateTruckLine(id, updateObj["TruckLine"].ToString());
        _truckschedule.updateCustomerName(id, updateObj["Customer"].ToString());
        _truckschedule.updateOrderNumber(id, updateObj["OrderNumber"].ToString());
        _truckschedule.updateWeight(id, updateObj["Weight"].ToString());
        _truckschedule.updateUnit(id, updateObj["TruckNumber"].ToString());
        _truckschedule.updateGeneralDirection(id, updateObj["Direction"].ToString());

        UpdateUserFlags();
        LoadControls();
        hf_UpdateItem.Value = string.Empty;
    }

    protected void hf_LoadTodayTotals_ValueChanged(object sender, EventArgs e) {
        LoadControls();
        LoadTodayTotals();
        hf_LoadTodayTotals.Value = string.Empty;
    }

    #endregion

}