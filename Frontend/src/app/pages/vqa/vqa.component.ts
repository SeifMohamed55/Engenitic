import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-vqa',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './vqa.component.html',
  styleUrl: './vqa.component.scss'
})
export class VqaComponent {



  selectedFile: File | null = null; // Store the file externally
  fileValidationError: string | null = null;


  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;


    const acceptableTypes: string[] = ['image/jpg', 'image/png', 'image/jpeg'];
    const maxSize = 2 * 1024 * 1024; // 2 MB

    // Validate the file type and size
    if (!acceptableTypes.includes(this.selectedFile.type)) {
      this.selectedFile = null;
      this.fileValidationError = "invalid image type";
      return ;
    }

    if (this.selectedFile.size > maxSize) {
      this.selectedFile = null;
      return ;
    }


    const image = new Image();
    const url = URL.createObjectURL(file);

    image.onload = () => {
      this.fileValidationError = null; // Valid image
      URL.revokeObjectURL(url); // Free memory
    };

    image.onerror = () => {
      this.fileValidationError = 'Invalid file. The file is not a valid image.';
      this.selectedFile = null; // Reset selected file
      URL.revokeObjectURL(url); // Free memory
    };

    image.src = url; // Validate the file as an image
    }
    else {
      this.selectedFile = null; // Reset selected file
    }
}
}
