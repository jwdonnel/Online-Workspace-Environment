<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AlarmClock.ascx.cs" Inherits="Apps_AlarmClock_AlarmClock"
    ClientIDMode="Static" %>
<asp:HiddenField ID="hf_currUser_AlarmClock" runat="server" />
<div class="pad-all">
    <div id="alarm-time">
        12:00:00 AM
    </div>
    <a href="#" id="minimize-controls" onclick="return false;" style="font-size: 11px;">Collapse controls</a>
    <div class="clear-space-five">
    </div>
    <div id="setAlarm-holder">
        <h3 class="font-bold float-left margin-right">Alarm Time</h3>
        (Hour/Minute/Second)
            <div class="clear-space-five">
            </div>
        <select id="ddl_alarmHour">
            <option value="12">12</option>
            <option value="1">1</option>
            <option value="2">2</option>
            <option value="3">3</option>
            <option value="4">4</option>
            <option value="5">5</option>
            <option value="6">6</option>
            <option value="7">7</option>
            <option value="8">8</option>
            <option value="9">9</option>
            <option value="10">10</option>
            <option value="11">11</option>
        </select><span> :</span>
        <select id="ddl_alarmMinute">
            <option value="00">00</option>
            <option value="01">01</option>
            <option value="02">02</option>
            <option value="03">03</option>
            <option value="04">04</option>
            <option value="05">05</option>
            <option value="06">06</option>
            <option value="07">07</option>
            <option value="08">08</option>
            <option value="09">09</option>
            <option value="10">10</option>
            <option value="11">11</option>
            <option value="12">12</option>
            <option value="13">13</option>
            <option value="14">14</option>
            <option value="15">15</option>
            <option value="16">16</option>
            <option value="17">17</option>
            <option value="18">18</option>
            <option value="19">19</option>
            <option value="20">20</option>
            <option value="21">21</option>
            <option value="22">22</option>
            <option value="23">23</option>
            <option value="24">24</option>
            <option value="25">25</option>
            <option value="26">26</option>
            <option value="27">27</option>
            <option value="28">28</option>
            <option value="29">29</option>
            <option value="30">30</option>
            <option value="31">31</option>
            <option value="32">32</option>
            <option value="33">33</option>
            <option value="34">34</option>
            <option value="35">35</option>
            <option value="36">36</option>
            <option value="37">37</option>
            <option value="38">38</option>
            <option value="39">39</option>
            <option value="40">40</option>
            <option value="41">41</option>
            <option value="42">42</option>
            <option value="43">43</option>
            <option value="44">44</option>
            <option value="45">45</option>
            <option value="46">46</option>
            <option value="47">47</option>
            <option value="48">48</option>
            <option value="49">49</option>
            <option value="50">50</option>
            <option value="51">51</option>
            <option value="52">52</option>
            <option value="53">53</option>
            <option value="54">54</option>
            <option value="55">55</option>
            <option value="56">56</option>
            <option value="57">57</option>
            <option value="58">58</option>
            <option value="59">59</option>
        </select><span> :</span>
        <select id="ddl_alarmSecond" class="margin-right-sml">
            <option value="00">00</option>
            <option value="01">01</option>
            <option value="02">02</option>
            <option value="03">03</option>
            <option value="04">04</option>
            <option value="05">05</option>
            <option value="06">06</option>
            <option value="07">07</option>
            <option value="08">08</option>
            <option value="09">09</option>
            <option value="10">10</option>
            <option value="11">11</option>
            <option value="12">12</option>
            <option value="13">13</option>
            <option value="14">14</option>
            <option value="15">15</option>
            <option value="16">16</option>
            <option value="17">17</option>
            <option value="18">18</option>
            <option value="19">19</option>
            <option value="20">20</option>
            <option value="21">21</option>
            <option value="22">22</option>
            <option value="23">23</option>
            <option value="24">24</option>
            <option value="25">25</option>
            <option value="26">26</option>
            <option value="27">27</option>
            <option value="28">28</option>
            <option value="29">29</option>
            <option value="30">30</option>
            <option value="31">31</option>
            <option value="32">32</option>
            <option value="33">33</option>
            <option value="34">34</option>
            <option value="35">35</option>
            <option value="36">36</option>
            <option value="37">37</option>
            <option value="38">38</option>
            <option value="39">39</option>
            <option value="40">40</option>
            <option value="41">41</option>
            <option value="42">42</option>
            <option value="43">43</option>
            <option value="44">44</option>
            <option value="45">45</option>
            <option value="46">46</option>
            <option value="47">47</option>
            <option value="48">48</option>
            <option value="49">49</option>
            <option value="50">50</option>
            <option value="51">51</option>
            <option value="52">52</option>
            <option value="53">53</option>
            <option value="54">54</option>
            <option value="55">55</option>
            <option value="56">56</option>
            <option value="57">57</option>
            <option value="58">58</option>
            <option value="59">59</option>
        </select>
        <select id="ddl_alarmTimeOfDay">
            <option selected="selected" value="AM">AM</option>
            <option value="PM">PM</option>
        </select>
        <div class="clear-space">
        </div>
        <a href="#" id="btn_setAlarm" onclick="return false;">Set Alarm</a>
        <div class="clear-space">
        </div>
        <asp:RadioButton ID="rbAlarmOn" runat="server" GroupName="alarmOnOff" Text=" On" CssClass="margin-right" />
        <asp:RadioButton ID="rbAlarmOff" runat="server" GroupName="alarmOnOff" Text=" Off" />
        <div class="clear-space-two">
        </div>
        <a href="#" id="btnResetAlarm" onclick="return false;" style="font-size: 11px;">Reset</a>
        <div class="clear-space">
        </div>
        <span id="alarmTextSet"></span>
    </div>
</div>
<div id="alarmclock-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" style="min-width: 275px!important;">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalPadContent">
                    <h3 class="float-left pad-right">Set Snooze for
                    </h3>
                    <select id="ddl_snooze" class="float-left margin-right">
                        <option value="1">1</option>
                        <option value="2">2</option>
                        <option value="3">3</option>
                        <option value="4">4</option>
                        <option value="5" selected="selected">5</option>
                        <option value="10">10</option>
                        <option value="15">15</option>
                        <option value="20">20</option>
                    </select>
                    <h3 class="float-left">minute(s)</h3>
                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <input id="alarmClockSnooze" class="input-buttons" value="Snooze" style="width: 55px;" />
                    <input id="alarmClockCancel" class="input-buttons" value="Cancel" style="width: 55px;" />
                    <div id="alarmSound">
                    </div>
                    <div class="clear-space">
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/AlarmClock/alarmClock.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/AlarmClock/alarmClock.js" />