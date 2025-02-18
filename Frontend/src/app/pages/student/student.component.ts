import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';

@Component({
  selector: 'app-student',
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './student.component.html',
  styleUrl: './student.component.scss'
})
export class StudentComponent implements OnInit {

  constructor(
    private _UserService:UserService
  ){}

  userId !: number;
  disapleButton : boolean = false;
  userPic !: any;
  selectedFile: File | null = null;
  previewUrl : string | null = null;
  fileValidationError: string | null = null;

  ngOnInit(): void {

    this._UserService.userId.subscribe(data =>{
      this.userId = data;
    });

    this._UserService.getProfileData(this.userId).subscribe({
      next : res =>{
        console.log(res);
      },
      error : err => {
        console.log(err);
      }
    });
    
  }

  studentProfileForm : FormGroup = new FormGroup({
    image : new FormControl(''),
    email : new FormControl('', [Validators.email, Validators.required]),
    userName : new FormControl('', [Validators.required]),
    oldPassword : new FormControl('', [Validators.required, Validators.minLength(5)]),
    newPassword : new FormControl('', [Validators.required, Validators.minLength(5)])
  }, {
    validators : []
  })


  onFileChange(event: Event): void {

    const input = event.target as HTMLInputElement;

    this.fileValidationError = null;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;


    const acceptableTypes: string[] = ['image/jpg', 'image/png', 'image/jpeg'];
    const maxSize = 2 * 1024 * 1024;

    // Validate the file type and size
    if (!acceptableTypes.includes(this.selectedFile.type)) {
      this.selectedFile = null;
      this.fileValidationError = "invalid image type";
      return ;
    }

    if (this.selectedFile.size > maxSize) {
      this.selectedFile = null;
      this.fileValidationError = "image size must be less than 2MB";
      return ;
    }

    if(this.previewUrl){
      URL.revokeObjectURL(this.previewUrl);
    }

    this.previewUrl = URL.createObjectURL(this.selectedFile);
    }
    else {
      this.selectedFile = null; // Reset selected file
    }
  }


    handleSubmit () : void{
    this.disapleButton = true;
    if(this.studentProfileForm.valid){
      const formData = new FormData();
      formData.append('email', this.studentProfileForm.get('email')?.value);
      formData.append('userName', this.studentProfileForm.get('userName')?.value);
      formData.append('oldPassword', this.studentProfileForm.get('oldPassword')?.value);
      formData.append('newPassword', this.studentProfileForm.get('newPassword')?.value);
      if (this.selectedFile){
        formData.append('image', this.selectedFile);
      }
      formData.forEach(el => console.log(el));
    }
    else {
      this.studentProfileForm.markAllAsTouched();
    }
    this.disapleButton = false;
  }


}
