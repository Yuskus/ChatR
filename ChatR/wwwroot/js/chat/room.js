// Функция для настройки высоты textarea
function setupTextareaHeight(textarea) {
    const minRows = Number.parseInt(textarea.dataset.minRows, 10) || 1;
    const maxRows = Number.parseInt(textarea.dataset.maxRows, 10) || 3;

    // Временно сбрасываем высоту, чтобы корректно измерить scrollHeight
    textarea.style.height = 'auto';
    textarea.style.overflowY = 'hidden';

    const lineHeight = Number.parseFloat(getComputedStyle(textarea).lineHeight);
    const singleRowHeight = lineHeight || 20; // fallback в пикселях

    const paddingTop = Number.parseFloat(getComputedStyle(textarea).paddingTop);
    const paddingBottom = Number.parseFloat(getComputedStyle(textarea).paddingBottom);
    const totalPadding = paddingTop + paddingBottom;

    // Вычисляем высоту только по строкам
    const numberOfLines = textarea.value ? textarea.value.split('\n').length : 1;
    const desiredHeight = singleRowHeight * Math.min(maxRows, Math.max(minRows, numberOfLines)) + totalPadding;

    if (numberOfLines > maxRows) {
        textarea.style.height = `${singleRowHeight * maxRows + totalPadding}px`;
        textarea.style.overflowY = 'auto'; // Показать скролл, если больше 3 строк
    } else {
        textarea.style.height = `${desiredHeight}px`;
        textarea.style.overflowY = 'hidden';
    }
}

// Инициализация высоты textarea при загрузке
const messageInput = document.getElementById("messageInput");
const editMessageInput = document.getElementById("editMessageInput");
const sendButton = document.getElementById("sendButton");

setupTextareaHeight(messageInput);
setupTextareaHeight(editMessageInput);

sendButton.disabled = messageInput.value.trim() === '';

// Обновление высоты при изменении содержимого
messageInput.addEventListener('input', function () {
    setupTextareaHeight(this);
    sendButton.disabled = this.value.trim() === '';
});

editMessageInput.addEventListener('input', function () {
    setupTextareaHeight(this);
});

// Функция для отправки сообщения
async function sendMessage() {
    const content = messageInput.value.trim();
    if (!content) return;

    try {
        await connection.invoke("SendMessage", content, currentUserId, roomId);
        messageInput.value = '';
        setupTextareaHeight(messageInput);
    } catch (err) {
        console.error("Ошибка отправки:", err);
        alert("Не удалось отправить сообщение");
    }
}

// Функция для сохранения редактирования
async function saveEdit() {
    const idInput = document.getElementById("editMessageId").value;
    const content = editMessageInput.value.trim();
    if (!content) return;

    // Преобразуем в число
    const id = Number.parseInt(idInput, 10);
    if (Number.isNaN(id)) {
        console.error("Некорректный ID сообщения:", idInput);
        return;
    }

    try {
        await connection.invoke("UpdateMessage", id, content, currentUserId);
        exitEditMode();
    } catch (err) {
        console.error("Ошибка редактирования:", err);
    }
}

// Получение настройки отправки из localStorage
function getSendOnEnter() {
    const setting = localStorage.getItem('sendOnEnter');
    return setting === null ? true : setting === 'true'; // По умолчанию true (Enter)
}

// Сохранение настройки отправки в localStorage
function setSendOnEnter(value) {
    localStorage.setItem('sendOnEnter', value);
}

// Обработчик нажатия клавиш для отправки сообщения
messageInput.addEventListener('keydown', function (e) {
    const sendOnEnter = getSendOnEnter();

    // Проверяем, нажата ли Enter и не нажата ли Ctrl
    const isEnter = e.key === 'Enter';
    const isCtrlEnter = e.key === 'Enter' && e.ctrlKey;

    if ((sendOnEnter && isEnter && !e.shiftKey) || (!sendOnEnter && isCtrlEnter)) {
        e.preventDefault();
        sendMessage();
    }
});

// Обработчик нажатия клавиш для сохранения редактирования
editMessageInput.addEventListener('keydown', function (e) {
    const sendOnEnter = getSendOnEnter();

    // Проверяем, нажата ли Enter и не нажата ли Ctrl
    const isEnter = e.key === 'Enter';
    const isCtrlEnter = e.key === 'Enter' && e.ctrlKey;

    if ((sendOnEnter && isEnter && !e.shiftKey) || (!sendOnEnter && isCtrlEnter)) {
        e.preventDefault();
        saveEdit();
    }
});

// Добавление обработчиков для кнопки отправки при долгом нажатии
let pressTimer;

sendButton.addEventListener('mousedown', function (e) {
    if (e.button === 0) { // Левая кнопка мыши
        pressTimer = setTimeout(function () {
            showSendOptionMenu();
        }, 500); // Показываем меню после 500мс удержания
    }
});

sendButton.addEventListener('mouseup', function (e) {
    clearTimeout(pressTimer);
});

sendButton.addEventListener('mouseleave', function () {
    clearTimeout(pressTimer);
});

// Функция для отображения меню выбора способа отправки
function showSendOptionMenu() {
    const sendOnEnter = getSendOnEnter();
    const option = confirm(`Текущая настройка: ${sendOnEnter ? 'Enter' : 'Ctrl+Enter'}\n\nВыберите способ отправки сообщения:\n\nOK - Отправлять по Enter\nОтмена - Отправлять по Ctrl+Enter`);

    setSendOnEnter(option);
    alert(`Способ отправки сообщений изменён на: ${option ? 'Enter' : 'Ctrl+Enter'}`);
}

