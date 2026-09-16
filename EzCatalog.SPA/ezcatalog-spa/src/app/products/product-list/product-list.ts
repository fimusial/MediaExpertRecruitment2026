import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, Subject, takeUntil, timer } from 'rxjs';
import { ProductResponse, ProductsService } from '../../core/api/generated';

type Product = ProductResponse;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DecimalPipe, RouterLink],
  selector: 'ez-catalog-product-list',
  styleUrl: './product-list.css',
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit {
  private readonly api = inject(ProductsService);
  private readonly pageLimit = 25;
  private readonly skeletonDelayMs = 300;

  total = signal<string>('...');
  items = signal<Product[]>([]);
  nextCursor = signal<string | null>(null);
  loading = signal<boolean>(false);

  ngOnInit(): void {
    this.load();
  }

  protected load(cursor?: string): void {
    this.loading.set(false);

    const requestFinished$ = new Subject<void>();
    const timerSubscription = timer(this.skeletonDelayMs)
      .pipe(takeUntil(requestFinished$))
      .subscribe(() => this.loading.set(true));

    this.api
      .getProductsPage(cursor, this.pageLimit)
      .pipe(finalize(() => {
          requestFinished$.next();
          requestFinished$.complete();
          timerSubscription.unsubscribe();
          this.loading.set(false);
        }),
      ).subscribe((page) => {
        this.total.set(page.totalCount.toString());
        this.items.update((items) => [...items, ...page.products]);
        this.nextCursor.set(page.nextCursor);
      });
  }
}
