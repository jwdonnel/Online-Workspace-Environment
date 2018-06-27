$(document).ready(function () {
    loadingPopup.RemoveMessage();
    UpdateFontFamilyPreview();
    openWSE.InitializeSiteMenuTabs();

    LoadTinyMCEControls_Simple("htmlEditorHeader");
    LoadTinyMCEControls_Simple("htmlEditorFooter");

    var style = window.getComputedStyle($("body")[0]);
    if ($.trim($("#MainContent_tb_defaultfontsize").val()) == "") {
        var fontsize = style.getPropertyValue("font-size");
        if (fontsize) {
            $("#MainContent_tb_defaultfontsize").val(fontsize.replace("px", ""));
        }
    }

    if ($.trim($("#MainContent_tb_defaultfontcolor").val()) == "" || $("#MainContent_tb_defaultfontcolor").hasClass("use-default")) {
        var fontcolor = style.getPropertyValue("color");
        if (fontcolor) {
            $("#MainContent_tb_defaultfontcolor").val(rgbToHex(fontcolor));
            $("#MainContent_tb_defaultfontcolor").removeClass("use-default");
        }
    }
});

function DeleteUserUpdateFlag(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this update flag?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           $("#hf_deleteUserUpdateFlag").val(id);
           openWSE.CallDoPostBack("hf_deleteUserUpdateFlag", "");
           return true;
       }, function () {
           return false;
       });
}

var htmlEditorHeader_holderStr = "";
var htmlEditorFooter_holderStr = "";
var htmlEditorHeader_iframeHeadStr = "";
var htmlEditorHeader_iframeBodyStr = "";
var htmlEditorFooter_iframeHeadStr = "";
var htmlEditorFooter_iframeBodyStr = "";
function LoadTinyMCEControls_Simple(id) {
    if ($("#pnl_mailCustomize").length > 0) {
        if (typeof tinyMCE != "undefined") {
            if (document.getElementById(id) != null) {
                window.tinymce.dom.Event.domLoaded = true;
                tinymce.remove("#" + id);
                tinymce.init({
                    selector: "#" + id,
                    theme: "modern",
                    remove_script_host: false,
                    relative_urls: false,
                    plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen"],
                    toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
                });
            }
        }
        else {
            setTimeout(function () { LoadTinyMCEControls_Simple(id); }, 1000);
        }
    }
}
$(document.body).on("click", "#MainContent_pnl_mailCustomize_tab", function () {
    LoadTinyMCEControls_Simple("htmlEditorHeader");
    LoadTinyMCEControls_Simple("htmlEditorFooter");
});
$(document.body).on("click", "#btn_updateEmailHeader", function () {
    var iframeHeader = document.getElementById("htmlEditorHeader_ifr");
    var iframeHeaderDocument = iframeHeader.contentDocument || iframeHeader.contentWindow.document;
    if (iframeHeaderDocument) {
        var x = $.trim($(iframeHeaderDocument).find("body").html());
        if (x != null && x != "") {
            loadingPopup.Message("Updating. Please Wait...");
            $("#hf_UpdateHeader").val(escape(x));
            openWSE.CallDoPostBack("hf_UpdateHeader", "");
        }
    }
});
$(document.body).on("click", "#btn_updateEmailFooter", function () {
    var iframeFooter = document.getElementById("htmlEditorFooter_ifr");
    var iframeFooterDocument = iframeFooter.contentDocument || iframeFooter.contentWindow.document;
    if (iframeFooterDocument) {
        var x = $.trim($(iframeFooterDocument).find("body").html());
        if (x != null && x != "") {
            loadingPopup.Message("Updating. Please Wait...");
            $("#hf_UpdateFooter").val(escape(x));
            openWSE.CallDoPostBack("hf_UpdateFooter", "");
        }
    }
});
function ClearEmailHeader() {
    var iframeHeader = document.getElementById("htmlEditorHeader_ifr");
    var iframeHeaderDocument = iframeHeader.contentDocument || iframeHeader.contentWindow.document;
    if (iframeHeaderDocument) {
        $(iframeHeaderDocument).find("body").html("");
        loadingPopup.Message("Updating. Please Wait...");
        $("#hf_UpdateHeader").val("CLEARHEADER");
        openWSE.CallDoPostBack("hf_UpdateHeader", "");
    }
}
function ClearEmailFooter() {
    var iframeFooter = document.getElementById("htmlEditorFooter_ifr");
    var iframeFooterDocument = iframeFooter.contentDocument || iframeFooter.contentWindow.document;
    if (iframeFooterDocument) {
        $(iframeFooterDocument).find("body").html("");
        loadingPopup.Message("Updating. Please Wait...");
        $("#hf_UpdateFooter").val("CLEARFOOTER");
        openWSE.CallDoPostBack("hf_UpdateFooter", "");
    }
}
function UnescapeCode(text, id) {
    if ($("#pnl_mailCustomize").length > 0) {
        setTimeout(function () {
            try {
                if ((tinymce != null) && (tinymce != undefined)) {
                    try {
                        tinymce.get(id).setContent(unescape(text));
                    }
                    catch (evt) {
                        UnescapeCode(text, id);
                    }
                }
                else {
                    UnescapeCode(text, id);
                }
            }
            catch (evt2) { }
        }, 100);
    }
}

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

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function () {
    UpdateFontFamilyPreview();

    if ($("#pnl_mailCustomize").length > 0) {
        htmlEditorHeader_holderStr = tinymce.get('htmlEditorHeader').getContent();
        htmlEditorFooter_holderStr = tinymce.get('htmlEditorFooter').getContent();
    }
});
prm.add_endRequest(function () {
    UpdateFontFamilyPreview();

    if ($("#pnl_mailCustomize").length > 0) {
        LoadTinyMCEControls_Simple("htmlEditorHeader");
        LoadTinyMCEControls_Simple("htmlEditorFooter");

        setTimeout(function () {
            tinymce.get('htmlEditorHeader').setContent(htmlEditorHeader_holderStr);
            tinymce.get('htmlEditorFooter').setContent(htmlEditorFooter_holderStr);
        }, 250);
    }
});

