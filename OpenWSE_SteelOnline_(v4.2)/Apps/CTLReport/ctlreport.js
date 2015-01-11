var editCTLLine_ctlreport = "";
var prelistorder_ctlreport = "";
var searchMode_ctlreport = false;

$(document).ready(function () {
    LoadInit_CTLReport();
    LoadFontSize_CTLReport();

    var d = new Date();
    $("#tb_date_ctlreport").val((d.getMonth() + 1) + "/" + d.getDate() + "/" + d.getFullYear());
    LoadCTLReports();
});

$(document.body).on("click", "#app-ctlreport .exit-button-app, #app-ctlreport-min-bar .exit-button-app-min", function () {
    cookie.del("ctlreport-fontsize");
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    PageStartUp_CTLReports();
});

function PageStartUp_CTLReports() {
    var a = $("#ctl-report-holder").html();
    if (($.trim(a) == "") || ($.trim($("#tb_date_ctlreport").val()) == "")) {
        LoadInit_CTLReport();
        var d = new Date();
        $("#tb_date_ctlreport").val((d.getMonth() + 1) + "/" + d.getDate() + "/" + d.getFullYear());
        LoadCTLReports();
    }
}

function ExportToExcel_CTLReport(exportAll, ctlLine) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/CTLReport/CTLReportService.asmx/ExportToExcel",
        data: "{ '_date': '" + $("#tb_date_ctlreport").val() + "','_line': '" + ctlLine + "','_exportAll': '" + exportAll.toString() + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data1) {
            if (data1.d != "") {
                $.fileDownload(data1.d);
            }
            else {
                openWSE.AlertWindow("No records to export for CTL Line " + ctlLine);
            }
        }
    });
}

function LoadingCTLReports(message) {
    $.LoadingMessage("#ctlrport-load", message);
}

function LoadInit_CTLReport() {
    $("#tb_date_ctlreport").datepicker();
    $("#tb_search_ctlreport").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetCTLReports",
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
}

