/* Custom Table Functions
----------------------------------*/
function pageLoad() {
    customTables.TablePageLoad();
}

var resizeId_CustomTables;
$(window).resize(function () {
    clearTimeout(resizeId_CustomTables);
    resizeId_CustomTables = setTimeout(function () {
        customTables.ResizeChartData();
    }, 500);
});

var customTables = function () {

    // Private Variables
    var arrSortColCustomTables = new Array();
    var arrEditModeCustomTables = new Array();
    var arrAddValuesCustomTables = new Array();
    var arrDateSelectedCustomTables = new Array();
    var timerIsRunning = false;
    var timerIntveral = 1000 * 60;
    var timerCustomTables;


    // Load Functions
    function TablePageLoad() {
        $(".custom-table-timermarker").each(function () {
            var id = $(this).find(".custom-table-tableView-holder").attr("id").replace("pnl_", "").replace("_tableView", "");
            var innerHolder = $.trim($("#data-holder-" + id).html());
            if (innerHolder == "") {
                Load(id);
                $("#data-holder-" + id).html("<h3 class='pad-all'>Loading...</h3>");
            }
        });
    }
    function Load(id) {
        InitJquery(id);
        $(document.body).on("click", ".app-main-holder[data-appid='app-" + id + "'] .exit-button-app, .app-min-bar[data-appid='app-" + id + "'] .exit-button-app-min", function () {
            cookie.del(id + "-fontsize");
            cookie.del(id + "-customtable-viewmode");
        });

        var viewMode = cookie.get(id + "-customtable-viewmode");
        if (viewMode != null && viewMode != undefined && viewMode == "mobile") {
            $("#pnl_" + id + "_tableView").addClass("mobile-view");
        }

        if ($.trim($("#data-holder-" + id).html()) == "") {
            GetRecords(id, false);
        }

        $("#pnl_" + id + "_tableView").scroll(function () {
            var titleHeight = $("#pnl_" + id + "_tableView").find(".app-title-bg-color").outerHeight();
            if ($(this).scrollTop() > titleHeight) {
                $("#pnl_" + id + "_tableView").find(".app-menu-btn").css("top", $(this).scrollTop());
            }
            else {
                $("#pnl_" + id + "_tableView").find(".app-menu-btn").css("top", "");
            }
        });
    }
    function Timer() {
        timerIsRunning = true;
        timerCustomTables = setTimeout(function () {
            $(".custom-table-timermarker").each(function (index) {
                var id = $(this).find(".custom-table-tableView-holder").attr("id").replace("pnl_", "").replace("_tableView", "");
                if (id != "") {
                    GetRecords(id, true);
                }
            });
        }, timerIntveral);
    }
    $(document.body).on("click", ".custom-table-timermarker .custom-table-tableView-holder", function (e) {
        if (e.target.className.indexOf("img-appmenu") == -1) {
            var id = $(this).attr("id").replace("pnl_", "").replace("_tableView", "");
            var $menu = $("#" + id + "-sidebar-menu");
            if ($menu.length > 0) {
                if ($menu.hasClass("showmenu")) {
                    MenuClick(id);
                }
            }
        }
    });


    // View Mode Functions
    function SetupViewModeForTable(id) {
        if (IsViewMobileMode(id)) {
            $("#" + id + "-sidebar-menu-viewallrecords").show();
            $("#search_" + id + "_holder").removeClass("float-right");
            $("#search_" + id + "_holder").find(".searchwrapper").css("width", "100%");
            $("#search_" + id + "_divider").show();
            if (!$("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                $("#pnl_" + id + "_tableView").addClass("mobile-view");
            }

            $("#viewmode-selector-" + id).val("mobile");

            if ($.trim($("#" + id + "-complete-table").find(".custom-table-add-mobile").html()) != "") {
                var viewMode = $("#pnl_" + id + "_tableView").attr("data-viewmode");
                if (viewMode == "" || viewMode == null || viewMode == "mobile-add") {
                    $("#btn_" + id + "_addRecord").hide();
                    $("#btn_" + id + "_viewRecords").show();
                    $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-add-mobile").show();
                }
                else {
                    $("#btn_" + id + "_viewRecords").hide();
                    $("#btn_" + id + "_addRecord").show();
                    $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();
                }
            }
            else {
                $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-view")
                $("#btn_" + id + "_viewRecords").hide();
                $("#btn_" + id + "_addRecord").hide();
                $("#" + id + "-sidebar-menu-viewallrecords").hide();
                $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
                $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();
            }
        }
        else {
            $("#" + id + "-sidebar-menu-viewallrecords").hide();
            $("#search_" + id + "_holder").addClass("float-right");
            $("#search_" + id + "_holder").find(".searchwrapper").css("width", "300px");
            $("#search_" + id + "_divider").hide();
            if ($("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                $("#pnl_" + id + "_tableView").removeClass("mobile-view");
            }

            $("#viewmode-selector-" + id).val("desktop");
        }
    }
    function IsViewMobileMode(id) {
        var viewMode = cookie.get(id + "-customtable-viewmode");
        if (viewMode != null && viewMode != undefined && viewMode == "desktop") {
            return false;
        }
        else if (navigator.userAgent.match(/Android/i) || navigator.userAgent.match(/webOS/i) || navigator.userAgent.match(/iPhone/i) || navigator.userAgent.match(/iPad/i) || navigator.userAgent.match(/iPod/i) || navigator.userAgent.match(/BlackBerry/i) || navigator.userAgent.match(/Windows Phone/i) || $("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
            return true;
        }

        return false;
    }
    function ChangeViewMode(id) {
        var selectedMode = $("#viewmode-selector-" + id).val();
        if (selectedMode == "mobile") {
            $("#pnl_" + id + "_tableView").addClass("mobile-view");
            cookie.set(id + "-customtable-viewmode", "mobile", "30");
        }
        else {
            $("#pnl_" + id + "_tableView").removeClass("mobile-view");
            cookie.set(id + "-customtable-viewmode", "desktop", "30");
        }

        GetRecords(id, false);
        MenuClick(id);
    }


    // Initialize Jquery Functions
    function InitJquery(id) {
        openWSE.GetScriptFunction("//www.google.com/jsapi", function () {
            google.load("visualization", "1", { callback: function () { }, packages: ["corechart"] });
            google.load("visualization", "1", { callback: function () { }, packages: ["gauge"] });
        });

        $("#tb_exportDateFrom_" + id).datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#tb_exportDateTo_" + id).datepicker("option", "minDate", selectedDate);
            }
        });
        $("#tb_exportDateTo_" + id).datepicker({
            defaultDate: "+1w",
            changeMonth: true,
            numberOfMonths: 2,
            onClose: function (selectedDate) {
                $("#tb_exportDateFrom_" + id).datepicker("option", "maxDate", selectedDate);
            }
        });

        $("#tb_exportDateFrom_" + id).val(new Date().toLocaleDateString());
        $("#tb_exportDateTo_" + id).val(new Date().toLocaleDateString());

        if ($(".app-main-external").length == 0) {
            $(document.body).on("click", ".app-main-holder[data-appid='app-" + id + "'] .maximize-button-app", function () {
                ReBuildChartData(id);
            });

            $(document.body).on("dblclick", ".app-main-holder[data-appid='app-" + id + "'] .app-head-dblclick", function () {
                ReBuildChartData(id);
            });
        }
    }
    function AutoCompleteAddEdit(id, editMode) {
        var rowType = "add";
        if (editMode) {
            rowType = "edit";
        }
        $("#" + id + "-" + rowType + "Row").find(".data-td").each(function (index) {
            var $input = $(this).find("input");
            var columnName = $(this).attr("data-columnname");
            if (editMode) {
                columnName = $(this).find(".td-columnName-" + rowType).html();
            }
            var columnType = $input.attr("data-type");

            if ($.trim(columnType) == "datetime" || $.trim(columnType) == "date") {
                $input.datepicker({
                    changeMonth: true,
                    changeYear: true,
                    constrainInput: false
                });
            }
            else if ($.trim(columnType) == "boolean") {
                $input.css("width", "auto");
                $input.attr("type", "checkbox");
                $input.attr("onkeyup", "");
                if (openWSE.ConvertBitToBoolean($input.val())) {
                    $input.prop("checked", true);
                }
            }
            else {
                $input.autocomplete({
                    minLength: 0,
                    autoFocus: false,
                    source: function (request, response) {
                        $.ajax({
                            url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetCustomTableData",
                            data: "{ 'prefixText': '" + request.term + "', 'count': '10', 'id': '" + id + "', 'columnName': '" + columnName + "' }",
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
        });
    }


    // Get Functions
    function GetRecords(id, fromTimer) {
        if (!fromTimer) {
            openWSE.LoadingMessage1("Loading...");
        }

        var arr = GetSortDirCol(id);
        SaveInputAddValues(id, false);

        var dateSelected = "";
        if ($("#sidebar-items-" + id).length > 0) {
            dateSelected = GetDateSelected(id);
            GetSidebar(id, dateSelected);
        }

        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/GetRecords",
            data: "{'id': '" + id + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "','date': '" + dateSelected + "'}",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    if (data.d[0].length > 0) {
                        var innerHeader = data.d[0][0];
                        var innerAddRow = data.d[0][1];
                        var innerItemRow = data.d[0][2];
                        var allowEdit = data.d[1] == "true";

                        if ($.trim($("#cblist_" + id + "_holder").html()) == "") {
                            $("#cblist_" + id + "_holder").html(BuildColumnCheckList(id, innerHeader));
                        }

                        $("#data-holder-" + id).html(BuildTable(id, innerHeader, innerAddRow, innerItemRow, false, allowEdit));

                        LoadFontSizes(id);
                        AutoCompleteAddEdit(id, false);
                        RestoreInputAddValues(id, false);
                        RefreshColumnsToShow(id);
                    }
                    else {
                        $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
                    }

                    SetupViewModeForTable(id);
                    openWSE.RemoveUpdateModal();

                    if (!timerIsRunning) {
                        Timer();
                    }

                    setTimeout(function () {
                        var chartOn = data.d[2] == "true";
                        if (chartOn) {
                            $("#pnl_" + id + "_tableView").hide();
                            $("#pnl_" + id + "_chartView").show();
                        }

                        ReBuildChartData(id);
                    }, 250);

                    LoadCustomizations(id);
                }
                else {
                    $("#data-holder-" + id).html("<h4>No data available</h4>");
                    openWSE.RemoveUpdateModal();
                }
            },
            error: function (data) {
                $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
                openWSE.RemoveUpdateModal();
            }
        });
    }
    function GetSidebar(id, dateSelected) {
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/GetSidebar",
            data: "{ 'id': '" + id + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    var x = "";

                    var selectedClass = "";
                    if (dateSelected == "") {
                        selectedClass = " tsactive";
                    }
                    x += "<div id=\"expand_" + id + "_All\" class=\"tsdiv" + selectedClass + "\" onclick=\"customTables.MonthSelect('" + id + "', 'all')\"><div class=\"pad-all-sml\"><h4>All Dates</h4></div></div>";
                    x += "<div class=\"clear-space-two\"></div>";
                    for (var i = 0; i < data.d.length; i++) {
                        selectedClass = "";
                        if (dateSelected == data.d[i][0]) {
                            selectedClass = " tsactive";
                        }
                        x += "<div id=\"expand_" + id + "_" + data.d[i][0] + "\" class=\"tsdiv" + selectedClass + "\" onclick=\"customTables.MonthSelect('" + id + "', '" + data.d[i][0] + "')\"><div class=\"pad-all-sml\">" + data.d[i][1] + "</div></div>";
                        x += "<div class=\"clear-space-two\"></div>";
                    }

                    $("#month-selector-" + id).html(x);

                    if ($("#" + id + "-load").find(".tsactive").length == 0) {
                        $("#expand_" + id + "_All").addClass("tsactive");
                    }
                }
                else {
                    $("#month-selector-" + id).html("<h4>No Dates Available</h4>");
                }

                openWSE.RemoveUpdateModal();
            }
        });
    }


    // Sidebar Menu Functions
    function AddRecord_MenuClick(id) {
        $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-add");
        $("#btn_" + id + "_addRecord").hide();
        $("#btn_" + id + "_viewRecords").show();
        $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-add-mobile").show();

        $("#pnl_" + id + "_chartView").hide();
        $("#pnl_" + id + "_tableView").show();

        MenuClick(id);
    }
    function ViewRecords_MenuClick(id) {
        $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-view");
        $("#btn_" + id + "_viewRecords").hide();
        $("#btn_" + id + "_addRecord").show();
        $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();

        $("#pnl_" + id + "_chartView").hide();
        $("#pnl_" + id + "_tableView").show();

        MenuClick(id);
    }
    function MenuClick(id) {
        var $menu = $("#" + id + "-sidebar-menu");
        if ($menu.length > 0) {
            if (!$menu.hasClass("showmenu")) {
                $menu.show();
                $menu.addClass("showmenu");
                $menu.find(".customtable-sidebar-innercontent").show();
                $menu.css("width", $("#" + id + "-load").outerWidth() - 50);
            }
            else {
                $menu.removeClass("showmenu");
                $menu.find(".customtable-sidebar-innercontent").hide();
                $menu.css("width", "0px");
                $menu.hide();
            }
        }
    }
    function MonthSelect(id, _date) {
        if (_date == "all") {
            _date = "";
        }
        SetDateSelected(id, _date);
        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function Refresh(id) {
        openWSE.LoadingMessage1("Loading...");

        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function RecordstoSelect(id) {
        openWSE.LoadingMessage1("Loading...");
        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function ExportToExcelAll(id) {
        $("#btnExport-" + id).hide();
        $("#exportingNow_" + id).show();
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/ExportToExcel",
            data: "{ 'id': '" + id + "','startDate': '" + $("#tb_exportDateFrom_" + id).val() + "','endDate': '" + $("#tb_exportDateTo_" + id).val() + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d == "File path wrong.") {
                    $("#btnExport-" + id).show();
                    $("#exportingNow_" + id).hide();
                    openWSE.AlertWindow("Error saving file. Check file path under System Settings.");
                }
                else if (data.d != "") {
                    $("#btnExport-" + id).show();
                    $("#exportingNow_" + id).hide();
                    $.fileDownload(data.d);
                }

                else {
                    $("#btnExport-" + id).show();
                    $("#exportingNow_" + id).hide();
                    openWSE.AlertWindow("No records to export");
                }
            }
        });
    }
    function GetDateSelected(id) {
        var dateSelected = "";
        for (var i = 0; i < arrDateSelectedCustomTables.length; i++) {
            if (arrDateSelectedCustomTables[i][0] == id) {
                dateSelected = arrDateSelectedCustomTables[i][1];
            }
        }

        return dateSelected;
    }
    function SetDateSelected(id, selected) {
        var found = false;
        for (var i = 0; i < arrDateSelectedCustomTables.length; i++) {
            if (arrDateSelectedCustomTables[i][0] == id) {
                arrDateSelectedCustomTables[i][1] = selected;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = selected;

            arrDateSelectedCustomTables[arrDateSelectedCustomTables.length] = arrInner;
        }
    }


    // Search Functions
    function Search(id) {
        if (IsViewMobileMode(id)) {
            $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-view");
        }
        SetEditMode(id, false, "");
        GetRecords(id, false);
    }
    function KeyPressSearch(event, id) {
        try {
            if (event.which == 13) {
                Search(id);
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                Search(id);
                return false;
            }
            delete evt;
        }
    }


    // Column Selector Functions
    function BuildColumnCheckList(id, innerHeader) {
        var cbList = "";
        var totalShown = 0;
        if (innerHeader.length > 0) {
            $("#li_cblist_" + id).show();
            for (var i = 0; i < innerHeader.length; i++) {
                var columnName = innerHeader[i][1];
                var cbInput = "<input type=\"checkbox\" id=\"cb_" + id + "_" + columnName + "\" value=\"" + columnName + "\" checked=\"checked\" onchange=\"customTables.UpdateColumnsToShow('" + id + "', this, true);\" />";
                cbInput += "<label for=\"cb_" + id + "_" + columnName + "\">&nbsp;" + columnName + "</label>";
                cbInput += "<div class='clear-space-five'></div>";
                if (innerHeader[i][3] == "false") {
                    cbList += "<div style='display: none;'>" + cbInput + "</div>";
                }
                else {
                    cbList += cbInput;
                    totalShown++;
                }
            }
        }

        if (cbList == "" || totalShown == 0) {
            $("#li_cblist_" + id).hide();
        }

        return cbList;
    }
    function UpdateColumnsToShow(id, _this, checkTotal) {
        var val = $(_this).val();
        if ($(_this).is(":checked")) {
            if (!IsViewMobileMode(id)) {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").show();
            }
            else {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").parent().show();
            }
        }
        else {
            if (!IsViewMobileMode(id)) {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").hide();
            }
            else {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").parent().hide();
            }
        }

        if (checkTotal) {
            CheckTotalColumnsChecked(id);
        }
    }
    function CheckTotalColumnsChecked(id) {
        var totalChecked = 0;
        $("#cblist_" + id + "_holder").find("input[type='checkbox']").each(function () {
            $(this).prop("disabled", false);
            if ($(this).is(":checked")) {
                totalChecked++;
            }
        });

        if (totalChecked <= 1) {
            $("#cblist_" + id + "_holder").find("input[type='checkbox']").each(function () {
                if ($(this).is(":checked")) {
                    $(this).prop("disabled", true);
                }
            });
        }
    }
    function RefreshColumnsToShow(id) {
        $("#cblist_" + id + "_holder").find("input[type='checkbox']").each(function () {
            UpdateColumnsToShow(id, this, false);
        });

        CheckTotalColumnsChecked(id);
    }


    // Build Table for Mobile/Desktop
    function BuildTable(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        if (IsViewMobileMode(id)) {
            return BuildMobileView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
        }
        else {
            return BuildDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
        }
    }
    function BuildMobileView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        var addDiv = "";
        if (allowEdit && !editMode) {
            addDiv = "<table id='" + id + "-addRow'>";
            for (var i = 0; i < innerAddRow.length; i++) {
                addDiv += "<tr>";
                addDiv += "<td class='headerName'>" + innerAddRow[i][5].replace(/\//g, " / ") + "</td>";
                addDiv += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class='data-td addRow'>";

                var inputType = "text";
                if (innerAddRow[i][3] == "integer" || innerAddRow[i][3] == "decimal" || innerAddRow[i][3] == "money") {
                    inputType = "number";
                }

                var nullableEle = innerAddRow[i][4];
                if (innerAddRow[i][3] == "boolean") {
                    nullableEle = "";
                }

                addDiv += "<input type='" + inputType + "' class='textEntry-noWidth' maxlength='" + innerAddRow[i][1] + "' onkeyup=\"customTables.AddRecordKeyPress(event, '" + innerAddRow[i][2] + "');\" data-type='" + innerAddRow[i][3] + "' style='width: 92%;' />" + nullableEle;
                addDiv += "</td></tr>";
            }
            addDiv += "</table><div class='addBtn'><input type='button' class='input-buttons-create' value='Add Entry' onclick=\"customTables.AddRecord('" + id + "');return false;\" /></div>";
        }

        var itemDiv = "";
        if (innerItemRow.length > 0) {
            for (var i = 0; i < innerItemRow.length; i++) {
                if (!editMode) {
                    itemDiv += "<div class=\"item-row\">";
                }
                else {
                    if (openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                        itemDiv += "<div id=\"" + id + "-editRow\" class=\"item-row\">";
                    }
                    else {
                        itemDiv += "<div class=\"item-row\">";
                    }
                }

                itemDiv += "<table>";
                for (var j = 0; j < innerItemRow[i][1].length; j++) {
                    var colName = $.trim(innerHeader[j][4]);
                    colName = colName.replace(/\//g, " / ");

                    itemDiv += "<tr>";
                    var innerVal = innerItemRow[i][1][j][0].toString();

                    if (innerItemRow[i][1][j][1].toString() == "date" && !openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                        var tempDate = new Date(innerVal);
                        innerVal = tempDate.toLocaleDateString();
                    }
                    else if (innerItemRow[i][1][j][1].toString() == "money" && !openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                        innerVal = "$" + innerVal.formatMoney(2);
                    }

                    if (innerItemRow[i][1][j][1].toString() == "boolean") {
                        var checked = "";
                        if (openWSE.ConvertBitToBoolean(innerVal)) {
                            checked = " checked='checked'";
                        }
                        innerVal = "<input type='checkbox' value='" + innerVal + "'" + checked + " disabled='disabled' />";
                    }


                    itemDiv += "<td data-columnname=\"" + innerHeader[j][1] + "\" class=\"headerName td-sort-click" + innerHeader[j][0] + "\" onclick=\"customTables.OnSortClick(this,'" + innerHeader[j][1] + "','" + innerHeader[j][2] + "');\" title=\"Sort by " + colName + "\">" + colName + "</td>";
                    itemDiv += "<td class=\"data-td dataItem\" data-type=\"" + innerItemRow[i][1][j][1].toString() + "\">" + innerVal + "</td>";
                    itemDiv += "</tr>";
                }
                itemDiv += "</table>";

                if (allowEdit) {
                    var entryID = innerItemRow[i][0].toString();
                    var editBtn = "";
                    var delBtn = "";
                    if (!editMode) {
                        editBtn = "<input type=\"button\" class=\"input-buttons\" onclick=\"customTables.EditRecord('" + id + "', '" + entryID + "');return false;\" value=\"Edit\" />";
                        delBtn = "<input type=\"button\" class=\"input-buttons no-margin\" onclick=\"customTables.DeleteRecord('" + id + "', '" + entryID + "');return false;\" value=\"Delete\" />";
                    }
                    else {
                        if (openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                            editBtn = "<input type=\"button\" class=\"input-buttons\" onclick=\"customTables.UpdateRecord('" + id + "', '" + entryID + "');return false;\" value=\"Update\" />";
                            delBtn = "<input type=\"button\" class=\"input-buttons no-margin\" onclick=\"customTables.CancelRecord('" + id + "');return false;\" value=\"Cancel\" />";
                        }
                    }
                    itemDiv += "<div class=\"edit-buttons\" align=\"center\">" + editBtn + delBtn + "</div>";
                }
                itemDiv += "</div>";
            }
        }
        else {
            itemDiv = "<div class='emptyGridView'>No data found</div>";
        }

        if (addDiv == "" && itemDiv == "") {
            return "<div id=\"" + id + "-complete-table\"><div class='emptyGridView'>No data found</div></div>";
        }
        else {
            return "<div id=\"" + id + "-complete-table\"><div class='custom-table-add-mobile'>" + addDiv + "</div><div class='custom-table-data-mobile'>" + itemDiv + "</div></div>";
        }
    }
    function BuildDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        var xHeader = "<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">";
        xHeader += "<tbody><tr class=\"myHeaderStyle\">";
        xHeader += "<td style=\"width: 40px;\"></td>";
        for (var i = 0; i < innerHeader.length; i++) {
            var colName = $.trim(innerHeader[i][4]);
            colName = colName.replace(/\//g, " / ");
            xHeader += "<td data-columnname=\"" + innerHeader[i][1] + "\" class=\"td-sort-click" + innerHeader[i][0] + "\" onclick=\"customTables.OnSortClick(this,'" + innerHeader[i][1] + "','" + innerHeader[i][2] + "');\" title=\"Sort by " + colName + "\">" + colName + "</td>";
        }

        if (allowEdit) {
            xHeader += "<td class=\"edit-buttons\" style=\"width: 70px;\"></td></tr>";
        }

        var xAddRow = "";
        if (allowEdit) {
            xAddRow = "<tr id=\"" + id + "-addRow\" class=\"GridNormalRow myItemStyle\">";
            xAddRow += "<td class=\"GridViewNumRow border-bottom\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
            if (!editMode) {
                for (var i = 0; i < innerAddRow.length; i++) {
                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"data-td border-right border-bottom\" align=\"center\">";

                    var inputType = "text";
                    if (innerAddRow[i][3] == "integer" || innerAddRow[i][3] == "decimal" || innerAddRow[i][3] == "money") {
                        inputType = "number";
                    }

                    var nullableEle = innerAddRow[i][4];
                    if (innerAddRow[i][3] == "boolean") {
                        nullableEle = "";
                    }

                    xAddRow += "<input type='" + inputType + "' class='textEntry-noWidth' maxlength='" + innerAddRow[i][1] + "' onkeyup=\"customTables.AddRecordKeyPress(event, '" + innerAddRow[i][2] + "');\" data-type='" + innerAddRow[i][3] + "' style='width: 85%;' />" + nullableEle + "</td>";
                }
                xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'><a href=\"#Add\" class=\"td-add-btn\" onclick=\"customTables.AddRecord('" + id + "');return false;\" title=\"Add Row\"></a></td>";
            }
            else {
                for (var i = 0; i < innerAddRow.length; i++) {
                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"border-right border-bottom\"></td>";
                }
                xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px; height: 27px;'></td>";
            }

            xAddRow += "</tr>";
        }

        var xItem = "";
        for (var i = 0; i < innerItemRow.length; i++) {
            if (!editMode) {
                xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
            }
            else {
                if (openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                    xItem += "<tr id=\"" + id + "-editRow\" class=\"GridNormalRow myItemStyle item-row\">";
                }
                else {
                    xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
                }
            }
            xItem += "<td class=\"GridViewNumRow border-bottom\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
            for (var j = 0; j < innerItemRow[i][1].length; j++) {
                var innerVal = innerItemRow[i][1][j][0].toString();
                if (innerItemRow[i][1][j][1].toString() == "date" && !openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                    var tempDate = new Date(innerVal);
                    innerVal = tempDate.toLocaleDateString();
                }
                else if (innerItemRow[i][1][j][1].toString() == "money" && !openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                    innerVal = "$" + innerVal.formatMoney(2);
                }

                var tdAlign = "";
                if (innerItemRow[i][1][j][1].toString() == "boolean") {
                    var checked = "";
                    if (openWSE.ConvertBitToBoolean(innerVal)) {
                        checked = " checked='checked'";
                    }
                    innerVal = "<input type='checkbox' value='" + innerVal + "'" + checked + " disabled='disabled' />";
                    tdAlign = " align='center'";
                }

                xItem += "<td" + tdAlign + " data-columnname=\"" + innerHeader[j][1] + "\" class=\"data-td border-right border-bottom\" data-type=\"" + innerItemRow[i][1][j][1].toString() + "\">" + innerVal + "</td>";
            }

            if (allowEdit) {
                var entryID = innerItemRow[i][0].toString();
                var editBtn = "";
                var delBtn = "";
                if (!editMode) {
                    editBtn = "<a href=\"#Edit\" class=\"margin-right td-edit-btn\" onclick=\"customTables.EditRecord('" + id + "', '" + entryID + "');return false;\" title=\"Edit Row\"></a>";
                    delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"customTables.DeleteRecord('" + id + "', '" + entryID + "');return false;\" title=\"Delete Row\"></a>";
                }
                else {
                    if (openWSE.ConvertBitToBoolean(innerItemRow[i][2])) {
                        editBtn = "<a href=\"#Update\" class=\"margin-right td-update-btn\" onclick=\"customTables.UpdateRecord('" + id + "', '" + entryID + "');return false;\" title=\"Update Row\"></a>";
                        delBtn = "<a href=\"#Cancel\" class=\"td-cancel-btn\" onclick=\"customTables.CancelRecord('" + id + "');return false;\" title=\"Cancel Edit\"></a>";
                    }
                }
                xItem += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'>" + editBtn + delBtn + "</td>";
            }
            xItem += "</tr>";
        }

        if (xAddRow == "" && xItem == "") {
            return "<div id=\"" + id + "-complete-table\">" + xHeader + "</tbody></table><div class='emptyGridView'>No data found</div></div>";
        }
        else {
            return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + xItem + "</tbody></table></div>";
        }
    }


    // Add, Edit, Update, Cancel, Delete Functions
    $(document.body).on("keypress", ".textEntry-noWidth[data-type='integer']", function (e) {
        var code = (e.which) ? e.which : e.keyCode;
        var val = String.fromCharCode(code);

        if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9" && val != "-" && val != ".") {
            return false;
        }
    });
    $(document.body).on("keypress", ".textEntry-noWidth[data-type='money']", function (e) {
        var code = (e.which) ? e.which : e.keyCode;
        var val = String.fromCharCode(code);

        if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9" && val != "-" && val != "." && val != "," && val != "$") {
            return false;
        }
    });

    function AddRecord(id) {
        var recordVals = new Array();
        var error = false;
        $("#" + id + "-addRow").find(".data-td").each(function (index) {
            if (!error) {
                var $input = $(this).find("input");
                if ($input.attr("type") == "checkbox") {
                    if ($input.length > 0) {
                        var recordCols = new Array();
                        recordCols[0] = $(this).attr("data-columnname");

                        var isChecked = "0";
                        if ($input.prop("checked")) {
                            isChecked = "1";
                        }

                        recordCols[1] = $.trim(isChecked)
                        recordVals[index] = recordCols;
                    }
                }
                else if (($(this).find(".not-nullable").length > 0) && ($.trim($input.val()) == "")) {
                    error = true;
                }
                else {
                    if ($input.length > 0) {
                        var recordCols = new Array();
                        recordCols[0] = $.trim($(this).attr("data-columnname"));
                        recordCols[1] = $.trim($input.val());
                        recordVals[index] = recordCols;
                    }
                }
            }
        });

        if (!error) {
            openWSE.LoadingMessage1("Adding Record. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/AddRecord",
                data: JSON.stringify({ "id": id, "recordVals": recordVals }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    openWSE.RemoveUpdateModal();
                    if (data.d[0] == "Success") {
                        $("#" + id + "-addRow").find(".data-td").each(function (index) {
                            $(this).find("input").val("");
                        });

                        SaveInputAddValues(id, false);
                        GetRecords(id, false);
                        setTimeout(function () {
                            if ($("#" + id + "-addRow").find(".textEntry-noWidth").length > 0) {
                                $("#" + id + "-addRow").find(".textEntry-noWidth").eq(0).focus();
                            }
                        }, 250);
                    }
                    else {
                        openWSE.AlertWindow(data.d[1]);
                    }
                }
            });
        }
        else {
            openWSE.AlertWindow("One or more records cannot be empty. Please check your values and try again.");
        }
    }
    function AddRecordKeyPress(event, id) {
        try {
            if (event.which == 13) {
                AddRecord(id);
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                AddRecord(id);
                return false;
            }
            delete evt;
        }
    }

    function EditRecord(id, cid) {
        openWSE.LoadingMessage1("Loading...");
        var arr = GetSortDirCol(id);

        var dateSelected = "";
        if ($("#sidebar-items-" + id).length > 0) {
            dateSelected = GetDateSelected(id);
            GetSidebar(id, dateSelected);
        }

        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/EditRecord",
            data: "{'id': '" + id + "','cid': '" + cid + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "','date': '" + dateSelected + "'}",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    var innerHeader = data.d[0];
                    var innerAddRow = data.d[1];
                    var innerItemRow = data.d[2];

                    $("#data-holder-" + id).html(BuildTable(id, innerHeader, innerAddRow, innerItemRow, true, true));
                    LoadFontSizes(id);
                    SetEditMode(id, true, cid);

                    AutoCompleteAddEdit(id, true);
                    SetFocus(id, true);

                    timerIsRunning = false;
                    clearTimeout(timerCustomTables);
                    RefreshColumnsToShow(id);

                    if ($("#data-holder-" + id).find(".td-columnValue-edit").length > 0) {
                        $("#data-holder-" + id).find(".td-columnValue-edit").each(function () {
                            var val = $.trim($(this).html());
                            var $parentEle = $(this).parent();

                            if ($parentEle.find("input[type='text']").length > 0) {
                                $parentEle.find("input[type='text']").val(val);
                            }
                            else if ($parentEle.find("input[type='number']").length > 0) {
                                $parentEle.find("input[type='number']").val(val);
                            }
                            else if ($parentEle.find("input[type='checkbox']").length > 0) {
                                $parentEle.find("input[type='checkbox']").prop("checked", openWSE.ConvertBitToBoolean(val));
                            }
                            else {
                                $parentEle.find("input").val(val);
                            }
                        });
                    }
                }
                else {
                    openWSE.AlertWindow("Error pulling records.");
                }

                SetupViewModeForTable(id);
                if (IsViewMobileMode(id)) {
                    $("#btn_" + id + "_addRecord").hide();
                }

                LoadCustomizations(id);
                openWSE.RemoveUpdateModal();
            }
        });
    }
    function GetEditMode(id) {
        var arr = new Array();
        arr[0] = false
        for (var i = 0; i < arrEditModeCustomTables.length; i++) {
            if (arrEditModeCustomTables[i][0] == id) {
                if (arrEditModeCustomTables[i][1] == true) {
                    arr[0] = true;
                    arr[1] = arrEditModeCustomTables[i][2];
                }
            }
        }

        return arr;
    }
    function SetEditMode(id, x, cid) {
        var found = false;
        for (var i = 0; i < arrEditModeCustomTables.length; i++) {
            if (arrEditModeCustomTables[i][0] == id) {
                arrEditModeCustomTables[i][1] = x;
                arrEditModeCustomTables[i][2] = cid;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = x;
            arrInner[2] = cid;

            arrEditModeCustomTables[arrEditModeCustomTables.length] = arrInner;
        }
    }

    function UpdateRecord(id, cid) {
        SetEditMode(id, false, "");
        var recordVals = new Array();
        var error = false;
        $("#" + id + "-editRow").find(".data-td").each(function (index) {
            if (!error) {
                var $input = $(this).find("input");
                var $div = $(this).find(".td-columnName-edit");
                // openWSE.AlertWindow($input.attr("type"));
                if ($input.attr("type") == "checkbox") {
                    if ($input.length > 0) {
                        var recordCols = new Array();
                        recordCols[0] = $.trim($div.text());

                        var isChecked = "0";
                        if ($input.prop("checked")) {
                            isChecked = "1";
                        }

                        recordCols[1] = $.trim(isChecked)
                        recordVals[index] = recordCols;
                    }
                }
                else if (($(this).find(".not-nullable").length > 0) && ($.trim($input.val()) == "")) {
                    error = true;
                }
                else {
                    if ($input.length > 0) {
                        var recordCols = new Array();
                        recordCols[0] = $.trim($div.text());
                        recordCols[1] = $.trim($input.val())
                        recordVals[index] = recordCols;
                    }
                }
            }
        });

        if (!error) {
            openWSE.LoadingMessage1("Updating Record. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/UpdateRecord",
                data: JSON.stringify({ "id": id, "recordVals": recordVals, "cid": cid }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    openWSE.RemoveUpdateModal();
                    if (data.d[0] == "Success") {
                        GetRecords(id, false);
                    }
                    else {
                        openWSE.AlertWindow(data.d[1]);
                    }
                }
            });
        }
        else {
            openWSE.AlertWindow("One or more records cannot be empty. Please check your values and try again.");
        }
    }
    function UpdateRecordKeyPress(event, id, cid) {
        try {
            if (event.which == 13) {
                UpdateRecord(id, cid);
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                UpdateRecord(id, cid);
                return false;
            }
            delete evt;
        }
    }

    function CancelRecord(id) {
        SetEditMode(id, false, "");
        GetRecords(id, false);
    }
    function DeleteRecord(id, cid) {
        openWSE.ConfirmWindow("Are you sure you want to delete this row?",
            function () {
                openWSE.LoadingMessage1("Deleting Record. Please Wait...");
                $.ajax({
                    url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/DeleteRecord",
                    data: "{'id': '" + id + "','cid': '" + cid + "'}",
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        openWSE.RemoveUpdateModal();
                        if (data.d[0] == "Success") {
                            GetRecords(id, false);
                        }
                        else {
                            openWSE.AlertWindow(data.d[1]);
                        }
                    }
                });
            }, null);
    }


    // Sort Functions
    function OnSortClick(_this, col, id) {
        var $_this = $(_this);
        if ($_this.hasClass("asc")) {
            sortDir = "DESC";
        }
        else {
            sortDir = "ASC";
        }

        var found = false;
        for (var i = 0; i < arrSortColCustomTables.length; i++) {
            if (arrSortColCustomTables[i][0] == id) {
                arrSortColCustomTables[i][1] = sortDir;
                arrSortColCustomTables[i][2] = col;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = sortDir;
            arrInner[2] = col;
            arrSortColCustomTables[arrSortColCustomTables.length] = arrInner;
        }

        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function GetSortDirCol(id) {
        var arr = new Array();
        arr[0] = "DESC";
        arr[1] = "TimeStamp";
        for (var i = 0; i < arrSortColCustomTables.length; i++) {
            if (arrSortColCustomTables[i][0] == id) {
                arr[0] = arrSortColCustomTables[i][1];
                arr[1] = arrSortColCustomTables[i][2];
            }
        }

        return arr;
    }


    // Data Chart Functions
    function ViewChart(id, _this) {
        var chartType = $(_this).find("input[type='hidden']").val();

        if (chartType != "" && chartType != null) {
            // Get Chart Data
            MenuClick(id);
            openWSE.LoadingMessage1("Loading Chart Data...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/SetChartView",
                data: JSON.stringify({ "id": id, "view": "true" }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                complete: function (data) {
                    BuildChartData(id, chartType);

                    $("#pnl_" + id + "_tableView").hide();
                    $("#pnl_" + id + "_chartView").show();

                    openWSE.RemoveUpdateModal();
                }
            });
        }
    }
    function BuildChartData(id, chartType) {
        var errorOccurred = false;

        try {
            if ($("#pnl_" + id + "_chartView").length == 1) {
                openWSE.GetScriptFunction("//www.google.com/jsapi", function () {
                    var _innerHtml = "<a href='#' onclick=\"customTables.ViewTableData('" + id + "');return false;\" class=\"customtables-chart-topbtns float-left pad-all\"><span class=\"pg-prev-btn float-left pad-right-sml\" style=\"margin-top: -3px;\"></span>Back</a>";
                    _innerHtml += "<a href='#' class='customtables-chart-topbtns float-right pad-all img-refresh' title='Refresh Data' onclick=\"customTables.Refresh('" + id + "');return false;\"></a>";
                    _innerHtml += "<div class='google-chart-holder'></div>";

                    $("#pnl_" + id + "_chartView").html(_innerHtml);

                    var chartTitle = $.trim($("#hf_" + id + "_chartTitle").val());

                    var chartWidth = $(window).width();
                    var chartHeight = $(window).height();

                    var $mainAppDiv = $("#" + id + "-load").closest(".app-body");
                    if ($mainAppDiv.length > 0) {
                        chartWidth = $mainAppDiv.width();
                        chartHeight = $mainAppDiv.height() - $("#pnl_" + id + "_chartView").find(".customtables-chart-topbtns").outerHeight();
                    }
                    else {
                        chartHeight = chartHeight - ($("#always-visible").outerHeight() + $("#pnl_" + id + "_chartView").find(".customtables-chart-topbtns").outerHeight());
                    }

                    var docEle = $("#pnl_" + id + "_chartView").find(".google-chart-holder")[0];
                    var options = {
                        title: chartTitle,
                        width: chartWidth - 20,
                        height: chartHeight - 10
                    };

                    var chart = null;
                    var dataCollection = new Array();

                    var columnNames = new Array();
                    if (IsViewMobileMode(id)) {
                        if ($(".item-row").length > 0) {
                            $("#" + id + "-complete-table").find(".item-row").eq(0).find(".headerName").each(function () {
                                if (ChartColumnInList(id, $(this).attr("data-columnname"))) {
                                    columnNames.push($.trim($(this).html()));
                                }
                            });
                        }
                    }
                    else {
                        $("#" + id + "-complete-table").find(".myHeaderStyle").find(".td-sort-click").each(function () {
                            if (ChartColumnInList(id, $(this).attr("data-columnname"))) {
                                columnNames.push($.trim($(this).html()));
                            }
                        });
                    }

                    dataCollection.push(columnNames);

                    $("#" + id + "-complete-table").find(".item-row").each(function () {
                        var $itemRow = $(this);
                        var rowData = new Array();

                        $itemRow.find(".data-td").each(function () {
                            var canAdd = true;
                            if (IsViewMobileMode(id)) {
                                if (!ChartColumnInList(id, $(this).parent().find(".headerName").attr("data-columnname"))) {
                                    canAdd = false;
                                }
                            }
                            else {
                                if (!ChartColumnInList(id, $(this).attr("data-columnname"))) {
                                    canAdd = false;
                                }
                            }

                            if (canAdd) {
                                var x = $.trim($(this).html());
                                if ($(this).attr("data-type") == "integer") {
                                    x = parseInt(x);
                                }
                                else if ($(this).attr("data-type") == "decimal") {
                                    x = parseFloat(x);
                                }
                                else if ($(this).attr("data-type") == "money") {
                                    x = x.replace(/$/g, "").replace(/,/g, "").replace(/ /g, "");
                                    x = parseFloat(x);
                                }
                                else if ($(this).attr("data-type") == "boolean") {
                                    x = openWSE.ConvertBitToBoolean(x);
                                }
                                else if ($(this).attr("data-type") == "date" || $(this).attr("data-type") == "datetime") {
                                    x = new Date(x);
                                }

                                rowData.push(x);
                            }
                        });

                        dataCollection.push(rowData);
                    });

                    var packageToLoad = "corechart";
                    if (chartType.toLowerCase() == "gauge") {
                        packageToLoad = "gauge";
                    }

                    google.load("visualization", "1", {
                        callback: function () {
                            var data = google.visualization.arrayToDataTable(dataCollection);
                            ChartLoadCallback(data, options, chartType, docEle);
                        }, packages: [packageToLoad]
                    });
                });
            }
        }
        catch (evt) {
            errorOccurred = true;
        }

        if (errorOccurred) {
            openWSE.AlertWindow("There was an error loading Google Charts API.");
        }
    }
    function ChartColumnInList(id, name) {
        var splitVals = $("#hf_" + id + "_chartColumns").val().split(';');
        for (var i = 0; i < splitVals.length; i++) {
            if (splitVals[i] == name && splitVals[i] != "") {
                return true;
            }
        }

        return false;
    }
    function ChartLoadCallback(data, options, chartType, docEle) {
        var errorOccurred = false;

        try {
            var chartCase = {
                "area": function () {
                    chart = new google.visualization.AreaChart(docEle);
                },
                "bar": function () {
                    chart = new google.visualization.BarChart(docEle);
                },
                "bubble": function () {
                    chart = new google.visualization.BubbleChart(docEle);
                },
                "column": function () {
                    chart = new google.visualization.ColumnChart(docEle);
                },
                "combo": function () {
                    chart = new google.visualization.ComboChart(docEle);
                },
                "donut": function () {
                    var moreOptions = {
                        pieHole: 0.4
                    };
                    $.extend(options, moreOptions);
                    chart = new google.visualization.PieChart(docEle);
                },
                "gauge": function () {
                    chart = new google.visualization.Gauge(docEle);
                },
                "line": function () {
                    chart = new google.visualization.LineChart(docEle);
                },
                "pie": function () {
                    chart = new google.visualization.PieChart(docEle);
                },
                "scatter": function () {
                    chart = new google.visualization.ScatterChart(docEle);
                }
            };

            chartCase[chartType.toLowerCase()]();
            if (chart != null) {
                chart.draw(data, options);
            }
            else {
                errorOccurred = true;
            }
        }
        catch (evt) {
            errorOccurred = true;
        }

        if (errorOccurred) {
            openWSE.AlertWindow("There was an error loading Google Charts API.");
        }
    }
    function ReBuildChartData(id) {
        if ($("#pnl_" + id + "_chartView").css("display") == "block") {
            var chartType = $("#pnl_" + id + "_chartType").find("input[type='hidden']").val();
            BuildChartData(id, chartType);
        }
    }
    function ViewTableData(id) {
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/SetChartView",
            data: JSON.stringify({ "id": id, "view": "false" }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            complete: function (data) {
                openWSE.RemoveUpdateModal();

                $("#pnl_" + id + "_chartView").hide();
                $("#pnl_" + id + "_chartView").html("");
                $("#pnl_" + id + "_tableView").show();

                GetRecords(id, false);
            }
        });
    }
    function ResizeChartData() {
        $(".custom-table-timermarker").each(function () {
            var id = $(this).find(".custom-table-tableView-holder").attr("id").replace("pnl_", "").replace("_tableView", "");
            if ($("#pnl_" + id + "_chartView").css("display") != "none") {
                ReBuildChartData(id);
            }
        });
    }


    // Customization Functions
    function LoadCustomizations(id) {
        try {
            var customizationsStr = unescape($.trim($("#hf_" + id + "_customizations").val()));
            var customizationJson = $.parseJSON(customizationsStr);

            var isMobileView = $("#pnl_" + id + "_tableView").hasClass("mobile-view");

            var $dataHolder = $("#data-holder-" + id);
            if ($dataHolder.length > 0) {
                for (var i = 0; i < customizationJson.length; i++) {
                    var name = customizationJson[i].customizeName;
                    var value = customizationJson[i].customizeValue.replace(/\+/g, " ");
                    if (value != "") {
                        switch (name) {

                            case "HeaderColor":
                                if (!isMobileView) {
                                    $dataHolder.find(".myHeaderStyle").css("background", value);
                                    SetCustomizationForeColor($dataHolder.find(".myHeaderStyle"), value);
                                }
                                else {
                                    $dataHolder.find(".headerName").css("background-color", value);
                                    SetCustomizationForeColor($dataHolder.find(".headerName"), value);
                                }
                                break;

                            case "PrimaryRowColor":
                                if (!isMobileView) {
                                    $dataHolder.find(".item-row").each(function (index) {
                                        if (index % 2 == 0) {
                                            $(this).css("background", value);
                                            SetCustomizationForeColor($(this), value);
                                        }
                                    });
                                }
                                else {
                                    $dataHolder.find(".item-row").each(function (index) {
                                        if (index % 2 == 0) {
                                            $(this).find(".dataItem").css("background", value);
                                            SetCustomizationForeColor($(this).find(".dataItem"), value);
                                        }
                                    });
                                }
                                break;

                            case "AlternativeRowColor":
                                if (!isMobileView) {
                                    $dataHolder.find(".item-row").each(function (index) {
                                        if (index % 2 != 0) {
                                            $(this).css("background", value);
                                            SetCustomizationForeColor($(this), value);
                                        }
                                    });
                                }
                                else {
                                    $dataHolder.find(".item-row").each(function (index) {
                                        if (index % 2 != 0) {
                                            $(this).find(".dataItem").css("background", value);
                                            SetCustomizationForeColor($(this).find(".dataItem"), value);
                                        }
                                    });
                                }
                                break;

                            case "FontFamily":
                                $dataHolder.css("font-family", value);
                                break;

                        }
                    }
                }
            }
        }
        catch (evt) { }
    }
    function SetCustomizationForeColor(ele, backgroundColor) {
        var rgbVal = openWSE.HexToRgb(backgroundColor);
        if (rgbVal) {
            if (rgbVal.r + rgbVal.g + rgbVal.b < 425) {
                $(ele).addClass("customization-light-color");
            }
            else {
                $(ele).addClass("customization-dark-color");
            }
        }
    }
    function LoadFontSizes(id) {
        try {
            var fontSize = cookie.get(id + "-fontsize");
            if ((fontSize != null) && (fontSize != "")) {
                $("." + id + "-record-font").css("font-size", fontSize);
                $("#data-holder-" + id).css("font-size", fontSize);
                $("#font-size-selector-" + id).val(fontSize);
            }
        }
        catch (evt) { }
    }
    function ChangeFont(id) {
        var fontsize = $("#font-size-selector-" + id).val();
        $("." + id + "-record-font").css("font-size", fontsize);
        $("#data-holder-" + id).css("font-size", fontsize);
        $("#data-holder-" + id).find("input").css("font-size", fontsize);
        cookie.set(id + "-fontsize", fontsize, "30");
    }


    function SetFocus(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        if ($("#" + id + "-" + rowType).find("input").length > 0) {
            $("#" + id + "-" + rowType).find("input").eq(0).focus();
        }
    }
    function SaveInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }
        arrAddValuesCustomTables = new Array();
        $("#" + id + "-" + rowType).find("input").each(function (index) {
            arrAddValuesCustomTables[index] = $(this).val();
        });
    }
    function RestoreInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        $("#" + id + "-" + rowType).find("input").each(function (index) {
            $(this).val(arrAddValuesCustomTables[index]);
        });
        arrAddValuesCustomTables = new Array();
    }
    String.prototype.formatMoney = function (c, d, t) {
        var n = this,
            c = isNaN(c = Math.abs(c)) ? 2 : c,
            d = d == undefined ? "." : d,
            t = t == undefined ? "," : t,
            s = n < 0 ? "-" : "",
            i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "",
            j = (j = i.length) > 3 ? j % 3 : 0;
        return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
    };

    return {
        TablePageLoad: TablePageLoad,
        Load: Load,
        Refresh: Refresh,
        KeyPressSearch: KeyPressSearch,
        Search: Search,
        ViewChart: ViewChart,
        ViewTableData: ViewTableData,
        RecordstoSelect: RecordstoSelect,
        ExportToExcelAll: ExportToExcelAll,
        MonthSelect: MonthSelect,
        AddRecord: AddRecord,
        EditRecord: EditRecord,
        DeleteRecord: DeleteRecord,
        UpdateRecord: UpdateRecord,
        CancelRecord: CancelRecord,
        OnSortClick: OnSortClick,
        AddRecordKeyPress: AddRecordKeyPress,
        UpdateRecordKeyPress: UpdateRecordKeyPress,
        MenuClick: MenuClick,
        AddRecord_MenuClick: AddRecord_MenuClick,
        ViewRecords_MenuClick: ViewRecords_MenuClick,
        ChangeFont: ChangeFont,
        ChangeViewMode: ChangeViewMode,
        UpdateColumnsToShow: UpdateColumnsToShow,
        ResizeChartData: ResizeChartData
    }
}();
