$(document.body).on("click", "#btn_createnew_startupscript", function () {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/AddScriptJS",
        type: "POST",
        data: '{ "jsScript": "' + escape($("#tb_createnew_startupscript").val()) + '","applyTo": "' + $("#dd_startupApplyTo").val() + '","sequence": "' + $("#dd_startupsequence").val() + '"  }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#hf_UpdateStartupScripts").val(new Date().toString());
                __doPostBack("hf_UpdateStartupScripts", "");
            }
            else if (!openWSE.ConvertBitToBoolean(data.d)) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Script is invalid");
            }
            else if (data.d == "duplicate") {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Duplicate script");
            }
            else {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Error trying to add startup script");
            }
        },
        error: function () {
            openWSE.AlertWindow("There was an error adding script. Please try again.");
        }
    });
});

function DeleteStartupScript(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this startup script?",
       function () {
           openWSE.LoadingMessage1("Deleting. Please Wait...");
           $.ajax({
               url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/DeleteScriptJS",
               type: "POST",
               data: '{ "id": "' + x + '" }',
               contentType: "application/json; charset=utf-8",
               success: function (data) {
                   if (openWSE.ConvertBitToBoolean(data.d)) {
                       $("#hf_UpdateStartupScripts").val(new Date().toString());
                       __doPostBack("hf_UpdateStartupScripts", "");
                   }
                   else {
                       openWSE.RemoveUpdateModal();
                       openWSE.AlertWindow("Error deleting startup script");
                   }
               },
               error: function () {
                   openWSE.AlertWindow("There was an error deleting script. Please try again.");
               }
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
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/EditScriptJS",
        type: "POST",
        data: '{ "id": "' + x + '","path": "' + escape($("#tb_edit_startupscript").val()) + '","applyTo": "' + escape($("#ddl-edit-app-js").val()) + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#hf_UpdateStartupScripts").val(new Date().toString());
                __doPostBack("hf_UpdateStartupScripts", "");
            }
            else {
                openWSE.AlertWindow("Error updating startup script");
                $("#hf_UpdateStartupScripts").val(new Date().toString());
                __doPostBack("hf_UpdateStartupScripts", "");
            }
        },
        error: function () {
            openWSE.AlertWindow("There was an error editing script. Please try again.");
        }
    });
}

function EditStartupScript(x) {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $("#hf_EditStartupScripts").val(x);
    __doPostBack("hf_EditStartupScripts", "");
}

function CancelStartupScript() {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $("#hf_UpdateStartupScripts").val(new Date().toString());
    __doPostBack("hf_UpdateStartupScripts", "");
}

$(document.body).on("click", "#btn_createnew_startupscript_CSS", function () {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/AddScriptCSS",
        type: "POST",
        data: '{ "cssScript": "' + escape($("#tb_createnew_startupscript_CSS").val()) + '","applyTo": "' + $("#dd_startupApplyTo_CSS").val() + '","sequence": "' + $("#dd_startupsequence_CSS").val() + '","theme": "' + $("#dd_theme_CSS").val() + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                __doPostBack("hf_UpdateStartupScripts_css", "");
            }
            else if (!openWSE.ConvertBitToBoolean(data.d)) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Script is invalid");
            }
            else if (data.d == "duplicate") {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Duplicate script");
            }
            else {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("Error trying to add startup script");
            }
        },
        error: function () {
            openWSE.AlertWindow("There was an error adding css script. Please try again.");
        }
    });
});

function DeleteStartupScript_CSS(x) {
    openWSE.ConfirmWindow("Are you sure you want to delete this startup script?",
       function () {
           openWSE.LoadingMessage1("Deleting. Please Wait...");
           $.ajax({
               url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/DeleteScriptCSS",
               type: "POST",
               data: '{ "id": "' + x + '" }',
               contentType: "application/json; charset=utf-8",
               success: function (data) {
                   if (openWSE.ConvertBitToBoolean(data.d)) {
                       $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                       __doPostBack("hf_UpdateStartupScripts_css", "");
                   }
                   else {
                       openWSE.RemoveUpdateModal();
                       openWSE.AlertWindow("Error deleting startup script");
                   }
               },
               error: function () {
                   openWSE.AlertWindow("There was an error deleting css script. Please try again.");
               }
           });
       }, null);
}

function DoneEditStartupScript_CSS(x) {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/EditScriptCSS",
        type: "POST",
        data: '{ "id": "' + x + '","path": "' + escape($("#tb_edit_startupscript_CSS").val()) + '","applyTo": "' + escape($("#ddl-edit-app-css").val()) + '","theme": "' + escape($("#ddl-edit-theme-css").val()) + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                __doPostBack("hf_UpdateStartupScripts_css", "");
            }
            else {
                openWSE.AlertWindow("Error updating startup script");
                $("#hf_EditStartupScripts_css").val(new Date().toString());
                __doPostBack("hf_EditStartupScripts_css", "");
            }
        },
        error: function () {
            openWSE.AlertWindow("There was an error editing css script. Please try again.");
        }
    });
}

function EditStartupScript_CSS(x) {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $("#hf_EditStartupScripts_css").val(x);
    __doPostBack("hf_EditStartupScripts_css", "");
}

