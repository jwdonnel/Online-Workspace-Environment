function DeleteConfirmation() {
    var x = "<div id='update-element'><div class='update-element-overlay'><div class='update-element-align'><div class='update-element-modal'>";
    x += openWSE.loadingImg + "<h3 class='inline-block'>";
    x += "Deleting. Please Wait...</h3></div></div></div></div>";
    $("#steeltrucks-load").append(x);
    $("#update-element").show();
}

$(window).resize(function() {
    ResizeSideBar();
});

$(document.body).on("click", ".td-sort-click", function () {
    var $this = $(this).find("a");
    if ($this.length > 0) {
        LoadingMessage1("Sorting. Please Wait...");
        var href = $this.attr("href");
        href = href.replace("javascript:", "");
        href = href.replace("__doPostBack(", "");
        href = href.replace(")", "");
        href = href.replace(/'/g, "");
        var arr = href.split(",");
        __doPostBack(arr[0], arr[1]);
    }
});

function ResizeSideBar() {
    var h = $(window).height() - $(".app-title-bg-color").outerHeight();
    $(".content-overflow-app").css("height", h - 25);
    $(".sidebar-padding").css("height", h - 30);
}

$(document.body).on("change", "#dd_display_steeltrucks", function() {
    var x = "<div id='update-element'><div class='update-element-overlay'><div class='update-element-align'><div class='update-element-modal'>";
    x += openWSE.loadingImg + "<h3 class='inline-block'>";
    x += "Updating. Please Wait...</h3></div></div></div></div>";
    $("#steeltrucks-load").append(x);
    $("#update-element").show();
});


$(document.body).on("click", ".tsdivclick", function() {
    var user = $(this).text();
    var usertemp = user.replace(/ /g, "_");
    var hf = document.getElementById("hf_currDriverexp");
    var hf2 = document.getElementById("hf_userselected_steeltrucks");
    if ((hf.value != "") && (document.getElementById("Schedule_Modal").style.display == "block")) {
        $("#expand_" + hf.value).removeClass("tsactive");
    } else if (document.getElementById("Schedule_Modal").style.display == "none") {
        window.setTimeout("loadScheduleModal()", 500);
    }
    hf.value = "expand_" + usertemp;
    hf2.value = usertemp;
    enableCurrSelected();
    document.getElementById("hf_refresher_steeltrucks").value = new Date().toString();
    __doPostBack("hf_refresher_steeltrucks", "");
});

function CommonCarrierDateSelecte(a) {
    var hf = document.getElementById("hf_currDriverexp");

    if (hf.value != "") {
        $("#expand_" + hf.value).removeClass("tsactive");
    }

    hf.value = "expand_" + a;
    document.getElementById("hf_dateselected_steeltrucks").value = a;
    __doPostBack("hf_dateselected_steeltrucks", "");
}

$(document.body).on("change", "#font-size-selector", function() {
    var fontsize = $("#font-size-selector").val();
    $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
    cookie.set("commoncarriers-fontsize", fontsize, "30");
});

function reloadCurr() {
    try {
        var sf = document.getElementById("Schedule_Modal");
        var hf_user = document.getElementById(hf_userselected_steeltrucks);
        if ((hf_user.value != null) || (hf_user.value != "")) {
            var usertemp = hf_user.value;
            $("#expand_" + usertemp).addClass("tsactive");
            enableCurrSelected();
        }

        if (sf.style.display != "none") {
            sf.style.display = "block";
        }
    } catch(evt) {
        try {
            var hf_user = document.getElementById("hf_dateselected_steeltrucks");
            if ((hf_user.value != null) || (hf_user.value != "")) {
                var usertemp = hf_user.value;
                $("#expand_" + usertemp).addClass("tsactive");
            }
        } catch(evt2) {
        }
    }
}

function eventnewtsClick() {
    var cf = document.getElementById("pnl_newSchModal");
    if (cf.style.display == "none") {
        cf.style.display = "block";
        $("#hdl2").removeClass("active");
        $("#other").hide();
        $("#hdl1").addClass("active");
        $("#steeltrucks").fadeIn(200);
        el = document.getElementById("Createnew_Overlay");
        el.style.display = (el.style.display == "block") ? "none" : "block";
        el.style.visibility = (el.style.visibility == "visible") ? "hidden" : "visible";
        $("#pnl_newSchModal").fadeTo(150, 1.0);
        $(window).resize();
    } else {
        $("#pnl_newSchModal").fadeTo(150, 0.0, function() {
            cf.style.display = "none";
            el = document.getElementById("Createnew_Overlay");
            el.style.display = (el.style.display == "block") ? "none" : "block";
            el.style.visibility = (el.style.visibility == "visible") ? "hidden" : "visible";
        });
    }
    window.setTimeout(function () { openWSE.RemoveUpdateModal(); }, 500);
}

function eventnewtsClick2(driver) {
    var cf = document.getElementById("pnl_newSchModal");
    if (cf.style.display == "none") {
        document.getElementById(tb_drivername_steeltrucks).value = driver;
        cf.style.display = "block";
        $("#hdl2").removeClass("active");
        $("#other").hide();
        $("#hdl1").addClass("active");
        $("#steeltrucks").fadeIn(200);
        el = document.getElementById("Createnew_Overlay");
        el.style.display = (el.style.display == "block") ? "none" : "block";
        el.style.visibility = (el.style.visibility == "visible") ? "hidden" : "visible";
        $("#pnl_newSchModal").fadeTo(150, 1.0);
        $(window).resize();
    } else {
        $("#pnl_newSchModal").fadeTo(150, 0.0, function() {
            cf.style.display = "none";
            el = document.getElementById("Createnew_Overlay");
            el.style.display = (el.style.display == "block") ? "none" : "block";
            el.style.visibility = (el.style.visibility == "visible") ? "hidden" : "visible";
        });
    }
    window.setTimeout(function () { openWSE.RemoveUpdateModal(); }, 500);
}

function showButtons(xdelete, xadd, xedit) {
    if ((xdelete.substring(0, 3) != 'x_-') && (!openWSE.ConvertBitToBoolean(document.getElementById("hf_editing").value))) {
        var temp = xadd.replace("a_", "");
        if ($("#expand_" + temp).hasClass("tsactive") != true) {
            document.getElementById(xdelete).style.display = 'block';
            document.getElementById(xadd).style.display = 'block';
            document.getElementById(xedit).style.display = 'block';
        }
    }
}

function hideButtons(xdelete, xadd, xedit) {
    if ((xdelete.substring(0, 3) != 'x_-') && (!openWSE.ConvertBitToBoolean(document.getElementById("hf_editing").value))) {
        var temp = xadd.replace("a_", "");
        if ($("#expand_" + temp).hasClass("tsactive") != true) {
            document.getElementById(xdelete).style.display = 'none';
            document.getElementById(xadd).style.display = 'none';
            document.getElementById(xedit).style.display = 'none';
        }
    }
}

function enableCurrSelected() {
    var hf = document.getElementById("hf_currDriverexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = '';
            document.getElementById("a_" + temp).style.display = '';
            document.getElementById("e_" + temp).style.display = '';
        } catch(evt) {
        }
    }
}

