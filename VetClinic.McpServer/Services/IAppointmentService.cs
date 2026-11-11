namespace VetClinic.McpServer.Services;

public interface IAppointmentService
{
    IReadOnlyList<Appointment> GetUpcoming(int clinicId, int days);
    Appointment Book(int clinicId, string petName, string species,
                     string ownerName, DateTime startsAt, string reason);
    bool Cancel(int appointmentId);
}
