import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {FormGroup, FormControl, Validators} from '@angular/forms';
import {ReactiveFormsModule} from '@angular/forms';
import { UserService } from '../../feature/users/user.service';
import { Login } from '../../interfaces/users/login';
import { ToastrModule, ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    ToastrModule,
    ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  constructor (private _UserService:UserService, private toastr: ToastrService, private _Router:Router){

  }

  loginResponse !: Login;
  buttonDisabled : boolean = false;

  loginForm : FormGroup = new FormGroup ({
    email : new FormControl('', [Validators.email, Validators.required]),
    password : new FormControl('', [Validators.minLength(5), Validators.required])
  });

  handleLogin(){

    this.buttonDisabled = true;

    if(this.loginForm.valid){
      this._UserService.loginData(this.loginForm.value).subscribe({
        next : res =>{
          console.log(res);
          this.loginResponse = res.data;
          this._UserService.registered.next(this.loginResponse.accessToken);
          this._UserService.userId.next(this.loginResponse.id);
          this._UserService.userName.next(this.loginResponse.name);
          localStorage.setItem('Token',this.loginResponse.accessToken);
          localStorage.setItem('name',this.loginResponse.name);
          localStorage.setItem('id',String(this.loginResponse.id));
          this._UserService.getUserImage(this.loginResponse.id).subscribe({
            next : res => {
              this._UserService.image.next(URL.createObjectURL(res));
              localStorage.setItem('image', this._UserService.image.value);
            },
            error : err =>{
              console.log(err);
            }
          });
          this.toastr.success(res.message);
          this._Router.navigate(["/home"]);
        },
        error : err =>{
          if ( err.error.message ){
            this.toastr.error(err.error.message);
          }
          else {
            this.toastr.error("an error has during connecting to the server occured try again later");
          }
        }
      });
    }
    else {
      this.loginForm.markAllAsTouched();
    }
    this.buttonDisabled = false;
  };
}
