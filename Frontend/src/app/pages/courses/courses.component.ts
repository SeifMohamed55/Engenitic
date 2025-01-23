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

  end : number = this.paginagingNum;

  start : number = 0;

  links : number[] = Array.from({length : this.end - this.start }, (_, i) => i + this.start + 1);
  

  diff : number = 0;

  leftArrow() : void{
    while(this.start !== 0){
      this.handleLinks(0);
    }
  };

  rightArrow() : void{
    while(this.end !== this.totalNumCourses){
      this.handleLinks(this.links.length - 1);
    }
  };

  handleLinks(idx : number) : void{

    console.log(idx + 1)

    //if it is the last index and the end is more than the total and the start is lower than total as well 
    if( idx + 1 === this.paginagingNum  &&  this.end + this.paginagingNum > this.totalNumCourses && this.start + this.paginagingNum < this.totalNumCourses){
      this.start += this.paginagingNum;
      this.diff = this.end + this.paginagingNum - this.totalNumCourses;
      this.end = this.totalNumCourses;
      this.links.splice(0);
      this.links = Array.from({length : this.end - this.start}, (_, i) => i + this.start + 1);
      console.log(this.links);
    }

    else if (idx === 0  && this.start + this.paginagingNum >= this.totalNumCourses){
      console.log(this.diff);
      this.end += this.diff - this.paginagingNum;
      this.start -= this.paginagingNum;
      this.links.splice(0);
      this.links = Array.from({length : this.paginagingNum}, (_, i) => i + this.start + 1);
      console.log(this.links);
    }

    // if it is the last index but the start is bigger than the total
    else if( this.end - (idx + 1 + this.start) === 0  && this.start + this.paginagingNum >= this.totalNumCourses) {
      console.log("this case is fired");
      return ;
    }

    else if(idx + this.start  === this.start && this.start !== 0) {
      this.end -= this.paginagingNum;
      this.start -= this.paginagingNum;
      this.links.splice(0);
      this.links = Array.from({length : this.paginagingNum}, (_, i) => i + this.start + 1);
      console.log(this.links);
    } 

    else if(idx + this.start + 1 === this.end){
      this.end += this.paginagingNum;
      this.start += this.paginagingNum;
      this.links.splice(0);
      this.links = Array.from({length : this.paginagingNum}, (_, i) => i + this.start + 1);
      console.log(this.links);
    }
    console.log(`the start is :  ${this.start} , and the end is : ${this.end}, and the paginaging number is : ${this.paginagingNum}`);
  }
}
