import { Component, OnDestroy, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../../feature/users/user.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-unothorized',
  standalone : true,
  imports: [],
  templateUrl: './unothorized.component.html',
  styleUrl: './unothorized.component.scss',
})
export class UnothorizedComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  constructor(
    private toastr: ToastrService,
    private userService: UserService
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    this.userService.redirectReason.next('unauthorized');
    this.userService.redirectReason
      .pipe(takeUntil(this.destroy$))
      .subscribe((reason) => {
        if (reason === 'unauthorized') {
          this.toastr.error(
            'This section is only available to users with the appropriate permissions.'
          );
          this.userService.redirectReason.next(null);
        }
      });
  }
}
