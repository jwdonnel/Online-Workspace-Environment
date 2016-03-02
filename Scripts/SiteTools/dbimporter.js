var dbType = "";
var currStepBeforePostback = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();

var columnNameChangeArray = new Array();
prm.add_beginRequest(function () {
    columnNameChangeArray = new Array();
    $("#ImportWizard-element").find(".column-namechange").each(function () {
        var name = $(this).parent().parent().find("input[type='checkbox']").val();
        columnNameChangeArray.push({ "name": name, "value": $(this).val() });
    });

    if ($("#ImportWizard-element").css("display") == "block") {
        if ($("#create-wizard-steps").css("display") == "block") {
            $("#create-wizard-steps").find(".steps").each(function (index) {
                if ($(this).css("display") != "none") {
                    currStepBeforePostback = ".step" + (index + 1);
                }
            });
        }
        else if ($("#summary-wizard-steps").css("display") == "block") {
            $("#summary-wizard-steps").find(".steps").each(function (index) {
                if ($(this).css("display") != "none") {
                    currStepBeforePostback = ".step" + (index + 1);
                }
            });
        }
    }
});
prm.add_endRequest(function () {
    $(document).tooltip({ disabled: false })
    openWSE.RadioButtonStyle();
    $("#lbl_connectionstring").css("display", "block");
    $("#div_connectionstring").css("display", "block");

    if ($("#hf_deleteimport").val() != "") {
        if (dbType == "delete") {
            Confirm_Delete();
        }
    }

    dbType = "";

    EnableDisableColumnNamechange();
    CheckIfConnected();

    AdjustImportModal();

    $("#ImportWizard-element").find(".column-namechange").each(function () {
        var name = $(this).parent().parent().find("input[type='checkbox']").val();
        for (var i = 0; i < columnNameChangeArray.length; i++) {
            if (columnNameChangeArray[i].name == name) {
                $(this).val(columnNameChangeArray[i].value);
                break;
            }
        }
    });

    if (currStepBeforePostback != "") {
        if ($("#ImportWizard-element").css("display") == "block") {
            if ($("#create-wizard-steps").css("display") == "block") {
                $("#create-wizard-steps").find(".steps").hide();
                $("#create-wizard-steps").find(currStepBeforePostback).show();
            }
            else if ($("#summary-wizard-steps").css("display") == "block") {
                $("#summary-wizard-steps").find(".steps").hide();
                $("#summary-wizard-steps").find(currStepBeforePostback).show();
            }
        }
    }

    for (var i = 0 ; i < collapsedData.length; i++) {
        $(".more-table-details[data-id='" + collapsedData[i] + "']").show();
        $(".showhidedetails[data-rowid='" + collapsedData[i] + "']").html("Hide Details");
    }

    UpdateChartColumnSelectors();

    currStepBeforePostback = "";
});


$(document.body).on("change", "#MainContent_cb_ddselect input[type='checkbox']", function () {
    openWSE.LoadingMessage1("Updating...");
    EnableDisableColumnNamechange();
});
function EnableDisableColumnNamechange() {
    var totalChecked = 0;
    var $checkBoxes = $("#MainContent_cb_ddselect").find("input[type='checkbox']");
    $checkBoxes.each(function () {
        if (!$(this).prop("checked")) {
            $(this).parent().find(".column-namechange").hide();
        }
        else {
            $(this).parent().find(".column-namechange").show();
            totalChecked++;
        }
    });

    if (totalChecked == 1) {
        $checkBoxes.each(function () {
            if ($(this).prop("checked")) {
                $(this).prop("disabled", true);
            }
            else {
                $(this).prop("disabled", false);
            }
        });
    }
}

