"use strict";

let connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();

let currentUser = null;

// Элементы DOM
const setUserButton = document.getElementById("setUserButton");
const currentUserDisplay = document.getElementById("currentUserDisplay");
const userModal = document.getElementById("userModal");
const userNameInput = document.getElementById("userNameInput");
const cancelUserButton = document.getElementById("cancelUserButton");
const confirmUserButton = document.getElementById("confirmUserButton");
const sendButton = document.getElementById("sendButton");
const messageInput = document.getElementById("messageInput");
const messagesList = document.getElementById("messagesList");

function setUserName(name) {
    if (!name || name.trim().length === 0) {
        alert("Имя не может быть пустым");
        return false;
    }

    currentUser = name.trim();
    currentUserDisplay.textContent = `Пользователь: ${currentUser}`;
    setUserButton.disabled = true;
    sendButton.disabled = false;
    messageInput.focus();

    userModal.style.display = "none";
    localStorage.setItem("chatUserName", currentUser);

    return true;
}

// Загрузка истории
async function loadMessageHistory() {
    try {
        const response = await fetch('/api/chat/history');
        if (!response.ok) throw new Error("Не удалось загрузить историю");

        const history = await response.json();

        messagesList.innerHTML = '';

        history.forEach(msg => {
            const isSelf = msg.user === currentUser;
            const messageElement = createMessageElement(msg, isSelf);
            messagesList.appendChild(messageElement);
        });
    } catch (error) {
        console.error("Ошибка загрузки истории:", error);
        alert("Не удалось загрузить историю чата.");
    }
}

// Отправка сообщения
async function sendMessage(user, message) {
    try {
        await connection.invoke("SendMessage", user, message);
    } catch (err) {
        console.error("Ошибка отправки:", err);
        alert("Не удалось отправить сообщение. Проверьте соединение.");
    }
}

// Удаление сообщения
async function deleteMessage(messageId) {
    if (!confirm("Вы уверены, что хотите удалить это сообщение?")) {
        return;
    }

    try {
        await connection.invoke("DeleteMessage", messageId);
    } catch (error) {
        console.error("Ошибка удаления:", error);
        alert("Не удалось удалить сообщение.");
    }
}

// Создание элемента сообщения
function createMessageElement(msg, isSelf) {
    const li = document.createElement("li");
    li.className = `message-item ${isSelf ? 'message-self' : 'message-other'}`;
    li.dataset.messageId = msg.id;

    const header = document.createElement("div");
    header.className = "message-header";
    header.textContent = msg.user;

    const content = document.createElement("div");
    content.className = "message-content";
    content.textContent = msg.message || msg.content;

    const timestamp = document.createElement("div");
    timestamp.className = "message-timestamp";
    const date = new Date(msg.timestamp);
    timestamp.textContent = `[${date.toLocaleDateString()} ${date.toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    })}]`;

    if (isSelf) {
        const deleteBtn = document.createElement("button");
        deleteBtn.className = "delete-btn";
        deleteBtn.innerHTML = "🗑️";
        deleteBtn.title = "Удалить";
        deleteBtn.addEventListener("click", () => deleteMessage(msg.id));
        li.appendChild(deleteBtn);
    }

    li.appendChild(header);
    li.appendChild(content);
    li.appendChild(timestamp);
    return li;
}

// === Обработчики SignalR ===

// Обработчик ReceiveMessage
connection.on("ReceiveMessage", function (id, timestamp, user, message) {
    const isSelf = user === currentUser;
    const msg = { id, timestamp, user, message };
    const messageElement = createMessageElement(msg, isSelf);
    messagesList.appendChild(messageElement);
});

// Обработка удаления
connection.on("MessageDeleted", function (messageId) {
    const element = document.querySelector(`li[data-message-id="${messageId}"]`);
    if (element) {
        element.style.opacity = "0.5";
        element.style.textDecoration = "line-through";
        setTimeout(() => element.remove(), 1000);
    }
});

// === Жизненный цикл подключения ===

connection.onreconnecting(() => {
    console.warn("Переподключение...");
    sendButton.disabled = true;
});

connection.onreconnected(() => {
    console.log("Переподключено");
    sendButton.disabled = false;
    loadMessageHistory(); // Перезагружаем историю
});

connection.onclose(async () => {
    console.warn("Соединение закрыто");
    sendButton.disabled = true;
    setTimeout(() => connection.start(), 5000);
});

// === Старт ===

async function start() {
    try {
        await connection.start();
        console.log("SignalR подключён");
        sendButton.disabled = false;
        await loadMessageHistory(); // Загружаем историю
    } catch (err) {
        console.error("Ошибка подключения:", err);
        setTimeout(start, 5000);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const savedName = localStorage.getItem("chatUserName");
    if (savedName) setUserName(savedName);

    start();
});

// === Обработчики UI ===

setUserButton.addEventListener("click", () => {
    userModal.style.display = "flex";
    userNameInput.value = "";
    setTimeout(() => userNameInput.focus(), 100);
});

cancelUserButton.addEventListener("click", () => {
    userModal.style.display = "none";
});

confirmUserButton.addEventListener("click", () => {
    const name = userNameInput.value;
    setUserName(name);
});

sendButton.addEventListener("click", () => {
    if (!currentUser) {
        alert("Сначала установите имя!");
        return;
    }

    const text = messageInput.value.trim();
    if (!text) {
        alert("Введите сообщение!");
        return;
    }

    sendMessage(currentUser, text);
    messageInput.value = "";
});

messageInput.addEventListener("keypress", e => {
    if (e.key === "Enter" && currentUser) sendButton.click();
});

globalThis.addEventListener("click", e => {
    if (e.target === userModal) userModal.style.display = "none";
});