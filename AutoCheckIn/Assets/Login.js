function Login(mode, user, password, vcode, question, answer) {
    var result;

    var verify = document.getElementsByName('tsdm_verify')[0];
    var userName = document.getElementsByName('username')[0];
    var pwd = document.getElementsByName('password')[0];
    var login = document.getElementById('submit');
    var cookietime = document.getElementsByName('cookietime')[0];
    var loginfield = document.getElementsByName('fastloginfield')[0];
    var questionid = document.getElementsByName('questionid')[0];
    var answerInput = document.getElementsByName('answer')[0];

    if (verify == null) {
        result =
        {
            Error: 0x4000
        };
        return JSON.stringify(result);
    }

    if (userName == null) {
        result =
        {
            Error: 0x4001
        };
        return JSON.stringify(result);
    }

    if (pwd == null) {
        result =
        {
            Error: 0x4002
        };
        return JSON.stringify(result);
    }

    if (login == null) {
        result =
        {
            Error: 0x4003
        };
        return JSON.stringify(result);
    }

    if (loginfield == null) {
        result =
        {
            Error: 0x4004
        };
        return JSON.stringify(result);
    }

    if (questionid == null) {
        result =
        {
            Error: 0x4005
        };
        return JSON.stringify(result);
    }

    if (answerInput == null) {
        result =
        {
            Error: 0x4006
        };
        return JSON.stringify(result);
    }

    verify.value = vcode;
    userName.value = user;
    pwd.value = password;
    cookietime.checked = true;
    loginfield.options[mode].selected = true;
    questionid.options[question].selected = true;
    answerInput.value = answer;

    login.click();

    result =
    {
        Error: 0
    };
    return JSON.stringify(result);
}