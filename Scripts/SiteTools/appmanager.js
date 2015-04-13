var _editmode = 0; // 0 = no edit, 1 = edit, 2 = reset
var canLoadEditorText = 0; // 0 = false, 1 = true
var dbType = "";

$(window).load(function () {
    LoadSourceCode();
    try {
        LoadCategoryCookies(true);
    }
    catch (evt) { }

    $(".sitemenu-selection").find("li").on("click", function () {
        openWSE.LoadingMessage1("Loading...");
    });
});

function ConfirmLoaderFileCancel(_this) {
    openWSE.ConfirmWindow("Cancelling will delete this app. Are you sure you want to continue?",
        function () {
            openWSE.LoadingMessage1('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
        }, null);

    return false;
}

function PerformAppCleanUp() {
    var x = "<div id='ConfirmCleanup-element' class='Modal-element' style='display: none;'>";
    x += "<div class='Modal-overlay'>";
    x += "<div class='Modal-element-align'>";
    x += "<div class='Modal-element-modal' style='width: 400px; min-width: 400px;'>";
    x += "<div class='ModalHeader'><div><span class='Modal-title'></span></div></div>";

    x += "<div align='center' class='pad-all'>Do you want to delete any files found as well? Doing so will permanently delete these app files.";
    x += "<div class='clear-space'></div><div class='clear-space-five'></div>";
    x += "<input type='button' class='input-buttons' value='Yes' onclick=\"StartAppCleanUp('true');return false;\" style='width: 65px;' />";
    x += "<input type='button' class='input-buttons' value='No' onclick=\"StartAppCleanUp('false'); return false;\" style='width: 65px; margin-left: 10px!important;' />";
    x += "<input type='button' class='input-buttons no-margin' value='Cancel' onclick=\"StartAppCleanUp('cancel'); return false;\" style='width: 65px; margin-left: 10px!important;' />";
    x += "<div class='clear-space'></div>";
    x += "</div></div></div></div></div></div>";

    $("body").append(x);
    openWSE.LoadModalWindow(true, "ConfirmCleanup-element", "Perform App Clean Up?");
}

function StartAppCleanUp(deleteFiles) {
    if (deleteFiles != "cancel") {
        openWSE.LoadingMessage1("Cleaning up...");
        $("#hf_performCleanup").val(deleteFiles);
        __doPostBack("hf_performCleanup", "");
    }
    else {
        $("#ConfirmCleanup-element").remove();
    }
}

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
    openWSE.LoadingMessage1("Changing Page Size...");
});

$(document.body).on("change", "#MainContent_dd_maxonload_create", function () {
    var $resize = $("#MainContent_dd_allowresize_create");
    var $maximize = $("#MainContent_dd_allowmax_create");
    if (openWSE.ConvertBitToBoolean($(this).val())) {
        $resize.val("0");
        $maximize.val("0");
        $resize.attr("disabled", "disabled");
        $maximize.attr("disabled", "disabled");
    }
    else {
        $resize.val("1");
        $maximize.val("1");
        $resize.removeAttr("disabled");
        $maximize.removeAttr("disabled");
    }
});

$(document.body).on("change", "#MainContent_dd_maxonload_edit", function () {
    var $resize = $("#MainContent_dd_allowresize_edit");
    var $maximize = $("#MainContent_dd_allowmax_edit");
    if (openWSE.ConvertBitToBoolean($(this).val())) {
        $resize.val("0");
        $maximize.val("0");
        $resize.attr("disabled", "disabled");
        $maximize.attr("disabled", "disabled");
    }
    else {
        $resize.val("1");
        $maximize.val("1");
        $resize.removeAttr("disabled");
        $maximize.removeAttr("disabled");
    }
});

$(document.body).on("change", "#MainContent_fu_uploadnew", function () {
    var fu = $("#MainContent_fu_uploadnew").val().toLowerCase();
    if (fu.indexOf(".zip") != -1) {
        $("#zipfileLoadname").fadeIn(openWSE_Config.animationSpeed);
        canContinue = true;
    }
    else if ((fu.indexOf(".html") != -1) || (fu.indexOf(".htm") != -1) || (fu.indexOf(".txt") != -1) || (fu.indexOf(".aspx") != -1) || (fu.indexOf(".ascx") != -1) || (fu.indexOf(".pdf") != -1) || (fu.indexOf(".doc") != -1) || (fu.indexOf(".xsl") != -1)) {
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

function UnescapeCode(text) {
    setTimeout(function () {
        if ((tinyMCE != null) && (tinyMCE != undefined)) {
            try {
                if (canLoadEditorText == 1) {
                    tinyMCE.activeEditor.setContent(unescape(text));
                    openWSE.RemoveUpdateModal();
                }
                else {
                    UnescapeCode(text);
                }
            }
            catch (evt) {
                UnescapeCode(text);
            }
        }
        else {
            UnescapeCode(text);
        }
    }, 50);
}

function UnescapeJavascriptCode(text) {
    var path = "../../Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    if ($('#HTMLCODE').css("display") == "none") {
        editor.getSession().setMode('ace/mode/html');
    }
    else {
        editor.getSession().setMode('ace/mode/javascript');
    }
    editor.getSession().setUseWrapMode(false);
    editor.getSession().setValue(unescape(text))
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
    var id = cookie.get('app_category_id');
    var category = cookie.get('app_category');
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
    cookie.set("app_category", category, "30");
    cookie.set("app_category_id", id, "30");
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
            cookie.set("app_category", "", "30");
            cookie.set("app_category_id", "", "30");
        }
        else {
            $("." + category).hide("slide", { direction: "right" }, openWSE_Config.animationSpeed, function () {
                $(".app-icon-category-list").show();
                $("#Category-Back-Name").html("");
                $("#Category-Back-Name-id").html("");
                cookie.set("app_category", "", "30");
                cookie.set("app_category_id", "", "30");
            });
        }
    }
    else {
        $(".app-icon-category-list").show();
        $("#Category-Back-Name").html("");
        $("#Category-Back-Name-id").html("");
        cookie.set("app_category", "", "30");
        cookie.set("app_category_id", "", "30");
    }
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    GetAppsInCategory();
    canLoadEditorText = 0;
    $("#app-editor-holder").show();
    LoadTinyMCEControls_Full("htmlEditor");
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
});

