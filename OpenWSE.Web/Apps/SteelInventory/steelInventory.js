var steelInventory = function () {
    function InitializeAutoComplete() {
        var $mainTable = $(".gridview-table[data-tableid='MainInventory']");
        $mainTable.find("input").each(function () {
            if ($(this).attr("id") !== "txt_Image_Insert" && $(this).attr("data-name") !== "Image") {
                $(this).autocomplete({
                    minLength: 0,
                    autoFocus: false,
                    source: function (request, response) {
                        var arr = [];
                        var colName = this.element.parent().attr("data-columnname");
                        $mainTable.find(".myItemStyle > td[data-columnname='" + colName + "']").each(function () {
                            var val = $.trim($(this).html());
                            var foundItem = false;
                            for (var i = 0; i < arr.length; i++) {
                                if (arr[i] === val) {
                                    foundItem = true;
                                    break;
                                }
                            }

                            if (!foundItem && val !== null && val.indexOf("<input") !== 0 && val.toLowerCase().indexOf(request.term.toLowerCase()) !== -1) {
                                arr.push(val);
                            }
                        });

                        response($.map(arr, function (item) {
                            return {
                                label: item,
                                value: item
                            }
                        }));
                    }
                });
            }
        });
    }

    function UpdateQuantity(_this) {
        var initValue = $.trim($(_this).attr("data-initvalue"));
        var newValue = $.trim($(_this).val());
        if (initValue !== newValue) {
            loadingPopup.Message("Updating...");
            $("#hf_UpdateQuantityId").val($(_this).attr("data-id"));
            $("#hf_UpdateQuantity").val(newValue);
            openWSE.CallDoPostBack("hf_UpdateQuantity", "");
        }
    }
    function OnQuantityKeyPress(_this, event) {
        try {
            if (event.which == 13) {
                event.preventDefault();
                UpdateQuantity(_this);
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                event.preventDefault();
                UpdateQuantity(_this);
            }
            delete evt;
        }
    }
    
    function AddItem() {
        loadingPopup.Message("Loading...");

        var info = {};
        var $row = $(".gridview-table[data-tableid='MainInventory']").find("tr.addItemRow");
        if ($row.length > 0) {
            $row.find("input").each(function () {
                info[$(this).parent().attr("data-columnname")] = $(this).val();
            });

            info["Image"] = $("#img_Image_Insert").attr("data-src");

            if (!info["Gauge"] || !info["Grade"]) {
                openWSE.AlertWindow("Gauge and Grade cannot be empty.");
                return;
            }
        }

        $("#hf_AddItem").val(escape(JSON.stringify(info)));
        openWSE.CallDoPostBack("hf_AddItem", "");
    }

    function EditItem(id) {
        if (id) {
            loadingPopup.Message("Loading...");
            $("#hf_EditItem").val(id);
            openWSE.CallDoPostBack("hf_EditItem", "");
        }
    }
    function DeleteItem(id) {
        if (id) {
            openWSE.ConfirmWindow("Are you sure you want to delete this item?", function () {
                loadingPopup.Message("Deleting...");
                $("#hf_DeleteItem").val(id);
                openWSE.CallDoPostBack("hf_DeleteItem", "");
            }, null);
        }
    }
    function UpdateItem(id) {
        loadingPopup.Message("Loading...");

        var info = {
            ID: id
        };
        var $row = $(".gridview-table[data-tableid='MainInventory']").find("tr.myItemStyle[data-id='" + id + "']");
        if ($row.length > 0) {
            $row.find("input").each(function () {
                info[$(this).attr("data-name")] = $(this).val();
            });

            info["Image"] = $row.find("img[data-name='Image']").attr("data-src");
        }

        $("#hf_UpdateItem").val(escape(JSON.stringify(info)));
        openWSE.CallDoPostBack("hf_UpdateItem", "");
    }
    function CancelItem() {
        loadingPopup.Message("Loading...");
        $("#hf_EditItem").val("CancelUpdate");
        openWSE.CallDoPostBack("hf_EditItem", "");
    }

    var fromEdit = false;
    function onInventorySelectClick() {
        fromEdit = true;
        openWSE.LoadModalWindow(true, 'InventoryUpload-element', 'Select Image', "#pnl_InventoryList");
    }
    function onInventoryImageClick(imageId) {
        if (fromEdit) {
            $("#img_EditInventoryImg").attr("data-src", imageId);
            $("#img_EditInventoryImg").attr("src", "Images/" + imageId);
        }
        else {
            $("#img_Image_Insert").attr("data-src", imageId);
            $("#img_Image_Insert").attr("src", "Images/" + imageId);
        }

        openWSE.LoadModalWindow(false, 'InventoryUpload-element', '');
    }
    $(document.body).on("click", "#img_Image_Insert", function () {
        fromEdit = false;
        openWSE.LoadModalWindow(true, 'InventoryUpload-element', 'Select Image', "#pnl_InventoryList");
    });

    $(document.body).on("click", ".steel-inventory-image", function () {
        var str = "<div class='steelinventory-image-preview-overlay' onclick='steelInventory.ClosePreview();' style='display: none;'>";

        var name = $(this).attr("src").split("/");
        name = name[name.length - 1];

        str += "<div class='steelinventory-image-preview'>";
        str += "<h3 class='float-left'>" + name + "</h3>";
        str += "<a href='#' class='float-right img-close-dark' onclick='steelInventory.ClosePreview();'></a>";
        str += "<div class='clear-space'></div>";
        str += "<img alt='' src='" + $(this).attr("src") + "' /></div>";
        str += "</div>";

        $("body").append(str);
        $(".steelinventory-image-preview-overlay").fadeIn(openWSE.animationSpeed);
    });
    function ClosePreview() {
        $(".steelinventory-image-preview-overlay").fadeOut(openWSE.animationSpeed, function () {
            $(".steelinventory-image-preview-overlay").remove();
        });
    }

    function UploadImage() {
        var formData = new FormData();
        formData.append("postedfile", $("#FileUploadControl")[0].files[0]);

        loadingPopup.Message("Loading...");
        $.ajax({
            url: openWSE.siteRoot() + "Apps/SteelInventory/UploadInventoryImage.ashx",
            data: formData,
            type: 'POST',
            contentType: false,
            processData: false,
            complete: function (data) {
                $("#FileUploadControl").val("");
                if ($("#img_EditInventoryImg").length > 0) {
                    $("#hf_RefreshImageList").val($("#img_EditInventoryImg").closest("tr.myItemStyle").attr("data-id"));
                }
                else {
                    $("#hf_RefreshImageList").val("Refresh");
                }
                openWSE.CallDoPostBack("hf_RefreshImageList", "");
            }
        });
    }
    function DeleteImage(name) {
        openWSE.ConfirmWindow("Are you sure you want to delete this image?", function () {
            loadingPopup.Message("Loading...");
            if ($("#img_EditInventoryImg").length > 0) {
                $("#hf_DeleteImage_EditId").val($("#img_EditInventoryImg").closest("tr.myItemStyle").attr("data-id"));
            }

            $("#hf_DeleteImage").val(name);
            openWSE.CallDoPostBack("hf_DeleteImage", "");
        }, null);
    }

    return {
        InitializeAutoComplete: InitializeAutoComplete,
        UpdateQuantity: UpdateQuantity,
        OnQuantityKeyPress: OnQuantityKeyPress,
        AddItem: AddItem,
        EditItem: EditItem,
        DeleteItem: DeleteItem,
        UpdateItem: UpdateItem,
        CancelItem: CancelItem,
        onInventorySelectClick: onInventorySelectClick,
        onInventoryImageClick: onInventoryImageClick,
        ClosePreview: ClosePreview,
        UploadImage: UploadImage,
        DeleteImage: DeleteImage
    }

}();

