var table_Days_Height_wc = 0;
var date_wc;
var month_wc = new Array(12);
var selected_m_wc = "";
var selected_d_wc = "";
var selected_y_wc = "";
month_wc[0] = "January";
month_wc[1] = "February";
month_wc[2] = "March";
month_wc[3] = "April";
month_wc[4] = "May";
month_wc[5] = "June";
month_wc[6] = "July";
month_wc[7] = "August";
month_wc[8] = "September";
month_wc[9] = "October";
month_wc[10] = "November";
month_wc[11] = "December";

var calViewType_wc = "calendar";

var reloadingSchedule_wc = false;
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    if (calViewType_wc == "overview") {
        $("#usercalendar_wc").hide();
        $("#usercalendar_wc").html("");

        GetSidebarOverview_wc();
        $("#useroverview_wc").show();
    }
    else {
        $("#useroverview_wc").hide();

        onCalendarLoad_wc();
        $("#usercalendar_wc").show();
    }

    RestorePostBack_wc();

    getPendingRequests(true);
    $("#update-element-wc").remove();
});

var temp_startDate_wc = "";
var temp_endDate_wc = "";
var temp_startTime_wc = "";
var temp_endTime_wc = "";
var temp_startampm = "";
var temp_endampm_wc = "";
var temp_employee_wc = "";
var temp_reason_wc = "";
var temp_description_wc = "";
var temp_hours_wc = "";
prm.add_beginRequest(function (sender, args) {
    if ($("#NewCalEvent-element_wc").css("display") == "block") {
        temp_startDate_wc = $("#app-workcalendar").find("#tb_newevent_start_wc").val();
        temp_endDate_wc = $("#app-workcalendar").find("#tb_newevent_end_wc").val();
        temp_startTime_wc = $("#app-workcalendar").find("#dd_newevent_starttime_wc").val();
        temp_endTime_wc = $("#app-workcalendar").find("#dd_newevent_endtime_wc").val();
        temp_startampm = $("#app-workcalendar").find("#dd_newevent_starttime_wc_ampm").val();
        temp_endampm_wc = $("#app-workcalendar").find("#dd_newevent_endtime_wc_ampm").val();
        temp_employee_wc = $("#app-workcalendar").find("#dd_employees_wc").val();
        if (temp_employee_wc.toLowerCase() == "loading...") {
            temp_description_wc = "";
        }

        temp_reason_wc = $("#app-workcalendar").find("#dd_reason_wc").val();
        temp_description_wc = $("#app-workcalendar").find("#tb_newevent_desc_wc").val();
        temp_hours_wc = $("#app-workcalendar").find("#tb_hours").val();
    }
});

function RestorePostBack_wc() {
    if ($("#NewCalEvent-element_wc").css("display") == "block") {
        $("#app-workcalendar").find("#tb_newevent_start_wc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker("option", "minDate", selectedDate);
            },
            onSelect: function (dataText, inst) {
                UpdateTotalHours_wc();
            }
        });
        $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onSelect: function (dataText, inst) {
                UpdateTotalHours_wc();
            }
        });


        if (temp_startTime_wc != "") {
            $("#app-workcalendar").find("#tb_newevent_start_wc").val(temp_startDate_wc);
            $("#app-workcalendar").find("#tb_newevent_end_wc").val(temp_endDate_wc);
            $("#app-workcalendar").find("#dd_newevent_starttime_wc").val(temp_startTime_wc);
            $("#app-workcalendar").find("#dd_newevent_endtime_wc").val(temp_endTime_wc);
            $("#app-workcalendar").find("#dd_newevent_starttime_wc_ampm").val(temp_startampm);
            $("#app-workcalendar").find("#dd_newevent_endtime_wc_ampm").val(temp_endampm_wc);
            $("#app-workcalendar").find("#dd_employees_wc").val(temp_employee_wc);
            $("#app-workcalendar").find("#dd_reason_wc").val(temp_reason_wc);
            $("#app-workcalendar").find("#tb_newevent_desc_wc").val(temp_description_wc);
            $("#app-workcalendar").find("#tb_hours").val(temp_hours_wc);

            $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker("option", "minDate", temp_startDate_wc);

            temp_startDate_wc = "";
            temp_endDate_wc = "";
            temp_startTime_wc = "";
            temp_endTime_wc = "";
            temp_startampm = "";
            temp_endampm_wc = "";
            temp_employee_wc = "";
            temp_reason_wc = "";
            temp_description_wc = "";
            temp_hours_wc = "";
        }
    }
}

var isPrintMode = false;
$(document).ready(function () {
    var parm1 = getParameterByName("month");
    var parm2 = getParameterByName("year");
    var parm3 = getParameterByName("group");
    if (parm1 != "" && parm2 != "" && parm1 != null && parm2 != null) {
        selected_m_wc = parm1;
        selected_y_wc = parm2;
        $("body").append("<div id='rendering-print'><div class='text'>Rendering Printable Calendar<br />Please wait...</div></div>");
        $("#wCal-titlebar, .pc-menu-bar").hide();
        $("#print-view-date").html("<h2 class='pad-all'>" + month_wc[parseInt(parm1)] + " " + parm2 + "</h2>");
        $("#print-view-date").show();
        if (parm3 != "" && parm3 != null) {
            $("#group_select_wc").val(parm3);
        }
        isPrintMode = true;
    }

    if (calViewType_wc == "overview") {
        $("#usercalendar_wc").hide();
        $("#usercalendar_wc").html("");

        GetSidebarOverview_wc();
        $("#useroverview_wc").show();
    }
    else {
        $("#useroverview_wc").hide();

        onCalendarLoad_wc();
        $("#usercalendar_wc").show();
    }

    getPendingRequests(true);
});

