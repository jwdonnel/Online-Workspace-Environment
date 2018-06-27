var bookmarkApp = function () {
    var loaderHolder = "#bookmarkviewer-load";

    function LoadBookmarks(i) {
        bookmarkApp.BookmarkViewerResizeApp();

        if (i == 1) {
            $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all'>Loading bookmarks...</h3>");
        }

        openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=load&sortby=" + $("#sortby_bookmarkviewer").val(), null, {
            contentType: "text/plain; charset=x-user-defined"
        }, null, null, function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#showbookmarks_bookmarkviewer").html(response);
            }
            else {
                $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all no-bookmarks'>No bookmarks found</h3>");
            }

            GetBookmarkCount();

            setTimeout(function () {
                $("#showbookmarks_bookmarkviewer").find(".bookmark-table-styles").find("img").each(function () {
                    this.onerror = function () {
                        $(this).css("visibility", "hidden");
                    };
                });

                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
        });
    }
    function BookmarkViewerResizeApp() {
        $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#bookmarkviewer_scroller").removeClass("bookmarkviewer-800-maxwidth");
        $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#showbookmarks_bookmarkviewer").removeClass("bookmarkviewer-800-maxwidth");
        if ($(".app-main-holder[data-appid='app-bookmarkviewer']").outerWidth() < 800) {
            $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#bookmarkviewer_scroller").addClass("bookmarkviewer-800-maxwidth");
            $(".app-main-holder[data-appid='app-bookmarkviewer']").find("#showbookmarks_bookmarkviewer").addClass("bookmarkviewer-800-maxwidth");
        }
    }
    function GetBookmarkCount() {
        openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=count&sortby=" + $("#sortby_bookmarkviewer").val(), null, {
            contentType: "text/plain; charset=x-user-defined"
        }, null, null, function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#bookmarkCount").html(response);
            }

            setTimeout(function () {
                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
        });
    }


    function AddBookmark() {
        var name = escape(document.getElementById("tb_addbookmarkname_bookmarkviewer").value);
        var url = escape(document.getElementById("tb_addbookmarkhtml_bookmarkviewer").value);
        if ($.trim(url) != "") {
            openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=add&sortby=" + $("#sortby_bookmarkviewer").val() + "&name=" + name + "&url=" + url, null, {
                contentType: "text/plain; charset=x-user-defined"
            }, null, null, function (data) {
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
                    loadingPopup.RemoveMessage(loaderHolder);
                }, 100);
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
                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
        }
    }
    function OnSortChange_Bookmark() {
        LoadBookmarks(1);
    }
    function edit_click() {
        openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=finishEdit&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + escape($("#editBookmarkid").val()) + "&name=" + escape($("#editBookmarkName").val()) + "&html=" + escape($("#editBookmarkHtml").val()), null, {
            contentType: "text/plain; charset=x-user-defined"
        }, null, null, function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#showbookmarks_bookmarkviewer").html(response);
            }

            setTimeout(function () {
                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
        });

        $('#pnl_share_bookmarkviewer').html('');
        $("#bookmark-share-edit-element").find(".ModalButtonHolder").show();
        openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');
    }
    function remove_in_iFrame(id) {
        openWSE.ConfirmWindow("Are you sure you want to remove this bookmark?",
            function () {
                openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=remove&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id, null, {
                    contentType: "text/plain; charset=x-user-defined"
                }, null, null, function (data) {
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
                        loadingPopup.RemoveMessage(loaderHolder);
                    }, 100);
                });
            }, null);
    }
    function edit_in_iFrame(id) {
        openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=edit&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id, null, {
            contentType: "text/plain; charset=x-user-defined"
        }, null, null, function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#pnl_share_bookmarkviewer").html(response);
                $("#bookmark-share-edit-element").find(".ModalButtonHolder").show();
                openWSE.LoadModalWindow(true, 'bookmark-share-edit-element', 'Edit Bookmark');
                $("#editBookmarkHtml").focus();
            }

            setTimeout(function () {
                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
        });
    }
    function share_in_iFrame(id) {
        openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=share&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id, null, {
            contentType: "text/plain; charset=x-user-defined"
        }, null, null, function (data) {
            var response = data.responseText;
            if (response != "") {
                $("#pnl_share_bookmarkviewer").html(response);
                $("#bookmark-share-edit-element").find(".ModalButtonHolder").hide();
                openWSE.LoadModalWindow(true, 'bookmark-share-edit-element', 'Share Bookmark');
            }

            setTimeout(function () {
                loadingPopup.RemoveMessage(loaderHolder);
            }, 100);
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
            openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=finishShare&sortby=" + $("#sortby_bookmarkviewer").val() + "&id=" + id + "&listusers=" + users, null, {
                contentType: "text/plain; charset=x-user-defined"
            }, null, null, function (data) {
                setTimeout(function () {
                    loadingPopup.RemoveMessage(loaderHolder);
                }, 100);
            });
        }

        $('#pnl_share_bookmarkviewer').html('');
        $("#bookmark-share-edit-element").find(".ModalButtonHolder").hide();
        openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');
    }



    /* Youtube Video */
    function CloseBookmark_YouTube() {
        $(".bookmarkIframe").remove();
        $(".bookmark-table-YoutubeLink").removeClass("youtube-active");
    }
    function embedded_video_bookmark(html, id) {
        CloseBookmark_YouTube();
        var x = "<div id='bookmarkIframe_" + html + "' class='bookmarkIframe'><a href='#' onclick=\"bookmarkApp.CloseBookmark_YouTube();return false;\">Close</a><div class='clear-space-two'></div>";
        x+= "<iframe width='100%' height='400px' src='//www.youtube.com/embed/" + html + "?version=3&autoplay=1' border='0'></iframe></div>";
        $("#" + id).find("td").eq(1).append(x);
        $("#" + id).addClass("youtube-active");
    }


    /* Search Functions */
    function ClearSearchBookmarks() {
        $("#tb_search_bookmarks").val("");
        LoadBookmarks(1);
    }
    function SearchBookmarks() {
        var searchVal = $("#tb_search_bookmarks").val();
        if (searchVal != "") {
            $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all'>Searching...</h3>");
            openWSE.AjaxCall("Apps/Bookmark_Viewer/BookmarkViewer.ashx?action=search&sortby=" + $("#sortby_bookmarkviewer").val() + "&val=" + escape(searchVal), null, {
                contentType: "text/plain; charset=x-user-defined"
            }, null, null, function (data) {
                var response = data.responseText;
                if (response != "") {
                    $("#showbookmarks_bookmarkviewer").html(response);
                }
                else {
                    $("#showbookmarks_bookmarkviewer").html("<h3 class='pad-all no-bookmarks'>No bookmarks found</h3>");
                }

                setTimeout(function () {
                    loadingPopup.RemoveMessage(loaderHolder);
                }, 100);
            });
        }
    }
    function KeyPressSearchBookmarks(event) {
        try {
            if (event.which == 13) {
                SearchBookmarks();
                return false;
            }
        }
        catch (evt) {
            if (event.keyCode == 13) {
                SearchBookmarks();
                return false;
            }
            delete evt;
        }
    }


    $(document.body).on("click", ".bookmarkviewer-update-img", function () {
        loadingPopup.Message("Updating...", loaderHolder);
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

    return {
        LoadBookmarks: LoadBookmarks,
        BookmarkViewerResizeApp: BookmarkViewerResizeApp,
        AddBookmark: AddBookmark,
        OnSortChange_Bookmark: OnSortChange_Bookmark,
        edit_click: edit_click,
        remove_in_iFrame: remove_in_iFrame,
        edit_in_iFrame: edit_in_iFrame,
        share_in_iFrame: share_in_iFrame,
        share_click: share_click,
        CloseBookmark_YouTube: CloseBookmark_YouTube,
        embedded_video_bookmark: embedded_video_bookmark,
        ClearSearchBookmarks: ClearSearchBookmarks,
        SearchBookmarks: SearchBookmarks,
        KeyPressSearchBookmarks: KeyPressSearchBookmarks
    }
}();

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
    try {
        bookmarkApp.BookmarkViewerResizeApp();
        var searchVal = $("#tb_search_bookmarks").val();
        if (searchVal != "") {
            bookmarkApp.SearchBookmarks();
        }
        else {
            bookmarkApp.LoadBookmarks(0);
        }

        $("#tb_search_bookmarks").autocomplete({
            minLength: 1,
            autoFocus: true,
            source: function (request, response) {
                openWSE.AjaxCall("WebServices/AutoComplete.asmx/GetBookmarks", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                    dataFilter: function (data) { return data; }
                }, function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            label: item,
                            value: item
                        };
                    }));
                });
            }
        }).focus(function () {
            $(this).autocomplete("search", "");
        });
    }
    catch (evt) { }
});
$(document).ready(function () {
    bookmarkApp.BookmarkViewerResizeApp();
    bookmarkApp.LoadBookmarks(1);

    $("#tb_search_bookmarks").autocomplete({
        minLength: 1,
        autoFocus: true,
        source: function (request, response) {
            openWSE.AjaxCall("WebServices/AutoComplete.asmx/GetBookmarks", "{ 'prefixText': '" + request.term + "', 'count': '10' }", {
                dataFilter: function (data) { return data; }
            }, function (data) {
                response($.map(data.d, function (item) {
                    return {
                        label: item,
                        value: item
                    };
                }));
            });
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
});
$(window).resize(function () {
    bookmarkApp.BookmarkViewerResizeApp();
});
