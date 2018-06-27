/* Table Import Functions
----------------------------------*/
Sys.Application.add_load(function () {
    dbImport.TablePageLoad();
});

$(document).ready(function () {
    dbImport.TablePageLoad();
});

var resizeId_DBImport;
$(window).resize(function () {
    clearTimeout(resizeId_DBImport);
    resizeId_DBImport = setTimeout(function () {
        dbImport.ResizeChartData();
    }, 500);
});

var dbImport = function () {
    var arrSortColCustomTables = new Array();
    var arrEditModeCustomTables = new Array();
    var arrAddValuesCustomTables = new Array();
    var arrPageSizeCustomTables = new Array();
    var timerIsRunning = false;
    var timerIntveral = 1000 * 60;
    var timerCustomTables;

    
    // Load Functions
    function TablePageLoad() {
        $(".dbImport-table-timermarker").each(function () {
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
            cookieFunctions.del(id + "-fontsize");
            cookieFunctions.del(id + "-customtable-viewmode");
        });

        cookieFunctions.get(id + "-customtable-viewmode", function (viewMode) {
            if ((viewMode != null && viewMode != undefined && viewMode == "mobile") || isMobileDevice()) {
                $("#pnl_" + id + "_tableView").addClass("mobile-view");
                if (isMobileDevice()) {
                    $("#li_" + id + "_viewMode").hide();
                }
                else {
                    $("#li_" + id + "_viewMode").show();
                }
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
        });
    }
    function Timer() {
        timerIsRunning = true;
        timerCustomTables = setTimeout(function () {
            $(".dbImport-table-timermarker").each(function (index) {
                var id = $(this).find(".custom-table-tableView-holder").attr("id").replace("pnl_", "").replace("_tableView", "");
                if (id != "") {
                    GetRecords(id, true);
                }
            });
        }, timerIntveral);
    }
    $(document.body).on("click", ".dbImport-table-timermarker .custom-table-tableView-holder, .dbImport-table-timermarker .customtable-sidebar-open-overlay", function (e) {
        if (e.target.className.indexOf("img-appmenu") == -1) {
            var $this = $(this).closest(".dbImport-table-timermarker").find(".custom-table-tableView-holder");
            var id = $this.attr("id").replace("pnl_", "").replace("_tableView", "");
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
        if (openWSE.TableFormulas.IsMobileViewMode(id) && $("#hf_" + id + "_tableview").val() !== "excel") {
            $("#" + id + "-sidebar-menu-viewallrecords").show();
            $("#search_" + id + "_holder").removeClass("float-right");
            $("#search_" + id + "_holder").find(".searchwrapper").css("width", "100%");
            $("#search_" + id + "_divider").show();
            if (!$("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                $("#pnl_" + id + "_tableView").addClass("mobile-view");
            }

            $("#" + id + "-sidebar-menu").find(".desktop-img").removeClass("active");
            $("#" + id + "-sidebar-menu").find(".mobile-img").addClass("active");

            if ($.trim($("#" + id + "-complete-table").find(".custom-table-add-mobile").html()) != "") {
                var viewMode = $("#pnl_" + id + "_tableView").attr("data-viewmode");
                if (viewMode == "mobile-add") {
                    ShowSummaryButton(id);
                    $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-summary-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-add-mobile").show();
                }
                else if (viewMode == "mobile-summary") {
                    $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-summary-mobile").show();
                }
                else {
                    ShowSummaryButton(id);
                    $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-summary-mobile").hide();
                    $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();
                }
            }
            else {
                $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-view")
                ShowSummaryButton(id);
                $("#" + id + "-sidebar-menu-viewallrecords").hide();
                $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
                $("#" + id + "-complete-table").find(".custom-table-summary-mobile").hide();
                $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();
            }
        }
        else {
            $("#" + id + "-sidebar-menu-viewallrecords").hide();
            $("#search_" + id + "_holder").addClass("float-right");
            $("#search_" + id + "_holder").find(".searchwrapper").css("width", "");
            $("#search_" + id + "_divider").hide();
            if ($("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                $("#pnl_" + id + "_tableView").removeClass("mobile-view");
            }

            $("#" + id + "-sidebar-menu").find(".desktop-img").addClass("active");
            $("#" + id + "-sidebar-menu").find(".mobile-img").removeClass("active");
        }
    }
    function ChangeViewMode(id, selectedMode) {
        var _tempMode = "desktop";
        if (selectedMode == "mobile") {
            $("#pnl_" + id + "_tableView").addClass("mobile-view");
            _tempMode = "mobile";
        }
        else {
            $("#pnl_" + id + "_tableView").removeClass("mobile-view");
        }

        cookieFunctions.set(id + "-customtable-viewmode", _tempMode, "30", function () {
            GetRecords(id, false);
        });
    }

    function ShowSummaryButton(id) {
        if ($.trim($("#" + id + "-complete-table").find(".custom-table-summary-mobile").html()) != "") {
            $("#btn_" + id + "_viewSummary").show();
        }
        else {
            $("#btn_" + id + "_viewSummary").hide();
        }
    }

    // Initialize Jquery Functions
    function InitJquery(id) {
        openWSE.GetScriptFunction("//www.google.com/jsapi", function () {
            google.load("visualization", "1", { callback: function () { }, packages: ["corechart"] });
            google.load("visualization", "1", { callback: function () { }, packages: ["gauge"] });
        });

        if ($(".app-main-external").length == 0) {
            $(".app-main-holder[data-appid='app-" + id + "']").on("resizestop", function (event, ui) {
                ReBuildChartData(id);
            });

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

            var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
            if ($.trim(columnType) == "date") {
                if (isChrome) {
                    $input.attr("type", "date");
                }
                else {
                    $input.datepicker({
                        changeMonth: true,
                        changeYear: true,
                        constrainInput: false
                    });
                }
            }
            else if ($.trim(columnType) == "datetime") {
                if (isChrome) {
                    $input.attr("type", "datetime-local");
                }
                else {
                    $input.datepicker({
                        changeMonth: true,
                        changeYear: true,
                        constrainInput: false
                    });
                }
            }
            else if ($input.attr("type") == "number") {
                if ($.trim(columnType) == "decimal") {
                    $input.attr("step", "0.1");
                    if (!$input.val()) {
                        $input.val("0.0");
                    }
                }
                else if ($.trim(columnType) == "money") {
                    $input.attr("step", "0.01");
                    if (!$input.val()) {
                        $input.val("0.00");
                    }
                }
                else if (!$input.val()) {
                    $input.val("0");
                }
            }
            else if ($.trim(columnType) == "boolean") {
                $input.css("width", "auto");
                $input.attr("type", "checkbox");
                $input.attr("onkeyup", "");
                $input.prop("checked", false);
                $input.attr("value", "");
                if ($input.val() !== "" && openWSE.ConvertBitToBoolean($input.val())) {
                    $input.prop("checked", true);
                }
            }

            if ($.trim(columnType) != "boolean") {
                if ($input.attr("type") == "text" && $("#hf_" + id + "_tableview").val() !== "excel") {
                    $input.after("");
                    $input.parent().prepend("<div class='auto-complete-dropdown-holder'><span class=\"auto-complete-dropdown img-menudropdown\" onclick=\"dbImport.AutoCompleteDropdownClick(this, '" + id + "');\"></span></div>");
                    $input.parent().find(".auto-complete-dropdown-holder").prepend($input);
                }

                $input.autocomplete({
                    minLength: 0,
                    autoFocus: false,
                    source: function (request, response) {
                        openWSE.AjaxCall("WebServices/AutoComplete.asmx/GetdbImportData", "{ 'prefixText': '" + escape(request.term) + "', 'count': '-1', 'id': '" + id + "', 'columnName': '" + columnName + "' }", {
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
                });
            }
        });
    }
    function AutoCompleteDropdownClick(ele, id) {
        var $input = $(ele).parent().find("input[type='text']");
        if ($input.length > 0) {
            $input.autocomplete("search", "");
        }
    }


    // Get Functions
    function GetRecords(id, fromTimer) {
        if (!fromTimer) {
            loadingPopup.Message("Loading...");
        }

        var arr = GetSortDirCol(id);
        SaveInputAddValues(id, false);
        openWSE.AjaxCall("WebServices/dbImportservice.asmx/GetRecords", "{'id': '" + id + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "'}", null, function (data) {
            if (data.d.length > 0) {
                var isLocal = data.d[3];
                var applicationId = data.d[4];
                if (data.d[0].length > 0) {
                    var innerHeader = data.d[0][0];
                    var innerAddRow = data.d[0][1];
                    var innerItemRow = data.d[0][2];
                    var allowEdit = data.d[1] == "true";

                    if ($.trim($("#cblist_" + id + "_holder").html()) == "") {
                        $("#cblist_" + id + "_holder").html(BuildColumnCheckList(id, innerHeader));
                    }

                    $("#data-holder-" + id).html(BuildTable(id, innerHeader, innerAddRow, innerItemRow, false, allowEdit));

                    openWSE.TableFormulas.SetSummaryData(id);
                    LoadFontSizes(id);
                    RestoreInputAddValues(id, false);
                    LoadDefaultValues(id);
                    AutoCompleteAddEdit(id, false);
                    RefreshColumnsToShow(id);

                    $("#" + id + "-addRow").find("input[data-type='guid']").each(function () {
                        if (isLocal && $(this).closest(".data-td").attr("data-columnname") === "ApplicationId") {
                            $(this).val(applicationId);
                        }
                        else if ($.trim($(this).val()) == "") {
                            $(this).val(GenerateGuid());
                        }
                    });
                }
                else {
                    $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
                }

                SetupViewModeForTable(id);
                loadingPopup.RemoveMessage();

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
                HideAllApplicationIds(id, isLocal);
            }
        });
    }
    function GetRowData(_this) {
        var rowData = new Array();
        $(_this).closest(".item-row").find(".data-td").each(function (index) {
            if ($(this).find(".td-columnValue-edit").length > 0) {
                rowData[index] = $.trim($(this).find(".td-columnValue-edit").html());
            }
            else {
                rowData[index] = $.trim($(this).html());
            }
        });

        return rowData;
    }

    function HideAllApplicationIds(id, isLocal) {
        if (isLocal) {
            $("#" + id + "-complete-table").find("td[data-columnname='ApplicationId']").hide();
            $("#cb_" + id + "_ApplicationId").hide();
            $("label[for='cb_" + id + "_ApplicationId']").hide();
        }
    }

    // Sidebar Menu Functions
    function AddRecord_MenuClick(id) {
        $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-add");
        ShowSummaryButton(id);
        $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-summary-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-add-mobile").show();

        $("#pnl_" + id + "_chartView").hide();
        $("#pnl_" + id + "_tableView").show();

        MenuClick(id);
    }
    function ViewRecords_MenuClick(id) {
        $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-view");
        ShowSummaryButton(id);
        $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-summary-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-data-mobile").show();

        $("#pnl_" + id + "_chartView").hide();
        $("#pnl_" + id + "_tableView").show();

        MenuClick(id);
    }
    function ViewSummary_MenuClick(id) {
        $("#pnl_" + id + "_tableView").attr("data-viewmode", "mobile-summary");
        $("#" + id + "-complete-table").find(".custom-table-add-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-data-mobile").hide();
        $("#" + id + "-complete-table").find(".custom-table-summary-mobile").show();

        $("#pnl_" + id + "_chartView").hide();
        $("#pnl_" + id + "_tableView").show();

        MenuClick(id);
    }
    function MenuClick(id) {
        var $menu = $("#" + id + "-sidebar-menu");
        if ($menu.length > 0) {
            if (!$menu.hasClass("showmenu")) {
                $menu.addClass("showmenu");
                $menu.after("<div class='customtable-sidebar-open-overlay'></div>");
            }
            else {
                $menu.parent().find(".customtable-sidebar-open-overlay").remove();
                $menu.removeClass("showmenu");
            }
        }
    }
    function Refresh(id) {
        loadingPopup.Message("Loading...");

        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function RecordstoSelect(id) {
        loadingPopup.Message("Loading...");
        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }
    function ExportToExcelAll(id) {
        loadingPopup.Message("Exporting...");
        openWSE.AjaxCall("WebServices/dbImportservice.asmx/ExportToExcel", "{ 'id': '" + id + "' }", null, function (data) {
            loadingPopup.RemoveMessage();
            if (data.d == "File path wrong.") {
                openWSE.AlertWindow("Error saving file. Check file path under System Settings.");
            }
            else if (data.d != "") {
                $.fileDownload(data.d);
            }

            else {
                openWSE.AlertWindow("No records to export");
            }
        });
    }

    // Search Functions
    function Search(id) {
        if (openWSE.TableFormulas.IsMobileViewMode(id)) {
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
                var columnName = innerHeader[i][4];
                var shownName = innerHeader[i][1];
                var cbInput = "<input type=\"checkbox\" id=\"cb_" + id + "_" + columnName + "\" value=\"" + columnName + "\" checked=\"checked\" onchange=\"dbImport.UpdateColumnsToShow('" + id + "', this, true);\" />";
                cbInput += "<label for=\"cb_" + id + "_" + columnName + "\">&nbsp;" + shownName + "</label>";
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
            if (!openWSE.TableFormulas.IsMobileViewMode(id)) {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").show();
            }
            else {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").parent().show();
            }
        }
        else {
            if (!openWSE.TableFormulas.IsMobileViewMode(id)) {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").hide();
            }
            else {
                $("#pnl_" + id + "_tableView").find("td[data-columnname='" + val + "']").parent().hide();
            }
        }

        if (checkTotal) {
            CheckTotalColumnsChecked(id);
            SetupGridviewPager(id);
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
        SetupGridviewPager(id);
    }


    // Build Table for Mobile/Desktop
    function BuildTable(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        if (openWSE.TableFormulas.IsMobileViewMode(id)) {
            return BuildMobileView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
        }
        else {
            return BuildDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
        }
    }
    function BuildMobileView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        var showRowCount = CanShowRowCount(id);

        var addDiv = "";
        if (allowEdit && !editMode) {
            addDiv = "<table id='" + id + "-addRow'>";
            for (var i = 0; i < innerAddRow.length; i++) {
                addDiv += "<tr>";
                addDiv += "<td class='headerName'>" + innerHeader[i][1] + "</td>";
                addDiv += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class='data-td addRow'>";
                addDiv += "<div class='input-customtable-holder'><input type='text' class='textEntry-noWidth' maxlength='" + innerAddRow[i][1] + "' onkeyup=\"dbImport.AddRecordKeyPress(event, '" + innerAddRow[i][2] + "');\" data-type='" + innerAddRow[i][3] + "' style='width: 100%;' />" + innerAddRow[i][4] + "</div>";
                addDiv += "</td></tr>";
            }
            addDiv += "</table><div class='addBtn'><input type='button' class='input-buttons-create' value='Add Entry' onclick=\"dbImport.AddRecord('" + id + "');return false;\" /></div>";
        }

        var itemDiv = "";
        if (innerItemRow.length > 0) {
            for (var i = 0; i < innerItemRow.length; i++) {
                if (!editMode) {
                    itemDiv += "<div class=\"item-row\">";
                }
                else {
                    if (openWSE.ConvertBitToBoolean(innerItemRow[i][1])) {
                        itemDiv += "<div id=\"" + id + "-editRow\" class=\"item-row\">";
                    }
                    else {
                        itemDiv += "<div class=\"item-row\">";
                    }
                }

                itemDiv += "<table>";
                if (showRowCount) {
                    itemDiv += "<tr><td class=\"custom-table-rowcount-mobile\" colspan=\"2\">" + (i + 1).toString() + "</td></tr>";
                }

                for (var j = 0; j < innerItemRow[i][0].length; j++) {
                    var colName = innerHeader[j][1].replace(/_/g, " ");
                    itemDiv += "<tr>";
                    itemDiv += "<td data-columnname=\"" + innerHeader[j][4] + "\" class=\"headerName td-sort-click" + innerHeader[j][0] + "\" onclick=\"dbImport.OnSortClick(this,'" + innerHeader[j][4] + "','" + innerHeader[j][2] + "');\" title=\"Sort by " + colName + "\">" + colName + "</td>";
                    itemDiv += "<td class=\"data-td dataItem\" data-type=\"" + innerItemRow[i][0][j][1].toString() + "\">" + innerItemRow[i][0][j][0].toString() + "</td>";
                    itemDiv += "</tr>";
                }
                itemDiv += "</table>";

                if (allowEdit) {
                    var editBtn = "";
                    var delBtn = "";
                    if (!editMode) {
                        editBtn = "<a href=\"javascript:void(0);\" class=\"input-buttons-create float-left margin-right\" onclick=\"dbImport.EditRecord('" + id + "', this);return false;\">Edit</a>";
                        delBtn = "<a href=\"javascript:void(0);\" class=\"input-buttons-create float-right margin-left\" onclick=\"dbImport.DeleteRecord('" + id + "', this);return false;\">Delete</a>";
                    }
                    else {
                        if (openWSE.ConvertBitToBoolean(innerItemRow[i][1])) {
                            editBtn = "<a href=\"javascript:void(0);\" class=\"input-buttons-create float-left margin-right\" onclick=\"dbImport.UpdateRecord('" + id + "', this);return false;\">Update</a>";
                            delBtn = "<a href=\"javascript:void(0);\" class=\"input-buttons-create float-right margin-left\" onclick=\"dbImport.CancelRecord('" + id + "');return false;\">Cancel</a>";
                        }
                    }
                    itemDiv += "<div class=\"edit-buttons myItemStyle-action-btns\" align=\"center\">" + editBtn + delBtn + "<div class='clear'></div></div>";
                }
                itemDiv += "</div>";
            }
        }
        else {
            itemDiv = "<div class='emptyGridView'>No data found</div>";
        }

        var summaryDiv = openWSE.TableFormulas.BuildSummaryHolder(id);

        if (addDiv == "" && itemDiv == "") {
            return "<div id=\"" + id + "-complete-table\"><div class='emptyGridView'>No data found</div></div>";
        }
        else {
            return "<div id=\"" + id + "-complete-table\"><div class='custom-table-add-mobile'>" + addDiv + "</div><div class='custom-table-summary-mobile'>" + summaryDiv + "</div><div class='custom-table-data-mobile'>" + itemDiv + "</div></div>";
        }
    }
    function BuildDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        switch ($("#hf_" + id + "_tableview").val()) {
            case "excel":
                return ExcelDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
            default:
                return DefaultDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit);
        }
    }

    function DefaultDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        var showRowCount = CanShowRowCount(id);

        var xHeader = "<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">";
        xHeader += "<tbody><tr class=\"myHeaderStyle\">";
        if (showRowCount) {
            xHeader += "<td style=\"width: 45px;\"></td>";
        }

        for (var i = 0; i < innerHeader.length; i++) {
            var colName = innerHeader[i][1];
            xHeader += "<td data-columnname=\"" + innerHeader[i][4] + "\" class=\"td-sort-click" + innerHeader[i][0] + "\" onclick=\"dbImport.OnSortClick(this,'" + innerHeader[i][4] + "','" + innerHeader[i][2] + "');\" title=\"Sort by " + colName + "\">" + colName + "</td>";
        }

        if (allowEdit) {
            xHeader += "<td class=\"edit-buttons myItemStyle-action-btns\" style=\"width: 70px;\"></td></tr>";
        }

        var xAddRow = "";
        if (allowEdit) {
            xAddRow = "<tr id=\"" + id + "-addRow\" class=\"GridNormalRow myItemStyle\">";

            if (showRowCount) {
                xAddRow += "<td class=\"GridViewNumRow border-bottom\"><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
            }

            if (!editMode) {
                for (var i = 0; i < innerAddRow.length; i++) {
                    var align = "";
                    if (innerAddRow[i][3] == "boolean") {
                        align = " align='left'";
                    }

                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"data-td border-bottom\"" + align + ">";
                    xAddRow += "<div class='input-customtable-holder'><input type='text' class='textEntry-noWidth' maxlength='" + innerAddRow[i][1] + "' onkeyup=\"dbImport.AddRecordKeyPress(event, '" + innerAddRow[i][2] + "');\" data-type='" + innerAddRow[i][3] + "' style='width: 100%;' />" + innerAddRow[i][4] + "</div></td>";
                }
                xAddRow += "<td class=\"edit-buttons myItemStyle-action-btns border-bottom\" align=\"center\"><a href=\"#Add\" class=\"td-add-btn\" onclick=\"dbImport.AddRecord('" + id + "');return false;\" title=\"Add Row\"></a></td>";
            }
            else {
                for (var i = 0; i < innerAddRow.length; i++) {
                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"border-bottom\"></td>";
                }
                xAddRow += "<td class=\"edit-buttons myItemStyle-action-btns border-bottom\" align=\"center\" style='height: 27px;'></td>";
            }

            xAddRow += "</tr>";
        }

        var xItem = "";
        for (var i = 0; i < innerItemRow.length; i++) {
            if (!editMode) {
                xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
            }
            else {
                if (openWSE.ConvertBitToBoolean(innerItemRow[i][1])) {
                    xItem += "<tr id=\"" + id + "-editRow\" class=\"GridNormalRow myItemStyle item-row\">";
                }
                else {
                    xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
                }
            }

            if (showRowCount) {
                xItem += "<td class=\"GridViewNumRow border-bottom\"><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
            }

            for (var j = 0; j < innerItemRow[i][0].length; j++) {
                xItem += "<td data-columnname=\"" + innerHeader[j][4] + "\" class=\"data-td border-bottom\" data-type=\"" + innerItemRow[i][0][j][1].toString() + "\">" + innerItemRow[i][0][j][0].toString() + "</td>";
            }

            if (allowEdit) {
                var editBtn = "";
                var delBtn = "";
                if (!editMode) {
                    editBtn = "<a href=\"#Edit\" class=\"td-edit-btn\" onclick=\"dbImport.EditRecord('" + id + "', this);return false;\" title=\"Edit Row\"></a>";
                    delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"dbImport.DeleteRecord('" + id + "', this);return false;\" title=\"Delete Row\"></a>";
                }
                else {
                    if (openWSE.ConvertBitToBoolean(innerItemRow[i][1])) {
                        editBtn = "<a href=\"#Update\" class=\"td-update-btn\" onclick=\"dbImport.UpdateRecord('" + id + "', this);return false;\" title=\"Update Row\"></a>";
                        delBtn = "<a href=\"#Cancel\" class=\"td-cancel-btn\" onclick=\"dbImport.CancelRecord('" + id + "');return false;\" title=\"Cancel Edit\"></a>";
                    }
                }

                var tdStyle = "";
                if (editBtn === "" && delBtn === "") {
                    tdStyle = " style='height: 27px;'";
                }

                xItem += "<td class=\"edit-buttons myItemStyle-action-btns border-bottom\" align=\"center\"" + tdStyle + ">" + editBtn + delBtn + "</td>";
            }
            xItem += "</tr>";
        }

        if (xAddRow == "" && xItem == "") {
            return openWSE.TableFormulas.BuildSummaryHolder(id) + "<div id=\"" + id + "-complete-table\">" + xHeader + "</tbody></table><div class='emptyGridView'>No data found</div></div>";
        }
        else {
            return openWSE.TableFormulas.BuildSummaryHolder(id) + "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + xItem + "</tbody></table></div>";
        }
    }

    // Excel View for Desktop
    function ExcelDesktopView(id, innerHeader, innerAddRow, innerItemRow, editMode, allowEdit) {
        var showRowCount = CanShowRowCount(id);

        var xHeader = "<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">";
        xHeader += "<tbody><tr class=\"myHeaderStyle-excel\">";
        if (showRowCount) {
            xHeader += "<td></td>";
        }

        for (var i = 0; i < innerHeader.length; i++) {
            var colName = innerHeader[i][1];
            xHeader += "<td data-columnname=\"" + innerHeader[i][4] + "\" class=\"td-sort-click" + innerHeader[i][0] + "\" onclick=\"dbImport.OnSortClick(this,'" + innerHeader[i][4] + "','" + innerHeader[i][2] + "');\" title=\"Sort by " + colName + "\">" + colName + "</td>";
        }

        if (allowEdit) {
            xHeader += "<td class=\"edit-buttons myItemStyle-action-btns edit-column-1-items\"></td></tr>";
        }

        var xAddRow = "";
        if (allowEdit) {
            xAddRow = "<tr id=\"" + id + "-addRow\" class=\"myItemStyle-excel\">";

            if (showRowCount) {
                xAddRow += "<td><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
            }

            if (!editMode) {
                for (var i = 0; i < innerAddRow.length; i++) {
                    var align = "";
                    if (innerAddRow[i][3] == "boolean") {
                        align = " align='left'";
                    }

                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"data-td border-bottom\"" + align + ">";
                    xAddRow += "<div class='input-customtable-holder'><input type='text' class='textEntry-noWidth' maxlength='" + innerAddRow[i][1] + "' onkeyup=\"dbImport.AddRecordKeyPress(event, '" + innerAddRow[i][2] + "');\" data-type='" + innerAddRow[i][3] + "' style='width: 100%;' />" + innerAddRow[i][4] + "</div></td>";
                }
                xAddRow += "<td class=\"edit-buttons myItemStyle-action-btns\" align=\"center\"><a href=\"#Add\" class=\"td-add-btn\" onclick=\"dbImport.AddRecord('" + id + "');return false;\" title=\"Add Row\"></a></td>";
            }
            else {
                for (var i = 0; i < innerAddRow.length; i++) {
                    xAddRow += "<td data-columnname=\"" + innerAddRow[i][0] + "\" class=\"border-bottom\"></td>";
                }
                xAddRow += "<td class=\"edit-buttons myItemStyle-action-btns\" align=\"center\" style='height: 27px;'></td>";
            }

            xAddRow += "</tr>";
        }

        var xItem = "";
        for (var i = 0; i < innerItemRow.length; i++) {
            if (!editMode) {
                xItem += "<tr class=\"myItemStyle-excel item-row\">";
            }
            else {
                if (openWSE.ConvertBitToBoolean(innerItemRow[i][1])) {
                    xItem += "<tr id=\"" + id + "-editRow\" class=\"myItemStyle-excel item-row\">";
                }
                else {
                    xItem += "<tr class=\"myItemStyle-excel item-row\">";
                }
            }

            if (showRowCount) {
                xItem += "<td><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
            }

            for (var j = 0; j < innerItemRow[i][0].length; j++) {
                var onClickEvent = "";
                var innerVal = innerItemRow[i][0][j][0].toString();
                if (allowEdit && (innerVal.indexOf("<input") == -1 || innerVal.indexOf("disabled") > 0)) {
                    onClickEvent = " onclick=\"dbImport.EditRecord('" + id + "', this);return false;\"";
                }

                xItem += "<td data-columnname=\"" + innerHeader[j][4] + "\" class=\"data-td\"" + onClickEvent + " data-type=\"" + innerItemRow[i][0][j][1].toString() + "\">" + innerItemRow[i][0][j][0].toString() + "</td>";
            }

            if (allowEdit) {
                var delBtn = "";
                if (!editMode) {
                    delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"dbImport.DeleteRecord('" + id + "', this);return false;\" title=\"Delete Row\"></a>";
                }

                var tdStyle = "";
                if (delBtn === "") {
                    tdStyle = " style='height: 27px;'";
                }

                xItem += "<td class=\"edit-buttons myItemStyle-action-btns\" align=\"center\"" + tdStyle + ">" + delBtn + "</td>";
            }
            xItem += "</tr>";
        }

        if (xAddRow == "" && xItem == "") {
            return openWSE.TableFormulas.BuildSummaryHolder(id) + "<div id=\"" + id + "-complete-table\">" + xHeader + "</tbody></table><div class='emptyGridView'>No data found</div></div>";
        }
        else {
            return openWSE.TableFormulas.BuildSummaryHolder(id) + "<div id=\"" + id + "-complete-table\">" + xHeader + xItem + xAddRow + "</tbody></table></div>";
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
                        recordCols[1] = $.trim($input.val())
                        recordVals[index] = recordCols;
                    }
                }
            }
        });

        if (!error) {
            loadingPopup.Message("Adding Record. Please Wait...");
            openWSE.AjaxCall("WebServices/dbImportservice.asmx/AddRecord", JSON.stringify({ "id": id, "recordVals": recordVals }), null, function (data) {
                loadingPopup.RemoveMessage();
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
    function GenerateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function EditRecord(id, _this) {
        loadingPopup.Message("Loading...");
        var arr = GetSortDirCol(id);

        var indexOfEle = 0;
        if ($("#hf_" + id + "_tableview").val() === "excel") {
            indexOfEle = $(_this).index();
        }

        openWSE.AjaxCall("WebServices/dbImportservice.asmx/EditRecord", JSON.stringify({ "id": id, "rowData": GetRowData(_this), "search": $("#tb_search_" + id).val(), "recordstopull": $("#RecordstoSelect_" + id).val(), "sortCol": arr[1], "sortDir": arr[0] }), null, function (data) {
            if (data.d.length > 0) {
                var innerHeader = data.d[0];
                var innerAddRow = data.d[1];
                var innerItemRow = data.d[2];
                var isLocal = data.d[3];

                $("#data-holder-" + id).html(BuildTable(id, innerHeader, innerAddRow, innerItemRow, true, true));

                openWSE.TableFormulas.SetSummaryData(id);
                LoadFontSizes(id);
                SetEditMode(id, true, _this);
                LoadDefaultValues(id);

                if ($("#hf_" + id + "_tableview").val() === "excel") {
                    $("#" + id + "-editRow").find("input").each(function (index1) {
                        if (index1 !== indexOfEle) {
                            $(this).closest(".data-td").find(".input-customtable-holder").hide();
                            $(this).closest(".data-td").find(".td-columnName-edit").hide();
                            $(this).closest(".data-td").find(".td-columnValue-edit").show();
                        }
                        else {
                            $(this).attr("onblur", "dbImport.UpdateRecord('" + id + "', this);");
                        }
                    });
                }

                AutoCompleteAddEdit(id, true);
                SetFocus(id, true, indexOfEle);

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
                            $parentEle.find("input[type='number']").val(val.replace(/,/g, ""));
                        }
                        else if ($parentEle.find("input[type='checkbox']").length > 0) {
                            $parentEle.find("input[type='checkbox']").prop("checked", (val !== "" && openWSE.ConvertBitToBoolean(val)));
                        }
                        else if ($parentEle.find("input[type='datetime-local']").length > 0) {
                            try {
                                var tempDate = new Date(val);
                                var day = ("0" + tempDate.getDate()).slice(-2);
                                var month = ("0" + (tempDate.getMonth() + 1)).slice(-2);
                                var newDate = tempDate.getFullYear() + "-" + (month) + "-" + (day);
                                newDate += "T";
                                var hours = ("0" + tempDate.getHours()).slice(-2);
                                var minutes = ("0" + tempDate.getMinutes()).slice(-2);
                                var seconds = ("0" + tempDate.getSeconds()).slice(-2);
                                $parentEle.find("input").val(newDate + hours + ":" + minutes + ":" + seconds + ".000");
                            }
                            catch (evt) { }
                        }
                        else if ($parentEle.find("input[type='date']").length > 0) {
                            try {
                                var tempDate = new Date(val);
                                var day = ("0" + tempDate.getDate()).slice(-2);
                                var month = ("0" + (tempDate.getMonth() + 1)).slice(-2);
                                var newDate = tempDate.getFullYear() + "-" + (month) + "-" + (day);
                                $parentEle.find("input").val(newDate);
                            }
                            catch (evt) { }
                        }
                        else {
                            $parentEle.find("input").val(val);
                        }
                    });
                }

                SetupViewModeForTable(id);
                LoadCustomizations(id);
                HideAllApplicationIds(id, isLocal);

                loadingPopup.RemoveMessage();
            }
            else {
                openWSE.AlertWindow("Error pulling records.");
            }
        });
    }

    function SetupGridviewPager(id) {
        var $tableEle = $("#data-holder-" + id).find("#" + id + "-complete-table > table");
        if ($tableEle.length > 0) {
            $("#data-holder-" + id).find("#" + id + "-complete-table").removeClass("gridview-table-holder");
            $tableEle.removeClass("gridview-table");
            if ($("#hf_" + id + "_tableview").val() !== "excel" && !$("#pnl_" + id + "_tableView").hasClass("mobile-view")) {
                $("#data-holder-" + id).find("#" + id + "-complete-table").addClass("gridview-table-holder");
                $tableEle.addClass("gridview-table");
                $tableEle.attr("data-tableid", id + "_Gridview");
                $tableEle.attr("data-initalsortcolumn", "");
                $tableEle.attr("data-initalsortdir", "");
                $tableEle.attr("data-allowpaging", "true");
                $tableEle.attr("data-columnspan", GetColSpan(id));
                $tableEle.find(".GridViewPager").remove();
                $tableEle.find("tbody").append(BuildPager(id));
                $tableEle.find(".GridViewPager .table-pagesize-selector > select").val(GetPageSize(id));
                openWSE.GridViewMethods.InitializeTable(id + "_Gridview");
            }
        }
    }
    function BuildPager(id) {
        var str = "<tr class='GridViewPager'><td colspan='" + GetColSpan(id) + "'>";
        str += "<div class='table-pagesize-selector'><span class='font-bold margin-right'>Page size:</span><select onchange=\"openWSE.GridViewMethods.PageSizeChange(this);dbImport.SetPageSize('" + id + "', this);\" style='margin-left: 0 !important;'><option value='10'>10</option><option value='20'>20</option><option value='30'>30</option><option value='40'>40</option><option value='50'>50</option><option value='all'>All</option></select><div class='clear'></div></div>";
        str += "<div class='table-pagesize-outof'></div>";
        str += "<div class='gridview-pager-holder'></div>";
        str += "<div class='clear'></div>";
        str += "</td></tr>";
        return str;
    }
    function GetColSpan(id) {
        var count = 0;
        $("#" + id + "-complete-table").find("tr.myHeaderStyle > td").each(function () {
            if ($(this).css("display") !== "none") {
                count++;
            }
        });
        return count;
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
    function SetEditMode(id, x, _this) {
        var found = false;
        for (var i = 0; i < arrEditModeCustomTables.length; i++) {
            if (arrEditModeCustomTables[i][0] == id) {
                arrEditModeCustomTables[i][1] = x;
                arrEditModeCustomTables[i][2] = _this;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = x;
            arrInner[2] = _this;

            arrEditModeCustomTables[arrEditModeCustomTables.length] = arrInner;
        }
    }

    function UpdateRecord(id, _this) {
        SetEditMode(id, false, "");
        var recordVals = new Array();
        var error = false;
        $("#" + id + "-editRow").find(".data-td").each(function (index) {
            if (!error) {
                var $input = $(this).find("input");
                var $div = $(this).find(".td-columnName-edit");
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
            loadingPopup.Message("Updating Record. Please Wait...");
            openWSE.AjaxCall("WebServices/dbImportservice.asmx/UpdateRecord", JSON.stringify({ "id": id, "recordVals": recordVals, "rowData": GetRowData(_this) }), null, function (data) {
                loadingPopup.RemoveMessage();
                if (data.d[0] == "Success") {
                    GetRecords(id, false);
                }
                else {
                    openWSE.AlertWindow(data.d[1]);
                }
            });
        }
        else {
            openWSE.AlertWindow("One or more records cannot be empty. Please check your values and try again.");
        }
    }
    function UpdateRecordKeyPress(event, id, _this) {
        try {
            if (event.which == 13) {
                UpdateRecord(id, _this);
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                UpdateRecord(id, _this);
                return false;
            }
            delete evt;
        }
    }

    function CancelRecord(id) {
        SetEditMode(id, false, "");
        GetRecords(id, false);
    }
    function DeleteRecord(id, _this) {
        openWSE.ConfirmWindow("Are you sure you want to delete this row?",
            function () {
                loadingPopup.Message("Deleting Record. Please Wait...");
                openWSE.AjaxCall("WebServices/dbImportservice.asmx/DeleteRecord", JSON.stringify({ "id": id, "rowData": GetRowData(_this) }), null, function (data) {
                    loadingPopup.RemoveMessage();
                    if (data.d[0] == "Success") {
                        GetRecords(id, false);
                    }
                    else {
                        openWSE.AlertWindow(data.d[1]);
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
        arr[0] = "";
        arr[1] = "";
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
            MenuClick(id);

            // Get Chart Data
            loadingPopup.Message("Loading Chart Data...");
            openWSE.AjaxCall("WebServices/dbImportservice.asmx/SetChartView", JSON.stringify({ "id": id, "view": "true" }), null, null, null, function (data) {
                BuildChartData(id, chartType);

                $("#pnl_" + id + "_tableView").hide();
                $("#pnl_" + id + "_chartView").show();

                loadingPopup.RemoveMessage();
            });
        }
    }
    function BuildChartData(id, chartType) {
        var errorOccurred = false;

        try {
            if ($("#pnl_" + id + "_chartView").length == 1) {
                openWSE.GetScriptFunction("//www.google.com/jsapi", function () {
                    var _innerHtml = "<a href='#' onclick=\"dbImport.ViewTableData('" + id + "');return false;\" class=\"customtables-chart-topbtns float-left pad-all\"><span class=\"pg-prev-btn float-left pad-right-sml\" style=\"margin-top: -3px;\"></span>Back</a>";
                    _innerHtml += "<a href='#' class='customtables-chart-topbtns float-right pad-all img-refresh' title='Refresh Data' onclick=\"dbImport.Refresh('" + id + "');return false;\"></a>";
                    _innerHtml += "<div class='google-chart-holder'></div>";

                    $("#pnl_" + id + "_chartView").html(_innerHtml);

                    var chartTitle = $.trim($("#hf_" + id + "_chartTitle").val());

                    var chartWidth = $(window).width();
                    var chartHeight = $(window).height();

                    var $mainAppDiv = $("#" + id + "-load").closest(".app-body");
                    if ($mainAppDiv.length > 0) {
                        chartWidth = $mainAppDiv.width();
                        chartHeight = $mainAppDiv.height() - ($("#pnl_" + id + "_chartView").find(".customtables-chart-topbtns").outerHeight() + 20);
                    }
                    else {
                        chartHeight = chartHeight - ($("#top_bar").outerHeight() + $("#pnl_" + id + "_chartView").find(".customtables-chart-topbtns").outerHeight() + 20);
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
                    if (openWSE.TableFormulas.IsMobileViewMode(id)) {
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
                            if (openWSE.TableFormulas.IsMobileViewMode(id)) {
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
                                    x = x.replace(/\$/g, "").replace(/,/g, "").replace(/ /g, "");
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
        openWSE.AjaxCall("WebServices/dbImportservice.asmx/SetChartView", JSON.stringify({ "id": id, "view": "false" }), null, null, null, function (data) {
            loadingPopup.RemoveMessage();

            $("#pnl_" + id + "_chartView").hide();
            $("#pnl_" + id + "_chartView").html("");
            $("#pnl_" + id + "_tableView").show();

            GetRecords(id, false);
        });
    }
    function ResizeChartData() {
        $(".dbImport-table-timermarker").each(function () {
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
            cookieFunctions.get(id + "-fontsize", function (fontSize) {
                if ((fontSize != null) && (fontSize != "")) {
                    $("." + id + "-record-font").css("font-size", fontSize);
                    $("#data-holder-" + id).css("font-size", fontSize);
                    $("#font-size-selector-" + id).val(fontSize);
                }
            });
        }
        catch (evt) { }
    }
    function ChangeFont(id, _this) {
        var currfontsizeIndex = 0;
        var currfontsize = $("#font-size-selector-" + id).val();
        for (var i = 0; i < $("#font-size-selector-" + id).find("option").length; i++) {
            if (currfontsize === $("#font-size-selector-" + id).find("option").eq(i).val()) {
                currfontsizeIndex = i;
                break;
            }
        }

        if ($(_this).hasClass("font-smaller-img")) {
            currfontsizeIndex--;
        }
        else {
            currfontsizeIndex++;
        }

        if (currfontsizeIndex < $("#font-size-selector-" + id).find("option").length && currfontsizeIndex >= 0) {
            var fontsize = "";
            for (var i = 0; i < $("#font-size-selector-" + id).find("option").length; i++) {
                if (i === currfontsizeIndex) {
                    fontsize = $("#font-size-selector-" + id).find("option").eq(i).val();
                    break;
                }
            }

            if (fontsize != "") {
                $("#font-size-selector-" + id).val(fontsize);
                $("." + id + "-record-font").css("font-size", fontsize);
                $("#data-holder-" + id).css("font-size", fontsize);
                $("#data-holder-" + id).find("input").css("font-size", fontsize);
                cookieFunctions.set(id + "-fontsize", fontsize, "30");
            }
        }
    }
    function LoadDefaultValues(id) {
        try {
            var $addRow = $("#" + id + "-addRow");

            if ($addRow.length > 0) {
                var customizationsStr = unescape($.trim($("#hf_" + id + "_customizations").val()));
                var customizationJson = $.parseJSON(customizationsStr);
                var defaultValues = new Array();

                for (var i = 0; i < customizationJson.length; i++) {
                    var name = customizationJson[i].customizeName;
                    var value = customizationJson[i].customizeValue.replace(/\+/g, " ");
                    if (value != "" && name == "DefaultValues") {
                        defaultValues = $.parseJSON(value);
                        break;
                    }
                }

                for (var i = 0; i < defaultValues.length; i++) {
                    var $rowInput = $addRow.find(".data-td[data-columnname='" + defaultValues[i].name + "']");
                    if ($rowInput.length > 0) {
                        if ($rowInput.find("input[type='checkbox']").length > 0) {
                            $rowInput.find("input").prop("checked", defaultValues[i].value);
                        }
                        else if ($.trim($rowInput.find("input").val()) == "") {
                            $rowInput.find("input").val(defaultValues[i].value);
                        }
                    }
                }
            }
        }
        catch (evt) { }
    }
    function CanShowRowCount(id) {
        var customizationsStr = unescape($.trim($("#hf_" + id + "_customizations").val()));
        var customizationJson = $.parseJSON(customizationsStr);

        for (var i = 0; i < customizationJson.length; i++) {
            var name = customizationJson[i].customizeName;
            var value = customizationJson[i].customizeValue.replace(/\+/g, " ");
            if (name == "ShowRowCounts" && openWSE.ConvertBitToBoolean(value)) {
                return true;
            }
        }

        return false;
    }


    function SetFocus(id, editMode, indexOfEle) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        if ($("#" + id + "-" + rowType).find("input").length > 0) {
            if ($("#" + id + "-" + rowType).find("input").eq(indexOfEle).prop("disabled")) {
                $("#" + id + "-" + rowType).find("input").eq($("#" + id + "-" + rowType).find("input").length - 1).focus();
            }
            else {
                $("#" + id + "-" + rowType).find("input").eq(indexOfEle).focus();
            }
        }
    }
    function SaveInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }
        arrAddValuesCustomTables = new Array();
        $("#" + id + "-" + rowType).find("input").each(function (index) {
            if ($(this).attr("type") == "checkbox") {
                arrAddValuesCustomTables[index] = $(this).prop("checked");
            }
            else {
                arrAddValuesCustomTables[index] = $(this).val();
            }
        });
    }
    function RestoreInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        $("#" + id + "-" + rowType).find("input").each(function (index) {
            if ($(this).attr("type") == "checkbox") {
                $(this).prop("checked", false);
                if (arrAddValuesCustomTables[index]) {
                    $(this).prop("checked", true);
                }
            }
            else {
                $(this).val(arrAddValuesCustomTables[index]);
            }
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

    function GetPageSize(id) {
        var pageSize = "20";
        for (var i = 0; i < arrPageSizeCustomTables.length; i++) {
            if (arrPageSizeCustomTables[i][0] == id) {
                pageSize = arrPageSizeCustomTables[i][1];
            }
        }

        return pageSize;
    }
    function SetPageSize(id, _this) {
        var found = false;
        for (var i = 0; i < arrPageSizeCustomTables.length; i++) {
            if (arrPageSizeCustomTables[i][0] == id) {
                arrPageSizeCustomTables[i][1] = $(_this).val();
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = $(_this).val();

            arrPageSizeCustomTables[arrPageSizeCustomTables.length] = arrInner;
        }
    }

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
        ViewSummary_MenuClick: ViewSummary_MenuClick,
        ChangeFont: ChangeFont,
        ChangeViewMode: ChangeViewMode,
        UpdateColumnsToShow: UpdateColumnsToShow,
        ResizeChartData: ResizeChartData,
        AutoCompleteDropdownClick: AutoCompleteDropdownClick,
        SetPageSize: SetPageSize
    }
}();
