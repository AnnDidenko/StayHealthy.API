namespace StayHealthy.Client.Models.Availability;

public class Facility
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}