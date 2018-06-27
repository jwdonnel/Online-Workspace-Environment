<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SteelTrucks.ascx.cs" Inherits="Apps_SteelTrucks_SteelTrucks"
    ClientIDMode="Static" %>

<div id="pnl_steelTrucks_tableView">
    <div class="pad-all app-title-bg-color">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
            <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
        </div>
        <div class="float-right">
            <div class="searchwrapper">
                <input id="tb_search_steeltrucks" type="text" class="searchbox" placeholder="Search Schedule..." onkeypress="steelTruckFunctions.KeyPressSearch(event)" />
                <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_steeltrucks').val('');steelTruckFunctions.RefreshScheduleList();return false;"></a><a href="#" class="searchbox_submit" onclick="steelTruckFunctions.SearchTruckSchedule();return false;"></a>
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <asp:HiddenField ID="hf_AutoCompleteList_SteelTrucks" runat="server" />
    <h2 id="CurrDriverName-Holder" class="float-left margin-top pad-top-big pad-bottom-big pad-left"></h2>
    <a href="#" id="btn_refresh_steeltrucks" class="img-refresh" onclick="steelTruckFunctions.Refresh();return false;"><span>Refresh</span></a>
    <div class="clear"></div>
    <div id="errorPullingRecords" style="color: Red">
    </div>
    <div id="driver-sch-holder" class="pad-left pad-right">Loading...</div>
</div>
<div id="steelTrucks-sidebar-menu">
    <div class="pad-all" style="padding-top: 0!important;">
        <ul>
            <li>
                <div class="section-pad" style="text-align: center;">
                    <a class="sidebar-menu-buttons" onclick="steelTruckFunctions.LoadCreateNew(''); return false;"><span class="td-add-btn"></span>Create New</a>
                    <div class="clear-space-five"></div>
                    <a class="sidebar-menu-buttons" onclick="steelTruckFunctions.LoadGenDirEditor(); return false;"><span class="td-edit-btn"></span>General Directions</a>
                    <div class="clear"></div>
                </div>
            </li>
            <li>
                <div class="section-pad">
                    <h3 class="section-pad-title" style="padding-top: 3px !important;">Records to Pull</h3>
                    <select id="RecordstoSelect_SteelTrucks" class="float-right" onchange="steelTruckFunctions.RecordstoSelect()">
                        <option value="5">5</option>
                        <option value="10">10</option>
                        <option value="25">25</option>
                        <option value="50">50</option>
                        <option value="75">75</option>
                        <option value="100">100</option>
                        <option value="200" selected="selected">200</option>
                        <option value="300">300</option>
                        <option value="400">400</option>
                        <option value="500">500</option>
                    </select>
                    <div class="clear"></div>
                </div>
            </li>
            <li style="display: none;">
                <div class="section-pad">
                    <h3>Export to Spreadsheet</h3>
                    <div class="clear-space"></div>
                    <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td>
                                <div class="pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                                    From
                                </div>
                                <input id="tb_exportDateFrom" type="text" class="textEntry" style="width: 85px;" />
                                <div class="clear-space-five">
                                </div>
                                <div class="pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                                    To
                                </div>
                                <input id="tb_exportDateTo" type="text" class="textEntry" style="width: 85px;" />
                            </td>
                            <td>
                                <input id="btn_exportAll" type="button" class="input-buttons no-margin" onclick="steelTruckFunctions.ExportToExcelAll_steeltrucks()"
                                    title="Select a date to export to an Excel file" value="Export" />
                                <span id="exportingNow" class="margin-left" style="display: none;">Exporting...</span>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space-five">
                    </div>
                    <div class="pad-right font-bold float-left" style="width: 42px; padding-top: 4px;">
                        Driver
                    </div>
                    <select id="driverListOptions-steeltrucks" style="width: 168px;">
                    </select>
                </div>
            </li>
            <li>
                <div class="section-pad">
                    <h3>Drivers</h3>
                    <div class="clear-space-five"></div>
                    <div id="driver-index-holder"></div>
                </div>
            </li>
        </ul>
    </div>
</div>
<div id="DirEdit-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="520">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="steelTruckFunctions.LoadGenDirEditor();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="GenDirList-steeltrucks">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="CreateTruckSch-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="655">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="steelTruckFunctions.LoadCreateNew('');return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
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
                        <div class="clear-space"></div>
                        <div id="createNewError" style="color: Red"></div>
                        <div class="clear-space"></div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input type="button" class="input-buttons modal-cancel-btn" onclick="steelTruckFunctions.LoadCreateNew(''); return false;"
                        value="Close" />
                    <input type="button" class="input-buttons modal-ok-btn" onclick="steelTruckFunctions.CallCreateNew(); return false;"
                        value="Create" />
                </div>
            </div>
        </div>
    </div>
</div>
<div id="TruckEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="1010">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="steelTruckFunctions.LoadEventEditor();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <a id="printBtn-steeltrucks--hidden" target="_blank" onclick="steelTruckFunctions.printClickEvent(this);" style="visibility: hidden;"></a>
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
                        <div id="steeltrucks_editbtnHolder" class="float-right">
                            <input id="btn_exportToexcel_steeltrucks" type="button" class="input-buttons float-right margin-left"
                                onclick="steelTruckFunctions.ExportToExcel_steeltrucks()" title="Export the current driver and details to an Excel file"
                                value="Export to Excel" style="margin-right: 0px!important; margin-top: -6px;" />
                            <a id="CanceleditBtn-steeltrucks" href="#cancel" class="float-right margin-right margin-left"
                                onclick="steelTruckFunctions.CancelEditHeaderEvent();return false;" style="display: none;">
                                <span class="td-cancel-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Cancel</a>
                            <a id="editBtn-steeltrucks" href="#edit" class="float-right margin-right margin-left"
                                onclick="steelTruckFunctions.EditHeaderEvent();return false;">
                                <span class="td-edit-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Edit</a>
                            <a id="printBtn-steeltrucks" class="float-right margin-right margin-left">
                                <span class="img-printer float-left margin-right-sml"></span>Print</a>
                            <a id="deleteEditBtn-steeltrucks" href="#delete" class="float-right margin-right margin-left"
                                onclick="steelTruckFunctions.DeleteHeaderEvent();return false;">
                                <span class="td-delete-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Delete</a>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div id="EventList-steeltrucks" style="max-height: 500px; overflow: auto;">
                        </div>
                        <div id="schedulenumber-holder_steeltrucks"></div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input id="btn-save-eventeditor" type="button" value="Save" class="input-buttons modal-ok-btn" />
                    <input type="button" value="Cancel" onclick="steelTruckFunctions.LoadEventEditor();" class="input-buttons modal-cancel-btn" />
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/SteelTrucks/steeltrucks.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Scripts/jquery/jquery.fileDownload.js" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/SteelTrucks/steeltrucks.js" />
