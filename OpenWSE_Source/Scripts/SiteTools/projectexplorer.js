var scrollPos = 0;

Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
    var $innerScroll = $("#projectPages");
    if ($innerScroll.length > 0) {
        scrollPos = $innerScroll.scrollTop();
    }
});
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
    var $innerScroll = $("#projectPages");
    if ($innerScroll.length > 0) {
        $innerScroll.scrollTop(scrollPos);
        scrollPos = 0;
    }

    $(window).resize();
    ReloadSelected();
});

var ftpLocation = "";
function PromptFTPCredentials(folder) {
    ftpLocation = folder;

    var title = "Please provide your credentials for " + folder;
    $("#MessageActivationPopup").find(".title").html(title);
    $("#MessageActivationPopup").show();

    $("#tb_ftpUsername").focus();
}
function TryLogin() {
    var ftpUsername = $.trim($("#tb_ftpUsername").val());
    var ftpPassword = $.trim($("#tb_ftpPassword").val());

    if (ftpLocation != "" && ftpUsername != "" && ftpPassword != "") {
        openWSE.LoadingMessage1("Attempting to Connect...");
        $.ajax({
            url: openWSE.siteRoot() + "WebServices/FTPConnect.asmx/TryConnect",
            type: "POST",
            data: '{ "ftpLocation": "' + escape(ftpLocation) + '", "username": "' + escape(ftpUsername) + '", "password": "' + escape(ftpPassword) + '" }',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                openWSE.RemoveUpdateModal();
                if (openWSE.ConvertBitToBoolean(data.d)) {
                    $("#MessageActivationPopup").hide();
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
function CancelLogin() {
    window.top.location.href = window.top.location.href.split('#')[0];
}

$(document).ready(function () {
    $(window).resize();
});
$(window).resize(function () {
    var ht = $("body").outerHeight() - ($("#app_title_bg").outerHeight() + $("#projectInfo").outerHeight() + 20);
    $("#projectPages, #pnl_nosavedproject").css("height", ht + "px");
    $("#editor").css("height", (ht + ($("#projectInfo").outerHeight() - 80)) + "px");
});

$(document.body).on("click", "#lbtn_addNewPage", function () {
    $("#hf_currFolder").val(currFolderSel);
});
$(document.body).on("click", "#lbtn_addNewFolder", function () {
    $("#hf_currFolder").val(currFolderSel);
});

function LoadEditor(text) {
    var path = openWSE.siteRoot() + "Scripts/AceEditor";
    ace.config.set("workerPath", path);
    var editor = ace.edit('editor');
    editor.setTheme('ace/theme/chrome');
    editor.getSession().setMode('ace/mode/html');
    editor.getSession().setUseWrapMode(false);
    editor.getSession().setValue(unescape(text));
    $("#lbtn_savePage").hide();
    $("#lbtn_cancelPage").hide();
    $("#lbtn_previewPage").show();
    editor.getSession().on('change', function (e) {
        $("#lbtn_savePage").show();
        $("#lbtn_cancelPage").show();
        $("#lbtn_previewPage").hide();
    });
}

var currSelected = "";
function LoadSavedPage(_this) {
    openWSE.LoadingMessage1("Loading Page...");
    var id = $(_this).parent().attr("folder");
    currSelected = $(_this).attr("id");
    $("#selectpagehint").hide();
    $("#currFileShowing").html("<b class='pad-right'>Current File Path:</b>" + currFolderSel + "/" + $(_this).text());
    $("#hf_LoadSavedPage").val($(_this).attr("id"));
    __doPostBack("hf_LoadSavedPage", "");
}

function LoadSavedPageFolder(_this) {
    currFolderSel = $(_this).parent().attr("folder");
    previousPageSel = "";
    var splitPage = currFolderSel.split("/");
    for (var i = 0; i < splitPage.length - 1; i++) {
        previousPageSel += splitPage[i];
        if (i < splitPage.length - 2) {
            previousPageSel += "/";
        }
    }
    AddEditControlsToPageList();
}

function Reload_Cancel() {
    openWSE.LoadingMessage1("Cancelling Changes...");
    $("#hf_LoadSavedPage").val(currSelected);
    __doPostBack("hf_LoadSavedPage", "");
}

function ReloadSelected() {
    if ($("#" + currSelected).length > 0) {
        $("#" + currSelected).addClass("selectedPage");
    }
}

function ClearSelected() {
    $("#currFileShowing").html("");
    $("#" + currSelected).removeClass("selectedPage");
    $("#htmlEditor").hide();
    $("#imgEditor").hide();
    $("#selectpagehint").show();
    $("#lbtn_savePage").hide();
    $("#lbtn_cancelPage").hide();
    $("#lbtn_previewPage").show();
    currSelected = "";
}

function SavePage() {
    openWSE.ConfirmWindow("Are you sure you want to save your changes? Doing so will overwrite the current file.",
       function () {
           openWSE.LoadingMessage1("Saving File...");
           $("#lbtn_savePage").hide();
           $("#lbtn_cancelPage").hide();
           $("#lbtn_previewPage").show();
           var editor = ace.edit('editor');
           $("#hf_editorID").val(currSelected);
           $("#hidden_editor").val(escape(editor.getSession().getValue()));
           __doPostBack("hidden_editor", "");
       }, null);
}


var editModeId = "";
var editMode = false;

// File Edit
function EditPageName(id) {
    if (editMode) {
        CancelEditPageName(editModeId);
        CancelEditFolderName(editModeId);
    }

    editModeId = id;
    var $this = $("#" + id).parent();
    $this.find(".td-delete-btn").remove();
    $this.find(".td-edit-btn").remove();

    var cancelBtn = "<a href='#cancel' class='td-cancel-btn float-right margin-right' onclick=\"CancelEditPageName('" + id + "'); return false;\" title='Cancel'></a>";
    var updateBtn = "<a href='#update' class='td-update-btn float-right margin-right' onclick=\"UpdatePageName('" + id + "'); return false;\" title='Update File Name'></a>";
    var tbEdit = "<div class='img-file float-left margin-right img-filefolder-edit'></div><input type='text' class='TextBoxControls tb_filenameEdit' value='" + $this.text() + "' style='width: 175px;' />";

    $this.append(tbEdit + cancelBtn + updateBtn);
    $this.find("#" + id).hide();

    $(".tb_filenameEdit").focus();

    editMode = true;
}
function CancelEditPageName(id) {
    var $this = $("#" + id).parent();
    $(".img-filefolder-edit").remove();
    $this.find(".td-cancel-btn").remove();
    $this.find(".td-update-btn").remove();
    $this.find("input").remove();

    deleteBtn = "<a href='#delete' class='td-delete-btn float-right margin-right' onclick=\"DeletePage('" + id + "'); return false;\" title='Delete File' style='display: none;'></a>";
    editBtn = "<a href='#edit' class='td-edit-btn float-right margin-right' onclick=\"EditPageName('" + id + "'); return false;\" title='Edit File Name' style='display: none;'></a>";

    $this.append(deleteBtn + editBtn);
    $this.find("#" + id).show();
    editMode = false;
    editModeId = "";
}
function UpdatePageName(id) {
    var r = true;
    if (($("#lbtn_savePage").css("display") == "block") && ($("#htmlEditor").css("display") == "block")) {
        openWSE.ConfirmWindow("Are you sure you want to edit this file's name? If you choose yes, any unsaved data will be lost.",
           function () {
               editMode = false;
               editModeId = "";

               var $this = $("#" + id).parent();
               var newName = $this.find("input").val();

               ClearSelected();

               openWSE.LoadingMessage1("Updating...");

               $("#hf_editorID").val(id);
               $("#hf_updatePageName").val(newName);
               __doPostBack("hf_updatePageName", "");
           }, null);
    }
    else {
        editMode = false;
        editModeId = "";

        var $this = $("#" + id).parent();
        var newName = $this.find("input").val();

        ClearSelected();

        openWSE.LoadingMessage1("Updating...");

        $("#hf_editorID").val(id);
        $("#hf_updatePageName").val(newName);
        __doPostBack("hf_updatePageName", "");
    }
}
function DeletePage(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this file?",
       function () {
           if (id != "") {
               openWSE.LoadingMessage1("Deleting File...");

               if (id == currSelected) {
                   ClearSelected();
               }

               $("#hf_deleteFile").val(id);
               __doPostBack("hf_deleteFile", "");
           }
       }, null);
}

// Folder Edit
function EditFolderName(id) {
    if (editMode) {
        CancelEditPageName(editModeId);
        CancelEditFolderName(editModeId);
    }

    editModeId = id;
    var $this = $("#" + id).parent();
    $this.find(".td-delete-btn").remove();
    $this.find(".td-edit-btn").remove();

    var cancelBtn = "<a href='#cancel' class='td-cancel-btn float-right margin-right' onclick=\"CancelEditFolderName('" + id + "'); return false;\" title='Cancel'></a>";
    var updateBtn = "<a href='#update' class='td-update-btn float-right margin-right' onclick=\"UpdateFolderName('" + id + "'); return false;\" title='Update Folder Name'></a>";
    var tbEdit = "<div class='img-folder float-left margin-right img-filefolder-edit'></div><input type='text' class='TextBoxControls tb_foldernameEdit' value='" + $this.find("a").text() + "' style='width: 175px;' />";

    $this.append(tbEdit + cancelBtn + updateBtn);
    $this.find("#" + id).hide();

    $(".tb_foldernameEdit").focus();

    editMode = true;
}
function CancelEditFolderName(id) {
    var $this = $("#" + id).parent();
    $(".img-filefolder-edit").remove();
    $this.find(".td-cancel-btn").remove();
    $this.find(".td-update-btn").remove();
    $this.find("input").remove();

    deleteBtn = "<a href='#delete' class='td-delete-btn float-right margin-right' onclick=\"DeleteFolder('" + id + "'); return false;\" title='Delete File' style='display: none;'></a>";
    editBtn = "<a href='#edit' class='td-edit-btn float-right margin-right' onclick=\"EditFolderName('" + id + "'); return false;\" title='Edit File Name' style='display: none;'></a>";

    $this.append(deleteBtn + editBtn);
    $this.find("#" + id).show();
    editMode = false;
    editModeId = "";
}
function UpdateFolderName(id) {
    var r = true;
    if (($("#lbtn_savePage").css("display") == "block") && ($("#htmlEditor").css("display") == "block")) {
        openWSE.ConfirmWindow("Are you sure you want to edit this folder's name? If you choose yes, any unsaved data will be lost.",
           function () {
               editMode = false;
               editModeId = "";

               var newName = $("#" + id).parent().find("input").val();

               ClearSelected();

               openWSE.LoadingMessage1("Updating...");

               $("#hf_editorID").val(id);
               $("#hf_updateFolderName").val(newName);
               __doPostBack("hf_updateFolderName", "");
           }, null);
    }
    else {
        editMode = false;
        editModeId = "";

        var newName = $("#" + id).parent().find("input").val();

        ClearSelected();

        openWSE.LoadingMessage1("Updating...");

        $("#hf_editorID").val(id);
        $("#hf_updateFolderName").val(newName);
        __doPostBack("hf_updateFolderName", "");
    }
}
function DeleteFolder(id) {
    openWSE.ConfirmWindow("Are you sure you want to delete this folder and all its contents?",
        function () {
            openWSE.LoadingMessage1("Deleting Folder...");
            $("#hf_deleteFolder").val(id);
            __doPostBack("hf_deleteFolder", "");
        }, null);
}


$(document.body).on("keypress", ".tb_filenameEdit", function () {
    if (event.which == 13) {
        event.preventDefault();
        UpdatePageName(editModeId);
    }
});
$(document.body).on("keypress", ".tb_foldernameEdit", function () {
    if (event.which == 13) {
        event.preventDefault();
        var fullPath = $("#" + editModeId).attr("folder");
        if (currFolderSel != "") {
            fullPath = currFolderSel + "/" + fullPath;
        }

        UpdateFolderName(editModeId, fullPath);
    }
});

function AddEditControlsToPageList() {
    var deleteBtn;
    var editBtn;

    SetFolders();

    // Remove buttons if there
    $(".td-edit-btn").remove();
    $(".td-delete-btn").remove();

    $(".page-list li").each(function () {
        if (!$(this).hasClass("type-folder-back")) {
            var id = $(this).find("a").attr("id");
            if ($(this).attr("type") == "folder") {
                deleteBtn = "<a href='#delete' class='td-delete-btn float-right margin-right' onclick=\"DeleteFolder('" + id + "'); return false;\" title='Delete Folder' style='display: none;'></a>";
                editBtn = "<a href='#edit' class='td-edit-btn float-right margin-right' onclick=\"EditFolderName('" + id + "'); return false;\" title='Edit Folder Name' style='display: none;'></a>";
                $(this).append(deleteBtn + editBtn);
            }
            else if ($(this).find(".td-update-btn").length == 0) {
                deleteBtn = "<a href='#delete' class='td-delete-btn float-right margin-right' onclick=\"DeletePage('" + id + "'); return false;\" title='Delete File' style='display: none;'></a>";
                editBtn = "<a href='#edit' class='td-edit-btn float-right margin-right' onclick=\"EditPageName('" + id + "'); return false;\" title='Edit File Name' style='display: none;'></a>";
                $(this).append(deleteBtn + editBtn);
            }
        }
    });

    $(".page-list li").hover(
    function () {
        if (!editMode && !dragDropEnabled) {
            $(this).find(".td-edit-btn").show();
            $(this).find(".td-delete-btn").show();
        }
    },
    function () {
        $(this).find(".td-edit-btn").hide();
        $(this).find(".td-delete-btn").hide();
    });
}

var currFolderSel = "";
var previousPageSel = "";
var currDragDrop = "";
var $currDragDropId;
var currDragType = "";
var dragDropEnabled = false;
var confirmMessage = "Are you sure you want to move this file? If you choose yes, any unsaved data will be lost.";
function SetFolders() {
    $(".type-folder-back").remove();
    $(".page-list li").each(function () {
        var folder = $(this).attr("folder");

        if (currFolderSel == "" && folder.indexOf('/') == -1 && $(this).attr("type") == "folder") {
            $(this).show();
        }
        else if (currFolderSel == folder && $(this).attr("type") == "file") {
            $(this).show();
        }
        else if (currFolderSel != "") {
            var splitFolder = folder.split(currFolderSel + '/');
            if (splitFolder.length == 2 && splitFolder[1].indexOf('/') == -1 && $(this).attr("type") == "folder") {
                $(this).show();
            }
            else {
                $(this).hide();
            }
        }
        else {
            $(this).hide();
        }
    });

    if (currFolderSel != "") {
        var folderLi = "<li folder='" + previousPageSel + "' class='type-folder-back'><a href='#' onclick='LoadSavedPageFolder(this);return false;'><div class='img-folder float-left margin-right'></div>../</a></li>";
        $(".page-list").prepend(folderLi);
    }

    var count = 0;
    var tempCFS = "";
    var splitCFS = currFolderSel.split('/');
    for (var i = 0; i < splitCFS.length; i++) {
        if (count >= 4) {
            tempCFS += "<br />";
            count = 0;
        }

        if (splitCFS[i] != "") {
            tempCFS += "/" + splitCFS[i];
            count++;
        }
    }

    $("#current-path-selected").html("<b class='margin-right-sml'>Current Path:</b>" + tempCFS + "/");

    $(".page-list li").draggable({
        axis: "y",
        containment: "#pnl_pages",
        start: function (event, ui) {
            if ($(this).hasClass("type-folder-back")) {
                return false;
            }

            $(this).addClass("bg-drag-select");
            
            $currDragDropId = $(this);
            currDragDrop = $(this).find("a").eq(0).attr("id");

            if ($(this).attr("type") == "folder") {
                currDragType = "folder";
            }
            else {

                currDragType = "file";
            }

            dragDropEnabled = true;
        },
        stop: function (event, ui) {
            $(this).removeClass("bg-drag-select");
            dragDropEnabled = false;
        },
        revert : function(event, ui) {
            // on older version of jQuery use "draggable"
            // $(this).data("draggable")
            // on 2.x versions of jQuery use "ui-draggable"
            // $(this).data("ui-draggable")
            $(this).data("uiDraggable").originalPosition = {
                top : 0,
                left : 0
            };
            // return boolean
            return !event;
            // that evaluate like this:
            // return event !== false ? false : true;
        }
    });
    $(".page-list li").droppable({
        drop: function (event, ui) {
            if ($(this).attr("type") == "folder" || $(this).hasClass("type-folder-back")) {
                var selector = "hf_moveFile";
                if (currDragType == "folder") {
                    selector = "hf_moveFolder";
                }

                if (($("#lbtn_savePage").css("display") == "block") && ($("#htmlEditor").css("display") == "block")) {
                    openWSE.ConfirmWindow(confirmMessage,
                        function () {
                            $("#hf_editorID").val(currDragDrop);
                            if ($.trim($(this).attr("folder")) == "" || $(this).text() == "../") {
                                var splitFolderName = currFolderSel.split('/');
                                var UpDir = "";
                                for (var i = 0; i < splitFolderName.length - 1; i++) {
                                    UpDir += splitFolderName[i] + "/";
                                }

                                if (UpDir == "") {
                                    UpDir = "root";
                                }

                                $("#" + selector).val(UpDir);
                            }
                            else {
                                $("#" + selector).val($(this).attr("folder"));
                            }
                            $currDragDropId = null;
                            openWSE.LoadingMessage1("Updating...");
                            ClearSelected();
                            __doPostBack(selector, "");
                        }, null);
                }
                else {
                    $("#hf_editorID").val(currDragDrop);
                    if ($.trim($(this).attr("folder")) == "" || $(this).text() == "../") {
                        var splitFolderName = currFolderSel.split('/');
                        var UpDir = "";
                        for (var i = 0; i < splitFolderName.length - 1; i++) {
                            UpDir += splitFolderName[i] + "/";
                        }

                        if (UpDir == "") {
                            UpDir = "root";
                        }

                        $("#" + selector).val(UpDir);
                    }
                    else {
                        $("#" + selector).val($(this).attr("folder"));
                    }
                    $currDragDropId = null;
                    openWSE.LoadingMessage1("Updating...");
                    ClearSelected();
                    __doPostBack(selector, "");
                }

            }
            if ($currDragDropId != null) {
                openWSE.RemoveUpdateModal();
                $currDragDropId.animate({
                    top: 0,
                    left: 0
                });
            }
        },
        over: function (event, ui) {
            if ($(this).attr("type") == "folder" || $(this).hasClass("type-folder-back")) {
                $(this).addClass("over-draggable");
            }
        },
        out: function (event, ui) {
            if ($(this).attr("type") == "folder" || $(this).hasClass("type-folder-back")) {
                $(this).removeClass("over-draggable");
            }
        }
    });
}