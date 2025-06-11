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

  constructor(private _UserService: UserService) {}
  handleForgetPass(): void {
    this.buttonDisabled = true;
    if (this.ForgetPassForm.valid) {
      this._UserService
        .forgetPassword(this.ForgetPassForm.value)
        .pipe(
          takeUntil(this.destroy$),
          tap((res) => console.log(res)),
          catchError((err) => {
            console.log(err);
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
