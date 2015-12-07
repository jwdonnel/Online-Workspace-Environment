/* Variables */
var columns = new Array();
var availableCharts = null;
var collapsedData = new Array();
var loadedTables = new Array();


/* On Page load Functions */
$(document).ready(function () {
    AddIDRow();
    InitializeSortColumns();
});

var currStepBeforePostback = "";
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function () {
    if ($("#ImportWizard-element").css("display") == "block") {
        $("#create-wizard-steps").find(".steps").each(function (index) {
            if ($(this).css("display") != "none") {
                currStepBeforePostback = ".step" + (index + 1);
            }
        });
    }
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    if (currStepBeforePostback != "") {
        if ($("#ImportWizard-element").css("display") == "block") {
            $("#create-wizard-steps").find(".steps").hide();
            $("#create-wizard-steps").find(currStepBeforePostback).show();
        }
    }

    for (var i = 0 ; i < collapsedData.length; i++) {
        $(".more-table-details[data-id='" + collapsedData[i] + "']").show();
        $(".showhidedetails[data-rowid='" + collapsedData[i] + "']").html("Hide Details");
    }

    currStepBeforePostback = "";
});


/* Custom Table List Functions */
var tableEditing = "";
function DeleteEntry(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "? If you select ok you will need to supply the correct password for the user who created the table.",
       function () {
           if (id != "") {
               $("#hf_tableDeleteID").val(id);

               if (UserIsSocialAccount != null && UserIsSocialAccount) {
                   $("#MainContent_btn_passwordConfirm").trigger("click");
               }
               else {
                   openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
                   $("#MainContent_tb_passwordConfirm").focus();
               }
           }
       }, null);
}
function CancelDelete() {
    openWSE.LoadModalWindow(false, "password-element", "");
    $("#hf_tableDeleteID").val("");
    $("#MainContent_tb_passwordConfirm").val("");
}
function PerformDelete(id) {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        openWSE.LoadingMessage1("Deleting Table. Please Wait...");
    }, 100);
    $("#hf_tableDelete").val(id);
    __doPostBack("hf_tableDelete", "");
}
function EditEntry(id) {
    tableEditing = id;
    openWSE.LoadingMessage1("Getting Information. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/EditTable",
        data: "{ 'id': '" + id + "' }",
        contentType: "application/json; charset=utf-8",
        cache: false,
        success: function (data) {
            if (!data.d || data.d.length == 0) {
                openWSE.AlertWindow("An error occured while trying to get your table info.");
            }
            else {
                try {
                    StartCreateWizard();

                    $("#tb_tablename").val(data.d[0]);
                    $("#tb_description").val(data.d[1]);

                    var $modalContent = $("#UsersAllowedEdit-element");
                    if (data.d[2] != "") {
                        $modalContent.html(data.d[2]);
                    }

                    $("#cb_allowNotifi").prop("checked", data.d[3]);

                    try {
                        columns = $.parseJSON(data.d[4]);
                    } catch (evt2) { }

                    SetCustomizations(data.d[5]);

                    AddIDRow();
                    for (var i = 0; i < columns.length; i++) {
                        BuildColumn(columns[i], false, false, i + 1);
                    }

                    $("#tr-usersallowed").hide();
                    $("#tr-usersallowedEdit").show();
                    $("#div_InstallAfterLoad").hide();
                    $("#div_isPrivate").hide();

                    $("#btn_finishImportWizard").val("Update");
                    openWSE.LoadModalWindow(true, "ImportWizard-element", "Custom Table Wizard");
                }
                catch (evt) { }
            }

            openWSE.RemoveUpdateModal();
        },
        error: function (data) {
            openWSE.AlertWindow("An error occured while trying to get your table info.");
            openWSE.RemoveUpdateModal();
        }
    });
}
function RecreateApp(appid, tableId, tableName, description) {
    openWSE.LoadingMessage1("Creating App. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/RecreateApp",
        data: "{ 'appId': '" + appid + "','tableId': '" + tableId + "','tableName': '" + escape(tableName) + "', 'description': '" + escape(description) + "' }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occured while trying to create the app.");
            }
            else {
                $("#hf_tableUpdate").val(new Date().toString());
                __doPostBack("hf_tableUpdate", "");
            }
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to create the app.");
        }
    });
}
function ShowHideTableDetails(id) {
    var $details = $(".more-table-details[data-id='" + id + "']");
    var $detailsBtn = $(".showhidedetails[data-rowid='" + id + "']");

    if ($details.length > 0) {
        var addEntry = false;
        if ($details.eq(0).css("display") == "none") {
            $details.show();
            $detailsBtn.html("Hide Details");
            addEntry = true;
        }
        else {
            $details.hide();
            $detailsBtn.html("Show Details");
        }

        var index = -1;
        for (var i = 0 ; i < collapsedData.length; i++) {
            if (collapsedData[i] == id) {
                index = i;
                break;
            }
        }

        if (index != -1) {
            collapsedData.splice(index, 1);
        }

        if (addEntry) {
            collapsedData.push(id);
        }
    }
}


