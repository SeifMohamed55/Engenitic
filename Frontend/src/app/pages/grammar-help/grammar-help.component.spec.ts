import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GrammarHelpComponent } from './grammar-help.component';

describe('GrammarHelpComponent', () => {
  let component: GrammarHelpComponent;
  let fixture: ComponentFixture<GrammarHelpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GrammarHelpComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GrammarHelpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
