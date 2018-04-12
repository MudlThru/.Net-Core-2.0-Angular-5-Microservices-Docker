## Putting it all together
### Configuring nGinx
**Why are we using nGinx and not something else like IIS?**

Well, it all comes down to familiarity. nGinx is available on multiple platforms (Windows, Linux), as we are going to be targeting a Debian Linux distribution later on it makes sense to try and build our knowledge now whilst we are developing the application, we can leverage that knowledge later to prevent us from getting stuck.

Browse to `webdev\nginx\conf`, right-click on `nginx.conf` and select **Open in Code**:

![nGinx.conf](img/Config%20-%2001%20-%20nGinx%20conf.png)
 
As you can see VS Code does a good job of showing us the commented out lines, this makes it a lot easier to edit than in some of the other text editors I have used in the past.

You shouldn’t need to touch anything before the first `server {` entry, somewhere around **line 35**. 

For this we want to make the following changes:
```nginx
# HTTP server
#
server {
    listen       8080;
    server_name  localhost;
    
    # redirect to HTTPS
    location / {
        return 301 https://localhost\$request_uri; # Can we use the $host keyword?
    }

    #error_page  404              /404.html;

    # redirect server error pages to the static page /50x.html
    #
    #error_page   500 502 503 504  /50x.html;
    #location = /50x.html {
    #    root   html;
    #}

}
```
That was brief. We’ve basically said here that we want to listen to everything on port 8080. You can change this to port 80, the standard HTTP port, if you like but you will need to ensure that you’re not running any other webservers running/listening on this port (i.e. IIS, Apache etc).

Any traffic hitting **port 8080** will be redirected to the HTTPS version on **port 443**.

The next `server {` section gets a bit more interesting:
```nginx
# HTTPS server
#
server {
    # Copy each section in here
}
```
There will be a lot of stuff going on in here, I’ll try to explain it the best that I can:

We want to listen on **port 443**, and we want the connection to be secure so we use the `ssl` keyword. We want to use the ssl **certificate** and **key** files we created earlier with OpenSSL (in production the files will be created by LetsEncrypt).
```nginx
# Setup SSL
listen       443 ssl;
server_name  localhost;

ssl_certificate      ../../ssl/cert.pem;
ssl_certificate_key  ../../ssl/cert.key;

ssl_session_cache    shared:SSL:1m;
ssl_session_timeout  5m;

ssl_ciphers  HIGH:!aNULL:!MD5;
ssl_prefer_server_ciphers  on;
```
We set some defaults for the server and its locations:
```nginx
# Set the root folder for all locations
root ..\dev\ang\dist;
keepalive_timeout 5;
charset utf-8;
```
We create a static files rule so any scripts, images style sheets are processed correctly and delivered as they are with no meddling from anything else.
```nginx
# Static files
location ~ ^/(scripts.*js|styles|images) {
    gzip_static on;
    expires 1y;
    add_header Cache-Control public;
    add_header ETag "";
    break;
}
```
We add a rule for processing **PHP** files; this will pass any `.php` files to a **FastCGI** process on **port 5003**, which we will setup in a moment.

```nginx
#Redirect php traffic
rewrite ^/php/([^/]+)/([^/]+)/([^/]+)/?$ /$1.php?method=$2&id=$3? last;
rewrite ^/php/([^/]+)/([^/]+)/?$ /$1.php?method=$2? last;
rewrite ^/php/([^/]+)/?$ /$1.php? last;

#Redirect php traffic
location ~ \.php$ {
    root           C:/Users/WebDev/Documents/webdev/dev/php; #Because nGinx on windows doesn't support relative paths for fastcgi you can however create a junction (example: mklink /J files ..\files)
    #root          ../dev/php;
    fastcgi_pass   127.0.0.1:5003;
    fastcgi_index  index.php;
    fastcgi_param  SCRIPT_FILENAME  $document_root$fastcgi_script_name;
    include        fastcgi_params;
}
```
We add a rule for the `auth` WebAPI and another one for the `api` WebApi which will push the requests to the .Net **Kestrel** webserver, which will be explained later on.
```nginx
#Redirects all auth traffic
location /auth/ {
    proxy_pass  http://localhost:5001; #Port number must align with port used for this site in Kestrel
    #limit_req   zone=one burst=10;
    proxy_http_version 1.1;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
    }  

#Redirects all api traffic
location /api/ {
    proxy_pass  http://localhost:5002; #Port number must align with port used for this site in Kestrel
    #limit_req   zone=one burst=10;
    proxy_http_version 1.1;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
    }  
}
```
We also setup a couple of `signin` routes for external authentication providers. This will be used later when we setup our `auth` WebAPI to allow users to authenticate via **Facebook** or **Google**.
```nginx
#External login provider callbacks
location /signin-google {
    proxy_pass http://localhost:5001;
    proxy_http_version 1.1;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}  

location /signin-facebook {
    proxy_pass http://localhost:5001;
    proxy_http_version 1.1;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```
