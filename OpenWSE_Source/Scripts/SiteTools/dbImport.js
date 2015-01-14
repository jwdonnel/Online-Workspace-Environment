var dbType = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    $(document).tooltip({ disabled: false })
    openWSE.RadioButtonStyle();
    if ($("#MainContent_dd_provider").val() == "excel") {
        $("#lbl_connectionstring").css("display", "none");
        $("#div_connectionstring").css("display", "none");
        $("#lbl_connectionstring_excel").css("display", "block");
        $("#div_connectionstring_excel").css("display", "block");
    }
    else {
        $("#lbl_connectionstring").css("display", "block");
        $("#div_connectionstring").css("display", "block");
        $("#lbl_connectionstring_excel").css("display", "none");
        $("#div_connectionstring_excel").css("display", "none");
    }

    if ($("#MainContent_HiddenField1").val() != "") {
        if (dbType == "delete") {
            Confirm_Delete();
        }
    }

    dbType = "";
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

$(document.body).on("change", "#MainContent_dd_provider", function () {
    if ($(this).val() == "excel") {
        $("#lbl_connectionstring").css("display", "none");
        $("#div_connectionstring").css("display", "none");
        $("#lbl_connectionstring_excel").css("display", "block");
        $("#div_connectionstring_excel").css("display", "block");
    }
    else {
        $("#lbl_connectionstring").css("display", "block");
        $("#div_connectionstring").css("display", "block");
        $("#lbl_connectionstring_excel").css("display", "none");
        $("#div_connectionstring_excel").css("display", "none");
    }
});


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function Confirm_Delete() {
    $("#db_overlay").css("display", "block");
    $("#db_overlay").css("visibility", "visible");
    $("#db_modal").css("display", "block");
    $("#db_modal").css("opacity", "1.0");
    $("#db_modal").css("filter", "alpha(opacity=100)");
    $("#MainContent_tb_passwordConfirm").focus();
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
    $("#db_overlay").css("display", "none");
    $("#db_overlay").css("visibility", "hidden");
    $("#db_modal").fadeOut(300);
    dbType = "";
    $("#MainContent_tb_passwordConfirm").val("");
}

function BeginWork() {
    $("#db_overlay").css("display", "none");
    $("#db_overlay").css("visibility", "hidden");
    $("#db_modal").fadeOut(300);
    openWSE.LoadingMessage1("Deleting Import. Please Wait...");
    $("#hf_StartDelete").val(new Date().toString());
    __doPostBack("hf_StartDelete", "");
}