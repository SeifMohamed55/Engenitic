import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';
import { UserData } from '../../interfaces/users/user-data';

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

  userData : UserData = {
    banned : false,
    email : '',
    id : 0,
    image : {
      imageURL : '',
      name : ''
    },
    phoneNumber : '',
    phoneRegionCode : '',
    userName : ''
  };
  disableButtonEmail : boolean = true;
  disableButtonUserName : boolean = true;
  disableButtonImage : boolean = true;
  disableButtonPassword : boolean = false;
  emailForm = new FormGroup({
    'email' : new FormControl(this.userData.email, [Validators.required, Validators.email])
  });
  userNameForm = new FormGroup({
    'userName' : new FormControl(this.userData.userName, [Validators.required])
  });
  passwordForm = new FormGroup({
    'oldPassword' : new FormControl('', [Validators.required]),
    'newPassword' : new FormControl('', [Validators.required])
  });
  selectedFile: File | null = null;
  previewUrl : string | null = null;
  fileValidationError: string | null = null;
  
  ngOnInit(): void {
    this._UserService.userId.subscribe(id => {
      if (id) {
        this.userData.id = id;
        this._UserService.getUserImage(this.userData.id).subscribe({
          next : image => {
            this.previewUrl = URL.createObjectURL(image);
            this.initiateForms();
          },
          error : err => {
            console.log(err);
          }
        })
      }
    });
  };

  initiateForms() : void {
    this._UserService.getProfileData(this.userData.id).subscribe({
      next : res => {
        console.log(res);
        this.userData = res.data;
        this._UserService.image.subscribe(image => {
          this.previewUrl = image;
        })
        this.emailForm.patchValue({ email: this.userData.email });
        this.userNameForm.patchValue({ userName: this.userData.userName });
        this.passwordForm.reset();
      },
      error : err => {
        console.log(err);
      }
    })
  };
  
  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileValidationError = null; 
    const acceptableTypes: string[] = ['image/jpg', 'image/png', 'image/jpeg'];
    const maxSize = 2 * 1024 * 1024;
  

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;

      if (file.name === this.userData.image.name && file.size === this.userData.image.imageURL.length) {
        this.fileValidationError = "The image you inserted is the same as the original one!";
        this.disableButtonImage = true;
        return;
      }
      if (!acceptableTypes.includes(this.selectedFile.type)) {
        this.selectedFile = null;
        this.fileValidationError = "Invalid image type. Only JPG, PNG, and JPEG are allowed.";
        this.disableButtonImage = true;
        return;
      }
      if (this.selectedFile.size > maxSize) {
        this.selectedFile = null;
        this.fileValidationError = "Image size must be less than 2MB.";
        this.disableButtonImage = true;
        return;
      }
      this.disableButtonImage = false;
      if (this.previewUrl) {
        URL.revokeObjectURL(this.previewUrl);
      }
      this.previewUrl = URL.createObjectURL(this.selectedFile);
    } else {
      this.selectedFile = null; // Reset selected file
    }
  };
  handleEmailSubmit () : void{
  };
  handleUserNameSubmit () : void{
  };
  handleImageSubmit () : void{
  };
  handlePasswordSubmit () : void{
  };
};
