<%@ control language="C#" autoeventwireup="true" inherits="Apps_Feedback, App_Web_pcermj0w" clientidmode="Static" %>
<div class="pad-all">
    <h3 class="float-left pad-top">Give me your feedback on the site.</h3>
    <div class="clear-space"></div>
    <textarea id="tb_feedback_overlay" class="textEntry" onfocus="if (this.value=='Enter your comments here...') { this.value=''; this.style.color='#353535'; }" onblur="if (this.value=='') { this.value='Enter your comments here...'; this.style.color='#B7B7B7'; }" style="width: 285px; height: 80px;">Enter your comments here...</textarea>
    <div class="clear-space"></div>
    <input type="button" class="input-buttons" value="Submit" onclick="openWSE.SubmitFeedback('tb_feedback_overlay');" />
    <input type="button" class="input-buttons" value="Clear" onclick="$('#tb_feedback_overlay').val('Enter your comments here...'); $('#tb_feedback_overlay').css('color', '#B7B7B7');" />
    <span class="float-right"><small>250 Character Max</small></span>
</div>
