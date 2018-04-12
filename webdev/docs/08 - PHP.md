# .Net Core 2.0, Angular 5, Microservices & Docker
## Adding a secure PHP project

So what if you have some PHP code you would like to use in your project, how can we fit htis in to our solution? If you've started to think of our project as a set of parts, which slot together, you can probably see where we are going.

We can slot in our PHP code as a seperate micro-service, it can be completely independent from our .Net code but still be part of our project.

### Extract PHP
Right-click on the downloaded PHP Zip file, extract it to a new folder called `php` under `webdev\nginx`.
 
![Extract](img/PHP%20-%2002%20-%20Extract.png)

It might take a couple of minutes to chug through all of the files:
 
### Update php.ini
The next job is to create a `php.ini` file, fortunately the PHP download includes a couple of examples files that you could just rename.
Go ahead and rename `php.ini-production` to `php.ini`:

![php.ini](img/PHP - 04 - dev php folder.png)
 
I don’t believe anything needs to be changed here for our project, however, it is always good practice to review the configuration for any settings which may expose your server/application to external threats.

## Content
If it doesn’t already exist, create the `webdev\dev\php` folder.
 
Within VS Code a new php folder should have appeared:
![folder](img/PHP%20-%2005%20-%20VS%20Code.png)
 
In this folder we are going to create two php files.
TODO: Use a proper restful design for the api end point (https://stackoverflow.com/questions/28094865/nginx-configuration-for-a-restful-api)

### loggedin.php
`loggedin.php` is the end point for this test PHP api, if you are going to develop a proper PHP api you might want to structure your files properly, and likely review the nGinx rules.

This file bascially checks the authentication for the user JWT token and allows the requested method to be called if the token is valid, and the user has the correct permissions:
```php
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
```

____
**Attribution:** 

The following section refers to code taken from [angular-codeigniter-seed](https://github.com/rmcdaniel/angular-codeigniter-seed) by [Richard McDaniel](https://github.com/rmcdaniel)  ( [Source](https://github.com/rmcdaniel/angular-codeigniter-seed/blob/master/api/application/helpers/jwt_helper.php) , [License](https://github.com/rmcdaniel/angular-codeigniter-seed/blob/master/LICENSE.md) )

I do not intend to explain in too much detail what this code is doing, thit is in essence placeholders for your own code but provide a good reference point from which to work from.

If you wish to learn more I recommend you review the GitHub project.
____

### Jwt_helper.php
`jwt_helper.php` is the main file we are using, it does all of the heavy lifting for us in terms of deciphering the JWT and validating its contents.

I'm not oing to explain what it all does because that would take too long.

If you want to find out more about this file you can view the original file [here](https://github.com/rmcdaniel/angular-codeigniter-seed/blob/master/api/application/helpers/jwt_helper.php).

That will do for now, the only thing we need to be concerned about is the nGinx config, whcoh we will cover once we have our Angular site setup.