$(document.body).on("change", "#MainContent_cb_InstallAfterLoad", function () {
    if ($(this).is(":checked")) {
        $("#div_isPrivate").show();
    }
    else {
        $("#div_isPrivate").hide();
    }
});
$(document.body).on("click", ".TestConnection", function () {
    openWSE.RemoveUpdateModal();
    $("#MainContent_lbl_error").html("");
    openWSE.LoadingMessage1("Testing Connection...");
});
$(document.body).on("click", ".td-update-btn", function () {
    openWSE.RemoveUpdateModal();
    $("#MainContent_lbl_error").html("");
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

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

        UpdateChartColumnSelectors();
    }

    AdjustImportModal();
}
function UpdateChartColumnSelectors() {
    if ($("#MainContent_rb_adv_enabled").is(":checked")) {
        $("#chart-column-selector").hide();
    }
    else {
        var $this = $("#ddl_ChartType");
        if ($this.val().toLowerCase() != "none") {
            $("#chart-column-selector").show();
            var columnArray = new Array();
            $("#MainContent_cb_ddselect").find("input[type='checkbox']").each(function () {
                var forLabel = $(this).attr("id");
                if ($("label[for='" + forLabel + "']").length > 0) {
                    var name = $.trim($(this).val());
                    var value = $.trim($("label[for='" + forLabel + "']").find(".column-namechange").val());
                    if ($(this).is(":checked")) {
                        columnArray.push({
                            "name": name,
                            "value": value
                        });
                    }
                }
            });

            var chartColumnStr = "<div class='clear-space-five'></div>";
            for (var i = 0; i < columnArray.length; i++) {
                var checked = "";
                for (var jj = 0; jj < chartColumnsSelected.length; jj++) {
                    if (columnArray[i].name == chartColumnsSelected[jj]) {
                        checked = "checked='checked'";
                        break;
                    }
                }

                chartColumnStr += "<input type='checkbox' id='cb_" + columnArray[i].name + "_chartColumn' value='" + columnArray[i].name + "' " + checked + " />";
                chartColumnStr += "<label for='cb_" + columnArray[i].name + "_chartColumn'>&nbsp;" + columnArray[i].value + "</label><div class='clear-space-five'></div>";
            }
            $("#chart-column-selector-holder").html(chartColumnStr);
        }
    }
}

/* On Page load Functions */
$(document).ready(function () {
    openWSE.RadioButtonStyle();
});


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function BeginWork() {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        openWSE.LoadingMessage1("Deleting Import. Please Wait...");
    }, 100);
    $("#hf_StartDelete").val(new Date().toString());
    __doPostBack("hf_StartDelete", "");
}
function OnDelete() {
    openWSE.ConfirmWindow("Are you sure you want to delete this import? App will have to be reapplied to all users if re-imported.",
       function () {
           openWSE.LoadingMessage1("Please Wait...");
           dbType = "delete";
           return r;
       }, 
       function () {
           openWSE.RemoveUpdateModal();
           return r;
       });
}
function CancelRequest() {
    openWSE.LoadModalWindow(false, "password-element", "");
    dbType = "";
    $("#MainContent_tb_passwordConfirm").val("");
}



$(document.body).on("change", "#MainContent_cb_AllowEditAdd", function () {
    if ($(this).prop("checked")) {
        openWSE.ConfirmWindow("Allowing this table to be editable will force you to keep the non nullable columns selected. Do you want to continue?",
       function () {
           $("#tr-usersallowed").show();
           ReDisableSelectedColumns();
           EnableDisableColumnNamechange();
       },
       function () {
           $("#tr-usersallowed").hide();
           ReEnableSelectedColumns();
           EnableDisableColumnNamechange();
           $("#MainContent_cb_AllowEditAdd").prop("checked", false);
       });
    }
    else {
        $("#tr-usersallowed").hide();
        ReEnableSelectedColumns();
        EnableDisableColumnNamechange();
    }
});
function ReDisableSelectedColumns() {
    $(".column-allownull").each(function () {
        if ($.trim($(this).html()) == "false") {
            var $checkbox = $(this).parent().parent().find("input[type='checkbox']");
            $checkbox.prop("disabled", true);
            $checkbox.prop("checked", true);
        }
    });
}
function ReEnableSelectedColumns() {
    $(".column-allownull").each(function () {
        if ($.trim($(this).html()) == "false") {
            var $checkbox = $(this).parent().parent().find("input[type='checkbox']");
            $checkbox.prop("disabled", false);
        }
    });
}

$(document.body).on("click", ".checkbox-new-click, .checkbox-edit-click", function (e) {
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

    UpdateUsersAllowedToEditNew();
});
function UpdateUsersAllowedToEditNew() {
    var usersAllowedToEdit = "";
    $(".checkbox-new-click, .checkbox-edit-click").each(function () {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                usersAllowedToEdit += $input.val().toLowerCase() + ";";
            }
        }
    });

    $("#hf_usersAllowedToEdit").val(usersAllowedToEdit);
}

