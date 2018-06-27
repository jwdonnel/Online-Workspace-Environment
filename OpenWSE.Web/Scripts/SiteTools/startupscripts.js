$(document.body).on("click", "#btn_createnew_startupscript", function () {
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/AddScriptJS", '{ "jsScript": "' + escape($("#tb_createnew_startupscript").val()) + '","applyTo": "' + $("#dd_startupApplyTo").val() + '","sequence": "' + $("#dd_startupsequence").val() + '"  }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateStartupScripts").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
        }
        else if (!openWSE.ConvertBitToBoolean(data.d)) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Script is invalid");
        }
        else if (data.d == "duplicate") {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Duplicate script");
        }
        else {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Error trying to add startup script");
        }
    }, function () {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("There was an error adding script. Please try again.");
    });
});

function DeleteStartupScript(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this startup script?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/DeleteScriptJS", '{ "id": "' + x + '" }', null, function (data) {
               if (openWSE.ConvertBitToBoolean(data.d)) {
                   $("#hf_UpdateStartupScripts").val(new Date().toString());
                   openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
               }
               else {
                   loadingPopup.RemoveMessage();
                   openWSE.AlertWindow("Error deleting startup script");
               }
           }, function () {
               loadingPopup.RemoveMessage();
               openWSE.AlertWindow("There was an error deleting script. Please try again.");
           });
       }, null);
}

function tb_edit_startupscript_KeyPressed(event, x) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13') {
        DoneEditStartupScript(x);
    }
}

function DoneEditStartupScript(x) {
    loadingPopup.Message("Updating. Please Wait...");
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/EditScriptJS", '{ "id": "' + x + '","path": "' + escape($("#tb_edit_startupscript").val()) + '","applyTo": "' + escape($("#ddl-edit-app-js").val()) + '" }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateStartupScripts").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
        }
        else {
            openWSE.AlertWindow("Error updating startup script");
            $("#hf_UpdateStartupScripts").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
        }
    }, function () {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("There was an error editing script. Please try again.");
    });
}

function EditStartupScript(x) {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_EditStartupScripts").val(x);
    openWSE.CallDoPostBack("hf_EditStartupScripts", "");
}

function CancelStartupScript() {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_UpdateStartupScripts").val(new Date().toString());
    openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
}

$(document.body).on("click", "#btn_createnew_startupscript_CSS", function () {
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/AddScriptCSS", '{ "cssScript": "' + escape($("#tb_createnew_startupscript_CSS").val()) + '","applyTo": "' + $("#dd_startupApplyTo_CSS").val() + '","sequence": "' + $("#dd_startupsequence_CSS").val() + '","theme": "' + $("#dd_theme_CSS").val() + '" }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateStartupScripts_css").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
        }
        else if (!openWSE.ConvertBitToBoolean(data.d)) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Script is invalid");
        }
        else if (data.d == "duplicate") {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Duplicate script");
        }
        else {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("Error trying to add startup script");
        }
    }, function () {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("There was an error adding css script. Please try again.");
    });
});

function DeleteStartupScript_CSS(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this startup script?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/DeleteScriptCSS", '{ "id": "' + x + '" }', null, function (data) {
               if (openWSE.ConvertBitToBoolean(data.d)) {
                   $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                   openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
               }
               else {
                   loadingPopup.RemoveMessage();
                   openWSE.AlertWindow("Error deleting startup script");
               }
           }, function () {
               loadingPopup.RemoveMessage();
               openWSE.AlertWindow("There was an error deleting css script. Please try again.");
           });
       }, null);
}

function DoneEditStartupScript_CSS(x) {
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/EditScriptCSS", '{ "id": "' + x + '","path": "' + escape($("#tb_edit_startupscript_CSS").val()) + '","applyTo": "' + escape($("#ddl-edit-app-css").val()) + '","theme": "' + escape($("#ddl-edit-theme-css").val()) + '" }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateStartupScripts_css").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
        }
        else {
            openWSE.AlertWindow("Error updating startup script");
            $("#hf_EditStartupScripts_css").val(new Date().toString());
            openWSE.CallDoPostBack("hf_EditStartupScripts_css", "");
        }
    }, function () {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("There was an error editing css script. Please try again.");
    });
}

