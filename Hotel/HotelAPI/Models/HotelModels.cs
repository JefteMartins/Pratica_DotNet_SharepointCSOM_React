namespace HotelAPI.Models;

public record HotelModel(int Id, string Name, string Location, int Stars, string Description, string ImageUrl);
public record RoomModel(int Id, string Title, string RoomType, decimal PricePerNight, int HotelId, string Status);
public record BookingModel(int Id, string BookingCode, int RoomId, string GuestName, DateTime CheckIn, DateTime CheckOut, decimal TotalAmount, string Status);
