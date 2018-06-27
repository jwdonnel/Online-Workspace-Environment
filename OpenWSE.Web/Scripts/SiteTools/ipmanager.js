$(document.body).on("keydown", "#tb_createnew_listener", function (e) {
    if (event.which == 13 || event.keyCode == 13) {
        AddIPToListener();
        e.preventDefault();
    }
});

function AddIPToListener() {
    loadingPopup.Message("Updating...");
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/AddIP", '{ "ip": "' + $.trim($("#tb_createnew_listener").val()) + '" }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateIPListener").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateIPListener", "");
        }
        else if (!openWSE.ConvertBitToBoolean(data.d)) {
            openWSE.AlertWindow("IP Address is invalid");
            loadingPopup.RemoveMessage();
        }
        else if (data.d == "duplicate") {
            openWSE.AlertWindow("Duplicate ip address");
            loadingPopup.RemoveMessage();
        }
        else {
            openWSE.AlertWindow("Error trying to add IP");
            loadingPopup.RemoveMessage();
        }
    }, function () {
        openWSE.AlertWindow("There was an error adding ip. Please try again.");
    });
}

function DeleteIP(ip) {
    openWSE.ConfirmWindow("Are you sure you want to delete this IP?",
        function () {
            loadingPopup.Message("Deleting IP. Please Wait...");
            openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/DeleteIP", '{ "ip": "' + ip + '" }', null, function (data) {
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    $("#hf_UpdateIPListener").val(new Date().toString());
                    openWSE.CallDoPostBack("hf_UpdateIPListener", "");
                }
                else {
                    openWSE.AlertWindow("Error Deleting IP");
                    loadingPopup.RemoveMessage();
                }
            }, function () {
                openWSE.AlertWindow("There was an error deleting ip. Please try again.");
            });
        }, null);
}

function blockAllowIpAddress(ip) {
    loadingPopup.Message("Please Wait...");
    $("#hf_BlockAllowIPAddress_Watchlist").val(ip);
    openWSE.CallDoPostBack("hf_BlockAllowIPAddress_Watchlist", "");
}

function AddIPWatchlist() {
    var ipAddress = $.trim($("#txt_ipaddress_watchlist").val());
    if (ipAddress) {
        loadingPopup.Message("Please Wait...");
        $("#hf_AddIPAddress_Watchlist").val(escape(ipAddress));
        openWSE.CallDoPostBack("hf_AddIPAddress_Watchlist", "");
    }
    else {
        openWSE.AlertWindow("IP Address cannot be empty. Please try again.");
    }
}

function SetToCurrentIP(ip) {
    var setIpBtn = "<span class=\"td-view-btn use-currentip-btn\" onclick=\"SetToCurrentIP_Clicked('" + ip + "', this);\" title=\"Use current IP\"></span>";

    $(".use-currentip-btn").remove();

    if ($("#tb_createnew_listener").length > 0) {
        $("#tb_createnew_listener").parent().append(setIpBtn);
    }

    if ($("#txt_ipaddress_watchlist").length > 0) {
        $("#txt_ipaddress_watchlist").parent().append(setIpBtn);
    }
}

function SetToCurrentIP_Clicked(ip, _this) {
    if ($(_this).parent().find("input").length > 0) {
        $(_this).parent().find("input").val(ip);
    }
}

function confirmDeleteWatchlistIP(ip) {
    openWSE.ConfirmWindow("Are you sure you want to delete this IP?",
        function () {
            loadingPopup.Message("Deleting IP. Please Wait...");
            $("#hf_DeleteWatchlistIP").val(ip);
            openWSE.CallDoPostBack("hf_DeleteWatchlistIP", "");
        }, null);
}

var __this;
function UpdateActive(ip, active, _this) {
    __this = _this;
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/UpdateIP", '{ "ip": "' + ip + '","activeIP": "' + active + '"  }', null, function (data) {
        if (openWSE.ConvertBitToBoolean(data.d)) {
            $("#hf_UpdateIPListener").val(new Date().toString());
            openWSE.CallDoPostBack("hf_UpdateIPListener", "");
        }
        else if (data.d == "sameip") {
            openWSE.AlertWindow("Cannot disable your own IP address when more than one are active.");
            loadingPopup.RemoveMessage();
            ResetStatus(__this);
        }
        else if (data.d == "differentip") {
            openWSE.AlertWindow("Cannot Enable another IP address until your current IP address is.");
            loadingPopup.RemoveMessage();
            ResetStatus(__this);
        }
        else {
            loadingPopup.RemoveMessage();
        }
    }, function () {
        openWSE.AlertWindow("There was an error updating ip. Please try again.");
    });
}

function ResetStatus(_this) {
    var $_id = $(_this).closest(".switch-slider");
    var left = $_id.css("left");
    var totalWidth = ($_id.parent().width() - $_id.find(".RadioButton-Toggle-Overlay").width()) + 2;
    if ((left == "0") || (left == "0px")) {
        $_id.animate({
            left: -totalWidth
        }, openWSE_Config.animationSpeed, function () {
            $_id.find(".cb-disable").find('input').trigger('click');
        });
    }
    else {
        $_id.animate({
            left: 0
        }, openWSE_Config.animationSpeed, function () {
            $_id.find(".cb-enable").find('input').trigger('click');
        });
    }
}

$(document.body).on("keypress", "#MainContent_tb_autoblock_count", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

