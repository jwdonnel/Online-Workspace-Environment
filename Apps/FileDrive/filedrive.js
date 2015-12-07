function DeleteConfirmation() {
    openWSE.LoadingMessage1("Deleting. Please Wait...");
}

function FileDriveMenuClick() {
    var $menu = $("#filedrive-sidebar-menu");
    if ($menu.length > 0) {
        if (!$menu.hasClass("showmenu")) {
            $menu.show();
            $menu.addClass("showmenu");
            $menu.find(".customtable-sidebar-innercontent").show();
            $menu.css("width", $("#mydocuments-load").outerWidth() - 50);
        }
        else {
            $menu.removeClass("showmenu");
            $menu.find(".customtable-sidebar-innercontent").hide();
            $menu.css("width", "0px");
            $menu.hide();
        }
    }
}

$(document.body).on("change", "#dd_display_documents", function () {
    openWSE.RemoveUpdateModal();
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("change", "#dd_groups", function () {
    document.getElementById("hf_currexp").value = "expand_View_All_Documents";
});

$(document.body).on("click", ".ajaxCall_Modal_documents", function () {
    openWSE.LoadModalWindow(true, "FileUpload-element", "File Upload");
    FileDriveMenuClick();
    return false;
});

$(document.body).on("click", ".searchbox_submit", function () {
    if ($.trim($("#tb_search_documents").val()) != "") {
        openWSE.LoadingMessage1("Searching. Please Wait...");
    }
});

$(document.body).on("click", "#btn_groups", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("click", ".td-sort-click", function () {
    var $this = $(this).find("a");
    if ($this.length > 0) {
        openWSE.LoadingMessage1("Sorting. Please Wait...");
        var href = $this.attr("href");
        href = href.replace("javascript:", "");
        href = href.replace("__doPostBack(", "");
        href = href.replace(")", "");
        href = href.replace(/'/g, "");
        var arr = href.split(",");
        __doPostBack(arr[0], arr[1]);
    }
});

var curr_moveFolder = "";
var curr_myAlbums = "";

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function (sender, args) {
    if ($("#dd_moveFolder_documents").length > 0) {
        curr_moveFolder = $("#dd_moveFolder_documents").val();
    }

    if ($("#dd_myAlbums_documents").length > 0) {
        curr_myAlbums = $("#dd_myAlbums_documents").val();
    }
});

prm.add_endRequest(function (sender, args) {
    FDPostBackCall();

    if ($("#dd_moveFolder_documents").length > 0 && curr_moveFolder != "") {
        $("#dd_moveFolder_documents").val(curr_moveFolder);
    }

    if ($("#dd_myAlbums_documents").length > 0 && curr_moveFolder != "") {
        $("#dd_myAlbums_documents").val(curr_myAlbums);
    }

    ReloadEqualizer();
});

function FDPostBackCall() {
    try {
        var hf_category_documents = document.getElementById('hf_category_documents_documents');
        if (hf_category_documents.value == "") {
            $get('mf_View_All_Documents').style.fontWeight = "bold";
        } else {
            $get('mf_' + hf_category_documents.value).style.fontWeight = "bold";
        }

        document.getElementById("hf_editing").value = "false";
    }
    catch (evt) {
        openWSE.RemoveUpdateModal();
    }

    reloadCurr();

    $("#tb_search_documents").autocomplete({
        minLength: 1,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetFiles",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        };
                    }));
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    ReloadPlay();
}

function resizeContent() {
    $(".content-overflow-app").css("height", $(window).height() - ($(".app-title-bg-color").outerHeight() + 25));
}

$(window).resize(function () {
    resizeContent();
});

$(document).ready(function () {
    FDPostBackCall();
    equalizerDiv = "<div class='equalizer-img' style='background-image: url(\"" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/equalizer.gif\");'></div>";

    $(".content-overflow-app").scroll(function () {
        if ($.trim($("#audioPlayer").html()) != "") {
            if ($(".content-overflow-app").scrollTop() == 0) {
                $("#audioPlayer").css({
                    position: "",
                    top: "",
                    bottom: ""
                });

                $("#audioPlayer").removeClass("header-shadow");
            }
            else {
                $("#audioPlayer").css({
                    position: "relative",
                    top: $(".content-overflow-app").scrollTop(),
                    bottom: "auto"
                });

                $("#audioPlayer").addClass("header-shadow");
            }
        }
    });

    resizeContent();
});


$(document.body).on("click", ".RandomActionBtns-docs", function () {
    openWSE.LoadingMessage1("Loading...");
});

$(document.body).on("click", "#imgbtn_search_documents", function () {
    var hf_refresh = document.getElementById('hf_searchFiles_documents');
    var t = document.getElementById('tb_search_documents');
    if ((hf_refresh.value != t.value) && (t.value != '') && (t.value != 'Search for documents')) {
        hf_refresh.value = t.value;
        __doPostBack('hf_searchFiles_documents', "");
    }
    return false;
});

$(document.body).on("click", "#btn_refreshdatabase_documents", function () {
    $.ajax({
        url: "../../Apps/FileDrive/FileDriveService.asmx/RefreshFolder",
        type: "POST",
        data: '{ "group": "' + document.getElementById('hf_ddgroups').value + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            document.getElementById('hf_searchFiles_documents').value = "refreshfiles";
            __doPostBack('hf_searchFiles_documents', "");
        }
    });
});

function reloadCurr() {
    var hf_folder = document.getElementById("hf_currexp");
    if (hf_folder != null) {
        if ((hf_folder.value != null) || (hf_folder.value != "")) {
            var foldertemp = hf_folder.value;
            $("#" + foldertemp).addClass("tsactive");
            enableCurrSelected();
        }
    }
}

$(document.body).on("click", ".tsdivclick", function () {
    var folderName = $(this).children('.first-node').text();
    var folderId = $(this).children('.second-node').text();
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        $("#" + hf.value).removeClass("tsactive");
    }
    hf.value = "expand_" + folderId;
    $("#expand_" + folderId).addClass("tsactive");
    enableCurrSelected();
    document.getElementById("hf_folderchange_documents").value = folderName;
    document.getElementById("hf_category_documents").value = folderId;
    __doPostBack("hf_folderchange_documents", "");
});

