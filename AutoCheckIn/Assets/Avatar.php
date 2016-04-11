<?php
$userid = $_GET['uid'];
$url = "http://www.tsdm.net/home.php?mod=space&uid=".$userid;

$ch = curl_init(); 
$timeout = 5; 
curl_setopt($ch, CURLOPT_URL, $url); 
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1); 
curl_setopt($ch, CURLOPT_USERAGENT, "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11");
curl_setopt($ch, CURLOPT_HTTPHEADER, array('X-FORWARDED-FOR:111.222.333.4', 'CLIENT-IP:111.222.333.4'));  
curl_setopt($ch, CURLOPT_REFERER, "http://higan.me");  
//在需要用户检测的网页里需要增加下面两行 
//curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_ANY); 
//curl_setopt($ch, CURLOPT_USERPWD, US_NAME.":".US_PWD); 
$contents = curl_exec($ch); 
curl_close($ch); 

$isMatched = preg_match("/=\"([^\"]*)\" onerror=\"this.onerror=null;this.src='http:\/\/www.tsdm.net\/uc_server\/images\/noavatar/",$contents,$matchs);
if($isMatched)
{
	$big = str_replace("avatar_middle","avatar_big",$matchs[1]);
	header("location: ".$big);
}
else
{
	echo "NULL";	
}
?>