var params = "";
var iframeDownloadurl = "";
var wlmd_holder = "";
prm.add_beginRequest(function (sender, args) {
    try {
        if (tinyMCE.activeEditor.getContent() != null) {
            $("#hidden_temp_html").val(escape(tinyMCE.activeEditor.getContent()));
        }
    }
    catch (evt1) { }

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
        document.title = "App Editor";
    }, 1000);

    $(document).tooltip({ disabled: true });
    $("#a_html_create").css("font-weight", "bold");
    $("#a_html_create").css("text-decoration", "underline");

    try {
        var path = "../../Scripts/AceEditor";
        ace.config.set("workerPath", path);
        var editor = ace.edit('editor');
        editor.setTheme('ace/theme/chrome');
        if ($('#HTMLCODE').css("display") == "none") {
            editor.getSession().setMode('ace/mode/html');
        }
        else {
            editor.getSession().setMode('ace/mode/javascript');
        }
        editor.getSession().setUseWrapMode(false);

        if (_editmode == 0) {
            var temp = $("#hidden_temp_script").val();
            editor.getSession().setValue(unescape(temp));
            var temp2 = $("#hidden_temp_html").val();
            tinyMCE.activeEditor.setContent(unescape(temp2));
        }
        else if (_editmode == 2) {
            editor.getSession().setValue("$(document).ready(function () {  });");
        }

        editor.getSession().on('change', function (e) {
            var x = editor.getSession().getValue();
            x = x.replace(/\+/g, "%2B");
            $("#hidden_temp_script").val(escape(x));
        });
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
    if ($('#HTMLCODE').css("display") == "none") {
        editor.getSession().setMode('ace/mode/html');
    }
    else {
        editor.getSession().setMode('ace/mode/javascript');
    }
    editor.getSession().setUseWrapMode(false);
    if (_editmode == 1) {
        var x = editor.getSession().getValue();
        x = x.replace(/\+/g, "%2B");
        $("#hidden_editor").val(escape(x));
    }
    else {
        temp = temp.replace(/\+/g, "%2B");
        $("#hidden_editor").val(escape(temp));
    }
}

$(document.body).on("click", ".rbbuttons, .app-category-div", function () {
    if ((document.getElementById("MainContent_Edit_Controls") == null) || ($("#MainContent_Edit_Controls").css("display") == "none")) {
        openWSE.LoadingMessage1("Loading. Please Wait...");
    }
});

$(document.body).on("click", "#MainContent_btn_create_easy", function () {
    if (($("#MainContent_tb_filename_create").val() != "") && ($("#MainContent_tb_appname").val() != "")) {
        $("#lbl_ErrorUpload").html("");
        openWSE.LoadingMessage1("Creating App. Please Wait...");
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
        openWSE.LoadingMessage1("Creating App. Please Wait...");
        return true;
    }
    else {
        $("#lbl_ErrorUpload").html("<span style='color:#FF0000'>Error! Must have a filename and app name defined.</span>");
        setTimeout(function () { $("#lbl_ErrorUpload").html(""); }, 3000);
        return false;
    }
}

function CreateApp_Click() {
    if (PostErrorMessage()) {
        ReLoadAceEditor();
        if (tinyMCE.activeEditor.getContent() != null) {
            var text = tinyMCE.activeEditor.getContent();
            if (text == "") {
                text = new Date().toString();
            }
            document.getElementById("hf_createapp").value = escape(text);
            __doPostBack("hf_createapp", "");
        }
    }
}

function SaveApp_Click() {
    openWSE.ConfirmWindow("Are you sure you want to save this file? Any changes will overwrite the original.",
      function () {
          ReLoadAceEditor();
          if (tinyMCE.activeEditor.getContent() != null) {
              var text = tinyMCE.activeEditor.getContent();
              if (text == "") {
                  text = new Date().toString();
              }
              openWSE.LoadingMessage1("Saving App...");
              document.getElementById("hf_saveapp").value = escape(text);
              document.getElementById("hidden_temp_script").value = "";
              __doPostBack("hf_saveapp", "");
          }
      }, null);
}

$(document.body).on("change", "#dd_filename_ext", function () {
    var path = "../../Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    if ($(this).val() == ".html") {
        $("#span-autocreate,#dd_autocreate_create,#ascxNote").fadeOut(openWSE_Config.animationSpeed);
        $('#HTMLCODE').show();
        $("#MainContent_cb_wrapIntoIFrame").show();
        $("label[for='MainContent_cb_wrapIntoIFrame']").show();
        $('#javascriptcode_Title').html('Javascript File Editor');
        editor.getSession().setMode('ace/mode/javascript');
        editor.getSession().setValue("$(document).ready(function () {  });");
    }
    else {
        $("#span-autocreate,#dd_autocreate_create,#ascxNote").fadeIn(openWSE_Config.animationSpeed);
        $('#HTMLCODE').hide();
        $("#MainContent_cb_wrapIntoIFrame").hide();
        $("label[for='MainContent_cb_wrapIntoIFrame']").hide();
        $('#javascriptcode_Title').html('Source File Editor');
        editor.getSession().setMode('ace/mode/html');
        editor.getSession().setValue("");
    }
});


