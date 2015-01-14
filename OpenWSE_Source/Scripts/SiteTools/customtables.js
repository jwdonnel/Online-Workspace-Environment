var columns = new Array();
$(document).ready(function () {
    AddIDRow();
});

$(function () {
    $("#table_columns").sortable({
        cancel: ".columnIDRow, .border-right",
        containment: "#sortContainer",
        cursor: "move",
        stop: function (event, ui) {
            columns = new Array();
            var index = 0;
            $("#table_columns .myItemStyle").each(function () {
                var $this = $(this);
                var $thisName = $this.find(".column-name");
                var $thisType = $this.find(".column-type");
                var $thisNullable = $this.find(".column-nullable");
                var name = $thisName.text();
                if ((name.toLowerCase() != "columnid") && (name.toLowerCase() != "timestamp")) {
                    var type = $thisType.text();
                    var nullable = $thisNullable.find("#cb_nullable_row").is(":checked");

                    var colRow = new Array();
                    colRow[0] = name;
                    colRow[1] = type;
                    colRow[2] = nullable;

                    columns[index] = colRow;
                    index++;
                }
            });

            $("#table_columns").html("");
            AddIDRow();
            for (var i = 0; i < columns.length; i++) {
                BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 3);
            }
        }
    });
});

function DeleteTable(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "? If you select ok you will need to supply the correct password for the user who created the table.",
       function () {
           if (id != "") {
               $("#hf_tableDeleteID").val(id);
               $("#db_overlay").css("display", "block");
               $("#db_overlay").css("visibility", "visible");
               $("#db_modal").css("display", "block");
               $("#db_modal").css("opacity", "1.0");
               $("#db_modal").css("filter", "alpha(opacity=100)");
               $("#hf_buRestore_type").val("RestoreLast");
               $("#MainContent_tb_passwordConfirm").focus();
           }
       }, null);
}

function CancelDelete() {
    $("#db_overlay").css("display", "none");
    $("#db_overlay").css("visibility", "hidden");
    $("#db_modal").fadeOut(300);
    $("#hf_tableDeleteID").val("");
    $("#MainContent_tb_passwordConfirm").val("");
}

function PerformDelete(id) {
    $("#db_overlay").css("display", "none");
    $("#db_overlay").css("visibility", "hidden");
    $("#db_modal").fadeOut(300);
    openWSE.LoadingMessage1("Deleting Table. Please Wait...");
    $("#hf_tableDelete").val(id);
    __doPostBack("hf_tableDelete", "");
}

var tableEditing = "";
function EditTable(table, name) {
    openWSE.LoadingMessage1("Getting Information. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/GetCustomTableList",
        data: "{ 'table': '" + table + "' }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            if (data.d == null) {
                $("#successMessage").html("<span style='color: Red;'>An error occured while trying to pull your table.</span>");
                setTimeout(function () {
                    $("#successMessage").html("");
                }, 3500);
                openWSE.RemoveUpdateModal();
                tableEditing = "";
            }
            else {
                $("#tb_tablename").val(name);
                $("#tb_columnName").val("");
                $("#ddl_datatypes").val("nvarchar");
                $("#cb_nullable").prop("checked", false);
                columns = data.d;

                AddIDRow();
                for (var i = 0; i < columns.length; i++) {
                    BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 3);
                }

                $("#createBtns").hide();
                $(".table-action-btns").hide();
                $(".table-action-cb").prop('disabled', true);
                $("#Divcb_installHolder").hide();
                $("#editBtns").show();
                tableEditing = table;

                openWSE.RemoveUpdateModal();
            }
        },
        error: function (data) {
            $("#successMessage").html("<span style='color: Red;'>An error occured while trying to pull your table.</span>");
            setTimeout(function () {
                $("#successMessage").html("");
            }, 3500);
            openWSE.RemoveUpdateModal();
            tableEditing = "";
        }
    });
}

function CancelUpdate() {
    $("#tb_tablename").val("");
    $("#tb_columnName").val("");
    $("#ddl_datatypes").val("nvarchar");
    $("#cb_nullable").prop("checked", false);
    columns = new Array();

    AddIDRow();
    for (var i = 0; i < columns.length; i++) {
        BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
    }

    $("#createBtns").show();
    $(".table-action-btns").show();
    $("#Divcb_installHolder").show();
    $(".table-action-cb").prop('disabled', false);
    $("#editBtns").hide();

    tableEditing = "";
}

$(document.body).on("change", "#MainContent_cb_InstallAfterLoad", function () {
    if ($(this).is(":checked")) {
        $("#div_isPrivate").show();
    }
    else {
        $("#div_isPrivate").hide();
    }
});

