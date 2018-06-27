var _isAreaGraph = false;
var _lineWidth = 2;
var _curveType = 'none'; /*'function'*/

$(document).ready(function () {
    cookieFunctions.get("updateIntervalAnalytics", function (updateIntervalCookie) {
        if (updateIntervalCookie && !isNaN(updateIntervalCookie)) {
            $("#dd_interval").val(updateIntervalCookie);
            updateinterval = parseInt(updateIntervalCookie);
        }

        cookieFunctions.get("timeToTrackAnalytics", function (timeToTrackCookie) {
            if (timeToTrackCookie && !isNaN(timeToTrackCookie)) {
                $("#dd_track").val(timeToTrackCookie);
            }

            openWSE.GetScriptFunction("https://www.gstatic.com/charts/loader.js", function () {
                google.charts.load("visualization", "1", {
                    callback: function () {
                        InitializeGraph();
                        buildIPLocationMap();
                    },
                    mapsApiKey: $("#hf_GoogleMapsAPIKey").val(),
                    packages: ["corechart", "map"]
                });
            });

            $(".sitemenu-selection").find("li").on("click", function () {
                if ($(this).find("a").attr("href").indexOf("activity") > 0) {
                    $(window).resize();
                }
            });

            adjustHttpRefererHolders();
            RefreshAutoCompleteUsersToIgnore();
        });
    });

    $(".sitemenu-selection").find("a[href='#?tab=loginActivity']").on("click", function () {
        setTimeout(function () {
            buildIPLocationMap();
        }, openWSE_Config.animationSpeed);
    });
});

var currentMapIndexLookup = 0;
var mapData = new Array();
var mapTempData = new Array();
var mapIPList = new Array();
function buildIPLocationMap() {
    if ($("#hf_GoogleMapsAPIKey").val()) {
        $("#loginmap_div_error").hide();

        if (mapData.length > 0) {
            finishBuildingLoginMap();
        }
        else if (mapTempData.length === 0) {
            mapTempData = new Array();
            mapTempData.push(["Lat", "Long", "Name"]);

            currentMapIndexLookup = 0;
            mapIPList = new Array();
            $("#MainContent_pnl_loginactivity").find(".ipsearchval").each(function () {
                var ipVal = $.trim($(this).html());
                var foundItem = false;
                for (var i = 0; i < mapIPList.length; i++) {
                    if (mapIPList[i] === ipVal) {
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem) {
                    mapIPList.push(ipVal);
                }
            });

            getIPGeoLocation();
        }
    }
    else {
        $("#loginmap_showhide").hide();
        $("#loginmap_div").hide();
        $("#loginmap_div_loading").hide();
        $("#loginmap_div_error").show();
    }
}
function getIPGeoLocation() {
    if (currentMapIndexLookup < mapIPList.length) {
        var url = location.protocol + "//freegeoip.net/json/" + mapIPList[currentMapIndexLookup];

        var xhr = createCORSRequest("GET", url);
        if (!xhr) {
            openWSE.LogConsoleMessage("CORS not supported");
        }
        else {
            xhr.onload = function (response) {
                try {
                    var data = JSON.parse(response.target.responseText);
                    if (data.latitude && data.longitude) {
                        var name = data.ip;
                        if (data.city) {
                            name += " | " + data.city;
                        }
                        if (data.region_name) {
                            name += " | " + data.region_name;
                        }
                        if (data.zip_code) {
                            name += " | " + data.zip_code;
                        }
                        if (data.country_name) {
                            name += " | " + data.country_name;
                        }

                        mapTempData.push([data.latitude, data.longitude, name]);
                    }
                }
                catch (evt) { }
                updateMapIndexLookup();
            };
            xhr.onerror = function () {
                updateMapIndexLookup();
            };
            xhr.send();
        }
    }
    else {
        mapData = mapTempData;
        mapTempData = new Array();
        finishBuildingLoginMap();
    }
}
function updateMapIndexLookup() {
    setTimeout(function () {
        currentMapIndexLookup++;
        getIPGeoLocation();
    }, 100);
}
function finishBuildingLoginMap() {
    $("#loginmap_div").show();
    $("#loginmap_div_loading").hide();
    $("#loginmap_div_error").hide();

    $("#loginmap_div").html("");
    var map = new google.visualization.Map(document.getElementById("loginmap_div"));
    var options = {
        zoomLevel: 2,
        mapType: "normal",
        showTooltip: false,
        showInfoWindow: true,
        useMapTypeControl: false,
    };

    if (mapData && mapData.length > 1) {
        var builtData = google.visualization.arrayToDataTable(mapData);
        map.draw(builtData, options);
    }
}
function createCORSRequest(method, url) {
    var xhr = new XMLHttpRequest();
    if ("withCredentials" in xhr) {
        // XHR for Chrome/Firefox/Opera/Safari.
        xhr.open(method, url, true);
    } else if (typeof XDomainRequest != "undefined") {
        // XDomainRequest for IE.
        xhr = new XDomainRequest();
        xhr.open(method, url);
    } else {
        // CORS not supported.
        xhr = null;
    }
    return xhr;
}

function ConfirmClearAllLoginActivity(_this) {
    openWSE.ConfirmWindow("Are you sure you want to clear the login activity?",
        function () {
            loadingPopup.Message('Clearing. Please Wait...');
            var id = $(_this).attr("id");
            openWSE.CallDoPostBack(id, "");
        }, null);

    return false;
}

function DeleteLoginActivity(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this login event?",
        function () {
            loadingPopup.Message("Deleting...");
            $("#MainContent_hf_DeleteLoginEvent").val(id);
            openWSE.CallDoPostBack("MainContent_hf_DeleteLoginEvent", "");
        }, null);
}

