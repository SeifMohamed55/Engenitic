import { CommonModule } from '@angular/common';
import { CoursesService } from './../../feature/courses/courses.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject, takeUntil } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-main-course',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './main-course.component.html',
  styleUrl: './main-course.component.scss',
})
export class MainCourseComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  studentId!: number;
  enrollmentId!: number;
  mainCourseResponse: any;
  displayQuiz : boolean = false;

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService
  ) {}

  handleArrow(event: Event): void {
    const element = event.currentTarget as HTMLDivElement;

    const arrowElement = element.querySelector('.myArrow') as HTMLElement;

    if (arrowElement) {
      arrowElement.classList.toggle('rotate-90');
    }

    const paragraph = element.nextSibling as HTMLParagraphElement;
    if (paragraph) {
      const isOpen = paragraph.classList.contains('max-h-0');
      paragraph.classList.toggle('max-h-0', !isOpen);
      paragraph.classList.toggle('opacity-0', !isOpen);
      paragraph.classList.toggle('max-h-[500px]', isOpen); // adjust as needed
      paragraph.classList.toggle('opacity-100', isOpen);
      paragraph.classList.toggle('py-5', isOpen);
    }
  }

  ngOnInit(): void {
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.studentId = Number(params.get('studentId')) ?? 0;
        this.enrollmentId = Number(params.get('enrollmentId')) ?? 0;
      });

    this._CoursesService
      .getCurrentStage(this.studentId, this.enrollmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
        },
        error: (err) => {
          console.log(err);
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handlePrevious(): void {}

  handleNext(): void {
    document.body.style.overflow = 'hidden';
    this.displayQuiz = true;
  }
  handleClosingQuiz() : void {
    document.body.style.overflow = 'auto';
    this.displayQuiz = false;
  }
}
