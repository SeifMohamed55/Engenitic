import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit, OnDestroy {

  constructor(
    private _UserService: UserService,
    private _ToastrService: ToastrService
  ) {}

  private destroy$ = new Subject<void>();
  userId: number = 0;
  userEmail: string = '';
  userName: string = '';
  userImage: { name: string; Url: string } = { name: '', Url: '' };
  disableButtonEmail = true;
  disableButtonUserName = true;
  disableButtonImage = true;
  disableButtonPassword = false;
  selectedFile: File | null = null;
  previewUrl: string | null = null;
  fileValidationError: string | null = null;
  newEmail = '';
  newUserName = '';

  emailForm = new FormGroup({
    email: new FormControl(this.userEmail, [Validators.required, Validators.email])
  });

  userNameForm = new FormGroup({
    userName: new FormControl(this.userName, [Validators.required])
  });

  passwordForm = new FormGroup({
    oldPassword: new FormControl('', [Validators.required, Validators.minLength(5)]),
    newPassword: new FormControl('', [Validators.required, Validators.minLength(5)])
  }, {
    validators : [this.passwordValidation]
  });

  ngOnInit(): void {
    if (typeof window !== 'undefined' && localStorage) {
      this.userId = parseInt(localStorage.getItem('id')!) || 0;
      this.userName = localStorage.getItem('name') || '';
      this.previewUrl = localStorage.getItem('image_url') || '';
      this.userImage.Url = localStorage.getItem('image_url') || '';
    }
    this.initiateForms();

    this.emailForm.get('email')?.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(newVal => {
      this.newEmail = newVal || '';
      this.disableButtonEmail = !newVal || newVal === this.userEmail;
    });

    this.userNameForm.get('userName')?.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(newVal => {
      this.newUserName = newVal || '';
      this.disableButtonUserName = !newVal || newVal === this.userName;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  initiateForms(): void {
    this._UserService.getProfileData(this.userId).pipe(takeUntil(this.destroy$)).subscribe({
      next: res => {
        console.log(res);
        this.userImage.name = res.data.image.name;
        this.userEmail = res.data.email;
        this.emailForm.patchValue({ email: this.userEmail });
        this.userNameForm.patchValue({ userName: this.userName });
        this.passwordForm.reset();
      },
      error: err => {
        this._ToastrService.error(err.error?.message || 'An error occurred.');
      }
    });
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;
    this.disableButtonImage = true;
    const acceptableTypes = ['image/jpg', 'image/png', 'image/jpeg'];
    const maxSize = 2 * 1024 * 1024;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      if (file.name === this.userImage.name) {
        this.fileValidationError = 'No changes detected! Your profile picture remains the same.';
        return;
      }
      if (!acceptableTypes.includes(file.type)) {
        this.fileValidationError = 'Invalid image type. Only JPG, PNG, and JPEG are allowed.';
        return;
      }
      if (file.size > maxSize) {
        this.fileValidationError = 'Image size must be less than 2MB.';
        return;
      }

      this.selectedFile = file;
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        this.previewUrl = reader.result as string;
        this.disableButtonImage = false;
      };
    } else {
      this.selectedFile = null;
      this.previewUrl = null;
    }
  };

  passwordValidation (passwordForm : AbstractControl) : ValidationErrors | null {
    const oldPassword = passwordForm.get('oldPassword')?.value;
    const newPassword = passwordForm.get('newPassword')?.value;
    if(oldPassword === newPassword){
      return {samePassword : true}
    }
    return null;
  };

  handleEmailSubmit(): void {
    if (this.emailForm.valid) {
      this._UserService.updateEmail(this.userId, this.newEmail).pipe(takeUntil(this.destroy$)).subscribe({
        next: res => {
          this._ToastrService.success(res.message);
          this.userEmail = this.newEmail;
          this.emailForm.patchValue({ email: this.newEmail });
          this.disableButtonEmail = true;
        },
        error: err => {
          console.error(err);
        }
      });
    } else {
      this.emailForm.markAllAsTouched();
    }
  }

  handleUserNameSubmit(): void {
    if (this.userNameForm.valid) {
      this._UserService.updateUserName(this.userId, this.newUserName).pipe(takeUntil(this.destroy$)).subscribe({
        next: res => {
          this._ToastrService.success(res.message);
          this.userName = this.newUserName;
          this._UserService.userName.next(this.newUserName);
          localStorage.setItem('name', this.newUserName);
          this.disableButtonUserName = true;
        },
        error: err => {
          console.error(err);
        }
      });
    } else {
      this.userNameForm.markAllAsTouched();
    }
  }

  handleImageSubmit(): void {
    if (!this.selectedFile) return;
    const formData = new FormData();
    formData.append('image', this.selectedFile);
    formData.append('id', `${this.userId}`);
    this._UserService.updateImage(formData).pipe(takeUntil(this.destroy$)).subscribe({
      next: res => {
        this._ToastrService.success(res.message);
        this.userImage.name = this.selectedFile!.name;
        this.userImage.Url = this.previewUrl!;
        this._UserService.image.next(this.previewUrl!);
        localStorage.setItem('image_url', this.previewUrl!);
        this.disableButtonImage = true;
      },
      error: err => {
        console.error(err);
      }
    });
  }

  handlePasswordSubmit(): void {
    if (this.passwordForm.valid) {
      const {oldPassword, newPassword} : any = this.passwordForm.value;
      this._UserService.updatePassword(this.userId, oldPassword, newPassword).pipe(takeUntil(this.destroy$)).subscribe({
        next : res =>{
          this._ToastrService.success(res.message);
        },
        error : err =>{
          if(err.error.message) {
            this._ToastrService.error(err.error.message[0].description);
          }else {
            console.log(err);
          }
        }
      })
    }
    else {
      this.passwordForm.markAllAsTouched();
    }
  }
}
