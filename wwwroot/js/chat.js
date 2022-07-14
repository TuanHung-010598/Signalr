"use strict";

var token = 'I-lClVPif7RZyoP9cJD9t6Yid1XZ5vvtdI2xkxQJ93zkAfrTOuEJfW6dxha4FeZCl1TAzLv0ZjxnNWoM2haPxHa_5Zs4L_cQoix87U3-CoHCWscmjDIrILQxES1csIL8Q990BUpsKPtiCt9MISdX8JkGyy7Aj4cbobXE5P5Mqn_IPH7pwjZFFA4EG5LcATFUkuiIjmkD5OB8rmuRFzSmvD4wus0r8nglivEBvF5qEsy0UzdrMeCbGbfzu-cesgVnPVWK1BKQA3Z4AkpMdEW3TONb8IRYkllthIDcLMNixLEN4GnjRHASfco7XCWll8SZXGCHwbuuFUtLnXffRVWfPNjIZq0brYelc5DFtEiawfMeKVcW2_DeGFvPTPUJCuTT5Ct0x3iKGizN626QKJ_mrrBrQqWDtx0iUzymk5FXwr09Rl1GXjhZ6MtagAtfg55377fug3vDzbNO5d3-kWSPnd491Muy1nYjwBBEe-ZD4TKIiYlboP5ZtlZTCKIOnuUfI6Q_ZHaZ8hFpuCPY65Os36jb0c0LuovJ013wtKW1g3Pz74smHpYjPtNjFADLm2cNh45NPqrkVQvKij8L-UW1EX20swFU3ws4rnueYzwb7af0qfeUlGmCnevCfDJwUqCACfZagau2Hbs3vrf9M0yyEg';
var connection = new signalR.HubConnectionBuilder().withUrl("/signalRHub?token=" + token + "&hotelId=5400").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (sender, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${sender} says ${message}`;
});

connection.on("HubMessage", function (data) {
    alert(data);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage",5400, user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});