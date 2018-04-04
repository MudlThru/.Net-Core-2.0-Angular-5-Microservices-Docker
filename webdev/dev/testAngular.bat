@echo off

Echo Testing Angular application

cd ang
cmd /c ng build
cmd /c ng serve

start http://localhost:4200
