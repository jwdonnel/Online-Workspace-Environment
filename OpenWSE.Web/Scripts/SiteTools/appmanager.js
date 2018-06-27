var _editmode = 0; // 0 = no edit, 1 = edit, 2 = reset
var canLoadEditorText = 0; // 0 = false, 1 = true
var dbType = "";
var aceMode = "ace/mode/php";

window.onload = function () {
    LoadSourceCode();
    try {
        LoadCategoryCookies(true);
    }
    catch (evt) { }
};

$(document).ready(function () {
    $(window).resize();
});

$(window).resize(function () {
    try {
        var h = $(window).height();
        $("#editor").css("height", h - 410);
    }
    catch (evt) { }
});

function ConfirmLoaderFileCancel(_this) {
    openWSE.ConfirmWindow("Cancelling will delete this app. Are you sure you want to continue?",
        function () {
            loadingPopup.Message('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

function UnescapeEditValues() {
    var unescapedVal = "";
    if ($("#txt_appparam_edit").length > 0) {
        unescapedVal = unescape($("#txt_appparam_edit").attr("data-value")).replace(/&nbsp;/g, " ");
        $("#txt_appparam_edit").val(unescapedVal);
    }

    if ($("#txt_appparamdesc_edit").length > 0) {
        unescapedVal = unescape($("#txt_appparamdesc_edit").attr("data-value")).replace(/&nbsp;/g, " ");
        $("#txt_appparamdesc_edit").val(unescapedVal);
    }
}

$(document.body).on("change", "#MainContent_dd_enablebg_create", function () {
    if ($(this).val() == "app-main") {
        $("#backgroundcolorholder_create").show();
    }
    else {
        $("#backgroundcolorholder_create").hide();
    }
});

$(document.body).on("change", "#MainContent_dd_enablebg_edit", function () {
    if ($(this).val() == "app-main") {
        $("#backgroundcolorholder_edit").show();
    }
    else {
        $("#backgroundcolorholder_edit").hide();
    }
});

$(document.body).on("change", "#MainContent_dd_allowpopout_create", function () {
    if ($(this).val() == "1") {
        $("#popoutlocdiv").show();
        if ($.trim($("#MainContent_tb_popoutLoc_create").val()) == "") {
            $("#MainContent_tb_popoutLoc_create").val("~/ExternalAppHolder.aspx?appId=app-" + $.trim($("#MainContent_tb_filename_create").val()));
        }
    }
    else {
        $("#popoutlocdiv").hide();
    }
});

$(document.body).on("change", "#MainContent_tb_backgroundColor_edit", function () {
    if ($("#MainContent_cb_backgroundColor_edit_default").prop("checked")) {
        $("#MainContent_cb_backgroundColor_edit_default").trigger("click");
    }
});
$(document.body).on("change", "#MainContent_tb_iconColor_edit", function () {
    if ($("#MainContent_cb_iconColor_edit_default").prop("checked")) {
        $("#MainContent_cb_iconColor_edit_default").trigger("click");
    }
});
$(document.body).on("change", "#MainContent_tb_iconColor_create", function () {
    if ($("#MainContent_cb_iconColor_create_default").prop("checked")) {
        $("#MainContent_cb_iconColor_create_default").trigger("click");
    }
});
$(document.body).on("change", "#MainContent_tb_backgroundColor_create", function () {
    if ($("#MainContent_cb_backgroundColor_create_default").prop("checked")) {
        $("#MainContent_cb_backgroundColor_create_default").trigger("click");
    }
});

function LoadDefaultPageSelector() {
    openWSE.LoadModalWindow(true, "LoaderApp-element", "App Loader File");
}

function LoadNewDefaultPageSelector() {
    openWSE.LoadModalWindow(true, "LoaderAppNew-element", "App Load File")
    $(window).resize();
}

function ValidateForm() {
    if ($("#newupload").css("display") != "none") {
        var fu = $("#MainContent_fu_uploadnew").val().toLowerCase();
        if ((canContinue) && ($("#MainContent_tb_appname").val() != "")) {
            if (fu.indexOf(".zip") != -1) {
                if ($("#MainContent_tb_loadFilename").val() != "") {
                    return true;
                }
            }
            else {
                return true;
            }
        }
    }
    else {
        return true;
    }
    return false;
}

$(document.body).on("change", "#MainContent_cb_InstallAfterLoad", function () {
    if ($(this).is(":checked")) {
        $("#div_isPrivate").show();
    }
    else {
        $("#div_isPrivate").hide();
    }
});

$(document.body).on("change", "#MainContent_dd_allowpopout_edit", function () {
    var $allowPopout = $("#MainContent_dd_allowpopout_edit");
    if (openWSE.ConvertBitToBoolean($allowPopout.val())) {
        $("#MainContent_tb_allowpopout_edit").val("~/ExternalAppHolder.aspx?appId=" + $("#MainContent_hf_appchange").val());
    }
    else {
        $("#MainContent_tb_allowpopout_edit").val("");
    }
});

$(document.body).on("change", "#MainContent_ddl_pageSize", function () {
    loadingPopup.Message("Changing Page Size...");
});

$(document.body).on("change", "#MainContent_dd_maxonload_create", function () {
    var $resize = $("#MainContent_dd_allowresize_create");
    var $maximize = $("#MainContent_dd_allowmax_create");
    var $minWidth = $("#MainContent_tb_minwidth_create");
    var $minHeight = $("#MainContent_tb_minheight_create");
    if (openWSE.ConvertBitToBoolean($(this).val())) {
        $resize.val("0");
        $maximize.val("0");
        $resize.attr("disabled", "disabled");
        $maximize.attr("disabled", "disabled");
        $minWidth.attr("disabled", "disabled");
        $minHeight.attr("disabled", "disabled");

        if ($.trim($minWidth.val()) == "") {
            $minWidth.val("500");
        }

        if ($.trim($minHeight.val()) == "") {
            $minHeight.val("500");
        }
    }
    else {
        $resize.val("1");
        $maximize.val("1");
        $resize.removeAttr("disabled");
        $maximize.removeAttr("disabled");
        $minWidth.removeAttr("disabled");
        $minHeight.removeAttr("disabled");
    }
});

$(document.body).on("change", "#MainContent_dd_maxonload_edit", function () {
    var $resize = $("#MainContent_dd_allowresize_edit");
    var $maximize = $("#MainContent_dd_allowmax_edit");
    var $minWidth = $("#MainContent_tb_minwidth_edit");
    var $minHeight = $("#MainContent_tb_minheight_edit");
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

$(document.body).on("change", "#MainContent_fu_uploadnew", function () {
    var fu = $("#MainContent_fu_uploadnew").val().toLowerCase();
    if (fu.indexOf(".zip") != -1) {
        $("#zipfileLoadname").fadeIn(openWSE_Config.animationSpeed);
        canContinue = true;
    }
    else if ((fu.indexOf(".html") != -1) || (fu.indexOf(".htm") != -1) || (fu.indexOf(".txt") != -1) || (fu.indexOf(".aspx") != -1) || (fu.indexOf(".ascx") != -1) || (fu.indexOf(".pdf") != -1) || (fu.indexOf(".doc") != -1) || (fu.indexOf(".xsl") != -1) || (fu.indexOf(".dll") != -1)) {
        $("#zipfileLoadname").fadeOut(openWSE_Config.animationSpeed);
        canContinue = true;
    }
    else {
        canContinue = false;
    }
});

$(document.body).on("change", "#MainContent_fu_image_edit", function () {
    var fu = $("#MainContent_fu_image_edit").val().toLowerCase();
    if ((fu.indexOf(".png") != -1) || (fu.indexOf(".jpg") != -1) || (fu.indexOf(".jpeg") != -1)) {
        $("#MainContent_btn_save_2").show();
        $("#MainContent_btn_save").hide();
    }
    else {
        $("#MainContent_btn_save_2").hide();
        $("#MainContent_btn_save").show();
    }
});

$(document.body).on("change", "#ddl_categories", function () {
    loadingPopup.Message("Updating...");
});

function ChangeIconUploadType(x) {
    if (x == 0) {
        $("#urlIcon-tab").hide();
        $("#uploadIcon-tab").show();
        $("#uploadIcon").hide();
        $("#urlIcon").show();
    }
    else {
        $("#uploadIcon-tab").hide();
        $("#urlIcon-tab").show();
        $("#urlIcon").hide();
        $("#uploadIcon").show();
    }
}

function ChangeIconUploadTypeEdit(x) {
    if (x == 0) {
        $("#urlIcon-tab-edit").hide();
        $("#uploadIcon-tab-edit").show();
        $("#uploadIcon-edit").hide();
        $("#urlIcon-edit").show();
    }
    else {
        $("#uploadIcon-tab-edit").hide();
        $("#urlIcon-tab-edit").show();
        $("#urlIcon-edit").hide();
        $("#uploadIcon-edit").show();
    }
}

function UnescapeJavascriptCode(text) {
    var path = "../../Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    editor.getSession().setMode(aceMode);
    editor.getSession().setUseWrapMode(false);
    editor.setShowPrintMargin(false);
    editor.getSession().setValue(unescape(text));
    $(window).resize();
}

function ViewCode() {
    if ($("#pnl_app_information").css("display") != "none") {
        $("#pnl_app_information").fadeOut(openWSE_Config.animationSpeed, function () {
            $("#pnl_htmleditor").fadeIn(openWSE_Config.animationSpeed);
        });
    }
    else {
        $("#pnl_htmleditor").fadeOut(openWSE_Config.animationSpeed, function () {
            $("#pnl_app_information").fadeIn(openWSE_Config.animationSpeed);
        });
    }
}

function LoadCategoryCookies(fadein) {
    cookieFunctions.get('app_category_id', function (id) {
        cookieFunctions.get('app_category', function (category) {
            if ((document.getElementById("Category-Back") != null) && (id != null) && (id != "") && (category != null) && (category != "")) {
                var c = getParameterByName("c");
                if (c != "params") {
                    $(".app-icon-category-list").hide();
                    $("#Category-Back").show();
                    $("." + id).show();
                    $("#Category-Back-Name").html(category);
                    $("#Category-Back-Name-id").html(id);
                }
            }
            if (fadein) {
                $("#app-editor-holder").fadeIn(openWSE_Config.animationSpeed);
            }
            else {
                $("#app-editor-holder").show();
            }
        });
    });
}

function CategoryClick(id, category) {
    $(".app-icon-category-list").hide();
    $("#Category-Back").fadeIn(openWSE_Config.animationSpeed);
    if (openWSE_Config.animationSpeed == 0) {
        $("." + id).show();
    }
    else {
        $("." + id).show("slide", { direction: "right" }, openWSE_Config.animationSpeed);
    }
    $("#Category-Back-Name").html(category);
    $("#Category-Back-Name-id").html(id);
    cookieFunctions.set("app_category", category, "30");
    cookieFunctions.set("app_category_id", id, "30");
}

$(document.body).on("click", "#Category-Back", function () {
    var category = $("#Category-Back-Name-id").html();
    $("#Category-Back").fadeOut(openWSE_Config.animationSpeed);
    if (category != "" && $("." + category).length > 0) {
        if (openWSE_Config.animationSpeed == 0) {
            $("." + category).hide();
            $(".app-icon-category-list").show();
            $("#Category-Back-Name").html("");
            $("#Category-Back-Name-id").html("");
            cookieFunctions.set("app_category", "", "30");
            cookieFunctions.set("app_category_id", "", "30");
        }
        else {
            $("." + category).hide("slide", { direction: "right" }, openWSE_Config.animationSpeed, function () {
                $(".app-icon-category-list").show();
                $("#Category-Back-Name").html("");
                $("#Category-Back-Name-id").html("");
                cookieFunctions.set("app_category", "", "30");
                cookieFunctions.set("app_category_id", "", "30");
            });
        }
    }
    else {
        $(".app-icon-category-list").show();
        $("#Category-Back-Name").html("");
        $("#Category-Back-Name-id").html("");
        cookieFunctions.set("app_category", "", "30");
        cookieFunctions.set("app_category_id", "", "30");
    }
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    canLoadEditorText = 0;
    $("#app-editor-holder").show();
    ReapplyViewMode();
    LoadSourceCode();
    try {
        LoadCategoryCookies(false);
    }
    catch (evt) { }

    if ((wlmd_holder != "") && ($("#App-element").css("display") == "block")) {
        $("#MainContent_wlmd_holder").html(unescape(wlmd_holder));
        if (iframeDownloadurl != "") {
            $("#iframe-appDownloader").attr("src", iframeDownloadurl);
            iframeDownloadurl = "";
        }
        wlmd_holder = "";
    }

    if (dbType == "delete") {
        Confirm_Delete();
    }

    dbType = "";

    if (params != "") {
        $("#MainContent_pnl_app_params_holder").html(params);
        params = "";
    }

    if ($("#MainContent_dd_enablebg_edit").val() == "app-main") {
        $("#backgroundcolorholder_edit").show();
    }
    else {
        $("#backgroundcolorholder_edit").hide();
    }
});

var params = "";
var iframeDownloadurl = "";
var wlmd_holder = "";
prm.add_beginRequest(function (sender, args) {
    try {
        var elem = args.get_postBackElement();
        if (elem != null) {
            if (elem.id == "hf_UpdateAll") {
                if ($("#App-element").css("display") == "block") {
                    iframeDownloadurl = $("#iframe-appDownloader").attr("src");
                }

                if ($("#MainContent_pnl_app_params_holder").length > 0) {
                    params = $.trim($("#MainContent_pnl_app_params_holder").html());
                }

                if ($("#MainContent_wlmd_holder").length > 0) {
                    wlmd_holder = $.trim($("#MainContent_wlmd_holder").html());
                }
            }
            else if ((elem.id == "MainContent_btn_delete") || (elem.id == "MainContent_btn_passwordConfirm") || (elem.id == "hf_StartDelete") || (elem.id == "hf_UpdateNotificationsInd") || (elem.id == "lb_clearNoti")) {
                if (($("#MainContent_wlmd_holder").html() != "") && ($("#App-element").css("display") == "block")) {
                    wlmd_holder = escape($("#MainContent_wlmd_holder").html());
                    iframeDownloadurl = $("#iframe-appDownloader").attr("src");
                }

                if ($("#MainContent_pnl_app_params_holder").length > 0) {
                    params = $.trim($("#MainContent_pnl_app_params_holder").html());
                }

                if ($("#MainContent_wlmd_holder").length > 0) {
                    wlmd_holder = $.trim($("#MainContent_wlmd_holder").html());
                }
            }
        }
    }
    catch (evt2) { }
});

function ReapplyViewMode() {
    if ((_editmode == 0) || (_editmode == 2)) {
        if ($("#pnl_app_information").css("display") == "none") {
            $("#pnl_app_information").css("display", "none");
            $("#pnl_htmleditor").css("display", "block");
        }
        else {
            $("#pnl_app_information").css("display", "block");
            $("#pnl_htmleditor").css("display", "none");
        }
    }
}

function RevertToProperties() {
    $("#pnl_htmleditor").fadeOut(openWSE_Config.animationSpeed, function () {
        $("#pnl_app_information").fadeIn(openWSE_Config.animationSpeed);
    });
}

function LoadSourceCode() {
    setTimeout(function () {
        var pageTitle = "App Manager";
        if (openWSE_Config.siteName) {
            pageTitle += " / " + openWSE_Config.siteName;
        }
        document.title = pageTitle;
    }, 1000);

    $(document).tooltip({ disabled: true });
    $("#a_html_create").css("font-weight", "bold");
    $("#a_html_create").css("text-decoration", "underline");

    try {
        var path = "../../Scripts/AceEditor";
        ace.config.set("workerPath", path);
        var editor = ace.edit('editor');
        editor.setTheme('ace/theme/chrome');
        editor.getSession().setMode(aceMode);
        editor.getSession().setUseWrapMode(false);
        editor.setShowPrintMargin(false);

        if (_editmode == 0) {
            var temp = $("#hidden_temp_script").val();
            editor.getSession().setValue(unescape(temp));
        }

        editor.getSession().on('change', function (e) {
            var x = editor.getSession().getValue();
            x = x.replace(/\+/g, "%2B");
            $("#hidden_temp_script").val(escape(x));
        });

        $(window).resize();
    }
    catch (evt) { }
}

$("form").submit(function () {
    ReLoadAceEditor();
});

function ReLoadAceEditor() {
    var temp = $("#hidden_temp_script").val();
    var path = "../../Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    editor.getSession().setMode(aceMode);
    editor.getSession().setUseWrapMode(false);
    editor.setShowPrintMargin(false);
    if (_editmode == 1) {
        var x = editor.getSession().getValue();
        x = x.replace(/\+/g, "%2B");
        $("#hidden_editor").val(escape(x));
    }
    else {
        temp = temp.replace(/\+/g, "%2B");
        $("#hidden_editor").val(escape(temp));
    }

    $(window).resize();
}

$(document.body).on("click", ".rbbuttons, .app-category-div", function () {
    if ((document.getElementById("MainContent_Edit_Controls") == null) || ($("#MainContent_Edit_Controls").css("display") == "none")) {
        loadingPopup.Message("Loading. Please Wait...");
    }
});

$(document.body).on("click", "#MainContent_btn_create_easy", function () {
    if (($("#MainContent_tb_filename_create").val() != "") && ($("#MainContent_tb_appname").val() != "")) {
        $("#lbl_ErrorUpload").html("");
        loadingPopup.Message("Creating App. Please Wait...");
    }
    else {
        $("#lbl_ErrorUpload").html("<span style='color:#FF0000'>Error! Must have a filename and app name defined.</span>");
        setTimeout(function () { $("#lbl_ErrorUpload").html(""); }, 3000);
        return false;
    }
});

$(document.body).on("click", "#MainContent_btn_uploadnew", function () {
    PostErrorMessage();
});

function PostErrorMessage() {
    if (($("#MainContent_tb_filename_create").val() != "") && ($("#MainContent_tb_appname").val() != "") && (ValidateForm())) {
        $("#lbl_ErrorUpload").html("");
        loadingPopup.Message("Creating App. Please Wait...");
        return true;
    }
    else {
        $("#lbl_ErrorUpload").html("<span style='color:#FF0000'>Error! Must have a filename and app name defined.</span>");
        setTimeout(function () { $("#lbl_ErrorUpload").html(""); }, 3000);
        return false;
    }
}

function SaveApp_Click() {
    openWSE.ConfirmWindow("Are you sure you want to save this file? Any changes will overwrite the original.",
      function () {
          ReLoadAceEditor();
          loadingPopup.Message("Saving App...");
          $("#hf_saveapp").val(new Date().toString());
          $("#hidden_temp_script").val("");
          openWSE.CallDoPostBack("hf_saveapp", "");
      }, null);
}

/*App Paramaters*/
function EditParameter(id) {
    loadingPopup.Message("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_edit').value = id;
    openWSE.CallDoPostBack('hf_appchange_params_edit', "");
}

function DeleteParameter(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this parameter?",
      function () {
          loadingPopup.Message("Updating. Please Wait...");

          document.getElementById('hf_appchange_params_delete').value = id;
          openWSE.CallDoPostBack('hf_appchange_params_delete', "");
      }, null);
}

$(document.body).on("keypress", "#txt_appparam_edit, #txt_appparamdesc_edit", function (event) {
    if (event.which == 13) {
        UpdateParameter();
        event.preventDefault();
    }
});

function UpdateParameter() {
    loadingPopup.Message("Updating. Please Wait...");

    document.getElementById('hf_appchange_paramsdesc_update').value = escape($.trim($("#txt_appparamdesc_edit").val()));
    document.getElementById('hf_appchange_params_update').value = escape($.trim($("#txt_appparam_edit").val()));

    openWSE.CallDoPostBack('hf_appchange_params_update', "");
}

function CancelParameterEdit() {
    loadingPopup.Message("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_cancel').value = new Date().toString();
    openWSE.CallDoPostBack('hf_appchange_params_cancel', "");
}

function AddAppParameter() {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_app_params").val(escape($.trim($("#txt_app_params").val())));
    $("#hf_app_params_description").val(escape($.trim($("#txt_app_params_description").val())));
    $("#hf_btnapp_addParms").val(new Date().toString());
    openWSE.CallDoPostBack("hf_btnapp_addParms", "");
}


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function Confirm_Delete() {
    loadingPopup.RemoveMessage();
    $("#hf_appdeleteid").val(deleteId);

    if (UserIsSocialAccount != null && UserIsSocialAccount) {
        $("#MainContent_btn_passwordConfirm").trigger("click");
    }
    else {
        openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
        $("#MainContent_tb_passwordConfirm").focus();
    }
}

var deleteId = "";
function OnDelete(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this app? App will have to be reapplied to all users if re-installed.",
      function () {
          dbType = "delete";
          deleteId = id;
          Confirm_Delete();
          return true;
      }, function () {
          window.setTimeout(function () {
              loadingPopup.RemoveMessage();
          }, 500);
          return false;
      });
}

function CancelRequest() {
    openWSE.LoadModalWindow(false, "password-element", "");
    dbType = "";
    deleteId = "";
    $("#MainContent_tb_passwordConfirm").val("");
}

function BeginWork() {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        openWSE.LoadModalWindow(false, "password-element", "");
        loadingPopup.Message("Deleting App. Please Wait...");
    }, 100);
    $("#hf_appdeleteid").val(deleteId);
    $("#hf_StartDelete").val(new Date().toString());
    openWSE.CallDoPostBack("hf_StartDelete", "");
}

var canContinue = false;
var tempId = "";
function appchange(id) {
    loadingPopup.Message("Loading...");
    var inner = $.trim($("#MainContent_wlmd_holder").html());
    if ((inner != "") && ($("#App-element").css("display") == "block")) {
        setTimeout(function () {
            loadingPopup.RemoveMessage();
        }, 500);
        return false;
    }
    if ($("#MainContent_tb_title_edit").val() == "") {
        if (id == "reset") {
            id = tempId;
        }

        if (document.getElementById("MainContent_hf_appchange").value != id) {
            if (document.getElementById("hf_isParams").value == "0") {
                document.getElementById("MainContent_wlmd_holder").innerHTML = "";
                document.getElementById("MainContent_hf_appchange").value = id;
                openWSE.CallDoPostBack("MainContent_hf_appchange", "");
            }
            else {
                if (document.getElementById("MainContent_hf_appchange_params").value != id) {
                    document.getElementById("MainContent_hf_appchange_params").value = id;
                    openWSE.CallDoPostBack("MainContent_hf_appchange_params", "");
                }
                else {
                    setTimeout(function () { loadingPopup.RemoveMessage(); }, 500);
                }
            }
        }
        else {
            tempId = id;
            document.getElementById("MainContent_hf_appchange").value = "reset";
            openWSE.CallDoPostBack("MainContent_hf_appchange", "");
        }
    }
}
