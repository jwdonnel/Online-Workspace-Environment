var dbType = "";
var dbMessage = "";
Sys.Application.add_load(function () {
    switch (dbType) {
        case "restore":
            Confirm_Restore();
            break;
    }
    dbType = "";
});

function EditSlot(id) {
    document.getElementById("hf_EditSlot").value = id;
    openWSE.CallDoPostBack("hf_EditSlot", "");
}

$(document.body).on("change", "#cbAutoFixDB, #dd_defaultTableList, #MainContent_dd_backupstoshow", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

function DeleteSlot(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this auto backup?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           document.getElementById("hf_DeleteSlot").value = id;
           openWSE.CallDoPostBack("hf_DeleteSlot", "");
       }, null);
}

function RestoreDefaultValues() {
    openWSE.ConfirmWindow("Are you sure you want to restore the default table values to this database?",
       function () {
           loadingPopup.Message("Restoring Defaults...");
           document.getElementById("hf_restoreDefaults").value = new Date().toString();
           openWSE.CallDoPostBack("hf_restoreDefaults", "");
       }, null);
}

$(document.body).on("click", "#MainContent_btn_checkDatabase", function () {
    loadingPopup.Message("Scanning Database. Please Wait...");
});

$(document.body).on("click", "#MainContent_btn_UpdateDatabase", function () {
    loadingPopup.Message("Updating Database. Please Wait...");
});

$(document.body).on("click", ".RandomActionBtns-backup-perform", function () {
    loadingPopup.Message("Backing Up Database...");
});

function UpdateSlot(id) {
    document.getElementById("hf_dayofweek_edit").value = $("#ddl_dayofweek_Edit").val();
    document.getElementById("hf_timeofweek_edit").value = $("#ddl_timeofweekHour_Edit").val() + ":" + $("#ddl_timeofweekMin_Edit").val() + " " + $("#ddl_timeofweekAmPm_Edit").val();
    document.getElementById("hf_backuptype_edit").value = $("#ddl_backuptype_Edit").val();
    document.getElementById("hf_UpdateSlot").value = id;
    openWSE.CallDoPostBack("hf_UpdateSlot", "");
}

function CancelSlot() {
    document.getElementById("hf_CancelSlot").value = new Date().toString();
    openWSE.CallDoPostBack("hf_CancelSlot", "");
}

$(document.body).on("click", ".dbviewer-update-img, .dbviewer-update a", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

$(document.body).on("change", "#MainContent_dd_recoverymodal", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

function Confirm_Backup() {
    dbMessage = "Backing Up Database. Please Wait...";
}

function StartBackingUp() {
    openWSE.LoadModalWindow(false, "tablestorestore-element", '');
    loadingPopup.Message(dbMessage);
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

function AddNewAutoBackupClick() {
    loadingPopup.Message("Updating. Please Wait...");
    var time = $.trim($("#ddl_absBackupTimeHour").val()) + ":" + $.trim($("#ddl_absBackupTimeMin").val()) + " " + $.trim($("#ddl_absBackupTimeAmPm").val());
    $("#hf_autoBackup_day").val($.trim($("#ddl_absDaytoRun").val()));
    $("#hf_autoBackup_time").val(time);
    $("#hf_autoBackup_type").val($.trim($("#ddl_absBackupType").val()));
    $("#lbtn_addAbs").val(new Date().toString());
    openWSE.CallDoPostBack("lbtn_addAbs", "");
}


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */

function Confirm_Restore() {
    dbMessage = "Building Compatible Tables List...";
    $("#hf_buRestore_type").val("Restore");
    if (UserIsSocialAccount != null && UserIsSocialAccount) {
        $("#MainContent_btn_passwordConfirm").trigger("click");
    }
    else {
        openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
        $("#MainContent_tb_passwordConfirm").focus();
    }
}

function Confirm_Delete() {
    dbMessage = "Deleting Backup. Please Wait...";
    $("#hf_buRestore_type").val("Delete");
    if (UserIsSocialAccount != null && UserIsSocialAccount) {
        $("#MainContent_btn_passwordConfirm").trigger("click");
    }
    else {
        openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
        $("#MainContent_tb_passwordConfirm").focus();
    }
}

function OnRestore(id) {
    loadingPopup.Message("Please Wait...");
    dbType = "restore";
    $("#lb_restorethis").val(id);
    openWSE.CallDoPostBack("lb_restorethis", "");
}

function OnDownload(filePath) {
    $.fileDownload(unescape(filePath));
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
               loadingPopup.RemoveMessage();
           }, 500);
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
        loadingPopup.Message(dbMessage);
    }, 100);
    $("#hf_StartWork").val(new Date().toString());
    openWSE.CallDoPostBack("hf_StartWork", "");
}