var table_Days_Height_pc = 0;
var date_pc;
var month_pc = new Array(12);
var selected_m_pc = "";
var selected_d_pc = "";
var selected_y_pc = "";
month_pc[0] = "January";
month_pc[1] = "February";
month_pc[2] = "March";
month_pc[3] = "April";
month_pc[4] = "May";
month_pc[5] = "June";
month_pc[6] = "July";
month_pc[7] = "August";
month_pc[8] = "September";
month_pc[9] = "October";
month_pc[10] = "November";
month_pc[11] = "December";

var reloadingSchedule_pc = false;

function pageLoad() {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
        RestorePostBack_pc();
    }
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
        RestorePostBack_pc();
    }
});

var temp_startDate_pc = "";
var temp_endDate_pc = "";
var temp_startTime_pc = "";
var temp_endTime_pc = "";
var temp_startampm_pc = "";
var temp_endampm_pc = "";
var temp_reason_pc = "";
var temp_description_pc = "";
var temp_hours_pc = "";
prm.add_beginRequest(function (sender, args) {
    if ($("#NewCalEvent-element").css("display") == "block") {
        temp_startDate_pc = $("#NewCalEvent-element").find("#tb_newevent_start_pc").val();
        temp_endDate_pc = $("#NewCalEvent-element").find("#tb_newevent_end_pc").val();
        temp_startTime_pc = $("#NewCalEvent-element").find("#dd_newevent_starttime_pc").val();
        temp_endTime_pc = $("#NewCalEvent-element").find("#dd_newevent_endtime_pc").val();
        temp_startampm_pc = $("#NewCalEvent-element").find("#dd_newevent_starttime_pc_ampm").val();
        temp_endampm_pc = $("#NewCalEvent-element").find("#dd_newevent_endtime_pc_ampm").val();
        temp_reason_pc = $("#NewCalEvent-element").find("#tb_newevent_title_pc").val();
        temp_description_pc = $("#NewCalEvent-element").find("#tb_newevent_desc_pc").val();
        temp_hours_pc = $("#NewCalEvent-element").find("#tb_hours").val();
    }
});

function RestorePostBack_pc() {
    if ($("#NewCalEvent-element").css("display") == "block") {
        $("#NewCalEvent-element").find("#tb_newevent_start_pc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker("option", "minDate", selectedDate);
            }
        });
        $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2
        });


        if (temp_startTime_pc != "") {
            $("#NewCalEvent-element").find("#tb_newevent_start_pc").val(temp_startDate_pc);
            $("#NewCalEvent-element").find("#tb_newevent_end_pc").val(temp_endDate_pc);
            $("#NewCalEvent-element").find("#dd_newevent_starttime_pc").val(temp_startTime_pc);
            $("#NewCalEvent-element").find("#dd_newevent_endtime_pc").val(temp_endTime_pc);
            $("#NewCalEvent-element").find("#dd_newevent_starttime_pc_ampm").val(temp_startampm_pc);
            $("#NewCalEvent-element").find("#dd_newevent_endtime_pc_ampm").val(temp_endampm_pc);
            $("#NewCalEvent-element").find("#tb_newevent_title_pc").val(temp_reason_pc);
            $("#NewCalEvent-element").find("#tb_newevent_desc_pc").val(temp_description_pc);
            $("#NewCalEvent-element").find("#tb_hours").val(temp_hours_pc);

            $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker("option", "minDate", temp_startDate_pc);

            temp_startDate_pc = "";
            temp_endDate_pc = "";
            temp_startTime_pc = "";
            temp_endTime_pc = "";
            temp_startampm_pc = "";
            temp_endampm_pc = "";
            temp_reason_pc = "";
            temp_description_pc = "";
            temp_hours_pc = "";
        }
    }
}

$(document).ready(function () {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
    }
});

$(window).resize(function () {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
    }
});

$(document.body).on("click", ".app-main-holder[data-appid='app-personalcalendar'] .maximize-button-app", function () {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
    }
});

