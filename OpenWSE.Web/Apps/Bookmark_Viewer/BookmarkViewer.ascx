<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookmarkViewer.ascx.cs"
    Inherits="Apps_Bookmark_Viewer_BookmarkViewer" ClientIDMode="Static" %>
<div id="bookmarkviewer_scroller" class="pad-all app-title-bg-color">
    <div class="float-left">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
    </div>
    <div class="pad-top" align="right">
        <div class="searchwrapper">
            <div class="searchboxholder">
                <input id="tb_search_bookmarks" type="text" class="searchbox" placeholder="Search bookmarks..." onkeypress="bookmarkApp.KeyPressSearchBookmarks(event)" />
            </div>
            <a href="#" class="searchbox_submit" onclick="bookmarkApp.SearchBookmarks(); return false;"></a>
            <a href="#" onclick="bookmarkApp.ClearSearchBookmarks(); return false;" class="searchbox_clear"></a>
        </div>
    </div>
    <div class="clear-space-five">
    </div>
    <div class="float-right pad-left-big">
        <b class="pad-right">Sort By:</b>
        <select id="sortby_bookmarkviewer" onchange="bookmarkApp.OnSortChange_Bookmark()">
            <option value="1">Date (Descending)</option>
            <option value="2">Date (Ascending)</option>
            <option value="3">Alphabetically (Descending)</option>
            <option value="4">Alphabetically (Ascending)</option>
        </select>
    </div>
    <div class="pad-bottom">
        <b class="pad-right">Bookmarks:</b><span id="bookmarkCount"></span>
    </div>
    <asp:Panel ID="pnl_BookmarkPnlBtns" runat="server">
        <input type="button" value="Add" class="input-buttons no-margin" onclick="openWSE.LoadModalWindow(true, 'bookmark-add-element', 'Add New Bookmark'); $('#tb_addbookmarkname_bookmarkviewer').val(''); $('#tb_addbookmarkhtml_bookmarkviewer').val(''); $('#lbl_errormessage_bookmarkviewer').html(''); $('#tb_addbookmarkhtml_bookmarkviewer').focus();" style="width: 65px;" />
    </asp:Panel>
    <div class="clear"></div>
</div>
<div id="showbookmarks_bookmarkviewer">
</div>
<div id="iframeHolder_bookmarkviewer">
</div>
<div id="bookmark-share-edit-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="700">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', '');$('#pnl_share_bookmarkviewer').html('');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="pnl_share_bookmarkviewer">
                        </div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input type="button" class="input-buttons modal-ok-btn" value="Save" onclick="bookmarkApp.edit_click()" />
                    <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'bookmark-share-edit-element', ''); $('#pnl_share_bookmarkviewer').html('');" />
                </div>
            </div>
        </div>
    </div>
</div>
<div id="bookmark-add-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="700">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'bookmark-add-element', '');$('#tb_addbookmarkname_bookmarkviewer').val('');$('#tb_addbookmarkhtml_bookmarkviewer').val('');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="pnl_add_bookmarkviewer">
                            <h4>Type in the URL of the webpage you would like to bookmark. Bookmarking youtube pages
                            will give you a special link to the video itself from within the site.</h4>
                            <div class="clear-space">
                            </div>
                            <div class="modal-descDivs-item">Url</div>
                            <input type="text" id="tb_addbookmarkhtml_bookmarkviewer" class="textEntry" style="width: 100%;" />
                            <div class="clear-space-five"></div>
                            <div class="modal-descDivs-item">Name</div>
                            <input type="text" id="tb_addbookmarkname_bookmarkviewer" class="textEntry margin-right-sml" style="width: 100%;" />
                            <div class="clear-space">
                            </div>
                            <span id="lbl_errormessage_bookmarkviewer" class="float-right" style="color: Red"></span>
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input type="button" id="btn_addbookmark_bookmarkviewer" value="Done" class="input-buttons bookmarkviewer-update-img modal-ok-btn" onclick="bookmarkApp.AddBookmark();" />
                    <input type="button" value="Cancel" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'bookmark-add-element', ''); $('#tb_addbookmarkname_bookmarkviewer').val(''); $('#tb_addbookmarkhtml_bookmarkviewer').val('');" />
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/Bookmark_Viewer/bookmarks.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/Bookmark_Viewer/bookmarks.js" />
