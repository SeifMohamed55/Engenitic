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
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

export interface QuizSubmit {
  questionId: number;
  answerId: number;
}

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
  safeUrl!: SafeResourceUrl;
  errorString: string = '';
  quizFormGroup = new FormGroup({
    questions: new FormArray<any>([]),
  });

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    // getting params
    this._ActivatedRoute.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.studentId = Number(params.get('studentId')) ?? 0;
        this.enrollmentId = Number(params.get('enrollmentId')) ?? 0;
        this.courseId = Number(params.get('courseId')) ?? 0;
      });

    // current stage fetch
    this._CoursesService
      .getCurrentStage(this.studentId, this.enrollmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.mainCourseResponse = res.data;
          this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
            this.mainCourseResponse.videoUrl
          );
          this.creatingAnswers(this.mainCourseResponse);
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

    // quiz titles fetch
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

  // view stage
  handleStageClick(position: number): void {
    console.log(position, this.mainCourseResponse.latestStage);
  }
  // getter for the questions array
  get questionsArray(): FormArray {
    return this.quizFormGroup.get('questions') as FormArray;
  }

  // adding ids to our answers
  creatingAnswers(MainResponse: MainCourse): void {
    this.questionsArray.clear();

    for (let i = 0; i < MainResponse.questions.length; i++) {
      const questionGroup = new FormGroup({
        question_id: new FormControl(MainResponse.questions[i].id, [
          Validators.required,
        ]),
        answer_1: new FormGroup({
          isCorrect: new FormControl(false, [Validators.required]),
          answer_id: new FormControl(MainResponse.questions[i].answers[0].id, [
            Validators.required,
          ]),
        }),
        answer_2: new FormGroup({
          isCorrect: new FormControl(false, [Validators.required]),
          answer_id: new FormControl(MainResponse.questions[i].answers[1].id, [
            Validators.required,
          ]),
        }),
        answer_3: new FormGroup({
          isCorrect: new FormControl(false, [Validators.required]),
          answer_id: new FormControl(MainResponse.questions[i].answers[2].id, [
            Validators.required,
          ]),
        }),
        answer_4: new FormGroup({
          isCorrect: new FormControl(false, [Validators.required]),
          answer_id: new FormControl(MainResponse.questions[i].answers[3].id, [
            Validators.required,
          ]),
        }),
      });
      this.questionsArray.push(questionGroup);
    }
  }
  // checking button handler
  handleChecking(questionIdx: number, answerNumber: number): void {
    const questionGroup = this.questionsArray.at(questionIdx) as FormGroup;

    // Reset all answers to false
    [1, 2, 3, 4].forEach((n) => {
      questionGroup.get(`answer_${n}`)?.get('isCorrect')?.patchValue(false);
    });

    // Set selected answer to true
    questionGroup
      .get(`answer_${answerNumber}`)
      ?.get('isCorrect')
      ?.patchValue(true);
  }

  // drop menu handler
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

  // handle previous button
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

  // handling next button
  handleNext(): void {
    document.body.style.overflow = 'hidden';
    this.displayQuiz = true;
  }

  // handling quiz closing
  handleClosingQuiz(): void {
    document.body.style.overflow = 'auto';
    this.displayQuiz = false;
  }

  // submit quiz handler
  handleSubmitQuiz(): void {
    const quizSubmition: QuizSubmit[] = [] as QuizSubmit[];
    for (let i = 0; i < this.questionsArray.length; i++) {
      const questionGroup = this.questionsArray.at(i) as FormGroup;
      // Reset all answers to false
      [1, 2, 3, 4].forEach((n) => {
        if (
          questionGroup.get(`answer_${n}`)?.get('isCorrect')?.value === true
        ) {
          quizSubmition.push({
            questionId: questionGroup.get('question_id')?.value,
            answerId: questionGroup.get(`answer_${n}`)?.get('answer_id')?.value,
          });
        }
      });
    }
    if (quizSubmition.length === this.mainCourseResponse.questions.length) {
      console.log(quizSubmition);
      this.errorString = '';
    } else {
      this.errorString =
        'you must complete the quiz in order to submit';
    }
  }
}
