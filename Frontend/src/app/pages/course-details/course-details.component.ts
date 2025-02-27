import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { CoursesService } from '../../feature/courses/courses.service';
import { CourseDetails } from '../../interfaces/courses/course-details';
import { UserService } from '../../feature/users/user.service';
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-course-details',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './course-details.component.html',
  styleUrl: './course-details.component.scss'
})
export class CourseDetailsComponent implements OnInit, OnDestroy {

  constructor
  (
    private _ActivatedRoute:ActivatedRoute, 
    private _CoursesService:CoursesService, 
    private _UserService:UserService,
    private _ToastrService:ToastrService,
    private _Router:Router
  ){};

  private destroy$ = new Subject<void>();
  courseDetailsResopnse !: CourseDetails
  id !: number | null;  

  ngOnInit(): void {

    this.id = Number.parseInt(this._ActivatedRoute.snapshot.paramMap.get('id')!);

    if(this.id){
      this._CoursesService.getCourseDetails(this.id).pipe(takeUntil(this.destroy$)).subscribe({
        next : res =>{
          console.log(res);
          this.courseDetailsResopnse = res.data;
        },
        error : err =>{
          console.log(err);
          if (err.error.message){
            this._ToastrService.error(err.error.message);
            this._Router.navigate(['/offered-courses']);
          }
        }
      })
    }
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  enrollForm : FormGroup = new FormGroup({});

  handleSubmit() : void{
    console.log(this.id , this._UserService.userId.value);
  }
}
