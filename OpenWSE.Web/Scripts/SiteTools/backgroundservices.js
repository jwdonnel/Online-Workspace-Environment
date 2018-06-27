$(document).ready(function () {
    SetServiceRefreshTimer();
});

var backgroundServicesLocked = false;

var currLogListScroll = 0;
function LoadServiceSettings(id, showLoading) {
    if (showLoading) {
        loadingPopup.Message("Loading Log...");
    }

    var fullNamespace = $.trim($(".main-table-rows[data-serviceid='" + id + "']").attr("data-namespace"));
    openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/GetCurrentBackgroundServiceLogs", "{ serviceNamespace: '" + fullNamespace + "' }", {
        cache: false
    }, function (data) {
        if (data.d != null && data.d != "") {
            try {
                var obj = $.parseJSON(data.d);
                BuildServiceInfo(id, obj);
            }
            catch (evt) { }
        }

        loadingPopup.RemoveMessage();
    }, function (data) {
        loadingPopup.RemoveMessage();
        openWSE.AlertWindow("There was an error trying to get the logs for this service. Please try again.");
    });
}
function BuildServiceInfo(id, obj) {
    var name = $.trim($(".main-table-rows[data-serviceid='" + id + "']").find(".service-name-style").html());
    var state = $.trim($(".state-holder[data-id='" + id + "']").find("span").html());
    var dateUpdated = $.trim($(".date-holder[data-id='" + id + "']").html());

    var str = "";

    str += "<div class='float-left' style='font-size: 14px;'><span>Background Service is&nbsp;</span><span class='state-" + state + "'>" + state + "</span></div>";
    if ($("#elapsedtime_holder").length > 0 && state == "Running") {
        str += "<div class='float-right' style='font-size: 14px;'><span class='date-title-holder' data-id='" + id + "'>Elapsed Time&nbsp;</span><span class='date-holder-modal' data-id='" + id + "'>" + $.trim($("#elapsedtime_holder").html()) + "</span></div>";
    }
    else {
        str += "<div class='float-right' style='font-size: 14px;'><span class='date-title-holder' data-id='" + id + "'>Last Updated&nbsp;</span><span class='date-holder-modal' data-id='" + id + "'>" + dateUpdated + "</span></div>";
    }

    str += "<div class='clear-space'></div>";
    str += "<div class='clear-space'></div>";

    if ($("#log-list-scroll").length > 0) {
        currLogListScroll = $("#log-list-scroll").scrollTop();
    }
    else {
        currLogListScroll = 0;
    }

    if (obj.length > 0) {
        str += "<div id='log-list-scroll' class='margin-top-sml' style='max-height: 250px; overflow: auto;'><table cellpadding='5' cellspacing='0' style='min-width: 100%;'><tbody>";
        str += "<tr class='myHeaderStyle'><td style='min-width: 200px;'>Message</td><td width='150px'>Date</td></tr>";

        for (var i = 0; i < obj.length; i++) {
            str += "<tr class='myItemStyle GridNormalRow'>";
            str += "<td class='border-bottom'>" + unescape(obj[i].message) + "</td>";
            str += "<td class='border-bottom'>" + obj[i].date + "</td></tr>";
        }

        str += "</tbody></table></div>";
    }
    else {
        str += "<div>No logs found</div>";
    }

    str += "<div class='clear'></div>";

    var strButtons = "<input type='button' class='input-buttons modal-cancel-btn' onclick=\"CloseServiceSettings();\" value='Close' />";
    if (obj.length > 0) {
        strButtons += "<input type='button' class='input-buttons modal-ok-btn' onclick=\"ClearLog('" + id + "');\" value='Clear Log' />";
    }
    $("#ServiceStatus-element").find(".ModalButtonHolder").html(strButtons + "<div class='clear'></div>");

    $("#servicesettings_holder").html(str);
    $("#log-list-scroll").scrollTop(currLogListScroll);

    if ($("#ServiceStatus-element").css("display") == "none") {
        var modalTitle = "Background Service Log";
        if (name != "") {
            modalTitle = name + " Log";
        }

        openWSE.LoadModalWindow(true, "ServiceStatus-element", modalTitle);
    }

    if (state == "Running") {
        StartTimeInterval(id, dateUpdated);
    }
}
function CloseServiceSettings() {
    runningTimeInterval = false;
    clearTimeout(timerTimeout);
    openWSE.LoadModalWindow(false, "ServiceStatus-element", "");
    $("#servicesettings_holder").html("");
}