function enableCurrSelected() {
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = '';
            document.getElementById("e_" + temp).style.display = '';
        } catch (evt) {
        }
    }
}

function disableCurrSelected() {
    var hf = document.getElementById("hf_currexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = 'none';
            document.getElementById("e_" + temp).style.display = 'none';
        } catch (evt) {
        }
    }
}

function onSearchClearFiles(tbid) {
    $get(tbid).value = 'Search Files';
    var hf_refresh = document.getElementById('hf_searchFiles_documents');
    var t = document.getElementById(tbid);
    if ((t.value == '') || (t.value == 'Search Files')) {
        openWSE.LoadingMessage1("Searching. Please Wait...");
        hf_refresh.value = t.value;
        __doPostBack('hf_searchFiles_documents', "");
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

function deletefolder(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete this folder? Any files in this folder will be moved to the Root Directory.",
    function () {
        openWSE.LoadingMessage1("Deleting...");
        $.ajax({
            url: "../../Apps/FileDrive/FileDriveService.asmx/DeleteFolder",
            type: "POST",
            data: '{ "folderId": "' + _this.getAttribute("href") + '","group": "' + document.getElementById('hf_ddgroups').value + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                document.getElementById('hf_searchFiles_documents').value = "deletefolder";
                __doPostBack('hf_searchFiles_documents', "");
            }
        });
    }, null);
}

function editfolder(_this, folder) {
    document.getElementById("hf_editing").value = "true";
    var currEdit = $(_this).attr('id');
    document.getElementById(currEdit).style.display = 'none';
    var element = $(_this).closest('div');
    element.append("<br /><input id='tb_editfolderentry' maxlength='15' type='text' value='" + folder.replace(/_/g, " ") + "' class='textEntry margin-right-sml' style='width: 105px;' />");
    element.append("<small><a href='#updatefolder' onclick=\"updatefolder('" + folder + "');return false;\" class='RandomActionBtns-docs margin-left margin-right'>Update</a></small>");
    element.append("<small><a href='#canceledit' onclick='canceleditfolder();return false;' class='RandomActionBtns-docs'>Cancel</a></small>");
    window.setTimeout(function () { openWSE.RemoveUpdateModal(); }, 500);
}

