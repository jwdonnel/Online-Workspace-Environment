<%@ page title="Startup Scripts" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_StartupScripts, App_Web_ogqsad33" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <div class="clear-space"></div>
        <a href="#" id="aFindReplace" runat="server" onclick="openWSE.LoadModalWindow(true, 'FindReplace-element', 'Find and Replace');return false;" class="float-right input-buttons no-margin" title="Find and replace all script paths." style="position: relative; z-index: 1;"><span class="float-left img-replace margin-right-sml"></span>Find and Replace</a>
        <asp:Panel ID="pnlLinkBtns" runat="server">
        </asp:Panel>
        <div class="clear-space">
        </div>
        <div id="FindReplace-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="510">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'FindReplace-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
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
        </div>
        <div class="pad-bottom">
            Add/Update/Edit/Delete the startup scripts. Functionality will depend on the arrangment of the scripts. To edit a script, go to the File Manager page.
        </div>
        <div id="startupcss" style="display: none">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="clear-margin">
                        <asp:Panel ID="pnl_startupscripts_css" runat="server">
                        </asp:Panel>
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
