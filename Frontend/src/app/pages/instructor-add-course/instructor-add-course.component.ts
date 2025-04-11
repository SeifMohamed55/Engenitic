import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormGroup,
  FormControl,
  FormArray,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { CoursesService } from '../../feature/courses/courses.service';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-instructor-add-course',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './instructor-add-course.component.html',
  styleUrls: ['./instructor-add-course.component.scss'],
})
export class InstructorAddCourseComponent implements OnInit, OnDestroy {
  selectedFile: File | null = null;
  fileValidationError: string | null = null;
  currentLevelIndex = 0;
  currentLevelIndexQuiz = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private cd: ChangeDetectorRef,
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this.addLevelCourse(); // Initialize with one level
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  addingCourseForm = new FormGroup({
    title: new FormControl('', [Validators.required]),
    description: new FormControl('', [Validators.required]),
    requirements: new FormControl('', [Validators.required]),
    levels: new FormArray([]),
  });

  // Level management methods
  addLevelCourse(): void {
    const level = new FormGroup({
      videoUrl: new FormControl('', [Validators.required]),
      levelTitle : new FormControl('', [Validators.required]),
      levelDescription : new FormControl('', [Validators.required]),
      quizzes: new FormArray([
        this.createQuiz(), // Start with one quiz per level
      ]),
    });
    (this.addingCourseForm.get('levels') as FormArray).push(level);
    this.currentLevelIndex = this.levels.length - 1;
    this.currentLevelIndexQuiz = this.quizzes.length - 1;
    this.cd.detectChanges();
  }

  deleteLevelCourse(): void {
    const levelArray = this.levels;
    if (levelArray.length > 0) {
      levelArray.removeAt(this.currentLevelIndex);
      this.currentLevelIndex = Math.min(
        this.currentLevelIndex,
        levelArray.length - 1
      );
      this.cd.detectChanges();
    }
    this.currentLevelIndexQuiz = this.quizzes.length - 1;
  }

  previousLevelCourse(): void {
    if (this.currentLevelIndex > 0) this.currentLevelIndex--;
    this.currentLevelIndexQuiz = this.quizzes.length - 1;
  }

  nextLevelCourse(): void {
    if (this.currentLevelIndex < this.levels.length - 1)
      this.currentLevelIndex++;
    this.currentLevelIndexQuiz = this.quizzes.length - 1;
  }

  // Quiz management methods
  createQuiz(): FormGroup {
    return new FormGroup({
      question: new FormControl('', [Validators.required]),
      answer_1: new FormGroup({
        answer: new FormControl('', [Validators.required]),
        isCorrect: new FormControl(false),
      }),
      answer_2: new FormGroup({
        answer: new FormControl('', [Validators.required]),
        isCorrect: new FormControl(false),
      }),
      answer_3: new FormGroup({
        answer: new FormControl('', [Validators.required]),
        isCorrect: new FormControl(false),
      }),
      answer_4: new FormGroup({
        answer: new FormControl('', [Validators.required]),
        isCorrect: new FormControl(false),
      }),
    });
  }

  addLevelQuiz(): void {
    const quizzes = this.currentLevel.get('quizzes') as FormArray;
    quizzes.push(this.createQuiz());
    this.currentLevelIndexQuiz = quizzes.length - 1;
    this.cd.detectChanges();
  }

  deleteLevelQuiz(): void {
    const quizzes = this.quizzes;
    if (quizzes.length > 0) {
      quizzes.removeAt(this.currentLevelIndexQuiz);
      this.currentLevelIndexQuiz = Math.min(
        this.currentLevelIndexQuiz,
        quizzes.length - 1
      );
      this.cd.detectChanges();
    }
  }

  previousLevelQuiz(): void {
    if (this.currentLevelIndexQuiz > 0) this.currentLevelIndexQuiz--;
  }

  nextLevelQuiz(): void {
    if (this.currentLevelIndexQuiz < this.quizzes.length - 1)
      this.currentLevelIndexQuiz++;
  }

  // Form helpers
  get levels(): FormArray {
    return this.addingCourseForm.get('levels') as FormArray;
  }

