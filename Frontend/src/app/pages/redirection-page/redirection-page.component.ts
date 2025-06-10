import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  interval,
  Subscription,
  tap,
  switchMap,
  takeUntil,
  Subject,
  catchError,
  throwError,
  pipe,
  finalize,
} from 'rxjs';
import { CoursesService } from '../../feature/courses/courses.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-redirection-page',
  imports: [],
  templateUrl: './redirection-page.component.html',
  styleUrl: './redirection-page.component.scss',
})
export class RedirectionPageComponent implements OnInit, OnDestroy {
  counter: number = 5;
  subscription!: Subscription;
  userId!: number;
  newEmail!: string;
  token!: string;
  destroy$ = new Subject<void>();

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.queryParamMap
      .pipe(
        takeUntil(this.destroy$),
        switchMap((params) => {
          const userId = Number(params.get('userId'));
          const newEmail = String(params.get('newEmail'));
          const token = String(params.get('token'));
          return this._CoursesService
            .confirmEmailChange(userId, newEmail, token)
            .pipe(
              catchError((error) => {
                console.error('Email confirmation failed:', error);
                return throwError(() => error);
              })
            );
        })
      )
      .subscribe({
        next: (response) => {
          console.log(response);
          this._ToastrService.success('Email Changed Successfully !');
          this._Router.navigate(['/home']);
        },
        error: (error) => {
          console.error('Failed to change email', error);
        },
      });

    this.subscription = interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.counter === 0) {
          return;
        } else {
          this.counter -= 1;
        }
      });
  }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
