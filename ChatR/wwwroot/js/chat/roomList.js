const roomList = document.getElementById("roomList");

roomList?.addEventListener("click", async function (e) {
    e.preventDefault();

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
    
    if (e.target.classList.contains("join-btn")) {
        const id = Number.parseInt(e.target.dataset.id, 10);
        if (Number.isNaN(id)) {
            console.error("Некорректный ID комнаты");
            return;
        }
        
        e.target.closest('form').submit();
    }
});