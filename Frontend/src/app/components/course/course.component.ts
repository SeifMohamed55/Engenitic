import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { Course } from '../../interfaces/courses/course';
import { ToastrService } from 'ngx-toastr';
import { NgxPaginationModule } from 'ngx-pagination';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-course',
  standalone: true,
  imports: [CommonModule, RouterModule, NgxPaginationModule],
  templateUrl: './course.component.html',
  styleUrls: ['./course.component.scss'],
})
export class CourseComponent implements OnInit, OnDestroy {
  isLoading = false;
  private destroy$ = new Subject<void>();
  isSearchActivated = false;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  courses: Course[] = [];

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this.setupSubscriptions();
  }

  private setupSubscriptions(): void {
    // Handle search activation state
    this._CoursesService.isSearchActive
      .pipe(takeUntil(this.destroy$))
      .subscribe((isActive) => {
        this.isSearchActivated = isActive;
        if (isActive && this.currentPage !== 1) {
          this.navigateToPage(1);
        }
      });

    // Handle route parameters
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        const page = Number(params.get('collectionNumber')) || 1;
        this.currentPage = page;
        this.loadData();
      });

    // Handle search results
    this._CoursesService.currentSearchResults
      .pipe(takeUntil(this.destroy$))
      .subscribe((results) => {
        if (results) {
          this.handleSearchResults(results);
        } else {
          this.fetchCourses(this.currentPage);
        }
      });
  }

  private loadData(): void {
    if (this.isSearchActivated) {
      // If in search mode, trigger search for the current page
      this._CoursesService
        .searchForCourseCollection(
          this._CoursesService.currentSearchQuery,
          this.currentPage
        )
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => this._CoursesService.updateSearchResults(res),
          error: (err) => this.handleError(err),
        });
    } else {
      // Normal course fetch
      this.fetchCourses(this.currentPage);
    }
  }

  private handleSearchResults(results: any): void {
    this.courses = results.data.paginatedList;
    this.totalItems = results.totalItems;
  }

  private fetchCourses(page: number): void {
    this.isLoading = true;
    this._CoursesService
      .coursesOffered(page)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.courses = res.data.paginatedList;
          this.totalItems = res.data.totalItems;
          this.isLoading = false;
        },
        error: (err) => {
          this.handleError(err);
          this.isLoading = false;
        },
      });
  }

  onPageChange(page: number): void {
    this.navigateToPage(page);
  }

  private navigateToPage(page: number): void {
    this._Router.navigate(['/offered-courses', page]);
  }

  clearSearch(): void {
    this._CoursesService.clearSearchResults();
    this.navigateToPage(1);
  }

  private handleError(err: any): void {
    console.error('Error:', err);
    this._ToastrService.error(err.error?.message || 'An error occurred');
    this._Router.navigate(['/home']);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}