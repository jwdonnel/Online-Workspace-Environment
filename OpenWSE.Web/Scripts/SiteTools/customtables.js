/* Variables */
var columns = new Array();
var summaryColumns = new Array();
var availableCharts = null;
var loadedTables = new Array();
var defaultValues = new Array();


/* On Page load Functions */
$(document).ready(function () {
    AddIDRow();
    AddIDRowSummary();
    InitializeSortColumns();
});

var beforePostBackChecklist = new Array();
var currStepBeforePostback = "";
var pnl_tableListStr = "";
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
    if ($("#ImportWizard-element").css("display") == "block") {
        $("#create-wizard-steps").find(".steps").each(function (index) {
            if ($(this).css("display") != "none") {
                currStepBeforePostback = ".step" + (index + 1);
            }
        });
    }

    beforePostBackChecklist.push({
        "installApp": $("#MainContent_cb_InstallAfterLoad").prop("checked"),
        "private": $("#cb_isPrivate").prop("checked"),
        "notifications": $("#cb_allowNotifi").prop("checked")
    });

    var elem = args.get_postBackElement();
    if (elem && elem.id === "hf_UpdateAll") {
        pnl_tableListStr = $.trim($("#MainContent_pnl_tableList").html());
    }
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    if (currStepBeforePostback != "") {
        if ($("#ImportWizard-element").css("display") == "block") {
            $("#create-wizard-steps").find(".steps").hide();
            $("#create-wizard-steps").find(currStepBeforePostback).show();
        }
    }

    if (beforePostBackChecklist.length > 0) {
        $("#MainContent_cb_InstallAfterLoad").prop("checked", beforePostBackChecklist[0].installApp);
        $("#cb_isPrivate").prop("checked", beforePostBackChecklist[0].private);

        $("#div_isPrivate").hide();
        if ($("#MainContent_cb_InstallAfterLoad").is(":checked") && $("#div_InstallAfterLoad").css("display") != "none") {
            $("#div_isPrivate").show();
        }

        $("#cb_allowNotifi").prop("checked", beforePostBackChecklist[0].notifications);
    }

    beforePostBackChecklist = new Array();
    SetCustomizations();
    currStepBeforePostback = "";

    if (pnl_tableListStr != "") {
        $("#MainContent_pnl_tableList").html(pnl_tableListStr);
    }

    pnl_tableListStr = "";
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
        loadingPopup.Message("Deleting Table. Please Wait...");
    }, 100);
    $("#hf_tableDelete").val(id);
    openWSE.CallDoPostBack("hf_tableDelete", "");
}
function EditEntry(id) {
    tableEditing = id;
    loadingPopup.Message("Getting Information. Please Wait...");
    openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/EditTable", "{ 'id': '" + id + "' }", {
        cache: false
    }, function (data) {
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

                customizationStr = data.d[5];
                SetCustomizations();

                AddIDRow();
                RemoveDefaultValueRows();
                for (var i = 0; i < columns.length; i++) {
                    BuildColumn(columns[i], false, false, i + 1);
                    BuildDefaultValues(columns[i], i, false, "");
                }

                try {
                    summaryColumns = $.parseJSON(data.d[6]);
                } catch (evt2) { }

                AddIDRowSummary();
                for (var i = 0; i < summaryColumns.length; i++) {
                    BuildSummaryColumn(summaryColumns[i], false, false, i + 1);
                }

                UpdateChartColumnSelectors();
                UpdateSummaryColumnDropdown();
                UpdateMonthSelector();

                setChartColumnsCustomizations();

                $("#tr-usersallowed").hide();
                $("#tr-usersallowedEdit").show();
                $("#div_InstallAfterLoad").hide();
                $("#div_isPrivate").hide();

                beforePostBackChecklist = new Array();
                beforePostBackChecklist.push({
                    "installApp": $("#MainContent_cb_InstallAfterLoad").prop("checked"),
                    "private": $("#cb_isPrivate").prop("checked"),
                    "notifications": $("#cb_allowNotifi").prop("checked")
                });

                $("#btn_finishImportWizard").val("Update");
                openWSE.LoadModalWindow(true, "ImportWizard-element", "Custom Table Wizard");
            }
            catch (evt) { }
        }

        loadingPopup.RemoveMessage();
    }, function (data) {
        openWSE.AlertWindow("An error occured while trying to get your table info.");
        loadingPopup.RemoveMessage();
    });
}
function RecreateApp(appid, tableId, tableName, description) {
    loadingPopup.Message("Creating App. Please Wait...");
    openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/RecreateApp", "{ 'appId': '" + appid + "','tableId': '" + tableId + "','tableName': '" + escape(tableName) + "', 'description': '" + escape(description) + "' }", {
        cache: false
    }, function (data) {
        if (!openWSE.ConvertBitToBoolean(data.d)) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("An error occured while trying to create the app.");
        }
        else {
            $("#hf_tableUpdate").val(new Date().toString());
            openWSE.CallDoPostBack("hf_tableUpdate", "");
        }
    }, function (data) {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("An error occured while trying to create the app.");
    });
}
function ShowHideTableDetails(id, name) {
    openWSE.LoadModalWindow(true, id + "Modal-element", name + " Details");
}
function CloseTableDetailsModal(id) {
    openWSE.LoadModalWindow(false, id + "Modal-element", "");
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

    $("#dd_formulatype").val("Sum");
    $("#table_addcolumn_Controls").show();
    $("#table_addcolumn_summary_Controls").show();
    $(".view-edit-condition").hide();

    customizationStr = "";

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

    if (columns && columns.length == 0 && currentStep == 2) {
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
    $("#tb_summaryname").val("");
    $("#dd_formulatype").val("Sum");
    $(".view-edit-condition").hide();
    $("#ddl_datatypes").val("nvarchar");
    $("#tb_length").show();
    $("#tb_length").val("100");
    $("#cb_nullable").prop("checked", false);
    $("#UsersAllowedEdit-element").html("");
    $("#chart-column-selector-holder").html("");
    $("#table_columns").html("");
    $("#table_columns_summary").html("");
    $("#table_addcolumn_Controls").show();
    $("#table_addcolumn_summary_Controls").show();

    RemoveDefaultValueRows();
    AddIDRow();
    AddIDRowSummary();

    columns = new Array();
    summaryColumns = new Array();
    defaultValues = new Array();
    SetDefaultCustomizations();
    customizationStr = "";

    currStepBeforePostback = "";
    $("#ImportWizard-element").find(".Modal-element-modal").css("top", "auto");
    $("#ImportWizard-element").find(".Modal-element-modal").css("left", "auto");
}
function AdjustCreateBox() {
    var top = $("#ImportWizard-element").find(".Modal-element-modal").css("top");
    if (top == "auto" || top == "0px") {
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
        loadingPopup.Message("Please Wait...");
        openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/GetFileUploadContents", data, {
            processData: false,
            contentType: false
        }, function (response) {
            //code after success
            var innerHtml = '<input type="file" id="excelFileUpload" class="margin-right" /><input type="button" class="input-buttons" onclick="ImportExcelSpreadSheet()" value="Import" />';
            $("#div-excelFileUpload").html(innerHtml);
            loadingPopup.RemoveMessage();

            if (data.d == "false") {
                openWSE.AlertWindow("Failed to import file. Please try again.");
            }
        }, function (er) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow(er);
        });
    }
    catch (evt) {
        loadingPopup.RemoveMessage();
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
            loadingPopup.Message("Updating Table. Please Wait...");

            $("#UsersAllowedEdit-element").find(".checkbox-usersallowed").each(function () {
                if ($(this).prop("checked")) {
                    usersAllowed += $(this).val().toLowerCase() + ";";
                }
            });
        }
        else {
            loadingPopup.Message("Creating Table. Please Wait...");

            $("#MainContent_pnl_usersAllowedToEdit").find(".checkbox-usersallowed").each(function () {
                if ($(this).prop("checked")) {
                    usersAllowed += $(this).val().toLowerCase() + ";";
                }
            });
        }

        var description = $.trim($("#tb_description").val());
        var columnData = JSON.stringify(columns);
        var summaryData = JSON.stringify(summaryColumns);
        var customizations = BuildCustomizationString();
        columnData = escape(columnData);
        summaryData = escape(summaryData);
        customizations = escape(customizations);
        tableName = escape(tableName);
        usersAllowed = escape(usersAllowed);

        var serviceMethod = "Create";
        var methodData = "{ 'columns': '" + columnData + "', 'summaryData': '" + summaryData + "', 'customizations': '" + customizations + "', 'tableName': '" + tableName + "', 'description': '" + description + "', 'installForUser': '" + $("#MainContent_cb_InstallAfterLoad").is(":checked") + "', 'notifyUsers': '" + $("#cb_allowNotifi").is(":checked") + "', 'isPrivate': '" + $("#cb_isPrivate").is(":checked") + "', 'usersAllowed': '" + usersAllowed + "' }";
        if (tableEditing != "") {
            serviceMethod = "Update";
            methodData = "{ 'tableID': '" + tableEditing + "', 'columns': '" + columnData + "', 'summaryData': '" + summaryData + "', 'customizations': '" + customizations + "', 'tableName': '" + tableName + "', 'description': '" + description + "', 'notifyUsers': '" + $("#cb_allowNotifi").is(":checked") + "', 'usersAllowed': '" + usersAllowed + "' }";
        }

        openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/" + serviceMethod, methodData, {
            cache: false
        }, function (data) {
            if (data.d === "false") {
                openWSE.AlertWindow("An error occured while trying to " + serviceMethod.toLowerCase() + " your table.");
                loadingPopup.RemoveMessage();
            }
            else {
                try {
                    var fd = new FormData();
                    var files = $("#MainContent_fu_image_create").get(0).files;
                    if (files.length > 0 && fd) {
                        fd.append("TableID", data.d);
                        fd.append("TableType", "CustomTables");
                        fd.append("Filedata", files[0]);
                        var xmlHttpReq = openWSE.CreateXMLHttpRequest();
                        if (xmlHttpReq != null) {
                            xmlHttpReq.addEventListener("load", function () {
                                finishTableUpdate();
                            }, false);
                            xmlHttpReq.open("POST", openWSE.siteRoot() + "Handlers/AppIconUpload.ashx");
                            xmlHttpReq.send(fd);
                        }
                    }
                    else if ($.trim($("#MainContent_tb_imageurl").val()) !== "") {
                        openWSE.AjaxCall("WebServices/CustomTableCreator.asmx/UploadAppIcon_Url", "{ 'file': '" + escape($.trim($("#MainContent_tb_imageurl").val())) + "', 'tableId': '" + escape(data.d) + "', 'TableType': 'CustomTables' }", {
                            cache: false
                        }, null, null, function () {
                            finishTableUpdate();
                        });
                    }
                    else {
                        finishTableUpdate();
                    }
                }
                catch (evt) {
                    finishTableUpdate();
                }
            }
        }, function (data) {
            openWSE.AlertWindow("An error occured while trying to " + serviceMethod.toLowerCase() + " your table.");
            loadingPopup.RemoveMessage();
        });
    }
}
function finishTableUpdate() {
    tableEditing = "";
    setTimeout(function () {
        CloseCreateWizard();

        $("#hf_tableUpdate").val(new Date().toString());
        openWSE.CallDoPostBack("hf_tableUpdate", "");
    }, 1000);
}


