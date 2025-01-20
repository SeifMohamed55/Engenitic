import { Component } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';


@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm : FormGroup = new FormGroup({
    image : new FormControl(''),
    email : new FormControl(''),
    userName : new FormControl(''),
    password: new FormControl(''),
    repassword : new FormControl(''),
    phone : new FormControl(''),
    userType : new FormControl('student')
  });

  registerSubmit(){
    if(this.registerForm.valid){
      console.log(this.registerForm.value);
    }
    else {
      this.registerForm.markAllAsTouched();
    }
  }
}
