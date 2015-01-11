/* Database Imports Functions
----------------------------------*/
var arrSortColdbImports = new Array();
var arrEditModedbImports = new Array();
var arrAddValuesdbImports = new Array();
var timerIsRunning_dbImports = false;
var timerIntveral_dbImports = 1000 * 60;
var timerdbImports;

function Load_dbImports(id, allowEdit) {
    var innerHolder = $.trim($("#data-holder-" + id).html());

    $(document.body).on("click", "#app-" + id + " .exit-button-app, #app-" + id + "-min-bar .exit-button-app-min", function () {
        cookie.del(id + "-fontsize");
    });

    if (innerHolder == "") {
        GetRecords_dbImports(id, false, allowEdit);
    }
}

function Timer_dbImports(allowEdit) {
    timerIsRunning_dbImports = true;
    timerdbImports = setTimeout(function () {
        $(".custom-table-timermarker").each(function (index) {
            var id = $(this).attr("id").replace("-load", "");
            if (id != "") {
                GetRecords_dbImports(id, true, allowEdit);
            }
        });
    }, timerIntveral_dbImports);
}

function LoadingMessage_dbImports(id, message) {
    var x = "<div class='update-element-" + id + "'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#" + id + "-load").append(x);
    $(".update-element-" + id).show();
}

function LoadFontSizes_dbImports(id) {
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

function GetRecords_dbImports(id, fromTimer, allowEdit) {
    if (!fromTimer) {
        LoadingMessage_dbImports(id, "Loading Records. Please Wait...");
    }

    var arr = GetSortDirCol_dbImports(id);
    if (allowEdit) {
        SaveInputAddValues_dbImports(id, false);
    }

    $.ajax({
        url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/GetRecords",
        data: "{'id': '" + id + "','search': '" + $("#tb_search_" + id).val() + "','recordstopull': '" + $("#RecordstoSelect_" + id).val() + "','sortCol': '" + arr[1] + "','sortDir': '" + arr[0] + "','allowEdit': '" + allowEdit.toString() + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d.length > 0) {
                $("#data-holder-" + id).html(BuildTable_dbImports(data.d[0], data.d[1], id, data.d[2], false, allowEdit));
                LoadFontSizes_dbImports(id);

                if (allowEdit) {
                    AutoCompleteAddEdit_dbImports(id, false);
                    RestoreInputAddValues_dbImports(id, false);
                }
            }
            else {
                $("#data-holder-" + id).html("<h3>Error pulling records! Please try again.</h3>");
            }

            if ($(".update-element-" + id).length > 0) {
                $(".update-element-" + id).remove();
            }

            if (!timerIsRunning_dbImports) {
                Timer_dbImports(allowEdit);
            }
        }
    });
}

function BuildTable_dbImports(innerHeader, innerAddRow, id, arr, editMode, allowEdit) {
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
        xAddRow += "<td class=\"GridViewNumRow\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\"></div></td>";
        xAddRow += innerAddRow;
        if (!editMode) {
            xAddRow += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'><a href=\"#Add\" class=\"td-add-btn\" onclick=\"AddRecord_dbImports('" + id + "');return false;\" title=\"Add Row\"></a></td>";
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
            xItem += "<tr class=\"GridNormalRow myItemStyle\">";
        }
        else {
            if (openWSE.ConvertBitToBoolean(arr[i][1])) {
                xItem += "<tr id=\"" + id + "-editRow\" class=\"GridNormalRow myItemStyle\">";
                hasEdit = true;
            }
            else {
                xItem += "<tr class=\"GridNormalRow myItemStyle\">";
            }
        }
        xItem += "<td class=\"GridViewNumRow\" style=\"width: 40px;\"><div class=\"pad-top-sml pad-bottom-sml\">" + (i + 1).toString() + "</div></td>";
        for (var j = 0; j < arr[i][0].length; j++) {
            xItem += "<td class=\"border-right border-bottom\">" + arr[i][0][j].toString() + "</td>";
        }

        if (allowEdit) {
            var columnID = arr[i][0].toString();
            var editBtn = "";
            var delBtn = "";

            if (!editMode) {
                editBtn = "<a href=\"#Edit\" class=\"margin-right td-edit-btn\" onclick=\"EditRecord_dbImports('" + id + "', this);return false;\" title=\"Edit Row\"></a>";
                delBtn = "<a href=\"#Delete\" class=\"td-delete-btn\" onclick=\"DeleteRecord_dbImports('" + id + "', this);return false;\" title=\"Delete Row\"></a>";
            }
            else {
                if (openWSE.ConvertBitToBoolean(arr[i][1])) {
                    editBtn = "<a href=\"#Update\" class=\"margin-right td-update-btn\" onclick=\"UpdateRecord_dbImports('" + id + "', this);return false;\" title=\"Update Row\"></a>";
                    delBtn = "<a href=\"#Cancel\" class=\"td-cancel-btn\" onclick=\"CancelRecord_dbImports('" + id + "');return false;\" title=\"Cancel Edit\"></a>";
                }
            }

            xItem += "<td class=\"edit-buttons border-bottom\" align=\"center\" style='width: 70px;'>" + editBtn + delBtn + "</td>";
        }

        xItem += "</tr>";
    }

    if ((allowEdit) && (editMode) && (!hasEdit)) {
        SetEditMode_dbImports(id, false, "");
        openWSE.AlertWindow("The was an issue trying to find item in database. Please note that some items may not be found in the database due to certain characters within a column.");
        BuildTable_dbImports(innerHeader, innerAddRow, id, arr, false, allowEdit);
    }
    else {
        if (xItem == "") {
            return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + "</tbody></table><div class='emptyGridView'>No records found</div></div>"; 
        }
        return "<div id=\"" + id + "-complete-table\">" + xHeader + xAddRow + xItem + "</tbody></table></div>";
    }
}

