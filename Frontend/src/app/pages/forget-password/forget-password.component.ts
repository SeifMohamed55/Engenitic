import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { catchError, of, Subject, takeUntil, tap } from 'rxjs';
import { UserService } from '../../feature/users/user.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-forget-password',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './forget-password.component.html',
  styleUrl: './forget-password.component.scss',
})
export class ForgetPasswordComponent {
  private destroy$ = new Subject<void>();
  buttonDisabled: boolean = false;
  ForgetPassForm: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.email, Validators.required]),
  });

  constructor(
    private _UserService: UserService,
    private _ToastrService: ToastrService
  ) {}

  handleForgetPass(): void {
    this.buttonDisabled = true;
    if (this.ForgetPassForm.valid) {
      this._UserService
        .forgetPassword(this.ForgetPassForm.value)
        .pipe(
          takeUntil(this.destroy$),
          tap((res) =>
            this._ToastrService.success(
              res.message || 'a message has been sent to your email'
            )
          ),
          catchError((err) => {
            this._ToastrService.error(
              err.error.message || 'something went wrong try again'
            );
            return of(null);
          })
        )
        .subscribe();
    } else {
      this.ForgetPassForm.markAllAsTouched();
    }
    this.buttonDisabled = false;
  }
}
