/* Custom Table Functions
----------------------------------*/
var arrSortColCustomTables = new Array();
var arrEditModeCustomTables = new Array();
var arrAddValuesCustomTables = new Array();
var arrDateSelectedCustomTables = new Array();
var timerIsRunning_CustomTables = false;
var timerIntveral_CustomTables = 1000 * 60;
var timerCustomTables;

function Load_CustomTables(id) {
    InitJquery_CustomTables(id);
    var innerHolder = $.trim($("#data-holder-" + id).html());

    $(document.body).on("click", "#app-" + id + " .exit-button-app, #app-" + id + "-min-bar .exit-button-app-min", function () {
        cookie.del(id + "-fontsize");
    });

    if (innerHolder == "") {
        GetRecords_CustomTables(id, false);
    }
}

function Timer_CustomTables() {
    timerIsRunning_CustomTables = true;
    timerCustomTables = setTimeout(function () {
        $(".custom-table-timermarker").each(function (index) {
            var id = $(this).attr("id").replace("-load", "");
            if (id != "") {
                GetRecords_CustomTables(id, true);
            }
        });
    }, timerIntveral_CustomTables);
}

function LoadingMessage_CustomTables(id, message) {
    var x = "<div class='update-element-" + id + "'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#" + id + "-load").append(x);
    $(".update-element-" + id).show();
}

function GetRecords_CustomTables(id, fromTimer) {
    if (!fromTimer) {
        LoadingMessage_CustomTables(id, "Loading Records. Please Wait...");
    }

    var arr = GetSortDirCol_CustomTables(id);
    SaveInputAddValues_CustomTables(id, false);

    var dateSelected = "";
    if ($("#sidebar-items-" + id).length > 0) {
        dateSelected = GetDateSelected_CustomTables(id);
        GetSidebar_CustomTables(id, dateSelected);
    }

    $.ajax({
        url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/GetRecords",
        data: "{'id': '" + id + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "','date': '" + dateSelected + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d.length > 0) {
                $("#data-holder-" + id).html(BuildTable_CustomTables(data.d[0], data.d[1], id, data.d[2], false));
                LoadFontSizes_CustomTables(id);

                AutoCompleteAddEdit_CustomTables(id, false);
                RestoreInputAddValues_CustomTables(id, false);
            }
            else {
                $("#data-holder-" + id).html("<h4>Error pulling records! Please try again.</h4>");
            }

            if ($(".update-element-" + id).length > 0) {
                $(".update-element-" + id).remove();
            }

            if (!timerIsRunning_CustomTables) {
                Timer_CustomTables();
            }
        }
    });
}

function GetSidebar_CustomTables(id, dateSelected) {
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
                x += "<div id=\"expand_" + id + "_All\" class=\"tsdiv" + selectedClass + "\" onclick=\"MonthSelect_CustomTables('" + id + "', 'all')\"><div class=\"pad-all-sml\"><h4>All Dates</h4></div></div>";
                x += "<div class=\"sidebar-divider-no-margin\"></div>";
                for (var i = 0; i < data.d.length; i++) {
                    selectedClass = "";
                    if (dateSelected == data.d[i][0]) {
                        selectedClass = " tsactive";
                    }
                    x += "<div id=\"expand_" + id + "_" + data.d[i][0] + "\" class=\"tsdiv" + selectedClass + "\" onclick=\"MonthSelect_CustomTables('" + id + "', '" + data.d[i][0] + "')\"><div class=\"pad-all-sml\">" + data.d[i][1] + "</div></div>";
                    x += "<div class=\"sidebar-divider-no-margin\"></div>";
                }

                $("#month-selector-" + id).html(x);

                if ($("#" + id + "-load").find(".tsactive").length == 0) {
                    $("#expand_" + id + "_All").addClass("tsactive");
                }
            }
            else {
                $("#month-selector-" + id).html("<h4>No Dates Available</h4>");
            }

            if ($(".update-element-" + id).length > 0) {
                $(".update-element-" + id).remove();
            }
        }
    });
}

