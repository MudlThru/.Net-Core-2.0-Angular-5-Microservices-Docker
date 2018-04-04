import { Component } from '@angular/core';
//AL 20180222
import { Router } from "@angular/router"; //for logout click event
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  
  //AL 20180222
  constructor(
        public auth: AuthService,
        private router: Router
    ){
    }

    logout(): boolean {
        //logs out the current user, then redirects them to the Home view
        if(this.auth.logout()){
            this.router.navigate([""]);
        }
        return false;
    }
  
}