function AllowBlockLoginEvent(id) {
    loadingPopup.Message("Updating...");
    $("#MainContent_hf_AllowBlockLoginEvent").val(id);
    openWSE.CallDoPostBack("MainContent_hf_AllowBlockLoginEvent", "");
}

function ResetPageViewCount(pageName) {
    openWSE.ConfirmWindow("Are you sure you want to reset this page count?",
        function () {
            loadingPopup.Message("Updating. Please Wait...");
            $("#hf_resetPageCount").val(pageName);
            openWSE.CallDoPostBack("hf_resetPageCount", "");
        }, null);
}

function LoadCommonStatisticsModal(eleId) {
    loadingPopup.Message("Loading. Please Wait...");
    openWSE.AjaxCall("WebServices/NetworkLog.asmx/GetCommonStatistics", "{ 'div': '" + eleId + "' }", {
        cache: false
    }, function (data) {
        loadingPopup.RemoveMessage();

        if (data != null && data.d != null) {
            var ele = "<div id='CommonStatistics-element' class='Modal-element' style='display: none;'>";
            ele += "<div class='Modal-overlay'>";
            ele += "<div class='Modal-element-align'>";
            ele += "<div class='Modal-element-modal' data-setwidth='650'>";

            // Header
            ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
            ele += "<a href='#' onclick=\"openWSE.LoadModalWindow(false, 'CommonStatistics-element', ''); $('#CommonStatistics-element').remove();return false;\" class='ModalExitButton'></a>";
            ele += "</div><span class='Modal-title'></span></div></div>";

            var detailStr = "<div id='commonstatistics-view-detailstable'>";
            detailStr += "<table class='gridview-table' style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>";
            detailStr += "<tr class='myHeaderStyle'>";

            if (data.d[2] === "true") {
                detailStr += "<td width='45px' style='padding-left: 10px;'>#</td>";
            }

            if (eleId == "div_NewVisitors") {
                detailStr += "<td>IP Address</td><td style='width: 160px;'>Date</td></tr>";
            }
            else if (eleId == "div_RecentLogins") {
                detailStr += "<td>IP Address</td><td>Username</td><td>Activity Type</td><td style='width: 160px;'>Date</td></tr>";
            }
            else if (eleId == "div_RegisteredUsers") {
                detailStr += "<td>Username</td><td>Is Online</td><td>Email</td><td style='width: 160px;'>Date</td></tr>";
            }

            if (data.d[0].length > 0) {
                for (var i = 0; i < data.d[0].length; i++) {
                    var rowClass = "GridNormalRow";
                    if (i % 2 !== 0 && data.d[1] === "true") {
                        rowClass = "GridAlternate";
                    }

                    detailStr += "<tr class='myItemStyle " + rowClass + "'>";
                    if (data.d[2] === "true") {
                        detailStr += "<td class='GridViewNumRow border-bottom'>" + (i + 1).toString() + "</td>";
                    }

                    if (eleId == "div_NewVisitors") {
                        if (data.d[0][i][0] !== "127.0.0.1") {
                            detailStr += "<td class='border-bottom' align='left'><a href='#' onclick=\"ipSearch='';SearchLoginIp('" + data.d[0][i][0] + "');return false;\" style='opacity: 1.0!important; filter: alpha(opacity=100)!important;'>" + data.d[0][i][0] + "</a></td>";
                        }
                        else {
                            detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][0] + "</td>";
                        }
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][1] + "</td>";
                    }
                    else if (eleId == "div_RecentLogins") {
                        if (data.d[0][i][0] !== "127.0.0.1") {
                            detailStr += "<td class='border-bottom' align='left'><a href='#' onclick=\"ipSearch='';SearchLoginIp('" + data.d[0][i][0] + "');return false;\" style='opacity: 1.0!important; filter: alpha(opacity=100)!important;'>" + data.d[0][i][0] + "</a></td>";
                        }
                        else {
                            detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][0] + "</td>";
                        }
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][1] + "</td>";
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][2] + "</td>";
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][3] + "</td>";
                    }
                    else if (eleId == "div_RegisteredUsers") {
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][0] + "</td>";
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][1] + "</td>";
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][2] + "</td>";
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][3] + "</td>";
                    }

                    detailStr += "</tr>";
                }

                detailStr += "</tbody></table></div>";
            }
            else {
                detailStr += "</tbody></table></div>";
                detailStr += "<div class='emptyGridView'>No details available</div>";
            }

            detailStr += "<div class='clear-space'></div>";

            // Body
            var cancelButton = "<input class='input-buttons float-right no-margin' type='button' value='Close' onclick=\"openWSE.LoadModalWindow(false, 'CommonStatistics-element', ''); $('#CommonStatistics-element').remove();\" />";
            ele += "<div class='ModalScrollContent'><div class='ModalPadContent'>" + detailStr + "</div></div><div class='ModalButtonHolder'>" + cancelButton + "<div class='clear'></div></div>";
            ele += "</div></div></div></div>";

            $("body").append(ele);
            openWSE.LoadModalWindow(true, "CommonStatistics-element", $.trim($("#" + eleId).find(".common-stats-title").html()));
        }
        else {
            openWSE.AlertWindow("Could not load the details for this page.");
        }
    });
}

