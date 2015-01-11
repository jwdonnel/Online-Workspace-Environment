var currLine_ctlLogs = "";
var editLine_ctlLogs = "";

var editMode_beginRequest_ctlLogs = false;
var materialUsed_PCS_beginRequest = "";
var materialUsed_Size_beginRequest = "";
var micNumber_beginRequest = "";
var heatNumber_beginRequest = "";
var soNumber_beginRequest = "";
var sizeProduced_beginRequest = "";
var totalPieces_beginRequest = "";
var restockWeight_beginRequest = "";

$(document).ready(function () {
    LoadFontSize_CTLLogs();
    var d = new Date();
    $("#tb_date_ctllogsheet").val((d.getMonth() + 1) + "/" + d.getDate() + "/" + d.getFullYear());
    LoadCTLLogs();
});

$(document.body).on("click", "#app-ctllogsheet .exit-button-app, #app-ctllogsheet-min-bar .exit-button-app-min", function () {
    cookie.del("ctllogs-fontsize");
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_beginRequest(function (sender, args) {
    try {
        if ($("#ctllogsheet-load").find(".ctllog-edit-line-mode").length > 0) {
            materialUsed_PCS_beginRequest = escape($("#tb_materialUsed_pcs_ctllogs").val());
            materialUsed_Size_beginRequest = escape($("#tb_materialUsed_size_ctllogs").val());
            micNumber_beginRequest = escape($("#tb_micheatnum_mic_ctllogs").val());
            heatNumber_beginRequest = escape($("#tb_micheatnum_heat_ctllogs").val());
            soNumber_beginRequest = escape($("#tb_soNumber_ctllogs").val());
            sizeProduced_beginRequest = escape($("#tb_sizeProduced_ctllogs").val());
            totalPieces_beginRequest = escape($("#tb_totalPcs_ctllogs").val());
            restockWeight_beginRequest = escape($("#tb_coilWtRestock_ctllogs").val());
            editMode_beginRequest_ctlLogs = true;
        }
    }
    catch (evt) { }
});

prm.add_endRequest(function (sender, args) {
    PageStartUp_CTLLogs();
    LoadCTLLogs();
    $("#tb_date_ctllogsheet").datepicker();
    $("#update-element-ctllogsheets").remove();
});

function PageStartUp_CTLLogs() {
    if ($.trim($("#tb_date_ctllogsheet").val()) == "") {
        var d = new Date();
        $("#tb_date_ctllogsheet").val((d.getMonth() + 1) + "/" + d.getDate() + "/" + d.getFullYear());
    }
}

function ExportToExcel_CTLLogs() {
    if (currLine_ctlLogs == "") {
        currLine_ctlLogs = "1";
    }

    $.ajax({
        url: openWSE.siteRoot() + "Apps/CTLLogSheet/CTLLogSheet.asmx/ExportToExcel",
        data: "{ '_date': '" + $("#tb_date_ctllogsheet").val() + "','_line': '" + currLine_ctlLogs + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data1) {
            if (data1.d != "") {
                $.fileDownload(data1.d);
            }
            else {
                openWSE.AlertWindow("No records to export for CTL Line " + currLine_ctlLogs);
            }
        }
    });
}

function LoadingCTLLogs(message) {
    var x = "<div id='update-element-ctllogsheets'><div class='update-element-overlay' style='position: absolute!important'><div class='update-element-align' style='position: absolute!important'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>";
    x += message + "</h3></div></div></div></div>";
    $("#ctllogsheet-load").append(x);
    $("#update-element-ctllogsheets").show();
}

$(document.body).on("click", "#ctlLine1-li-ctllogsheet", function () {
    if (!$("#ctlLine1-li-ctllogsheet").hasClass("active")) {
        $("#ctlLine1-li-ctllogsheet").addClass("active");
        $("#ctlLine2-li-ctllogsheet").removeClass("active");
        LoadCTLLogs();
    }
});

$(document.body).on("click", "#ctlLine2-li-ctllogsheet", function () {
    if (!$("#ctlLine2-li-ctllogsheet").hasClass("active")) {
        $("#ctlLine2-li-ctllogsheet").addClass("active");
        $("#ctlLine1-li-ctllogsheet").removeClass("active");
        LoadCTLLogs();
    }
});

function InitializeTextBoxes_CTLLogs() {
    $("#tb_date_ctllogsheet").datepicker();
    $("#tb_employee_ctllogsheets").autocomplete({
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
}

function GetCurrCTLLine_CTLLogs() {
    currLine_ctlLogs = "";
    if ($("#ctlLine1-li-ctllogsheet").hasClass("active")) {
        currLine_ctlLogs = "1";
    }
    else {
        currLine_ctlLogs = "2"
    }
}





/* Load Table
----------------------------------*/
function DateChange() {
    editMode_beginRequest_ctlLogs = false;
    materialUsed_PCS_beginRequest = "";
    materialUsed_Size_beginRequest = "";
    micNumber_beginRequest = "";
    heatNumber_beginRequest = "";
    soNumber_beginRequest = "";
    sizeProduced_beginRequest = "";
    totalPieces_beginRequest = "";
    restockWeight_beginRequest = "";
    editLine_ctlLogs = "";
    LoadCTLLogs();
}

function LoadCTLLogs() {
    LoadingCTLLogs("Loading...");
    var date = $("#tb_date_ctllogsheet").val();
    GetCurrCTLLine_CTLLogs();
    $("#print-schedule-ctllogsheet").attr("href", "Apps/CTLLogSheet/CTLLogPrint.aspx?date=" + date + "&line=" + currLine_ctlLogs);
    $("#errorPullingRecords-ctllogsheet").html("");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/CTLLogSheet/CTLLogSheet.asmx/LoadReport",
        type: "POST",
        data: '{ "date": "' + date + '","line": "' + currLine_ctlLogs + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var response = data.d;
            $("#update-element-ctllogsheets").remove();
            if (response != null) {
                BuildMainTable_CTLLogs(data);
                InitializeTextBoxes_CTLLogs();
                LoadFontSize_CTLLogs();
            }
        },
        error: function (data) {
            $("#update-element-ctllogsheets").remove();
            $("#errorPullingRecords-ctllogsheet").html("There was a problem trying to get your records!");
        }
    });
}

