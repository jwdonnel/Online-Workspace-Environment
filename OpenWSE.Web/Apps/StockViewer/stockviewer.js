var stockviewerApp = function () {
    var totalRows = 1;
    var minWidth = 750;
    var tickers = new Array();
    var loaderHolder = "#stockviewer-load";

    function Initialize() {
        openWSE.AjaxCall("Apps/StockViewer/StockViewerService.asmx/GetTickers", '', null, function (data) {
            if (data.d) {
                var tempTickers = new Array();
                for (var i = 0; i < data.d.length; i++) {
                    var jsonStr = unescape(data.d[i]);
                    tempTickers.push(JSON.parse(jsonStr));
                }

                tempTickers = sortTickers(tempTickers);
                if (tickers !== tempTickers) {
                    tickers = tempTickers;
                }
            }

            buildTable();
        });
    }
    function ResizeTable() {
        var ht = $("#stockviewer-load").outerHeight() - $("#stockviewer-load").find(".app-title-bg-color").outerHeight();
        $("#stockviewer_holder").css("height", ht + "px");

        var widthStyle = "";
        if ($("#stockviewer_holder").outerWidth() <= minWidth) {
            widthStyle = "100%";
        }

        $(".stockviewer-div").each(function () {
            $(this).css("width", widthStyle);
        });
    }

    function buildTable() {
        tickers = sortTickers(tickers);

        var widthStyle = "";
        if ($("#stockviewer_holder").outerWidth() <= minWidth) {
            widthStyle = " style='width: 100%;'";
        }

        var str = "";
        for (var i = 0; i < tickers.length; i++) {
            str += "<div class='stockviewer-div' data-id='" + tickers[i].id + "'" + widthStyle + ">" + buildStockViewerIframe(tickers[i]) + "</div>";
        }

        if (str === "") {
            str = "<h3 class='pad-all'>No views added</h3>";
        }

        $("#stockviewer_holder").html(str + "<div class='clear'></div>");

        if ($("#stockviewer_holder").find(".stockviewer-iframe").length > 0) {
            loadingPopup.Message("Loading...", loaderHolder);
            $("#stockviewer_holder").find(".stockviewer-iframe").one("load", function () {
                loadingPopup.RemoveMessage(loaderHolder);
            });
        }

        if (tickers.length === 1) {
            $("#stockviewer_holder").find(".stockviewer-div").addClass("only-one");
        }
        else if (tickers.length === 2) {
            $("#stockviewer_holder").find(".stockviewer-div").addClass("only-two");
        }

        $("#stockviewer_holder").sortable({
            items: ".stockviewer-div",
            cancel: ".stockviewer-toolbar-name, .stockviewer-toolbar-edit, stockviewer-toolbar-close, .stockviewer-iframe-holder",
            containment: "parent",
            tolerance: "pointer",
            start: function (event, ui) {
                ui.item.each(function () {
                    var $iframe = $(this).find(".stockviewer-iframe-holder");
                    if ($iframe.find(".stockviewer-iframe-holder-sorting").length === 0) {
                        $iframe.append("<div class='stockviewer-iframe-holder-sorting'></div>");
                    }
                });
            },
            stop: function (event, ui) {
                $(".stockviewer-iframe-holder-sorting").remove();
                reorderViewers();
            }
        });

        ResizeTable();
    }
    function buildStockViewerIframe(ticker) {
        var str = "<div class='stockviewer-toolbar'><span class='stockviewer-toolbar-name'>" + ticker.symbol + "</span>";
        str += "<span class='stockviewer-toolbar-close' onclick=\"stockviewerApp.RemoveStockViewer('" + ticker.id + "');\"></span>";
        str += "<span class='stockviewer-toolbar-edit' onclick=\"stockviewerApp.OpenEditStockViewer('" + ticker.id + "');\"></span>";
        str += "<span class='stockviewer-toolbar-move'></span>";
        str += "<div class='clear'></div></div>";
        str += "<div class='stockviewer-iframe-holder'>";

        var query = "?symbol=" + ticker.symbol;
        query += "&interval=" + ticker.interval;
        query += "&timezone=" + ticker.timezone;
        query += "&theme=" + ticker.theme;
        query += "&barStyle=" + ticker.barStyle;
        query += "&lng=" + ticker.lng;
        query += "&showTopbar=" + ticker.showTopbar;
        query += "&showBottombar=" + ticker.showBottombar;
        query += "&showDetails=" + ticker.showDetails;
        query += "&showStockTwits=" + ticker.showStockTwits;
        query += "&showHeadlines=" + ticker.showHeadlines;
        query += "&showDrawingbar=" + ticker.showDrawingbar;
        query += "&showHotlist=" + ticker.showHotlist;
        query += "&showCalendar=" + ticker.showCalendar;
        query += "&timestamp=" + new Date().getTime();

        str += "<iframe class='stockviewer-iframe' src='" + openWSE.siteRoot() + "Apps/StockViewer/stockViewerIframe.html" + query + "'></iframe>";
        str += "</div>";
        return str;
    }
    function refreshIframe(ticker) {
        var $this = $("#stockviewer_holder").find(".stockviewer-div[data-id='" + ticker.id + "']");
        if ($this.length > 0) {
            $this.html(buildStockViewerIframe(ticker));
            loadingPopup.Message("Loading...", loaderHolder);
            $this.find(".stockviewer-iframe").one("load", function () {
                loadingPopup.RemoveMessage(loaderHolder);
            });
        }
    }
    function reorderViewers() {
        var hasSequenceUpdate = false;
        $(".stockviewer-div").each(function (index) {
            var id = $(this).attr("data-id");
            for (var i = 0; i < tickers.length; i++) {
                if (tickers[i].id === id) {
                    if (tickers[i].sequence !== index) {
                        hasSequenceUpdate = true;
                    }
                    tickers[i].sequence = index;
                    break;
                }
            }
        });

        if (hasSequenceUpdate) {
            for (var i = 0; i < tickers.length; i++) {
                var myJsonString = JSON.stringify(tickers[i]);
                openWSE.AjaxCall("Apps/StockViewer/StockViewerService.asmx/UpdateTicker", "{ 'id': '" + tickers[i].id + "', 'tickerJson': '" + escape(myJsonString) + "' }", null, function (data) {
                    refreshIframe(JSON.parse(unescape(data.d)));
                });
            }
        }
    }

    function sortTickers(obj) {
        return obj.sort(function (a, b) {
            var x = parseInt(a.sequence);
            var y = parseInt(b.sequence);
            return ((x < y) ? -1 : ((x > y) ? 1 : 0));
            //return ((x > y) ? -1 : ((x < y) ? 1 : 0));
        });
    }

    function OpenAddStockViewerModal() {
        openWSE.LoadModalWindow(true, "add-stockviewer-element", "Add Stock Viewer");
        $("#tb_stockviewer_addSymbol").val("");
        $("#tb_stockviewer_addSymbol").focus();
        $("#btn_stockviewer_add").show();
        $("#btn_stockviewer_update").hide();
        $("#btn_stockviewer_update").attr("onclick", "");
    }
    function AddStockViewer() {
        var symbol = $.trim($("#tb_stockviewer_addSymbol").val());
        if (symbol) {
            var _id = generateGuid();
            tickers.push({
                id: _id,
                symbol: symbol.toUpperCase(),
                interval: $("#select_stockviewer_interval").val(),
                timezone: $("#select_stockviewer_timezone").val(),
                theme: $("#select_stockviewer_theme").val(),
                barStyle: $("#select_stockviewer_barstyle").val(),
                lng: $("#select_stockviewer_language").val(),
                showTopbar: $("#cb_stockviewer_showtopbar").is(":checked"),
                showBottombar: $("#cb_stockviewer_showbottombar").is(":checked"),
                showDetails: $("#cb_stockviewer_showdetails").is(":checked"),
                showStockTwits: $("#cb_stockviewer_showstocktwits").is(":checked"),
                showHeadlines: $("#cb_stockviewer_showheadlines").is(":checked"),
                showDrawingbar: $("#cb_stockviewer_showdrawingbar").is(":checked"),
                showHotlist: $("#cb_stockviewer_showhotlist").is(":checked"),
                showCalendar: $("#cb_stockviewer_showcalendar").is(":checked"),
                sequence: tickers.length
            });

            loadingPopup.Message("Saving...", loaderHolder);
            var myJsonString = JSON.stringify(tickers[tickers.length - 1]);
            openWSE.AjaxCall("Apps/StockViewer/StockViewerService.asmx/SaveTicker", "{ 'id': '" + _id + "', 'tickerJson': '" + escape(myJsonString) + "' }", null, function (data) {
                loadingPopup.RemoveMessage(loaderHolder);
                buildTable();
            });

            openWSE.LoadModalWindow(false, "add-stockviewer-element", "");
        }
    }
    function KeyPressAddStockViewer(event) {
        try {
            if (event.which == 13) {
                AddStockViewer();
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                AddStockViewer();
                return false;
            }
            delete evt;
        }
    }
    function RemoveStockViewer(id) {
        openWSE.ConfirmWindow("Are you sure you want to remove this view?", function () {
            for (var i = 0; i < tickers.length; i++) {
                if (tickers[i].id === id) {
                    tickers.splice(i, 1);
                    break;
                }
            }

            loadingPopup.Message("Deleting...", loaderHolder);
            openWSE.AjaxCall("Apps/StockViewer/StockViewerService.asmx/DeleteTicker", "{ 'id': '" + id + "' }", null, function (data) {
                loadingPopup.RemoveMessage(loaderHolder);
                buildTable();
            });
        }, null);
    }

    function OpenEditStockViewer(id) {
        for (var i = 0; i < tickers.length; i++) {
            if (tickers[i].id === id) {
                $("#tb_stockviewer_addSymbol").val(tickers[i].symbol);
                $("#select_stockviewer_interval").val(tickers[i].interval);
                $("#select_stockviewer_timezone").val(tickers[i].timezone);
                $("#select_stockviewer_theme").val(tickers[i].theme);
                $("#select_stockviewer_barstyle").val(tickers[i].barStyle);
                $("#select_stockviewer_language").val(tickers[i].lng);
                $("#cb_stockviewer_showtopbar").prop("checked", tickers[i].showTopbar);
                $("#cb_stockviewer_showbottombar").prop("checked", tickers[i].showBottombar);
                $("#cb_stockviewer_showdetails").prop("checked", tickers[i].showDetails);
                $("#cb_stockviewer_showstocktwits").prop("checked", tickers[i].showStockTwits);
                $("#cb_stockviewer_showheadlines").prop("checked", tickers[i].showHeadlines);
                $("#cb_stockviewer_showdrawingbar").prop("checked", tickers[i].showDrawingbar);
                $("#cb_stockviewer_showhotlist").prop("checked", tickers[i].showHotlist);
                $("#cb_stockviewer_showcalendar").prop("checked", tickers[i].showCalendar);
                $("#btn_stockviewer_add").hide();
                $("#btn_stockviewer_update").show();
                $("#btn_stockviewer_update").attr("onclick", "stockviewerApp.UpdateStockViewer('" + tickers[i].id + "');");
                openWSE.LoadModalWindow(true, "add-stockviewer-element", "Edit Stock Viewer");
                $("#tb_stockviewer_addSymbol").focus();
                break;
            }
        }
    }
    function UpdateStockViewer(id) {
        for (var i = 0; i < tickers.length; i++) {
            if (tickers[i].id === id) {
                tickers[i].symbol = $.trim($("#tb_stockviewer_addSymbol").val());
                tickers[i].interval = $("#select_stockviewer_interval").val();
                tickers[i].timezone = $("#select_stockviewer_timezone").val();
                tickers[i].theme = $("#select_stockviewer_theme").val();
                tickers[i].barStyle = $("#select_stockviewer_barstyle").val();
                tickers[i].lng = $("#select_stockviewer_language").val();
                tickers[i].showTopbar = $("#cb_stockviewer_showtopbar").prop("checked");
                tickers[i].showBottombar = $("#cb_stockviewer_showbottombar").prop("checked");
                tickers[i].showDetails = $("#cb_stockviewer_showdetails").prop("checked");
                tickers[i].showStockTwits = $("#cb_stockviewer_showstocktwits").prop("checked");
                tickers[i].showHeadlines = $("#cb_stockviewer_showheadlines").prop("checked");
                tickers[i].showDrawingbar = $("#cb_stockviewer_showdrawingbar").prop("checked");
                tickers[i].showHotlist = $("#cb_stockviewer_showhotlist").prop("checked");
                tickers[i].showCalendar = $("#cb_stockviewer_showcalendar").prop("checked");

                loadingPopup.Message("Updating...", loaderHolder);
                var myJsonString = JSON.stringify(tickers[i]);
                openWSE.AjaxCall("Apps/StockViewer/StockViewerService.asmx/UpdateTicker", "{ 'id': '" + id + "', 'tickerJson': '" + escape(myJsonString) + "' }", null, function (data) {
                    loadingPopup.RemoveMessage(loaderHolder);
                    refreshIframe(JSON.parse(unescape(data.d)));
                });
                break;
            }
        }

        openWSE.LoadModalWindow(false, "add-stockviewer-element", "");
    }

    function generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    return {
        Initialize: Initialize,
        ResizeTable: ResizeTable,
        OpenAddStockViewerModal: OpenAddStockViewerModal,
        AddStockViewer: AddStockViewer,
        KeyPressAddStockViewer: KeyPressAddStockViewer,
        RemoveStockViewer: RemoveStockViewer,
        OpenEditStockViewer: OpenEditStockViewer,
        UpdateStockViewer: UpdateStockViewer
    }
}();

$(document).ready(function () {
    if ($("#stockviewer-load").length > 0) {
        stockviewerApp.Initialize();
    }
});
$(window).resize(function () {
    if ($("#stockviewer-load").length > 0) {
        stockviewerApp.ResizeTable();
    }
});
function pageLoad() {
    if ($("#stockviewer-load").length > 0) {
        stockviewerApp.Initialize();
    }
}
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
    if ($("#stockviewer-load").length > 0) {
        stockviewerApp.Initialize();
    }
});
