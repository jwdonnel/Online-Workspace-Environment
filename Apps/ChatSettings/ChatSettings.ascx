<%@ control language="C#" autoeventwireup="true" inherits="Apps_ChatSettings, App_Web_bclpmmev" clientidmode="Static" %>
<div id="chatlog-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color" style="height: 40px">
        <div class="float-left">
            <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
            <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
        </div>
        <asp:Panel ID="pnl_chatsettings_options" runat="server">
            <select id="chatsettings-options" class="float-right margin-top-sml">
                <option value="Chat-History-div">Chat History</option>
                <option value="Chat-Blocked-Users-div">Blocked Users</option>
                <option value="Chat-Settings-div">Settings</option>
            </select>
        </asp:Panel>
    </div>
    <div id="Chat-History-div" class="chat-settings-divs">
        <div class="clear" style="padding-top: 1px;">
        </div>
        <table cellpadding="0" cellspacing="0" style="min-height: 100%; height: 100%; width: 100%">
            <tbody>
                <tr>
                    <td valign="top" style="width: 270px;">
                        <div>
                            <div class="clear-margin pad-all font-bold" align="center">Select a log to view</div>
                            <div id="containerMessages">
                                <asp:GridView ID="GV_Messages_chatlog" runat="server" CellPadding="0" CellSpacing="0"
                                    AutoGenerateColumns="False" Width="100%" GridLines="None" AllowPaging="False"
                                    OnRowCreated="GV_Messages_RowCreated" ShowHeaderWhenEmpty="True">
                                    <AlternatingRowStyle CssClass="GridAlternate" />
                                    <EmptyDataRowStyle ForeColor="Black" />
                                    <RowStyle CssClass="GridNormalRow" />
                                    <EmptyDataTemplate>
                                        <div class="pad-left pad-right">
                                            <div class="emptyGridView">
                                                You have no chat logs recorded.
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <input type="hidden" class="hf-message-mID" value='<%#Eval("ID") %>' />
                                                <div id='Message_List_<%#Eval("ID") %>' class="messageselector" style="padding: 10px;
                                                    float: left; width: 250px">
                                                    <div class="cursor-pointer">
                                                        <div class='<%#Eval("isRead")%>'>
                                                            <table width="100%">
                                                                <tr>
                                                                    <td>
                                                                        <a href="#delete" class="td-delete-btn float-right margin-right" title="Delete Chat Date"
                                                                            onclick="DeleteLog(this, '<%#Eval("Date") %>');return false;"></a>
                                                                    </td>
                                                                    <td onclick="eventMessageOpen(this)">
                                                                        <h4>
                                                                            <%#Eval("Subject")%></h4>
                                                                        <div class="clear-margin">
                                                                            <span style="color: Blue">
                                                                                <%#Eval("Date") %></span>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </td>
                    <td valign="top">
                        <div class="messagebody-id">
                            <div style="cursor: default; overflow: auto; width: 100%">
                                <div class="pad-all-big">
                                    <div class="clear-margin">
                                        <div class="mb-container">
                                            <h3>
                                                View Chat Conversations Here</h3>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <div id="Chat-Blocked-Users-div" class="chat-settings-divs" style="display: none;">
        <div class="clear-space-five">
        </div>
        <div class="pad-all-big">
            <h4>
                Select the checkbox to block a user. Once user is blocked, that user's name and
                status in the Chat List will be removed. You can uncheck the user to display the
                user again. The user will still be able to see your username in their Chat List
                but will not be able to communicate with you until you unblock them.</h4>
            <div class="clear-space">
            </div>
            <div id="blocked-users-holder">
            </div>
        </div>
    </div>
    <div id="Chat-Settings-div" class="chat-settings-divs" style="display: none;">
        <div class="pad-all">
            <table cellpadding="10" cellspacing="10">
                <tr>
                    <td align="left" style="width: 185px;">
                        <span class="pad-right font-bold font-color-black">Chat Sound Notification</span>
                    </td>
                    <td align="left" style="width: 197px;">
                        <div class="field switch inline-block">
                            <asp:RadioButton ID="rb_chatsoundnoti_on" runat="server" Text="On" CssClass="cb-enable" />
                            <asp:RadioButton ID="rb_chatsoundnoti_off" runat="server" Text="Mute" CssClass="cb-disable selected" />
                        </div>
                    </td>
                    <td>
                        <small>Select Mute if you dont want to hear a sound when a new chat message comes in.</small>
                    </td>
                </tr>
            </table>
            <div class="clear-space">
            </div>
            <table cellpadding="10" cellspacing="10">
                <tr>
                    <td align="left" style="width: 185px;">
                        <span class="pad-right font-bold font-color-black">Chat Timeout</span>
                    </td>
                    <td style="width: 197px;">
                        <input type="text" id="tb_updateintervals_chatSettings" class="textEntry" maxlength="3"
                            onkeypress="ChatIntervalUpdate(event)" style="width: 35px;" /><span class="pad-left">minute(s)</span>
                        <input type="button" id="btn_updateintervals_chatSettings" class="margin-left input-buttons"
                            value="Update" />
                    </td>
                    <td>
                        <small>This value will represent the amount of time of inactivity before your chat status
                            turns to away. (Default is 10 minutes)</small>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <asp:Panel ID="emoticons_log_chatsettings" runat="server" style="display: none;">
    </asp:Panel>
</div>
