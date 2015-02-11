$(document).ready(function () {
    //loading_bookmarkviewer();
    LoadBookmarks(1);

    $("#tb_search_bookmarks").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetBookmarks",
                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        };
                    }));
                }
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    try {
        var searchVal = $("#tb_search_bookmarks").val();
        if ((searchVal.toLowerCase() != "search for bookmark") && (searchVal != "")) {
            SearchBookmarks();
        }
        else {
            LoadBookmarks(0);
        }

        $("#tb_search_bookmarks").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                $.ajax({
                    url: openWSE.siteRoot() + "WebServices/AutoComplete.asmx/GetBookmarks",
                    data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataFilter: function (data) { return data; },
                    success: function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item,
                                value: item
                            };
                        }));
                    }
                });
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
    }
    catch (evt) { }
});

$(document.body).on("keydown", "#tb_search_bookmarks", function (event) {
    try {
        if (event.which == 13) {
            SearchBookmarks();
            return false;
        }
    } catch (evt) {
        if (event.keyCode == 13) {
            SearchBookmarks();
            return false;
        }
        delete evt;
    }
});

$(document.body).on("keydown", "#tb_addbookmarkname_bookmarkviewer, #tb_addbookmarkhtml_bookmarkviewer", function (event) {
    try {
        if (event.which == 13) {
            AddBookmark();
            return false;
        }
    } catch (evt) {
        if (event.keyCode == 13) {
            AddBookmark();
            return false;
        }
        delete evt;
    }
});

$(document.body).on("keydown", "#editBookmarkName, #editBookmarkHtml", function (event) {
    try {
        if (event.which == 13) {
            edit_click();
            return false;
        }
    } catch (evt) {
        if (event.keyCode == 13) {
            edit_click();
            return false;
        }
        delete evt;
    }
});

function LoadBookmarks(i) {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=load&sortby=" + $("#sortby_bookmarkviewer").val(),
        type: "POST",
        beforeSend: function (xhr) {
            xhr.overrideMimeType("text/plain; charset=x-user-defined");
        },
        complete: function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#showbookmarks_bookmarkviewer").html(response);
            }
            if (i == 1) {
                openWSE.RemoveUpdateModal();
            }

            GetBookmarkCount();
        }
    });
}

function OnSortChange_Bookmark() {
    loading_bookmarkviewer();
    LoadBookmarks(1);
}

function GetBookmarkCount() {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=count&sortby=" + $("#sortby_bookmarkviewer").val(),
        type: "POST",
        beforeSend: function (xhr) {
            xhr.overrideMimeType("text/plain; charset=x-user-defined");
        },
        complete: function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#bookmarkCount").html(response);
            }
        }
    });
}

