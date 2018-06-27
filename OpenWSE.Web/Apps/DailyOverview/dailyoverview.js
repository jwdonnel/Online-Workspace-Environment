var hideControls = 0;
Sys.Application.add_load(function () {
    loadingPopup.RemoveMessage();
    $("#tb_search_dailyoverview").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetTruckSchedule", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                dataFilter: function (data) { return data; }
            }, function (data) {
                response($.map(data.d, function (item) {
                    return {
                        label: item,
                        value: item
                    };
                }));
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    if (hideControls == 1) {
        $("#pnl_multisort_dailyoverview, #min_Controls, #btn_showallmonth_dailyoverview, .searchwrapper").hide();
        $("#hyp_hidecontrols").html("Show Controls");
    }
    else {
        $("#pnl_multisort_dailyoverview, #min_Controls, #btn_showallmonth_dailyoverview, .searchwrapper").show();
        $("#hyp_hidecontrols").html("Hide Controls");
    }

    ResizeSideBar();

    cookieFunctions.get("dailyoverview-fontsize", function (fontsize) {
        if ((fontsize != null) && (fontsize != "")) {
            $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
            $("#font-size-selector option").each(function () {
                if ($(this).val() == fontsize) {
                    $(this).attr('selected', 'selected');
                } else {
                    $(this).removeAttr('selected');
                }
            });
        } else {
            $("#font-size-selector option").each(function () {
                if ($(this).val() == "small") {
                    $(this).attr('selected', 'selected');
                } else {
                    $(this).removeAttr('selected');
                }
            });
        }
    });
});

var scrollPause = false;
var scrollDir = "down";
function pageScroll() {
    if (!scrollPause) {
        var elem = document.getElementById("autoscroll-content-dailyoverview");
        if (elem != null) {
            try {
                elem.scrollTop = incrScroll; // horizontal and vertical scroll increments

                if (scrollDir == "down") {
                    incrScroll += incrScroll_temp;
                }
                else if (scrollDir == "up") {
                    incrScroll -= incrScroll_temp;
                    if (incrScroll < -1) {
                        scrollDir = "down";
                    }
                }

                if (elem.scrollTop + incrScroll_temp < incrScroll) {
                    scrollDir = "up"
                }
            }
            catch (evt) { }
        }

        try {
            setTimeout("pageScroll()", incrTime); // scrolls every 100 milliseconds
        }
        catch (evt) {
            setTimeout("pageScroll()", 75);
        }
    }
}

function PausePageScroll(_this) {
    if (!scrollPause) {
        scrollPause = true;
        $(_this).removeClass("img-pause").addClass("img-play");
    }
    else {
        scrollPause = false;
        $(_this).removeClass("img-play").addClass("img-pause");
        pageScroll();
    }
}


$(document.body).on("click", ".td-sort-click", function () {
    var $this = $(this).find("a");
    if ($this.length > 0) {
        loadingPopup.Message("Sorting. Please Wait...");
        var href = $this.attr("href");
        href = href.replace("javascript:", "");
        href = href.replace("openWSE.CallDoPostBack(", "");
        href = href.replace(")", "");
        href = href.replace(/'/g, "");
        var arr = href.split(",");
        openWSE.CallDoPostBack(arr[0], arr[1]);
    }
});

function ResizeSideBar() {
    var h = $(window).height() - $(".app-title-bg-color").outerHeight();
    $(".content-overflow-app").css("height", h - 25);
    $(".sidebar-padding").css("height", h - 30);
}

$(document.body).on("click", ".day-picker-hover a", function () {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Changing Day. Please Wait...");
});

$(document.body).on("click", ".calendar-delivery-pickups-title a", function () {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Changing Month. Please Wait...");
});

$(document.body).on("click", ".dailyoverview-update-img, .dailyoverview-update a", function() {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Loading. Please Wait...");
});
$(document.body).on("change", "#dd_display_dailyoverview, #dd_month_dailyoverview", function() {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Loading. Please Wait...");
});

$(document.body).on("change", "#font-size-selector", function() {
    var fontsize = $("#font-size-selector").val();
    $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
    cookieFunctions.set("dailyoverview-fontsize", fontsize, "30");
});

function HideSidebar_overview(x) {
    if (x == 0) {
        $('.sidebar-scroll-app').fadeOut(150, function () {
            $("#showsidebar_overview").css("display", "block");
        });
    } else {
        $("#showsidebar_overview").css("display", "none");
        $('.sidebar-scroll-app').fadeIn(150);
    }
    return false;
}

function HideControls() {
    var $_min_Controls = $("#min_Controls");
    if ($_min_Controls.css("display") != "none") {
        $("#pnl_multisort_dailyoverview, #min_Controls, #btn_showallmonth_dailyoverview, .searchwrapper").fadeOut(200);
        $("#hyp_hidecontrols").html("Show Controls");
        hideControls = 1;
    }
    else {
        $("#pnl_multisort_dailyoverview, #min_Controls, #btn_showallmonth_dailyoverview, .searchwrapper").fadeIn(200);
        $("#hyp_hidecontrols").html("Hide Controls");
        hideControls = 0;
    }
}

$(window).resize(function() {
    ResizeSideBar();
});