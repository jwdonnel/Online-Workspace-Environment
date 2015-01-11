<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SteelTrucks.ascx.cs" Inherits="Apps_SteelTrucks_SteelTrucks"
    ClientIDMode="Static" %>
<div id="steeltrucks-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color" style="height: 40px">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <div class="float-right pad-top-sml" style="font-size: 15px">
            <div class="float-right">
                <div id="searchwrapper" style="width: 375px;">
                    <input id="tb_search_steeltrucks" type="text" class="searchbox" onfocus="if(this.value=='Search for Truck Schedule')this.value=''"
                        onblur="if(this.value=='')this.value='Search for Truck Schedule'" onkeypress="KeyPressSearch_SteelTrucks(event)"
                        value="Search for Truck Schedule" />
                    <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_steeltrucks').val('Search for Truck Schedule');RefreshScheduleList();return false;"></a><a href="#" class="searchbox_submit" onclick="SearchTruckSchedule();return false;"></a>
                </div>
            </div>
        </div>
    </div>
    <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
            <td valign="top" class="td-sidebar pad-right">
                <div id="sidebar-items-steeltrucks" class="sidebar-items-fixed">
                    <div class='pad-all' style="border-bottom: 1px solid #CCC;">
                        <h3>Export to Spreadsheet</h3>
                        <div class="clear-space">
                        </div>
                        <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <div class="pad-left pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                                        From
                                    </div>
                                    <input id="tb_exportDateFrom" type="text" class="textEntry" style="width: 85px;" />
                                    <div class="clear-space-five">
                                    </div>
                                    <div class="pad-left pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                                        To
                                    </div>
                                    <input id="tb_exportDateTo" type="text" class="textEntry" style="width: 85px;" />
                                </td>
                                <td>
                                    <input id="btn_exportAll" type="button" class="input-buttons no-margin" onclick="ExportToExcelAll_steeltrucks()"
                                        title="Select a date to export to an Excel file" value="Export" />
                                    <span id="exportingNow" class="margin-left" style="display: none;">Exporting...</span>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space-five">
                        </div>
                        <div class="pad-left pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                            Driver
                        </div>
                        <select id="driverListOptions-steeltrucks" style="width: 168px;">
                        </select>
                        <div class='clear-space-five'>
                        </div>
                    </div>
                    <div class='clear-space-five'>
                    </div>
                    <div id="general-dir-holder" class="pad-all">
                        <input type='button' onclick='LoadGenDirEditor();' class='input-buttons float-left'
                            value='Direction Edit' />
                        <input id="createnewts" type="button" class="input-buttons float-right no-margin"
                            onclick="LoadCreateNew('');" title="Create New Schedule" value="Create New" />
                    </div>
                    <div id="driver-index-holder" class="pad-all">
                    </div>
                </div>
            </td>
            <td valign="top">
                <div class="pad-all">
                    <div class="pad-top">
                        <div class="float-left pad-left">
                            <h2 id="CurrDriverName-Holder"></h2>
                            <div id="errorPullingRecords" style="color: Red">
                            </div>
                        </div>
                        <a class="float-right margin-right margin-left img-refresh cursor-pointer" onclick="RefreshScheduleList();return false;" title="Refresh List"></a>
                        <select id="font-size-selector-steeltrucks" class="float-right margin-left" onchange="FontSelection_SteelTrucks()">
                            <option value="x-small">Font Size: x-Small</option>
                            <option value="small" selected="selected">Font Size: Small</option>
                            <option value="medium">Font Size: Medium</option>
                            <option value="large">Font Size: Large</option>
                            <option value="x-large">Font Size: x-Large</option>
                        </select>
                        <div class="float-right margin-right">
                            <span class="font-bold pad-right">Records to Pull</span>
                            <select id="RecordstoSelect" onchange="RecordstoSelect_SteelTrucks()">
                                <option value="5">5</option>
                                <option value="10">10</option>
                                <option value="25">25</option>
                                <option value="50" selected="selected">50</option>
                                <option value="75">75</option>
                                <option value="100">100</option>
                                <option value="200">200</option>
                                <option value="all">All</option>
                            </select>
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
                <div class="clear-margin">
                    <div class="pad-left-big">
                        <div id="driver-sch-holder">
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</div>
<div id="DirEdit-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="LoadGenDirEditor();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    <div id="GenDirList-steeltrucks" style="max-height: 500px; overflow: auto;">
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="TruckEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="max-width: 1010px;">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="LoadEventEditor();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    <div class="float-left">
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <span class="truckscheduleheader">Driver</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Date</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Unit</span>
                                </td>
                                <td>
                                    <span class="truckscheduleheader">Direction</span>
                                </td>
                            </tr>
                            <tr>
                                <td class="pad-right-big margin-right-big">
                                    <div id="eventEdit-Driver" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="eventEdit-Date" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="eventEdit-Unit" style="padding: 5px;">
                                    </div>
                                </td>
                                <td class="pad-right-big margin-right-big">
                                    <div id="eventEdit-Dir" style="padding: 5px;">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="float-right">
                        <input id="btn_exportToexcel_steeltrucks" type="button" class="input-buttons float-right margin-left"
                            onclick="ExportToExcel_steeltrucks()" title="Export the current driver and details to an Excel file"
                            value="Export to Excel" style="margin-right: 0px!important;" />
                        <a id="CanceleditBtn-steeltrucks" href="#cancel" class="sb-links float-right margin-right margin-left"
                            onclick="CancelEditHeaderEvent();return false;" style="display: none;">
                            <span class="td-cancel-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Cancel</a>
                        <a id="editBtn-steeltrucks" href="#edit" class="sb-links float-right margin-right margin-left"
                            onclick="EditHeaderEvent();return false;">
                            <span class="td-edit-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Edit</a>
                        <a id="printBtn-steeltrucks" class="sb-links float-right margin-right margin-left"
                            target="_blank">
                            <span class="img-printer float-left margin-right-sml"></span>Print</a>
                        <a id="deleteEditBtn-steeltrucks" href="#delete" class="sb-links float-right margin-right margin-left"
                            onclick="DeleteHeaderEvent();return false;">
                            <span class="td-delete-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Delete</a>
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
<div id="CreateTruckSch-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="LoadCreateNew('');return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    Use the form below to create a new driver schedule. You may also choose an existing
                    driver to add a new schedule for. To access the list of current drivers, click inside
                    the textboxes to display the dropdown.
                    <div class="clear-space">
                    </div>
                    <table style="width: 100%;" cellpadding="5" cellspacing="5">
                        <tbody>
                            <tr>
                                <td>
                                    <strong>Driver Name</strong>
                                </td>
                                <td>
                                    <strong>Date</strong>
                                </td>
                                <td>
                                    <strong>General Direction</strong>
                                </td>
                                <td>
                                    <strong>Unit</strong>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <input id="tb_drivername_steeltrucks" type="text" class="textEntry" style="width: 125px" />
                                </td>
                                <td>
                                    <input id="tb_date_steeltrucks" type="text" class="textEntry float-left" style="width: 125px" />
                                </td>
                                <td>
                                    <select id="dd_generaldirection_steeltrucks" style="width: 140px">
                                    </select>
                                </td>
                                <td>
                                    <input id="tb_unit_steeltrucks" type="text" class="textEntry float-left" style="width: 125px" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <div class="clear-space">
                    </div>
                    <div style="height: 35px">
                        <div id="createNewError" class="float-left" style="color: Red">
                        </div>
                        <input type="button" class="input-buttons float-right" onclick="LoadCreateNew(''); return false;"
                            value="Close" />
                        <input type="button" class="input-buttons float-right" onclick="CallCreateNew(); return false;"
                            value="Create" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
