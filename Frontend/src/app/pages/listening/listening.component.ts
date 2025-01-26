import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-listening',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './listening.component.html',
  styleUrl: './listening.component.scss'
})
export class ListeningComponent {
  listeningForm : FormGroup = new FormGroup ({
    sentence : new FormControl('', [Validators.required]),
  });

  isDisabled : boolean = false;

  handleSubmit() : void{
    this.isDisabled = true;
    if(this.listeningForm.valid){
      console.log(this.listeningForm.value);
      this.isDisabled = false;
    }
    else {
      this.listeningForm.markAllAsTouched();
      this.isDisabled = false;
    }
  }
}
