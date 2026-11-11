using ModelContextProtocol.Server;

using System.ComponentModel;

using VetClinic.McpServer.Services;

namespace VetClinic.McpServer.Tools;

[McpServerToolType]
public class AppointmentTools(IAppointmentService service)
{
    [McpServerTool(ReadOnly = true)]
    [Description("Lists upcoming appointments for a clinic.")]
    public IReadOnlyList<Appointment> GetUpcomingAppointments(
        [Description("The clinic ID")] int clinicId,
        [Description("How many days ahead to look, 1 to 30")] int days = 7)
        => service.GetUpcoming(clinicId, Math.Clamp(days, 1, 30));

    [McpServerTool]
    [Description("Books a new appointment and returns it.")]
    public Appointment BookAppointment(
        int clinicId,
        string petName,
        [Description("Species, e.g. Dog, Cat, Rabbit")] string species,
        string ownerName,
        [Description("Start time in ISO 8601, e.g. 2026-11-12T10:30")] DateTime startsAt,
        string reason)
        => service.Book(clinicId, petName, species, ownerName, startsAt, reason);

    [McpServerTool(Destructive = true)]
    [Description("Cancels an appointment by ID. Returns true if it existed.")]
    public bool CancelAppointment(int appointmentId) => service.Cancel(appointmentId);
}
