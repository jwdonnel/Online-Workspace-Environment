/* Database Import Functions
----------------------------------*/
function pageLoad() {
    $(".dbImport-table-timermarker").each(function () {
        var id = $(this).attr("id").replace("-load", "");
        var innerHolder = $.trim($("#data-holder-" + id).html());
        if (innerHolder == "") {
            dbImport.Load(id);
        }
    });
}

var dbImport = function () {
    var arrSortColdbImport = new Array();
    var arrEditModedbImport = new Array();
    var arrAddValuesdbImport = new Array();
    var timerIsRunning = false;
    var timerIntveral = 1000 * 60;
    var timerdbImport;

    function Load(id) {
        InitJquery(id);
        var innerHolder = $.trim($("#data-holder-" + id).html());

        $(document.body).on("click", "#app-" + id + " .exit-button-app, #app-" + id + "-min-bar .exit-button-app-min", function () {
            cookie.del(id + "-fontsize");
        });

        if (innerHolder == "") {
            GetRecords(id, false);
        }
    }

    function Timer() {
        timerIsRunning = true;
        timerdbImport = setTimeout(function () {
            $(".dbImport-table-timermarker").each(function (index) {
                var id = $(this).attr("id").replace("-load", "");
                if (id != "") {
                    GetRecords(id, true);
                }
            });
        }, timerIntveral);
    }

    function GetRecords(id, fromTimer) {
        if (!fromTimer) {
            openWSE.LoadingMessage1("Loading Records. Please Wait...");
        }

        var arr = GetSortDirCol(id);
        SaveInputAddValues(id, false);

        $.ajax({
            url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/GetRecords",
            data: "{'id': '" + id + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "'}",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    var dataArray = data.d[0];
                    if (dataArray.length > 0) {
                        var allowEdit = data.d[1] == "true";

                        $("#data-holder-" + id).html(BuildTable(dataArray[0], dataArray[1], id, dataArray[2], false, allowEdit));

                        LoadFontSizes(id);

                        AutoCompleteAddEdit(id, false);
                        RestoreInputAddValues(id, false);

                        if ($("#data-holder-" + id).find(".td-columnValue-add").length > 0) {
                            $("#data-holder-" + id).find(".td-columnValue-add").each(function () {
                                var val = $.trim($(this).html());
                                if (val != "") {
                                    $(this).parent().find("input[type='text']").val(val);
                                }
                            });
                        }
                    }
                    else {
                        $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
                    }

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
                }
            },
            error: function (err) {
                $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
            }
        });
    }

    function BuildTable(innerHeader, innerAddRow, id, arr, editMode, allowEdit) {
        var xHeader = "<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">";
        xHeader += "<tbody><tr class=\"myHeaderStyle\">";
        xHeader += "<td style=\"width: 40px;\"></td>";
        xHeader += innerHeader;

        if (allowEdit) {
            xHeader += "<td class=\"edit-buttons\" style=\"width: 70px;\"></td>";
        }

        xHeader += "</tr>";

        var xAddRow = "";
        if (allowEdit) {
            xAddRow += "<tr id=\"" + id + "-addRow\" class=\"GridNormalRow myItemStyle\">";
            xAddRow += "<td class=\"GridViewNumRow border-bottom\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
            xAddRow += innerAddRow;
            if (!editMode) {
                xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'><a href=\"#Add\" class=\"td-add-btn\" onclick=\"dbImport.AddRecord('" + id + "');return false;\" title=\"Add Row\"></a></td>";
            }
            else {
                xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px; height: 27px;'></td>";
            }
            xAddRow += "</tr>";
        }

        var hasEdit = false;
        var xItem = "";
        for (var i = 0; i < arr.length; i++) {
            if (!editMode) {
                xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
            }
            else {
                if (openWSE.ConvertBitToBoolean(arr[i][1])) {
                    xItem += "<tr id=\"" + id + "-editRow\" class=\"GridNormalRow myItemStyle item-row\">";
                    hasEdit = true;
                }
                else {
                    xItem += "<tr class=\"GridNormalRow myItemStyle item-row\">";
                }
            }
            xItem += "<td class=\"GridViewNumRow border-bottom\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
            for (var j = 0; j < arr[i][0].length; j++) {
                xItem += "<td class=\"border-right border-bottom\" data-type=\"" + arr[i][0][j][1].toString() + "\">" + arr[i][0][j][0].toString() + "</td>";
            }

            if (allowEdit) {
                var columnID = arr[i][0].toString();
                var editBtn = "";
                var delBtn = "";

                if (!editMode) {
                    editBtn = "<a href=\"#Edit\" class=\"margin-right td-edit-btn\" onclick=\"dbImport.EditRecord('" + id + "', this);return false;\" title=\"Edit Row\"></a>";
                    delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"dbImport.DeleteRecord('" + id + "', this);return false;\" title=\"Delete Row\"></a>";
                }
                else {
                    if (openWSE.ConvertBitToBoolean(arr[i][1])) {
                        editBtn = "<a href=\"#Update\" class=\"margin-right td-update-btn\" onclick=\"dbImport.UpdateRecord('" + id + "', this);return false;\" title=\"Update Row\"></a>";
                        delBtn = "<a href=\"#Cancel\" class=\"td-cancel-btn\" onclick=\"dbImport.CancelRecord('" + id + "');return false;\" title=\"Cancel Edit\"></a>";
                    }
                }

                xItem += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'>" + editBtn + delBtn + "</td>";
            }

            xItem += "</tr>";
        }

        if ((allowEdit) && (editMode) && (!hasEdit)) {
            SetEditMode(id, false, "");
            openWSE.AlertWindow("The was an issue trying to find item in database. Please note that some items may not be found in the database due to certain characters within a column.");
            BuildTable(innerHeader, innerAddRow, id, arr, false, allowEdit);
        }
        else {
            if (xItem == "") {
                return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + "</tbody></table><div class='emptyGridView'>No records found</div></div>";
            }
            return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + xItem + "</tbody></table></div>";
        }
    }

    function Refresh(id) {
        openWSE.LoadingMessage1("Loading Records. Please Wait...");

        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }

    function RecordstoSelect(id) {
        openWSE.LoadingMessage1("Loading Records. Please Wait...");
        var arr = GetEditMode(id);
        if (arr[0] == true) {
            EditRecord(id, arr[1]);
        }
        else {
            GetRecords(id, false);
        }
    }

    function InitJquery(id) {
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
            $("#app-" + id).on("resizestop", function (event, ui) {
                ReBuildChartData(id);
            });

            $(document.body).on("click", "#app-" + id + " .maximize-button-app", function () {
                ReBuildChartData(id);
            });

            $(document.body).on("dblclick", "#app-" + id + " .app-head-dblclick", function () {
                ReBuildChartData(id);
            });
        }
    }

    function AutoCompleteAddEdit(id, editMode) {
        var rowType = "add";
        if (editMode) {
            rowType = "edit";
        }
        $("#" + id + "-" + rowType + "Row").find(".border-right").each(function (index) {
            var $input = $(this).find("input");
            var columnName = $(this).find(".td-columnName-" + rowType).html();
            var columnType = $(this).find("input").attr("data-type");

            if ($.trim(columnType) == "datetime") {
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
                            url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetdbImportData",
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

    function OnSortClick(_this, col, id) {
        var $_this = $(_this);
        if ($_this.hasClass("asc")) {
            sortDir = "DESC";
        }
        else {
            sortDir = "ASC";
        }

        var found = false;
        for (var i = 0; i < arrSortColdbImport.length; i++) {
            if (arrSortColdbImport[i][0] == id) {
                arrSortColdbImport[i][1] = sortDir;
                arrSortColdbImport[i][2] = col;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = sortDir;
            arrInner[2] = col;
            arrSortColdbImport[arrSortColdbImport.length] = arrInner;
        }

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
            url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/ExportToExcel",
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

    function EditRecord(id, _this) {
        openWSE.LoadingMessage1("Loading Records. Please Wait...");
        var arr = GetSortDirCol(id);
        var rowData = new Array();
        $(_this).closest(".GridNormalRow").find(".border-right").each(function (index) {
            rowData[index] = $(this).html();
        });

        $.ajax({
            url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/EditRecord",
            data: JSON.stringify({ "id": id, "rowData": rowData, "search": $("#tb_search_" + id).val(), "recordstopull": $("#RecordstoSelect_" + id).val(), "sortCol": arr[1], "sortDir": arr[0] }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.d.length > 0) {
                    $("#data-holder-" + id).html(BuildTable(data.d[0], data.d[1], id, data.d[2], true, true));
                    LoadFontSizes(id);
                    SetEditMode(id, true, _this);

                    AutoCompleteAddEdit(id, true);
                    SetFocus(id, true);

                    timerIsRunning = false;
                    clearTimeout(timerdbImport);

                    if ($("#data-holder-" + id).find(".td-columnValue-edit").length > 0) {
                        $("#data-holder-" + id).find(".td-columnValue-edit").each(function () {
                            var val = $.trim($(this).html());
                            $(this).parent().find("input[type='text']").val(val);
                        });
                    }
                }
                else {
                    openWSE.AlertWindow("Error pulling records.");
                }

                openWSE.RemoveUpdateModal();
            },
            error: function (err) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occured while calling the server.");
            }
        });
    }
    function DeleteRecord(id, _this) {
        openWSE.ConfirmWindow("Are you sure you want to delete this row?",
            function () {
                var rowData = new Array();
                $(_this).closest(".GridNormalRow").find(".border-right").each(function (index) {
                    rowData[index] = $(this).html();
                });

                openWSE.LoadingMessage1("Deleting Record. Please Wait...");
                $.ajax({
                    url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/DeleteRecord",
                    data: JSON.stringify({ "id": id, "rowData": rowData }),
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
    function UpdateRecord(id, _this) {
        SetEditMode(id, false, "");
        var recordVals = new Array();
        var error = false;
        $("#" + id + "-editRow").find(".border-right").each(function (index) {
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

        var rowData = new Array();
        $(_this).closest(".GridNormalRow").find(".border-right").each(function (index) {
            rowData[index] = $(this).find(".td-columnValue-edit").html();
        });

        if (!error) {
            openWSE.LoadingMessage1("Updating Records. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/UpdateRecord",
                data: JSON.stringify({ "id": id, "recordVals": recordVals, "rowData": rowData }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if ($(".update-element-" + id).length > 0) {
                        $(".update-element-" + id).remove();
                    }

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

    function CancelRecord(id) {
        SetEditMode(id, false, "");
        GetRecords(id, false);
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
    function AddRecord(id) {
        var recordVals = new Array();
        var error = false;
        $("#" + id + "-addRow").find(".border-right").each(function (index) {
            if (!error) {
                var $input = $(this).find("input");
                var $div = $(this).find(".td-columnName-add");
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
            openWSE.LoadingMessage1("Adding Record. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/AddRecord",
                data: JSON.stringify({ "id": id, "recordVals": recordVals }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    openWSE.RemoveUpdateModal();
                    if (data.d[0] == "Success") {
                        $("#" + id + "-addRow").find(".border-right").each(function (index) {
                            $(this).find("input").val("");
                        });

                        SaveInputAddValues(id, false);
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

    function SetFocus(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        var count = 0;
        $("#" + id + "-" + rowType).find("input").each(function (index) {
            if (count == 0) {
                $(this).focus();
                count++;
            }
        });
    }

    function SaveInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }
        arrAddValuesdbImport = new Array();
        $("#" + id + "-" + rowType).find("input").each(function (index) {
            arrAddValuesdbImport[index] = $(this).val();
        });
    }

    function RestoreInputAddValues(id, editMode) {
        var rowType = "addRow";
        if (editMode) {
            rowType = "editRow";
        }

        $("#" + id + "-" + rowType).find("input[type='text']").each(function (index) {
            $(this).val(arrAddValuesdbImport[index]);
        });
        arrAddValuesdbImport = new Array();
    }

    function GetSortDirCol(id) {
        var arr = new Array();
        arr[0] = "";
        arr[1] = "";
        for (var i = 0; i < arrSortColdbImport.length; i++) {
            if (arrSortColdbImport[i][0] == id) {
                arr[0] = arrSortColdbImport[i][1];
                arr[1] = arrSortColdbImport[i][2];
            }
        }

        return arr;
    }

    function GetEditMode(id) {
        var arr = new Array();
        arr[0] = false
        for (var i = 0; i < arrEditModedbImport.length; i++) {
            if (arrEditModedbImport[i][0] == id) {
                if (arrEditModedbImport[i][1] == true) {
                    arr[0] = true;
                    arr[1] = arrEditModedbImport[i][2];
                }
            }
        }

        return arr;
    }

    function SetEditMode(id, x, cid) {
        var found = false;
        for (var i = 0; i < arrEditModedbImport.length; i++) {
            if (arrEditModedbImport[i][0] == id) {
                arrEditModedbImport[i][1] = x;
                arrEditModedbImport[i][2] = cid;
                found = true;
                break;
            }
        }
        if (!found) {
            var arrInner = new Array();
            arrInner[0] = id;
            arrInner[1] = x;
            arrInner[2] = cid;

            arrEditModedbImport[arrEditModedbImport.length] = arrInner;
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

    $(document.body).on("change", ".custom-table-font-selector", function () {
        var $select = $(this);
        var fontsize = $select.val();
        var id = $select.attr("id").replace("font-size-selector-", "");
        $("." + id + "-record-font").css("font-size", fontsize);
        $("#data-holder-" + id).css("font-size", fontsize);
        $("#data-holder-" + id).find("input").css("font-size", fontsize);
        cookie.set(id + "-fontsize", fontsize, "30");
    });

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

    function Search(id) {
        SetEditMode(id, false, "");
        GetRecords(id, false);
    }

    function ViewChart(id, _this) {
        var chartType = $(_this).find("input[type='hidden']").val();

        if (chartType != "" && chartType != null) {
            // Get Chart Data
            openWSE.LoadingMessage1("Loading Chart Data...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/SetChartView",
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
                $.getScript("//www.google.com/jsapi").done(function (script, textStatus) {
                    var _innerHtml = "<a href='#' onclick=\"dbImport.ViewTableData('" + id + "');return false;\"><span class='pg-prev-btn float-left pad-right-sml' style='padding-top: 0px; margin-top: -3px;'></span>Back to Table</a>";
                    _innerHtml += "<a href='#' class='float-right margin-right img-refresh' title='Refresh Data' onclick=\"dbImport.Refresh('" + id + "');return false;\"></a>";
                    _innerHtml += "</div><div class='google-chart-holder'>";

                    $("#pnl_" + id + "_chartView").html("<div class='pad-all'>" + _innerHtml + "</div>");

                    var chartTitle = $.trim($("#hf_" + id + "_chartTitle").val());

                    var chartWidth = $(window).width();
                    var chartHeight = $(window).height();

                    var $mainAppDiv = $("#" + id + "-load").closest(".app-body");
                    if ($mainAppDiv.length > 0) {
                        chartWidth = $mainAppDiv.width();
                        chartHeight = $mainAppDiv.height();
                    }

                    var docEle = $("#pnl_" + id + "_chartView").find(".google-chart-holder")[0];
                    var options = {
                        title: chartTitle,
                        width: chartWidth - 30,
                        height: chartHeight - 65
                    };

                    var chart = null;
                    var dataCollection = new Array();

                    var columnNames = new Array();
                    $("#" + id + "-complete-table").find(".myHeaderStyle").find(".td-sort-click").each(function () {
                        columnNames.push($.trim($(this).html()));
                    });

                    dataCollection.push(columnNames);

                    $("#" + id + "-complete-table").find(".item-row").each(function () {
                        var $itemRow = $(this);
                        var rowData = new Array();

                        $itemRow.find("td").each(function () {
                            if (!$(this).hasClass("GridViewNumRow") && !$(this).hasClass("edit-buttons")) {
                                var x = $.trim($(this).html());
                                if ($(this).attr("data-type") == "int32") {
                                    x = parseInt(x);
                                }
                                else if ($(this).attr("data-type") == "decimal") {
                                    x = parseFloat(x);
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
            url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/SetChartView",
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

    return {
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
        UpdateRecordKeyPress: UpdateRecordKeyPress
    }
}();