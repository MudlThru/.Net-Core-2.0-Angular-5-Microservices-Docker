# .Net Core 2.0, Angular 5, Microservices & Docker
## The front end
Unless you are purely building a web facing api, as a method for applications to communicate, you are going to want some kind of front-end user interface.

For this project I have chosen Angular 5; you can of course opt for whichever web front-end technology you feel comfortable with, providing it can leverage our apis.

### Services
#### App Module
The app module is the starting point for your Angular application, in general your components, services etc. will be registered here.

Our `app` module file can be found at `webdev\dev\ang\src\app\app.module.ts`. If you compare the version below with the version the Angular created for you, you will see that we have changed some of the `@angular` `import` statements and included imports for our own serices and components. You will find all of the code for these in the `webdev\dev\ang` folder. 

You will also notice that we need to create some folders to provide our Angular project with some structure. We need to create the following folders under the `webdev\dev\ang\src\app\` folder:
* Components
* Interfaces
* Services

Once these folders are created move the `app.component.*` files into a new `app` folder underneath the `components` folder.

Your `webdev/dev/ang/src/app/app.module.ts` should look like this:
```ts
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

// Imports from previous project example
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; //updated from 'FormsModule'
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http'; //updated from '@angular/http';
import { RouterModule } from '@angular/router';

// Services
import { AuthService } from './services/auth.service';
import { AuthInterceptor } from './services/auth.interceptor';
import { AuthResponseInterceptor } from './services/auth.response.interceptor';

// Components
import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';

import { AboutComponent } from './components/about/about.component';

import { LoginComponent } from './components/login/login.component';
import { LoginExternalProvidersComponent } from './components/login/login.externalproviders.component';

import {RegisterComponent } from './components/user/register.component';

import { PageNotFoundComponent } from './components/pagenotfound/pagenotfound.component';

import { ValueComponent } from './components/value/value.component';
import { ValueListComponent } from './components/value/value-list.component';

import { PHPTestComponent } from './components/phptest/phptest.component';
import { AdminTestComponent } from './components/admintest/admintest.component';

@NgModule({
declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    
    AboutComponent,

    LoginComponent,
    LoginExternalProvidersComponent,

    RegisterComponent,

    PageNotFoundComponent,
    
    ValueComponent,
    ValueListComponent,
    
    PHPTestComponent,
    
    AdminTestComponent
    
],
imports: [
    BrowserModule,
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
        { path: '', redirectTo: 'home', pathMatch: 'full' },
        
        { path: 'home', component: HomeComponent },
        
        { path: 'about', component:AboutComponent },
        
        { path: 'login', component:LoginComponent },
        
        { path: 'register', component:RegisterComponent },
        
        { path: '**', component:PageNotFoundComponent }//{ path: '**', redirectTo: 'home' }
    ])
],
providers: [
        AuthService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthResponseInterceptor,
            multi: true
        },
        { 
            provide: 'BASE_URL', 
            useValue: '/' 
        }
    ],
