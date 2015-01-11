<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SiteMonitor.aspx.cs" Inherits="Apps_SiteMonitor_SiteMonitor" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Site Monitor</title>
    <script type="text/javascript" src="//www.google.com/jsapi"></script>
    <style type="text/css">
        .averagerequests
        {
            padding: 75px 5px 25px 5px;
        }

            .averagerequests span
            {
                font-size: 17px;
                font-weight: normal;
                color: #555;
            }
    </style>
</head>
<body>
    <form id="formSiteMonitor" runat="server">
        <asp:ScriptManager ID="ScriptManager_SiteMonitor" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="sitemonitor-load" class="main-div-app-bg">
            <div class="pad-all app-title-bg-color" style="height: 40px">
                <div class="float-left">
                    <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                    <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
                </div>
            </div>
            <table class="content-table" cellpadding="0" cellspacing="0">
                <tbody>
                    <tr>
                        <td class="td-sidebar">
                            <div id="sidebar-padding-trucksh" class="sidebar-padding">
                                <div class="sidebar-scroll-app">
                                    <asp:Panel ID="pnl_ect" runat="server" Width="230px">
                                        <div class="clear-margin">
                                            <div class="clear-space-five">
                                            </div>
                                            <h3 class="font-bold">Network Information</h3>
                                            <div class="clear-space">
                                            </div>
                                            <div id="pnl_NetworkInfoHolder">
                                                <h3>Loading Information...</h3>
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </td>
                        <td class="td-content">
                            <div class="content-main" style="min-width: 450px !important;">
                                <table width="100%" cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td class="td-content-inner">
                                                <div class="content-overflow-app">
                                                    <div id="activity" class="pad-all-big">
                                                        <small><b class="pad-right-sml">Note:</b>If charts are not updating every given time
                                                        interval, switch to another browser. (Works best in Google Chrome and Internet Explorer
                                                        8+)</small>
                                                        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                                            <ContentTemplate>
                                                                <div class="clear-space">
                                                                </div>
                                                                <div class="float-left pad-right-big">
                                                                    <span><b>Update Interval:</b></span>
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
                                                                    <span><b>Time to Track:</b></span>
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
                                                                    </asp:DropDownList>
                                                                </div>
                                                                <div class="clear-space">
                                                                </div>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                        <asp:Panel ID="pnl_interactivityholder" runat="server">
                                                            <table width="100%">
                                                                <tbody>
                                                                    <tr>
                                                                        <td valign="top">
                                                                            <div id="ChartRequests">
                                                                                <h3>Loading Charts. Please Wait...</h3>
                                                                            </div>
                                                                        </td>
                                                                        <td valign="top" style="width: 200px;">
                                                                            <div id="statholder">
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </asp:Panel>
                                                        <div class="clear" style="height: 20px">
                                                        </div>
                                                        <table width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div id="ChartGraph3">
                                                                        </div>
                                                                    </td>
                                                                    <td valign="top" style="width: 200px;">
                                                                        <div id="statholder-chart3">
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                        <div class="clear" style="height: 20px">
                                                        </div>
                                                        <table width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div id="ChartGraph2">
                                                                        </div>
                                                                    </td>
                                                                    <td valign="top" style="width: 200px;">
                                                                        <div id="statholder-chart2">
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </form>
</body>
</html>
