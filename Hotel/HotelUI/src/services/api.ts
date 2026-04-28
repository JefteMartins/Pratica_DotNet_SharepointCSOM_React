import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7233/api',
});

export const hotelService = {
  getHotels: () => api.get('/hotels'),
  getAllRooms: () => api.get('/rooms'),
  getRooms: (hotelId: number) => api.get(`/hotels/${hotelId}/rooms`),
  updateRoomStatus: (roomId: number, status: string) => api.patch(`/rooms/${roomId}/status`, status, {
    headers: { 'Content-Type': 'application/json' }
  }),
  createBooking: (booking: any) => api.post('/bookings', booking),
  getBookings: () => api.get('/bookings'),
  getDashboardStats: () => api.get('/dashboard/stats'),
};

export const labService = {
  getTasks: () => api.get('/lab/tasks'),
  seedData: (count: number) => api.post(`/lab/seed?count=${count}`),
  getPaged: (pageSize: number, pos?: string) => 
    api.get(`/lab/paged?pageSize=${pageSize}${pos ? `&pos=${encodeURIComponent(pos)}` : ''}`),
  getStream: (pageSize: number) => 
    api.get(`/lab/stream?pageSize=${pageSize}`),
  createSequential: (count: number) => 
    api.post(`/lab/write/sequential?count=${count}`),
  createBatched: (count: number, batchSize: number = 50) => 
    api.post(`/lab/write/batched?count=${count}&batchSize=${batchSize}`),
  toggleStress: (enabled: boolean) => 
    api.post(`/lab/resilience/stress-toggle?enabled=${enabled}`),
  createResilient: (title: string) => 
    api.post(`/lab/resilience/create?title=${encodeURIComponent(title)}`),
  searchTasks: (filters: any) => 
    api.post('/lab/search', filters),
  deleteSequential: (count: number) => 
    api.delete(`/lab/write/sequential?count=${count}`),
  deleteBatched: (count: number, batchSize: number = 50) => 
    api.delete(`/lab/write/batched?count=${count}&batchSize=${batchSize}`),
  deleteByFilter: (filters: any) => 
    api.post('/lab/delete-by-filter', filters),
  updateTask: (task: any) => 
    api.put('/lab/task', task),
};

export default api;
