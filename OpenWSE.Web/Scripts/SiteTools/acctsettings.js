$(document).ready(function () {
    UpdateFontFamilyPreview();
});

$(document.body).on("change", "#tb_backgroundColor_edit", function () {
    if ($("#cb_backgroundColor_edit_default").prop("checked")) {
        $("#cb_backgroundColor_edit_default").trigger("click");
    }
});
$(document.body).on("change", "#tb_iconColor_edit", function () {
    if ($("#cb_iconColor_edit_default").prop("checked")) {
        $("#cb_iconColor_edit_default").trigger("click");
    }
});

function rgbToHex(fontcolor) {
    if (fontcolor.indexOf("#") == 0) {
        return fontcolor;
    }

    fontcolor = fontcolor.toLowerCase().replace("rgb(", "").replace(")", "").replace(/ /g, "");
    var splitColor = fontcolor.split(",");
    if (splitColor.length == 3) {
        var r = splitColor[0];
        var g = splitColor[1];
        var b = splitColor[2];

        return "#" + byte2Hex(r) + byte2Hex(g) + byte2Hex(b);
    }

    return "#515151";
}
function byte2Hex(n) {
    var nybHexString = "0123456789ABCDEF";
    return String(nybHexString.substr((n >> 4) & 0x0F, 1)) + nybHexString.substr(n & 0x0F, 1);
}

function SetDefaultStyles() {
    var style = window.getComputedStyle($("body")[0]);
    if ($.trim($("#tb_defaultfontsize").val()) == "") {
        var fontsize = style.getPropertyValue("font-size");
        if (fontsize) {
            $("#tb_defaultfontsize").val(fontsize.replace("px", ""));
        }
    }

    if ($.trim($("#tb_defaultfontcolor").val()) == "" || $("#tb_defaultfontcolor").hasClass("use-default")) {
        var fontcolor = style.getPropertyValue("color");
        if (fontcolor) {
            $("#tb_defaultfontcolor").val(rgbToHex(fontcolor));
            $("#tb_defaultfontcolor").removeClass("use-default");
        }
    }
}

Sys.Application.add_load(function () {
    if (learMoreOn) {
        $("#moreInfo-PrivateAccount").show();
    }
    else {
        $("#moreInfo-PrivateAccount").hide();
    }

    openWSE.InitializeThemeColorOption("pnl_ColorOptions");
});

$(document.body).on("change", "#dd_enablebg_edit", function () {
    if ($(this).val() == "app-main") {
        $("#backgroundcolorholder_edit").show();
    }
    else {
        $("#backgroundcolorholder_edit").hide();
    }
});

$(document.body).on("keypress", "#MainContent_tb_backgroundlooptimer, #MainContent_tb_defaultfontsize, #tb_updateintervals", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

var backgroundScrollTop = 0;
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function () {
    if ($("#Background-element").css("display") == "block") {
        backgroundScrollTop = $("#Background-element").find(".ModalScrollContent").scrollTop();
    }
});
prm.add_endRequest(function () {
    UpdateFontFamilyPreview();
    if ($("#Background-element").css("display") == "block") {
        $("#Background-element").find(".ModalScrollContent").scrollTop(backgroundScrollTop);
    }

    backgroundScrollTop = 0;
});

$(document.body).on("keypress", "#tb_updateintervals, #txt_AppGridSize", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

$(document.body).on("keypress", "#tb_animationSpeed", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    if (code == 13) {
        e.preventDefault();
        UpdateAnimationSpeed();
    }
    else {
        var val = String.fromCharCode(code);

        if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
            return false;
        }
    }
});

$(document.body).on("click", "#btn_clear_acctImage", function () {
    openWSE.ConfirmWindow('Are you sure you want to clear your image profile?', function () {
        loadingPopup.Message("Updating...");
        $("#hf_clear_acctImage").val(new Date().toString());
        openWSE.CallDoPostBack("hf_clear_acctImage", "");
    }, null);

    return false;
});

function UpdateAnimationSpeed() {
    loadingPopup.Message("Updating. Please Wait...");
    $("#btnUpdateAnimiation").show();
    $("#hf_AnimationSpeed").val($.trim($("#tb_animationSpeed").val()));
    openWSE.CallDoPostBack("hf_AnimationSpeed", "");
}

function ResetAnimationSpeed() {
    $("#tb_animationSpeed").val("200");
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_AnimationSpeed").val("200");
    openWSE.CallDoPostBack("hf_AnimationSpeed", "");
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
    loadingPopup.Message("Enabling Notification...");
    document.getElementById("hf_updateEnabled_notification").value = id;
    document.getElementById("hf_updateDisabled_notification").value = "";
    openWSE.CallDoPostBack("hf_updateEnabled_notification", "");
}

