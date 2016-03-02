var changesMade = false;
function EditNotifi(id) {
    openWSE.LoadingMessage1("Loading Controls...");
    document.getElementById("hf_EditNotifi").value = id;
    __doPostBack("hf_EditNotifi", "");
}

function UpdateNotifi(id) {
    openWSE.LoadingMessage1("Updating Notification...");
    document.getElementById("hf_EditNotifi").value = id;
    document.getElementById("hf_UpdateImgNotifi").value = $("#tb_udpateImg").val();
    document.getElementById("hf_UpdateDescNotifi").value = $("#tb_udpatedesc").val();
    document.getElementById("hf_UpdateNameNotifi").value = $("#tb_udpatename").val();
    __doPostBack("hf_UpdateNameNotifi", "");
}

var currOverlay = "";
function EditAssociation(id) {
    currOverlay = id;
    openWSE.LoadingMessage1("Loading Controls...");
    document.getElementById("hf_AssociationNotifi").value = id;
    __doPostBack("hf_AssociationNotifi", "");
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
    document.getElementById("hf_NotifiID").value = currOverlay;
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
    document.getElementById("hf_NotifiID").value = currOverlay;
    document.getElementById("hf_removeapp").value = id;
    __doPostBack("hf_removeapp", "");
}

function DeleteNotifi(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "?",
       function () {
           if (id != "") {
               openWSE.LoadingMessage1("Deleting Notification...");
               document.getElementById("hf_DeleteNotifi").value = id;
               __doPostBack("hf_DeleteNotifi", "");
           }
       }, null);
}

$(document.body).on("click", ".Upload-Button-Action", function () {
    openWSE.LoadingMessage1("Adding Notification...");
});

$(document.body).on("click", "#cb_UseAppImg", function () {
    if ($(this).attr("checked") == "checked") {
        $("#txt_uploadNofiImg").attr("disabled", "disabled");
    }
    else {
        $("#txt_uploadNofiImg").removeAttr("disabled");
    }
});

function RefreshList() {
    currOverlay = "";
    if (changesMade) {
        changesMade = false;
        openWSE.LoadingMessage1("Updating List...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        __doPostBack("hf_refreshList", "");
    }
}

