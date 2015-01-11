var currUser = "";
var currDriverCall = "";
var genDirArray;
var recordsToPull = 50;

$(document.body).on("click", "#app-steeltrucks .exit-button-app, #app-steeltrucks-min-bar .exit-button-app-min", function () {
    cookie.del("steeltrucks-fontsize");
});

$(document).ready(function () {
    openWSE.LoadingMessage1("Please Wait...");
    LoadInit();
    $("#tb_exportDateFrom").val(new Date().toLocaleDateString());
    $("#tb_exportDateTo").val(new Date().toLocaleDateString());
    LoadFontSize();
    LoadScheduleList("");
    InitializeJqueryObjects();
});

function BuildDriverExportList() {
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/GetDriverListOptions",
        data: "{ }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                $("#driverListOptions-steeltrucks").html(data.d);
            }
        }
    });
}

function ExportToExcel_steeltrucks() {
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/ExportToExcel",
        data: "{ '_date': '" + $("#eventEdit-Date").html() + "','_driver': '" + $("#eventEdit-Driver").html() + "','_unit': '" + $("#eventEdit-Unit").html() + "','_dir': '" + $("#eventEdit-Dir").html() + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d != "") {
                $.fileDownload(data.d);
            }
            else {
                openWSE.AlertWindow("No records to export");
            }
        }
    });
}

function ExportToExcelAll_steeltrucks() {
    $("#btn_exportAll").hide();
    $("#exportingNow").show();
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/ExportToExcelAll",
        data: "{ '_dateFrom': '" + $("#tb_exportDateFrom").val() + "','_dateTo': '" + $("#tb_exportDateTo").val() + "','_driver': '" + $("#driverListOptions-steeltrucks").val() + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d == "File path wrong.") {
                $("#btn_exportAll").show();
                $("#exportingNow").hide();
                openWSE.AlertWindow("Error saving file. Check file path under System Settings.");
            }
            else if (data.d != "") {
                $("#btn_exportAll").show();
                $("#exportingNow").hide();
                $.fileDownload(data.d);
            }

            else {
                $("#btn_exportAll").show();
                $("#exportingNow").hide();
                openWSE.AlertWindow("No records to export");
            }
        }
    });
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    PageStartUp();
});

function PageStartUp() {
    var a = $("#driver-index-holder").html();
    var b = $("#driver-sch-holder").html();
    if (($.trim(a) == "") || ($.trim(b) == "")) {
        LoadInit();
    }

    InitializeJqueryObjects();
}

function InitializeJqueryObjects() {
    $("#tb_exportDateFrom").datepicker({
        defaultDate: "+1w",
        changeMonth: true,
        numberOfMonths: 2,
        onClose: function (selectedDate) {
            $("#tb_exportDateTo").datepicker("option", "minDate", selectedDate);
        }
    });
    $("#tb_exportDateTo").datepicker({
        defaultDate: "+1w",
        changeMonth: true,
        numberOfMonths: 2,
        onClose: function (selectedDate) {
            $("#tb_exportDateFrom").datepicker("option", "maxDate", selectedDate);
        }
    });

    $("#tb_date_steeltrucks").datepicker();
    $("#tb_drivername_steeltrucks").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListOfSMWDrivers",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
    $("#tb_unit_steeltrucks").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListOfSMWUnits",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
    $("#tb_search_steeltrucks").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetTruckSchedule",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    $(function () {
        $("#GenDir_Modal-steeltrucks, #Createnew_Modal-steeltrucks, #Event_modal-steeltrucks").draggable({
            containment: "#container",
            cancel: '.ModalPadContent, .ModalExitButton',
            drag: function (event, ui) {
                $(this).css("opacity", "0.6");
            },
            stop: function (event, ui) {
                $(this).css("opacity", "1.0");
            }
        });
    });
}

var sortDir = "DESC";
var sortCol = "Date";
function OnSortClick(_this, col) {
    var $_this = $(_this);
    $(".td-sort-click").removeClass("asc");
    $(".td-sort-click").removeClass("active");
    $(".td-sort-click").removeClass("desc");
    $_this.addClass("active");

    if ((sortCol != col) || (sortCol == "")) {
        sortDir = "DESC";
    }

    sortCol = col;
    if (sortDir == "DESC") {
        sortDir = "ASC";
        $_this.addClass("asc");
    }
    else {
        sortDir = "DESC";
        $_this.addClass("desc");
    }

    LoadScheduleList(currDriverCall);
}

