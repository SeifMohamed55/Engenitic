import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormGroup,
  FormControl,
  FormArray,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { CoursesService } from '../../feature/courses/courses.service';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';

interface Quiz {
  id: number;
  title: string;
  position: number;
  videoUrl: string | null;
  description: string;
  questions: Question[];
}

interface Question {
  id: number;
  questionText: string;
  position: number;
  answers: Answer[];
}

interface Answer {
  id: number;
  answerText: string;
  position: number;
  isCorrect: boolean;
}

interface CourseEditing {
  id: number;
  code: string;
  title: string;
  description: string;
  requirements: string;
  instructorId: number;
  tags: string[];
  quizes: Quiz[];
}

@Component({
  selector: 'app-instructor-edit-course',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './instructor-edit-course.component.html',
  styleUrls: ['./instructor-edit-course.component.scss'],
})
export class InstructorEditCourseComponent implements OnInit, OnDestroy {
  courseForm!: FormGroup;
  currentLevelIndex = 0;
  currentQuestionIndex = 0;
  courseData?: CourseEditing;
  courseId!: number;
  instructorId!: number;
  private destroy$ = new Subject<void>();
  selectedFile: File | null = null;
  fileValidationError: string | null = null;

  constructor(
    private cd: ChangeDetectorRef,
    private coursesService: CoursesService,
    private toastr: ToastrService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        takeUntil(this.destroy$),
        switchMap((params) => {
          this.instructorId = Number(params.get('instructorId'));
          this.courseId = Number(params.get('courseId'));
          return this.coursesService.getCourseWithQuizzes(this.courseId);
        })
      )
      .subscribe({
        next: (res) => {
          this.courseData = res.data;
          this.initializeForm();
          this.cd.detectChanges();
        },
        error: (err) => this.handleError(err),
      });
  }

  private initializeForm(): void {
    this.courseForm = new FormGroup({
      title: new FormControl(this.courseData?.title || '', Validators.required),
      description: new FormControl(
        this.courseData?.description || '',
        Validators.required
      ),
      requirements: new FormControl(
        this.courseData?.requirements || '',
        Validators.required
      ),
      quizes: new FormArray(
        this.courseData?.quizes?.map((quiz) => this.createQuizGroup(quiz)) ||
          [],
        [this.validateQuizzes]
      ),
    });
  }

  // Quiz Management
  addQuiz(): void {
    this.quizes.push(
      this.createQuizGroup({
        id: 0,
        title: '',
        position: this.quizes.length + 1,
        description: '',
        videoUrl: null,
        questions: [this.createDefaultQuestion()], // Add initial question with answers
      })
    );
    this.currentLevelIndex = this.quizes.length - 1;
    this.currentQuestionIndex = this.questions.length - 1;
  }

  private createDefaultQuestion(): Question {
    return {
      id: 0,
      questionText: '',
      position: 1,
      answers: Array(4)
        .fill(null)
        .map((_, index) => ({
          id: 0,
          answerText: '',
          position: index + 1,
          isCorrect: false,
        })),
    };
  }

  private createQuizGroup(quiz?: Quiz): FormGroup {
    return new FormGroup({
      id: new FormControl(quiz?.id || 0),
      title: new FormControl(quiz?.title || '', Validators.required),
      position: new FormControl(
        quiz?.position || this.quizes.length + 1,
        Validators.required
      ),
      videoUrl: new FormControl(quiz?.videoUrl || null),
      description: new FormControl(quiz?.description || ''),
      questions: new FormArray(
        quiz?.questions?.map((question) =>
          this.createQuestionGroup(question)
        ) || []
      ),
    });
  }

  // Question Management
  addQuestion(): void {
    this.questions.push(this.createQuestionGroup());
    this.currentQuestionIndex = this.questions.length - 1;
  }

  private createQuestionGroup(question?: Question): FormGroup {
    return new FormGroup(
      {
        id: new FormControl(question?.id || 0),
        questionText: new FormControl(
          question?.questionText || '',
          Validators.required
        ),
        position: new FormControl(
          question?.position || this.questions.length + 1
        ),
        answers: new FormArray(
          question?.answers?.map((answer) => this.createAnswerGroup(answer)) ||
            this.createEmptyAnswers()
        ),
      },
      { validators: this.atLeastOneCorrectAnswerValidator }
    );
  }

  // Answer Management
  private createEmptyAnswers(): FormGroup[] {
    return Array(4)
      .fill(null)
      .map((_, index) =>
        this.createAnswerGroup({
          id: 0,
          answerText: '',
          position: index + 1,
          isCorrect: false,
        })
      );
  }

  private createAnswerGroup(answer?: Answer): FormGroup {
    return new FormGroup({
      id: new FormControl(answer?.id || 0),
      answerText: new FormControl(
        answer?.answerText || '',
        Validators.required
      ),
      position: new FormControl(answer?.position || 0),
      isCorrect: new FormControl(answer?.isCorrect || false),
    });
  }

  // Form Accessors
  get quizes(): FormArray {
    return this.courseForm.get('quizes') as FormArray;
  }

  get currentQuiz(): FormGroup {
    return this.quizes.at(this.currentLevelIndex) as FormGroup;
  }

  get questions(): FormArray {
    return this.currentQuiz.get('questions') as FormArray;
  }

  get currentQuestion(): FormGroup {
    return this.questions.at(this.currentQuestionIndex) as FormGroup;
  }

  get answers(): FormArray {
    return this.currentQuestion.get('answers') as FormArray;
  }

  // Navigation
  deleteQuiz(): void {
    if (this.quizes.length > 0) {
      this.quizes.removeAt(this.currentLevelIndex);
      this.quizes.controls.forEach((quiz, index) => {
        quiz.get('position')?.setValue(index + 1);
      });
      this.currentLevelIndex = Math.max(0, this.currentLevelIndex - 1);
    }
    this.currentQuestionIndex = this.questions.length - 1;
  }

  deleteQuestion(): void {
    if (this.questions.length > 0) {
      this.questions.removeAt(this.currentQuestionIndex);
      this.questions.controls.forEach((question, index) => {
        question.get('position')?.setValue(index + 1);
      });
    }
    this.currentQuestionIndex = Math.max(0, this.currentQuestionIndex - 1);
  }

  previousLevelCourse(): void {
    if (this.currentLevelIndex > 0) this.currentLevelIndex--;
    this.currentQuestionIndex = this.questions.length - 1;
  }

  nextLevelCourse(): void {
    if (this.currentLevelIndex < this.quizes.length - 1)
      this.currentLevelIndex++;
    this.currentQuestionIndex = this.questions.length - 1;
  }

  previousLevelQuiz(): void {
    if (this.currentQuestionIndex > 0) this.currentQuestionIndex--;
  }

  nextLevelQuiz(): void {
    if (this.currentQuestionIndex < this.questions.length - 1)
      this.currentQuestionIndex++;
  }

  setCorrectAnswer(questionIndex: number, answerIndex: number): void {
    const question = this.questions.at(questionIndex);
    if (!question) return;

    const answers = question.get('answers') as FormArray;

    answers.controls.forEach((answer, index) => {
      answer.get('isCorrect')?.setValue(index === answerIndex);
    });

    // Update validity state
    question.updateValueAndValidity();
  }

  atLeastOneCorrectAnswerValidator: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors | null => {
    const answers = control.get('answers') as FormArray;
    const hasCorrect = answers?.controls.some(
      (answer) => answer.get('isCorrect')?.value
    );
    return hasCorrect ? null : { noCorrectAnswer: true };
  };

  private validateQuizzes(control: AbstractControl): ValidationErrors | null {
    const quizes = control as FormArray;
    const invalidQuizes = quizes.controls.some((quiz) => {
      const questions = quiz.get('questions') as FormArray;
      return questions.controls.some((question) => question.invalid);
    });
    return invalidQuizes ? { invalidQuizzes: true } : null;
  }

  private markAllAsTouched(formGroup: FormGroup | FormArray): void {
    Object.values(formGroup.controls).forEach((control) => {
      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markAllAsTouched(control);
      } else {
        control.markAsTouched();
      }
    });
  }

  private handleError(error: any): void {
    this.toastr.error(error.message || 'An error occurred');
  }

  trackByIndex(index: number): number {
    return index;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Submission
  onSubmitEditCourse(): void {
    if (this.courseForm.invalid) {
      this.markAllAsTouched(this.courseForm);
      this.toastr.error('Please fill all required fields');
      return;
    }

    const formValue = structuredClone(this.courseForm.value);

    const courseData: CourseEditing = {
      ...formValue,
      id: this.courseId,
      instructorId: this.instructorId || 0,
      tags: this.courseData?.tags || [],
      code: this.courseData?.code || '',
    };

    console.log(courseData);

    this.coursesService
      .editCourse(courseData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.toastr.success(res.message);
        },
        error: (err) => {
          console.warn(err);
          err.error
            ? this.toastr.error(err.error.message)
            : this.toastr.error(err);
        },
      });
  }

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

  onSubmitEditImage(event: Event): void {
    event.preventDefault();
    const submitCourse: FormData = new FormData();
    if (this.selectedFile) {
      submitCourse.append('image', this.selectedFile);
      submitCourse.append('courseId', String(this.courseId));
      submitCourse.append('instructorId', String(this.instructorId));
      this.coursesService
        .editCourseImage(submitCourse)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            console.log(res);
            this.toastr.success(res.message);
          },
          error: (err) => {
            console.warn(err);
            err.error
              ? this.toastr.error(err.error.message)
              : this.toastr.error(err);
          },
        });
    } else {
      this.fileValidationError = 'image field is required';
    }
  }
}
