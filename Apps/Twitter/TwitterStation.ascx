<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TwitterStation.ascx.cs" Inherits="Apps_Twitter_TwitterStation" ClientIDMode="Static" %>
<div class="pad-all app-title-bg-color">
    <div class="float-left">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" style="margin-top: 4px;" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
        <div class="clear"></div>
    </div>
    <div class="float-right margin-top">
        <a href="#update" title="Refresh" class="img-refresh margin-left-big" onclick="twitterStation.GetFeeds(true);return false;"></a>
        <a href="#edit" id="btn_editTwitterFeeds" runat="server" title="Edit Feeds" class="td-edit-btn margin-left-big" style="padding:0!important;" onclick="openWSE.LoadModalWindow(true, 'TwitterEditFeeds_element', 'Edit Feeds');return false;"></a>
        <a href="#add" id="btn_addNewTwitterFeed" runat="server" title="Add New Feed" class="td-add-btn margin-left-big" style="padding:0!important;" onclick="twitterStation.AddFeed();return false;"></a>
    </div>
    <div class="clear"></div>
</div>
<div id="twitterstation-posts">
</div>
<div id="TwitterAdd_element" runat="server" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="565">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="twitterStation.CloseModal();return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <small><b class="pad-right-sml">Note:</b>Leaving the Title and Caption empty for Profile Feed Type will use the default Title and Caption for that user.</small>
                        <div class="clear-space"></div>
                        <asp:Label ID="Label3" runat="server" Width="125px" Text="Account/Search" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                        <asp:TextBox ID="tb_twitteraccount" runat="server" CssClass="textEntry margin-right"
                            Width="355px"></asp:TextBox><span id="must-have-twitter-search" style="display: none; color: red;">*</span>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="Label1" runat="server" Width="125px" Text="Title" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                        <asp:TextBox ID="tb_title" runat="server" CssClass="textEntry margin-right" Width="355px"
                            MaxLength="150"></asp:TextBox>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="Label2" runat="server" Width="125px" Text="Caption" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                        <asp:TextBox ID="tb_caption" runat="server" CssClass="textEntry margin-right-sml"
                            Width="355px" MaxLength="150"></asp:TextBox>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_adding" runat="server" Width="125px" Text="Feed Type" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                        <asp:DropDownList ID="dd_mode" runat="server" CssClass="margin-right-big">
                            <asp:ListItem Text="Profile" Value="Profile"></asp:ListItem>
                            <asp:ListItem Text="Search" Value="Search"></asp:ListItem>
                        </asp:DropDownList>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_amountdisplay" runat="server" Width="125px" Text="Tweets to Display"
                            CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                        <asp:DropDownList ID="dd_display_amount" runat="server" CssClass="margin-right-big">
                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                            <asp:ListItem Text="4" Value="4" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                            <asp:ListItem Text="15" Value="15"></asp:ListItem>
                            <asp:ListItem Text="20" Value="20"></asp:ListItem>
                        </asp:DropDownList>
                        <input id="btn_add" type="button" class="input-buttons float-right margin-left-sml" value="Add Feed" onclick="twitterStation.FinishAdd();" style="display: none;" />
                        <input id="btn_update" type="button" class="input-buttons float-right margin-left-sml" value="Update Feed" onclick="twitterStation.UpdateFeed();" style="display: none;" />
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_errorTwitter" runat="server" ForeColor="Red"></asp:Label>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="TwitterEditFeeds_element" runat="server" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="700">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'TwitterEditFeeds_element', '');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="twitter_edit_feeds_holder">
                        </div>
                        <div class="clear">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/Twitter/twitter.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/Twitter/twitter.js" />
