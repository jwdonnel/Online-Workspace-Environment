$(document.body).on("click", ".plt-update", function() {
    var x = "<div id='update-element'><div class='update-element-align'>";
    x += "<div class='update-element-modal'>" + openWSE.loadingImg + "<h3 class='inline-block'>Updating. Please Wait...</h3></div></div></div>";
    $("#plt-load").append(x);
    $("#update-element").show();
});
//__doPostBack("UpdatePanel4_ProductLeadTime", "");

Sys.Application.add_load(function() {
    $(".date-picker-tb").datepicker();
    //RemoveUpdateModal();
});
$(document.body).on("click", "#tb_Date1_ProductLeadTime", function() {
    $("#tb_Date1_ProductLeadTime").datepicker("refresh");
});
$(document.body).on("click", "#tb_Date2_ProductLeadTime", function() {
    $("#tb_Date2_ProductLeadTime").datepicker("refresh");
});
$(document.body).on("click", "#tb_Date3_ProductLeadTime", function() {
    $("#tb_Date3_ProductLeadTime").datepicker("refresh");
});
$(document.body).on("click", "#tb_Date4_ProductLeadTime", function() {
    $("#tb_Date4_ProductLeadTime").datepicker("refresh");
});
$(document.body).on("click", "#tb_Date5_ProductLeadTime", function() {
    $("#tb_Date5_ProductLeadTime").datepicker("refresh");
});