document.addEventListener("DOMContentLoaded", function () {
    var anchor = document.getElementById("posts-anchor");
    if (!anchor) return;

    var skip = parseInt(anchor.getAttribute("data-skip") || "0", 10);
    if (skip > 0) {
        anchor.scrollIntoView({ behavior: "auto" });
    }
});
