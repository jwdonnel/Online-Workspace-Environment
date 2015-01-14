function getParameterByName(e) {
    e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var t = "[\\?&]" + e + "=([^&#]*)";
    var n = new RegExp(t);
    var r = n.exec(window.location.search);

    if (r == null)
        return "";
    else
        return decodeURIComponent(r[1].replace(/\+/g, " "));
}

function TryAgain() {
    var link = getParameterByName("aspxerrorpath");
    if (link == null || link == "") {
        link = window.location.href.split("ErrorPages/")[0] + "Default.aspx";
    }
    else {
        if (link.charAt(0) != '/') {
            link = "/" + link;
        }
        link = window.location.origin + link;
    }

    window.location = link;
}