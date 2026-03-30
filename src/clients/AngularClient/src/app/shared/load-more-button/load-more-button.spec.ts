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
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit loadMore event when button is clicked', () => {
    const spy = vi.fn();
    component.loadMore.subscribe(spy);

    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should render a button with "Load More" text', () => {
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.textContent.trim()).toBe('Load More');
  });
});
