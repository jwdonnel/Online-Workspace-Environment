var changesMade = false;
var groupArray = new Array();

$(document).ready(function () {
    BuildGroupArray();
});

function BuildGroupArray() {
    groupArray = new Array();
    openWSE.AjaxCall("WebServices/GroupSettings.asmx/CheckIfGroupNameExists", "{ }", {
        cache: false
    }, function (data) {
        if (data.d != null) {
            for (var i = 0; i < data.d.length; i++) {
                groupArray.push(data.d[i]);
            }
        }
    });
}

function ResetControls() {
    loadingPopup.Message("Please Wait...");
    if (!openWSE.ConvertBitToBoolean(document.getElementById("hf_reset").value)) {
        document.getElementById("hf_reset").value = "1";
    }
    else {
        document.getElementById("hf_reset").value = "0";
    }

    currentEditId = "";
    openWSE.CallDoPostBack("hf_reset", "");
}

function AddGroupNetworkIP() {
    var ip = $.trim($("#tb_createnew_listener").val());
    if (ip) {
        loadingPopup.Message("Updating. Please Wait...");
        $("#hf_addIp").val(ip);
        openWSE.CallDoPostBack("hf_addIp", "");
    }
}

var currentEditId = "";
var groupEditMode = false;
function EditGroup(id) {
    groupEditMode = true;
    currentEditId = id;
    $("#group-userdefaults-button").show();
    loadingPopup.Message("Updating. Please Wait...");
    document.getElementById("hf_edit").value = id;
    openWSE.CallDoPostBack("hf_edit", "");
}

function SetGroupDefaultsEditBtn() {
    var $groupEditBtn = $("#group-userdefaults-button");
    if ($groupEditBtn.length > 0 && currentEditId != "") {
        $groupEditBtn.attr("onclick", "openWSE.LoadIFrameContent(\"SiteTools/UserTools/AcctSettings.aspx?u=newuserdefaults&groupid=" + currentEditId + "&iframeMode=true\");return false;");
    }
}

function LogoutOfGroup() {
    loadingPopup.Message("Please Wait...");
    document.getElementById("hf_logoutGroup").value = new Date().toString();
    openWSE.CallDoPostBack("hf_logoutGroup", "");
}

function LoginToGroup(id) {
    loadingPopup.Message("Please Wait...");
    document.getElementById("hf_loginGroup").value = id;
    openWSE.CallDoPostBack("hf_loginGroup", "");
}

function GroupNetwork(id) {
    loadingPopup.Message("Loading. Please Wait...");
    $("#hf_groupNetwork").val(id);
    openWSE.CallDoPostBack("hf_groupNetwork", "");
}

function InviteToGroup(id) {
    loadingPopup.Message("Loading. Please Wait...");
    inviteGroupId = id;
    $("#hf_inviteUser").val(id);
    openWSE.CallDoPostBack("hf_inviteUser", "");
}

var inviteGroupId = "";
function invite_click() {
    loadingPopup.Message("Updating. Please Wait...");
    var userList = "";
    $(".invite-tb-list").each(function (index) {
        if ($(this).is(":checked")) {
            userList += $(this).val() + ",";
        }
    });
    $("#hf_inviteUserList").val(userList);
    $("#hf_finishInviteUser").val(inviteGroupId);
    inviteGroupId = "";
    openWSE.CallDoPostBack("hf_finishInviteUser", "");
}

function CreateGroup() {
    groupEditMode = false;
    $("#group-userdefaults-button").hide();
    openWSE.LoadModalWindow(true, 'NewEdit-Group-element', 'Create New Group');
    $('#MainContent_tb_companyname').focus()
}

$(document.body).on("click", "#MainContent_btn_finish_add", function () {
    for (var i = 0; i < groupArray.length; i++) {
        if ((!groupEditMode) & ($.trim($("#MainContent_tb_companyname").val()).toString().toLowerCase() == groupArray[i])) {
            $("#MainContent_lbl_error").html("Group name already exists.");
            return false;
        }
    }

    if ($.trim($("#MainContent_tb_companyname").val()) != "") {
        $("#MainContent_lbl_error").html("");
        loadingPopup.Message("Updating. Please Wait...");
        groupEditMode = false;
    }
    else {
        $("#MainContent_lbl_error").html("Please enter a group name.");
        return false;
    }
});