function canceleditfolder() {
    document.getElementById("hf_editing").value = "false";
    document.getElementById('hf_searchFiles_documents').value = "cancel";
    __doPostBack('hf_searchFiles_documents', "");
}

function updatefolder(folder) {
    $.ajax({
        url: "../../Apps/FileDrive/FileDriveService.asmx/UpdateFolder",
        type: "POST",
        data: '{ "oldname": "' + folder + '","newname": "' + document.getElementById('tb_editfolderentry').value + '","group": "' + document.getElementById('hf_ddgroups').value + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                document.getElementById("hf_editing").value = "false";
                document.getElementById('hf_searchFiles_documents').value = "update";
                __doPostBack('hf_searchFiles_documents', "");
            }
            else {
                document.getElementById('hf_searchFiles_documents').value = "";
                openWSE.AlertWindow("Error Updating Folder! Check folder name then continue.");
                openWSE.RemoveUpdateModal();
            }
        }
    });
}

$(document.body).on("keydown", "#tb_newfolderentry", function () {
    try {
        if (event.which == 13) {
            createnewfolder();
            return false;
        }
    } catch (evt) {
        if (event.keyCode == 13) {
            createnewfolder();
            return false;
        }
        delete evt;
    }
});

var currPlaying_ID = "";
var equalizerDiv = "";
function PlaySong(_this, tagType, mediaType, path, height) {
    var canContinue = false;
    $_playButton = $(_this);
    var src = $_playButton.attr("src");

    if (src.indexOf("play.png") != -1) {
        src = src.replace("play.png", "stop.png");
        canContinue = true;

        try {
            if (currPlaying_ID != "") {
                var $_currPlaying = $("#" + currPlaying_ID);
                if (($_currPlaying != null) && ($_currPlaying != "") && ($_currPlaying != undefined)) {
                    var srcPrev = $_currPlaying.attr("src");
                    srcPrev = srcPrev.replace("stop.png", "play.png");
                    $_currPlaying.attr("src", srcPrev);
                    $(".equalizer-img").remove();
                    $(".equalizer-holder").hide();
                    $("#audioPlayer").html("");
                    $("#audioPlayer").css("min-height", "0px");
                    currPlaying_ID = "";
                }
            }
        }
        catch (evt) { }
    }
    else {
        src = src.replace("stop.png", "play.png");
        $(".equalizer-img").remove();
        $(".equalizer-holder").hide();
        $("#audioPlayer").html("");
        $("#audioPlayer").css("min-height", "0px");
        currPlaying_ID = "";
    }

    $_playButton.attr("src", src);

    if (canContinue) {
        $_playButton.parent().parent().find(".equalizer-holder").append(equalizerDiv);
        $_playButton.parent().parent().find(".equalizer-holder").show();
        currPlaying_ID = $_playButton.attr("id");

        var iframe = document.createElement("iframe");
        iframe.setAttribute("id", "audioPlayer_view");
        iframe.setAttribute("width", "100%");
        iframe.setAttribute("height", height + "px");
        iframe.setAttribute("frameborder", "0");
        iframe.setAttribute("scrolling", "no");

        $("#audioPlayer").html(iframe);
        $("#audioPlayer").css("min-height", height + "px");

        var x = "<script src='audiojs/audiojs/audio.min.js'></script>";
        x += "<script type='text/javascript' src='//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js'></script>";
        x += "<script>audiojs.events.ready(function () {audiojs.createAll();});setTimeout(function(){$('#audioPlayer').trigger(\"play\");$('.audiojs').addClass('playing');},250);</script>";
        x += "<style>.audiojs{width:100%!important;position:relative!important}.audiojs .time{float:right!important;border-left:none!important}";
        x += ".audiojs .scrubber{position:absolute!important;left:0!important;right:115px!important;width:auto!important}</style>";
        x += "<" + tagType + " id='audioPlayer' preload='auto' src='" + path + "' type='" + mediaType + "' style='width:100%'></" + tagType + ">";

        var doc = iframe.document;
        if (iframe.contentDocument) {
            doc = iframe.contentDocument; // For NS6
        } else if (iframe.contentWindow) {
            doc = iframe.contentWindow.document; // For IE5.5 and IE6
        }

        if (doc) {
            doc.open();
            doc.writeln(x);
            doc.close();

            var checked = "";
            if ($("#continue_play_hidden").val() == "true") {
                checked = " checked='checked'";
            }

            var checked2 = "";
            if ($("#shuffle_play_hidden").val() == "true") {
                checked2 = " checked='checked'";
            }

            var checked3 = "";
            if ($("#repeat_play_hidden").val() == "true") {
                checked3 = " checked='checked'";
            }

            var prevImg = "<a href='#prev' title='Previous Song' class='prev-btn margin-right' onclick=\"PrevSong('" + path + "');return false;\"><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/prev.png' class='cursor-pointer'></a>";
            var playImg = "<a href='#play' title='Play' class='play-btn margin-right' onclick=\"PlayCurrSong('" + path + "');return false;\" style='display: none;'><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/play.png' class='cursor-pointer'></a>";
            var pauseImg = "<a href='#pause' title='Pause' class='pause-btn margin-right' onclick=\"PauseCurrSong('" + path + "');return false;\"><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/pause.png' class='cursor-pointer'></a>";
            var nextImg = "<a href='#next' title='Next Song' class='next-btn' onclick=\"NextSong('" + path + "');return false;\"><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/next.png' class='cursor-pointer'></a>";

            var audioControls = "<div class='clear'></div><table cellpadding='5' cellspacing='5' width='100%'><tr>";
            audioControls += "<td width='43%' align='left' style='font-size: 17px;'>" + $_playButton.parent().find(".audio-file").html() + "</td>";
            audioControls += "<td width='57%' align='right'><div class='pad-left-big float-left'>" + prevImg + playImg + pauseImg + nextImg + "</div>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_cont_play' onchange='ContinuousPlay_Changed();' value='false'" + checked + " /><label for='cb_cont_play'>&nbsp;Continuous</label></span>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_shuffle_play' onchange='ShufflePlay_Changed();' value='false'" + checked2 + " /><label for='cb_shuffle_play'>&nbsp;Shuffle</label></span>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_repeat_play' onchange='RepeatPlay_Changed();' value='false'" + checked3 + " /><label for='cb_repeat_play'>&nbsp;Repeat</label></span></td>";
            audioControls += "</tr></table><div class='clear-space-five'></div><div class='border-bottom'></div>";
            $("#audioPlayer").append(audioControls);

            $(".content-overflow-app").scroll();
        }
    }
}