function LoadFontSize() {
    var fontsize = cookie.get("steeltrucks-fontsize");
    if ((fontsize != null) && (fontsize != "")) {
        $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
        $("#font-size-selector-steeltrucks option").each(function () {
            if ($(this).val() == fontsize) {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
    else {
        $("#font-size-selector-steeltrucks option").each(function () {
            if ($(this).val() == "small") {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
}

function FontSelection_SteelTrucks() {
    var fontsize = $("#font-size-selector-steeltrucks").val();
    $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
    cookie.set("steeltrucks-fontsize", fontsize, "30");
}

function LoadInit() {
    BuildDriverList();
    BuildDriverExportList();
    BuildGeneralDirections();
}

function BuildWeightTotals() {
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/BuildWeightTotals",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                $("#general-dir-holder").html(response);
            }
        },
        error: function (data) { }
    });
}

function BuildGeneralDirections() {
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/BuildGeneralDirections",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                genDirArray = new Array();
                for (var i = 0; i < response.length; i++) {
                    genDirArray[i] = response[i];
                }
            }
        },
        error: function (data) { }
    });
}

function BuildDriverList() {
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/GetDriverList",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                $("#driver-index-holder").html(response);
                openWSE.RemoveUpdateModal();
                if (currUser != "") {
                    ReCallDriver($("#" + currUser));
                }
                else {
                    ViewAllRecords();
                }
            }
        },
        error: function (data) { openWSE.RemoveUpdateModal(); }
    });
}

$(document.body).on("click", ".tsdivclick", function () {
    $('#tb_search_steeltrucks').val("Search for Truck Schedule");
    openWSE.LoadingMessage1("Loading Driver...");
    CallDriver(this);
});

function CallDriver(_this) {
    var _driverName = $.trim($(_this).find("span").text());
    var $this = $(_this).parent();
    var id = $(".tsactive").attr("id");
    currUser = $this.attr("id");
    $("#" + id).removeClass("tsactive");
    $("#" + id).find(".imgstsDiv").hide();
    $this.addClass("tsactive");
    var x = "<span class='pad-right-big font-bold' style='border-right:1px solid #555'>" + _driverName + "</span>";
    x += "<span id='recordCount-Holder' class='pad-right-big margin-left-big' style='border-right:1px solid #555'>Counting...</span>";
    var img = "<a href='#' onclick='CloseDriver();ViewAllRecords();return false;' class='td-cancel-btn margin-left-big'></a>";
    $("#CurrDriverName-Holder").html(x + img);

    currDriverCall = _driverName;
    LoadScheduleList(_driverName);
}

function ReCallDriver(_this) {
    var _driverName = $.trim($(_this).find("span").text());
    var $this = $(_this);
    currUser = $this.attr("id");
    $this.addClass("tsactive");
    $this.find(".imgstsDiv").show();
    var x = "<span class='pad-right-big font-bold' style='border-right:1px solid #555'>" + _driverName + "</span>";
    x += "<span id='recordCount-Holder' class='pad-right-big margin-left-big' style='border-right:1px solid #555'>Counting...</span>";
    var img = "<a href='#' onclick='CloseDriver();ViewAllRecords();return false;' class='td-cancel-btn margin-left-big'></a>";
    $("#CurrDriverName-Holder").html(x + img);

    currDriverCall = _driverName;
    LoadScheduleList(_driverName);
}

