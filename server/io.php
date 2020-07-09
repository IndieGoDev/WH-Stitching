<?php
# Disable Warnings
SET_ERROR_HANDLER( "KESALAHAN", E_ALL );
function KESALAHAN(){}

# online
$host = "localhost";
$user = "u886988804_huda";
$pass = "78987899";
$db   = "u886988804_whstc";

# lokal
//$host = "localhost";
//$user = "huda_indie_dev";
//$pass = "78987899";
//$db   = "wh_stitching";

$koneksi = new MySQLI( $host, $user, $pass, $db);

# WebRequest POST
$mode = $_POST['mode'];
$data = $_POST['data'];

# Encode Decode
function bin($data, $mode){
	try {
		if ($mode)
			return decbin(hexdec(bin2hex($data)));
		else
			return hex2bin(dechex(bindec($data)));
	} catch (Exception $e) { echo "error code : ".$e; }
}

switch ($mode) {
	case 'LoginQR':
	case 'LoginPass':
		$user = $koneksi->query("SELECT * FROM user WHERE ".($mode == 'LoginQR' ? "id" : "guard")." = ".$data);
		if ($user->num_rows != 0 && $data != null) {
			$user = mysqli_fetch_assoc($user);
			echo $mode.'|'.$user['id'].'|'.$user['guard'];
		} else {
			echo $mode == 'LoginQR' ? 'Gagal|QR Code Salah' : 'Gagal|Password Salah';
		}
		break;
	default:break;
}

# bin("11111111111111111111111111111111", false); 32bit

?>