function UpdateTable() {

    openWSE.ConfirmWindow("Are you sure you want to update this table? Any changes will be permanent.",
       function () {
           if (columns.length == 0) {
               openWSE.AlertWindow("Must have more than 1 column for this table.");
           }
           else if ($.trim($("#tb_tablename").val()) == "") {
               openWSE.AlertWindow("Tablename cannot be blank.");
           }
           else {
               openWSE.LoadingMessage1("Updating Table. Please Wait...");
               $.ajax({
                   type: "POST",
                   url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/Update",
                   data: JSON.stringify({ "columns": columns, "tableName": $("#tb_tablename").val(), "tableID": tableEditing }),
                   contentType: "application/json; charset=utf-8",
                   dataType: "json",
                   cache: false,
                   success: function (data) {
                       if (!openWSE.ConvertBitToBoolean(data.d)) {
                           $("#successMessage").html("<span style='color: Red;'>An error occured while trying to update your table.</span>");
                           setTimeout(function () {
                               $("#successMessage").html("");
                           }, 3500);
                           openWSE.RemoveUpdateModal();
                       }
                       else {
                           setTimeout(function () {
                               $("#successMessage").html("<span style='color: Green;'>Your custom table has been updated.</span>");
                               setTimeout(function () {
                                   $("#successMessage").html("");
                               }, 3500);

                               $("#tb_tablename").val("");
                               $("#tb_columnName").val("");
                               $("#ddl_datatypes").val("nvarchar");
                               $("#cb_nullable").prop("checked", false);
                               columns = new Array();

                               AddIDRow();
                               for (var i = 0; i < columns.length; i++) {
                                   BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
                               }

                               $("#createBtns").show();
                               $(".table-action-btns").show();
                               $("#Divcb_installHolder").show();
                               $("#editBtns").hide();

                               openWSE.RemoveUpdateModal();
                               tableEditing = "";

                               $("#hf_tableUpdate").val(new Date().toString());
                               __doPostBack("hf_tableUpdate", "");
                           }, 2000);
                       }
                   },
                   error: function (data) {
                       $("#successMessage").html("<span style='color: Red;'>An error occured while trying to update your table.</span>");
                       setTimeout(function () {
                           $("#successMessage").html("");
                       }, 3500);
                       openWSE.RemoveUpdateModal();
                       tableEditing = "";
                   }
               });
           }
       }, null);
}




/*
 * Table Creator
 */
function AddIDRow() {
    var primaryKey = ""; // "<span class='img-password margin-right float-left' title='Primary Key'></span>";

    var column = "<table cellspacing='0' cellpadding='0' style='width: 625px!important; border-collapse: collapse;'>";
    column += "<tr class='GridNormalRow columnIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
    column += "<td width='45px' align='center' class='GridViewNumRow'>1</td>";
    column += "<td class='border-right'><span class='pad-left'><span class='column-name'>" + primaryKey + "ColumnID</span></span></td>";
    column += "<td class='border-right column-type' align='center' width='135px'>uniqueidentifier</td>";
    column += "<td class='border-right column-nullable' align='center' width='75px'><input id='cb_nullable_row' type='checkbox' disabled /></td>";
    column += "<td class='border-right' align='center' width='75px'><div style='padding: 4px;'>N/A</div></td>";
    column += "</tr></table></td></tr></table>";

    column += "<table cellspacing='0' cellpadding='0' style='width: 625px!important; border-collapse: collapse;'>";
    column += "<tr class='GridNormalRow columnIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
    column += "<td width='45px' align='center' class='GridViewNumRow'>2</td>";
    column += "<td class='border-right'><span class='pad-left'><span class='column-name'>TimeStamp</span></span></td>";
    column += "<td class='border-right column-type' align='center' width='135px'>DateTime</td>";
    column += "<td class='border-right column-nullable' align='center' width='75px'><input id='cb_nullable_row' type='checkbox' disabled /></td>";
    column += "<td class='border-right' align='center' width='75px'><div style='padding: 4px;'>N/A</div></td>";
    column += "</tr></table></td></tr></table>";

    $("#table_columns").html(column);
}

