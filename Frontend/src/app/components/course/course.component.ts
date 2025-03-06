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

  collectionNumber: number = 0; // Collection number for pagination
  currentPage: number = 1; // Current page number
  itemsPerPage: number = 6; // Items per page
  totalPages: number = 0; // Total number of pages
  totalItems: number = 0; // Total number of items (courses)
  courses!: Course[]; // Array of courses

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private _Router: Router
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.paramMap.subscribe(params => {
      // Get the collection number from the route
      this.collectionNumber = params.get('collectionNumber') ? Number(params.get('collectionNumber')) : 0;
      
      // Fetch courses based on the collection number
      this.fetchCourses(this.collectionNumber);
    });
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Fetch courses from the service
  fetchCourses(collectionNumber: number): void {
    this._CoursesService.coursesOffered(collectionNumber).pipe(takeUntil(this.destroy$)).subscribe({
      next: (res) => {
        console.log(res);
        this.courses = res.data.paginatedList;
        this.totalPages = res.data.totalPages;
        this.totalItems = res.data.totalItems;
      },
      error: (err) => {
        console.error(err);
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

