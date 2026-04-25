import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5118/api',
});

export const sharePointApi = {
  getTasks: () => api.get('/tasks'),
  seedData: (count: number) => api.post(`/tasks/seed?count=${count}`),
};

export default api;