$(document.body).on("dblclick", ".app-main-holder[data-appid='app-personalcalendar'] .app-head-dblclick", function () {
    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
    }
});

function KeyPressSearch_pc(event) {
    try {
        if (event.which == 13) {
            onCalendarLoad_pc();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            onCalendarLoad_pc();
        }
        delete evt;
    }
}

function HideTitleSearch_PC(_this) {
    if ($("#pCal-titlebar").is(":visible")) {
        $(_this).html("Show Title/Search");
        $("#pCal-titlebar").hide();
    }
    else {
        $(_this).html("Hide Title/Search");
        $("#pCal-titlebar").show();
    }

    if ($("#personalcalendar-load").length > 0) {
        onCalendarLoad_pc();
    }
}



/*****************************************************/
/* Build the calendar
/*****************************************************/
function onCalendarLoad_pc() {
    LoadingPersonalCalendar("Loading...");
    $(".app-main-holder[data-appid='app-personalcalendar']").resizable({
        stop: function (event, ui) {
            onCalendarLoad_pc();
        }
    });

    var d_temp = new Date();
    if ((selected_m_pc == "") && (selected_y_pc == "")) {
        buildCalendar_PC(d_temp.getMonth(), d_temp.getFullYear());
        GetMonthInfo_PC(d_temp.getMonth(), d_temp.getFullYear());
    }
    else {
        buildCalendar_PC(selected_m_pc, selected_y_pc);
        GetMonthInfo_PC(selected_m_pc, selected_y_pc);
    }
}




/*****************************************************/
/* Build Monthy Calendar
/*****************************************************/
function buildCalendar_PC(m, y) {
    LoadingPersonalCalendar("Loading...");
    $("#hourviewtype_pc").hide();
    date_pc = new Date(y, m, 1, new Date().getHours(), new Date().getMinutes(), new Date().getSeconds(), new Date().getMilliseconds());

    var ddMonth = "<select id='dd_monthSelect' class='margin-right-sml float-right' onchange='onMonthYearChange_PC()'>";
    for (var i = 0; i < month_pc.length; i++) {
        var selected = "";
        if (date_pc.getMonth() == i) {
            selected = " selected='selected'";
        }
        ddMonth += "<option value='" + i + "' " + selected + ">" + month_pc[i] + "</option>";
    }
    ddMonth += "</select>";

    var ddYear = "<select id='dd_yearSelect' class='margin-left-sml float-right' onchange='onMonthYearChange_PC()'>";
    for (var i = 2004; i < 2024; i++) {
        var selected = "";
        if (date_pc.getFullYear() == i) {
            selected = " selected='selected'";
        }
        ddYear += "<option value='" + i + "' " + selected + ">" + i + "</option>";
    }
    ddYear += "</select>";

    var prevBtn = "<a href='#' onclick='onPrevClick_PC();return false;' class='pg-prev-btn margin-right float-right' title='Previous Month' style='margin-top: 5px;'></a>";
    var nextBtn = "<a href='#' onclick='onNextClick_PC();return false;' class='pg-next-btn margin-left float-right' title='Next Month' style='margin-top: 5px;'></a>";
    $("#currmonthBtns_pc").html("<div class='font-year-pc pad-top-sml'>" + nextBtn + ddYear + " " + ddMonth + prevBtn + "</div>");

    var h = $(".app-main-holder[data-appid='app-personalcalendar']").height() - $(".app-main-holder[data-appid='app-personalcalendar']").find(".app-head").height();
    table_Days_Height_pc = h - ($(".app-main-holder[data-appid='app-personalcalendar']").find(".pc-menu-bar").height() + $("#pCal-titlebar").height());
    var str = "<table cellpadding='0' cellspacing='0' style='width:100%;height:" + table_Days_Height_pc + "px'><tbody><tr class='days-of-week'><td>Sun</td><td>Mon</td><td>Tue</td><td>Wed</td><td>Thur</td><td>Fri</td><td>Sat</td></tr>";
    var dinm = daysInMonth_pc(date_pc.getMonth(), date_pc.getFullYear());
    var count = 1;
    for (var r = 0; r < 6; r++) {
        str += "<tr class='table-Days'>";
        for (var c = 0; c < 7; c++) {
            if (count == dinm) {
                str += "<td class='not-apart-of-month'></td>";
            } else {
                if (count == 1) {
                    if (c == findStartofCal_pc()) {
                        str += createDay_PC(date_pc.getMonth(), count, date_pc.getFullYear());
                        count++;
                    } else {
                        str += "<td class='not-apart-of-month'></td>";
                    }
                } else {
                    str += createDay_PC(date_pc.getMonth(), count, date_pc.getFullYear());
                    count++;
                }

            }
        }
        str += "</tr>";
    }
    str += "</tbody></table>";
    $("#usercalendar_pc").html(str);
}

