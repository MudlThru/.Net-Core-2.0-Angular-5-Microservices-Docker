@echo off
echo create self-signed cert for development and testing
C:\OpenSSL-Win64\bin\openssl.exe req -config C:\OpenSSL-Win64\bin\openssl.cfg -x509 -newkey rsa:4096 -keyout %userprofile%\documents\webdev\ssl\cert.key -out %userprofile%\documents\webdev\ssl\cert.pem -days 365 -subj "/CN=localhost" -nodes
C:\OpenSSL-Win64\bin\openssl.exe pkcs12 -keypbe PBE-SHA1-3DES -certpbe PBE-SHA1-3DES -export -in %userprofile%\documents\webdev\ssl\cert.pem -inkey %userprofile%\documents\webdev\ssl\cert.key -out %userprofile%\documents\webdev\ssl\localhost.pfx -name "localhost"
pause