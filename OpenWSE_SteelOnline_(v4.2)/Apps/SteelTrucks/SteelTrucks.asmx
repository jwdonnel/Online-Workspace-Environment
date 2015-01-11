<%@ WebService Language="C#" Class="SteelTrucks" %>
#region

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;

#endregion

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class SteelTrucks : WebService {
    private const string AppId = "app-steeltrucks";
    private readonly AppLog _applog = new AppLog(false);
    private readonly IIdentity _userId;
    private TruckSchedule _truckSchedule;
    private readonly App _apps = new App();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();

    private string _username;
    private string _message;
    private string _group;

    public SteelTrucks() {
        _userId = HttpContext.Current.User.Identity;
    }

    [WebMethod]
    public string GetDriverList() {
        MemberDatabase _member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
        string _siteTheme = _member.SiteTheme;

        var ts = new TruckSchedule();
        var str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            str.Append("<div class='clear-space'></div><div id='sbar_steeltrucks'>");
            str.Append("<div class='clear-space'></div><div class='clear-margin'>");
            str.Append("<h3>List of Drivers</h3><div class='clear-space-five'></div>");
            if (ts.drivers_coll.Count == 0) {
                str.Append("There are no drivers available");
            }
            else {
                str.Append("<a href='#' onclick='ViewAllRecords();return false;' class='sb-links'><b><u>View All Schedules</u></b></a>");
                ts.drivers_coll.Sort();
                foreach (string curruser in ts.drivers_coll) {
                    string temp_curruser = curruser;
                    try {
                        temp_curruser = curruser.Replace("_", " ");
                    }
                    catch { }
                    string xdelete = "x_" + curruser.Replace("/", "--");
                    string xadd = "a_" + curruser.Replace("/", "--");
                    string xedit = "e_" + curruser.Replace("/", "--");
                    str.Append("<div id='expand_" + curruser.Replace("/", "--") + "' class='tsdiv' onmouseover=\"showButtons('" + xdelete + "','" + xadd + "','" + xedit + "')\"");
                    str.Append("onmouseout=\"hideButtons('" + xdelete + "','" + xadd + "','" + xedit + "')\">");
                    str.Append("<div class='tsdivclick'><span class='float-left' style='padding: 3px; font-size: 14px'>" + temp_curruser + "</span>");
                    str.Append("</div><a id='" + xadd + "' href='#addto' class='td-add-btn float-right margin-right-sml margin-top-sml imgstsDiv' title='Add New Schedule' onclick=\"LoadCreateNew('" + temp_curruser + "');return false;\" style='display: none; padding: 0px!important;'></a>");
                    str.Append("<a id='" + xdelete + "' href='#delete_" + curruser + "' title='Delete Driver' onclick='deleteUser(this);return false;' class='td-delete-btn float-right margin-right margin-top-sml imgstsDiv' style='display: none; padding: 0px!important;'></a>");
                    str.Append("<a id='" + xedit + "' href='#edit_" + curruser + "' title='Edit Driver' onclick=\"editUser(this,'" + curruser + "');return false;\" class='td-edit-btn float-right margin-right margin-top-sml imgstsDiv' style='display: none; padding: 0px!important;'></a>");
                    str.Append("</span></div>");
                }
            }
            str.Append("</div>");
        }
        return str.ToString();
    }

    [WebMethod]
    public string GetDriverListOptions() {
        var ts = new TruckSchedule();
        var str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (ts.drivers_coll.Count > 0) {
                str.Append("<option value='all'>All Drivers</option>");
                ts.drivers_coll.Sort();
                foreach (string curruser in ts.drivers_coll) {
                    str.Append("<option value='" + curruser + "'>" + curruser.Replace("_", " ") + "</option>");
                }
            }
        }
        return str.ToString();
    }

    [WebMethod]
    public string BuildWeightTotals() {
        int totalweight = 0;
        _truckSchedule = new TruckSchedule();
        var str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            str.Append("<h3><u>Weight Totals for Today</u></h3><div class='clear-space'></div>");
            var gd = new GeneralDirection(true);
            foreach (Dictionary<string, string> row in gd.generaldirection) {
                str.Append("<h4 class='pad-right float-left font-bold'>" + row["GeneralDirection"] + ":</h4><h4 class='float-left'>" + (_truckSchedule.calTotalWeightGD(DateTime.Now.ToShortDateString(), row["GeneralDirection"].ToString()).ToString("#,##0")) + " lbs</h4><div class='clear'></div>");
                totalweight += Convert.ToInt32(_truckSchedule.calTotalWeightGD(DateTime.Now.ToShortDateString(), row["GeneralDirection"].ToString()));
            }
            str.Append("<div class='clear' style='width: 50%; margin: 8px 0; border-top: 1px solid #939393;'></div><h3 style='color: #353535;'>&nbsp;<b>Total:&nbsp;&nbsp;</b>" + totalweight.ToString("#,##0") + " lbs</h3>");
            str.Append("<small>(Total = SMW Trucks + Common Carriers)</small><div class='clear-space'></div><div class='clear-space'></div>");
            str.Append("<input type='button' onclick='LoadGenDirEditor();' class='input-buttons float-left' value='Direction Edit' />");
        }
        return str.ToString();
    }

    [WebMethod]
    public object[] BuildGeneralDirections() {
        List<object> _array = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var gd = new GeneralDirection(true);
            foreach (Dictionary<string, string> row in gd.generaldirection) {
                string dir = row["GeneralDirection"].ToString();
                if (!_array.Contains(dir))
                    _array.Add(dir);
            }
        }
        return _array.ToArray();
    }

    [WebMethod]
    public string CallCreateNew(string _driver, string _date, string _dir, string _unit) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _truckSchedule = new TruckSchedule();
            _driver = _driver.Replace(" ", "_");
            string d = DateTime.Now.ToShortDateString();
            try {
                d = Convert.ToDateTime(_date).ToShortDateString();
            }
            catch { }
            _truckSchedule.addItem(_driver, string.Empty, d, _unit, string.Empty, string.Empty, string.Empty, 1, _dir, string.Empty, string.Empty);
            UpdateUserFlags();
        }
        return "";
    }

    [WebMethod]
    public object[] LoadScheduleList(string driver, string search, string recordstopull, string sortCol, string sortDir) {
        driver = HttpUtility.UrlDecode(driver);
        search = HttpUtility.UrlDecode(search);

        List<object> objectHolder = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (sortCol == "")
                sortCol = "Date";

            if (sortDir == "")
                sortDir = "DESC";

            _truckSchedule = new TruckSchedule(false, sortCol, sortDir);
            List<TruckSchedule_Coll> ts;
            if (!string.IsNullOrEmpty(driver))
                ts = _truckSchedule.getUserData(driver.Replace(" ", "_"), sortCol, sortDir);
            else {
                _truckSchedule = new TruckSchedule(true, sortCol, sortDir);
                ts = _truckSchedule.scheduler_coll;
            }

            List<TruckSchedule_Coll> truck_coll = _truckSchedule.getUserData_noDup(ts);

            int count = 50;
            int.TryParse(recordstopull, out count);

            int index = 0;
            if ((!string.IsNullOrEmpty(search.Trim())) && (search.Trim() != "Search for Truck Schedule")) {
                ts = new List<TruckSchedule_Coll>();
                foreach (var x in _truckSchedule.scheduler_coll) {
                    if (index >= count) {
                        if (recordstopull != "all")
                            break;
                    }

                    char[] trimBoth = { ' ' };
                    string findValue = search.ToLower().TrimEnd(trimBoth);
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
                        index++;
                    }
                }

                truck_coll = _truckSchedule.getUserData_noDup(ts);
            }

            index = 0;
            foreach (var x in truck_coll) {
                if (index >= count) {
                    if (recordstopull != "all")
                        break;
                }

                string weight = "N/A";
                if (!string.IsNullOrEmpty(x.Weight))
                    weight = _truckSchedule.calTotalWeightGD(x.Date, x.GeneralDirection, x.Unit, x.DriverName).ToString("#,##0");

                string entries = _truckSchedule.GetEntryCount(x.DriverName, x.Date, x.Unit).ToString();

                if (entries == "1") {
                    if ((string.IsNullOrEmpty(x.CustomerName)) && (string.IsNullOrEmpty(x.City))
                        && (string.IsNullOrEmpty(x.OrderNumber)) && (string.IsNullOrEmpty(x.Weight))) {
                        entries = "0";
                    }
                }

                string openEventClick = "OpenEvent('" + x.DriverName + "', '" + x.Date + "', '" + x.Unit + "', '" + x.GeneralDirection + "');";
                object[] _obj = new object[8];
                _obj[0] = openEventClick;
                _obj[1] = x.DriverName.Replace("_", " ");
                _obj[2] = x.Date;
                _obj[3] = x.LastUpdated;
                _obj[4] = x.Unit;
                _obj[5] = x.GeneralDirection;
                _obj[6] = weight;
                _obj[7] = entries;
                objectHolder.Add(_obj);
                index++;
            }
        }
        return objectHolder.ToArray();
    }

    [WebMethod]
    public string LoadEvent(string driver, string date, string unit, string dir) {
        _truckSchedule = new TruckSchedule(driver.Replace(" ", "_"), date, unit, dir);
        List<TruckSchedule_Coll> ts = _truckSchedule.scheduler_coll;
        StringBuilder str = new StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            // Build Header
            str.Append("<div id='EventSort-steeltrucks' class='margin-top-sml'><table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
            str.Append("<tr class='myHeaderStyle'>");
            str.Append("<td style='width: 55px;'></td>");
            str.Append("<td align='center'>Customer Name</td>");
            str.Append("<td align='center'>City of Delivery</td>");
            str.Append("<td align='center'>Order #</td>");
            str.Append("<td align='center'>Weight (lbs)</td>");
            str.Append("<td style='width: 75px;'></td>");
            str.Append("</tr>");


            // Build List Body
            int count = 0;
            for (int i = 0; i < ts.Count; i++) {
                string strWeight = "";
                int tempWeight;
                if (Int32.TryParse(ts[i].Weight.Replace(",", ""), out tempWeight))
                    strWeight = tempWeight.ToString("#,##0");
                else
                    strWeight = ts[i].Weight.Replace(",", "");

                bool canContinue = true;
                string colId = ts[i].ID.ToString();
                string editButton = "<a href='#' class='td-edit-btn margin-right edit-button-event' onclick='EditEvent(\"" + colId + "\");return false;' title='Edit'></a>";
                string deleteButton = "<a href='#' class='td-delete-btn delete-button-event' onclick='DeleteEvent(\"" + colId + "\",\"" + driver + "\",\"" + date + "\",\"" + unit + "\",\"" + dir + "\");return false;' title='Delete'></a>";
                if ((string.IsNullOrEmpty(ts[i].CustomerName)) && (string.IsNullOrEmpty(ts[i].City))
                    && (string.IsNullOrEmpty(ts[i].OrderNumber)) && (string.IsNullOrEmpty(ts[i].Weight))) {
                    count -= 1;
                    canContinue = false;
                }

                if (canContinue) {
                    str.Append("<tr class='GridNormalRow myItemStyle EventRow'>");
                    str.Append("<td id='" + colId + "-seq' class='GridViewNumRow border-bottom' align='center' style='width: 55px;'>" + (i + 1).ToString() + "</td>");
                    str.Append("<td id='" + colId + "-customer' class='border-right border-bottom non-moveable'>" + ts[i].CustomerName + "</td>");
                    str.Append("<td id='" + colId + "-city' class='border-right border-bottom non-moveable'>" + ts[i].City + "</td>");
                    str.Append("<td id='" + colId + "-order' class='border-right border-bottom non-moveable'>" + addWhiteSpace(ts[i].OrderNumber) + "</td>");
                    str.Append("<td id='" + colId + "-weight' class='border-right border-bottom non-moveable'>" + strWeight + "</td>");
                    str.Append("<td id='" + colId + "-actions' class='border-right border-bottom non-moveable' align='center' style='width: 75px;'>" + editButton + deleteButton + "</td>");
                    str.Append("</tr>");
                    count++;
                }
            }

            // Build Add New Item
            string addButton = "<a href='#' class='td-add-btn' onclick='AddEvent(\"" + driver + "\",\"" + date + "\",\"" + unit + "\",\"" + dir + "\");return false;' title='Add'></a>";
            string onKeyUp = "onkeyup='AddRecordKeyPress_SteelTrucks(event, \"" + driver + "\",\"" + date + "\",\"" + unit + "\",\"" + dir + "\")'";
            string customerTb = "<input type=\"text\" value=\"Customer Name\" id=\"tbCustomerNew\" class=\"textEntry customername-tb-autosearch\" " + onKeyUp + " onfocus=\"if(this.value=='Customer Name')this.value=''\" onblur=\"if(this.value=='')this.value='Customer Name'\" style=\"width:95%;\">";
            string cityTb = "<input type=\"text\" value=\"City of Delivery\" id=\"tbCityNew\" class=\"textEntry city-tb-autosearch\" " + onKeyUp + " onfocus=\"if(this.value=='City of Delivery')this.value=''\" onblur=\"if(this.value=='')this.value='City of Delivery'\" style=\"width:95%;\">";
            string ordernumTb = "<input type=\"text\" value=\"Order Number\" id=\"tbOrderNew\" class=\"textEntry\" " + onKeyUp + " onfocus=\"if(this.value=='Order Number')this.value=''\" onblur=\"if(this.value=='')this.value='Order Number'\" style=\"width:95%;\">";
            string weightTb = "<input type=\"text\" value=\"Weight\" id=\"tbWeightNew\" class=\"textEntry\" " + onKeyUp + " onfocus=\"if(this.value=='Weight')this.value=''\" onblur=\"if(this.value=='')this.value='Weight'\" style=\"width:95%;\">";

            str.Append("<tr class='GridNormalRow myItemStyle AddNewEventRow'>");
            str.Append("<td class='GridViewNumRow border-bottom' align='center' style='width: 55px;'></td>");
            str.Append("<td class='border-right border-bottom' align='center'>" + customerTb + "</td>");
            str.Append("<td class='border-right border-bottom' align='center'>" + cityTb + "</td>");
            str.Append("<td class='border-right border-bottom' align='center'>" + ordernumTb + "</td>");
            str.Append("<td class='border-right border-bottom' align='center'>" + weightTb + "</td>");
            str.Append("<td class='border-right border-bottom' align='center' style='width: 75px;'>" + addButton + "</td>");
            str.Append("</tr>");
            str.Append("</tbody></table></div>");

            str.Append("<div class='pad-all' align='right'><b class='pad-right'>TOTAL WEIGHT:</b>" + _truckSchedule.calTotalWeightGD(date, dir, unit, driver.Replace(" ", "_")).ToString("#,##0") + " lbs</div>");
        }
        return str.ToString();
    }

    [WebMethod]
    public string EventActionAdd(string _customer, string _city, string _order, string _weight, string _driver, string _date, string _unit, string _dir) {
        bool needAddNew = false;
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _truckSchedule = new TruckSchedule(_driver.Replace(" ", "_"), _date, _unit, _dir);
            if (_truckSchedule.scheduler_coll.Count == 1) {
                if ((string.IsNullOrEmpty(_truckSchedule.scheduler_coll[0].CustomerName)) && (string.IsNullOrEmpty(_truckSchedule.scheduler_coll[0].City))
                    && (string.IsNullOrEmpty(_truckSchedule.scheduler_coll[0].OrderNumber)) && (string.IsNullOrEmpty(_truckSchedule.scheduler_coll[0].Weight))
                    && (_truckSchedule.scheduler_coll[0].Sequence == 1)) {
                    Guid id = _truckSchedule.scheduler_coll[0].ID;
                    _truckSchedule.updateCustomerName(id, _customer.Trim());
                    _truckSchedule.updateCity(id, _city.Trim());
                    _truckSchedule.updateOrderNumber(id, _order.Trim());
                    _truckSchedule.updateWeight(id, _weight.Trim());
                }
                else
                    needAddNew = true;
            }
            else if (_truckSchedule.scheduler_coll.Count > 1)
                needAddNew = true;

            if (needAddNew) {
                _truckSchedule.addItem(_driver.Replace(" ", "_"), string.Empty, _date, _unit, _customer.Trim(),
                   _city.Trim(), _order.Trim(), _truckSchedule.scheduler_coll.Count + 1, _dir, _weight.Trim(), string.Empty);
            }

            UpdateUserFlags();
            return LoadEvent(_driver, _date, _unit, _dir);
        }
        return string.Empty;
    }

    [WebMethod]
    public string EventActionDelete(string _id, string _driver, string _date, string _unit, string _dir, string _eventList) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _truckSchedule = new TruckSchedule(_driver.Replace(" ", "_"), _date, _unit, _dir);
            List<TruckSchedule_Coll> ts = _truckSchedule.scheduler_coll;

            Guid id = Guid.Parse(_id);
            if (ts.Count <= 1) {
                _truckSchedule.updateCustomerName(id, "");
                _truckSchedule.updateCity(id, "");
                _truckSchedule.updateOrderNumber(id, "");
                _truckSchedule.updateWeight(id, "");
                _truckSchedule.updateSequence(id, "1");
            }
            else {
                _truckSchedule.deleteSlot(id);

                string[] eventList = _eventList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < eventList.Length; i++) {
                    _truckSchedule.updateSequence(Guid.Parse(eventList[i]), i.ToString());
                }
            }
            UpdateUserFlags();
            return LoadEvent(_driver, _date, _unit, _dir);
        }
        return string.Empty;
    }

    [WebMethod]
    public string EventActionEditUpdate(string _id, string _driver, string _date, string _unit,
        string _dir, string _customer, string _city, string _order, string _weight, string _eventList) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _truckSchedule = new TruckSchedule(false);

            Guid id = Guid.Parse(_id);
            _truckSchedule.updateCustomerName(id, _customer);
            _truckSchedule.updateCity(id, _city);
            _truckSchedule.updateOrderNumber(id, _order);
            _truckSchedule.updateWeight(id, _weight);

            _truckSchedule = new TruckSchedule(_driver.Replace(" ", "_"), _date, _unit, _dir);
            List<TruckSchedule_Coll> ts = _truckSchedule.scheduler_coll;

            string[] eventList = _eventList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < eventList.Length; i++) {
                _truckSchedule.updateSequence(Guid.Parse(eventList[i]), i.ToString());
            }

            UpdateUserFlags();
            return LoadEvent(_driver, _date, _unit, _dir);
        }
        return string.Empty;
    }

    [WebMethod]
    public string DriverEdit(string _case, string _driver, string _oldname, string _newname) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            _driver = HttpUtility.UrlDecode(_driver);
            _oldname = HttpUtility.UrlDecode(_oldname);
            _newname = HttpUtility.UrlDecode(_newname);

            _truckSchedule = new TruckSchedule();
            switch (_case) {
                case "deleteUser":
                    _truckSchedule.deleteSlots(_driver);
                    break;
                case "updateUser":
                    if ((_newname.ToLower() != "create new folder") && (!string.IsNullOrEmpty(_newname)) && (_newname.ToLower() != "root directory")) {
                        // _newname = removeRegex(_newname);
                        _truckSchedule.updateDriverName(_oldname, _newname);
                    }
                    break;
            }

            return GetDriverList();
        }
        return string.Empty;
    }

    [WebMethod]
    public object[] EventHeaderEdit(string _Orgdriver, string _Orgdate, string _Orgunit, string _Orgdir, string _date, string _unit, string _dir) {
        object[] returnVals = new object[3];
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var ts = new TruckSchedule(_Orgdriver.Replace(" ", "_"), _Orgdate, _Orgunit, _Orgdir);

            // Update Dates
            if ((!string.IsNullOrEmpty(_date)) && (_date != _Orgdate)) {
                foreach (var t in ts.scheduler_coll) {
                    ts.updateDate(t.ID, _date);
                }
                returnVals[0] = _date;
            }
            else
                returnVals[0] = _Orgdate;


            // Update Units
            if ((!string.IsNullOrEmpty(_unit)) && (_unit != _Orgunit)) {
                foreach (var t in ts.scheduler_coll) {
                    ts.updateUnit(t.ID, _unit);
                }
                returnVals[1] = _unit;
            }
            else
                returnVals[1] = _Orgunit;


            if ((!string.IsNullOrEmpty(_dir)) && (_dir != _Orgdir)) {
                foreach (var t in ts.scheduler_coll) {
                    ts.updateGeneralDirection(t.ID, _dir);
                }
                returnVals[2] = _dir;
            }
            else
                returnVals[2] = _Orgdir;
        }
        return returnVals;
    }

    [WebMethod]
    public void EventHeaderDelete(string _driver, string _date, string _unit, string _dir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var ts = new TruckSchedule(_driver.Replace(" ", "_"), _date, _unit, _dir);
            foreach (var t in ts.scheduler_coll) {
                ts.deleteSlot(t.ID);
            }
            UpdateUserFlags();
        }
    }

    [WebMethod]
    public string ExportToExcel(string _date, string _driver, string _unit, string _dir) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _Path = "SteelTrucks_Driver_" + _driver.Replace(" ", "_") + "-" + _date.Replace("/", "_") + ".xls";
            string directory = ServerSettings.GetServerMapLocation + "Apps\\SteelTrucks\\Exports";
            string p = Path.Combine(directory, _Path);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var temp = new DataTable();
            try {
                _truckSchedule = new TruckSchedule(_driver.Replace(" ", "_"), _date, _unit, _dir);
                if (_truckSchedule.scheduler_coll.Count == 0) {
                    return "";
                }
                else {
                    temp.Columns.Add(new DataColumn("Date"));
                    temp.Columns.Add(new DataColumn("Driver"));
                    temp.Columns.Add(new DataColumn("Unit"));
                    temp.Columns.Add(new DataColumn("Direction"));
                    temp.Columns.Add(new DataColumn("Customer"));
                    temp.Columns.Add(new DataColumn("City"));
                    temp.Columns.Add(new DataColumn("OrderNumber"));
                    temp.Columns.Add(new DataColumn("Weight"));

                    foreach (TruckSchedule_Coll x in _truckSchedule.scheduler_coll) {
                        DataRow drsch = temp.NewRow();
                        drsch["Date"] = x.Date;
                        drsch["Driver"] = x.DriverName.Replace("_", " ");
                        drsch["Unit"] = x.Unit;
                        drsch["Direction"] = x.GeneralDirection;
                        drsch["Customer"] = x.CustomerName;
                        drsch["City"] = x.City;
                        drsch["OrderNumber"] = x.OrderNumber;
                        drsch["Weight"] = x.Weight;
                        temp.Rows.Add(drsch);
                    }
                }

                var FI = new FileInfo(p);
                var stringWriter = new StringWriter();
                var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWriter);
                var DataGrd = new DataGrid();
                DataGrd.DataSource = temp;
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

                return "Apps/SteelTrucks/Exports/" + _Path;
            }
            catch (Exception ex) {
                return "";
            }
        }
        return string.Empty;
    }

    [WebMethod]
    public string ExportToExcelAll(string _dateFrom, string _dateTo, string _driver) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string _Path = "SteelTrucks-" + _driver.Replace(" ", "_") + "-" + _dateFrom.Replace("/", "_") + "-" + _dateTo.Replace("/", "_") + ".xls";
            string directory = ServerSettings.GetServerMapLocation + "Apps\\SteelTrucks\\Exports";
            string p = Path.Combine(directory, _Path);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var temp = new DataTable();
            try {
                temp.Columns.Add(new DataColumn("Date"));
                temp.Columns.Add(new DataColumn("Driver"));
                temp.Columns.Add(new DataColumn("Unit"));
                temp.Columns.Add(new DataColumn("Direction"));
                temp.Columns.Add(new DataColumn("Customer"));
                temp.Columns.Add(new DataColumn("City"));
                temp.Columns.Add(new DataColumn("OrderNumber"));
                temp.Columns.Add(new DataColumn("Weight"));

                DateTime StartDate = Convert.ToDateTime(_dateFrom);
                DateTime EndDate = Convert.ToDateTime(_dateTo);

                foreach (DateTime day in EachDay(StartDate, EndDate)) {
                    _truckSchedule = new TruckSchedule(day.ToShortDateString());
                    if (_truckSchedule.scheduler_coll.Count > 0) {
                        foreach (TruckSchedule_Coll x in _truckSchedule.scheduler_coll) {
                            if ((x.DriverName.ToLower().Replace(" ", "_") == _driver.ToLower().Replace(" ", "_")) || (_driver.ToLower() == "all")) {
                                DataRow drsch = temp.NewRow();
                                drsch["Date"] = x.Date;
                                drsch["Driver"] = x.DriverName.Replace("_", " ");
                                drsch["Unit"] = x.Unit;
                                drsch["Direction"] = x.GeneralDirection;
                                drsch["Customer"] = x.CustomerName;
                                drsch["City"] = x.City;
                                drsch["OrderNumber"] = x.OrderNumber;
                                drsch["Weight"] = x.Weight;
                                temp.Rows.Add(drsch);
                            }
                        }
                    }
                }

                if (temp.Rows.Count == 0)
                    return "";

                var FI = new FileInfo(p);
                var stringWriter = new StringWriter();
                var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWriter);

                DataView dv = temp.DefaultView;
                dv.Sort = "Date ASC, Driver ASC";

                var DataGrd = new DataGrid();
                DataGrd.DataSource = dv.Table;
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

                return "Apps/SteelTrucks/Exports/" + _Path;
            }
            catch (Exception ex) {
                return "File path wrong.";
            }
        }
        return string.Empty;
    }

    public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru) {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }

    private void UpdateUserFlags() {
        MemberDatabase m;
        foreach (string username in from MembershipUser user in Membership.GetAllUsers() select user.UserName) {
            m = new MemberDatabase(username);
            bool hasapp1 = m.UserHasApp(AppId);
            bool hasapp2 = m.UserHasApp("app-dailyoverview");
            if ((hasapp1) && (username.ToLower() != HttpContext.Current.User.Identity.Name.ToLower())) {
                _uuf.addFlag(username, AppId, "");
            }
            if (hasapp2) {
                _uuf.addFlag(username, "app-dailyoverview", "");
            }
        }
    }

    private string removeRegex(string name) {
        string fnew1 = name.Replace("'", "");
        string fnew1_temp = fnew1;
        fnew1 = fnew1_temp.Replace("&", "and");
        string fnew2_temp = fnew1;
        fnew1 = fnew2_temp.Replace("%", "");
        string fnew3_temp = fnew1;
        fnew1 = fnew3_temp.Replace(">", "");
        string fnew4_temp = fnew1;
        fnew1 = fnew4_temp.Replace("<", "");
        string fnew5_temp = fnew1;
        fnew1 = fnew5_temp.Replace("/", "");
        string fnew6_temp = fnew1;
        fnew1 = fnew6_temp.Replace(" ", "_");
        string fnew7_temp = fnew1;
        fnew1 = System.Text.RegularExpressions.Regex.Replace(fnew7_temp, @"<(.|\n)*?>", string.Empty);
        return fnew1;
    }

    private string addWhiteSpace(string x) {
        string ret = x.Replace(",", ", ");
        ret = ret.Replace(".", ", ");
        return ret;
    }
}