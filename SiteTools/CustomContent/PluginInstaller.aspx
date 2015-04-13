<%@ page title="Plugin Installer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_CustomContent_PluginInstaller, App_Web_tiq55j4e" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="table-settings-box">
            <div class="td-settings-title">
                <div id="searchwrapper" style="width: 300px;">
                    <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Plugins')this.value=''" onblur="if(this.value=='')this.value='Search Plugins'"
                            Text="Search Plugins"></asp:TextBox>
                        <a href="#" onclick="$('#MainContent_tb_search').val('Search Plugins');return false;"
                            class="searchbox_clear" title="Clear search"></a>
                        <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                            OnClick="imgbtn_search_Click" />
                    </asp:Panel>
                </div>
                <div class="clear-space">
                </div>
                <asp:UpdatePanel ID="updatePnl_PluginTotals" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Label ID="lbl_PluginsAvailable" runat="server" ClientIDMode="Static"></asp:Label>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatePnl_PluginList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_PluginList" runat="server" ClientIDMode="Static"></asp:Panel>
                        <div class="clear-space"></div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Select an plugin above to install/uninstall
            </div>
        </div>
        <asp:UpdatePanel ID="updatePnl_Postbacks" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_UninstallPlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_UninstallPlugin_ValueChanged" />
                <asp:HiddenField ID="hf_InstallPlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_InstallPlugin_ValueChanged" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <script type="text/javascript">
        function UninstallPlugin(id) {
            openWSE.LoadingMessage1("Uninstalling...");
            $("#hf_UninstallPlugin").val(id);
            __doPostBack("hf_UninstallPlugin", "");
        }

        function InstallPlugin(id) {
            openWSE.LoadingMessage1("Installing...");
            $("#hf_InstallPlugin").val(id);
            __doPostBack("hf_InstallPlugin", "");
        }
    </script>
</asp:Content>
