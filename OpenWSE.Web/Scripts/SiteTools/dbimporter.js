var dbType = "";
var currStepBeforePostback = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
var uploadFiles = null;

var beforePostBackChecklist = new Array();
var columnNameChangeArray = new Array();
var pnl_ImportedTablesStr = "";
prm.add_beginRequest(function (sender, args) {
    beforePostBackChecklist = new Array();
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

    beforePostBackChecklist.push({
        "installApp": $("#MainContent_cb_InstallAfterLoad").prop("checked"),
        "private": $("#cb_isPrivate").prop("checked"),
        "notifications": $("#cb_allowNotifi").prop("checked"),
        "editable": $("#MainContent_cb_AllowEditAdd").prop("checked"),
        "usersAllowed": $.trim($("#MainContent_pnl_usersAllowedToEdit").html())
    });

    var elem = args.get_postBackElement();
    if (elem && elem.id === "hf_UpdateAll") {
        pnl_ImportedTablesStr = $.trim($("#MainContent_pnl_ImportedTables").html());
    }

    var files = $("#MainContent_fu_image_create").get(0).files;
    if (files.length > 0) {
        uploadFiles = files;
    }
});
prm.add_endRequest(function () {
    $(document).tooltip({ disabled: false })
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

    if (beforePostBackChecklist.length > 0) {
        $("#MainContent_cb_InstallAfterLoad").prop("checked", beforePostBackChecklist[0].installApp);
        $("#cb_isPrivate").prop("checked", beforePostBackChecklist[0].private);

        $("#div_isPrivate").hide();
        if ($("#MainContent_cb_InstallAfterLoad").is(":checked") && $("#div_installAfterLoad").css("display") != "none") {
            $("#div_isPrivate").show();
        }

        $("#cb_allowNotifi").prop("checked", beforePostBackChecklist[0].notifications);
        $("#MainContent_cb_AllowEditAdd").prop("checked", beforePostBackChecklist[0].editable);

        if (beforePostBackChecklist[0].usersAllowed) {
            $("#MainContent_pnl_usersAllowedToEdit").html(beforePostBackChecklist[0].usersAllowed);
        }

        $("#tr-usersallowed").hide();
        if (beforePostBackChecklist[0].editable) {
            $("#tr-usersallowed").show();
        }
    }

    beforePostBackChecklist = new Array();
    SetCustomizations();
    InitializeSortColumns();
    UpdateChartColumnSelectors();
    UpdateSummaryColumnDropdown();

    currStepBeforePostback = "";

    if (pnl_ImportedTablesStr != "") {
        $("#MainContent_pnl_ImportedTables").html(pnl_ImportedTablesStr);
    }

    pnl_ImportedTablesStr = "";
});