var serverTime_interval;
function UpdateCurrentTime(timezone) {
    window.clearInterval(serverTime_interval);
    serverTime_interval = null;

    serverTime_interval = window.setInterval(function () {
        var now = new Date();
        var now_utc = new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds());
        var x = now_utc.addHours(timezone);
        var localDate = x.toLocaleDateString();
        var localTime = x.toLocaleTimeString();
        $("#lbl_currentServerTime").html(localDate + " " + localTime)
    }, 1000)
}
Date.prototype.addHours = function (h) {
    this.setHours(this.getHours() + h);
    return this;
}

function BGDelete() {
    openWSE.ConfirmWindow("Are you sure you want to delete this background?",
       function () {
           loadingPopup.Message("Deleting. Please Wait...");
           return true;
       }, function () {
           return false;
       });
}

var viewAsStringEnabled = false;
function ViewAsString() {
    if (!viewAsStringEnabled) {
        $("#aViewAsString").html("View as List");
        var asString = "";
        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").each(function (index) {
            if (index > 0) {
                asString += "," + $.trim($(this).find(".text").html());
            }
            else {
                asString += $.trim($(this).find(".text").html());
            }
        });



        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").hide();

        if ($("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").length > 0) {
            $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").remove();
        }

        $("#MainContent_pnl_keywordsMetaTag").append("<div class='keyword-split-string-item'></div>");
        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").html(asString);
        $("#btnKeywordsUpdate").hide();
        $("#lbtn_clearAllKeywordsMeta").hide();
        viewAsStringEnabled = true;
    }
    else {
        $("#aViewAsString").html("View as String");
        $("#btnKeywordsUpdate").show();
        $("#lbtn_clearAllKeywordsMeta").show();
        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").remove();
        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").show();
        viewAsStringEnabled = false;
    }
}

$(document.body).on("click", ".TestConnection", function () {
    loadingPopup.Message("Testing Connection...");
});

var keyWordDelete = false;
$(document.body).on("click", "#MainContent_pnl_keywordsMetaTag", function (e) {
    if (!viewAsStringEnabled) {
        if (e.target.class != "keyword-split-array-item" && e.target.class != "text" && e.target.class != "keyword-split-array-input-remove" && !keyWordDelete) {
            if ($(this).find(".keyword-split-array-input").length == 0) {
                $(this).append("<input type='text' class='keyword-split-array-input' />");
            }
            $(".keyword-split-array-input").focus();
        }
        keyWordDelete = false;
    }
});

$(document.body).on("click", ".keyword-split-array-item", function (e) {
    if (e.target.class != "keyword-split-array-input-remove") {
        if ($(this).find(".keyword-split-array-input").length == 0) {
            var keyword = $.trim($(this).find(".text").html());
            var width = $(this).width();
            $(this).removeClass("keyword-split-array-item");
            $(this).addClass("keyword-split-array-item-edit");
            $(this).html("<input type='text' class='keyword-split-array-input' value='" + keyword + "' style='width: " + width + "px;' />");
        }

        $(this).find(".keyword-split-array-input").focus();
    }
});

$(document.body).on("blur", ".keyword-split-array-input", function (e) {
    var keyword = $.trim($(this).val());

    if ($(this).parent().hasClass("keyword-split-array-item-edit") && keyword == "") {
        $(this).parent().remove();
    }

    var keywordSplit = keyword.split(',');
    for (var i = 0; i < keywordSplit.length; i++) {
        var newKeyWord = $.trim(keywordSplit[i]);
        if (newKeyWord != "") {
            if (!$(this).parent().hasClass("keyword-split-array-item-edit")) {
                $("#MainContent_pnl_keywordsMetaTag").append("<div class='keyword-split-array-item'><span class='text'>" + newKeyWord + "</span><span class='keyword-split-array-input-remove' title='Remove'></span></div>");
            }
            else {
                $(this).parent().removeClass("keyword-split-array-item-edit");
                $(this).parent().addClass("keyword-split-array-item");
                $(this).parent().html("<span class='text'>" + newKeyWord + "</span><span class='keyword-split-array-input-remove' title='Remove'></span>");
            }
        }
    }
    $(".keyword-split-array-input").remove();
});