/* Wizard Functions */
function StartCreateWizard() {
    $("#create-wizard-steps").show();

    $("#create-wizard-steps").find("li").hide();
    $("#create-wizard-steps").find(".step1").show();
    $("#btn_finishImportWizard, .prev-step").hide();
    $(".next-step").show();
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");

    $("#tr-usersallowed").show();
    $("#tr-usersallowedEdit").hide();
    $("#div_InstallAfterLoad").show();
    $("#div_isPrivate").show();
    $("#chart-column-selector-holder").html("");

    $("#MainContent_pnl_usersAllowedToEdit").find(".checkbox-click").each(function () {
        var $input = $(this).find("input[type='checkbox']");
        var userChecked = false;
        if ($input.val().toLowerCase() == $.trim($("#lbl_UserFullName").html()).toLowerCase()) {
            userChecked = true;
        }

        $input.prop("checked", userChecked);
    });
    InitializeSortColumns();
    SetDefaultCustomizations();
    $("#btn_finishImportWizard").val("Create");
    openWSE.LoadModalWindow(true, "ImportWizard-element", "Custom Table Wizard");
}
function CloseCreateWizard() {
    ClearControls();
    openWSE.LoadModalWindow(false, "ImportWizard-element", "");
}
function ConfirmCancelWizard() {
    openWSE.ConfirmWindow("Are you sure you want to cancel the custom table wizard?", function () {
        CloseCreateWizard();
    }, null);
}
function CreateNextStep() {
    var currentStep = 1;

    var $ele = $("#create-wizard-steps");
    for (var i = 0; i < $ele.find("li").length; i++) {
        if ($ele.find("li").eq(i).css("display") != "none") {
            currentStep = i + 2;
            break;
        }
    }

    if (columns && columns.length == 0 && currentStep == 4) {
        $(".next-step").addClass("next-disabled");
        $(".next-step").prop("disabled", true);
    }
    else {
        $(".next-step").removeClass("next-disabled");
        $(".next-step").prop("disabled", false);
    }

    $ele.find("li").hide();
    $ele.find(".step" + currentStep).show();

    $("#btn_finishImportWizard").hide();
    $(".next-step, .prev-step").show();

    if (i >= $ele.find("li").length - 2) {
        $(".next-step").hide();
        $("#btn_finishImportWizard").show();
    }

    if ($ele.find(".step" + currentStep).find(".textEntry").length > 0) {
        $ele.find(".step" + currentStep).find(".textEntry").eq(0).focus();
    }

    AdjustCreateBox();
}
function CreatePrevStep() {
    var currentStep = 1;

    var $ele = $("#create-wizard-steps");

    for (var i = 0; i < $ele.find("li").length; i++) {
        if ($ele.find("li").eq(i).css("display") != "none") {
            currentStep = i;
            break;
        }
    }

    if (columns.length == 0 && currentStep == 4) {
        $(".next-step").addClass("next-disabled");
        $(".next-step").prop("disabled", true);
    }
    else {
        $(".next-step").removeClass("next-disabled");
        $(".next-step").prop("disabled", false);
    }

    $ele.find("li").hide();
    $ele.find(".step" + currentStep).show();

    $("#btn_finishImportWizard").hide();
    $(".next-step, .prev-step").show();

    if (i == 1) {
        $(".prev-step").hide();
    }

    if ($ele.find(".step" + currentStep).find(".textEntry").length > 0) {
        $ele.find(".step" + currentStep).find(".textEntry").eq(0).focus();
    }

    AdjustCreateBox();
}
function ClearControls() {
    $("#tb_tablename").val("");
    $("#tb_description").val("");
    $("#tb_columnName").val("");
    $("#ddl_datatypes").val("nvarchar");
    $("#tb_length").val("100");
    $("#cb_nullable").prop("checked", false);
    $("#UsersAllowedEdit-element").html("");
    $("#chart-column-selector-holder").html("");
    $("#table_columns").html("");
    AddIDRow();

    columns = new Array();
    SetDefaultCustomizations();

    currStepBeforePostback = "";
    $("#ImportWizard-element").find(".Modal-element-modal").css("top", "auto");
    $("#ImportWizard-element").find(".Modal-element-modal").css("left", "auto");
}
function AdjustCreateBox() {
    var top = $("#ImportWizard-element").find(".Modal-element-modal").css("top");
    if (top == "auto") {
        $("#ImportWizard-element").find(".Modal-element-align").css({
            marginTop: -($("#ImportWizard-element").find(".Modal-element-modal").height() / 2),
            marginLeft: -($("#ImportWizard-element").find(".Modal-element-modal").width() / 2)
        });
    }

    openWSE.AdjustModalWindowView();
}
function ImportExcelSpreadSheet() {
    try {
        var data = new FormData();
        var files = $("#excelFileUpload").get(0).files;
        if (files.length > 0) {
            data.append("excelFileUpload", files[0]);
        }
        openWSE.LoadingMessage1("Please Wait...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/GetFileUploadContents",
            type: "POST",
            processData: false,
            contentType: false,
            data: data,
            success: function (response) {
                //code after success
                var innerHtml = '<input type="file" id="excelFileUpload" class="margin-right" /><input type="button" class="input-buttons" onclick="ImportExcelSpreadSheet()" value="Import" />';
                $("#div-excelFileUpload").html(innerHtml);
                openWSE.RemoveUpdateModal();

                if (data.d == "false") {
                    openWSE.AlertWindow("Failed to import file. Please try again.");
                }
            },
            error: function (er) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow(er);
            }

        });
    }
    catch (evt) {
        openWSE.RemoveUpdateModal();
        openWSE.AlertWindow("Failed to import file. Please try again.");
    }
}
function CreateUpdateTable() {
    var tableName = $.trim($("#tb_tablename").val());
    if (tableName == "") {
        openWSE.AlertWindow("Must provide a table name.");
    }
    else if (columns.length == 0) {
        openWSE.AlertWindow("Must have more than 1 column for this table.");
    }
    else {
        var usersAllowed = "";
        if (tableEditing != "") {
            openWSE.LoadingMessage1("Updating Table. Please Wait...");

            $("#UsersAllowedEdit-element").find(".checkbox-usersallowed").each(function () {
                if ($(this).prop("checked")) {
                    usersAllowed += $(this).val().toLowerCase() + ";";
                }
            });
        }
        else {
            openWSE.LoadingMessage1("Creating Table. Please Wait...");

            $("#UsersAllowed-element").find(".checkbox-usersallowed").each(function () {
                if ($(this).prop("checked")) {
                    usersAllowed += $(this).val().toLowerCase() + ";";
                }
            });
        }

        var description = $.trim($("#tb_description").val());
        var columnData = JSON.stringify(columns);
        var customizations = BuildCustomizationString();
        columnData = escape(columnData);
        customizations = escape(customizations);
        tableName = escape(tableName);
        usersAllowed = escape(usersAllowed);

        var serviceMethod = "Create";
        var methodData = "{ 'columns': '" + columnData + "', 'customizations': '" + customizations + "', 'tableName': '" + tableName + "', 'description': '" + description + "', 'installForUser': '" + $("#MainContent_cb_InstallAfterLoad").is(":checked") + "', 'notifyUsers': '" + $("#cb_allowNotifi").is(":checked") + "', 'isPrivate': '" + $("#cb_isPrivate").is(":checked") + "', 'usersAllowed': '" + usersAllowed + "' }";
        if (tableEditing != "") {
            serviceMethod = "Update";
            methodData = "{ 'tableID': '" + tableEditing + "', 'columns': '" + columnData + "', 'customizations': '" + customizations + "', 'tableName': '" + tableName + "', 'description': '" + description + "', 'notifyUsers': '" + $("#cb_allowNotifi").is(":checked") + "', 'usersAllowed': '" + usersAllowed + "' }";
        }

        $.ajax({
            type: "POST",
            url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/" + serviceMethod,
            data: methodData,
            contentType: "application/json; charset=utf-8",
            cache: false,
            success: function (data) {
                if (!openWSE.ConvertBitToBoolean(data.d)) {
                    openWSE.AlertWindow("An error occured while trying to " + serviceMethod.toLowerCase() + " your table.");
                    openWSE.RemoveUpdateModal();
                }
                else {
                    tableEditing = "";
                    setTimeout(function () {
                        CloseCreateWizard();

                        $("#hf_tableUpdate").val(new Date().toString());
                        __doPostBack("hf_tableUpdate", "");
                    }, 1000);
                }
            },
            error: function (data) {
                openWSE.AlertWindow("An error occured while trying to " + serviceMethod.toLowerCase() + " your table.");
                openWSE.RemoveUpdateModal();
            }
        });
    }
}


