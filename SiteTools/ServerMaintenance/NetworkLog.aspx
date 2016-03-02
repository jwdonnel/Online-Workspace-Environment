<%@ Page Title="Network Log" Async="true" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="NetworkLog.aspx.cs" Inherits="SiteTools_NetworkLog" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Panel ID="pnlLinkBtns" runat="server">
        </asp:Panel>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_updateIgnore" runat="server" OnValueChanged="hf_updateIgnore_Changed" />
                <asp:HiddenField ID="hf_updateRefreshOnError" runat="server" OnValueChanged="hf_updateRefreshOnError_Changed" />
                <asp:HiddenField ID="hf_updateAllow" runat="server" OnValueChanged="hf_updateAllow_Changed" />
                <asp:HiddenField ID="hf_deleteError" runat="server" OnValueChanged="hf_deleteError_Changed" />
                <asp:HiddenField ID="hf_resetHitCount" runat="server" OnValueChanged="hf_resetHitCount_Changed" />
                <asp:HiddenField ID="hf_DeleteLoginEvent" runat="server" OnValueChanged="hf_DeleteLoginEvent_Changed" />
                <asp:HiddenField ID="hf_AllowBlockLoginEvent" runat="server" OnValueChanged="hf_AllowBlockLoginEvent_Changed" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="log" class="pnl-section" style="display: none;">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="searchwrapper" style="min-width: 385px; width: 50%">
                        <asp:Panel ID="Panel1_applog" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Events')this.value=''" onblur="if(this.value=='')this.value='Search Events'"
                                Text="Search Events"></asp:TextBox>
                            <a href="#" onclick="$('#MainContent_tb_search').val('Search Events');return false;"
                                class="searchbox_clear"></a>
                            <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                OnClick="imgbtn_search_Click" />
                        </asp:Panel>
                    </div>
                    <span class="float-right margin-left-big">Leave search textbox blank to search
                    all events</span>
                    <asp:HiddenField ID="hf_searchreset" runat="server" ClientIDMode="Static" OnValueChanged="hf_searchreset_Changed" />
                    <div class="clear-space">
                    </div>
                    <div class="float-left">
                        <div class="float-left pad-top">
                            <div class="float-left pad-right">
                                <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                            </div>
                            <div class="float-left pad-left">
                                <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Ignored</b>
                            </div>
                        </div>
                        <div class="float-left pad-left-big margin-left-big pad-top">
                            <asp:CheckBox ID="cb_ViewErrorsOnly" ClientIDMode="Static" runat="server" Text="&nbsp;View Errors Only" AutoPostBack="true" OnCheckedChanged="cb_ViewErrorsOnly_CheckedChanged" />
                        </div>
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="lbtn_refresherrors" runat="server" OnClick="lbtn_refresherrors_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                        <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('SiteTools/iframes/LogFolder.aspx', this);return false;" class="margin-right-big">View Logging Folder</a>
                        <asp:LinkButton ID="LinkButton2" runat="server" CssClass="margin-right-big" OnClientClick="return ConfirmClearLogFolder(this);">Delete All Log Files</asp:LinkButton>
                        <asp:LinkButton ID="LinkButton1" runat="server" OnClientClick="return ConfirmClearLogAll(this);">Clear Events</asp:LinkButton>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:GridView ID="GV_Requests" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                        Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                        <EmptyDataRowStyle ForeColor="Black" />
                        <RowStyle CssClass="GridNormalRow" />
                        <EmptyDataTemplate>
                            <div class="emptyGridView">
                                No Events Available
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                        <tr>
                                            <td style="width: 40px;"></td>
                                            <td>Event Information
                                            </td>
                                            <td style="width: 75px; text-align: center;">Actions
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                        <tbody>
                                            <td class="GridViewNumRow" valign="middle" style="width: 40px; text-align: center;">
                                                <%#Container.DataItemIndex + 1 %>
                                            </td>
                                            <td class="border-right" valign="top">
                                                <table>
                                                    <tr>
                                                        <td class="eventlog-datecol">Date</td>
                                                        <td><%#Eval("date") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Event Name</td>
                                                        <td><%#Eval("name") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Event Message</td>
                                                        <td><%#Eval("comment") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Stack Trace</td>
                                                        <td><%#Eval("stacktrace") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Exception Type</td>
                                                        <td><%#Eval("exceptiontype") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Machine Name</td>
                                                        <td><%#Eval("machinename") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Request Url</td>
                                                        <td><%#Eval("requesturl") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Username</td>
                                                        <td><%#Eval("username") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">IP Address</td>
                                                        <td><%#Eval("ipAddress") %></td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td class='border-right' style="width: 75px; text-align: center;">
                                                <%#Eval("ignore")%>
                                                <%#Eval("refresh")%>
                                                <%#Eval("delete")%>
                                            </td>
                                        </tbody>
                                    </table>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="ignore" class="pnl-section" style="display: none">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="searchwrapper" style="min-width: 385px; width: 50%">
                        <asp:Panel ID="Panel1" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_SearchIgnore" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Events')this.value=''" onblur="if(this.value=='')this.value='Search Events'"
                                Text="Search Events"></asp:TextBox>
                            <a href="#" onclick="$('#MainContent_tb_SearchIgnore').val('Search Events');return false;"
                                class="searchbox_clear"></a>
                            <asp:LinkButton ID="imgbtn_searchIgnore" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                OnClick="imgbtn_search_Click" />
                        </asp:Panel>
                    </div>
                    <span class="float-right margin-left-big">Leave search textbox blank to search
                    all events</span>
                    <div class="clear-space">
                    </div>
                    <div class="float-left">
                        Sorted from newest to oldest Ignore certain requests from being recorded if it contains or equals a certain event momment.
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="lbtn_refreshignore" runat="server" OnClick="lbtn_refreshignore_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                        <asp:LinkButton ID="lbtnClearAllIgnored" runat="server" OnClientClick="return ConfirmClearAllIgnored(this);">Clear All Ignored</asp:LinkButton>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:GridView ID="gv_Ignore" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                        Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                        <EmptyDataRowStyle ForeColor="Black" />
                        <RowStyle CssClass="GridNormalRow" />
                        <EmptyDataTemplate>
                            <div class="emptyGridView">
                                No Ignored Events Available
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                        <tr>
                                            <td style="width: 41px;"></td>
                                            <td style="width: 150px;">Date
                                            </td>
                                            <td style="width: 65px;">Hits
                                            </td>
                                            <td>Event Message
                                            </td>
                                            <td style="width: 100px; text-align: center;">Actions
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                        <tbody>
                                            <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                <%#Container.DataItemIndex + 1 %>
                                            </td>
                                            <td class="border-right" style="width: 151px;">
                                                <%#Eval("date") %>
                                            </td>
                                            <td class="border-right" style="width: 65px; text-align: center;">
                                                <%#Eval("timesHit") %>
                                            </td>
                                            <td class="border-right" valign="middle">
                                                <%#Eval("comment") %>
                                            </td>
                                            <td class='border-right' style="width: 101px; text-align: center;">
                                                <%#Eval("refresh") %>
                                                <%#Eval("ignore") %>
                                            </td>
                                        </tbody>
                                    </table>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="activity" class="pnl-section" style="display: none">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <div class="table-settings-box no-border">
                        <div class="td-settings-ctrl">
                            <div class="float-left pad-right-big">
                                <b class="pad-right">Update Interval</b>
                                <asp:DropDownList ID="dd_interval" CssClass="margin-left" runat="server" ClientIDMode="Static">
                                    <asp:ListItem Text="1 Second" Value="1000"></asp:ListItem>
                                    <asp:ListItem Text="2 Seconds" Value="2000"></asp:ListItem>
                                    <asp:ListItem Text="5 Seconds" Value="5000" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="10 Seconds" Value="10000"></asp:ListItem>
                                    <asp:ListItem Text="15 Seconds" Value="15000"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <a href="#" id="imgPausePlay" class="margin-left-sml margin-top-sml margin-right-sml float-left cursor-pointer img-pause"
                                title="Pause/Play"></a>
                            <div class="float-left pad-left-big">
                                <b class="pad-right">Time to Track</b>
                                <asp:DropDownList ID="dd_track" CssClass="margin-left" runat="server" ClientIDMode="Static">
                                    <asp:ListItem Text="5 Second" Value="5"></asp:ListItem>
                                    <asp:ListItem Text="10 Seconds" Value="10"></asp:ListItem>
                                    <asp:ListItem Text="15 Seconds" Value="15"></asp:ListItem>
                                    <asp:ListItem Text="20 Seconds" Value="20"></asp:ListItem>
                                    <asp:ListItem Text="25 Seconds" Value="25"></asp:ListItem>
                                    <asp:ListItem Text="30 Seconds" Value="30" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="1 Minute" Value="60"></asp:ListItem>
                                    <asp:ListItem Text="2 Minute" Value="120"></asp:ListItem>
                                    <asp:ListItem Text="3 Minute" Value="180"></asp:ListItem>
                                    <asp:ListItem Text="4 Minute" Value="240"></asp:ListItem>
                                    <asp:ListItem Text="5 Minute" Value="300"></asp:ListItem>
                                    <asp:ListItem Text="1 hour" Value="3600"></asp:ListItem>
                                    <asp:ListItem Text="12 hour" Value="7200"></asp:ListItem>
                                    <asp:ListItem Text="1 Day" Value="86400"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="clear-space">
                            </div>
                            <table width="100%" cellpadding="10" cellspacing="10" border="0">
                                <tr>
                                    <td valign="top" style="width: 250px;">
                                        <div id="network-info-div" class="pad-right-big">
                                            <h3 align="center" class="font-bold">Network Information</h3>
                                            <div class="clear-space">
                                            </div>
                                            <div id="pnl_NetworkInfoHolder">
                                            </div>
                                        </div>
                                    </td>
                                    <td valign="top">
                                        <asp:Panel ID="pnl_interactivityholder" runat="server">
                                            <table width="100%">
                                                <tbody>
                                                    <tr>
                                                        <td valign="top">
                                                            <div id="ChartRequests">
                                                                <h3>Loading Google Chart. Please Wait...</h3>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td valign="top">
                                                            <div id="statholder">
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="td-settings-desc">
                            If charts are not updating every given time
            interval, switch to another browser. (Works best in Google Chrome and Internet Explorer
            8+).
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="loginActivity" class="pnl-section" style="display: none;">
            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                <ContentTemplate>
                    <div class="table-settings-box no-border">
                        <div class="td-settings-ctrl">
                            <div class="float-left pad-right">
                                <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                            </div>
                            <div class="float-left pad-left">
                                <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Blocked</b>
                            </div>
                            <a id="clearloginsearch" href="#" class="float-left pad-left-big margin-left-big" onclick="ClearIpSearch();return false;" style="display: none;">Clear Search
                            </a>
                            <div class="float-left pad-left-big margin-left-big">
                                <span id="loginactError"></span>
                            </div>
                            <div class="float-right">
                                <asp:LinkButton ID="lbtn_refreshLoginactivity" runat="server" OnClick="lbtn_refreshLoginactivity_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                                <asp:LinkButton ID="lbtnClearAllLogin" runat="server" OnClientClick="return ConfirmClearAllLoginActivity(this);">Clear All Login Activity</asp:LinkButton>
                            </div>
                            <div class="clear-space">
                            </div>
                            <asp:GridView ID="gv_LoginActivity" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                                Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                                <EmptyDataRowStyle ForeColor="Black" />
                                <RowStyle CssClass="GridNormalRow" />
                                <EmptyDataTemplate>
                                    <div class="emptyGridView">
                                        No Login Activity Available
                                    </div>
                                </EmptyDataTemplate>
                                <Columns>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                                <tr>
                                                    <td style="width: 41px;"></td>
                                                    <td style="width: 100px;">IP Address
                                                    </td>
                                                    <td style="width: 205px;">Name Used
                                                    </td>
                                                    <td>Event Message
                                                    </td>
                                                    <td style="width: 65px;">Type
                                                    </td>
                                                    <td style="width: 50px;">Valid
                                                    </td>
                                                    <td style="width: 165px;">Date
                                                    </td>
                                                    <td style="width: 75px; text-align: center;">Actions
                                                    </td>
                                                </tr>
                                            </table>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <table class="myItemStyle ipsearchrow" cellpadding="5" cellspacing="0">
                                                <tbody>
                                                    <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                        <%#Container.DataItemIndex + 1 %>
                                                    </td>
                                                    <td class="border-right" style="width: 100px; text-align: center;">
                                                        <span class="ipsearchval" style="display: none;"><%#Eval("IpAddressNoLink") %></span>
                                                        <%#Eval("IpAddress") %>
                                                    </td>
                                                    <td class="border-right" style="width: 205px; text-align: center;">
                                                        <%#Eval("UserName") %>
                                                    </td>
                                                    <td class="border-right" valign="middle">
                                                        <%#Eval("Message") %>
                                                    </td>
                                                    <td class="border-right" style="width: 65px; text-align: center;">
                                                        <%#Eval("ActType") %>
                                                    </td>
                                                    <td class="border-right" style="width: 50px; text-align: center;">
                                                        <%#Eval("Success") %>
                                                    </td>
                                                    <td class="border-right" style="width: 165px; text-align: center;">
                                                        <%#Eval("Date") %>
                                                    </td>
                                                    <td class='border-right' style="width: 75px; text-align: center;">
                                                        <%#Eval("AllowBlock") %>
                                                        <%#Eval("Delete") %>
                                                    </td>
                                                </tbody>
                                            </table>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                            <div id="loginactivitysearch" class="emptyGridView" style="display: none;">
                                No Login Activity Found
                            </div>
                            <div class="clear-space"></div>
                        </div>
                        <div class="td-settings-desc">
                            This list is partially populated from the IP Watchlist. Blocking or allowing an IP Address on this activity log will update the IP Watchlist. Sorted from newest to oldest.
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="ipwatchlist" class="pnl-section" style="display: none;">
            <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                <ContentTemplate>
                    <asp:Literal ID="ltl_lockedipwatchlist" runat="server"></asp:Literal>
                    <div class="table-settings-box no-border">
                        <div class="td-settings-title">
                            IP Watch List
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="clear-space"></div>
                            <div class="float-left pad-right">
                                <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                            </div>
                            <div class="float-left pad-left">
                                <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Blocked</b>
                            </div>
                            <asp:LinkButton ID="lbtn_refreshIPWatchList" runat="server" OnClick="lbtn_refreshIPWatchList_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-left margin-left img-refresh" Style="margin-left: 402px;"></asp:LinkButton>
                            <div class="clear-space"></div>
                            <asp:GridView ID="GV_WatchList" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                                Width="575px" GridLines="None" OnRowCommand="GV_WatchList_RowCommand" AllowPaging="false"
                                ShowHeaderWhenEmpty="True" OnRowDeleting="GV_WatchList_RowDeleting" OnRowDataBound="GV_WatchList_RowDataBound">
                                <AlternatingRowStyle CssClass="GridAlternate" />
                                <EmptyDataRowStyle ForeColor="Black" />
                                <RowStyle CssClass="GridNormalRow" />
                                <EmptyDataTemplate>
                                    <div class="emptyGridView">
                                        There are no IP Addresses being watched
                                    </div>
                                </EmptyDataTemplate>
                                <Columns>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                                <tr>
                                                    <td style="width: 41px;"></td>
                                                    <td>IP Address
                                                    </td>
                                                    <td width="60px">Attempts
                                                    </td>
                                                    <td width="150px">Last Attempt
                                                    </td>
                                                    <td width="75px">Actions
                                                    </td>
                                                </tr>
                                            </table>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                                <tr>
                                                    <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                        <%#Container.DataItemIndex + 1 %>
                                                    </td>
                                                    <td class="border-right">
                                                        <div class="pad-left">
                                                            <font style="font-size: 14px; font-weight: bold;">
                                                                <%#Eval("ip")%></font><small>-
                                                                    <%#Eval("blockMessage") %></small>
                                                        </div>
                                                    </td>
                                                    <td width="60px" align="center" class="border-right">
                                                        <%#Eval("attempts")%>
                                                    </td>
                                                    <td width="150px" align="center" class="border-right">
                                                        <%#Eval("LastAttempt")%>
                                                    </td>
                                                    <td width="75px" align="center" class="border-right">
                                                        <asp:LinkButton ID="lb_block" runat="server" CommandName="block" CommandArgument='<%#Eval("ip") %>'
                                                            CssClass='<%#Eval("blocked") %>' ToolTip="Block/Allow IP"></asp:LinkButton>
                                                        <asp:LinkButton ID="lb_delete" runat="server" CommandName="delete" CommandArgument='<%#Eval("ip") %>'
                                                            ToolTip="Delete from IP List" CssClass="td-delete-btn RandomActionBtns"></asp:LinkButton>
                                                        <asp:Label ID="lb_blockActions_na" runat="server" Text="-" Enabled="false" Visible="false"></asp:Label>
                                                    </td>
                                                </tr>
                                            </table>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                            <div class="clear" style="height: 35px">
                            </div>
                            <asp:Panel ID="pnl_addip" runat="server" DefaultButton="btn_createnew_listener">
                                <table cellpadding="10" cellspacing="10">
                                    <tbody>
                                        <tr>
                                            <td align="right" style="width: 185px;">
                                                <span class="pad-right font-bold">Add IP Address</span>
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_createnew_listener"
                                                    runat="server" CssClass="textEntry margin-right-big" Width="150px"></asp:TextBox>
                                                <asp:Button ID="btn_createnew_listener" runat="server" Text="Add IP Address" CssClass="input-buttons margin-right-big RandomActionBtns"
                                                    OnClick="btn_createnew_listener_Click" />
                                                <div class="clear-space-five"></div>
                                                <small>Enter a new ip address for the site to block.</small>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div class="clear-space-two">
                                </div>
                                <div id="ipwatch_postmessage" runat="server" style="margin-left: 107px;">
                                </div>
                                <div class="clear" style="height: 20px;">
                                </div>
                            </asp:Panel>
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
                    <asp:Literal ID="ltl_lockediplistener" runat="server"></asp:Literal>
                    <div class="table-settings-box no-border">
                        <div class="td-settings-title">
                            IP Listener List
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="clear"></div>
                            <asp:Panel ID="pnl_iplistener_holder" runat="server">
                            </asp:Panel>
                            <div id="listener_postmessage" style="margin-left: 205px; margin-top: -10px;">
                            </div>
                            <asp:HiddenField ID="hf_UpdateIPListener" ClientIDMode="Static" runat="server" OnValueChanged="hf_UpdateIPListener_ValueChanged" />
                            <div class="clear" style="height: 20px;">
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            The IP Listener can be used to set up an <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://en.wikipedia.org/wiki/Intranet', this);return false;">internal</a> only version of the site and/or only allowing certain ip address to
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
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Record Network Activity
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="cb_netactOn" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="cb_netactOn_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="cb_netactOff" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="cb_netactOff_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Disable this feature if you dont want the website to track errors and Sql database changes.
                        </div>
                    </div>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Recorded Events to Keep
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:Panel ID="pnl_rtk_E" runat="server" DefaultButton="btn_Update_rtk_E">
                                <asp:TextBox ID="tb_Records_to_keep_E" runat="server" CssClass="textEntry margin-right"
                                    Width="65px" TextMode="Number"></asp:TextBox><asp:Button ID="btn_Update_rtk_E" runat="server" CssClass="input-buttons RandomActionBtns"
                                        Text="Update" OnClick="btn_Update_rtk_E_Clicked" />
                            </asp:Panel>
                            <div class="clear"></div>
                        </div>
                        <div class="td-settings-desc">
                            Change the number of events to keep when events are recorded to the Activity log. Older events will be deleted first before adding new ones.
                        </div>
                    </div>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Disable Javascript Error Alerts
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_DisableJavascriptErrorAlerts_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_DisableJavascriptErrorAlerts_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_DisableJavascriptErrorAlerts_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_DisableJavascriptErrorAlerts_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Set this to no to allow the javascript to notify errors to the user .
                        </div>
                    </div>
                    <asp:Panel ID="pnl_RecordLogFile" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Record Login Activity
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_recordLoginActivity_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_recordLoginActivity_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_recordLoginActivity_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_recordLoginActivity_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Disable this feature if you dont want the website to track
                                                user logins and logoffs.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Record Errors Only
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_RecordErrorsOnly_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_RecordErrorsOnly_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_RecordErrorsOnly_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_RecordErrorsOnly_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Enable this feature if you only want to record errors.
                            </div>
                        </div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Record Errors To Log File
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_recordLogFile_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_recordLogFile_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_recordLogFile_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_recordLogFile_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Disable this feature if you dont want to save a physical log file to the Logging folder.
                            </div>
                        </div>
                    </asp:Panel>
                    <div id="tableEmailAct" runat="server" class="table-settings-box">
                        <div class="td-settings-title">
                            Alert Upon Error
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="cb_emailon" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="cb_emailon_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="cb_emailoff" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="cb_emailoff_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Disable this feature if you dont want to recieve error alerts.
                        </div>
                    </div>
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
                                            Width="50px" TextMode="Number"></asp:TextBox><asp:Button ID="btn_autoblock_count" runat="server" CssClass="input-buttons RandomActionBtns"
                                                Text="Update" OnClick="btn_autoblock_count_Clicked" />
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Set the number of attempts before IP is blocked. (Must be greater than 0)
                                </div>
                            </div>
                        </asp:Panel>
                    </asp:Panel>
                    <asp:Panel ID="pnl_autoDeleteLoginActivity" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Auto Delete Login Activity
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_autoDeleteLoginActivity_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_autoDeleteLoginActivity_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_autoDeleteLoginActivity_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_autoDeleteLoginActivity_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Set to True if you want the site to automatically delete the login activity after a certain amount of days.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_autoDeleteLoginActivity_days" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Days to Keep Login Activity
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_daystokeepLoginActivity_holder" runat="server" DefaultButton="btn_daystokeepLoginActivity">
                                        <asp:TextBox ID="tb_daystokeepLoginActivity" runat="server" CssClass="textEntry margin-right-big"
                                            Width="50px" TextMode="Number"></asp:TextBox><span class="pad-left-sml pad-right-big">Day(s)</span><asp:Button ID="btn_daystokeepLoginActivity" runat="server" CssClass="input-buttons RandomActionBtns"
                                                Text="Update" OnClick="btn_daystokeepLoginActivity_Clicked" />
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Set the number of days to keep for the login acitivty. (Must be greater than 0)
                                </div>
                            </div>
                        </asp:Panel>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>
