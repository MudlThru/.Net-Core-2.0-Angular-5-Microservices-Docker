# .Net Core 2.0, Angular 5, Microservices & Docker

## Introduction

### Aim
My goal for 2018 is to learn a modern web development stack, moving away from monolithic ASP.Net MVC web applications, building my own knowledge and hopefully making myself more employable in the process.

After reading [ASP.NET Core 2 and Angular 5](packtpub.com) by Valerio De Sanctis, [Building Microservices with .NET Core 2.0](packtpub.com) by Gaurav Aroraa and [Deployment with Docker](packtpub.com) by Srdjan Grubor (all Available from PacktPub.com). I decided I would expand on the source material and see if I can take the knowledge gained form these three titles and create a guide for myself, and any others who might be interested.

_**Note:** This is purely a knowledge building exercise; the code, and configuration, will not likely be production ready but will hopefully provide a base from which you can build on.

I intend for this to be an evolving document which, I will revise as time goes on to include additional information and any changes to the steps required._

### Technology
I have decided to challenge myself to learning the following technologies; as they represent a complete set of technology layers found in use today, they also provide a good method of deconstructing a project into parts, which can be fully de-coupled and interchanged:

#### API Development

##### VSCode 
I have played with VSCode personally and at work, it is fast which makes it a complete dream to use. I can work with multiple languages in the same IDE and manage GIT changes etc. all within one place.

##### (Dot).Net Core
I am familiar with C# and the .Net framework so it seemed logical to target this first new branch of the technology, especially as Core can be used on a multitude of different environments including Linux, OSX & Windows.

##### PHP
I have used PHP in the past for personal projects; it is quick to script something together, although speed can completely depend on how complex the task is. I have previously used Slim and Twig for a basic MVC application so I might go down that route again, as opposed to using Laravel/Symphony. We’ll see how it goes.

##### Node.JS
I was going to say I have never used Node but that would be a lie. I have never developed anything to specifically run as a Node application. JavaScript is ubiquitous, and as I have found out, Node.JS is forging JavaScript’s path forward. It makes sense to at least investigate what can be achieved and compare it with dotnet Core and PHP.

#### Web Server/Reverse Proxy

##### nGinx
I have played around with nGinx at home, along with PHP and Core, to test how to reverse proxy Core MVC applications and server PHP pages. I plan to expand on my knowledge of nGinx configuration and leverage some tricks to build a single application out of many technologies but obfuscate it all through nGinx.

#### Application containerisation

##### Docker
Docker intrigues me, I’ve used virtual machines for over a decade now but have not yet used containers, I feel this is definitely something that is going to grow as time goes on and is going to be of paramount importance to software developers going forward. 

Docker also has the benefits that it can be used to scale all, or part, of my planned application. This is something that should be of interest to anyone planning to deploy a scalable application.

##### Debian
Debian is great, it is the basis of some of the most popular Linux distributions out there – end of. I’m going to attempt to deploy my complete application to Docker Debian containers on a Debian virtual server.

#### Source Control

##### GIT
**Don’t panic**, I have been using source control! 

My current experience has focused on basic usage of TFS in a corporate setting. For this project I want to expand my knowledge of GIT by making this project public via GitHub. I’ve used Git previously and I have used BitBucket and SourceTree but GIT is cross platform, which is how things should be.

#### HTTPS certificates

##### OpenSSL 
I have been a fan of OpenSSL for a long time, I use it at work and will continue to do so for some time. We’re going to use it to produce some test certificates on our development environment.

##### LetsEncrypt 
I have been using LetsEncrypt for about a year and cannot fault it for the price 😉. It is a great service and the developers have made the process as simple as possible to be able to create and renew certificates on a Linux system, I haven’t even tried using it on Windows.

_**Note:** At the time of writing, LetsEncrypt are on the verge of releasing wildcard certificates, however, I will cover how to create multiple subdomain certificates on the same nGinx server. If wildcard certificates become available, I will also include this within the guide._

#### Database

##### SQL 
SQL and I go way back, about 15 years or so. Now it is available on Linux and as an Express addition – it should meet my needs.

##### SQLite 
SQLite is a quick win when it comes to designing a database structure and testing it out. I may still use this during development, although if I can quickly spin up a SQL instance in Docker the benefits are slightly diminished.

##### MySQL 
I’m not sure if SQL will allow me to do all of the High Availability stuff that I believe I can do with MySQL in a Docker swarm. Time will tell if I need to switch from MSSQL to MySQL.

### Why are you (not) using…?
**Good question(s)**. This document focuses on constructing an application for deployment on machines that I have complete control of. Although it can be potentially torturous, I believe it is good practice to occasionally push yourself to understand, as much as possible, the infrastructure that your applications are running on. This provides a deeper knowledge which in-turn aides you to diagnose issues when things go wrong.

I’ve shyed away from using Azure and AWS for hosting, as I want to try to understand as much of the stack as possible. This also frees me from the pressures of any cost implications, which as a parent of small children is a blessing - spare time and money are not something I have in abundance.

Perhaps later on, when I have completed the initial version of the document, I will be able to expand further into various hosting scenarios and other configurations.

### Configuration
**Question:** I don’t have access to a Windows 10 Pro machine, can I still use this guide?
In short, yes. I’m initially writing this document for myself whilst using Windows 10 Pro. I do however have a paunchent for running Linux machines for my other personal projects.

The stack of technologies I have employed for this project are available across multiple OSs so in theory nothing should stop you from following this guide on a Linux or OSX machine. The only thing that will not work for you are some of the scripts and configuration files, I will aim to create an alternative set of these files for other OSs.