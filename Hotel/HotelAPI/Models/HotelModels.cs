namespace HotelAPI.Models;

public class HotelModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class RoomModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int HotelId { get; set; }
    public string Status { get; set; } = "Available";
    public int HotelStars { get; set; }
}

public class BookingModel
{
    public int Id { get; set; }
    public string? BookingCode { get; set; }
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public string? HotelName { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Confirmed";
}
