var _isAreaGraph = false;
var _lineWidth = 2;
var _curveType = 'none'; /*'function'*/

var graphWidth = $(window).width() - $("#sidebar-padding-trucksh").outerWidth() - 50;
var timeOut1, timeOut2;
$(document).ready(function () {
    ResizeSideBar();
});

$(window).resize(function () {
    ResizeSideBar();
});

function ResizeSideBar() {
    var h = $(window).height() - $(".app-title-bg-color").outerHeight();
    $(".content-overflow-app").css("height", h - 25);
    $(".sidebar-padding").css("height", h - 30);

    graphWidth = $(window).width() - $("#sidebar-padding-trucksh").outerWidth() - 50;
    BuildGraphs();
    NetworkMap();
    startGraphTimer1();
}

google.load("visualization", "1", { packages: ["corechart"] });
google.load("visualization", "1", { packages: ["geochart"] });
var runningBuildGraphs = false;
function BuildGraphs() {
    if (!runningBuildGraphs) {
        runningBuildGraphs = true;
        var width = graphWidth - 300;
        var height = 300;
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
                        BuildGraph2(data.d[2], width, height);
                    }
                    if (data.d[3] != null) {
                        BuildGraph3(data.d[3], width, height);
                    }
                    if (data.d[4] != null) {
                        document.getElementById("statholder-chart2").innerHTML = data.d[4];
                    }
                    if (data.d[5] != null) {
                        document.getElementById("statholder-chart3").innerHTML = data.d[5];
                    }
                    if (data.d[6] != null) {
                        if (data.d[6] != "") {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = data.d[6];
                        }
                        else {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = "Not information available";
                        }
                    }
                }
                runningBuildGraphs = false;
            }
        });
    }
}

function BuildRequests(data, _width, _height) {
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

function BuildGraph2(data, _width, _height) {
    var rows = data.length;
    var finalArray = new Array();
    finalArray.push(["Interval", "Speed"]);
    for (var i = 0; i < rows; i++) {
        finalArray.push([i.toString(), parseInt(data[i])]);
    }

    var _data = google.visualization.arrayToDataTable(finalArray);
    var options = {
        curveType: _curveType,
        title: 'Server Upload Speed',
        vAxis: { title: 'KB/s', minValue: 0 },
        hAxis: { title: 'Interval' },
        width: _width,
        height: _height,
        lineWidth: _lineWidth,
        series: { 0: { color: "#FF6A00" } },
        backgroundColor: "transparent"
    };

    if (_isAreaGraph) {
        var chart = new google.visualization.AreaChart(document.getElementById('ChartGraph2'));
        chart.draw(_data, options);
    }
    else {
        var chart = new google.visualization.LineChart(document.getElementById('ChartGraph2'));
        chart.draw(_data, options);
    }
}

function BuildGraph3(data, _width, _height) {
    var rows = data.length;
    var finalArray = new Array();
    finalArray.push(["Interval", "Speed"]);
    for (var i = 0; i < rows; i++) {
        finalArray.push([i.toString(), parseInt(data[i])]);
    }

    var _data = google.visualization.arrayToDataTable(finalArray);
    var options = {
        curveType: _curveType,
        title: 'Server Download Speed',
        vAxis: { title: 'KB/s', minValue: 0 },
        hAxis: { title: 'Interval' },
        width: _width,
        height: _height,
        lineWidth: _lineWidth,
        series: { 0: { color: "#555" } },
        backgroundColor: "transparent"
    };

    if (_isAreaGraph) {
        var chart = new google.visualization.AreaChart(document.getElementById('ChartGraph3'));
        chart.draw(_data, options);
    }
    else {
        var chart = new google.visualization.LineChart(document.getElementById('ChartGraph3'));
        chart.draw(_data, options);
    }
}

var runningNetworkMap = false;
function NetworkMap() {
    if (!runningNetworkMap) {
        runningNetworkMap = true;
        var networkInfo = $("#network-info-div").outerWidth() + 150;
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/NetworkLog.asmx/GetMap",
            data: "{ }",
            type: "POST",
            cache: false,
            contentType: "application/json",
            dataType: "json",
            success: function (data) {
                if (data != null) {
                    if (data.d[0] != null) {
                        if (document.getElementById("pnl_NetworkInfoHolder") != null) {
                            document.getElementById("pnl_NetworkInfoHolder").innerHTML = data.d[0];
                        }
                    }
                    if (data.d[1] != null) {
                        var rows = data.d[1].length;
                        var finalArray = new Array();
                        finalArray.push(["Country", "IP's Connected"]);
                        var count = 0;
                        for (var i = 0; i < rows; i++) {
                            var country = data.d[1][i][1];
                            count = 0;
                            for (var j = 0; j < rows; j++) {
                                if (country == data.d[1][j][1]) {
                                    count++;
                                }
                            }
                            finalArray.push([country, count]);
                        }

                        BuildIPLoc(data.d[1]);
                        var _data = google.visualization.arrayToDataTable(finalArray);

                        var options = {
                            width: graphWidth - networkInfo,
                            height: 400,
                            backgroundColor: "transparent"
                        };

                        var geomap = new google.visualization.GeoChart(document.getElementById('NetworkMap'));
                        geomap.draw(_data, options);
                    }
                }
                runningNetworkMap = false;
            }
        });
    }
}

var paused = false;
$(document.body).on("click", "#imgPausePlay", function () {
    if (paused == false) {
        paused = true;
        $(this).removeClass("img-pause");
        $(this).addClass("img-play");
        $(this).attr("title", "Resume");
        clearTimeout(timeOut1);
    }
    else {
        paused = false;
        $(this).removeClass("img-play");
        $(this).addClass("img-pause");
        $(this).attr("title", "Pause");
        startGraphTimer1();
    }

    return false;
});

var updateinterval = 5000;
$(document.body).on("change", "#dd_interval", function () {
    updateinterval = $("#dd_interval").val();
    clearTimeout(timeOut1);
    BuildGraphs();
    startGraphTimer1();
});


function startGraphTimer1() {
    timeOut1 = setTimeout(function () {
        if (!paused) {
            BuildGraphs();
            startGraphTimer1();
        }
    }, (updateinterval));
}