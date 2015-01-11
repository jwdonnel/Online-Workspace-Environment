<%@ Page Language="C#" AutoEventWireup="true" CodeFile="eRequest.aspx.cs" Inherits="Integrated_Pages_eRequest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>eRequest</title>
    <link id="Link1" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/site_desktop.css" />
    <link id="Link3" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
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
        input.textEntry
        {
            width: 225px !important;
        }
        input.textEntryPhoneNumber
        {
            -moz-border-radius: 2px;
            -webkit-border-radius: 2px;
            background: #FFF;
            border: 1px solid #D9D9D9 !important;
            border-radius: 2px;
            border-top: 1px solid #C0C0C0 !important;
            box-sizing: border-box;
            color: #8E8E8E;
            font-family: Arial,sans-serif;
            font-size: 13px;
            font-style: normal;
            padding: 4px 8px;
            width: 50px;
        }
        input.textEntryPostal
        {
            -moz-border-radius: 2px;
            -webkit-border-radius: 2px;
            background: #FFF;
            border: 1px solid #D9D9D9 !important;
            border-radius: 2px;
            border-top: 1px solid #C0C0C0 !important;
            box-sizing: border-box;
            color: #8E8E8E;
            font-family: Arial,sans-serif;
            font-size: 13px;
            font-style: normal;
            padding: 4px 8px;
            width: 75px;
        }
        input.textEntryQty
        {
            -moz-border-radius: 2px;
            -webkit-border-radius: 2px;
            background: #FFF;
            border: 1px solid #D9D9D9 !important;
            border-radius: 2px;
            border-top: 1px solid #C0C0C0 !important;
            box-sizing: border-box;
            color: #8E8E8E;
            font-family: Arial,sans-serif;
            font-size: 13px;
            font-style: normal;
            padding: 4px 8px;
            width: 50px;
        }
        input.textEntryCustom
        {
            -moz-border-radius: 2px;
            -webkit-border-radius: 2px;
            background: #FFF;
            border: 1px solid #D9D9D9 !important;
            border-radius: 2px;
            border-top: 1px solid #C0C0C0 !important;
            box-sizing: border-box;
            color: #8E8E8E;
            font-family: Arial,sans-serif;
            font-size: 13px;
            font-style: normal;
            padding: 4px 8px;
            width: 47px;
        }
        #update-element-request
        {
            display: none;
        }
        .update-element-modal-request
        {
            background: #F9F9F9;
            -moz-box-shadow: -3px 3px 3px rgba(0,0,0,.5);
            -webkit-box-shadow: 0 5px 7px rgba(0,0,0,.5);
            border: 3px solid #7f7f7f;
            border-bottom: 3px solid #707070 !important;
            box-shadow: 0 5px 7px rgba(0,0,0,.5);
            margin: 0 auto;
            text-align: center;
            z-index: 5000;
            -moz-border-radius: 15px;
            -webkit-border-radius: 15px;
            border-radius: 15px;
        }
        .update-element-modal-request
        {
            padding: 12px 20px 0 20px;
        }
        .update-element-align-load-request
        {
            position: fixed;
            text-align: left;
            z-index: 5000;
            top: 50%;
            left: 50%;
            margin-left: -180px;
            margin-top: -35px;
        }
        .update-element-modal-request h3
        {
            color: #353535;
            clear: both;
            margin-top: 1px;
            margin-left: 10px;
        }
        .update-element-modal-request .input-buttons
        {
            width: 75px;
        }
        #page-load-ab-request
        {
            height: 100%;
            left: 0;
            position: fixed;
            text-align: center;
            top: 0;
            width: 100%;
            z-index: 10000;
        }
        #page-load-ab
        {
            background: rgba(0,0,0,0.7);
            height: 100%;
            left: 0;
            position: fixed;
            text-align: center;
            top: 34px;
            width: 100%;
            z-index: 7000;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
    </asp:ScriptManager>
    <asp:HiddenField ID="hf_SessionID" runat="server" />
    <asp:UpdatePanel ID="updatepnl_newApp_deliverypickups" runat="server">
        <ContentTemplate>
            <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
            <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
            <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
            <script type="text/javascript">
                var standardSize = true;
                function pageLoad() {
                    $(".loading-frame").fadeOut(300, function () {
                        $(".loading-frame").remove();
                    });

                    if (standardSize) {
                        $('#tb_WidthCustom,#tb_LengthCustom,#a_standard').hide();
                        $('#ddl_Length,#ddl_Width,#a_customsize').show();
                        $('#tb_WidthCustom,#tb_LengthCustom').val("");
                    }
                    else {
                        $('#ddl_Length,#ddl_Width,#a_customsize').hide();
                        $('#tb_WidthCustom,#tb_LengthCustom,#a_standard').show();
                    }

                    CalculateWeight();
                }

                $(document.body).on("click", ".contact-update", function () {
                    $('body').append('<div class="loading-frame">Updating Page. Please Wait.</div>');
                    $(".loading-frame").fadeIn(300);
                });

                $(document.body).on("change", ".contact-update-ddl", function () {
                    $('body').append('<div class="loading-frame">Updating Page. Please Wait.</div>');
                    $(".loading-frame").fadeIn(300);
                });

                function RemoveMessage() {
                    setTimeout(function () {
                        $('#returnMessage').fadeOut(300, function () {
                            $('#returnMessage').remove();
                        });
                    }, 3000);
                }

                function UseStandardSize() {
                    standardSize = true;
                    $('#tb_WidthCustom,#tb_LengthCustom,#a_standard').fadeOut(200, function () {
                        $('#ddl_Length,#ddl_Width,#a_customsize').fadeIn(200);
                        $('#tb_WidthCustom,#tb_LengthCustom').val("");
                    });
                }

                function UseCustomSize() {
                    standardSize = false;
                    $('#ddl_Length,#ddl_Width,#a_customsize').fadeOut(200, function () {
                        $('#tb_WidthCustom,#tb_LengthCustom,#a_standard').fadeIn(200);
                    });
                }

                function AnyMoreRequests() {
                    var x = "<div id='update-element-request'><div id='page-load-ab-request'>";
                    x += "<div class='update-element-align-load-request'><div class='update-element-modal-request' style='height:70px!important'>";
                    x += "<h3 class='inline-block'>Are you sure you want to send the inquires?</h3>";
                    x += "<div class='clear-margin'>";
                    x += "<input type='button' class='input-buttons contact-update' value='Yes' onclick='SendRequest();' />";
                    x += "<input type='button' class='input-buttons' value='Cancel' onclick='CancelRequest();' style='margin-right:0!important' />";
                    x += "</div></div></div></div></div>";
                    $("body").append(x);
                    $("#update-element-request").fadeIn(300, function () {
                        ReadjustUpdateAlignment();
                    });
                }

                function ResetConfirmation() {
                    var x = "<div id='update-element-request'><div id='page-load-ab-request'>";
                    x += "<div class='update-element-align-load-request'><div class='update-element-modal-request' style='height:100px!important'>";
                    x += "<h3 class='inline-block'>What would you like Reset?</h3><div class='clear'></div><b>Note:</b> Reseting all will clear out previous<br />requests and will not send them.";
                    x += "<div class='clear-margin'><input type='button' class='input-buttons contact-update' value='Current' onclick='ClearOutCurrent();' style='margin-left:7px' />";
                    x += "<input type='button' class='input-buttons contact-update' value='All' onclick='ClearAll();' />";
                    x += "<input type='button' class='input-buttons' value='Cancel' onclick='CancelRequest();' />";
                    x += "</div></div></div></div></div>";
                    $("body").append(x);
                    $("#update-element-request").fadeIn(300, function () {
                        ReadjustUpdateAlignment();
                    });
                }

                function deleteEntry(i) {
                    var r = confirm("Are you sure you want to delete this entry?")
                    if (r == true) {
                        $('body').append('<div class="loading-frame">Deleting Entry. Please Wait.</div>');
                        $(".loading-frame").fadeIn(300);
                        document.getElementById("hf_DeleteRequest").value = i;
                        __doPostBack("hf_DeleteRequest", "");
                    }
                }

                function ReadjustUpdateAlignment() {
                    var currUpdateWidth = -($(".update-element-modal").width() / 2);
                    $(".update-element-align-load").css({
                        marginLeft: currUpdateWidth,
                        top: "50%"
                    });
                }

                function ClearOutCurrent() {
                    $("#update-element-request").fadeOut(300, function () {
                        $("#update-element-request").remove();
                    });
                    document.getElementById("hf_ResetRequest").value = "current";
                    __doPostBack("hf_ResetRequest", "");
                }

                function ClearAll() {
                    $("#update-element-request").fadeOut(300, function () {
                        $("#update-element-request").remove();
                    });
                    document.getElementById("hf_ResetRequest").value = "all";
                    __doPostBack("hf_ResetRequest", "");
                }

                function SendRequest() {
                    $("#update-element-request").fadeOut(300, function () {
                        $("#update-element-request").remove();
                    });
                    document.getElementById("hf_SendRequest").value = new Date().toString();
                    __doPostBack("hf_SendRequest", "");
                }

                function CancelRequest() {
                    $("#update-element-request").fadeOut(300, function () {
                        $("#update-element-request").remove();
                    });
                }

                $(document.body).on("keyup", "#tb_Quantity", function (event) {
                    CalculateWeight();
                });

                function CalculateWeight() {
                    var textEntered = $("#tb_Quantity").val();
                    var $_totalWeight = $("#lbl_totalWeight");
                    if ((textEntered == "") || (textEntered == undefined) || (textEntered == null)) {
                        $_totalWeight.html("");
                    }
                    else {
                        if ((textEntered.indexOf("0") != -1) || (textEntered.indexOf("1") != -1) || (textEntered.indexOf("2") != -1) || (textEntered.indexOf("3") != -1) || (textEntered.indexOf("4") != -1) || (textEntered.indexOf("5") != -1) || (textEntered.indexOf("6") != -1) || (textEntered.indexOf("7") != -1) || (textEntered.indexOf("8") != -1) || (textEntered.indexOf("9") != -1)) {
                            try {
                                var x = parseFloat(textEntered);
                                var z = $("#weightPer").html();
                                if ((z != "") && (z != undefined) && (z != null)) {
                                    var y = parseFloat(z);
                                    $_totalWeight.html("<br /><b class='pad-right'>Total Weight:</b>" + (x * y) + " lbs");
                                }
                                else {
                                    $_totalWeight.html("");
                                }
                            }
                            catch (evt) { }
                        }
                    }
                }
            </script>
            <div class="float-left" style="width: 470px; height: 48px">
                To request pricing and availability on our stock, please fill out the form below.<br />
                <b class="pad-right">How To:</b>To send multiple inquires, select 'Add Inquire'.<br />
                Your contact information will be saved/updated each time you add a new inquire.
            </div>
            <div class="float-right" style="padding-top: 28px">
                <asp:Label ID="lbl_Total" runat="server" Font-Size="Medium" CssClass="font-color-light-black"></asp:Label>
            </div>
            <div class="clear" style="height: 20px">
            </div>
            <hr />
            <div class="clear" style="height: 20px">
            </div>
            <div class="clear-space">
            </div>
            <asp:Panel ID="pnl_entry" runat="server">
                <div class="float-left" style="margin-right: 30px">
                    <div class="float-left" style="width: 325px">
                        <h3>
                            <strong class="font-color-black">Inquire</strong> Request</h3>
                        <div class="clear-space">
                        </div>
                        Type<br />
                        <asp:DropDownList ID="ddl_Type" runat="server" CssClass="contact-update-ddl" OnSelectedIndexChanged="ddl_Type_Changed"
                            AutoPostBack="true">
                        </asp:DropDownList>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    Grade<br />
                                    <asp:DropDownList ID="ddl_Grade" runat="server" CssClass="contact-update-ddl" OnSelectedIndexChanged="ddl_Grade_Changed"
                                        AutoPostBack="true" Style="margin-right: 30px">
                                    </asp:DropDownList>
                                </td>
                                <td>
                                    Gauge<br />
                                    <asp:DropDownList ID="ddl_Gauge" runat="server" CssClass="contact-update-ddl" OnSelectedIndexChanged="ddl_Gauge_Changed"
                                        AutoPostBack="true">
                                    </asp:DropDownList>
                                    <asp:Label ID="lbl_Thickness" runat="server" CssClass="pad-left font-bold font-color-light-black"></asp:Label>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space">
                        </div>
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td valign="top">
                                    Width<br />
                                    <asp:DropDownList ID="ddl_Width" runat="server" OnSelectedIndexChanged="ddl_Width_Changed"
                                        AutoPostBack="true">
                                    </asp:DropDownList>
                                    <asp:TextBox ID="tb_WidthCustom" runat="server" CssClass="textEntryCustom" MaxLength="4"
                                        Style="display: none" />
                                </td>
                                <td valign="top">
                                    <div class="pad-left pad-right" style="font-size: 18px; padding-top: 18px">
                                        X</div>
                                </td>
                                <td valign="top">
                                    Length<br />
                                    <asp:DropDownList ID="ddl_Length" runat="server" Style="margin-right: 30px">
                                    </asp:DropDownList>
                                    <asp:TextBox ID="tb_LengthCustom" runat="server" CssClass="textEntryCustom" MaxLength="4"
                                        Style="display: none; margin-right: 37px;" />
                                </td>
                                <td valign="top">
                                    Quantity<br />
                                    <asp:TextBox ID="tb_Quantity" runat="server" CssClass="textEntryQty" /><asp:Label
                                        ID="lblWeightPerSheet" runat="server" CssClass="pad-left"></asp:Label>
                                </td>
                            </tr>
                        </table>
                        <div class="clear-space-two">
                        </div>
                        <a href="#custom" id="a_customsize" onclick="UseCustomSize();return false;" class="sb-links"
                            style="text-decoration: underline">Use Custom Size</a> <a href="#standard" id="a_standard"
                                onclick="UseStandardSize();return false;" class="sb-links" style="text-decoration: underline;
                                display: none">Use Standard Size</a>
                        <div class="clear-margin">
                            <div class="clear-space">
                            </div>
                            <asp:Label ID="lbl_ctlHint" runat="server"></asp:Label>
                            <asp:Label ID="lbl_weightPerSheet" runat="server"></asp:Label>
                            <asp:Label ID="lbl_totalWeight" runat="server"></asp:Label>
                        </div>
                    </div>
                </div>
                <div class="float-left">
                    <h3>
                        <strong class="font-color-black">Contact</strong> Information</h3>
                    <div class="clear-space">
                    </div>
                    <div class="float-left" style="width: 270px">
                        Name<br />
                        <asp:TextBox ID="tb_Name" runat="server" CssClass="textEntry" MaxLength="250" />
                        <div class="clear-space">
                        </div>
                        E-mail Address<br />
                        <asp:TextBox ID="tb_Email" runat="server" CssClass="textEntry" MaxLength="250" />
                        <div class="clear-space">
                        </div>
                        Company<br />
                        <asp:TextBox ID="tb_Company" runat="server" CssClass="textEntry" MaxLength="250" />
                    </div>
                    <div class="float-left" style="width: 250px">
                        Phone Number<br />
                        <asp:TextBox ID="tb_PNAreaCode" runat="server" CssClass="textEntryPhoneNumber margin-right"
                            MaxLength="3" Width="42px" /><span>-</span><asp:TextBox ID="tb_PN1" runat="server"
                                CssClass="textEntryPhoneNumber margin-right margin-left" MaxLength="3" Width="42px" /><span>-</span><asp:TextBox
                                    ID="tb_PN2" runat="server" CssClass="textEntryPhoneNumber margin-right margin-left"
                                    MaxLength="4" />
                        <div class="clear-space">
                        </div>
                        Questions/Comments<br />
                        <asp:TextBox ID="tb_Comment" runat="server" TextMode="MultiLine" Height="70px" Font-Names="Arial"
                            Width="215px" BorderColor="#D9D9D9" BorderWidth="1px" MaxLength="350" Style="padding: 5px" />
                    </div>
                    <div class="clear-margin">
                        <div class="clear" style="height: 20px">
                        </div>
                        <asp:Button ID="btn_Add" runat="server" CssClass="input-buttons-create contact-update float-left"
                            OnClick="btn_Add_Clicked" Text="Add Inquire" Width="115px" Style="margin-left: -12px; margin-right: 20px;" />
                        <asp:Button ID="btn_Send" runat="server" CssClass="input-buttons-create contact-update float-left"
                            OnClick="btn_Send_Clicked" Text="Submit All" Width="115px" />
                        <div class="float-right">
                            <asp:Button ID="btn_ViewCurrent" runat="server" CssClass="input-buttons contact-update"
                                OnClick="btn_View_Clicked" Text="View" Width="65px" Visible="false" />
                            <asp:Button ID="btn_Reset" runat="server" CssClass="input-buttons contact-update"
                                OnClick="btn_Reset_Clicked" Text="Reset" Width="65px" />
                        </div>
                    </div>
                    <div class="clear-margin">
                        <asp:Literal ID="Submit_Message" runat="server"></asp:Literal>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnl_view" runat="server" Visible="false">
                <asp:Button ID="btn_ViewEntry" runat="server" CssClass="input-buttons contact-update"
                    OnClick="btn_ViewEntry_Clicked" Text="Go Back" />
                <div class="clear" style="height: 20px">
                </div>
                <asp:Panel ID="pnl_entryHolder" runat="server">
                </asp:Panel>
            </asp:Panel>
            <asp:HiddenField ID="hf_SendRequest" runat="server" OnValueChanged="hf_SendRequest_Changed" />
            <asp:HiddenField ID="hf_ResetRequest" runat="server" OnValueChanged="hf_ResetRequest_Changed" />
            <asp:HiddenField ID="hf_DeleteRequest" runat="server" OnValueChanged="hf_DeleteRequest_Changed" />
        </ContentTemplate>
    </asp:UpdatePanel>
    </form>
</body>
</html>
