import { Component } from '@angular/core';
import { TaskFormComponent } from './components/task-form/task-form.component';
import { TaskListComponent } from './components/task-list/task-list.component';
import { Task } from './models/task.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [TaskFormComponent, TaskListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  selectedTask: Task | null = null;
  refreshToken = 0;

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
}