$(document.body).on("change", "#MainContent_cb_ddselect input[type='checkbox']", function () {
    loadingPopup.Message("Updating...");
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
    loadingPopup.RemoveMessage();
    $("#MainContent_lbl_error").html("");
    loadingPopup.Message("Testing Connection...");
});
$(document.body).on("click", ".td-update-btn", function () {
    loadingPopup.RemoveMessage();
    $("#MainContent_lbl_error").html("");
    loadingPopup.Message("Updating. Please Wait...");
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
            var columnArray = GetColumnsArray();

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


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function BeginWork() {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        loadingPopup.Message("Deleting Import. Please Wait...");
    }, 100);
    $("#hf_StartDelete").val(new Date().toString());
    openWSE.CallDoPostBack("hf_StartDelete", "");
}
function OnDelete() {
    openWSE.ConfirmWindow("Are you sure you want to delete this import? App will have to be reapplied to all users if re-imported.",
       function () {
           loadingPopup.Message("Please Wait...");
           dbType = "delete";
           return r;
       }, 
       function () {
           loadingPopup.RemoveMessage();
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
    $("#hf_summaryData").val(JSON.stringify(summaryColumns));

    if ($("#btn_finishImportWizard").val() != "Update") {
        $("#hf_importClick").val(new Date().toString());
        openWSE.CallDoPostBack("hf_importClick", "");
    }
    else {
        $("#hf_updateimport").val(id);
        openWSE.CallDoPostBack("hf_updateimport", "");
    }
}
function EditEntry(id) {
    loadingPopup.Message("Please Wait...");
    $("#hf_editimport").val(id);
    openWSE.CallDoPostBack("hf_editimport", "");
}
function RecreateApp(id) {
    loadingPopup.Message("Creating App...");
    $("#hf_createAppImport").val(id);
    openWSE.CallDoPostBack("hf_createAppImport", "");
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
function FinishEditCreate(id) {
    try {
        var fd = new FormData();
        if (uploadFiles && uploadFiles.length > 0 && fd) {
            fd.append("TableID", id);
            fd.append("TableType", "TableImporter");
            fd.append("Filedata", uploadFiles[0]);
            var xmlHttpReq = openWSE.CreateXMLHttpRequest();
            if (xmlHttpReq != null) {
                xmlHttpReq.addEventListener("load", function () {
                    finishTableUpdate(true);
                }, false);
                xmlHttpReq.open("POST", openWSE.siteRoot() + "Handlers/AppIconUpload.ashx");
                xmlHttpReq.send(fd);
            }
        }
        else if ($.trim($("#MainContent_tb_imageurl").val()) !== "") {
            openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/UploadAppIcon_Url", "{ 'file': '" + escape($.trim($("#MainContent_tb_imageurl").val())) + "', 'tableId': '" + escape(id) + "', 'TableType': 'TableImporter' }", {
                cache: false
            }, null, null, function () {
                finishTableUpdate(true);
            });
        }
        else {
            finishTableUpdate(false);
        }
    }
    catch (evt) {
        finishTableUpdate(false);
    }
}
function finishTableUpdate(postback) {
    uploadFiles = null;
    $("#MainContent_fu_image_create").val("");
    $("#MainContent_fu_image_create").get(0).files = null;
    openWSE.LoadModalWindow(false, "ImportWizard-element", "");

    if (postback) {
        $("#hf_updateList").val(new Date().toString());
        openWSE.CallDoPostBack("hf_updateList", "");
    }
}



/* Import Wizard */
var testConnectionGood = false;
function StartImportWizard() {
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
    InitializeSortColumns();
    openWSE.LoadModalWindow(true, "ImportWizard-element", "Table Import Wizard");
}
function LoadEditImportWizard(name, description, selectStatement, connString, provider, customizations, allowEdit, notifyUsers, id, summaryData) {
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

    $("#tb_summaryname").val("");
    $("#dd_formulatype").val("Sum");
    $("#table_columns_summary").html("");
    AddIDRowSummary();

    SetDefaultCustomizations();

    $("#table_addcolumn_summary_Controls").show();
    $(".view-edit-condition").hide();

    $("#ImportWizard-element").find(".Modal-element-modal").css("top", "auto");
    $("#ImportWizard-element").find(".Modal-element-modal").css("left", "auto");

    $("#div_installAfterLoad").hide();
    $("#div_isPrivate").hide();

    $("#MainContent_tb_Databasename").val(unescape(name));
    $("#tb_description").val(unescape(description));
    $("#MainContent_tb_selectcomm").val(unescape(selectStatement));

    customizationStr = unescape(customizations);
    SetCustomizations();

    RebuildDefaultValuesTable();

    try {
        summaryColumns = $.parseJSON(unescape(summaryData));
    } catch (evt2) { }

    AddIDRowSummary();
    for (var i = 0; i < summaryColumns.length; i++) {
        BuildSummaryColumn(summaryColumns[i], false, false, i + 1);
    }

    InitializeSortColumns();
    UpdateSummaryColumnDropdown();

    $("#btn_finishImportWizard").val("Update");
    $("#btn_finishImportWizard").attr("onclick", "ImportTableClick('" + id + "');");

    $("#cb_allowNotifi").prop("checked", notifyUsers);
    $("#MainContent_cb_AllowEditAdd").prop("checked", allowEdit);
    if (allowEdit) {
        $("#tr-usersallowed").show();
    }

    beforePostBackChecklist = new Array();
    beforePostBackChecklist.push({
        "installApp": $("#MainContent_cb_InstallAfterLoad").prop("checked"),
        "private": $("#cb_isPrivate").prop("checked"),
        "notifications": $("#cb_allowNotifi").prop("checked"),
        "editable": $("#MainContent_cb_AllowEditAdd").prop("checked"),
        "usersAllowed": $.trim($("#MainContent_pnl_usersAllowedToEdit").html())
    });

    openWSE.LoadModalWindow(true, "ImportWizard-element", "Edit " + name);
}
function ConfirmCancelWizard() {
    openWSE.ConfirmWindow("Are you sure you want to cancel the wizard import?", function () {
        openWSE.LoadModalWindow(false, "ImportWizard-element", "");
        loadingPopup.Message("Cancelling...");
        $("#hf_cancelWizard").val(new Date().toString());
        openWSE.CallDoPostBack("hf_cancelWizard", "");
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
    UpdateSummaryColumnDropdown();
    RebuildDefaultValuesTable();
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

    AdjustImportModal();
    CheckIfConnected();
    UpdateChartColumnSelectors();
    UpdateSummaryColumnDropdown();
    RebuildDefaultValuesTable();
}
function CheckIfConnected() {
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    for (var i = 0; i < $(".import-steps").find("li").length; i++) {
        if ($(".import-steps").find("li").eq(i).css("display") != "none" && $(".import-steps").find("li").eq(i).attr("id") == "step2") {
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
        if (top == "auto" || top == "0px") {
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
    customizationStr = "";
    $("#MainContent_tb_Databasename").val("");
    $("#tb_description").val("");

    $("#dd_fontfamilycustomization").val("");
    $("#span_fontfamilypreview").css("font-family", "");

    $("#tb_tableheadercolor").val("#FFFFFF");
    $("#tb_tableheadercolor").prop("disabled", true);
    $("#cb_usedefaultheadercolor").prop("checked", true);

    $("#tb_primaryrowcolor").val("#FFFFFF");
    $("#tb_primaryrowcolor").prop("disabled", true);
    $("#cb_usedefaultprimarycolor").prop("checked", true);

    $("#tb_alternativerowcolor").val("#FFFFFF");
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

    $("#tb_summaryname").val("");
    $("#dd_formulatype").val("Sum");
    $("#table_columns_summary").html("");
    AddIDRowSummary();

    $("#table_addcolumn_summary_Controls").show();
    $(".view-edit-condition").hide();
    InitializeSortColumns();

    SetDefaultCustomizations();

    defaultValues = new Array();
}


/* Customizations */
var chartColumnsSelected = new Array();
var customizationStr = "";
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

    // Default Values
    customizationArray.push({
        "customizeName": "DefaultValues",
        "customizeValue": JSON.stringify(defaultValues)
    });

    // Show Row Counts
    customizationArray.push({
        "customizeName": "ShowRowCounts",
        "customizeValue": $("#rb_showrowcounts_on").prop("checked").toString().toLowerCase()
    });

    // Show Description on App
    customizationArray.push({
        "customizeName": "ShowDescriptionOnApp",
        "customizeValue": $("#rb_showdescription_on").prop("checked").toString().toLowerCase()
    });

    // App Style Background Color
    customizeName = "AppStyleBackgroundColor";
    if (!$("#cb_appbackground_color_default").prop("checked")) {
        customizeValue = $.trim($("#tb_appbackground_color").val());
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

    // App Style Title Color
    customizeName = "AppStyleTitleColor";
    if (!$("#cb_apptitle_color_default").prop("checked")) {
        customizeValue = $.trim($("#tb_apptitle_color").val());
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

    // App Style Background Image
    customizationArray.push({
        "customizeName": "AppStyleBackgroundImage",
        "customizeValue": $.trim($("#tb_appbackground_image").val())
    });

    // Table View Style
    customizationArray.push({
        "customizeName": "TableViewStyle",
        "customizeValue": $.trim($("#dd_viewstyle").val())
    });

    return JSON.stringify(customizationArray);
}
function SetDefaultCustomizations() {
    $("#rb_showrowcounts_on").prop("checked", true);
    $("#rb_showrowcounts_off").prop("checked", false);

    $("#rb_showdescription_on").prop("checked", false);
    $("#rb_showdescription_off").prop("checked", true);

    openWSE.RadioButtonStyle();

    $("#tb_appbackground_color").prop("disabled", true);
    $("#tb_appbackground_color").val("#FFFFFF");
    $("#cb_appbackground_color_default").prop("checked", true);

    $("#tb_apptitle_color").prop("disabled", true);
    $("#tb_apptitle_color").val("#FFFFFF");
    $("#cb_apptitle_color_default").prop("checked", true);

    $("#dd_viewstyle").val("default");
    $("#MainContent_tb_imageurl").val("");

    if (!uploadFiles || uploadFiles.length == 0) {
        $("#MainContent_fu_image_create").val("");
        $("#MainContent_fu_image_create").get(0).files = null;
    }

    ChangeIconUploadType(1);
}
function SetCustomizations() {
    try {
        chartColumnsSelected = new Array();

        if (!customizationStr) {
            return;
        }

        var customizations = $.parseJSON(customizationStr);
        for (var i = 0; i < customizations.length; i++) {
            var value = customizations[i].customizeValue;

            switch (customizations[i].customizeName) {
                case "HeaderColor":
                    if (value != "") {
                        $("#cb_usedefaultheadercolor").prop("checked", false);
                        $("#tb_tableheadercolor").prop("disabled", false);
                        $("#tb_tableheadercolor").val(value);
                    }
                    else {
                        $("#cb_usedefaultheadercolor").prop("checked", true);
                    }
                    break;

                case "PrimaryRowColor":
                    if (value != "") {
                        $("#cb_usedefaultprimarycolor").prop("checked", false);
                        $("#tb_primaryrowcolor").prop("disabled", false);
                        $("#tb_primaryrowcolor").val(value);
                    }
                    else {
                        $("#cb_usedefaultprimarycolor").prop("checked", true);
                    }
                    break;

                case "AlternativeRowColor":
                    if (value != "") {
                        $("#cb_usedefaultalternativecolor").prop("checked", false);
                        $("#tb_alternativerowcolor").prop("disabled", false);
                        $("#tb_alternativerowcolor").val(value);
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

                case "ShowRowCounts":
                    if (openWSE.ConvertBitToBoolean(value)) {
                        $("#rb_showrowcounts_on").prop("checked", true);
                        $("#rb_showrowcounts_off").prop("checked", false);
                        $("#rb_showrowcounts_on").parent().addClass("selected");
                        $("#rb_showrowcounts_off").parent().removeClass("selected");
                    }
                    else {
                        $("#rb_showrowcounts_on").prop("checked", false);
                        $("#rb_showrowcounts_off").prop("checked", true);
                        $("#rb_showrowcounts_on").parent().removeClass("selected");
                        $("#rb_showrowcounts_off").parent().addClass("selected");
                    }
                    openWSE.RadioButtonStyle();
                    break;

                case "DefaultValues":
                    defaultValues = new Array();
                    if (value) {
                        try {
                            defaultValues = $.parseJSON(value);
                        }
                        catch (evt1) { }
                    }
                    break;

                case "AppStyleBackgroundColor":
                    if (value != "") {
                        $("#cb_appbackground_color_default").prop("checked", false);
                        $("#tb_appbackground_color").prop("disabled", false);
                        $("#tb_appbackground_color").val(value);
                    }
                    else {
                        $("#cb_appbackground_color_default").prop("checked", true);
                    }
                    break;

                case "AppStyleTitleColor":
                    if (value != "") {
                        $("#cb_apptitle_color_default").prop("checked", false);
                        $("#tb_apptitle_color").prop("disabled", false);
                        $("#tb_apptitle_color").val(value);
                    }
                    else {
                        $("#cb_apptitle_color_default").prop("checked", true);
                    }
                    break;

                case "AppStyleBackgroundImage":
                    $("#tb_appbackground_image").val(value);
                    break;

                case "ShowDescriptionOnApp":
                    if (openWSE.ConvertBitToBoolean(value)) {
                        $("#rb_showdescription_on").prop("checked", true);
                        $("#rb_showdescription_off").prop("checked", false);
                        $("#rb_showdescription_on").parent().addClass("selected");
                        $("#rb_showdescription_off").parent().removeClass("selected");
                    }
                    else {
                        $("#rb_showdescription_on").prop("checked", false);
                        $("#rb_showdescription_off").prop("checked", true);
                        $("#rb_showdescription_on").parent().removeClass("selected");
                        $("#rb_showdescription_off").parent().addClass("selected");
                    }
                    openWSE.RadioButtonStyle();
                    break;

                case "TableViewStyle":
                    $("#dd_viewstyle").val(value);
                    break;
            }
        }
    }
    catch (evt) { }
}

function ShowHideTableDetails(id, name) {
    openWSE.LoadModalWindow(true, id + "Modal-element", name + " Details");
}
function CloseTableDetailsModal(id) {
    openWSE.LoadModalWindow(false, id + "Modal-element", "");
}

$(document.body).on("change", "#MainContent_dd_ddtables", function () {
    loadingPopup.Message("Updating...");
});

function UseConnectionString(x, y) {
    $("#MainContent_hf_usestring").val("Use connection string " + y + " - '" + x + "'");
    loadingPopup.Message("Updating...");
    openWSE.CallDoPostBack("MainContent_hf_usestring", "");
}
function DeleteConnectionString(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this connection string?",
       function () {
           $("#MainContent_hf_deletestring").val(x);
           loadingPopup.Message("Updating...");
           openWSE.CallDoPostBack("MainContent_hf_deletestring", "");
       }, null);
}
function EditConnectionString(x) {
    $("#MainContent_hf_editstring").val(x);
    loadingPopup.Message("Loading...");
    openWSE.CallDoPostBack("MainContent_hf_editstring", "");
}
function UpdateConnectionString(x) {
    $("#MainContent_hf_updatestring").val(x);
    $("#MainContent_hf_connectionNameEdit").val(escape($.trim($("#tb_connNameedit").val())));
    $("#MainContent_hf_connectionStringEdit").val(escape($.trim($("#tb_connStringedit").val())));
    $("#MainContent_hf_databaseProviderEdit").val(escape($.trim($("#edit-databaseProvider").val())));
    loadingPopup.Message("Updating...");
    openWSE.CallDoPostBack("MainContent_hf_updatestring", "");
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
function KeyPress_AddConnectionString(event) {
    try {
        if (event.which == 13) {
            event.preventDefault();
            AddConnectionString_Clicked();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            event.preventDefault();
            AddConnectionString_Clicked();
        }
        delete evt;
    }
}
function AddConnectionString_Clicked() {
    if ($.trim($("#tb_connectionname").val()) !== "" && $.trim($("#tb_connectionstring").val()) !== "") {
        $("#MainContent_hf_addconnectionstring").val(new Date().toString());
        $("#MainContent_hf_connectionNameEdit").val(escape($.trim($("#tb_connectionname").val())));
        $("#MainContent_hf_connectionStringEdit").val(escape($.trim($("#tb_connectionstring").val())));
        $("#MainContent_hf_databaseProviderEdit").val(escape($.trim($("#dd_provider_connectionstring").val())));
        loadingPopup.Message("Saving...");
        openWSE.CallDoPostBack("MainContent_hf_addconnectionstring", "");
    }
}


/* Summary Controls */
var summaryColumns = new Array();
function AddIDRowSummary() {
    var columnSummary = "<tr class='myHeaderStyle'>";
    columnSummary += "<td width='55px'></td>";
    columnSummary += "<td>Summary Name</td>";
    columnSummary += "<td width='200px'>Column</td>";
    columnSummary += "<td width='135px'>Formula</td>";
    columnSummary += "<td class='edit-column-2-items' align='center'></td>";
    columnSummary += "</tr>";

    if ($("#table_columns_summary").find("tbody").length == 0) {
        $("#table_columns_summary").html("<tbody></tbody>");
    }
    $("#table_columns_summary").find("tbody").html(columnSummary);
}
function UpdateSummaryColumnDropdown() {
    var columnArray = GetColumnsArray();

    var optionList = "";
    for (var i = 0; i < columnArray.length; i++) {
        optionList += "<option value='" + columnArray[i].name + "'>" + columnArray[i].value + "</option>";
    }

    $("#dd_columnSummary").html(optionList);

    if ($("#dd_columnSummary_edit").length > 0) {
        var tempVal = $("#dd_columnSummary_edit").val();
        $("#dd_columnSummary_edit").html("");
        var optionList = "";
        for (var i = 0; i < columnArray.length; i++) {
            optionList += "<option value='" + columnArray[i].name + "'>" + columnArray[i].value + "</option>";
        }

        $("#dd_columnSummary_edit").html(optionList);
        $("#dd_columnSummary_edit").val(tempVal);
    }
}
function AddNewSummaryColumn() {
    var len = summaryColumns.length;
    var summaryName = $.trim($("#tb_summaryname").val());

    if (summaryName == "") {
        openWSE.AlertWindow("Summary name cannot be blank. Please enter a name and try again.");
    }
    else {
        var canContinue = true;
        for (var i = 0; i < len; i++) {
            if ((summaryColumns[i].summaryName == summaryName)) {
                canContinue = false;
                openWSE.AlertWindow("Summary name already exists! Please use a different name.");
                break;
            }
        }

        if (canContinue) {
            var formulaType = $("#dd_formulatype").val();
            if (formulaType.toLowerCase().indexOf("if") > 0) {
                formulaType += "(";
                if ($("#tb_formulaCondition").length > 0) {
                    formulaType += escape($.trim($("#tb_formulaCondition").val()));
                }
                formulaType += ")";
            }

            summaryColumns[len] = {
                "summaryName": summaryName,
                "columnName": $("#dd_columnSummary").val(),
                "formulaType": formulaType
            };

            BuildSummaryColumn(summaryColumns[len], false, false, len + 1);
            $("#tb_summaryname").focus();
        }
    }
}
function AddNewSummaryRowTextBoxKeyDown(event) {
    try {
        if (event.which == 13) {
            AddNewSummaryColumn();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddNewSummaryColumn();
        }
        delete evt;
    }
}
function EditSummaryControls(index) {
    $("#table_columns_summary tbody").sortable("disable");
    AddIDRowSummary();
    index = (parseInt(index) - 1);

    var isEdit = false;
    for (var i = 0; i < summaryColumns.length; i++) {
        if ((summaryColumns[i].summaryName == summaryColumns[index].summaryName)) {
            BuildSummaryColumn(summaryColumns[i], true, true, i + 1);
            isEdit = true;
        }
        else {
            BuildSummaryColumn(summaryColumns[i], true, false, i + 1);
        }
    }

    $("#table_addcolumn_summary_Controls").show();
    if (isEdit) {
        $("#table_addcolumn_summary_Controls").hide();
    }
}
function FinishEditSummary(index, cancelled) {
    var canContinue = true;
    index = (parseInt(index) - 1);
    if (!cancelled) {
        var summaryName = $.trim($("#tb_summaryname_edit").val());
        for (var i = 0; i < summaryColumns.length; i++) {
            if ((summaryColumns[i].summaryName == summaryName) && (index != i)) {
                canContinue = false;
                openWSE.AlertWindow("Summary name already exists! Please use a different name.");
                break;
            }
        }

        if (canContinue) {
            var formulaType = $("#dd_formulatype_edit").val();
            if (formulaType.toLowerCase().indexOf("if") > 0) {
                formulaType += "(";
                if ($("#tb_formulaCondition_edit").length > 0) {
                    formulaType += escape($.trim($("#tb_formulaCondition_edit").val()));
                }
                formulaType += ")";
            }

            summaryColumns[index].summaryName = summaryName;
            summaryColumns[index].columnName = $("#dd_columnSummary_edit").val();
            summaryColumns[index].formulaType = formulaType;
        }
    }

    $("#table_addcolumn_summary_Controls").show();

    if (canContinue) {
        $("#table_columns_summary tbody").sortable("enable");
        AddIDRowSummary();
        for (var i = 0; i < summaryColumns.length; i++) {
            BuildSummaryColumn(summaryColumns[i], false, false, i + 1);
        }
    }
}
function FinishEditSummaryKeyDown(event, index) {
    try {
        if (event.which == 13) {
            FinishEditSummary(index, false);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            FinishEditSummary(index, false);
        }
        delete evt;
    }
}
function DeleteSummaryRow(index) {
    openWSE.ConfirmWindow("Are you sure you want to delete this summary column?",
        function () {
            index = (parseInt(index) - 1);
            summaryColumns.splice(index, 1);
            if (summaryColumns.length == 0) {
                summaryColumns = new Array();
            }

            AddIDRowSummary();
            for (var i = 0; i < summaryColumns.length; i++) {
                BuildSummaryColumn(summaryColumns[i], false, false, i + 1);
            }

            UpdateSummaryColumnDropdown();
        }, null);
}
var fixHelper = function (e, ui) {
    ui.children().each(function () {
        $(this).width($(this).width());
    });
    return ui;
};
function InitializeSortColumns() {
    $("#table_columns_summary tbody").sortable({
        cancel: ".entryIDRow, .border-right, .myHeaderStyle",
        helper: fixHelper,
        axis: 'y',
        containment: "#sortContainer_Summary",
        cursor: "move",
        start: function (event, ui) {
            openWSE.noAnimationForSortableRows(true);
        },
        stop: function (event, ui) {
            openWSE.noAnimationForSortableRows(false);
            SortColumnStopSummary();
        }
    });
}
function SortColumnStopSummary() {
    var tempColumns = summaryColumns;
    summaryColumns = new Array();
    var index = 0;
    $("#table_columns_summary .myItemStyle").each(function () {
        var $this = $(this);
        var summaryName = $.trim($this.find(".summary-name").html());
        var columnName = $.trim($this.find(".summary-column").html());
        
        var formulaVal = "";
        if ($this.find(".summary-formula").find(".view-edit-condition").find(".formula-code").length > 0) {
            formulaVal = $this.find(".summary-formula").find(".view-edit-condition").find(".formula-code").eq(0).html();
        }

        $this.find(".summary-formula").find(".view-edit-condition").remove();
        var formulaType = $.trim($this.find(".summary-formula").html());
        if (formulaType.toLowerCase().indexOf("if") > 0) {
            formulaType += "(";
            if (formulaVal) {
                formulaType += escape($.trim(formulaVal));
            }
            formulaType += ")";
        }

        summaryColumns[index] = {
            "summaryName": summaryName,
            "columnName": columnName,
            "formulaType": formulaType
        };
        index++;
    });

    $("#table_columns_summary").html("<tbody></tbody>");
    AddIDRowSummary();
    for (var i = 0; i < summaryColumns.length; i++) {
        BuildSummaryColumn(summaryColumns[i], false, false, i + 1);
    }

    InitializeSortColumns();
    UpdateSummaryColumnDropdown();
}
function BuildSummaryColumn(summarycolumn, inEditMode, isEditVal, columnNum) {
    var name = summarycolumn.summaryName;
    var column = summarycolumn.columnName;
    var formulaType = summarycolumn.formulaType;

    var formula = openWSE.TableFormulas.GetFormula(formulaType);
    formulaType = openWSE.TableFormulas.GetFormulaType(formulaType, formula);

    var nameEdit = name;
    var columnEdit = column;
    var formulaTypeEdit = formulaType;

    if (isEditVal) {
        name = "<input type='text' id='tb_summaryname_edit' class='textEntry' onkeydown='FinishEditSummaryKeyDown(event, " + columnNum + ")' maxlength='100' style='width: 95%;' />";
        column = "<select id='dd_columnSummary_edit' style='width: 95%;'></select>";
        formulaType = "<select id='dd_formulatype_edit' onchange='onFormulaTypeChange(this);'></select>";
    }

    if (formulaTypeEdit.toLowerCase().indexOf("if") !== -1) {
        formulaType = CreateFormulaTypeConditionDiv(formulaType, columnNum, formula, isEditVal);
    }

    var columnStr = "<tr class='GridNormalRow myItemStyle'>";
    columnStr += "<td align='left' class='GridViewNumRow'>" + columnNum + "</td>";
    columnStr += "<td align='left'><span class='summary-name'>" + name + "</span></td>";
    columnStr += "<td class='summary-column' align='left'>" + column + "</td>";
    columnStr += "<td class='summary-formula' align='left'>" + formulaType + "</td>";

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a onclick='EditSummaryControls(" + columnNum + ");return false;' class='td-edit-btn' title='Edit'></a>";
        actionButton += "<a onclick='DeleteSummaryRow(" + columnNum + ");return false;' class='td-delete-btn' title='Delete'></a>";
    }

    if (isEditVal) {
        actionButton = "<a onclick='FinishEditSummary(" + columnNum + ", false);return false;' class='td-update-btn' title='Update'></a>";
        actionButton += "<a onclick='FinishEditSummary(" + columnNum + ", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
    }

    if (actionButton == "") {
        actionButton = "<div class='float-left' style='width: 22px; height: 22px;'></div>";
    }

    columnStr += "<td class='edit-column-2-items' align='center'>" + actionButton + "</td>";
    columnStr += "</tr>";

    $("#table_columns_summary").find("tbody").append(columnStr);
    $("#tb_summaryname").val("");

    if (isEditVal) {
        var columns = GetColumnsArray();
        var optionList = "";
        for (var i = 0; i < columns.length; i++) {
            optionList += "<option value='" + columns[i].name + "'>" + columns[i].value + "</option>";
        }
        $("#dd_columnSummary_edit").html(optionList);
        $("#dd_formulatype_edit").html($.trim($("#dd_formulatype").html()));

        $("#tb_summaryname_edit").val(nameEdit);
        $("#dd_columnSummary_edit").val(columnEdit);
        $("#dd_formulatype_edit").val(formulaTypeEdit);

        if (formula && $("#tb_formulaCondition_edit").length > 0) {
            $("#tb_formulaCondition_edit").val(unescape(formula));
        }
    }

    AdjustImportModal();
}
function GetColumnsArray() {
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

    return columnArray;
}

function onFormulaTypeChange(_this) {
    if ($(_this).val().toLowerCase().indexOf("if") !== -1) {
        if ($(_this).parent().find(".view-edit-condition").length === 0) {
            $(_this).parent().append(CreateFormulaTypeConditionDiv("", "new", "", true));
        }
        else {
            $(_this).parent().find(".view-edit-condition").show();
        }
    }
    else if ($(_this).parent().find(".view-edit-condition").length > 0) {
        $(_this).parent().find(".view-edit-condition").hide();
    }
}
function CreateFormulaTypeConditionDiv(formulaType, columnNum, formula, isEditVal) {
    formulaType += "<div class='view-edit-condition'><div class='clear'></div>";
    var buttonTitle = "View Condition";
    if (isEditVal) {
        buttonTitle = "Edit Condition";
    }

    formulaType += "<a href='#' onclick=\"EditViewFormulaCondition('" + columnNum + "');return false;\">" + buttonTitle + "</a>";
    formulaType += BuildFormulaModalWindow(columnNum, formula, isEditVal);
    formulaType += "</div>";
    return formulaType;
}
function BuildFormulaModalWindow(columnNum, formula, isEditVal) {
    var ele = "<div id='SummaryRow-Index-" + columnNum + "-element' class='Modal-element' style='display: none;'>";
    ele += "<div class='Modal-overlay'>";
    ele += "<div class='Modal-element-align'>";
    ele += "<div class='Modal-element-modal' data-setwidth='400'>";

    // Header
    ele += "<div class='ModalHeader'><div style='cursor: default !important;'><div class='app-head-button-holder-admin'>";
    ele += "<a href='#' onclick=\"CloseEditViewFormulaCondition('" + columnNum + "');return false;\" class='ModalExitButton'></a>";
    ele += "</div><span class='Modal-title'></span></div></div>";

    var editTextName = "tb_formulaCondition_edit";
    if (columnNum === "new") {
        editTextName = "tb_formulaCondition";
    }

    // Body
    var saveButton = "<input class='input-buttons no-margin float-right' type='button' value='Close' onclick=\"CloseEditViewFormulaCondition('" + columnNum + "');\" />";
    var message = "<small>Enter in a valid formula condition below. Formulas will be based off of Excel formulas. (ex. \"NAME_OF_COLUMN\", \">=2000\")</small><div class='clear-space'></div>";
    if (!isEditVal) {
        message += "<div class='formula-code pad-all border-top border-bottom border-right border-left' style='background: #F5F5F5; overflow: auto;'>" + unescape(formula) + "</div>";
    }
    else {
        message += "<textarea id='" + editTextName + "' class='textEntry' data-allowallchars='true' style='width: 100%;'></textarea>";
    }

    ele += "<div class='ModalPadContent'><div class='message-text'>" + message + "<div class='clear-space'></div></div><div class='button-holder'>" + saveButton + "<div class='clear'></div></div></div></div>";
    ele += "</div></div></div></div>";

    return ele;
}
function EditViewFormulaCondition(columnNum) {
    openWSE.LoadModalWindow(true, "SummaryRow-Index-" + columnNum + "-element", "Formula Condition");
}
function CloseEditViewFormulaCondition(columnNum) {
    openWSE.LoadModalWindow(false, "SummaryRow-Index-" + columnNum + "-element", "");
}


/* Build Default Values */
var defaultValues = new Array();
function RebuildDefaultValuesTable() {
    RemoveDefaultValueRows();
    var columns = GetColumnsArray();
    for (var i = 0; i < columns.length; i++) {
        BuildDefaultValues(columns[i], i, false, "");
    }
}
function RemoveDefaultValueRows(columnName) {
    if (columnName) {
        if ($("#table_table_defaultvalues").find(".myItemStyle[data-columnname='" + columnName + "']").length > 0) {
            $("#table_table_defaultvalues").find(".myItemStyle[data-columnname='" + columnName + "']").remove();
        }
    }
    else {
        $("#table_table_defaultvalues").find(".myItemStyle").remove();
    }
}
function BuildDefaultValues(column, columnNum, inEditMode, editColumnName) {
    var strDefaultVals = "";
    strDefaultVals += "<tr class='GridNormalRow myItemStyle' data-columnname='" + column.name + "'>";
    strDefaultVals += "<td align='left' class='GridViewNumRow'>" + (columnNum + 1).toString() + "</td>";
    strDefaultVals += "<td align='left'>" + column.value + "</td>";

    var defaultVal = GetDefaultValue(column.name);
    if (editColumnName == column.name) {
        strDefaultVals += "<td class='column-default-value' align='left'><input type='text' class='textEntry default-value-edit-textbox' onkeydown='FinishEditDefaultValueKeyPress(event, \"" + column.name + "\")' style='width: 95%;' /></td>";
    }
    else {
        strDefaultVals += "<td class='column-default-value' align='left'>" + defaultVal + "</td>";
    }

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a onclick='EditDefaultValue(\"" + column.name + "\");return false;' class='td-edit-btn' title='Edit'></a>";
    }

    if (editColumnName == column.name) {
        actionButton = "<a onclick='FinishEditDefaultValue(\"" + column.name + "\", false);return false;' class='td-update-btn' title='Update'></a>";
        actionButton += "<a onclick='FinishEditDefaultValue(\"" + column.name + "\", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
    }

    if (actionButton == "") {
        actionButton = "<div class='float-left' style='width: 22px; height: 22px;'></div>";
    }

    strDefaultVals += "<td align='center'>" + actionButton + "</td>";
    strDefaultVals += "</tr>";

    $("#table_table_defaultvalues").find("tbody").append(strDefaultVals);
}
function EditDefaultValue(realName) {
    var columnNum = 0;
    var columns = GetColumnsArray();

    RemoveDefaultValueRows();
    for (var i = 0; i < columns.length; i++) {
        var isEditMode = false;
        var editColumnName = "";
        if (columns[i].name == realName) {
            isEditMode = true;
            editColumnName = columns[i].name;
            columnNum = i;
        }

        BuildDefaultValues(columns[i], i, isEditMode, editColumnName);
    }

    var $input = $("#table_table_defaultvalues").find(".myItemStyle").eq(columnNum).find(".column-default-value > input");
    if ($input.length > 0) {
        $input.val(GetDefaultValue(realName));
        $input.focus();
    }
}
function FinishEditDefaultValueKeyPress(event, columnName) {
    try {
        if (event.which == 13) {
            FinishEditDefaultValue(columnName, false);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            FinishEditDefaultValue(columnName, false);
        }
        delete evt;
    }
}
function FinishEditDefaultValue(columnName, isCancelled) {
    if (!isCancelled) {
        var foundItem = false;
        var newVal = $(".default-value-edit-textbox").val();
        if ($(".default-value-edit-textbox").attr("type") == "checkbox") {
            newVal = $(".default-value-edit-textbox").prop("checked");
        }

        for (var i = 0; i < defaultValues.length; i++) {
            if (defaultValues[i].name === columnName) {
                defaultValues[i].value = newVal;
                foundItem = true;
                break;
            }
        }

        if (!foundItem) {
            defaultValues.push({
                "name": columnName,
                "value": newVal
            });
        }
    }

    var columns = GetColumnsArray();

    RemoveDefaultValueRows();
    for (var i = 0; i < columns.length; i++) {
        BuildDefaultValues(columns[i], i, false, "");
    }
}
function GetDefaultValue(columnName) {
    for (var i = 0; i < defaultValues.length; i++) {
        if (defaultValues[i].name === columnName) {
            return defaultValues[i].value;
        }
    }

    return "";
}

function ChangeIconUploadType(x) {
    if (x == 0) {
        $("#urlIcon-tab").hide();
        $("#uploadIcon-tab").show();
        $("#uploadIcon").hide();
        $("#urlIcon").show();
    }
    else {
        $("#uploadIcon-tab").hide();
        $("#urlIcon-tab").show();
        $("#urlIcon").hide();
        $("#uploadIcon").show();
    }
}