function ImportBookmarks() {
    loading_bookmarkviewer();
    var fd = new FormData;
    fd.append("Filedata", document.getElementById("importBookmarks_upload").files[0]);
    xmlHttpReq = createXMLHttpRequest();
    if (xmlHttpReq != null) {
        xmlHttpReq.addEventListener("load", ImportComplete, false);
        xmlHttpReq.open("POST", openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=import&sortby=" + $("#sortby_bookmarkviewer").val());
        xmlHttpReq.send(fd);
    }
}

function ImportComplete(evt) {
    LoadBookmarks(1);
    openWSE.LoadModalWindow(false, 'bookmark-import-element', '');
}

function AddBookmark() {
    var name = escape(document.getElementById("tb_addbookmarkname_bookmarkviewer").value);
    var url = escape(document.getElementById("tb_addbookmarkhtml_bookmarkviewer").value);
    if ($.trim(url) != "") {
        loading_bookmarkviewer();
        $.ajax({
            url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=add&sortby=" + $("#sortby_bookmarkviewer").val() + "&name=" + name + "&url=" + url,
            type: "POST",
            beforeSend: function (xhr) {
                xhr.overrideMimeType("text/plain; charset=x-user-defined");
            },
            complete: function (data) {
                var response = data.responseText;
                if (response != "") {
                    $("#showbookmarks_bookmarkviewer").find(".no-bookmarks").remove();
                    $("#showbookmarks_bookmarkviewer").prepend(response);
                    $("#tb_addbookmarkname_bookmarkviewer").val("");
                    $("#tb_addbookmarkhtml_bookmarkviewer").val("");
                    $("#lbl_errormessage_bookmarkviewer").css("color", "Green");
                    $("#lbl_errormessage_bookmarkviewer").html("");
                    var count = parseInt($("#bookmarkCount").html());
                    count += 1;
                    $("#bookmarkCount").html(count);

                    openWSE.LoadModalWindow(false, 'bookmark-add-element', '');
                    $('#tb_addbookmarkname_bookmarkviewer').val('');
                    $('#tb_addbookmarkhtml_bookmarkviewer').val('');
                    setTimeout(function () {
                        $("#lbl_errormessage_bookmarkviewer").html("");
                    }, openWSE_Config.animationSpeed);
                }
                else {
                    $("#lbl_errormessage_bookmarkviewer").css("color", "Red");
                    $("#lbl_errormessage_bookmarkviewer").html("An error occurred while processing your request!");
                }

                setTimeout(function () {
                    $("#lbl_errormessage_bookmarkviewer").html("");
                }, 3000);

                openWSE.RemoveUpdateModal();
            }
        });
    }
    else {
        openWSE.LoadModalWindow(false, 'bookmark-add-element', '');
        $('#tb_addbookmarkname_bookmarkviewer').val('');
        $('#tb_addbookmarkhtml_bookmarkviewer').val('');
        setTimeout(function () {
            $("#lbl_errormessage_bookmarkviewer").html("");
        }, openWSE_Config.animationSpeed);
    }
}

function remove_in_iFrame(id) {
    openWSE.ConfirmWindow("Are you sure you want to remove this bookmark?",
        function () {
            loading_bookmarkviewer();

            $.ajax({
                url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=remove&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id,
                type: "POST",
                beforeSend: function (xhr) {
                    xhr.overrideMimeType("text/plain; charset=x-user-defined");
                },
                complete: function (data) {
                    var response = data.responseText;
                    if (response != "") {
                        $("#" + response).remove();
                    }
                    openWSE.RemoveUpdateModal();

                    var count = parseInt($("#bookmarkCount").html());
                    count -= 1;
                    if (count == 0) {
                        $("#showbookmarks_bookmarkviewer").html("<h3 class='no-bookmarks pad-right-big pad-left-big pad-top-big'>No bookmarks available</h3>");
                    }
                    $("#bookmarkCount").html(count)
                }
            });
        }, null);
}

function edit_in_iFrame(id) {
    loading_bookmarkviewer();

    $.ajax({
        url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=edit&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id,
        type: "POST",
        beforeSend: function (xhr) {
            xhr.overrideMimeType("text/plain; charset=x-user-defined");
        },
        complete: function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#pnl_share_bookmarkviewer").html(response);
                openWSE.LoadModalWindow(true, 'bookmark-share-edit-element', 'Edit Bookmark');
                $("#editBookmarkHtml").focus();
            }
            openWSE.RemoveUpdateModal();
        }
    });
}

function edit_click() {
    $.ajax({
        url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=finishEdit&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + escape($("#editBookmarkid").val()) + "&name=" + escape($("#editBookmarkName").val()) + "&html=" + escape($("#editBookmarkHtml").val()),
        type: "POST",
        beforeSend: function (xhr) {
            xhr.overrideMimeType("text/plain; charset=x-user-defined");
        },
        complete: function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#showbookmarks_bookmarkviewer").html(response);
            }
            openWSE.RemoveUpdateModal();
        }
    });

    $('#pnl_share_bookmarkviewer').html('');
    openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');
}

