import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DialogModule,
    InputText,
    Button,
    PrimeTemplate,
  ],
  styleUrl: './profile.component.scss',
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  private readonly errors = inject(GlobalErrorService);

  visible = true;

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
  });

  readonly pending = signal(false);

  ngOnInit(): void {
    this.errors.clear();
    this.pending.set(true);
    this.auth.refreshMe().subscribe({
      next: (u) => {
        this.form.patchValue({ firstName: u.firstName, lastName: u.lastName });
        this.pending.set(false);
      },
      error: () => this.pending.set(false),
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.errors.clear();
    this.pending.set(true);
    const { firstName, lastName } = this.form.getRawValue();
    this.auth.updateProfile(firstName, lastName).subscribe({
      next: () => {
        this.form.markAsPristine();
        this.pending.set(false);
      },
      error: () => this.pending.set(false),
    });
  }

  close(): void {
    void this.router.navigate(['/dashboard'], { replaceUrl: true });
  }
}
