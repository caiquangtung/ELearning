import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly active = signal(0);

  readonly isLoading = this.active.asReadonly();

  start(): void {
    this.active.update((n) => n + 1);
  }

  stop(): void {
    this.active.update((n) => (n > 0 ? n - 1 : 0));
  }
}

