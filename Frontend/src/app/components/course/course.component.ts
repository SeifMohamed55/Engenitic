import { CoursesService } from './../../feature/courses/courses.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Course } from '../../interfaces/courses/course';
import { ToastrService } from 'ngx-toastr';
import {NgxPaginationModule} from 'ngx-pagination';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-course',
  imports: [CommonModule, RouterModule,NgxPaginationModule],
  templateUrl: './course.component.html',
  styleUrl: './course.component.scss'
})
export class CourseComponent implements OnInit , OnDestroy{
  
  private destroy$ = new Subject<void>();

  currentPage: number = 1; // Current page number
  itemsPerPage: number = 10; // Items per page
  totalItems: number = 0; // Total number of items (courses)
  courses : Course[] = []; // Array of courses

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      // Get the collection number from the route
      this.currentPage = params.get('collectionNumber') ? Number(params.get('collectionNumber')) : 0;
      
      // Fetch courses based on the collection number
      this.fetchCourses(this.currentPage);
    });
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Fetch courses from the service
  fetchCourses(currentPage: number): void {
    this._CoursesService.coursesOffered(currentPage).subscribe({
      next: (res) => {
        console.log(res);
        this.courses = res.data.paginatedList;
        this.totalItems = res.data.totalItems;
      },
      error: (err) => {
        if (err.error.message) {
          this._ToastrService.error(err.error.message);
        }
        else {
          this._ToastrService.error("an error has occured please try again later !");
          this._Router.navigate(['/home']);
        }
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this._Router.navigate(['/offered-courses', page]);
    this.fetchCourses(this.currentPage);
  }
}

