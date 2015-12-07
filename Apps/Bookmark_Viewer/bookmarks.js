$(document).ready(function () {
    BookmarkViewerResizeApp();
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
        BookmarkViewerResizeApp();
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
    BookmarkViewerResizeApp();

    if (i == 1) {
        $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all'>Loading bookmarks...</h3>");
    }

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
            else {
                $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all no-bookmarks'>No bookmarks found</h3>");
            }

            GetBookmarkCount();
            
            setTimeout(function () {
                $("#showbookmarks_bookmarkviewer").find(".bookmark-table-styles").find("img").error(function () {
                    $(this).css("visibility", "hidden");
                });

                openWSE.RemoveUpdateModal();
            }, 100);
        }
    });
}

function OnSortChange_Bookmark() {
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

            setTimeout(function () {
                openWSE.RemoveUpdateModal();
            }, 100);
        }
    });
}

function ImportBookmarks() {
    $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all'>Loading bookmarks...</h3>");
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

                setTimeout(function () {
                    openWSE.RemoveUpdateModal();
                }, 100);
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

        setTimeout(function () {
            openWSE.RemoveUpdateModal();
        }, 100);
    }
}

function remove_in_iFrame(id) {
    openWSE.ConfirmWindow("Are you sure you want to remove this bookmark?",
        function () {
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

                    var count = parseInt($("#bookmarkCount").html());
                    count -= 1;
                    if (count == 0) {
                        $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all no-bookmarks'>No bookmarks found</h3>");
                    }
                    $("#bookmarkCount").html(count);

                    setTimeout(function () {
                        openWSE.RemoveUpdateModal();
                    }, 100);
                }
            });
        }, null);
}

function edit_in_iFrame(id) {
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

            setTimeout(function () {
                openWSE.RemoveUpdateModal();
            }, 100);
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
            
            setTimeout(function () {
                openWSE.RemoveUpdateModal();
            }, 100);
        }
    });

    $('#pnl_share_bookmarkviewer').html('');
    openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');
}

function share_in_iFrame(id) {
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

            setTimeout(function () {
                openWSE.RemoveUpdateModal();
            }, 100);
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
        $.ajax({
            url: openWSE.siteRoot() + "Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=finishShare&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id + "&listusers=" + users,
            type: "POST",
            beforeSend: function (xhr) {
                xhr.overrideMimeType("text/plain; charset=x-user-defined");
            },
            complete: function (data) {
                setTimeout(function () {
                    openWSE.RemoveUpdateModal();
                }, 100);
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
    openWSE.LoadingMessage1("Updating...");
});

$(document.body).on("click", ".app-main-holder[data-appid='app-bookmarkviewer'] .exit-button-app, .app-min-bar[data-appid='app-bookmarkviewer'] .exit-button-app-min", function () {
    $("#youtube-player-element").remove();
});

function ClearSearchBookmarks() {
    $("#tb_search_bookmarks").val("Search for bookmark");
    LoadBookmarks(1);
}

function SearchBookmarks() {
    var searchVal = $("#tb_search_bookmarks").val();
    if ((searchVal != "Search for bookmark") && (searchVal != "")) {
        $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all'>Searching...</h3>");
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
                else {
                    $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all no-bookmarks'>No bookmarks found</h3>");
                }

                setTimeout(function () {
                    openWSE.RemoveUpdateModal();
                }, 100);
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
    var x = "<iframe width='587px' height='355px' src='http://www.youtube.com/embed/" + html + "?version=3&autoplay=1' border='0'></iframe>";
    $("#bm-yt-body").html(x);

    var inframeName = $("#" + name).find(".bookmark-html-link").html();
    if (inframeName.length > 40) {
        inframeName = inframeName.substr(0, 40) + "..";
    }

    name = "<span title='" + $("#" + name).find(".bookmark-html-link").html() + "'><img alt='favicon' src='http://www.youtube.com/favicon.ico' class='float-left pad-right' />" + inframeName + "</span>";
    openWSE.LoadModalWindow(true, "youtube-player-element", name);
}

function BookmarkViewerResizeApp() {
    $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#bookmarkviewer_scroller").removeClass("bookmarkviewer-800-maxwidth");
    $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#showbookmarks_bookmarkviewer").removeClass("bookmarkviewer-800-maxwidth");
    if ($(".app-main-holder[data-appid='app-bookmarkviewer']").outerWidth() < 800) {
        $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#bookmarkviewer_scroller").addClass("bookmarkviewer-800-maxwidth");
        $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#showbookmarks_bookmarkviewer").addClass("bookmarkviewer-800-maxwidth");
    }
}

$(window).resize(function () {
    BookmarkViewerResizeApp();
});