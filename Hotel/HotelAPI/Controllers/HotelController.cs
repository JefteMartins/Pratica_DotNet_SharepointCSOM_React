using Microsoft.AspNetCore.Mvc;
using HotelAPI.Services;
using HotelAPI.Models;

namespace HotelAPI.Controllers;

[ApiController]
[Route("api")]
public class HotelController : ControllerBase
{
    private readonly ISharePointService _sharePointService;

    public HotelController(ISharePointService sharePointService)
    {
        _sharePointService = sharePointService;
    }

    [HttpGet("hotels")]
    public async Task<IActionResult> GetHotels()
    {
        var hotels = await _sharePointService.GetHotelsAsync();
        return Ok(hotels);
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _sharePointService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("hotels/{id}/rooms")]
    public async Task<IActionResult> GetRooms(int id)
    {
        var rooms = await _sharePointService.GetRoomsByHotelAsync(id);
        return Ok(rooms);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings()
    {
        var bookings = await _sharePointService.GetBookingsAsync();
        return Ok(bookings);
    }

    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] BookingModel booking)
    {
        try
        {
            var result = await _sharePointService.CreateBookingAsync(booking);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro interno ao processar a reserva.", Detail = ex.Message });
        }
    }

    [HttpPatch("rooms/{id}/status")]
    public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] string status)
    {
        var result = await _sharePointService.UpdateRoomStatusAsync(id, status);
        return Ok(result);
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _sharePointService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
