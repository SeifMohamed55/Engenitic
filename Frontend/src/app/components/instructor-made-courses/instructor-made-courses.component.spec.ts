import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InstructorMadeCoursesComponent } from './instructor-made-courses.component';

describe('InstructorMadeCoursesComponent', () => {
  let component: InstructorMadeCoursesComponent;
  let fixture: ComponentFixture<InstructorMadeCoursesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InstructorMadeCoursesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InstructorMadeCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
