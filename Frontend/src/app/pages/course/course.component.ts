import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-course',
  imports: [CommonModule, RouterModule],
  templateUrl: './course.component.html',
  styleUrl: './course.component.scss'
})
export class CourseComponent implements OnInit {
  

  collectionId !: number | null;

  constructor(private _ActivatedRoute : ActivatedRoute){

  }

  ngOnInit(): void {
    this.collectionId = Number.parseInt(this._ActivatedRoute.snapshot.paramMap.get('collection')!);
    console.log(this.collectionId);
  }

  
}
