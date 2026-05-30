import { computed, Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly activeRequests = signal(0);

  readonly activeCount = this.activeRequests.asReadonly();
  readonly isLoading = computed(() => this.activeRequests() > 0);

  start(): void {
    this.activeRequests.update((count) => count + 1);
  }

  stop(): void {
    this.activeRequests.update((count) => (count > 0 ? count - 1 : 0));
  }
}