function BuildTable_CustomTables(innerHeader, innerAddRow, id, arr, editMode) {
    var xHeader = "<table cellpadding=\"5\" cellspacing=\"0\" style=\"width: 100%;\">";
    xHeader += "<tbody><tr class=\"myHeaderStyle\">";
    xHeader += "<td style=\"width: 40px;\"></td>";
    xHeader += innerHeader;
    xHeader += "<td class=\"edit-buttons\" style=\"width: 70px;\"></td></tr>";

    var xAddRow = "<tr id=\"" + id + "-addRow\" class=\"GridNormalRow myItemStyle\">";
    xAddRow += "<td class=\"GridViewNumRow\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
    xAddRow += innerAddRow;
    if (!editMode) {
        xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'><a href=\"#Add\" class=\"td-add-btn\" onclick=\"AddRecord_CustomTables('" + id + "');return false;\" title=\"Add Row\"></a></td>";
    }
    else {
        xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px; height: 27px;'></td>";
    }
    xAddRow += "</tr>";

    var xItem = "";
    for (var i = 0; i < arr.length; i++) {
        if (!editMode) {
            xItem += "<tr class=\"GridNormalRow myItemStyle\">";
        }
        else {
            if (openWSE.ConvertBitToBoolean(arr[i][2])) {
                xItem += "<tr id=\"" + id + "-editRow\" class=\"GridNormalRow myItemStyle\">";
            }
            else {
                xItem += "<tr class=\"GridNormalRow myItemStyle\">";
            }
        }
        xItem += "<td class=\"GridViewNumRow\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
        for (var j = 0; j < arr[i][1].length; j++) {
            xItem += "<td class=\"border-right border-bottom\">" + arr[i][1][j].toString() + "</td>";
        }

        var columnID = arr[i][0].toString();
        var editBtn = "";
        var delBtn = "";
        if (!editMode) {
            editBtn = "<a href=\"#Edit\" class=\"margin-right td-edit-btn\" onclick=\"EditRecord_CustomTables('" + id + "', '" + columnID + "');return false;\" title=\"Edit Row\"></a>";
            delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"DeleteRecord_CustomTables('" + id + "', '" + columnID + "');return false;\" title=\"Delete Row\"></a>";
        }
        else {
            if (openWSE.ConvertBitToBoolean(arr[i][2])) {
                editBtn = "<a href=\"#Update\" class=\"margin-right td-update-btn\" onclick=\"UpdateRecord_CustomTables('" + id + "', '" + columnID + "');return false;\" title=\"Update Row\"></a>";
                delBtn = "<a href=\"#Cancel\" class=\"td-cancel-btn\" onclick=\"CancelRecord_CustomTables('" + id + "');return false;\" title=\"Cancel Edit\"></a>";
            }
        }
        xItem += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'>" + editBtn + delBtn + "</td>";
        xItem += "</tr>";
    }

    return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + xItem + "</tbody></table></div>";
}

function MonthSelect_CustomTables(id, _date) {
    if (_date == "all") {
        _date = "";
    }
    SetDateSelected_CustomTables(id, _date);
    var arr = GetEditMode_CustomTables(id);
    if (arr[0] == true) {
        EditRecord_CustomTables(id, arr[1]);
    }
    else {
        GetRecords_CustomTables(id, false);
    }
}

function Refresh_CustomTables(id) {
    LoadingMessage_CustomTables(id, "Loading Records. Please Wait...");
    var arr = GetEditMode_CustomTables(id);
    if (arr[0] == true) {
        EditRecord_CustomTables(id, arr[1]);
    }
    else {
        GetRecords_CustomTables(id, false);
    }
}

function RecordstoSelect_CustomTables(id) {
    LoadingMessage_CustomTables(id, "Loading Records. Please Wait...");
    var arr = GetEditMode_CustomTables(id);
    if (arr[0] == true) {
        EditRecord_CustomTables(id, arr[1]);
    }
    else {
        GetRecords_CustomTables(id, false);
    }
}

function InitJquery_CustomTables(id) {
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
}

function AutoCompleteAddEdit_CustomTables(id, editMode) {
    var rowType = "add";
    if (editMode) {
        rowType = "edit";
    }
    $("#" + id + "-" + rowType + "Row").find(".border-right").each(function (index) {
        var $input = $(this).find("input");
        var columnName = $(this).find(".td-columnName-" + rowType).html();
        var columnType = $(this).find(".td-columnType-" + rowType).html();

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

function OnSortClick_CustomTables(_this, col, id) {
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

    var arr = GetEditMode_CustomTables(id);
    if (arr[0] == true) {
        EditRecord_CustomTables(id, arr[1]);
    }
    else {
        GetRecords_CustomTables(id, false);
    }
}

function ExportToExcelAll_CustomTables(id) {
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

function EditRecord_CustomTables(id, cid) {
    LoadingMessage_CustomTables(id, "Loading Records. Please Wait...");
    var arr = GetSortDirCol_CustomTables(id);

    var dateSelected = "";
    if ($("#sidebar-items-" + id).length > 0) {
        dateSelected = GetDateSelected_CustomTables(id);
        GetSidebar_CustomTables(id, dateSelected);
    }

    $.ajax({
        url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/EditRecord",
        data: "{'id': '" + id + "','cid': '" + cid + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "','date': '" + dateSelected + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d.length > 0) {
                $("#data-holder-" + id).html(BuildTable_CustomTables(data.d[0], data.d[1], id, data.d[2], true));
                LoadFontSizes_CustomTables(id);
                SetEditMode_CustomTables(id, true, cid);

                AutoCompleteAddEdit_CustomTables(id, true);
                SetFocus_CustomTables(id, true);

                timerIsRunning_CustomTables = false;
                clearTimeout(timerCustomTables);
            }
            else {
                openWSE.AlertWindow("Error pulling records.");
            }

            if ($(".update-element-" + id).length > 0) {
                $(".update-element-" + id).remove();
            }
        }
    });
}

function DeleteRecord_CustomTables(id, cid) {
    openWSE.ConfirmWindow("Are you sure you want to delete this row?",
        function () {
            LoadingMessage_CustomTables(id, "Deleting Record. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/DeleteRecord",
                data: "{'id': '" + id + "','cid': '" + cid + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if ($(".update-element-" + id).length > 0) {
                        $(".update-element-" + id).remove();
                    }

                    if (data.d[0] == "Success") {
                        GetRecords_CustomTables(id, false);
                    }
                    else {
                        openWSE.AlertWindow(data.d[1]);
                    }
                }
            });
        }, null);
}

