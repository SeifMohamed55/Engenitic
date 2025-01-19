import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VqaComponent } from './vqa.component';

describe('VqaComponent', () => {
  let component: VqaComponent;
  let fixture: ComponentFixture<VqaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VqaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VqaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