function createDay_PC(m, d, y) {
    var Now = new Date();
    var hf_d = document.getElementById("dateselected_pc");
    var returnStr = "";
    if (hf_d != null) {
        if (hf_d.value != "") {
            var sd = hf_d.value.split("_");
            if (Now.getMonth() == m && Now.getDate() == d && Now.getFullYear() == y) {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_PC(this);'><span class='current-day' title='Click to add event'>" + d + "</span></span></td>";
            }
            else {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_PC(this);' title='Click to add event'>" + d + "</span></td>";
            }
        }
        else {
            if (Now.getMonth() == m && Now.getDate() == d && Now.getFullYear() == y) {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_PC(this);'><span class='current-day' title='Click to add event'>" + d + "</span></span></td>";
            }
            else {
                returnStr = "<td id='" + m + "_" + d + "_" + y + "'><span class='calendar-td-day cursor-pointer' onclick='onDayClick_PC(this);' title='Click to add event'>" + d + "</span></td>";
            }
        }
    }

    return returnStr;
}

function GetMonthInfo_PC(month, year) {
    if (!reloadingSchedule_pc) {
        reloadingSchedule_pc = true;
        selected_m_pc = month;
        selected_y_pc = year;
        viewtype = $(".app-main-holder[data-appid='app-personalcalendar']").find("#hf_viewtype_vacationentry").val();
        if ((viewtype == null) || (viewtype == "")) {
            viewtype = "all";
        }

        var search = $.trim($("#tb_search_pcal").val());
        if (search == "Search Current Month") {
            search = "";
        }

        $.ajax({
            url: openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/GetMonthInfo",
            type: "POST",
            data: '{ "month": "' + month + '","year": "' + year + '","search": "' + search.toLowerCase() + '" }',
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
                            var tdWidth = $(".app-main-holder[data-appid='app-personalcalendar']").find("#" + startDate).width();

                            var bufferLen = len;
                            if (len > 1) {
                                var $nextTd = $(".app-main-holder[data-appid='app-personalcalendar']").find("#" + startDate).next("td");
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

                            var eventDiv = BuildCalendarEvent_pc(response[i][0], response[i][1], tdWidth, response[i][4], response[i][5], len, sameId);
                            $(".app-main-holder[data-appid='app-personalcalendar']").find("#" + startDate).append(eventDiv);
                        }
                    }

                    SetMarginEvents_pc();
                    SetTDHeights_pc();
                }

                reloadingSchedule_pc = false;
                openWSE.RemoveUpdateModal();
            }
        });
    }
}

