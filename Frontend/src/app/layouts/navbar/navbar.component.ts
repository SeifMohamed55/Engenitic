import { CommonModule } from '@angular/common';
import { Component, NgModule, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements OnInit {

  constructor(private _Router:Router){

  }
  
  registered : string | null = 'asdasdasd';
  userName : string = 'abdelrhman khaled';
  userPicture : string = '';
  userRole : string = '';


  ngOnInit(): void {

  }

  handleLogout(): void {
    localStorage.clear();
    this._Router.navigate(['/home']);
    this.registered = null;
  };
  
  handleProfile():void {

  };
}
