function GetRequest(a) {
   var url = a.search;
   var theRequest = new Object();
   if (url.indexOf("?") != -1) {
      var str = url.substr(1);
      var strs = str.split("&");
      for(var i = 0; i < strs.length; i ++) {
         theRequest[strs[i].split("=")[0]]=unescape(strs[i].split("=")[1]);
      }
   }
   return theRequest;
}

function GetLoginState() {
    var list = document.getElementsByClassName("pd2")[0].getElementsByTagName("a");
    var result;
    var nameOrLogin = list[0].textContent || list[0].innerText;
    
    if(nameOrLogin == "登录")
    {
        result =
            {
                Error: 0,
                IsLogin: false,
                UserName: null,
                Uid:-1
            };
            return JSON.stringify(result);
    }
    else
    {
        var request = GetRequest(list[0]);
        
        result = 
        {
            Error:0,
            IsLogin : true,
            UserName : nameOrLogin,
            Uid:parseInt(request['uid'])   
        }
            return JSON.stringify(result);
    }
}