function SetMarginEvents_pc() {
    var td_width = 0;
    var length_temp = 0;
    var bufferMargin = 2;
    $(".app-main-holder[data-appid='app-personalcalendar']").find(".table-Days").each(function (index) {
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
                var event_height = $(this).height();
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
    DisplayEvents_pc();
}

function SetTDHeights_pc() {
    $(".app-main-holder[data-appid='app-personalcalendar']").find(".table-Days").each(function (index) {
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

function BuildCalendarEvent_pc(id, usercolor, width, reason, daterange, length, sameId) {
    var str = "";
    if ((length > 0) && (($("." + id).length == 0) || (sameId))) {
        str += "<div class='calendar-event " + id + "' style='background: " + usercolor + "; width: " + width + "px;' onclick=\"ViewNewCalendarEvent_pc('" + id + "');\" title='" + reason.replace("/n", " ") + "'>";
        var _reason = reason.replace("/n", " - ");
        if (length == 1) {
            _reason = reason.replace("/n", "<br />");
        }
        str += "<div class='calendar-event-font inline-block'>" + _reason + "</div></div>";
    }
    return str;
}




/*****************************************************/
/* Universal functions
/*****************************************************/
function daysInMonth_pc(m, y) {
    return new Date(y, m + 1, 0).getDate() + 1;
}

function findStartofCal_pc() {
    var d = new Date(month_pc[date_pc.getMonth()] + "1, " + date_pc.getFullYear());
    return d.getDay();
}

function onNextClick_PC() {
    $("#usercalendar_pc").hide();
    var m = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_monthSelect").val());
    var y = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_yearSelect").val());
    if (m == 11) {
        y = y + 1;
        m = 0;
    }
    else {
        m = m + 1;
    }

    buildCalendar_PC(m, y);
    GetMonthInfo_PC(m, y);

    $("#usercalendar_pc").show();
}

function onPrevClick_PC() {
    $("#usercalendar_pc").hide();
    var m = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_monthSelect").val());
    var y = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_yearSelect").val());
    if (m == 0) {
        y = y - 1;
        m = 11;
    }
    else {
        m = m - 1;
    }

    buildCalendar_PC(m, y);
    GetMonthInfo_PC(m, y);

    $("#usercalendar_pc").show();
}

function onMonthYearChange_PC() {
    $("#usercalendar_pc").hide();
    var m = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_monthSelect").val());
    var y = parseInt($(".app-main-holder[data-appid='app-personalcalendar']").find("#dd_yearSelect").val());

    buildCalendar_PC(m, y);
    GetMonthInfo_PC(m, y);

    $("#usercalendar_pc").show();
}

function onDayClick_PC(_this) {
    var Now = new Date();
    var hidden_date_pc = document.getElementById("hidden_date_pc");
    hidden_date_pc.value = "";
    var temp = new Date(date_pc.getFullYear(), date_pc.getMonth(), (parseInt(_this.innerHTML) + 1), date_pc.getHours(), date_pc.getMinutes(), date_pc.getSeconds(), date_pc.getMilliseconds());
    var hf_d = document.getElementById("dateselected_pc");
    hf_d.value = date_pc.getMonth() + "_" + _this.innerText + "_" + date_pc.getFullYear();
    var curr = (date_pc.getMonth() + 1) + "/" + _this.innerText + "/" + date_pc.getFullYear();
    AddNewCalendarEvent_pc(curr, false);
}

function DeleteEventInfo_pc(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this event?",
    function () {
        LoadingPersonalCalendar("Deleting...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/DeleteEventInfo",
            type: "POST",
            data: '{ "id": "' + id + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                openWSE.RemoveUpdateModal();
                if (openWSE.ConvertBitToBoolean(response)) {
                    ViewNewCalendarEvent_pc("");
                    $("." + id).remove();
                }
            }
        });
    }, null);
}

function DisplayEvents_pc() {
    $(".app-main-holder[data-appid='app-personalcalendar']").find(".table-Days td").each(function (index) {
        $(this).find(".calendar-event").each(function (index) {
            $(this).css("display", "block");
        });
    });
}

function AddNewCalendarEvent_pc(startDate, edit) {
    if ($("#NewCalEvent-element").css("display") != "block") {
        if (startDate == "") {
            startDate = new Date().toLocaleDateString();
        }

        $("#NewCalEvent-element").find("#tb_newevent_start_pc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker("option", "minDate", selectedDate);
            }
        });
        $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 2
        });

        $("#NewCalEvent-element").find("#tb_newevent_end_pc").datepicker("option", "minDate", startDate);

        if ($("#hf_editmode-pc").val() == "" || !openWSE.ConvertBitToBoolean($("#hf_editmode-pc").val())) {
            $("#NewCalEvent-element").find("#tb_newevent_start_pc").val(startDate);
            $("#NewCalEvent-element").find("#tb_newevent_end_pc").val(startDate);
            $("#NewCalEvent-element").find("#tb_newevent_title_pc").val("Event for " + startDate);
            $("#NewCalEvent-element").find("#btn_createnewevent_pc").val("Create");
        }
        else {
            $("#NewCalEvent-element").find("#btn_createnewevent_pc").val("Update");
        }

        var title = "New Event";
        if (edit) {
            title = "Edit Event";
        }

        openWSE.LoadModalWindow(true, 'NewCalEvent-element', title);
    }
    else {
        openWSE.LoadModalWindow(false, 'NewCalEvent-element', '');
        $("#NewCalEvent-element").find("#tb_newevent_title_pc").val("");
        $("#NewCalEvent-element").find("#tb_newevent_desc_pc").val("");
        $("#NewCalEvent-element").find("#tb_newevent_start_pc").val("");
        $("#NewCalEvent-element").find("#tb_newevent_end_pc").val("");
        $("#NewCalEvent-element").find("#btn_createnewevent_pc").show();
        if (openWSE.ConvertBitToBoolean($("#hf_editmode-pc").val())) {
            $("#hf_editmode-pc").val("false");
            ViewNewCalendarEvent_pc(_id_pc);
            _id_pc = "";
        }
    }
}

