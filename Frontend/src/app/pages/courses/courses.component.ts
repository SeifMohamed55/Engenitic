import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-courses',
  imports: [CommonModule,RouterModule],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.scss'
})
export class CoursesComponent {

  totalNumCourses : number = 23;
  
  paginagingNum : number = 5;

  idxx : number = 0;

  links : number[] = Array.from({length : Math.min(this.paginagingNum, this.totalNumCourses - this.idxx + 1) }, (_, i) => this.idxx + i + 1);


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

    this.links  = Array.from({length : this.paginagingNum }, (_, i ) => {
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