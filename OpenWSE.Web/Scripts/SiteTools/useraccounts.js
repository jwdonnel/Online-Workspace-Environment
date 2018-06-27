$(document).ready(function () {
    CloseEditUserContent();
});

function CreateNewMultiUserInput() {
    var newMultiUserInput = "<div><div class='textbox-group-padding float-left'><div class='textbox-group'>";
    newMultiUserInput += "<div class='username-login-img'></div>";
    newMultiUserInput += "<input type='text' value='' class='pending-add-input signintextbox' onkeypress='onMultiUserEnterAdd(event)' placeholder='Username' autocomplete='off' />";
    newMultiUserInput += "</div></div>";
    newMultiUserInput += "<a href='#' class='add-multi-user-btn float-left margin-top' title='Add new user field' onclick='AddNewMultiUser();return false;'><span class='td-add-btn'></span></a>";
    newMultiUserInput += "<div class='clear-space'></div></div>";
    return newMultiUserInput;
}

function ConfirmDeleteRole(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this role? Any users associated with this role will be changed to the Standard role.",
        function () {
            loadingPopup.Message('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

function CreateMultipleUsers() {
    openWSE.LoadModalWindow(false, "NewUser-element", "");

    $("#multiusercreate").html(CreateNewMultiUserInput());

    openWSE.LoadModalWindow(true, "Multiple-User-Create-element", "Create Multiple Users");
}
function AddNewMultiUser() {
    if ($.trim($(".pending-add-input").val()) == "") {
        return false;
    }
    else {
        $("#multiusercreate").find(".add-multi-user-btn").each(function () {
            $(this).attr("onclick", "RemoveNewMultiUser(this);return false;");
            $(this).attr("title", "Remove new user field");
            $(this).removeClass("add-multi-user-btn");
            $(this).addClass("del-multi-user-btn");
            $(this).find("span").removeClass("td-add-btn");
            $(this).find("span").addClass("td-delete-btn");
            $(this).parent().find("input").removeClass("pending-add-input");
            $(this).parent().find("input").addClass("user-added-input");
        });

        $("#multiusercreate").append(CreateNewMultiUserInput());
    }
    $(".pending-add-input").focus();
}
function FinishCreateMultiUsers() {
    $("#multiuserpasswordrequired").hide();
    $("#multiuserconfirmpasswordrequired").hide();
    $("#userlistrequired").hide();

    var error = false;

    if ($("#MainContent_txt_multiUser_password").val() == "") {
        $("#multiuserpasswordrequired").show();
        error = true;
    }

    if ($("#MainContent_txt_multiUser_confirmpassword").val() == "") {
        $("#multiuserconfirmpasswordrequired").show();
        error = true;
    }

    if ($("#multiusercreate").find(".user-added-input").length == 0) {
        $("#userlistrequired").show();
        error = true;
    }

    if ($("#MainContent_txt_multiUser_confirmpassword").val() != $("#MainContent_txt_multiUser_password").val()) {
        openWSE.AlertWindow("Confirm password must match password.");
        error = true;
    }

    var userList = "";

    $("#multiusercreate").find(".user-added-input").each(function () {
        var val = $.trim($(this).val().replace(/;/g, ""));
        if (val != "") {
            userList += val + ";";
        }
    });

    if (userList == "") {
        $("#userlistrequired").show();
        error = true;
    }

    if (error) {
        return;
    }

    loadingPopup.Message("Creating Users...");

    $("#MainContent_hf_createMultiUsers").val(userList);
    openWSE.CallDoPostBack("MainContent_hf_createMultiUsers", "");
}

function onMultiUserEnterAdd(event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13') {
        AddNewMultiUser();
        event.preventDefault();
    }
}

function RemoveNewMultiUser(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this field?",
       function () {
           $(_this).parent().remove();
       }, null);
}

function CancelMultipleUsers() {
    openWSE.LoadModalWindow(false, "Multiple-User-Create-element", "");
    openWSE.LoadModalWindow(true, "NewUser-element", "Create New User");
    ClearMultiUserFields();
}

function ClearMultiUserFields() {
    $("#multiusercreate").html("");
    $("#MainContent_txt_multiUser_email").val("");
    $("#MainContent_txt_multiUser_password").val("");
    $("#MainContent_txt_multiUser_confirmpassword").val("");
    $("#multiuserpasswordrequired").hide();
    $("#multiuserconfirmpasswordrequired").hide();
    $("#userlistrequired").hide();
}

function ClearNewUserFields() {
    $("#MainContent_RegisterUser_CreateUserStepContainer_UserName").val("");
    $("#MainContent_RegisterUser_CreateUserStepContainer_Email").val("");
    $("#MainContent_RegisterUser_CreateUserStepContainer_Password").val("");
    $("#MainContent_RegisterUser_CreateUserStepContainer_ConfirmPassword").val("");
    $("#MainContent_RegisterUser_CreateUserStepContainer_tb_firstnamereg").val("");
    $("#MainContent_RegisterUser_CreateUserStepContainer_tb_lastnamereg").val("");

    var $buttonHolder = $("#NewUser-element").find(".ModalButtonHolder");
    if ($buttonHolder.find("#MainContent_RegisterUser_CreateUserStepContainer_CreateUserButton").length == 0) {
        $buttonHolder.prepend($("#MainContent_RegisterUser_CreateUserStepContainer_CreateUserButton"));
    }
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    if (passwordResetOpen) {
        $("#MainContent_ChangeUserPassword_ChangePasswordContainerID_NewPassword").val(passwordResetNewPassword);
        $("#MainContent_ChangeUserPassword_ChangePasswordContainerID_ConfirmNewPassword").val(passwordResetConfirmPassword);
        passwordResetOpen = false;
        passwordResetNewPassword = "";
        passwordResetConfirmPassword = "";
    }

    $(window).resize();
});

