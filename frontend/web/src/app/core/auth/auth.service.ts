import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, UserDto } from '../models/auth.models';

const STORAGE_ACCESS = 'elearning_access';
const STORAGE_REFRESH = 'elearning_refresh';
const STORAGE_USER = 'elearning_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiV1 = `${environment.apiUrl}/api/v1`;

  readonly user = signal<UserDto | null>(null);

  readonly isAuthenticated = computed(() => this.user() !== null);

  constructor() {
    this.hydrateFromStorage();
  }

  accessToken(): string | null {
    return sessionStorage.getItem(STORAGE_ACCESS);
  }

  hydrateFromStorage(): void {
    const token = sessionStorage.getItem(STORAGE_ACCESS);
    const raw = sessionStorage.getItem(STORAGE_USER);
    if (token && raw) {
      try {
        this.user.set(JSON.parse(raw) as UserDto);
      } catch {
        this.clearStorage();
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.apiV1}/identity/login`, { email, password })
      .pipe(tap((res) => this.persistAuth(res)));
  }

  register(body: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.apiV1}/identity/register`, body)
      .pipe(tap((res) => this.persistAuth(res)));
  }

  refreshMe(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiV1}/identity/me`).pipe(
      tap((u) => {
        sessionStorage.setItem(STORAGE_USER, JSON.stringify(u));
        this.user.set(u);
      }),
    );
  }

  updateProfile(firstName: string, lastName: string): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiV1}/identity/me`, { firstName, lastName }).pipe(
      tap((u) => {
        sessionStorage.setItem(STORAGE_USER, JSON.stringify(u));
        this.user.set(u);
      }),
    );
  }

  logout(): void {
    this.clearStorage();
    void this.router.navigate(['/login']);
  }

  persistAuth(res: AuthResponseDto): void {
    sessionStorage.setItem(STORAGE_ACCESS, res.accessToken);
    sessionStorage.setItem(STORAGE_REFRESH, res.refreshToken);
    sessionStorage.setItem(STORAGE_USER, JSON.stringify(res.user));
    this.user.set(res.user);
  }

  private clearStorage(): void {
    sessionStorage.removeItem(STORAGE_ACCESS);
    sessionStorage.removeItem(STORAGE_REFRESH);
    sessionStorage.removeItem(STORAGE_USER);
    this.user.set(null);
  }
}
