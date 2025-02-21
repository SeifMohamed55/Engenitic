import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

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

  constructor(
    private _ToastrService:ToastrService
  ) {}

  handleSubmit(): void {
    this.buttonDisabled = true;
    if (this.grammarForm.valid) {
      this.response = this.grammarForm.get('sentence')?.value;
      console.log(this.grammarForm.value);
    } 
    else {
      this.grammarForm.markAllAsTouched();
      this._ToastrService.error("an error has occured");
    }
    this.buttonDisabled = false;
  };
}
