import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-confirm-password',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './confirm-password.component.html',
  styleUrl: './confirm-password.component.scss',
})
export class ConfirmPasswordComponent {
  registerForm: FormGroup = new FormGroup(
    {
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(5),
      ]),
      repassword: new FormControl('', [
        Validators.required,
        Validators.minLength(5),
      ]),
    },
    {
      validators: [this.confirmPassword],
    }
  );

  //confirm pass function
  confirmPassword(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const repassword = group.get('repassword')?.value;

    if (password !== repassword) {
      group.get('repassword')?.setErrors({ passwordMiss: true });
    } else {
      group.get('repassword')?.setErrors(null);
    }
    return null;
  }

  handleNewPassword(): void {
    if (this.registerForm.valid) {
      console.log(this.registerForm.value);
    } else {
      this.registerForm.markAllAsTouched();
    }
  }
}
