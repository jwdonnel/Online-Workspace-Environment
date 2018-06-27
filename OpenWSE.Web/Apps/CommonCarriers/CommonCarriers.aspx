<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CommonCarriers.aspx.cs" Inherits="Apps_CommonCarriers" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Common Carriers</title>
    <link type="text/css" rel="Stylesheet" href="commoncarriers.css" />
</head>
<body>
    <form id="form_steeltrucks" runat="server">
        <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="steeltrucks-load" class="main-div-app-bg">
            <div class="pad-all app-title-bg-color">
                <asp:UpdatePanel ID="UpdatePanel1_steeltrucks" runat="server">
                    <ContentTemplate>
                        <div class="float-left">
                            <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
                            <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
                        </div>
                        <div class="float-right pad-right-sml">
                            <div class="clear-space-five">
                            </div>
                            <asp:Button ID="btn_export" runat="server" Text="Export to Excel" CssClass="input-buttons"
                                OnClick="btnExportToExcel_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="clear"></div>
            </div>
            <div class="content-main">
                <table width="100%" cellspacing="0" cellpadding="0">
                    <tbody>
                        <tr>
                            <td class="td-content-inner">
                                <div class="content-overflow-app">
                                    <div class="pad-top-big margin-top">
                                        <asp:UpdatePanel ID="updatepnl_schDriver_steeltrucks" runat="server">
                                            <ContentTemplate>
                                                <div class="pad-left pad-right pad-bottom">
                                                    <asp:DropDownList ID="dd_MonthSelector" runat="server" CssClass="margin-right-sml" OnSelectedIndexChanged="dd_dateselected_Changed" AutoPostBack="true">
                                                        <asp:ListItem Value="1" Text="January"></asp:ListItem>
                                                        <asp:ListItem Value="2" Text="February"></asp:ListItem>
                                                        <asp:ListItem Value="3" Text="March"></asp:ListItem>
                                                        <asp:ListItem Value="4" Text="April"></asp:ListItem>
                                                        <asp:ListItem Value="5" Text="May"></asp:ListItem>
                                                        <asp:ListItem Value="6" Text="June"></asp:ListItem>
                                                        <asp:ListItem Value="7" Text="July"></asp:ListItem>
                                                        <asp:ListItem Value="8" Text="August"></asp:ListItem>
                                                        <asp:ListItem Value="9" Text="September"></asp:ListItem>
                                                        <asp:ListItem Value="10" Text="October"></asp:ListItem>
                                                        <asp:ListItem Value="11" Text="November"></asp:ListItem>
                                                        <asp:ListItem Value="12" Text="December"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:DropDownList ID="dd_YearSelector" runat="server" OnSelectedIndexChanged="dd_dateselected_Changed" AutoPostBack="true">
                                                        <asp:ListItem Value="2012" Text="2012"></asp:ListItem>
                                                        <asp:ListItem Value="2013" Text="2013"></asp:ListItem>
                                                        <asp:ListItem Value="2014" Text="2014"></asp:ListItem>
                                                        <asp:ListItem Value="2015" Text="2015"></asp:ListItem>
                                                        <asp:ListItem Value="2016" Text="2016"></asp:ListItem>
                                                        <asp:ListItem Value="2017" Text="2017"></asp:ListItem>
                                                        <asp:ListItem Value="2018" Text="2018"></asp:ListItem>
                                                        <asp:ListItem Value="2019" Text="2019"></asp:ListItem>
                                                        <asp:ListItem Value="2020" Text="2020"></asp:ListItem>
                                                        <asp:ListItem Value="2021" Text="2021"></asp:ListItem>
                                                        <asp:ListItem Value="2022" Text="2022"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:LinkButton ID="refresh_steeltrucks_Click" runat="server" CssClass="float-right margin-right margin-left-big margin-top-sml RandomActionBtns img-refresh"
                                                        ToolTip="Refresh Files" OnClick="btn_refresh_Click" />
                                                    <div class="float-right borderShadow2" style="padding: 4px 2px; background-color: #F9F9F9; color: #555!important">
                                                        <div class="float-right pad-left pad-right">
                                                            <b class="pad-right">TOTAL MONTHLY WEIGHT</b><asp:Label ID="lbl_smwtruckweight_steeltrucks"
                                                                runat="server" Text=""></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="clear-space">
                                                    </div>
                                                    <a href="#" class="float-left" onclick="commonCarriers.LoadGenDirEditor();return false;">Edit General Directions</a>
                                                    <a href="#" class="float-right" onclick="commonCarriers.LoadTodayTotals();return false;">View Today's Totals</a>
                                                    <div class="clear"></div>
                                                </div>
                                                <asp:HiddenField ID="hf_InsertItem" runat="server" OnValueChanged="hf_InsertItem_ValueChanged" />
                                                <asp:HiddenField ID="hf_EditItem" runat="server" OnValueChanged="hf_EditItem_ValueChanged" />
                                                <asp:HiddenField ID="hf_DeleteItem" runat="server" OnValueChanged="hf_DeleteItem_ValueChanged" />
                                                <asp:HiddenField ID="hf_UpdateItem" runat="server" OnValueChanged="hf_UpdateItem_ValueChanged" />
                                                <asp:HiddenField ID="hf_AutoCompleteList" runat="server" />
                                                <asp:HiddenField ID="hf_LoadTodayTotals" runat="server" OnValueChanged="hf_LoadTodayTotals_ValueChanged" />
                                                <asp:Panel ID="pnl_SteelTrucks" runat="server" CssClass="pad-left pad-right"></asp:Panel>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                    <div id="DirEdit-element" class="Modal-element">
                                        <div class="Modal-overlay">
                                            <div class="Modal-element-align">
                                                <div class="Modal-element-modal" data-setwidth="520">
                                                    <div class="ModalHeader">
                                                        <div>
                                                            <div class="app-head-button-holder-admin">
                                                                <a href="#" onclick="commonCarriers.LoadGenDirEditor();return false;" class="ModalExitButton"></a>
                                                            </div>
                                                            <span class="Modal-title"></span>
                                                        </div>
                                                    </div>
                                                    <div class="ModalScrollContent">
                                                        <div class="ModalPadContent">
                                                            <div id="GenDirList-steeltrucks">
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div id="DailyTotals-element" class="Modal-element">
                                        <div class="Modal-overlay">
                                            <div class="Modal-element-align">
                                                <div class="Modal-element-modal" data-setwidth="400">
                                                    <div class="ModalHeader">
                                                        <div>
                                                            <div class="app-head-button-holder-admin">
                                                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'DailyTotals-element', '');return false;" class="ModalExitButton"></a>
                                                            </div>
                                                            <span class="Modal-title"></span>
                                                        </div>
                                                    </div>
                                                    <div class="ModalScrollContent">
                                                        <div class="ModalPadContent">
                                                            <asp:UpdatePanel ID="updatepnl_ect_steeltrucks" runat="server">
                                                                <ContentTemplate>
                                                                    <asp:Literal ID="ltl_ect" runat="server"></asp:Literal>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <script type="text/javascript" src="../../Scripts/jquery/jquery.fileDownload.js"></script>
            <script type="text/javascript" src="commoncarriers.js"></script>
        </div>
    </form>
</body>
</html>