function InitializeTextBoxes_CTLReport() {
    $("#tb_dueDate_new,#tb_dueDate_edit,#tb_reportdate_edit").datepicker();
    $("#tb_approvedBy_ctlreport, #tb_approvedby_edit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListOfUsersByFullName",
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
    $("#tb_customer_new,#tb_customer_edit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCustomersTS",
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
    $("#tb_material_new,#tb_material_edit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetTypes",
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
    $("#tb_gauge_new,#tb_gauge_edit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetGauges",
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
    $("#tb_width_new,#tb_width_edit").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetWidths",
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

    $(function () {
        $("#table-ctl-reports tbody").sortable({
            axis: 'y',
            cancel: '.non-moveable, .myHeaderStyle',
            helper: fixHelper_CTLReports,
            containment: '#ctl-report-holder',
            opacity: 0.9,
            scrollSensitivity: 40,
            scrollSpeed: 40,
            start: function (event, ui) {
                prelistorder_ctlreport = "";
                $("#table-ctl-reports").find(".ctlline-reports-id-sortable").each(function () {
                    var temp = $(this).html();
                    if (temp != "") {
                        prelistorder_ctlreport += (temp + ",");
                    }
                });
            },
            stop: function (event, ui) {
                var listorder = "";
                $("#table-ctl-reports").find(".ctlline-reports-id-sortable").each(function () {
                    var temp = $(this).html();
                    if (temp != "") {
                        listorder += (temp + ",");
                    }
                });

                if (prelistorder_ctlreport != listorder) {
                    editCTLLine_ctlreport = "";
                    LoadingCTLReports("Updating Sequence...");
                    $.ajax({
                        url: openWSE.siteRoot() + "Apps/CTLReport/CTLReportService.asmx/UpdateSequence",
                        type: "POST",
                        data: '{ "ids": "' + listorder + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            LoadCTLReports();
                            openWSE.RemoveUpdateModal();
                        },
                        error: function (data) {
                            $("#errorPullingRecords-ctlreport").html("There was a problem trying to update the sequence!");
                            openWSE.RemoveUpdateModal();
                        }
                    });
                }
            }
        });
    });
}

var fixHelper_CTLReports = function (e, ui) {
    ui.children().each(function () {
        $(this).width($(this).width());
    });
    return ui;
};





/* Load Table
----------------------------------*/
function LoadCTLReports() {
    LoadingCTLReports("Loading...");
    var date = $("#tb_date_ctlreport").val();
    $("#errorPullingRecords-ctlreport").html("");
    $.ajax({
        url: "Apps/CTLReport/CTLReportService.asmx/LoadReport",
        type: "POST",
        data: '{ "date": "' + date + '","searchValue": "' + $("#tb_search_ctlreport").val() + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                if ($("#ctlreport-search-tab").css("display") == "none") {
                    BuildMainTable(data, false);
                    if ($.trim($("#ctl-report-search-holder").html()) != "") {
                        $("#ctl-report-search-holder").html("<h3>Loading Data. Please Wait...</h3>");
                    }
                }
                else {
                    BuildMainTable(data, true);
                    if ($.trim($("#ctl-report-holder").html()) != "") {
                        $("#ctl-report-holder").html("<h3>Loading Data. Please Wait...</h3>");
                    }
                }

                InitializeTextBoxes_CTLReport();
                LoadFontSize_CTLReport();
                openWSE.RemoveUpdateModal();
            }
        },
        error: function (data) {
            $("#errorPullingRecords-ctlreport").html("There was a problem trying to get your records!");
            openWSE.RemoveUpdateModal();
        }
    });
}

function BuildMainTable(data, searchMode) {
    searchMode_ctlreport = searchMode;
    var $holder = $("#ctl-report-holder");
    var x = "<table id='table-ctl-reports' cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>";
    x += BuildCTLLineRowHeader();
    for (var i = 0; i < data.d.length; i++) {
        x += BuildCTLLineRowBody(data.d[i]);
    }

    x += "</tbody></table>";
    if (data.d.length == 0) {
        x += "<div class='emptyGridView'>No data available</div>";
    }

    $holder.html(x);
}

function BuildCTLLineRowHeader() {
    var x = "<tr class='myHeaderStyle'>";
    if (searchMode_ctlreport) {
        x += "<td style='width: 50px; max-width: 50px;'></td>";
    }
    else {
        x += "<td style='width: 50px; max-width: 50px;'>Seq</td>";
    }
    x += "<td>Coil Number</td>";
    x += "<td>Gauge</td>";
    x += "<td>Material</td>";
    x += "<td>Width</td>";
    x += "<td>Line</td>";
    x += "<td>Approved By</td>";

    if (searchMode_ctlreport) {
        x += "<td>Date Issued</td>";
    }

    x += "</tr>";
    return x;
}

function BuildCTLLineRowBody(data) {
    var x = "<tr class='myItemStyle GridNormalRow cursor-pointer' onclick='OpenEditModalCTLLine(\"" + data[0] + "\");' title='Click to open ticket'>";
    if (searchMode_ctlreport) {
        x += "<td class='GridViewNumRow non-moveable' align='center'></td>";
    }
    else {
        x += "<td class='GridViewNumRow' align='center' style='cursor: move;'>" + data[1] + "<span class='ctlline-reports-id-sortable display-none'>" + data[0] + "</span></td>";
    }
    x += "<td class='border-right non-moveable' align='center'><div class='pad-top-sml pad-bottom-sml'>" + data[2] + "</div></td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[3] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[4] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[5] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[6] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[7] + "</td>";

    if (searchMode_ctlreport) {
        x += "<td class='border-right non-moveable' align='center'>" + data[8] + "</td>";
    }

    x += "</tr>";
    return x;
}





/* Search
----------------------------------*/
function SearchTruckCTLRecords() {
    var searchVal = $("#tb_search_ctlreport").val();
    if (($.trim(searchVal) != "") || ($.trim(searchVal) != "Search for CTL Report")) {
        $("#ctlreport-tab, #ctlreport-tab-topcontrols").fadeOut(openWSE_Config.animationSpeed, function () {
            $("#ctlreport-search-tab, #ctlreport-tab-search-topcontrols").fadeIn(openWSE_Config.animationSpeed, function () {
                LoadCTLReports();
            });
        });
    }
}

function KeyPressSearch_CTLReport(event) {
    try {
        if (event.which == 13) {
            SearchTruckCTLRecords();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            SearchTruckCTLRecords();
        }
        delete evt;
    }
}

function RefreshCTLReport() {
    $("#errorPullingRecords-ctlreport").html("");
    LoadCTLReports();
}

function CloseReport() {
    $("#errorPullingRecords-ctlreport").html("");
    $("#tb_search_ctlreport").val("Search for CTL Report");
    $("#ctlreport-search-tab, #ctlreport-tab-search-topcontrols").fadeOut(openWSE_Config.animationSpeed, function () {
        $("#ctlreport-tab, #ctlreport-tab-topcontrols").fadeIn(openWSE_Config.animationSpeed, function () {
            LoadCTLReports();
        });
    });
}





/* Load Fonts
----------------------------------*/
function LoadFontSize_CTLReport() {
    var fontsize = cookie.get("ctlreport-fontsize");
    if ((fontsize != null) && (fontsize != "")) {
        $("#ctlrport-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_CTLLines input").css("font-size", fontsize);
        $("#font-size-selector-ctlreport option").each(function () {
            if ($(this).val() == fontsize) {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
    else {
        $("#font-size-selector-ctlreport option").each(function () {
            if ($(this).val() == "small") {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
}

function FontSelection_CTLReport() {
    var fontsize = $("#font-size-selector-ctlreport").val();
    $("#ctlrport-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_CTLLines input").css("font-size", fontsize);
    cookie.set("ctlreport-fontsize", fontsize, "30");
}





/* Edit and Delete Controls
----------------------------------*/
function OpenEditModalCTLLine(id) {
    openWSE.LoadModalWindow(true, 'ctlreport-edit-element', 'Edit/View Issued Ticket');
}

function OpenNewModalCTLLine() {
    $("#errorPullingRecords-ctlreport").html("");
    BuildMainTableAdd();
    InitializeTextBoxes_CTLReport();
    LoadFontSize_CTLReport();
    openWSE.LoadModalWindow(true, 'ctlreport-edit-element', 'New Issued Ticket');
}

function BuildMainTableEdit(data) {
    var $holder = $("#EventList-steeltrucks");
    var x = "<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>";
    x += BuildCTLLineRowHeaderEdit();
    for (var i = 0; i < data.d.length; i++) {
        x += BuildCTLLineRowBodyEdit(data.d[i]);
    }
    x += BuildCTLLineRowBodyAdd();
    x += "</tbody></table>";
    $holder.html(x);
}

function BuildMainTableAdd() {
    var $holder = $("#EventList-steeltrucks");
    var x = "<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>";
    x += BuildCTLLineRowHeaderEdit();
    x += BuildCTLLineRowBodyAdd();
    x += "</tbody></table>";
    $holder.html(x);

    $("#ctl-eventEdit-btns").hide();
    $("#ctl-eventEdit-Material").html("<input id='tb_Material_add' class='textEntry' type='text' value='Material' onfocus=\"if(this.value=='Material')this.value=''\" onblur=\"if(this.value=='')this.value='Material'\" style='width: 150px;' />");
    $("#ctl-eventEdit-CoilsToCut").html("<input id='tb_CoilsToCut_add' class='textEntry' type='text' value='Coils to Cut' onfocus=\"if(this.value=='Coils to Cut')this.value=''\" onblur=\"if(this.value=='')this.value='Coils to Cut'\" style='width: 150px;' />");
    $("#ctl-eventEdit-CoilWeight").html("<input id='tb_CoilWeight_add' class='textEntry' type='text' value='Coil Weight' onfocus=\"if(this.value=='Coil Weight')this.value=''\" onblur=\"if(this.value=='')this.value='Coil Weight'\" style='width: 150px;' />");
    $("#ctl-eventEdit-Gauge").html("<input id='tb_Gauge_add' class='textEntry' type='text' value='Gauge' onfocus=\"if(this.value=='Gauge')this.value=''\" onblur=\"if(this.value=='')this.value='Gauge'\" style='width: 150px;' />");
    $("#ctl-eventEdit-CoilNumber").html("<input id='tb_CoilNumber_add' class='textEntry' type='text' value='Coil Number' onfocus=\"if(this.value=='Coil Number')this.value=''\" onblur=\"if(this.value=='')this.value='Coil Number'\" style='width: 150px;' />");
    $("#ctl-eventEdit-Width").html("<input id='tb_Width_add' class='textEntry' type='text' value='Width' onfocus=\"if(this.value=='Width')this.value=''\" onblur=\"if(this.value=='')this.value='Width'\" style='width: 150px;' />");
}

function BuildCTLLineRowHeaderEdit() {
    var x = "<tr class='myHeaderStyle'>";
    x += "<td style='width: 50px; max-width: 50px;'>Seq</td>";
    x += "<td>Due Date</td>";
    x += "<td>Customer</td>";
    x += "<td>Order #</td>";
    x += "<td>Line #</td>";
    x += "<td># of Lifts</td>";
    x += "<td># of Pallets</td>";
    x += "<td></td>";
    x += "</tr>";
    return x;
}

function BuildCTLLineRowBodyEdit(data) {
    var x = "<tr class='myItemStyle GridNormalRow'>";
    x += "<td class='GridViewNumRow' align='center' style='cursor: move;'>" + data[1] + "<span class='ctlline-reports-id-sortable display-none'>" + data[0] + "</span></td>";
    x += "<td class='border-right non-moveable' align='center'><div class='pad-top-sml pad-bottom-sml'>" + data[2] + "</div></td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[3] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[4] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[5] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[6] + "</td>";
    x += "<td class='border-right non-moveable' align='center'>" + data[7] + "</td>";
    x += "<td class='border-right non-moveable' align='center'></td>";
    x += "</tr>";
    return x;
}

function BuildCTLLineRowBodyAdd() {
    var x = "<tr class='myItemStyle GridNormalRow'>";
    x += "<td class='GridViewNumRow' align='center'></td>";
    x += "<td class='border-right' align='center' style='width: 85px;'><input id='tb_duedate_add' class='textEntry' type='text' value='Due Date' onfocus=\"if(this.value=='Due Date')this.value=''\" onblur=\"if(this.value=='')this.value='Due Date'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center' style='minwidth: 100px;'><input id='tb_customer_add' class='textEntry editNewEnterCTLLine' type='text' value='Customer' onfocus=\"if(this.value=='Customer')this.value=''\" onblur=\"if(this.value=='')this.value='Customer'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center' style='width: 75px;'><input id='tb_orderNum_add' class='textEntry' type='text' value='Order #' onfocus=\"if(this.value=='Order #')this.value=''\" onblur=\"if(this.value=='')this.value='Order #'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center' style='width: 70px;'><input id='tb_lineNum_add' class='textEntry editNewEnterCTLLine' type='text' value='Line #' onfocus=\"if(this.value=='Line #')this.value=''\" onblur=\"if(this.value=='')this.value='Line #'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center' style='width: 90px;'><input id='tb_NumofLifts_add' class='textEntry editNewEnterCTLLine' type='text' value='# of Lifts' onfocus=\"if(this.value=='# of Lifts')this.value=''\" onblur=\"if(this.value=='')this.value='# of Lifts'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center' style='width: 90px;'><input id='tb_NumofPallets_add' class='textEntry' type='text' value='# of Pallets' onfocus=\"if(this.value=='# of Pallets')this.value=''\" onblur=\"if(this.value=='')this.value='# of Pallets'\" style='width: 98%;' /></td>";
    x += "<td class='border-right' align='center'><a href='#' class='td-add-btn' onclick='AddNewTicketLine();return false;'></a></td>";
    x += "</tr>";
    return x;
}

function AddNewTicketLine() {
    var material = $.trim($("#tb_Material_add").val());
    var coilsToCut = $.trim($("#tb_CoilsToCut_add").val());
    var coilWeight = $.trim($("#tb_CoilWeight_add").val());
    var gauge = $.trim($("#tb_Gauge_add").val());
    var coilNumber = $.trim($("#tb_CoilNumber_add").val());
    var width = $.trim($("#tb_Width_add").val());

    var orderDate = $.trim($("#tb_duedate_add").val());
    var customer = $.trim($("#tb_customer_add").val());
    var orderNum = $.trim($("#tb_orderNum_add").val());
    var lineNum = $.trim($("#tb_lineNum_add").val());
    var numofLifts = $.trim($("#tb_NumofLifts_add").val());
    var numofPallets = $.trim($("#tb_NumofPallets_add").val());

    var sequence = $("#EventList-steeltrucks").find(".myItemStyle").length;

    $.ajax({
        url: openWSE.siteRoot() + "Apps/CTLReport/CTLReportService.asmx/AddNewLine",
        type: "POST",
        data: '{ "line": "' + lineNum + '","sequence": "' + sequence + '","gauge": "' + gauge + '","material": "' + material + '","width": "' + width + '","coilNumber": "' + coilNumber + '","coilWeight": "' + coilWeight + '","customer": "' + customer + '","orderDate": "' + orderDate + '","weight": "' + '","orderNumber": "' + orderNum + '","date": "' + date + '","docNumber": "' + docNumber + '","orderDate": "' + orderDate + '","orderDate": "' + orderDate + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            if (response != null) {
                if ($("#ctlreport-search-tab").css("display") == "none") {
                    BuildMainTable(data, false);
                    if ($.trim($("#ctl-report-search-holder").html()) != "") {
                        $("#ctl-report-search-holder").html("<h3>Loading Data. Please Wait...</h3>");
                    }
                }
                else {
                    BuildMainTable(data, true);
                    if ($.trim($("#ctl-report-holder").html()) != "") {
                        $("#ctl-report-holder").html("<h3>Loading Data. Please Wait...</h3>");
                    }
                }

                InitializeTextBoxes_CTLReport();
                LoadFontSize_CTLReport();
            }
        },
        error: function (data) {
            $("#errorPullingRecords-ctlreport").html("There was a problem trying to get your records!");
        }
    });
}