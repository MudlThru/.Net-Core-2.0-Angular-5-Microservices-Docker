# .Net Core 2.0, Angular 5, Microservices & Docker

## Folders

### Create the project folders
To try and keep things as generic as possible between OSs for every reader of the guide I propose the use of a dedicated folder to store our project files, this may save time later.

![Documents](/img/Project Folders - 01 - Documents.png)

For Windows users, use the following script – we created this in the last section:

    c:
    cd %userprofile%\documents
    mkdir webdev
    mkdir webdev\dev
    mkdir webdev\nginx
    mkdir webdev\nginx\php
    mkdir webdev\ssl


The above script will create a new folder called `webdev` in your `My Documents` folder under your account, it will also create 3 folders within `webdev`:

* Dev – For storing your development projects & files
* nGinx – For storing an instance of nGinx purely for this project
* ssl – For the OpenSSL certificate files

![webdev](/img/Project Folders - 02 - Documents - webdev.png)
  
### Extract nGinx
From the `Downloads` folder, double-click on the **nginx** Zip file and navigate down until you can see the sub-folders and `nginx.exe`. Select all of the files and copy them to the `webdev\nginx` folder.
 
Once done you should only see some subfolders and `nginx.exe` in the root folder:

![nginx](/img/Project Folders - 03 - Downloads - nginx.png)