var dbType = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    $(document).tooltip({ disabled: false })
    openWSE.RadioButtonStyle();
    $("#lbl_connectionstring").css("display", "block");
    $("#div_connectionstring").css("display", "block");

    if ($("#MainContent_HiddenField1").val() != "") {
        if (dbType == "delete") {
            Confirm_Delete();
        }
    }

    $(".cb-allowedit-edit").each(function () {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                $(this).parent().find(".edit-usersallowed-btn").show();
            }
            else {
                $(this).parent().find(".edit-usersallowed-btn").hide();
            }
        }
    });

    dbType = "";

    if ($("#cb_addChart").prop("checked")) {
        $("#chart_selector").show();
        $("#tr-chart-title").show();
    }
    else {
        $("#chart_selector").hide();
        $("#tr-chart-title").hide();
    }

    if ($("#cb_isPrivate").is(":checked")) {
        $("#div_isPrivate").show();
    }
    else {
        $("#div_isPrivate").hide();
    }
});

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
        $("#tr-usersallowed").show();
    }
    else {
        $("#tr-usersallowed").hide();
    }
});
$(document.body).on("click", ".cb-allowedit-edit", function () {
    var $input = $(this).find("input[type='checkbox']");
    if ($input.length > 0) {
        if ($input.prop("checked")) {
            $(this).parent().find(".edit-usersallowed-btn").show();
        }
        else {
            $(this).parent().find(".edit-usersallowed-btn").hide();
        }
    }
});

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