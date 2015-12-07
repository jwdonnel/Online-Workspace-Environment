var tableHtml = "";

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    if (tableHtml != "") {
        $("#pnl_WebServiceList").html(tableHtml);
    }
});

function SaveTableHtml() {
    tableHtml = $.trim($("#pnl_WebServiceList").html());
}

var currentEdit = "";
$(document.body).on("keypress", "#tb_filenameEdit, #tb_descriptionEdit", function (e) {
    var code = e.keyCode || e.which;
    if (code == 13) {
        e.preventDefault();
        UpdateWebService(currentEdit);
    }
});

$(document.body).on("change", "#cb_hideStandardServices", function () {
    openWSE.LoadingMessage1("Updating List...");
});

function DownloadWS(id) {
    $("#hf_wsId").val(id);
    $("#btn_download_hidden").trigger("click");
}

function DeleteWebService(id, name) {
    tableHtml = "";
    currentEdit = "";
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
      function () {
          openWSE.LoadingMessage1("Deleting Web Service...");
          $("#hf_DeleteWS").val(id);
          __doPostBack("hf_DeleteWS", "");
      }, null);
}

function EditWebService(id) {
    tableHtml = "";
    currentEdit = id;
    openWSE.LoadingMessage1("Loading Controls...");
    $("#hf_EditWS").val(id);
    __doPostBack("hf_EditWS", "");
}

function UpdateWebService(id) {
    tableHtml = "";
    currentEdit = "";
    openWSE.LoadingMessage1("Updating Web Service...");
    $("#hf_UpdateWS").val(id);
    $("#hf_FilenameEdit").val($("#tb_filenameEdit").val());
    $("#hf_DescriptionEdit").val($("#tb_descriptionEdit").val());
    __doPostBack("hf_UpdateWS", "");
}

function CancelWebService() {
    tableHtml = "";
    currentEdit = "";
    openWSE.LoadingMessage1("Loading Controls...");
    $("#hf_CancelWS").val(new Date().toString());
    __doPostBack("hf_CancelWS", "");
}