import { Component, Inject } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { AuthService } from "../../services/auth.service";

@Component({
    selector: "value",
    templateUrl: './value.component.html',
    styleUrls: ['./value.component.css']
})

export class ValueComponent {
    value: string;//Value;

    constructor(private activatedRoute: ActivatedRoute,
        private router: Router,
        private http: HttpClient,
        public auth: AuthService,
        @Inject('BASE_URL') private baseUrl: string) {

        // create an empty object from the Value interface
        //this.value = <Value>{};

        var id = +this.activatedRoute.snapshot.params["id"];
        console.log(id);
        if (id) {
            var url = this.baseUrl + "api/values/" + id;

            //this.http.get<Value>(url).subscribe(result => {
			this.http.get<string>(url).subscribe(result => {
                this.value = result;
            }, error => console.error(error));
        }
        else {
            console.log("Invalid id: routing back to home...");
            this.router.navigate(["home"]);
        }
    }

	/*
    onEdit() {
        this.router.navigate(["value/edit", this.value.Id]);
    }


    onDelete() {
        if (confirm("Do you really want to delete this value?")) {
            var url = this.baseUrl + "api/value/" + this.value.Id;
            this.http
                .delete(url)
                .subscribe(result => {
                    console.log("Value " + this.value.Id + " has been deleted.");
                    this.router.navigate(["home"]);
                }, error => console.log(error));
        }
    }
	*/
	
}