var _id_pc = "";
function ViewNewCalendarEvent_pc(id) {
    if ($("#ViewCalEvent-element").css("display") != "block") {
        $.ajax({
            url: openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/GetEventInfo",
            type: "POST",
            data: '{ "id": "' + id + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var response = data.d;
                if (response != "") {
                    _id_pc = id;
                    $("#ViewCalEvent-element").find("#eventholderCal_modal").html(data.d);
                    openWSE.LoadModalWindow(true, 'ViewCalEvent-element', 'Event Details');
                }
            }
        });
    }
    else {
        openWSE.LoadModalWindow(false, 'ViewCalEvent-element', '');
        setTimeout(function () {
            _id_pc = "";
            $("#hf_editmode-pc").val("false");
            $("#ViewCalEvent-element").find("#eventholderCal_modal").html("");
        }, openWSE_Config.animationSpeed);
    }
}

function EditCalendarEvent_pc(title, desc, color, startDate, endDate) {
    $("#hf_editmode-pc").val("true");

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

    $("#NewCalEvent-element").find("#tb_newevent_start_pc").val(sdate);
    $("#NewCalEvent-element").find("#tb_newevent_end_pc").val(edate);
    $("#NewCalEvent-element").find("#dd_newevent_starttime_pc").val(dstart[1].replace(":00", ""));
    $("#NewCalEvent-element").find("#dd_newevent_endtime_pc").val(dend[1].replace(":00", ""));
    $("#NewCalEvent-element").find("#dd_newevent_starttime_pc_ampm").val(dstart[2]);
    $("#NewCalEvent-element").find("#dd_newevent_endtime_pc_ampm").val(dend[2]);
    $("#NewCalEvent-element").find("#tb_newevent_title_pc").val(title);
    $("#NewCalEvent-element").find("#tb_newevent_desc_pc").val(desc);

    $("#NewCalEvent-element").find(".calendar-bg-selectors").each(function (index) {
        if ($(this).hasClass("selected")) {
            $(this).removeClass("selected");
        }
    });

    $("#NewCalEvent-element").find(".calendar-bg-selectors").each(function (index) {
        if (color == $(this).attr("id").replace("cal-color-", "")) {
            $(this).addClass("selected");
        }
    });

    openWSE.LoadModalWindow(false, 'ViewCalEvent-element', '');
    $("#ViewCalEvent-element").find("#eventholderCal_modal").html("");
    AddNewCalendarEvent_pc(sdate, true);
}

