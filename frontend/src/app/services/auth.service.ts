import { Injectable, computed, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

const sessionStorageKey = 'task-manager.session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5063/api/auth';
  private readonly sessionState = signal<AuthResponse | null>(this.readStoredSession());

  readonly session = this.sessionState.asReadonly();
  readonly currentUser = computed(() => this.sessionState()?.user ?? null);
  readonly isAuthenticated = computed(() => this.sessionState() !== null);

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, payload).pipe(
      tap((session) => this.setSession(session))
    );
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, payload).pipe(
      tap((session) => this.setSession(session))
    );
  }

  logout(): void {
    localStorage.removeItem(sessionStorageKey);
    this.sessionState.set(null);
  }

  getAccessToken(): string | null {
    return this.sessionState()?.accessToken ?? null;
  }

  private setSession(session: AuthResponse): void {
    localStorage.setItem(sessionStorageKey, JSON.stringify(session));
    this.sessionState.set(session);
  }

  private readStoredSession(): AuthResponse | null {
    const rawSession = localStorage.getItem(sessionStorageKey);

    if (!rawSession) {
      return null;
    }

    try {
      const session = JSON.parse(rawSession) as AuthResponse;
      const expiresAt = new Date(session.expiresAtUtc);

      if (Number.isNaN(expiresAt.getTime()) || expiresAt <= new Date()) {
        localStorage.removeItem(sessionStorageKey);
        return null;
      }

      return session;
    } catch {
      localStorage.removeItem(sessionStorageKey);
      return null;
    }
  }
}