/*TinyMCE*/
function LoadTinyMCEControls_Simple(id) {
    if (document.getElementById("pnl_htmleditor") != null) {
        window.tinymce.dom.Event.domLoaded = true;
        var ed = new tinymce.Editor(id, {
            selector: "#" + id,
            theme: "modern",
            height: 400,
            plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste moxiemanager", "autoresize"],
            autoresize_min_height: 400,
            toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
        }, tinymce.EditorManager);
        ed.render();

        canLoadEditorText = 1;
    }
}

function LoadTinyMCEControls_Full(id) {
    if (document.getElementById("pnl_htmleditor") != null) {
        window.tinymce.dom.Event.domLoaded = true;
        // Creates a new editor instance
        var ed = new tinymce.Editor(id, {
            selector: "#" + id,
            theme: "modern",
            height: 400,
            plugins: ["advlist autolink lists link image charmap print preview hr anchor pagebreak", "searchreplace wordcount visualblocks visualchars code fullscreen", "insertdatetime media nonbreaking save table contextmenu directionality", "emoticons template paste", "autoresize"],
            toolbar1: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image",
            toolbar2: "print preview media | forecolor backcolor emoticons",
            autoresize_min_height: 400,
            templates: [{ title: 'Test template 1', content: 'Test 1' }, { title: 'Test template 2', content: 'Test 2'}]
        }, tinymce.EditorManager);
        ed.render();
        canLoadEditorText = 1;
    }
}


/*App Paramaters*/
function EditParameter(id) {
    openWSE.LoadingMessage1("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_edit').value = id;
    __doPostBack('hf_appchange_params_edit', "");
}

function DeleteParameter(id) {
    openWSE.LoadingMessage1("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_delete').value = id;
    __doPostBack('hf_appchange_params_delete', "");
}

$(document.body).on("keypress", "#txt_appparam_edit", function (event) {
    if (event.which == 13) {
        UpdateParameter();
        event.preventDefault();
    }
});

function UpdateParameter() {
    openWSE.LoadingMessage1("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_update').value = $("#txt_appparam_edit").val();
    __doPostBack('hf_appchange_params_update', "");
}

function CancelParameterEdit() {
    openWSE.LoadingMessage1("Updating. Please Wait...");

    document.getElementById('hf_appchange_params_cancel').value = new Date().toString();
    __doPostBack('hf_appchange_params_cancel', "");
}


/* --------------------------- */
/* Password Protected Requests */
/* --------------------------- */
function Confirm_Delete() {
    openWSE.RemoveUpdateModal();
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
              openWSE.RemoveUpdateModal();
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
        openWSE.LoadingMessage1("Deleting App. Please Wait...");
    }, 100);
    $("#hf_appdeleteid").val(deleteId);
    $("#hf_StartDelete").val(new Date().toString());
    __doPostBack("hf_StartDelete", "");
}

$(document.body).on("change", "#ddl_categories", function () {
    GetAppsInCategory();
});

function GetAppsInCategory() {
    if ($("#pnl_AppList").length > 0) {
        var selected = $("#ddl_categories").val();
        $("#noItemsCategory").hide();

        $(".app-item-installer").each(function (index) {
            $(this).find(".GridViewNumRow").html(index + 1);
        });

        if (selected == "") {
            $(".app-item-installer").show();
        }
        else {
            var found = 0;
            $(".app-item-installer").each(function () {
                var categoryId = $(this).attr("data-category").split(';');
                if (categoryId.indexOf(selected) == -1) {
                    $(this).hide();
                }
                else {
                    $(this).show();
                    found++;
                    $(this).find(".GridViewNumRow").html(found);
                }
            });

            if (found == 0) {
                $("#noItemsCategory").show();
            }
        }
    }
}