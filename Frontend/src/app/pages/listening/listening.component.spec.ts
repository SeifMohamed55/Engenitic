import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListeningComponent } from './listening.component';

describe('ListeningComponent', () => {
  let component: ListeningComponent;
  let fixture: ComponentFixture<ListeningComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListeningComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListeningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
