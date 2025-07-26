// Создаем подключение к хабу
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    // .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Список подключенных пользователей
let usersList = {};

// Обработчик получения сообщений
connection.on("ReceiveMessage", function (user, message) {
    const encodedMsg = user + ": " + message;
    const li = document.createElement("li");
    li.classList.add("list-group-item");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

// Обработчик подключения нового пользователя
connection.on("UserConnected", function (username) {
    // users[connectionId] = connectionId.substr(0, 5); // Используем часть ID как имя для простоты
    // // updateUserList();

    const li = document.createElement("li");
    li.classList.add("list-group-item", "list-group-item-success");
    li.textContent = `Пользователь ${username} подключился`;
    document.getElementById("messagesList").appendChild(li);
});

// Обработчик отключения пользователя
connection.on("UserDisconnected", function (username) {
    // const userName = users[connectionId] || "Неизвестный";
    // delete users[connectionId];
    // updateUserList();

    const li = document.createElement("li");
    li.classList.add("list-group-item", "list-group-item-danger");
    li.textContent = `Пользователь ${username} отключился`;
    document.getElementById("messagesList").appendChild(li);
});

// Обновление списка пользователей
function updateUserList() {
    const userList = document.getElementById("userList");
    userList.innerHTML = "";

    for (const id in users) {
        const li = document.createElement("li");
        li.classList.add("list-group-item");
        li.textContent = users[id];
        userList.appendChild(li);
    }
}

// Обработчик клика по кнопке отправки
document.getElementById("sendButton").addEventListener("click", function (event) {
    const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;

    if (user && message) {
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

        document.getElementById("messageInput").value = "";
    } else {
        alert("Пожалуйста, введите имя и сообщение");
    }

    event.preventDefault();
});

// Начинаем соединение
connection.start()
//     .then(function (){
//     const userid = document.getElementById("userId").value;
//     const username = document.getElementById("userInput").value;
//
//     console.log("Registering user:", { userid, username });
//     // Регистрируем пользователя на сервере
//     connection.invoke("RegisterUser", userid, username).catch(function (err) {
//         return console.error(err.toString());
//     });
// })
//     .catch(function (err) {
//     return console.error(err.toString());
// })
    .then(function() {
    connection.invoke("GetMessageHistory").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});


// Обработчик получения приватного сообщения
connection.on("ReceivePrivateMessage", function (from, message) {
    const encodedMsg = `[Приватно от ${from.username}]: ${message}`;
    const li = document.createElement("li");
    li.classList.add("list-group-item", "private-message");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

// Обработчик обновления списка пользователей
connection.on("UpdateUserList", function (users) {
    const userList = document.getElementById("userList");
    userList.innerHTML = "";
    console.log("Received users:", users);

    users.forEach(user => {
        user[user.id] = user;
        console.log("user:", user);
        const li = document.createElement("li");
        li.classList.add("list-group-item");
        
        const span = document.createElement("span");
        span.textContent = user.username;
        const button = document.createElement("button");
        button.classList.add("btn", "btn-outline-primary", "btn-sm");
        button.textContent = "Написать";

        const buttonCall = document.createElement("button");
        buttonCall.classList.add("btn", "btn-outline-primary", "btn-sm");
        buttonCall.textContent = "Позвонить";
        // // Добавляем возможность отправки приватного сообщения
        // li.addEventListener("click", function() {
        //     const recipient = this.textContent;
        //     document.getElementById("recipientInput").value = recipient;
        //     document.getElementById("privateMessageModal").style.display = "block";
        // });

        button.onclick = function () {
            document.getElementById("recipientInput").value = user.username;
            document.getElementById("recipientInputId").value = user.id;
            document.getElementById("privateMessageModal").style.display = "block";
        }
        
        buttonCall.onclick = function(){
            SignalRConn.invoke("CallUser",user.id)
                .then(()=>console.log("Starting call...",user.id))
                .catch(err=>{console.error("Error while starting call...",err)});
3
            document.getElementById("callingToInput").textContent = user.username;
            document.getElementById("callingToInputId").value = user.id;
            document.getElementById("WebRTCVoiceChatModal").style.display = "block";
        };

        li.appendChild(span);
        li.appendChild(button);
        li.appendChild(buttonCall);
        userList.appendChild(li);
    });
});

// Функция для отправки приватного сообщения
document.getElementById("sendPrivateButton").addEventListener("click", function (event) {
    // const recipient = document.getElementById("recipientInput").value;
    const recipient = document.getElementById("recipientInputId").value;
    const message = document.getElementById("privateMessageInput").value;

    if (recipient && message) {
        connection.invoke("SendPrivateMessage", recipient, message).catch(function (err) {
            return console.error(err.toString());
        });

        document.getElementById("privateMessageInput").value = "";
        document.getElementById("privateMessageModal").style.display = "none";
    }

    event.preventDefault();
});

// Обработчик получения истории сообщений
connection.on("ReceiveMessageHistory", function (messages) {
    const messagesList = document.getElementById("messagesList");
    messagesList.innerHTML = ""; // Очищаем текущий список
    console.log("History messages", messages);

    messages.forEach(msg => {
        const encodedMsg = formatMessage(msg);
        const li = document.createElement("li");
        li.classList.add("list-group-item");

        if (msg.isPrivate) {
            li.classList.add("private-message");
        }

        li.textContent = encodedMsg;
        messagesList.appendChild(li);
    });
});

// Функция форматирования сообщения
function formatMessage(msg) {
    const date = new Date(msg.timestamp);
    const timeString = date.toLocaleTimeString();

    if (msg.isPrivate) {
        return `[${timeString}] [Приватно ${msg.userName} -> ${msg.recipient}]: ${msg.message}`;
    } else {
        return `[${timeString}] ${msg.userName}: ${msg.message}`;
    }
}

let typingTimer;
const doneTypingInterval = 1000; // время в мс, после которого считаем, что пользователь закончил печатать

document.getElementById("messageInput").addEventListener("keydown", function (event) {
    clearTimeout(typingTimer);
    
    connection.invoke("UserIsTyping").catch(function (err) {
        return console.error(err.toString());
    })
});

// Оповещаем сервер, когда пользователь закончил печатать
document.getElementById("messageInput").addEventListener("keyup", function() {
    clearTimeout(typingTimer);

    const username = document.getElementById("userInput").value;
    typingTimer = setTimeout(function() {
        connection.invoke("UserStoppedTyping").catch(function (err) {
            return console.error(err.toString());
        });
    }, doneTypingInterval);
});

// Обработчик события "пользователь печатает"
connection.on("UserTyping", function (username) {
    const typingIndicator = document.getElementById("typingIndicator");
    typingIndicator.textContent = `${username} печатает...`;
    typingIndicator.style.display = "block";
});

// Обработчик события "пользователь закончил печатать"
connection.on("UserStoppedTyping", function (username) {
    const typingIndicator = document.getElementById("typingIndicator");
    typingIndicator.style.display = "none";
});

// Обработчики состояния соединения
connection.onreconnecting(error => {
    const statusMessage = `Соединение потеряно. Переподключение... Ошибка: ${error}`;
    document.getElementById("connectionStatus").textContent = statusMessage;
    document.getElementById("connectionStatus").style.color = "orange";
});

connection.onreconnected(connectionId => {
    const statusMessage = `Соединение восстановлено. ID: ${connectionId}`;
    document.getElementById("connectionStatus").textContent = statusMessage;
    document.getElementById("connectionStatus").style.color = "green";

    // Запрашиваем историю сообщений и список пользователей заново
    connection.invoke("GetMessageHistory").catch(function (err) {
        return console.error(err.toString());
    });
});

connection.onclose(error => {
    const statusMessage = `Соединение закрыто. Обновите страницу для повторного подключения.`;
    document.getElementById("connectionStatus").textContent = statusMessage;
    document.getElementById("connectionStatus").style.color = "red";
});