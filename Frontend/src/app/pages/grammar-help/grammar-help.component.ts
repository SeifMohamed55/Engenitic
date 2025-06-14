import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  ReactiveFormsModule,
  FormGroup,
  FormControl,
  Validators,
} from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ModelsService } from '../../feature/models/models.service';
import { Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-grammar-help',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './grammar-help.component.html',
  styleUrls: ['./grammar-help.component.scss'],
})
export class GrammarHelpComponent {
  response: string = '';
  score!: number;
  buttonDisabled = false;
  destroy$ = new Subject<void>();

  grammarForm: FormGroup = new FormGroup({
    sentence: new FormControl('', [Validators.required]),
  });

  constructor(
    private _ToastrService: ToastrService,
    private _ModelsService: ModelsService
  ) {}

  handleSubmit(): void {
    this.buttonDisabled = true;
    if (this.grammarForm.valid) {
      this._ModelsService
        .grammarCorrection(this.grammarForm.get('sentence')?.value)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            this.response = res.data.correctedText;
            this.score = Number(res.data.score);
          },
          error: (err) => {
            err.error
              ? this._ToastrService.warning(err.error.message)
              : this._ToastrService.error(
                  'an error has occured to the server try again later'
                );
          },
        });
    } else {
      this.grammarForm.markAllAsTouched();
    }
    this.buttonDisabled = false;
  }
}
