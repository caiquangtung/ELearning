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
  templateUrl: './register.component.html',
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