function Refresh_dbImports(id, allowEdit) {
    LoadingMessage_dbImports(id, "Loading Records. Please Wait...");
    var arr = GetEditMode_dbImports(id);
    if (arr[0] == true) {
        EditRecord_dbImports(id, arr[1]);
    }
    else {
        GetRecords_dbImports(id, false, allowEdit);
    }
}

function RecordstoSelect_dbImports(id, allowEdit) {
    LoadingMessage_dbImports(id, "Loading Records. Please Wait...");
    var arr = GetEditMode_dbImports(id);
    if (arr[0] == true) {
        EditRecord_dbImports(id, arr[1]);
    }
    else {
        GetRecords_dbImports(id, false, allowEdit);
    }
}

function AutoCompleteAddEdit_dbImports(id, editMode) {
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

function OnSortClick_dbImports(_this, col, id, allowEdit) {
    var $_this = $(_this);
    if ($_this.hasClass("asc")) {
        sortDir = "DESC";
    }
    else {
        sortDir = "ASC";
    }

    var found = false;
    for (var i = 0; i < arrSortColdbImports.length; i++) {
        if (arrSortColdbImports[i][0] == id) {
            arrSortColdbImports[i][1] = sortDir;
            arrSortColdbImports[i][2] = col;
            found = true;
            break;
        }
    }
    if (!found) {
        var arrInner = new Array();
        arrInner[0] = id;
        arrInner[1] = sortDir;
        arrInner[2] = col;
        arrSortColdbImports[arrSortColdbImports.length] = arrInner;
    }

    var arr = GetEditMode_dbImports(id);
    if (arr[0] == true) {
        EditRecord_dbImports(id, arr[1]);
    }
    else {
        GetRecords_dbImports(id, false, allowEdit);
    }
}

function ExportToExcelAll_dbImports(id) {
    $("#btnExport-" + id).hide();
    $("#exportingNow_" + id).show();
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/ExportToExcel",
        data: "{ 'id': '" + id + "' }",
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

function EditRecord_dbImports(id, _this) {
    LoadingMessage_dbImports(id, "Loading Records. Please Wait...");
    var arr = GetSortDirCol_dbImports(id);

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
                $("#data-holder-" + id).html(BuildTable_dbImports(data.d[0], data.d[1], id, data.d[2], true, true));
                LoadFontSizes_dbImports(id);
                SetEditMode_dbImports(id, true, _this);

                AutoCompleteAddEdit_dbImports(id, true);
                SetFocus_dbImports(id, true);

                timerIsRunning_dbImports = false;
                clearTimeout(timerdbImports);
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

function DeleteRecord_dbImports(id, _this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this row?",
        function () {
            LoadingMessage_dbImports(id, "Deleting Record. Please Wait...");

            var rowData = new Array();
            $(_this).closest(".GridNormalRow").find(".border-right").each(function (index) {
                rowData[index] = $(this).html();
            });

            $.ajax({
                url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/DeleteRecord",
                data: JSON.stringify({ "id": id, "rowData": rowData }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if ($(".update-element-" + id).length > 0) {
                        $(".update-element-" + id).remove();
                    }

                    if (data.d[0] == "Success") {
                        GetRecords_dbImports(id, false, true);
                    }
                    else {
                        openWSE.AlertWindow(data.d[1]);
                    }
                }
            });
        }, null);
}

function UpdateRecordKeyPress_dbImports(event, id, _this) {
    try {
        if (event.which == 13) {
            UpdateRecord_dbImports(id, _this);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            UpdateRecord_dbImports(id, _this);
            return false;
        }
        delete evt;
    }
}

function UpdateRecord_dbImports(id, _this) {
    SetEditMode_dbImports(id, false, "");
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
        LoadingMessage_dbImports(id, "Updating Record. Please Wait...");
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
                    GetRecords_dbImports(id, false, true);
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

function CancelRecord_dbImports(id) {
    SetEditMode_dbImports(id, false, "");
    GetRecords_dbImports(id, false, true);
}

function AddRecordKeyPress_dbImports(event, id) {
    try {
        if (event.which == 13) {
            AddRecord_dbImports(id);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddRecord_dbImports(id);
            return false;
        }
        delete evt;
    }
}

function AddRecord_dbImports(id) {
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
        LoadingMessage_dbImports(id, "Adding Record. Please Wait...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/dbImportservice.asmx/AddRecord",
            data: JSON.stringify({ "id": id, "recordVals": recordVals }),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if ($(".update-element-" + id).length > 0) {
                    $(".update-element-" + id).remove();
                }

                if (data.d[0] == "Success") {
                    $("#" + id + "-addRow").find("input").each(function (index) {
                        $(this).val("");
                    });

                    SaveInputAddValues_dbImports(id, false);
                    GetRecords_dbImports(id, false, true);
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

function SetFocus_dbImports(id, editMode) {
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

function SaveInputAddValues_dbImports(id, editMode) {
    var rowType = "addRow";
    if (editMode) {
        rowType = "editRow";
    }
    arrAddValuesdbImports = new Array();
    $("#" + id + "-" + rowType).find("input").each(function (index) {
        arrAddValuesdbImports[index] = $(this).val();
    });
}

function RestoreInputAddValues_dbImports(id, editMode) {
    var rowType = "addRow";
    if (editMode) {
        rowType = "editRow";
    }

    $("#" + id + "-" + rowType).find("input").each(function (index) {
        if ($.trim($(this).val()) == "") {
            $(this).val(arrAddValuesdbImports[index]);
        }
    });
    arrAddValuesdbImports = new Array();
}

function GetSortDirCol_dbImports(id) {
    var arr = new Array();
    arr[0] = "";
    arr[1] = "";
    for (var i = 0; i < arrSortColdbImports.length; i++) {
        if (arrSortColdbImports[i][0] == id) {
            arr[0] = arrSortColdbImports[i][1];
            arr[1] = arrSortColdbImports[i][2];
        }
    }

    return arr;
}

function GetEditMode_dbImports(id) {
    var arr = new Array();
    arr[0] = false
    for (var i = 0; i < arrEditModedbImports.length; i++) {
        if (arrEditModedbImports[i][0] == id) {
            if (arrEditModedbImports[i][1] == true) {
                arr[0] = true;
                arr[1] = arrEditModedbImports[i][2];
            }
        }
    }

    return arr;
}

function SetEditMode_dbImports(id, x, _this) {
    var found = false;
    for (var i = 0; i < arrEditModedbImports.length; i++) {
        if (arrEditModedbImports[i][0] == id) {
            arrEditModedbImports[i][1] = x;
            arrEditModedbImports[i][2] = _this;
            found = true;
            break;
        }
    }
    if (!found) {
        var arrInner = new Array();
        arrInner[0] = id;
        arrInner[1] = x;
        arrInner[2] = _this;

        arrEditModedbImports[arrEditModedbImports.length] = arrInner;
    }
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

function KeyPressSearch_dbImports(event, id, allowEdit) {
    try {
        if (event.which == 13) {
            Search_dbImports(id, allowEdit);
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            Search_dbImports(id, allowEdit);
            return false;
        }
        delete evt;
    }
}

function Search_dbImports(id, allowEdit) {
    SetEditMode_dbImports(id, false, "");
    GetRecords_dbImports(id, false, allowEdit);
}