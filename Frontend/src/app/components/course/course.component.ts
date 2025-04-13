import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { Course } from '../../interfaces/courses/course';
import { ToastrService } from 'ngx-toastr';
import { NgxPaginationModule } from 'ngx-pagination';
import { combineLatest, Subject, takeUntil, distinctUntilChanged } from 'rxjs';

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
  private initialSearchNavigation = true;

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSubscriptions(): void {
    combineLatest([
      this._ActivatedRoute.paramMap,
      this._CoursesService.isSearchActive,
    ])
      .pipe(
        takeUntil(this.destroy$),
        distinctUntilChanged((prev, curr) => {
          // Compare page numbers and search state
          return (
            prev[0].get('collectionNumber') ===
              curr[0].get('collectionNumber') && prev[1] === curr[1]
          );
        })
      )
      .subscribe(([params, isSearchActive]) => {
        const page = Number(params.get('collectionNumber')) || 1;
        this.currentPage = page;
        this.isSearchActivated = isSearchActive;

        if (isSearchActive) {
          this.handleSearchNavigation(page);
        } else {
          this.handleNormalNavigation(page);
        }
      });

    this._CoursesService.currentSearchResults
      .pipe(takeUntil(this.destroy$))
      .subscribe((results) => {
        if (results) {
          this.courses = results.data.paginatedList;
          this.totalItems = results.data.totalItems;
        }
      });
  }

  private handleSearchNavigation(page: number): void {
    if (this.initialSearchNavigation && page !== 1) {
      this.initialSearchNavigation = false;
      this.navigateToPage(1);
      return;
    }

    this.initialSearchNavigation = false;
    this.loadSearchData(page);
  }

  private handleNormalNavigation(page: number): void {
    this.initialSearchNavigation = true;
    if (!this.isSearchActivated) {
      this.fetchCourses(page);
    }
  }

  private loadSearchData(page: number): void {
    this.isLoading = true;
    this._CoursesService
      .searchForCourseCollection(this._CoursesService.currentSearchQuery, page)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this._CoursesService.updateSearchResults(res);
          this.isLoading = false;
        },
        error: (err) => {
          this.handleError(err);
          this.isLoading = false;
        },
      });
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
    if (this.currentPage !== page) {
      this._Router.navigate(['/offered-courses', page], {
        replaceUrl: true, // Critical fix for history stack
      });
    }
  }

  clearSearch(): void {
    this._CoursesService.clearSearchResults();
    this.initialSearchNavigation = true;
    if (this.currentPage !== 1) {
      this.navigateToPage(1);
    } else {
      this.fetchCourses(1);
    }
  }

  private handleError(err: any): void {
    this.isLoading = false;
    console.error('Error:', err);
    this._ToastrService.error(err.error?.message || 'An error occurred');
    this._Router.navigate(['/home']);
  }
}
