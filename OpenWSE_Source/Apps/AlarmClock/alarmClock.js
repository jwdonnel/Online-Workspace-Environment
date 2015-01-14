$(function () {
    $(document).ready(function () {
        if (alarmClockVars.alarmInterval == null) {
            alarmClock.start();
        }
    });

    var alarmclockTextTimer;
    var alarmClockVars = {
        $timeHolder: null,
        alarmInterval: null,
        modalTitle: "Alarm!",
        currentAlarm: "",
        currentAlarmState: "",
        wavFile: "Apps/AlarmClock/alarm.wav",
        initSize: 300,
        $mainHolder: $("#alarmclock-load"),
        $mainElement: $("#alarmclock-element")
    };

    var alarmClock = {
        start: function () {
            alarmClockVars.$timeHolder = alarmClockVars.$mainHolder.find("#alarm-time");
            var expanded = cookie.get("alarm-clock-controls_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
            if ((expanded != "") && (expanded != null) && (expanded != undefined)) {
                if (expanded == "0") {
                    alarmClockVars.$mainHolder.find("#minimize-controls").html("Expand controls");
                    $("#alarmclock-load").css("background", "transparent");
                    alarmClockVars.$mainHolder.find("#setAlarm-holder").hide();
                    $("#app-alarmclock").css({
                        height: 145,
                        minHeight: 145
                    });
                }
            }

            var currentAlarm = cookie.get("alarm-clock_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
            if ((currentAlarm == "") || (currentAlarm == null) || (currentAlarm == undefined)) {
                if (alarmClockVars.currentAlarmState == "") {
                    alarmClockVars.$mainHolder.find("#rbAlarmOff").prop("checked", true);
                    alarmClockVars.currentAlarmState = "off";
                }
            }
            else {
                var splitAlarmTime = currentAlarm.split(" ");
                if (splitAlarmTime.length == 4) {
                    alarmClockVars.$mainHolder.find("#ddl_alarmHour").val(splitAlarmTime[0]);
                    alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val(splitAlarmTime[1]);
                    alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val(splitAlarmTime[2]);
                    alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val(splitAlarmTime[3]);

                    if (alarmClockVars.currentAlarm == "") {
                        alarmClockVars.currentAlarm = splitAlarmTime[0] + ":" + splitAlarmTime[1] + ":" + splitAlarmTime[2] + " " + splitAlarmTime[3];
                    }
                }

                var currentAlarmState = cookie.get("alarm-clock-state_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
                if ((currentAlarmState == "") || (currentAlarmState == null) || (currentAlarmState == undefined) || (currentAlarmState == "off")) {
                    if (alarmClockVars.currentAlarmState == "") {
                        alarmClockVars.$mainHolder.find("#rbAlarmOff").prop("checked", true);
                        alarmClockVars.currentAlarmState = "off";
                    }
                }
                else {
                    if (alarmClockVars.currentAlarmState == "") {
                        alarmClockVars.$mainHolder.find("#rbAlarmOn").prop("checked", true);
                        alarmClockVars.currentAlarmState = "on";
                    }
                }
            }

            var date = new Date();
            var currTime = date.toLocaleTimeString();
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
        },

        snooze: function () {
            alarmClockVars.$mainElement.find("#alarmSound").html("");
            var snoozeMinutes = parseInt(alarmClockVars.$mainElement.find("#ddl_snooze").val());

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
                    this.displayMessage("Snooze set for " + snoozeMinutes + " minute(s)", "#555");
                    success = true;
                }
            }

            openWSE.LoadModalWindow(false, "alarmclock-element", "");

            if (!success) {
                this.displayMessage("Could not set snooze", "Red");
            }
        },

        cancel: function () {
            alarmClockVars.currentAlarm = alarmClockVars.$mainHolder.find("#ddl_alarmHour").val() + ":" + alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val() + ":" + alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val() + " " + alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val();
            alarmClockVars.$mainElement.find("#alarmSound").html("");
            openWSE.LoadModalWindow(false, "alarmclock-element", "");
        },

        alarmHorn: function () {
            var filename = alarmClockVars.wavFile;
            var x = '<div style="visibility:hidden"><audio autoplay="autoplay" loop><source src="' + filename + '" type="audio/x-wav" />';
            x += '<embed hidden="true" autostart="true" loop="false" src="' + filename + '" /></audio></div>';
            alarmClockVars.$mainElement.find("#alarmSound").html(x);
            openWSE.LoadModalWindow(true, "alarmclock-element", alarmClockVars.modalTitle);
        },

        set: function () {
            var setHour = parseInt(alarmClockVars.$mainHolder.find("#ddl_alarmHour").val());
            var setMinutes = parseInt(alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val());
            var setSeconds = parseInt(alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val());
            if (alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val() == "PM") {
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

            alarmClockVars.currentAlarm = alarmClockVars.$mainHolder.find("#ddl_alarmHour").val() + ":" + alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val() + ":" + alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val() + " " + alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val();
            alarmClockVars.currentAlarmState = "on";

            cookie.set("alarm-clock_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val(), alarmClockVars.$mainHolder.find("#ddl_alarmHour").val() + " " + alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val() + " " + alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val() + " " + alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val(), 30);
            cookie.set("alarm-clock-state_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val(), "on", 30);
            alarmClockVars.$mainHolder.find("#rbAlarmOn").prop("checked", true);

            this.displayMessage("Set to go off in " + hours + ":" + minutes + ":" + seconds + " (hh/mm/ss)", "#555");
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
            cookie.set("alarm-clock-state_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val(), "on", 30);
            alarmClockVars.$mainHolder.find("#rbAlarmOn").prop("checked", true);
            alarmClockVars.currentAlarmState = "on";
            this.displayMessage("Alarm turned on", "#555");
        },

        turnOff: function () {
            cookie.set("alarm-clock-state_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val(), "off", 30);
            alarmClockVars.$mainHolder.find("#rbAlarmOff").prop("checked", true);
            alarmClockVars.currentAlarmState = "off";
            this.displayMessage("Alarm turned off", "#555");
        },

        displayMessage: function (message, color) {
            clearTimeout(alarmclockTextTimer);
            alarmClockVars.$mainHolder.find("#alarmTextSet").css("color", color);
            alarmClockVars.$mainHolder.find("#alarmTextSet").html(message);
            alarmclockTextTimer = setTimeout(function () {
                alarmClockVars.$mainHolder.find("#alarmTextSet").html("");
            }, 5000);
        }
    }

    alarmClockVars.$mainHolder.find("#btn_setAlarm").click(function () {
        alarmClock.set();
    });

    alarmClockVars.$mainHolder.find("#rbAlarmOn").change(function () {
        alarmClock.turnOn();
    });

    alarmClockVars.$mainHolder.find("#rbAlarmOff").change(function () {
        alarmClock.turnOff();
    });

    alarmClockVars.$mainElement.find("#alarmClockSnooze").click(function () {
        alarmClock.snooze();
    });

    alarmClockVars.$mainElement.find("#alarmClockCancel").click(function () {
        alarmClock.cancel();
    });

    alarmClockVars.$mainHolder.find("#minimize-controls").click(function () {
        if (alarmClockVars.$mainHolder.find("#setAlarm-holder").css("display") == "none") {
            $(this).html("Collapse controls");
            alarmClockVars.$mainHolder.find("#setAlarm-holder").slideDown(openWSE_Config.animationSpeed, function () {
                $("#alarmclock-load").css("background", "#FFFFFF");
            });
            $("#app-alarmclock").animate({
                height: alarmClockVars.initSize,
                minHeight: alarmClockVars.initSize
            }, openWSE_Config.animationSpeed);
            cookie.del("alarm-clock-controls_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
        }
        else {
            $(this).html("Expand controls");
            alarmClockVars.$mainHolder.find("#setAlarm-holder").slideUp(openWSE_Config.animationSpeed, function () {
                $("#alarmclock-load").css("background", "transparent");
            });
            $("#app-alarmclock").animate({
                height: 145,
                minHeight: 145
            }, openWSE_Config.animationSpeed);
            cookie.set("alarm-clock-controls_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val(), "0", 30);
        }
    });

    alarmClockVars.$mainHolder.find("#btnResetAlarm").click(function () {
        openWSE.ConfirmWindow("Are you sure you want to reset the alarm clock? Reseting this deletes any cookies this app creates and sets all values back to the default.",
            function () {
                alarmClockVars.$mainHolder.find("#ddl_alarmHour").val("12");
                alarmClockVars.$mainHolder.find("#ddl_alarmMinute").val("00");
                alarmClockVars.$mainHolder.find("#ddl_alarmSecond").val("00");
                alarmClockVars.$mainHolder.find("#ddl_alarmTimeOfDay").val("AM");
                alarmClockVars.currentAlarm = "12:00:00 AM";
                alarmClockVars.currentAlarmState = "off";

                cookie.del("alarm-clock_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
                cookie.del("alarm-clock-state_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
                cookie.del("alarm-clock-controls_" + alarmClockVars.$mainHolder.find("#hf_currUser_AlarmClock").val());
                alarmClockVars.$mainHolder.find("#rbAlarmOff").prop("checked", true);
            }, null);
    });
});