import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    Card,
    FloatLabel,
    InputText,
    Password,
    Button,
    Message,
  ],
  template: `
    <div class="auth-wrapper">
      <p-card header="Sign in">
        <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-column gap-3">
          <p-floatlabel>
            <input
              pInputText
              id="login-email"
              type="email"
              formControlName="email"
              class="w-full"
              autocomplete="username"
              fluid
            />
            <label for="login-email">Email</label>
          </p-floatlabel>
          <p-floatlabel>
            <p-password
              inputId="login-password"
              formControlName="password"
              [feedback]="false"
              [toggleMask]="true"
              styleClass="w-full"
              inputStyleClass="w-full"
              [fluid]="true"
              autocomplete="current-password"
            />
            <label for="login-password">Password</label>
          </p-floatlabel>
          @if (localError()) {
            <p-message severity="error" [text]="localError()!" />
          }
          <p-button type="submit" label="Sign in" icon="pi pi-sign-in" [disabled]="form.invalid || pending()" [loading]="pending()" styleClass="w-full" />
        </form>
        <p class="text-sm text-color-secondary mt-3 mb-0">
          No account?
          <a routerLink="/register" class="text-primary font-medium">Register</a>
        </p>
      </p-card>
    </div>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  readonly pending = signal(false);
  readonly localError = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) return;
    this.localError.set(null);
    this.errors.clear();
    this.pending.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => {
        this.pending.set(false);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
        void this.router.navigateByUrl(returnUrl);
      },
      error: () => {
        this.pending.set(false);
        this.localError.set('Invalid email or password.');
      },
    });
  }
}
