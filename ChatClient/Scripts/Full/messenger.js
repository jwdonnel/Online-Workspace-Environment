/* --------------------------------------SO MESSENGER----------------------------------------------
* Copyright: John Donnelly
* Version: 2.3
* Created: 11/25/2013
* Last Updated: 10/29/2015
*
* Requirements: Internet Explorer 7.0 and above, Firefox 3.0 and above, Chrome (all), Opera
* ------------------------------------------------------------------------------------------------
*/

var pageUrl = "../ChatClient/ChatService.asmx";
var currUser = "";
var loadingtag = "<h3 class='float-left' style='padding-top: 7px; padding-left: 5px;'>Loading posts. Please wait...</h3>";
var loadingposts = 0;
var isPostBack = 1;
var isLoadingModal = 1;

function getParameterByName(e) {
    e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var t = "[\\?&]" + e + "=([^&#]*)";
    var n = new RegExp(t);
    var r = n.exec(window.location.search);
    if (r == null) {
        return "";
    }
    else {
        return decodeURIComponent(r[1].replace(/\+/g, " "))
    }
}

$(document).ready(function () {
    Array.prototype.indexOf;
    if (!Array.prototype.indexOf) {
        Array.prototype.indexOf = function (obj, start) {
            for (var i = (start || 0), j = this.length; i < j; i++) {
                if (this[i] === obj) {
                    return i;
                }
            }
            return -1;
        };
    }

    LoadFontStyles();

    currUser = getParameterByName("user");
    $("#chatmessages").html(loadingtag);
    loadingposts = 1;
    updateChatMessages();
    FindAndCreateEmoticons();

    document.title = currUser + " - Chat Client";
});

function FindAndCreateEmoticons() {
    $.ajax({
        type: "POST",
        url: pageUrl + "/GetEmoticons",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#emoticons-holder").html("");
            if (data.d.length > 0) {
                for (var i = 0; i < data.d.length; i++) {
                    var title = data.d[i];
                    title = title.substring(title.lastIndexOf("/") + 1);
                    title = title.replace(title.substring(title.indexOf(".")), "");
                    $("#emoticons-holder").append("<img alt='{" + i + "}' src='" + data.d[i] + "' title='" + title + "' />");
                }
            }
        }
    });
}

var _isTyping = false;
function ChatBoxKeyDown(event) {
    try {
        if (event.which == 13) {
            postChatMessage();
            _isTyping = false;
            $('#chatEnterMessage').focus();
            return false;
        }
        else {
            if (_isTyping == false) {
                isTyping("1");
                _isTyping = true;
            }
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            postChatMessage();
            _isTyping = false;
            $('#chatEnterMessage').focus();
            return false;
        }
        else {
            if (_isTyping == false) {
                isTyping("1");
                _isTyping = true;
            }
        }
        delete evt;
    }
}

function ChatBoxFocus() {
    if ((($.trim($('#chatEnterMessage').val()) == "Type message here...") || ($.trim($('#chatEnterMessage').val()) == "")) && (!$('#chatEnterMessage').is(':disabled')) && (_isTyping == false)) {
        $('#chatEnterMessage').val("");
        if (loadingposts == 0) {
            isTyping("0");
        }
    }
}

function ChatBoxBlur() {
    isTyping("0");
    _isTyping = false;
    if ($.trim($('#chatEnterMessage').val()) == "") {
        $('#chatEnterMessage').val("Type message here...");
    }
}

$(document.body).on("focus", "#chatEnterMessage", function () {
    ChatBoxFocus();
});

$(document.body).on("blur", "#chatEnterMessage", function () {
    ChatBoxBlur();
});

$(document.body).on("click", ".emoticons img", function () {
    var textbox = document.getElementById("chatEnterMessage");
    if (!textbox.disabled) {
        var emoticon = $(this).attr("alt");
        if (textbox.value == "Type message here...") {
            textbox.value = "";
        }

        textbox.value += emoticon + " ";
        textbox.focus();
    }
});

function ViewEmoticons() {
    $("#addImage-overlay").hide();
    $this = $("#emoticon-overlay");
    if ($this.css("display") != "block") {
        $this.show();
        var marTop = -($this.height() / 2);
        $this.css({
            marginTop: marTop
        });
    }
    else {
        $this.hide();
    }
}

function ViewAddImage() {
    $("#emoticon-overlay").hide();
    $this = $("#addImage-overlay");
    $this.find("#tb_addImage").val("Image Url");
    if ($this.css("display") != "block") {
        $this.show();
        $('#tb_addImage').focus();
        var marTop = -($this.height() / 2);
        $this.css({
            marginTop: marTop
        });
    }
    else {
        $this.hide();
    }
}

