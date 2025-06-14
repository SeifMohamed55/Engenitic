import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CoursesService } from '../../feature/courses/courses.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './courses.component.html',
  styleUrls: ['./courses.component.scss'],
})
export class CoursesComponent implements OnDestroy {
  private destroy$ = new Subject<void>();
  searchForm = new FormGroup({
    title: new FormControl('', [Validators.required]),
  });

  constructor(
    private _CoursesService: CoursesService,
    private _ToastrService: ToastrService
  ) {}

  onSearchCourses(): void {
    const searchTerm = this.searchForm.get('title')?.value?.trim() || '';
    if (!searchTerm) {
      this._CoursesService.clearSearchResults();
      return;
    }

    this._CoursesService
      .searchForCourseCollection(searchTerm)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this._CoursesService.updateSearchResults(res);
        },
        error: (err) => {
          this._ToastrService.error(
            err.error.message || 'something went wrong , try again later'
          );
          this._CoursesService.clearSearchResults();
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
