const viewMode = document.querySelector(".card");
const editMode = document.getElementById("editMode");
const editButton = document.getElementById("editButton");
const cancelEdit = document.getElementById("cancelEdit");

editButton?.addEventListener("click", () => {
    viewMode.classList.add("d-none");
    editButton.classList.add("d-none");

    editMode.classList.remove("d-none");
});

cancelEdit?.addEventListener("click", () => {
    viewMode.classList.remove("d-none");
    editButton.classList.remove("d-none");

    editMode.classList.add("d-none");
});