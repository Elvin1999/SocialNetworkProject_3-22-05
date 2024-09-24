"use strict"

var connection = new signalR.HubConnectionBuilder().withUrl("/chathub")
    .build();

connection.start().then(function () {
    GetAllUsers();
})
    .catch(function (err) {
        return console.error(err.toString());
    })

let element = document.querySelector("#alert");

connection.on("Connect", function (info) {
    GetAllUsers();
    element.style.display = "block";
    element.innerHTML = info;
    setTimeout(() => {
        element.innerHTML = "";
        element.style.display = "none";
    }, 5000);
})

connection.on("Disconnect", function (info) {
    GetAllUsers();
    element.style.display = "block";
    element.innerHTML = info;
    setTimeout(() => {
        element.innerHTML = "";
        element.style.display = "none";
    }, 5000);
})

connection.on("ReceiveNotification", function () {
    GetMyRequests();
    GetAllUsers();
});

async function GetMessageCall(receiverId, senderId) {
    await connection.invoke("GetMessages", receiverId, senderId);
}


async function SendFollowCall(id) {
    await connection.invoke("SendFollow", id);
}


connection.on("ReceiveMessages", function (receiverId, senderId) {
    GetMessages(receiverId, senderId);
});