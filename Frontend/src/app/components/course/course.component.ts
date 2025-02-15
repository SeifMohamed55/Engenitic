import { CoursesService } from './../../feature/courses/courses.service';
import { CommonModule } from '@angular/common';
import { Component, OnInit, Output } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Course } from '../../interfaces/courses/course';
import { ToastrService } from 'ngx-toastr';
import { EventEmitter } from 'stream';

@Component({
  selector: 'app-course',
  imports: [CommonModule, RouterModule],
  templateUrl: './course.component.html',
  styleUrl: './course.component.scss'
})
export class CourseComponent implements OnInit {
  
  dataToParent : number = 0; 

  collectionNumber !: number | null;

  courses !: Course[]

  constructor(private _ActivatedRoute : ActivatedRoute, private _CoursesService:CoursesService, private _ToastrService:ToastrService){

  }

  ngOnInit(): void {
    this.collectionNumber = Number.parseInt(this._ActivatedRoute.snapshot.paramMap.get('collection')!);
    console.log(this.collectionNumber);

    this._CoursesService.coursesOffered(this.collectionNumber).subscribe({
      next : res =>{
        console.log(res);
        this.dataToParent = (res.data.totalPages);
        this.courses = res.data.paginatedList;
      },
      error : err => {
        console.log(err);
        if (err.error.message){
          this._ToastrService.error(err.error.message)
        }
      }
    });
  }
}
