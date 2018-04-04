@echo off

echo Get root folder path
SET rootFolder=%~dp0

rem timestamp file
set timestamp = "timestamp.txt"

rem for %%f in (%filename%) do set filedt=%%~tf
rem for %%f in (!output_filename!) do set outfiledt=%%~tf
rem if %filedt:~0, 10% LSS %outfiledt:~0, 10% DO SOMETHING

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
rem start https://localhost
rem start http://localhost:5000/api/values

echo Waiting for user to terminate nginx
pause

echo Killing all instances of dotnet.exe
taskkill /F /FI "IMAGENAME eq dotnet.exe"

echo Stopping nginx
rem cd %rootFolder%/nginx-1.13.9
rem cmd /c nginx.exe -s stop
cmd /c taskkill /f /IM nginx.exe

echo Killing PHP FastCGI...
cmd /c taskkill /f /IM php-cgi.exe