$(document.body).on("keypress", ".keyword-split-array-input", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    if (code == 13) {
        $(".keyword-split-array-input").trigger("blur");
        e.preventDefault();
    }
    else if (code == 44) {
        $(".keyword-split-array-input").trigger("blur");
        $("#MainContent_pnl_keywordsMetaTag").trigger("click");
        e.preventDefault();
    }
});
$(document.body).on("keypress", "#MainContent_tb_totalWorkspacesAllowed, #MainContent_tb_defaultfontsize", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

function UpdateSiteKeywords() {
    var keywords = "";

    if (!viewAsStringEnabled) {
        $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").each(function () {
            if ($(this).find(".text").length != 0) {
                var val = $.trim($(this).find(".text").html());
                if (val != "") {
                    keywords += val + ",";
                }
            }
        });
    }
    else if ($("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").length > 0) {
        keywords = $.trim($("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").html());
    }

    if (keywords == "") {
        keywords = "REMOVEKEYWORDS";
    }

    $("#aViewAsString").html("View as String");
    $("#btnKeywordsUpdate").show();
    $("#lbtn_clearAllKeywordsMeta").show();
    $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-string-item").remove();
    $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").show();
    viewAsStringEnabled = false;

    loadingPopup.Message("Updating Keywords...");
    $("#hf_keywordsMetaTag").val(escape(keywords));
    openWSE.CallDoPostBack("hf_keywordsMetaTag", "");
}

$(document.body).on("click", ".keyword-split-array-input-remove", function () {
    $(this).parent().remove();
    keyWordDelete = true;
});

$(document.body).on("change", "#MainContent_FileUpload2", function () {
    var fu = $("#MainContent_FileUpload2").val().toLowerCase();
    if ((fu.indexOf(".png") != -1) || (fu.indexOf(".jpg") != -1) || (fu.indexOf(".jpeg") != -1) || (fu.indexOf(".gif") != -1)) {
        $("#MainContent_btn_uploadlogo").removeAttr("disabled");
        $("#fu_error_message").html("");
    }
    else {
        $("#MainContent_btn_uploadlogo").attr("disabled", "disabled");
        $("#fu_error_message").html("File type not valid");
    }
});

$(document.body).on("change", "#MainContent_FileUpload4", function () {
    var fu = $("#MainContent_FileUpload4").val().toLowerCase();
    if ((fu.indexOf(".png") != -1) || (fu.indexOf(".jpg") != -1) || (fu.indexOf(".jpeg") != -1) || (fu.indexOf(".gif") != -1) || (fu.indexOf(".ico") != -1)) {
        $("#MainContent_btn_uploadlogo_fav").removeAttr("disabled");
        $("#fu_error_message_2").html("");
    }
    else {
        $("#MainContent_btn_uploadlogo_fav").attr("disabled", "disabled");
        $("#fu_error_message_2").html("File type not valid");
    }
});

$(document.body).on("change", "#MainContent_FileUpload5", function () {
    var fu = $("#MainContent_FileUpload5").val().toLowerCase();
    if ((fu.indexOf(".png") != -1) || (fu.indexOf(".jpg") != -1) || (fu.indexOf(".jpeg") != -1) || (fu.indexOf(".gif") != -1)) {
        $("#MainContent_btn_uploadbgImage").removeAttr("disabled");
        $("#fu_error_message_3").html("");
    }
    else {
        $("#MainContent_btn_uploadbgImage").attr("disabled", "disabled");
        $("#fu_error_message_3").html("File type not valid");
    }
});

$(document.body).on("change", "#MainContent_dd_bgmanage", function () {
    loadingPopup.Message("Updating. Please Wait...");
});

$(window).resize(function () {
    var width = 0;
    var height = 0;
    if ($("#restore_modal").css("display") == "block") {
        width = $('#restore_modal').outerWidth();
    }
    else {
        width = $('#delete_modal').outerWidth();
    }

    if ($("#restore_modal").css("display") == "block") {
        height = $('#restore_modal').outerHeight();
    }
    else {
        height = $('#delete_modal').outerHeight();
    }

    $('.vModal_align').css({
        left: ($(window).width() - width) / 2,
        top: ($(window).height() - height) / 2
    });
});

$(document.body).on("change", "#MainContent_dd_defaultbodyfontfamily", function (e) {
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
                if ($("#MainContent_dd_defaultbodyfontfamily").val()) {
                    cssFile = "<link href='" + openWSE.siteRoot() + "CustomFonts/" + $("#MainContent_dd_defaultbodyfontfamily").val() + "' type='text/css' rel='stylesheet' />";
                }

                doc.open();
                doc.write("<link href='" + siteMainCss + "' type='text/css' rel='stylesheet' />" + cssFile + "<span style='font-size: 15px;'>This is the font preview</span>");
                doc.close();
            }
            catch (evt) { }
        }
    }
}
