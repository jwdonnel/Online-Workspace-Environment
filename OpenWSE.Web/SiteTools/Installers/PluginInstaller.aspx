﻿<%@ Page Title="Plugin Installer" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="PluginInstaller.aspx.cs" Inherits="SiteTools_PluginInstaller" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Plugin Installer
        </div>
        <div class="title-line"></div>
        <div class="td-settings-ctrl">
            <asp:UpdatePanel ID="updatePnl_PluginList" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_PluginList" runat="server" ClientIDMode="Static"></asp:Panel>
                    <div class="clear"></div>
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
    </asp:UpdatePanel>
</asp:Content>
