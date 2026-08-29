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

// Фильтр пользователей в модалке добавления участника
document.addEventListener("DOMContentLoaded", function () {
    var searchInput = document.getElementById("memberSearch");
    var select = document.getElementById("memberSelect");

    if (!searchInput || !select) return;

    var allUsers = JSON.parse(searchInput.getAttribute("data-users") || "[]");

    function renderUsers(filter) {
        var lowerFilter = (filter || "").toLowerCase();
        select.innerHTML = '<option value="">-- Выберите пользователя --</option>';

        var filtered = allUsers.filter(function (u) {
            var name = (u.name || "").toLowerCase().replace(/\s+/g, " ").trim();
            return name.indexOf(lowerFilter) > -1;
        });

        filtered.forEach(function (u) {
            var option = document.createElement("option");
            option.value = u.id;
            var displayName = u.name.replace(/\s+/g, " ").trim();
            option.textContent = displayName + " (ID: " + u.id + ")";
            select.appendChild(option);
        });
    }

    searchInput.addEventListener("input", function () {
        renderUsers(this.value);
    });

    renderUsers("");
});