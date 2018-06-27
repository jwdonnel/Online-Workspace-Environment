var commonCarriers = function () {
    var customerAutoCompleteData = new Array();
    var trucklineAutoCompleteData = new Array();
    var trucknumberAutoCompleteData = new Array();

    function InitializeAutoComplete() {
        LoadAutoCompleteData();
        $(".datepicker").datepicker();
    }
    function LoadAutoCompleteData() {
        var vals = $.trim($("#hf_AutoCompleteList").val());
        vals = unescape(vals);
        vals = vals.replace(/~/g, " ");
        var data = JSON.parse(vals);

        if (data.length === 3) {
            trucklineAutoCompleteData = data[0];
            customerAutoCompleteData = data[1];
            trucknumberAutoCompleteData = data[2];

            $("#tb_TruckLine_Insert, .cc-edit-control[data-column='TruckLine']").autocomplete({
                minLength: 1,
                autoFocus: true,
                source: function (request, response) {
                    var tempArr = SearchAutoCompleteList(trucklineAutoCompleteData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
            }).focus(function () {
                $(this).autocomplete("search", "");
            });

            $("#tb_Customer_Insert, .cc-edit-control[data-column='Customer']").autocomplete({
                minLength: 1,
                autoFocus: true,
                source: function (request, response) {
                    var tempArr = SearchAutoCompleteList(customerAutoCompleteData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
            }).focus(function () {
                $(this).autocomplete("search", "");
            });

            $("#tb_TruckNumber_Insert, .cc-edit-control[data-column='TruckNumber']").autocomplete({
                minLength: 1,
                autoFocus: true,
                source: function (request, response) {
                    var tempArr = SearchAutoCompleteList(trucknumberAutoCompleteData, request.term);
                    response($.map(tempArr, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }));
                }
            }).focus(function () {
                $(this).autocomplete("search", "");
            });
        }
    }
    function SearchAutoCompleteList(arr, val) {
        var newArr = new Array();
        if (val === "") {
            newArr = arr;
        }
        else {
            val = val.toLowerCase();
            for (var i = 0; i < arr.length; i++) {
                if (arr[i].toLowerCase().indexOf(val) !== -1) {
                    newArr.push(arr[i]);
                    if (newArr.length === 20) {
                        break;
                    }
                }
            }
        }

        newArr.sort();
        return newArr;
    }
    function ResizeScreen() {
        var h = $(window).height() - $(".app-title-bg-color").outerHeight();
        $(".content-overflow-app").css("height", h - 25);
    }

    $(document.body).on("change", "#dd_MonthSelector, #dd_YearSelector", function () {
        loadingPopup.Message("Loading...");
    });

    function LoadGenDirEditor() {
        if ($("#DirEdit-element").css("display") != "block") {
            openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/BuildEditor", "{ }", null, function (data) {
                $("#GenDirList-steeltrucks").html(data.d);
                openWSE.LoadModalWindow(true, "DirEdit-element", "Direction Editor");
            }, function (data) {
                openWSE.AlertWindow("There was an error loading direction list. Please try again");
            });
        } else {
            openWSE.LoadModalWindow(false, "DirEdit-element", "");
            setTimeout(function () {
                $("#GenDirList-steeltrucks").html("");
                CancelItem();
            }, openWSE_Config.animationSpeed);
        }
    }

    function InsertItem() {
        loadingPopup.Message("Loading...");

        var info = {};
        var $row = $(".gridview-table[data-tableid='CommonCarriersTable']").find("tr.addItemRow");
        if ($row.length > 0) {
            $row.find("input").each(function () {
                info[$(this).parent().attr("data-columnname").replace(/ /g, "")] = $(this).val();
            });

            info["Direction"] = $("#dd_Direction_Insert").val();

            if (info["TruckLine"] === "") {
                openWSE.AlertWindow("Truck Line cannot be empty");
                return;
            }
        }

        $("#hf_InsertItem").val(escape(JSON.stringify(info)));
        openWSE.CallDoPostBack("hf_InsertItem", "");
    }
    function EditItem(id) {
        loadingPopup.Message("Loading...");
        document.getElementById("hf_EditItem").value = id;
        openWSE.CallDoPostBack("hf_EditItem", "");
    }
    function DeleteItem(id) {
        openWSE.ConfirmWindow("Are you sure you want to delete this item?", function () {
            loadingPopup.Message("Deleting...");
            document.getElementById("hf_DeleteItem").value = id;
            openWSE.CallDoPostBack("hf_DeleteItem", "");
        }, null);
    }
    function CancelItem() {
        loadingPopup.Message("Loading...");
        document.getElementById("hf_EditItem").value = new Date().toString();
        openWSE.CallDoPostBack("hf_EditItem", "");
    }
    function UpdateItem(id) {
        loadingPopup.Message("Loading...");

        var info = {};
        var $row = $(".gridview-table[data-tableid='CommonCarriersTable']").find("tr.myItemStyle[data-id='" + id + "']");
        if ($row.length > 0) {
            info["ID"] = id;
            $row.find(".cc-edit-control").each(function () {
                info[$(this).parent().attr("data-columnname").replace(/ /g, "")] = $(this).val();
            });

            if (info["TruckLine"] === "") {
                openWSE.AlertWindow("Truck Line cannot be empty");
                return;
            }
        }

        $("#hf_UpdateItem").val(escape(JSON.stringify(info)));
        openWSE.CallDoPostBack("hf_UpdateItem", "");
    }
    function UpdateItem_KeyPress(event, id) {
        if (event.which == 13 || event.keyCode == 13) {
            UpdateItem(id);
        }
    }

    function LoadTodayTotals() {
        loadingPopup.Message("Loading...");
        $("#updatepnl_ect_steeltrucks").html("Loading totals...");
        openWSE.LoadModalWindow(true, "DailyTotals-element", "Weight Totals for Today");
        $("#hf_LoadTodayTotals").val(new Date().toString());
        openWSE.CallDoPostBack("hf_LoadTodayTotals", "");
    }

    return {
        InitializeAutoComplete: InitializeAutoComplete,
        ResizeScreen: ResizeScreen,
        LoadGenDirEditor: LoadGenDirEditor,
        InsertItem: InsertItem,
        EditItem: EditItem,
        DeleteItem: DeleteItem,
        CancelItem: CancelItem,
        UpdateItem: UpdateItem,
        UpdateItem_KeyPress: UpdateItem_KeyPress,
        LoadTodayTotals: LoadTodayTotals
    }

}();

var steelTruckFunctions = function () {

    function deletegd(id, dir) {
        openWSE.ConfirmWindow("Are you sure you want to delete this General Direction? (Any associated schedule will default to Kansas.)",
            function () {
                openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/DeleteDirection", "{ 'id': '" + id + "','gendir': '" + escape(dir) + "' }", null, function (data) {
                    if (!openWSE.ConvertBitToBoolean(data.d)) {
                        openWSE.AlertWindow("Error deleting direction. Please try again.");
                    } else {
                        $("#GenDirList-steeltrucks").html(data.d);
                        BuildGeneralDirections();
                    }
                }, function (data) {
                    openWSE.AlertWindow("There was an error deleting direction. Please try again");
                });
            }, null);
    }
    function addgd() {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/AddDirection", "{ 'gendir': '" + escape(document.getElementById("tb_addgendir").value) + "' }", null, function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            }
            else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            }
            else {
                $("#GenDirList-steeltrucks").html(data.d);
                BuildGeneralDirections();
            }
        }, function (data) {
            openWSE.AlertWindow("There was an error adding direction. Please try again");
        });
    }
    function editgd(dir) {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/EditDirection", "{ 'gendir': '" + escape(dir) + "' }", null, function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        }, function (data) {
            openWSE.AlertWindow("There was an error editing direction. Please try again");
        });
    }
    function updategd(id) {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/UpdateDirection", "{ 'id':'" + id + "','gendir': '" + escape(document.getElementById("tb_editgendir").value) + "' }", null, function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Direction Already Exists</b>");
            } else if (data.d == "blank") {
                $("#gderror").html("<b style='color: Red; font-size: 11px;'>Cannot Add Blank</b>");
            } else {
                $("#GenDirList-steeltrucks").html(data.d);
            }
        }, function (data) {
            openWSE.AlertWindow("There was an error updating direction. Please try again");
        });
    }
    function canceleditgd() {
        openWSE.AjaxCall("WebServices/UpdateGeneralDirections.asmx/BuildEditor", "{ }", null, function (data) {
            $("#GenDirList-steeltrucks").html(data.d);
        }, function (data) {
            openWSE.AlertWindow("There was an error loading direction list. Please try again");
        });
    }
    function OnKeyPress_DirEditNew(event, id) {
        try {
            if (event.which == 13) {
                if (id == "") {
                    addgd();
                }
                else {
                    updategd(id);
                }
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                if (id == "") {
                    addgd();
                }
                else {
                    updategd(id);
                }
            }
            delete evt;
        }
    }

    function BuildGeneralDirections() {
        openWSE.AjaxCall("Apps/SteelTrucks/SteelTrucks.asmx/BuildGeneralDirections", "", null, function (data) {
            var response = data.d;
            if (response != null) {
                genDirArray = new Array();
                for (var i = 0; i < response.length; i++) {
                    genDirArray[i] = response[i];
                }
            }
        });
    }

    return {
        addgd: addgd,
        editgd: editgd,
        deletegd: deletegd,
        canceleditgd: canceleditgd,
        updategd: updategd,
        OnKeyPress_DirEditNew: OnKeyPress_DirEditNew
    }

}();

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
    commonCarriers.InitializeAutoComplete();
    commonCarriers.ResizeScreen();

    if ($("#DailyTotals-element").is(":visible")) {
        $("#DailyTotals-element").find(".Modal-element-align").css({
            marginTop: -($("#DailyTotals-element").find(".Modal-element-modal").height() / 2),
            marginLeft: -($("#DailyTotals-element").find(".Modal-element-modal").width() / 2)
        });

        openWSE.AdjustModalWindowView();
    }
});
$(window).resize(function () {
    commonCarriers.ResizeScreen();
});
$(document).ready(function () {
    commonCarriers.InitializeAutoComplete();
    commonCarriers.ResizeScreen();
});
