<%@ Page Title="Startup Scripts" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="StartupScripts.aspx.cs" Inherits="SiteTools_StartupScripts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div id="FindReplace-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'FindReplace-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalPadContent">
                            <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_findreplace" runat="server" Enabled="false" Visible="false">
                                        <h3>Find and Replace</h3>
                                        <div class="clear-space-two"></div>
                                        <asp:TextBox ID="tb_findvalue" runat="server" CssClass="textEntry margin-right" onfocus="if(this.value=='Find value')this.value=''"
                                            onblur="if(this.value=='')this.value='Find value'" Text="Find value" Width="350px"></asp:TextBox>
                                        <div class="clear-space-five"></div>
                                        <asp:TextBox ID="tb_replacevalue" runat="server" CssClass="textEntry margin-right"
                                            onfocus="if(this.value=='Replace with value')this.value=''" onblur="if(this.value=='')this.value='Replace with value'"
                                            Text="Replace with value" Width="350px">
                                        </asp:TextBox><asp:Button ID="btn_findreplace" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Replace" OnClick="btn_findreplace_Clicked" />
                                        <div class="clear-space"></div>
                                    </asp:Panel>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="btn_findreplace" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="pad-bottom">
            <div class="float-left">
                <small>Add/Update/Edit/Delete the startup scripts. Functionality will depend on the arrangment
            of the scripts. To edit a script, go to the File Manager page.</small>
            </div>
            <div class="clear">
            </div>
            <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
            <small><b class="pad-right-sml">Note:</b>'Seq.' = Sequence of load.</small>
            <a href="#" id="aFindReplace" runat="server" onclick="openWSE.LoadModalWindow(true, 'FindReplace-element', 'Find and Replace');return false;" class="sb-links float-right" title="Find and replace all script paths."><span class="float-left img-replace margin-right-sml"></span>Find and Replace</a>
        </div>
        <div id="startupcss" style="display: none">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="clear-margin">
                        <asp:Panel ID="pnl_startupscripts_css" runat="server">
                        </asp:Panel>
                        <div class="clear-space-two">
                        </div>
                        <div id="startupscript_postmessage_css" class="float-right">
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                    <asp:HiddenField ID="hf_UpdateStartupScripts_css" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_UpdateStartupScripts_CSS_ValueChanged" />
                    <asp:HiddenField ID="hf_EditStartupScripts_css" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_EditStartupScripts_CSS_ValueChanged" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="startupjs">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="clear-margin">
                        <asp:Panel ID="pnl_startupscripts" runat="server">
                        </asp:Panel>
                        <div class="clear-space-two">
                        </div>
                        <div id="startupscript_postmessage" class="float-right">
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="clear-space">
                        </div>
                    </div>
                    <asp:HiddenField ID="hf_UpdateStartupScripts" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_UpdateStartupScripts_ValueChanged" />
                    <asp:HiddenField ID="hf_EditStartupScripts" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_EditStartupScripts_ValueChanged" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/startupscripts.js")%>'> </script>
        <script type="text/javascript">
            function ReAssignButtonSelected() {
                if ($("#startupjs").css("display") != "none") {
                    $('#MainContent_lbtn_css').removeClass('selected');
                    $('#MainContent_lbtn_js').addClass('selected');
                }
                else {
                    $('#MainContent_lbtn_js').removeClass('selected');
                    $('#MainContent_lbtn_css').addClass('selected');
                }
            }
        </script>
    </div>
</asp:Content>