/* Customizations */
function fontFamilyChange() {
    $("#span_fontfamilypreview").css("font-family", $("#dd_fontfamilycustomization").val());
}
function UseDefaultChanged(cbCtrl, tbCtrl) {
    var $cb = $("#" + cbCtrl);
    var $tb = $("#" + tbCtrl);
    if ($cb.length > 0 && $tb.length > 0) {
        if ($cb.is(":checked")) {
            $tb.prop("disabled", true);
        }
        else {
            $tb.prop("disabled", false);
        }
    }
}
function SetDefaultCustomizations() {
    $("#tb_tableheadercolor").prop("disabled", true);
    $("#tb_tableheadercolor").val("FFFFFF");
    $("#tb_tableheadercolor").css("color", "#000000");
    $("#tb_tableheadercolor").css("background-color", "#FFFFFF");
    $("#cb_usedefaultheadercolor").prop("checked", true);

    $("#tb_primaryrowcolor").prop("disabled", true);
    $("#tb_primaryrowcolor").val("FFFFFF");
    $("#tb_primaryrowcolor").css("color", "#000000");
    $("#tb_primaryrowcolor").css("background-color", "#FFFFFF");
    $("#cb_usedefaultprimarycolor").prop("checked", true);

    $("#tb_alternativerowcolor").prop("disabled", true);
    $("#tb_alternativerowcolor").val("FFFFFF");
    $("#tb_alternativerowcolor").css("color", "#000000");
    $("#tb_alternativerowcolor").css("background-color", "#FFFFFF");
    $("#cb_usedefaultalternativecolor").prop("checked", true);

    $("#dd_fontfamilycustomization").val("");
    $("#span_fontfamilypreview").css("font-family", "");

    $("#ddl_ChartType").val("None");
    $("#img_charttype_holder").hide();
    $("#chart-title-holder").hide();
    $("#chart-column-selector").hide();

}
function BuildCustomizationString() {
    var customizationArray = new Array();
    var customizeName = "";
    var customizeValue = "";

    // HeaderColor
    customizeName = "HeaderColor";
    if (!$("#cb_usedefaultheadercolor").prop("checked")) {
        customizeValue = $.trim($("#tb_tableheadercolor").val());
        if (customizeValue.indexOf("#") != 0) {
            customizeValue = "#" + customizeValue;
        }

        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": customizeValue
        });
    }
    else {
        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": ""
        });
    }

    // PrimaryRowColor
    customizeName = "PrimaryRowColor";
    if (!$("#cb_usedefaultprimarycolor").prop("checked")) {
        customizeValue = $.trim($("#tb_primaryrowcolor").val());
        if (customizeValue.indexOf("#") != 0) {
            customizeValue = "#" + customizeValue;
        }

        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": customizeValue
        });
    }
    else {
        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": ""
        });
    }

    // AlternativeRowColor
    customizeName = "AlternativeRowColor";
    if (!$("#cb_usedefaultalternativecolor").prop("checked")) {
        customizeValue = $.trim($("#tb_alternativerowcolor").val());
        if (customizeValue.indexOf("#") != 0) {
            customizeValue = "#" + customizeValue;
        }

        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": customizeValue
        });
    }
    else {
        customizationArray.push({
            "customizeName": customizeName,
            "customizeValue": ""
        });
    }

    // FontFamily
    customizeName = "FontFamily";
    customizeValue = $.trim($("#dd_fontfamilycustomization").val());
    customizationArray.push({
        "customizeName": customizeName,
        "customizeValue": customizeValue
    });

    // ChartType
    customizeName = "ChartType";
    customizeValue = $.trim($("#ddl_ChartType").val());
    customizationArray.push({
        "customizeName": customizeName,
        "customizeValue": customizeValue
    });

    // ChartTitle
    customizeName = "ChartTitle";
    customizeValue = $.trim($("#tb_chartTitle").val());
    customizationArray.push({
        "customizeName": customizeName,
        "customizeValue": customizeValue
    });

    // ChartColumns
    customizeName = "ChartColumns";
    customizeValue = "";
    $("#chart-column-selector-holder").find("input[type='checkbox']").each(function () {
        if ($(this).prop("checked")) {
            customizeValue += $(this).val() + ";";
        }
    });
    customizationArray.push({
        "customizeName": customizeName,
        "customizeValue": customizeValue
    });

    return JSON.stringify(customizationArray);
}
function SetCustomizations(customizationStr) {
    try {
        var customizations = $.parseJSON(customizationStr);
        for (var i = 0; i < customizations.length; i++) {
            var value = customizations[i].customizeValue;

            switch (customizations[i].customizeName) {
                case "HeaderColor":
                    if (value != "") {
                        $("#cb_usedefaultheadercolor").prop("checked", false);
                        $("#tb_tableheadercolor").prop("disabled", false);
                        $("#tb_tableheadercolor").val(value.replace("#", ""));
                        $("#tb_tableheadercolor").css("background-color", value);
                    }
                    else {
                        $("#cb_usedefaultheadercolor").prop("checked", true);
                    }
                    break;

                case "PrimaryRowColor":
                    if (value != "") {
                        $("#cb_usedefaultprimarycolor").prop("checked", false);
                        $("#tb_primaryrowcolor").prop("disabled", false);
                        $("#tb_primaryrowcolor").val(value.replace("#", ""));
                        $("#tb_primaryrowcolor").css("background-color", value);
                    }
                    else {
                        $("#cb_usedefaultprimarycolor").prop("checked", true);
                    }
                    break;

                case "AlternativeRowColor":
                    if (value != "") {
                        $("#cb_usedefaultalternativecolor").prop("checked", false);
                        $("#tb_alternativerowcolor").prop("disabled", false);
                        $("#tb_alternativerowcolor").val(value.replace("#", ""));
                        $("#tb_alternativerowcolor").css("background-color", value);
                    }
                    else {
                        $("#cb_usedefaultalternativecolor").prop("checked", true);
                    }
                    break;

                case "FontFamily":
                    if (value != "") {
                        $("#dd_fontfamilycustomization").val(value);
                        fontFamilyChange();
                    }
                    else {
                        $("#dd_fontfamilycustomization").val("");
                        fontFamilyChange();
                    }
                    break;

                case "ChartType":
                    $("#ddl_ChartType").val(value);
                    ChartTypeChangeEvent();
                    break;

                case "ChartTitle":
                    $("#tb_chartTitle").val(value);
                    break;

                case "ChartColumns":
                    var columnSplit = value.split(';');
                    var chartColumnStr = "";
                    for (var ii = 0; ii < columns.length; ii++) {
                        var checked = "";
                        for (var jj = 0; jj < columnSplit.length; jj++) {
                            if (columns[ii].realName == columnSplit[jj]) {
                                checked = "checked='checked'";
                                break;
                            }
                        }

                        chartColumnStr += "<input type='checkbox' id='cb_" + columns[ii].realName + "_chartColumn' value='" + columns[ii].realName + "' " + checked + " />";
                        chartColumnStr += "<label for='cb_" + columns[ii].realName + "_chartColumn'>&nbsp;" + columns[ii].shownName + "</label><div class='clear-space-five'></div>";
                    }
                    $("#chart-column-selector-holder").html(chartColumnStr);
                    break;
            }
        }
    }
    catch (evt) { }
}


