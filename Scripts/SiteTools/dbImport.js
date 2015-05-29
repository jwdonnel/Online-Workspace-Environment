var dbType = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
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
});

$(document.body).on("change", "#MainContent_cb_ddselect input[type='checkbox']", function () {
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

$(document.body).on("change", "#cb_addChart", function () {
    if ($(this).prop("checked")) {
        $("#chart_selector").show();
        $("#tr-chart-title").show();
    }
    else {
        $("#chart_selector").hide();
        $("#tr-chart-title").hide();
    }
});

$(document.body).on("change", "#ddl_ChartType", function () {
    $("#img_charttype").attr("src", "../../Standard_Images/ChartTypes/" + $(this).val().replace(/ /g, "").toLowerCase() + ".png");
    $("#lnk_chartTypeSetup").attr("href", "https://google-developers.appspot.com/chart/interactive/docs/gallery/" + $(this).val().replace(/ /g, "").toLowerCase() + "chart");
});

$(document).ready(function () {
    openWSE.RadioButtonStyle();
});

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

$(document.body).on("click", ".RandomActionBtns, .td-update-btn", function () {
    openWSE.RemoveUpdateModal();
    $("#MainContent_lbl_error").html("");
    openWSE.LoadingMessage1("Updating. Please Wait...");
});


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function Confirm_Delete() {
    if (UserIsSocialAccount != null && UserIsSocialAccount) {
        $("#MainContent_btn_passwordConfirm").trigger("click");
    }
    else {
        openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
        $("#MainContent_tb_passwordConfirm").focus();
    }
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

function BeginWork() {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        openWSE.LoadingMessage1("Deleting Import. Please Wait...");
    }, 100);
    $("#hf_StartDelete").val(new Date().toString());
    __doPostBack("hf_StartDelete", "");
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

$(document.body).on("click", ".checkbox-edit-click", function (e) {
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
$(document.body).on("click", ".checkbox-new-click", function (e) {
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
    $(".checkbox-new-click").each(function () {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                usersAllowedToEdit += $input.val().toLowerCase() + ";";
            }
        }
    });

    $("#hf_usersAllowedToEdit").val(usersAllowedToEdit);
}

function EditUsersAllowedToEditEdit(id) {
    $("#hf_usersAllowedToEdit_Edit").val(id);
    openWSE.LoadingMessage1("Loading. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/DatabaseImportCreator.asmx/EditUsersAllowedToEditForDatabaseImport",
        data: JSON.stringify({ "id": id }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();
            var $modalContent = $("#UsersAllowedEdit-element").find("#pnl_usersAllowedToEdit_Edit");
            if (data.d != "") {
                var updateBtn = "<input type='button' class='input-buttons' onclick=\"UpdateUsersAllowedToEditEdit();\" value='Update' />";
                var cancelBtn = "<input type='button' class='input-buttons no-margin' onclick=\"CancelUsersAllowedToEditEdit();\" value='Cancel' />";
                $modalContent.html(data.d + "<div class='clear-space'></div><div align='right' class='pad-bottom'>" + updateBtn + cancelBtn + "</div>");
            }
            openWSE.LoadModalWindow(true, "UsersAllowedEdit-element", "Users Allowed To Edit")
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to update your table.");
        }
    });
}
function UpdateUsersAllowedToEditEdit() {
    var usersAllowedToEdit = "";
    $(".checkbox-edit-click").each(function () {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                usersAllowedToEdit += $input.val().toLowerCase() + ";";
            }
        }
    });

    openWSE.LoadingMessage1("Updating. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/DatabaseImportCreator.asmx/UpdateUsersAllowedToEditForDatabaseImport",
        data: JSON.stringify({ "id": $("#hf_usersAllowedToEdit_Edit").val(), "usersAllowed": usersAllowedToEdit }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        complete: function (data) {
            openWSE.RemoveUpdateModal();
            CancelUsersAllowedToEditEdit();

        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to update your table.");
        }
    });
}
function CancelUsersAllowedToEditEdit() {
    openWSE.LoadModalWindow(false, "UsersAllowedEdit-element", "");
    $("#hf_usersAllowedToEdit_Edit").val("");
    $("#pnl_usersAllowedToEdit_Edit").html("");
}

