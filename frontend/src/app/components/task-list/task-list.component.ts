import { CommonModule, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, inject } from '@angular/core';
import { Task } from '../../models/task.model';
import { TaskService } from '../../services/task.service';

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
      error: () => {
        this.loading = false;
        this.errorMessage = 'Nao foi possivel carregar as tarefas. Confirme se a API esta em execucao.';
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
      error: () => {
        this.errorMessage = 'Nao foi possivel excluir a tarefa.';
      }
    });
  }
}
