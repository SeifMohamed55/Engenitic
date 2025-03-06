import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CoursesService } from '../../feature/courses/courses.service';
import { NgxPaginationModule, PaginatePipe, PaginationService } from 'ngx-pagination';
import { InstructorCourse } from '../../interfaces/courses/instructor-course';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-instructor-made-courses',
  imports: [NgxPaginationModule, CommonModule, RouterModule],
  templateUrl: './instructor-made-courses.component.html',
  styleUrl: './instructor-made-courses.component.scss'
})
export class InstructorMadeCoursesComponent implements OnInit , OnDestroy {
  
  private destroy$ = new Subject<void>();
  userId !: number;
  itemsPerPage : number = 6;
  currentPage : number = 1;
  totalItems !: number;
  instrucorCourses !: InstructorCourse[];

  constructor(
    private _ActivatedRoute:ActivatedRoute,
    private _CoursesService:CoursesService
  ){}


  onPageChange(page : number) : void{
    this.currentPage = page;
    this.fetchCollection(this.currentPage);
  }


  ngOnInit(): void {
    this._ActivatedRoute.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params =>{
      this.userId = parseInt(params.get('userId')!) ?? '';
      this.currentPage = parseInt(params.get('collectionId')!) ?? 0
      this.fetchCollection(this.currentPage);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  };

  fetchCollection(collectionNumber : number):void {
    this._CoursesService.getCreatedCourses(collectionNumber, this.userId).pipe(takeUntil(this.destroy$)).subscribe({
      next : res =>{
        this.totalItems = res.data.totalItems;
        this.instrucorCourses = res.data.paginatedList;
        console.log(res);
      },
      error : err =>{
        console.log(err);
      }
    })
  }
}
