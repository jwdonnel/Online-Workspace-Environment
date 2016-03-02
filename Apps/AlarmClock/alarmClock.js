$(function () {
    $(document).ready(function () {
        alarmClock.initializeOnLoad();
    });

    var alarmclockTextTimer;
    var alarmClockVars = {
        $timeHolder: null,
        alarmInterval: null,
        modalTitle: "Alarm!",
        currentAlarm: "",
        currentAlarmState: "",
        wavFile: "Apps/AlarmClock/alarm.wav",
        initSize: 300
    };

    var alarmClock = {
        initializeOnLoad: function () {
            if (alarmClockVars.alarmInterval == null) {
                alarmClock.start();
            }
        },

        start: function () {
            alarmClockVars.$timeHolder = $("#alarmclock-load").find("#alarm-time");
            var expanded = cookie.get("alarm-clock-controls_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
            if ((expanded != "") && (expanded != null) && (expanded != undefined)) {
                if (expanded == "0") {
                    $("#alarmclock-load").find("#minimize-controls").html("Expand controls");
                    $("#alarmclock-load").css("background", "transparent");
                    $("#alarmclock-load").find("#setAlarm-holder").hide();
                    $("#app-alarmclock").css({
                        height: 145,
                        minHeight: 145
                    });
                }
            }

            var currentAlarm = cookie.get("alarm-clock_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
            if ((currentAlarm == "") || (currentAlarm == null) || (currentAlarm == undefined)) {
                if (alarmClockVars.currentAlarmState == "") {
                    $("#alarmclock-load").find("#rbAlarmOff").prop("checked", true);
                    alarmClockVars.currentAlarmState = "off";
                }
            }
            else {
                var splitAlarmTime = currentAlarm.split(" ");
                if (splitAlarmTime.length == 4) {
                    $("#alarmclock-load").find("#ddl_alarmHour").val(splitAlarmTime[0]);
                    $("#alarmclock-load").find("#ddl_alarmMinute").val(splitAlarmTime[1]);
                    $("#alarmclock-load").find("#ddl_alarmSecond").val(splitAlarmTime[2]);
                    $("#alarmclock-load").find("#ddl_alarmTimeOfDay").val(splitAlarmTime[3]);

                    if (alarmClockVars.currentAlarm == "") {
                        alarmClockVars.currentAlarm = splitAlarmTime[0] + ":" + splitAlarmTime[1] + ":" + splitAlarmTime[2] + " " + splitAlarmTime[3];
                    }
                }

                var currentAlarmState = cookie.get("alarm-clock-state_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
                if ((currentAlarmState == "") || (currentAlarmState == null) || (currentAlarmState == undefined) || (currentAlarmState == "off")) {
                    if (alarmClockVars.currentAlarmState == "") {
                        $("#alarmclock-load").find("#rbAlarmOff").prop("checked", true);
                        alarmClockVars.currentAlarmState = "off";
                    }
                }
                else {
                    if (alarmClockVars.currentAlarmState == "") {
                        $("#alarmclock-load").find("#rbAlarmOn").prop("checked", true);
                        alarmClockVars.currentAlarmState = "on";
                    }
                }
            }

            var date = new Date();
            var currTime = date.toLocaleTimeString();
            if (alarmClockVars.$timeHolder) {
                alarmClockVars.$timeHolder.html(currTime);
                if ((currTime == alarmClockVars.currentAlarm) && (alarmClockVars.currentAlarmState == "on")) {
                    this.alarmHorn();
                }

                alarmClockVars.intveral = setInterval(function () {
                    date = new Date();
                    var currTime_2 = date.toLocaleTimeString();
                    alarmClockVars.$timeHolder.html(currTime_2);
                    if ((currTime_2 == alarmClockVars.currentAlarm) && (alarmClockVars.currentAlarmState == "on")) {
                        alarmClock.alarmHorn();
                    }
                }, 1000);
            }
        },

        snooze: function () {
            $("#alarmclock-element").find("#alarmSound").html("");
            var snoozeMinutes = parseInt($("#alarmclock-element").find("#ddl_snooze").val());

            var success = false;

            var splitStr = alarmClockVars.currentAlarm.split(":");
            if (splitStr.length == 3) {
                var hour = parseInt(splitStr[0]);
                var min = parseInt(splitStr[1]);
                var splitStr2 = splitStr[2].split(" ");
                if (splitStr2.length == 2) {
                    var sec = parseInt(splitStr2[0]);
                    if (splitStr2[1] == "PM") {
                        hour += 12;
                    }

                    var snoozeTime = new Date();
                    snoozeTime.setHours(hour);
                    snoozeTime.setMinutes(min + snoozeMinutes);
                    snoozeTime.setSeconds(sec);
                    alarmClockVars.currentAlarm = snoozeTime.toLocaleTimeString();
                    this.displayMessage("Snooze set for " + snoozeMinutes + " minute(s)", "#EEE");
                    success = true;
                }
            }

            openWSE.LoadModalWindow(false, "alarmclock-element", "");

            if (!success) {
                this.displayMessage("Could not set snooze", "Red");
            }
        },

        cancel: function () {
            alarmClockVars.currentAlarm = $("#alarmclock-load").find("#ddl_alarmHour").val() + ":" + $("#alarmclock-load").find("#ddl_alarmMinute").val() + ":" + $("#alarmclock-load").find("#ddl_alarmSecond").val() + " " + $("#alarmclock-load").find("#ddl_alarmTimeOfDay").val();
            $("#alarmclock-element").find("#alarmSound").html("");
            openWSE.LoadModalWindow(false, "alarmclock-element", "");
        },

        alarmHorn: function () {
            var filename = alarmClockVars.wavFile;
            var x = '<div style="visibility:hidden"><audio autoplay="autoplay" loop><source src="' + filename + '" type="audio/x-wav" />';
            x += '<embed hidden="true" autostart="true" loop="false" src="' + filename + '" /></audio></div>';
            $("#alarmclock-element").find("#alarmSound").html(x);
            openWSE.LoadModalWindow(true, "alarmclock-element", alarmClockVars.modalTitle);
        },

        set: function () {
            var setHour = parseInt($("#alarmclock-load").find("#ddl_alarmHour").val());
            var setMinutes = parseInt($("#alarmclock-load").find("#ddl_alarmMinute").val());
            var setSeconds = parseInt($("#alarmclock-load").find("#ddl_alarmSecond").val());
            if ($("#alarmclock-load").find("#ddl_alarmTimeOfDay").val() == "PM") {
                setHour += 12;
            }

            var EndTime = new Date();
            EndTime.setHours(setHour);
            EndTime.setMinutes(setMinutes);
            EndTime.setSeconds(setSeconds);

            var minutes = this.parseTime(EndTime.toLocaleTimeString()) - this.parseTime(new Date().toLocaleTimeString());

            var hours = Math.floor(minutes / 60);
            if (hours < 0) {
                if (Math.abs(hours) < 12) {
                    hours = 12 + hours;
                }
                else {
                    hours = 24 + hours;
                }
            }

            if (minutes < 0) {
                minutes = Math.abs(minutes);
                minutes = minutes % 60;
                minutes = 60 - minutes;
            }
            else {
                minutes = minutes % 60;
            }

            var seconds = EndTime.getSeconds() - new Date().getSeconds();
            if (seconds < 0) {
                seconds = 60 + seconds;
                minutes--;
            }

            if (((hours.toString().charAt(0) != "0") && (hours.toString().length == 1)) || ((hours.toString().charAt(0) == "0") && (hours.toString().length == 1))) {
                hours = "0" + hours.toString();
            }
            if (((minutes.toString().charAt(0) != "0") && (minutes.toString().length == 1)) || ((minutes.toString().charAt(0) == "0") && (minutes.toString().length == 1))) {
                minutes = "0" + minutes.toString();
            }
            if (((seconds.toString().charAt(0) != "0") && (seconds.toString().length == 1)) || ((seconds.toString().charAt(0) == "0") && (seconds.toString().length == 1))) {
                seconds = "0" + seconds.toString();
            }

            alarmClockVars.currentAlarm = $("#alarmclock-load").find("#ddl_alarmHour").val() + ":" + $("#alarmclock-load").find("#ddl_alarmMinute").val() + ":" + $("#alarmclock-load").find("#ddl_alarmSecond").val() + " " + $("#alarmclock-load").find("#ddl_alarmTimeOfDay").val();
            alarmClockVars.currentAlarmState = "on";

            cookie.set("alarm-clock_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val(), $("#alarmclock-load").find("#ddl_alarmHour").val() + " " + $("#alarmclock-load").find("#ddl_alarmMinute").val() + " " + $("#alarmclock-load").find("#ddl_alarmSecond").val() + " " + $("#alarmclock-load").find("#ddl_alarmTimeOfDay").val(), 30);
            cookie.set("alarm-clock-state_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val(), "on", 30);
            $("#alarmclock-load").find("#rbAlarmOn").prop("checked", true);

            this.displayMessage("Set to go off in " + hours + ":" + minutes + ":" + seconds + " (hh/mm/ss)", "#EEE");
        },

        parseTime: function (s) {
            var c = s.split(':');
            var hours = parseInt(c[0]);
            if (c.length == 3) {
                var c2 = c[2].split(" ");
                if (c2.length == 2) {
                    if (c2[1] == "PM") {
                        hours += 12;
                    }
                }
            }
            return hours * 60 + parseInt(c[1]);
        },

        turnOn: function () {
            cookie.set("alarm-clock-state_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val(), "on", 30);
            $("#alarmclock-load").find("#rbAlarmOn").prop("checked", true);
            alarmClockVars.currentAlarmState = "on";
            this.displayMessage("Alarm turned on", "#EEE");
        },

        turnOff: function () {
            cookie.set("alarm-clock-state_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val(), "off", 30);
            $("#alarmclock-load").find("#rbAlarmOff").prop("checked", true);
            alarmClockVars.currentAlarmState = "off";
            this.displayMessage("Alarm turned off", "#EEE");
        },

        displayMessage: function (message, color) {
            clearTimeout(alarmclockTextTimer);
            $("#alarmclock-load").find("#alarmTextSet").css("color", color);
            $("#alarmclock-load").find("#alarmTextSet").html(message);
            alarmclockTextTimer = setTimeout(function () {
                $("#alarmclock-load").find("#alarmTextSet").html("");
            }, 5000);
        }
    }

    $("#alarmclock-load").find("#btn_setAlarm").click(function () {
        alarmClock.set();
    });

    $("#alarmclock-load").find("#rbAlarmOn").change(function () {
        alarmClock.turnOn();
    });

    $("#alarmclock-load").find("#rbAlarmOff").change(function () {
        alarmClock.turnOff();
    });

    $("#alarmclock-element").find("#alarmClockSnooze").click(function () {
        alarmClock.snooze();
    });

    $("#alarmclock-element").find("#alarmClockCancel").click(function () {
        alarmClock.cancel();
    });

    $("#alarmclock-load").find("#minimize-controls").click(function () {
        if ($("#alarmclock-load").find("#setAlarm-holder").css("display") == "none") {
            $(this).html("Collapse controls");
            $("#alarmclock-load").css("background", "");
            $("#alarmclock-load").find("#setAlarm-holder").slideDown(openWSE_Config.animationSpeed);
            $("#app-alarmclock").animate({
                height: alarmClockVars.initSize,
                minHeight: alarmClockVars.initSize
            }, openWSE_Config.animationSpeed);
            cookie.del("alarm-clock-controls_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
        }
        else {
            $(this).html("Expand controls");
            $("#alarmclock-load").css("background", "transparent");
            $("#alarmclock-load").find("#setAlarm-holder").slideUp(openWSE_Config.animationSpeed);
            $("#app-alarmclock").animate({
                height: 145,
                minHeight: 145
            }, openWSE_Config.animationSpeed);
            cookie.set("alarm-clock-controls_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val(), "0", 30);
        }
    });

    $("#alarmclock-load").find("#btnResetAlarm").click(function () {
        openWSE.ConfirmWindow("Are you sure you want to reset the alarm clock? Reseting this deletes any cookies this app creates and sets all values back to the default.",
            function () {
                $("#alarmclock-load").find("#ddl_alarmHour").val("12");
                $("#alarmclock-load").find("#ddl_alarmMinute").val("00");
                $("#alarmclock-load").find("#ddl_alarmSecond").val("00");
                $("#alarmclock-load").find("#ddl_alarmTimeOfDay").val("AM");
                alarmClockVars.currentAlarm = "12:00:00 AM";
                alarmClockVars.currentAlarmState = "off";

                cookie.del("alarm-clock_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
                cookie.del("alarm-clock-state_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
                cookie.del("alarm-clock-controls_" + $("#alarmclock-load").find("#hf_currUser_AlarmClock").val());
                $("#alarmclock-load").find("#rbAlarmOff").prop("checked", true);
            }, null);
    });
});