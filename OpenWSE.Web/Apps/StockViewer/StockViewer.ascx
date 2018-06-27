<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StockViewer.ascx.cs" Inherits="Apps_StockViewer_StockViewer" ClientIDMode="Static" %>
<div class="pad-all app-title-bg-color">
    <div class="float-left">
        <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
        <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
    </div>
    <input type="button" class="input-buttons float-right no-margin"
        value="Add" onclick="stockviewerApp.OpenAddStockViewerModal();" title="Add Stock Viewer" />
    <div class="clear"></div>
</div>
<div id="stockviewer_holder"></div>
<div id="add-stockviewer-element" class="Modal-element outside-main-app-div">
    <div class="Modal-overlay">
        <div class="Modal-element-align">
            <div class="Modal-element-modal" data-setwidth="455">
                <div class="ModalHeader">
                    <div>
                        <div class="app-head-button-holder-admin">
                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'add-stockviewer-element', '');return false;"
                                class="ModalExitButton"></a>
                        </div>
                        <span class="Modal-title"></span>
                    </div>
                </div>
                <div class="ModalScrollContent">
                    <div class="ModalPadContent">
                        <div class="float-left pad-right-big margin-right-big">
                            <div class="modal-descDivs-item">Symbol</div>
                            <input type="text" id="tb_stockviewer_addSymbol" class="textEntry" onkeypress="stockviewerApp.KeyPressAddStockViewer(event);" style="width: 100px; text-transform: uppercase;" />
                        </div>
                        <div class="float-left pad-right-big margin-right-big">
                            <div class="modal-descDivs-item">Default Interval</div>
                            <select id="select_stockviewer_interval" style="width: 100px;">
                                <option value="1">1 minute</option>
                                <option value="3">3 minutes</option>
                                <option value="5">5 minutes</option>
                                <option value="15">15 minutes</option>
                                <option value="30">30 minutes</option>
                                <option value="60">1 hour</option>
                                <option value="120">2 hours</option>
                                <option value="180">3 hours</option>
                                <option value="240">4 hours</option>
                                <option value="D">1 day</option>
                                <option value="W">1 week</option>
                            </select>
                        </div>
                        <div class="float-left">
                            <div class="modal-descDivs-item">Timezone</div>
                            <select id="select_stockviewer_timezone" style="width: 100px;">
                                <option value="Etc/UTC">UTC</option>
                                <option value="exchange">Exchange</option>
                            </select>
                        </div>
                        <div class="clear-space-five"></div>
                        <div class="float-left pad-right-big margin-right-big">
                            <div class="modal-descDivs-item">Color Theme</div>
                            <select id="select_stockviewer_theme" style="width: 100px;">
                                <option value="White">White</option>
                                <option value="Grey">Grey</option>
                                <option value="Blue">Blue</option>
                                <option value="Black">Black</option>
                            </select>
                        </div>
                        <div class="float-left pad-right-big margin-right-big">
                            <div class="modal-descDivs-item">Bar's Style</div>
                            <select id="select_stockviewer_barstyle" style="width: 100px;">
                                <option value="0">Bars</option>
                                <option value="1">Candles</option>
                                <option value="9">Hollow Candles</option>
                                <option value="8">Heikin Ashi</option>
                                <option value="2">Line</option>
                                <option value="3">Area</option>
                                <option value="4">Renko</option>
                                <option value="7">Line Break</option>
                                <option value="5">Kagi</option>
                                <option value="6">Point and Figure</option>
                            </select>
                        </div>
                        <div class="float-left">
                            <div class="modal-descDivs-item">Language</div>
                            <select id="select_stockviewer_language" style="width: 100px;">
                                <option value="en">English</option>
                            </select>
                        </div>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <div class="float-left pad-right-big margin-right-big stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showtopbar" /><label for="cb_stockviewer_showtopbar">&nbsp;Show top toolbar</label>
                            </div>
                        </div>
                        <div class="float-left stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showcalendar" /><label for="cb_stockviewer_showcalendar">&nbsp;Show Calendar</label>
                            </div>
                        </div>
                        <div class="clear-space"></div>
                        <div class="float-left pad-right-big margin-right-big stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showbottombar" /><label for="cb_stockviewer_showbottombar">&nbsp;Show bottom toolbar</label>
                            </div>
                        </div>
                        <div class="float-left stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showdetails" /><label for="cb_stockviewer_showdetails">&nbsp;Show Details</label>
                            </div>
                        </div>
                        <div class="clear-space"></div>
                        <div class="float-left pad-right-big margin-right-big stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showstocktwits" /><label for="cb_stockviewer_showstocktwits">&nbsp;Show StockTwits</label>
                            </div>
                        </div>
                        <div class="float-left stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showheadlines" /><label for="cb_stockviewer_showheadlines">&nbsp;Show Headlines</label>
                            </div>
                        </div>
                        <div class="clear-space"></div>
                        <div class="float-left pad-right-big margin-right-big stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showdrawingbar" /><label for="cb_stockviewer_showdrawingbar">&nbsp;Show drawing bar</label>
                            </div>
                        </div>
                        <div class="float-left stockviewer-checkbox-editadd">
                            <div class="radiobutton-style">
                                <input type="checkbox" id="cb_stockviewer_showhotlist" /><label for="cb_stockviewer_showhotlist">&nbsp;Show Hotlist</label>
                            </div>
                        </div>
                        <div class="clear-space"></div>
                    </div>
                </div>
                <div class="ModalButtonHolder">
                    <input type="button" id="btn_stockviewer_add" class="input-buttons modal-ok-btn" value="Add" onclick="stockviewerApp.AddStockViewer()" />
                    <input type="button" id="btn_stockviewer_update" class="input-buttons modal-ok-btn" value="Update" style="display: none;" />
                    <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'add-stockviewer-element', '');" />
                </div>
            </div>
        </div>
    </div>
</div>
<input type="hidden" data-scriptelement="true" data-tagname="link" data-tagtype="text/css" data-tagrel="stylesheet" data-tagsrc="~/Apps/StockViewer/stockviewer.css" />
<input type="hidden" data-scriptelement="true" data-tagname="script" data-tagtype="text/javascript" data-tagsrc="~/Apps/StockViewer/stockviewer.js" />
