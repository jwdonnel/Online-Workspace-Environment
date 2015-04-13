var graphWidth = $("#maincontent_overflow").outerWidth();


var _isAreaGraph = false;
var _lineWidth = 2;
var _curveType = 'none'; /*'function'*/

$(document).ready(function () {
    $.getScript("//www.google.com/jsapi").done(function (script, textStatus) {
        google.load("visualization", "1", { callback: function () { }, packages: ["corechart"] });
    });

    openWSE.RadioButtonStyle();
    BuildGraphs();

    var url = location.href;
    load(url == "" ? "1" : url);

    $("#MainContent_tb_search").autocomplete({
        minLength: 0,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetListOfAppLogEvents",
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
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});

function ConfirmClearLogAll(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear all errors?",
        function () {
            openWSE.LoadingMessage1('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
        }, null);

    return false;
}

function ConfirmClearLogFolder(_this) {
    openWSE.ConfirmWindow("Are you sure you want to delete all error log files in the Logging folder?",
        function () {
            openWSE.LoadingMessage1('Deleting. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
        }, null);

    return false;
}

function ConfirmClearAllIgnored(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear the ignored errors?",
        function () {
            openWSE.LoadingMessage1('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
        }, null);

    return false;
}

function ConfirmClearAllLoginActivity(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear the login activity?",
        function () {
            openWSE.LoadingMessage1('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            __doPostBack(id, "");
        }, null);

    return false;
}

$(document.body).on("click", ".searchbox_clear", function () {
    openWSE.LoadingMessage1("Updating...");
    $('#hf_searchreset').val(new Date().toString());
    __doPostBack("hf_searchreset", "");
});

$(document.body).on("click", "#btn_createnew_listener", function () {
    AddIPToListener();
});

$(document.body).on("change", "#cb_ViewErrorsOnly", function () {
    openWSE.LoadingMessage1('Updating. Please Wait...');
});

$(document.body).on("keydown", "#tb_createnew_listener", function (e) {
    if (event.which == 13 || event.keyCode == 13) {
        AddIPToListener();
        e.preventDefault();
    }
});

function AddIPToListener() {
    openWSE.LoadingMessage1("Updating...");
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/AddIP",
        type: "POST",
        data: '{ "ip": "' + $.trim($("#tb_createnew_listener").val()) + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#listener_postmessage").html("");
                $("#hf_UpdateIPListener").val(new Date().toString());
                __doPostBack("hf_UpdateIPListener", "");
            }
            else if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#listener_postmessage").html("<small style='color: red'>IP Address is invalid</small>");
                openWSE.RemoveUpdateModal();
            }
            else if (data.d == "duplicate") {
                $("#listener_postmessage").html("<small style='color: red'>Duplicate ip address</small>");
                openWSE.RemoveUpdateModal();
            }
            else {
                $("#listener_postmessage").html("<small style='color: red'>Error trying to add ip</small>");
                openWSE.RemoveUpdateModal();
            }
            window.setTimeout(function () { $("#listener_postmessage").html(""); }, 4000);
        },
        error: function () {
            openWSE.AlertWindow("There was an error adding ip. Please try again.");
        }
    });
}

function SetToCurrentIP(ip) {
    $("#tb_createnew_listener").val(ip);
}

function DeleteLoginActivity(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this login event?",
        function () {
            openWSE.LoadingMessage1("Deleting...");
            $("#MainContent_hf_DeleteLoginEvent").val(id);
            __doPostBack("MainContent_hf_DeleteLoginEvent", "");
        }, null);
}

function AllowBlockLoginEvent(id) {
    openWSE.LoadingMessage1("Updating...");
    $("#MainContent_hf_AllowBlockLoginEvent").val(id);
    __doPostBack("MainContent_hf_AllowBlockLoginEvent", "");
}

function ResetHitCount(id) {
    openWSE.ConfirmWindow("Are you sure you want to reset the hit count?",
        function () {
            openWSE.LoadingMessage1("Resetting...");
            $("#MainContent_hf_resetHitCount").val(id);
            __doPostBack("MainContent_hf_resetHitCount", "");
        }, null);
}