$(window).resize(function () {
    onCalendarLoad_wc();
});

$(document.body).on("click", "#app-workcalendar .maximize-button-app", function () {
    onCalendarLoad_wc();
});

$(document.body).on("dblclick", "#app-workcalendar .app-head-dblclick", function () {
    onCalendarLoad_wc();
});

$(document.body).on("change", "#overview-view-wc", function () {
    calViewType_wc = $(this).val();
    if ($("#workcalendar-load").length > 0) {
        if (calViewType_wc == "overview") {
            $("#usercalendar_wc").hide();
            $("#usercalendar_wc").html("");

            GetSidebarOverview_wc();
            $("#useroverview_wc").show();
        }
        else {
            $("#useroverview_wc").hide();

            onCalendarLoad_wc();
            $("#usercalendar_wc").show();
        }
    }
});

$(document.body).on("change", "#group_select_wc", function () {
    if ($("#workcalendar-load").length > 0) {
        if (calViewType_wc == "overview") {
            GetSidebarOverview_wc();
        }
        else {
            onCalendarLoad_wc();
        }
        getPendingRequests(true);
    }
});

function GetSidebarOverview_wc() {
    LoadingWorkCalendar("Loading...");
    $(".font-year-wc").html("");
    $("#sidebar-caloverview-wc").html("Loading...");
    $("#pnl_cancelled_wc").html("Loading...");
    $("#pnl_offtoday_wc").html("Loading...");
    $("#pnl_offtomorrow_wc").html("Loading...");
    $("#pnl_recentrejected_wc").html("Loading...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/Loadce_Overview",
        type: "POST",
        data: '{ "group": "' + $("#group_select_wc").val() + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            var str = "<h3 class='font-bold'>User Vacation Time</h3><div class='clear-space-five'></div>" + response[0];
            str += "<div class='clear' style='height: 25px;'></div><h3 class='font-bold'>Hours Remaining</h3><div class='clear-space-five'></div>" + response[1];
            $("#sidebar-caloverview-wc").html(str);

            $.ajax({
                url: "Apps/WorkCalendar/WorkCalendar.asmx/BuildTextBase",
                type: "POST",
                data: '{ "group": "' + $("#group_select_wc").val() + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    $("#pnl_cancelled_wc").html(response[0]);
                    $("#pnl_offtoday_wc").html(response[1]);
                    $("#pnl_offtomorrow_wc").html(response[2]);
                    $("#pnl_recentrejected_wc").html(response[3]);
                    $("#update-element-wc").remove();
                }
            });
        }
    });
}

function KeyPressSearch_wc(event) {
    try {
        if (event.which == 13) {
            onCalendarLoad_wc();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            onCalendarLoad_wc();
        }
        delete evt;
    }
}

function HideTitleSearch_wc(_this) {
    if ($("#wCal-titlebar").is(":visible")) {
        $(_this).html("Show Title/Search");
        $("#wCal-titlebar").hide();
    }
    else {
        $(_this).html("Hide Title/Search");
        $("#wCal-titlebar").show();
    }

    if ($("#workcalendar-load").length > 0) {
        onCalendarLoad_wc();
    }
}




/*****************************************************/
/* Pending Approvals
/*****************************************************/
function getPendingRequests(count) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/GetPendingRequests",
        type: "POST",
        data: '{ "group": "' + $("#group_select_wc").val() + '","countOnly": "' + count.toString() + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (count) {
                CountPending_wc(response);
            }
            else {
                CountPending_wc(response);

                var str = "";
                for (var i = 0; i < response.length; i++) {
                    str += response[i];
                }

                if (str == "") {
                    str = "<h4 class='pad-all'>No pending approvals. Check back later.</h4>";
                }

                $("#pendingapprovallist_wc").html(str);
                openWSE.LoadModalWindow(true, 'RequestApproval-element_wc', 'Pending Requests');
            }
        }
    });
}

function CountPending_wc(response) {
    if (response.length > 0) {
        $("#menubtn_requests").addClass("activeIn");
        $("#menubtn_requests").attr("title", response.length + " pending request(s)");
    }
    else {
        $("#menubtn_requests").attr("title", "0 pending requests");
        $("#menubtn_requests").removeClass("activeIn");
    }
}

function LoadRequestApprovalModal() {
    if ($("#RequestApproval-element_wc").is(":visible")) {
        openWSE.LoadModalWindow(false, 'RequestApproval-element_wc', '');
        $("#pendingapprovallist_wc").html("");
    }
    else {
        getPendingRequests(false);
    }
}


function ApproveRequest(id) {
    LoadingWorkCalendar("Approving. Please Wait...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/ApproveRequest",
        type: "POST",
        data: '{ "group": "' + $("#group_select_wc").val() + '","id": "' + id + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            var str = "";
            for (var i = 0; i < response.length; i++) {
                str += response[i];
            }

            if (str == "") {
                str = "<h4 class='pad-all'>No pending approvals. Check back later.</h4>";
            }

            $("#pendingapprovallist_wc").html(str);
            $("#update-element-wc").remove();

            if (calViewType_wc == "overview") {
                GetSidebarOverview_wc();
            }
            else {
                onCalendarLoad_wc();
            }

            getPendingRequests(true);
        }
    });
}

function RejectRequest(id) {
    LoadingWorkCalendar("Rejecting. Please Wait...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/RejectRequest",
        type: "POST",
        data: '{ "group": "' + $("#group_select_wc").val() + '","id": "' + id + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            var str = "";
            for (var i = 0; i < response.length; i++) {
                str += response[i];
            }

            if (str == "") {
                str = "<h4 class='pad-all'>No pending approvals. Check back later.</h4>";
            }

            $("#pendingapprovallist_wc").html(str);
            $("#update-element-wc").remove();

            if (calViewType_wc == "overview") {
                GetSidebarOverview_wc();
            }
            else {
                onCalendarLoad_wc();
            }

            getPendingRequests(true);
        }
    });
}