function ImportTableClick(id) {
    UpdateUsersAllowedToEditNew();
    $(".column-allownull").each(function () {
        if ($.trim($(this).html()) == "false") {
            var $checkbox = $(this).parent().parent().find("input[type='checkbox']");
            $checkbox.prop("disabled", true);
            $checkbox.prop("checked", true);
        }
    });

    var columnOverrides = "";
    $("#MainContent_cb_ddselect").find("input[type='checkbox']").each(function () {
        if ($(this).prop("checked")) {
            var value = $.trim($(this).parent().find(".column-namechange").val());
            if (value == "") {
                value = $(this).val();
            }

            columnOverrides += $(this).val() + "=" + value + ";";
        }
    });

    $("#hf_customizations").val(BuildCustomizationString());
    $("#hf_columnOverrides").val(columnOverrides);

    if ($("#btn_finishImportWizard").val() != "Update") {
        $("#hf_importClick").val(new Date().toString());
        __doPostBack("hf_importClick", "");
    }
    else {
        $("#hf_updateimport").val(id);
        __doPostBack("hf_updateimport", "");
    }
}
function EditEntry(id) {
    openWSE.LoadingMessage1("Please Wait...");
    $("#hf_editimport").val(id);
    __doPostBack("hf_editimport", "");
}
function RecreateApp(id) {
    openWSE.LoadingMessage1("Creating App...");
    $("#hf_createAppImport").val(id);
    __doPostBack("hf_createAppImport", "");
}
function DeleteEntry(id, name) {
    dbType = "delete";
    $("#hf_deleteimport").val(id);
    Confirm_Delete();
}
function Confirm_Delete() {
    if (UserIsSocialAccount != null && UserIsSocialAccount) {
        $("#MainContent_btn_passwordConfirm").trigger("click");
    }
    else {
        openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
        $("#MainContent_tb_passwordConfirm").focus();
    }
}


