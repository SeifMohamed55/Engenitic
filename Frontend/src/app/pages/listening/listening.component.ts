import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ModelsService } from '../../feature/models/models.service';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-listening',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './listening.component.html',
  styleUrl: './listening.component.scss',
})
export class ListeningComponent implements OnDestroy {
  constructor(
    private _ModelsService: ModelsService,
    private _ToastrService: ToastrService
  ) {}

  listeningForm: FormGroup = new FormGroup({
    text: new FormControl('', [Validators.required]),
  });

  audioUrl: string | null = null;
  destroy$ = new Subject<void>();
  responsive!: File | null;
  isDisabled: boolean = false;

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.audioUrl) {
      URL.revokeObjectURL(this.audioUrl);
    }
  }

  handleSubmit(): void {
    this.isDisabled = true;
    if (this.listeningForm.valid) {
      this._ModelsService
        .textToSpeech(this.listeningForm.value)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            this.audioUrl = URL.createObjectURL(res);
          },
          error: (err) => {
            err.error
              ? this._ToastrService.error(err.error.message)
              : this._ToastrService.error(
                  'an error has occured try again later'
                );
          },
        });
    } else {
      this.listeningForm.markAllAsTouched();
    }
    this.isDisabled = false;
  }
}
