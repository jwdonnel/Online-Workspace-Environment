<%@ control language="C#" autoeventwireup="true" inherits="Apps_PersonalCalendar_PersonalCalendar, App_Web_jkeaai22" clientidmode="Static" %>
<div id="pCal-titlebar" class="pad-all app-title-bg-color">
    <div class="float-left">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
        <div class="clear"></div>
    </div>
    <div class="pad-left pad-right pad-top">
        <div class="searchwrapper float-right" style="width: 300px;">
            <input id="tb_search_pcal" type="text" class="searchbox" onfocus="if(this.value=='Search Current Month')this.value=''"
                onblur="if(this.value=='')this.value='Search Current Month'" onkeypress="KeyPressSearch_pc(event)"
                value="Search Current Month" />
            <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_pcal').val('Search Current Month');onCalendarLoad();return false;"></a><a href="#" class="searchbox_submit" onclick="onCalendarLoad();return false;"></a>
        </div>
    </div>
    <div class="clear"></div>
</div>
<input id="dateselected_pc" type="hidden" value="" />
<input id="hidden_date_pc" name="sdate" type="hidden" value="" />
<div class="pc-menu-bar">
    <a href="#" class="menu-btns" onclick="AddNewCalendarEvent_pc('', false);return false;">Add Event</a> <a href="#" class="menu-btns" onclick="HideTitleSearch_PC(this);return false;">Hide Title/Search</a>
    <div id="currmonthBtns_pc" class="currmonthBtns">
    </div>
</div>
<div id="usercalendar_pc" style="position: relative;">
</div>
<div id="hidden-div-holder-pc" style="display: none;">
</div>
<input type="hidden" id="hf_editmode-pc" />
<div id="ViewCalEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="510">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="ViewNewCalendarEvent_pc('');return false;" class="ModalExitButton"></a>
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
<div id="NewCalEvent-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="510">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="AddNewCalendarEvent_pc('', false);return false;" class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                            Title
                        </div>
                        <input id="tb_newevent_title_pc" type="text" class="textEntry" maxlength="250" style="width: 300px; border: 1px solid #CCC;" />
                        <div class="clear-space-five">
                        </div>
                        <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                            Description
                        </div>
                        <textarea id="tb_newevent_desc_pc" class="textEntry" rows="5" cols="80" style="width: 335px; font-family: Arial; border: 1px solid #CCC;"></textarea>
                        <div class="clear-space-five">
                        </div>
                        <div class="float-left pad-right-big">
                            <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                                Start Date
                            </div>
                            <input id="tb_newevent_start_pc" type="text" class="textEntry margin-right" style="width: 85px; border: 1px solid #CCC;" />
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
                            <div class="pad-left pad-right font-bold float-left" style="width: 85px; padding-top: 4px;">
                                End Date
                            </div>
                            <input id="tb_newevent_end_pc" type="text" class="textEntry margin-right" style="width: 85px; border: 1px solid #CCC;" />
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
                        </div>
                        <div class="float-right">
                            <div id="cal-color-yellow" class="inline-block cursor-pointer calendar-bg-selectors selected"
                                style="background-color: Yellow">
                            </div>
                            <div id="cal-color-blue" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Blue">
                            </div>
                            <div id="cal-color-gray" class="inline-block cursor-pointer calendar-bg-selectors"
                                style="background-color: Gray">
                            </div>
                            <div class="clear-space-two">
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
                        <div class="clear-space">
                        </div>
                        <input id="btn_createnewevent_pc" type="button" class="input-buttons float-right no-margin"
                            onclick="btn_createnewevent_pc_Clicked()" value="Create" />
                        <input type="button" class="input-buttons float-left" onclick="AddNewCalendarEvent_pc('', false)"
                            value="Cancel" />
                        <div id="errormessagepc" style="color: Red; height: 15px;">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
