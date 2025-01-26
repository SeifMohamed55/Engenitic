import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-course-details',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './course-details.component.html',
  styleUrl: './course-details.component.scss'
})
export class CourseDetailsComponent implements OnInit {

  constructor(private _ActivatedRoute:ActivatedRoute){

  };

  id !: string | null;  

  ngOnInit(): void {
    this.id = this._ActivatedRoute.snapshot.paramMap.get('id');
  }

  enrollForm : FormGroup = new FormGroup({});

  handleSubmit() : void{
    console.log(this.id);
  }
}
