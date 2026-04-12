import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, Card, FloatLabel, InputText, Button],
  template: `
    <p-card header="Profile">
      <form [formGroup]="form" (ngSubmit)="save()" class="flex flex-column gap-3" style="max-width: 28rem">
        <p-floatlabel>
          <input pInputText id="pf-fn" formControlName="firstName" class="w-full" fluid />
          <label for="pf-fn">First name</label>
        </p-floatlabel>
        <p-floatlabel>
          <input pInputText id="pf-ln" formControlName="lastName" class="w-full" fluid />
          <label for="pf-ln">Last name</label>
        </p-floatlabel>
        <p class="text-sm text-color-secondary">Email: {{ auth.user()?.email }} (read-only)</p>
        <p-button
          type="submit"
          label="Save"
          icon="pi pi-check"
          [disabled]="form.invalid || form.pristine || pending()"
          [loading]="pending()"
        />
      </form>
    </p-card>
  `,
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);
  private readonly errors = inject(GlobalErrorService);

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
}