bootstrap: [AppComponent]
})
export class AppModule { }
```

____
**Attribution:** 

The following sections refers to code taken from [ASP.NET Core 2 and Angular 5](https://www.packtpub.com/application-development/aspnet-core-2-and-angular-5) by Valerio De Sanctis (Published by [Packt](https://github.com/PacktPublishing)) ( [Source](https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5) ,
[License](https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5/blob/master/LICENSE) ). 

I do not intend to explain in too much detail what this code is doing, it is in essence placeholders for your own code but provide a good reference point from which to work from.

If you require further information or would like to learn more, I fully encourage you to read, and work through, this book.
____

### Interfaces
Interfaces are essentially how Angular understands the data passed from our Api(s), you will need to create the following interface TypeScript files within the newly created interfaces folder:

`user.ts` is the Angular class to accompany the .Net class from the `auth` WebAPI project.
```ts
interface User {
    UserName: string;
    Password: string;
    Email: string;
    DisplayName: string;
}
```
`token.response.ts` is the Angular class to accompany the .Net class from the `auth` WebAPI project.
```ts
interface TokenResponse {
    token: string;
    expiration: number;
    refresh_token: string
}
```
`value.ts` is the Angular class to accompany the .Net class from the `api` WebAPI project.
```ts
interface Value { 
    Id: number;
    Title: string;
    Description: string;
    Text: string;
}
```
### Services
`Auth.service.ts` is the bit of magic that allows Angular to authenticate the user with our `auth` WebAPI. In order for it to work seamlessly we have to emply some tricks.
```ts
import { EventEmitter, Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { isPlatformBrowser } from "@angular/common";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import "rxjs/Rx";

@Injectable()
export class AuthService {
    authKey: string = "auth";
    phpKey: string = "php"; //TODO: is this in use?
    clientId: string = "TestMakerFree"; //TODO: this will need to be updated

    constructor(
        private http: HttpClient,
        @Inject(PLATFORM_ID) private platformId: any
    ) 
    {

    }

    //performs the login
    login(username: string, password: string): Observable<boolean> {
        var url = "auth/token/auth";
        var data = {
            username: username,
            password: password,
            client_id: this.clientId,
            //required when signing up with username/password
            grant_type: "password",
            //space seperated list of scopes for which the token is issued
            scope: "offline_access profile email"
        };
        return this.getAuthFromServer(url, data);
    }

    //try to refresh token
    refreshToken(): Observable<boolean> {
        var url = "auth/token/auth";
        var data = {
            client_id: this.clientId,
            //required when signing up with username/password
            grant_type: "refresh_token",
            refresh_token: this.getAuth()!.refresh_token,
            //space-seperated list of scopes for which the token is issued
            scope: "offline_access profile email"
        };

        return this.getAuthFromServer(url, data);
    }

    //performs the logout
    logout(): boolean {
        this.setAuth(null);
        return true;
    }

    //persist auth into localStorage or removes it if a NULL argument is given
    setAuth(auth: TokenResponse | null): boolean {
        if(isPlatformBrowser(this.platformId)){
            if(auth){
                if(!localStorage){
                    if('localStorage' in window && window['localStorage'] !== null){
                        localStorage = window.localStorage;
                    }
                } 
                localStorage.setItem(
                    this.authKey,
                    JSON.stringify(auth)
                );
            } else {
                localStorage.removeItem(this.authKey);
            }
        }
        return true;
    }

    //retrieves the auth JSON object (or NULL if none)
    getAuth(): TokenResponse | null {
        if(isPlatformBrowser(this.platformId)){
            if(!localStorage){
                if('localStorage' in window && window['localStorage'] !== null){
                    localStorage = window.localStorage;
                }
            } else {
                var auth = localStorage.getItem(this.authKey);
            }

            var i = localStorage.getItem(this.authKey);
            if(i) {
                return JSON.parse(i);
            }
        }
        return null;
    }

    //retrieve the access & refresh tokens from the server
    getAuthFromServer(url: string, data: any): Observable<boolean> {
        return this.http.post<TokenResponse>(url, data)
            .map((res) => {
                let token = res && res.token;
                //if the token is there, login has been successful
                if(token) {
                    //store username and jwt token
                    this.setAuth(res);
                    //successful login
                    return true;
                }
            })
            .catch(error => {
                return new Observable<any>(error);
            })
    }

    //returns TRUE if the user is logged in, FALSE otherwise.
    isLoggedIn(): boolean {
        if(isPlatformBrowser(this.platformId)){
            return localStorage.getItem(this.authKey) != null;
        }
        return false;
    }

}
```

____
**Note:** The following two files are pure magic, you don't need to understand how they work but it is probably worth spending a few minutes looking through the code.
____

`Auth.interceptor.ts` is the first trick we use to intercept any requests to the api(s) and append the bearer token to the HTTP headers, if the user is logged in.
```ts
import { Injectable, Injector } from "@angular/core";
import { HttpHandler, HttpEvent, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { AuthService } from "./auth.service";
import { Observable } from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private injector: Injector){}

    //Adds our authorization token to the header
    intercept(
        request: HttpRequest<any>,
        next: HttpHandler): Observable<HttpEvent<any>> {
            var auth = this.injector.get(AuthService);
            var token = (auth.isLoggedIn()) ? auth.getAuth()!.token : null;
            if(token) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${token}`
                    }
                });
            }
            return next.handle(request);
        }

}
```
`Auth.response.interceptor.ts` is the second trick, this will capture the responses from the api(s) and request a refresh token if it has expired.
```ts
import { Injectable, Injector } from "@angular/core";
import { Router } from "@angular/router";
import { 
    HttpClient, 
    HttpHandler, HttpEvent, HttpInterceptor, 
    HttpRequest, HttpResponse, HttpErrorResponse 
} from "@angular/common/http";
import { AuthService } from "./auth.service";
import { Observable } from "rxjs"; //"rxjs/Observable";

@Injectable()
export class AuthResponseInterceptor implements HttpInterceptor {
    currentRequest: HttpRequest<any>;
    auth: AuthService;

    constructor(
        private injector: Injector,
        private router: Router
    )
    { }

    intercept(
        request: HttpRequest<any>,
        next: HttpHandler): Observable<HttpEvent<any>> 
    {
        this.auth = this.injector.get(AuthService);
        var token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;

        if(token) {
            //save current request
            this.currentRequest = request;

            return next.handle(request)
                .do((event: HttpEvent<any>) => {
                    if(event instanceof HttpResponse) {
                        //do nothing
                    }
                })
                .catch(error => {
                    return this.handleError(error, next)
                });
        }
        else {
            return next.handle(request);
        }

    }

    handleError(err: any, next: HttpHandler) {
        if(err instanceof HttpErrorResponse) {
            if(err.status === 401) {
                //JWT token might have expired:
                //try to get a new one using refresh token
                
                //  ===[2018.01.05 FIX - BOOK UPDATE]===
                // cfr. https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5/issues/8
                // and  https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5/issues/15
                // store current request into a local variable
                var previousRequest = this.currentRequest;

                // thanks to @mattjones61 for the following code
                return this.auth.refreshToken()
                    .flatMap((refreshed) => {
                        var token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;
                        if (token) {
                            previousRequest = previousRequest.clone({
                                setHeaders: { Authorization: `Bearer ${token}` }
                            });
                            console.log("header token reset");
                        }

                        return next.handle(previousRequest);
                    });
            }
        }
        return Observable.throw(err);
    }

}
```
### Components
Now onto our components. If you don’t have any experience of Angular, you may not know that each component describes the rendering and functionality of a part, or component, of the application.

#### App
The `App` component contains base functionality, in our case just the logout function, but you can add whatever you want here. 

#### Home
The `Home` component currently has no functionality but does provide the layout for our home screen with three embedded components, the list of values, the PHP authentication test and the Admin role authentication test.

#### NavMenu
The `NavMenu` component has limited functionality, apart from the logout function. It does, however, do some nifty stuff in the menu to show/hide options based on whether the user is logged in or not.

#### User 
The `user` component is a registration form with validation. On form submission the data is sent to our `auth` WebAPI, at which point the data is used to check and create a new `user` record.

#### Login 
The `login` component handles the custom user login. The component also includes the placeholder for the 3rd party external login providers.


#### External Providers
The `External Providers` component provides the login buttons for each of the 3rd party providers.

#### PHPTest
`PHPTest` is a simple component used to query our PHP api and return a string value if the user has the required permission, it doesn't do anythingelse.

#### AdminTest 
`AdminTest` is similar to `PHPTest` with the exception that it requires the **Administrator** role. 

### Progressive Web Apps
Moving forward I intend to rewrite this section to make use of the Angular Progressive Web App template.

https://vitalflux.com/angular-5-create-progressive-web-apps-pwa/
