import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-student-enrolled-courses',
  imports: [RouterModule, CommonModule],
  templateUrl: './student-enrolled-courses.component.html',
  styleUrl: './student-enrolled-courses.component.scss'
})
export class StudentEnrolledCoursesComponent implements OnInit{

  collectionId !: number | null;
  courseDone : boolean = false;

  constructor(private _ActivatedRouter:ActivatedRoute){

  }

  ngOnInit(): void {
    this.collectionId = Number.parseInt(this._ActivatedRouter.snapshot.paramMap.get('collection')!);
    console.log(this.collectionId);
  }
}
