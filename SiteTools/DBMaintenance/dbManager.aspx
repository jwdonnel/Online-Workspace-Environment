<%@ page title="Database Manager" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_DbManager, App_Web_qpm402mx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div id="dbviewer-load">

            <asp:Panel ID="DBMaintenance" runat="server">
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Database Recovery
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <div class="float-left pad-top pad-bottom" style="width: 365px; margin-right: 60px; border-right: 1px solid #DDD">
                            <div class="clear-space">
                            </div>
                            <div class="clear-space">
                            </div>
                            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                                <ContentTemplate>
                                    <div class="float-left font-bold pad-right">
                                        <div class="float-left pad-top-sml font">
                                            Download Current Database:
                                        </div>
                                    </div>
                                    <asp:LinkButton ID="lbtn_downloaddb" runat="server" OnClick="lbtn_downloaddb_Click"
                                        PostBackUrl="~/SiteTools/DBMaintenance/dbManager.aspx" Style="margin-left: 50px;">Download</asp:LinkButton>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="lbtn_downloaddb" />
                                </Triggers>
                            </asp:UpdatePanel>
                            <div class="clear-margin">
                                <asp:TextBox ID="tb_download_backup" runat="server" Text="Download File Description"
                                    Width="300" CssClass="textEntry" MaxLength="50" onfocus="if(this.value=='Download File Description')this.value=''"
                                    onblur="if(this.value=='')this.value='Download File Description'"></asp:TextBox>
                            </div>
                        </div>
                        <div class="float-left pad-top pad-bottom" style="width: 365px; margin-right: 60px; border-right: 1px solid #DDD">
                            <span class="font-bold">Upload Backup <%= ServerSettings.BackupFileExt %> file</span>
                            <div class="clear-space-five">
                            </div>
                            <asp:FileUpload ID="FileUpload1" runat="server" />
                            <asp:LinkButton ID="lbtn_uploaddb" runat="server" CssClass="margin-left RandomActionBtns"
                                OnClick="lbtn_uploaddb_Click">Upload</asp:LinkButton>
                            <div class="clear-margin">
                                <asp:TextBox ID="tb_upload_desc" runat="server" Text="Upload File Description" CssClass="textEntry"
                                    MaxLength="50" onfocus="if(this.value=='Upload File Description')this.value=''"
                                    Width="300" onblur="if(this.value=='')this.value='Upload File Description'"></asp:TextBox>
                            </div>
                        </div>
                        <div class="float-left pad-top pad-bottom" style="width: 365px;">
                            <div class="clear-space"></div>
                            <div class="clear-space"></div>
                            <div class="clear" style="height: 3px;"></div>
                            <span class="font-bold">Backup Non-Asp Database:</span>
                            <div class="float-right" style="padding-right: 70px;">
                                <asp:LinkButton ID="lbtn_buchat" runat="server" OnClick="lbtn_buchat_Click" CssClass="RandomActionBtns-backup-perform"
                                    Text="Backup" />
                            </div>
                            <div class="clear-margin">
                                <asp:TextBox ID="tb_backup_databse" runat="server" Text="Backup File Description"
                                    Width="300" CssClass="textEntry" MaxLength="50" onfocus="if(this.value=='Backup File Description')this.value=''"
                                    onblur="if(this.value=='')this.value='Backup File Description'"></asp:TextBox>
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:UpdatePanel ID="updatepnl_list" runat="server">
                            <ContentTemplate>
                                <asp:XmlDataSource ID="XmlDataSource_1" runat="server" DataFile="~/Backups/BackupLog.xml"
                                    XPath="Backups/Backup"></asp:XmlDataSource>
                                <asp:HiddenField ID="HiddenField1_sitesettings" runat="server" ClientIDMode="Static" />
                                <asp:GridView ID="GV_BackupList" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                                    GridLines="None" OnRowCommand="GV_BackupList_RowCommand" OnRowDataBound="GV_BackupList_RowDataBound"
                                    AllowPaging="false" ShowHeaderWhenEmpty="True">
                                    <EmptyDataRowStyle ForeColor="Black" />
                                    <RowStyle CssClass="GridNormalRow" />
                                    <EmptyDataTemplate>
                                        <div class="emptyGridView">
                                            No backups in log
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <table class="myHeaderStyle" cellpadding="5" cellspacing="0" style="min-width: 1160px;">
                                                    <tr>
                                                        <td style="min-width: 150px;">Filename
                                                        </td>
                                                        <td width="150px">Backup Date
                                                        </td>
                                                        <td width="150px">Restore Date
                                                        </td>
                                                        <td width="80px">Size
                                                        </td>
                                                        <td width="150px">Backed up by
                                                        </td>
                                                        <td width="100px" align="center">Actions
                                                        </td>
                                                    </tr>
                                                </table>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <table class="myItemStyle" cellpadding="5" cellspacing="0" style="min-width: 1160px;">
                                                    <tr>
                                                        <td style="min-width: 150px; border-right: 1px solid #CCC; border-left: 1px solid #CCC;">
                                                            <%# XPath("Filename") %>
                                                            <div class="clear-space-two">
                                                            </div>
                                                            <div style="font-size: 11px">
                                                                <%# XPath("Description")%>
                                                            </div>
                                                        </td>
                                                        <td align="center" width="150px" style="border-right: 1px solid #CCC;">
                                                            <%# XPath("BackupDate")%>
                                                        </td>
                                                        <td align="center" width="150px" style="border-right: 1px solid #CCC;">
                                                            <%# XPath("RestoreDate")%>
                                                        </td>
                                                        <td align="center" width="80px" style="border-right: 1px solid #CCC;">
                                                            <%# XPath("Size")%>
                                                        </td>
                                                        <td align="center" width="150px" style="border-right: 1px solid #CCC;">
                                                            <%# XPath("User")%>
                                                        </td>
                                                        <td width="100px" align="center" style="border-right: 1px solid #CCC;">
                                                            <asp:LinkButton ID="lb_restorethis" runat="server" CommandArgument='<%# XPath("Filename") %>'
                                                                CommandName="Restore" CssClass="td-restore-btn margin-right-sml" OnClientClick="openWSE.LoadingMessage1('Please Wait...');dbType='restore';" ToolTip="Restore"></asp:LinkButton>
                                                            <asp:LinkButton ID="lb_downloadthis" runat="server" CommandArgument='<%# XPath("Filename") %>'
                                                                CommandName="Download" CssClass="td-download-btn margin-right-sml" PostBackUrl="~/SiteTools/DBMaintenance/dbManager.aspx"
                                                                ToolTip="Download"></asp:LinkButton>
                                                            <a id="lb_deletethis" class="td-delete-btn cursor-pointer" onclick="OnDelete('<%# XPath("Filename") %>');"
                                                                title="Delete"></a>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <asp:HiddenField ID="hf_buRestore_type" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_buRestoreCommand_Value" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_StartWork" runat="server" OnValueChanged="hf_StartWork_Changed"
                                    ClientIDMode="Static" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <div class="td-settings-desc">
                        Backups may take awhile depending on the database size. Backup files are encrypted
                    and can only be used with this site. Not all tables from the database will be backed
                    up. Only the critical tables will be backed up. Any type of restore/delete will
                    require a password to continue. To backup all tables, choose the 'Download Current Database' feature.
                    </div>
                </div>
                <asp:Panel ID="pnl_adminOnly_DbScanner" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Database Scanner
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <asp:UpdatePanel ID="updatepnl_DbScanner" runat="server">
                                <ContentTemplate>
                                    <div class="pad-all">
                                        <div class="pad-left">
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
                                                <asp:Label ID="lbl_updatedbHint" runat="server" Font-Size="Small" Visible="false" Enabled="false" Text="You may have to update the database more than once to completely update it."></asp:Label>
                                                <div class="clear-space-five"></div>
                                            </div>
                                        </div>
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
                <div class="table-settings-box no-border">
                    <div class="td-settings-title">
                        Automatic Backup System
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
                                <div class="table-settings-box">
                                    <div class="td-settings-title">
                                        Auto Backup
                                    </div>
                                    <div class="title-line"></div>
                                    <div class="td-settings-ctrl">
                                        <div class="field switch inline-block">
                                            <asp:RadioButton ID="rb_autoBackuState_on" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                                OnCheckedChanged="rb_autoBackuState_on_CheckedChanged" AutoPostBack="True" />
                                            <asp:RadioButton ID="rb_autoBackuState_off" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                                OnCheckedChanged="rb_autoBackuState_off_CheckedChanged" AutoPostBack="True" />
                                        </div>
                                    </div>
                                    <div class="td-settings-desc">
                                        Turning this feature on will automatically start running the Automatic Backup System. The automatic backup system only works when the Site Application is on. Each backup will run at the given time and day as specified in the list.
                                    </div>
                                </div>
                                <asp:Panel ID="pnl_AutoBackup" runat="server">
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="pnl_Entries" runat="server">
                                    </asp:Panel>
                                    <asp:Panel ID="pnl_addEntry" runat="server">
                                        <table class="myItemStyle GridNormalRow" cellpadding="0" cellspacing="0" style="width: 750px;">
                                            <tr>
                                                <td class="GridViewNumRow" width="45px"></td>
                                                <td align="center" width="150px" style="border-right: 1px solid #CCC;">
                                                    <asp:DropDownList ID="ddl_absDaytoRun" runat="server">
                                                        <asp:ListItem Text="Sunday" Value="Sunday"></asp:ListItem>
                                                        <asp:ListItem Text="Monday" Value="Monday"></asp:ListItem>
                                                        <asp:ListItem Text="Tuesday" Value="Tuesday"></asp:ListItem>
                                                        <asp:ListItem Text="Wednesday" Value="Wednesday"></asp:ListItem>
                                                        <asp:ListItem Text="Thursday" Value="Thursday"></asp:ListItem>
                                                        <asp:ListItem Text="Friday" Value="Friday"></asp:ListItem>
                                                        <asp:ListItem Text="Saturday" Value="Saturday"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td align="center" style="border-right: 1px solid #CCC;">
                                                    <asp:DropDownList ID="ddl_absBackupTimeHour" runat="server">
                                                        <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="2" Value="2"></asp:ListItem>
                                                        <asp:ListItem Text="3" Value="3"></asp:ListItem>
                                                        <asp:ListItem Text="4" Value="4"></asp:ListItem>
                                                        <asp:ListItem Text="5" Value="5"></asp:ListItem>
                                                        <asp:ListItem Text="6" Value="6"></asp:ListItem>
                                                        <asp:ListItem Text="7" Value="7"></asp:ListItem>
                                                        <asp:ListItem Text="8" Value="8"></asp:ListItem>
                                                        <asp:ListItem Text="9" Value="9"></asp:ListItem>
                                                        <asp:ListItem Text="10" Value="10"></asp:ListItem>
                                                        <asp:ListItem Text="11" Value="11"></asp:ListItem>
                                                        <asp:ListItem Text="12" Value="12"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <span class="font-bold">:</span>
                                                    <asp:DropDownList ID="ddl_absBackupTimeMin" runat="server" CssClass="margin-right">
                                                        <asp:ListItem Text="00" Value="00"></asp:ListItem>
                                                        <asp:ListItem Text="05" Value="05"></asp:ListItem>
                                                        <asp:ListItem Text="10" Value="10"></asp:ListItem>
                                                        <asp:ListItem Text="15" Value="15"></asp:ListItem>
                                                        <asp:ListItem Text="20" Value="20"></asp:ListItem>
                                                        <asp:ListItem Text="25" Value="25"></asp:ListItem>
                                                        <asp:ListItem Text="30" Value="30"></asp:ListItem>
                                                        <asp:ListItem Text="35" Value="35"></asp:ListItem>
                                                        <asp:ListItem Text="40" Value="40"></asp:ListItem>
                                                        <asp:ListItem Text="45" Value="45"></asp:ListItem>
                                                        <asp:ListItem Text="50" Value="50"></asp:ListItem>
                                                        <asp:ListItem Text="55" Value="55"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:DropDownList ID="ddl_absBackupTimeAmPm" runat="server">
                                                        <asp:ListItem Text="AM" Value="am"></asp:ListItem>
                                                        <asp:ListItem Text="PM" Value="pm"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td align="center" width="150px" style="border-right: 1px solid #CCC;">
                                                    <asp:DropDownList ID="ddl_absBackupType" runat="server">
                                                        <asp:ListItem Text="Partial" Value="partial"></asp:ListItem>
                                                        <asp:ListItem Text="Full" Value="full"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td align="center" width="75px" style="border-right: 1px solid #CCC;">
                                                    <asp:LinkButton ID="lbtn_addAbs" runat="server" OnClick="lbtn_addAbs_Clicked" CssClass="td-add-btn RandomActionBtns"
                                                        ToolTip="Add Schedule"></asp:LinkButton>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </asp:Panel>
                                <div class="clear" style="height: 20px;">
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
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
                                <asp:Button ID="btn_finish_addtables" runat="server" Text="Restore" CssClass="input-buttons"
                                    OnClick="btn_finish_addtables_Click" OnClientClick="StartBackingUp()" CausesValidation="false" />
                                <asp:Button ID="btn_canceltables" runat="server" Text="Cancel" CssClass="input-buttons RandomActionBtns no-margin"
                                    OnClick="btn_canceltables_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
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
                                                <span class="password-hint">Enter the password of the user who created the backup.</span>
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
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type='text/javascript' src='<%=ResolveUrl("~/Scripts/SiteTools/dbManager.js")%>'></script>
    </div>
</asp:Content>
