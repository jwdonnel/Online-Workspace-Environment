<%@ Page Title="Site Settings" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="SiteSettings.aspx.cs" Inherits="SiteTools_SiteSettings" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .ajax__slider_h_rail
        {
            width: 178px !important;
            margin-right: 10px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div id="sitesettings">
            <div id="MainSettings">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_ServerSettings" CssClass="pnl-section" runat="server">
                            <div class="editor_titles">
                                <div class="title-line"></div>
                                <h3>Site/Server Settings</h3>
                            </div>
                            <div class="clear-space-five">
                            </div>
                            <small><b class="pad-right-sml">Note:</b>Server side settings and overrides will effect every user. Not Administrative users will not be able to edit certain settings.</small>
                            <div class="clear-space">
                            </div>
                            <div class="pad-all">
                                <asp:Panel ID="pnl_autoupdate" runat="server">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Auto Update System</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_autoupdate_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                        OnCheckedChanged="rb_autoupdate_on_CheckedChanged" AutoPostBack="True" />
                                                    <asp:RadioButton ID="rb_autoupdate_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                        OnCheckedChanged="rb_autoupdate_off_CheckedChanged" AutoPostBack="True" />
                                                </div>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Turning off the Auto Update System may cause some features to stop working, but might increase performance.</small>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Cache Workspace</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_cachehp_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_cachehp_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_cachehp_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_cachehp_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Turn On to cache the user workspace. (Disabled by default)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table id="ClearAppProp_Controls" cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Clear User App&nbsp;&nbsp;<br />
                                                Properties</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_clearapps" runat="server" Text="Clear Properties" OnClick="btn_clearapps_Click"
                                                CssClass="RandomActionBtns input-buttons" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Delete all entries in the UserApps Table. (Used for fixing
                                                errors on the user side)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table id="ClearUserNoti_Controls" cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Clear User Notifications</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_clearnotiall" runat="server" Text="Clear Notifications" OnClick="btn_clearnotiall_Click"
                                                CssClass="RandomActionBtns input-buttons" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Delete all entries in the UserNotifications Table.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table id="Table1" cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Clear User Update&nbsp;&nbsp;<br />
                                                Flags</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_clearflagall" runat="server" Text="Clear User Flags" OnClick="btn_clearflagall_Click"
                                                CssClass="RandomActionBtns input-buttons" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Delete all entries in the UserUpdateFlags Table.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Clear Sever Cache</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_clearCache" runat="server" Text="Clear Cache" CssClass="input-buttons margin-right RandomActionBtns"
                                                OnClick="btn_clearCache_Click" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small><span class="font-bold">Last Cache Clear:</span>
                                                <asp:Label ID="lbl_lastcacheclear" runat="server" Text="" CssClass="pad-left-sml"></asp:Label></small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <asp:Panel ID="pnl_updateadminnote" runat="server" DefaultButton="btn_updateadminnote">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Administrator Note</span>
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_adminnote" runat="server" CssClass="textEntry float-left" Width="350px"
                                                    AutoPostBack="False" TextMode="MultiLine" Height="40px" Font-Names='"Arial"'
                                                    BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                                    ForeColor="#353535"></asp:TextBox>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:Button ID="btn_updateadminnote" runat="server" CssClass="no-margin RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateadminnote_Click" />
                                                <div class="clear-space-five">
                                                </div>
                                                <asp:LinkButton ID="btn_updateadminnote_clear" runat="server" CssClass="RandomActionBtns sb-links"
                                                    OnClick="btn_updateadminnote_clear_Click" Style="color: #467DB7!important">Clear</asp:LinkButton>
                                            </td>
                                            <td>
                                                <small>Add a note for all users to see on a workspace overlay.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <small>
                                        <div style="padding-left: 210px;">
                                            <asp:Label ID="lbl_adminnoteby" runat="server" Text=""></asp:Label>
                                        </div>
                                    </small>
                                    <asp:Panel ID="pnlLoginMessage" runat="server">
                                        <div class="clear-space">
                                        </div>
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Login Page Message</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_loginPageMessage" runat="server" CssClass="textEntry float-left"
                                                        Width="350px" AutoPostBack="False" TextMode="MultiLine" Height="40px" Font-Names='"Arial"'
                                                        BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                                        ForeColor="#353535"></asp:TextBox>
                                                </td>
                                                <td class="td-settings-ctrl">
                                                    <asp:Button ID="btn_loginPageMessage" runat="server" CssClass="no-margin RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_loginPageMessage_Click" />
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:LinkButton ID="lbtn_loginPageMessage" runat="server" CssClass="RandomActionBtns sb-links"
                                                        OnClick="lbtn_loginPageMessage_clear_Click" Style="color: #467DB7!important">Clear</asp:LinkButton>
                                                </td>
                                                <td>
                                                    <small>Add a note for all users to see on the Login Page.</small>
                                                </td>
                                            </tr>
                                        </table>
                                        <small>
                                            <div style="padding-left: 210px;">
                                                <asp:Label ID="lbl_loginMessageDate" runat="server" Text="N/A"></asp:Label>
                                            </div>
                                        </small>
                                    </asp:Panel>
                                </asp:Panel>
                                <div class="clear-space">
                                </div>
                                <asp:Panel ID="pnl_updateFolder" runat="server" DefaultButton="btn_updateFolder">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">FileDrive Folder</span>
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_updateFolder" runat="server" CssClass="textEntry" Width="361px"></asp:TextBox><br />
                                                <asp:LinkButton ID="btn_usedefaultloc" runat="server" CssClass="sb-links RandomActionBtns margin-top-sml"
                                                    OnClick="btn_usedefaultloc_Click">Use default path</asp:LinkButton>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:Button ID="btn_updateFolder" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_updateFolder_Click" />
                                            </td>
                                            <td>
                                                <small>This sets the root path for the FileDrive App to search through.</small>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_NetworkActivitySettings" runat="server" CssClass="pnl-section">
                            <div class="editor_titles">
                                <div class="title-line"></div>
                                <h3>Network Monitor Settings</h3>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="pad-all">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Record Login Activity</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_recordLoginActivity_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_recordLoginActivity_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_recordLoginActivity_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_recordLoginActivity_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Disable this feature if you dont want the website to track
                                                user logins and log offs.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Record Network Activity</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="cb_netactOn" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="cb_netactOn_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="cb_netactOff" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="cb_netactOff_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Disable this feature if you dont want the website to track
                                                user inneractions.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table id="tableEmailAct" runat="server" cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Alert Upon Error</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="cb_emailon" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="cb_emailon_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="cb_emailoff" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="cb_emailoff_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Disable this feature if you dont want to recieve error alerts.</small>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_emailSettings" runat="server" CssClass="pnl-section">
                            <div class="editor_titles">
                                <div class="title-line"></div>
                                <h3>Mail Settings</h3>
                            </div>
                            <div class="float-left pad-top-big">
                                <small>The mail settings are used for outgoing email such as the notifications, updates,
                            and message forwarding.</small>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="pad-all">
                                <asp:Panel ID="pnl_mailsettings" runat="server">
                                    <div class="clear-margin" style="font-size: 12px;">
                                        <div class="float-left pad-right-big pad-left">
                                            <b class="pad-right-sml">Updated By:</b><asp:Label ID="lbl_updatedbymailsettings"
                                                runat="server" Text="N/A"></asp:Label>
                                        </div>
                                        <div class="float-left pad-left-big pad-right-big">
                                            <b class="pad-right-sml">Date Updated:</b><asp:Label ID="lbl_dateupdatedmailsettings"
                                                runat="server" Text="N/A"></asp:Label>
                                        </div>
                                        <div class="float-left pad-left-big pad-right-big" style="margin-top: -4px;">
                                            <asp:LinkButton ID="lbtn_TurnOnOff_Email" runat="server" CssClass="RandomActionBtns sb-links"
                                                OnClick="lbtn_TurnOnOff_Email_Clicked" Style="color: #467DB7!important;"></asp:LinkButton>
                                        </div>
                                        <div class="float-left pad-left-big">
                                            <h3>
                                                <span class="pad-right font-bold">Status:</span><asp:Label ID="lbl_emailStatus" runat="server"></asp:Label></h3>
                                        </div>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">SMTP Server Address</span>
                                            </td>
                                            <td style="width: 200px;">
                                                <asp:TextBox ID="tb_smtpserver" runat="server" CssClass="textEntry margin-right-big"
                                                    Width="200px"></asp:TextBox>
                                            </td>
                                            <td align="right" style="width: 110px;">
                                                <span class="pad-right font-bold">Port Number</span>
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_portnumber" runat="server" CssClass="textEntry margin-right-big"
                                                    Width="50px" MaxLength="4"></asp:TextBox>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Enter in the smtp server address and port number<br />
                                                    that your outgoing mail will use. (Example: smtp.gmail.com:587)</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">SSL Enabled</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_ssl_enabled" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                        OnCheckedChanged="rb_ssl_enabled_Checked" AutoPostBack="true" />
                                                    <asp:RadioButton ID="rb_ssl_disabled" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                        OnCheckedChanged="rb_ssl_disabled_Checked" AutoPostBack="true" />
                                                </div>
                                            </td>
                                            <td class="td-settings-desc">
                                                <div class="pad-left-big">
                                                    <small>Check to make sure that the port number being used to send email is not SSL enabled.
                                                    If it is, click enabled for SSL.</small>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Email Address</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:TextBox ID="tb_usernamesmtp" runat="server" CssClass="textEntry"
                                                    Width="200px"></asp:TextBox>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Use the email address associated with the smtp server.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Email Password</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:TextBox ID="tb_passwordsmtp" runat="server" CssClass="textEntry"
                                                    Width="200px"></asp:TextBox>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Enter the password associated with the email address and smtp server.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Button ID="btn_updatemailsettings" runat="server" Text="Update Mail Settings"
                                        ClientIDMode="Static" CssClass="input-buttons RandomActionBtns float-left"
                                        OnClick="btn_updatemailsettings_Click" />
                                    <asp:LinkButton ID="lbtn_testconnection" runat="server" Text="Test Connection" Style="color: #467DB7!important"
                                        CssClass="sb-links margin-top-sml margin-left-big TestConnection float-left"
                                        OnClick="lbtn_testconnection_Click" />
                                    <asp:LinkButton ID="lbtn_SendTestEmail" runat="server" Style="color: #467DB7!important"
                                        CssClass="sb-links margin-top-sml margin-left-big TestConnection float-left" OnClick="lbtn_SendTestEmail_Click" />
                                    <div class="clear-space">
                                        <div class="clear-space">
                                        </div>
                                        <a id="btn_customizeSMTP" runat="server" clientidmode="Static" href="#iframecontent"
                                            class="sb-links margin-right float-left" onclick="openWSE.LoadIFrameContent('SiteTools/iframes/EmailSettings.aspx', this);return false;"
                                            style="display: block;"><span class="img-customize margin-right-sml float-left"></span>Customize Outgoing Email Messages</a>
                                    </div>
                                    <asp:Label ID="lbl_testconnection" runat="server" Visible="false" CssClass="float-left pad-left-big margin-left-big"
                                        Font-Size="Small" Style="padding-top: 7px;" Text=""></asp:Label>
                                    <small>
                                        <asp:Label ID="lbl_mailsettings_error" runat="server" ForeColor="Red" Visible="false"
                                            Style="margin-left: 180px;" Text="All fields must be filled out."></asp:Label></small>
                                    <div class="clear-space">
                                    </div>
                                </asp:Panel>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_admincontrolsonly" CssClass="pnl-section" runat="server" Enabled="false" Visible="false">
                            <div class="editor_titles">
                                <div class="title-line"></div>
                                <h3>Administrative Settings</h3>
                            </div>
                            <div class="float-left pad-top-big">
                                <small>These settings are only for the Administrator user. No users can see or modify these
                                settings.</small>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="pad-all">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Clear All User&nbsp;&nbsp;<br />
                                                Chat Logs</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_clearuserchats" runat="server" Text="Clear Chat Logs" OnClick="btn_clearuserchats_Click"
                                                CssClass="RandomActionBtns input-buttons" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Delete all user chat entries.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Show New Updates&nbsp;&nbsp;<br />
                                                on User Login</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Button ID="btn_ShowUpdates" runat="server" Text="Show Updates" OnClick="btn_showUpdates_Click"
                                                CssClass="RandomActionBtns input-buttons" Width="135px" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Will show the latest updates upon user login. (Will apply
                                                to all users) </small>
                                            <br />
                                            <small><span class="font-bold pad-right-sml">Last Updated:</span><asp:Label
                                                ID="lbl_dateUpdated_sup" runat="server"></asp:Label></small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Total Number of&nbsp;&nbsp;<br />
                                                Workspaces Allowed</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <asp:Panel ID="pnl_totalworkspacesAllowed" runat="server" DefaultButton="btn_updateTotalWorkspaces">
                                                <asp:TextBox ID="tb_totalWorkspacesAllowed" runat="server" CssClass="textEntry" Width="45px"></asp:TextBox>
                                                <asp:Button ID="btn_updateTotalWorkspaces" runat="server" Text="Update" OnClick="btn_updateTotalWorkspaces_Click"
                                                    CssClass="RandomActionBtns input-buttons margin-left" />
                                            </asp:Panel>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Determine how many workspaces a user is allowed to have.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Use SSL Redirect</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_sslredirect_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_sslredirect_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_sslredirect_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_sslredirect_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>If your site requires ssl to access, select True to redirect
                                                the http requests to https. May Slow down requests. (Will not redirect on apps) You must setup Bindings within IIS on your
                                                server to point to both https and http urls. (No users except the Administrator
                                                will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <asp:Panel ID="pnl_sslValidation" runat="server" Enabled="false" Visible="false">
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">URL Validation</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_urlvalidation_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                        OnCheckedChanged="rb_urlvalidation_on_CheckedChanged" AutoPostBack="True" />
                                                    <asp:RadioButton ID="rb_urlvalidation_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                        OnCheckedChanged="rb_urlvalidation_off_CheckedChanged" AutoPostBack="True" />
                                                </div>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>If you want to check if the URL is valid, enable this setting.</small>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                <div class="clear-space">
                                </div>
                                <table id="ChatClient_Controls" cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Chat Client</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_chatclient_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_chatclient_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_chatclient_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_chatclient_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Disabling this will disable all users from accessing the
                                                chat client.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Associate Apps&nbsp;&nbsp;<br />
                                                & Plugins with Groups</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_AssociateWithGroups_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_AssociateWithGroups_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_AssociateWithGroups_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_AssociateWithGroups_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Set this to Yes to lock apps, plugins, custom tables, database imports, and anything that is specific to groups. This is done by taking the creator and matching the group they are in.</small>                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock File Manager</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_LockFileManager_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_LockFileManager_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_LockFileManager_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_LockFileManager_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the File Manager so no other user can view the source
                                                code. (No users except the Administrator will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Site Plugins</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_siteplugins_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_siteplugins_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_siteplugins_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_siteplugins_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the Site Plugins feature so no user can upload, install
                                                them, or edit them.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Notifications&nbsp;&nbsp;<br />
                                                Manager</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_sitenotifi_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_sitenotifi_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_sitenotifi_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_sitenotifi_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the Site Notifications feature so no user can upload,
                                                install, or edit them.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Overlay&nbsp;&nbsp;<br />
                                                Manager</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_siteoverlay_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_siteoverlay_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_siteoverlay_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_siteoverlay_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the Site Overlay feature so no user can upload, install,
                                                or edit them.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock .ASCX&nbsp;&nbsp;<br />
                                                App Edit</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_lockascx_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_lockascx_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_lockascx_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_lockascx_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Prevent anyone (except the administrator) from creating
                                                and editing any .ascx file extension apps. Applies to the App Editor.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Custom Tables</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_lockcustomtables_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_lockcustomtables_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_lockcustomtables_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_lockcustomtables_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Prevent anyone (except the administrator) from creating
                                                a custom table.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Startup Scripts</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_Lockstartupscripts_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_Lockstartupscripts_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_Lockstartupscripts_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_Lockstartupscripts_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the Startup Scripts so no other user can edit. (No
                                                users except the Administrator will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock IP Listener</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_Lockiplisteneron" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_Lockiplisteneron_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_Lockiplisteneroff" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_Lockiplisteneroff_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the IP Listener so no other user can edit.(No users
                                                except the Administrator will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Lock Site&nbsp;&nbsp;<br />
                                                Customizations</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_siteCustomizations_On" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_siteCustomizations_On_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_siteCustomizations_Off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_siteCustomizations_Off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Lock the Site Customizations so no other user can edit.(No
                                                users except the Administrator will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Allow App Rating</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_allowapprating_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_allowapprating_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_allowapprating_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_allowapprating_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Set to No if you want to disable and hide all app ratings.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Allow User Privacy</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_allowUserPrivacy_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_allowUserPrivacy_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_allowUserPrivacy_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_allowUserPrivacy_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Set to true if you want to allow all users to decide if
                                                they want to be private. This will stop any logging of the user
                                                in the Network Log and disables the chat client for a user.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Site Status</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_siteonline" runat="server" Text="Online" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_siteonline_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_siteoffline" runat="server" Text="Offline" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_siteoffline_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Take the site offline. (No users except the Administrator
                                                will be able to access it)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Email On Created&nbsp;&nbsp;<br />
                                                Account</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_emailonReg_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_emailonReg_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_emailonReg_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_emailonReg_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Email the Administrator when a user creates a new account.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Hide All App Icons</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_hideAllAppIcons_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_hideAllAppIcons_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_hideAllAppIcons_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_hideAllAppIcons_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Select Yes to hide all app icons.</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <asp:Panel ID="pnl_showpreviewbutton" runat="server">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Show Preview Button&nbsp;&nbsp;<br />
                                                    on Login Screen</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_ShowPreviewButtonLogin_on" runat="server" Text="Show" CssClass="RandomActionBtns cb-enable"
                                                        OnCheckedChanged="rb_ShowPreviewButtonLogin_on_CheckedChanged" AutoPostBack="True" />
                                                    <asp:RadioButton ID="rb_ShowPreviewButtonLogin_off" runat="server" Text="Hide" CssClass="RandomActionBtns cb-disable"
                                                        OnCheckedChanged="rb_ShowPreviewButtonLogin_off_CheckedChanged" AutoPostBack="True" />
                                                </div>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Select Show to show the preview button on the login screen.
                                                    (Disabled by default)</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                </asp:Panel>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">No Login Required</span>
                                        </td>
                                        <td class="td-settings-ctrl">
                                            <div class="field switch inline-block">
                                                <asp:RadioButton ID="rb_nologinrequired_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                    OnCheckedChanged="rb_nologinrequired_on_CheckedChanged" AutoPostBack="True" />
                                                <asp:RadioButton ID="rb_nologinrequired_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                    OnCheckedChanged="rb_nologinrequired_off_CheckedChanged" AutoPostBack="True" />
                                            </div>
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Set to No to make the site available to anyone without
                                                an account.</small>
                                        </td>
                                    </tr>
                                </table>
                                <asp:Panel ID="pnl_NoLoginMainPage" runat="server" Enabled="false" Visible="false">
                                    <div class="clear-space">
                                    </div>
                                    <a href="#iframecontent" class="sb-links margin-right float-left" onclick="openWSE.LoadIFrameContent('SiteTools/UserMaintenance/AcctSettings.aspx?u=demouser&toolview=true', this);return false;"
                                        style="display: block;"><span class="img-customize margin-right-sml float-left"></span>No Login/Demo Customizations</a>
                                </asp:Panel>
                                <div class="clear-space">
                                </div>
                                <div class="clear-space">
                                </div>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="clear-space">
                            </div>
                            <asp:Panel ID="pnl_twitterSettings" CssClass="pnl-section" runat="server">
                                <div class="editor_titles">
                                    <div class="title-line"></div>
                                    <h3>
                                        <img id="twitter_logo" alt="" src="~/Standard_Images/ApiLoginImages/twitter_login.png" runat="server" />Twitter OAuth Settings</h3>
                                </div>
                                <div class="float-left pad-top-big">
                                    <small>Your application's OAuth settings. Keep the "Access Token Secret" and "Consumer Secret" a secret. This key
                                should never be human-readable.</small>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="pad-all">
                                    <asp:Panel ID="pnl_TwitterAccessToken" runat="server" DefaultButton="btn_updateTwitterAccessToken">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Access Token</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_updateTwitterAccessToken" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateTwitterAccessToken" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateTwitterAccessToken_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_TwitterAccessTokenSecret" runat="server" DefaultButton="btn_updateTwitterAccessTokenSecret">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Access Token Secret</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_updateTwitterAccessTokenSecret" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateTwitterAccessTokenSecret" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateTwitterAccessTokenSecret_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_TwitterConsumerKey" runat="server" DefaultButton="btn_updateTwitterConsumerKey">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Consumer Key</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_updateTwitterConsumerKey" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateTwitterConsumerKey" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateTwitterConsumerKey_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_TwitterConsumerSecret" runat="server" DefaultButton="btn_updateTwitterConsumerSecret">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Consumer Secret</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_updateTwitterConsumerSecret" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateTwitterConsumerSecret" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateTwitterConsumerSecret_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </div>
                                <div class="clear-space">
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnl_googleSettings" CssClass="pnl-section" runat="server">
                                <div class="editor_titles">
                                    <div class="title-line"></div>
                                    <h3>
                                        <img id="google_logo" alt="" src="~/Standard_Images/ApiLoginImages/google_login.png" runat="server" />Google OAuth 2.0 Settings</h3>
                                </div>
                                <div class="float-left pad-top-big">
                                    <small>Your Google OAuth 2.0 settings. Keep the "Client Secret" a secret. This key
                                should never be human-readable. Make sure your redirect url(s) match your Google App.</small>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="pad-left pad-top pad-bottom-big pad-right">
                                    <asp:Panel ID="pnl_GoogleClientId" runat="server" DefaultButton="btn_updateGoogleClientId">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Client ID</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txt_GoogleClientId" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateGoogleClientId" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateGoogleClientId_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_GoogleClientSecret" runat="server" DefaultButton="btn_updateGoogleClientSecret">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Client Secret</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txt_GoogleClientSecret" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateGoogleClientSecret" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateGoogleClientSecret_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space"></div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Redirect Url(s)</span>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltl_googleRedirect" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnl_facebookSettings" CssClass="pnl-section" runat="server" Style="padding-bottom: 20px;">
                                <div class="editor_titles">
                                    <div class="title-line"></div>
                                    <h3>
                                        <img id="facebook_img" alt="" src="~/Standard_Images/ApiLoginImages/facebook_login.png" runat="server" />Facebook API Settings</h3>
                                </div>
                                <div class="float-left pad-top-big">
                                    <small>Your Facebook Graph API settings. Keep the "App Secret" a secret. This key
                                should never be human-readable. Make sure your redirect url(s) match your Facebook App.</small>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="pad-left pad-top pad-right">
                                    <asp:Panel ID="pnl_facebookAppId" runat="server" DefaultButton="btn_updateFacebookAppId">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">App ID</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txt_facebookAppId" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateFacebookAppId" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateFacebookAppId_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_facebookAppSecret" runat="server" DefaultButton="btn_updateFacebookAppSecret">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">App Secret</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txt_facebookAppSecret" runat="server" CssClass="textEntry"
                                                        Width="400px"></asp:TextBox>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:Button ID="btn_updateFacebookAppSecret" runat="server" CssClass="margin-left RandomActionBtns input-buttons"
                                                        Text="Update" OnClick="btn_updateFacebookAppSecret_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <div class="clear-space"></div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Redirect Url(s)</span>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltl_facebookRedirect" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </asp:Panel>
                        </asp:Panel>
                        <asp:Panel ID="pnl_UserRegister" runat="server" CssClass="pnl-section">
                            <div id="userReg">
                                <div class="editor_titles">
                                    <div class="title-line"></div>
                                    <h3>User Registration</h3>
                                </div>
                                <div class="clear-space">
                                </div>
                                <div class="pad-all">
                                    <div id="rb_usersignup_holder" runat="server">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold">Allow User Signup</span>
                                                </td>
                                                <td class="td-settings-ctrl">
                                                    <div class="field switch inline-block">
                                                        <asp:RadioButton ID="rb_allowusersignup_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                            OnCheckedChanged="rb_allowusersignup_on_Checked" AutoPostBack="true" />
                                                        <asp:RadioButton ID="rb_allowusersignup_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                            OnCheckedChanged="rb_allowusersignup_off_Checked" AutoPostBack="true" />
                                                    </div>
                                                </td>
                                                <td class="td-settings-desc">
                                                    <small>Select On if you want to allow users to sign up for a new account on the login
                                                        screen.</small>
                                                </td>
                                            </tr>
                                        </table>
                                        <asp:Panel ID="pnl_socialSignIn_UserSignup" runat="server" Visible="false" Enabled="false">
                                            <div class="clear-space"></div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold">Google+ Sign In</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <div class="field switch inline-block">
                                                            <asp:RadioButton ID="rb_googlePlusSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_googlePlusSignIn_on_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_googlePlusSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_googlePlusSignIn_off_Checked" AutoPostBack="true" />
                                                        </div>
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Allow users to sign in using their Google+ accounts. <a href="https://developers.google.com/+/quickstart/csharp" target="_blank">Integration Information</a></small>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div class="clear-space"></div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold">Twitter Sign In</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <div class="field switch inline-block">
                                                            <asp:RadioButton ID="rb_twitterSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_twitterSignIn_on_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_twitterSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_twitterSignIn_off_Checked" AutoPostBack="true" />
                                                        </div>
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Allow users to sign in using their Twitter accounts. <a href="https://dev.twitter.com/web/sign-in/implementing" target="_blank">Integration Information</a></small>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div class="clear-space"></div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold">Facebook Sign In</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <div class="field switch inline-block">
                                                            <asp:RadioButton ID="rb_facebookSignIn_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_facebookSignIn_on_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_facebookSignIn_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_facebookSignIn_off_Checked" AutoPostBack="true" />
                                                        </div>
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Allow users to sign in using their Facebook accounts. <a href="https://developers.facebook.com/docs/facebook-login/login-flow-for-web/v2.2" target="_blank">Integration Information</a></small>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                        <asp:Panel ID="pnl_ConfirmationEmailSignUp" runat="server">
                                            <div class="clear-space">
                                            </div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold">Send Confirmation Email</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <div class="field switch inline-block">
                                                            <asp:RadioButton ID="rb_SignUpConfirmationEmail_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_SignUpConfirmationEmail_on_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_SignUpConfirmationEmail_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_SignUpConfirmationEmail_off_Checked" AutoPostBack="true" />
                                                        </div>
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Select Yes if you want to send a confirmation email to the user to verify email. Emails will only be sent if creating new account (Not signing in through other service).</small>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                        <asp:Panel ID="pnl_UserSignUp" runat="server">
                                            <div class="clear-space">
                                            </div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold">Email Association</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <div class="field switch inline-block">
                                                            <asp:RadioButton ID="rb_emailassociation_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_emailassociation_on_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_emailassociation_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_emailassociation_off_Checked" AutoPostBack="true" />
                                                        </div>
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Select On if you want to limit the user sign up to be associated with a certain
                                                            email address.</small>
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:Panel ID="pnl_emailassociation" runat="server" DefaultButton="btn_UpdateEmailAssociation">
                                                <div class="clear-space">
                                                </div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="td-settings-title">
                                                            <span class="pad-right font-bold font-color-black">Email Address</span>
                                                        </td>
                                                        <td class="td-settings-ctrl">
                                                            <b>@</b><asp:TextBox ID="tb_EmailAssociation" runat="server" CssClass="textEntry margin-left margin-right"
                                                                Width="150px"></asp:TextBox><div class="clear-space-five">
                                                                </div>
                                                            <asp:Button ID="btn_UpdateEmailAssociation" runat="server" Text="Update" CssClass="input-buttons RandomActionBtns"
                                                                OnClick="btn_UpdateEmailAssociation_Click" Style="margin-left: 22px" />
                                                            <div id="emailassociation_error" style="color: Red; padding-left: 25px">
                                                            </div>
                                                        </td>
                                                        <td class="td-settings-desc">
                                                            <small>Email address that can only be used during sign up. (Email is validated with
                                                            an activation link)</small>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </asp:Panel>
                                            <div class="clear-space">
                                            </div>
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">
                                                        <span class="pad-right font-bold font-color-black">User Sign Up Role</span>
                                                    </td>
                                                    <td class="td-settings-ctrl">
                                                        <asp:DropDownList ID="dd_usersignuprole" runat="server" CssClass="margin-right">
                                                        </asp:DropDownList>
                                                        <asp:Button ID="btn_usersignuprole" runat="server" OnClick="dd_usersignuprole_Changed" CssClass="input-buttons RandomActionBtns" Text="Update" />
                                                    </td>
                                                    <td class="td-settings-desc">
                                                        <small>Select an initial role for the user registering an account. (To create new roles, go to User Accounts and select Manage Roles at the top)</small>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div class="clear-space">
                                            </div>
                                            <a id="btn_customizeua" href="#iframecontent" class="sb-links margin-right float-left"
                                                onclick="openWSE.LoadIFrameContent('SiteTools/UserMaintenance/AcctSettings.aspx?toolview=true&u=NewUserDefaults&NoRegistration=true', this);return false;"
                                                style="display: block;"><span class="img-customize margin-right-sml float-left"></span>New User Customization Settings</a>
                                        </asp:Panel>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnl_Customizations" runat="server" CssClass="pnl-section">
                            <div class="editor_titles">
                                <div class="title-line"></div>
                                <h3>Site Customizations</h3>
                            </div>
                            <div class="float-left pad-top-big">
                                <small>Changes made below will effect the entire website</small>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="pad-all">
                                <asp:Panel ID="pnl_meteTagCustomizations" runat="server">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold font-color-black">Site Description</span>
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_descriptionMetaTage" runat="server" CssClass="textEntry float-left"
                                                    Width="500px" AutoPostBack="False" TextMode="MultiLine" Font-Names='"Arial"'
                                                    BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Height="75px" Style="padding: 4px;"
                                                    ForeColor="#353535"></asp:TextBox>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:Button ID="btn_descriptionMetaTage" runat="server" CssClass="no-margin RandomActionBtns input-buttons"
                                                    Text="Update" OnClick="btn_descriptionMetaTage_Click" />
                                                <div class="clear-space-five"></div>
                                                <asp:LinkButton ID="lbtn_clearDescriptionMeta" runat="server" CssClass="RandomActionBtns sb-links"
                                                    OnClick="lbtn_clearDescriptionMeta_Click" Style="color: #467DB7!important">Clear</asp:LinkButton>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>This will update the description meta tag on the Site.master.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold font-color-black">Site Keywords</span>
                                            </td>
                                            <td>
                                                <asp:Panel ID="pnl_keywordsMetaTag" runat="server" CssClass="keyword-split-array-holder">
                                                </asp:Panel>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:HiddenField ID="hf_keywordsMetaTag" runat="server" ClientIDMode="Static" OnValueChanged="hf_keywordsMetaTag_Changed" />
                                                <input type="button" class="no-margin input-buttons" value="Update" onclick="UpdateSiteKeywords();" />
                                                <div class="clear-space-five"></div>
                                                <asp:LinkButton ID="lbtn_clearAllKeywordsMeta" runat="server" CssClass="RandomActionBtns sb-links"
                                                    OnClick="lbtn_clearAllKeywordsMeta_Click" Style="color: #467DB7!important">Clear</asp:LinkButton>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>This will update the keywords meta tag on the Site.master.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Site Map</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:Button ID="btn_CreateSiteMap" runat="server" Text="Create Sitemap File" OnClick="btn_CreateSiteMap_Click"
                                                    CssClass="RandomActionBtns input-buttons" Width="135px" />
                                                <asp:Panel ID="pnl_viewDeleteSiteMap" runat="server" Enabled="false" Visible="false">
                                                    <div class="clear-space-five"></div>
                                                    <asp:HyperLink ID="hyp_viewSiteMap" runat="server" CssClass="margin-right" Text="View" Target="_blank"></asp:HyperLink>
                                                    <asp:LinkButton ID="lbtn_deleteSiteMap" runat="server" OnClick="lbtn_deleteSiteMap_Click" CssClass="RandomActionBtns" Text="Delete"></asp:LinkButton>
                                                </asp:Panel>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Creates the sitemap.xml used for search engines. Create this before creating your Robots file.</small><div class="clear-space-two"></div>
                                                <small><span class="font-bold">Last Modified:</span>
                                                    <asp:Label ID="lbl_siteMapModified" runat="server" Text="" CssClass="pad-left-sml"></asp:Label></small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Robots File</span>
                                            </td>
                                            <td class="td-settings-ctrl">
                                                <asp:Button ID="btn_CreateRobotsFile" runat="server" Text="Create Robots File" OnClick="btn_CreateRobotsFile_Click"
                                                    CssClass="RandomActionBtns input-buttons" Width="135px" />
                                                <asp:Panel ID="pnl_viewDeleteRobotTxt" runat="server" Enabled="false" Visible="false">
                                                    <div class="clear-space-five"></div>
                                                    <asp:HyperLink ID="hyp_viewRobotsTxt" runat="server" CssClass="margin-right" Text="View" Target="_blank"></asp:HyperLink>
                                                    <asp:LinkButton ID="lbtn_deleteRobotsTxt" runat="server" OnClick="lbtn_deleteRobotsTxt_Click" CssClass="RandomActionBtns" Text="Delete"></asp:LinkButton>
                                                </asp:Panel>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Creates the robots.txt used for search engines.</small><div class="clear-space-two"></div>
                                                <small><span class="font-bold">Last Modified:</span>
                                                    <asp:Label ID="lbl_robotsLastModified" runat="server" Text="" CssClass="pad-left-sml"></asp:Label></small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                </asp:Panel>
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="td-settings-title">
                                            <span class="pad-right font-bold">Background Photos</span>
                                        </td>
                                        <td align="left" valign="top" style="width: 459px">
                                            <asp:FileUpload ID="FileUpload5" runat="server" Width="252px" />
                                            <asp:Button ID="btn_uploadbgImage" runat="server" CssClass="input-buttons margin-left"
                                                Text="Upload" OnClick="btn_uploadbgImage_Click" disabled="disabled" />
                                            <div class="clear">
                                            </div>
                                            <div id="fu_error_message_3" style="color: Red">
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <asp:DropDownList ID="dd_bgmanage" runat="server" AutoPostBack="true" CssClass="margin-right-big"
                                                OnSelectedIndexChanged="dd_bgmanage_change">
                                            </asp:DropDownList>
                                            <asp:LinkButton ID="lbtn_bgmanage" runat="server" CssClass="sb-links margin-right" Enabled="false"
                                                Visible="false" OnClick="lbtn_bgmanage_click">Delete</asp:LinkButton>
                                            <asp:LinkButton ID="lbtn_bgmanage_SetasDefault" runat="server" CssClass="sb-links RandomActionBtns" Enabled="false"
                                                Visible="false" OnClick="lbtn_bgmanage_SetasDefault_click">Set as Default</asp:LinkButton>
                                            <div class="clear">
                                            </div>
                                            <asp:Image ID="img_previewbg" runat="server" CssClass="margin-right margin-top boxshadow-dark rounded-corners-2" Enabled="false" Visible="false" Style="max-height: 200px;" />
                                        </td>
                                        <td class="td-settings-desc">
                                            <small>Upload new photos to the background list on the workspace. (.png, .jpg, .jpeg, .gif ONLY)</small>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space">
                                </div>
                                <asp:Panel ID="pnl_ImageCustomizations" runat="server" Enabled="false" Visible="false">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Main Site Logo</span>
                                            </td>
                                            <td style="width: 255px">
                                                <div class="pad-all-sml inline-block margin-right-big">
                                                    <asp:Label ID="lbl_workspaceLogo" runat="server"></asp:Label>
                                                    <asp:Image ID="img_workspaceLogo" runat="server" Width="175px" />
                                                </div>
                                            </td>
                                            <td align="left" valign="top" style="width: 250px">
                                                <asp:Panel ID="pnl_MainSiteLogoUpload" runat="server" Enabled="false" Visible="false">
                                                    <asp:FileUpload ID="FileUpload2" runat="server" />
                                                    <div class="clear-space">
                                                    </div>
                                                    <asp:Button ID="btn_uploadlogo" runat="server" CssClass="input-buttons" Text="Update"
                                                        OnClick="btn_uploadlogo_Click" disabled="disabled" />
                                                    <div class="clear">
                                                    </div>
                                                    <div id="fu_error_message" style="color: Red">
                                                    </div>
                                                </asp:Panel>
                                            </td>
                                            <td class="td-settings-desc">
                                                <asp:Panel ID="pnl_MainSiteLogoDesc" runat="server" Enabled="false" Visible="false">
                                                    <small>Upload a new logo to display on the workspace. This logo will be centered
                                                    on the page. Uploading new logo will overwrite previous logo. (.png, .jpg, .jpeg, .gif ONLY)</small>
                                                </asp:Panel>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Fav Icon</span>
                                            </td>
                                            <td style="width: 255px">
                                                <div class="pad-all-sml inline-block margin-right-big">
                                                    <asp:Image ID="img_Favicon" runat="server" Style="max-height: 65px" />
                                                </div>
                                            </td>
                                            <td align="left" valign="top" style="width: 250px">
                                                <asp:FileUpload ID="FileUpload4" runat="server" />
                                                <div class="clear-space">
                                                </div>
                                                <asp:Button ID="btn_uploadlogo_fav" runat="server" CssClass="input-buttons" Text="Update"
                                                    OnClick="btn_uploadlogo_fav_Click" disabled="disabled" />
                                                <div class="clear">
                                                </div>
                                                <div id="fu_error_message_2" style="color: Red">
                                                </div>
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Upload a new logo to display on the browser tab. Uploading new logo will overwrite previous favicon. (.png, .jpg, .jpeg, .gif, .ico
                                                    ONLY)</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="td-settings-title">
                                                <span class="pad-right font-bold">Main Logo&nbsp;&nbsp;<br />
                                                    Transparency</span>
                                            </td>
                                            <td style="width: 459px;">
                                                <div class="float-left pad-right pad-top-sml">
                                                    <div id="Slider1" class="ajax__slider_h_rail">
                                                    </div>
                                                </div>
                                                <asp:Button ID="btn_updateLogoOpacity" runat="server" CssClass="RandomActionBtns margin-left input-buttons"
                                                    Text="Update" OnClick="btn_updateLogoOpacity_Click" />
                                                <div>
                                                    <b class="pad-right">Current value:</b><span id="currentLogoOpacity"></span>
                                                </div>
                                                <asp:HiddenField ID="hf_opacity" runat="server" ClientIDMode="Static" />
                                            </td>
                                            <td class="td-settings-desc">
                                                <small>Change the opacity/transparency of the main logo on the workspace.</small>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_loginTheme" runat="server">
                                        <table cellpadding="10" cellspacing="10">
                                            <tr>
                                                <td class="td-settings-title">
                                                    <span class="pad-right font-bold font-color-black">Login Theme</span>
                                                </td>
                                                <td align="left" style="width: 460px;">
                                                    <asp:DropDownList ID="dd_Logintheme" runat="server" CssClass="margin-right-big">
                                                    </asp:DropDownList>
                                                    <asp:Button ID="btn_LoginTheme" runat="server" OnClick="dd_Logintheme_Changed" CssClass="RandomActionBtns input-buttons" Text="Update" />
                                                </td>
                                                <td class="td-settings-desc">
                                                    <small>Change the look of the site login screen.</small>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </asp:Panel>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbtn_testconnection" />
                        <asp:AsyncPostBackTrigger ControlID="rb_allowusersignup_on" />
                        <asp:AsyncPostBackTrigger ControlID="rb_allowusersignup_off" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div id="CustomizationIFrame" style="display: none; margin-left: -60px; margin-right: -40px; margin-top: 4px;">
            </div>
        </div>
        <div class="clear-space">
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/sitesettings.js")%>'></script>
    </div>
</asp:Content>