function BuildMainTable_CTLLogs(data) {
    var $holder = $("#ctl-logs-holder");
    var x = "<table id='table-ctl-logs' cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>";
    x += BuildCTLLineRowHeader_CTLLogs();
    for (var i = 0; i < data.d[0].length; i++) {
        x += BuildCTLLineRow_CTLLogs(data.d[0][i]);
    }

    x += "</tbody></table>";

    if (data.d[0].length == 0) {
        x += "<div class='emptyGridView'>No Schedule Available</div>";
        $("#tb_employee_ctllogsheets").prop("disabled", true);
        $("#ddl_shift_ctllogsheets").prop("disabled", true);
        $("#btn_updateheader_ctllogsheets").hide();
    }
    else {
        $("#docNumber_ctllogsheet").html(data.d[1][0]);
        $("#revision_ctllogsheet").html(data.d[1][1]);
        $("#approvedBy_ctllogsheet").html(data.d[1][2]);

        $("#tb_employee_ctllogsheets").prop("disabled", false);
        $("#ddl_shift_ctllogsheets").prop("disabled", false);
        $("#btn_updateheader_ctllogsheets").show();

        $("#tb_employee_ctllogsheets").val($.trim(data.d[2][0]));

        var shift = $.trim(data.d[2][1]);
        if (shift != "") {
            $("#ddl_shift_ctllogsheets").val(shift);
        }
    }

    $holder.html(x);
}

function BuildCTLLineRowHeader_CTLLogs() {
    var x = "<tr class='myHeaderStyle'>";
    x += "<td>Coil #</td>";
    x += "<td style='width: 125px;'>Material Used</td>";
    x += "<td style='width: 125px;'>Mic/Heat #</td>";
    x += "<td>Customer</td>";
    x += "<td style='width: 75px;'>SO #</td>";
    x += "<td style='width: 115px;'>Size Produced</td>";
    x += "<td style='width: 100px;'>Total Pieces</td>";
    x += "<td>Coil Wt/Restock</td>";
    x += "<td style='width: 70px;'></td>";
    x += "</tr>";
    return x;
}