/* Import Wizard */
var testConnectionGood = false;
function StartImportWizard() {
    $("#error_onupdatecreate").html("");
    $(".import-steps").find("li").hide();
    $(".import-steps").find("#step1").show();
    $("#btn_finishImportWizard, .prev-step").hide();
    $(".next-step").show();
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    if ($("#ddl_ChartType").val() == "None") {
        $("#chart-title-holder").hide();
        $("#chart-column-selector").hide();
        $("#img_charttype_holder").hide();
    }
    else {
        $("#chart-title-holder").show();
        $("#chart-column-selector").show();
        $("#img_charttype_holder").show();
    }

    ResetImportWizardControls();

    $("#div_installAfterLoad").show();
    $("#div_isPrivate").show();

    $("#ImportWizard-element").find(".Modal-element-modal").css("top", "auto");
    $("#ImportWizard-element").find(".Modal-element-modal").css("left", "auto");

    $("#btn_finishImportWizard").val("Import");
    $("#btn_finishImportWizard").attr("onclick", "ImportTableClick();");

    openWSE.LoadModalWindow(true, "ImportWizard-element", "Table Import Wizard");
}
function LoadEditImportWizard(name, description, selectStatement, connString, provider, customizations, allowEdit, notifyUsers, id) {
    $("#error_onupdatecreate").html("");
    $(".import-steps").find("li").hide();
    $(".import-steps").find("#step1").show();
    $("#btn_finishImportWizard, .prev-step").hide();
    $(".next-step").show();
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    if ($("#ddl_ChartType").val() == "None") {
        $("#chart-title-holder").hide();
        $("#chart-column-selector").hide();
        $("#img_charttype_holder").hide();
    }
    else {
        $("#chart-title-holder").show();
        $("#chart-column-selector").show();
        $("#img_charttype_holder").show();
    }

    $("#ImportWizard-element").find(".Modal-element-modal").css("top", "auto");
    $("#ImportWizard-element").find(".Modal-element-modal").css("left", "auto");

    $("#div_installAfterLoad").hide();
    $("#div_isPrivate").hide();

    $("#MainContent_tb_Databasename").val(unescape(name));
    $("#tb_description").val(unescape(description));
    $("#MainContent_tb_selectcomm").val(unescape(selectStatement));
    SetCustomizations(unescape(customizations));
    $("#btn_finishImportWizard").val("Update");
    $("#btn_finishImportWizard").attr("onclick", "ImportTableClick('" + id + "');");

    $("#cb_allowNotifi").prop("checked", notifyUsers);
    $("#MainContent_cb_AllowEditAdd").prop("checked", allowEdit);
    if (allowEdit) {
        $("#tr-usersallowed").show();
    }

    openWSE.LoadModalWindow(true, "ImportWizard-element", "Edit " + name);
}
function ConfirmCancelWizard() {
    openWSE.ConfirmWindow("Are you sure you want to cancel the wizard import?", function () {
        openWSE.LoadModalWindow(false, "ImportWizard-element", "");
        openWSE.LoadingMessage1("Cancelling...");
        $("#hf_cancelWizard").val(new Date().toString());
        __doPostBack("hf_cancelWizard", "");
    }, null);
}
function CreateNextStep() {
    var currentStep = 1;
    for (var i = 0; i < $(".import-steps").find("li").length; i++) {
        if ($(".import-steps").find("li").eq(i).css("display") != "none") {
            currentStep = i + 2;
            break;
        }
    }

    $(".import-steps").find("li").hide();
    $(".import-steps").find("#step" + currentStep).show();

    $("#btn_finishImportWizard").hide();
    $(".next-step, .prev-step").show();

    if (i >= $(".import-steps").find("li").length - 2) {
        $(".next-step").hide();
        $("#btn_finishImportWizard").show();
    }

    AdjustImportModal();
    CheckIfConnected();
    UpdateChartColumnSelectors();
}
function CreatePrevStep() {
    var currentStep = 1;
    for (var i = 0; i < $(".import-steps").find("li").length; i++) {
        if ($(".import-steps").find("li").eq(i).css("display") != "none") {
            currentStep = i;
            break;
        }
    }

    $(".import-steps").find("li").hide();
    $(".import-steps").find("#step" + currentStep).show();

    $("#btn_finishImportWizard").hide();
    $(".next-step, .prev-step").show();

    if (i == 1) {
        $(".prev-step").hide();
    }

    var top = $("#ImportWizard-element").find(".Modal-element-modal").css("top");
    if (top == "auto") {
        $("#ImportWizard-element").find(".Modal-element-align").css({
            marginTop: -($("#ImportWizard-element").find(".Modal-element-modal").height() / 2),
            marginLeft: -($("#ImportWizard-element").find(".Modal-element-modal").width() / 2)
        });
    }

    CheckIfConnected();
    UpdateChartColumnSelectors();
}
function CheckIfConnected() {
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    for (var i = 0; i < $(".import-steps").find("li").length; i++) {
        if ($(".import-steps").find("li").eq(i).css("display") != "none" && $(".import-steps").find("li").eq(i).attr("id") == "step4") {
            if (!testConnectionGood) {
                $(".next-step").prop("disabled", true);
                $(".next-step").addClass("next-disabled");
            }
            break;
        }
    }
}
function AdjustImportModal() {
    if ($("#ImportWizard-element").css("display") == "block") {
        var top = $("#ImportWizard-element").find(".Modal-element-modal").css("top");
        if (top == "auto") {
            $("#ImportWizard-element").find(".Modal-element-align").css({
                marginTop: -($("#ImportWizard-element").find(".Modal-element-modal").height() / 2),
                marginLeft: -($("#ImportWizard-element").find(".Modal-element-modal").width() / 2)
            });
        }
    }
}
function ResetImportWizardControls() {
    testConnectionGood = false;
    chartColumnsSelected = new Array();
    $("#MainContent_tb_Databasename").val("");
    $("#tb_description").val("");

    $("#dd_fontfamilycustomization").val("");
    $("#span_fontfamilypreview").css("font-family", "");

    $("#tb_tableheadercolor").val("FFFFFF");
    $("#tb_tableheadercolor").css("background-color", "#FFFFFF");
    $("#tb_tableheadercolor").css("color", "#000");
    $("#tb_tableheadercolor").prop("disabled", true);
    $("#cb_usedefaultheadercolor").prop("checked", true);

    $("#tb_primaryrowcolor").val("FFFFFF");
    $("#tb_primaryrowcolor").css("background-color", "#FFFFFF");
    $("#tb_primaryrowcolor").css("color", "#000");
    $("#tb_primaryrowcolor").prop("disabled", true);
    $("#cb_usedefaultprimarycolor").prop("checked", true);

    $("#tb_alternativerowcolor").val("FFFFFF");
    $("#tb_alternativerowcolor").css("background-color", "#FFFFFF");
    $("#tb_alternativerowcolor").css("color", "#000");
    $("#tb_alternativerowcolor").prop("disabled", true);
    $("#cb_usedefaultalternativecolor").prop("checked", true);

    $("#cb_allowNotifi").prop("checked", true);
    $("#MainContent_cb_AllowEditAdd").prop("checked", false);
    $("#tr-usersallowed").hide();

    $("#ddl_ChartType").val("None");
    $("#img_charttype_holder").hide();
    $("#tb_chartTitle").val("");
    $("#chart-title-holder").hide();
    $("#chart-column-selector").hide();
}


