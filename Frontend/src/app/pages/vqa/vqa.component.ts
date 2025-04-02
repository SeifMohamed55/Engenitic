import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ModelsService } from '../../feature/models/models.service';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-vqa',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './vqa.component.html',
  styleUrl: './vqa.component.scss',
})
export class VqaComponent implements OnDestroy {
  constructor(
    private _ModelsService: ModelsService,
    private _ToastrService:ToastrService
  ){};

  private destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  vqaForm: FormGroup = new FormGroup({
    image: new FormControl('', [Validators.required]),
    question: new FormControl('', [Validators.required]),
  });

  isDisabled: boolean = false;
  response: string = '';
  selectedFile: File | null = null; // Store the file externally
  fileValidationError: string | null = null;

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;

      const acceptableTypes: string[] = [
        'image/jpg',
        'image/png',
        'image/jpeg',
      ];
      const maxSize = 2 * 1024 * 1024;

      if (!acceptableTypes.includes(this.selectedFile.type)) {
        this.selectedFile = null;
        this.fileValidationError = 'invalid image type';
        return;
      }

      if (this.selectedFile.size > maxSize) {
        this.fileValidationError = 'image size must be less than 2MB';
        this.selectedFile = null;
        return;
      }
    } else {
      this.selectedFile = null;
    }
  }

  handleSubmit(): void {
    this.isDisabled = true;
    if (this.vqaForm.valid) {
      const formData = new FormData();
      if (this.selectedFile) {
        formData.append('image', this.selectedFile);
      }
      formData.append('question', this.vqaForm.get('question')?.value);
      this._ModelsService.VqaModel(formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next : res => {
          console.log(res);
          this.response = res.data.answer
        },
        error : err => {
          console.log(err);
          if(err.error.message) {
            this._ToastrService.error(err.error.message);
          }else {
            this._ToastrService.error("something wrong with the server try again later !");
          }
        }
      })
    } else {
      console.error(`an error has occured : ${this.vqaForm.value}`);
      this.vqaForm.markAllAsTouched();
    }
    this.isDisabled = false;
  }
}