/*****************************************************/
/* Build the calendar
/*****************************************************/
function onCalendarLoad_wc() {
    if ($("#workcalendar-load").length > 0) {
        if (calViewType_wc == "calendar") {
            LoadingWorkCalendar("Loading...");
            $("#app-workcalendar").resizable({
                stop: function (event, ui) {
                    onCalendarLoad_wc();
                }
            });

            var d_temp = new Date();
            if ((selected_m_wc == "") && (selected_y_wc == "")) {
                buildCalendar_wc(d_temp.getMonth(), d_temp.getFullYear());
                GetMonthInfo_wc(d_temp.getMonth(), d_temp.getFullYear());
            }
            else {
                buildCalendar_wc(selected_m_wc, selected_y_wc);
                GetMonthInfo_wc(selected_m_wc, selected_y_wc);
            }
        }
    }
}



/*****************************************************/
/* Build Monthy Calendar
/*****************************************************/
function buildCalendar_wc(m, y) {
    $("#printbtn").attr("href", openWSE.siteRoot() + "ExternalAppHolder.aspx?appId=app-workcalendar&hidetoolbar=true&print=true&month=" + m + "&year=" + y + "&group=" + $("#group_select_wc").val());

    LoadingWorkCalendar("Loading...");
    date_wc = new Date(y, m, 1, new Date().getHours(), new Date().getMinutes(), new Date().getSeconds(), new Date().getMilliseconds());

    var ddMonth = "<select id='dd_monthSelect_wc' class='margin-right-sml float-right' onchange='onMonthYearChange_wc()'>";
    for (var i = 0; i < month_wc.length; i++) {
        var selected = "";
        if (date_wc.getMonth() == i) {
            selected = " selected='selected'";
        }
        ddMonth += "<option value='" + i + "' " + selected + ">" + month_wc[i] + "</option>";
    }
    ddMonth += "</select>";

    var ddYear = "<select id='dd_yearSelect_wc' class='margin-left-sml float-right' onchange='onMonthYearChange_wc()'>";
    for (var i = 2004; i < 2024; i++) {
        var selected = "";
        if (date_wc.getFullYear() == i) {
            selected = " selected='selected'";
        }
        ddYear += "<option value='" + i + "' " + selected + ">" + i + "</option>";
    }
    ddYear += "</select>";

    var prevBtn = "<a href='#' onclick='onPrevClick_wc();return false;' class='pg-prev-btn margin-right float-right' title='Previous Month' style='margin-top: 1px;'></a>";
    var nextBtn = "<a href='#' onclick='onNextClick_wc();return false;' class='pg-next-btn margin-left float-right' title='Next Month' style='margin-top: 1px;'></a>";
    $("#currmonthBtns_wc").html("<div class='font-year-wc pad-top-sml'>" + nextBtn + ddYear + " " + ddMonth + prevBtn + "</div>");

    var h = $("#app-workcalendar").height() - $("#app-workcalendar .app-head").height();
    table_Days_Height_wc = h - ($("#app-workcalendar").find(".pc-menu-bar").height() + $("#wCal-titlebar").height());
    var str = "<table cellpadding='0' cellspacing='0' style='width:100%;height:" + table_Days_Height_wc + "px'><tbody><tr class='days-of-week'><td>Sun</td><td>Mon</td><td>Tue</td><td>Wed</td><td>Thur</td><td>Fri</td><td>Sat</td></tr>";
    var dinm = daysInMonth_wc(date_wc.getMonth(), date_wc.getFullYear());
    var count = 1;
    for (var r = 0; r < 6; r++) {
        str += "<tr class='table-Days'>";
        for (var c = 0; c < 7; c++) {
            if (count == dinm) {
                str += "<td class='not-apart-of-month'></td>";
            } else {
                if (count == 1) {
                    if (c == findStartofCal_wc()) {
                        str += createDay_wc(date_wc.getMonth(), count, date_wc.getFullYear());
                        count++;
                    } else {
                        str += "<td class='not-apart-of-month'></td>";
                    }
                } else {
                    str += createDay_wc(date_wc.getMonth(), count, date_wc.getFullYear());
                    count++;
                }

            }
        }
        str += "</tr>";
    }
    str += "</tbody></table>";
    $("#usercalendar_wc").html(str);
}

function createDay_wc(m, d, y) {
    var Now = new Date();
    var hf_d = document.getElementById("dateselected_wc");
    var returnStr = "";
    if (hf_d != null) {
        if (hf_d.value != "") {
            var sd = hf_d.value.split("_");
            if (Now.getMonth() == m && Now.getDate() == d && Now.getFullYear() == y) {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_wc(this);'><span class='current-day' title='Click to add event'>" + d + "</span></span></td>";
            }
            else {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_wc(this);' title='Click to add event'>" + d + "</span></td>";
            }
        }
        else {
            if (Now.getMonth() == m && Now.getDate() == d && Now.getFullYear() == y) {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_wc(this);'><span class='current-day' title='Click to add event'>" + d + "</span></span></td>";
            }
            else {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_wc(this);' title='Click to add event'>" + d + "</span></td>";
            }
        }
    }

    return returnStr;
}

