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
};

export default api;
