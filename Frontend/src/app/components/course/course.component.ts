import { CoursesService } from './../../feature/courses/courses.service';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Course } from '../../interfaces/courses/course';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-course',
  imports: [CommonModule, RouterModule],
  templateUrl: './course.component.html',
  styleUrl: './course.component.scss'
})
export class CourseComponent implements OnInit {
  
  totalNumCourses !: number;
  
  paginagingNum : number = 10;
  
  idxx : number = 0;
  
  collectionNumber !: number | null;
  
  courses !: Course[]

  links: number[] = [];
  
  constructor(private _ActivatedRoute : ActivatedRoute, private _CoursesService:CoursesService, private _ToastrService:ToastrService){
    
  }
  
  ngOnInit(): void {
    
    this._ActivatedRoute.paramMap.subscribe(params =>{
      if(params.get('collectionNumber')){
        this.collectionNumber =  Number.parseInt(params.get('collectionNumber')!); 
      }
      else {
        this.collectionNumber = 0
      }
      this._CoursesService.coursesOffered(this.collectionNumber).subscribe({
        next : res=>{
          console.log(res);
          this.courses = res.data.paginatedList;
          this.totalNumCourses = res.data.paginatedList.length;
        },
        error : err =>{
          console.log(err);
          if(err.error.message){
            this._ToastrService.error(err.error.message);
          }
        }
      })
    });
  }
  

  handleLinks(value : number, idx : number) : void {

    this.idxx = value;
    
    const calc = Math.floor(this.paginagingNum / 2);

    let temp : number = 0;

    if (value - calc <= 0){
      temp =  - (value-1);
    }
    else if (this.totalNumCourses - calc < value){
      temp =  - (this.paginagingNum - (this.totalNumCourses - value ) - 1);
    }
    else {
      temp = 0 - calc;
    }

    this.links  = Array.from({length : Math.min(this.paginagingNum, this.totalNumCourses) }, (_, i) => {
      return  this.idxx + i + temp; 
    });

  }

  leftArrow() : void{
    this.handleLinks(1, 0);
  };

  rightArrow() : void{
    this.handleLinks(this.totalNumCourses, this.paginagingNum - 1);
  };

}
