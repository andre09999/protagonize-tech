import { DOCUMENT } from '@angular/common';
import { Injectable, computed, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const themeStorageKey = 'task-manager.theme';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly themeState = signal<ThemeMode>(this.getInitialTheme());

  readonly theme = this.themeState.asReadonly();
  readonly isDarkMode = computed(() => this.themeState() === 'dark');

  constructor() {
    this.applyTheme(this.themeState());
  }

  toggleTheme(): void {
    const nextTheme = this.themeState() === 'dark' ? 'light' : 'dark';
    this.themeState.set(nextTheme);
    this.applyTheme(nextTheme);
  }

  private applyTheme(theme: ThemeMode): void {
    this.document.documentElement.dataset['theme'] = theme;
    this.document.documentElement.style.colorScheme = theme;
    localStorage.setItem(themeStorageKey, theme);
  }

  private getInitialTheme(): ThemeMode {
    const storedTheme = localStorage.getItem(themeStorageKey);

    if (storedTheme === 'light' || storedTheme === 'dark') {
      return storedTheme;
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
