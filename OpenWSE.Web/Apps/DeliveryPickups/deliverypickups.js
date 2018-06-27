var date;
var selected_m;
var selected_d;
var selected_y;
var schedule_items;
var btn_finish;
var lbl_SelectedDate;
var cb_sendEmail;
var step1;
var step2;

$(document).ready(function () {
    ResizeSideBar();
    loadFontSize();
    openWSE.CallDoPostBack('btn_modalClose_deliverypickups', '');
});

$(document.body).on("change", "#font-size-selector", function() {
    var fontsize = $("#font-size-selector").val();
    $(".GridNormalRow").css("font-size", fontsize);
    cookieFunctions.set("deliverypickups-fontsize", fontsize, "30");
});

function loadFontSize() {
    cookieFunctions.get("deliverypickups-fontsize", function (fontsize) {
        if ((fontsize != null) && (fontsize != "")) {
            $(".GridNormalRow").css("font-size", fontsize);
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
}

$(document.body).on("change", "#dd_display_deliverypickups", function() {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Updating. Please Wait...");
});

function ResizeSideBar() {
    var h = $(window).height() - $(".stylefour").outerHeight();
    $(".content-overflow-app").css("height", h - 25);
    $(".sidebar-padding").css("height", h - 30);
}

$(document.body).on("click", ".deliverypickup-update a, a.deliverypickup-update, .deliverypickup-update-img,.GridDateNext, .GridDateNext, .GridDatePrev, .catButtons", function() {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Updating. Please Wait...");
});

$(document.body).on("click", ".deliverypickup-update-modal", function() {
    onUpdating_Sch();
});

$(document.body).on("click", ".GridDatePrev", function() {
    try {
        var hf_d = document.getElementById("hf_dateselected_deliverypickups");
        var parts = hf_d.value.split('/');
        var d = new Date(parts[2], parts[0], 1, new Date().getHours(), new Date().getMinutes(), new Date().getSeconds(), new Date().getMilliseconds());
        var temp = new Date(d.getFullYear(), d.getMonth(), (parseInt(parts[1]) - 1), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds());
        var curr = temp.getMonth() + "/" + temp.getDate() + "/" + temp.getFullYear();
        hf_d.value = "";
        hf_d.value = curr;
        openWSE.CallDoPostBack("hf_refreshTimer_deliverypickups", "");
    } catch(evt) {
        openWSE.AlertWindow("Error");
    }
});

$(document.body).on("click", ".GridDateNext", function() {
    try {
        var hf_d = document.getElementById("hf_dateselected_deliverypickups");
        var parts = hf_d.value.split('/');
        var d = new Date(parts[2], parts[0], 1, new Date().getHours(), new Date().getMinutes(), new Date().getSeconds(), new Date().getMilliseconds());
        var temp = new Date(d.getFullYear(), d.getMonth(), (parseInt(parts[1]) + 1), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds());
        var curr = temp.getMonth() + "/" + temp.getDate() + "/" + temp.getFullYear();
        hf_d.value = "";
        hf_d.value = curr;
        openWSE.CallDoPostBack("hf_refreshTimer_deliverypickups", "");
    } catch(evt) {
        openWSE.AlertWindow("Error");
    }
});

$(document.body).on("click", ".catButtons", function() {
    var hf_category = document.getElementById("hf_category_deliverypickups");
    var t = $(this).attr('href');
    t = t.split("#");
    t = t[1];
    if (hf_category.value != t) {
        hf_category.value = t;
        openWSE.CallDoPostBack("hf_category_deliverypickups", "");
    } else {
        hf_category.value = "";
        openWSE.CallDoPostBack("hf_category_deliverypickups", "");
    }
    return false;
});

var prm = Sys.WebForms.PageRequestManager.getInstance();

prm.add_endRequest(function() {
    loadFontSize();

    $("#tb_search_deliverypickups").autocomplete({
        minLength: 1,
        source: function (request, response) {
            openWSE.AjaxCall("WebServices/AutoComplete_Custom.asmx/GetSchedule", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
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
    }).focus(function() {
        $(this).autocomplete("search", "");
    });
    $("#tb_Date1_deliverypickups").datepicker();
});

function onSchedulerLoad(a, b, c, d, e, s1, s2) {
    hf_dID = a;
    hf_d = document.getElementById(a);
    schedule_items = document.getElementById(b);
    btn_finish = c;
    lbl_SelectedDate = d;
    cb_sendEmail = e;
    step1 = s1;
    step2 = s2;
    $("#tb_Date1_deliverypickups").datepicker();
}

$(window).resize(function() {
    ResizeSideBar();
});

function onTypeChange(_this) {
    if (_this.selectedIndex == 0) {
        schedule_items.disabled = "";
        try {
            document.getElementById(btn_finish).value = "Schedule Delivery";
            try {
                var lbl_date = document.getElementById(lbl_SelectedDate).innerHTML.split("Pickup");
                document.getElementById(lbl_SelectedDate).innerHTML = lbl_date[0] + "Delivery" + lbl_date[1];
            } catch(evt) {
            }
        } catch(evt) {
        }
    } else {
        schedule_items.disabled = "disabled";
        try {
            document.getElementById(btn_finish).value = "Schedule Pickup";
            try {
                var lbl_date = document.getElementById(lbl_SelectedDate).innerHTML.split("Delivery");
                document.getElementById(lbl_SelectedDate).innerHTML = lbl_date[0] + "Pickup" + lbl_date[1];
            } catch(evt) {
            }
        } catch(evt) {
        }
    }
}

function sendEmailChecked() {
    var cb_sendEmail = document.getElementById("cb_sendEmail_deliverypickups");
    var emailMarker = document.getElementById("emailmarker");
    if (cb_sendEmail.checked == false) {
        emailMarker.style.display = "none";
        cb_sendEmail.checked = false;
    } else {
        emailMarker.style.display = "inline";
        cb_sendEmail.checked = true;
    }

}

$(document.body).on("click", ".ajaxCall_Modal", function () {
    openWSE.LoadModalWindow(true, "NewDelPickup_element", "Schedule New Delivery/Pickup");
    return false;
});

function btnStepOne() {
    fadeOutSteps(step1, step2);
}

function btnStepBack() {
    fadeOutSteps(step2, step1);
}

function fadeInSteps(s) {
    $("#" + s).fadeIn(300);
}

function fadeOutSteps(s1, s2) {
    $("#" + s1).fadeOut(300, function() {
        fadeInSteps(s2);
    });
}

function onUpdating_Sch() {
    document.getElementById('container_Sch').style.display = "none";
    document.getElementById('loading_Sch').style.display = "block";
}

function onUpdated_Sch() {
    document.getElementById('loading_Sch').style.display = "none";
    document.getElementById('container_Sch').style.display = "block";
}