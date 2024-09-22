
function GetAllUsers() {
    $.ajax({
        url: "/Home/GetAllUsers",
        method: "GET",
        success: function (data) {
            let content = "";
            for (let i = 0; i < data.length; i++) {
                let style = '';
                let subContent = '';
                if (data[i].hasRequestPending) {
                    subContent = `<button class='btn btn-outline-secondary'>Already Sent</button>`;
                }
                else {
                    if (data[i].isFriend) {
                        subContent = `<button class='btn btn-outline-secondary'>UnFollow</button>`;
                    }
                    else {
                        subContent = `<button class='btn btn-outline-primary' onclick="SendFollow('${data[i].id}')">Follow</button>`;
                    }
                }

                if (data[i].isOnline) {
                    style = 'border:5px solid springgreen;';
                }
                else {
                    style = 'border:5px solid red;';
                }
                let item = `
                <div class='card' style='${style}width:220px;margin:5px;'>
                    <img style='width:100%;height:220px;' src='/images/${data[i].imageUrl}'/>
                    <div class='card-body'>
                        <h5 class='card-title'>${data[i].userName}</h5>
                        <p class='card-text'>${data[i].email}</p>
                        ${subContent}
                    </div>
                </div>
                `;

                content += item;
            }

            $("#allusers").html(content);
        }
    })
}

GetAllUsers();
GetMyRequests();

function SendFollow(id) {
    const element = document.querySelector("#alert");
    element.style.display = "none";
    $.ajax({
        url: `/Home/SendFollow/${id}`,
        method: "GET",
        success: function (data) {
            element.style.display = "block";
            element.innerHTML = "Your friend request sent successfully";
            GetAllUsers();
            SendFollowCall(id);
            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            },5000)
        }
    })
}

function DeclineRequest(id, senderId) {
    $.ajax({
        url: `/Home/DeclineRequest?id=${id}&senderId=${senderId}`,
        method: "GET",
        success: function () {
            const element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You declined request";
            SendFollowCall(senderId);
            GetAllUsers();
            GetMyRequests();
            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000)
        }
    })
}

function AcceptRequest(id, id2, requestId) {
    $.ajax({
        url: `/Home/AcceptRequest?userId=${id}&senderid=${id2}&requestId=${requestId}`,
        method: "GET",
        success: function (data) {
            const element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You accept request successfully";
            SendFollowCall(id);
            SendFollowCall(id2);

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            },5000)
        }
    })
}

function DeleteRequest(id) {
    $.ajax({
        url: `/Home/DeleteRequest/${id}`,
        method: "DELETE",
        success: function (data) {
            GetMyRequests();
        }
    })
}

function GetMyRequests() {
    $.ajax({
        url: '/Home/GetAllRequests',
        method: 'GET',
        success: function (data) {
            let content = "";
            let subContent = "";
            for (let i = 0; i < data.length; i++) {
                if (data[i].status == "Request") {
                    subContent = `
                    <div class='card-body'>
                    <button class='btn btn-success' onclick="AcceptRequest('${data[i].senderId}','${data[i].receiverId}',${data[i].id})">Accept</button>
                    <button class='btn btn-secondary' onclick="DeclineRequest(${data[i].id},'${data[i].senderId}')">Decline</button>
                    </div>
                    `;
                }
                else {
                    subContent = `
                    <div class='card-body'>
                    <button class='btn btn-warning' onclick="DeleteRequest(${data[i].id})">Delete</button>
                    </div>
                    `;
                }

                let item = `
                <div class='card' style='width:15rem'>
                <div class='card-body'>
                <h5>Request</h5>
                <ul style="list-style:none;">
                    <li>${data[i].content}</li>
                </ul>
                ${subContent}
                </div>
                </div>
                `;
                content += item;
            }
            $("#requests").html(content);
        }
    })
}