var recordCount = 0;
function LoadScheduleList(driver) {
    $("#errorPullingRecords").html("");
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/LoadScheduleList",
        type: "POST",
        data: '{ "driver": "' + escape(driver) + '","search": "' + escape($("#tb_search_steeltrucks").val()) + '","recordstopull": "' + recordsToPull + '","sortCol": "' + sortCol + '","sortDir": "' + sortDir + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            openWSE.RemoveUpdateModal();
            if (response != null) {
                var $holder = $("#driver-sch-holder");
                recordCount = 0;
                var x = "<div class='margin-top-sml'><table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>";
                x += "<tr class='myHeaderStyle'>";
                x += "<td width='55px'></td>";

                if (sortCol == "DriverName") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"DriverName\");' title='Sort by Driver Name'>Driver Name</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"DriverName\");' title='Sort by Driver Name'>Driver Name</td>";
                }

                if (sortCol == "Date") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"Date\");' title='Sort by Date'>Date</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"Date\");' title='Sort by Date'>Date</td>";
                }

                if (sortCol == "LastUpdated") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"LastUpdated\");' title='Sort by Last Updated'>Last Updated</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"LastUpdated\");' title='Sort by Last Updated'>Last Updated</td>";
                }

                if (sortCol == "Unit") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"Unit\");' title='Sort by Unit'>Unit</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"Unit\");' title='Sort by Unit'>Unit</td>";
                }

                if (sortCol == "GeneralDirection") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"GeneralDirection\");' title='Sort by Driection'>Driection</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"GeneralDirection\");' title='Sort by Driection'>Driection</td>";
                }

                if (sortCol == "Weight") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "' onclick='OnSortClick(this,\"Weight\");' title='Sort by Weight'>Weight (lbs)</td>";
                }
                else {
                    x += "<td class='td-sort-click' onclick='OnSortClick(this,\"Weight\");' title='Sort by Weight'>Weight (lbs)</td>";
                }

                x += "<td style='width: 75px;'>Entries</td>";
                x += "</tr>";

                if (data.d.length == 0) {
                    x += "</tbody></table></div><div class='emptyGridView' style='width: 1039px;'>No schedules found</div>";
                }
                else {
                    for (var i = 0; i < data.d.length; i++) {
                        var countIndex = "<div class='pad-top-sml pad-bottom-sml'>" + (i + 1) + "</div>";
                        x += "<tr class='myItemStyle GridNormalRow cursor-pointer' onclick=\"" + data.d[i][0] + "\" title='View Event Details'>";
                        x += "<td class='GridViewNumRow border-bottom' style='width: 55px;'>" + countIndex + "</td>";
                        x += "<td class='border-right border-bottom'>" + data.d[i][1] + "</td>";
                        x += "<td align='center' class='border-right border-bottom'>" + data.d[i][2] + "</td>";
                        x += "<td align='center' class='border-right border-bottom'>" + data.d[i][3] + "</td>";
                        x += "<td class='border-right border-bottom'>" + data.d[i][4] + "</td>";
                        x += "<td class='border-right border-bottom'>" + data.d[i][5] + "</td>";
                        x += "<td class='border-right border-bottom'>" + data.d[i][6] + "</td>";
                        x += "<td align='center' class='border-right border-bottom'>" + data.d[i][7] + "</td>";
                        x += "</tr>";
                        recordCount++;
                    }
                    x += "</tbody></table></div>";
                }
                $holder.html(x);
                $("#recordCount-Holder").html(recordCount + " Records");
                LoadFontSize();
            }
        },
        error: function (data) {
            canceledituser();
            openWSE.RemoveUpdateModal();
            $("#recordCount-Holder").html("ERROR!");
            $("#errorPullingRecords").html("There was a problem trying to get your records! Try changing the 'Records to Pull' to something smaller.");
        }
    });
}

function ViewAllRecords() {
    openWSE.LoadingMessage1("Loading All...");
    CloseDriver();
    var x = "<span class='pad-right-big font-bold' style='border-right:1px solid #555'>All Schedules</span>";
    x += "<span id='recordCount-Holder' class='pad-right-big margin-left-big'>Counting...</span>";
    $("#CurrDriverName-Holder").html(x);
    LoadScheduleList("");
}

function CloseDriver() {
    var id = $(".tsactive").attr("id");
    $("#" + id).removeClass("tsactive");
    $("#" + id).find(".imgstsDiv").hide();
    $("#CurrDriverName-Holder").html("");
    $("#errorPullingRecords").html("");
    currUser = "";
    currDriverCall = "";
}

function BuildGenDirDropdown(id) {
    var selectDir = document.getElementById(id);
    selectDir.innerHTML = "";
    for (var i = 0; i < genDirArray.length; i++) {
        var optionDir = document.createElement("option");
        optionDir.text = genDirArray[i];
        try {
            // for IE earlier than version 8
            selectDir.add(optionDir, optionDir.options[null]);
        }
        catch (e) {
            selectDir.add(optionDir, null);
        }
    }
}

function SearchTruckSchedule() {
    if (CheckIfSearchable()) {
        openWSE.LoadingMessage1("Collecting Results...");
        CloseDriver();

        var searchVal = $("#tb_search_steeltrucks").val();
        if (($.trim(searchVal) != "") || ($.trim(searchVal) != "Search for Truck Schedule")) {
            var x = "<span class='pad-right-big font-bold' style='border-right:1px solid #555'>Search Results</span>";
            x += "<span id='recordCount-Holder' class='pad-right-big margin-left-big' style='border-right:1px solid #555'>Counting...</span>";
            var img = "<a href='#' onclick='$(\"#tb_search_steeltrucks\").val(\"Search for Truck Schedule\");CloseDriver();ViewAllRecords();return false;' class='td-cancel-btn margin-left-big'></a>";
            $("#CurrDriverName-Holder").html(x + img);
        }

        LoadScheduleList("");
    }
}

function KeyPressSearch_SteelTrucks(event) {
    try {
        if (event.which == 13) {
            SearchTruckSchedule();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            SearchTruckSchedule();
        }
        delete evt;
    }
}

