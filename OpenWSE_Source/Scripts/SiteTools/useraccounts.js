$(document).ready(function () {
    CloseEditUserContent();
    $("#MainContent_tb_search").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetListOfUsers",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});

function CreateNewMultiUserInput() {
    var newMultiUserInput = "<div class='pad-bottom'><input type='text' value='' class='pending-add-input textEntryReg' onkeypress='onMultiUserEnterAdd(event)' style='width: 215px;' />";
    newMultiUserInput += "<a href='#' class='add-multi-user-btn' title='Add new user field' onclick='AddNewMultiUser();return false;'><span class='td-add-btn margin-left' style='padding:0px!important;'></span></a>";
    newMultiUserInput += "</div>";
    return newMultiUserInput;
}

function ConfirmDeleteRole(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this role? Any users associated with this role will be changed to the Standard role.",
        function () {
            openWSE.LoadingMessage1('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
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

    openWSE.LoadingMessage1("Creating Users...");

    $("#MainContent_hf_createMultiUsers").val(userList);
    __doPostBack("MainContent_hf_createMultiUsers", "");
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
    $("#MainContent_RegisterUser_CreateUserStepContainer_Color1").val("FFFFFF");
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    $("#MainContent_pwreset_overlay").find(".Modal-element-modal").draggable({
        containment: "container",
        cancel: '.ModalPadContent, .ModalExitButton',
        drag: function (event, ui) {
            var $this = $(this);
            $this.css("opacity", "0.6");
            $this.css("filter", "alpha(opacity=60)");

            // Apply an overlay over app
            // This fixes the issues when dragging iframes
            if ($this.find("iframe").length > 0) {
                var $_id = $this.find(".ModalPadContent");
                $wo = $_id.find(".app-overlay-fix");
                if ($wo.length == 0) {
                    if ($_id.length == 1) {
                        $_id.append("<div class='app-overlay-fix'></div>");
                    }
                }
            }
        },
        stop: function (event, ui) {
            var $this = $(this);
            $this.css("opacity", "1.0");
            $this.css("filter", "alpha(opacity=100)");
            $wo = $(this).find(".app-overlay-fix");
            if ($wo.length == 1) {
                $wo.remove();
            }
        }
    });

    $("#MainContent_pwreset_overlay").find(".Modal-element-align").css({
        marginTop: -($("#MainContent_pwreset_overlay").find(".Modal-element-modal").height() / 2),
        marginLeft: -($("#MainContent_pwreset_overlay").find(".Modal-element-modal").width() / 2)
    });

    $(window).resize();
    if ($("#MainContent_tb_search").val() == "") {
        $("#MainContent_tb_search").val("Search Users");
    }

    $("#MainContent_tb_search").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetListOfUsers",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});

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
        openWSE.LoadingMessage1('Updating. Please Wait...');
    }
    else {
        setTimeout(function () {
            $(".failureNotification").css("visibility", "hidden");
            $(".failureNotification").css("display", "none")
        }, 3000);
    }
});

$(document.body).on("change", "#MainContent_ddl_sortby", function () {
    openWSE.LoadingMessage1('Updating. Please Wait...');
});


var isEditUser = false;
function CloseEditUserContent() {
    var url = location.href;
    if (url.indexOf("AcctSettings") == -1) {
        if (isEditUser) {
            isEditUser = false;
            openWSE.LoadingMessage1("Updating List...");
            document.getElementById("hf_refreshList").value = new Date().toString();
            __doPostBack("hf_refreshList", "");
        }
    }
    else {
        isEditUser = true;
    }
}

function DeleteUser(user) {
    openWSE.ConfirmWindow("Are you sure you want to delete this User?",
      function () {
          openWSE.LoadingMessage1('Deleting. Please Wait...');
          document.getElementById("hf_deleteUser").value = user;
          __doPostBack("hf_deleteUser", "");
          return true;
      }, function () {
          return false;
      });
}

function LockUser(user) {
    document.getElementById("hf_lockUser").value = user;
    __doPostBack("hf_lockUser", "");
}

function ResetPassword(user) {
    openWSE.LoadingMessage1("Loading Password Reset...");
    document.getElementById("hf_resetPassword").value = user;
    __doPostBack("hf_resetPassword", "");
}

function EmailUser(user) {
    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_emailUser").value = user;
    __doPostBack("hf_emailUser", "");
}

function CancelEmailUser(user) {
    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_noemailUser").value = user;
    __doPostBack("hf_noemailUser", "");
}

$(document.body).on("change", "#MainContent_ddl_pageSize", function () {
    openWSE.LoadingMessage1("Changing Page Size...");
});

$(document.body).on("click", ".searchbox_clear", function () {
    openWSE.LoadingMessage1("Updating...");
    $('#MainContent_tb_search').val('Search Users');
    $("#hf_clearsearch").val(new Date().toString());
    __doPostBack("hf_clearsearch", "");
});

$(function () {
    $(window).hashchange(function () {
        CloseEditUserContent();
    });
});