<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProdLeadTimesNoEdit.ascx.cs"
    Inherits="Apps_ProdLeadTimesNoEdit" ClientIDMode="Static" %>
<asp:UpdatePanel ID="updatepnl_pltviewer" runat="server">
    <ContentTemplate>
        <div class="pad-all app-title-bg-color">
            <div class="float-left">
                <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
            </div>
            <div align="right" class="float-right" style="font-size: 15px">
                <div class="pad-right">
                    <b>Time Updated: </b>
                    <asp:Label ID="lbl_timeupdated_pltnoedit" runat="server" Text="N/A"></asp:Label>
                    <div class="clear-space-five">
                    </div>
                    <b>Updated by: </b>
                    <asp:Label ID="lbl_updatedby_pltnoedit" runat="server" Text="N/A"></asp:Label>
                </div>
            </div>
            <div class="clear"></div>
        </div>
        <div class="clear-space">
        </div>
        <div class="float-right">
            <div class="float-right pad-right-big pad-top">
                <asp:Label ID="lbl_lastrefreshed_pltviewer" runat="server" Text=""></asp:Label>
            </div>
            <asp:Button ID="btn_update_pltviewer" runat="server" Text="Update" OnClick="btn_update_pltviewer_Clicked"
                CssClass="input-buttons float-right" />
        </div>
        <div class="clear">
        </div>
        <div id="leadtimeViewer_relative" style="position: relative">
            <div id="leadtimeViewer_absolute" style="position: absolute; left: 0; right: 0; top: 50%">
                <table border="0" cellpadding="15" cellspacing="15" style="text-align: center; width: 100%">
                    <tbody>
                        <tr>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>CTL 1 (Little)</b></h1>
                                </div>
                            </td>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>CTL 2 (Big)</b></h1>
                                </div>
                            </td>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>1/4'' Shear</b></h1>
                                </div>
                            </td>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>1/2'' Shear</b></h1>
                                </div>
                            </td>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>Laser Cutter</b></h1>
                                </div>
                            </td>
                            <td valign="middle" style="width: 20%; background-color: #6382b7;">
                                <div class="pad-all inline-block">
                                    <h1 class="font-color-white font-bold">
                                        <b>Burn Table</b></h1>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label1_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label2_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label3_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label4_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label6_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                            <td>
                                <div class="pad-all">
                                    <h1 class="font-color-light-black">
                                        <b>
                                            <asp:Label ID="Label5_pltnoedit" runat="server" Text="N/A"></asp:Label></b>
                                    </h1>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/ProdLeadTimes/prodleadviewer.js" />
