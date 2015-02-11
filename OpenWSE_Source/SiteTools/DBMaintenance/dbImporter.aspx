<%@ Page Title="Database Importer" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="dbImporter.aspx.cs" Inherits="SiteTools_dbImporter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        #confirmPassword
        {
            float: left;
            padding-left: 10px;
            margin-top: -3px;
        }

        .checkbox-new-click, .checkbox-edit-click
        {
            cursor: default;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <small><b class="pad-right-sml">Note:</b>All imported databases automatically create a app
        that you will have to install for each user after importing. The Table Name will
        also be the name of the app created.</small>
        <div class="clear-space">
        </div>
        <asp:UpdatePanel ID="UpdatePanel8" runat="server">
            <ContentTemplate>
                <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user after import"
                    Checked="true" />
                <div class="clear-space-five">
                </div>
                <div id="div_isPrivate">
                    <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)" ClientIDMode="Static"
                        Checked="False" />
                    <div class="clear-space-five">
                    </div>
                </div>
                <asp:CheckBox ID="cb_AllowEditAdd" runat="server" Text="&nbsp;Allow edit and add for table"
                    Checked="False" />
                <div class="clear-space-five">
                </div>
                <asp:CheckBox ID="cb_addChart" runat="server" Text="&nbsp;Enable Data Chart"
                    ClientIDMode="Static" Checked="true" />
                <div class="clear-space-five">
                </div>
                <asp:CheckBox ID="cb_allowNotifi" runat="server" Text="&nbsp;Enable Notifications - Notify users upon change"
                    ClientIDMode="Static" Checked="true" />
                <div class="clear-space">
                </div>
                <div id="chart_selector">
                    <table cellpadding="10" cellspacing="10">
                        <tr>
                            <td align="right" style="width: 127px;">
                                <span class="pad-right font-bold">Chart Type</span>

                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-left margin-right" ClientIDMode="Static">
                                </asp:DropDownList>
                            </td>
                            <td>
                                <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" class="margin-left margin-right" style="max-height: 80px;" />

                            </td>
                            <td>
                                <small>This chart image will be used as the main App Icon.<br />
                                    Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">HERE</a> to see how the data should be setup for this chart type.
                                </small>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space-two"></div>
                    <table id="tr-chart-title" cellpadding="10" cellspacing="10">
                        <tr>
                            <td align="right" style="width: 127px;">
                                <span class="pad-right font-bold">Chart Title</span>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry margin-left float-left"
                                    MaxLength="250" Width="400px" ClientIDMode="Static"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                    <div class="clear-space-five">
                    </div>
                </div>
                <table cellpadding="10" cellspacing="10" width="900px">
                    <tr>
                        <td align="right" style="width: 127px;">
                            <span class="pad-right font-bold">Advance Mode</span>
                        </td>
                        <td>
                            <div class="margin-left margin-right-big float-left">
                                <div class="field switch">
                                    <asp:RadioButton ID="rb_adv_enabled" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_adv_enabled_Checked" AutoPostBack="true" />
                                    <asp:RadioButton ID="rb_adv_disabled" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_adv_disabled_Checked" AutoPostBack="true" />
                                </div>
                            </div>
                            <div class="float-left">
                                <small>Advance mode allows you to<br />
                                    manually create the SELECT COMMAND.</small>
                            </div>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rb_adv_enabled" />
                <asp:AsyncPostBackTrigger ControlID="rb_adv_disabled" />
            </Triggers>
        </asp:UpdatePanel>
        <div class="clear-space">
        </div>
        <table cellpadding="10" cellspacing="10">
            <tr>
                <td align="right" style="width: 127px;">
                    <span class="pad-right font-bold">App Name</span>
                </td>
                <td>
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <asp:TextBox ID="tb_Databasename" runat="server" CssClass="textEntry margin-left float-left"
                                MaxLength="150" AutoCompleteType="Search"></asp:TextBox>
                            <div class="float-left pad-left-big" style="width: 350px;">
                                <small>App Name does not have to reflect the [TABLENAME] in your SELECT statement.</small>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>
            <tr id="tr-usersallowed" style="display: none;">
                <td align="right" style="width: 127px;">
                    <span class="pad-right font-bold">Users Allowed&nbsp;&nbsp;<br />
                        To Edit
                    </span>
                </td>
                <td>
                    <a href="#" class="margin-left sb-links" onclick="openWSE.LoadModalWindow(true, 'UsersAllowed-element', 'Users Allowed To Edit');return false;"><span class="img-users float-left margin-right-sml"></span>Select Users</a>
                    <div id="UsersAllowed-element" class="Modal-element">
                        <div class="Modal-overlay">
                            <div class="Modal-element-align">
                                <div class="Modal-element-modal" style="width: 650px;">
                                    <div class="ModalHeader">
                                        <div>
                                            <div class="app-head-button-holder-admin">
                                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'UsersAllowed-element', '');return false;"
                                                    class="ModalExitButton"></a>
                                            </div>
                                            <span class="Modal-title"></span>
                                        </div>
                                    </div>
                                    <div class="ModalPadContent">
                                        <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                        </asp:Panel>
                                        <div align="right">
                                            <input type="button" class="input-buttons no-margin" value="Done" onclick="openWSE.LoadModalWindow(false, 'UsersAllowed-element', '');" />
                                        </div>
                                        <asp:HiddenField ID="hf_usersAllowedToEdit" runat="server" ClientIDMode="Static" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 127px;">
                    <span id="lbl_connectionstring" class="pad-right font-bold">Connection String</span>
                </td>
                <td>
                    <div id="div_connectionstring">
                        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tb_connstring" runat="server" CssClass="textEntry margin-left float-left"
                                    Width="600px" AutoPostBack="false" TextMode="MultiLine" Height="70px" Font-Names='"Arial"'
                                    BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                    ForeColor="#353535"></asp:TextBox>
                                <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://www.connectionstrings.com', this);return false;"
                                    class="margin-left margin-top-sml margin-right float-left pad-all-sml">Need Help?</a>
                                <asp:LinkButton ID="lbtn_uselocaldatasource" runat="server" CssClass="sb-links margin-left margin-top-sml float-left RandomActionBtns"
                                    OnClick="lbtn_uselocaldatasource_Click">Use Local Database</asp:LinkButton>
                                <div class="clear-space-two">
                                </div>
                                <asp:Panel ID="pnl_lbl2" runat="server">
                                    <span class="margin-left"><small>To load SELECT COMMAND controls, click
                                    <asp:LinkButton ID="lbtn_loadselectcommand" runat="server" CssClass="RandomActionBtns"
                                        OnClick="lbtn_loadselectcommand_Click">here</asp:LinkButton></small></span>
                                </asp:Panel>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="lbtn_uselocaldatasource" />
                                <asp:AsyncPostBackTrigger ControlID="lbtn_loadselectcommand" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 127px;">
                    <asp:UpdatePanel ID="UpdatePanel9" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="pnl_lbl" runat="server">
                                <span class="pad-right font-bold">Select Command</span>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td>
                    <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="pnl_txtselect" runat="server">
                                <asp:TextBox ID="tb_selectcomm" runat="server" CssClass="textEntry margin-left float-left"
                                    Width="600px" AutoPostBack="False" TextMode="MultiLine" Height="70px" Font-Names='"Arial"'
                                    BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                    ForeColor="#353535"></asp:TextBox>
                                <a href="#" onclick="$('#<%= tb_selectcomm.ClientID %>').val('SELECT [COLUMN NAME], [COLUMN NAME], [COLUMN NAME] FROM [TABLENAME]');return false;"
                                    class="margin-left margin-top-sml float-left pad-all-sml">Need Help?</a>
                            </asp:Panel>
                            <asp:Panel ID="pnl_ddselect" runat="server" Visible="false" Enabled="false">
                                <span class="margin-left float-left margin-right-big">SELECT</span>
                                <asp:CheckBoxList ID="cb_ddselect" runat="server" CssClass="margin-left-big dbimport-cb"
                                    RepeatDirection="Vertical" RepeatColumns="3" CellPadding="5" CellSpacing="5"
                                    AutoPostBack="true" OnSelectedIndexChanged="cb_ddselect_Changed">
                                </asp:CheckBoxList>
                                <div class="clear-space">
                                </div>
                                <div class="float-left">
                                    <span class="margin-left">FROM</span><asp:DropDownList ID="dd_ddtables" CssClass="margin-left margin-right"
                                        OnSelectedIndexChanged="dd_ddtables_Changed" runat="server" AutoPostBack="true">
                                    </asp:DropDownList>
                                </div>
                                <div class="float-left">
                                    <asp:TextBox ID="tb_conditional" runat="server" CssClass="textEntry margin-left"
                                        Text="[CONDITIONAL STATEMENT]" Width="300px" onfocus="if(this.value=='[CONDITIONAL STATEMENT]')this.value=''"
                                        onblur="if(this.value=='')this.value='[CONDITIONAL STATEMENT]'"></asp:TextBox>
                                    <div class="clear-space-two">
                                    </div>
                                    <div class="margin-left-big">
                                        <small>Leave blank if no condition.</small>
                                    </div>
                                </div>
                                <div class="clear-space">
                                </div>
                                <span class="margin-left">ORDER BY</span>
                                <asp:DropDownList ID="dd_orderby" CssClass="margin-left" runat="server">
                                </asp:DropDownList>
                                <asp:DropDownList ID="dd_orderdirection" CssClass="margin-left" runat="server">
                                    <asp:ListItem Text="ASC" Value="ASC"></asp:ListItem>
                                    <asp:ListItem Text="DESC" Value="DESC"></asp:ListItem>
                                </asp:DropDownList>
                                <div class="clear-space-five">
                                </div>
                                <div class="clear-space">
                                </div>
                            </asp:Panel>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="dd_ddtables" />
                        </Triggers>
                    </asp:UpdatePanel>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 127px;">
                    <span class="pad-right font-bold">Database Provider</span>
                </td>
                <td>
                    <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                        <ContentTemplate>
                            <asp:DropDownList ID="dd_provider" CssClass="margin-left" runat="server" Width="200px">
                                <asp:ListItem Text="System.Data.SqlClient" Value="System.Data.SqlClient"></asp:ListItem>
                                <asp:ListItem Text="System.Data.SqlServerCe.4.0" Value="System.Data.SqlServerCe.4.0"></asp:ListItem>
                                <asp:ListItem Text="System.Data.Odbc" Value="System.Data.Odbc"></asp:ListItem>
                                <asp:ListItem Text="System.Data.OracleClient" Value="System.Data.OracleClient"></asp:ListItem>
                                <asp:ListItem Text="System.Data.OleDb" Value="System.Data.OleDb"></asp:ListItem>
                            </asp:DropDownList>
                            <div class="float-right" style="padding-right: 38px">
                                <asp:LinkButton ID="btn_test" runat="server" Text="Test Connection" CssClass="sb-links TestConnection"
                                    OnClick="btn_test_Click" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 127px;">
                    <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                        <ContentTemplate>
                            <asp:LinkButton ID="btn_clear" runat="server" CssClass="sb-links RandomActionBtns margin-right"
                                OnClick="btn_clear_Click">Clear Entries</asp:LinkButton>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td>
                    <asp:UpdatePanel ID="UpdatePanel7" runat="server">
                        <ContentTemplate>
                            <asp:Label ID="lbl_error" runat="server" Text="" CssClass="float-left pad-top margin-left"
                                Enabled="False" Visible="False" Style="max-width: 610px;"></asp:Label>
                            <div class="float-right" style="padding-right: 38px">
                                <asp:Button ID="btn_import" runat="server" Text="Import Database" CssClass="input-buttons RandomActionBtns"
                                    OnClick="btn_import_Click" ClientIDMode="Static" Style="margin-right: 0 !important;" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>
        </table>
        <div class="clear-space">
        </div>
        <div class="actions no-pad-left">
            <div class="clear" style="height: 30px;">
            </div>
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Saved Connection Strings</h3>
            </div>
            <div class="clear-space">
            </div>
            <small><b class="pad-right-sml">Note:</b>All connection strings are encrypted. You cannot
            view entire connection string such as Username and Password.</small>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="UpdatePanel10" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnl_savedconnections_holder" runat="server">
                    </asp:Panel>
                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <span class="pad-right font-bold font-color-black">New Connection String</span>
                    <asp:TextBox ID="tb_connectionname" runat="server" CssClass="textEntry" Text="[CONNECTION NAME]"
                        Width="200px" onfocus="if(this.value=='[CONNECTION NAME]')this.value=''" onblur="if(this.value=='')this.value='[CONNECTION NAME]'"
                        MaxLength="50"></asp:TextBox>
                    <asp:TextBox ID="tb_connectionstring" runat="server" CssClass="textEntry margin-left-big margin-right-big"
                        Text="[CONNECTION STRING]" Width="500px" onfocus="if(this.value=='[CONNECTION STRING]')this.value=''"
                        onblur="if(this.value=='')this.value='[CONNECTION STRING]'"></asp:TextBox>
                    <div class="clear-space">
                    </div>
                    <div class="float-left" style="padding-left: 160px">
                        <asp:DropDownList ID="dd_provider_connectionstring" CssClass="margin-right-big" runat="server"
                            Width="200px">
                            <asp:ListItem Text="System.Data.SqlServerCe.4.0" Value="System.Data.SqlServerCe.4.0"></asp:ListItem>
                            <asp:ListItem Text="System.Data.SqlClient" Value="System.Data.SqlClient"></asp:ListItem>
                            <asp:ListItem Text="System.Data.Odbc" Value="System.Data.Odbc"></asp:ListItem>
                            <asp:ListItem Text="System.Data.OracleClient" Value="System.Data.OracleClient"></asp:ListItem>
                            <asp:ListItem Text="System.Data.OleDb" Value="System.Data.OleDb"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:Button ID="btn_addconnectionstring" runat="server" Text="Add" CssClass="input-buttons"
                            OnClick="btn_addconnectionstring_Click" />
                    </div>
                    <div class="clear-margin">
                        <div id="savedconnections_postmessage" style="margin-left: 160px;">
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="actions no-pad-left" style="margin-top: 0 !important;">
            <div class="clear" style="height: 30px;">
            </div>
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Current Imported Databases</h3>
            </div>
            <div class="clear-space">
            </div>
            <small><b class="pad-right-sml">Note:</b>The connection strings of the imported databases
            cannot be overridden. In order to change them, you must delete the database and
            re-import it. (Deleting will not delete actual database, only the connection string.)<br />
                Click on a row to edit or delete the imported database.</small>
            <div class="clear" style="height: 5px;">
            </div>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <asp:Label ID="lbl_error2" runat="server" Text="" CssClass="clear" Enabled="False"
                        Visible="False"></asp:Label>
                    <div style="width: 100%; min-width: 1040px;">
                        <small>
                            <asp:LinkButton ID="lbtn_selectAll" runat="server" CssClass="sb-links RandomActionBtns"
                                OnClick="lbtn_selectAll_Click">Select All</asp:LinkButton></small>
                        <asp:LinkButton ID="refresh_Click" runat="server" CssClass="float-right img-refresh RandomActionBtns"
                            ToolTip="Refresh" OnClick="btn_refresh_Click" />
                        <div class="clear-space">
                        </div>
                        <asp:HiddenField ID="HiddenField1" runat="server" />
                        <asp:GridView ID="GV_Imports" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                            GridLines="None" OnRowCommand="GV_Imports_RowCommand" OnRowEditing="GV_Imports_RowEdit"
                            OnRowDataBound="GV_Imports_RowDataBound" OnRowCancelingEdit="GV_Imports_CancelEdit"
                            OnRowUpdating="GV_Imports_RowUpdate" AllowPaging="False" ShowHeaderWhenEmpty="True"
                            OnRowDeleting="GV_Imports_RowDelete" Width="100%">
                            <EmptyDataRowStyle ForeColor="Black" />
                            <RowStyle CssClass="GridNormalRow" />
                            <AlternatingRowStyle CssClass="GridAlternate" />
                            <EmptyDataTemplate>
                                <div class="emptyGridView">
                                    No Imported Databases
                                </div>
                            </EmptyDataTemplate>
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                            <tr>
                                                <td width="20px" align="center">
                                                    <asp:LinkButton ID="imgbtn_del" runat="server" ToolTip="Delete selected items" OnClientClick="OnDelete();"
                                                        CssClass="td-delete-light-btn"></asp:LinkButton>
                                                </td>
                                                <td style="width: 142px;">Date Imported
                                                </td>
                                                <td style="width: 125px;">App Name
                                                </td>
                                                <td style="width: 85px;">Chart Type
                                                </td>
                                                <td style="width: 150px;">Chart Title
                                                </td>
                                                <td style="min-width: 250px;">Select Command
                                                </td>
                                                <td style="width: 155px;">Database Provider
                                                </td>
                                                <td style="width: 60px;">Notify
                                                </td>
                                                <td style="width: 125px;">Imported By
                                                </td>
                                                <td style="width: 75px;">Actions
                                                </td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Panel ID="pnl_FileMain" class="panelcontainer" runat="server">
                                            <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                                <tbody>
                                                    <td width="22px" align="center" class="GridViewNumRow">
                                                        <asp:CheckBox ID="CheckBoxIndv" runat="server" OnCheckedChanged="CheckBoxIndv_CheckChanged"
                                                            Text='<%#Eval("ID") %>' CssClass="HiddenText" AutoPostBack="true" />
                                                    </td>
                                                    <td class="border-right" style="width: 141px;">
                                                        <%#Eval("date") %>
                                                    </td>
                                                    <td class="border-right" style="width: 125px;">
                                                        <%#Eval("tablename") %>
                                                        <asp:HiddenField ID="hf_appID" runat="server" Value='<%#Eval("ID") %>' />
                                                    </td>
                                                    <td class="border-right" style="width: 85px;">
                                                        <%#Eval("chartType") %>
                                                    </td>
                                                    <td class="border-right" style="width: 200px;">
                                                        <%#Eval("chartTitle") %>
                                                    </td>
                                                    <td class="border-right" style="min-width: 252px;">
                                                        <%#Eval("selectcomm") %>
                                                        <div class="clear-margin">
                                                            <b class="pad-right">Allow Edit/Add:</b><%#Eval("AllowEdit") %>
                                                        </div>
                                                    </td>
                                                    <td class="border-right" style="width: 155px;">
                                                        <%#Eval("provider") %>
                                                    </td>
                                                    <td class="border-right" align="center" style="width: 60px;">
                                                        <%#Eval("notify") %>
                                                    </td>
                                                    <td class="border-right" style="width: 125px;">
                                                        <%#Eval("importedby") %>
                                                        <asp:HiddenField ID="hf_importedby" runat="server" Value='<%#Eval("importedby_username") %>' />
                                                    </td>
                                                    <td class="border-right" align="center" style="width: 75px;">
                                                        <asp:LinkButton ID="LinkButton1" CssClass="td-edit-btn RandomActionBtns margin-right"
                                                            runat="server" CommandName="Edit" ToolTip="Edit"></asp:LinkButton>
                                                        <asp:LinkButton ID="LinkButton3" CssClass="td-delete-btn RandomActionBtns" runat="server"
                                                            CommandName="Delete" CommandArgument='<%#Eval("ID") %>' ToolTip="Delete"></asp:LinkButton>
                                                        <asp:LinkButton ID="LinkButton2" CssClass="sb-links RandomActionBtns" runat="server"
                                                            ToolTip="Create App">Create</asp:LinkButton>
                                                    </td>
                                                </tbody>
                                            </table>
                                        </asp:Panel>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:HiddenField ID="hf_editID" runat="server" Value='<%#Eval("ID") %>' />
                                        <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                            <tbody>
                                                <td width="22px" align="center" class="GridViewNumRow">
                                                    <asp:CheckBox ID="CheckBoxIndv" runat="server" OnCheckedChanged="CheckBoxIndv_CheckChanged"
                                                        Text='<%#Eval("ID") %>' CssClass="HiddenText" AutoPostBack="true" />
                                                </td>
                                                <td class="border-right" style="width: 141px;">
                                                    <%#Eval("date") %>
                                                </td>
                                                <td class="border-right" style="width: 125px;">
                                                    <asp:TextBox CssClass="textEntry" runat="server" ID="tb_editName" Text='<%#Eval("tablename") %>'
                                                        Width="98%" MaxLength="150"></asp:TextBox>
                                                </td>
                                                <td class="border-right" style="width: 85px;">
                                                    <asp:DropDownList ID="ddl_chartType_edit" runat="server"></asp:DropDownList>
                                                </td>
                                                <td class="border-right" style="width: 200px;">
                                                    <asp:TextBox CssClass="textEntry" runat="server" ID="tb_chartTitle_edit" Text='<%#Eval("chartTitle") %>'
                                                        Width="98%" MaxLength="250"></asp:TextBox>
                                                </td>
                                                <td class="border-right" style="min-width: 252px;">
                                                    <asp:TextBox CssClass="textEntry pad-all-sml" runat="server" ID="tb_editCommand"
                                                        Text='<%#Eval("selectcomm") %>' Width="98%" Font-Names="Arial" Height="65px"
                                                        TextMode="MultiLine"></asp:TextBox>
                                                    <div class="clear-margin">
                                                        <asp:CheckBox ID="cb_AllowEditAdd_edit" runat="server" Text="&nbsp;Allow edit and add for table" CssClass="cb-allowedit-edit" />
                                                        <div class="clear-space-two"></div>
                                                        <a href="#" class="sb-links edit-usersallowed-btn" onclick="EditUsersAllowedToEditEdit('<%#Eval("ID") %>');return false;"><span class="img-users float-left margin-right-sml"></span>Select Users Allowed To Edit</a>
                                                    </div>
                                                </td>
                                                <td class="border-right" style="width: 155px;">
                                                    <%#Eval("provider") %>
                                                </td>
                                                <td class="border-right" align="center" style="width: 60px;">
                                                    <asp:CheckBox ID="cb_NotifyUsers_Edit" runat="server" ToolTip="Notify users upon table changes" />
                                                </td>
                                                <td class="border-right" style="width: 125px;">
                                                    <%#Eval("importedby") %>
                                                </td>
                                                <td class="border-right" align="center" style="width: 75px;">
                                                    <asp:LinkButton ID="lbtn_update" CssClass="margin-right td-update-btn RandomActionBtns"
                                                        runat="server" CausesValidation="True" CommandName="Update" ToolTip="Update"></asp:LinkButton>
                                                    <asp:LinkButton ID="lbtn_cancel" CssClass="td-cancel-btn RandomActionBtns" runat="server"
                                                        CausesValidation="False" CommandName="Cancel" ToolTip="Cancel"></asp:LinkButton>
                                                </td>
                                            </tbody>
                                        </table>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:HiddenField ID="hf_usestring" runat="server" OnValueChanged="hf_usestring_Changed" />
                    <asp:HiddenField ID="hf_deletestring" runat="server" OnValueChanged="hf_deletestring_Changed" />
                    <asp:HiddenField ID="hf_editstring" runat="server" OnValueChanged="hf_editstring_Changed" />
                    <asp:HiddenField ID="hf_updatestring" runat="server" OnValueChanged="hf_updatestring_Changed" />
                    <asp:HiddenField ID="hf_connectionNameEdit" runat="server" />
                    <asp:HiddenField ID="hf_connectionStringEdit" runat="server" />
                    <asp:HiddenField ID="hf_databaseProviderEdit" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="db_overlay" class="Modal-overlay" style="display: none;">
            <div class="password-element-align-db">
                <div id="db_modal" class="password-element-modal-db" style="display: none; min-height: 57px!important;">
                    <div id="confirmPassword">
                        <asp:UpdatePanel ID="updatepnl_passwordConfirm" runat="server">
                            <ContentTemplate>
                                <asp:Panel ID="pnl_passwordConfirm" runat="server" DefaultButton="btn_passwordConfirm">
                                    <b class="margin-right">Password:</b>
                                    <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="TextBoxControls"></asp:TextBox>
                                    <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons margin-left"
                                        Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');"
                                        Style="margin-top: -2px; margin-right: 5px!important" />
                                    <input type="button" class="input-buttons" value="Cancel" onclick="CancelRequest()"
                                        style="margin-top: -2px" />
                                </asp:Panel>
                                <asp:HiddenField ID="hf_StartDelete" runat="server" OnValueChanged="hf_StartDelete_Changed"
                                    ClientIDMode="Static" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div class="clear-space-five"></div>
                        <div class="float-left">
                            <small><b class="pad-right-sml">Note:</b>Enter in Created By user password.</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="UsersAllowedEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" style="width: 650px;">
                        <div class="ModalHeader">
                            <div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalPadContent">
                            <input type="hidden" id="hf_usersAllowedToEdit_Edit" />
                            <asp:Panel ID="pnl_usersAllowedToEdit_Edit" runat="server" ClientIDMode="Static">
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/dbImport.js")%>'></script>
        <script type="text/javascript">
            $(document.body).on("change", "#<%=dd_ddtables.ClientID %>", function () {
                openWSE.LoadingMessage1("Updating...");
            });

            function UseConnectionString(x, y) {
                $("#<%=tb_connstring.ClientID %>").html("Use connection string " + y + " - '" + x + "'");
                $("#<%=hf_usestring.ClientID %>").val(new Date().toString());
                openWSE.LoadingMessage1("Updating...");
                __doPostBack("<%=hf_usestring.ClientID %>", "");
            }

            function DeleteConnectionString(x) {
                openWSE.ConfirmWindow("Are you sure you want to delete this connection string?",
                   function () {
                       $("#<%=hf_deletestring.ClientID %>").val(x);
                       openWSE.LoadingMessage1("Updating...");
                       __doPostBack("<%=hf_deletestring.ClientID %>", "");
                   }, null);
               }

               function EditConnectionString(x) {
                   $("#<%=hf_editstring.ClientID %>").val(x);
                   openWSE.LoadingMessage1("Loading...");
                   __doPostBack("<%=hf_editstring.ClientID %>", "");
               }

               function UpdateConnectionString(x) {
                   $("#<%=hf_updatestring.ClientID %>").val(x);
                   $("#<%=hf_connectionNameEdit.ClientID %>").val(escape($("#tb_connNameedit").val()));
                   $("#<%=hf_connectionStringEdit.ClientID %>").val(escape($("#tb_connStringedit").val()));
                   $("#<%=hf_databaseProviderEdit.ClientID %>").val(escape($("#edit-databaseProvider").val()));
                   openWSE.LoadingMessage1("Updating...");
                   __doPostBack("<%=hf_updatestring.ClientID %>", "");
               }

               function KeyPressEdit_Connection(event, x) {
                   try {
                       if (event.which == 13) {
                           event.preventDefault();
                           UpdateConnectionString(x);
                       }
                   }
                   catch (evt) {
                       if (event.keyCode == 13) {
                           event.preventDefault();
                           UpdateConnectionString(x);
                       }
                       delete evt;
                   }
               }
        </script>
    </div>
</asp:Content>
