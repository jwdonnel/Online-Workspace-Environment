$(document).ready(function () {
    openWSE.RadioButtonStyle();
});

$(document.body).on("click", ".Button-Action", function () {
    openWSE.LoadingMessage1("Updating. Please wait...");
});

$(document.body).on("click", ".Upload-Button-Action", function () {
    openWSE.LoadingMessage1("Uploading. Please wait...");
});

$(window).load(function () {
    if ($("#pnl_AddControls").length > 0) {
        var editor = initAce();
        editor.getSession().setValue("");
        $("#hf_InitializeCode").val("");
        editor.getSession().on('change', function (e) {
            $("#hf_InitializeCode").val(escape(editor.getSession().getValue()));
        });
    }
});

function initAce() {
    ace.config.set("workerPath", "../../Scripts/AceEditor");
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    editor.getSession().setMode('ace/mode/javascript');
    editor.getSession().setUseWrapMode(true);

    return editor;
}

function startAceUpdateEditor() {
    var editor = initAce_updateEditor();
    editor.getSession().setValue(unescape($("#hf_updateInitializeCode").val()));
    editor.getSession().on('change', function (e) {
        $("#hf_updateInitializeCode").val(escape(editor.getSession().getValue()));
    });
}

function initAce_updateEditor() {
    ace.config.set("workerPath", "../../Scripts/AceEditor");
    var editor = ace.edit('updateEditor');
    editor.setTheme('ace/theme/chrome');
    editor.getSession().setMode('ace/mode/javascript');
    editor.getSession().setUseWrapMode(true);

    return editor;
}

function ReInitAce() {
    var editor = initAce();
    editor.getSession().setValue("");
    $("#hf_InitializeCode").val("");
}

$("form").submit(function () {
    if ($("#pnl_AddControls").length > 0) {
        var editor = initAce();
        $("#hf_InitializeCode").val(escape(editor.getSession().getValue()));
    }
});

function MapPathSelect() {
    $('#UploadPlugin').fadeOut(openWSE_Config.animationSpeed, function () {
        $('#AddPlugin').fadeIn(openWSE_Config.animationSpeed);
        $('#aUploadFile').removeClass('selected');
        $('#aMapPath').addClass('selected'); 
    });
}

function UploadFileSelect() {
    $('#AddPlugin').fadeOut(openWSE_Config.animationSpeed, function () {
        $('#UploadPlugin').fadeIn(openWSE_Config.animationSpeed);
        $('#aMapPath').removeClass('selected');
        $('#aUploadFile').addClass('selected'); 
    });
}

function DeletePlugin(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
       function () {
           openWSE.LoadingMessage1("Deleting Plugin...");
           $("#hf_deletePlugin").val(id);
           __doPostBack("hf_deletePlugin", "");
       }, null);
}

function EditPlugin(id) {
    $("#hf_editPlugin").val(id);
    __doPostBack("hf_editPlugin", "");
}

function UpdateEnabled(id) {
    openWSE.LoadingMessage1("Enabling. Please wait...");
    $("#hf_enablePlugin").val(id);
    __doPostBack("hf_enablePlugin", "");
}

function UpdateDisabled(id) {
    openWSE.LoadingMessage1("Disabling. Please wait...");
    $("#hf_disablePlugin").val(id);
    __doPostBack("hf_disablePlugin", "");
}

function CancelPlugin() {
    openWSE.LoadingMessage1("Cancelling. Please wait...");
    $("#hf_cancelPlugin").val(new Date().toString());
    __doPostBack("hf_cancelPlugin", "");
}

$(document.body).on("change", "#MainContent_dd_aw", function () {
    var val = $("#MainContent_dd_aw").val();
    if (val == "") {
        $("#cb_installerAfter_div").show();
    }
    else {
        $("#cb_installerAfter_div").hide();
    }
});

function UpdatePlugin(id) {
    openWSE.LoadingMessage1("Updating. Please wait...");
    $("#hf_updateName").val($("#txt_updateName").val());
    $("#hf_updateLoc").val($("#txt_updateLoc").val());
    $("#hf_updateDesc").val($("#txt_updateDesc").val());
    $("#hf_updateaw").val($("#dd_updateAW").val());

    var editor = initAce_updateEditor();
    $("#hf_updateInitializeCode").val(escape(editor.getSession().getValue()));

    $("#hf_updatePlugin").val(id);
    __doPostBack("hf_updatePlugin", "");
}

$(document.body).on("change", "#MainContent_fileupload_Plugin", function () {
    var fu = $("#MainContent_fileupload_Plugin").val().toLowerCase();
    if ((fu.indexOf(".js") != -1) || (fu.indexOf(".css") != -1)) {
        $("#lbl_uploadMessage").html("");
    }
    else {
        $("#lbl_uploadMessage").html("<span style='color:Red'>File type not valid. Must be a javascript file (.js)</span>");
    }
    setTimeout(function () { $("#lbl_uploadMessage").html(""); }, 5000);
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    openWSE.RadioButtonStyle();
});