function UpdateDisabled_notification(id) {
    loadingPopup.Message("Disabling Notification...");
    document.getElementById("hf_updateDisabled_notification").value = id;
    document.getElementById("hf_updateEnabled_notification").value = "";
    openWSE.CallDoPostBack("hf_updateDisabled_notification", "");
}

function UpdateEmail_notification(_this, id) {
    loadingPopup.Message("Updating Notification...");
    document.getElementById("hf_collId_notification").value = id;
    if ($(_this).prop("checked") === true) {
        document.getElementById("hf_updateEmail_notification").value = "1";
    }
    else {
        document.getElementById("hf_updateEmail_notification").value = "0";
    }
    openWSE.CallDoPostBack("hf_updateEmail_notification", "");
}

function UpdateEnabled_overlay(id) {
    loadingPopup.Message("Enabling Overlay...");
    document.getElementById("hf_updateEnabled_overlay").value = id;
    openWSE.CallDoPostBack("hf_updateEnabled_overlay", "");
}

function UpdateDisabled_overlay(id) {
    loadingPopup.Message("Disabling Overlay...");
    document.getElementById("hf_updateDisabled_overlay").value = id;
    openWSE.CallDoPostBack("hf_updateDisabled_overlay", "");
}

function addAdminPage(id) {
    document.getElementById('hf_addAdminPage').value = id;
    openWSE.CallDoPostBack('hf_addAdminPage', "");
}

function removeAdminPage(id) {
    document.getElementById('hf_removeAdminPage').value = id;
    openWSE.CallDoPostBack('hf_removeAdminPage', "");
}

function addGroup(id) {
    document.getElementById('hf_addGroup').value = id;
    openWSE.CallDoPostBack('hf_addGroup', "");
}

function removeGroup(id) {
    document.getElementById('hf_removeGroup').value = id;
    openWSE.CallDoPostBack('hf_removeGroup', "");
}

