function CheckIn(content, smile) {
    var result;

    var say = document.getElementById('todaysay');
    if (say == null) {
        result =
        {
            Error: 0x2000,
        };
        return JSON.stringify(result);
    }

    if (content == null)
    {
        content = '无聊...';
    }
    say.value = content;

    var lis = document.getElementsByName('qdxq')[0].options;

    if (lis == null || lis.length == 0) {
        result =
        {
            Error: 0x2001,
        };
        return JSON.stringify(result);
    }

    if (smile == -1) {
        var index = Math.floor(Math.random() * lis.length);
        lis[index].selected = true;
    }
    else if (smile > -1 && smile < lis.length) {
        lis[smile].selected = true;
    }
    else {
        result =
        {
            Error: 0x2003,
        };
        return JSON.stringify(result);
    }

    var inputs = document.getElementsByName('qiandao')[0].getElementsByTagName('input');
    var button;
    for (var index = 0; index < inputs.length; index++) {
        var element = inputs[index];
        if(element.value == "点我签到!")
        {
            button = element;
        }
    }

    if (button == null) {
        result =
        {
            Error: 0x2002,
        };
        return JSON.stringify(result);
    }

    button.click();

    result =
    {
        Error: 0,
    };
    return JSON.stringify(result);
}