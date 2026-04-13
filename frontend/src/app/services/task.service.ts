import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Task, TaskRequest } from '../models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5063/api/tasks';

  getTasks(status?: string): Observable<Task[]> {
    let params = new HttpParams();

    if (status && status !== 'Todos') {
      params = params.set('status', status);
    }

    return this.http.get<Task[]>(this.apiUrl, { params });
  }

  createTask(payload: TaskRequest): Observable<Task> {
    return this.http.post<Task>(this.apiUrl, payload);
  }

  updateTask(id: number, payload: TaskRequest): Observable<Task> {
    return this.http.put<Task>(`${this.apiUrl}/${id}`, payload);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
