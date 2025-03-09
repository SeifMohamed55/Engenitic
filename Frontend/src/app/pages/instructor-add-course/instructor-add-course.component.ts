import { Component } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface Level {
  videoUrl: string;
  quizUrl: string;
}

interface Course {
  title: string;
  id: string;
  description: string;
  requirements: string;
  imageUrl: string;
  levels: Level[];
}

@Component({
  selector: 'app-instructor-add-course',
  imports: [CommonModule],
  templateUrl: './instructor-add-course.component.html',
  styleUrl: './instructor-add-course.component.scss'
})
export class InstructorAddCourseComponent {
  course: Course = {
    title: '',
    id: '',
    description: '',
    requirements: '',
    imageUrl: '',
    levels: []
  };

  currentLevelIndex = 0;

  addLevel() {
    this.course.levels.push({
      videoUrl: '',
      quizUrl: ''
    });
    this.currentLevelIndex = this.course.levels.length - 1;
  }

  previousLevel() {
    if (this.currentLevelIndex > 0) {
      this.currentLevelIndex--;
    }
  }

  nextLevel() {
    if (this.currentLevelIndex < this.course.levels.length - 1) {
      this.currentLevelIndex++;
    }
  }

  onImageUrlChange() {
    // Image preview will be handled by the template
  }

  onSubmit() {
    // Here you would typically send the course data to your backend
    console.log('Course data:', this.course);
    alert('Course created successfully!');
  }
}