function EditService(id) {
    loadingPopup.Message("Loading...");
    $("#hf_EditService").val(id);
    openWSE.CallDoPostBack("hf_EditService", "");
}
function DeleteService(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this Background Service? Doing so may stop other features from working.",
        function () {
            loadingPopup.Message("Deleting Service...");
            $("#hf_DeleteService").val(id);
            openWSE.CallDoPostBack("hf_DeleteService", "");
        }, null);
}
function ClearLog(id) {
    var name = $.trim($(".main-table-rows[data-serviceid='" + id + "']").find(".service-name-style").html());

    openWSE.ConfirmWindow("Are you sure you want to clear the log for " + name + "?", function () {
        loadingPopup.Message("Deleting Log...");

        runningTimeInterval = false;
        clearTimeout(timerTimeout);

        var fullNamespace = $.trim($(".main-table-rows[data-serviceid='" + id + "']").attr("data-namespace"));
        openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/DeleteBackgroundServiceLog", "{ serviceNamespace: '" + fullNamespace + "' }", {
            cache: false
        }, function (data) {
            loadingPopup.RemoveMessage();
            LoadServiceSettings(id, true);
        }, function (data) {
            loadingPopup.RemoveMessage();
            openWSE.AlertWindow("There was an error trying to get the logs for this service. Please try again.");
            LoadServiceSettings(id, true);
        });
    }, null);
}

function UpdateService(id) {
    loadingPopup.Message("Updating Service...");

    var newName = $.trim($("#tb_newNameEdit").val());
    var newDescription = $.trim($("#tb_newDescriptionEdit").val());
    var logInfo = $("#tb_logInfoEdit").prop("checked");
    var autoStart = $("#tb_autoStartEdit").prop("checked");

    if (newName != "") {
        $("#hf_ServiceNameEdit").val(newName);
        $("#hf_ServiceDescriptionEdit").val(newDescription);
        $("#hf_ServiceLogEdit").val(logInfo);
        $("#hf_ServiceAutoStartEdit").val(autoStart);

        $("#hf_UpdateService").val(id);
        openWSE.CallDoPostBack("hf_UpdateService", "");
    }
    else {
        setTimeout(function () {
            loadingPopup.RemoveMessage();
        }, 250);

        openWSE.AlertWindow("Service name cannot be empty. Please try again.");
    }
}
function CancelEditService() {
    loadingPopup.Message("Loading...");
    $("#hf_CancelService").val(new Date().toString());
    openWSE.CallDoPostBack("hf_CancelService", "");
}

function LoadEditControlValues() {
    if ($("#span_newNameEdit").length > 0 && $("#span_newDescriptionEdit").length > 0) {
        $("#tb_newNameEdit").val($.trim($("#span_newNameEdit").html()));
        $("#tb_newDescriptionEdit").val($.trim($("#span_newDescriptionEdit").html()));
    }
}
function UpdateKeyDownEvent(event, id) {
    try {
        if (event.which == 13) {
            UpdateService(id);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            UpdateService(id);
        }
        delete evt;
    }
}

function UpdateServiceState(id, state) {
    switch (state) {
        case "Sleeping":
        case "Running":
            loadingPopup.Message("Stopping Service...");
            $("#hf_StopService").val(id);
            openWSE.CallDoPostBack("hf_StopService", "");
            break;

        case "Error":
            loadingPopup.Message("Restarting Service...");
            $("#hf_RestartService").val(id);
            openWSE.CallDoPostBack("hf_RestartService", "");
            break;

        case "Stopped":
            loadingPopup.Message("Starting Service...");
            $("#hf_StartService").val(id);
            openWSE.CallDoPostBack("hf_StartService", "");
            break;
    }
}

