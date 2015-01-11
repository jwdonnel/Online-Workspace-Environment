function DeleteFeed(id) {
    if (id != "") {
        $("#hf_delete_TwitterFeed").val(id);
        __doPostBack("hf_delete_TwitterFeed", "");
    }
}

function AddFeed() {
    isEditMode = false;
    $("#lbl_errorTwitter").html("");
    $("#btn_add").show();
    $("#btn_update").hide();
    $("#hf_editID").val("");
    $("#tb_title").val("");
    $("#tb_caption").val("");
    $("#tb_twitteraccount").val("");
    openWSE.LoadModalWindow(true, 'TwitterAdd-element', "Add Twitter Feed");
}

isEditMode = false;
function EditFeed(id) {
    if (id != "") {
        isEditMode = true;
        $("#lbl_errorTwitter").html("");
        $("#btn_update").show();
        $("#btn_add").hide();
        openWSE.LoadModalWindow(true, 'TwitterAdd-element', "Edit Twitter Feed");
        $("#hf_edit_TwitterFeed").val(id);
        __doPostBack("hf_edit_TwitterFeed", "");
    }
}

var timeoutInt;
$(window).load(function () {
    timeoutInt = setTimeout('refresh()', (120000 / 2));
    $(window).resize();
});

var prm = Sys.WebForms.PageRequestManager.getInstance();
prm.add_endRequest(function (sender, args) {
    clearTimeout(timeoutInt);
    timeoutInt = setTimeout('refresh()', (120000 / 2));
    $(window).resize();

    if (isEditMode) {
        $("#btn_update").show();
        $("#btn_add").hide();
    }
    else {
        $("#btn_update").hide();
        $("#btn_add").show();
    }
});

function AddHrefTarget_TwitterFeeds() {
    $(".ajax_twitter").find("a").each(function () {
        var $this = $(this);
        if ($this.attr("target") != "_blank") {
            $this.attr("target", "_blank");
        }
    });
}

function refresh() {
    var d = new Date();
    document.getElementById("hf_updateall").value = d;
    __doPostBack("hf_updateall", "");
}

$(window).resize(function() {
    var small = ($(window).width() / 3) - 10;
    var large = ($(window).width() / 2) - 10;
    $(".Small").css("width", small + "px");
    $(".Large").css("width", large + "px");
});