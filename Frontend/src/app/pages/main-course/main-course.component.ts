import { CoursesService } from './../../feature/courses/courses.service';
import { Component, OnDestroy, OnInit,  } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject, takeUntil } from 'rxjs';


@Component({
  selector: 'app-main-course',
  imports: [],
  templateUrl: './main-course.component.html',
  styleUrl: './main-course.component.scss',
})
export class MainCourseComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  studentId!: number;
  enrollmentId!: number;
  mainCourseResponse: any;



  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.studentId = Number(params.get('studentId')) ?? 0;
        this.enrollmentId = Number(params.get('enrollmentId')) ?? 0;
      });

    this._CoursesService
      .getCurrentStage(this.studentId, this.enrollmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
        },
        error: (err) => {
          console.log(err);
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handlePrevious(): void {}

  handleNext(): void {}
}
