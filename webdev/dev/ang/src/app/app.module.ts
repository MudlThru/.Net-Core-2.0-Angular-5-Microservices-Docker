import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

// Imports from previous project example
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
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
        { path: 'about', component: AboutComponent },
        { path: 'login', component: LoginComponent },
        { path: 'register', component: RegisterComponent },
        { path: '**', component: PageNotFoundComponent }
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
