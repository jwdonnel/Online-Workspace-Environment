<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CTLReports.ascx.cs" Inherits="Apps_CTLReport_CTLReports"
    ClientIDMode="Static" %>
<div id="ctlrport-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color" style="height: 40px">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <div class="float-right pad-top-sml" style="font-size: 15px">
            <div class="float-right">
                <div id="searchwrapper" style="width: 375px;">
                    <input id="tb_search_ctlreport" type="text" class="searchbox" onfocus="if(this.value=='Search for CTL Report')this.value=''"
                        onblur="if(this.value=='')this.value='Search for CTL Report'" onkeypress="KeyPressSearch_CTLReport(event)"
                        value="Search for CTL Report" />
                    <a href="#" class="searchbox_clear" onclick="$('#tb_search_ctlreport').val('Search for CTL Report');return false;"></a>
                    <a href="#" class="searchbox_submit" onclick="SearchTruckCTLRecords();return false;"></a>
                </div>
            </div>
        </div>
    </div>
    <div id="ReportViewer_CTLLines">
        <table cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <td valign="top">
                    <div class="stylefour" style="margin-top: 60px;">
                        <div id="ctlreport-tab-topcontrols">
                            <div class="float-left pad-top-big margin-top-sml pad-left-big">
                                <div class="float-left">
                                    Ticket Date
                                    <input type="text" id="tb_date_ctlreport" class="textEntry margin-left-sml" style="width: 100px;" /></div>
                                <a href="#search" class="margin-left" title="Search Date" onclick="$('#backbtn-searchresults').hide();LoadCTLReports();return false;">
                                    <img alt="Search Dates" src="Apps/CTLReport/search.png" style="padding-right: 0px!important;" /></a>
                            </div>
                        </div>
                        <div id="ctlreport-tab-search-topcontrols" class="float-left" style="display: none;">
                            <div class="pad-left" style="padding-top: 18px;">
                                <div class="float-left font-bold" style='border-right: 1px solid #555; padding-right: 20px;
                                    font-size: 17px;'>
                                    Search Results</div>
                                <a href='#' onclick='CloseReport();return false;' class='td-cancel-btn margin-left-big'
                                    style="margin-top: -2px;"></a>
                            </div>
                        </div>
                        <div class="pad-top-big margin-top-sml float-right">
                                <a href="#" class="img-refresh float-right margin-right-big margin-left-big" onclick="RefreshCTLReport()"
                                title="Refresh List"></a>
                            <select id="font-size-selector-ctlreport" class="float-right margin-left" onchange="FontSelection_CTLReport()">
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
                    <div id="ctlreport-tab">
                        <div class="clear-margin" style="margin-top: 65px;">
                            <div class="pad-left-big pad-right-big">
                                <div class="float-left">
                                    <span class="pad-right-sml font-bold">Note:</span>To view the coil ticket, click on the
                                    line you want to view.
                                </div>
                                <input id="btn_exportToexcelAll_ctlreport" type="button" class="input-buttons float-right no-margin"
                                    title="Exports the current date and all tickets to Excel" value="Export to Excel"
                                    onclick="ExportToExcel_CTLReport(true, '')" />
                                <input id="btn_issueNewTicket_ctlreport" type="button" class="input-buttons float-right"
                                    title="Create a new decoil ticket" value="Create Ticket" onclick="OpenNewModalCTLLine()" />
                                <div class="clear-space-five">
                                </div>
                                <div id="ctl-report-holder" class="pad-top-big">
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="ctlreport-search-tab" style="display: none;">
                        <div class="clear-margin" style="margin-top: 65px;">
                            <div class="pad-left-big pad-right-big">
                                <div class="float-left">
                                    <span class="pad-right-sml font-bold">Note:</span>Search results displays all information
                                    for each entry. These are editable results and can be exported to an Excel Spreadsheet.
                                </div>
                                <div class="clear-space-five">
                                </div>
                                <div id="ctl-report-search-holder" class="pad-top-big">
                                </div>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div id="errorPullingRecords-ctlreport" class="pad-left-big" style="color: Red">
    </div>
</div>
<div id="ctlreport-edit-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="max-width: 1010px;">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'ctlreport-edit-element', '');$('#pnl_innerHtml_ctlreport').html('');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    <div class="float-left">
                        <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <span class="truckscheduleheader">Material</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Coils Cut</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Coil Weight</span>
                                </td>
                            </tr>
                            <tr>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-Material" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-CoilsToCut" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-CoilWeight" style="padding: 5px;">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <span class="truckscheduleheader">Gauge</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Coil Number</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Width</span>
                                </td>
                            </tr>
                            <tr>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-Gauge" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-CoilNumber" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="ctl-eventEdit-Width" style="padding: 5px;">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="ctl-eventEdit-btns" class="float-right">
                        <input id="btn_exportToexcel_steeltrucks" type="button" class="input-buttons float-right margin-left"
                            onclick="ExportToExcel_steeltrucks()" title="Export the current driver and details to an Excel file"
                            value="Export to Excel" style="margin-right: 0px!important;" />
                        <a id="editBtn-steeltrucks" href="#edit" class="sb-links float-right margin-right margin-left"
                            onclick="EditHeaderEvent();return false;">
                            <span class="margin-right-sml float-left td-edit-btn" style="padding: 0px!important;"></span>Edit</a>
                        <a id="CanceleditBtn-steeltrucks" href="#cancel" class="sb-links float-right margin-right margin-left"
                            onclick="CancelEditHeaderEvent();return false;" style="display: none;"><span class="margin-right-sml float-left td-cancel-btn" style="padding: 0px!important;"></span>Cancel</a>
                        <a id="printBtn-steeltrucks" class="sb-links float-right margin-right margin-left"
                            target="_blank">
                            <span class="margin-right-sml float-left img-printer"></span>Print</a>
                        <a id="deleteEditBtn-steeltrucks" href="#delete" class="sb-links float-right margin-right margin-left"
                            onclick="DeleteHeaderEvent();return false;">
                            <span class="margin-right-sml float-left td-delete-btn" style="padding: 0px!important;"></span>Delete</a>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div id="EventList-steeltrucks" style="max-height: 500px; overflow: auto;">
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