function UpdateRecordKeyPress_CustomTables(event, id, cid) {
    try {
        if (event.which == 13) {
            UpdateRecord_CustomTables(id, cid);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            UpdateRecord_CustomTables(id, cid);
            return false;
        }
        delete evt;
    }
}

function UpdateRecord_CustomTables(id, cid) {
    SetEditMode_CustomTables(id, false, "");
    var recordVals = new Array();
    var error = false;
    $("#" + id + "-editRow").find(".border-right").each(function (index) {
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
        LoadingMessage_CustomTables(id, "Updating Record. Please Wait...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/UpdateRecord",
            data: JSON.stringify({ "id": id, "recordVals": recordVals, "cid": cid }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if ($(".update-element-" + id).length > 0) {
                    $(".update-element-" + id).remove();
                }

                if (data.d[0] == "Success") {
                    GetRecords_CustomTables(id, false);
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

function CancelRecord_CustomTables(id) {
    SetEditMode_CustomTables(id, false, "");
    GetRecords_CustomTables(id, false);
}

function AddRecordKeyPress_CustomTables(event, id) {
    try {
        if (event.which == 13) {
            AddRecord_CustomTables(id);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddRecord_CustomTables(id);
            return false;
        }
        delete evt;
    }
}

function AddRecord_CustomTables(id) {
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
        LoadingMessage_CustomTables(id, "Adding Record. Please Wait...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableService.asmx/AddRecord",
            data: JSON.stringify({ "id": id, "recordVals": recordVals }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if ($(".update-element-" + id).length > 0) {
                    $(".update-element-" + id).remove();
                }

                if (data.d[0] == "Success") {
                    $("#" + id + "-addRow").find(".border-right").each(function (index) {
                        $(this).find("input").val("");
                    });

                    SaveInputAddValues_CustomTables(id, false);
                    GetRecords_CustomTables(id, false);
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

function SetFocus_CustomTables(id, editMode) {
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

function SaveInputAddValues_CustomTables(id, editMode) {
    var rowType = "addRow";
    if (editMode) {
        rowType = "editRow";
    }
    arrAddValuesCustomTables = new Array();
    $("#" + id + "-" + rowType).find("input").each(function (index) {
        arrAddValuesCustomTables[index] = $(this).val();
    });
}

function RestoreInputAddValues_CustomTables(id, editMode) {
    var rowType = "addRow";
    if (editMode) {
        rowType = "editRow";
    }

    $("#" + id + "-" + rowType).find("input").each(function (index) {
        $(this).val(arrAddValuesCustomTables[index]);
    });
    arrAddValuesCustomTables = new Array();
}

function GetSortDirCol_CustomTables(id) {
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

function GetDateSelected_CustomTables(id) {
    var dateSelected = "";
    for (var i = 0; i < arrDateSelectedCustomTables.length; i++) {
        if (arrDateSelectedCustomTables[i][0] == id) {
            dateSelected = arrDateSelectedCustomTables[i][1];
        }
    }

    return dateSelected;
}

function SetDateSelected_CustomTables(id, selected) {
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

function GetEditMode_CustomTables(id) {
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

function SetEditMode_CustomTables(id, x, cid) {
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

function LoadFontSizes_CustomTables(id) {
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

function KeyPressSearch_CustomTables(event, id) {
    try {
        if (event.which == 13) {
            Search_CustomTables(id);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            Search_CustomTables(id);
            return false;
        }
        delete evt;
    }
}

function Search_CustomTables(id) {
    SetEditMode_CustomTables(id, false, "");
    GetRecords_CustomTables(id, false);
}