function CheckIfSearchable() {
    var searchVal = $("#tb_search_steeltrucks").val();
    if (($.trim(searchVal) == "") || ($.trim(searchVal) == "Search for Truck Schedule")) {
        return false;
    }

    return true;
}

function RecordstoSelect_SteelTrucks() {
    //var reload = $("#errorPullingRecords").html();
    //$("#errorPullingRecords").html("");
    //if (reload != "") {
    recordsToPull = $("#RecordstoSelect").val();
    RefreshScheduleList();
    //}
}

function RefreshScheduleList() {
    $("#errorPullingRecords").html("");
    if (currUser != "") {
        var _driverName = $(".tsactive").attr("id");
        if (currUser == _driverName) {
            openWSE.LoadingMessage1("Retrying...");
            ReCallDriver($("#" + currUser));
        }
    }
    else {
        ViewAllRecords();
    }
}




/* -------------------- */
/* Scheduled Event Code */
/* -------------------- */
var needScheduleReload = false;
function LoadCreateNew(existingDriver) {
    if ($("#CreateTruckSch-element").css("display") != "block") {
        BuildGenDirDropdown("dd_generaldirection_steeltrucks");
        $("#tb_drivername_steeltrucks").val(existingDriver);

        var today = new Date();
        var dd = today.getDate();
        if (dd < 10)
            dd = "0" + dd.toString();

        var mm = today.getMonth() + 1;
        if (mm < 10)
            mm = "0" + mm.toString();

        var yyyy = today.getFullYear()

        $("#tb_date_steeltrucks").val(mm + "/" + dd + "/" + yyyy);
        $("#tb_unit_steeltrucks").val("");
        openWSE.LoadModalWindow(true, "CreateTruckSch-element", "New Truck Schedule Wizard");
        if ($.trim(existingDriver) == "") {
            $("#tb_drivername_steeltrucks").focus();
        }
        else {
            $("#tb_unit_steeltrucks").focus();
        }
    } 
    else {
        openWSE.LoadModalWindow(false, "CreateTruckSch-element", "");
    }
}

$(document.body).on("keydown", "#tb_drivername_steeltrucks, #tb_unit_steeltrucks", function (event) {
    try {
        if (event.which == 13) {
            CallCreateNew();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            CallCreateNew();
        }
        delete evt;
    }
});

function CallCreateNew() {
    var driver = $.trim($("#tb_drivername_steeltrucks").val());
    var date = $.trim($("#tb_date_steeltrucks").val());
    var dir = $.trim($("#dd_generaldirection_steeltrucks").val());
    var unit = $.trim($("#tb_unit_steeltrucks").val());

    if ((driver == "") || (date == "") || (unit == "")) {
        $("#createNewError").html("Must have all fields filled out.");
        setTimeout(function () {
            $("#createNewError").html("");
        }, 3000);
    }
    else {
        openWSE.LoadingMessage1("Creating Schedule...");
        $.ajax({
            url: "Apps/SteelTrucks/SteelTrucks.asmx/CallCreateNew",
            type: "POST",
            data: '{ "_driver": "' + driver + '","_date": "' + date + '","_dir": "' + dir + '","_unit": "' + unit + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
                OpenEvent(driver.replace(" ", "_"), date, unit, dir);
                LoadCreateNew("");
                LoadInit();
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
                $("#createNewError").html("Error. Could not add new schedule.");
                setTimeout(function () {
                    $("#createNewError").html("");
                }, 3000);
            }
        });
    }
}

