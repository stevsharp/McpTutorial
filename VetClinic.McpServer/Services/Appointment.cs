namespace VetClinic.McpServer.Services;

public record Appointment(int Id, int ClinicId, string PetName, string Species,
                          string OwnerName, DateTime StartsAt, string Reason);
