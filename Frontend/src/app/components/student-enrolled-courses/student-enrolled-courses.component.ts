import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute } from '@angular/router';
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
export class StudentEnrolledCoursesComponent implements OnInit, OnDestroy{

  private destroy$ = new Subject<void>();
  collectionId !: number | null;
  courseDone : boolean = false;
  userCourses !: UserEnrolledCourses;
  collectionNumber: number = 0; // Collection number for pagination
  currentPage: number = 1; // Current page number
  itemsPerPage: number = 6; // Items per page
  totalPages: number = 0; // Total number of pages
  totalItems: number = 0; // Total number of items (courses)

  constructor(
    private _ActivatedRouter:ActivatedRoute,
    private _CoursesService:CoursesService,
  ){}

  ngOnInit(): void {
    this._ActivatedRouter.paramMap.subscribe(params =>{
      
      this.collectionId = params.get('collectionId') ? Number.parseInt(params.get('collectionId')!) : 0;

      this.fetchCollection(this.collectionId);
    });
    console.log(this.collectionId);
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchCollection(collectionId : number) : void {
    
    this._CoursesService.getEnrolledCourses(this.currentPage, collectionId).pipe(takeUntil(this.destroy$)).subscribe({
      next : res =>{
        console.log(res);
        this.userCourses = res.data;
        this.totalItems = this.userCourses.totalItems
      },
      error : err =>{
        console.log(err);
      }
    })
  };



}