$(document.body).on("click", ".joinquit", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

function DeleteGroup(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this Group? (Doing so will lock out all users associated with this group)",
        function () {
            loadingPopup.Message("Updating. Please Wait...");
            document.getElementById("hf_delete").value = id;
            openWSE.CallDoPostBack("hf_delete", "");
        }, null);
}

var currGroup = "";
function ViewGroup(name) {
    currGroup = name;
    loadingPopup.Message("Updating...");
    document.getElementById("hf_viewusers").value = name;
    openWSE.CallDoPostBack("hf_viewusers", "");
}

function ViewGroup_Standard(name) {
    loadingPopup.Message("Updating...");
    document.getElementById("hf_viewusers_Standard").value = name;
    openWSE.CallDoPostBack("hf_viewusers_Standard", "");
}

function ClearViewGroup() {
    if (document.getElementById("associatedusers_clear") != null) {
        document.getElementById("associatedusers_clear").innerHTML = "";
    }
}

function RemoveUser(_this, username) {
    changesMade = true;
    var tempUsername = username;
    var tempGroup = currGroup;

    try {
        var $this = $(_this).closest(".app-icon-admin-group");
        if ($this.length > 0) {
            $this.remove();
            var $div = $this.find(".img-collapse-sml");
            $div.removeClass("img-collapse-sml");
            $div.addClass("img-expand-sml");
            var $a = $this.find("a");
            var click = $a.attr("onclick");
            click = click.replace("RemoveUser", "AddUser");
            $a.attr("onclick", click);
            $("#package-removed").prepend($this);
        }
        else {
            tempUsername = _this;
        }
    }
    catch (evt) {
        tempUsername = _this;
    }


    if (currGroup == "") {
        tempGroup = username;
    }

    loadingPopup.Message("Updating...");
    document.getElementById("hf_removeuser").value = tempUsername + ";" + tempGroup;
    openWSE.CallDoPostBack("hf_removeuser", "");
}

function AddUser(_this, username) {
    changesMade = true;
    var tempUsername = username;
    var tempGroup = currGroup;
    $("#noUsersdiv").remove();
    try {
        var $this = $(_this).closest(".app-icon-admin-group");
        if ($this.length > 0) {
            $this.remove();
            var $div = $this.find(".img-expand-sml");
            $div.removeClass("img-expand-sml");
            $div.addClass("img-collapse-sml");
            var $a = $this.find("a");
            var click = $a.attr("onclick");
            click = click.replace("AddUser", "RemoveUser");
            $a.attr("onclick", click);
            $("#package-added").prepend($this);
        }
        else {
            tempUsername = _this;
        }
    }
    catch (evt) {
        tempUsername = _this;
    }

    if (currGroup == "") {
        tempGroup = username;
    }

    loadingPopup.Message("Updating...");
    document.getElementById("hf_adduser").value = tempUsername + ";" + tempGroup;
    openWSE.CallDoPostBack("hf_adduser", "");
}

function RefreshList() {
    currGroup = "";
    $("#MainContent_pnl_users").html("");
    $("#MainContent_pnl_modalTitle").html("");
    if (changesMade) {
        changesMade = false;
        loadingPopup.Message("Updating List...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        openWSE.CallDoPostBack("hf_refreshList", "");
    }
}

function UpdateGroupNetwork(id) {
    $("#MainContent_ipMessage").html("");
    loadingPopup.Message("Updating...");
    $("#hf_groupNetwork_Update").val(id);
    openWSE.CallDoPostBack("hf_groupNetwork_Update", "");
}

function DeleteGroupNetwork(id) {
    $("#MainContent_ipMessage").html("");
    openWSE.ConfirmWindow("Are you sure you want to delete this Ip Address?",
        function () {
            loadingPopup.Message("Deleting...");
            $("#hf_groupNetwork_Delete").val(id);
            openWSE.CallDoPostBack("hf_groupNetwork_Delete", "");
        }, null);
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    SetGroupDefaultsEditBtn();
});