$(document).ready(function () {
    steelInventory.InitializeAutoComplete();
});

var arrEditVals = {};
var arrNewVals = {};
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function () {
    arrEditVals = {};
    if ($("tr.steelinventory-editrow").length > 0) {
        $("tr.steelinventory-editrow").find("input").each(function () {
            arrEditVals[$(this).attr("data-name")] = $(this).val();
        });
        arrEditVals["ImageSrc"] = $("tr.steelinventory-editrow").find("#img_EditInventoryImg").attr("src");
        arrEditVals["ImageDataSrc"] = $("tr.steelinventory-editrow").find("#img_EditInventoryImg").attr("data-src");
    }

    arrNewVals = {};
    if ($("tr.addItemRow").length > 0) {
        $("tr.addItemRow").find("input").each(function () {
            arrNewVals[$(this).parent().attr("data-columnname")] = $(this).val();
        });
        arrNewVals["ImageSrc"] = $("tr.addItemRow").find("#img_Image_Insert").attr("src");
        arrNewVals["ImageDataSrc"] = $("tr.addItemRow").find("#img_Image_Insert").attr("data-src");
    }
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    for (var key1 in arrEditVals) {
        if (key1 === "ImageSrc") {
            $("tr.steelinventory-editrow").find("#img_EditInventoryImg").attr("src", arrEditVals[key1]);
        }
        else if (key1 === "ImageDataSrc") {
            $("tr.steelinventory-editrow").find("#img_EditInventoryImg").attr("data-src", arrEditVals[key1]);
        }
        else {
            $("tr.steelinventory-editrow").find("input[data-name='" + key1 + "']").val(arrEditVals[key1]);
        }
    }

    for (var key2 in arrNewVals) {
        if (key2 === "ImageSrc") {
            $("tr.addItemRow").find("#img_Image_Insert").attr("src", arrNewVals[key2]);
        }
        else if (key2 === "ImageDataSrc") {
            $("tr.addItemRow").find("#img_Image_Insert").attr("data-src", arrNewVals[key2]);
        }
        else {
            $("tr.addItemRow").find("td[data-columnname='" + key2 + "']").find("input").val(arrNewVals[key2]);
        }
    }

    arrEditVals = {};
    arrNewVals = {};

    steelInventory.InitializeAutoComplete();
});