import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { UserManagementComponent } from '../../components/user-management/user-management.component';
import { CvReviewComponent } from '../../components/cv-review/cv-review.component';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, UserManagementComponent, CvReviewComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss',
})
export class AdminComponent {
  activeTab: 'users' | 'cvs' = 'users';
  currentDate = new Date();
}
