namespace OnlineTravelAgency.Shared.DTOs;

public record class ReserveFlightDto
{
    public string FlightId { get; init; }
    public string ProviderId { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public List<Passenger> Passengers { get; init; }
}
public record class Passenger
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly BirthDate { get; init; }
    public string NationalId { get; init; } // conver this to a ValueObjet
    public string PassportId { get; init; } // conver this to a ValueObjet
}