function BuildColumn(name, type, nullable, inEditMode, isEditVal, columnNum) {
    var nameEdit = name;
    var typeEdit = type;
    var nullableEdit = nullable;
    if (isEditVal) {
        var disabledControlName = "";
        if (tableEditing != "") {
            disabledControlName = "disabled='disabled' ";
        }

        name = "<input type='text' id='tb_columnName_edit' class='textEntry' onkeydown='FinishEditKeyDown(event)' maxlength='100' " + disabledControlName + "style='width: 170px;' />";
        type = "<select id='ddl_datatypes_edit'><option value='nvarchar'>nvarchar</option><option value='DateTime'>DateTime</option><option value='Integer'>Integer</option><option value='Decimal'>Decimal</option><option value='Boolean'>Boolean</option></select>";
        nullable = "<input id='cb_nullable_edit' type='checkbox' />";
    }
    else {
        if (openWSE.ConvertBitToBoolean(nullableEdit)) {
            nullable = "<input id='cb_nullable_row' type='checkbox' checked='checked' disabled />";
        }
        else {
            nullable = "<input id='cb_nullable_row' type='checkbox' disabled />";
        }
    }

    var column = "<table cellspacing='0' cellpadding='5' style='width: 625px!important; border-collapse: collapse;'>";
    column += "<tr class='GridNormalRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
    column += "<td width='45px' align='center' class='GridViewNumRow'>" + columnNum + "</td>";
    column += "<td class='border-right'><span class='pad-left'><span class='column-name'>" + name + "</span></td>";
    column += "<td class='border-right column-type' align='center' width='135px'>" + type + "</td>";
    column += "<td class='border-right column-nullable' align='center' width='75px'>" + nullable + "</td>";

    var actionButton = "";
    if (!inEditMode) {
        actionButton = "<a href='#edit' onclick='EditControls(\"" + name + "\");return false;' class='td-edit-btn margin-right' title='Edit'></a>";
        actionButton += "<a href='#delete' onclick='DeleteRow(\"" + columnNum + "\");return false;' class='td-delete-btn' title='Delete'></a>";
    }

    if (isEditVal) {
        actionButton = "<a href='#update' onclick='FinishEdit(\"" + columnNum + "\", \"" + nameEdit + "\", false);return false;' class='td-update-btn margin-right' title='Update'></a>";
        actionButton += "<a href='#cancel' onclick='FinishEdit(\"" + columnNum + "\", \"" + nameEdit + "\", true);return false;' class='td-cancel-btn' title='Cancel'></a>";
    }

    column += "<td class='border-right' align='center' width='75px'>" + actionButton + "</td>";
    column += "</tr></table></td></tr></table>";

    $("#table_columns").append(column);
    $("#tb_columnName").val("");

    if (isEditVal) {
        $("#tb_columnName_edit").val(nameEdit);
        $("#ddl_datatypes_edit").val(typeEdit);

        if (openWSE.ConvertBitToBoolean(nullableEdit)) {
            $("#cb_nullable_edit").prop("checked", true);
        }
        else {
            $("#cb_nullable_edit").prop("checked", false);
        }
    }
}


// Input Controls
function AddNewColumn() {
    var len = columns.length;
    var newColumn = $.trim($("#tb_columnName").val());
    newColumn = FormatColumnName(newColumn);
    $("#tb_columnName").val(newColumn);

    if (newColumn == "") {
        openWSE.AlertWindow("Column name cannot be blank. Please enter a name and try again.");
    }
    else if ((newColumn.toLowerCase() == "columnid") || (newColumn.toLowerCase() == "timestamp")) {
        openWSE.AlertWindow("Column already exists! Please use a different name.");
    }
    else {
        var canContinue = true;
        for (var i = 0; i < len; i++) {
            if ((columns[i][0].toLowerCase() == newColumn.toLowerCase()) || (newColumn.toLowerCase() == "columnid") || (newColumn.toLowerCase() == "timestamp")) {
                canContinue = false;
                openWSE.AlertWindow("Column already exists! Please use a different name.");
            }
        }

        if (canContinue) {
            var dataType = $("#ddl_datatypes").val();
            var nullable = $("#cb_nullable").prop('checked').toString();

            var colRow = new Array();
            colRow[0] = newColumn;
            colRow[1] = dataType;
            colRow[2] = nullable;

            columns[len] = colRow;

            BuildColumn(newColumn, dataType, nullable, false, false, len + 3);

            $("#tb_columnName").focus();
        }
    }
}

function AddNewRowTextBoxKeyDown(event) {
    try {
        if (event.which == 13) {
            AddNewColumn();
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            AddNewColumn();
        }
        delete evt;
    }
}

function EditControls(name) {
    $("#table_columns").sortable("disable");
    AddIDRow();
    for (var i = 0; i < columns.length; i++) {
        if ((columns[i][0].toLowerCase() == name.toLowerCase()) && (name.toLowerCase() != "columnid") && (name.toLowerCase() != "timestamp")) {
            BuildColumn(columns[i][0], columns[i][1], columns[i][2], true, true, i + 3);
        }
        else {
            BuildColumn(columns[i][0], columns[i][1], columns[i][2], true, false, i + 3);
        }
    }
}

function FinishEditKeyDown(event) {
    try {
        if (event.which == 13) {
            FinishEdit(false);
        }
    }
    catch (evt) {
        if (event.keyCode == 13) {
            FinishEdit(false);
        }
        delete evt;
    }
}