function share_in_iFrame(id) {
    loading_bookmarkviewer();

    $.ajax({
        url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=share&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id,
        type: "POST",
        beforeSend: function (xhr) {
            xhr.overrideMimeType("text/plain; charset=x-user-defined");
        },
        complete: function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#pnl_share_bookmarkviewer").html(response);
                openWSE.LoadModalWindow(true, 'bookmark-share-edit-element', 'Share Bookmark');
            }
            openWSE.RemoveUpdateModal();
        }
    });
}

function share_click(id) {
    var users = "";
    $(".share-tb-list").each(function (index) {
        var $this = $(this);
        if ($this.attr("checked")) {
            users += $this.val() + ";";
        }
    });

    if (users != "") {
        loading_bookmarkviewer();
        $.ajax({
            url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=finishShare&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id + "&listusers=" + users,
            type: "POST",
            beforeSend: function (xhr) {
                xhr.overrideMimeType("text/plain; charset=x-user-defined");
            },
            complete: function (data) {
                openWSE.RemoveUpdateModal();
            }
        });
    }

    $('#pnl_share_bookmarkviewer').html('');
    openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');
}

function ImportBookmark() {
    openWSE.LoadModalWindow(true, 'bookmark-import-element', 'Import Bookmarks');
}

$(document.body).on("change", "#importBookmarks_upload", function () {
    var fu = $("#importBookmarks_upload").val().toLowerCase();
    if (fu.indexOf(".html") != -1) {
        $("#importnBtn_bookmarks").removeAttr("disabled");
        $("#importError").html("");
    }
    else {
        $("#importnBtn_bookmarks").attr("disabled", "disabled");
        $("#importError").html("File type not valid");
    }
});

$(document.body).on("click", ".bookmarkviewer-update-img", function() {
    loading_bookmarkviewer();
});

$(document.body).on("click", "#app-bookmarkviewer .exit-button-app", function () {
    $("#youtube-player-element").remove();
});

function loading_bookmarkviewer() {
    $.LoadingMessage("#bookmarkviewer-load");
}

function ClearSearchBookmarks() {
    $("#tb_search_bookmarks").val("Search for bookmark");
    loading_bookmarkviewer();
    LoadBookmarks(1);
}

function SearchBookmarks() {
    var searchVal = $("#tb_search_bookmarks").val();
    if ((searchVal != "Search for bookmark") && (searchVal != "")) {
        loading_bookmarkviewer();

        $.ajax({
            url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=search&sortby=" + $("#sortby_bookmarkviewer").val() + "&val=" + escape(searchVal),
            type: "POST",
            beforeSend: function (xhr) {
                xhr.overrideMimeType("text/plain; charset=x-user-defined");
            },
            complete: function (data) {
                var response = data.responseText;
                if (response != "") {
                    $("#showbookmarks_bookmarkviewer").html(response);
                }
                openWSE.RemoveUpdateModal();
            }
        });
    }
}

function CloseBookmark_YouTube() {
    openWSE.LoadModalWindow(false, "youtube-player-element", "");
    $("#bm-yt-body").html("");
}

function embedded_video_bookmark(html, name) {
    CloseBookmark_YouTube();
    name = unescape(name);

    if (name.length > 70) {
        name = name.substr(0, 70) + "...";
    }

    var x = "<iframe width='587px' height='355px' src='http://www.youtube.com/embed/" + html + "?version=3&autoplay=1' border='0'></iframe>";
    $("#bm-yt-body").html(x);

    var inframeName = $("#" + name).find(".bookmark-html-link").html();
    if (inframeName.length > 70) {
        inframeName = inframeName.substr(0, 70);
    }

    name = "<img alt='favicon' src='http://www.youtube.com/favicon.ico' class='float-left pad-right' />" + inframeName;
    openWSE.LoadModalWindow(true, "youtube-player-element", name);
}