function GetMonthInfo_wc(month, year) {
    if (!reloadingSchedule_wc) {
        reloadingSchedule_wc = true;
        selected_m_wc = month;
        selected_y_wc = year;
        viewtype = $("#app-workcalendar").find("#hf_viewtype_vacationentry").val();
        if ((viewtype == null) || (viewtype == "")) {
            viewtype = "all";
        }

        var search = $.trim($("#tb_search_wcal").val());
        if (search == "Search Current Month") {
            search = "";
        }

        // $("#lnk-printCalendar-workcal").attr("href", openWSE.siteRoot() + "Apps/WorkCalendar/PrintCalendar.aspx?group=" + $("#group_select_wc").val() + "&month=" + month + "&year=" + year + "&search=" + search.toLowerCase() + "&height=");

        $.ajax({
            url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/GetMonthInfo",
            type: "POST",
            data: '{ "month": "' + month + '","year": "' + year + '","group": "' + $("#group_select_wc").val() + '","search": "' + search.toLowerCase() + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                if (response.length > 0) {
                    var prevId = "";
                    for (var i = 0; i < response.length; i++) {
                        if (response[i].length == 6) {
                            var sameId = false;
                            var startDate = response[i][2];
                            var len = response[i][3];
                            var tdWidth = $("#app-workcalendar").find("#" + startDate).width();

                            var bufferLen = len;
                            if (len > 1) {
                                var $nextTd = $("#app-workcalendar").find("#" + startDate);
                                for (var j = 0; j < len - 1; j++) {
                                    if ($nextTd.length > 0) {
                                        if (!$nextTd.hasClass("not-apart-of-month")) {
                                            tdWidth += $nextTd.width();
                                            $nextTd = $nextTd.next("td");
                                        }
                                        else {
                                            bufferLen--;
                                        }
                                        if (prevId == response[i][0]) {
                                            sameId = true;
                                        }

                                        prevId = response[i][0];
                                    }
                                }

                                tdWidth += bufferLen - 1;
                            }

                            var eventDiv = BuildCalendarEvent(response[i][0], response[i][1], tdWidth, response[i][4], response[i][5], len, sameId);
                            $("#app-workcalendar").find("#" + startDate).append(eventDiv);
                        }
                    }

                    SetMarginEvents_wc();
                    SetTDHeights_wc();
                }

                reloadingSchedule_wc = false;
                $("#update-element-wc").remove();

                if (isPrintMode) {
                    setTimeout(function () {
                        $("#wCal-titlebar, .pc-menu-bar").remove();
                        $("#container, .main-div-app-bg").css("position", "relative");
                        $("#workcalendar-printview").css("height", $("#workcalendar-printview").height());
                        html2canvas(document.getElementById("workcalendar-printview"), {
                            onrendered: function (canvas) {
                                document.body.appendChild(canvas);
                                $("canvas").attr("id", "canvas");
                                $("#app-workcalendar").hide();
                                $("#rendering-print .text").html("Finished. Click <a id='download_canvas' href='#'>HERE</a> to Download.");
                                document.getElementById("download_canvas").addEventListener("click", function () {
                                    var fileName = "WorkCalendar_" + (parseInt(selected_m_wc) + 1) + "-" + selected_y_wc + ".jpg";
                                    downloadCanvas(this, "canvas", fileName);
                                }, false);
                            }
                        });
                    }, 500);
                }
            }
        });
    }
}

function SetMarginEvents_wc() {
    var td_width = 0;
    var length_temp = 0;
    var bufferMargin = 2;
    $("#app-workcalendar").find(".table-Days").each(function (index) {
        var margintop = bufferMargin;
        var margintopnext = bufferMargin;
        var index2_temp = 0;
        var dayLength = 0;
        $(this).find("td").each(function (index2) {
            if (index2_temp > dayLength) {
                margintop = bufferMargin;
            }
            else {
                margintop = margintopnext;
            }
            td_width = $(this).width();
            $(this).find(".calendar-event").each(function (index3) {
                $(this).css("margin-top", margintop + "px");
                var event_width = $(this).width();
                var event_height = $(this).outerHeight();
                var length = Math.round(event_width / td_width);
                margintop += event_height;
                if ((length > 1) && (length_temp >= index3)) {
                    length_temp = length;
                    dayLength = length + index2;
                    if ((index2_temp == 1) && (length_temp == 2)) {
                        margintopnext = bufferMargin;
                    }
                    else {
                        margintopnext += event_height;
                    }
                }
                else {
                    if (index2_temp >= length_temp) {
                        margintopnext = bufferMargin;
                    }
                    index2_temp++;
                }
            });
        });
    });
    DisplayEvents_wc();
}

function SetTDHeights_wc() {
    $("#app-workcalendar").find(".table-Days").each(function (index) {
        var $this = $(this).find(".calendar-event");
        if ($this.length > 0) {
            var total_height = 0;
            var marginList = new Array();
            $this.each(function (index) {
                var marginTopVal = parseInt($this.eq(index).css("margin-top").replace("px", ""));
                if (!ContainsMarginValue(marginList, marginTopVal)) {
                    marginList.push(marginTopVal);
                    total_height += $this[index].clientHeight;
                }
            });
            total_height += 40;
            $(this).css("height", total_height + "px");
        }
    });
}
function ContainsMarginValue(arr, val) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == val) {
            return true;
        }
    }

    return false;
}

function BuildCalendarEvent(id, usercolor, width, reason, daterange, length, sameId) {
    var str = "";
    if ((length > 0) && (($("." + id).length == 0) || (sameId))) {
        str += "<div class='calendar-event " + id + "' style='background: " + usercolor + "; width: " + width + "px;' onclick=\"ViewNewCalendarEvent_wc('" + id + "');\" title='" + reason.replace("/n", " ") + "'>";
        var _reason = reason.replace("/n", " - ");
        if (length == 1) {
            _reason = reason.replace("/n", "<br />");
        }
        str += "<div class='calendar-event-font inline-block'>" + _reason + "</div></div>";
    }
    return str;
}

function downloadCanvas(link, canvasId, filename) {
    link.href = document.getElementById(canvasId).toDataURL();
    link.download = filename;
}


