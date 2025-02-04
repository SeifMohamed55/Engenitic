import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InstructorAddCourseComponent } from './instructor-add-course.component';

describe('InstructorAddCourseComponent', () => {
  let component: InstructorAddCourseComponent;
  let fixture: ComponentFixture<InstructorAddCourseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InstructorAddCourseComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InstructorAddCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
