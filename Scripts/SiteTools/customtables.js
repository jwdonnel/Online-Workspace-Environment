var columns = new Array();
$(document).ready(function () {
    AddIDRow();
});

$(function () {
    $("#table_columns").sortable({
        cancel: ".entryIDRow, .border-right",
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
                if ((name.toLowerCase() != "entryid") && (name.toLowerCase() != "timestamp")) {
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

$(document.body).on("change", "#cb_addChart", function () {
    if ($(this).prop("checked")) {
        $("#chart_selector").show();
        $("#tr-chart-title").show();
    }
    else {
        $("#chart_selector").hide();
        $("#tr-chart-title").hide();
    }
});

$(document.body).on("change", "#ddl_ChartType", function () {
    $("#img_charttype").attr("src", "../../Standard_Images/ChartTypes/" + $(this).val().replace(/ /g, "").toLowerCase() + ".png");
    $("#lnk_chartTypeSetup").attr("href", "https://google-developers.appspot.com/chart/interactive/docs/gallery/" + $(this).val().replace(/ /g, "").toLowerCase() + "chart");
});

function DeleteTable(id, name) {
    openWSE.ConfirmWindow("Are you sure you want to delete " + name + "? If you select ok you will need to supply the correct password for the user who created the table.",
       function () {
           if (id != "") {
               $("#hf_tableDeleteID").val(id);

               if (UserIsSocialAccount != null && UserIsSocialAccount) {
                   $("#MainContent_btn_passwordConfirm").trigger("click");
               }
               else {
                   openWSE.LoadModalWindow(true, "password-element", "Need Password to Continue");
                   $("#hf_buRestore_type").val("RestoreLast");
                   $("#MainContent_tb_passwordConfirm").focus();
               }
           }
       }, null);
}

function CancelDelete() {
    openWSE.LoadModalWindow(false, "password-element", "");
    $("#hf_tableDeleteID").val("");
    $("#MainContent_tb_passwordConfirm").val("");
}

function PerformDelete(id) {
    openWSE.LoadModalWindow(false, "password-element", "");
    setTimeout(function () {
        openWSE.LoadingMessage1("Deleting Table. Please Wait...");
    }, 100);
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
                openWSE.AlertWindow("An error occured while trying to pull your table.");
                openWSE.RemoveUpdateModal();
                tableEditing = "";
            }
            else {
                var id = data.d[0][0];
                var chartTitle = data.d[0][1];

                $("#tb_chartTitle").val(chartTitle);

                $("#tr-chart-title").hide();
                if ($("#" + id + "-charttype-select").val() != "None") {
                    $("#tr-chart-title").show();
                }

                $("#tb_tablename").val(name);
                $("#tb_columnName").val("");
                $("#ddl_datatypes").val("nvarchar");
                $("#cb_nullable").prop("checked", false);

                for (var i = 1; i < data.d.length; i++) {
                    columns.push(data.d[i]);
                }

                AddIDRow();
                for (var i = 1; i < columns.length; i++) {
                    BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 3);
                }

                $("#createBtns").hide();
                $("#tr-usersallowed").hide();
                $(".table-action-btns").hide();
                $(".table-action-cb").prop('disabled', true);
                $("#Divcb_installHolder").hide();
                $("#editBtns").show();
                tableEditing = table;

                openWSE.RemoveUpdateModal();
            }
        },
        error: function (data) {
            openWSE.AlertWindow("An error occured while trying to pull your table.");
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

    $("#tb_chartTitle").val("");
    $("#tr-chart-title").hide();
    $("#tr-usersallowed").show();
    if ($("#cb_addChart").prop("checked")) {
        $("#tr-chart-title").show();
    }

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
                   data: JSON.stringify({ "columns": columns, "tableName": $("#tb_tablename").val(), "tableID": tableEditing, "chartTitle": $("#tb_chartTitle").val() }),
                   contentType: "application/json; charset=utf-8",
                   dataType: "json",
                   cache: false,
                   success: function (data) {
                       if (!openWSE.ConvertBitToBoolean(data.d)) {
                           openWSE.RemoveUpdateModal();
                           openWSE.AlertWindow("An error occured while trying to update your table.");
                       }
                       else {
                           setTimeout(function () {
                               $("#tb_tablename").val("");
                               $("#tb_columnName").val("");
                               $("#ddl_datatypes").val("nvarchar");
                               $("#cb_nullable").prop("checked", false);

                               $("#tb_chartTitle").val("");
                               $("#tr-chart-title").hide();
                               if ($("#cb_addChart").prop("checked")) {
                                   $("#tr-chart-title").show();
                               }

                               columns = new Array();

                               AddIDRow();
                               for (var i = 0; i < columns.length; i++) {
                                   BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
                               }

                               $("#createBtns").show();
                               $("#tr-usersallowed").show();
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
                       openWSE.RemoveUpdateModal();
                       openWSE.AlertWindow("An error occured while trying to update your table.");
                       tableEditing = "";
                   }
               });
           }
       }, null);
}

function EditUsersAllowed(appId, id, tableName) {
    openWSE.LoadingMessage1("Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/GetEditableUsers",
        data: JSON.stringify({ "appId": appId }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();

            var $modalContent = $("#UsersAllowedEdit-element").find(".ModalPadContent");
            if (data.d != "") {
                var updateBtn = "<input type='button' class='input-buttons' onclick=\"UpdateEditUsersAllowed('" + id + "');\" value='Update' />";
                var cancelBtn = "<input type='button' class='input-buttons no-margin' onclick=\"CancelEditUsersAllowed();\" value='Cancel' />";
                $modalContent.html(data.d + "<div class='clear-space'></div><div align='right' class='pad-bottom'>" + updateBtn + cancelBtn + "</div>");
            }

            openWSE.LoadModalWindow(true, "UsersAllowedEdit-element", tableName + " - Users Allowed To Edit");
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to update your table.");
        }
    });
}
function UpdateEditUsersAllowed(id) {
    var usersAllowed = "";
    $("#UsersAllowedEdit-element").find(".checkbox-usersallowed").each(function () {
        if ($(this).prop("checked")) {
            usersAllowed += $(this).val().toLowerCase() + ";";
        }
    });

    openWSE.LoadingMessage1("Updating Table. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/UpdateEditableUsers",
        data: JSON.stringify({ "id": id, "usersAllowed": usersAllowed }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        complete: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.LoadModalWindow(false, "UsersAllowedEdit-element", "");
            $("#UsersAllowedEdit-element").find(".ModalPadContent").html("");
        }
    });
}
function CancelEditUsersAllowed() {
    openWSE.LoadModalWindow(false, "UsersAllowedEdit-element", "");
    $("#UsersAllowedEdit-element").find(".ModalPadContent").html("");
}

