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

export default api;
