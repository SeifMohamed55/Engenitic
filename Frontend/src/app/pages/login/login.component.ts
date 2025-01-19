import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {FormGroup, FormControl, Validators} from '@angular/forms';
import {ReactiveFormsModule} from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  loginForm : FormGroup = new FormGroup ({
    email : new FormControl('', [Validators.email, Validators.required]),
    password : new FormControl('', [Validators.min(5), Validators.required])
  });

  handleLogin(){
    if(this.loginForm.valid){
      alert(JSON.stringify(this.loginForm.value));
    }
    else {
      this.loginForm.markAllAsTouched();
    }
  }
}
