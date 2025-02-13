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

  constructor (private _UserService:UserService, private toastr: ToastrService){

  }

  loginResponse !: Login 

  loginForm : FormGroup = new FormGroup ({
    email : new FormControl('', [Validators.email, Validators.required]),
    password : new FormControl('', [Validators.minLength(5), Validators.required])
  });

  handleLogin(){
    if(this.loginForm.valid){
      this._UserService.loginData(this.loginForm.value).subscribe({
        next : res=>{
          this.loginResponse = res.user;
          console.log(this.loginResponse);
          this.toastr.success("user logged in successfully !");
        },
        error : err =>{
          console.log(err);
        }
      });
    }
    else {
      this.loginForm.markAllAsTouched();
    }
  }
}