function AddImage() {
    var textbox = document.getElementById("chatEnterMessage");
    if (!textbox.disabled) {
        var text = $.trim($("#tb_addImage").val());
        if (checkURL(text)) {
            if ((text != "Image Url") && (text != "")) {
                textbox.value = "<img alt='' src='" + text + "' class='chat-message-image' />";
                $("#addImage-overlay").hide();
                postChatMessage();
            }
        }
        else {
            $("#image-error-msg").html("<span style='color: Red;'>Not an image!</span>");
            setTimeout(function () {
                $("#image-error-msg").html("");
            }, 2500);
        }
    }
}

function checkURL(url) {
    return (url.match(/\.(jpeg|jpg|gif|png)$/) != null);
}

function AddImageKeyPress() {
    try {
        if (event.which == 13) {
            AddImage();
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddImage();
            return false;
        }
        delete evt;
    }
}

function GetEmoticon(smsg) {
    $(".emoticons img").each(function () {
        var $this = $(this);
        var src = $this.attr("src");
        var emoticon = $this.attr("alt");
        while (smsg.indexOf(emoticon) != -1) {
            smsg = smsg.replace(emoticon, "<img alt='' src='" + src + "' />");
        }
    });
    return smsg;
}

var _isTypingCall = false;
function isTyping(t) {
    if (!_isTypingCall) {
        _isTypingCall = true;
        $.ajax({
            type: "POST",
            url: pageUrl + "/CallIsTyping",
            data: '{ "typing": "' + t + '","userto": "' + currUser + '" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            complete: function (data) {
                _isTypingCall = false;
            }
        });
    }
}

var _isAddCall = false;
function postChatMessage() {
    var textbox = document.getElementById("chatEnterMessage");
    if ((textbox.value == "Type message here...") || (textbox.value == "")) {
        textbox.disabled = false;
    }
    else {
        if (!_isAddCall) {
            _isAddCall = true;
            $("#emoticon-overlay").hide();
            $("#addImage-overlay").hide();
            $("#chatmessages").append("<div id='chatloading_img'>Please wait...</div>");
            var elem = document.getElementById("chatmessages");
            elem.scrollTop = elem.scrollHeight;
            textbox.disabled = true;
            $.ajax({
                type: "POST",
                url: pageUrl + "/CallAddMessage",
                data: '{ "message": "' + escape(textbox.value) + '","userto": "' + currUser + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d[0] != null) {
                        textbox.value = "";
                        var tempcm = $.trim($('#chatmessages').html());
                        tempcm = tempcm.replace(/"/gi, "'");
                        var tempnoposts = "<div class='chatLineSep-noposts'>No posts available</div>";
                        if (tempcm == tempnoposts) {
                            $('#chatmessages').html("");
                        }


                        updateMessages(data.d);
                        _isTyping = false;
                        textbox.focus();
                        textbox.disabled = false;
                    }

                    _isAddCall = false;
                },
                error: function (data) {
                    textbox.value = "";
                    var tempcm = $.trim($('#chatmessages').html());
                    tempcm = tempcm.replace(/"/gi, "'");
                    var tempnoposts = "<div class='chatLineSep-noposts'>No posts available</div>";
                    if (tempcm == tempnoposts) {
                        $('#chatmessages').html("");
                    }
                    _isTyping = false;
                    textbox.focus();
                    textbox.disabled = false;
                    _isAddCall = false;
                }
            });
        }
    }
}

function LoadFontStyles() {
    $.ajax({
        type: "POST",
        url: pageUrl + "/LoadFontStyle",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            if (data.d.length == 3) {
                LoadCustomFontFamily(data.d[0]);
                LoadCustomFontSize(data.d[1]);
                LoadCustomFontColor(data.d[2]);
            }
        }
    });
}
function LoadCustomFontFamily(url) {
    if (url != null && url != "") {
        $("link[href='" + url + "']").remove();
        var head = document.getElementsByTagName('head')[0];
        link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = url;
        head.appendChild(link);
    }
}
function LoadCustomFontSize(fontSize) {
    if (fontSize != null && fontSize != "") {
        $("body").css("font-size", fontSize);
    }
}
function LoadCustomFontColor(fontColor) {
    if (fontColor != null && fontColor != "") {
        $("body").css("color", fontColor);
    }
}


var _isUpdateCall = false;
function updateChatMessages() {
    if (!_isUpdateCall) {
        _isUpdateCall = true;
        $.ajax({
            type: "POST",
            url: pageUrl + "/CallMessages",
            data: '{ "userto": "' + currUser + '","isPostBack": "' + isPostBack + '" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data.d[0] != null)
                    updateMessages(data.d[0]);

                _isUpdateCall = false;
                updateChatMessages();
            },
            error: function (data) {
                _isUpdateCall = false;
                updateChatMessages();
            }
        });
    }
}

function updateMessages(data) {
    setChatMessages(data[1]);
    setIsTyping(data[2]);
    setOnlineStatus(data[0]);
}

