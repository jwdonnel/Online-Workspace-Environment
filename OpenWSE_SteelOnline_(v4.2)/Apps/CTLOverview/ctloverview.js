var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    LoadInit_CTLOverview();
});

$(document).ready(function () {
    LoadInit_CTLOverview();
    LoadFontSize_CTLOverview();
});

function LoadInit_CTLOverview() {
    $("#tb_date_ctloverview").datepicker();
    $("#tb_search_ctloverview").autocomplete({
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

function RefreshCTLOverview() {
    openWSE.LoadingMessage1("Loading Data...");
    $("#hf_search_ctloverview").val($("#tb_search_ctloverview").val());
    $("#hf_refresh_ctloverview").val(new Date().toString());
    __doPostBack("hf_refresh_ctloverview", "");
}

function ExportToExcel_CTLOverview() {
    $("#hf_export_ctloverview").val(new Date().toString());
    __doPostBack("hf_export_ctloverview", "");
}

function KeyPressSearch_CTLOverview(event) {
    try {
        if (event.which == 13) {
            RefreshCTLOverview();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            RefreshCTLOverview();
        }
        delete evt;
    }
}



/* Load Fonts
----------------------------------*/
function LoadFontSize_CTLOverview() {
    var fontsize = cookie.get("ctloverview-fontsize");
    if ((fontsize != null) && (fontsize != "")) {
        $("#ctloverview-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_CTLOverview input").css("font-size", fontsize);
        $("#font-size-selector-ctloverview option").each(function () {
            if ($(this).val() == fontsize) {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
    else {
        $("#font-size-selector-ctloverview option").each(function () {
            if ($(this).val() == "small") {
                $(this).attr('selected', 'selected');
            }
            else {
                $(this).removeAttr('selected');
            }
        });
    }
}

function FontSelection_CTLOverview() {
    var fontsize = $("#font-size-selector-ctloverview").val();
    $("#ctloverview-load").find(".GridNormalRow, .GridAlternate, .myHeaderStyle, #ReportViewer_CTLOverview input").css("font-size", fontsize);
    cookie.set("ctloverview-fontsize", fontsize, "30");
}