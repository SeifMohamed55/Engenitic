import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { UserEnrolledCourses } from '../../interfaces/courses/user-enrolled-courses';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-student-enrolled-courses',
  imports: [RouterModule, CommonModule, NgxPaginationModule],
  templateUrl: './student-enrolled-courses.component.html',
  styleUrl: './student-enrolled-courses.component.scss'
})
export class StudentEnrolledCoursesComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();
  userId!: number;
  courseDone: boolean = false;
  userCourses!: UserEnrolledCourses;
  currentPage: number = 1;
  itemsPerPage: number = 6;
  totalItems: number = 0;

  constructor(
    private _ActivatedRouter: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this._ActivatedRouter.paramMap.subscribe(params => {
      const userIdParam = params.get('userId');
      const pageParam = params.get('collectionId');

      this.userId = userIdParam ? Number.parseInt(userIdParam) : 0;
      this.currentPage = pageParam ? Number.parseInt(pageParam) : 1;

      this.fetchCollection();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchCollection(): void {
    if (!this.userId) return;

    this._CoursesService.getEnrolledCourses(this.currentPage, this.userId).pipe(takeUntil(this.destroy$)).subscribe({
      next: res => {
        this.userCourses = res.data;
        this.totalItems = res.data.totalItems;
      },
      error: err => {
        console.error(err);
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this._Router.navigate(['/profile/student', this.userId, page]);
    this.fetchCollection();
  }
}