function btn_createnewevent_pc_Clicked() {
    var title = $("#NewCalEvent-element").find("#tb_newevent_title_pc").val();
    var desc = $("#NewCalEvent-element").find("#tb_newevent_desc_pc").val();
    var start = $("#NewCalEvent-element").find("#tb_newevent_start_pc").val() + " " + $("#NewCalEvent-element").find("#dd_newevent_starttime_pc").val() + " " + $("#NewCalEvent-element").find("#dd_newevent_starttime_pc_ampm").val();
    var end = $("#NewCalEvent-element").find("#tb_newevent_end_pc").val() + " " + $("#NewCalEvent-element").find("#dd_newevent_endtime_pc").val() + " " + $("#NewCalEvent-element").find("#dd_newevent_endtime_pc_ampm").val();

    if (($.trim(title) == "") || ($.trim(start) == "") || ($.trim(end) == "")) {
        $(".app-main-holder[data-appid='app-personalcalendar']").find("#errormessagepc").html("Must fill out Title, Start Date, and End Date.");
        setTimeout(function () {
            $(".app-main-holder[data-appid='app-personalcalendar']").find("#errormessagepc").html("");
        }, 3000);
    }
    else {
        $("#btn_createnewevent_pc").hide();
        var colorCode = "yellow";
        $("#NewCalEvent-element").find(".calendar-bg-selectors").each(function (index) {
            if ($(this).hasClass("selected")) {
                colorCode = $(this).attr("id").replace("cal-color-", "");
            }
        });

        var _url = openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/AddEvent";
        var _data = '{ "title": "' + title + '","desc": "' + desc + '","startdate": "' + start + '","enddate": "' + end + '","color": "' + colorCode + '" }';
        if ($("#hf_editmode-pc").val() != "" && openWSE.ConvertBitToBoolean($("#hf_editmode-pc").val())) {
            _url = openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/UpdateEvent";
            _data = '{ "id": "' + _id_pc + '","title": "' + title + '","desc": "' + desc + '","startdate": "' + start + '","enddate": "' + end + '","color": "' + colorCode + '" }';
        }

        LoadingPersonalCalendar("Adding Event...");
        $.ajax({
            url: _url,
            type: "POST",
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                AddNewCalendarEvent_pc("", false);
                onCalendarLoad_pc();
                if ($("#hf_editmode-pc").val() != "" && openWSE.ConvertBitToBoolean($("#hf_editmode-pc").val())) {
                    $("#hf_editmode-pc").val("false");
                    ViewNewCalendarEvent_pc(_id_pc);
                    _id_pc = "";
                }
            },
            error: function (data) {
                AddNewCalendarEvent_pc("", false);
                onCalendarLoad_pc();
            }
        });
    }
}

$(document.body).on("click", "#NewCalEvent-element .calendar-bg-selectors", function () {
    var $this = $(this);
    $("#NewCalEvent-element").find(".calendar-bg-selectors").each(function (index) {
        if ($(this).hasClass("selected")) {
            $(this).removeClass("selected");
        }
    });

    $this.addClass("selected");
});

function LoadingPersonalCalendar(message) {
    openWSE.RemoveUpdateModal();
    openWSE.LoadingMessage1(message);
}

var eleClass;
$(document.body).on("mouseenter", ".app-main-holder[data-appid='app-personalcalendar'] .calendar-event", function () {
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

$(document.body).on("mouseleave", ".app-main-holder[data-appid='app-personalcalendar'] .calendar-event", function () {
    $("." + eleClass).removeClass("calendar-event-hover");
});