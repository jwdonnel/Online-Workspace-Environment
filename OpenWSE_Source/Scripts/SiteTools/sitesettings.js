$(document).ready(function () {
    openWSE.RadioButtonStyle();
    openWSE.RemoveUpdateModal();
    BuildLinks();

    var url = location.href;
    load(url == "" ? "1" : url);
});

var currentTab = "";
function BuildLinks() {
    $(".pnl-section").each(function (index) {
        var id = $(this).attr("id").replace("MainContent_pnl_", "");
        $(".sitemenu-selection").append("<li><a href='#?tab=" + id + "'>" + $(this).attr("data-title") + "</a></li>");
    });

    $(".sitemenu-selection").find("li").find("a").on("click", function () {
        load($(this).attr("href"));
        return false;
    });
}

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    openWSE.RadioButtonStyle();
    load(currentTab);
});

function BGDelete() {
    openWSE.ConfirmWindow("Are you sure you want to delete this background?",
       function () {
           openWSE.LoadingMessage1("Deleting. Please Wait...");
           return true;
       }, function () {
           return false;
       });
}

$(document.body).on("click", ".TestConnection", function () {
    openWSE.LoadingMessage1("Testing Connection...");
});

var keyWordDelete = false;
$(document.body).on("click", "#MainContent_pnl_keywordsMetaTag", function (e) {
    if (e.target.class != "keyword-split-array-item" && e.target.class != "text" && e.target.class != "keyword-split-array-input-remove" && !keyWordDelete) {
        if ($(this).find(".keyword-split-array-input").length == 0) {
            $(this).append("<input type='text' class='keyword-split-array-input' />");
        }
        $(".keyword-split-array-input").focus();
    }
    keyWordDelete = false;
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
$(document.body).on("keypress", "#MainContent_tb_totalWorkspacesAllowed", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

function UpdateSiteKeywords() {
    var keywords = "";

    $("#MainContent_pnl_keywordsMetaTag").find(".keyword-split-array-item").each(function () {
        if ($(this).find(".text").length != 0) {
            var val = $.trim($(this).find(".text").html());
            if (val != "") {
                keywords += val + ",";
            }
        }
    });

    if (keywords == "") {
        keywords = "REMOVEKEYWORDS";
    }

    openWSE.LoadingMessage1("Updating Keywords...");
    $("#hf_keywordsMetaTag").val(escape(keywords));
    __doPostBack("hf_keywordsMetaTag", "");
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

$(document.body).on("change", "#MainContent_FileUpload6", function () {
    var fu = $("#MainContent_FileUpload6").val().toLowerCase();
    if (fu.indexOf(".gif") != -1) {
        $("#MainContent_btn_loading_lrg").removeAttr("disabled");
        $("#fu_error_message_6").html("");
    }
    else {
        $("#MainContent_btn_loading_lrg").attr("disabled", "disabled");
        $("#fu_error_message_6").html("File type not valid");
    }
});

$(document.body).on("change", "#MainContent_FileUpload7", function () {
    var fu = $("#MainContent_FileUpload7").val().toLowerCase();
    if (fu.indexOf(".gif") != -1) {
        $("#MainContent_btn_loading_sml").removeAttr("disabled");
        $("#fu_error_message_7").html("");
    }
    else {
        $("#MainContent_btn_loading_sml").attr("disabled", "disabled");
        $("#fu_error_message_7").html("File type not valid");
    }
});

$(document.body).on("change", "#MainContent_dd_bgmanage", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
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

function load(num) {
    $(".pnl-section").hide();
    $(".sitemenu-selection").find("li").removeClass("active");

    var index = 0;
    currentTab = num;

    var arg1 = num.split("tab=");
    if (arg1.length > 1) {
        var arg2 = arg1[1].split("#");
        if (arg2.length == 1) {
            index = GetPnlSectionIndex(arg2[0]);
        }
    }

    $(".pnl-section").eq(index).show();
    $(".sitemenu-selection").find("li").eq(index).addClass("active");
}

function GetPnlSectionIndex(ele) {
    var pnlIndex = 0;
    $(".pnl-section").each(function (index) {
        if ($(this).attr("id") == "MainContent_pnl_" + ele) {
            pnlIndex = index;
        }
    });

    return pnlIndex;
}