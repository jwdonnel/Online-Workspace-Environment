function EditPackage(id) {
    currPackage = id;
    document.getElementById("hf_edit").value = id;
    loadingPopup.Message("Updating. Please Wait...");
    openWSE.CallDoPostBack("hf_edit", "");
}

var currPackage = "";
function AddApp(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-expand-sml");
    $div.removeClass("img-expand-sml");
    $div.addClass("img-collapse-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("AddApp", "RemoveApp");
    $a.attr("onclick", click);
    $("#PackageEdit-element").find("#package-added").prepend($this);

    var appAssocationList_added = $.trim($("#hf_appAssocationList_added").val());
    var appAssocationList_removed = $.trim($("#hf_appAssocationList_removed").val());

    appAssocationList_added = appAssocationList_added.replace(id + ";", "");
    appAssocationList_removed = appAssocationList_removed.replace(id + ";", "");
    appAssocationList_added += id + ";";

    $("#hf_appAssocationList_added").val(appAssocationList_added);
    $("#hf_appAssocationList_removed").val(appAssocationList_removed);
}
function RemoveApp(_this, id) {
    var $this = $(_this).closest(".app-icon-admin");
    $this.remove();
    var $div = $this.find(".img-collapse-sml");
    $div.removeClass("img-collapse-sml");
    $div.addClass("img-expand-sml");
    var $a = $this.find("a");
    var click = $a.attr("onclick");
    click = click.replace("RemoveApp", "AddApp");
    $a.attr("onclick", click);
    $("#PackageEdit-element").find("#package-removed").prepend($this);

    var appAssocationList_added = $.trim($("#hf_appAssocationList_added").val());
    var appAssocationList_removed = $.trim($("#hf_appAssocationList_removed").val());

    appAssocationList_added = appAssocationList_added.replace(id + ";", "");
    appAssocationList_removed = appAssocationList_removed.replace(id + ";", "");
    appAssocationList_removed += id + ";";

    $("#hf_appAssocationList_added").val(appAssocationList_added);
    $("#hf_appAssocationList_removed").val(appAssocationList_removed);
}

function RefreshList() {
    loadingPopup.Message("Updating List...");
    document.getElementById("hf_refreshList").value = new Date().toString();
    openWSE.CallDoPostBack("hf_refreshList", "");
}

function DeletePackageCategory(id, type) {
    openWSE.ConfirmWindow("Are you sure you want to delete this " + type + "?",
       function () {
           loadingPopup.Message("Updating. Please Wait...");
           document.getElementById("hf_delete").value = id;
           openWSE.CallDoPostBack("hf_delete", "");
       }, function () {
           window.setTimeout(function () { loadingPopup.RemoveMessage(); }, 1000);
       });
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
    $("#NewPackage-element").find("#package-added").prepend($this);

    newAppAssocationList_checked = newAppAssocationList_checked.replace(id + ",", "");
    newAppAssocationList_checked += id + ",";

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
    $("#NewPackage-element").find("#package-removed").prepend($this);

    newAppAssocationList_checked = newAppAssocationList_checked.replace(id + ",", "");
    newAppAssocationList_checked += id + ",";

    $("#hf_newAppAssocationList_Checked").val(escape(newAppAssocationList_checked));
}

var tempPackageAppList = "";
var tempPackagesAddedList = "";
var tempPackagesRemovedList = "";
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
    if ($("#NewPackage-element").css("display") == "block") {
        tempPackageAppList = $.trim($("#NewPackage-element").find("#pnl_appsInPackage").html());
        tempPackagesAddedList = $.trim($("#hf_newAppAssocationList_Checked").val());
    }
    else if ($("#PackageEdit-element").css("display") == "block") {
        tempPackagesAddedList = $.trim($("#hf_appAssocationList_added").val());
        tempPackagesRemovedList = $.trim($("#hf_appAssocationList_removed").val());
        document.getElementById("hf_edit").value = currPackage;
    }
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    if ($("#NewPackage-element").css("display") == "block") {
        $("#NewPackage-element").find("#pnl_appsInPackage").html(tempPackageAppList);
        $("#hf_newAppAssocationList_Checked").val(tempPackagesAddedList);
    }
    else if ($("#PackageEdit-element").css("display") == "block") {
        $("#hf_appAssocationList_added").val(tempPackagesAddedList);
        $("#hf_appAssocationList_removed").val(tempPackagesRemovedList);
        document.getElementById("hf_edit").value = currPackage;
    }

    tempPackageAppList = "";
    tempPackagesAddedList = "";
    tempPackagesRemovedList = "";
});