function PlayCurrSong(path) {
    $(".play-btn").hide();
    $(".pause-btn").show();

    $(".equalizer-img").show();
    $(".equalizer-holder").show();
    $(".audio").each(function () {
        if ($(this).prev().attr("data-src") == path) {
            var src = $(this).attr("src");
            src = src.replace("play.png", "stop.png");
            $(this).attr("src", src);
        }
    });
    $("#audioPlayer_view").contents().find("#audioPlayer").trigger("play");
}

function PauseCurrSong(path) {
    $(".pause-btn").hide();
    $(".play-btn").show();

    $(".equalizer-img").hide();
    $(".equalizer-holder").hide();
    $(".audio").each(function () {
        if ($(this).prev().attr("data-src") == path) {
            var src = $(this).attr("src");
            src = src.replace("stop.png", "play.png");
            $(this).attr("src", src);
        }
    });
    $("#audioPlayer_view").contents().find("#audioPlayer").trigger("pause");
}

function NextSong(mp3) {
    var foundCurrent = false;
    var nextIndex = 0;
    if ($("#shuffle_play_hidden").val() == "false") {
        for (var i = 0; i < $("#GV_Files_documents").find(".audio-file").length; i++) {
            if (foundCurrent) {
                nextIndex = i;
                break;
            }
            else if ($("#GV_Files_documents").find(".audio-file").eq(i).attr("data-src") == mp3) {
                foundCurrent = true;
            }
        }
    }
    else {
        nextIndex = Math.floor((Math.random() * $("#GV_Files_documents").find(".audio-file").length));
    }

    $(".audio").eq(nextIndex).trigger("click");
}

