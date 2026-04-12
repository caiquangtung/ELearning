import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class GlobalErrorService {
  readonly message = signal<string | null>(null);

  set(msg: string | null): void {
    this.message.set(msg);
  }

  clear(): void {
    this.message.set(null);
  }
}
