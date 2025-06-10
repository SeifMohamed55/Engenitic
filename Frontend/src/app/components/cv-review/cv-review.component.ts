import { Component } from '@angular/core';
import { UserService } from '../../feature/users/user.service';

@Component({
  selector: 'app-cv-review',
  imports: [],
  templateUrl: './cv-review.component.html',
  styleUrl: './cv-review.component.scss',
})
export class CvReviewComponent {
  // cvs: InstructorCV[] = [];
  // filteredCVs: InstructorCV[] = [];
  selectedStatus: string = '';

  constructor(private adminService: UserService) {
    
  }

  ngOnInit() {

  }

  // filterCVs() {
  //   this.filteredCVs = this.cvs.filter((cv) => {
  //     return !this.selectedStatus || cv.status === this.selectedStatus;
  //   });
  // }

  // reviewCV(cvId: string, status: 'approved' | 'rejected') {
  //   const action = status === 'approved' ? 'approve' : 'reject';
  //   if (confirm(`Are you sure you want to ${action} this CV?`)) {
  //     this.adminService.reviewCV(cvId, status).subscribe();
  //   }
  // }

  // trackByCvId(index: number, cv: InstructorCV): string {
  //   return cv.id;
  // }
}
