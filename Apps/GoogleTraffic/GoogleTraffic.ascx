<%@ control language="C#" autoeventwireup="true" inherits="Apps_GoogleTraffic_GoogleTraffic, App_Web_ksl1q0s3" clientidmode="Static" %>
<div id="googletraffic-load" class="main-div-app-bg">
    <div class="pad-all app-title-bg-color">
        <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
        <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
    </div>
    <iframe id="iframe_googletraffic" class="iFrame-apps" width="100%" frameborder="0" marginheight="0" marginwidth="0" src="Apps/GoogleTraffic/googletraffic.html"></iframe>
</div>
