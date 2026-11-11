using ModelContextProtocol.Server;

using System.ComponentModel;

namespace VetClinic.McpServer.Resources;

[McpServerResourceType]
public class ClinicResources
{
    [McpServerResource(UriTemplate = "clinic://{clinicId}/hours",
                       Name = "Clinic opening hours",
                       MimeType = "text/plain")]
    [Description("Opening hours for a clinic.")]
    public static string GetHours(int clinicId) =>
        clinicId == 12
            ? "Mon-Fri 09:00-20:00, Sat 10:00-14:00, Sun closed"
            : "Unknown clinic";

    [McpServerResource(UriTemplate = "clinic://services",
                       Name = "Services offered",
                       MimeType = "text/plain")]
    [Description("Services every clinic in the network offers.")]
    public static string GetServices() =>
        "Vaccinations, check-ups, surgery, dental care, microchipping";
}
