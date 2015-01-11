function EditPackage(id) {
    currPackage = id;
    changesMade = false;
    document.getElementById("hf_edit").value = id;
    openWSE.LoadingMessage1("Updating. Please Wait...");
    __doPostBack("hf_edit", "");
}

var changesMade = false;
var currPackage = "";
function AddApp(_this, id) {
    changesMade = true;
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-expand-sml");
    $div.removeClass("img-expand-sml");
    $div.addClass("img-collapse-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("AddApp", "RemoveApp");
    $a.attr("onclick", click);
    $("#package-added").prepend($this);

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_currPackage").value = currPackage;
    document.getElementById("hf_addapp").value = id;
    __doPostBack("hf_addapp", "");
}

function RemoveApp(_this, id) {
    changesMade = true;
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-collapse-sml");
    $div.removeClass("img-collapse-sml");
    $div.addClass("img-expand-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("RemoveApp", "AddApp");
    $a.attr("onclick", click);
    $("#package-removed").prepend($this);

    openWSE.LoadingMessage1("Updating...");
    document.getElementById("hf_currPackage").value = currPackage;
    document.getElementById("hf_removeapp").value = id;
    __doPostBack("hf_removeapp", "");
}

function RefreshList() {
    if (changesMade) {
        changesMade = false;
        openWSE.LoadingMessage1("Updating List...");
        document.getElementById("hf_refreshList").value = new Date().toString();
        __doPostBack("hf_refreshList", "");
    }
}

function DeletePackageCategory(id, type) {
    openWSE.ConfirmWindow("Are you sure you want to delete this " + type + "?",
       function () {
           openWSE.LoadingMessage1("Updating. Please Wait...");
           document.getElementById("hf_delete").value = id;
           __doPostBack("hf_delete", "");
       }, function () {
           window.setTimeout(function () { openWSE.RemoveUpdateModal(); }, 1000);
       });
}