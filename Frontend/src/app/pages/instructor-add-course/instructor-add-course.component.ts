import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import {
  FormGroup,
  FormControl,
  FormArray,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
} from '@angular/forms';

@Component({
  selector: 'app-instructor-add-course',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './instructor-add-course.component.html',
  styleUrls: ['./instructor-add-course.component.scss'],
})
export class InstructorAddCourseComponent implements OnInit {
  courseForm!: FormGroup;
  selectedFile: File | null = null;
  fileValidationError: string | null = null;
  currentLevelIndex = 0;

  constructor(private cdRef: ChangeDetectorRef) {}

  ngOnInit() {
    this.initializeForm();
    this.addLevel(); // Add first level by default
  }

  initializeForm() {
    this.courseForm = new FormGroup({
      title: new FormControl('', [Validators.required]),
      description: new FormControl('', [Validators.required]),
      requirements: new FormControl('', [Validators.required]),
      imageUrl: new FormControl(null, [Validators.required]),
      levels: new FormArray([]),
    });
  }

  get levels(): FormArray {
    return this.courseForm.get('levels') as FormArray;
  }

  addLevel() {
    const levelGroup = new FormGroup({
      videoUrl: new FormControl('', [Validators.required]),
      quiz: new FormControl('', [Validators.required]),
      answers: new FormArray([
        this.createAnswer(),
        this.createAnswer(),
        this.createAnswer(),
        this.createAnswer(),
      ]),
    });

    this.levels.push(levelGroup);
    this.currentLevelIndex = this.levels.length - 1;
    this.cdRef.detectChanges();
  }

  createAnswer(): FormGroup {
    return new FormGroup({
      answer: new FormControl('', Validators.required),
      isCorrect: new FormControl(false),
    });
  }

  getAnswers(levelIndex: number): FormArray {
    return this.levels.at(levelIndex).get('answers') as FormArray;
  }

  selectCorrectAnswer(levelIndex: number, answerIndex: number) {
    const answers = this.getAnswers(levelIndex);
    answers.controls.forEach((control, i) => {
      control.get('isCorrect')?.setValue(i === answerIndex);
    });
  }

  deleteLevel() {
    if (this.levels.length > 1) {
      this.levels.removeAt(this.currentLevelIndex);
      this.currentLevelIndex = Math.max(0, this.currentLevelIndex - 1);
      this.cdRef.detectChanges();
    }
  }

  previousLevel() {
    if (this.currentLevelIndex > 0) {
      this.currentLevelIndex--;
      this.cdRef.detectChanges();
    }
  }

  nextLevel() {
    if (this.currentLevelIndex < this.levels.length - 1) {
      this.currentLevelIndex++;
      this.cdRef.detectChanges();
    }
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;

    if (input.files?.length) {
      const file = input.files[0];
      const acceptableTypes = ['image/jpg', 'image/png', 'image/jpeg'];
      const maxSize = 2 * 1024 * 1024; // 2MB

      if (!acceptableTypes.includes(file.type)) {
        this.fileValidationError = 'Invalid image type';
        return;
      }

      if (file.size > maxSize) {
        this.fileValidationError = 'Image size must be less than 2MB';
        return;
      }

      this.selectedFile = file;
      this.courseForm.patchValue({ imageUrl: file });
    }
  }

  onSubmit() {
    if (this.courseForm.valid) {
      console.log('Course data :', this.courseForm.value);
    } else {
      this.courseForm.markAllAsTouched();
      console.log('Course error :', this.courseForm.value);
    }
  }
}