// Удаляем дублирующий обработчик, так как он уже добавлен ниже
// Обработчики отправки и редактирования остаются только внизу файла
const container = document.getElementById("messagesList");
const currentUserId = Number.parseInt(container.dataset.currentUserId);
const roomId = Number.parseInt(container.dataset.roomId);
const groupName = container.dataset.groupName;

// Прокрутка вниз
function scrollToBottom() {
    container.scrollTop = container.scrollHeight;
}

// Вызываем один раз при загрузке
scrollToBottom();

// Подключение к SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Создание элемента сообщения
function createMessageElement(msg) {
    const li = document.createElement("li");
    li.className = "list-group-item border-0";
    li.dataset.messageId = msg.id;

    const isMine = msg.userId === currentUserId;

    const date = new Date(msg.timestamp);

    const pad = (num) => num.toString().padStart(2, '0');

    const day = pad(date.getDate());
    const month = pad(date.getMonth() + 1); // Месяцы в JS — от 0
    const year = date.getFullYear();
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());

    const formattedDate = `${day}.${month}.${year} ${hours}:${minutes}`;

    let content = `<strong>
                            ${msg.userId
            ? `<a href="/Users/Profile/${msg.userId}" class="text-decoration-none">
                                       ${escapeHtml(msg.user?.firstName || 'DELETED')} ${escapeHtml(msg.user?.lastName || 'DELETED')}
                                   </a>`
            : `${escapeHtml(msg.user?.firstName || 'DELETED')} ${escapeHtml(msg.user?.lastName || 'DELETED')}`
        }
                           </strong>
                           <small class="text-muted">[${formattedDate}]</small>
                           <div class="multiline-text">${escapeHtml(msg.content)}</div>`;

    if (isMine) {
        content += `
                    <div class="mt-1">
                        <button class="btn btn-sm btn-warning edit-btn" 
                                data-id="${msg.id}" 
                                data-content="${escapeHtml(msg.content)}">Редактировать</button>
                        <button class="btn btn-sm btn-danger delete-btn" 
                                data-id="${msg.id}">Удалить</button>
                    </div>`;
    }

    li.innerHTML = content;
    return li;
}

// Экранирование HTML
function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

// Отправка сообщения
document.getElementById("sendButton").addEventListener("click", async function () {
    const input = document.getElementById("messageInput");
    const content = input.value.trim();
    if (!content) return;

    try {
        await connection.invoke("SendMessage", content, currentUserId, roomId);
        input.value = '';
    } catch (err) {
        console.error("Ошибка отправки:", err);
        alert("Не удалось отправить сообщение");
    }
});

// Редактирование
document.getElementById("saveEditButton").addEventListener("click", async function () {
    const idInput = document.getElementById("editMessageId").value;
    const content = document.getElementById("editMessageInput").value.trim();
    if (!content) return;

    // Преобразуем в число
    const id = Number.parseInt(idInput, 10);
    if (Number.isNaN(id)) {
        console.error("Некорректный ID сообщения:", idInput);
        return;
    }

    try {
        await connection.invoke("UpdateMessage", id, content, currentUserId);
        exitEditMode();
    } catch (err) {
        console.error("Ошибка редактирования:", err);
    }
});

document.getElementById("cancelEditButton").addEventListener("click", exitEditMode);

function exitEditMode() {
    document.getElementById("editForm").classList.add("d-none");
    document.getElementById("messageInput").focus();
}

// Обработчики событий (делегирование)
document.getElementById("messagesList").addEventListener("click", function (e) {
    if (e.target.classList.contains("delete-btn")) {
        const id = Number.parseInt(e.target.dataset.id, 10);
        if (Number.isNaN(id)) {
            console.error("Некорректный ID сообщения");
            return;
        }

        if (confirm("Удалить сообщение?")) {
            connection.invoke("DeleteMessage", id, currentUserId)
                .catch(err => console.error("Ошибка удаления:", err));
        }
    }

    if (e.target.classList.contains("edit-btn")) {
        const id = Number.parseInt(e.target.dataset.id, 10);
        if (Number.isNaN(id)) {
            console.error("Некорректный ID сообщения");
            return;
        }

        const content = e.target.dataset.content;
        const editInput = document.getElementById("editMessageInput");

        document.getElementById("editMessageId").value = id;
        editInput.value = content;

        // Важно: обновить высоту сразу после установки значения!
        setupTextareaHeight(editInput);

        document.getElementById("editForm").classList.remove("d-none");
        document.getElementById("messageInput").blur();
        editInput.focus(); // Опционально: фокус в поле редактирования
    }
});

// SignalR события
connection.on("ReceiveMessage", function (id, content, userId, userName, timestamp) {
    const [firstName, lastName] = userName.split(' ');

    const msg = {
        id: id,
        content: content,
        userId: userId,
        user: { firstName: firstName, lastName: lastName || '' },
        timestamp: timestamp
    };

    const el = createMessageElement(msg);
    container.appendChild(el);
    scrollToBottom();

    setupTextareaHeight(messageInput);
});

connection.on("MessageDeleted", function (id) {
    const el = document.querySelector(`[data-message-id="${id}"]`);
    if (el) el.remove();
});

connection.on("MessageUpdated", function (id, content) {
    const el = document.querySelector(`[data-message-id="${id}"]`);
    if (el) {
        const contentDiv = el.querySelector("div");
        if (contentDiv) contentDiv.textContent = content;

        const editButton = el.querySelector(".edit-btn");
        if (editButton) editButton.dataset.content = content;
    }
});

// Подключение
async function start() {
    try {
        await connection.start();
        console.log("SignalR подключён");
        await connection.invoke("JoinGroup", groupName);
    } catch (err) {
        console.error("Ошибка подключения:", err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    console.log("Подключение потеряно. Переподключение...");
    await start();
});

start();

// Получение токена из куки
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}