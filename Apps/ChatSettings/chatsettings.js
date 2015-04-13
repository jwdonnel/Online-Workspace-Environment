Sys.Application.add_load(function () {
    GetChatTimeout();
});

function GetChatTimeout() {
    openWSE.RadioButtonStyle();
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/GetChatTimeout",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            $("#tb_updateintervals_chatSettings").val(data.d);
        }
    });
}

$(document.body).on("click", ".RandomActionBtns-mmes, .catButtons-mmes", function() {
    UpdateScreen_MyMessages();
});

$(document.body).on("change", "#chatsettings-options", function () {
    $("#blocked-users-holder").html("");
    $(".chat-settings-divs").hide();

    var val = $("#chatsettings-options").val();
    if (val == "Chat-Blocked-Users-div") {
        LoadBlockedUsers();
    }

    $("#" + val).fadeIn(openWSE_Config.animationSpeed, function () { openWSE.RadioButtonStyle(); });
});

function LoadBlockedUsers() {
    $("#blocked-users-holder").html("<div class='pad-all'><h3>Loading blocked users. Please wait...</h3></div>");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/LoadBlockedUsers",
        data: '{ }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            $("#blocked-users-holder").html(data.d);
        }
    });
}

function UpdateScreen_MyMessages() {
    $.LoadingMessage("#chatlog-load");
}

function CheckIfAnyLogs() {
    if ($(".messageselector").length > 0) {
        $("#nochatlogsavailable").remove();
    }
    else {
        $("#containerMessages").append("<div id='nochatlogsavailable' class='emptyGridView'>You have no chat logs recorded.</div>");
    }
}

function DeleteLog(_this, date) {
    openWSE.ConfirmWindow("Are you sure you want to delete this chat log?",
        function () {
            UpdateScreen_MyMessages();
            var $this = $(_this).closest(".messageselector");
            $.ajax({
                type: "POST",
                url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/DeleteLog",
                data: '{ "messageDate": "' + date + '" }',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                cache: false,
                success: function (data) {
                    var response = data.d;
                    $(".mb-container").html("");
                    $this.parent().remove();
                    $("#update-element").remove();
                    CheckIfAnyLogs();
                }
            });
        }, null);
}

function eventMessageOpen(_this) {
    var id = $(_this).closest(".messageselector").attr("id");
    try {
        var $this = $("#" + id);
        var m = $(".messagebody-id")[0];
        var m_hf = $this.parent().find(".hf-message-mID")[0];
        var messageid = m_hf.value;
        $(".messageselector").each(function () {
            if ($(this).attr("id") == id) {
                $(this).css({ background: "#efefef", fontWeight: "bold" });
            } else {
                $(this).css({ background: "", fontWeight: "" });
            }
        });

        UpdateScreen_MyMessages();

        $.ajax({
            type: "POST",
            url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/LoadMessage",
            data: '{ "type": "' + "chat" + '", "messageId": "' + messageid + '" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            success: function (data) {
                var response = data.d;
                $(".mb-container").html("");
                $("#update-element").remove();

                $("#emoticons_log_chatsettings img").each(function () {
                    var $this = $(this);
                    var src = $this.attr("src");
                    var emoticon = $this.attr("alt");
                    while (response.indexOf(emoticon) != -1) {
                        response = response.replace(emoticon, "<img alt='' src='" + src + "' />");
                    }
                });

                if (response != "") {
                    $(".mb-container").html(response);
                }
            },
            error: function (data) {
                openWSE.AlertWindow("Failed to get message!");
            }
        });
    }
    catch (evt) {
    }
}

$(document.body).on("change", "#rb_chatsoundnoti_on", function () {
    openWSE.RemoveUpdateModal();
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/UpdateSettingSound",
        data: '{ "allow": "' + "1" + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();
            $("#hf_chatsound").val("1");
        }
    });
});

$(document.body).on("change", "#rb_chatsoundnoti_off", function () {
    openWSE.RemoveUpdateModal();
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/UpdateSettingSound",
        data: '{ "allow": "' + "0" + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();
            $("#hf_chatsound").val("0");
        }
    });
});

$(document.body).on("click", "#btn_updateintervals_chatSettings", function () {
    UpdateChatInterval();
});

function UpdateChatInterval() {
    openWSE.RemoveUpdateModal();
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/UpdateSettingTimeout",
        data: '{ "time": "' + $("#tb_updateintervals_chatSettings").val() + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();
        }
    });
}

$(document.body).on("change", ".chatblock-checked", function () {
    var cb = $(this).attr("id").replace(/chatblock_/gi, "");
    cb = cb.replace(/_/gi, " ");
    var blockUser = "0";
    if (this.checked) {
        blockUser = "1";
    }

    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "Apps/ChatSettings/GetChatSettings.asmx/BlockUser",
        data: '{ "block": "' + blockUser + '", "userName": "' + cb + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            try {
                if (openWSE.ConvertBitToBoolean(blockUser)) {
                    if ($("#app-ChatClient-" + cb).length > 0) {
                        $("#app-ChatClient-" + cb).remove();
                    }
                }
                chatClient.updateChatUsers_NoLoop();
            }
            catch (evt) { openWSE.AlertWindow(evt.message); }
        }
    });
});

function ChatIntervalUpdate(event) {
    try {
        if (event.which == 13) {
            UpdateChatInterval();
            return false;
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            UpdateChatInterval();
            return false;
        }
        delete evt;
    }
}

$(document).keypress(function (e) {
    if (e.keyCode === 13) {
        e.preventDefault();
        return false;
    }
});