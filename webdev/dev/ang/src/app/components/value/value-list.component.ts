import { Component, Inject, Input, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from '@angular/common/http';

@Component({
    selector: "value-list",
    templateUrl: './value-list.component.html',
    styleUrls: ['./value-list.component.css']
})

export class ValueListComponent implements OnInit {
    @Input() class: string;
    title: string;
    selectedValue: Value;
    values: Value[];

    constructor(private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        private router: Router) {
    }

    ngOnInit() {
        console.log("ValueListComponent " +
            " instantiated with the following class: "
            + this.class);

        var url = this.baseUrl + "api/values/";

        switch (this.class) {
            case "latest":
            default:
                this.title = "Latest Values";
                //url += "Latest/";
                break;
            case "byTitle":
                this.title = "Values by Title";
                url += "ByTitle/";
                break;
            case "random":
                this.title = "Random Values";
                url += "Random/";
                break;
        }

        this.http.get<Value[]>(url).subscribe(result => {
            this.values = result;
        }, error => console.error(error));
    }

    onSelect(value: Value) {
        this.selectedValue = value;
        console.log("value with Id "
            + this.selectedValue.Id
            + " has been selected.");
        this.router.navigate(["/value", this.selectedValue.Id]);
    }
}
