import { ActivatedRoute } from '@angular/router';
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
} from 'rxjs';
import { CoursesService } from '../../feature/courses/courses.service';

@Component({
  selector: 'app-redirection-page',
  imports: [],
  templateUrl: './redirection-page.component.html',
  styleUrl: './redirection-page.component.scss',
})
export class RedirectionPageComponent implements OnInit, OnDestroy {
  counter: number = 10;
  subscription!: Subscription;
  userId!: number;
  newEmail!: string;
  token!: string;
  destroy$ = new Subject<void>();

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService
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
          console.log('Email changed successfully', response);
        },
        error: (error) => {
          console.error('Failed to change email', error);
        },
      });

    this.subscription = interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.counter -= 1;
        if (this.counter === 0) {
          return;
        }
      });
  }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
