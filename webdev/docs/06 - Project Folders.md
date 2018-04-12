# .Net Core 2.0, Angular 5, Microservices & Docker
## Scaffolding the project folders
### Base projects
Open VS Code and create a new file using the code below and save it in the `webdev\dev` folder as `scaffold.bat`
```bat
@echo off

echo Install Angular CLI
cmd /c npm install -g @angular/cli

echo Create Angular project
cmd /c ng new ang --service-worker
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
```
This script installs the Angular CLI, creates a base Angular project and creates two .Net Core WebAPI projects.
 
![Script](img/Project%20Folders%20-%2014%20-%20Script.png)

It might look like it’s not doing much to start with, eventually when it gets going you should be left with the following folders created for you.
 
The ang folder contains a standard Angular app, the auth and api folders contain base WebAPI projects with different packages installed. 

Not bad for a relatively small script, you can of course embellish the script to create a Visual Studio Solution and add the projects to it automatically and include any additional packages you might need etc.

### Update references
One thing we need to do is update the Angular app to make use of the packages we included in the ang project (Bootstrap, Bootswatch & jQuery).

Browse to `webdev\dev\ang` and right-click on `.angular-cli.json` and select “Open with Code”:

Find the `scripts` element, on or around line 22:
```json
"scripts": [],
```
Insert the following lines into the scripts array element and save the file:
```json
"scripts": [
        "../node_modules/jquery/dist/jquery.min.js",
        "../node_modules/bootstrap/dist/js/bootstrap.js"
    ],
```

Next we need to add some CSS style sheets, browse down the the `src` folder and right-click on the `styles.css` file and select “Open with Code”:

In here at line 2 we are going to paste in the following:
```ts
@import "../node_modules/bootstrap/dist/css/bootstrap.min.css";

/*
@import "../node_modules/bootswatch/dist/materia/variables";
@import "../node_modules/bootstrap/scss/bootstrap";
@import "../node_modules/bootswatch/dist/materia/bootswatch";
@import "../node_modules/bootswatch/dist/materia/bootstrap.min.css";
*/
```
This uses the default Bootstrap theme, the commented-out lines are for the Bootswatch themes. To use the Bootswatch themes you need to comment out line 2 and un-comment lines 5-8.

**TODO: Explain what Bootswatch is with a link to the site.**

### Internet Explorer / Edge Polyfils
One last thing, if you intend to target Internet Explorer / Edge users you are going to have to edit the `polyfils.ts` file in the `webdev\dev\ang\src` folder, as per the other edits so far right-click on the file and select “Open with Code”:

You then want to uncomment the lines relating to IE and/or Edge, make sure to only include the ones which do not require to run any additional `npm install` commands, we don’t need these yet:

![polyfils content](img/Project%20Folders%20-%2019%20-%20polyfils.png)
 
Right, so that is the basic config for the Angular app setup.
The next thing to do is to create some components… but before we do that lets make sure that everything builds and works.

## Basic Testing
### Testing Angular
In VS Code create a new file, copying the code below, and save it to `webdev\dev\testAngular.bat`:
```bat
@echo off

Echo Testing Angular application

cd ang
cmd /c ng build
cmd /c ng serve

start http://localhost:4200
```
Now double-click on the `testAngular.bat` script.
 
A terminal window will open and will appear not to do much initially, after a while it will start churning out some info.
 

If you see any prompts during the build process click on “Allow access”, you may want to check that the browser window hasn’t appeared over the top of this dialog, if it has you will likely need to refresh the browser once all permissions have been granted.
 
![Node.js prompt](img/Project%20Folders%20-%2020%20-%20NodeJs%20prompt.png)
 
The script should open a new browser session pointing to http://localhost:4200, the default for `ng serve`, if this doesn’t happen you should be able to point to the page manually.
 
![Home](img/Project%20Folders%20-%2021%20-%20Home.png)
 
If you see the “Welcome to app!” page it is working, now close the browser and in the terminal press `Ctrl + C`, and then `Y` to stop `ng serve`.

![ng serve](img/Project%20Folders%20-%2022%20-%20ngserve.png)
 
### Testing the WebAPI projects
In VS Code create a new file, copying the code below, and save it as `webdev\dev\testAuth.bat`:
```bat
@echo off

Echo Testing the Auth WebAPI project

cd auth
cmd /c dotnet build

start /min dotnet run

start http://localhost:5000/api/values

Echo Waiting to terminate
pause

echo Killing all instances of dotnet.exe
taskkill /F /FI "IMAGENAME eq dotnet.exe"
```
Do the same this time saving the file as `testAPI.bat`:
```bat
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
```
Now, one by one, run each of the script files. You will see the dotnet build process restore the required packages and build the project:
 
![auth test](img/Project%20Folders%20-%2023%20-%20auth%20test.png)
 
You will also notice a new browser session open up showing a simple Json output, this is the default behaviour of the bootstrapped ValuesController, which we will get to later.
 
![values](img/Project%20Folders%20-%2024%20-%20auth%20test%20values.png)
 
Once you are happy that has worked, close the browser, click into the terminal and press a key, this will then kill the running version of dotnet. You can then move onto testing the next script.

You might wonder why we have created these scripts, the answer is purely that they will become useful if/when you need to debug something later without the need to run the rest of the application (i.e. the Angular site).

## Summary
OK, so far, we’ve downloaded lots of files, installed lots of applications and copied lots of files whilst doing a minimal amount of coding.

We have also setup our Angular and WebAPI projects, the next step is to get them to talk to each other, but there’s a problem everything is running under localhost with weird ports and both WebAPI projects are using the same port as well. 

If you don’t know much about web development, this is a bad thing as the current setup isn’t going to work very nicely, if at all! Luckily, we are ahead of the game as we have downloaded nGinx and can configure this to work as a reverse proxy, we also need to start making some tweaks to our WebAPI projects.