var driverVar = "";
var dateVar = "";
var unitVar = "";
var dirVar = "";
var editMode = false;
function EditHeaderEvent() {
    if (editMode) {
        openWSE.LoadingMessage1("Updating Schedule...");
        $.ajax({
            url: "Apps/SteelTrucks/SteelTrucks.asmx/EventHeaderEdit",
            data: "{ '_Orgdriver': '" + driverVar + "', '_Orgdate': '" + dateVar + "', '_Orgunit': '" + unitVar + "', '_Orgdir': '" + dirVar + "', '_date': '" + $("#eventEdit-Date-tb").val() + "', '_unit': '" + $("#eventEdit-Unit-tb").val() + "', '_dir': '" + $("#eventEdit-Dir-tb").val() + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataFilter: function (data) { return data; },
            success: function (data) {
                if (data.d != null) {
                    needScheduleReload = true;
                    $("#printBtn-steeltrucks,#deleteEditBtn-steeltrucks").show();
                    $("#CanceleditBtn-steeltrucks").hide();
                    $("#editBtn-steeltrucks").html('<span class="float-left td-edit-btn margin-right-sml" style="padding: 0px!important;"></span>Edit');
                    OpenEvent(driverVar, data.d[0], data.d[1], data.d[2]);
                    driverVar = "";
                    unitVar = "";
                    dateVar = "";
                    dirVar = "";

                    openWSE.RemoveUpdateModal();
                }
            },
            error: function (data) {
                openWSE.RemoveUpdateModal();
            }
        });
    }
    else {
        editMode = true;
        $("#printBtn-steeltrucks,#deleteEditBtn-steeltrucks").hide();
        $("#CanceleditBtn-steeltrucks").show();
        $("#editBtn-steeltrucks").html('<span class="float-left td-update-btn margin-right-sml" style="padding: 0px!important;"></span>Update');
        dirVar = $("#eventEdit-Dir").text();
        unitVar = $("#eventEdit-Unit").text();
        dateVar = $("#eventEdit-Date").text();
        driverVar = $("#eventEdit-Driver").text();

        $("#eventEdit-Dir").html("<select id='eventEdit-Dir-tb' style='width:140px'></select>");
        $("#eventEdit-Unit").html("<input type='text' id='eventEdit-Unit-tb' value='" + unitVar + "' class='textEntry' style='width:100px' />");
        $("#eventEdit-Date").html("<input type='text' id='eventEdit-Date-tb' value='" + dateVar + "' class='textEntry' style='width:100px' />");

        BuildGenDirDropdown("eventEdit-Dir-tb");

        $("#eventEdit-Dir-tb").val(dirVar);
        $("#eventEdit-Date-tb").datepicker();
        $("#eventEdit-Unit-tb").autocomplete({
            minLength: 0,
            autoFocus: true,
            source: function (request, response) {
                $.ajax({
                    url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListOfSMWUnits",
                    data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataFilter: function (data) { return data; },
                    success: function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }))
                    }
                });
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
    }
}

function CancelEditHeaderEvent() {
    editMode = false;
    $("#eventEdit-Date").html(dateVar);
    $("#eventEdit-Unit").html(unitVar);
    $("#eventEdit-Dir").html(dirVar);
    $("#printBtn-steeltrucks,#deleteEditBtn-steeltrucks").show();
    $("#CanceleditBtn-steeltrucks").hide();
    $("#editBtn-steeltrucks").html('<span class="float-left td-edit-btn margin-right-sml" style="padding: 0px!important;"></span>Edit');
    driverVar = "";
    unitVar = "";
    dateVar = "";
    dirVar = "";
}

function DeleteHeaderEvent() {
    openWSE.ConfirmWindow("Are you sure you want to delete this schedule?",
        function () {
            openWSE.LoadingMessage1("Deleting Schedule...");
            $.ajax({
                url: "Apps/SteelTrucks/SteelTrucks.asmx/EventHeaderDelete",
                type: "POST",
                data: '{ "_driver": "' + $("#eventEdit-Driver").html() + '","_date": "' + $("#eventEdit-Date").html() + '","_unit": "' + $("#eventEdit-Unit").html() + '","_dir": "' + $("#eventEdit-Dir").html() + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    openWSE.RemoveUpdateModal();
                    needScheduleReload = true;
                    LoadEventEditor();
                },
                error: function (data) {
                    openWSE.RemoveUpdateModal();
                }
            });
        }, null);
}

function OpenEvent(driver, date, unit, dir) {
    openWSE.LoadingMessage1("Loading Schedule...");
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/LoadEvent",
        type: "POST",
        data: '{ "driver": "' + driver + '","date": "' + date + '","unit": "' + unit + '","dir": "' + dir + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            openWSE.RemoveUpdateModal();
            if (response != null) {
                $("#printBtn-steeltrucks").attr("href", "Apps/SteelTrucks/SchedulePrint.aspx?user=" + driver.replace(" ", "_") + "&date=" + date + "&unit=" + unit + "&gd=" + dir);
                $("#eventEdit-Driver").html(driver.replace("_", " "));
                $("#eventEdit-Date").html(date);
                $("#eventEdit-Unit").html(unit);
                $("#eventEdit-Dir").html(dir);
                $("#EventList-steeltrucks").html(response);
                if (!editMode) {
                    LoadEventEditor();
                }
                editMode = false;
                ReloadAutoCompleteAdd();
            }
        },
        error: function (data) {
            canceledituser();
            openWSE.RemoveUpdateModal();
        }
    });
}

function AddRecordKeyPress_SteelTrucks(event, driver, date, unit, dir) {
    try {
        if (event.which == 13) {
            AddEvent(driver, date, unit, dir);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddEvent(driver, date, unit, dir);
            return false;
        }
        delete evt;
    }
}

