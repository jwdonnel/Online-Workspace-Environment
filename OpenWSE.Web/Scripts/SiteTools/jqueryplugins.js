$(document.body).on("click", ".Button-Action", function () {
    loadingPopup.Message("Updating. Please wait...");
});

$(document.body).on("click", ".Upload-Button-Action", function () {
    loadingPopup.Message("Uploading. Please wait...");
});

window.onload = function () {
    if ($("#pnl_AddControls").length > 0) {
        var editor = initAce();
        editor.getSession().setValue("");
        $("#hf_InitializeCode").val("");
        editor.getSession().on('change', function (e) {
            $("#hf_InitializeCode").val(escape(editor.getSession().getValue()));
        });
    }
};

function SelectAllUpload() {
    var state = $("#a_selectall").attr("data-state");
    var $cb = $("#MainContent_checkbox_FileList");
    if (state == "off") {
        $("#a_selectall").attr("data-state", "on");
        $("#a_selectall").html("Deselect All");
        $cb.find("input[type='checkbox']").each(function () {
            $(this).prop("checked", true);
        });
    }
    else {
        $("#a_selectall").attr("data-state", "off");
        $("#a_selectall").html("Select All");
        $cb.find("input[type='checkbox']").each(function () {
            $(this).prop("checked", false);
        });
    }
}

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

function ViewAssociations(id) {
    loadingPopup.Message("Uploading. Please wait...");
    $("#hf_BuildAssociatedTable").val(id);
    openWSE.CallDoPostBack("hf_BuildAssociatedTable", "");
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

function DeletePlugin(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "? Deleting this will also delete any associated files.",
       function () {
           loadingPopup.Message("Deleting Plugin...");
           $("#hf_deletePlugin").val(id);
           openWSE.CallDoPostBack("hf_deletePlugin", "");
       }, null);
}

function EditPlugin(id) {
    $("#hf_editPlugin").val(id);
    openWSE.CallDoPostBack("hf_editPlugin", "");
}

function UpdateEnabled(id) {
    loadingPopup.Message("Enabling. Please wait...");
    $("#hf_enablePlugin").val(id);
    openWSE.CallDoPostBack("hf_enablePlugin", "");
}

function UpdateDisabled(id) {
    loadingPopup.Message("Disabling. Please wait...");
    $("#hf_disablePlugin").val(id);
    openWSE.CallDoPostBack("hf_disablePlugin", "");
}

function CancelPlugin() {
    loadingPopup.Message("Cancelling. Please wait...");
    $("#hf_cancelPlugin").val(new Date().toString());
    openWSE.CallDoPostBack("hf_cancelPlugin", "");
}

$(document.body).on("change", "#MainContent_dd_aw", function () {
    var val = $("#MainContent_dd_aw").val();
    if (val == "") {
        $("#cb_installAfter").show();
    }
    else {
        $("#cb_installAfter").hide();
    }
});

function UpdatePlugin(id) {
    loadingPopup.Message("Updating. Please wait...");
    $("#hf_updateName").val($("#txt_updateName").val());
    $("#hf_updateLoc").val($("#txt_updateLoc").val());
    $("#hf_updateDesc").val($("#txt_updateDesc").val());
    $("#hf_updateaw").val($("#dd_updateAW").val());

    var editor = initAce_updateEditor();
    $("#hf_updateInitializeCode").val(escape(editor.getSession().getValue()));

    $("#hf_updatePlugin").val(id);
    openWSE.CallDoPostBack("hf_updatePlugin", "");
}

$(document.body).on("change", "#MainContent_fileupload_Plugin", function () {
    var fu = $("#MainContent_fileupload_Plugin").val().toLowerCase();
    if ((fu.indexOf(".js") != -1) || (fu.indexOf(".css") != -1) || (fu.indexOf(".zip") != -1)) {
        $("#lbl_uploadMessage").html("");
    }
    else {
        $("#lbl_uploadMessage").html("<span style='color:Red'>File type not valid.</span>");
    }
});
