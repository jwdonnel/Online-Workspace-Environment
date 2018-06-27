<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Scheduler.aspx.cs" Inherits="Integrated_Pages_Scheduler" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Scheduler</title>
    <link rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/StyleSheets/Main/main.css" />
    <link id="Link2" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/StyleSheets/Main/sitemaster.css" />
    <link rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/StyleSheets/Main/jqueryUI.css" />
    <style type="text/css">
        body
        {
            background-color: #F2F2F2;
            background-color: transparent;
            font-size: 17px;
            line-height: 28px;
            font-weight: 400;
            font-style: normal;
        }

        .input-buttons-create
        {
            width: auto!important;
        }

        .loading-frame
        {
            position: absolute;
            top: 0;
            color: #FFF;
            background: #576c96;
            text-align: center;
            padding: 4px 5px;
            font-size: 15px;
            left: 50%;
            width: 300px;
            margin-left: -155px;
            -moz-box-shadow: 0 2px 4px rgba(0,0,0,0.5);
            -o-box-shadow: 0 2px 4px rgba(0,0,0,0.5);
            -webkit-box-shadow: 0 2px 4px rgba(0,0,0,0.5);
            box-shadow: 0 2px 4px rgba(0,0,0,0.5);
            display: none;
        }

        .ui-datepicker
        {
            font-size: 12px !important;
        }

        input.textEntry, .textEntry-noWidth
        {
            padding: 10px 15px!important;
        }

        td.contactFormInput
        {
            padding: 0 10px;
            text-align: left;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="updatepnl_newApp_deliverypickups" runat="server">
            <ContentTemplate>
                <script type="text/javascript" src="//code.jquery.com/jquery-1.12.3.min.js"></script>
                <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
                <script type="text/javascript" src="//code.jquery.com/ui/1.12.0/jquery-ui.min.js"></script>
                <script type="text/javascript">
                    function pageLoad() {
                        $("#tb_Date1_deliverypickups").datepicker();
                        $(".loading-frame").fadeOut(300, function () {
                            $(".loading-frame").remove();
                        });
                    }

                    $(document.body).on("click", ".deliverypickup-update", function () {
                        $('body').append('<div class="loading-frame">Updating Page. Please Wait.</div>');
                        $(".loading-frame").fadeIn(300);
                    });

                    function onTypeChange(_this) {
                        if (_this.selectedIndex == 0) {
                            $("#schedule_items_deliverypickups").removeAttr("disabled");
                        } else {
                            $("#schedule_items_deliverypickups").attr("disabled", "disabled");
                        }
                    }

                    function sendEmailChecked() {
                        var cb_sendEmail = document.getElementById("cb_sendEmail_deliverypickups");
                        var emailMarker = document.getElementById("emailmarker");
                        if (cb_sendEmail.checked == false) {
                            emailMarker.style.display = "none";
                            cb_sendEmail.checked = false;
                        } else {
                            emailMarker.style.display = "inline";
                            cb_sendEmail.checked = true;
                        }
                    }
                </script>
                <div class="pad-all">
                    <div class="pad-left pad-bottom">
                        Fill out the information below to schedule a new appointment for a delivery or pickup.
                    <span style="color: #F00; padding-left: 3px">*</span> are required fields.
                    </div>
                </div>
                <div class="float-left" style="width: 300px">
                    <table cellpadding="10" cellspacing="10" width="290px">
                        <tbody>
                            <tr>
                                <td class="contactFormInput">
                                    <div class="float-left">
                                        Delivery / Pickup<br />
                                        <select id="spd_deliverypickups" onchange="onTypeChange(this)" runat="server" style="width: 100px;">
                                            <option>Delivery</option>
                                            <option>Pickup</option>
                                        </select>
                                    </div>
                                    <div class="float-right PadRightBig">
                                        Items<br />
                                        <input id="schedule_items_deliverypickups" type="text" class="textEntry-noWidth" style="width: 30px; margin-right: 17px"
                                            maxlength="2" runat="server" />
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="contactFormInput">Truck Line<br />
                                    <input id="schedule_name_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                        style="width: 190px;" />
                                    <span style="color: Red; padding-left: 5px;"><small>*</small></span>
                                </td>
                            </tr>
                            <tr>
                                <td class="contactFormInput">Mill/Processor Name:<br />
                                    <input id="schedule_from_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                        style="width: 190px;" />
                                    <span style="color: Red; padding-left: 5px;"><small>*</small></span>
                                </td>
                            </tr>
                            <tr>
                                <td class="contactFormInput">E-mail<br />
                                    <input id="schedule_email_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                        style="width: 190px; margin-bottom: 5px;" />
                                    <span id="emailmarker" style="display: none; color: Red; padding-left: 5px;">
                                        <small>*</small></span><br />
                                    <span>Send Confirmation Email </span>
                                    <input id="cb_sendEmail_deliverypickups" type="checkbox" runat="server" onchange="sendEmailChecked()" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="float-left" style="padding: 10px 0; width: 265px">
                    Phone Number<br />
                    <input id="schedule_phone1_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                        style="width: 30px;" maxlength="3" />
                    -
                <input id="schedule_phone2_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                    style="width: 30px;" maxlength="3" />
                    -
                <input id="schedule_phone3_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                    style="width: 45px;" maxlength="4" />
                    <div class="clear-space">
                    </div>
                    Truck Number<br />
                    <input id="schedule_trucknum_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                        style="width: 190px; margin-bottom: 5px;" />
                    <div class="clear-space">
                    </div>
                    Comments (optional)<br />
                    <textarea id="schedule_comment_deliverypickups" runat="server" class="TextBoxComment"
                        rows="2" style="width: 215px; height: 65px; font-family: Arial; padding: 3px; border: 1px solid #D9D9D9"></textarea>
                    <div id="failMessage_deliverypickups" runat="server" class="clear">
                    </div>
                </div>
                <div class="float-left" style="padding: 10px 0">
                    Schedule Date<br />
                    <asp:TextBox runat="server" ID="tb_Date1_deliverypickups" autocomplete="off" OnTextChanged="tb_Date1_TextChanged"
                        CssClass="textEntry" Style="width: 100px;" AutoPostBack="True" />
                    <asp:LinkButton ID="lb_clearDate_deliverypickups" runat="server" CssClass="margin-left deliverypickup-update"
                        OnClick="lb_clearDate_Click">Clear Date</asp:LinkButton><br />
                    <small><i>Click in the box above to select a date</i></small>
                    <div class="clear" style="padding: 15px 0">
                        Schedule Time<br />
                        <div class="clear-margin pad-all">
                            Time slots not listed indicates another appointment<br />
                            (Hours: 6:30 am - 10:00 pm, Monday - Friday)
                        </div>
                        <asp:DropDownList ID="dd_schTimeSlot_deliverypickups" Style='width: 95px;' runat="server"
                            Enabled="False" Visible="False">
                        </asp:DropDownList>
                        <asp:Button ID="btn_selectTime_deliverypickups" runat="server" CssClass="input-buttons margin-left-big deliverypickup-update"
                            Text="Select Time" Enabled="False" Visible="False" OnClick="btn_selectTime_Click" /><div
                                class="clear-space">
                            </div>
                        <asp:Label ID="lbl_slotsopen_deliverypickups" runat="server" Enabled="False" Visible="False"></asp:Label>
                    </div>
                </div>
                <div class="clear-margin" style="text-align: center">
                    <br />
                    <asp:Label ID="lbl_SelectedDate_deliverypickups" runat="server" Text="" Enabled="false"
                        Visible="false"></asp:Label>
                    <div class="clear-space">
                    </div>
                    <asp:Button ID="btn_finish_deliverypickups" runat="server" Text="" CssClass="input-buttons-create deliverypickup-update"
                        Enabled="false" Visible="false" OnClick="btn_finish_Click" />
                </div>
                <asp:Panel ID="Step3_deliverypickups" runat="server" CssClass="pad-top" Style="padding-left: 20px; display: none">
                    <div id="CompleteSch_deliverypickups" runat="server">
                    </div>
                    <div class="clear-space">
                    </div>
                    Click
                <asp:LinkButton ID="btn_newApp_deliverypickups" OnClick="btn_newApp_Click" runat="server">HERE</asp:LinkButton>
                    to schedule a new appointment or click the close button at the top right of this
                popup.
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