/* Customizations */
var customizationStr = "";
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
    $("#tb_tableheadercolor").val("#FFFFFF");
    $("#cb_usedefaultheadercolor").prop("checked", true);

    $("#tb_primaryrowcolor").prop("disabled", true);
    $("#tb_primaryrowcolor").val("#FFFFFF");
    $("#cb_usedefaultprimarycolor").prop("checked", true);

    $("#tb_alternativerowcolor").prop("disabled", true);
    $("#tb_alternativerowcolor").val("#FFFFFF");
    $("#cb_usedefaultalternativecolor").prop("checked", true);

    $("#dd_fontfamilycustomization").val("");
    $("#span_fontfamilypreview").css("font-family", "");

    $("#ddl_ChartType").val("None");
    $("#img_charttype_holder").hide();
    $("#chart-title-holder").hide();
    $("#chart-column-selector").hide();
    $("#dd_columnSummary").html("<option value=''></option>");

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

    $("#MainContent_tb_imageurl").val("");
    $("#MainContent_fu_image_create").val("");
    $("#MainContent_fu_image_create").get(0).files = null;

    $("#dd_viewstyle").val("default");

    ChangeIconUploadType(1);
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

    // Month Selector
    customizationArray.push({
        "customizeName": "MonthSelector",
        "customizeValue": $.trim($("#dd_monthselector").val())
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
function SetCustomizations() {
    try {
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
                    setChartColumnsCustomizations();
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
function setChartColumnsCustomizations() {
    try {
        if (!customizationStr) {
            return;
        }

        var customizations = $.parseJSON(customizationStr);
        var value = customizations[6].customizeValue;

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
    }
    catch (evt) { }
}


/* Build Default Values */
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
    strDefaultVals += "<tr class='GridNormalRow myItemStyle' data-columnname='" + column.realName + "'>";
    strDefaultVals += "<td align='left' class='GridViewNumRow'>" + (columnNum + 1).toString() + "</td>";
    strDefaultVals += "<td align='left'>" + column.shownName + "</td>";

    var defaultVal = GetDefaultValue(column.realName);
    if (column.dataType.toLowerCase() === "boolean") {
        var checked = "";
        if (defaultVal != "" && openWSE.ConvertBitToBoolean(defaultVal)) {
            checked = " checked='checked'";
        }

        if (editColumnName == column.realName) {
            strDefaultVals += "<td class='column-default-value' align='left'><input type='checkbox'" + checked + " class='default-value-edit-textbox' /></td>";
        }
        else {
            strDefaultVals += "<td class='column-default-value' align='left'><input type='checkbox'" + checked + " disabled='disabled' />" + "</td>";
        }
    }
    else {
        if (editColumnName == column.realName) {
            strDefaultVals += "<td class='column-default-value' align='left'><input type='text' class='textEntry default-value-edit-textbox' onkeydown='FinishEditDefaultValueKeyPress(event, \"" + column.realName + "\")' style='width: 100%;' /></td>";
        }
        else {
            strDefaultVals += "<td class='column-default-value' align='left'>" + defaultVal + "</td>";
        }
    }

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a onclick='EditDefaultValue(\"" + column.realName + "\");return false;' class='td-edit-btn' title='Edit'></a>";
    }

    if (editColumnName == column.realName) {
        actionButton = "<a onclick='FinishEditDefaultValue(\"" + column.realName + "\", false);return false;' class='td-update-btn' title='Update'></a>";
        actionButton += "<a onclick='FinishEditDefaultValue(\"" + column.realName + "\", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
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

    RemoveDefaultValueRows();
    for (var i = 0; i < columns.length; i++) {
        var isEditMode = false;
        var editColumnName = "";
        if (columns[i].realName == realName) {
            isEditMode = true;
            editColumnName = columns[i].realName;
            columnNum = i;
        }

        BuildDefaultValues(columns[i], i, isEditMode, editColumnName);
    }

    var $input = $("#table_table_defaultvalues").find(".myItemStyle").eq(columnNum).find(".column-default-value > input");
    if ($input.length > 0) {
        var defaultVal = GetDefaultValue(realName);

        var columnType = $.trim(columns[columnNum].dataType.toLowerCase());

        var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
        if (columnType == "date") {
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

            $input.val(defaultVal);
        }
        else if (columnType == "datetime") {
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

            $input.val(defaultVal);
        }
        else if (columnType == "decimal" || columnType == "money") {
            $input.attr("step", "0.1");
            $input.attr("type", "number");
            $input.val(defaultVal);
        }
        else if (columnType == "integer") {
            $input.attr("step", "1");
            $input.attr("type", "number");
            $input.val(defaultVal);
        }
        else if ($.trim(columnType) == "boolean") {
            $input.css("width", "auto");
            $input.attr("type", "checkbox");
            $input.attr("onkeyup", "");
            $input.prop("checked", false);
            $input.attr("value", "");
            if (defaultVal) {
                $input.prop("checked", true);
            }
        }
        else {
            $input.attr("maxlength", columns[columnNum].dataLength);
            $input.val(defaultVal);
        }

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
        cancel: ".no-sort, .myHeaderStyle",
        helper: fixHelper,
        axis: 'y',
        containment: "#sortContainer",
        cursor: "move",
        start: function (event, ui) {
            openWSE.noAnimationForSortableRows(true);
        },
        stop: function (event, ui) {
            openWSE.noAnimationForSortableRows(false);
            SortColumnStop();
        }
    });

    $("#table_columns_summary tbody").sortable({
        cancel: ".no-sort, .myHeaderStyle",
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
            if (parseInt(len) > 3999 || parseInt(len) < 0 || parseInt(len).toString() == NaN || !len) {
                len = "100";
            }

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
    RemoveDefaultValueRows();
    for (var i = 0; i < columns.length; i++) {
        BuildColumn(columns[i], false, false, i + 1);
        BuildDefaultValues(columns[i], i, false, "");
    }

    InitializeSortColumns();
    UpdateChartColumnSelectors();
    UpdateSummaryColumnDropdown();
    UpdateMonthSelector();
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
    UpdateMonthSelector();
}
function AddIDRow() {
    var column = "<tr class='myHeaderStyle'>";
    column += "<td width='55px'></td>";
    column += "<td>Column Name</td>";
    column += "<td width='135px'>Data Type</td>";
    column += "<td width='75px'>Length</td>";
    column += "<td width='75px'>Nullable</td>";
    column += "<td class='edit-column-2-items' align='center'></td>";
    column += "</tr>";

    if ($("#table_columns").find("tbody").length == 0) {
        $("#table_columns").html("<tbody></tbody>");
    }
    $("#table_columns").find("tbody").html(column);
}
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
        nameEdit = "<input type='text' id='tb_columnName_edit' class='textEntry' onkeydown='FinishEditKeyDown(event, \"" + realName + "\", " + columnNum + ")' maxlength='100' style='width: 95%;' />";
        type = "<select id='ddl_datatypes_edit'><option value='nvarchar'>nvarchar</option><option value='Date'>Date</option><option value='DateTime'>DateTime</option><option value='Integer'>Integer</option><option value='Money'>Money</option><option value='Decimal'>Decimal</option><option value='Boolean'>Boolean</option></select>";

        var cssHideLen = "display: block;";
        if (typeEdit != "nvarchar") {
            cssHideLen = "display: none;";
        }

        len = "<input type='text' class='textEntry' id='tb_length_edit' maxlength='4' onkeydown='FinishEditKeyDown(event, \"" + realName + "\", " + columnNum + ")' style='width: 95%;" + cssHideLen + "' />";
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
    column += "<td align='left' class='GridViewNumRow'>" + columnNum + "</td>";
    column += "<td class='no-sort' align='left'><span class='column-name'>" + nameEdit + "</span><span class='real-column-name' style='display: none;'>" + realName + "</span></td>";
    column += "<td class='no-sort column-type' align='left'>" + type + "</td>";
    column += "<td class='no-sort column-length' align='left'>" + len + "</td>";
    column += "<td class='no-sort column-nullable' align='left'>" + nullable + "</td>";

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a onclick='EditControls(\"" + realName + "\");return false;' class='td-edit-btn' title='Edit'></a>";
        actionButton += "<a onclick='DeleteRow(\"" + columnNum + "\");return false;' class='td-delete-btn' title='Delete'></a>";
    }

    if (isEditVal) {
        actionButton = "<a onclick='FinishEdit(\"" + columnNum + "\", \"" + realName + "\", false);return false;' class='td-update-btn' title='Update'></a>";
        actionButton += "<a onclick='FinishEdit(\"" + columnNum + "\", \"" + realName + "\", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
    }

    if (actionButton == "") {
        actionButton = "<div class='float-left' style='width: 22px; height: 22px;'></div>";
    }

    column += "<td class='no-sort edit-column-2-items' align='center'>" + actionButton + "</td>";
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
        name = "<input type='text' id='tb_summaryname_edit' class='textEntry' onkeydown='FinishEditSummaryKeyDown(event, " + columnNum + ")' maxlength='100' style='width: 90%;' />";
        column = "<select id='dd_columnSummary_edit' style='width: 90%;'></select>";
        formulaType = "<select id='dd_formulatype_edit' onchange='onFormulaTypeChange(this);'></select>";
    }

    if (formulaTypeEdit.toLowerCase().indexOf("if") !== -1) {
        formulaType = CreateFormulaTypeConditionDiv(formulaType, columnNum, formula, isEditVal);
    }

    var columnStr = "<tr class='GridNormalRow myItemStyle'>";
    columnStr += "<td align='left' class='GridViewNumRow'>" + columnNum + "</td>";
    columnStr += "<td class='no-sort' align='left'><span class='summary-name'>" + name + "</span></td>";
    columnStr += "<td class='no-sort summary-column' align='left'>" + column + "</td>";
    columnStr += "<td class='no-sort summary-formula' align='left'>" + formulaType + "</td>";

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

    columnStr += "<td class='no-sort edit-column-2-items' align='center'>" + actionButton + "</td>";
    columnStr += "</tr>";

    $("#table_columns_summary").find("tbody").append(columnStr);
    $("#tb_summaryname").val("");

    if (isEditVal) {
        var optionList = "";
        for (var i = 0; i < columns.length; i++) {
            optionList += "<option value='" + columns[i].realName + "'>" + columns[i].shownName + "</option>";
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
            UpdateSummaryColumnDropdown();
            UpdateMonthSelector();

            RemoveDefaultValueRows(realColumnName);
            BuildDefaultValues(columns[len], len, false, "");
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
        RemoveDefaultValueRows();
        for (var i = 0; i < columns.length; i++) {
            BuildColumn(columns[i], false, false, i + 1);
            BuildDefaultValues(columns[i], i, false, "");
        }

        UpdateChartColumnSelectors();
        UpdateSummaryColumnDropdown();
        UpdateMonthSelector();
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
            RemoveDefaultValueRows();
            for (var i = 0; i < columns.length; i++) {
                BuildColumn(columns[i], false, false, i + 1);
                BuildDefaultValues(columns[i], i, false, "");
            }

            if (columns.length == 0 && $("#create-wizard-steps").find(".step4").css("display") != "none") {
                $("#chart-column-selector-holder").html("<h3 class='pad-all'>No columns available</h3>");
                $(".next-step").addClass("next-disabled");
                $(".next-step").prop("disabled", true);
            }
            else {
                UpdateChartColumnSelectors();
                UpdateSummaryColumnDropdown();
                UpdateMonthSelector();
                $(".next-step").removeClass("next-disabled");
                $(".next-step").prop("disabled", false);
            }

            if (columns && columns.length == 0) {
                $(".next-step").addClass("next-disabled");
                $(".next-step").prop("disabled", true);
            }
            else {
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

    AdjustCreateBox();
}
function UpdateChartColumnSelectors() {
    var chartColumnStr = "";
    for (var i = 0; i < columns.length; i++) {
        chartColumnStr += "<span class='cb_style inline-block'><input type='checkbox' id='cb_" + columns[i].realName + "_chartColumn' value='" + columns[i].realName + "' checked='checked' />";
        chartColumnStr += "<label for='cb_" + columns[i].realName + "_chartColumn'>&nbsp;" + columns[i].shownName + "</label></span><div class='clear-space-five'></div>";
    }
    $("#chart-column-selector-holder").html(chartColumnStr);
}


function UpdateMonthSelector() {
    var optionList = "<option value=''></option>";
    for (var i = 0; i < columns.length; i++) {
        if (columns[i].dataType.toLowerCase() == "date" || columns[i].dataType.toLowerCase() == "datetime") {
            optionList += "<option value='" + columns[i].realName + "'>" + columns[i].shownName + "</option>";
        }
    }

    $("#dd_monthselector").html(optionList);

    if (!customizationStr) {
        return;
    }

    var customizations = $.parseJSON(customizationStr);
    for (var i = 0; i < customizations.length; i++) {
        if (customizations[i].customizeName == "MonthSelector") {
            $("#dd_monthselector").val(customizations[i].customizeValue);
            break;
        }
    }
}


/* Summary Controls */
function UpdateSummaryColumnDropdown() {
    var optionList = "";
    for (var i = 0; i < columns.length; i++) {
        optionList += "<option value='" + columns[i].realName + "'>" + columns[i].shownName + "</option>";
    }

    $("#dd_columnSummary").html(optionList);

    if ($("#dd_columnSummary_edit").length > 0) {
        var tempVal = $("#dd_columnSummary_edit").val();
        $("#dd_columnSummary_edit").html("");
        var optionList = "";
        for (var i = 0; i < columns.length; i++) {
            optionList += "<option value='" + columns[i].realName + "'>" + columns[i].shownName + "</option>";
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
            UpdateMonthSelector();
        }, null);
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

    formulaType += "<a onclick=\"EditViewFormulaCondition('" + columnNum + "');return false;\" style=\"visibility: visible!important;\">" + buttonTitle + "</a>";
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
