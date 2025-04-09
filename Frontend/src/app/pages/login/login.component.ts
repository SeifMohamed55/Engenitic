import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../feature/users/user.service';
import { LoginData } from '../../interfaces/users/login';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, CommonModule, ToastrModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private destroyImage$ = new Subject<void>();
  loginResponse!: LoginData;
  buttonDisabled: boolean = false;

  loginForm: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.email, Validators.required]),
    password: new FormControl('', [
      Validators.minLength(5),
      Validators.required,
    ]),
    rememberMe: new FormControl(false, [Validators.required]),
  });

  constructor(
    private _UserService: UserService,
    private toastr: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    // Listen for the message event from the popup window
    if (typeof window !== 'undefined') {
      window.addEventListener('message', this.receiveMessage.bind(this), false);
    }
  }

  ngOnDestroy(): void {
    // Cleanup event listener
    if (typeof window !== 'undefined') {
      window.removeEventListener(
        'message',
        this.receiveMessage.bind(this),
        false
      );
    }

    this.destroy$.next();
    this.destroy$.complete();

    this.destroyImage$.next();
    this.destroyImage$.complete();
  }

  handleRememberMe(event : Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.loginForm.get('rememberMe')?.patchValue(checked);
  }

  googleLogin(): void {
    const popup = window.open(
      'https://localhost/api/google/login',
      'PopupWindow',
      'width=800,height=600'
    );
  }

  receiveMessage(event: MessageEvent): void {
    if (event.origin !== 'https://localhost') {
      return;
    }

    const response = event.data;
    console.log(response);
    if (response.code === 200) {
      this.handleGoogleLoginSuccess(response);
    } else if (response.code === 400) {
      this.toastr.error(response.message);
    } else {
      this.toastr.error('an error occured to the server try again later !');
    }
  }

  handleGoogleLoginSuccess(response: any): void {
    // Implement handling of successful Google login, similar to your normal login
    this._UserService.registered.next(response.data.AccessToken);
    this._UserService.userId.next(response.data.Id);
    this._UserService.userName.next(response.data.Name);
    this._UserService.role.next(response.data.Roles[0]);
    this._UserService.image.next(response.data.Image.ImageURL);

    localStorage.setItem('Token', response.data.AccessToken);
    localStorage.setItem('name', response.data.Name);
    localStorage.setItem('id', String(response.data.Id));
    localStorage.setItem('role', response.data.Roles[0]);
    localStorage.setItem('image_url', response.data.Image.ImageURL);

    this.toastr.success(response.message);
    this._Router.navigate(['/home']);
  }

  handleLogin() {
    if (!this.loginForm.valid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    console.log(this.loginForm.value);
    this.buttonDisabled = true;

    this._UserService
      .loginData(this.loginForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (!res?.data) {
            this.toastr.error('Invalid response from server');
            this.buttonDisabled = false;
            return;
          }

          this.loginResponse = res.data;
          this._UserService.registered.next(this.loginResponse.accessToken);
          this._UserService.userId.next(this.loginResponse.id);
          this._UserService.userName.next(this.loginResponse.name);
          this._UserService.role.next(this.loginResponse.roles[0]);
          this._UserService.image.next(this.loginResponse.image.imageURL);

          localStorage.setItem('Token', this.loginResponse.accessToken);
          localStorage.setItem('name', this.loginResponse.name);
          localStorage.setItem('id', String(this.loginResponse.id));
          localStorage.setItem('role', this.loginResponse.roles[0]);
          localStorage.setItem('image_url', this.loginResponse.image.imageURL);

          this.toastr.success(res.message);
          this._Router.navigate(['/home']);
        },
        error: (err) => {
          const errorMessage =
            err?.error?.message ||
            'An error occurred while connecting to the server. Please try again later.';
          this.toastr.error(errorMessage);
        },
      })
      .add(() => {
        this.buttonDisabled = false;
      });
  }
}
