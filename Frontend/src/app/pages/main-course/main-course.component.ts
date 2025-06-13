import { CommonModule } from '@angular/common';
import { CoursesService } from './../../feature/courses/courses.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  catchError,
  forkJoin,
  map,
  of,
  Subject,
  switchMap,
  takeUntil,
  tap,
} from 'rxjs';
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
  errorString: string = '';
  quizFormGroup = new FormGroup({
    questions: new FormArray<any>([]),
  });
  reviewForm: FormGroup = new FormGroup({
    content: new FormControl('', [
      Validators.required,
      Validators.minLength(10),
    ]),
    rating: new FormControl(3, [
      Validators.required,
      Validators.min(1),
      Validators.max(5),
    ]),
  });
  hoveredRating = 0;
  selectedRating = 0;

  currentReview: FormGroup = new FormGroup({});

  constructor(
    private _ActivatedRoute: ActivatedRoute,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService
  ) {}

  ngOnInit(): void {
    // getting params
    this._ActivatedRoute.paramMap
      .pipe(
        takeUntil(this.destroy$),
        tap((params) => {
          this.studentId = Number(params.get('studentId')) ?? 0;
          this.enrollmentId = Number(params.get('enrollmentId')) ?? 0;
          this.courseId = Number(params.get('courseId')) ?? 0;
        }),
        switchMap(() => {
          return forkJoin({
            stage: this._CoursesService.getCurrentStage(
              this.studentId,
              this.enrollmentId
            ),
            quizTitles: this._CoursesService.getQuizTitles(this.courseId),
          });
        })
      )
      .subscribe({
        next: ({ stage, quizTitles }) => {
          console.log(stage);
          this.mainCourseResponse = stage.data;
          this.currentReview = new FormGroup({
            content: new FormControl(
              this.mainCourseResponse.reviewDTO?.content,
              [Validators.required, Validators.minLength(10)]
            ),
            rating: new FormControl(this.mainCourseResponse.reviewDTO?.rating, [
              Validators.required,
              Validators.min(1),
              Validators.max(5),
            ]),
          });
          this.creatingAnswers(this.mainCourseResponse);
          this.levelsTitles = quizTitles.data;
        },
        error: (err) => {
          const msg =
            err?.error?.message ??
            'an error happened retrieving the course data, try again later';
          this._ToastrService.error(msg);
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // view stage
  handleStageClick(position: number): void {
    this._CoursesService
      .getEnrollmentStage(this.studentId, this.enrollmentId, position)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.mainCourseResponse = res.data;
        },
        error: (err) => {
          err.error
            ? this._ToastrService.error(err.error.message)
            : this._ToastrService.error(
                'an error has occured try again later !'
              );
        },
      });
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
    this._CoursesService
      .getEnrollmentStage(
        this.studentId,
        this.enrollmentId,
        this.mainCourseResponse.position - 1
      )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.mainCourseResponse = res.data;
        },
        error: (err) => {
          err.error
            ? this._ToastrService.error(err.error.message)
            : this._ToastrService.error(
                'an error has occured try again later !'
              );
        },
      });
  }

  // handling next button
  handleNext(): void {
    if (
      this.mainCourseResponse.latestStage === this.mainCourseResponse.position
    ) {
      document.body.style.overflow = 'hidden';
      this.displayQuiz = true;
    } else {
      this._CoursesService
        .getEnrollmentStage(
          this.studentId,
          this.enrollmentId,
          this.mainCourseResponse.position + 1
        )
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            console.log(res);
            this.mainCourseResponse = res.data;
          },
          error: (err) => {
            err.error
              ? this._ToastrService.error(err.error.message)
              : this._ToastrService.error(
                  'an error has occured try again later !'
                );
          },
        });
    }
  }

  // handling quiz closing
  handleClosingQuiz(): void {
    document.body.style.overflow = 'auto';
    this.displayQuiz = false;
  }

  // submit quiz handler
  handleSubmitQuiz(): void {
    const quizSubmition: {
      userId: number;
      quizId: number;
      enrollmentId: number;
      userAnswers: QuizSubmit[];
    } = {
      userId: this.studentId,
      quizId: this.mainCourseResponse.id,
      enrollmentId: this.enrollmentId,
      userAnswers: [] as QuizSubmit[],
    };
    for (let i = 0; i < this.questionsArray.length; i++) {
      const questionGroup = this.questionsArray.at(i) as FormGroup;
      // Reset all answers to false
      [1, 2, 3, 4].forEach((n) => {
        if (
          questionGroup.get(`answer_${n}`)?.get('isCorrect')?.value === true
        ) {
          quizSubmition.userAnswers.push({
            questionId: questionGroup.get('question_id')?.value,
            answerId: questionGroup.get(`answer_${n}`)?.get('answer_id')?.value,
          });
        }
      });
    }
    if (
      quizSubmition.userAnswers.length ===
      this.mainCourseResponse.questions.length
    ) {
      this._CoursesService
        .submitQuiz(quizSubmition)
        .pipe(
          takeUntil(this.destroy$),
          switchMap((res) => {
            this.errorString = '';
            if (res.data.isPassed) {
              // if he passed the exam
              this._ToastrService.success(res.message);
              return this._CoursesService
                .getCurrentStage(this.studentId, this.enrollmentId)
                .pipe(
                  tap((res) => {
                    this.handleClosingQuiz();
                    this.mainCourseResponse = res.data;
                  }),
                  catchError((err) => {
                    if (err.error?.message) {
                      this._ToastrService.error(err.error.message);
                    } else {
                      this._ToastrService.error(
                        'An error occurred on the server, try again later'
                      );
                    }
                    this.displayQuiz = false;
                    return of(null);
                  })
                );
            } else {
              // if failed
              this._ToastrService.error(res.message);
              this.handleClosingQuiz();
              return of(null);
            }
          }),
          catchError((err) => {
            if (err.error?.message) {
              this._ToastrService.error(err.error.message);
            } else {
              this._ToastrService.error(
                'An error occurred on the server, try again later'
              );
            }
            return of(null);
          })
        )
        .subscribe();
    } else {
      this.errorString = 'you must complete the quiz in order to submit';
    }
  }

  setRating(rating: number): void {
    this.selectedRating = rating;
    this.reviewForm.patchValue({ rating });
  }

  onSubmit(): void {
    if (this.reviewForm.valid) {
      this._CoursesService
        .addReview({
          courseId: this.courseId,
          ...this.reviewForm.value,
        })
        .pipe(
          takeUntil(this.destroy$),
          tap((res) => this._ToastrService.success(res.message)),
          tap((res) => (this.mainCourseResponse.reviewDTO = res.data)),
          catchError((err) => {
            this._ToastrService.error(
              err.error.message || 'something went wrong try again'
            );
            return of(null);
          })
        )
        .subscribe();
    } else {
      this.reviewForm.markAllAsTouched();
    }
  }

  setEditRating(rating: number): void {
    this.selectedRating = rating;
    this.currentReview.patchValue({ rating });
  }

  updateReview(): void {
    if (this.currentReview.valid) {
      this._CoursesService
        .editReview({
          reviewId: this.mainCourseResponse.reviewDTO?.reviewId,
          ...this.currentReview.value,
        })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            console.log({
              reviewId: this.mainCourseResponse.reviewDTO?.reviewId,
              ...this.currentReview.value,
            });
            this._ToastrService.success(
              res.message || 'review is saved successfully'
            );
          },
          error: (err) => {
            this._ToastrService.error(
              err.error.message || 'an error has occured'
            );
          },
        });
    } else {
      this.currentReview.markAllAsTouched();
    }
  }

  deleteReview(reviewId: number): void {
    this._CoursesService
      .deleteReview(reviewId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this._ToastrService.success(
            res.message || 'review is deleted successfully'
          );
          this.mainCourseResponse.reviewDTO = null;
        },
        error: (err) => {
          this._ToastrService.error(
            err.error.message || 'an error has occured'
          );
        },
      });
  }
}
