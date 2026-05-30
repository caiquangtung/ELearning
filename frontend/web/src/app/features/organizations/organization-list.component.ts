import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import { LmsApiService, OrganizationDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import {
  PageShellComponent,
  UiButtonComponent,
  UiDataTableBodyTemplateDirective,
  UiDataTableComponent,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui';

@Component({
  selector: 'app-organization-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    DialogModule,
    InputText,
    Tag,
    PrimeTemplate,
    Skeleton,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  styleUrl: './organization-list.component.scss',
  templateUrl: './organization-list.component.html',
})
export class OrganizationListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<OrganizationDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly skeletonRows = Array.from({ length: 6 });

  createVisible = false;
  newName = '';
  newSlug = '';
  readonly pageSize = 20;
  private pageNum = 1;

  isAdmin(): boolean {
    return this.auth.user()?.roles?.some((r) => r === 'Admin') ?? false;
  }

  ngOnInit(): void {
    this.reload();
  }

  openCreateDialog(): void {
    this.newName = '';
    this.newSlug = '';
    this.createVisible = true;
  }

  closeCreateDialog(): void {
    if (this.creating()) return;
    this.createVisible = false;
  }

  onPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    this.loading.set(true);
    this.api
      .listOrganizations({ page: this.pageNum, pageSize: this.pageSize })
      .subscribe({
        next: (page) => {
          this.page.set(page);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  createOrg(): void {
    const name = this.newName.trim();
    if (!name) return;
    this.errors.clear();
    this.creating.set(true);
    const slug = this.newSlug.trim() || null;
    this.api.createOrganization({ name, slug }).subscribe({
      next: () => {
        this.newName = '';
        this.newSlug = '';
        this.createVisible = false;
        this.creating.set(false);
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }
}
