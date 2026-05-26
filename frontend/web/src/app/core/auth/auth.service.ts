import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, UserDto } from '../models/auth.models';
import { AuthStorageService } from './auth-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(AuthStorageService);
  private readonly apiV1 = `${environment.apiUrl}/api/v1`;

  readonly user = signal<UserDto | null>(null);

  readonly isAuthenticated = computed(() => this.user() !== null);

  constructor() {
    this.hydrateFromStorage();
  }

  accessToken(): string | null {
    return this.storage.accessToken();
  }

  hydrateFromStorage(): void {
    if (this.storage.accessToken()) this.user.set(this.storage.user());
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
        this.storage.setUser(u);
        this.user.set(u);
      }),
    );
  }

  updateProfile(firstName: string, lastName: string): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiV1}/identity/me`, { firstName, lastName }).pipe(
      tap((u) => {
        this.storage.setUser(u);
        this.user.set(u);
      }),
    );
  }

  logout(): void {
    this.clearStorage();
    void this.router.navigate(['/login']);
  }

  persistAuth(res: AuthResponseDto): void {
    this.storage.setAccessToken(res.accessToken);
    this.storage.setRefreshToken(res.refreshToken);
    this.storage.setUser(res.user);
    this.user.set(res.user);
  }

  private clearStorage(): void {
    this.storage.clear();
    this.user.set(null);
  }
}
