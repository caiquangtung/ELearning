import { Component, inject } from '@angular/core';
import { Card } from 'primeng/card';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [Card],
  template: `
    <p-card header="Dashboard">
      <p class="mt-0">
        Signed in as <strong>{{ auth.user()?.fullName }}</strong>
        ({{ auth.user()?.email }}).
      </p>
      <p class="text-color-secondary">Use the menu to manage organizations, courses, and training classes.</p>
    </p-card>
  `,
})
export class DashboardComponent {
  readonly auth = inject(AuthService);
}