function FinishEdit(index, name, cancelled) {
    var canContinue = true;
    index = (parseInt(index) - 3);
    if (!cancelled) {
        var columnName = $.trim($("#tb_columnName_edit").val());
        columnName = FormatColumnName(columnName);
        $("#tb_columnName_edit").val(columnName);

        for (var i = 0; i < columns.length; i++) {
            if (((columns[i][0].toLowerCase() == columnName.toLowerCase()) && (index != i)) || (columnName.toLowerCase() == "columnid") || (columnName.toLowerCase() == "timestamp")) {
                canContinue = false;
                openWSE.AlertWindow("Column already exists! Please use a different name.");
            }
        }

        if (canContinue) {
            columns[index][0] = columnName;
            columns[index][1] = $("#ddl_datatypes_edit").val();
            columns[index][2] = $("#cb_nullable_edit").prop('checked').toString();
        }
    }

    if (canContinue) {
        $("#table_columns").sortable("enable");
        AddIDRow();
        for (var i = 0; i < columns.length; i++) {
            BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 3);
        }
    }
}

function DeleteRow(index) {
    openWSE.ConfirmWindow("Are you sure you want to delete this column?",
        function () {
            index = (parseInt(index) - 3);
            columns.splice(index, 1);
            if (columns.length == 0) {
                columns = new Array();
            }
        }, null);

    AddIDRow();
    var colNum = 2;
    for (var i = 0; i < columns.length; i++) {
        if (i == 0) {
            colNum += 1;
        }
        BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, colNum);
        colNum++;
    }
}

function ClearControls() {
    openWSE.ConfirmWindow("Are you sure you want to start a new custom table without saving this one?",
        function () {
            $("#tb_tablename").val("");
            $("#tb_columnName").val("");
            $("#ddl_datatypes").val("nvarchar");
            $("#cb_nullable").prop("checked", false);
            columns = new Array();

            AddIDRow();
            for (var i = 0; i < columns.length; i++) {
                BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
            }
        }, null);
}

function FormatColumnName(name) {
    name = name.replace(/[|&#;$%@"<>()+,]/g, "");
    name = name.replace(/\[/g, "");
    name = name.replace(/\]/g, "");
    name = name.replace(/ /g, "_");

    return name;
}

function RecreateApp(id, tableName, sidebar) {
    openWSE.LoadingMessage1("Creating Table. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/RecreateApp",
        data: "{ 'appId': '" + id + "','tableName': '" + tableName + "','showSidebar': '" + sidebar + "' }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            if (!openWSE.ConvertBitToBoolean(data.d)) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow("An error occured while trying to create the app.");
            }
            else {
                openWSE.RemoveUpdateModal();

                $("#hf_tableUpdate").val(new Date().toString());
                __doPostBack("hf_tableUpdate", "");
            }
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to create the app.");
        }
    });
}

function CreateTable() {
    var tableName = $.trim($("#tb_tablename").val());
    if (tableName == "") {
        openWSE.AlertWindow("Must provide a table name.");
    }
    else if (columns.length == 0) {
        openWSE.AlertWindow("Must have more than 1 column for this table.");
    }
    else {
        openWSE.LoadingMessage1("Creating Table. Please Wait...");
        $.ajax({
            type: "POST",
            url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/Create",
            data: JSON.stringify({ "columns": columns, "tableName": tableName, "installForUser": $("#MainContent_cb_InstallAfterLoad").is(":checked"), "showSidebar": $("#cb_showSidebar").is(":checked"), "isPrivate": $("#cb_isPrivate").is(":checked") }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            success: function (data) {
                if (!openWSE.ConvertBitToBoolean(data.d)) {
                    $("#successMessage").html("<span style='color: Red;'>An error occured while trying to create your table.</span>");
                    setTimeout(function () {
                        $("#successMessage").html("");
                    }, 3500);
                    openWSE.RemoveUpdateModal();
                }
                else {
                    setTimeout(function () {
                        $("#successMessage").html("<span style='color: Green;'>Your new custom table has been created.</span>");
                        setTimeout(function () {
                            $("#successMessage").html("");
                        }, 3500);

                        $("#tb_tablename").val("");
                        $("#tb_columnName").val("");
                        $("#ddl_datatypes").val("nvarchar");
                        $("#cb_nullable").prop("checked", false);
                        columns = new Array();

                        AddIDRow();
                        for (var i = 0; i < columns.length; i++) {
                            BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
                        }

                        openWSE.RemoveUpdateModal();

                        $("#hf_tableUpdate").val(new Date().toString());
                        __doPostBack("hf_tableUpdate", "");
                    }, 2000);
                }
            },
            error: function (data) {
                $("#successMessage").html("<span style='color: Red;'>An error occured while trying to create your table.</span>");
                setTimeout(function () {
                    $("#successMessage").html("");
                }, 3500);
                openWSE.RemoveUpdateModal();
            }
        });
    }
}