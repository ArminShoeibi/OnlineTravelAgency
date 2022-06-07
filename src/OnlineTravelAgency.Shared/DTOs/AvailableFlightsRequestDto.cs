namespace OnlineTravelAgency.Shared.DTOs;

public record class AvailableFlightsRequestDto
{
    public string Origin { get; init; }
    public string Destination { get; init; }
    public DateTimeOffset DepartureDate { get; init; }
    public byte NumberOfAdults { get; init; }
    public byte NumberOfChildren { get; init; }
    public byte NumberOfInfant { get; init; }
}