$(document.body).on("change", "#dd_theme, #dd_backgroundSelector, #dd_imageFolder", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

$(document.body).on("click", "#lb_clearbackground", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

$(document.body).on("click", ".updatesettings", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

$(document.body).on("change", "#dd_maxonload_edit", function () {
    var $resize = $("#dd_allowresize_edit");
    var $maximize = $("#dd_allowmax_edit");
    var $minWidth = $("#tb_minwidth_edit");
    var $minHeight = $("#tb_minheight_edit");
    if (openWSE.ConvertBitToBoolean($(this).val())) {
        $resize.attr("disabled", "disabled");
        $maximize.attr("disabled", "disabled");
        $minWidth.attr("disabled", "disabled");
        $minHeight.attr("disabled", "disabled");
    }
    else {
        $resize.removeAttr("disabled");
        $maximize.removeAttr("disabled");
        $minWidth.removeAttr("disabled");
        $minHeight.removeAttr("disabled");
    }
});

$(document.body).on("change", "#rb_sitetipsonload_on, #rb_sitetipsonload_off", function () {
    cookieFunctions.del("siteTipsOnPageLoad");
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
    loadingPopup.Message("Updating. Please Wait...");
    document.getElementById('pnl_images').innerHTML = "";
    document.getElementById('hf_backgroundselector').value = new Date().toString();
    openWSE.CallDoPostBack('hf_backgroundselector', "");
}

$(document.body).on("click", "#pnl_images .image-selector", function (e) {
    if (e.target.className != "delete-uploadedimg") {
        if ($("#CurrentBackground").find(".image-selector").length > 0) {
            var $this = $(this);
            openWSE.ConfirmWindowAltBtns("Do you want to remove the previous selected background(s)? Click No to add the new background to the existing list.",
                function () {
                    var id = $this.find("img").attr("src");
                    var bi = document.getElementById('hf_backgroundimgClearAdd');
                    if (bi.value != id) {
                        bi.value = id;

                        loadingPopup.Message("Updating. Please Wait...");
                        openWSE.CallDoPostBack('hf_backgroundimgClearAdd', "");
                    }
                }, function () {
                    var id = $this.find("img").attr("src");
                    var bi = document.getElementById('hf_backgroundimg');
                    if (bi.value != id) {
                        bi.value = id;

                        loadingPopup.Message("Updating. Please Wait...");
                        openWSE.CallDoPostBack('hf_backgroundimg', "");
                    }
                });
        }
        else {
            var id = $(this).find("img").attr("src");
            var bi = document.getElementById('hf_backgroundimg');
            if (bi.value != id) {
                bi.value = id;

                loadingPopup.Message("Updating. Please Wait...");
                openWSE.CallDoPostBack('hf_backgroundimg', "");
            }
        }
    }
});

$(document.body).on("click", "#CurrentBackground .remove-selectedimg", function () {
    var img = $(this).attr("data-imgsrc");
    loadingPopup.Message("Updating. Please Wait...");
    document.getElementById('hf_removebackgroundimg').value = img;
    openWSE.CallDoPostBack('hf_removebackgroundimg', "");
});

$(document.body).on("click", "#pnl_images .delete-uploadedimg", function () {
    var img = $(this).attr("data-imgsrc");
    openWSE.ConfirmWindow("Are you sure you want to permanently delete " + img + "?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           document.getElementById('hf_deleteUploadedImage').value = img;
           openWSE.CallDoPostBack('hf_deleteUploadedImage', "");
       }, null);
});

function DeleteUserAccount() {
    openWSE.ConfirmWindow("Are you sure you want to delete your account? There is no going back if once you click Ok.",
       function () {
           loadingPopup.Message("Deleting Account. Please Wait...");
           $("#hf_DeleteUserAccount").val(new Date().toString());
           openWSE.CallDoPostBack("hf_DeleteUserAccount", "");
       }, null);
}

function DeleteAllOverrides() {
    openWSE.ConfirmWindow("Are you sure you want to delete all your app overrides? There is no going back if once you click Ok.",
       function () {
           loadingPopup.Message("Deleting Overrides. Please Wait...");
           $("#hf_DeleteUserAppOverrides").val(new Date().toString());
           openWSE.CallDoPostBack("hf_DeleteUserAppOverrides", "");
       }, null);
}

var overrideEditId = "";
function DeleteOverrides(id) {
    if (id == "") {
        id = overrideEditId;
    }
    openWSE.ConfirmWindow("Are you sure you want to delete your app overrides? There is no going back if once you click Ok.",
       function () {
           loadingPopup.Message("Deleting Overrides. Please Wait...");
           $("#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").val(id);
           openWSE.CallDoPostBack("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", "");
       }, null);
}

function EditOverrides(id) {
    overrideEditId = id;
    loadingPopup.Message("Loading Overrides. Please Wait...");
    $("#hf_EditUserAppOverrides").val(id);
    openWSE.CallDoPostBack("hf_EditUserAppOverrides", "");
}

function UpdateOverrides() {
    if (overrideEditId != "") {
        loadingPopup.Message("Saving Overrides. Please Wait...");
        $("#hf_UpdateUserAppOverrides").val(overrideEditId);
        openWSE.CallDoPostBack("hf_UpdateUserAppOverrides", "");
    }
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

function RemoveAllPlugins() {
    loadingPopup.Message("Uninstalling All Plugins...");
    document.getElementById("hf_removeAllPlugins").value = new Date().toString();
    openWSE.CallDoPostBack("hf_removeAllPlugins", "");
}

$(document.body).on("change", "#dd_defaultbodyfontfamily", function (e) {
    UpdateFontFamilyPreview();
});

function UpdateFontFamilyPreview() {
    var x = "<iframe id='iframe_fontfamilypreview' style='width: 100%; height: 40px; border: none;'></iframe>";
    $("#span_fontfamilypreview").html(x);

    var doc = document.getElementById("iframe_fontfamilypreview");
    if (doc) {
        doc = doc.contentWindow.document;
        if (doc) {
            try {
                var siteMainCss = openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + '/StyleSheets/Main/sitemaster.css';

                var cssFile = "";
                if ($("#dd_defaultbodyfontfamily").val() != "inherit") {
                    cssFile = "<link href='" + openWSE.siteRoot() + "CustomFonts/" + $("#dd_defaultbodyfontfamily").val() + "' type='text/css' rel='stylesheet' />";
                }

                doc.open();
                doc.write("<link href='" + siteMainCss + "' type='text/css' rel='stylesheet' />" + cssFile + "<span style='font-size: 15px;'>This is the font preview</span>");
                doc.close();
            }
            catch (evt) { }
        }
    }
}

$(document.body).on("change", "#rb_BoxedLayout_acctOptions, #rb_WideLayout_acctOptions", function () {
    loadingPopup.Message("Updating...");
});

function SiteLayoutOptionUpdated(option) {
    if ($("#rb_BoxedLayout").length > 0 && $("#rb_WideLayout").length > 0) {
        if (option === "Boxed") {
            $("#site_mainbody").attr("data-layoutoption", "Boxed");
            $("#rb_BoxedLayout").prop("checked", true);
            $("#rb_WideLayout").prop("checked", false);
        }
        else {
            $("#site_mainbody").attr("data-layoutoption", "Wide");
            $("#rb_BoxedLayout").prop("checked", false);
            $("#rb_WideLayout").prop("checked", true);
        }
    }
}