/*****************************************************/
/* Universal functions
/*****************************************************/
function daysInMonth_wc(m, y) {
    return new Date(y, m + 1, 0).getDate() + 1;
}

function findStartofCal_wc() {
    var d = new Date(month_wc[date_wc.getMonth()] + "1, " + date_wc.getFullYear());
    return d.getDay();
}

function onNextClick_wc() {
    // $("#usercalendar_wc").hide();
    var m = parseInt($("#app-workcalendar").find("#dd_monthSelect_wc").val());
    var y = parseInt($("#app-workcalendar").find("#dd_yearSelect_wc").val());
    if (m == 11) {
        y = y + 1;
        m = 0;
    }
    else {
        m = m + 1;
    }

    buildCalendar_wc(m, y);
    GetMonthInfo_wc(m, y);

    // $("#usercalendar_wc").show();
}

function onPrevClick_wc() {
    // $("#usercalendar_wc").hide();
    var m = parseInt($("#app-workcalendar").find("#dd_monthSelect_wc").val());
    var y = parseInt($("#app-workcalendar").find("#dd_yearSelect_wc").val());
    if (m == 0) {
        y = y - 1;
        m = 11;
    }
    else {
        m = m - 1;
    }

    buildCalendar_wc(m, y);
    GetMonthInfo_wc(m, y);

    // $("#usercalendar_wc").show();
}

function onMonthYearChange_wc() {
    // $("#usercalendar_wc").hide();
    var m = parseInt($("#app-workcalendar").find("#dd_monthSelect_wc").val());
    var y = parseInt($("#app-workcalendar").find("#dd_yearSelect_wc").val());

    buildCalendar_wc(m, y);
    GetMonthInfo_wc(m, y);

    // $("#usercalendar_wc").show();
}

function onDayClick_wc(_this) {
    var Now = new Date();
    var hidden_date_wc = document.getElementById("hidden_date_wc");
    hidden_date_wc.value = "";
    var temp = new Date(date_wc.getFullYear(), date_wc.getMonth(), (parseInt(_this.innerHTML) + 1), date_wc.getHours(), date_wc.getMinutes(), date_wc.getSeconds(), date_wc.getMilliseconds());
    var hf_d = document.getElementById("dateselected_wc");
    hf_d.value = date_wc.getMonth() + "_" + _this.innerText + "_" + date_wc.getFullYear();
    var curr = (date_wc.getMonth() + 1) + "/" + _this.innerText + "/" + date_wc.getFullYear();
    AddNewCalendarEvent_wc(curr, false);
}

function CancelEventInfo_wc(id) {
    openWSE.ConfirmWindow("Are you sure you want to cancel this event?",
        function () {
            LoadingWorkCalendar("Cancelling...");
            $.ajax({
                url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/CancelEventInfo",
                type: "POST",
                data: '{ "id": "' + id + '","groupname": "' + $("#group_select_wc").val() + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    if (openWSE.ConvertBitToBoolean(response)) {
                        ViewNewCalendarEvent_wc("");
                        $("." + id).remove();
                    }

                    $("#update-element-wc").remove();
                }
            });
        }, null);
}

function DeleteEventInfo_wc(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this event?",
        function () {
            LoadingWorkCalendar("Deleting...");
            $.ajax({
                url: "Apps/WorkCalendar/WorkCalendar.asmx/DeleteEventInfo",
                type: "POST",
                data: '{ "id": "' + id + '","groupname": "' + $("#group_select_wc").val() + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    $("#update-element-wc").remove();
                    if (openWSE.ConvertBitToBoolean(response)) {
                        ViewNewCalendarEvent_wc("");
                        $("." + id).remove();
                    }

                    $("#update-element-wc").remove();
                }
            });
        }, null);
}

function DisplayEvents_wc() {
    $("#app-workcalendar").find(".table-Days td").each(function (index) {
        $(this).find(".calendar-event").each(function (index) {
            $(this).css("display", "block");
        });
    });
}

var _id_wc = "";
var _title_edit_wc = "";
var _employee_edit_wc = "";
function ViewNewCalendarEvent_wc(id) {
    if ($("#ViewCalEvent-element_wc").css("display") != "block") {
        $.ajax({
            url: "Apps/WorkCalendar/WorkCalendar.asmx/GetEventInfo",
            type: "POST",
            data: '{ "id": "' + id + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                if (response != "") {
                    _id_wc = id;
                    $("#ViewCalEvent-element_wc").find("#eventholderCal_modal").html(data.d);
                    openWSE.LoadModalWindow(true, 'ViewCalEvent-element_wc', 'Event Details');
                }
            }
        });
    }
    else {
        openWSE.LoadModalWindow(false, 'ViewCalEvent-element_wc', '');
        setTimeout(function () {
            _id_wc = "";
            _title_edit_wc = "";
            _employee_edit_wc = "";
            $("#hf_editmode-wc").val("false");
            $("#ViewCalEvent-element_wc").find("#eventholderCal_modal").html("");
        }, openWSE_Config.animationSpeed);
    }
}