function AddEvent(driver, date, unit, dir) {
    openWSE.LoadingMessage1("Adding Event...");

    // Get textbox values
    var customer = $("#tbCustomerNew").val();
    var city = $("#tbCityNew").val();
    var order = $("#tbOrderNew").val();
    var weight = $("#tbWeightNew").val();

    needScheduleReload = true;
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/EventActionAdd",
        type: "POST",
        data: '{ "_customer": "' + customer + '","_city": "' + city + '","_order": "' + order + '","_weight": "' + weight + '","_driver": "' + driver + '","_date": "' + date + '","_unit": "' + unit + '","_dir": "' + dir + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            openWSE.RemoveUpdateModal();
            if (response != null) {
                $("#EventList-steeltrucks").html(response);
                ReloadAutoCompleteAdd();
            }
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
        }
    });
}

function EditRecordKeyPress_SteelTrucks(event, id) {
    try {
        if (event.which == 13) {
            UpdateEditEvent(id);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            UpdateEditEvent(id);
            return false;
        }
        delete evt;
    }
}

var fixHelper = function (e, ui) {
    ui.children().each(function () {
        $(this).width($(this).width());
    });
    return ui;
};


var customerEdit = "";
var cityEdit = "";
var orderEdit = "";
var weightEdit = "";
var tempActionBtns = "";
function EditEvent(id) {
    customerEdit = $("#" + id + "-customer").text();
    cityEdit = $("#" + id + "-city").text();
    orderEdit = $("#" + id + "-order").text();
    weightEdit = $("#" + id + "-weight").text();
    tempActionBtns = $("#" + id + "-actions").html();

    var onKeyUp = "onkeyup='EditRecordKeyPress_SteelTrucks(event, \"" + id + "\")'";

    $("#" + id + "-seq").css("cursor", "move");
    $("#" + id + "-customer").html("<input type='text' id='eventcustomerEdit' value='" + customerEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
    $("#" + id + "-city").html("<input type='text' id='eventcityEdit' value='" + cityEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
    $("#" + id + "-order").html("<input type='text' id='eventorderEdit' value='" + orderEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
    $("#" + id + "-weight").html("<input type='text' id='eventweightEdit' value='" + weightEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");

    var updateButton = "<a href='#' class='td-update-btn margin-right' onclick='UpdateEditEvent(\"" + id + "\");return false;' title='Update'></a>";
    var cancelButton = "<a href='#' class='td-cancel-btn' onclick='CancelEditEvent(\"" + id + "\");return false;' title='Cancel'></a>";
    $("#" + id + "-actions").html(updateButton + cancelButton);

    $(".AddNewEventRow,.edit-button-event,.delete-button-event").css("visibility", "hidden");

    $("#eventcustomerEdit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCustomersTS",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
    $("#eventcityEdit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCityTS",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    $("#EventSort-steeltrucks > table tbody").sortable({
        axis: 'y',
        cancel: '.non-moveable, .myHeaderStyle',
        helper: fixHelper,
        containment: '#EventSort-steeltrucks',
        opacity: 0.9
    });

    $("#eventcustomerEdit").focus();
}

function UpdateEditEvent(id) {
    needScheduleReload = true;
    openWSE.LoadingMessage1("Updating Row...");

    var dir = $("#eventEdit-Dir").text();
    var unit = $("#eventEdit-Unit").text();
    var date = $("#eventEdit-Date").text();
    var driver = $("#eventEdit-Driver").text();

    var customer = $("#eventcustomerEdit").val();
    var city = $("#eventcityEdit").val();
    var order = $("#eventorderEdit").val();
    var weight = $("#eventweightEdit").val();

    var eventList = BuildEventListOrder();

    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/EventActionEditUpdate",
        type: "POST",
        data: '{ "_id": "' + id + '","_driver": "' + driver + '","_date": "' + date + '","_unit": "' + unit + '","_dir": "' + dir + '","_customer": "' + customer + '","_city": "' + city + '","_order": "' + order + '","_weight": "' + weight + '","_eventList": "' + eventList + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            openWSE.RemoveUpdateModal();
            customerEdit = "";
            cityEdit = "";
            orderEdit = "";
            weightEdit = "";
            tempActionBtns = "";
            if (response != null) {
                $("#EventList-steeltrucks").html(response);
                ReloadAutoCompleteAdd();
            }
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
        }
    });
}

function CancelEditEvent(id) {
    editMode = true;
    var dir = $("#eventEdit-Dir").text();
    var unit = $("#eventEdit-Unit").text();
    var date = $("#eventEdit-Date").text();
    var driver = $("#eventEdit-Driver").text();

    customerEdit = "";
    cityEdit = "";
    orderEdit = "";
    weightEdit = "";
    tempActionBtns = "";

    OpenEvent(driver, date, unit, dir);

    $(".AddNewEventRow,.edit-button-event,.delete-button-event").css("visibility", "visible");
}

function DeleteEvent(id, driver, date, unit, dir) {
    openWSE.ConfirmWindow("Are you sure you want to delete this row?",
        function () {
            var eventList = BuildEventListOrder();
            eventList = eventList.replace(id + ";", "");
            needScheduleReload = true;
            openWSE.LoadingMessage1("Deleting Row...");
            $.ajax({
                url: "Apps/SteelTrucks/SteelTrucks.asmx/EventActionDelete",
                type: "POST",
                data: '{ "_id": "' + id + '","_driver": "' + driver + '","_date": "' + date + '","_unit": "' + unit + '","_dir": "' + dir + '","_eventList": "' + eventList + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    openWSE.RemoveUpdateModal();
                    if (response != null) {
                        $("#EventList-steeltrucks").html(response);
                        ReloadAutoCompleteAdd();
                    }
                },
                error: function (data) {
                    openWSE.RemoveUpdateModal();
                }
            });
        }, null);
}

function LoadEventEditor() {
    if ($("#TruckEvent-element").css("display") != "block") {
        openWSE.LoadModalWindow(true, "TruckEvent-element", "Truck Event Editor");
    }
    else {
        openWSE.LoadModalWindow(false, "TruckEvent-element", "");
        setTimeout(function () {
            $("#eventEdit-Driver").html("");
            $("#eventEdit-Date").html("");
            $("#eventEdit-Unit").html("");
            $("#eventEdit-Dir").html("");
            $("#EventList-steeltrucks").html("");
            driverVar = "";
            unitVar = "";
            dateVar = "";
            dirVar = "";
            $("#printBtn-steeltrucks").attr("href", "#");
            editMode = false;
            $("#printBtn-steeltrucks,#deleteEditBtn-steeltrucks").show();
            $("#editBtn-steeltrucks").html('<span class="float-left td-edit-btn margin-right-sml" style="padding: 0px!important;"></span>Edit');
            if (needScheduleReload) {
                openWSE.LoadingMessage1("Reloading...");
                LoadInit();
            }
            needScheduleReload = false;
        }, openWSE_Config.animationSpeed);
    }
}

function ReloadAutoCompleteAdd() {
    LoadFontSize();
    $(".customername-tb-autosearch").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCustomersTS",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
    $(".city-tb-autosearch").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCityTS",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
}

function BuildEventListOrder() {
    var eventList = "";
    $("#EventSort-steeltrucks").find(".EventRow").each(function (index) {
        eventList += $(this).find(".GridViewNumRow").attr("id").replace("-seq", "") + ";";
    });
    return eventList;
}


/* ---------------- */
/* DRIVER EDIT CODE */
/* ---------------- */
function showButtons(xdelete, xadd, xedit) {
    if (xdelete.substring(0, 3) != 'x_-') {
        var temp = xadd.replace("a_", "");
        if (($("#expand_" + temp).hasClass("tsactive") != true) && ($("#expand_" + temp).hasClass("tsdiv-active-edit") != true)) {
            document.getElementById(xdelete).style.display = 'block';
            document.getElementById(xadd).style.display = 'block';
            document.getElementById(xedit).style.display = 'block';
        }
    }
}

function hideButtons(xdelete, xadd, xedit) {
    if (xdelete.substring(0, 3) != 'x_-') {
        var temp = xadd.replace("a_", "");
        if (($("#expand_" + temp).hasClass("tsactive") != true) && ($("#expand_" + temp).hasClass("tsdiv-active-edit") != true)) {
            document.getElementById(xdelete).style.display = 'none';
            document.getElementById(xadd).style.display = 'none';
            document.getElementById(xedit).style.display = 'none';
        }
    }
}

function deleteUser(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this Driver and all of the schedules associated with this?",
        function () {
            var _driverName = _this.getAttribute("href").replace("#delete_", "");
            openWSE.LoadingMessage1("Deleting " + _driverName + "...");
            $.ajax({
                url: "Apps/SteelTrucks/SteelTrucks.asmx/DriverEdit",
                type: "POST",
                data: '{ "_case": "' + "deleteUser" + '","_driver": "' + escape(_driverName) + '","_oldname": "' + "" + '","_newname": "' + "" + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var response = data.d;
                    if (response != null) {
                        $("#driver-index-holder").html(response);
                        $("#CurrDriverName-Holder").html("");
                        openWSE.RemoveUpdateModal();
                        if (currUser != "") {
                            if (currUser != "expand_" + _driverName) {
                                ReCallDriver($("#" + currUser));
                            }
                            else {
                                ViewAllRecords();
                            }
                        }
                        else {
                            ViewAllRecords();
                        }
                    }
                },
                error: function (data) {
                    canceledituser();
                    openWSE.RemoveUpdateModal();
                }
            });
        }, null);
}

