import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../feature/users/user.service';
import { Login } from '../../interfaces/users/login';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';

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
export class LoginComponent implements OnDestroy {

  constructor(
    private _UserService: UserService, 
    private toastr: ToastrService, 
    private _Router: Router
  ) {}

  private destroy$ = new Subject<void>();
  loginResponse!: Login;
  buttonDisabled: boolean = false;

  loginForm: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.email, Validators.required]),
    password: new FormControl('', [Validators.minLength(5), Validators.required])
  });

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handleLogin() {
    if (!this.loginForm.valid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.buttonDisabled = true;

    this._UserService.loginData(this.loginForm.value).pipe(takeUntil(this.destroy$)).subscribe({
      next: res => {
        if (!res?.data) {
          this.toastr.error("Invalid response from server");
          this.buttonDisabled = false;
          return;
        }

        this.loginResponse = res.data;
        this._UserService.registered.next(this.loginResponse.accessToken);
        this._UserService.userId.next(this.loginResponse.id);
        this._UserService.userName.next(this.loginResponse.name);
        this._UserService.role.next(this.loginResponse.roles[0]);

        localStorage.setItem('Token', this.loginResponse.accessToken);
        localStorage.setItem('name', this.loginResponse.name);
        localStorage.setItem('id', String(this.loginResponse.id));
        localStorage.setItem('role', this.loginResponse.roles[0]);

        this._UserService.getUserImage(this.loginResponse.id).pipe(takeUntil(this.destroy$)).subscribe({
          next: res => {
            const imageUrl = URL.createObjectURL(res);
            this._UserService.image.next(imageUrl);
            localStorage.setItem('image', imageUrl);
          },
          error: err => console.error("Failed to fetch user image", err)
        });

        this.toastr.success(res.message);
        this._Router.navigate(["/home"]);
      },
      error: err => {
        const errorMessage = err?.error?.message || "An error occurred while connecting to the server. Please try again later.";
        this.toastr.error(errorMessage);
      }
    }).add(() => {
      this.buttonDisabled = false; // Ensure button is re-enabled after request completes
    });
  }
}