function EditCalendarEvent_wc(title, employee, desc, startDate, endDate, hours) {
    $("#hf_editmode-wc").val("true");

    var dstart = startDate.split(" ");
    var dend = endDate.split(" ");

    var s = dstart[0].split('/');
    var s_m = s[0];
    var s_d = s[1];
    if ((s_m.charAt(0) != '0') && (s_m.charAt(0) != '1')) {
        s_m = "0" + s_m;
    }
    if ((s_d.charAt(0) != '0') && (s_d.charAt(0) != '1') && (s_d.charAt(0) != '2') && (s_d.charAt(0) != '3')) {
        s_d = "0" + s_d;
    }

    var e = dend[0].split('/');
    var e_m = e[0];
    var e_d = e[1];
    if ((e_m.charAt(0) != '0') && (e_m.charAt(0) != '1')) {
        e_m = "0" + e_m;
    }
    if ((e_d.charAt(0) != '0') && (e_d.charAt(0) != '1') && (e_d.charAt(0) != '2') && (e_d.charAt(0) != '3')) {
        e_d = "0" + e_d;
    }

    var sdate = s_m + "/" + s_d + "/" + s[2];
    var edate = e_m + "/" + e_d + "/" + e[2];

    $("#app-workcalendar").find("#tb_newevent_start_wc").val(sdate);
    $("#app-workcalendar").find("#tb_newevent_end_wc").val(edate);
    $("#app-workcalendar").find("#dd_newevent_starttime_wc").val(dstart[1].replace(":00", ""));
    $("#app-workcalendar").find("#dd_newevent_endtime_wc").val(dend[1].replace(":00", ""));
    $("#app-workcalendar").find("#dd_newevent_starttime_wc_ampm").val(dstart[2]);
    $("#app-workcalendar").find("#dd_newevent_endtime_wc_ampm").val(dend[2]);
    $("#app-workcalendar").find("#tb_newevent_desc_wc").val(desc);
    $("#app-workcalendar").find("#tb_hours").val(hours);

    _title_edit_wc = title;
    _employee_edit_wc = employee;

    openWSE.LoadModalWindow(false, 'ViewCalEvent-element_wc', '');
    $("#ViewCalEvent-element_wc").find("#eventholderCal_modal").html("");
    AddNewCalendarEvent_wc(sdate, true);
}

function LoadingWorkCalendar(message) {
    $("#update-element-wc").remove();
    var x = "<div id='update-element-wc'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#workcalendar-load").append(x);
    $("#update-element-wc").show();
}

var eleClass;
$(document.body).on("mouseenter", "#app-workcalendar .calendar-event", function () {
    eleClass = "";
    classNames = openWSE.GetElementClassList(this);
    for (var i = 0; i < classNames.length; i++) {
        if (classNames[i] != "calendar-event" && classNames[i] != "calendar-event-hover") {
            eleClass = classNames[i];
            break;
        }
    }
    if (!$("." + eleClass).hasClass("calendar-event-hover")) {
        $("." + eleClass).addClass("calendar-event-hover");
    }
});

$(document.body).on("mouseleave", "#app-workcalendar .calendar-event", function () {
    $("." + eleClass).removeClass("calendar-event-hover");
});





/*****************************************************/
/* Add new event
/*****************************************************/
function AddNewCalendarEvent_wc(startDate, edit) {
    if ($("#NewCalEvent-element_wc").css("display") != "block") {
        if (startDate == "") {
            var date = new Date();
            var m = (date.getMonth() + 1).toString();
            var d = date.getDate().toString();

            if ((m.charAt(0) != '0') && (m.charAt(0) != '1')) {
                m = "0" + m;
            }
            if ((d.charAt(0) != '0') && (d.charAt(0) != '1') && (d.charAt(0) != '2') && (d.charAt(0) != '3')) {
                d = "0" + d;
            }

            startDate = m + "/" + d + "/" + date.getFullYear();
        }

        $("#app-workcalendar").find("#dd_newevent_starttime_wc").val("8:00");
        $("#app-workcalendar").find("#dd_newevent_endtime_wc").val("5:00");
        $("#app-workcalendar").find("#dd_newevent_starttime_wc_ampm").val("AM");
        $("#app-workcalendar").find("#dd_newevent_endtime_wc_ampm").val("PM");

        UpdateReasonDD_wc();
        UpdateEmployeeDD_wc();

        $("#app-workcalendar").find("#tb_newevent_start_wc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker("option", "minDate", selectedDate);
            },
            onSelect: function (dataText, inst) {
                UpdateTotalHours_wc();
            }
        });
        $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onSelect: function (dataText, inst) {
                UpdateTotalHours_wc();
            }
        });

        $("#app-workcalendar").find("#tb_newevent_end_wc").datepicker("option", "minDate", startDate);

        if ($("#hf_editmode-wc").val() == "" || !openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
            $("#app-workcalendar").find("#tb_newevent_start_wc").val(startDate);
            $("#app-workcalendar").find("#tb_newevent_end_wc").val(startDate);
            $("#app-workcalendar").find("#btn_createnewevent_wc").val("Create");
        }
        else {
            $("#app-workcalendar").find("#btn_createnewevent_wc").val("Update");
        }

        var title = "New Event";
        if (edit) {
            title = "Edit Event";
        }

        UpdateTotalHours_wc();
        openWSE.LoadModalWindow(true, 'NewCalEvent-element_wc', title);
    }
    else {
        $("#NewCalEvent-element_wc").hide();
        $("#app-workcalendar").find("#tb_newevent_desc_wc").val("");
        $("#app-workcalendar").find("#tb_newevent_start_wc").val("");
        $("#app-workcalendar").find("#tb_newevent_end_wc").val("");
        $("#app-workcalendar").find("#btn_createnewevent_wc").show();
        if (openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
            $("#hf_editmode-wc").val("false");
            ViewNewCalendarEvent_wc(_id_wc);
            _id_wc = "";
        }
    }
}