function BuildCTLLineRow_CTLLogs(data) {
    var muPcs = "";
    var muSize = "";
    var micNum = "";
    var heatNum = "";
    var soNumber = "";
    var sizeProd = "";
    var totalPcs = "";
    var restockWt = "";

    if (data[3] != null) {
        muPcs = data[4];
        muSize = data[5];
        micNum = data[6];
        heatNum = data[7];
        soNumber = data[8];
        sizeProd = data[9];
        totalPcs = data[10];
        restockWt = data[11];
    }

    var x = "";
    if ((data[0] == editLine_ctlLogs) && (editLine_ctlLogs != "")) {
        var add_updateBtn = "<a href='#Save' class='pad-all-sml margin-right img-backup' onclick='SaveCTLLine_ctlLogs(\"" + data[0] + "\");return false;' title='Save'></a>";
        if (data[3] != null) {
            add_updateBtn = "<a href='#Save' class='pad-all-sml margin-right img-backup' onclick='UpdateCTLLine_ctlLogs(\"" + data[0] + "\", \"" + data[3] + "\");return false;' title='Save'></a>";
            x = "<tr class='ctllog-edit-line-mode myItemStyle GridNormalRow' style='background: #F9F9F9; border-bottom: 1px solid #AAA!important;'>";
        }
        else {
            x = "<tr class='myItemStyle GridNormalRow' style='border-bottom: 1px solid #AAA!important;'>";
        }

        if (editMode_beginRequest_ctlLogs) {
            muPcs = materialUsed_PCS_beginRequest;
            muSize = materialUsed_Size_beginRequest;
            micNum = micNumber_beginRequest;
            heatNum = heatNumber_beginRequest;
            soNumber = soNumber_beginRequest;
            sizeProd = sizeProduced_beginRequest;
            totalPcs = totalPieces_beginRequest;
            restockWt = restockWeight_beginRequest;
            editMode_beginRequest_ctlLogs = false;
            materialUsed_PCS_beginRequest = "";
            materialUsed_Size_beginRequest = "";
            micNumber_beginRequest = "";
            heatNumber_beginRequest = "";
            soNumber_beginRequest = "";
            sizeProduced_beginRequest = "";
            totalPieces_beginRequest = "";
            restockWeight_beginRequest = "";
        }

        var cancelBtn = "<a href='#Cancel' class='td-cancel-btn' onclick='CancelEditCTLLine_ctlLogs();return false;' title='Cancel Edit'></a>";

        x += "<td class='border-right' align='center'><span class='font-bold'>" + data[1] + "</span></td>";
        x += "<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left pad-top-sml'>PCS:</b><input id='tb_materialUsed_pcs_ctllogs' class='textEntry float-right' type='text' value='" + muPcs + "' style='width: 70px;' /><div class='clear-space-five'></div><div style='border-bottom: 1px solid #CCC;'></div><div class='clear-space-five'></div><b class='pad-right float-left pad-top-sml'>Size:</b><input id='tb_materialUsed_size_ctllogs' class='textEntry float-right' type='text' value='" + muSize + "' style='width: 70px;' /><div class='clear-space'></div></td>";
        x += "<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left pad-top-sml'>Mic:</b><input id='tb_micheatnum_mic_ctllogs' class='textEntry float-right' type='text' value='" + micNum + "' style='width: 70px;' /><div class='clear-space-five'></div><div style='border-bottom: 1px solid #CCC;'></div><div class='clear-space-five'></div><b class='pad-right float-left pad-top-sml'>Heat:</b><input id='tb_micheatnum_heat_ctllogs' class='textEntry float-right' type='text' value='" + heatNum + "' style='width: 70px;' /><div class='clear-space'></div></td>";
        x += "<td class='border-right' align='center'><span class='font-bold'>" + data[2] + "</span></td>";
        x += "<td class='border-right' align='center'><input id='tb_soNumber_ctllogs' class='textEntry' type='text' value='" + soNumber + "' style='width: 98%;' /></td>";
        x += "<td class='border-right' align='center'><input id='tb_sizeProduced_ctllogs' class='textEntry' type='text' value='" + sizeProd + "' style='width: 98%;' /></td>";
        x += "<td class='border-right' align='center'><input id='tb_totalPcs_ctllogs' class='textEntry' type='text' value='" + totalPcs + "' style='width: 98%;' /></td>";
        x += "<td class='border-right' align='center'><input id='tb_coilWtRestock_ctllogs' class='textEntry' type='text' value='" + restockWt + "' style='width: 98%;' /></td>";
        x += "<td class='border-right' align='center'>" + add_updateBtn + cancelBtn + "</td>";
        x += "</tr>";
    }
    else {
        var editBtn = "<a href='#Edit' class='td-edit-btn' onclick='EditCTLLine_CTLLogs(\"" + data[0] + "\");return false;' title='Edit Row'></a>";
        x = "<tr class='myItemStyle GridNormalRow' style='border-bottom: 1px solid #AAA!important;'>";
        x += "<td class='border-right' align='center'>" + data[1] + "</td>";
        x += "<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>PCS:</b><span class='float-right'>" + muPcs + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #CCC;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Size:</b><span class='float-right'>" + muSize + "</span><div class='clear-space'></div></td>";
        x += "<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>Mic:</b><span class='float-right'>" + micNum + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #CCC;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Heat:</b><span class='float-right'>" + heatNum + "</span><div class='clear-space'></div></td>";
        x += "<td class='border-right' align='center'>" + data[2] + "</td>";
        x += "<td class='border-right' align='center'>" + soNumber + "</td>";
        x += "<td class='border-right' align='center'>" + sizeProd + "</td>";
        x += "<td class='border-right' align='center'>" + totalPcs + "</td>";
        x += "<td class='border-right' align='center'>" + restockWt + "</td>";
        x += "<td class='border-right' align='center'>" + editBtn + "</td>";
        x += "</tr>";
    }

    return x;
}