function setChatMessages(msg) {
    if (loadingposts == 1) {
        if ($.trim(msg) != $.trim($("#chatmessages").text())) {
            $("#chatmessages").html("");
            loadingposts = 0;
        }
    }
    var elem;
    if ((msg != "") && (msg != null) && (isPostBack == 1)) {
        $("#chatmessages").hide();
        $("#chatmessages").html(GetEmoticon(msg));
        var contMessageArray = new Array();
        for (var i = $(".chatLineSep").size() - 1; i >= 0; i--) {
            if ($(".chatLineSep").eq(i).hasClass("continued-chat")) {
                contMessageArray.push("<div class='clear-space-five'></div>" + $(".chatLineSep").eq(i).html());
                $(".chatLineSep").eq(i).remove();
            }
            else {
                for (var j = contMessageArray.length - 1; j >= 0; j--) {
                    $(".chatLineSep").eq(i).find(".smsg-text").append(contMessageArray[j]);
                }
                contMessageArray = new Array();
            }
        }
        $("#chatmessages").show();
        elem = document.getElementById("chatmessages");
        elem.scrollTop = elem.scrollHeight;
        AdjustScroll();
    }

    if ((msg != "") && (msg != null) && (isPostBack == 0)) {
        $("#chatloading_img").remove();
        if ($("#chatmessages").find(".chatLineSep-noposts").text().toLowerCase() == "no posts available")
            $("#chatmessages").html("");


        var obj = $(".chatLineSep");
        var arr = $.makeArray(obj);
        var index = $(".chatLineSep").size() - 1;
        var dateId1 = $(arr).eq(index).find(".date-nodisplay").attr("id");
        var dateId2 = $(msg).find("#" + dateId1);

        if (dateId2.length == 0) {
            $("#chatmessages").append(GetEmoticon(msg));
            if ($(".chatLineSep").last().hasClass("continued-chat")) {
                var contMessage = $(".chatLineSep").last().html();
                $(".chatLineSep").last().remove();
                for (var i = $(".chatLineSep").size() - 1; i >= 0; i--) {
                    if (!$(".chatLineSep").eq(i).hasClass("continued-chat")) {
                        $(".chatLineSep").eq(i).find(".smsg-text").append("<div class='clear-space-five'></div>" + contMessage);
                        break;
                    }
                }
            }

            elem = document.getElementById("chatmessages");
            elem.scrollTop = elem.scrollHeight;
            AdjustScroll();
        }
    }

    if (isPostBack == 1) {
        isPostBack = 0;
    }

    AdjustTimeZone();
}

function AdjustScroll() {
    $("img").load(function () {
        elem = document.getElementById("chatmessages");
        elem.scrollTop = elem.scrollHeight;
    });
}

function AdjustTimeZone() {
    $(".date-chat-line").each(function () {
        if (!$(this).hasClass("time-adjusted")) {
            try {
                if ($(this).html().split(";").length > 1) {
                    var d = new Date();
                    var s = $(this).html().split(";")[0];
                    var timeZoneServer = $(this).html().split(";")[1];
                    var parts = s.match(/(\d+)\:(\d+) (\w+)/);
                    var hours = /am/i.test(parts[3]) ? parseInt(parts[1], 10) : parseInt(parts[1], 10) + 12;
                    var minutes = parseInt(parts[2], 10);

                    var currentTimezone = d.getTimezoneOffset();
                    currentTimezone = (-1) * (((currentTimezone / 60) * -1) - parseInt(timeZoneServer));
                    hours = hours - currentTimezone;

                    d.setHours(hours);
                    d.setMinutes(minutes);

                    var splitTime = d.toLocaleTimeString().split(":");
                    var ampm = splitTime[2].split(" ")[1];


                    $(this).html(splitTime[0] + ":" + splitTime[1] + " " + ampm);
                    $(this).addClass("time-adjusted");
                }
            }
            catch (evt) { }
        }
    });
}

function setOnlineStatus(isOnline) {
    var chatEnterMessage = document.getElementById("chatEnterMessage");
    if (isOnline == "offline") {
        chatEnterMessage.disabled = true;
        $("#userStatusMessage").html("<div>User is offline</div>");
        $("#chatEnterMessage").val("");
    }
    else if (isOnline == "online") {
        if ((chatEnterMessage.disabled != false) || (isLoadingModal == 1)) {
            chatEnterMessage.disabled = false;
            $("#userStatusMessage").html("");
            if (isLoadingModal == 1) {
                $("#chatEnterMessage").val("");
            }
            else {
                $("#chatEnterMessage").val("Type message here...");
            }
            $("#chatEnterMessage").focus();
        }
    }

    isLoadingModal = 0;
}

function setIsTyping(msg) {
    if (msg == "istyping") {
        $("#userStatusMessage").html("<div>" + currUser + " is typing...</div>");
    }
    else {
        $("#userStatusMessage").html("");
    }
}