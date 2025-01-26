import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-grammar-help',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, NgxSpinnerModule],
  templateUrl: './grammar-help.component.html',
  styleUrls: ['./grammar-help.component.scss'],
  animations: [] // Include this if animations are used
})
export class GrammarHelpComponent {
  response!: string;
  disableButton = false;

  grammarForm: FormGroup = new FormGroup({
    sentence: new FormControl('', [Validators.required])
  });

  constructor(private _ngxSpinnerService: NgxSpinnerService) {}

  handleSubmit(): void {
    this.disableButton = true;
    this._ngxSpinnerService.show();

    if (this.grammarForm.valid) {
      console.log(this.grammarForm.value);
      this.disableButton = false;
      this._ngxSpinnerService.hide();
    } else {
      this.grammarForm.markAllAsTouched();
      this.disableButton = false;
      this._ngxSpinnerService.hide();
      console.error(`Error: ${this.grammarForm.value}`);
    }
  }
}
