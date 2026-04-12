import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-register',
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
      <p-card header="Create account">
        <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-column gap-3">
          <p-floatlabel>
            <input pInputText id="reg-fn" formControlName="firstName" class="w-full" fluid autocomplete="given-name" />
            <label for="reg-fn">First name</label>
          </p-floatlabel>
          <p-floatlabel>
            <input pInputText id="reg-ln" formControlName="lastName" class="w-full" fluid autocomplete="family-name" />
            <label for="reg-ln">Last name</label>
          </p-floatlabel>
          <p-floatlabel>
            <input pInputText id="reg-email" type="email" formControlName="email" class="w-full" fluid autocomplete="email" />
            <label for="reg-email">Email</label>
          </p-floatlabel>
          <p-floatlabel>
            <p-password
              inputId="reg-password"
              formControlName="password"
              [toggleMask]="true"
              styleClass="w-full"
              inputStyleClass="w-full"
              [fluid]="true"
              autocomplete="new-password"
            />
            <label for="reg-password">Password</label>
          </p-floatlabel>
          @if (localError()) {
            <p-message severity="error" [text]="localError()!" />
          }
          <p-button
            type="submit"
            label="Register"
            icon="pi pi-user-plus"
            [disabled]="form.invalid || pending()"
            [loading]="pending()"
            styleClass="w-full"
          />
        </form>
        <p class="text-sm text-color-secondary mt-3 mb-0">
          Already have an account?
          <a routerLink="/login" class="text-primary font-medium">Sign in</a>
        </p>
      </p-card>
    </div>
  `,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  readonly pending = signal(false);
  readonly localError = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) return;
    this.localError.set(null);
    this.errors.clear();
    this.pending.set(true);
    const v = this.form.getRawValue();
    this.auth.register(v).subscribe({
      next: () => {
        this.pending.set(false);
        void this.router.navigateByUrl('/');
      },
      error: () => {
        this.pending.set(false);
        this.localError.set('Registration failed. Email may already be in use.');
      },
    });
  }
}
