import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { CourseDetails } from '../../interfaces/courses/course-details';
import { UserService } from '../../feature/users/user.service';
import { ToastrService } from 'ngx-toastr';
import {
  catchError,
  combineLatest,
  of,
  startWith,
  Subject,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';
import { OwlOptions, CarouselModule } from 'ngx-owl-carousel-o';
import { Course } from '../../interfaces/courses/course';

@Component({
  selector: 'app-course-details',
  imports: [ReactiveFormsModule, CommonModule, CarouselModule, RouterModule],
  templateUrl: './course-details.component.html',
  styleUrl: './course-details.component.scss',
})
export class CourseDetailsComponent implements OnInit, OnDestroy {
  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _UserService: UserService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  private destroy$ = new Subject<void>();
  private refreshRandomCourses$ = new Subject<void>();
  courseDetailsResopnse: CourseDetails = {} as CourseDetails;
  randomCourses: Course[] = [];
  courseId!: number;
  userId!: number;
  isEnrolled: boolean = false;
  customOptions: OwlOptions = {
    lazyLoad: true,
    mouseDrag: true,
    touchDrag: true,
    pullDrag: true,
    dots: true,
    navSpeed: 1000,
    items: 1,
    center: true,
    margin: 150,
  };

  ngOnInit(): void {
    combineLatest([
      this._UserService.userId,
      this._ActivatedRoute.paramMap,
      // Use the refresh trigger to force refetch
      this.refreshRandomCourses$.pipe(
        startWith(void 0), // Initial fetch
        switchMap(() => this._CoursesService.GetRandomCourses())
      ),
    ])
      .pipe(
        takeUntil(this.destroy$),
        tap(([userID, params, courses]) => {
          this.userId = Number(userID);
          this.courseId = Number(params.get('courseId'));
          if (courses) {
            this.randomCourses = courses.data;
          }
        }),
        switchMap(([_, params]) => {
          const courseId = Number(params.get('courseId'));
          if (!courseId) return of(null);

          return this._CoursesService.getCourseDetails(courseId).pipe(
            tap((res) => {
              this.courseDetailsResopnse = res.data;
              this.isEnrolled = res.data.isEnrolled || false;
            }),
            catchError((err) => {
              this.handleError(err);
              return of(null);
            })
          );
        })
      )
      .subscribe();

    // Trigger refresh when route params change
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.refreshRandomCourses$.next();
      });
  }

  private handleError(err: any): void {
    console.error(err);
    const errorMessage =
      err.error?.message || 'An error occurred on the server, try again later';
    this._ToastrService.error(errorMessage);
    this._Router.navigate(['/offered-courses']);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handleSubmit(): void {
    console.log(this.userId);
    if (!this.userId) {
      this._ToastrService.warning('you must be user to enroll a course');
      this._Router.navigate(['/login']);
    }
    this._CoursesService
      .enrollCourseHandler(this.courseId, this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this._ToastrService.success(res.message);
          this._Router.navigate([
            '/main-course',
            this.userId,
            res?.data?.id,
            this.courseId,
          ]);
        },
        error: (err) => {
          if (err.error.message) {
            this._ToastrService.error(err.error.message);
          } else {
            this._ToastrService.error('an error has occured, try again later.');
          }
        },
      });
  }
}
