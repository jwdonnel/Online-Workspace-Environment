<%@ Page Title="Database Manager" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="dbManager.aspx.cs" Inherits="SiteTools_DbManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Database Manager
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <div id="dbviewer-load">
        <asp:Panel ID="DBMaintenance" runat="server">
            <asp:Panel ID="pnl_databaseRecovery" runat="server" ClientIDMode="Static" CssClass="pnl-section">
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <div class="float-left pad-bottom-big pad-right-big margin-right">
                            <div class="clear">
                            </div>
                            <div class="input-settings-holder">
                                <span class="font-bold">Backup Entire Database</span>
                                <div class="clear-space-two"></div>
                                <asp:TextBox ID="tb_download_backup" runat="server" Text="Backup File Description"
                                    Width="300" CssClass="textEntry margin-right float-left margin-bottom" MaxLength="50" onfocus="if(this.value=='Backup File Description')this.value=''"
                                    onblur="if(this.value=='')this.value='Backup File Description'"></asp:TextBox>
                                <asp:LinkButton ID="lbtn_downloaddb" runat="server" OnClick="lbtn_downloaddb_Click" CssClass="input-buttons margin-right-big">Backup</asp:LinkButton>
                                <div class="clear">
                                </div>
                            </div>
                        </div>
                        <div class="float-left pad-bottom-big pad-right-big margin-right">
                            <div class="clear">
                            </div>
                            <div class="input-settings-holder">
                                <span class="font-bold">Backup Non-Asp Tables</span>
                                <div class="clear-space-two"></div>
                                <asp:TextBox ID="tb_backup_databse" runat="server" Text="Backup File Description"
                                    Width="300" CssClass="textEntry margin-right float-left margin-bottom" MaxLength="50" onfocus="if(this.value=='Backup File Description')this.value=''"
                                    onblur="if(this.value=='')this.value='Backup File Description'"></asp:TextBox>
                                <asp:LinkButton ID="lbtn_buchat" runat="server" OnClick="lbtn_buchat_Click" CssClass="RandomActionBtns-backup-perform input-buttons margin-right-big"
                                    Text="Backup" />
                                <div class="clear">
                                </div>
                            </div>
                        </div>
                        <div class="float-left pad-bottom-big">
                            <div class="clear">
                            </div>
                            <div class="input-settings-holder">
                                <span class="font-bold">Upload Backup <%= ServerSettings.BackupFileExt %> file</span>
                                <div class="clear-space-two"></div>
                                <asp:TextBox ID="tb_upload_desc" runat="server" Text="Upload File Description" CssClass="textEntry margin-right float-left margin-bottom"
                                    MaxLength="50" onfocus="if(this.value=='Upload File Description')this.value=''"
                                    Width="300" onblur="if(this.value=='')this.value='Upload File Description'"></asp:TextBox>
                                <asp:LinkButton ID="lbtn_uploaddb" runat="server" CssClass="RandomActionBtns input-buttons"
                                    OnClick="lbtn_uploaddb_Click">Upload</asp:LinkButton>
                                <div class="clear-space-five">
                                </div>
                                <asp:FileUpload ID="FileUpload1" runat="server" />
                                <div class="clear">
                                </div>
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:UpdatePanel ID="updatepnl_list" runat="server">
                            <ContentTemplate>
                                <asp:HiddenField ID="HiddenField1_sitesettings" runat="server" ClientIDMode="Static" />
                                <asp:Panel ID="pnl_backuplist" runat="server">
                                </asp:Panel>
                                <asp:HiddenField ID="lb_restorethis" runat="server" ClientIDMode="Static" OnValueChanged="lb_restorethis_Changed" />
                                <asp:HiddenField ID="hf_buRestore_type" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_buRestoreCommand_Value" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_StartWork" runat="server" OnValueChanged="hf_StartWork_Changed"
                                    ClientIDMode="Static" />
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="lbtn_buchat" />
                            </Triggers>
                        </asp:UpdatePanel>
                        <div class="clear-space"></div>
                    </div>
                    <div class="td-settings-desc">
                        Backups may take awhile depending on the database size. Backup files are encrypted
                    and can only be used with this site. Not all tables from the database will be backed
                    up. Only the critical tables will be backed up. Any type of restore/delete will
                    require a password to continue. To backup all tables, choose the 'Backup Entire Database' feature.
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnl_adminOnly_RestoreDefaults" ClientIDMode="Static" runat="server" Visible="false" Enabled="false" CssClass="pnl-section" Style="display: none;">
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <asp:UpdatePanel ID="updatepnl_tableDefaults" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:DropDownList ID="dd_defaultTableList" AutoPostBack="true" runat="server" CssClass="margin-right-big" ClientIDMode="Static" OnSelectedIndexChanged="dd_defaultTableList_SelectedIndexChanged"></asp:DropDownList>
                                <asp:LinkButton ID="lbtn_updateDefaultTableList" runat="server" CssClass="float-right margin-top-sml img-refresh RandomActionBtns" OnClick="lbtn_updateDefaultTableList_Click"></asp:LinkButton>
                                <input type="button" class="input-buttons" value="Restore Defaults" onclick="RestoreDefaultValues();" />
                                <small class="pad-right-big">Data below comes from the DatabaseDefaultValues.xml and not from your actual database.</small>
                                <asp:Panel ID="pnl_defaultTableHolder" runat="server">
                                </asp:Panel>
                                <asp:HiddenField ID="hf_restoreDefaults" runat="server" ClientIDMode="Static" OnValueChanged="hf_restoreDefaults_ValueChanged" />
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="dd_defaultTableList" />
                                <asp:AsyncPostBackTrigger ControlID="lbtn_updateDefaultTableList" />
                                <asp:AsyncPostBackTrigger ControlID="hf_restoreDefaults" />
                            </Triggers>
                        </asp:UpdatePanel>
                        <div class="clear-space"></div>
                    </div>
                    <div class="td-settings-desc">
                        If you need to restore the default values for the database table selected, you can click on the "Restore Defaults" button next to the table dropdown list.<br />
                        <b class="pad-right-sml">Warning:</b>This will override any existing values with the same Ids.
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnl_adminOnly_DbScanner" runat="server" ClientIDMode="Static" CssClass="pnl-section" Style="display: none;">
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <asp:UpdatePanel ID="updatepnl_DbScanner" runat="server">
                            <ContentTemplate>
                                <asp:Panel ID="pnl_databaseChecker" runat="server">
                                </asp:Panel>
                                <div class="pad-left">
                                    <div class="clear-space"></div>
                                    <div class="clear-space"></div>
                                    <asp:Button ID="btn_checkDatabase" runat="server" CssClass="input-buttons" Text="Scan" Width="75px" OnClick="btn_checkDatabase_Click" />
                                    <asp:Button ID="btn_UpdateDatabase" runat="server" CssClass="input-buttons" Text="Fix" Width="75px" OnClick="btn_UpdateDatabase_Click" Visible="false" Enabled="false" />
                                    <div class="clear-space"></div>
                                    <asp:CheckBox ID="cbAutoFixDB" ClientIDMode="Static" runat="server" Text="&nbsp;Automatically fix any database issues" Visible="false" Enabled="false" OnCheckedChanged="cbAutoFixDB_CheckedChanged" AutoPostBack="true" />
                                    <div class="clear-space"></div>
                                    <asp:Label ID="lbl_updatedbHint" runat="server" Font-Size="X-Small" Visible="false" Enabled="false" Text="You may have to update the database more than once to completely update it."></asp:Label>
                                    <div class="clear-space-five"></div>
                                    <asp:Panel ID="pnl_databaseissues" runat="server">
                                    </asp:Panel>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <div class="td-settings-desc">
                        You can use this feature to check if all your table are up to date with the current system. When changing databases from SqlCe to an Sql Express, you will need to run this to insert and update existing tables.<br />
                        This will not overwrite any data in your current database. This process may take a couple of minutes.
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnl_autobackstyem" runat="server" CssClass="pnl-section" ClientIDMode="Static" Style="display: none;">
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Database Auto Backup
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:UpdatePanel ID="UpdatePanel7" runat="server">
                            <ContentTemplate>
                                <asp:HiddenField ID="hf_EditSlot" runat="server" OnValueChanged="hf_EditSlot_Changed"
                                    ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_DeleteSlot" runat="server" OnValueChanged="hf_DeleteSlot_Changed"
                                    ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_CancelSlot" runat="server" OnValueChanged="hf_CancelSlot_Changed"
                                    ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_UpdateSlot" runat="server" OnValueChanged="hf_UpdateSlot_Changed"
                                    ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_dayofweek_edit" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_timeofweek_edit" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_backuptype_edit" runat="server" ClientIDMode="Static" />
                                <div class="clear"></div>
                                <span style="font-size: 14px;">Background Service is&nbsp;</span><asp:Label ID="lbl_autoBackupsystem_status" runat="server" Font-Size="14px"></asp:Label>
                                <asp:Panel ID="pnl_AutoBackup" runat="server">
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_Entries" runat="server">
                                    </asp:Panel>
                                    <asp:HiddenField ID="lbtn_addAbs" runat="server" ClientIDMode="Static" OnValueChanged="lbtn_addAbs_Clicked" />
                                    <asp:HiddenField ID="hf_autoBackup_day" runat="server" ClientIDMode="Static" />
                                    <asp:HiddenField ID="hf_autoBackup_time" runat="server" ClientIDMode="Static" />
                                    <asp:HiddenField ID="hf_autoBackup_type" runat="server" ClientIDMode="Static" />
                                </asp:Panel>
                                <div class="clear">
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <div class="td-settings-desc">
                        To turn on/off the Auto Backup System, go to the Background Services page and start/stop service. Click <a href="../ServerMaintenance/BackgroundServices.aspx">here</a> to turn on/off service.
                    </div>
                </div>
            </asp:Panel>
        </asp:Panel>
        <div id="tablestorestore-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="430" data-setmaxheight="400">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div class="clear-margin">
                                    Compatible Tables shown below are from this backup file. Select the table(s) you wish to restore.
                                        <div class="clear-space">
                                        </div>
                                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                        <ContentTemplate>
                                            <a href="#" id="btn_deselectall_tables" onclick="DeselectAllTables();return false;">Deselect All</a> <a href="#" id="btn_selectall_tables" onclick="SelectAllTables();return false;"
                                                style="display: none;">Select All</a>
                                            <div class="clear-space">
                                            </div>
                                            <asp:Panel ID="pnl_tablerestore" runat="server">
                                                <asp:CheckBoxList ID="cblist_tables" runat="server">
                                                </asp:CheckBoxList>
                                                <div class="clear-space">
                                                </div>
                                            </asp:Panel>
                                            <div class="clear" style="height: 20px;">
                                            </div>
                                            <asp:HiddenField ID="hf_filename_tablesrestore" runat="server" />
                                            <div class="clear-space-five">
                                            </div>
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="btn_canceltables" />
                                            <asp:AsyncPostBackTrigger ControlID="btn_finish_addtables" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_finish_addtables" runat="server" Text="Restore" CssClass="input-buttons float-left"
                                OnClick="btn_finish_addtables_Click" OnClientClick="StartBackingUp()" CausesValidation="false" />
                            <asp:Button ID="btn_canceltables" runat="server" Text="Cancel" CssClass="input-buttons RandomActionBtns float-right no-margin"
                                OnClick="btn_canceltables_Click" CausesValidation="false" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
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
                                            <span class="password-hint">Enter the password of the user who created the backup.</span>
                                            <div class="clear-space"></div>
                                            <b class="pad-right">Password</b>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="textEntry-noWidth"></asp:TextBox>
                                            <div class="clear-space"></div>
                                            <div class="clear-space"></div>
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="loadingPopup.Message('Validating Password...');" />
                                            <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelRequest()" />
                                            <div class="clear"></div>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript" src="../../Scripts/jquery/jquery.fileDownload.js"></script>
</asp:Content>