function CancelStartupScript_CSS() {
    openWSE.LoadingMessage1("Updating. Please Wait...");
    $("#hf_UpdateStartupScripts_css").val(new Date().toString());
    __doPostBack("hf_UpdateStartupScripts_css", "");
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
    var url = location.href;
    load(url == "" ? "1" : url);

    if ($("#startupjs").css("display") == "none") {
        $("#hdl2").addClass("active");
        $("#hdl1").removeClass("active");
    }
    else {
        $("#hdl2").removeClass("active");
        $("#hdl1").addClass("active");
    }

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
                prelistorder_js = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_js += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                var listorder = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_js != listorder) {
                    Loading_ss();
                    $.ajax({
                        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/UpdateSeqScriptJS",
                        type: "POST",
                        data: '{ "sequence": "' + listorder + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (openWSE.ConvertBitToBoolean(data.d)) {
                                $("#hf_UpdateStartupScripts").val(new Date().toString());
                                __doPostBack("hf_UpdateStartupScripts", "");
                            }
                            else {
                                openWSE.RemoveUpdateModal();
                            }
                        },
                        error: function () {
                            openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                        }
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
                prelistorder_css = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_css += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                var listorder = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_css != listorder) {
                    Loading_ss();
                    $.ajax({
                        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/UpdateSeqScriptCSS",
                        type: "POST",
                        data: '{ "sequence": "' + listorder + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (openWSE.ConvertBitToBoolean(data.d)) {
                                $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                                __doPostBack("hf_UpdateStartupScripts_css", "");
                            }
                            else {
                                openWSE.RemoveUpdateModal();
                            }
                        },
                        error: function () {
                            openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                        }
                    });
                }
            }
        });
    });
});

$(function () {
    $(".sitemenu-selection").find("li").find("a").on("click", function () {
        load($(this).attr("href"));
        return false;
    });
});

function Loading_ss() {
    openWSE.LoadingMessage1("Updating Sequence. Please Wait...");
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    openWSE.RadioButtonStyle();

    setTimeout(function () {
        if ($("#startupjs").css("display") == "none") {
            $("#hdl2").addClass("active");
            $("#hdl1").removeClass("active");
        }
        else {
            $("#hdl2").removeClass("active");
            $("#hdl1").addClass("active");
        }
    }, 1500);

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
                prelistorder_js = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_js += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                var listorder = "";
                $("#js_sortable .sorted-js").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_js != listorder) {
                    Loading_ss();
                    $.ajax({
                        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/UpdateSeqScriptJS",
                        type: "POST",
                        data: '{ "sequence": "' + listorder + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (openWSE.ConvertBitToBoolean(data.d)) {
                                $("#hf_UpdateStartupScripts").val(new Date().toString());
                                __doPostBack("hf_UpdateStartupScripts", "");
                            }
                            else {
                                openWSE.RemoveUpdateModal();
                            }
                        },
                        error: function () {
                            openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                        }
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
                prelistorder_css = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        prelistorder_css += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                var listorder = "";
                $("#css_sortable .sorted-css").each(function () {
                    var temp = $(this).find("span").text();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_css != listorder) {
                    Loading_ss();
                    $.ajax({
                        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/UpdateSeqScriptCSS",
                        type: "POST",
                        data: '{ "sequence": "' + listorder + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (openWSE.ConvertBitToBoolean(data.d)) {
                                $("#hf_UpdateStartupScripts_css").val(new Date().toString());
                                __doPostBack("hf_UpdateStartupScripts_css", "");
                            }
                            else {
                                openWSE.RemoveUpdateModal();
                            }
                        },
                        error: function () {
                            openWSE.AlertWindow("There was an error updating sequence for script. Please try again.");
                        }
                    });
                }
            }
        });
    });
});


function load(num) {
    $(".RadioButton-Toggle-Overlay").remove();
    openWSE.RadioButtonStyle();

    if (window.location.href.indexOf("?css_view=true") > 0) {
        try
        {
            var loc = window.location.href.split("?css_view=true");
            window.location.href = loc[0] + "#?a=stylesheets";
        }
        catch (evt) { }
    }

    arg1 = num.split("?tab=");
    if (arg1.length > 1) {
        arg2 = arg1[1].split("#");
        if (arg2.length == 1) {
            arg2 = arg2[0].split("&");
            if (arg2[0] == "javascripts") {
                $('#hdl2').removeClass('active');
                $('#hdl1').addClass('active');
                $('#startupcss').hide();
                $('#startupjs').fadeIn(openWSE_Config.animationSpeed);
                document.title = "Startup Scripts: Javascripts";
            }
            else if (arg2[0] == "stylesheets") {
                $('#hdl1').removeClass('active');
                $('#hdl2').addClass('active');
                $('#startupjs').hide();
                $('#startupcss').fadeIn(openWSE_Config.animationSpeed);
                document.title = "Startup Scripts: Style Sheets";
            }
            else {
                $('#hdl2').removeClass('active');
                $('#hdl1').addClass('active');
                $('#startupcss').hide();
                $('#startupjs').fadeIn(openWSE_Config.animationSpeed);
                document.title = "Network Activity: Statistics";
            }
        }
    }
    else {
        $('#hdl2').removeClass('active');
        $('#hdl1').addClass('active');
        $('#startupcss').hide();
        $('#startupjs').show(openWSE_Config.animationSpeed);
        document.title = "Startup Scripts: Javascripts";
    }
}