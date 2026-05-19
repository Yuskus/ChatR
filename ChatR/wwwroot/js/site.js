// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Передача roomId в форму
document.querySelectorAll('[data-bs-target="#addMemberModal"]').forEach(btn => {
    btn.addEventListener('click', function () {
        const roomId = this.dataset.roomId;
        document.getElementById('modalRoomId').value = roomId;
    });
});