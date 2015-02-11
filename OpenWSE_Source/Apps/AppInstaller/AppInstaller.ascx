<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppInstaller.ascx.cs"
    Inherits="Apps_AppInstaller_AppInstaller" %>
<div id="appinstaller-load" class="main-div-app-bg">
    <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
            <td valign="top" class="td-sidebar pad-right">
                <div id="sidebar-items-appinstaller" class="sidebar-items-fixed">
                    <div class="stylefour">
                        <ul class="homedashlinks">
                            <li id="hdl1_apps" class="active" style="width: 50%; text-align: center; margin-top: 0px;"><a href="#"
                                onclick="AppsTab_AppInstaller();return false;" style="float: none; line-height: 44px;">Apps</a></li>
                            <li id="hdl2_plugins" style="width: 50%; text-align: center; margin-top: 0px;"><a href="#" onclick="PluginsTab_AppInstaller();return false;"
                                style="float: none; line-height: 44px;">Plugins</a></li>
                        </ul>
                    </div>
                    <div class="clear" style="height: 80px;">
                    </div>
                    <div id="app-category-sidebar">
                        <div class="pad-left">
                            <h3>App Categories</h3>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="pad-left pad-right">
                            <small><b class="pad-right-sml">Note:</b>Some categories may be empty due to unauthorized
                            apps.</small>
                        </div>
                        <div id="app-category-holder" class="pad-all">
                            <h4>Loading Categories. Please Wait...</h4>
                        </div>
                    </div>
                    <div id="plugin-info-sidebar" style="display: none;">
                        <div class="pad-left">
                            <h3>Site Plugins</h3>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div class="pad-left pad-right">
                            <small><b class="pad-right-sml">Note:</b>Installing a plugin may cause instability for
                            some apps. Some plugins may not work well when more than one plugin is installed.</small>
                        </div>
                    </div>
                </div>
            </td>
            <td valign="top">
                <div class="pad-all app-title-bg-color" style="height: 40px">
                    <div class="float-left">
                        <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                        <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
                    </div>
                    <div class="float-right pad-top-sml" style="font-size: 15px">
                        <div class="float-right">
                            <div id="searchwrapper" style="width: 375px;">
                                <input id="tb_search_AppInstaller" type="text" class="searchbox" onfocus="if(this.value=='Search Apps/Plugins')this.value=''"
                                    onblur="if(this.value=='')this.value='Search Apps/Plugins'" value="Search Apps/Plugins" />
                                <a href="#" title="Clear search" class="searchbox_clear" onclick="$('#tb_search_AppInstaller').val('Search Apps/Plugins');Search_appinstaller();return false;"></a><a href="#" class="searchbox_submit" onclick="Search_appinstaller();return false;"></a>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="clear-margin">
                    <div class="pad-left-big">
                        <div class="float-right" style="padding: 5px 20px; text-align: right;">
                            <div class="pad-top-sml">
                                <span class="font-bold pad-right">Apps Available:</span><asp:Label ID="lbl_TotalApps"
                                    runat="server" Text="0"></asp:Label>
                                <div class="clear-space-five">
                                </div>
                                <span class="font-bold pad-right">Plugins Available:</span><asp:Label ID="lbl_TotalPlugins"
                                    runat="server" Text="0"></asp:Label>
                            </div>
                        </div>
                        <div id="select-btns-appinstaller" class="float-left" style="padding: 5px 20px;">
                            <div class="pad-top-sml">
                                <a href="#" id="hyp_selectedAll_appinstaller" class="sb-links margin-right-big"
                                    onclick="SelectAll_AppInstaller();return false;">Select All</a> <a href="#" id="hyp_deselectAll_appinstaller"
                                        class="sb-links margin-right-big" onclick="DeselectAll_AppInstaller();return false;"
                                        style="display: none;">Deselect All</a> <a href="#" id="hyp_installSelected_appinstaller"
                                            class="sb-links margin-right-left" onclick="InstallSelected_AppInstaller();return false;"
                                            style="display: none;">Install Selected</a>
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <div id="all_apps_holder" runat="server" clientidmode="Static" style="display: block;">
                        </div>
                        <div id="apps_holder">
                            <h3 class='pad-left pad-top-big'>Loading Apps...</h3>
                        </div>
                        <div id="plugins_holder" style="display: none;">
                            <h3 class='pad-left pad-top-big'>Loading Plugins...</h3>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</div>
<div id="appinstaller-About-element" class="Modal-element outside-main-app-div">
    <div class="Modal-element-align">
        <div class="Modal-element-modal">
            <div class="ModalHeader">
                <div>
                    <div class="app-head-button-holder-admin">
                        <a href="#" onclick="openWSE.LoadModalWindow(false, 'appinstaller-About-element', '');$('#AboutAppdHolder').html('');return false;"
                            class="ModalExitButton"></a>
                    </div>
                    <span class="Modal-title"></span>
                </div>
            </div>
            <div class="ModalPadContent" style="width: 500px; max-height: 400px; overflow: auto; margin-right: 0px; margin-top: -5px;">
                <div id="AboutAppdHolder">
                </div>
            </div>
        </div>
    </div>
</div>
