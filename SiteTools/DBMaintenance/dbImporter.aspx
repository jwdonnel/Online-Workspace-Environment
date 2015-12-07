<%@ page title="Database Importer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_dbImporter, App_Web_h3zobxng" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .checkbox-new-click, .checkbox-edit-click
        {
            cursor: default;
        }

        #MainContent_cb_ddselect td
        {
            padding: 5px 0;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="clear-space"></div>
        <div class="clear-space-two"></div>
        <ul class="sitemenu-selection">
        </ul>
        <div class="clear-space"></div>
        <asp:Panel ID="pnl_TableEntries" runat="server" CssClass="pnl-section margin-top" data-title="Imported Tables">
            <div class="pad-top-big">
                <a href="#" class="input-buttons-create float-left margin-right-big" onclick="StartImportWizard();return false;">Import Wizard</a>
                <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
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
            </div>
            <div class="clear-space">
            </div>
            <div class="table-settings-box no-border" style="margin-top: 0">
                <div class="td-settings-ctrl">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <div style="width: 100%; min-width: 1040px;">
                                <asp:Panel ID="pnl_ImportedTables" runat="server">
                                </asp:Panel>
                                <asp:HiddenField ID="hf_deleteimport" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_editimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_editimport_ValueChanged" />
                                <asp:HiddenField ID="hf_updateimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_updateimport_ValueChanged" />
                                <asp:HiddenField ID="hf_createAppImport" runat="server" ClientIDMode="Static" OnValueChanged="hf_createAppImport_ValueChanged" />
                            </div>
                            <div class="clear-space">
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                            <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
                <div class="td-settings-desc">
                    The connection strings of the imported databases
            cannot be overridden. In order to change them, you must delete the database and
            re-import it. (Deleting will not delete actual database, only the connection string.)<br />
                    Click on a row to edit or delete the imported database.
                </div>
            </div>
            <div id="ImportWizard-element" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="750">
                            <div class="ModalHeader">
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#close" onclick="ConfirmCancelWizard();return false;" class="ModalExitButton"></a>
                                    </div>
                                    <span class="Modal-title"></span>
                                </div>
                            </div>
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    <ul class="import-steps">
                                        <li id="step1">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">Give your new table a name and description. This name will be applied for both the App and the table name in the import list. Since these tables have a unique id that the user cannot see, you can name this table anything. The app name can be change at any time by going to the App Management->App Manager. App Name does not have to reflect the [TABLENAME] in your SELECT statement.</div>
                                                <asp:TextBox ID="tb_Databasename" runat="server" CssClass="textEntry margin-right"
                                                    MaxLength="150" AutoCompleteType="Search" placeholder="App Name"></asp:TextBox>
                                                <div class="clear-space"></div>
                                                <asp:TextBox ID="tb_description" runat="server" CssClass="textEntry" Width="98%" AutoCompleteType="Search" ClientIDMode="Static" placeholder="Description"></asp:TextBox>
                                            </div>
                                        </li>
                                        <li id="step2">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">
                                                    All imported databases automatically create an app that you will have to install for each user after importing.<br />
                                                    Select the options you wish to include in this Table Import.
                                                </div>
                                                <div id="div_installAfterLoad">
                                                    <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user after import"
                                                        Checked="true" />
                                                    <div class="clear-space">
                                                    </div>
                                                </div>
                                                <div id="div_isPrivate">
                                                    <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)" ClientIDMode="Static"
                                                        Checked="False" />
                                                    <div class="clear-space">
                                                    </div>
                                                </div>
                                                <asp:CheckBox ID="cb_allowNotifi" runat="server" Text="&nbsp;Enable Notifications - Notify users upon change"
                                                    ClientIDMode="Static" Checked="true" />
                                                <div class="clear-space"></div>
                                                <asp:CheckBox ID="cb_AllowEditAdd" runat="server" Text="&nbsp;Allow this import to be editable"
                                                    Checked="False" />
                                                <div class="clear-space">
                                                </div>
                                                <div id="tr-usersallowed" style="display: none;">
                                                    <table cellpadding="0" cellspacing="0">
                                                        <tr id="tr1">
                                                            <td class="settings-name-column" valign="top" style="width: 150px!important;">
                                                                <div class="pad-top-big">Users Allowed To Edit</div>
                                                            </td>
                                                            <td>
                                                                <asp:UpdatePanel ID="updatePnl_UsersAllowedToEdit" runat="server" UpdateMode="Conditional">
                                                                    <ContentTemplate>
                                                                        <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                                                        </asp:Panel>
                                                                    </ContentTemplate>
                                                                </asp:UpdatePanel>
                                                                <asp:HiddenField ID="hf_usersAllowedToEdit" runat="server" ClientIDMode="Static" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div class="clear">
                                                    </div>
                                                </div>
                                            </div>
                                        </li>
                                        <li id="step3">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">
                                                    You can customize the look of your table by adjusting the controls below.
                                                </div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Table Header Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_tableheadercolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultheadercolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultheadercolor', 'tb_tableheadercolor');" /></td>
                                                        <td><small>Set the color of your header's background. The forecolor will be adjusted to a black/white depending on the color chosen.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Primary Row Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_primaryrowcolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultprimarycolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultprimarycolor', 'tb_primaryrowcolor');" /></td>
                                                        <td><small>Set your primary row's color that will alternate every other row starting from the first row.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Alternative Row Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_alternativerowcolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultalternativecolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultalternativecolor', 'tb_alternativerowcolor');" /></td>
                                                        <td><small>Set your alternative row's color that will alternate every other row starting from the second row.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Default Font Family
                                                        </td>
                                                        <td>
                                                            <select id="dd_fontfamilycustomization" onchange="fontFamilyChange();">
                                                                <option value="">Use Default</option>
                                                                <option value="Georgia, serif">Georgia, serif</option>
                                                                <option value="'Palatino Linotype', 'Book Antiqua', Palatino, serif">"Palatino Linotype", "Book Antiqua", Palatino, serif</option>
                                                                <option value="'Times New Roman', Times, serif">"Times New Roman", Times, serif</option>
                                                                <option value="Arial, Helvetica, sans-serif">Arial, Helvetica, sans-serif</option>
                                                                <option value="'Arial Black', Gadget, sans-serif">"Arial Black", Gadget, sans-serif</option>
                                                                <option value="'Comic Sans MS', cursive, sans-serif">"Comic Sans MS", cursive, sans-serif</option>
                                                                <option value="Impact, Charcoal, sans-serif">Impact, Charcoal, sans-serif</option>
                                                                <option value="'Lucida Sans Unicode', 'Lucida Grande', sans-serif">"Lucida Sans Unicode", "Lucida Grande", sans-serif</option>
                                                                <option value="Tahoma, Geneva, sans-serif">Tahoma, Geneva, sans-serif</option>
                                                                <option value="'Trebuchet MS', Helvetica, sans-serif">"Trebuchet MS", Helvetica, sans-serif</option>
                                                                <option value="Verdana, Geneva, sans-serif">Verdana, Geneva, sans-serif</option>
                                                                <option value="'Courier New', Courier, monospace">"Courier New", Courier, monospace</option>
                                                                <option value="'Lucida Console', Monaco, monospace">"Lucida Console", Monaco, monospace</option>
                                                            </select>
                                                            <div class="clear-space-five"></div>
                                                            <span id="span_fontfamilypreview" style="font-size: 15px;">This is the font preview</span>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;"></td>
                                                        <td><small>Set your table's main font family that you want to display for the entire table.</small></td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </li>
                                        <li id="step4">
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
                                                            <div class="clear-space">
                                                            </div>
                                                            <asp:Label ID="lbl_error" runat="server" Text="" CssClass="pad-top float-left" Enabled="False" Visible="False"></asp:Label>
                                                        </ContentTemplate>
                                                        <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="lbtn_uselocaldatasource" />
                                                            <asp:AsyncPostBackTrigger ControlID="btn_test" />
                                                        </Triggers>
                                                    </asp:UpdatePanel>
                                                </div>
                                            </div>
                                        </li>
                                        <li id="step5">
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
                                                        <asp:HiddenField ID="hf_customizations" runat="server" ClientIDMode="Static" />
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
                                        <li id="step6">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">You can enable the table data chart below. Select which chart type you want to show along with which columns to associate the chart with.</div>
                                                <div id="chart_selector">
                                                    <table width="100%">
                                                        <tr>
                                                            <td class="settings-name-column">Chart Type
                                                            </td>
                                                            <td>
                                                                <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-right-big" ClientIDMode="Static" onchange="ChartTypeChangeEvent();">
                                                                </asp:DropDownList>
                                                            </td>
                                                            <td id="img_charttype_holder">
                                                                <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" class="margin-right-big" style="max-height: 50px;" />
                                                            </td>
                                                            <td>
                                                                <small>This chart image will be used as the main App Icon.<br />
                                                                    Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">HERE</a> to see how the data should be setup for this chart type.</small>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div id="chart-title-holder">
                                                        <div class="clear-space"></div>
                                                        <table id="tr-chart-title">
                                                            <tr>
                                                                <td class="settings-name-column">Chart Title
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry" AutoCompleteType="Search" MaxLength="150" ClientIDMode="Static"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="chart-column-selector">
                                                        <div class="clear-space"></div>
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="settings-name-column">Data Columns
                                                                </td>
                                                                <td width="240px">
                                                                    <div id="chart-column-selector-holder"></div>
                                                                </td>
                                                                <td>
                                                                    <small>Select the columns you wish to add to use on the chart.</small>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </div>
                                                <div id="error_onupdatecreate" class="pad-all"></div>
                                            </div>
                                        </li>
                                    </ul>
                                    <div class="clear"></div>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <input type="button" class="input-buttons float-right no-margin" value="Cancel" onclick="ConfirmCancelWizard();" />
                                <input type="button" class="input-buttons prev-step float-left" value="Previous" onclick="CreatePrevStep();" style="display: none;" />
                                <input type="button" class="input-buttons next-step float-left no-margin" value="Next" onclick="CreateNextStep();" style="display: none;" />
                                <input id="btn_finishImportWizard" type="button" class="input-buttons RandomActionBtns float-left no-margin" value="Import" onclick="ImportTableClick();" />
                                <div class="clear"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_SavedConnections" runat="server" CssClass="pnl-section margin-top" data-title="Saved Connections">
            <div class="pad-top-big">
                <a href="#" class="input-buttons-create float-left margin-right-big" onclick="openWSE.LoadModalWindow(true, 'AddConnectionString-Element', 'Add New Connection');return false;">Add Connection</a>
            </div>
            <div class="clear-space"></div>
            <div class="table-settings-box no-border" style="margin-top: 0">
                <div class="td-settings-ctrl">
                    <asp:UpdatePanel ID="UpdatePanel10" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="pnl_savedconnections_holder" runat="server">
                            </asp:Panel>
                            <div class="clear-space">
                            </div>
                            <asp:HiddenField ID="hf_deletestring" runat="server" OnValueChanged="hf_deletestring_Changed" />
                            <asp:HiddenField ID="hf_editstring" runat="server" OnValueChanged="hf_editstring_Changed" />
                            <asp:HiddenField ID="hf_updatestring" runat="server" OnValueChanged="hf_updatestring_Changed" />
                            <asp:HiddenField ID="hf_connectionNameEdit" runat="server" />
                            <asp:HiddenField ID="hf_connectionStringEdit" runat="server" />
                            <asp:HiddenField ID="hf_databaseProviderEdit" runat="server" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div id="AddConnectionString-Element" class="Modal-element">
                        <div class="Modal-overlay">
                            <div class="Modal-element-align">
                                <div class="Modal-element-modal" data-setwidth="550">
                                    <div class='ModalHeader'>
                                        <div>
                                            <div class="app-head-button-holder-admin">
                                                <a href="#close" onclick="CloseAddConnectionModal();return false;" class="ModalExitButton"></a>
                                            </div>
                                            <span class='Modal-title'></span>
                                        </div>
                                    </div>
                                    <div class="ModalScrollContent">
                                        <div class="ModalPadContent">
                                            Add a custom connection to use when importing a database table. Username and Passwords will be hidden when finished.
                                            <div class="clear-space"></div>
                                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                                <ContentTemplate>
                                                    <asp:Panel ID="Panel1" runat="server" DefaultButton="btn_addconnectionstring">
                                                        <asp:TextBox ID="tb_connectionname" runat="server" CssClass="textEntry margin-top margin-right-big" Text="Connection Name"
                                                            Width="98%" onfocus="if(this.value=='Connection Name')this.value=''" onblur="if(this.value=='')this.value='Connection Name'"
                                                            MaxLength="50"></asp:TextBox>
                                                        <asp:TextBox ID="tb_connectionstring" runat="server" CssClass="textEntry margin-top margin-right-big"
                                                            Text="Connection String" Width="98%" Height="75px" TextMode="MultiLine" onfocus="if(this.value=='Connection String')this.value=''"
                                                            onblur="if(this.value=='')this.value='Connection String'"></asp:TextBox>
                                                        <asp:DropDownList ID="dd_provider_connectionstring" CssClass="margin-top margin-right-big" runat="server"
                                                            Width="200px">
                                                        </asp:DropDownList>
                                                        <div class="clear-margin" style="height: 25px;">
                                                            <div id="savedconnections_postmessage">
                                                            </div>
                                                        </div>
                                                    </asp:Panel>
                                                </ContentTemplate>
                                                <Triggers>
                                                    <asp:AsyncPostBackTrigger ControlID="btn_addconnectionstring" />
                                                </Triggers>
                                            </asp:UpdatePanel>
                                        </div>
                                    </div>
                                    <div class="ModalButtonHolder">
                                        <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CloseAddConnectionModal();" />
                                        <asp:Button ID="btn_addconnectionstring" runat="server" Text="Add" CssClass="input-buttons no-margin float-left RandomActionBtns"
                                            OnClick="btn_addconnectionstring_Click" />
                                        <div class="clear"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="td-settings-desc">
                    All connection strings are encrypted. You cannot view entire connection string such as Username and Password. If you edit a connection string with a user Id and password, you will need to re-enter them before saving.
                </div>
            </div>
        </asp:Panel>
        <div id="password-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="400">
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
                                            <div class="clear-space"></div>
                                            <div class="clear-space"></div>
                                            <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelRequest()" />
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');" />
                                            <div class="clear"></div>
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
        <script type="text/javascript" src='<%=ResolveUrl("~/WebControls/jscolor/jscolor.js")%>'></script>
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
    </div>
</asp:Content>
