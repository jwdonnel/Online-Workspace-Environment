var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function (sender, args) {
});
prm.add_endRequest(function (sender, args) {
    ReloadPlay();
    ReloadEqualizer();
    GetFilesChecked();
});
$(document).ready(function () {
    ReloadPlay();
    equalizerDiv = "<div class='equalizer-img' style='background-image: url(\"" + openWSE.siteRoot() + "Apps/FileDrive/Images/equalizer.gif\");'></div>";
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
        $(".content-main").css("padding-bottom", "");
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
            var stopImg = "<a href='#stop' title='Stop' class='stop-btn margin-right' onclick=\"StopCurrSong('" + path + "');return false;\"><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/stop.png' class='cursor-pointer'></a>";
            var nextImg = "<a href='#next' title='Next Song' class='next-btn' onclick=\"NextSong('" + path + "');return false;\"><img alt=''src='" + openWSE.siteRoot() + "App_Themes/" + openWSE_Config.siteTheme + "/App/next.png' class='cursor-pointer'></a>";

            var audioControls = "<div class='clear'></div><table cellpadding='5' cellspacing='5' width='100%'><tr>";
            audioControls += "<td width='43%' align='left' style='font-size: 17px;'>" + $_playButton.parent().find(".audio-file").html() + "</td>";
            audioControls += "<td width='57%' align='right'><div class='pad-left-big float-left'>" + prevImg + playImg + pauseImg + stopImg + nextImg + "</div>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_cont_play' onchange='ContinuousPlay_Changed();' value='false'" + checked + " /><label for='cb_cont_play'>&nbsp;Continuous</label></span>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_shuffle_play' onchange='ShufflePlay_Changed();' value='false'" + checked2 + " /><label for='cb_shuffle_play'>&nbsp;Shuffle</label></span>";
            audioControls += "<span class='margin-left margin-right'><input type='checkbox' id='cb_repeat_play' onchange='RepeatPlay_Changed();' value='false'" + checked3 + " /><label for='cb_repeat_play'>&nbsp;Repeat</label></span></td>";
            audioControls += "</tr></table>";
            $("#audioPlayer").append(audioControls);
            $(".content-main").css("padding-bottom", "75px");
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
function StopCurrSong(path) {
    $(".audio").each(function () {
        if ($(this).prev().attr("data-src") == path) {
            var src = $(this).attr("src");
            src = src.replace("stop.png", "play.png");
            $(this).attr("src", src);
            $(".equalizer-img").remove();
            $(".equalizer-holder").hide();
            $("#audioPlayer").html("");
            $("#audioPlayer").css("min-height", "0px");
            $(".content-main").css("padding-bottom", "");
            currPlaying_ID = "";
            return;
        }
    });
}
function NextSong(mp3) {
    var foundCurrent = false;
    var nextIndex = 0;
    if ($("#shuffle_play_hidden").val() == "false") {
        for (var i = 0; i < $("#pnl_FilesDocuments").find(".audio-file").length; i++) {
            if (foundCurrent) {
                nextIndex = i;
                break;
            }
            else if ($("#pnl_FilesDocuments").find(".audio-file").eq(i).attr("data-src") == mp3) {
                foundCurrent = true;
            }
        }
    }
    else {
        nextIndex = Math.floor((Math.random() * $("#pnl_FilesDocuments").find(".audio-file").length));
    }

    $(".audio").eq(nextIndex).trigger("click");
}
function PrevSong(mp3) {
    var foundCurrent = false;
    var nextIndex = 0;
    if ($("#shuffle_play_hidden").val() == "false") {
        for (var i = 0; i < $("#pnl_FilesDocuments").find(".audio-file").length; i++) {
            if (foundCurrent) {
                nextIndex = i;
                break;
            }
            else if ($("#pnl_FilesDocuments").find(".audio-file").eq(i).attr("data-src") == mp3) {
                foundCurrent = true;
            }
        }

        if (nextIndex == 0 && $("#pnl_FilesDocuments").find(".audio-file").length > 1) {
            nextIndex = 1;
        }
        else if (nextIndex - 2 < 0) {
            nextIndex = $("#pnl_FilesDocuments").find(".audio-file").length - 1;
        }
        else {
            nextIndex = nextIndex - 2;
        }
    }
    else {
        nextIndex = Math.floor((Math.random() * $("#pnl_FilesDocuments").find(".audio-file").length));
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

var selectedFileList = new Array();
function OnFileCheckedChange(_this) {
    if ($(_this).is(":checked")) {
        var foundItem = false;
        for (var i = 0; i < selectedFileList.length; i++) {
            if (selectedFileList[i] == $(_this).attr("data-id")) {
                foundItem = true;
                break;
            }
        }

        if (!foundItem) {
            selectedFileList.push($(_this).attr("data-id"));
        }
    }
    else {
        var tempArray = new Array();
        for (var i = 0; i < selectedFileList.length; i++) {
            if (selectedFileList[i] != $(_this).attr("data-id")) {
                tempArray.push(selectedFileList[i]);
            }
        }

        selectedFileList = tempArray;
    }

    GetFilesChecked();
}
function OnSelectAllCheckChanged(_this) {
    selectedFileList = new Array();
    $("#pnl_FilesDocuments").find(".cb-movefile").each(function () {
        if (!$(this).closest(".myItemStyle").hasClass("hide-table-row") && !$(this).closest(".myItemStyle").hasClass("search-hide")) {
            if ($(_this).is(":checked")) {
                $(this).prop("checked", true);
                $(this).attr("checked", "checked");

                var foundItem = false;
                for (var i = 0; i < selectedFileList.length; i++) {
                    if (selectedFileList[i] == $(this).attr("data-id")) {
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem) {
                    selectedFileList.push($(this).attr("data-id"));
                }
            }
            else {
                $(this).prop("checked", false);
                $(this).removeAttr("checked");
            }
        }
    });

    GetFilesChecked();
}
function GetFilesChecked() {
    var listStr = "";
    for (var i = 0; i < selectedFileList.length; i++) {
        listStr += selectedFileList[i] + ";";
    }
    $("#hf_moveFiles_documents").val(listStr);

    $("#pnl_FilesDocuments").find(".cb-movefile").each(function () {
        var id = $(this).attr("data-id");
        for (var i = 0; i < selectedFileList.length; i++) {
            if (id == selectedFileList[i]) {
                $(this).attr("checked", "checked");
                break;
            }
        }
    });

    var totalAvailable = 0;
    var checkedCount = 0;
    $("#pnl_FilesDocuments").find(".cb-movefile").each(function () {
        if (!$(this).closest(".myItemStyle").hasClass("hide-table-row") && !$(this).closest(".myItemStyle").hasClass("search-hide")) {
            totalAvailable++;
            if ($(this).is(":checked")) {
                checkedCount++;
            }
        }
    });

    $("#pnl_FilesDocuments").find(".myHeaderStyle").find("input[type='checkbox']").prop("checked", false);
    $("#pnl_FilesDocuments").find(".myHeaderStyle").find("input[type='checkbox']").removeAttr("checked");
    if (checkedCount == totalAvailable && checkedCount > 0) {
        $("#pnl_FilesDocuments").find(".myHeaderStyle").find("input[type='checkbox']").prop("checked", true);
        $("#pnl_FilesDocuments").find(".myHeaderStyle").find("input[type='checkbox']").attr("checked", "checked");
    }

    if (selectedFileList.length === 0) {
        $("#movetofolder-holder").hide();
    }
    else {
        $("#movetofolder-holder").show();
    }
}

$(document.body).on("change", "#dd_folders, #dd_folderEditList", function () {
    loadingPopup.Message("Updating. Please Wait...");
});
$(document.body).on("change", "#dd_groups", function () {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_groupsChange").val($(this).val());
    openWSE.CallDoPostBack("hf_groupsChange", "");
});
$(document.body).on("change", "#dd_moveGroup_documents", function () {
    loadingPopup.Message("Updating. Please Wait...");
    $("#hf_moveGroupChange").val($(this).val());
    openWSE.CallDoPostBack("hf_moveGroupChange", "");
});

function EditFile(id) {
    loadingPopup.Message("Loading...");
    $("#hf_fileEdit").val(id);
    openWSE.CallDoPostBack("hf_fileEdit", "");
}
function LoadEditValues(id) {
    var dataValue = $(".myItemStyle[data-id='" + id + "']").find(".editFilename").attr("data-value");
    dataValue = unescape(dataValue);
    dataValue = dataValue.replace(/!_!/g, " ");
    $(".myItemStyle[data-id='" + id + "']").find(".editFilename").val(dataValue);

    dataValue = $(".myItemStyle[data-id='" + id + "']").find(".editFolder").attr("data-value");
    $(".myItemStyle[data-id='" + id + "']").find(".editFolder").val(dataValue);
}
function UpdateFile(id, ext) {
    loadingPopup.Message("Updating...");
    $("#hf_fileUpdate").val(id);
    $("#hf_fileUpdateName").val(escape($(".editFilename").val()));
    $("#hf_fileUpdateExt").val(ext);
    $("#hf_fileUpdateFolder").val($(".editFolder").val());

    openWSE.CallDoPostBack("hf_fileUpdate", "");
}
function DownloadFile(id) {
    $("#hf_fileDownload").val(id);
    $("#btn_DownloadFile").trigger("click");
}
function DeleteFile(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this file?", function () {
        loadingPopup.Message("Deleting...");
        $("#hf_fileDelete").val(id);
        openWSE.CallDoPostBack("hf_fileDelete", "");
    }, null);
}
function DeleteSelectedFiles() {
    openWSE.ConfirmWindow("Are you sure you want to delete the selected files?", function () {
        loadingPopup.Message("Deleting...");
        $("#hf_fileDeleteSelected").val(new Date().toString());
        openWSE.CallDoPostBack("hf_fileDeleteSelected", "");
    }, null);
}

function DeleteFolder() {
    openWSE.ConfirmWindow("Are you sure you want to delete this folder?", function () {
        loadingPopup.Message("Deleting...");
        $("#hf_DeleteFolder").val($("#dd_folderEditList").val());
        openWSE.CallDoPostBack("hf_DeleteFolder", "");
    }, null);
}
