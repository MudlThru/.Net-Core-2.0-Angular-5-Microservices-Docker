import { EventEmitter, Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import 'rxjs/Rx';

@Injectable()
export class AuthService {
    authKey: string = 'auth';
    // phpKey: string = 'php'; // TODO: is this in use?
    clientId: string = 'MyApp'; // TODO: this will need to be updated

    constructor(
        private http: HttpClient,
        @Inject(PLATFORM_ID) private platformId: any
    ) {

    }

    // performs the login
    login(username: string, password: string): Observable<boolean> {
        var url = 'auth/token/auth';
        var data = {
            username: username,
            password: password,
            client_id: this.clientId,
            // required when signing up with username/password
            grant_type: 'password',
            // space seperated list of scopes for which the token is issued
            scope: 'offline_access profile email'
        };
        return this.getAuthFromServer(url, data);
    }

    // try to refresh token
    refreshToken(): Observable<boolean> {
        var url = 'auth/token/auth';
        var data = {
            client_id: this.clientId,
            // required when signing up with username/password
            grant_type: 'refresh_token',
            refresh_token: this.getAuth()!.refresh_token,
            // space-seperated list of scopes for which the token is issued
            scope: 'offline_access profile email'
        };

        return this.getAuthFromServer(url, data);
    }

    // performs the logout
    logout(): boolean {
        this.setAuth(null);
        return true;
    }

    // persist auth into localStorage or removes it if a NULL argument is given
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

    // retrieves the auth JSON object (or NULL if none)
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

    // retrieve the access & refresh tokens from the server
    getAuthFromServer(url: string, data: any): Observable<boolean> {
        return this.http.post<TokenResponse>(url, data)
            .map((res) => {
                let token = res && res.token;
                // if the token is there, login has been successful
                if(token) {
                    // store username and jwt token
                    this.setAuth(res);
                    // successful login
                    return true;
                }
            })
            .catch(error => {
                return new Observable<any>(error);
            })
    }

    // returns TRUE if the user is logged in, FALSE otherwise.
    isLoggedIn(): boolean {
        if(isPlatformBrowser(this.platformId)){
            return localStorage.getItem(this.authKey) != null;
        }
        return false;
    }

}