/* Users Allowed Modal */
$(document.body).on("click", ".checkbox-click", function (e) {
    if ($(e.target)[0].localName.toLowerCase() != "input") {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                $input.prop("checked", false);
            }
            else {
                $input.prop("checked", true);
            }
        }
    }
});


/* Table Editor and Creator */
var fixHelper = function (e, ui) {
    ui.children().each(function () {
        $(this).width($(this).width());
    });
    return ui;
};
$(document.body).on("keypress", "#tb_length, #tb_length_edit", function (e) {
    try {
        var code = (e.which) ? e.which : e.keyCode;
        var val = String.fromCharCode(code);

        if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
            return false;
        }
    }
    catch (evt) { }
});
$(document.body).on("change", "#ddl_datatypes", function () {
    $("#tb_length").show();
    if ($(this).val() != "nvarchar") {
        $("#tb_length").hide();
    }
});
$(document.body).on("change", "#ddl_datatypes_edit", function () {
    $("#tb_length_edit").show();
    if ($(this).val() != "nvarchar") {
        $("#tb_length_edit").hide();
    }
});
$(document.body).on("change", "#MainContent_cb_InstallAfterLoad", function () {
    if ($(this).is(":checked")) {
        $("#div_isPrivate").show();
    }
    else {
        $("#div_isPrivate").hide();
    }
});
function FormatColumnName(name) {
    name = name.replace(/[|&#;$%@"<>()+,:'.=+?/*\[\]{}-]/g, "");
    name = name.replace(/ /g, "_");
    name = name.replace(/\//g, "_");
    return name;
}
function InitializeSortColumns() {
    $("#table_columns tbody").sortable({
        cancel: ".entryIDRow, .border-right, .myHeaderStyle",
        helper: fixHelper,
        axis: 'y',
        containment: "#sortContainer",
        cursor: "move",
        stop: function (event, ui) {
            SortColumnStop();
        }
    });
}
function SortColumnStop() {
    var tempColumns = columns;
    columns = new Array();
    var index = 0;
    $("#table_columns .myItemStyle").each(function () {
        var $this = $(this);
        var $thisName = $this.find(".column-name");
        var $realName = $this.find(".real-column-name");
        var $thisType = $this.find(".column-type");
        var $thisLength = $this.find(".column-length");
        var $thisNullable = $this.find(".column-nullable");
        var name = $.trim($realName.html());
        if ((name.toLowerCase() != "applicationid") && (name.toLowerCase() != "entryid") && (name.toLowerCase() != "timestamp")) {
            var type = $thisType.text();
            var len = $thisLength.text();
            var nullable = $thisNullable.find("#cb_nullable_row").is(":checked");

            columns[index] = {
                "realName": name,
                "shownName": $.trim($thisName.html()),
                "dataType": type,
                "dataLength": len,
                "nullable": nullable
            };
            index++;
        }
    });

    $("#table_columns").html("<tbody></tbody>");
    AddIDRow();
    for (var i = 0; i < columns.length; i++) {
        BuildColumn(columns[i], false, false, i + 1);
    }

    InitializeSortColumns();
    UpdateChartColumnSelectors();
} 
function AddIDRow() {
    var column = "<tr class='myHeaderStyle'>";
    column += "<td width='45px'></td>";
    column += "<td>Column Name</td>";
    column += "<td width='135px'>Data Type</td>";
    column += "<td width='75px'>Length</td>";
    column += "<td width='75px'>Nullable</td>";
    column += "<td width='75px'>Actions</td>";
    column += "</tr>";

    if ($("#table_columns").find("tbody").length == 0) {
        $("#table_columns").html("<tbody></tbody>");
    }
    $("#table_columns").find("tbody").html(column);
}
function BuildColumn(column, inEditMode, isEditVal, columnNum) {
    var realName = column.realName;
    var name = column.shownName;
    var type = column.dataType;
    var len = column.dataLength;
    var nullable = column.nullable;

    var nameEdit = name;
    var typeEdit = type;
    var lenEdit = len;
    var nullableEdit = nullable;

    if (columns.length == 0 && $("#create-wizard-steps").find(".step3").css("display") != "none") {
        $(".next-step").addClass("next-disabled");
        $(".next-step").prop("disabled", true);
    }
    else {
        $(".next-step").removeClass("next-disabled");
        $(".next-step").prop("disabled", false);
    }

    if (isEditVal) {
        nameEdit = "<input type='text' id='tb_columnName_edit' class='textEntry' onkeydown='FinishEditKeyDown(event, \"" + realName + "\", " + columnNum + ")' maxlength='100' style='width: 90%;' />";
        type = "<select id='ddl_datatypes_edit'><option value='nvarchar'>nvarchar</option><option value='Date'>Date</option><option value='DateTime'>DateTime</option><option value='Integer'>Integer</option><option value='Money'>Money</option><option value='Decimal'>Decimal</option><option value='Boolean'>Boolean</option></select>";

        var cssHideLen = "display: block;";
        if (typeEdit != "nvarchar") {
            cssHideLen = "display: none;";
        }

        len = "<input type='text' class='textEntry' id='tb_length_edit' maxlength='4' onkeydown='FinishEditKeyDown(event, \"" + realName + "\", " + columnNum + ")' style='width: 55px;" + cssHideLen + "' />";
        nullable = "<input id='cb_nullable_edit' type='checkbox' />";
    }
    else {
        if (openWSE.ConvertBitToBoolean(nullableEdit)) {
            nullable = "<input id='cb_nullable_row' type='checkbox' checked='checked' disabled />";
        }
        else {
            nullable = "<input id='cb_nullable_row' type='checkbox' disabled />";
        }

        if (typeEdit != "nvarchar") {
            len = "";
        }
    }

    var column = "<tr class='GridNormalRow myItemStyle'>";
    column += "<td align='center' class='GridViewNumRow'>" + columnNum + "</td>";
    column += "<td class='border-right'><span class='pad-left'><span class='column-name'>" + nameEdit + "</span><span class='real-column-name' style='display: none;'>" + realName + "</span></td>";
    column += "<td class='border-right column-type' align='center'>" + type + "</td>";
    column += "<td class='border-right column-length' align='center'>" + len + "</td>";
    column += "<td class='border-right column-nullable' align='center'>" + nullable + "</td>";

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a href='#edit' onclick='EditControls(\"" + realName + "\");return false;' class='td-edit-btn margin-right' title='Edit'></a>";
        actionButton += "<a href='#delete' onclick='DeleteRow(\"" + columnNum + "\");return false;' class='td-delete-btn' title='Delete'></a>";
    }

    if (isEditVal) {
        actionButton = "<a href='#update' onclick='FinishEdit(\"" + columnNum + "\", \"" + realName + "\", false);return false;' class='td-update-btn margin-right' title='Update'></a>";
        actionButton += "<a href='#cancel' onclick='FinishEdit(\"" + columnNum + "\", \"" + realName + "\", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
    }

    if (actionButton == "") {
        actionButton = "<div class='float-left' style='width: 22px; height: 22px;'></div>";
    }

    column += "<td class='border-right' align='center'>" + actionButton + "</td>";
    column += "</tr>";

    $("#table_columns").find("tbody").append(column);
    $("#tb_columnName").val("");

    if (isEditVal) {
        $("#tb_columnName_edit").val(name);
        $("#tb_length_edit").val(lenEdit);
        $("#ddl_datatypes_edit").val(typeEdit);

        if (openWSE.ConvertBitToBoolean(nullableEdit)) {
            $("#cb_nullable_edit").prop("checked", true);
        }
        else {
            $("#cb_nullable_edit").prop("checked", false);
        }
    }

    AdjustCreateBox();
}


/* Input Controls */
function AddNewColumn() {
    var len = columns.length;
    var newColumn = $.trim($("#tb_columnName").val());
    var realColumnName = FormatColumnName(newColumn);

    if (realColumnName == "") {
        openWSE.AlertWindow("Column name cannot be blank. Please enter a name and try again.");
    }
    else if ((realColumnName.toLowerCase() == "applicationid") || (realColumnName.toLowerCase() == "entryid") || (realColumnName.toLowerCase() == "timestamp")) {
        openWSE.AlertWindow("Column already exists! Please use a different name.");
    }
    else {
        var canContinue = true;
        for (var i = 0; i < len; i++) {
            if ((columns[i].realName == realColumnName) || (realColumnName.toLowerCase() == "applicationid") || (realColumnName.toLowerCase() == "entryid") || (realColumnName.toLowerCase() == "timestamp")) {
                canContinue = false;
                openWSE.AlertWindow("Column already exists! Please use a different name.");
                break;
            }
        }

        if (canContinue) {
            var dataType = $("#ddl_datatypes").val();
            var nullable = $("#cb_nullable").prop('checked');
            var newLen = $.trim($("#tb_length").val());
            if (parseInt(newLen) > 3999 || parseInt(newLen) < 0 || parseInt(newLen).toString() == NaN) {
                newLen = "100";
            }

            $("#tb_length").val("100");

            columns[len] = {
                "realName": realColumnName,
                "shownName": newColumn,
                "dataType": dataType,
                "dataLength": newLen,
                "nullable": nullable
            };

            BuildColumn(columns[len], false, false, len + 1);
            UpdateChartColumnSelectors();
            $("#tb_columnName").focus();
        }
    }
}
function AddNewRowTextBoxKeyDown(event) {
    try {
        if (event.which == 13) {
            AddNewColumn();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddNewColumn();
        }
        delete evt;
    }
}
function EditControls(name) {
    $("#table_columns tbody").sortable("disable");
    AddIDRow();
    var isEdit = false;
    for (var i = 0; i < columns.length; i++) {
        if ((columns[i].realName == name) && (name.toLowerCase() != "applicationid") && (name.toLowerCase() != "entryid") && (name.toLowerCase() != "timestamp")) {
            BuildColumn(columns[i], true, true, i + 1);
            isEdit = true;
        }
        else {
            BuildColumn(columns[i], true, false, i + 1);
        }
    }

    $("#table_addcolumn_Controls").show();
    if (isEdit) {
        $("#table_addcolumn_Controls").hide();
    }
}
function FinishEditKeyDown(event, name, index) {
    try {
        if (event.which == 13) {
            FinishEdit(index, name, false);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            FinishEdit(index, name, false);
        }
        delete evt;
    }
}
function FinishEdit(index, realColumnName, cancelled) {
    var canContinue = true;
    index = (parseInt(index) - 1);
    if (!cancelled) {
        var columnName = $.trim($("#tb_columnName_edit").val());
        for (var i = 0; i < columns.length; i++) {
            if (((columns[i].realName == realColumnName) && (index != i)) || (realColumnName.toLowerCase() == "applicationid") || (realColumnName.toLowerCase() == "entryid") || (realColumnName.toLowerCase() == "timestamp")) {
                canContinue = false;
                openWSE.AlertWindow("Column already exists! Please use a different name.");
                break;
            }
        }

        if (canContinue) {
            var dataLength = parseInt($.trim($("#tb_length_edit").val()));
            if (dataLength.toString() == "NaN") {
                dataLength = 100;
            }

            columns[index].realName = realColumnName;
            columns[index].shownName = columnName;
            columns[index].dataType = $("#ddl_datatypes_edit").val();
            columns[index].dataLength = dataLength;
            columns[index].nullable = $("#cb_nullable_edit").prop('checked');
        }
    }

    $("#table_addcolumn_Controls").show();

    if (canContinue) {
        $("#table_columns tbody").sortable("enable");
        AddIDRow();
        for (var i = 0; i < columns.length; i++) {
            BuildColumn(columns[i], false, false, i + 1);
        }

        UpdateChartColumnSelectors();
    }
}
function DeleteRow(index) {
    openWSE.ConfirmWindow("Are you sure you want to delete this column?",
        function () {
            index = (parseInt(index) - 1);
            columns.splice(index, 1);
            if (columns.length == 0) {
                columns = new Array();
            }

            AddIDRow();
            for (var i = 0; i < columns.length; i++) {
                BuildColumn(columns[i], false, false, i + 1);
            }

            if (columns.length == 0 && $("#create-wizard-steps").find(".step4").css("display") != "none") {
                $("#chart-column-selector-holder").html("<h3 class='pad-all'>No columns available</h3>");
                $(".next-step").addClass("next-disabled");
                $(".next-step").prop("disabled", true);
            }
            else {
                UpdateChartColumnSelectors();
                $(".next-step").removeClass("next-disabled");
                $(".next-step").prop("disabled", false);
            }
        }, null);
}


/* Chart Selector */
function ChartTypeChangeEvent() {
    var $this = $("#ddl_ChartType");
    if ($this.val().toLowerCase() == "none") {
        $("#img_charttype_holder").hide();
        $("#chart-title-holder").hide();
        $("#chart-column-selector").hide();
    }
    else {
        $("#img_charttype_holder").show();
        $("#chart-title-holder").show();
        $("#chart-column-selector").show();
        $("#img_charttype").attr("src", openWSE.siteRoot() + "Standard_Images/ChartTypes/" + $this.val().replace(/ /g, "").toLowerCase() + ".png");

        var galleryFolder = $this.val().toLowerCase() + "chart";
        switch ($this.val().toLowerCase()) {
            case "donut":
                galleryFolder = "piechart#donut";
                break;

            case "gauge":
                galleryFolder = $this.val().toLowerCase();
                break;
        }
        $("#lnk_chartTypeSetup").attr("href", "https://google-developers.appspot.com/chart/interactive/docs/gallery/" + galleryFolder);
    }
}
function UpdateChartColumnSelectors() {
    var chartColumnStr = "";
    for (var i = 0; i < columns.length; i++) {
        chartColumnStr += "<input type='checkbox' id='cb_" + columns[i].realName + "_chartColumn' value='" + columns[i].realName + "' checked='checked' />";
        chartColumnStr += "<label for='cb_" + columns[i].realName + "_chartColumn'>&nbsp;" + columns[i].shownName + "</label><div class='clear-space-five'></div>";
    }
    $("#chart-column-selector-holder").html(chartColumnStr);
}
