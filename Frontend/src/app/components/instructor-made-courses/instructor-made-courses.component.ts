import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CoursesService } from '../../feature/courses/courses.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { InstructorCourse } from '../../interfaces/courses/instructor-course';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-instructor-made-courses',
  imports: [NgxPaginationModule, CommonModule, RouterModule],
  templateUrl: './instructor-made-courses.component.html',
  styleUrl: './instructor-made-courses.component.scss'
})
export class InstructorMadeCoursesComponent implements OnInit, OnDestroy {
  
  private destroy$ = new Subject<void>();
  userId: number = 0;
  itemsPerPage: number = 10;
  currentPage: number = 0;
  totalItems: number = 0;
  instructorCourses : InstructorCourse[] = [];

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService, 
    private _Router:Router
  ) {}

  onPageChange(page: number): void {
    this.currentPage = page;
    this._Router.navigate(['/profile/instructor', this.userId, this.currentPage]); 
    this.fetchCollection(this.currentPage);
  }

  ngOnInit(): void {
    this._ActivatedRoute.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.userId = Number(params.get('userId')) || 0;
      this.currentPage = Number(params.get('collectionId')) || 1;
      this.fetchCollection(this.currentPage); // Always fetch courses on init
    });
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchCollection(currentPage: number): void {
    this._CoursesService.getCreatedCourses(currentPage, this.userId).subscribe({
      next: (res) => {
        this.totalItems = res.data.totalItems;
        this.instructorCourses = res.data.paginatedList;
      },
      error: (err) => {
        this._ToastrService.error('Failed to fetch courses. Please try again.');
      }
    });
  }

  deleteCourse(courseId: number, instructorId: number): void {
    this._CoursesService.deleteCourse(courseId, instructorId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this._ToastrService.success(res.message);
          this.fetchCollection(1);
          this._Router.navigate(['/profile/instructor', this.userId, 1]); 
        },
        error: err => {
          this._ToastrService.error('Failed to delete course.');
        }
      });
  }
}