function ImportTableClick() {
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

    $("#hf_columnOverrides").val(columnOverrides);
    $("#hf_importClick").val(new Date().toString());
    __doPostBack("hf_importClick", "");
}

function EditEntry(id) {
    openWSE.LoadingMessage1("Please Wait...");
    $("#hf_editimport").val(id);
    __doPostBack("hf_editimport", "");
}

function DeleteEntry(id) {
    dbType = "delete";
    $("#hf_deleteimport").val(id);
    Confirm_Delete();
}

function CreateAppEntry(id) {
    openWSE.LoadingMessage1("Creating App...");
    $("#hf_createAppImport").val(id);
    __doPostBack("hf_createAppImport", "");
}

function CancelEntry() {
    openWSE.LoadingMessage1("Cancelling...");
    $("#hf_editimport").val("cancel");
    __doPostBack("hf_editimport", "");
}

function UpdateEntry(id) {
    if ($.trim($(".appname-edit").val()) != "") {
        openWSE.LoadingMessage1("Updating...");

        $("#hf_editAppName").val($.trim($(".appname-edit").val()));
        $("#hf_editEditable").val($(".editable-edit").prop("checked"));
        $("#hf_editChartType").val($.trim($(".charttype-edit").val()));
        $("#hf_editChartTitle").val($.trim($(".charttitle-edit").val()));
        $("#hf_editSelectCommand").val($.trim($(".selectcommand-edit").val()));

        var columnOverrides = "";
        $(".column-namechange-edit").each(function () {
            var value = $.trim($(this).val());
            if (value == "") {
                value = $(this).attr("data-name");
            }

            columnOverrides += $(this).attr("data-name") + "=" + value + ";";
        });

        $("#hf_editOverrideColumns").val(columnOverrides);

        $("#hf_editNotifyUsers").val($(".notifyusers-edit").prop("checked"));

        $("#hf_updateimport").val(id);
        __doPostBack("hf_updateimport", "");
    }
    else {
        openWSE.AlertWindow("App/Table Name cannot be empty!");
    }
}

function OnEditableChange(_this) {
    if ($(_this).prop("checked")) {
        $(".edit-usersallowed-btn").show();
    }
    else {
        $(".edit-usersallowed-btn").hide();
    }
}

// Import Wizard
var testConnectionGood = false;
function StartImportWizard() {
    $(".import-steps").find("li").hide();
    $(".import-steps").find("#step1").show();
    $("#btn_finishImportWizard, .prev-step").hide();
    $(".next-step").show();
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    openWSE.LoadModalWindow(true, "ImportWizard-element", "Table Import Wizard");
}
function CloseImportWizard() {
    openWSE.LoadModalWindow(false, "ImportWizard-element", "");
}
function ConfirmCancelWizard() {
    openWSE.ConfirmWindow("Are you sure you want to cancel the wizard import?", function () {
        CloseImportWizard();
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

    var top = $("#ImportWizard-element").find(".Modal-element-modal").css("top");
    if (top == "auto") {
        $("#ImportWizard-element").find(".Modal-element-align").css({
            marginTop: -($("#ImportWizard-element").find(".Modal-element-modal").height() / 2),
            marginLeft: -($("#ImportWizard-element").find(".Modal-element-modal").width() / 2)
        });
    }

    CheckIfConnected();
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
}
function CheckIfConnected() {
    $(".next-step").prop("disabled", false);
    $(".next-step").removeClass("next-disabled");
    for (var i = 0; i < $(".import-steps").find("li").length; i++) {
        if ($(".import-steps").find("li").eq(i).css("display") != "none" && $(".import-steps").find("li").eq(i).attr("id") == "step3") {
            if (!testConnectionGood) {
                $(".next-step").prop("disabled", true);
                $(".next-step").addClass("next-disabled");
            }
            break;
        }
    }
}