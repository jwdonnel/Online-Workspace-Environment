var changesMade = false;
function EditOverlay(id) {
    openWSE.LoadingMessage1("Loading Controls...");
    document.getElementById("hf_EditOverlay").value = id;
    __doPostBack("hf_EditOverlay", "");
}

function UpdateOverlay(id) {
    openWSE.LoadingMessage1("Updating Overlay...");
    document.getElementById("hf_EditOverlay").value = id;
    document.getElementById("hf_UpdateDescOverlay").value = $("#tb_udpatedesc").val();
    document.getElementById("hf_displayType").value = $("#dd_displayTypeUpdate").val();
    document.getElementById("hf_UpdateNameOverlay").value = $("#tb_udpatename").val();
    __doPostBack("hf_UpdateNameOverlay", "");
}

var currOverlay = "";
function EditAssociation(id) {
    currOverlay = id;
    openWSE.LoadingMessage1("Loading Controls...");
    document.getElementById("hf_AssociationOverlay").value = id;
    __doPostBack("hf_AssociationOverlay", "");
}

function AddAssociation(_this, id) {
    changesMade = true;
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-expand-sml");
    $div.removeClass("img-expand-sml");
    $div.addClass("img-collapse-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("AddAssociation", "RemoveAssociation");
    $a.attr("onclick", click);
    $("#package-added").prepend($this);

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_OverlayID").value = currOverlay;
    document.getElementById("hf_addapp").value = id;
    __doPostBack("hf_addapp", "");
}

function RemoveAssociation(_this, id) {
    changesMade = true;
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-collapse-sml");
    $div.removeClass("img-collapse-sml");
    $div.addClass("img-expand-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("RemoveAssociation", "AddAssociation");
    $a.attr("onclick", click);
    $("#package-removed").prepend($this);

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_OverlayID").value = currOverlay;
    document.getElementById("hf_removeapp").value = id;
    __doPostBack("hf_removeapp", "");
}

function RefreshList() {
    currOverlay = "";
    if (changesMade) {
        changesMade = false;
        openWSE.LoadingMessage1("Updating List...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        __doPostBack("hf_refreshList", "");
    }
}

function DeleteOverlay(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
       function () {
           if (id != "") {
               openWSE.LoadingMessage1("Deleting Overlay...");
               document.getElementById("hf_DeleteOverlay").value = id;
               __doPostBack("hf_DeleteOverlay", "");
           }
       }, null);
}

$(document.body).on("click", ".Upload-Button-Action", function () {
    var x = $("#MainContent_txt_uploadOverlayName").val();
    if (($.trim(x) != "") && (x != "Overlay Name")) {
        openWSE.LoadingMessage1("Uploading. Please wait...");
    }
    else {
        return false;
    }
});

$(document.body).on("change", "#MainContent_fileupload_Overlay", function () {
    var fu = $("#MainContent_fileupload_Overlay").val().toLowerCase();
    if ((fu.indexOf(".ascx") != -1) || (fu.indexOf(".zip") != -1)) {
        $("#MainContent_btn_uploadOverlay").removeAttr("disabled");
        $("#lbl_uploadMessage").html("");
    }
    else {
        $("#MainContent_btnMainContent_btn_uploadOverlay_uploadPlugin").attr("disabled", "disabled");
        $("#lbl_uploadMessage").html("<span style='color:Red'>File type not valid. Must be an ASP.Net user control file (.ascx)</span>");
    }
    setTimeout(function () { $("#lbl_uploadMessage").html(""); }, 5000);
});

$(window).load(function () {
    document.title = "Overlay Manager";
});