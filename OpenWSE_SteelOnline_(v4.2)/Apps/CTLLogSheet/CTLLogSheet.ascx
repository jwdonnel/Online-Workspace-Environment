<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CTLLogSheet.ascx.cs" Inherits="Apps_CTLLogSheet"
    ClientIDMode="Static" %>
<div id="ctllogsheet-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color" style="height: 40px">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <div class="float-right pad-top-sml" style="font-size: 15px">
        </div>
    </div>
    <div id="ReportViewer_ctllogsheet">
        <table cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <td valign="top">
                    <div class="stylefour" style="margin-top: 60px;">
                        <ul class="ctlLineLinks float-left margin-right">
                            <li id="ctlLine1-li-ctllogsheet" class="active"><a href="#Line1" onclick="return false;">
                                Line 1</a></li>
                            <li id="ctlLine2-li-ctllogsheet"><a href="#Line2" onclick="return false;">Line 2</a></li>
                        </ul>
                        <div class="float-left pad-top-big margin-top-sml pad-left-big">
                            <div class="float-left">
                                Date
                                <input type="text" id="tb_date_ctllogsheet" class="textEntry margin-left-sml" style="width: 100px;" /></div>
                            <a href="#search" class="margin-left" title="Search Date" onclick="DateChange();return false;">
                                <img alt="Search Dates" src="Apps/CTLReport/search.png" style="padding-right: 0px!important;" /></a>
                        </div>
                        <div class="pad-top-big margin-top-sml float-right">
                            <a href="#" class="img-refresh float-right margin-right-big margin-left-big" onclick="LoadCTLLogs()" title="Refresh List"></a>
                            <select id="font-size-selector-ctllogsheet" class="float-right margin-left" onchange="FontSelection_CTLLogs()">
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
                    <div class="clear-margin" style="margin-top: 65px;">
                        <div class="pad-left-big pad-right-big">
                            <div class="float-left">
                                <span class="pad-right-sml font-bold">Note:</span>The name on this report identifies
                                the individual performing the required inspections for these items, which serves
                                as the record of inspection.
                            </div>
                            <div class="float-right">
                                <a id="print-schedule-ctllogsheet" href="Apps/CTLLogSheet/CTLLogPrint.aspx" target="_blank"
                                    class="sb-links" title="Print current date">
                                    <span class="img-printer float-left margin-right-sml"></span>Print
                                    Log</a></div>
                            <div class="clear-space">
                            </div>
                            <div class="float-left pad-right-big">
                                <span class="font-bold pad-right">Doc Number</span><span id="docNumber_ctllogsheet">SF-07-05</span>
                            </div>
                            <div class="float-left pad-right-big pad-left-big">
                                <span class="font-bold pad-right">Revision</span><span id="revision_ctllogsheet">00</span>
                            </div>
                            <div class="float-left pad-right-big pad-left-big">
                                <span class="font-bold pad-right">Approved By</span><span id="approvedBy_ctllogsheet">Not
                                    Assigned</span>
                            </div>
                            <input id="btn_exportToexcel_ctllogsheet" type="button" class="input-buttons float-right no-margin"
                                title="Exports the current date and the current line" value="Export to Excel"
                                onclick="ExportToExcel_CTLLogs()" />
                            <div class="clear-space-five">
                            </div>
                            <div class="float-left pad-right-big">
                                <span class="font-bold pad-right">Employee</span><input id="tb_employee_ctllogsheets"
                                    type="text" class="textEntry" value="Employee Name" onfocus="if(this.value=='Employee Name')this.value=''"
                                    onblur="if(this.value=='')this.value='Employee Name'" style="width: 125px;" />
                            </div>
                            <div class="float-left pad-right-big pad-left-big">
                                <span class="font-bold pad-right">Shift</span>
                                <select id="ddl_shift_ctllogsheets">
                                    <option value="1">One</option>
                                    <option value="2">Two</option>
                                    <option value="3">Three</option>
                                </select>
                            </div>
                            <input id="btn_updateheader_ctllogsheets" type="button" class="input-buttons margin-left"
                                title="Update Employee name and Shift for current date and line number." value="Update"
                                onclick="UpdateHeader_CTLLogs()" />
                            <div class="clear-space-five">
                            </div>
                            <div id="ctl-logs-holder" class="pad-top-big">
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div id="errorPullingRecords-ctllogsheet" style="color: Red">
    </div>
</div>
