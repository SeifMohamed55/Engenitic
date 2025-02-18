import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { Component,  OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements OnInit {

  constructor(private _Router:Router, private _UserService:UserService, private _ToastrService:ToastrService){

  }



  userId !: number;
  registered : string | null = null 
  userName : string = 'abdelrhman khaled';
  userPicture : string = '';
  userRole : string = '';

  ngOnInit(): void {

    this._UserService.registered.subscribe(data=>{
      this.registered = data
    });

    this._UserService.userId.subscribe(data =>{
      this.userId = data;
    });

    if (typeof window !== 'undefined') {
      const token: string | null = localStorage.getItem("Token");
      if (token) {
        this._UserService.registered.next(token);
      }
    }
  };


  handleLogout(): void {
    this._Router.navigate(['/home']);
    this._UserService.logoutConfirmation().subscribe({
      next : res =>{
        this._ToastrService.success(res.message);
        localStorage.clear();
        this._UserService.registered.next("");
      },
      error : err => {
        console.log(err);
        this._ToastrService.error("something went wrong try again later !");
      }
    });
  };
}
