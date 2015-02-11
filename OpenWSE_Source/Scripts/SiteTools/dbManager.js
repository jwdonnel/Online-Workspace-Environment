var dbType = "";
var dbMessage = "";
Sys.Application.add_load(function () {
    openWSE.RadioButtonStyle();
    switch (dbType) {
        case "restore":
            Confirm_Restore();
            break;
        case "restorelast":
            Confirm_RestoreLast();
            break;
    }
    dbType = "";
});

function EditSlot(id) {
    document.getElementById("hf_EditSlot").value = id;
    __doPostBack("hf_EditSlot", "");
}

$(document.body).on("change", "#cbAutoFixDB", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

function DeleteSlot(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this auto backup?",
       function () {
           openWSE.LoadingMessage1("Deleting. Please Wait...");
           document.getElementById("hf_DeleteSlot").value = id;
           __doPostBack("hf_DeleteSlot", "");
       }, null);
}

$(document.body).on("click", "#MainContent_btn_checkDatabase", function () {
    openWSE.LoadingMessage1("Scanning Database. Please Wait...");
});

$(document.body).on("click", "#MainContent_btn_UpdateDatabase", function () {
    openWSE.LoadingMessage1("Updating Database. Please Wait...");
});

$(document.body).on("click", ".RandomActionBtns-backup-perform", function () {
    openWSE.LoadingMessage1("Backing Up Database...");
});

function UpdateSlot(id) {
    document.getElementById("hf_dayofweek_edit").value = $("#ddl_dayofweek_Edit").val();
    document.getElementById("hf_timeofweek_edit").value = $("#ddl_timeofweekHour_Edit").val() + ":" + $("#ddl_timeofweekMin_Edit").val() + " " + $("#ddl_timeofweekAmPm_Edit").val();
    document.getElementById("hf_backuptype_edit").value = $("#ddl_backuptype_Edit").val();
    document.getElementById("hf_UpdateSlot").value = id;
    __doPostBack("hf_UpdateSlot", "");
}

function CancelSlot() {
    document.getElementById("hf_CancelSlot").value = new Date().toString();
    __doPostBack("hf_CancelSlot", "");
}

$(document.body).on("click", ".dbviewer-update-img, .dbviewer-update a", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("change", "#MainContent_dd_recoverymodal", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

function Confirm_Backup() {
    dbMessage = "Backing Up Database. Please Wait...";
}

function StartBackingUp() {
    openWSE.LoadModalWindow(false, "tablestorestore-element", '');
    openWSE.LoadingMessage1(dbMessage);
}

function DeselectAllTables() {
    $("#btn_deselectall_tables").hide();
    $("#btn_selectall_tables").show();
    $("#MainContent_cblist_tables").find("input[type='checkbox']").each(function () {
        $(this).prop("checked", false);
    });
}

function SelectAllTables() {
    $("#btn_selectall_tables").hide();
    $("#btn_deselectall_tables").show();
    $("#MainContent_cblist_tables").find("input[type='checkbox']").each(function () {
        $(this).prop("checked", true);
    });
}


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */

function Confirm_Restore() {
    $("#db_overlay").css("display", "block");
    $("#db_overlay").css("visibility", "visible");
    $("#db_modal").css("display", "block");
    $("#db_modal").css("opacity", "1.0");
    $("#db_modal").css("filter", "alpha(opacity=100)");
    $("#hf_buRestore_type").val("Restore");
    $("#MainContent_tb_passwordConfirm").focus();
    dbMessage = "Restoring Database. Please Wait...";
}

function Confirm_RestoreLast() {
    $("#db_overlay").css("display", "block");
    $("#db_overlay").css("visibility", "visible");
    $("#db_modal").css("display", "block");
    $("#db_modal").css("opacity", "1.0");
    $("#db_modal").css("filter", "alpha(opacity=100)");
    $("#hf_buRestore_type").val("RestoreLast");
    $("#MainContent_tb_passwordConfirm").focus();
    dbMessage = "Restoring Database. Please Wait...";
}

function Confirm_Delete() {
    $("#db_overlay").css("display", "block");
    $("#db_overlay").css("visibility", "visible");
    $("#db_modal").css("display", "block");
    $("#db_modal").css("opacity", "1.0");
    $("#db_modal").css("filter", "alpha(opacity=100)");
    $("#hf_buRestore_type").val("Delete");
    $("#MainContent_tb_passwordConfirm").focus();
    dbMessage = "Deleting Backup. Please Wait...";
}

function OnDelete(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete these backups?",
       function () {
           document.getElementById("HiddenField1_sitesettings").value = id;
           if ($("#HiddenField1_sitesettings").val() != "") {
               Confirm_Delete();
           }
       }, function () {
           window.setTimeout(function () {
               openWSE.RemoveUpdateModal();
           }, 500);
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
    openWSE.LoadingMessage1(dbMessage);
    $("#hf_StartWork").val(new Date().toString());
    __doPostBack("hf_StartWork", "");
}