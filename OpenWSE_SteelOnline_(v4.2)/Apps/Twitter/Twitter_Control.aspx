<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Twitter_Control.aspx.cs"
    Inherits="Apps_Twitter_Twitter_Control" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Twitter Control</title>
    <style type="text/css">
        .ajax_twitter
        {
            background: #F9F9F9;
            display: block;
            border-bottom: 1px solid #CCC;
            min-height: 40px;
        }
        
        .ajax__twitter_itemlist
        {
            height: 100%;
            min-height: 100%;
            display: table;
        }
        
        .ajax__twitter_itemlist li
        {
            background-color: transparent !important;
            border-bottom: 1px solid transparent !important;
            clear: none !important;
            margin: 0px !important;
            padding: 15px !important;
            width: 250px;
            min-height: 100px;
            float: left;
        }
        
        .ajax__twitter_itemlist li div a
        {
            color: #0094FF !important;
        }
        
        .ajax__twitter_createat
        {
            color: #555 !important;
        }
        
        .ajax__twitter_footer img
        {
            display: none;
        }
        
        .ajax__twitter_footer
        {
            background: url(../../App_Themes/Standard/App/twitter.png) no-repeat right;
        }
        
        .ajax__twitter_header
        {
            -moz-box-shadow: 0 2px 4px rgba(0, 0, 0, .2);
            -o-box-shadow: 0 2px 4px rgba(0, 0, 0, .2);
            -webkit-box-shadow: 0 2px 4px rgba(0, 0, 0, .2);
            box-shadow: 0 2px 4px rgba(0, 0, 0, .2);
            border-bottom: 1px solid #E5E5E5;
            background: #EFEFEF;
        }
        
        .ajax__twitter_header h3
        {
            font-size: 18px !important;
        }
        
        .ajax__twitter_header h4
        {
            font-size: 12px !important;
        }	

	.ajax__twitter_header img
	{
	    -moz-border-radius: 5px;
	    -webkit-border-radius: 5px;
	    border-radius: 5px;
	}
    </style>
</head>
<body style="background: #F5F5F5 !important">
    <div id="twitter-load" class="main-div-app-bg">
        <form id="formTwitter" runat="server">
        <ajaxToolkit:ToolkitScriptManager runat="Server" ID="ScriptManager1" CombineScripts="false" AsyncPostBackTimeout="360000" />
        <div class="pad-all app-title-bg-color" style="min-height: 40px;">
            <div class="float-left">
                <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
            </div>
            <div>
                <asp:LinkButton ID="btn_refresh" runat="server" CssClass="float-right margin-right RandomActionBtns margin-top-sml pad-all-sml img-refresh"
                    ToolTip="Refresh Feeds" OnClick="btn_refresh_Click" />
                <input type="button" value="Add Feed" class="input-buttons float-right" onclick="AddFeed();" />
                <div class="clear-space-two">
                </div>
                Feeds are automatically updated every minute
            </div>
        </div>
        <div id="TwitterAdd-element" class="Modal-element outside-main-app-div">
            <div class="Modal-overlay" style="background: none!important;">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'TwitterAdd-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="updatepnl_twitteraccounts_add" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_profile" runat="server" DefaultButton="btn_add">
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
                                        <asp:Label ID="Label3" runat="server" Width="125px" Text="Account/Search" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                                        <asp:TextBox ID="tb_twitteraccount" runat="server" CssClass="textEntry margin-right"
                                            Width="355px"></asp:TextBox>
                                        <div class="clear-space">
                                        </div>
                                        <asp:Label ID="lbl_adding" runat="server" Width="125px" Text="Feed Type" CssClass="font-bold font-color-light-black pad-right"></asp:Label>
                                        <asp:DropDownList ID="dd_mode" runat="server" CssClass="margin-right-big">
                                            <asp:ListItem Text="Search" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="Profile" Value="2"></asp:ListItem>
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
                                        <asp:Button ID="btn_add" runat="server" Text="Add Feed" CssClass="input-buttons float-right margin-left-sml RandomActionBtns"
                                            OnClick="btn_add_Click" />
                                        <asp:Button ID="btn_update" runat="server" Text="Update Feed" CssClass="input-buttons float-right margin-left-sml RandomActionBtns"
                                            OnClick="btn_update_Click" Style="display: none;" />
                                        <div class="clear-space">
                                        </div>
                                        <asp:Label ID="lbl_errorTwitter" runat="server" ForeColor="Red"></asp:Label>
                                        <div class="clear-space">
                                        </div>
                                        <asp:HiddenField ID="hf_editID" runat="server" />
                                    </asp:Panel>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="btn_refresh" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <asp:UpdatePanel ID="updatepnl_twitteraccounts" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_updateall" runat="server" ClientIDMode="Static" OnValueChanged="hf_updateall_Changed" />
                <asp:HiddenField ID="hf_delete_TwitterFeed" runat="server" ClientIDMode="Static"
                    OnValueChanged="hf_delete_TwitterFeed_Changed" />
                <asp:HiddenField ID="hf_edit_TwitterFeed" runat="server" ClientIDMode="Static" OnValueChanged="hf_edit_TwitterFeed_Changed" />
                <asp:Panel ID="pnl_twitter_holder" runat="server">
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        </form>
    </div>
</body>
</html>
