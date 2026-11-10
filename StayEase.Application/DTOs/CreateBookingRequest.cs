namespace StayEase.Application.DTOs;

public record CreateBookingRequest(
    int RoomId,
    string CustomerId,
    DateOnly CheckIn,
    DateOnly CheckOut);