function DeleteIP(ip) {
    openWSE.ConfirmWindow("Are you sure you want to delete this IP?",
        function () {
            openWSE.LoadingMessage1("Deleting IP. Please Wait...");
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/DeleteIP",
                type: "POST",
                data: '{ "ip": "' + ip + '" }',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (openWSE.ConvertBitToBoolean(data.d)) {
                        $("#listener_postmessage").html("<small style='color: green'>IP Address has been deleted</small>");
                        $("#hf_UpdateIPListener").val(new Date().toString());
                        __doPostBack("hf_UpdateIPListener", "");
                    }
                    else {
                        $("#listener_postmessage").html("<small style='color: red'>Error Deleting IP</small>");
                        openWSE.RemoveUpdateModal();
                    }
                    window.setTimeout(function () { $("#listener_postmessage").html(""); }, 4000);
                },
                error: function () {
                    openWSE.AlertWindow("There was an error deleting ip. Please try again.");
                }
            });
        }, null);
}

var __this;
function UpdateActive(ip, active, _this) {
    __this = _this;
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateServerSettings.asmx/UpdateIP",
        type: "POST",
        data: '{ "ip": "' + ip + '","activeIP": "' + active + '"  }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (openWSE.ConvertBitToBoolean(data.d)) {
                $("#hf_UpdateIPListener").val(new Date().toString());
                __doPostBack("hf_UpdateIPListener", "");
            }
            else if (data.d == "sameip") {
                $("#listener_postmessage").html("<small style='color: red'>Cannot disable your own IP address when more than one are active</small>");
                openWSE.RemoveUpdateModal();
                ResetStatus(__this);
                window.setTimeout(function () { $("#listener_postmessage").html(""); }, 4000);
            }
            else if (data.d == "differentip") {
                $("#listener_postmessage").html("<small style='color: red'>Cannot Enable another IP address until your current IP address is.</small>");
                openWSE.RemoveUpdateModal();
                ResetStatus(__this);
                window.setTimeout(function () { $("#listener_postmessage").html(""); }, 4000);
            }
            else {
                openWSE.RemoveUpdateModal();
            }
        },
        error: function () {
            openWSE.AlertWindow("There was an error updating ip. Please try again.");
        }
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

$(window).resize(function () {
    graphWidth = $("#maincontent_overflow").outerWidth();
    BuildGraphs();
});

var runningGraph = false;
function BuildGraphs() {
    var width = graphWidth - 500;
    var height = 300;
    if (!runningGraph) {
        runningGraph = true;
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/NetworkLog.asmx/GetGraphs",
            data: "{ '_dataInt': '" + $("#dd_track").val() + "' }",
            type: "POST",
            cache: false,
            contentType: "application/json",
            dataType: "json",
            success: function (data) {
                if (data != null) {
                    if (data.d[0] != null) {
                        BuildRequests(data.d[0], width, height);
                    }
                    if (data.d[1] != null) {
                        document.getElementById("statholder").innerHTML = data.d[1];
                    }
                    if (data.d[2] != null) {
                        if (data.d[2] != "") {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = data.d[2];
                        }
                        else {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = "Not information available";
                        }
                    }
                }

                runningGraph = false;
                setTimeout(function () {
                    if (!paused) {
                        BuildGraphs();
                    }
                }, (updateinterval));
            }
        });
    }
}

function BuildRequests(data, _width, _height) {
    try {
        var rows = data.length;
        var finalArray = new Array();
        finalArray.push(["Interval", "Requests"]);
        for (var i = 0; i < rows; i++) {
            finalArray.push([i.toString(), parseInt(data[i])]);
        }

        var _data = google.visualization.arrayToDataTable(finalArray);
        var options = {
            curveType: _curveType,
            title: 'Site Requests',
            vAxis: { title: '# Requests', minValue: 0 },
            hAxis: { title: 'Interval' },
            width: _width,
            height: _height,
            lineWidth: _lineWidth,
            backgroundColor: "transparent"
        };

        if (_isAreaGraph) {
            var chart = new google.visualization.AreaChart(document.getElementById('ChartRequests'));
            chart.draw(_data, options);
        }
        else {
            var chart = new google.visualization.LineChart(document.getElementById('ChartRequests'));
            chart.draw(_data, options);
        }
    }
    catch (evt) { }
}

