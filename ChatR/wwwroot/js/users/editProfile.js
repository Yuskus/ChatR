const deleteAccountButton = document.getElementById("deleteForm").querySelector('.delete-btn');

deleteAccountButton.addEventListener("click", function (e) {
    e.preventDefault();
    
    if (confirm('Вы уверены, что хотите удалить аккаунт? Это действие нельзя отменить.')) {
        const form = e.target.closest('form');
        if (form) {
            form.submit();
        }
    }
});