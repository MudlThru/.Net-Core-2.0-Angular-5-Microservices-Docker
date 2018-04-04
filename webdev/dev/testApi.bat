@echo off

Echo Testing the Api WebAPI project

cd api
cmd /c dotnet build

start /min dotnet run

start http://localhost:5000/api/values

Echo Waiting to terminate
pause

echo Killing all instances of dotnet.exe
taskkill /F /FI "IMAGENAME eq dotnet.exe"
