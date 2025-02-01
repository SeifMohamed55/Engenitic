import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {

  disableButton : boolean = false;
  selectedFile: File | null = null; // Store the file externally
  fileValidationError: string | null = null;

  registerForm: FormGroup = new FormGroup(
    {
      phoneGroup: new FormGroup({
        countryCode: new FormControl('', [
          Validators.pattern(/^\+\d{1,4}$/), // Country code validation
        ]),
        phone: new FormControl('', [
          Validators.pattern(/^\d{7,15}$/), // Phone number validation
        ]),
      }, this.phoneNumberValidation), 
      image: new FormControl(''),
      email: new FormControl('', [Validators.email, Validators.required]),
      userName: new FormControl('', [Validators.required]),
      password: new FormControl('', [Validators.required, Validators.minLength(5)]),
      repassword: new FormControl('', [Validators.required, Validators.minLength(5)]),
      userType: new FormControl('student'),
    },
    {
      validators: [this.confirmPassword, this.phoneNumberValidation],
    }
  );

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

  phoneNumberValidation(group: AbstractControl) : ValidationErrors | null{
    const countryCode = group.get('countryCode');
    const phoneNumber = group.get('phone');

    if(!countryCode?.value && phoneNumber?.value){
      return {countryCodeMissing : true}
    }
    else if(countryCode?.value && !phoneNumber?.value){
      return {phoneNumberMissing : true}
    }
    else if(countryCode?.invalid){
      return {countryCodeInvalid : true}
    }
    else if(countryCode?.invalid){
      return {countryCodeInvalid : true}
    }
    else if(phoneNumber?.invalid){
      return {phoneNumberInvalid : true}
    }
    return null
  }


  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;


    const acceptableTypes: string[] = ['image/jpg', 'image/png', 'image/jpeg'];
    const maxSize = 2 * 1024 * 1024; // 2 MB

    // Validate the file type and size
    if (!acceptableTypes.includes(this.selectedFile.type)) {
      this.selectedFile = null;
      this.fileValidationError = "invalid image type";
      return ;
    }

    if (this.selectedFile.size > maxSize) {
      this.fileValidationError = 'image size must be less than 2MB';
      this.selectedFile = null;
      return ;
    }
  }
    else {
      this.selectedFile = null; // Reset selected file
    }
}


  registerSubmit(): void {
    this.disableButton = true;
    if (this.registerForm.valid) {
      const formData = new FormData();
      formData.append('userName', this.registerForm.get('userName')?.value);
      formData.append('email', this.registerForm.get('email')?.value);
      formData.append('password', this.registerForm.get('password')?.value);
      formData.append('repassword', this.registerForm.get('repassword')?.value);
      formData.append('phoneNumber', this.registerForm.get('phoneGroup')?.get('phone')?.value);
      formData.append('countryCode', this.registerForm.get('phoneGroup')?.get('countryCode')?.value);
      formData.append('userType', this.registerForm.get('userType')?.value);
      if(this.selectedFile){
        formData.append('image', this.selectedFile);
      }
      formData.forEach(el => console.log(el));
    } else {
      this.registerForm.markAllAsTouched();
    }
    this.disableButton = false;
  }
}
