<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalCalendar.ascx.cs"
    Inherits="Apps_PersonalCalendar_PersonalCalendar" ClientIDMode="Static" %>
<div id="usercalendar_pc"></div>
<div id="ViewCalEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="530">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="personalCalendarApp.ViewNewCalendarEvent('');return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div id="eventholderCal_modal">
                        </div>
                        <div class="clear">
                        </div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                </div>
            </div>
        </div>
    </div>
</div>
<div id="NewCalEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="530">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="personalCalendarApp.AddNewCalendarEvent('', false);return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div class="modal-descDivs-item">
                            Title
                        </div>
                        <input id="tb_newevent_title_pc" type="text" class="textEntry" maxlength="250" style="width: 100%;" />
                        <div class="clear-space-five">
                        </div>
                        <div class="modal-descDivs-item">
                            Description
                        </div>
                        <textarea id="tb_newevent_desc_pc" class="textEntry" rows="3" cols="80" style="width: 100%;"></textarea>
                        <div class="clear-space-five">
                        </div>
                        <div class="modal-descDivs-item">
                            Start Date
                        </div>
                        <input id="tb_newevent_start_pc" type="text" class="textEntry margin-right" style="width: 100px;" />
                        <select id="dd_newevent_starttime_pc" class="margin-right-sml">
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
                        <select id="dd_newevent_starttime_pc_ampm" class="margin-right">
                            <option selected="selected" value="AM">AM</option>
                            <option value="PM">PM</option>
                        </select>
                        <div class="clear-space-five">
                        </div>
                        <div class="modal-descDivs-item">
                            End Date
                        </div>
                        <input id="tb_newevent_end_pc" type="text" class="textEntry margin-right" style="width: 100px;" />
                        <select id="dd_newevent_endtime_pc" class="margin-right-sml">
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
                        <select id="dd_newevent_endtime_pc_ampm" class="margin-right">
                            <option value="AM">AM</option>
                            <option selected="selected" value="PM">PM</option>
                        </select>
                        <div class="clear"></div>
                        <div class="pad-top-big margin-top-big">
                            <div id="cal-color-yellow" class="inline-block cursor-pointer calendar-bg-selectors selected"
                                style="background-color: Yellow">
                            </div>
                            <div id="cal-color-blue" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Blue">
                            </div>
                            <div id="cal-color-gray" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Gray">
                            </div>
                            <div id="cal-color-orange" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Orange">
                            </div>
                            <div id="cal-color-red" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Red">
                            </div>
                            <div id="cal-color-green" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Green">
                            </div>
                        </div>
                        <div class="clear">
                        </div>
                        <div id="errormessagepc" style="color: Red;">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input type="button" class="input-buttons modal-cancel-btn" onclick="personalCalendarApp.AddNewCalendarEvent('', false)" value="Cancel" />
                    <input id="btn_createnewevent_pc" type="button" class="input-buttons modal-ok-btn" onclick="personalCalendarApp.btn_createnewevent_Clicked()" value="Create" />
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/PersonalCalendar/personalcalendar.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/PersonalCalendar/personalcalendar.js" />
