import { Component, Inject } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { AuthService } from "../../services/auth.service";

@Component({
    selector: "admintest",
    templateUrl: './admintest.component.html',
    styleUrls: ['./admintest.component.css']
})

export class AdminTestComponent {
    admintest: string;//Value;

    constructor(private activatedRoute: ActivatedRoute,
        private router: Router,
        private http: HttpClient,
        public auth: AuthService,
        @Inject('BASE_URL') private baseUrl: string) {

        // create an empty object from the admintest interface
        //this.admintest = <admintest>{};

        //var id = +this.activatedRoute.snapshot.params["id"];
        //console.log(id);
        //if (id) {
            //var url = 'https://localhost/api.php';//this.baseUrl + "api.php";
            var url = this.baseUrl + "auth/admin";// + id;

            //this.http.get<admintest>(url).subscribe(result => {
			this.http.get<string>(url).subscribe(result => {
                this.admintest = result;
            }, error => console.error(error));
        //}
        //else {
        //    console.log("Invalid id: routing back to home...");
        //    this.router.navigate(["home"]);
        //}
    }
	
}
