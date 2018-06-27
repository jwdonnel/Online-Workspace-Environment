<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkCalendar.ascx.cs"
    Inherits="Apps_WorkCalendar_WorkCalendar" ClientIDMode="Static" %>
<div id="wCal-titlebar" class="pad-all app-title-bg-color">
    <div class="float-left">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
        <div class="clear"></div>
    </div>
    <div class="pad-left pad-right pad-top">
        <div class="searchwrapper float-right">
            <input id="tb_search_wcal" type="text" class="searchbox" onfocus="if(this.value=='Search Current Month')this.value=''"
                onblur="if(this.value=='')this.value='Search Current Month'" onkeypress="KeyPressSearch_wc(event)"
                value="Search Current Month" style="width: 155px;" />
            <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_wcal').val('Search Current Month');onCalendarLoad_wc();return false;"></a><a href="#" class="searchbox_submit" onclick="onCalendarLoad_wc();return false;"></a>
        </div>
    </div>
    <div class="clear"></div>
</div>
<input id="dateselected_wc" type="hidden" value="" />
<input id="hidden_date_wc" name="sdate" type="hidden" value="" />
<div class="pc-menu-bar">
    <a href="#" class="menu-btns" onclick="AddNewCalendarEvent_wc('', false);return false;">Add Event</a>
    <a id="lnk-printCalendar-workcal" href="#" class="menu-btns" target="_blank" style="display: none;">Print</a>
    <a id="menubtn_employees" runat="server" href="#" class="menu-btns"
        onclick="LoadTypeEdit_wc('1');return false;">Employees</a> <a id="menubtn_reasons"
            runat="server" href="#" class="menu-btns" onclick="LoadTypeEdit_wc('2');return false;">Reasons</a> <a id="menubtn_requests" runat="server" href="#" class="menu-btns" onclick="LoadRequestApprovalModal();return false;">Requests</a><a id="printbtn" href="#" target="_blank" class="menu-btns">
                            Print</a><a href="#" class="menu-btns" onclick="HideTitleSearch_wc(this);return false;">
                            Hide Header</a><span class="font-bold pad-right-sml pad-left-big">Calendar View:</span>
    <select id="overview-view-wc">
        <option value="calendar">Calendar</option>
        <option value="overview">Overview</option>
    </select>
    <span class="font-bold pad-right-sml pad-left-big">Group:</span>
    <select id="group_select_wc" runat="server">
    </select>
    <div id="currmonthBtns_wc" class="currmonthBtns">
    </div>
</div>
<div id="workcalendar-printview">
    <div id="print-view-date" style="display: none;"></div>
    <div id="usercalendar_wc" style="position: relative;">
    </div>
