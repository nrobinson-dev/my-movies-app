import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadMoreButton } from './load-more-button';

describe('LoadMoreButton', () => {
  let component: LoadMoreButton;
  let fixture: ComponentFixture<LoadMoreButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadMoreButton],
    }).compileComponents();

    fixture = TestBed.createComponent(LoadMoreButton);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
