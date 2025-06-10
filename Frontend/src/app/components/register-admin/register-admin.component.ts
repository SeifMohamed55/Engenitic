import { Component } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';
import { UserService } from '../../feature/users/user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register-admin',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './register-admin.component.html',
  styleUrl: './register-admin.component.scss',
})
export class RegisterAdminComponent {
  private destroy$ = new Subject<void>();
  buttonDisabled: boolean = false;
  selectedFile: File | null = null;
  fileValidationError: string | null = null;

  constructor(
    private _ToastrService: ToastrService,
    private _UserService: UserService
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // the form variable
  registerForm: FormGroup = new FormGroup(
    {
      phoneGroup: new FormGroup(
        {
          countryCode: new FormControl('', [Validators.pattern(/^\+\d{1,4}$/)]),
          phone: new FormControl('', [Validators.pattern(/^\d{7,15}$/)]),
        },
        this.phoneNumberValidation
      ),
      image: new FormControl(''),
      email: new FormControl('', [Validators.email, Validators.required]),
      userName: new FormControl('', [Validators.required]),
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
      validators: [this.confirmPassword, this.phoneNumberValidation],
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

  //phone number validation
  phoneNumberValidation(group: AbstractControl): ValidationErrors | null {
    const countryCode = group.get('countryCode');
    const phoneNumber = group.get('phone');

    if (!countryCode?.value && phoneNumber?.value) {
      return { countryCodeMissing: true };
    } else if (countryCode?.value && !phoneNumber?.value) {
      return { phoneNumberMissing: true };
    } else if (countryCode?.invalid) {
      return { countryCodeInvalid: true };
    } else if (countryCode?.invalid) {
      return { countryCodeInvalid: true };
    } else if (phoneNumber?.invalid) {
      return { phoneNumberInvalid: true };
    }
    return null;
  }

  //handling images
  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;

      const acceptableTypes: string[] = [
        'image/jpg',
        'image/png',
        'image/jpeg',
      ];
      const maxSize = 2 * 1024 * 1024;

      if (!acceptableTypes.includes(this.selectedFile.type)) {
        this.selectedFile = null;
        this.fileValidationError = 'invalid image type';
        return;
      }

      if (this.selectedFile.size > maxSize) {
        this.fileValidationError = 'image size must be less than 2MB';
        this.selectedFile = null;
        return;
      }
    } else {
      this.selectedFile = null;
    }
  }

  // on submiting form
  registerSubmit(): void {
    this.buttonDisabled = true;
    if (this.registerForm.valid) {
      const formData = new FormData();
      formData.append('username', this.registerForm.get('userName')?.value);
      formData.append('email', this.registerForm.get('email')?.value);
      formData.append('password', this.registerForm.get('password')?.value);
      formData.append(
        'confirmPassword',
        this.registerForm.get('repassword')?.value
      );
      formData.append(
        'phoneNumber',
        this.registerForm.get('phoneGroup')?.get('phone')?.value
      );
      formData.append(
        'phoneRegion',
        this.registerForm.get('phoneGroup')?.get('countryCode')?.value
      );
      formData.append('role', 'admin');
      if (this.selectedFile) {
        formData.append('image', this.selectedFile);
      }
      this._UserService
        .registerAdmin(formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            this._ToastrService.success(res.message);
          },
          error: (err) => {
            if (err.error.message) {
              this._ToastrService.error(err.error.message);
            } else {
              this._ToastrService.error(
                'an error has during connecting to the server occured try again later'
              );
            }
          },
        });
    } else {
      this.registerForm.markAllAsTouched();
    }
    this.buttonDisabled = false;
  }
}
