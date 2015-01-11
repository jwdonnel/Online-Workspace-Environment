<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Contact.aspx.cs" Inherits="Integrated_Pages_Contact" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Contact Us</title>
    <link id="Link1" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/site_desktop.css" />
    <link id="Link3" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .loading-frame
        {
            position: absolute;
            top: 0;
            color: #FFF;
            background: #9C2F29;
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

        .sectionTitle
        {
            width: 100%;
            color: #222;
            font-size: 18px;
            padding-bottom: 5px;
            letter-spacing: .1em;
            border-bottom: 1px solid #DDD;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <span class="sectionTitle">Inquiry / Request Form</span>
        <span class="pad-left-big">Fill out the form below</span>
        <div class="clear-space">
        </div>
        For more information about J.F. Gleeson & Associates products
    and services, please feel free to contact us through our online contact form below.
    We will do our best to make sure your questions or comments are answered.
    <div class="clear-space">
    </div>
        <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="updatepnl_newApp_deliverypickups" runat="server">
            <ContentTemplate>
                <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
                <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
                <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
                <script type="text/javascript">
                    function pageLoad() {
                        $(".loading-frame").fadeOut(300, function () {
                            $(".loading-frame").remove();
                        });
                    }

                    $(document.body).on("click", ".contact-update", function () {
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
                </script>
                <div class="clear-space">
                </div>
                <div class="float-left pad-bottom-big pad-right-big margin-right">
                    Name<br />
                    <asp:TextBox ID="tb_Name" runat="server" CssClass="textEntry" />
                    <div class="clear-space">
                    </div>
                    E-mail Address<br />
                    <asp:TextBox ID="tb_Email" runat="server" CssClass="textEntry" />
                    <div class="clear-space">
                    </div>
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
                        Width="215px" BorderColor="#D9D9D9" BorderWidth="1px" Style="padding: 5px" />
                </div>
                <div class="float-left">
                    Company<br />
                    <asp:TextBox ID="tb_Company" runat="server" CssClass="textEntry" />
                    <div class="clear-space">
                    </div>
                    Company Address<br />
                    <asp:TextBox ID="tb_Address" runat="server" CssClass="textEntry" />
                    <div class="clear-space">
                    </div>
                    City<br />
                    <asp:TextBox ID="tb_City" runat="server" CssClass="textEntry" />
                    <div class="clear-space">
                    </div>
                    <table cellpadding="0" cellspacing="0">
                        <tr>
                            <td style="width: 100px">State<br />
                                <select id="dd_State" runat="server">
                                    <option value="AL">AL</option>
                                    <option value="AK">AK</option>
                                    <option value="AZ">AZ</option>
                                    <option value="AR">AR</option>
                                    <option value="CA">CA</option>
                                    <option value="CO">CO</option>
                                    <option value="CT">CT</option>
                                    <option value="DE">DE</option>
                                    <option value="DC">DC</option>
                                    <option value="FL">FL</option>
                                    <option value="GA">GA</option>
                                    <option value="HI">HI</option>
                                    <option value="ID">ID</option>
                                    <option value="IL">IL</option>
                                    <option value="IN">IN</option>
                                    <option value="IA">IA</option>
                                    <option value="KS" selected="selected">KS</option>
                                    <option value="KY">KY</option>
                                    <option value="LA">LA</option>
                                    <option value="ME">ME</option>
                                    <option value="MD">MD</option>
                                    <option value="MA">MA</option>
                                    <option value="MI">MI</option>
                                    <option value="MN">MN</option>
                                    <option value="MS">MS</option>
                                    <option value="MO">MO</option>
                                    <option value="MT">MT</option>
                                    <option value="NE">NE</option>
                                    <option value="NV">NV</option>
                                    <option value="NH">NH</option>
                                    <option value="NJ">NJ</option>
                                    <option value="NM">NM</option>
                                    <option value="NY">NY</option>
                                    <option value="NC">NC</option>
                                    <option value="ND">ND</option>
                                    <option value="OH">OH</option>
                                    <option value="OK">OK</option>
                                    <option value="OR">OR</option>
                                    <option value="PA">PA</option>
                                    <option value="RI">RI</option>
                                    <option value="SC">SC</option>
                                    <option value="SD">SD</option>
                                    <option value="TN">TN</option>
                                    <option value="TX">TX</option>
                                    <option value="UT">UT</option>
                                    <option value="VT">VT</option>
                                    <option value="VA">VA</option>
                                    <option value="WA">WA</option>
                                    <option value="WV">WV</option>
                                    <option value="WI">WI</option>
                                    <option value="WY">WY</option>
                                </select>
                            </td>
                            <td>Postal Code<br />
                                <asp:TextBox ID="tb_PostalCode" runat="server" CssClass="textEntryPostal" />
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space">
                    </div>
                    <div class="clear-margin">
                        <asp:Button ID="btn_Send" runat="server" CssClass="input-buttons contact-update"
                            OnClick="btn_Send_Clicked" Text="Send" Width="65px" />
                        <asp:Button ID="btn_Reset" runat="server" CssClass="input-buttons contact-update"
                            OnClick="btn_Reset_Clicked" Text="Reset" Width="65px" />
                    </div>
                </div>
                <div class="clear-margin">
                    <asp:Literal ID="Submit_Message" runat="server"></asp:Literal>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
