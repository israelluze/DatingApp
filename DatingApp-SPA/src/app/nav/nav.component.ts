import { AuthService } from './../_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public authService: AuthService, private alertity: AlertifyService) { }

  ngOnInit() {
  }

  login(){
      this.authService.login(this.model).subscribe(next => {
       this.alertity.success('Logged in successfully');
      }, error => {
        this.alertity.error(error);
      });
  }

  loggedIn(){
    return this.authService.loggedIn();
  }

  logout(){
    localStorage.removeItem('token');
    this.alertity.message('logged out');
  }

}
