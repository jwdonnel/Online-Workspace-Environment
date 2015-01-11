$(document).ready(function () {
    setTimeout(function () {
        resizePLTViewer();
    }, 300);
});

$(document.body).on("dblclick", ".app-head-dblclick", function () {
    setTimeout(function () {
        resizePLTViewer();
    }, 300);
});

$(document.body).on("click", "#app-pltnoedit .maximize-button-app", function () {
    setTimeout(function () {
        resizePLTViewer();
    }, 300);
});

$(document.body).on("resize", "#app-pltnoedit", function (e) {
    resizePLTViewer();
});

function resizePLTViewer() {
    var height = document.getElementById("app-pltnoedit").offsetHeight - 150;
    $("#leadtimeViewer_relative").css("height", height + "px");
    fontSize =  (height * .1) / 1.5;
    if (fontSize < 20) {
        var fontSize = 20;
    }
    $("#leadtimeViewer_absolute h1").css("font-size", fontSize + "px");

    var height2 = $("#leadtimeViewer_absolute").height() / 2;
    $("#leadtimeViewer_absolute").css("margin-top", "-" + height2 + "px");
}