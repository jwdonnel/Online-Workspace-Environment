$(document).ready(function () {
    BuildLinks();

    var url = location.href;
    load(url == "" ? "1" : url);
});

$(function () {
    $(window).hashchange(function () {
        var url = location.href;
        load(url == "" ? "1" : url);
    });
});

var currentTab = "";
function BuildLinks() {
    $(".pnl-section").each(function (index) {
        var id = $(this).attr("id").replace("pnl_", "");
        $(".sitemenu-selection").append("<li><a href='#?tab=" + id + "'>" + $(this).attr("data-title") + "</a></li>");
    });

    $(".sitemenu-selection").find("li").on("click", function () {
        load($(this).find("a").attr("href"));
    });
}

Sys.Application.add_load(function () {
    openWSE.RadioButtonStyle();
    if (learMoreOn) {
        $("#moreInfo-PrivateAccount").show();
    }
    else {
        $("#moreInfo-PrivateAccount").hide();
    }
});

$(document.body).on("keypress", "#MainContent_tb_backgroundlooptimer", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    load(currentTab);
});

$(document.body).on("keypress", "#tb_updateintervals, #txt_AppGridSize", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

function InitializeSiteAnimationSpeed(speed) {
    $("#currentAnimationSpeed").html("<b class='pad-right-sml'>Animation Speed:</b>" + speed + " ms");
    $("#Slider2").slider({
        range: "min",
        min: 0,
        max: 2000,
        step: 5,
        value: speed,
        slide: function (event, ui) {
            $("#currentAnimationSpeed").html("<b class='pad-right-sml'>Animation Speed:</b>" + ui.value + " ms");
        },
        stop: function (event, ui) {
            $("#btnUpdateAnimiation").show();
            $("#hf_AnimationSpeed").val(ui.value);
        }
    });
}

function UpdateAnimationSpeed() {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    __doPostBack("hf_AnimationSpeed", "");
}

function ResetAnimationSpeed() {
    $("#Slider2").slider({ value: 200 });
    $("#currentAnimationSpeed").html("<b class='pad-right-sml'>Animation Speed:</b>200 ms");
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $("#hf_AnimationSpeed").val("200");
    __doPostBack("hf_AnimationSpeed", "");
}

var learMoreOn = false;
function LearnMore() {
    if ($("#moreInfo-PrivateAccount").css("display") == "none") {
        $("#moreInfo-PrivateAccount").fadeIn(openWSE_Config.animationSpeed);
        learMoreOn = true;
    }
    else {
        $("#moreInfo-PrivateAccount").fadeOut(openWSE_Config.animationSpeed);
        learMoreOn = false;
    }
}

function HideSidebar_AccountSettings(x) {
    if (x == 0) {
        $('#sidebar-padding-accountsettings').fadeOut(openWSE_Config.animationSpeed, function () {
            $("#showsidebar-accountsettings").css("display", "block");
        });
    }
    else {
        $("#showsidebar-accountsettings").css("display", "none");
        $('#sidebar-padding-accountsettings').fadeIn(openWSE_Config.animationSpeed);
    }
    return false;
}

function UpdateEnabled_notification(id) {
    openWSE.LoadingMessage1("Enabling Notification...");
    document.getElementById("hf_updateEnabled_notification").value = id;
    __doPostBack("hf_updateEnabled_notification", "");
}

function UpdateDisabled_notification(id) {
    openWSE.LoadingMessage1("Disabling Notification...");
    document.getElementById("hf_updateDisabled_notification").value = id;
    __doPostBack("hf_updateDisabled_notification", "");
}

function UpdateEmail_notification(_this, id) {
    openWSE.LoadingMessage1("Updating Notification...");
    document.getElementById("hf_collId_notification").value = id;
    if ($(_this).attr("checked") == "checked") {
        document.getElementById("hf_updateEmail_notification").value = "1";
    }
    else {
        document.getElementById("hf_updateEmail_notification").value = "0";
    }
    __doPostBack("hf_updateEmail_notification", "");
}

function UpdateEnabled_overlay(id) {
    openWSE.LoadingMessage1("Enabling Overlay...");
    document.getElementById("hf_updateEnabled_overlay").value = id;
    __doPostBack("hf_updateEnabled_overlay", "");
}

function UpdateDisabled_overlay(id) {
    openWSE.LoadingMessage1("Disabling Overlay...");
    document.getElementById("hf_updateDisabled_overlay").value = id;
    __doPostBack("hf_updateDisabled_overlay", "");
}

function addAdminPage(id) {
    document.getElementById('hf_addAdminPage').value = id;
    __doPostBack('hf_addAdminPage', "");
}

function removeAdminPage(id) {
    document.getElementById('hf_removeAdminPage').value = id;
    __doPostBack('hf_removeAdminPage', "");
}

function addGroup(id) {
    document.getElementById('hf_addGroup').value = id;
    __doPostBack('hf_addGroup', "");
}

function removeGroup(id) {
    document.getElementById('hf_removeGroup').value = id;
    __doPostBack('hf_removeGroup', "");
}

$(document.body).on("change", "#dd_theme, #dd_backgroundSelector, #dd_imageFolder", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("click", "#lb_clearbackground", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("click", ".updatesettings", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});


/* Background Selector Functions
----------------------------------*/
$(document.body).on("click", ".bg-selectors", function () {
    var bc = $(this).css("background-color");
    var i = $(this).css("background-color").length - 1;
    bc = bc.substring(4, i);
    c = bc;
    var v = 'rgba(' + bc + ', ' + (($('#Slider1').slider('value'))) + ');';
    $('#newsettings').html(v);
    $('#app_panel_appearance_div').css('background-color', v);
    $(".bg-selectors").removeClass('selected');
    $(this).addClass('selected');
    $('#hf_opacity').val($('#Slider1').slider('value'));
    $('#hf_panelcolor').val(c);
});

function CurrBackground_panel(x) {
    $('#backgroundcolors_div div').each(function (index) {
        var bc = $(this).css("background-color");
        var i = $(this).css("background-color").length - 1;
        bc = bc.substring(4, i);
        bc = bc.replace("(", "");
        var n = bc.split(", ");
        if (n.length >= 3) {
            bc = n[0] + ", " + n[1] + ", " + n[2];
            if (bc == x) {
                $(this).addClass('selected');
            }
            else {
                $(this).removeClass('selected');
            }
        }
    });
}

function BackgroundSelector() {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    document.getElementById('pnl_images').innerHTML = "";
    document.getElementById('hf_backgroundselector').value = new Date().toString();
    __doPostBack('hf_backgroundselector', "");
}

$(document.body).on("click", "#pnl_images .image-selector-acct", function (e) {
    if (e.target.className != "delete-uploadedimg") {
        var id = $(this).find("img").attr("src");
        var bi = document.getElementById('hf_backgroundimg');
        if (bi.value != id) {
            bi.value = id;

            openWSE.LoadingMessage1("Updating. Please Wait...");
            __doPostBack('hf_backgroundimg', "");
        }
    }
});

$(document.body).on("click", "#pnl_images .image-selector-active", function (e) {
    if (e.target.className != "delete-uploadedimg") {
        var id = $(this).find("img").attr("src");
        var bi = document.getElementById('hf_removebackgroundimgEdit');
        if (bi.value != id) {
            bi.value = id;

            openWSE.LoadingMessage1("Updating. Please Wait...");
            __doPostBack('hf_removebackgroundimgEdit', "");
        }
    }
});

$(document.body).on("click", "#CurrentBackground .remove-background-img", function () {
    var img = $(this).attr("data-imgsrc");
    openWSE.LoadingMessage1("Updating. Please Wait...");
    document.getElementById('hf_removebackgroundimg').value = img;
    __doPostBack('hf_removebackgroundimg', "");
});

$(document.body).on("click", "#pnl_images .delete-uploadedimg", function () {
    var img = $(this).attr("data-imgsrc");
    openWSE.ConfirmWindow("Are you sure you want to delete " + img + "?",
       function () {
           openWSE.LoadingMessage1("Deleting. Please Wait...");
           document.getElementById('hf_deleteUploadedImage').value = img;
           __doPostBack('hf_deleteUploadedImage', "");
       }, null);
});

function load(num) {
    $(".pnl-section").hide();
    $(".sitemenu-selection").find("li").removeClass("active");

    currentTab = num;
    var index = 0;

    var arg1 = num.split("tab=");
    if (arg1.length > 1) {
        var arg2 = arg1[arg1.length - 1].split("#");
        if (arg2.length == 1) {
            index = GetPnlSectionIndex(arg2[0]);
        }
    }

    $(".pnl-section").eq(index).show();
    $(".sitemenu-selection").find("li").eq(index).addClass("active");
}

function GetPnlSectionIndex(ele) {
    var pnlIndex = 0;
    $(".pnl-section").each(function (index) {
        if ($(this).attr("id") == "pnl_" + ele) {
            pnlIndex = index;
        }
    });

    return pnlIndex;
}

function DeleteUserAccount() {
    openWSE.ConfirmWindow("Are you sure you want to delete your account? There is no going back if once you click Ok.",
       function () {
           openWSE.LoadingMessage1("Deleting Account. Please Wait...");
           $("#hf_DeleteUserAccount").val(new Date().toString());
           __doPostBack("hf_DeleteUserAccount", "");
       }, null);
}

function DeleteAllOverrides() {
    openWSE.ConfirmWindow("Are you sure you want to delete all your app overrides? There is no going back if once you click Ok.",
       function () {
           openWSE.LoadingMessage1("Deleting Overrides. Please Wait...");
           $("#hf_DeleteUserAppOverrides").val(new Date().toString());
           __doPostBack("hf_DeleteUserAppOverrides", "");
       }, null);
}

var overrideEditId = "";
function DeleteOverrides(id) {
    if (id == "") {
        id = overrideEditId;
    }
    openWSE.ConfirmWindow("Are you sure you want to delete your app overrides? There is no going back if once you click Ok.",
       function () {
           openWSE.LoadingMessage1("Deleting Overrides. Please Wait...");
           $("#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").val(id);
           __doPostBack("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", "");
       }, null);
}

function EditOverrides(id) {
    overrideEditId = id;
    openWSE.LoadingMessage1("Loading Overrides. Please Wait...");
    $("#hf_EditUserAppOverrides").val(id);
    __doPostBack("hf_EditUserAppOverrides", "");
}

function UpdateOverrides() {
    if (overrideEditId != "") {
        openWSE.LoadingMessage1("Saving Overrides. Please Wait...");
        $("#hf_UpdateUserAppOverrides").val(overrideEditId);
        __doPostBack("hf_UpdateUserAppOverrides", "");
    }
}

function RemovePlugin(id) {
    openWSE.LoadingMessage1("Removing Plugin...");
    document.getElementById("hf_removePlugin").value = id;
    __doPostBack("hf_removePlugin", "");
}

function AddPlugin(id) {
    openWSE.LoadingMessage1("Adding Plugin...");
    document.getElementById("hf_addPlugin").value = id;
    __doPostBack("hf_addPlugin", "");
}

function RemoveAllPlugins() {
    openWSE.LoadingMessage1("Uninstalling All Plugins...");
    document.getElementById("hf_removeAllPlugins").value = new Date().toString();
    __doPostBack("hf_removeAllPlugins", "");
}