function editUser(_this, user) {
    canceledituser();
    var $this = $(_this).closest('div');
    $this.addClass("tsdiv-active-edit");
    $this.append("<input id='tb_edituserentry' maxlength='50' type='text' value='" + user.replace(/_/g, " ") + "' onkeypress='OnKeyPress_DriverEdit(event,\"" + user + "\")' class='textEntry float-left' style='width: 220px;' />");
    $this.append("<a id='driverEditBtns-cancel' href='#' onclick='canceledituser();return false;' class='td-cancel-btn'></a>");
    $this.append("<a id='driverEditBtns-update' href='#' onclick=\"updateUser('" + user + "');return false;\" class='td-update-btn'></a>");
}

function updateUser(user) {
    var _driverName = document.getElementById('tb_edituserentry').value.replace(/ /g, "_");
    openWSE.LoadingMessage1("Updating " + _driverName + "...");
    $.ajax({
        url: "Apps/SteelTrucks/SteelTrucks.asmx/DriverEdit",
        type: "POST",
        data: '{ "_case": "' + "updateUser" + '","_driver": "' + "" + '","_oldname": "' + escape(user) + '","_newname": "' + escape(_driverName) + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                $("#driver-index-holder").html(response);
                openWSE.RemoveUpdateModal();
                if (currUser != "") {
                    ReCallDriver($("#" + "expand_" + _driverName));
                }
            }
        },
        error: function (data) {
            canceledituser();
            openWSE.RemoveUpdateModal();
        }
    });
}

