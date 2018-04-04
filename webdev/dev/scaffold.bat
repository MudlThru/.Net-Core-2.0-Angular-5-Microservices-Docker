@echo off

echo Install Angular CLI
cmd /c npm install -g @angular/cli

echo Create Angular project
cmd /c ng new ang
rem ng build
cd ang
cmd /c npm install --save bootstrap@3.3.7 bootswatch@3.3.7 jquery@3.3.1
cd ..

echo Create WebAPI projects
cmd /c dotnet new webapi -n auth
cmd /c dotnet new webapi -n api

echo Adding packages to auth
cd auth
dotnet add package Mapster
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet restore
cd ..

echo Adding packages to api
cd api
dotnet add package Mapster
dotnet restore
cd ..
