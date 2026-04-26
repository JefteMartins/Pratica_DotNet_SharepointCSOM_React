using HotelAPI.Models;

namespace HotelAPI.Services;

public interface ISharePointService
{
    // Hotels
    Task<List<HotelModel>> GetHotelsAsync();
    
    // Rooms
    Task<List<RoomModel>> GetRoomsByHotelAsync(int hotelId);
    Task<bool> UpdateRoomStatusAsync(int roomId, string newStatus);
    
    // Bookings
    Task<BookingModel> CreateBookingAsync(BookingModel booking);
    Task<List<BookingModel>> GetBookingsAsync();
    
    // Dashboard
    Task<object> GetDashboardStatsAsync();
}
