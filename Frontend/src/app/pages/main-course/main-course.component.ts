import { CommonModule } from '@angular/common';
import { CoursesService } from './../../feature/courses/courses.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MainCourse } from '../../interfaces/courses/main-course';
import { Levels } from '../../interfaces/courses/levels';
import { ToastrService } from 'ngx-toastr';

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
  courseId!: number;
  mainCourseResponse: MainCourse = {} as MainCourse;
  levelsTitles: Levels[] = [] as Levels[];
  displayQuiz: boolean = false;
  quizForm: FormArray = new FormArray<any>([]);

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.studentId = Number(params.get('studentId')) ?? 0;
        this.enrollmentId = Number(params.get('enrollmentId')) ?? 0;
        this.courseId = Number(params.get('courseId')) ?? 0;
      });

    this._CoursesService
      .getCurrentStage(this.studentId, this.enrollmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.mainCourseResponse = res.data;
        },
        error: (err) => {
          if (err.error) {
            this._ToastrService.error(err.error.message);
          } else {
            this._ToastrService.error(
              'an error happened retrieving the course try again later'
            );
          }
        },
      });

    this._CoursesService
      .getQuizTitles(this.courseId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.levelsTitles = res.data;
        },
        error: (err) => {
          if (err.error) {
            this._ToastrService.error(err.error.message);
          } else {
            this._ToastrService.error(
              'an error happened retrieving the levels details try again later'
            );
          }
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  creatingAnswers(questions: any[]): void {
    for (let i = 0; i < questions.length; i++) {
      this.quizForm.push(
        new FormGroup({
          question_id: new FormControl('', [Validators.required]),
          answer_1: new FormGroup({
            isCorrect: new FormControl(false, [Validators.required]),
            answer_id: new FormControl('', [Validators.required]),
          }),
          answer_2: new FormGroup({
            isCorrect: new FormControl(false, [Validators.required]),
            answer_id: new FormControl('', [Validators.required]),
          }),
          answer_3: new FormGroup({
            isCorrect: new FormControl(false, [Validators.required]),
            answer_id: new FormControl('', [Validators.required]),
          }),
          answer_4: new FormGroup({
            isCorrect: new FormControl(false, [Validators.required]),
            answer_id: new FormControl('', [Validators.required]),
          }),
        })
      );

      this.quizForm.at(i).get('question_id')?.patchValue('asdsad');
      this.quizForm.at(i).get('answer1')?.get('answer_id')?.patchValue(1);
      this.quizForm.at(i).get('answer2')?.get('answer_id')?.patchValue(2);
      this.quizForm.at(i).get('answer3')?.get('answer_id')?.patchValue(3);
      this.quizForm.at(i).get('answer4')?.get('answer_id')?.patchValue(4);
    }
  }

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

  handlePrevious(): void {
    console.log(this.mainCourseResponse.position - 1);
    this._CoursesService
      .getEnrollmentStage(
        this.studentId,
        this.enrollmentId,
        this.mainCourseResponse.position - 1
      )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
        },
        error: (err) => {
          console.warn(err);
        },
      });
  }

  handleNext(): void {
    document.body.style.overflow = 'hidden';
    this.displayQuiz = true;
  }

  handleClosingQuiz(): void {
    document.body.style.overflow = 'auto';
    this.displayQuiz = false;
  }
}
