import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { UserManagementComponent } from '../../components/user-management/user-management.component';
import { RegisterAdminComponent } from "../../components/register-admin/register-admin.component";

@Component({
  selector: 'app-admin',
  imports: [CommonModule, UserManagementComponent, RegisterAdminComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss',
})
export class AdminComponent {
  activeTab: 'users' | 'cvs' | 'registerAdmin' = 'users';
  currentDate = new Date();
}
