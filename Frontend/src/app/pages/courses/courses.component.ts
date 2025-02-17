import { CommonModule } from '@angular/common';
import {  Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-courses',
  imports: [CommonModule,RouterModule],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.scss'
})
export class CoursesComponent {

}