var personalCalendarApp = function () {
    var _editMode = false;
    var _editId = "";

    $(document.body).on("click", "#NewCalEvent-element .calendar-bg-selectors", function () {
        var $this = $(this);
        $("#NewCalEvent-element").find(".calendar-bg-selectors").each(function (index) {
            if ($(this).hasClass("selected")) {
                $(this).removeClass("selected");
            }
        });

        $this.addClass("selected");
    });

    function CreateCalendarObj() {
        return {
            container: "#usercalendar_pc",
            AddNewCalendarFunction: personalCalendarApp.AddNewCalendarEvent,
            ViewEventFunction: personalCalendarApp.ViewNewCalendarEvent,
            GetMonthInfoFunction: personalCalendarApp.GetMonthInfo
        };
    }

    function ViewNewCalendarEvent(id) {
        if ($("#ViewCalEvent-element").css("display") != "block") {
            openWSE.AjaxCall("Apps/PersonalCalendar/PersonalCalendar.asmx/GetEventInfo", '{ "id": "' + id + '" }', null, function (data) {
                var response = data.d;
                if (response.length === 2 && response[0] !== "" && response[1] !== "") {
                    _editId = id;
                    $("#ViewCalEvent-element").find("#eventholderCal_modal").html(response[0]);
                    $("#ViewCalEvent-element").find(".ModalButtonHolder").html(response[1]);
                    openWSE.LoadModalWindow(true, 'ViewCalEvent-element', 'Event Details');
                }
            });
        }
        else {
            openWSE.LoadModalWindow(false, 'ViewCalEvent-element', '');
            setTimeout(function () {
                _editId = "";
                _editMode = false;
                $("#ViewCalEvent-element").find("#eventholderCal_modal").html("");
            }, openWSE_Config.animationSpeed);
        }
    }
    function GetMonthInfo(id, m, y, search) {
        openWSE.AjaxCall("Apps/PersonalCalendar/PersonalCalendar.asmx/GetMonthInfo", '{ "month": "' + m + '","year": "' + y + '","search": "' + search.toLowerCase() + '" }', null, function (data) {
            openWSE.CalendarViewApps.BuildMonthEvents(id, data.d);
        });
    }

    function AddNewCalendarEvent(startDate, edit) {
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

            if (!_editMode) {
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
            if (_editMode) {
                _editMode = false;
                personalCalendarApp.ViewNewCalendarEvent(_editId);
                _editId = "";
            }
        }
    }
    function EditCalendarEvent(title, desc, color, startDate, endDate) {
        _editMode = true;

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
        personalCalendarApp.AddNewCalendarEvent(sdate, true);
    }
    function DeleteEventInfo(id) {
        openWSE.ConfirmWindow("Are you sure you want to delete this event?",
        function () {
            loadingPopup.Message("Deleting...", "#personalcalendar-load");
            openWSE.AjaxCall("Apps/PersonalCalendar/PersonalCalendar.asmx/DeleteEventInfo", '{ "id": "' + id + '" }', null, function (data) {
                var response = data.d;
                loadingPopup.RemoveMessage("#personalcalendar-load");
                if (openWSE.ConvertBitToBoolean(response)) {
                    personalCalendarApp.ViewNewCalendarEvent("");
                    $("." + id).remove();
                }
            });
        }, null);
    }
    function btn_createnewevent_Clicked() {
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
            if (_editMode) {
                _url = openWSE.siteRoot() + "Apps/PersonalCalendar/PersonalCalendar.asmx/UpdateEvent";
                _data = '{ "id": "' + _editId + '","title": "' + title + '","desc": "' + desc + '","startdate": "' + start + '","enddate": "' + end + '","color": "' + colorCode + '" }';
            }

            loadingPopup.Message("Adding Event...", "#personalcalendar-load");
            openWSE.AjaxCall(_url, _data, null, function (data) {
                personalCalendarApp.AddNewCalendarEvent("", false);
                openWSE.CalendarViewApps.RefreshCalendar("app-personalcalendar");
                if (_editMode) {
                    _editMode = false;
                    personalCalendarApp.ViewNewCalendarEvent(_editId);
                    _editId = "";
                }
            }, function (data) {
                personalCalendarApp.AddNewCalendarEvent("", false);
                openWSE.CalendarViewApps.RefreshCalendar("app-personalcalendar");
            });
        }
    }

    return {
        CreateCalendarObj: CreateCalendarObj,
        GetMonthInfo: GetMonthInfo,
        AddNewCalendarEvent: AddNewCalendarEvent,
        ViewNewCalendarEvent: ViewNewCalendarEvent,
        btn_createnewevent_Clicked: btn_createnewevent_Clicked,
        EditCalendarEvent: EditCalendarEvent,
        DeleteEventInfo: DeleteEventInfo
    }
}();

$(document).ready(function () {
    if ($("#personalcalendar-load").length > 0) {
        openWSE.CalendarViewApps.InitializeCalendar("app-personalcalendar", personalCalendarApp.CreateCalendarObj());
    }
});
function pageLoad() {
    if ($("#personalcalendar-load").length > 0) {
        openWSE.CalendarViewApps.InitializeCalendar("app-personalcalendar", personalCalendarApp.CreateCalendarObj());
    }
}
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
    if ($("#personalcalendar-load").length > 0) {
        openWSE.CalendarViewApps.InitializeCalendar("app-personalcalendar", personalCalendarApp.CreateCalendarObj());
    }
});
