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
import { Roles } from '../../core/auth/roles';

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
  templateUrl: './login.component.html',
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

  returnUrl(): string | null {
    return this.route.snapshot.queryParamMap.get('returnUrl');
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
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const userRoles = this.auth.user()?.roles ?? [];
        const defaultRoute = userRoles.includes(Roles.Admin)
          ? '/admin'
          : userRoles.includes(Roles.Instructor)
            ? '/teach'
            : '/learn';
        void this.router.navigateByUrl(returnUrl || defaultRoute);
      },
      error: () => {
        this.pending.set(false);
        this.localError.set('Invalid email or password.');
      },
    });
  }
}
