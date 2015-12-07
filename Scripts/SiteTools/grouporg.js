var changesMade = false;
var groupArray = new Array();

$(document).ready(function () {
    BuildGroupArray();

    $("#MainContent_tb_search").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetListOfGroups",
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

$(document.body).on("click", ".searchbox_clear", function () {
    openWSE.LoadingMessage1("Updating...");
    $('#MainContent_tb_search').val('Search Groups');
    $("#hf_clearsearch").val(new Date().toString());
    __doPostBack("hf_clearsearch", "");
});

function BuildGroupArray() {
    groupArray = new Array();
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/GroupSettings.asmx/CheckIfGroupNameExists",
        data: "{ }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            if (data.d != null) {
                for (var i = 0; i < data.d.length; i++) {
                    groupArray.push(data.d[i]);
                }
            }
        }
    });
}

function ResetControls() {
    openWSE.LoadingMessage1("Please Wait...");
    if (!openWSE.ConvertBitToBoolean(document.getElementById("hf_reset").value)) {
        document.getElementById("hf_reset").value = "1";
    }
    else {
        document.getElementById("hf_reset").value = "0";
    }

    currentEditId = "";
    __doPostBack("hf_reset", "");
}

var currentEditId = "";
var groupEditMode = false;
function EditGroup(id) {
    groupEditMode = true;
    currentEditId = id;
    $("#group-userdefaults-button").show();
    openWSE.LoadingMessage1("Updating. Please Wait...");
    document.getElementById("hf_edit").value = id;
    __doPostBack("hf_edit", "");
}

function SetGroupDefaultsEditBtn() {
    var $groupEditBtn = $("#group-userdefaults-button");
    if ($groupEditBtn.length > 0 && currentEditId != "") {
        $groupEditBtn.attr("onclick", "openWSE.LoadIFrameContent(\"SiteTools/UserMaintenance/AcctSettings.aspx?toolView=true&u=newuserdefaults&groupid=" + currentEditId + "\", this);return false;");
    }
}

function LogoutOfGroup() {
    openWSE.LoadingMessage1("Please Wait...");
    document.getElementById("hf_logoutGroup").value = new Date().toString();
    __doPostBack("hf_logoutGroup", "");
}

function LoginToGroup(id) {
    openWSE.LoadingMessage1("Please Wait...");
    document.getElementById("hf_loginGroup").value = id;
    __doPostBack("hf_loginGroup", "");
}

function GroupNetwork(id) {
    openWSE.LoadingMessage1("Loading. Please Wait...");
    $("#hf_groupNetwork").val(id);
    __doPostBack("hf_groupNetwork", "");
}

function InviteToGroup(id) {
    openWSE.LoadingMessage1("Loading. Please Wait...");
    inviteGroupId = id;
    $("#hf_inviteUser").val(id);
    __doPostBack("hf_inviteUser", "");
}

var inviteGroupId = "";
function invite_click() {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    var userList = "";
    $(".invite-tb-list").each(function (index) {
        if ($(this).is(":checked")) {
            userList += $(this).val() + ",";
        }
    });
    $("#hf_inviteUserList").val(userList);
    $("#hf_finishInviteUser").val(inviteGroupId);
    inviteGroupId = "";
    __doPostBack("hf_finishInviteUser", "");
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
        openWSE.LoadingMessage1("Updating. Please Wait...");
        groupEditMode = false;
    }
    else {
        $("#MainContent_lbl_error").html("Please enter a group name.");
        return false;
    }
});

$(document.body).on("click", ".joinquit", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

function DeleteGroup(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this Group? (Doing so will lock out all users associated with this group)",
        function () {
            openWSE.LoadingMessage1("Updating. Please Wait...");
            document.getElementById("hf_delete").value = id;
            __doPostBack("hf_delete", "");
        }, null);
}

var currGroup = "";
function ViewGroup(name) {
    currGroup = name;
    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_viewusers").value = name;
    __doPostBack("hf_viewusers", "");
}

function ViewGroup_Standard(name) {
    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_viewusers_Standard").value = name;
    __doPostBack("hf_viewusers_Standard", "");
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

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_removeuser").value = tempUsername + ";" + tempGroup;
    __doPostBack("hf_removeuser", "");
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

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_adduser").value = tempUsername + ";" + tempGroup;
    __doPostBack("hf_adduser", "");
}

function RefreshList() {
    currGroup = "";
    $("#MainContent_pnl_users").html("");
    $("#MainContent_pnl_modalTitle").html("");
    if (changesMade) {
        changesMade = false;
        openWSE.LoadingMessage1("Updating List...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        __doPostBack("hf_refreshList", "");
    }
}

function UpdateGroupNetwork(id) {
    $("#MainContent_ipMessage").html("");
    openWSE.LoadingMessage1("Updating...");
    $("#hf_groupNetwork_Update").val(id);
    __doPostBack("hf_groupNetwork_Update", "");
}

function DeleteGroupNetwork(id) {
    $("#MainContent_ipMessage").html("");
    openWSE.ConfirmWindow("Are you sure you want to delete this Ip Address?",
        function () {
            openWSE.LoadingMessage1("Deleting...");
            $("#hf_groupNetwork_Delete").val(id);
            __doPostBack("hf_groupNetwork_Delete", "");
        }, null);
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    $(".RadioButton-Toggle-Overlay").remove();
    openWSE.RadioButtonStyle();
    SetGroupDefaultsEditBtn();
});