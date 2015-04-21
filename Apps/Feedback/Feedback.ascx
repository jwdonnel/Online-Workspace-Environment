<%@ control language="C#" autoeventwireup="true" inherits="Apps_Feedback, App_Web_nbvx505l" clientidmode="Static" %>
<div id="feedback-load" class="main-div-app-bg">
    <div class="pad-all">
        <h3 class="float-left pad-top">Give me your feedback on the site.</h3>
        <div class="clear-space"></div>
        <asp:TextBox ID="tb_feedback_overlay" runat="server" TextMode="MultiLine" Height="70px" Width="265px" MaxLength="250" BorderColor="#DDDDDD" BorderWidth="1px" BorderStyle="Solid" CssClass="border-radius-5 pad-all" Font-Names="Arial" Font-Size="14px" Text="Enter your comments here..." ForeColor="#B7B7B7" onfocus="if (this.value=='Enter your comments here...') { this.value=''; this.style.color='#353535'; }" onblur="if (this.value=='') { this.value='Enter your comments here...'; this.style.color='#B7B7B7'; }"></asp:TextBox>
        <div class="clear-space"></div>
        <input type="button" class="input-buttons" value="Submit" onclick="openWSE.SubmitFeedback('tb_feedback_overlay');" />
        <input type="button" class="input-buttons" value="Clear" onclick="$('#tb_feedback_overlay').val('Enter your comments here...'); $('#tb_feedback_overlay').css('color', '#B7B7B7');" />
        <span class="float-right"><small>250 Character Max</small></span>
    </div>
</div>