/* Edit and Save Controls
----------------------------------*/
function SaveCTLLine_ctlLogs(id) {
    if ((id != "") && (id != undefined) && (id != "undefined")) {
        var date = $("#tb_date_ctllogsheet").val();
        var materialUsed_PCS = escape($("#tb_materialUsed_pcs_ctllogs").val());
        var materialUsed_Size = escape($("#tb_materialUsed_size_ctllogs").val());
        var micNumber = escape($("#tb_micheatnum_mic_ctllogs").val());
        var heatNumber = escape($("#tb_micheatnum_heat_ctllogs").val());
        var soNumber = escape($("#tb_soNumber_ctllogs").val());
        var sizeProduced = escape($("#tb_sizeProduced_ctllogs").val());
        var totalPieces = escape($("#tb_totalPcs_ctllogs").val());
        var restockWeight = escape($("#tb_coilWtRestock_ctllogs").val());

        var employee = escape($("#tb_employee_ctllogsheets").val());
        var shift = escape($("#ddl_shift_ctllogsheets").val());

        GetCurrCTLLine_CTLLogs();
        LoadingCTLLogs("Saving Row...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/CTLLogSheet/CTLLogSheet.asmx/SaveRow",
            type: "POST",
            data: '{ "id": "' + escape(id) + '","line": "' + currLine_ctlLogs + '","materialUsed_PCS": "' + materialUsed_PCS + '","materialUsed_Size": "' + materialUsed_Size + '","micNumber": "' + micNumber + '","heatNumber": "' + heatNumber + '","soNumber": "' + soNumber + '","sizeProduced": "' + sizeProduced + '","totalPieces": "' + totalPieces + '","restockWeight": "' + restockWeight + '","employee": "' + employee + '","shift": "' + shift + '","date": "' + date + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#update-element-ctllogsheets").remove();
                if (data.d == "") {
                    editLine_ctlLogs = "";
                    LoadCTLLogs();
                }
                else {
                    openWSE.AlertWindow("Cannot save entry because Employee Name and Shift are not valid!");
                }
            },
            error: function (data) {
                $("#update-element-ctllogsheets").remove();
                $("#errorPullingRecords-ctllogsheet").html("There was a problem trying to update your row!");
            }
        });
    }
}

