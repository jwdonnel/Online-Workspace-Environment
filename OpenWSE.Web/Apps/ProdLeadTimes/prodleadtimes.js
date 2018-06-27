$(document.body).on("click", ".plt-update", function() {
    loadingPopup.RemoveMessage();
    loadingPopup.Message("Updating. Please Wait...");
});
//openWSE.CallDoPostBack("UpdatePanel4_ProductLeadTime", "");

Sys.Application.add_load(function() {
    $(".date-picker-tb").datepicker();
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