<%@ page title="Database Importer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_dbImporter, App_Web_uowmthqy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .checkbox-new-click, .checkbox-edit-click
        {
            cursor: default;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="pad-top-big margin-top-big">
            <input type="button" class="input-buttons-create" value="Import Wizard" onclick="StartImportWizard();" />
        </div>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Imported Tables
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                    <ContentTemplate>
                        <div style="width: 100%; min-width: 1040px;">
                            <asp:LinkButton ID="refresh_Click" runat="server" CssClass="float-right img-refresh RandomActionBtns margin-right"
                                ToolTip="Refresh" OnClick="btn_refresh_Click" Style="margin-top: -20px;" />
                            <div class="clear-space">
                            </div>
                            <div class="searchwrapper" style="width: 350px;">
                                <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                                    <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                        onfocus="if(this.value=='Search Imports')this.value=''" onblur="if(this.value=='')this.value='Search Imports'"
                                        Text="Search Imports"></asp:TextBox>
                                    <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                                        OnClick="imgbtn_clearsearch_Click" />
                                    <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                        OnClick="imgbtn_search_Click" />
                                </asp:Panel>
                            </div>
                            <asp:Panel ID="pnl_ImportedTables" runat="server">
                            </asp:Panel>
                            <asp:HiddenField ID="hf_deleteimport" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_editimport_ValueChanged" />
                            <asp:HiddenField ID="hf_updateimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_updateimport_ValueChanged" />
                            <asp:HiddenField ID="hf_createAppImport" runat="server" ClientIDMode="Static" OnValueChanged="hf_createAppImport_ValueChanged" />
                            <asp:HiddenField ID="hf_editAppName" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editEditable" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editChartType" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editChartTitle" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editSelectCommand" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editOverrideColumns" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hf_editNotifyUsers" runat="server" ClientIDMode="Static" />
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="clear-space">
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                The connection strings of the imported databases
            cannot be overridden. In order to change them, you must delete the database and
            re-import it. (Deleting will not delete actual database, only the connection string.)<br />
                Click on a row to edit or delete the imported database.
            </div>
        </div>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Saved Connection Strings
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
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
                                <asp:ListItem Text="System.Data.SqlClient" Value="System.Data.SqlClient"></asp:ListItem>
                                <asp:ListItem Text="System.Data.SqlServerCe.4.0" Value="System.Data.SqlServerCe.4.0"></asp:ListItem>
                                <asp:ListItem Text="System.Data.Odbc" Value="System.Data.Odbc"></asp:ListItem>
                                <asp:ListItem Text="System.Data.OracleClient" Value="System.Data.OracleClient"></asp:ListItem>
                                <asp:ListItem Text="System.Data.OleDb" Value="System.Data.OleDb"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:Button ID="btn_addconnectionstring" runat="server" Text="Add" CssClass="input-buttons RandomActionBtns"
                                OnClick="btn_addconnectionstring_Click" />
                        </div>
                        <div class="clear-margin">
                            <div id="savedconnections_postmessage" style="margin-left: 160px;">
                            </div>
                        </div>
                        <asp:HiddenField ID="hf_deletestring" runat="server" OnValueChanged="hf_deletestring_Changed" />
                        <asp:HiddenField ID="hf_editstring" runat="server" OnValueChanged="hf_editstring_Changed" />
                        <asp:HiddenField ID="hf_updatestring" runat="server" OnValueChanged="hf_updatestring_Changed" />
                        <asp:HiddenField ID="hf_connectionNameEdit" runat="server" />
                        <asp:HiddenField ID="hf_connectionStringEdit" runat="server" />
                        <asp:HiddenField ID="hf_databaseProviderEdit" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                All connection strings are encrypted. You cannot view entire connection string such as Username and Password.
            </div>
        </div>
        <div id="password-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="490">
                        <div class='ModalHeader'>
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="CancelRequest();return false;" class="ModalExitButton"></a>
                                </div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_passwordConfirm" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_passwordConfirm" runat="server" DefaultButton="btn_passwordConfirm">
                                            <span class="password-hint">Enter the password of the user who created the import.</span>
                                            <div class="clear-space"></div>
                                            <b class="pad-right">Password</b>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="TextBoxControls"></asp:TextBox>
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons margin-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');"
                                                Style="margin-top: -2px; margin-right: 5px!important" />
                                            <input type="button" class="input-buttons" value="Cancel" onclick="CancelRequest()"
                                                style="margin-top: -2px" />
                                            <div class="clear-space"></div>
                                        </asp:Panel>
                                        <asp:HiddenField ID="hf_StartDelete" runat="server" OnValueChanged="hf_StartDelete_Changed"
                                            ClientIDMode="Static" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="UsersAllowedEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <input type="hidden" id="hf_usersAllowedToEdit_Edit" />
                                <asp:Panel ID="pnl_usersAllowedToEdit_Edit" runat="server" ClientIDMode="Static">
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="ImportWizard-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="CloseImportWizard();return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <ul class="import-steps">
                                    <li id="step1">
                                        <div class="step-container">
                                            <div class="wizard-text-hint">
                                                All imported databases automatically create an app that you will have to install for each user after importing. The Table Name will also be the name of the app created.<br />
                                                Select the options you wish to include in this Table Import.
                                            </div>
                                            <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user after import"
                                                Checked="true" />
                                            <div class="clear-space">
                                            </div>
                                            <div id="div_isPrivate">
                                                <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)" ClientIDMode="Static"
                                                    Checked="False" />
                                                <div class="clear-space">
                                                </div>
                                            </div>
                                            <asp:CheckBox ID="cb_AllowEditAdd" runat="server" Text="&nbsp;Allow this import to be editable"
                                                Checked="False" />
                                            <div class="clear-space">
                                            </div>
                                            <div id="tr-usersallowed" style="display: none;">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 155px!important;">Users Allowed To Edit
                                                        </td>
                                                        <td>
                                                            <a href="#" class="margin-left input-buttons" onclick="openWSE.LoadModalWindow(true, 'UsersAllowed-element', 'Users Allowed To Edit');return false;"><span class="img-users float-left margin-right-sml"></span>Select Users</a>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <div class="clear-space">
                                                </div>
                                            </div>
                                            <asp:CheckBox ID="cb_addChart" runat="server" Text="&nbsp;Enable Data Chart - User Google Charts to allow the data to be graphed out"
                                                ClientIDMode="Static" />
                                            <div class="clear-space">
                                            </div>
                                            <asp:CheckBox ID="cb_allowNotifi" runat="server" Text="&nbsp;Enable Notifications - Notify users upon change"
                                                ClientIDMode="Static" Checked="true" />
                                        </div>
                                    </li>
                                    <li id="step2">
                                        <div class="step-container">
                                            <div class="wizard-text-hint">Enter/select the information below. If you chose to include the Data Charts, you will need to select the chart type that will represent your data along with a title.</div>
                                            <div id="chart_selector" style="display: none;">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="settings-name-column">Chart Type
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-right-big" ClientIDMode="Static">
                                                            </asp:DropDownList>
                                                            <div class="clear-space-five"></div>
                                                            <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" class="margin-right-big" style="max-height: 50px;" />
                                                        </td>
                                                        <td>
                                                            <small>This chart image will be used as the main App Icon.<br />
                                                                Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">HERE</a> to see how the data should be setup for this chart type.</small>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <div class="clear-space"></div>
                                                <table id="tr-chart-title" cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="settings-name-column">Chart Title
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry" MaxLength="150" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <div class="clear-space"></div>
                                            </div>
                                            <table cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td class="settings-name-column">App Name
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tb_Databasename" runat="server" CssClass="textEntry"
                                                            MaxLength="150" AutoCompleteType="Search"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>
                                                        <div class="clear-space"></div>
                                                        <small>App Name does not have to reflect the [TABLENAME] in your SELECT statement.</small>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </li>
                                    <li id="step3">
                                        <div class="step-container">
                                            <div class="wizard-text-hint">Enter in the Database Connection String and select the Database Provider that you will be attempting to connect to. Before you can continue to the next step, you must test the connection.</div>
                                            <div id="div_connectionstring">
                                                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                                    <ContentTemplate>
                                                        <asp:Panel ID="pnl_enterConnectionstring" runat="server">
                                                            <asp:TextBox ID="tb_connstring" runat="server" CssClass="textEntry"
                                                                Width="98%" AutoPostBack="false" TextMode="MultiLine" Height="70px" Font-Names='"Arial"'
                                                                BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                                                ForeColor="#353535"></asp:TextBox>
                                                            <div class="clear-space">
                                                            </div>
                                                            <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://www.connectionstrings.com', this);return false;" class="float-right">Need Help?</a>
                                                            <asp:DropDownList ID="dd_provider" runat="server">
                                                                <asp:ListItem Text="System.Data.SqlClient" Value="System.Data.SqlClient"></asp:ListItem>
                                                                <asp:ListItem Text="System.Data.SqlServerCe.4.0" Value="System.Data.SqlServerCe.4.0"></asp:ListItem>
                                                                <asp:ListItem Text="System.Data.Odbc" Value="System.Data.Odbc"></asp:ListItem>
                                                                <asp:ListItem Text="System.Data.OracleClient" Value="System.Data.OracleClient"></asp:ListItem>
                                                                <asp:ListItem Text="System.Data.OleDb" Value="System.Data.OleDb"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </asp:Panel>
                                                        <asp:Panel ID="pnl_loadSavedConnection" runat="server" Enabled="false" Visible="false">
                                                        </asp:Panel>
                                                        <div class="clear-space">
                                                        </div>
                                                        <asp:HiddenField ID="hf_usestring" runat="server" OnValueChanged="hf_usestring_Changed" />
                                                        <asp:LinkButton ID="btn_test" runat="server" Text="Test Connection" CssClass="TestConnection float-right" OnClick="btn_test_Click" />
                                                        <asp:LinkButton ID="lbtn_CancelUseSavedDatasource" runat="server" CssClass="RandomActionBtns" OnClick="lbtn_CancelUseSavedDatasource_Click" Enabled="false" Visible="false">Cancel</asp:LinkButton>
                                                        <asp:LinkButton ID="lbtn_uselocaldatasource" runat="server" CssClass="RandomActionBtns margin-right-big" OnClick="lbtn_uselocaldatasource_Click">Use Local Database</asp:LinkButton>
                                                        <asp:LinkButton ID="lbtn_useSavedDatasource" runat="server" CssClass="RandomActionBtns" OnClick="lbtn_useSavedDatasource_Click">Use Saved Connection</asp:LinkButton>
                                                    </ContentTemplate>
                                                    <Triggers>
                                                        <asp:AsyncPostBackTrigger ControlID="lbtn_uselocaldatasource" />
                                                        <asp:AsyncPostBackTrigger ControlID="btn_test" />
                                                    </Triggers>
                                                </asp:UpdatePanel>
                                            </div>
                                        </div>
                                    </li>
                                    <li id="step4">
                                        <div class="step-container">
                                            <div class="wizard-text-hint">Here you will be able to customize which columns and table to import. You will also be able to rename the columns for the app (Only if in simple mode).</div>
                                            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                                                <ContentTemplate>
                                                    <table cellpadding="0" cellspacing="0">
                                                        <tr>
                                                            <td class="settings-name-column">Advance Mode
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
                                                                <div class="float-left pad-top-sml">
                                                                    <small>Advance mode allows you to manually create the SELECT COMMAND.</small>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div class="clear-space"></div>
                                                    <div class="clear-space"></div>
                                                    <asp:Panel ID="pnl_txtselect" runat="server">
                                                        <asp:TextBox ID="tb_selectcomm" runat="server" CssClass="textEntry"
                                                            Width="98%" AutoPostBack="False" TextMode="MultiLine" Height="70px" Font-Names='"Arial"'
                                                            BorderColor="#D9D9D9" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;"
                                                            ForeColor="#353535"></asp:TextBox>
                                                        <div class="clear"></div>
                                                        <a href="#" onclick="$('#<%= tb_selectcomm.ClientID %>').val('SELECT [COLUMN NAME], [COLUMN NAME], [COLUMN NAME] FROM [TABLENAME]');return false;"
                                                            class="margin-top">Need Help?</a>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnl_ddselect" runat="server" Visible="false" Enabled="false">
                                                        <span class="margin-left float-left margin-right-big">SELECT</span>
                                                        <asp:CheckBoxList ID="cb_ddselect" runat="server" CssClass="margin-left-big dbimport-cb" CellPadding="5" CellSpacing="5"
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
                                                    <asp:HiddenField ID="hf_columnOverrides" runat="server" ClientIDMode="Static" />
                                                    <asp:HiddenField ID="hf_importClick" runat="server" OnValueChanged="btn_import_Click" ClientIDMode="Static" />
                                                    <asp:HiddenField ID="hf_cancelWizard" runat="server" OnValueChanged="btn_CancelWizard_Click" ClientIDMode="Static" />
                                                </ContentTemplate>
                                                <Triggers>
                                                    <asp:AsyncPostBackTrigger ControlID="rb_adv_enabled" />
                                                    <asp:AsyncPostBackTrigger ControlID="rb_adv_disabled" />
                                                    <asp:AsyncPostBackTrigger ControlID="dd_ddtables" />
                                                </Triggers>
                                            </asp:UpdatePanel>
                                        </div>
                                    </li>
                                </ul>
                                <div class="clear"></div>
                                <asp:UpdatePanel ID="updatepnl_testLabel" runat="server">
                                    <ContentTemplate>
                                        <asp:Label ID="lbl_error" runat="server" Text="" CssClass="pad-top float-left" Enabled="False" Visible="False"></asp:Label>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear"></div>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons float-left" value="Cancel" onclick="ConfirmCancelWizard();" />
                            <input type="button" class="input-buttons prev-step float-left no-margin" value="Previous" onclick="CreatePrevStep();" style="display: none;" />
                            <input type="button" class="input-buttons next-step float-right no-margin" value="Next" onclick="CreateNextStep();" style="display: none;" />
                            <input id="btn_finishImportWizard" type="button" class="input-buttons-create RandomActionBtns no-margin" value="Import" onclick="ImportTableClick();" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="UsersAllowed-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'UsersAllowed-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
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
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/dbImport.js")%>'></script>
        <script type="text/javascript">
            $(document.body).on("change", "#<%=dd_ddtables.ClientID %>", function () {
                openWSE.LoadingMessage1("Updating...");
            });

            function UseConnectionString(x, y) {
                $("#<%=hf_usestring.ClientID %>").val("Use connection string " + y + " - '" + x + "'");
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
    </>
</asp:Content>