function UpdateCTLLine_ctlLogs(schId, id) {
    if ((id != "") && (id != undefined) && (id != "undefined")) {
        var date = $("#tb_date_ctllogsheet").val();
        var materialUsed_PCS = escape($("#tb_materialUsed_pcs_ctllogs").val());
        var materialUsed_Size = escape($("#tb_materialUsed_size_ctllogs").val());
        var micNumber = escape($("#tb_micheatnum_mic_ctllogs").val());
        var heatNumber = escape($("#tb_micheatnum_heat_ctllogs").val());
        var soNumber = escape($("#tb_soNumber_ctllogs").val());
        var sizeProduced = escape($("#tb_sizeProduced_ctllogs").val());
        var totalPieces = escape($("#tb_totalPcs_ctllogs").val());
        var restockWeight = escape($("#tb_coilWtRestock_ctllogs").val());

        var employee = escape($("#tb_employee_ctllogsheets").val());
        var shift = escape($("#ddl_shift_ctllogsheets").val());

        GetCurrCTLLine_CTLLogs();
        LoadingCTLLogs("Saving Row...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/CTLLogSheet/CTLLogSheet.asmx/UpdateRow",
            type: "POST",
            data: '{ "id": "' + escape(id) + '","schId": "' + escape(schId) + '","line": "' + currLine_ctlLogs + '","materialUsed_PCS": "' + materialUsed_PCS + '","materialUsed_Size": "' + materialUsed_Size + '","micNumber": "' + micNumber + '","heatNumber": "' + heatNumber + '","soNumber": "' + soNumber + '","sizeProduced": "' + sizeProduced + '","totalPieces": "' + totalPieces + '","restockWeight": "' + restockWeight + '","employee": "' + employee + '","shift": "' + shift + '","date": "' + date + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#update-element-ctllogsheets").remove();
                if (data.d == "") {
                    editLine_ctlLogs = "";
                    LoadCTLLogs();
                }
                else {
                    openWSE.AlertWindow("Cannot save entry because Employee Name and Shift are not valid!");
                }
            },
            error: function (data) {
                $("#update-element-ctllogsheets").remove();
                $("#errorPullingRecords-ctllogsheet").html("There was a problem trying to update your row!");
            }
        });
    }
}

function EditCTLLine_CTLLogs(id) {
    if ((id != "") && (id != undefined) && (id != "undefined")) {
        editLine_ctlLogs = id;
        LoadCTLLogs();
    }
}

function CancelEditCTLLine_ctlLogs() {
    editLine_ctlLogs = "";
    editMode_beginRequest_ctlLogs = false;
    materialUsed_PCS_beginRequest = "";
    materialUsed_Size_beginRequest = "";
    micNumber_beginRequest = "";
    heatNumber_beginRequest = "";
    soNumber_beginRequest = "";
    sizeProduced_beginRequest = "";
    totalPieces_beginRequest = "";
    restockWeight_beginRequest = "";
    LoadCTLLogs();
}

function UpdateHeader_CTLLogs() {
    var date = $("#tb_date_ctllogsheet").val();

    var employee = escape($("#tb_employee_ctllogsheets").val());
    var shift = escape($("#ddl_shift_ctllogsheets").val());

    GetCurrCTLLine_CTLLogs();
    LoadingCTLLogs("Saving Header Info...");
    $.ajax({
        url: openWSE.siteRoot() + "Apps/CTLLogSheet/CTLLogSheet.asmx/UpdateHeader",
        type: "POST",
        data: '{ "date": "' + date + '","line": "' + currLine_ctlLogs + '","employee": "' + employee + '","shift": "' + shift + '" }',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#update-element-ctllogsheets").remove();
            if (data.d != "") {
                openWSE.AlertWindow("Cannot save entry because Employee Name and Shift are not valid!");
            }
        },
        error: function (data) {
            $("#update-element-ctllogsheets").remove();
            $("#errorPullingRecords-ctllogsheet").html("There was a problem trying to update your row!");
        }
    });
}




/* Load Fonts
----------------------------------*/
function LoadFontSize_CTLLogs() {
    var fontsize = cookie.get("ctllogs-fontsize");
    if ((fontsize != null) && (fontsize != "")) {
        $("#ctllogsheet-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_ctllogsheet input").css("font-size", fontsize);
        $("#font-size-selector-ctllogsheet option").each(function () {
            if ($(this).val() == fontsize) {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
    else {
        $("#font-size-selector-ctllogsheet option").each(function () {
            if ($(this).val() == "small") {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
}

function FontSelection_CTLLogs() {
    var fontsize = $("#font-size-selector-ctllogsheet").val();
    $("#ctllogsheet-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_ctllogsheet input").css("font-size", fontsize);
    cookie.set("ctllogs-fontsize", fontsize, "30");
}