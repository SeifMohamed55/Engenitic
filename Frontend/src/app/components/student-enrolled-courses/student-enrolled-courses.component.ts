import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { UserEnrolledCourses } from '../../interfaces/courses/user-enrolled-courses';
import { pipe, Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-student-enrolled-courses',
  imports: [RouterModule, CommonModule, NgxPaginationModule],
  providers: [],
  templateUrl: './student-enrolled-courses.component.html',
  styleUrl: './student-enrolled-courses.component.scss',
})
export class StudentEnrolledCoursesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  isInView: boolean = false;
  userId!: number;
  courseDone: boolean = false;
  userCourses: UserEnrolledCourses = {
    totalPages: 0,
    totalItems: 0,
    currentlyViewing: '',
    paginatedList: [],
  };
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;

  constructor(
    private _ActivatedRouter: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this._ActivatedRouter.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
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

    this._CoursesService
      .getEnrolledCourses(this.currentPage, this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.userCourses = res.data ?? {
            totalPages: 0,
            totalItems: 0,
            currentlyViewing: '',
            paginatedList: [],
          };
          this.totalItems = this.userCourses.totalItems;
        },
        error: (err) => {
          console.log(err);
          this.userCourses = {
            totalPages: 0,
            totalItems: 0,
            currentlyViewing: '',
            paginatedList: [],
          };
        },
      });
  }

  trackByCourse(index: number, course: any): number {
    return course.course.id;
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this._Router
      .navigate(['/profile/student', this.userId, page])
      .then(() => this.fetchCollection());
  }
}
