<%@ Page Title="IP Manager" Async="true" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="IPManager.aspx.cs" Inherits="SiteTools_IPManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_lockedipwatchlist" runat="server"></asp:Literal>
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            IP Manager
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <div id="ipwatchlist" class="pnl-section">
        <asp:UpdatePanel ID="UpdatePanel6" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_DeleteWatchlistIP" runat="server" OnValueChanged="hf_DeleteWatchlistIP_ValueChanged" ClientIDMode="Static" />
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <div class="float-left pad-right">
                            <span class="td-allow-btn float-left"></span><b style="line-height: 22px;">Allowed</b>
                        </div>
                        <div class="float-left pad-left">
                            <span class="td-ignore-btn float-left"></span><b style="line-height: 22px;">Blocked</b>
                        </div>
                        <div class="clear-space"></div>
                        <asp:HiddenField ID="hf_BlockAllowIPAddress_Watchlist" runat="server" ClientIDMode="Static" OnValueChanged="hf_BlockAllowIPAddress_Watchlist_ValueChanged" />
                        <asp:HiddenField ID="hf_AddIPAddress_Watchlist" runat="server" ClientIDMode="Static" OnValueChanged="hf_AddIPAddress_Watchlist_ValueChanged" />
                        <asp:Panel ID="pnl_watchlist" runat="server"></asp:Panel>
                        <div class="clear"></div>
                    </div>
                    <div class="td-settings-desc">
                        Blocking an IP Address will redirect every request to a default blocked page for that IP. That (blocked) IP will not be able to access the site until the IP is released from the watch list.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="iplistener" class="pnl-section" style="display: none;">
        <asp:UpdatePanel ID="UpdatePanel7" runat="server">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <asp:Panel ID="pnl_iplistener_holder" runat="server">
                        </asp:Panel>
                        <asp:HiddenField ID="hf_UpdateIPListener" ClientIDMode="Static" runat="server" OnValueChanged="hf_UpdateIPListener_ValueChanged" />
                        <div class="clear-space">
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        The IP Listener can be used to set up an <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://en.wikipedia.org/wiki/Intranet');return false;">internal</a> only version of the site and/or only allowing certain ip address to
                            access the site. (This works best for companies with a static IP Address)<br />
                        If any IP address is active to listen for, all other IP addresses will be blocked (Only the active ip address(s) will be available to access). You may have multiple IP addresses active. Turn off or delete all IP address to allow for everyone to access the site.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="networkSettings" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
        <asp:UpdatePanel ID="UpdatePanel8" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnl_ipAutoBlock" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Auto Block IP
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_AutoIPBlock_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_AutoIPBlock_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_AutoIPBlock_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_AutoIPBlock_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Set to True if you want the site to automatically block any IP after a certain amount of attempts.
                        </div>
                    </div>
                    <asp:Panel ID="pnl_attemptsBeforeBlock" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Attempts Before Blocked
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <asp:Panel ID="pnl_autoblock_count" runat="server" DefaultButton="btn_autoblock_count">
                                    <asp:TextBox ID="tb_autoblock_count" runat="server" CssClass="textEntry margin-right-big"
                                        Width="55px" TextMode="Number"></asp:TextBox><asp:Button ID="btn_autoblock_count" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Update" OnClick="btn_autoblock_count_Clicked" />
                                </asp:Panel>
                            </div>
                            <div class="td-settings-desc">
                                Set the number of attempts before IP is blocked. (Must be greater than 0)
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