function ViewPageDetails(pageName, realPageName) {
    loadingPopup.Message("Loading. Please Wait...");
    openWSE.AjaxCall("WebServices/NetworkLog.asmx/GetPageDetails", "{ 'pageName': '" + pageName + "' }", {
        cache: false
    }, function (data) {
        loadingPopup.RemoveMessage();

        if (data != null && data.d != null) {
            var ele = "<div id='PageDetails-element' class='Modal-element' style='display: none;'>";
            ele += "<div class='Modal-overlay'>";
            ele += "<div class='Modal-element-align'>";
            ele += "<div class='Modal-element-modal' data-setwidth='650'>";

            // Header
            ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
            ele += "<a href='#' onclick=\"openWSE.LoadModalWindow(false, 'PageDetails-element', ''); $('#PageDetails-element').remove();return false;\" class='ModalExitButton'></a>";
            ele += "</div><span class='Modal-title'></span></div></div>";

            var detailStr = "<div id='page-view-detailstable'>The table below shows the details of where each view came from. You can delete individual entries.<div class='clear-space'></div>";
            detailStr += "<table class='gridview-table' style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>";
            detailStr += "<tr class='myHeaderStyle'>";

            if (data.d[2] === "true") {
                detailStr += "<td width='45px' style='padding-left: 10px;'>#</td>";
            }

            detailStr += "<td width='125px'>IP Address</td><td>Username</td><td width='150px'>Date</td><td class='edit-column-1-items'></td></tr>";

            if (data.d[0].length > 0) {
                for (var i = 0; i < data.d[0].length; i++) {
                    var rowClass = "GridNormalRow";
                    if (i % 2 !== 0 && data.d[1] === "true") {
                        rowClass = "GridAlternate";
                    }

                    detailStr += "<tr class='myItemStyle " + rowClass + "' data-detailsid='" + data.d[0][i][0] + "'>";
                    if (data.d[2] === "true") {
                        detailStr += "<td class='GridViewNumRow border-bottom'>" + (i + 1).toString() + "</td>";
                    }

                    if (data.d[0][i][1] !== "127.0.0.1") {
                        detailStr += "<td class='border-bottom' align='left'><a href='#' onclick=\"ipSearch='';SearchLoginIp('" + data.d[0][i][1] + "');return false;\" style='opacity: 1.0!important; filter: alpha(opacity=100)!important;'>" + data.d[0][i][1] + "</a></td>";
                    }
                    else {
                        detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][1] + "</td>";
                    }

                    detailStr += "<td class='border-bottom' align='left'>" + data.d[0][i][2] + "</td>";
                    detailStr += "<td class=' border-bottom' align='left'>" + data.d[0][i][3] + "</td>";
                    detailStr += "<td class='border-bottom myItemStyle-action-btns' align='center'><a href=\"javascript:void(0);\" class='td-delete-btn' title='Delete Item' onclick=\"DeletePageViewDetailItem('" + data.d[0][i][0] + "', '" + escape(pageName) + "');return false;\"></a></td>";
                    detailStr += "</tr>";
                }

                detailStr += "</tbody></table></div>";
            }
            else {
                detailStr += "</tbody></table></div>";
                detailStr += "<div class='emptyGridView'>No details available</div>";
            }

            detailStr += "<div class='clear-space'></div>";

            // Body
            var cancelButton = "<input class='input-buttons float-right no-margin' type='button' value='Close' onclick=\"openWSE.LoadModalWindow(false, 'PageDetails-element', ''); $('#PageDetails-element').remove();\" />";
            ele += "<div class='ModalScrollContent'><div class='ModalPadContent'>" + detailStr + "</div></div><div class='ModalButtonHolder'>" + cancelButton + "<div class='clear'></div></div>";
            ele += "</div></div></div></div>";

            $("body").append(ele);
            openWSE.LoadModalWindow(true, "PageDetails-element", unescape(realPageName).replace(/\+/g, " ") + " Details");
        }
        else {
            openWSE.AlertWindow("Could not load the details for this page.");
        }
    });
}

