import { CommonModule, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, inject } from '@angular/core';
import { Task } from '../../models/task.model';
import { TaskService } from '../../services/task.service';
import { extractApiErrorMessage } from '../../utils/api-error.util';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent implements OnInit, OnChanges {
  @Input() refreshToken = 0;
  @Output() editRequested = new EventEmitter<Task>();
  @Output() deleted = new EventEmitter<number>();

  private readonly taskService = inject(TaskService);

  tasks: Task[] = [];
  loading = false;
  errorMessage = '';
  selectedStatus = 'Todos';

  ngOnInit(): void {
    this.loadTasks();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['refreshToken'] && !changes['refreshToken'].firstChange) {
      this.loadTasks();
    }
  }

  loadTasks(): void {
    this.loading = true;
    this.errorMessage = '';

    this.taskService.getTasks(this.selectedStatus).subscribe({
      next: (tasks) => {
        this.tasks = tasks;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.loading = false;
        this.errorMessage = extractApiErrorMessage(error, 'Nao foi possivel carregar as tarefas.');
      }
    });
  }

  filterByStatus(status: string): void {
    this.selectedStatus = status;
    this.loadTasks();
  }

  remove(task: Task): void {
    if (!confirm(`Deseja realmente excluir a tarefa "${task.titulo}"?`)) {
      return;
    }

    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        this.deleted.emit(task.id);
        this.loadTasks();
      },
      error: (error: unknown) => {
        this.errorMessage = extractApiErrorMessage(error, 'Nao foi possivel excluir a tarefa.');
      }
    });
  }
}
