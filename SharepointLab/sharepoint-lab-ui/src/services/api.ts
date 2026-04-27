import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5118/api',
});

export const sharePointApi = {
  getTasks: () => api.get('/tasks'),
  seedData: (count: number) => api.post(`/tasks/seed?count=${count}`),
  getPaged: (pageSize: number, pos?: string) => 
    api.get(`/tasks/paged?pageSize=${pageSize}${pos ? `&pos=${encodeURIComponent(pos)}` : ''}`),
  getStream: (pageSize: number) => 
    api.get(`/tasks/stream?pageSize=${pageSize}`),
  createSequential: (count: number) => 
    api.post(`/tasks/write/sequential?count=${count}`),
  createBatched: (count: number, batchSize: number = 50) => 
    api.post(`/tasks/write/batched?count=${count}&batchSize=${batchSize}`),
  toggleStress: (enabled: boolean) => 
    api.post(`/tasks/resilience/stress-toggle?enabled=${enabled}`),
  createResilient: (title: string) => 
    api.post(`/tasks/resilience/create?title=${encodeURIComponent(title)}`),
  searchTasks: (filters: any) => 
    api.post('/tasks/search', filters),
  deleteSequential: (count: number) => 
    api.delete(`/tasks/write/sequential?count=${count}`),
  deleteBatched: (count: number, batchSize: number = 50) => 
    api.delete(`/tasks/write/batched?count=${count}&batchSize=${batchSize}`),
  deleteByFilter: (filters: any) => 
    api.post('/tasks/delete-by-filter', filters),
  updateTask: (task: any) => 
    api.put('/tasks', task),
};

export default api;