function EditStartupScript_CSS(x) {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_EditStartupScripts_css").val(x);
    openWSE.CallDoPostBack("hf_EditStartupScripts_css", "");
}

function CancelStartupScript_CSS() {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_UpdateStartupScripts_css").val(new Date().toString());
    openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
}

var fixHelper = function (e, ui) {
    ui.children().each(function () {
        $(this).width($(this).width());
    });
    return ui;
};

var prelistorder_js = "";
var prelistorder_css = "";
$(document).ready(function () {
    $(function () {
        $("#js_sortable-table tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper,
            containment: '#js_sortable',
            opacity: 0.9,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                openWSE.noAnimationForSortableRows(true);

                prelistorder_js = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_js += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                openWSE.noAnimationForSortableRows(false);

                var listorder = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_js != listorder) {
                    Loading_ss();
                    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/UpdateSeqScriptJS", '{ "sequence": "' + listorder + '" }', null, function (data) {
                        if (openWSE.ConvertBitToBoolean(data.d)) {
                            $("#hf_UpdateStartupScripts").val(new Date().toString());
                            openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
                        }
                        else {
                            loadingPopup.RemoveMessage();
                        }
                    }, function () {
                        loadingPopup.RemoveMessage();
                        openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                    });
                }
            }
        });
        $("#css_sortable-table tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper,
            containment: '#css_sortable',
            opacity: 0.9,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                openWSE.noAnimationForSortableRows(true);

                prelistorder_css = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_css += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                openWSE.noAnimationForSortableRows(false);

                var listorder = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_css != listorder) {
                    Loading_ss();
                    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/UpdateSeqScriptCSS", '{ "sequence": "' + listorder + '" }', null, function (data) {
                        if (openWSE.ConvertBitToBoolean(data.d)) {
                            $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                            openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
                        }
                        else {
                            loadingPopup.RemoveMessage();
                        }
                    }, function () {
                        loadingPopup.RemoveMessage();
                        openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                    });
                }
            }
        });
    });
});

function Loading_ss() {
    loadingPopup.Message("Updating Sequence. Please Wait...");
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    $(function () {
        $("#js_sortable-table tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper,
            containment: '#js_sortable',
            opacity: 0.9,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                openWSE.noAnimationForSortableRows(true);

                prelistorder_js = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_js += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                openWSE.noAnimationForSortableRows(false);

                var listorder = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_js != listorder) {
                    Loading_ss();
                    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/UpdateSeqScriptJS", '{ "sequence": "' + listorder + '" }', null, function (data) {
                        if (openWSE.ConvertBitToBoolean(data.d)) {
                            $("#hf_UpdateStartupScripts").val(new Date().toString());
                            openWSE.CallDoPostBack("hf_UpdateStartupScripts", "");
                        }
                        else {
                            loadingPopup.RemoveMessage();
                        }
                    }, function () {
                        loadingPopup.RemoveMessage();
                        openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                    });
                }
            }
        });
        $("#css_sortable-table tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper,
            containment: '#css_sortable',
            opacity: 0.9,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                openWSE.noAnimationForSortableRows(true);

                prelistorder_css = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_css += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                openWSE.noAnimationForSortableRows(false);

                var listorder = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_css != listorder) {
                    Loading_ss();
                    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/UpdateSeqScriptCSS", '{ "sequence": "' + listorder + '" }', null, function (data) {
                        if (openWSE.ConvertBitToBoolean(data.d)) {
                            $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                            openWSE.CallDoPostBack("hf_UpdateStartupScripts_css", "");
                        }
                        else {
                            loadingPopup.RemoveMessage();
                        }
                    }, function () {
                        loadingPopup.RemoveMessage();
                        openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                    });
                }
            }
        });
    });
});

function ReAssignButtonSelected() {
    if ($("#startupjs").css("display") != "none") {
        $('#MainContent_lbtn_css').removeClass('selected');
        $('#MainContent_lbtn_js').addClass('selected');
    }
    else {
        $('#MainContent_lbtn_js').removeClass('selected');
        $('#MainContent_lbtn_css').addClass('selected');
    }
}
