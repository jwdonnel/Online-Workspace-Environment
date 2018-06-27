<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MessageBoard.ascx.cs"
    Inherits="Apps_MessageBoard_MessageBoard" ClientIDMode="Static" %>
<div class="message-board-toolbar">
    <a id="message-board-backbtn" onclick="messageBoardApp.CloseDiscussion(); return false;" title="Close discussion" style="display: none;"><span class="discussion-back-button"></span>Back</a>
    <a id="message-board-createbtn" onclick="messageBoardApp.AddNewMessageDialog(); return false;" title="Create message"><span class="discussion-add-button"></span>Create Discussion</a>
    <a onclick="messageBoardApp.LoadMBFeeds(); return false;" title="View the available RSS feeds"><span class="discussion-rss-button"></span>RSS Feeds</a>
    <div class="message-board-groupselector">
        <span class="pad-right">Group</span>
        <select id="dd_currentGroup_messageboard" runat="server" onchange="messageBoardApp.GroupChange();">
        </select>
    </div>
    <div class="clear"></div>
</div>
<div id="message-board-currentdiscussion"></div>
<div id="MessageList_messageboard">
    <h3 class="pad-all">Loading...</h3>
</div>
<div id="NewMessageBoard-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="750">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="messageBoardApp.CloseNewMessageBoard(); return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="Messageboard_Holder">
                            <span class="pad-right">Post to Group</span>
                            <select id="dd_groups_messageboard" runat="server">
                            </select>
                            <div class="clear-space"></div>
                            <span class="modal-descDivs-item">Title</span>
                            <input type="text" id="messageBoard_newTitle" class="textEntry" maxlength="500" style="width: 100%;" />
                            <div class="clear-space-five"></div>
                            <span class="modal-descDivs-item">Message</span>
                            <textarea id="Editor_messageboard" style="min-height: 100px; width: 100%"></textarea>
                        </div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input id="imgbtn_update" type="button" class="input-buttons modal-ok-btn" value="Post" onclick="messageBoardApp.PostMessage()" />
                    <input type="button" class="input-buttons modal-ok-btn" value="Clear" onclick="messageBoardApp.ClearNewMessage(); return false;" />
                    <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="messageBoardApp.CloseNewMessageBoard(); return false;" />
                </div>
            </div>
        </div>
    </div>
</div>
<div id="MBRSS-Feed-Selector-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="550">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'MBRSS-Feed-Selector-element', '');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="AddMBRSSFeedHolder">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/MessageBoard/messageboard.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="//cdn.tinymce.com/4/tinymce.min.js" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/MessageBoard/messageboard.js" />
