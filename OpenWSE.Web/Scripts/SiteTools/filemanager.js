window.onload = function () {
    LoadSourceCode();
    setTimeout(function () {
        loadingPopup.RemoveMessage();
        var pageTitle = "File Manager";
        if (openWSE_Config.siteName) {
            pageTitle += " / " + openWSE_Config.siteName;
        }
        document.title = pageTitle;
    }, 1500);
};

var aceMode = "ace/mode/javascript";

var prm = Sys.WebForms.PageRequestManager.getInstance();

prm.add_endRequest(function () {
    loadingPopup.RemoveMessage();
    try {
        var hf_category = document.getElementById('MainContent_hf_category');
        if (hf_category.value == "") {
            $get('mf_View_All_Documents').style.fontWeight = "bold";
        }
        else {
            $get('mf_' + hf_category.value).style.fontWeight = "bold";
        }
        document.getElementById("hf_editing").value = "false";
        reloadCurr();
        LoadSourceCode();
    }
    catch (evt) { }
    
    var pageTitle = "File Manager";
    if (openWSE_Config.siteName) {
        pageTitle += " / " + openWSE_Config.siteName;
    }
    document.title = pageTitle;
});

function LoadSourceCode() {
    setTimeout(function () {
        $("#div_source_create").css("display", "none");
    }, 500);
    LoadEditor();
}

function ConfirmSaveFile(_this) {
    openWSE.ConfirmWindow("Are you sure you want to save this file? Any changes will overwrite the original.",
        function () {
            loadingPopup.Message('Saving. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

$(document.body).on("click", "#MainContent_lbtn_save", function () {
    if ($("#editor").css("display") == "block") {
        var editor = ace.edit('editor');
        editor.setTheme('ace/theme/chrome');
        editor.getSession().setMode(aceMode);
        editor.getSession().setUseWrapMode(true);
        editor.setShowPrintMargin(false);
        var x = editor.getSession().getValue();
        x = x.replace(/\+/g, "%2B");
        $("#hidden_editor").val(escape(x));
    }
});

function LoadEditor() {
    if ($("#editor").css("display") == "block") {
        try {
            var path = "../../Scripts/AceEditor";
            ace.config.set("workerPath", path);
            var editor = ace.edit('editor');
            editor.setTheme('ace/theme/chrome');
            editor.getSession().setMode(aceMode);
            editor.getSession().setUseWrapMode(false);
            editor.setShowPrintMargin(false);

            editor.getSession().on('change', function (e) {
                $("#hidden_temp_script").val(escape(editor.getSession().getValue()));
            });
        }
        catch (evt) { }
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
    editor.getSession().setValue(unescape(text))
}

function UnescapeJavascriptCodeReadOnly(text) {
    var path = "../../Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    editor.setReadOnly(true);
    editor.getSession().setMode(aceMode);
    editor.getSession().setUseWrapMode(false);
    editor.setShowPrintMargin(false);
    editor.getSession().setValue(unescape(text))
}

function reloadCurr() {
    var hf_folder = document.getElementById("hf_currexp");
    if ((hf_folder.value != null) || (hf_folder.value != "")) {
        var foldertemp = hf_folder.value;
        $("#" + foldertemp).addClass("tsactive");
        enableCurrSelected();
    }
}

function enableCurrSelected() {
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = '';
            document.getElementById("e_" + temp).style.display = '';
        }
        catch (evt) { }
    }
}

function disableCurrSelected() {
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = 'none';
            document.getElementById("e_" + temp).style.display = 'none';
        }
        catch (evt) { }
    }
}

function showdelete(xdelete, xedit) {
    if ((xdelete.substring(0, 3) != 'x_-') && (!openWSE.ConvertBitToBoolean(document.getElementById("hf_editing").value))) {
        var temp = xedit.replace("e_", "");
        if ($("#expand_" + temp).hasClass("tsactive") != true) {
            document.getElementById(xdelete).style.display = 'block';
            document.getElementById(xedit).style.display = 'block';
        }
    }
}

function hidedelete(xdelete, xedit) {
    if ((xdelete.substring(0, 3) != 'x_-') && (!openWSE.ConvertBitToBoolean(document.getElementById("hf_editing").value))) {
        var temp = xedit.replace("e_", "");
        if ($("#expand_" + temp).hasClass("tsactive") != true) {
            document.getElementById(xdelete).style.display = 'none';
            document.getElementById(xedit).style.display = 'none';
        }
    }
}

$(document).ready(function () {
    $(document).tooltip({ disabled: true });
    $(window).resize();
});

$(window).resize(function () {
    try {
        var h = $(window).height();
        $("#editor, #ace_scroller").css("height", h - 335);
        $("#editor, #ace_scroller").css("width", $("#MainContent_pnl2").outerWith());
    }
    catch (evt) { }
});

$(document.body).on("change", "#dd_viewtype", function () {
    loadingPopup.Message("Updating. Please Wait...");
});