We can, if we want to be really cleaver, use a Regular Expression (RegEx) to combine the `/auth/` and `/signin-?` routes, as they are all going to the same place:
```nginx
#Auth api and External login provider callbacks
location ~ ^\/(auth\/|signin-[a-z]+$) {
    proxy_pass http://localhost:5001;
    proxy_http_version 1.1;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```
Finally, we add a rule for the Angular site.
```nginx
#Angular site
location / {
    #root   ..\dev\ang\dist;
    #index  index.html index.htm;
    try_files $uri /index.html;
}
```
### Running everything
So we now have everything we need to start running our application locally and do some testing.

In order to coordinate everything I am going to use a Windows batch script, this will build all of the projects and start the different web servers:

#### runServer.bat
```bat
@echo off

echo Get root folder path
SET rootFolder=%~dp0

rem timestamp file
set timestamp = "timestamp.txt"

for %%f in (%filename%) do set filedt=%%~tf

echo build Auth application
cd %rootFolder%/dev/auth
cmd /c dotnet build
rem cmd /c dotnet ef database update
start /min dotnet run --urls http://0.0.0.0:5001

echo build Api application
cd %rootFolder%/dev/api
cmd /c dotnet build
start /min dotnet run --urls http://0.0.0.0:5002

echo build Angular site
cd %rootFolder%/dev/ang
cmd /c ng build

rem Record the last run timestamp for next time
cd %rootFolder%
echo %date%%time% > "timestamp.txt"

echo Starting nginx
cd %rootFolder%/nginx
start nginx.exe

ECHO Starting PHP FastCGI...
cd %rootFolder%/nginx/php
start /min php-cgi.exe -b 127.0.0.1:5003 -c %rootFolder%/nginx/php/php.ini

rem start browser sessions for each type
start http://localhost:8080

echo Waiting for user to terminate nginx
pause

echo Killing all instances of dotnet.exe
taskkill /F /FI "IMAGENAME eq dotnet.exe"

echo Stopping nginx
cmd /c taskkill /f /IM nginx.exe

echo Killing PHP FastCGI...
cmd /c taskkill /f /IM php-cgi.exe
```
When you run the script, you should see each of the parts of the application being built.

![Script](img/Config%20-%2002%20-%20script.png)

 
### Troubleshooting
#### Permissions
You may also come across some permission prompts for **dotnet** and **nginx**, go ahead and OK these:

![dotnet](img/Config%20-%2003%20-%20prompt%20dotnet.png)
![nginx](img/Config%20-%2004%20-%20prompt%20nginx.png)
 
#### Missing C++ Runtime
If you see the following error dialog, you need to make sure you have the **.Net C++ runtime** installed:

![](img/Config%20-%2005%20-%20error%20VC%20Runtime.png)
 
#### Page not loading
If it’s not working and you cannot see the `nginx.exe` process running, it could be because you do not have any internet connection, so nginx cannot resolve the localhost address.

#### Untrusted certificate issue
If you see the following page it means you haven’t trusted the localhost certificate we created earlier:

![HTTPS](img/Config%20-%2006%20-%20error%20HTTPS.png)
 
You can click on Details, and then click on “Go on to the webpage”:

![HTTPS bypass](img/Config%20-%2007%20-%20error%20HTTPS%20bypass.png)
 
Or, we can save ourselves a load of effort and register our local certificate on the machine as a trusted certificate. Use the instructions in the **Setup** section to do this.

Once the certificate is installed, close any open browser sessions and re-run the runserver script. The page should be displayed straight away without any need to allow the certificate.
 
### Testing
The application will change the presented elements depending on the state of the user authentication state, controlled by the token.

#### Anonymous user
As you can see, the standard presentation doesn’t return any values in column one as the user isn’t authenticated yet. In column 2 our PHP script requires authentication, thus also returning no results. Column 3 also requires authentication, but this time not standard user authentication.

![Home Anon](img/Config%20-%2008%20-%20Home%20Anon.png)
 
#### Login as a user
Login as one of the standard users, which we created in our database seeder method.

![Login User](img/Config%20-%2009%20-%20Login%20User.png)
 
When you are logged in, you will see the values in column 1 are displayed and our PHP test in column 2 returns the logged in result. Column 3 still doesn’t return any results, because this user is does not have the admin role.

![Home User](img/Config%20-%2010%20-%20Home%20User.png)
 
#### Login as admin
Login as the admin user.

![Login Admin](img/Config%20-%2011%20-%20Login%20Admin.png)
 
As an admin you will see that not only do you see the results in the first 2 columns but you also see some results in column 3.

![Home Admin](img/Config%20-%2012%20-%20Home%20Admin.png)
 
#### Summary
nGinx is very powerful when you get to playing with the configurations. Obviously the above isn’t production ready, however, it suits our needs for local development.
We’ve got a neat little script which will build and run everything that is required to make our project work.
We have also tested all of the user states and can see that values are returned according to the user’s permissions.
