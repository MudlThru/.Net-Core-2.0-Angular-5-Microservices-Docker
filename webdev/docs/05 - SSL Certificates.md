# .Net Core 2.0, Angular 5, Microservices & Docker

## SSL Certificates

### Creating our development SSL certificate
Next go back to VS Code and copy the code below, saving it to `webdev\ssl\createTestCert.bat`, then browse to the `webdev\ssl` folder and double-click on the script.

    @echo off

    echo create self-signed cert for development and testing

    C:\OpenSSL-Win64\bin\openssl.exe req -config C:\OpenSSL-Win64\bin\openssl.cfg -x509 -newkey rsa:4096 -keyout %userprofile%\documents\webdev\ssl\cert.key -out %userprofile%\documents\webdev\ssl\cert.pem -days 365 -subj "/CN=localhost" -nodes

    C:\OpenSSL-Win64\bin\openssl.exe pkcs12 -keypbe PBE-SHA1-3DES -certpbe PBE-SHA1-3DES -export -in %userprofile%\documents\webdev\ssl\cert.pem -inkey %userprofile%\documents\webdev\ssl\cert.key -out %userprofile%\documents\webdev\ssl\localhost.pfx -name "localhost"

This script creates three files a random key (`cert.key`), a certificate (`cert.pem`) and a personal certificate we can use in .Net or IIS (`localhost.pfx`).

![SSL](/img/Project Folders - 04 - Documents - webdev - ssl.png)

The `cert.pem` and `cert.key` will be used by nGinx for our local web server, we can use the `localhost.pfx` for testing in VSCode.
 
### Trusting our certificate
In the `webdev\ssl` folder right-click on the `localhost.pfx` file and select **Install PFX** from the context menu.

![Install PFX](/img/Project Folders - 05 - Documents - webdev - ssl - install cert.png)
 
In the Store Location box select **Current User** and then click **Next**.

![Current User](/img/Project Folders - 06 - Cert Import - Current User.png)
 
The files path will be populated for you, click Next:

![File name](/img/Project Folders - 07 - Cert Import - File name.png)
 
Enter a password, this is so you can export the private key later. For our purposes any password will do:

![Password](/img/Project Folders - 08 - Cert Import - Password.png)
 
For the certificate store, select **Place all certificates in the following store** and then browse to **Trusted Root Certification Authorities**, then click **Next**:

![Cert store](/img/Project Folders - 09 - Cert Import - Cert Store.png)
 
Click **Finish**:

![Completion](/img/Project Folders - 10 - Cert Import - Finish.png)
 
A security warning box will show, click **Yes**. _We can trust this certificate because we created it._

![Security prompt](/img/Project Folders - 11 - Cert Import - Confirm.png)
 
To check your certificate open the **Start** menu and type in `cert`, then select **Manage user certificates** from the search results:

![Manage Certs](/img/Project Folders - 12 - Start - Manage Certs.png)
 
Navigate to **Trusted Root Certification Authorities** > **Certificates**, you should then see your **localhost** certificate in the list. _If you need to remove the certificate you can do so from here._

![certmgr](/img/Project Folders - 13 - certmgr - Trusted Root - Certificates.png)