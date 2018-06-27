<%@ Page Title="Database Importer" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="dbImporter.aspx.cs" Inherits="SiteTools_dbImporter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Table Importer
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <asp:Panel ID="pnl_TableEntries" runat="server" ClientIDMode="Static" CssClass="pnl-section">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <a id="importNewWizardBtn" runat="server" class="input-buttons-create" onclick="StartImportWizard();return false;">Import Wizard</a>
                <div class="clear-space">
                </div>
                <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_ImportedTables" runat="server">
                        </asp:Panel>
                        <asp:Panel ID="pnl_tableDetailList" runat="server">
                        </asp:Panel>
                        <asp:HiddenField ID="hf_deleteimport" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_editimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_editimport_ValueChanged" />
                        <asp:HiddenField ID="hf_updateimport" runat="server" ClientIDMode="Static" OnValueChanged="hf_updateimport_ValueChanged" />
                        <asp:HiddenField ID="hf_createAppImport" runat="server" ClientIDMode="Static" OnValueChanged="hf_createAppImport_ValueChanged" />
                        <asp:HiddenField ID="hf_updateList" runat="server" ClientIDMode="Static" OnValueChanged="hf_updateList_ValueChanged" />
                        <div class="clear">
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="hf_editimport" />
                        <asp:AsyncPostBackTrigger ControlID="hf_updateimport" />
                        <asp:AsyncPostBackTrigger ControlID="hf_createAppImport" />
                        <asp:AsyncPostBackTrigger ControlID="hf_updateList" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                The connection strings of the imported databases
            cannot be overridden. In order to change them, you must delete the database and
            re-import it. (Deleting will not delete actual database, only the connection string.)
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
                                            Give your table a name and a description. New tables will use the name for the App name. To change the app name of an existing table, you can be change it in the App Management->App Manager page.
                                            <div class="clear-space"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Name</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_Databasename" runat="server" CssClass="textEntry" MaxLength="150" AutoCompleteType="None" Width="100%"></asp:TextBox>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Description</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_description" runat="server" CssClass="textEntry" AutoCompleteType="None" ClientIDMode="Static" Width="100%"></asp:TextBox>
                                            </div>
                                        </div>
                                    </li>
                                    <li id="step2">
                                        <div class="step-container">
                                            Enter in the Database Connection String and select the Database Provider that you will be attempting to connect to. Before you can continue to the next step, you must test the connection.
                                            <div class="clear-space"></div>
                                            <div id="div_connectionstring">
                                                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                                    <ContentTemplate>
                                                        <asp:Panel ID="pnl_enterConnectionstring" runat="server">
                                                            <asp:TextBox ID="tb_connstring" runat="server" CssClass="textEntry" Width="100%" AutoPostBack="false" TextMode="MultiLine" Height="70px"></asp:TextBox>
                                                            <div class="clear-space">
                                                            </div>
                                                            <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://www.connectionstrings.com');return false;" class="float-right">Need Help?</a>
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
                                    <li id="step3">
                                        <div class="step-container">
                                            Here you will be able to customize which columns and table to import. You will also be able to rename the columns for the app (Only if in simple mode).
                                            <div class="clear-space"></div>
                                            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                                                <ContentTemplate>
                                                    <div class="input-settings-holder">
                                                        <span class="font-bold">Advance Mode</span>
                                                        <div class="clear-space-two"></div>
                                                        <div class="field switch">
                                                            <asp:RadioButton ID="rb_adv_enabled" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                                OnCheckedChanged="rb_adv_enabled_Checked" AutoPostBack="true" />
                                                            <asp:RadioButton ID="rb_adv_disabled" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                                OnCheckedChanged="rb_adv_disabled_Checked" AutoPostBack="true" />
                                                        </div>
                                                        <div class="clear-space-two"></div>
                                                        <small>Advance mode allows you to manually create the SELECT COMMAND.</small>
                                                    </div>
                                                    <asp:Panel ID="pnl_txtselect" runat="server">
                                                        <asp:TextBox ID="tb_selectcomm" runat="server" CssClass="textEntry"
                                                            Width="100%" AutoPostBack="False" TextMode="MultiLine" Height="70px"></asp:TextBox>
                                                        <div class="clear"></div>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnl_ddselect" runat="server" Visible="false" Enabled="false">
                                                        <div class="input-settings-holder">
                                                            <span class="font-bold">SELECT</span>
                                                            <div class="clear-space-two"></div>
                                                            <asp:CheckBoxList ID="cb_ddselect" runat="server" CssClass="dbimport-cb" CellPadding="0" CellSpacing="0"
                                                                AutoPostBack="true" OnSelectedIndexChanged="cb_ddselect_Changed">
                                                            </asp:CheckBoxList>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="input-settings-holder">
                                                            <span class="font-bold">FROM</span>
                                                            <div class="clear-space-two"></div>
                                                            <div class="float-left">
                                                                <asp:DropDownList ID="dd_ddtables" CssClass="margin-right"
                                                                    OnSelectedIndexChanged="dd_ddtables_Changed" runat="server" AutoPostBack="true">
                                                                </asp:DropDownList>
                                                            </div>
                                                            <div class="float-left">
                                                                <asp:TextBox ID="tb_conditional" runat="server" CssClass="textEntry"
                                                                    Text="[CONDITIONAL STATEMENT]" Width="300px" onfocus="if(this.value=='[CONDITIONAL STATEMENT]')this.value=''"
                                                                    onblur="if(this.value=='')this.value='[CONDITIONAL STATEMENT]'"></asp:TextBox>
                                                            </div>
                                                            <div class="clear-space-two"></div>
                                                            <small>Leave the conditional statement blank if no condition is needed</small>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="input-settings-holder">
                                                            <span class="font-bold">ORDER BY</span>
                                                            <div class="clear-space-two"></div>
                                                            <div class="float-left">
                                                                <asp:DropDownList ID="dd_orderby" runat="server" CssClass="margin-right">
                                                                </asp:DropDownList>
                                                            </div>
                                                            <div class="float-left">
                                                                <asp:DropDownList ID="dd_orderdirection" runat="server">
                                                                    <asp:ListItem Text="ASC" Value="ASC"></asp:ListItem>
                                                                    <asp:ListItem Text="DESC" Value="DESC"></asp:ListItem>
                                                                </asp:DropDownList>
                                                            </div>
                                                            <div class="clear"></div>
                                                        </div>
                                                        <div class="clear"></div>
                                                    </asp:Panel>
                                                    <asp:HiddenField ID="hf_customizations" runat="server" ClientIDMode="Static" />
                                                    <asp:HiddenField ID="hf_summaryData" runat="server" ClientIDMode="Static" />
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
                                    <li id="step4">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Default Values</span>
                                            <div class="clear-space"></div>
                                            Set the default values for each column. These values will appear when creating a new record only.
                                            <div class="clear-space"></div>
                                            <table id="table_table_defaultvalues" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                <tbody>
                                                    <tr class="myHeaderStyle">
                                                        <td width="45px"></td>
                                                        <td>Column Name</td>
                                                        <td>Default Value</td>
                                                        <td class="edit-column-2-items"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-space"></div>
                                        </div>
                                    </li>
                                    <li id="step5">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Summaries</span>
                                            <div class="clear-space"></div>
                                            Create a summary that will displayed at the top of the App where you can use a set of predefined formulas.
                                            <div class="clear-space"></div>
                                            <div id="sortContainer_Summary">
                                                <table id="table_columns_summary" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                    <tbody>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="clear">
                                            </div>
                                            <table id="table_addcolumn_summary_Controls" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                <tbody>
                                                    <tr class="GridNormalRow addItemRow">
                                                        <td width="35px" align="left" class="GridViewNumRow"></td>
                                                        <td align="left">
                                                            <input type="text" id="tb_summaryname" class="textEntry" onkeydown="AddNewSummaryRowTextBoxKeyDown(event)" maxlength="100" style="width: 95%;" />
                                                        </td>
                                                        <td align="left" width="184px">
                                                            <select id="dd_columnSummary" style="width: 95%;">
                                                            </select>
                                                        </td>
                                                        <td align="left" width="119px">
                                                            <select id="dd_formulatype" onchange="onFormulaTypeChange(this);">
                                                                <option value="Sum">Sum</option>
                                                                <option value="Min">Min</option>
                                                                <option value="Max">Max</option>
                                                                <option value="Average">Average</option>
                                                                <option value="SumIf">SumIf</option>
                                                                <option value="MinIf">MinIf</option>
                                                                <option value="MaxIf">MaxIf</option>
                                                                <option value="AverageIf">AverageIf</option>
                                                            </select>
                                                        </td>
                                                        <td align="center" style="width: 64px;">
                                                            <a onclick="AddNewSummaryColumn();return false;" class="td-add-btn" title="Add Column"></a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-space"></div>
                                        </div>
                                    </li>
                                    <li id="step6">
                                        <div class="step-container">
                                            All imported databases automatically create an app that you will have to install for each user after importing. Select the options you wish to include in this Table Import.
                                            <div class="clear-space"></div>
                                            <div id="div_installAfterLoad">
                                                <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" CssClass="cb_style" Text="&nbsp;Install app for current user after import" Checked="true" />
                                                <div class="clear-space"></div>
                                            </div>
                                            <div id="div_isPrivate">
                                                <asp:CheckBox ID="cb_isPrivate" runat="server" CssClass="cb_style" Text="&nbsp;Make this app private (Only for me)" ClientIDMode="Static" Checked="False" />
                                                <div class="clear-space"></div>
                                            </div>
                                            <asp:CheckBox ID="cb_allowNotifi" runat="server" CssClass="cb_style" Text="&nbsp;Enable Notifications - Notify users upon change" ClientIDMode="Static" Checked="true" />
                                            <div class="clear-space"></div>
                                            <asp:CheckBox ID="cb_AllowEditAdd" runat="server" CssClass="cb_style" Text="&nbsp;Allow this import to be editable" Checked="False" />
                                            <div class="clear-space"></div>
                                            <div class="clear-space"></div>
                                            <div id="tr-usersallowed" class="input-settings-holder" style="display: none;">
                                                <span class="font-bold">Users Allowed To Edit</span>
                                                <div class="clear-space-two"></div>
                                                <asp:UpdatePanel ID="updatePnl_UsersAllowedToEdit" runat="server" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                                        </asp:Panel>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                                <asp:HiddenField ID="hf_usersAllowedToEdit" runat="server" ClientIDMode="Static" />
                                                <div class="clear">
                                                </div>
                                            </div>
                                        </div>
                                    </li>
                                    <li id="step7">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Customizations</span>
                                            <div class="clear-space"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Table Header Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_tableheadercolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultheadercolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultheadercolor', 'tb_tableheadercolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the color of your header's background. The forecolor will be adjusted to a black/white depending on the color chosen.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Primary Row Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_primaryrowcolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultprimarycolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultprimarycolor', 'tb_primaryrowcolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set your primary row's color that will alternate every other row starting from the first row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Alternative Row Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_alternativerowcolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultalternativecolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultalternativecolor', 'tb_alternativerowcolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set your alternative row's color that will alternate every other row starting from the second row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Show Row Counts</span>
                                                <div class="clear-space-two"></div>
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_showrowcounts_on" ClientIDMode="Static" runat="server" Text="Yes" CssClass="cb-enable no-postback" Checked="true" />
                                                    <asp:RadioButton ID="rb_showrowcounts_off" ClientIDMode="Static" runat="server" Text="No" CssClass="cb-disable no-postback" />
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Set your alternative row's color that will alternate every other row starting from the second row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Table View Style</span>
                                                <div class="clear-space-two"></div>
                                                <select id="dd_viewstyle">
                                                    <option value="default">Use Default</option>
                                                    <option value="excel">Excel Spreadsheet</option>
                                                </select>
                                                <div class="clear-space-two"></div>
                                                <small>Set your table's view style. You can select between the default style and an Excel Spreadsheet style. Styles will have there own way of editing an item.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Default Font Family</span>
                                                <div class="clear-space-two"></div>
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
                                                <div class="clear-space-two"></div>
                                                <small>Set your table's main font family that you want to display for the entire table.</small>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                    </li>
                                    <li id="step8">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">App Customizations</span>
                                            <div class="clear-space"></div>
                                            <div id="appicon-holder-create" class="input-settings-holder">
                                                <span class="font-bold">App Icon</span>
                                                <div class="clear-space-two"></div>
                                                <div class="float-left">
                                                    <a id="urlIcon-tab" onclick="ChangeIconUploadType(0);return false;">Click here to use Url Image</a>
                                                    <a id="uploadIcon-tab" onclick="ChangeIconUploadType(1);return false;" style="display: none;">Click here to Upload Icon</a>
                                                </div>
                                                <div class="clear-space-five"></div>
                                                <div id="uploadIcon">
                                                    <input type="file" id="MainContent_fu_image_create" />
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                                </div>
                                                <div id="urlIcon" style="display: none">
                                                    <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" Width="95%" placeholder="Image Url"></asp:TextBox>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Upload or copy and paste a url image to save as the App Icon.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Show Description</span>
                                                <div class="clear-space-two"></div>
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_showdescription_on" ClientIDMode="Static" runat="server" Text="Yes" CssClass="cb-enable no-postback" Checked="true" />
                                                    <asp:RadioButton ID="rb_showdescription_off" ClientIDMode="Static" runat="server" Text="No" CssClass="cb-disable no-postback" />
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Set to Yes to show the description of your table below the title on the app.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Title Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_apptitle_color" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_apptitle_color_default" runat="server" CssClass="cb_style" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_apptitle_color_default', 'tb_apptitle_color');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app title color or use the default.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Header Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_appbackground_color" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_appbackground_color_default" runat="server" CssClass="cb_style" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_appbackground_color_default', 'tb_appbackground_color');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app header background color or use the default.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Header Image</span>
                                                <div class="clear-space-two"></div>
                                                <input type="text" id="tb_appbackground_image" class="textEntry" placeholder="Image Url" style="width: 100%" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app header background image (if wanted). Leave blank if you do not want an image as the background.</small>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                    </li>
                                    <li id="step9">
                                        <div class="step-container">
                                            You can enable the table data chart below. Select which chart type you want to show along with which columns to associate the chart with.
                                            <div class="clear-space"></div>
                                            <div id="chart_selector">
                                                <div class="input-settings-holder">
                                                    <span class="font-bold">Chart Type</span>
                                                    <div class="clear-space-two"></div>
                                                    <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-right-big" ClientIDMode="Static" onchange="ChartTypeChangeEvent();">
                                                    </asp:DropDownList>
                                                    <div id="img_charttype_holder" class="margin-top-sml">
                                                        <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" style="max-height: 50px;" />
                                                    </div>
                                                    <div class="clear-space-two"></div>
                                                    <small>This chart image will be used as the main App Icon. Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">here</a> to see how the data should be setup for this chart type.</small>
                                                </div>
                                                <div id="chart-title-holder">
                                                    <div id="tr-chart-title">
                                                        <div class="input-settings-holder">
                                                            <span class="font-bold">Chart Title</span>
                                                            <div class="clear-space-two"></div>
                                                            <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry" AutoCompleteType="None" Width="100%" MaxLength="150" ClientIDMode="Static"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div id="chart-column-selector">
                                                    <div class="input-settings-holder">
                                                        <span class="font-bold">Data Columns</span>
                                                        <div class="clear-space-two"></div>
                                                        <div id="chart-column-selector-holder"></div>
                                                        <div class="clear-space-two"></div>
                                                        <small>Select the columns you wish to add to use on the chart.</small>
                                                    </div>
                                                </div>
                                                <div class="clear"></div>
                                            </div>
                                        </div>
                                    </li>
                                </ul>
                                <div class="clear"></div>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="ConfirmCancelWizard();" />
                            <input type="button" class="input-buttons prev-step modal-ok-btn" value="Previous" onclick="CreatePrevStep();" style="display: none;" />
                            <input type="button" class="input-buttons next-step  modal-ok-btn" value="Next" onclick="CreateNextStep();" style="display: none;" />
                            <input id="btn_finishImportWizard" type="button" class="input-buttons RandomActionBtns modal-ok-btn" value="Import" onclick="ImportTableClick();" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl_SavedConnections" runat="server" ClientIDMode="Static" CssClass="pnl-section" Style="display: none;">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="UpdatePanel10" runat="server">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_savedconnections_holder" runat="server">
                        </asp:Panel>
                        <div class="clear">
                        </div>
                        <asp:HiddenField ID="hf_deletestring" runat="server" OnValueChanged="hf_deletestring_Changed" />
                        <asp:HiddenField ID="hf_editstring" runat="server" OnValueChanged="hf_editstring_Changed" />
                        <asp:HiddenField ID="hf_updatestring" runat="server" OnValueChanged="hf_updatestring_Changed" />
                        <asp:HiddenField ID="hf_connectionNameEdit" runat="server" />
                        <asp:HiddenField ID="hf_connectionStringEdit" runat="server" />
                        <asp:HiddenField ID="hf_databaseProviderEdit" runat="server" />
                        <asp:HiddenField ID="hf_addconnectionstring" runat="server" OnValueChanged="hf_addconnectionstring_ValueChanged" />
                    </ContentTemplate>
                </asp:UpdatePanel>
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
                                        Enter the password of the user who created the import.
                                        <div class="clear-space"></div>
                                        <div class="input-settings-holder">
                                            <span class="font-bold">Password</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="textEntry-noWidth" Width="99%"></asp:TextBox>
                                        </div>
                                        <div class="clear-space"></div>
                                        <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelRequest()" />
                                        <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-left"
                                            Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="loadingPopup.Message('Validating Password...');" />
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
</asp:Content>
