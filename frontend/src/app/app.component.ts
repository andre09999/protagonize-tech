import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { AuthPanelComponent } from './components/auth-panel/auth-panel.component';
import { TaskFormComponent } from './components/task-form/task-form.component';
import { TaskListComponent } from './components/task-list/task-list.component';
import { Task } from './models/task.model';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, AuthPanelComponent, TaskFormComponent, TaskListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);

  selectedTask: Task | null = null;
  refreshToken = 0;

  readonly currentUser = this.authService.currentUser;
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly currentThemeLabel = computed(() => this.themeService.isDarkMode() ? 'Modo claro' : 'Modo escuro');

  handleTaskSaved(): void {
    this.selectedTask = null;
    this.refreshToken++;
  }

  handleEdit(task: Task): void {
    this.selectedTask = task;
  }

  handleCancel(): void {
    this.selectedTask = null;
  }

  handleDeleted(selectedTaskId: number): void {
    if (this.selectedTask?.id === selectedTaskId) {
      this.selectedTask = null;
    }

    this.refreshToken++;
  }

  handleAuthenticated(): void {
    this.selectedTask = null;
    this.refreshToken++;
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
    this.selectedTask = null;
  }
}