</div>
<div id="useroverview_wc" style="position: relative; display: none;">
    <table style="width: 100%">
        <tbody>
            <tr>
                <td valign="top" style="border-right: 1px solid #DDD; width: 250px;">
                    <div id="sidebar-caloverview-wc" class="pad-all">
                    </div>
                </td>
                <td valign="top">
                    <div class="pad-all-big">
                        <div class="calendar-overview">
                            <h2 class="font-bold">Happening Now</h2>
                            <div class="clear-space">
                            </div>
                            <div id="pnl_offtoday_wc">
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="calendar-overview">
                            <h2 class="font-bold">Tomorrow's Events</h2>
                            <div class="clear-space">
                            </div>
                            <div id="pnl_offtomorrow_wc">
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="calendar-overview">
                            <h2 class="font-bold">Recently Cancelled</h2>
                            <div class="clear-space">
                            </div>
                            <div id="pnl_cancelled_wc">
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="calendar-overview">
                            <h2 class="font-bold">Recently Rejected</h2>
                            <div class="clear-space">
                            </div>
                            <div id="pnl_recentrejected_wc">
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</div>
<input type="hidden" id="hf_editmode-wc" />
<div id="ViewCalEvent-element_wc" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="510">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="ViewNewCalendarEvent_wc('');return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="eventholderCal_modal">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="NewCalEvent-element_wc" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="510">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="AddNewCalendarEvent_wc('', false);return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                            Employee
                        </div>
                        <select id="dd_employees_wc">
                        </select>
                        <div class="clear-space-five">
                        </div>
                        <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                            Reason
                        </div>
                        <select id="dd_reason_wc">
                        </select>
                        <div class="clear-space-five">
                        </div>
                        <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                            Description
                        </div>
                        <textarea id="tb_newevent_desc_wc" class="textEntry" rows="5" cols="80" style="width: 335px; font-family: Arial; border: 1px solid #CCC;"></textarea>
                        <div class="clear-space-five">
                        </div>
                        <div class="float-left pad-right-big">
                            <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                                Start Date
                            </div>
                            <input id="tb_newevent_start_wc" type="text" class="textEntry margin-right" style="width: 85px; border: 1px solid #CCC;" />
                            <select id="dd_newevent_starttime_wc" class="margin-right-sml">
                                <option value="12:00">12:00</option>
                                <option value="12:15">12:15</option>
                                <option value="12:30">12:30</option>
                                <option value="12:45">12:45</option>
                                <option value="1:00">1:00</option>
                                <option value="1:15">1:15</option>
                                <option value="1:30">1:30</option>
                                <option value="1:45">1:45</option>
                                <option value="2:00">2:00</option>
                                <option value="2:15">2:15</option>
                                <option value="2:30">2:30</option>
                                <option value="2:45">2:45</option>
                                <option value="3:00">3:00</option>
                                <option value="3:15">3:15</option>
                                <option value="3:30">3:30</option>
                                <option value="3:45">3:45</option>
                                <option value="4:00">4:00</option>
                                <option value="4:15">4:15</option>
                                <option value="4:30">4:30</option>
                                <option value="4:45">4:45</option>
                                <option value="5:00">5:00</option>
                                <option value="5:15">5:15</option>
                                <option value="5:30">5:30</option>
                                <option value="5:45">5:45</option>
                                <option value="6:00">6:00</option>
                                <option value="6:15">6:15</option>
                                <option value="6:30">6:30</option>
                                <option value="6:45">6:45</option>
                                <option value="7:00">7:00</option>
                                <option value="7:15">7:15</option>
                                <option value="7:30">7:30</option>
                                <option value="7:45">7:45</option>
                                <option value="8:00">8:00</option>
                                <option value="8:15">8:15</option>
                                <option value="8:30">8:30</option>
                                <option value="8:45">8:45</option>
                                <option value="9:00">9:00</option>
                                <option value="9:15">9:15</option>
                                <option value="9:30">9:30</option>
                                <option value="9:45">9:45</option>
                                <option value="10:00">10:00</option>
                                <option value="10:15">10:15</option>
                                <option value="10:30">10:30</option>
                                <option value="10:45">10:45</option>
                                <option value="11:00">11:00</option>
                                <option value="11:15">11:15</option>
                                <option value="11:30">11:30</option>
                                <option value="11:45">11:45</option>
                            </select>
                            <select id="dd_newevent_starttime_wc_ampm" class="margin-right">
                                <option selected="selected" value="AM">AM</option>
                                <option value="PM">PM</option>
                            </select>
                            <div class="clear-space-five">
                            </div>
                            <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                                End Date
                            </div>
                            <input id="tb_newevent_end_wc" type="text" class="textEntry margin-right" style="width: 85px; border: 1px solid #CCC;" />
                            <select id="dd_newevent_endtime_wc" class="margin-right-sml">
                                <option value="12:00">12:00</option>
                                <option value="12:15">12:15</option>
                                <option value="12:30">12:30</option>
                                <option value="12:45">12:45</option>
                                <option value="1:00">1:00</option>
                                <option value="1:15">1:15</option>
                                <option value="1:30">1:30</option>
                                <option value="1:45">1:45</option>
                                <option value="2:00">2:00</option>
                                <option value="2:15">2:15</option>
                                <option value="2:30">2:30</option>
                                <option value="2:45">2:45</option>
                                <option value="3:00">3:00</option>
                                <option value="3:15">3:15</option>
                                <option value="3:30">3:30</option>
                                <option value="3:45">3:45</option>
                                <option value="4:00">4:00</option>
                                <option value="4:15">4:15</option>
                                <option value="4:30">4:30</option>
                                <option value="4:45">4:45</option>
                                <option value="5:00">5:00</option>
                                <option value="5:15">5:15</option>
                                <option value="5:30">5:30</option>
                                <option value="5:45">5:45</option>
                                <option value="6:00">6:00</option>
                                <option value="6:15">6:15</option>
                                <option value="6:30">6:30</option>
                                <option value="6:45">6:45</option>
                                <option value="7:00">7:00</option>
                                <option value="7:15">7:15</option>
                                <option value="7:30">7:30</option>
                                <option value="7:45">7:45</option>
                                <option value="8:00">8:00</option>
                                <option value="8:15">8:15</option>
                                <option value="8:30">8:30</option>
                                <option value="8:45">8:45</option>
                                <option value="9:00">9:00</option>
                                <option value="9:15">9:15</option>
                                <option value="9:30">9:30</option>
                                <option value="9:45">9:45</option>
                                <option value="10:00">10:00</option>
                                <option value="10:15">10:15</option>
                                <option value="10:30">10:30</option>
                                <option value="10:45">10:45</option>
                                <option value="11:00">11:00</option>
                                <option value="11:15">11:15</option>
                                <option value="11:30">11:30</option>
                                <option value="11:45">11:45</option>
                            </select>
                            <select id="dd_newevent_endtime_wc_ampm" class="margin-right">
                                <option value="AM">AM</option>
                                <option selected="selected" value="PM">PM</option>
                            </select>
                        </div>
                        <div class="float-left">
                            <small><b class="font-color-light-black">Hour(s)</b></small><br />
                            <input type="text" value="8" id="tb_hours" disabled="disabled" class="aspNetDisabled textEntry"
                                style="width: 50px;" />
                        </div>
                        <div class="clear-space">
                        </div>
                        <input id="btn_createnewevent_wc" type="button" class="input-buttons float-right no-margin"
                            onclick="btn_createnewevent_wc_Clicked()" value="Create" />
                        <input type="button" class="input-buttons float-left" onclick="AddNewCalendarEvent_wc('', false)"
                            value="Cancel" />
                        <div id="errormessagewc" style="color: Red; height: 15px;">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="TypeEdit-element_wc" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="400">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="LoadTypeEdit_wc('0');return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="Typelist_wc">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div id="RequestApproval-element_wc" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="400">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="LoadRequestApprovalModal();return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="pendingapprovallist_wc">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/WorkCalendar/workcalendar.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/WorkCalendar/workcalendar.js" />
