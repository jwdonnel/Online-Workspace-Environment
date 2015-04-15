<%@ page title="License Manager" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_LicenseManager, App_Web_et3auwnc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .cc-type span
        {
            font-weight: normal!important;
            padding-right: 0px!important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:UpdatePanel ID="updatepnl1" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnl_nonTrialVersion" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-ctrl">
                            <h3 class="float-left margin-right-big">
                                <span class="font-bold pad-right-sml">License Status :</span>
                                <asp:Label ID="lbl_licenseStatus" runat="server" Text=""></asp:Label></h3>
                            <asp:Button ID="lbtn_tryValidate" runat="server" CssClass="margin-left RandomActionBtns input-buttons float-left"
                                Text="Try to validate" OnClick="lbtn_tryValidate_Clicked" Style="margin-top: -5px;"></asp:Button>
                            <div class="clear-space-five">
                            </div>
                            <asp:Label ID="lbl_licenseInvalidHint" runat="server" Text="An invalid or expired license file means you will not be able to access any feature of this site until this is validated."
                                Enabled="false" Visible="false" Style="font-size: 12px; font-style: italic;"></asp:Label>
                            <div class="clear">
                            </div>
                            <div class="table-settings-box no-border" style="padding-bottom: 0px!important; margin-bottom: 0px!important;">
                                <div class="td-settings-title">
                                    License File Contents
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_licenseFileContents" runat="server" Enabled="false" Visible="false">
                                        <asp:Panel ID="pnl_licenseContents" runat="server" CssClass="pad-left-sml pad-top-big">
                                        </asp:Panel>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Licenses are good for a single domain.
            Your license file will contain all the information you need to validate your request.
            Once the license is validated, it will not have to repeat this procedure until the
            site is restarted or the session state has ended. Site must have an active internet connection in order to validate a license.
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_trialVersion" runat="server" Enabled="false" Visible="false">
                    <div class="pad-all">
                        <h4>Enter the information below to start your <span id="trialLength" runat="server"></span>full version<span id="trialtext" runat="server"></span>. Please note, the <b>Website Url</b> is the url to which this trial copy will be hosted from. (Example: http://mydomain.com)</h4>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_trialVersion_Info" runat="server" DefaultButton="btn_SubmitTrial" CssClass="float-left pad-right-big margin-right-big">
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td align="right" style="width: 125px;">
                                        <h4 class="font-bold pad-right">Website Name</h4>
                                    </td>
                                    <td>
                                            <asp:TextBox ID="txt_WebsiteName" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_WebsiteName" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <h4 class="font-bold pad-right">Website Url</h4>
                                    </td>
                                    <td>
                                            <asp:TextBox ID="txt_WebsiteUrl" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:LinkButton ID="lbtn_useDefaultUrl" runat="server" Font-Size="Small" Text="Use Default" CssClass="margin-left margin-right" CausesValidation="false" OnClick="lbtn_useDefaultUrl_Click"></asp:LinkButton>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_WebsiteUrl" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <h4 class="font-bold pad-right">Email Address</h4>
                                    </td>
                                    <td>
                                            <asp:TextBox ID="txt_emailAddress" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_emailAddress" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>
                                        <asp:Button ID="btn_SubmitTrial" runat="server" OnClick="btn_SubmitTrial_Clicked" OnClientClick="GeneratingLicense();"
                                            Text="Generate License" CssClass="input-buttons no-margin float-right" Style="margin-right: 100px!important;" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div class="float-left pad-left-big pad-top-big margin-top-big">
                            <img alt="soLogo" src="../../Standard_Images/About Logos/openwse.png" />
                        </div>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnlCCLicenseType1" runat="server">
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <h4>A license will be automatically generated and uploaded to your computer. You must have an internet connection for activation.<br />
                            During the trial version, all postbacks will check the expiration date of the trial. Once the trial is up, you will be able to purchase the full version.<br />
                            A trial license can only be generated once per website url/domain. Once that trial has expired, you will either have to change your website url/domain or purchase the full version.
                            <br />
                            <br />
                        </h4>
                        <h4>If you require help, please email John Donnelly at <a href="mailto:jwdonnel@gmail.com" target="_blank">jwdonnel@gmail.com</a>.
                        </h4>
                    </div>
                    <div class="pad-all">
                        <div class="clear-space"></div>
                        <h4>If you like what you see, you can purchase the full version by clicking <a href="LicenseManager.aspx?purchase=true">here</a>.<br />
                            The full version will feature a lifetime license for one domain.
                            <br />
                            Payments will be made using <a href="https://www.paypal.com/" target="_blank">PayPal
                            <img alt="paypal" src="../../Standard_Images/About Logos/Paypal.png" style="height: 18px;" /></a></h4>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_purchaseVersion" runat="server" Enabled="false" Visible="false">
                    <div class="pad-all">
                        <asp:LinkButton ID="lbtn_goFoward_purchased" runat="server" Text="Back to License Info" CausesValidation="false" OnClick="lbtn_goFoward_purchased_Click" Enabled="false" Visible="false"></asp:LinkButton>
                        <div class="clear-space"></div>
                        <h4>The OpenWSE is a large platform with many features and capabilities.<br />
                            With this license, you will get full access to everything the platform has to offer along with any updates and fixes for free.<br />
                            Click on the Buy Now button below to get your full version license.</h4>
                        <div class="clear-space">
                        </div>
                        <div class="float-left pad-right-big margin-right-big pad-top-big margin-top-big">
                            <iframe src="<%=OpenWSE.Core.Licensing.CheckLicense.LicenseSite %>iframes/PayPal_BuyNow.aspx" class="float-left margin-left" style="width: 205px; height: 90px; border: none; overflow: hidden;"></iframe>
                        </div>
                        <div class="float-left pad-left-big pad-top-big margin-top-big">
                            <img alt="soLogo" src="../../Standard_Images/About Logos/openwse.png" />
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_ValCodePurchased" runat="server" CssClass="pad-top-big margin-top-big pad-bottom-big margin-bottom-big" DefaultButton="btn_ValidationCodePurchased">
                            <div class="clear-space">
                            </div>
                            <h4>If you have already purchased the OpenWSE and have your validation code, enter it below and click Activate.<br />
                                Your validation code is the same as your receipt id from PayPal without the hyphens(-).
                            </h4>
                            <div class="clear-space">
                            </div>
                            <span class="font-bold pad-right">Validation Code:</span><asp:TextBox ID="txt_ValidationCodePurchased" runat="server" CssClass="textEntry margin-right"></asp:TextBox><asp:Button ID="btn_ValidationCodePurchased" runat="server" Text="Activate" CssClass="input-buttons" OnClick="btn_ValidationCodePurchased_Click" />
                            <div class="clear-space"></div>
                            <asp:Label ID="lbl_valError" runat="server" ForeColor="Red" Text=""></asp:Label>
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnlCCLicenseType2" runat="server">
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <h4>A license will be automatically generated and uploaded to your computer. You must have an internet connection for activation.<br />
                            If you have any problems with getting your license validated, please email John Donnelly at <a href="mailto:jwdonnel@gmail.com" target="_blank">jwdonnel@gmail.com</a>. Make sure to include your validation code in the email.<br />
                            An email will be sent to you as soon as the issue is found or resolved.<br />
                            <br />
                        </h4>
                        <h4>If you require help, please email John Donnelly at <a href="mailto:jwdonnel@gmail.com" target="_blank">jwdonnel@gmail.com</a>.
                        </h4>
                    </div>
                    <div class="pad-all">
                        <div class="clear-space"></div>
                        <h4>The full version will feature a lifetime license for one domain.
                            <br />
                            Payments will be made using <a href="https://www.paypal.com/" target="_blank">PayPal
                            <img alt="paypal" src="../../Standard_Images/About Logos/Paypal.png" style="height: 18px;" /></a></h4>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_purchaseFinishVersion" runat="server" Enabled="false" Visible="false">
                    <div class="pad-all">
                        <asp:LinkButton ID="lbtn_goBack_purchased" runat="server" Text="Go Back" CausesValidation="false" OnClick="lbtn_goBack_purchased_Click"></asp:LinkButton>
                        <div class="clear-space"></div>
                        <h4>Enter the information below to start your full version. Please note, the <b>Website Url</b> is the url to which this trial copy will be hosted from. (Example: http://mydomain.com)</h4>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_fullVersionInfo" runat="server" DefaultButton="btn_SubmitFull" CssClass="float-left pad-right-big margin-right-big">
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td align="right" style="width: 125px;">
                                        <h4 class="font-bold pad-right">Website Name:</h4>
                                    </td>
                                    <td>
                                        <div class="pad-bottom">
                                            <asp:TextBox ID="txt_WebsiteName_Purchased" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_WebsiteName_Purchased" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <h4 class="font-bold pad-right">Website Url:</h4>
                                    </td>
                                    <td>
                                        <div class="pad-bottom">
                                            <asp:TextBox ID="txt_WebsiteUrl_Purchased" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:LinkButton ID="lbtn_useDefaultUrl_Purchased" runat="server" Font-Size="Small" Text="Use Default" CssClass="margin-left margin-right" CausesValidation="false" OnClick="lbtn_useDefaultUrl_Purchased_Click"></asp:LinkButton>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_WebsiteUrl_Purchased" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <h4 class="font-bold pad-right">Email Address:</h4>
                                    </td>
                                    <td>
                                        <div class="pad-bottom">
                                            <asp:TextBox ID="txt_emailAddress_Purchased" runat="server" CssClass="textEntry"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_emailAddress_Purchased" ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>
                                        <asp:Button ID="btn_SubmitFull" runat="server" OnClick="btn_SubmitFull_Clicked" OnClientClick="GeneratingLicense_Purchased();"
                                            Text="Generate License" CssClass="input-buttons no-margin float-right" Style="margin-right: 100px!important;" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div class="float-left pad-left-big pad-top-big margin-top-big">
                            <img alt="soLogo" src="../../Standard_Images/About Logos/openwse.png" />
                        </div>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnlCCLicenseType3" runat="server">
                        </asp:Panel>
                        <div class="clear-space"></div>
                        <h4>A license will be automatically generated and uploaded to your computer. You must have an internet connection for activation.<br />
                            During the trial version, all postbacks will check the expiration date of the trial. Once the trial is up, you will be able to purchase the full version.<br />
                            A trial license can only be generated once per website url/domain. Once that trial has expired, you will either have to change your website url/domain or purchase the full version.
                            <br />
                            <br />
                        </h4>
                        <h4>If you require help, please email John Donnelly at <a href="mailto:jwdonnel@gmail.com" target="_blank">jwdonnel@gmail.com</a>.
                        </h4>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Upload License File
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:FileUpload ID="fu_newLicenseFile" runat="server" />
                <div class="clear-space">
                </div>
                <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons" OnClientClick="return ConfirmUploadLicense(this);" Text="Upload License" />
            </div>
            <div class="td-settings-desc">
                You only need to upload a License
    File if either your current license has expired, missing, corrupted, or if site
    name and/or domain has changed.
            </div>
        </div>
    </div>
    <script type="text/javascript">
        function GeneratingLicense() {
            openWSE.RemoveUpdateModal();
            try {
                if (($.trim($("#<% =txt_WebsiteName.ClientID %>").val()) != "") && ($.trim($("#<% =txt_WebsiteUrl.ClientID %>").val()) != "") && ($.trim($("#<% =txt_emailAddress.ClientID %>").val()) != "")) {
                    openWSE.LoadingMessage1("Sending Information...");
                }
            }
            catch (evt) { }
        }

        function ConfirmUploadLicense(_this) {
            openWSE.ConfirmWindow("Are you sure you want to upload this new License File? Doing so will overwrite the current license.",
                function () {
                    openWSE.LoadingMessage1('Uploading. Please Wait...');
                    var id = $(_this).attr("id");
                    __doPostBack(id, "");
                }, null);

            return false;
        }

        function GeneratingLicense_Purchased() {
            openWSE.RemoveUpdateModal();
            try {
                if (($.trim($("#<% =txt_WebsiteName_Purchased.ClientID %>").val()) != "") && ($.trim($("#<% =txt_WebsiteUrl_Purchased.ClientID %>").val()) != "") && ($.trim($("#<% =txt_emailAddress_Purchased.ClientID %>").val()) != "")) {
                    openWSE.LoadingMessage1("Sending Information...");
                }
            }
            catch (evt) { }
        }
    </script>
</asp:Content>
