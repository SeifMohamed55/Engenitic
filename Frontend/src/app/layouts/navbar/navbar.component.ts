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
  registered : string | null = null;
  userName : string = '';
  userPicture : string = '';
  userRole : string = '';

  ngOnInit(): void {

    if(typeof localStorage !== 'undefined'){
      if(localStorage.getItem('Token')){
        this._UserService.registered.next(localStorage.getItem('Token')!);
      }
      if(localStorage.getItem('name')){
        this._UserService.userName.next(localStorage.getItem('name')!);
      }
      if(localStorage.getItem('image')){
        this._UserService.image.next(localStorage.getItem('image')!);
      }
      if(localStorage.getItem('id')){
        this._UserService.userId.next(parseInt(localStorage.getItem('id')!));
      }
    }


    this._UserService.registered.subscribe(data=>{
      if(data){
        this.registered = data;
      }
    });

    this._UserService.userId.subscribe(data =>{
      this.userId = data;
    });

    this._UserService.userName.subscribe(data =>{
      this.userName = data;
    });

    this._UserService.image.subscribe(image =>{
      this.userPicture = image;
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
        localStorage.clear();
        this._UserService.userId.next(0);
        this._UserService.image.next('');
        this._UserService.userName.next('');
        this._UserService.registered.next('');
        this._ToastrService.success(res.message);
      },
      error : err => {
        console.log(err);
        this._ToastrService.error("something went wrong try again later !");
      }, complete : () =>{
        this._UserService.registered.subscribe(data =>{
          this.registered = data;
        })
      }
    });
  };
}
