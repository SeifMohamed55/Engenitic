import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { CourseDetails } from '../../interfaces/courses/course-details';
import { UserService } from '../../feature/users/user.service';
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';
import { OwlOptions, CarouselModule } from 'ngx-owl-carousel-o';

@Component({
  selector: 'app-course-details',
  imports: [ReactiveFormsModule, CommonModule, CarouselModule],
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
  courseDetailsResopnse : CourseDetails = {} as CourseDetails;
  courseId!: number;
  userId!: number;
  customOptions: OwlOptions = {
    lazyLoad: true,
    mouseDrag: true,
    touchDrag: true,
    pullDrag: true,
    dots: true,
    navSpeed: 1000,
    items : 1,
    center : true,
    margin : 150,
  };

  ngOnInit(): void {
    this._UserService.userId
      .pipe(takeUntil(this.destroy$))
      .subscribe((userID) => {
        this.userId = Number(userID);
      });
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.courseId = Number(params.get('courseId'));

        if (this.courseId) {
          this._CoursesService.getCourseDetails(this.courseId).subscribe({
            next: (res) => {
              this.courseDetailsResopnse = res.data;
            },
            error: (err) => {
              console.log(err);
              if (err.error.message) {
                this._ToastrService.error(err.error.message);
                this._Router.navigate(['/offered-courses']);
              }
            },
          });
        }
      });
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
          this._Router.navigate(['/main-course', this.userId, res?.data?.id]);
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
