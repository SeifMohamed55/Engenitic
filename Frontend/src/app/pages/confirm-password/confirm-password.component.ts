import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { UserService } from '../../feature/users/user.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-confirm-password',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './confirm-password.component.html',
  styleUrl: './confirm-password.component.scss',
})
export class ConfirmPasswordComponent implements OnInit, OnDestroy {
  destroy$ = new Subject<void>();
  buttonDisabled: boolean = false;
  registerForm: FormGroup = new FormGroup(
    {
      password: new FormControl('', [
        Validators.required,
        Validators.pattern(
          /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\d]).{6,}$/
        ),
      ]),
      repassword: new FormControl('', [Validators.required]),
    },
    {
      validators: [this.confirmPassword],
    }
  );
  email: string = '';
  token: string = '';

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _UserService: UserService,
    private _ToastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.queryParamMap
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.email = res.get('email') as string;
          this.token = res.get('token') as string;
        },
        error: (err) => {
          this._ToastrService.error(
            err.error.message || 'something went wrong try again later'
          );
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
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
      this._UserService
        .ResetPassword({
          email: this.email,
          token: this.token,
          newPassword: this.registerForm.get('password')?.value,
          confirmPassword: this.registerForm.get('repassword')?.value,
        })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            this._ToastrService.success(
              res.message || 'password is reseted successfully'
            );
          },
          error: (err) => {
            this._ToastrService.error(
              err.error.message || 'an error has occured'
            );
          },
        });
    } else {
      this.registerForm.markAllAsTouched();
    }
  }
}
