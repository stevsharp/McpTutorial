namespace VetClinic.McpServer.Services;

public class InMemoryAppointmentService : IAppointmentService
{
    private readonly Lock _lock = new();

    private readonly List<Appointment> _appointments = new()
    {
        new Appointment(1, 1, "Fido", "Dog", "John Doe", DateTime.Now.AddDays(1), "Checkup"),

        new Appointment(2, 1, "Whiskers", "Cat", "Jane Smith", DateTime.Now.AddDays(2), "Vaccination"),

        new(1, 12, "Luna", "Cat", "Maria P.", DateTime.Today.AddHours(10), "Vaccination"),
        new(2, 12, "Rex",  "Dog", "Nikos K.", DateTime.Today.AddDays(1).AddHours(12), "Limping")
    };

    public IReadOnlyList<Appointment> GetUpcoming(int clinicId, int days)
    {
        lock (_lock) { 
            return [.. _appointments
                .Where(a => a.ClinicId == clinicId && a.StartsAt <= DateTime.Now.AddDays(days))
                .OrderBy(a => a.StartsAt)];
        }
    }

    public Appointment Book(int clinicId, string petName, string species,
                            string ownerName, DateTime startsAt, string reason)
    {
        lock (_lock) { 
            var id = _appointments.Max(a => a.Id) + 1;
            var appointment = new Appointment(id, clinicId, petName, species, ownerName, startsAt, reason);
            _appointments.Add(appointment);
            return appointment;
        }
    }
    public bool Cancel(int appointmentId)
    {
        lock (_lock) {
            var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment == null) return false;
            _appointments.Remove(appointment);
            return true;
        }
    }
}