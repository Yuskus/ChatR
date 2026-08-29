const roomList = document.getElementById("roomList");

roomList?.addEventListener("click", function (e) {
    if (e.target.classList.contains("delete-btn")) {
        const id = Number.parseInt(e.target.dataset.id, 10);
        if (Number.isNaN(id)) {
            console.error("Некорректный ID комнаты");
            return;
        }

        if (confirm('Удалить комнату?')){
            e.target.closest('form').submit();
        }
    }

    if (e.target.classList.contains("leave-btn") ||
        e.target.classList.contains("delete-btn") ||
        e.target.closest(".btn-outline-success[data-room-id]")) {
        e.stopPropagation();
    }
});
