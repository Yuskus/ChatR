document.addEventListener("DOMContentLoaded", function () {
    // Скролл к якорю постов при загрузке страницы с skip > 0
    var anchor = document.getElementById("posts-anchor");
    if (!anchor) return;

    var skip = parseInt(anchor.getAttribute("data-skip") || "0", 10);
    if (skip > 0) {
        anchor.scrollIntoView({ behavior: "auto" });
    }

    // Заполнение модального окна редактирования при открытии
    var editModal = document.getElementById("editModal");
    if (!editModal) return;

    editModal.addEventListener("show.bs.modal", function (event) {
        var button = event.relatedTarget;
        var postId = button.getAttribute("data-post-id");
        var content = button.getAttribute("data-post-content");

        editModal.querySelector("#EditPostId").value = postId;
        editModal.querySelector("#EditPostContent").value = decodeHTML(content);
    });
});

function decodeHTML(html) {
    var el = document.createElement("textarea");
    el.innerHTML = html;
    return el.value;
}
