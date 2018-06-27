function EditOverlay(id) {
    loadingPopup.Message("Loading Controls...");
    document.getElementById("hf_EditOverlay").value = id;
    openWSE.CallDoPostBack("hf_EditOverlay", "");
}

function UpdateOverlay() {
    if (currOverlay) {
        loadingPopup.Message("Updating Overlay...");

        document.getElementById("hf_appAssocationList_added").value = appAssocationList_added;
        document.getElementById("hf_appAssocationList_removed").value = appAssocationList_removed;

        document.getElementById("hf_EditOverlay").value = currOverlay;
        document.getElementById("hf_UpdateDescOverlay").value = $("#tb_udpatedesc").val();
        document.getElementById("hf_displayType").value = $("#dd_displayTypeUpdate").val();
        document.getElementById("hf_UpdateOverlayName").value = $("#tb_udpatename").val();
        openWSE.CallDoPostBack("hf_UpdateOverlayName", "");
    }
}

var currOverlay = "";
function EditAssociation(id) {
    currOverlay = id;
    appAssocationList_added = "";
    appAssocationList_removed = "";
    openWSE.LoadModalWindow(true, "App-element", "Edit Overlay");
}

var appAssocationList_added = "";
var appAssocationList_removed = "";
function AddAssociation(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-expand-sml");
    $div.removeClass("img-expand-sml");
    $div.addClass("img-collapse-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("AddAssociation", "RemoveAssociation");
    $a.attr("onclick", click);
    $("#App-element").find("#package-added").prepend($this);

    appAssocationList_added = appAssocationList_added.replace(id + ";", "");
    appAssocationList_removed = appAssocationList_removed.replace(id + ";", "");
    appAssocationList_added += id + ";";
}
function RemoveAssociation(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-collapse-sml");
    $div.removeClass("img-collapse-sml");
    $div.addClass("img-expand-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("RemoveAssociation", "AddAssociation");
    $a.attr("onclick", click);
    $("#App-element").find("#package-removed").prepend($this);

    appAssocationList_added = appAssocationList_added.replace(id + ";", "");
    appAssocationList_removed = appAssocationList_removed.replace(id + ";", "");
    appAssocationList_removed += id + ";";
}

var newAppAssocationList_checked = "";
function AddAssociation_New(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-expand-sml");
    $div.removeClass("img-expand-sml");
    $div.addClass("img-collapse-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("AddAssociation_New", "RemoveAssociation_New");
    $a.attr("onclick", click);
    $("#AddOverlay-element").find("#package-added").prepend($this);

    newAppAssocationList_checked = newAppAssocationList_checked.replace(id + ";", "");
    newAppAssocationList_checked += id + ";";

    $("#hf_newAppAssocationList_Checked").val(escape(newAppAssocationList_checked));
}
function RemoveAssociation_New(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-collapse-sml");
    $div.removeClass("img-collapse-sml");
    $div.addClass("img-expand-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("RemoveAssociation_New", "AddAssociation_New");
    $a.attr("onclick", click);
    $("#AddOverlay-element").find("#package-removed").prepend($this);

    newAppAssocationList_checked = newAppAssocationList_checked.replace(id + ";", "");
    newAppAssocationList_checked += id + ";";

    $("#hf_newAppAssocationList_Checked").val(escape(newAppAssocationList_checked));
}

function RefreshList() {
    currOverlay = "";
    loadingPopup.Message("Updating List...");
    document.getElementById("hf_refreshList").value = new Date().toString();
    openWSE.CallDoPostBack("hf_refreshList", "");
}

function DeleteOverlay(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
       function () {
           if (id != "") {
               loadingPopup.Message("Deleting Overlay...");
               document.getElementById("hf_DeleteOverlay").value = id;
               openWSE.CallDoPostBack("hf_DeleteOverlay", "");
           }
       }, null);
}

$(document.body).on("click", ".Upload-Button-Action", function () {
    var x = $("#MainContent_txt_uploadOverlayName").val();
    if (($.trim(x) != "") && (x != "Overlay Name")) {
        loadingPopup.Message("Uploading. Please wait...");
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
});
