var ison = 0;
Sys.Application.add_load(function () {
    if (cookie.get("dbviewer_interval") != null) {
        if (openWSE.ConvertBitToBoolean(cookie.get("dbviewer_interval"))) {
            $('#dd_interval').css('display', '');
            $('#a_turn_onoff_refresh').html('Turn off');
            UpdateTables();
            ison = 1;
        } else {
            $('#dd_interval').css('display', 'none');
            $('#a_turn_onoff_refresh').html('Turn on');
            clearTimeout(timeout_int);
            ison = 0;
        }
    }
    else {
        if (ison == 1) {
            $('#dd_interval').css('display', '');
            $('#a_turn_onoff_refresh').html('Turn off');
            UpdateTables();
        } else {
            $('#dd_interval').css('display', 'none');
            $('#a_turn_onoff_refresh').html('Turn on');
            clearTimeout(timeout_int);
        }
    }

    $("#MainContent_GV_dbviewer").attr("border", "0");
    $("#MainContent_GV_dbviewer").addClass("border-bottom");
    $("#MainContent_GV_dbviewer").find("tr").each(function () {
        if (!$(this).hasClass("myHeaderStyle")) {
            $(this).find("td").each(function () {
                $(this).addClass("border-right");
                $(this).addClass("border-bottom");
            });
        }
    });
});

$(document.body).on("click", ".dbviewer-update-img, .dbviewer-update a", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

$(document.body).on("click", "#a_turn_onoff_refresh", function () {
    if (ison == 0) {
        ison = 1;
        $('#dd_interval').css('display', '');
        $('#a_turn_onoff_refresh').html('Turn off');
        UpdateTables();
        cookie.set("dbviewer_interval", "1", "30");
    } else {
        ison = 0;
        $('#dd_interval').css('display', 'none');
        $('#a_turn_onoff_refresh').html('Turn on');
        clearTimeout(timeout_int);
        cookie.set("dbviewer_interval", "0", "30");
    }
    openWSE.RemoveUpdateModal();
    return false;
});

$(document.body).on("click", ".td-sort-click", function () {
    var $this = $(this).find("a");
    if ($this.length > 0) {
        openWSE.LoadingMessage1("Sorting. Please Wait...");
        var href = $this.attr("href");
        href = href.replace("javascript:", "");
        href = href.replace("__doPostBack(", "");
        href = href.replace(")", "");
        href = href.replace(/'/g, "");
        var arr = href.split(",");
        __doPostBack(arr[0], arr[1]);
    }
});

$(document.body).on("change", "#dd_display, #dd_table", function () {
    openWSE.LoadingMessage1("Updating. Please Wait...");
});

var updateinterval = 5000;
$(document.body).on("change", "#dd_interval", function () {
    updateinterval = $("#dd_interval").val();
});
var timeout_int;

function UpdateTables() {
    timeout_int = setTimeout(function () {
        if (ison == 1) {
            $("#hf_updatetable").val(new Date().toString());
            __doPostBack("hf_updatetable", "");
        }
    }, (updateinterval));
}