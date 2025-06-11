import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../../feature/users/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserData } from '../../interfaces/admin/admin-data';
import { catchError, of, Subject, takeUntil, tap } from 'rxjs';
import { ToastrService, ToastrModule } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxPaginationModule } from 'ngx-pagination';

@Component({
  selector: 'app-user-management',
  imports: [CommonModule, FormsModule, NgxPaginationModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss',
})
export class UserManagementComponent implements OnInit, OnDestroy {
  userId: number = 0;
  users: UserData[] = [];
  filteredUsers: UserData[] = [];
  searchTerm: string = '';
  selectedRole: string = '';
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;
  destroy$ = new Subject<void>();

  constructor(
    private adminService: UserService,
    private toaster: ToastrService,
    private _Router: Router,
    private _ActivatedRoute: ActivatedRoute
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    this.fetchCollection();
  }

  fetchCollection(): void {
    this.adminService
      .getAllUsers(this.currentPage)
      .pipe(
        takeUntil(this.destroy$),
        tap((res) => {
          this.users = res.data.paginatedList;
          this.totalItems = res.data.totalItems;
        }),
        tap(() => this.filterUsers()),
        tap((res) => console.log(res))
      )
      .subscribe();

    this._ActivatedRoute.paramMap
      .pipe(
        takeUntil(this.destroy$),
        tap((res) => {
          this.userId = parseInt(res.get('userId') as string);
          this.currentPage = parseInt(res.get('userCollectionId') as string);
        })
      )
      .subscribe();
  }

  handleBan(userId: number): void {
    this.adminService
      .banUser(userId)
      .pipe(
        takeUntil(this.destroy$),
        tap((res) => {
          // Update the user's banned status in the local array
          this.users = this.users.map((user) =>
            user.id === userId ? { ...user, banned: true } : user
          );
          this.toaster.success(res.message || 'User banned successfully');
          this.filterUsers();
        }),
        catchError((err) => {
          console.log(err);
          this.toaster.error('Something went wrong, try again');
          return of(null);
        })
      )
      .subscribe();
  }

  handleUnban(userId: number): void {
    this.adminService
      .unbanUser(userId)
      .pipe(
        takeUntil(this.destroy$),
        tap((res) => {
          // Update the user's banned status in the local array
          this.users = this.users.map((user) =>
            user.id === userId ? { ...user, banned: false } : user
          );
          this.toaster.success(res.message || 'User unbanned successfully');
          this.filterUsers();
        }),
        catchError((err) => {
          console.log(err);
          this.toaster.error('Something went wrong, try again');
          return of(null);
        })
      )
      .subscribe();
  }

  handleCv(fileLink: string): void {
    window.open(fileLink, 'PopupWindow', 'width=800,height=600');
  }

  filterUsers(): void {
    if (!this.selectedRole) {
      this.filteredUsers = this.users.filter((user) => {
        const matchesSearch =
          !this.searchTerm ||
          user.userName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
          user.email.toLowerCase().includes(this.searchTerm.toLowerCase());
        return matchesSearch;
      });
    } else {
      this.filteredUsers = this.users.filter((user) => {
        const matchesSearch =
          !this.searchTerm ||
          user.userName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
          user.email.toLowerCase().includes(this.searchTerm.toLowerCase());

        const matchesRole =
          !this.selectedRole || user.roles.includes(this.selectedRole);

        return matchesSearch && matchesRole;
      });
    }
  }

  trackByUserId(index: number, user: UserData): number {
    return user.id;
  }

  // for pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this._Router
      .navigate(['/profile/admin', this.userId, page])
      .then(() => this.fetchCollection());
  }
}