function DeletePageViewDetailItem(id, pageName) {
    openWSE.ConfirmWindow("Are you sure you want to delete this item?",
     function () {
         loadingPopup.Message("Deleting. Please Wait...");
         openWSE.AjaxCall("WebServices/NetworkLog.asmx/DeletePageDetailsItem", "{ 'id': '" + id + "' }", {
             cache: false
         }, function (data) {
             loadingPopup.RemoveMessage();

             if (data.d == "true") {
                 var tempName = unescape(pageName);
                 $(".myItemStyle[data-detailsid='" + id + "']").remove();
                 var innerCount = $.trim($(".page-view-count[data-pageviewcount='" + tempName + "']").html());
                 if (innerCount) {
                     innerCount = parseInt(innerCount) - 1;
                     if (innerCount < 0) {
                         innerCount = 0;
                     }

                     $(".page-view-count[data-pageviewcount='" + tempName + "']").html(innerCount);
                 }

                 if ($("#page-view-detailstable").find(".myItemStyle").length === 0) {
                     $("#page-view-detailstable").append("<div class='emptyGridView'>No details available</div>");
                 }
             }
         });
     }, null);
}

function DeleteUserToIgnore(username) {
    openWSE.ConfirmWindow("Are you sure you want to delete this user from the ignore list?",
     function () {
         loadingPopup.Message("Updating. Please Wait...");
         $("#hf_DeleteUserToIgnore").val(username);
         openWSE.CallDoPostBack("hf_DeleteUserToIgnore", "");
     }, null);
}

$(window).resize(function () {
    setTimeout(function () {
        FinishBuildingGraph();
    }, 100);
});

