<%@ Page Title="Analytics" Async="true" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Analytics.aspx.cs" Inherits="SiteTools_Analytics" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Analytics
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hf_resetPageCount" runat="server" ClientIDMode="Static" OnValueChanged="hf_resetPageCount_ValueChanged" />
            <asp:HiddenField ID="hf_DeleteLoginEvent" runat="server" OnValueChanged="hf_DeleteLoginEvent_Changed" />
            <asp:HiddenField ID="hf_AllowBlockLoginEvent" runat="server" OnValueChanged="hf_AllowBlockLoginEvent_Changed" />
            <asp:HiddenField ID="hf_DeleteUserToIgnore" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteUserToIgnore_ValueChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <div id="activity" class="pnl-section">
        <div class="table-settings-box">
            <div class="td-settings-title">
                Common statistics
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <a href="javascript:__doPostBack('ctl00$MainContent$btn_refreshPageViews','')" class="float-right RandomActionBtns">Refresh</a>
                <div class="clear-space"></div>
                <asp:UpdatePanel ID="updatePnl_CommonStats" runat="server">
                    <ContentTemplate>
                        <div id="div_NewVisitors" class="common-stats-div">
                            <asp:Label ID="lbl_NewVisitors_Count" runat="server" CssClass="common-stats-count" Text="0"></asp:Label>
                            <div class="common-stats-title">New Visitors</div>
                            <div class="common-stats-moreinfo" onclick="LoadCommonStatisticsModal('div_NewVisitors');">More info<span class="common-stats-moreinfo-image"></span></div>
                        </div>
                        <div id="div_RegisteredUsers" class="common-stats-div">
                            <asp:Label ID="lbl_RegisteredUsers_Count" CssClass="common-stats-count" runat="server" Text="0"></asp:Label>
                            <div class="common-stats-title">New Registered Users</div>
                            <div class="common-stats-moreinfo" onclick="LoadCommonStatisticsModal('div_RegisteredUsers');">More info<span class="common-stats-moreinfo-image"></span></div>
                        </div>
                        <div id="div_RecentLogins" class="common-stats-div">
                            <asp:Label ID="lbl_RecentLogins_Count" runat="server" CssClass="common-stats-count" Text="0"></asp:Label>
                            <div class="common-stats-title">Recent Logins</div>
                            <div class="common-stats-moreinfo" onclick="LoadCommonStatisticsModal('div_RecentLogins');">More info<span class="common-stats-moreinfo-image"></span></div>
                        </div>
                        <div class="clear"></div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Statistics are based off of the past
                <asp:Label ID="lbl_DaysToTrackCommonStatistics" runat="server" Text="5"></asp:Label>
                days from the current date.
            </div>
        </div>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Network Traffic and Requests
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <div class="input-settings-holder">
                    <span class="font-bold">Update Interval</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_interval" CssClass="float-left margin-right" runat="server" ClientIDMode="Static">
                        <asp:ListItem Text="1 Second" Value="1000"></asp:ListItem>
                        <asp:ListItem Text="2 Seconds" Value="2000"></asp:ListItem>
                        <asp:ListItem Text="5 Seconds" Value="5000" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="10 Seconds" Value="10000"></asp:ListItem>
                        <asp:ListItem Text="15 Seconds" Value="15000"></asp:ListItem>
                    </asp:DropDownList>
                    <a href="#" id="imgPausePlay" class="float-left margin-left cursor-pointer img-pause"
                        title="Pause/Play" style="margin-top: 5px;"></a>
                    <div class="clear"></div>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Time to Track</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_track" runat="server" ClientIDMode="Static">
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
                <div class="clear-space-five">
                </div>
                <div id="network-info-div">
                    <div id="pnl_NetworkInfoHolder">
                    </div>
                </div>
                <div class="clear"></div>
                <asp:Panel ID="pnl_interactivityholder" runat="server">
                    <div class="clear-space-five"></div>
                    <div id="ChartRequests">
                        <h3 class="pad-top-big pad-bottom-big">Loading Google Chart. Please Wait...</h3>
                    </div>
                    <div class="clear-space"></div>
                </asp:Panel>
            </div>
            <div id="pnl_interactivityholder_Description" class="td-settings-desc">
                If charts are not updating every given time interval, switch to another browser. (Works best in Google Chrome and Internet Explorer 8+).
            </div>
        </div>
        <asp:UpdatePanel ID="updatpnl_PageViews" runat="server">
            <ContentTemplate>
                <div id="pnl_individualpageviews" clientidmode="Static" runat="server" class="table-settings-box">
                    <div class="td-settings-title">
                        Page Views
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:LinkButton ID="btn_refreshPageViews" runat="server" OnClientClick="loadingPopup.Message('Updating...');" OnClick="btn_refreshPageViews_Click" CssClass="float-right" Text="Refresh"></asp:LinkButton>
                        <div id="page-view-textcol">
                            <asp:Panel ID="pnl_individualPageRequests" runat="server" ClientIDMode="Static">
                            </asp:Panel>
                        </div>
                        <div class="clear"></div>
                    </div>
                    <div class="td-settings-desc">
                        The list above shows all the views for a page. You can reset the totals for each individual page by clicking the "X" button.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="loginActivity" class="pnl-section" style="display: none;">
        <asp:HiddenField ID="hf_GoogleMapsAPIKey" Value="" runat="server" ClientIDMode="Static" />
        <asp:UpdatePanel ID="UpdatePanel5" runat="server">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <div class="float-left pad-right">
                            <span id="loginactError"></span>
                        </div>
                        <div class="float-right">
                            <asp:LinkButton ID="lbtn_refreshLoginactivity" runat="server" OnClick="lbtn_refreshLoginactivity_Click" CssClass="RandomActionBtns margin-left-big" Text="Refresh"></asp:LinkButton>
                        </div>
                        <div class="float-right">
                            <asp:LinkButton ID="lbtnClearAllLogin" runat="server" OnClientClick="return ConfirmClearAllLoginActivity(this);">Clear All Login Activity</asp:LinkButton>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_loginactivity" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        This list is partially populated from the IP Watchlist. Blocking or allowing an IP Address on this activity log will update the IP Watchlist. Sorted from newest to oldest.
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="lbtn_refreshLoginactivity" />
            </Triggers>
        </asp:UpdatePanel>
        <div class="pad-left pad-right">
            <div id="loginmap_div" style="display: none;"></div>
            <div id="loginmap_div_loading">
                <h3 class="pad-top-big pad-bottom-big">Please wait while we load the map. This may take a few seconds.</h3>
            </div>
            <div id="loginmap_div_error" style="display: none;">Google Maps API Key is required. Go to Site Settings page under the Administrative Settings tab to add the API Key.</div>
        </div>
    </div>
    <div id="networkSettings" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
        <asp:UpdatePanel ID="UpdatePanel8" runat="server">
            <ContentTemplate>
                <div id="analyticsAccordion" class="custom-accordion">
                    <h3>Login Activity</h3>
                    <div>
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
                                                Width="50px" TextMode="Number" min="0"></asp:TextBox><span class="pad-left-sml pad-right-big">Day(s)</span><asp:Button ID="btn_daystokeepLoginActivity" runat="server" CssClass="input-buttons RandomActionBtns"
                                                    Text="Update" OnClick="btn_daystokeepLoginActivity_Clicked" />
                                        </asp:Panel>
                                    </div>
                                    <div class="td-settings-desc">
                                        Set the number of days to keep for the login acitivty. (Must be greater than 0)
                                    </div>
                                </div>
                            </asp:Panel>
                        </asp:Panel>
                    </div>
                    <h3>Page Views</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Record Page Views
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_RecordPageViews_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_RecordPageViews_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_RecordPageViews_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_RecordPageViews_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Set this to Off if you don't want to record the page view count for when users navigate the site.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_userstoignore_holder" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Page View Count - Users to Ignore
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="Panel3" runat="server" DefaultButton="btn_AddUserToIgnore">
                                        <asp:TextBox ID="tb_AddUserToIgnore" runat="server" placeholder="Enter Username" CssClass="textEntry margin-right users-to-ignore-tb float-left"
                                            Width="250px"></asp:TextBox><asp:LinkButton ID="btn_AddUserToIgnore" runat="server" CssClass="td-add-btn float-left RandomActionBtns margin-top-sml"
                                                ToolTip="Add User" OnClick="btn_AddUserToIgnore_Clicked" />
                                        <div class="clear"></div>
                                    </asp:Panel>
                                    <div class="clear-space"></div>
                                    <asp:Panel ID="pnl_userstoignore" runat="server">
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Add/Delete users to that you would like to ignore when incrementing the Page View count. 
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <h3>Site Requests</h3>
                    <div>
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Record Site Requests
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_recordSiteRequests_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_recordSiteRequests_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_recordSiteRequests_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_recordSiteRequests_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Set this to Off if you don't want to record site requests that are added into the system with every request by any user.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_MaxRequestSize" runat="server" DefaultButton="btn_requestRecordSize">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Max Request Size
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:TextBox ID="tb_maxRequestSize" runat="server" CssClass="textEntry margin-right-big"
                                        Width="75px" TextMode="Number"></asp:TextBox><asp:Button ID="btn_requestRecordSize" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Update" OnClick="btn_requestRecordSize_Clicked" />
                                </div>
                                <div class="td-settings-desc">
                                    Set this to Off if you don't want to record site requests that are added into the system with every request by any user.
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>

