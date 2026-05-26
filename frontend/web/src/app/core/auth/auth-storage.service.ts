import { Injectable } from '@angular/core';
import { UserDto } from '../models/auth.models';

const STORAGE_ACCESS = 'elearning_access';
const STORAGE_REFRESH = 'elearning_refresh';
const STORAGE_USER = 'elearning_user';

@Injectable({ providedIn: 'root' })
export class AuthStorageService {
  accessToken(): string | null {
    return sessionStorage.getItem(STORAGE_ACCESS);
  }

  user(): UserDto | null {
    const raw = sessionStorage.getItem(STORAGE_USER);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as UserDto;
    } catch {
      this.clear();
      return null;
    }
  }

  setAccessToken(token: string): void {
    sessionStorage.setItem(STORAGE_ACCESS, token);
  }

  setRefreshToken(token: string): void {
    sessionStorage.setItem(STORAGE_REFRESH, token);
  }

  setUser(user: UserDto): void {
    sessionStorage.setItem(STORAGE_USER, JSON.stringify(user));
  }

  clear(): void {
    sessionStorage.removeItem(STORAGE_ACCESS);
    sessionStorage.removeItem(STORAGE_REFRESH);
    sessionStorage.removeItem(STORAGE_USER);
  }
}
