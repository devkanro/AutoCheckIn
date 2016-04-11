function Logout() {
    var list = document.getElementsByClassName("pd2")[0].getElementsByTagName("a");
    var result;
    var logout;

    for (var index = 0; index < list.length; index++) {
        var e = list[index];

        if ((e.textContent || e.innerText) == "退出") {
            logout = e;
            break;
        }
    }

    if (logout == null) {
        result = {
            Error: 0x5000
        };

        return JSON.stringify(result);
    }

    logout.click();
    result = {
        Error: 0
    };

    return JSON.stringify(result);
}