import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  LmsApiService,
  PublicFeaturedCourseDto,
} from '../../core/api/lms-api.service';
import { PagedList } from '../../core/models/paged-list.model';

@Component({
  selector: 'app-public-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './public-catalog.component.html',
  styleUrl: './public-catalog.component.scss',
})
export class PublicCatalogComponent {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly page = signal<PagedList<PublicFeaturedCourseDto> | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  search = '';
  category = '';
  level = '';
  price = '';
  sort = 'Newest';
  pageNum = 1;
  pageSize = 12;

  readonly categories = ['', 'AI', 'Data', 'Security', 'DevOps', 'Design', 'Business', 'Technology'];
  readonly levels = ['', 'Beginner', 'Intermediate', 'Advanced'];
  readonly sortOptions = [
    { label: 'Newest', value: 'Newest' },
    { label: 'Title A-Z', value: 'TitleAsc' },
    { label: 'Price low-high', value: 'PriceAsc' },
    { label: 'Price high-low', value: 'PriceDesc' },
  ];

  constructor() {
    this.route.queryParamMap.subscribe((query) => {
      this.search = query.get('search') ?? '';
      this.category = query.get('category') ?? '';
      this.level = query.get('level') ?? '';
      this.price = query.get('price') ?? '';
      this.sort = query.get('sort') ?? 'Newest';
      this.pageNum = Math.max(1, Number(query.get('page') ?? 1));
      this.load();
    });
  }

  applyFilters(): void {
    void this.router.navigate(['/catalog'], {
      queryParams: {
        search: this.search.trim() || null,
        category: this.category || null,
        level: this.level || null,
        price: this.price || null,
        sort: this.sort === 'Newest' ? null : this.sort,
        page: null,
      },
    });
  }

  clearFilters(): void {
    this.search = '';
    this.category = '';
    this.level = '';
    this.price = '';
    this.sort = 'Newest';
    void this.router.navigate(['/catalog']);
  }

  goToPage(page: number): void {
    const next = Math.max(1, page);
    void this.router.navigate(['/catalog'], {
      queryParams: { page: next === 1 ? null : next },
      queryParamsHandling: 'merge',
    });
  }

  priceLabel(course: PublicFeaturedCourseDto): string {
    if (course.priceCents <= 0) return 'Free';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: course.currency || 'USD',
    }).format(course.priceCents / 100);
  }

  durationLabel(minutes: number): string {
    const hours = Math.max(1, Math.round(minutes / 60));
    return `${hours}h guided`;
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    const priceBounds = this.priceBounds();

    this.api
      .listPublicCourses({
        page: this.pageNum,
        pageSize: this.pageSize,
        search: this.search,
        category: this.category,
        level: this.level,
        minPriceCents: priceBounds.min,
        maxPriceCents: priceBounds.max,
        sort: this.sort,
      })
      .subscribe({
        next: (page) => {
          this.page.set(page);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Course catalog is temporarily unavailable.');
          this.page.set(null);
          this.loading.set(false);
        },
      });
  }

  private priceBounds(): { min: number | null; max: number | null } {
    if (this.price === 'free') return { min: null, max: 0 };
    if (this.price === 'paid') return { min: 1, max: null };
    return { min: null, max: null };
  }
}