var runningGraph = false;
function BuildGraphs() {
    setTimeout(function () {
        if (!runningGraph) {
            runningGraph = true;

            openWSE.AjaxCall("WebServices/NetworkLog.asmx/GetGraphs", "{ '_dataInt': '" + $("#dd_track").val() + "' }", {
                cache: false
            }, function (data) {
                if (data != null) {
                    if (data.d[0] != null) {
                        BuildRequests(data.d[0]);
                    }
                    if (data.d[1] != null) {
                        if (data.d[1] != "") {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = data.d[1];
                        }
                        else {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = "Not information available";
                        }
                    }
                }

                runningGraph = false;
                if (!paused) {
                    BuildGraphs();
                }
            });
        }
    }, (updateinterval));
}
function InitializeGraph() {
    runningGraph = true;

    openWSE.AjaxCall("WebServices/NetworkLog.asmx/GetGraphs", "{ '_dataInt': '" + $("#dd_track").val() + "' }", {
        cache: false
    }, function (data) {
        if (data != null) {
            if (data.d[0] != null) {
                BuildRequests(data.d[0]);
            }
            if (data.d[1] != null) {
                if (data.d[1] != "") {
                    document.getElementById("pnl_NetworkInfoHolder").innerHTML = data.d[1];
                }
                else {
                    document.getElementById("pnl_NetworkInfoHolder").innerHTML = "Not information available";
                }
            }
        }

        runningGraph = false;
        if (!paused) {
            BuildGraphs();
        }
    });
}

var finalArray = new Array();
function BuildRequests(data) {
    if ($("#ChartRequests").css("display") != "none") {
        try {
            var rows = data.length;
            finalArray = new Array();
            finalArray.push(["Interval", "Requests"]);
            for (var i = 0; i < rows; i++) {
                finalArray.push([i.toString(), parseInt(data[i])]);
            }

            FinishBuildingGraph();
        }
        catch (evt) { }
    }
}
function FinishBuildingGraph() {
    try {
        if (finalArray.length > 0) {
            var _data = google.visualization.arrayToDataTable(finalArray);

            var chartWidth = 0;
            if ($(window).width() > 1000) {
                $("#network-info-div").find(".network-info-holder").each(function () {
                    chartWidth += $(this).outerWidth();
                });
            }
            else {
                chartWidth = $("#main_container").outerWidth() - 85;
            }

            var chartHeight = 325;

            var options = {
                curveType: _curveType,
                title: 'Site Requests',
                vAxis: {
                    title: '# Requests',
                    minValue: 0,
                    textStyle: {
                        bold: true,
                        fontSize: 12,
                        color: '#848484'
                    },
                    titleTextStyle: {
                        bold: true,
                        fontSize: 12,
                        color: '#848484'
                    }
                },
                hAxis: {
                    title: 'Interval',
                    textStyle: {
                        bold: true,
                        fontSize: 12,
                        color: '#848484'
                    },
                    titleTextStyle: {
                        bold: true,
                        fontSize: 12,
                        color: '#848484'
                    }
                },
                width: chartWidth,
                height: chartHeight,
                chartArea: {
                    width: "80%",
                    height: "50%"
                },
                lineWidth: _lineWidth,
                backgroundColor: "transparent",
                legend: { position: "none" }
            };

            if (chartWidth > 0) {
                if (_isAreaGraph) {
                    var chart = new google.visualization.AreaChart(document.getElementById('ChartRequests'));
                    chart.draw(_data, options);
                }
                else {
                    var chart = new google.visualization.LineChart(document.getElementById('ChartRequests'));
                    chart.draw(_data, options);
                }
            }
        }
    }
    catch (evt) {
    }
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
    updateinterval = parseInt($("#dd_interval").val());
    cookieFunctions.set("updateIntervalAnalytics", $("#dd_interval").val(), "30");
});

$(document.body).on("change", "#dd_track", function () {
    cookieFunctions.set("timeToTrackAnalytics", $("#dd_track").val(), "30");
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    adjustHttpRefererHolders();
    RefreshAutoCompleteUsersToIgnore();

    mapData = new Array();
    buildIPLocationMap();
});

