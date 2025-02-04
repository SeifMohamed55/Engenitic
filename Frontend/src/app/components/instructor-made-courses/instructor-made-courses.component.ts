import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-instructor-made-courses',
  imports: [],
  templateUrl: './instructor-made-courses.component.html',
  styleUrl: './instructor-made-courses.component.scss'
})
export class InstructorMadeCoursesComponent implements OnInit {
  
  collectionId !: number | null 

  constructor(private _ActivatedRoute:ActivatedRoute){

  }


  ngOnInit(): void {
    this.collectionId =  parseInt(this._ActivatedRoute.snapshot.paramMap.get('collection')!)
  }
  
}
