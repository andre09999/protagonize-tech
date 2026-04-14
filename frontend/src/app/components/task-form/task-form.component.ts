import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskRequest } from '../../models/task.model';
import { TaskService } from '../../services/task.service';
import { extractApiErrorMessage } from '../../utils/api-error.util';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css'
})
export class TaskFormComponent implements OnChanges {
  @Input() selectedTask: Task | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly taskService = inject(TaskService);

  feedbackMessage = '';
  feedbackType: 'success' | 'error' | '' = '';
  submitting = false;

  readonly form = this.formBuilder.nonNullable.group({
    titulo: ['', [Validators.required, Validators.maxLength(150)]],
    descricao: ['', [Validators.required, Validators.maxLength(500)]],
    status: ['Pendente', [Validators.required]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedTask']) {
      this.syncFormWithSelectedTask();
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.setFeedback('error', 'Preencha os campos obrigatorios antes de salvar.');
      return;
    }

    this.submitting = true;
    this.feedbackMessage = '';

    const payload: TaskRequest = this.form.getRawValue();
    const request$ = this.selectedTask
      ? this.taskService.updateTask(this.selectedTask.id, payload)
      : this.taskService.createTask(payload);

    request$.subscribe({
      next: () => {
        this.submitting = false;
        this.setFeedback('success', this.selectedTask ? 'Tarefa atualizada com sucesso.' : 'Tarefa criada com sucesso.');
        this.form.reset({ titulo: '', descricao: '', status: 'Pendente' });
        this.saved.emit();
      },
      error: (error: unknown) => {
        this.submitting = false;
        this.setFeedback('error', extractApiErrorMessage(error, 'Nao foi possivel salvar a tarefa.'));
      }
    });
  }

  clearSelection(): void {
    this.form.reset({ titulo: '', descricao: '', status: 'Pendente' });
    this.feedbackMessage = '';
    this.feedbackType = '';
    this.cancelled.emit();
  }

  private syncFormWithSelectedTask(): void {
    if (!this.selectedTask) {
      this.form.reset({ titulo: '', descricao: '', status: 'Pendente' });
      return;
    }

    this.form.reset({
      titulo: this.selectedTask.titulo,
      descricao: this.selectedTask.descricao,
      status: this.selectedTask.status
    });
  }

  private setFeedback(type: 'success' | 'error', message: string): void {
    this.feedbackType = type;
    this.feedbackMessage = message;
  }
}