var runningTimeInterval = false;
var timerTimeout;
function StartTimeInterval(id, dateUpdated) {
    if (!runningTimeInterval) {
        runningTimeInterval = true;
        var $stateEle = $(".state-holder[data-id='" + id + "']").find("span");
        var $dateTitleObj = $("#ServiceStatus-element").find(".date-title-holder[data-id='" + id + "']");

        if ($("#ServiceStatus-element").css("display") == "block" && $stateEle.length > 0 && $dateTitleObj.length > 0) {
            if ($.trim($stateEle.html()) == "Running") {
                $dateTitleObj.html("Elapsed Time&nbsp;");
                if (TimeElapsedUpdate(id, dateUpdated)) {
                    timerTimeout = setTimeout(function () {
                        runningTimeInterval = false;
                        StartTimeInterval(id, dateUpdated)
                    }, 1000);
                }
            }
            else {
                runningTimeInterval = false;
                $dateTitleObj.html("Last Updated&nbsp;");
            }
        }
    }
}
function TimeElapsedUpdate(id, dateUpdated) {
    var $timeEle = $("#ServiceStatus-element").find(".date-holder-modal[data-id='" + id + "']");
    if ($timeEle.length > 0) {
        try {
            var milliseconds = new Date() - new Date(dateUpdated);
            var seconds = (milliseconds / 1000) % 60;
            var minutes = ((milliseconds / (1000 * 60)) % 60);
            var hours = ((milliseconds / (1000 * 60 * 60)) % 60);

            var hourStr = Math.floor(hours).toString();
            if (hourStr.length == 1) {
                hourStr = "0" + hourStr;
            }

            var minuteStr = Math.floor(minutes).toString();
            if (minuteStr.length == 1) {
                minuteStr = "0" + minuteStr;
            }

            var secondStr = Math.floor(seconds).toString();
            if (secondStr.length == 1) {
                secondStr = "0" + secondStr;
            }

            $timeEle.html("<span id='elapsedtime_holder'><span class='font-bold'>" + hourStr + ":" + minuteStr + ":" + secondStr + "&nbsp;</span>(HH:mm:ss)</span>");
        }
        catch (evt) {
            $timeEle.html("<span id='elapsedtime_holder'>N/A</span>");
        }

        return true;
    }

    return false;
}

var refreshInterval = 5 * 1000; // 8 Seconds
function SetServiceRefreshTimer() {
    setTimeout(function () {
        openWSE.AjaxCall("WebServices/UpdateServerSettings.asmx/GetCurrentBackgroundServiceStates", "{ }", {
            cache: false
        }, function (data) {
            if (data.d != null && data.d != "") {
                try {
                    var obj = $.parseJSON(data.d)
                    for (var i = 0; i < obj.length; i++) {
                        $(".state-holder[data-id='" + obj[i].id + "']").each(function () {
                            $(this).html("<span class='state-" + obj[i].state + "'>" + obj[i].state + "</span>");
                        });
                        $(".state-update-btn[data-id='" + obj[i].id + "']").each(function () {
                            $(this).html(GetActionBtn(obj[i].id, obj[i].state));
                        });
                        $(".date-holder[data-id='" + obj[i].id + "']").each(function () {
                            $(this).html(obj[i].date);
                        });
                        $(".user-holder[data-id='" + obj[i].id + "']").each(function () {
                            $(this).html(obj[i].user);
                        });
                        $(".cpu-holder[data-id='" + obj[i].id + "']").each(function () {
                            $(this).html(obj[i].cpu);
                        });

                        if ($("#ServiceStatus-element").find(".date-holder-modal[data-id='" + obj[i].id + "']").length > 0) {
                            LoadServiceSettings(obj[i].id, false);
                        }
                    }
                }
                catch (evt) { }
            }

            SetServiceRefreshTimer();
        }, function (data) {
            SetServiceRefreshTimer();
        });
    }, refreshInterval);
}

function GetActionBtn(id, state) {
    if (!backgroundServicesLocked) {
        switch (state) {
            case "Running":
            case "Sleeping":
                return "<a href='#stop' class='img-stop' onclick=\"UpdateServiceState('" + id + "', '" + state + "');return false;\" title='Stop Service' style='padding: 3px;'></a>";

            case "Stopped":
                return "<a href='#start' class='img-play' onclick=\"UpdateServiceState('" + id + "', '" + state + "');return false;\" title='Start Service' style='padding: 3px;'></a>";

            case "Error":
                return "<a href='#restart' class='img-refresh' onclick=\"UpdateServiceState('" + id + "', '" + state + "');return false;\" title='Restart Service' style='padding: 3px;'></a>";
        }
    }

    return "";
}