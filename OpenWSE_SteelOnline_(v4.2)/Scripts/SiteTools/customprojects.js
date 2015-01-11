var tableHtml = "";

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function () {
    if (tableHtml != "") {
        $("#pnl_CustomPageList").html(tableHtml);
    }
});

function SaveTableHtml() {
    tableHtml = $.trim($("#pnl_CustomPageList").html());
}

$(document.body).on("keypress", "#tb_nameEdit, #tb_descriptionEdit", function (e) {
    var code = e.keyCode || e.which;
    if (code == 13) {
        e.preventDefault();
        UpdateCustomPage(currentEdit);
    }
});

function ConnectFTP() {
    $("#div_fileUpload, #MainContent_btn_uploadFile").css("visibility", "hidden");
    $("#div_ftpUpload, #btn_ftpUpload").show();
}

function UploadFiles() {
    $("#div_ftpUpload, #btn_ftpUpload").hide();
    $("#div_fileUpload, #MainContent_btn_uploadFile").css("visibility", "visible");
}

function TryFTPConnect() {
    var ftpLocation = $.trim($("#tb_ftpLocation").val());
    var ftpUsername = $.trim($("#tb_ftpUsername").val());
    var ftpPassword = $.trim($("#tb_ftpPassword").val());
    var name = $.trim($("#MainContent_txt_UploadName").val());

    if (ftpLocation != "" && ftpUsername != "" && ftpPassword != "" && name != "") {
        openWSE.LoadingMessage1("Attempting to Connect...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/FTPConnect.asmx/TryConnect",
            type: "POST",
            data: '{ "ftpLocation": "' + escape(ftpLocation) + '", "username": "' + escape(ftpUsername) + '", "password": "' + escape(ftpPassword) + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    AddFTPConnection();
                }
                else {
                    openWSE.AlertWindow("Failed to connect to the FTP address. Please make sure the FTP address is correct and that the username and password reflect that FTP location.");
                }
            },
            error: function () {
                openWSE.AlertWindow("Failed to connect to the FTP address. Please make sure the FTP address is correct and that the username and password reflect that FTP location.");
                openWSE.RemoveUpdateModal();
            }
        });
    }
}
function AddFTPConnection() {
    var ftpLocation = $.trim($("#tb_ftpLocation").val());
    var name = $.trim($("#MainContent_txt_UploadName").val());
    var description = $.trim($("#MainContent_tb_descriptionUpload").val());

    if (ftpLocation != "" && name != "") {
        openWSE.LoadingMessage1("Adding FTP Location...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/FTPConnect.asmx/AddCustomFTPLocation",
            type: "POST",
            data: '{ "ftpLocation": "' + escape(ftpLocation) + '", "name": "' + escape(name) + '", "description": "' + escape(description) + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
                if (data.d != "") {
                    window.location.href = window.location.href.split("?")[0] + "?ProjectID=" + data.d;
                }
                else {
                    openWSE.AlertWindow("There was an error adding your new Custom Project. Please try again.");
                }
            },
            error: function () {
                openWSE.AlertWindow("There was an error adding your new Custom Project. Please try again.");
                openWSE.RemoveUpdateModal();
            }
        });
    }
}

$(window).resize(function () {
    var $this = $(".loaderApp-overlay").find(".loaderApp-element-modal");
    var mLeft = -($this.width() / 2);
    var mTop = -($this.height() / 2);
    $(".loaderApp-overlay").find(".loaderApp-element-align").css({
        marginLeft: mLeft,
        marginTop: mTop
    });
});

function LoadDefaultPageSelector() {
    $(".loaderApp-overlay").show();
    $(window).resize();
}

function DownloadCP(id) {
    $("#hf_download").val(id);
    $("#btn_download_hidden").trigger("click");
}

function DeleteCustomPage(id, name) {
    tableHtml = "";
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
        function () {
            openWSE.LoadingMessage1("Deleting...");
            $("#hf_DeleteCP").val(id);
            __doPostBack("hf_DeleteCP", "");
        }, null);
}

function EditCustomPage(id) {
    openWSE.LoadIFrameContent("SiteSettings/iframes/ProjectExplorer.aspx?projectId=" + id, this); return false;
}

function ClosePageEditor() {
    var url = location.href;
    if (url.indexOf("ProjectExplorer") == -1) {
        openWSE.LoadingMessage1("Updating...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        __doPostBack("hf_refreshList", "");
    }
}

$(function () {
    $(window).hashchange(function () {
        ClosePageEditor();
    });
});