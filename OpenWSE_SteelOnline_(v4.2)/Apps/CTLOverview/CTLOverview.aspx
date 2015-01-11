<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CTLOverview.aspx.cs" Inherits="Apps_CTLOverview_CTLOverview" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>CTL Schedule Overview</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager_CTLOverview" runat="server" AsyncPostBackTimeout="360000">
    </asp:ScriptManager>
    <div id="ctloverview-load" class="main-div-app-bg">
        <div class="pad-all app-title-bg-color" style="height: 40px">
            <div class="float-left">
                <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
            </div>
            <div class="float-right pad-top-sml" style="font-size: 15px">
                <div class="float-right">
                    <div id="searchwrapper" style="width: 375px;">
                        <input id="tb_search_ctloverview" type="text" class="searchbox" onfocus="if(this.value=='Search for CTL Logs')this.value=''"
                            onblur="if(this.value=='')this.value='Search for CTL Logs'" onkeypress="KeyPressSearch_CTLOverview(event)"
                            value="Search for CTL Logs" />
                        <a href="#" class="searchbox_clear" onclick="$('#tb_search_ctloverview').val('Search for CTL Logs');RefreshCTLOverview();return false;">
                        </a><a href="#" class="searchbox_submit" onclick="RefreshCTLOverview();return false;">
                        </a>
                    </div>
                </div>
            </div>
        </div>
        <table cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <td valign="top">
                    <div class="stylefour" style="margin-top: 60px;">
                        <div class="float-left pad-top-big margin-top-sml pad-left-big">
                            <div id="ctloverview-topcontrols">
                                <div class="float-left">
                                    Date
                                    <asp:TextBox ID="tb_date_ctloverview" runat="server" CssClass="textEntry margin-left-sml"
                                        Width="100px"></asp:TextBox>
                                </div>
                                <a href="#search" class="margin-left" title="Search Date" onclick="RefreshCTLOverview();return false;">
                                    <img alt="Search Dates" src="search.png" style="padding-right: 0px!important;" /></a>
                            </div>
                            <div id="ctloverview-search-topcontrols" style="display: none;">
                                <div style="padding-top: 5px;">
                                    <div class="float-left font-bold" style='border-right: 1px solid #555; padding-right: 20px;
                                        font-size: 17px;'>
                                        Search Results</div>
                                    <a href='#' onclick="$('#tb_search_ctloverview').val('Search for CTL Logs');RefreshCTLOverview();return false;"
                                        class='margin-left-big td-cancel-btn' style="margin-top: -2px;"></a>
                                </div>
                            </div>
                        </div>
                        <div class="pad-top-big margin-top-sml float-right">
                            <a href="#" class="img-refresh float-right margin-right-big margin-left-big" onclick="RefreshCTLOverview()"
                                title="Refresh List"></a>
                            <select id="font-size-selector-ctloverview" class="float-right margin-left" onchange="FontSelection_CTLOverview()">
                                <option value="x-small">Font Size: x-Small</option>
                                <option value="small" selected="selected">Font Size: Small</option>
                                <option value="medium">Font Size: Medium</option>
                                <option value="large">Font Size: Large</option>
                                <option value="x-large">Font Size: x-Large</option>
                            </select>
                            <div class="clear-space">
                            </div>
                        </div>
                    </div>
                    <div id="ReportViewer_CTLOverview">
                        <div class="clear-margin" style="margin-top: 65px;">
                            <div class="pad-left-big pad-right-big">
                                <div class="float-left">
                                    <asp:UpdatePanel ID="updatepnl_export_ctloverview" runat="server">
                                        <ContentTemplate>
                                            <asp:Panel ID="header_ctloverview" runat="server">
                                                <div class="float-left pad-right-big">
                                                    <span class="font-bold pad-right">Doc Number</span><span id="docNumber_ctllogsheet"
                                                        runat="server">SF-07-05</span>
                                                </div>
                                                <div class="float-left pad-right-big pad-left-big">
                                                    <span class="font-bold pad-right">Revision</span><span id="revision_ctllogsheet"
                                                        runat="server">00</span>
                                                </div>
                                                <div class="float-left pad-right-big pad-left-big">
                                                    <span class="font-bold pad-right">Approved By</span><span id="approvedBy_ctllogsheet"
                                                        runat="server">Not Assigned</span>
                                                </div>
                                            </asp:Panel>
                                            <asp:HiddenField ID="hf_DataUpdate_ctloverview" runat="server" OnValueChanged="hf_DataUpdate_ctloverview_Changed" />
                                            <asp:HiddenField ID="hf_export_ctloverview" runat="server" OnValueChanged="hf_export_ctloverview_Changed" />
                                            <asp:HiddenField ID="hf_refresh_ctloverview" runat="server" OnValueChanged="hf_refresh_ctloverview_Changed" />
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                                <input id="btn_exportToexcel_ctllogsheet" type="button" class="input-buttons float-right no-margin"
                                    title="Exports the current date and the current line" value="Export to Excel"
                                    onclick="ExportToExcel_CTLOverview()" style="margin-top: -5px!important;" />
                                <div class="clear-space">
                                </div>
                                <asp:UpdatePanel ID="updatepnl_ctloverview" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="ctl_logs_holder" runat="server" CssClass="pad-top-big">
                                        </asp:Panel>
                                        <asp:HiddenField ID="hf_search_ctloverview" runat="server" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
