import { Component } from '@angular/core';
import { Router } from "@angular/router"; //for logout click event
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'nav-menu',
    templateUrl: './navmenu.component.html',
    styleUrls: ['./navmenu.component.css']
})
export class NavMenuComponent {
    
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