function btn_createnewevent_wc_Clicked() {
    var employee = $("#app-workcalendar").find("#dd_employees_wc").val();
    var title = $("#app-workcalendar").find("#dd_reason_wc").val();
    var desc = $.trim($("#app-workcalendar").find("#tb_newevent_desc_wc").val()).toString();
    var start = $.trim($("#app-workcalendar").find("#tb_newevent_start_wc").val() + " " + $("#app-workcalendar").find("#dd_newevent_starttime_wc").val() + " " + $("#app-workcalendar").find("#dd_newevent_starttime_wc_ampm").val()).toString();
    var end = $.trim($("#app-workcalendar").find("#tb_newevent_end_wc").val() + " " + $("#app-workcalendar").find("#dd_newevent_endtime_wc").val() + " " + $("#app-workcalendar").find("#dd_newevent_endtime_wc_ampm").val()).toString();
    var hours = $.trim($("#tb_hours").val());

    if ((title == "") || (start == "") || (end == "")) {
        $("#errormessagewc").html("Must fill out Title, Start Date, and End Date.");
        setTimeout(function () {
            $("#errormessagewc").html("");
        }, 3000);
    }
    else {
        $("#btn_createnewevent_wc").hide();
        var _url = "Apps/WorkCalendar/WorkCalendar.asmx/AddEvent";
        var _data = '{ "employee": "' + employee + '","title": "' + title + '","desc": "' + desc + '","startdate": "' + start + '","enddate": "' + end + '","hours": "' + hours + '","group": "' + $("#group_select_wc").val() + '" }';
        if (openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
            _url = "Apps/WorkCalendar/WorkCalendar.asmx/UpdateEvent";
            _data = '{ "id": "' + _id_wc + '","employee": "' + employee + '","title": "' + title + '","desc": "' + desc + '","startdate": "' + start + '","enddate": "' + end + '","hours": "' + hours + '" }';
        }

        LoadingWorkCalendar("Updating...");
        $.ajax({
            url: _url,
            type: "POST",
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    AddNewCalendarEvent_wc("", false);
                    onCalendarLoad_wc();
                    if (openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
                        $("#hf_editmode-wc").val("false");
                        ViewNewCalendarEvent_wc(_id_wc);
                        _id_wc = "";
                    }
                }
                else {
                    openWSE.AlertWindow("There was an error trying to add/update your event. Please try again.");
                }

                $("#btn_createnewevent_wc").show();
                $("#update-element-wc").remove();
            },
            error: function (data) {
                openWSE.AlertWindow("There was an error trying to add/update your event. Please try again.");
                $("#btn_createnewevent_wc").show();
                $("#update-element-wc").remove();
            }
        });
    }
}

$(document.body).on("change", "#dd_newevent_starttime_wc", function () {
    UpdateTotalHours_wc();
});

$(document.body).on("change", "#dd_newevent_starttime_wc_ampm", function () {
    UpdateTotalHours_wc();
});

$(document.body).on("change", "#dd_newevent_endtime_wc", function () {
    UpdateTotalHours_wc();
});

$(document.body).on("change", "#dd_newevent_endtime_wc_ampm", function () {
    UpdateTotalHours_wc();
});

function UpdateTotalHours_wc() {
    var start = $.trim($("#tb_newevent_start_wc").val() + " " + $("#dd_newevent_starttime_wc").val() + " " + $("#dd_newevent_starttime_wc_ampm").val());
    var end = $.trim($("#tb_newevent_end_wc").val() + " " + $("#dd_newevent_endtime_wc").val() + " " + $("#dd_newevent_endtime_wc_ampm").val());
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/CalculateTotalHours",
        type: "POST",
        data: '{ "fulltimeStart": "' + start + '","fulltimeEnd": "' + end + '","userName": "' + $("#dd_employees_wc").val() + '","reason": "' + $("#dd_reason_wc").val() + '","group": "' + $("#group_select_wc").val() + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (openWSE.ConvertBitToBoolean(response[0])) {
                $("#tb_hours").prop("disabled", false);
                $("#tb_hours").css("background-color",  "#FFFFFF");
            }
            else {
                $("#tb_hours").prop("disabled", true);
                $("#tb_hours").css("background-color", "#EFEFEF");
            }

            if (openWSE.ConvertBitToBoolean(response[1])) {
                $("#tb_hours").val(response[1]);
            }
        }
    });
}




/*****************************************************/
/* Load/Edit Types (Reasons and Employees)
/*****************************************************/
function LoadTypeEdit_wc(x) {
    if (x == "0") {
        openWSE.LoadModalWindow(false, "TypeEdit-element_wc", "");
        setTimeout(function () {
            $("#Typelist_wc").html("");
        }, openWSE_Config.animationSpeed);

        if (calViewType_wc == "overview") {
            GetSidebarOverview_wc();
        }
    }
    else if (x == "1") {
        if ($("#TypeEdit-element_wc").css("display") != "block") {
            LoadingWorkCalendar("Loading...");
            $.ajax({
                url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=ptohours" + "&group=" + escape($("#group_select_wc").val()),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                complete: function (data) {
                    $("#Typelist_wc").html(data.responseText);
                    openWSE.LoadModalWindow(true, "TypeEdit-element_wc", "User Vacation Time Edit");
                    $("#update-element-wc").remove();
                }
            });
        }
        else {
            openWSE.LoadModalWindow(false, "TypeEdit-element_wc", "");
            setTimeout(function () {
                $("#Typelist_wc").html("");
            }, openWSE_Config.animationSpeed);

            if (calViewType_wc == "overview") {
                GetSidebarOverview_wc();
            }
        }
    }
    else if (x == "2") {
        if ($("#TypeEdit-element_wc").css("display") != "block") {
            LoadingWorkCalendar("Loading...");
            $.ajax({
                url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=ptotypes" + "&group=" + escape($("#group_select_wc").val()),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                complete: function (data) {
                    $("#Typelist_wc").html(data.responseText);
                    openWSE.LoadModalWindow(true, "TypeEdit-element_wc", "Vacation Reason Edit");
                    $("#update-element-wc").remove();
                }
            });
        }
        else {
            openWSE.LoadModalWindow(false, "TypeEdit-element_wc", "");
            setTimeout(function () {
                $("#Typelist_wc").html("");
            }, openWSE_Config.animationSpeed);

            if (calViewType_wc == "overview") {
                GetSidebarOverview_wc();
            }
        }
    }
    else {
        openWSE.LoadModalWindow(false, "TypeEdit-element_wc", "");
        setTimeout(function () {
            $("#Typelist_wc").html("");
        }, openWSE_Config.animationSpeed);

        if (calViewType_wc == "overview") {
            GetSidebarOverview_wc();
        }
    }
}