  get currentLevel(): FormGroup {
    return this.levels.at(this.currentLevelIndex) as FormGroup;
  }

  get quizzes(): FormArray {
    return this.currentLevel.get('quizzes') as FormArray;
  }

  get currentQuiz(): FormGroup {
    return this.quizzes.at(this.currentLevelIndexQuiz) as FormGroup;
  }

  answers(answer: string): FormGroup {
    return this.currentQuiz.get(answer) as FormGroup;
  }

  // Correct answer handling
  setCorrectAnswer(answerKey: string): void {
    ['answer_1', 'answer_2', 'answer_3', 'answer_4'].forEach((key) => {
      const control = this.answers(key).get('isCorrect');
      control?.setValue(key === answerKey);
    });
    this.validateQuiz(this.currentQuiz);
  }

  validateQuiz(quiz: FormGroup): void {
    const hasCorrect = ['answer_1', 'answer_2', 'answer_3', 'answer_4'].some(
      (key) => quiz.get(key)?.get('isCorrect')?.value
    );
    quiz.setErrors(hasCorrect ? null : { noCorrectAnswer: true });
  }

  // File handling
  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;

    if (input.files?.length) {
      const file = input.files[0];
      const validTypes = ['image/jpg', 'image/png', 'image/jpeg'];
      const maxSize = 2 * 1024 * 1024;

      if (!validTypes.includes(file.type)) {
        this.fileValidationError = 'Invalid image type (only JPG/PNG allowed)';
        return;
      }

      if (file.size > maxSize) {
        this.fileValidationError = 'Image size must be less than 2MB';
        return;
      }
      this.selectedFile = file;
    }
  }

  // Form submission
  onSubmit(): void {
    const formValue = this.addingCourseForm.value;

    this.levels.controls.forEach((level) => {
      const quizzes = (level as FormGroup).get('quizzes') as FormArray;
      quizzes.controls.forEach((quiz) => this.validateQuiz(quiz as FormGroup));
    });

    if (!this.selectedFile) {
      this.fileValidationError = 'Course image is required';
      return;
    }

    if (this.addingCourseForm.valid) {
      console.log('Form submitted:', this.addingCourseForm.value);

      const submitCourse = new FormData();
      if (this.addingCourseForm.get('title')?.value) {
        submitCourse.append(
          'title',
          `${this.addingCourseForm.get('title')?.value}`
        );
      }

      if (this.addingCourseForm.get('description')?.value) {
        submitCourse.append(
          'description',
          `${this.addingCourseForm.get('description')?.value}`
        );
      }

      if (this.addingCourseForm.get('requirements')?.value) {
        submitCourse.append(
          'requirements',
          `${this.addingCourseForm.get('requirements')?.value}`
        );
      }

      if (this.selectedFile) {
        submitCourse.append('image', this.selectedFile);
      }

      if (localStorage.getItem('id')) {
        submitCourse.append(
          'instructorId',
          localStorage.getItem('id') as string
        );
      }

      const quizzesData = formValue?.levels?.map(
        (level: any, levelIndex: number) => ({
          title: level.levelTitle,
          position: levelIndex + 1,
          videoUrl: level.videoUrl,
          description : level.levelDescribtion,
          questions: level.quizzes.map((quiz: any, quizIndex: number) => ({
            questionText: quiz.question,
            position: quizIndex + 1,
            answers: ['answer_1', 'answer_2', 'answer_3', 'answer_4'].map(
              (key, idx) => ({
                answerText: quiz[key].answer,
                position: idx + 1,
                isCorrect: quiz[key].isCorrect,
              })
            ),
          })),
        })
      );

      submitCourse.append('Quizes', JSON.stringify(quizzesData));

      this._CoursesService
        .addCourse(submitCourse)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            this._ToastrService.success(res.message);
          },
          error: (err) => {
            if(err.error.message) {
              this._ToastrService.error(err.error.message);
            }
            else {
              console.log(err);
              this._ToastrService.error("something went wrong try again later");
            }
          },
        });
    } else {
      this.addingCourseForm.markAllAsTouched();
      console.warn('Form is invalid', this.addingCourseForm.value);
    }
  }
}