var passwordResetOpen = false;
var passwordResetNewPassword = "";
var passwordResetConfirmPassword = "";
prm.add_beginRequest(function () {
    if ($("#MainContent_pwreset_overlay").css("display") == "block" && $("#MainContent_ChangeUserPassword_ChangePasswordContainerID_pnl_changePassword").length > 0) {
        passwordResetOpen = true;
        passwordResetNewPassword = $("#MainContent_ChangeUserPassword_ChangePasswordContainerID_NewPassword").val();
        passwordResetConfirmPassword = $("#MainContent_ChangeUserPassword_ChangePasswordContainerID_ConfirmNewPassword").val();
    }
});

function ShowPasswordChangeLoadingMessage() {
    setTimeout(function () {
        var $this1 = $("#MainContent_pwreset_overlay").find("#MainContent_ChangeUserPassword_ChangePasswordContainerID_NewPasswordRequired");
        var $this2 = $("#MainContent_pwreset_overlay").find("#MainContent_ChangeUserPassword_ChangePasswordContainerID_ConfirmNewPasswordRequired");
        if ((!$this1.is(":visible") && !$this2.is(":visible")) || ($this1.css("visibility") == "hidden" && $this2.css("display") == "none")) {
            loadingPopup.Message("Updating. Please Wait...");
        }
    }, 250);
}

$(document.body).on("click", ".continue-create-user", function () {
    var $_passwordReq = $("#MainContent_RegisterUser_CreateUserStepContainer_PasswordRequired");
    var $_passwordComp = $("#MainContent_RegisterUser_CreateUserStepContainer_PasswordCompare");
    var $_email = $("#MainContent_RegisterUser_CreateUserStepContainer_EmailRequired");
    var $_username = $("#MainContent_RegisterUser_CreateUserStepContainer_UserNameRequired");

    var hidden1 = $_passwordReq.css("visibility");
    var none1 = $_passwordReq.css("display");
    var hidden2 = $_passwordComp.css("visibility");
    var none2 = $_passwordComp.css("display");
    var hidden3 = $_email.css("visibility");
    var none3 = $_email.css("display");
    var hidden4 = $_username.css("visibility");
    var none4 = $_username.css("display");

    if (((hidden1 == "hidden") || (none1 == "none")) && ((hidden2 == "hidden") || (none2 == "none")) && ((hidden3 == "hidden") || (none3 == "none")) && ((hidden4 == "hidden") || (none4 == "none"))) {
        loadingPopup.Message('Updating. Please Wait...');
    }
    else {
        setTimeout(function () {
            $(".failureNotification").css("visibility", "hidden");
            $(".failureNotification").css("display", "none")
        }, 3000);
    }
});

