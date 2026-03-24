import { Component, output } from '@angular/core';

@Component({
  selector: 'load-more-button',
  imports: [],
  standalone: true,
  template: `
    <div class="load-more-container">
      <button (click)="loadMore.emit()" class="btn-load-more">Load More</button>
    </div>
  `,
  styleUrl: './load-more-button.css',
})
export class LoadMoreButton {
  loadMore = output();
}