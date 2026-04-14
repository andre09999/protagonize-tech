import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { extractApiErrorMessage } from '../../utils/api-error.util';

type AuthMode = 'login' | 'register';

@Component({
  selector: 'app-auth-panel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth-panel.component.html',
  styleUrl: './auth-panel.component.css'
})
export class AuthPanelComponent {
  @Output() authenticated = new EventEmitter<void>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  mode: AuthMode = 'login';
  submitting = false;
  feedbackMessage = '';
  feedbackType: 'success' | 'error' | '' = '';

  readonly form = this.formBuilder.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
    confirmPassword: ['']
  });

  switchMode(mode: AuthMode): void {
    if (this.mode === mode) {
      return;
    }

    this.mode = mode;
    this.feedbackMessage = '';
    this.feedbackType = '';
    this.form.controls.confirmPassword.reset('');
  }

  submit(): void {
    if (this.form.invalid || this.hasInvalidConfirmation()) {
      this.form.markAllAsTouched();
      this.setFeedback('error', 'Revise os campos e tente novamente.');
      return;
    }

    const { username, password } = this.form.getRawValue();
    const request$ = this.mode === 'login'
      ? this.authService.login({ username, password })
      : this.authService.register({ username, password });

    this.submitting = true;
    this.feedbackMessage = '';

    request$.subscribe({
      next: () => {
        this.submitting = false;
        this.setFeedback(
          'success',
          this.mode === 'login'
            ? 'Login realizado com sucesso.'
            : 'Conta criada com sucesso.'
        );
        this.form.reset({ username: '', password: '', confirmPassword: '' });
        this.authenticated.emit();
      },
      error: (error: unknown) => {
        this.submitting = false;
        this.setFeedback(
          'error',
          extractApiErrorMessage(
            error,
            this.mode === 'login'
              ? 'Nao foi possivel autenticar agora.'
              : 'Nao foi possivel criar a conta agora.'
          )
        );
      }
    });
  }

  hasInvalidConfirmation(): boolean {
    if (this.mode !== 'register') {
      return false;
    }

    const { password, confirmPassword } = this.form.getRawValue();
    return !confirmPassword || password !== confirmPassword;
  }

  private setFeedback(type: 'success' | 'error', message: string): void {
    this.feedbackType = type;
    this.feedbackMessage = message;
  }
}
