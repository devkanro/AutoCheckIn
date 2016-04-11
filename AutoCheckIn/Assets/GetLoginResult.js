function GetLoginResult() {
    var message = document.getElementById('messagetext');
    var messageText = message.textContent || message.innerText;
    return messageText;
}