var isEditUser = false;
function CloseEditUserContent() {
    var url = location.href;
    if (url.indexOf("AcctSettings") == -1) {
        if (isEditUser) {
            isEditUser = false;
            loadingPopup.Message("Updating List...");
            document.getElementById("hf_refreshList").value = new Date().toString();
            openWSE.CallDoPostBack("hf_refreshList", "");
        }
    }
    else {
        isEditUser = true;
    }
}

function DeleteUser(user) {
    openWSE.ConfirmWindow("Are you sure you want to delete this User?",
      function () {
          loadingPopup.Message('Deleting. Please Wait...');
          document.getElementById("hf_deleteUser").value = user;
          openWSE.CallDoPostBack("hf_deleteUser", "");
          return true;
      }, function () {
          return false;
      });
}

function LockUser(user) {
    document.getElementById("hf_lockUser").value = user;
    openWSE.CallDoPostBack("hf_lockUser", "");
}

function ResetPassword(user) {
    loadingPopup.Message("Loading Password Reset...");
    document.getElementById("hf_resetPassword").value = user;
    openWSE.CallDoPostBack("hf_resetPassword", "");
}

function EmailUser(user) {
    loadingPopup.Message("Updating...");
    document.getElementById("hf_emailUser").value = user;
    openWSE.CallDoPostBack("hf_emailUser", "");
}

function CancelEmailUser(user) {
    loadingPopup.Message("Updating...");
    document.getElementById("hf_noemailUser").value = user;
    openWSE.CallDoPostBack("hf_noemailUser", "");
}

$(document.body).on("change", "#MainContent_ddl_pageSize", function () {
    loadingPopup.Message("Changing Page Size...");
});

$(function () {
    $(window).hashchange(function () {
        CloseEditUserContent();
    });
});

function EditUserApps(userId) {
    loadingPopup.Message("Loading...");
    document.getElementById("hf_editUserApps").value = userId;
    openWSE.CallDoPostBack("hf_editUserApps", "");
}

function CancelUserApps() {
    loadingPopup.Message("Loading...");
    document.getElementById("hf_editUserApps").value = "cancel";
    openWSE.CallDoPostBack("hf_editUserApps", "");
}

function RemovePlugin(id) {
    loadingPopup.Message("Removing Plugin...");
    document.getElementById("hf_removePlugin").value = id;
    openWSE.CallDoPostBack("hf_removePlugin", "");
}

function AddPlugin(id) {
    loadingPopup.Message("Adding Plugin...");
    document.getElementById("hf_addPlugin").value = id;
    openWSE.CallDoPostBack("hf_addPlugin", "");
}

function AddApp(id) {
    loadingPopup.Message("Adding App...");
    document.getElementById("hf_addApp").value = id;
    openWSE.CallDoPostBack("hf_addApp", "");
}

function RemoveApp(id) {
    loadingPopup.Message("Uninstalling App...");
    document.getElementById("hf_removeApp").value = id;
    openWSE.CallDoPostBack("hf_removeApp", "");
}

function RemoveAllPlugins() {
    loadingPopup.Message("Uninstalling All Plugins...");
    document.getElementById("hf_removeAllPlugins").value = new Date().toString();
    openWSE.CallDoPostBack("hf_removeAllPlugins", "");
}

function RemoveAllApp() {
    loadingPopup.Message("Uninstalling All Apps...");
    document.getElementById("hf_removeAllApp").value = new Date().toString();
    openWSE.CallDoPostBack("hf_removeAllApp", "");
}

function AppPackageInstall() {
    loadingPopup.Message("Installing App Package...");
    var id = document.getElementById("ddl_appPackages").value;
    document.getElementById("hf_appPackage").value = id;
    openWSE.CallDoPostBack("hf_appPackage", "");
}