function disableCurrSelected() {
    var hf = document.getElementById("hf_currDriverexp");
    if (hf.value != "") {
        try {
            var temp = hf.value.replace("expand_", "");
            document.getElementById("x_" + temp).style.display = 'none';
            document.getElementById("a_" + temp).style.display = 'none';
            document.getElementById("e_" + temp).style.display = 'none';
        } catch(evt) {
        }
    }
}

function LoadGenDirEditor() {
    if ($("#DirEdit-element").css("display") != "block") {
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/BuildEditor",
            data: "{ }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#GenDirList").html(data.d);
                openWSE.LoadModalWindow(true, "DirEdit-element", "Direction Editor");
            },
            error: function (data) {
                openWSE.AlertWindow("There was an error loading direction list. Please try again");
            }
        });
    } else {
        openWSE.LoadModalWindow(false, "DirEdit-element", "");
        setTimeout(function () {
            $("#GenDirList").html("");
        }, openWSE_Config.animationSpeed);
    }
}

function deletegd(id, dir) {
    openWSE.ConfirmWindow("Are you sure you want to delete this General Direction? (Any associated schedule will default to Kansas.)",
        function () {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/DeleteDirection",
                data: "{ 'id': '" + id + "','gendir': '" + escape(dir) + "' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (!openWSE.ConvertBitToBoolean(data.d)) {
                        openWSE.AlertWindow("Error deleting direction. Please try again.");
                    } else {
                        $("#GenDirList").html(data.d);
                        document.getElementById("hf_refresher_steeltrucks").value = new Date().toString();
                        __doPostBack("hf_refresher_steeltrucks", "");
                    }
                },
                error: function (data) {
                    openWSE.AlertWindow("There was an error deleting direction. Please try again");
                }
            });
        }, null);
}

function addgd() {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/AddDirection",
        data: "{ 'gendir': '" + escape(document.getElementById("tb_addgendir").value) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            } 
            else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } 
            else {
                $("#GenDirList").html(data.d);
                document.getElementById("hf_refresher_steeltrucks").value = new Date().toString();
                __doPostBack("hf_refresher_steeltrucks", "");
            }
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error adding direction. Please try again");
        }
    });
}

function editgd(dir) {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/EditDirection",
        data: "{ 'gendir': '" + escape(dir) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#GenDirList").html(data.d);
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error editing direction. Please try again");
        }
    });
}

function updategd(id) {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/UpdateDirection",
        data: "{ 'id':'" + id +"','gendir': '" + escape(document.getElementById("tb_editgendir").value) + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            } else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#GenDirList").html(data.d);
            }
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error updating direction. Please try again");
        }
    });
}

function canceleditgd() {
    $.ajax({
        url: openWSE.siteRoot() + "WebServices/UpdateGeneralDirections.asmx/BuildEditor",
        data: "{ }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#GenDirList").html(data.d);
        },
        error: function (data) {
            openWSE.AlertWindow("There was an error loading direction list. Please try again");
        }
    });
}

function OnKeyPress_DirEditNew(event, id) {
    try {
        if (event.which == 13) {
            if (id == "") {
                addgd();
            }
            else {
                updategd(id);
            }
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            if (id == "") {
                addgd();
            }
            else {
                updategd(id);
            }
        }
        delete evt;
    }
}