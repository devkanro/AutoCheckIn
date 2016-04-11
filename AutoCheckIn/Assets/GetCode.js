function GetCode() {
    var result;
    var img = document.getElementsByClassName('c cl')[0].getElementsByTagName('div')[0].getElementsByTagName('img')[0];

    if (img == null) {
        result =
        {
            Error: 0x3000
        };
    }

    result =
    {
        Error: 0,
        Width: img.width,
        Height: img.height
    };
    return JSON.stringify(result);
}