function canceledituser() {
    var id = $(".tsdiv-active-edit").attr("id");
    $("#" + id).removeClass("tsdiv-active-edit");
    $("#tb_edituserentry").remove();
    $("#driverEditBtns-cancel").remove();
    $("#driverEditBtns-update").remove();
    $("#" + id).find("img").show();
}

function OnKeyPress_DriverEdit(event, user) {
    try {
        if (event.which == 13) {
            updateUser(user);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            updateUser(user);
        }
        delete evt;
    }
}


/* ------------------- */
/* DIRECTION EDIT CODE */
/* ------------------- */
function LoadGenDirEditor() {
    if ($("#DirEdit-element").css("display") != "block") {
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/BuildEditor",
            data: "{ }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#GenDirList-steeltrucks").html(data.d);
                openWSE.LoadModalWindow(true, "DirEdit-element", "Direction Editor");
            },
            error: function (data) {
                openWSE.AlertWindow("There was an error loading direction list. Please try again");
            }
        });
    } else {
        openWSE.LoadModalWindow(false, "DirEdit-element", "");
        setTimeout(function () {
            $("#GenDirList-steeltrucks").html("");
        }, openWSE_Config.animationSpeed);
    }
}

function deletegd(id, dir) {
    openWSE.ConfirmWindow("Are you sure you want to delete this General Direction? (Any associated schedule will default to Kansas.)",
        function () {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/DeleteDirection",
                data: "{ 'id': '" + id + "','gendir': '" + escape(dir) + "' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (!openWSE.ConvertBitToBoolean(data.d)) {
                        openWSE.AlertWindow("Error deleting direction. Please try again.");
                    } else {
                        $("#GenDirList-steeltrucks").html(data.d);
                        BuildGeneralDirections();
                    }
                },
                error: function (data) {
                    openWSE.AlertWindow("There was an error deleting direction. Please try again");
                }
            });
        }, null);
}

function addgd() {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/AddDirection",
        data: "{ 'gendir': '" + escape(document.getElementById("tb_addgendir").value) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            }
            else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            }
            else {
                $("#GenDirList-steeltrucks").html(data.d);
                BuildGeneralDirections();
            }
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error adding direction. Please try again");
        }
    });
}

function editgd(dir) {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/EditDirection",
        data: "{ 'gendir': '" + escape(dir) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error editing direction. Please try again");
        }
    });
}

function updategd(id) {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/UpdateDirection",
        data: "{ 'id':'" + id + "','gendir': '" + escape(document.getElementById("tb_editgendir").value) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            } else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#GenDirList-steeltrucks").html(data.d);
            }
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error updating direction. Please try again");
        }
    });
}

function canceleditgd() {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/BuildEditor",
        data: "{ }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error loading direction list. Please try again");
        }
    });
}

function OnKeyPress_DirEditNew(event, id) {
    try {
        if (event.which == 13) {
            if (id == "") {
                addgd();
            }
            else {
                updategd(id);
            }
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            if (id == "") {
                addgd();
            }
            else {
                updategd(id);
            }
        }
        delete evt;
    }
}