/* Add New Connection Modal */
function CloseAddConnectionModal() {
    openWSE.LoadModalWindow(false, "AddConnectionString-Element", "");
    $("#MainContent_tb_connectionname").val("Connection Name");
    $("#MainContent_tb_connectionstring").val("Connection String");
    $("#savedconnections_postmessage").html("");
}


/* Customizations */
var chartColumnsSelected = new Array();
function fontFamilyChange() {
    var summaryCtrl = "";
    if ($("#create-wizard-steps").css("display") == "none") {
        summaryCtrl = "Summary";
    }

    $("#span_fontfamilypreview" + summaryCtrl).css("font-family", $("#dd_fontfamilycustomization" + summaryCtrl).val());
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
function BuildCustomizationString() {
    var customizationArray = new Array();
    var customizeName = "";
    var customizeValue = "";

    var summaryCtrl = "";
    if ($("#create-wizard-steps").css("display") == "none") {
        summaryCtrl = "Summary";
    }

    // HeaderColor
    customizeName = "HeaderColor";
    if (!$("#cb_usedefaultheadercolor" + summaryCtrl).prop("checked")) {
        customizeValue = $.trim($("#tb_tableheadercolor" + summaryCtrl).val());
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
    if (!$("#cb_usedefaultprimarycolor" + summaryCtrl).prop("checked")) {
        customizeValue = $.trim($("#tb_primaryrowcolor" + summaryCtrl).val());
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
    if (!$("#cb_usedefaultalternativecolor" + summaryCtrl).prop("checked")) {
        customizeValue = $.trim($("#tb_alternativerowcolor" + summaryCtrl).val());
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
    customizeValue = $.trim($("#dd_fontfamilycustomization" + summaryCtrl).val());
    customizationArray.push({
        "customizeName": customizeName,
        "customizeValue": customizeValue
    });

    if (summaryCtrl == "") {
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
    }

    return JSON.stringify(customizationArray);
}
function SetCustomizations(customizationStr) {
    try {
        chartColumnsSelected = new Array();

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
                    chartColumnsSelected = value.split(';');
                    var chartColumnStr = "";

                    var columns = new Array();
                    $("#MainContent_cb_ddselect").find("input[type='checkbox']").each(function () {
                        var forLabel = $(this).attr("id");
                        if ($("label[for='" + forLabel + "']").length > 0) {
                            var name = $.trim($(this).val());
                            var value = $.trim($("label[for='" + forLabel + "']").find(".column-namechange").val());
                            if ($(this).is(":checked")) {
                                columns.push({
                                    "realName": name,
                                    "shownName": value
                                });
                            }
                        }
                    });

                    for (var ii = 0; ii < columns.length; ii++) {
                        var checked = "";
                        for (var jj = 0; jj < chartColumnsSelected.length; jj++) {
                            if (columns[ii].realName == chartColumnsSelected[jj]) {
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


var collapsedData = new Array();
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

$(document.body).on("change", "#MainContent_dd_ddtables", function () {
    openWSE.LoadingMessage1("Updating...");
});

function UseConnectionString(x, y) {
    $("#MainContent_hf_usestring").val("Use connection string " + y + " - '" + x + "'");
    openWSE.LoadingMessage1("Updating...");
    __doPostBack("MainContent_hf_usestring", "");
}
function DeleteConnectionString(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this connection string?",
       function () {
           $("#MainContent_hf_deletestring").val(x);
           openWSE.LoadingMessage1("Updating...");
           __doPostBack("MainContent_hf_deletestring", "");
       }, null);
}
function EditConnectionString(x) {
    $("#MainContent_hf_editstring").val(x);
    openWSE.LoadingMessage1("Loading...");
    __doPostBack("MainContent_hf_editstring", "");
}
function UpdateConnectionString(x) {
    $("#MainContent_hf_updatestring").val(x);
    $("#MainContent_hf_connectionNameEdit").val(escape($("#tb_connNameedit").val()));
    $("#MainContent_hf_connectionStringEdit").val(escape($("#tb_connStringedit").val()));
    $("#MainContent_hf_databaseProviderEdit").val(escape($("#edit-databaseProvider").val()));
    openWSE.LoadingMessage1("Updating...");
    __doPostBack("MainContent_hf_updatestring", "");
}
function KeyPressEdit_Connection(event, x) {
    try {
        if (event.which == 13) {
            event.preventDefault();
            UpdateConnectionString(x);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            event.preventDefault();
            UpdateConnectionString(x);
        }
        delete evt;
    }
}