var paused = false;
$(document.body).on("click", "#imgPausePlay", function () {
    if (paused == false) {
        paused = true;
        $(this).removeClass("img-pause");
        $(this).addClass("img-play");
        $(this).attr("title", "Resume");
    }
    else {
        paused = false;
        $(this).removeClass("img-play");
        $(this).addClass("img-pause");
        $(this).attr("title", "Pause");
        BuildGraphs();
    }

    return false;
});

var updateinterval = 5000;
$(document.body).on("change", "#dd_interval", function () {
    updateinterval = $("#dd_interval").val();
});


var currentTab = "";
var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    openWSE.RadioButtonStyle();
    load(currentTab);
    SearchLoginIp(ipSearch);

    $("#MainContent_tb_search").autocomplete({
        minLength: 0,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetListOfAppLogEvents",
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
                        }
                    }))
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});

function IgnoreError(id) {
    openWSE.ConfirmWindow("Are you sure you want to ignore this event?",
        function () {
            openWSE.LoadingMessage1("Updating Event...");
            $("#MainContent_hf_updateIgnore").val(id);
            __doPostBack("MainContent_hf_updateIgnore", "");
        }, null);
}

function RefreshPageOnError(id) {
    $("#MainContent_hf_updateRefreshOnError").val(id);
    __doPostBack("MainContent_hf_updateRefreshOnError", "");
}

function AllowError(id) {
    openWSE.ConfirmWindow("Are you sure you want to allow this event?",
        function () {
            openWSE.LoadingMessage1("Updating Event...");
            $("#MainContent_hf_updateAllow").val(id);
            __doPostBack("MainContent_hf_updateAllow", "");
        }, null);
}

function DeleteEvent(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this event?",
        function () {
            openWSE.LoadingMessage1("Deleting Event...");
            $("#MainContent_hf_deleteError").val(id);
            __doPostBack("MainContent_hf_deleteError", "");
        }, null);
}

$(function () {
    $(".sitemenu-selection").find("li").find("a").on("click", function () {
        load($(this).attr("href"));
        return false;
    });
});

function load(num) {
    paused = true;
    currentTab = num;

    $("#search_box").hide();
    $(".pnl-section").hide();
    $(".sitemenu-selection").find("li").removeClass("active");

    var index = 0;

    var arg1 = num.split("?tab=");
    if (arg1.length > 1) {
        var arg2 = arg1[1].split("#");
        if (arg2.length == 1) {
            index = GetPnlSectionIndex(arg2[0]);
        }
    }

    $(".pnl-section").eq(index).show();
    $(".sitemenu-selection").find("li").eq(index).addClass("active");

    switch (index) {
        case 0:
            BuildGraphs();
            paused = false;
            break;

        case 1:
        case 2:
            $("#search_box").show();
            break;
    }
}

function GetPnlSectionIndex(ele) {
    var pnlIndex = 0;
    $(".pnl-section").each(function (index) {
        if ($(this).attr("id") == ele) {
            pnlIndex = index;
        }
    });

    return pnlIndex;
}

var ipSearch = "";
function SearchLoginIp(ip) {
    ipSearch = ip;
    ShowAllLoginSearchRows();
    $("#loginactivitysearch").hide();
    $("#clearloginsearch").show();

    if (ipSearch != "") {
        var count = 0;
        $(".ipsearchrow").each(function (index) {
            var rowVal = $.trim($(this).find(".ipsearchval").html());
            if (rowVal != ipSearch) {
                $(this).closest("tr").hide();
            }
            else {
                count++;
                $(this).find(".GridViewNumRow").html(count);
            }
        });

        if (count == 0) {
            $("#loginactivitysearch").show();
        }
    }
    else {
        $("#clearloginsearch").hide();
    }
}

function ShowAllLoginSearchRows() {
    $(".ipsearchrow").each(function (index) {
        $(this).closest("tr").show();
        $(this).find(".GridViewNumRow").html(index + 1);
    });
}

function ClearIpSearch() {
    ipSearch = "";
    ShowAllLoginSearchRows();
    $("#clearloginsearch, #loginactivitysearch").hide();
}