function deleteType(id, Type) {
    openWSE.ConfirmWindow("Are you sure you want to delete this Vacation Reason?",
        function () {
            $.ajax({
                url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=deletetype&typeid=" + id + "&type=" + Type + "&group=" + escape($("#group_select_wc").val()),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                complete: function (data) {
                    if (!openWSE.ConvertBitToBoolean(data.responseText)) {
                        openWSE.AlertWindow("Error deleting reason. Please try again.");
                    } else {
                        $("#Typelist_wc").html(data.responseText);
                    }
                    UpdateReasonDD_wc();
                }
            });
        }, null);
}

function addType() {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=addtype&type=" + document.getElementById("tb_addtype").value + "&group=" + escape($("#group_select_wc").val()) + "&deduct=" + document.getElementById("add-reason-checkbox").checked,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        complete: function (data) {
            if (data.responseText == "false") {
                $("#typeerror").html("<b style='color: Red; font-size: 11px;'>Reason Already Exists</b>");
            } else if (data.responseText == "blank") {
                $("#typeerror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#Typelist_wc").html(data.responseText);
            }
            UpdateReasonDD_wc();
        }
    });
}

function editType(Type) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=edittype&type=" + Type + "&group=" + escape($("#group_select_wc").val()),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        complete: function (data) {
            $("#Typelist_wc").html(data.responseText);
            UpdateReasonDD_wc();
        }
    });
}

function updateType(id) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=updatetype&type=" + document.getElementById("tb_edittype").value + "&typeid=" + id + "&group=" + escape($("#group_select_wc").val()) + "&deduct=" + document.getElementById("edit-reason-checkbox").checked,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        complete: function (data) {
            if (data.responseText == "false") {
                $("#typeerror").html("<b style='color: Red; font-size: 11px;'>Reason Already Exists</b>");
            } else if (data.responseText == "blank") {
                $("#typeerror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#Typelist_wc").html(data.responseText);
            }
            UpdateReasonDD_wc();
        }
    });
}

function canceleditType() {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=ptotypes" + "&group=" + escape($("#group_select_wc").val()),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        complete: function (data) {
            $("#Typelist_wc").html(data.responseText);
        }
    });
}

function updateUser(user) {
    LoadingWorkCalendar("Loading...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendarAdmin.ashx?action=updateUser&user=" + user + "&hours=" + document.getElementById("tb_edithours_" + user).value + "&group=" + escape($("#group_select_wc").val()),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        complete: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.responseText)) {
                $("#Typelist_wc").html(data.responseText);
                $.ajax({
                    url: "Apps/WorkCalendar/WorkCalendar.asmx/Loadce_Overview",
                    type: "POST",
                    data: '{ "group": "' + $("#group_select_wc").val() + '" }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var response = data.d;
                        var str = "<h3 class='font-bold'>User Vacation Time</h3><div class='clear-space-five'></div>" + response[0];
                        str += "<div class='clear' style='height: 25px;'></div><h3 class='font-bold'>Hours Remaining</h3><div class='clear-space-five'></div>" + response[1];
                        $("#sidebar-caloverview-wc").html(str);
                        setTimeout(function () {
                            $("#update-element-wc").remove();
                        }, 500);
                    },
                    error: function (err) {
                        setTimeout(function () {
                            $("#update-element-wc").remove();
                        }, 500);
                    }
                });
            }
        }
    });
}

function UpdateReasonDD_wc() {
    var x = document.getElementById("dd_reason_wc");
    if ((x != null) && (x != undefined)) {
        x.innerHTML = "";

        var option = document.createElement("option");
        option.text = "Loading...";
        x.add(option);

        $.ajax({
            url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/GetReasonsForGroup",
            type: "POST",
            data: '{ "group": "' + $("#group_select_wc").val() + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                x.innerHTML = "";
                if (response.length > 0) {
                    for (var j = 0; j < response.length; j++) {
                        var option = document.createElement("option");
                        option.text = response[j];
                        option.value = response[j];
                        x.add(option);
                    }
                }

                if (openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
                    $("#app-workcalendar").find("#dd_reason_wc").val(_title_edit_wc);
                }

            }
        });
    }
}

function UpdateEmployeeDD_wc() {
    var x = document.getElementById("dd_employees_wc");
    if ((x != null) && (x != undefined)) {
        x.innerHTML = "";

        var option = document.createElement("option");
        option.text = "Loading...";
        x.add(option);

        $("#btn_createnewevent_wc").hide();
        $.ajax({
            url: openWSE.siteRoot() + "Apps/WorkCalendar/WorkCalendar.asmx/GetUserListForGroup",
            type: "POST",
            data: '{ "group": "' + $("#group_select_wc").val() + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                x.innerHTML = "";
                if (response.length > 0) {
                    for (var j = 0; j < response.length; j++) {
                        var option = document.createElement("option");
                        option.text = response[j][0];
                        option.value = response[j][1];
                        x.add(option);
                    }
                }

                $("#btn_createnewevent_wc").show();
                if (openWSE.ConvertBitToBoolean($("#hf_editmode-wc").val())) {
                    $("#app-workcalendar").find("#dd_employees_wc").val(_employee_edit_wc);
                }
            }
        });
    }
}