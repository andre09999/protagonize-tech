export interface Task {
  id: number;
  titulo: string;
  descricao: string;
  status: string;
  dataCriacao: string;
}

export interface TaskRequest {
  titulo: string;
  descricao: string;
  status: string;
}
