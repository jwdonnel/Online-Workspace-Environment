<%@ control language="C#" autoeventwireup="true" inherits="Apps_Bookmark_Viewer_BookmarkViewer, App_Web_yzrbs0is" clientidmode="Static" %>
<div id="bookmarkviewer-load" class="main-div-app-bg">
    <div id="bookmarkviewer_scroller" class="pad-all app-title-bg-color"
        style="min-height: 70px">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <div class="pad-top" align="right">
            <div id="searchwrapper" style="width: 400px;">
                <input type="text" id="tb_search_bookmarks" class="searchbox" onfocus="if(this.value=='Search for bookmark')this.value=''"
                    onblur="if(this.value=='')this.value='Search for bookmark'" value="Search for bookmark" />
                <a href="#" onclick="ClearSearchBookmarks(); return false;" title="Clear search"
                    class="searchbox_clear"></a><a href="#" class="searchbox_submit" onclick="SearchBookmarks(); return false;"></a>
            </div>
        </div>
        <div class="clear-space-five">
        </div>
        <div class="float-right pad-left-big">
            <b class="pad-right">Sort By:</b>
            <select id="sortby_bookmarkviewer" onchange="OnSortChange_Bookmark()">
                <option value="1">Date (Descending)</option>
                <option value="2">Date (Ascending)</option>
                <option value="3">Alphabetically (Descending)</option>
                <option value="4">Alphabetically (Ascending)</option>
            </select>
        </div>
        <div class="pad-bottom">
            <b class="pad-right">Bookmarks:</b><span id="bookmarkCount"></span>
        </div>
        <input type="button" value="Import" class="input-buttons" onclick="ImportBookmark();"
            style="width: 65px;" />
        <input type="button" value="Add" class="input-buttons" onclick="openWSE.LoadModalWindow(true, 'bookmark-add-element', 'Add New Bookmark'); $('#tb_addbookmarkname_bookmarkviewer').val(''); $('#tb_addbookmarkhtml_bookmarkviewer').val(''); $('#lbl_errormessage_bookmarkviewer').html(''); $('#tb_addbookmarkhtml_bookmarkviewer').focus();" style="width: 65px;" />
    </div>
    <div id="showbookmarks_bookmarkviewer">
    </div>
    <div id="iframeHolder_bookmarkviewer">
    </div>
</div>
<div id="bookmark-share-edit-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="min-height: 200px; width: 680px;">
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
            </div>
        </div>
    </div>
</div>
<div id="bookmark-import-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="height: 200px; width: 680px;">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'bookmark-import-element', '');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <h4 class="float-left pad-top-sml">
                            <b>Import your bookmarks</b></h4>
                        <div class="clear-space">
                        </div>
                        To upload your bookmarks saved to your browser, export the bookmarks to an .html
                    file.
                    <div class="clear-space">
                    </div>
                        <asp:FileUpload ID="importBookmarks_upload" runat="server" />
                        <div class="clear" style="height: 15px">
                        </div>
                        <div align="right">
                            <input type="button" id="importnBtn_bookmarks" disabled="disabled" value="Import" onclick="ImportBookmarks()" class="input-buttons" />
                        </div>
                        <div class="float-right pad-top pad-right">
                            <span id="importError" style="color: Red"></span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="bookmark-add-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="min-height: 200px; width: 680px;">
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
                            <h4 class="pad-bottom">Type in the URL of the webpage you would like to bookmark. Bookmarking youtube pages
                            will give you a special link to the video itself from within the site.</h4>
                            <div class="clear-space">
                            </div>
                            <table cellspacing="5" cellpadding="5" style="width: 100%;">
                                <tr>
                                    <td><span class="pad-right font-bold">Url</span></td>
                                    <td>
                                        <input type="text" id="tb_addbookmarkhtml_bookmarkviewer" class="textEntry" style="width: 98%;" /></td>
                                </tr>
                                <tr>
                                    <td style="width: 50px;"><span class="pad-right font-bold">Name</span></td>
                                    <td>
                                        <input type="text" id="tb_addbookmarkname_bookmarkviewer" class="textEntry margin-right-sml" style="width: 98%;" /></td>
                                </tr>
                            </table>
                            <div class="clear-space">
                            </div>
                            <div align="right">
                                <input type="button" id="btn_addbookmark_bookmarkviewer" value="Done" class="input-buttons bookmarkviewer-update-img"
                                    onclick="AddBookmark();" style="width: 60px;" />
                                <input type="button" value="Cancel" class="input-buttons" onclick="openWSE.LoadModalWindow(false, 'bookmark-add-element', ''); $('#tb_addbookmarkname_bookmarkviewer').val(''); $('#tb_addbookmarkhtml_bookmarkviewer').val('');"
                                    style="width: 60px;" />
                            </div>
                            <span id="lbl_errormessage_bookmarkviewer" class="float-right" style="color: Red"></span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="youtube-player-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="min-height: 200px; width: 592px;">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="CloseBookmark_YouTube();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent" style="padding: 0!important">
                        <div id="bm-yt-body" style="padding-left: 1px;">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
