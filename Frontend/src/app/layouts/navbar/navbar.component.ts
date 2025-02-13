import { CommonModule } from '@angular/common';
import { Component, NgModule, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {

  constructor(private _Router:Router, private _UserService:UserService){

  }


  userName : string = 'abdelrhman khaled';
  userPicture : string = '';
  userRole : string = '';


  handleLogout(): void {
    localStorage.clear();
    this._UserService.registered.next("");
    this._Router.navigate(['/home']);
  };
  
  handleProfile():void {

  };
}
