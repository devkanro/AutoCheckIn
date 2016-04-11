function CheckSign() {
    var result;

    var sign = document.getElementsByName("qiandao")[0];

    if (sign == null) {
        result = {
            Error: 0,
			IsSigned : true
        };

        return JSON.stringify(result);
    }

    result = {
        Error: 0,
		IsSigned : false
    };

    return JSON.stringify(result);
}