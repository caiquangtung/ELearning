import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
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
    FloatLabel,
    InputText,
    Password,
    Button,
    Message,
  ],
  template: `
    <main class="login-page">
      <section class="brand-panel" aria-label="ELearning">
        <div class="brand-mark">
          <span>EL</span>
        </div>
        <div>
          <p class="eyebrow">B2B + B2C LMS</p>
          <h1>ELearning</h1>
          <p class="brand-copy">Manage classes, courses, quizzes, orders, certificates, and learner activity from one workspace.</p>
        </div>
        <div class="brand-metrics" aria-label="Platform highlights">
          <div>
            <strong>Hybrid</strong>
            <span>Zoom + VOD</span>
          </div>
          <div>
            <strong>B2B</strong>
            <span>Organizations</span>
          </div>
          <div>
            <strong>B2C</strong>
            <span>Commerce</span>
          </div>
        </div>
      </section>

      <section class="form-panel" aria-label="Sign in form">
        <div class="form-shell">
          <div class="form-header">
            <p class="eyebrow">Welcome back</p>
            <h2>Sign in to your account</h2>
            <p>Use your workspace credentials to continue.</p>
          </div>

          <form [formGroup]="form" (ngSubmit)="submit()" class="login-form">
            <div class="field-group">
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
              @if (emailInvalid()) {
                <small>Enter a valid email address.</small>
              }
            </div>

            <div class="field-group">
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
              @if (passwordInvalid()) {
                <small>Password is required.</small>
              }
            </div>

            @if (localError()) {
              <p-message severity="error" [text]="localError()!" />
            }

            <p-button
              type="submit"
              label="Sign in"
              icon="pi pi-sign-in"
              [disabled]="form.invalid || pending()"
              [loading]="pending()"
              styleClass="w-full login-submit"
            />
          </form>

          <p class="signup-copy">
            No account?
            <a routerLink="/register">Register</a>
          </p>
        </div>
      </section>
    </main>
  `,
  styleUrl: './login.component.scss',
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

  emailInvalid(): boolean {
    const field = this.form.controls.email;
    return field.invalid && (field.dirty || field.touched);
  }

  passwordInvalid(): boolean {
    const field = this.form.controls.password;
    return field.invalid && (field.dirty || field.touched);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
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