function PrevSong(mp3) {
    var foundCurrent = false;
    var nextIndex = 0;
    if ($("#shuffle_play_hidden").val() == "false") {
        for (var i = 0; i < $("#GV_Files_documents").find(".audio-file").length; i++) {
            if (foundCurrent) {
                nextIndex = i;
                break;
            }
            else if ($("#GV_Files_documents").find(".audio-file").eq(i).attr("data-src") == mp3) {
                foundCurrent = true;
            }
        }

        if (nextIndex == 0 && $("#GV_Files_documents").find(".audio-file").length > 1) {
            nextIndex = 1;
        }
        else if (nextIndex - 2 < 0) {
            nextIndex = $("#GV_Files_documents").find(".audio-file").length - 1;
        }
        else {
            nextIndex = nextIndex - 2;
        }
    }
    else {
        nextIndex = Math.floor((Math.random() * $("#GV_Files_documents").find(".audio-file").length));
    }

    $(".audio").eq(nextIndex).trigger("click");
}

function ReloadEqualizer(mp3) {
    if (currPlaying_ID != "") {
        $("#" + currPlaying_ID).parent().parent().find(".equalizer-holder").show();
    }
}

function ContinuousPlay_Changed() {
    if ($("#cb_cont_play").prop("checked")) {
        $("#continue_play_hidden").val("true");
    }
    else {
        $("#continue_play_hidden").val("false");
    }
}

function ShufflePlay_Changed() {
    if ($("#cb_shuffle_play").prop("checked")) {
        $("#shuffle_play_hidden").val("true");
    }
    else {
        $("#shuffle_play_hidden").val("false");
    }
}

function RepeatPlay_Changed() {
    if ($("#cb_repeat_play").prop("checked")) {
        $("#repeat_play_hidden").val("true");
        $("#cb_cont_play").prop("disabled", true);
        $("#cb_shuffle_play").prop("disabled", true);
    }
    else {
        $("#repeat_play_hidden").val("false");
        $("#cb_cont_play").prop("disabled", false);
        $("#cb_shuffle_play").prop("disabled", false);
    }
}

function ReloadPlay() {
    try {
        if (currPlaying_ID != "") {
            var $_currPlaying = $("#" + currPlaying_ID);
            if (($_currPlaying != null) && ($_currPlaying != "") && ($_currPlaying != undefined)) {
                var srcPrev = $_currPlaying.attr("src");
                srcPrev = srcPrev.replace("play.png", "stop.png");
                $_currPlaying.attr("src", srcPrev);
                $_currPlaying.parent().parent().find(".equalizer-holder").append(equalizerDiv);
                $_playButton.parent().parent().find(".equalizer-holder").show();
            }
        }
    }
    catch (evt) { }
}

function createnewfolder() {
    $.ajax({
        url: "../../Apps/FileDrive/FileDriveService.asmx/NewFolder",
        type: "POST",
        data: '{ "foldername": "' + document.getElementById('tb_newfolderentry').value + '","group": "' + document.getElementById('hf_ddgroups').value + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                document.getElementById('foldermessage').innerHTML = "<small style='color: Green;'>Folder Created</small>";
                document.getElementById('tb_newfolderentry').value = '';
                document.getElementById('tb_newfolderentry').focus();
                document.getElementById('hf_searchFiles_documents').value = "create";
                __doPostBack('hf_searchFiles_documents', "");
                window.setTimeout(function () { document.getElementById('foldermessage').innerHTML = ""; }, 5000);
            } else {
                document.getElementById('foldermessage').innerHTML = "<small style='color: Red;'>Error Creating Folder! Check folder name then continue.</small>";
                document.getElementById('tb_newfolderentry').focus();
                openWSE.RemoveUpdateModal();
                window.setTimeout(function () { document.getElementById('foldermessage').innerHTML = ""; }, 5000);
            }
        }
    });
}