function RefreshAutoCompleteUsersToIgnore() {
    $(".users-to-ignore-tb").autocomplete({
        minLength: 0,
        autoFocus: true,
        source: function (request, response) {
            openWSE.AjaxCall("WebServices/AutoComplete.asmx/GetListOfUsersWithAdminIncluded", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                dataFilter: function (data) { return data; }
            }, function (data) {
                response($.map(data.d, function (item) {
                    return {
                        label: item,
                        value: item
                    }
                }));
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    $(".usertoignore-item").find(".users-to-ignore-tb").each(function () {
        $(this).val(unescape($(this).val()).replace(/\+/g, " "));
    });
}

function SearchLoginIp(ip) {
    if (ip) {
        CloseSearchLoginIP();

        loadingPopup.Message("Updating...");

        var url = location.protocol + "//freegeoip.net/json/" + ip;

        var xhrSearch = createCORSRequest("GET", url);
        if (!xhrSearch) {
            SearchLoginIPFallback(ip);
        }
        else {
            xhrSearch.onload = function (response) {
                loadingPopup.RemoveMessage();
                try {
                    var data = JSON.parse(response.target.responseText);
                    var htmlMessage = "";
                    if (data.ip) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>IP Address:</span>" + data.ip + "<div class='clear-space-five'></div>";
                    }
                    if (data.city) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>City:</span>" + data.city + "<div class='clear-space-five'></div>";
                    }
                    if (data.country_code) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Country Code:</span>" + data.country_code + "<div class='clear-space-five'></div>";
                    }
                    if (data.country_name) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Country Name:</span>" + data.country_name + "<div class='clear-space-five'></div>";
                    }
                    if (data.latitude) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Latitude:</span>" + data.latitude + "<div class='clear-space-five'></div>";
                    }
                    if (data.longitude) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Longitude:</span>" + data.longitude + "<div class='clear-space-five'></div>";
                    }
                    if (data.region_name) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Region Name:</span>" + data.region_name + "<div class='clear-space-five'></div>";
                    }
                    if (data.time_zone) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Time Zone:</span>" + data.time_zone + "<div class='clear-space-five'></div>";
                    }
                    if (data.zip_code) {
                        htmlMessage += "<span class='font-bold pad-right-sml'>Zip Code:</span>" + data.zip_code + "<div class='clear-space-five'></div>";
                    }

                    if (htmlMessage) {
                        var ele = "<div id='ipsearch-element' class='Modal-element' style='display: none;'>";
                        ele += "<div class='Modal-overlay'>";
                        ele += "<div class='Modal-element-align'>";
                        ele += "<div class='Modal-element-modal'>";

                        // Header
                        ele += "<div class='ModalHeader'><div><div class='app-head-button-holder-admin'>";
                        ele += "<a href='#' onclick=\"CloseSearchLoginIP();return false;\" class='ModalExitButton'></a>";
                        ele += "</div><span class='Modal-title'></span></div></div>";

                        // Body

                        ele += "<div class='ModalScrollContent'><div class='ModalPadContent'><div style='white-space: nowrap;'>" + htmlMessage + "</div></div></div>";
                        ele += "</div></div></div></div>";

                        $("body").append(ele);
                        openWSE.LoadModalWindow(true, "ipsearch-element", "IP Search");
                        $(window).resize();
                    }
                    else {
                        SearchLoginIPFallback(ip);
                    }
                }
                catch (evt) {
                    SearchLoginIPFallback(ip);
                }
            };
            xhrSearch.onerror = function () {
                SearchLoginIPFallback(ip);
            };
            xhrSearch.send();
        }
    }
}
function CloseSearchLoginIP() {
    openWSE.LoadModalWindow(false, "ipsearch-element", "");
    $("#ipsearch-element").remove();
}
function SearchLoginIPFallback(ip) {
    loadingPopup.RemoveMessage();
    window.open("http://whatismyipaddress.com/ip/" + ip);
}

$(document.body).on("keypress", "#MainContent_tb_daystokeepLoginActivity", function (e) {
    var code = (e.which) ? e.which : e.keyCode;
    var val = String.fromCharCode(code);

    if (val != "0" && val != "1" && val != "2" && val != "3" && val != "4" && val != "5" && val != "6" && val != "7" && val != "8" && val != "9") {
        return false;
    }
});

function adjustHttpRefererHolders() {
    $(".http-referer-text").each(function () {
        var text = $.trim($(this).html()).toString();
        if (!text || text.toLowerCase() == "unknown") {
            $(this).parent().hide();
        }
    });
}