function ImportExcelSpreadSheet() {
    try {
        var data = new FormData();
        var files = $("#excelFileUpload").get(0).files;
        if (files.length > 0) {
            data.append("excelFileUpload", files[0]);
        }
        openWSE.LoadingMessage1("Please Wait...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/GetFileUploadContents",
            type: "POST",
            processData: false,
            contentType: false,
            data: data,
            success: function (response) {
                //code after success
                var innerHtml = '<input type="file" id="excelFileUpload" class="margin-right" /><input type="button" class="input-buttons" onclick="ImportExcelSpreadSheet()" value="Import" />';
                $("#div-excelFileUpload").html(innerHtml);
                openWSE.RemoveUpdateModal();

                if (data.d == "false") {
                    openWSE.AlertWindow("Failed to import file. Please try again.");
                }
            },
            error: function (er) {
                openWSE.RemoveUpdateModal();
                openWSE.AlertWindow(er);
            }

        });
    }
    catch (evt) {
        openWSE.RemoveUpdateModal();
        openWSE.AlertWindow("Failed to import file. Please try again.");
    }
}


/*
 * Table Creator
 */
function AddIDRow() {
    var primaryKey = ""; // "<span class='img-password margin-right float-left' title='Primary Key'></span>";

    var column = "<table cellspacing='0' cellpadding='0' style='width: 625px!important; border-collapse: collapse;'>";
    column += "<tr class='GridNormalRow entryIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
    column += "<td width='45px' align='center' class='GridViewNumRow'>1</td>";
    column += "<td class='border-right'><span class='pad-left'><span class='column-name'>" + primaryKey + "EntryID</span></span></td>";
    column += "<td class='border-right column-type' align='center' width='135px'>uniqueidentifier</td>";
    column += "<td class='border-right column-nullable' align='center' width='75px'><input id='cb_nullable_row' type='checkbox' disabled /></td>";
    column += "<td class='border-right' align='center' width='75px'><div style='padding: 4px;'>N/A</div></td>";
    column += "</tr></table></td></tr></table>";

    column += "<table cellspacing='0' cellpadding='0' style='width: 625px!important; border-collapse: collapse;'>";
    column += "<tr class='GridNormalRow entryIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
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
        type = "<select id='ddl_datatypes_edit'><option value='nvarchar'>nvarchar</option><option value='Date'>Date</option><option value='DateTime'>DateTime</option><option value='Integer'>Integer</option><option value='Decimal'>Decimal</option><option value='Boolean'>Boolean</option></select>";
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
    else if ((newColumn.toLowerCase() == "entryid") || (newColumn.toLowerCase() == "timestamp")) {
        openWSE.AlertWindow("Column already exists! Please use a different name.");
    }
    else {
        var canContinue = true;
        for (var i = 0; i < len; i++) {
            if ((columns[i][0].toLowerCase() == newColumn.toLowerCase()) || (newColumn.toLowerCase() == "entryid") || (newColumn.toLowerCase() == "timestamp")) {
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
        if ((columns[i][0].toLowerCase() == name.toLowerCase()) && (name.toLowerCase() != "entryid") && (name.toLowerCase() != "timestamp")) {
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
            if (((columns[i][0].toLowerCase() == columnName.toLowerCase()) && (index != i)) || (columnName.toLowerCase() == "entryid") || (columnName.toLowerCase() == "timestamp")) {
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

function UpdateSidebarChartType(id) {
    openWSE.LoadingMessage1("Creating Table. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/UpdateSidebarChartType",
        data: "{ 'id': '" + id + "','sidebar': '" + $("#" + id + "-sidebar-cb").prop("checked") + "','notifyUsers': '" + $("#" + id + "-notifi-cb").prop("checked") + "','chartType': '" + $("#" + id + "-charttype-select").val() + "' }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: function (data) {
            openWSE.RemoveUpdateModal();
        },
        error: function (data) {
            openWSE.RemoveUpdateModal();
            openWSE.AlertWindow("An error occured while trying to create the app.");
        }
    });
}

function RecreateApp(appid, tableName, sidebar, chartType) {
    openWSE.LoadingMessage1("Creating Table. Please Wait...");
    $.ajax({
        type: "POST",
        url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/RecreateApp",
        data: "{ 'appId': '" + appid + "','tableName': '" + tableName + "','showSidebar': '" + sidebar + "','chartType': '" + chartType + "' }",
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

        var chartType = "";
        var chartTitle = tableName;
        if ($("#cb_addChart").prop("checked")) {
            chartType = $("#ddl_ChartType").val();
            chartTitle = $("#tb_chartTitle").val();
        }

        var usersAllowed = "";
        $("#UsersAllowed-element").find(".checkbox-usersallowed").each(function () {
            if ($(this).prop("checked")) {
                usersAllowed += $(this).val().toLowerCase() + ";";
            }
        });

        $.ajax({
            type: "POST",
            url: openWSE.siteRoot() + "WebServices/CustomTableCreator.asmx/Create",
            data: JSON.stringify({ "columns": columns, "tableName": tableName, "installForUser": $("#MainContent_cb_InstallAfterLoad").is(":checked"), "showSidebar": $("#cb_showSidebar").is(":checked"), "notifyUsers": $("#cb_allowNotifi").is(":checked"), "isPrivate": $("#cb_isPrivate").is(":checked"), "chartType": chartType, "chartTitle": chartTitle, "usersAllowed": usersAllowed }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            success: function (data) {
                if (!openWSE.ConvertBitToBoolean(data.d)) {
                    openWSE.AlertWindow("An error occured while trying to create your table.");
                    openWSE.RemoveUpdateModal();
                }
                else {
                    setTimeout(function () {
                        $("#tb_tablename").val("");
                        $("#tb_columnName").val("");
                        $("#ddl_datatypes").val("nvarchar");
                        $("#cb_nullable").prop("checked", false);
                        columns = new Array();

                        AddIDRow();
                        for (var i = 0; i < columns.length; i++) {
                            BuildColumn(columns[i][0], columns[i][1], columns[i][2], false, false, i + 2);
                        }

                        $("#hf_tableUpdate").val(new Date().toString());
                        __doPostBack("hf_tableUpdate", "");
                    }, 2000);
                }
            },
            error: function (data) {
                openWSE.AlertWindow("An error occured while trying to create your table.");
                openWSE.RemoveUpdateModal();
            }
        });
    }
}

$(document.body).on("click", ".checkbox-click", function (e) {
    if ($(e.target)[0].localName.toLowerCase() != "input") {
        var $input = $(this).find("input[type='checkbox']");
        if ($input.length > 0) {
            if ($input.prop("checked")) {
                $input.prop("checked", false);
            }
            else {
                $input.prop("checked", true);
            }
        }
    }
});