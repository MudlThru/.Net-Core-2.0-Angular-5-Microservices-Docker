<?php
require 'jwt_helper.php'; //Required to validate the JWT

$issuer = 'https://localhost/';//
$audience = 'https://localhost/';//
$secret_key = 'some_test_key';//'some_test_key';

$reqmethod = strtolower($_SERVER['REQUEST_METHOD']); //Gets the REST method (GET, POST, PUT, DELETE)
$class  = filter_input(INPUT_GET, 'class',  FILTER_SANITIZE_STRING);
$method = filter_input(INPUT_GET, 'method', FILTER_SANITIZE_STRING);
$request = explode("/", substr(@$_SERVER['PATH_INFO'], 1)); //Gets the route/entity (i.e. controller for the route)
$rest = $reqmethod.'_method';  //Builds a function name

if (function_exists($rest)) 
	call_user_func($rest, $request);
else {
	http_response_code(503);//404); //Throw an error if the function cannot be found
	exit(503);
}

/*
Rest Function(s)
*/

function get_method($request = NULL) {
	$userid = authenticated(array('Administrator')); //Require authentication
	$name = '';
	if(isset($_GET["name"])) $name = $_GET["name"];
	//echo $request[0]."\n";
	//echo $_SERVER['PATH_INFO'];
	$arr = array("Get ".$name."!");
	header('Content-type: application/json');
	echo json_encode($arr);
}

function put_method($request = NULL) {
	$userid = authenticated(array('RegisteredUser')); //Require authentication
	if ($userid != false) {
		parse_str(file_get_contents("php://input"),$put_vars);
		if(isset($put_vars["name"])) $name = $put_vars["name"];
		$arr = array("Put ".$name."!");
		header('Content-type: application/json');
		echo json_encode($arr);
	} else {
		http_response_code(401);
		exit(401);
		return false;
	}
}

function post_method($request = NULL) {
	$userid = authenticated(array('RegisteredUser')); //Require authentication
	if ($userid != false) {
		if(isset($_POST["name"])) $name = $_POST["name"];
		$arr = array("Post ".$name."!");
		header('Content-type: application/json');
		echo json_encode($arr);
	} else {
		http_response_code(401);
		exit(401);
		return false;
	}
}

function delete_method($request = NULL) {
	$userid = authenticated(array('Administrator')); //Require authentication and Administrator role
	if ($userid != false) {
		parse_str(file_get_contents("php://input"),$delete_vars);
		if(isset($delete_vars["name"])) $name = $delete_vars["name"];
		$arr = array("Delete ".$name."!");
		header('Content-type: application/json');
		echo json_encode($arr);
	} else {
		http_response_code(401);
		exit(401);
		return false;
	}
}

/*
Authentication Function(s)
Visit https://www.iana.org/assignments/jwt/jwt.xhtml for JWT claim naming examples
If you are building a larger api in PHP you can move this function into another file (i.e. auth.php) and reference it in dependent scripts:
require 'auth.php';
*/
function authenticated($roles){
	global $secret_key;
	global $issuer;
	$headers = getallheaders();
	if (array_key_exists('Authorization', $headers)) {
		$jwt = $headers['Authorization'];
		$jwt = str_replace('Bearer ', '', $jwt); //Added to remove "Bearer " from the token.
		$token = JWT::decode($jwt, $secret_key, array('HS256'));
		//Check to see if the user has the required roles
		$roleCheck = false; //Default the rolecheck value to false
		if(isset($roles)){
			foreach ($roles as $role){
				$roleResult = $token->{'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'};
				if(in_array($role, $roleResult))
				{
					$roleCheck = true;
				}
			}
		} else {
			$roleCheck = true;
		}
		//Check that the token is valid, otherwise return a 401 Unauthorized error
		if ($token->exp >= time() && $token->iss == $issuer) {
			if($roleCheck){
				//loggedin
				return $token->sub;
			} else {
				http_response_code(403); //Forbidden
				exit(403);
			}
		} else {
			http_response_code(401); //Unauthorized
			exit(401);
			return false;
		}
	} else {
		http_response_code(401); //Unauthorized
		exit(401);
		return false;
	}
}

?>