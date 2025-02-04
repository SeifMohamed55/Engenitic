import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InstructorEditCourseComponent } from './instructor-edit-course.component';

describe('InstructorEditCourseComponent', () => {
  let component: InstructorEditCourseComponent;
  let fixture: ComponentFixture<InstructorEditCourseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InstructorEditCourseComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InstructorEditCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
