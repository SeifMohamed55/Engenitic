import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-grammar-help',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './grammar-help.component.html',
  styleUrls: ['./grammar-help.component.scss'],
  animations: [] // Include this if animations are used
})
export class GrammarHelpComponent {
  response : string = '';
  buttonDisabled = false;

  grammarForm: FormGroup = new FormGroup({
    sentence: new FormControl('', [Validators.required])
  });

  constructor(private _ngxSpinnerService: NgxSpinnerService) {}

  handleSubmit(): void {
    this.buttonDisabled = true;

    if (this.grammarForm.valid) {
      console.log(this.grammarForm.value);
      this.buttonDisabled = false;
    } 
    else {
      this.grammarForm.markAllAsTouched();
      this.buttonDisabled = false;
      console.error(`Error: ${this.grammarForm.value}`);
    }
  }
}
