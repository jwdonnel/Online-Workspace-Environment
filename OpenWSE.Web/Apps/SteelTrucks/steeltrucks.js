var steelTruckFunctions = function () {
    var currentUser = "";
    var currDriverCall = "";
    var genDirArray;
    var initSearchPlaceholder = "Search Schedule...";
    var loaderHolder = "#steeltrucks-load";

    var listOfSMWDriversData = new Array();
    var listOfSMWUnitsData = new Array();
    var listCustomersTSData = new Array();
    var listCityTSData = new Array();

    function LoadInit() {
        BuildDriverList();
        BuildGeneralDirections();
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

        var vals = $.trim($("#hf_AutoCompleteList_SteelTrucks").val());
        vals = unescape(vals);
        vals = vals.replace(/~/g, " ");
        var data = JSON.parse(vals);

        if (data.length === 4) {
            listOfSMWDriversData = data[0];
            listOfSMWUnitsData = data[1];
            listCustomersTSData = data[2];
            listCityTSData = data[3];
        }

        $("#tb_date_steeltrucks").datepicker();
        $("#tb_drivername_steeltrucks").autocomplete({
            minLength: 0,
            autoFocus: true,
            source: function (request, response) {
                if (listOfSMWDriversData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listOfSMWDriversData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListOfSMWDrivers", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
        $("#tb_unit_steeltrucks").autocomplete({
            minLength: 0,
            autoFocus: true,
            source: function (request, response) {
                if (listOfSMWUnitsData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listOfSMWUnitsData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListOfSMWUnits", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });

        $(function () {
            $("#GenDir_Modal-steeltrucks, #Createnew_Modal-steeltrucks, #Event_modal-steeltrucks").draggable({
                containment: "#main_container",
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
    function SearchAutoCompleteList(arr, val) {
        var newArr = new Array();
        if (val === "") {
            newArr = arr;
        }
        else {
            val = val.toLowerCase();
            for (var i = 0; i < arr.length; i++) {
                if (arr[i].toLowerCase().indexOf(val) !== -1) {
                    newArr.push(arr[i]);
                    if (newArr.length === 20) {
                        break;
                    }
                }
            }
        }

        newArr.sort();
        return newArr;
    }

    function Refresh() {
        $("#errorPullingRecords").html("");
        if (currentUser != "") {
            var _driverName = $(".tsactive").attr("data-id");
            if (currentUser == _driverName) {
                loadingPopup.Message("Retrying...");
                ReCallDriver($(".tsdiv[data-id='" + currentUser + "']"));
            }
        }
        else {
            ViewAllRecords();
        }
    }

    function LoadGenDirEditor() {
        if ($("#DirEdit-element").css("display") != "block") {
            openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/BuildEditor", "{ }", null, function (data) {
                $("#GenDirList-steeltrucks").html(data.d);
                openWSE.LoadModalWindow(true, "DirEdit-element", "Direction Editor");
            }, function (data) {
                openWSE.AlertWindow("There was an error loading direction list. Please try again");
            });
        } else {
            openWSE.LoadModalWindow(false, "DirEdit-element", "");
            setTimeout(function () {
                $("#GenDirList-steeltrucks").html("");
            }, openWSE_Config.animationSpeed);
        }
    }
    function BuildGeneralDirections() {
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/BuildGeneralDirections", "", null, function (data) {
            var response = data.d;
            if (response != null) {
                genDirArray = new Array();
                for (var i = 0; i < response.length; i++) {
                    genDirArray[i] = response[i];
                }
            }
        });
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
    function BuildDriverList() {
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/GetDriverList", "", null, function (data) {
            var response = data.d;
            if (response != null) {
                $("#driver-index-holder").html(response);

                var optionList = "<option value='all'>All Drivers</option>";
                $("#driver-index-holder").find(".tsdiv").each(function () {
                    var name = $.trim($(this).find(".tsdivclick > span").html());
                    optionList += "<option value='" + name + "'>" + name.replace("_", " ") + "</option>";
                });
                $("#driverListOptions-steeltrucks").html(optionList);

                loadingPopup.RemoveMessage(loaderHolder);
                if (currentUser != "") {
                    ReCallDriver($(".tsdiv[data-id='" + currentUser + "']"));
                }
                else {
                    ViewAllRecords();
                }
            }
        });
    }

    function ExportToExcel_steeltrucks() {
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/ExportToExcel", "{ '_date': '" + $("#eventEdit-Date").html() + "','_driver': '" + $("#eventEdit-Driver").html() + "','_unit': '" + $("#eventEdit-Unit").html() + "','_dir': '" + $("#eventEdit-Dir").html() + "' }", null, function (data) {
            if (data.d != "") {
                $.fileDownload(data.d);
            }
            else {
                openWSE.AlertWindow("No records to export");
            }
        });
    }
    function ExportToExcelAll_steeltrucks() {
        $("#btn_exportAll").hide();
        $("#exportingNow").show();
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/ExportToExcelAll", "{ '_dateFrom': '" + $("#tb_exportDateFrom").val() + "','_dateTo': '" + $("#tb_exportDateTo").val() + "','_driver': '" + $("#driverListOptions-steeltrucks").val() + "' }", null, function (data) {
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
        });
    }

    function RecordstoSelect() {
        RefreshScheduleList();
    }

    function ReCallDriver(_this) {
        var _driverName = $.trim($(_this).find("span").text());
        var $this = $(_this);
        currentUser = $this.attr("data-id");
        $this.addClass("tsactive");
        BuildCurrDriverNameHolder(_driverName, true);
        currDriverCall = _driverName;
        LoadScheduleList(_driverName);
    }
    $(document.body).on("click", ".tsdivclick", function () {
        $('#tb_search_steeltrucks').val("");
        loadingPopup.Message("Loading Driver...", loaderHolder);
        var _driverName = $.trim($(this).find("span").text());
        var $this = $(this).parent();
        var id = $(".tsactive").attr("data-id");
        currentUser = $this.attr("data-id");
        $(".tsdiv[data-id='" + id + "']").removeClass("tsactive");
        $this.addClass("tsactive");
        BuildCurrDriverNameHolder(_driverName, true);
        currDriverCall = _driverName;
        LoadScheduleList(_driverName);
    });
    function RefreshScheduleList() {
        $("#errorPullingRecords").html("");
        if (currentUser != "") {
            var _driverName = $(".tsactive").attr("data-id");
            if (currentUser == _driverName) {
                loadingPopup.Message("Retrying...", loaderHolder);
                ReCallDriver($(".tsdiv[data-id='" + currentUser + "']"));
            }
        }
        else {
            ViewAllRecords();
        }
    }

    function CloseDriver(noClearSearch) {
        var id = $(".tsactive").attr("data-id");
        $(".tsdiv[data-id='" + id + "']").removeClass("tsactive");
        $("#CurrDriverName-Holder").html("");
        $("#errorPullingRecords").html("");
        if (!noClearSearch) {
            $("#tb_search_steeltrucks").val("");
        }
        currentUser = "";
        currDriverCall = "";
    }
    function ViewAllRecords() {
        loadingPopup.Message("Loading All...", loaderHolder);
        CloseDriver();
        BuildCurrDriverNameHolder("All Schedules", false);
        LoadScheduleList("");
    }
    function BuildCurrDriverNameHolder(_driverName, includeClose) {
        var x = "";
        var img = "";
        if (includeClose) {
            x = "<span class='pad-right-big font-bold float-left' style='border-right:1px solid #555'>" + _driverName + "</span>";
            x += "<span id='recordCount-Holder' class='pad-right-big margin-left-big float-left' style='border-right:1px solid #555'>Counting...</span>";
            img = "<a href='#' onclick='steelTruckFunctions.ViewAllRecords();return false;' class='td-cancel-btn margin-left-big margin-right float-left'></a>";
        }
        $("#CurrDriverName-Holder").html(x + img + "<div class='clear'></div>");
    }

    // Search Functions
    function SearchTruckSchedule() {
        if (CheckIfSearchable()) {
            loadingPopup.Message("Collecting Results...", loaderHolder);
            CloseDriver(true);

            var searchVal = $("#tb_search_steeltrucks").val();
            if (($.trim(searchVal) != "") || ($.trim(searchVal) != initSearchPlaceholder)) {
                BuildCurrDriverNameHolder("Search Results", true);
            }

            LoadScheduleList("");
        }
    }
    function KeyPressSearch(event) {
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
        if (($.trim(searchVal) == "") || ($.trim(searchVal) == initSearchPlaceholder)) {
            return false;
        }

        return true;
    }

    // Load Table Functions
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

    var allowSort = false;
    var recordCount = 0;
    function LoadScheduleList(driver) {
        loadingPopup.Message("Loading...", loaderHolder);

        $("#errorPullingRecords").html("");
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/LoadScheduleList", '{ "driver": "' + escape(driver) + '","search": "' + escape($("#tb_search_steeltrucks").val()) + '","recordstopull": "' + $("#RecordstoSelect_SteelTrucks").val() + '","sortCol": "' + sortCol + '","sortDir": "' + sortDir + '" }', null, function (data) {
            var response = data.d;
            loadingPopup.RemoveMessage(loaderHolder);
            if (response != null) {
                var $holder = $("#driver-sch-holder");
                recordCount = 0;

                var sortStyle = " style='cursor: default!important; text-decoration: none!important;'";

                var x = "<div class='margin-top-sml'><table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>";
                x += "<tr class='myHeaderStyle'>";

                var onclickSortDriverName = sortStyle;
                if (allowSort) {
                    onclickSortDriverName = " onclick='steelTruckFunctions.OnSortClick(this,\"DriverName\");' title='Sort by Driver Name'";
                }
                if (sortCol == "DriverName") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortDriverName + ">Driver Name</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortDriverName + ">Driver Name</td>";
                }

                var onclickSortDate = sortStyle;
                if (allowSort) {
                    onclickSortDate = " onclick='steelTruckFunctions.OnSortClick(this,\"Date\");' title='Sort by Date'";
                }
                if (sortCol == "Date") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortDate + ">Date</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortDate + ">Date</td>";
                }

                var onclickSortLastUpdated = sortStyle;
                if (allowSort) {
                    onclickSortLastUpdated = " onclick='steelTruckFunctions.OnSortClick(this,\"LastUpdated\");' title='Sort by Last Updated'";
                }
                if (sortCol == "LastUpdated") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortLastUpdated + ">Last Updated</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortLastUpdated + ">Last Updated</td>";
                }

                var onclickSortUnit = sortStyle;
                if (allowSort) {
                    onclickSortUnit = " onclick='steelTruckFunctions.OnSortClick(this,\"Unit\");' title='Sort by Unit'";
                }
                if (sortCol == "Unit") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortUnit + ">Unit</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortUnit + ">Unit</td>";
                }

                var onclickSortGeneralDirection = sortStyle;
                if (allowSort) {
                    onclickSortGeneralDirection = " onclick='steelTruckFunctions.OnSortClick(this,\"GeneralDirection\");' title='Sort by Driection'";
                }
                if (sortCol == "GeneralDirection") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortGeneralDirection + ">Driection</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortGeneralDirection + ">Driection</td>";
                }

                var onclickSortWeight = sortStyle;
                if (allowSort) {
                    onclickSortWeight = " onclick='steelTruckFunctions.OnSortClick(this,\"Weight\");' title='Sort by Weight'";
                }
                if (sortCol == "Weight") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortWeight + ">Weight (lbs)</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortWeight + ">Weight (lbs)</td>";
                }

                var onclickSortAdditionalInfo = sortStyle;
                if (allowSort) {
                    onclickSortAdditionalInfo = " onclick='steelTruckFunctions.OnSortClick(this,\"AdditionalInfo\");' title='Sort by Number'";
                }
                if (sortCol == "AdditionalInfo") {
                    x += "<td class='td-sort-click active " + sortDir.toLocaleLowerCase() + "'" + onclickSortAdditionalInfo + ">Number</td>";
                }
                else {
                    x += "<td class='td-sort-click'" + onclickSortAdditionalInfo + ">Number</td>";
                }

                x += "</tr>";

                if (data.d.length == 0) {
                    x += "</tbody></table></div><div class='emptyGridView' style='width: 1039px;'>No schedules found</div>";
                }
                else {
                    for (var i = 0; i < data.d.length; i++) {
                        var countIndex = "<div class='pad-top-sml pad-bottom-sml'>" + (i + 1) + "</div>";
                        x += "<tr class='myItemStyle GridNormalRow cursor-pointer' onclick=\"" + data.d[i][0] + "\" title='View Event Details'>";
                        x += "<td class='border-bottom'>" + data.d[i][1] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][2] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][3] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][4] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][5] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][6] + "</td>";
                        x += "<td class='border-bottom'>" + data.d[i][7] + "</td>";
                        x += "</tr>";
                        recordCount++;
                    }
                    x += "</tbody></table></div>";
                }
                $holder.html(x);
                $("#recordCount-Holder").html(recordCount + " Records");
            }
        }, function (data) {
            canceledituser();
            loadingPopup.RemoveMessage(loaderHolder);
            $("#recordCount-Holder").html("ERROR!");
            $("#errorPullingRecords").html("There was a problem trying to get your records! Try changing the 'Records to Pull' to something smaller.");
        });
    }
    function OpenEvent(driver, date, unit, dir) {
        loadingPopup.Message("Loading Schedule...", loaderHolder);
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/LoadEvent", '{ "driver": "' + driver + '","date": "' + date + '","unit": "' + unit + '","dir": "' + dir + '" }', null, function (data) {
            var response = data.d[0];
            loadingPopup.RemoveMessage(loaderHolder);
            if (response != null) {
                $("#eventEdit-Driver").html(driver.replace("_", " "));
                $("#eventEdit-Date").html(date.split(" ")[0]);
                $("#eventEdit-Unit").html(unit);
                $("#eventEdit-Dir").html(dir);
                $("#EventList-steeltrucks").html(response);
                if (data.d[1]) {
                    $("#schedulenumber-holder_steeltrucks").show();
                    $("#schedulenumber-holder_steeltrucks").html(data.d[1]);
                }
                else {
                    $("#schedulenumber-holder_steeltrucks").hide();
                    $("#schedulenumber-holder_steeltrucks").html("");
                }

                if (!editMode) {
                    LoadEventEditor();
                }

                $("#btn-save-eventeditor").attr("onclick", "steelTruckFunctions.SaveDriverRows('" + driver + "', '" + date + "', '" + unit + "', '" + dir + "', true);");
                $("#printBtn-steeltrucks").attr("onclick", "steelTruckFunctions.SaveDriverRowsAndPrint('" + driver + "', '" + date + "', '" + unit + "', '" + dir + "');");

                var onClickEventStr = "steelTruckFunctions.SaveDriverRows('" + driver + "', '" + date + "', '" + unit + "', '" + dir + "');";
                $("#btn_exportToexcel_steeltrucks").attr("onclick", onClickEventStr + "steelTruckFunctions.ExportToExcel_steeltrucks();");
                $("#editBtn-steeltrucks").attr("onclick", onClickEventStr + "steelTruckFunctions.EditHeaderEvent();return false;");

                editMode = false;
                ReloadAutoCompleteAdd();
            }
        }, function (data) {
            canceledituser();
            loadingPopup.RemoveMessage(loaderHolder);
        });
    }

    // Create New Functions 
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
            loadingPopup.Message("Creating Schedule...", loaderHolder);
            openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/CallCreateNew", '{ "_driver": "' + driver + '","_date": "' + date + '","_dir": "' + dir + '","_unit": "' + unit + '" }', null, function (data) {
                loadingPopup.RemoveMessage(loaderHolder);
                OpenEvent(driver.replace(" ", "_"), date, unit, dir);
                LoadCreateNew("");
                LoadInit();
            }, function (data) {
                loadingPopup.RemoveMessage(loaderHolder);
                $("#createNewError").html("Error. Could not add new schedule.");
                setTimeout(function () {
                    $("#createNewError").html("");
                }, 3000);
            });
        }
    }

    // Driver Name Edit
    function deleteUser(_this) {
        openWSE.ConfirmWindow("Are you sure you want to delete this Driver and all of the schedules associated with this?",
            function () {
                var _driverName = _this.getAttribute("href").replace("#delete_", "");
                loadingPopup.Message("Deleting " + _driverName + "...", loaderHolder);
                openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/DriverEdit", '{ "_case": "' + "deleteUser" + '","_driver": "' + escape(_driverName) + '","_oldname": "' + "" + '","_newname": "' + "" + '" }', null, function (data) {
                    var response = data.d;
                    if (response != null) {
                        $("#driver-index-holder").html(response);
                        $("#CurrDriverName-Holder").html("");
                        loadingPopup.RemoveMessage(loaderHolder);
                        if (currentUser != "") {
                            if (currentUser != "expand_" + _driverName) {
                                ReCallDriver($(".tsdiv[data-id='" + currentUser + "']"));
                            }
                            else {
                                ViewAllRecords();
                            }
                        }
                        else {
                            ViewAllRecords();
                        }
                    }
                });
            }, null);
    }
    function editUser(_this, user) {
        canceledituser();
        var $this = $(_this).closest('div');
        $this.addClass("tsdiv-active-edit");
        $this.append("<input id='tb_edituserentry' maxlength='50' type='text' value='" + user.replace(/_/g, " ") + "' onkeypress='steelTruckFunctions.OnKeyPress_DriverEdit(event,\"" + user + "\")' class='textEntry float-left' style='width: 220px;' />");
        $this.append("<a id='driverEditBtns-cancel' href='#' onclick='steelTruckFunctions.canceledituser();return false;' class='td-cancel-btn'></a>");
        $this.append("<a id='driverEditBtns-update' href='#' onclick=\"steelTruckFunctions.updateUser('" + user + "');return false;\" class='td-update-btn'></a>");
        $this.append("<div id='driverEditBtns-clear' class='clear'></div>");
    }
    function updateUser(user) {
        var _driverName = document.getElementById('tb_edituserentry').value.replace(/ /g, "_");
        loadingPopup.Message("Updating " + _driverName + "...", loaderHolder);
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/DriverEdit", '{ "_case": "' + "updateUser" + '","_driver": "' + "" + '","_oldname": "' + escape(user) + '","_newname": "' + escape(_driverName) + '" }', null, function (data) {
            var response = data.d;
            if (response != null) {
                $("#driver-index-holder").html(response);
                loadingPopup.RemoveMessage(loaderHolder);
                if (currentUser != "") {
                    ReCallDriver($(".tsdiv[data-id='expand_" + _driverName + "']"));
                }
            }
        }, function (data) {
            canceledituser();
            loadingPopup.RemoveMessage(loaderHolder);
        });
    }
    function canceledituser() {
        var id = $(".tsdiv-active-edit").attr("data-id");
        $(".tsdiv[data-id='" + id + "']").removeClass("tsdiv-active-edit");
        $("#tb_edituserentry").remove();
        $("#driverEditBtns-cancel").remove();
        $("#driverEditBtns-update").remove();
        $("#driverEditBtns-clear").remove();
        if ($(".tsdiv[data-id='" + id + "']").hasClass("tsactive")) {
            $(".tsdiv[data-id='" + id + "']").find("img").show();
        }
        else {
            $(".tsdiv[data-id='" + id + "']").find("img").hide();
        }
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

    // Driver Edit Functions
    var customerEdit = "";
    var cityEdit = "";
    var orderEdit = "";
    var weightEdit = "";
    var tempActionBtns = "";
    var updateItems = {};
    function EditEvent(id) {
        customerEdit = $("#" + id + "-customer").text();
        cityEdit = $("#" + id + "-city").text();
        orderEdit = $("#" + id + "-order").text();
        weightEdit = $("#" + id + "-weight").text();
        tempActionBtns = $("#" + id + "-actions").html();

        var onKeyUp = "onkeyup='steelTruckFunctions.EditRecordKeyPress(event, \"" + id + "\")'";

        $("#" + id + "-seq").css("cursor", "move");
        $("#" + id + "-customer").append("<input type='text' id='eventcustomerEdit' value='" + customerEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
        $("#" + id + "-city").append("<input type='text' id='eventcityEdit' value='" + cityEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
        $("#" + id + "-order").append("<input type='text' id='eventorderEdit' value='" + orderEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");
        $("#" + id + "-weight").append("<input type='text' id='eventweightEdit' value='" + weightEdit + "' class='textEntry' " + onKeyUp + " style='width:95%' />");

        var updateButton = "<a href='#' class='td-update-btn margin-right' onclick='steelTruckFunctions.UpdateEditEvent(\"" + id + "\");return false;' title='Update'></a>";
        var cancelButton = "<a href='#' class='td-cancel-btn' onclick='steelTruckFunctions.CancelEditEvent(\"" + id + "\");return false;' title='Cancel'></a>";
        $("#" + id + "-actions").append(updateButton + cancelButton);


        $("#" + id + "-customer").parent().find(".ts-row-data").hide();
        $(".AddNewEventRow,.edit-button-event,.delete-button-event").hide();

        $("#eventcustomerEdit").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                if (listCustomersTSData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listCustomersTSData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListCustomersTS", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
        $("#eventcityEdit").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                if (listCityTSData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listCityTSData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListCityTS", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });

        $("#EventSort-steeltrucks > table tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper,
            containment: '#EventSort-steeltrucks',
            opacity: 0.9,
            tolerance: 'pointer'
        });
        $("#EventSort-steeltrucks > table tbody").sortable("option", "disabled", false);

        $("#eventcustomerEdit").focus();
    }
    function EditRecordKeyPress(event, id) {
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
    function UpdateEditEvent(id) {
        var dir = $("#eventEdit-Dir").text();
        var unit = $("#eventEdit-Unit").text();
        var date = $("#eventEdit-Date").text();
        var driver = $("#eventEdit-Driver").text();

        var customer = $("#eventcustomerEdit").val();
        var city = $("#eventcityEdit").val();
        var order = $("#eventorderEdit").val();
        var weight = numberWithCommas($("#eventweightEdit").val());

        var additionalInfo = $.trim($("#schedulenumber-holder_steeltrucks").find("h2").html());

        if (!updateItems["updateItems"]) {
            updateItems["updateItems"] = {};
        }

        updateItems["updateItems"][id] = {
            "driver": driver,
            "date": date,
            "unit": unit,
            "dir": dir,
            "customer": customer,
            "city": city,
            "order": order,
            "weight": weight,
            "additionalInfo": additionalInfo
        }
        updateItems["eventList"] = BuildEventListOrder();

        $("#" + id + "-customer").find(".ts-row-data").html(customer);
        $("#" + id + "-city").find(".ts-row-data").html(city);
        $("#" + id + "-order").find(".ts-row-data").html(order);
        $("#" + id + "-weight").find(".ts-row-data").html(weight);

        CancelEditEvent(id);
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

        $("#" + id + "-customer").parent().find(".ts-row-data").show();
        $("#" + id + "-customer").parent().find(".textEntry").remove();
        $("#" + id + "-actions").find(".td-update-btn").remove();
        $("#" + id + "-actions").find(".td-cancel-btn").remove();
        $(".AddNewEventRow,.edit-button-event,.delete-button-event").show();

        $("#EventSort-steeltrucks > table tbody").sortable("option", "disabled", true);
        UpdateWeightTotalInEditor();
    }
    function DeleteEvent(id) {
        openWSE.ConfirmWindow("Are you sure you want to delete this row?",
            function () {
                if (!updateItems["deleteItems"]) {
                    updateItems["deleteItems"] = "";
                }

                updateItems["deleteItems"] += id + ";";
                var eventList = BuildEventListOrder();
                updateItems["eventList"] = eventList.replace(id + ";", "");
                $("#" + id + "-actions").parent().remove();
                UpdateWeightTotalInEditor();
            }, null);
    }
    function LoadEventEditor() {
        if ($("#TruckEvent-element").css("display") != "block") {
            updateItems = {};
            openWSE.LoadModalWindow(true, "TruckEvent-element", "Truck Event Editor");
        }
        else {
            if (updateItems && Object.keys(updateItems).length) {
                openWSE.ConfirmWindowAltBtns("Changes have been made. Do you want to discard them?",
                    function () {
                        finishCloseEventEditor();
                    }, null);
            }
            else {
                finishCloseEventEditor();
            }
        }
    }
    function finishCloseEventEditor() {
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
                loadingPopup.Message("Reloading...", loaderHolder);
                LoadInit();
            }
            needScheduleReload = false;
        }, openWSE_Config.animationSpeed);
    }
    function ReloadAutoCompleteAdd() {
        $(".customername-tb-autosearch").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                if (listCustomersTSData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listCustomersTSData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListCustomersTS", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
        $(".city-tb-autosearch").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                if (listCityTSData.length > 0) {
                    var tempArr = SearchAutoCompleteList(listCityTSData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
                else {
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListCityTS", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    });
                }
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
    function SaveDriverRows(driver, date, unit, dir, finishClose) {
        loadingPopup.Message("Loading Schedule...", loaderHolder);
        var updateItemsStr = escape(JSON.stringify(updateItems));
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/SaveDriverRows", '{ "_driver": "' + driver + '","_date": "' + date + '","_unit": "' + unit + '","_dir": "' + dir + '","_rowData": "' + updateItemsStr + '" }', null, function (data) {
            loadingPopup.RemoveMessage(loaderHolder);
            updateItems = {};
            if (finishClose) {
                finishCloseEventEditor();
            }
            steelTruckFunctions.Refresh();
        });
    }
    function SaveDriverRowsAndPrint(driver, date, unit, dir) {
        loadingPopup.Message("Loading Schedule...", loaderHolder);
        var updateItemsStr = escape(JSON.stringify(updateItems));
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/SaveDriverRows", '{ "_driver": "' + driver + '","_date": "' + date + '","_unit": "' + unit + '","_dir": "' + dir + '","_rowData": "' + updateItemsStr + '" }', null, function (data) {
            loadingPopup.RemoveMessage(loaderHolder);
            updateItems = {};
            steelTruckFunctions.Refresh();

            setTimeout(function () {
                loadingPopup.RemoveMessage(loaderHolder);

                var url = "Apps/SteelTrucks/SchedulePrint.aspx?user=" + driver.replace(" ", "_") + "&date=" + date + "&unit=" + unit + "&gd=" + dir;
                $("#printBtn-steeltrucks--hidden").attr("href", url);
                $("#printBtn-steeltrucks--hidden").click();

                //var specs = "width=" + ($(window).width() - 50) + "px,height=" + ($(window).height() - 50) + "px,location=no,menubar=no,toolbar=no,status=no,resizable=yes,scrollbars=yes";
                //var myWindow = window.open(url, "Print Truck Schedule", specs);
                //if (myWindow) {
                //    myWindow.focus();
                //}
                //else {
                //    openWSE.AlertWindow("Make sure allow popups from this site.<br />Click the image below to see example.<br /><br /><a href='Apps/SteelTrucks/AllowPopupsAlert.png' target='_blank'><img alt='' src='Apps/SteelTrucks/AllowPopupsAlert.png' /></a>");
                //}
            }, 1);
        });
    }
    function printClickEvent(_this) {
        window.open(_this.href, '_blank');
    }

    // Add New Driver Row
    function AddRecordKeyPress_SteelTrucks(event, driver, date, unit, dir) {
        if (event.which == 13 || event.keyCode == 13) {
            AddEvent(driver, date, unit, dir);
            return false;
        }
    }
    function AddEvent(driver, date, unit, dir) {
        // Get textbox values
        var customer = $("#tbCustomerNew").val();
        var city = $("#tbCityNew").val();
        var order = $("#tbOrderNew").val();
        var weight = numberWithCommas($("#tbWeightNew").val());
        var additionalInfo = $.trim($("#schedulenumber-holder_steeltrucks").find("h2").html());
        var colId = GenerateGuid();
        var rowNum = $("#EventSort-steeltrucks").find(".EventRow").length + 1;

        if (!updateItems["addItems"]) {
            updateItems["addItems"] = {};
        }

        updateItems["addItems"][colId] = {
            "driver": driver,
            "date": date,
            "unit": unit,
            "dir": dir,
            "customer": customer,
            "city": city,
            "order": order,
            "weight": weight,
            "additionalInfo": additionalInfo
        }

        var editButton = "<a href='#' class='td-edit-btn margin-right edit-button-event' onclick='steelTruckFunctions.EditEvent(\"" + colId + "\");return false;' title='Edit'></a>";
        var deleteButton = "<a href='#' class='td-delete-btn delete-button-event' onclick='steelTruckFunctions.DeleteEvent(\"" + colId + "\");return false;' title='Delete'></a>";

        var htmlStr = "<tr class='GridNormalRow myItemStyle EventRow'>";
        htmlStr += "<td id='" + colId + "-seq' class='GridViewNumRow border-bottom' style='width: 35px;'>" + rowNum + "</td>";
        htmlStr += "<td id='" + colId + "-customer' class='border-bottom non-moveable'><span class='ts-row-data'>" + customer + "</span></td>";
        htmlStr += "<td id='" + colId + "-city' class='border-bottom non-moveable'><span class='ts-row-data'>" + city + "</span></td>";
        htmlStr += "<td id='" + colId + "-order' class='border-bottom non-moveable'><span class='ts-row-data'>" + order + "</span></td>";
        htmlStr += "<td id='" + colId + "-weight' class='border-bottom non-moveable'><span class='ts-row-data'>" + weight + "</span></td>";
        htmlStr += "<td id='" + colId + "-actions' class='border-bottom non-moveable myItemStyle-action-btns' align='center' style='width: 75px;'>" + editButton + deleteButton + "</td>";
        htmlStr += "</tr>";

        $("#EventSort-steeltrucks").find(".AddNewEventRow").before(htmlStr);

        $("#tbCustomerNew").val("");
        $("#tbCityNew").val("");
        $("#tbOrderNew").val("");
        $("#tbWeightNew").val("");

        setTimeout(function () {
            $("#EventSort-steeltrucks > table tbody").sortable();
            $("#EventSort-steeltrucks > table tbody").sortable("option", "disabled", true);
            UpdateWeightTotalInEditor();
        }, 1);
    }
    function GenerateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function UpdateWeightTotalInEditor() {
        var total = 0;
        $("#EventSort-steeltrucks").find("tr.EventRow").each(function () {
            var val = $.trim($(this).find("td").eq(4).find(".ts-row-data").html());
            if (val) {
                val = val.replace(/,/g, "");
                var valInt = parseInt(val);
                if (!isNaN(valInt)) {
                    total += valInt;
                }
            }
        });

        $("#span_WeightTotals_Editor").html(numberWithCommas(total));
    }
    function numberWithCommas(x) {
        try {
            if (!isNaN(x)) {
                try {
                    var nf = new Intl.NumberFormat();
                    return nf.format(x);
                }
                catch (evt) {
                    return x.toLocaleString();
                }
            }
            
            var tempX = x.replace(/,/g, "");
            var xInt = parseInt(tempX);
            if (!isNaN(xInt)) {
                try {
                    var nf = new Intl.NumberFormat();
                    return nf.format(xInt);
                }
                catch (evt) {
                    return xInt.toLocaleString();
                }
            }
        }
        catch (evt) { }
        return x;
    }

    // Schedule Edit Functions
    var driverVar = "";
    var dateVar = "";
    var unitVar = "";
    var dirVar = "";
    var editMode = false;
    function EditHeaderEvent() {
        if (editMode) {
            loadingPopup.Message("Updating Schedule...", loaderHolder);
            openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/EventHeaderEdit", "{ '_Orgdriver': '" + driverVar + "', '_Orgdate': '" + dateVar + "', '_Orgunit': '" + unitVar + "', '_Orgdir': '" + dirVar + "', '_date': '" + $("#eventEdit-Date-tb").val() + "', '_unit': '" + $("#eventEdit-Unit-tb").val() + "', '_dir': '" + $("#eventEdit-Dir-tb").val() + "' }", {
                dataFilter: function (data) { return data; }
            }, function (data) {
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

                    loadingPopup.RemoveMessage(loaderHolder);
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
                    openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetListOfSMWUnits", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                        dataFilter: function (data) { return data; }
                    }, function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
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
                loadingPopup.Message("Deleting Schedule...", loaderHolder);
                openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/EventHeaderDelete", '{ "_driver": "' + $("#eventEdit-Driver").html() + '","_date": "' + $("#eventEdit-Date").html() + '","_unit": "' + $("#eventEdit-Unit").html() + '","_dir": "' + $("#eventEdit-Dir").html() + '" }', null, function (data) {
                    loadingPopup.RemoveMessage(loaderHolder);
                    needScheduleReload = true;
                    LoadEventEditor();
                });
            }, null);
    }

    // General Direction Edit Functions
    function deletegd(id, dir) {
        openWSE.ConfirmWindow("Are you sure you want to delete this General Direction? (Any associated schedule will default to Kansas.)",
            function () {
                openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/DeleteDirection", "{ 'id': '" + id + "','gendir': '" + escape(dir) + "' }", null, function (data) {
                    if (!openWSE.ConvertBitToBoolean(data.d)) {
                        openWSE.AlertWindow("Error deleting direction. Please try again.");
                    } else {
                        $("#GenDirList-steeltrucks").html(data.d);
                        BuildGeneralDirections();
                    }
                }, function (data) {
                    openWSE.AlertWindow("There was an error deleting direction. Please try again");
                });
            }, null);
    }
    function addgd() {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/AddDirection", "{ 'gendir': '" + escape(document.getElementById("tb_addgendir").value) + "' }", null, function (data) {
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
        }, function (data) {
            openWSE.AlertWindow("There was an error adding direction. Please try again");
        });
    }
    function editgd(dir) {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/EditDirection", "{ 'gendir': '" + escape(dir) + "' }", null, function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        }, function (data) {
            openWSE.AlertWindow("There was an error editing direction. Please try again");
        });
    }
    function updategd(id) {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/UpdateDirection", "{ 'id':'" + id + "','gendir': '" + escape(document.getElementById("tb_editgendir").value) + "' }", null, function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            } else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#GenDirList-steeltrucks").html(data.d);
            }
        }, function (data) {
            openWSE.AlertWindow("There was an error updating direction. Please try again");
        });
    }
    function canceleditgd() {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/BuildEditor", "{ }", null, function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        }, function (data) {
            openWSE.AlertWindow("There was an error loading direction list. Please try again");
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

    var fixHelper = function (e, ui) {
        ui.children().each(function () {
            $(this).width($(this).width());
        });
        return ui;
    };

    return {
        LoadInit: LoadInit,
        Refresh: Refresh,
        LoadGenDirEditor: LoadGenDirEditor,
        ExportToExcel_steeltrucks: ExportToExcel_steeltrucks,
        ExportToExcelAll_steeltrucks: ExportToExcelAll_steeltrucks,
        RecordstoSelect: RecordstoSelect,
        RefreshScheduleList: RefreshScheduleList,
        CloseDriver: CloseDriver,
        ViewAllRecords: ViewAllRecords,
        SearchTruckSchedule: SearchTruckSchedule,
        KeyPressSearch: KeyPressSearch,
        OnSortClick: OnSortClick,
        LoadScheduleList: LoadScheduleList,
        LoadCreateNew: LoadCreateNew,
        CallCreateNew: CallCreateNew,
        OpenEvent: OpenEvent,
        deleteUser: deleteUser,
        editUser: editUser,
        updateUser: updateUser,
        canceledituser: canceledituser,
        OnKeyPress_DriverEdit: OnKeyPress_DriverEdit,
        UpdateEditEvent: UpdateEditEvent,
        CancelEditEvent: CancelEditEvent,
        EditEvent: EditEvent,
        DeleteEvent: DeleteEvent,
        AddEvent: AddEvent,
        AddRecordKeyPress_SteelTrucks: AddRecordKeyPress_SteelTrucks,
        CancelEditHeaderEvent: CancelEditHeaderEvent,
        EditHeaderEvent: EditHeaderEvent,
        DeleteHeaderEvent: DeleteHeaderEvent,
        LoadEventEditor: LoadEventEditor,
        addgd: addgd,
        editgd: editgd,
        deletegd: deletegd,
        canceleditgd: canceleditgd,
        updategd: updategd,
        OnKeyPress_DirEditNew: OnKeyPress_DirEditNew,
        EditRecordKeyPress: EditRecordKeyPress,
        SaveDriverRows: SaveDriverRows,
        SaveDriverRowsAndPrint: SaveDriverRowsAndPrint,
        printClickEvent: printClickEvent
    }
}();

$(document).ready(function () {
    $("#tb_exportDateFrom").val(new Date().toLocaleDateString());
    $("#tb_exportDateTo").val(new Date().toLocaleDateString());
    steelTruckFunctions.LoadInit();
    steelTruckFunctions.LoadScheduleList("");
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
    var a = $("#driver-index-holder").html();
    var b = $("#driver-sch-holder").html();
    if (($.trim(a) == "") || ($.trim(b) == "")) {
        steelTruckFunctions.LoadInit();
    }
});
