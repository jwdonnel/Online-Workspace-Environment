$(window).load(function () {
    LoadSourceCode();
    setTimeout(function () {
        openWSE.RemoveUpdateModal();
        document.title = "File Manager";
    }, 1500);
});

var aceMode = "ace/mode/javascript";

var prm = Sys.WebForms.PageRequestManager.getInstance();

prm.add_endRequest(function () {
    openWSE.RemoveUpdateModal();
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
    document.title = "File Manager";
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
            openWSE.LoadingMessage1('Saving. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
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

$(document.body).on("click", ".tsdivclick", function () {
    $('div#container').block({ message: '<b>Updating Query. Please wait.<b>' });
    var folder = $(this).children('.first-node').text();
    var foldertemp = folder.replace(/ /g, "_");
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        $("#" + hf.value).removeClass("tsactive");
    }
    hf.value = "expand_" + foldertemp;
    $("#expand_" + foldertemp).addClass("tsactive");
    enableCurrSelected();
    document.getElementById("MainContent_hf_folderchange").value = folder;
    document.getElementById("MainContent_hf_category").value = foldertemp;
    __doPostBack("MainContent_hf_folderchange", "");
});

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

function onSearchClearFiles(tbid) {
    $get('' + tbid + '').value = 'Search for documents';
    var hf_refresh = document.getElementById('MainContent_hf_searchFiles');
    var t = document.getElementById('MainContent_tb_search');
    if ((t.value == '') || (t.value == 'Search for documents')) {
        hf_refresh.value = t.value;
        $('div#container').block({ message: '<b>Searching Documents. Please wait.<b>' });
        __doPostBack('MainContent_hf_searchFiles', "");
    }
    return false;
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